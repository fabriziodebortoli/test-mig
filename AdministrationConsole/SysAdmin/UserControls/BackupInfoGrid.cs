using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	///<summary>
	/// UserControl con il datagridview che contiene le informazioni dei vari file di backup
	///</summary>
	//================================================================================
	public partial class BackupInfoGrid : UserControl
	{
		// Columns definition
		private DataGridViewImageColumn backupDBTypeColumn = null;
		private DataGridViewCheckBoxColumn selectedColumn = null;
		private DataGridViewTextBoxColumn dbNameColumn = null;
		private DataGridViewTextBoxColumn bakPathColumn = null;
		private DataGridViewButtonColumn browseColumn = null;

		// properties per esporre fuori le info del DataGrid
		//--------------------------------------------------------------------------------
		public DataGridView BakDataGridView_Control { get { return BakDataGridView; } }
		public object BakDataGridView_DataSource { get { return BakDataGridView.DataSource; } set { BakDataGridView.DataSource = value; } }

		// Events
		//--------------------------------------------------------------------------------
		public EventHandler AfterBackupFileChanged;

		//--------------------------------------------------------------------------------
		public BackupInfoGrid()
		{
			InitializeComponent();

			BakDataGridView.AutoGenerateColumns = false;
			BakDataGridView.Columns.Clear();
		}

		//--------------------------------------------------------------------------------
		private void BackupInfoGrid_Load(object sender, EventArgs e)
		{
			if (!DesignMode)
				DefineDataGridStyle();
		}

		///<summary>
		/// Definisco lo stile delle colonne del DataGrid
		///</summary>
		//--------------------------------------------------------------------------------
		private void DefineDataGridStyle()
		{
			// Immagine per il tipo di database
			backupDBTypeColumn = new DataGridViewImageColumn();
			backupDBTypeColumn.Name = DataTableConsts.BackupDBType;
			backupDBTypeColumn.DataPropertyName = DataTableConsts.BackupDBType;
			backupDBTypeColumn.HeaderText = string.Empty;
			backupDBTypeColumn.Width = 25;
			backupDBTypeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			backupDBTypeColumn.ReadOnly = true;

			Assembly plugInsAssembly = typeof(PlugIn).Assembly;
			Image image = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".MagoNet16.png"));
				if (image != null)
					backupDBTypeColumn.DefaultCellStyle.NullValue = image;

			BakDataGridView.Columns.Add(backupDBTypeColumn);
		
			// Selected
			selectedColumn = new DataGridViewCheckBoxColumn();
			selectedColumn.Name = DataTableConsts.Selected;
			selectedColumn.DataPropertyName = DataTableConsts.Selected;
			selectedColumn.HeaderText = string.Empty;
			selectedColumn.Width = 35;
			selectedColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			BakDataGridView.Columns.Add(selectedColumn);

			// Nome database
			dbNameColumn = new DataGridViewTextBoxColumn();
			dbNameColumn.Name = DataTableConsts.DBName;
			dbNameColumn.DataPropertyName = DataTableConsts.DBName;
			dbNameColumn.HeaderText = Strings.Database;
			dbNameColumn.Width = 250;
			dbNameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			dbNameColumn.ReadOnly = true;
			dbNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			BakDataGridView.Columns.Add(dbNameColumn);

			// Path file di backup
			bakPathColumn = new DataGridViewTextBoxColumn();
			bakPathColumn.Name = DataTableConsts.BakPath;
			bakPathColumn.DataPropertyName = DataTableConsts.BakPath;
			bakPathColumn.HeaderText = Strings.BackupFilePath;
			bakPathColumn.Width = 400;
			bakPathColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			bakPathColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			BakDataGridView.Columns.Add(bakPathColumn);

			// Pulsante browse
			browseColumn = new DataGridViewButtonColumn();
			browseColumn.Name = DataTableConsts.Browse;
			browseColumn.DataPropertyName = DataTableConsts.Browse;
			browseColumn.HeaderText = string.Empty;
			browseColumn.Width = 30;
			browseColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			browseColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			BakDataGridView.Columns.Add(browseColumn);
		}

		///<summary>
		/// Intercetto l'evento di click sulla cella con il pulsante di Browse
		///</summary>
		//--------------------------------------------------------------------------------
		private void BakDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == -1 || e.RowIndex == -1)
				return;

			DataGridViewCell cell = BakDataGridView[e.ColumnIndex, e.RowIndex];
			DataGridViewRow row = BakDataGridView.Rows[e.RowIndex];

			// se ho effettuato il click sul pulsante browse apro la OpenFileDialog
			if (BakDataGridView.Columns[e.ColumnIndex].Name.Equals(DataTableConsts.Browse))
			{
				OpenFileDialog fileDlg = new OpenFileDialog();

				fileDlg.Title = Strings.OpenBackupFileDlgTitle;
				if (!string.IsNullOrWhiteSpace(row.Cells[DataTableConsts.BakPath].Value.ToString()))
					fileDlg.InitialDirectory = Path.GetDirectoryName(row.Cells[DataTableConsts.BakPath].Value.ToString());

				fileDlg.FileName = row.Cells[DataTableConsts.DBName].Value.ToString() + ".bak";
				fileDlg.Multiselect = false;
				fileDlg.DefaultExt = "*.bak";
				fileDlg.CheckPathExists = true;
				fileDlg.CheckFileExists = false;
				fileDlg.Filter = "BAK files (*.bak)|*.bak|All files (*.*)|*.*";

				if (fileDlg.ShowDialog() == DialogResult.OK)
				{
					row.Cells[DataTableConsts.Selected].Value = true;
					row.Cells[DataTableConsts.BakPath].Value = fileDlg.FileName;

					// ho scelto un file, quindi aggiorno questo path anche nella classe di appoggio della riga
					BindingManagerBase bmb = this.BindingContext[BakDataGridView.DataSource, BakDataGridView.DataMember];
					DataRow dr = ((DataRowView)bmb.Current).Row;
					if (dr == null)
						return;

					BackupConnectionInfo bci = dr[DataTableConsts.BackupConnectionInfo] as BackupConnectionInfo;
					if (bci != null)
						bci.BackupFilePath = fileDlg.FileName;

					// ruoto l'evento a chi mi contiene per far capire che e' variata la cella con il path del backup
					if (AfterBackupFileChanged != null)
						AfterBackupFileChanged(this, EventArgs.Empty);
				}
			}
		}

		///<summary>
		/// Imposto dall'esterno il readonly sul pulsante di browse e sul path del file di backup
		///</summary>
		//--------------------------------------------------------------------------------
		public void SetCellsReadOnly()
		{
			foreach (DataGridViewRow dr in BakDataGridView.Rows)
			{
				dr.Cells[DataTableConsts.BakPath].ReadOnly = true;
				dr.Cells[DataTableConsts.Browse].ReadOnly = true;
			}
		}

		//--------------------------------------------------------------------------------
		private void BakDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			UpdateBackupFileOnCurrentRow(e);
		}

		//--------------------------------------------------------------------------------
		private void BakDataGridView_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			UpdateBackupFileOnCurrentRow(e);
		}

		//--------------------------------------------------------------------------------
		private void UpdateBackupFileOnCurrentRow(DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == -1 || e.RowIndex == -1)
				return;

			DataGridViewCell cell = BakDataGridView[e.ColumnIndex, e.RowIndex];
			DataGridViewRow row = BakDataGridView.Rows[e.RowIndex];

			// sto lasciando la cella con il path del backup
			if (BakDataGridView.Columns[e.ColumnIndex].Name.Equals(DataTableConsts.BakPath))
			{
				// se il path e' vuoto metto la riga non selezionata
				row.Cells[DataTableConsts.Selected].Value = !string.IsNullOrWhiteSpace(row.Cells[DataTableConsts.BakPath].Value.ToString());

				// aggiorno questo path anche nella classe di appoggio della riga
				BindingManagerBase bmb = this.BindingContext[BakDataGridView.DataSource, BakDataGridView.DataMember];
				DataRow dr = ((DataRowView)bmb.Current).Row;
				if (dr == null)
					return;

				BackupConnectionInfo bci = dr[DataTableConsts.BackupConnectionInfo] as BackupConnectionInfo;
				if (bci != null)
				{
					bci.BackupFilePath = row.Cells[DataTableConsts.BakPath].Value.ToString();
					row.Cells[DataTableConsts.BakPath].Value = bci.BackupFilePath;
				}

				// ruoto l'evento a chi mi contiene per far capire che e' variata la cella con il path del backup
				if (AfterBackupFileChanged != null)
					AfterBackupFileChanged(this, EventArgs.Empty);
			}
		}

		//--------------------------------------------------------------------------------
		private void BakDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			DataGridViewCell cell = BakDataGridView[e.ColumnIndex, e.RowIndex];
			DataGridViewRow row = BakDataGridView.Rows[e.RowIndex];

			if (BakDataGridView.Columns[e.ColumnIndex].Name.Equals(DataTableConsts.BackupDBType))
			{
				if (e.Value == null)
					return;

				BackupDBType cellValue = (BackupDBType)e.Value;

				Assembly plugInsAssembly = typeof(PlugIn).Assembly;

				switch (cellValue)
				{
					case BackupDBType.SYSDB:
					case BackupDBType.ERP:
						e.Value = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".MagoNet16.png"));
						break;
					case BackupDBType.DMS:
						e.Value = Image.FromStream(plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".EasyAttachment16.png"));
						break;
				}
			}
		}
	}
}
