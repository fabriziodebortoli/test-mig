using System;

using Microarea.Common.Generic;
using Microarea.Common.Applications;
using Microarea.Common.Lexan;

namespace Microarea.RSWeb.WoormViewer
{
	/// <summary>
	/// Summary description for Borders.
	/// </summary>
	public class TableBorders
	{
        public Borders TableTitle = new Borders();
        public Borders ColumnTitle = new Borders();
        public Borders Body = new Borders();
        public Borders Total = new Borders();

        public bool ColumnTitleSeparator = false;
        public BorderPen ColumnTitleSepPen = new BorderPen();
        public bool ColumnSeparator = false;
        public BorderPen ColumnSepPen = new BorderPen();

		public bool RowSeparator = false;
		public bool DynamicRowSeparator = false;
		public BorderPen RowSepPen = new BorderPen();
        public BorderPen InterlinePen = new BorderPen();

        //------------------------------------------------------------------------------
        public TableBorders()
		{
			Init(true);
			RowSeparator = false;

			//riporto cs#74193 da woorm c++ - Aggiunta nuovi bordi
			TableTitle.Bottom = false;
			ColumnTitle.Bottom = false;
			Total.Top = false;
		}

		//------------------------------------------------------------------------------
		public TableBorders(bool enabled)
		{
			Init(enabled);
			RowSeparator = false;

			//riporto cs#74193 da woorm c++ - Aggiunta nuovi bordi sulla tabella
			TableTitle.Bottom = false;
			ColumnTitle.Bottom = false;
			Total.Top = false;
		}

		//------------------------------------------------------------------------------
		internal void Init(bool enabled)
		{
			TableTitle.Top   = enabled;
			TableTitle.Left  = enabled;
			TableTitle.Bottom = enabled;
			TableTitle.Right = enabled;

			ColumnTitle.Top      = enabled;
			ColumnTitle.Left     = enabled;
			ColumnTitle.Bottom	= enabled;
			ColumnTitle.Right    = enabled;
			ColumnTitleSeparator= enabled;

			RowSeparator    = enabled;
			ColumnSeparator = enabled;

			Body.Top     = enabled;
			Body.Left    = enabled;
			Body.Bottom  = enabled;
			Body.Right   = enabled;

			Total.Top	= enabled;
			Total.Left   = enabled;
			Total.Bottom = enabled;
			Total.Right  = enabled;
		}

		//------------------------------------------------------------------------------
		public bool IsDefault()
		{
			TableBorders brd = new TableBorders();
			return brd.Equals(this);
		}

		//------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return (this == (TableBorders)obj);
		}

		//------------------------------------------------------------------------------
		public static bool operator !=(TableBorders e1,TableBorders e2)
		{
			return !(e1 == e2);
		}

		//------------------------------------------------------------------------------
		public static bool operator ==(TableBorders e1,TableBorders e2)
		{
			if (Object.ReferenceEquals(e1,e2))
				return true;

			if (Object.ReferenceEquals(e1,null) || Object.ReferenceEquals(e2,null))
				return false;

			return
				e1.Body.Bottom == e2.Body.Bottom &&
				e1.Body.Left == e2.Body.Left &&
				e1.Body.Right == e2.Body.Right &&
				e1.Body.Top == e2.Body.Top &&
				e1.ColumnTitle.Bottom == e2.ColumnTitle.Bottom &&
				e1.ColumnTitle.Left == e2.ColumnTitle.Left &&
				e1.ColumnTitle.Right == e2.ColumnTitle.Right &&
				e1.ColumnTitle.Top == e2.ColumnTitle.Top &&

				e1.ColumnSeparator == e2.ColumnSeparator &&
                e1.ColumnSepPen == e2.ColumnSepPen &&
				e1.ColumnTitleSeparator == e2.ColumnTitleSeparator &&
                e1.ColumnTitleSepPen == e2.ColumnTitleSepPen &&

				e1.RowSeparator == e2.RowSeparator &&
				e1.RowSepPen == e2.RowSepPen &&
                e1.InterlinePen == e2.InterlinePen &&

                e1.TableTitle.Bottom == e2.TableTitle.Bottom &&
				e1.TableTitle.Left == e2.TableTitle.Left &&
				e1.TableTitle.Right == e2.TableTitle.Right &&
				e1.TableTitle.Top == e2.TableTitle.Top &&
				e1.Total.Bottom == e2.Total.Bottom &&
				e1.Total.Left == e2.Total.Left &&
				e1.Total.Right == e2.Total.Right &&
				e1.Total.Top == e2.Total.Top;
		}

