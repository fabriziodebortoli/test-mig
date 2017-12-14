
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public class SecurityWizardManager : WizardManager
	{
		private WizardParameters wizardParameters = null;

		//---------------------------------------------------------------------
		public SecurityWizardManager(WizardParameters wizardParameters)
		{
			this.wizardParameters = wizardParameters;
		}

		//---------------------------------------------------------------------
		public WizardParameters GetImportSelections()
		{
			return wizardParameters;
		}
	}
}

