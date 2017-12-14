using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina di riepilogo con le selezioni effettuate dall'utente
	///</summary>
	//================================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private EntityManager entityMng = null;
		private RSSelections rsSelections = null;
		private CatalogTableEntry cte;
		private List<string> filesToCheck = null;

		private Font fontBold;

		//--------------------------------------------------------------------------------
		public SummaryPage()
		{
			InitializeComponent();

			fontBold = new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Bold);
		}

		// viene richiamata tutte le volte che visualizzo la pagina
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			rsSelections = ((RSWizard)this.WizardManager).Selections;
			
			entityMng = new EntityManager(rsSelections);

			filesToCheck = new List<string>();

			// visualizzo informazioni generiche
			DisplayGenericSelections();

			// solo se non sto eliminando l'entita' devo controllare di avere a disposizione
			// le informazioni della MasterTable per poter andare avanti
			if (rsSelections.EntityAction != EntityAction.DELETE)
			{
				cte = rsSelections.GetRegisteredTableEntry(rsSelections.MasterTable);
				if (cte == null || string.IsNullOrWhiteSpace(cte.Application) || string.IsNullOrWhiteSpace(cte.Module))
				{
					this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.DisabledFinish);
					// faccio la Clear della text-box 
					SummaryRichTextBox.Clear();
					SummaryRichTextBox.AppendText(Strings.NoMasterTableInfo);
					return true;
				}
			}

			// a seconda dell'azione da fare visualizzo le informazioni
			switch (rsSelections.EntityAction)
			{
				case EntityAction.NEW:
				case EntityAction.EDIT:
					DisplayEntityInfo();
					DisplayInvolvedFilesInfo();
					break;
				case EntityAction.DELETE:
					DisplayEntityInfoToDelete();
					break;
			}

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);

			// trucco per fare in modo di impostare il cursore alla prima riga della textbox
			SummaryRichTextBox.SelectionStart = 1;
			SummaryRichTextBox.Focus();

			return true;
		}

		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			if (rsSelections.EntityAction == EntityAction.DELETE)
				return (rsSelections.PrioritiesDictionary.Count > 1) ? "SetEntityPrioritiesPage" : "ChooseEntityPage";

			return (rsSelections.PrioritiesDictionary.Count > 1) ? "SetEntityPrioritiesPage" : "SelectEntityRelationsPage";
		}

		///<summary>
		/// Mostra le info generiche nella textbox
		///</summary>
		//---------------------------------------------------------------------
		private void DisplayGenericSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			// inserisco righe di testo per fare un riassunto delle selezioni effettuate
			SummaryRichTextBox.SelectionFont = fontBold;

			SummaryRichTextBox.SelectionBullet = true;
			switch (rsSelections.EntityAction)
			{
				case EntityAction.NEW:
					SummaryRichTextBox.AppendText(string.Format(Strings.NewEntity, rsSelections.Entity));
					break;
				case EntityAction.EDIT:
					SummaryRichTextBox.AppendText(string.Format(Strings.EditEntity, rsSelections.Entity));
					break;
				case EntityAction.DELETE:
					SummaryRichTextBox.AppendText(string.Format(Strings.DeleteEntity, rsSelections.Entity));
					break;
			}
			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.SelectionBullet = true;
			if (!rsSelections.EntityDescription.IsNullOrEmpty())
			{
				SummaryRichTextBox.AppendText(string.Format(Strings.Description, rsSelections.EntityDescription));
				SummaryRichTextBox.AppendText("\r\n");
			}
			
			if (rsSelections.EncryptFiles)
			{
				SummaryRichTextBox.AppendText(Strings.EncryptFiles);
				SummaryRichTextBox.AppendText("\r\n");
			}

			SummaryRichTextBox.SelectionBullet = true;
			SummaryRichTextBox.AppendText(string.Format(Strings.MasterTable, rsSelections.MasterTable));
			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.SelectionBullet = true;
			SummaryRichTextBox.AppendText(string.Format(Strings.DocumentNamespace, rsSelections.DocumentNamespace));
			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.SelectionBullet = true;
			SummaryRichTextBox.AppendText(Strings.SelectedColumns);
			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionBullet = false;
			
			foreach (CatalogColumn cc in rsSelections.MasterTblColumns)
			{
				string columnsText = string.Format("\t {0} {1} ", cc.Name, cc.DataTypeName);
				if (!string.IsNullOrWhiteSpace(cc.DataTypeName))
					columnsText += cc.HasLength() ? string.Format("({0}) ; ", cc.ColumnSize.ToString()) : ";";
				else
					columnsText += ";";
				SummaryRichTextBox.AppendText(columnsText);
				SummaryRichTextBox.AppendText("\r\n");
			}
		}

		///<summary>
		/// Mostra le info dell'entity da creare o da modificare
		///</summary>
		//---------------------------------------------------------------------
		private void DisplayEntityInfo()
		{
			SummaryRichTextBox.SelectionBullet = false;
			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionFont = fontBold;
			SummaryRichTextBox.AppendText(Strings.TablesAndColsAssociated);
			SummaryRichTextBox.AppendText("\r\n");

			foreach (RSRelatedTable tbl in rsSelections.RelatedTablesList)
			{
				NameSpace namespaceTable = new NameSpace(tbl.TableNamespace, NameSpaceObjectType.Table);
				string tableName = namespaceTable.GetTokenValue(NameSpaceObjectType.Table);

				IBaseModuleInfo modi;
				RowSecurityObjectsInfo rsi = entityMng.GetRowSecurityObjectsInfo(tableName, out modi);
				if (rsi != null && !string.IsNullOrWhiteSpace(rsi.FilePath) && !filesToCheck.ContainsNoCase(rsi.FilePath))
				{
					filesToCheck.Add(rsi.FilePath);
					if (rsSelections.EncryptFiles)
					{
						string crsFile = Path.Combine(Path.GetDirectoryName(rsi.FilePath), Path.GetFileNameWithoutExtension(rsi.FilePath) + NameSolverStrings.CrsExtension);
						if (!filesToCheck.ContainsNoCase(crsFile))
							filesToCheck.Add(crsFile);
					}
				}

				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(Strings.Table + " " + tbl.TableNamespace);
				SummaryRichTextBox.AppendText("\r\n");
				SummaryRichTextBox.SelectionBullet = false;
				SummaryRichTextBox.AppendText("\t" + Strings.ColumnsColon + " ");

				for (int k = 0; k < tbl.ColumnsList.Count; k++)
				{
					List<string> cols = tbl.ColumnsList[k];
					for (int i = 0; i < cols.Count; i++)
					{
						string colName = cols[i];
						SummaryRichTextBox.AppendText((k > 0) ? ("; " + colName) : colName);
						if (i < cols.Count - 1)
							SummaryRichTextBox.AppendText("; ");
					}
				}

				SummaryRichTextBox.AppendText("\r\n");
			}
		}

		//---------------------------------------------------------------------
		private void DisplayInvolvedFilesInfo()
		{
			string rslFilePath = Path.Combine
						 (
						 BasePathFinder.BasePathFinderInstance.GetApplicationModuleObjectsPath(cte.Application, cte.Module),
						 NameSolverStrings.RowSecurityObjectsXml
						 );

			if (!filesToCheck.ContainsNoCase(rslFilePath))
			{
				filesToCheck.Add(rslFilePath);
				if (rsSelections.EncryptFiles)
				{
					string crsFile = Path.Combine(Path.GetDirectoryName(rslFilePath), Path.GetFileNameWithoutExtension(rslFilePath) + NameSolverStrings.CrsExtension);
					if (!filesToCheck.ContainsNoCase(crsFile))
						filesToCheck.Add(crsFile);
				}
			}

			SummaryRichTextBox.SelectionBullet = false;
			SummaryRichTextBox.AppendText("\r\n"); 
			SummaryRichTextBox.SelectionFont = fontBold;
			SummaryRichTextBox.AppendText(Strings.InvolvedFiles);
			SummaryRichTextBox.AppendText("\r\n"); 

			SummaryRichTextBox.SelectionBullet = true;
			foreach (string fileName in filesToCheck)
			{
				SummaryRichTextBox.AppendText(fileName);
				SummaryRichTextBox.AppendText("\r\n"); 
			}
		}

		///<summary>
		/// Visualizza nel sommario l'elenco delle occorrenze dell'entity da eliminare
		///</summary>
		//---------------------------------------------------------------------
		private void DisplayEntityInfoToDelete()
		{
			SummaryRichTextBox.SelectionBullet = false;
			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.AppendText(Strings.InvolvedFiles);
			SummaryRichTextBox.AppendText("\r\n");

			List<string> files = new List<string>();

			foreach (RowSecurityObjectsInfo item in rsSelections.RowSecurityObjectsList)
			{
				foreach (RSEntity rsent in item.RSEntities)
				{
					if (string.Compare(rsent.Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						if (!files.Contains(item.FilePath))
							files.Add(item.FilePath);
						break;
					}
				}
				foreach (RSTable tbl in item.RSTables)
				{
					foreach (RSEntityBase rseb in tbl.RsEntityBaseList)
					{
						if (string.Compare(rseb.Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
							if (!files.Contains(item.FilePath))
								files.Add(item.FilePath);
					}
				}
			}

			foreach (string item in files)
			{
				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(item);
				SummaryRichTextBox.AppendText("\r\n");
				SummaryRichTextBox.SelectionBullet = false;

				if (!string.IsNullOrWhiteSpace(item) && !filesToCheck.ContainsNoCase(item))
				{
					filesToCheck.Add(item);
					if (rsSelections.EncryptFiles)
					{
						string crsFile = Path.Combine(Path.GetDirectoryName(item), Path.GetFileNameWithoutExtension(item) + NameSolverStrings.CrsExtension);
						if (!filesToCheck.ContainsNoCase(crsFile))
							filesToCheck.Add(crsFile);
					}
				}
			}
		}

		//---------------------------------------------------------------------
		public override bool OnWizardFinish()
		{
			if (DiagnosticViewer.ShowQuestion(Strings.ContinueElaboration, string.Empty) != DialogResult.Yes)
				return false;

			// eseguo un pre-check dei files per capire se sono readonly, in modo da visualizzare un avvertimento
			PrecheckFiles();
			if (filesToCheck.Count > 0)
			{
				string message = Strings.FilesWithROAttributes + "\r\n";
				foreach (string filePath in filesToCheck)
					message += filePath + "\r\n";
				message += Strings.FilesWillBeOverwritten;

				if (DiagnosticViewer.ShowQuestion(message, string.Empty) != DialogResult.Yes)
					return false;
			}

			SummaryRichTextBox.Visible = false;
			SummaryLabel.Visible = false;
			OperationsListView.Visible = true;
			InitializeListView();

			entityMng.ElaborationCompleted += new EventHandler(em_ElaborationCompleted);
			entityMng.OperationCompleted += new EntityManager.OperationCompletedDelegate(em_OperationCompleted);
			Thread t = entityMng.Execute();

			return false; // procedo nell'elaborazione e lascio aperta la pagina
		}

		//--------------------------------------------------------------------------------
		private void InitializeListView()
		{
			OperationsListView.Size = new Size(SummaryRichTextBox.Size.Width, SummaryRichTextBox.Size.Height);
			OperationsListView.SmallImageList = OperationsListView.LargeImageList = ((RSWizard)this.WizardManager).StateImageList;

			OperationsListView.Items.Clear();
			OperationsListView.Columns.Clear();
			OperationsListView.Columns.Add(string.Empty, 20, HorizontalAlignment.Left);
			OperationsListView.Columns.Add(string.Empty, 400, HorizontalAlignment.Left);
		}

		//---------------------------------------------------------------------
		private void em_OperationCompleted(bool success, string message, string filePath)
		{
			Invoke(new MethodInvoker(() =>
			{
				OperationsListView.BeginUpdate();
				ListViewItem lvi = new ListViewItem();
				lvi.ImageIndex = success ? PlugInTreeNode.GetResultGreenStateImageIndex : PlugInTreeNode.GetResultRedStateImageIndex;
				lvi.SubItems.Add(message);
				lvi.Tag = filePath;
				OperationsListView.Items.Add(lvi);
				OperationsListView.EndUpdate();
			}));
		}

		//---------------------------------------------------------------------
		private void em_ElaborationCompleted(object sender, EventArgs e)
		{
			Invoke(new MethodInvoker(() =>
			{
				bool ok = true;
				// cerco almeno un item con l'immagine di errore, in modo da impostare dopo l'icona corretta
				foreach (ListViewItem item in OperationsListView.Items)
				{
					if (item.ImageIndex == PlugInTreeNode.GetResultRedStateImageIndex)
					{
						ok = false;
						break;
					}
				}

				OperationsListView.BeginUpdate();
				ListViewItem lvi = new ListViewItem();
				lvi.ImageIndex = ok ? PlugInTreeNode.GetResultGreenStateImageIndex : PlugInTreeNode.GetResultRedStateImageIndex;
				lvi.SubItems.Add(Strings.ElaborationCompleted);
				OperationsListView.Items.Add(lvi);
				OperationsListView.EndUpdate();

				// se l'esecuzione e' completata disabilito tutti i pulsanti
				this.WizardForm.SetWizardButtons(WizardButton.DisableAll);

				((WizardForm)this.WizardForm).SetCancelButtonText(Strings.Close);
			}));
		}

		//--------------------------------------------------------------------------------
		private void OperationsListView_DoubleClick(object sender, EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null ||
				list.SelectedItems.Count == 0 || 
				list.SelectedItems.Count > 1)
				return;

			ListViewItem item = list.SelectedItems[0];
			if (item != null && item.Tag != null && !string.IsNullOrWhiteSpace(item.Tag as string))
				Process.Start("notepad.exe", item.Tag.ToString());
		}

		///<summary>
		/// Metodo che fa un check preventivo sui file coinvolti per avvertire l'utente se sono readonly
		///</summary>
		//--------------------------------------------------------------------------------
		private void PrecheckFiles()
		{
			for (int i = filesToCheck.Count - 1; i >= 0; i--)
			{
				if (
					!File.Exists(filesToCheck[i]) || 
					(File.GetAttributes(filesToCheck[i]) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly
					)
					filesToCheck.RemoveAt(i);
			}
		}
	}
}
