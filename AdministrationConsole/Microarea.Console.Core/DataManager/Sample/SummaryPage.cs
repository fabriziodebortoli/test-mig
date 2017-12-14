using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Import;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Sample
{
	//=========================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private SampleSelections	sampleSel = null;
		private ArrayList			tablesSelectedList = new ArrayList();

		//---------------------------------------------------------------------
		public SummaryPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			sampleSel = ((Common.DataManagerWizard)this.WizardManager).GetSampleSelections();
			LoadSampleSelections();
			
			return true;
		}

		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerSample + "SummaryPage");
			return true;
		}

		# region OnWizardFinish e OnWizardBack
		//---------------------------------------------------------------------
        public override bool OnWizardFinish()
		{
			DialogResult result = DiagnosticViewer.ShowQuestion(DataManagerStrings.MsgRunOperation, DataManagerStrings.LblRunOperation);
			
			if (result == DialogResult.Yes)
			{
				SampleManager sampleMng = new SampleManager(sampleSel, ((Common.DataManagerWizard)this.WizardManager).DBDiagnostic);
				Thread t = sampleMng.SampleDataManagement();

				// richiamo il contenitore ExportWizard che spara l'evento x visualizzare l'ExecutionForm
				((Common.DataManagerWizard)this.WizardManager).OnFinishPage(t);
			}
			else
				return false; // non procedo nell'elaborazione ma lascio aperta la pagina

			sampleSel.ContextInfo.UndoImpersonification();

			return base.OnWizardFinish();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			return (sampleSel.Mode == SampleSelections.ModeType.EXPORT)
				? "BaseColumnsPage" : "ErrorParamsPage";
		}
		# endregion

		# region Funzioni per comporre le stringhe riepilogative delle selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void LoadSampleSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			Font fontBold		= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Bold);
			Font fontRegular	= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Regular);

			// inserisco righe di testo per fare un riassunto delle selezioni effettuate
			SummaryRichTextBox.SelectionFont = fontBold;

			if (sampleSel.Mode == DefaultSelections.ModeType.EXPORT)
				SummaryRichTextBox.AppendText
					(
					string.Format(DataManagerStrings.MsgCreateData + "\r\n",
					DataManagerStrings.SampleText, 
					sampleSel.ContextInfo.CompanyName));
			else
				SummaryRichTextBox.AppendText
					(
					string.Format(DataManagerStrings.MsgLoadData + "\r\n", 
					sampleSel.ContextInfo.CompanyName,
					DataManagerStrings.SampleText)
					);
			
			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.SelectionFont = fontRegular;

			// ESPORTAZIONE DATI DI ESEMPIO //
			if (sampleSel.Mode == DefaultSelections.ModeType.EXPORT)
			{
				SummaryRichTextBox.SelectionBullet = true;

				if (sampleSel.ExportSel.AllTables)
				{
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderAllTables + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;
					this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
				}
				else
				{
					SummaryRichTextBox.SelectionFont = fontBold;
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderSubsetOfTables + "\r\n");
					WriteSelectedTablesList();
					SummaryRichTextBox.SelectionBullet = false;
				}

				SummaryRichTextBox.AppendText("\r\n");
				SummaryRichTextBox.SelectionBullet = true;

				if (sampleSel.ExportSel.WriteQuery)
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgWriteQuery + "\r\n");
				
				SummaryRichTextBox.SelectionBullet = false;
				SummaryRichTextBox.AppendText("\r\n");

				if (sampleSel.ExportSel.ExportTBCreated || sampleSel.ExportSel.ExportTBModified)
				{
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ExportOperationUpperCase) + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;

					if (sampleSel.ExportSel.ExportTBCreated)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " + DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");

					if (sampleSel.ExportSel.ExportTBModified)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
				}
			}
			else // IMPORTAZIONE DATI DI ESEMPIO GIÀ ESISTENTI
			{
				// se non ci sono file da importare
				if (sampleSel.ImportSel.ImportList.Count == 0)
				{
					SummaryRichTextBox.SelectionFont = fontBold;

					SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.ErrSampleFilesNotExist + "\r\n", sampleSel.SelectedIsoState));

					// disabilito il pulsante Finish
					this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.DisabledFinish);
				}
				else
				{
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgFilesList + "\r\n");

					foreach (ImportItemInfo item in sampleSel.ImportSel.ImportList)
					{
						SummaryRichTextBox.SelectionBullet = true;
						SummaryRichTextBox.SelectionFont = fontBold;
						SummaryRichTextBox.AppendText(string.Format("{0}\r\n", Path.GetFileName(item.PathName)));
						SummaryRichTextBox.SelectionBullet = false;

						foreach (string file in item.SelectedFiles)
						{
							SummaryRichTextBox.SelectionFont = fontRegular;
							SummaryRichTextBox.AppendText(string.Format("\t{0}\r\n", file));
						}
					}

					this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
				}

				SummaryRichTextBox.AppendText("\r\n");
			
				if (sampleSel.ImportSel.ImportTBCreated || sampleSel.ImportSel.ImportTBModified)
				{
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ImportOperationUpperCase) + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;

					if (sampleSel.ImportSel.ImportTBCreated)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " + DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");

					if (sampleSel.ImportSel.ImportTBModified)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
				}

				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(sampleSel.ImportSel.UseUtcDateTimeFormat ? (DataManagerStrings.MsgUseUTCFormat + "\r\n") : (DataManagerStrings.MsgDoNotUseUTCFormat + "\r\n"));

				SummaryRichTextBox.AppendText(sampleSel.ImportSel.InsertExtraFieldsRow ? (DataManagerStrings.MsgInsertExtraFieldsRowOn + "\r\n") : (DataManagerStrings.MsgInsertExtraFieldsRowOff + "\r\n"));
				SummaryRichTextBox.AppendText(sampleSel.ImportSel.DisableCheckFK ? (DataManagerStrings.MsgDisableCheckFKOn + "\r\n") : (DataManagerStrings.MsgDisableCheckFKOff + "\r\n"));

				if (sampleSel.ImportSel.DeleteTableContext)
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOn + "\r\n");
				else
				{
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOff + "\r\n");

					// se ho scelto di non cancellare il contenuto della tabella visualizzo
					// le opzioni da adottare in caso di esistenza della stessa.
					string updateString = string.Empty;
					switch (sampleSel.ImportSel.UpdateExistRow)
					{
						case ImportSelections.UpdateExistRowType.SKIP_ROW:
							updateString = DataManagerStrings.MsgUpdateExistRowOff;
							break;
						case ImportSelections.UpdateExistRowType.UPDATE_ROW:
							updateString = DataManagerStrings.MsgUpdateExistRowOn;
							break;
						case ImportSelections.UpdateExistRowType.SKIP_ROW_ERROR:
							updateString = DataManagerStrings.MsgUpdateExistRowError;
							break;
					}
					SummaryRichTextBox.AppendText(updateString + "\r\n");
				}

				SummaryRichTextBox.AppendText(DataManagerStrings.MsgErrorAction + " ");

				switch (sampleSel.ImportSel.ErrorRecovery)
				{
					case ImportSelections.TypeRecovery.CONTINUE:
						SummaryRichTextBox.AppendText(DataManagerStrings.MsgErrActionContinue);
						break;

					case ImportSelections.TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK:
						SummaryRichTextBox.AppendText(DataManagerStrings.MsgErrActionContinueLastFileRollback);
						break;

					case ImportSelections.TypeRecovery.STOP_LAST_FILE_ROLLBACK:
						SummaryRichTextBox.AppendText(DataManagerStrings.MsgErrActionStopLastFileRollback);
						break;

					case ImportSelections.TypeRecovery.STOP_ALL_FILE_ROLLBACK:
						SummaryRichTextBox.AppendText(DataManagerStrings.MsgErrActionStopAllFileRollback);
						break;
				}
			}
		}

		// per sapere se ci sono delle tabelle selezionate
		//---------------------------------------------------------------------
		private bool CheckExistingSelectedTables()
		{
			foreach (CatalogEntry catEntry in sampleSel.ExportSel.Catalog.TblDBList)
			{
				SummaryRichTextBox.SelectionFont = new Font(SummaryRichTextBox.Font, 
					SummaryRichTextBox.Font.Style | FontStyle.Regular);

				if (catEntry.Selected)
					tablesSelectedList.Add(string.Format("{0}.{1}.{2}", catEntry.Application, catEntry.Module, catEntry.TableName));
			}

			tablesSelectedList.Sort();

			return (tablesSelectedList.Count > 0) ? true : false;
		}

		//---------------------------------------------------------------------
		private void WriteSelectedTablesList()
		{
			SummaryRichTextBox.SelectionBullet = false;

			if (CheckExistingSelectedTables())
			{
				string app1 = string.Empty;
				foreach (string s in tablesSelectedList)
				{
					string [] str = s.Split(new Char[] {'.'});
					string app, table = string.Empty;

					app	= str[0].ToString() + "." + str[1].ToString();
					table = str[2].ToString();

					// su rottura di Applicazione + Modulo inserisco la riga di intestazione
					// e poi elenco tutte le tabelle selezionate
					if (string.Compare(app, app1, true, CultureInfo.InvariantCulture) != 0)
					{
						SummaryRichTextBox.AppendText(string.Format("- {0}\r\n", app));
						app1 = app;
					}

					SummaryRichTextBox.AppendText(string.Format("\t{0}\r\n", table));
				}

				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			}
			else
			{
				SummaryRichTextBox.AppendText(DataManagerStrings.ErrSampleTablesNotExist + "\r\n");
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.DisabledFinish);
			}
		}
		# endregion
	}
}
