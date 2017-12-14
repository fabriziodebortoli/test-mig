using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	internal class UIMultiSelectionDropDownEditorElement : RadDropDownListEditorElement, IDisposable
	{
		private enum AssignFrom { Text, Value }

		string multiSelectionDelimiter = ";";
		string selectedValues;

		//-----------------------------------------------------------------------------------------
		internal string MultiSelectionDelimiter
		{
			get { return multiSelectionDelimiter; }
			set
			{
				multiSelectionDelimiter = value;
			}
		}

		//-----------------------------------------------------------------------------------------
		public string SelectedValues
		{
			get
			{
				return selectedValues;
			}
			set
			{
				selectedValues = value;
				if (selectedValues == null || selectedValues.Trim().Length == 0)
				{
					return;
				}

				AssignValues(value, AssignFrom.Value);
				UpdateText();
				UpdateSelectedValues();
				OnNotifyPropertyChanged("SelectedValues");
			}
		}

		//-----------------------------------------------------------------------------------------
		private void AssignValues(string text, AssignFrom assignFrom)
		{
			string[] values = text.Split(
							new string[] { this.multiSelectionDelimiter },
							StringSplitOptions.RemoveEmptyEntries
							);

			foreach (var item in this.ListElement.Items)
			{
				bool selected = false;
				foreach (var val in values)
				{
					string compareTo = assignFrom == AssignFrom.Text ?  item.Text : item.Value.ToString();
					selected |= String.Compare(compareTo, val, StringComparison.OrdinalIgnoreCase) == 0;
				}
				item.Selected = selected;
			}
		}

		//-----------------------------------------------------------------------------------------
		internal UIMultiSelectionDropDownEditorElement()
		{
			this.CreatingVisualItem += new CreatingVisualListItemEventHandler(CustomEditorElement_CreatingVisualItem);
			this.ListElement.ItemDataBinding += this.CustomEditorElement_ItemDataBinding;
		}

		//-----------------------------------------------------------------------------------------
		private void UpdateSelectedValues()
		{
			StringBuilder valuesBld = new StringBuilder();
			foreach (var item in this.ListElement.SelectedItems)
			{
				valuesBld.Append(item.Value.ToString()).Append(this.MultiSelectionDelimiter);
			}

			if (valuesBld.Length > 0)
			{
				valuesBld.Replace(this.MultiSelectionDelimiter, String.Empty, valuesBld.Length - 1, 1);
			}

			selectedValues = valuesBld.ToString();
		}

		//-----------------------------------------------------------------------------------------
		private void UpdateText()
		{
			StringBuilder textBld = new StringBuilder();
			foreach (RadListDataItem item in this.ListElement.SelectedItems)
			{
				textBld.Append(item.Text).Append(MultiSelectionDelimiter);
			}
			if (textBld.Length > 0)
			{
				textBld.Replace(this.MultiSelectionDelimiter, String.Empty, textBld.Length - 1, 1);
			}
			this.Text = this.ToolTipText = textBld.ToString();
		}

		//-----------------------------------------------------------------------------------------
		internal void Init()
		{
			this.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;

			RootRadElement parent = this.Parent as RootRadElement;

			Debug.Assert(parent != null);

			if (parent != null)
			{
				//Minimum size della tendina aperta: do` valori arbitrari (uso sempre la width del control)
				int minValue = parent.ControlBounds.Width;

				this.Popup.MinimumSize = new Size(minValue, minValue);
			}
		}

		//-----------------------------------------------------------------------------------------
		public override void ClosePopup(RadPopupCloseReason reason)
		{
			base.ClosePopup(reason);

			GridDataCellElement cell = this.Parent as GridDataCellElement;
			if (cell != null)
			{
				cell.GridViewElement.EndEdit();
			}
		}

		//-----------------------------------------------------------------------------------------
		private void CustomEditorElement_ItemDataBinding(object sender, ListItemDataBindingEventArgs args)
		{
			args.NewItem = new UIDropListDataItem();
		}

		//-----------------------------------------------------------------------------------------
		void CustomEditorElement_CreatingVisualItem(object sender, CreatingVisualListItemEventArgs args)
		{
			UICheckedDropDownVisualItem item = new UICheckedDropDownVisualItem();
			item.ToggleStateChanged += new EventHandler<EventArgs>(Item_ToggleStateChanged);
			item.Disposing += new EventHandler(Item_Disposing);
			args.VisualItem = item;
		}

		//-----------------------------------------------------------------------------------------
		void Item_Disposing(object sender, EventArgs e)
		{
			UICheckedDropDownVisualItem item = sender as UICheckedDropDownVisualItem;
			if (item != null)
			{
				item.ToggleStateChanged -= new EventHandler<EventArgs>(Item_ToggleStateChanged);
				item.Disposing -= new EventHandler(Item_Disposing);
			}
		}

		//-----------------------------------------------------------------------------------------
		void Item_ToggleStateChanged(object sender, EventArgs e)
		{
			UpdateText();
			UpdateSelectedValues();
			OnNotifyPropertyChanged("SelectedValues");
		}

		//-----------------------------------------------------------------------------------------
		protected override void OnPopupClosing(RadPopupClosingEventArgs e)
		{
			base.OnPopupClosing(e);

			if (
				e.CloseReason == RadPopupCloseReason.Mouse &&
				this.PopupForm.Bounds.Contains(Control.MousePosition)
				)
				e.Cancel = true;
		}

		//-----------------------------------------------------------------------------------------
		public new void Dispose()
		{
			base.Dispose(true);

			this.CreatingVisualItem -= new CreatingVisualListItemEventHandler(CustomEditorElement_CreatingVisualItem);
			this.ListElement.ItemDataBinding -= this.CustomEditorElement_ItemDataBinding;
		}

		//-----------------------------------------------------------------------------------------
		public override void ShowPopup()
		{
			bool[] selected = new bool[this.Items.Count];
			for (int i = 0; i < selected.Length; i++)
			{
				selected[i] = this.Items[i].Selected;
			}
			base.ShowPopup();
			for (int i = 0; i < selected.Length; i++)
			{
				this.Items[i].Selected = selected[i];
			}
		}
	}
}
