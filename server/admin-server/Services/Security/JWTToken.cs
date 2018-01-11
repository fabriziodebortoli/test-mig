using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.AdminServer.Services.Security
{
	//================================================================================
	public class JWTToken
	{
		string signature;

		public JWTTokenHeader header;
		public BootstrapToken payload;

		//--------------------------------------------------------------------------------
		public string GenerateEncodedToken(string secretKey)
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
			byte[] encodedBytes = Encoding.UTF8.GetBytes(input);
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
}

