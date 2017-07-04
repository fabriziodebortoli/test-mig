using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.AdminServer.Controllers.Controllers.Helpers
{
	//================================================================================
	public class BootstrapTokenContainer
	{
		const int defaultTokenDurationMinutes = 5;

		string jwtToken;
		string message;
		bool result;
		int resultCode;
		DateTime expirationDate;

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
			this.jwtToken = String.Empty;
		}

		public void SetResult(bool result, int resultCode, string message, BootstrapToken token = null, string secretKey = "")
		{
			this.result = result;
			this.resultCode = resultCode;
			this.message = message;

			this.expirationDate = result ? DateTime.Now.AddMinutes(defaultTokenDurationMinutes) : DateTime.MinValue;
			this.jwtToken = result ? GenerateJWTToken(token, secretKey) : String.Empty;
		}

		//----------------------------------------------------------------------
		private string GenerateJWTToken(BootstrapToken bootstrapToken, string secretKey)
		{
			JWTToken jwtToken = new JWTToken();
			JWTTokenHeader jWTTokenHeader = new JWTTokenHeader();
			jWTTokenHeader.alg = "HS256";
			jWTTokenHeader.typ = "JWT";
			jwtToken.header = jWTTokenHeader;
			jwtToken.payload = bootstrapToken;
			return jwtToken.GetToken(secretKey);
		}
	}

	//================================================================================
	public class JWTToken
	{
		string signature;

		public JWTTokenHeader header;
		public BootstrapToken payload;

		//--------------------------------------------------------------------------------
		public string GetToken(string secretKey)
		{
			try
			{
				this.signature = GetTokenSignature(secretKey, this.header, this.payload);
				return String.Concat(EncodeHeader(this.header), ".", EncodePayload(this.payload), ".", this.signature);
			}
			catch (Exception)
			{
				return String.Empty; // todo: add log
			}
		}

		//--------------------------------------------------------------------------------
		public static string GetTokenSignature(string secret, JWTTokenHeader tokenHeader, BootstrapToken payload)
		{
			string encodedString = EncodeHeader(tokenHeader) + "." + EncodePayload(payload);
			byte[] hashValue;

			using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secret)))
			{
				hashValue = hmac.ComputeHash(Encoding.ASCII.GetBytes(encodedString));
			}

			return Convert.ToBase64String(hashValue);
		}

		//--------------------------------------------------------------------------------
		static string EncodeHeader(JWTTokenHeader tokenHeader)
		{
			string header = JsonConvert.SerializeObject(tokenHeader);
			return EncodeToBase64(header);
		}

		//--------------------------------------------------------------------------------
		static string EncodePayload(BootstrapToken bootstrapToken)
		{
			string payload = JsonConvert.SerializeObject(bootstrapToken);
			return EncodeToBase64(payload);
		}

		//--------------------------------------------------------------------------------
		static string EncodeToBase64(string input)
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
		public bool CloudAdmin;
		public string PreferredLanguage;
		public string ApplicationLanguage;
		public UserTokens UserTokens;
		public Subscription[] Subscriptions;
		public List<ServerURL> Urls;

		//--------------------------------------------------------------------------------
		public BootstrapToken()
		{
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
