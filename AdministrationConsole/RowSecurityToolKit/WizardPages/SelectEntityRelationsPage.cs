using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per la scelta delle tabelle/colonne che hanno una relazione con l'entita'
	///</summary>
	//================================================================================
	public partial class SelectEntityRelationsPage : InteriorWizardPage
	{
		private Font verdanaFont = new Font("Verdana", 8.25F);

		private RSSelections rsSelections = null;
		private Dictionary<string, List<List<CatalogColumn>>> colsDict;

		private List<CatalogTableEntry> filteredTables; // lista tabelle prive della mastertable (ad uso dei controls della form)

		//--------------------------------------------------------------------------------
		public SelectEntityRelationsPage()
		{
			InitializeComponent();
		}

		// viene richiamata tutte le volte che visualizzo la pagina
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			rsSelections = ((RSWizard)this.WizardManager).Selections;

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			// se non ho selezionato almeno una riga nel datagrid non procedo
			if (!ExistSelectedRow())
				return WizardForm.NoPageChange;

			// se ci sono dei valori nelle combobox vuoti visualizzo un msg di avvertimento
			if (!CheckColumnsComboBox())
			{
				if (DiagnosticViewer.ShowQuestion(Strings.ColumnsMissing, string.Empty) == DialogResult.No)
					return WizardForm.NoPageChange;
			}

			GetControlsValue();

			// se non ci sono selezioni valide non procedo
			if (rsSelections.RelatedTablesList.Count == 0)
			{
				DiagnosticViewer.ShowWarning(Strings.NoValidSelections, string.Empty);
				return WizardForm.NoPageChange;
			}

			return (rsSelections.PrioritiesDictionary.Count > 1) ? WizardForm.NextPage : "SummaryPage";
		}

		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			DGVRelations.AutoGenerateColumns = false;
			if (!DesignMode)
				DesignDGVStyle();

			EncryptFilesCheckBox.Checked = rsSelections.EncryptFiles;

			SelDeselCBox.CheckState = CheckState.Unchecked;
			SelDeselCBox.Text = (SelDeselCBox.CheckState == CheckState.Checked) ? Strings.DeselectAllRows : Strings.SelectAllRows;

			// mostro la MasterTable e le colonne di riferimento selezionate
			LblMasterTable.Text = string.Format(Strings.MasterTable, rsSelections.MasterTable);
			string columnsText = Strings.SelectedColumns;
			foreach (CatalogColumn cc in rsSelections.MasterTblColumns)
			{
				columnsText += string.Format(" {0} {1} ", cc.Name, cc.DataTypeName);
				columnsText += cc.HasLength() ? string.Format("({0}) ; ", cc.ColumnSize.ToString()) : ";";
			}
			LblColumns.Text = columnsText.Substring(0, columnsText.Length - 2);

			// copio l'array delle tabelle e poi lo epuro levando la MasterTable
			filteredTables = rsSelections.RegisteredTablesList.ToList();
			for (int i = filteredTables.Count - 1; i >= 0; i--)
			{
				if (string.Compare(filteredTables[i].TableName, rsSelections.MasterTable, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					filteredTables.RemoveAt(i);
					break;
				}
			}

			// riempio una struttura con le tabelle + colonne piu' prossime a quelle selezionate
			// ed inizializzo il DataGrid con i valori letti
			LoadTblAndColReferences();

			// dopo che ho caricato nel grid le tabelle + colonne piu' prossime
			// devo andare ad arricchire il datagrid con le selezioni effettuate dall'utente, pre-impostando
			// le colonne ed eventualmente inserendo nuove righe
			if (rsSelections.RelatedTablesList != null && rsSelections.RelatedTablesList.Count > 0)
				FillUserSelectionsInDGV();
		}

		///<summary>
		/// leggo le selezioni fatte dall'utente e le memorizzo in memoria
		///</summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			rsSelections.EncryptFiles = EncryptFilesCheckBox.Checked;
			rsSelections.RelatedTablesList = new List<RSRelatedTable>();

			// aggiungo d'ufficio al dictionary la tabella master e le sue colonne 
			// deciso con Anna in data 08/10/2013: non si tratta di una duplicazione, ma serve nel caso di
			// piu' entita' "spalmate" sulla stessa tabella
			List<string> columnsList = rsSelections.MasterTblColumns.Select(column => column.Name).ToList(); // estraggo il nome dall'oggetto CatalogColumn

			RSRelatedTable masterRelatedTable = new RSRelatedTable(rsSelections.MasterTableNamespace);
			masterRelatedTable.ColumnsList.Add(columnsList);
			rsSelections.RelatedTablesList.Add(masterRelatedTable);

			for (int i = 0; i < DGVRelations.Rows.Count; i++)
			{
				string tblName;

				// skippo la NewRow
				if (DGVRelations.Rows[i].IsNewRow)
					continue;

				// skippo le righe non selezionate
				DataGridViewCheckImageButtonCell imageCell = DGVRelations.Rows[i].Cells[ConstStrings.Selected] as DataGridViewCheckImageButtonCell;
				if (imageCell != null && imageCell.ButtonState == PushButtonState.Normal) 
					continue;

				// controllo la cella con il nome tabella
				DataGridViewComboBoxCell tableCell = DGVRelations.Rows[i].Cells[ConstStrings.TableName] as DataGridViewComboBoxCell;
				if (tableCell == null || tableCell.Value == null)
					continue;
				else
				{
					// leggo il nome della tabella, se e' vuoto skippo la riga
					tblName = ((CatalogTableInfo)tableCell.Value).TableName;
					if (string.IsNullOrWhiteSpace(tblName))
						continue;
				}

				// ri-definisco una lista di colonne (uso la stessa lista)
				columnsList = new List<string>();

				// faccio un loop sulle celle con le colonne, partendo dall'indice 2 (le prime due colonne sono escluse)
				for (int k = 2; k < DGVRelations.Rows[i].Cells.Count; k++)
				{
					DataGridViewComboBoxCell colCell = DGVRelations.Rows[i].Cells[k] as DataGridViewComboBoxCell;
					if (colCell == null || colCell.Value == null)
						continue;
					else
					{
						// leggo il nome della colonna, se e' vuoto non procedo neppure ad analizzare le celle successive
						string colName = colCell.Value.ToString();
						if (string.IsNullOrWhiteSpace(colName))
							break;
						else
							columnsList.Add(colName);
					}
				}

				if (columnsList.Count > 0)
				{
					// identifico il namespace della tabella
					string tblNs = filteredTables.Find(table => string.Compare(table.TableName, tblName, StringComparison.InvariantCultureIgnoreCase) == 0).Namespace;
					if (string.IsNullOrWhiteSpace(tblNs))
						continue;

					// se il nr di colonne che mi aspetto e' coerente le aggiungo (il nome della tabella puo' essere ripetuto dall'11/04/17)
					if (rsSelections.MasterTblColumns.Count == columnsList.Count)
					{
						RSRelatedTable relTbl = rsSelections.RelatedTablesList.Find(ns => string.Compare(ns.TableNamespace, tblNs, StringComparison.InvariantCultureIgnoreCase) == 0);
						if (relTbl == null)
						{
							relTbl = new RSRelatedTable(tblNs);
							rsSelections.RelatedTablesList.Add(relTbl);
						}
						relTbl.ColumnsList.Add(columnsList);
					}
				}
			}
		}

		///<summary>
		/// Controllo le selezioni effettuate prima di passare alla pagina successiva
		/// L'utente deve aver selezionato almeno una riga
		///</summary>
		//---------------------------------------------------------------------
		private bool ExistSelectedRow()
		{
			bool atLeastOneSelected = false;

			for (int i = 0; i < DGVRelations.Rows.Count; i++)
			{
				// skippo la NewRow
				if (DGVRelations.Rows[i].IsNewRow)
					continue;

				DataGridViewCheckImageButtonCell imageCell = DGVRelations.Rows[i].Cells[ConstStrings.Selected] as DataGridViewCheckImageButtonCell;
				if (imageCell != null && imageCell.ButtonState == PushButtonState.Hot)
				{
					atLeastOneSelected = true;
					break;
				}
			}

			if (!atLeastOneSelected)
			{
				DiagnosticViewer.ShowWarning(Strings.SelectAtLeastOneRow, string.Empty);
				return false;
			}

			return true;
		}

		///<summary>
		/// Controllo le selezioni effettuate prima di passare alla pagina successiva
		/// Se esistono colonne selezionate che hanno dei valori nelle combo vuote, visualizzo
		/// un warning
		///</summary>
		//---------------------------------------------------------------------
		private bool CheckColumnsComboBox()
		{
			for (int i = 0; i < DGVRelations.Rows.Count; i++)
			{
				// skippo la NewRow
				if (DGVRelations.Rows[i].IsNewRow)
					continue;

				DataGridViewCheckImageButtonCell imageCell = DGVRelations.Rows[i].Cells[ConstStrings.Selected] as DataGridViewCheckImageButtonCell;
				if (imageCell != null && imageCell.ButtonState == PushButtonState.Hot)
				{
					// scorro le colonne della riga selezionata
					for (int k = 1; k < DGVRelations.Rows[i].Cells.Count; k++)
					{
						DataGridViewComboBoxCell comboCell = DGVRelations.Rows[i].Cells[k] as DataGridViewComboBoxCell;
						if (comboCell == null)
							continue;
						if (k == 1) // combo tabella
						{
							CatalogTableInfo cti = comboCell.Value as CatalogTableInfo;
							if (cti != null && string.IsNullOrWhiteSpace(cti.TableName))
								return false;
						}

						if (k > 1) // combo colonne 
							if (comboCell.Value == null || string.IsNullOrWhiteSpace(comboCell.Value.ToString()))
								return false;
					}
				}
			}

			return true;
		}

		///<summary>
		/// In base alle colonne che ho selezionato nella pagina precedente vado a cercare tutte
		/// le tbl + colonne che potrebbero avere attinenza con la master table e relative colonne prescelte
		/// Riempio un dictionary con le informazioni necessarie
		///</summary>
		//---------------------------------------------------------------------
		private void LoadTblAndColReferences()
		{
			// riempio il dictionary con l'elenco delle tabelle + colonne che hanno attinenza con 
			// con quelle selezionate in precedenza
			colsDict = new Dictionary<string, List<List<CatalogColumn>>>();

			// per ogni colonna selezionata nell'entita' vado a cercare in tutte le tabelle del database
			// le colonne dello stesso tipo e le metto una lista
			for (int i = 0; i < rsSelections.MasterTblColumns.Count; i++)
			{
				CatalogColumn cc = rsSelections.MasterTblColumns[i];

				foreach (CatalogTableEntry cte in filteredTables)
				{
					// mi faccio ritornare solo la colonna con lo stesso nome e datatype (se esiste)
					CatalogColumn colSameName = cte.GetColumnByNameAndDataType(cc.Name, cc.DataTypeName);

					// carico tutte le colonne con lo stesso tipo e metto Selected la colonna predefinita
					List<CatalogColumn> colsByDataType = cte.LoadColumnsByDataType(cc.DataTypeName);
					if (colSameName != null)
					{
						foreach (CatalogColumn c in colsByDataType)
						{
							if (string.Compare(c.Name, colSameName.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
								c.Selected = true;
						}
					}

					List<List<CatalogColumn>> listOfList;
					if (colsDict.TryGetValue(cte.TableName, out listOfList))
					{
						if (colSameName != null)
						{
							List<CatalogColumn> a = new List<CatalogColumn>();
							a.Add(colSameName);
							listOfList.Add(a);
						}
					}
					else
					{
						if (colSameName != null)
						{
							listOfList = new List<List<CatalogColumn>>();
							List<CatalogColumn> a = new List<CatalogColumn>();
							a.Add(colSameName);
							listOfList.Add(a);
							colsDict.Add(cte.TableName, listOfList);
						}
					}
				}
			}

			// scremo il contenuto del dictionary considerando solo le tabelle che hanno
			// tutte le reference columns (e che saranno sicuramente un sottoinsieme).
			// per ottenerle controllo il numero di liste trovate con il numero delle colonne che mi aspetto
			// se ne ho un numero minore le elimino
			colsDict.ToList().Where(pair => ((List<List<CatalogColumn>>)pair.Value).Count
				< rsSelections.MasterTblColumns.Count).ToList().ForEach(pair => colsDict.Remove(pair.Key));

			// inizializzo i valori nel DataGrid sulla base delle selezioni effettuate
			if (colsDict.Count > 0)
				InitValuesToDataGrid();
		}

		/// <summary>
		/// Inizializzo le righe del DataGrid con i valori piu' prossimi, utilizzando le strutture 
		/// che ho riempito in precedenza in memoria
		/// </summary>
		//---------------------------------------------------------------------
		private void InitValuesToDataGrid()
		{
			try
			{
				foreach (KeyValuePair<string, List<List<CatalogColumn>>> kvp in colsDict)
				{
					bool isHotRow = false;

					if (rsSelections.RelatedTablesList != null)
					{
						RSRelatedTable tbl = rsSelections.RelatedTablesList.Find
							(ns => string.Compare(new NameSpace(ns.TableNamespace, NameSpaceObjectType.Table).GetTokenValue(NameSpaceObjectType.Table), kvp.Key, StringComparison.InvariantCultureIgnoreCase) == 0);
						if (tbl != null)
							isHotRow = true;
					}

					int newRowIdx = DGVRelations.Rows.Add(); // nuova riga
					DataGridViewRow newRow = DGVRelations.Rows[newRowIdx];
					if (newRow == null)
						continue;

					// aggiungo la cella Selected alla riga
					((DataGridViewCheckImageButtonCell)(newRow.Cells[0])).ButtonState = isHotRow ? PushButtonState.Hot : PushButtonState.Normal;

					// aggiungo la cella Table alla riga
					DataGridViewComboBoxCell tableCell = newRow.Cells[1] as DataGridViewComboBoxCell;
					tableCell.DataSource = filteredTables;

					CatalogTableInfo cti = null;
					CatalogTableEntry cte = null;
					foreach (CatalogTableEntry e in filteredTables)
					{
						if (String.Compare(e.TableName, kvp.Key, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							cti = e.CatalogTableInfo;
							cte = e;
							break;
						}
					}

					tableCell.DisplayMember = ConstStrings.TableName;
					tableCell.ValueMember = ConstStrings.CatalogTableInfo;
					tableCell.Value = cti;

					// aggiungo N celle di tipo Column alla riga, caricando solo le colonne dello stesso DataType
					DataGridViewComboBoxCell colCell = null;

					for (int i = 0; i < kvp.Value.Count; i++)
					{
						List<CatalogColumn> item = kvp.Value[i];
						foreach (CatalogColumn cc in item)
						{
							colCell = (DataGridViewComboBoxCell)newRow.Cells[i + 2];
							colCell.DisplayMember = ConstStrings.Name;
							colCell.ValueMember = ConstStrings.Name;

							if (cti != null)
							{
								// carico tutte le colonne con lo stesso tipo e metto Selected la colonna predefinita
								List<CatalogColumn> colsByDataType = cte.LoadColumnsByDataType(cc.DataTypeName);
								colsByDataType.Sort(new CatalogColumnGenericComparer());
								colCell.DataSource = colsByDataType;

								foreach (CatalogColumn e in colsByDataType)
								{
									colCell.Tag = e;

									if (String.Compare(e.Name, rsSelections.MasterTblColumns[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
									{
										colCell.Value = e.Name;
										break;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Function, "SelectEntityRelationsPage::InitValuesToDataGrid");
				ei.Add(DatabaseLayerStrings.Library, "RowSecurityToolKit");
				((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic.Set(DiagnosticType.Error, Strings.ExceptionInfo, ei);
				DiagnosticViewer.ShowDiagnostic(((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic);
			}

			Refresh();
		}

		///<summary>
		/// Scorro le selezioni effettuate dall'utente e le vado ad integrare nel datagrid
		///</summary>
		//---------------------------------------------------------------------
		private void FillUserSelectionsInDGV()
		{
			try
			{
				// scorro le tabelle selezionate e vado a ricercare la corrispondente riga nel datagrid
				foreach (RSRelatedTable relatedTbl in rsSelections.RelatedTablesList)
				{
					bool rowFound = false;

					foreach (DataGridViewRow dgvr in this.DGVRelations.Rows)
					{
						if (dgvr.IsNewRow || string.Compare(relatedTbl.TableNamespace, rsSelections.MasterTableNamespace, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							if (dgvr.Cells[1].Value != null)
								rowFound = true; // skippo la riga ma non la devo aggiungere manualmente!
							continue;
						}

						CatalogTableInfo tblInfo = (CatalogTableInfo)dgvr.Cells[1].Value;
						if (tblInfo == null)
							continue;

						NameSpace namespaceTable = new NameSpace(relatedTbl.TableNamespace, NameSpaceObjectType.Table);
						string tableName = namespaceTable.GetTokenValue(NameSpaceObjectType.Table);

						// cerco nelle righe del datagrid se c'e' gia' una riga con il nome della tabella
						if (string.Compare(tblInfo.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							rowFound = true;
							DataGridViewCheckImageButtonCell imgCell = dgvr.Cells[0] as DataGridViewCheckImageButtonCell;
							if (imgCell != null)
								imgCell.ButtonState = PushButtonState.Hot;

							// se ho trovato la riga vado a valorizzare le N celle di tipo Column presenti nella riga

							// se la tabella e' stata ripetuta piu' volte devo inserire le righe manualmente
							if (relatedTbl.ColumnsList.Count > 1)
							{
								List<string> firstRowCols = relatedTbl.ColumnsList[0];
								for (int i = 0; i < firstRowCols.Count; i++)
								{
									string item = firstRowCols[i];
									DataGridViewComboBoxCell cCell = dgvr.Cells[i + 2] as DataGridViewComboBoxCell;
									cCell.Value = item;
								}

								AddNewRow(relatedTbl, true);
							}
							else
							{
								foreach (List<string> cols in relatedTbl.ColumnsList)
								{
									for (int i = 0; i < cols.Count; i++)
									{
										string item = cols[i];
										DataGridViewComboBoxCell cCell = dgvr.Cells[i + 2] as DataGridViewComboBoxCell;
										cCell.Value = item;
									}
								}
							}
							break;
						}
					}

					if (rowFound)
						continue;

					// se non ho trovato la riga la devo aggiungere manualmente
					AddNewRow(relatedTbl);
				}
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Function, "SelectEntityRelationsPage::FillUserSelectionsInDGV");
				ei.Add(DatabaseLayerStrings.Library, "RowSecurityToolKit");
				((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic.Set(DiagnosticType.Error, Strings.ExceptionInfo, ei);
				DiagnosticViewer.ShowDiagnostic(((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic);
			}

			Refresh();
		}

		///<summary>
		/// Metodo che consente di inserire nel DGV le righe che aveva aggiunto "manualmente" l'utente
		/// ovvero scegliendo nomi tabelle + colonne che non erano tra i valori proposti di default
		///</summary>
		//---------------------------------------------------------------------
		private void AddNewRow(RSRelatedTable relatedTbl, bool excludeFirstElement = false)
		{
			try
			{
				for (int z = 0; z < relatedTbl.ColumnsList.Count; z++)
				{
					if (excludeFirstElement && z == 0)
						continue;

					List<string> colsList = relatedTbl.ColumnsList[z];

					int newRowIdx = DGVRelations.Rows.Add();// nuova riga
					DataGridViewRow newRow = DGVRelations.Rows[newRowIdx];
					if (newRow == null) continue;

					// aggiungo la cella Selected alla riga
					DataGridViewCheckImageButtonCell imageCell = newRow.Cells[0] as DataGridViewCheckImageButtonCell;
					// imposto il PushButtonState.Hot per le selezioni che ho fatto in precedenza
					imageCell.ButtonState = PushButtonState.Hot;

					// aggiungo la cella Table alla riga
					DataGridViewComboBoxCell tableCell = newRow.Cells[1] as DataGridViewComboBoxCell;
					tableCell.DataSource = filteredTables;

					CatalogTableInfo cti = null;
					CatalogTableEntry cte = null;
					foreach (CatalogTableEntry e in filteredTables)
					{
						NameSpace namespaceTable = new NameSpace(relatedTbl.TableNamespace, NameSpaceObjectType.Table);
						string tableName = namespaceTable.GetTokenValue(NameSpaceObjectType.Table);

						if (string.Compare(e.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							cti = e.CatalogTableInfo;
							cte = e;
							break;
						}
					}

					tableCell.DisplayMember = ConstStrings.TableName;
					tableCell.ValueMember = ConstStrings.CatalogTableInfo;
					tableCell.Value = cti;

					// aggiungo N celle di tipo Column alla riga, caricando solo le colonne dello stesso DataType
					for (int i = 0; i < colsList.Count; i++)
					{
						DataGridViewComboBoxCell colCell = newRow.Cells[i + 2] as DataGridViewComboBoxCell;
						colCell.DisplayMember = ConstStrings.Name;
						colCell.ValueMember = ConstStrings.Name;

						string colName = colsList[i];

						if (cti != null)
						{
							CatalogColumn catCol = cte.GetColumnInfo(colName);
							string dt = string.Empty;
							if (catCol == null)
								dt = rsSelections.MasterTblColumns[i].DataTypeName;

							// carico tutte le colonne con lo stesso tipo
							List<CatalogColumn> colsByDataType = cte.LoadColumnsByDataType((dt == string.Empty) ? catCol.DataTypeName : dt);
							colsByDataType.Sort(new CatalogColumnGenericComparer());
							colCell.DataSource = colsByDataType;

							foreach (CatalogColumn e in colsByDataType)
							{
								if (String.Compare(e.Name, colName, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									colCell.Value = colName;
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, e.Message);
				ei.Add(DatabaseLayerStrings.Function, "SelectEntityRelationsPage::AddNewRow");
				ei.Add(DatabaseLayerStrings.Library, "RowSecurityToolKit");
				((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic.Set(DiagnosticType.Error, Strings.ExceptionInfo, ei);
				DiagnosticViewer.ShowDiagnostic(((RSWizard)this.WizardManager).EntityDiagnostic.Diagnostic);
			}
		}

		///<summary>
		/// Definisco lo stile delle colonne del DataGridView
		/// Aggiungo un numero variabile di colonne a seconda delle selezioni effettuate nella pagina precedente
		///</summary>
		//--------------------------------------------------------------------------------
		private void DesignDGVStyle()
		{
			DGVRelations.Columns.Clear();

			// imposto un font piu' piccolo allo stile delle celle delle DataGridView
			DataGridViewCellStyle verdanaCellStyle = new DataGridViewCellStyle();
			verdanaCellStyle.Font = verdanaFont;
			// anche negli header di colonna
			DGVRelations.ColumnHeadersDefaultCellStyle.Font = verdanaFont;
			DGVRelations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

			// Pulsante con immagine per il selected
			DataGridViewCheckImageButtonColumn selectedColumn = new DataGridViewCheckImageButtonColumn();
			selectedColumn.Name = ConstStrings.Selected;
			selectedColumn.DataPropertyName = ConstStrings.Selected;
			selectedColumn.HeaderText =  Strings.DGSelected;
			selectedColumn.ValueType = typeof(bool);
			selectedColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			selectedColumn.DefaultCellStyle = verdanaCellStyle;
			selectedColumn.Width = 28;
			DGVRelations.Columns.Add(selectedColumn);

			// Colonna Table
			DataGridViewComboBoxColumn tableColumn = new DataGridViewComboBoxColumn();
			tableColumn.Name = ConstStrings.TableName;
			tableColumn.DataPropertyName = ConstStrings.TableName;
			tableColumn.HeaderText = Strings.DGTableName;
			tableColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			tableColumn.DefaultCellStyle = verdanaCellStyle;
			tableColumn.Width = 250;
			DGVRelations.Columns.Add(tableColumn);

			// Colonne Column (ne aggiungo tante quante sono quelle scelte in precedenza)
			for (int i = 0; i < rsSelections.MasterTblColumns.Count; i++)
			{
				CatalogColumn cc = rsSelections.MasterTblColumns[i];
				DataGridViewComboBoxColumn columnColumn = new DataGridViewComboBoxColumn();
				columnColumn.Name = string.Concat(ConstStrings.ColumnName, i.ToString());
				columnColumn.DataPropertyName = string.Concat(ConstStrings.ColumnName, i.ToString());
				columnColumn.HeaderText = cc.Name;
				columnColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
				columnColumn.DefaultCellStyle = verdanaCellStyle;
				columnColumn.Width = 200;
				DGVRelations.Columns.Add(columnColumn);
			}
		}

		//--------------------------------------------------------------------------------
		private void DGVRelations_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("DataError " + e.Exception.ToString());
		}

		///<summary>
		/// Sul click della cella effettuo le seguenti operazioni:
		/// 1. per la colonna 0 imposto l'immagine checked/unchecked
		/// 2. per la colonna 1 imposto il DataSource della combobox con l'elenco delle tabelle del Catalog
		///</summary>
		//--------------------------------------------------------------------------------
		private void DGVRelations_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				// click sul pulsante checked/unchecked
				if (DGVRelations.CurrentCell.ColumnIndex == 0)
				{
					if (!(DGVRelations.CurrentCell is DataGridViewCheckImageButtonCell))
						return;

					// inizializzo l'immagine sul pulsante tramite il ButtonState
					PushButtonState currState = ((DataGridViewCheckImageButtonCell)(DGVRelations.CurrentRow.Cells[ConstStrings.Selected])).ButtonState;
					if (currState == PushButtonState.Hot)
						((DataGridViewCheckImageButtonCell)(DGVRelations.CurrentRow.Cells[ConstStrings.Selected])).ButtonState = PushButtonState.Normal;
					if (currState == PushButtonState.Normal)
						((DataGridViewCheckImageButtonCell)(DGVRelations.CurrentRow.Cells[ConstStrings.Selected])).ButtonState = PushButtonState.Hot;
				}

				// click sulla cella con la combo delle tabelle
				if (DGVRelations.CurrentCell.ColumnIndex == 1)
				{
					DataGridViewComboBoxCell tableCell = DGVRelations.CurrentCell as DataGridViewComboBoxCell;
					if (tableCell == null)
						return;
					tableCell.DataSource = filteredTables; // riempio di DataSource con l'elenco delle tabelle filtrate
					tableCell.DisplayMember = ConstStrings.TableName;
					tableCell.ValueMember = ConstStrings.CatalogTableInfo;
				}
			}
			catch
			{ }
		}

		///<summary>
		/// Al termine dell'edit della cella 1, ovvero quella con la combobox delle tabelle,
		/// vado ad assegnare il DataSource delle N possibili combobox delle colonne successive
		/// con l'elenco delle colonne lette dal catalog filtrate per DataType
		///</summary>
		//--------------------------------------------------------------------------------
		private void DGVRelations_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				// ho terminato di editare la colonna 1, ovvero quella con il nome della tabella
				// quindi vado a popolare le combobox successive con le colonne suddivise per datatype
				if (e.ColumnIndex == 1)
				{
					DataGridViewComboBoxCell dgcb = (DataGridViewComboBoxCell)DGVRelations[e.ColumnIndex, e.RowIndex];
					// estrapolo il CatalogTableInfo della cella
					CatalogTableInfo cti = dgcb.Value as CatalogTableInfo;
					if (cti == null)
						return;

					// prima di tutto estrapolo il CatalogTableEntry della tabella appena selezionata nella combobox
					CatalogTableEntry cte = null;
					foreach (CatalogTableEntry tableEntry in filteredTables)
					{
						if (String.Compare(tableEntry.TableName, ((CatalogTableInfo)dgcb.Value).TableName, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							cte = tableEntry;
							break;
						}
					}
					if (cte == null)
						return;

					// dictionary di appoggio che mi serve per tenere da parte gli array con le colonne filtrate
					// (faccio un dictionary in modo da non duplicare le liste a parita' di DataType)
					Dictionary<string, List<CatalogColumn>> dataTypeColsDict = new Dictionary<string, List<CatalogColumn>>();
					foreach (CatalogColumn refCol in rsSelections.MasterTblColumns)
					{
						// se NON contiene chiavi con quel DataType, 
						// carico la lista delle colonne dello stesso tipo e le aggiungo al dictionary
						if (!dataTypeColsDict.ContainsKey(refCol.DataTypeName))
							dataTypeColsDict.Add(refCol.DataTypeName, cte.LoadColumnsByDataType(refCol.DataTypeName));
					}

					// faccio un loop sulle celle della riga corrente partendo dall'indice 2 (le prime due colonne sono escluse)
					for (int i = 2; i < this.DGVRelations.Rows[e.RowIndex].Cells.Count; i++)
					{
						DataGridViewComboBoxCell colCell = this.DGVRelations.Rows[e.RowIndex].Cells[i] as DataGridViewComboBoxCell;
						if (rsSelections.MasterTblColumns.Count == 0)
							break;

						CatalogColumn catCol = rsSelections.MasterTblColumns[i - 2];
						List<CatalogColumn> filteredColumns;
						if (dataTypeColsDict.TryGetValue(catCol.DataTypeName, out filteredColumns))
						{
							filteredColumns.Sort(new CatalogColumnGenericComparer());
							colCell.DataSource = filteredColumns;
							colCell.DisplayMember = ConstStrings.Name;
							colCell.ValueMember = ConstStrings.Name;
						}
					}
				}

				if (e.ColumnIndex > 1)
				{
					DataGridViewComboBoxCell dgcb = (DataGridViewComboBoxCell)DGVRelations[1, e.RowIndex];
					// estrapolo il CatalogTableInfo della cella nella colonna 1
					CatalogTableInfo cti = dgcb.Value as CatalogTableInfo;
					if (cti == null)
						return;

					DataGridViewComboBoxCell colCell = (DataGridViewComboBoxCell)DGVRelations[e.ColumnIndex, e.RowIndex];
					if (colCell == null || colCell.DataSource == null)
						return;

					List<CatalogColumn> ds = colCell.DataSource as List<CatalogColumn>;

					foreach (CatalogColumn cc in ds)
					{
						if (cc.Name == colCell.Value.ToString())
							colCell.Tag = cc;
					}
				}
			}
			catch
			{ }
		}

		///<summary>
		/// Click sul pulsante Seleziona/Deseleziona tutto
		/// Seleziono tutte le righe (esclusa la new row, con l'asterisco)
		///</summary>
		//--------------------------------------------------------------------------------
		private void SelDeselCBox_CheckedChanged(object sender, EventArgs e)
		{
			((CheckBox)sender).Text = (((CheckBox)sender).CheckState == CheckState.Checked) ? Strings.DeselectAllRows : Strings.SelectAllRows;
			((CheckBox)sender).Image = (((CheckBox)sender).CheckState == CheckState.Checked) ? Strings.Check_unselected : Strings.Check_selected;

			for (int i = 0; i < DGVRelations.Rows.Count; i++)
			{
				DataGridViewCheckImageButtonCell imageCell = DGVRelations.Rows[i].Cells[ConstStrings.Selected] as DataGridViewCheckImageButtonCell;
				if (imageCell == null)
					continue;

				// skippo la NewRow
				if (DGVRelations.Rows[i].IsNewRow)
				{
					imageCell.ButtonState = PushButtonState.Normal;
					continue;
				}

				imageCell.ButtonState = (((CheckBox)sender).CheckState == CheckState.Checked) ? PushButtonState.Hot : PushButtonState.Normal;
			}

			DGVRelations.Refresh();
		}
	}
}
