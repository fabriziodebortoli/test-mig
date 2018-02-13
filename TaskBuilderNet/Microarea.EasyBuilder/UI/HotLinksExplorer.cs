using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;
using System.Drawing;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// 
	/// </summary>
	public partial class HotLinksExplorer : UserControl
	{
		/// <remarks/>
		public event EventHandler OpenTwinPanel;
		/// <remarks/>
		public event EventHandler<NodeArgs> NodeSelected;
		private bool propertyGridMode;
		private TreeNode patternsNode;
		private TreeNode documentNode;
		private bool showTemplates = true;
		TreeNode dragNode;
        Action<TreeNode> treeFiller;
		private const string ReferenceObjectsString = "ReferenceObjects";

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public HotLinksExplorer(Action<TreeNode> treeFiller, bool usedInGridJson = false)
		{
			InitializeComponent();
            this.treeFiller = treeFiller;
			this.Text = Resources.HotLinksExplorer;
			this.propertyGridMode = usedInGridJson;

			LoadTree();

			if (propertyGridMode)
			{
				Controls.Remove(ToolBar);
				Controls.Remove(lblTreeCaption);
				TreeHotLinks.Location = new Point(0, 0);
				TreeHotLinks.Height = Height;
				TreeHotLinks.Width = Width;
			}
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



		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public void RefreshDocument()
		{
            if (documentNode == null)
                return;
			treeHotLinks.Nodes.Remove(documentNode);
			documentNode = null;
			LoadTreeWithDocumentHotLinks();
			ExpandFirstNode();
			Update();
		}

		//--------------------------------------------------------------------------------
		private void LoadTree()
		{
			Cursor = Cursors.WaitCursor;
			treeHotLinks.BeginUpdate();
			//inibisce il check changed event handler
			try
			{
				treeHotLinks.Scrollable = false;
				treeHotLinks.Nodes.Clear();
				treeHotLinks.ImageList = new ImageList();
				treeHotLinks.ImageList.Images.Add(Resources.Document);
				treeHotLinks.ImageList.Images.Add(Resources.Repository);
				treeHotLinks.ImageList.Images.Add(Resources.Application32x32);
				treeHotLinks.ImageList.Images.Add(Resources.Module32x32);
				treeHotLinks.ImageList.Images.Add(Resources.MiniHotLink);

				if (showTemplates)
					LoadTreeWithHotLinksTemplates();

				LoadTreeWithDocumentHotLinks();

				treeHotLinks.TreeViewNodeSorter = new TreeNodeComparer();
				treeHotLinks.Sort();

				/*if (propertyGridMode)   attivare solo quando gli hotlink in custom dynamic saranno utilizzabili
					LoadTreeCustom();*/

			}
			finally
			{
				Cursor = Cursors.Default;
				treeHotLinks.Scrollable = true;
				treeHotLinks.EndUpdate();
			}
		}

		//--------------------------------------------------------------------------------
		private void LoadTreeWithHotLinksTemplates()
		{
			patternsNode = new TreeNode(Resources.HotLinksTemplates, 1, 1);
			treeHotLinks.Nodes.Add(patternsNode);
			patternsNode.Expand();

			try
			{
				List<string> appTitles = new List<string>();
				List<string> modTitles = new List<string>();
				List<string> titles = new List<string>();
				List<string> moduleNamespaces = new List<string>();
				CUtility.GetHotLinks(appTitles, modTitles, titles, moduleNamespaces);
				if (titles.Count == 0)
					return;

				for (int i = 0; i < titles.Count; i++)
				{
					string app = appTitles[i];
					string mod = modTitles[i];
					string title = titles[i];
					string nameSpace = moduleNamespaces[i];
					TreeNode appNode = GetNode(patternsNode.Nodes, app, 2);
					TreeNode modNode = GetNode(appNode.Nodes, mod, 3);
					TreeNode hklNode = GetNode(modNode.Nodes, title, 4);
					hklNode.Tag = new NameSpace(nameSpace);
				}
			}
			finally
			{
				Cursor = Cursors.Default;
				treeHotLinks.Scrollable = true;
			}
		}

		//-----------------------------------------------------------------------------
		private void LoadTreeWithDocumentHotLinks()
		{
            if (treeFiller == null)
                return;

            documentNode = new TreeNode(Resources.DocumentHotLinks, 0, 0);
            treeFiller(documentNode);

			if (documentNode.Nodes.Count > 0)
			{
				documentNode.Expand();
				treeHotLinks.Nodes.Add(documentNode);
			}
		}

		//--------------------------------------------------------------------------------
		private void LoadTreeCustom()
		{
			//aggiungo cartella dove raccogliere i file esportati da EasyStudio
			string custPath = BasePathFinder.BasePathFinderInstance.GetCustomAllCompaniesPath();
			custPath = Path.Combine(custPath, NameSolverStrings.Applications);
			
			string[] listApplications = Directory.GetDirectories(custPath);

			foreach (var applicPath in listApplications)
			{
				if (Path.GetFileName(applicPath) == "ERP") continue;
				//string applicPath = Path.Combine(custPath, item);
				TreeNode applicNode = treeHotLinks.Nodes.Add(Path.GetFileName(applicPath));
				applicNode.ImageIndex = 2;
				applicNode.Tag =  applicPath;
				string[] listModules = Directory.GetDirectories(applicPath);

				foreach (var module in listModules)
				{
					TreeNode moduleNode = applicNode.Nodes.Add(Path.GetFileName(module));
					moduleNode.ImageIndex = 3;
					moduleNode.Tag = moduleNode;
					string modulePath = Path.Combine(applicPath, module, ReferenceObjectsString);
					string[] listHotlinks = Directory.GetFiles(modulePath);
					foreach (var hl in listHotlinks)
					{
						TreeNode hlNode = moduleNode.Nodes.Add(Path.GetFileNameWithoutExtension(hl));
						hlNode.ImageIndex = 4;
						hlNode.Tag = hl;
					}
				}


			}

			return;
		}


		//-----------------------------------------------------------------------------
		private TreeNode GetNode(TreeNodeCollection nodes, string text, int imageindex)
		{
			foreach (TreeNode node in nodes)
				if (node.Text == text)
					return node;
			TreeNode n = new TreeNode(text, imageindex, imageindex);
			nodes.Add(n);
			return n;
		}

		//-----------------------------------------------------------------------------
		private void tbShowTemplates_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			treeHotLinks.Scrollable = false;
			Update();

			showTemplates = !showTemplates;
			if (showTemplates)
				LoadTreeWithHotLinksTemplates();
			else
			{
				treeHotLinks.Nodes.Remove(patternsNode);
				patternsNode = null;
			}

			Cursor = Cursors.Default;
			treeHotLinks.Scrollable = true;
			ExpandFirstNode();
			Update();
		}

		//-----------------------------------------------------------------------------
		private void ExpandFirstNode()
		{
			if (treeHotLinks.Nodes.Count > 0)
				treeHotLinks.Nodes[0].Expand();
		}

		//-----------------------------------------------------------------------------
		private void treeHotLinks_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
				treeHotLinks.SelectedNode = treeHotLinks.GetNodeAt(e.X, e.Y);
			else if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				dragNode = (TreeNode)treeHotLinks.GetNodeAt(e.Location);
				//Se il nodo selezionato è nullo o diverso dal precedente selezionato, 
				//viene impostato il nodo su cui si è effettuata il click per il drag drop
				if (dragNode != null && (treeHotLinks.SelectedNode == null || treeHotLinks.SelectedNode != dragNode))
					treeHotLinks.SelectedNode = dragNode;
			}
		}

		//-----------------------------------------------------------------------------
		private void treeHotLinks_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			if (dragNode == null)
				return;

			try
			{
				AddHotLinkEventArgs dropObject = null;
				string name = dragNode.Tag as string;
				if (name != null)
					dropObject = new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromDocument, name);
				else
				{
					NameSpace ns = dragNode.Tag as NameSpace;
					if (ns != null)
						dropObject = new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromTemplate, ns);
				}
				if (dropObject != null)
					DoDragDrop(dropObject, DragDropEffects.Copy);
			}
			catch { }		
		}

		//-----------------------------------------------------------------------------
		private void treeHotLinks_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			TreeNode nodeSelected = (TreeNode)treeHotLinks.GetNodeAt(e.Location);
			if (nodeSelected != null && NodeSelected != null)		
				NodeSelected(this, new NodeArgs() {NodeSelected=nodeSelected });

		}
	}

	/// <summary>
	/// 
	/// </summary>
	//-----------------------------------------------------------------------------
	public class NodeArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public TreeNode NodeSelected { get; set; }
	}


}
