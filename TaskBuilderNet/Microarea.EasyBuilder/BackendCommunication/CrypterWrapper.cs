using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.SalesModulesReader;
using Microarea.TaskBuilderNet.ParametersManager;

namespace Microarea.EasyBuilder.BackendCommunication
{
	/// <summary>
	/// CrypterWrapper.
	/// </summary>
	//=========================================================================
	internal class CrypterWrapper : BaseWsWrapper, IDisposable
	{
		private CrypterRef.Crypter30 crypter;

		//---------------------------------------------------------------------
		public CrypterWrapper
			(
			ProxySettings	proxySettings,
			string			aCrypterUrl
			) : base(proxySettings)
		{
			string crypterUrl = (aCrypterUrl != null && aCrypterUrl.Length > 0) ? aCrypterUrl.Trim() : String.Empty;
			try
			{
				if (crypterUrl == null || crypterUrl.Length == 0)
				{
					string cfgCrypterUrl = Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30;
					if (cfgCrypterUrl != null && cfgCrypterUrl.Trim().Length > 0)
						crypterUrl = cfgCrypterUrl.Trim();
				}
			}
			catch(Exception exception)
			{
				Debug.Fail("CrypterWrapper constructor error: " + exception.Message);
			}
			
			if (crypterUrl == null || crypterUrl.Length == 0)
			{
				crypterUrl = @"http://www.microarea.it/Crypter30/Crypter30.asmx";
			}
			crypter = new CrypterRef.Crypter30();
			crypter.Url = crypterUrl;
		}

		//---------------------------------------------------------------------
		public CrypterWrapper(ProxySettings proxySettings) : this(proxySettings, String.Empty)
		{
		}

		//---------------------------------------------------------------------
		public string CrypterUrl
		{
			get { return (crypter != null) ? crypter.Url : String.Empty; }
		}
		
		////---------------------------------------------------------------------
		//public bool Login(
		//    string password,
		//    string login,
		//    out IntegratedSolution[] solutions,
		//    out string companyCode
		//    )
		//{
		//    solutions = null;
		//    companyCode = string.Empty;
		//    // N.B.: it is not surrounded by a try/catch block to test the connection
		//    //SetupWebServiceCall(this.crypter);

		//    return crypter.Login(
		//        password,
		//        login,
		//        out solutions,
		//        out companyCode
		//        );
		//}

		//---------------------------------------------------------------------
		public bool CryptSalesModules(string password, string login, string solutionName, out string msg) 
		{
			msg = null;
			try
			{
				//SetupWebServiceCall(this.crypter);
			}
			catch (Exception exc)
			{
				// TODO messagio errore.
				Debug.WriteLine(exc.ToString());
				return false;
			}

            CrypterBag cb = null;
            string val = null;
            MessagesInfoWriter msgInfos = new MessagesInfoWriter();
            bool cryptOk = false;
            string sessionCode = "h725";
            string sessionID = Guid.NewGuid().ToString();
            IParametersManagerFactory factory = new ParametersManagerFactory();

            try
			{

                string[] fileNames = CommonFunctions.GetSalesModulesPaths(solutionName);
				if (fileNames == null || fileNames.Length == 0)
					return false;
                List<Attachment> files = new List<Attachment>() { };
				foreach (string filename in fileNames)
				{
                    Attachment att = new Attachment();
                    FileStream fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    att.Content = buffer;
                    att.Id = filename;
                    files.Add(att);

                }

                cb = new CrypterBag(login, password, solutionName, null);
                cb.Attachments = files;
                string resultString = string.Empty;
                cb.GetXmlString(out resultString, false);

                val = factory.GetParametersManager(CrypterBag.ProtocolVersion).SetParameter(
                false,
                resultString,
                sessionID,
                sessionCode
                );
                cryptOk = crypter.ProcessModule(ref val, ref sessionID, ref sessionCode);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                msgInfos.AddMessage(new MessageInfo(MessagesCode.GenericServerError, exc.Message));
                msg = msgInfos.ToString();
                return false;
            }
            string resultData = factory.GetParametersManager(CrypterBag.ProtocolVersion).GetParameter(
                  false,
                  val,
                  sessionID,
                  sessionCode);
            CrypterBag newcb = (CrypterBag)(CrypterBag.GetFromXmlString(resultData, typeof(CrypterBag)));
            msgInfos.AddMessage(newcb.Messages);
            if (!cryptOk)
            {
                msg = msgInfos.ToString();
                return false;
            }
            foreach (Attachment att in newcb.Attachments)
            {
                try
                {
                    byte[] buffer = att.Content;
                    FileStream fs = new FileStream(att.Id, FileMode.Create);
                    BinaryWriter sw = new BinaryWriter(fs);
                    sw.Write(buffer, 0, buffer.Length);
                    fs.Close();
                    sw.Close();
                }
                catch (Exception exc)
                {
                    msgInfos.AddMessage(new MessageInfo(MessagesCode.GenericServerError, exc.Message));
                    msg = msgInfos.ToString();
                    return false;
                }
            }
            return true;
		}

		////---------------------------------------------------------------------
		//public bool RegisterIntegratedSolution(string password, string login, string solutionName, string description, string webServiceUrl) 
		//{
		//    try
		//    {
		//        //SetupWebServiceCall(this.crypter);
		//    }
		//    catch (Exception exc)
		//    {
		//        // TODO messagio errore.
		//        Debug.WriteLine(exc.ToString());
		//        return false;
		//    }
		//    try
		//    {
		//        return crypter.RegisterIntegratedSolution(password, login, solutionName, description, webServiceUrl);
		//    }
		//    catch (Exception exc)
		//    {
		//        // TODO messaggio errore.
		//        Debug.WriteLine(exc.ToString());
		//        return false;
		//    }
		//}
    
        //---------------------------------------------------------------------
        public bool IntegratedSolutionExists(string solutionName, out bool bExists)
        {
            bExists = false;
            try
            {
                //SetupWebServiceCall(this.crypter);
            }
            catch (Exception exc)
            {
                // TODO messagio errore.
                Debug.WriteLine(exc.ToString());
                return false;
            }
            try
            {
                bExists = crypter.IntegratedSolutionExists(solutionName);
                return true;
            }
            catch (Exception exc)
            {
                // TODO messaggio errore.
                Debug.WriteLine(exc.ToString());
                return false;
            }
        }

		//---------------------------------------------------------------------
		public static bool TestCredentials(string userName, string password)
		{
			try
			{
				CrypterRef.Crypter30 loginService = new CrypterRef.Crypter30();

				return loginService.Login(password, userName);
			}
			catch
			{
				return false;
			}
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (crypter != null)
				{
					crypter.Dispose();
					crypter = null;
				}
			}
		}

		#endregion
	}
}
