using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
 
    //=============================================================================================
	public class UITree : RadTreeView, IUIContainer
	{
        TBCUI cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		public event EventHandler<UITreeNodeEventArgs> UINodeFormatting;
		public event EventHandler<UITreeNodeEventArgs> UISelectedNodeChanged;
		public event EventHandler<UITreeCancelEventArgs> BeforeSelect;

		[Browsable(false)]
		[Obsolete("do not use LineStyle, use UITreeLineStyle instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new TreeLineStyle LineStyle { get { return base.LineStyle; } set { base.LineStyle = value; } }

		public UITreeLineStyle UITreeLineStyle { get { return (UITreeLineStyle)base.LineStyle; } set { base.LineStyle = (Telerik.WinControls.UI.TreeLineStyle)value; } }

		//---------------------------------------------------------------------------
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RadTreeNodeCollection UINodes
		{
			get { return base.Nodes; }
		}

		[Browsable(false)]
		[Obsolete("do not use Nodes, use UINodes instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new RadTreeNodeCollection Nodes
		{
			get { return base.Nodes; }
		}

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		//---------------------------------------------------------------------------
		public UITree()
		{
            cui = CreateController(); 
	        ThemeClassName = typeof(RadTreeView).ToString();
			UITreeLineStyle = UITreeLineStyle.Solid;
			HideSelection = false;
			ShowLines = true;
			AttachEvents();
		}

        //---------------------------------------------------------------------------
        protected virtual TBCUI CreateController()
        {
            return new TBCUI(this, Interfaces.NameSpaceObjectType.Control);
        }

		//---------------------------------------------------------------------------
		protected override RadTreeViewElement CreateTreeViewElement()
		{
			return new UITreeViewElement();
		}

		//---------------------------------------------------------------------------
		protected virtual void OnNodeFormatting(UITreeNodeEventArgs e)
		{
			if (UINodeFormatting != null)
			{
				UINodeFormatting(this, e);
			}
		}

		//---------------------------------------------------------------------------
		protected virtual void OnSelectedNodeChanged(UITreeNodeEventArgs e)
		{
			if (UISelectedNodeChanged != null)
			{
				UISelectedNodeChanged(this, e);
			}
		}

		//---------------------------------------------------------------------------
		private void OnBeforeSelect(object sender, UITreeCancelEventArgs e)
		{
			if (BeforeSelect != null)
				BeforeSelect(sender, e);
		}

		//-------------------------------------------------------------------------
		void UITree_SelectedNodeChanged(object sender, RadTreeViewEventArgs e)
		{
			UITreeNodeEventArgs uie = new UITreeNodeEventArgs(e.Node);

			OnSelectedNodeChanged(uie);
		}

		//-------------------------------------------------------------------------
		private void UITree_NodeFormatting(object sender, TreeNodeFormattingEventArgs e)
		{
			UITreeNodeEventArgs uie = new UITreeNodeEventArgs(e.Node);

			OnNodeFormatting(uie);
		}

		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			DetachEvents();
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }

		//---------------------------------------------------------------------------
		void InnerControl_SelectedNodeChanging(object sender, RadTreeViewCancelEventArgs e)
		{
			OnBeforeSelect(sender, new UITreeCancelEventArgs(e));
		}

		//-------------------------------------------------------------------------
		public System.Collections.IList ChildControls
		{
			get { return base.Controls; }
		}

		//-----------------------------------------------------------------------------------------
		private void AttachEvents()
		{
			this.SelectedNodeChanging += new RadTreeView.RadTreeViewCancelEventHandler(InnerControl_SelectedNodeChanging);
			this.NodeFormatting += new TreeNodeFormattingEventHandler(UITree_NodeFormatting);
			this.SelectedNodeChanged += new RadTreeViewEventHandler(UITree_SelectedNodeChanged);
		}

		//-----------------------------------------------------------------------------------------
		private void DetachEvents()
		{
			this.NodeFormatting -= new TreeNodeFormattingEventHandler(UITree_NodeFormatting);
			this.SelectedNodeChanged -= new RadTreeViewEventHandler(UITree_SelectedNodeChanged);
			this.SelectedNodeChanging -= new RadTreeView.RadTreeViewCancelEventHandler(InnerControl_SelectedNodeChanging);
		}
	}
}
