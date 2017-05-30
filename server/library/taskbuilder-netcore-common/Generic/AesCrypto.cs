using System.Security.Cryptography;
using System.Text;

namespace Microarea.Common.Generic
{
	public class AesCrypto
	{
		private static byte[] key = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };
		private static byte[] iv = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };

		//-----------------------------------------------------------------------
		public static string EncryptString(string inString)
		{
			var aesCSP = Aes.Create();

			aesCSP.Key = key;
			aesCSP.IV = iv;
			byte[] inBlock = UnicodeEncoding.Unicode.GetBytes(inString);
			ICryptoTransform xfrm = aesCSP.CreateEncryptor();
			byte[] outBlock = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);
			return UnicodeEncoding.Unicode.GetString(outBlock);
		}

		//-----------------------------------------------------------------------
		public static string DecryptString(string inString)
		{
			var aesCSP = Aes.Create();

			aesCSP.Key = key;
			aesCSP.IV = iv;
			byte[] inBlock = UnicodeEncoding.Unicode.GetBytes(inString);
			ICryptoTransform xfrm = aesCSP.CreateDecryptor();
			byte[] outBlock = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);

			return UnicodeEncoding.Unicode.GetString(outBlock);
		}
	}
}
