using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.View;
using System.Text.RegularExpressions;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;

namespace Microarea.EasyBuilder.UI
{

	//=========================================================================
	/// <remarks/>
	internal partial class ViewOutlineTreeControl : UserControl
	{

		//================================================================================
		class ViewNodesComparer : IComparer<IComponent>
		{
			public int Compare(IComponent x, IComponent y)
			{
				MBodyEditColumn bec1 = x as MBodyEditColumn;
				MBodyEditColumn bec2 = y as MBodyEditColumn;
				return (bec1 == null || bec2 == null) ?
						0 : (bec1.ColPos).CompareTo(bec2.ColPos);
			}
		}

		public enum ShowMode { Labels, Variables, ResourcesNames, Layout };

		/// <remarks/>
		public event EventHandler OpenProperties;
		/// <remarks/>
		public event EventHandler<DeleteObjectEventArgs> DeleteObject;
		/// <remarks/>
		public event EventHandler PromoteControl;

		private const string FilterOnLabel = "Filter on";
		private const string FilterOffLabel = "Filter off";

		object[] dummyArgs = { };

		ISelectionService selectionService;
		private FormEditor editor;
		internal ViewOutlineTreeNode viewNode;

		ShowMode showWith = ShowMode.Labels;
		IWindowWrapperContainer filteredComponent = null;
		internal bool isFilteredTree = false;
		private TreeNode selectedFocusNode;
		private TreeNode previousNode = null;
		private bool populatingTree = false;

		//--------------------------------------------------------------------------------
		public IWindowWrapperContainer FilteredComponent
		{
			get { return filteredComponent; }
			set { filteredComponent = value; }
		}

		//--------------------------------------------------------------------------------
		internal ToolStripLabel TsFilterLabel
		{
			get { return tsFilterLabel; }
			set { tsFilterLabel = value; }
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public ViewOutlineTreeControl(FormEditor editor)
		{
			InitializeComponent();
			if (editor.RunAsEasyStudioDesigner)
			{
				tsComboShowWith.Enabled = false;
				showWith = ShowMode.ResourcesNames;
			}

			tsComboShowWith.SelectedItem = showWith.ToString();

			this.Text = Resources.ViewOutlineTitle;
			this.editor = editor;
			treeViewOutlineManagers.ImageList = ImageLists.ToolBoxTree;
			selectionService = editor.GetService(typeof(ISelectionService)) as ISelectionService;

			treeViewOutlineManagers.LostFocus += (sender, args) => selectedFocusNode = treeViewOutlineManagers.SelectedNode;
			treeViewOutlineManagers.GotFocus += (sender, args) => treeViewOutlineManagers.SelectedNode = selectedFocusNode;

			if (selectionService != null)
				selectionService.SelectionChanged += new EventHandler(SelectionService_SelectionChanged);

			TreeViewExtender extender = new TreeViewExtender(treeViewOutlineManagers);

			PopulateTree(editor.Controller);
		}

		//-----------------------------------------------------------------------------
		void SelectionService_SelectionChanged(object sender, EventArgs e)
		{
			if (selectionService == null)
				return;

			if (selectionService.SelectionCount == 1)
				SelectOrFilter(selectionService.PrimarySelection);
			else
				SelectObject(null);
		}

		/// <summary>
		/// Popola il tree a partire dall'object model in memoria
		/// </summary>
		//-----------------------------------------------------------------------------
		public void PopulateTree(DocumentController documentController)
		{
			try
			{
				populatingTree = true;
				treeViewOutlineManagers.BeginUpdate();

				if (documentController == null)
				{
					treeViewOutlineManagers.Nodes.Add(Resources.InitSources);
					populatingTree = false;
					treeViewOutlineManagers.EndUpdate();
					return;
				}

				treeViewOutlineManagers.Nodes.Clear();

				// controllerNode = new ViewOutlineTreeNode(Resources.Controller, documentController, ImageLists.ViewOutlineImageIndex.MVCController);
				if (showWith == ShowMode.Layout)
				{
					ILayoutComponent layoutRoot = documentController.View.LayoutObject;
					if (layoutRoot == null)
						return;
					viewNode = new ViewOutlineTreeNode(layoutRoot.LayoutObject.LayoutDescription, layoutRoot.LayoutObject, ImageLists.ViewOutlineImageIndex.MVCView);
					viewNode.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Bold);
					treeViewOutlineManagers.Nodes.Add(viewNode);
					AddLayoutTree(viewNode.Nodes, layoutRoot as IEasyBuilderContainer);
				}
				else
				{
					viewNode = new ViewOutlineTreeNode(Resources.View, documentController.View, ImageLists.ViewOutlineImageIndex.MVCView);
					treeViewOutlineManagers.Nodes.Add(viewNode.ToTreeNode(showWith));
					AddViewComponents(viewNode, documentController.View);
				}


				treeViewOutlineManagers.SelectedNode = viewNode;
				viewNode.ExpandAll();
				treeViewOutlineManagers.SelectedNode.EnsureVisible();

				if (FilteredComponent != null && viewNode.Tag != FilteredComponent)
				{
					ViewOutlineTreeNode filteredNode = null;
					if (showWith != ShowMode.Layout)
						treeViewOutlineManagers.Nodes.Clear();

					filteredNode = FindNode(viewNode.Nodes, FilteredComponent);
					if (filteredNode != null)
						treeViewOutlineManagers.Nodes.Add(filteredNode);
				}

			}
			finally
			{
				if(editor.RunAsEasyStudioDesigner)
					ExtractSubTree();
				treeViewOutlineManagers.EndUpdate();
			}
			populatingTree = false;
		}

