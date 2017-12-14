using System;
using System.Windows.Forms;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
namespace Microarea.EasyBuilder.MenuEditor
{
	partial class MainToolbar
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
				if (components != null)
					components.Dispose();

				if (menuDesignForm != null)
				{
					if (!menuDesignForm.IsDisposed)
						menuDesignForm.DirtyChanged -= new EventHandler<DirtyChangedEventArgs>(MenuDesignForm_DirtyChanged);

					menuDesignForm = null;
				}

				if (propertyEditor != null)
				{
					if (!propertyEditor.IsDisposed)
						propertyEditor.FormClosed -= (FormClosedEventHandler)delegate { propertyEditor.HostedControl.OnClose(); propertyEditor = null; };

					propertyEditor = null;
				}
				
				SafeDispose<TBDockContent<PropertyEditor>>(ref propertyEditor);
			}
			base.Dispose(disposing);
		}

		private void SafeDispose<T>(ref T ctrl) where T: class, IDisposable
		{
			if (ctrl != null)
			{
				try
				{
					ctrl.Dispose();
					ctrl = null;
				}
				catch { }
			}
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainToolbar));
			this.mnuDataManagers = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuDataManagersAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDataManagersEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDataManagersDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuDataManagersProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.viewModelImages = new System.Windows.Forms.ImageList(this.components);
			this.tsSave = new System.Windows.Forms.ToolStripMenuItem();
			this.tsbApplyChanges = new System.Windows.Forms.ToolStripButton();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tsbProperties = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.lblActiveApplication = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			this.lblActiveModule = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.mnuDataManagers.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// mnuDataManagers
			// 
			this.mnuDataManagers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDataManagersAdd,
            this.mnuDataManagersEdit,
            this.mnuDataManagersDelete,
            this.toolStripSeparator1,
            this.mnuDataManagersProperties});
			this.mnuDataManagers.Name = "mnuDataManagers";
			resources.ApplyResources(this.mnuDataManagers, "mnuDataManagers");
			// 
			// mnuDataManagersAdd
			// 
			this.mnuDataManagersAdd.Name = "mnuDataManagersAdd";
			resources.ApplyResources(this.mnuDataManagersAdd, "mnuDataManagersAdd");
			// 
			// mnuDataManagersEdit
			// 
			this.mnuDataManagersEdit.Name = "mnuDataManagersEdit";
			resources.ApplyResources(this.mnuDataManagersEdit, "mnuDataManagersEdit");
			// 
			// mnuDataManagersDelete
			// 
			this.mnuDataManagersDelete.Name = "mnuDataManagersDelete";
			resources.ApplyResources(this.mnuDataManagersDelete, "mnuDataManagersDelete");
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// mnuDataManagersProperties
			// 
			this.mnuDataManagersProperties.Name = "mnuDataManagersProperties";
			resources.ApplyResources(this.mnuDataManagersProperties, "mnuDataManagersProperties");
			// 
			// viewModelImages
			// 
			this.viewModelImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("viewModelImages.ImageStream")));
			this.viewModelImages.TransparentColor = System.Drawing.Color.Transparent;
			this.viewModelImages.Images.SetKeyName(0, "Formatter.png");
			this.viewModelImages.Images.SetKeyName(1, "Class.png");
			// 
			// tsSave
			// 
			this.tsSave.Name = "tsSave";
			resources.ApplyResources(this.tsSave, "tsSave");
			// 
			// tsbApplyChanges
			// 
			this.tsbApplyChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbApplyChanges.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveAndApply;
			resources.ApplyResources(this.tsbApplyChanges, "tsbApplyChanges");
			this.tsbApplyChanges.Name = "tsbApplyChanges";
			this.tsbApplyChanges.Click += new System.EventHandler(this.tsbApplyChanges_Click);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveAndExit;
			resources.ApplyResources(this.tsbSave, "tsbSave");
			this.tsbSave.Name = "tsbSave";
			this.tsbSave.Click += new System.EventHandler(this.TsbSaveAndExit_Click);
			// 
			// tsbProperties
			// 
			this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties24;
			resources.ApplyResources(this.tsbProperties, "tsbProperties");
			this.tsbProperties.Name = "tsbProperties";
			this.tsbProperties.Click += new System.EventHandler(this.TsbProperties_Click);
			// 
			// toolStripSeparator2
			// 
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			// 
			// toolStripLabel1
			// 
			resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
			this.toolStripLabel1.Name = "toolStripLabel1";
			// 
			// lblActiveApplication
			// 
			resources.ApplyResources(this.lblActiveApplication, "lblActiveApplication");
			this.lblActiveApplication.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.lblActiveApplication.Name = "lblActiveApplication";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// toolStripLabel3
			// 
			resources.ApplyResources(this.toolStripLabel3, "toolStripLabel3");
			this.toolStripLabel3.Name = "toolStripLabel3";
			// 
			// lblActiveModule
			// 
			resources.ApplyResources(this.lblActiveModule, "lblActiveModule");
			this.lblActiveModule.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.lblActiveModule.Name = "lblActiveModule";
			// 
			// toolStrip
			// 
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbApplyChanges,
            this.tsbSave,
            this.tsbProperties,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.lblActiveApplication,
            this.toolStripSeparator3,
            this.toolStripLabel3,
            this.lblActiveModule});
			this.toolStrip.Name = "toolStrip";
			// 
			// MainToolbar
			// 
			this.Controls.Add(this.toolStrip);
			resources.ApplyResources(this, "$this");
			this.Name = "MainToolbar";
			this.mnuDataManagers.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ContextMenuStrip mnuDataManagers;
		private ToolStripMenuItem mnuDataManagersAdd;
		private ToolStripMenuItem mnuDataManagersEdit;
		private ToolStripMenuItem mnuDataManagersDelete;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem mnuDataManagersProperties;
		private ImageList viewModelImages;
		private ToolStripMenuItem tsSave;
		private ToolStripButton tsbApplyChanges;
		private ToolStripButton tsbSave;
		private ToolStripButton tsbProperties;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripLabel toolStripLabel1;
		private ToolStripLabel lblActiveApplication;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripLabel toolStripLabel3;
		private ToolStripLabel lblActiveModule;
		private ToolStrip toolStrip;
	}
}
