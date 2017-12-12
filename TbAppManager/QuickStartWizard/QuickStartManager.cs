using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.MenuManager.QuickStartWizard
{
	///<summary>
	/// Manager che si occupa di costruire il wizard con tutte le sue pagine
	///</summary>
	//================================================================================
	public class QuickStartManager
	{
		///<summary>
		/// Entry-point per l'apertura del wizard
		///</summary>
		//--------------------------------------------------------------------------------
		public bool RunQuickStartWizard(LoginManager loginManager, System.Windows.Forms.IWin32Window owner)
		{
			QuickStartWizard qsWizard = new QuickStartWizard(loginManager);
            qsWizard.ShowInTaskbar = true;
			qsWizard.AddWizardPages();
			
			DialogResult dr = qsWizard.Run(owner);
			if (dr == DialogResult.Cancel)
				return false;

			return true;
		}
	}
}
