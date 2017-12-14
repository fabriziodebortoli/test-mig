using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.IO;
using System.Globalization;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.WebServices.TbServices
{
	//==================================================================================
	[WebService(Namespace="http://microarea.it/TbServices/")]
	public class TbServices : System.Web.Services.WebService
	{
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		//---------------------------------------------------------------------------
		[WebMethod]
		public void Init()
		{
			TbServicesApplication.TbServicesEngine.Init();
		} 

		//---------------------------------------------------------------------------
		/// <summary>
		/// Chiude la corrente sessione di lavoro (login)
		/// </summary>
		[WebMethod]
		public void CloseTB(string authenticationToken)
		{
			TbServicesApplication.TbServicesEngine.CloseLogin(authenticationToken);
		}

		/// <summary>
		/// Istanzia un tb per poter eseguire una funzione esterna
		/// </summary>
		/// <remarks>Se un tb con la stessa company e data di applicazionee' gia' stato istanziato usa quello, altrimenti crea un nuovo thread di login</remarks>
		/// <param name="authenticationToken">Token di autenticazione dell'utente logginato in easy look</param>
		/// <param name="applicationDate">data di applicazione necessaria</param>
		/// <param name="easyToken">token dell'utente system del tb istanziato</param>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		[WebMethod(Description = "")]
		public int CreateTB(string authenticationToken, DateTime applicationDate, bool checkDate, out string easyToken)
		{
			easyToken = authenticationToken;
			string user = string.Empty;
			return TbServicesApplication.TbServicesEngine.CreateTB
				(
				authenticationToken, 
				applicationDate, 
				checkDate,
				out user
				);
		}

		/// <summary>
		/// Istanzia un tb per poter eseguire una funzione esterna
		/// </summary>
		/// <remarks>Se un tb con la stessa company e data di applicazionee' gia' stato istanziato usa quello, altrimenti crea un nuovo thread di login</remarks>
		/// <param name="authenticationToken">Token di autenticazione dell'utente logginato in easy look</param>
		/// <param name="applicationDate">data di applicazione necessaria</param>
		/// <param name="easyToken">token dell'utente system del tb istanziato</param>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		[WebMethod(Description = "")]
		public int CreateTBEx(string authenticationToken, DateTime applicationDate, bool checkDate, out string easyToken, out string server)
		{
			easyToken = authenticationToken;
			string user = string.Empty;
			return TbServicesApplication.TbServicesEngine.CreateTB
				(
				authenticationToken,
				applicationDate,
				checkDate,
				true,
				out user,
				out server
				);
		}

		/// <summary>
		/// Ritorna il tipo di binding impostato genericamente nel web config dell'applicazione TbServices
		/// </summary>
		//-----------------------------------------------------------------------
		[WebMethod(Description = "")]
		public WCFBinding GetWCFBinding()
		{
			return TbServicesApplication.TbServicesEngine.GetWCFBinding();
		}

		/// <summary>
		/// Comunica che un particolare Easy Look non necessita pi?del tbLoader che utilizzava
		/// Viene chiamato quando easy look fa log off
		/// Lo user deve essere sempre easy look
		/// </summary>
		/// <param name="easyToken"></param>
		//-----------------------------------------------------------------------
		[WebMethod]
		public void ReleaseTB(string easyToken)
		{
			TbServicesApplication.TbServicesEngine.ReleaseTB(easyToken);			
		}

		[WebMethod]
		//-----------------------------------------------------------------------
        public string GetTbLoaderInstantiatedListXML(string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
			return string.Empty;

			try
			{
				return TbServicesApplication.TbServicesEngine.GetTbLoaderInstantiatedListXml();
			}
			catch(Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return string.Empty;
			}
		}

		[WebMethod]
		/// <summary>
		/// Restituisce true se c'e' almeno un tbloader istanziato
		/// </summary>
		/// <param name="authenticationToken"></param>
		//-----------------------------------------------------------------------
		public bool IsTbLoaderInstantiated(string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
				return false;

			try
			{
				return TbServicesApplication.TbServicesEngine.IsTbLoaderInstantiated(authenticationToken);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return false;
			}
		}

		/// <summary>
		/// Termina forzatamente il thread
		/// </summary>
		[WebMethod]
		//-----------------------------------------------------------------------
		public void KillThread(int threadID, int processId, string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
				return ;

			try
			{
				TbServicesApplication.TbServicesEngine.KillThread(threadID, processId, authenticationToken);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
			}
		}

		/// <summary>
		/// Prova a chiudere il thread, fallisce se il thread e' impegnato
		/// </summary>
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool StopThread(int threadID, int processId, string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
				throw new Exception (Strings.InvalidToken);//localizzata come il server...
			//lascio le eccezioni
			return TbServicesApplication.TbServicesEngine.StopThread(threadID, processId, authenticationToken);
		}

		/// <summary>
		/// Termina forzatamente il processo
		/// </summary>
		[WebMethod]
		//-----------------------------------------------------------------------
		public void KillProcess(int processId, string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
				return;

			try
			{
				TbServicesApplication.TbServicesEngine.KillProcess(processId, authenticationToken);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
			}
		}

		/// <summary>
		/// Prova a chiudere il processo, fallisce se il processo e' impegnato
		/// </summary>
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool StopProcess(int processId, string authenticationToken)
		{
			if (!TbServicesApplication.TbServicesEngine.IsValidToken(authenticationToken) && !TbServicesApplication.TbServicesEngine.IsValidTokenForConsole(authenticationToken))
				throw new Exception(Strings.InvalidToken);//localizzata come il server...
			//lascio le eccezioni
			return TbServicesApplication.TbServicesEngine.StopProcess(processId, authenticationToken);
		}
		
		[WebMethod]
		//-----------------------------------------------------------------------
		public void SetForceApplicationDate(bool force)
		{
			//lasciato per retrocompatibilita, in realta non fa nulla	
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public bool SetData(string authenticationToken, string data, DateTime applicationDate, int postingAction, out string result, bool useApproximation)
		{
			result = string.Empty;
			return TbServicesApplication.TbServicesEngine.SetData(authenticationToken, data, applicationDate, postingAction, out result, useApproximation);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
        public StringCollection GetData(string authenticationToken, string parameters, DateTime applicationDate, int loadAction, int resultType, int formatType, bool useApproximation)
        {
            return TbServicesApplication.TbServicesEngine.GetData(authenticationToken, parameters, applicationDate, loadAction, resultType, formatType, useApproximation);
        }

		[WebMethod]
	    //-----------------------------------------------------------------------
		public string XmlGetParameters(string authenticationToken, string parameters, DateTime applicationDate, bool useApproximation)
		{
			return TbServicesApplication.TbServicesEngine.XmlGetParameters(authenticationToken, parameters, applicationDate, useApproximation);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetXMLHotLink
			(
			string authenticationToken, 
			string docNamespace, 
			string nsUri, 
			string fieldXPath
			)
		{
			return TbServicesApplication.TbServicesEngine.GetXMLHotLink(authenticationToken, docNamespace, nsUri, fieldXPath);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetDocumentSchema
			(
				string authenticationToken,
				string documentNamespace, 
				string profileName, 
				string forUser
			) 
		{
			return TbServicesApplication.TbServicesEngine.GetDocumentSchema(authenticationToken, documentNamespace, profileName, forUser);
		}

		[WebMethod]
		//-----------------------------------------------------------------------
		public string GetReportSchema
			(
			string authenticationToken,
			string reportNamespace, 
			string forUser
			) 
		{
			return TbServicesApplication.TbServicesEngine.GetReportSchema(authenticationToken, reportNamespace, forUser);
		}

		//------------------------------------------------------------------------------------------------------
		[WebMethod]
		public string GetXMLEnum(string authenticationToken, int enumID, string userLanguage)
		{
			try
			{
				MagicDocumentSession mdSession = new MagicDocumentSession(this.Context, authenticationToken, userLanguage);
				return mdSession.GetXMLEnum((ushort)enumID);
			}
			catch { }
			
			return string.Empty;
		}

        //------------------------------------------------------------------------------------------------------
        [WebMethod]
        public string GetEnumsXml(string userLanguage)
        {
            try
            {
                return TbServicesApplication.TbServicesEngine.GetEnumsXml(userLanguage);
            }
            catch { }

            return string.Empty;
        }

		//----------------------------------------------------------------------------------------------------
		[WebMethod]
		public string GetXMLHotLinkDef(
			string authenticationToken, 
			string docNameSpace, 
			string nsUri, 
			string fieldXPath,
			string companyName)
		{
			GetHotLinkService qLink = new GetHotLinkService(GetApplicationPath());
			return qLink.GetHotLinkDef(docNameSpace, nsUri, fieldXPath, companyName);
		}

		//------------------------------------------------------------------------------------------------------
		private string GetApplicationPath()
		{
			if (!ApplicationPathFinder.IsInit())
			{
				string path = Server.MapPath(".");
				return ApplicationPathFinder.GetCurrentApplicationRoot(path);
			}
			else
				return ApplicationPathFinder.RootPath;
		}

		//------------------------------------------------------------------------------------------------------
		[WebMethod]
		public string RunFunction(string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg)
		{
			return TbServicesApplication.TbServicesEngine.RunFunction(authenticationToken, request, nameSpace, functionName, out errorMsg);
		}

		//---------------------------------------------------------------------------
		[WebMethod]
		public string GetCachedFile(string authenticationToken, string nameSpace, string user, string company)
		{
			return TbServicesApplication.TbServicesEngine.GetCachedFile(authenticationToken, nameSpace, user, company);
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public byte[] GetFileStream(string authenticationToken, string nameSpace, string user, string company)
		{
			return TbServicesApplication.TbServicesEngine.GetFileStream(authenticationToken, nameSpace, user, company);
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public bool IsAlive()
		{
			return true;
		}

		[WebMethod]
		//---------------------------------------------------------------------------
		public DiagnosticSimpleItem[] GetDiagnosticItems(string authenticationToken, bool clear)
		{
			return TbServicesApplication.TbServicesEngine.GetDiagnosticItems(authenticationToken, clear);
		}

        /// <summary>
        /// It returns Server printer names list
        /// </summary>
        /// <param name="printers"></param>
        //-----------------------------------------------------------------------
        [WebMethod]
        public bool GetServerPrinterNames(out string [] printers)
        {
            return TbServicesApplication.TbServicesEngine.GetServerPrinterNames(out printers);
        }

		/// <summary>
		/// It returns Mago.Net installed dictionaries
		/// </summary>
		//------------------------------------------------------------------------------------------------------
		[WebMethod]
		public string[] GetInstalledCultures()
		{
			CultureInfo[] installedCultures = InstallationData.InternalGetInstalledDictionaries(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin"));
			string[] cultures = new string[installedCultures.Length];
			
			for (int i = 0; i < installedCultures.Length; i++ )
				cultures[i] = installedCultures[i].Name;

			return cultures;
		}

        /// <summary>
        /// It return the complete path of the image
        /// </summary>
        //------------------------------------------------------------------------------------------------------
        [WebMethod]
        public string ResolvePath(string image)
        {
            string filepath = image;
            if (image.StartsWith("Image."))
                filepath = PathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(image));

            return filepath;
        }
    }
}
