
namespace Microarea.Console.Plugin.AuditingAdmin
{
    partial class AuditingQuery
    {
        private System.Windows.Forms.Label tableLabel;
        private System.Windows.Forms.GroupBox rowsGroupBox;
        private System.Windows.Forms.CheckBox chkChangeKey;
        private System.Windows.Forms.CheckBox chkDelete;
        private System.Windows.Forms.CheckBox chkUpdate;
        private System.Windows.Forms.CheckBox chkInsert;
        private System.Windows.Forms.GroupBox userGroupBox;
        private System.Windows.Forms.ComboBox cmbUsers;
        private System.Windows.Forms.CheckBox cbAllUsers;
        private System.Windows.Forms.GroupBox dateGroupBox;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Button btnRunQuery;
        private System.Windows.Forms.ComboBox comboBoxTables;
        private System.Windows.Forms.Panel panelData;
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.DataGrid dtgQueryResult;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ToolTip LanguageToolTip;
        private System.ComponentModel.IContainer components;

        //---------------------------------------------------------------------
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuditingQuery));
			this.panelData = new System.Windows.Forms.Panel();
			this.dtgQueryResult = new System.Windows.Forms.DataGrid();
			this.panelFilter = new System.Windows.Forms.Panel();
			this.btnDelete = new System.Windows.Forms.Button();
			this.tableLabel = new System.Windows.Forms.Label();
			this.rowsGroupBox = new System.Windows.Forms.GroupBox();
			this.chkChangeKey = new System.Windows.Forms.CheckBox();
			this.chkDelete = new System.Windows.Forms.CheckBox();
			this.chkUpdate = new System.Windows.Forms.CheckBox();
			this.chkInsert = new System.Windows.Forms.CheckBox();
			this.userGroupBox = new System.Windows.Forms.GroupBox();
			this.cmbUsers = new System.Windows.Forms.ComboBox();
			this.cbAllUsers = new System.Windows.Forms.CheckBox();
			this.dateGroupBox = new System.Windows.Forms.GroupBox();
			this.dtpFrom = new System.Windows.Forms.DateTimePicker();
			this.dtpTo = new System.Windows.Forms.DateTimePicker();
			this.lblFrom = new System.Windows.Forms.Label();
			this.lblTo = new System.Windows.Forms.Label();
			this.btnRunQuery = new System.Windows.Forms.Button();
			this.comboBoxTables = new System.Windows.Forms.ComboBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.LanguageToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panelData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dtgQueryResult)).BeginInit();
			this.panelFilter.SuspendLayout();
			this.rowsGroupBox.SuspendLayout();
			this.userGroupBox.SuspendLayout();
			this.dateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelData
			// 
			this.panelData.AllowDrop = true;
			resources.ApplyResources(this.panelData, "panelData");
			this.panelData.Controls.Add(this.dtgQueryResult);
			this.panelData.Name = "panelData";
			// 
			// dtgQueryResult
			// 
			this.dtgQueryResult.AlternatingBackColor = System.Drawing.Color.GhostWhite;
			this.dtgQueryResult.BackColor = System.Drawing.Color.GhostWhite;
			this.dtgQueryResult.BackgroundColor = System.Drawing.Color.Lavender;
			this.dtgQueryResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dtgQueryResult.CaptionBackColor = System.Drawing.Color.RoyalBlue;
			resources.ApplyResources(this.dtgQueryResult, "dtgQueryResult");
			this.dtgQueryResult.CaptionForeColor = System.Drawing.Color.White;
			this.dtgQueryResult.DataMember = "";
			this.dtgQueryResult.FlatMode = true;
			this.dtgQueryResult.ForeColor = System.Drawing.Color.MidnightBlue;
			this.dtgQueryResult.GridLineColor = System.Drawing.Color.RoyalBlue;
			this.dtgQueryResult.HeaderBackColor = System.Drawing.Color.MidnightBlue;
			this.dtgQueryResult.HeaderFont = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dtgQueryResult.HeaderForeColor = System.Drawing.Color.Lavender;
			this.dtgQueryResult.LinkColor = System.Drawing.Color.Teal;
			this.dtgQueryResult.Name = "dtgQueryResult";
			this.dtgQueryResult.ParentRowsBackColor = System.Drawing.Color.Lavender;
			this.dtgQueryResult.ParentRowsForeColor = System.Drawing.Color.MidnightBlue;
			this.dtgQueryResult.SelectionBackColor = System.Drawing.Color.Teal;
			this.dtgQueryResult.SelectionForeColor = System.Drawing.Color.PaleGreen;
			// 
			// panelFilter
			// 
			resources.ApplyResources(this.panelFilter, "panelFilter");
			this.panelFilter.Controls.Add(this.btnDelete);
			this.panelFilter.Controls.Add(this.tableLabel);
			this.panelFilter.Controls.Add(this.rowsGroupBox);
			this.panelFilter.Controls.Add(this.userGroupBox);
			this.panelFilter.Controls.Add(this.dateGroupBox);
			this.panelFilter.Controls.Add(this.btnRunQuery);
			this.panelFilter.Controls.Add(this.comboBoxTables);
			this.panelFilter.Name = "panelFilter";
			// 
			// btnDelete
			// 
			resources.ApplyResources(this.btnDelete, "btnDelete");
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			this.btnDelete.MouseHover += new System.EventHandler(this.btnDelete_MouseHover);
			// 
			// tableLabel
			// 
			resources.ApplyResources(this.tableLabel, "tableLabel");
			this.tableLabel.Name = "tableLabel";
			// 
			// rowsGroupBox
			// 
			this.rowsGroupBox.Controls.Add(this.chkChangeKey);
			this.rowsGroupBox.Controls.Add(this.chkDelete);
			this.rowsGroupBox.Controls.Add(this.chkUpdate);
			this.rowsGroupBox.Controls.Add(this.chkInsert);
			this.rowsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.rowsGroupBox, "rowsGroupBox");
			this.rowsGroupBox.Name = "rowsGroupBox";
			this.rowsGroupBox.TabStop = false;
			// 
			// chkChangeKey
			// 
			this.chkChangeKey.Checked = true;
			this.chkChangeKey.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.chkChangeKey, "chkChangeKey");
			this.chkChangeKey.Name = "chkChangeKey";
			// 
			// chkDelete
			// 
			this.chkDelete.Checked = true;
			this.chkDelete.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.chkDelete, "chkDelete");
			this.chkDelete.Name = "chkDelete";
			// 
			// chkUpdate
			// 
			this.chkUpdate.Checked = true;
			this.chkUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.chkUpdate, "chkUpdate");
			this.chkUpdate.Name = "chkUpdate";
			// 
			// chkInsert
			// 
			this.chkInsert.Checked = true;
			this.chkInsert.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.chkInsert, "chkInsert");
			this.chkInsert.Name = "chkInsert";
			// 
			// userGroupBox
			// 
			this.userGroupBox.Controls.Add(this.cmbUsers);
			this.userGroupBox.Controls.Add(this.cbAllUsers);
			this.userGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.userGroupBox, "userGroupBox");
			this.userGroupBox.Name = "userGroupBox";
			this.userGroupBox.TabStop = false;
			// 
			// cmbUsers
			// 
			this.cmbUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.cmbUsers, "cmbUsers");
			this.cmbUsers.Name = "cmbUsers";
			// 
			// cbAllUsers
			// 
			this.cbAllUsers.Checked = true;
			this.cbAllUsers.CheckState = System.Windows.Forms.CheckState.Checked;
			resources.ApplyResources(this.cbAllUsers, "cbAllUsers");
			this.cbAllUsers.Name = "cbAllUsers";
			this.cbAllUsers.CheckedChanged += new System.EventHandler(this.cbAllUsers_CheckedChanged);
			// 
			// dateGroupBox
			// 
			this.dateGroupBox.Controls.Add(this.dtpFrom);
			this.dateGroupBox.Controls.Add(this.dtpTo);
			this.dateGroupBox.Controls.Add(this.lblFrom);
			this.dateGroupBox.Controls.Add(this.lblTo);
			this.dateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.dateGroupBox, "dateGroupBox");
			this.dateGroupBox.Name = "dateGroupBox";
			this.dateGroupBox.TabStop = false;
			// 
			// dtpFrom
			// 
			this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			resources.ApplyResources(this.dtpFrom, "dtpFrom");
			this.dtpFrom.Name = "dtpFrom";
			this.dtpFrom.Value = new System.DateTime(2003, 11, 12, 0, 0, 0, 0);
			this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
			// 
			// dtpTo
			// 
			this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			resources.ApplyResources(this.dtpTo, "dtpTo");
			this.dtpTo.Name = "dtpTo";
			this.dtpTo.Value = new System.DateTime(2003, 11, 17, 18, 30, 23, 87);
			this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
			// 
			// lblFrom
			// 
			resources.ApplyResources(this.lblFrom, "lblFrom");
			this.lblFrom.Name = "lblFrom";
			// 
			// lblTo
			// 
			resources.ApplyResources(this.lblTo, "lblTo");
			this.lblTo.Name = "lblTo";
			// 
			// btnRunQuery
			// 
			resources.ApplyResources(this.btnRunQuery, "btnRunQuery");
			this.btnRunQuery.Name = "btnRunQuery";
			this.btnRunQuery.Click += new System.EventHandler(this.btnRunQuery_Click);
			this.btnRunQuery.MouseHover += new System.EventHandler(this.btnRunQuery_MouseHover);
			// 
			// comboBoxTables
			// 
			this.comboBoxTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.comboBoxTables, "comboBoxTables");
			this.comboBoxTables.Name = "comboBoxTables";
			this.comboBoxTables.Sorted = true;
			this.comboBoxTables.SelectedIndexChanged += new System.EventHandler(this.comboBoxTables_SelectedIndexChanged);
			this.comboBoxTables.TabIndexChanged += new System.EventHandler(this.comboBoxTables_SelectedIndexChanged);
			// 
			// splitter1
			// 
			resources.ApplyResources(this.splitter1, "splitter1");
			this.splitter1.Name = "splitter1";
			this.splitter1.TabStop = false;
			// 
			// AuditingQuery
			// 
			this.AllowDrop = true;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panelFilter);
			this.Controls.Add(this.panelData);
			this.Name = "AuditingQuery";
			this.ShowInTaskbar = false;
			this.panelData.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dtgQueryResult)).EndInit();
			this.panelFilter.ResumeLayout(false);
			this.rowsGroupBox.ResumeLayout(false);
			this.userGroupBox.ResumeLayout(false);
			this.dateGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion	

	}
}
