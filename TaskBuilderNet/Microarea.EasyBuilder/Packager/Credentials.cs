using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.Packager
{
	//====================================================================================
	internal partial class Credentials : ThemedForm
	{
		public string Username { get { return TxtUsername.Text; } }
		public string Password { get { return TxtPassword.Text; } }

		public bool HideButtons { get; set; }

		public string Header
		{
			get { return LblInformation.Text; }
			set { LblInformation.Text = value; }
		}

		//--------------------------------------------------------------------------------
		public Credentials()
		{
			InitializeComponent();

			string username,password = null;
			if (!String.IsNullOrWhiteSpace(username = Settings.Default.Username))
				TxtUsername.Text = username;
			else
				TxtUsername.Text = String.Empty;

			if (!String.IsNullOrWhiteSpace(password = Settings.Default.Password))
				TxtPassword.Text = Crypto.Decrypt(password);
			else
				TxtPassword.Text = String.Empty;
		}

		//--------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (HideButtons)
			{
				btnOk.Visible = false;
				btnCancel.Visible = false;
			}

			EnableBtnTestCredentials(this, EventArgs.Empty);
		}



		//--------------------------------------------------------------------------------
		private void BtnTestCredentials_Click(object sender, EventArgs e)
		{
			if (CrypterWrapper.TestCredentials(TxtUsername.Text, TxtPassword.Text))
			{
				LblCredentialsTestOutput.ForeColor = Color.ForestGreen;
				LblCredentialsTestOutput.Text = Resources.RightCredentials;
			}
			else
			{
				LblCredentialsTestOutput.ForeColor = Color.Red;
				LblCredentialsTestOutput.Text = Resources.WrongCredentials;
			}
		}

		//--------------------------------------------------------------------------------
		private void EnableBtnTestCredentials(object sender, EventArgs e)
		{
			BtnTestCredentials.Enabled =
				!String.IsNullOrWhiteSpace(TxtUsername.Text) && !String.IsNullOrWhiteSpace(TxtPassword.Text);
		}

		//--------------------------------------------------------------------------------
		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (!CrypterWrapper.TestCredentials(TxtUsername.Text, TxtPassword.Text))
			{
				MessageBox.Show(Resources.WrongCredentials);
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
		}
	}
}
