using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using System.Diagnostics;

namespace Microarea.EasyBuilder.UI
{
	//=========================================================================
	/// <remarks/>
	internal partial class ObjectModelTreeControl : UserControl
	{
		//================================================================================
		class ViewComponentComparer : IComparer<IComponent>
		{
			//--------------------------------------------------------------------------------
			public int Compare(IComponent x, IComponent y)
			{
				//metto il tabber in testa, che posso colassarlo e occupa poco spazio
				if (x is MTabber && !(y is MTabber))
					return -1;
				if (!(x is MTabber) && y is MTabber)
					return 1;
				EasyBuilderControl bwwX = x as EasyBuilderControl;
				EasyBuilderControl bwwY = y as EasyBuilderControl;

				int cmp = (bwwX == null || bwwY == null) ? 0 : bwwX.TabOrder.CompareTo(bwwY.TabOrder);
				if (cmp == 0)
					return ((EasyBuilderComponent)x).Name.CompareTo(((EasyBuilderComponent)y).Name);
				return cmp;
			}
		}

		/// <remarks/>
		public event EventHandler OpenProperties;
		/// <remarks/>
		public event EventHandler RefreshModel;
		/// <remarks/>
		public event EventHandler<AddDataManagerEventArgs> AddDbt;
		/// <remarks/>
		internal event EventHandler<AddFieldEventArgs> AddField;
		/// <remarks/>
		public event EventHandler<DeleteObjectEventArgs> DeleteObject;
		/// <remarks/>
		public event EventHandler<AddHotLinkEventArgs> AddHotLink;
		/// <remarks/>
		public event EventHandler<AddDataManagerEventArgs> AddDataManager;
		/// <remarks/>
		public event EventHandler<DeclareComponentEventArgs> DeclareComponent;

		ObjectModelTreeNode controllerNode;
		ObjectModelTreeNode documentNode;
		ObjectModelTreeNode businessObjectsNode;

		object[] dummyArgs = { };

		ISelectionService selectionService;
		private FormEditor editor;
		private TreeNode selectedFocusNode;
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
		public ObjectModelTreeControl(FormEditor editor, bool manageOnlyViewModel)
		{
			InitializeComponent();
			this.Text = Resources.ObjectModelTitle;
			this.editor = editor;
			treeDataManagers.ImageList = ImageLists.ObjectModelTree;
			selectionService = editor.GetService(typeof(ISelectionService)) as ISelectionService;
		
			if (selectionService != null)
				selectionService.SelectionChanged += new EventHandler(SelectionService_SelectionChanged);

			TreeViewExtender extender = new TreeViewExtender(treeDataManagers);

			//questi due eventi evitano che durante la perdita/riacquisizione di fuoco del tree, venga selezionato automaticamente il rootNode
			treeDataManagers.LostFocus += (sender, args) => selectedFocusNode = treeDataManagers.SelectedNode;
			treeDataManagers.GotFocus += (sender, args) => treeDataManagers.SelectedNode = selectedFocusNode;

			PopulateTree(editor.Controller);
		}

		//-----------------------------------------------------------------------------
		private void TreeDataManagers_KeyDown(object sender, KeyEventArgs e)
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
		void SelectionService_SelectionChanged(object sender, EventArgs e)
		{
			if (selectionService == null)
				return;

			if (selectionService.SelectionCount == 1)
				SelectObject(selectionService.PrimarySelection);
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
				treeDataManagers.BeginUpdate();

				if (documentController == null)
				{
					treeDataManagers.Nodes.Add(Resources.InitSources);
					return;
				}

				treeDataManagers.Nodes.Clear();

				controllerNode = new ObjectModelTreeNode(Resources.Controller, documentController, ImageLists.ObjectModelImageIndex.MVCController);
                controllerNode.NodeFont = treeDataManagers.Font;
                treeDataManagers.Nodes.Add(controllerNode);
				documentNode = FillDataModel(controllerNode, documentController.Document, null, null, null);
                FillBusinessObjects();
				controllerNode.Expand();

				treeDataManagers.SelectedNode = controllerNode;
				selectionService.SetSelectedComponents(new Object[] { controllerNode.Tag });

			}
			finally
			{
				treeDataManagers.EndUpdate();
			}
		}

