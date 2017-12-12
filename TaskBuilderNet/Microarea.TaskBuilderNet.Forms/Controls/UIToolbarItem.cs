using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//===================================================================================
	public class UIToolbarButton : CommandBarButton, IUIComponent, IUIToolbarItem
	{
		ITBCUI cui;

		[Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }
	
		public event EventHandler VisibleChanged;
		public event EventHandler ParentChanged;

		//-------------------------------------------------------------------------
		public void OnVisibleChanged()
		{
			if (VisibleChanged != null)
				VisibleChanged(null, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public void OnParentChanged(object owner)
		{
			if (ParentChanged != null)
				ParentChanged(owner, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public UIToolbarButton() 
		{
			cui = new TBCUI(this, Interfaces.NameSpaceObjectType.ToolbarButton);

			this.ThemeRole = typeof(CommandBarButton).Name;
			this.Disposing += new EventHandler(UIToolbarButton_Disposing);
		}

		//-------------------------------------------------------------------------
		void UIToolbarButton_Disposing(object sender, EventArgs e)
		{
			if (cui != null)
			{
				cui.Dispose();
				cui = null;
			}
		}

		//-------------------------------------------------------------------------
		public bool Visible
		{
			get
			{
				return cui.IsVisible;
			}
			set
			{
				//TODOLUCA
			}
		}

	}

	//===================================================================================
	public class UIToolbarSplitButton : CommandBarSplitButton, IUIComponent, IUIToolbarItem
	{
		ITBCUI cui;

		[Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }

		public event EventHandler VisibleChanged;
		public event EventHandler ParentChanged;

		//-------------------------------------------------------------------------
		public void OnVisibleChanged()
		{
			if (VisibleChanged != null)
				VisibleChanged(this, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public void OnParentChanged(object owner)
		{
			if (ParentChanged != null)
				ParentChanged(owner, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public UIToolbarSplitButton()
		{
			cui = new TBCUI(this, Interfaces.NameSpaceObjectType.ToolbarButton);

            this.ThemeRole = typeof(CommandBarSplitButton).Name;
			this.Disposing += new EventHandler(UIToolbarSplitButton_Disposing);

			buttonBorder.Visibility = ElementVisibility.Hidden;
			buttonSeparator.BorderRightWidth = 0;
			buttonSeparator.BorderLeftWidth = 0.1f;
		}

		//-------------------------------------------------------------------------
		void UIToolbarSplitButton_Disposing(object sender, EventArgs e)
		{
			if (cui != null)
			{
				cui.Dispose();
				cui = null;
			}
		}

		//-------------------------------------------------------------------------
		public bool Visible
		{
			get
			{
				return cui.IsVisible;
			}
			set
			{
				//TODOLUCA
			}
		}
	}
}
