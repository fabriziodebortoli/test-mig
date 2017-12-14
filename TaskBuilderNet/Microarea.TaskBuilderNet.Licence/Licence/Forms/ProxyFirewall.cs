using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	/// <summary>
	/// Summary description for ProxyFirewall.
	/// </summary>
	//============================================================================
	public class ProxyFirewall : System.Windows.Forms.Form
	{
		private TextBox txtProxyHttp;
		private TextBox txtPortHttp;
		private Label label1;
		private Label label2;
		private Label label4;
		private Label lblHttp;
		private CheckBox chkUseCredentials;
		private TextBox txtDomain;
		private TextBox txtUser;
		private TextBox txtPassword;
		private Label lblDomain;
		private Label lblUser;
		private Label lblPassword;
		private Label lblFtp;
		private TextBox txtPortFtp;
		private TextBox txtProxyFtp;
		private GroupBox grpAddresses;
		private GroupBox grpCredentials;
		private Panel pnlCredentials;
		private ErrorProvider errorProviderPort;		
		private Button BtnCancel;
		private Button BtnOK;

		private IContainer components;

		//---------------------------------------------------------------------
		private	ProxySettings	state;
		private PictureBox pictureBox1;
		private bool			dirty = false;

		//---------------------------------------------------------------------
		public ProxyFirewall(ProxySettings state)
		{
			InitializeComponent();
			this.state = state;
			if (this.state == null) this.state = new ProxySettings();
			SetVisualState();
			BtnOK.Enabled=false;

			// hide ftp stuff until ftp proxying is not implemented. TODO: remove when ready
			lblFtp.Visible = false;
			txtProxyFtp.Visible = false;
			txtPortFtp.Visible = false;
		}

		//---------------------------------------------------------------------
		public void SetVisualState()
		{
			txtProxyHttp.Text	= state.HttpProxy.Server;
			if ((state.HttpProxy.Server == null || state.HttpProxy.Server.Length == 0) && state.HttpProxy.Port == 0)
				txtPortHttp.Text = string.Empty;
			else
				txtPortHttp.Text	= state.HttpProxy.Port.ToString();

			txtProxyFtp.Text	= state.FtpProxy.Server;
			if ((state.FtpProxy.Server == null || state.FtpProxy.Server.Length == 0) && state.FtpProxy.Port == 0)
				txtPortFtp.Text = string.Empty;
			else
				txtPortFtp.Text	= state.FtpProxy.Port.ToString();

			chkUseCredentials.Checked = state.FirewallCredentialsSettings.NeedsCredentials;
			pnlCredentials.Enabled = state.FirewallCredentialsSettings.NeedsCredentials;
			txtDomain.Text		= state.FirewallCredentialsSettings.Domain;
			txtUser.Text = state.FirewallCredentialsSettings.Name;
			txtPassword.Text = 
				string.IsNullOrEmpty(state.FirewallCredentialsSettings.Password)?
				string.Empty:
				Storer.Unstore(state.FirewallCredentialsSettings.Password);
		}

		//---------------------------------------------------------------------
		public ProxySettings GetVisualState()
		{
			ProxySettings newState = new ProxySettings();

			newState.FirewallCredentialsSettings.NeedsCredentials = chkUseCredentials.Checked;
			newState.FirewallCredentialsSettings.Domain	= txtDomain.Text;
			newState.FirewallCredentialsSettings.Name		= txtUser.Text;
			newState.FirewallCredentialsSettings.Password	= Storer.Store(txtPassword.Text);
			
			string http = String.Format(CultureInfo.InvariantCulture, "{0}{1}", Uri.UriSchemeHttp, Uri.SchemeDelimiter);
			string httpText = txtProxyHttp.Text.Trim();
			if (httpText.ToLower(CultureInfo.InvariantCulture).StartsWith(http.ToLower(CultureInfo.InvariantCulture)) || httpText.Length == 0)
				newState.HttpProxy.Server	= httpText;
			else
				newState.HttpProxy.Server = httpText.Insert(0, http);
			
			newState.HttpProxy.Port		= txtPortHttp.Text.Length == 0 ? 0 : Int32.Parse(txtPortHttp.Text);

			newState.FtpProxy.Server	= txtProxyFtp.Text;
			newState.FtpProxy.Port		= txtPortFtp.Text.Length == 0 ? 0 : Int32.Parse(txtPortFtp.Text);

			return newState;
		}

		//---------------------------------------------------------------------
		public bool IsFormValid()
		{
			if (txtProxyHttp.Text.Length == 0 && txtPortHttp.Text.Length == 0)
			{
				return true;
			}

			if (txtProxyHttp.Text.Length != 0 && txtPortHttp.Text.Length == 0)
			{
				txtPortHttp.Select(0, txtPortHttp.Text.Length);
				// Set the ErrorProvider error with the text to display.
				this.errorProviderPort.SetError(txtPortHttp, LicenceStrings.MissingPortNumber);
				return false;
			}

			string http = String.Format(CultureInfo.InvariantCulture, "{0}{1}", Uri.UriSchemeHttp, Uri.SchemeDelimiter);
			if (txtProxyHttp.Text.Length != 0 && !txtProxyHttp.Text.ToLower(CultureInfo.InvariantCulture).StartsWith(http.ToLower(CultureInfo.InvariantCulture)) &&
				(
				txtProxyHttp.Text.IndexOf(Uri.SchemeDelimiter) != -1 ||
				txtProxyHttp.Text.IndexOf(":") != -1 ||
				txtProxyHttp.Text.IndexOf("//") != -1 )
				)
			{
				txtPortHttp.Select(0, txtPortHttp.Text.Length);
				// Set the ErrorProvider error with the text to display.
				this.errorProviderPort.SetError(txtProxyHttp, LicenceStrings.InvalidProxyAddress);
				return false;
			}
			
				string s = txtProxyHttp.Text;
				s = s.ToLower(CultureInfo.InvariantCulture).StartsWith(http.ToLower(CultureInfo.InvariantCulture)) ? s : s.Insert(0, http);
				try
				{
					Uri u = new Uri(s);
				}

				catch 
				{
					txtPortHttp.Select(0, txtPortHttp.Text.Length);
					// Set the ErrorProvider error with the text to display.
					this.errorProviderPort.SetError(txtProxyHttp, LicenceStrings.InvalidProxyAddress);
					return false;
				}
			

			if (txtProxyFtp.Text.Length != 0 && txtPortFtp.Text.Length == 0)
			{
				txtPortHttp.Select(0, txtPortFtp.Text.Length);
				// Set the ErrorProvider error with the text to display.
				this.errorProviderPort.SetError(txtPortFtp, LicenceStrings.MissingPortNumber);
				return false;
			}

			// TODO - aggiungere altre validazioni

			return true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProxyFirewall));
            this.txtProxyHttp = new System.Windows.Forms.TextBox();
            this.txtPortHttp = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblHttp = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkUseCredentials = new System.Windows.Forms.CheckBox();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.pnlCredentials = new System.Windows.Forms.Panel();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblDomain = new System.Windows.Forms.Label();
            this.lblFtp = new System.Windows.Forms.Label();
            this.txtPortFtp = new System.Windows.Forms.TextBox();
            this.txtProxyFtp = new System.Windows.Forms.TextBox();
            this.grpAddresses = new System.Windows.Forms.GroupBox();
            this.grpCredentials = new System.Windows.Forms.GroupBox();
            this.errorProviderPort = new System.Windows.Forms.ErrorProvider(this.components);
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlCredentials.SuspendLayout();
            this.grpAddresses.SuspendLayout();
            this.grpCredentials.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtProxyHttp
            // 
            resources.ApplyResources(this.txtProxyHttp, "txtProxyHttp");
            this.txtProxyHttp.Name = "txtProxyHttp";
            this.txtProxyHttp.TextChanged += new System.EventHandler(this.SetDirty);
            // 
            // txtPortHttp
            // 
            resources.ApplyResources(this.txtPortHttp, "txtPortHttp");
            this.txtPortHttp.Name = "txtPortHttp";
            this.txtPortHttp.TextChanged += new System.EventHandler(this.SetDirty);
            this.txtPortHttp.Validating += new System.ComponentModel.CancelEventHandler(this.Port_Validating);
            this.txtPortHttp.Validated += new System.EventHandler(this.Port_Validated);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // lblHttp
            // 
            resources.ApplyResources(this.lblHttp, "lblHttp");
            this.lblHttp.Name = "lblHttp";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // chkUseCredentials
            // 
            resources.ApplyResources(this.chkUseCredentials, "chkUseCredentials");
            this.chkUseCredentials.Name = "chkUseCredentials";
            this.chkUseCredentials.CheckedChanged += new System.EventHandler(this.SetDirty);
            this.chkUseCredentials.Click += new System.EventHandler(this.chkUseCredentials_Click);
            // 
            // txtDomain
            // 
            resources.ApplyResources(this.txtDomain, "txtDomain");
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.TextChanged += new System.EventHandler(this.SetDirty);
            // 
            // txtUser
            // 
            resources.ApplyResources(this.txtUser, "txtUser");
            this.txtUser.Name = "txtUser";
            this.txtUser.TextChanged += new System.EventHandler(this.SetDirty);
            // 
            // txtPassword
            // 
            resources.ApplyResources(this.txtPassword, "txtPassword");
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.TextChanged += new System.EventHandler(this.SetDirty);
            // 
            // pnlCredentials
            // 
            this.pnlCredentials.Controls.Add(this.lblPassword);
            this.pnlCredentials.Controls.Add(this.lblUser);
            this.pnlCredentials.Controls.Add(this.lblDomain);
            this.pnlCredentials.Controls.Add(this.txtUser);
            this.pnlCredentials.Controls.Add(this.txtDomain);
            this.pnlCredentials.Controls.Add(this.txtPassword);
            resources.ApplyResources(this.pnlCredentials, "pnlCredentials");
            this.pnlCredentials.Name = "pnlCredentials";
            // 
            // lblPassword
            // 
            resources.ApplyResources(this.lblPassword, "lblPassword");
            this.lblPassword.Name = "lblPassword";
            // 
            // lblUser
            // 
            resources.ApplyResources(this.lblUser, "lblUser");
            this.lblUser.Name = "lblUser";
            // 
            // lblDomain
            // 
            resources.ApplyResources(this.lblDomain, "lblDomain");
            this.lblDomain.Name = "lblDomain";
            // 
            // lblFtp
            // 
            resources.ApplyResources(this.lblFtp, "lblFtp");
            this.lblFtp.Name = "lblFtp";
            // 
            // txtPortFtp
            // 
            resources.ApplyResources(this.txtPortFtp, "txtPortFtp");
            this.txtPortFtp.Name = "txtPortFtp";
            this.txtPortFtp.TextChanged += new System.EventHandler(this.SetDirty);
            this.txtPortFtp.Validating += new System.ComponentModel.CancelEventHandler(this.Port_Validating);
            this.txtPortFtp.Validated += new System.EventHandler(this.Port_Validated);
            // 
            // txtProxyFtp
            // 
            resources.ApplyResources(this.txtProxyFtp, "txtProxyFtp");
            this.txtProxyFtp.Name = "txtProxyFtp";
            this.txtProxyFtp.TextChanged += new System.EventHandler(this.SetDirty);
            // 
            // grpAddresses
            // 
            resources.ApplyResources(this.grpAddresses, "grpAddresses");
            this.grpAddresses.Controls.Add(this.txtProxyFtp);
            this.grpAddresses.Controls.Add(this.txtPortFtp);
            this.grpAddresses.Controls.Add(this.lblFtp);
            this.grpAddresses.Controls.Add(this.txtPortHttp);
            this.grpAddresses.Controls.Add(this.txtProxyHttp);
            this.grpAddresses.Controls.Add(this.label1);
            this.grpAddresses.Controls.Add(this.lblHttp);
            this.grpAddresses.Controls.Add(this.label2);
            this.grpAddresses.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpAddresses.Name = "grpAddresses";
            this.grpAddresses.TabStop = false;
            // 
            // grpCredentials
            // 
            resources.ApplyResources(this.grpCredentials, "grpCredentials");
            this.grpCredentials.Controls.Add(this.chkUseCredentials);
            this.grpCredentials.Controls.Add(this.pnlCredentials);
            this.grpCredentials.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpCredentials.Name = "grpCredentials";
            this.grpCredentials.TabStop = false;
            // 
            // errorProviderPort
            // 
            this.errorProviderPort.BlinkRate = 300;
            this.errorProviderPort.ContainerControl = this;
            resources.ApplyResources(this.errorProviderPort, "errorProviderPort");
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // ProxyFirewall
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.CancelButton = this.BtnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.grpCredentials);
            this.Controls.Add(this.grpAddresses);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ProxyFirewall";
            this.ShowInTaskbar = false;
            this.pnlCredentials.ResumeLayout(false);
            this.pnlCredentials.PerformLayout();
            this.grpAddresses.ResumeLayout(false);
            this.grpAddresses.PerformLayout();
            this.grpCredentials.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void chkUseCredentials_Click(object sender, System.EventArgs e)
		{
			pnlCredentials.Enabled = ((CheckBox)sender).Checked;
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			if (!IsFormValid())
				return;

			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private bool IsPortValid(string txtPort)
		{
			if (txtPort.Length == 0)	// permetto di non specificarlo
				return true;
			int port = 0;
			try		{ port = Int32.Parse(txtPort);	}
			catch	{ return false;	}
			return port >= 0;
		}

		//---------------------------------------------------------------------
		private void Port_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			TextBox txtBox = (TextBox)sender;
			if (!IsPortValid(txtBox.Text))
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				txtBox.Select(0, txtBox.Text.Length);

				// Set the ErrorProvider error with the text to display.
				this.errorProviderPort.SetError(txtBox, LicenceStrings.InvalidPortNumber);
			}
		}

		//---------------------------------------------------------------------
		private void Port_Validated(object sender, System.EventArgs e)
		{
			// If all conditions have been met, clear the ErrorProvider of errors.
			errorProviderPort.SetError((TextBox)sender, string.Empty);
		}

		//---------------------------------------------------------------------
		private void SetDirty(object sender, System.EventArgs e)
		{
			Dirty = true;
		}

		//---------------------------------------------------------------------
		public bool Dirty
		{
			get { return this.dirty; }
			set
			{
				this.dirty = value;
				BtnOK.Enabled = value;
			}
		}

		//---------------------------------------------------------------------
	}

	//============================================================================
	public static class ProxyFirewallManager
	{
		//---------------------------------------------------------------------
		public static ProxySettings Show(ProxySettings ps)
		{
			ProxyFirewall proxyForm = new ProxyFirewall(ps);

			if (proxyForm.ShowDialog() != DialogResult.OK)
				return null;

			ProxySettings newState = proxyForm.GetVisualState();
			return newState;

		}
	}

}
