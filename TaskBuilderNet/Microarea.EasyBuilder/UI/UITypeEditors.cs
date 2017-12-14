using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;
using Microarea.EasyBuilder.MVC;
using System.Collections.Generic;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using Microarea.EasyBuilder.CppData;
using System.Text.RegularExpressions;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder.UI
{
	//=============================================================================
	/// <remarks/>
	public class FieldUITypeEditor : UITypeEditor, IDisposable
	{
		TreeView tvTypes;
		IWindowsFormsEditorService service;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override bool IsDropDownResizable { get { return true; } }
		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			try
			{
				// user control
				CreateTree();
				SelectNode(tvTypes.Nodes, value);

				service.DropDownControl(tvTypes);
				if (IsValidSelection(tvTypes.SelectedNode))
					return tvTypes.SelectedNode.Tag;

				return value;
			}
			finally
			{
				tvTypes.Dispose();
			}
		}

		//--------------------------------------------------------------------------------
		private bool IsValidSelection(TreeNode treeNode)
		{
			if (treeNode == null || treeNode.Tag == null)
				return false;
			if (((DataType)treeNode.Tag) == DataType.Enum)
				return false;
			return true;
		}

		//--------------------------------------------------------------------------------
		private void SelectNode(TreeNodeCollection nodes, Object value)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag.Equals(value))
				{
					tvTypes.SelectedNode = node;
					break;
				}
				SelectNode(node.Nodes, value);
			}
		}


		//--------------------------------------------------------------------------------
		private void CreateTree()
		{
			tvTypes = new TreeView();
			tvTypes.ImageList = ImageLists.DatabaseExplorerTree;
			tvTypes.AfterSelect += new TreeViewEventHandler(tvTypes_AfterSelect);

			AddTreeItem(DataType.String);
			AddTreeItem(DataType.Text);
			AddTreeItem(DataType.Integer);
			AddTreeItem(DataType.Long);
			AddTreeItem(DataType.Double);
			AddTreeItem(DataType.Money);
			AddTreeItem(DataType.Quantity);
			AddTreeItem(DataType.Percent);
			AddTreeItem(DataType.Bool);
			AddTreeItem(DataType.Date);
			AddTreeItem(DataType.Time);
			AddTreeItem(DataType.DateTime);
			AddTreeItem(DataType.Enum);
			AddTreeItem(DataType.ElapsedTime);
			tvTypes.Sort();
			tvTypes.Size = new Size(150, 250);
		}

		//--------------------------------------------------------------------------------
		void tvTypes_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Action == TreeViewAction.ByMouse)
				service.CloseDropDown();
		}

		//--------------------------------------------------------------------------------
		private void AddTreeItem(DataType type)
		{
			TreeNode node = new TreeNode(type.ToString());
			node.ImageIndex = node.SelectedImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.Field;
			tvTypes.Nodes.Add(node);
			node.Tag = type;
			if (type == DataType.Enum)
			{
				foreach (DataType dt in DataType.EnumTypes)
				{
					TreeNode enumNode = new TreeNode(dt.ToString());
					enumNode.ImageIndex = enumNode.SelectedImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.Field;

					enumNode.Tag = dt;
					node.Nodes.Add(enumNode);
				}
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Dispose
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (tvTypes != null && !tvTypes.Disposing && !tvTypes.IsDisposed)
			{
				tvTypes.Dispose();
				tvTypes = null;
			}
		}
	}


	//=============================================================================
	/// <remarks/>
	public class EventsUITypeEditor : UITypeEditor
	{
		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			EasyBuilderBehaviour behaviour = context.Instance as EasyBuilderBehaviour;
			if (behaviour == null)
				return behaviour.Event;

			ListView lvEvents = new ListView();
			lvEvents.MultiSelect = false;
			lvEvents.View = View.List;
			lvEvents.SmallImageList = new ImageList();
			lvEvents.SmallImageList.Images.Add(Resources.Events);
			lvEvents.LargeImageList = lvEvents.SmallImageList;

			try
			{
				IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

				foreach (System.Reflection.EventInfo evnt in behaviour.EventOwner.GetType().GetEvents())
					lvEvents.Items.Add(new ListViewItem(evnt.Name, 0));

				service.DropDownControl(lvEvents);
				if (lvEvents.SelectedItems.Count > 0)
					behaviour.Event = lvEvents.SelectedItems[0].Text;

				return value;
			}
			finally
			{
				lvEvents.Dispose();
			}
		}
	}

	//=============================================================================
	/// <remarks/>
	public class FormModeUITypeEditor : UITypeEditor
	{
		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		override public object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			EasyBuilderBehaviour behaviour = context.Instance as EasyBuilderBehaviour;
			if (behaviour == null)
				return behaviour.Event;

			CheckedListBox lbxModes = new CheckedListBox();

			try
			{
				IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

				if (behaviour.FormModeRule.InBrowseEnabled)
					lbxModes.Items.Add(EasyBuilderBehaviourFormMode.InBrowseMode, behaviour.FormModeRule.InBrowse);
				if (behaviour.FormModeRule.InNewEnabled)
					lbxModes.Items.Add(EasyBuilderBehaviourFormMode.InNewMode, behaviour.FormModeRule.InNew);
				if (behaviour.FormModeRule.InEditEnabled)
					lbxModes.Items.Add(EasyBuilderBehaviourFormMode.InEditMode, behaviour.FormModeRule.InEdit);
				if (behaviour.FormModeRule.InFindEnabled)
					lbxModes.Items.Add(EasyBuilderBehaviourFormMode.InFindMode, behaviour.FormModeRule.InFind);

				service.DropDownControl(lbxModes);

				return value;
			}
			finally
			{
				lbxModes.Dispose();
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class EventOwnerUITypeEditor : UITypeEditor
	{
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			EasyBuilderBehaviour behaviour = context.Instance as EasyBuilderBehaviour;

			TBSite site = behaviour != null ? behaviour.Site as TBSite : null;

			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			// user control
			TreeView treeView = new TreeView();
			TreeNode root = new TreeNode(Resources.Controller, (int)ImageLists.ObjectModelImageIndex.Database, (int)ImageLists.ObjectModelImageIndex.Database);
			treeView.Nodes.Add(root);
			DocumentController controller = site.GetService(typeof(DocumentController)) as DocumentController;
			if (controller == null)
				return base.EditValue(context, provider, value);

			root.Tag = controller;
			TreeNode none = new TreeNode(Resources.None, (int)ImageLists.ObjectModelImageIndex.None, (int)ImageLists.ObjectModelImageIndex.None);
			treeView.Nodes.Add(none);
			none.Tag = string.Empty;

			controller.Document.Site = new TBSite(controller, null, site.Editor, EasyBuilderSerializer.DocumentPropertyName);

			ObjectModelTreeControl.FillDataModel(root, controller.Document, null, null, null);

			treeView.ImageList = ImageLists.ObjectModelTree;
			treeView.Tag = service;
			treeView.Width = treeView.Width * 2;    // TODOBRUNA trovare modo di accedere a dati propertygrid
			treeView.Height = treeView.Height * 2;
			treeView.MouseDoubleClick += new MouseEventHandler(TreeView_MouseDoubleClick);
			TreeNode oldSelectedNode = treeView.SelectedNode;
			service.DropDownControl(treeView);

			// no selection
			if (treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
				return base.EditValue(context, provider, value);

			return base.EditValue(context, provider, value);
		}

		//-----------------------------------------------------------------------------
		void TreeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			TreeView treeView = sender as TreeView;

			if (treeView != null && treeView.SelectedNode != null && treeView.SelectedNode.Tag is EasyBuilderComponent)
			{
				IWindowsFormsEditorService service = treeView.Tag as IWindowsFormsEditorService;
				treeView.MouseDoubleClick -= new MouseEventHandler(TreeView_MouseDoubleClick);
				service.CloseDropDown();
				treeView.Dispose();
			}
		}
	}

	/// <remarks/>
	//=============================================================================
	public class ImagesUITypeEditor : UITypeEditor
	{
		/// <remarks/>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override bool IsDropDownResizable { get { return true; } }

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			ImagesSelector imagesSelector = new ImagesSelector(new List<string>(value as List<string>));
			imagesSelector.Closed += new EventHandler(imageSelector_Closed);
			imagesSelector.Tag = service;

			object oldValue = value;
			service.DropDownControl(imagesSelector);
			value = imagesSelector.Images;

			PropertyChangingNotifier.OnComponentPropertyChanged(provider, (IComponent)context.Instance, "Images", oldValue, value);
			return imagesSelector.Images;
		}

		//--------------------------------------------------------------------------------
		void imageSelector_Closed(object sender, EventArgs e)
		{
			ImagesSelector imagesSelector = sender as ImagesSelector;
			if (imagesSelector == null)
				return;

			IWindowsFormsEditorService service = imagesSelector.Tag as IWindowsFormsEditorService;
			if (service == null)
				return;

			service.CloseDropDown();
		}
	}

	/// <remarks/>
	//=============================================================================
	public class ImageUITypeEditor : UITypeEditor
	{
		/// <remarks/>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override bool IsDropDownResizable { get { return true; } }

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			ImageSelector imageSelector = new ImageSelector(value as string);
			imageSelector.Closed += new EventHandler(imageSelector_Closed);
			imageSelector.Tag = service;

			string oldValue = value as string;
			service.DropDownControl(imageSelector);

			value = imageSelector.BkgImage as string;
			PropertyChangingNotifier.OnComponentPropertyChanged(provider, (IComponent)context.Instance, "BackgroundImage", oldValue, value);
			return value;
		}

		//--------------------------------------------------------------------------------
		void imageSelector_Closed(object sender, EventArgs e)
		{
			ImageSelector imageSelector = sender as ImageSelector;
			if (imageSelector == null)
				return;

			IWindowsFormsEditorService service = imageSelector.Tag as IWindowsFormsEditorService;
			if (service == null)
				return;

			service.CloseDropDown();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class EnumFlagUITypeEditor<T> : UITypeEditor where T : struct, IConvertible
	{
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{

			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			string sValue = value.ToString();
			TreeView treeView = new TreeView
			{
				CheckBoxes = true
			};
			treeView.SuspendLayout();
			string[] names = Enum.GetNames(typeof(T));
			Array vals = Enum.GetValues(typeof(T));
			for (int i = 0; i < names.Length; i++)
			{
				object val = vals.GetValue(i);
				if (Convert.ToInt32(val) == 0)
					continue;
				treeView.Nodes.Add(names[i]).Tag = val;
			}

			if (sValue != null)
			{
				string[] tokens = Regex.Split(sValue, ", ");
				foreach (TreeNode item in treeView.Nodes)
				{
					foreach (string t in tokens)
					{
						if (t.Equals(item.Text))
						{
							item.Checked = true;
							break;
						}
					}
				}
			}
			treeView.ImageList = ImageLists.EnumsTreeControl;
			treeView.Tag = service;

			treeView.ResumeLayout();
			service.DropDownControl(treeView);

			int e = 0;
			foreach (TreeNode n in treeView.Nodes)
			{
				if (n.Checked)
				{
					e |= Convert.ToInt32(n.Tag);
				}
			}
			return Enum.ToObject(typeof(T), e);
		}

	}

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class LinePositionUITypeEditor : EnumFlagUITypeEditor<ELinePos> { }

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class ControlStyleUITypeEditor : EnumFlagUITypeEditor<EControlStyle> { }
	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class RowViewUITypeEditor : UITypeEditor
	{
		IWindowsFormsEditorService service;
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			FormEditor editor = provider.GetService(typeof(FormEditor)) as FormEditor;
			string folder = Path.GetDirectoryName(editor.CurrentJsonFile);
			ListBox list = new ListBox();
			list.MouseDoubleClick += List_MouseDoubleClick;
			list.Tag = service;
			foreach (string item in Directory.GetFiles(folder))
				if (Path.GetExtension(item).Equals(NameSolverStrings.TbjsonExtension))
					list.Items.Add(Path.GetFileNameWithoutExtension(item));
			service.DropDownControl(list);
			return (list.SelectedItem == null) ? value : list.SelectedItem;
		}

		//-----------------------------------------------------------------------------
		void List_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListBox list = sender as ListBox;

			if (list.SelectedItem == null)
				return;

			service = list.Tag as IWindowsFormsEditorService;
			list.MouseDoubleClick -= new MouseEventHandler(List_MouseDoubleClick);
			service.CloseDropDown();
		}

	}

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class DataBindingUITypeEditor : UITypeEditor
	{
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IDataBindingConsumer dataBindingConsumer = context.Instance as IDataBindingConsumer;
			if (dataBindingConsumer == null)
				return value;

			IComponent component = dataBindingConsumer as IComponent;

			TBSite site = component != null ? component.Site as TBSite : null;

			if (site == null)
				return value;

			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			IDataBinding dataBinding = value as IDataBinding;

			IDataManager fixedDataObjOwner = dataBindingConsumer.FixedDataManager;

			DocumentController controller = site.GetService(typeof(DocumentController)) as DocumentController;
			if (controller == null)
				return base.EditValue(context, provider, value);

			// user control
			TreeView treeView = new TreeView();

			TreeNode none = new TreeNode(Resources.None, (int)ImageLists.ObjectModelImageIndex.None, (int)ImageLists.ObjectModelImageIndex.None);
			treeView.Nodes.Add(none);
			none.Tag = string.Empty;

			controller.Document.Site = new TBSite(controller, null, site.Editor, EasyBuilderSerializer.DocumentPropertyName);


			TreeNode root = new TreeNode(Resources.Controller, (int)ImageLists.ObjectModelImageIndex.Database, (int)ImageLists.ObjectModelImageIndex.Database);
			root.NodeFont = treeView.Font;

			treeView.Nodes.Add(root);
			root.Tag = controller;
			ObjectModelTreeControl.FillDataModel(root, controller.Document, dataBindingConsumer.CompatibleDataType, fixedDataObjOwner, dataBindingConsumer.ExcludedBindParentType);
			IDataManager oldParent = null;
			string oldFieldName = string.Empty;
			string oldMATable = string.Empty;
			if (value != null)
			{
				oldParent = dataBinding.Parent;
				oldFieldName = dataBinding.Parent.Record.GetField((IDataObj)dataBinding.Data).Name;
				oldMATable = dataBinding.Parent.Record.Name;
			}
			else if (fixedDataObjOwner != null)
				oldParent = fixedDataObjOwner;

			if (oldParent != null)
				treeView.SelectedNode = GetCurrentNode(treeView.Nodes, oldParent, oldFieldName, oldMATable);


			treeView.ImageList = ImageLists.ObjectModelTree;
			treeView.Tag = service;
			treeView.Width = treeView.Width * 2;
			treeView.Height = treeView.Height * 2;
			treeView.MouseDoubleClick += new MouseEventHandler(TreeView_MouseDoubleClick);
			TreeNode oldSelectedNode = treeView.SelectedNode;
			service.DropDownControl(treeView);

			// no selection
			if (treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
				return base.EditValue(context, provider, value);

			// selection
			if ((oldSelectedNode == null || oldSelectedNode != treeView.SelectedNode))
			{
				if (treeView.SelectedNode.Tag is MSqlRecordItem)
				{
					MSqlRecordItem mItem = (MSqlRecordItem)treeView.SelectedNode.Tag;
					MSqlRecord msqlRecord = mItem.Record as MSqlRecord;
					IDataManager selectedParent = msqlRecord.Site.Container as IDataManager;

					// only a predefined data manager can be selected
					if (fixedDataObjOwner != null && string.Compare(fixedDataObjOwner.Record.Name, selectedParent.Record.Name, true) != 0)
					{
						MessageBox.Show(string.Format(Resources.MandatoryDbtFields, fixedDataObjOwner));
						return base.EditValue(context, provider, value);
					}

					dataBinding = new FieldDataBinding((MDataObj)mItem.DataObj, selectedParent);
				}
				else
				{
					dataBinding = null;
				}
			}

			return base.EditValue(context, provider, dataBinding);

		}

		//--------------------------------------------------------------------------------
		private IDataManager GetParentDataManager(IDataObj dataObj)
		{
			EasyBuilderComponent cmp = ((EasyBuilderComponent)dataObj).Site.Container as EasyBuilderComponent;
			while (cmp != null)
			{
				if (cmp is IDataManager)
					return (IDataManager)cmp;
				cmp = cmp.Site.Container as EasyBuilderComponent;
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		internal object EditTableFields(ITypeDescriptorContext context, MBodyEditColumn control, IServiceProvider provider, object value)
		{
			return value;
		}

		//-----------------------------------------------------------------------------
		void TreeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			TreeView treeView = sender as TreeView;

			if (treeView == null || treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
				return;

			if (treeView.SelectedNode.Tag is MSqlRecordItem || treeView.SelectedNode.Tag is DataField || treeView.SelectedNode.Tag as string == "")
			{
				IWindowsFormsEditorService service = treeView.Tag as IWindowsFormsEditorService;
				treeView.MouseDoubleClick -= new MouseEventHandler(TreeView_MouseDoubleClick);
				service.CloseDropDown();
				treeView.Dispose();
			}
		}

		//-----------------------------------------------------------------------------
		TreeNode GetCurrentNode(TreeNodeCollection nodes, IDataManager parent, string dataObjName, string tableName)
		{
			TreeNode parentNode = FindParentNode(nodes, parent);
			if (parentNode == null)
				throw new Exception("Parent node not propertyFound!");

			TreeNode[] MATable = parentNode.Nodes?.Find(tableName, false);

			TreeNode[] dataObjNodes = MATable[0]?.Nodes?.Find(dataObjName, false);

			if (dataObjNodes == null || dataObjNodes.Length == 0)
				return null;
			if (dataObjNodes.Length > 1)
				throw new Exception("Too many dataObj nodes");

			return dataObjNodes[0];
		}

		//-----------------------------------------------------------------------------
		private TreeNode FindParentNode(TreeNodeCollection nodes, IDataManager parent)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag == parent)
					return node;
				TreeNode childNode = FindParentNode(node.Nodes, parent);

				if (childNode != null)
					return childNode;
			}
			return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class DataSourceUITypeEditor : UITypeEditor
	{
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{

			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			((Control)service).Cursor = Cursors.WaitCursor;
			CppInfo info = CppInfo.GetCurrent();

			FormEditor editor = provider.GetService(typeof(FormEditor)) as FormEditor;

			bool isBodyEdit = context.Instance is MBodyEdit;
			bool isBodyEditColumn = context.Instance is MBodyEditColumn;
			bool isMultiSel = context.Instance is MHeaderStrip;
			return isBodyEditColumn
				? EditColumnValue(value, service, info, editor, (MBodyEditColumn)context.Instance)//le colonne di bodyedit dipendono dal datasource impostato per il bodyedit e necessitano di trattamento diverso
				: EditNonColumnValue(value, service, info, editor, isBodyEdit, isMultiSel);

		}

		//--------------------------------------------------------------------------------
		private object EditNonColumnValue(object value, IWindowsFormsEditorService service, CppInfo info, FormEditor editor, bool isBodyEdit, bool isMultiSel)
		{
			DataSourceTreeControl dstc = new DataSourceTreeControl();
			TreeView treeView = dstc.DSTreeView;
			treeView.CheckBoxes = isMultiSel;
			treeView.SuspendLayout();
			treeView.AfterCheck += new TreeViewEventHandler(Node_AfterCheck);
			TreeNode none = new TreeNode(Resources.None, (int)ImageLists.ObjectModelImageIndex.None, (int)ImageLists.ObjectModelImageIndex.None);
			treeView.Nodes.Add(none);
			none.Tag = string.Empty;


			foreach (Document doc in info.Documents)
			{
				if (doc.IsEmpty)
					continue;
				ObjectModelTreeNode docNode = new ObjectModelTreeNode(doc.Name, null, ImageLists.ObjectModelImageIndex.MVCDocument);
				treeView.Nodes.Add(docNode);
				foreach (DataManager dbt in doc.Dbts)
				{
					if (isBodyEdit && !dbt.IsSlaveBuffered)
						continue;
					AddDataManagerNode(docNode, dbt, true, isBodyEdit);
				}
				if (!isBodyEdit)
				{
					foreach (DataManager hkl in doc.HotLinks)
					{
						AddDataManagerNode(docNode, hkl, false, false);
					}
					foreach (DataField var in doc.Variables)
					{
						ObjectModelTreeNode fieldNode = new ObjectModelTreeNode(var.Name, var.Name, ImageLists.ObjectModelImageIndex.Field);
						docNode.Nodes.Add(fieldNode);
					}
				}
			}
			treeView.Sort();

			if (value != null)
			{
				if (isMultiSel)
				{
					string[] tokens = ((string)value).Split(',');
					Dictionary<TreeNode, int> parents = new Dictionary<TreeNode, int>();
					foreach (string t in tokens)
					{
						treeView.SelectedNode = GetCurrentNode(treeView.Nodes, t.Trim(), editor.JsonFormContext);
						if (treeView.SelectedNode != null)
						{
							treeView.SelectedNode.Checked = true;
							if (!parents.ContainsKey(treeView.SelectedNode.Parent))
								parents.Add(treeView.SelectedNode.Parent, 0);
							parents[treeView.SelectedNode.Parent] += 1;
						}

					}
					foreach (TreeNode item in parents.Keys)
					{
						if (item.GetNodeCount(true) == parents[item])
						{
							item.Checked = true;
							item.Expand();
						}
					}

				}
				else
				{
					treeView.SelectedNode = GetCurrentNode(treeView.Nodes, (string)value, editor.JsonFormContext);
					treeView.MouseDoubleClick += new MouseEventHandler(TreeView_MouseDoubleClick);
				}
			}
			treeView.ImageList = ImageLists.ObjectModelTree;
			treeView.Tag = service;
			if (service is Control)
			{
				dstc.Width = ((Control)service).Width;
			}


			TreeNode oldSelectedNode = treeView.SelectedNode;

			treeView.ResumeLayout();
			((Control)service).Cursor = Cursors.Default;
			service.DropDownControl(dstc);
			if (isMultiSel)
			{
				StringBuilder dataSource = new StringBuilder();
				foreach (TreeNode n in CheckedNodes(treeView.Nodes))
				{
					if (dataSource.Length != 0 && n.Tag != null)
						dataSource.Append(", ");
					dataSource.Append(n.Tag);
				}
				return dataSource.ToString();
			}
			else
			{
				// no selection
				if (treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
					return value;

				// selection
				string dataSource = (String)value;
				if ((oldSelectedNode == null || oldSelectedNode != treeView.SelectedNode))
				{
					if (treeView.SelectedNode.Tag is string)
						dataSource = (string)treeView.SelectedNode.Tag;
				}
				return dataSource;
			}


		}

		//--------------------------------------------------------------------------------
		private List<TreeNode> CheckedNodes(TreeNodeCollection theNodes)
		{
			List<TreeNode> aResult = new List<TreeNode>();

			if (theNodes != null)
			{
				foreach (System.Windows.Forms.TreeNode aNode in theNodes)
				{
					if (aNode.Checked)
					{
						aResult.Add(aNode);
					}

					aResult.AddRange(CheckedNodes(aNode.Nodes));
				}
			}

			return aResult;
		}

		//--------------------------------------------------------------------------------
		private object EditColumnValue(object value, IWindowsFormsEditorService service, CppInfo info, FormEditor editor, MBodyEditColumn col)
		{
			if (col == null)
				return value;
			MBodyEdit body = col.BodyEdit;
			if (string.IsNullOrEmpty(body.DataSource))
				return value;
			ListBox list = new ListBox();
			list.Tag = service;
			list.MouseDoubleClick += List_MouseDoubleClick;
			((Control)service).Cursor = Cursors.WaitCursor;
			List<DataManager> dbts = new List<DataManager>();
			DataManager dm = null;
			foreach (Document doc in info.Documents)
			{
				foreach (var dbt in doc.Dbts)
				{
					if (!dbt.IsSlaveBuffered)
						continue;
					if (dbt.Name == body.DataSource)
					{
						dbts.Add(dbt);
						if (doc.Name == editor.JsonFormContext.Document)
						{
							dm = dbt;
							break;
						}
					}
				}
			}

			if (dbts.Count == 0)
				return value;
			if (dm == null)
				dm = dbts[0];

			((Control)service).Cursor = Cursors.Default;

			foreach (var field in dm.Fields)
				list.Items.Add(field.Name);
			list.SelectedItem = value;
			service.DropDownControl(list);

			// no selection
			return (list.SelectedItem == null) ? value : list.SelectedItem;

		}

		//-----------------------------------------------------------------------------
		void List_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListBox list = sender as ListBox;

			if (list.SelectedItem == null)
				return;

			IWindowsFormsEditorService service = list.Tag as IWindowsFormsEditorService;
			list.MouseDoubleClick -= new MouseEventHandler(List_MouseDoubleClick);
			service.CloseDropDown();
		}

		//--------------------------------------------------------------------------------
		private void AddDataManagerNode(ObjectModelTreeNode docNode, DataManager dataManager, bool dbt, bool isBodyEdit)
		{
			ObjectModelTreeNode dbtNode = new ObjectModelTreeNode(
				dataManager.Name,
				isBodyEdit ? dataManager.Name : null,//per il bodyedit devo selezionare il solo dbt, assegno il tag che mi permette di selezionare il nodo
				dbt ? ImageLists.ObjectModelImageIndex.DataManagerNode : ImageLists.ObjectModelImageIndex.HotLink);
			docNode.Nodes.Add(dbtNode);

			if (!isBodyEdit)//se sono un bodyedit, non devo selezionare campi
			{
				foreach (DataField field in dataManager.Fields)
				{
					ObjectModelTreeNode fieldNode = new ObjectModelTreeNode(field.Name, string.Concat(dataManager.Name, '.', field.Name), ImageLists.ObjectModelImageIndex.Field);
					dbtNode.Nodes.Add(fieldNode);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private TreeNode GetCurrentNode(TreeNodeCollection treeNodeCollection, string dataSource, NameSpace jsonFormContext)
		{
			string docName = jsonFormContext.Document;
			if (string.IsNullOrEmpty(dataSource))
			{
				if (!string.IsNullOrEmpty(docName))
				{
					TreeNode[] docNodes = treeNodeCollection.Find(docName, false);
					if (docNodes != null && docNodes.Length > 0)
						return docNodes[0];
				}
			}
			else
			{
				string[] tokens = dataSource.Split('.');
				if (tokens != null && tokens.Length == 1)
				{
					//campo di documento
					TreeNode[] fieldNodes = treeNodeCollection.Find(tokens[0], true);
					if (fieldNodes == null)
						return null;
					if (fieldNodes.Length == 0)
						return null;
					//ne ho trovato solo uno
					else if (fieldNodes.Length == 1 || string.IsNullOrEmpty(docName))
						return fieldNodes[0];
					//ne ho trovato più di uno: ritorno quello del documento corrente, se ch'è
					foreach (TreeNode fieldNode in fieldNodes)
					{
						if (fieldNode.Parent != null && fieldNode.Parent.Name == docName)
							return fieldNode;
					}
					//ritorno il primo a disposizione, come ultima chance
					return fieldNodes[0];
				}
				else if (tokens.Length == 2)
				{
					TreeNode[] dbtNodes = treeNodeCollection.Find(tokens[0], true);
					if (dbtNodes == null || dbtNodes.Length == 0)
						return null;
					TreeNode dbtNode = null;
					if (dbtNodes.Length == 1)
						dbtNode = dbtNodes[0];
					else if (!string.IsNullOrEmpty(docName))
					{
						//ne ho trovato più di uno: ritorno quello del documento corrente, se c'è
						foreach (TreeNode n in dbtNodes)
						{
							if (n != null && n.Parent != null && n.Parent.Name == docName)
							{
								dbtNode = n;
								break;
							}
						}

					}
					if (dbtNode == null)
						dbtNode = dbtNodes[0];

					TreeNode[] fieldNodes = dbtNode.Nodes.Find(tokens[1], true);
					return fieldNodes.Length > 0 ? fieldNodes[0] : null;
				}
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		void TreeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			TreeView treeView = sender as TreeView;

			if (treeView == null || treeView.SelectedNode == null || treeView.SelectedNode.Tag == null)
				return;

			if (treeView.SelectedNode.Tag is string)
			{
				IWindowsFormsEditorService service = treeView.Tag as IWindowsFormsEditorService;
				treeView.MouseDoubleClick -= new MouseEventHandler(TreeView_MouseDoubleClick);
				service.CloseDropDown();
			}
		}

		//-----------------------------------------------------------------------------
		private void Node_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (e.Action == TreeViewAction.Unknown)
				return;
			e.Node.Expand();
			if (e.Node.Nodes.Count > 0)
				CheckAllChildNodes(e.Node, e.Node.Checked);
		}

		//-----------------------------------------------------------------------------		
		private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
		{
			foreach (TreeNode node in treeNode.Nodes)
			{
				node.Checked = nodeChecked;
				if (node.Nodes.Count > 0)
				{
					if (nodeChecked)
						node.Expand();
					else
						node.Collapse();
					CheckAllChildNodes(node, nodeChecked);
				}
			}
		}

	}



	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class ItemSourceUITypeEditor : UITypeEditor
	{
		TreeView tvItemSources;
		IWindowsFormsEditorService service;

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override bool IsDropDownResizable { get { return true; } }
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			const string prefix = "ItemSource.";

			service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			((Control)service).Cursor = Cursors.WaitCursor;

			try
			{
				CreateTree();

				TreeNode oldSelectedNode = tvItemSources.SelectedNode;
				((Control)service).Cursor = Cursors.Default;

				string itemSourceNS = (String)value;
				tvItemSources.SelectedNode = Find(tvItemSources.Nodes, itemSourceNS.StartsWith(prefix) ? itemSourceNS : prefix + itemSourceNS);
				service.DropDownControl(tvItemSources);

				TreeNode currentNode = tvItemSources.SelectedNode;

				if (currentNode == null || currentNode.Tag == null)
					return value;

				// selection
				if ((oldSelectedNode == null || oldSelectedNode != currentNode))
				{
					itemSourceNS = currentNode.Tag.ToString();
				}
				return itemSourceNS.StartsWith(prefix) ? itemSourceNS.Substring(prefix.Length) : itemSourceNS;
			}
			finally
			{
				tvItemSources.Dispose();
			}

		}

		//--------------------------------------------------------------------------------
		private void CreateTree()
		{
			tvItemSources = new TreeView();
			tvItemSources.Size = new Size(250, 300);
			tvItemSources.DoubleClick += TvItemSources_DoubleClick;

			try
			{
				tvItemSources.Scrollable = false;
				tvItemSources.Nodes.Clear();
				tvItemSources.ImageList = new ImageList();
				tvItemSources.ImageList.Images.Add(Resources.Application32x32);
				tvItemSources.ImageList.Images.Add(Resources.Module32x32);
				tvItemSources.ImageList.Images.Add(Resources.Database);

				LoadTreeItems();

				tvItemSources.TreeViewNodeSorter = new TreeNodeComparer();
				tvItemSources.Sort();
			}
			finally
			{
				((Control)service).Cursor = Cursors.Default;
				tvItemSources.Scrollable = true;
				tvItemSources.EndUpdate();
			}
		}

		//--------------------------------------------------------------------------------
		private void TvItemSources_DoubleClick(object sender, EventArgs e)
		{
			TreeView treeView = sender as TreeView;

			if (treeView != null && treeView.SelectedNode != null)
			{
				treeView.MouseDoubleClick -= new MouseEventHandler(TvItemSources_DoubleClick);
				service.CloseDropDown();
				treeView.Dispose();
			}
		}

		//--------------------------------------------------------------------------------
		private void LoadTreeItems()
		{
			try
			{
				List<string> appTitles = new List<string>();
				List<string> modTitles = new List<string>();
				List<string> titles = new List<string>();
				List<string> moduleNamespaces = new List<string>();

				CUtility.GetItemSources(appTitles, modTitles, titles, moduleNamespaces);

				if (titles.Count == 0)
					return;

				for (int i = 0; i < titles.Count; i++)
				{
					string app = appTitles[i];
					string mod = modTitles[i];
					string title = titles[i];
					string nameSpace = moduleNamespaces[i];
					TreeNode appNode = GetNode(tvItemSources.Nodes, app, 0);
					TreeNode modNode = GetNode(appNode.Nodes, mod, 1);
					TreeNode itemSourceNode = GetNode(modNode.Nodes, title, 2);
					itemSourceNode.Tag = nameSpace;
				}
			}
			finally
			{
				((Control)service).Cursor = Cursors.Default;
				tvItemSources.Scrollable = true;
			}
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
		private TreeNode Find(TreeNodeCollection nodes, string ns)
		{
			foreach (TreeNode item in nodes)
			{
				if (item.Tag != null && item.Tag.ToString().CompareNoCase(ns))
					return item;
				TreeNode n = Find(item.Nodes, ns);
				if (n != null)
					return n;
			}
			return null;
		}

	}


	/// <summary>
	/// 
	/// </summary>
	//=============================================================================
	public class HotlinkUITypeEditor : UITypeEditor
	{
		TreeNode currentNode;
		IWindowsFormsEditorService service;

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			const string prefix = "HotKeyLink.";

			service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			((Control)service).Cursor = Cursors.WaitCursor;

			FormEditor editor = provider.GetService(typeof(FormEditor)) as FormEditor;
            Action<TreeNode> treeFiller = null;
            if (editor.RunAsEasyStudioDesigner)
                treeFiller = new Action<TreeNode>(
					(TreeNode aNode) =>
					{
						SortedDictionary<string, string> hotLinks = editor.Document.GetUnWrappedHotLinks();
						foreach (string name in hotLinks.Keys)
						{
							TreeNode hklNode = new TreeNode(string.Format("{0}: {1}", name, hotLinks[name]), 4, 4);
							aNode.Nodes.Add(hklNode);
							hklNode.Tag = name;
						}
					}
				);

			HotLinksExplorer hltc = new HotLinksExplorer(treeFiller, true);
			hltc.NodeSelected += TreeHotLinks_NodeDoubleClick;

			TreeView treeView = hltc.TreeHotLinks;

			TreeNode oldSelectedNode = treeView.SelectedNode;
			((Control)service).Cursor = Cursors.Default;

			string hotlinkNS = (String)value;
			treeView.SelectedNode = Find(treeView.Nodes, hotlinkNS.StartsWith(prefix) ? hotlinkNS : prefix + hotlinkNS);
			service.DropDownControl(hltc);

			currentNode = treeView.SelectedNode;

			if (currentNode == null || currentNode.Tag == null)
				return value;

			// selection
			if ((oldSelectedNode == null || oldSelectedNode != currentNode))
			{
				hotlinkNS = currentNode.Tag.ToString();
			}
			return hotlinkNS.StartsWith(prefix) ? hotlinkNS.Substring(prefix.Length) : hotlinkNS;
		}

		//-----------------------------------------------------------------------------
		private TreeNode Find(TreeNodeCollection nodes, string ns)
		{
			foreach (TreeNode item in nodes)
			{
				if (item.Tag?.ToString() == ns)
					return item;
				TreeNode n = Find(item.Nodes, ns);
				if (n != null)
					return n;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		private void TreeHotLinks_NodeDoubleClick(object sender, NodeArgs e)
		{
			service.CloseDropDown();
		}

	}


	/// <summary>
	/// Internal use
	/// </summary>
	//=========================================================================
	public class EasyBuilderComponentCollectionEditor : CollectionEditor
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override CollectionForm CreateCollectionForm()
		{
			CollectionForm form = base.CreateCollectionForm();
			form.Width = 1000;
			form.Height = 500;
			return form;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemType"></param>
		/// <returns></returns>
		protected override object CreateInstance(Type itemType)
		{
			EasyBuilderComponent cmp = (EasyBuilderComponent)base.CreateInstance(itemType); //item
			cmp.ParentComponent = Context.Instance as EasyBuilderComponent; //grid o tiem
			return cmp;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public EasyBuilderComponentCollectionEditor(Type type)
			: base(type)
		{
		}

	}

	/// <summary>
	/// Internal use
	/// </summary>
	//================================================================================

	public class AnchorTypeEditor : UITypeEditor
	{
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{

			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			((Control)service).Cursor = Cursors.WaitCursor;

			ListBox lvItems = new ListBox();
			lvItems.Tag = service;
			lvItems.MouseDoubleClick += List_MouseDoubleClick;
			BaseWindowWrapper bww = context.Instance as BaseWindowWrapper;
			IWindowWrapperContainer parent = bww?.Parent;
			// in caso di multi selezione bbw e parent saranno null

			try
			{
				lvItems.Items.Add("COL1");
				lvItems.Items.Add("COL2");
				if (bww != null && parent != null)
				{
					foreach (IComponent cmp in parent.Components)
					{
						BaseWindowWrapper child = cmp as BaseWindowWrapper;
						if (child == null)
							continue;
						//semplificazione: l'ordine delle finestre corrisponde a quello di creazione: posso agganciarmi
						//solo alla destra di una finestra che mi precede nello zorder, perché prima devo averla creata
						if (child == bww)
							break;
						lvItems.Items.Add(child.Id);
					}
				}
				service.DropDownControl(lvItems);
				if (lvItems.SelectedItems.Count > 0)
					return lvItems.SelectedItem;

				return value;
			}
			finally
			{
				lvItems.Dispose();
			}

		}

		//-----------------------------------------------------------------------------
		void List_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListBox list = sender as ListBox;

			if (list.SelectedItem == null)
				return;

			IWindowsFormsEditorService service = list.Tag as IWindowsFormsEditorService;
			list.MouseDoubleClick -= new MouseEventHandler(List_MouseDoubleClick);
			service.CloseDropDown();
		}

	}

}
