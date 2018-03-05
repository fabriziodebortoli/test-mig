using System;
using System.Collections;
using System.Collections.Generic;

using System.Drawing;

using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.NameSolver;

using TaskBuilderNetCore.Interfaces;
using Microarea.Common.Temp;


namespace Microarea.Common.Applications
{
    /// <summary>
    /// Summary description for FontStyles.
    /// </summary>
    /// ================================================================================
    public class FontElement
	{
		public enum FontSource { STANDARD, CUSTOM, WOORM, WOORM_TEMPLATE}

        private const string fontDefault = "<Default>"; //ghost font style used by report template
		private char[] areaSeparators = new char[] { ',' };

		private string		styleName;
		private string		faceName;
		private int			size;
		private FontStyle	fontStyle;     
		private Color		color = Color.FromArgb(255,0,0,0);
		private INameSpace	owner;
		private FontSource	source;
		private List<string> limitedContextArea = new List<string>();
		private ExternalAPI.LOGFONT logFont;

		// properties
		public string		StyleName			{ get { return styleName;}}
		public string		FaceName			{ get { return faceName;}}
		public int			Size				{ get { return size;}}
		public FontStyle	FontStyle			{ get { return fontStyle;}}     
		public Color		Color				{ get { return color;}}
		public INameSpace	Owner				{ get { return owner;}}
		public FontSource	Source				{ get { return source;}}
		public List<string>	LimitedContextArea	{ get { return limitedContextArea;} set { limitedContextArea = value; } }

		//------------------------------------------------------------------------------
		public FontElement(string styleName, string faceName, int size, FontStyle fontStyle, Color color, INameSpace owner, FontSource source, ExternalAPI.LOGFONT logFont)
		{																								  
			this.styleName = styleName;
			this.faceName = faceName;
            this.fontStyle = fontStyle;      
            this.size = size;
			this.color = color;
			this.owner = owner;
			this.source = source;
			this.logFont = logFont;
		}

		//------------------------------------------------------------------------------
		public FontElement(string styleName, string faceName, int size, FontStyle fontStyle, NameSpace owner, FontSource source)
		{
			this.styleName = styleName;
			this.faceName = faceName;
            this.fontStyle = fontStyle; 
            this.size = size;
			this.owner = owner;
			this.source = source;
		}

		//------------------------------------------------------------------------------
		public ExternalAPI.LOGFONT LogFont { get { return logFont; } }

		//------------------------------------------------------------------------------
		public bool IsNoneFont () 
		{
			return string.Compare(styleName, fontDefault) == 0;
		}
		
		//------------------------------------------------------------------------------
		public string GetLimitedArea()
		{
			string s = string.Empty;
			foreach (var item in LimitedContextArea)
			{
				s = s + item;
				if (item != LimitedContextArea[LimitedContextArea.Count - 1])
					s += areaSeparators;
			}
			return s;
		}
	}
	
	/// <summary>
	/// Gruppo di fontStyles che hanno lo stesso nome, ma provenienza differente
	/// </summary>
	//=============================================================================
	public class FontStylesGroup
	{
		private string  styleName;
        //private string  appName;
        //private string  moduleName;

        private List<FontElement> fontStyles = new List<FontElement>();

		// properties
		public string		StyleName	{ get { return styleName; } }

        public List<FontElement> FontStyles { get { return fontStyles; } }

		//------------------------------------------------------------------------------
		public FontStylesGroup (string name)
		{
			styleName = name;

        }

		/// <summary>
		/// Sceglie il font più opportuno per il context
		/// </summary>
		//------------------------------------------------------------------------------
		public FontElement GetFontStyle (INameSpace context)
		{
			if (fontStyles.Count == 0)
				return null;

			return BestFontForContext(context);
		}

		/// <summary>
		/// Aggiunge un font controllando prima se esiste già
		/// </summary>
		//------------------------------------------------------------------------------
		public int AddFont (FontElement font)
		{
			if (font == null)
				return -1;
			
			FontElement el;
			for (int i=0; i < fontStyles.Count; i++)
			{
				el = (FontElement) fontStyles[i];
				if (el.Source == font.Source && el.Owner == font.Owner)
				{
					el = font;
					return i;
				}
			}
			
			fontStyles.Add(font);
            return fontStyles.Count - 1;

        }		

