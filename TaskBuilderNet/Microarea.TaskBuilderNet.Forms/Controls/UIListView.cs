using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UIListView : RadListView, IUIControl, ITBDataSource
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		TemplateItemCollection<ListViewDataItemCollection, UIListViewDataItem> internalItems;
		TemplateItemCollection<ListViewCheckedItemCollection, UIListViewDataItem> internalCheckedItems;
		TemplateItemCollection<ListViewSelectedItemCollection, UIListViewDataItem> internalSelectedItems;

		[Browsable(false)]
		[Obsolete("do not use SelectedIndexChanged, use UISelectedIndexChanged instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler SelectedIndexChanged;

		public event EventHandler UISelectedIndexChanged;

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		//-----------------------------------------------------------------------------------------
		public UIListView()
		{
            cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control);
			internalItems = new TemplateItemCollection<ListViewDataItemCollection, UIListViewDataItem>(base.Items);
			internalCheckedItems = new TemplateItemCollection<ListViewCheckedItemCollection, UIListViewDataItem>(base.CheckedItems);
			internalSelectedItems = new TemplateItemCollection<ListViewSelectedItemCollection, UIListViewDataItem>(base.SelectedItems);
			
			AttachEvents();
            ThemeClassName = typeof(RadListView).ToString();
		}

        //-----------------------------------------------------------------------------------------
        void UIListView_ItemCreating(object sender, ListViewItemCreatingEventArgs e)
        {
            e.Item = new UIListViewDataItem();
        }

		//-----------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
			DetachEvents();
		}

		//-----------------------------------------------------------------------------------------
		private void AttachEvents()
		{
			Debug.Assert(SelectedIndexChanged == null, "Se non hai capito, non devi usarlo");
			
			base.SelectedIndexChanged += new EventHandler(UIListView_SelectedIndexChanged);
            this.ItemCreating += new ListViewItemCreatingEventHandler(UIListView_ItemCreating);
		}

		//-----------------------------------------------------------------------------------------
		private void DetachEvents()
		{
			base.SelectedIndexChanged -= new EventHandler(UIListView_SelectedIndexChanged);
            this.ItemCreating -= new ListViewItemCreatingEventHandler(UIListView_ItemCreating);
		}

		//-----------------------------------------------------------------------------------------
		void UIListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (UISelectedIndexChanged != null)
				UISelectedIndexChanged(sender, e);
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection<ListViewDataItemCollection, UIListViewDataItem> UIItems
		{
			get { return internalItems; }
			set { internalItems = value; }
		}

		[Browsable(false)]
		[Obsolete("do not use CheckedItems, use UICheckedItems instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new ListViewCheckedItemCollection CheckedItems
		{
			get { return base.CheckedItems; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection<ListViewCheckedItemCollection, UIListViewDataItem> UICheckedItems
		{
			get
			{
				return internalCheckedItems;
			}
		}


		[Browsable(false)]
		[Obsolete("do not use SelectedItems, use UISelectedItems instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new ListViewSelectedItemCollection SelectedItems
		{
			get { return base.SelectedItems; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection<ListViewSelectedItemCollection, UIListViewDataItem> UISelectedItems
		{
			get
			{
				return internalSelectedItems;
			}
		}

		//-----------------------------------------------------------------------------------------
		public UIListViewStyle UIListViewStyle { get { return (UIListViewStyle)base.ViewType; } set { base.ViewType = (ListViewType)value; } }

		[Browsable(false)]
		[Obsolete("do not use ViewType, use UIListViewStyle instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new ListViewType ViewType { get { return base.ViewType; } set { base.ViewType = value; } }
		    

		[Browsable(false)]
		[Obsolete("do not use Items, use UIItems instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new ListViewDataItemCollection Items
		{
			get { return base.Items; }
		}

		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue
		{
			get
			{
				if (base.SelectedItem != null)
					return base.SelectedItem.Value;
				return base.Text;
			}
		}
	}

	//=============================================================================================
	[TypeConverter(typeof(UIListViewDataItemTypeConverter))]
	public class UIListViewDataItem : ListViewDataItem
	{
		//-----------------------------------------------------------------------------------------
		public UIListViewDataItem()
			: base()
		{
		}

		//-----------------------------------------------------------------------------------------
		public UIListViewDataItem(string text)
			: base(text)
		{
		}

		//-----------------------------------------------------------------------------------------
		public UIListViewDataItem(object val)
			: base(val)
		{
		}

		//-----------------------------------------------------------------------------------------
		public UIListViewDataItem(string text, string[] values)
			: base(text, values)
		{
		}
	}

	//=============================================================================================
	public class UIListViewDataItemTypeConverter : TypeConverter
	{
		//clone della classe converter telerik con solo il tipo nostro (derivato) al posto dell'originale

		//-----------------------------------------------------------------------------------------
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (!(destinationType == typeof(string)) && !(destinationType == typeof(InstanceDescriptor)))
			{
				return base.CanConvertTo(context, destinationType);
			}
			return true;
		}

		//-----------------------------------------------------------------------------------------
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
			{
				throw new ArgumentNullException("destinationType");
			}
			ListViewDataItem item = value as ListViewDataItem;
			if ((destinationType == typeof(InstanceDescriptor)) && (item != null))
			{
				ConstructorInfo constructor;
				string[] strArray = new string[item.SubItems.Count];
				for (int i = 0; i < strArray.Length; i++)
				{
					strArray[i] = Convert.ToString(item.SubItems[i], CultureInfo.InvariantCulture);
				}
				if (item.SubItems.Count == 0)
				{
					constructor = typeof(UIListViewDataItem).GetConstructor(new Type[] { typeof(string) });
					if (constructor != null)
					{
						return new InstanceDescriptor(constructor, new object[] { item.Text }, false);
					}
				}
				constructor = typeof(UIListViewDataItem).GetConstructor(new Type[] { typeof(string), typeof(string[]) });
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[] { item.Text, strArray }, false);
				}
			}
			UIListViewDataItem listViewDataItem = value as UIListViewDataItem;
			if ((destinationType == typeof(string)) && listViewDataItem != null)
			{
				return listViewDataItem.Text;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	//=============================================================================================
	public enum UIListViewStyle
	{
		ListView = ListViewType.ListView,
		IconsView = ListViewType.IconsView,
		DetailsView = ListViewType.DetailsView
	}

}
