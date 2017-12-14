using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for TreeNodeTimer.
	/// </summary>
	public class TreeNodeTimer : Timer
	{
		TreeNode currentNode;

		public TreeNodeTimer(TreeNode currentNode)
		{
			this.currentNode = currentNode;
		}

		public TreeNode CurrentNode { get { return currentNode; } }
	}
}
