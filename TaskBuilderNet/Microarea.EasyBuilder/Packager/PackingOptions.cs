using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.Packager
{
	//================================================================================
	internal partial class PackingOptions : ThemedForm
	{
		private const string DefaultExcludeExtensions = "*.cs; *.vb; *.pdb; *.cfg;";

		Credentials cred;
		private bool canDesign;
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public PackingOptions(bool canDesign)
		{
			this.canDesign = canDesign;

			InitializeComponent();
			InitializeOptionsWindow();

			if (canDesign && BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable())
			{
				cred = new Credentials();
				cred.TopLevel = false;
				cred.Dock = DockStyle.Fill;
				cred.FormBorderStyle = FormBorderStyle.None;
				cred.BackColor = Color.White;
				cred.HideButtons = true;
				TabPage tabPageAccount = new TabPage(Resources.Account);
				tabPageAccount.Controls.Add(cred);

				tabControl.TabPages.Add(tabPageAccount);
			}
		}

		//-----------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (canDesign && BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable())
			{
				cred.Show();
			}
		}

		//-----------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (DialogResult == System.Windows.Forms.DialogResult.OK && canDesign && cred != null)
			{
				if (
					cred.Username != Settings.Default.Username ||
					cred.Password != Crypto.Decrypt(Settings.Default.Password)
					)
				{
					if (!CrypterWrapper.TestCredentials(cred.Username, cred.Password))
					{
						MessageBox.Show(Resources.WrongCredentials);
						e.Cancel = true;
						return;
					}
					Settings.Default.Username = cred.Username;
					Settings.Default.Password = Crypto.Encrypt(cred.Password);

					BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void InitializeOptionsWindow()
		{
			chkExcludeSources.Checked = Settings.Default.ExcludeSources;	
			chkExportPublishedOnly.Checked = Settings.Default.ExportPublishedOnly;
		}

		//-----------------------------------------------------------------------------
		private void PackingOptions_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != System.Windows.Forms.DialogResult.OK)
				return;

			//Se ci sono stati dei cambiamenti nelle estensioni li salvo se sono validi
			CheckExcludeExtensions(e);
		}

		//-----------------------------------------------------------------------------
		private void CheckExcludeExtensions(FormClosingEventArgs e)
		{
			if (Settings.Default.ExportPublishedOnly == chkExportPublishedOnly.Checked &&
                Settings.Default.ExcludeSources== chkExcludeSources.Checked)
				return;

			Settings.Default.ExportPublishedOnly = chkExportPublishedOnly.Checked;
			Settings.Default.ExcludeSources = chkExcludeSources.Checked;
			BaseCustomizationContext.CustomizationContextInstance.SaveSettings();	
		}
	}
}