		//-----------------------------------------------------------------------------
		private void ExtractSubTree()
		{
			if (treeViewOutlineManagers.Nodes.Count != 1) return;
			TreeNode temp = treeViewOutlineManagers.Nodes[0];

			var g = temp?.Nodes;
			if (g == null || g.Count != 1) return;
			var g1 = g[0];
			treeViewOutlineManagers.Nodes.Clear();
			treeViewOutlineManagers.Nodes.Add(g1);
		}

		//-----------------------------------------------------------------------------
		private void AddLayoutTree(TreeNodeCollection parentNodes, IEasyBuilderContainer container)
		{
			List<IComponent> components = new List<IComponent>();

			foreach (IComponent cmp in container.Components)
			{
				ILayoutComponent layoutCmp = cmp as ILayoutComponent;
				if (layoutCmp == null || layoutCmp.LayoutObject == null)
					continue;

				EasyBuilderComponent ebComponent = layoutCmp.LayoutObject as EasyBuilderComponent;

				if (ebComponent == null || !ebComponent.CanShowInObjectModel)
					continue;
				ViewOutlineTreeNode node = new ViewOutlineTreeNode(layoutCmp.LayoutDescription, ebComponent);
				parentNodes.Add(node);

				IEasyBuilderContainer cnt = cmp as IEasyBuilderContainer;

				if (layoutCmp.LayoutObject is MLayoutContainer)
					node.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Bold);
				else if (cnt != null)
					node.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Italic);

