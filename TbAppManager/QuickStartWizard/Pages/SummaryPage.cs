using System;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.MenuManager.QuickStartWizard.Pages
{
	///<summary>
	/// Pagina finale riassuntiva
	/// Sul Finish viene aperta un'altra form di elaborazione che, su un thread separato,
	/// richiama la classe che si occupa di creare tutta la struttura dei database/azienda/etc
	///</summary>
	//================================================================================
	public partial class SummaryPage : ExteriorWizardPage
	{
		private QuickStartSelections qsSelections = null;

		//--------------------------------------------------------------------------------
		public SummaryPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			this.m_watermarkPicture.Image = QuickStartStrings.QuickStartLarge;

			qsSelections = ((QuickStartWizard)this.WizardManager).QSSelections;

			LoadSelections();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			return true;
		}

		//---------------------------------------------------------------------
		public override bool OnWizardFinish()
		{
			// se ho scelto di non skippare la configurazione apro la form
			if (!qsSelections.SkipBaseConfiguration)
			{
				// clicco sul pulsante Finish e apro la form di elaborazione
				QSElaborationForm qseForm = new QSElaborationForm
						(
						qsSelections,
						((QuickStartWizard)this.WizardManager).QSDiagnostic,
						((QuickStartWizard)this.WizardManager).LoginManager
						);
				qseForm.ShowDialog(this);
			}

			return base.OnWizardFinish(); // ritorna sempre true
		}

		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			return (qsSelections.SkipBaseConfiguration) ? "ChooseActionPage" : base.OnWizardBack();
		}

		//---------------------------------------------------------------------
		private void LoadSelections()
		{
			// faccio la Clear della text-box 
			SummaryRichTextBox.Clear();

			SummaryRichTextBox.SelectionBullet = true;

			SummaryRichTextBox.AppendText(QuickStartStrings.RegistrationStatus + ": " + QuickStartStrings.InstallationActivated + "\r\n");
			SummaryRichTextBox.AppendText(QuickStartStrings.InstallationVersion + ": " + ((QuickStartWizard)this.WizardManager).LoginManager.GetInstallationVersion() + "\r\n");

			string edition = ((QuickStartWizard)this.WizardManager).LoginManager.GetEdition();
			string formatEdition = QuickStartStrings.ProductEdition + ": " + edition;
			if ((((QuickStartWizard)this.WizardManager).LoginManager.GetDBNetworkType() == DBNetworkType.Small) &&
				string.Compare(edition, NameSolverStrings.ProfessionalEdition, StringComparison.InvariantCultureIgnoreCase) == 0)
				formatEdition += " Lite"; // se dbnetwork = small e edition = professional si tratta della professional lite
			SummaryRichTextBox.AppendText(formatEdition + "\r\n");

			SummaryRichTextBox.AppendText(QuickStartStrings.CountryCode + ": " + ((QuickStartWizard)this.WizardManager).LoginManager.GetCountry() + "\r\n");
			SummaryRichTextBox.AppendText(QuickStartStrings.InstallationName + ": " + InstallationData.InstallationName + "\r\n");

			SummaryRichTextBox.SelectionBullet = false;

			SummaryRichTextBox.AppendText("\r\n");

			if (qsSelections.SkipBaseConfiguration)
				SummaryRichTextBox.AppendText(QuickStartStrings.SkipConfigurationDescri + "\r\n");
			else
			{
				SummaryRichTextBox.AppendText(string.Format(QuickStartStrings.BaseConfigurationDescri,
					qsSelections.CompanyName, qsSelections.Server, qsSelections.LoadDefaultData ? QuickStartStrings.DefaultDataSet : QuickStartStrings.SampleDataSet) + "\r\n");
				SummaryRichTextBox.AppendText(string.Format(QuickStartStrings.CompanyDBName, qsSelections.CompanyDBName) + "\r\n");
				SummaryRichTextBox.AppendText(string.Format(QuickStartStrings.SystemDBName, qsSelections.SystemDBName) + "\r\n");
			}

			SummaryRichTextBox.AppendText(QuickStartStrings.ClickFinishBtn);
		}
	}
}
