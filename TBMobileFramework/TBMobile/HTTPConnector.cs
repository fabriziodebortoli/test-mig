using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TBMobile
{
	public static class ConnectionSettings
	{
		//10.0.2.2 is the equivalent of localhost when user in emulator
		//test.microarea.eu is Microarea test server
		private static string server = "localhost";
		private static string installation = "development";
		private static string baseUrl;
		private static string baseRestAPIUrl;
		private static string user = "sa";
		private static string company = "MagoWebLook";
		private static string password = "";

		public static string User
		{
			get { return ConnectionSettings.user; }
			set { ConnectionSettings.user = value; }
		}
		
		public static string Company
		{
			get { return ConnectionSettings.company; }
			set { ConnectionSettings.company = value; }
		}
		
		public static string Password
		{
			get { return ConnectionSettings.password; }
			set { ConnectionSettings.password = value; }
		}

		public static string Server {
			get{ return server;}
			set { server = value; baseUrl = baseRestAPIUrl = null;}
		}
		public static string Installation {
			get{ return installation; }
			set { installation = value; baseUrl = baseRestAPIUrl = null;}
		}
		internal static string BaseUrl
		{
			get 
			{
				if (baseUrl == null) 
					baseUrl = string.Concat ("http://", server, "/", installation, "/easylook/tbloader/"); 
				return baseUrl;
			}
		}
		internal static string BaseRestAPIUrl
		{
			get 
			{
				if (baseRestAPIUrl == null) 
					baseRestAPIUrl = baseUrl + "rest-api/";
				return baseRestAPIUrl;
			}
		}
	}
	internal class ResponseEventArgs : EventArgs
	{
		public JObject ResponseObject { get; set; }
		public string Cookies { get; set; }
		public bool Success { get { object o = ResponseObject["success"]; return o == null ? false : string.Compare("true", o.ToString(), StringComparison.OrdinalIgnoreCase) == 0; } }
		public string Message { get { object o = ResponseObject["message"]; return o == null ? "" : o.ToString(); } }
		public string ResponseText { get; set;}

		public int Code { get { int code = 0; Int32.TryParse((string)ResponseObject["code"], out code); return code; } }
	}
	internal delegate void OnResponseReady(ResponseEventArgs args);
	internal class HTTPConnector
	{
		private class RequestState
		{
			public OnResponseReady onReady;
			public HttpWebRequest req;
			public byte[] postBytes;
			public RequestState(HttpWebRequest req, OnResponseReady onReady, byte[] postBytes)
			{
				this.req = req;
				this.onReady = onReady;
				this.postBytes = postBytes;
			}
		}


		private string cookies = "";
		public HTTPConnector()
		{
			
		}
		/// <summary>
		/// Send an asynchronous request, returns the request id, that can be used in the Response event to match the right server response.
		/// Time consuming operations, such as internet calls, has to be done asynchronously to avoid blocking the main thread
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="parms"></param>
		/// <returns></returns>
		private void SendRequest(string uri, OnResponseReady onReady, params string[] parms)
		{
			try
			{
				StringBuilder sb = new StringBuilder ();
				for (int i = 1; i < parms.Length; i+=2)
				{
					if (sb.Length > 0)
						sb.Append ('&');
					sb.Append (System.Net.WebUtility.UrlEncode(parms [i - 1]));
					sb.Append ('=');
					sb.Append(System.Net.WebUtility.UrlEncode(parms[i]));
				}
				string postdata = sb.ToString();

				byte[] postBytes = Encoding.UTF8.GetBytes(postdata);
				HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
				req.ContentType = "application/x-www-form-urlencoded";
				req.Headers[HttpRequestHeader.Cookie] = cookies;
				req.Method = "POST";
				RequestState state = new RequestState(req, onReady, postBytes);
				Debug.WriteLine("REQUEST * " + postdata);
				req.BeginGetRequestStream (OnGetRequestStream, state);
			}
			catch (Exception ex) 
			{
				if (onReady != null) {
					ResponseEventArgs args = new ResponseEventArgs
					{
						ResponseObject = new JObject { new JProperty("success", false), new JProperty("message", ex.Message) },
						ResponseText = ex.Message
					};
					onReady (args);
				}
			}

		}
		/// <summary>
		/// called when request stream is ready
		/// </summary>
		/// <param name="obj"></param>
		private void OnGetRequestStream(IAsyncResult obj)
		{
			RequestState state = (RequestState)obj.AsyncState;
			using (Stream postStream = state.req.EndGetRequestStream (obj))
			{
				postStream.Write(state.postBytes, 0, state.postBytes.Length);
				postStream.Flush();
			}
			state.req.BeginGetResponse(OnGetResponse, state);
		}
		/// <summary>
		/// called when response is ready
		/// </summary>
		/// <param name="obj"></param>
		private void OnGetResponse(IAsyncResult obj)
		{
			RequestState state = (RequestState)obj.AsyncState;
			if (state.onReady != null)
			{
				string responseText = "";
				JObject responseObj;
				string c = "";
				try
				{
					WebResponse resp = state.req.EndGetResponse(obj);
					using (Stream s = resp.GetResponseStream())
					{
						using (StreamReader sr = new StreamReader(s))
						{
							responseText = sr.ReadToEnd();
							Debug.WriteLine("RESPONSE * " + responseText);
				
							c = resp.Headers["Set-Cookie"];
						}
					}
				}
				catch (Exception ex)
				{
					responseText = ex.Message;
				}
				try
				{
					responseObj = JObject.Parse(responseText);
				}
				catch (Exception ex) 
				{
					responseObj = new JObject { new JProperty("success", false), new JProperty("message", ex.Message) };
				}
				ResponseEventArgs args = new ResponseEventArgs
				{
					ResponseObject = responseObj,
					Cookies = c,
					ResponseText = responseText
				};
				
				
				if (!string.IsNullOrEmpty(args.Cookies))
					cookies = args.Cookies;

				state.onReady(args);
			}
		
			
		}
		/// <summary>
		/// Performs login asynchronously
		/// </summary>
		/// <param name="user"></param>
		/// <param name="company"></param>
		/// <param name="pwd"></param>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		internal void Login(string user, string company, string pwd, bool overwrite, OnResponseReady onReady = null)
		{ 
			string uri = ConnectionSettings.BaseUrl + "doLogin/?";

			SendRequest(uri, onReady, "user", user, "password", pwd, "company", company, "overwrite", overwrite ? "true" : "false");
		}
		/// <summary>
		/// performs logoff asynchronously
		/// </summary>
		/// <returns></returns>
		internal void Logoff(OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseUrl + "doLogoff/";

			SendRequest(uri, onReady);
		}
		/// <summary>
		/// opens a document defined server side, identified by its namespace
		/// </summary>
		/// <param name="ns"></param>
		/// <param name="docSession"></param>
		/// <returns></returns>
		internal void OpenDocument(string ns, string docSession, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseUrl + "runDocument/";

			SendRequest(uri, onReady, "session", docSession);
		}
		/// <summary>
		/// opens a document defined client side with a JSON grammar and dinamically created server side
		/// </summary>
		/// <param name="documentStructure"></param>
		/// <param name="docSession"></param>
		/// <returns></returns>
		internal void OpenDynamicDocument(string documentStructure, string docSession, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseRestAPIUrl + "runJSONDocument/";

			SendRequest(uri, onReady, "session", docSession, "jsonData", documentStructure);
		}

		internal void CloseDocument(string docSession, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseUrl + "closeDocument/";
			SendRequest(uri, onReady, "session", docSession);
		}
		internal void BrowseRecord(string docSession, string jsonKey, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseRestAPIUrl + "browseRecord/";
			SendRequest(uri, onReady, "session", docSession, "jsonKey", jsonKey);
		}
		internal void GetData(string docSession, string dbtName, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseRestAPIUrl + "getData/";
			SendRequest(uri, onReady, "session", docSession, "dbtName", dbtName, "includeSlaves", "true");
		}
		internal void SetData(string docSession, string jsonData, OnResponseReady onReady = null)
		{
			string uri = ConnectionSettings.BaseRestAPIUrl + "setData/";
			SendRequest(uri, onReady, "session", docSession, "jsonData", jsonData);
		}
		internal void GetMessages(string docSession, bool clear, OnResponseReady onReady)
		{
			string uri = ConnectionSettings.BaseRestAPIUrl + "getMessages/";
			SendRequest(uri, onReady, "session", docSession, "clear", clear ? "true": "false");
		}
		/// <summary>
		/// posts a command, identified by verb, to the document identified by docSession
		/// </summary>
		/// <param name="docSession"></param>
		/// <param name="verb"></param>
		/// <returns></returns>
		internal void PostCommand(string docSession, string verb, OnResponseReady onReady = null)
		{	
			string uri = ConnectionSettings.BaseRestAPIUrl + verb + "/";

			SendRequest(uri, onReady, "session", docSession);

		}


		
	}


}
