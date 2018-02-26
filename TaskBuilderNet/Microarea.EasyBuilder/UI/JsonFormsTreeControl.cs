using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using System.Diagnostics;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	internal partial class JsonFormsTreeControl : UserControl
	{
		private FormEditor editor;

		internal event EventHandler<JsonFormSelectedEventArgs> JsonFormOpenSelected;
		internal event EventHandler<JsonFormSelectedEventArgs> JsonFormDeleted;
		internal event EventHandler<JsonFormSelectedEventArgs> JsonItemRename;

		internal event EventHandler<JsonNodeSelectedEventArgs> JsonItemAdd;

		private bool suspendSelection;
		private TreeNode previousNode;
		//-----------------------------------------------------------------------------
		public JsonFormsTreeControl(FormEditor editor)
		{
			InitializeComponent();
			treeViewManager.TreeViewNodeSorter = new JsonTreeNodeComparer();
			this.Text = Resources.FormManagementTitle;
			this.editor = editor;

			TreeViewExtender extender = new TreeViewExtender(treeViewManager);
			treeViewManager.ImageList = ImageLists.JsonFormsTreeControl;
			PopulateTree();
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public void PopulateTree()
		{
			string[] pathTokens = null;
			if (treeViewManager.SelectedNode != null)
			{
				pathTokens = treeViewManager.SelectedNode.FullPath.Split(new[] { treeViewManager.PathSeparator }, StringSplitOptions.None);
			}
			tsbRefresh.Enabled = tsbRename.Enabled = true;
			tsbDelete.Enabled = tsbOpen.Enabled = tsbAdd.Enabled = tsbOpenLocation.Enabled = false;
			treeViewManager.BeginUpdate();
			treeViewManager.Nodes.Clear();

			List<ClientDocumentInfo> clients = new List<ClientDocumentInfo>();
			foreach (BaseApplicationInfo ai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				if (ai.ApplicationType != (ApplicationType.TaskBuilder) && ai.ApplicationType != (ApplicationType.TaskBuilderApplication)) continue;
				TreeNode appNode = new TreeNode(ai.Name, (int)ImageLists.JsonFormsTreeImageIndex.Application, (int)ImageLists.JsonFormsTreeImageIndex.Application);
				appNode.Name = ai.Name;
				appNode.Tag = ai;
				treeViewManager.Nodes.Add(appNode);
				foreach (BaseModuleInfo mi in ai.Modules)
				{
					TreeNode modNode = new TreeNode(mi.Name, (int)ImageLists.JsonFormsTreeImageIndex.Module, (int)ImageLists.JsonFormsTreeImageIndex.Module);
					modNode.ToolTipText = mi.Title;
					modNode.Name = mi.Name;
					modNode.Tag = mi;
					appNode.Nodes.Add(modNode);

					AddJsonFiles(mi.NameSpace, modNode);
					foreach (DocumentInfo di in mi.Documents)
					{
						TreeNode docNode = new TreeNode(di.Name, (int)ImageLists.JsonFormsTreeImageIndex.Document, (int)ImageLists.JsonFormsTreeImageIndex.Document);
						docNode.ToolTipText = di.Title;
						docNode.Name = di.Name;
						docNode.Tag = di;
						modNode.Nodes.Add(docNode);
						AddJsonFiles(di.NameSpace, docNode);
					}
					if (mi.ClientDocumentsObjectInfo != null && mi.ClientDocumentsObjectInfo.ServerDocuments != null)
					{
						foreach (ServerDocumentInfo sd in mi.ClientDocumentsObjectInfo.ServerDocuments)
						{
							foreach (ClientDocumentInfo cd in sd.ClientDocsInfos)
							{
								if (!clients.Contains(cd, new ClientDocumentInfoComparer()))
									clients.Add(cd);
							}
						}
					}

				}
			}

			foreach (ClientDocumentInfo cd in clients)
			{
				string name = cd.NameSpace.Document;
				TreeNode docNode = new TreeNode(name, (int)ImageLists.JsonFormsTreeImageIndex.Document, (int)ImageLists.JsonFormsTreeImageIndex.Document);
				docNode.ToolTipText = cd.Title;
				docNode.Name = name;
				docNode.Tag = cd;
				TreeNode modNode = FindNode(cd.NameSpace.Application, cd.NameSpace.Module, null);
				if (modNode == null)
				{
					Debug.Assert(false);
					continue;
				}
				modNode.Nodes.Add(docNode);
				AddJsonFiles(cd.NameSpace, docNode);
			}

			//aggiungo cartella dove raccogliere i file esportati da EasyStudio
			string custPath = BasePathFinder.BasePathFinderInstance.GetCustomModulePath(
                BasePathFinder.GetEasyStudioHomeFolderName(), NameSolverStrings.Extensions, NameSolverStrings.EasyStudio);
			custPath = Path.Combine(custPath, NameSolverStrings.JsonForms, NameSolverStrings.AllUsers);
			if (!Directory.Exists(custPath))
				Directory.CreateDirectory(custPath);
			TreeNode custNode = new TreeNode("CUSTOM", (int)ImageLists.JsonFormsTreeImageIndex.Application, (int)ImageLists.JsonFormsTreeImageIndex.Application);
			custNode.Tag = custPath;
			custNode.ToolTipText = "Json dialogs saved in ES";
			treeViewManager.Nodes.Add(custNode);
			AddJsonFiles(null, custNode, custPath); //path troppo specifico per farlo calcolare alla AddJsonFile con 2 param


			treeViewManager.Sort();
			treeViewManager.EndUpdate();

			TreeNode toSelect = null;
			if (pathTokens != null)
			{
				TreeNodeCollection nodes = treeViewManager.Nodes;
				foreach (var item in pathTokens)
				{
					TreeNode[] foundNodes = nodes.Find(item, false);
					if (foundNodes.Length != 1)
						break;
					toSelect = foundNodes[0];
					nodes = toSelect.Nodes;
				}
			}

			treeViewManager.SelectedNode = toSelect;
			if (treeViewManager.SelectedNode != null && !treeViewManager.SelectedNode.IsExpanded)
				treeViewManager.SelectedNode.Expand();

		}

		//--------------------------------------------------------------------------------
		private void AddJsonFiles(INameSpace owner, TreeNode parentNode)
		{
			string jsonPath = BasePathFinder.BasePathFinderInstance.GetJsonFormPath(owner);
			if (Directory.Exists(jsonPath))
			{
				AddJsonFiles(owner, parentNode, jsonPath);
			}
		}

		//--------------------------------------------------------------------------------------------------
		private static void AddJsonFiles(INameSpace owner, TreeNode parentNode, string jsonPath)
		{
			foreach (var jsonFile in Directory.GetFiles(jsonPath, '*' + NameSolverStrings.TbjsonExtension))
			{
				string text = Path.GetFileNameWithoutExtension(jsonFile);
				TreeNode jsonNode = new TreeNode(text, (int)ImageLists.JsonFormsTreeImageIndex.Form, (int)ImageLists.JsonFormsTreeImageIndex.Form);
				jsonNode.Tag = new JsonFormSelectedEventArgs(jsonFile, owner);
				jsonNode.Name = text;
				parentNode.Nodes.Add(jsonNode);
			}
			foreach (var jsonFolder in Directory.GetDirectories(jsonPath))
			{
				string text = Path.GetFileNameWithoutExtension(jsonFolder);
				TreeNode jsonNode = new TreeNode(text, (int)ImageLists.JsonFormsTreeImageIndex.Folder, (int)ImageLists.JsonFormsTreeImageIndex.Folder);
				jsonNode.Tag = jsonFolder;
				jsonNode.Name = text;
				parentNode.Nodes.Add(jsonNode);
				AddJsonFiles(owner, jsonNode, jsonFolder);
			}
		}

		//--------------------------------------------------------------------------------------------------
		public void SelectJsonForm(string file, INameSpace nameSpace)
		{
			treeViewManager.SelectedNode = FindNode(treeViewManager.Nodes, file);
		}

		//--------------------------------------------------------------------------------------------------
		public void SelectNode(string app, string mod, string doc)
		{
			if (suspendSelection) return;
			if (app == null) return;

			TreeNode n = FindNode(app, mod, doc);
			treeViewManager.SelectedNode = n;
		}
	
		//-----------------------------------------------------------------------------
		private TreeNode FindNode(string app, string mod, string doc)
		{
			TreeNode n = null;
			foreach (TreeNode appNode in treeViewManager.Nodes)
			{
				if (appNode.Name.Equals(app, StringComparison.InvariantCultureIgnoreCase))
				{
					n = appNode;
					if (mod == null) break;
					foreach (TreeNode modNode in appNode.Nodes)
					{
						if (modNode.Name.Equals(mod, StringComparison.InvariantCultureIgnoreCase))
						{
							n = modNode;
							if (doc == null) break;
							foreach (TreeNode docNode in modNode.Nodes)
							{
								if (docNode.Name.Equals(doc, StringComparison.InvariantCultureIgnoreCase))
								{
									n = docNode;
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}

			return n;
		}

		//-----------------------------------------------------------------------------
		private void SetNodeLikeSelected()
		{
			if (previousNode != null)
			{
				previousNode.BackColor = Color.Transparent;
				previousNode.ForeColor = Color.Black;
			}
			previousNode = treeViewManager.SelectedNode;
			if (previousNode != null)
			{
				previousNode.BackColor = SystemColors.Highlight;
				previousNode.ForeColor = SystemColors.HighlightText;
			}
		}

		//--------------------------------------------------------------------------------------------------
		private TreeNode FindNode(TreeNodeCollection treeNodeCollection, string file)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				JsonFormSelectedEventArgs args = item.Tag as JsonFormSelectedEventArgs;
				if (args != null && args.JsonFile.Equals(file, StringComparison.InvariantCultureIgnoreCase))
				{
					return item;
				}
				TreeNode child = FindNode(item.Nodes, file);
				if (child != null)
					return child;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		private void treeViewManager_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (JsonFormOpenSelected == null)
				return;

			JsonFormSelectedEventArgs args = e.Node.Tag as JsonFormSelectedEventArgs;
			if (args == null)
				return;

			OpenForm(args);
		}

		//-----------------------------------------------------------------------------
		private void treeViewManager_AfterSelect(object sender, TreeViewEventArgs e)
		{
			tsbOpenLocation.Enabled = renameToolStripMenuItem.Enabled = tsbRename.Enabled =
				openLocationToolStripMenuItem.Enabled = tsbRefresh.Enabled = true;

			tsbDelete.Enabled = deleteToolStripMenuItem.Enabled = tsbOpen.Enabled =
			openToolStripMenuItem.Enabled = openInNewWindowToolStripMenuItem.Enabled = e.Node.Tag is JsonFormSelectedEventArgs;

			if (e.Node.Tag is string)
				tsbDelete.Enabled = deleteToolStripMenuItem.Enabled = true;

			JsonNodeSelectedEventArgs args = GetSelectedNodeInfo(e.Node);

			//rename consentito su cartelle e tile/panel, non su moduli, documenti e application
			if (	(args.Module != null		&& e.Node.Text.Equals(args.Module.ToString(), StringComparison.InvariantCultureIgnoreCase)) 
				||  (args.Document != null		&& e.Node.Text.Equals(args.Document.ToString(), StringComparison.InvariantCultureIgnoreCase))
				||	(args.Application != null	&& e.Node.Text.Equals(args.Application.ToString(), StringComparison.InvariantCultureIgnoreCase))
				)
					renameToolStripMenuItem.Enabled = tsbRename.Enabled = false;

			//se non ho un contesto, non posso aggiungere il form
			tsbAdd.Enabled = addToolStripMenuItem.Enabled =
				args.Document != null || args.Module != null || args.Folder != null;
			//evito eventuali selezioni 'di ritorno' scatenate dal gestore dell'evento
			suspendSelection = true;
			suspendSelection = false;
			SetNodeLikeSelected();

		}

		//-----------------------------------------------------------------------------
		private JsonNodeSelectedEventArgs GetSelectedNodeInfo(TreeNode n)
		{
			JsonNodeSelectedEventArgs args = new JsonNodeSelectedEventArgs();

			if (n.Tag is BaseApplicationInfo)
			{
				args.Application = ((BaseApplicationInfo)n.Tag).Name;
			}
			else if (n.Tag is BaseModuleInfo)
			{
				args.Application = ((BaseModuleInfo)n.Tag).ParentApplicationName;
				args.Module = ((BaseModuleInfo)n.Tag).Name;
			}
			else if (n.Tag is DocumentInfo)
			{
				args.Application = ((DocumentInfo)n.Tag).OwnerModule.ParentApplicationName;
				args.Module = ((DocumentInfo)n.Tag).OwnerModule.Name;
				args.Library = ((DocumentInfo)n.Tag).NameSpace.Library;
				args.Document = ((DocumentInfo)n.Tag).Name;
			}
			else if (n.Tag is ClientDocumentInfo)
			{
				args.Application = ((ClientDocumentInfo)n.Tag).NameSpace.Application;
				args.Module = ((ClientDocumentInfo)n.Tag).NameSpace.Module;
				args.Library = ((ClientDocumentInfo)n.Tag).NameSpace.Library;
				args.Document = ((ClientDocumentInfo)n.Tag).NameSpace.Document;
			}
			else if (n.Tag is JsonFormSelectedEventArgs)
			{
				INameSpace owner = ((JsonFormSelectedEventArgs)n.Tag).Owner;
				if(owner != null)
				{
					args.Application = owner.Application;
					args.Module = owner.Module;
					args.Library = owner.NameSpaceType.Type == NameSpaceObjectType.Document
						? owner.Library		: null;
					args.Document = owner.NameSpaceType.Type == NameSpaceObjectType.Document
						? owner.Document	: null;
				}

				if (n.Parent != null && n.Parent.Tag is string)//folder
				{
					args.Folder = (string)n.Parent.Tag;
				}
			}
			else if (n.Tag is string)//folder
			{
				if(n.Parent!=null)
					args = GetSelectedNodeInfo(n.Parent);
				args.Folder = (string)n.Tag;
			}

			return args;
		}

		//-----------------------------------------------------------------------------
		private void tsbDelete_Click(object sender, EventArgs e)
		{
			if (JsonFormDeleted == null)
				return;
			TreeNode n = treeViewManager.SelectedNode;
			if (n == null)
				return;

			JsonFormSelectedEventArgs args = n.Tag as JsonFormSelectedEventArgs;
			if (args != null)
				JsonFormDeleted(this, args);
			else
			{
				var args2 = n.Tag as string;
				if (args2 != null)
					JsonFormDeleted(this, new JsonFormSelectedEventArgs(args2, null));
			}
		}

		//-----------------------------------------------------------------------------
		private void tsbRename_Click(object sender, EventArgs e)
		{
			if (JsonItemRename == null)
				return;
			TreeNode n = treeViewManager.SelectedNode;
			if (n == null)
				return;
			JsonFormSelectedEventArgs args = n.Tag as JsonFormSelectedEventArgs;
			if (args != null)
				JsonItemRename(this, args);
			else
			{
				var args2 = n.Tag as string;
				if (args2 != null)
					JsonItemRename(this, new JsonFormSelectedEventArgs(args2, null));
			}
		}

		//-----------------------------------------------------------------------------
		private void tsbAdd_Click(object sender, EventArgs e)
		{
			if (JsonItemAdd == null)
				return;
			TreeNode n = treeViewManager.SelectedNode;
			if (n == null)
				return;
			JsonNodeSelectedEventArgs args = GetSelectedNodeInfo(n);
			//se non ho un contesto, non posso aggiungere il form
			if (args.Document == null && args.Module == null && args.Folder == null)
				return;
			JsonItemAdd(this, args);
		}

		//-----------------------------------------------------------------------------
		private void tsbOpen_Click(object sender, EventArgs e)
		{
			SelectAndOpenForm(false);
		}

		//-----------------------------------------------------------------------------
		private void tsbOpenInNewWindow_Click(object sender, EventArgs e)
		{
			SelectAndOpenForm(true);
		}

		//-----------------------------------------------------------------------------
		private void SelectAndOpenForm(bool openInNewWindow)
		{
			TreeNode n = treeViewManager.SelectedNode;
			if (n == null)
				return;

			JsonFormSelectedEventArgs args = n.Tag as JsonFormSelectedEventArgs;
			if (args == null)
				return;

			args.OpenInNewWindow = openInNewWindow;

			OpenForm(args);

		}

		//-----------------------------------------------------------------------------
		private void OpenForm(JsonFormSelectedEventArgs args)
		{
			JsonFormOpenSelected(this, args);
		}

		//-----------------------------------------------------------------------------
		private void treeViewManager_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				treeViewManager.SelectedNode = e.Node;
		}

		//-----------------------------------------------------------------------------
		private void openLocationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			object arg = treeViewManager?.SelectedNode?.Tag;
			if (arg == null)
				return;
			string fileLocation = "";
			if (arg is JsonFormSelectedEventArgs)
				fileLocation = ((JsonFormSelectedEventArgs)arg).JsonFile;
			else if (arg is DocumentInfo)
				fileLocation = BasePathFinder.BasePathFinderInstance.GetDocumentPath(((DocumentInfo)arg).NameSpace);
			else if (arg is ClientDocumentInfo)
				fileLocation = BasePathFinder.BasePathFinderInstance.GetDocumentPath(((ClientDocumentInfo)arg).NameSpace);
			else if (arg is BaseModuleInfo)
				fileLocation = ((BaseModuleInfo)arg).Path;
			else if (arg is BaseApplicationInfo)
				fileLocation = ((BaseApplicationInfo)arg).Path;
			else if (arg is string)
				fileLocation = (string)arg;
			if (fileLocation.EndsWith(NameSolverStrings.TbjsonExtension))
				fileLocation = Path.GetDirectoryName(fileLocation);
			if (!Directory.Exists(fileLocation))
				return;
			Process.Start(fileLocation);
		}

		//-----------------------------------------------------------------------------
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tsbRename_Click(sender, e);
		}

		//-----------------------------------------------------------------------------
		private void tsbRefresh_Click(object sender, EventArgs e)
		{
			PopulateTree();
		}


		//-----------------------------------------------------------------------------
		/*internal void ResetSelections()
		{
			treeViewManager.SelectedNode = null;
			SetNodeLikeSelected();
		}*/

		//-----------------------------------------------------------------------------
		private void treeViewManager_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == (int)Keys.Delete)
				tsbDelete_Click( sender, e);
			if (e.KeyValue == (int)Keys.Enter)
				tsbOpen_Click(sender, e);
		}
	}

	//================================================================================
	internal class JsonNodeSelectedEventArgs : EventArgs
	{
		public string Application { get; set; }
		public string Module { get; set; }
		public string Library { get; set; }
		public string Document { get; set; }
		public INameSpace NameSpace
		{
			get
			{
				if (Module == null || Application == null)
					return null;
				return (Document == null)
					? new NameSpace(string.Concat(NameSpaceSegment.Module, ".", Application, ".", Module))
					: new NameSpace(string.Concat(NameSpaceSegment.Document, ".", Application, ".", Module, ".", Library, ".", Document));
			}
		}

		public string Folder { get; set; }
	}

	//================================================================================
	internal class JsonFormSelectedEventArgs : EventArgs
	{
		private string jsonFile;
		private INameSpace owner;
		public bool OpenInNewWindow { get; set; }
		public string JsonFile
		{
			get { return jsonFile; }
		}

		public INameSpace Owner
		{
			get { return owner; }
		}

		public JsonFormSelectedEventArgs(string jsonFile, INameSpace owner, bool openInNewWindow = false)
		{
			this.jsonFile = jsonFile;
			this.owner = owner;
			this.OpenInNewWindow = openInNewWindow;
		}

	}

	//================================================================================
	internal class ClientDocumentInfoComparer : IEqualityComparer<ClientDocumentInfo>
	{
		//--------------------------------------------------------------------------------------------------
		public bool Equals(ClientDocumentInfo x, ClientDocumentInfo y)
		{
			return x.NameSpace.Equals(y.NameSpace);
		}

		//--------------------------------------------------------------------------------------------------
		public int GetHashCode(ClientDocumentInfo obj)
		{
			return obj.NameSpace.GetHashCode();
		}
	}

	//================================================================================
	internal class JsonTreeNodeComparer : IComparer, IComparer<TreeNode>
	{
		//--------------------------------------------------------------------------------------------------
		public int Compare(TreeNode x, TreeNode y)
		{
			if (x.Tag == null && y.Tag != null)
				return 1;
			if (x.Tag != null && y.Tag == null)
				return -1;
			if (x.Tag == null && y.Tag == null)
				return x.Name.CompareTo(y.Name);

			int cmp = x.Tag.GetType().ToString().CompareTo(y.Tag.GetType().ToString());
			if (cmp != 0)
				return cmp;
			return x.Name.CompareTo(y.Name);
		}

		//--------------------------------------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			return Compare((TreeNode)x, (TreeNode)y);
		}

	}
}

