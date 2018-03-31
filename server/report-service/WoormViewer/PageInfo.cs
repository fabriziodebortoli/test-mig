using System.Drawing;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;
using Microarea.Common.Generic;

namespace Microarea.RSWeb.WoormViewer
{
	/// <summary>
	/// Summary description for PageInfo.
	/// </summary>
	/// ================================================================================
	public class PageInfo
	{
		// definite in wingdi.h in C++
		public const int DMORIENT_PORTRAIT = 1;
		public const int DMORIENT_LANDSCAPE = 2;
		public const int DMPAPER_A4 = 9;

		public const int DMCOLLATE_FALSE = 0;
		public const int DMCOLLATE_TRUE  = 1;

		public const uint DM_ORIENTATION	= 0x00000001;
		public const uint DM_PAPERSIZE		= 0x00000002;
		public const uint DM_SCALE			= 0x00000010;
		public const uint DM_COPIES			= 0x00000100;
		public const uint DM_COLLATE		= 0x00008000;

		// indica che non si seleziona un formato particolare ma dimensioni speciali
		private const int DMPAPER_SPECIAL	= 0;
		private const int A4_WIDTH			= 2100;
		private const int A4_HEIGHT			= 2970;
		private const int MU_DECIMAL		= 3;
		private const double MU_SCALE		= 100.0;

		Rectangle margins = new Rectangle(0, 0, 0, 0); // in pixel
		
		Rectangle rect = new Rectangle(0, 0, 0, 0); // in pixel
		
		bool		usePrintableArea = true;

		//I valori di default devono essere allineati con quelli in 
		//c:\Development\Standard\TaskBuilder\Framework\TbWoormViewer\Pageinfo.cpp

		// copertina semplificata alla struttura DEVMODE 
		// uint  dmFields			= DM_ORIENTATION | DM_PAPERSIZE | DM_SCALE | DM_COPIES | DM_COLLATE;
		short dmOrientation		= DMORIENT_PORTRAIT;
		short dmPaperSize		= DMPAPER_A4;
		
		short dmPaperLength		= A4_HEIGHT; // in decimi di millimetro
		short dmPaperWidth		= A4_WIDTH;	 // in decimi di millimetro

		short dmScale			= 100;
		short dmCopies			= 1;
		short dmCollate			= DMCOLLATE_TRUE;

		//attributes not used in Easylook, used only for Z-print in woorm c++. Here they are only parsed
		PrinterPageInfo printerPageInfo = new PrinterPageInfo();
		//ArrayList		arHPageSplitter = new ArrayList();
		//ArrayList		arVPageSplitter = new ArrayList();

		public Rectangle Margins	{ get { return margins; } }
		public Rectangle Rect		{ get { return rect; } set { rect = value; } }
		public bool Landscape		{ get { return dmOrientation == DMORIENT_LANDSCAPE; } }
		public short DmPaperLength	{ get { return dmPaperLength; } set { dmPaperLength = value;} }
		public short DmPaperWidth	{ get { return dmPaperWidth; } set { dmPaperWidth = value;} }
		public bool IsDefault		{ get; set; }

		// parsa la vecchia sintassi tenendo conto che deve passare da pixel in 
		// decimi di millimetro come previsto dalla struttura DEVMODE
		//--------------------------------------------------------------------------
		private bool OldParse(Parser lex)
		{
			bool ok = true;
			int	cx = 0;
			int cy = 0;
			
			ok = lex.ParseInt(out cx) && lex.ParseInt(out cy);

			if (ok)
			{
				dmOrientation = DMORIENT_PORTRAIT;
				if (lex.LookAhead(Token.LANDSCAPE))
				{
					lex.SkipToken();
					dmOrientation = DMORIENT_LANDSCAPE;

					// compatibilita' TaskBuilder rel. 1.1 (valori in pixel video)
					if (cx == 1122 && cy == 794)
					{
						dmPaperSize		= DMPAPER_A4;
						dmPaperWidth	= A4_HEIGHT;
						dmPaperLength	= A4_WIDTH;
			
						return ok;
					}
				}
			}

			// in decimi di millimetro
			dmPaperSize		= DMPAPER_SPECIAL;
			dmPaperWidth	= (short)UnitConvertions.LPtoMU(cx, UnitConvertions.MeasureUnits.CM, MU_SCALE, MU_DECIMAL);
			dmPaperLength	= (short)UnitConvertions.LPtoMU(cy, UnitConvertions.MeasureUnits.CM, MU_SCALE, MU_DECIMAL);
			
			return ok;
		}

		//--------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			if (!lex.LookAhead(Token.PAGE_INFO))
				return true;
			
			lex.SkipToken();

			// controlla il vecchio formato
			if (lex.NextTokenIsInt)
				return OldParse(lex);

