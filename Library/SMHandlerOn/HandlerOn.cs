using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Security.Cryptography;

using Microarea.Library.SMBaseHandler;
using Microarea.Library.WSLogger;

namespace Microarea.Library.SMHandlerOn
{
	//=========================================================================
	public class HandlerOn
	{
        Common common = null;
        public string ErrorMessage = String.Empty;
		private static WSCryptLogWriter handlerLogger;

		//---------------------------------------------------------------------
		public HandlerOn(WSCryptLogWriter logger,string pname)
		{
			handlerLogger = logger;
            common = CommonFactory.GetCommon(pname);
        }

		//---------------------------------------------------------------------
		public bool HandleOn(string sourceFileFullName, out string destFileFullName)
		{ 
			destFileFullName = String.Empty;
			if (sourceFileFullName == string.Empty	|| 
				sourceFileFullName == null			|| 
				!File.Exists(sourceFileFullName)) 
			{
				ErrorMessage = "Error during loading SalesModule." ;
				handlerLogger.WriteAndSendError("SMHandlerOn.HandlerOn.HandleOn", String.Format("Impossible to find file {0}", sourceFileFullName));
				return false;
			}
			try
			{
				if (Helper.HasExactExtension(sourceFileFullName))
				{
					ErrorMessage = "File extension not valid." ;
					handlerLogger.WriteAndSendError("SMHandlerOn.HandlerOn.HandleOn", String.Format("File {0} extension not valid.", sourceFileFullName));
					return false;
				}
				destFileFullName	= sourceFileFullName;
				destFileFullName	= Path.ChangeExtension(destFileFullName, Common.SMExtension);
				string content		= GetContentFromPath(sourceFileFullName);
				return HandleOnFromString(content, destFileFullName);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ErrorMessage = "Exception during SalesModule processing." ;
				handlerLogger.WriteAndSendError("SMHandlerOn.HandlerOn.HandleOn", String.Format("Exception during cryptring of file {0}.", sourceFileFullName), exc.Message);
				return false;
			}
		}
		
		//---------------------------------------------------------------------
		private static string HandleOn(byte[] buffer, string key, string block)
		{
			byte[] bkey		= Algorithm.ToByteArray(key);
			byte[] ivbBlock	= Algorithm.ToByteArray(block);
			return HandleOn(buffer, "DES", bkey, ivbBlock);
		}

		//---------------------------------------------------------------------
		private static string HandleOn(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{
			if (buffer == null || buffer.Length == 0 )
				return string.Empty;
			SymmetricAlgorithm cProvider = Algorithm.Init(algoritmName, key, ivBlock);
			MemoryStream ms = new MemoryStream();
			try
			{
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cProvider.CreateEncryptor(key, ivBlock), 
					CryptoStreamMode.Write
					);
				BinaryWriter bw = new BinaryWriter(cs);
				bw.Write(buffer);
				bw.Flush();
				cs.FlushFinalBlock();
				ms.Flush();
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				handlerLogger.WriteAndSendError("SMHandlerOn.HandlerOn.HandleOn", "Exception during cryptring procedure.", exc.Message);
				return string.Empty;
			}
			byte[] buf = new byte[256];
			buf = ms.GetBuffer();
			return Convert.ToBase64String(buf, 0, (int)ms.Length);
		}

		//---------------------------------------------------------------------
		private  string GetContentFromPath(string path)
		{ 
			if (path == string.Empty || path == null)
				return string.Empty;
			byte[] buffer = Helper.GetFileAsBytes(path);
			
			return GetContent(buffer);
		}

		//---------------------------------------------------------------------
		private  string GetContent(byte[] buffer)
		{ 
			if (buffer == null || buffer.Length == 0 )
				return string.Empty;
			return HandleOn(buffer, common.InnerBytes, common.OuterBytes);
		}

		//---------------------------------------------------------------------
		private  bool HandleOnFromString(string aValue, string destPath)
		{ 
			if (aValue == string.Empty || aValue == null)
				return false;
			try
			{
				byte[] buffer = Convert.FromBase64String(aValue);
				FileStream fs = new FileStream(destPath, FileMode.Create);
				BinaryWriter sw = new BinaryWriter(fs);
				sw.Write(buffer, 0, buffer.Length);
				fs.Close();
				sw.Close();
			}
			catch (Exception exc)
			{
				ErrorMessage = "Exception during SalesModule processing";
				handlerLogger.WriteAndSendError("SMHandlerOn.HandlerOn.HandleOnFromString", String.Format("Exception during saving crypted file : {0}.", destPath) , exc.Message);
				Debug.Fail(exc.Message);
				return false;
			}
			return true;
		}
	}
}
