using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebSockets;
using Microarea.TaskBuilderNet.Interfaces;
using WaitingWnd = Microarea.TaskBuilderNet.UI.WinControls.WaitingWindow;
using System.Drawing;

namespace Microarea.EasyBuilder.Packager
{
	///<summary>
	///Internal use
	///</summary>
	//================================================================================
	public partial class ChooseCustomizationContext : ThemedForm
	{
		//string application;
		//string module;

		///<summary>
		///Internal use
		///</summary>
		//--------------------------------------------------------------------------------
		public ChooseCustomizationContext()
		{
			InitializeComponent();

            // DPI screen scaling
            float dpiScale = 1;
            Graphics g = this.CreateGraphics();
            try
            {
                dpiScale = g.DpiY / 96;
            }
            finally
            {
                g.Dispose();
            }
            int w = (int)(this.Size.Width * dpiScale);
            int h = (int)(this.Size.Height /* dpiScale*/);
            this.Size = new System.Drawing.Size(w, h);
		}

		///<summary>
		///Internal use
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool Choose()
		{
			ChooseCustomizationContext form = new ChooseCustomizationContext();
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				form.ShowDialog(); //è diventata solo un messaggio di errore non apertura contesto
				return false;

				/*DialogResult result = form.ShowDialog();
				if (result != System.Windows.Forms.DialogResult.OK)
					return false;

				BaseCustomizationContext.CustomizationContextInstance.CurrentApplication = form.application;
				BaseCustomizationContext.CustomizationContextInstance.CurrentModule = form.module;
				BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(form.application, form.module);
				return true;*/
			}
		}
 /*       //--------------------------------------------------------------------------------
        private void ChooseCustomizationContext_Load(object sender, EventArgs e)
        {
            TopMost = true;
            int selectedIndex = -1;
            string activeAppName = Settings.Default.LastApplication;

            IBaseApplicationInfo currentTbApp = null;
            var items = cbApplications.Items;
            foreach (var ebApp in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
            {
                if (ebApp == null || ebApp.ModuleInfo == null)
                    continue;
                currentTbApp = ebApp.ModuleInfo.ParentApplicationInfo;
                if (!items.Contains(currentTbApp))
                {
                    items.Add(currentTbApp);

                    if (ebApp.ApplicationName == activeAppName)
                        selectedIndex = items.Count - 1;
                }
            }
            cbApplications.SelectedIndex = selectedIndex > -1 ? selectedIndex : items.Count - 1;
            TopMost = false;
        }

		//--------------------------------------------------------------------------------
		private void cbApplications_SelectedIndexChanged(object sender, EventArgs e)
		{
			int idx = -1, selectedIndex = -1;
			string modName = Settings.Default.LastModule;
			cbModules.Items.Clear();

			BaseApplicationInfo bai = cbApplications.SelectedItem as BaseApplicationInfo;
			if (bai == null)
				return;

			IList<IEasyBuilderApp> easyBuilderApps =  BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderApps(((BaseApplicationInfo)cbApplications.SelectedItem).Name, bai.ApplicationType);
			foreach (IEasyBuilderApp ebApp in easyBuilderApps)
			{
				idx++;
				IBaseModuleInfo bmi = bai.GetModuleInfoByName(ebApp.ModuleName);
				cbModules.Items.Add(bmi);
				if (bmi.Name == modName)
					selectedIndex = idx;
			}
			cbModules.SelectedIndex =  selectedIndex > -1 ? selectedIndex : cbModules.Items.Count - 1;
			//abilitiamo il bottone ok solo se sono stati selezionati un'applicazione e un modulo
			btnOk.Enabled = cbModules.SelectedIndex > -1 && cbApplications.SelectedIndex > -1;
		}

		//--------------------------------------------------------------------------------
		private void btnAdd_Click(object sender, EventArgs e)
		{
			AddRenameCustomization ac = new AddRenameCustomization(CustomizationStatus.AddApplication);

			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				DialogResult result = ac.ShowDialog(this);
				if (result != System.Windows.Forms.DialogResult.OK)
					return;
			}

			if (ac.CreateApplicationInStandardFolder && BaseCustomizationContext.CustomizationContextInstance.NotAlone(Resources.AddApplicationCaption, 1, 1, this))
				return;

			WaitingWnd waitWnd = new WaitingWnd(String.Format(Resources.WaitingMessageCreation, ac.ApplicationName), 40);
			waitWnd.Show();
			bool appCreated = false;
			Functions.DoParallelProcedure(new Action(
				() =>
				{
					ApplicationType appType = ApplicationType.Customization;
					if (ac.CreateApplicationInStandardFolder)
						appType = ApplicationType.Standardization;

					if (appCreated = BaseCustomizationContext.CustomizationContextInstance.AddNewEasyBuilderApp(ac.ApplicationName, ac.ModuleName, appType))
					{
						//Memorizzo applicazione e modulo perch`e verranno poi salvati nel file di Settings.
						application = ac.ApplicationName;
						module = ac.ModuleName;
					}
				}));
			waitWnd.Close();
			waitWnd.Dispose();

			if (appCreated)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (DialogResult != DialogResult.OK)
				return;

			if (string.IsNullOrEmpty(application) || string.IsNullOrEmpty(module))
			{
				application = cbApplications.SelectedItem == null
					? ""
					: cbApplications.SelectedItem.ToString();
				module = cbModules.SelectedItem == null
					? ""
					: cbModules.SelectedItem.ToString();
			}
			if (string.IsNullOrEmpty(application) || string.IsNullOrEmpty(module))
			{
				e.Cancel = true;
				return;
			}

			//se ho scelto una applicazione e modulo, avviso il customizationContext del menu
			string content = string.Concat(application, ";", module);
			ServerWebSocketConnector.PushToClients("", "CustomizationContextUpdated", content);
		}

		//-----------------------------------------------------------------------------
		private void cbModules_SelectedIndexChanged(object sender,EventArgs e)
		{
			//abilitiamo il bottone ok solo se sono stati selezionati un'applicazione e un modulo
			btnOk.Enabled = cbModules.SelectedIndex > -1 && cbApplications.SelectedIndex > -1;
		}*/

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
