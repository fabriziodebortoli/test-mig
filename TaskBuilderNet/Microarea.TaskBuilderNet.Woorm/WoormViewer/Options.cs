using System.Drawing;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;

namespace Microarea.TaskBuilderNet.Woorm.WoormViewer
{
	/// <summary>
	/// Summary description for Options.
	/// </summary>
	/// ================================================================================
	public class Options
	{
		// nome del bitmap di sfondo
		string		bkgnBitmap;
		Point		bitmapOrigin = new Point(0,0);

		// printer flags
		bool	prnNoBitmap = false;
		bool	prnNoBorders = false;
		bool	prnShowLabels = true;
		bool	prnShowTitles = true;
		bool	prnUseDraftFont = false;

		/// console (video) flags
		bool	conNoBitmap = false;
		bool	conNoBorders = false;
		bool	conShowLabels = true;
		bool	conShowTitles = true;

		// Schema
		bool	createSchema1 = false;
		bool	createSchema2 = false;

		public	bool CreateSchema1 { get { return createSchema1; }}
		public bool CreateSchema2 { get { return createSchema2; } }
        //TODO missing schema type: Standard, ElectronicInvoice

		// properties
		// nome del bitmap di sfondo
        public string BkgnBitmap { get { return bkgnBitmap; } set { bkgnBitmap = value; } }
        public Point BitmapOrigin { get { return bitmapOrigin; } set { bitmapOrigin = value; } }

		// printer flags
		public bool	PrnNoBitmap		{ get { return prnNoBitmap; }}
		public bool	PrnNoBorders	{ get { return prnNoBorders; }}
		public bool	PrnShowLabels	{ get { return prnShowLabels; }}
		public bool	PrnShowTitles	{ get { return prnShowTitles; }}
		public bool	PrnUseDraftFont	{ get { return prnUseDraftFont; }}

		/// console (video) flags
		public bool	ConNoBitmap		{ get { return conNoBitmap; }}
		public bool	ConNoBorders	{ get { return conNoBorders; }}
		public bool	ConShowLabels	{ get { return conShowLabels; }}
		public bool	ConShowTitles	{ get { return conShowTitles; }}

