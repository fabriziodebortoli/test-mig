using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
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

        List<Tuple<string, string, string, string>> DocsInfos = null; 

        /// <summary>
        /// 
        /// </summary>
        //--------------------------------------------------------------------------------
        public BusinessObjectsExplorer(Editor editor)
		{
			InitializeComponent();
			this.Text = Resources.BusinessObjectsExplorer;

            this.editor = editor;
            this.editor.ComponentDeleted += new EventHandler<DeleteObjectEventArgs>(editor_ComponentDeleted);

            InitTreeNodesInfos();

			LoadBusinessObjects();
		}

        //------------------------------------------------------------------------------------------
        private void InitTreeNodesInfos()
        {
            DocsInfos = new List<Tuple<string, string, string, string>>();
        }

        //--------------------------------------------------------------------------------
        private void editor_ComponentDeleted(object sender, DeleteObjectEventArgs e)
        {
            string selectedNode = string.Empty;
            if (treeBusinessObjects.SelectedNode != null)
                selectedNode = treeBusinessObjects.SelectedNode.Text;

            var deletedComp = e.Component as Microarea.EasyBuilder.ReferenceableComponent; //Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent;

            if (deletedComp != null)
            {
                NameSpace nsDoc = deletedComp.Component as NameSpace;

                if (nsDoc != null)
                {
                    string ns = nsDoc.ToString();
                    var findDeletedDoc = DocsInfos.Where(d => (d.Item4 == ns)).Single();

                    Status status = GetStatus(nsDoc);
                    if (!IsFiltered(status))
                    {
                        TreeNode appNode = GetNode(documentsNode.Nodes, findDeletedDoc.Item1, 0);
                        TreeNode modNode = GetNode(appNode.Nodes, findDeletedDoc.Item2, 0);
                        TreeNode docNode = GetNode(modNode.Nodes, findDeletedDoc.Item3, (int)ImageLists.TreeBusinessObjectImageIndex.BusinessObject24x24);
                        if (docNode != null)
                            docNode.Tag = nsDoc;
                        UpdateNodeUI(docNode, status);
                    }
                }
            }

            if (selectedNode != string.Empty)
            {
                TreeNode[] nodes = treeBusinessObjects.Nodes.Find(selectedNode, true);
                if (nodes.Length > 0)
                    treeBusinessObjects.SelectedNode = nodes[0];
            }
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
            SuspendLayout();
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
                documentsNode.Expand();
                treeBusinessObjects.EndUpdate();
                ResumeLayout();
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
        }

		//--------------------------------------------------------------------------------
		private void LoadTreeWithDocuments()
		{
            TreeNode appNode = null;

            documentsNode = new TreeNode(ControllerSources.BusinessObjectsPropertyName, 0, 0);
			treeBusinessObjects.Nodes.Add(documentsNode);

			try
			{
                DocsInfos.Clear();
				CUtility.GetDocuments(DocsInfos);
                
                if (DocsInfos.Count == 0)
                    return;

                var apps = DocsInfos.Select(d => d.Item1).Distinct();
                foreach (string app in apps)
                {
                    appNode = GetNode(documentsNode.Nodes, app, (int)ImageLists.TreeBusinessObjectImageIndex.Application);
                    appNode.Expand();
                    //load all modules
                    var mods = DocsInfos.Where(m => m.Item1 == app).Distinct().OrderBy(x => x.Item2);
                    foreach (Tuple<string, string, string, string> mod in mods)
                    {
                        TreeNode modNode = GetNode(appNode.Nodes, mod.Item2, (int)ImageLists.TreeBusinessObjectImageIndex.Module);

                        var docs = DocsInfos.Where(d => (d.Item1 == app && d.Item2 == mod.Item2)).Distinct().OrderBy(x => x);
                        
                        foreach (Tuple<string, string, string, string> doc in docs)
                        {
                            string ns = doc.Item4;
                            NameSpace docNS = new NameSpace(ns);
                            Status status = GetStatus(docNS);
                            if (IsFiltered(status))
                                continue;
                            //load documents for this mod
                            TreeNode docNode = GetNode(modNode.Nodes, doc.Item3, (int)ImageLists.TreeBusinessObjectImageIndex.BusinessObject24x24);
                            if (docNode != null)
                                docNode.Tag = docNS;
                            UpdateNodeUI(docNode, status);
                        }
                    }
                }
            }
			finally
			{
                
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
                {
                    UpdateNodeUI(dragNode, status);
                    //treeBusinessObjects.Update();
                }
			}
			catch { }		
		}
	
		//-----------------------------------------------------------------------------
		private void RefreshUI()
		{
			string selectedNode = string.Empty;
			if (treeBusinessObjects.SelectedNode != null)
				selectedNode = treeBusinessObjects.SelectedNode.Text;
			
			LoadBusinessObjects();

			if (selectedNode != string.Empty)
			{
				TreeNode[] nodes = treeBusinessObjects.Nodes.Find(selectedNode, true);
				if (nodes.Length > 0)
					treeBusinessObjects.SelectedNode = nodes[0];
			}
			
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
