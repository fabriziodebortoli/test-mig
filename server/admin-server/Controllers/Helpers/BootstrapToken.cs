using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using System;
using System.Collections.Generic;
using Newtonsoft;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class BootstrapTokenContainer
	{
		BootstrapToken bootstrapToken;
		string jwtToken;
		string message;
		bool result;
		int resultCode;
		DateTime expirationDate;

		public BootstrapToken PlainToken { get => bootstrapToken; set => bootstrapToken = value; }
		public DateTime ExpirationDate { get => expirationDate; set => expirationDate = value; }
		public bool Result { get => result; set => result = value; }
		public string Message { get => message; set => message = value; }
		public int ResultCode { get => resultCode; set => resultCode = value; }
		public string JwtToken { get => jwtToken; set => jwtToken = value; }

		public BootstrapTokenContainer()
		{
			this.bootstrapToken = new BootstrapToken();
			this.expirationDate = DateTime.MinValue;
			this.result = false;
			this.message = String.Empty;
			this.resultCode = -1;
		}
	}

	//================================================================================
	public class JWTToken
	{
		string signature;

		public JWTTokenHeader header;
		public BootstrapToken payload;

		public string GetToken()
		{
			this.SignToken("LeonardoDaVinci");

			return String.Concat(
				EncodeHeader(),
				".",
				EncodePayload(),
				".",
				this.signature
				);
		}

		void SignToken(string secret)
		{
			string encodedString = EncodeHeader() + "." + EncodePayload();
			byte[] hashValue;

			using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secret)))
			{
				hashValue = hmac.ComputeHash(Encoding.ASCII.GetBytes(encodedString));
			}

			this.signature = System.Text.Encoding.UTF8.GetString(hashValue);
		}

		string EncodeHeader()
		{
			string header = JsonConvert.SerializeObject(this.header);
			return EncodeToBase64(header);
		}

		string EncodePayload()
		{
			string payload = JsonConvert.SerializeObject(this.payload);
			return EncodeToBase64(payload);
		}

		string EncodeToBase64(string input)
		{
			byte[] encodedBytes = System.Text.Encoding.Unicode.GetBytes(input);
			return Convert.ToBase64String(encodedBytes);
		}
	}

	//================================================================================
	public class JWTTokenHeader
	{
		public string typ;
		public string alg;

		//--------------------------------------------------------------------------------
		public JWTTokenHeader()
		{
			this.typ = String.Empty;
			this.alg = String.Empty;
		}
	}

	//================================================================================
	public class BootstrapToken
    {
        public string AccountName;
        public bool ProvisioningAdmin;
        public string PreferredLanguage;
        public string ApplicationLanguage;
        public UserTokens UserTokens;
        public Subscription[] Subscriptions;
        public List<string> Urls;
		
		//--------------------------------------------------------------------------------
		public BootstrapToken() {
			this.AccountName = String.Empty;
			this.ProvisioningAdmin = false;
			this.PreferredLanguage = String.Empty;
			this.ApplicationLanguage = String.Empty;
			this.UserTokens = new UserTokens();
			this.Subscriptions = new Subscription[] { };
			this.Urls = new List<string>();
		}
    }
    
}