		//-----------------------------------------------------------------------------
		private void FillBusinessObjects()
		{
			//Le standardizzazioni demandano la gestione dei BusinessObjects al module editor.
			if (documentNode == null || BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization)
				return;

			if (businessObjectsNode != null)
			{
				businessObjectsNode.Parent.Nodes.Remove(businessObjectsNode);
			}

			businessObjectsNode = new ObjectModelTreeNode(
				ControllerSources.BusinessObjectsPropertyName,
				ControllerSources.BusinessObjectsPropertyName,
				ImageLists.ObjectModelImageIndex.BusinessObjects
				);
            businessObjectsNode.NodeFont = new Font(documentNode.NodeFont, FontStyle.Bold);
			documentNode.Nodes.Add(businessObjectsNode);

			UpdateReferencedComponents(businessObjectsNode, typeof(BusinessObject), ImageLists.ObjectModelImageIndex.BusinessObjects);
		}

		//-----------------------------------------------------------------------------
		private void UpdateReferencedComponents(ObjectModelTreeNode containerNode, Type componentType, ImageLists.ObjectModelImageIndex nodeType)
		{
			string key = null;
			foreach (ReferenceableComponent refComponent in editor.ComponentDeclarator.GetReferenceableComponents(componentType))
			{
				key = refComponent.ToString();
				if (containerNode.Nodes.ContainsKey(key))
					continue;

				ObjectModelTreeNode packageNode = new ObjectModelTreeNode(refComponent.ToString(), refComponent, nodeType);
				packageNode.ToolTipText = string.Format("class {0}", refComponent.MainClass);
				if (!refComponent.IsValid)
				{
					packageNode.ForeColor = Color.Red;
					packageNode.Text = string.Format("{0} - {1}", Resources.InvalidComponent, packageNode.Text);
				}

				containerNode.Nodes.Add(packageNode);
			}
			containerNode.Expand();
		}


		//-----------------------------------------------------------------------------
		private void AddViewComponents(TreeNodeCollection parentNodes, IContainer container)
		{
			//clono la lista per ordinarla in base a ZIndex
			List<IComponent> components = new List<IComponent>();

			foreach (IComponent cmp in container.Components)
			{
				EasyBuilderComponent ebComponent = cmp as EasyBuilderComponent;
				if (SkipComponent(ebComponent))
					continue;
				components.Add(ebComponent);
			}

			components.Sort(new ViewComponentComparer());

			foreach (EasyBuilderComponent ebComponent in components)
			{
				if (!ebComponent.CanShowInObjectModel)
					continue;
				string name = ebComponent.Name;
				//se sono nel json designer devo far vedere l'idc
				if (ebComponent.DesignModeType == EDesignMode.Static && ebComponent is BaseWindowWrapper)
				{
					string id = ((BaseWindowWrapper)ebComponent).Id;
					if (!string.IsNullOrEmpty(id))
						name = id;
				}
				// se il name del componente è vuoto vuol dire che nel C++ il componente
				// non è più presente (cambio di configurazione)
				if (string.IsNullOrEmpty(name))
					continue;
				ObjectModelTreeNode node = new ObjectModelTreeNode(name, ebComponent);

				parentNodes.Add(node);
				IContainer cnt = ebComponent as IContainer;
				if (cnt != null)
					AddViewComponents(node.Nodes, cnt);
			}

		}

