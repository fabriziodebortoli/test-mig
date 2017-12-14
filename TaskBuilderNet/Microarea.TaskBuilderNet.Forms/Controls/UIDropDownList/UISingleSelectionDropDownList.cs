using System;
using System.ComponentModel;
using System.Diagnostics;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Data;
using DataPositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;
using DataPositionChangedEventHandler = Telerik.WinControls.UI.Data.PositionChangedEventHandler;
using DataPositionChangingEventHandler = Telerik.WinControls.UI.Data.PositionChangingEventHandler;


namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UISingleSelectionDropDownList : UIDropDownList
	{
        public event EventHandler<UIPositionChangedEventArgs> UISelectedIndexChanged;
		public event EventHandler<UIPositionChangingCancelEventArgs> UISelectedIndexChanging;

		UIDropDownStyle uiDropDownListStyle;
		
		[Browsable(false)]
		[Obsolete("do not use SelectedIndexChanged, use UIISelectedIndexChanged instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler<PositionChangingCancelEventArgs> SelectedIndexChanging;

		[Browsable(false)]
		[Obsolete("do not use SelectedIndexChanged, use UIISelectedIndexChanged instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new event EventHandler<DataPositionChangedEventArgs> SelectedIndexChanged;

		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override object UIValue
		{ 
			get 
			{
				return base.SelectedValue == null ? base.Text : base.SelectedValue;
			}
			set 
			{
				base.SelectedValue = value; 
			}
		}

		//-------------------------------------------------------------------------
		public UIDropDownStyle UIDropDownListStyle
		{
			get { return this.uiDropDownListStyle; }
			set { uiDropDownListStyle = value; }
		}

		[Browsable(false)]
		[Obsolete("do not use DropDownStyle, use UIDropDownListStyle instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new RadDropDownStyle DropDownStyle
		{
			get { return base.DropDownStyle; }
			set
			{
				Debug.Assert(false, "Se non hai capito, non devi usarlo");
			}
		}

		//-----------------------------------------------------------------------------------------
		public UISingleSelectionDropDownList()
		{
			this.uiDropDownListStyle = UIDropDownStyle.DropDownList;
			WireEvents(); 
		}
		
		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			UnwireEvents();
		}

		//-------------------------------------------------------------------------
		public override string DefaultBindingProperty
		{
			get { return "SelectedValue"; }
		}
		
		//-----------------------------------------------------------------------------------------
		private void WireEvents()
		{
			Debug.Assert(SelectedIndexChanged == null, "Se non hai capito, non devi usarlo");
			Debug.Assert(SelectedIndexChanging == null, "Se non hai capito, non devi usarlo");

			base.SelectedIndexChanged += new DataPositionChangedEventHandler(UIDropDownList_SelectedIndexChanged);
			base.SelectedIndexChanging += new DataPositionChangingEventHandler(UIDropDownList_SelectedIndexChanging);
		}

		//-----------------------------------------------------------------------------------------
		private void UnwireEvents()
		{
			base.SelectedIndexChanged -= new DataPositionChangedEventHandler(UIDropDownList_SelectedIndexChanged);
			base.SelectedIndexChanging -= new DataPositionChangingEventHandler(UIDropDownList_SelectedIndexChanging);
		}

		//-----------------------------------------------------------------------------------------
		protected override void UIDropDownList_PopupClosed(object sender, RadPopupClosedEventArgs args)
		{
			// questa riga di codice fa scattare il validating sul controllo anche
			// durante la selezione dell'item. Si salva il vecchio valore e lo
			// ripristina se la validazione è fallita.
			object oldValue = this.SelectedValue;
			CancelEventArgs e = new CancelEventArgs();
			FireValidating(e);
			if (e.Cancel == true)
				this.SelectedValue = oldValue;

			base.UIDropDownList_PopupClosed(sender, args);
		}

		//-----------------------------------------------------------------------------------------
		void UIDropDownList_SelectedIndexChanging(object sender, PositionChangingCancelEventArgs e)
		{
			if (UISelectedIndexChanging != null)
				UISelectedIndexChanging(sender, new UIPositionChangingCancelEventArgs(e));
		}

		//-----------------------------------------------------------------------------------------
		void UIDropDownList_SelectedIndexChanged(object sender, DataPositionChangedEventArgs e)
		{
			if (UISelectedIndexChanged != null)
				UISelectedIndexChanged(sender, new UIPositionChangedEventArgs(e));
		}

		//-----------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new UIDropListDataItem SelectedItem
		{
			get
			{
				//la combo telerik infila nel Value di un RadListDataItem nuovo di zecca l'oggetto passato via
				//databinding
				//return base.SelectedItem; //il giorno in cui correggono il buco 9738, sarà così
				return base.SelectedValue as UIDropListDataItem;
			}

			set
			{
				base.SelectedItem = value;
			}
		}
	}
}
