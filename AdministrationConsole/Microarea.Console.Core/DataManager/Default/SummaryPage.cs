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

namespace Microarea.Console.Core.DataManager.Default
{
	//=========================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private DefaultSelections defaultSel = null;

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

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();
			LoadDefaultSelections();
			
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnWizardFinish()
		{
			DialogResult result =
				DiagnosticViewer.ShowQuestion(DataManagerStrings.MsgRunOperation, DataManagerStrings.LblRunOperation);
			
			if (result == DialogResult.Yes)
			{
				DefaultManager defMng = new DefaultManager(defaultSel, ((Common.DataManagerWizard)this.WizardManager).DBDiagnostic, ((Common.DataManagerWizard)this.WizardManager).BrandLoader);
				Thread t = defMng.DefaultDataManagement();

				((Common.DataManagerWizard)this.WizardManager).OnFinishPage(t);
			}
			else
				return false; // non procedo nell'elaborazione ma lascio aperta la pagina

			defaultSel.ContextInfo.UndoImpersonification();

			return base.OnWizardFinish();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			return (defaultSel.Mode == DefaultSelections.ModeType.EXPORT)
				? "ConfigurationFileParamPage"
				: "ErrorParamsPage";
		}

		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerDefault + "SummaryPage");
			return true;
		}
	
		# region Funzioni per comporre le stringhe riepilogative delle selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void LoadDefaultSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			Font fontBold		= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Bold);
			Font fontRegular	= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Regular);

			// inserisco righe di testo per fare un riassunto delle selezioni effettuate
			SummaryRichTextBox.SelectionFont = fontBold;

			if (defaultSel.Mode == DefaultSelections.ModeType.EXPORT)
				SummaryRichTextBox.AppendText
					(
					string.Format(DataManagerStrings.MsgCreateData + "\r\n",
					DataManagerEngineStrings.DefaultText, 
					defaultSel.ContextInfo.CompanyName)
					);
			else
				SummaryRichTextBox.AppendText
					(
					string.Format(DataManagerStrings.MsgLoadData + "\r\n", 
					defaultSel.ContextInfo.CompanyName,
					DataManagerStrings.DefaultOptionalText)
					);
			
			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionFont = fontRegular;

			// generazione dati default
			if (defaultSel.Mode == DefaultSelections.ModeType.EXPORT)
			{
				SummaryRichTextBox.SelectionBullet = true;

				if (defaultSel.ExportSel.AllTables)
				{
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderAllTables + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;
				}
				else
				{
					SummaryRichTextBox.SelectionFont = fontBold;
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderSubsetOfTables + "\r\n");
					WriteSelectedTablesList();
					SummaryRichTextBox.SelectionBullet = false;
				}

				if (defaultSel.ExportSel.WriteQuery)
				{
					SummaryRichTextBox.AppendText("\r\n");
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgWriteQuery + "\r\n");
				}
				
				SummaryRichTextBox.SelectionBullet = false;

				if (defaultSel.ExportSel.ExportTBCreated || defaultSel.ExportSel.ExportTBModified)
				{
					SummaryRichTextBox.AppendText("\r\n");
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ExportOperationUpperCase) + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;

					if (defaultSel.ExportSel.ExportTBCreated)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " +  DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");

					if (defaultSel.ExportSel.ExportTBModified)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
				}

				SummaryRichTextBox.SelectionBullet = false;

				if (defaultSel.ExportSel.ExecuteScriptTextBeforeExport)
				{
					SummaryRichTextBox.AppendText("\r\n");
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(DataManagerEngineStrings.ExecutionScript + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;
				}

				SummaryRichTextBox.SelectionBullet = false;

				if (defaultSel.ExportSel.SaveInConfigurationFile)
				{
					SummaryRichTextBox.AppendText("\r\n");
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(DataManagerStrings.SaveSelectionsInFile + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;
					SummaryRichTextBox.AppendText(" - " + defaultSel.ExportSel.ConfigurationFilePathToSave + "\r\n");
				}
			}
			else // caricamento dati default già esistenti
			{
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgFilesList + "\r\n");

				foreach (ImportItemInfo item in defaultSel.ImportSel.ImportList)
				{
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.SelectionFont = fontBold;
					SummaryRichTextBox.AppendText(string.Format("{0}\r\n", Path.GetFileName(item.PathName)));

					foreach (ImportItem file in item.SelectedFiles)
					{
						SummaryRichTextBox.SelectionBullet = false;
						SummaryRichTextBox.SelectionFont = fontRegular;
						SummaryRichTextBox.AppendText(string.Format("\t{0}\r\n", file.File));
					}
				}

				SummaryRichTextBox.SelectionBullet = false;
				SummaryRichTextBox.AppendText("\r\n");

				if (defaultSel.ImportSel.ImportTBCreated || defaultSel.ImportSel.ImportTBModified)
				{
					SummaryRichTextBox.SelectionBullet = true;
					SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ImportOperationUpperCase) + "\r\n");
					SummaryRichTextBox.SelectionBullet = false;

					if (defaultSel.ImportSel.ImportTBCreated)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " + DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");

					if (defaultSel.ImportSel.ImportTBModified)
						SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
				}

				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(defaultSel.ImportSel.UseUtcDateTimeFormat ? (DataManagerStrings.MsgUseUTCFormat + "\r\n") : (DataManagerStrings.MsgDoNotUseUTCFormat + "\r\n"));

				SummaryRichTextBox.AppendText(defaultSel.ImportSel.InsertExtraFieldsRow ? (DataManagerStrings.MsgInsertExtraFieldsRowOn + "\r\n") : (DataManagerStrings.MsgInsertExtraFieldsRowOff + "\r\n"));
				SummaryRichTextBox.AppendText(defaultSel.ImportSel.DisableCheckFK ? (DataManagerStrings.MsgDisableCheckFKOn + "\r\n") : (DataManagerStrings.MsgDisableCheckFKOff + "\r\n"));

				if (defaultSel.ImportSel.DeleteTableContext)
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOn + "\r\n");
				else
				{
					SummaryRichTextBox.AppendText(DataManagerStrings.MsgDeleteTableContextOff + "\r\n");

					// se ho scelto di non cancellare il contenuto della tabella visualizzo
					// le opzioni da adottare in caso di esistenza della stessa.
					string updateString = string.Empty;
					switch (defaultSel.ImportSel.UpdateExistRow)
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

				switch (defaultSel.ImportSel.ErrorRecovery)
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

		//---------------------------------------------------------------------
		private void WriteSelectedTablesList()
		{
			SummaryRichTextBox.SelectionBullet = false;
			// in questo array inserisco le varie stringhe
			// (ho scelto un arraylist xchè la stringcollection non ha il sorting)
			ArrayList list = new ArrayList();
	
			foreach (CatalogEntry catEntry in defaultSel.ExportSel.Catalog.TblDBList)
			{
				SummaryRichTextBox.SelectionFont = new Font(SummaryRichTextBox.Font, 
					SummaryRichTextBox.Font.Style | FontStyle.Regular);

				if (catEntry.Selected)
					list.Add(string.Format("{0}.{1}.{2}", catEntry.Application, catEntry.Module, catEntry.TableName));
			}

			list.Sort();

			string app1 = string.Empty;
			foreach (string s in list)
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
		}
		# endregion
	}
}
