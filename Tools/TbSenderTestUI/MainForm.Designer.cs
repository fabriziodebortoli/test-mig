namespace TbSenderTestUI
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnAddPdf = new System.Windows.Forms.Button();
			this.btnTest = new System.Windows.Forms.Button();
			this.txtTestData = new System.Windows.Forms.TextBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabViewTable = new System.Windows.Forms.TabPage();
			this.btnUploadSingleLot = new System.Windows.Forms.Button();
			this.btnDoSomething = new System.Windows.Forms.Button();
			this.btnAllot = new System.Windows.Forms.Button();
			this.btnTick = new System.Windows.Forms.Button();
			this.btnRefreshViews = new System.Windows.Forms.Button();
			this.lblMessageLots = new System.Windows.Forms.Label();
			this.dgvLotsQueue = new System.Windows.Forms.DataGridView();
			this.lotIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.idExtDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.statusExtDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.deliveryTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.printTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.totalAmountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.printAmountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.postageAmountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.sendAfterDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.statusDescriptionExtDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.totalPagesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.faxDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseeDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addressDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.zipDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cityDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.countyDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.countryDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseeNamespaceDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseePrimaryKeyDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tBMsgLotsBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.lblMessageQueue = new System.Windows.Forms.Label();
			this.dgvMessageQueue = new System.Windows.Forms.DataGridView();
			this.msgIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.faxDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.zipDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.countyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.countryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.subjectDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docNamespaceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docPrimaryKeyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseeNamespaceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addresseePrimaryKeyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docPagesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docSizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.docImageDataGridViewImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.tBMsgQueueBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.tabSubscribe = new System.Windows.Forms.TabPage();
			this.txtToken = new System.Windows.Forms.TextBox();
			this.lblToken = new System.Windows.Forms.Label();
			this.btnCharge = new System.Windows.Forms.Button();
			this.lblPdfSubscriptionPath = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.txtPdfSubscriptionPath = new System.Windows.Forms.TextBox();
			this.txtLogin = new System.Windows.Forms.TextBox();
			this.lblLogin = new System.Windows.Forms.Label();
			this.btnSubscribe = new System.Windows.Forms.Button();
			this.subscriptionEditor = new TbSenderTestUI.SubscriptionEditor();
			this.tabAddPdf = new System.Windows.Forms.TabPage();
			this.msgEditor = new TbSenderTestUI.MessageEditor();
			this.tabTests = new System.Windows.Forms.TabPage();
			this.lotOptionsUI1 = new TbSenderTestUI.LotOptionsUI();
			this.tabParameters = new System.Windows.Forms.TabPage();
			this.cmbCurrentCompany = new System.Windows.Forms.ComboBox();
			this.lblCurrentCompany = new System.Windows.Forms.Label();
			this.tabControl.SuspendLayout();
			this.tabViewTable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvLotsQueue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tBMsgLotsBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMessageQueue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tBMsgQueueBindingSource)).BeginInit();
			this.tabSubscribe.SuspendLayout();
			this.tabAddPdf.SuspendLayout();
			this.tabTests.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(722, 611);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnAddPdf
			// 
			this.btnAddPdf.Location = new System.Drawing.Point(508, 454);
			this.btnAddPdf.Name = "btnAddPdf";
			this.btnAddPdf.Size = new System.Drawing.Size(75, 23);
			this.btnAddPdf.TabIndex = 2;
			this.btnAddPdf.Text = "Add";
			this.btnAddPdf.UseVisualStyleBackColor = true;
			this.btnAddPdf.Click += new System.EventHandler(this.btnAddPdf_Click);
			// 
			// btnTest
			// 
			this.btnTest.Location = new System.Drawing.Point(43, 59);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(75, 23);
			this.btnTest.TabIndex = 3;
			this.btnTest.Text = "Test";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// txtTestData
			// 
			this.txtTestData.Location = new System.Drawing.Point(124, 62);
			this.txtTestData.Name = "txtTestData";
			this.txtTestData.Size = new System.Drawing.Size(100, 20);
			this.txtTestData.TabIndex = 4;
			this.txtTestData.Text = "File01.pdf";
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabViewTable);
			this.tabControl.Controls.Add(this.tabSubscribe);
			this.tabControl.Controls.Add(this.tabAddPdf);
			this.tabControl.Controls.Add(this.tabTests);
			this.tabControl.Controls.Add(this.tabParameters);
			this.tabControl.Location = new System.Drawing.Point(12, 50);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(789, 546);
			this.tabControl.TabIndex = 0;
			// 
			// tabViewTable
			// 
			this.tabViewTable.Controls.Add(this.btnUploadSingleLot);
			this.tabViewTable.Controls.Add(this.btnDoSomething);
			this.tabViewTable.Controls.Add(this.btnAllot);
			this.tabViewTable.Controls.Add(this.btnTick);
			this.tabViewTable.Controls.Add(this.btnRefreshViews);
			this.tabViewTable.Controls.Add(this.lblMessageLots);
			this.tabViewTable.Controls.Add(this.dgvLotsQueue);
			this.tabViewTable.Controls.Add(this.lblMessageQueue);
			this.tabViewTable.Controls.Add(this.dgvMessageQueue);
			this.tabViewTable.Location = new System.Drawing.Point(4, 22);
			this.tabViewTable.Name = "tabViewTable";
			this.tabViewTable.Padding = new System.Windows.Forms.Padding(3);
			this.tabViewTable.Size = new System.Drawing.Size(781, 520);
			this.tabViewTable.TabIndex = 1;
			this.tabViewTable.Text = "ViewTable";
			this.tabViewTable.UseVisualStyleBackColor = true;
			// 
			// btnUploadSingleLot
			// 
			this.btnUploadSingleLot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnUploadSingleLot.Location = new System.Drawing.Point(342, 271);
			this.btnUploadSingleLot.Name = "btnUploadSingleLot";
			this.btnUploadSingleLot.Size = new System.Drawing.Size(106, 23);
			this.btnUploadSingleLot.TabIndex = 6;
			this.btnUploadSingleLot.Text = "Upload single Lot";
			this.btnUploadSingleLot.UseVisualStyleBackColor = true;
			this.btnUploadSingleLot.Click += new System.EventHandler(this.btnUploadSingleLot_Click);
			// 
			// btnDoSomething
			// 
			this.btnDoSomething.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnDoSomething.Location = new System.Drawing.Point(529, 271);
			this.btnDoSomething.Name = "btnDoSomething";
			this.btnDoSomething.Size = new System.Drawing.Size(106, 23);
			this.btnDoSomething.TabIndex = 7;
			this.btnDoSomething.Text = "Do something";
			this.btnDoSomething.UseVisualStyleBackColor = true;
			this.btnDoSomething.Click += new System.EventHandler(this.btnDoSomething_Click);
			// 
			// btnAllot
			// 
			this.btnAllot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAllot.Location = new System.Drawing.Point(220, 271);
			this.btnAllot.Name = "btnAllot";
			this.btnAllot.Size = new System.Drawing.Size(106, 23);
			this.btnAllot.TabIndex = 3;
			this.btnAllot.Text = "Allot Messages";
			this.btnAllot.UseVisualStyleBackColor = true;
			this.btnAllot.Click += new System.EventHandler(this.btnAllot_Click);
			// 
			// btnTick
			// 
			this.btnTick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnTick.Location = new System.Drawing.Point(682, 271);
			this.btnTick.Name = "btnTick";
			this.btnTick.Size = new System.Drawing.Size(75, 23);
			this.btnTick.TabIndex = 8;
			this.btnTick.Text = "Tick";
			this.btnTick.UseVisualStyleBackColor = true;
			this.btnTick.Click += new System.EventHandler(this.btnTick_Click);
			// 
			// btnRefreshViews
			// 
			this.btnRefreshViews.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefreshViews.Location = new System.Drawing.Point(99, 271);
			this.btnRefreshViews.Name = "btnRefreshViews";
			this.btnRefreshViews.Size = new System.Drawing.Size(106, 23);
			this.btnRefreshViews.TabIndex = 2;
			this.btnRefreshViews.Text = "Refresh Views";
			this.btnRefreshViews.UseVisualStyleBackColor = true;
			this.btnRefreshViews.Click += new System.EventHandler(this.btnRefreshViews_Click);
			// 
			// lblMessageLots
			// 
			this.lblMessageLots.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblMessageLots.AutoSize = true;
			this.lblMessageLots.Location = new System.Drawing.Point(18, 286);
			this.lblMessageLots.Name = "lblMessageLots";
			this.lblMessageLots.Size = new System.Drawing.Size(63, 13);
			this.lblMessageLots.TabIndex = 4;
			this.lblMessageLots.Text = "Lots queue:";
			// 
			// dgvLotsQueue
			// 
			this.dgvLotsQueue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvLotsQueue.AutoGenerateColumns = false;
			this.dgvLotsQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvLotsQueue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.lotIDDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn,
            this.idExtDataGridViewTextBoxColumn,
            this.statusExtDataGridViewTextBoxColumn,
            this.deliveryTypeDataGridViewTextBoxColumn,
            this.printTypeDataGridViewTextBoxColumn,
            this.totalAmountDataGridViewTextBoxColumn,
            this.printAmountDataGridViewTextBoxColumn,
            this.postageAmountDataGridViewTextBoxColumn,
            this.sendAfterDataGridViewTextBoxColumn,
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.descriptionDataGridViewTextBoxColumn,
            this.statusDescriptionExtDataGridViewTextBoxColumn,
            this.totalPagesDataGridViewTextBoxColumn,
            this.faxDataGridViewTextBoxColumn1,
            this.addresseeDataGridViewTextBoxColumn1,
            this.addressDataGridViewTextBoxColumn1,
            this.zipDataGridViewTextBoxColumn1,
            this.cityDataGridViewTextBoxColumn1,
            this.countyDataGridViewTextBoxColumn1,
            this.countryDataGridViewTextBoxColumn1,
            this.addresseeNamespaceDataGridViewTextBoxColumn1,
            this.addresseePrimaryKeyDataGridViewTextBoxColumn1});
			this.dgvLotsQueue.DataSource = this.tBMsgLotsBindingSource;
			this.dgvLotsQueue.Location = new System.Drawing.Point(18, 305);
			this.dgvLotsQueue.Name = "dgvLotsQueue";
			this.dgvLotsQueue.ReadOnly = true;
			this.dgvLotsQueue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvLotsQueue.Size = new System.Drawing.Size(739, 209);
			this.dgvLotsQueue.TabIndex = 5;
			// 
			// lotIDDataGridViewTextBoxColumn
			// 
			this.lotIDDataGridViewTextBoxColumn.DataPropertyName = "LotID";
			this.lotIDDataGridViewTextBoxColumn.HeaderText = "LotID";
			this.lotIDDataGridViewTextBoxColumn.Name = "lotIDDataGridViewTextBoxColumn";
			this.lotIDDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// statusDataGridViewTextBoxColumn
			// 
			this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
			this.statusDataGridViewTextBoxColumn.HeaderText = "Status";
			this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
			this.statusDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// idExtDataGridViewTextBoxColumn
			// 
			this.idExtDataGridViewTextBoxColumn.DataPropertyName = "IdExt";
			this.idExtDataGridViewTextBoxColumn.HeaderText = "IdExt";
			this.idExtDataGridViewTextBoxColumn.Name = "idExtDataGridViewTextBoxColumn";
			this.idExtDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// statusExtDataGridViewTextBoxColumn
			// 
			this.statusExtDataGridViewTextBoxColumn.DataPropertyName = "StatusExt";
			this.statusExtDataGridViewTextBoxColumn.HeaderText = "StatusExt";
			this.statusExtDataGridViewTextBoxColumn.Name = "statusExtDataGridViewTextBoxColumn";
			this.statusExtDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// deliveryTypeDataGridViewTextBoxColumn
			// 
			this.deliveryTypeDataGridViewTextBoxColumn.DataPropertyName = "DeliveryType";
			this.deliveryTypeDataGridViewTextBoxColumn.HeaderText = "DeliveryType";
			this.deliveryTypeDataGridViewTextBoxColumn.Name = "deliveryTypeDataGridViewTextBoxColumn";
			this.deliveryTypeDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// printTypeDataGridViewTextBoxColumn
			// 
			this.printTypeDataGridViewTextBoxColumn.DataPropertyName = "PrintType";
			this.printTypeDataGridViewTextBoxColumn.HeaderText = "PrintType";
			this.printTypeDataGridViewTextBoxColumn.Name = "printTypeDataGridViewTextBoxColumn";
			this.printTypeDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// totalAmountDataGridViewTextBoxColumn
			// 
			this.totalAmountDataGridViewTextBoxColumn.DataPropertyName = "TotalAmount";
			this.totalAmountDataGridViewTextBoxColumn.HeaderText = "TotalAmount";
			this.totalAmountDataGridViewTextBoxColumn.Name = "totalAmountDataGridViewTextBoxColumn";
			this.totalAmountDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// printAmountDataGridViewTextBoxColumn
			// 
			this.printAmountDataGridViewTextBoxColumn.DataPropertyName = "PrintAmount";
			this.printAmountDataGridViewTextBoxColumn.HeaderText = "PrintAmount";
			this.printAmountDataGridViewTextBoxColumn.Name = "printAmountDataGridViewTextBoxColumn";
			this.printAmountDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// postageAmountDataGridViewTextBoxColumn
			// 
			this.postageAmountDataGridViewTextBoxColumn.DataPropertyName = "PostageAmount";
			this.postageAmountDataGridViewTextBoxColumn.HeaderText = "PostageAmount";
			this.postageAmountDataGridViewTextBoxColumn.Name = "postageAmountDataGridViewTextBoxColumn";
			this.postageAmountDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// sendAfterDataGridViewTextBoxColumn
			// 
			this.sendAfterDataGridViewTextBoxColumn.DataPropertyName = "SendAfter";
			this.sendAfterDataGridViewTextBoxColumn.HeaderText = "SendAfter";
			this.sendAfterDataGridViewTextBoxColumn.Name = "sendAfterDataGridViewTextBoxColumn";
			this.sendAfterDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.DataPropertyName = "TBCreated";
			this.dataGridViewTextBoxColumn1.HeaderText = "TBCreated";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.DataPropertyName = "TBModified";
			this.dataGridViewTextBoxColumn2.HeaderText = "TBModified";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.DataPropertyName = "TBCreatedID";
			this.dataGridViewTextBoxColumn3.HeaderText = "TBCreatedID";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.DataPropertyName = "TBModifiedID";
			this.dataGridViewTextBoxColumn4.HeaderText = "TBModifiedID";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// descriptionDataGridViewTextBoxColumn
			// 
			this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
			this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
			this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
			this.descriptionDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// statusDescriptionExtDataGridViewTextBoxColumn
			// 
			this.statusDescriptionExtDataGridViewTextBoxColumn.DataPropertyName = "StatusDescriptionExt";
			this.statusDescriptionExtDataGridViewTextBoxColumn.HeaderText = "StatusDescriptionExt";
			this.statusDescriptionExtDataGridViewTextBoxColumn.Name = "statusDescriptionExtDataGridViewTextBoxColumn";
			this.statusDescriptionExtDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// totalPagesDataGridViewTextBoxColumn
			// 
			this.totalPagesDataGridViewTextBoxColumn.DataPropertyName = "TotalPages";
			this.totalPagesDataGridViewTextBoxColumn.HeaderText = "TotalPages";
			this.totalPagesDataGridViewTextBoxColumn.Name = "totalPagesDataGridViewTextBoxColumn";
			this.totalPagesDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// faxDataGridViewTextBoxColumn1
			// 
			this.faxDataGridViewTextBoxColumn1.DataPropertyName = "Fax";
			this.faxDataGridViewTextBoxColumn1.HeaderText = "Fax";
			this.faxDataGridViewTextBoxColumn1.Name = "faxDataGridViewTextBoxColumn1";
			this.faxDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// addresseeDataGridViewTextBoxColumn1
			// 
			this.addresseeDataGridViewTextBoxColumn1.DataPropertyName = "Addressee";
			this.addresseeDataGridViewTextBoxColumn1.HeaderText = "Addressee";
			this.addresseeDataGridViewTextBoxColumn1.Name = "addresseeDataGridViewTextBoxColumn1";
			this.addresseeDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// addressDataGridViewTextBoxColumn1
			// 
			this.addressDataGridViewTextBoxColumn1.DataPropertyName = "Address";
			this.addressDataGridViewTextBoxColumn1.HeaderText = "Address";
			this.addressDataGridViewTextBoxColumn1.Name = "addressDataGridViewTextBoxColumn1";
			this.addressDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// zipDataGridViewTextBoxColumn1
			// 
			this.zipDataGridViewTextBoxColumn1.DataPropertyName = "Zip";
			this.zipDataGridViewTextBoxColumn1.HeaderText = "Zip";
			this.zipDataGridViewTextBoxColumn1.Name = "zipDataGridViewTextBoxColumn1";
			this.zipDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// cityDataGridViewTextBoxColumn1
			// 
			this.cityDataGridViewTextBoxColumn1.DataPropertyName = "City";
			this.cityDataGridViewTextBoxColumn1.HeaderText = "City";
			this.cityDataGridViewTextBoxColumn1.Name = "cityDataGridViewTextBoxColumn1";
			this.cityDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// countyDataGridViewTextBoxColumn1
			// 
			this.countyDataGridViewTextBoxColumn1.DataPropertyName = "County";
			this.countyDataGridViewTextBoxColumn1.HeaderText = "County";
			this.countyDataGridViewTextBoxColumn1.Name = "countyDataGridViewTextBoxColumn1";
			this.countyDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// countryDataGridViewTextBoxColumn1
			// 
			this.countryDataGridViewTextBoxColumn1.DataPropertyName = "Country";
			this.countryDataGridViewTextBoxColumn1.HeaderText = "Country";
			this.countryDataGridViewTextBoxColumn1.Name = "countryDataGridViewTextBoxColumn1";
			this.countryDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// addresseeNamespaceDataGridViewTextBoxColumn1
			// 
			this.addresseeNamespaceDataGridViewTextBoxColumn1.DataPropertyName = "AddresseeNamespace";
			this.addresseeNamespaceDataGridViewTextBoxColumn1.HeaderText = "AddresseeNamespace";
			this.addresseeNamespaceDataGridViewTextBoxColumn1.Name = "addresseeNamespaceDataGridViewTextBoxColumn1";
			this.addresseeNamespaceDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// addresseePrimaryKeyDataGridViewTextBoxColumn1
			// 
			this.addresseePrimaryKeyDataGridViewTextBoxColumn1.DataPropertyName = "AddresseePrimaryKey";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn1.HeaderText = "AddresseePrimaryKey";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn1.Name = "addresseePrimaryKeyDataGridViewTextBoxColumn1";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// tBMsgLotsBindingSource
			// 
			this.tBMsgLotsBindingSource.DataSource = typeof(Microarea.TaskBuilderNet.TbSenderBL.TB_MsgLots);
			// 
			// lblMessageQueue
			// 
			this.lblMessageQueue.AutoSize = true;
			this.lblMessageQueue.Location = new System.Drawing.Point(18, 20);
			this.lblMessageQueue.Name = "lblMessageQueue";
			this.lblMessageQueue.Size = new System.Drawing.Size(91, 13);
			this.lblMessageQueue.TabIndex = 0;
			this.lblMessageQueue.Text = "Messages queue:";
			// 
			// dgvMessageQueue
			// 
			this.dgvMessageQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvMessageQueue.AutoGenerateColumns = false;
			this.dgvMessageQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMessageQueue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.msgIDDataGridViewTextBoxColumn,
            this.faxDataGridViewTextBoxColumn,
            this.addresseeDataGridViewTextBoxColumn,
            this.addressDataGridViewTextBoxColumn,
            this.zipDataGridViewTextBoxColumn,
            this.cityDataGridViewTextBoxColumn,
            this.countyDataGridViewTextBoxColumn,
            this.countryDataGridViewTextBoxColumn,
            this.subjectDataGridViewTextBoxColumn,
            this.docNamespaceDataGridViewTextBoxColumn,
            this.docPrimaryKeyDataGridViewTextBoxColumn,
            this.addresseeNamespaceDataGridViewTextBoxColumn,
            this.addresseePrimaryKeyDataGridViewTextBoxColumn,
            this.docFileNameDataGridViewTextBoxColumn,
            this.docPagesDataGridViewTextBoxColumn,
            this.docSizeDataGridViewTextBoxColumn,
            this.docImageDataGridViewImageColumn});
			this.dgvMessageQueue.DataSource = this.tBMsgQueueBindingSource;
			this.dgvMessageQueue.Location = new System.Drawing.Point(18, 39);
			this.dgvMessageQueue.Name = "dgvMessageQueue";
			this.dgvMessageQueue.ReadOnly = true;
			this.dgvMessageQueue.Size = new System.Drawing.Size(739, 231);
			this.dgvMessageQueue.TabIndex = 1;
			// 
			// msgIDDataGridViewTextBoxColumn
			// 
			this.msgIDDataGridViewTextBoxColumn.DataPropertyName = "MsgID";
			this.msgIDDataGridViewTextBoxColumn.HeaderText = "MsgID";
			this.msgIDDataGridViewTextBoxColumn.Name = "msgIDDataGridViewTextBoxColumn";
			this.msgIDDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// faxDataGridViewTextBoxColumn
			// 
			this.faxDataGridViewTextBoxColumn.DataPropertyName = "Fax";
			this.faxDataGridViewTextBoxColumn.HeaderText = "Fax";
			this.faxDataGridViewTextBoxColumn.Name = "faxDataGridViewTextBoxColumn";
			this.faxDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// addresseeDataGridViewTextBoxColumn
			// 
			this.addresseeDataGridViewTextBoxColumn.DataPropertyName = "Addressee";
			this.addresseeDataGridViewTextBoxColumn.HeaderText = "Addressee";
			this.addresseeDataGridViewTextBoxColumn.Name = "addresseeDataGridViewTextBoxColumn";
			this.addresseeDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// addressDataGridViewTextBoxColumn
			// 
			this.addressDataGridViewTextBoxColumn.DataPropertyName = "Address";
			this.addressDataGridViewTextBoxColumn.HeaderText = "Address";
			this.addressDataGridViewTextBoxColumn.Name = "addressDataGridViewTextBoxColumn";
			this.addressDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// zipDataGridViewTextBoxColumn
			// 
			this.zipDataGridViewTextBoxColumn.DataPropertyName = "Zip";
			this.zipDataGridViewTextBoxColumn.HeaderText = "Zip";
			this.zipDataGridViewTextBoxColumn.Name = "zipDataGridViewTextBoxColumn";
			this.zipDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// cityDataGridViewTextBoxColumn
			// 
			this.cityDataGridViewTextBoxColumn.DataPropertyName = "City";
			this.cityDataGridViewTextBoxColumn.HeaderText = "City";
			this.cityDataGridViewTextBoxColumn.Name = "cityDataGridViewTextBoxColumn";
			this.cityDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// countyDataGridViewTextBoxColumn
			// 
			this.countyDataGridViewTextBoxColumn.DataPropertyName = "County";
			this.countyDataGridViewTextBoxColumn.HeaderText = "County";
			this.countyDataGridViewTextBoxColumn.Name = "countyDataGridViewTextBoxColumn";
			this.countyDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// countryDataGridViewTextBoxColumn
			// 
			this.countryDataGridViewTextBoxColumn.DataPropertyName = "Country";
			this.countryDataGridViewTextBoxColumn.HeaderText = "Country";
			this.countryDataGridViewTextBoxColumn.Name = "countryDataGridViewTextBoxColumn";
			this.countryDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// subjectDataGridViewTextBoxColumn
			// 
			this.subjectDataGridViewTextBoxColumn.DataPropertyName = "Subject";
			this.subjectDataGridViewTextBoxColumn.HeaderText = "Subject";
			this.subjectDataGridViewTextBoxColumn.Name = "subjectDataGridViewTextBoxColumn";
			this.subjectDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docNamespaceDataGridViewTextBoxColumn
			// 
			this.docNamespaceDataGridViewTextBoxColumn.DataPropertyName = "DocNamespace";
			this.docNamespaceDataGridViewTextBoxColumn.HeaderText = "DocNamespace";
			this.docNamespaceDataGridViewTextBoxColumn.Name = "docNamespaceDataGridViewTextBoxColumn";
			this.docNamespaceDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docPrimaryKeyDataGridViewTextBoxColumn
			// 
			this.docPrimaryKeyDataGridViewTextBoxColumn.DataPropertyName = "DocPrimaryKey";
			this.docPrimaryKeyDataGridViewTextBoxColumn.HeaderText = "DocPrimaryKey";
			this.docPrimaryKeyDataGridViewTextBoxColumn.Name = "docPrimaryKeyDataGridViewTextBoxColumn";
			this.docPrimaryKeyDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// addresseeNamespaceDataGridViewTextBoxColumn
			// 
			this.addresseeNamespaceDataGridViewTextBoxColumn.DataPropertyName = "AddresseeNamespace";
			this.addresseeNamespaceDataGridViewTextBoxColumn.HeaderText = "AddresseeNamespace";
			this.addresseeNamespaceDataGridViewTextBoxColumn.Name = "addresseeNamespaceDataGridViewTextBoxColumn";
			this.addresseeNamespaceDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// addresseePrimaryKeyDataGridViewTextBoxColumn
			// 
			this.addresseePrimaryKeyDataGridViewTextBoxColumn.DataPropertyName = "AddresseePrimaryKey";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn.HeaderText = "AddresseePrimaryKey";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn.Name = "addresseePrimaryKeyDataGridViewTextBoxColumn";
			this.addresseePrimaryKeyDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docFileNameDataGridViewTextBoxColumn
			// 
			this.docFileNameDataGridViewTextBoxColumn.DataPropertyName = "DocFileName";
			this.docFileNameDataGridViewTextBoxColumn.HeaderText = "DocFileName";
			this.docFileNameDataGridViewTextBoxColumn.Name = "docFileNameDataGridViewTextBoxColumn";
			this.docFileNameDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docPagesDataGridViewTextBoxColumn
			// 
			this.docPagesDataGridViewTextBoxColumn.DataPropertyName = "DocPages";
			this.docPagesDataGridViewTextBoxColumn.HeaderText = "DocPages";
			this.docPagesDataGridViewTextBoxColumn.Name = "docPagesDataGridViewTextBoxColumn";
			this.docPagesDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docSizeDataGridViewTextBoxColumn
			// 
			this.docSizeDataGridViewTextBoxColumn.DataPropertyName = "DocSize";
			this.docSizeDataGridViewTextBoxColumn.HeaderText = "DocSize";
			this.docSizeDataGridViewTextBoxColumn.Name = "docSizeDataGridViewTextBoxColumn";
			this.docSizeDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// docImageDataGridViewImageColumn
			// 
			this.docImageDataGridViewImageColumn.DataPropertyName = "DocImage";
			this.docImageDataGridViewImageColumn.HeaderText = "DocImage";
			this.docImageDataGridViewImageColumn.Name = "docImageDataGridViewImageColumn";
			this.docImageDataGridViewImageColumn.ReadOnly = true;
			this.docImageDataGridViewImageColumn.Visible = false;
			// 
			// tBMsgQueueBindingSource
			// 
			this.tBMsgQueueBindingSource.DataSource = typeof(Microarea.TaskBuilderNet.TbSenderBL.TB_MsgQueue);
			// 
			// tabSubscribe
			// 
			this.tabSubscribe.Controls.Add(this.txtToken);
			this.tabSubscribe.Controls.Add(this.lblToken);
			this.tabSubscribe.Controls.Add(this.btnCharge);
			this.tabSubscribe.Controls.Add(this.lblPdfSubscriptionPath);
			this.tabSubscribe.Controls.Add(this.btnBrowse);
			this.tabSubscribe.Controls.Add(this.txtPdfSubscriptionPath);
			this.tabSubscribe.Controls.Add(this.txtLogin);
			this.tabSubscribe.Controls.Add(this.lblLogin);
			this.tabSubscribe.Controls.Add(this.btnSubscribe);
			this.tabSubscribe.Controls.Add(this.subscriptionEditor);
			this.tabSubscribe.Location = new System.Drawing.Point(4, 22);
			this.tabSubscribe.Name = "tabSubscribe";
			this.tabSubscribe.Size = new System.Drawing.Size(781, 520);
			this.tabSubscribe.TabIndex = 3;
			this.tabSubscribe.Text = "Subscribe";
			this.tabSubscribe.UseVisualStyleBackColor = true;
			// 
			// txtToken
			// 
			this.txtToken.Location = new System.Drawing.Point(226, 410);
			this.txtToken.Name = "txtToken";
			this.txtToken.ReadOnly = true;
			this.txtToken.Size = new System.Drawing.Size(242, 20);
			this.txtToken.TabIndex = 9;
			// 
			// lblToken
			// 
			this.lblToken.AutoSize = true;
			this.lblToken.Location = new System.Drawing.Point(184, 413);
			this.lblToken.Name = "lblToken";
			this.lblToken.Size = new System.Drawing.Size(41, 13);
			this.lblToken.TabIndex = 8;
			this.lblToken.Text = "Token:";
			// 
			// btnCharge
			// 
			this.btnCharge.Location = new System.Drawing.Point(539, 469);
			this.btnCharge.Name = "btnCharge";
			this.btnCharge.Size = new System.Drawing.Size(75, 23);
			this.btnCharge.TabIndex = 7;
			this.btnCharge.Text = "Charge";
			this.btnCharge.UseVisualStyleBackColor = true;
			this.btnCharge.Click += new System.EventHandler(this.btnCharge_Click);
			// 
			// lblPdfSubscriptionPath
			// 
			this.lblPdfSubscriptionPath.AutoSize = true;
			this.lblPdfSubscriptionPath.Location = new System.Drawing.Point(22, 453);
			this.lblPdfSubscriptionPath.Name = "lblPdfSubscriptionPath";
			this.lblPdfSubscriptionPath.Size = new System.Drawing.Size(117, 13);
			this.lblPdfSubscriptionPath.TabIndex = 6;
			this.lblPdfSubscriptionPath.Text = "PDF Subscription Path:";
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(447, 469);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.btnBrowse.TabIndex = 5;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// txtPdfSubscriptionPath
			// 
			this.txtPdfSubscriptionPath.Location = new System.Drawing.Point(22, 472);
			this.txtPdfSubscriptionPath.Name = "txtPdfSubscriptionPath";
			this.txtPdfSubscriptionPath.Size = new System.Drawing.Size(409, 20);
			this.txtPdfSubscriptionPath.TabIndex = 4;
			// 
			// txtLogin
			// 
			this.txtLogin.Location = new System.Drawing.Point(226, 384);
			this.txtLogin.Name = "txtLogin";
			this.txtLogin.ReadOnly = true;
			this.txtLogin.Size = new System.Drawing.Size(242, 20);
			this.txtLogin.TabIndex = 3;
			// 
			// lblLogin
			// 
			this.lblLogin.AutoSize = true;
			this.lblLogin.Location = new System.Drawing.Point(184, 387);
			this.lblLogin.Name = "lblLogin";
			this.lblLogin.Size = new System.Drawing.Size(36, 13);
			this.lblLogin.TabIndex = 2;
			this.lblLogin.Text = "Login:";
			// 
			// btnSubscribe
			// 
			this.btnSubscribe.Location = new System.Drawing.Point(80, 382);
			this.btnSubscribe.Name = "btnSubscribe";
			this.btnSubscribe.Size = new System.Drawing.Size(75, 23);
			this.btnSubscribe.TabIndex = 1;
			this.btnSubscribe.Text = "Subscribe";
			this.btnSubscribe.UseVisualStyleBackColor = true;
			this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
			// 
			// subscriptionEditor
			// 
			this.subscriptionEditor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.subscriptionEditor.Location = new System.Drawing.Point(22, 18);
			this.subscriptionEditor.Name = "subscriptionEditor";
			this.subscriptionEditor.Size = new System.Drawing.Size(446, 358);
			this.subscriptionEditor.SubscriberInfo = ((Microarea.TaskBuilderNet.TbSenderBL.PostaLite.ISubscriberInfo)(resources.GetObject("subscriptionEditor.SubscriberInfo")));
			this.subscriptionEditor.TabIndex = 0;
			// 
			// tabAddPdf
			// 
			this.tabAddPdf.Controls.Add(this.msgEditor);
			this.tabAddPdf.Controls.Add(this.btnAddPdf);
			this.tabAddPdf.Location = new System.Drawing.Point(4, 22);
			this.tabAddPdf.Name = "tabAddPdf";
			this.tabAddPdf.Padding = new System.Windows.Forms.Padding(3);
			this.tabAddPdf.Size = new System.Drawing.Size(781, 520);
			this.tabAddPdf.TabIndex = 0;
			this.tabAddPdf.Text = "Add PDF";
			this.tabAddPdf.UseVisualStyleBackColor = true;
			// 
			// msgEditor
			// 
			this.msgEditor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.msgEditor.Location = new System.Drawing.Point(6, 6);
			this.msgEditor.Name = "msgEditor";
			this.msgEditor.Size = new System.Drawing.Size(469, 487);
			this.msgEditor.TabIndex = 0;
			// 
			// tabTests
			// 
			this.tabTests.Controls.Add(this.lotOptionsUI1);
			this.tabTests.Controls.Add(this.txtTestData);
			this.tabTests.Controls.Add(this.btnTest);
			this.tabTests.Location = new System.Drawing.Point(4, 22);
			this.tabTests.Name = "tabTests";
			this.tabTests.Size = new System.Drawing.Size(781, 520);
			this.tabTests.TabIndex = 2;
			this.tabTests.Text = "Tests";
			this.tabTests.UseVisualStyleBackColor = true;
			// 
			// lotOptionsUI1
			// 
			this.lotOptionsUI1.Location = new System.Drawing.Point(384, 32);
			this.lotOptionsUI1.Name = "lotOptionsUI1";
			this.lotOptionsUI1.Size = new System.Drawing.Size(244, 279);
			this.lotOptionsUI1.TabIndex = 5;
			// 
			// tabParameters
			// 
			this.tabParameters.Location = new System.Drawing.Point(4, 22);
			this.tabParameters.Name = "tabParameters";
			this.tabParameters.Size = new System.Drawing.Size(781, 520);
			this.tabParameters.TabIndex = 4;
			this.tabParameters.Text = "Parameters";
			this.tabParameters.UseVisualStyleBackColor = true;
			// 
			// cmbCurrentCompany
			// 
			this.cmbCurrentCompany.FormattingEnabled = true;
			this.cmbCurrentCompany.Location = new System.Drawing.Point(12, 23);
			this.cmbCurrentCompany.Name = "cmbCurrentCompany";
			this.cmbCurrentCompany.Size = new System.Drawing.Size(233, 21);
			this.cmbCurrentCompany.TabIndex = 2;
			// 
			// lblCurrentCompany
			// 
			this.lblCurrentCompany.AutoSize = true;
			this.lblCurrentCompany.Location = new System.Drawing.Point(12, 6);
			this.lblCurrentCompany.Name = "lblCurrentCompany";
			this.lblCurrentCompany.Size = new System.Drawing.Size(91, 13);
			this.lblCurrentCompany.TabIndex = 3;
			this.lblCurrentCompany.Text = "Current Company:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(813, 646);
			this.Controls.Add(this.lblCurrentCompany);
			this.Controls.Add(this.cmbCurrentCompany);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.btnCancel);
			this.Name = "MainForm";
			this.Text = "PostaLite integration tester";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.tabControl.ResumeLayout(false);
			this.tabViewTable.ResumeLayout(false);
			this.tabViewTable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvLotsQueue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tBMsgLotsBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvMessageQueue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tBMsgQueueBindingSource)).EndInit();
			this.tabSubscribe.ResumeLayout(false);
			this.tabSubscribe.PerformLayout();
			this.tabAddPdf.ResumeLayout(false);
			this.tabTests.ResumeLayout(false);
			this.tabTests.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MessageEditor msgEditor;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnAddPdf;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.TextBox txtTestData;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabAddPdf;
		private System.Windows.Forms.TabPage tabViewTable;
		private System.Windows.Forms.DataGridView dgvMessageQueue;
		private System.Windows.Forms.TabPage tabTests;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBCreatedDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBModifiedDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBCreatedIDDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBModifiedIDDataGridViewTextBoxColumn;
		private System.Windows.Forms.Label lblMessageLots;
		private System.Windows.Forms.DataGridView dgvLotsQueue;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;
		private System.Windows.Forms.Label lblMessageQueue;
		private System.Windows.Forms.Button btnRefreshViews;
		private System.Windows.Forms.Button btnTick;
		private System.Windows.Forms.DataGridViewTextBoxColumn descriptionExtDataGridViewTextBoxColumn;
		private System.Windows.Forms.BindingSource tBMsgQueueBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn msgIDDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBMsgLotsDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn faxDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseeDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addressDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn zipDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn cityDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn countyDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn countryDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn subjectDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn docNamespaceDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn docPrimaryKeyDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseeNamespaceDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseePrimaryKeyDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn docFileNameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn docPagesDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn docSizeDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewImageColumn docImageDataGridViewImageColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn lotIDDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn idExtDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn statusExtDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn deliveryTypeDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn printTypeDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn totalAmountDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn printAmountDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn postageAmountDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn sendAfterDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn statusDescriptionExtDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn totalPagesDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn faxDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseeDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn addressDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn zipDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn cityDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn countyDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn countryDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseeNamespaceDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn addresseePrimaryKeyDataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn tBMsgQueueDataGridViewTextBoxColumn;
		private System.Windows.Forms.BindingSource tBMsgLotsBindingSource;
		private System.Windows.Forms.Button btnAllot;
		private System.Windows.Forms.Button btnDoSomething;
		private System.Windows.Forms.Button btnUploadSingleLot;
		private LotOptionsUI lotOptionsUI1;
		private System.Windows.Forms.TabPage tabSubscribe;
		private System.Windows.Forms.Button btnSubscribe;
		private SubscriptionEditor subscriptionEditor;
		private System.Windows.Forms.TextBox txtLogin;
		private System.Windows.Forms.Label lblLogin;
		private System.Windows.Forms.TextBox txtPdfSubscriptionPath;
		private System.Windows.Forms.Button btnCharge;
		private System.Windows.Forms.Label lblPdfSubscriptionPath;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.ComboBox cmbCurrentCompany;
		private System.Windows.Forms.Label lblCurrentCompany;
		private System.Windows.Forms.TabPage tabParameters;
		private System.Windows.Forms.TextBox txtToken;
		private System.Windows.Forms.Label lblToken;
	}
}

