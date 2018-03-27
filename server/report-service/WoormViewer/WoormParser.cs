using System;
using System.Drawing;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;
using Microarea.Common.Temp;

namespace Microarea.RSWeb.WoormViewer
{
	/// <summary>
	/// Summary description for WoormParser.
	/// </summary>
	/// ================================================================================
	public class WoormParser : Parser
	{
		//------------------------------------------------------------------------------
		public WoormParser(SourceType src) : base(src)
		{}

		//------------------------------------------------------------------------------
		public bool ParseRect(Token token, out Rectangle rect)
		{
			int top = 0;
			int left = 0;
			int right = 0;
			int bottom = 0;

			bool ok=
				ParseTag        (token) &&
				ParseOpen       () &&
				ParseSignedInt  (out top) &&
				ParseComma      () &&
				ParseSignedInt  (out left) &&
				ParseComma      () &&
				ParseSignedInt  (out bottom) &&
				ParseComma      () &&
				ParseSignedInt  (out right) &&
				ParseClose      ();

			rect = new Rectangle(left, top, right - left, bottom - top);
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseRect (out Rectangle rect)
		{
			return ParseRect (Token.RECT, out rect);
		}

		//------------------------------------------------------------------------------
		public bool ParseRatio (out int hRatio, out int vRatio)
		{
			hRatio  = 0;
			vRatio  = 0;

			if (LookAhead(Token.RATIO))
				if (!(
						ParseTag    (Token.RATIO) &&
						ParseOpen   () &&
						ParseInt    (out hRatio) &&
						ParseComma  () &&
						ParseInt    (out vRatio) &&
						ParseClose  ()
					))
					return false;

			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParsePen (BorderPen pen)
		{                                     
			Color color;
			if (!ParseColor (Token.PEN, out color)) 
				return false;
			
			pen.Color = color;
			if (LookAhead(Token.SIZE))
			{
				int width;
				if (!ParseSize(out width)) return false;
				pen.Width = width;
			}
			return ParseSep();
		}


		//------------------------------------------------------------------------------
		public bool ParseBorders (Borders brd)
		{
			int top = 0;
			int left = 0;
			int right = 0;
			int bottom = 0;
			
			bool ok =
				ParseTag    (Token.BORDERS) &&
				ParseOpen   () &&
				ParseInt    (out top) &&
				ParseComma  () &&
				ParseInt    (out left) &&
				ParseComma  () &&
				ParseInt    (out bottom) &&
				ParseComma  () &&
				ParseInt    (out right) &&
				ParseClose  () &&
				ParseSep();

			brd.Top = top !=0;
			brd.Left = left !=0;
			brd.Bottom = bottom !=0;
			brd.Right = right !=0;

			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseRGB (out Color color)
		{
			return ParseColor(Token.RGB, out color);
		}

		//------------------------------------------------------------------------------
		public bool ParseTextColor (out Color color, bool parseSep = true)
		{
			return
				ParseColor  (Token.TEXTCOLOR, out color) &&
                parseSep ? ParseSep() : true;
		}

		//------------------------------------------------------------------------------
		public bool ParseBkgColor (out Color color, bool parseSep = true)
		{
			return
				ParseColor  (Token.BKGCOLOR, out color) &&
                parseSep ? ParseSep() : true;
		}

		//------------------------------------------------------------------------------
		public bool ParseAlign (out AlignType align)
		{
            int a = 0; 
			bool ok =
				ParseTag    (Token.ALIGN) &&
				ParseInt	(out a) &&
				ParseSep    ();

            align = (AlignType) a;
            return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseExtendedAlign (out AlignType align)
		{
			int a = 0; 

			bool ok =
				ParseTag    (Token.ALIGN) &&
				ParseInt	(out a);

            align = (AlignType) a;

           if (ok && this.LookAhead(new Token[] { Token.INTEGER, Token.BYTE }))
            {
				int extended = 0;
                ok = ok && ParseInt(out extended);
                if (ok && extended == 0)
					align &= ~AlignType.DT_EX_VCENTER_LABEL;
            }

			return ok && ParseSep();
		}

		//------------------------------------------------------------------------------
		public bool ParseWidth (out int width)
		{
			width = 1;
			return
				ParseTag    (Token.WIDTH) &&
				ParseInt    (out width);
		}

		//------------------------------------------------------------------------------
		public bool ParseSize (out int size)
		{
			size = 1;
			return
				ParseTag    (Token.SIZE) &&
				ParseInt    (out size);
		}

		//------------------------------------------------------------------------------
		public bool ParseFont(out string styleName)
		{
			styleName = DefaultFont.None;
			return
				ParseTag(Token.FONTSTYLE) &&
				ParseString(out styleName) &&
				ParseSep();
		}

		//------------------------------------------------------------------------------
		public bool ParseTR (Token tag, out int row, out int col)
		{
			row = 0;
			col = 0;

			return
				ParseTag    (tag)&&
				ParseOpen   () &&
				ParseInt    (out row) &&
				ParseComma  () &&
				ParseInt    (out col) &&
				ParseClose  ();
		}

        public bool ParseTable(out int row, out int col)
        {
            return ParseTR(Token.TABLE, out row, out col);
        }

        public bool ParseRepeater(out int row, out int col)
        {
            return ParseTR(Token.REPEATER, out row, out col);
        }

		//------------------------------------------------------------------------------
		public bool ParseColumn (out string title)
		{
			return ParseString(out title);
		}

		//------------------------------------------------------------------------------
		public bool ParseOrigin (out Point origin)
		{
			int x = 0;
			int y = 0;

			bool ok=
				ParseTag        (Token.ORIGIN) &&
				ParseOpen       () &&
				ParseSignedInt  (out x) &&
				ParseComma      () &&
				ParseSignedInt  (out y) &&
				ParseClose      ();

			origin = new Point(x, y);
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseHeights
			(
				out int   title,
				out int   columnTitle,
				out int   cell,
				out int   total
			)
		{
			title = 0;
			columnTitle = 0;
			cell = 0;
			total = 0;

			return
				ParseTag    (Token.HEIGHTS) &&
				ParseOpen   () &&
				ParseInt    (out title) &&
				ParseComma  () &&
				ParseInt    (out columnTitle) &&
				ParseComma  () &&
				ParseInt    (out cell) &&
				ParseComma  () &&
				ParseInt    (out total) &&
				ParseClose  ();
		}

		//------------------------------------------------------------------------------
		public bool ParseFormat(out string styleName)
		{
            return ParseFormat(out styleName, true);
		}

        //------------------------------------------------------------------------------
        public bool ParseFormat(out string styleName, bool checkTag)
        {
            styleName = DefaultFormat.None;
            if (checkTag && !LookAhead(Token.FORMATSTYLE))
                return true;

            if (checkTag && !ParseTag(Token.FORMATSTYLE))
                return false;

            return ParseString(out styleName);
        }

		//------------------------------------------------------------------------------
		public bool ParseBarCode (BarCode barCode)
		{
			bool ok = true;
			
			SkipToken();	// salto il token BarCode...
			if (!LookAhead(Token.ROUNDOPEN))
				return true;

			string s; int i; bool b = true;
			SkipToken();
			
			if (LookAhead(Token.TEXTSTRING))
			{
				//parsa la stringa che dice il tipo di barcode
				ok = ParseString(out s); 
				barCode.BarCodeTypeName = s; 
				barCode.BarCodeType = BarCodeWrapper.GetBarCodeType(s);
			}
			else
			{
				//parsa l'alias della variabile che contiene il tipo di barcode
				ok = ParseUShort(out barCode.BarCodeTypeAlias);
			}

			if (ok && Matched(Token.COMMA)) {ok = ParseSignedInt(out i); barCode.NarrowBar = (short)i;}
			if (ok && Matched(Token.COMMA)) {ok = ParseBool(out b); barCode.Vertical = b;}
			if (ok && Matched(Token.COMMA)) {ok = ParseBool(out b); barCode.ShowLabel = b;}
			
			// EAN128 optional parameters
			if (ok && LookAhead(Token.COMMA))
			{
				Matched(Token.COMMA);
				ok = ParseSignedInt(out i); 
				barCode.CheckSumType = i;
			}

			if (ok && LookAhead(Token.COMMA))
			{
				Matched(Token.COMMA);
				ok = ParseSignedInt(out i); 
				barCode.CustomBarHeight = i;
			}

			if (ok && LookAhead(Token.COMMA))
			{
				Matched(Token.COMMA);
				ok = ParseInt(out i); 
				barCode.HumanTextAlias = Convert.ToUInt16(i);
			}

            // TODO in futuro parsing degli parametri aggiuntivi che attualmente non sono supportati dalla libreria bwipjs

            while (!MatchedNext(Token.ROUNDCLOSE)) {}

            //Matched(Token.ROUNDCLOSE);
            //ok = ok && ParseClose();
			
			return ok;
		}
	
	}
}
