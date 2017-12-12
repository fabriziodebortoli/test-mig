namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	partial class MatViews
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MatViews));
			this.LblInfo = new System.Windows.Forms.Label();
			this.BtnCheck = new System.Windows.Forms.Button();
			this.ListMsg = new System.Windows.Forms.ListView();
			this.PictBoxProgress = new System.Windows.Forms.PictureBox();
			this.GBoxActions = new System.Windows.Forms.GroupBox();
			this.RBtnCreateViews = new System.Windows.Forms.RadioButton();
			this.RBtnDropViews = new System.Windows.Forms.RadioButton();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).BeginInit();
			this.GBoxActions.SuspendLayout();
			this.SuspendLayout();
			// 
			// LblInfo
			// 
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.Name = "LblInfo";
			// 
			// BtnCheck
			// 
			resources.ApplyResources(this.BtnCheck, "BtnCheck");
			this.BtnCheck.Name = "BtnCheck";
			this.BtnCheck.UseVisualStyleBackColor = true;
			this.BtnCheck.Click += new System.EventHandler(this.BtnCheck_Click);
			// 
			// ListMsg
			// 
			resources.ApplyResources(this.ListMsg, "ListMsg");
			this.ListMsg.BackColor = System.Drawing.SystemColors.Control;
			this.ListMsg.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.ListMsg.HideSelection = false;
			this.ListMsg.Name = "ListMsg";
			this.ListMsg.UseCompatibleStateImageBehavior = false;
			this.ListMsg.View = System.Windows.Forms.View.Details;
			// 
			// PictBoxProgress
			// 
			resources.ApplyResources(this.PictBoxProgress, "PictBoxProgress");
			this.PictBoxProgress.Image = global::Microarea.Console.Plugin.ApplicationDBAdmin.Strings.Loader;
			this.PictBoxProgress.Name = "PictBoxProgress";
			this.PictBoxProgress.TabStop = false;
			// 
			// GBoxActions
			// 
			resources.ApplyResources(this.GBoxActions, "GBoxActions");
			this.GBoxActions.Controls.Add(this.RBtnCreateViews);
			this.GBoxActions.Controls.Add(this.RBtnDropViews);
			this.GBoxActions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GBoxActions.Name = "GBoxActions";
			this.GBoxActions.TabStop = false;
			// 
			// RBtnCreateViews
			// 
			resources.ApplyResources(this.RBtnCreateViews, "RBtnCreateViews");
			this.RBtnCreateViews.Checked = true;
			this.RBtnCreateViews.Name = "RBtnCreateViews";
			this.RBtnCreateViews.TabStop = true;
			this.RBtnCreateViews.UseVisualStyleBackColor = true;
			// 
			// RBtnDropViews
			// 
			resources.ApplyResources(this.RBtnDropViews, "RBtnDropViews");
			this.RBtnDropViews.Name = "RBtnDropViews";
			this.RBtnDropViews.UseVisualStyleBackColor = true;
			// 
			// BtnOk
			// 
			resources.ApplyResources(this.BtnOk, "BtnOk");
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.UseVisualStyleBackColor = true;
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			// 
			// MatViews
			// 
			this.AcceptButton = this.BtnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BtnCancel;
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.GBoxActions);
			this.Controls.Add(this.PictBoxProgress);
			this.Controls.Add(this.ListMsg);
			this.Controls.Add(this.BtnCheck);
			this.Controls.Add(this.LblInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "MatViews";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MatViews_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.PictBoxProgress)).EndInit();
			this.GBoxActions.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.Button BtnCheck;
		private System.Windows.Forms.ListView ListMsg;
		private System.Windows.Forms.PictureBox PictBoxProgress;
		private System.Windows.Forms.GroupBox GBoxActions;
		private System.Windows.Forms.RadioButton RBtnCreateViews;
		private System.Windows.Forms.RadioButton RBtnDropViews;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Button BtnCancel;
	}
}