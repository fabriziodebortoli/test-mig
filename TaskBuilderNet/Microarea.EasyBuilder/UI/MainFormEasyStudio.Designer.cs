using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using Microarea.TaskBuilderNet.UI.WinControls.Others;
using WeifenLuo.WinFormsUI.Docking;
namespace Microarea.EasyBuilder.UI
{
    partial class MainFormEasyStudio 
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormEasyStudio));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.toolStrip = new Microarea.TaskBuilderNet.UI.WinControls.Others.ClickThroughToolStrip();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tsbSaveAs = new System.Windows.Forms.ToolStripButton();
			this.tsbOptions = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbTabOrder = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbToolbox = new System.Windows.Forms.ToolStripButton();
			this.tsbOpenCodeEditor = new System.Windows.Forms.ToolStripButton();
			this.tsbLocalization = new System.Windows.Forms.ToolStripButton();
			this.tsbProperties = new System.Windows.Forms.ToolStripButton();
			this.tsbObjectModel = new System.Windows.Forms.ToolStripButton();
			this.tsbViewOutline = new System.Windows.Forms.ToolStripButton();
			this.tsbDBExplorer = new System.Windows.Forms.ToolStripButton();
			this.tsbHotLinks = new System.Windows.Forms.ToolStripButton();
			this.tsbBusinessObjects = new System.Windows.Forms.ToolStripButton();
			this.tsbEnums = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolAlignStaticArea = new System.Windows.Forms.ToolStripButton();
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
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tscomboLanguages = new System.Windows.Forms.ToolStripComboBox();
			this.tsbAddLanguage = new System.Windows.Forms.ToolStripButton();
			this.tsbRemoveLanguage = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabelActiveAppAndMod = new System.Windows.Forms.ToolStripLabel();
			this.lblActiveAppAndModule = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbExit = new System.Windows.Forms.ToolStripButton();
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
			resources.ApplyResources(this.toolStrip, "toolStrip");
			this.toolStrip.ClickThrough = true;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbSaveAs,
            this.tsbOptions,
            this.toolStripSeparator1,
            this.tsbTabOrder,
            this.toolStripSeparator6,
            this.tsbToolbox,
            this.tsbOpenCodeEditor,
            this.tsbLocalization,
            this.tsbProperties,
            this.tsbObjectModel,
            this.tsbViewOutline,
            this.tsbDBExplorer,
            this.tsbHotLinks,
            this.tsbBusinessObjects,
            this.tsbEnums,
            this.toolStripSeparator7,
            this.toolAlignStaticArea,
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
            this.toolStripSeparator2,
            this.tscomboLanguages,
            this.tsbAddLanguage,
            this.tsbRemoveLanguage,
            this.toolStripSeparator4,
            this.toolStripLabelActiveAppAndMod,
            this.lblActiveAppAndModule,
            this.toolStripSeparator3,
            this.tsbExit});
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.estb_ItemClicked);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Image = global::Microarea.EasyBuilder.Properties.Resources.Save24;
			resources.ApplyResources(this.tsbSave, "tsbSave");
			this.tsbSave.Name = "tsbSave";
			this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
			// 
			// tsbSaveAs
			// 
			this.tsbSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSaveAs.Image = global::Microarea.EasyBuilder.Properties.Resources.SaveAs24;
			resources.ApplyResources(this.tsbSaveAs, "tsbSaveAs");
			this.tsbSaveAs.Name = "tsbSaveAs";
			this.tsbSaveAs.Click += new System.EventHandler(this.tsbSaveAs_Click);
			// 
			// tsbOptions
			// 
			this.tsbOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOptions.Image = global::Microarea.EasyBuilder.Properties.Resources.Options24;
			resources.ApplyResources(this.tsbOptions, "tsbOptions");
			this.tsbOptions.Name = "tsbOptions";
			this.tsbOptions.Click += new System.EventHandler(this.tsbOptions_Click);
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
			this.tsbToolbox.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.ToolboxControl;
			this.tsbToolbox.Click += new System.EventHandler(this.tsbToolbox_Click);
			// 
			// tsbOpenCodeEditor
			// 
			this.tsbOpenCodeEditor.Image = global::Microarea.EasyBuilder.Properties.Resources.Outline24;
			resources.ApplyResources(this.tsbOpenCodeEditor, "tsbOpenCodeEditor");
			this.tsbOpenCodeEditor.Name = "tsbOpenCodeEditor";
			this.tsbOpenCodeEditor.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.OpenCodeEditor;
			this.tsbOpenCodeEditor.Click += new System.EventHandler(this.tsbOpenCodeEditor_Click);
			// 
			// tsbLocalization
			// 
			this.tsbLocalization.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbLocalization.Image = global::Microarea.EasyBuilder.Properties.Resources.Localization24;
			resources.ApplyResources(this.tsbLocalization, "tsbLocalization");
			this.tsbLocalization.Name = "tsbLocalization";
			this.tsbLocalization.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.Localization;
			this.tsbLocalization.Click += new System.EventHandler(this.tsbLocalization_Click);
			// 
			// tsbProperties
			// 
			this.tsbProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbProperties.Image = global::Microarea.EasyBuilder.Properties.Resources.Properties24;
			resources.ApplyResources(this.tsbProperties, "tsbProperties");
			this.tsbProperties.Name = "tsbProperties";
			this.tsbProperties.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.PropertyEditor;
			this.tsbProperties.Click += new System.EventHandler(this.tsbProperties_Click);
			// 
			// tsbObjectModel
			// 
			this.tsbObjectModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbObjectModel.Image = global::Microarea.EasyBuilder.Properties.Resources.ObjectModel24;
			resources.ApplyResources(this.tsbObjectModel, "tsbObjectModel");
			this.tsbObjectModel.Name = "tsbObjectModel";
			this.tsbObjectModel.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.ObjectModelTreeControl;
			this.tsbObjectModel.Click += new System.EventHandler(this.tsbObjectModel_Click);
			// 
			// tsbViewOutline
			// 
			this.tsbViewOutline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbViewOutline.Image = global::Microarea.EasyBuilder.Properties.Resources.Broadcasting24;
			resources.ApplyResources(this.tsbViewOutline, "tsbViewOutline");
			this.tsbViewOutline.Name = "tsbViewOutline";
			this.tsbViewOutline.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.ViewOutlineTreeControl;
			this.tsbViewOutline.Click += new System.EventHandler(this.tsbViewOutline_Click);
			// 
			// tsbDBExplorer
			// 
			this.tsbDBExplorer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbDBExplorer.Image = global::Microarea.EasyBuilder.Properties.Resources.Database24;
			resources.ApplyResources(this.tsbDBExplorer, "tsbDBExplorer");
			this.tsbDBExplorer.Name = "tsbDBExplorer";
			this.tsbDBExplorer.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.DatabaseExplorer;
			this.tsbDBExplorer.Click += new System.EventHandler(this.tsbDBExplorer_Click);
			// 
			// tsbHotLinks
			// 
			this.tsbHotLinks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbHotLinks.Image = global::Microarea.EasyBuilder.Properties.Resources.HotLinks;
			resources.ApplyResources(this.tsbHotLinks, "tsbHotLinks");
			this.tsbHotLinks.Name = "tsbHotLinks";
			this.tsbHotLinks.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.HotLinksExplorer;
			this.tsbHotLinks.Click += new System.EventHandler(this.tsbHotLinks_Click);
			// 
			// tsbBusinessObjects
			// 
			resources.ApplyResources(this.tsbBusinessObjects, "tsbBusinessObjects");
			this.tsbBusinessObjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbBusinessObjects.Image = global::Microarea.EasyBuilder.Properties.Resources.BusinessObjects24x24;
			this.tsbBusinessObjects.Name = "tsbBusinessObjects";
			this.tsbBusinessObjects.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.BusinessObjectsExplorer;
			this.tsbBusinessObjects.Click += new System.EventHandler(this.tsbBusinessObjects_Click);
			// 
			// tsbEnums
			// 
			this.tsbEnums.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbEnums.Image = global::Microarea.EasyBuilder.Properties.Resources.Enums;
			resources.ApplyResources(this.tsbEnums, "tsbEnums");
			this.tsbEnums.Name = "tsbEnums";
			this.tsbEnums.Tag = Microarea.EasyBuilder.UI.MainFormEasyStudio.PanelItems.EnumsTreeControl;
			this.tsbEnums.Click += new System.EventHandler(this.tsbEnums_Click);
			// 
			// toolStripSeparator7
			// 
			resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			// 
			// toolAlignStaticArea
			// 
			this.toolAlignStaticArea.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolAlignStaticArea.Image = global::Microarea.EasyBuilder.Properties.Resources.AlignStaticArea;
			this.toolAlignStaticArea.Name = "toolAlignStaticArea";
			resources.ApplyResources(this.toolAlignStaticArea, "toolAlignStaticArea");
			this.toolAlignStaticArea.Tag = Microarea.EasyBuilder.Selections.Action.AlignToStaticArea;
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
			// toolStripSeparator2
			// 
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			// 
			// tscomboLanguages
			// 
			this.tscomboLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.tscomboLanguages.DropDownWidth = 300;
			resources.ApplyResources(this.tscomboLanguages, "tscomboLanguages");
			this.tscomboLanguages.Name = "tscomboLanguages";
			this.tscomboLanguages.SelectedIndexChanged += new System.EventHandler(this.tscomboLanguages_SelectedIndexChanged);
			// 
			// tsbAddLanguage
			// 
			this.tsbAddLanguage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbAddLanguage.Image = global::Microarea.EasyBuilder.Properties.Resources.AddLocalization;
			resources.ApplyResources(this.tsbAddLanguage, "tsbAddLanguage");
			this.tsbAddLanguage.Name = "tsbAddLanguage";
			this.tsbAddLanguage.Click += new System.EventHandler(this.tsbAddLanguage_Click);
			// 
			// tsbRemoveLanguage
			// 
			this.tsbRemoveLanguage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbRemoveLanguage.Image = global::Microarea.EasyBuilder.Properties.Resources.RemoveLocalization;
			resources.ApplyResources(this.tsbRemoveLanguage, "tsbRemoveLanguage");
			this.tsbRemoveLanguage.Name = "tsbRemoveLanguage";
			this.tsbRemoveLanguage.Click += new System.EventHandler(this.tsbRemoveLanguage_Click);
			// 
			// toolStripSeparator4
			// 
			resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			// 
			// toolStripLabelActiveAppAndMod
			// 
			this.toolStripLabelActiveAppAndMod.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			resources.ApplyResources(this.toolStripLabelActiveAppAndMod, "toolStripLabelActiveAppAndMod");
			this.toolStripLabelActiveAppAndMod.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripLabelActiveAppAndMod.Name = "toolStripLabelActiveAppAndMod";
			// 
			// lblActiveAppAndModule
			// 
			resources.ApplyResources(this.lblActiveAppAndModule, "lblActiveAppAndModule");
			this.lblActiveAppAndModule.ForeColor = System.Drawing.Color.Black;
			this.lblActiveAppAndModule.Name = "lblActiveAppAndModule";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// tsbExit
			// 
			this.tsbExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbExit.Image = global::Microarea.EasyBuilder.Properties.Resources.Exit24;
			resources.ApplyResources(this.tsbExit, "tsbExit");
			this.tsbExit.Name = "tsbExit";
			this.tsbExit.Click += new System.EventHandler(this.tsbExit_Click);
			// 
			// MainFormEasyStudio
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "MainFormEasyStudio";
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
		private ToolStripButton tsbSaveAs;
		private ToolStripButton tsbOptions;
		private ToolStripButton tsbTabOrder;
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripButton tsbToolbox;
		private ToolStripButton tsbOpenCodeEditor;
		private ToolStripButton tsbLocalization;
		private ToolStripButton tsbProperties;
		private ToolStripButton tsbObjectModel;
		private ToolStripButton tsbViewOutline;
		private ToolStripButton tsbDBExplorer;
		private ToolStripButton tsbHotLinks;
		private ToolStripButton tsbBusinessObjects;
		private ToolStripButton tsbEnums;
		private ToolStripSeparator toolStripSeparator7;
		private ToolStripButton toolAlignStaticArea;
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
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripComboBox tscomboLanguages;
		private ToolStripButton tsbAddLanguage;
		private ToolStripButton tsbRemoveLanguage;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripLabel toolStripLabelActiveAppAndMod;
		private ToolStripLabel lblActiveAppAndModule;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripButton tsbExit;
		private ToolTip toolTip;
		private ToolStripSeparator toolStripSeparator1;
	}
}