				if (cnt != null)
					AddLayoutTree(node.Nodes, cnt);
			}
		}

		//-----------------------------------------------------------------------------
		private void AddViewComponents(ViewOutlineTreeNode parent, IContainer container)
		{
			TreeNodeCollection parentNodes = (TreeNodeCollection)parent.Nodes;
			List<IComponent> components = new List<IComponent>();

			foreach (IComponent cmp in container.Components)
			{
				EasyBuilderComponent ebComponent = cmp as EasyBuilderComponent;
				if (ebComponent == null)
					continue;
				components.Add(ebComponent);
			}

			if (container is MBodyEdit) //allora i suoi components sono BEColumn, li sorto per ColPos
				components.Sort(new ViewNodesComparer());

			ViewOutlineTreeNode staticArea = null, staticArea2 = null;
			EasyBuilderComponent StaticAreaComponent = null, StaticAreaComponent2 = null;

			if (container is MTileDialog || container is MEasyStudioPanel) // guardo se tra i componetns ci sono delle Static Area
				CheckForStaticAreas(parentNodes, components, out staticArea, out staticArea2, out StaticAreaComponent, out StaticAreaComponent2);
			//se venisse trovata una static, viene aggiunta alla lista dei parentNodes e tolta da quelli dei components

			foreach (EasyBuilderComponent ebComponent in components)
			{
				if (!ebComponent.CanShowInObjectModel)
					continue;

				string name = ebComponent.Name;

				if (ebComponent.DesignModeType == EDesignMode.Static && ebComponent is BaseWindowWrapper)
				{   //se sono nel json designer devo far vedere l'idc
					string id = ((BaseWindowWrapper)ebComponent).Id;
					if (!string.IsNullOrEmpty(id))
						name = id;
				}

				// se il name del componente è vuoto vuol dire che nel C++ il componente non è più presente (cambio di configurazione)
				if (string.IsNullOrEmpty(name))
					continue;

				ViewOutlineTreeNode node = new ViewOutlineTreeNode(name, ebComponent);
				if (ebComponent is MToolbarItem)
					node.ImageIndex = node.SelectedImageIndex = (int)ImageLists.ToolBoxIndex.ToolbarButton;
				else if (ebComponent is MToolbar)
					node.ImageIndex = node.SelectedImageIndex = (int)ImageLists.ToolBoxIndex.Toolbar;


				if (staticArea != null && NodeLocationInStaticArea(ebComponent, StaticAreaComponent))
				{
					node.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Italic);
					staticArea.Nodes.Add(node.ToTreeNode(showWith));
				}
				else if (staticArea2 != null && NodeLocationInStaticArea(ebComponent, StaticAreaComponent2))
				{
					node.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Italic);
					staticArea2.Nodes.Add(node.ToTreeNode(showWith));
				}
				else
				{
					if (node.Tag is MTileDialog)
						node.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Bold);
					parentNodes.Add(node.ToTreeNode(showWith));
				}

				IContainer cnt = ebComponent as IContainer;
				if (cnt != null)
					AddViewComponents(node, cnt);
			}

		}

		//-----------------------------------------------------------------------------
		private void CheckForStaticAreas(TreeNodeCollection parentNodes, List<IComponent> components, out ViewOutlineTreeNode staticArea, out ViewOutlineTreeNode staticArea2, out EasyBuilderComponent StaticAreaComponent, out EasyBuilderComponent StaticAreaComponent2)
		{
			staticArea = null;
			staticArea2 = null;
			StaticAreaComponent = null;
			StaticAreaComponent2 = null;

			foreach (EasyBuilderComponent ebComponent in components)
			{
				if (ebComponent is GenericGroupBox)
				{
					if (((GenericGroupBox)ebComponent).Id == MParsedControl.staticAreaID)
					{
						string name = ((GenericGroupBox)ebComponent).Name;
						staticArea = new ViewOutlineTreeNode(name, ebComponent);
						StaticAreaComponent = ebComponent;
					}
					if (((GenericGroupBox)ebComponent).Id == MParsedControl.staticArea2ID)
					{
						string name = ((GenericGroupBox)ebComponent).Name;
						staticArea2 = new ViewOutlineTreeNode(name, ebComponent);
						StaticAreaComponent2 = ebComponent;
					}
				}
			}

			if (staticArea != null && StaticAreaComponent != null)
			{
				staticArea.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Italic);
				parentNodes.Add(staticArea.ToTreeNode(showWith));
				components.Remove(StaticAreaComponent);
			}
			if (staticArea2 != null && StaticAreaComponent2 != null)
			{
				staticArea2.NodeFont = new Font(treeViewOutlineManagers.Font, FontStyle.Italic);
				parentNodes.Add(staticArea2.ToTreeNode(showWith));
				components.Remove(StaticAreaComponent2);
			}
		}

		//-----------------------------------------------------------------------------
		private bool NodeLocationInStaticArea(EasyBuilderComponent ebComponent, EasyBuilderComponent StaticAreaComponent)
		{
			BaseWindowWrapper bsWrapper = ebComponent as BaseWindowWrapper;
			GenericGroupBox staticArea = StaticAreaComponent as GenericGroupBox;
			if (bsWrapper == null || staticArea == null)
				return false;
			Point minPoint = new Point(staticArea.Location.X, staticArea.Location.Y);
			Point maxPoint = new Point(staticArea.Location.X + staticArea.Size.Width, staticArea.Location.Y + staticArea.Size.Height);

			//se il punto in alto a sx del component è all'interno della static area, senza bordo a destra per dare la possibilità di appoggiarcisi
			if (bsWrapper.Location.X >= minPoint.X && bsWrapper.Location.X < maxPoint.X)
				return (bsWrapper.Location.Y >= minPoint.Y && bsWrapper.Location.Y <= maxPoint.Y);
			return false;

		}

		//-----------------------------------------------------------------------------
		internal void UpdateViewOutline(IComponent component)
		{
			if (component == null)
				return;

			if (component is DocumentController) // refresh all
			{
				PopulateTree(component as DocumentController);
				treeViewOutlineManagers.SelectedNode = viewNode;
				return;
			}

			if (showWith == ShowMode.Layout)
			{
				PopulateTree(editor.Controller);
				treeViewOutlineManagers.SelectedNode = viewNode;
				return;
			}

			if (component.Site == null || component.Site.Container == null)
				return;

			IContainer container = component.Site.Container;
			ViewOutlineTreeNode containerNode = null;

			EasyBuilderComponent ebComponent = component as EasyBuilderComponent;

			// fa parte del view model
			if (component is IWindowWrapper || container is IWindowWrapper)
			{
				// default
				container = container is IWindowWrapperContainer ? container : (component is IWindowWrapperContainer ? component as IWindowWrapperContainer : container);

				containerNode = FindNode(treeViewOutlineManagers.Nodes, (object)container);
				if (containerNode != null)
				{
					containerNode.Nodes.Clear();
					AddViewComponents(containerNode, container);
				}
			}
		}

		//-----------------------------------------------------------------------------
		private ViewOutlineTreeNode FindNode(TreeNodeCollection nodes, object o, bool searchAlsoById = false)
		{
			foreach (TreeNode node in nodes)
			{
				ViewOutlineTreeNode n = node as ViewOutlineTreeNode;

				if (n != null && n.Tag == o)
					return n;
				if (searchAlsoById && !(n?.Tag is DocumentView) && o is EasyBuilderControl)
				{
					if (n != null && ((EasyBuilderControl)n.Tag).Id == ((EasyBuilderControl)o).Id)
						return n;
				}

				ViewOutlineTreeNode found = FindNode(node.Nodes, o, searchAlsoById);
				if (found != null)
					return found;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		internal void SelectObject(object o, bool searchAlsoById = false)
		{
			if (o == null)
			{
				treeViewOutlineManagers.SelectedNode = null;
				if (selectionService.SelectionCount > 1) //annullo selezione nel caso di multiselezione
					ResetHighlightNodeColors();
				return;
			}

			ViewOutlineTreeNode found = FindNode(treeViewOutlineManagers.Nodes, o, searchAlsoById);
			if (found == null)
			{
				IComponent component = o as IComponent;
				if (component != null && component.Site != null)
					SelectObject(component.Site.Container);
				return;
			}

			ResetHighlightNodeColors();
			if (found != null && found.Tag != this.treeViewOutlineManagers.SelectedNode?.Tag)
				SetHighlightNodeColors(found);

		}

		//-----------------------------------------------------------------------------
		private void ResetHighlightNodeColors()
		{
			if (previousNode == null)
				return;
			previousNode.BackColor = Color.White;
			previousNode.ForeColor = Color.Black;
		}

		//-----------------------------------------------------------------------------
		private void SetHighlightNodeColors(ViewOutlineTreeNode found)
		{
			treeViewOutlineManagers.SelectedNode = null;
		//	found.BackColor = SystemColors.Highlight;
			found.ForeColor = Color.Blue;
			previousNode = found;
			found.ExpandAll();
			found.EnsureVisible();
		}

		//-----------------------------------------------------------------------------
		internal void SelectOrFilter(object o)
		{
			if (!isFilteredTree)
			{
				SelectObject(o);
				return;
			}

			IWindowWrapperContainer container = o as IWindowWrapperContainer;
			if (container == null)
			{
				BaseWindowWrapper control = o as BaseWindowWrapper;
				if (control != null)
				{
					container = control.ParentComponent as IWindowWrapperContainer;
					if (container != null && container != FilteredComponent)
					{
						FilteredComponent = container;
						PopulateTree(editor.Controller);
					}
					SelectObject(control);
					return;
				}
			}

			if (container == FilteredComponent)
				return;
			FilteredComponent = container;
			PopulateTree(editor.Controller);
			SelectObject(container);
			return;
		}

		//-----------------------------------------------------------------------------
		private void TsPromote_Click(object sender, EventArgs e)
		{
			if (PromoteControl != null)
				PromoteControl(this, e);
		}

		//-----------------------------------------------------------------------------
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UpdateViewOutline(editor?.Controller);
		}

		//-----------------------------------------------------------------------------
		private void TsProperties_Click(object sender, EventArgs e)
		{
			if (OpenProperties != null)
				OpenProperties(this, e);
		}

		//-----------------------------------------------------------------------------
		private void tsDelete_Click(object sender, EventArgs e)
		{
			if (this.treeViewOutlineManagers.SelectedNode != null)
			{
				if (DeleteObject != null)
					DeleteObject(this, new DeleteObjectEventArgs(this.treeViewOutlineManagers.SelectedNode.Tag as EasyBuilderComponent));
			}
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_DragOver(object sender, DragEventArgs e)
		{
			Point p = treeViewOutlineManagers.PointToClient(new Point(e.X, e.Y));
			ViewOutlineTreeNode currentItem = treeViewOutlineManagers.GetNodeAt(p.X, p.Y) as ViewOutlineTreeNode;
			if (currentItem == null)
				return;

			//Non è consentito drag-drop-are un enumerativo sul ViewOutline tree.
			EnumsDropObject enumsDropObject = e.Data.GetData(typeof(EnumsDropObject)) as EnumsDropObject;
			if (enumsDropObject != null)
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			DataModelDropObject ddo = e.Data.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (ddo != null)//sto spostando un oggetto all'interno del ViewOutline tree.
			{
				if (currentItem.Tag is MTileDialog)
				{
					MTileDialog tile = ddo.Component as MTileDialog;
					if (tile != null && !tile.HasCodeBehind)
						e.Effect = DragDropEffects.Move;
					return;
				}
				//sto spostando un controllo grafico per modificare il taborder
				if (currentItem.Tag is EasyBuilderControl &&
					ddo.Component is EasyBuilderControl &&
					currentItem.Tag != ddo.Component &&
					//	((EasyBuilderControl)currentItem.Tag).TabStop && ((EasyBuilderControl)ddo.Component).TabStop &&
					// controllo non più necessario, perchè è stata implementata la possibilità di spostare tutti gli oggetti nel VIewOutlineTree, compresi quelli con TabStop = false.
					((EasyBuilderControl)currentItem.Tag).Parent == ((EasyBuilderControl)ddo.Component).Parent)
					e.Effect = DragDropEffects.Move;
				else
					e.Effect = DragDropEffects.None;
				return;
			}

			string sqlRecordName = e.Data.GetData(typeof(string)) as string;
			if (sqlRecordName == null)
			{
				//Hotlink, posso dropparli solo se c'è un Master
				AddHotLinkEventArgs hotLinkEventArgs = e.Data.GetData(typeof(AddHotLinkEventArgs)) as AddHotLinkEventArgs;
				if (hotLinkEventArgs != null)
				{
					e.Effect = currentItem.Tag.Equals(ControllerSources.HotLinksPropertyName)
						? DragDropEffects.Copy
						: DragDropEffects.None;
					return;
				}

				ComponentDeclarationRequest declarationRequest = e.Data.GetData(typeof(ComponentDeclarationRequest)) as ComponentDeclarationRequest;
				if (declarationRequest != null)
				{
					e.Effect = currentItem.Tag.Equals(ControllerSources.BusinessObjectsPropertyName)
						? DragDropEffects.Copy
						: DragDropEffects.None;
					return;
				}
			}
			//drop di tabella su DBT
			if (currentItem.Tag.Equals(ControllerSources.DBTPropertyName) || currentItem.Tag is MDBTMaster)
			{
				e.Effect = editor.Document.Batch ? DragDropEffects.None : DragDropEffects.Copy;
				return;
			}

			//drop di tabella su DBT slave buffered 1-n-n
			if (currentItem.Tag is MDBTSlaveBuffered)
			{
				e.Effect = editor.Document.Batch ? DragDropEffects.None : DragDropEffects.Copy;
				return;
			}
			//drop di tabella su DataManager o HotLink
			e.Effect = currentItem.Tag.Equals(ControllerSources.HotLinksPropertyName) ||
						currentItem.Tag.Equals(ControllerSources.DataManagersPropertyName)
				? DragDropEffects.Copy
				: DragDropEffects.None;
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Effect == DragDropEffects.None)
				return;

			Point p = treeViewOutlineManagers.PointToClient(new Point(e.X, e.Y));
			ViewOutlineTreeNode targetNode = treeViewOutlineManagers.GetNodeAt(p.X, p.Y) as ViewOutlineTreeNode;
			if (targetNode == null)
				return;

			DataModelDropObject ddo = e.Data.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (!(ddo?.Component is EasyBuilderControl))
				return;

			EasyBuilderControl control = ddo.Component as EasyBuilderControl;
			int newIndex = ((EasyBuilderControl)targetNode.Tag).TabOrder;
			int oldIndex = control.TabOrder;
			control.TabOrder = newIndex;
			TBSite site = control.Site as TBSite;
			if (site == null)
				return;
			FormEditor formEditor = site.Editor as FormEditor;
			if (!(ddo.Component is MBodyEditColumn))
				formEditor?.OnComponentPropertyChanged((control), ReflectionUtils.GetPropertyName(() => (control).TabOrder), oldIndex, newIndex);
			if (editor.RunAsEasyStudioDesigner)
				formEditor?.RefreshWrappers(true);

			if (!editor.RunAsEasyStudioDesigner)
				PopulateTree(editor.Controller);
		}

		//-----------------------------------------------------------------------------
		private void EnableActions(TreeNode node)
		{
			EasyBuilderComponent ebComponent = node == null ? null : node.Tag as EasyBuilderComponent;

			deleteToolStripMenuItem.Enabled = tsButtonDelete.Enabled = (
					(
						(ebComponent as DocumentController) == null &&
						(ebComponent as MDocument) == null &&
						(ebComponent as MView) == null &&
						ebComponent != null &&
						ebComponent.CanBeDeleted
					)
				);

			tsComboShowWith.Visible = tsFilterLabel.Visible = true;
			tsFilterLabel.Enabled = (showWith != ShowMode.Layout) && !editor.ImEditingOnlyThisTile;

			//Save as ...
			SaveAsToolStripMenuItem.Visible = !editor.RunAsEasyStudioDesigner;
			exportToJsonMenuItem.Enabled = IsExportToJsonEnabled();
			exportToTemplateMenuItem.Enabled = IsSaveAsTemplateEnabled();

			highlightContainerToolStripMenuItem.Enabled =
				highlightContainerToolStripMenuItem.Visible = IsAContainer(ebComponent) && !editor.ImEditingOnlyThisTile;

			promoteToolStripMenuItem.Visible = editor.RunAsEasyStudioDesigner;
			promoteToolStripMenuItem.Enabled = ebComponent is GenericWindowWrapper;
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = true;
			EasyBuilderComponent cmp = e.Node.Tag as EasyBuilderComponent;
			if (cmp != null && !cmp.HasCodeBehind)
			{
				Rectangle r = e.Node.Bounds;
				e.Graphics.DrawImage(Resources.Custom, new Point(r.Left - 26, r.Top - 3));
			}
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (populatingTree)
				return;

			EnableActions(e.Node);

			if (editor.SelectionService.PrimarySelection != e.Node.Tag)
				selectionService.SetSelectedComponents(new Object[] { e.Node.Tag });

		}

		//-----------------------------------------------------------------------------
		private void OnEventTsShowWith_SelectedIndexChanged(object sender, EventArgs e)
		{
			string label = ((ToolStripComboBox)sender).SelectedItem.ToString();
			if (ShowMode.Labels.ToString() == label)
				showWith = ShowMode.Labels;
			else if (ShowMode.Layout.ToString() == label)
				showWith = ShowMode.Layout;
			else if (ShowMode.ResourcesNames.ToString() == label)
				showWith = ShowMode.ResourcesNames;
			else if (ShowMode.Variables.ToString() == label)
				showWith = ShowMode.Variables;

			if (editor != null)
			{
				PopulateTree(editor.Controller);
				if (treeViewOutlineManagers.SelectedNode != null)
					EnableActions(treeViewOutlineManagers.SelectedNode);
			}
		}

		//-----------------------------------------------------------------------------
		private void tsFilterLabel_Click(object sender, EventArgs e)
		{
			TreeNode found = FindNode(viewNode.Nodes, editor.SelectionService.PrimarySelection, true);
			if (tsFilterLabel.Text == FilterOnLabel)
				ResetFIlter();
			else
				SetFilter();
		}

		//-----------------------------------------------------------------------------
		internal void SetFilter()
		{
			isFilteredTree = true;
			tsFilterLabel.Text = FilterOnLabel;
			tsFilterLabel.ForeColor = Color.Salmon;
		}

		//-----------------------------------------------------------------------------
		internal void ResetFIlter()
		{
			isFilteredTree = false;
			FilteredComponent = null;
			tsFilterLabel.Text = FilterOffLabel;
			tsFilterLabel.ForeColor = SystemColors.Desktop;
			PopulateTree(editor.Controller);
		}

		//-----------------------------------------------------------------------------
		private bool IsExportToJsonEnabled()
		{
			if (showWith != ShowMode.Labels && showWith != ShowMode.Variables)
				return false;

			TreeNode node = treeViewOutlineManagers.SelectedNode;
			if (node == null || node.Tag == null)
				return false;

			return node.Tag is MTileDialog;
		}
		//-----------------------------------------------------------------------------
		private void ExportToJsonMenuItem_Click(object sender, EventArgs e)
		{
			if (!IsExportToJsonEnabled())
				return;

			TreeNode node = treeViewOutlineManagers.SelectedNode;
			MTileDialog dialog = node?.Tag as MTileDialog;
			SerializationAddOnService ser = (SerializationAddOnService)dialog?.Site.GetService(typeof(SerializationAddOnService));
			string fileName = ser?.GenerateJsonFor(dialog, true);
			if (!string.IsNullOrEmpty(fileName))
				MessageBox.Show(this, string.Format(Resources.ExportToJsonOk, fileName), Resources.ExportToJsonCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			else
				MessageBox.Show(this, Resources.ExportToJsonError, Resources.ExportToJsonCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		//-----------------------------------------------------------------------------
		private bool IsSaveAsTemplateEnabled()
		{
			if (showWith != ShowMode.Labels && showWith != ShowMode.Variables)
				return false;

			TreeNode node = treeViewOutlineManagers.SelectedNode;
			return node != null && node.Tag != null && node.Tag is WindowWrapperContainer && !(node.Tag is MView);
		}

		//-----------------------------------------------------------------------------
		private void ExportToTemplateMenuItem_Click(object sender, EventArgs e)
		{
			if (IsSaveAsTemplateEnabled())
				editor?.SaveAsTemplate(treeViewOutlineManagers.SelectedNode.Tag as WindowWrapperContainer);
		}

		//-----------------------------------------------------------------------------
		private bool IsAContainer(EasyBuilderComponent ebComponent)
		{
			MLayoutContainer layoutContainer = ebComponent as MLayoutContainer;
			if (layoutContainer == null || showWith != ShowMode.Layout)
				return false;
			return layoutContainer.LinkedComponent == null || (layoutContainer.LayoutObject is MLayoutContainer);
		}

		//-----------------------------------------------------------------------------
		private void highlightContainerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			editor?.HighlightContainer(treeViewOutlineManagers.SelectedNode);
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_ItemDrag(object sender, ItemDragEventArgs e)
		{
			ViewOutlineTreeNode treenode = (e?.Item as ViewOutlineTreeNode);
			IComponent component = treenode?.Tag as IComponent;
			if (component == null || component.Site == null)
				return;

			DoDragDrop(new DataModelDropObject(
				component,
				treeViewOutlineManagers.SelectedNode.Text
				), DragDropEffects.Copy | DragDropEffects.Move);
		}

		//-----------------------------------------------------------------------------
		private void treeViewOutlineManagers_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyValue)
			{
				case 46://Cancel
					{
						tsDelete_Click(this, EventArgs.Empty);
						return;
					}
				default:
					return;
			}
		}

		//-----------------------------------------------------------------------------
		private void TreeViewOutlineManagers_MouseDown(object sender, MouseEventArgs e)
		{
			treeViewOutlineManagers.SelectedNode = treeViewOutlineManagers.GetNodeAt(e.Location);
			if (treeViewOutlineManagers.SelectedNode != null)
				EnableActions(treeViewOutlineManagers.SelectedNode);

			if (treeViewOutlineManagers.SelectedNode == null)
				selectionService.SetSelectedComponents(null);
		}

		//=========================================================================
		internal class ViewOutlineTreeNode : TreeNode
		{
			//-----------------------------------------------------------------------------
			public ViewOutlineTreeNode(string text, object tag, ImageLists.ViewOutlineImageIndex nodeType)
				: base(text, (int)nodeType, (int)nodeType)
			{
				this.Tag = tag;
				this.Name = text;
			}

			//-----------------------------------------------------------------------------
			public ViewOutlineTreeNode(string text, object tag, ImageLists.ToolBoxIndex nodeType)
				: base(text, (int)nodeType, (int)nodeType)
			{
				this.Tag = tag;
				this.Name = text;
			}

			//-----------------------------------------------------------------------------
			public ViewOutlineTreeNode(string text, EasyBuilderComponent component)
				: base(text)
			{
				this.Name = text;
				this.Tag = component;
				this.ImageIndex = this.SelectedImageIndex = GetToolBoxImageIndex(component);
			}

			//-----------------------------------------------------------------------------
			private int GetToolBoxImageIndex(IComponent cmp)
			{
				EasyBuilderComponent comp = (cmp as EasyBuilderComponent);
				if (comp != null && !comp.IsValidComponent)		return (int)ImageLists.ToolBoxIndex.Control;

				if (cmp is MTileDialog)					return (int)ImageLists.ToolBoxIndex.Tile;
				if (cmp is MFrame)						return (int)ImageLists.ToolBoxIndex.Frame;
				if (cmp is MBodyEdit)					return (int)ImageLists.ToolBoxIndex.BodyEdit;
				if (cmp is GenericListCtrl)				return (int)ImageLists.ToolBoxIndex.ListCtrl;
				if (cmp is MHeaderStrip)				return (int)ImageLists.ToolBoxIndex.HeaderStrip;
				if (cmp is MLabel)						return (int)ImageLists.ToolBoxIndex.Label;
				if (cmp is MParsedStatic)				return (int)ImageLists.ToolBoxIndex.ParsedStatic;
				if (cmp is MPropertyGrid)				return (int)ImageLists.ToolBoxIndex.PropertyGrid;
				if (cmp is MBodyEditColumn)				return (int)ImageLists.ToolBoxIndex.BodyEditColumn;
				if (cmp is MToolbar)					return (int)ImageLists.ToolBoxIndex.Toolbar;
				if (cmp is MToolbarItem)				return (int)ImageLists.ToolBoxIndex.ToolbarButton;

				if (cmp is DocumentController || cmp is DocumentView)
					return (int)ImageLists.ToolBoxIndex.MVCController;
				if (cmp is MTilePanel || cmp is MPanel || cmp is MEasyStudioPanel)
					return (int)ImageLists.ToolBoxIndex.Panel;
				if (cmp is GenericCheckBox || cmp is MCheckBox)
					return (int)ImageLists.ToolBoxIndex.CheckBox;
				if (cmp is GenericComboBox || cmp is MParsedCombo)
					return (int)ImageLists.ToolBoxIndex.ComboBox;
				if (cmp is GenericEdit || cmp is MParsedEdit)
					return (int)ImageLists.ToolBoxIndex.Edit;
				if (cmp is GenericGroupBox || cmp is MParsedGroupBox)
					return (int)ImageLists.ToolBoxIndex.GroupBox;
				if (cmp is GenericListBox || cmp is MParsedListBox)
					return (int)ImageLists.ToolBoxIndex.ListBox;
				if (cmp is GenericPushButton || cmp is MPushButton)
					return (int)ImageLists.ToolBoxIndex.PushButton;
				if (cmp is GenericRadioButton || cmp is MRadioButton)
					return (int)ImageLists.ToolBoxIndex.RadioButton;
				if (cmp is GenericTreeView || cmp is MTreeView)
					return (int)ImageLists.ToolBoxIndex.TreeView;
				if (cmp is MTab ||cmp is MTileGroup)
					return (int)ImageLists.ToolBoxIndex.Tab;
				if (cmp is MTabber || cmp is MTileManager)
					return (int)ImageLists.ToolBoxIndex.Tabber;
				
				return (int)ImageLists.ToolBoxIndex.Control;
			}

			//-----------------------------------------------------------------------------
			public static explicit operator TreeNodeCollection(ViewOutlineTreeNode v)
			{
				throw new NotImplementedException();
			}
		}
	}

    internal static class TreeNodeExtensions
    {
        //-----------------------------------------------------------------------------
        internal static TreeNode ToTreeNode(this ViewOutlineTreeControl.ViewOutlineTreeNode @this, ViewOutlineTreeControl.ShowMode showWith)
        {
            try
            {
                ViewOutlineTreeControl.ViewOutlineTreeNode newNode = @this;

                string temp = "";
                BaseWindowWrapper bwWrapper = @this.Tag as BaseWindowWrapper;
                MBodyEditColumn column = @this.Tag as MBodyEditColumn;
                switch (showWith)
                {
                    case ViewOutlineTreeControl.ShowMode.ResourcesNames:
                        if (bwWrapper != null)
                            temp = string.IsNullOrEmpty(bwWrapper.Id) ? bwWrapper.FullId : bwWrapper.Id;
                        else
                            temp = (column != null) ? column.Id : @this.Name;
                        break;
                    case ViewOutlineTreeControl.ShowMode.Variables:
                        if (bwWrapper != null)
                            temp = string.IsNullOrEmpty(bwWrapper.Name) ? bwWrapper.ControlLabel : bwWrapper.Name;
                        else
                            temp = (column != null) ? column.Name : @this.Name;
                        break;
                    case ViewOutlineTreeControl.ShowMode.Labels:
                    default:
                        temp = @this.Text;
                        if (bwWrapper != null)
                            temp = string.IsNullOrEmpty(bwWrapper.ControlLabel) ? bwWrapper.Text : bwWrapper.ControlLabel;
                        else if (column != null)
                            temp = string.IsNullOrEmpty(column.ColumnTitle) ? column.Name : column.ColumnTitle;
                        else
                        {
                            var control = @this.Tag as MParsedControl;
                            if (control != null)
                            {
                                temp = string.IsNullOrEmpty(control.ControlLabel) ? control.Text : control.ControlLabel;
                            }
                        }
                        break;
                };

                newNode.Text = RemoveSpecialCharacters(temp);
                return newNode;

            }
            catch (Exception)
            {
                @this.Text = RemoveSpecialCharacters(@this.Text);
                return @this;
            }
        }

        const string antiPattern = @"[^a-zA-Z0-9_.\s()[]{}-]+";
        static readonly Regex antiCharsRegex = new Regex(antiPattern, RegexOptions.Compiled);
        const string proPattern = @"[\r\n&]+";
        static readonly Regex proCharsRegex = new Regex(proPattern, RegexOptions.Compiled);
        //-----------------------------------------------------------------------------
        static string RemoveSpecialCharacters(string str)
        {
            var res = proCharsRegex.Replace(str, " ");
            res = antiCharsRegex.Replace(res, string.Empty);

            return res;
        }
    }

}
