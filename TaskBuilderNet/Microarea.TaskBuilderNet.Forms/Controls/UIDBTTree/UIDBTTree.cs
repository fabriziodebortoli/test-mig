using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.TaskBuilderNet.Forms.Controls.UIDBTTree
{
    public class UIDBTTree : UITree
    {
        private UIPanel                 viewPanel;
        private UIToolbar               toolbar;
        private ResizeExtender          extender;

        [Browsable(false)]
        public UIToolbar Toolbar { get { return toolbar; } }

        private TBWFCUIDBTTree DBTTreeCUI { get { return CUI as TBWFCUIDBTTree; } }

        //---------------------------------------------------------------------
        public UIDBTTree()
        {
            AddPanel();
            AddToolbar();

            System.Windows.Forms.ImageList imagelist = new System.Windows.Forms.ImageList();
            imagelist.Images.Add(Resources.TreeEditDBT);
            imagelist.Images.Add(Resources.TreeEditRecord);
            ImageList = imagelist;

            Layout += new LayoutEventHandler(UIDBTTree_Layout);
            SizeChanged += new EventHandler(UIDBTTree_SizeChanged);
            UINodeFormatting += new EventHandler<UITreeNodeEventArgs>(UIDBTTree_UINodeFormatting);
       }

        
        
        //---------------------------------------------------------------------------
        void UIDBTTree_SizeChanged(object sender, EventArgs e)
        {
            AdjustLayout();
        }

        //---------------------------------------------------------------------------
        private void AdjustLayout()
        {
            AlignPanel();
            ResizeToolbar();
        }

        //---------------------------------------------------------------------------
        void UIDBTTree_Layout(object sender, LayoutEventArgs e)
        {
            AdjustLayout();
        }

        //---------------------------------------------------------------------------
        protected override TBCUI CreateController()
        {
            return new TBWFCUIDBTTree(this);
        }

        //-------------------------------------------------------------------------
        internal void UpdateToolbar()
        {
            toolbar.ApplyEnableConditions();
        }

        //-------------------------------------------------------------------------
        void UIDBTTree_UINodeFormatting(object sender, UITreeNodeEventArgs e)
        {
            TBBindingListItem item = e.UiTreeNode.DataBoundItem as TBBindingListItem;
            if (item == null)
                return;
            if (item.Key is MSqlRecord)
            {
                e.UiTreeNode.ImageIndex = 1;
            }
            if (item.Key is MDBTObject)
            {
                e.UiTreeNode.ImageIndex = 0;
            }
        }

        //-------------------------------------------------------------------------
        private void AddPanel()
        {
            viewPanel = new UIPanel();
            viewPanel.Width = 0;
            AlignPanel();
            Controls.Add(viewPanel);
      
            extender = new ResizeExtender();
            extender.ResizableControl = viewPanel;
            extender.ResizeBorder = ResizeExtender.ResizeBorderKind.Left;
        }

        //-------------------------------------------------------------------------
        private void AddToolbar()
        {
            toolbar = new UIToolbar();

            SizeF positionOffset = TreeViewElement.PositionOffset;
            positionOffset.Height = 50;
            TreeViewElement.PositionOffset = positionOffset;

            ResizeToolbar();

            Controls.Add(toolbar);

            toolbar.AddButton("New", Resources.DBTTree_AddNode, (sender, args) => { DBTTreeCUI.AddNode(); }, () => { return EnableToolbarButtonCondition<MDBTObject>(); }, Resources.ToolbarGrid_NewRow);
            toolbar.AddButton("Delete", Resources.DBTTree_DeleteNode, (sender, args) => { DBTTreeCUI.DeleteNode(); }, () => { return EnableToolbarButtonCondition<MSqlRecord>(); }, Resources.ToolbarGrid_DeleteRow);
            toolbar.AddButton("Toggle", Resources.DBTTree_ToggleNode, (sender, args) => { DBTTreeCUI.ToggleNode(); }, () => { return true; }, Resources.TreeEditToggle);
        }

        //Abilita il bottone della toolbar se contiene un item di tipo T
        //-------------------------------------------------------------------------
        internal bool  EnableToolbarButtonCondition<T>()
        {
            if (DBTTreeCUI.Document != null && DBTTreeCUI.Document.FormMode == FormModeType.Browse)
                return false;

            if (this.SelectedNode == null)
                return false;
            
            TBBindingListItem item = this.SelectedNode.DataBoundItem as TBBindingListItem;
            if (item == null)
                return false;

            return item.Key is T;
        }

        //-------------------------------------------------------------------------
        private void AlignPanel()
        {
            viewPanel.Height = Height;
            viewPanel.Left = Width - viewPanel.Width;
        }

        //-------------------------------------------------------------------------
        private void ResizeToolbar()
        {
            toolbar.Width = Width - viewPanel.Width;
        }

        //---------------------------------------------------------------------
        public void AddTbBinding(MDBTSlaveBuffered dataManager)
        {
            DBTTreeCUI.AddTbBinding(this, dataManager);
        }

        //---------------------------------------------------------------------
        public void AddTbBinding<T>(MDBTSlaveBuffered dataManager) where T : UIUserControl
        {
            DBTTreeCUI.AddTbBinding<T>(dataManager);
        }

        //---------------------------------------------------------------------
        public void AddTbBinding<T>(MSqlRecord record) where T : UIUserControl
        {
            DBTTreeCUI.AddTbBinding<T>(record);
        }

        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Layout -= new LayoutEventHandler(UIDBTTree_Layout);
            SizeChanged -= new EventHandler(UIDBTTree_SizeChanged);
            UINodeFormatting -= new EventHandler<UITreeNodeEventArgs>(UIDBTTree_UINodeFormatting);
        }

        //---------------------------------------------------------------------
        internal void ClearPanel()
        {
            foreach (Control c in viewPanel.Controls)
                c.Dispose();
            
            viewPanel.Controls.Clear();
            viewPanel.Width = 0;

            AdjustLayout();
        }

        //---------------------------------------------------------------------
        internal void AddControlToPanel(UIUserControl ctrl)
        {
            viewPanel.Controls.Add(ctrl);
            viewPanel.Width = ctrl.Width;
            ctrl.Dock = DockStyle.Fill;
        }
    }
}
