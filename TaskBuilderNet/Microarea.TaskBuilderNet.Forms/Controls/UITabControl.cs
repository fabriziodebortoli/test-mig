using System;
using System.ComponentModel;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Forms.Designers;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//================================================================================================================
	[ToolboxItem(true)]
	[Designer(typeof(UITabControlDesigner))]
	[DefaultProperty("Pages")]
	[DefaultEvent("UISelectedPageChanged")]
	public class UITabControl : RadPageView, IUIContainer
	{
        TBWFCUITabber cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		TemplateItemCollection<RadPageViewPageCollection, UITabPage> internalPages;

		public event EventHandler<EventArgs> UISelectedPageChanged;

		[Browsable(false)]
		[Obsolete("do not use SelectedPageChanged, use UISelectedPageChanged instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler<EventArgs> SelectedPageChanged;

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection<RadPageViewPageCollection, UITabPage> UIPages
		{
			get { return internalPages; }
			set { internalPages = value; }
		}


		[Browsable(false)]
		[Obsolete("do not use Items, use UIPages instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new RadPageViewPageCollection Pages
		{
			get { return base.Pages; }
		}

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		[Browsable(false)]
		[Obsolete("do not use Controls, use UISelectedPage instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new RadPageViewPage SelectedPage
		{
			get { return base.SelectedPage; }
		}

		//-------------------------------------------------------------------------
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public UITabPage UISelectedPage 
		{
			get { return base.SelectedPage as UITabPage; }
			set { base.SelectedPage = value; }
		}
	
	    //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return UIPages; }
        }

		//-------------------------------------------------------------------------
		public UITabControl()
        {
            cui = new TBWFCUITabber(this); 
			internalPages = new TemplateItemCollection<RadPageViewPageCollection, UITabPage>(base.Pages);
			AttachEvents();

            ThemeClassName = typeof(RadPageView).ToString();
		}

		//-----------------------------------------------------------------------------------------
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

		//-----------------------------------------------------------------------------------------
		private void AttachEvents()
		{
			Debug.Assert(SelectedPageChanged == null, "Se non hai capito, non devi usarlo");

			base.SelectedPageChanged += new EventHandler(UITabControl_SelectedPageChanged);
		}
	
		//-----------------------------------------------------------------------------------------
		private void DetachEvents()
		{
			base.SelectedPageChanged -= new EventHandler(UITabControl_SelectedPageChanged);
		}

		//-----------------------------------------------------------------------------------------
		void UITabControl_SelectedPageChanged(object sender, EventArgs e)
		{
			if (UISelectedPageChanged != null)
				UISelectedPageChanged(sender, e);
		}

		//-------------------------------------------------------------------------
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			ShowCloseButton = false;
		}
		//-------------------------------------------------------------------------
		[DefaultValue(false)]
		public bool ShowCloseButton
		{
			get
			{
                RadPageViewStripElement s = this.ViewElement as RadPageViewStripElement;
				if (s != null)
					return s.ShowItemCloseButton;
				
				return false;
			}
			set
			{

                RadPageViewStripElement s = this.ViewElement as RadPageViewStripElement;
				if (s != null)
				{
					s.ShowItemCloseButton = value;
					s.StripButtons = value == true
						? StripViewButtons.RightScroll | StripViewButtons.LeftScroll | StripViewButtons.Close
						: StripViewButtons.RightScroll | StripViewButtons.LeftScroll ;
				}
			}
		}

		//-----------------------------------------------------------------------------------------
		[EditorBrowsable(EditorBrowsableState.Always)]
		public UITabControlViewMode TabControlViewMode { get { return (UITabControlViewMode)this.ViewMode; } set { this.ViewMode = (PageViewMode)value; } }

        //-------------------------------------------------------------------------
        public void AddTabPage(Type tabType, string tabTitle, IUIUserControl parent)
        {
            cui.AddTabPage(tabType, tabTitle, parent);
        }
	}

	//================================================================================================================
	public enum UITabControlViewMode 
	{
		BackStage = PageViewMode.Backstage, 
		ExplorerBar = PageViewMode.ExplorerBar,
		Outlook = PageViewMode.Outlook, 
		Stack = PageViewMode.Stack,
		Strip = PageViewMode.Strip
	}
}
