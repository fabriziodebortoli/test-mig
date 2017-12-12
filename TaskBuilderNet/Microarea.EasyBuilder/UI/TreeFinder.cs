using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Drawing;

namespace Microarea.EasyBuilder.UI
{
	internal partial class TreeFinder : ToolStrip
	{
		private TreeView treeView;
		private TreeNode previousNode;

		public TreeView TreeView
		{
			get { return treeView; }
			set
			{
				if (this.treeView != null)
					this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvTables_KeyDown);
				treeView = value;
				if (this.treeView != null)
					this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvTables_KeyDown);
			}
		}

		public TreeFinder()
		{
			InitializeComponent();
		}

		//--------------------------------------------------------------------------			
		private void DoKeyDown(KeyEventArgs e)
		{
			if (e.Control && e.KeyValue == (int)Keys.F) //Ctrl+F
			{
				Visible = true;
				tsText.Text = string.Empty;
				tsText.Focus();
			}

			if (e.Control && e.KeyValue == (int)Keys.F3) //Ctrl+F3
			{
				if (treeView.SelectedNode == null)
					return;
				Visible = true;
				tsText.Text = treeView.SelectedNode.Text;
				tsText.Focus();
			}

			if (e.KeyValue == 13) //Enter
				RestartSelectNext();
			if (!e.Control && !e.Shift && e.KeyValue == (int)Keys.F3) //F3 Search Forward
				SelectNext();
			if (e.Shift && e.KeyValue == (int)Keys.F3) //Shift+F3 Search Backward
				SelectPrevious();
			if (e.KeyValue == (int)Keys.Back) // Back
				RestartSelectNext();
		}

		internal void RestartSelectNext()
		{
			tsText.TextBox.BackColor = Color.White;
			SelectNext(true);
		}

		private void SelectNext(bool startOnTop = false)
		{
			TreeNode n = treeView.SelectedNode;
			if (n == null || startOnTop)
				n = treeView.Nodes[0];
			else
				n = SearchNext(n);
			while (n != null)
			{
				if (n.Text.IndexOfNoCase(tsText.Text) >= 0)
				{
					treeView.SelectedNode = n;
					treeView.SelectedNode.EnsureVisible();
					SetNodeLikeSelected();
					return;
				}
				n = SearchNext(n);
			}
			tsText.TextBox.BackColor = Color.Red;
		}

		private void SelectPrevious()
		{
			TreeNode n = treeView.SelectedNode;
			if (n == null)
				n = LastLeaf(treeView.Nodes[treeView.Nodes.Count - 1]);
			else
				n = SearchPrevious(n);
			while (n != null)
			{
				if (n.Text.IndexOfNoCase(tsText.Text) >= 0)
				{
					treeView.SelectedNode = n;
					treeView.SelectedNode.EnsureVisible();
					break;
				}
				n = SearchPrevious(n);
			}

		}

		//--------------------------------------------------------------------------			
		internal void tsNextFind_Click(object sender, EventArgs e)
		{
			SelectNext();
		}

		//--------------------------------------------------------------------------			
		private void tsPreviousFind_Click(object sender, EventArgs e)
		{
			SelectPrevious();
		}

		//--------------------------------------------------------------------------			
		private void tsClose_Click(object sender, EventArgs e)
		{
			Visible = false;
		}

		//--------------------------------------------------------------------------			
		private TreeNode SearchNext(TreeNode n)
		{
			TreeNode next = FirstLeaf(n);
			if (next == n)
				next = n.NextNode;
			if (next == null)
			{
				while ((n = n.Parent) != null)
				{
					if (n.NextNode != null)
						return n.NextNode;
				}

			}


			return next;
		}

		//--------------------------------------------------------------------------			
		private TreeNode FirstLeaf(TreeNode n)
		{
			if (n.Nodes.Count > 0)
				return n.Nodes[0];
			return n;
		}

		//--------------------------------------------------------------------------			
		private TreeNode SearchPrevious(TreeNode n)
		{
			TreeNode prev = n.PrevNode;
			if (prev == null)
				prev = n.Parent;
			else
				prev = LastLeaf(prev);

			return prev;
		}

		//--------------------------------------------------------------------------			
		private TreeNode LastLeaf(TreeNode n)
		{
			if (n.Nodes.Count == 0)
				return n;

			n = n.Nodes[n.Nodes.Count - 1];
			return LastLeaf(n);
		}


		//--------------------------------------------------------------------------			
		private void tsText_KeyDown(object sender, KeyEventArgs e)
		{
			DoKeyDown(e);
		}

		//--------------------------------------------------------------------------			
		private void tvTables_KeyDown(object sender, KeyEventArgs e)
		{
			DoKeyDown(e);
		}


		//-----------------------------------------------------------------------------
		private void SetNodeLikeSelected()
		{
			if (previousNode != null)
			{
				previousNode.BackColor = Color.Transparent;
				previousNode.ForeColor = Color.Black;
			}
			previousNode = treeView.SelectedNode;
			if (previousNode != null)
			{
				previousNode.BackColor = SystemColors.Highlight;
				previousNode.ForeColor = SystemColors.HighlightText;
			}
		}

	}
}
