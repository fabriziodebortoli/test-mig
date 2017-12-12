using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Core.RegressionTestLibrary.WizardPages
{
	/// <summary>
	/// Summary description for TimeEvaluationPage.
	/// </summary>
	//=========================================================================
	public partial class FoldersSelectionPage : InteriorWizardPage
	{
		# region Variabili private
		private RegressionTestSelections dataSel = null;
		//private MigrationImages myImages = null;		
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------------	
		public FoldersSelectionPage()
		{
			this.components = new System.ComponentModel.Container();
			InitializeComponent();

			//myImages = new MigrationImages();
			//this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[MigrationImages.GetMigrationBmpSmallIndex()];
		}
		# endregion

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			dataSel = ((RegressionTestWizard)this.WizardManager).DataSelections;

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			GetControlsValue();
			return base.OnKillActive();
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();
			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
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
				RegressionTestLibraryConsts.SearchParameter + "FoldersSelectionPage"
				);

			return true;
		}
		#endregion

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base delle selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			RepositoryPathTextBox.Text = dataSel.RepositoryPath;
			WinZipPathTextBox.Text = dataSel.WinZipPath;
			ExtraUpdatePathTextBox.Text = dataSel.ExtraUpdateFilePath;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo alla DataSelections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			dataSel.RepositoryPath = RepositoryPathTextBox.Text;
			dataSel.WinZipPath = WinZipPathTextBox.Text;
			dataSel.ExtraUpdateFilePath = ExtraUpdatePathTextBox.Text;
		}
		# endregion

		# region Gestioni interattive
		private void RepositoryPathButton_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog.SelectedPath = RepositoryPathTextBox.Text;
			FolderBrowserDialog.ShowDialog();
			RepositoryPathTextBox.Text = FolderBrowserDialog.SelectedPath;
		}

		private void WinZipPathButton_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog.SelectedPath = WinZipPathTextBox.Text;
			FolderBrowserDialog.ShowDialog();
			WinZipPathTextBox.Text = FolderBrowserDialog.SelectedPath;
		}

		private void ExtraUpdatePathButton_Click(object sender, System.EventArgs e)
		{
			ExtraUpdateFileDialog.FileName = ExtraUpdatePathTextBox.Text;
			ExtraUpdateFileDialog.ShowDialog();
			ExtraUpdatePathTextBox.Text = ExtraUpdateFileDialog.FileName;
		}
		# endregion

	}
}

