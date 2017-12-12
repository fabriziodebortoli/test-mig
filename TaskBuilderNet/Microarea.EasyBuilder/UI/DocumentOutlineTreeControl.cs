using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microarea.EasyBuilder.Properties;
using Newtonsoft.Json.Linq;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;
using System.ComponentModel.Design;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	///		
	/// </summary>
	//=============================================================================
	public partial class DocumentOutlineTreeControl : UserControl
	{
		/// <remarks/>
		public event EventHandler OpenProperties;
		internal event EventHandler<JsonFormSelectedEventArgs> JsonFormOpenSelected;
		ISelectionService selectionService;
		private TreeNode previousNode;
		private FormEditor editor;

		private const string hrefPrefix = "href : ";
		private string fileLocation = string.Empty;
		private List<string> ids = new List<string>();
		private JsonPropertiesParser jsonPropParser;

		/// <remarks/>
		public string FileLocation { get { return fileLocation; } set { fileLocation = value; } }

		/// <remarks/>
		public JObject JsonDeserialized { get;  set; } //last Json Deserialiazed based on Tree ?

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public DocumentOutlineTreeControl(FormEditor editor)
		{
			InitializeComponent();
			this.Text = Resources.DocOutlineTitle;
			this.editor = editor;
			treeDocOutlineManager.ImageList = ImageLists.DocOutlineTree;
			selectionService = editor.GetService(typeof(ISelectionService)) as ISelectionService;

			if (selectionService != null)
				selectionService.SelectionChanged += new EventHandler(SelectionService_SelectionChanged);
		}

		//-----------------------------------------------------------------------------
		void SelectionService_SelectionChanged(object sender, EventArgs e)
		{
			if (selectionService == null)
				return;
			editor.SelectionService_SelectionChanged(sender, e);
		}

		//-----------------------------------------------------------------------------
		internal void AddToTheIdsList(string id)
		{
			if(id!=null)
				ids.Add(id);
		}

		//-----------------------------------------------------------------------------
		internal void DeserializeJson(string fileJsonPath)
		{
			FileLocation = fileJsonPath;
			jsonPropParser = new JsonPropertiesParser(FileLocation, editor);
			DocOutlineProperties doc = jsonPropParser.Parse();
			string json = File.ReadAllText(FileLocation);

			JsonDeserialized = doc?.InnerJObject;
			PopulateThroughDoc(doc);
			UpdateCodeEditor(false);
		}

		//-----------------------------------------------------------------------------
		private void PopulateThroughDoc(DocOutlineProperties root)
		{
			ids.Clear();
			string nameFirstNode = GetNodeName(root);
			int imageIndex = GetNumberImageFor(root?.Type ?? AllowedTypes.View);
			TreeNode firstNode = new TreeNode(GetNodeName(root), imageIndex, imageIndex);
			treeDocOutlineManager.Nodes.Clear();
			firstNode.Tag = root;
			firstNode.ToolTipText = GetTooltipFor(root);
			PopulateThroughDocRec(root, firstNode, 1);
			treeDocOutlineManager.Nodes.Add(firstNode);
			AddToTheIdsList(root.Id);
			treeDocOutlineManager.ShowNodeToolTips = true;
			treeDocOutlineManager.ExpandAll();
			treeDocOutlineManager.Invalidate();
		}

		//-----------------------------------------------------------------------------
		private string GetNodeName(DocOutlineProperties item)
		{
			string firstPart = Resources.NotFound.ToString();
			if (item != null)
			{
				firstPart = !string.IsNullOrEmpty(item.Id) ? item.Id :
					!string.IsNullOrEmpty(item.Href) ? hrefPrefix + item.Href :
					(item is LayoutContainerProperties) ? (item as LayoutContainerProperties).ToString() :
					!string.IsNullOrEmpty(item.Name) ? item.Name : Resources.NotFound.ToString();
			}
			return firstPart;
		}

		//-----------------------------------------------------------------------------
		private void PopulateThroughDocRec(DocOutlineProperties root, TreeNode firstNode, int level)
		{
			if (root?.Items == null)
				return;
			foreach (var item in root.Items)
			{
				if (item == null || !Enum.IsDefined(typeof(AllowedTypes), item.Type) ||
					(item.Type.ToString() == "Undefined" && item.Href == null))
					continue;

				int imageIndex = GetNumberImageFor(item.Type);
				TreeNode annidation = new TreeNode(GetNodeName(item), imageIndex, imageIndex);
				annidation.ToolTipText = GetTooltipFor(item);
				firstNode.Nodes.Add(annidation);
				AddToTheIdsList(annidation.Text);

				annidation.Tag = item;
				PopulateThroughDocRec(item, annidation, ++level);
			}
		}

		//-----------------------------------------------------------------------------
		private string GetTooltipFor(DocOutlineProperties item)
		{
			switch (item.Type)	
			{
				case AllowedTypes.Generic:
					return Resources.GenericElement;
				case AllowedTypes.Error:
					return Resources.ErrorParsingJson;//"Found errors parsing nested items";
				case AllowedTypes.Undefined:
					if (item.Href != null)
						return Resources.HRefDefinition;
					break;					
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		internal void DeserializeJsonByCode(string codeJson)
		{
			JsonDeserialized = JObject.Parse(codeJson);
			RefreshDocTree(); //non serve updatare il codeeditor
		}

		//-----------------------------------------------------------------------------
		internal void UpdateCodeEditor(bool refreshTree = true)
		{
			editor.jsonEditorConnector.UpdateWindow(GetJsonDeserialized());
			if (refreshTree)
				RefreshDocTree();
		}

		//-----------------------------------------------------------------------------
		internal void RefreshDocTree()
		{
			DocOutlineProperties doc = jsonPropParser.CreateWrapper(JsonDeserialized);
			PopulateThroughDoc(doc);
		}

		//-----------------------------------------------------------------------------
		internal string GetJsonDeserialized()
		{
			return JsonDeserialized.ToString();
		}

		//-----------------------------------------------------------------------------
		private void AddNodeDocTree(string newNodeName, JObject newItemCreated, AllowedTypes allowedType)
		{
			TreeNode selected = treeDocOutlineManager?.SelectedNode;
			if (selected == null || ((selected.Tag as DocOutlineProperties) == null))
				return;

			if (!editor.IsDirty)
				editor.SetDirty(true);

			TreeNode added = AddNewTreeNode(allowedType, selected, newNodeName, newItemCreated);
			AddJObjectToJsonDeserialized(((DocOutlineProperties)selected.Tag), newItemCreated);
			treeDocOutlineManager.SelectedNode = null;
			treeDocOutlineManager.SelectedNode = added;
		}

		//-----------------------------------------------------------------------------
		private TreeNode AddNewTreeNode(AllowedTypes type, TreeNode selected, string newNodeName, JObject newItemToCreate)
		{
			int imageIndex = GetNumberImageFor(type);
			treeDocOutlineManager.BeginUpdate();
			TreeNode added = new TreeNode(newNodeName, imageIndex, imageIndex);
			DocOutlineProperties propNewNode = CreateJsonItem(newItemToCreate);
			added.Tag = propNewNode;
			treeDocOutlineManager?.SelectedNode?.Nodes.Add(added);
			((DocOutlineProperties)treeDocOutlineManager?.SelectedNode?.Tag).Items.Add(propNewNode);
			AddToTheIdsList(newNodeName);
			added.EnsureVisible();
			treeDocOutlineManager.EndUpdate();
			return added;
		}

		//-----------------------------------------------------------------------------
		private void AddJObjectToJsonDeserialized(DocOutlineProperties docTreeNode, JObject added)
		{
			JObject selected = docTreeNode?.InnerJObject;
			if (selected == null)
				return;
			JArray items = selected["items"] as JArray;
			if (items == null)
			{
				items = new JArray();
				selected["items"] = items;
			}
			items.Add(added);
			if (!editor.IsDirty)
				editor.SetDirty(true);
			UpdateCodeEditor(false);
		}

		//-----------------------------------------------------------------------------
		private DocOutlineProperties CreateJsonItem(JToken jItem)
		{
			JObject jObj = jItem as JObject;
			if (jObj == null)
				return null;
			DocOutlineProperties propertiesTree = new DocOutlineProperties(jObj, editor);
			return jsonPropParser.GetPropertyFromType(propertiesTree.Type, jObj) ?? propertiesTree;
		}

		//-----------------------------------------------------------------------------
		private void SendToJsonFormOpen(DocOutlineProperties item, bool openInNewWindow = true)
		{
			if (JsonFormOpenSelected == null || (item.Type != AllowedTypes.Tile && item.Href == null))
				return;

			string jsonFile = CalculateId(item);
			JsonFormSelectedEventArgs args = new JsonFormSelectedEventArgs(jsonFile, null);

			args.OpenInNewWindow = openInNewWindow;
			JsonFormOpenSelected(this, args);
		}

		//-----------------------------------------------------------------------------
		private void EnableActions(TreeNode node)
		{
			List<ToolStripItem> allToolStrip = new List<ToolStripItem>() {
				buttonTM, buttonTG, buttonTD, buttonAddFromFileSystem, buttonOpenLocation,
				tsmiAdd, deafultTileManagerTsmi, deafultTileGroupTsmi, newTileTsmi, newToolBarTsmi,  newToolBarButtonTsmi,
				newHeaderStripTsmi, tileFromFileSystemTsmi, newHeaderStripButtonTsmi, newViewTsmi, newSeparatorTsmi,
				tsmiOpen, inHereTsmi, inNewWindowTsmi, fileSystemLocationTsmi };

			allToolStrip.ForEach(item => item.Visible = false);
			allToolStrip.ForEach(item => item.Enabled = true);
			allToolStrip.Clear();

			var docPropNode = (DocOutlineProperties)node?.Tag;
			AllowedTypes x = docPropNode != null ? docPropNode.Type : AllowedTypes.Generic;
			bool isHRef = docPropNode?.Href != null;

			switch (x)
			{
				case AllowedTypes.Frame:
					allToolStrip.AddRange(new List<ToolStripItem>() { newToolBarTsmi, newViewTsmi, tileFromFileSystemTsmi });
					break;
				case AllowedTypes.TileGroup:
				case AllowedTypes.LayoutContainer:
					allToolStrip.AddRange(new List<ToolStripItem>() { newTileTsmi, buttonTD, buttonAddFromFileSystem, tileFromFileSystemTsmi });
					break;
				case AllowedTypes.Tile:
				case AllowedTypes.Undefined:
				case AllowedTypes.Error:
					allToolStrip.AddRange(new List<ToolStripItem>() { tsmiOpen, inHereTsmi, inNewWindowTsmi, fileSystemLocationTsmi, buttonOpenLocation });
					break;
				case AllowedTypes.TileManager:
					allToolStrip.AddRange(new List<ToolStripItem>() { deafultTileGroupTsmi, buttonTG, tileFromFileSystemTsmi });
					break;
				case AllowedTypes.Toolbar:
					allToolStrip.AddRange(new List<ToolStripItem>() { newToolBarButtonTsmi, newSeparatorTsmi });
					break;
				case AllowedTypes.HeaderStrip:
					allToolStrip.Add(newHeaderStripButtonTsmi); break;
				case AllowedTypes.View:
				case AllowedTypes.Panel:
					allToolStrip.AddRange(new List<ToolStripItem>() { deafultTileManagerTsmi, deafultTileGroupTsmi, buttonTM, buttonTG, tileFromFileSystemTsmi });
					if (CanAddHeaderStrip())
						allToolStrip.Add(newHeaderStripTsmi);
					break;

				default: break; // case AllowedTypes.Tab:case AllowedTypes.Tabber: case AllowedTypes.TilePanel:TODOROBY
			}
			if (isHRef && !allToolStrip.Contains(tsmiOpen))
			{
				allToolStrip.AddRange(new List<ToolStripItem>() { tsmiOpen, inHereTsmi, inNewWindowTsmi, fileSystemLocationTsmi, buttonOpenLocation });
			}
			if (allToolStrip.Count > 0)
				allToolStrip.Add(tsmiAdd);
			allToolStrip.ForEach(item => item.Visible = true);
		}

		//-----------------------------------------------------------------------------
		private bool CanAddHeaderStrip()
		{
			//calcola se nella view c'è già un header strip, se c'è false, altrim vero.
			// TODOROBY
			return true;
		}

		//-----------------------------------------------------------------------------
		private string CalculateId(DocOutlineProperties item)
		{
			string jsonFileId = item.Id != null ? item.Id : item.Href;
			if (jsonFileId.StartsWith("M.") || jsonFileId.StartsWith("D."))
				return StaticFunctions.GetFileFromJsonFileId(jsonFileId);
			string jsonFile = Path.GetDirectoryName(FileLocation);
			return Path.Combine(jsonFile, jsonFileId + NameSolverStrings.TbjsonExtension);
		}

		//-----------------------------------------------------------------------------
		private int GetNumberImageFor(AllowedTypes type)
		{
			switch (type)
			{
				case AllowedTypes.Frame:
					return (int)ImageLists.DocOutlineTreeImageIndex.Frame;
				case AllowedTypes.View:
					return (int)ImageLists.DocOutlineTreeImageIndex.View;
				case AllowedTypes.Panel:
				case AllowedTypes.TilePanel:
					return (int)ImageLists.DocOutlineTreeImageIndex.Panel;
				case AllowedTypes.Tab:
				case AllowedTypes.TileGroup:
					return (int)ImageLists.DocOutlineTreeImageIndex.TileGroup;
				case AllowedTypes.Tabber:
				case AllowedTypes.TileManager:
					return (int)ImageLists.DocOutlineTreeImageIndex.TileManager;
				case AllowedTypes.LayoutContainer:
					return (int)ImageLists.DocOutlineTreeImageIndex.LayoutContainer;
				case AllowedTypes.HeaderStrip:
					return (int)ImageLists.DocOutlineTreeImageIndex.HeaderStrip;
				case AllowedTypes.Toolbar:
					return (int)ImageLists.DocOutlineTreeImageIndex.Toolbar;
				case AllowedTypes.ToolbarButton:
					return (int)ImageLists.DocOutlineTreeImageIndex.ToolbarButton;
				case AllowedTypes.Generic:
					return (int)ImageLists.DocOutlineTreeImageIndex.Generic;
				case AllowedTypes.Error:
					return (int)ImageLists.DocOutlineTreeImageIndex.Error;
				case AllowedTypes.Undefined:
				case AllowedTypes.Tile:
				default:
					return (int)ImageLists.DocOutlineTreeImageIndex.Tile;
			}
		}

		/// <remarks />
		//-----------------------------------------------------------------------------
		public string SaveDocOutline()
		{
			string file = Path.Combine(Path.GetDirectoryName(FileLocation), Path.GetFileNameWithoutExtension(FileLocation) + NameSolverStrings.TbjsonExtension);
			string newJson = GetJsonDeserialized();
			try
			{
				using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
				{
					sw.Write(newJson);
				}
				editor.SetDirty(false);
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show(String.Format(Resources.UnauthorizedAccessreadOnlyFile, file));
				newJson = Resources.UnauthorizedAccessreadOnlyFile;
			}

			return newJson;
		}

		//-----------------------------------------------------------------------------
		private void DeleteJObjectFromJsonDeserialized()
		{
			TreeNode selectedNode = treeDocOutlineManager?.SelectedNode;
			if (selectedNode == null || ((selectedNode.Tag as DocOutlineProperties) == null))
				return;

			((DocOutlineProperties)selectedNode.Tag).InnerJObject.Remove();
			UpdateCodeEditor();

			if (!editor.IsDirty)
				editor.SetDirty(true);
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_MouseDown(object sender, MouseEventArgs e)
		{
			treeDocOutlineManager.SelectedNode = treeDocOutlineManager.GetNodeAt(e.Location);
			if (treeDocOutlineManager.SelectedNode != null)
				EnableActions(treeDocOutlineManager.SelectedNode);

			if (treeDocOutlineManager.SelectedNode == null)
			{
				selectionService.SetSelectedComponents(null);
				SetNodeLikeSelected();
			}
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_AfterSelect(object sender, TreeViewEventArgs e)
		{
			selectionService.SetSelectedComponents(new Object[] { e.Node.Tag });
			SetNodeLikeSelected();                                  //forzare l'hightlighting del nodo nel tree
			editor.UpdateJsonCodeSelection(previousNode.Text);      //forzare l'hightlighting del nodo nel codeEditor
		}

		//-----------------------------------------------------------------------------
		private void SetNodeLikeSelected()
		{
			if (previousNode != null)
			{
				previousNode.BackColor = Color.Transparent;
				previousNode.ForeColor = Color.Black;
			}
			previousNode = treeDocOutlineManager.SelectedNode;
			if (previousNode != null)
			{
				previousNode.BackColor = SystemColors.Highlight;
				previousNode.ForeColor = SystemColors.HighlightText;
			}
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			DocOutlineProperties item = ((DocOutlineProperties)e.Node.Tag);
			if (item != null)
				SendToJsonFormOpen(item);
		}

		//-----------------------------------------------------------------------------
		private void tsmiOpenInNewWindow_Click(object sender, EventArgs e)
		{
			DocOutlineProperties selected = treeDocOutlineManager?.SelectedNode?.Tag as DocOutlineProperties;
			if (selected != null)
				SendToJsonFormOpen(selected, true);
		}

		//-----------------------------------------------------------------------------
		private void tsmiOpen_Click(object sender, EventArgs e)
		{
			DocOutlineProperties selected = treeDocOutlineManager?.SelectedNode?.Tag as DocOutlineProperties;
			if (selected != null)
				SendToJsonFormOpen(selected, false);
		}

		//-----------------------------------------------------------------------------
		private void tsmiOpenProperties_Click(object sender, EventArgs e)
		{
			OpenProperties?.Invoke(this, e);
		}

		//-----------------------------------------------------------------------------
		private void tsRefresh_Click(object sender, EventArgs e)
		{
			RefreshDocTree();
		}

		//-----------------------------------------------------------------------------
		private void tsTileDialog_Click(object sender, EventArgs e)
		{
			string newNodeName = editor.CallAddItem(Path.GetDirectoryName(FileLocation));
			CreateHrefForDocOutline(newNodeName);
		}

		//-----------------------------------------------------------------------------
		private void ToolStripClickToAddItem_Click(object sender, EventArgs e)
		{
			DocOutObj t = sender is ToolStripMenuItem ?
				(DocOutObj)((ToolStripMenuItem)sender).Tag :
				(DocOutObj)((ToolStripButton)sender).Tag;
			string newNodeName = CreateUniqueId(t.defaultname);
			JObject newItemToCreate = JsonProcessor.CreateNewDocOutItem(t, newNodeName);
			AddNodeDocTree(newNodeName, newItemToCreate, t.allowedType);
		}

		//-----------------------------------------------------------------------------
		private void tsHeaderStripButton_Click(object sender, EventArgs e)
		{
			// TODOROBY-MARCO creare oggetto HeaderStrip Button ?
		}

		//-----------------------------------------------------------------------------
		private string CreateUniqueId(string newNodeName)
		{
			if (!ids.Contains(newNodeName))
			{
				string controlName = newNodeName;
				int index = 0;
				while (true)
				{
					newNodeName = controlName + "_" + (index++).ToString();
					if (!ids.Contains(newNodeName))
						break;
				}
			}
			return newNodeName;
		}

		//-----------------------------------------------------------------------------
		private void tsbAddFromFileSystem_Click(object sender, EventArgs e)
		{
			TreeNode selected = treeDocOutlineManager?.SelectedNode;
			if (selected == null || ((selected.Tag as DocOutlineProperties) == null))
				return;
			DocOutlineProperties elemSelected = (DocOutlineProperties)selected.Tag;
			List<AllowedTypes> childrenAdmitted = GetChildrenAdmittedFor(elemSelected.Type);


			OpenFileDialog dialog = new OpenFileDialog() { InitialDirectory = Path.GetDirectoryName(FileLocation), Filter = "TbJson Files (.tbjson)|*.tbjson" };

			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			string newNodeName = Path.GetFileNameWithoutExtension(dialog.FileName);

			DocOutlineProperties docAdded = jsonPropParser.Parse(dialog.FileName);
			if (!childrenAdmitted.Contains(docAdded.Type))
			{			
				MessageBox.Show(string.Format(Resources.CannotAddThisItem, elemSelected.Type, docAdded.Type));
				return;
			}
			
			CreateHrefForDocOutline(newNodeName, docAdded.Type);
			RefreshDocTree();
		}

		//-----------------------------------------------------------------------------
		private void CreateHrefForDocOutline(string newNodeName, AllowedTypes docAddedType = AllowedTypes.Undefined)
		{
			if (string.IsNullOrEmpty(newNodeName)) //ha premuto Cancel 
				return;
			JObject newItemToCreate = JsonProcessor.CreateNewHref(newNodeName);
			newNodeName = hrefPrefix + newNodeName;
			docAddedType = docAddedType == AllowedTypes.Undefined ? AllowedTypes.Tile : docAddedType;
			AddNodeDocTree(newNodeName, newItemToCreate, docAddedType);
		}

		//-----------------------------------------------------------------------------
		private List<AllowedTypes> GetChildrenAdmittedFor(AllowedTypes elemtype)
		{
			switch (elemtype)
			{
				case AllowedTypes.Frame:
					return new List<AllowedTypes>() { AllowedTypes.Toolbar, AllowedTypes.View };
				case AllowedTypes.TileGroup:
				case AllowedTypes.LayoutContainer:
					return new List<AllowedTypes>() { AllowedTypes.Tile };
				case AllowedTypes.TileManager:
					return new List<AllowedTypes>() { AllowedTypes.TileGroup };
				case AllowedTypes.Toolbar:
					return new List<AllowedTypes>() { AllowedTypes.ToolbarButton };
				case AllowedTypes.View:
				case AllowedTypes.Panel:
					return new List<AllowedTypes>() { AllowedTypes.TileManager, AllowedTypes.TileGroup, AllowedTypes.HeaderStrip };
				default: return null;
			}
		}

		//-----------------------------------------------------------------------------
		private void tsmiOpenLocation_Click(object sender, EventArgs e)
		{
			TreeNode selected = treeDocOutlineManager?.SelectedNode;
			if (selected == null || ((selected.Tag as DocOutlineProperties) == null))
				return;
			string f = FileLocation;
			if (f.EndsWith(NameSolverStrings.TbjsonExtension))
				f = Path.GetDirectoryName(f);
			if (!Directory.Exists(f))
				return;
			Process.Start(f);
		}

		//-----------------------------------------------------------------------------
		private void tsmiDelete_Click(object sender, EventArgs e)
		{
			DeleteJObjectFromJsonDeserialized();
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_DragOver(object sender, DragEventArgs e)
		{
			Point p = treeDocOutlineManager.PointToClient(new Point(e.X, e.Y));
			TreeNode currentItem = treeDocOutlineManager?.GetNodeAt(p.X, p.Y) as TreeNode;
			if (currentItem == null)
				return;
			e.Effect = DragDropEffects.Move;
			return;
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Effect == DragDropEffects.None)
				return;

			Point p = treeDocOutlineManager.PointToClient(new Point(e.X, e.Y));
			TreeNode targetNode = treeDocOutlineManager?.GetNodeAt(p.X, p.Y) as TreeNode;
			if (targetNode == null)
				return;
			DataModelDropObject ddo = e.Data.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (!(ddo?.Component is DocOutlineProperties))
				return;
			RefreshDocTree();
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = true;
			Rectangle r = e.Node.Bounds;
			e.Graphics.DrawImage(Resources.Custom, new Point(r.Left - 26, r.Top - 3));
		}

		//-----------------------------------------------------------------------------
		private void treeDocOutlineManager_ItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode treenode = (e?.Item as TreeNode);
			IComponent component = treenode?.Tag as IComponent;
			if (component == null || component.Site == null)
				return;

			DoDragDrop(new DataModelDropObject(
				component,
				treeDocOutlineManager.SelectedNode.Text
				), DragDropEffects.Copy | DragDropEffects.Move);
		}
	}

}
