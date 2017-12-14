using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Services;
using DataSynchronizer;
using System.Web;
using System.Collections.Generic;

namespace Microarea.WebServices.DataSynchronizer
{
    ///<summary>
    /// WebService che si occupa della sincronizzazione dei dati con  provider verso CRM esterni
    ///</summary>
    //================================================================================
    [WebService(Namespace = "http://microarea.it/DataSynchronizer/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    //================================================================================
    public class MicroareaDataSynchronizer : System.Web.Services.WebService
    {
        Assembly iMagoStudioDll = Global.iMagoStudioDll;

        /// <summary>
        /// Init del ws (richiamato dalla Init di LoginManager)
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool Init(string username, string password, bool windowsAuthentication, string company)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { username, password, windowsAuthentication, company };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                {
                    return true;
                    //DataSynchronizerApplication.DSEngine.InitTimer(username, password, windowsAuthentication, company);

                        }
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred during DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }

            return true;
        }

        [WebMethod]
        public bool Reboot(string rebootToken)
        {

            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { rebootToken };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                  return DataSynchronizerApplication.DSEngine.Reboot(rebootToken);
              
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred during DataSynchronizer:Init: {0}", ex.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Notifica di un'azione eseguita sul singolo documento di ERP (insert/update/delete)
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool Notify(string authenticationToken, int logID, string providerName, string tableName, string docNamespace, string docGuid, string onlyForDMS, string iMagoConfigurations)
        {
            //ritorna ok, ho preso la chiamata ed ora la elaboro in maniera asincrona
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { authenticationToken, logID, providerName, tableName, docNamespace, docGuid, onlyForDMS, iMagoConfigurations };
                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
         //   else
         //       DataSynchronizerApplication.DSEngine.Notify(authenticationToken, logID, providerName, onlyForDMS, iMagoConfigurations);

            return true;
        }

        /// <summary>
        /// Metodo generico per verificare se il ws risponde
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { };
                return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            else
                return true;
        }

        /// <summary>
        /// Metodo per l'invoke del metodo in iMagoStudio
        /// </summary>
        ///
        [WebMethod]
        //-----------------------------------------------------------------------
        public object InvokeMethodMagoStudio(string methodName, object[] parametersArray, ParameterModifier[] pMod = null)
        {

            Type type = iMagoStudioDll.GetType("DataSynchronizer.MicroareaDataSynchronizer");
            if (type != null)
            {
                MethodInfo methodInfo = null;

                try
                {
                    methodInfo = type.GetMethod(methodName);
                }
                catch { }

                if (methodInfo == null)
                {
                    var types = new List<Type>();
                    foreach (var arg in parametersArray) { types.Add(Type.GetType(arg.GetType().FullName)); }
                    methodInfo = type.GetMethod(methodName, types.ToArray(), pMod);
                }

                if (methodInfo != null)
                {
                    object result = null;
                    object classInstance = Activator.CreateInstance(type, null);

                    if (parametersArray.Length == 0)
                    {
                        return result = methodInfo.Invoke(classInstance, null);
                    }
                    else
                    {
                        return type.InvokeMember(methodInfo.Name, BindingFlags.InvokeMethod, null, classInstance, parametersArray, pMod, System.Globalization.CultureInfo.CurrentCulture, null);
                    }
                }
                return null;
            }
            else
                return null;
        }


        /// <summary>
        /// Metodo per verificare se e' in esecuzione la batch massiva per quel provider
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsMassiveSynchronizing(int companyId)
        {
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { companyId };
                return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            // else
            //      return DataSynchronizerApplication.DSEngine.IsMassiveSynchronizing(companyId);
            return true;
        }

        /// <summary>
        /// Metodo per verificare se e' in esecuzione la batch massiva di validazione
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsMassiveValidating()
        {
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { };
                return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            else
                return DataSynchronizerApplication.DSEngine.IsMassiveValidating();
        }

        /// <summary>
        /// Metodo richiamato in esecuzione della procedura massiva di esportazione dati
        /// da Mago verso l'applicazione esterna
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SynchronizeOutbound(string authenticationToken, string providerName, string xmlParametes = "")
        {
            // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { authenticationToken, providerName, xmlParametes };
                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
           // else
            //    DataSynchronizerApplication.DSEngine.SynchronizeOutbound(authenticationToken, providerName);

            return true;
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SynchronizeOutboundDelta(string authenticationToken, string providerName, string startSynchroDate, string xmlParametes = "")
        {
            // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { authenticationToken, providerName, startSynchroDate, xmlParametes };
                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
           // else
            //    DataSynchronizerApplication.DSEngine.SynchronizeOutbound(authenticationToken, providerName, startSynchroDate, true);

            return true;
        }

        /// <summary>
        /// Metodo richiamato in esecuzione della procedura massiva di ri-esecuzione degli errori
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SynchronizeErrorsRecovery(string authenticationToken, string providerName)
        {
            // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { authenticationToken, providerName };
                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
           // else
           //     DataSynchronizerApplication.DSEngine.SynchronizeErrorsRecovery(authenticationToken, providerName);

            return true;
        }

        ///<summary>
        /// Metodo richiamato da Mago per impostare le informazioni sul web-reference del DataSynchronizer
        /// Utilizzato nella form dei parametri dei DS_Providers
        ///</summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SetProviderParameters(string authenticationToken, string providerName, string url, string username, string password, bool skipCrtValidation, string IAFModule, string parameters, out string message)
        {
            message = string.Empty;

            DataSynchronizerApplication.DSEngine.SetCulture();

            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(9);
                pMod[8] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, providerName, url, username, password, skipCrtValidation, IAFModule, parameters, message };
                return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            else
                return DataSynchronizerApplication.DSEngine.SetProviderParameters(authenticationToken, providerName, url, username, password, skipCrtValidation, IAFModule, parameters, out message);
        }

        ///<summary>
        /// Metodo richiamato da Mago per testare le informazioni sul web-reference del DataSynchronizer
        /// Utilizzato nella form dei parametri dei DS_Providers
        ///</summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool TestProviderParameters(string authenticationToken, string providerName, string url, string username, string password, bool skipCrtValidation, string parameters, out string message, bool disabled)
        {
            message = string.Empty;
            DataSynchronizerApplication.DSEngine.SetCulture();

            // se disabled = true non procedo a fare il test della connessione e ritorno subito
            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(9);
                pMod[7] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, providerName, url, username, password, skipCrtValidation, parameters, message, disabled };
                bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                message = parametersArray[7] as string;
                return res;
            }
            else
                return DataSynchronizerApplication.DSEngine.TestProviderParameters(authenticationToken, providerName, url, username, password, skipCrtValidation, parameters, out message, disabled);
        }

        /// <summary>
        /// Dato il namespace viene ritornata una stringa che rappresenta le azioni consentite
        /// sul documento (la stringa si riferisce a Insert/Update/Delete (IUD) e i valori ammessi sono 0/1)
        /// Con Infinity viene ritornato sempre 111
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetActionsForDocument(string providerName, string docNamespace)
        {
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { providerName, docNamespace };
                return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            else
                return DataSynchronizerApplication.DSEngine.GetActionsForDocument(providerName, docNamespace);
        }

        /// <summary>
        /// Web Method per la Validazione dell'XML tramite XSD
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool ValidateDocument(string authenticationToken, string providerName, string nameSpace, string tableName, string guidDoc, string serializedErrors, int workerId, out string message)
        {
            message = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {
                    ParameterModifier pMod = new ParameterModifier(9);
                    pMod[7] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, nameSpace, tableName, guidDoc, serializedErrors, workerId, message, false };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                }
                else
                    return DataSynchronizerApplication.DSEngine.ValidateDocument(authenticationToken, providerName, nameSpace, guidDoc, serializedErrors, workerId, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(message);
                Debug.WriteLine(string.Format("An error occurred (validation) DataSynchronizer:ValidateDocument: {0}", message.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// Web Method per la Validazione massiva
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool ValidateOutbound(string authenticationToken, string providerName, bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId, out string message)
        {
            message = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {
                    ParameterModifier pMod = new ParameterModifier(8);
                    pMod[7] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, bCheckFK, bCheckXSD, filters, serializedTree, workerId, message };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                }
                else
                    return DataSynchronizerApplication.DSEngine.ValidateOutbound(authenticationToken, providerName, bCheckFK, bCheckXSD, filters, serializedTree, workerId, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(message);
                Debug.WriteLine(string.Format("An error occurred (ValidateOutbound) DataSynchronizer:ValidateOutbound: {0}", message.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// Web Method per creazione di server esterni su Infinity
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool CreateExternalServer(string authenticationToken, string providerName, string extservername, string connstr, out string message)
        {
            message = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {
                    ParameterModifier pMod = new ParameterModifier(5);
                    pMod[4] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, extservername, connstr, message };
                    bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                    message = parametersArray[4] as string;
                    return res;
                }
                else
                    return DataSynchronizerApplication.DSEngine.CreateExternalServer(authenticationToken, providerName, extservername, connstr, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred (CreateExternalServer) DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// Web Method per la ricerca delle company da associare all'azienda di Mago
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool CheckCompaniesToBeMapped(string authenticationToken, string providerName, out string companylist, out string message)
        {
            message = string.Empty;
            companylist = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {
                    ParameterModifier pMod = new ParameterModifier(4);
                    pMod[2] = true;
                    pMod[3] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, companylist, message };
                    bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                    message = parametersArray[3] as string;
                    companylist = parametersArray[2] as string;
                    return res;
                }
                else
                    return DataSynchronizerApplication.DSEngine.CheckCompaniesToBeMapped(authenticationToken, providerName, out companylist, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred (CheckCompaniesToBeMapped) DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// Web Method per l'associazione dell'azienda di Mago
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool MapCompany(string authenticationToken, string providerName, string appreg, int magocompany, string infinitycompany, string taxid, out string message)
        {
            message = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {

                    ParameterModifier pMod = new ParameterModifier(7);
                    pMod[6] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, appreg, magocompany, infinitycompany, taxid, message };
                    bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                    message = parametersArray[6] as string;
                    return res;
                }
                else
                    return DataSynchronizerApplication.DSEngine.MapCompany(authenticationToken, providerName, appreg, magocompany, infinitycompany, taxid, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred (MapCompany) DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// Web Method per l'upload dei package (BO e TBL) su Infinity
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool UploadActionPackage(string authenticationToken, string providerName, string actionpath, out string message)
        {
            message = string.Empty;

            try
            {
                if (iMagoStudioDll != null)
                {
                    ParameterModifier pMod = new ParameterModifier(4);
                    pMod[3] = true;
                    ParameterModifier[] mods = { pMod };

                    object[] parametersArray = new object[] { authenticationToken, providerName, actionpath, message };
                    bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                    message = parametersArray[3] as string;
                    return res;
                }
                else
                    return DataSynchronizerApplication.DSEngine.UploadActionPackage(authenticationToken, providerName, actionpath, out message);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred (UploadActionPackage) DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }
        }

        ///<summary>
        ///</summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SetConvergenceCriteria(string authenticationToken, string providerName, string xmlCriteria, out string message)
        {
            message = string.Empty;
            DataSynchronizerApplication.DSEngine.SetCulture();

            // se disabled = true non procedo a fare il test della connessione e ritorno subito
            if (iMagoStudioDll != null)
            {

                ParameterModifier pMod = new ParameterModifier(4);
                pMod[3] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, providerName, xmlCriteria, message };
                bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                message = parametersArray[3] as string;
                return res;
            }
            else
                return DataSynchronizerApplication.DSEngine.SetConvergenceCriteria(authenticationToken, providerName, xmlCriteria, out message);
        }

        ///<summary>
        ///</summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool GetConvergenceCriteria(string authenticationToken, string providerName, string actionName, out string xmlCriteria, out string message)
        {
            message = string.Empty;
            xmlCriteria = string.Empty;
            DataSynchronizerApplication.DSEngine.SetCulture();

            // se disabled = true non procedo a fare il test della connessione e ritorno subito
            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(5);
                pMod[3] = true;
                pMod[4] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, providerName, actionName, xmlCriteria, message };
                bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                message = parametersArray[4] as string;
                xmlCriteria = parametersArray[3] as string;
                return res;
            }
            else
                return DataSynchronizerApplication.DSEngine.GetConvergenceCriteria(authenticationToken, providerName, actionName, out xmlCriteria, out message);
        }

        ///<summary>
        ///</summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SetGadgetPerm(string authenticationToken, string providerName, out string message)
        {
            message = string.Empty;
            DataSynchronizerApplication.DSEngine.SetCulture();

            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(3);
                pMod[2] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, providerName, message };
                bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                message = parametersArray[2] as string;
                return res;
            }
            else
                // se disabled = true non procedo a fare il test della connessione e ritorno subito
                return DataSynchronizerApplication.DSEngine.SetGadgetPerm(authenticationToken, providerName, out message);
        }

        [WebMethod]
        public bool PurgeSynchroConnectorLog(int companyId)
        {
            if (iMagoStudioDll != null)
            {
                object[] parametersArray = new object[] { companyId };
                return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
            }
            else
                return DataSynchronizerApplication.DSEngine.PurgeSynchroConnectorLog(companyId);
        }

        /// <summary>
        /// Web Method per la ricerca delle company da associare all'azienda di Mago
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool CheckVersion(string authenticationToken, string providerName, string magoVersion, out string message)
        {
            message = string.Empty;

            return true;

            //try
            //{
            //    if (iMagoStudioDll != null)
            //    {

            //        ParameterModifier pMod = new ParameterModifier(4);
            //        pMod[3] = true;
            //        ParameterModifier[] mods = { pMod };

            //        object[] parametersArray = new object[] { authenticationToken, providerName, magoVersion, message };
            //        bool res = (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
            //        message = parametersArray[3] as string;
            //        return res;
            //    }
            //    else
            //        return DataSynchronizerApplication.DSEngine.CheckVersion(authenticationToken, providerName, magoVersion, out message);
            //}
            //catch (Exception ex)
            //{
            //    DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
            //    Debug.WriteLine(string.Format("An error occurred (CheckVersion) DataSynchronizer:Init: {0}", ex.ToString()));
            //    throw ex;
            //}
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool PauseResume(string authenticationToken, string providerName, bool bPause)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, bPause };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                return true;
               // else
               //     return DataSynchronizerApplication.DSEngine.PauseResume(authenticationToken, providerName, bPause);
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred (PauseResume) DataSynchronizer:Init: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool MassiveAbort(string authenticationToken, string providerName)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                // else
                //     return DataSynchronizerApplication.DSEngine.Abort(authenticationToken, providerName);
                return true;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:MassiveAbort: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetRuntimeFlows(string authenticationToken, string providerName)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetRuntimeFlows: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetLogsByNamespace(string authenticationToken, string providerName, string strNamespace, bool bOnlyError)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, strNamespace, bOnlyError };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetLogsByNamespace: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetLogsByNamespaceDelta(string authenticationToken, string providerName, string strNamespace, bool bOnlyError, bool bDelta)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, strNamespace, bOnlyError, bDelta };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetLogsByNamespaceDelta: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetLogsByDocId(string authenticationToken, string providerName, string TbDocGuid)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, TbDocGuid };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetLogsByDocId: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetMassiveSynchroLogs(string authenticationToken, string providerName, string fromDelta, string OnlyError)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, fromDelta, OnlyError };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetMassiveSynchroLogs: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetSynchroLogs(string authenticationToken, string providerName, string fromDelta, string OnlyError)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, fromDelta, OnlyError };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetMassiveSynchroLogs: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public string GetSynchroLogsByFilters(string authenticationToken,
                                                string providerName,
                                                string Namespace,
                                                bool FromDelta,
                                                bool FromBatch,
                                                bool AllStatus,
                                                bool Status,
                                                bool AllDate,
                                                DateTime FromDate,
                                                DateTime ToDate,
                                                DateTime SynchDate,
                                                string   FlowName,
                                                int Offset)
        {
            try
            {
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName, Namespace, FromDelta, FromBatch, AllStatus, Status, AllDate, FromDate, ToDate, SynchDate, FlowName, Offset };
                    return (string)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:GetMassiveSynchroLogs: {0}", ex.ToString()));
                throw ex;
            }
        }

        [WebMethod]
        //-----------------------------------------------------------------------
        public bool ImagoStudioRuntimeInstalled(string authenticationToken)
        {
            return iMagoStudioDll != null;

        }

        /// <summary>
        /// Metodo richiamato in esecuzione della procedura massiva di esportazione dati
        /// da Mago verso l'applicazione esterna
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool NeedMassiveSynchro(string authenticationToken, string providerName)
        {
            try
            {
                // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, providerName };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:NeedMassiveSynchro: {0}", ex.ToString()));
                throw ex;
            }
        }

        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsActionQueued(string authenticationToken, string requestGuid)
        {
            try
            {
                // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, requestGuid };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:IsActionQueued: {0}", ex.ToString()));
                throw ex;
            }
        }

        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsActionRunning(string authenticationToken, string requestGuid)
        {
            try
            {
                // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
                if (iMagoStudioDll != null)
                {
                    object[] parametersArray = new object[] { authenticationToken, requestGuid };
                    return (bool)InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(ex);
                Debug.WriteLine(string.Format("An error occurred in DataSynchronizer:IsActionRunning: {0}", ex.ToString()));
                throw ex;
            }
        }


        /// <summary>
        /// Notifica di un'azione eseguita sul singolo documento di ERP (insert/update/delete)
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool NotifyGuid(string authenticationToken, int logID, string providerName, string tableName, string docNamespace, string docGuid, string onlyForDMS, string iMagoConfigurations, out string requestGuid)
        {
            requestGuid = string.Empty;
            //ritorna ok, ho preso la chiamata ed ora la elaboro in maniera asincrona
            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(9);
                pMod[8] = true;
                ParameterModifier[] mods = { pMod };

                object[] parametersArray = new object[] { authenticationToken, logID, providerName, tableName, docNamespace, docGuid, onlyForDMS, iMagoConfigurations, requestGuid };

                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray, mods);
                requestGuid = parametersArray[8] as string;
            }
            else
                return true;

            return true;
        }

        /// <summary>
        /// Metodo richiamato in esecuzione della procedura massiva di ri-esecuzione degli errori
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool SynchronizeErrorsRecoveryImago(string authenticationToken, string providerName, out string requestGuid)
        {
            requestGuid = string.Empty;

            // per la procedura massiva di sincronizzazione, richiamata dalla batch di Mago
            if (iMagoStudioDll != null)
            {
                ParameterModifier pMod = new ParameterModifier(3);
                pMod[2] = true;
                ParameterModifier[] mods = { pMod };
                object[] parametersArray = new object[] { authenticationToken, providerName, requestGuid };
                InvokeMethodMagoStudio(System.Reflection.MethodBase.GetCurrentMethod().Name, parametersArray);

                requestGuid = parametersArray[2] as string;
            }
            else
                return true;

            return true;
        }

        /// <summary>
        /// Aggiorna (o nel caso crea) il mapping tra authToken, username di Mago, ip e username windows
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool UpdateUserMapping(string authenticationToken, string windowsUsername, string computerName)
        {
            
            if (iMagoStudioDll != null)
            {
                try
                {
                    ParameterModifier pMod = new ParameterModifier(3);
                    pMod[2] = true;
                    ParameterModifier[] mods = { pMod };
                    object[] parametersArray = new object[] { authenticationToken, windowsUsername, computerName };
                    InvokeMethodMagoStudio(MethodBase.GetCurrentMethod().Name, parametersArray);
                }
                catch (Exception e)
                {
                    DataSynchronizerApplication.DSEngine.WriteErrorLog(e);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Test se il provider e' attivo oppure no
        /// </summary>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsProviderEnabled(string authenticationToken, string providerName)
        {
            try
            {
                return DataSynchronizerApplication.DSEngine.IsProviderEnabled(authenticationToken, providerName);
            }
            catch (Exception e)
            {
                DataSynchronizerApplication.DSEngine.WriteErrorLog(e);
                return false;
            }
        }
    }
}
