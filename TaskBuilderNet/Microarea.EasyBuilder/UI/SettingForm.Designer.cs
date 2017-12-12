namespace Microarea.EasyBuilder.UI
{
	partial class SettingForm
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
			if (disposing)
			{
				if (cred != null && !cred.IsDisposed)
				{
					cred.Dispose();
					cred = null;
				}
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabControlOptions = new System.Windows.Forms.TabControl();
			this.TabPageAccount = new System.Windows.Forms.TabPage();
			this.TabPageBackendUrl = new System.Windows.Forms.TabPage();
			this.TxtSNGeneratorUrl = new System.Windows.Forms.TextBox();
			this.LblSNGeneratorUrl = new System.Windows.Forms.Label();
			this.TxtCrypterUrl = new System.Windows.Forms.TextBox();
			this.LblCrypterUrl = new System.Windows.Forms.Label();
			this.TabPageCommon = new System.Windows.Forms.TabPage();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.tsbAdd = new System.Windows.Forms.ToolStripButton();
			this.tsbDelete = new System.Windows.Forms.ToolStripButton();
			this.cmsCommonAssemblies = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiAddAssembly = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiRemoveAssembly = new System.Windows.Forms.ToolStripMenuItem();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.cbUseMsBuild = new System.Windows.Forms.CheckBox();
			this.cbShowHiddenFields = new System.Windows.Forms.CheckBox();
			this.addRemoveAssemblyDisabled = new System.Windows.Forms.Label();
			this.treeViewAssemblies = new System.Windows.Forms.TreeView();
			this.tabControlOptions.SuspendLayout();
			this.TabPageBackendUrl.SuspendLayout();
			this.TabPageCommon.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.cmsCommonAssemblies.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// tabControlOptions
			// 
			this.tabControlOptions.Controls.Add(this.TabPageAccount);
			this.tabControlOptions.Controls.Add(this.TabPageBackendUrl);
			this.tabControlOptions.Controls.Add(this.TabPageCommon);
			resources.ApplyResources(this.tabControlOptions, "tabControlOptions");
			this.tabControlOptions.Name = "tabControlOptions";
			this.tabControlOptions.SelectedIndex = 0;
			// 
			// TabPageAccount
			// 
			resources.ApplyResources(this.TabPageAccount, "TabPageAccount");
			this.TabPageAccount.Name = "TabPageAccount";
			this.TabPageAccount.UseVisualStyleBackColor = true;
			// 
			// TabPageBackendUrl
			// 
			this.TabPageBackendUrl.Controls.Add(this.TxtSNGeneratorUrl);
			this.TabPageBackendUrl.Controls.Add(this.LblSNGeneratorUrl);
			this.TabPageBackendUrl.Controls.Add(this.TxtCrypterUrl);
			this.TabPageBackendUrl.Controls.Add(this.LblCrypterUrl);
			resources.ApplyResources(this.TabPageBackendUrl, "TabPageBackendUrl");
			this.TabPageBackendUrl.Name = "TabPageBackendUrl";
			this.TabPageBackendUrl.UseVisualStyleBackColor = true;
			// 
			// TxtSNGeneratorUrl
			// 
			resources.ApplyResources(this.TxtSNGeneratorUrl, "TxtSNGeneratorUrl");
			this.TxtSNGeneratorUrl.Name = "TxtSNGeneratorUrl";
			// 
			// LblSNGeneratorUrl
			// 
			resources.ApplyResources(this.LblSNGeneratorUrl, "LblSNGeneratorUrl");
			this.LblSNGeneratorUrl.Name = "LblSNGeneratorUrl";
			// 
			// TxtCrypterUrl
			// 
			resources.ApplyResources(this.TxtCrypterUrl, "TxtCrypterUrl");
			this.TxtCrypterUrl.Name = "TxtCrypterUrl";
			// 
			// LblCrypterUrl
			// 
			resources.ApplyResources(this.LblCrypterUrl, "LblCrypterUrl");
			this.LblCrypterUrl.Name = "LblCrypterUrl";
			// 
			// TabPageCommon
			// 
			this.TabPageCommon.Controls.Add(this.treeViewAssemblies);
			this.TabPageCommon.Controls.Add(this.toolStrip);
			resources.ApplyResources(this.TabPageCommon, "TabPageCommon");
			this.TabPageCommon.Name = "TabPageCommon";
			this.TabPageCommon.UseVisualStyleBackColor = true;
			this.TabPageCommon.Layout += new System.Windows.Forms.LayoutEventHandler(this.TabPageCommon_Layout);
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAdd,
            this.tsbDelete});
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.Name = "toolStrip";
			// 
			// tsbAdd
			// 
			this.tsbAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAdd.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			resources.ApplyResources(this.tsbAdd, "tsbAdd");
			this.tsbAdd.Name = "tsbAdd";
			this.tsbAdd.Click += new System.EventHandler(this.tsbAdd_Click);
			// 
			// tsbDelete
			// 
			this.tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbDelete, "tsbDelete");
			this.tsbDelete.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsbDelete.Name = "tsbDelete";
			this.tsbDelete.Click += new System.EventHandler(this.tsbDelete_Click);
			// 
			// cmsCommonAssemblies
			// 
			this.cmsCommonAssemblies.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddAssembly,
            this.tsmiRemoveAssembly});
			this.cmsCommonAssemblies.Name = "cmsCommonAssemblies";
			resources.ApplyResources(this.cmsCommonAssemblies, "cmsCommonAssemblies");
			this.cmsCommonAssemblies.Opening += new System.ComponentModel.CancelEventHandler(this.cmsCommonAssemblies_Opening);
			// 
			// tsmiAddAssembly
			// 
			this.tsmiAddAssembly.Image = global::Microarea.EasyBuilder.Properties.Resources.AddFile;
			this.tsmiAddAssembly.Name = "tsmiAddAssembly";
			resources.ApplyResources(this.tsmiAddAssembly, "tsmiAddAssembly");
			this.tsmiAddAssembly.Click += new System.EventHandler(this.tsmiAddAssembly_Click);
			// 
			// tsmiRemoveAssembly
			// 
			this.tsmiRemoveAssembly.Image = global::Microarea.EasyBuilder.Properties.Resources.Delete;
			this.tsmiRemoveAssembly.Name = "tsmiRemoveAssembly";
			resources.ApplyResources(this.tsmiRemoveAssembly, "tsmiRemoveAssembly");
			this.tsmiRemoveAssembly.Click += new System.EventHandler(this.tsmiRemoveAssembly_Click);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Folder.png");
			this.imageList1.Images.SetKeyName(1, "Module32x32.png");
			// 
			// cbUseMsBuild
			// 
			resources.ApplyResources(this.cbUseMsBuild, "cbUseMsBuild");
			this.cbUseMsBuild.Name = "cbUseMsBuild";
			this.cbUseMsBuild.UseVisualStyleBackColor = true;
			// 
			// cbShowHiddenFields
			// 
			resources.ApplyResources(this.cbShowHiddenFields, "cbShowHiddenFields");
			this.cbShowHiddenFields.Name = "cbShowHiddenFields";
			this.cbShowHiddenFields.UseVisualStyleBackColor = true;
			// 
			// addRemoveAssemblyDisabled
			// 
			this.addRemoveAssemblyDisabled.ForeColor = System.Drawing.Color.Red;
			resources.ApplyResources(this.addRemoveAssemblyDisabled, "addRemoveAssemblyDisabled");
			this.addRemoveAssemblyDisabled.Name = "addRemoveAssemblyDisabled";
			// 
			// treeViewAssemblies
			// 
			this.treeViewAssemblies.ContextMenuStrip = this.cmsCommonAssemblies;
			resources.ApplyResources(this.treeViewAssemblies, "treeViewAssemblies");
			this.treeViewAssemblies.ImageList = this.imageList1;
			this.treeViewAssemblies.Name = "treeViewAssemblies";
			this.treeViewAssemblies.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAssemblies_AfterSelect);
			// 
			// SettingForm
			// 
			this.AcceptButton = this.btnOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ControlBox = false;
			this.Controls.Add(this.addRemoveAssemblyDisabled);
			this.Controls.Add(this.tabControlOptions);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "SettingForm";
			this.tabControlOptions.ResumeLayout(false);
			this.TabPageBackendUrl.ResumeLayout(false);
			this.TabPageBackendUrl.PerformLayout();
			this.TabPageCommon.ResumeLayout(false);
			this.TabPageCommon.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.cmsCommonAssemblies.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabControlOptions;
		private System.Windows.Forms.TabPage TabPageAccount;
		private System.Windows.Forms.TabPage TabPageBackendUrl;
		private System.Windows.Forms.TextBox TxtSNGeneratorUrl;
		private System.Windows.Forms.Label LblSNGeneratorUrl;
		private System.Windows.Forms.TextBox TxtCrypterUrl;
		private System.Windows.Forms.Label LblCrypterUrl;
		private System.Windows.Forms.TabPage TabPageCommon;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton tsbAdd;
		private System.Windows.Forms.ToolStripButton tsbDelete;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ContextMenuStrip cmsCommonAssemblies;
		private System.Windows.Forms.ToolStripMenuItem tsmiAddAssembly;
		private System.Windows.Forms.ToolStripMenuItem tsmiRemoveAssembly;
		private System.Windows.Forms.CheckBox cbShowHiddenFields;
        private System.Windows.Forms.CheckBox cbUseMsBuild;
		private System.Windows.Forms.Label addRemoveAssemblyDisabled;
		private System.Windows.Forms.TreeView treeViewAssemblies;
	}
}