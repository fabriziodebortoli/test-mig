using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	/// <summary>
	/// UIDropDownList
	/// </summary>
	public abstract class UIDropDownList : RadDropDownList, IUIControl, ITBBindableObject, ITBDataSource, IUIHostingControl, IUIGridEditorControl
	{
		TBWFCUIControl cui;

		int maxDropDownWidth = 0;
		int maxDropDownItems = 20;
		
		public event EventHandler<EventArgs> UIDropDownOpening;
		public event EventHandler<EventArgs> UIDropDownClosed;

		TemplateItemCollection<RadListDataItemCollection, UIDropListDataItem> internalItems;

		//-----------------------------------------------------------------------------------------
		[Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public abstract object UIValue
		{
			get;
			set;
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection<RadListDataItemCollection, UIDropListDataItem> UIItems
		{
			get
			{
				return internalItems;
			}
			set
			{
				internalItems = value;
			}
		}

		[Browsable(false)]
		[Obsolete("do not use Items, use UIItems instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new RadListDataItemCollection Items
		{
			get { return base.Items; }
		}

		//-------------------------------------------------------------------------
		public virtual object GetFocusableElement()
		{
			return DropDownListElement;
		}

		//-----------------------------------------------------------------------------------------
		public virtual Control HostedControl
		{
			get
			{
				return DropDownListElement.EditableElement.TextBox.TextBoxItem.HostedControl;
			}
		}

		//-----------------------------------------------------------------------------------------
		public abstract string DefaultBindingProperty
		{
			get;
		}
		
		//-----------------------------------------------------------------------------------------
		protected UIDropDownList()
		{
			cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control);
			internalItems = new TemplateItemCollection<RadListDataItemCollection, UIDropListDataItem>(base.Items);
			WireEvents();
			ThemeClassName = typeof(RadDropDownList).ToString();
			base.DropDownStyle = RadDropDownStyle.DropDownList;
			base.DropDownListElement.DropDownSizingMode = SizingMode.RightBottom;
			this.DropDownListElement.MaxDropDownItems = maxDropDownItems;
			this.FitItemsToSize = false;
		}

		//-----------------------------------------------------------------------------------------
		protected override void OnItemDataBound(object sender, ListItemDataBoundEventArgs args)
		{
			base.OnItemDataBound(sender, args);
			SetDropDownWidthForItemText(args.NewItem.Text);
			SetDropDownHeightForItems();
		}

		//-----------------------------------------------------------------------------------------
		void ListElement_ItemsChanged(object sender, Telerik.WinControls.Data.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == Telerik.WinControls.Data.NotifyCollectionChangedAction.Add)
			{
				if (e.NewItems == null || e.NewItems.Count < 0)
					return;

				foreach (var current in e.NewItems)
				{
					RadListDataItem item = current as RadListDataItem;
					if (item == null)
						continue;

					SetDropDownWidthForItemText(item.Text);
					SetDropDownHeightForItems();
				}
			}
		}

		//-----------------------------------------------------------------------------------------
		private void SetDropDownHeightForItems()
		{
			this.DropDownListElement.MaxDropDownItems = this.DefaultItemsCountInDropDown =
				Math.Min
				(
				this.DropDownListElement.ListElement.Items.Count,
				maxDropDownItems
				);
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void SetDropDownWidthForItemText(string text)
		{
			int width = TextWidth(text);
			if (width > maxDropDownWidth)
				maxDropDownWidth = width;

			this.DropDownListElement.DropDownWidth = maxDropDownWidth;
		}

		//-----------------------------------------------------------------------------------------
		public int TextWidth(String text)
		{
			using (Graphics graphics = this.CreateGraphics())
			{
				//TODOLUCA il + 30 è per le multiselection che hanno la picture box in più
				return (int)graphics.MeasureString(text, this.Font).Width + 30;
			}
		}

		//-------------------------------------------------------------------------
		public void FireValidated()
		{
			OnValidated(EventArgs.Empty);
		}

		//-------------------------------------------------------------------------
		public void FireValidating(CancelEventArgs e)
		{
			OnValidating(e);
		}

		//-----------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			UnwireEvents();
			if (cui != null)
			{
				cui.Dispose();
				cui = null;
			}
		}

		//-----------------------------------------------------------------------------------------
		private void WireEvents()
		{
			base.PopupOpening += new CancelEventHandler(UIDropDownList_PopupOpening);
			base.PopupClosed += new RadPopupClosedEventHandler(UIDropDownList_PopupClosed);
			this.DropDownListElement.ListElement.ItemsChanged += new Telerik.WinControls.Data.NotifyCollectionChangedEventHandler(ListElement_ItemsChanged);
		}

		//-----------------------------------------------------------------------------------------
		private void UnwireEvents()
		{
			base.PopupOpening -= new CancelEventHandler(UIDropDownList_PopupOpening);
			base.PopupClosed -= new RadPopupClosedEventHandler(UIDropDownList_PopupClosed);
			this.DropDownListElement.ListElement.ItemsChanged -= new Telerik.WinControls.Data.NotifyCollectionChangedEventHandler(ListElement_ItemsChanged);
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void UIDropDownList_PopupClosed(object sender, RadPopupClosedEventArgs args)
		{
			if (UIDropDownClosed != null)
				UIDropDownClosed(sender, EventArgs.Empty);
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void UIDropDownList_PopupOpening(object sender, CancelEventArgs e)
		{
			if (UIDropDownOpening != null)
				UIDropDownOpening(sender, EventArgs.Empty);
		}
	}
}
