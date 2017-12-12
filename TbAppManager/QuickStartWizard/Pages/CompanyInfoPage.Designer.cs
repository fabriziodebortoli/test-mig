namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	partial class CompanyInfoPage
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompanyInfoPage));
			this.LblServer = new System.Windows.Forms.Label();
			this.ComboSQLServers = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo(this.components);
			this.TxtPassword = new System.Windows.Forms.TextBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.TxtLogin = new System.Windows.Forms.TextBox();
			this.LblLogin = new System.Windows.Forms.Label();
			this.GBoxConnectionInfo = new System.Windows.Forms.GroupBox();
			this.GBoxDataToLoad = new System.Windows.Forms.GroupBox();
			this.RadioSampleData = new System.Windows.Forms.RadioButton();
			this.RadioDefaultData = new System.Windows.Forms.RadioButton();
			this.ChBoxShowOptions = new System.Windows.Forms.CheckBox();
			this.GBoxOptions = new System.Windows.Forms.GroupBox();
			this.TxtCompany = new System.Windows.Forms.TextBox();
			this.TxtCompanyDB = new System.Windows.Forms.TextBox();
			this.TxtSystemDB = new System.Windows.Forms.TextBox();
			this.LblSystemDB = new System.Windows.Forms.Label();
			this.LblCompany = new System.Windows.Forms.Label();
			this.LblCompanyDB = new System.Windows.Forms.Label();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.GBoxConnectionInfo.SuspendLayout();
			this.GBoxDataToLoad.SuspendLayout();
			this.GBoxOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
			// 
			// m_headerPicture
			// 
			resources.ApplyResources(this.m_headerPicture, "m_headerPicture");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// LblServer
			// 
			resources.ApplyResources(this.LblServer, "LblServer");
			this.LblServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblServer.Name = "LblServer";
			// 
			// ComboSQLServers
			// 
			this.ComboSQLServers.FormattingEnabled = true;
			resources.ApplyResources(this.ComboSQLServers, "ComboSQLServers");
			this.ComboSQLServers.Name = "ComboSQLServers";
			this.ComboSQLServers.SelectedSQLServer = "";
			// 
			// TxtPassword
			// 
			resources.ApplyResources(this.TxtPassword, "TxtPassword");
			this.TxtPassword.Name = "TxtPassword";
			// 
			// LblPassword
			// 
			resources.ApplyResources(this.LblPassword, "LblPassword");
			this.LblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblPassword.Name = "LblPassword";
			// 
			// TxtLogin
			// 
			resources.ApplyResources(this.TxtLogin, "TxtLogin");
			this.TxtLogin.Name = "TxtLogin";
			// 
			// LblLogin
			// 
			resources.ApplyResources(this.LblLogin, "LblLogin");
			this.LblLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblLogin.Name = "LblLogin";
			// 
			// GBoxConnectionInfo
			// 
			this.GBoxConnectionInfo.Controls.Add(this.ComboSQLServers);
			this.GBoxConnectionInfo.Controls.Add(this.LblServer);
			this.GBoxConnectionInfo.Controls.Add(this.TxtLogin);
			this.GBoxConnectionInfo.Controls.Add(this.TxtPassword);
			this.GBoxConnectionInfo.Controls.Add(this.LblPassword);
			this.GBoxConnectionInfo.Controls.Add(this.LblLogin);
			this.GBoxConnectionInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GBoxConnectionInfo, "GBoxConnectionInfo");
			this.GBoxConnectionInfo.Name = "GBoxConnectionInfo";
			this.GBoxConnectionInfo.TabStop = false;
			// 
			// GBoxDataToLoad
			// 
			this.GBoxDataToLoad.Controls.Add(this.RadioSampleData);
			this.GBoxDataToLoad.Controls.Add(this.RadioDefaultData);
			this.GBoxDataToLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GBoxDataToLoad, "GBoxDataToLoad");
			this.GBoxDataToLoad.Name = "GBoxDataToLoad";
			this.GBoxDataToLoad.TabStop = false;
			// 
			// RadioSampleData
			// 
			resources.ApplyResources(this.RadioSampleData, "RadioSampleData");
			this.RadioSampleData.Name = "RadioSampleData";
			this.RadioSampleData.UseVisualStyleBackColor = true;
			// 
			// RadioDefaultData
			// 
			resources.ApplyResources(this.RadioDefaultData, "RadioDefaultData");
			this.RadioDefaultData.Checked = true;
			this.RadioDefaultData.Name = "RadioDefaultData";
			this.RadioDefaultData.TabStop = true;
			this.RadioDefaultData.UseVisualStyleBackColor = true;
			// 
			// ChBoxShowOptions
			// 
			resources.ApplyResources(this.ChBoxShowOptions, "ChBoxShowOptions");
			this.ChBoxShowOptions.Name = "ChBoxShowOptions";
			this.ChBoxShowOptions.UseVisualStyleBackColor = true;
			this.ChBoxShowOptions.CheckedChanged += new System.EventHandler(this.ChBoxShowOptions_CheckedChanged);
			// 
			// GBoxOptions
			// 
			this.GBoxOptions.Controls.Add(this.TxtCompany);
			this.GBoxOptions.Controls.Add(this.TxtCompanyDB);
			this.GBoxOptions.Controls.Add(this.TxtSystemDB);
			this.GBoxOptions.Controls.Add(this.LblSystemDB);
			this.GBoxOptions.Controls.Add(this.LblCompany);
			this.GBoxOptions.Controls.Add(this.LblCompanyDB);
			this.GBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GBoxOptions, "GBoxOptions");
			this.GBoxOptions.Name = "GBoxOptions";
			this.GBoxOptions.TabStop = false;
			// 
			// TxtCompany
			// 
			resources.ApplyResources(this.TxtCompany, "TxtCompany");
			this.TxtCompany.Name = "TxtCompany";
			// 
			// TxtCompanyDB
			// 
			resources.ApplyResources(this.TxtCompanyDB, "TxtCompanyDB");
			this.TxtCompanyDB.Name = "TxtCompanyDB";
			// 
			// TxtSystemDB
			// 
			resources.ApplyResources(this.TxtSystemDB, "TxtSystemDB");
			this.TxtSystemDB.Name = "TxtSystemDB";
			// 
			// LblSystemDB
			// 
			resources.ApplyResources(this.LblSystemDB, "LblSystemDB");
			this.LblSystemDB.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblSystemDB.Name = "LblSystemDB";
			// 
			// LblCompany
			// 
			resources.ApplyResources(this.LblCompany, "LblCompany");
			this.LblCompany.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblCompany.Name = "LblCompany";
			// 
			// LblCompanyDB
			// 
			resources.ApplyResources(this.LblCompanyDB, "LblCompanyDB");
			this.LblCompanyDB.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblCompanyDB.Name = "LblCompanyDB";
			// 
			// CompanyInfoPage
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.ChBoxShowOptions);
			this.Controls.Add(this.GBoxOptions);
			this.Controls.Add(this.GBoxDataToLoad);
			this.Controls.Add(this.GBoxConnectionInfo);
			this.Name = "CompanyInfoPage";
			this.Load += new System.EventHandler(this.CompanyInfoPage_Load);
			this.Controls.SetChildIndex(this.GBoxConnectionInfo, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.Controls.SetChildIndex(this.GBoxDataToLoad, 0);
			this.Controls.SetChildIndex(this.GBoxOptions, 0);
			this.Controls.SetChildIndex(this.ChBoxShowOptions, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.GBoxConnectionInfo.ResumeLayout(false);
			this.GBoxConnectionInfo.PerformLayout();
			this.GBoxDataToLoad.ResumeLayout(false);
			this.GBoxDataToLoad.PerformLayout();
			this.GBoxOptions.ResumeLayout(false);
			this.GBoxOptions.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LblServer;
		private TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo ComboSQLServers;
		private System.Windows.Forms.TextBox TxtPassword;
		private System.Windows.Forms.Label LblPassword;
		private System.Windows.Forms.TextBox TxtLogin;
		private System.Windows.Forms.Label LblLogin;
		private System.Windows.Forms.GroupBox GBoxConnectionInfo;
		private System.Windows.Forms.GroupBox GBoxDataToLoad;
		private System.Windows.Forms.RadioButton RadioSampleData;
		private System.Windows.Forms.RadioButton RadioDefaultData;
		private System.Windows.Forms.CheckBox ChBoxShowOptions;
		private System.Windows.Forms.GroupBox GBoxOptions;
		private System.Windows.Forms.TextBox TxtCompany;
		private System.Windows.Forms.TextBox TxtCompanyDB;
		private System.Windows.Forms.TextBox TxtSystemDB;
		private System.Windows.Forms.Label LblSystemDB;
		private System.Windows.Forms.Label LblCompany;
		private System.Windows.Forms.Label LblCompanyDB;
	}
}
