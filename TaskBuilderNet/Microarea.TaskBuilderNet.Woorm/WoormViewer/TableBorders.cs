using System;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Lexan;

namespace Microarea.TaskBuilderNet.Woorm.WoormViewer
{
	/// <summary>
	/// Summary description for Borders.
	/// </summary>
	public class TableBorders
	{
		public bool TableTitleTop;
		public bool TableTitleLeft;
		public bool TableTitleBottom;
		public bool TableTitleRight;

		public bool ColumnTitleTop;
		public bool ColumnTitleLeft;
		public bool ColumnTitleBottom;
		public bool ColumnTitleRight;
		public bool ColumnTitleSeparator;

		public bool RowSeparator;
		public bool DynamicRowSeparator;
		public bool ColumnSeparator;
		public BorderPen RowSepPen;

		public bool BodyTop;
		public bool BodyLeft;
		public bool BodyBottom;
		public bool BodyRight;

		public bool TotalTop;
		public bool TotalLeft;
		public bool TotalBottom;
		public bool TotalRight;

		//------------------------------------------------------------------------------
		public TableBorders()
		{
			Init(true);
			RowSeparator = false;

			//riporto cs#74193 da woorm c++ - Aggiunta nuovi bordi
			TableTitleBottom = false;
			ColumnTitleBottom = false;
			TotalTop = false;
		}

		//------------------------------------------------------------------------------
		public TableBorders(bool enabled)
		{
			Init(enabled);
			RowSeparator = false;

			//riporto cs#74193 da woorm c++ - Aggiunta nuovi bordi sulla tabella
			TableTitleBottom = false;
			ColumnTitleBottom = false;
			TotalTop = false;
		}

