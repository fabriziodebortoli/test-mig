using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	/// <summary>
	/// 
	/// </summary>
	public partial class BusinessObjectsExplorer : UserControl
	{

		/// <remarks/>
		public event EventHandler OpenTwinPanel;

		private enum Status { NotExposed, Exposed, InErrorState };
		private TreeNode documentsNode;
        private Editor editor;
		private TreeNode dragNode = null;
		private string objectType = typeof(BusinessObject).Name;

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public BusinessObjectsExplorer(Editor editor)
		{
			InitializeComponent();
			this.Text = Resources.BusinessObjectsExplorer;

			this.editor = editor;
			this.editor.ComponentDeleted += new EventHandler<EventArgs>(editor_ComponentDeleted);

			LoadBusinessObjects();
		}

		//--------------------------------------------------------------------------------
		private void editor_ComponentDeleted(object sender, EventArgs e)
		{
			RefreshUI();
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			InitializeFilters();
		}


		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible && OpenTwinPanel != null)
				OpenTwinPanel(this, e);
		}


		//--------------------------------------------------------------------------------
		private void InitializeFilters()
		{
			mnuFilterNotExposed.Checked = true;
			mnuFilterInErrorState.Checked = true;
			mnuFilterExposedAndUsed.Checked = false;
		}
		//--------------------------------------------------------------------------------
		private void LoadBusinessObjects()
		{
			Cursor = Cursors.WaitCursor;
			//inibisce il check changed event handler
			try
			{
				treeBusinessObjects.BeginUpdate();

				treeBusinessObjects.Scrollable = false;
				treeBusinessObjects.Nodes.Clear();
				treeBusinessObjects.ImageList = ImageLists.TreeBusinessObject();

				LoadTreeWithDocuments();

				treeBusinessObjects.TreeViewNodeSorter = new TreeNodeComparer();
				treeBusinessObjects.Sort();
			}
			finally
			{
				ExpandFirstNode();
				Cursor = Cursors.Default;
				treeBusinessObjects.Scrollable = true;
				treeBusinessObjects.EndUpdate();
			}
		}

		//--------------------------------------------------------------------------------
		private void UpdateNodeUI(TreeNode node, Status status)
		{
			switch (status)
			{
				case Status.Exposed:
					node.ForeColor = Color.Green;
					break;
				case Status.InErrorState:
					node.ForeColor = Color.Red;
					break;
				default:
					node.ForeColor = treeBusinessObjects.ForeColor;
					break;
			}
			treeBusinessObjects.Update();
		}

		//--------------------------------------------------------------------------------
		private void LoadTreeWithDocuments()
		{
			documentsNode = new TreeNode(ControllerSources.BusinessObjectsPropertyName, 0, 0);
			treeBusinessObjects.Nodes.Add(documentsNode);

			try
			{
				List<string> appTitles = new List<string>();
				List<string> modTitles = new List<string>();
				List<string> titles = new List<string>();
				List<string> docNamespaces = new List<string>();
				CUtility.GetDocuments(appTitles, modTitles, titles, docNamespaces);
				if (titles.Count == 0)
					return;

				for (int i = 0; i < titles.Count; i++)
				{
					string app = appTitles[i];
					string mod = modTitles[i];
					string title = titles[i];
					string nameSpace = docNamespaces[i];

					NameSpace docNs = new NameSpace(nameSpace);
					Status status = GetStatus(docNs);

					if (IsFiltered(status))
						continue;
					
					TreeNode appNode = GetNode(documentsNode.Nodes, app, (int)ImageLists.TreeBusinessObjectImageIndex.Application);
					TreeNode modNode = GetNode(appNode.Nodes, mod, (int)ImageLists.TreeBusinessObjectImageIndex.Module);
					TreeNode docNode = GetNode(modNode.Nodes, title, (int)ImageLists.TreeBusinessObjectImageIndex.BusinessObject24x24);

					appNode.Expand();

					docNode.Tag = docNs;
					UpdateNodeUI(docNode, status);
				}
			}
			finally
			{
				Cursor = Cursors.Default;
				treeBusinessObjects.Scrollable = true;
				documentsNode.Expand();
			}
		}

		//--------------------------------------------------------------------------------
		private bool IsFiltered(Status status)
		{
			bool excluded = false;
			switch (status)
			{
				case Status.NotExposed: excluded = !mnuFilterNotExposed.Checked; break;
				case Status.InErrorState: excluded = !mnuFilterInErrorState.Checked; break;
				case Status.Exposed: excluded = !mnuFilterExposedAndUsed.Checked; break;
			}
			return excluded;
		}

		//-----------------------------------------------------------------------------
		private Status GetStatus(NameSpace ns)
		{
			bool inCustomization = editor.ComponentDeclarator.IsReferenced(objectType, ns);
			bool exposed = editor.ComponentDeclarator.IsDeclared(objectType, ns);

			if (!inCustomization && !exposed)
				return Status.NotExposed;
			else if (inCustomization && exposed)
				return Status.Exposed;

			return Status.InErrorState;
		}

		//-----------------------------------------------------------------------------
		private TreeNode GetNode(TreeNodeCollection nodes, string text, int imageindex)
		{
			foreach (TreeNode node in nodes)
				if (node.Text == text)
					return node;
			TreeNode n = new TreeNode(text, imageindex, imageindex);
			n.Name = text;
			nodes.Add(n);
			return n;
		}

		//-----------------------------------------------------------------------------
		private void ExpandFirstNode()
		{
			if (treeBusinessObjects.Nodes.Count > 0)
				treeBusinessObjects.Nodes[0].Expand();
		}

		//-----------------------------------------------------------------------------
		private void treeDocuments_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
				treeBusinessObjects.SelectedNode = treeBusinessObjects.GetNodeAt(e.X, e.Y);
			else if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				dragNode = (TreeNode)treeBusinessObjects.GetNodeAt(e.Location);
				//Se il nodo selezionato è nullo o diverso dal precedente selezionato, 
				//viene impostato il nodo su cui si è effettuata il click per il drag drop
				if (dragNode != null && (treeBusinessObjects.SelectedNode == null || treeBusinessObjects.SelectedNode != dragNode))
					treeBusinessObjects.SelectedNode = dragNode;
			}
		}

		//-----------------------------------------------------------------------------
		private void treeDocuments_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			if (dragNode == null || dragNode.Tag == null)
				return;

			try
			{
				NameSpace nameSpace = dragNode.Tag as NameSpace;

				if (GetStatus(nameSpace) == Status.Exposed)
					return;

				ComponentDeclarationRequest dropObject = new ComponentDeclarationRequest
					(
						ComponentDeclarationRequest.Action.Add, 
						typeof(BusinessObject), 
						nameSpace,
						this.treeBusinessObjects
					);
				if (dropObject != null)
					DoDragDrop(dropObject, DragDropEffects.Copy);
				
				Status status = GetStatus(nameSpace);

				if (IsFiltered(status))
					dragNode.Parent.Nodes.Remove(dragNode);
				else
					UpdateNodeUI(dragNode, status);
			}
			catch { }		
		}
	
		//-----------------------------------------------------------------------------
		private void RefreshUI()
		{
			string selectedNode = string.Empty;
			if (treeBusinessObjects.SelectedNode != null)
				selectedNode = treeBusinessObjects.SelectedNode.Text;
			
			UseWaitCursor = true;
			treeBusinessObjects.Nodes.Clear();
			LoadBusinessObjects();

			if (selectedNode != string.Empty)
			{
				TreeNode[] nodes = treeBusinessObjects.Nodes.Find(selectedNode, true);
				if (nodes.Length > 0)
					treeBusinessObjects.SelectedNode = nodes[0];
			}
			
			UseWaitCursor = false;
		}

		//-----------------------------------------------------------------------------
		private void mnuFilterNotExposed_Click(object sender, EventArgs e)
		{
			mnuFilterNotExposed.Checked = !mnuFilterNotExposed.Checked;
			RefreshUI();
		}

		//-----------------------------------------------------------------------------
		private void mnuFilterInErrorState_Click(object sender, System.EventArgs e)
		{
			mnuFilterInErrorState.Checked = !mnuFilterInErrorState.Checked;
			RefreshUI();
		}

		//-----------------------------------------------------------------------------
		private void mnuFilterExposedAndUsed_Click(object sender, EventArgs e)
		{
			mnuFilterExposedAndUsed.Checked = !mnuFilterExposedAndUsed.Checked;
			RefreshUI();
		}
	}
}