		//------------------------------------------------------------------------------
		private bool ParseOption(WoormParser lex, bool blk)
		{
			bool ok = true;
			do
			{
				switch (lex.LookAhead())
				{
					case Token.BITMAP: 
						{
							lex.SkipToken();
							ok = lex.ParseString(out bkgnBitmap);

							if (ok && lex.LookAhead(Token.ORIGIN))
								ok = lex.ParseOrigin(out bitmapOrigin);
							break;
						}

					case Token.EOF: lex.SetError(WoormViewerStrings.WoormViewerErrorUnexpectedEof); ok = false; break;

					case Token.NO_PRN_BKGN_BITMAP	: lex.SkipToken(); prnNoBitmap     = true;		break;
					case Token.NO_PRN_BORDERS		: lex.SkipToken(); prnNoBorders    = true;		break;
					case Token.NO_PRN_LABELS		: lex.SkipToken(); prnShowLabels   = false;		break;
					case Token.NO_PRN_TITLES		: lex.SkipToken(); prnShowTitles   = false;		break;
					case Token.USE_DRAFT_FONT		: lex.SkipToken(); prnUseDraftFont = true;		break;

					case Token.NO_CON_BKGN_BITMAP	: lex.SkipToken(); conNoBitmap     = true;		break;
					case Token.NO_CON_BORDERS		: lex.SkipToken(); conNoBorders    = true;		break;
					case Token.NO_CON_LABELS		: lex.SkipToken(); conShowLabels   = false;		break;
					case Token.NO_CON_TITLES		: lex.SkipToken(); conShowTitles   = false;		break;

                    case Token.CREATE_SCHEMA:
                        {
							int rel = 1;
							lex.SkipToken(); 

                            if (lex.LookAhead(Token.INTEGER))
								ok = lex.ParseInt(out rel);
							
							if (rel == 2)
								createSchema1 = true;
							else
								createSchema2 = true;
							break;
                        }

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
		private bool ParseOptions (WoormParser lex)
		{
			bool ok = true;
			do { ok = ParseOption(lex, true) && !lex.Error && !lex.Eof; }
			while (ok && !lex.LookAhead(Token.END));

			return ok;
		}


		//------------------------------------------------------------------------------
		private bool ParseBlock(WoormParser lex)
		{
			if (lex.LookAhead(Token.BEGIN))
				return
					lex.ParseBegin  () &&
					ParseOptions    (lex) &&
					lex.ParseEnd    ();

			return ParseOption(lex, false);
		}

		//------------------------------------------------------------------------------
		public bool Parse(WoormParser lex)
		{
			if (!lex.LookAhead(Token.OPTIONS))
				return true;
			
			lex.SkipToken();
			return ParseBlock (lex);
		}

		//------------------------------------------------------------------------------
		public bool Unparse(Unparser unparser)
		{
			//if standard value don't save
			if (Changed() == 0) 
			    return true;

			bool blk = (Changed() > 1);

			unparser.WriteTag    (Token.OPTIONS, false);
			if (blk)
				unparser.WriteLine();

			// begin block
			if (blk) unparser.WriteBegin ();

			
			if (!bkgnBitmap.IsNullOrEmpty())
			{
			    unparser.WriteTag (Token.BITMAP, false);
				
			    // salvo sempre il namespace completo
				NameSpace ns = new NameSpace(bkgnBitmap);
				NameSpace aFileNs = null;
				if (ns.IsValid())
					aFileNs = ns;
				//else if (IsDosName(bkgnBitmap))  //TODOLUCA
				//	aFileNs = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(bkgnBitmap) as NameSpace;

				if (
					aFileNs != null && aFileNs.IsValid() && 
					(aFileNs.NameSpaceType.Type == NameSpaceObjectType.Image || aFileNs.NameSpaceType.Type == NameSpaceObjectType.File)
					)
			        unparser.WriteString(aFileNs.ToString(), false);
			    else
			        unparser.WriteString(bkgnBitmap, false);

			    // se size diversa da 0,0,0,0 la unparsa altrimenti lascia come'
			    if (bitmapOrigin.X + bitmapOrigin.Y > 0)
			        unparser.WriteOrigin (bitmapOrigin);
			}
			
			if (prnNoBitmap)		unparser.WriteTag (Token.NO_PRN_BKGN_BITMAP);
			if (prnNoBorders)		unparser.WriteTag (Token.NO_PRN_BORDERS);
			if (!prnShowLabels)		unparser.WriteTag (Token.NO_PRN_LABELS);
			if (!prnShowTitles)		unparser.WriteTag (Token.NO_PRN_TITLES);
			if (prnUseDraftFont)	unparser.WriteTag (Token.USE_DRAFT_FONT);
			
			if (conNoBitmap)		unparser.WriteTag (Token.NO_CON_BKGN_BITMAP);
			if (conNoBorders)		unparser.WriteTag (Token.NO_CON_BORDERS);
			if (!conShowLabels)		unparser.WriteTag (Token.NO_CON_LABELS);
			if (!conShowTitles)		unparser.WriteTag (Token.NO_CON_TITLES);
			if (createSchema1 || createSchema2)	
			{ 
			    unparser.WriteTag (Token.CREATE_SCHEMA, false);
			    if (createSchema2)
			        unparser.Write (2);
			    else
			        unparser.WriteLine();
			}

			// write endig END block if needed
			if (blk) 
			    unparser.WriteEnd();
			
			unparser.WriteLine();

			return true;
		}

		//------------------------------------------------------------------------------
		private int Changed()
		{
			int counter = 0;

			if (!BkgnBitmap.IsNullOrEmpty()) counter++;
			if (BitmapOrigin.X + BitmapOrigin.Y > 0) counter++;

			if (PrnNoBitmap) counter++;
			if (PrnNoBorders) counter++;
			if (!PrnShowLabels) counter++;
			if (!PrnShowTitles) counter++;
			if (PrnUseDraftFont) counter++;

			if (ConNoBitmap) counter++;
			if (ConNoBorders) counter++;
			if (!ConShowLabels) counter++;
			if (!ConShowTitles) counter++;

			if (CreateSchema1|| CreateSchema2) counter++;

			return counter;
		}

		//------------------------------------------------------------------------------
		internal void SaveDefault(INameSpace ns)
		{
			if (!ns.IsValid()) 
				return;

			//CString sFilename = aNs.GetObjectName() + szSettingsExt;

			//// i settings sono letti e scritti con l'owner di modulo
			//CTBNamespace aModNs(CTBNamespace::MODULE, aNs.GetApplicationName() + CTBNamespace::GetSeparator() + aNs.GetModuleName());

			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrinter, m_strDefaultPrinter, sFilename);
			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnNoBitmap,	m_bDefaultPrnNoBitmap, sFilename);
			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnNoBorders, m_bDefaultPrnNoBorders, sFilename);
			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnShowLabels, m_bDefaultPrnShowLabels, sFilename);
			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnShowTitles, m_bDefaultPrnShowTitles, sFilename);
			//AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnUseDraftFont,	m_bDefaultPrnUseDraftFont, sFilename);

			//AfxSaveSettings(aModNs, sFilename, szDefaultPrinter);
		}
	}
}
