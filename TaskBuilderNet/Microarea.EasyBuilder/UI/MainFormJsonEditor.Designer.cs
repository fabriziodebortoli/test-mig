using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using Microarea.TaskBuilderNet.UI.WinControls.Others;
using WeifenLuo.WinFormsUI.Docking;
namespace Microarea.EasyBuilder.UI
{
	partial class MainFormJsonEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormJsonEditor));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.toolStrip = new Microarea.TaskBuilderNet.UI.WinControls.Others.ClickThroughToolStrip();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbTabOrder = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbToolbox = new System.Windows.Forms.ToolStripButton();
			this.tsbOpenJsonEditor = new System.Windows.Forms.ToolStripButton();
			this.tsbProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbViewOutline = new System.Windows.Forms.ToolStripButton();
			this.tsbFormsExplorer = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbAlignLeft = new System.Windows.Forms.ToolStripButton();
			this.tsbAlignRight = new System.Windows.Forms.ToolStripButton();
			this.tsbAlignBottom = new System.Windows.Forms.ToolStripButton();
			this.tsbAlignTop = new System.Windows.Forms.ToolStripButton();
			this.tsbMakeSameWidth = new System.Windows.Forms.ToolStripButton();
			this.tsbMakeSameHeight = new System.Windows.Forms.ToolStripButton();
			this.tsbMakeSameSize = new System.Windows.Forms.ToolStripButton();
			this.tsbDistributeHorizontally = new System.Windows.Forms.ToolStripButton();
			this.tsbDistributeVertically = new System.Windows.Forms.ToolStripButton();
			this.tsbCenterHorizontally = new System.Windows.Forms.ToolStripButton();
			this.tsbCenterVertically = new System.Windows.Forms.ToolStripButton();
			this.tsbUndo = new System.Windows.Forms.ToolStripButton();
			this.tsbRedo = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.lblActiveAppAndModule = new System.Windows.Forms.ToolStripLabel();
			this.tslRecent = new System.Windows.Forms.ToolStripLabel();
			this.tscbRecent = new System.Windows.Forms.ToolStripComboBox();
			this.tsbCloseCurrentDialog = new System.Windows.Forms.ToolStripButton();
			this.tsbRefreshRecents = new System.Windows.Forms.ToolStripButton();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
			resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
			this.toolStripContainer1.Name = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip);
			// 
			// toolStrip
			// 
			this.toolStrip.ClickThrough = true;
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.toolStripSeparator1,
            this.tsbTabOrder,
            this.toolStripSeparator6,
            this.tsbToolbox,
            this.tsbOpenJsonEditor,
            this.tsbProperties,
            this.tsbViewOutline,
            this.tsbFormsExplorer,
            this.toolStripSeparator7,
            this.tsbAlignLeft,
            this.tsbAlignRight,
            this.tsbAlignBottom,
            this.tsbAlignTop,
            this.tsbMakeSameWidth,
            this.tsbMakeSameHeight,
            this.tsbMakeSameSize,
            this.tsbDistributeHorizontally,
            this.tsbDistributeVertically,
            this.tsbCenterHorizontally,
            this.tsbCenterVertically,
            this.tsbUndo,
            this.tsbRedo,
            this.toolStripSeparator2,
            this.lblActiveAppAndModule,
            this.tslRecent,
            this.tscbRecent,
            this.tsbCloseCurrentDialog,
            this.tsbRefreshRecents});
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip_ItemClicked);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Image = global::Microarea.EasyBuilder.Properties.Resources.Save24;
			resources.ApplyResources(this.tsbSave, "tsbSave");
			this.tsbSave.Name = "tsbSave";
			this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
			// 
			// toolStripSeparator1
			// 
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			// 
			// tsbTabOrder
			// 
			this.tsbTabOrder.CheckOnClick = true;
			this.tsbTabOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbTabOrder.Image = global::Microarea.EasyBuilder.Properties.Resources.TabOrder24;
			resources.ApplyResources(this.tsbTabOrder, "tsbTabOrder");
			this.tsbTabOrder.Name = "tsbTabOrder";
			this.tsbTabOrder.Click += new System.EventHandler(this.tsbTabOrder_Click);
			// 
			// toolStripSeparator6
			// 
			resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			// 
			// tsbToolbox
			// 
			this.tsbToolbox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbToolbox.Image = global::Microarea.EasyBuilder.Properties.Resources.Toolbox24;
			resources.ApplyResources(this.tsbToolbox, "tsbToolbox");
			this.tsbToolbox.Name = "tsbToolbox";
			this.tsbToolbox.Tag = "";
			this.tsbToolbox.Click += new System.EventHandler(this.tsbToolbox_Click);
			// 
			// tsbOpenJsonEditor
			// 
			this.tsbOpenJsonEditor.Image = global::Microarea.EasyBuilder.Properties.Resources.Outline24;
			resources.ApplyResources(this.tsbOpenJsonEditor, "tsbOpenJsonEditor");
			this.tsbOpenJsonEditor.Name = "tsbOpenJsonEditor";
			this.tsbOpenJsonEditor.Tag = "";
			this.tsbOpenJsonEditor.Click += new System.EventHandler(this.tsbOpenJsonEditor_Click);
			// 
			// tsbProperties
			// 
			this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties24;
			resources.ApplyResources(this.tsbProperties, "tsbProperties");
			this.tsbProperties.Name = "tsbProperties";
			this.tsbProperties.Tag = "";
			this.tsbProperties.Click += new System.EventHandler(this.tsbProperties_Click);
			// 
			// tsbViewOutline
			// 
			this.tsbViewOutline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbViewOutline.Image = global::Microarea.EasyBuilder.Properties.Resources.Broadcasting24;
			resources.ApplyResources(this.tsbViewOutline, "tsbViewOutline");
			this.tsbViewOutline.Name = "tsbViewOutline";
			this.tsbViewOutline.Tag = "";
			this.tsbViewOutline.Click += new System.EventHandler(this.tsbViewOutline_Click);
			// 
			// tsbFormsExplorer
			// 
			this.tsbFormsExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbFormsExplorer.Image = global::Microarea.EasyBuilder.Properties.Resources.MultipleInputs24;
			resources.ApplyResources(this.tsbFormsExplorer, "tsbFormsExplorer");
			this.tsbFormsExplorer.Name = "tsbFormsExplorer";
			this.tsbFormsExplorer.Tag = "";
			this.tsbFormsExplorer.Click += new System.EventHandler(this.tsbFormsExplorer_Click);
			// 
			// toolStripSeparator7
			// 
			resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			// 
			// tsbAlignLeft
			// 
			this.tsbAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAlignLeft.Image = global::Microarea.EasyBuilder.Properties.Resources.AlignLeft;
			this.tsbAlignLeft.Name = "tsbAlignLeft";
			resources.ApplyResources(this.tsbAlignLeft, "tsbAlignLeft");
			this.tsbAlignLeft.Tag = Microarea.EasyBuilder.Selections.Action.AlignLeft;
			// 
			// tsbAlignRight
			// 
			this.tsbAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAlignRight.Image = global::Microarea.EasyBuilder.Properties.Resources.AlignRight;
			this.tsbAlignRight.Name = "tsbAlignRight";
			resources.ApplyResources(this.tsbAlignRight, "tsbAlignRight");
			this.tsbAlignRight.Tag = Microarea.EasyBuilder.Selections.Action.AlignRight;
			// 
			// tsbAlignBottom
			// 
			this.tsbAlignBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAlignBottom.Image = global::Microarea.EasyBuilder.Properties.Resources.AlignBottom;
			this.tsbAlignBottom.Name = "tsbAlignBottom";
			resources.ApplyResources(this.tsbAlignBottom, "tsbAlignBottom");
			this.tsbAlignBottom.Tag = Microarea.EasyBuilder.Selections.Action.AlignBottom;
			// 
			// tsbAlignTop
			// 
			this.tsbAlignTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAlignTop.Image = global::Microarea.EasyBuilder.Properties.Resources.AlignTop;
			this.tsbAlignTop.Name = "tsbAlignTop";
			resources.ApplyResources(this.tsbAlignTop, "tsbAlignTop");
			this.tsbAlignTop.Tag = Microarea.EasyBuilder.Selections.Action.AlignTop;
			// 
			// tsbMakeSameWidth
			// 
			this.tsbMakeSameWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbMakeSameWidth.Image = global::Microarea.EasyBuilder.Properties.Resources.MakeSameWidht;
			this.tsbMakeSameWidth.Name = "tsbMakeSameWidth";
			resources.ApplyResources(this.tsbMakeSameWidth, "tsbMakeSameWidth");
			this.tsbMakeSameWidth.Tag = Microarea.EasyBuilder.Selections.Action.SameWidth;
			// 
			// tsbMakeSameHeight
			// 
			this.tsbMakeSameHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbMakeSameHeight.Image = global::Microarea.EasyBuilder.Properties.Resources.MakeSameHeight;
			this.tsbMakeSameHeight.Name = "tsbMakeSameHeight";
			resources.ApplyResources(this.tsbMakeSameHeight, "tsbMakeSameHeight");
			this.tsbMakeSameHeight.Tag = Microarea.EasyBuilder.Selections.Action.SameHeight;
			// 
			// tsbMakeSameSize
			// 
			this.tsbMakeSameSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbMakeSameSize.Image = global::Microarea.EasyBuilder.Properties.Resources.MakeSameSize;
			this.tsbMakeSameSize.Name = "tsbMakeSameSize";
			resources.ApplyResources(this.tsbMakeSameSize, "tsbMakeSameSize");
			this.tsbMakeSameSize.Tag = Microarea.EasyBuilder.Selections.Action.SameSize;
			// 
			// tsbDistributeHorizontally
			// 
			this.tsbDistributeHorizontally.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDistributeHorizontally.Image = global::Microarea.EasyBuilder.Properties.Resources.MakeHorizantalSpaceEqual;
			this.tsbDistributeHorizontally.Name = "tsbDistributeHorizontally";
			resources.ApplyResources(this.tsbDistributeHorizontally, "tsbDistributeHorizontally");
			this.tsbDistributeHorizontally.Tag = Microarea.EasyBuilder.Selections.Action.DistributeHorizontally;
			// 
			// tsbDistributeVertically
			// 
			this.tsbDistributeVertically.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDistributeVertically.Image = global::Microarea.EasyBuilder.Properties.Resources.MakeVerticalSpaceEqual;
			this.tsbDistributeVertically.Name = "tsbDistributeVertically";
			resources.ApplyResources(this.tsbDistributeVertically, "tsbDistributeVertically");
			this.tsbDistributeVertically.Tag = Microarea.EasyBuilder.Selections.Action.DistributeVertically;
			// 
			// tsbCenterHorizontally
			// 
			this.tsbCenterHorizontally.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCenterHorizontally.Image = global::Microarea.EasyBuilder.Properties.Resources.CenterHoriz;
			this.tsbCenterHorizontally.Name = "tsbCenterHorizontally";
			resources.ApplyResources(this.tsbCenterHorizontally, "tsbCenterHorizontally");
			this.tsbCenterHorizontally.Tag = Microarea.EasyBuilder.Selections.Action.CenterHorizontally;
			// 
			// tsbCenterVertically
			// 
			this.tsbCenterVertically.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCenterVertically.Image = global::Microarea.EasyBuilder.Properties.Resources.CenterVert;
			this.tsbCenterVertically.Name = "tsbCenterVertically";
			resources.ApplyResources(this.tsbCenterVertically, "tsbCenterVertically");
			this.tsbCenterVertically.Tag = Microarea.EasyBuilder.Selections.Action.CenterVertically;
			// 
			// tsbUndo
			// 
			this.tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbUndo, "tsbUndo");
			this.tsbUndo.Name = "tsbUndo";
			this.tsbUndo.Tag = "";
			this.tsbUndo.Click += new System.EventHandler(this.tsbUndo_Click);
			// 
			// tsbRedo
			// 
			this.tsbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tsbRedo, "tsbRedo");
			this.tsbRedo.Name = "tsbRedo";
			this.tsbRedo.Tag = "";
			this.tsbRedo.Click += new System.EventHandler(this.tsbRedo_Click);
			// 
			// toolStripSeparator2
			// 
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			// 
			// lblActiveAppAndModule
			// 
			resources.ApplyResources(this.lblActiveAppAndModule, "lblActiveAppAndModule");
			this.lblActiveAppAndModule.ForeColor = System.Drawing.Color.Black;
			this.lblActiveAppAndModule.Name = "lblActiveAppAndModule";
			// 
			// tslRecent
			// 
			this.tslRecent.Name = "tslRecent";
			resources.ApplyResources(this.tslRecent, "tslRecent");
			// 
			// tscbRecent
			// 
			this.tscbRecent.AutoToolTip = true;
			this.tscbRecent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.tscbRecent.Name = "tscbRecent";
			resources.ApplyResources(this.tscbRecent, "tscbRecent");
			this.tscbRecent.SelectedIndexChanged += new System.EventHandler(this.tscbRecent_SelectedIndexChanged);
			// 
			// tsbCloseCurrentDialog
			// 
			this.tsbCloseCurrentDialog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCloseCurrentDialog.Image = global::Microarea.EasyBuilder.Properties.Resources.Exit24;
			this.tsbCloseCurrentDialog.Name = "tsbCloseCurrentDialog";
			resources.ApplyResources(this.tsbCloseCurrentDialog, "tsbCloseCurrentDialog");
			this.tsbCloseCurrentDialog.Click += new System.EventHandler(this.tsbCloseCurrentDialog_Click);
			// 
			// tsbRefreshRecents
			// 
			this.tsbRefreshRecents.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbRefreshRecents.Image = global::Microarea.EasyBuilder.Properties.Resources.Refresh;
			resources.ApplyResources(this.tsbRefreshRecents, "tsbRefreshRecents");
			this.tsbRefreshRecents.Name = "tsbRefreshRecents";
			this.tsbRefreshRecents.Click += new System.EventHandler(this.tsbRefreshRecents_Click);
			// 
			// MainFormJsonEditor
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "MainFormJsonEditor";
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}


		#endregion

		private ToolStripContainer toolStripContainer1;
		private ClickThroughToolStrip toolStrip;
		private ToolStripButton tsbSave;
		private ToolStripButton tsbTabOrder;
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripButton tsbToolbox;
		private ToolStripButton tsbOpenJsonEditor;
		private ToolStripButton tsbProperties;
		private ToolStripButton tsbViewOutline;
		private ToolStripButton tsbFormsExplorer;
		private ToolStripSeparator toolStripSeparator7;
		private ToolStripButton tsbAlignLeft;
		private ToolStripButton tsbAlignRight;
		private ToolStripButton tsbAlignBottom;
		private ToolStripButton tsbAlignTop;
		private ToolStripButton tsbMakeSameWidth;
		private ToolStripButton tsbMakeSameHeight;
		private ToolStripButton tsbMakeSameSize;
		private ToolStripButton tsbDistributeHorizontally;
		private ToolStripButton tsbDistributeVertically;
		private ToolStripButton tsbCenterHorizontally;
		private ToolStripButton tsbCenterVertically;
		private ToolStripButton tsbUndo;
		private ToolStripButton tsbRedo;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripLabel lblActiveAppAndModule;
		private ToolStripLabel tslRecent;
		private ToolStripComboBox tscbRecent;
		private ToolStripButton tsbCloseCurrentDialog;
		private ToolStripButton tsbRefreshRecents;
		private ToolStripSeparator toolStripSeparator1;
		private ToolTip toolTip;
	}
}
