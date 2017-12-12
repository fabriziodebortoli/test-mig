using System.Drawing;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;


namespace Microarea.Console.Core.RegressionTestLibrary.WizardPages
{
	/// <summary>
	/// Summary description for SummaryPage.
	/// </summary>
	//=========================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		# region Variabili private
		private RegressionTestSelections dataSel = null;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public SummaryPage()
		{
			InitializeComponent();
		}
		# endregion
		
		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			dataSel = ((RegressionTestWizard)this.WizardManager).DataSelections;
			LoadSelections();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			return true;
		}
		# endregion

		# region OnWizardFinish e OnWizardBack
		//---------------------------------------------------------------------
        public override bool OnWizardFinish()
		{
			DialogResult result = 
				DiagnosticViewer.ShowQuestion(RegressionTestLibraryStrings.MsgRunOperation, RegressionTestLibraryStrings.AttentionLabel);
			
			if (result == DialogResult.Yes)
			{
				// richiamo il gestore RegressionTestWizard che spara l'evento x visualizzare la ProcessingForm
				((RegressionTestWizard)this.WizardManager).OnFinishPage();
			}
			else
				return false; // non procedo nell'elaborazione ma lascio aperta la pagina

			return base.OnWizardFinish();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			return base.OnWizardBack();
		}

		# endregion

		#region OnWizardHelp
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage
				(
				this, 
				RegressionTestLibraryConsts.NamespacePlugIn, 
				RegressionTestLibraryConsts.SearchParameter + "SummaryPage"
				);

			return true;
		}
		#endregion

		# region Funzioni per comporre le stringhe riepilogative delle selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void LoadSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			SummaryRichTextBox.SelectionFont = new Font(SummaryRichTextBox.Font, 
				SummaryRichTextBox.Font.Style | FontStyle.Bold);

			SummaryRichTextBox.AppendText("Aggiornamento database dei Test di Non Regressione.");

			SummaryRichTextBox.SelectionFont = new Font(SummaryRichTextBox.Font, 
				SummaryRichTextBox.Font.Style | FontStyle.Regular);
			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.AppendText("\r\n");

			SummaryRichTextBox.SelectionBullet = true;

			SummaryRichTextBox.AppendText("Elenco Units da considerare.\r\n");	

			SummaryRichTextBox.SelectionBullet = false;

			foreach (AreaItem area in dataSel.AreaItems.Values)
			{
				SummaryRichTextBox.AppendText(string.Format("Area: {0}\r\n", area.Name));	
				foreach (DataSetItem dataSet in area.DataSetItems.Values)
				{
					SummaryRichTextBox.AppendText(string.Format("   DataSet: {0}\r\n", dataSet.Name));	
				}
			}
		}
		# endregion
	}
}
