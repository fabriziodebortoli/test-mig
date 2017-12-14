using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.Woorm.TBWebFormControl
{
    public class TBFieldSet : WebControl
    {
        public TBFieldSet() { Align = AlignStyle.Left; }

        public enum AlignStyle { Left, Center, Right }
        public AlignStyle Align { get; set; }

        public string Caption { get; set; }

        protected override string TagName { get { return "fieldset"; } }

        protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Fieldset; } }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            base.RenderContents(writer);

            if (Align != AlignStyle.Left)
            {
               writer.AddAttribute("align", Align == AlignStyle.Center ? "center" : "right");    //funziona IE8 e FireFox3.6, ma è deprecato        
               writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, Align == AlignStyle.Center ? "center" : "right"); //previsto in HTML4, ma ancora non supportato
               //su Opera 10.51 non funzionano entrambi
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
            writer.WriteEncodedText(Caption);
            writer.RenderEndTag();
         }
    }
}
