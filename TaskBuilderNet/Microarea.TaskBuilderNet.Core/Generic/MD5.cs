using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class MD5Code
	{
		//-----------------------------------------------------------------------------
		public static string GetFileMD5Code(string file)
		{
			using (FileStream fs = new FileStream(file, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(fs);

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < retVal.Length; i++)
					sb.Append(retVal[i].ToString("x2"));

				return sb.ToString();
			}
		}
	}
}
