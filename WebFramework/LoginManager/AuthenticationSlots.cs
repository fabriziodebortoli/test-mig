using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.Licence;

namespace Microarea.WebServices.LoginManager
{
    //-----------------------------------------------------------------------
    internal class AuthenticationProcess
    {
        /// <summary>
        /// elenco dei processi microarea che possono effettuare una login
        /// </summary>
        //-----------------------------------------------------------------------
        internal static int IsValidProcess(string processName, bool webLogin, bool gdiLogin)
        {
            LoginSlotType calType = GetCalType(processName);

            if (calType == LoginSlotType.Invalid)
                return (int)LoginReturnCodes.InvalidProcessError;

            if (calType == LoginSlotType.Gdi)//con gdi devi essere gdi
                return (gdiLogin) ? (int)LoginReturnCodes.NoError : (int)LoginReturnCodes.GDIApplicationAccessDenied;

            else if (calType == LoginSlotType.Mobile)
                return (!webLogin && !gdiLogin) ? (int)LoginReturnCodes.NoError : (int)LoginReturnCodes.InvalidProcessError;

            else if (!((calType == LoginSlotType.EasyLook && (webLogin || gdiLogin)) ||//easylook può essere gdi o web
                 (calType == LoginSlotType.ThirdPart && gdiLogin) ||//solo gdi
                 (calType == LoginSlotType.MagicDocument && gdiLogin))) //solo gdi
                return (int)LoginReturnCodes.WebApplicationAccessDenied;

            return (int)LoginReturnCodes.NoError;
        }

        //-----------------------------------------------------------------------
        internal static LoginSlotType GetCalType(string processName)
        {
            if (processName == null || processName.Length == 0)
                return LoginSlotType.Invalid;

            if (
                string.Compare(processName, ProcessType.MenuManager, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.MicroareaConsole, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.SchedulerAgent, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.SchedulerManager, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.WMS, true, CultureInfo.InvariantCulture) == 0
                )
                return LoginSlotType.Gdi;

            if (
                string.Compare(processName, ProcessType.InvisibleWMS, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.InvisibleMAN, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(processName, ProcessType.InvisibleWARMAN, true, CultureInfo.InvariantCulture) == 0
                )
                return LoginSlotType.Mobile;

            if (string.Compare(processName, ProcessType.EasyLook, true, CultureInfo.InvariantCulture) == 0)
                return LoginSlotType.EasyLook;

            if (string.Compare(processName, ProcessType.MagicDocuments, true, CultureInfo.InvariantCulture) == 0)
                return LoginSlotType.MagicDocument;

            return LoginSlotType.ThirdPart;
        }
    }

    /// <summary>
    /// Summary description for AuthenticationSlots.
    /// </summary>
    //=========================================================================
    internal abstract class AuthenticationSlots
    {
        private int gdiCal = -1;
        private int gdiConcurrent = -1;
        private int wmsCal = -1;
        private int manufacturingCal = -1;
        private int easyCal = -1;
        private int mdCal = -1;
        private int tpCal = -1;

        internal bool licut = false;//licenza utente per ENT todo

        protected AuthenticationSlot[] GDISlots; //fixed size
        protected ConcurrentSlot[] GDIConcurrentSlots;//fixed size
        protected ConcurrentSlot[] WMSSlots;//fixed size
        protected ConcurrentSlot[] ManufacturingSlots;//fixed size
        protected UnlimitedSlots MDSlots;
        protected UnlimitedSlots MLSlots;
        protected UnlimitedSlots ELSlots;
        //slot per utilizzo di singola cal se login da stesso terminale.
        //protected UnlimitedSlots AddedSlots;
        private UnlimitedSlots skedulerSlots;

        protected ActivationObject activationManager = null;
        protected SqlConnection sysDBConnection = null;
        protected IDiagnostic diagnostic = null;

        protected Hashtable CALNumberForArticle = null;

        //-----------------------------------------------------------------------
        public static AuthenticationSlots Create(ActivationObject activationManager)
        {
            if (activationManager.GetEdition() == Edition.Enterprise)
                return new FunctionalSlots();

            return new GeneralSlots();
        }

        //-----------------------------------------------------------------------
        internal bool AreMobileCalAssigned(MobileCal mc)
        {
            if (mc != MobileCal.WMS && mc != MobileCal.Manufacturing) return false;

            string query = "SELECT COUNT (*) FROM MSD_LoginsArticles WHERE  Article = @Art";//  OR Article =  @Art2";

            SqlCommand aSqlCommand = new SqlCommand();

            int val = 0;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;

                aSqlCommand.Parameters.AddWithValue("@Art", mc.ToString());
                //aSqlCommand.Parameters.AddWithValue("@Art2", MobileCal.WMS.ToString());

                //se l'utente è già stato assegnato all'articolo esco con successo
                val = (int)aSqlCommand.ExecuteScalar();

            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsUserAssignedToArticle: " + err.Message);
                aSqlCommand.Dispose();
                return false;
            }

            aSqlCommand.Dispose();

            return val > 0;
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlots() { }

        //-----------------------------------------------------------------------
        public int Init(ActivationObject activationManager, SqlConnection sysDBConnection, IDiagnostic diagnostic)
        {
            this.diagnostic = diagnostic;
            this.activationManager = activationManager;
            this.sysDBConnection = sysDBConnection;

            Hashtable tpProducerCal;//numero di cal per ogni PK

            if (!activationManager.GetCalNumber(out gdiCal, out gdiConcurrent, out easyCal, out mdCal, out tpCal, out wmsCal, out manufacturingCal, out tpProducerCal, out CALNumberForArticle))
            {
                if (!diagnostic.WriteChildDiagnostic(activationManager.Diagnostic, true))
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, LoginManagerStrings.ErrLicenzaUtente);

                return (int)LoginReturnCodes.NoLicenseError;
            }
            //rinnovo tutti gli slot
            skedulerSlots = new UnlimitedSlots();
            MDSlots = new UnlimitedSlots(mdCal);
            MLSlots = new UnlimitedSlots(tpCal);
            ELSlots = new UnlimitedSlots(easyCal);
            //AddedSlots = new UnlimitedSlots();

            GDISlots = null;
            GDIConcurrentSlots = null;
            WMSSlots = null;
            ManufacturingSlots = null;

            Hashtable usedCal = new Hashtable();//per ogni funzionalità indica se usata
            foreach (DictionaryEntry de in CALNumberForArticle)
                usedCal.Add(de.Key, false);
            Hashtable mobileUsedCal = new Hashtable();//per ogni funzionalità indica se usata

            mobileUsedCal.Add(MobileCal.WMS.ToString(), false);

            mobileUsedCal.Add(MobileCal.Manufacturing.ToString(), false);

            //creo gli slot gdi
            GDISlots = new AuthenticationSlot[gdiCal];
            for (int i = 0; i < gdiCal; i++)
                GDISlots[i] = new AuthenticationSlot(LoginSlotType.Gdi, usedCal.Clone() as Hashtable);

            GDIConcurrentSlots = new ConcurrentSlot[gdiConcurrent];
            for (int i = 0; i < gdiConcurrent; i++)
                GDIConcurrentSlots[i] = new ConcurrentSlot(LoginSlotType.Gdi, usedCal.Clone() as Hashtable);

            int totmobile = wmsCal + manufacturingCal;
            WMSSlots = new ConcurrentSlot[totmobile];
            for (int i = 0; i < totmobile; i++)
                WMSSlots[i] = new ConcurrentSlot(LoginSlotType.Gdi, mobileUsedCal.Clone() as Hashtable);

            ManufacturingSlots = new ConcurrentSlot[0];//todo lascio vuoto a zero se no crea slot nulli---esiste un solo slot per le mobile, questo non usato
                                                       //for (int i = 0; i < manufacturingCal; i++)
                                                       //    ManufacturingSlots[i] = new ConcurrentSlot(LoginSlotType.Gdi, mobileUsedCal.Clone() as Hashtable);


            if (!ReserveLogins())
                return (int)LoginReturnCodes.ArticlesTableReadingFailure;

            return (int)LoginReturnCodes.NoError;
        }

        //---------------------------------------------------------------------------
        protected virtual bool ReserveLogins()
        {
            return true;
        }

        //-----------------------------------------------------------------------
        public bool IsUserLogged(int loginID)
        {
            foreach (AuthenticationSlot slot in GDISlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }

            foreach (AuthenticationSlot slot in ELSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }
            foreach (AuthenticationSlot slot in MDSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }
            foreach (AuthenticationSlot slot in MLSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }

            foreach (AuthenticationSlot slot in WMSSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }

            foreach (AuthenticationSlot slot in ManufacturingSlots)
            {
                if ((slot.LoginID == loginID) && slot.Logged)
                    return true;
            }


            return false;
        }

        //-----------------------------------------------------------------------
        protected virtual int AssignUserToLicenzaUtente(int loginID, ActivationObject activationManager, AuthenticationSlot slot, LoginSlotType calType)
        {
            return (int)LoginReturnCodes.NoError;
        }

        //-----------------------------------------------------------------------
        protected virtual bool IsToLogAsConcurrent(bool concurrent)
        {
            return concurrent;
        }

