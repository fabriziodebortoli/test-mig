using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UITextBox : RadMaskedEditBox, IUIControl, ITBBindableObject, IMaskedInput, IUIHostingControl, IUIGridEditorControl
	{
        TBWFCUIControl cui;
		UIMaskStatus maskStatus;

		[Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }

		[Browsable(false)]
		public UIMaskStatus MaskStatus
		{
			get { return maskStatus; }
			set { maskStatus = value; }
		}

        //-------------------------------------------------------------------------
		int maxLength;

		//-------------------------------------------------------------------------
		public UITextBox()
		{
            cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control); 
			this.KeyPress +=new KeyPressEventHandler(InnerControl_KeyPress);
			base.TabStop = true;
			//disabilita le frecce e il mouse wheel per evitare incremento/decremento dei numerici
            MaskedEditBoxElement.EnableMouseWheel = false;
            MaskedEditBoxElement.EnableArrowKeys = false;
			MaskedEditBoxElement.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
			MaskStatus = UIMaskStatus.None;
            ThemeClassName = typeof(RadMaskedEditBox).ToString();
		}

		//-------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
		}

		[Browsable(false)]
		[Obsolete("do not use RootElement")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-------------------------------------------------------------------------
		public new RootRadElement RootElement { get { return base.RootElement; } }

		[DefaultValue(Microarea.TaskBuilderNet.Interfaces.View.MaskType.None)]
		//-------------------------------------------------------------------------
		public Microarea.TaskBuilderNet.Interfaces.View.MaskType UIMaskType 
		{ 
			get { return (Microarea.TaskBuilderNet.Interfaces.View.MaskType)base.MaskType; } 
			set { base.MaskType = (Telerik.WinControls.UI.MaskType)value; }
		}

		//-------------------------------------------------------------------------
		void InnerControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ( Char.IsDigit(e.KeyChar)  && Text.Length == maxLength)
				e.Handled = true;
		}

		[DefaultValue(0)]
		//-------------------------------------------------------------------------
		public int MaxLength { get { return maxLength; } set { maxLength = value; } }
       
		//-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue 
		{ 
			get 
			{
				//Se la Textbox ha una maschera di tipo Percent, viene tolto il carattere %, siccome il control lo mette (ApplyToEditableControl di InputStrategies), 
				//il control deve toglierlo
				Regex percentRegex = new Regex("p[0-9]+");
				if (percentRegex.Match(Mask).Success)
				{
					//TODO BRUNA SILVANO: Sono ridondanti, ma una fa funzionare campo percentuale in testa, l'altra sulal griglia
					//siamo in attesa di risposta da Telerik per un possibile bug, per affrontare il problema in maniera piu' ortodossa
					string trimmedText = Text.Replace("%", "").Replace(PromptChar.ToString(), "").Trim();
					Text = trimmedText;//qui in caso di griglia assegnazione non va a buon fine perche il campo ha ancora la maschera percentule
					return trimmedText;
				}
				Text = Text.Replace(PromptChar.ToString(), "").Trim();
				return Text;
			} 

			set
			{
				if (value != null)
				{
					Text = value.ToString();
				}
				else
				{
					Text = String.Empty;
				}
			}
		
		}

		//-------------------------------------------------------------------------
		public object GetFocusableElement()
		{
			return MaskedEditBoxElement;
		}

		//-------------------------------------------------------------------------
		[DefaultValue(true)]
		public new bool TabStop { get { return base.TabStop; } set { base.TabStop = value; } }

        //-------------------------------------------------------------------------
        public Control HostedControl
        {
            get { return MaskedEditBoxElement.TextBoxItem.HostedControl;}
        }
		
		//-------------------------------------------------------------------------
		public string DefaultBindingProperty
		{
			get { return "Text"; }
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

	}
}
