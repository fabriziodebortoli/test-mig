using System;

namespace Microarea.AdminServer.Services.Security
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
		public string JwtToken { get => jwtToken; }

        //----------------------------------------------------------------------
        public BootstrapTokenContainer()
		{
			this.expirationDate = DateTime.MinValue;
			this.result = false;
			this.message = String.Empty;
			this.resultCode = -1;
			this.jwtToken = String.Empty;
		}

        //----------------------------------------------------------------------
        public void SetToken(bool result, int resultCode, string message, BootstrapToken token = null, string secretKey = "")
		{
			this.result = result;
			this.resultCode = resultCode;
			this.message = message;
			this.expirationDate = result ? DateTime.UtcNow.AddMinutes(defaultTokenDurationMinutes) : DateTime.MinValue;
			this.jwtToken = result ? GenerateJWTToken(token, secretKey) : String.Empty;
		}

		//----------------------------------------------------------------------
		private string GenerateJWTToken(BootstrapToken bootstrapToken, string secretKey)
		{
			JWTToken jwtToken = new JWTToken();
			JWTTokenHeader jWTTokenHeader = new JWTTokenHeader();
			jWTTokenHeader.alg = "HS256";
			jWTTokenHeader.typ = AuthorizationInfo.TypeJwtName;
			jwtToken.header = jWTTokenHeader;
			jwtToken.payload = bootstrapToken;
			return jwtToken.GenerateEncodedToken(secretKey);
		}
	}
}
