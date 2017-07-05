using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Tokens;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Microarea.AdminServer.Library
{
	//================================================================================
	public class SecurityManager
    {
		//--------------------------------------------------------------------------------
		public static OperationResult ValidateToken(string jwtTokenText, string secretKey)
		{
			OperationResult opRes = new OperationResult();

			if (String.IsNullOrEmpty(jwtTokenText))
			{
				opRes.Result = false;
				opRes.Code = (int)TokenReturnCodes.Invalid;
				opRes.Message = TokenReturnCodes.Invalid.ToString();
				return opRes;
			}

			string[] tokenParts = jwtTokenText.Split('.');

			if (tokenParts.Length != 3)
			{
				opRes.Result = false;
				opRes.Code = (int)TokenReturnCodes.Invalid;
				opRes.Message = TokenReturnCodes.Invalid.ToString();
				return opRes;
			}

			// decoding token header

			string base64Header = tokenParts[0];
			byte[] data = Convert.FromBase64String(base64Header);
			string decodedString = Encoding.Unicode.GetString(data);
			JWTTokenHeader jwtHeader = JsonConvert.DeserializeObject<JWTTokenHeader>(decodedString);

			// decoding token payload

			string base64Payload = tokenParts[1];
			data = Convert.FromBase64String(base64Payload);
			decodedString = Encoding.Unicode.GetString(data);
			BootstrapToken bootstrapToken = JsonConvert.DeserializeObject<BootstrapToken>(decodedString);

			// computing a signature, to match the one that is coming within the request

			string signatureToCheck = JWTToken.GetTokenSignature(secretKey, jwtHeader, bootstrapToken);

			// matching the two signatures, so we can detect if the token has been tampered

			bool signatureMatch = String.Compare(signatureToCheck, tokenParts[2], false) == 0;

			if (!signatureMatch)
			{
				opRes.Result = false;
				opRes.Code = (int)TokenReturnCodes.Suspected;
				opRes.Message = Strings.InvalidToken;
				return opRes;
			}

			// token verification passed

			opRes.Result = true;
			opRes.Code = (int)TokenReturnCodes.Valid;
			opRes.Message = Strings.ValidToken;

			return opRes;
		}

		/// <summary>
		/// Check information in AuthorizationHeader
		/// </summary>
		/// <param name="authenticationHeader"></param>
		/// <returns>OperationResult</returns>
		//-----------------------------------------------------------------------------	
		public static OperationResult ValidateAuthorization(string authenticationHeader, string secretKey)
		{
			//@@TODO stringhe
			if (String.IsNullOrEmpty(authenticationHeader))
				return new OperationResult(false, "Strings.MissingAuthHeader", (int)AppReturnCodes.AuthorizationHeaderMissing);

			AuthorizationInfo authInfo = null;

			try
			{
				authInfo = JsonConvert.DeserializeObject<AuthorizationInfo>(authenticationHeader);
			}
			catch (Exception e)
			{
				return new OperationResult(false, String.Format("Strings.ExceptionMessage", e.Message), (int)AppReturnCodes.AuthorizationHeaderMissing);
				//StatusCode = 500
			}

			if (authInfo == null)
				return new OperationResult(false, "Strings.InvalidAuthHeader", (int)AppReturnCodes.AuthorizationHeaderMissing);

			if (String.IsNullOrEmpty(authInfo.SecurityValue))
				return new OperationResult(false, "Strings.MissingToken", (int)AppReturnCodes.MissingToken);

			if (authInfo.IsJwtToken)
				return ValidateToken(authInfo.SecurityValue, secretKey);

			return new OperationResult(false, "string.Format(Strings.UnknownAuthType, authInfo.Type)", (int)AppReturnCodes.Undefined);
		}

	}
}