		//-----------------------------------------------------------------------------
		private static bool SkipComponent(EasyBuilderComponent ebComponent)
		{
			return ebComponent == null;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public static ObjectModelTreeNode FillDataModel(
			TreeNode parentNode,
			MDocument document,
			IDataType filterDataType,
			IDataManager filterDbt,
			Type excludedParentType
			)
		{
			ObjectModelTreeNode n = new ObjectModelTreeNode(EasyBuilderSerializer.DocumentPropertyName, document, ImageLists.ObjectModelImageIndex.MVCDocument);
            n.NodeFont = parentNode.NodeFont;
            parentNode.Nodes.Add(n);

			FillDocumentNode(n, document, filterDataType, filterDbt, excludedParentType);
			return n;
		}

		//-----------------------------------------------------------------------------
		private static void FillDocumentNode(
			ObjectModelTreeNode documentNode,
			MDocument document,
			IDataType filterDataType,
			IDataManager filterDbt,
			Type excludedParentType
			)
		{
			if (documentNode == null)
				return;

			ObjectModelTreeNode dbtsNode = new ObjectModelTreeNode(ControllerSources.DBTPropertyName, ControllerSources.DBTPropertyName, ImageLists.ObjectModelImageIndex.Dbts);
            dbtsNode.NodeFont = new Font(documentNode.NodeFont, FontStyle.Bold);
            documentNode.Nodes.Add(dbtsNode);
			documentNode.Expand();


			MDBTMaster aMDbtMaster = document.Master as MDBTMaster;
			if (aMDbtMaster != null)
			{
				if (filterDbt == null || aMDbtMaster.Name == filterDbt.GetType().Name)
				{
					ObjectModelTreeNode dbtNode = FillDataManagerNode(aMDbtMaster, filterDataType, ImageLists.ObjectModelImageIndex.Master);
					dbtsNode.Nodes.Add(dbtNode);
					dbtsNode.Expand();
				}
			}
			foreach (IComponent component in document.Components)
			{
				MDBTSlave dbtSlave = component as MDBTSlave;
				if (dbtSlave == null)
					continue;

				// filtro su Dbt specifico
				if (filterDbt != null && dbtSlave.GetType().Name != filterDbt.GetType().Name)
					continue;

				// filtro su tipologia di DBT
				if (excludedParentType != null && dbtSlave.GetType().IsSubclassOf(excludedParentType))
					continue;

				ImageLists.ObjectModelImageIndex nodeType = dbtSlave.Relation == DataRelationType.OneToMany
					? ImageLists.ObjectModelImageIndex.SlaveBuffered
					: ImageLists.ObjectModelImageIndex.Slave;

				ObjectModelTreeNode dbtNode = FillDataManagerNode(dbtSlave, filterDataType, nodeType);
				dbtsNode.Nodes.Add(dbtNode);
				dbtsNode.Expand();

				if (dbtSlave is MDBTSlaveBuffered)
					FillDbtSubSlave(filterDataType, dbtSlave, dbtNode);
			}
    
			//Hotlinks, datamanagers e localfield possono venir aggiunti solamente se c'è un dbt master
			FillHotLinks(documentNode, document);
			FillDataManagers(documentNode, document);
			FillLocalFields(documentNode, document);
		}

		//-----------------------------------------------------------------------------
		private static void FillDbtSubSlave(IDataType filterDataType, MDBTSlave dbtSlave, ObjectModelTreeNode dbtNode)
		{
			foreach (MDBTSlave subSlave in ((MDBTSlaveBuffered)dbtSlave).SlavePrototypes)
			{
				ImageLists.ObjectModelImageIndex nodeType = subSlave.Relation == DataRelationType.OneToMany
					? ImageLists.ObjectModelImageIndex.SlaveBuffered
					: ImageLists.ObjectModelImageIndex.Database;
				ObjectModelTreeNode subDbtNode = FillDataManagerNode(subSlave, filterDataType, nodeType);
				dbtNode.Nodes.Add(subDbtNode);
				dbtNode.Expand();

				if (subSlave is MDBTSlaveBuffered)
					FillDbtSubSlave(filterDataType, subSlave, subDbtNode);
			}
		}

		//-----------------------------------------------------------------------------
		private static void FillLocalFields(ObjectModelTreeNode docNode, MDocument document)
		{
			ObjectModelTreeNode fieldsNode = null;

			MemberInfo[] dataObjMemberInfos = ReflectionUtils.GetMemberInfos(document.GetType(), typeof(MDataObj));
			object[] dummyArgs = { };
			foreach (MemberInfo dataObjMemberInfo in dataObjMemberInfos)
			{
				if (dataObjMemberInfo == null)
					continue;

				FieldInfo fInfo = dataObjMemberInfo as FieldInfo;
				if (fInfo == null)
					continue;

				MDataObj aMDataObj = fInfo.GetValue(document) as MDataObj;
				if (aMDataObj == null)
					continue;

				// il nodo appare solo se c'è qualcosa visto che è di fatto read-only
				if (fieldsNode == null)
				{
					fieldsNode = new ObjectModelTreeNode(ControllerSources.LocalFieldsPropertyName, ControllerSources.LocalFieldsPropertyName, ImageLists.ObjectModelImageIndex.LocalFields);
                    fieldsNode.NodeFont = new Font(docNode.NodeFont, FontStyle.Bold);
                    docNode.Nodes.Add(fieldsNode);
				}
			
				ObjectModelTreeNode DataObjNode = new ObjectModelTreeNode(dataObjMemberInfo.Name, aMDataObj, ImageLists.ObjectModelImageIndex.Field);
				fieldsNode.Nodes.Add(DataObjNode);
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal static ObjectModelTreeNode FillDataManagerNode(EasyBuilderComponent ebComponent, IDataType filterDataType, ImageLists.ObjectModelImageIndex nodeType)
		{
			object[] dummyArgs = { };
			ObjectModelTreeNode newNode = new ObjectModelTreeNode(ebComponent.GetType().Name, ebComponent, nodeType);

			MGenericDataManager dataManager = ebComponent as MGenericDataManager;

			foreach (var item in dataManager.Components)
			{
				MSqlRecord sqlRecord = item as MSqlRecord;
				if (sqlRecord == null)
					continue;

				MDBTObject dbt = dataManager as MDBTObject;
				if (dbt != null && sqlRecord == dbt.OldRecord)
					continue;

				ObjectModelTreeNode mSqlRecNode = new ObjectModelTreeNode(sqlRecord.Name, sqlRecord, ImageLists.ObjectModelImageIndex.Table);
				newNode.Nodes.Add(mSqlRecNode);

				List<ObjectModelTreeNode> nodes = new List<ObjectModelTreeNode>();
				foreach (var recordItem in sqlRecord.Components)
				{
					MSqlRecordItem sqlRecordItem = recordItem as MSqlRecordItem;
					if (sqlRecordItem == null)
						continue;

					MDataObj aMDataObj = sqlRecordItem.DataObj as MDataObj;
					if (aMDataObj == null)
						continue;

					if (
						filterDataType != null &&
						(
							filterDataType.Type != aMDataObj.DataType.Type
							||
							(filterDataType.Type != DataType.Enum.Type && filterDataType.Tag != aMDataObj.DataType.Tag)
						)
					)
						continue;
					IRecordField field = sqlRecord.GetField(aMDataObj);

					Debug.Assert(field != null);
					//string fieldName = field != null
					//	? field.Name
					//	: dataObjMemberInfo.Name;

					ImageLists.ObjectModelImageIndex nodeImage = field != null && field.IsSegmentKey
						? ImageLists.ObjectModelImageIndex.KeyDatabaseItem
						: field is MLocalSqlRecordItem
							? ImageLists.ObjectModelImageIndex.Field
							: ImageLists.ObjectModelImageIndex.DatabaseItem;

					ObjectModelTreeNode dataObjNode = new ObjectModelTreeNode(field.Name, field, nodeImage);
					nodes.Add(dataObjNode);
				}

				nodes.Sort((x, y) =>
				{
					if (x.ImageIndex == y.ImageIndex)
						return x.Text.CompareTo(y.Text);
					//invertiti: per prime le immagini dei campi chiave
					return y.ImageIndex.CompareTo(x.ImageIndex);
				});

				mSqlRecNode.Nodes.AddRange(nodes.ToArray());
			}
			return newNode;
		}

		//-----------------------------------------------------------------------------	
		internal static void FillHotLinks(ObjectModelTreeNode docNode, MDocument document)
		{
			if (docNode == null)
				return;

			ObjectModelTreeNode hotLinksNode = new ObjectModelTreeNode(ControllerSources.HotLinksPropertyName, ControllerSources.HotLinksPropertyName, ImageLists.ObjectModelImageIndex.HotLinks);
            hotLinksNode.NodeFont = new Font(docNode.NodeFont, FontStyle.Bold);
            docNode.Nodes.Add(hotLinksNode);

			foreach (IComponent component in document.Components)
			{
				MHotLink hotLink = component as MHotLink;
				if (hotLink == null)
					continue;

				ObjectModelTreeNode hklNode = FillDataManagerNode(hotLink, null, ImageLists.ObjectModelImageIndex.HotLink);
				// non controllo se ha componenti perche' esistono hotlink che non ne hanno
				hotLinksNode.Nodes.Add(hklNode);
			}
			hotLinksNode.Expand();
		}

		//-----------------------------------------------------------------------------	
		internal static void FillDataManagers(ObjectModelTreeNode docNode, MDocument document)
		{
			if (docNode == null)
				return;

			ObjectModelTreeNode dataManagersNode = new ObjectModelTreeNode(ControllerSources.DataManagersPropertyName, ControllerSources.DataManagersPropertyName, ImageLists.ObjectModelImageIndex.DataManagerNode);
            dataManagersNode.NodeFont = new Font(docNode.NodeFont, FontStyle.Bold);
            docNode.Nodes.Add(dataManagersNode);

			foreach (IComponent component in document.Components)
			{
				MDataManager dataManager = component as MDataManager;
				if (dataManager == null)
					continue;

				ObjectModelTreeNode dmNode = FillDataManagerNode(dataManager, null, ImageLists.ObjectModelImageIndex.DataManager);
				if (dmNode.Nodes.Count > 0)
					dataManagersNode.Nodes.Add(dmNode);
			}
			dataManagersNode.Expand();
		}

		//-----------------------------------------------------------------------------
		private void TsProperties_Click(object sender, EventArgs e)
		{
			OpenProperties?.Invoke(this, e);
		}

		//-----------------------------------------------------------------------------
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UpdateReferences();

			if (RefreshModel != null)
				RefreshModel(this, e);
		}

		//-----------------------------------------------------------------------------
		internal void UpdateObjectModel(IComponent component)
		{
			if (component == null)
				return;
			// refresh all
			if (component is DocumentController)
			{
				PopulateTree(component as DocumentController);
				treeDataManagers.SelectedNode = controllerNode;
				return;
			}

			if (component.Site == null || component.Site.Container == null)
				return;

			IContainer container = component.Site.Container;
			ObjectModelTreeNode containerNode = null;

			EasyBuilderComponent ebComponent = component as EasyBuilderComponent;
			ReferenceableComponent refComponent = component as ReferenceableComponent;
            if (refComponent != null)
            {
                FillBusinessObjects();
                containerNode = documentNode;
                if (containerNode == null)
                    return;
                if (container is MDocument || (ebComponent != null && ebComponent.Document == editor.Document))
                {
                    documentNode.Nodes.Clear();
                    FillDocumentNode(containerNode, editor.Document, null, null, null);
                }
                return;
            }
        }

		//-----------------------------------------------------------------------------
		private ObjectModelTreeNode FindNode(TreeNodeCollection nodes, object o)
		{
			foreach (ObjectModelTreeNode n in nodes)
			{
				if (n.Tag == o)
					return n;
				ObjectModelTreeNode found = FindNode(n.Nodes, o);
				if (found != null)
					return found;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		internal void SelectObject(object o)
		{
			IWindowWrapper ww = o as IWindowWrapper;
			if (ww != null)
				return;

			if (o == null)
			{
				this.treeDataManagers.SelectedNode = null;
				return;
			}
			ObjectModelTreeNode found = FindNode(treeDataManagers.Nodes, o);
			if (found == null)
			{
				IComponent component = o as IComponent;
				if (component != null && component.Site != null)
					SelectObject(component.Site.Container);

				return;

			}

			if (found != this.treeDataManagers.SelectedNode)
				this.treeDataManagers.SelectedNode = found;
		}

		//-----------------------------------------------------------------------------
		private void addDBTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (AddDbt != null)
				AddDbt(this, new AddDataManagerEventArgs(editor.Controller.Document.Batch ? DBTEditor.EB_LocalData : string.Empty, null));
		}

		//-----------------------------------------------------------------------------
		private void tsDelete_Click(object sender, EventArgs e)
		{
			if (this.treeDataManagers.SelectedNode != null)
			{
				if (DeleteObject != null)
					DeleteObject(this, new DeleteObjectEventArgs(this.treeDataManagers.SelectedNode.Tag as EasyBuilderComponent));
			}
		}

		//-----------------------------------------------------------------------------
		private void addHotLinkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (AddHotLink != null)
				AddHotLink(this, new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromTable, string.Empty));
		}

		//-----------------------------------------------------------------------------
		private void wrapExistingHotlinkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (AddHotLink != null)
				AddHotLink(this, new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromTemplate, string.Empty));
		}



		//-----------------------------------------------------------------------------
		private void addLocalFieldToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddFieldToDataModel(true);
		}

		//-----------------------------------------------------------------------------
		private void addFieldToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AddFieldToDataModel(false);
		}

		//-----------------------------------------------------------------------------
		private void AddFieldToDataModel(bool local)
		{
			if (AddField == null)
				return;

			Cursor = Cursors.WaitCursor;

			if (treeDataManagers == null || treeDataManagers.SelectedNode == null)
				return;

			IDataManager dataManager = treeDataManagers.SelectedNode.Tag as IDataManager;
			if (dataManager == null)
				return;

			try
			{
				AddField(this, new AddFieldEventArgs(dataManager, local));
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		//-----------------------------------------------------------------------------
		private void treeDataManagers_DragOver(object sender, DragEventArgs e)
		{
			Point p = treeDataManagers.PointToClient(new Point(e.X, e.Y));
			ObjectModelTreeNode currentItem = treeDataManagers.GetNodeAt(p.X, p.Y) as ObjectModelTreeNode;
			if (currentItem == null)
				return;

			//Non è consentito drag-drop-are un enumerativo sull'object model tree.
			EnumsDropObject enumsDropObject = e.Data.GetData(typeof(EnumsDropObject)) as EnumsDropObject;
			if (enumsDropObject != null)
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			DataModelDropObject ddo = e.Data.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (ddo != null)//sto spostando un oggetto all'interno dell'objectModel
			{
				//sto spostando un controllo grafico per modificare il taborder
				if (currentItem.Tag is EasyBuilderControl &&
					ddo.Component is EasyBuilderControl &&
					currentItem.Tag != ddo.Component &&
					((EasyBuilderControl)currentItem.Tag).TabStop &&
					((EasyBuilderControl)ddo.Component).TabStop &&
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

            // drag&drop sul master
            if (currentItem.Tag is MDBTMaster)
            {
                e.Effect = editor.Document.Batch ? DragDropEffects.None : DragDropEffects.Copy;
                return;

            }

            //drop di tabella su DBT
            if (currentItem.Tag.Equals(ControllerSources.DBTPropertyName))
            {
                e.Effect = editor.Document.Batch ? DragDropEffects.None : DragDropEffects.Copy;
                // il dbt master va solo sulle masterTable
                if (currentItem.Nodes.Count == 0)
                {
                    MSqlRecord rec = new MSqlRecord(sqlRecordName);
                    if (!rec.IsMasterTable)
                        e.Effect = DragDropEffects.None;
                }
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
		private void treeDataManagers_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Effect == DragDropEffects.None)
				return;

			Point p = treeDataManagers.PointToClient(new Point(e.X, e.Y));
			ObjectModelTreeNode targetNode = treeDataManagers.GetNodeAt(p.X, p.Y) as ObjectModelTreeNode;
			if (targetNode == null)
				return;

			DataModelDropObject ddo = e.Data.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (ddo != null)//sto spostando un oggetto all'interno dell'objectModel
			{
				if (ddo.Component is EasyBuilderControl)//sto spostando un controllo grafico per modificare il taborder
				{
					int newIndex = ((EasyBuilderControl)targetNode.Tag).TabOrder;
					int oldIndex = ((EasyBuilderControl)ddo.Component).TabOrder;
					((EasyBuilderControl)ddo.Component).TabOrder = newIndex;
					TBSite site = ((EasyBuilderControl)ddo.Component).Site as TBSite;
					if (site != null)
					{
						FormEditor formEditor = site.Editor as FormEditor;
						if (formEditor != null)
							formEditor.OnComponentPropertyChanged(((EasyBuilderControl)ddo.Component), ReflectionUtils.GetPropertyName(() => ((EasyBuilderControl)ddo.Component).TabOrder), oldIndex, newIndex);
					}
				}
				return;
			}

			//business objects
			ComponentDeclarationRequest declarationRequest = e.Data.GetData(typeof(ComponentDeclarationRequest)) as ComponentDeclarationRequest;
			if (
				declarationRequest != null &&
				DeclareComponent != null
				)
			{
				try
				{
					DeclareComponent(this, new DeclareComponentEventArgs(declarationRequest));
				}
				catch (Exception exc)
				{
					MessageBox.Show(exc.Message);
				}
				return;
			}

			//Hotlinks
			AddHotLinkEventArgs hotLinkEventArgs = e.Data.GetData(typeof(AddHotLinkEventArgs)) as AddHotLinkEventArgs;
			if (hotLinkEventArgs != null || targetNode.Tag.Equals(ControllerSources.HotLinksPropertyName))
			{
				AddHotLinkEventArgs hklArgs = null;
				string sqlRecordName = e.Data.GetData(typeof(string)) as string;
				if (!sqlRecordName.IsNullOrEmpty())
					hklArgs = new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromTable, sqlRecordName);
				else
					hklArgs = e.Data.GetData(typeof(AddHotLinkEventArgs)) as AddHotLinkEventArgs;

				if (hklArgs != null && AddHotLink != null)
					AddHotLink(this, hklArgs);

				return;
			}

			if (targetNode.Tag.Equals(ControllerSources.DBTPropertyName) || targetNode.Tag is MDBTMaster || targetNode.Tag is MDBTSlaveBuffered)
			{
				string sqlRecordName = e.Data.GetData(typeof(string)) as string;
				if (sqlRecordName.IsNullOrEmpty())
					return;

				if (AddDbt != null)
					AddDbt(this, new AddDataManagerEventArgs(sqlRecordName, targetNode.Tag as MDBTObject));

				return;
			}

			//drop di datamanager da module object model tree control
			AddDataManagerEventArgs dataManagerEventArgs = e.Data.GetData(typeof(AddDataManagerEventArgs)) as AddDataManagerEventArgs;
			if (dataManagerEventArgs != null)
			{
				if (AddDataManager != null)
					AddDataManager(this, dataManagerEventArgs);
				return;
			}
			//drop di tabella da database explorer
			if (targetNode.Tag.Equals(ControllerSources.DataManagersPropertyName))
			{
				string sqlRecordName = e.Data.GetData(typeof(string)) as string;
				if (sqlRecordName.IsNullOrEmpty())
					return;

				if (AddDataManager != null)
					AddDataManager(this, new AddDataManagerEventArgs(sqlRecordName, null));
				return;
			}
		}

		//-----------------------------------------------------------------------------
		private void EnableActions(TreeNode node)
		{
			addDBTToolStripMenuItem.Enabled = tsbAddNewDbt.Enabled = node != null && node.Tag != null && node.Tag.Equals(ControllerSources.DBTPropertyName);
			addDBTToolStripMenuItem.Visible = addDBTToolStripMenuItem.Enabled;
			addFieldToolStripMenuItem.Enabled = tsbAddField.Enabled = node != null && node.Tag != null && node.Tag is MDBTObject;
			if (editor.Controller != null && editor.Controller.Document.Batch)
				addFieldToolStripMenuItem.Enabled = false;
			addFieldToolStripMenuItem.Visible = addFieldToolStripMenuItem.Enabled;
			addLocalFieldToolStripMenuItem.Enabled = tsbAddLocalField.Enabled = node != null && node.Tag != null && node.Tag is IDataManager;
			addLocalFieldToolStripMenuItem.Visible = addLocalFieldToolStripMenuItem.Enabled;
			addHotLinkToolStripMenuItem.Enabled = wrapExistingHotlinkToolStripMenuItem.Enabled = tsbAddNewHotlink.Enabled = tsbAddHotlinkFromTemplate.Enabled =
				node != null && node.Tag != null && node.Tag.Equals(ControllerSources.HotLinksPropertyName);
			addHotLinkToolStripMenuItem.Visible = addHotLinkToolStripMenuItem.Enabled;
			EasyBuilderComponent ebComponent = node == null ? null : node.Tag as EasyBuilderComponent;


			tsDelete.Enabled = tsbDelete.Enabled = (
					(
						(ebComponent as DocumentController) == null &&
						(ebComponent as MDocument) == null &&
						(ebComponent as MView) == null &&
						ebComponent != null &&
						ebComponent.CanBeDeleted
					)
				);
		}

		//-----------------------------------------------------------------------------
		private void useExistingHotlinkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (AddHotLink != null)
				AddHotLink(this, new AddHotLinkEventArgs(AddHotLinkEventArgs.RequestType.FromDocument, string.Empty));
		}


		//-----------------------------------------------------------------------------
		private void UpdateReferences()
		{
			if (treeDataManagers.SelectedNode == null || treeDataManagers.SelectedNode.Tag == null)
				return;

			// current single selection
			ReferenceableComponent selComponent = treeDataManagers.SelectedNode.Tag as ReferenceableComponent;
			if (selComponent != null)
			{
				UpdateReference(selComponent, true);
				return;
			}

			// all references refresh
			if (treeDataManagers.SelectedNode.Tag.ToString() == ControllerSources.BusinessObjectsPropertyName)
			{
				int nLast = treeDataManagers.SelectedNode.Nodes.Count - 1;
				int nCurrent = 0;
				foreach (ObjectModelTreeNode node in treeDataManagers.SelectedNode.Nodes)
				{
					ReferenceableComponent refComponent = node.Tag as ReferenceableComponent;
					if (refComponent != null)
						UpdateReference(refComponent, nCurrent == nLast);
					nCurrent++;
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void UpdateReference(ReferenceableComponent refComponent, bool recalculateReferences)
		{
			if (refComponent == null)
				return;

			ComponentDeclarationRequest request = new ComponentDeclarationRequest
				(
					recalculateReferences ? ComponentDeclarationRequest.Action.UpdateWithReferences : ComponentDeclarationRequest.Action.Update,
					refComponent
				);
			if (request != null && DeclareComponent != null)
				DeclareComponent(this, new DeclareComponentEventArgs(request));
		}

		//-----------------------------------------------------------------------------
		private void treeDataManagers_DrawNode(object sender, DrawTreeNodeEventArgs e)
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
		private void treeDataManagers_AfterSelect(object sender, TreeViewEventArgs e)
		{
			EnableActions(e.Node);
			
			//if (treeDataManagers.SelectedNode != null && treeDataManagers.SelectedNode.Tag is IComponent)
			//	selectionService.SetSelectedComponents(new Object[] { treeDataManagers.SelectedNode.Tag });


			//if ((e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse) && e.Node != null && e.Node.Tag is IComponent)
			selectionService.SetSelectedComponents(new Object[] { e.Node.Tag });

		}

		//-----------------------------------------------------------------------------
		private void treeDataManagers_ItemDrag(object sender, ItemDragEventArgs e)
		{
			ObjectModelTreeNode treenode = (e.Item as ObjectModelTreeNode);
			IComponent component = treenode.Tag as IComponent;
			if (component == null || component.Site == null)
				return;

			DoDragDrop(new DataModelDropObject(
				component,
				treeDataManagers.SelectedNode.Text
				), DragDropEffects.Copy | DragDropEffects.Move);
		}

		//-----------------------------------------------------------------------------
		private void treeDataManagers_MouseDown(object sender, MouseEventArgs e)
		{
			treeDataManagers.SelectedNode = treeDataManagers.GetNodeAt(e.Location);

			if (treeDataManagers.SelectedNode == null)
				selectionService.SetSelectedComponents(null);
		}
	}

	//=========================================================================
	internal class ObjectModelTreeNode : TreeNode
	{
		private ImageLists.ObjectModelImageIndex nodeType;

		//-----------------------------------------------------------------------------
		public ObjectModelTreeNode(string text, object tag, ImageLists.ObjectModelImageIndex nodeType)
			: base(text)
		{
			this.Tag = tag;
			this.Name = text;
            this.nodeType = nodeType;
            CheckInvalid();
        }

		//-----------------------------------------------------------------------------
		public ObjectModelTreeNode(string text, EasyBuilderComponent component)
			: base(text)
		{
			this.Name = text;
			this.Tag = component;
            CheckInvalid();
        }

        //-----------------------------------------------------------------------------
        private void CheckInvalid()
        {
            EasyBuilderComponent comp = Tag as EasyBuilderComponent;
            this.ImageIndex = comp != null && !comp.IsValidComponent ? (int)ImageLists.ObjectModelImageIndex.InvalidObject : (int) nodeType;
            this.SelectedImageIndex = this.ImageIndex;
            if (comp != null && !comp.IsValidComponent)
            {
                nodeType = (ImageLists.ObjectModelImageIndex) this.ImageIndex;
                this.ToolTipText = Resources.InvalidObjectModelItem;
            }
        }
	}
}