		//------------------------------------------------------------------------------
		internal void Init(bool enabled)
		{
			TableTitleTop   = enabled;
			TableTitleLeft  = enabled;
			TableTitleBottom = enabled;
			TableTitleRight = enabled;

			ColumnTitleTop      = enabled;
			ColumnTitleLeft     = enabled;
			ColumnTitleBottom	= enabled;
			ColumnTitleRight    = enabled;
			ColumnTitleSeparator= enabled;

			RowSeparator    = enabled;
			ColumnSeparator = enabled;

			BodyTop     = enabled;
			BodyLeft    = enabled;
			BodyBottom  = enabled;
			BodyRight   = enabled;

			TotalTop	= enabled;
			TotalLeft   = enabled;
			TotalBottom = enabled;
			TotalRight  = enabled;
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
				e1.BodyBottom == e2.BodyBottom &&
				e1.BodyLeft == e2.BodyLeft &&
				e1.BodyRight == e2.BodyRight &&
				e1.BodyTop == e2.BodyTop &&
				e1.ColumnSeparator == e2.ColumnSeparator &&
				e1.ColumnTitleBottom == e2.ColumnTitleBottom &&
				e1.ColumnTitleLeft == e2.ColumnTitleLeft &&
				e1.ColumnTitleRight == e2.ColumnTitleRight &&
				e1.ColumnTitleSeparator == e2.ColumnTitleSeparator &&
				e1.ColumnTitleTop == e2.ColumnTitleTop &&
				e1.RowSeparator == e2.RowSeparator &&
				e1.RowSepPen == e2.RowSepPen &&
				e1.TableTitleBottom == e2.TableTitleBottom &&
				e1.TableTitleLeft == e2.TableTitleLeft &&
				e1.TableTitleRight == e2.TableTitleRight &&
				e1.TableTitleTop == e2.TableTitleTop &&
				e1.TotalBottom == e2.TotalBottom &&
				e1.TotalLeft == e2.TotalLeft &&
				e1.TotalRight == e2.TotalRight &&
				e1.TotalTop == e2.TotalTop;
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

			if (!TableTitleTop)     counter++;
			if (TableTitleBottom)	counter++;
			if (!TableTitleLeft)    counter++;
			if (!TableTitleRight)   counter++;

			if (!ColumnTitleTop)        counter++;
			if (ColumnTitleBottom)		counter++;
			if (!ColumnTitleLeft)       counter++;
			if (!ColumnTitleRight)      counter++;
			if (!ColumnTitleSeparator)  counter++;

			if (RowSeparator)       counter++;
			if (!ColumnSeparator)   counter++;

			if (!BodyTop)       counter++;
			if (!BodyLeft)      counter++;
			if (!BodyBottom)    counter++;
			if (!BodyRight)     counter++;

			if (!TotalLeft)     counter++;
			if (TotalTop)		counter++;
			if (!TotalBottom)   counter++;
			if (!TotalRight)    counter++;

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
					case Token.NO_TTT			: lex.SkipToken(); TableTitleTop		= false;    break;
					case Token.NO_TTL			: lex.SkipToken(); TableTitleLeft		= false;    break;
					case Token.NO_TTR			: lex.SkipToken(); TableTitleRight		= false;    break;
					case Token.TITLE_BOTTOM		: lex.SkipToken(); TableTitleBottom		= true;		break;
					case Token.NO_CTT			: lex.SkipToken(); ColumnTitleTop		= false;    break;
					case Token.NO_CTL			: lex.SkipToken(); ColumnTitleLeft		= false;    break;
					case Token.NO_CTR			: lex.SkipToken(); ColumnTitleRight		= false;    break;
					case Token.NO_CTS			: lex.SkipToken(); ColumnTitleSeparator	= false;    break;
					case Token.COLTITLE_BOTTOM	: lex.SkipToken(); ColumnTitleBottom	= true;		break;
					case Token.YE_RSE   : 
										  lex.SkipToken(); 
										  RowSeparator			= true;     
										  DynamicRowSeparator = lex.Parsed(Token.DYNAMIC);

										  if (lex.LookAhead(Token.PEN))
										  {
											  RowSepPen = new BorderPen();
											  ok = ok && lex.ParsePen(RowSepPen);
										  }
										  break;

					case Token.NO_CSE   : lex.SkipToken(); ColumnSeparator		= false;    break;
					case Token.NO_BOT   : lex.SkipToken(); BodyTop				= false;    break;
					case Token.NO_BOR   : lex.SkipToken(); BodyRight			= false;    break;
					case Token.NO_BOL   : lex.SkipToken(); BodyLeft				= false;    break;
					case Token.NO_BOB   : lex.SkipToken(); BodyBottom			= false;    break;
					case Token.TOTAL_TOP: lex.SkipToken(); TotalTop				= true;		break;
					case Token.NO_TOR   : lex.SkipToken(); TotalRight			= false;    break;
					case Token.NO_TOL   : lex.SkipToken(); TotalLeft			= false;    break;
					case Token.NO_TOB   : lex.SkipToken(); TotalBottom			= false;    break;

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
			if (!TableTitleTop) unparser.WriteTag(Token.NO_TTT);
			if (TableTitleBottom) unparser.WriteTag(Token.TITLE_BOTTOM);
			if (!TableTitleLeft) unparser.WriteTag(Token.NO_TTL);
			if (!TableTitleRight) unparser.WriteTag(Token.NO_TTR);

			if (!ColumnTitleTop) unparser.WriteTag(Token.NO_CTT);
			if (ColumnTitleBottom) unparser.WriteTag(Token.COLTITLE_BOTTOM);
			if (!ColumnTitleLeft) unparser.WriteTag(Token.NO_CTL);
			if (!ColumnTitleRight) unparser.WriteTag(Token.NO_CTR);

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

			if (!BodyTop) unparser.WriteTag(Token.NO_BOT);
			if (!BodyBottom) unparser.WriteTag(Token.NO_BOB);
			if (!BodyLeft) unparser.WriteTag(Token.NO_BOL);
			if (!BodyRight) unparser.WriteTag(Token.NO_BOR);

			if (TotalTop) unparser.WriteTag(Token.TOTAL_TOP);
			if (!TotalBottom) unparser.WriteTag(Token.NO_TOB);
			if (!TotalLeft) unparser.WriteTag(Token.NO_TOL);
			if (!TotalRight) unparser.WriteTag(Token.NO_TOR);
			if (blk) unparser.WriteEnd();

			return true;
		}
	}
}