using Microarea.TaskBuilderNet.UI.CodeEditor.UserControls;

namespace Microarea.TaskBuilderNet.UI.CodeEditor
{
	class WoormToolTipManager
	{
		WoormEditorControl codeEditor;

		//-------------------------------------------------------------------------------
		private WoormToolTipManager(WoormEditorControl codeEditor)
		{
			this.codeEditor = codeEditor;
		}

		//-------------------------------------------------------------------------------
		public static void Attach(WoormEditorControl codeEditor)
		{
			//WoormToolTipManager tp = new WoormToolTipManager(codeEditor);
			//codeEditor.TextEditor.TextArea.ToolTipRequest += tp.OnToolTipRequest;

		}

		////-------------------------------------------------------------------------------
		//void OnToolTipRequest(object sender, ICSharpCode.TextEditor.ToolTipRequestEventArgs e)
		//{
		//	string tip = codeEditor.GetTooltip(e.LogicalPosition);
		//	if (!string.IsNullOrEmpty(tip))
		//		e.ShowToolTip(tip);
		//}
	}
}
