using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Import
{
	//=========================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private ImportSelections importSel = null;

		//---------------------------------------------------------------------
		public SummaryPage()
		{
			InitializeComponent();
		}

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			importSel = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();
			LoadImportSelections();
			
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			return true;
		}
		# endregion

		# region OnWizardFinish
		//---------------------------------------------------------------------
        public override bool OnWizardFinish()
		{
			DialogResult result = DiagnosticViewer.ShowQuestion(DataManagerStrings.MsgRunOperation, DataManagerStrings.LblRunOperation);
			
			if (result == DialogResult.Yes)
			{
				ImportManager impMng = new ImportManager(importSel, ((Common.DataManagerWizard)this.WizardManager).DBDiagnostic);
				Thread t = impMng.Import();

				// richiamo il contenitore ImportWizard che spara l'evento x visualizzare l'ExecutionForm
				((Common.DataManagerWizard)this.WizardManager).OnFinishPage(t);
			}
			else
				return false; // non procedo nell'elaborazione ma lascio aperta la pagina

			importSel.ContextInfo.UndoImpersonification();

			return base.OnWizardFinish();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerImport + "SummaryPage");
			return true;
		}
		#endregion

		# region Funzione per comporre le stringhe riepilogative delle selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void LoadImportSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			Font fontBold		= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Bold);
			Font fontRegular	= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Regular);

			// inserisco righe di testo per fare un riassunto delle selezioni effettuate
			SummaryRichTextBox.SelectionFont = fontBold;
			SummaryRichTextBox.AppendText(string.Format(DataManagerEngineStrings.MsgImportCompanyData + "\r\n", importSel.ContextInfo.CompanyName));

			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionFont = fontRegular;

			SummaryRichTextBox.AppendText(DataManagerStrings.MsgFilesList + "\r\n");

			foreach (ImportItemInfo item in importSel.ImportList)
			{
				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.SelectionFont = fontBold;
				SummaryRichTextBox.AppendText(string.Format("{0}\r\n", Path.GetFileName(item.PathName)));

				foreach (string file in item.SelectedFiles)
				{
					SummaryRichTextBox.SelectionBullet = false;
					SummaryRichTextBox.SelectionFont = fontRegular;
					SummaryRichTextBox.AppendText(string.Format("\t{0}\r\n", file));
				}
			}
	
			SummaryRichTextBox.SelectionBullet = false;
			SummaryRichTextBox.AppendText("\r\n");

			if (importSel.ImportTBCreated || importSel.ImportTBModified)
			{
				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ImportOperationUpperCase) + "\r\n");
				SummaryRichTextBox.SelectionBullet = false;

				if (importSel.ImportTBCreated)
					SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " + DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");
	
				if (importSel.ImportTBModified)
					SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
			}

			SummaryRichTextBox.SelectionBullet = true;

			SummaryRichTextBox.AppendText(importSel.UseUtcDateTimeFormat ? (DataManagerStrings.MsgUseUTCFormat + "\r\n") : (DataManagerStrings.MsgDoNotUseUTCFormat + "\r\n"));

			SummaryRichTextBox.AppendText(importSel.InsertExtraFieldsRow ? (DataManagerStrings.MsgInsertExtraFieldsRowOn + "\r\n") : (DataManagerStrings.MsgInsertExtraFieldsRowOff + "\r\n"));
			SummaryRichTextBox.AppendText(importSel.DisableCheckFK ? (DataManagerStrings.MsgDisableCheckFKOn + "\r\n") : (DataManagerStrings.MsgDisableCheckFKOff + "\r\n"));

			if (importSel.DeleteTableContext)
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOn + "\r\n");
			else
			{
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOff + "\r\n");

				// se ho scelto di non cancellare il contenuto della tabella visualizzo
				// le opzioni da adottare in caso di esistenza della stessa.
				string updateString = string.Empty;
				switch (importSel.UpdateExistRow)
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

			switch (importSel.ErrorRecovery)
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
		# endregion
	}
}
