using System.Collections;
using System.IO;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;


namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//================================================================================
	public class SourceBindingNode : TreeNode
	{
		NodeInfoContainer nodeInfos = new NodeInfoContainer();

		//--------------------------------------------------------------------------------
		public SSafeNodeInfo NodeInfo { get { return nodeInfos.NodeInfo; } }

		//--------------------------------------------------------------------------------
		public NodeInfoContainer NodeInfoContainer { get { return nodeInfos; } }

		//--------------------------------------------------------------------------------
		public new SourceBindingNode Parent { get { return base.Parent as SourceBindingNode; } }
		
		//--------------------------------------------------------------------------------
		public ArrayList PathComponents { get { return NodeInfo.PathComponents; } }
		
		//--------------------------------------------------------------------------------
		public ArrayList GroupedDynamicPathComponents { get { return nodeInfos.GroupedDynamicPathComponents; } }
	
		//--------------------------------------------------------------------------------
		public void SetPathComponentAt (int index, string path) 
		{
			nodeInfos.SetPathComponentAt(index, path); 
		}

		//--------------------------------------------------------------------------------
		public SourceBindingNode(TreeNode template)
			: base(((NodeTag) template.Tag).NodeName, template.ImageIndex, template.SelectedImageIndex)
		{
			nodeInfos.Add(new SSafeNodeInfo());
		}

		//--------------------------------------------------------------------------------
		public SourceBindingNode(SourceBindingNode template)
			: base(template.Text, template.ImageIndex, template.SelectedImageIndex)
		{
			nodeInfos.Add(template.NodeInfo);
			this.Tag = template.Tag;
		}

		//--------------------------------------------------------------------------------
		public void CalculateInfos()
		{
			if (Parent != null)
			{
				PathComponents.AddRange(new string[Parent.PathComponents.Count]);
				Parent.NodeInfo.Childs.Add(NodeInfo);
				NodeInfo.Parent = Parent.NodeInfo;
			}

			string path = Tag.ToString();
			
			NodeType type = ((NodeTag)Tag).GetNodeType();
			if (type == NodeType.SOLUTION || type == NodeType.PROJECT )
				path = Path.GetDirectoryName(path);

			NodeInfo.LocalPath = path;
			NodeInfo.NodeType = ((NodeTag)Tag).GetNodeType();
			NodeInfo.Active = true;
			PathComponents.Add(NodeInfo.GetRelativePath());
		
		}
		
	}
}
