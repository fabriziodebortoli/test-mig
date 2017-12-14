using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UITreeViewElement : RadTreeViewElement
	{
		protected override RadTreeNode CreateNewNode()
		{
			return new UITreeNode();
		}

		protected override RadTreeNode CreateNewNode(string defaultText)
		{
			return new UITreeNode(defaultText);
		}
	}
}
