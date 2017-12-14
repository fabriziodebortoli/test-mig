namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	partial class QuickStart
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickStart));
			this.ImgList = new System.Windows.Forms.ImageList(this.components);
			this.BtnRunBaseMode = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.TabControlPhantom = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.InvisibleTabControl();
			this.TabPageInfo = new System.Windows.Forms.TabPage();
			this.LblPresentation1 = new System.Windows.Forms.Label();
			this.LblCredentials = new System.Windows.Forms.Label();
			this.BtnRunAdvancedMode = new System.Windows.Forms.Button();
			this.LblPresentation = new System.Windows.Forms.Label();
			this.GBoxConnectionInfo = new System.Windows.Forms.GroupBox();
			this.TxtPassword = new System.Windows.Forms.TextBox();
			this.LblPassword = new System.Windows.Forms.Label();
			this.TxtLogin = new System.Windows.Forms.TextBox();
			this.LblLogin = new System.Windows.Forms.Label();
			this.LblServer = new System.Windows.Forms.Label();
			this.ComboSQLServers = new Microarea.TaskBuilderNet.Data.DatabaseWinControls.NGSQLServersCombo(this.components);
			this.TabPageElaboration = new System.Windows.Forms.TabPage();
			this.InfoPictureBox = new System.Windows.Forms.PictureBox();
			this.LblUserInfo = new System.Windows.Forms.Label();
			this.BtnBack = new System.Windows.Forms.Button();
			this.BtnShowDiagnostic = new System.Windows.Forms.Button();
			this.LblDetail = new System.Windows.Forms.Label();
			this.BtnStartElab = new System.Windows.Forms.Button();
			this.LblPresentation2 = new System.Windows.Forms.Label();
			this.PictBoxProgress = new System.Windows.Forms.PictureBox();
			this.ListOperations = new System.Windows.Forms.ListView();
			this.TabControlPhantom.SuspendLayout();
			this.TabPageInfo.SuspendLayout();
			this.GBoxConnectionInfo.SuspendLayout();
			this.TabPageElaboration.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InfoPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).BeginInit();
			this.SuspendLayout();
			// 
			// ImgList
			// 
			this.ImgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImgList.ImageStream")));
			this.ImgList.TransparentColor = System.Drawing.Color.Transparent;
			this.ImgList.Images.SetKeyName(0, "ResultGreen.png");
			this.ImgList.Images.SetKeyName(1, "ResultRed.png");
			this.ImgList.Images.SetKeyName(2, "Information.png");
			// 
			// BtnRunBaseMode
			// 
			resources.ApplyResources(this.BtnRunBaseMode, "BtnRunBaseMode");
			this.BtnRunBaseMode.Name = "BtnRunBaseMode";
			this.BtnRunBaseMode.UseVisualStyleBackColor = true;
			this.BtnRunBaseMode.Click += new System.EventHandler(this.BtnRunBaseMode_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			// 
			// TabControlPhantom
			// 
			this.TabControlPhantom.Controls.Add(this.TabPageInfo);
			this.TabControlPhantom.Controls.Add(this.TabPageElaboration);
			resources.ApplyResources(this.TabControlPhantom, "TabControlPhantom");
			this.TabControlPhantom.Name = "TabControlPhantom";
			this.TabControlPhantom.SelectedIndex = 0;
			// 
			// TabPageInfo
			// 
			this.TabPageInfo.BackColor = System.Drawing.Color.Lavender;
			this.TabPageInfo.Controls.Add(this.LblPresentation1);
			this.TabPageInfo.Controls.Add(this.LblCredentials);
			this.TabPageInfo.Controls.Add(this.BtnRunAdvancedMode);
			this.TabPageInfo.Controls.Add(this.LblPresentation);
			this.TabPageInfo.Controls.Add(this.GBoxConnectionInfo);
			this.TabPageInfo.Controls.Add(this.BtnCancel);
			this.TabPageInfo.Controls.Add(this.BtnRunBaseMode);
			resources.ApplyResources(this.TabPageInfo, "TabPageInfo");
			this.TabPageInfo.Name = "TabPageInfo";
			// 
			// LblPresentation1
			// 
			resources.ApplyResources(this.LblPresentation1, "LblPresentation1");
			this.LblPresentation1.Name = "LblPresentation1";
			// 
			// LblCredentials
			// 
			this.LblCredentials.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblCredentials, "LblCredentials");
			this.LblCredentials.Name = "LblCredentials";
			// 
			// BtnRunAdvancedMode
			// 
			resources.ApplyResources(this.BtnRunAdvancedMode, "BtnRunAdvancedMode");
			this.BtnRunAdvancedMode.Name = "BtnRunAdvancedMode";
			this.BtnRunAdvancedMode.UseVisualStyleBackColor = true;
			this.BtnRunAdvancedMode.Click += new System.EventHandler(this.BtnRunAdvancedMode_Click);
			// 
			// LblPresentation
			// 
			this.LblPresentation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblPresentation, "LblPresentation");
			this.LblPresentation.Name = "LblPresentation";
			// 
			// GBoxConnectionInfo
			// 
			this.GBoxConnectionInfo.Controls.Add(this.TxtPassword);
			this.GBoxConnectionInfo.Controls.Add(this.LblPassword);
			this.GBoxConnectionInfo.Controls.Add(this.TxtLogin);
			this.GBoxConnectionInfo.Controls.Add(this.LblLogin);
			this.GBoxConnectionInfo.Controls.Add(this.LblServer);
			this.GBoxConnectionInfo.Controls.Add(this.ComboSQLServers);
			this.GBoxConnectionInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.GBoxConnectionInfo, "GBoxConnectionInfo");
			this.GBoxConnectionInfo.Name = "GBoxConnectionInfo";
			this.GBoxConnectionInfo.TabStop = false;
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
			// LblServer
			// 
			resources.ApplyResources(this.LblServer, "LblServer");
			this.LblServer.Name = "LblServer";
			// 
			// ComboSQLServers
			// 
			this.ComboSQLServers.FormattingEnabled = true;
			resources.ApplyResources(this.ComboSQLServers, "ComboSQLServers");
			this.ComboSQLServers.Name = "ComboSQLServers";
			this.ComboSQLServers.SelectedSQLServer = "";
			// 
			// TabPageElaboration
			// 
			this.TabPageElaboration.BackColor = System.Drawing.Color.Lavender;
			this.TabPageElaboration.Controls.Add(this.InfoPictureBox);
			this.TabPageElaboration.Controls.Add(this.LblUserInfo);
			this.TabPageElaboration.Controls.Add(this.BtnBack);
			this.TabPageElaboration.Controls.Add(this.BtnShowDiagnostic);
			this.TabPageElaboration.Controls.Add(this.LblDetail);
			this.TabPageElaboration.Controls.Add(this.BtnStartElab);
			this.TabPageElaboration.Controls.Add(this.LblPresentation2);
			this.TabPageElaboration.Controls.Add(this.PictBoxProgress);
			this.TabPageElaboration.Controls.Add(this.ListOperations);
			resources.ApplyResources(this.TabPageElaboration, "TabPageElaboration");
			this.TabPageElaboration.Name = "TabPageElaboration";
			// 
			// InfoPictureBox
			// 
			this.InfoPictureBox.Image = global::Microarea.TaskBuilderNet.Data.Properties.Resources.Information;
			resources.ApplyResources(this.InfoPictureBox, "InfoPictureBox");
			this.InfoPictureBox.Name = "InfoPictureBox";
			this.InfoPictureBox.TabStop = false;
			// 
			// LblUserInfo
			// 
			resources.ApplyResources(this.LblUserInfo, "LblUserInfo");
			this.LblUserInfo.Name = "LblUserInfo";
			// 
			// BtnBack
			// 
			resources.ApplyResources(this.BtnBack, "BtnBack");
			this.BtnBack.Name = "BtnBack";
			this.BtnBack.UseVisualStyleBackColor = true;
			this.BtnBack.Click += new System.EventHandler(this.BtnBack_Click);
			// 
			// BtnShowDiagnostic
			// 
			resources.ApplyResources(this.BtnShowDiagnostic, "BtnShowDiagnostic");
			this.BtnShowDiagnostic.Name = "BtnShowDiagnostic";
			this.BtnShowDiagnostic.UseVisualStyleBackColor = true;
			this.BtnShowDiagnostic.Click += new System.EventHandler(this.BtnShowDiagnostic_Click);
			// 
			// LblDetail
			// 
			this.LblDetail.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblDetail, "LblDetail");
			this.LblDetail.Name = "LblDetail";
			// 
			// BtnStartElab
			// 
			resources.ApplyResources(this.BtnStartElab, "BtnStartElab");
			this.BtnStartElab.Name = "BtnStartElab";
			this.BtnStartElab.UseVisualStyleBackColor = true;
			this.BtnStartElab.Click += new System.EventHandler(this.BtnStartElab_Click);
			// 
			// LblPresentation2
			// 
			resources.ApplyResources(this.LblPresentation2, "LblPresentation2");
			this.LblPresentation2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblPresentation2.Name = "LblPresentation2";
			// 
			// PictBoxProgress
			// 
			this.PictBoxProgress.Image = global::Microarea.TaskBuilderNet.Data.Properties.Resources.Loader;
			resources.ApplyResources(this.PictBoxProgress, "PictBoxProgress");
			this.PictBoxProgress.Name = "PictBoxProgress";
			this.PictBoxProgress.TabStop = false;
			// 
			// ListOperations
			// 
			this.ListOperations.BackColor = System.Drawing.Color.Lavender;
			this.ListOperations.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ListOperations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.ListOperations.HideSelection = false;
			this.ListOperations.LargeImageList = this.ImgList;
			resources.ApplyResources(this.ListOperations, "ListOperations");
			this.ListOperations.MultiSelect = false;
			this.ListOperations.Name = "ListOperations";
			this.ListOperations.Scrollable = false;
			this.ListOperations.ShowGroups = false;
			this.ListOperations.SmallImageList = this.ImgList;
			this.ListOperations.UseCompatibleStateImageBehavior = false;
			this.ListOperations.View = System.Windows.Forms.View.Details;
			// 
			// QuickStart
			// 
			this.AcceptButton = this.BtnRunBaseMode;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.CancelButton = this.BtnCancel;
			this.Controls.Add(this.TabControlPhantom);
			this.MaximizeBox = false;
			this.Name = "QuickStart";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickStart_FormClosing);
			this.TabControlPhantom.ResumeLayout(false);
			this.TabPageInfo.ResumeLayout(false);
			this.GBoxConnectionInfo.ResumeLayout(false);
			this.GBoxConnectionInfo.PerformLayout();
			this.TabPageElaboration.ResumeLayout(false);
			this.TabPageElaboration.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InfoPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button BtnRunBaseMode;
		private System.Windows.Forms.Button BtnCancel;
		private Microarea.TaskBuilderNet.Data.DatabaseWinControls.InvisibleTabControl TabControlPhantom;
		private System.Windows.Forms.TabPage TabPageInfo;
		private System.Windows.Forms.TabPage TabPageElaboration;
		private System.Windows.Forms.Label LblPresentation2;
		private System.Windows.Forms.ListView ListOperations;
		private System.Windows.Forms.Button BtnStartElab;
		private System.Windows.Forms.ImageList ImgList;
		private System.Windows.Forms.Label LblDetail;
		private System.Windows.Forms.Label LblPresentation;
		private System.Windows.Forms.GroupBox GBoxConnectionInfo;
		private System.Windows.Forms.TextBox TxtPassword;
		private System.Windows.Forms.Label LblPassword;
		private System.Windows.Forms.TextBox TxtLogin;
		private System.Windows.Forms.Label LblLogin;
		private System.Windows.Forms.Label LblServer;
		private NGSQLServersCombo ComboSQLServers;
		private System.Windows.Forms.PictureBox PictBoxProgress;
		private System.Windows.Forms.Button BtnShowDiagnostic;
		private System.Windows.Forms.Button BtnBack;
		private System.Windows.Forms.Button BtnRunAdvancedMode;
		private System.Windows.Forms.Label LblCredentials;
		private System.Windows.Forms.Label LblUserInfo;
		private System.Windows.Forms.PictureBox InfoPictureBox;
		private System.Windows.Forms.Label LblPresentation1;
	}
}
