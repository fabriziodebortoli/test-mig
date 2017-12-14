using System;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	//============================================================================
	public partial class ActivateSerial : Form
	{
		private ClientStub clientStub ;
		bool canClose = false;
		ProductInfo foundProduct = null;
        private SerialNumberInfo integrativeSerial = null;
        public event ModificationManager EnableNext;
        public event ModificationManager EnableLicensedForm;
		//---------------------------------------------------------------------------
		public ActivateSerial(ClientStub clientStub)
		{
			InitializeComponent();
            this.DialogResult = DialogResult.OK;
			this.clientStub = clientStub;
			StbTheSerial.Modified += new ModificationManager(StbTheSerial_Modified);
			string prod = clientStub.GetBrandedProductTitle();
            if (prod.IsNullOrEmpty())
                this.Text = LicenceStrings.ActivateProduct;
            else
                this.Text = String.Format("{0} by {1} - {2}", prod, clientStub.GetBrandedProducerName(), LicenceStrings.ActivateProduct);

			LblSerial.Text = String.Format(LblSerial.Text, prod);
		}

        bool isWizardMode = false;
        //---------------------------------------------------------------------------
        internal void WizardMode()
        {
            isWizardMode = true;
            BtnCancel.Visible = false;
            BtnOK.Visible = false;
            labelForWizardMode.Visible = true;
            label1.Visible = false;
            label2.Visible = false;
            pictureBox2.Visible = false;
        }

		//---------------------------------------------------------------------------
		void StbTheSerial_Modified(object sender, EventArgs e)
		{
			LblInfo.Text = null;
			LblSerialType.Text = null;
			bool ok = StbTheSerial.IsValid && StbTheSerial.Complete;
			BtnOK.Enabled = linkLabel1.Enabled = ok;
            if (EnableNext != null) EnableNext(sender, e);
			if (!StbTheSerial.IsValid)
				LblInfo.Text = LicenceStrings.SerialNumberWrongFormat;
			else if (ok && StbTheSerial.Serial.IsDemo(CalTypeEnum.Master))
				LblSerialType.Text = LicenceStrings.DemoVersion;//"Demo Version";
			else if (ok && StbTheSerial.Serial.IsDeveloper(CalTypeEnum.Master))
				LblSerialType.Text = LicenceStrings.DvlpVersion;//"Development Version";
			else if (ok && StbTheSerial.Serial.IsNFS(CalTypeEnum.Master))
				LblSerialType.Text = LicenceStrings.NFSVersion;//"NotForSale Version";

            else if (ok && StbTheSerial.Serial.IsTest(CalTypeEnum.Master))
                LblSerialType.Text = LicenceStrings.TestVersion;
            else if (ok && StbTheSerial.Serial.IsBackUp(CalTypeEnum.Master))
                LblSerialType.Text = LicenceStrings.BackupVersion;
            else if (ok && StbTheSerial.Serial.IsStandAlone(CalTypeEnum.Master))
                LblSerialType.Text = LicenceStrings.StandAloneVersion;
		}

        bool extension = false;
		//---------------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{

            foundProduct = null;
            //su ok si legge  il seriale, se è demo, rnfs, dfns, dvlp, 
            //edition e db per riportare poi la corretta solution.
            //di cui ai tag prodid='MN' editio='PRO'
            //prendo la Solution, ne faccio unc Licensed impostando il serial number al  basicserver che trovo.

            //non ci sono prodotti caricati...
            if (clientStub.ActivationObj.Products.Length == 0)
            {
                clientStub.TryToReloadActivationObject();//se arriva qui perch` non trova nessun prodotto potrebbe capitare subito dopo installazione, non ho capito perchè allora provo a fargli ricaricare solo la parte di  configurazione prodotti.
                if (clientStub.ActivationObj.Products.Length == 0)
                {
                    clientStub.SetError(LicenceStrings.NoProducts, null, null);
                    ViewDiagnostic();

                    return;
                }
            }

			bool demo = StbTheSerial.Serial.IsDemo(CalTypeEnum.Master);
            bool integrative = StbTheSerial.Serial.IsIntegrative(CalTypeEnum.Master);
            bool special = StbTheSerial.Serial.IsSpecial(CalTypeEnum.Master) ;
            extension = integrative;

           if( !FoundProduct())
			{
				ViewDiagnostic();
				return ;
			}
            if (integrative)
            {
                IntegrativeManagement im = new IntegrativeManagement(StbTheSerial.Serial, LblSerialType.Text);
                DialogResult res = im.ShowDialog();
                if (res == DialogResult.OK)
                {
                    integrativeSerial = im.Serial;
                    CreateLicenseds(special, integrative, demo);
                    return;
                }
                if (res == DialogResult.Cancel) return;
            }

			CreateLicenseds(special, integrative, demo);

		}
       
		//---------------------------------------------------------------------------
		private bool FoundProduct()
        {
            
			foundProduct = clientStub.GetProductBySerial(StbTheSerial.Serial);

             return (foundProduct != null);
		}

        //---------------------------------------------------------------------------
		private void ViewDiagnostic()
		{
			DiagnosticViewer.ShowDiagnostic(clientStub.Diagnostic);
		}

		private delegate void EnableWaitingControlFunction(bool enable);
		//---------------------------------------------------------------------
		private void EnableWaitingControl(bool enable)
		{
			waitingControl1.BringToFront();
			waitingControl1.Visible = enable;
		}

		//---------------------------------------------------------------------
		public void Wait(bool wait)
		{
			BeginInvoke(new EnableWaitingControlFunction(EnableWaitingControl), new object[] { wait });
			SafeGui.ControlEnabled(BtnCancel, !wait);
			SafeGui.ControlEnabled(BtnOK, !wait);
            SafeGui.ControlEnabled(linkLabel1, !wait);
			SafeGui.ControlEnabled(LnkProxy, !wait);
			SafeGui.ControlEnabled(StbTheSerial, !wait);
			SafeGui.ControlVisible(LblInfo, !wait);
			SafeGui.ControlVisible(LblSerialType, !wait);
		}

		delegate void MyRegistrationDelegate(bool pre);
        delegate void MyDelegate();
		bool reg = false;
		//---------------------------------------------------------------------
		private void InvokeRegister(bool pre)
		{
            MyRegistrationDelegate X = new MyRegistrationDelegate(Register);
			AsyncCallback cb = new AsyncCallback(AfterRegister);
			IAsyncResult ar = X.BeginInvoke(pre, cb, null);
		}

		//---------------------------------------------------------------------
		private void Register(bool pre)
		{
            if (pre)
                reg = clientStub.PreRegister();
            else
                reg = clientStub.Register();
		}

		//---------------------------------------------------------------------
		private void InvokeViewDiagnostic()
		{
			BeginInvoke(new MyDelegate(ViewDiagnostic), new object[] { });
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
            MyRegistrationDelegate X = (MyRegistrationDelegate)((AsyncResult)ar).AsyncDelegate;
			X.EndInvoke(ar);
            AfterRegister();
		}

        //---------------------------------------------------------------------
        private void AfterRegister()
        {
         
            canClose = reg && clientStub.ProductActivated();
            bool canviewerror = reg && (clientStub.ProductActivated() || extension);
            Wait(false);
            SetCursor(Cursors.Default);
            if (!canviewerror)
                InvokeViewDiagnostic();

            if (reg)
            {
                if (!extension) InvokeViewDiagnostic();
                else
                {
                    extension = false;
                    UserInfo ui = clientStub.GetUserInfo();
                    string userName = LicenceStrings.UnknownUser;
                    string userCodFisc = LicenceStrings.UnknownUser;
                    if (ui != null)
                    {
                        if (!string.IsNullOrWhiteSpace(ui.Company))
                            userName = ui.Company;
                        if (!string.IsNullOrWhiteSpace(ui.CodFisc))
                            userCodFisc = ui.CodFisc;
                    }
                    string msg = String.Format(LicenceStrings.VerifyUser, userName, userCodFisc);
                    DialogResult r = SafeMessageBox.Show(this, msg, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
                    if (r == DialogResult.Yes)
                    {
                        Wait(true);
                        if (isWizardMode) Register(false); 
                        else InvokeRegister(false);
                    }
                    if (r == DialogResult.No)
                    {
                        if (foundProduct != null)
                            clientStub.DeleteLicensed(foundProduct.CompleteName);
                        clientStub.DeleteUserInfo();
                        clientStub.ReloadActivation();
                    }
                }
                InvokeClose();
                return;
            }
        }
        //---------------------------------------------------------------------------
        private void CreateLicenseds(bool special, bool integrative, bool demo)
        {
            SetCursor(Cursors.WaitCursor);
            bool res = clientStub.SaveLicenseds(foundProduct, special || integrative, true, StbTheSerial.Serial, integrativeSerial);
            if (!res)
            {
                SetCursor(Cursors.Default);
                clientStub.SetError(LicenceStrings.ErrorSavingLicence, null, null);
                ViewDiagnostic();
                return;

            }
            foundProduct.HasLicensed = true;

            if (isWizardMode)
            {
                Wait(true);
                Register(integrative);
                AfterRegister();
                SetCursor(Cursors.Default);
                if (!clientStub.ProductActivated())
                    OpenLicensedConfigurationFormForWizard();

                return;
            }

            if (special || integrative)
            {
                Wait(true);
                InvokeRegister(integrative);
            }
            else  //visualizza griglia per attivazioni e info utente, salva e dopo attiva
                OpenLicensedConfigurationForm();
            SetCursor(Cursors.Default);
        }
        //---------------------------------------------------------------------
        private void OpenLicensedConfigurationFormForWizard()
        {
            if (EnableLicensedForm != null)
                EnableLicensedForm(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private void OpenLicensedConfigurationForm()
        {
            if (foundProduct == null) return;

            LicensedConfigurationForm lcf = new LicensedConfigurationForm(clientStub, foundProduct.CompleteName, StbTheSerial.Serial, foundProduct.EditionId, false);
            lcf.StartPosition = FormStartPosition.CenterParent;
            if (isWizardMode)lcf.WizardMode();
            if (lcf.ShowDialog(this) == DialogResult.OK)
            {
                canClose = true;
                this.Close();
            }
            
        }

		//---------------------------------------------------------------------
		private delegate void SetCursorDelegate(Cursor cursor);

		//---------------------------------------------------------------------
		private void SetCursor(Cursor cursor)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new SetCursorDelegate(SetCursor), new object[] { cursor });
			}
			else
				Cursor = cursor;
		}

		//---------------------------------------------------------------------
		private void LnkProxy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ProxySettings state = clientStub.GetProxySetting();
			ProxyFirewall proxyForm = new ProxyFirewall(state);

			if (proxyForm.ShowDialog() != DialogResult.OK)
				return;

			ProxySettings newState = proxyForm.GetVisualState();
			clientStub.SetProxySetting(newState);
		}

		//---------------------------------------------------------------------
		private void ActivateSerial_FormClosing(object sender, FormClosingEventArgs e)
		{
            if (canClose && DialogResult == DialogResult.Abort)
            {
                if(foundProduct != null)
                    clientStub.DeleteLicensed(foundProduct.CompleteName);
                clientStub.DeleteUserInfo();
                clientStub.ReloadActivation();
            }
			e.Cancel = !canClose;
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, EventArgs e)
		{
            this.DialogResult = DialogResult.Abort;
			canClose = true;
		}

        //---------------------------------------------------------------------
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!FoundProduct())
            {
                ViewDiagnostic();
                return;
            }
            OpenLicensedConfigurationForm();

        }

        //---------------------------------------------------------------------
        internal bool WizardNext()
        {
             BtnOK_Click(null, null) ;
            return clientStub.ProductActivated();
        }

        //---------------------------------------------------------------------
        internal void WizardBack()
        {
            BtnCancel_Click(null, null);
        }

        private void BtnOK_EnabledChanged(object sender, EventArgs e)
        {

        }

     
    }
}
