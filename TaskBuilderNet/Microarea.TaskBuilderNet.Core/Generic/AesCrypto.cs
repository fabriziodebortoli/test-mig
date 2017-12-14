using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Core.Generic
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

			//string output = UnicodeEncoding.Unicode.GetString(outBlock);
			return Convert.ToBase64String(outBlock);
		}

		//-----------------------------------------------------------------------
		public static string DecryptString(string inString)
		{
			var aesCSP = Aes.Create();

			aesCSP.Key = key;
			aesCSP.IV = iv;
			byte[] inBlock = Convert.FromBase64String(inString);
			ICryptoTransform xfrm = aesCSP.CreateDecryptor();
			byte[] outBlock = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);

			return UnicodeEncoding.Unicode.GetString(outBlock);
		}
	}
}

