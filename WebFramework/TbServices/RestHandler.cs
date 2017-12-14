using System;
using System.Web;
using Microarea.TaskBuilderNet.Core.SoapCall;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Microarea.WebServices.TbServices
{
	public class RestHandler : IHttpHandler
	{
       
        [DllImport("User32")]
        private extern static int GetGuiResources(IntPtr hProcess, int uiFlags);

		//---------------------------------------------------------------------------
		public bool IsReusable
		{
			get { return true; }
		}

		//---------------------------------------------------------------------------
		public void ProcessRequest(HttpContext context)
		{
			bool needComma = false;
			try
			{
				context.Response.ContentType = "application/json";
				context.Response.Output.WriteLine("{");
				
				string functionName = context.Request.Params["Function"];
                if (functionName != null)
                    CallFunction(context, functionName);
                else if (context.Request.PathInfo.Equals("/getUserObjects/", StringComparison.InvariantCultureIgnoreCase))
                {
                    WriteComma(context, ref needComma);
                    WriteUserObjects(context);
                }
                WriteComma(context, ref needComma);
				context.Response.Output.Write("\"success\": true");	
			}
			catch (Exception ex)
			{
                WriteComma(context, ref needComma);
				context.Response.Output.Write("\"success\": false");

                WriteComma(context, ref needComma);
				context.Response.Output.Write("\"errorDetail\": \r\n");
				ErrorInfo er = new ErrorInfo();
				er.Message = ex.Message;
				er.StackTrace = ex.StackTrace;
				WriteJSon(er, context.Response.OutputStream);
			}
			finally
			{
				context.Response.Output.Write("\r\n}");			
			}
		}

        private void WriteUserObjects(HttpContext context)
        {
            bool needComma = false;
            TbServicesApplication.TbServicesEngine.ValidateTBLoaders();
            context.Response.Output.Write("\"processes\":\r\n[");
            foreach (TbLoaderInfo item in TbServicesApplication.TbServicesEngine.TbLoaders)
            {
                WriteComma(context, ref needComma);
                context.Response.Output.Write("\r\n{\r\n\"Process Id\": ");
                context.Response.Output.Write(item.ProcessId);
                context.Response.Output.Write(",\r\n");
                context.Response.Output.Write(string.Format("\"User Objects\": {0}", GetGuiResources(item.ProcessHandle, 1/*GR_USEROBJECTS*/)));
                context.Response.Output.Write("\r\n}");
            }
            context.Response.Output.Write("\r\n]");
        }

        private void CallFunction(HttpContext context, string functionName)
        {
            bool needComma = false;
            int lastDot = functionName.LastIndexOf('.');
            string moduleNamespace = functionName.Substring(0, lastDot);
            functionName = functionName.Substring(lastDot + 1);
            string authenticationToken = context.Request.Params["Token"];
            DateTime applicationDate;
            if (!DateTime.TryParse(context.Request.Params["ApplicationDate"], out applicationDate))
                applicationDate = DateTime.Now;

            string user = "";
            int tbPort = TbServicesApplication.TbServicesEngine.CreateTB(authenticationToken, applicationDate, true, out user);
            if (tbPort < 0)
                throw new ApplicationException("Error connecting to TBLoader instance; error code: " + tbPort);

            string verb = context.Request.Path.Substring(context.Request.Path.LastIndexOf("/rest/") + 6);//6: lunghezza di '/rest/'

            DynamicProxy proxy = null;
            TimeSpan webServicesTimeout = TimeSpan.FromMinutes(10);
            bool forceAssemblyCreation = false;
            string url = string.Concat("http://localhost:", tbPort, "/", moduleNamespace, "?wsdl");

            //costruisco la classe proxy a partire dal wsdl
            proxy = ServiceClientCache.GetClientProxy(url, webServicesTimeout, forceAssemblyCreation);

            //recupero il metodo da invocare
            MethodInfo mi = proxy.GetMethod(functionName);
            if (mi == null)
                throw new ApplicationException("Method not found: " + functionName);

			WriteComma(context, ref needComma);
            context.Response.Output.Write("\"content\": \r\n");
            if (verb == "invoke")
            {
                Invoke(context, authenticationToken, proxy, mi);
            }
            else if (verb == "query")
            {
                Query(context, authenticationToken, proxy, mi);
            }
            else
            {
                context.Response.Output.Write("null");
                throw new ApplicationException("Verb not supported: " + verb);
            }
            WriteComma(context, ref needComma);
        }

		//---------------------------------------------------------------------------
		private static void WriteComma(HttpContext context, ref bool needComma)
		{
			if (needComma)
				context.Response.Output.Write(",\r\n");
			else
				needComma = true;
		}

		//---------------------------------------------------------------------------
		private void Invoke(HttpContext context, string authenticationToken, DynamicProxy proxy, MethodInfo mi)
		{
			//confronto il nomero di parametri ricevuti con quelli attesi
			ParameterInfo[] methodParams = mi.GetParameters();

			Object[] parameters = new Object[methodParams.Length];

			//travaso i parametri della function description nell'array di parametri da passare nell'invocazione dinamica
			for (int i = 0; i < methodParams.Length; i++)
			{
				ParameterInfo param = methodParams[i];
				string typeName = param.ParameterType.FullName.TrimEnd('&');
				bool isDestTypeArray = false;
				//se e` un array, devo crearlo con la classe Array, che passa
				//il numero di elementi al costruttore
				if (typeName.EndsWith("[]"))
				{
					typeName = typeName.Substring(0, typeName.LastIndexOf("[]"));
					isDestTypeArray = true;
				}
				Type typeToCreate = param.ParameterType.Assembly.GetType(typeName);

				if (param.Name == "HeaderInfo")
				{
					object hi = Activator.CreateInstance(typeToCreate);
					object[] parms = new object[1];
					parms[0] = authenticationToken;
					typeToCreate.GetProperty("AuthToken").GetSetMethod().Invoke(hi, parms);
					parameters[i] = hi;
				}
				else
				{
					string requestParam = context.Request.Params[param.Name];
					if (isDestTypeArray)
					{
						string[] tokens = Adjust(Regex.Split(requestParam, "(?<!,),(?!,)"));//spezzo in base alla virgola, se ne ho due lo considero un escaping
						Array ar = Array.CreateInstance(typeToCreate, tokens.Length);
						for (int j = 0; j < ar.Length; j++)
							ar.SetValue(Convert.ChangeType(tokens[j], typeToCreate), j);
						parameters[i] = ar;

					}
					else
					{
						parameters[i] = Convert.ChangeType(requestParam, typeToCreate);
					}
				}
			}

			//effettuo la chiamata via reflection
			Object ret = proxy.CallMethod(mi, parameters);

			/*per tutti i parametri che sono out o in-out, travaso il valore indietro nella function description*/
			context.Response.Output.WriteLine("{");
			bool needComma = false;
			if (ret != null)
			{
				WriteComma(context, ref needComma);
				context.Response.Write("\"ReturnValue\": ");
				WriteJSon(ret, context.Response.OutputStream);
			}
			for (int i = 0; i < methodParams.Length; i++)
			{
				ParameterInfo param = methodParams[i];
				if (param.Name == "HeaderInfo")
					continue;

				if (param.ParameterType.IsByRef)
				{
					WriteComma(context, ref needComma);
					context.Response.Write("\"");
					context.Response.Write(param.Name);
					context.Response.Write("\": ");
					WriteJSon(parameters[i], context.Response.OutputStream);
				}

			}
			context.Response.Output.Write(Environment.NewLine);
			context.Response.Output.Write("}\r\n");
		}

		//---------------------------------------------------------------------------
		private string[] Adjust(string[] ar)
		{
			for (int i = 0; i < ar.Length; i++)
				ar[i] = ar[i].Replace(",,", ",");
			return ar;
		}

		//---------------------------------------------------------------------------
		private void Query(HttpContext context, string authenticationToken, DynamicProxy proxy, MethodInfo mi)
		{
			ParameterInfo[] methodParams = mi.GetParameters();

			context.Response.Output.WriteLine("{");
			bool needComma = false;
			if (mi.ReturnParameter != null)
			{
				WriteComma(context, ref needComma);
				context.Response.Write("\"ReturnValue\": ");
				WriteJSon(mi.ReturnParameter.ParameterType.FullName, context.Response.OutputStream);
			}
			for (int i = 0; i < methodParams.Length; i++)
			{
				ParameterInfo param = methodParams[i];
				if (param.Name == "HeaderInfo")
					continue;

				WriteComma(context, ref needComma);
				context.Response.Write("\"");
				context.Response.Write(param.Name);
				context.Response.Write("\": ");
				WriteJSon(param.ParameterType.FullName, context.Response.OutputStream);

			}
			context.Response.Output.Write(Environment.NewLine);
			context.Response.Output.Write("}\r\n");
		}

		//---------------------------------------------------------------------------
		private void WriteJSon(object o, Stream s)
		{
			DataContractJsonSerializer json = new DataContractJsonSerializer(o.GetType());
			json.WriteObject(s, o);
		}
	}

	[Serializable]
	class ErrorInfo
	{
		public string Message;
		public string StackTrace;
	}
}