			bool ok = 
				lex.ParseOpen	() &&
				lex.ParseShort	(out dmOrientation)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperSize)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperWidth)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperLength)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmScale)			&& lex.ParseComma() &&
				lex.ParseShort	(out dmCopies)			&& lex.ParseComma() &&
				lex.ParseShort	(out dmCollate)			&& lex.ParseComma() &&
				lex.ParseBool	(out usePrintableArea)	&&
				lex.ParseClose	();

			if (ok && lex.LookAhead(Token.MARGINS))
			{
				int top = 0;
				int left = 0;
				int right = 0;
				int bottom = 0;
				ok =	lex.ParseTag        (Token.MARGINS) &&
						lex.ParseOpen       ()				&&
						lex.ParseSignedInt  (out top)       &&	lex.ParseComma () &&
						lex.ParseSignedInt  (out left)      &&	lex.ParseComma () &&
						lex.ParseSignedInt  (out bottom)    &&	lex.ParseComma () &&
						lex.ParseSignedInt  (out right)     &&
						lex.ParseClose      ();

				margins.Y = top;
				margins.X = left;
				margins.Width = right -left;
				margins.Height = bottom - top;
			}

            if (ok && lex.LookAhead(Token.PAGE_PRINTER_INFO))
            {
                printerPageInfo.Parse(lex);
            }
            else
            {
                printerPageInfo.dmOrientation   = dmOrientation;
                printerPageInfo.dmPaperLength   = dmPaperLength;
                printerPageInfo.dmPaperSize     = dmPaperSize;
                printerPageInfo.dmPaperWidth    = dmPaperWidth;
            }

            /*short splitter = 0;
			if (ok && lex.LookAhead(Token.PAGE_HSPLITTER))
			{	
				lex.SkipToken();
				ok = lex.ParseOpen();
				do
				{
					ok = lex.ParseShort(out splitter);
					arHPageSplitter.Add(splitter);
				}while (lex.Matched(Token.COMMA) && ok);
				ok = ok && lex.ParseClose();
			}

			if (ok && lex.LookAhead(Token.PAGE_VSPLITTER))
			{
				lex.SkipToken();
				ok = lex.ParseOpen();
				do
				{
					ok = lex.ParseShort(out splitter);
					arVPageSplitter.Add(splitter);
				}while (lex.Matched(Token.COMMA) && ok);

				ok = ok && lex.ParseClose();
			}*/

            return ok;
		}

		//--------------------------------------------------------------------------
		internal bool Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.PAGE_INFO, false);
			unparser.WriteOpen(false);
			unparser.Write(dmOrientation, false); unparser.WriteComma(false);
			unparser.Write(dmPaperSize, false); unparser.WriteComma(false);
			unparser.Write(dmPaperWidth, false); unparser.WriteComma(false);
			unparser.Write(dmPaperLength, false); unparser.WriteComma(false);
			unparser.Write(dmScale, false); unparser.WriteComma(false);
			unparser.Write(dmCopies, false); unparser.WriteComma(false);
			unparser.Write(dmCollate, false); unparser.WriteComma(false);
			unparser.Write(usePrintableArea, false);
			unparser.WriteClose(false);

			// Parsa i margini solo se ci sono e se devo usarli
			if (!margins.IsEmpty && !usePrintableArea)
			{
				unparser.WriteTag(Token.MARGINS, false);
				unparser.WriteOpen(false);
				unparser.Write(margins.Top, false); unparser.WriteComma(false);
				unparser.Write(margins.Left, false); unparser.WriteComma(false);
				unparser.Write(margins.Bottom, false); unparser.WriteComma(false);
				unparser.Write(margins.Right, false);
				unparser.WriteClose();
			}

			unparser.WriteLine();

			if (
				
				//inutile m_PrinterPageInfo.dmOrientation	!= dmOrientation ||
					printerPageInfo.dmPaperSize != dmPaperSize ||
					printerPageInfo.dmPaperWidth != dmPaperWidth ||
					printerPageInfo.dmPaperLength != dmPaperLength
				)
				printerPageInfo.Unparse(unparser);

			return true;
		}

        //---------------------------------------------------------------
        public string ToJson(bool invert, string name = "pageinfo", bool bracket = false)
        {
            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{';

            //TODO RSWEB - pagina a video > pagina in stampa (per impostare la pagina fisica occorre scalare tutte le dimensioni)
            //int l = this.printerPageInfo.dmPaperLength / 10 ;
            //int w = this.printerPageInfo.dmPaperWidth / 10 ;
            int l = this.dmPaperLength / 10;
            int w = this.dmPaperWidth / 10;
            //----

            if (invert)
            {
                int t = l;
                l = w;
                w = t;
            }

            s += l.ToJson("length") + ',';
            s += w.ToJson("width") + ',';

            s += this.Margins.ToJson("margin");
                       
            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s; ;
        }

    }

    /// <summary>
    /// Summary description for PrinterPageInfo.
    /// Class not used in Easylook, used only for Z-print in woorm c++. Here he is only parsed
    /// </summary>
    /// ================================================================================
    public class PrinterPageInfo
	{
		bool  UseCloningPrint	= true;

		public short dmOrientation		= PageInfo.DMORIENT_PORTRAIT;
		public short dmPaperSize        = PageInfo.DMPAPER_A4;
		public short dmPaperLength      = 2100;		//	A4_WIDTH	in decimi di millimetro
		public short dmPaperWidth       = 2970;		// 	A4_HEIGHT	in decimi di millimetro
	
		//--------------------------------------------------------------------------
		public bool Parse(Parser lex)
		{
			if (!lex.LookAhead(Token.PAGE_PRINTER_INFO))
				return true;
			
			lex.SkipToken();

			bool ok = 
				lex.ParseOpen	() &&

				lex.ParseShort	(out dmOrientation)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperSize)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperWidth)		&& lex.ParseComma() &&
				lex.ParseShort	(out dmPaperLength)		&& lex.ParseComma() &&
			
				lex.ParseBool	(out UseCloningPrint)	&&

				lex.ParseClose	();

			return ok;
		}

		//--------------------------------------------------------------------------
		public void Unparse(Unparser unparser)
		{
			unparser.WriteTag(Token.PAGE_PRINTER_INFO, false);
			unparser.WriteOpen(false);
            //TODO orientation
			unparser.Write(1, false); unparser.WriteComma(false);
            //----
			unparser.Write(dmPaperSize, false); unparser.WriteComma(false);
			unparser.Write(dmPaperWidth, false); unparser.WriteComma(false);
			unparser.Write(dmPaperLength, false); unparser.WriteComma(false);

			unparser.Write(UseCloningPrint, false);

			unparser.WriteClose();
		}

	
	}

}
