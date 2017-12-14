using System;
using System.Drawing;
using System.Runtime.Serialization;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace RSjson
{
	[Serializable]
	public class FontData : ISerializable
	{
		const string ITALIC = "Italic";
		const string BOLD = "Bold";
		const string STRIKEOUT = "Strikeout";
		const string UNDERLINE = "Underline";
		const string FAMILY = "Family";
		const string SIZE = "Size";

		public bool Italic;
		public bool Bold;
		public bool Strikeout;
		public bool Underline;
		public string Family;
		public int Size;
		
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

			Family	= fe.FaceName; 
			Size	= fe.Size; 
		}

		//------------------------------------------------------------------------------
		public FontData(SerializationInfo info, StreamingContext context)
		{
			Italic		= info.GetBoolean(ITALIC);
			Bold		= info.GetBoolean(BOLD);
			Strikeout	= info.GetBoolean(STRIKEOUT);
			Underline	= info.GetBoolean(UNDERLINE);

			Family		= info.GetString(FAMILY);
			Size		= info.GetInt32(SIZE); 
		}
		//------------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(ITALIC, Italic);
			info.AddValue(BOLD, Bold);
			info.AddValue(STRIKEOUT, Strikeout);
			info.AddValue(UNDERLINE, Underline);

			info.AddValue(FAMILY, Family);
			info.AddValue(SIZE, Size);
		}
	}
	/// <summary>
	/// Summary description for BasicText.
	/// </summary>
	/// ================================================================================
	[Serializable]
	[KnownType(typeof(FontData))]
	public class BasicText : ISerializable
	{
		//stringhe usate per serializzare
		const string TEXT = "Text"; 
		const string FONTDATA = "FontData";
		const string ALIGN = "Align";
		

		private FontData fontData = null;
		private WoormDocument document; 
		
		public string Text;
		public Color TextColor = Defaults.DefaultTextColor;
		public Color BkgColor = Defaults.DefaultBackColor;
		public int Align = Defaults.DefaultTextAlign;
		public string FontStyleName = DefaultFont.Testo;
		public string DataType = "String";	// tipo del dato contenuto dal campo
		
		public FontData FontData { 
			get 
			{ 
				if (fontData == null)
					fontData =  document == null ? null : new FontData(document.GetFontElement(FontStyleName));
 				return fontData;
			}

			set	{	fontData = value;	}
		}
		

		//------------------------------------------------------------------------------
		public BasicText(SerializationInfo info, StreamingContext context)
		{
			Text = info.GetString(TEXT);
			FontData = info.GetValue<FontData>(FONTDATA);
			Align = info.GetInt32(ALIGN);
		}
		//------------------------------------------------------------------------------
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(TEXT, Text);
			info.AddValue(FONTDATA, FontData);
			info.AddValue(ALIGN, Align);
		}
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
			DataType = s.DataType;
		}
	}
}
