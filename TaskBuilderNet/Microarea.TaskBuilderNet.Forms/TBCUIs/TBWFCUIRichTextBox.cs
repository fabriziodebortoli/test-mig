using System;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Forms.TBCUIs
{
	//================================================================================================================
	internal class TBWFCUIRichTextBox : TBWFCUIControl
	{
		//---------------------------------------------------------------------
		public TBWFCUIRichTextBox(UIRichTextBox ctrl)
			:
			base(ctrl, Interfaces.NameSpaceObjectType.Control)
		{

		}

		//---------------------------------------------------------------------
		protected override void DocumentFormModeChanged(object sender, EventArgs e)
		{
			base.DocumentFormModeChanged(sender, e);

			if (sender == null)
				return;

			UIRichTextBox control = Component as UIRichTextBox;
			IMAbstractFormDoc doc = sender as IMAbstractFormDoc;
			if (doc == null || control == null)
				return;

			control.Enabled = true;
			control.IsReadOnly = !IsEnabled || (doc.FormMode == FormModeType.Browse || doc.FormMode == FormModeType.Design);
		}
	}
}
