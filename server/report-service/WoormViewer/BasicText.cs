using System;
using System.Drawing;

using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.RSWeb.WoormViewer;
using Microarea.Common.ExpressionManager;
//using Microarea.RSWeb.Temp;

namespace Microarea.RSWeb.Objects
{
	//[Serializable]
	public class FontData //: ISerializable
	{
		const string FAMILY = "Family";
		const string SIZE = "Size";
		const string ITALIC = "Italic";
		const string BOLD = "Bold";
		const string STRIKEOUT = "Strikeout";
		const string UNDERLINE = "Underline";

		public string Family = "Arial";
		public int Size = 8;
		public bool Bold = false;
		public bool Italic = false;
		public bool Underline = false;
		public bool Strikeout = false;
        public Color FontColor = Color.Black;
		
		public FontData()
		{
		}

		public FontData(FontElement fe)
		{
			if (fe == null)
				return;

			Italic		= (FontStyle.Italic & fe.FontStyle) == FontStyle.Italic;
			Bold		= (FontStyle.Bold & fe.FontStyle) == FontStyle.Bold;
			Strikeout	= (FontStyle.Strikeout & fe.FontStyle) == FontStyle.Strikeout;
			Underline	= (FontStyle.Underline & fe.FontStyle) == FontStyle.Underline;
            FontColor   = fe.Color;
			Family	    = fe.FaceName; 
			Size	    = fe.Size; 
		}

		//------------------------------------------------------------------------------
		//public FontData(SerializationInfo info, StreamingContext context)
		//{
		//	Italic		= info.GetBoolean(ITALIC);
		//	Bold		= info.GetBoolean(BOLD);
		//	Strikeout	= info.GetBoolean(STRIKEOUT);
		//	Underline	= info.GetBoolean(UNDERLINE);

		//	Family		= info.GetString(FAMILY);
		//	Size		= info.GetInt32(SIZE); 
		//}
		//------------------------------------------------------------------------------
		//public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
		//	info.AddValue(FAMILY, Family);
		//	info.AddValue(SIZE, Size);
		//	info.AddValue(ITALIC, Italic);
		//	info.AddValue(BOLD, Bold);
		//	info.AddValue(STRIKEOUT, Strikeout);
		//	info.AddValue(UNDERLINE, Underline);
		//}

        //------------------------------------------------------------------------------
        public string ToJson(string name = "font", bool bracket = false)
        {
           string s = name.ToJson() + ":{" +
                                        Family.ToJson("face") +
                                        ',' + Size.ToJson("size") +
                                        (Italic ? ',' + Italic.ToJson("italic") : "") +
                                        (Bold ? ',' + Bold.ToJson("bold") : "") +
                                        (Underline ? ',' + Underline.ToJson("underline") : "") +
                                        (FontColor != Color.Black ? ',' + FontColor.ToJson("fontcolor") : "") +
                                        //',' + Strikeout.ToJson("strikeout") + 
                                     '}';



            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        public bool Compare(FontData fd)
        {
            return this.Family.CompareNoCase(fd.Family) &&
                this.Size == fd.Size &&
                this.Bold == fd.Bold &&
                this.Italic == fd.Italic &&
                this.Underline == fd.Underline &&
                this.Strikeout == fd.Strikeout;
        }
    }
    /// <summary>
    /// Summary description for BasicText.
    /// </summary>
    /// ================================================================================
 //   [Serializable]
	//[KnownType(typeof(FontData))]
	public class BasicText //: ISerializable
	{
		protected WoormDocument document; 
		//stringhe usate per serializzare
		//const string TEXT = "Text"; 
		//const string FONTDATA = "FontData";
		//const string ALIGN = "Align";
		
		public string Text;
		public Color TextColor = Defaults.DefaultTextColor;
		public Color BkgColor = Defaults.DefaultBackColor;
		public AlignType Align = Defaults.DefaultTextAlign;
		public string FontStyleName = DefaultFont.Testo;
        protected FontData fontData = null;

		public string DataType = "String";	// tipo del dato contenuto dal campo
		
		public FontData FontData { 
			get 
			{
                if (fontData == null)
                {
                    if (document == null) return new FontData();

                    FontElement fe = document.GetFontElement(FontStyleName);
                    if (fe == null) return new FontData();

                    fontData = new FontData(fe);
                }
 				return fontData;
			}

			set	{	fontData = value;	}
		}
		
		//------------------------------------------------------------------------------
		//public BasicText(SerializationInfo info, StreamingContext context)
		//{
		//	Text = info.GetString(TEXT);
		//	FontData = info.GetValue<FontData>(FONTDATA);
		//	Align = info.GetInt32(ALIGN);
		//}
		//------------------------------------------------------------------------------
		//public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
		//	info.AddValue(TEXT, Text);
		//	info.AddValue(FONTDATA, FontData);
		//	info.AddValue(ALIGN, Align);
		//}
		//-----------------------------------------------------------------------------
		public BasicText()
		{
		}
		
		//------------------------------------------------------------------------------
		public BasicText(WoormDocument doc)
		{
			document = doc;
		}

		//------------------------------------------------------------------------------
		public BasicText(BasicText s)
		{
			Text = s.Text;
			TextColor = s.TextColor;
			BkgColor = s.BkgColor;
			Align = s.Align;
			FontStyleName = s.FontStyleName;
            fontData = s.FontData;
            DataType = s.DataType;

            document = s.document;
        }
	}

    public class BasicTextColored : BasicText
    {
        public Expression TextColorExpr = null;
        public Expression BkgColorExpr = null;
        public bool MiniHtml = false;

        public BasicTextColored(WoormDocument doc) : base(doc)
        {
        }

        //------------------------------------------------------------------------------
        public BasicTextColored(BasicText s) : base(s)
        {

        }
    }
}
