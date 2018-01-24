using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microarea.Common.WebServicesWrapper;
using Microsoft.AspNetCore.Http;

namespace Microarea.Common.Applications
{
	//=========================================================================
	public class UserInfo
	{
		//private const string			SessionKey				= "UserInfoKey";

		public const string AuthenticationTokenKey = "authtoken";
		public string AuthenticationToken = string.Empty;

		public string Company = string.Empty;
		//public int						CompanyId				= -1;
		public CultureInfo CompanyCulture = CultureInfo.InvariantCulture;
		public bool UseUnicode = false;
		public string CompanyDbConnection = string.Empty;
		public string Provider = string.Empty;
		public TaskBuilderNetCore.Data.DBMSType DatabaseType = TaskBuilderNetCore.Data.DBMSType.SQLSERVER;

		public string User = string.Empty;
		public string ImpersonatedUser = string.Empty;
		//public int						LoginId					= -1;
		public string Password = string.Empty;
		public bool Admin = false;
		public CultureInfo UserUICulture = CultureInfo.InvariantCulture;
		public CultureInfo UserCulture = CultureInfo.InvariantCulture;

		public bool Valid = true;
		public string ErrorExplain = string.Empty;
		public int ErrorCode = 0;

		public bool OverwriteLogin = false;
		public LoginSlotType CalType = LoginSlotType.Invalid;
		public ActivationState ActivationState = ActivationState.Undefined;

		//---------------------------------------------------------------------
		public UserInfo(LoginInfoMessage msg, string sAuthT)
		{
			this.AuthenticationToken = sAuthT;

			this.ImpersonatedUser = this.User = msg.userName;
			this.Company = msg.companyName;
			this.Admin = msg.admin;
			this.CompanyDbConnection = AdjustConnectionString(msg.connectionString);
			this.Provider = msg.providerName;
			this.UseUnicode = msg.useUnicode;
			this.UserUICulture = new CultureInfo(msg.preferredLanguage);
			this.UserCulture = new CultureInfo(msg.applicationLanguage);
		}

		//---------------------------------------------------------------------
		//modifica la stringa di connessione in modo da farle usare il connection pool
		static private string AdjustConnectionString(string connectionString)
		{
			string pattern = @"Pooling\s*=\s*false";
			return Regex.Replace(connectionString, pattern, "Pooling=true", RegexOptions.IgnoreCase);
		}

		//---------------------------------------------------------------------
		public void SetCulture()
		{
			DictionaryFunctions.SetCultureInfo
				(
					InstallationData.ServerConnectionInfo.PreferredLanguage,
					InstallationData.ServerConnectionInfo.ApplicationLanguage
				);
		}

		public void SetCulture(string preferredLanguage, string applicationLanguage)
		{
			UserUICulture = new CultureInfo(preferredLanguage);
			UserCulture = new CultureInfo(applicationLanguage);

			DictionaryFunctions.SetCultureInfo
			    (
			        preferredLanguage,
			        applicationLanguage
			    );
		}

		//---------------------------------------------------------------------
		public bool IsDemo
		{
			get
			{
				return (ActivationState == ActivationState.Demo || ActivationState == ActivationState.DemoWarning);
			}
		}

		public string UserInfoLicensee { get { return "Microarea SpA - Zucchetti"; } }

		//----------- TODO RSWEB
		public string GetEdition() { return "Professional"; }
		public string GetCountry() { return "IT"; }

		public string GetUserDescriptionById(int id)
		{
			throw new NotImplementedException();
		}

		internal object GetUserDescriptionByName(string sid)
		{
			throw new NotImplementedException();
		}

		internal object GetInstallationVersion()
		{
			throw new NotImplementedException();
		}

		public bool IsAuthenticated()
		{
			return this.AuthenticationToken != string.Empty;
		}

	    public ISecurity NewSecurity(string company, string user, bool applySecurityFilter)
		{
			throw new NotImplementedException();
		}

		public bool IsActivated(string v1, string v2)
		{
			return true;
		}

		public bool IsValidToken(string authenticationToken)
		{
			return this.AuthenticationToken == authenticationToken;
		}
	}

	/// ================================================================================
	public class LoginInfoMessage
	{
		public string userName { get; set; }
		public string companyName { get; set; }
		public bool admin { get; set; }
		public string connectionString { get; set; }
		public string providerName { get; set; }
		public bool useUnicode { get; set; }
		public string preferredLanguage { get; set; }
		public string applicationLanguage { get; set; }

		public static LoginInfoMessage GetLoginInformation(Microsoft.AspNetCore.Http.ISession session, string authtoken, string baseAddress = "http://localhost:5000/")
		{
			//prima era GetRemoteLoginInformation, ora unificata temporaneamente per togliere la conoscenza del base address di accountmanager
			string loginInfo = session != null ? session.GetString(authtoken) : string.Empty;
			if (loginInfo.IsNullOrEmpty())
			{
				loginInfo = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.GetJsonLoginInformation(authtoken);

				if (session != null && !loginInfo.IsNullOrEmpty())
					session.SetString(authtoken, loginInfo);
			}

			LoginInfoMessage msg = JsonConvert.DeserializeObject<LoginInfoMessage>(loginInfo);
			return msg;
		}

		public static LoginInfoMessage GetLoginInformation(string authtoken, string baseAddress = "http://localhost:5000/")
		{
			//prima era GetRemoteLoginInformation, ora unificata temporaneamente per togliere la conoscenza del base address di accountmanager
			string loginInfo = Microarea.Common.WebServicesWrapper.LoginManager.LoginManagerInstance.GetJsonLoginInformation(authtoken);
			if (loginInfo.IsNullOrEmpty())
				return null;

			LoginInfoMessage msg = JsonConvert.DeserializeObject<LoginInfoMessage>(loginInfo);
			return msg;
		}

		public static async Task<string> GetRemoteLoginInformation(string authtoken, string baseAddress = "http://localhost:5000/")
		{
			using (var client = new HttpClient())
			{
				try
				{
					client.BaseAddress = new Uri(baseAddress);

					var content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>(UserInfo.AuthenticationTokenKey, authtoken)
					});

					var response = await client.PostAsync("account-manager/getLoginInformation/", content);
					response.EnsureSuccessStatusCode(); // Throw in not success

					var stringResponse = await response.Content.ReadAsStringAsync();
					return stringResponse;
				}
				catch (HttpRequestException e)
				{
					Console.WriteLine($"Request exception: {e.Message}");
					return null;
				}
			}
		}
	}

}
