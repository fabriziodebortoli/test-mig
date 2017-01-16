using System;
using System.Xml;

using Microarea.RSWeb.Generic;
using Microarea.RSWeb.NameSolver;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.RSWeb.WebServicesWrapper
{
    public class TbServices : ITbServices
    {
 
        //---------------------------------------------------------------------------
        public TbServices()
            : this(BasePathFinder.BasePathFinderInstance.TbServicesUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut)
        {

        }
        //---------------------------------------------------------------------------
        public TbServices(string TbServicesUrl, int timeout)
        {
        }
 
        //---------------------------------------------------------------------------
        public void CloseTB(string authenticationToken)
        {
        }

        /// <summary>
        /// Verifica che l'istanza di tb passata sia viva e valida per la company connessa.
        /// </summary>
        /// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader</param>
        /// <returns>true se ?un'interfaccia valida</returns>
        //-----------------------------------------------------------------------
        private bool IsValidTbLoaderInstance(ITbLoaderClient tbInterface, string companyName, string userName)
        {
            string tmpUser = string.Empty;
            string tmpCompany = string.Empty;

            return false;
        }

        /// <summary>
        /// Verifica che l'istanza di tb passata sia viva e valida per l'utente connesso e per la data specificata
        /// </summary>
        /// <param name="tbInterface">Referenza ad un'interfaccia a tbLoader</param>
        /// <param name="applicationDate">Data di applicazione necessaria</param>
        /// <returns>true se ?un'interfaccia valida</returns>
        //-----------------------------------------------------------------------
        private bool IsValidTbLoaderInstance(ITbLoaderClient tbInterface, string companyName, string userName, DateTime applicationDate)
        {
            return  false;
        }

        //-----------------------------------------------------------------------
        private void ThrowEasyError(int errorCode, string authenticationToken)
        {
            if (errorCode >= 0)
                return;

            TaskBuilderNetCore.Interfaces.IDiagnosticSimpleItem[] details = null;
            try
            {
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
            catch (Exception ex )
            {
                throw new TbServicesException(WebServicesWrapperStrings.CannotObtainDiagnostic, null/*new IDiagnosticSimpleItem[0], ex TODO rsweb*/);
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
            return CreateTB(pathFinder, authenticationToken, companyName, null, tbInterface as ITbLoaderClient, applicationDate, useRemoteServer);
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
        private ITbLoaderClient CreateTB(IBasePathFinder pathFinder, string authenticationToken, string companyName, string userName, ITbLoaderClient tbInterface, DateTime applicationDate, bool useRemoteServer = false)
        {
            throw new NotImplementedException();
           // int soapPort = -1;

           // // controllo che il tb sia compatibile e vivo in termini di utente, company e data
           // if (IsValidTbLoaderInstance(tbInterface, companyName, userName, applicationDate))
           //     return tbInterface;
           // string server = Environment.MachineName;
           // string dummy = string.Empty;
           // if (useRemoteServer)
           // {
           //     soapPort = tbServices.CreateTBEx(authenticationToken, applicationDate, true, out dummy, out server);
           // }
           // else
           // {
           //     soapPort = tbServices.CreateTB(authenticationToken, applicationDate, true, out dummy);
           // }

           // //il tb non esiste o non ?compatibile ne istanzio un altro
           // if (soapPort < 0)
           //     ThrowEasyError(soapPort, authenticationToken);

           //// TbLoaderClientInterface tb = new TbLoaderClientInterface(pathFinder, soapPort, authenticationToken, GetWCFBinding());
           // if (!server.CompareNoCase(tb.TbServer))
           //     tb.TbServer = server;
           // return tb;
        }

        /// <summary>
        /// Ritorna il tipo di binding impostato genericamente nel web config dell'applicazione TbServices
        /// </summary>
        //-----------------------------------------------------------------------
        public WCFBinding GetWCFBinding()
        {
            return WCFBinding.None;
        }

        /// <summary>
        /// Comunica al server che le istanze di easy look richieste non sono pi?necessarie
        /// </summary>
        /// <param name="token">token di autenticazione di un'istanza di tbLoader</param>
        //-----------------------------------------------------------------------
        public void ReleaseTB(string token)
        {
            
        }
 
        //-----------------------------------------------------------------------
        public string[] GetData(string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation)
        {
            string[] pages = null;
            return pages;
        }

        //-----------------------------------------------------------------------
        public bool SetData(string authenticationToken, string data, DateTime applicationDate, int action, out XmlDocument resDoc, bool useApproximation)
        {
            resDoc = null;
            return false;
        }

        //-----------------------------------------------------------------------
        public bool SetData(string authenticationToken, string data, DateTime applicationDate, int action, out string result, bool useApproximation)
        {
            result = string.Empty;
            return false;
        }

        /// <returns>-1 if impossible to create TBLoader; 1 if SetData failed; 0 if everything is OK</returns>
        //-----------------------------------------------------------------------
        public int SetData(string authenticationToken, string data, DateTime applicationDate, int action, bool useApproximation, out string result)
        {
            result = string.Empty;
            return -1;
        }

        //-----------------------------------------------------------------------
        public XmlDocument XmlGetParameters(string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation)
        {
            
            return null;
        }

        //-----------------------------------------------------------------------
        public XmlDocument GetXMLHotLink(string authenticationToken, string docNamespace, string nsUri, string fieldXPath)
        {
            
            return null;
        }

        //----------------------------------------------------------------------
        public string getXMLEnum(string authenticationToken, int enumPos, string language)
        {
            return string.Empty;
        }

        //-----------------------------------------------------------------------
        public string GetDocumentSchema(string authenticationToken, string documentNamespace, string profileName, string forUser)
        {
            return string.Empty; 
        }

        //-----------------------------------------------------------------------
        public string GetReportSchema(string authenticationToken, string reportNamespace, string forUser)
        {
            return string.Empty;
        }

        //-----------------------------------------------------------------------
        public string RunFunction(string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg)
        {
            errorMsg = "not implemented";
            return string.Empty;
        }

        //---------------------------------------------------------------------------
        public bool IsAlive()
        {
            return false;
        }

        //---------------------------------------------------------------------------
        public void Init()
        {
            
        }

        //---------------------------------------------------------------------------
        public string GetTbLoaderInstantiatedListXML(string authToken)
        {
            return string.Empty;
        }

        //ritorna true se esiste un tbloader istanziato collegato al token di autenticazione
        //---------------------------------------------------------------------------
        public bool IsTbLoaderInstantiated(string authToken)
        {
            return false;
        }


        //---------------------------------------------------------------------------
        public void KillThread(int threadID, int processID, string authTok)
        {
           
        }

        //---------------------------------------------------------------------------
        public bool StopThread(int threadID, int processID, string authTok)
        {
            return false;
        }

        //---------------------------------------------------------------------------
        public void KillProcess(int processID, string authTok)
        {
            
        }

        //---------------------------------------------------------------------------
        public bool StopProcess(int processID, string authTok)
        {
            return false;
        }

       
    }
}