		//------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//------------------------------------------------------------------------------
		public int Changed()
		{
			int counter = 0;

			if (!TableTitle.Top)     counter++;
			if (TableTitle.Bottom)	counter++;
			if (!TableTitle.Left)    counter++;
			if (!TableTitle.Right)   counter++;

			if (!ColumnTitle.Top)        counter++;
			if (ColumnTitle.Bottom)		counter++;
			if (!ColumnTitle.Left)       counter++;
			if (!ColumnTitle.Right)      counter++;

			if (!ColumnTitleSeparator)  counter++;
			if (!ColumnSeparator)   counter++;

			if (RowSeparator)       counter++;

			if (!Body.Top)       counter++;
			if (!Body.Left)      counter++;
			if (!Body.Bottom)    counter++;
			if (!Body.Right)     counter++;

			if (!Total.Left)     counter++;
			if (Total.Top)		counter++;
			if (!Total.Bottom)   counter++;
			if (!Total.Right)    counter++;

			return counter;
		}

		//------------------------------------------------------------------------------
		internal bool ParseBorder(WoormParser lex, bool blk)
		{
			bool ok = true;

			do
			{
				switch (lex.LookAhead())
				{
					case Token.EOF: lex.SetError(WoormViewerStrings.WoormViewerErrorUnexpectedEof); ok = false; break;

					case Token.NO_TTT			: lex.SkipToken(); TableTitle.Top		= false;    break;
					case Token.NO_TTL			: lex.SkipToken(); TableTitle.Left		= false;    break;
					case Token.NO_TTR			: lex.SkipToken(); TableTitle.Right		= false;    break;
					case Token.TITLE_BOTTOM		: lex.SkipToken(); TableTitle.Bottom	= true;		break;

					case Token.NO_CTT			: lex.SkipToken(); ColumnTitle.Top		= false;    break;
					case Token.NO_CTL			: lex.SkipToken(); ColumnTitle.Left		= false;    break;
					case Token.NO_CTR			: lex.SkipToken(); ColumnTitle.Right	= false;    break;
					case Token.COLTITLE_BOTTOM	: lex.SkipToken(); ColumnTitle.Bottom	= true;		break;

					case Token.NO_CTS			: 
                                            {
                                                lex.SkipToken();
                                                ColumnTitleSeparator = false;

                                                if (lex.LookAhead(Token.PEN))
                                                {
                                                    ColumnTitleSepPen = new BorderPen();
                                                    ok = ok && lex.ParsePen(ColumnTitleSepPen);
                                                    if (ColumnTitleSepPen.Width > 0)
                                                        ColumnTitleSeparator = true;
                                                }
                                                break;
                                            }

                    case Token.NO_CSE   : 
                                           { 
                                                lex.SkipToken(); 
                                                ColumnSeparator = false; 

                                                if (lex.LookAhead(Token.PEN))
                                                {
                                                    ColumnSepPen = new BorderPen();
                                                    ok = ok && lex.ParsePen(ColumnSepPen);
                                                    if (ColumnSepPen.Width > 0)
                                                        ColumnSeparator = true;
                                                }
                                                break; 
                                          }

					case Token.YE_RSE   : 
										  lex.SkipToken(); 
										  RowSeparator			= true;   
  
										  DynamicRowSeparator = lex.Matched(Token.DYNAMIC);

										  if (lex.LookAhead(Token.PEN))
										  {
											  RowSepPen = new BorderPen();
											  ok = ok && lex.ParsePen(RowSepPen);
										  }
										  break;
                    case Token.INTERLINE:
                                        lex.SkipToken();
                                        InterlinePen = new BorderPen();
                                        ok = ok && lex.ParsePen(RowSepPen);
                                        break;

                    case Token.NO_BOT   : lex.SkipToken(); Body.Top				= false;    break;
					case Token.NO_BOR   : lex.SkipToken(); Body.Right			= false;    break;
					case Token.NO_BOL   : lex.SkipToken(); Body.Left			= false;    break;
					case Token.NO_BOB   : lex.SkipToken(); Body.Bottom			= false;    break;

					case Token.TOTAL_TOP: lex.SkipToken(); Total.Top			= true;		break;
					case Token.NO_TOR   : lex.SkipToken(); Total.Right			= false;    break;
					case Token.NO_TOL   : lex.SkipToken(); Total.Left			= false;    break;
					case Token.NO_TOB   : lex.SkipToken(); Total.Bottom			= false;    break;

					case Token.END :
						if (blk) return ok;
						lex.SetError(WoormViewerStrings.UnexpectedEnd);
						return false;

					default :
						if (blk)
						{
							lex.SetError(WoormViewerStrings.EndNotFound);
							ok = false;
						}
						break;
				}
			}
			while (ok && blk);

			return ok;
		}

		//------------------------------------------------------------------------------
		internal bool ParseBorders (WoormParser lex)
		{
			bool ok = true;

			do { ok = ParseBorder(lex, true) && !lex.Error && !lex.Eof; }
			while (ok && !lex.LookAhead(Token.END));

			return ok;
		}


