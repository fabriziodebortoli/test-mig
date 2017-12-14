using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Properties;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Controls
{
	//================================================================================
    public partial class ArchivedDocumentGrid : UserControl
    {
        private DMSOrchestrator dmsOrchestrator = null;


        Font f = new Font("Verdana", 8.25F);

        //for datagridview paging
        // Data Table as a source to the DataGridView Control
        private DataTable dtSource = new DataTable();

        // To hold the current page no
        int currentPageIndex = 0;
        // Page Size of the GridView
        private int pageSize = 20;

        public event EventHandler SelectionChanged;
        public event EventHandler EmptyGrid;
        public event EventHandler GridBinded;
        public event EventHandler Accept;
        public event EventHandler SelectionDeleted;
        public event EventHandler<DocumentEventArgs> DocumentSelected;
        private List<DataRow> tempCurrentBoundRows = new List<DataRow>();
        private Diagnostic documentGridDiagnostic = new Diagnostic("DocumentGrid");

        private const string checkColumnName = "CHECK";
        private int checkPosition = -1;//se diverso da -1 mi indica la preenza della riga quindi i suo index


        //---------------------------------------------------------------------
        private bool InvalidateSelection { get; set; }

        //---------------------------------------------------------------------
        public bool EnableDelete { set { deleteToolStripMenuItem.Visible = value; } }

        //--------------------------------------------------------------------------------------------------
        public bool Empty { get { return dataGridView == null || dataGridView.Rows.Count <= 0; } }

        //--------------------------------------------------------------------------------------------------
        public List<DataRow> CurrentBoundRows
        {
            get
            {
                if (InvalidateSelection)
                {
                    tempCurrentBoundRows = new List<DataRow>();
                    if (dataGridView.SelectedRows != null && dataGridView.SelectedRows.Count > 0)
                    {
                        for (int i = 0; i < dataGridView.SelectedRows.Count; i++)
                        {
                            DataGridViewRow gridRow = dataGridView.SelectedRows[i];
                            tempCurrentBoundRows.Add(((DataRowView)(gridRow.DataBoundItem)).Row);
                        }
                    }
                    InvalidateSelection = false;
                }
                return tempCurrentBoundRows;
            }
        }

        //--------------------------------------------------------------------------------------------------
        public void ClearSelectionPages()
        {
            Checked.Clear();
            noChecked.Clear();
        }
     
        List<int> Checked = new List<int>();
        List<int> noChecked = new List<int>();


        //--------------------------------------------------------------------------------------------------
        public void GetCheckedRows()
        {
            CommitEdit();

            if (checkPosition <= -1) return;
            
            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                int id = (int)dataGridView[0, i].Value;
                if (dataGridView[checkColumnName, i].Value != null && (bool)dataGridView[checkColumnName, i].Value)
                {

                    if (!Checked.Contains(id))
                    {
                        Checked.Add(id);

                    } noChecked.Remove(id);
                }

                else
                {

                    if (!noChecked.Contains(id))
                    {
                        noChecked.Add(id);

                    }
                    Checked.Remove(id);
                }
            }
        }


        //--------------------------------------------------------------------------------------------------
        public void SetCheckedRows()
        {

            if (checkPosition <= -1) return;

            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                if (Checked.Contains((int)dataGridView[CommonStrings.ArchivedDocID, i].Value))
                {
                    dataGridView[checkColumnName, i].Value = true;
                }
                else if (noChecked.Contains((int)dataGridView[CommonStrings.ArchivedDocID, i].Value))
                {
                    dataGridView[checkColumnName, i].Value = false;
                }
                else if (SelectedAll)
                {
                    dataGridView[checkColumnName, i].Value = true;
                }
                else
                {
                    dataGridView[checkColumnName, i].Value = false;
                }

            }
            dataGridView.RefreshEdit();
        }
        
        //--------------------------------------------------------------------------------------------------
        public List<DataRow> CheckedRows
        {
            get
            {
                GetCheckedRows();//carica le correnti sulle quali ci potrebbero esssere modifiche
                List<DataRow> l = new List<DataRow>();
                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    if (Checked.Contains((int)dtSource.Rows[i][CommonStrings.ArchivedDocID]))
                       l.Add(dtSource.Rows[i]);

                    else if (noChecked.Contains((int)dtSource.Rows[i][CommonStrings.ArchivedDocID]))
                    {
                        //nothing to do 
                    }
                    else if (SelectedAll)
                    {
                        l.Add(dtSource.Rows[i]);
                    }
                 

                }
                
                return l;
            }
        }

        //--------------------------------------------------------------------------------------------------
        public int PageSize { set { this.pageSize = value; } get { return this.pageSize; } }

        //--------------------------------------------------------------------------------------------------
        public DataTable DataSource
        {
            set
            {
                this.dtSource = value;
                if (dtSource != null)
                {
                    if (dtSource.Rows.Count == 0 || dtSource.Rows.Count <= pageSize)
                    {
                        // Make controls Enabled property to false if no data binded to the gridview
                        TSPreviousPageButton.Enabled = false;
                        TSNextPageButton.Enabled = false;

                        TSSNrPagesLabel.Text = "0 / 0";
                    }
                }

                BindGrid();
            }
            get
            {
                return this.dtSource;
            }
        }

        //--------------------------------------------------------------------------------------------------
        public ArchivedDocumentGrid()
        {
            InitializeComponent();
            h = dataGridView.Height;
        }

        //--------------------------------------------------------------------------------------------------
        private void RemoveEvents()
        {
            dataGridView.SelectionChanged -= new EventHandler(dataGridView1_SelectionChanged);
            dataGridView.MouseWheel -= new MouseEventHandler(dataGridView_MouseWheel);
        }

        private bool SOSMode = false;
        // Necessità del metodo init perchè essendo uno user control il costruttore non può prendere parametri che si lamenta il designer
        //--------------------------------------------------------------------------------------------------
        public void Init(DMSOrchestrator orchestrator, bool sosMode = false)
        {
            RemoveEvents();
            this.dmsOrchestrator = orchestrator;
            dataGridView.Columns.Clear();
            dataGridView.AutoGenerateColumns = false;
            SOSMode = sosMode;
            if (SOSMode)
            {
                deleteToolStripMenuItem.Visible = false;
                DesignSOSTableStyle();
                DataSource = new DataTable();
               // dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            }
            else
            {
                DesignTableStyle();
                DataSource = orchestrator.GetArchivedDocuments();
                
            }dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView.EnableHeadersVisualStyles = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = true;
            dataGridView.AllowUserToOrderColumns = true;
            dataGridView.AllowUserToResizeRows = false;
            dataGridView.AllowUserToResizeColumns = true;



            dataGridView.RowHeadersVisible = false;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.Lavender;
            dataGridView.BackgroundColor = Color.Lavender;

            dataGridView.SelectionChanged += new EventHandler(dataGridView1_SelectionChanged);
            //Intercetto manualmente l'evento della rotellina del mouse sul datagrid
            dataGridView.MouseWheel += new MouseEventHandler(dataGridView_MouseWheel);

            if (dataGridView.SelectedCells.Count == 0 && dataGridView.Rows.Count > 0)
                dataGridView.Rows[0].Selected = true;
            dataGridView.Size = TSContainer.ContentPanel.Size;

        }

        //--------------------------------------------------------------------------------------------------
        public void Filter(FilterEventArgs fea)
        {
            DataSource = dmsOrchestrator.GetArchivedDocuments(fea);
        }

        //--------------------------------------------------------------------------------------
        private void DesignSOSTableStyle()
        {
            // dataGridView.DataError += new DataGridViewDataErrorEventHandler(dataGridView_DataError);

            dataGridView.Columns.Clear();
            DataGridViewCellStyle verdanaStyle = new DataGridViewCellStyle();
            verdanaStyle.Font = f;

            DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn();
            DataGridViewCheckBoxColumn cbc = new DataGridViewCheckBoxColumn();
            DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn docTypeColumn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn keysColumn = new DataGridViewTextBoxColumn();

            //ID column
            idColumn.Name = CommonStrings.ArchivedDocID;
            idColumn.DataPropertyName = CommonStrings.ArchivedDocID;
            idColumn.HeaderText = String.Empty;
            idColumn.Width = 1;
            idColumn.Visible = false;
            idColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            idColumn.ReadOnly = true;
            idColumn.DefaultCellStyle = verdanaStyle;
            idColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(idColumn);
            //check column
            
            cbc.ThreeState = false;
            cbc.TrueValue = true;
            cbc.FalseValue = false;
            cbc.Tag = checkColumnName;
            cbc.Name = checkColumnName;
            cbc.HeaderText = string.Empty;
            cbc.Width = 30;
            cbc.SortMode = DataGridViewColumnSortMode.NotSortable;
            cbc.ReadOnly = false;
            dataGridView.Columns.Add(cbc);
            checkPosition = 1;// porcheria?

            //FileName column
            fileNameColumn.Name = CommonStrings.Name;
            fileNameColumn.DataPropertyName = CommonStrings.Name;
            fileNameColumn.HeaderText = Strings.Name;
            fileNameColumn.Width = 200;
            fileNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            fileNameColumn.ReadOnly = true;
            fileNameColumn.DefaultCellStyle = verdanaStyle;
            fileNameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(fileNameColumn);

            //Document Type column
            docTypeColumn.Name = CommonStrings.SOSDocumentType;
            docTypeColumn.DataPropertyName = CommonStrings.DocNamespace;
            docTypeColumn.HeaderText = Strings.SOSDocumentType;
            docTypeColumn.Width = 200;
            docTypeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            docTypeColumn.ReadOnly = true;
            docTypeColumn.DefaultCellStyle = verdanaStyle;
            docTypeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(docTypeColumn);


            //keys column
            keysColumn.Name = CommonStrings.DocKeyDescription;
            keysColumn.DataPropertyName = CommonStrings.DocKeyDescription;
            keysColumn.HeaderText = Strings.DocKeyDescription;
            keysColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            keysColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            keysColumn.ReadOnly = true;
            keysColumn.DefaultCellStyle = verdanaStyle;
            keysColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(keysColumn);

        }

        //--------------------------------------------------------------------------------------
        void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

            MessageBox.Show(String.Format(Strings.Error, e.Exception.ToString()), CommonStrings.EasyAttachmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //questi errori possono anche non essere tradotti? forse questo metodo è inutile.
            if (e.Context == DataGridViewDataErrorContexts.Commit)
                MessageBox.Show("Commit error", CommonStrings.EasyAttachmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
                MessageBox.Show("Cell change", CommonStrings.EasyAttachmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (e.Context == DataGridViewDataErrorContexts.Parsing)
                MessageBox.Show("Parsing error", CommonStrings.EasyAttachmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (e.Context == DataGridViewDataErrorContexts.LeaveControl)
                MessageBox.Show("Leave control error", CommonStrings.EasyAttachmentTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if ((e.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[e.RowIndex].ErrorText = "an error";
                view.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "an error";
                e.ThrowException = false;
            }
        }

        //--------------------------------------------------------------------------------------
        private void DesignTableStyle()
        {
            DataGridViewCellStyle verdanaStyle = new DataGridViewCellStyle();

            verdanaStyle.Font = f;

            DataGridViewTextBoxColumn idColumn = null;
            DataGridViewTextBoxColumn modifiedDateColumn = null;
            DataGridViewTextBoxColumn workerColumn = null;
            DataGridViewTextBoxColumn fileNameColumn = null;
            DataGridViewImageColumn attachedColumn = null;

            DataGridViewImageColumn storageTypeColumn = null;

            //ID column
            idColumn = new DataGridViewTextBoxColumn();
            idColumn.Name = CommonStrings.ArchivedDocID;
            idColumn.DataPropertyName = CommonStrings.ArchivedDocID;
            idColumn.HeaderText = Strings.ID;
            idColumn.Width = 40;
            idColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            idColumn.ReadOnly = true;
            idColumn.DefaultCellStyle = verdanaStyle;
            idColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(idColumn);

            //Attached column
            attachedColumn = new DataGridViewImageColumn();
            attachedColumn.DefaultCellStyle.NullValue = null;
            attachedColumn.Name = CommonStrings.Attached;
            attachedColumn.DataPropertyName = CommonStrings.Attached;
            attachedColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            attachedColumn.ReadOnly = true;
            attachedColumn.DefaultCellStyle = verdanaStyle;
            attachedColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            attachedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            attachedColumn.Width = 25;
            attachedColumn.Resizable = DataGridViewTriState.False;
            dataGridView.Columns.Add(attachedColumn);

            
            //bool checkoutEnabled = dmsOrchestrator.SettingsManager.UserSettingState.Options.RepositoryOptionsState.EnableCheckInCheckOut;
         
            //MODIFIERID column
            DataGridViewImageColumn modifieridcol = null;
            modifieridcol = new DataGridViewImageColumn();
            modifieridcol.DefaultCellStyle.NullValue = null;
            modifieridcol.Name = CommonStrings.ModifierID;
            modifieridcol.ToolTipText = Strings.ModifierID;
            modifieridcol.DataPropertyName = CommonStrings.ModifierID;
            modifieridcol.SortMode = DataGridViewColumnSortMode.Automatic;
            modifieridcol.ReadOnly = true;
            modifieridcol.DefaultCellStyle = verdanaStyle;
            modifieridcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            modifieridcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            modifieridcol.Width = 25;
            //modifieridcol.Visible = dmsOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.EnableCheckInCheckOut;
            modifieridcol.Resizable = DataGridViewTriState.False;
            dataGridView.Columns.Add(modifieridcol);


            //StorageType
            storageTypeColumn = new DataGridViewImageColumn();
            storageTypeColumn.Name = CommonStrings.StorageType;
            storageTypeColumn.DataPropertyName = CommonStrings.StorageType;
            storageTypeColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            storageTypeColumn.ReadOnly = true;
            storageTypeColumn.ToolTipText = Strings.StorageType;          
            storageTypeColumn.DefaultCellStyle = verdanaStyle;
            storageTypeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            storageTypeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            storageTypeColumn.Width = 20;
            storageTypeColumn.Resizable = DataGridViewTriState.False;
            dataGridView.Columns.Add(storageTypeColumn);
            
         
            //FileName column
            fileNameColumn = new DataGridViewTextBoxColumn();
            fileNameColumn.Name = CommonStrings.Name;
            fileNameColumn.DataPropertyName = CommonStrings.Name;
            fileNameColumn.HeaderText = Strings.Name;
            fileNameColumn.Width = 280;
            fileNameColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            fileNameColumn.ReadOnly = true;
            fileNameColumn.DefaultCellStyle = verdanaStyle;
            fileNameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(fileNameColumn);

            //worker name column. Contains the name of the worker that archived the current document
            workerColumn = new DataGridViewTextBoxColumn();
            workerColumn.Name = CommonStrings.WorkerName;
            workerColumn.DataPropertyName = CommonStrings.WorkerName;
            workerColumn.HeaderText = Strings.WorkerName;
            workerColumn.Width = 100;
            workerColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            workerColumn.ReadOnly = true;
            workerColumn.DefaultCellStyle = verdanaStyle;
            workerColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView.Columns.Add(workerColumn);

            //DateModified column
            modifiedDateColumn = new DataGridViewTextBoxColumn();
            modifiedDateColumn.Name = CommonStrings.TBModified;
            modifiedDateColumn.DataPropertyName = CommonStrings.TBModified;
            modifiedDateColumn.HeaderText = Strings.DateModified;
            modifiedDateColumn.Width = 150;
            modifiedDateColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            modifiedDateColumn.ReadOnly = true;
            modifiedDateColumn.DefaultCellStyle = verdanaStyle;
            modifiedDateColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            modifiedDateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns.Add(modifiedDateColumn);

            //StorageType column
            //storageTypeColumn = new DataGridViewTextBoxColumn();
            //storageTypeColumn.Name = CommonStrings.StorageType;
            //storageTypeColumn.DataPropertyName = CommonStrings.StorageType;
            //storageTypeColumn.HeaderText = Strings.StorageType;
            //storageTypeColumn.Width = 40;
            //storageTypeColumn.SortMode = DataGridViewColumnSortMode.Automatic;
            //storageTypeColumn.ReadOnly = true;
            //storageTypeColumn.DefaultCellStyle = verdanaStyle;
            //storageTypeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //storageTypeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //dataGridView.Columns.Add(storageTypeColumn);                        
        }

        //--------------------------------------------------------------------------------------------------
        void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            InvalidateSelection = true;
            drag = false;
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

        }

        ///<summary>
        /// Intercetto manualmente l'evento della rotellina del mouse sul datagrid in modo da spostarmi tra le righe
        ///</summary>
        //--------------------------------------------------------------------------------------------------
        void dataGridView_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                this.SuspendLayout();
                MoveInGrid(e.Delta > 0);
                this.ResumeLayout();
            }
            catch
            {
            }
        }

        //this.images is an ImageList with your bitmaps
        //---------------------------------------------------------------------
        void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (!SOSMode && e.ColumnIndex == 1 && e.RowIndex == -1)
            {
                e.PaintBackground(e.ClipBounds, false);

                Point pt = e.CellBounds.Location;// where you want the bitmapin the cell
                int offset = (e.CellBounds.Width - this.imageList1.ImageSize.Width) / 2;
                pt.X += offset;
                pt.Y += 1;
                this.imageList1.Draw(e.Graphics, pt, 0);
                e.Handled = true;
            }
            if (!SOSMode && e.ColumnIndex == 2 && e.RowIndex == -1)
            {
                e.PaintBackground(e.ClipBounds, false);

                Point pt = e.CellBounds.Location;// where you want the bitmapin the cell
                int offset = (e.CellBounds.Width - this.imageList1.ImageSize.Width) / 2;
                pt.X += offset;
                pt.Y += 1;
                this.imageList1.Draw(e.Graphics, pt, 1);
                e.Handled = true;
            }

            if (!SOSMode && e.ColumnIndex == 3 && e.RowIndex == -1)
            {
                e.PaintBackground(e.ClipBounds, false);

                Point pt = e.CellBounds.Location;// where you want the bitmapin the cell
                int offset = (e.CellBounds.Width - this.imageList1.ImageSize.Width) / 2;
                pt.X += offset;
                pt.Y += 1;
                this.imageList1.Draw(e.Graphics, pt, (dmsOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageToFileSystem) ? 2 : 3);
                e.Handled = true;
            }     
        }

        /// <summary>
        /// Show a different bitmap for each file type
        /// </summary>
        //---------------------------------------------------------------------
        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!SOSMode && e.ColumnIndex == 1)//colonna dell'icon che indica se è attached
            {
                if (e.Value == null)
                    return;

                DataGridViewCell cell = this.dataGridView[e.ColumnIndex, e.RowIndex];
                bool cellValue = (bool)e.Value;
                e.Value = (cellValue) ? Resources.paperclip16 : Resources.Transparent;//devo mettere l'icona trasparente, se metto null da eccezione


            }
            if (!SOSMode && e.ColumnIndex == 2)//colonna dell'icon che indica se è checkedout
            {
                if (e.Value == null)
                    return;

                DataGridViewCell cell = this.dataGridView[e.ColumnIndex, e.RowIndex];
                int cellValue = (cell.Value == DBNull.Value)?  -1 : (int)cell.Value;
                bool checkedout = (cellValue > -1);
                e.Value = checkedout ? Resources.Modifying : Resources.Transparent;//devo mettere l'icona trasparente, se metto null da eccezione
                if (checkedout)
                    cell.ToolTipText = dmsOrchestrator.GetWorkerName(cellValue);
                else
                    cell.ToolTipText = null;

            }

            if (!SOSMode && e.ColumnIndex == 3)//colonna dell'icon che indica lo storage type
            {
                if (e.Value == null)
                    return;
                
                DataGridViewRow rowView = dataGridView.Rows[e.RowIndex];
                DataRow currRow = ((DataRowView)(rowView.DataBoundItem)).Row;

                DataGridViewCell cell = this.dataGridView[e.ColumnIndex, e.RowIndex];
                StorageTypeEnum cellValue = (cell.Value == DBNull.Value) ? StorageTypeEnum.Database : (StorageTypeEnum)cell.Value;
                switch (cellValue)
                {
                    case StorageTypeEnum.Database:
                        //e.Value = Resources.Database16x16;
                        //cell.ToolTipText = Strings.BinarySaveOnDB;
                        e.Value = Resources.Transparent;
                        cell.ToolTipText = string.Empty;
                        break;
                    case StorageTypeEnum.FileSystem:
                        e.Value = Resources.Folder16x16;
                        AttachmentInfo ai = GetAttachmentInfo(currRow);
                        cell.ToolTipText = string.Format(Strings.BinarySaveOnFileSystem, (ai != null) ? ai.StorageFile : string.Empty);
                        break;
                }                        

            }
        }

        //--------------------------------------------------------------------------------------------------
        private void InitDeleteSelectedRow()
        {
            if (CurrentBoundRows == null || CurrentBoundRows.Count == 0)
                return;

            if (SelectionDeleted != null)
                SelectionDeleted(this, EventArgs.Empty);
        }

        //--------------------------------------------------------------------------------------------------
        internal void CompleteDeleteRow(DataRow row)
        {
            if (row == null) return;

            string val = row[CommonStrings.ArchivedDocID].ToString();

            foreach (DataGridViewRow r in dataGridView.SelectedRows)
            {
                if (String.Compare(val, r.Cells[CommonStrings.ArchivedDocID].Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    dataGridView.Rows.Remove(r);
                    break;
                }
            }

            foreach (DataRow rr in DataSource.Rows)
            {
                if (String.Compare(val, rr[CommonStrings.ArchivedDocID].ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    rr.Delete();
                    DataSource.AcceptChanges();
                    break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        internal void CompleteDeleteRow2(DataRow row)
        {
            if (row == null) return;

            string val = row[CommonStrings.ArchivedDocID].ToString();

            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                if (String.Compare(val, r.Cells[CommonStrings.ArchivedDocID].Value.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    dataGridView.Rows.Remove(r);
                    break;
                }
            }

            foreach (DataRow rr in DataSource.Rows)
            {
                if (String.Compare(val, rr[CommonStrings.ArchivedDocID].ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    rr.Delete();
                    DataSource.AcceptChanges();
                    break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        internal void Adjust()
        {
            if (
                dataGridView.RowCount > 0 &&
                (dataGridView.SelectedRows == null || dataGridView.SelectedRows.Count == 0 || dataGridView.CurrentCell == null)
                )
            {
                dataGridView.CurrentCell = dataGridView.Rows[dataGridView.RowCount - 1].Cells[1];
            }

            if (dataGridView.RowCount == 0)
            {
                if (TSPreviousPageButton.Enabled)
                    TSPreviousPageButton_Click(this, EventArgs.Empty);
                else if (TSNextPageButton.Enabled)
                    TSNextPageButton_Click(this, EventArgs.Empty);
                else
                    LblNoItem.Visible = true;
            }
            else
                Paging();
        }

        //l'attachmentInfo viene caricato on demand solo quando serve
        //---------------------------------------------------------------------
        public AttachmentInfo GetAttachmentInfo(DataRow currentRow)
        {
            AttachmentInfo att = null;

            if (currentRow != null)
            {
                att = currentRow[CommonStrings.AttachmentInfo] as AttachmentInfo;
                if (att == null && currentRow[CommonStrings.ArchivedDocID] != null && (int)currentRow[CommonStrings.ArchivedDocID] > -1)
                {
                    att = dmsOrchestrator.GetAttachmentInfoFromArchivedDocId((int)currentRow[CommonStrings.ArchivedDocID]);
                    currentRow[CommonStrings.AttachmentInfo] = att;
                }
            }

            return att;
        }

        //---------------------------------------------------------------------
        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            foreach (DataRow CurrentBoundRow in CurrentBoundRows)
            {
                AttachmentInfo id = GetAttachmentInfo(CurrentBoundRow);

                if (id != null && DocumentSelected != null)
                    DocumentSelected(sender, new DocumentEventArgs(id));
            }
        }

        //---------------------------------------------------------------------
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridView.SelectedRows == null || dataGridView.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }
        }

        //---------------------------------------------------------------------
        internal int GetSelectedID()
        {
            if (dataGridView.SelectedRows != null && dataGridView.SelectedRows.Count > 0)
                return ((int)dataGridView.SelectedRows[0].Cells[CommonStrings.ArchivedDocID].Value);
            return -1;
        }

        //---------------------------------------------------------------------
        internal bool TrySelectRow(int id)
        {
            if (id == -1) return false;
            // ora che sono nella pagina giusta seleziono la riga che mi interessa
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (((int)row.Cells[CommonStrings.ArchivedDocID].Value) == id)
                {
                    dataGridView.CurrentCell = dataGridView.Rows[row.Index].Cells[1];


                    return true;
                }
            }
            return false;
        }

        //---------------------------------------------------------------------
        private void MoveInGrid(bool up)
        {
           
            if (dataGridView.Rows != null && dataGridView.Rows.Count > 0)
            {
                int i = 0;
                if (dataGridView.SelectedRows != null && dataGridView.SelectedRows.Count > 0)
                    i = dataGridView.SelectedRows[0].Index;

                if (up) --i; else ++i;

                if (dataGridView.Rows.Count >= i && i >= 0 && i < dataGridView.Rows.Count)
                    dataGridView.CurrentCell = dataGridView.Rows[i].Cells[1];//imposto la cell così scrolla da solo se ce ne fosse bisogno

                if (i == dataGridView.Rows.Count && TSNextPageButton.Enabled)
                    TSNextPageButton_Click(this, EventArgs.Empty);

                if (i == -1 && TSPreviousPageButton.Enabled)
                {
                    TSPreviousPageButton_Click(this, EventArgs.Empty);
                    dataGridView.CurrentCell = dataGridView.Rows[dataGridView.Rows.Count - 1].Cells[1];//imposto la cell così scrolla da solo se ce ne fosse bisogno 
                }
            }

            if (!SOSMode) AdjustSelection();
        }

        //---------------------------------------------------------------------
        private void AdjustSelection()
        {
            if (selection.ContainsKey(currentPageIndex) || noselection.ContainsKey(currentPageIndex)) return;
            if (SelectedAll)
            {
                if (!SOSMode)  dataGridView.SelectAll();

                if (checkPosition > -1)
                    for (int i = 0; i < dataGridView.RowCount; i++)
                        ((DataGridViewCheckBoxCell)dataGridView[checkColumnName, i]).Value = true;
                dataGridView.RefreshEdit();



            }
        }

        public event EventHandler ArchivedDocReplaced;

        //---------------------------------------------------------------------
        private bool Exists(AttachmentInfo attachmentInfo)
        {

            int existingRowIdx = -1;
            // scorro tutte le righe del datasource completo e cerco il documento archiviato
            // se lo trovo memorizzo il suo indice
            for (int i = 0; i < DataSource.Rows.Count; i++)
            {
                DataRow archDocRow = DataSource.Rows[i];
                if ((int)archDocRow[CommonStrings.ArchivedDocID] == attachmentInfo.ArchivedDocId)
                {
                    existingRowIdx = i + 1;


                    break;
                }
            }

            // ho trovato il documento
            if (existingRowIdx > -1)
            {
                // calcolo il nr delle pagine totali
                int totPages = (int)Math.Ceiling(Convert.ToDecimal(DataSource.Rows.Count) / pageSize);
                // calcolo il nr di pagina in cui si trova la riga che sto cercando
                int goToPage = (int)Math.Ceiling(Convert.ToDecimal(existingRowIdx) / pageSize) - 1;

                // devo scorrere in avanti
                if (goToPage > currentPageIndex)
                {
                    int nrPages = goToPage - currentPageIndex;
                    for (int i = 0; i < nrPages; i++)
                        TSNextPageButton_Click(this, EventArgs.Empty);
                }

                // devo scorrere indietro
                if (goToPage < currentPageIndex)
                {
                    int nrPages = currentPageIndex - goToPage;
                    for (int i = 0; i < nrPages; i++)
                        TSPreviousPageButton_Click(this, EventArgs.Empty);
                }

                // ora che sono nella pagina giusta seleziono la riga che mi interessa
                if (TrySelectRow(attachmentInfo.ArchivedDocId))
                {
                    
                    ////Siccome dopo la riconcialiazione dei papery sarebbe opportuno impostare la graffetta sui documenti del repository 
                    ////ho fatto in modo che dopo la riconcialiazione rieffettuo la add, 
                    ////il metodo exist si accorge che la riga esiste già e ne modifica il valore "attached" se è il caso.
                    ////devo modificarla sia sulla riga della griglia sia sulla riga del datasource, 
                    ////perche poi la formatting della cella si basa sul valore della griglia, 
                    ////ma se non modifico il datasource alla successiva add mi resetta il valore, 
                    ////almeno fin oa che non faccio refresh di tutto.
                    ////


                    foreach (DataRow dr in DataSource.Rows)
                    {
                        if ((int)dr[CommonStrings.ArchivedDocID] == (int)CurrentBoundRows[0][CommonStrings.ArchivedDocID])
                        {

                            //se sto facendo un replace, seleziono la riga ma rimangono tutti i valori vechhi, evo quindi capire se ho fatto un replace e  aggiornare ivalori.
                            AttachmentInfo a = dr[CommonStrings.AttachmentInfo] as AttachmentInfo;


                            dr[CommonStrings.Name] = CurrentBoundRows[0][CommonStrings.Name] = attachmentInfo.Name;
                            dr[CommonStrings.WorkerName] = CurrentBoundRows[0][CommonStrings.WorkerName] = attachmentInfo.ModifiedBy;
                            dr[CommonStrings.TBModified] = CurrentBoundRows[0][CommonStrings.TBModified] = attachmentInfo.LastWriteTimeUtc;
                            dr[CommonStrings.AttachmentInfo] = CurrentBoundRows[0][CommonStrings.AttachmentInfo] = attachmentInfo;
                            dr[CommonStrings.Attached] = CurrentBoundRows[0][CommonStrings.Attached] = (dmsOrchestrator.SearchManager.IsAttached(attachmentInfo.ArchivedDocId));
                            dr[CommonStrings.StorageType] = CurrentBoundRows[0][CommonStrings.StorageType] = (attachmentInfo.StorageType);                            
                            DataSource.AcceptChanges();
                            if (ArchivedDocReplaced != null) ArchivedDocReplaced(null, null);
                            DataSource.AcceptChanges();

                        }
                    }

                    return true;
                }
            }

            return false;
        }

        //---------------------------------------------------------------------
        internal void Add(AttachmentInfo attachmentInfo)
        {
            if (attachmentInfo == null)
                return;

            if (DataSource == null)//nessun documento
                DataSource = new ArchivedDocDataTable();
            if (Exists(attachmentInfo)) return;
            // se la riga esiste gia' nel grid devo selezionarla e non procedere
            //ma DOVREI AGGIORNARLA!( vedi riconciliazione pending papery)

            DataRow newRow = DataSource.NewRow();
            newRow[CommonStrings.ArchivedDocID] = attachmentInfo.ArchivedDocId;
            newRow[CommonStrings.Name] = attachmentInfo.Name;
            newRow[CommonStrings.TBModified] = DateTime.Now;
            newRow[CommonStrings.TBCreated] = DateTime.Now;
            newRow[CommonStrings.Attached] = false;
            newRow[CommonStrings.StorageType] = dmsOrchestrator.SettingsManager.UsersSettingState.Options.StorageOptionsState.StorageToFileSystem; ;
            newRow[CommonStrings.AttachmentInfo] = attachmentInfo;
            newRow[CommonStrings.WorkerName] = attachmentInfo.ModifiedBy; 
            newRow[CommonStrings.ModifierID] = attachmentInfo.ModifierID;
			newRow[CommonStrings.IsWoormReport] = attachmentInfo.IsWoormReport;

            DataSource.Rows.Add(newRow);
            DataSource.AcceptChanges();
            BindGrid();

            if (dataGridView.RowCount > 0)//imposto la cell così scrolla sull'ultima aggiunta
            {
                // calcolo le pagine totali (esclusa la corrente)
                int totPages = (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize) - 1;
                // conto quante me ne mancano alla fine ed eseguo tanti pagedown fino ad arrivare all'ultima pagina
                int nr = totPages - currentPageIndex;
                for (int i = 0; i < nr; i++)
                    TSNextPageButton_Click(this, EventArgs.Empty);

                dataGridView.ClearSelection();
                dataGridView.Rows[dataGridView.Rows.Count - 1].Selected = true;
            }
        }

        //---------------------------------------------------------------------
        private void GetNewRow(ref DataRow newRow, DataRow source)
        {
            foreach (DataColumn col in dtSource.Columns)
                newRow[col.ColumnName] = source[col.ColumnName];
        }

        // To Make the GridView columns read only.
        //---------------------------------------------------------------------
        private void MakeGridColumnsReadOnly()
        {
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                if (dataGridView.Columns[i].Tag as string == checkColumnName)
                    continue;//check

                dataGridView.Columns[i].ReadOnly = true;
            }
        }


        // Method to bind the data to the gridview
        //---------------------------------------------------------------------
        public void BindGrid()
        {
            //if data source contains records
            if (dtSource.Rows.Count > 0)
            {
                SafeGui.ControlVisible(LblNoItem, false);
                Paging();
                if (GridBinded != null)
                    GridBinded(this, EventArgs.Empty);
            }
            else
            {

                // se datasource vuoto, svuoto la griglia e mostro label
                dataGridView.DataSource = null;
                SafeGui.ControlVisible(LblNoItem, true);
                if (EmptyGrid != null)
                    EmptyGrid(this, EventArgs.Empty);
            }
            // To Make the gridview columns as read only.
            MakeGridColumnsReadOnly();
        }

        //---------------------------------------------------------------------
        private void Paging()
        {
            if (dtSource.Rows.Count <= 0) return;

            int id = GetSelectedID();

            //Create a temporary table
            DataTable tmptable = dtSource.Clone();
            double p = (dataGridView.Height / 23);//in questo modo la quantità di righe viene calcolata in base allo spazio disponibile (23 è lo spazio di default, spero non dia problemi con dpi settings diversi dal default - provare)
            pageSize = Convert.ToInt32(p);

            //set the start index
            int startIndex = currentPageIndex * pageSize;

            //set the last index
            int endIndex = currentPageIndex * pageSize + pageSize;

            // if end index value greater than datasource records count set the end index as datasource rows count.
            if (endIndex > dtSource.Rows.Count)
                endIndex = dtSource.Rows.Count;

            // Get the records from start index to end index and add it to the above created temp data table.
            for (int i = startIndex; i < endIndex; i++)
            {
                DataRow newRow = tmptable.NewRow();
                // GetNewRow will get the datarow ata a particular index from datasource.
                GetNewRow(ref newRow, dtSource.Rows[i]);
                tmptable.Rows.Add(newRow);
            }
            SafeControlProperty.Set(dataGridView, "DataSource", tmptable);
            //threadsafe
            //dataGridView.DataSource = tmptable;

            // Assign text to the ToolStripStatusLabel
            string text = (currentPageIndex + 1) + " / " + (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize);

            SafeToolStripItem.Text(TSSNrPagesLabel, text);
            //threadsafe
            //TSSNrPagesLabel.Text = (currentPageIndex + 1) + " / " + (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize);

			// enable/disable navigation page buttons
		//	TSPreviousPageButton.Enabled = (currentPageIndex > 0);
			//TSNextPageButton.Enabled = (currentPageIndex < (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize) - 1);
            ButtonEnabled();
			//questo serve perchè se sono sull'ultima pagina, resize della griglia, le pagine diminuirebbero ma io rimarrei sull'ultima pagina vuota e non sarebbe corretto.
			if (((currentPageIndex) * pageSize) > dtSource.Rows.Count && !TSNextPageButton.Enabled)
				TSPreviousPageButton_Click(this, EventArgs.Empty);

            TrySelectRow(id);
        }

        //--------------------------------------------------------------------------------
        void ButtonEnabled()
        {
            //BeginInvoke(new MethodInvoker(() =>
            //{
                TSPreviousPageButton.Enabled = (currentPageIndex > 0);
                TSNextPageButton.Enabled = (currentPageIndex < (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize) - 1);
            //}));
        }


        private int h = 0;
        public event EventHandler GridSizeChanging; //per dire alle form di preview di non modificarsi
        public event EventHandler GridSizeChanged;
        //---------------------------------------------------------------------
        private void dataGridView_SizeChanged(object sender, EventArgs e)
        {
            //così non scateno il paging se cambio solo la larghezza.

            if (dataGridView.Height != h)
            {
                if (GridSizeChanging != null)
                    GridSizeChanging(this, EventArgs.Empty);

                h = dataGridView.Height;
                Paging();

                if (GridSizeChanged != null)
                    GridSizeChanged(this, EventArgs.Empty);

            }
        }

        //---------------------------------------------------------------------
        private void SaveAs()
        {
            if (CurrentBoundRows == null || CurrentBoundRows.Count == 0)
                return;
            AttachmentInfo ai = null;

            foreach (DataRow CurrentBoundRow in CurrentBoundRows)
            {
                ai = GetAttachmentInfo(CurrentBoundRow);
                if (ai == null || (ai.DocContent == null && !ai.VeryLargeFile))
                    return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = ai.ExtensionType;
                saveFileDialog.DefaultExt.Replace(".", "");
                saveFileDialog.Filter = String.Format("{0} (*.*)|*.*", Strings.AllFiles);
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer, Environment.SpecialFolderOption.Create);
                saveFileDialog.FileName = ai.Name;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    if (ai.VeryLargeFile)
                    {
                        File.Copy(ai.TempPath, saveFileDialog.FileName);
                        return;
                    }

                    using (Stream myStream = saveFileDialog.OpenFile())
                    using (StreamWriter s = new StreamWriter(myStream))
                        myStream.Write(ai.DocContent, 0, ai.DocContent.Length);

                    Cursor.Current = Cursors.Default;
                }

                ai.DisposeDocContent();
            }
        }

        //---------------------------------------------------------------------
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentBoundRows == null || CurrentBoundRows.Count == 0)
                return;

            if (CurrentBoundRows.Count == 1)
            {
                SaveAs();
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.ShowNewFolderButton = true;

            try
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    AttachmentInfo ai = null;
                    foreach (DataRow CurrentBoundRow in CurrentBoundRows)
                    {
                        ai = GetAttachmentInfo(CurrentBoundRow);
                        if (ai == null || (ai.DocContent == null && !ai.VeryLargeFile))
                            continue;

                        if (ai.VeryLargeFile)
                        {
                            File.Copy(ai.TempPath, Path.Combine(dialog.SelectedPath, ai.Name));
                            continue;
                        }

                        using (Stream myStream = File.OpenWrite(Path.Combine(dialog.SelectedPath, ai.Name)))
                        using (StreamWriter s = new StreamWriter(myStream))
                            myStream.Write(ai.DocContent, 0, ai.DocContent.Length);
                    }
                    Cursor.Current = Cursors.Default;
                    ai.DisposeDocContent();
                }
            }
            catch { }//todo errore, per ora niente
        }

        //---------------------------------------------------------------------
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitDeleteSelectedRow();
        }

        ///<summary>
        /// Go to previous row
        ///</summary>
        //---------------------------------------------------------------------
        private void TSPreviousRowButton_Click(object sender, EventArgs e)
        {
            MoveInGrid(true);
        }

        ///<summary>
        /// Go to next row
        ///</summary>
        //---------------------------------------------------------------------
        private void TSNextRowButton_Click(object sender, EventArgs e)
        {
            MoveInGrid(false);
        }

        ///<summary>
        /// Go to previous page
        ///</summary>
        //---------------------------------------------------------------------
        private void TSPreviousPageButton_Click(object sender, EventArgs e)
        {

            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            GetCheckedRows();
            currentPageIndex--;

            if (currentPageIndex == 0)
                TSPreviousPageButton.Enabled = false;
            else
                TSPreviousPageButton.Enabled = true;

            TSNextPageButton.Enabled = true;

            BindGrid(); 
            SetCheckedRows();
            if (!SOSMode) AdjustSelection();
            this.Cursor = currentCursor;
        }

        private Dictionary<int, List<DataRow>> selection = new Dictionary<int, List<DataRow>>();
        private Dictionary<int, List<DataRow>> noselection = new Dictionary<int, List<DataRow>>();
        ///<summary>
        /// Go to next page
        ///</summary>
        //---------------------------------------------------------------------
        private void TSNextPageButton_Click(object sender, EventArgs e)
        {
        
            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            GetCheckedRows();
            currentPageIndex++;


            if (currentPageIndex >= (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize) - 1)
                TSNextPageButton.Enabled = false;
            else
                TSNextPageButton.Enabled = true;

            TSPreviousPageButton.Enabled = true;

            BindGrid(); 
            SetCheckedRows();
            if (!SOSMode) AdjustSelection();
            this.Cursor = currentCursor;
        }

        //---------------------------------------------------------------------
        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            InitDeleteSelectedRow();
            e.Cancel = true;
        }

        //---------------------------------------------------------------------
        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (Accept != null) Accept(null, null); e.Handled = true; break;
                case Keys.PageDown:
                    if (TSNextPageButton.Enabled)
                        TSNextPageButton_Click(sender, e);
                    break;
                case Keys.PageUp:
                    if (TSPreviousPageButton.Enabled)
                        TSPreviousPageButton_Click(sender, e);
                    break;
                case Keys.Down:
                    MoveInGrid(false);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    MoveInGrid(true);
                    e.Handled = true;
                    break;
                case Keys.Home:
                    // se ho cliccato su CTRL-HOME seleziono la prima riga della pagina corrente
                    if (e.Modifiers == Keys.Control)
                    {
                        if (dataGridView.Rows != null && dataGridView.Rows.Count > 0)
                            dataGridView.Rows[0].Selected = true;
                    }
                    else // altrimenti mi posiziono in cima alla prima pagina
                    {
                        // memorizzo l'indice della pagina corrente
                        int currentIdx = currentPageIndex;
                        // eseguo tanti pageup fino ad arrivare alla prima pagina
                        for (int i = 0; i < currentIdx; i++)
                            TSPreviousPageButton_Click(sender, e);
                        // seleziono la prima riga
                        dataGridView.Rows[0].Selected = true;
                    }
                    break;
                case Keys.End:
                    // se ho cliccato su CTRL-END seleziono l'ultima riga della pagina corrente
                    if (e.Modifiers == Keys.Control)
                    {
                        if (dataGridView.Rows != null && dataGridView.Rows.Count > 0)
                            dataGridView.Rows[dataGridView.Rows.Count - 1].Selected = true;
                    }
                    else // altrimenti mi posiziono in fondo alla prima pagina
                    {
                        // calcolo le pagine totali (esclusa la corrente)
                        int totPages = (int)Math.Ceiling(Convert.ToDecimal(dtSource.Rows.Count) / pageSize) - 1;
                        // conto quante me ne mancano alla fine ed eseguo tanti pagedown fino ad arrivare all'ultima pagina
                        int nr = totPages - currentPageIndex;
                        for (int i = 0; i < nr; i++)
                            TSNextPageButton_Click(sender, e);
                        // seleziono l'ultima riga
                        dataGridView.Rows[dataGridView.Rows.Count - 1].Selected = true;
                    }
                    break;
            }
        }

        //---------------------------------------------------------------------
        private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Clicks != 1 || e.RowIndex == -1 || dataGridView.Rows.Count == 0)
                return;//così non si mangia il doppio click

            if (dataGridView.Columns[e.ColumnIndex].Tag as string == checkColumnName) return;//check

            //seleziona riga anche con tasto destro
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.Button == MouseButtons.Right)
            {
                if (dataGridView.CurrentCell.RowIndex != e.RowIndex)
                {
                    dataGridView.ClearSelection();
                    dataGridView.CurrentCell = dataGridView.Rows[e.RowIndex].Cells[1];
                }
                return;
            }


            List<string> ss = new List<string> { };
            AttachmentInfo ai = null;
            if (CurrentBoundRows != null && CurrentBoundRows.Count > 0)
                foreach (DataRow r in CurrentBoundRows)
                {
                    ai = GetAttachmentInfo(r);
                    if (ai.SaveAttachmentFile())
                        ss.Add(ai.TempPath);
                }
            if (ss.Count > 0)
                this.DoDragDrop(new DataObject(DataFormats.FileDrop, ss.ToArray()), DragDropEffects.Copy);
        }

        //devo rilanciare l'evento così che chi contiene sa
        //---------------------------------------------------------------------
        private void dataGridView_DragEnter(object sender, DragEventArgs e)
        {
            OnDragEnter(e);
        }

        //devo rilanciare l'evento così che chi contiene sa
        //---------------------------------------------------------------------
        private void dataGridView_DragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        public bool drag = false;  //sul mouse down gestisco l'eventualità del drag& drop.
        //--------------------------------------------------------------------------------
        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;  //sul mouse down gestisco l'eventualità del drag& drop.
        }

        //--------------------------------------------------------------------------------
        private void dataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        public bool SelectedAll = false;//per gestire la visualizzazione del pulsante differenzata

        //---------------------------------------------------------------------
        private void TSBSelectAll_Click(object sender, EventArgs e)
        {
            
            if (SelectedAll)
            {
                Checked.Clear();
                dataGridView.ClearSelection();

                if (checkPosition > -1)
                    for (int i = 0; i < dataGridView.RowCount; i++)
                        dataGridView[checkColumnName, i].Value = false;
                dataGridView.RefreshEdit();
             
                TSBSelectAll.Text = Strings.SelectAll;
                TSBSelectAll.Image = Resources.Checkbox16;
                SelectedAll = false;

            }
            else
            {
                if (!SOSMode) dataGridView.SelectAll();
                noChecked.Clear();
                if (checkPosition > -1)
                    for (int i = 0; i < dataGridView.RowCount; i++)
                        ((DataGridViewCheckBoxCell)dataGridView[checkColumnName, i]).Value = true;
                dataGridView.RefreshEdit();

                SelectedAll = true;
                TSBSelectAll.Text = Strings.DeselectAll;
                TSBSelectAll.Image = Resources.UnCheckbox16;

            } 
         
        }

        //---------------------------------------------------------------------
        public void SetModifierIDOnRow(int workerId = -1)
        {

            foreach (DataRow currentrow in CurrentBoundRows)
            {
                int i = (int)currentrow[CommonStrings.ArchivedDocID];
                foreach (DataRow dr in DataSource.Rows)
                {
                    if ((int)dr[CommonStrings.ArchivedDocID] == i)
                    {
                        dr[CommonStrings.ModifierID] = workerId;
                        currentrow[CommonStrings.ModifierID] = workerId;
                    }
                }
            }
            DataSource.AcceptChanges();
        }

        //---------------------------------------------------------------------
        public void RefreshRowData()
        {
            foreach (DataRow currentrow in CurrentBoundRows)
            {
                int i = (int)currentrow[CommonStrings.ArchivedDocID];
                foreach (DataRow dr in DataSource.Rows)
                {
                    if ((int)dr[CommonStrings.ArchivedDocID] == i)
                    {
                        dr[CommonStrings.ModifierID] = -1;
                        currentrow[CommonStrings.ModifierID] = -1;

                        dr[CommonStrings.TBModified] = DateTime.Now.ToString();
                        currentrow[CommonStrings.TBModified] = DateTime.Now.ToString();

                        string w = dmsOrchestrator.GetCurrentWorkerName();

                        dr[CommonStrings.WorkerName] = w;
                        currentrow[CommonStrings.WorkerName] = w;
                    }
                }
            }
            DataSource.AcceptChanges();
        }

        //--------------------------------------------------------------------------------
        private List<AttachmentInfo> GetCurrentDocuments()
        {

            List<AttachmentInfo> currentDocuments = new List<AttachmentInfo>();
            if (CurrentBoundRows == null || CurrentBoundRows.Count == 0)
                return currentDocuments;
            foreach (DataRow CurrentBoundRow in CurrentBoundRows)
            {
                AttachmentInfo ai = GetAttachmentInfo(CurrentBoundRow);
                currentDocuments.Add(ai);

            }
            return currentDocuments;
        }

        //--------------------------------------------------------------------------------
        void CheckIn()
        {
            foreach (AttachmentInfo ai in GetCurrentDocuments())
                if (dmsOrchestrator.CheckIn(ai))
                    RefreshRowData();
            if (SelectionChanged != null) SelectionChanged(CommonStrings.CheckinSender, null);

        }

        //--------------------------------------------------------------------------------
        void Undo()
        {
            foreach (AttachmentInfo ai in GetCurrentDocuments())
                if (dmsOrchestrator.Undo(ai))
                    SetModifierIDOnRow();

        }

        //--------------------------------------------------------------------------------
        void CheckOut()
        {
            foreach (AttachmentInfo ai in GetCurrentDocuments())
                if (dmsOrchestrator.CheckOut(ai))
                {
                    OpenDocument(ai);
                    SetModifierIDOnRow(dmsOrchestrator.WorkerId);
                }

        }

        //todo, è ripetuta qua e la, unifica?
        //--------------------------------------------------------------------------------
        private void OpenDocument(AttachmentInfo currentAttachment)
        {
            if (currentAttachment == null)
                return;

            try
            {
                currentAttachment.OpenDocument();
            }
            catch (Exception exc)
            {
                documentGridDiagnostic.SetError(exc.ToString());
                DiagnosticViewer.ShowDiagnosticAndClear(documentGridDiagnostic);
            }
        }

        //---------------------------------------------------------------------
        private void TSMICheckout_Click(object sender, EventArgs e)
        {
            CheckOut();
        }

        //---------------------------------------------------------------------
        private void TSMICheckIn_Click(object sender, EventArgs e)
        {
            CheckIn();

        }

        //---------------------------------------------------------------------
        private void TSMIUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        //---------------------------------------------------------------------
        private void dataGridView_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            if (SOSMode) //|| !dmsOrchestrator.SettingsManager.UsersSettingState.Options.RepositoryOptionsState.EnableCheckInCheckOut)
            {
                TSMICheckIn.Visible = TSMIUndo.Visible = TSMICheckout.Visible = false;
                return;
            }

            DataGridViewCell cell = this.dataGridView[CommonStrings.ModifierID, e.RowIndex];
            int cellValue = (cell.Value == DBNull.Value) ? -1 : (int)cell.Value;
            bool checkedout = (cellValue > -1);
            if (checkedout && cellValue != dmsOrchestrator.WorkerId)//altro utente non posso fare ne checkin ne chekuoi o undo
            {
                TSMICheckIn.Visible = TSMIUndo.Visible = TSMICheckout.Visible = false;
            }
            else
            {
                TSMICheckIn.Visible = TSMIUndo.Visible = checkedout;
                TSMICheckout.Visible = !checkedout;
            }
        }


        //---------------------------------------------------------------------
        internal void LabelNoDocInVisible()
        {
            SafeGui.ControlVisible(LblNoItem, false);
        }

        //---------------------------------------------------------------------
        internal void CommitEdit()
        {
            dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

        }


        Hashtable columnorder = new Hashtable();

        //---------------------------------------------------------------------
        public void dataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ( dataGridView.Columns[e.ColumnIndex].Name == checkColumnName) 
                return;//colonna non esistente nel datatable
            
            GetCheckedRows();
            List<DataRow> checkedRows = CheckedRows;

            bool ASC = true;
            if (columnorder.Contains(e.ColumnIndex))
            {
                ASC = (bool)columnorder[e.ColumnIndex];
                columnorder[e.ColumnIndex] = !ASC;

            }

            else columnorder.Add(e.ColumnIndex, !ASC);
                    string orderval = ASC? " Asc":" desc";

            //ordino tutto il datatable, altrimenti di default  con la paginazioe attuale ordinerebbe solo la pagina corrente
            DataView dv = dtSource.DefaultView;
            dv.Sort = dataGridView.Columns[e.ColumnIndex].DataPropertyName + orderval ;
          
            dtSource = dv.ToTable();
            
            Paging();
            SetCheckedRows();
        }
       
    }

	//================================================================================
	public class DocumentEventArgs : EventArgs
	{
		public DocumentEventArgs(AttachmentInfo id) { this.id = id; }
		public AttachmentInfo id { get; set; }
	}
}
