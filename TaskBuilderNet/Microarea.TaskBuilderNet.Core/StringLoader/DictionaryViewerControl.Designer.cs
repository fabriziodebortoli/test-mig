
namespace Microarea.TaskBuilderNet.Core.StringLoader
{
    partial class DictionaryViewerControl
    {
        private System.Windows.Forms.Button btnOpenDictionary;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.DataGrid dgDictionaryItems;
        private DictionaryDataSet dictionaryDataSet;
        private System.Windows.Forms.CheckedListBox checkedListBoxFilter;
        private System.Windows.Forms.TextBox txtBoxId;
        private System.Windows.Forms.TextBox txtBoxName;
        private System.Windows.Forms.Label LabelId;
        private System.Windows.Forms.Label labelName;
        private Microarea.TaskBuilderNet.Core.StringLoader.SatelliteAssemblyDataSet satelliteAssemblyDataSet;
        private System.Data.DataView dictionaryHeadDataView;
        private System.Windows.Forms.DataGrid dgDictionaryItemRows;
        private System.Data.DataView satelliteHeadDataView;
        private System.Data.DataView satelliteRowDataView;
        private System.Data.DataView dictionaryRowDataView;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle10;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle11;

        private System.Windows.Forms.DataGridTableStyle dataGridTableStyle2;
        private System.Windows.Forms.DataGridTableStyle dataGridTableDictMaster;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle dataGridTextBoxColumn1;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle100;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle101;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle102;
        private System.Windows.Forms.DataGridTableStyle dataGridTableStyleDictSlave;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle110;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle111;
        private System.Windows.Forms.DataGridTableStyle dataGridTableStyleSattSlave;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle121;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle122;
        private Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle autoSizeTextColumnStyle123;
        private System.Windows.Forms.Panel panelDictionary;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.TextBox textBoxType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Panel panelSatellite;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        //--------------------------------------------------------------------------------
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DictionaryViewerControl));
            this.btnOpenDictionary = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dgDictionaryItems = new System.Windows.Forms.DataGrid();
            this.dictionaryHeadDataView = new System.Data.DataView();
            this.dictionaryDataSet = new Microarea.TaskBuilderNet.Core.StringLoader.DictionaryDataSet();
            this.dataGridTableStyle2 = new System.Windows.Forms.DataGridTableStyle();
            this.autoSizeTextColumnStyle10 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle11 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.dataGridTableDictMaster = new System.Windows.Forms.DataGridTableStyle();
            this.autoSizeTextColumnStyle100 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle101 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle102 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.dgDictionaryItemRows = new System.Windows.Forms.DataGrid();
            this.dictionaryRowDataView = new System.Data.DataView();
            this.dataGridTableStyleDictSlave = new System.Windows.Forms.DataGridTableStyle();
            this.autoSizeTextColumnStyle110 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle111 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.dataGridTableStyleSattSlave = new System.Windows.Forms.DataGridTableStyle();
            this.autoSizeTextColumnStyle121 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle122 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.autoSizeTextColumnStyle123 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            this.checkedListBoxFilter = new System.Windows.Forms.CheckedListBox();
            this.txtBoxId = new System.Windows.Forms.TextBox();
            this.txtBoxName = new System.Windows.Forms.TextBox();
            this.LabelId = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.satelliteAssemblyDataSet = new Microarea.TaskBuilderNet.Core.StringLoader.SatelliteAssemblyDataSet();
            this.satelliteHeadDataView = new System.Data.DataView();
            this.satelliteRowDataView = new System.Data.DataView();
            this.panelDictionary = new System.Windows.Forms.Panel();
            this.labelType = new System.Windows.Forms.Label();
            this.panelSatellite = new System.Windows.Forms.Panel();
            this.textBoxType = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.dataGridTextBoxColumn1 = new Microarea.TaskBuilderNet.Core.StringLoader.AutoSizeTextColumnStyle();
            ((System.ComponentModel.ISupportInitialize)(this.dgDictionaryItems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryHeadDataView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDictionaryItemRows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryRowDataView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteAssemblyDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteHeadDataView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteRowDataView)).BeginInit();
            this.panelDictionary.SuspendLayout();
            this.panelSatellite.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenDictionary
            // 
            resources.ApplyResources(this.btnOpenDictionary, "btnOpenDictionary");
            this.btnOpenDictionary.Name = "btnOpenDictionary";
            this.btnOpenDictionary.Click += new System.EventHandler(this.btnOpenDictionary_Click);
            // 
            // openFileDialog
            // 
            resources.ApplyResources(this.openFileDialog, "openFileDialog");
            // 
            // dgDictionaryItems
            // 
            this.dgDictionaryItems.AllowNavigation = false;
            resources.ApplyResources(this.dgDictionaryItems, "dgDictionaryItems");
            this.dgDictionaryItems.BackColor = System.Drawing.Color.White;
            this.dgDictionaryItems.DataMember = "";
            this.dgDictionaryItems.DataSource = this.dictionaryHeadDataView;
            this.dgDictionaryItems.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgDictionaryItems.Name = "dgDictionaryItems";
            this.dgDictionaryItems.PreferredColumnWidth = 100;
            this.dgDictionaryItems.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.dataGridTableStyle2,
            this.dataGridTableDictMaster});
            // 
            // dictionaryHeadDataView
            // 
            this.dictionaryHeadDataView.AllowDelete = false;
            this.dictionaryHeadDataView.AllowEdit = false;
            this.dictionaryHeadDataView.AllowNew = false;
            this.dictionaryHeadDataView.Table = this.dictionaryDataSet.DictionaryHead;
            // 
            // dictionaryDataSet
            // 
            this.dictionaryDataSet.DataSetName = "DictionaryDataSet";
            this.dictionaryDataSet.Locale = new System.Globalization.CultureInfo("en-US");
            this.dictionaryDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // dataGridTableStyle2
            // 
            this.dataGridTableStyle2.DataGrid = this.dgDictionaryItems;
            this.dataGridTableStyle2.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.autoSizeTextColumnStyle10,
            this.autoSizeTextColumnStyle11});
            this.dataGridTableStyle2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridTableStyle2.MappingName = "SatelliteAssembly";
            resources.ApplyResources(this.dataGridTableStyle2, "dataGridTableStyle2");
            // 
            // autoSizeTextColumnStyle10
            // 
            this.autoSizeTextColumnStyle10.Format = "";
            this.autoSizeTextColumnStyle10.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle10, "autoSizeTextColumnStyle10");
            // 
            // autoSizeTextColumnStyle11
            // 
            this.autoSizeTextColumnStyle11.Format = "";
            this.autoSizeTextColumnStyle11.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle11, "autoSizeTextColumnStyle11");
            // 
            // dataGridTableDictMaster
            // 
            this.dataGridTableDictMaster.DataGrid = this.dgDictionaryItems;
            this.dataGridTableDictMaster.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.autoSizeTextColumnStyle100,
            this.autoSizeTextColumnStyle101,
            this.autoSizeTextColumnStyle102});
            this.dataGridTableDictMaster.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridTableDictMaster.MappingName = "DictionaryHead";
            resources.ApplyResources(this.dataGridTableDictMaster, "dataGridTableDictMaster");
            // 
            // autoSizeTextColumnStyle100
            // 
            this.autoSizeTextColumnStyle100.Format = "";
            this.autoSizeTextColumnStyle100.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle100, "autoSizeTextColumnStyle100");
            // 
            // autoSizeTextColumnStyle101
            // 
            this.autoSizeTextColumnStyle101.Format = "";
            this.autoSizeTextColumnStyle101.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle101, "autoSizeTextColumnStyle101");
            // 
            // autoSizeTextColumnStyle102
            // 
            this.autoSizeTextColumnStyle102.Format = "";
            this.autoSizeTextColumnStyle102.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle102, "autoSizeTextColumnStyle102");
            // 
            // dgDictionaryItemRows
            // 
            resources.ApplyResources(this.dgDictionaryItemRows, "dgDictionaryItemRows");
            this.dgDictionaryItemRows.DataMember = "";
            this.dgDictionaryItemRows.DataSource = this.dictionaryRowDataView;
            this.dgDictionaryItemRows.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgDictionaryItemRows.Name = "dgDictionaryItemRows";
            this.dgDictionaryItemRows.PreferredColumnWidth = 120;
            this.dgDictionaryItemRows.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.dataGridTableStyleDictSlave,
            this.dataGridTableStyleSattSlave});
            // 
            // dictionaryRowDataView
            // 
            this.dictionaryRowDataView.AllowDelete = false;
            this.dictionaryRowDataView.AllowEdit = false;
            this.dictionaryRowDataView.AllowNew = false;
            this.dictionaryRowDataView.Table = this.dictionaryDataSet.DictionaryRows;
            // 
            // dataGridTableStyleDictSlave
            // 
            this.dataGridTableStyleDictSlave.DataGrid = this.dgDictionaryItemRows;
            this.dataGridTableStyleDictSlave.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.autoSizeTextColumnStyle110,
            this.autoSizeTextColumnStyle111});
            this.dataGridTableStyleDictSlave.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridTableStyleDictSlave.MappingName = "DictionaryRows";
            resources.ApplyResources(this.dataGridTableStyleDictSlave, "dataGridTableStyleDictSlave");
            // 
            // autoSizeTextColumnStyle110
            // 
            this.autoSizeTextColumnStyle110.Format = "";
            this.autoSizeTextColumnStyle110.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle110, "autoSizeTextColumnStyle110");
            // 
            // autoSizeTextColumnStyle111
            // 
            this.autoSizeTextColumnStyle111.Format = "";
            this.autoSizeTextColumnStyle111.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle111, "autoSizeTextColumnStyle111");
            // 
            // dataGridTableStyleSattSlave
            // 
            this.dataGridTableStyleSattSlave.DataGrid = this.dgDictionaryItemRows;
            this.dataGridTableStyleSattSlave.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.autoSizeTextColumnStyle121,
            this.autoSizeTextColumnStyle122,
            this.autoSizeTextColumnStyle123});
            this.dataGridTableStyleSattSlave.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridTableStyleSattSlave.MappingName = "Resource";
            resources.ApplyResources(this.dataGridTableStyleSattSlave, "dataGridTableStyleSattSlave");
            // 
            // autoSizeTextColumnStyle121
            // 
            this.autoSizeTextColumnStyle121.Format = "";
            this.autoSizeTextColumnStyle121.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle121, "autoSizeTextColumnStyle121");
            // 
            // autoSizeTextColumnStyle122
            // 
            this.autoSizeTextColumnStyle122.Format = "";
            this.autoSizeTextColumnStyle122.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle122, "autoSizeTextColumnStyle122");
            // 
            // autoSizeTextColumnStyle123
            // 
            this.autoSizeTextColumnStyle123.Format = "";
            this.autoSizeTextColumnStyle123.FormatInfo = null;
            resources.ApplyResources(this.autoSizeTextColumnStyle123, "autoSizeTextColumnStyle123");
            // 
            // checkedListBoxFilter
            // 
            resources.ApplyResources(this.checkedListBoxFilter, "checkedListBoxFilter");
            this.checkedListBoxFilter.Name = "checkedListBoxFilter";
            this.checkedListBoxFilter.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxFilter_ItemCheck);
            // 
            // txtBoxId
            // 
            resources.ApplyResources(this.txtBoxId, "txtBoxId");
            this.txtBoxId.Name = "txtBoxId";
            this.txtBoxId.TextChanged += new System.EventHandler(this.txtBoxId_TextChanged);
            // 
            // txtBoxName
            // 
            resources.ApplyResources(this.txtBoxName, "txtBoxName");
            this.txtBoxName.Name = "txtBoxName";
            this.txtBoxName.TextChanged += new System.EventHandler(this.txtBoxName_TextChanged);
            // 
            // LabelId
            // 
            this.LabelId.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.LabelId, "LabelId");
            this.LabelId.Name = "LabelId";
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // satelliteAssemblyDataSet
            // 
            this.satelliteAssemblyDataSet.DataSetName = "SatelliteAssemblyDataSet";
            this.satelliteAssemblyDataSet.Locale = new System.Globalization.CultureInfo("en-US");
            this.satelliteAssemblyDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // satelliteHeadDataView
            // 
            this.satelliteHeadDataView.AllowDelete = false;
            this.satelliteHeadDataView.AllowEdit = false;
            this.satelliteHeadDataView.AllowNew = false;
            this.satelliteHeadDataView.Table = this.satelliteAssemblyDataSet.SatelliteAssembly;
            // 
            // satelliteRowDataView
            // 
            this.satelliteRowDataView.AllowDelete = false;
            this.satelliteRowDataView.AllowEdit = false;
            this.satelliteRowDataView.AllowNew = false;
            this.satelliteRowDataView.Table = this.satelliteAssemblyDataSet.Resource;
            // 
            // panelDictionary
            // 
            this.panelDictionary.BackColor = System.Drawing.SystemColors.Control;
            this.panelDictionary.Controls.Add(this.labelType);
            this.panelDictionary.Controls.Add(this.txtBoxName);
            this.panelDictionary.Controls.Add(this.labelName);
            this.panelDictionary.Controls.Add(this.txtBoxId);
            this.panelDictionary.Controls.Add(this.checkedListBoxFilter);
            this.panelDictionary.Controls.Add(this.LabelId);
            resources.ApplyResources(this.panelDictionary, "panelDictionary");
            this.panelDictionary.Name = "panelDictionary";
            // 
            // labelType
            // 
            this.labelType.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.labelType, "labelType");
            this.labelType.Name = "labelType";
            // 
            // panelSatellite
            // 
            this.panelSatellite.Controls.Add(this.textBoxType);
            this.panelSatellite.Controls.Add(this.lblType);
            resources.ApplyResources(this.panelSatellite, "panelSatellite");
            this.panelSatellite.Name = "panelSatellite";
            // 
            // textBoxType
            // 
            resources.ApplyResources(this.textBoxType, "textBoxType");
            this.textBoxType.Name = "textBoxType";
            this.textBoxType.TextChanged += new System.EventHandler(this.textBoxType_TextChanged);
            // 
            // lblType
            // 
            resources.ApplyResources(this.lblType, "lblType");
            this.lblType.Name = "lblType";
            // 
            // dataGridTextBoxColumn1
            // 
            this.dataGridTextBoxColumn1.Format = "";
            this.dataGridTextBoxColumn1.FormatInfo = null;
            resources.ApplyResources(this.dataGridTextBoxColumn1, "dataGridTextBoxColumn1");
            // 
            // DictionaryViewerControl
            // 
            this.Controls.Add(this.panelSatellite);
            this.Controls.Add(this.panelDictionary);
            this.Controls.Add(this.dgDictionaryItemRows);
            this.Controls.Add(this.dgDictionaryItems);
            this.Controls.Add(this.btnOpenDictionary);
            this.Name = "DictionaryViewerControl";
            resources.ApplyResources(this, "$this");
            this.Load += new System.EventHandler(this.DictionaryViewerControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgDictionaryItems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryHeadDataView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDictionaryItemRows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dictionaryRowDataView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteAssemblyDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteHeadDataView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.satelliteRowDataView)).EndInit();
            this.panelDictionary.ResumeLayout(false);
            this.panelDictionary.PerformLayout();
            this.panelSatellite.ResumeLayout(false);
            this.panelSatellite.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
