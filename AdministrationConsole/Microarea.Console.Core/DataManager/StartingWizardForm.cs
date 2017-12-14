using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Common;
using Microarea.Console.Core.DBLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Core.DataManager
{
	/// <summary>
	/// Summary description for StartingWizardForm.
	/// </summary>
	//=========================================================================
	public partial class StartingWizardForm : System.Windows.Forms.Form
	{
		private ContextInfo			contextInfo		= null;
		private BrandLoader			brandLoader		= null;
		private ImportExportManager	importExportMng = null;

		private string companyId;
		private Images myImages	= null;

		// evento sparato al PlugIn prima di aprire il wizard (per controllare se il db aziendale è free)
		public delegate bool BeforeStartingWizard();
		public event BeforeStartingWizard OnBeforeStartingWizard;

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------
		public StartingWizardForm(string companyIdNode, ContextInfo context, BrandLoader brand, string companyName)
		{
			InitializeComponent();

			contextInfo = context;
			companyId	= companyIdNode;
			brandLoader	= brand;

			myImages = new Images();

			DescriptionLabel.Text = string.Format(DescriptionLabel.Text, companyName);
		}

		# region LinkLabel per l'Import
		//---------------------------------------------------------------------------
		private void ImportWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (AskCompanyDBIsFree())
			{
				// utilizzo il costruttore con l'indicazione dell'id della company
				importExportMng = new ImportExportManager(companyId, contextInfo, brandLoader);
				importExportMng.RunImportWizard();
			}
			else
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.ErrCompanyDBIsNotFree, DBLibraryStrings.LblAttention, MessageBoxIcon.Exclamation);
		}

		//---------------------------------------------------------------------------
		private void ImportWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OperationPictureBox.Show();
			OperationPictureBox.Image = myImages.LargePictureImageList.Images[Images.GetImportBmpLargeIndex()];
			DetailsLabel.Text = DataManagerStrings.ImportPresentationText;
		}
		# endregion

		# region LinkLabel per l'Export
		//---------------------------------------------------------------------------
		private void ExportWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			importExportMng = new ImportExportManager(companyId, contextInfo, brandLoader);
			importExportMng.RunExportWizard();
		}

		//---------------------------------------------------------------------------
		private void ExportWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OperationPictureBox.Show();
			OperationPictureBox.Image = myImages.LargePictureImageList.Images[Images.GetExportBmpLargeIndex()];
			DetailsLabel.Text = DataManagerStrings.ExportPresentationText;
		}
		# endregion

		# region LinkLabel per il Default
		//---------------------------------------------------------------------------
		private void DefaultWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			importExportMng = new ImportExportManager(companyId, contextInfo, brandLoader);
			// aggancio l'evento di controllo dello stato FREE del db aziendale
			importExportMng.OnGetFreeStatusCompanyDB += new ImportExportManager.GetFreeStatusCompanyDB(AskCompanyDBIsFree);
			importExportMng.RunDefaultWizard();
		}

		//---------------------------------------------------------------------------
		private void DefaultWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OperationPictureBox.Show();
			OperationPictureBox.Image = myImages.LargePictureImageList.Images[Images.GetDefaultBmpLargeIndex()];
			DetailsLabel.Text = DataManagerStrings.DefaultPresentationText;
		}
		# endregion

		# region LinkLabel per il Sample
		//---------------------------------------------------------------------------
		private void SampleWizardLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			importExportMng = new ImportExportManager(companyId, contextInfo, brandLoader);
			// aggancio l'evento di controllo dello stato FREE del db aziendale
			importExportMng.OnGetFreeStatusCompanyDB += new ImportExportManager.GetFreeStatusCompanyDB(AskCompanyDBIsFree);
			importExportMng.RunSampleWizard();
		}

		//---------------------------------------------------------------------------
		private void SampleWizardLinkLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OperationPictureBox.Show();
			OperationPictureBox.Image = myImages.LargePictureImageList.Images[Images.GetSampleBmpLargeIndex()];
			DetailsLabel.Text = DataManagerStrings.SamplePresentationText;
		}
		# endregion

		/// <summary>
		/// muovendo il mouse in ogni punto della form pulisco il testo e l'immagine
		/// </summary>
		//---------------------------------------------------------------------------
		private void StartingWizardForm_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// trovare una bitmap neutra da mettere (per ora metto quella dell'import)
			OperationPictureBox.Image = myImages.LargePictureImageList.Images[Images.GetImportBmpLargeIndex()];
			DetailsLabel.Text = DataManagerStrings.GenericPresentationText;
		}

		# region Evento sparato al PlugIn per sapere se uno o più utenti sono collegati al db lato Mago
		/// <summary>
		/// spara l'evento al PlugIn per sapere se il database è libero da utenti (lato Mago)
		/// (a sua volta viene chiesto alla Console di interrogare il Login Manager per sapere il numero
		/// degli utenti collegati. Se è uguale a zero allora consento di procedere all'apertura del wizard)
		/// NOTE: 
		/// Import: il controllo viene fatto subito, prima dell'apertura del wizard
		/// Export: non viene fatto alcun controllo, è sempre consentito esportare dei dati
		/// Default+Sample: il controllo viene effettuato solo se l'utente ha scelto di "Caricare i dati"
		/// </summary>
		//---------------------------------------------------------------------------
		private bool AskCompanyDBIsFree()
		{
			if (OnBeforeStartingWizard != null)
				return OnBeforeStartingWizard();

			return false;
		}
		#endregion
	}
}