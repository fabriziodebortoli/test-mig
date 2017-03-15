using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.WebServicesWrapper
{
	public class LoginManagerSessionManager
	{
		public static Dictionary<string, LoginManagerSession> LoginManagerSessionTable = new Dictionary<string, LoginManagerSession>();
	}
	
	//================================================================================================
	public class LoginManagerSession
	{
		LoginManagerState loginManagerState = LoginManagerState.UnInitialized;

		public LoginManagerSession()
		{
		}

		public LoginManagerSession(string authenticationToken)
		{
			this.AuthenticationToken = authenticationToken;
		}
	
		public void Init(string authenticationToken)
		{
			this.AuthenticationToken = authenticationToken;
		}

		public string AuthenticationToken { get; internal set; }
		public LoginManagerState LoginManagerState { get; internal set; }

		public string UserName { get; internal set; }
		public bool Admin { get; internal set; }
		public string CompanyName { get; internal set; }
		public string DbServer { get; internal set; }
		public string DbUser { get; internal set; }
		public string DbName { get; internal set; }
		public bool Security { get; internal set; }
		public bool Auditing { get; internal set; }
		public string Password { get; internal set; }
		public bool PasswordNeverExpired { get; internal set; }
		public DateTime ExpiredDatePassword { get; internal set; }
		public bool UserCannotChangePassword { get; internal set; }
		public bool ExpiredDateCannotChange { get; internal set; }
		public bool UserMustChangePassword { get; internal set; }
		public string ApplicationLanguage { get; internal set; }
		public string PreferredLanguage { get; internal set; }
	}

	//================================================================================================
	public class LoginManager
	{
		private string baseUrl = "http://localhost:5000/";
		//private string loginManagerUrl;
		private int webServicesTimeOut;

		public string ProviderName { get; internal set; }


		//-----------------------------------------------------------------------------------------
		public LoginManager()
		{
		}

		//-----------------------------------------------------------------------------------------
		public LoginManager(string loginManagerUrl, int webServicesTimeOut)
		{
			this.baseUrl = loginManagerUrl;
			this.webServicesTimeOut = webServicesTimeOut;
		}

		//-----------------------------------------------------------------------------------------
		private string PostData(string url, string body)
		{
			WebRequest request = (HttpWebRequest)WebRequest.Create(url);
			var data = Encoding.ASCII.GetBytes(body);

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			//request.ContentLength = data.Length;

			Task<Stream> stream = request.GetRequestStreamAsync();
			Stream s = stream.Result;
			s.Write(data, 0, data.Length);

			Task<WebResponse> response = request.GetResponseAsync();
			WebResponse r = response.Result;

			var responseString = new StreamReader(r.GetResponseStream()).ReadToEnd();
			return responseString;
		}

		//-----------------------------------------------------------------------------------------
		public string[] EnumCompanies(string userName)
		{
			var postData = "user=" + userName;
			var response = PostData(baseUrl + "account-manager/getCompanyForUser", postData);
			return new string[] { };
		}

		//-----------------------------------------------------------------------------------------
		public bool IsDeveloperActivation()
		{
			var response = PostData(baseUrl + "account-manager/IsDeveloperActivation", "");
			return true;
		}

		//-----------------------------------------------------------------------------------------
		public string GetConfigurationHash()
		{
			var response = PostData(baseUrl + "account-manager/GetConfigurationHash", "");
			return "";
		}

		//-----------------------------------------------------------------------------------------
		public void GetLoginInformation(string token)
		{
			var postData = "token=" + token;
			var response = PostData(baseUrl + "account-manager/GetLoginInformation", postData);
		}

		//-----------------------------------------------------------------------------------------
		public string GetUserInfo()
		{
			var response = PostData(baseUrl + "account-manager/GetUserInfo", "");
			return "";
		}

		//-----------------------------------------------------------------------------------------
		public string GetInstallationVersion()
		{
			var response = PostData(baseUrl + "account-manager/GetInstallationVersion", "");
			return "";
		}

		//-----------------------------------------------------------------------------------------
		public bool IsActivated(string application, string functionality)
		{
			var postData = "application=" + application + "&=functionality" + functionality;

			var response = PostData(baseUrl + "account-manager/IsActivated", postData);
			return true;
		}

		internal bool IsEasyBuilderDeveloper(string authenticationToken)
		{
			throw new NotImplementedException();
		}

		internal void IsAlive()
		{
			throw new NotImplementedException();
		}

		internal int ChangePassword(string newPassword)
		{
			throw new NotImplementedException();
		}

		internal void LogOff()
		{
			throw new NotImplementedException();
		}

		internal bool IsRegistered(out string message, out ActivationState dummy)
		{
			throw new NotImplementedException();
		}

		internal void Ping()
		{
			throw new NotImplementedException();
		}

		internal int ValidateUser(string username, string password, bool winNT)
		{
			throw new NotImplementedException();
		}

		internal int Login(string company, string processType, bool overwriteLogin)
		{
			throw new NotImplementedException();
		}

		internal void SSOLogOff(string cryptedtoken)
		{
			throw new NotImplementedException();
		}

		internal int LoginViaInfinityToken(string cryptedtoken, string username, string password, string company)
		{
			throw new NotImplementedException();
		}

		internal DBNetworkType GetDBNetworkType()
		{
			throw new NotImplementedException();
		}

		internal int GetUsagePercentageOnDBSize()
		{
			throw new NotImplementedException();
		}

		internal bool IsSecurityLightEnabled()
		{
			throw new NotImplementedException();
		}

		internal void RefreshSecurityStatus()
		{
			throw new NotImplementedException();
		}

		internal string[] GetModules()
		{
			throw new NotImplementedException();
		}

		internal bool CheckActivationExpression(string currentApplicationName, string activationExpression)
		{
			throw new NotImplementedException();
		}
	}
}
