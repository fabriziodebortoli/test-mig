using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Microarea.TaskBuilderNet.Licence.SalesModulesReader
{
        public class HandlerOff
	{
		Common common = null;
        string pname = null;
		public HandlerOff(string pname)
		{
            this.pname = pname;
            common = CommonFactory.GetCommon(pname);
        }
		//---------------------------------------------------------------------
		public string GetArticleStringByPath(string path)
		{ 
			if (path == string.Empty || path == null)
				return string.Empty;
			
			byte[] buffer = Helper.GetFileAsBytes(path);
			return GetArticleString(buffer);
		}

		//---------------------------------------------------------------------
		private string GetArticleString(byte[] buffer)
		{ 
			if (buffer == null || buffer.Length == 0 )
				return string.Empty;
			
			return GetArticle(buffer, common.InnerBytes, common.OuterBytes);
		}

		//-----------------------------------------------------------------------------
		private  string GetArticle(byte[]  buffer, string inner, string outer)
		{
			return HandleOff1(buffer, inner, outer);
		}

		//---------------------------------------------------------------------
		private static string HandleOff1(byte[] buffer, string key, string block)
		{
			byte[] bkey		= Algorithm.ToByteArray(key);
			byte[] ivbBlock	= Algorithm.ToByteArray(block);

			return HandleOff1(buffer, "DES", bkey, ivbBlock);
		}

		//---------------------------------------------------------------------
		private static string HandleOff1(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{	
			if (buffer == null || buffer.Length == 0 )
				return string.Empty;
			SymmetricAlgorithm cProvider = Algorithm.Init(algoritmName, key, ivBlock);
			StreamReader sr;
			try
			{
				cProvider.KeySize = key.Length * 8;
				cProvider.BlockSize = ivBlock.Length * 8;

				MemoryStream ms = new MemoryStream(buffer);
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cProvider.CreateDecryptor(key, ivBlock), 
					CryptoStreamMode.Read
					);
				sr = new StreamReader(cs, System.Text.Encoding.UTF8);
				return sr.ReadToEnd();
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				return string.Empty;
			}
		}

	}
}
