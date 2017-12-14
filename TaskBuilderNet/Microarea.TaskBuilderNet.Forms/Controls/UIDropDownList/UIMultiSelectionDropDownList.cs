using System;
using System.ComponentModel;
using System.Diagnostics;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Data;
using DataPositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	/// <summary>
	/// UIMultiSelectionDropDownList
	/// </summary>
	public class UIMultiSelectionDropDownList : UIDropDownList
	{
		//-------------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use SelectedIndexChanged, it is a nonsense")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler<PositionChangingCancelEventArgs> SelectedIndexChanging;

		//-------------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use SelectedIndexChanged, it is a nonsense")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler<DataPositionChangedEventArgs> SelectedIndexChanged;

		//-------------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use SelectedValue, use SelectedValues instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object SelectedValue
		{
			get
			{
				return null;
			}
			set
			{
				Debug.Assert(false, "Se non hai capito, non devi usarlo");
			}
		}

		//-------------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use SelectedValue, it is a nonsense")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int SelectedIndex
		{
			get
			{
				return -1;
			}
			set
			{
				Debug.Assert(false, "Se non hai capito, non devi usarlo");
			}
		}

		//-------------------------------------------------------------------------
		[Browsable(false)]
		[Obsolete("do not use SelectedValue, it is a nonsense")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new RadListDataItem SelectedItem
		{
			get
			{
				return null;
			}
			set
			{
				Debug.Assert(false, "Se non hai capito, non devi usarlo");
			}
		}

		//-------------------------------------------------------------------------
		private UIMultiSelectionDropDownEditorElement UIDropDownEditorElement { get { return DropDownListElement as UIMultiSelectionDropDownEditorElement; } }

		//-------------------------------------------------------------------------
		[DefaultValue(";")]
		public string MultiSelectionDelimiter { get { return UIDropDownEditorElement.MultiSelectionDelimiter; } set { UIDropDownEditorElement.MultiSelectionDelimiter = value; } }

		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override object UIValue
		{
			get
			{
				return this.SelectedValues;
			}
			set
			{
				this.SelectedValues = value as string;
			}
		}

		//-------------------------------------------------------------------------
		public override string Text
		{
			get
			{
				return this.DropDownListElement.Text;
			}
			set
			{
				this.DropDownListElement.Text = value;
			}
		}

		//-----------------------------------------------------------------------------------------
		public UIMultiSelectionDropDownList()
		{
			WireEvents();
			UIDropDownEditorElement.Init();
		}

		//-------------------------------------------------------------------------
		private void WireEvents()
		{
			Debug.Assert(SelectedIndexChanged == null, "Se non hai capito, non devi usarlo");
			Debug.Assert(SelectedIndexChanging == null, "Se non hai capito, non devi usarlo");

			this.UIDropDownEditorElement.PropertyChanged += new PropertyChangedEventHandler(UIDropDownEditorElement_PropertyChanged);
			this.PropertyChanged += new PropertyChangedEventHandler(UIMultiSelectionDropDownList_PropertyChanged);
			base.ItemDataBinding += new ListItemDataBindingEventHandler(UIDropDownList_ItemDataBinding);
		}

		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			UnwireEvents();
		}

		//-------------------------------------------------------------------------
		private void UnwireEvents()
		{
			this.UIDropDownEditorElement.PropertyChanged -= new PropertyChangedEventHandler(UIDropDownEditorElement_PropertyChanged);
			this.PropertyChanged -= new PropertyChangedEventHandler(UIMultiSelectionDropDownList_PropertyChanged);
			base.ItemDataBinding -= new ListItemDataBindingEventHandler(UIDropDownList_ItemDataBinding);
		}

		//-------------------------------------------------------------------------
		void UIMultiSelectionDropDownList_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "DataSource")
			{
				base.SelectedItem = null;
			}
		}

		//-------------------------------------------------------------------------
		public override string DefaultBindingProperty
		{
			get { return "SelectedValues"; }
		}

		//-----------------------------------------------------------------------------------------
		void UIDropDownEditorElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SelectedValues")
			{
				OnNotifyPropertyChanged("SelectedValues");
			}
		}

		//-----------------------------------------------------------------------------------------
		void UIDropDownList_ItemDataBinding(object sender, ListItemDataBindingEventArgs args)
		{
			args.NewItem = new UIDropListDataItem();
		}

		//-----------------------------------------------------------------------------------------
		public string SelectedValues
		{
			get
			{
				return this.UIDropDownEditorElement.SelectedValues;
			}
			set
			{
				this.UIDropDownEditorElement.SelectedValues = value;
			}
		}

		//-----------------------------------------------------------------------------------------
		protected override RadDropDownListElement CreateDropDownListElement()
		{
			return new UIMultiSelectionDropDownEditorElement();
		}
	}
}
