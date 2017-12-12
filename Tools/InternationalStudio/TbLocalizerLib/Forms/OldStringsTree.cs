using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public partial class OldStringsTree : Form
	{
		private bool nodesAdded = false;
		public DictionaryTreeNode[] SelectedItems
		{
			get 
			{
				List<DictionaryTreeNode> l = new List<DictionaryTreeNode>();
				foreach (ListViewItem item in listView.Items)
					l.Add(item.Tag as DictionaryTreeNode);

				return l.ToArray();
			}
		}

		//---------------------------------------------------------------------
		public OldStringsTree()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		private void OldStringsTree_Load(object sender, EventArgs e)
		{
			if (!nodesAdded)
			{
				AddNodes(treeView.Nodes, DictionaryCreator.MainContext.ProjectsTree.Nodes);
				nodesAdded = true;
			}
			treeView.ExpandAll();
		}

		//---------------------------------------------------------------------
		private void AddNodes(TreeNodeCollection nodes, TreeNodeCollection sourceNodes)
		{
			foreach (LocalizerTreeNode node in sourceNodes)
			{
				try
				{
					if (node.Type != NodeType.SOLUTION
								&& node.Type != NodeType.PROJECT
								&& !node.IsBaseLanguageNode)
						continue;

					if (node.Type == NodeType.LASTCHILD)
					{
						XmlElement resource = ((DictionaryTreeNode)node).GetResourceNode();
						if (resource == null)
							continue;
						bool invalid = false;
						foreach (XmlElement stringNode in resource.ChildNodes)
							if (stringNode.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
							{
								invalid = true;
								break;
							}
						if (!invalid)
							continue;
					}
					TreeNode newNode = new TreeNode(node.Name);
					newNode.Tag = node;
					newNode.ImageIndex = 0;

					AddNodes(newNode.Nodes, node.Nodes);
					if (newNode.Nodes.Count > 0 || node.Type == NodeType.LASTCHILD)
						nodes.Add(newNode);
				}
				catch (Exception e)
				{

					string s = e.Message;
				}
			}
		}

		//---------------------------------------------------------------------
		private void treeView_DoubleClick(object sender, EventArgs e)
		{
			TreeNode node = treeView.GetNodeAt(treeView.PointToClient(MousePosition));
			AddNode(node);
		}

		//---------------------------------------------------------------------
		private void AddNode(TreeNode node)
		{
			if (node != null 
				&& node.Nodes.Count == 0
				&& !listView.Items.ContainsKey(node.FullPath))
			{
				ListViewItem item = listView.Items.Add(node.FullPath);
				item.ImageIndex = 0;
				item.Name = node.FullPath;
				item.Tag = node.Tag;
			}
		}

		//---------------------------------------------------------------------
		private void listViewItems_DoubleClick(object sender, EventArgs e)
		{
			Point p = listView.PointToClient(MousePosition);
			ListViewItem item = listView.GetItemAt(p.X, p.Y);
			if (item != null)
				item.Remove();
		}

		//---------------------------------------------------------------------
		private void treeView_KeyPress(object sender, KeyPressEventArgs e)
		{
			AddNode(treeView.SelectedNode);
		}

		//---------------------------------------------------------------------
		private void listView_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (listView.SelectedItems.Count == 0)
				return;

			ListViewItem item = listView.SelectedItems[0];
			if (item != null)
				item.Remove();

		}
	}
}