		//------------------------------------------------------------------------------
		internal bool ParseBlock(WoormParser lex)
		{
			if (lex.LookAhead(Token.BEGIN))
				return
	
			lex.ParseBegin  ()      &&
			ParseBorders    (lex)   &&
			lex.ParseEnd    ();

			return ParseBorder(lex, false);
		}

		//------------------------------------------------------------------------------
		public bool Parse(WoormParser lex)
		{
			// don't exist option section so standard value
			bool ok =true;

			// try if all m_Borders are off
			if (lex.LookAhead(Token.NO_BORDERS))
			{
				ok = lex.ParseTag (Token.NO_BORDERS);
				if (ok) Init (false);
				return ok;
			}

			// parse eventual m_Borders flags
			if (lex.LookAhead(Token.BORDERS))
				ok = (lex.ParseTag(Token.BORDERS) && ParseBlock (lex));

			return ok;
		}

        //------------------------------------------------------------------------------
        public bool Unparse(Unparser unparser)
        {
            if (IsDefault())
                return true;

            bool blk = (Changed() > 1);

            unparser.WriteTag(Token.BORDERS, false);
            unparser.WriteBlank();

            if (blk)
                unparser.WriteLine();

            if (blk) unparser.WriteBegin();
            if (!TableTitle.Top) unparser.WriteTag(Token.NO_TTT);
            if (TableTitle.Bottom) unparser.WriteTag(Token.TITLE_BOTTOM);
            if (!TableTitle.Left) unparser.WriteTag(Token.NO_TTL);
            if (!TableTitle.Right) unparser.WriteTag(Token.NO_TTR);

            if (!ColumnTitle.Top) unparser.WriteTag(Token.NO_CTT);
            if (ColumnTitle.Bottom) unparser.WriteTag(Token.COLTITLE_BOTTOM);
            if (!ColumnTitle.Left) unparser.WriteTag(Token.NO_CTL);
            if (!ColumnTitle.Right) unparser.WriteTag(Token.NO_CTR);

            if (!ColumnTitleSeparator) unparser.WriteTag(Token.NO_CTS);
            if (!ColumnSeparator) unparser.WriteTag(Token.NO_CSE);

            if (RowSeparator || DynamicRowSeparator)
            {
                unparser.WriteTag(Token.YE_RSE, false);
                if (DynamicRowSeparator)
                    unparser.WriteTag(Token.DYNAMIC, false);
                if (RowSepPen != null)
                    unparser.WritePen(RowSepPen, false);
                unparser.WriteLine();
            }

            if (!Body.Top) unparser.WriteTag(Token.NO_BOT);
            if (!Body.Bottom) unparser.WriteTag(Token.NO_BOB);
            if (!Body.Left) unparser.WriteTag(Token.NO_BOL);
            if (!Body.Right) unparser.WriteTag(Token.NO_BOR);

            if (Total.Top) unparser.WriteTag(Token.TOTAL_TOP);
            if (!Total.Bottom) unparser.WriteTag(Token.NO_TOB);
            if (!Total.Left) unparser.WriteTag(Token.NO_TOL);
            if (!Total.Right) unparser.WriteTag(Token.NO_TOR);
            if (blk) unparser.WriteEnd();

            return true;
        }
       //------------------------------------------------------------------------------
        /*
        public string ToJson(string name = "tableborders", bool bracket = false)
        {
            string s = name.ToJson() + ":{" +
                                TableTitle.ToJson("table_title") + ',' +
                                ColumnTitle.ToJson("column_title") + ',' +
                                Body.ToJson("body") + ',' +
                                Total.ToJson("total") + ',' +
                                ColumnTitleSeparator.ToJson("column_title_sep") + ',' +
                                ColumnSeparator.ToJson("column_sep") + ',' +
                                RowSeparator.ToJson("row_sep") + ',' +
                                DynamicRowSeparator.ToJson("row_sep_dynamic") + ',' +
                                RowSepPen.ToJson("row_sep_pen") +
                                '}';
            / *
                    if (this.Borders.ColumnTitleSeparator && this.Borders.ColumnTitleSepPen != null && this.Borders.ColumnTitleSepPen.Width > 0) 
                    s += this.Borders.ColumnTitleSepPen.ToJson("ColumnTitleSepPen") +  ',';
                if (this.Borders.ColumnSeparator && this.Borders.ColumnSepPen != null && this.Borders.ColumnSepPen.Width > 0)
                    s += this.Borders.ColumnSepPen.ToJson("ColumnSepPen") + ',';
                if (this.Borders.RowSeparator && this.Borders.RowSepPen != null && this.Borders.RowSepPen.Width > 0)
                    s += this.Borders.RowSepPen.ToJson("RowSepPen") + ',';

             * /
            if (bracket)
                s = '{' + s + '}';

            return s;
        }
*/
    }
}