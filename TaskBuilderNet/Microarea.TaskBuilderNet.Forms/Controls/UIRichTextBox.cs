using System;
using System.ComponentModel;
using Microarea.TaskBuilderNet.Forms.TBCUIs;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.RichTextBox;
using Telerik.WinControls.RichTextBox.FileFormats.Html;
using Telerik.WinControls.RichTextBox.FormatProviders;
using Telerik.WinControls.RichTextBox.FormatProviders.Txt;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//================================================================================================================
	public class UIRichTextBox : RadRichTextBox, IUIControl, ITBBindableObject
	{
		TBWFCUIRichTextBox cui;

		ITextBasedDocumentFormatProvider formatProvider = null;
		 
        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
	
        //-------------------------------------------------------------------------
		public UIRichTextBox()
		{
            ThemeClassName = typeof(RadRichTextBox).ToString();
			cui = new TBWFCUIRichTextBox(this);
			
			formatProvider = GetFormatProvider();
		
			this.DocumentContentChanged += new EventHandler(RichTextBox_DocumentContentChanged);
		}

		//-------------------------------------------------------------------------
        protected virtual ITextBasedDocumentFormatProvider GetFormatProvider()
		{
			return new TxtFormatProvider();
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

			this.DocumentContentChanged -= new EventHandler(RichTextBox_DocumentContentChanged);
			formatProvider = null;
        }

        //-------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public object UIValue
        {
			get { return formatProvider.Export(this.Document); }
        }

        //-------------------------------------------------------------------------
        public string DefaultBindingProperty
        {
            get { return "Text"; }
        }

		//----------------------------------------------------------------------------
		void RichTextBox_DocumentContentChanged(object sender, EventArgs e)
		{
			OnTextChanged(new EventArgs());
		}

		//----------------------------------------------------------------------------
		public override string Text
		{
			get
			{
				return formatProvider.Export(this.Document);
			}
			set
			{
				this.DocumentContentChanged -= new EventHandler(RichTextBox_DocumentContentChanged);
				this.Document = formatProvider.Import(value);
				OnTextChanged(new EventArgs());
				this.DocumentContentChanged += new EventHandler(RichTextBox_DocumentContentChanged);
			}
		}
    }

	//================================================================================================================
	public class UIRichHtmlTextBox : UIRichTextBox, IUIControl, ITBBindableObject
	{
		//----------------------------------------------------------------------------
		protected override ITextBasedDocumentFormatProvider GetFormatProvider()
		{
			return new HtmlFormatProvider();
		}

		//----------------------------------------------------------------------------
		public UIRichHtmlTextBox()
			: base()
		{
			ThemeClassName = GetType().BaseType.BaseType.ToString();
		}
	}
}