		/// <summary>
		// Si occupa di scegliere il font migliore da applicare secondo contesto. La
		// scaletta delle priorità è la seguente:
		//	1) il font corrispondente ad uno specifico namespace 
		//	2) il font corrispondente alla stessa applicazione e modulo
		//	3) il font corrispondente alla stessa applicazione	(il primo trovato)
		//	4) il font corrispondente di altre applicazioni		(il primo trovato)
		//	5) l'ultimo caricato
		//	- a parità di font, il font custom è più forte di quello standard
		//	- se ce n'è uno solo definito, viene ritornato l'unico
		/// </summary>
		//------------------------------------------------------------------------------
		private FontElement	BestFontForContext (INameSpace context)
		{
			// se non ho contesto prendo l'ultimo dichiarato
			if (context == null || !context.IsValid())
				return (FontElement) fontStyles[fontStyles.Count -1];

			NameSpace nsModule = new NameSpace 
				(
				context.Application + NameSpace.TokenSeparator + context.Module,
				NameSpaceObjectType.Module
				);

			// Cerco il mio corrispondente preciso, e mi predispongo già quello 
			// con lo stesso nome di applicazione e/o con lo stesso nome di modulo
			FontElement exactFont	= null;
			FontElement appFont		= null;
			FontElement modFont		= null;
			FontElement otherAppFont= null;

			foreach (FontElement font in fontStyles)
			{
				if (font == null)
					continue;

				// ho il corrispondente identico
				if (font.Owner == context && HasPriority(exactFont, font, context, nsModule))
					exactFont = font;

				// il primo trovato con la stessa applicazione
				if (
					string.Compare(font.Owner.Application, context.Application, StringComparison.OrdinalIgnoreCase) == 0 && 
					HasPriority(appFont, font, context, nsModule)
					)
					appFont = font;

				// il primo trovato con lo stesso modulo
				if (font.Owner == nsModule && HasPriority(modFont, font, context, nsModule))
					modFont = font;

				// il primo trovato di altre applicazioni
				if (
					string.Compare(font.Owner.Application, context.Application, StringComparison.OrdinalIgnoreCase) != 0 && 
					HasPriority(otherAppFont, font, context, nsModule)
					)
					otherAppFont = font;	
			}

			if (exactFont != null)		return exactFont;		// di stesso namespace (report di woorm)
			if (modFont != null)		return modFont;			// di modulo
			if (appFont != null)		return appFont;			// di applicazione
			if (otherAppFont != null)	return otherAppFont;	// di altre applicazioni

			// l'ultimo caricato
			return (FontElement) fontStyles[fontStyles.Count -1];
		}

		/// <summary>
		/// Definisce se il nuovo font ha priorità su quello già scelto
		/// </summary>
		//------------------------------------------------------------------------------
		private bool HasPriority (FontElement oldFont, FontElement newFont, INameSpace context, INameSpace moduleContext)
		{
			if (newFont == null)
				return false;

			bool forArea = newFont.LimitedContextArea.Count == 0;

			// area di applicazione del font
			if (!forArea)
				foreach (string area in newFont.LimitedContextArea)
				{	
					if	(
						string.Compare(context.FullNameSpace,		area, StringComparison.OrdinalIgnoreCase)	== 0 ||
						string.Compare(moduleContext.FullNameSpace,	area, StringComparison.OrdinalIgnoreCase)	== 0 ||
						string.Compare(context.Application,			area, StringComparison.OrdinalIgnoreCase)	== 0
						)
					{
						forArea = true;
						break;
					}
				}

			if (oldFont == null)
				return forArea;

			return	forArea &&
				string.Compare(oldFont.Owner.FullNameSpace, newFont.Owner.FullNameSpace, StringComparison.OrdinalIgnoreCase) == 0 &&
				oldFont.Source == FontElement.FontSource.STANDARD && 
				newFont.Source == FontElement.FontSource.CUSTOM;
		}
	
	//Metodo che dice se questo FontStylesGroup viene da un template
	//---------------------------------------------------------------------------------
	public bool IsFromTemplate()
	{
		foreach (FontElement fe in fontStyles)
		{
			if (fe.Source == FontElement.FontSource.WOORM_TEMPLATE)
				return true;
		}
		return false;
	}
}

	/// <summary>
	/// Summary description for FontStyles.
	/// </summary>
	/// ================================================================================
	public class ApplicationFontStyles
	{
		private FontStyles	fs;
		private bool		loaded = true;
        //	private TbSession		reportSession;
        private PathFinder pathFinder;
        //-----------------------------------------------------------------------------
        public bool		Loaded	{ get { return loaded; }}
		public PathFinder PathFinder { get { return pathFinder; } set { pathFinder = value; }}
        public FontStyles Fs { get { return fs; } }

