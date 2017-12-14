using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UIEditableDropDownList : UISingleSelectionDropDownList, IMaskedInput
	{
        private RadMaskedEditBox maskedTextBox;
		private HostedTextBoxBase hostedControl;
		private UIMaskStatus maskStatus;

		//-----------------------------------------------------------------------------------------
		[Browsable(false)]
		public UIMaskStatus MaskStatus
		{
			get { return maskStatus; }
			set { maskStatus = value; }
		}

		//-----------------------------------------------------------------------------------------
		public override Control HostedControl
		{

			get
			{
				return maskedTextBox.MaskedEditBoxElement.TextBoxItem.HostedControl;
			}
		}


		//-----------------------------------------------------------------------------------------
		public UIEditableDropDownList()
		{
            maskedTextBox = new RadMaskedEditBox();
            maskedTextBox.Font = this.Font;

			hostedControl = (HostedTextBoxBase)this.Controls[0];
			hostedControl.TextChanged += new EventHandler(hostedControl_TextChanged);
			maskedTextBox.TextChanged += new EventHandler(maskedTextBox_TextChanged);
			Controls.Add(maskedTextBox);

			//per tenere nascosta la textbox originale (hostedControl)
			hostedControl.Visible = false;
			hostedControl.VisibleChanged += new EventHandler(hostedControl_VisibleChanged);

			maskedTextBox.Multiline = true;//per rendere possibile il resize
			maskedTextBox.Left = 0;
			maskedTextBox.Top = 0;
			MaskStatus = UIMaskStatus.None;

			this.UIDropDownListStyle = UIDropDownStyle.DropDown;
			this.FontChanged += new EventHandler(UIEditableDropDownList_FontChanged);
		}

		//-----------------------------------------------------------------------------------------
		void UIEditableDropDownList_FontChanged(object sender, EventArgs e)
		{
			maskedTextBox.Font = Font;
		}

		//-----------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				this.FontChanged -= new EventHandler(UIEditableDropDownList_FontChanged);
			}
		}

		//-----------------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			maskedTextBox.Width = Width - 20;
			maskedTextBox.Height = Height + 2;
		}


		//-----------------------------------------------------------------------------------------
		void hostedControl_VisibleChanged(object sender, EventArgs e)
		{
			hostedControl.Visible = false;
		}

		//-----------------------------------------------------------------------------------------
		void maskedTextBox_TextChanged(object sender, EventArgs e)
		{
			hostedControl.Text = maskedTextBox.Text;
		}

		//-----------------------------------------------------------------------------------------
		void hostedControl_TextChanged(object sender, EventArgs e)
		{
			maskedTextBox.Text = hostedControl.Text;
		}

		//-----------------------------------------------------------------------------------------
		public override string Text { get { return maskedTextBox.Text; } set { maskedTextBox.Text = value; } }
		public string Mask { get { return maskedTextBox.Mask; } set { maskedTextBox.Mask = value; } }
		public Microarea.TaskBuilderNet.Interfaces.View.MaskType UIMaskType { get { return (Microarea.TaskBuilderNet.Interfaces.View.MaskType)maskedTextBox.MaskType; } set { maskedTextBox.MaskType = (Telerik.WinControls.UI.MaskType)value; } }
		public System.Windows.Forms.CharacterCasing CharacterCasing { get { return maskedTextBox.CharacterCasing; } set { this.maskedTextBox.CharacterCasing = value; } }
		public char PromptChar { get { return this.maskedTextBox.PromptChar; } set { this.maskedTextBox.PromptChar = value; } }
		public CultureInfo Culture { get { return this.maskedTextBox.MaskedEditBoxElement.Culture; } set { this.maskedTextBox.MaskedEditBoxElement.Culture = value; } }
		public bool Multiline { get { return this.maskedTextBox.Multiline; } set { this.maskedTextBox.Multiline = value; } }
		public bool ReadOnly { get { return maskedTextBox.ReadOnly; } set { maskedTextBox.ReadOnly = value; } }
	}
}
