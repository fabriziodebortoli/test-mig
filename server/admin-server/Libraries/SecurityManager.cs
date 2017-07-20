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
		public static OperationResult ValidateToken(string jwtTokenText, string secretKey, bool isCloudAdmin = false, bool isProvisioningAdmin = false)
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
			string decodedString = Encoding.UTF8.GetString(data);
			JWTTokenHeader jwtHeader = JsonConvert.DeserializeObject<JWTTokenHeader>(decodedString);

			// decoding token payload

			string base64Payload = tokenParts[1];
			data = Convert.FromBase64String(base64Payload);
			decodedString = Encoding.UTF8.GetString(data);
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

            // token verification passed, so we can assume this token is valid

            if(true)//(isCloudAdmin) Todo ilaria ruoli
			{
			//	if (!bootstrapToken.CloudAdmin)
				{
					opRes.Result = false;
					opRes.Code = (int)TokenReturnCodes.MissingCloudAdminRole;
					opRes.Message = Strings.MissingRole;
					return opRes;
				}
			}
            if (true)//(isProvisioningAdmin) Todo ilaria ruoli
			{
				//if (!bootstrapToken.ProvisioningAdmin)
				{
					opRes.Result = false;
					opRes.Code = (int)TokenReturnCodes.MissingProvisioningAdminRole;
					opRes.Message = Strings.MissingRole;
					return opRes;
				}
			}

			opRes.Result = true;
			opRes.Code = (int)TokenReturnCodes.Valid;
			opRes.Message = Strings.ValidToken;
			opRes.Content = bootstrapToken;
			return opRes;
		}

		/// <summary>
		/// Check information in AuthorizationHeader
		/// </summary>
		/// <param name="authenticationHeader"></param>
		/// <returns>OperationResult</returns>
		//-----------------------------------------------------------------------------	
		public static OperationResult ValidateAuthorization(string authenticationHeader, string secretKey, bool isCloudAdmin = false, bool isProvisioningAdmin = false)
		{
			if (String.IsNullOrEmpty(authenticationHeader))
				return new OperationResult(false, Strings.AuthorizationHeaderMissing, (int)AppReturnCodes.AuthorizationHeaderMissing);

			AuthorizationInfo authInfo = null;

			try
			{
				authInfo = JsonConvert.DeserializeObject<AuthorizationInfo>(authenticationHeader);
			}
			catch (Exception e)
			{
				return new OperationResult(false, String.Format(Strings.ExceptionMessage, e.Message), (int)AppReturnCodes.AuthorizationHeaderMissing);
				//StatusCode = 500
			}

			if (authInfo == null)
				return new OperationResult(false, Strings.InvalidAuthHeader, (int)AppReturnCodes.AuthorizationHeaderMissing);

			if (String.IsNullOrEmpty(authInfo.SecurityValue))
				return new OperationResult(false, Strings.MissingToken, (int)AppReturnCodes.MissingToken);

			if (authInfo.IsJwtToken)
				return ValidateToken(authInfo.SecurityValue, secretKey, isCloudAdmin, isProvisioningAdmin);

			return new OperationResult(false, string.Format(Strings.UnknownAuthType, authInfo.Type), (int)AppReturnCodes.Undefined);
		}
	}
}
