using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	/// <remarks/>
	internal partial class SaveMenu : ThemedForm
	{
		//---------------------------------------------------------------------
		/// <remarks/>
		public bool PublishMenu
		{
			get { return chkPublish.Checked; }
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}
		//---------------------------------------------------------------------
		/// <remarks/>
		public SaveMenu(bool isStandardization)
		{
			InitializeComponent();

			//Le standardizzazioni hanno i file pubblicati per default.
			if (isStandardization)
			{
				chkPublish.Visible = false;
			}
		}
	}
}