        //-----------------------------------------------------------------------
        public int Login(int loginID, string loginName, string macIp, int companyID, string companyName, string processName, bool webAllowed, bool gdiAllowed, bool concurrent, bool overwriteLogin, out string authenticationToken)
        {

            authenticationToken = string.Empty;

            if (loginID < 0 || loginName == null || loginName.Length == 0)
                return (int)LoginReturnCodes.InvalidUserError;

            if (companyID < 0 || companyName == null || companyName.Length == 0)
                return (int)LoginReturnCodes.InvalidCompanyError;

            LoginSlotType calType = AuthenticationProcess.GetCalType(processName);
            if (calType == LoginSlotType.Invalid)
                return (int)LoginReturnCodes.InvalidProcessError;


            int ok = AuthenticationProcess.IsValidProcess(processName, webAllowed, gdiAllowed || concurrent);

            if (ok != (int)LoginReturnCodes.NoError)
                return ok;

            //lo scheduler occupa i suoi slot infiniti.
            if ((string.Compare(processName, ProcessType.SchedulerAgent, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(processName, ProcessType.SchedulerManager, true, CultureInfo.InvariantCulture) == 0))
            {
                AuthenticationSlot slot = new AuthenticationSlot(LoginSlotType.Gdi, null);
                authenticationToken = Guid.NewGuid().ToString();
                slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
                skedulerSlots.Add(slot);
                return (int)LoginReturnCodes.NoError;
            }

            //webservice mobile che si logina invisibile occupando  gli slot infiniti dello scheduler
            //se esiste già uno slot loginato con processtype indicato torno il suo token e non eseguo ulteriori login
            if ((string.Compare(processName, ProcessType.InvisibleMAN, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(processName, ProcessType.InvisibleWMS, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(processName, ProcessType.InvisibleWARMAN, true, CultureInfo.InvariantCulture) == 0))
            {
                foreach (AuthenticationSlot s in skedulerSlots)
                {
                    if (s != null && String.Compare(s.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        authenticationToken = s.ApplicationTokens["Mobile"] as string;
                        return (int)LoginReturnCodes.NoError;
                    }
                }
                AuthenticationSlot slot = new AuthenticationSlot(LoginSlotType.Gdi, null);
                authenticationToken = Guid.NewGuid().ToString();
                slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
                skedulerSlots.Add(slot);
                return (int)LoginReturnCodes.NoError;
            }


            //magicLink, easylook readonly(utente solo web) e magicDocs occupano i loro slot specifici 
            if (calType == LoginSlotType.ThirdPart ||
                calType == LoginSlotType.MagicDocument ||
                (calType == LoginSlotType.EasyLook && webAllowed && !gdiAllowed && !concurrent))
                return LoginSpecificSlot(calType, loginID, loginName, companyID, companyName, processName, out authenticationToken);

            //WMS + manufacturing
            if (string.Compare(processName, ProcessType.WMS, true, CultureInfo.InvariantCulture) == 0)
            {
                return LoginWMSSlot(calType, loginID, loginName, companyID, companyName, processName, macIp, out authenticationToken);
            }

            //Se utente concurrent occupa le sue cal (con la verifica della cal che non risultano alive) ( quindi non lo sposto nel functional slot)
            if (IsToLogAsConcurrent(concurrent))
            {
                AuthenticationSlot slot = GetFreeConcurrentSlot();
                if (slot != null)
                {
                    authenticationToken = Guid.NewGuid().ToString();
                    slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
                    return (int)LoginReturnCodes.NoError;
                }
                else return (int)LoginReturnCodes.NoCalAvailableError;
            }
            return LoginSlot(calType, loginID, macIp, loginName, companyID, companyName, processName, overwriteLogin, out authenticationToken);
        }

        //-----------------------------------------------------------------------
        private AuthenticationSlot GetFreeConcurrentSlot()
        {
            FreePendingSlot();
            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
                if (!slot.Logged)
                    return slot;

            return null;
        }

        //-----------------------------------------------------------------------
        internal void FreePendingMobileSlot()
        {
            foreach (AuthenticationSlot slot in skedulerSlots)
                if (!slot.IsAlive && slot.Logged && slot.IsInvisibleWMS())
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("Slot not alive: Mobile user {0} disconnected. ", slot.LoginName));
                    slot.Clear(false);
                }

            //if (WMSSlots != null && WMSSlots.Length > 0)
            //{
            //    foreach (AuthenticationSlot slot in WMSSlots)
            //        if (!slot.IsAlive && slot.Logged)
            //        {
            //            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("Slot not alive: WMS MOBILE user {0} disconnected. ", slot.LoginName));
            //            slot.Clear(false);
            //        }
            //}

            //if (ManufacturingSlots != null && ManufacturingSlots.Length > 0)
            //{
            //    foreach (AuthenticationSlot slot in ManufacturingSlots)
            //        if (!slot.IsAlive && slot.Logged)
            //        {
            //            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("Slot not alive: WMS MOBILE user {0} disconnected. ", slot.LoginName));
            //            slot.Clear(false);
            //        }
            //}
        }

        //-----------------------------------------------------------------------
        internal void FreePendingSlot()
        {
            //Libero preventivamente tutti gli slot che risultano non alive.
            //direi che questa procedurava fatta solo per gli slot di tipo concurrent
            //perche per gli altri la procedura di sovrascrittura delle cal funziona già.
            //Questa metodologia ha un delay di 6 minuti, 
            //quindi la cal appesa si accroge di essere appesa al più tardi dopo 6 minuti.
            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
                if (!slot.IsAlive && slot.Logged)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("Slot not alive: Floating user {0} disconnected. ", slot.LoginName));
                    slot.Clear(false);
                }

        }

