using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Drawing;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
    /// <summary>
    /// Form della configurazione dell'applicazione installata.
    /// </summary>
    //=========================================================================
    public class LicensedConfigurationForm : Form
    {
        private System.ComponentModel.IContainer components = null;
        ClientStub clientStub;
        private TabControl TbcGrids;
        private ImageList ImageList;
        private Button btnCancel;
        private Button BtnOK;
        internal static Diagnostic diagnostic = new Diagnostic("LoginManager");//?
        private Label LblInfo;
        IList<FillingError> errors = new List<FillingError>();
        private WaitingControl waitingControl1;
        private LinkLabel LnkProxysettings;
        private PictureBox PbProxy;
        private Button BtnSms;
        internal bool FirstTime = false;
        public event ModificationManager EnableNext;
        //public bool AllowSMS = false;

        //---------------------------------------------------------------------
        public LicensedConfigurationForm(ClientStub clientStub, string text, bool allowSms)
            : this(clientStub, text, null, null, allowSms)

        { }

        //---------------------------------------------------------------------
        public LicensedConfigurationForm(ClientStub clientStub, string text, SerialNumberInfo startSerial, string editionID, bool allowSms)
        {
            InitializeComponent();
            BtnSms.Visible = allowSms;
            this.clientStub = clientStub;
            this.Text = text;
            if (!clientStub.ProductActivated())
                this.Text += " " + LicenceStrings.NotActive;
            FirstTime = (startSerial != null);
            ArrayList list = new ArrayList();
            Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            //raccolgo tutti i prodotti attivati e non , 
            //ma devo filtrare per prodotti con stesso id di prodotto , 
            //come le varie edition di mago
            //considerando che per i verticali potrebbe non essere indicato perchè non obbligatori0(in tal caso dovrebbe esserci il prodname)
            string identifier = null;
            foreach (ProductInfo p in clientStub.ActivationObj.Products)
            {
                if (p == null || string.IsNullOrEmpty(p.ProductId)) continue;
                identifier = String.IsNullOrWhiteSpace(p.Family) ? p.ProductId : p.Family;
                //se lo stesso prod id è contenuto vedo se quello che mi arriva è licenziato e ne do la precedenza
                //Siccome in caso di magix e mbook ho due prodotti che devono essere esclusivi, perchè entrambi master, ma non hanno lo stesso prodid, ho inventato
                //l'attributo family che indica lo stesso codice per prodotti che devono essere esclusivi anche se convivono nella stessa app

                if (ht.Contains(identifier) && (String.Compare(editionID, p.EditionId, StringComparison.InvariantCultureIgnoreCase) == 0 || string.IsNullOrWhiteSpace(editionID)))
                {
                    ProductInfo x = ht[identifier] as ProductInfo;
                    if (p.HasLicensed || x == null)
                        ht.Remove(identifier);
                    else//a questo punto se mago pro e net non hanno licensed metto il pro che ha priorità più bassa e quindi gli do  vantaggi 
                        //( statisticamente è più probabile pro che ent_ prblematico analizzata con guarnaccia
                    {
                        if (!x.HasLicensed && x.Priority > p.Priority)
                            ht.Remove(identifier);
                        else
                            continue;
                    }
                }
                //caso di stesso prodotto attivato 2 volte per esempio magoSTD e magoPRO: ignoro prendo l'ultimo
                if (String.Compare(editionID, p.EditionId, StringComparison.InvariantCultureIgnoreCase) == 0 || string.IsNullOrWhiteSpace(editionID))
                    ht.Add(identifier, p);
            }


            //UserInfo
            UserinfoTabPage uitp = new UserinfoTabPage(clientStub, this);
            TbcGrids.TabPages.Add(uitp);

            //algoritmo di posizionamento  in base alla priorità idicata in solution,
            //nel caso di priorità guale il posizionamento sarà casuale .
            List<ProductInfo> l = new List<ProductInfo> { };
            foreach (ProductInfo p in ht.Values)
            {
                int priority = p.Priority;
                while (true)
                {
                    if (l.Count == 0)//primo
                    { priority = 0; break; }

                    else if (priority == -1 || priority >= l.Count)//ultimo
                    { priority = l.Count; break; }

                    //cerco la posizione corretta  facendo attenzione di posizionarlo dopo al prodotto con priorità inferiore 
                    ProductInfo p_inlist = l[priority];
                    if (p_inlist == null) break;

                    if (p_inlist.Priority < priority && p_inlist.Priority > -1)
                        priority++;
                    else break;

                }
                l.Insert(priority, p);
            }
            //ora aggiungo le tab nell'ordine impostato dall'algoritmo sopra.
            foreach (ProductInfo p in l)
            {
                ProductTabPage t = new ProductTabPage(clientStub, p, this, startSerial);
                if (t.HasRows)
                    TbcGrids.TabPages.Add(t);
                else
                    clientStub.Diagnostic.Set(Interfaces.DiagnosticType.Warning | Interfaces.DiagnosticType.LogInfo, p.ViewName + " has not any article available for this configuration or this country");
            }
        }

        //---------------------------------------------------------------------
        private void LicensedConfigurationForm_Load(object sender, EventArgs e)
        {
            if (EnableNext != null) EnableNext(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private bool VerifyLoggedUsers()
        {
            int retVal = clientStub.GetLoggedUsersNumber();

            if (retVal > 0)
            {
                DialogResult res = MessageBox.Show(this, LicenceStrings.ReinitWarning, LicenceStrings.MsgTitleWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.No)
                    return false;
            }
            return true;
        }

        delegate void MyDelegate();
        bool reg = false;

        //---------------------------------------------------------------------
        private void InvokeRegister()
        {
            InvokeEnableControls(false);
            MyDelegate X = new MyDelegate(Register);
            AsyncCallback cb = new AsyncCallback(AfterRegister);
            IAsyncResult ar = X.BeginInvoke(cb, null);
        }

        //---------------------------------------------------------------------
        private void Register()
        {
            reg = clientStub.Register();
        }
        //---------------------------------------------------------------------
        private void InvokeClose()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MyDelegate(Close), new object[] { });
            }
            else
                this.Close();

        }

        //---------------------------------------------------------------------
        private void AfterRegister(IAsyncResult ar)
        {
            MyDelegate X = (MyDelegate)((AsyncResult)ar).AsyncDelegate;
            X.EndInvoke(ar);
            AfterRegister();
        }

        //---------------------------------------------------------------------
        private void AfterRegister()
        {
            try
            {
                Wait(false);

                InvokeViewDiagnostic();
                InvokeEnableControls(true);

                if (reg)
                {
                    InvokeClose();
                    return;
                }
            }
            catch (Exception exc)
            {
                Wait(false);
                clientStub.Diagnostic.SetError(exc.Message);
                InvokeViewDiagnostic();
                InvokeEnableControls(true);
            }
        }

        //---------------------------------------------------------------------
        private void BtnOK_Click(object sender, EventArgs e)
        {

            Wait(true);
            if (!VerifyLoggedUsers())
            {
                Wait(false);
                return;
            }
            bool ok = true;
            errors.Clear();
            //la chek dependencyExpression potrebbe essere fatta qui 
            //in modo da avere la visione globale di tutte le pagine compilate, 
            //perchè se guardiamo i prodotti salvati potrebbero essere in una 
            //situazione diversa e quindi il controllo potrebbe dare dei false
            List<ModuleDependecies> list = new List<ModuleDependecies>();
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                bool res = page.Check();
                ok = ok && res;
                foreach (FillingError err in page.GetErrors())
                    errors.Add(err);

                if (page is ProductTabPage)
                    list.AddRange(((ProductTabPage)(page)).GetModuleDependeciesList());
            }
            bool depOK = CheckDependenciesExpression(list);
            ok = (ok && depOK);
            if (!ok)
                ok = ShowErrors();
            if (!ok)
                Wait(false);
            if (ok)
            {
                foreach (ActivationTabPage page in TbcGrids.TabPages)
                    page.Save();

                if (isWizardMode)
                {
                    Register();
                    AfterRegister();

                    return;
                }
                InvokeRegister();
            }
        }


        //---------------------------------------------------------------------------
        private bool CheckDependenciesExpression(List<ModuleDependecies> moduleslist)
        {
            if (moduleslist == null || moduleslist.Count == 0)
                return true;
            bool ok = true;
            foreach (ModuleDependecies md in moduleslist)
            {
                if (!md.licensed) continue;
                try
                {
                    if (DepExpEvaluator.CheckDependenciesExpression(md.expression, moduleslist))
                        continue;
                }
                catch (DependenciesExpressionException exc)
                {
                    clientStub.SetError(String.Format(LicenceStrings.ExceptionParsingDependency, md.localizedName, exc.Message), null, null);
                    continue;
                }
                string message = TranslateExpression(md.expression, moduleslist);

                FillingError err = new FillingError(FillingError.ErrorType.Warning, String.Format(LicenceStrings.DependenciesMissing, md.localizedName, message));
                errors.Add(err); ok = false;
            }
            return ok;
        }

        //---------------------------------------------------------------------------
        private string TranslateExpression(string exp, List<ModuleDependecies> moduleslist)
        {
            string expTranslated = exp;
            expTranslated = exp.Replace("!", String.Format(" {0} ", LicenceStrings.NOT));
            expTranslated = expTranslated.Replace("&", String.Format(CultureInfo.InvariantCulture, " {0} ", LicenceStrings.AND));
            expTranslated = expTranslated.Replace("|", String.Format(CultureInfo.InvariantCulture, " {0} ", LicenceStrings.OR));
            string[] splitted = exp.Split(new char[] { '!', '&', '|' });

            //siccome è successo che il nome di un modulo è contenuto dentro un altro 
            //(vedi inventrory e inventory Location), 
            //succedeva che la replace poteva sostituire anche parti di altri moduli 
            //invalidando la traduzione e rendendola senza senso, 
            //allora ordino la lista in oridne di lunghezza e poi vado al contrario nella sostituzione 
            //in modo da sostituire prima i più lunghi che sicuramente non sono contenuti nei successivi)
            if (splitted.Length > 1)
                InsertionSort(ref splitted);

            for (int i = splitted.Length - 1; i >= 0; i--)
            {
                string token = splitted[i];
                if (String.IsNullOrWhiteSpace(token)) continue;
                string trimmedToken = token.Replace('(', ' ');
                trimmedToken = trimmedToken.Replace(')', ' ');
                trimmedToken = trimmedToken.Trim();
                foreach (ModuleDependecies md in moduleslist)
                {
                    if (String.Compare(md.name, trimmedToken, true, CultureInfo.InvariantCulture) == 0)
                    {
                        expTranslated = expTranslated.Replace(trimmedToken, md.localizedName);
                        break;
                    }
                }
            }
            return expTranslated;
        }
        //---------------------------------------------------------------------------
        public void InsertionSort(ref string[] splitted)
        {
            int x = splitted.Length;
            int i;
            int j;
            string indice;

            for (i = 1; i < x; i++)
            {
                indice = splitted[i];
                j = i;

                while ((j > 0) && (splitted[j - 1].Length > indice.Length))
                {
                    splitted[j] = splitted[j - 1];
                    j = j - 1;
                }
                splitted[j] = indice;
            }
        }


        private delegate void EnableWaitingControlFunction(bool enable);
        //---------------------------------------------------------------------
        private void EnableWaitingControl(bool enable)
        {
            if (enable)
                waitingControl1.BringToFront();

            waitingControl1.Visible = enable;
        }

        //---------------------------------------------------------------------
        public void Wait(bool wait)
        {
            BeginInvoke(new EnableWaitingControlFunction(EnableWaitingControl), new object[] { wait });
        }

        //---------------------------------------------------------------------
        private void InvokeViewDiagnostic()
        {
            BeginInvoke(new MyDelegate(ViewDiagnostic), new object[] { });
        }

        //---------------------------------------------------------------------------
        private void ViewDiagnostic()
        {
            DiagnosticViewer.ShowDiagnostic(clientStub.Diagnostic);

        }

        //---------------------------------------------------------------------------
        private void InvokeEnableControls(bool enable)
        {
            SafeGui.ControlEnabled(btnCancel, enable);
            SafeGui.ControlEnabled(BtnOK, enable);
            SafeGui.ControlEnabled(TbcGrids, enable);
        }

        //---------------------------------------------------------------------
        public bool ShowErrors()
        {

            StringBuilder sb = new StringBuilder();
            bool showError = false;
            bool onlyWarning = true;
            bool procede = true;
            foreach (FillingError error in errors)
            {
                if (error.Error == FillingError.ErrorType.None) continue;
                onlyWarning = (onlyWarning && error.Error == FillingError.ErrorType.Warning);
                showError = true;
                sb.Append(error.ToString());
            }
            if (showError)
            {
                ErrorViewer dialog = new ErrorViewer(sb.ToString(), onlyWarning);
                DialogResult r = dialog.ShowDialog(this);
                procede = (r == DialogResult.OK);
            }
            return procede;
        }

        //---------------------------------------------------------------------
        internal bool ThereIsADemoOrDvlpMasterProduct(bool alsornfs)
        {
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                if (page is ProductTabPage)
                {
                    bool ok = ((ProductTabPage)page).IsMasterDemoOrDvlpOrRNFS(alsornfs);
                    if (ok) return true;

                }
            }
            return false;
        }

        //---------------------------------------------------------------------
        internal bool ThereIsARnfsMasterProduct()
        {
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                if (page is ProductTabPage)
                {
                    bool ok = ((ProductTabPage)page).IsMasterRNFS();
                    if (ok) return true;

                }
            }
            return false;
        }

        //---------------------------------------------------------------------
        internal bool ThereIsADvlpKProduct()
        {
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                if (page is ProductTabPage)
                {
                    bool ok = ((ProductTabPage)page).ThereIsADvlpKProduct();
                    if (ok) return true;

                }
            }
            return false;
        }


        //---------------------------------------------------------------------
        internal string GetCountry()
        {
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                if (page is UserinfoTabPage)
                    return ((UserinfoTabPage)page).GetCountry();
            }
            return null;
        }
        //---------------------------------------------------------------------
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicensedConfigurationForm));
            this.TbcGrids = new System.Windows.Forms.TabControl();
            this.ImageList = new System.Windows.Forms.ImageList(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LblInfo = new System.Windows.Forms.Label();
            this.waitingControl1 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.WaitingControl();
            this.LnkProxysettings = new System.Windows.Forms.LinkLabel();
            this.PbProxy = new System.Windows.Forms.PictureBox();
            this.BtnSms = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).BeginInit();
            this.SuspendLayout();
            // 
            // TbcGrids
            // 
            resources.ApplyResources(this.TbcGrids, "TbcGrids");
            this.TbcGrids.ImageList = this.ImageList;
            this.TbcGrids.Name = "TbcGrids";
            this.TbcGrids.SelectedIndex = 0;
            // 
            // ImageList
            // 
            this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
            this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList.Images.SetKeyName(0, "ResultRed.png");
            this.ImageList.Images.SetKeyName(1, "ResultGreen.png");
            this.ImageList.Images.SetKeyName(2, "user.gif");
            this.ImageList.Images.SetKeyName(3, "ResultGrey.png");
            this.ImageList.Images.SetKeyName(4, "ResultOrange.png");
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LblInfo
            // 
            resources.ApplyResources(this.LblInfo, "LblInfo");
            this.LblInfo.Name = "LblInfo";
            // 
            // waitingControl1
            // 
            resources.ApplyResources(this.waitingControl1, "waitingControl1");
            this.waitingControl1.BackColor = System.Drawing.Color.Transparent;
            this.waitingControl1.Name = "waitingControl1";
            // 
            // LnkProxysettings
            // 
            resources.ApplyResources(this.LnkProxysettings, "LnkProxysettings");
            this.LnkProxysettings.Name = "LnkProxysettings";
            this.LnkProxysettings.TabStop = true;
            this.LnkProxysettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkProxysettings_LinkClicked);
            // 
            // PbProxy
            // 
            resources.ApplyResources(this.PbProxy, "PbProxy");
            this.PbProxy.Name = "PbProxy";
            this.PbProxy.TabStop = false;
            // 
            // BtnSms
            // 
            resources.ApplyResources(this.BtnSms, "BtnSms");
            this.BtnSms.Name = "BtnSms";
            this.BtnSms.UseVisualStyleBackColor = true;
            this.BtnSms.Click += new System.EventHandler(this.BtnSms_Click);
            // 
            // LicensedConfigurationForm
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.BtnSms);
            this.Controls.Add(this.PbProxy);
            this.Controls.Add(this.LnkProxysettings);
            this.Controls.Add(this.LblInfo);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.TbcGrids);
            this.Controls.Add(this.waitingControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "LicensedConfigurationForm";
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LicensedConfigurationForm_FormClosing);
            this.Load += new System.EventHandler(this.LicensedConfigurationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PbProxy)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        //---------------------------------------------------------------------
        private void LicensedConfigurationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = reg ? DialogResult.OK : DialogResult.No;
        }

        //---------------------------------------------------------------------
        private void LnkProxysettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProxySettings state = clientStub.GetProxySetting();
            ProxyFirewall proxyForm = new ProxyFirewall(state);

            if (proxyForm.ShowDialog() != DialogResult.OK)
                return;

            ProxySettings newState = proxyForm.GetVisualState();
            clientStub.SetProxySetting(newState);
        }

        //---------------------------------------------------------------------
        private void BtnSms_Click(object sender, EventArgs e)
        {
            //validazione con sms solo se non è prima attivazione....
            //(if !primaattivazione)
            string url = BasePathFinder.BasePathFinderInstance.PingViaSMSPage;
            Process.Start(url);
            this.Close();
        }


        //---------------------------------------------------------------------
        internal bool WizardNext()
        {
            BtnOK_Click(null, null);
            return clientStub.ProductActivated();
        }

        //---------------------------------------------------------------------
        internal void WizardBack()
        {

        }

        bool isWizardMode = false;
        //---------------------------------------------------------------------------
        internal void WizardMode()
        {
            isWizardMode = true;
            BtnSms.Visible = false;
            BtnOK.Visible = false;
            btnCancel.Visible = false;
            AutoScroll = false;
            TbcGrids.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            foreach (ActivationTabPage page in TbcGrids.TabPages)
            {
                if (page is ProductTabPage)
                {
                    ((ProductTabPage)page).WizardSettings(TbcGrids.Size);
                    page.Size = TbcGrids.Size;
                    page.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
                }
                //if (page is UserinfoTabPage)
                //{
                //    AutoScroll = true;
                //}
            }
        }

    }

    //=========================================================================
    public class UserinfoTabPage : ActivationTabPage
    {
        private UserInfoForm uif;
        ClientStub clientStub;
        LicensedConfigurationForm ParentForm;

        //---------------------------------------------------------------------
        public UserinfoTabPage(ClientStub clientStub, LicensedConfigurationForm parent)
            : base(LicenceStrings.UserInfo)
        {
            this.ParentForm = parent;
            this.clientStub = clientStub;
            UseVisualStyleBackColor = true;
            ImageIndex = 2;
            uif = new UserInfoForm(clientStub);
            this.Controls.Add(uif);
            uif.Modified += new EventHandler(Modified);
            AutoScroll = true;
        }

        //---------------------------------------------------------------------
        public override bool Check()
        {
            return uif.GetErrors().Count == 0;
        }
        //---------------------------------------------------------------------
        public override IList<FillingError> GetErrors()
        {
            return uif.GetErrors();
        }
        //---------------------------------------------------------------------
        public override bool Save()
        {
            if (IsModified)
            {
                IsModified = false;
                return uif.Save();
            }
            return true;
        }

        //---------------------------------------------------------------------
        public string GetCountry()
        {
            return uif.GetCountry();
        }
    }

    //=========================================================================
    public class ActivationTabPage : TabPage
    {
        public bool IsModified = false;

        //---------------------------------------------------------------------
        public ActivationTabPage(string title)
            : base(title)
        { }

        //---------------------------------------------------------------------
        public virtual bool Check()
        {
            return true;
        }
        //---------------------------------------------------------------------
        public virtual bool Save()
        {
            return true;
        }
        //---------------------------------------------------------------------
        public virtual IList<FillingError> GetErrors()
        {
            return new List<FillingError> { };
        }

        //---------------------------------------------------------------------
        protected void Modified(object sender, EventArgs e)
        {
            //aggiungo l'indicatore di file modificato non salvato.
            if (this.Text.IndexOf('*') < 0)
                this.Text = this.Text + "*";
            IsModified = true;

        }
    }

    //=========================================================================
    public class ProductTabPage : ActivationTabPage
    {
        private ProductInfo product;
        ClientStub clientStub;
        ProductGrid pGrid;
        LicensedConfigurationForm ParentForm;
        public bool HasRows { get; set; }
        //---------------------------------------------------------------------
        public ProductTabPage(ClientStub clientStub, ProductInfo product, LicensedConfigurationForm parent, SerialNumberInfo startSerial)
            : base(product.ViewName)//nome del file solution che sarà il titolo della tab
        {

            this.ParentForm = parent;
            this.product = product;
            this.clientStub = clientStub;
            UseVisualStyleBackColor = true;
            if (product.HasLicensed)
                ImageIndex = 1;
            else
                ImageIndex = 3;
            if (!clientStub.ProductActivated())
                ImageIndex = 0;
            if (parent.FirstTime)
                ImageIndex = 3;

            pGrid = new ProductGrid(clientStub, product, startSerial, parent);
            pGrid.Dirty += new EventHandler(Modified);
            pGrid.PostInitializeComponent();
            if (pGrid.NeedlessActivated) ImageIndex = 1;

            HasRows = pGrid.HasRows();//per mdc che per country non allowed veniva tab vuota
            if (HasRows) this.Controls.Add(pGrid);

        }


        //---------------------------------------------------------------------
        public override bool Check()
        {
            return pGrid.Check();
        }

        //---------------------------------------------------------------------
        public List<ModuleDependecies> GetModuleDependeciesList()
        {
            return pGrid.ModuleDependeciesList;
        }

        //---------------------------------------------------------------------
        public override IList<FillingError> GetErrors()
        {
            return pGrid.GetErrors();
        }

        //---------------------------------------------------------------------
        public override bool Save()
        {
            if (IsModified)
            {
                IsModified = false;
                return pGrid.Save();
            }
            return true;
        }
        //---------------------------------------------------------------------
        public bool IsMasterRNFS()
        {
            return pGrid.IsMasterRNFS();
        }

        //---------------------------------------------------------------------
        public bool IsMasterDemoOrDvlpOrRNFS(bool alsornfs)
        {
            return pGrid.IsMasterDemoOrDVLPOrRNFS(alsornfs);
        }
        //---------------------------------------------------------------------
        public bool ThereIsADvlpKProduct()
        {
            return pGrid.ThereIsADvlpKProduct();
        }



        //---------------------------------------------------------------------
        internal void WizardSettings(Size s)
        {
            pGrid.Size = new Size(s.Width - 7, s.Height - 25);
        }

    }


}
