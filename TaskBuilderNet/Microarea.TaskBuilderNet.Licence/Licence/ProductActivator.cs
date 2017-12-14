using System;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//================================================================================
    public class ProductActivator
    {
        private LoginManager loginManager;
        private Diagnostic diagnosticActivator = new Diagnostic("ProductActivator");
        public event ModificationManager EnableNext;
        public event EventHandler Pee; //:-)
        public event EventHandler StopPee;
        public Diagnostic DiagnosticActivator { get { return diagnosticActivator; } }
       

        Panel wizardPageParent = null;
        ActivateSerial asf; LicensedConfigurationForm lcf;

        //--------------------------------------------------------------------------------
        public ProductActivator(LoginManager loginManager)
        {
            this.loginManager = loginManager;
        }

        //---------------------------------------------------------------------
        public bool ShowAboutAndActivate(IWin32Window owner)
        {
            if (IsFirstActivation())
            {

                try
                {
                    asf = new ActivateSerial(new ClientStub(BasePathFinder.BasePathFinderInstance, loginManager));
                }
                catch (Exception e)
                {
                    diagnosticActivator.Set(DiagnosticType.Error, "Impossible to continue, exception at " + e.Message, "");
                    DiagnosticViewer.ShowDiagnostic(diagnosticActivator);
                    return false;
                }

                asf.StartPosition = FormStartPosition.CenterParent;
                asf.ShowDialog(owner);
                WriteMasterSolutionName();
                return ProductActivated();
            }
            else
            {
                Pee?.Invoke(null,null);
                //dare messaggio di spiegazione, qualcosa tipo: Attenzione, il prodotto non è attivato, molto probabilmente il problema è dovuto ad una mancata convalida dell'attivazione che avviene periodicamente tramite una connessione ad internet, 
                //puoi convalidare il prodotto adesso, se hai una connessione internet funzionante.
                string productName = GetProductName();

               
                MessageBox.Show(LicenceStrings.NoActivatedExplanation, productName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);


                lcf = new LicensedConfigurationForm
                    (new ClientStub(BasePathFinder.BasePathFinderInstance), productName, true);

                lcf.StartPosition = FormStartPosition.CenterParent;

                DialogResult res = lcf.ShowDialog(owner);

                bool ok = ProductActivated();
                if (res == DialogResult.No)
                    return ok;

                if (!ok)
                {//potrebbero essere problemi di  permessi sulla machine keys o sulle cartelle dell'installazione.
                    string pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string pathMachineKeys = "Microsoft\\Crypto\\RSA\\MachineKeys";
                    string path = Path.Combine(pathAppData, pathMachineKeys);
                    // "Error 230-Permission. Please verify the permission on folder Standard or on folder {0} (may be you have to set Everyone - FullControl on them)."
                    diagnosticActivator.Set(DiagnosticType.Error, (String.Format(LicenceStrings.Permission230, path)));
                    DiagnosticViewer.ShowDiagnostic(diagnosticActivator);
                    return ShowAboutAndActivate(owner);
                }
                StopPee?.Invoke(null, null);
                WriteMasterSolutionName();
                return true;

            }
        }

        //---------------------------------------------------------------------
        private void WriteMasterSolutionName()
        {
            InstallationData.ServerConnectionInfo.MasterSolutionName = loginManager.GetMasterSolution();
            InstallationData.ServerConnectionInfo.UnParse(BasePathFinder.BasePathFinderInstance.ServerConnectionFile);
        }

        //---------------------------------------------------------------------
        public bool ProductActivated()
        {
            int daysToExpiration;
            ActivationState s = loginManager.GetActivationState(out daysToExpiration);
            return (s != ActivationState.Disabled && s != ActivationState.NoActivated);
        }

        //---------------------------------------------------------------------
        public string GetProductName()
        {
            if (!String.IsNullOrEmpty(loginManager.GetMasterSolutionBrandedName()))
                return String.Format("{0} {1}", LicenceStrings.My, loginManager.GetMasterSolutionBrandedName());
            return string.Empty;
        }

        //---------------------------------------------------------------------
        private bool IsFirstActivation()
        {
            return loginManager.IsVirginActivation();
        }

        //---------------------------------------------------------------------
        public bool ShowAboutAndActivateForWizard(Panel parent)
        {
            if (parent == null) return false;
            wizardPageParent = parent;

            if (IsFirstActivation())
            {
                try
                {
                    asf = new ActivateSerial(new ClientStub(BasePathFinder.BasePathFinderInstance, loginManager));
                    asf.WizardMode();
                }
                catch (Exception e)
                {
                    diagnosticActivator.Set(DiagnosticType.Error, "Impossible to continue, exception at " + e.Message, "");
                    DiagnosticViewer.ShowDiagnostic(diagnosticActivator);
                    return false;
                }

                asf.FormBorderStyle = FormBorderStyle.None;
                asf.TopLevel = false;
                asf.EnableNext += new ModificationManager(asf_EnableNext);
                asf.EnableLicensedForm += new ModificationManager(asf_EnableLicensedForm);
                asf.Dock = DockStyle.Fill;
                parent.Controls.Add(asf);

                asf.Show();
                return ProductActivated();
            }
            else
            {
                EnableLicensedForm();
                return true;
            }
        }

        //---------------------------------------------------------------------
        void asf_EnableLicensedForm(object sender, EventArgs e)
        {
            EnableLicensedForm();
        }

        //---------------------------------------------------------------------
        void EnableLicensedForm()
        {
            lcf = new LicensedConfigurationForm
                (new ClientStub(BasePathFinder.BasePathFinderInstance), GetProductName(), true);
            lcf.MinimumSize = new System.Drawing.Size(0, 0);
            lcf.MaximumSize = new System.Drawing.Size(0, 0);
            lcf.FormBorderStyle = FormBorderStyle.None;
            lcf.Location = new System.Drawing.Point(0, 0);
            lcf.Size = new System.Drawing.Size(lcf.Width + 25, wizardPageParent.Height);
            lcf.TopLevel = false;
            lcf.WizardMode();
            lcf.EnableNext += new ModificationManager(asf_EnableNext);
            wizardPageParent.Controls.Clear(); asf = null;
            wizardPageParent.Controls.Add(lcf);
            lcf.Show();

            bool ok = ProductActivated();
        }

        //---------------------------------------------------------------------
        void asf_EnableNext(object sender, EventArgs e)
        {
            if (EnableNext != null)
                EnableNext(sender, e);
        }

        //---------------------------------------------------------------------
        public bool WizardNext()
        {
            if (asf != null)
                return asf.WizardNext();
            else if (lcf != null)
                return lcf.WizardNext();
            return true;
        }

        //---------------------------------------------------------------------
        public void WizardBack()
        {
            if (asf != null)
                asf.WizardBack();
            else if (lcf != null)
                lcf.WizardBack();
        }
    }
}
