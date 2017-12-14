using System;
using System.Security.Cryptography;

//-----------------------------------------------------------------------------
namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Classe per il crypting attraverso l'algoritmo RSA
	/// </summary>
	//-----------------------------------------------------------------------------
	public class RSACrypto
	{
		/// <summary>
		/// Crypta un array di byte
		/// </summary>
		/// <param name="DataToEncrypt">array di byte da cryptare</param>
		/// <param name="RSAKeyInfo">chiave di crypt</param>
		/// <param name="DoOAEPPadding"></param>
		/// <returns>l'array di byte cryptato</returns>
		//-----------------------------------------------------------------------------
		public static  byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
		{
			try
			{    
				//Create a new instance of RSACryptoServiceProvider.
				RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

				//Import the RSA Key information. This only needs
				//toinclude the public key information.
				RSA.ImportParameters(RSAKeyInfo);

				//Encrypt the passed byte array and specify OAEP padding.  
				//OAEP padding is only available on Microsoft Windows XP or
				//later.  
				return RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
			}
				//Catch and display a CryptographicException  
				//to the console.
			catch(CryptographicException e)
			{
				Console.WriteLine(e.Message);

				return null;
			}

		}

		/// <summary>
		/// Decrypta un array di byte
		/// </summary>
		/// <param name="DataToDecrypt">array di byte da decryptare</param>
		/// <param name="RSAKeyInfo">chiave di crypt</param>
		/// <param name="DoOAEPPadding"></param>
		/// <returns>l'array di byte decryptato</returns>
		//-----------------------------------------------------------------------------
		static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo,bool DoOAEPPadding)
		{
			try
			{
				//Create a new instance of RSACryptoServiceProvider.
				RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

				//Import the RSA Key information. This needs
				//to include the private key information.
				RSA.ImportParameters(RSAKeyInfo);

				//Decrypt the passed byte array and specify OAEP padding.  
				//OAEP padding is only available on Microsoft Windows XP or
				//later.  
				return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
			}
				//Catch and display a CryptographicException  
				//to the console.
			catch(CryptographicException e)
			{
				Console.WriteLine(e.ToString());

				return null;
			}
		}
	}
}
