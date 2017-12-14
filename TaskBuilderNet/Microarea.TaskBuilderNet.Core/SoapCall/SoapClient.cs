using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;


//	Legenda:
//		FunctionName		nome della funzione
//		RefParam			nome del parametro passato by	ref
//		ValueParam			nome del parametro passato per	value
//		OutParam			nome del parametro passato per	out
//		valore				il valore attuale del parametro
//		ServiceName			servizio che implementa la funzione (es:/WebService2/Service2.asmx)

// sintassi di Request:
/*
POST ServiceName HTTP/1.1
Host: localhost
Content-Type: text/xml; charset=utf-8
Content-Length: length
SOAPAction: "http://tempuri.org/FunctionName"

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
<soap:Header>
	<HeaderInfo soap:mustUnderstand="1" xmlns="http://tempuri.org/>
		<AuthToken>valore</AuthToken>
	</HeaderInfo>
</soap:Header>
				
<soap:Body>
    <FunctionName xmlns="http://tempuri.org/">
      <RefParam>valore</RefParam>
      <ValueParam>valore</ValueParam>
    </FunctionName>
  </soap:Body>
</soap:Envelope>

*/

// Sintassi di Respose
/*
HTTP/1.1 200 OK
Content-Type: text/xml; charset=utf-8
Content-Length: length

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <FunctionNameResponse xmlns="http://tempuri.org/">
      <FunctionNameResult>string</FunctionNameResult>
      <RefParam>valore</RefParam>
      <OutParam>valore</OutParam>
    </FunctionNameResponse>
  </soap:Body>
</soap:Envelope>
*/

namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	class DummyTbSoapArgument : TbSoapArgument
	{
		int handle = 0;
		string funcNamespace = "";
		public DummyTbSoapArgument(string authToken, string funcNamespace, int handle)
		{
			this.handle = handle;
			this.HeaderInfo = new TBHeaderInfo();
			this.HeaderInfo.AuthToken = authToken;
			this.funcNamespace = funcNamespace;
		}
		public override int GetContextHandle()
		{
			return handle;
		}

		public override string GetFunctionNamespace()
		{
			return funcNamespace;
		}
	}

	/// <summary>
	/// Summary description for SoapClientInterface.
	/// </summary>
	///=============================================================================
	public class SoapClient
	{
		private const string			microareaWSNamespace = "urn:Microarea.Web.Services";

		protected	FunctionPrototype	prototype = null;
		protected	Stack				actualParameters = null;
		protected	object				result = null;
		protected	ArrayList			parametersData = new ArrayList();
		protected	HttpWebRequest		request = null;
		protected	WebResponse			response = null;
		protected	XmlDocument			xmlParser;
		protected	XmlNamespaceManager namespaceManager;
		protected	string				soapAction;
		protected	string				serviceNamespace;
		protected	string				authenticationToken;
		protected	string				server;
		protected	int 				port;
		protected	string				service;
		protected	bool				useHTTP = true;

		//-----------------------------------------------------------------------------
		public	object	Result		{ get { return result; }}

		public string	Server		{ get { return server; } set { server = value; }}
		public int	    Port		{ get { return port; } set { port = value; }}
		public string	Service		{ get { return service; } set { service = value; }}
		public bool		UseHTTP		{ get { return useHTTP; }}

		//-----------------------------------------------------------------------------
		public string Url			
		{ 
			get 
			{ 
				// cotruisco la Url di chiamata sintassi (server:port/service)
				string url;
				
				url = server;
				url += ":" + port;
				url += "/" + service;
				
				return url; 
			}
		}

		//-----------------------------------------------------------------------------
		public void AddParam (string name, string type, ParameterModeType mode, object val)
		{ 
			prototype.Parameters.Add(new Parameter(name, type, mode));
			parametersData.Add(new DataItem(val));
		}

		//-----------------------------------------------------------------------------
		public object GetParameterByName(string name)
		{
			for (int i = 0; i < prototype.Parameters.Count; i++)
				if (string.Compare(name, prototype.Parameters[i].Name, true, CultureInfo.InvariantCulture) == 0) 
					return ((DataItem)parametersData[i]).Data;

			return null;
		}

		//-----------------------------------------------------------------------------
		public bool IsExternalService { get { return prototype.Service != ""; } }

		// Ammette solo l'attributo Soap.Literal
		//-----------------------------------------------------------------------------
		public SoapClient (FunctionPrototype prototype, Stack actualParameters)
		{
			this.prototype			= prototype;
			this.actualParameters	= actualParameters;

			
			BindParametersData();
			CreateXmlParser();
			InitServiceNamespace(microareaWSNamespace);
		}

		//-----------------------------------------------------------------------------
		public SoapClient(FunctionPrototype prototype)
		{
			this.prototype			= prototype;
			CreateXmlParser();
			InitServiceNamespace(microareaWSNamespace);
		}

		//-----------------------------------------------------------------------------
		public SoapClient()
		{
			CreateXmlParser();
			InitServiceNamespace(microareaWSNamespace);
		}

		//-----------------------------------------------------------------------------
		private void InitServiceNamespace(string ns)
		{
			if (ns == serviceNamespace) return;
			string existingNs  = namespaceManager.LookupNamespace("ns");
			if (existingNs != null && existingNs.Length > 0)
				namespaceManager.RemoveNamespace("ns", existingNs);

			serviceNamespace = ns;
			namespaceManager.AddNamespace("ns", serviceNamespace);
		}

		//-----------------------------------------------------------------------------
		virtual public void SetConnectionInfo(string server, int port, string authenticationToken, string nameSpace, string functionName)
		{
			server = "http://localhost";

			this.server = /*"http://" +*/ server;
			this.port = port;
			this.authenticationToken = authenticationToken;

			service	= "Invoke?Handler=" + nameSpace;
			soapAction = string.Format("\"#{0}\"", functionName);
		
			InitServiceNamespace(microareaWSNamespace);
		}

		// gestisco quanto letto dal prototipo della funzione per definire il WebService (interno o esterno)
		// se Service non contiene niente allora suppongo che si stia parlando di un WebService di TaskBuilder
		// il cui nome lo deduco dal namespace del prototipo della funzione
		//-----------------------------------------------------------------------------
		virtual public void SetConnectionInfo(string server, int port, string authenticationToken)
		{
			string ns = microareaWSNamespace;
			if (IsExternalService)
			{
				port		= prototype.Port;
				service		= prototype.Service;
				server		= (prototype.Server == "") ? "http://localhost" : prototype.Server;
				ns			= (prototype.ServiceNamespace == "") ? "http://tempuri.org/" : prototype.ServiceNamespace;
				soapAction	= string.Format("\"{0}{1}\"", serviceNamespace, prototype.Name);
			}
			else
			{
				// Se non ho una interfaccia a TB (non è caricato) allora collasso su localhost e porta 80
				server = "http://localhost";
			
				this.server = /*"http://" +*/ server;
				this.port = port;
				this.authenticationToken = authenticationToken;

                service = "Invoke?Handler=" + prototype.NameSpace.GetEndPointNameSpace();
				ns = microareaWSNamespace;
				soapAction = string.Format("\"#{0}\"", prototype.Name);
			}

			InitServiceNamespace(ns);
		}

		// bind ai dati presenti nello stack della espressione
		//-----------------------------------------------------------------------------
		private void BindParametersData()
		{
			parametersData.Clear();
			for (int i = 0; i < prototype.Parameters.Count; i++)
				parametersData.Add(actualParameters.Pop());
		}

		// parser gestore del messaggio soap di risposta
		//-----------------------------------------------------------------------------
		private void CreateXmlParser()
		{
			xmlParser = new XmlDocument();
			namespaceManager = new XmlNamespaceManager(xmlParser.NameTable);
			namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
		}

		// controlla che non ci sia stato un errore nel pacchetto SOAP
		//-----------------------------------------------------------------------------
		private void CheckSoapMessage()
		{
			// Recuper il valore di ritorno ed i parametri della chiamata SOAP
			XmlNode functionNode = xmlParser.SelectSingleNode(string.Format("//soap:Envelope/soap:Body/soap:Fault"), namespaceManager);
			if (functionNode == null) return;

			// recupera il valore di ritorno
			string faultCode	= functionNode.SelectSingleNode("faultcode", namespaceManager).InnerText;
			string faultString  = functionNode.SelectSingleNode("faultstring", namespaceManager).InnerText;
			string details		= functionNode.SelectSingleNode("detail", namespaceManager).InnerText;

			throw (new SoapClientException(string.Format(SoapCallStrings.SoapFault, faultCode, faultString, details)));
		}

		//-----------------------------------------------------------------------------
		public string BuildRequest()
		{
			StringBuilder postData = new StringBuilder();

			postData.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			postData.Append("<soap:Envelope ");
			postData.Append("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ");
			postData.Append("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" ");
			postData.Append("xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
			postData.Append(">");

			if (!IsExternalService)
			{
				postData.Append("<soap:Header><HeaderInfo soap:mustUnderstand=\"1\" ");
				postData.AppendFormat("xmlns=\"{0}\"><AuthToken>{1}</AuthToken></HeaderInfo></soap:Header>",
					serviceNamespace,
					authenticationToken);
			}
			// costruisce il body della chiamata
			postData.Append("<soap:Body>");
			postData.Append(string.Format("<{0} xmlns=\"{1}\">", prototype.Name, serviceNamespace));

			// Aggiunge i parametri di tipo [In] e [In Out] (Value e ref in C#)
			for (int i = 0; i < prototype.Parameters.Count; i++)
			{
				Parameter param = prototype.Parameters[i];

				// i parametri di tipo [Out] non si spediscono nella Request Soap ma si leggono dalla response
				if (param.Mode == ParameterModeType.Out)
					continue;

				string paramName = param.Name;
				postData.AppendFormat("<{0}>", paramName);
				postData.Append(SoapTypes.To(((DataItem)parametersData[i]).Data));
				postData.AppendFormat("</{0}>", paramName);
			}

			postData.AppendFormat("</{0}>", prototype.Name);
			postData.Append("</soap:Body>");
			postData.Append("</soap:Envelope>");

			return postData.ToString();
		}

		//-----------------------------------------------------------------------------
		public string ChangeToken(string requestString)
		{
			const string startTokenVal	= "<AuthToken>";
			const string endTokenVal	= "</AuthToken>";

			int startToken = requestString.IndexOf(startTokenVal);
			if (startToken < 0)
				return requestString;

			int endToken = requestString.IndexOf("</AuthToken>");
			if (endToken < 0)
				return requestString;

			startToken += startTokenVal.Length;

			string oldToken = requestString.Substring(startToken, endToken - startToken);

			string newReq = requestString.Replace(startTokenVal + oldToken + endTokenVal, startTokenVal + this.authenticationToken + endTokenVal);
	
			return newReq;
		}

		//-----------------------------------------------------------------------------
		private void CreateSoapRequest(string encodedBody)
		{
			request = (HttpWebRequest)HttpWebRequest.Create(Url);
			request.Method = "POST";

			// trasorma l'encoding per renderlo compatibile con il protocollo HTTP
			byte[] someByte = Encoding.UTF8.GetBytes(encodedBody);

			// Definisce il tipo di codifica dei dati inviati.
			request.ContentType="text/xml; charset=utf-8";
		
			// indica quanti byte devono essere inviati.
			request.ContentLength = someByte.Length;

			// definisce l'azione SOAP da eseguire
			request.Headers.Add("SOAPAction", soapAction);
		
			// scrive il messaggio SOAP nello stream di chiamata
			Stream newStream = request.GetRequestStream();
			newStream.Write(someByte, 0 ,someByte.Length);
			newStream.Close();
		}

		// Effettua la SOAP request e recupera la SOAP Response
		//-----------------------------------------------------------------------------
		private string SoapResponse()
		{
			HttpWebResponse webReponse = null;
			try
			{
				webReponse = (HttpWebResponse) request.GetResponse();
			}
			catch (WebException ex)
			{
				if (ex.Response as HttpWebResponse == null)
					throw ex;
				
				//if the SOAP server signals an error, it is contained in the Response property
				webReponse = (HttpWebResponse) ex.Response;	
			}
			
			Stream ReceiveStream = webReponse.GetResponseStream();

			StreamReader sr = new StreamReader(ReceiveStream, System.Text.Encoding.UTF8);

			string buffer = "";
			Char[] read = new Char[256];
			int count = sr.Read(read, 0, read.Length);
			while (count > 0) 
			{
				buffer += new String(read, 0, count);
				count = sr.Read(read, 0, read.Length);
			}
			return buffer;
		}

		// Recupera il valore di ritorno ed i parametri della chiamata SOAP
		//-----------------------------------------------------------------------------
		private void ParseSoapResponse()
		{
			XmlNode soapBody = xmlParser.SelectSingleNode("//soap:Envelope/soap:Body", namespaceManager);
			XmlNode soapResponse = soapBody.SelectSingleNode(string.Format("ns:{0}Response", prototype.Name), namespaceManager);

			// nel caso non si utilizzi l'attributo Soap "Wrapped" non esiste un elemento di 
			// response che raccoglie (wrap) i parametri di ritorno.
			if (soapResponse == null) soapResponse = soapBody;

			// Parametri di tipo [Out] e [In Out] (out e ref in C#)
			foreach (XmlNode node in soapResponse.ChildNodes)
			{
				if (node == null)
					throw (new SoapClientException(SoapCallStrings.BadResponse));

				// il primo è sempre il valore di ritorno
				if (node == soapResponse.FirstChild)
				{
					if (prototype.ReturnType == "DataArray")
					{     
						result = DataArray.XmlConvertToDataArray(node.OuterXml, prototype.ReturnBaseType);
					}
					else
					{
						// Valore di ritorno (potrebbe non avere un FirstChild se la funzione ritorna valori nulli o "")
						string resultValue = (node.FirstChild == null) ? soapResponse.Value : node.FirstChild.Value;
						resultValue = (resultValue == null) ? "" : resultValue;
						result = SoapTypes.From(resultValue, prototype.ReturnType);
					}
					continue;
				}

				int i = prototype.ParamIndex(node.Name);
				if (i < 0)
					throw (new SoapClientException(string.Format(SoapCallStrings.ParameterNotFound, node.Name)));
				
				// i parametri in OUT e REF non hanno il FirstChild se sono nulli o stringa vuota.
				Parameter param = prototype.Parameters[i];

			
					
				if (node.FirstChild == null)
					((DataItem)parametersData[i]).Data = ObjectHelper.CreateObject(param.Type);
				else
				{
					if (param.Type == FrameworkType.Microsoft.DataArray)
					{
						DataArray values = new DataArray(param.BaseType);

						foreach (XmlNode arrayElementNode in node.ChildNodes)
						{
							values.Elements.Add(SoapTypes.From(arrayElementNode.FirstChild.Value, param.BaseType));
						}
						((DataItem)parametersData[i]).Data = values;
					}
					else
						((DataItem)parametersData[i]).Data = SoapTypes.From(node.FirstChild.Value, param.Type);
				}
			}
		}

		//-----------------------------------------------------------------------------
		public void Invoke()
		{
			try 
			{
				CreateSoapRequest(BuildRequest());
				string soapResponse = SoapResponse();

				xmlParser.LoadXml(soapResponse);

				CheckSoapMessage();
				ParseSoapResponse();
			} 
			catch(WebException e) 
			{
				throw(new SoapClientException(e.Message, e));
			} 
			catch (XmlException e)
			{
				throw (new SoapClientException(SoapCallStrings.BadResponse, e));
			}
			catch (Exception e)
			{
				throw (new SoapClientException(e.Message, e));
			} 
			finally 
			{
				if ( response != null ) 
					response.Close();
			}
		}

		//-----------------------------------------------------------------------------
		public string DispatchRequest(string request)
		{
			string soapResponse = string.Empty;
			try 
			{
				CreateSoapRequest(request);
				soapResponse = SoapResponse();
			} 
			catch(WebException e) 
			{
				throw(new SoapClientException(e.Message, e));
			} 
			catch (XmlException e)
			{
				throw (new SoapClientException(SoapCallStrings.BadResponse, e));
			}
			finally 
			{
			}
			return soapResponse;
		}

		//-----------------------------------------------------------------------------
		public void ParseResponse(string soapResponse)
		{
			try 
			{
				xmlParser.LoadXml(soapResponse);

				CheckSoapMessage();
				ParseSoapResponse();
			} 
			catch(WebException e) 
			{
				throw(new SoapClientException(e.Message, e));
			} 
			catch (XmlException e)
			{
				throw (new SoapClientException(SoapCallStrings.BadResponse, e));
			}
			finally 
			{
				if ( response != null ) 
					response.Close();
			}
		}
	}

	public class WCFSoapClient
	{
		IFunctionPrototype prototype;
		string server;
		int port;
		string authenticationToken;
		Binding mexBinding;
		Binding communicationBinding;
		TimeSpan timeout;

		/// <summary>
		/// Imposta il timeout del binding
		/// </summary>
		/// <param name="bnd"></param>
		/// <param name="p"></param>
		//--------------------------------------------------------------------------------
		public static void SetTimeoutToBinding(Binding bnd, TimeSpan timeout)
		{
			bnd.ReceiveTimeout = bnd.SendTimeout = bnd.OpenTimeout = bnd.CloseTimeout = timeout;
		}
		/// <summary>
		/// Imposta i valori massimi dei buffer per il binding
		/// </summary>
		/// <param name="binding"></param>
		//--------------------------------------------------------------------------------
		public static void SetBufferMaxValuesToBinding(Binding binding)
		{
			if (binding is NetTcpBinding)
			{
				NetTcpBinding b = (NetTcpBinding)binding;

				b.MaxReceivedMessageSize = int.MaxValue;
				b.MaxBufferSize = int.MaxValue;
				SetQuotas(b.ReaderQuotas);
				return;
			}

			if (binding is BasicHttpBinding)
			{
				BasicHttpBinding b = (BasicHttpBinding)binding;

				b.MaxBufferSize = int.MaxValue;
				b.MaxReceivedMessageSize = int.MaxValue;
				SetQuotas(b.ReaderQuotas);
				return;
			}

			if (binding is WSHttpBinding)
			{
				WSHttpBinding b = (WSHttpBinding)binding;

				b.MaxReceivedMessageSize = int.MaxValue;
				SetQuotas(b.ReaderQuotas);
				return;
			}

			if (binding is CustomBinding)
			{
				TcpTransportBindingElement el = ((CustomBinding)binding).Elements.Find<TcpTransportBindingElement>();
				if (el != null)
				{
					el.MaxReceivedMessageSize = el.MaxBufferSize = int.MaxValue;
				}
				return;
			}

			throw new Exception(string.Format(SoapCallStrings.UnsupportedBinding, binding));
		}
		//--------------------------------------------------------------------------------
		internal static void SetProxyToBinding(Binding binding, Uri address)
		{
			if (binding is NetTcpBinding)
			{
				return;
			}

			if (binding is BasicHttpBinding)
			{
				BasicHttpBinding b = (BasicHttpBinding)binding;
				b.ProxyAddress = address;
				b.UseDefaultWebProxy = false;
				return;
			}

			if (binding is WSHttpBinding)
			{
				WSHttpBinding b = (WSHttpBinding)binding;
				b.ProxyAddress = address;
				b.UseDefaultWebProxy = false;
				return;
			}

			if (binding is CustomBinding)
			{
				return;
			}

			throw new Exception(string.Format(SoapCallStrings.UnsupportedBinding, binding));
		}
		//--------------------------------------------------------------------------------
		private static void SetQuotas (XmlDictionaryReaderQuotas quotas)
		{
			quotas.MaxDepth = int.MaxValue;
			quotas.MaxArrayLength = int.MaxValue;
			quotas.MaxBytesPerRead = int.MaxValue;
			quotas.MaxStringContentLength = int.MaxValue;
			quotas.MaxNameTableCharCount = int.MaxValue;
		}

		//--------------------------------------------------------------------------------
		public WCFSoapClient (
			IFunctionPrototype prototype, 
			string server, 
			int port, 
			string authenticationToken, 
			Binding mexBinding,
			Binding communicationBinding,
			TimeSpan timeout
			)
		{
			this.prototype = prototype;
			this.server = server;
			this.mexBinding = mexBinding;
			this.communicationBinding = communicationBinding;
			this.authenticationToken = authenticationToken;
			this.port = port;
			this.timeout = timeout;
		}

		//--------------------------------------------------------------------------------
		public object Call(object[] parameters)
		{
			string url = GetServiceUrl();
			DynamicProxy client = null;
			try
			{
				client = ServiceClientCache.GetClientProxy(mexBinding, communicationBinding, url, timeout, false);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(string.Format(SoapCallStrings.InvalidServiceUrl, url), ex);
			}

			try
			{
				MethodInfo mi = client.GetMethod(prototype.Name);
				List<object> parms = new List<object>();

				AddHeaderParam(client, parms);

				parms.AddRange(parameters);

				object[] paramObjs = parms.ToArray();
				for (int i = 0; i < paramObjs.Length; i++)
				{
                    try
                    {
                        if (paramObjs[i] is Int64)
                            paramObjs[i] = Convert.ToInt32(paramObjs[i]);
                        else if (paramObjs[i] is UInt32)
                            paramObjs[i] = ObjectHelper.CastInt(paramObjs[i]);//cast necessario, caso degli enumerativi che sono uint32 ma nelle soap function sono int32
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        Debug.WriteLine("SoapClient.Call parameter: " + i.ToString() + " " + paramObjs[i].ToString());
                    }
				}

				object ret = mi.Invoke(client.ObjectInstance, paramObjs);

				for (int i = 1; i < paramObjs.Length; i++)
					parameters[i - 1] = paramObjs[i];

				return ret;
			}
			catch (Exception ex)
			{
				throw new ApplicationException(string.Format(SoapCallStrings.ErrorInvokingRemoteFunction, prototype.Name, ex.ToString()), ex);
			}
			finally
			{
			}
		}

		//-----------------------------------------------------------------------
		public static object CallInProcess(string serviceName, string methodName, string authToken, params object[] parms)
		{
			return CallInProcess(serviceName, "", methodName, authToken, 0, parms);
		}
		//-----------------------------------------------------------------------
		public static object CallInProcess(string serviceName, string functionNamespace, string methodName, string authToken, int handle, params object[] parms)
		{
			//recupero l'assembly dove si trova la classe WCF per invocare la funzione
			Assembly asmb = ServiceClientCache.GetServicesAssembly();
			if (asmb == null)
				throw new ApplicationException(SoapCallStrings.CannotLoadWcfAsmb);

			//costruisco dinamicamente il tipo della classe che mi permette di fare la chiamata
			string typeName = string.Format("{0}.{1}", ServiceClientCache.AssemblyNamespace, serviceName);
			Type classType = asmb.GetType(typeName);
			if (classType == null)
				throw new ApplicationException(string.Format(SoapCallStrings.CannotFindClass, typeName));

			//costruisco dimanicamente il nome del metodo da chiamare
			string name = "____" + methodName;
			//String^ nameSpace = CServiceGenerator::GetFunctionNamespace(pFunctionDescription);

			//recupero il metodo by reflection
			MethodInfo mi = classType.GetMethod(name);
			if (mi == null)
				throw new ApplicationException(string.Format(SoapCallStrings.CannotFindMethod, name, typeName));

			DummyTbSoapArgument dummy = new DummyTbSoapArgument(authToken, string.Format("{0}.{1}", functionNamespace, methodName), handle);
			object[] newParms = new object[parms.Length + 1];
			newParms[0] = dummy.GetThreadHwnd();

            for (int i = 1; i < mi.GetParameters().Length; i++)
            {
                ParameterInfo pi = mi.GetParameters()[i];
                newParms[i] = Convert.ChangeType(parms[i - 1], pi.ParameterType);
            }
				
			return mi.Invoke(null, newParms);
		}


		//--------------------------------------------------------------------------------
		private void AddHeaderParam(DynamicProxy client, List<object> parms)
		{
			Type headerType = client.ObjectType.Assembly.GetType("Microarea.Web.Services.TBHeaderInfo");
			object header = Activator.CreateInstance(headerType);
			PropertyInfo pi = headerType.GetProperty("AuthToken");
			pi.SetValue(header, authenticationToken, null);
			parms.Add(header);
		}

		//--------------------------------------------------------------------------------
		private string GetServiceUrl()
		{
			UriBuilder builder = new UriBuilder();
			builder.Host = server;
			builder.Scheme = mexBinding.Scheme;
			builder.Port = port;
			builder.Path = prototype.NameSpace.GetEndPointNameSpace() + "/mex";
			return builder.ToString();
		}

		
	}
}