        //------------------------------------------------------------------------------
        public ApplicationFontStyles(PathFinder aPathFinder)
		{
			// è necessario inizializzare prima una sessione di lavoro.
			this.pathFinder = aPathFinder;
			if (pathFinder == null)
				throw (new Exception(ApplicationsStrings.FontStyleSessionError));

			fs = new FontStyles();
		}

		//------------------------------------------------------------------------------
		public void Load()
		{
			if (pathFinder == null)
				return;
			// considera che tutto sia ok. Se anche solo uno dei file fonts non è caricato
			// o se la reportSession non è valorizzata allora considera il tutto non caricato.
			loaded = true;

			// carica tutti gli enums che fanno parte della applicazione (controllando che esista)
			foreach (ApplicationInfo ai in pathFinder.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
				{
					NameSpace nsOwner = new NameSpace(ai.Name + NameSpace.TokenSeparator + mi.Name, NameSpaceObjectType.Module);
					if (!fs.Load(mi.GetFontsFullFilename(), nsOwner, FontElement.FontSource.STANDARD, ai.Name, mi.Name)) 
						loaded = false;
					
                    //TODO LARA E BRUNA X EASYBUILDER
					//if (!fs.Load(mi.GetCustomFontsFullFilename(pathFinder.Company), nsOwner, FontElement.FontSource.CUSTOM, ai.Name, mi.Name)) 
					//	loaded = false;
				}
		}

		//------------------------------------------------------------------------------
		public FontElement GetFontElement(string name, INameSpace context)
		{
			return fs.GetFontElement(name, context);
		}
	}

    /// <summary>
    /// Summary description for FontStyles.
    /// </summary>
    /// ================================================================================
    public class FontStyles : Dictionary<string, FontStylesGroup>
    {
        private const int OLD_RELEASE = 2;
        private const int RELEASE = 3;
        private char[] areaSeparators = new char[] { ',' };

        private bool newStyle = true;
        private FontElement.FontSource source;
        private INameSpace owner;
        private int fontRelease = 0;

        public INameSpace OwnerNameSpace {get{  return owner;} }
		public  int FontRelease { get { return fontRelease; } }

        //------------------------------------------------------------------------------
        public FontStyles()
            :
            base(StringComparer.OrdinalIgnoreCase)
		{
		}

		//------------------------------------------------------------------------------
		public FontElement GetFontElement(string name, INameSpace context)
		{
			FontStylesGroup pGroup = null;
            if (!TryGetValue(name, out pGroup))
				return null;

			return pGroup.GetFontStyle(context);
		}

		//------------------------------------------------------------------------------
		private bool AddFont (FontElement font)
		{
			if (font == null)
				return false;

			// se non esiste lo creo nuovo
			FontStylesGroup group = null;
			if (!ContainsKey(font.StyleName)) 
			{
				group = new FontStylesGroup(font.StyleName);
				Add(font.StyleName, group);
			}

			group = (FontStylesGroup) this[font.StyleName]; 
			
			if (group == null)
				return false;

			return group.AddFont(font) >= 0;
		}

