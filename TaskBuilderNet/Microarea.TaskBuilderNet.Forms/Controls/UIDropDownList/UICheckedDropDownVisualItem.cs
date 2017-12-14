using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Forms.Properties;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	internal class UICheckedDropDownVisualItem : RadListVisualItem
	{
		public event EventHandler<EventArgs> ToggleStateChanged;

		StackLayoutElement stack;
		RadLabelElement content;
		RadImageItem picture;

		//-----------------------------------------------------------------------------------------
		public UICheckedDropDownVisualItem()
		{
			this.DrawText = false;
		}

		//-----------------------------------------------------------------------------------------
		protected virtual void OnToggleStateChanged()
		{
			if (ToggleStateChanged != null)
			{
				ToggleStateChanged(this, EventArgs.Empty);
			}
		}

		//-----------------------------------------------------------------------------------------
		protected override void CreateChildElements()
		{
			base.CreateChildElements();
			stack = new StackLayoutElement();
			stack.Orientation = Orientation.Horizontal;
			this.Children.Add(stack);

			picture = new RadImageItem();
			stack.Children.Add(picture);

			content = new RadLabelElement();
			content.StretchHorizontally = false;
			content.StretchVertically = true;
			content.TextAlignment = ContentAlignment.MiddleLeft;
			content.NotifyParentOnMouseInput = true;
			stack.Children.Add(content);
		}

		//-----------------------------------------------------------------------------------------
		protected override void SynchronizeProperties()
		{
			base.SynchronizeProperties();
			this.content.Text = this.Data.Text;

			Image old = this.picture.Image;
			this.picture.Image = this.Data.Selected ? Resources.Checked : Resources.Unchecked;

			if (old != this.picture.Image)
			{
				OnToggleStateChanged();
			}
		}

		//-----------------------------------------------------------------------------------------
		protected override Type ThemeEffectiveType
		{
			get
			{
				return typeof(RadListVisualItem);
			}
		}
	}
}
