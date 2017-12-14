using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form di archiviazione veloce
	///</summary>
	//================================================================================
	public partial class QuickAttachment : MenuTabForm
	{
		private DMSOrchestrator dmsOrchestrator = null;
		private DTCategories dtEnabledCategories = null;

		///<summary>
		/// Costruttore
		///</summary>
		//---------------------------------------------------------------------
		public QuickAttachment(DMSOrchestrator dmsOrch)
		{
			InitializeComponent();

			dmsOrchestrator = new DMSOrchestrator();
			dmsOrchestrator.InitializeManager(dmsOrch);
			dmsOrchestrator.InUnattendedMode = true;

			// mi faccio ritornare l'elenco delle categorie
			dtEnabledCategories = dmsOrchestrator.CategoryManager.GetCategories();
			for (int i = dtEnabledCategories.Rows.Count - 1; i >= 0; i--)
			{
				DataRow dr = dtEnabledCategories.Rows[i];
				if ((bool)dr[CommonStrings.Disable])
					dtEnabledCategories.Rows[i].Delete(); // levo le categorie con stato Disabled=true
			}

			DGVDocuments.AutoGenerateColumns = false;

			if (!DesignMode)
				InitDataGridView();
		}

		///<summary>
		/// Inizializzo il DataGrid con le colonne
		///</summary>
		//---------------------------------------------------------------------
		private void InitDataGridView()
		{
			DGVDocuments.Columns.Clear();

			Font f = new Font("Verdana", 8.25F, FontStyle.Underline);
			
			DataGridViewCellStyle blueVerdanaStyle = new DataGridViewCellStyle();
			blueVerdanaStyle.Font = f;
			blueVerdanaStyle.ForeColor = Color.MidnightBlue;

			// colonna Selected (non usata)
			/*DataGridViewCheckBoxColumn selectedColumn = new DataGridViewCheckBoxColumn();
			selectedColumn.Name = CommonStrings.Selected;
			selectedColumn.HeaderText = "To attach";
			selectedColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			selectedColumn.Width = 50;
			selectedColumn.Frozen = true;
			selectedColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			DGVDocuments.Columns.Add(selectedColumn);*/

			// colonna Immagine (idx = 0)
			DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
			imageColumn.Name = CommonStrings.Image;
			imageColumn.HeaderText = string.Empty;
			imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			imageColumn.Width = 30;
			imageColumn.Frozen = true;
			imageColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			DGVDocuments.Columns.Add(imageColumn);

			// colonna Documento (nome file) (idx = 1)
			DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn();
			fileNameColumn.Name = CommonStrings.Name;
			fileNameColumn.HeaderText = "Document";
			fileNameColumn.ReadOnly = true;
			fileNameColumn.Width = 300;
			fileNameColumn.Frozen = true;
			fileNameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			fileNameColumn.DefaultCellStyle = blueVerdanaStyle;
			DGVDocuments.Columns.Add(fileNameColumn);

			//---------------------------------------------------------------------------------------
			// N.B.: fino a questo punto le colonne sono frizzate

			// colonna PathFile (path file) NON VISIBILE! (idx = 2)
			DataGridViewTextBoxColumn pathFileColumn = new DataGridViewTextBoxColumn();
			pathFileColumn.Name = CommonStrings.Path;
			pathFileColumn.ReadOnly = true;
			pathFileColumn.Visible = false;
			DGVDocuments.Columns.Add(pathFileColumn);

			// colonna ArchivedDocID (utile per i documenti caricati da Repository) NON VISIBILE! (idx = 3)
			DataGridViewTextBoxColumn archivedDocIDColumn = new DataGridViewTextBoxColumn();
			archivedDocIDColumn.Name = CommonStrings.ArchivedDocID;
			archivedDocIDColumn.ReadOnly = true;
			archivedDocIDColumn.Visible = false;
			DGVDocuments.Columns.Add(archivedDocIDColumn);

			// colonna Descrizione (idx = 4)
			DataGridViewTextBoxColumn descriptionColumn = new DataGridViewTextBoxColumn();
			descriptionColumn.Name = CommonStrings.Description;
			descriptionColumn.HeaderText = Strings.Description;
			descriptionColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			descriptionColumn.Width = 400;
			descriptionColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			DGVDocuments.Columns.Add(descriptionColumn);

			// colonna Tags liberi (idx = 5)
			DataGridViewTextBoxColumn freeTagsColumn = new DataGridViewTextBoxColumn();
			freeTagsColumn.Name = CommonStrings.Tags;
			freeTagsColumn.HeaderText = "Free Tags";
			freeTagsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			freeTagsColumn.Width = 300;
			freeTagsColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			DGVDocuments.Columns.Add(freeTagsColumn);

			// colonna Tag Anno (idx = 6)
			DataGridViewTextBoxColumn yearTagColumn = new DataGridViewTextBoxColumn();
			yearTagColumn.Name = CommonStrings.FiscalYear;
			yearTagColumn.HeaderText = "Year";
			yearTagColumn.Width = 100;
			yearTagColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			DGVDocuments.Columns.Add(yearTagColumn);

			

			// per ogni categoria NON disabilitata creo una colonna 
			foreach (DataRow dr in dtEnabledCategories.Rows)
			{
				DataGridViewComboBoxColumn categorycolumn = new DataGridViewComboBoxColumn();
				categorycolumn.Name = dr[CommonStrings.Name].ToString();
				categorycolumn.HeaderText = dr[CommonStrings.Name].ToString();
				categorycolumn.SortMode = DataGridViewColumnSortMode.NotSortable;
				categorycolumn.FlatStyle = FlatStyle.Flat;
				categorycolumn.Width = 300;
				categorycolumn.MinimumWidth = 100;
				categorycolumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				categorycolumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

				// riempio i valori di default
				DTCategoriesValues categoryValues = dr[CommonStrings.ValueSet] as DTCategoriesValues;
				if (categoryValues != null)
					foreach (DataRow val in categoryValues.Rows)
						categorycolumn.Items.Add(val[CommonStrings.Value].ToString());
				
				DGVDocuments.Columns.Add(categorycolumn);
			}
		}

		///<summary>
		/// Intercetto il click per aprire il RepositoryBrowser
		///</summary>
		//---------------------------------------------------------------------
		private void TSRepository_Click(object sender, EventArgs e)
		{
		//	using (SafeThreadCallContext context = new SafeThreadCallContext())
		//	{
		//		RepositoryBrowser rep = new RepositoryBrowser(this, dmsOrchestrator);
				
		//		List<AttachmentInfo> attachmentInfos = rep.GetArchivedDocs(this);
		//		if (attachmentInfos != null && attachmentInfos.Count > 0)
		//		{
		//			foreach (AttachmentInfo ai in attachmentInfos)
		//				AddFileToRows(Path.Combine(ai.OriginalPath, ai.Name), ai);
		//		}
		//	}
		}

		///<summary>
		/// Intercetto il click per aprire l'Esplora Risorse
		///</summary>
		//---------------------------------------------------------------------
		private void TSFileSystem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.AutoUpgradeEnabled = true;
			openFileDialog.CheckFileExists = true;
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = String.Format("{0} (*.*)|*.*", Strings.AllFiles);

			if (openFileDialog.ShowDialog(this) != DialogResult.OK ||
				string.IsNullOrWhiteSpace(openFileDialog.FileName))
				return;

			for (int i = 0; i < openFileDialog.FileNames.Length; i++)
				AddFileToRows(openFileDialog.FileNames[i]);
		}

		///<summary>
		/// Intercetto il click per aprire il gestore dei Device
		///</summary>
		//---------------------------------------------------------------------
		private void TSDevice_Click(object sender, EventArgs e)
		{
			// bisogna fare cosi', altrimenti c'e' un bell'errore di ThreadSafeCall!
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				// demando al codice della form gli opportuni controlli prima di aprirla
				List<string> acquiredFileList = Acquisition.OpenForm(dmsOrchestrator.BarcodeManager);

				if (acquiredFileList != null && acquiredFileList.Count > 0)
				{
					this.Update();

					foreach (string file in acquiredFileList)
					{
						if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
							continue;

						AddFileToRows(file);
					}
				}
			}
		}

		///<summary>
		/// Intercetto il DragDrop sul DataGrid
		///</summary>
		//---------------------------------------------------------------------
		private void DGVDocuments_DragDrop(object sender, DragEventArgs e)
		{
			string[] fileArray = e.Data.GetData(DataFormats.FileDrop) as string[];

			if (fileArray == null)
				return;

			for (int i = 0; i < fileArray.Length; i++)
			{
				string file = fileArray[i];

				DirectoryInfo di = new DirectoryInfo(file);
				if (di.Exists)
					ExploreDirectory(di);
				else
					AddFileToRows(file);
			}
		}
		
		///<summary>
		/// Abilito il dragdrop nel datagrid solo per i file
		///</summary>
		//---------------------------------------------------------------------
		private void DGVDocuments_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;
			else
				e.Effect = DragDropEffects.None;
		}

		///<summary>
		/// Aggiungo una riga al datagrid per ogni file selezionato
		///</summary>
		//---------------------------------------------------------------------
		private void AddFileToRows(string filePath, AttachmentInfo ai = null)
		{
			//prima di aggiungere la riga devo controllare che il file non sia stato gia' aggiunto in precedenza
			foreach (DataGridViewRow dgvr in DGVDocuments.Rows)
				if (string.Compare(dgvr.Cells[CommonStrings.Path].Value.ToString(), filePath, StringComparison.InvariantCultureIgnoreCase) == 0)
					return;

			int i = DGVDocuments.Rows.Add();

			DGVDocuments.Rows[i].Cells[CommonStrings.Image].Value = (ai != null); // se ai = null metto icona trasparente
			DGVDocuments.Rows[i].Cells[CommonStrings.Name].Value = Path.GetFileName(filePath);
			DGVDocuments.Rows[i].Cells[CommonStrings.Path].Value = filePath;
			DGVDocuments.Rows[i].Cells[CommonStrings.ArchivedDocID].Value = (ai != null) ? ai.ArchivedDocId : -1;
			DGVDocuments.Rows[i].Cells[CommonStrings.Description].Value = (ai != null && !string.IsNullOrWhiteSpace(ai.Description)) ? ai.Description : TxtHeading.Text;
			DGVDocuments.Rows[i].Cells[CommonStrings.Tags].Value = string.Empty;
			DGVDocuments.Rows[i].Cells[CommonStrings.FiscalYear].Value = DateTime.Now.Year; // mettiamo l'anno corrente
			if (ai != null)
				DGVDocuments.Rows[i].Tag = ai;

			foreach (DataRow dr in dtEnabledCategories.Rows)
				DGVDocuments.Rows[i].Cells[dr[CommonStrings.Name].ToString()].Value = string.Empty;
		}

		///<summary>
		/// Metodo ricorsivo che scorre i folder e i subfolder ed aggiunge tutti i file trovati nel grid
		///</summary>
		//--------------------------------------------------------------------------------
		private void ExploreDirectory(DirectoryInfo dir, bool deep = false)
		{
			if (dir == null || !dir.Exists)
				return;

			if (
				dir.GetDirectories().Length > 0 && 
				(deep || MessageBox.Show(this, Strings.DeepAttach, Strings.ArchivedDocument, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				)
				foreach (DirectoryInfo f in dir.GetDirectories())
					ExploreDirectory(f, true);

			foreach (FileInfo f in dir.GetFiles())
				AddFileToRows(f.FullName);
		}

		///<summary>
		/// Elimino le righe selezionate dal grid
		///</summary>
		//--------------------------------------------------------------------------------
		private void TSDelete_Click(object sender, EventArgs e)
		{
			if (DGVDocuments.SelectedRows.Count == 0)
				return;

			foreach (DataGridViewRow dgvr in DGVDocuments.SelectedRows)
				DGVDocuments.Rows.Remove(dgvr);
		}

		///<summary>
		/// Pulisco tutta la griglia del grid
		///</summary>
		//--------------------------------------------------------------------------------
		private void TSClearAll_Click(object sender, EventArgs e)
		{
			DGVDocuments.Rows.Clear();
		}

		///<summary>
		/// Apro la form per la modifica multipla
		///</summary>
		//--------------------------------------------------------------------------------
        private void TSModify_Click(object sender, EventArgs e)
        {
			if (DGVDocuments.SelectedRows.Count == 0)
				return;
	
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				// richiamo la form con metodo statico
				ModificationSelections selections = ModifySelectedRows.OpenForm(dtEnabledCategories);
				// vado ad applicare il risultato alle righe selezionate (decidere se andare in append sulle stringhe)
				if (selections == null)
					return;

				ApplyModificationSelectionsToRows(selections);
			}
        }

		///<summary>
		/// Vado ad applicare la modifica multipla alle righe selezionate
		/// (al momento vado in append all'esistente, a parte per le categorie che sovrascrivo solo se vuote in origine)
		/// Forse vale la pena mettere un flag
		///</summary>
		//--------------------------------------------------------------------------------
		private void ApplyModificationSelectionsToRows(ModificationSelections selections)
		{
			foreach (DataGridViewRow dgvr in DGVDocuments.SelectedRows)
			{
				// vado in append per la descrizione ed i free tags
				string description = dgvr.Cells[CommonStrings.Description].Value.ToString();
				if (string.IsNullOrWhiteSpace(description))
					dgvr.Cells[CommonStrings.Description].Value = selections.Description;
				else
					dgvr.Cells[CommonStrings.Description].Value += " " + selections.Description;

				string tags = dgvr.Cells[CommonStrings.Tags].Value.ToString();
				if (string.IsNullOrWhiteSpace(tags))
					dgvr.Cells[CommonStrings.Tags].Value = selections.FreeTags;
				else
					dgvr.Cells[CommonStrings.Tags].Value += " " + selections.FreeTags;

				// sovrascrivo il fiscal year solo se e' valorizzato
				if (!string.IsNullOrWhiteSpace(selections.Year))
					dgvr.Cells[CommonStrings.FiscalYear].Value = selections.Year;
	
				// se presente lascio il valore originale della categoria, altrimenti lo inserisco
				for (int i = 0; i < dtEnabledCategories.Rows.Count; i++)
				{
					DataRow dr = dtEnabledCategories.Rows[i];
					string categoryName = dr[CommonStrings.Name].ToString();

					string categoryValue = dgvr.Cells[categoryName].Value.ToString();
					if (string.IsNullOrWhiteSpace(categoryValue))
						dgvr.Cells[categoryName].Value = selections.CategoryValues[i];
				}
			}
		}

		///<summary>
		/// Valorizzo l'icona della prima colonna
		///</summary>
		//--------------------------------------------------------------------------------
		private void DGVDocuments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// colonna dell'icona che indica se è archiviato
			if (e.ColumnIndex == 0)
			{
				if (e.Value == null)
					return;

				DataGridViewCell cell = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex];
				bool cellValue = (bool)e.Value;
				e.Value = (cellValue) ? Resources.paperclip16 : Resources.Transparent;
			}
		}

		//--------------------------------------------------------------------------------
		private void OpenDocument(DataGridViewCellEventArgs e)
		{ 
			// se la colonna non e' il nome file non procedo
			if (e.ColumnIndex != 1)
				return;

			try
			{
				DataGridViewRow currentRow = DGVDocuments.Rows[e.RowIndex];
				if (currentRow.Tag != null)
				{
					AttachmentInfo ai = (AttachmentInfo)currentRow.Tag;
                    ai.OpenDocument();
				}
				else
				{
					// andiamo a prendere il valore della colonna path che e' nascosta
					DataGridViewCell cell = DGVDocuments[2, e.RowIndex];
					Process.Start(cell.Value.ToString());
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}
		}

		///<summary>
		/// Sul doppioclick della colonna FileName apro la preview con il programma predefinito
		///</summary>
		//--------------------------------------------------------------------------------
		private void DGVDocuments_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			OpenDocument(e);
		}

		///<summary>
		/// Sul click della colonna FileName apro la preview con il programma predefinito
		///</summary>
		//--------------------------------------------------------------------------------
		private void DGVDocuments_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			OpenDocument(e);
		}

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHeading.Text))
                return;

            foreach(DataGridViewRow row in DGVDocuments.Rows)
                row.Cells[CommonStrings.Description].Value =  TxtHeading.Text;
        }
	}
}
