using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.Commons;
using Microarea.AdminServer.Controllers.Helpers.Tokens;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.GWAMCaller;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Libraries
{
	//================================================================================
	public class SecurityManager
    {
		//--------------------------------------------------------------------------------
		public static OperationResult ValidateToken(string jwtTokenText, string secretKey, string roleName, string entityKey, string level)
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

			bool roleHasBeenFound = false;

			foreach (IAccountRoles iAccountRole in bootstrapToken.Roles)
			{
				if (String.Compare(iAccountRole.RoleName, roleName, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					if (String.Compare(iAccountRole.EntityKey, entityKey, StringComparison.CurrentCultureIgnoreCase) == 0)
					{
						if (String.Compare(iAccountRole.Level, level, StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							roleHasBeenFound = true;
							break;
						}
					}
				}
			}

			if (!roleHasBeenFound)
			{
				opRes.Result = false;
				opRes.Code = (int)TokenReturnCodes.MissingRole;
				opRes.Message = Strings.MissingRole;
				return opRes;
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
		public static OperationResult ValidateAuthorization(
			string authenticationHeader, string secretKey, string roleName, string entityKey, string level)
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
				return ValidateToken(authInfo.SecurityValue, secretKey, roleName, entityKey, level);

			return new OperationResult(false, string.Format(Strings.UnknownAuthType, authInfo.Type), (int)AppReturnCodes.Undefined);
		}

		//-----------------------------------------------------------------------------	
		public static byte[] Get128BitSalt()
		{
			// generate a 128-bit salt using a secure PRNG
			byte[] salt = new byte[128 / 8];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}

			return salt;
		}

		//-----------------------------------------------------------------------------	
		public static string HashThis(string input, byte[] salt)
		{
			// derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: input, 
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 10000,
				numBytesRequested: 256 / 8));

			return hashed;
		}

        //-----------------------------------------------------------------------------	
        public static string HashThis(DateTime input)
        {
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input.ToUniversalTime().ToString("o"),
                salt: null,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }


        //-----------------------------------------------------------------------------	
        public static string GetRandomPassword()
		{
            //todo ripristina
			return "Microarea..."; // Guid.NewGuid().ToString();
        }

        #region Crypting /Decrypting
        //---------------------------------------------------------------------
        public static string EncryptString(string text)
        {
            if (text == null) return null;
            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(Encoding.UTF8.GetBytes(DirVal()), aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        var iv = aesAlg.IV;
                        var decryptedContent = msEncrypt.ToArray();
                        var result = new byte[iv.Length + decryptedContent.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        public static string DecryptString(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            try
            {
                var fullCipher = Convert.FromBase64String(text);
                var iv = new byte[16];
                var cipher = new byte[fullCipher.Length - iv.Length];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

                using (var aesAlg = Aes.Create())
                {
                    using (var decryptor = aesAlg.CreateDecryptor(Encoding.UTF8.GetBytes(DirVal()), iv))
                    {
                        string result;
                        using (var msDecrypt = new MemoryStream(cipher))
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (var srDecrypt = new StreamReader(csDecrypt))
                                {
                                    result = srDecrypt.ReadToEnd();
                                }
                            }
                        }
                        return result;
                    }
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// metodo dal nome vago che torna la chiave usata per crypt e decrypt sottoforma di arraydi byte, meno leggibilità per lieve offuscazione
        /// H8jKhZjkdM l7na*+t5vfL08bVt_suca
        /// </summary>;
        /// <returns></returns>
        //---------------------------------------------------------------------
        static string DirVal()
        {
            byte[] bytes = new byte[] { 72, 0, 56, 0, 106, 0, 75, 0, 104, 0, 90, 0, 106, 0, 107, 0, 100, 0, 77, 0, 32, 0, 108, 0, 55, 0, 110, 0, 97, 0, 42, 0, 43, 0, 116, 0, 53, 0, 118, 0, 102, 0, 76, 0, 48, 0, 56, 0, 98, 0, 86, 0, 116, 0, 95, 0, 115, 0, 117, 0, 99, 0, 97, 0 };
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        #endregion

        //---------------------------------------------------------------------
        public static DateTime GetDateFromCryptedString(string encrypted)
        {  
            //decrypt della stringa contenuta nel db e parse a datetime
            string s = DecryptString(encrypted);
            if (s == null)
                return DateTime.MinValue;
            DateTime t = DateTime.MinValue;
            DateTime.TryParse(s, out t);
            return t.ToUniversalTime();
        }

        //---------------------------------------------------------------------
        public static int GetDateHashing(DateTime t)
        {
            return t.ToUniversalTime().GetHashCode();
        }

        //---------------------------------------------------------------------
        public static string GetCryptedStringFromDate(DateTime t)
        {
           return EncryptString(t.ToUniversalTime().ToString("s"));
        }

		//---------------------------------------------------------------------
		public async static Task<string> ValidatePermission(string authHeader, IHttpHelper httpHelper, string gwamURL)
		{
			Task<string> responseData = await GwamCaller.ValidateGWAMToken(authHeader, httpHelper, gwamURL);
			return responseData.Result;
		}
	}
}