		//------------------------------------------------------------------------------
		private bool ParseNewStyle (Parser lex)
		{
			string styleName = "";
			string faceName = "";
			Color color = Defaults.DefaultTextColor;
			int	size = 0;

			if (lex.LookAhead(Token.ID))
			{
				string id;
				lex.ParseID(out id);
				if (string.Compare(id, "name", StringComparison.OrdinalIgnoreCase) !=0 )
					return false;
			}
			if (!(
				lex.ParseString (out styleName)	&&
				lex.ParseTag (Token.FACENAME)	&&	lex.ParseString (out faceName)	&&
				lex.ParseTag (Token.SIZE)		&&	lex.ParseInt	(out size)
				))
				return false;

			// Make conversion from Tipographic point to logical inch point (sett pag 664 of Petzold book)
			size = ((size * 100) / 72);

            FontStyle fontStyle = FontStyle.Regular;         

            ExternalAPI.LOGFONT logFont = new ExternalAPI.LOGFONT();
			if (lex.LookAhead(Token.STYLE)) 
			{
				if (!(
					lex.ParseTag(Token.STYLE) &&
					lex.ParseOpen() &&
					lex.ParseLong(out logFont.lfHeight)			&& lex.ParseComma() &&
					lex.ParseLong(out logFont.lfWidth)			&& lex.ParseComma() &&
					lex.ParseLong(out logFont.lfEscapement)		&& lex.ParseComma() &&
					lex.ParseLong(out logFont.lfWeight)			&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfItalic)			&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfUnderline)		&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfStrikeOut)		&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfCharSet)		&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfOutPrecision)	&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfClipPrecision)	&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfQuality)		&& lex.ParseComma() &&
					lex.ParseByte(out logFont.lfPitchAndFamily)	&& lex.ParseComma() &&
					lex.ParseColor	(Token.TEXTCOLOR, out color)&&
					lex.ParseClose()
					))
					return false;

                // FW_NORMAL è uguale a 400 come definito in WINGDI.H
                //nel parsing dei file .wrmt il default della lfWeight che arriva è 0. Il bold va applicato solo quando la lfWeight > 400
                if (logFont.lfWeight > 400) fontStyle |= FontStyle.Bold;   
                if (logFont.lfItalic != 0) fontStyle |= FontStyle.Italic; 
                if (logFont.lfUnderline != 0) fontStyle |= FontStyle.Underline;
                if (logFont.lfStrikeOut != 0) fontStyle |= FontStyle.Strikeout;
            }

            // parso le aree di applicazione se esistono
            List<string> limitedArea = new List<string>();
			if (lex.LookAhead(Token.TEXTSTRING) && !ParseLimitedArea(lex, limitedArea))
				return false;

			if (!lex.ParseSep())
				return false;

			FontElement font = new FontElement(styleName, faceName, size, fontStyle, color, owner, source, logFont);
			font.LimitedContextArea = limitedArea;
			AddFont (font);
			return true;
		}

		//------------------------------------------------------------------------------
		private bool ParseLimitedArea(Parser lex, List<string> limitedArea)
		{
			string aree;

			if (!lex.ParseString(out aree))
				return false;
			
			// se è vuota non mi arrabbio
			if (aree == null || aree.Length == 0)
				return true;
	
			string[] areas = aree.Split(areaSeparators, 100);
			foreach (string area in areas) 
				limitedArea.Add(area);

			return true;
		}

		//------------------------------------------------------------------------------
		private bool ParseOldStyle (Parser lex)
		{
			string styleName = "";
			string faceName = "";
			Color color = Defaults.DefaultTextColor;
			int	size = 0;

			ExternalAPI.LOGFONT logFont = new ExternalAPI.LOGFONT();
			logFont.lfWeight = 400;

			if (lex.LookAhead(Token.ID))
			{
				string id;
				lex.ParseID(out id);
				if (string.Compare(id, "name", StringComparison.OrdinalIgnoreCase) != 0)
					return false;
			}

			if (!( 
				lex.ParseString (out styleName)	&&
				lex.ParseTag (Token.FACENAME)	&&	lex.ParseString (out faceName)	&&
				lex.ParseTag (Token.SIZE)		&&	lex.ParseInt	(out size)
				)) 
				return false;

			// Make conversion from Tipographic point to logical inch point (sett pag 664 of Petzold book)
			size = ((size * 100) / 72);

            FontStyle fontStyle = FontStyle.Regular;         
            if (lex.LookAhead(Token.STYLE)) 
			{
				lex.SkipToken();
				bool loop = true;
				while (loop)
				{
					switch (lex.LookAhead())
					{
						case Token.ITALIC: lex.SkipToken(); logFont.lfItalic = 1; fontStyle |= FontStyle.Italic;  break;
						case Token.BOLD: lex.SkipToken(); logFont.lfWeight = 700; fontStyle |= FontStyle.Bold;break;
						case Token.UNDERLINE: lex.SkipToken(); logFont.lfUnderline = 1; fontStyle |= FontStyle.Underline;  break;
						case Token.STRIKEOUT: lex.SkipToken(); logFont.lfStrikeOut = 1; fontStyle |= FontStyle.Strikeout; break;
						case Token.TEXTCOLOR:	lex.ParseColor(Token.TEXTCOLOR, out color);break;

						case Token.EOF:			lex.SetError(ApplicationsStrings.FontStyleUnexpectedEof); return false;
						case Token.END:			lex.SetError(ApplicationsStrings.FontStylesUnexpectedEnd); return false;
					
						default	: loop = false; break;
					}
				}
			}

			if (!lex.ParseSep())
				return false;

			FontElement font = new FontElement(styleName, faceName, size, fontStyle, color, owner, source, logFont);
			AddFont(font);

			return true;
		}

		//------------------------------------------------------------------------------
		private bool ParseStyle(Parser lex)
		{
			return 
				newStyle 
				? ParseNewStyle(lex)
				: ParseOldStyle(lex);
		}

		//------------------------------------------------------------------------------
		private bool ParseBlock(Parser lex)
		{
			if (lex.LookAhead(Token.BEGIN))
				return
					lex.ParseBegin	()		&&
					ParseStyles		(lex)	&&
					lex.ParseEnd	();

			return ParseStyle(lex);
		}

		//------------------------------------------------------------------------------
		private bool ParseStyles(Parser lex)
		{
			bool ok = true;

			do {ok = ParseStyle(lex) && !lex.Error && !lex.Eof;}
			while (ok && !lex.LookAhead(Token.END));

			return ok;
		}

		//------------------------------------------------------------------------------
		private bool ParseHeader(Parser lex)
		{
			if (lex.LookAhead(Token.FONTSTYLES))
			{
				lex.SkipToken();
				return ParseBlock(lex);
			}
			
			return true;
		}

		// I fonts possono essere caricati anche da soli e non solo nel contesto di woorm.
		// Se parsa i font di applicazione richiede la release altrimenti se sono all'interno 
		// di un report (woorm) non bisogna parsare la release ma solo la tabella di stili

		// Parse autonomo da file contenente una tabella di font (fonts.ini)
		//------------------------------------------------------------------------------
		private bool Parse(Parser lex)
		{
			if (!lex.ParseTag (Token.RELEASE) || !lex.ParseInt(out fontRelease))
				return false;
					
			if (fontRelease > RELEASE)
			{
				lex.SetError(ApplicationsStrings.BadFontStyleRelease);
				return false;
			}

			// abilita la nuova sintassi
			newStyle = fontRelease > OLD_RELEASE;

			return ParseHeader(lex);
		}

		// siamo all'interno di un report e quindi la release è un'altra
		//------------------------------------------------------------------------------
		public bool Parse(Parser lex, int woormRelease, INameSpace owner, bool fromTemplate)
		{
			newStyle = woormRelease > OLD_RELEASE;
			//imposto se il font e' caricato dal template o e' del report stesso
			source = fromTemplate ? FontElement.FontSource.WOORM_TEMPLATE : FontElement.FontSource.WOORM;
			this.owner = owner;

			return ParseHeader(lex);
		}

		//-----------------------------------------------------------------------------
		public bool Load(string filename, NameSpace owner, FontElement.FontSource source, string appName, String moduleName)
		{

            if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filename))
                return true;
		
			// mi evito di passarli di metodo in metodo
			this.source = source;
			this.owner = owner;

			Parser lex = new Parser(Parser.SourceType.FromFile);
			if (lex.Open(filename))
			{
				bool ok = Parse(lex);
				lex.Close();
				return ok;
			}

			return false;
		}
        //-----------------------------------------------------------------------------
        public bool UnparseAll(Unparser unparser)
        {
            unparser.WriteTag(Token.RELEASE, false);
            unparser.Write(fontRelease);
            unparser.WriteTag(Token.FONTSTYLES, false);
			unparser.WriteBlank();

			unparser.WriteBegin();
            foreach (FontStylesGroup group in this.Values)
            {
                foreach (FontElement style in group.FontStyles)
                    UnparseStyle(unparser, style);
            }
            unparser.WriteEnd();
			unparser.WriteLine();

            return true;
        }

        //-----------------------------------------------------------------------------
        public bool Unparse(Unparser unparser, INameSpace ns, FontElement.FontSource source)
		{
			// se chiamata da wrmeng deve sempre salvare
			//if (!pFontTable->IsModified())
			//    return false;

			this.source = source;

			bool notEmpty = IsUnparsingNeeded(ns);

			//se non c'è niente da unparsare, esco (occhio, in questo modo non passo neanche dal codice che
			//adesso è commentato
			if (!notEmpty)
				return true;

			// scrive  la release supportata
			if (source != FontElement.FontSource.WOORM && source != FontElement.FontSource.WOORM_TEMPLATE)
			{
			    unparser.WriteTag(Token.RELEASE, false);
			    unparser.Write(fontRelease);
			}

			unparser.WriteTag(Token.FONTSTYLES, false);
			unparser.WriteBlank();

			unparser.WriteBegin();
			UnparseStyles(ns, unparser);
			unparser.WriteEnd();
			unparser.WriteLine();

			//TODOLUCA
			// avviso che è stata cambiata la data del file
			//if (source != FontElement.FontSource.WOORM && source != FontElement.FontSource.WOORM_TEMPLATE)
			//    pFontTable->AddFileLoaded(aModule, m_Source, ::GetFileDate(unparser.GetFilePath()));

			//if (notEmpty)
			//    return true;

			//// se è vuoto viene eliminato
			//if (source != FontElement.FontSource.WOORM && source != FontElement.FontSource.WOORM_TEMPLATE)
			//{
			//    CString strPath = unparser.GetFilePath();
			//    unparser.Close();
			//    DeleteFile((LPCTSTR) strPath);
			//    pFontTable->RemoveFileLoaded(aModule, m_Source);
			//}

			return true;
		}

		//-----------------------------------------------------------------------------
		private bool IsUnparsingNeeded(INameSpace ns)
		{
			foreach (FontStylesGroup group in this.Values)
			{
				foreach (FontElement style in group.FontStyles)
				{
					if (style.Source == source && style.Owner == ns /*&&  pStyle->IsChanged()*/)
						return true;
				}
			}

			return false;
		}

		//-----------------------------------------------------------------------------
		private void UnparseStyles(INameSpace ns, Unparser unparser)
		{
			foreach (KeyValuePair<string,FontStylesGroup> group in this)
			{
				foreach (FontElement style in group.Value.FontStyles)
				{
					if (style.Source == source && style.Owner == ns /*&&  pStyle->IsChanged()*/)
						UnparseStyle(unparser, style);
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void UnparseStyle(Unparser unparser, FontElement style)
		{
			// Make conversion from Tipographic point to logical inch point 
			// (see pag 664 of Petzold book). Invert sign for not include External Leading
			long nFontHeight = style.LogFont.lfHeight;
			if (nFontHeight < 0)
				nFontHeight = nFontHeight * (-1);
			long nFontPitch = ((nFontHeight * 72) / 100) + 1;

			unparser.WriteString(style.StyleName, false);
			unparser.WriteBlank();
			unparser.WriteTag(Token.FACENAME, false);
			unparser.WriteString(style.FaceName, false);
			unparser.WriteBlank();
			unparser.WriteTag(Token.SIZE, false);
			unparser.Write(nFontPitch, false);
			unparser.WriteBlank();

			UnparseStyleOption(unparser, style);

			// area di applicazione
			if (!style.GetLimitedArea().IsNullOrEmpty() && source != FontElement.FontSource.WOORM && source != FontElement.FontSource.WOORM_TEMPLATE)
				unparser.WriteString(style.GetLimitedArea());

			unparser.WriteSep(true);
		}

		//------------------------------------------------------------------------------
		private void UnparseStyleOption(Unparser unparser, FontElement style)
		{
			if (style.LogFont == null)
				return;

			if (
				style.LogFont.lfWidth != 0 ||
				style.LogFont.lfEscapement != 0 ||
				style.LogFont.lfOrientation != 0 ||
				style.LogFont.lfWeight != 400  || /*(long)FontWeights.FW_NORMAL*/
				style.LogFont.lfItalic != 0 ||
				style.LogFont.lfUnderline != 0 ||
				style.LogFont.lfStrikeOut != 0 ||
				style.LogFont.lfCharSet != 0 ||
				style.LogFont.lfOutPrecision != 0 ||
				style.LogFont.lfClipPrecision != 0 ||
				style.LogFont.lfQuality != 0 ||
				style.LogFont.lfPitchAndFamily != (0<<4) ||
				style.Color != Defaults.DefaultTextColor
		)
			{
				unparser.WriteTag(Token.STYLE, false);
				unparser.WriteOpen(false);
				unparser.Write(style.LogFont.lfWidth, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfEscapement, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfOrientation, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfWeight, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfItalic, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfUnderline, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfStrikeOut, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfCharSet, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfOutPrecision, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfClipPrecision, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfQuality, false); unparser.WriteComma(false);
				unparser.Write(style.LogFont.lfPitchAndFamily, false); unparser.WriteComma(false);
				unparser.WriteColor(Token.TEXTCOLOR, style.Color, false);
				unparser.WriteClose(false);
			}
		}
	}
}