        //-----------------------------------------------------------------------
        internal AuthenticationSlot FreePendingWMSSlot(string mac)
        {//libero lo slot se la cal wms non è piu stata rinfrescata, devo però verificare quella non connessa da più tempo.
            AuthenticationSlot slotToFree = null;

            foreach (AuthenticationSlot slot in WMSSlots)
                if (!slot.IsAliveWMS && slot.Logged)
                {
                    if (slot.MACAddress == mac && !string.IsNullOrWhiteSpace (mac)) 
                    { 
                        slotToFree = slot;
                        break; 
                    }
                    if (slotToFree == null)
                        slotToFree = slot;
                    else
                        if (slot.LastTick >= slotToFree.LastTick) slotToFree = slot;

                }

            if (slotToFree != null)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("WMS Slot not alive: Floating user {0} disconnected. ", slotToFree.LoginName));
                slotToFree.Clear(false);
                return slotToFree;
            }
            return null;

        }
        //-----------------------------------------------------------------------
        protected int LoginWMSSlot(LoginSlotType slotType, int loginID, string loginName, int companyID, string companyName, string processName, string macip, out string authenticationToken)
        {

            authenticationToken = Guid.NewGuid().ToString();
            //prima di tutto vedo se trovo uno slot con lo stesso macip e ce lo metto dentro, dopo aver fatto logoff del token esistente se c'è.
            if (!string.IsNullOrWhiteSpace(macip))
            {
                for (int i = 0; i < WMSSlots.Length; i++)
                    if (!WMSSlots[i].Reserved && String.Compare(WMSSlots[i].MACAddress, macip, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        if (WMSSlots[i].Logged)
                        {
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("WMS Slot Unique Identifier Recognition: user {0} disconnected.", WMSSlots[i].LoginName));

                            Logout(WMSSlots[i].GetTokenByApplication(LoginSlotType.Gdi));
                            WMSSlots[i].Clear(false);
                        }
                        return LoginWMSSlot(WMSSlots[i], loginID, loginName, companyID, companyName, processName, macip, authenticationToken);
                    }
            }

            //poi vedo se ce nè uno libero
            for (int i = 0; i < WMSSlots.Length; i++)
                if (!WMSSlots[i].Logged && !WMSSlots[i].Reserved)//in realtà non è mai riservato: sono di tipo floating
                    return LoginWMSSlot(WMSSlots[i], loginID, loginName, companyID, companyName, processName, macip, authenticationToken);

            //poi vedo se se riesco ad occuparne uno liberandolo da un utente pending
            return LoginWMSSlot(FreePendingWMSSlot(macip), loginID, loginName, companyID, companyName, processName, macip, authenticationToken);

        }
        //-----------------------------------------------------------------------
        private int LoginWMSSlot(AuthenticationSlot slot, int loginID, string loginName, int companyID, string companyName, string processName, string macip, string authenticationToken)
        {
            if (slot == null) return (int)LoginReturnCodes.NoCalAvailableError;

            slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
            slot.MACAddress = macip;
            slot.IsAliveWMS = true;
            MobileCal mb = ConsumeMobileCal(loginID, authenticationToken, macip);
            if (mb == MobileCal.None)
            {
                slot.LogoutToken(authenticationToken);
                slot.Clear(false);
                return (int)LoginReturnCodes.NoCalAvailableError;
            }
            return (int)LoginReturnCodes.NoError;

        }
        //----------------------------------------------------------------------
        internal MobileCal ConsumeMobileCal(int loginID, string authenticationToken, string macip)
        {
            MobileCal mc = MobileCal.None;
            try
            {
                ArticleInfo wms = activationManager.GetWmsArticle();
                if (wms != null)
                {
                    int reswms = AssignUserToMobileArticle(loginID, authenticationToken, wms, MobileCal.WMS, macip);
                    if (reswms == 0) mc |= MobileCal.WMS;
                }

                ArticleInfo manuf = activationManager.GetManufacturingMobileArticle();
                if (manuf != null)
                {
                    int resmanuf = AssignUserToMobileArticle(loginID, authenticationToken, manuf, MobileCal.Manufacturing, macip);
                    if (resmanuf == 0) mc |= MobileCal.Manufacturing;
                }
            }
            catch { }
            return mc;
        }
        ////todo manufacturing non so se serve.... serve ma bisogna cambiare facenfo la somma delle cal
        ////-----------------------------------------------------------------------
        //protected int LoginManufacturingMobileSlot(LoginSlotType slotType, int loginID, string loginName, int companyID, string companyName, string processName, out string authenticationToken)
        //{
        //    //FreePendingWMSSlot();per ora no, francesco evita di fare troppe chiamate

        //    authenticationToken = null;
        //    for (int i = 0; i < ManufacturingSlots.Length; i++)
        //        if (!ManufacturingSlots[i].Logged && !ManufacturingSlots[i].Reserved)//in realtà non è mai riservato: sono di tipo floating
        //        {
        //            authenticationToken = Guid.NewGuid().ToString();
        //            ManufacturingSlots[i].Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
        //            return (int)LoginReturnCodes.NoError;
        //        }


        //    return (int)LoginReturnCodes.NoCalAvailableError;

        //}

        //-----------------------------------------------------------------------
        protected int LoginSpecificSlot(LoginSlotType slotType, int loginID, string loginName, int companyID, string companyName, string processName, out string authenticationToken)
        {
            authenticationToken = null;
            AuthenticationSlot slot = GetFirstAvailableSlot(slotType);
            if (slot == null)
                return (int)LoginReturnCodes.NoCalAvailableError;
            else
            {
                authenticationToken = Guid.NewGuid().ToString();
                slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, false);
                return (int)LoginReturnCodes.NoError;
            }
        }

        //-----------------------------------------------------------------------
        protected abstract int LoginSlot(LoginSlotType calType, int loginID, string macIp, string loginName, int companyID, string companyName, string processName, bool overwriteLogin, out string authenticationToken);

        //-----------------------------------------------------------------------
        protected virtual void ClearSlot(AuthenticationSlot slot)
        {
        }

        //-----------------------------------------------------------------------
        protected virtual void ClearVerticalFloatingCalTable(string token)
        {
        }

        //-----------------------------------------------------------------------
        public void Logout(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                return;

            bool reserved = false;
            for (int i = 0; i < GDISlots.Length; i++)
                if (GDISlots[i].ContainsToken(authenticationToken))
                {
                    reserved = GDISlots[i].Reserved;
                    int id = GDISlots[i].LoginID;

                    GDISlots[i].LogoutToken(authenticationToken);
                    ClearSlot(GDISlots[i]);

                    //un utente potenzialmente riservato sta facendo logout, 
                    //allora per ottimizzare vado a verificare se è riservato 
                    //e se c'è lo stesso utente (su altra company) 
                    //loginato su cal non riservata lo promuovo su questa 
                    //così libero una cal
                    if (reserved)
                    {
                        //verifico se lo stesso utente che sta facendo logout da cal riservata 
                        //sia connesso anche su cal non riservata.
                        AuthenticationSlot s = GetLoggedGdiNamedSlotNotReserved(id);
                        if (s == null)
                            return;
                        //mi tengo da parte i valori
                        string auth = s.GetTokenByApplication(s.CalType);
                        string loginName = s.LoginName;
                        int companyID = s.CompanyID;
                        string companyName = s.CompanyName;
                        string processName = s.ProcessName;
                        //elimino la login dalla cal non riservata
                        s.LogoutToken(auth);
                        s.Clear(false);
                        // riposiziono la login su quella riservata
                        GDISlots[i].Set(loginName, companyID, companyName, processName, auth);


                    }
                    return;
                }

            for (int i = 0; i < GDIConcurrentSlots.Length; i++)
                if (GDIConcurrentSlots[i].ContainsToken(authenticationToken))
                {
                    ClearVerticalFloatingCalTable(authenticationToken);
                    GDIConcurrentSlots[i].LogoutToken(authenticationToken);
                    ClearSlot(GDIConcurrentSlots[i]);
                    return;
                }

            for (int i = 0; i < WMSSlots.Length; i++)
                if (WMSSlots[i].ContainsToken(authenticationToken))
                {
                    WMSSlots[i].LogoutToken(authenticationToken);
                    ClearSlot(WMSSlots[i]);
                    return;
                }

            for (int i = 0; i < ManufacturingSlots.Length; i++)
                if (ManufacturingSlots[i].ContainsToken(authenticationToken))
                {
                    ManufacturingSlots[i].LogoutToken(authenticationToken);
                    ClearSlot(ManufacturingSlots[i]);
                    return;
                }

            for (int i = ELSlots.Count - 1; i >= 0; i--)
                if (ELSlots[i].ContainsToken(authenticationToken))
                {
                    ELSlots.Remove(ELSlots[i]);
                    return;
                }

            for (int i = MDSlots.Count - 1; i >= 0; i--)
                if (MDSlots[i].ContainsToken(authenticationToken))
                {
                    MDSlots.Remove(MDSlots[i]);
                    return;
                }

            for (int i = MLSlots.Count - 1; i >= 0; i--)
                if (MLSlots[i].ContainsToken(authenticationToken))
                {
                    MLSlots.Remove(MLSlots[i]);
                    return;
                }

            for (int i = skedulerSlots.Count - 1; i >= 0; i--)
                if (skedulerSlots[i].ContainsToken(authenticationToken))
                {
                    skedulerSlots.Remove(skedulerSlots[i]);
                    return;
                }

            //for (int i = AddedSlots.Count - 1; i >= 0; i--)
            //    if (AddedSlots[i].ContainsToken(authenticationToken))
            //    {
            //        AddedSlots.Remove(AddedSlots[i]);
            //        return;
            //    }
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlot GetLoggedGdiNamedSlotNotReserved(int loginID)
        {
            foreach (AuthenticationSlot slot in GDISlots)
                if (slot.LoginID == loginID && slot.Logged && !slot.Reserved)
                    return slot;
            return null;
        }

        //TODO
        //Nel numero degli utenti loginati bisogna comprendere 
        //anche lo stesso utente connesso più volte dalla stessa macchina?
        //-----------------------------------------------------------------------
        internal int GetLoggedUsersNumber()
        {
            int i = 0;
            foreach (AuthenticationSlot slot in GDISlots)
            {
                if (slot != null && slot.Logged)
                    i++;
            }

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
            {
                if (slot != null && slot.Logged)
                    i++;
            }

            foreach (AuthenticationSlot slot in MLSlots)
            {
                if (slot != null && slot.Logged)
                    i++;
            }
            foreach (AuthenticationSlot slot in ELSlots)
            {
                if (slot != null && slot.Logged)
                    i++;
            }
            foreach (AuthenticationSlot slot in MDSlots)
            {
                if (slot != null && slot.Logged)
                    i++;
            }

            foreach (AuthenticationSlot sslot in skedulerSlots)
            {
                if (sslot != null && sslot.Logged)
                    i++;
            }

            foreach (AuthenticationSlot wmsslot in WMSSlots)
            {
                if (wmsslot != null && wmsslot.Logged)
                    i++;
            }

            foreach (AuthenticationSlot wmsslot in ManufacturingSlots)
            {
                if (wmsslot != null && wmsslot.Logged)
                    i++;
            }

            return i;
        }

        //-----------------------------------------------------------------------
        internal int GetCompanyLoggedUsersNumber(int companyID)
        {
            int i = 0;
            foreach (AuthenticationSlot slot in GDISlots)
            {
                if (slot != null && slot.Logged && slot.CompanyID == companyID)
                    i++;
            }

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
            {
                if (slot != null && slot.Logged && slot.CompanyID == companyID)
                    i++;
            }

            foreach (AuthenticationSlot slot in ELSlots)
            {
                if (slot != null && slot.Logged && slot.CompanyID == companyID)
                    i++;
            }
            foreach (AuthenticationSlot slot in MDSlots)
            {
                if (slot != null && slot.Logged && slot.CompanyID == companyID)
                    i++;
            }
            foreach (AuthenticationSlot slot in MLSlots)
            {
                if (slot != null && slot.Logged && slot.CompanyID == companyID)
                    i++;
            }

            foreach (AuthenticationSlot sslot in skedulerSlots)
            {
                if (sslot != null && sslot.Logged && sslot.CompanyID == companyID)
                    i++;
            }

            foreach (AuthenticationSlot wmsslot in WMSSlots)
            {
                if (wmsslot != null && wmsslot.Logged && wmsslot.CompanyID == companyID)
                    i++;

            }

            foreach (AuthenticationSlot wmsslot in ManufacturingSlots)
            {
                if (wmsslot != null && wmsslot.Logged && wmsslot.CompanyID == companyID)
                    i++;

            }

            return i;
        }

        //-----------------------------------------------------------------------
        public string ToXml(bool writeTokens)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("AllUsers");
            doc.AppendChild(root);

            XmlElement gdiNode = doc.CreateElement("SmartClient");
            XmlElement easyNode = doc.CreateElement("Web");
            XmlElement magicNode = doc.CreateElement("Office");
            XmlElement tpNode = doc.CreateElement("ThirdParty");
            XmlElement concNode = doc.CreateElement("Concurrent");
            XmlElement wmsNode = doc.CreateElement("WMS");
            XmlElement mobileNode = doc.CreateElement("Mobile");

            root.AppendChild(gdiNode);
            root.AppendChild(easyNode);
            root.AppendChild(magicNode);
            root.AppendChild(tpNode);
            root.AppendChild(concNode);
            root.AppendChild(wmsNode);
            root.AppendChild(mobileNode);//mobile e scheduler

            ArrayList allSlots = new ArrayList();
            if (GDISlots != null)
                allSlots.AddRange(GDISlots);
            if (GDIConcurrentSlots != null)
                allSlots.AddRange(GDIConcurrentSlots);
            if (ELSlots != null)
                allSlots.AddRange(GetELSlots());
            if (MLSlots != null)
                allSlots.AddRange(GetMLSlots());
            if (MDSlots != null)
                allSlots.AddRange(GetMDSlots());

            foreach (AuthenticationSlot slot in allSlots)
            {
                XmlElement xUser = doc.CreateElement("User");
                slot.ToXml(xUser, writeTokens);

                XmlElement parent = null;
                switch (slot.SlotType)
                {
                    case LoginSlotType.EasyLook:
                        parent = easyNode;
                        break;
                    case LoginSlotType.Gdi:
                        if (slot is ConcurrentSlot)
                            parent = concNode;
                        else
                            parent = gdiNode;
                        break;

                    case LoginSlotType.ThirdPart:
                        parent = tpNode;
                        break;
                    case LoginSlotType.MagicDocument:
                        parent = magicNode;
                        break;

                    default:
                        continue;
                }

                parent.AppendChild(xUser);
            }

            foreach (AuthenticationSlot slot in WMSSlots)
            {
                XmlElement xUser = doc.CreateElement("User");
                slot.ToXml(xUser, writeTokens);
                wmsNode.AppendChild(xUser);
            }
            foreach (AuthenticationSlot slot in ManufacturingSlots)
            {
                XmlElement xUser = doc.CreateElement("User");
                slot.ToXml(xUser, writeTokens);
                wmsNode.AppendChild(xUser);
            }
            foreach (AuthenticationSlot slot in skedulerSlots)
            {
                XmlElement xUser = doc.CreateElement("User");
                slot.ToXml(xUser, writeTokens);
                mobileNode.AppendChild(xUser);
            }
            ////todo manuf mobile slot
            //foreach (AuthenticationSlot slot in ManufacturingSlots)
            //{
            //    XmlElement xUser = doc.CreateElement("User");
            //    slot.ToXml(xUser, writeTokens);
            //    wmsNode.AppendChild(xUser); ;
            //   }

            return doc.InnerXml;
        }

        //-----------------------------------------------------------------------
        private AuthenticationSlot[] GetSkedulerlots()
        {
            AuthenticationSlot[] list = new AuthenticationSlot[skedulerSlots.Count];
            for (int i = 0; i < skedulerSlots.Count; i++)
                list[i] = skedulerSlots[i];
            return list;
        }

        //-----------------------------------------------------------------------
        protected AuthenticationSlot[] GetMDSlots()
        {
            AuthenticationSlot[] list = new AuthenticationSlot[MDSlots.Count];
            for (int i = 0; i < MDSlots.Count; i++)
                list[i] = MDSlots[i];
            return list;
        }
        //-----------------------------------------------------------------------
        protected AuthenticationSlot[] GetMLSlots()
        {
            AuthenticationSlot[] list = new AuthenticationSlot[MLSlots.Count];
            for (int i = 0; i < MLSlots.Count; i++)
                list[i] = MLSlots[i];
            return list;
        }
        //-----------------------------------------------------------------------
        protected AuthenticationSlot[] GetELSlots()
        {
            AuthenticationSlot[] list = new AuthenticationSlot[ELSlots.Count];
            for (int i = 0; i < ELSlots.Count; i++)
                list[i] = ELSlots[i];
            return list;
        }

        /// <summary>
        /// ritorna il primo slot disponibile//esclusi i concurrent  
        /// </summary>
        /// <returns></returns>
        //-----------------------------------------------------------------------
        protected AuthenticationSlot GetFirstAvailableSlot(LoginSlotType slotType)
        {
            if (slotType == LoginSlotType.Gdi)
            {
                for (int i = 0; i < GDISlots.Length; i++)
                    if (!GDISlots[i].Logged && !GDISlots[i].Reserved)
                        return GDISlots[i];
            }

            if (slotType == LoginSlotType.EasyLook)
            {
                AuthenticationSlot elSlot = new AuthenticationSlot(LoginSlotType.EasyLook, null);
                if (ELSlots.Add(elSlot))
                    return elSlot;
                return null;
            }

            if (slotType == LoginSlotType.ThirdPart)
            {
                AuthenticationSlot mlSlot = new AuthenticationSlot(LoginSlotType.ThirdPart, null);
                if (MLSlots.Add(mlSlot))
                    return mlSlot;
                return null;
            }

            if (slotType == LoginSlotType.MagicDocument)
            {
                AuthenticationSlot mdSlot = new AuthenticationSlot(LoginSlotType.MagicDocument, null);
                if (MDSlots.Add(mdSlot))
                    return mdSlot;
                return null;
            }

            return null;
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlot GetLoggedGDISlot(int loginID, int companyID)
        {
            foreach (AuthenticationSlot slot in GDISlots)
                if (slot.LoginID == loginID && slot.CompanyID == companyID && slot.Logged)
                    return slot;
            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
                if (slot.LoginID == loginID && slot.CompanyID == companyID && slot.Logged)
                    return slot;
            return null;
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlot GetSlot(string authenticationToken, bool refresh)
        {
            AuthenticationSlot slot = GetSlot(authenticationToken);
            if (slot != null && refresh)
                slot.IsAlive = true;
            return slot;
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlot GetConcurrentSlot(string authenticationToken)
        {
            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }
            return null;

        }
        //dati compnay user della login del mago-infinity io cerco fra gli slot gdi se esiste un tale utente loggato
        //-----------------------------------------------------------------------
        public string GetSlot_InfinityMode_AutTok(int loginid, int companyid)
        {
            foreach (AuthenticationSlot slot in GDISlots)
                if (slot.CompanyID == companyid && slot.LoginID == loginid)
                    return slot.ApplicationTokens["Gdi"] as string;

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)

                if (slot.CompanyID == companyid && slot.LoginID == loginid)
                    return slot.ApplicationTokens["Gdi"] as string;

            return null;
        }

        //dati company user della login del mago-infinity io cerco fra gli slot gdi se esiste un tale utente loggato
        //-----------------------------------------------------------------------
        public AuthenticationSlot GetSlotBySSOID(string ssoid)
        {
            foreach (AuthenticationSlot slot in GDISlots)
                if (String.Compare(slot.ssologinDataBag.SSOID, ssoid, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return slot;

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)

                if (String.Compare(slot.ssologinDataBag.SSOID, ssoid, StringComparison.InvariantCultureIgnoreCase) == 0)

                    return slot;

            return null;
        }

       
        //-----------------------------------------------------------------------
        public AuthenticationSlot GetSlot(string authenticationToken)
        {
            foreach (AuthenticationSlot slot in GDISlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            foreach (AuthenticationSlot slot in WMSSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            foreach (AuthenticationSlot slot in ManufacturingSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            foreach (AuthenticationSlot slot in ELSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }
            foreach (AuthenticationSlot slot in MDSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }
            foreach (AuthenticationSlot slot in MLSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            foreach (AuthenticationSlot slot in skedulerSlots)
            {
                if (slot.ContainsToken(authenticationToken))
                    return slot;
            }

            //foreach (AuthenticationSlot slot in AddedSlots)
            //{
            //    if (slot.ContainsToken(authenticationToken))
            //        return slot;
            //}

            return null;
        }

        //----------------------------------------------------------------------
        internal int GetTokenProcessType(string token)
        {
            AuthenticationSlot slot = GetSlot(token);

            string app = slot.GetApplicationByToken(token);
            LoginSlotType slotType = AuthenticationSlot.GetSlotTypeFromApplicationName(app);
            return (int)slotType;
        }

        //-----------------------------------------------------------------------
        internal void RemoveUser(int loginID)
        {
            for (int i = 0; i < GDISlots.Length; i++)
                if (GDISlots[i] != null && GDISlots[i].LoginID == loginID)
                    GDISlots[i].Clear(false);

            for (int i = 0; i < GDIConcurrentSlots.Length; i++)
                if (GDIConcurrentSlots[i] != null && GDIConcurrentSlots[i].LoginID == loginID)
                    GDIConcurrentSlots[i].Clear(false);

            for (int n = ELSlots.Count - 1; n >= 0; n--)
                if (ELSlots[n] != null && ELSlots[n].LoginID == loginID)
                    ELSlots.Remove(ELSlots[n]);

            for (int n = MDSlots.Count - 1; n >= 0; n--)
                if (MDSlots[n] != null && MDSlots[n].LoginID == loginID)
                    MDSlots.Remove(MDSlots[n]);

            for (int n = MLSlots.Count - 1; n >= 0; n--)
                if (MLSlots[n] != null && MLSlots[n].LoginID == loginID)
                    MLSlots.Remove(MLSlots[n]);

            for (int n = skedulerSlots.Count - 1; n >= 0; n--)
                if (skedulerSlots[n] != null && skedulerSlots[n].LoginID == loginID)
                    skedulerSlots.Remove(skedulerSlots[n]);

            for (int i = 0; i < WMSSlots.Length; i++)
                if (WMSSlots[i] != null && WMSSlots[i].LoginID == loginID)
                    WMSSlots[i].Clear(false);

            for (int i = 0; i < ManufacturingSlots.Length; i++)
                if (ManufacturingSlots[i] != null && ManufacturingSlots[i].LoginID == loginID)
                    ManufacturingSlots[i].Clear(false);


            //for (int n = AddedSlots.Count - 1; n >= 0; n--)
            //    if (AddedSlots[n] != null && AddedSlots[n].LoginID == loginID)
            //        AddedSlots.Remove(AddedSlots[n]);
        }

        //-----------------------------------------------------------------------
        internal void RemoveCompany(int companyID)
        {
            for (int i = 0; i < GDISlots.Length; i++)
                if (GDISlots[i] != null && GDISlots[i].CompanyID == companyID)
                    GDISlots[i].Clear(true);

            for (int i = 0; i < GDIConcurrentSlots.Length; i++)
                if (GDIConcurrentSlots[i] != null && GDIConcurrentSlots[i].CompanyID == companyID)
                    GDIConcurrentSlots[i].Clear(true);

            for (int n = ELSlots.Count - 1; n >= 0; n--)
                if (ELSlots[n] != null && ELSlots[n].CompanyID == companyID)
                    ELSlots.Remove(ELSlots[n]);

            for (int n = MDSlots.Count - 1; n >= 0; n--)
                if (MDSlots[n] != null && MDSlots[n].CompanyID == companyID)
                    MDSlots.Remove(MDSlots[n]);

            for (int n = MLSlots.Count - 1; n >= 0; n--)
                if (MLSlots[n] != null && MLSlots[n].CompanyID == companyID)
                    MLSlots.Remove(MLSlots[n]);

            for (int n = skedulerSlots.Count - 1; n >= 0; n--)
                if (skedulerSlots[n] != null && skedulerSlots[n].CompanyID == companyID)
                    skedulerSlots.Remove(skedulerSlots[n]);

            for (int i = 0; i < WMSSlots.Length; i++)
                if (WMSSlots[i] != null && WMSSlots[i].CompanyID == companyID)
                    WMSSlots[i].Clear(true);

            for (int i = 0; i < ManufacturingSlots.Length; i++)
                if (ManufacturingSlots[i] != null && ManufacturingSlots[i].CompanyID == companyID)
                    ManufacturingSlots[i].Clear(true);


            //for (int n = AddedSlots.Count -1 ; n >= 0 ; n--)
            //    if (AddedSlots[n] != null && AddedSlots[n].CompanyID == companyID)
            //        AddedSlots.Remove(AddedSlots[n]);
        }

        //-----------------------------------------------------------------------
        internal void RemoveAssociation(int loginID, int companyID)
        {
            for (int i = 0; i < GDISlots.Length; i++)
                if (GDISlots[i] != null && GDISlots[i].CompanyID == companyID && GDISlots[i].LoginID == loginID)
                    GDISlots[i].Clear(true);

            for (int i = 0; i < GDIConcurrentSlots.Length; i++)
                if (GDIConcurrentSlots[i] != null && GDIConcurrentSlots[i].CompanyID == companyID && GDIConcurrentSlots[i].LoginID == loginID)
                    GDIConcurrentSlots[i].Clear(true);

            for (int n = ELSlots.Count - 1; n >= 0; n--)
                if (ELSlots[n] != null && ELSlots[n].CompanyID == companyID && ELSlots[n].LoginID == loginID)
                    ELSlots.Remove(ELSlots[n]);

            for (int n = MDSlots.Count - 1; n >= 0; n--)
                if (MDSlots[n] != null && MDSlots[n].CompanyID == companyID && MDSlots[n].LoginID == loginID)
                    MDSlots.Remove(MDSlots[n]);

            for (int n = MLSlots.Count - 1; n >= 0; n--)
                if (MLSlots[n] != null && MLSlots[n].CompanyID == companyID && MLSlots[n].LoginID == loginID)
                    MLSlots.Remove(MLSlots[n]);

            for (int n = skedulerSlots.Count - 1; n >= 0; n--)
                if (skedulerSlots[n] != null && skedulerSlots[n].CompanyID == companyID && skedulerSlots[n].LoginID == loginID)
                    skedulerSlots.Remove(skedulerSlots[n]);

            for (int i = 0; i < WMSSlots.Length; i++)
                if (WMSSlots[i] != null && WMSSlots[i].CompanyID == companyID && WMSSlots[i].LoginID == loginID)
                    WMSSlots[i].Clear(true);

            for (int i = 0; i < ManufacturingSlots.Length; i++)
                if (ManufacturingSlots[i] != null && ManufacturingSlots[i].CompanyID == companyID && ManufacturingSlots[i].LoginID == loginID)
                    ManufacturingSlots[i].Clear(true);

            //for (int n = AddedSlots.Count - 1; n >= 0; n--)
            //    if (AddedSlots[n] != null && AddedSlots[n].CompanyID == companyID && AddedSlots[n].LoginID == loginID)
            //        AddedSlots.Remove(AddedSlots[n]);
        }

        //----------------------------------------------------------------------
        public int AssignUserToArticle(int loginID, string token, string articleName)
        {

            if (activationManager == null)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AssignUserToArticle: activationManager is null");
                return (int)LoginReturnCodes.ActivationManagerInitializationFailure;
            }

            ArticleInfo ai = activationManager.GetArticleByName(articleName);
            if (ai == null)
                return (int)LoginReturnCodes.NoLicenseError;

            return AssignUserToArticle(loginID, token, ai);
        }

        //----------------------------------------------------------------------
        public abstract int AssignUserToArticle(int loginID, string token, ArticleInfo article);
        //----------------------------------------------------------------------
        public abstract int AssignUserToMobileArticle(int loginID, string token, ArticleInfo article, MobileCal mc, string macip);
		
		//----------------------------------------------------------------------
		protected abstract bool IsUserAssignedToArticle(int loginID, string token, string articleName);

        //----------------------------------------------------------------------
        internal abstract void RefreshFloatingMark();

        //----------------------------------------------------------------------
        private void DeleteMobile()
        {
            SqlCommand sqlCommand = new SqlCommand();

            sqlCommand.Connection = sysDBConnection;
            try
            {

                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles  WHERE  Article = @Art  OR Article =  @Art2";
                sqlCommand.Parameters.AddWithValue("@Art", MobileCal.Manufacturing.ToString());
                sqlCommand.Parameters.AddWithValue("@Art2", MobileCal.WMS.ToString());

                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException)
            {
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
            }
        }

        //----------------------------------------------------------------------
        internal void ClearMobileCalAssignment(MobileCal mc)
        {
            SqlCommand sqlCommand = new SqlCommand();

            try
            {
                if ((int)mc == 0) return;
                if ((int)mc == 3)
                {
                    DeleteMobile();
                    return;
                }

                sqlCommand.Connection = sysDBConnection;
                sqlCommand.CommandText = null;
                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles  WHERE  Article = @Art";
                sqlCommand.Parameters.AddWithValue("@Art", mc.ToString());
                sqlCommand.ExecuteNonQuery();
            }

            catch (SqlException)
            {
            }
            finally
            {
                if (sqlCommand != null)
                    sqlCommand.Dispose();
            }
        }

    }

    //-----------------------------------------------------------------------
    internal class GeneralSlots : AuthenticationSlots
    {
        //-----------------------------------------------------------------------
        protected AuthenticationSlot GetReserved(int loginID)
        {
            foreach (AuthenticationSlot slot in GDISlots)
            {
                if (slot.LoginID == loginID && slot.Reserved)
                    return slot;
            }

            return null;
        }
        //----------------------------------------------------------------------
        internal override void RefreshFloatingMark()
        {
            Init(this.activationManager, this.sysDBConnection, this.diagnostic);
        }
        /// <remarks>
        /// arriviamo qui solo se la login è di tipo GDI o EasyLook con utente GDI
        /// </remarks>
        //-----------------------------------------------------------------------
        protected override int LoginSlot(LoginSlotType calType, int loginID, string macIp, string loginName, int companyID, string companyName, string processName, bool overwriteLogin, out string authenticationToken)
        {
            authenticationToken = null;
            AuthenticationSlot slot = GetReserved(loginID);

            #region esiste slot riservato
            if (slot != null)//gdi riservata
            {
                //è presente uno slot riservato all'utente
                if (!slot.Logged)//non loginato
                {
                    //lo slot è disponibile quindo lo utilizzo
                    authenticationToken = Guid.NewGuid().ToString();
                    slot.Set(loginName, companyID, companyName, processName, authenticationToken);//non setto login id, è già uno slot riservato
                    return (int)LoginReturnCodes.NoError;
                }
                else//lo slot è già occupato
                {
                    //se arriva la login dalla stessa macchina per lo stesso utente allora devo poter accedere alla stessa cal.
                    /*if (true)
					{
						authenticationToken = Guid.NewGuid().ToString();
						slot.AddToken(loginName, companyID, companyName, processName, authenticationToken);//non setto login id, è già uno slot riservato
						return (int)LoginReturnCodes.NoError;
					}*/
                    //devo sovrascriverlo, solo se usano stessa company, se valorizzato!
                    if (slot.CompanyID == -1 || slot.CompanyID == companyID)
                    {
                        if (!overwriteLogin)//se non sovrascrivo torno utente già loginato
                            return (int)LoginReturnCodes.UserAlreadyLoggedError;

                        else//sovrascrivo su uno slot sicuramente di tipo gdi, perchè riservato
                        {
                            if (calType == LoginSlotType.EasyLook && slot.CalType == LoginSlotType.EasyLook)
                            //se è easylook e la login era easylook torno lo stesso token
                            {
                                authenticationToken = slot.GetTokenByApplication(calType);
                                if (string.IsNullOrEmpty(authenticationToken))
                                    // errore nella gestione delle cal, non c'è il token dove mi aspetto di trovarlo
                                    return (int)LoginReturnCodes.CalManagementError;

                                return (int)LoginReturnCodes.NoError;
                            }

                            authenticationToken = Guid.NewGuid().ToString();
                            slot.Set(loginName, companyID, companyName, processName, authenticationToken);//non setto login id, è già uno slot riservato
                            return (int)LoginReturnCodes.NoError;
                        }
                    }
                }
            }
            #endregion   
            #region NON esiste slot riservato o loginato con altra company
            //else // gdi non riservata o riservata occupata da altra company
            {
                //Non sono presenti slot riservati all'utente\company liberi
                //cerco se è presente una licenza libera gdi

                //cerco prima se lo stesso utente è già connesso con easylook su GDI(EL non assegna quindi potrebbe esserci un altro loginato)
                slot = GetLoggedGDISlot(loginID, companyID);
                if (slot != null)//esiste un gdi già occupato dallo stesso ut-comp
                {
                    if (!overwriteLogin)
                        return (int)LoginReturnCodes.UserAlreadyLoggedError;

                    else if (calType == LoginSlotType.EasyLook && slot.CalType == LoginSlotType.EasyLook)//login el su el
                                                                                               //se è easylook e la login era easylook torno lo stesso token
                    {
                        authenticationToken = slot.GetTokenByApplication(calType);
                        if (string.IsNullOrEmpty(authenticationToken))
                            // errore nella gestione delle cal, non c'è il token dove mi aspetto di trovarlo
                            return (int)LoginReturnCodes.CalManagementError;
                        return (int)LoginReturnCodes.NoError;
                    }
                    else if (calType == LoginSlotType.Gdi) //gdi su el 
                    {
                        authenticationToken = Guid.NewGuid().ToString();
                        bool reserve = GetReserved(loginID) == null;
                        //solo se non esiste già un riservato per questo utente, se riservo devo anche scriverlo sul db.... TODO
                        slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, reserve);

                        return (int)LoginReturnCodes.NoError;
                    }
                }

                slot = GetFirstAvailableSlot(LoginSlotType.Gdi);
                if (slot != null)
                {
                    if (calType == LoginSlotType.Gdi)//riservo solo se login gdi
                    {
                        int autaRes = AssignUserToLicenzaUtente(loginID, activationManager, slot, calType);
                        if (autaRes != (int)LoginReturnCodes.NoError)
                            return autaRes;
                    }

                    authenticationToken = Guid.NewGuid().ToString();
                    slot.Set(loginID, loginName, companyID, companyName, processName, authenticationToken, slot.Reserved);
                    return (int)LoginReturnCodes.NoError;
                }
                else
                {
                    if (calType == LoginSlotType.EasyLook)
                        return LoginSpecificSlot(LoginSlotType.EasyLook, loginID, loginName, companyID, companyName, processName, out authenticationToken);
                    return (int)LoginReturnCodes.NoCalAvailableError;
                }
            }
            #endregion 
        }

        //-----------------------------------------------------------------------
        private bool CanUserUseFunctionalityMobile(int loginID, MobileCal mc)
        {
 
            string query = "SELECT COUNT (LoginID) FROM MSD_LoginsArticles WHERE LoginID = @LoginID And Article = @Art ";

            SqlCommand aSqlCommand = new SqlCommand();

            int val = 0;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@LoginID", loginID);
                aSqlCommand.Parameters.AddWithValue("@Art", mc.ToString());


                //se l'utente è già stato assegnato all'articolo esco con successo
                val = (int)aSqlCommand.ExecuteScalar();



            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsUserAssignedToArticle: " + err.Message);
                aSqlCommand.Dispose();
                return false;
            }

            aSqlCommand.Dispose();

            return val > 0;
        }

        //-----------------------------------------------------------------------
        protected override int AssignUserToLicenzaUtente(int loginID, ActivationObject activationManager, AuthenticationSlot slot, LoginSlotType calType)
        {
            ArticleInfo articleInfo = activationManager.GetUserLicenceArticle();

            int autaRes = 0;
            if (calType == LoginSlotType.Gdi)
            {
                //se l'utente è già assegnato non gli riservo anche questo slot
                if (IsUserAssignedToArticle(loginID, string.Empty, articleInfo.Name))
                    return autaRes;
                autaRes = AssignUserToArticle(loginID, string.Empty, articleInfo);
                if (autaRes != (int)LoginReturnCodes.NoError)
                {
                    diagnostic.Set(DiagnosticType.Warning, "UserAssignmentToArticleFailure");
                    return autaRes;
                }
                if (GetReserved(loginID) == null)
                    //solo se non ci sono già slot riservati per quell'utente (magari non nel db ma in memoria...)                
                    slot.Reserved = true;

            }

            return autaRes;
        }

        /// <summary>
        /// Deve essere chiamata nella init pre prenotare gli slot per gli utenti gdi
        /// presenti nella loginarticles
        /// </summary>
        /// <param name="loginID"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        //-----------------------------------------------------------------------
        protected bool Reserve(int loginID)
        {
            AuthenticationSlot slot = GetFirstAvailableSlot(LoginSlotType.Gdi);
            if (slot == null)
                return false;

            slot.Set(loginID, true);

            return true;
        }

        //---------------------------------------------------------------------------
        protected override bool ReserveLogins()
        {
            ArticleInfo art = activationManager.GetUserLicenceArticle();
            if (art == null)
                return false;

            if (art.IncorrectSerialsMessage != null)
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, art.IncorrectSerialsMessage);

            //riempio l'array degli utenti con quelli già configurati nel DB
            List<int> list = GetUserForArticle(art.Name);
            foreach (int loginID in list) Reserve(loginID);

            return true;
        }

        //----------------------------------------------------------------------
        public List<int> GetUserForArticle(string articleName)
        {

            string query = "SELECT LoginId FROM MSD_LoginsArticles WHERE Article = @Art";
            List<int> list = new List<int> { };

            SqlCommand aSqlCommand = new SqlCommand();
            SqlDataReader aSqlDataReader = null;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@Art", articleName);
                aSqlDataReader = aSqlCommand.ExecuteReader();

                while (aSqlDataReader.Read())
                {
                    int loginID = (int)aSqlDataReader[LoginManagerStrings.LoginID];
                    list.Add(loginID);
                }
            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetUserForArticle: " + err.Message);
                return list;
            }
            finally
            {
                if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                    aSqlDataReader.Close();

                aSqlCommand.Dispose();

            }
            return list;
        }

        //----------------------------------------------------------------------
        private int GetAvailableCALForArticle(string articleName)
        {
            int maxArticleCAL = (int)CALNumberForArticle[articleName];
            int usedArticleCAL = GetUsedCALForArticle(articleName);

            return (maxArticleCAL - usedArticleCAL);
        }
        //----------------------------------------------------------------------
        private int GetUsedCALForArticle(string articleName)
        {
            int used = 0;

            foreach (AuthenticationSlot authSlot in WMSSlots)
            {
                if (authSlot.UsedCALForArticle == null) continue;
                bool isUsed = (bool)authSlot.UsedCALForArticle[articleName];
                if (isUsed)
                    used++;
            }

            return used;
        }

        //----------------------------------------------------------------------
        internal bool IsUserAssignedToArticleMobile(int loginID, string token, string articleName)
        {
            if (articleName == string.Empty)
                return false;

            AuthenticationSlot slot = GetSlot(token);
            return (slot != null &&
                    slot.UsedCALForArticle != null &&
                    (bool)slot.UsedCALForArticle[articleName]);

        }

        //----------------------------------------------------------------------
        public override int AssignUserToMobileArticle(int loginID, string token, ArticleInfo article, MobileCal mc,string macip)
        {
          
                if (IsUserAssignedToArticleMobile(loginID, token, mc.ToString()))
                    return (int)LoginReturnCodes.NoError;
            bool assigned = AreMobileCalAssigned(mc);
            //se ce ne sono lo assegno
            //dopo aver verificato che l'utente possa usarlo
            if (assigned && !CanUserUseFunctionalityMobile(loginID, mc))
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("User {0} is not allowed to use  Mobile {1}.", loginID, article.Name));
                return (int)LoginReturnCodes.UserNotAllowed;
            }
            //se non lo sta usando verifico se ci sono CAL disponibili
            int availableCAL = GetAvailableCALForArticle(mc.ToString());

            //se non ce ne sono guardo se ci sono degli slot pending... e li purgo
            if (availableCAL <= 0)
            {
                FreePendingWMSSlot(macip);
                availableCAL = GetAvailableCALForArticle(mc.ToString());
                if (availableCAL <= 0)
                    return (int)LoginReturnCodes.NoCalAvailableError;
            }

           

            AuthenticationSlot slot = GetSlot(token);
            if (slot == null)
                return (int)LoginReturnCodes.UserAssignmentToArticleFailure;

            if (slot.UsedCALForArticle != null)
                slot.UsedCALForArticle[mc.ToString()] = true;

            return (int)LoginReturnCodes.NoError;
            /*

                        if(mc==MobileCal.WMS)
                             return AssignUserToArticle(loginID, token, article.Name, article.WmsMobileCalNumber);
                        if(mc==MobileCal.Manufacturing)
                            return AssignUserToArticle(loginID, token, article.Name, article.ManufacturingMobileCalNumber);

                          return AssignUserToArticle(loginID, token, article.Name, article.NamedCalNumber);*/
        }

        //----------------------------------------------------------------------
        public override int AssignUserToArticle(int loginID, string token, ArticleInfo article)
        {
            if (article == null)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AssignUserToArticle: article is null");
                return (int)LoginReturnCodes.NoLicenseError;
            }

            return AssignUserToArticle(loginID, token, article.Name, article.NamedCalNumber);
        }

        List<VerticalFloatingCalTableItem> VerticalFloatingCalTable = new List<VerticalFloatingCalTableItem>();
        //----------------------------------------------------------------------
        private int AssignUserToArticle(int loginID, string token, string articleName, int calNumber)
        {


            //se l'utente è associato ad una cal non devo riassegnarlo
            if (IsUserAssignedToArticle(loginID, token, articleName))
                return (int)LoginReturnCodes.NoError;

            //utente non associato
            //verifico se ci sono cal disponibili
            int calUsed = 0;
            string query = "SELECT COUNT(*) FROM MSD_LoginsArticles WHERE Article = @Art";
            SqlCommand aSqlCommand = new SqlCommand();
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@Art", articleName);

                calUsed = (int)aSqlCommand.ExecuteScalar();
            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AssignUserToArticle: " + err.Message);
                return (int)LoginReturnCodes.ArticlesTableReadingFailure;
            }

            string artname = articleName.ToLower();
            FreePendingVerticalSlot();
            VerticalFloatingCalTableItem vfct = new VerticalFloatingCalTableItem(artname, token);
            int a = 0;
            if (VerticalFloatingCalTable.Contains(vfct)) return (int)LoginReturnCodes.NoError;
            foreach (VerticalFloatingCalTableItem v in VerticalFloatingCalTable)
            {
                if (String.Compare(v.ArtName, artname, StringComparison.InvariantCultureIgnoreCase) == 0)
                    a++;
            }
            int cal = 3;
            if (activationManager.IsDevelopmentIU()) cal = 999;
            if (!activationManager.IsSpecial() || calNumber > 0) cal = calNumber;
            //non ci sono cal libere per l'articolo specificato.
            if ((calUsed + a) >= cal)
            {
                // diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AssignUserToArticle: occupied cal not reserved = " +a.ToString() );
                return (int)LoginReturnCodes.NoCalAvailableError;
            }

            foreach (AuthenticationSlot slot in GDIConcurrentSlots)
                if ((slot.ContainsToken(token)) && slot.Logged)
                {
                    VerticalFloatingCalTable.Add(vfct);
                    return (int)LoginReturnCodes.NoError;
                }

            //associo l'utente al l'articolo.
            query = "INSERT INTO MSD_LoginsArticles (Article, LoginID) VALUES (@Art, @LoginID)";

            aSqlCommand = new SqlCommand();
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@Art", articleName);
                aSqlCommand.Parameters.AddWithValue("@LoginID", loginID);

                aSqlCommand.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AssignUserToArticle: " + err.Message);
                return (int)LoginReturnCodes.ArticlesTableReadingFailure;
            }

            return (int)LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        private void FreePendingVerticalSlot()
        {
            if (VerticalFloatingCalTable == null || VerticalFloatingCalTable.Count == 0) return;

            for (int i = VerticalFloatingCalTable.Count - 1; i >= 0; i--)
            {
                if (VerticalFloatingCalTable[i] == null) continue;

                if (GetConcurrentSlot(VerticalFloatingCalTable[i].Token) == null)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "A vertical slot occupied by an orphan token was released!");

                    VerticalFloatingCalTable.Remove(VerticalFloatingCalTable[i]);
                }
            }
        }


        //----------------------------------------------------------------------
        protected override bool IsUserAssignedToArticle(int loginID, string token, string articleName)
        {
            string query = "SELECT COUNT(*) FROM MSD_LoginsArticles WHERE LoginID = @LoginID And Article = @Art";

            SqlCommand aSqlCommand = new SqlCommand();

            int val = 0;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@LoginID", loginID);
                aSqlCommand.Parameters.AddWithValue("@Art", articleName);

                //se l'utente è già stato assegnato all'articolo esco con successo
                val = (int)aSqlCommand.ExecuteScalar();
            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsUserAssignedToArticle: " + err.Message);
                aSqlCommand.Dispose();
                return false;
            }

            aSqlCommand.Dispose();

            return val > 0;
        }

        //-----------------------------------------------------------------------
        protected override void ClearVerticalFloatingCalTable(string token)
        {
            if (VerticalFloatingCalTable != null && VerticalFloatingCalTable.Count > 0)
                for (int i = VerticalFloatingCalTable.Count - 1; i >= 0; i--)
                {
                    VerticalFloatingCalTableItem v = VerticalFloatingCalTable[i] as VerticalFloatingCalTableItem;
                    if (v == null) continue;
                    if (string.Compare(v.Token, token) == 0)
                        VerticalFloatingCalTable.Remove(v);
                }
        }

        //-----------------------------------------------------------------------
        protected override void ClearSlot(AuthenticationSlot slot)
        {
            if (!slot.Logged)
                slot.Clear(slot.Reserved);

        }
    }

    /// <remarks>
    /// le cal di ent sono floating, cioè non vengono riservate all'utente che per primo le ha prese, 
    /// anche se non è loginato, e inoltre permettono allo stesso utente di loginarsi più volte contemporaneamente 
    /// occupando tante cal quanti sono gli utenti connessi.
    /// </remarks> 
    //=========================================================================
    internal class FunctionalSlots : AuthenticationSlots
    {

        //-----------------------------------------------------------------------
        protected override void ClearSlot(AuthenticationSlot slot)
        {
            if (!slot.Logged)
                slot.Clear(false);
        }

        //-----------------------------------------------------------------------
        protected override int LoginSlot(LoginSlotType calType, int loginID, string macIp, string loginName, int companyID, string companyName, string processName, bool overwriteLogin, out string authenticationToken)
        {
            authenticationToken = string.Empty;
            return (int)LoginReturnCodes.NoCalAvailableError;
        }

        //-----------------------------------------------------------------------
        protected override bool IsToLogAsConcurrent(bool concurrent)
        {
            return true;
        }

        //----------------------------------------------------------------------
        private int GetUsedCALForArticle(string articleName)
        {
            int used = 0;

            foreach (AuthenticationSlot authSlot in GDIConcurrentSlots)
            {
                if (authSlot.UsedCALForArticle == null) continue;
                if (authSlot.UsedCALForArticle[articleName] == null) continue;
                bool isUsed = (bool)authSlot.UsedCALForArticle[articleName];
                if (isUsed)
                    used++;
            }

            foreach (AuthenticationSlot authSlot in WMSSlots)
            {
                if (authSlot.UsedCALForArticle == null) continue;
                if (authSlot.UsedCALForArticle[articleName] == null) continue;

                bool isUsed = (bool)authSlot.UsedCALForArticle[articleName];
                if (isUsed)
                    used++;
            }

            return used;
        }

        //----------------------------------------------------------------------
        private int GetAvailableCALForArticle(string articleName)
        {
            if (CALNumberForArticle.ContainsKey(articleName))
            {
                int maxArticleCAL = (int)CALNumberForArticle[articleName];
                int usedArticleCAL = GetUsedCALForArticle(articleName);

                return (maxArticleCAL - usedArticleCAL);
            }
            return 0;

        }

        //----------------------------------------------------------------------
        public override int AssignUserToMobileArticle(int loginID, string token, ArticleInfo article, MobileCal mc, string macip)
        {
            if (IsUserAssignedToArticle(loginID, token, mc.ToString()))
                    return (int)LoginReturnCodes.NoError;
            bool assigned = AreMobileCalAssigned(mc);

            //se ce ne sono lo assegno
            //dopo aver verificato che l'utente possa usarlo
            if (assigned && !CanUserUseFunctionalityMobile(loginID, mc))
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("User {0} is not allowed to use {1}.", loginID, article.Name));
                return (int)LoginReturnCodes.UserNotAllowed;
            }
            //se non lo sta usando verifico se ci sono CAL disponibili
            int availableCAL = GetAvailableCALForArticle(mc.ToString());

            //se non ce ne sono guardo se ci sono degli slot pending... e li purgo
            if (availableCAL <= 0)
            {
                FreePendingWMSSlot(macip); 
                availableCAL = GetAvailableCALForArticle(mc.ToString());
                if (availableCAL <= 0)
                    return (int)LoginReturnCodes.NoCalAvailableError;
            }

          

            AuthenticationSlot slot = GetSlot(token);
            if (slot == null)
                return (int)LoginReturnCodes.UserAssignmentToArticleFailure;

            if (slot.UsedCALForArticle != null)
                slot.UsedCALForArticle[mc.ToString()] = true;

            return (int)LoginReturnCodes.NoError;
        }

        //-----------------------------------------------------------------------
        private bool CanUserUseFunctionalityMobile(int loginID, MobileCal mc)
        {
            string query = "SELECT COUNT (LoginID) FROM MSD_LoginsArticles WHERE LoginID = @LoginID And Article = @Art ";

            SqlCommand aSqlCommand = new SqlCommand();

            int val = 0;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@LoginID", loginID);
                aSqlCommand.Parameters.AddWithValue("@Art", mc.ToString());


                //se l'utente è già stato assegnato all'articolo esco con successo
                val = (int)aSqlCommand.ExecuteScalar();



            }
            catch (Exception)
            {
                //diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsUserAssignedToArticle: " + err.Message);
                aSqlCommand.Dispose();
                return false;
            }

            aSqlCommand.Dispose();

            return val > 0;
        }

        ////assegnazione dell'autToken alla funzionalità
        //----------------------------------------------------------------------
        public override int AssignUserToArticle(int loginID, string token, ArticleInfo article)
        {
            FreePendingSlot();
            //controllo se l'utente sta gia'utilizzando l'articolo specificato
            if (IsUserAssignedToArticle(loginID, token, article.Name))
                return (int)LoginReturnCodes.NoError;

            //se non lo sta usando verifico se ci sono CAL disponibili
            int availableCAL = GetAvailableCALForArticle(article.Name);

            if (availableCAL <= 0)
                return (int)LoginReturnCodes.NoCalAvailableError;


            //se ce ne sono lo assegno
            //dopo aver verificato che l'utente possa usarlo
            if (!CanUserUseFunctionality(loginID, article.Name))
                //{
                //    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("User {0} is not allowed to use {1}.", loginID, article.Name));
                return (int)LoginReturnCodes.UserNotAllowed;
            //}

            AuthenticationSlot slot = GetSlot(token);
            if (slot == null)
                return (int)LoginReturnCodes.UserAssignmentToArticleFailure;

            if (slot.UsedCALForArticle != null)
                slot.UsedCALForArticle[article.Name] = true;

            return (int)LoginReturnCodes.NoError;
        }


        //----------------------------------------------------------------------
        private bool EvaluateFloatingMark()
        {
            if (floatingMark != FloatingMark.NONE)
                return floatingMark == FloatingMark.YES;

            SqlCommand sqlCommand = new SqlCommand();
            int recordsCount = 0;
            try
            {
                //addwithvalue?
                sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_LoginsArticles where Article = @ArticolName AND LoginId=@loginID";
                sqlCommand.Connection = sysDBConnection;
                sqlCommand.Parameters.Add("@ArticolName", SqlDbType.NVarChar);
                sqlCommand.Parameters.Add("@loginID", SqlDbType.Int);
                sqlCommand.Parameters["@ArticolName"].Value = NameSolverStrings.FloatingMarkString;
                sqlCommand.Parameters["@loginID"].Value = NameSolverStrings.FloatingMarkNumber;
                recordsCount = (int)sqlCommand.ExecuteScalar();
            }
            catch (SqlException exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
            }
            finally
            {
                if (sqlCommand != null) sqlCommand.Dispose();
            }
            floatingMark = (recordsCount > 0) ? FloatingMark.YES : FloatingMark.NO;
            return (floatingMark == FloatingMark.YES);
        }

        //----------------------------------------------------------------------
        protected override bool IsUserAssignedToArticle(int loginID, string token, string articleName)
        {
            if (articleName == string.Empty)
                return false;

            AuthenticationSlot slot = GetSlot(token);
            return (slot != null &&
                    slot.UsedCALForArticle != null &&
                    slot.UsedCALForArticle[articleName] != null && 
                    (bool)slot.UsedCALForArticle[articleName]);

        }

        //----------------------------------------------------------------------
        internal override void RefreshFloatingMark()
        {
            floatingMark = FloatingMark.NONE;

        }






        enum FloatingMark { NONE, YES, NO };
        private FloatingMark floatingMark = FloatingMark.NONE;
        //-----------------------------------------------------------------------
        private bool CanUserUseFunctionality(int loginID, String articleName)
        {
            if (licut) return true;
            if (!EvaluateFloatingMark()) return true;

            //verifica sul db, che prima non si faceva
            //Siccome prima non esisteva controllo adesso se io non ho dati sul db non se è perche non ci sono perchè non ho mai 
            //abilitato la modifica o perchè ho deciso che l'utente non deve utilizzare questa funzionalità
            string query = "SELECT COUNT (LoginID) FROM MSD_LoginsArticles WHERE LoginID = @LoginID And (Article = @Art  OR Article =  @Art2)";

            SqlCommand aSqlCommand = new SqlCommand();

            int val = 0;
            try
            {
                aSqlCommand.CommandText = query;
                aSqlCommand.Connection = sysDBConnection;
                aSqlCommand.Parameters.AddWithValue("@LoginID", loginID);
                aSqlCommand.Parameters.AddWithValue("@Art", articleName);
                aSqlCommand.Parameters.AddWithValue("@Art2", "ERP-Ent.Manufacturing");

                //se l'utente è già stato assegnato all'articolo esco con successo
                val = (int)aSqlCommand.ExecuteScalar();



            }
            catch (Exception err)
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsUserAssignedToArticle: " + err.Message);
                aSqlCommand.Dispose();
                return false;
            }

            aSqlCommand.Dispose();

            return val > 0;
        }



    }

    //=========================================================================
    internal class ApplicationToken
    {
        public string Application = string.Empty;
        public string AuthenticationToken = string.Empty;
    }

    //-----------------------------------------------------------------------
    internal class AuthenticationSlot
    {
        private int loginID = -1;
        private int companyID = -1;
        private string loginName = string.Empty;
        private string companyName = string.Empty;

        private string processName = string.Empty;
        private IList<string> AddedTokens = new List<string>();
        protected bool reserved = false;
        private string producerKey = string.Empty;
        private string macAddress = string.Empty;
        private readonly LoginSlotType slotType = LoginSlotType.Invalid;
        private DateTime lastRefresh = DateTime.Now;
        private DateTime lastRefreshWMS = DateTime.Now; 
        private static long sixMinuteTicks = TimeSpan.TicksPerMinute * 6;
        //se l'ultimo refresh mi è stato fatto più di 6 minuti fa il token è in stato di NON Alive
        //(perchè in teoria mago chiede almeno ogni 5 minuti se il token è valido e questo ne refresha lo stato...)
        public bool IsAlive
        {
            get { return ((DateTime.Now.Subtract(lastRefresh)).Ticks < sixMinuteTicks); }
            set { if (value) lastRefresh = DateTime.Now; }
        }
        public bool IsAliveWMS
        {
            get { return ((DateTime.Now.Subtract(lastRefreshWMS)).Ticks < WMSAliveTime); }
            set { if (value) lastRefreshWMS = DateTime.Now; }
        }
        public long LastTick
        {
            get { return DateTime.Now.Subtract(lastRefresh).Ticks; }
        }


        public SSOLoginDataBag ssologinDataBag = null;

        public int LoginID { get { return loginID; } }
        public int CompanyID { get { return companyID; } }
        public string LoginName { get { return loginName; } }
        public string CompanyName { get { return companyName; } }
        public string ProcessName { get { return processName; } }
        public LoginSlotType SlotType { get { return slotType; } }
        public virtual bool Reserved { get { return reserved; } set { reserved = value; } }
        public string ProducerKey { get { return producerKey; } set { producerKey = value; } }
        public string MACAddress { get { return macAddress; } set { macAddress = value; } }
        internal long WMSAliveTime
        {
            get
            { return TimeSpan.TicksPerMinute * InstallationData.ServerConnectionInfo.WMSCalPurgeMinutes; }
        }

        /// <summary>
        /// Array degli articoli soggetti a CAL con un booleano che indica se e'stato usato dallútente relativo allo slot
        /// es. financials e manufacturing
        /// </summary>
        public Hashtable UsedCALForArticle = new Hashtable();

        /// <summary>
        /// Array delle applicazioni che possono fare login e relativo token in caso di autenticazione
        /// es. GDI MD EL
        /// </summary>
        public Hashtable ApplicationTokens = new Hashtable(); //GDI, easyLook, MD, Terze Parti


        //-----------------------------------------------------------------------
        public bool Logged
        {
            get
            {
                foreach (string tok in ApplicationTokens.Values)
                {
                    if (tok != string.Empty)
                        return true;
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------
        public LoginSlotType CalType
        {
            get
            {
                return AuthenticationProcess.GetCalType(processName);
            }
        }

        //-----------------------------------------------------------------------
        public bool UnNamed
        {
            get
            {
                return (SlotType == LoginSlotType.EasyLook) || (SlotType == LoginSlotType.MagicDocument);
            }
        }

        //-----------------------------------------------------------------------
        public AuthenticationSlot(LoginSlotType slotType, Hashtable usedCALForArticle)
        {
            this.slotType = slotType;
            this.UsedCALForArticle = usedCALForArticle;
            InitTokens();
        }

        //-----------------------------------------------------------------------
        public void InitTokens()
        {
            ApplicationTokens.Clear();
            ApplicationTokens.Add("EasyLook", string.Empty);
            ApplicationTokens.Add("Gdi", string.Empty);
            ApplicationTokens.Add("MagicDocument", string.Empty);
            ApplicationTokens.Add("ThirdPart", string.Empty);
        }

        //-----------------------------------------------------------------------
        private void InitUsedCal()
        {
            if (UsedCALForArticle == null) return;

            StringCollection keys = new StringCollection();
            foreach (string s in UsedCALForArticle.Keys)
                keys.Add(s);
            foreach (string s in keys)
                UsedCALForArticle[s] = false;
        }

        /// <summary>
        /// keep reserved fa si che si mantengano le info di utente che tiene lo slot, se è false
        /// si tiene solo il tipo di slot impostato in creazione dell'array
        /// </summary>
        /// <param name="keepReserved"></param>
        //-----------------------------------------------------------------------
        public void Clear(bool keepReserved)
        {
            if (!keepReserved)
            {
                loginID = -1;
                Reserved = false;
            }

            companyID = -1;

            if (!Reserved)
                loginName = string.Empty;

			companyName			= string.Empty;
			processName			= string.Empty;
            macAddress          = string.Empty;
			InitTokens();
			InitUsedCal();
		}

        //-----------------------------------------------------------------------
        public static string GetApplicationNameFromSlotType(LoginSlotType slotType)
        {
            switch (slotType)
            {
                case LoginSlotType.EasyLook:
                    return "EasyLook";
                case LoginSlotType.Gdi:
                    return "Gdi";
                case LoginSlotType.MagicDocument:
                    return "MagicDocument";
                case LoginSlotType.ThirdPart:
                    return "ThirdPart";
                case LoginSlotType.Mobile:
                    return "Mobile";

            }

            return string.Empty;
        }

        //-----------------------------------------------------------------------
        public static LoginSlotType GetSlotTypeFromApplicationName(string applicationName)
        {
            switch (applicationName)
            {
                case "EasyLook":
                    return LoginSlotType.EasyLook;
                case "Gdi":
                    return LoginSlotType.Gdi;
                case "MagicDocument":
                    return LoginSlotType.MagicDocument;
                case "ThirdPart":
                    return LoginSlotType.ThirdPart;
                case "Mobile":
                    return LoginSlotType.Mobile;
            }

            return LoginSlotType.Invalid;
        }


        //-----------------------------------------------------------------------
        public void Set(int loginID, bool reserved)
        {
            this.loginID = loginID;
            this.Reserved = reserved;
        }

        //-----------------------------------------------------------------------
        public void Set(int loginID, string loginName, int companyID, string companyName, string processName, string authenticationToken, bool reserved)
        {
            this.loginID = loginID;
            this.Reserved = reserved;

            Set(loginName, companyID, companyName, processName, authenticationToken);
        }

        //-----------------------------------------------------------------------
        public void Set(string loginName, int companyID, string companyName, string processName, string authenticationToken)
        {
            SetToken(processName, authenticationToken);
            Set(loginName, companyID, companyName, processName);
        }

        //-----------------------------------------------------------------------
        private void Set(string loginName, int companyID, string companyName, string processName)
        {
            this.companyID = companyID;
            this.loginName = loginName;
            this.companyName = companyName;
            this.processName = processName;
        }

        //-----------------------------------------------------------------------
        private void SetToken(string processName, string authenticationToken)
        {
            IsAlive = true;
            //ci deve essere un solo token alla volta quindi devo prima pulirlo poi assegnarlo
            InitTokens();
            ApplicationTokens[GetApplicationNameFromSlotType(AuthenticationProcess.GetCalType(processName))] = authenticationToken;

        }

        //-----------------------------------------------------------------------
        private void AddToken(string authenticationToken)
        {
            this.AddedTokens.Add(authenticationToken);
        }

        //-----------------------------------------------------------------------
        public bool ContainsToken(string token)
        {
            foreach (string appToken in ApplicationTokens.Values)
            {
                if (string.Compare(appToken, token, true, CultureInfo.InvariantCulture) == 0)
                    return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------
        public string GetApplicationByToken(string token)
        {
            foreach (DictionaryEntry de in ApplicationTokens)
            {
                if (string.Compare((string)de.Value, token, true, CultureInfo.InvariantCulture) == 0)
                    return (string)de.Key;
            }

            return String.Empty;
        }

        //-----------------------------------------------------------------------
        public string GetTokenByApplication(LoginSlotType calType)
        {
            string app = AuthenticationSlot.GetApplicationNameFromSlotType(calType);
            if (app == string.Empty)
                return string.Empty;

            return GetTokenByApplication(app);
        }
        //-----------------------------------------------------------------------
        public string GetTokenByApplication(string application)
        {
            foreach (DictionaryEntry de in ApplicationTokens)
            {
                if (string.Compare((string)de.Key, application, true, CultureInfo.InvariantCulture) == 0)
                    return (string)de.Value;
            }

            return String.Empty;
        }

        //-----------------------------------------------------------------------
        public void LogoutToken(string token)
        {
            String app = GetApplicationByToken(token);
            if (app == string.Empty)
                return;

            ApplicationTokens[app] = string.Empty;
        }
        //-----------------------------------------------------------------------
        public void ToXml(XmlElement xUser, bool writeTokens)
        {
            if (xUser == null)
                return;

            XmlAttribute nameAttribute = xUser.OwnerDocument.CreateAttribute("name");
            nameAttribute.Value = LoginName;
            xUser.Attributes.Append(nameAttribute);

            XmlAttribute companyAttribute = xUser.OwnerDocument.CreateAttribute("company");
            companyAttribute.Value = CompanyName;
            xUser.Attributes.Append(companyAttribute);

            if (!String.IsNullOrWhiteSpace(LoginName) && ProcessName == ProcessType.WMS)
            {
                XmlAttribute lastrefreshAttribute = xUser.OwnerDocument.CreateAttribute("lastrefresh");
                //solo per wms
                lastrefreshAttribute.Value = lastRefreshWMS.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                xUser.Attributes.Append(lastrefreshAttribute);


                XmlAttribute inactiveAttribute = xUser.OwnerDocument.CreateAttribute("inactive");
                inactiveAttribute.Value = (!IsAliveWMS).ToString();
                xUser.Attributes.Append(inactiveAttribute);


                XmlAttribute macipAttribute = xUser.OwnerDocument.CreateAttribute("id");
                macipAttribute.Value = macAddress;
                xUser.Attributes.Append(macipAttribute);

            }

            XmlAttribute processAttribute = xUser.OwnerDocument.CreateAttribute("process");
            processAttribute.Value = ProcessName;
            xUser.Attributes.Append(processAttribute);


            XmlAttribute slotAttribute = xUser.OwnerDocument.CreateAttribute("slotType");


            string slotTypeVal = string.Empty;
            switch (SlotType)
            {
                case LoginSlotType.EasyLook:
                    slotTypeVal = "EasyLook";
                    break;
                case LoginSlotType.Gdi:
                    slotTypeVal = "Gdi";
                    break;
                case LoginSlotType.MagicDocument:
                    slotTypeVal = "MagicDocument";
                    break;
                case LoginSlotType.Invalid:
                    slotTypeVal = "Invalid";
                    break;
                case LoginSlotType.Mobile:
                    slotTypeVal = "Mobile";
                    break;
            }
            slotAttribute.Value = slotTypeVal;
            xUser.Attributes.Append(slotAttribute);

            XmlAttribute loggedAttribute = xUser.OwnerDocument.CreateAttribute("logged");
            loggedAttribute.Value = Logged.ToString();
            xUser.Attributes.Append(loggedAttribute);

            XmlAttribute reservedAttribute = xUser.OwnerDocument.CreateAttribute("reserved");
            reservedAttribute.Value = Reserved.ToString();
            xUser.Attributes.Append(reservedAttribute);

            XmlElement tokensEl = xUser.OwnerDocument.CreateElement("ApplicationTokens");
            foreach (DictionaryEntry token in ApplicationTokens)
            {
                XmlElement tokenEl = xUser.OwnerDocument.CreateElement("ApplicationToken");
                tokenEl.SetAttribute("application", token.Key.ToString());

                if (writeTokens)
                    tokenEl.SetAttribute("token", token.Value.ToString());

                tokensEl.AppendChild(tokenEl);
            }
            xUser.AppendChild(tokensEl);

            XmlElement functionsEl = xUser.OwnerDocument.CreateElement("FunctionsCAL");

            if (UsedCALForArticle != null)
                foreach (DictionaryEntry token in this.UsedCALForArticle)
                {
                    XmlElement functionEl = xUser.OwnerDocument.CreateElement("FunctionCAL");
                    functionEl.SetAttribute("function", token.Key.ToString());
                    functionEl.SetAttribute("usedcal", token.Value.ToString());
                    functionsEl.AppendChild(functionEl);
                }
            xUser.AppendChild(functionsEl);

        }

        //-----------------------------------------------------------------------
        internal bool IsInvisibleWMS()
        {
            return ((string.Compare(processName, ProcessType.InvisibleMAN, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(processName, ProcessType.InvisibleWMS, true, CultureInfo.InvariantCulture) == 0 ||
                 string.Compare(processName, ProcessType.InvisibleWARMAN, true, CultureInfo.InvariantCulture) == 0));
        }



    }

    //Le concurrent sono come le gdi ma non vengono mai riservate, lo specifico utente può usare le cal named o le concurrent
    //=========================================================================
    internal class ConcurrentSlot : AuthenticationSlot
    {
        //-----------------------------------------------------------------------
        public ConcurrentSlot(LoginSlotType slotType, Hashtable usedCALForArticle)
            : base(slotType, usedCALForArticle)
        { }

        //-----------------------------------------------------------------------
        public override bool Reserved { get { return false; } set { reserved = false; } }

    }

    //=========================================================================
    public class UnlimitedSlots : IEnumerable
    {
        private IList<AuthenticationSlot> slots;
        private int fixedTo;

        //-----------------------------------------------------------------------
        public UnlimitedSlots() : this(Int32.MaxValue)
        {
        }

        //-----------------------------------------------------------------------
        public UnlimitedSlots(int fixedTo)
        {
            if (fixedTo < 0) throw new ArgumentOutOfRangeException("fixedTo must be zero or more");
            this.fixedTo = fixedTo;
            slots = new List<AuthenticationSlot>();
        }

        //-----------------------------------------------------------------------
        internal AuthenticationSlot this[int index]
        {
            get
            {
                return (slots == null) ? null : slots[index];
            }
            set
            {
                if (slots != null) slots[index] = value;
            }
        }

        //-----------------------------------------------------------------------
        public int FixedTo
        {
            get { return fixedTo; }
            set
            {
                if (slots.Count <= value)
                    fixedTo = value;

            }
        }

        //-----------------------------------------------------------------------
        public bool Illimited { get { return fixedTo == Int32.MaxValue; } set { if (value) fixedTo = Int32.MaxValue; } }

        //-----------------------------------------------------------------------
        public int Count { get { return (slots == null) ? 0 : slots.Count; } }

        //-----------------------------------------------------------------------
        internal bool Add(AuthenticationSlot slot)
        {
            if (slots == null || slot == null || !CanGetASlot())
                return false;

            slots.Add(slot);
            return true;
        }

        //-----------------------------------------------------------------------
        public void Clear()
        {
            slots = new List<AuthenticationSlot>();
            fixedTo = 0;
        }

        //-----------------------------------------------------------------------
        internal void Remove(AuthenticationSlot slot)
        {
            if (slots == null || slot == null)
                return;

            slots.Remove(slot);
        }

        //-----------------------------------------------------------------------
        public bool CanGetASlot()
        {
            return (slots != null && slots.Count < fixedTo);
        }

        // IEnumerable Members
        //-----------------------------------------------------------------------
        public IEnumerator GetEnumerator()
        {
            return slots.GetEnumerator();
        }

    }

    //-----------------------------------------------------------------------
    internal class VerticalFloatingCalTableItem : IComparable, IComparable<VerticalFloatingCalTableItem>
    {
        public string ArtName;
        public string Token;

        //-----------------------------------------------------------------------
        public VerticalFloatingCalTableItem(string artName, string token)
        {
            ArtName = artName.ToLower();
            Token = token;
        }

        //-----------------------------------------------------------------------
        public override int GetHashCode()
        {
            return (ArtName + Token).GetHashCode();
        }

        //-----------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            VerticalFloatingCalTableItem tempDataStr = obj as VerticalFloatingCalTableItem;
            if (Object.ReferenceEquals(tempDataStr, null))
                return false;
            return (
                (String.Compare(tempDataStr.Token, Token, StringComparison.InvariantCulture) == 0) &&
                String.Compare(tempDataStr.ArtName, ArtName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        //-----------------------------------------------------------------------
        public int CompareTo(object obj)
        {
            VerticalFloatingCalTableItem tempDataStr = obj as VerticalFloatingCalTableItem;
            if (Object.ReferenceEquals(tempDataStr, null))
                return 1;
            bool ok = (
                (String.Compare(tempDataStr.Token, Token, StringComparison.InvariantCulture) == 0) &&
                String.Compare(tempDataStr.ArtName, ArtName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (ok)
                return 0;
            return -1;
        }

        //-----------------------------------------------------------------------
        public int CompareTo(VerticalFloatingCalTableItem other)
        {
            bool ok = (
                (String.Compare(other.Token, Token, StringComparison.InvariantCulture) == 0) &&
                String.Compare(other.ArtName, ArtName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (ok)
                return 0;
            return -1;
        }
    }

    //----------------------------------------------------------------------
    internal class SSOLoginDataBag
    {
        public string loginName = null;
        public string companyName = null;
        public string password = null;
        public int loginid = -1;
        public int companyid = -1;
        public TokenInfinity token = null;

        public bool Valid { get { return token != null; } }
        public string SSOID { get { return token?.ssoid; } }

        internal bool CheckTime( )
        {
         
            if (token != null)
                return token.CheckTime();
            return false;

        }
    }

}

