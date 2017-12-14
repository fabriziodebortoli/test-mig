using System;
using System.Text;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;


namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	//================================================================================
	public class TbServices : ITbServices
    {
		#region variabili private
		private tbService.TbServices tbServices = new tbService.TbServices();
		#endregion

		#region costruttori
		//---------------------------------------------------------------------------
		public TbServices()
			: this(BasePathFinder.BasePathFinderInstance.TbServicesUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut)
		{

		}
		//---------------------------------------------------------------------------
		public TbServices (string TbServicesUrl, int timeout)
		{
			tbServices.Url = TbServicesUrl;
			tbServices.Timeout = timeout;
		}
		#endregion

		#region metodi per l'istanziazione dei tb
		//---------------------------------------------------------------------------
		public void CloseTB (string authenticationToken)
		{
			tbServices.CloseTB(authenticationToken);
		}

		/// <summary>
		/// Verifica che l'istanza di tb passata sia viva e valida per la company connessa.
		/// </summary>
		/// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader</param>
		/// <returns>true se ?un'interfaccia valida</returns>
		//-----------------------------------------------------------------------
		private bool IsValidTbLoaderInstance (TbLoaderClientInterface tbInterface, string companyName, string userName)
		{
			string tmpUser = string.Empty;
			string tmpCompany = string.Empty;

			return
				tbInterface != null &&
				tbInterface.Available &&
				tbInterface.GetCurrentUser(out tmpUser, out tmpCompany) &&
				string.Compare(tmpCompany, companyName, true) == 0 &&
				string.Compare(tmpUser, userName, true) == 0;
		}

		/// <summary>
		/// Verifica che l'istanza di tb passata sia viva e valida per l'utente connesso e per la data specificata
		/// </summary>
		/// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader</param>
		/// <param name="applicationDate">Data di applicazione necessaria</param>
		/// <returns>true se ?un'interfaccia valida</returns>
		//-----------------------------------------------------------------------
		private bool IsValidTbLoaderInstance (TbLoaderClientInterface tbInterface, string companyName, string userName, DateTime applicationDate)
		{
			return
				IsValidTbLoaderInstance(tbInterface, companyName, userName) &&
				tbInterface.GetApplicationDate().ToShortDateString() == applicationDate.ToShortDateString();
		}

		//-----------------------------------------------------------------------
		private void ThrowEasyError (int errorCode, string authenticationToken)
		{
			if (errorCode >= 0)
				return;

            DiagnosticSimpleItem[] details = null;
			try
			{
				details = tbServices.GetDiagnosticItems(authenticationToken, true);
			}
			catch (Exception ex)
			{
				throw new TbServicesException(WebServicesWrapperStrings.CannotObtainDiagnostic, new DiagnosticSimpleItem[0], ex);
			}
			switch (errorCode)
			{
				case (int)EasyLookConnectionCodes.UserNotAuthenticated:
					throw new TbServicesException(WebServicesWrapperStrings.UserNotAuthenticated, details);
				case (int)EasyLookConnectionCodes.EasyLookSysLoginFailed:
					throw new TbServicesException(WebServicesWrapperStrings.EasyLookSysLoginFailed, details);
				case (int)EasyLookConnectionCodes.StartTbLoaderFailed:
					throw new TbServicesException(WebServicesWrapperStrings.StartTbLoaderFailed, details);
				case (int)EasyLookConnectionCodes.InitTbLoginFailed:
					throw new TbServicesException(WebServicesWrapperStrings.InitTbLoginFailed, details);
				case (int)EasyLookConnectionCodes.SetApplicationDateFailed:
					throw new TbServicesException(WebServicesWrapperStrings.SetApplicationDateFailed, details);
			}
		}

		/// <summary>
		/// deve istanziare un TbLoader completamente loginato con user/pwd/company/applicationDate 
		/// se l'interfaccia ?diversa da null deve controllarne l'esistenza
		/// </summary>
		/// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader istanziata generica</param>
		/// <param name="tbInterfaceForHotLink">Referenza ad un'interfaccia a tbLoader istanziata per data</param>
		/// <param name="applicationDate">Data di applicazione</param>
		/// <returns>Referenza ad un'interfaccia a tbLoader</returns>		
		//-----------------------------------------------------------------------
		public ITbLoaderClient CreateTB(IBasePathFinder pathFinder, string authenticationToken, string companyName, ITbLoaderClient tbInterface, DateTime applicationDate, bool useRemoteServer = false)
		{
			return CreateTB(pathFinder, authenticationToken, companyName, null, tbInterface as TbLoaderClientInterface, applicationDate, useRemoteServer);
		}

		/// <summary>
		/// deve istanziare un TbLoader completamente loginato con user/pwd/company/ 
		/// e se richiesto anche per applicationDate 
		/// se l'interfaccia ?diversa da null deve controllarne l'esistenza
		/// </summary>
		/// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader istanziata generica</param>
		/// <param name="tbInterfaceForHotLink">Referenza ad un'interfaccia a tbLoader istanziata per data</param>
		/// <param name="applicationDate">Data di applicazione</param>
		/// <param name="hotlink">se imposto a false non esegue controlli di data</param>
		/// <returns>Referenza ad un'interfaccia a tbLoader</returns>
		//-----------------------------------------------------------------------
		private TbLoaderClientInterface CreateTB(IBasePathFinder pathFinder, string authenticationToken, string companyName, string userName, TbLoaderClientInterface tbInterface, DateTime applicationDate, bool useRemoteServer = false)
		{
			int soapPort = -1;

			// controllo che il tb sia compatibile e vivo in termini di utente, company e data
			if (IsValidTbLoaderInstance(tbInterface, companyName, userName, applicationDate))
				return tbInterface;
			string server = Environment.MachineName;
			string dummy = string.Empty;
			if (useRemoteServer)
			{
				soapPort = tbServices.CreateTBEx(authenticationToken, applicationDate, true, out dummy, out server);
			}
			else
			{
				soapPort = tbServices.CreateTB(authenticationToken, applicationDate, true, out dummy);
			}
					
			//il tb non esiste o non ?compatibile ne istanzio un altro
			if (soapPort < 0)
				ThrowEasyError(soapPort, authenticationToken);

			TbLoaderClientInterface tb = new TbLoaderClientInterface(pathFinder, soapPort, authenticationToken, GetWCFBinding());
			if (!server.CompareNoCase(tb.TbServer))
				tb.TbServer = server;
			return tb;
		}

		/// <summary>
		/// Ritorna il tipo di binding impostato genericamente nel web config dell'applicazione TbServices
		/// </summary>
		//-----------------------------------------------------------------------
		public WCFBinding GetWCFBinding()
		{
			return (WCFBinding)tbServices.GetWCFBinding();
		}

		/// <summary>
		/// Comunica al server che le istanze di easy look richieste non sono pi?necessarie
		/// </summary>
		/// <param name="token">token di autenticazione di un'istanza di tbLoader</param>
		//-----------------------------------------------------------------------
		public void ReleaseTB (string token)
		{
			if (token != string.Empty)
				tbServices.ReleaseTB(token);
		}
		#endregion

		#region Metodi per il cambio dati office <-> tb

		//-----------------------------------------------------------------------
		public string[] GetData (string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation)
		{
			string[] pages = tbServices.GetData(authenticationToken, paramsDoc.OuterXml, applicationDate, 0, 1, 0, useApproximation);

			return pages;
		}

		//-----------------------------------------------------------------------
		public bool SetData (string authenticationToken, string data, DateTime applicationDate, int action, out XmlDocument resDoc, bool useApproximation)
		{
			resDoc = new XmlDocument();
			string tmp = string.Empty;

			string easyToken = string.Empty;
			tbServices.CreateTB(authenticationToken, applicationDate, false, out easyToken);
			//non trappo niente perch?trappa il chiamante
			bool b = tbServices.SetData(authenticationToken, data, applicationDate, action, useApproximation, out tmp);
			if (tmp == string.Empty)
				return true;

			resDoc.LoadXml(tmp);

			return b;
		}

		//-----------------------------------------------------------------------
		public bool SetData(string authenticationToken, string data, DateTime applicationDate, int action, out string result, bool useApproximation)
		{

			string easyToken = string.Empty;
			tbServices.CreateTB(authenticationToken, applicationDate, false, out easyToken);
			//non trappo niente perch?trappa il chiamante
			bool b = tbServices.SetData(authenticationToken, data, applicationDate, action, useApproximation, out result);
			if (result == string.Empty)
				return true;

			return b;
		}

		/// <returns>-1 if impossible to create TBLoader; 1 if SetData failed; 0 if everything is OK</returns>
		//-----------------------------------------------------------------------
		public int SetData(string authenticationToken, string data, DateTime applicationDate, int action, bool useApproximation, out string result)
		{
			string easyToken = string.Empty;
			result = string.Empty;
			if (tbServices.CreateTB(authenticationToken, applicationDate, false, out easyToken) < 0)
				return -1;
			//non trappo niente perch?trappa il chiamante
			bool b = tbServices.SetData(authenticationToken, data, applicationDate, action, useApproximation, out result);
			if (result == string.Empty || !b)
				return 1;

			return 0;
		}

		//-----------------------------------------------------------------------
		public XmlDocument XmlGetParameters (string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation)
		{
			//non trappo niente perch?trappa il chiamante
			string res = tbServices.XmlGetParameters(authenticationToken, paramsDoc.OuterXml, applicationDate, useApproximation);
			XmlDocument resDoc = new XmlDocument();
			resDoc.LoadXml(res);
			return resDoc;
		}

		//-----------------------------------------------------------------------
		public XmlDocument GetXMLHotLink (string authenticationToken, string docNamespace, string nsUri, string fieldXPath)
		{
			string res = tbServices.GetXMLHotLink(authenticationToken, docNamespace, nsUri, fieldXPath);
			if (res == string.Empty)
				return null;

			XmlDocument resDoc = new XmlDocument();
			resDoc.LoadXml(res);
			return resDoc;
		}

		//----------------------------------------------------------------------
		public string getXMLEnum (string authenticationToken, int enumPos, string language)
		{
			return tbServices.GetXMLEnum(authenticationToken, enumPos, language);
		}

		//-----------------------------------------------------------------------
		public string GetDocumentSchema (string authenticationToken, string documentNamespace, string profileName, string forUser)
		{
			return tbServices.GetDocumentSchema(authenticationToken, documentNamespace, profileName, forUser);
		}

		//-----------------------------------------------------------------------
		public string GetReportSchema (string authenticationToken, string reportNamespace, string forUser)
		{
			return tbServices.GetReportSchema(authenticationToken, reportNamespace, forUser);
		}

		//-----------------------------------------------------------------------
		public string RunFunction (string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg)
		{
			return tbServices.RunFunction(authenticationToken, request, nameSpace, functionName, out errorMsg);
		}

		//---------------------------------------------------------------------------
		public bool IsAlive ()
		{
			return tbServices.IsAlive();
		}

		//---------------------------------------------------------------------------
		public void Init ()
		{
			tbServices.Init();
		}

		//---------------------------------------------------------------------------
		public string GetTbLoaderInstantiatedListXML (string authToken)
		{
			return tbServices.GetTbLoaderInstantiatedListXML(authToken);
		}

		//ritorna true se esiste un tbloader istanziato collegato al token di autenticazione
		//---------------------------------------------------------------------------
		public bool IsTbLoaderInstantiated(string authToken)
		{
			return tbServices.IsTbLoaderInstantiated(authToken);
		}


		//---------------------------------------------------------------------------
		public void KillThread (int threadID, int processID, string authTok)
		{
			tbServices.KillThread(threadID, processID, authTok);
		}

		//---------------------------------------------------------------------------
		public bool StopThread (int threadID, int processID, string authTok)
		{
			return tbServices.StopThread(threadID, processID, authTok);
		}

		//---------------------------------------------------------------------------
		public void KillProcess (int processID, string authTok)
		{
			tbServices.KillProcess(processID, authTok);
		}

		//---------------------------------------------------------------------------
		public bool StopProcess (int processID, string authTok)
		{
			return tbServices.StopProcess(processID, authTok);
		}

		#endregion
	}
}
