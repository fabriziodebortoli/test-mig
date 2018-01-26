using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Web.Services.Protocols;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    /// <summary>
    /// WebServicesDetail
    /// Visualizza lo stato ed eventualmente alcune informazioni sui web services installati sul server
    /// </summary>
    //=========================================================================
    public partial class WebServicesDetail : PlugInsForm
    {
        #region Variabili
        private LoginManager aLoginManager = null;
        private LockManager aLockManager = null;
        private TbServices aTBService = null;
        private TbSenderWrapper tbSender;
        private EasyAttachmentSync eaSync = null;
        private DataSynchronizer dataSynch = null;

        private Diagnostic diagnostic = new Diagnostic("WebServicesDetail");

        private ImageList consoleImageList;
        #endregion

        #region Eventi e Delegati
        public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
        public event SendDiagnostic OnSendDiagnostic;

        public delegate void EnableProgressBar(object sender);
        public event EnableProgressBar OnEnableProgressBar;
        public delegate void DisableProgressBar(object sender);
        public event DisableProgressBar OnDisableProgressBar;
        public delegate void SetProgressBarStep(object sender, int step);
        public event SetProgressBarStep OnSetProgressBarStep;
        public delegate void SetProgressBarValue(object sender, int currentValue);
        public event SetProgressBarValue OnSetProgressBarValue;
        public delegate void SetProgressBarText(object sender, string message);
        public event SetProgressBarText OnSetProgressBarText;

        // Il SysAdmin mi ritorna l'elenco delle aziende in MSD_Companies
        public delegate SqlDataReader AskAllCompanies();
        public event AskAllCompanies OnAskAllCompanies;

        // Il SysAdmin mi ritorna l'elenco degli utenti presenti nella MSD_Logins
        public delegate SqlDataReader AskAllLogins(bool localServer, string serverName);
        public event AskAllLogins OnAskAllLogins;

        // Il SysAdmin mi ritorna l'elenco degli utenti associati a una azienda
        public delegate SqlDataReader AskAllCompanyUsers(string companyId);
        public event AskAllCompanyUsers OnAskAllCompanyUsers;

        // Il SysAdmin mi ritorna i record tracciati
        public delegate SqlDataReader AskRecordsTraced(string company, string user, TraceActionType operationType, DateTime fromDate, DateTime toDate);
        public event AskRecordsTraced OnAskRecordsTraced;

        // Il SysAdmin cancella i records tracciati fino a una data
        public delegate bool DeleteRecordsTraced(DateTime toDate);
        public event DeleteRecordsTraced OnDeleteRecordsTraced;

        // evento per chiedere alla Console l'authentication token
        public delegate string GetAuthenticationToken();
        public event GetAuthenticationToken OnGetAuthenticationToken;
        #endregion

        /// <summary>
        /// Costruttore
        /// </summary>
        //---------------------------------------------------------------------
        public WebServicesDetail
            (
            LoginManager loginManager,
            LockManager lockManager,
            TbServices tbService,
            EasyAttachmentSync eaSync,
            TbSenderWrapper tbsender,
            DataSynchronizer dataSynch,
            ImageList images
            )
        {
            InitializeComponent();

            aLoginManager = loginManager;
            aLockManager = lockManager;
            aTBService = tbService;
            this.eaSync = eaSync;
            this.tbSender = tbsender;
            this.dataSynch = dataSynch;

            this.consoleImageList = images;

            LabelTitle.Text = string.Format(LabelTitle.Text, Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture));
            this.TabIndex = 3;
            this.TabStop = true;

            ViewDetails.TabStop = false;
            ViewDetails.View = View.Details;
            ViewDetails.Dock = DockStyle.Fill;
            ViewDetails.AllowColumnReorder = true;
            ViewDetails.Sorting = System.Windows.Forms.SortOrder.Ascending;
            ViewDetails.LargeImageList = ViewDetails.SmallImageList = this.consoleImageList;
        }

        #region Funzioni per il Check dello stato
        /// <summary>
        /// CheckStateLogin 
        /// </summary>
        //---------------------------------------------------------------------
        private bool CheckStateLogin()
        {
            bool loginState = false;

            try
            {
                loginState = aLoginManager.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LoginManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LoginManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LoginManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return loginState;
        }

        //---------------------------------------------------------------------
        private bool CheckStateTbService()
        {
            bool tbserviceState = false;

            try
            {
                tbserviceState = aTBService.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, "TBService", BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, "TBService", BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, "TBService", BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return tbserviceState;
        }

        /// <summary>
        /// CheckStateLock
        /// </summary>
        //---------------------------------------------------------------------
        private bool CheckStateLock()
        {
            bool lockState = false;

            try
            {
                lockState = aLockManager.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LockManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LockManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.LockManager, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return lockState;
        }

        //---------------------------------------------------------------------
        private bool CheckStateTbWebService(ITbWebService webService)
        {
            bool aState = false;

            try
            {
                aState = webService.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, webService.Name, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.TbSender, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.TbSender, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return aState;
        }

        /// <summary>
        /// CheckStateEasyAttachmentSync
        /// </summary>
        //---------------------------------------------------------------------
        private bool CheckStateEasyAttachmentSync()
        {
            bool eaState = false;

            try
            {
                eaState = eaSync.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.EasyAttachmentSync, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.EasyAttachmentSync, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.EasyAttachmentSync, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return eaState;
        }

        /// <summary>
        /// CheckStateDataSynchronizer
        /// </summary>
        //---------------------------------------------------------------------
        private bool CheckStateDataSynchronizer()
        {
            bool dsState = false;

            try
            {
                dsState = dataSynch.IsAlive();
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.DataSynchronizer, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (SoapException soapExc)
            {
                Debug.Fail(soapExc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.DataSynchronizer, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.CannotContactWebService, NameSolverStrings.DataSynchronizer, BasePathFinder.BasePathFinderInstance.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            return dsState;
        }
        #endregion

        #region CheckState - Effettua il Check dello stato dei Web Services
        /// <summary>
        /// CheckState
        /// </summary>
        //---------------------------------------------------------------------
        private void CheckState()
        {
            //Check di TBServices
            ListViewItem stateTbService = new ListViewItem();
            if (CheckStateTbService())
            {
                stateTbService.StateImageIndex = stateTbService.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                stateTbService.Text = Strings.ActiveWebService;
            }
            else
            {
                stateTbService.StateImageIndex = stateTbService.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                stateTbService.Text = Strings.NonActiveWebService;
            }
            stateTbService.SubItems.Add(NameSolverStrings.TbServices);
            stateTbService.SubItems.Add(BasePathFinder.BasePathFinderInstance.TbServicesUrl);
            ViewDetails.Items.Add(stateTbService);

            // TODO metti interfaccia anche a altri web services
            // TODO controllare attivazione, se non attivato, non appaia (es. TbSender, TbHermes)
            // TODO ciclare su collection invece di esplicitarli tutti
            AddTbWebService(this.tbSender);
            //AddTbWebService(this.tbHermes); // NON mostro TbHermes perche' in Mago4 non serve

            //Check di LoginManager
            ListViewItem stateLogin = new ListViewItem();
            if (CheckStateLogin())
            {
                stateLogin.StateImageIndex = stateLogin.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                stateLogin.Text = Strings.ActiveWebService;
            }
            else
            {
                stateLogin.StateImageIndex = stateLogin.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                stateLogin.Text = Strings.NonActiveWebService;
            }
            stateLogin.SubItems.Add(NameSolverStrings.LoginManager);
            stateLogin.SubItems.Add(BasePathFinder.BasePathFinderInstance.LoginManagerUrl);
            ViewDetails.Items.Add(stateLogin);

            //Check di LockManager
            ListViewItem stateLock = new ListViewItem();
            if (CheckStateLock())
            {
                stateLock.StateImageIndex = stateLock.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                stateLock.Text = Strings.ActiveWebService;
            }
            else
            {
                stateLock.StateImageIndex = stateLock.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                stateLock.Text = Strings.NonActiveWebService;
            }
            stateLock.SubItems.Add(NameSolverStrings.LockManager);
            stateLock.SubItems.Add(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
            ViewDetails.Items.Add(stateLock);

            //Check di EasyAttachmentSync
            ListViewItem eaSyncItem = new ListViewItem();
            if (CheckStateEasyAttachmentSync())
            {
                eaSyncItem.StateImageIndex = eaSyncItem.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                eaSyncItem.Text = Strings.ActiveWebService;
            }
            else
            {
                eaSyncItem.StateImageIndex = eaSyncItem.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                eaSyncItem.Text = Strings.NonActiveWebService;
            }
            eaSyncItem.SubItems.Add(NameSolverStrings.EasyAttachmentSync);
            eaSyncItem.SubItems.Add(BasePathFinder.BasePathFinderInstance.EasyAttachmentSyncUrl);
            ViewDetails.Items.Add(eaSyncItem);

            //Check di DataSynchronizer
            ListViewItem dataSynchroItem = new ListViewItem();
            if (CheckStateDataSynchronizer())
            {
                dataSynchroItem.StateImageIndex = dataSynchroItem.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                dataSynchroItem.Text = Strings.ActiveWebService;
            }
            else
            {
                dataSynchroItem.StateImageIndex = dataSynchroItem.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                dataSynchroItem.Text = Strings.NonActiveWebService;
            }
            dataSynchroItem.SubItems.Add(NameSolverStrings.DataSynchronizer);
            dataSynchroItem.SubItems.Add(BasePathFinder.BasePathFinderInstance.DataSynchronizerUrl);
            ViewDetails.Items.Add(dataSynchroItem);

            //
            ViewDetails.Items[0].Selected = true;
            BuildContextMenu();
            ViewDetails.ContextMenuStrip = webContextMenu;
        }

        //---------------------------------------------------------------------
        private void AddTbWebService(ITbWebService webService)
        {
            ListViewItem wsItem = new ListViewItem();
            if (CheckStateTbWebService(webService))
            {
                wsItem.StateImageIndex = wsItem.ImageIndex = PlugInTreeNode.GetGreenLampStateImageIndex;
                wsItem.Text = Strings.ActiveWebService;
            }
            else
            {
                wsItem.StateImageIndex = wsItem.ImageIndex = PlugInTreeNode.GetRedLampStateImageIndex;
                wsItem.Text = Strings.NonActiveWebService;
            }
            wsItem.SubItems.Add(webService.Name);
            wsItem.SubItems.Add(webService.Url);
            ViewDetails.Items.Add(wsItem);
        }
        #endregion

        #region StartCheck - Esegue il Check dello stato dei Web Services
        /// <summary>
        /// StartCheck
        /// Esegue il Check dello stato dei Web Services
        /// </summary>
        //---------------------------------------------------------------------
        public void StartCheck()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, Strings.Waiting);
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;
            CheckState();
            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            this.State = StateEnums.View;
        }
        #endregion

        #region Funzioni per la grafica (layout sugli oggetti, resize) e contextMenu

        #region SettingListView - Customizza il Layout della listView
        /// <summary>
        /// SettingListView
        /// </summary>
        //---------------------------------------------------------------------
        public void SettingListView()
        {
            ViewDetails.Columns.Clear();
            ViewDetails.Columns.Add(Strings.WebServiceState, -2, HorizontalAlignment.Left);
            ViewDetails.Columns.Add(Strings.WebServiceName, -2, HorizontalAlignment.Left);
            ViewDetails.Columns.Add(Strings.WebServicePath, -2, HorizontalAlignment.Left);
            StartCheck();
        }
        #endregion

        #region WebServicesDetail_Resize - Per settare a run-time correttamente le dimensioni della listView
        /// <summary>
        /// WebServicesDetail_Resize
        /// Per settare a run-time correttamente le dimensioni della listView
        /// </summary>
        //---------------------------------------------------------------------
        private void WebServicesDetail_Resize(object sender, System.EventArgs e)
        {
            if (ViewDetails.Columns.Count > 0)
            {
                for (int i = 0; i < ViewDetails.Columns.Count; i++)
                    ViewDetails.Columns[i].Width = -2;
            }
        }
        #endregion

        #region BuildContextMenu - Costruisce il contextMenu sugli oggetti della lista
        /// <summary>
        /// BuildContextMenu
        /// Costruisce il contextMenu sugli oggetti della lista
        /// </summary>
        //---------------------------------------------------------------------
        private void BuildContextMenu()
        {
            webContextMenu = new ContextMenuStrip();

            ToolStripMenuItem mi1 = new ToolStripMenuItem(Strings.ToolTipWebServiceDetail, imageList1.Images[2], new System.EventHandler(ShowWebServiceDetails));
            webContextMenu.Items.Add(mi1);
            webContextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mi2 = new ToolStripMenuItem(Strings.ToolTipWebServiceRestart, imageList1.Images[1], new System.EventHandler(WebServiceRestart));
            webContextMenu.Items.Add(mi2);
            webContextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mi3 = new ToolStripMenuItem(Strings.CutWebServiceUrl, imageList1.Images[0], new System.EventHandler(CutWebServiceUrl));
            webContextMenu.Items.Add(mi3);
        }
        #endregion
        #endregion

        #region Funzioni per il dettaglio sui Web Services

        #region CutWebServiceUrl - Copia del percorso di installazione del web service selezionato
        /// <summary>
        /// CutWebServiceUrl
        /// </summary>
        //---------------------------------------------------------------------
        private void CutWebServiceUrl(object sender, System.EventArgs e)
        {
            if (ViewDetails.SelectedItems.Count > 0)
            {
                //servizio
                ListViewItem selectedWebService = (ListViewItem)ViewDetails.SelectedItems[0];
                string url = selectedWebService.SubItems[ItemListIndexer.UrlPath].Text;
                if (url.Length > 0)
                    Clipboard.SetDataObject(url, true);
            }
        }
        #endregion

        #region ShowWebServiceDetails - Attiva il panel con i dettagli
        /// <summary>
        /// ShowWebServiceDetails
        /// Mostra i dettagli del Web services
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //---------------------------------------------------------------------
        private void ShowWebServiceDetails(object sender, System.EventArgs e)
        {
            if (ViewDetails.SelectedItems.Count > 0)
            {
                DetailsPanel.Controls.Clear();

                ListViewItem selectedWebService = (ListViewItem)ViewDetails.SelectedItems[0];

                if (selectedWebService.Text != Strings.NonActiveWebService)
                {
                    if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.LockManager, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        LocksDetail lockDetail = new LocksDetail(aLoginManager, aLockManager);
                        lockDetail.OnDisableProgressBar += new LocksDetail.DisableProgressBar(DisableConsoleProgressBar);
                        lockDetail.OnEnableProgressBar += new LocksDetail.EnableProgressBar(EnableConsoleProgressBar);
                        lockDetail.OnSetProgressBarStep += new LocksDetail.SetProgressBarStep(SetConsoleProgressBarStep);
                        lockDetail.OnSetProgressBarText += new LocksDetail.SetProgressBarText(SetConsoleProgressBarText);
                        lockDetail.OnSetProgressBarValue += new LocksDetail.SetProgressBarValue(SetConsoleProgressBarValue);
                        lockDetail.OnSendDiagnostic += new LocksDetail.SendDiagnostic(OnSendDiagnostic);
                        lockDetail.OnGetAuthenticationToken += new LocksDetail.GetAuthenticationToken(GetAuthenticationTokenFromConsole);
                        lockDetail.TopLevel = false;
                        lockDetail.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(lockDetail);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        lockDetail.Visible = true;
                    }
                    else if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.LoginManager, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        LoginsDetail loginsDetail = new LoginsDetail(aLoginManager, aTBService, this.consoleImageList);
                        loginsDetail.OnAskAllCompaniesFromSysAdmin += new LoginsDetail.AskAllCompaniesFromSysAdmin(CallAllCompaniesFromSysAdmin);
                        loginsDetail.OnAskAllLoginsFromSysAdmin += new LoginsDetail.AskAllLoginsFromSysAdmin(CallAllLoginsFromSysAdmin);
                        loginsDetail.OnAskAllCompanyUsersFromSysAdmin += new LoginsDetail.AskAllCompanyUsersFromSysAdmin(CallAllCompanyUsersFromSysAdmin);
                        loginsDetail.OnAskRecordsTracedFromSysAdmin += new LoginsDetail.AskRecordsTracedFromSysAdmin(CallRecordsTracedFromSysAdmin);
                        loginsDetail.OnDeleteRecordsTracedFromSysAdmin += new LoginsDetail.DeleteRecordsTracedFromSysAdmin(CallDeleteRecordsTracesFromSysAdmin);
                        loginsDetail.OnGetAuthenticationToken += new LoginsDetail.GetAuthenticationToken(GetAuthenticationTokenFromConsole);
                        loginsDetail.TopLevel = false;
                        loginsDetail.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(loginsDetail);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        loginsDetail.Visible = true;
                    }
                    else if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.TbServices, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        TBServiceDetail tbServiceDetail = new TBServiceDetail(aTBService);
                        tbServiceDetail.OnGetAuthenticationToken += new TBServiceDetail.GetAuthenticationToken(GetAuthenticationTokenFromConsole);
                        tbServiceDetail.TopLevel = false;
                        tbServiceDetail.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(tbServiceDetail);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        tbServiceDetail.Visible = true;
                    }
                    else if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.EasyAttachmentSync, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        EASyncDetail eaSyncDetail = new EASyncDetail(eaSync, this.consoleImageList);
                        eaSyncDetail.TopLevel = false;
                        eaSyncDetail.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(eaSyncDetail);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        eaSyncDetail.Visible = true;
                    }
                    else if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.TbSender, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        TBSenderDetails tbsenderDetails = new TBSenderDetails(tbSender, this.consoleImageList);
                        tbsenderDetails.TopLevel = false;
                        tbsenderDetails.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(tbsenderDetails);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        tbsenderDetails.Visible = true;
                    }
                    else if (string.Compare(selectedWebService.SubItems[ItemListIndexer.WebName].Text, NameSolverStrings.DataSynchronizer, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        DataSynchroDetail dataSynchroDetail = new DataSynchroDetail(dataSynch, aLoginManager, this.consoleImageList);
                        dataSynchroDetail.TopLevel = false;
                        dataSynchroDetail.Dock = DockStyle.Fill;
                        DetailsPanel.Controls.Add(dataSynchroDetail);
                        DetailsPanel.Height = this.Height * 2 / 3;
                        DetailsPanel.Visible = true;
                        dataSynchroDetail.Visible = true;
                    }
                }
            }
        }
        #endregion

        # region Eventi verso l'esterno
        //---------------------------------------------------------------------
        private SqlDataReader CallAllCompaniesFromSysAdmin()
        {
            if (OnAskAllCompanies != null)
                return OnAskAllCompanies();

            return null;
        }

        //---------------------------------------------------------------------
        private SqlDataReader CallAllLoginsFromSysAdmin(bool localServer, string serverName)
        {
            if (OnAskAllLogins != null)
                return OnAskAllLogins(localServer, serverName);

            return null;
        }

        //---------------------------------------------------------------------
        private SqlDataReader CallAllCompanyUsersFromSysAdmin(string companyId)
        {
            if (this.OnAskAllCompanyUsers != null)
                return OnAskAllCompanyUsers(companyId);

            return null;
        }

        //---------------------------------------------------------------------
        private SqlDataReader CallRecordsTracedFromSysAdmin(string company, string user, TraceActionType operationType, DateTime fromDate, DateTime toDate)
        {
            if (this.OnAskRecordsTraced != null)
                return OnAskRecordsTraced(company, user, operationType, fromDate, toDate);

            return null;
        }

        //---------------------------------------------------------------------
        private bool CallDeleteRecordsTracesFromSysAdmin(DateTime toDate)
        {
            if (OnDeleteRecordsTraced != null)
                return OnDeleteRecordsTraced(toDate);

            return false;
        }

        //---------------------------------------------------------------------------	
        public string GetAuthenticationTokenFromConsole()
        {
            if (OnGetAuthenticationToken != null)
                return OnGetAuthenticationToken();

            return string.Empty;
        }
        # endregion

        #region ViewDetails_ItemActivate - Se seleziono un elemento della lista, disattivo il panel
        /// <summary>
        /// ViewDetails_ItemActivate
        /// Disattiva il panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //---------------------------------------------------------------------
        private void ViewDetails_ItemActivate(object sender, System.EventArgs e)
        {
            ShowWebServiceDetails(sender, e);
        }
        #endregion

        #region ViewDetails_MouseDown - Click destro su listView
        /// <summary>
        /// ViewDetails_MouseDown
        /// Click destro su listView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //---------------------------------------------------------------------
        private void ViewDetails_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListViewItem itemSelected = ViewDetails.GetItemAt(e.X, e.Y);
            if (itemSelected != null)
            {
                //copia url solo se il campo non è vuoto
                if (itemSelected.SubItems[ItemListIndexer.UrlPath].Text.Length > 0)
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.Details].Enabled = true;
                else
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.Details].Enabled = false;

                //se il web è in modalità Non Attiva, non posso restartarlo
                if (string.Compare(itemSelected.SubItems[ItemListIndexer.ActiveState].Text, Strings.NonActiveWebService, true, CultureInfo.InvariantCulture) == 0)
                {
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.Restart].Enabled = false;
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.UrlCut].Enabled = false;
                }
                else
                {
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.Restart].Enabled = true;
                    ViewDetails.ContextMenuStrip.Items[MenuItemsIndexer.UrlCut].Enabled = true;
                }
            }
        }
        #endregion

        #endregion

        #region Funzioni per il restart del web services

        #region WebServiceRestart - Chiede conferma prima di procedere
        /// <summary>
        /// WebServiceRestart
        /// Chiede conferma prima di procedere
        /// </summary>
        //---------------------------------------------------------------------
        private void WebServiceRestart(object sender, System.EventArgs e)
        {
            if (ViewDetails.SelectedItems.Count > 0)
            {
                ListViewItem selectedWebService = (ListViewItem)ViewDetails.SelectedItems[0];
                string nameOfService = selectedWebService.SubItems[ItemListIndexer.WebName].Text;
                DialogResult askIfContinue = MessageBox.Show
                    (
                    this,
                    string.Format(Strings.AskIfRestartService, nameOfService),
                    Strings.CaptionRestartService,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                    );

                if (askIfContinue == DialogResult.Yes)
                {
                    switch (nameOfService)
                    {
                        case NameSolverStrings.LoginManager:
                            RestartLoginManager();
                            break;
                        case NameSolverStrings.LockManager:
                            RestartLockManager();
                            break;
                        case NameSolverStrings.TbServices:
                            RestartTbServices();
                            break;
                        case NameSolverStrings.EasyAttachmentSync:
                            RestartEasyAttachmentSync();
                            break;
                        case NameSolverStrings.DataSynchronizer:
                            RestartDataSynchronizer();
                            break;
                        // TODO eliminare switch, usare dictionary precaricato con vari ITbWebService e key==Name
                        case NameSolverStrings.TbSender:
                            RestartTbWebService(this.tbSender);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion

        #region RestartLoginManager - Restart del LoginManager
        /// <summary>
        /// RestartLoginManager
        /// </summary>
        //---------------------------------------------------------------------
        private void RestartLoginManager()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, NameSolverStrings.LoginManager));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;
            int result = -1;
            //re-start del servizio
            DialogResult askIfContinue = DialogResult.OK;
            if (aLoginManager.GetLoggedUsersNumber() > 0)
            {
                askIfContinue = MessageBox.Show(this,
                                                Strings.ExistLoggedUsers,
                                                Strings.CaptionRestartService,
                                                MessageBoxButtons.OKCancel,
                                                MessageBoxIcon.Question);
            }

            if (askIfContinue == DialogResult.OK)
            {
                try
                {
                    result = aLoginManager.Init(false, GetAuthenticationTokenFromConsole());
                    if (result != 0)
                        diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.LoginManager));
                    else
                        diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, NameSolverStrings.LoginManager));
                }
                catch (WebException webExc)
                {
                    ExtendedInfo extendedInfo = new ExtendedInfo();
                    if (webExc.Response != null)
                    {
                        HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                        if (webResponse.StatusDescription.Length > 0)
                        {
                            Debug.Fail(webResponse.StatusDescription);
                            extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                        }
                        else
                        {
                            Debug.Fail(webResponse.StatusCode.ToString());
                            extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                        }
                        webResponse.Close();
                    }
                    else
                    {
                        Debug.Fail(webExc.Status.ToString());
                        extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                    }
                    diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.LoginManager), extendedInfo);
                }
            }
            else
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.NotRestartWebService, NameSolverStrings.LoginManager));

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }
        #endregion

        #region RestartLockManager - Restart del LockManager
        /// <summary>
        /// RestartLockManager
        /// </summary>
        //---------------------------------------------------------------------
        private void RestartLockManager()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, NameSolverStrings.LockManager));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;

            //re-start del servizio
            try
            {
                string token = string.Empty;
                if (OnGetAuthenticationToken != null)
                    token = OnGetAuthenticationToken();

                if (token.Length == 0)
                    throw new Exception(Strings.AuthenticationTokenNotValid);

                aLockManager.Init("3B5B4C2F-E563-4b52-B187-A3071B65688E");
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, NameSolverStrings.LockManager));
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.LockManager), extendedInfo);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.Message);
            }

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }
        #endregion

        #region RestartTbServices - Restart del TbServices
        /// <summary>
        /// RestartLockManager
        /// </summary>
        //---------------------------------------------------------------------
        private void RestartTbServices()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, NameSolverStrings.TbServices));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;

            //re-start del servizio
            try
            {
                aTBService.Init();
                diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, NameSolverStrings.TbServices));
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.TbServices), extendedInfo);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.Message);
            }

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }

        #endregion

        //---------------------------------------------------------------------
        private void RestartTbWebService(ITbWebService webService)
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, webService.Name));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;

            //re-start del servizio
            try
            {
                if (webService.Init())
                    diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, webService.Name));
                else
                    diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, webService.Name));
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, webService.Name), extendedInfo);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.Message);
            }

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }

        #region RestartEasyAttachmentSync - Restart di EasyAttachmentSync
        /// <summary>
        /// RestartLockManager
        /// </summary>
        //---------------------------------------------------------------------
        private void RestartEasyAttachmentSync()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, NameSolverStrings.EasyAttachmentSync));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;

            //re-start del servizio
            try
            {
                if (eaSync.Init("{2E8164FA-7A8B-4352-B0DC-479984070507}"))
                    diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, NameSolverStrings.EasyAttachmentSync));
                else
                    diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.EasyAttachmentSync));
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.EasyAttachmentSync), extendedInfo);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.Message);
            }

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }
        #endregion

        #region RestartDataSynchronizer - Restart di DataSynchronizer
        /// <summary>
        /// RestartDataSynchronizer
        /// </summary>
        //---------------------------------------------------------------------
        private void RestartDataSynchronizer()
        {
            //Abilito la progressBar
            SetConsoleProgressBarValue(this, 1);
            SetConsoleProgressBarText(this, string.Format(Strings.RestartService, NameSolverStrings.DataSynchronizer));
            SetConsoleProgressBarStep(this, 5);
            EnableConsoleProgressBar(this);
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Show();
            //Setto lo stato della Form
            this.State = StateEnums.Processing;

            //re-start del servizio
            try
            {
                List<DataSynchroDatabaseInfo> dsInfoList = aLoginManager.GetDataSynchroDatabasesInfo("{2E8164FA-7A8B-4352-B0DC-479984070222}");
                
                if (dsInfoList.Count > 0 && this.dataSynch.Init(dsInfoList[0].LoginName, dsInfoList[0].LoginPassword, dsInfoList[0].LoginWindowsAuthentication, dsInfoList[0].CompanyName))
                    diagnostic.Set(DiagnosticType.Information, string.Format(Strings.AfterRestartService, NameSolverStrings.DataSynchronizer));
                else
                    diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.DataSynchronizer));
            }
            catch (WebException webExc)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                if (webExc.Response != null)
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
                    if (webResponse.StatusDescription.Length > 0)
                    {
                        Debug.Fail(webResponse.StatusDescription);
                        extendedInfo.Add(Strings.Description, webResponse.StatusDescription);
                    }
                    else
                    {
                        Debug.Fail(webResponse.StatusCode.ToString());
                        extendedInfo.Add(Strings.Description, webResponse.StatusCode.ToString());
                    }
                    webResponse.Close();
                }
                else
                {
                    Debug.Fail(webExc.Status.ToString());
                    extendedInfo.Add(Strings.Description, webExc.Status.ToString());
                }
                diagnostic.Set(DiagnosticType.Error, string.Format(Strings.CannotRestartWebService, NameSolverStrings.DataSynchronizer), extendedInfo);
            }
            catch (Exception exc)
            {
                diagnostic.Set(DiagnosticType.Error, exc.Message);
            }

            Cursor.Current = Cursors.Default;
            Cursor.Show();
            //Disabilito la progressBar
            SetConsoleProgressBarText(this, string.Empty);
            DisableConsoleProgressBar(this);
            if (diagnostic.Elements.Count > 0)
            {
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(this, diagnostic);
                    diagnostic.Clear();
                }
            }
            this.State = StateEnums.View;
        }
        #endregion
        #endregion

        #region Eventi per Abilitare e Impostare la ProgressBar
        /// <summary>
        /// EnableConsoleProgressBar
        /// </summary>
        //---------------------------------------------------------------------
        private void EnableConsoleProgressBar(object sender)
        {
            if (OnEnableProgressBar != null)
                OnEnableProgressBar(sender);
        }

        /// <summary>
        /// DisableConsoleProgressBar
        /// </summary>
        //---------------------------------------------------------------------
        private void DisableConsoleProgressBar(object sender)
        {
            if (OnDisableProgressBar != null)
                OnDisableProgressBar(sender);
        }

        /// <summary>
        /// SetConsoleProgressBarStep
        /// </summary>
        //---------------------------------------------------------------------
        private void SetConsoleProgressBarStep(object sender, int step)
        {
            if (OnSetProgressBarStep != null)
                OnSetProgressBarStep(sender, step);
        }

        /// <summary>
        /// SetConsoleProgressBarValue
        /// </summary>
        //---------------------------------------------------------------------
        private void SetConsoleProgressBarValue(object sender, int currentValue)
        {
            if (OnSetProgressBarValue != null)
                OnSetProgressBarValue(sender, currentValue);
        }

        /// <summary>
        /// SetConsoleProgressBarText
        /// </summary>
        //---------------------------------------------------------------------
        private void SetConsoleProgressBarText(object sender, string message)
        {
            if (OnSetProgressBarText != null)
                OnSetProgressBarText(sender, message);
        }
        #endregion
    }

    /// <summary>
    /// MenuItemsIndexer
    /// Indici degli oggetti menuItems (ContextMenu)
    /// </summary>
    //=========================================================================
    internal class MenuItemsIndexer
    {
        public const int Details = 0;
        public const int Restart = 2;
        public const int UrlCut = 4;
    }

    /// <summary>
    /// ItemListIndexer
    /// Indici delle colonne della listView
    /// </summary>
    //=========================================================================
    internal class ItemListIndexer
    {
        public const int ActiveState = 0;
        public const int WebName = 1;
        public const int UrlPath = 2;
    }
}
