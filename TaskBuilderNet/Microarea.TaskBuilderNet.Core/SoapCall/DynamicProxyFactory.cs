namespace Microarea.TaskBuilderNet.Core.SoapCall
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.Design;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Services.Discovery;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Microarea.TaskBuilderNet.Core.Generic;
    using Microarea.TaskBuilderNet.Core.NameSolver;
    using Microarea.TaskBuilderNet.Core.SerializableTypes;
    using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
    using WsdlNS = System.Web.Services.Description;
    using Interfaces;

    //================================================================================
    public class DynamicProxyFactory
	{
		private string descriptionUri;
		private string assemblyFilename;
		private Binding mexBinding = null;
		private DynamicProxyFactoryOptions options;

		private IEnumerable<Binding> bindings;
		private IEnumerable<ContractDescription> contracts;
		private IEnumerable<ServiceEndpoint> endpoints;

		private CodeCompileUnit codeCompileUnit;
		private CodeDomProvider codeDomProvider;
		private ServiceContractGenerator contractGenerator;

		private Collection<MetadataSection> metadataCollection;
		private IEnumerable<MetadataConversionError> importErrors;
		private IEnumerable<MetadataConversionError> codegenErrors;
		private IEnumerable<CompilerError> compilerErrors;

		private Assembly proxyAssembly;
		private string proxyCode;
		private const string endPointResourceName = "endpoint.res";
		private static string dllCachePath = null;
		
		//--------------------------------------------------------------------------------
		public IEnumerable<MetadataConversionError> ImportErrors { get { return importErrors; } }
		//--------------------------------------------------------------------------------
		public IEnumerable<MetadataConversionError> CodeGenerationErrors { get { return codegenErrors; } }
		//--------------------------------------------------------------------------------
		public IEnumerable<CompilerError> CompilerErrors { get { return compilerErrors; } }
		
		//--------------------------------------------------------------------------------
		static DynamicProxyFactory ()
		{
			dllCachePath = BasePathFinder.BasePathFinderInstance.GetAppDataPath(true);
		}
		
		//--------------------------------------------------------------------------------
		public DynamicProxyFactory (Binding mexBinding, string descriptionUri, bool forceAssemblyCreation, DynamicProxyFactoryOptions options)
		{
			if (descriptionUri == null)
				throw new ArgumentNullException("descriptionUri");

			if (options == null)
				throw new ArgumentNullException("options");

			this.descriptionUri = descriptionUri;
			this.options = options;
			this.mexBinding = mexBinding;
			CalculateAssemblyFileName();
			bool generateDebugInfo = false; 
#if DEBUG
			generateDebugInfo = !forceAssemblyCreation;
#endif
			string asmPath = GetAssemblyPath();
			//se e' valido l'assembly locale, lo carico, altrimenti lo rigenero
			if (!forceAssemblyCreation && IsValid(asmPath))
			{
				if (!LoadAssembly(asmPath))
					GenerateAssembly(generateDebugInfo);
			}
			else
			{
				GenerateAssembly(generateDebugInfo);
			}
		}

		//--------------------------------------------------------------------------------
		private void GenerateAssembly (bool generateDebuginfo)
		{
			DownloadMetadata();
			ImportMetadata();
			CreateProxy();
			WriteCode();
			CompileProxy(generateDebuginfo);
		}

		//--------------------------------------------------------------------------------
		private bool LoadAssembly (string asmPath)
		{
			try
			{
				byte[] buffer = null;
				using (FileStream fs = new FileStream(asmPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					buffer = new byte[fs.Length];
					fs.Read(buffer, 0, buffer.Length);
				}
				proxyAssembly = Assembly.Load(buffer);
				LoadServiceProperties();
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool IsValid (string asmPath)
		{
			try
			{
				FileInfo asmFileInfo = new FileInfo(asmPath);

				//Se non esiste allora deve essere generato
				if (!asmFileInfo.Exists)
					return false;

				//Se e` stato generato prima della data in cui e` stata invalidata la cache allora deve essere generato
				if (asmFileInfo.LastWriteTime < InstallationData.CacheDate)
					return false;

				AssemblyName asm = AssemblyName.GetAssemblyName(asmPath);
				//la versione dell'assembly deve essere la stessa di quello che l'ha generata
				return asm.Version == Assembly.GetExecutingAssembly().GetName().Version;
			}
			catch
			{
				return false;
			}
		}

		//--------------------------------------------------------------------------------
		private void LoadServiceProperties ()
		{
			List<ServiceEndpoint> newEndpoints = new List<ServiceEndpoint>();
			Stream ss = this.proxyAssembly.GetManifestResourceStream(endPointResourceName);
			XmlSerializer bf = new XmlSerializer(typeof(SerializableEndPointCollection));
			SerializableEndPointCollection endpoints = bf.Deserialize(ss) as SerializableEndPointCollection;
			foreach (SerializableEndpoint sep in endpoints)
				newEndpoints.Add(sep.GetEndPoint());
			this.endpoints = newEndpoints;
		}

		//--------------------------------------------------------------------------------
		public DynamicProxyFactory (Binding mexBinding, string descriptionUri, bool forceAssemblyGeneration)
			: this(mexBinding, descriptionUri, forceAssemblyGeneration, new DynamicProxyFactoryOptions())
		{
		}

		//--------------------------------------------------------------------------------
		private void DownloadMetadata ()
		{
			if (mexBinding != null)
			{
				MetadataExchangeClient mexClient = new MetadataExchangeClient(mexBinding);
				mexClient.ResolveMetadataReferences = true;
				mexClient.OperationTimeout = TimeSpan.FromHours(1);

				MetadataSet metadataSet = mexClient.GetMetadata(new EndpointAddress(descriptionUri));

				this.metadataCollection = metadataSet.MetadataSections;

			}
			else
			{

				using (WebClient client = new WebClient())
				{
					client.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
					using (DiscoveryClientProtocol disco = new DiscoveryClientProtocol())
					{
						disco.AllowAutoRedirect = true;
						disco.Proxy = client.Proxy;
						disco.UseDefaultCredentials = true;
						disco.DiscoverAny(descriptionUri);
						disco.ResolveAll();

						Collection<MetadataSection> results = new Collection<MetadataSection>();
						foreach (object document in disco.Documents.Values)
							AddDocumentToResults(document, results);
						this.metadataCollection = results;
					}

				}
			}
		}

		//--------------------------------------------------------------------------------
		void AddDocumentToResults (object document, Collection<MetadataSection> results)
		{
			WsdlNS.ServiceDescription wsdl = document as WsdlNS.ServiceDescription;
			XmlSchema schema = document as XmlSchema;
			XmlElement xmlDoc = document as XmlElement;

			if (wsdl != null)
			{
				results.Add(MetadataSection.CreateFromServiceDescription(wsdl));
			}
			else if (schema != null)
			{
				results.Add(MetadataSection.CreateFromSchema(schema));
			}
			else if (xmlDoc != null && xmlDoc.LocalName == "Policy")
			{
				results.Add(MetadataSection.CreateFromPolicy(xmlDoc, null));
			}
			else
			{
				MetadataSection mexDoc = new MetadataSection();
				mexDoc.Metadata = document;
				results.Add(mexDoc);
			}
		}


		//--------------------------------------------------------------------------------
		private void ImportMetadata ()
		{
			this.codeCompileUnit = new CodeCompileUnit();
			CreateCodeDomProvider();

			WsdlImporter importer = new WsdlImporter(new MetadataSet(metadataCollection));
			AddStateForDataContractSerializerImport(importer);
			AddStateForXmlSerializerImport(importer);

			this.bindings = importer.ImportAllBindings();
			this.contracts = importer.ImportAllContracts();

			this.endpoints = importer.ImportAllEndpoints();

			this.importErrors = importer.Errors;
		}

		//--------------------------------------------------------------------------------
		void AddStateForXmlSerializerImport (WsdlImporter importer)
		{
			XmlSerializerImportOptions importOptions =
				new XmlSerializerImportOptions(this.codeCompileUnit);
			importOptions.CodeProvider = this.codeDomProvider;

			importOptions.WebReferenceOptions = new WsdlNS.WebReferenceOptions();
			importOptions.WebReferenceOptions.CodeGenerationOptions =
				CodeGenerationOptions.GenerateProperties |
				CodeGenerationOptions.GenerateOrder;

			importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(
				typeof(TypedDataSetSchemaImporterExtension).AssemblyQualifiedName);
			importOptions.WebReferenceOptions.SchemaImporterExtensions.Add(
				typeof(DataSetSchemaImporterExtension).AssemblyQualifiedName);

			importer.State.Add(typeof(XmlSerializerImportOptions), importOptions);
		}

		//--------------------------------------------------------------------------------
		void AddStateForDataContractSerializerImport (WsdlImporter importer)
		{
			XsdDataContractImporter xsdDataContractImporter =
				new XsdDataContractImporter(this.codeCompileUnit);
			xsdDataContractImporter.Options = new ImportOptions();
			xsdDataContractImporter.Options.ImportXmlType =
				(this.options.FormatMode ==
					DynamicProxyFactoryOptions.FormatModeOptions.DataContractSerializer);

			xsdDataContractImporter.Options.CodeProvider = this.codeDomProvider;
			importer.State.Add(typeof(XsdDataContractImporter),
					xsdDataContractImporter);

			foreach (IWsdlImportExtension importExtension in importer.WsdlImportExtensions)
			{
				DataContractSerializerMessageContractImporter dcConverter =
					importExtension as DataContractSerializerMessageContractImporter;

				if (dcConverter != null)
				{
					if (this.options.FormatMode ==
						DynamicProxyFactoryOptions.FormatModeOptions.XmlSerializer)
						dcConverter.Enabled = false;
					else
						dcConverter.Enabled = true;
				}

			}
		}

		//--------------------------------------------------------------------------------
		private void CreateProxy ()
		{
			CreateServiceContractGenerator();

			foreach (ContractDescription contract in this.Contracts)
			{
				this.contractGenerator.GenerateServiceContractType(contract);
			}

			this.codegenErrors = this.contractGenerator.Errors;
		}

		//--------------------------------------------------------------------------------
		private void CompileProxy (bool generateDebugInfo)
		{
			// reference the required assemblies with the correct path.
			CompilerParameters compilerParams = new CompilerParameters();

			AddAssemblyReference(
				typeof(System.ServiceModel.ServiceContractAttribute).Assembly,
				compilerParams.ReferencedAssemblies);

			AddAssemblyReference(
				typeof(System.Web.Services.Description.ServiceDescription).Assembly,
				compilerParams.ReferencedAssemblies);

			AddAssemblyReference(
				typeof(System.Runtime.Serialization.DataContractAttribute).Assembly,
				compilerParams.ReferencedAssemblies);

			AddAssemblyReference(typeof(System.Xml.XmlElement).Assembly,
				compilerParams.ReferencedAssemblies);

			AddAssemblyReference(typeof(System.Uri).Assembly,
				compilerParams.ReferencedAssemblies);

			AddAssemblyReference(typeof(System.Data.DataSet).Assembly,
				compilerParams.ReferencedAssemblies);

			//incorporo come risorsa la serializzazione degli endpoints
			string resourceFile = Path.Combine(Path.GetTempPath(), endPointResourceName);
			using (FileStream fs = new FileStream(resourceFile, FileMode.Create, FileAccess.Write))
			{
				SerializableEndPointCollection endpoints = new SerializableEndPointCollection();
				foreach (ServiceEndpoint ep in Endpoints)
					endpoints.Add(new SerializableEndpoint(ep));
				XmlSerializer bf = new XmlSerializer(typeof(SerializableEndPointCollection));
				bf.Serialize(fs, endpoints);
			}
			compilerParams.EmbeddedResources.Add(resourceFile);

			compilerParams.OutputAssembly = GetAssemblyPath();

			CompilerResults results;
			if (generateDebugInfo)
			{
				//in debug genero anche il file sorgente
				compilerParams.IncludeDebugInformation = true;
				String sourceFile = GetAssemblySource();
				using (StreamWriter sw = new StreamWriter(sourceFile))
					sw.Write(this.proxyCode);
				results = this.codeDomProvider.CompileAssemblyFromFile(compilerParams, sourceFile);
			}
			else
			{
				// Set compiler argument to optimize output.
				compilerParams.CompilerOptions = "/optimize";

				results =
					this.codeDomProvider.CompileAssemblyFromSource(
						compilerParams,
						this.proxyCode);
			}

			this.compilerErrors = ToEnumerable(results.Errors);
			
			if (File.Exists(compilerParams.OutputAssembly))
			{
				this.proxyAssembly = results.CompiledAssembly;
				GenerateSerializers(compilerParams, results);
			}
			
			//cancello il file temporaneo di risorsa
			File.Delete(resourceFile);

		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera l'assembly per velocizzare la procedura di serializzazione
		/// </summary>
		/// <param name="compilerParams"></param>
		/// <param name="results"></param>
		private void GenerateSerializers(CompilerParameters compilerParams, CompilerResults results)
		{
			List<Type> types = new List<Type>();
			types.AddRange(this.proxyAssembly.GetTypes());
			SoapReflectionImporter importer = new SoapReflectionImporter();
			List<XmlTypeMapping> mappings = new List<XmlTypeMapping>();
			for (int i = 0; i < types.Count; i++)
			{
				try
				{
					XmlTypeMapping mapping = importer.ImportTypeMapping(types[i]);
					mappings.Add(mapping);
				}
				catch
				{
					//tipo non supportato
					types.RemoveAt(i);
					i--;
				}
			}

			string file = Path.ChangeExtension(compilerParams.OutputAssembly, "XmlSerializers.dll");
			compilerParams.OutputAssembly = file;
			compilerParams.EmbeddedResources.Clear();
			XmlSerializer.GenerateSerializer(types.ToArray(), mappings.ToArray(), compilerParams);
		}
		//--------------------------------------------------------------------------------
		private void CalculateAssemblyFileName ()
		{
			Uri uri = new Uri(descriptionUri);
			StringBuilder pattern = new StringBuilder();
			bool notFirst = false;
			foreach (char ch in Path.GetInvalidFileNameChars())
			{
				if (notFirst)
					pattern.Append('|');
				else
					notFirst = true;
				pattern.Append(Regex.Escape(ch.ToString()));
			}
			pattern.Append(@"|/|\\");
			string name = Regex.Replace(uri.LocalPath, pattern.ToString(), "_").TrimStart(new char[]{'_'});
			assemblyFilename = string.Format(
				"TbDynamicWCF.{0}.{1}{2}", 
				uri.Port,
				mexBinding != null ? mexBinding.Scheme : "",
				name);
		}
		//--------------------------------------------------------------------------------
		private string GetAssemblySource ()
		{
			return Path.Combine(dllCachePath, string.Format("{0}.cs", assemblyFilename));
		}

		//--------------------------------------------------------------------------------
		private string GetAssemblyPath ()
		{
			return Path.Combine(dllCachePath, string.Format("{0}.dll", assemblyFilename));
		}

		//--------------------------------------------------------------------------------
		private void WriteCode ()
		{
			using (StringWriter writer = new StringWriter())
			{
				//scrivo i matadati di versioning
				writer.WriteLine(string.Format("\r\n[assembly: System.Reflection.AssemblyVersion(\"{0}\")]", Assembly.GetExecutingAssembly().GetName().Version));

				CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
				codeGenOptions.BracingStyle = "C";
				this.codeDomProvider.GenerateCodeFromCompileUnit(
						this.codeCompileUnit, writer, codeGenOptions);
				writer.Flush();
				this.proxyCode = writer.ToString();
			}

			// use the modified proxy code, if code modifier is set.
			if (this.options.CodeModifier != null)
				this.proxyCode = this.options.CodeModifier(this.proxyCode);
		}

		//--------------------------------------------------------------------------------
		void AddAssemblyReference (Assembly referencedAssembly,
					StringCollection refAssemblies)
		{
			string path = Path.GetFullPath(referencedAssembly.Location);
			string name = Path.GetFileName(path);
			if (!(refAssemblies.Contains(name) ||
				  refAssemblies.Contains(path)))
			{
				refAssemblies.Add(path);
			}
		}

		//--------------------------------------------------------------------------------
		public ServiceEndpoint GetEndpoint (string contractName)
		{
			return GetEndpoint(contractName, null);
		}

		//--------------------------------------------------------------------------------
		public ServiceEndpoint GetEndpoint (string contractName,
						string contractNamespace)
		{
			ServiceEndpoint matchingEndpoint = null;

			foreach (ServiceEndpoint endpoint in Endpoints)
			{
				if (ContractNameMatch(endpoint.Contract, contractName) &&
					ContractNsMatch(endpoint.Contract, contractNamespace))
				{
					matchingEndpoint = endpoint;
					break;
				}
			}

			if (matchingEndpoint == null)
				throw new ArgumentException(string.Format(
					SoapCallStrings.EndpointNotFound,
					contractName, contractNamespace));

			return matchingEndpoint;
		}

		//--------------------------------------------------------------------------------
		private bool ContractNameMatch (ContractDescription cDesc, string name)
		{
			return (string.Compare(cDesc.Name, name, true) == 0);
		}

		//--------------------------------------------------------------------------------
		private bool ContractNsMatch (ContractDescription cDesc, string ns)
		{
			return ((ns == null) ||
					(string.Compare(cDesc.Namespace, ns, true) == 0));
		}

		//--------------------------------------------------------------------------------
		public DynamicProxy CreateProxy(string contractName, TimeSpan timeout)
		{
			return CreateProxy(contractName, null, timeout);
		}

		//--------------------------------------------------------------------------------
		public DynamicProxy CreateProxy(string contractName, string contractNamespace, TimeSpan timeout)
		{
			ServiceEndpoint endpoint = GetEndpoint(contractName, contractNamespace);

			return CreateProxy(endpoint, timeout);
		}

		//--------------------------------------------------------------------------------
		public DynamicProxy CreateProxy (ServiceEndpoint endpoint, TimeSpan timeout)
		{
			Type contractType = GetContractType(endpoint.Contract.Name, endpoint.Contract.Namespace);
			if (contractType == null)
				return null;

			Type proxyType = GetProxyType(contractType);
			WCFSoapClient.SetBufferMaxValuesToBinding(endpoint.Binding);
			WCFSoapClient.SetTimeoutToBinding(endpoint.Binding, timeout);

			if (string.Compare(endpoint.Address.Uri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) != 0
				&&
				string.Compare(endpoint.Address.Uri.Host, BasePathFinder.BasePathFinderInstance.RemoteWebServer, System.StringComparison.OrdinalIgnoreCase) != 0)
			{
				ILoginManager loginMng = new LoginManager();

                string server; int port;
				loginMng.GetProxySettings(out server, out port);
				if (loginMng.GetProxySettings(out server, out port) && !string.IsNullOrEmpty(server))
				{
					UriBuilder ub = new UriBuilder(server);
					if (port != 0)
						ub.Port = port;
				
					WCFSoapClient.SetProxyToBinding(endpoint.Binding, ub.Uri);
				}
			}
			
			
			return new DynamicProxy(proxyType, endpoint.Binding, endpoint.Address);
		}



		//--------------------------------------------------------------------------------
		private Type GetContractType (string contractName, string contractNamespace)
		{
			if (proxyAssembly == null)
				return null;

			Type[] allTypes = proxyAssembly.GetTypes();
			ServiceContractAttribute scAttr = null;
			Type contractType = null;
			XmlQualifiedName cName;
			foreach (Type type in allTypes)
			{
				// Is it an interface?
				if (!type.IsInterface) continue;

				// Is it marked with ServiceContract attribute?
				object[] attrs = type.GetCustomAttributes(
					typeof(ServiceContractAttribute), false);
				if ((attrs == null) || (attrs.Length == 0)) continue;

				// is it the required service contract?
				scAttr = (ServiceContractAttribute)attrs[0];
				cName = GetContractName(type, scAttr.Name, scAttr.Namespace);

				if (string.Compare(cName.Name, contractName, true) != 0)
					continue;

				if (string.Compare(cName.Namespace, contractNamespace,
							true) != 0)
					continue;

				contractType = type;
				break;
			}

			if (contractType == null)
				throw new ArgumentException(
					SoapCallStrings.UnknownContract);

			return contractType;
		}

		internal const string DefaultNamespace = "http://tempuri.org/";
		//--------------------------------------------------------------------------------
		internal static XmlQualifiedName GetContractName (Type contractType,
					string name, string ns)
		{
			if (String.IsNullOrEmpty(name))
			{
				name = contractType.Name;
			}

			if (ns == null)
			{
				ns = DefaultNamespace;
			}
			else
			{
				ns = Uri.EscapeUriString(ns);
			}

			return new XmlQualifiedName(name, ns);
		}

		//--------------------------------------------------------------------------------
		private Type GetProxyType (Type contractType)
		{
			Type clientBaseType = typeof(ClientBase<>).MakeGenericType(
					contractType);

			Type[] allTypes = ProxyAssembly.GetTypes();
			Type proxyType = null;

			foreach (Type type in allTypes)
			{
				// Look for a proxy class that implements the service 
				// contract and is derived from ClientBase<service contract>
				if (type.IsClass && contractType.IsAssignableFrom(type)
					&& type.IsSubclassOf(clientBaseType))
				{
					proxyType = type;
					break;
				}
			}

			if (proxyType == null)
				throw new DynamicProxyException(string.Format(
							SoapCallStrings.ProxyTypeNotFound,
							contractType.FullName));

			return proxyType;
		}


		//--------------------------------------------------------------------------------
		private void CreateCodeDomProvider ()
		{
			this.codeDomProvider = CodeDomProvider.CreateProvider(options.Language.ToString());
		}

		//--------------------------------------------------------------------------------
		private void CreateServiceContractGenerator ()
		{
			this.contractGenerator = new ServiceContractGenerator(
				this.codeCompileUnit);
			this.contractGenerator.Options |= ServiceContractGenerationOptions.ClientClass;
		}

		//--------------------------------------------------------------------------------
		public IEnumerable<MetadataSection> Metadata
		{
			get
			{
				return this.metadataCollection;
			}
		}

		//--------------------------------------------------------------------------------
		public IEnumerable<Binding> Bindings
		{
			get
			{
				return this.bindings;
			}
		}

		//--------------------------------------------------------------------------------
		public IEnumerable<ContractDescription> Contracts
		{
			get
			{
				return this.contracts;
			}
		}

		//--------------------------------------------------------------------------------
		public IEnumerable<ServiceEndpoint> Endpoints
		{
			get
			{
				return this.endpoints;
			}
		}

		//--------------------------------------------------------------------------------
		public Assembly ProxyAssembly
		{
			get
			{
				return this.proxyAssembly;
			}
		}

		//--------------------------------------------------------------------------------
		public string ProxyCode
		{
			get
			{
				return this.proxyCode;
			}
		}

		//--------------------------------------------------------------------------------
		public static string ToString (IEnumerable<MetadataConversionError> importErrors)
		{
			if (importErrors != null)
			{
				StringBuilder importErrStr = new StringBuilder();

				foreach (MetadataConversionError error in importErrors)
				{
					if (error.IsWarning)
						importErrStr.AppendFormat(SoapCallStrings.Warning, error.Message);
					else
						importErrStr.AppendFormat(SoapCallStrings.Error, error.Message);
				}

				return importErrStr.ToString();
			}
			else
			{
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		public static string ToString (IEnumerable<CompilerError> compilerErrors)
		{
			if (compilerErrors != null)
			{
				StringBuilder builder = new StringBuilder();
				foreach (CompilerError error in compilerErrors)
					builder.AppendLine(error.ToString());

				return builder.ToString();
			}
			else
			{
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		private static IEnumerable<CompilerError> ToEnumerable (CompilerErrorCollection collection)
		{
			if (collection == null) return null;

			List<CompilerError> errorList = new List<CompilerError>();
			foreach (CompilerError error in collection)
				errorList.Add(error);

			return errorList;
		}
	}
}
