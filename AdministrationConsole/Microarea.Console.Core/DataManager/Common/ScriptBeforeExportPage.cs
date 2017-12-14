using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Default;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class ScriptBeforeExportPage : InteriorWizardPage
	{
		private DefaultSelections	defaultSel	= null;
		private bool fromDefaultOrSample = false;
		private Images myImages = null;		

		//---------------------------------------------------------------------
		public ScriptBeforeExportPage()
		{
			InitializeComponent();
			myImages = new Images();
		}

		# region SetImageInHeaderPicture
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			// di default metto l'image dell'export
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetExportBmpSmallIndex()];

			fromDefaultOrSample = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefaultOrSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromDefaultOrSample = fromDefaultOrSample || ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false);
			if (fromDefaultOrSample && ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false))
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		
			this.m_headerPicture.Refresh();
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			defaultSel = ((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections();

			if (defaultSel == null)
				return false;

			SetImageInHeaderPicture();
			
			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}
		# endregion

		# region Get e Set delle selezioni effettuate dall'utente
		/// <summary>
		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			// se sono in Oracle non faccio il check sintattico perchè non c'è l'opzione set parseonly
			SyntaxCheckButton.Visible		= defaultSel.ExportSel.ContextInfo.DbType != DBMSType.ORACLE;
			
			ScriptTextBox.Text				= defaultSel.ExportSel.ScriptTextBeforeExport;
			ScriptTextBox.Enabled			= defaultSel.ExportSel.ExecuteScriptTextBeforeExport;
			ExecuteScriptCheckBox.Checked	= defaultSel.ExportSel.ExecuteScriptTextBeforeExport;
			SyntaxCheckButton.Enabled		= ScriptTextBox.Enabled;
		}

		/// <summary>
		/// considero i valori presenti nei control e li associo alle default selections
		/// </summary>
		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			defaultSel.ExportSel.ScriptTextBeforeExport			= ScriptTextBox.Text;
			defaultSel.ExportSel.ExecuteScriptTextBeforeExport	= ExecuteScriptCheckBox.Checked;
		}
		# endregion

		#region OnWizardBack e OnWizardNext
		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			GetControlsValue();
			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			GetControlsValue();

			// se l'utente ha scelto di eseguire lo script e non mi ha indicato un testo non faccio procedere
			if (defaultSel.ExportSel.ExecuteScriptTextBeforeExport && 
				defaultSel.ExportSel.ScriptTextBeforeExport.Length == 0)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(DataManagerStrings.CannotExecuteAnEmptyScript, DataManagerStrings.LblAttention, MessageBoxIcon.Error);
				return WizardForm.NoPageChange;
			}

			return base.OnWizardNext();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "ScriptBeforeExportPage");
			return true;
		}
		#endregion

		# region Eventi sui control
		/// <summary>
		/// Evento sul click del pulsante di check sintattico (solo per SQL Server)
		/// prima eseguo il comando set parseonly on
		/// poi lo script inserito dall'utente nella text box
		/// e poi eseguo il comando set parseonly off
		/// </summary>
		//---------------------------------------------------------------------
		private void SyntaxCheckButton_Click(object sender, System.EventArgs e)
		{
			TBCommand command;

			try
			{
				command = new TBCommand(DataManagerConsts.SetParseOnlyOn, defaultSel.ExportSel.ContextInfo.Connection);
				command.ExecuteNonQuery();

				command.CommandText = ScriptTextBox.Text;
				command.ExecuteNonQuery();

				command.CommandText = DataManagerConsts.SetParseOnlyOff;
				command.ExecuteNonQuery();
			}
			catch(TBException exp)
			{
				MessageBox.Show(string.Format(DataManagerStrings.WrongSyntax + "\n" + "({0})", exp.Message));
				return;
			}

			MessageBox.Show(DataManagerStrings.RightSyntax);
		}

		//---------------------------------------------------------------------
		private void ScriptTextBox_Leave(object sender, System.EventArgs e)
		{
			SyntaxCheckButton.Enabled = (ScriptTextBox.Text.Length == 0) ? false : true;
		}

		//---------------------------------------------------------------------
		private void ScriptTextBox_TextChanged(object sender, System.EventArgs e)
		{
			SyntaxCheckButton.Enabled = (ScriptTextBox.Text.Length == 0) ? false : true;
		}

		//---------------------------------------------------------------------
		private void ExecuteScriptCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			ScriptTextBox.Enabled = ((CheckBox)sender).Checked;
		}
		# endregion
	}
}