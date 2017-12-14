using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Forms.Controls.UIDBTTree;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms
{
    //================================================================================================================
    /// <summary>
    /// TBWFCUIGrid
    /// </summary>
    internal class TBWFCUIDBTTree : TBWFCUIControl
    {
        private UIDBTTree tree;
       
        private Dictionary<string, Type> controlMap = new Dictionary<string, Type>();
        private List<string> expandedNodes = new List<string>();

        MDBTSlaveBuffered DBTSlaveBuffered { get { return DataBinding != null ? DataBinding.Data as MDBTSlaveBuffered : null; } }

        //-------------------------------------------------------------------------
        public void AddTbBinding(UIDBTTree tree, MDBTSlaveBuffered dataManager)
        {
            DataBinding = new DBTDataBinding(dataManager);
            DBTList<DBTTreeItem> dataSource = new DBTList<DBTTreeItem>(dataManager, this);
            tree.DataSource = dataSource;
            tree.ExpandAll();
            this.tree = tree;
            this.tree.Disposed += new EventHandler(Tree_Disposed);
            this.tree.SelectedNodeChanged += new RadTreeView.RadTreeViewEventHandler(tree_SelectedNodeChanged);
            dataSource.ListChanged += new ListChangedEventHandler(dataSource_ListChanged);
            AdjustMemberMapping();
        }

        //-------------------------------------------------------------------------
        void dataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            tree.ClearPanel();
            tree.UpdateToolbar();
        }

        //-------------------------------------------------------------------------
        void tree_SelectedNodeChanged(object sender, RadTreeViewEventArgs e)
        {  
            tree.ClearPanel(); 
            tree.UpdateToolbar();
            
            TBBindingListItem item = e.Node.DataBoundItem as TBBindingListItem;
            if (item == null)
                return;

            if (item.Key is MSqlRecord)
            {
                MSqlRecord rec = (MSqlRecord)item.Key;
                MDBTObject dbt = (MDBTObject)rec.ParentComponent;
                Type ctrlType;
                if (controlMap.TryGetValue(rec.NameSpace.ToString(), out ctrlType))
                {
                    UIUserControl ctrl = (UIUserControl)Activator.CreateInstance(ctrlType);
                    tree.AddControlToPanel(ctrl);
                    ctrl.BindControls(rec);
                }
                return;
            }

            if (item.Key is MDBTObject)
            {
                MDBTObject dbt = (MDBTObject)item.Key;
                Type ctrlType;
                if (controlMap.TryGetValue(dbt.Namespace.ToString(), out ctrlType))
                {
                    UIUserControl ctrl = (UIUserControl)Activator.CreateInstance(ctrlType);
                    tree.AddControlToPanel(ctrl);
                    ctrl.BindControls(dbt);
                }
                return;
            }
        }

        //-------------------------------------------------------------------------
        public void AddTbBinding<T>(MDBTSlaveBuffered dataManager) where T : UIUserControl
        {
            controlMap.Add(dataManager.Namespace.ToString(), typeof(T));
        }

        //-------------------------------------------------------------------------
        public void AddTbBinding<T>(MSqlRecord record) where T : UIUserControl
        {
            controlMap.Add(record.NameSpace.ToString(), typeof(T));
        }

        //-------------------------------------------------------------------------
        void Tree_Disposed(object sender, EventArgs e)
        {
            this.tree.Disposed -= new EventHandler(Tree_Disposed);
            this.tree.SelectedNodeChanged -= new RadTreeView.RadTreeViewEventHandler(tree_SelectedNodeChanged);
           
            DBTList<DBTTreeItem> list = tree.DataSource as DBTList<DBTTreeItem>;
            if (list != null)
            {
                list.ListChanged += new ListChangedEventHandler(dataSource_ListChanged);
                list.Dispose();
            }
        }


        //---------------------------------------------------------------------------
        public void AdjustMemberMapping()
        {
            StringBuilder desc = new StringBuilder();
            StringBuilder child = new StringBuilder();
            int depth = ((DBTList<DBTTreeItem>)tree.DataSource).Depth;
            for (int i = 0; i < depth; i++)
            {
                if (desc.Length > 0)
                {
                    desc.Append('\\');
                    child.Append('\\');
                }
                desc.Append("Description");
                child.Append("Children");
            }

            tree.BeginInit();
            if (depth > 0)
            {
                tree.DisplayMember = desc.ToString();
                tree.ChildMember = child.ToString();
            }
            tree.EndInit(); //per forzare un refresh del tree sulla base del binding provider
            ExpandNodes(tree.UINodes);
        }


        //---------------------------------------------------------------------------
        public TBWFCUIDBTTree(UIDBTTree tree)
            : base(tree, Interfaces.NameSpaceObjectType.Control)
        {

        }

        //---------------------------------------------------------------------------
        protected override void DocumentFormModeChanged(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            IMAbstractFormDoc doc = sender as IMAbstractFormDoc;
            if (doc == null)
                return;

            GetTreeStatus(tree.UINodes);

            tree.UpdateToolbar();
        }

        //---------------------------------------------------------------------------
        private void ExpandNodes(RadTreeNodeCollection nodes)
        {
            if (nodes.Count == 0)
                return;

            foreach (UITreeNode node in nodes)
            {
                if (expandedNodes.Contains(node.Text))
                {
                    node.Expanded = true;
                }

                if (node.UINodes.Count > 0)
                {
                    ExpandNodes(node.UINodes);
                }
            }
        }

        //---------------------------------------------------------------------------
        internal void GetTreeStatus(RadTreeNodeCollection nodes)
        {
            expandedNodes.Clear();
            GetExpandedNodes(nodes);
        }

        //---------------------------------------------------------------------------
        internal void GetExpandedNodes(RadTreeNodeCollection nodes)
        {
            if (nodes.Count == 0)
                return;

            foreach (UITreeNode node in nodes)
            {
                if (node.Expanded)
                {
                    expandedNodes.Add(node.Text);
                }

                if (node.UINodes.Count > 0)
                {
                    GetExpandedNodes(node.UINodes);
                }
            }
        }

        //---------------------------------------------------------------------------
        internal void AddNode()
        {
            if (tree.SelectedNode != null)
            {
                TBBindingListItem item = tree.SelectedNode.DataBoundItem as TBBindingListItem;
                if (item == null)
                    return;

                if (item.Key is MSqlRecord)
                    return;

                if (item.Key is MDBTSlaveBuffered)
                {

                    GetTreeStatus(tree.UINodes);
              
                    MDBTSlaveBuffered dbt = (MDBTSlaveBuffered)item.Key;
                    dbt.AddRecord();

                    AdjustMemberMapping();
                    return;
                }
            }
        }

        //---------------------------------------------------------------------------
        internal void DeleteNode()
        {
            if (tree.SelectedNode != null)
            {
                TBBindingListItem item = tree.SelectedNode.DataBoundItem as TBBindingListItem;
                if (item == null)
                    return;

                MSqlRecord rec = item.Key as MSqlRecord;
                if (rec != null)
                {
                    MDBTSlaveBuffered dbt = rec.ParentComponent as MDBTSlaveBuffered;
                    if (dbt != null)
                    {
                        GetTreeStatus(tree.UINodes);
                        dbt.SetCurrentRowByRecord(rec);
                        dbt.DeleteRow(dbt.CurrentRow);
                        AdjustMemberMapping();
                        return;
                    }
                    
                }
            }
        }

        //---------------------------------------------------------------------------
        internal void ToggleNode()
        {
            if (tree.SelectedNode != null)
                tree.SelectedNode.Toggle();
        }
    }
    //=========================================================================
    class DBTList<T> : TBBindingList<DBTTreeItem>, IDisposable where T : DBTTreeItem
    {
        private MDBTSlaveBuffered dataManager;
        private TBWFCUIDBTTree treeCUI;

        //--------------------------------------------------------------------
        public int Depth
        {
            get
            {
                int d = 0;
                foreach (DBTTreeItem item in this)
                    d = Math.Max(d, (1 + item.Children.Depth));
                return d;
            }
        }

        //--------------------------------------------------------------------
        public DBTList(MDBTSlaveBuffered dataManager, TBWFCUIDBTTree treeCUI)
        {
            this.treeCUI = treeCUI;
            this.dataManager = dataManager;
            if (dataManager == null)
                return;

            for (int i = 0; i < dataManager.Rows.Count; i++)
            {
                MSqlRecord rec = (MSqlRecord)dataManager.Rows[i];
                AddItem(i, rec);
            }

            dataManager.RecordAdded += new EventHandler<RowEventArgs>(dataManager_RecordAdded);
           
            dataManager.RemovingRecord += new EventHandler<RowEventArgs>(dataManager_RemovingRecord); //rimozione righe quando viene rieseguita la query
            dataManager.DeletingRow += new EventHandler<RowEventArgs>(dataManager_RemovingRecord); //cancellazione riga da parte di utente
        }

        //--------------------------------------------------------------------
        public void Dispose()
        {
            foreach (DBTTreeItem item in this)
                item.Dispose();
            if (dataManager == null)
                return;
            dataManager.RecordAdded -= new EventHandler<RowEventArgs>(dataManager_RecordAdded);
            dataManager.RemovingRecord -= new EventHandler<RowEventArgs>(dataManager_RemovingRecord);
            dataManager.DeletingRow -= new EventHandler<RowEventArgs>(dataManager_RemovingRecord);
        }

        //--------------------------------------------------------------------
        void dataManager_RemovingRecord(object sender, RowEventArgs e)
        {
            foreach (DBTTreeItem item in this)
            {
                if (item.Key == e.Record)
                {
                    Remove(item);
                    item.Dispose();
                    return;
                }
            }
        }

        //--------------------------------------------------------------------
        void dataManager_RecordAdded(object sender, RowEventArgs e)
        {
            AddItem(e);
        }

        //--------------------------------------------------------------------
        private void AddItem(RowEventArgs e)
        {
            AddItem((int)e.RowNumber, (MSqlRecord)e.Record);
            treeCUI.AdjustMemberMapping();
        }

        //--------------------------------------------------------------------
        private void AddItem(int i, MSqlRecord rec)
        {
            DBTTreeItem item = new DBTTreeItem(rec, rec.RecordDescription);
            this.Add(item);

            item.Children = new DBTList<DBTTreeItem>(null, treeCUI);
            foreach (MDBTSlave slave in dataManager.SlavePrototypes)
            {
                MDBTSlaveBuffered dbtSlaveBuffered = dataManager.GetDBTSlave(slave.Name, i) as MDBTSlaveBuffered;
                DBTTreeItem dbtItem = new DBTTreeItem(dbtSlaveBuffered, dbtSlaveBuffered.Title);
                item.Children.Add(dbtItem);
                dbtItem.Children = new DBTList<DBTTreeItem>(dbtSlaveBuffered, treeCUI);
            }
        }


    }
    //=========================================================================
    class DBTTreeItem : TBBindingListItem, IDisposable
    {
        DBTList<DBTTreeItem> children = null;

        //--------------------------------------------------------------------
        public DBTTreeItem(object key, string description)
            : base(key, description)
        { }

        //---------------------------------------------------------------------
        public virtual DBTList<DBTTreeItem> Children
        {
            get { return children; }
            set { children = value; }
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            if (children != null)
                children.Dispose();
        }
    }
}
