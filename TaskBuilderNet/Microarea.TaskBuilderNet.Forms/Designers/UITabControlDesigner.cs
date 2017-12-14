using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.TaskBuilderNet.Forms.Controls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Designers
{
	public class UITabControlDesigner : ParentControlDesigner
    {
        private RadPageView pageView;
		private UITabControl tabControl;
		private ISelectionService selectionService;
		private IDesignerHost designerHost;
		private IComponentChangeService changeService;
		
		//-------------------------------------------------------------------------
		public override void Initialize(IComponent component)
        {
            base.Initialize(component);

			this.tabControl = (component as UITabControl);
			this.pageView = tabControl;
            this.pageView.PageExpanded += this.OnView_PageExpanded;
			this.pageView.PageCollapsed += this.OnView_PageCollapsed;
			this.pageView.ViewModeChanged += OnView_ViewModeChanged;
			this.pageView.SelectedPageChanged += OnView_SelectedPageChanged;
			this.pageView.PageAdded += OnView_PageAdded;
			this.selectionService = this.GetService(typeof(ISelectionService)) as ISelectionService;
			this.designerHost = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
			this.changeService = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            this.selectionService.SelectionChanged += OnSelectionService_SelectionChanged;
        }

		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				this.pageView.PageExpanded -= this.OnView_PageExpanded;
				this.pageView.PageCollapsed -= this.OnView_PageCollapsed;
                this.selectionService.SelectionChanged -= OnSelectionService_SelectionChanged;
				this.pageView.ViewModeChanged -= OnView_ViewModeChanged;
				this.pageView.SelectedPageChanged -= OnView_SelectedPageChanged;
				this.pageView.PageAdded -= OnView_PageAdded;
            }

            base.Dispose(disposing);
        }

		//-------------------------------------------------------------------------
		public override bool CanParent(Control control)
        {
            return false;
        }

		//-------------------------------------------------------------------------
		public override bool CanParent(ControlDesigner controlDesigner)
        {
            return false;
        }

		//-------------------------------------------------------------------------
		public override DesignerVerbCollection Verbs
        {
            get
            {
                DesignerVerbCollection verbs = base.Verbs;
                int count = verbs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (verbs[i].Text == "Add Group Header" || verbs[i].Text == "Add Page"
                        || verbs[i].Text == "Remove Page")
                    {
                        verbs.RemoveAt(i);
                        count--;
                        i--;
                    }
                }


                verbs.Add(new DesignerVerb("Add TabPage", OnAddPage));
				verbs.Add(new DesignerVerb("Remove TabPage", OnRemovePage));

                if (this.pageView.ViewMode == PageViewMode.Backstage)
                {
                    verbs.Add(new DesignerVerb("Add Group Header", OnAddGroup));
                } 

                return verbs;
            }
        }

		//-------------------------------------------------------------------------
		//protected override RadControlDesignerLiteActionList CreateActionList()
		//{
		//    switch (this.tabControl.ViewMode)
		//    {
		//        case PageViewMode.Stack:
		//            return new RadPageViewStackActionList(this);
		//        case PageViewMode.Outlook:
		//            return new RadPageViewOutlookActionList(this);
		//        case PageViewMode.ExplorerBar:
		//            return new RadPageViewExplorerBarActionList(this);
		//        case PageViewMode.Backstage:
		//            return new RadPageViewBackstageActionList(this);
		//        default:
		//            return new RadPageViewStripActionList(this);
		//    }
		//}

		//-------------------------------------------------------------------------
		protected override bool GetHitTest(Point point)
        {
			if (this.selectionService.GetComponentSelected(tabControl))
            {
                Point client = this.pageView.PointToClient(point);

                if (this.ItemContains(client))
                {
                    return true;
                }

				if (this.pageView.ViewMode == PageViewMode.Strip &&
                    this.HitTestStripButtons(client))
                {
                    return true;
                }

				if (this.pageView.ViewMode == PageViewMode.ExplorerBar &&
                    this.HitTestExplorerBarScrollbar(client))
                {
                    return true;
                }
			}

            return base.GetHitTest(point);
        }

		//-------------------------------------------------------------------------
		private bool HitTestExplorerBarScrollbar(Point clientPoint)
        {
			RadPageViewExplorerBarElement explorerBarElement = this.pageView.ViewElement as RadPageViewExplorerBarElement;
            if (explorerBarElement == null)
                return false;
            RadScrollBarElement scrollBarElement = explorerBarElement.Scrollbar;
            return scrollBarElement.ControlBoundingRectangle.Contains(clientPoint);
        }

		//-------------------------------------------------------------------------
		private bool HitTestStripButtons(Point client)
        {
			RadPageViewStripElement strip = this.pageView.ViewElement as RadPageViewStripElement;
            StripViewButtons buttons = strip.HitTestButtons(client);

            return buttons == StripViewButtons.LeftScroll || buttons == StripViewButtons.RightScroll;
        }

		//-------------------------------------------------------------------------
		private bool ItemContains(Point client)
        {
			return this.pageView.ViewElement.ItemFromPoint(client) != null;
        }

		//-------------------------------------------------------------------------
		private void OnView_PageCollapsed(object sender, RadPageViewEventArgs e)
        {
            this.changeService.OnComponentChanged(e.Page, null, null, null);
        }

		//-------------------------------------------------------------------------
		private void OnView_PageExpanded(object sender, RadPageViewEventArgs e)
        {
            this.changeService.OnComponentChanged(e.Page, null, null, null);
        }

		//-------------------------------------------------------------------------
		private void OnView_PageAdded(object sender, RadPageViewEventArgs e)
        {
            //ParentControlDesigner sets the Visible property to true when initializing each Page, we need to roll it back to false
			if (e.Page != this.pageView.SelectedPage)
            {
                e.Page.Visible = false;
            }
        }

		//-------------------------------------------------------------------------
		internal void OnAddPage(object sender, EventArgs e)
        {
            IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                return;
            }

			UITabPage page = host.CreateComponent(typeof(UITabPage)) as UITabPage;
			tabControl.UIPages.Add(page);  //this.pageView.Pages.Add(page);
            page.Text = page.Name;
			
			this.pageView.RootElement.UpdateLayout();
			tabControl.UISelectedPage = page; //this.pageView.SelectedPage = page;
        }

		//-------------------------------------------------------------------------
		internal void OnAddGroup(object sender, EventArgs e)
        {
            IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                return;
            }

            RadPageViewItemPage page = host.CreateComponent(typeof(RadPageViewItemPage)) as RadPageViewItemPage;
			this.pageView.Pages.Add(page);
			page.ItemType = PageViewItemType.GroupHeaderItem;
			page.Text = page.Name;

			this.pageView.RootElement.UpdateLayout();
            
        }

		//-------------------------------------------------------------------------
		internal void OnRemovePage(object sender, EventArgs e)
        {
            UITabPage page = tabControl.UISelectedPage as UITabPage;
            if (page == null)
            {
                return;
            }

            this.designerHost.DestroyComponent(page);
			this.selectionService.SetSelectedComponents(new IComponent[] { this.pageView }, SelectionTypes.Replace);
        }

		//-------------------------------------------------------------------------
		private void OnView_SelectedPageChanged(object sender, EventArgs e)
        {
            try
            {
                this.changeService.OnComponentChanged(this.pageView, null, null, null);

				if (this.pageView.SelectedPage is RadPageViewItemPage)
                {
					this.pageView.SelectedPage.Bounds = Rectangle.Empty;
					this.selectionService.SetSelectedComponents(new object[] { this.pageView.SelectedPage });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Message");
            }
        }

		//-------------------------------------------------------------------------
		private void OnView_ViewModeChanged(object sender, RadPageViewModeEventArgs e)
        {
            DesignerActionUIService actionUIService = base.Component.Site.GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
            if (actionUIService != null)
            {
                actionUIService.HideUI(base.Component);
            }
            //this.ResetActionList();

			this.selectionService.SetSelectedComponents(new object[0]);
			this.selectionService.SetSelectedComponents(new object[] { this.pageView });
        }

		//-------------------------------------------------------------------------
		private void OnSelectionService_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
				UITabPage selectedPage = tabControl.UISelectedPage as UITabPage;
				if (selectedPage == null || this.selectionService.GetComponentSelected(selectedPage))
                {
                    return;
                }

				UITabPage newSelectedPage = null;
				foreach (object selectedComponent in this.selectionService.GetSelectedComponents())
                {
					UITabControl tabber = selectedComponent as UITabControl;
					if (tabber == null)
						continue;

					UITabPage page = tabber.UISelectedPage as UITabPage;
                    if (page != null && page.Owner == this.pageView)
                    {
                        newSelectedPage = page;
                        break;
                    }
                }

                if (newSelectedPage != null)
                {
					tabControl.UISelectedPage = newSelectedPage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

    }
}
