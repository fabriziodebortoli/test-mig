using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
    public delegate void PageActivationEventHandler(object sender, int pageIndex, WizardPage page);

	//=========================================================================
	public interface IWizardForm 
    {
        WizardManager WizardManager { get; set; }
        List<WizardPage> Pages { get; }
		string PreviousPageName { get; }
        string NextPage { get; }
        string NoPageChange { get; }

        Font DefaultPageFont { get; }
		int ButtonsBarHeight { get; }
 
        event PageActivationEventHandler PageActivating;
        event PageActivationEventHandler PageActivated;

        void SetWizardButtons(WizardButton flags);

		void SetCurrentCursor(Cursor cursor);
		void SetDefaultCursor();
    }

    //=========================================================================
    [Flags]
    public enum WizardButton
    {
        /// <summary>
        /// Identifies the <b>Back</b> button.
        /// </summary>
        Back = 0x00000001,

        /// <summary>
        /// Identifies the <b>Next</b> button.
        /// </summary>
        Next = 0x00000002,

        /// <summary>
        /// Identifies the <b>Finish</b> button.
        /// </summary>
        Finish = 0x00000004,

        /// <summary>
        /// Identifies the disabled <b>Finish</b> button.
        /// </summary>
        DisabledFinish = 0x00000008,

        /// <summary>
        /// Identifies the disabled all buttons.
        /// </summary>
        DisableAll = 0x00000010,
    }
}
