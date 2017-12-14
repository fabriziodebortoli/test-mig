using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	internal enum CustomizationStatus
	{
		AddApplication,
		AddModule,
		RenameApplication,
		RenameModule
	}

	//=========================================================================
	internal partial class AddRenameCustomization : ThemedForm
	{
		private readonly string defaultNewApp = Resources.NewApplicationNameWithParameter;
		private readonly string defaultNewModule = Resources.NewModuleNameWithParameter;

		private CustomizationStatus status = CustomizationStatus.AddApplication;
		private string initialApplication = string.Empty;
		private string initialModule = string.Empty;
		private string applicationNameMemento;
		private List<string> listModuleNames = new List<string>();

		/// <remarks/>
		public string ApplicationName { get { return txtNewApplicationName.Text; } }
		/// <remarks/>
		public string ModuleName { get { return txtNewCustomizationName.Text; } }
		/// <remarks/>
		public bool CreateApplicationInStandardFolder { get { return CkbCreateApplicationInStandardFolder.Checked; } }
		
		/// <summary>
		/// per AddApplication, application e module sono opzionali per 
		/// per AddModule, module è opzionale
		/// per RenameApplication, module è opzionale
		/// per RenameModule, sono obbligatori sia application che module
		/// </summary>
		/// <param name="status"></param>
		/// <param name="listModuleNames"></param>
		/// <param name="initialApplication"></param>
		/// <param name="initialModule"></param>
		//-----------------------------------------------------------------------------
		public AddRenameCustomization(CustomizationStatus status, List<string> listModuleNames = null, string initialApplication = "", string initialModule = "")
		{
			InitializeComponent();
			this.status = status;
			this.initialApplication = initialApplication;
			this.initialModule = initialModule;
			if (listModuleNames != null)
				this.listModuleNames = listModuleNames;

			CkbCreateApplicationInStandardFolder.Visible = BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable();

			InitializeForm();
		}

		//-----------------------------------------------------------------------------
		private void InitializeForm()
		{
			switch (status)
			{
				case CustomizationStatus.AddApplication:
					lblDescription.Text = Resources.SelectApplicationAndModuleName;
					txtNewApplicationName.Text = GenerateNewApplicationName();
					txtNewCustomizationName.Text =  string.Format(defaultNewModule, 1);
					break;
				case CustomizationStatus.AddModule:
					lblDescription.Text = Resources.SelectNewModule;
					GenerateNewModuleName();
					txtNewApplicationName.Enabled = false;
					CkbCreateApplicationInStandardFolder.Visible = false;
					break;
				case CustomizationStatus.RenameApplication:
					lblDescription.Text = Resources.RenameApplicationName;
					txtNewCustomizationName.Text = initialModule;
					txtNewCustomizationName.Visible = false;
					lblModuleName.Visible = false;
					txtNewApplicationName.Text = initialApplication;
					CkbCreateApplicationInStandardFolder.Visible = false;
					break;
				case CustomizationStatus.RenameModule:
					lblDescription.Text = Resources.RenameModuleName;
					txtNewCustomizationName.Text = initialModule;
					txtNewApplicationName.Text = initialApplication;
					txtNewApplicationName.Enabled = false;
					CkbCreateApplicationInStandardFolder.Visible = false;
					break;
				default:
					break;
			}
			this.Text = GetFormText();

			btnAdd.Text = (status == CustomizationStatus.RenameApplication || status == CustomizationStatus.RenameModule)
				? Resources.Rename
				: Resources.Add;
		}

		//-----------------------------------------------------------------------------
		private string GetFormText()
		{
			IList<IEasyBuilderApp> easyBuilderApps = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(initialApplication);
			if (easyBuilderApps == null || easyBuilderApps.Count == 0)
			{
				return (status == CustomizationStatus.RenameApplication || status == CustomizationStatus.RenameModule)
						 ? Resources.RenameCustomization
						 : Resources.AddingNewCustomization;
			}

			switch (easyBuilderApps[0].ApplicationType)
			{
				case ApplicationType.Customization:
					return (status == CustomizationStatus.RenameApplication || status == CustomizationStatus.RenameModule)
						 ? Resources.RenameCustomization
						 : Resources.AddingNewCustomization;

				case ApplicationType.Standardization:
					return (status == CustomizationStatus.RenameApplication || status == CustomizationStatus.RenameModule)
						 ? Resources.RenameStandardization
						 : Resources.AddingNewStandardization;
				default:
					break;
			}
			return (status == CustomizationStatus.RenameApplication || status == CustomizationStatus.RenameModule)
						 ? Resources.RenameCustomization
						 : Resources.AddingNewCustomization;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (DialogResult != DialogResult.OK)
				return;

			switch (status)
			{
				case CustomizationStatus.AddApplication:
				case CustomizationStatus.AddModule:
					if (!IsValidApplicationName())
					{
						e.Cancel = true;
						MessageBox.Show(this, Resources.InvalidAppName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					if (!IsValidModuleName())
					{
						MessageBox.Show(this, Resources.InvalidModuleName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						e.Cancel = true;
						return;
					}

					if (ApplicationExistOrIsNotCustomizable(ApplicationName))
					{
						MessageBox.Show(this, Resources.ApplicationExistOrIsNotCustomizable, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						e.Cancel = true;
						return;
					}

					if (ExistsModule(ModuleName))
					{
						MessageBox.Show(this, Resources.AlreadyExistingModule, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						e.Cancel = true;
						return;
					}

					if (CkbCreateApplicationInStandardFolder.Checked && cbxRegisteredSolutions.SelectedItem == null)
					{
						DialogResult res = MessageBox.Show(this, Resources.ConfirmNewAppRegistration, this.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if (res != System.Windows.Forms.DialogResult.OK)
							e.Cancel = true;
						//else
						//    RegisterSolution();

						return;
					}
					break;
				case CustomizationStatus.RenameApplication:
					if (!IsValidApplicationName())
					{
						e.Cancel = true;
						MessageBox.Show(this, Resources.InvalidAppName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
					
					if (initialApplication.CompareNoCase(ApplicationName))
						return;

					if (ExistsApplication(ApplicationName))
					{
						MessageBox.Show(this, Resources.AlreadyExistingApplication, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						e.Cancel = true;
						return;
					}
					break;
				case CustomizationStatus.RenameModule:
					if (!IsValidModuleName() )
					{
						e.Cancel = true;
						MessageBox.Show(this, Resources.InvalidModuleName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					if (initialModule.CompareNoCase(ModuleName))
						return;

					if (ExistsModule(ModuleName))
					{
						MessageBox.Show(this, Resources.AlreadyExistingModule, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						e.Cancel = true;
						return;
					}
					break;
				default:
					break;
			}
		}

		////-----------------------------------------------------------------------------
		//private void RegisterSolution()
		//{
		//    string solutionName = cbxRegisteredSolutions.Text;

		//    using (CrypterWrapper crypter = new CrypterWrapper(CommonFunctions.GetProxySettings()))
		//    {
		//        bool bSolutionAlreadyRegistered = false;
		//        crypter.IntegratedSolutionExists(
		//            solutionName,
		//            out bSolutionAlreadyRegistered
		//            );

		//        if (bSolutionAlreadyRegistered)
		//        {
		//            //TODO MATTEO: gestire messagigo di errore.
		//            return;
		//        }

		//        crypter.RegisterIntegratedSolution(
		//            Crypto.Decrypt(Settings.Default.Password),
		//            Settings.Default.Username,
		//            solutionName,
		//            String.Empty,
		//            String.Empty
		//            );
		//    }
		//}

		//-----------------------------------------------------------------------------
		private bool IsValidModuleName()
		{
			return BaseCustomizationContext.CustomizationContextInstance.IsValidName(ModuleName) && ModuleName.Length <= 20;
		}

		//-----------------------------------------------------------------------------
		private bool IsValidApplicationName()
		{
			return BaseCustomizationContext.CustomizationContextInstance.IsValidName(ApplicationName) && ApplicationName.Length <= 20;
		}
	
		//-----------------------------------------------------------------------------
		private void GenerateNewModuleName()
		{
			int i = 0;
			string newModName = string.Empty;
			do
			{
				i++;
				newModName = string.Format(defaultNewModule, i);
			} while (ExistsModule(newModName));

			txtNewApplicationName.Text = initialApplication;
			txtNewCustomizationName.Text = newModName;
		}

		//-----------------------------------------------------------------------------
		private bool ExistsModule(string newModName)
		{
			return listModuleNames.Contains(newModName);
		}

		//-----------------------------------------------------------------------------
		private static bool ExistsApplication(string newAppName)
		{
			IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(newAppName);
			return appInfo != null;
		}

		//-----------------------------------------------------------------------------
		private string GenerateNewApplicationName()
		{
			int i = 0;
			string newAppName = string.Empty;
			do
			{
				i++;
				newAppName = string.Format(defaultNewApp,i);
			
			} while (ExistsApplication(newAppName));

			return newAppName;
		}
		/// <summary>
		/// Ritorna true se l'applicazione esiste già o se non è customizzabile
		/// usata per sapere se ad esempio sto aggiungo un'applicazione che si chiama ERP
		/// </summary>
		/// <param name="appName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		private static bool ApplicationExistOrIsNotCustomizable(string appName)
		{
			IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(appName);
			if (appInfo == null)
				return false;

			//Se esiste l'applicazione ma non è customizzabile allora non posso comunque aggiungere moduli
			if (
				appInfo.ApplicationType != ApplicationType.Customization &&
				appInfo.ApplicationType != ApplicationType.Standardization 
				)
				return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		private static bool ExistApplication(string newAppName)
		{
			return  BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(newAppName) != null;
		}

		//-----------------------------------------------------------------------------
		private void txtNewCustomizationName_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)Keys.Space)
				return;

			e.Handled = true;
		}

		//-----------------------------------------------------------------------------
		private void CkbCreateApplicationInStandardFolder_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CkbCreateApplicationInStandardFolder.Checked)
			{
				applicationNameMemento = txtNewApplicationName.Text;
			}
			else
			{
				txtNewApplicationName.Text = applicationNameMemento;
			}

			cbxRegisteredSolutions.Visible = CkbCreateApplicationInStandardFolder.Checked;
			txtNewApplicationName.Visible = !CkbCreateApplicationInStandardFolder.Checked;

			if (!CkbCreateApplicationInStandardFolder.Checked)
				return;

			if (!AskAndTestCredentialsIfNeeded())
				return;

			Task.Factory.StartNew(
				new Action(
					()
					=>
					{
						IntegratedSolution[] registeredSolutions = null;
						string companyCode = Settings.Default.CompanyCode;

						CrypterRef.Crypter30 registeredSolutionsRetriever = new CrypterRef.Crypter30();

						bool ok = registeredSolutionsRetriever.Login(
							Crypto.Decrypt(Settings.Default.Password),
							Settings.Default.Username,
							out registeredSolutions,
							out companyCode
							);

						if (!ok)
							throw new Exception("Failed to login to Microarea web site to retrieve solutions list.");

						if (String.Compare(companyCode, Settings.Default.CompanyCode) != 0)
						{
							Settings.Default.CompanyCode = companyCode;
							BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
						}

						try
						{
							this.BeginInvoke(
							new Action<IntegratedSolution[]>(PopulateSolutionsComboBox),
							registeredSolutions as object
							);
						}
						catch /*(Exception exc)*/
						{
							/*string s = exc.ToString();*/
						}
					}
					)
				);
		}

		//-----------------------------------------------------------------------------
		private bool AskAndTestCredentialsIfNeeded()
		{
			//Se non sono ancora stato fornite le credenziali di accesso al sito
			//microarea allora le chiedo adesso.
			if (
				String.IsNullOrWhiteSpace(Settings.Default.Username) ||
				String.IsNullOrWhiteSpace(Settings.Default.Password)
				)
			{
				Credentials cred = new Credentials();
				DialogResult res = cred.ShowDialog(this);

				if (res != System.Windows.Forms.DialogResult.OK)
					return false;

				Settings.Default.Username = cred.Username;
				Settings.Default.Password = Crypto.Encrypt(cred.Password);

				BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
			}

			bool okCredentials = false;
			try
			{
				okCredentials = CrypterWrapper.TestCredentials(Settings.Default.Username, Crypto.Decrypt(Settings.Default.Password));
			}
			catch (Exception exc)
			{
				MessageBox.Show(String.Format(Resources.ErrorCheckingCredentials, exc.ToString()));
				return false;
			}

			if (!okCredentials)
			{
				MessageBox.Show(Resources.WrongCredentialsOrPasswordExpired);

				Credentials cred = new Credentials();
				DialogResult res = cred.ShowDialog(this);

				if (res != System.Windows.Forms.DialogResult.OK)
					return false;
			}

			return true;
		}

		//-----------------------------------------------------------------------------
		private void PopulateSolutionsComboBox(
			IntegratedSolution[] registeredSolutions
			)
		{
			cbxRegisteredSolutions.Items.Clear();

			if (registeredSolutions == null || registeredSolutions.Length == 0)
				return;

			ArrayList tempForSort = new ArrayList(registeredSolutions);
			tempForSort.Sort();

			foreach (IntegratedSolution solution in tempForSort)
			{
				if (!String.IsNullOrWhiteSpace(solution.SolutionName) && !IsAlreadyUsed(solution.SolutionName))
					cbxRegisteredSolutions.Items.Add(solution);
			}

			cbxRegisteredSolutions.SelectedItem = cbxRegisteredSolutions.Items[0];
		}

		//Ritorna true se l'applicazione e' gia' presente nel contesto di customizzazione
		//-----------------------------------------------------------------------------
		private static bool IsAlreadyUsed(string applicationName)
		{
			IList<IEasyBuilderApp> listEBapps = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(applicationName);
			return listEBapps != null && listEBapps.Count > 0;
		}

		//-----------------------------------------------------------------------------
		//Tutte le volte che viene selezionato un elemento della combo allora allinea
		//il testo di txtNewApplicationName poichè è il valore ritornato dalla proprietà
		//ApplicationName utilizzata da chi esegue questa form per capire l'input dell'utente
		private void CbxRegisteredSolutions_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtNewApplicationName.Text = cbxRegisteredSolutions.SelectedItem.ToString();
		}

		//-----------------------------------------------------------------------------
		//Tutte le volte che cambia il testo della combo allora allinea
		//il testo di txtNewApplicationName poichè è il valore ritornato dalla proprietà
		//ApplicationName utilizzata da chi esegue questa form per capire l'input dell'utente
		private void CbxRegisteredSolutions_TextUpdate(object sender, EventArgs e)
		{
			txtNewApplicationName.Text = cbxRegisteredSolutions.Text;
		}
	}
}
