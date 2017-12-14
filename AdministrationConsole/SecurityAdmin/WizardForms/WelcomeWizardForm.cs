using System.Drawing;
using System.IO;

using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public partial class WelcomeWizardForm : ExteriorWizardPage
	{


		#region DataMenber privati
		private WizardParameters wizardParameters = null;
		#endregion

		#region Costruttore
		public WelcomeWizardForm()
		{
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		#endregion

		#region SetActive della Form
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
  
			wizardParameters = ((SecurityWizardManager)this.WizardManager).GetImportSelections();
			InitializeLabel();

			this.WizardForm.SetWizardButtons(WizardButton.Next);
			return true;
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni d'inizializzazione
		private void InitializeLabel()
		{
			if (wizardParameters == null ) 
				return;

            DescriptionLabel.Text = WizardStringMaker.GetWelcomeOperationDescription(wizardParameters.OperationType);
			WizardCompanyName.Text	= CommonObjectTreeFunction.GetCompanyName(wizardParameters.ShowObjectsTreeForm.CompanyId, wizardParameters.ShowObjectsTreeForm.Connection);

			if (!wizardParameters.ShowObjectsTreeForm.IsRoleLogin)
				UserOrRoleNameLabel.Text = Strings.UserLabel;
			else
				UserOrRoleNameLabel.Text = Strings.RoleLabel;

			UserOrRoleName.Text		= wizardParameters.ShowObjectsTreeForm.LoginName;
			ObjectName.Text			= wizardParameters.ShowObjectsTreeForm.CurrentMenuXmlNode.Title;


            Stream myStream = WizardStringMaker.GetImageByOperationType(wizardParameters.OperationType);
			if (myStream == null) return;
			m_watermarkPicture.Image = Image.FromStream (myStream, true);

		}
		//---------------------------------------------------------------------
		#endregion

	}
}
