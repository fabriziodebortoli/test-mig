using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Ionic.Zip;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.Interfaces;
using System.Security.Cryptography;
using Microarea.TaskBuilderNet.Core.loginMng;
using System.Xml;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    public class InfinityLoginData
    {
        public string Url { get; set; }
        public string DmsUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool SkipCrtValidation { get; set; }
        public string IAFModule { get; set; }
        public string Company { get; set; } // 001
        public string RegisteredApp { get; set; }// MAGO

        public InfinityLoginData()
        {
            Url = string.Empty;
            DmsUrl = string.Empty;
            User = string.Empty;
            Password = string.Empty;
            Company = string.Empty;
            RegisteredApp = string.Empty;
        }
    }

    public class InfinitySyncroService
    {
        private InfinitySyncro.InfinitySyncroService    _infinitySynchService        = null; // webservice InfinityCRM
        private InfinitySyncro.InfinitySyncroService    _infinitySynchServiceForTest = null; // webservice InfinityCRM - usato solo per il test del Configurazion Wizard
        private InfinityDms.InfinityDmsInterfaceService _infinityDmsService          = null; // webservice InfinityDms    

        //---------------------------------------------------------------------
        public InfinitySyncro.InfinitySyncroService GetSyncroService()
        {
            if (_infinitySynchService == null)
            {
                _infinitySynchService = new InfinitySyncro.InfinitySyncroService();
                _infinitySynchService.Timeout = 300000;
            }

            return _infinitySynchService;
        }

        //---------------------------------------------------------------------
        public InfinitySyncro.InfinitySyncroService GetSyncroServiceForTest()
        {
            if (_infinitySynchServiceForTest == null)
            {
                _infinitySynchServiceForTest = new InfinitySyncro.InfinitySyncroService();
                _infinitySynchServiceForTest.Timeout = 300000;
            }

            return _infinitySynchServiceForTest;
        }

        //---------------------------------------------------------------------
        public InfinityDms.InfinityDmsInterfaceService GetDMSSyncroService()
        {
            if (_infinityDmsService == null)
            {
                _infinityDmsService = new InfinityDms.InfinityDmsInterfaceService();
                _infinityDmsService.Timeout = 300000;
            }

            return _infinityDmsService;
        }
    }

    ///<summary>
    /// Wrapper per le chiamate ai webmethod esposti dal servizio InfinitySyncro
    /// dell'installazione di Infinity
    ///</summary>
    //=========================================================================
    internal class InfinitySyncroRef : IValidationProxy
    {
        private const string atomicLevel = "ENTITY";
        private string companyName = string.Empty;
        private string providerName = string.Empty;

        private InfinitySyncroService syncService = null;

        private IRetryable RealRetryier { get; set; }
        private IProviderLogWriter LogWriter { get; set; }
        public InfinityLoginData InfinityData { get; private set; }
        public string RegisteredApp { get { return InfinityData.RegisteredApp; } }

        // Qui viene memorizzato il token restituito dalla Connect ad Infinity
        // Lo stato viene aggiornato dall'esito della chiamata alla Connect ad Infinity.
        // La Connect viene chiamata esplicitamente quando viene instanziata questa classe. 
        // In caso di problemi con le chiamate ai metodi ExecuteSyncro, Rollback o Commit (ossia in quei web method di Infinity che prendono come parametro il token di connessione)
        // è stato implementato un sistema che riprova a rieseguire la chiamata ma solo dopo aver provato a ricollegarsi ad Infinity con una chiamata esplicita alla Connect.
        //---------------------------------------------------------------------
        private string _token = string.Empty;
        public string Token
        {
            get
            {
                return _token;
            }
            private set
            {
                _token = value;
            }
        }

        //---------------------------------------------------------------------
        private string _testToken = string.Empty;
        private string xmlCriteria;

        public string TestToken
        {
            get
            {
                return _testToken;
            }
            private set
            {
                _testToken = value;
            }
        }

        //---------------------------------------------------------------------
        public bool UseRetry { get; private set; }

        //---------------------------------------------------------------------
        public InfinitySyncroRef(string providerName, string companyName, IProviderLogWriter logWriter, bool enableRetry = false)
        {
            this.companyName = companyName;
            this.providerName = providerName;
            this.LogWriter = logWriter;

            syncService = new InfinitySyncroService();

            // Solo nel CRM viene usato il sistema di Ritentativi, inserito per diminuire il numero delle chiamate alla Connect di Infinity.
            UseRetry = enableRetry;
            RealRetryier = RetryableFactory.GetRetryer(UseRetry);

            InfinityData = new InfinityLoginData();
        }

        //---------------------------------------------------------------------
        ~InfinitySyncroRef()
        {
            if (!string.IsNullOrWhiteSpace(Token))
            {
                InfinityDisconnect();
            }
        }

        //--------------------------------------------------------------------------------
        public bool SetInfinityToken(string token, out string message, bool bForTest = false)
        {            
            message = string.Empty;
                        
            bool IsConnect = false;

            try
            {
                if (string.IsNullOrWhiteSpace(token) || token.StartsWith("-10"))
                {
                    LogWriter.WriteToLog(companyName, providerName, token, "SetInfinityToken");
                    message += ": connection failed";
                }
                else
                {
                    if (!bForTest)
                        Token = token;
                    else
                        TestToken = token;

                    IsConnect = true;
                }
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.Fail(we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "SetInfinityToken");
                message = we.Message;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(e.Message);
                LogWriter.WriteToLog(companyName, providerName, e.Message, "SetInfinityToken");
                message = e.Message;
            }

            return IsConnect;
        }

        // Usata nel Retrier (chiamata per nome "Connect")
        //--------------------------------------------------------------------------------
        public bool Connect()
        {
            string _testToken = InfinityConnect();

            bool IsConnect = false;

            try
            {
                if (string.IsNullOrWhiteSpace(_testToken) || _testToken.StartsWith("-10"))
                {
                    LogWriter.WriteToLog(companyName, providerName, _testToken, "Connect");
                }
                else
                {
                    Token = _testToken;
                    IsConnect = true;
                }
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.Fail(we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "Connect");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(e.Message);
                LogWriter.WriteToLog(companyName, providerName, e.Message, "Connect");
            }

            return IsConnect;
        }

        //--------------------------------------------------------------------------------
        public bool Connect(string authenticationToken, out string message)
        {
            if (!string.IsNullOrWhiteSpace(authenticationToken))
                InfinityDisconnect();
            
            message = string.Empty;
            Token = InfinityConnectWithIToken(authenticationToken);

            if (!Token.Contains("-13"))
                return SetInfinityToken(Token, out message);
            else
                return SetInfinityToken(InfinityConnect(), out message);

    }
        
        //--------------------------------------------------------------------------------
        public bool TestConnect(string username, string password, string company, out string message)
        {
            message = string.Empty;
            TestToken = string.Empty;

            bool isOK = SetInfinityToken(TestInfinityConnect(username, password, company), out message, true);

            if (isOK && !string.IsNullOrWhiteSpace(TestToken))
            {
                syncService.GetSyncroService().Url = syncService.GetSyncroServiceForTest().Url;
                syncService.GetSyncroService().SkipServerCertificateValidation = syncService.GetSyncroServiceForTest().SkipServerCertificateValidation;
                TestInfinityDisconnect();
            }

            return isOK;
        }
        
        //--------------------------------------------------------------------------------
        public bool CanDoRetry(SynchroResponseInfo res, out int time)
        {
            time = 0;
            // In caso di errore -11 ho errore di tipo: Server error:User not logged, quindi devo rieseguire la Connect
            if (res != null && 
                res.ProcessInfo != null && 
                res.ProcessInfo.GenericResult != null && 
                res.ProcessInfo.GenericResult.Equals("ko") && 
                res.ProcessInfo.ErrorMessage != null && 
                res.ProcessInfo.ErrorMessage.StartsWith("-11"))
            {
                time = 2000;
                LogWriter.WriteToLog(companyName, providerName, $"New attempt will be triggered after {time} ms. ErrorMsg: {res.ProcessInfo.ErrorMessage}", "InfinitySyncroRef.ExecuteSyncroSafe");
                return false;
            }
            return true;
        }

        // Esegue la chiamata ad Infinity di ExecuteSyncro e in caso di errore prova a rieseguirla con il Retryer
        //---------------------------------------------------------------------
        public string ExecuteSyncroSafe(ActionToExport action, byte[] importActionAr, out SynchroResponseInfo res)
        {
            int time = 0;
            res = null;

            try
            {
                Debug.WriteLine("----------------EXECUTE SYNCHRO----------------");
                Debug.WriteLine(string.Format("XML string to import: {0}\r\n", action.XmlToImport));
                Debug.WriteLine("--------------------------------");

                string syncroResponse = InfinityExecuteSyncro(action.Name, importActionAr);

                Debug.WriteLine(string.Format("Action {0} to CRM. Response: {1}\r\n", action.Name, syncroResponse));
                Debug.WriteLine("----------------END EXECUTE SYNCHRO----------------");

                // parso la risposta ritornata
                res = SynchroResponseParser.GetResponseInfo(syncroResponse, action);

                if (!CanDoRetry(res, out time))
                    throw new RetryAfterMsException(time);
                

                return syncroResponse;
            }
            catch (InvalidOperationException e)
            {
                object[] parameters = new object[] { action.Name, importActionAr };
                RealRetryier.Setup(e);
                return RealRetryier.RetryMethodReturnString(new SimpleRetryableObject(this, "InfinityExecuteSyncro", parameters), e);
            }
        }

        //---------------------------------------------------------------------
        public string CommitEntitySafe(string actionProcessId, string actionName, string xml)
        {
            try
            {
                string response = InfinityCommitEntity(actionProcessId, actionName, xml);

                if (!response.Equals("0"))
                    throw new InvalidOperationException();

                return response;
            }
            catch (InvalidOperationException e)
            {
                object[] parameters = new object[] { actionProcessId, actionName, xml };
                RealRetryier.Setup(e);
                return RealRetryier.RetryMethodReturnString(new SimpleRetryableObject(this, "InfinityCommitEntity", parameters), e);
            }
        }

        //---------------------------------------------------------------------
        public string RollbackEntitySafe(string actionProcessId, string actionName, string keys)
        {
            try
            {
                string response = InfinityRollbackEntity(actionProcessId, actionName, keys);

                if (!response.Equals("0"))
                    throw new InvalidOperationException();

                return response;
            }
            catch (InvalidOperationException e)
            {
                object[] parameters = new object[] { actionProcessId, actionName, keys };
                RealRetryier.Setup(e);
                return RealRetryier.RetryMethodReturnString(new SimpleRetryableObject(this, "InfinityRollbackEntity", parameters), e);
            }
        }

        //---------------------------------------------------------------------
        public byte[] GetXsd(bool specificFileRequest, object[] parameters)
        {
            if (specificFileRequest)
            {
                return syncService.GetSyncroService().getXsdByName(parameters[0].ToString(), parameters[1].ToString());
            }
            else
            {
                return syncService.GetSyncroService().getXsd(parameters[0].ToString(), parameters[1].ToString());
            }
        }

        //---------------------------------------------------------------------
        private string GetTempFolderForDataSynchronizer()
        {
            string temp = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), "DataSynchronizerTemp");
            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);
            return temp;
        }

        //---------------------------------------------------------------------
        public bool CheckInfinityData(out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(InfinityData.Url))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyURI);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.CheckInfinityData");
                return false;
            }

            if (string.IsNullOrWhiteSpace(InfinityData.User))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyCredentials);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.CheckInfinityData");
                return false;
            }

            if (string.IsNullOrWhiteSpace(InfinityData.Company))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.ParametersMissing);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.CheckInfinityData");
                return false;
            }

            if (string.IsNullOrWhiteSpace(InfinityData.RegisteredApp))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.ParametersMissing);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.CheckInfinityData");
                return false;
            }
                   
            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per testare la connessione ad Infinity con le credenziali specificate dall'utente
        /// e salvate nella tabella DS_Providers
        ///</summary>
        //---------------------------------------------------------------------
        public bool TestProviderParameters(string url, string username, string password, bool skipCrtValidation, string parameters, out string message)
        {
            message = string.Empty;
            bool bOk = false;

            if (string.IsNullOrWhiteSpace(url))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyURI);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyCredentials);
                LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            string company = string.Empty;
            string appReg = string.Empty;

            if (!Parse(parameters, out company, out appReg))
            {
                message = Resources.ParametersMissing;
                LogWriter.WriteToLog(companyName, providerName, Resources.ParametersMissing, "InfinitySyncroRef.TestProviderParameters", parameters);
                return false;
            }

            try
            {
                syncService.GetSyncroServiceForTest().SkipServerCertificateValidation = skipCrtValidation;
                syncService.GetSyncroServiceForTest().Url = url;
                bOk = TestConnect(username, password, company, out message);

                // validiamo anche il codice dell'applicazione registrata
                // L'appReg è nulla nel caso in cui sto facendo il "Check Data" del wizard e non ho ancora inserito in infinity l' informazione dell'applicazione registrata
                if (!string.IsNullOrEmpty(appReg))
                {
                    string checkResponse = TestInfinityCheckAppReg(appReg);
                    if (!checkResponse.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                        throw new WebException(checkResponse);
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, "Test Provider Parameters", "InfinitySyncroRef.TestProviderParameters", parameters);
            return bOk;
        }

        ///<summary>
        /// Inizializza la struttura con i dati di connessione ad Infinity (quelli salvati in DS_Providers)
        ///</summary>
        //---------------------------------------------------------------------
        public bool SetProviderParameters(string authenticationToken, string url, string username, string password, bool skipCrtValidation, string parameters, string iafmodule, out string message)
        {
            message = string.Empty;
            bool isConnected = false;
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    message = string.Format(Resources.InfinityConnectionError, Resources.EmptyURI);
                    LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.SetProviderParameters");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    message = string.Format(Resources.InfinityConnectionError, Resources.EmptyCredentials);
                    LogWriter.WriteToLog(companyName, providerName, message, "InfinitySyncroRef.SetProviderParameters");
                    return false;
                }

                string company = string.Empty;
                string regApp = string.Empty;

                if (!Parse(parameters, out company, out regApp))
                {
                    message = Resources.ParametersMissing;
                    LogWriter.WriteToLog(companyName, providerName, Resources.ParametersMissing, "InfinitySyncroRef.SetProviderParameters", parameters);
                    return false;
                }

                InfinityData.Url = url;
                InfinityData.User = username;
                InfinityData.Password = password;
                InfinityData.SkipCrtValidation = skipCrtValidation;
                InfinityData.Company = company;
                InfinityData.RegisteredApp = regApp;
                InfinityData.IAFModule = iafmodule;

                syncService.GetSyncroService().Url = InfinityData.Url;
                syncService.GetSyncroService().SkipServerCertificateValidation = InfinityData.SkipCrtValidation;

                // compongo al volo l'url del dms
                InfinityData.DmsUrl = InfinityData.Url.Replace("InfinitySyncro", "InfinityDmsInterface");
                syncService.GetDMSSyncroService().Url = InfinityData.DmsUrl;

                LogWriter.WriteToLog(companyName, providerName, "Set Provider Parameters end", "InfinitySyncroRef.SetProviderParameters");
           
                isConnected = Connect(authenticationToken, out message);
                return isConnected;
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.SetProviderParameters", "SetProviderParameters");
                return isConnected;
            }
        }

        /// <summary>
        /// Parse parameters 
        /// </summary>
        //--------------------------------------------------------------------------------
        private bool Parse(string parameters, out string company, out string regApp)
        {
            company = string.Empty;
            regApp = string.Empty;

            try
            {
                XElement xelem = XDocument.Parse(parameters).Element("Parameters");

                if (xelem != null)
                {
                    // attenzione che e' case-sensitive
                    if (xelem.Element("WSCode") != null)
                        company = xelem.Element("WSCode").Value; // 001
                    if (xelem.Element("RegisteredApp") != null)
                        regApp = xelem.Element("RegisteredApp").Value; // MAGO
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        ///<summary>
        /// Metodo che esegue multiple chiamate all'executesyncro (si presuppone di essere gia' connessi ad Infinity)
        /// (utilizzata per la gestione degli exclude in caso di cambio filtri)
        ///</summary>
        //--------------------------------------------------------------------------------
        public bool ExecuteSyncroWithoutConnect(List<ActionToExport> actionsList)
        {
            string syncroResponse = string.Empty;
            bool result = true;
            SynchroResponseInfo res = null;

            foreach (ActionToExport action in actionsList)
            {
                // gestione Upload immagini nel DMS
                if (action.Name.Equals("UploadDocuments"))
                    ExecuteDmsUpload(action);
                else
                {
                    try
                    {
                        // metto in un array di byte la stringa xml da inviare
                        //ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                        //byte[] importActionAr = asciiEncoding.GetBytes(action.XmlToImport);

                        byte[] importActionAr = Encoding.GetEncoding("iso-8859-1").GetBytes(action.XmlToImport);

                        syncroResponse = ExecuteSyncroSafe(action, importActionAr, out res);

                        ChechExecuteSyncroResponse(syncroResponse, res); //SynchroResponseParser.GetResponseInfo(syncroResponse, action));
                    }
                    catch (WebException we)
                    {
                        System.Diagnostics.Debug.Fail(we.Message);
                        LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySynchroRef.ExecuteSyncro", "executeSyncro");
                        result = result && false;
                        continue;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Fail(e.Message);
                        LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.ExecuteSyncro", "executeSyncro");
                        result = result && false;
                        continue;
                    }
                }
            }

            return result;
        }

        //--------------------------------------------------------------------------------
        public void ChechExecuteSyncroResponse(string syncroResponse, SynchroResponseInfo res)
        {
            if (res == null || !res.ProcessInfo.GenericResult.Equals("ko"))
                return;

            LogWriter.WriteToLog(companyName, providerName, syncroResponse, "InfinitySynchroRef.ExecuteSyncro", (res != null) ? res.ProcessInfo.ErrorMessage : string.Empty);

            string msg = string.Empty;

            if (!string.IsNullOrWhiteSpace(res.ProcessInfo.ErrorMessage))
            {
                msg = res.ProcessInfo.ErrorMessage;
                res = null;
                throw new Exception(msg);
            }
            else
            {
                foreach (ActionOperation ao in res.ActionDetail.Operations)
                    msg += string.Format("{0}: {1}\r\n", ao.Name, ao.Error);
                res = null;
                throw new Exception(msg);
            }

        }

        ///<summary>
        /// Metodo utilizzato per l'invio di una stringa xml a Infinity 
        /// (utilizzato sia nella massiva che nella singola Notify). 
        /// Si presuppone di essere già collegati ad Infinity.
        ///</summary>
        //--------------------------------------------------------------------------------
        public bool ExecuteSyncroWithoutConnect(ActionToExport action, out string message)
        {
            string syncroResponse = string.Empty;
            message = string.Empty;
            SynchroResponseInfo res = null;

            // gestione Upload immagini nel DMS
            if (action.Name.Equals("UploadDocuments"))
                ExecuteDmsUpload(action);
            else
            {
                try

                {
                    // metto in un array di byte la stringa xml da inviare
                    //ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                    //byte[] importActionAr = asciiEncoding.GetBytes(action.XmlToImport);

                    byte[] importActionAr = Encoding.GetEncoding("iso-8859-1").GetBytes(action.XmlToImport);

                    syncroResponse = ExecuteSyncroSafe(action, importActionAr, out res);

                    ChechExecuteSyncroResponse(syncroResponse, res);

                }
                catch (WebException we)
                {
                    System.Diagnostics.Debug.Fail(we.Message);
                    LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySynchroRef.ExecuteSyncroWithoutConnect", "executeSyncro");
                    message = we.Message;
                    return false;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Fail(e.Message);
                    LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.ExecuteSyncroWithoutConnect", "executeSyncro");
                    message = e.Message;
                    return false;
                }
            }

            return true;
        }
        
        ///<summary>
        /// Metodo che si occupa di uploadare nel DMS di Infinity l'eventuale immagine dell'articolo
        /// 1. creare un file upload.zar che contiene il testo xml
        /// 2. creare uno zip contenente il file da uploadare + il file upload.zar
        /// 3. eseguire la connect + uploadocuments + disconnect
        ///</summary>
        //--------------------------------------------------------------------------------
        private bool ExecuteDmsUpload(ActionToExport action)
        {
            string syncroResponse = string.Empty;
            string contextId = string.Empty;
            int count = 0;

            try
            {
                contextId = syncService.GetDMSSyncroService().connect(InfinityData.User, InfinityData.Password, InfinityData.Company);
                if (string.IsNullOrWhiteSpace(contextId) || contextId.StartsWith("-10"))
                {
                    LogWriter.WriteToLog(companyName, providerName, contextId, "InfinitySynchroRef.ExecuteDmsUpload");
                    return false;
                }

                string dsTempPath = GetTempFolderForDataSynchronizer();
                string filepath = string.Empty;
                foreach (string image in action.Image)
                {
                    string filename = image;
                    if (image.StartsWith("Image."))
                        filename = PathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(image));

                    string uploadZarPath = Path.Combine(dsTempPath, "upload.zar");
                    try
                    {
                        // scrivo il file upload.zar (se ne esiste uno con lo stesso nome lo sovrascrivo)
                        using (StreamWriter sw = new StreamWriter(uploadZarPath, false))
                            sw.Write(action.XmlToImport);
                    }
                    catch (Exception)
                    {
                    }

                    string zipFilePath = Path.Combine(dsTempPath, "Image.zip");
                    // se il file esiste lo elimino altrimenti il save dello zip si arrabbia
                    if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

                    try
                    {
                        // crea il file zip (attenzione: il file non deve esistere!)
                        using (ZipFile zip = new ZipFile(zipFilePath))
                        {
                            zip.AddFile(filename, string.Empty); // Il 2o param è string.Empty per evitare che venga replicato il path del file nello zip
                            zip.AddFile(uploadZarPath, string.Empty); // alla fine aggiungo il file upload.zar
                            zip.Save(); // salvo il file
                        }
                    }
                    catch (ZipException)
                    {
                        zipFilePath = string.Empty;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        zipFilePath = string.Empty;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        Debug.WriteLine("----------------EXECUTE UploadDocuments----------------");
                        Debug.WriteLine(string.Format("UploadDocuments for file: {0}\r\n", zipFilePath));

                        // eseguo la chiamata al DMS solo se il file zip esiste
                        if (File.Exists(zipFilePath))
                            syncroResponse = syncService.GetDMSSyncroService().uploadDocuments(contextId, File.ReadAllBytes(zipFilePath), "0000000000000000", 0);

                        Debug.WriteLine("--------------------------------");
                        Debug.WriteLine(string.Format("UploadDocuments Response: {0}\r\n", syncroResponse));
                        Debug.WriteLine("----------------END EXECUTE UploadDocuments----------------");

                        if (syncroResponse.Equals("0"))
                        {
                            action.IsSucceeded[count] = true;
                            break;
                        }
                    }

                    if (!syncroResponse.Equals("0"))
                        LogWriter.WriteToLog(companyName, providerName, string.Format("Impossible upload image {0}", image), "InfinitySynchroRef.ExecuteDmsUpload");

                    count++;
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.ExecuteDmsUpload");
                return false;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(contextId))
                    syncService.GetDMSSyncroService().disconnect(contextId);
            }

            return true;
        }

        ///<summary>
        /// Metodo generico esposto per le chiamate di test, dove sono cablati l'utente/password/company
        ///</summary>
        //--------------------------------------------------------------------------------
        public string ExecuteSyncroWithoutConnect(string actionName, string importXml)
        {
            string syncroResponse = string.Empty;
            ActionToExport action = new ActionToExport(actionName);
            action.XmlToImport = importXml;
            SynchroResponseInfo res = null;

            try
            {
                // metto in un array di byte la stringa xml da inviare
                //ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                //byte[] importActionAr = asciiEncoding.GetBytes(importXml);
                byte[] importActionAr = Encoding.GetEncoding("iso-8859-1").GetBytes(action.XmlToImport);

                syncroResponse = ExecuteSyncroSafe(action, importActionAr, out res);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.Fail(we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySynchroRef.ExecuteSyncro");
                return string.Empty;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(e.Message);
                LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.ExecuteSyncro");
                return string.Empty;
            }

            return syncroResponse;
        }

        //--------------------------------------------------------------------------------
        public string Rollback(ActionToImport action)
        {
            string syncroResponse = string.Empty;

            try
            {
                string keys = string.Empty;
                int count = 0;
                foreach (string key in action.InfinityKeys)
                {
                    if (!key.Contains("APPREG"))
                    {
                        if (count == 0)
                            keys = key;
                        else
                            keys += "|||" + key;

                        count++;
                    }
                }

                RollbackEntitySafe(action.ProccessId, action.ActionName, keys);

                Debug.WriteLine("----------------ROLLBACK----------------");
                Debug.WriteLine(string.Format("Keys : {0}\r\n", keys));
                Debug.WriteLine("--------------------------------");
                Debug.WriteLine(string.Format("Action {0} to CRM. Response: {1}\r\n", action.ActionName, syncroResponse));
                Debug.WriteLine("----------------END ROLLBACK----------------");
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.Fail(we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySynchroRef.Rollback");
                action.Message = syncroResponse;
                return string.Empty;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(e.Message);
                LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.Rollback");
                action.Message = syncroResponse;
                return string.Empty;
            }

            return syncroResponse;
        }

        //--------------------------------------------------------------------------------
        internal string Commit(string xml, ActionToImport action)
        {
            string syncroResponse = string.Empty;

            try
            {
                syncroResponse = CommitEntitySafe(action.ProccessId, action.ActionName, xml);

                Debug.WriteLine("---------------COMMIT-----------------");
                Debug.WriteLine(string.Format("Keys : {0}\r\n", string.Join(", ", action.InfinityKeys)));
                Debug.WriteLine("--------------------------------");
                Debug.WriteLine(string.Format("Action {0} to CRM. Response: {1}\r\n", action.ActionName, syncroResponse));
                Debug.WriteLine("---------------END COMMIT-----------------");
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.Fail(we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySynchroRef.Commit");
                action.DoRollback = true;
                return string.Empty;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(e.Message);
                LogWriter.WriteToLog(companyName, providerName, e.Message, "InfinitySynchroRef.Commit");
                action.DoRollback = true;
                return string.Empty;
            }

            return syncroResponse;
        }

        //--------------------------------------------------------------------------------
        public string InfinityConnect()
        {
            return syncService.GetSyncroService().connect(InfinityData.User, InfinityData.Password, InfinityData.Company);
        }

        //--------------------------------------------------------------------------------
        public string TestInfinityConnect(string username, string password, string company)
        {
            return syncService.GetSyncroServiceForTest().connect(username, password, company);
        }

        //--------------------------------------------------------------------------------
        public string TestInfinityCheckAppReg(string appReg) // validiamo anche il codice dell'applicazione registrata
        {
            return syncService.GetSyncroServiceForTest().checkAppReg(appReg);
        }

        //--------------------------------------------------------------------------------
        public string InfinityConnectWithIToken(string iToken)
        {
            return syncService.GetSyncroService().iMagoConnectWithToken(iToken);
        }

        //--------------------------------------------------------------------------------
        public string InfinityDisconnect()
        {
            syncService.GetSyncroService().disconnect(Token);
            Token = string.Empty;
            return Token;
        }

        //--------------------------------------------------------------------------------
        public string TestInfinityDisconnect()
        {
            syncService.GetSyncroServiceForTest().disconnect(TestToken);
            TestToken = string.Empty;
            return TestToken;
        }

        //--------------------------------------------------------------------------------
        public string InfinityCheckAppReg() // validiamo anche il codice dell'applicazione registrata
        {
            return syncService.GetSyncroService().checkAppReg(RegisteredApp);
        }

        //--------------------------------------------------------------------------------
        public string InfinityCommitEntity(string actionProcessId, string actionName, string xml)
        {
            return syncService.GetSyncroService().commitEntity(Token, actionProcessId, actionName, RegisteredApp, xml);
        }

        //--------------------------------------------------------------------------------
        public string InfinityRollbackEntity(string actionProcessId, string actionName, string keys)
        {
            return syncService.GetSyncroService().rollbackEntity(Token, actionProcessId, actionName, keys, RegisteredApp);
        }

        //--------------------------------------------------------------------------------
        public string InfinityExecuteSyncro(string actionName, byte[] importActionAr)
        {
            return syncService.GetSyncroService().executeSyncro(Token, RegisteredApp, actionName, importActionAr, atomicLevel);
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per generare i server esterni
        ///</summary>
        //---------------------------------------------------------------------
        public bool CreateExternalServer(string extservername, string connstr, out string message)
        {
            message = string.Empty;

            try
            {
                message = syncService.GetSyncroService().iMagoServerEst(extservername, connstr);
                if (!message.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(message);

                message = syncService.GetSyncroService().checkJDBCConnection(connstr);
                if (!message.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(message);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.CreateExternalServer");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.CreateExternalServer");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, extservername + " external server created", "InfinitySyncroRef.CreateExternalServer");

            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per recuperare la lista delle company mappabiloi
        ///</summary>
        //---------------------------------------------------------------------
        public bool CheckCompaniesToBeMapped(out string companylist, out string message)
        {
            string sDummy = string.Empty;
            message = string.Empty;
            companylist = string.Empty;

            try
            {
                string AllCompanies = syncService.GetSyncroService().getAllCompany(sDummy);
                if (!AllCompanies.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(AllCompanies);

                string AlreadyMappedCompanies = syncService.GetSyncroService().iMagoCompanyAlreadyMapped(sDummy);
                if (!AlreadyMappedCompanies.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(AlreadyMappedCompanies);

                // TODO: calcolare la differenza fra le due stringhe dopo averne fatto la tokenize
                string[] companySeparators = new string[] { ":" };
                string[] mappedSeparators = new string[] { "|||" };
                string[] CompanyString = AllCompanies.Split(companySeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                if (CompanyString.GetLength(0) <= 1)
                    throw new WebException("No company found for the Infinity License");

                string[] tkCompany = CompanyString[1].Split(mappedSeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                string[] tkAlreadyMapped = AlreadyMappedCompanies.Split(mappedSeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                List<string> diff = new List<string>();
                bool found = false;
                foreach (var tkc in tkCompany)
                {
                    found = false;
                    string[] infocompanySeparators = new string[] { "||" };
                    string[] companyName = tkc.Split(infocompanySeparators, 100, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var tka in tkAlreadyMapped)
                    {
                        if (!tka.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string[] mappedcompanyName = tka.Split('=');

                            if (mappedcompanyName.Length > 1 && companyName[0].ToString() == mappedcompanyName[1].ToString())
                            {
                                found = true;
                                continue;
                            }
                        }
                    }

                    if (!found)
                        diff.Add(companyName[0] + "=" + companyName[1]);

                }

                if (diff.Count > 0)
                    companylist = string.Join("|||", diff);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.CheckCompaniesToBeMapped");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.CheckCompaniesToBeMapped");
                return false;
            }

            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per associare una azienda di Mago con una di Infinity
        ///</summary>
        //---------------------------------------------------------------------
        public bool MapCompany(string appreg, int magocompany, string infinitycompany, string taxid, out string message)
        {
            message = string.Empty;

            try
            {
                byte[] buf = new byte[256];
                byte[] encrypted;
                string b64company;
                string InfoToBeCripted = taxid + "|||" + magocompany.ToString();
                using (Aes myAes = Aes.Create())
                {

                    myAes.Key = Encoding.ASCII.GetBytes("987MAG123XYZIMAG");
                    myAes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    myAes.Mode = CipherMode.ECB;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {

                                //Write all data to the stream.
                                swEncrypt.Write(InfoToBeCripted);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }

                    b64company = Convert.ToBase64String(encrypted, 0, encrypted.Length);
                }

                message = syncService.GetSyncroService().iMagoWizard(appreg, magocompany, infinitycompany, b64company);

                if (!message.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(message);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.MapCompany");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.MapCompany");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, "Map Company end", "InfinitySyncroRef.MapCompany");

            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per fare upload di package di azioni
        ///</summary>
        //---------------------------------------------------------------------
        public bool UploadActionPackage(string actionpath, out string message)
        {
            message = string.Empty;

            try
            {
                string[] filenames = Directory.GetFiles(actionpath, "*.zip");

                foreach (string filename in filenames)
                {
                    string ImportPackageReponse = syncService.GetSyncroService().importPackage(File.ReadAllBytes(filename));                    
                    message += "Uploading Package: " + Path.GetFileName(filename) + " - " + ImportPackageReponse + "\n";
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message += string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.UploadActionPackage");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message += string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.UploadActionPackage");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, "Upload Action Package end", "InfinitySyncroRef.UploadActionPackage");

            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per fare upload di package di azioni
        ///</summary>
        //---------------------------------------------------------------------
        public bool SetConvergenceCriteria(string xmlCriteria, out string message)
        {
            message = string.Empty;

            try
            {
                message = syncService.GetSyncroService().SetConvergencyCriteria(xmlCriteria);

                if (!message.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(message);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message += string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.SetConvergenceCriteria");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message += string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.SetConvergenceCriteria");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, "Set Convergence Criteria end", "InfinitySyncroRef.SetConvergenceCriteria");

            return true;
        }


        ///<summary>
        /// Richiamato dal DataSynchronizer ws per fare upload di package di azioni
        ///</summary>
        //---------------------------------------------------------------------
        public bool GetConvergenceCriteria(string actionName, out string xmlCriteria, out string message)
        {
            message = string.Empty;
            xmlCriteria = string.Empty;

            try
            {
                xmlCriteria = syncService.GetSyncroService().GetConvergencyCriteria(actionName);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message += string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.GetConvergenceCriteria");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message += string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.GetConvergenceCriteria");
                return false;
            }

            LogWriter.WriteToLog(companyName, providerName, "Set Convergence Criteria end", "InfinitySyncroRef.SetConvergenceCriteria");

            return true;
        }


        ///<summary>
        /// Richiamato dal DataSynchronizer ws per fare upload di package di azioni
        ///</summary>
        //---------------------------------------------------------------------
        public bool SetGadgetPerm(out string message)
        {
            message = string.Empty;
            xmlCriteria = string.Empty;

            try
            {
                string dummy = string.Empty;
                message = syncService.GetSyncroService().iMagoSetGadgetPermission(dummy);

                if (message.StartsWith("KO", StringComparison.InvariantCultureIgnoreCase))
                    throw new WebException(message);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message += string.Format(Resources.InfinityConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.GetConvergenceCriteria");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message += string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.GetConvergenceCriteria");
                return false;
            }

            return true;
        }

        ///<summary>
        /// Richiamato dal DataSynchronizer ws per verificare la versione
        ///</summary>
        //---------------------------------------------------------------------
        public bool CheckVersion(string magoVersion, out string message)
        {
            string sDummy = string.Empty;
            message = string.Empty;

            try
            {
                string VersionResponse = syncService.GetSyncroService().iMagoCheckVersion(magoVersion);
                if (!VersionResponse.StartsWith("OK", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] msgSeparators = new string[] { "|||" };
                    string[] Response = VersionResponse.Split(msgSeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                    throw new WebException(Response[1]);
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = we.Message;
                LogWriter.WriteToLog(companyName, providerName, we.Message, "InfinitySyncroRef.CheckVersion");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ex.Message.Contains("No such operation 'iMagoCheckVersion'"))
                    message = "Please upgrade Infinity to the required fast patch for this Mago version";
                else
                    message = string.Format(Resources.InfinityConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "InfinitySyncroRef.CheckVersion");
                return false;
            }

            return true;
        }


        ////Metodo che passata un'Action CRM, restituisce il nameSpace relativo di Mago
        //public static string GetNameSpaceByActionCRM(string actionCRM)
        //{
        //    string nameSpace = "";
        //    string pathFile = @"C:\Development_1_X\Standard\Applications\ERP\SynchroConnector\SynchroProviders\CRMInfinity\SynchroProfiles.xml";


        //    XmlTextReader reader = new XmlTextReader(pathFile);
        //    while (reader.Read())
        //    {
        //        if (reader.Name.Equals("Document"))
        //            nameSpace = reader.GetAttribute("namespace");
        //        else if (reader.Name.Equals("Action"))
        //        {
        //            if (reader.GetAttribute("name").Equals(actionCRM))
        //                return nameSpace;
        //        }
        //    }

        //    return "";
        //}
    }
} 
