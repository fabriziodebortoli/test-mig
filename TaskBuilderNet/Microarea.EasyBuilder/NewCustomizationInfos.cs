using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microsoft.CSharp;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder
{
	//================================================================================
	/// <summary>
	/// Stores all information about the current customization.
	/// </summary>
	[Serializable]
	internal class NewCustomizationInfos
	{
		internal const string CSSourceFileExtension = ".cs";
		internal const string CSProjectExtension = ".csproj";
        internal const string CSProjectUserExtension = ".csproj.user";
        internal const string MetaSourceFileExtension = "meta";
		internal const string PartialSourceFileExtension = "partial";
		internal const string UserMethodsSourceFileExtension = "usermethods";
		internal const string BusinessObjectsToken = "BusinessObjects";
        internal const string BusinessObjectToken = "BusinessObject";

		internal static readonly string[] AllSupportedExtensions = new string[] { CSSourceFileExtension, String.Concat(MetaSourceFileExtension, CSSourceFileExtension), String.Concat(UserMethodsSourceFileExtension, CSSourceFileExtension) };

		private SyntaxTree metadataInfoCompilationUnit;
		private SyntaxTree ebDesignerCompilationUnit;

		private SyntaxTree userMethodsCompilationUnit;
		private string userMethodsCode;

		private List<SyntaxTree> additionalCompilationUnits = new List<SyntaxTree>();
		private List<UsingDeclaration> usings = new List<UsingDeclaration>();

		private NameSpace customizationNamespace;
        private NameSpace safeCodeCustomizationNamespace;

        internal event EventHandler<UsingsChanged> UsingsChanged;

		//--------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		internal SyntaxTree MetadataInfoCompilationUnit
		{
			get
			{
				return metadataInfoCompilationUnit;
			}
		}

		//--------------------------------------------------------------------------------
		private static SyntaxTree InitMetadataInfoCompilationUnit()
		{
			SyntaxTree aCompilationUnit = new SyntaxTree();

			//L'asterisco è necessario per far si che le diverse dll compilate abbiano un AssemblyName diverso.
			//Questo serve perchè .net sia in grado, tra tutte le versioni della stessa dll caricate in memoria, di
			//trovare proprio quella che serve a noi.
			//Il problema è venuto a galla quando abbiamo cominciato a modificare le dll che erano referenziate dalla dll di
			//documento (la dll di modulo, le varie dll nella cartella ReferencedAssemblies e la dll degli enumerativi).
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			String newVersion = String.Format(
				"{0}.{1}.{2}.*",
				version.Major,
				version.Minor,
				version.Build
				);

			AttributeSection assemblyAttributeSection = new AttributeSection();
			assemblyAttributeSection.AttributeTarget = "assembly";
			aCompilationUnit.Members.Add(assemblyAttributeSection);

			ICSharpCode.NRefactory.CSharp.Attribute attribute =
				AstFacilities.GetAttribute(typeof(AssemblyCompanyAttribute).FullName, new PrimitiveExpression("Microarea S.p.A."));

			assemblyAttributeSection.Attributes.Add(attribute);

			assemblyAttributeSection = new AttributeSection();
			assemblyAttributeSection.AttributeTarget = "assembly";
			aCompilationUnit.Members.Add(assemblyAttributeSection);

			attribute =
				AstFacilities.GetAttribute(typeof(AssemblyVersionAttribute).FullName, new PrimitiveExpression(string.Format("{0}", newVersion)));

			assemblyAttributeSection.Attributes.Add(attribute);

			assemblyAttributeSection = new AttributeSection();
			assemblyAttributeSection.AttributeTarget = "assembly";
			aCompilationUnit.Members.Add(assemblyAttributeSection);

			attribute =
				AstFacilities.GetAttribute(typeof(AssemblyProductAttribute).FullName, new PrimitiveExpression("EasyStudio assembly"));

			assemblyAttributeSection.Attributes.Add(attribute);

			assemblyAttributeSection = new AttributeSection();
			assemblyAttributeSection.AttributeTarget = "assembly";
			aCompilationUnit.Members.Add(assemblyAttributeSection);

			attribute =
				AstFacilities.GetAttribute(typeof(AssemblyDescriptionAttribute).FullName, new PrimitiveExpression("EasyStudio generated assembly file "));

			assemblyAttributeSection.Attributes.Add(attribute);

			IEasyBuilderApp currentApp = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;
			if (
				currentApp.ApplicationType == ApplicationType.Standardization
				)
			{
				Type attrType = typeof(OwnerEasyBuilderAppAttribute);

				assemblyAttributeSection = new AttributeSection();
				assemblyAttributeSection.AttributeTarget = "assembly";
				aCompilationUnit.Members.Add(assemblyAttributeSection);

				attribute =
					AstFacilities.GetAttribute(
						attrType.FullName,
						new PrimitiveExpression(currentApp.ApplicationName),
						new PrimitiveExpression(currentApp.ModuleName)
						);

				assemblyAttributeSection.Attributes.Add(attribute);
			}

			return aCompilationUnit;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Gets the instance of the current CodeNameSpace containing the code
		/// automatically generated by easybuilder
		/// </summary>
		internal SyntaxTree EbDesignerCompilationUnit { get { return ebDesignerCompilationUnit; } }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Gets the instance of the current CodeNameSpace containing the code of
		/// all methods customized by the user
		/// </summary>
		internal SyntaxTree UserMethodsCompilationUnit { get { return userMethodsCompilationUnit; } }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Gets the instance of the current CodeNameSpace containing the code of
		/// all methods customized by the user
		/// </summary>
		internal string UserMethodsCode {
			get { return userMethodsCode; }
			set { userMethodsCode = value; } }

		//--------------------------------------------------------------------------------
		internal List<SyntaxTree> AdditionalCompilationUnits
		{
			get
			{
				if (additionalCompilationUnits == null)
					additionalCompilationUnits = new List<SyntaxTree>();

				return additionalCompilationUnits;
			}
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// NewCustomizationInfos
		/// </summary>
		internal NewCustomizationInfos(SyntaxTree userMethodsCompilationUnit)
			: this (userMethodsCompilationUnit, null, new SyntaxTree(), new List<SyntaxTree>(){})
		{
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// NewCustomizationInfos
		/// </summary>
		internal NewCustomizationInfos(
			SyntaxTree userMethodsCompilationUnit,
			SyntaxTree metadataInfoCompilationUnit,
			SyntaxTree ebDesignerCompilationUnit,
			IEnumerable<SyntaxTree> additionalCompilationUnits
			)
		{
			this.userMethodsCompilationUnit = userMethodsCompilationUnit;

			if (metadataInfoCompilationUnit == null)
				this.metadataInfoCompilationUnit = InitMetadataInfoCompilationUnit();
			else
				this.metadataInfoCompilationUnit = metadataInfoCompilationUnit;

			this.ebDesignerCompilationUnit = ebDesignerCompilationUnit;
			this.additionalCompilationUnits.AddRange(additionalCompilationUnits);

			//inizializzo il namespace del customizationinfo con quello estratto dalla NamespaceDeclaration dei sorgenti di modulo
			//se esistenti
			NamespaceDeclaration nsDecl = EasyBuilderSerializer.GetNamespaceDeclaration(ebDesignerCompilationUnit);
			if (nsDecl != null)
				InitCodeNamespace(new NameSpace(nsDecl.Name));
		}

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Ritorna la dichiarazione di classe a partire dal suo nome
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal TypeDeclaration FindClassInAdditionalNamespaces(string name)
		{
			TypeDeclaration type = null;
			foreach (SyntaxTree cu in AdditionalCompilationUnits)
			{
				NamespaceDeclaration codeNs = EasyBuilderSerializer.GetNamespaceDeclaration(cu);
				foreach (INode node in codeNs.Members)
				{
					type = node as TypeDeclaration;
					if (type != null && type.Name == name)
					{
						return type;
					}
				}
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the TaskBuilder namespace for the current customization.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
		internal NameSpace Namespace
		{
			get { return customizationNamespace; }
			set
			{
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (customizationNamespace.Equals(value))
                {
                    return;
                }
                customizationNamespace = value;
                customizationNamespace.Leaf = EasyBuilderSerializer.Escape(customizationNamespace.Leaf);

                safeCodeCustomizationNamespace = new NameSpace(EasyBuilderSerializer.Escape(customizationNamespace.FullNameSpace));

				NamespaceDeclaration nsDecl = EasyBuilderSerializer.GetNamespaceDeclaration(ebDesignerCompilationUnit);
				if (nsDecl == null)
                    throw new NoSourcesOrDatException();

				nsDecl.Name = safeCodeCustomizationNamespace.FullNameSpace;

                nsDecl = EasyBuilderSerializer.GetNamespaceDeclaration(userMethodsCompilationUnit);
                //Commentato Assert perche` troppo restrittivo: infatti e` ammesso che userMethodsCompilationUnit sia null nel caso di utilizzo di business objects.
                //System.Diagnostics.Debug.Assert(nsDecl != null);
                if (nsDecl != null)
                {
                    nsDecl.Name = safeCodeCustomizationNamespace.FullNameSpace;
                }
            }
		}

		/// <summary>
		/// Initializes the current instance with the given namespace name.
		/// </summary>
		/// <param name="ns">The name of the namespace</param>
		//--------------------------------------------------------------------------------
		internal void InitCodeNamespace(INameSpace ns)
		{
            customizationNamespace = ns as NameSpace;
            customizationNamespace.Leaf = EasyBuilderSerializer.Escape(customizationNamespace.Leaf);
            safeCodeCustomizationNamespace = EasyBuilderSerializer.Escape(customizationNamespace.FullNameSpace);
            NamespaceDeclaration decl = new NamespaceDeclaration(safeCodeCustomizationNamespace);

			if (!ebDesignerCompilationUnit.Contains(decl))
			{
				ebDesignerCompilationUnit.Members.Add(decl);
			}
		}

		//--------------------------------------------------------------------------------
		internal SyntaxTree InitAdditionalCompilationUnit(string @namespace)
		{
			//Se gia` esiste non faccio nulla perche` i suoi using sono gia` a
			//posto e anche gli using degli altri verso di lui sono gia` a poosto
			for (int i = 0; i < AdditionalCompilationUnits.Count; i++)
			{
				SyntaxTree current = (SyntaxTree)AdditionalCompilationUnits[i];
				NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(current);
				if (ns.Name == @namespace)
				{
					return current;
				}
			}

			SyntaxTree cu = new SyntaxTree();
			//Aggiungo alla cu appena creata gli using che ho in canna
			foreach (var u in usings)
			{
				cu.Members.Add(u.Clone());
			}
			string cuNamespace = Sources.GetSerializedNamespace(new NameSpace(@namespace));

			NamespaceDeclaration nsDecl = new NamespaceDeclaration(cuNamespace);
			cu.Members.Add(nsDecl);

			//Altrimenti la aggiungo alle additional compilation unit.
			AdditionalCompilationUnits.Add(cu);

			//Aggiungo alle altre cu lo using alla cu appena aggiunta
			AddUsing(cuNamespace);
			
			cu.FileName = Sources.GetCustomizedFilesPath(customizationNamespace, nsDecl.Name);
			return cu;
		}


		//--------------------------------------------------------------------------------
		internal void ClearUsings()
		{
			ClearUsing(ebDesignerCompilationUnit);
			ClearUsing(userMethodsCompilationUnit); 
			ClearUsingFromUserMethods();

			foreach (var cu in additionalCompilationUnits)
			{
				ClearUsing(cu);
			}

			usings.Clear();
		}

		//--------------------------------------------------------------------------------
		private void ClearUsingFromUserMethods()
		{
			if (userMethodsCode.IsNullOrEmpty())
				return;
			//userMethodsCode = userMethodsCompilationUnit.ToString();

			string usmUsings = userMethodsCode.Split(new string[] { "namespace" }, StringSplitOptions.None)[0];
			string umcUsings = userMethodsCompilationUnit.ToString().Split(new string[] { "namespace" }, StringSplitOptions.None)[0];
			if(!string.IsNullOrEmpty(usmUsings) && !string.IsNullOrEmpty(umcUsings))
				userMethodsCode = userMethodsCode.Replace(usmUsings, umcUsings);

		}

		//--------------------------------------------------------------------------------
		private static void ClearUsing(SyntaxTree compilationUnit)
		{
			if (compilationUnit == null)
				return;

			UsingDeclaration usingDeclaration = null;
			foreach (var child in compilationUnit.Members)
			{
				usingDeclaration = child as UsingDeclaration;
				if (usingDeclaration == null)
					continue;

				usingDeclaration.Remove(); // Removes this node from its parent, i.e. the compilationUnit
			}
		}

		//-------------------------------------------------------------------------------
		internal bool RefreshUsings()
		{
			bool added = false;

			//serve nel caso ci siano using vecchi (derivati da dll reference che ora non sono più usate)
			ClearUsings();
			string region = "#region Default Usings...\r\n";
			string endRegion = "#endregion\r\n";
		    userMethodsCode = userMethodsCode.Replace(region, string.Empty);
			if (!userMethodsCode.Contains(endRegion))
				userMethodsCode = string.Concat(endRegion, userMethodsCode);
			AddUsings(ProjectContentFacilities.DefaultUsings);
			
			foreach (SyntaxTree addOnCu in AdditionalCompilationUnits)
			{
				NamespaceDeclaration addOnNs = EasyBuilderSerializer.GetNamespaceDeclaration(addOnCu);
				added |= AddUsing(addOnNs.Name);
			}

			userMethodsCode = string.Concat(region, userMethodsCode);
			return added;
		}

		//--------------------------------------------------------------------------------
		private bool AddUsingToUserMethods(string @using)
		{
			if (userMethodsCode.IsNullOrEmpty())
				return false;
			usings.Add(new UsingDeclaration(@using));
			usings = usings.Distinct().ToList();
		
			//cerco namespace System.IO se lo trovo, non faccio niente, sto aggiungendo uno using che corrisponde al namespace corrente
			string toFind = string.Format("namespace {0}", @using);
			if (userMethodsCode.IndexOf(toFind) >= 0)
				return false;

			//cerco using System.IO; se lo trovo, non faccio niente
			toFind = string.Format("using {0};", @using);
			if (userMethodsCode.IndexOf(toFind) >= 0)
				return false;

			//altrimenti aggiungo
			userMethodsCode = userMethodsCode.Insert(0, toFind + "\r\n");
			return true;
		}

		//--------------------------------------------------------------------------------
		internal void AddUsings(List<string> usings)
		{
			foreach (var item in usings)
			{
				AddUsing(item);
			}
		}

		//--------------------------------------------------------------------------------
		internal bool AddUsing(string @using)
		{
			bool added = false;

			added |= AddUsing(@using, ebDesignerCompilationUnit);
			added |= AddUsing(@using, userMethodsCompilationUnit);

			added |= AddUsingToUserMethods(@using);

			foreach (var cu in additionalCompilationUnits)
			{
				added |= AddUsing(@using, cu);
			}

			return added;
		}

		//--------------------------------------------------------------------------------
		private bool AddUsing(string @using, SyntaxTree compilationUnit)
		{
			if (compilationUnit == null)
				return false;

			UsingsChanged?.Invoke(this, new EasyBuilder.UsingsChanged(@using, EasyBuilder.UsingsChanged.UsingsChangedEnum.Added));
			usings.Add(new UsingDeclaration(@using));
			usings = usings.Distinct().ToList();

			UsingDeclaration usingDeclaration = null;
			foreach (var child in compilationUnit.Members)
			{
				usingDeclaration = child as UsingDeclaration;
				if (usingDeclaration == null)
					continue;

				if (string.Compare(usingDeclaration.Namespace, @using, StringComparison.InvariantCulture) == 0)
					return false;
			}
			NamespaceDeclaration namespaceDeclaration = null;
			foreach (var child in compilationUnit.Members)
			{
				namespaceDeclaration = child as NamespaceDeclaration;
				if (namespaceDeclaration == null)
					continue;

				if (namespaceDeclaration.Name == @using)
				{
					return false;
				}
			}
			if (compilationUnit.Members.Count > 0)
			{
				compilationUnit.Members.InsertBefore(compilationUnit.Members.First(), new UsingDeclaration(@using));
			}
			else
			{
				compilationUnit.Members.Add(new UsingDeclaration(@using));
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		private static void RemoveUsing(SyntaxTree compilationUnit, string @using)
		{
			if (compilationUnit == null)
				return;

			UsingDeclaration usingDeclaration = null;

			List<AstNode> members = compilationUnit.Members.ToList();
			for (int i = members.Count - 1; i >= 0; i--)
			{
				usingDeclaration = members[i] as UsingDeclaration;
				if (usingDeclaration == null)
					continue;

				if (string.Compare(usingDeclaration.Namespace, @using, StringComparison.InvariantCulture) == 0)
				{
					compilationUnit.Members.Remove(usingDeclaration);
				}
			}
		}

		//--------------------------------------------------------------------------------
		internal void RemoveUsing(string fullNamespace)
		{
			UsingsChanged?.Invoke(this, new EasyBuilder.UsingsChanged(fullNamespace, EasyBuilder.UsingsChanged.UsingsChangedEnum.Removed));

			//levo lo usig incriminato dalla collection di using generale
			for (int i = usings.Count - 1; i >= 0; i--)
			{
				if (usings[i].Namespace.Contains(fullNamespace))
				{
					usings.RemoveAt(i);
		
					break;
				}
			}

			RemoveUsing(ebDesignerCompilationUnit, fullNamespace);
			RemoveUsing(userMethodsCompilationUnit, fullNamespace);
			RemoveUsingFromUserMethods(fullNamespace);
			foreach (var cu in additionalCompilationUnits)
			{
				RemoveUsing(cu, fullNamespace);
			}
		}

	
		//--------------------------------------------------------------------------------
		private void RemoveUsingFromUserMethods(string fullNamespace)
		{
			if (userMethodsCode.IsNullOrEmpty())
				return;

			string toDelete = string.Format("using {0};\r\n", fullNamespace);
			userMethodsCode = userMethodsCode.Replace(toDelete, string.Empty);
		}

		//--------------------------------------------------------------------------------
		internal IUnresolvedFile RemoveAdditionalNamespace(string fullNamespace)
		{
			if (additionalCompilationUnits == null)
				return null;

			IUnresolvedFile file = null;
			for (int i = additionalCompilationUnits.Count - 1; i >= 0; i--)
			{
				SyntaxTree cu = AdditionalCompilationUnits[i] as SyntaxTree;
				NamespaceDeclaration ns = EasyBuilderSerializer.GetNamespaceDeclaration(cu);
				if (ns.Name == fullNamespace)
				{
					file = cu.ToTypeSystem();

					AdditionalCompilationUnits.RemoveAt(i);
					RemoveUsing(fullNamespace);
				}
			}

			return file;
		}

	/*	//--------------------------------------------------------------------------------
		/// <summary>
		/// Returns a value indicating if a customization for the given
		/// customization namespace exists.
		/// </summary>
		internal static bool ExistCustomizationName(INameSpace customizationNamespace, IEasyBuilderApp app)
		{
			return
				File.Exists(BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(customizationNamespace, null, app)) ||
				File.Exists(BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(customizationNamespace, CUtility.GetUser(), app));
		}*/

		//--------------------------------------------------------------------------------
		internal void UpdateUserMethodsCompilationUnit(string fileContent, string fileName)
		{
			userMethodsCompilationUnit = AstFacilities.Parse(fileContent, fileName);
			userMethodsCode = fileContent;
		}

		//--------------------------------------------------------------------------------
		internal void UpdateUserMethodsCompilationUnit(SyntaxTree syntaxTree)
		{
			userMethodsCompilationUnit = syntaxTree;
		}
	}

	//================================================================================
	/// <summary>
	/// Internal use
	/// </summary>
	internal class UsingsChanged : EventArgs
	{
		internal enum UsingsChangedEnum { Added, Removed}

		string usingNamespace;

		UsingsChangedEnum usingsChangedType = UsingsChangedEnum.Added;

		//--------------------------------------------------------------------------------
		public string UsingNamespace
		{
			get
			{
				return usingNamespace;
			}
		}

		//--------------------------------------------------------------------------------
		internal UsingsChangedEnum UsingsChangedType
		{
			get
			{
				return usingsChangedType;
			}

			
		}

		/// <remarks />
		//--------------------------------------------------------------------------------
		internal UsingsChanged(string usingNamespace, UsingsChangedEnum usingsChangedType)
		{
			this.usingNamespace = usingNamespace;
			this.usingsChangedType = usingsChangedType;
		}
	}
}