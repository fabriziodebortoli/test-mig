using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Export
{
	//=========================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private ExportSelections exportSel = null;

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

			exportSel = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();
			LoadExportSelections();
			
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			return true;
		}

		# region OnWizardFinish
		//---------------------------------------------------------------------
        public override bool OnWizardFinish()
		{
			DialogResult result = DiagnosticViewer.ShowQuestion(DataManagerStrings.MsgRunOperation, DataManagerStrings.LblRunOperation);
			
			if (result == DialogResult.Yes)
			{
				ExportManager expMng = new ExportManager(exportSel, ((Common.DataManagerWizard)this.WizardManager).DBDiagnostic);
				Thread t = expMng.Export();

				// richiamo il contenitore ExportWizard che spara l'evento x visualizzare l'ExecutionForm
				((Common.DataManagerWizard)this.WizardManager).OnFinishPage(t);
			}
			else
				return false; // non procedo nell'elaborazione ma lascio aperta la pagina

			exportSel.ContextInfo.UndoImpersonification();

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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerExport + "SummaryPage");
			return true;
		}
		#endregion

		# region Funzione per comporre le stringhe riepilogative delle selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void LoadExportSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			Font fontBold		= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Bold);
			Font fontRegular	= new Font(SummaryRichTextBox.Font, SummaryRichTextBox.Font.Style | FontStyle.Regular);

			// inserisco righe di testo per fare un riassunto delle selezioni effettuate
			SummaryRichTextBox.SelectionFont = fontBold;
			SummaryRichTextBox.AppendText(string.Format(DataManagerEngineStrings.MsgExportCompanyData + "\r\n", exportSel.ContextInfo.CompanyName));

			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionFont = fontRegular;
			SummaryRichTextBox.SelectionBullet = true;
			
			if (exportSel.AllTables)
			{
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderAllTables + "\r\n");
				SummaryRichTextBox.SelectionBullet = false;
			}
			else
			{
				SummaryRichTextBox.SelectionFont = fontBold;
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgConsiderSubsetOfTables + "\r\n");
				SummaryRichTextBox.SelectionBullet = false;

				foreach (CatalogTableEntry catEntry in exportSel.Catalog.TblDBList)
				{
					SummaryRichTextBox.SelectionFont = fontRegular;
					if (catEntry.Selected)
						SummaryRichTextBox.AppendText(string.Format("\t{0}\r\n", catEntry.TableName));
				}
			}

			SummaryRichTextBox.AppendText("\r\n");
			SummaryRichTextBox.SelectionBullet = true;

			SummaryRichTextBox.AppendText(exportSel.SelectColumns ? (DataManagerStrings.MsgSelectColumnsOn + "\r\n") : (DataManagerStrings.MsgSelectColumnsOff + "\r\n"));
		
			if (exportSel.WriteQuery)
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgWriteQuery + "\r\n");
			
			SummaryRichTextBox.SelectionBullet = false;
			SummaryRichTextBox.AppendText("\r\n");

			if (exportSel.ExportTBCreated || exportSel.ExportTBModified)
			{
				SummaryRichTextBox.SelectionBullet = true;
				SummaryRichTextBox.AppendText(string.Format(DataManagerStrings.MsgConsiderBaseColumns, DataManagerStrings.ExportOperationUpperCase) + "\r\n");
				SummaryRichTextBox.SelectionBullet = false;

				if (exportSel.ExportTBCreated)
					SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBCreatedColNameForSql + " - " + DatabaseLayerConsts.TBCreatedIDColNameForSql + "\r\n");

				if (exportSel.ExportTBModified)
					SummaryRichTextBox.AppendText(" - " + DatabaseLayerConsts.TBModifiedColNameForSql + " - " + DatabaseLayerConsts.TBModifiedIDColNameForSql + "\r\n");
			}

			SummaryRichTextBox.SelectionBullet = true;
			SummaryRichTextBox.AppendText(exportSel.OneFileForTable ? (DataManagerStrings.MsgOneFileForTableOn + "\r\n") : (DataManagerStrings.MsgOneFileForTableOff + "\r\n"));

			if (exportSel.SchemaInfo)
				SummaryRichTextBox.AppendText(DataManagerStrings.MsgSchemaInfo + "\r\n");

			SummaryRichTextBox.SelectionBullet = false;
		}
		# endregion
	}
}
