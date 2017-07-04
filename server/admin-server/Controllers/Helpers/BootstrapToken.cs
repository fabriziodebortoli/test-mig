using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microarea.AdminServer.Controllers;
using System.Text;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class BootstrapTokenContainer
	{
		string jwtToken;
		string message;
		bool result;
		int resultCode;
		DateTime expirationDate;
        // Durata del token in minuti.
        int defaultTokenDuration = 5;		
		public DateTime ExpirationDate { get => expirationDate; set => expirationDate = value; }
		public bool Result { get => result; set => result = value; }
		public string Message { get => message; set => message = value; }
		public int ResultCode { get => resultCode; set => resultCode = value; }
		public string JwtToken { get => jwtToken; set => jwtToken = value; }

        public BootstrapTokenContainer()
		{
			this.expirationDate = DateTime.MinValue;
			this.result = false;
			this.message = String.Empty;
			this.resultCode = -1;
		}

        //--------------------------------------------------------------------------------
        internal void SetError( int resultcode, string message)
        {
            Result = false;
            ResultCode = resultcode;
            Message = message;
        }

        //--------------------------------------------------------------------------------
        internal void SetSuccess( int resultcode, string message, BootstrapToken t)
        {
            Result = true; 
            ResultCode = resultcode;
            Message = message;
            ExpirationDate = DateTime.Now.AddMinutes(defaultTokenDuration);
            if (t != null) jwtToken = GenerateJWTToken(t);
        }

        //----------------------------------------------------------------------
        private string GenerateJWTToken(BootstrapToken bootstrapToken)
        {
            JWTToken jwtToken = new JWTToken();
            JWTTokenHeader jWTTokenHeader = new JWTTokenHeader();
            jWTTokenHeader.alg = "HS256";
            jWTTokenHeader.typ = "JWT";
            jwtToken.header = jWTTokenHeader;
            jwtToken.payload = bootstrapToken;
            return jwtToken.GetToken();
        }
    }

	//================================================================================
	public class JWTToken
	{
		string signature;

		public JWTTokenHeader header;
		public BootstrapToken payload;

		//--------------------------------------------------------------------------------
		public string GetToken()
		{
			try
			{ 
				this.SignToken("LeonardoDaVinci");
				return String.Concat(EncodeHeader(), ".", EncodePayload(), ".", this.signature);
			}
			catch (Exception)
			{
				return String.Empty; // todo: add log
			}
		}

		//--------------------------------------------------------------------------------
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

		//--------------------------------------------------------------------------------
		string EncodeHeader()
		{
			string header = JsonConvert.SerializeObject(this.header);
			return EncodeToBase64(header);
		}

		//--------------------------------------------------------------------------------
		string EncodePayload()
		{
			string payload = JsonConvert.SerializeObject(this.payload);
			return EncodeToBase64(payload);
		}

		//--------------------------------------------------------------------------------
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
        public List<ServerURL> Urls;
		
		//--------------------------------------------------------------------------------
		public BootstrapToken() {
			this.AccountName = String.Empty;
			this.ProvisioningAdmin = false;
			this.PreferredLanguage = String.Empty;
			this.ApplicationLanguage = String.Empty;
			this.UserTokens = new UserTokens();
			this.Subscriptions = new Subscription[] { };
			this.Urls = new List<ServerURL>();
		}
    }
    
}
