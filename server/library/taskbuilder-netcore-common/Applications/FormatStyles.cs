using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Microarea.Common.NameSolver;


using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using TaskBuilderNetCore.Interfaces;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Microarea.Common.Applications
{
    //=========================================================================
    public class CurrentCulture
    {
        private string serverConnectionPreferredLanguage = string.Empty;
        private string currentUserPreferredLanguage = string.Empty;

        //---------------------------------------------------------------------
        //Proprietà pubbliche
        public string ServerConnectionPreferredLanguage { get { return serverConnectionPreferredLanguage; } set { serverConnectionPreferredLanguage = value; } }
        public string CurrentUserPreferredLanguage { get { return currentUserPreferredLanguage; } set { currentUserPreferredLanguage = value; } }

        //---------------------------------------------------------------------
        public CurrentCulture(UserInfo userInfo)
        {
            if (userInfo == null)
                return;

            serverConnectionPreferredLanguage = InstallationData.ServerConnectionInfo.PreferredLanguage;

            currentUserPreferredLanguage = userInfo.UserUICulture.ToString();
        }

        //---------------------------------------------------------------------
        public string GetCulture()
        {
            if (currentUserPreferredLanguage != null && currentUserPreferredLanguage != string.Empty)
                return currentUserPreferredLanguage;

            if (serverConnectionPreferredLanguage != null && serverConnectionPreferredLanguage != string.Empty)
                return serverConnectionPreferredLanguage;

            return null;
        }
    }

    //=========================================================================
    public class CNumberToLiteralManager
    {
        public static Dictionary<string, CNumberToLiteralLookUpTableManager> numberToLiteral = new Dictionary<string, CNumberToLiteralLookUpTableManager>();
        private static string culture = string.Empty;

        //---------------------------------------------------------------------
        //Proprietà
        public static string Culture
        {
            get { return culture; }
            set { culture = value; }
        }

        /// <summary>
        /// Sceglie il convertitore di numeri in lettere relativo alla lingua desiderata
        /// </summary>
        //------------------------------------------------------------------------------
        public static CNumberToLiteralLookUpTableManager GetNumberToLiteralManager(string language)
        {
            CNumberToLiteralLookUpTableManager ntlLookupTableManager = null;
            if (!numberToLiteral.TryGetValue(language, out ntlLookupTableManager))
            {
                // Se non esiste lo creo e lo aggiungo all'elenco
                return AddNumberToLiteralManager(language);
            }
            else
                return ntlLookupTableManager;
        }

        /// <summary>
        /// Sceglie il convertitore di numeri in lettere relativo alla lingua dell'applicazione
        /// </summary>
        //------------------------------------------------------------------------------
        public static CNumberToLiteralLookUpTableManager GetNumberToLiteralManager()
        {
            CNumberToLiteralLookUpTableManager ntlLookupTableManager = null;
            if (!numberToLiteral.TryGetValue(culture, out ntlLookupTableManager))
            {
                // Se non esiste lo creo e lo aggiungo all'elenco
                return AddNumberToLiteralManager(culture);
            }
            else
                return ntlLookupTableManager;
        }

        /// <summary>
        /// Agglunge alla lista il convertitore di numeri in lettere relativo alla lingua desiderata
        /// </summary>
        //------------------------------------------------------------------------------
        public static CNumberToLiteralLookUpTableManager AddNumberToLiteralManager(string language)
        {
            CNumberToLiteralLookUpTableManager NTLManager = null;
            // Controllo se esiste già
            if (!numberToLiteral.TryGetValue(language, out NTLManager))
            {
                //Cerco di parsare il file contenente le regole di conversione
                NTLManager = ParseNumberToLiteralLookUpFile(language);
                if (NTLManager != null)
                {
                    // Lo aggiungo alla lista
                    numberToLiteral.Add(language, NTLManager);
                    return NTLManager;
                }
                else
                    return null;
            }
            else
                return NTLManager;
        }

        /// <summary>
        /// Parsa il file contenente le regole di conversione di numeri in lettere
        /// relativo alla lingua desiderata
        /// </summary>
        //------------------------------------------------------------------------------
        public static CNumberToLiteralLookUpTableManager ParseNumberToLiteralLookUpFile(string language)
        {
            XmlDocument initXmlFile = new XmlDocument();
            string nomeFile = string.Empty;

			try
			{
				NameSpace ns = new NameSpace("Module.Framework.TbGenlib");
				nomeFile = PathFinder.PathFinderInstance.GetNumberToLiteralXmlFullName(ns, language);
				
				if (nomeFile == null || nomeFile == string.Empty)
					return null;

                CNumberToLiteralLookUpTableManager NTLManager = new CNumberToLiteralLookUpTableManager();

                initXmlFile = PathFinder.PathFinderInstance.LoadXmlDocument(initXmlFile, nomeFile);
                foreach (XmlNode n in initXmlFile.SelectSingleNode("LookUp/NameEntryes"))
                {
                    long v = 0;
                    string d = string.Empty;
                    v = long.Parse(n.Attributes["ValInt"].Value);
                    d = n.Attributes["ValStr"].Value;
                    //LuL.Add(v, d);
                    NTLManager.Add(v, d);
                }
                CNumberToLiteralLookUpTableManager.DeclinationType decType;

                decType = CNumberToLiteralLookUpTableManager.DeclinationType.Hundreds;
                XmlNode nGroup = initXmlFile.SelectSingleNode("LookUp/Groups/sHundreds");
                NTLManager.AddNumberGroup(decType, nGroup.Attributes["value"].Value);
                foreach (XmlNode nDeclination in nGroup.SelectNodes("Declination"))
                {
                    int digit = int.Parse(nDeclination.Attributes["digit"].Value);
                    string descri = nDeclination.Attributes["value"].Value;
                    NTLManager.AddDeclination(decType, digit, descri);
                    foreach (XmlNode nException in nDeclination.SelectNodes("Exception"))
                    {
                        int val = int.Parse(nException.Attributes["digit"].Value);
                        string kind = nException.Attributes["kind"].Value;
                        NTLManager.AddDeclinationException(decType, digit, kind, val);
                    }
                }

                decType = CNumberToLiteralLookUpTableManager.DeclinationType.Thousands;
                nGroup = initXmlFile.SelectSingleNode("LookUp/Groups/sThousands");
                NTLManager.AddNumberGroup(decType, nGroup.Attributes["value"].Value);
                foreach (XmlNode nDeclination in nGroup.SelectNodes("Declination"))
                {
                    int digit = int.Parse(nDeclination.Attributes["digit"].Value);
                    string descri = nDeclination.Attributes["value"].Value;
                    NTLManager.AddDeclination(decType, digit, descri);
                    foreach (XmlNode nException in nDeclination.SelectNodes("Exception"))
                    {
                        int val = int.Parse(nException.Attributes["value"].Value);
                        string kind = nException.Attributes["kind"].Value;
                        NTLManager.AddDeclinationException(decType, digit, kind, val);
                    }
                }

                decType = CNumberToLiteralLookUpTableManager.DeclinationType.Millions;
                nGroup = initXmlFile.SelectSingleNode("LookUp/Groups/sMillions");
                NTLManager.AddNumberGroup(decType, nGroup.Attributes["value"].Value);
                foreach (XmlNode nDeclination in nGroup.SelectNodes("Declination"))
                {
                    int digit = int.Parse(nDeclination.Attributes["digit"].Value);
                    string descri = nDeclination.Attributes["value"].Value;
                    NTLManager.AddDeclination(decType, digit, descri);
                    foreach (XmlNode nException in nDeclination.SelectNodes("Exception"))
                    {
                        int val = int.Parse(nException.Attributes["digit"].Value);
                        string kind = nException.Attributes["kind"].Value;
                        NTLManager.AddDeclinationException(decType, digit, kind, val);
                    }
                }

                decType = CNumberToLiteralLookUpTableManager.DeclinationType.Milliards;
                nGroup = initXmlFile.SelectSingleNode("LookUp/Groups/sMilliards");
                NTLManager.AddNumberGroup(decType, nGroup.Attributes["value"].Value);
                foreach (XmlNode nDeclination in nGroup.SelectNodes("Declination"))
                {
                    int digit = int.Parse(nDeclination.Attributes["digit"].Value);
                    string descri = nDeclination.Attributes["value"].Value;
                    NTLManager.AddDeclination(decType, digit, descri);
                    foreach (XmlNode nException in nDeclination.SelectNodes("Exception"))
                    {
                        int val = int.Parse(nException.Attributes["value"].Value);
                        string kind = nException.Attributes["kind"].Value;
                        NTLManager.AddDeclinationException(decType, digit, kind, val);
                    }
                }

                if (initXmlFile.SelectSingleNode("LookUp/Parameters") != null)
                {
                    if (initXmlFile.SelectSingleNode("LookUp/Parameters/UnitInvertion") != null)
                        NTLManager.bUnitInversion = (initXmlFile.SelectSingleNode("LookUp/Parameters/UnitInvertion").Attributes["value"].Value == "true");

                    if (initXmlFile.SelectSingleNode("LookUp/Parameters/Junction") != null)
                        NTLManager.m_Junction = initXmlFile.SelectSingleNode("LookUp/Parameters/Junction").Attributes["value"].Value;

                    if (initXmlFile.SelectSingleNode("LookUp/Parameters/Separator") != null)
                    {
                        NTLManager.m_Separator = initXmlFile.SelectSingleNode("LookUp/Parameters/Separator").Attributes["value"].Value;
                        if (initXmlFile.SelectSingleNode("LookUp/Parameters/Separator/Exeptions") != null)
                        {
                            foreach (XmlNode n in initXmlFile.SelectSingleNode("LookUp/Parameters/Separator/Exeptions"))
                            {
                                NTLManager.m_Exceptions.Add(int.Parse(n.Attributes["value"].Value));
                            }
                        }
                    }
                }
                return NTLManager;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Summary description for BaseFormatter.
    /// </summary>
    /// ================================================================================
    public abstract class Formatter
    {
        public enum FormatSource { STANDARD, CUSTOM, WOORM }
        public enum AlignType { NONE, LEFT, RIGHT }

        // data members
        private char[] areaSeparators = new char[] { ',' };
        private string styleName = "";
        private Token dataType = 0;
        private AlignType align = AlignType.NONE;
        private string head = "";
        private string tail = "";
        private int paddedLen = 0;
        private int outputWidth = 0;
        private int outputCharLen = 15;
        private int inputWidth = 0;
        private int inputCharLen = 15;
        private INameSpace owner = null;
        private FormatSource source = FormatSource.STANDARD;
        private List<string> limitedContextArea = new List<string>();
        private CultureInfo collateCulture = CultureInfo.InvariantCulture;
        private bool zeroPadded = false;

        // properties
        public string StyleName { get { return styleName; } }
        public Token DataType { get { return dataType; } set { dataType = value; } }
        public AlignType Align { get { return align; } }
        public string Head { get { return head; } }
        public string Tail { get { return tail; } }
        public int PaddedLen { get { return paddedLen; } }
        public int OutputWidth { get { return outputWidth; } }
        public int OutputCharLen { get { return outputCharLen; } }
        public int InputWidth { get { return inputWidth; } }
        public int InputCharLen { get { return inputCharLen; } }
        public INameSpace Owner { get { return owner; } set { owner = value; } }
        public FormatSource Source { get { return source; } set { source = value; } }
        public List<string> LimitedContextArea { get { return limitedContextArea; } set { limitedContextArea = value; } }
        public CultureInfo CollateCulture { get { return collateCulture; } set { collateCulture = value; } }
        public bool ZeroPadded { get { return zeroPadded; } set { zeroPadded = value; } }

        public virtual Formatter.AlignType GetDefaultAlign() { return AlignType.RIGHT; }

        //------------------------------------------------------------------------------
        public abstract string Format(string data);
        //------------------------------------------------------------------------------
        public abstract string Format(object data);

        //------------------------------------------------------------------------------
        virtual public void SetToLocale()
        {
        }

        //------------------------------------------------------------------------------
        virtual public void RestoreFromLocale()
        {
        }

        //------------------------------------------------------------------------------
        public abstract string FormatFromSoapData(string data);

        //------------------------------------------------------------------------------
        protected abstract bool ParseFmtVariable(Parser lex);

        //------------------------------------------------------------------------------
        protected bool ParseFmtAlign(Parser lex)
        {
            lex.SkipToken();

            switch (lex.LookAhead())
            {
                case Token.LEFT: lex.SkipToken(); align = AlignType.LEFT; break;
                case Token.RIGHT: lex.SkipToken(); align = AlignType.RIGHT; break;
                case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                default: lex.SetError(ApplicationsStrings.BadAlign); return false;
            }

            return true;
        }

        //------------------------------------------------------------------------------
        protected bool ParseFmtCommon(Parser lex)
        {
            lex.SkipToken();
            bool ok = lex.ParseString(out styleName);
            do
            {
                switch (lex.LookAhead())
                {
                    case Token.PREFIX: lex.SkipToken(); ok = lex.ParseString(out head); break;
                    case Token.POSTFIX: lex.SkipToken(); ok = lex.ParseString(out tail); break;
                    case Token.LEN: lex.SkipToken(); ok = lex.ParseInt(out paddedLen); break;
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            }
            while (ok);

            return ok;
        }

        //----------------------------------------------------------------------------
        internal bool Parse(Parser lex)
        {
            bool bOk =
                ParseFmtCommon(lex) &&
                ParseFmtVariable(lex);

            // programmative formatters are at the end before application criteria
            if (bOk && lex.LookAhead(Token.FROM))
            {
                string s;
                lex.SkipToken();
                bOk = lex.ParseString(out s);
                NameSpace ns = new NameSpace(s);
                if (bOk && ns.IsValid() && ns.NameSpaceType.Type == NameSpaceObjectType.Library)
                    owner = ns;
                else
                    bOk = false;
            }

            // application criteria is at the end 
            if (bOk && lex.LookAhead(Token.TEXTSTRING) && !ParseLimitedArea(lex))
                return false;

            bOk = bOk && lex.ParseSep();

            return bOk;
        }

        //------------------------------------------------------------------------------
        private bool ParseLimitedArea(Parser lex)
        {
            string aree;

            if (!lex.ParseString(out aree))
                return false;

            // se è vuota non mi arrabbio
            if (aree == null || aree.Length == 0)
                return true;

            string[] areas = aree.Split(areaSeparators, 100);
            foreach (string area in areas)
                limitedContextArea.Add(area);

            return true;
        }

        //arriva fino a 9.999.999.999(MAXDIGIT = 10)
        //ITRI internazionalizzare la NumberToWord nei formattatori
        //----------------------------------------------------------------------------
        public string NumberToWord(long aValue)
        {
            CNumberToLiteralLookUpTableManager NTLManager = CNumberToLiteralManager.GetNumberToLiteralManager();

            if (NTLManager == null)
                return string.Empty;
            else
                return NTLManager.Convert(aValue);
        }

        //------------------------------------------------------------------------------
        public string Padder(string padded, bool padRight)
        {
            if (PaddedLen <= 0)
                return padded;

            //pad Right o Left, se specificata la paddedlen
            int posTo = PaddedLen - padded.Length;

            if (posTo < 0) return TextOverflow(PaddedLen); //se piu lungo di paddedlen mostro ****
            if (posTo == 0) return padded;

            return padRight ? padded.PadRight(PaddedLen) : padded.PadLeft(PaddedLen);
        }

        //scrive tanti * quanto e' PaddedLen
        //------------------------------------------------------------------------------
        public static string TextOverflow(int textLen)
        {
            StringBuilder textToAppend = new StringBuilder();
            return textToAppend.Append('*', textLen).ToString();
        }

        //------------------------------------------------------------------------------
        internal string GetLimitedArea()
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
    }//-------- fine Formatter

    /// <summary>
    /// Gruppo di Formatter che hanno lo stesso nome, ma provenienza differente
    /// </summary>
    //=============================================================================
    public class FormatStylesGroup
    {
        private string styleName;
        private List<Formatter> formatStyles = new List<Formatter>();

        // properties
        public string StyleName { get { return styleName; } }
        public List<Formatter> FormatStyles { get { return formatStyles; } }

        //------------------------------------------------------------------------------
        public FormatStylesGroup(string name)
        {
            styleName = name;
        }

        /// <summary>
        /// Sceglie il formattatore più opportuno per il context
        /// </summary>
        //------------------------------------------------------------------------------
        public Formatter GetFormatter(INameSpace context)
        {
            if (formatStyles.Count == 0)
                return null;

            return BestFormatterForContext(context);
        }

        /// <summary>
        /// Aggiunge un formattatore controllando prima se esiste già
        /// </summary>
        //------------------------------------------------------------------------------
        public int AddFormatter(Formatter formatter)
        {
            if (formatter == null)
                return -1;

            // qualora ne esisistesse uno con le stesse caratteristiche
            // lo elimino perchè viene sostituito dal nuovo. 
            Formatter el;
            for (int i = formatStyles.Count - 1; i >= 0; i--)
            {
                el = (Formatter)formatStyles[i];
                if (el.Source == formatter.Source && el.Owner == formatter.Owner)
                    formatStyles.RemoveAt(i);
            }

            formatStyles.Add(formatter);
            return formatStyles.Count - 1;
        }

        /// <summary>
        // Si occupa di scegliere il formatter migliore da applicare secondo contesto. La
        // scaletta delle priorità è la seguente:
        //	1) il formatter corrispondente ad uno specifico namespace 
        //	2) il formatter corrispondente alla stessa applicazione e modulo
        //	3) il formatter corrispondente alla stessa applicazione	(il primo trovato)
        //	4) il formatter corrispondente di altre applicazioni		(il primo trovato)
        //	5) l'ultimo caricato
        //	- a parità di formatter, il formatter custom è più forte di quello standard
        //	- se ce n'è uno solo definito, viene ritornato l'unico
        /// </summary>
        //------------------------------------------------------------------------------
        private Formatter BestFormatterForContext(INameSpace context)
        {
            // se non ho contesto prendo l'ultimo dichiarato
            if (context == null || !context.IsValid())
                return (Formatter)formatStyles[formatStyles.Count - 1];

            NameSpace nsModule = new NameSpace
                (
                    context.Application + NameSpace.TokenSeparator + context.Module,
                    NameSpaceObjectType.Module
                );

            // Cerco il mio corrispondente preciso, e mi predispongo già quello 
            // con lo stesso nome di applicazione e/o con lo stesso nome di modulo
            Formatter exactFmt = null;
            Formatter appFmt = null;
            Formatter modFmt = null;
            Formatter otherAppFmt = null;

            foreach (Formatter formatter in formatStyles)
            {
                if (formatter == null)
                    continue;

                // ho il corrispondente identico
                if (formatter.Owner == context && HasPriority(exactFmt, formatter, context, nsModule))
                    exactFmt = formatter;

                // il primo trovato con la stessa applicazione
                if (
                        string.Compare(formatter.Owner.Application, context.Application, StringComparison.OrdinalIgnoreCase) == 0 &&
                        HasPriority(appFmt, formatter, context, nsModule)
                    )
                    appFmt = formatter;

                // il primo trovato con lo stesso modulo
                if (formatter.Owner == nsModule && HasPriority(modFmt, formatter, context, nsModule))
                    modFmt = formatter;

                // il primo trovato di altre applicazioni
                if (
                        string.Compare(formatter.Owner.Application, context.Application, StringComparison.OrdinalIgnoreCase) != 0 &&
                        HasPriority(otherAppFmt, formatter, context, nsModule)
                    )
                    otherAppFmt = formatter;
            }

            if (exactFmt != null) return exactFmt;      // di stesso namespace (report di woorm)
            if (modFmt != null) return modFmt;          // di modulo
            if (appFmt != null) return appFmt;          // di applicazione
            if (otherAppFmt != null) return otherAppFmt;    // di altre applicazioni

            // l'ultimo caricato
            return (Formatter)formatStyles[formatStyles.Count - 1];
        }

        /// <summary>
        /// Definisce se il nuovo formattatore ha priorità su quello già scelto
        /// </summary>
        //------------------------------------------------------------------------------
        private bool HasPriority(Formatter oldFmt, Formatter newFmt, INameSpace context, INameSpace moduleContext)
        {
            if (newFmt == null)
                return false;

            bool forArea = newFmt.LimitedContextArea.Count == 0;

            // area di applicazione del font
            if (!forArea)
                foreach (string area in newFmt.LimitedContextArea)
                {
                    if (
                            string.Compare(context.FullNameSpace, area, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(moduleContext.FullNameSpace, area, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(context.Application, area, StringComparison.OrdinalIgnoreCase) == 0
                        )
                    {
                        forArea = true;
                        break;
                    }
                }

            if (oldFmt == null)
                return forArea;

            return forArea &&
                    string.Compare(oldFmt.Owner.FullNameSpace, newFmt.Owner.FullNameSpace, StringComparison.OrdinalIgnoreCase) == 0 &&
                    oldFmt.Source == Formatter.FormatSource.STANDARD &&
                    newFmt.Source == Formatter.FormatSource.CUSTOM;
        }
    }

    /// <summary>
    /// Summary description for LongFormatter.
    /// </summary>
    /// ================================================================================
    public class LongFormatter : Formatter
    {
        //----------------------------------------------------------------------------
        private bool isThousSeparatorDefault = true;

        [Flags]
        public enum SignTag
        {
            ABSOLUTEVAL = 0x0000,
            MINUSPREFIX = 0x0001,
            MINUSPOSTFIX = 0x0002,
            ROUNDS = 0x0003,
            SIGNPREFIX = 0x0004,
            SIGNPOSTFIX = 0x0005
        }

        public enum FormatTag { NUMERIC, LETTER, ENCODED, ZERO_AS_DASH = 0x0099 }

        public SignTag Sign { get; set; } = SignTag.MINUSPREFIX;
        public string XTable = "ZAEGHMPSTK";
        public string ThousSeparator = "";

        public string refThousSeparator { get { return ThousSeparator; } }

        public string AsZeroValue = "--";
        
        public FormatTag FormatType { get; set; } = FormatTag.NUMERIC;

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            if (data is long)
                return Format((long)data);
            if (data is int)
                return Format((int)data);

            return data.ToString();
        }

        //----------------------------------------------------------------------------
        public string Format(long aValue) { return Format(aValue, true); }
        public string Format(long aValue, bool padEnabled)
        {
            StringBuilder result = new StringBuilder();
            bool sign = (aValue >= 0);                  //se positivo :true
            bool asZero = (FormatType == FormatTag.ZERO_AS_DASH && aValue == 0);//se sostitiusco null:true
            aValue = (Math.Abs(aValue));                //tolgo il segno,lo poi metto a seconda delle richiesta

            switch (Sign)                                   //sign prefix
            {
                case SignTag.MINUSPREFIX:
                case SignTag.SIGNPREFIX:
                    if (!sign) result.Append("-");
                    else if (!asZero && Sign == SignTag.SIGNPREFIX) result.Append("+");
                    break;
                case SignTag.ROUNDS:
                    if (!sign) result.Append("("); break;
            }
            switch (FormatType)
            {
                case FormatTag.NUMERIC:
                case FormatTag.ZERO_AS_DASH:
                    if (asZero) result.Append(AsZeroValue);
                    else
                    {
                        if (aValue == 0)
                            result.Append("0");
                        else
                        {
                            //uso la mia personalizzazione di formattazione
                            NumberFormatInfo customFormat = new NumberFormatInfo();
                            if (ThousSeparator != null && ThousSeparator != string.Empty)
                            {
                                customFormat.NumberGroupSeparator = ThousSeparator;
                                result.Append(aValue.ToString("#,#", customFormat));
                            }
                            else result.Append(aValue.ToString());
                        }
                    }
                    break;
                case FormatTag.LETTER:
                    result.Append(NumberToWord(aValue));
                    break;
                case FormatTag.ENCODED:
                    result.Append(aValue);
                    for (int i = 0; i < XTable.Length; i++)
                    {
                        result.Replace(i.ToString(), XTable[i].ToString());
                    }
                    break;
            }
            switch (Sign)                               //sign postfix
            {
                case SignTag.SIGNPOSTFIX:
                case SignTag.MINUSPOSTFIX:
                    if (!sign) result.Append("-");
                    else if (!asZero && Sign == SignTag.SIGNPOSTFIX) result.Append("+");
                    break;
                case SignTag.ROUNDS:
                    if (!sign) result.Append(")"); break;
            }

            if (!padEnabled)
                return result.ToString();

            string padded = result.ToString();
            if (!ZeroPadded || asZero)
                return Padder(Head + padded + Tail, Align != AlignType.RIGHT);

            // test di overflow
            if (padded.Length + Head.Length + Tail.Length > PaddedLen)
                return TextOverflow(PaddedLen);

            //paddo con zeri solo se asZero = false
            if (!asZero)
            {
                int padLen = PaddedLen - Head.Length - Tail.Length;
                if (padLen > 0) padded = padded.PadLeft(padLen, '0');
            }
            return Head + padded + Tail;
        }

        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                long converted = long.Parse(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                long converted = SoapTypes.FromSoapLong(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //----------------------------------------------------------------------------
        bool ParseFmtSign(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.SIGN) &&
                lex.ParseInt(out i);

            Sign = (SignTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDataStyle(Parser lex)
        {
            if (!lex.ParseTag(Token.STYLE))
                return false;

            int i = 0;
            if (!lex.ParseInt(out i))
                return false;

            FormatType = (FormatTag)i;
            if (FormatType != FormatTag.ZERO_AS_DASH || lex.LookAhead() != Token.NULL)
                return true;

            return lex.ParseTag(Token.NULL) && lex.ParseString(out AsZeroValue);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtThousand(Parser lex)
        {
            return
                lex.ParseTag(Token.THOUSAND) &&
                lex.ParseString(out ThousSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTable(Parser lex)
        {
            return lex.ParseTag(Token.TABLE) && lex.ParseString(out XTable);
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;
            ZeroPadded = false;

            do switch (lex.LookAhead())
                {
                    case Token.PADDED: ok = ZeroPadded = lex.ParseTag(Token.PADDED); break;
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.SIGN: ok = ParseFmtSign(lex); break;
                    case Token.STYLE: ok = ParseFmtDataStyle(lex); break;
                    case Token.THOUSAND: ok = ParseFmtThousand(lex); isThousSeparatorDefault = !ok; break;
                    case Token.TABLE: ok = ParseFmtTable(lex); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override void SetToLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (ThousSeparator == "." && isThousSeparatorDefault)
                ThousSeparator = FormatStyles.ApplicationLocale.NumberFormat.NumberGroupSeparator;
        }

        //------------------------------------------------------------------------------
        public override void RestoreFromLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (string.Compare(ThousSeparator, FormatStyles.ApplicationLocale.NumberFormat.NumberGroupSeparator, StringComparison.OrdinalIgnoreCase) == 0 && isThousSeparatorDefault)
                ThousSeparator = "";
        }
    }

    /// <summary>
    /// Summary description for IntFormatter.
    /// </summary>
    /// ================================================================================
    public class IntFormatter : LongFormatter
    {
        public string Format(int aValue) { return Format(aValue, true); }
        public string Format(int aValue, bool padEnabled) { return base.Format(aValue, padEnabled).ToString(); }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                int converted = SoapTypes.FromSoapInt(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }
    }

    /// <summary>
    /// Summary description for StringFormatter.
    /// </summary>
    /// ================================================================================
    public class StringFormatter : Formatter
    {
        const char MaskedYearChar = 'Y';
        const char MaskedNumberChar = '#';

        [Flags]
        public enum FormatTag
        {
            ASIS = 0x0000,
            UPPERCASE = 0x0001,
            LOWERCASE = 0x0002,
            CAPITALIZED = 0x0003,
            EXPANDED = 0x0004,
            MASKED = 0x0005
        }

        private bool isIrrilevantMask = false;
        private string mask = string.Empty;
        internal string InterChars = " ";   //stringa spaziatore
        internal FormatTag FormatType = FormatTag.ASIS;

        public string Mask { get { return mask; } set { mask = value; } }

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            string s = data as string;
            if (s == null)
                s = data.ToString();
            return Format(s);
        }

        //----------------------------------------------------------------------------
        bool IsLower(char ch)
        {
            return ch == char.ToLower(ch);
        }

        //----------------------------------------------------------------------------
        bool IsMaskApplied(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Length == mask.Length;
        }

        //----------------------------------------------------------------------------
        void ApplyMask(string text, StringBuilder result)
        {
            if (isIrrilevantMask || IsMaskApplied(text))
            {
                result.Append(text);
                return;
            }

            int nYear = 0;
            int nNumbers = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == MaskedYearChar)
                    nYear++;
                else if (mask[i] == MaskedNumberChar)
                    nNumbers++;
            }

            long lNr = text.Length > nYear ? Int32.Parse(text.Substring(nYear)) : Int32.Parse(text);
            // non c'è numero
            if (lNr == 0)
            {
                result.Append(text);
                return;
            }

            string sYear = string.Format("%d", DateTime.Now.Year /*TODO AfxGetApplicationYear()*/);
            string sNumber = ZeroPadded ? string.Format("%0" + string.Format("{0}", nNumbers) + "d", lNr) : string.Format("{0}", lNr);

            int nAppliedYear = sYear.Length - nYear;
            nNumbers = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == MaskedYearChar)
                {
                    result.Append(sYear[nAppliedYear]);
                    nAppliedYear++;
                }
                else if (mask[i] == MaskedNumberChar)
                {
                    result.Append(sNumber[nNumbers]);
                    nNumbers++;
                }
                else
                    result.Append(result[i]);
            }
        }

        //----------------------------------------------------------------------------
        public override string Format(string aValue) { return Format(aValue, true); }
        public string Format(string aValue, bool padEnabled)
        {
            StringBuilder result = new StringBuilder();
            switch (FormatType)
            {
                case FormatTag.ASIS:
                    result.Append(aValue.ToString());
                    break;
                case FormatTag.UPPERCASE:
                    result.Append(aValue.ToUpper());
                    break;
                case FormatTag.LOWERCASE:
                    result.Append(aValue.ToLower());
                    break;
                case FormatTag.CAPITALIZED:
                    //se e'una lettera minuscola ed e'la prima o segue un carattere non alfabetico, upperizzo
                    for (int i = 0; i < aValue.Length; i++)
                    {
                        if (
                                (char.IsLetter(aValue[i]) && IsLower(aValue[i])) &&
                                (i == 0 || !char.IsLetter(aValue[i - 1]))
                            )
                            result.Append(char.ToUpper(aValue[i]));
                        else
                            result.Append(aValue[i]);
                    }
                    break;
                case FormatTag.MASKED:
                    ApplyMask(aValue, result);
                    break;
                case FormatTag.EXPANDED:
                    for (int i = 0; i < aValue.Length; i++)
                    {
                        result.Append(aValue[i]);
                        if (i < (aValue.Length - 1)) result.Append(InterChars);
                    }
                    break;
            }

            if (!padEnabled)
                return result.ToString();

            result.Append(Tail);
            result.Insert(0, Head);
            return Padder(result.ToString(), Align != AlignType.RIGHT);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDataStyle(Parser lex)
        {
            if (!lex.ParseTag(Token.STYLE))
                return false;

            int i = 0;
            if (!lex.ParseInt(out i))
                return false;

            FormatType = (FormatTag)i;
            if (FormatType == FormatTag.EXPANDED)
                return lex.ParseString(out InterChars);
            else if (FormatType == FormatTag.MASKED)
            {
                if (lex.ParseString(out mask))
                {
                    if (lex.LookAhead() == Token.PADDED)
                        ZeroPadded = lex.ParseTag(Token.PADDED);
                }
            }
            return true;
        }

        //------------------------------------------------------------------------------
        public override Formatter.AlignType GetDefaultAlign()
        {
            return AlignType.LEFT;
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;
            do switch (lex.LookAhead())
                {
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.STYLE: ok = ParseFmtDataStyle(lex); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                string converted = SoapTypes.FromSoapString(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }
    }

    /// <summary>
    /// Summary description for TextFormatter.
    /// </summary>
    /// ================================================================================
    public class TextFormatter : StringFormatter
    {
    }

    /// <summary>
    /// Summary description for DoubleFormatter. 
    /// </summary> 
    /// ================================================================================			
    public class DoubleFormatter : Formatter
    {
        //----------------------------------------------------------------------------
        private bool isThousSeparatorDefault = true;
        private bool isDecSeparatorDefault = true;

        public enum RoundingTag
        {
            ROUND_NONE, ROUND_ABS, ROUND_SIGNED,
            ROUND_ZERO, ROUND_INF
        }

        public enum SignTag
        {
            ABSOLUTEVAL, MINUSPREFIX, MINUSPOSTFIX,
            ROUNDS, SIGNPREFIX, SIGNPOSTFIX
        }

        public enum FormatTag
        {
            FIXED, EXPONENTIAL, ENGINEER, ENCODED,
            LETTER, ZERO_AS_DASH = 0x0099
        }

        protected const int DBL_DIG = 15;
        protected const int SIGNIFICANT_DECIMAL = 7;

        public RoundingTag Rounding { get; set; } = RoundingTag.ROUND_NONE;
        public SignTag Sign { get; set; } = SignTag.MINUSPREFIX;
        public FormatTag FormatType { get; set; } = FormatTag.FIXED;
        public double Quantum = 0.0;
        public string ThousSeparator = ".";
        public string refThousSeparator { get { return ThousSeparator; } }
public string DecSeparator = ",";
        public string refDecSeparator { get { return DecSeparator; } }
        public bool ShowMSZero = true;
        public bool ShowLSZero = true;
        public string XTable = "ZAEGHMPSTK,";   //la virgola e' all'ultima posizione
        public int DecNumber = 2;
        public int refDecNumber { get { return DecNumber; } }

        public string AsZeroValue = "--";

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            return Format((double)data);
        }

        //----------------------------------------------------------------------------
        public string Format(double aValue) { return Format(aValue, true); }
        public string Format(double aValue, bool padEnabled)
        {
            bool sign = (aValue >= 0);
            aValue = Math.Round(aValue, DecNumber);
            aValue = Math.Abs(aValue);              //tolgo il segno, lo poi metto a seconda della richiesta

            bool asZero = (FormatType == FormatTag.ZERO_AS_DASH && aValue == 0);

            if (Quantum != 0 && Rounding != RoundingTag.ROUND_NONE)
                aValue = RoundingValue(aValue, Quantum, Rounding);//arrotondamento richiesto

            StringBuilder result = new StringBuilder();
            //result.Append(Head);						
            switch (Sign)                               //sign prefix
            {
                case SignTag.SIGNPREFIX:
                case SignTag.MINUSPREFIX:
                    if (!sign) result.Append("-");
                    else if (!asZero && Sign == SignTag.SIGNPREFIX) result.Append("+");
                    break;
                case SignTag.ROUNDS:
                    if (!sign) result.Append("("); break;
            }
            string valueResult;
            switch (FormatType)
            {
                case FormatTag.FIXED:
                case FormatTag.ZERO_AS_DASH:
                    if (asZero && padEnabled) result.Append(AsZeroValue);
                    else result.Append(ValueToAppend(aValue, FormatTag.FIXED));
                    break;
                case FormatTag.LETTER:
                    result.Append(NumberToWord((long)Math.Floor(aValue)));
                    result.Append(ValueToAppend(aValue, FormatTag.LETTER));
                    break;
                case FormatTag.ENCODED:
                    result.Append(ValueToAppend(aValue, FormatTag.ENCODED));
                    //replace dei numeri e del decseparator
                    for (int i = 0; i < XTable.Length; i++)
                    {
                        result.Replace(i.ToString(), XTable[i].ToString());
                        result.Replace(DecSeparator, XTable[XTable.Length - 1].ToString());
                    }
                    break;
                case FormatTag.EXPONENTIAL:
                    valueResult = ValueToAppend(aValue, FormatTag.EXPONENTIAL);
                    //lo zero iniziale lo mette di default, se richiesto viene eliminato
                    if (valueResult.Substring(0, 1) == "0" && !ShowMSZero) result.Append(valueResult.Remove(0, 1));
                    else result.Append(valueResult);
                    break;
                case FormatTag.ENGINEER:
                    result.Append(ValueToAppend(aValue, FormatTag.ENGINEER));
                    result.Replace("potZero", "0");
                    break;
            }
            switch (Sign)                               //sign postfix
            {
                case SignTag.SIGNPOSTFIX:
                case SignTag.MINUSPOSTFIX:
                    if (!sign) result.Append("-");
                    else if (!asZero && Sign == SignTag.SIGNPOSTFIX) result.Append("+");
                    break;
                case SignTag.ROUNDS:
                    if (!sign) result.Append(")");
                    break;
            }
            if (!padEnabled)
                return result.ToString();
            result.Append(Tail);
            result.Insert(0, Head);
            return Padder(result.ToString(), Align != AlignType.RIGHT);
        }


        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                double converted = double.Parse(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                double converted = SoapTypes.FromSoapDouble(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //restituisce value semi-formattato 
        //----------------------------------------------------------------------------
        string ValueToAppend(double aValue, FormatTag format)
        {
            //uso la mia personalizzazione di formattazione
            NumberFormatInfo customFormat = new NumberFormatInfo();

            if (DecSeparator != null && DecSeparator != " ")
                customFormat.NumberDecimalSeparator = DecSeparator;
            //se i due separatori sono uguali tolgo quello delle migliaia
            if (ThousSeparator != null && ThousSeparator != DecSeparator)
                customFormat.NumberGroupSeparator = ThousSeparator;

            StringBuilder placeDecObl = new StringBuilder();
            StringBuilder placeDecAny = new StringBuilder();
            placeDecObl.Append('0', DecNumber);     //tanti 0 quante sono le cifre decimali
            placeDecAny.Append('#', DecNumber);     //tanti # quante sono le cifre decimali

            string placeObl = "0";                      //zero iniziale
            string placeAny = "#";                      //no zero iniziale
            string thSep = (ThousSeparator != null && ThousSeparator != DecSeparator) ? "," : "";
            string decSep = ".";

            switch (format)
            {
                case FormatTag.FIXED:
                case FormatTag.ENCODED:
                    if (ShowMSZero && ShowLSZero) return (aValue.ToString(placeAny + thSep + placeObl + decSep + placeDecObl, customFormat));
                    if (!ShowMSZero && ShowLSZero) return (aValue.ToString(placeAny + thSep + placeAny + decSep + placeDecObl, customFormat));
                    if (ShowMSZero && !ShowLSZero) return (aValue.ToString(placeAny + thSep + placeObl + decSep + placeDecAny, customFormat));
                    if (!ShowMSZero && !ShowLSZero) return (aValue.ToString(placeAny + thSep + placeAny + decSep + placeDecAny, customFormat));
                    break;

                case FormatTag.LETTER://considero solo la parte decimale
                    double decimalPortion = aValue - (long)Math.Floor(aValue);
                    return ShowLSZero
                            ? decimalPortion.ToString(decSep + placeDecObl, customFormat)
                            : decimalPortion.ToString(decSep + placeDecAny, customFormat);

                case FormatTag.EXPONENTIAL://non considero thSep
                    if (ShowMSZero && ShowLSZero) return (aValue.ToString(placeObl + decSep + placeDecObl + "E+0", customFormat));
                    if (!ShowMSZero && ShowLSZero) return (aValue.ToString(placeAny + decSep + placeDecObl + "E+0", customFormat));
                    if (ShowMSZero && !ShowLSZero) return (aValue.ToString(placeObl + decSep + placeDecAny + "E+0", customFormat));
                    if (!ShowMSZero && !ShowLSZero) return (aValue.ToString(placeAny + decSep + placeDecAny + "E+0", customFormat));
                    break;

                case FormatTag.ENGINEER://non considero thSep
                    {
                        int cont = 0;
                        string signE = "";//se e' meno, cé'il segno di cont
                                          //per valori >1 e =0, l'esponente e' positivo
                        if (aValue >= 1.0 || aValue == 0.0)
                        {
                            while (aValue >= 1000)
                            { aValue /= 1000; cont++; }
                            signE = "+";
                        }
                        //per valori compresi tra zero e uno, l'esponente e'negativo
                        else
                            while (aValue < 1.0)
                            { aValue *= 1000; cont--; }

                        double expE = (Math.Pow(10, (double)DecNumber));
                        //tronco il numero alle cifre decimali richieste
                        aValue = (Math.Floor(aValue * expE)) / expE;
                        string potZero = "";
                        //se cont=0, ha comportamento indesiderato
                        if (cont != 0) { cont *= 3; potZero = cont.ToString(); }
                        else { potZero = "potZero"; signE = "+"; }

                        if (ShowMSZero && ShowLSZero) return (aValue.ToString(placeObl + decSep + placeDecObl + "E" + signE + potZero, customFormat));
                        if (!ShowMSZero && ShowLSZero) return (aValue.ToString(placeAny + decSep + placeDecObl + "E" + signE + potZero, customFormat));
                        if (ShowMSZero && !ShowLSZero) return (aValue.ToString(placeObl + decSep + placeDecAny + "E" + signE + potZero, customFormat));
                        if (!ShowMSZero && !ShowLSZero) return (aValue.ToString(placeAny + decSep + placeDecAny + "E" + signE + potZero, customFormat));
                        break;
                    }
            }
            if (DecSeparator == " ") return aValue.ToString().Replace(customFormat.NumberDecimalSeparator, " ");
            return aValue.ToString();

        }

        //----------------------------------------------------------------------------
        double RoundingValue(double nValue, double quantum, RoundingTag roundType)
        {
            int sign = (nValue >= 0) ? 1 : -1;
            double nQuantum = quantum;
            double e = 1;

            if (roundType != RoundingTag.ROUND_SIGNED) nValue = Math.Abs(nValue);
            while (Math.Floor(nQuantum) < nQuantum)
            {
                nQuantum *= 10;
                nValue *= 10;
                e *= 10;
            }

            switch (roundType)
            {
                case RoundingTag.ROUND_SIGNED: return Math.Floor(nValue + (nQuantum / 2)) / e;
                case RoundingTag.ROUND_ABS: return sign * Math.Floor(nValue + (nQuantum / 2)) / e;
                case RoundingTag.ROUND_INF: return sign * nQuantum * Math.Ceiling(nValue / nQuantum) / e;
                case RoundingTag.ROUND_ZERO: return sign * nQuantum * Math.Floor(nValue / nQuantum) / e;
                default: return sign * nValue / e;
            }
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDataStyle(Parser lex)
        {
            if (!lex.ParseTag(Token.STYLE))
                return false;

            int i = 0;
            if (!lex.ParseInt(out i))
                return false;

            FormatType = (FormatTag)i;
            if (FormatType != FormatTag.ZERO_AS_DASH || lex.LookAhead() != Token.NULL)
                return true;

            return lex.ParseTag(Token.NULL) && lex.ParseString(out AsZeroValue);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtRound(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.ROUND) &&
                lex.ParseInt(out i) &&
                lex.ParseDouble(out Quantum);

            Rounding = (RoundingTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtSign(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.SIGN) &&
                lex.ParseInt(out i);

            Sign = (SignTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTable(Parser lex)
        {
            return
                lex.ParseTag(Token.TABLE) &&
                lex.ParseString(out XTable);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtThousand(Parser lex)
        {
            return
                lex.ParseTag(Token.THOUSAND) &&
                lex.ParseString(out ThousSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtSep(Parser lex)
        {
            return
                lex.ParseTag(Token.SEPARATOR) &&
                lex.ParseString(out DecSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDecimal(Parser lex)
        {
            bool ok =
                lex.ParseTag(Token.PRECISION) &&
                lex.ParseInt(out DecNumber);

            if ((DecNumber < 0) || (DecNumber > DBL_DIG))
            {
                lex.SetError(ApplicationsStrings.BadPrecision);
                return false;
            }
            return ok;
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;

            ShowMSZero = true;
            ShowLSZero = true;

            do switch (lex.LookAhead())
                {
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.STYLE: ok = ParseFmtDataStyle(lex); break;
                    case Token.ROUND: ok = ParseFmtRound(lex); break;
                    case Token.THOUSAND: ok = ParseFmtThousand(lex); isThousSeparatorDefault = !ok; break;
                    case Token.SEPARATOR: ok = ParseFmtSep(lex); isDecSeparatorDefault = !ok; break;
                    case Token.PRECISION: ok = ParseFmtDecimal(lex); break;
                    case Token.SIGN: ok = ParseFmtSign(lex); break;
                    case Token.TABLE: ok = ParseFmtTable(lex); break;

                    case Token.HIDE_MS0: ShowMSZero = false; ok = lex.ParseTag(Token.HIDE_MS0); break;
                    case Token.HIDE_LS0: ShowLSZero = false; ok = lex.ParseTag(Token.HIDE_LS0); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);

            if (ThousSeparator == DecSeparator)
            {
                lex.SetError(ApplicationsStrings.BadSeparator);
                return false;
            }
            return ok;
        }

        //------------------------------------------------------------------------------
        public override void SetToLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (ThousSeparator == "." && isThousSeparatorDefault)
                ThousSeparator = FormatStyles.ApplicationLocale.NumberFormat.NumberGroupSeparator;

            if (DecSeparator == "," && isDecSeparatorDefault)
                DecSeparator = FormatStyles.ApplicationLocale.NumberFormat.NumberDecimalSeparator;
        }

        //------------------------------------------------------------------------------
        public override void RestoreFromLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (isThousSeparatorDefault)
                ThousSeparator = ".";

            if (isDecSeparatorDefault)
                DecSeparator = ",";
        }
    }

    /// <summary>
    /// Summary description for MoneyFormatter.
    /// </summary>
    /// ================================================================================
    public class MoneyFormatter : DoubleFormatter
    {
    }

    /// <summary>
    /// Summary description for QuantityFormatter.
    /// </summary>
    /// ================================================================================
    public class QuantityFormatter : DoubleFormatter
    {
    }

    /// <summary>
    /// Summary description for PercentFormatter.
    /// </summary>
    /// ================================================================================
    public class PercentFormatter : DoubleFormatter
    {
    }

    /// <summary>
    /// Summary description for DateFormatter.
    /// </summary>
    /// ================================================================================
    public class DateTimeFormatter : Formatter
    {
        //----------------------------------------------------------------------------
        private bool isFormatTypeDefault = true;

        public enum GeneralTag { DATETIME, TIME, DATE }
        public enum OrderTag { DATE_DMY, DATE_MDY, DATE_YMD }
        public enum DayDigitTag { DAY99, DAYB9, DAY9 }
        public enum MonthDigitTag { MONTH99, MONTHB9, MONTH9, MONTH999, MONTH9999 }
        public enum YearDigitTag { YEAR99, YEAR999, YEAR9999 }
        public enum WeekdayTag { NOWEEKDAY, PREFIXWEEKDAY, POSTFIXWEEKDAY }
        // TT SONO AMDesignator E PMDesignator
        // HH SONO LE ORE, SE BH SI INTENDE CHE L'ORA É ESPRIMIBILE CON UNA SOLA CIFRA ALLINEATA A DESTRA.
        public enum HourDigitTag
        {
            TIME_NONE = 0x0000,
            TIME_HF99 = 0x0001,
            TIME_HFB9 = 0x0002,
            TIME_HF9 = 0x0003,
            TIME_AMPM = 0x0010,
            TIME_ONLY = 0x0020,
            TIME_NOSEC = 0x0040,
            HHMMTT = TIME_HF99 | TIME_NOSEC | TIME_AMPM,
            BHMMTT = TIME_HFB9 | TIME_NOSEC | TIME_AMPM,
            HMMTT = TIME_HF9 | TIME_NOSEC | TIME_AMPM,
            HHMMSSTT = TIME_HF99 | TIME_AMPM,
            BHMMSSTT = TIME_HFB9 | TIME_AMPM,
            HMMSSTT = TIME_HF9 | TIME_AMPM,
            HHMM = TIME_HF99 | TIME_NOSEC,
            BHMM = TIME_HFB9 | TIME_NOSEC,
            HMM = TIME_HF9 | TIME_NOSEC,
            HHMMSS = TIME_HF99,
            BHMMSS = TIME_HFB9,
            HMMSS = TIME_HF9
        }

        public GeneralTag GeneralFormat { get; set; } = GeneralTag.DATE;
        public WeekdayTag WeekdayFormat { get; set; } = WeekdayTag.NOWEEKDAY;
        public DayDigitTag DayFormat { get; set; } = DayDigitTag.DAY99;
        public MonthDigitTag MonthFormat { get; set; } = MonthDigitTag.MONTH99;
        public YearDigitTag YearFormat { get; set; } = YearDigitTag.YEAR99;
        public OrderTag FormatType { get; set; } = OrderTag.DATE_DMY;
        public HourDigitTag TimeFormat { get; set; } = HourDigitTag.TIME_NONE;
        public string FirstSeparator = "/";
        public string SecondSeparator = "/";
        public string TimeSeparator = ":";
        public string TimeAM = "AM";
        public string TimePM = "PM";

        public string refFirstSeparator { get { return FirstSeparator; } }
        public string refSecondSeparator { get { return SecondSeparator; } }
        public string refTimeSeparator { get { return TimeSeparator; } }
        public string refTimeAM { get { return TimeAM; } }
        public string refTimePM { get { return TimePM; } }

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            return Format((DateTime)data);
        }

        //----------------------------------------------------------------------------
        public virtual string Format(DateTime aValue)
        {
            return Format(aValue, true);
        }

        //----------------------------------------------------------------------------
        public virtual string Format(DateTime aValue, bool padEnabled)
        {
            // null value di TB viene salvato anche sul database  quindi occorre visualizzarlo vuoto
            if (aValue == ObjectHelper.NullTbDateTime)
                return "";

            StringBuilder result = new StringBuilder();
            StringBuilder work = new StringBuilder();
            DateTimeFormatInfo customFormat = new DateTimeFormatInfo();

            //personalizzazione del formato:
            customFormat.AbbreviatedDayNames = new String[7]
            {
                    ApplicationsStrings.Domenica,
                    ApplicationsStrings.Lunedi,
                    ApplicationsStrings.Martedi,
                    ApplicationsStrings.Mercoledi,
                    ApplicationsStrings.Giovedi,
                    ApplicationsStrings.Venerdi,
                    ApplicationsStrings.Sabato,
            };
            customFormat.AbbreviatedMonthNames = new String[13]
            {
                ApplicationsStrings.Gennaio,
                ApplicationsStrings.Febbraio,
                ApplicationsStrings.Marzo,
                ApplicationsStrings.Aprile,
                ApplicationsStrings.Maggio,
                ApplicationsStrings.Giugno,
                ApplicationsStrings.Luglio,
                ApplicationsStrings.Agosto,
                ApplicationsStrings.Settembre,
                ApplicationsStrings.Ottobre,
                ApplicationsStrings.Novembre,
                ApplicationsStrings.Dicembre,
                ""
            };
            customFormat.DayNames = new String[7]
            {
                ApplicationsStrings.Domenica,
                ApplicationsStrings.Lunedi,
                ApplicationsStrings.Martedi,
                ApplicationsStrings.Mercoledi,
                ApplicationsStrings.Giovedi,
                ApplicationsStrings.Venerdi,
                ApplicationsStrings.Sabato
            };
            customFormat.MonthNames = new String[13]
            {
                ApplicationsStrings.Gennaio,
                ApplicationsStrings.Febbraio,
                ApplicationsStrings.Marzo,
                ApplicationsStrings.Aprile,
                ApplicationsStrings.Maggio,
                ApplicationsStrings.Giugno,
                ApplicationsStrings.Luglio,
                ApplicationsStrings.Agosto,
                ApplicationsStrings.Settembre,
                ApplicationsStrings.Ottobre,
                ApplicationsStrings.Novembre,
                ApplicationsStrings.Dicembre,
                ""
            };
            //if (TimeSeparator != null)	customFormat.TimeSeparator = TimeSeparator;   TODO rsweb
            if (TimeAM != null) customFormat.AMDesignator = TimeAM;
            if (TimePM != null) customFormat.PMDesignator = TimePM;

            //formattazione data se ricevo la richiesta di un Date o di un DateTime
            if (GeneralFormat != GeneralTag.TIME)
            {
                bool dayShort = (aValue.Day < 10);
                bool monthShort = (aValue.Month < 10);
                if (WeekdayFormat == WeekdayTag.PREFIXWEEKDAY) work.Append("dddd ");
                switch (FormatType)
                {
                    case OrderTag.DATE_DMY:
                        AppendFormatString(DayFormat, work, dayShort);
                        work.Append(FirstSeparator);
                        AppendFormatString(MonthFormat, work, monthShort);
                        work.Append(SecondSeparator);
                        AppendFormatString(YearFormat, work);
                        break;
                    case OrderTag.DATE_MDY:
                        AppendFormatString(MonthFormat, work, monthShort);
                        work.Append(FirstSeparator);
                        AppendFormatString(DayFormat, work, dayShort);
                        work.Append(SecondSeparator);
                        AppendFormatString(YearFormat, work);
                        break;
                    case OrderTag.DATE_YMD:
                        AppendFormatString(YearFormat, work);
                        work.Append(FirstSeparator);
                        AppendFormatString(MonthFormat, work, monthShort);
                        work.Append(SecondSeparator);
                        AppendFormatString(DayFormat, work, dayShort);
                        break;
                }
                if (WeekdayFormat == WeekdayTag.POSTFIXWEEKDAY) work.Append(" dddd");
            }
            if (GeneralFormat == GeneralTag.DATETIME) work.Append(" ");
            //formattazione orario se ricevo richiesta di un Time o di un DateTime
            if (GeneralFormat != GeneralTag.DATE)
            {
                bool setTT = false;
                string hourRange;   //H range 0-23, h range 1-12
                                    //se il risultato dello shift é dispari si deve visualizzare am-pm
                if ((((int)TimeFormat >> 4) % 2) == 1) { hourRange = "h"; setTT = true; }
                else hourRange = "H";
                switch (TimeFormat)
                {
                    case HourDigitTag.HHMMSS:
                    case HourDigitTag.HHMMSSTT:
                    case HourDigitTag.HHMMTT:
                    case HourDigitTag.HHMM:
                        work.Append(hourRange + hourRange);
                        break;
                    case HourDigitTag.BHMMSS:
                    case HourDigitTag.BHMMSSTT:
                    case HourDigitTag.BHMMTT:
                    case HourDigitTag.BHMM:
                        if ((aValue.Hour < 10 && !setTT) || ((setTT && aValue.Hour > 0)))
                            work.Append(" " + hourRange);
                        //se l'ora e'nel range 1-12, l'ora 0 viene scritta come 12 e non devo allineare a dx
                        else work.Append(hourRange + hourRange);
                        break;
                    case HourDigitTag.HMMSS:
                    case HourDigitTag.HMMSSTT:
                    case HourDigitTag.HMMTT:
                    case HourDigitTag.HMM:
                        work.Append(hourRange);
                        break;
                }
                if (work.Length != 0)
                    work.Append(":");

                work.Append("mm");
                //se il risultato dello shift é zero vanno visualizzati i secondi
                if (((int)TimeFormat >> 6) == 0) work.Append(":ss");
                if (setTT) work.Append("tt");
            }
            result.Append(aValue.ToString(work.ToString(), customFormat));

            if (!padEnabled)
                return result.ToString();
            result.Append(Tail);
            result.Insert(0, Head);
            return Padder(result.ToString(), Align != AlignType.RIGHT);
        }


        // date mal definite vengono formattate come stringa vuota
        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                DateTime converted = DateTime.Parse(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                DateTime converted = SoapTypes.FromSoapDateTime(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //----------------------------------------------------------------------------
        private StringBuilder AppendFormatString(DayDigitTag format, StringBuilder work, bool typeShort)
        {
            if (format == DayDigitTag.DAY9) return work.Append("d");
            if (format == DayDigitTag.DAYB9 && typeShort) return work.Append(" d");
            return work.Append("dd");
        }
        //----------------------------------------------------------------------------
        private StringBuilder AppendFormatString(MonthDigitTag format, StringBuilder work, bool typeShort)
        {
            switch (format)
            {
                case MonthDigitTag.MONTH9:
                    work.Append("M");
                    break;
                case MonthDigitTag.MONTH99:
                    work.Append("MM");
                    break;
                case MonthDigitTag.MONTHB9:
                    if (typeShort) work.Append(" M");
                    else work.Append("MM");
                    break;
                case MonthDigitTag.MONTH999:
                    work.Append("MMM");
                    break;
                case MonthDigitTag.MONTH9999:
                    work.Append("MMMM");
                    break;
            }
            return work;
        }
        //----------------------------------------------------------------------------
        private StringBuilder AppendFormatString(YearDigitTag format, StringBuilder work)
        {
            work.Append("yy");
            if (format == YearDigitTag.YEAR999) return work.Append("y");
            if (format == YearDigitTag.YEAR9999) return work.Append("yy");
            return work;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtOrder(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.ORDER) &&
                lex.ParseInt(out i);

            FormatType = (OrderTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtWeekDay(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.WEEKDAY) &&
                lex.ParseInt(out i);

            WeekdayFormat = (WeekdayTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDayFmt(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.DAY) &&
                lex.ParseInt(out i);
            DayFormat = (DayDigitTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtMonthFmt(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.MONTH) &&
                lex.ParseInt(out i);

            MonthFormat = (MonthDigitTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtYearFmt(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.YEAR) &&
                lex.ParseInt(out i);
            YearFormat = (YearDigitTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtFirstSep(Parser lex)
        {
            return
                lex.ParseTag(Token.BEFORE) &&
                lex.ParseTag(Token.MONTH) &&
                lex.ParseString(out FirstSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtSecondSep(Parser lex)
        {
            return
                lex.ParseTag(Token.AFTER) &&
                lex.ParseTag(Token.MONTH) &&
                lex.ParseString(out SecondSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeFmt(Parser lex)
        {
            int i = 0;
            bool ok = true;
            if (lex.LookAhead() == Token.TIME)
                ok = lex.ParseTag(Token.TIME);

            ok = ok && lex.ParseTag(Token.STYLE) &&
                    lex.ParseInt(out i) &&
                    ParseFmtTimeSep(lex) &&
                    ParseFmtTimeAMPM(lex);
            TimeFormat = (HourDigitTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeSep(Parser lex)
        {
            if (lex.LookAhead() != Token.SEPARATOR) return true;

            return
                lex.ParseTag(Token.SEPARATOR) &&
                lex.ParseString(out TimeSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeAMPM(Parser lex)
        {
            if (lex.LookAhead() != Token.POSTFIX) return true;

            return
                lex.ParseTag(Token.POSTFIX) &&
                lex.ParseString(out TimeAM) &&
                lex.ParseString(out TimePM);
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;

            do switch (lex.LookAhead())
                {
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.ORDER: ok = ParseFmtOrder(lex); isFormatTypeDefault = !ok; break;
                    case Token.WEEKDAY: ok = ParseFmtWeekDay(lex); break;
                    case Token.DAY: ok = ParseFmtDayFmt(lex); break;
                    case Token.MONTH: ok = ParseFmtMonthFmt(lex); break;
                    case Token.YEAR: ok = ParseFmtYearFmt(lex); break;
                    case Token.BEFORE: ok = ParseFmtFirstSep(lex); break;
                    case Token.AFTER: ok = ParseFmtSecondSep(lex); break;
                    case Token.STYLE:
                    case Token.TIME: ok = ParseFmtTimeFmt(lex); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override void SetToLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (FormatType == OrderTag.DATE_DMY && isFormatTypeDefault)
            {
                string pattern = FormatStyles.ApplicationLocale.DateTimeFormat.ShortDatePattern;
                InitFormatTypeFromDatePattern(pattern);
            }
        }

        //------------------------------------------------------------------------------
        public void InitFormatTypeFromDatePattern(string pattern)
        {
            pattern = pattern.ToLower();
            int yearPos = -1, yearLength = 0;
            int monthPos = -1, monthLength = 0;
            int dayPos = -1, dayLength = 0;

            for (int i = 0; i < pattern.Length; i++)
            {
                switch (pattern[i])
                {
                    case 'y':
                        yearLength++;
                        if (yearPos == -1)
                            yearPos = i;
                        break;
                    case 'm':
                        monthLength++;
                        if (monthPos == -1)
                            monthPos = i;
                        break;
                    case 'd':
                        dayLength++;
                        if (dayPos == -1)
                            dayPos = i;
                        break;
                }

            }

            if (yearPos < monthPos)
                FormatType = OrderTag.DATE_YMD;
            else if (dayPos < monthPos)
                FormatType = OrderTag.DATE_DMY;
            else
                FormatType = OrderTag.DATE_MDY;

            switch (yearLength)
            {
                case 4: YearFormat = YearDigitTag.YEAR9999; break;
                case 3: YearFormat = YearDigitTag.YEAR999; break;
                case 2: YearFormat = YearDigitTag.YEAR99; break;
            }

            switch (monthLength)
            {
                case 4: MonthFormat = MonthDigitTag.MONTH9999; break;
                case 3: MonthFormat = MonthDigitTag.MONTH999; break;
                case 2: MonthFormat = MonthDigitTag.MONTH99; break;
                case 1: MonthFormat = MonthDigitTag.MONTH9; break;
            }

            switch (dayLength)
            {
                case 2: DayFormat = DayDigitTag.DAY99; break;
                case 1: DayFormat = DayDigitTag.DAY9; break;
            }
        }

        //------------------------------------------------------------------------------
        public override void RestoreFromLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (isFormatTypeDefault)
                FormatType = OrderTag.DATE_DMY;
        }
    }

    /// <summary>
    /// Summary description for DateTimeFormatter.
    /// </summary>
    /// ================================================================================
    public class DateFormatter : DateTimeFormatter
    {
        public new string Format(DateTime aValue) { return Format(aValue, true); }
        public new string Format(DateTime aValue, bool padEnabled)
        {
            GeneralFormat = GeneralTag.DATE;
            return base.Format(aValue, padEnabled).ToString();
        }

        //----------------------------------------------------------------------------
        internal bool IsTimeAMPMFormat()
        {
            return TimeFormat == HourDigitTag.TIME_AMPM;
        }

        //----------------------------------------------------------------------------
        internal bool IsFullDateTimeFormat()
        {
            return DataType == Token.DATETIME && TimeFormat != HourDigitTag.TIME_NONE;
        }
    }

    /// <summary>
    /// Summary description for TimeFormatter.
    /// </summary>
    /// ================================================================================
    public class TimeFormatter : DateTimeFormatter
    {
        public override string Format(DateTime aValue) { return Format(aValue, true); }
        public override string Format(DateTime aValue, bool padEnabled)
        {
            GeneralFormat = GeneralTag.TIME;
            TimeFormat = HourDigitTag.HHMM;
            return base.Format(aValue, padEnabled).ToString();
        }
    }

    /// <summary>
    /// Summary description for ElapsedTimeFormatter.
    /// </summary>
    /// ================================================================================
    public class ElapsedTimeFormatter : Formatter
    {
        //----------------------------------------------------------------------------
        private bool isDecSeparatorDefault = true;
        public static int Precision = 1;

        public int refPrecision { get { return Precision; } }

        public enum FormatTag
        {
            TIME_D = 0X0001, TIME_H = 0X0002, TIME_M = 0x0004, TIME_S = 0x0008,
            TIME_CH = 0x0010, TIME_C = 0x1000, TIME_F = 0x2000,
            TIME_DEC = (TIME_C | TIME_F | TIME_CH),
            TIME_DHMS = (TIME_D | TIME_H | TIME_M | TIME_S),
            TIME_DHMSF = (TIME_DHMS | TIME_F),
            TIME_DHMCM = (TIME_DHMS | TIME_C),
            TIME_DHM = (TIME_D | TIME_H | TIME_M),
            TIME_DHCH = (TIME_DHM | TIME_C),
            TIME_DH = (TIME_D | TIME_H),
            TIME_DCD = (TIME_DH | TIME_C),
            TIME_HMS = (TIME_H | TIME_M | TIME_S),
            TIME_HMSF = (TIME_HMS | TIME_F),
            TIME_HMCM = (TIME_HMS | TIME_C),
            TIME_HM = (TIME_H | TIME_M),
            TIME_HCH = (TIME_HM | TIME_C),
            TIME_MS = (TIME_M | TIME_S),
            TIME_MSF = (TIME_MS | TIME_F),
            TIME_MCM = (TIME_MS | TIME_C),
            TIME_SF = (TIME_S | TIME_F)
        }

        protected const int DBL_DIG = 15;

        public string TimeSeparator = ":";

        public string refTimeSeparator {get { return TimeSeparator; } }
        public string DecSeparator = ",";
        public string refDecSeparator { get { return DecSeparator; } }

        public int DecNumber = 2;

        public int refDecNumber { get { return DecNumber; } } 

        public Token CaptionPos { get; set; }
        public FormatTag FormatType { get; set; } = FormatTag.TIME_HM;

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            return Format((TimeSpan)data);
        }

        //----------------------------------------------------------------------------
        public string Format(TimeSpan aValue) { return Format(aValue, true); }
        public string Format(TimeSpan aValue, bool padEnabled)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder work = new StringBuilder();
            NumberFormatInfo customFormat = new NumberFormatInfo();
            string stringFormat;
            bool appendFract = (FormatType == FormatTag.TIME_DHCH || FormatType == FormatTag.TIME_DHMCM ||
                                FormatType == FormatTag.TIME_DHMSF || FormatType == FormatTag.TIME_HMSF ||
                                FormatType == FormatTag.TIME_HMCM || FormatType == FormatTag.TIME_MSF);

            customFormat.NumberDecimalSeparator = (DecSeparator != null) ? DecSeparator : ".";
            if (TimeSeparator == null) TimeSeparator = ":";
            work.Append('0', (DecNumber != 0) ? DecNumber : 2);
            if (appendFract) stringFormat = "#." + work.ToString();
            else stringFormat = "0." + work.ToString();

            switch (FormatType)
            {
                case FormatTag.TIME_CH:
                    {
                        double cent = aValue.TotalSeconds / 100;
                        result.Append(cent.ToString(stringFormat, customFormat));
                        break;
                    }
                case FormatTag.TIME_D:
                    result.Append(Math.Floor(aValue.TotalDays));
                    break;
                case FormatTag.TIME_DCD:
                    result.Append(aValue.TotalDays.ToString(stringFormat, customFormat));
                    break;
                case FormatTag.TIME_H:
                    result.Append(Math.Floor(aValue.TotalHours));
                    break;
                case FormatTag.TIME_HCH:
                    result.Append(aValue.TotalHours.ToString(stringFormat, customFormat));
                    break;
                case FormatTag.TIME_M:
                    result.Append(Math.Floor(aValue.TotalMinutes));
                    break;
                case FormatTag.TIME_MCM:
                    result.Append(aValue.TotalMinutes.ToString(stringFormat, customFormat));
                    break;
                case FormatTag.TIME_S:
                    result.Append(Math.Floor(aValue.TotalSeconds));
                    break;
                case FormatTag.TIME_SF:
                    result.Append(aValue.TotalSeconds.ToString(stringFormat, customFormat));
                    break;
                case FormatTag.TIME_DH:
                case FormatTag.TIME_DHCH:
                    result.Append(aValue.Days + TimeSeparator + aValue.Hours.ToString("00"));
                    if (appendFract)
                    {
                        double fract = (aValue.TotalHours) - ((aValue.Days * 24) + aValue.Hours);
                        result.Append(fract.ToString(stringFormat, customFormat));
                    }
                    break;
                case FormatTag.TIME_DHM:
                case FormatTag.TIME_DHMCM:
                    result.Append(aValue.Days + TimeSeparator + aValue.Hours.ToString("00") +
                                                TimeSeparator + aValue.Minutes.ToString("00"));
                    if (appendFract)
                    {
                        double fract = ((aValue.TotalMinutes) -
                                        ((((aValue.Days * 24) + aValue.Hours) * 60) +
                                        aValue.Minutes));
                        result.Append(fract.ToString(stringFormat, customFormat));
                    }
                    break;
                case FormatTag.TIME_DHMS:
                case FormatTag.TIME_DHMSF:
                    result.Append(aValue.Days + TimeSeparator + aValue.Hours.ToString("00") +
                                                TimeSeparator + aValue.Minutes.ToString("00") +
                                                TimeSeparator + aValue.Seconds.ToString("00"));
                    if (appendFract)
                    {
                        double fract = (double)aValue.Milliseconds / 1000;
                        result.Append(fract.ToString(stringFormat, customFormat));
                    }
                    break;
                case FormatTag.TIME_HM:
                case FormatTag.TIME_HMCM:
                    {
                        int hour = (aValue.Days * 24) + aValue.Hours;
                        result.Append(hour.ToString() + TimeSeparator + aValue.Minutes.ToString("00"));
                        if (appendFract)
                        {
                            double fract = (aValue.TotalMinutes) -
                                            ((((aValue.Days * 24) + aValue.Hours) * 60) +
                                            aValue.Minutes);
                            result.Append(fract.ToString(stringFormat, customFormat));
                        }
                    }
                    break;
                case FormatTag.TIME_HMS:
                case FormatTag.TIME_HMSF:
                    {
                        int hour = (aValue.Days * 24) + aValue.Hours;
                        result.Append(hour.ToString("00") + TimeSeparator + aValue.Minutes.ToString("00") +
                                                            TimeSeparator + aValue.Seconds.ToString("00"));
                        if (appendFract)
                        {
                            double fract = (double)aValue.Milliseconds / 1000;
                            result.Append(fract.ToString(stringFormat, customFormat));
                        }
                        break;
                    }
                case FormatTag.TIME_MS:
                case FormatTag.TIME_MSF:
                    {
                        int min = (((aValue.Days * 24) + aValue.Hours) * 60) + aValue.Minutes;
                        result.Append(min.ToString("00") + TimeSeparator + aValue.Seconds.ToString("00"));
                        if (appendFract)
                        {
                            double fract = (double)aValue.Milliseconds / 1000;
                            result.Append(fract.ToString(stringFormat, customFormat));
                        }
                        break;
                    }
            }
            if (!padEnabled)
                return result.ToString();
            result.Append(Tail);
            result.Insert(0, Head);
            return Padder(result.ToString(), Align != AlignType.RIGHT);
        }

        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                TimeSpan converted = TimeSpan.Parse(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                //precision is now constant, but should be updatable; the problem, at present, 
                //is due to the fact that this information is set by an ERP procedure in an ERP table
                TimeSpan converted = TimeSpan.FromSeconds(double.Parse(data) / Precision);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimePrompt(Parser lex)
        {
            if (!lex.ParseTag(Token.PROMPT))
                return false;

            if (lex.Matched(Token.RIGHT))
                CaptionPos = Token.RIGHT;
            else
                if (!lex.ParseTag(Token.LEFT))
                return false;
            else
                CaptionPos = Token.LEFT;

            return true;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeFmt(Parser lex)
        {
            int i = 0;
            bool ok =
                lex.ParseTag(Token.STYLE) &&
                lex.ParseInt(out i);

            FormatType = (FormatTag)i;
            return ok;
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeSep(Parser lex)
        {
            if (lex.LookAhead() != Token.SEPARATOR) return true;

            return
                lex.ParseTag(Token.SEPARATOR) &&
                lex.ParseString(out TimeSeparator);
        }

        //----------------------------------------------------------------------------
        bool ParseFmtTimeDecimal(Parser lex)
        {
            if ((FormatType & FormatTag.TIME_DEC) != 0)
            {
                if (lex.LookAhead() != Token.PRECISION) return true;

                if (!lex.ParseTag(Token.PRECISION))
                    return false;

                if (lex.LookAhead(Token.STR))
                {
                    if (lex.ParseString(out DecSeparator))
                        isDecSeparatorDefault = false;
                    else
                        return false;
                }

                if (TimeSeparator == DecSeparator)
                {
                    lex.SetError(ApplicationsStrings.BadSeparator);
                    return false;
                }
                if (lex.NextTokenIsInt && !lex.ParseInt(out DecNumber))
                    return false;

                if ((DecNumber < 1) || (DecNumber > DBL_DIG))
                {
                    lex.SetError(ApplicationsStrings.BadPrecision);
                    return false;
                }
            }

            return true;
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;

            do switch (lex.LookAhead())
                {
                    case Token.PROMPT: ok = ParseFmtTimePrompt(lex); break;
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.SEPARATOR: ok = ParseFmtTimeSep(lex); break;
                    case Token.STYLE:
                        ok = ParseFmtTimeFmt(lex) &&
                                ParseFmtTimeSep(lex) &&
                                ParseFmtTimeDecimal(lex);
                        break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override void SetToLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (DecSeparator == "," && isDecSeparatorDefault)
                DecSeparator = FormatStyles.ApplicationLocale.NumberFormat.NumberDecimalSeparator;
        }

        //------------------------------------------------------------------------------
        public override void RestoreFromLocale()
        {
            if (FormatStyles.ApplicationLocale == null)
                return;

            if (isDecSeparatorDefault)
                DecSeparator = ",";
        }
    }

    /// <summary>
    /// Summary description for BoolFormatter.
    /// </summary>
    /// ================================================================================
    public class BoolFormatter : Formatter
    {
        public enum FormatTag { AS_ZERO, AS_CHAR }

        private string falseTag = string.Empty;
        private string trueTag = string.Empty;

        public string FalseTag { get { return falseTag == string.Empty ? ApplicationsStrings.FalseTag : falseTag; } }
        public string TrueTag { get { return trueTag == string.Empty ? ApplicationsStrings.TrueTag : trueTag; } }

        public FormatTag FormatType { get; set; } = FormatTag.AS_ZERO;

        //----------------------------------------------------------------------------
        public override string Format(object data)
        {
            return Format((bool)data);
        }

        //----------------------------------------------------------------------------
        public string Format(bool aValue) { return Format(aValue, true); }
        public string Format(bool aValue, bool padEnabled)
        {
            StringBuilder result = new StringBuilder();
            result.Append(Head);
            if (aValue) result.Append(TrueTag);
            else result.Append(FalseTag);
            if (!padEnabled)
                return result.ToString();
            result.Append(Tail);
            result.Insert(0, Head);
            return Padder(result.ToString(), Align != AlignType.RIGHT);
        }

        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                bool converted = bool.Parse(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                bool converted = SoapTypes.FromSoapBoolean(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //----------------------------------------------------------------------------
        bool ParseFmtBoolString(Parser lex)
        {
            return
                lex.ParseTag(Token.LOGIC) &&
                lex.ParseString(out trueTag) &&
                lex.ParseString(out falseTag);
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            // inital value
            FormatType = FormatTag.AS_ZERO;

            bool ok = true;
            do switch (lex.LookAhead())
                {
                    case Token.BITMAP: FormatType = FormatTag.AS_CHAR; lex.SkipToken(); break;
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.LOGIC: ok = ParseFmtBoolString(lex); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }
    }

    /// <summary>
    /// Summary description for EnumFormatter.
    /// </summary>
    /// ================================================================================
    public class EnumFormatter : StringFormatter
    {
        // tabella degli enumerativi corrente
        private Enums enums;

        [Flags]
        public new enum FormatTag
        {
            ASIS = 0x0000,
            UPPERCASE = 0x0001,
            LOWERCASE = 0x0002,
            CAPITALIZED = 0x0003,
            EXPANDED = 0x0004,
            FIRSTLETTER = 0X0005
        }

        internal new FormatTag FormatType = FormatTag.ASIS;
        //----------------------------------------------------------------------------
        public EnumFormatter(Enums enums) : base()
        {
            this.enums = enums;
        }

        //restituisce solamente l'item formattato sui tipi forniti
        //----------------------------------------------------------------------------
        public string Format(DataEnum aValue) { return Format(aValue, true); }
        public string Format(DataEnum aValue, bool padEnabled)
        {
            string resultValue = enums.LocalizedItemName(aValue);

            if (FormatType == FormatTag.FIRSTLETTER)
            {
                resultValue += aValue.ToString().Substring(0, 1);
                resultValue += Tail;
                resultValue.Insert(0, Head);
                return padEnabled ? Padder(resultValue.ToString(), Align != AlignType.RIGHT) : resultValue;
            }

            return base.Format(resultValue, padEnabled).ToString();
        }

        // se contiene un numero valido allora lo si considera il valore interno del DataEnum
        // altrimenti lo considero già in formato stringa "{T:I}"
        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            try
            {
                uint i = uint.Parse(data);
                return Format(new DataEnum(i));
            }
            catch
            {
                return data;
            }
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                DataEnum converted = SoapTypes.FromSoapDataEnum(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }

        //----------------------------------------------------------------------------
        bool ParseFmtDataStyle(Parser lex)
        {
            if (!lex.ParseTag(Token.STYLE))
                return false;

            int i = 0;
            if (!lex.ParseInt(out i))
                return false;

            FormatType = (FormatTag)i;
            return true;
        }

        //------------------------------------------------------------------------------
        override protected bool ParseFmtVariable(Parser lex)
        {
            bool ok = true;

            do switch (lex.LookAhead())
                {
                    case Token.ALIGN: ok = ParseFmtAlign(lex); break;
                    case Token.STYLE: ok = ParseFmtDataStyle(lex); break;

                    case Token.EOF: lex.SetError(ApplicationsStrings.UnexpectedEOF); return false;
                    case Token.END: lex.SetError(ApplicationsStrings.UnexpectedEnd); return false;
                    default: return ok;
                }
            while (ok);
            return ok;
        }
    }

    /// <summary>
    /// Summary description for GuidFormatter.
    /// </summary>
    /// ================================================================================
    public class GuidFormatter : StringFormatter
    {

        //----------------------------------------------------------------------------
        public string Format(Guid aValue) { return Format(aValue, true); }
        public string Format(Guid aValue, bool padEnabled)
        {
            //Non ho i tag originali, probabilmente nel vecchio formattatore
            //i guid non erano formattati in maniera particolare:
            //erano solo trasformati in stringa, allineati e concatenati con head e tail.
            //Se eventualmente si volessero aggiungere nuovi tipi di formattazione 
            //si deve passare al metodo ToString() 
            //la lettera relativa al tipo di formattazione voluto:
            //B:	{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
            //N:	xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //P:	(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
            //D:	xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
            //il metodo ToString trasforma tutte le lettere in minuscole
            //comunque il guid è case insensitive

            return base.Format(aValue.ToString("b"), padEnabled).ToString();
        }

        //------------------------------------------------------------------------------
        public override string Format(string data)
        {
            return data;
        }

        //------------------------------------------------------------------------------
        public override string FormatFromSoapData(string data)
        {
            try
            {
                Guid converted = SoapTypes.FromSoapGuid(data);
                return Format(converted);
            }
            catch
            {
                return data;
            }
        }
    }

    /// <summary>
    /// Summary description for FormatStyles.
    /// </summary>
    //================================================================================
    public class FormatStyles : Dictionary<string, FormatStylesGroup>
    {
        private const int RELEASE = 2;
        private ApplicationFormatStyles applicationFormatStyles;

        private Formatter.FormatSource currentSource;
        private INameSpace currentOwner;

        static private CultureInfo applicationLocale = null;

        //------------------------------------------------------------------------------
        public TbSession Session { get { return applicationFormatStyles.ReportSession; } }

        //------------------------------------------------------------------------------
        static public CultureInfo ApplicationLocale { get { return applicationLocale; } }

        //------------------------------------------------------------------------------
        public FormatStyles(ApplicationFormatStyles applicationFormatStyles, string currentUserPreferredLanguage, string serverConnectionPreferredLanguage)
            :
            base(StringComparer.OrdinalIgnoreCase)
        {
            this.applicationFormatStyles = applicationFormatStyles;

            if (currentUserPreferredLanguage != null && currentUserPreferredLanguage != string.Empty)
                CNumberToLiteralManager.Culture = currentUserPreferredLanguage;

            if (serverConnectionPreferredLanguage != null && serverConnectionPreferredLanguage != string.Empty)
                CNumberToLiteralManager.Culture = serverConnectionPreferredLanguage;

            applicationLocale = CultureInfo.CurrentCulture; //CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);  TODO RSWEB
        }

        //------------------------------------------------------------------------------
        public FormatStyles(ApplicationFormatStyles applicationFormatStyles)
            :
            base(StringComparer.OrdinalIgnoreCase)
        {
            this.applicationFormatStyles = applicationFormatStyles;

            CurrentCulture currentCulture = new CurrentCulture(applicationFormatStyles.ReportSession.UserInfo); //Setto le due variabili

            CNumberToLiteralManager.Culture = currentCulture.GetCulture();


            applicationLocale = CultureInfo.CurrentCulture; //CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);  TODO RSWEB
        }

        //------------------------------------------------------------------------------
        ~FormatStyles()
        {
            foreach (KeyValuePair<string, CNumberToLiteralLookUpTableManager> NTLManager in CNumberToLiteralManager.numberToLiteral)
                NTLManager.Value.Clear();
        }

        //------------------------------------------------------------------------------

        /// Attenzione: a differenza del C++ non può tornare formattatori che sono
        /// programmativi (owner di tipo library). Qualora li trovasse istanzia
        /// quello di default. Si può eventualmente in un secondo tempo implementare 
        /// la chiamata ad una FormatData(name, data) via WebService C#->C++
        //------------------------------------------------------------------------------
        public Formatter GetFormatter(string name, INameSpace context)
        {
            FormatStylesGroup pGroup = null;
            this.TryGetValue(name, out pGroup);

            // se non lo trovo nella tabella locale lo cerco in quella generale
            if (pGroup == null && applicationFormatStyles != null && applicationFormatStyles.Fs != this)
            {
                applicationFormatStyles.Fs.TryGetValue(name, out pGroup);
            }
            if (pGroup == null)
                return null;

            Formatter formatter = pGroup.GetFormatter(context);

            // ritorna il formattatore sostitutivo di quello programmativo scegliendolo in
            // base al tipo che avrebbe quello programmativo.
            if (formatter != null && formatter.Owner.NameSpaceType.Type == NameSpaceObjectType.Library)
                formatter = GetFormatter(GetDefaultNameByTypeToken(formatter.DataType), context);

            return formatter;
        }

        //------------------------------------------------------------------------------
        public string GetDefaultNameByTypeToken(Token dataType)
        {
            switch (dataType)
            {
                case Token.LONG_STRING: return "TestoEsteso";
                case Token.INTEGER: return "Intero";
                case Token.LONG_INTEGER: return "Esteso";
                case Token.ELAPSED_TIME: return "TempoTrascorso";
                case Token.SINGLE_PRECISION: return "Reale";
                case Token.DOUBLE_PRECISION: return "Reale";
                case Token.MONEY: return "Monetario";
                case Token.QUANTITY: return "Quantita";
                case Token.PERCENT: return "Percentuale";
                case Token.DATE: return "Data";
                case Token.DATETIME: return "DataOra";
                case Token.TIME: return "Ora";
                case Token.BOOLEAN: return "Logico";
                case Token.ENUM: return "Enumerativo";
                case Token.UUID: return "Uuid";
            }

            return DefaultFont.Testo;
        }

        //------------------------------------------------------------------------------
        private bool AddFormatter(Formatter formatter)
        {
            if (formatter == null)
                return false;

            // se non esiste lo creo nuovo
            FormatStylesGroup group = null;
            if (!ContainsKey(formatter.StyleName))
            {
                group = new FormatStylesGroup(formatter.StyleName);
                Add(formatter.StyleName, group);
            }

            group = (FormatStylesGroup)this[formatter.StyleName];

            if (group == null)
                return false;

            return group.AddFormatter(formatter) >= 0;
        }

        //------------------------------------------------------------------------------
        protected bool ParseStyle(Parser lex)
        {
            bool ok = true;
            Formatter fm = null;
            Token token = lex.LookAhead();
            switch (token)
            {
                case Token.STRING: fm = new StringFormatter(); ok = fm.Parse(lex); break;
                case Token.LONG_STRING: fm = new TextFormatter(); ok = fm.Parse(lex); break;
                case Token.INTEGER: fm = new IntFormatter(); ok = fm.Parse(lex); break;
                case Token.LONG_INTEGER: fm = new LongFormatter(); ok = fm.Parse(lex); break;
                case Token.ELAPSED_TIME: fm = new ElapsedTimeFormatter(); ok = fm.Parse(lex); break;
                case Token.SINGLE_PRECISION: fm = new DoubleFormatter(); ok = fm.Parse(lex); break;
                case Token.DOUBLE_PRECISION: fm = new DoubleFormatter(); ok = fm.Parse(lex); break;
                case Token.MONEY: fm = new MoneyFormatter(); ok = fm.Parse(lex); break;
                case Token.QUANTITY: fm = new QuantityFormatter(); ok = fm.Parse(lex); break;
                case Token.PERCENT: fm = new PercentFormatter(); ok = fm.Parse(lex); break;
                case Token.DATE: fm = new DateFormatter(); ok = fm.Parse(lex); break;
                case Token.DATETIME: fm = new DateTimeFormatter(); ok = fm.Parse(lex); break;
                case Token.TIME: fm = new TimeFormatter(); ok = fm.Parse(lex); break;
                case Token.BOOLEAN: fm = new BoolFormatter(); ok = fm.Parse(lex); break;
                case Token.ENUM: fm = new EnumFormatter(Session.Enums); ok = fm.Parse(lex); break;
                case Token.UUID: fm = new GuidFormatter(); ok = fm.Parse(lex); break;
                //				case Token.BLOB				:	fm = new BlobFormatter();				ok = fm.Parse(lex); break;

                default: lex.SetError(ApplicationsStrings.SyntaxError); ok = false; break;
            }

            fm.DataType = token;
            fm.Source = this.currentSource;
            // devo stare attenta a non sovrascrivermi quelli di libreria
            if (fm.Owner == null && this.currentOwner != null)
                fm.Owner = new NameSpace(this.currentOwner.FullNameSpace);

            if (FormatStyles.ApplicationLocale != null)
                fm.SetToLocale();

            if (ok)
                AddFormatter(fm);

            return ok;
        }

        //------------------------------------------------------------------------------
        bool ParseBlock(Parser lex)
        {
            if (lex.LookAhead(Token.BEGIN))
                return
                 lex.ParseBegin() &&
                 ParseStyles(lex) &&
                 lex.ParseEnd();

            return ParseStyle(lex);
        }

        //------------------------------------------------------------------------------
        bool ParseStyles(Parser lex)
        {
            //se la tabella è vuota non segnalo errore ma dò semplicemente un ASSERT
            if (lex.LookAhead(Token.END))
            {
                Debug.Fail("Empty Formatters table");
                return true;
            }

            bool ok = true;
            do { ok = ParseStyle(lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        // Se parsa stili di formattazione di applicazione richiede la release altrimenti
        // (per Woorm) non bisogna parsare la release ma solo la tabella di stili
        // Il terzo parametro indica (se a true) che si sta effettuando il parsing di un file 
        // di personalizzazione degli stili
        //------------------------------------------------------------------------------
        public bool Parse(Parser lex, bool parseRelease, INameSpace owner, Formatter.FormatSource source)
        {
            bool bOk = true;

            // mi evito di passarli di metodo in metodo
            currentSource = source;
            currentOwner = owner;


            // parsa la release solo per gli stili di applicazione
            if (parseRelease)
            {
                int release;
                if (!lex.ParseTag(Token.RELEASE) || !lex.ParseInt(out release))
                    return false;

                if (release != RELEASE)
                {
                    lex.SetError(ApplicationsStrings.BadFormatterRelease);
                    return false;
                }
            }

            if (lex.LookAhead(Token.FORMATSTYLES))
            {
                lex.SkipToken();
                return ParseBlock(lex);
            }
            return bOk;
        }

		//-----------------------------------------------------------------------------
		public bool Load(string filename, INameSpace owner, Formatter.FormatSource source)
		{
            if (!PathFinder.PathFinderInstance.ExistFile(filename))
                return true;
		
			Parser lex = new Parser(Parser.SourceType.FromFile);
			if (lex.Open(filename))
			{
				bool ok = Parse(lex, true, owner, source);
				lex.Close();
				return ok;
			}

            return false;
        }

        //------------------------------------------------------------------------------
        public string Format(string name, object data, INameSpace context)
        {
            Formatter formatter = GetFormatter(name, context);
            if (formatter == null) return data.ToString();

            return formatter.Format(data);
        }

        //------------------------------------------------------------------------------
        public string Format(string name, string data, INameSpace context)
        {
            Formatter formatter = GetFormatter(name, context);
            if (formatter == null) return data;

            return formatter.Format(data);
        }

        //------------------------------------------------------------------------------
        public string FormatFromSoapData(string name, string data, INameSpace context, CultureInfo collateCulture)
        {
            Formatter formatter = GetFormatter(name, context);
            if (formatter == null) return data;

            CultureInfo oldCulture = formatter.CollateCulture;
            try
            {
                formatter.CollateCulture = collateCulture;
                return formatter.FormatFromSoapData(data);
            }
            finally
            {
                formatter.CollateCulture = oldCulture;
            }
        }

        //------------------------------------------------------------------------------
        public string FormatFromSoapData(string name, object data, INameSpace context, CultureInfo collateCulture)
        {
            Formatter formatter = GetFormatter(name, context);
            if (formatter == null)
                return string.Empty;    //TODO format to soap data

            CultureInfo oldCulture = formatter.CollateCulture;
            try
            {
                formatter.CollateCulture = collateCulture;
                return formatter.Format(data);
            }
            finally
            {
                formatter.CollateCulture = oldCulture;
            }
        }

        //------------------------------------------------------------------------------
        public void SetToLocale()
        {
            applicationLocale = CultureInfo.CurrentCulture;  // (Thread.CurrentThread.CurrentCulture.Name);             TODO resweb

            foreach (FormatStylesGroup group in this.Values)
                foreach (Formatter formatter in group.FormatStyles)
                    formatter.SetToLocale();
        }

        //------------------------------------------------------------------------------
        public void RestoreFromLocale()
        {
            foreach (FormatStylesGroup group in this.Values)
                foreach (Formatter formatter in group.FormatStyles)
                    formatter.RestoreFromLocale();
        }

        //------------------------------------------------------------------------------
        public bool Unparse(Unparser unparser, INameSpace ns, Formatter.FormatSource source)
        {
            //TODOLUCA
            if (!ns.IsValid() /*|| !pTable*/)
                return true;

            //// prima aggancio i dati obbligatori
            //m_Namespace = aModule;
            this.currentSource = source;
            //m_pTable	= pTable.GetPointer();

            // se chiamata da wrmeng deve sempre salvare
            if (source != Formatter.FormatSource.WOORM /*&& !pTable->IsModified()*/)
                return true;

            bool notEmpty = IsUnparsingNeeded(ns);

            //se non c'è niente da unparsare, esco (occhio, in questo modo non passo neanche dal codice che
            //adesso è commentato
            if (!notEmpty)
                return true;

            // scrive  la release supportata
            if (source != Formatter.FormatSource.WOORM)
            {
                unparser.WriteTag(Token.RELEASE, false);
                unparser.Write(RELEASE);
            }

            unparser.WriteTag(Token.FORMATSTYLES, false);
            unparser.WriteBlank();

            unparser.WriteBegin();
            notEmpty = UnparseFormatsStyles(unparser, ns);
            unparser.WriteEnd();

            unparser.WriteLine();

            //TODOLUCA
            //// avviso che è stata cambiata la data del file
            //if (m_Source != Formatter::FROM_WOORM)
            //    pTable->AddFileLoaded(aModule, m_Source, ::GetFileDate(ofile.GetFilePath()));

            //if (notEmpty)
            //    return;

            //// se è vuoto viene eliminato
            //if (aSource != Formatter::FROM_WOORM)
            //{
            //    CString strPath = ofile.GetFilePath();
            //    ofile.Close();
            //    DeleteFile((LPCTSTR) strPath);
            //    pTable->RemoveFileLoaded(aModule, m_Source);
            //}

            return true;
        }

        //------------------------------------------------------------------------------
        public bool UnparseAll(Unparser unparser)
        {
            unparser.WriteTag(Token.RELEASE, false);
            unparser.Write(RELEASE);

            unparser.WriteTag(Token.FORMATSTYLES, false);
            unparser.WriteBlank();

            unparser.WriteBegin();

            foreach (FormatStylesGroup group in this.Values)
            {
                foreach (Formatter formatter in group.FormatStyles)
                    UnparseFormatStyle(unparser, formatter);
            }

            unparser.WriteEnd();
            unparser.WriteLine();

            return true;
        }


        //-----------------------------------------------------------------------------
        private bool IsUnparsingNeeded(INameSpace nameSpace)
        {
            foreach (FormatStylesGroup group in this.Values)
            {
                foreach (Formatter formatter in group.FormatStyles)
                {
                    NameSpace ns = string.Format
                        (
                        "{0}.{1}.{2}",
                        NameSpaceObjectType.Module,
                        formatter.Owner.Application,
                        formatter.Owner.Leaf
                        );

                    if (formatter.Owner.NameSpaceType.Type == NameSpaceObjectType.Report)
                        ns = formatter.Owner as NameSpace;

                    if (formatter.Source == currentSource && ns == nameSpace)
                        return true;
                }
            }

            return false;
        }

        //------------------------------------------------------------------------------
        private bool UnparseFormatsStyles(Unparser unparser, INameSpace nameSpace)
        {
            bool bExist = false;

            foreach (FormatStylesGroup group in this.Values)
            {
                foreach (Formatter formatter in group.FormatStyles)
                {
                    NameSpace ns = string.Format
                        (
                        "{0}.{1}.{2}",
                        NameSpaceObjectType.Module,
                        formatter.Owner.Application,
                        formatter.Owner.Leaf
                        );

                    if (formatter.Owner.NameSpaceType.Type == NameSpaceObjectType.Report)
                        ns = formatter.Owner as NameSpace;

                    if (formatter.Source == currentSource && ns == nameSpace)
                    {
                        UnparseFormatStyle(unparser, formatter);
                        bExist = true;
                    }
                }
            }

            return bExist;
        }

        //-----------------------------------------------------------------------------
        private void UnparseFormatStyle(Unparser unparser, Formatter formatter)
        {
            unparser.WriteTag(formatter.DataType, false);
            unparser.WriteString(formatter.StyleName, false);
            unparser.WriteBlank();

            UnparseFormatter(unparser, formatter);

            unparser.WriteSep(false);
            unparser.WriteLine();
        }

        //-----------------------------------------------------------------------------
        private void UnparseFormatter(Unparser unparser, Formatter formatter)
        {
            UnparseFmtCommon(unparser, formatter);
            UnparseFmtVariable(unparser, formatter);

            // è un formattatore programmativo derivante da libreria
            if (formatter.Owner.NameSpaceType.Type == NameSpaceObjectType.Library)
            {
                unparser.WriteTag(Token.FROM, false);
                unparser.WriteString(formatter.Owner.ToString(), false);
            }

            // area di applicazione
            if (!formatter.GetLimitedArea().IsNullOrEmpty())
                unparser.WriteString(formatter.GetLimitedArea(), false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtVariable(Unparser unparser, Formatter formatter)
        {
            if (
                    formatter.DataType == Token.INTEGER ||
                    formatter.DataType == Token.LONG
                )
            {
                UnparseLongData(unparser, formatter);
                return;
            }

            if (
                    formatter.DataType == Token.STRING ||
                    formatter.DataType == Token.UUID
                )
            {
                UnparseStringData(unparser, formatter);
                return;
            }

            if (
                    formatter.DataType == Token.DOUBLE ||
                    formatter.DataType == Token.MONEY ||
                    formatter.DataType == Token.QUANTITY ||
                    formatter.DataType == Token.PERCENT
                )
            {
                UnparseDoubleData(unparser, formatter);
                return;
            }

            if (
                    formatter.DataType == Token.DATE ||
                    formatter.DataType == Token.DATETIME ||
                    formatter.DataType == Token.TIME
                )
            {
                UnparseDateData(unparser, formatter);
                return;
            }

            if (formatter.DataType == Token.ELAPSED_TIME)
            {
                UnparseElapsedTimeData(unparser, formatter);
                return;
            }

            if (formatter.DataType == Token.BOOLEAN)
            {
                UnparseBoolData(unparser, formatter);
                return;
            }

            if (formatter.DataType == Token.ENUM)
            {
                UnparseEnumData(unparser, formatter);
                return;
            }
        }

        //-----------------------------------------------------------------------------
        private void UnparseEnumData(Unparser unparser, Formatter formatter)
        {
            EnumFormatter enumFmt = formatter as EnumFormatter;

            if (enumFmt.PaddedLen > 0)
                UnparseFmtAlign(unparser, enumFmt.GetDefaultAlign(), formatter);

            UnparseEnumDataStyle(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseEnumDataStyle(Unparser unparser, Formatter formatter)
        {
            EnumFormatter enumFmt = formatter as EnumFormatter;

            if (enumFmt.FormatType == EnumFormatter.FormatTag.ASIS)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.STYLE, false);
            unparser.Write((int)enumFmt.FormatType, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseBoolData(Unparser unparser, Formatter formatter)
        {
            BoolFormatter boolFmt = formatter as BoolFormatter;

            UnparseFmtAlign(unparser, boolFmt.GetDefaultAlign(), formatter);

            if (boolFmt.FormatType == BoolFormatter.FormatTag.AS_CHAR)
                unparser.WriteTag(Token.BITMAP, false);

            UnparseFmtBoolString(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtBoolString(Unparser unparser, Formatter formatter)
        {
            BoolFormatter boolFmt = formatter as BoolFormatter;

            if (
                    boolFmt.TrueTag == ApplicationsStrings.TrueTag &&
                    boolFmt.FalseTag == ApplicationsStrings.FalseTag
                )
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.LOGIC, false);
            unparser.WriteString(boolFmt.TrueTag, false);
            unparser.WriteString(boolFmt.FalseTag, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseElapsedTimeData(Unparser unparser, Formatter formatter)
        {
            ElapsedTimeFormatter timeFmt = formatter as ElapsedTimeFormatter;

            // must Unparsed in variable part because align default change
            // from type to type. Parse must be done in common part
            if (timeFmt.PaddedLen > 0)
                UnparseFmtAlign(unparser, timeFmt.GetDefaultAlign(), formatter);

            UnparseElapsedTimeFmt(unparser, formatter);
            UnparseFmtTimePrompt(unparser, formatter);
            UnparseFmtTimeSep(unparser, formatter);
            UnparseFmtTimeDecimal(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtTimeDecimal(Unparser unparser, Formatter formatter)
        {
            ElapsedTimeFormatter timeFmt = formatter as ElapsedTimeFormatter;
            string defDecSeparator = ",";
            int defDecimals = 2;

            if ((timeFmt.FormatType & ElapsedTimeFormatter.FormatTag.TIME_DEC) == 0)
                return;

            if (timeFmt.DecSeparator == defDecSeparator && timeFmt.DecNumber == defDecimals)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.PRECISION, false);

            if (timeFmt.DecSeparator != defDecSeparator)
                unparser.WriteString(timeFmt.DecSeparator, false);

            if (timeFmt.DecNumber != defDecimals)
                unparser.Write(timeFmt.DecNumber, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtTimePrompt(Unparser unparser, Formatter formatter)
        {
            ElapsedTimeFormatter timeFmt = formatter as ElapsedTimeFormatter;

            //Non mi è molto chiaro
            if (timeFmt.CaptionPos == 0)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.PROMPT, false);
            unparser.WriteBlank();
            unparser.WriteTag(timeFmt.CaptionPos, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseElapsedTimeFmt(Unparser unparser, Formatter formatter)
        {
            ElapsedTimeFormatter timeFmt = formatter as ElapsedTimeFormatter;

            if (timeFmt.FormatType == ElapsedTimeFormatter.FormatTag.TIME_HM)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.STYLE, false);
            unparser.Write((int)timeFmt.FormatType, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseDateData(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt != null)
            {
                if (dateFmt.PaddedLen > 0)
                    UnparseFmtAlign(unparser, dateFmt.GetDefaultAlign(), formatter);

                UnparseFmtOrder(unparser, formatter);
                UnparseFmtWeekDay(unparser, formatter);
                UnparseFmtDayFmt(unparser, formatter);
                UnparseFmtMonthFmt(unparser, formatter);
                UnparseFmtYearFmt(unparser, formatter);
                UnparseFmtFirstSep(unparser, formatter);
                UnparseFmtSecondSep(unparser, formatter);
                UnparseDateTimeFmt(unparser, formatter);
                return;
            }

        }

        //-----------------------------------------------------------------------------
        private void UnparseDateTimeFmt(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt == null)
                return;

            unparser.WriteBlank();

            if (dateFmt.TimeFormat != FormatStyleLocale.TimeFormat)
            {
                if (dateFmt.DataType == Token.TIME)
                    unparser.WriteTag(Token.TIME, false);

                unparser.WriteTag(Token.STYLE, false);
                unparser.Write((int)dateFmt.TimeFormat, false);
            }

            UnparseFmtTimeSep(unparser, formatter);
            UnparseFmtTimeAMPM(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtTimeAMPM(Unparser unparser, Formatter formatter)
        {

            //string timeSeparator =  formatter is DateFormatter
            //	? ((DateFormatter) formatter).TimeSeparator
            //	: ((ElapsedTimeFormatter) formatter).TimeSeparator;

            //if (timeSeparator.CompareNoCase(FormatStyles.ApplicationLocale.DateTimeFormat.TimeSeparator))           TODO resweb
            //	return;
            string timeSeparator = ".";

            DateTimeFormatter dtfm = formatter as DateTimeFormatter;
            if (dtfm != null)
            {
                timeSeparator = dtfm.TimeSeparator;

            }


            unparser.WriteBlank();
            unparser.WriteTag(Token.SEPARATOR, false);
            unparser.WriteString(timeSeparator, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtTimeSep(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt != null)
            {

                //  if (!dateFmt.IsTimeAMPMFormat()) return;

                unparser.WriteBlank();
                unparser.WriteTag(Token.POSTFIX, false);
                unparser.WriteString(dateFmt.TimeAM, false);
                unparser.WriteString(dateFmt.TimePM, false);
                return;
            }



        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtSecondSep(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.SecondSeparator.CompareNoCase(FormatStyleLocale.DateSeparator))
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.AFTER, false);
            unparser.WriteTag(Token.MONTH, false);
            unparser.WriteString(dateFmt.SecondSeparator, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtFirstSep(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.FirstSeparator.CompareNoCase(FormatStyleLocale.DateSeparator))
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.BEFORE, false);
            unparser.WriteTag(Token.MONTH, false);
            unparser.WriteString(dateFmt.FirstSeparator, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtYearFmt(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.YearFormat == FormatStyleLocale.ShortDateYearFormat)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.YEAR, false);
            unparser.Write((int)dateFmt.YearFormat, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtMonthFmt(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.MonthFormat == FormatStyleLocale.ShortDateMonthFormat)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.MONTH, false);
            unparser.Write((int)dateFmt.MonthFormat, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtDayFmt(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.DayFormat == FormatStyleLocale.ShortDateDayFormat)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.DAY, false);
            unparser.Write((int)dateFmt.DayFormat, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtWeekDay(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.WeekdayFormat == DateTimeFormatter.WeekdayTag.NOWEEKDAY)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.WEEKDAY, false);
            unparser.Write((int)dateFmt.WeekdayFormat, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtOrder(Unparser unparser, Formatter formatter)
        {
            DateTimeFormatter dateFmt = formatter as DateTimeFormatter;

            if (dateFmt.FormatType == FormatStyleLocale.ShortDateFormat)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.ORDER, false);
            unparser.Write((int)dateFmt.FormatType, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseDoubleData(Unparser unparser, Formatter formatter)
        {
            DoubleFormatter dblFmt = formatter as DoubleFormatter;

            if (dblFmt.PaddedLen > 0)
                UnparseFmtAlign(unparser, dblFmt.GetDefaultAlign(), formatter);

            UnparseDoubleDataStyle(unparser, formatter);
            UnparseFmtRound(unparser, formatter);
            UnparseFmtSign(unparser, formatter);
            UnparseFmtTable(unparser, formatter);
            UnparseFmtThousand(unparser, formatter);
            UnparseDoubleFmtSep(unparser, formatter);
            UnparseFmtDecimal(unparser, formatter);

            if (!dblFmt.ShowMSZero)
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.HIDE_MS0, false);
            }

            if (!dblFmt.ShowLSZero)
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.HIDE_LS0, false);
            }
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtDecimal(Unparser unparser, Formatter formatter)
        {
            DoubleFormatter dblFmt = formatter as DoubleFormatter;

            unparser.WriteBlank();
            unparser.WriteTag(Token.PRECISION, false);
            unparser.Write(dblFmt.DecNumber, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseDoubleFmtSep(Unparser unparser, Formatter formatter)
        {
            DoubleFormatter dblFmt = formatter as DoubleFormatter;
            string decSeparator = dblFmt.DecSeparator;
            string localeSep = FormatStyles.ApplicationLocale.NumberFormat.NumberDecimalSeparator;

            if (decSeparator.IsNullOrEmpty() || decSeparator.CompareNoCase(localeSep))
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.SEPARATOR, false);
            unparser.WriteString(decSeparator, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtRound(Unparser unparser, Formatter formatter)
        {
            DoubleFormatter dblFmt = formatter as DoubleFormatter;

            if (dblFmt.Rounding == DoubleFormatter.RoundingTag.ROUND_NONE)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.ROUND, false);
            unparser.Write((int)dblFmt.Rounding, false);
            unparser.WriteBlank();
            unparser.Write((double)dblFmt.Quantum, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseDoubleDataStyle(Unparser unparser, Formatter formatter)
        {
            DoubleFormatter dblFmt = formatter as DoubleFormatter;

            if (dblFmt.FormatType == DoubleFormatter.FormatTag.FIXED)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.STYLE, false);
            unparser.Write((int)dblFmt.FormatType, false);

            if (
                    dblFmt.FormatType != DoubleFormatter.FormatTag.ZERO_AS_DASH ||
                    dblFmt.AsZeroValue == "--"
                )
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.NULL, false);
            unparser.WriteString(dblFmt.AsZeroValue, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseStringData(Unparser unparser, Formatter formatter)
        {
            StringFormatter stringFmt = formatter as StringFormatter;

            if (stringFmt.PaddedLen > 0)
                UnparseFmtAlign(unparser, stringFmt.GetDefaultAlign(), formatter);

            UnparseStringDataStyle(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseStringDataStyle(Unparser unparser, Formatter formatter)
        {
            StringFormatter stringFmt = formatter as StringFormatter;

            if (stringFmt.FormatType == StringFormatter.FormatTag.ASIS)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.STYLE, false);
            unparser.Write((int)stringFmt.FormatType, false);

            if (stringFmt.FormatType == StringFormatter.FormatTag.EXPANDED)
            {
                unparser.WriteBlank();
                unparser.WriteString(stringFmt.InterChars, false);
            }
            else if (stringFmt.FormatType == StringFormatter.FormatTag.MASKED)
            {
                unparser.WriteBlank();
                unparser.WriteString(stringFmt.Mask, false);
                if (formatter.ZeroPadded)
                {
                    unparser.WriteBlank();
                    unparser.WriteTag(Token.PADDED, false);
                }
            }
        }

        //-----------------------------------------------------------------------------
        private void UnparseLongData(Unparser unparser, Formatter formatter)
        {
            LongFormatter pLongFmt = formatter as LongFormatter;

            // must Unparsed in variable part because align default change
            // from type to type. Parse must be done in common part
            if (pLongFmt.PaddedLen > 0)
                UnparseFmtAlign(unparser, pLongFmt.GetDefaultAlign(), formatter);

            if (pLongFmt.ZeroPadded)
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.PADDED, false);
            }

            UnparseFmtSign(unparser, formatter);
            UnparseLongDataStyle(unparser, formatter);
            UnparseFmtThousand(unparser, formatter);
            UnparseFmtTable(unparser, formatter);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtTable(Unparser unparser, Formatter formatter)
        {
            string table =
                formatter is LongFormatter
                ? (formatter as LongFormatter).XTable
                : (formatter as DoubleFormatter).XTable;

            if (table.IsNullOrEmpty())
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.TABLE, false);
            unparser.WriteString(table, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtThousand(Unparser unparser, Formatter formatter)
        {
            string sDefaultSeparator = formatter is LongFormatter
                ? FormatStyleLocale.ThousandLongSeparator
                : FormatStyleLocale.ThousandDoubleSeparator;

            string sSeparator =
                    formatter is LongFormatter
                    ? (formatter as LongFormatter).ThousSeparator
                    : (formatter as DoubleFormatter).ThousSeparator;

            if (sSeparator == sDefaultSeparator)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.THOUSAND, false);
            unparser.WriteString(sSeparator, false);

        }

        //-----------------------------------------------------------------------------
        private void UnparseLongDataStyle(Unparser unparser, Formatter formatter)
        {
            LongFormatter longFmt = (LongFormatter)formatter;

            if (longFmt == null || longFmt.FormatType == LongFormatter.FormatTag.NUMERIC)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.STYLE, false);
            unparser.Write((int)longFmt.FormatType);

            if (
                    longFmt.FormatType != LongFormatter.FormatTag.ZERO_AS_DASH ||
                    longFmt.AsZeroValue == "--"
                )
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.NULL, false);
            unparser.WriteString(longFmt.AsZeroValue, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtSign(Unparser unparser, Formatter formatter)
        {
            int nSignValue = 0;

            if (formatter is LongFormatter)
                nSignValue = (int)((LongFormatter)formatter).Sign;

            if (formatter is DoubleFormatter)
                nSignValue = (int)((DoubleFormatter)formatter).Sign;

            unparser.WriteBlank();
            unparser.WriteTag(Token.SIGN, false);
            unparser.Write(nSignValue, false);
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtAlign(Unparser unparser, Formatter.AlignType alignType, Formatter formatter)
        {
            if (formatter.Align == alignType || formatter.Align == Formatter.AlignType.NONE)
                return;

            unparser.WriteBlank();
            unparser.WriteTag(Token.ALIGN, false);

            switch (formatter.Align)
            {
                case Formatter.AlignType.LEFT: unparser.WriteTag(Token.LEFT, false); break;
                case Formatter.AlignType.RIGHT: unparser.WriteTag(Token.RIGHT, false); break;
            }
        }

        //-----------------------------------------------------------------------------
        private void UnparseFmtCommon(Unparser unparser, Formatter formatter)
        {
            if (!formatter.Head.IsNullOrEmpty())
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.PREFIX, false);
                unparser.WriteString(formatter.Head, false);
            }

            if (!formatter.Tail.IsNullOrEmpty())
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.POSTFIX, false);
                unparser.WriteString(formatter.Tail, false);
            }

            if (formatter.PaddedLen > 0)
            {
                unparser.WriteBlank();
                unparser.WriteTag(Token.LEN, false);
                unparser.Write(formatter.PaddedLen);
            }
        }
    }

	/// <summary>
	/// Summary description for ApplicationFormatStyles.
	/// </summary>
	/// ================================================================================
	public class ApplicationFormatStyles
	{
		private FormatStyles	fs;
		private bool			loaded = true;
		private TbSession			reportSession;
        private PathFinder pathFinder;
        //-----------------------------------------------------------------------------
        public FormatStyles	Fs		{ get { return fs; }}
		public bool			Loaded	{ get { return loaded; }}
		public TbSession		ReportSession	{ get { return reportSession; } set { reportSession = value; }}
        public PathFinder   PathFinder { get { return pathFinder; } set { pathFinder = value; } }

        //------------------------------------------------------------------------------
        public ApplicationFormatStyles(TbSession session)
        {
            // è necessario inizializzare prima una sessione di lavoro.
            this.reportSession = session;
            if (session == null)
                throw (new Exception("FormatStyles: sessione non inizializzata"));

            fs = new FormatStyles(this);
        }

        //------------------------------------------------------------------------------
        public ApplicationFormatStyles(string currentUserPreferredLanguage, string serverConnectionPreferredLanguage) //Qui passo le stringhe)
        {
            fs = new FormatStyles(this, currentUserPreferredLanguage, serverConnectionPreferredLanguage);
        }

        //------------------------------------------------------------------------------
        public Formatter GetFormatter(string name, INameSpace context)
        {
            return fs.GetFormatter(name, context);
        }

		//------------------------------------------------------------------------------
		public void Load()
		{
			if (ReportSession == null &&  PathFinder.PathFinderInstance == null)
				return;

            // considera che tutto sia ok. Se anche solo uno dei file non è caricato
            // o se la reportSession non è valorizzata allora considera il tutto non caricato.
            loaded = true;

			// carica tutti gli enums che fanno parte della applicazione (controllando che esista)
			foreach (ApplicationInfo ai in PathFinder.PathFinderInstance.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
				{
					NameSpace nsOwner = new NameSpace(ai.Name + NameSpace.TokenSeparator + mi.Name, NameSpaceObjectType.Module);

                    if (!fs.Load(mi.GetFormatsFullFilename(), nsOwner, Formatter.FormatSource.STANDARD))
                        loaded = false;

                    //TODO LARA E BRUNA EASYBILDER
                    //if (!fs.Load(mi.GetCustomFormatsFullFilename(PathFinder.PathFinderInstance.Company), nsOwner, Formatter.FormatSource.CUSTOM))
                    //    loaded = false;
                }
        }

        //------------------------------------------------------------------------------
        public void RestoreFromLocale()
        {
            fs.RestoreFromLocale();
        }

        //------------------------------------------------------------------------------
        public void SetToLocale()
        {
            fs.SetToLocale();
        }

        //------------------------------------------------------------------------------
        public string Format(string name, string data, INameSpace context)
        {
            return fs.Format(name, data, context);
        }

        //------------------------------------------------------------------------------
        public string Format(object data, INameSpace context)
        {
            if (data != null && data is DataArray)
            {
                DataArray ar = data as DataArray;
                if (ar.Count == 0)
                    return string.Empty;

                string all = string.Empty;
                foreach (object o in ar.Elements)
                {
                    all += Format(o, context) + '\n';
                }
                return all;
            }
            //----
            string name = ObjectHelper.DefaultFormatStyleName(data);
            string dataString = ObjectHelper.CastString(data);

            return fs.Format(name, dataString, context);
        }

        //-----------------------------------------------------------------------------
        public string GetJsonFormattersTable()
        {
            NameSpace ns = new NameSpace("");

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                JsonWriter jsonWriter = new JsonTextWriter(sw);

                try
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("formatters");

                    jsonWriter.WriteStartObject();

                    foreach (KeyValuePair<string, FormatStylesGroup> de in fs)
                    {
                        
                        FormatStylesGroup style = fs[de.Key];

                        jsonWriter.WritePropertyName(style.StyleName);

                        jsonWriter.WriteStartArray();
                        jsonWriter.WriteStartObject();

                        Formatter f = style.GetFormatter(ns);
                        PropertyInfo[] props = f.GetType().GetProperties();

                        foreach (PropertyInfo i in props)
                        {
                            jsonWriter.WritePropertyName(i.Name.ToString());
                            jsonWriter.WriteValue(i.GetValue(f, null).ToString());
                        }

                        jsonWriter.WriteEndObject();
                        jsonWriter.WriteEndArray();
                    }

                    jsonWriter.WriteEndObject();
                    jsonWriter.WriteEndObject();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }

                return sb.ToString();
            }
        }
    }

    /// ================================================================================
    public class CNumberToLiteralLookUpTableList : Dictionary<long, string>
    {
        //-----------------------------------------------------------------------------
        public string Get(long valore)
        {
            string retValue = string.Empty;
            TryGetValue(valore, out retValue);
            return retValue;
        }

        //-----------------------------------------------------------------------------
        public bool Exist(long valore)
        {
            return this.ContainsKey(valore);
        }
    }

    /// ================================================================================
    public class DeclinationException
    {
        public string m_Kind = string.Empty; //'S' = Start with
                                             //'E' = End with
                                             //'=' = Is equal (default)
        public int m_Value = 0;

        public DeclinationException(string kind, int val)
        {
            m_Kind = kind;
            m_Value = val;
        }

        public bool IsException(int val)
        {
            switch (m_Kind)
            {
                case "S":
                    if (val.ToString().StartsWith(m_Value.ToString()))
                        return true;
                    else
                        return false;
                case "E":
                    if (val.ToString().EndsWith(m_Value.ToString()))
                        return true;
                    else
                        return false;
                case "=":
                default:
                    if (val == m_Value)
                        return true;
                    else
                        return false;
            }
        }
    }

    /// ================================================================================
    public class Declination
    {
        public string m_Description = string.Empty;
        public List<DeclinationException> m_DeclinationExceptionList = new List<DeclinationException>();

        public Declination(string description)
        {
            m_Description = description;
        }

        public void AddDeclinationException(string kind, int val)
        {
            m_DeclinationExceptionList.Add(new DeclinationException(kind, val));
        }

        public bool IsException(int val)
        {
            foreach (DeclinationException de in m_DeclinationExceptionList)
            {
                if (de.IsException(val))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// ================================================================================
    public class CNumberGroup
    {
        public string m_Value = string.Empty;

        public Dictionary<int, Declination> m_DeclinationList = new Dictionary<int, Declination>();

        public CNumberGroup(string aValue)
        {
            m_Value = aValue;
        }

        public void AddDeclination(int aValue, string aDescription)
        {
            m_DeclinationList.Add(aValue, new Declination(aDescription));
        }

        public string GetDescription(int val, int lastDigit)
        {
            if (m_DeclinationList.TryGetValue(lastDigit, out Declination d) && !d.IsException(val))
                return d.m_Description;
            else
                return m_Value;
        }

        public void AddDeclinationException(int decValue, string kind, int val)
        {
            if (m_DeclinationList.TryGetValue(decValue, out Declination d))
            {
                d.AddDeclinationException(kind, val);
            }
        }
    }

    /// ================================================================================
    public class CNumberToLiteralLookUpTableManager
    {
        public enum DeclinationType { Hundreds, Thousands, Millions, Milliards };

        public CNumberToLiteralLookUpTableList m_LookUpList = new CNumberToLiteralLookUpTableList();
        public CNumberGroup m_Hundreds = null;
        public CNumberGroup m_Thousands = null;
        public CNumberGroup m_Millions = null;
        public CNumberGroup m_Milliards = null;
        public string m_Junction = string.Empty;
        public string m_Culture = string.Empty;
        public string m_Separator = string.Empty;
        public List<int> m_Exceptions = new List<int>();
        public bool bUnitInversion = false;

        //-----------------------------------------------------------------------------
        public string Convert(long num)
        {
            string result = string.Empty;

            if (m_LookUpList.Get(num) != null)
                return m_LookUpList.Get(num);

            char[] t = num.ToString().ToCharArray();
            long[] NiC = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int len = num.ToString().Length - 1;
            for (int n = 0; n <= len; n++)
                NiC[n + 11 - len] = long.Parse(t[n].ToString());

            for (long n = 0; n <= 11; n += 3)
            {
                long tripla = (NiC[n] * 100) + (NiC[n + 1] * 10) + NiC[n + 2];
                long doppia = (NiC[n + 1] * 10) + NiC[n + 2];

                if (tripla == 0)
                    continue;

                if (result != string.Empty)
                    result += AddSeparator(len + 1);

                if (tripla == 1)
                {
                    result += ConcatenaGruppoSingolare(n);
                    continue;
                }

                string tmpres = string.Empty;

                tmpres = m_LookUpList.Get(tripla);
                if (tmpres != null && tmpres != string.Empty)
                {
                    result += tmpres;
                    result += ConcatenaGruppo(n, (int)tripla, (int)NiC[n + 2]);
                    continue;
                }

                if (NiC[n] > 0)
                {
                    tmpres = m_LookUpList.Get(NiC[n] * 100);
                    if (tmpres == null && tmpres != string.Empty)
                    {
                        result += m_LookUpList.Get(NiC[n]);
                        result += m_Hundreds.GetDescription((int)tripla, (int)NiC[n + 2]);
                    }
                    else
                        result += tmpres;
                }

                if (doppia == 0)
                {
                    result += ConcatenaGruppo(n, (int)tripla, (int)NiC[n + 2]);
                    continue;
                }

                tmpres = m_LookUpList.Get(doppia);
                if (tmpres != null && tmpres != string.Empty)
                {
                    result += tmpres;
                    result += ConcatenaGruppo(n, (int)tripla, (int)NiC[n + 2]);
                    continue;
                }

                if (bUnitInversion)
                {
                    if (NiC[n + 2] > 0)
                        result += m_LookUpList.Get(NiC[n + 2]);

                    result += m_Junction;

                    if (NiC[n + 1] > 0)
                    {
                        result += m_LookUpList.Get(NiC[n + 1] * 10);
                    }
                }
                else
                {
                    if (NiC[n + 1] > 0)
                    {
                        result += m_LookUpList.Get(NiC[n + 1] * 10);
                    }

                    result += m_Junction;

                    if (NiC[n + 2] > 0)
                        result += m_LookUpList.Get(NiC[n + 2]);
                }
                result += ConcatenaGruppo(n, (int)tripla, (int)NiC[n + 2]);
            }

            return result;
        }

        //-----------------------------------------------------------------------------
        private string AddSeparator(int len)
        {
            foreach (int d in m_Exceptions)
                if (d == len)
                    return string.Empty;

            return m_Separator;
        }

        //-----------------------------------------------------------------------------
        private string ConcatenaGruppo(long i, int val, int lastDigit)
        {
            string res = string.Empty;
            switch (i)
            {
                case 9:
                    return string.Empty;
                case 6:
                    res = m_Thousands.GetDescription(val, lastDigit);
                    break;
                case 3:
                    res = m_Millions.GetDescription(val, lastDigit);
                    break;
                case 0:
                    res = m_Milliards.GetDescription(val, lastDigit);
                    break;
            }

            return res;
        }

        //-----------------------------------------------------------------------------
        private string ConcatenaGruppoSingolare(long i)
        {
            string res = string.Empty;
            switch (i)
            {
                case 9:
                    return m_LookUpList.Get(1);
                case 6:
                    res = m_LookUpList.Get(1000);
                    if (res == string.Empty)
                    {
                        res = m_LookUpList.Get(1);
                        res += m_Thousands.GetDescription(1, 1);
                    }
                    break;
                case 3:
                    if (res == string.Empty)
                    {
                        res = m_LookUpList.Get(1);
                        res += m_Millions.GetDescription(1, 1);
                    }
                    break;
                case 0:
                    res = m_LookUpList.Get(1000000000);
                    if (res == string.Empty)
                    {
                        res = m_LookUpList.Get(1);
                        res += m_Milliards.GetDescription(1, 1);
                    }
                    break;
            }

            return res;
        }

        //-----------------------------------------------------------------------------
        public void Clear()
        {
            m_Hundreds = null;
            m_Thousands = null;
            m_Millions = null;
            m_Milliards = null;
            m_Junction = string.Empty;
            m_Culture = string.Empty;
            m_Separator = string.Empty;
            bUnitInversion = false;

            m_LookUpList.Clear();
            m_Exceptions.Clear();
        }

        //-----------------------------------------------------------------------------
        public void Add(long key, string description)
        {
            m_LookUpList.Add(key, description);
        }

        //-----------------------------------------------------------------------------
        public void AddSeparatorException(int aValue)
        {
            m_Exceptions.Add(aValue);
        }

        //-----------------------------------------------------------------------------
        public void AddNumberGroup(DeclinationType eDecType, string aValue)
        {
            switch (eDecType)
            {
                case DeclinationType.Hundreds:
                    m_Hundreds = new CNumberGroup(aValue);
                    break;
                case DeclinationType.Thousands:
                    m_Thousands = new CNumberGroup(aValue);
                    break;
                case DeclinationType.Millions:
                    m_Millions = new CNumberGroup(aValue);
                    break;
                case DeclinationType.Milliards:
                    m_Milliards = new CNumberGroup(aValue);
                    break;
            }

        }

        //-----------------------------------------------------------------------------
        public void AddDeclination(DeclinationType eDecType, int aValue, string aDescription)
        {
            switch (eDecType)
            {
                case DeclinationType.Hundreds:
                    m_Hundreds.AddDeclination(aValue, aDescription);
                    break;
                case DeclinationType.Thousands:
                    m_Thousands.AddDeclination(aValue, aDescription);
                    break;
                case DeclinationType.Millions:
                    m_Millions.AddDeclination(aValue, aDescription);
                    break;
                case DeclinationType.Milliards:
                    m_Milliards.AddDeclination(aValue, aDescription);
                    break;
            }
        }

        //-----------------------------------------------------------------------------
        public void AddDeclinationException(DeclinationType eDecType, int aDecValue, string aKind, int aExcValue)
        {
            switch (eDecType)
            {
                case DeclinationType.Hundreds:
                    m_Hundreds.AddDeclinationException(aDecValue, aKind, aExcValue);
                    break;
                case DeclinationType.Thousands:
                    m_Thousands.AddDeclinationException(aDecValue, aKind, aExcValue);
                    break;
                case DeclinationType.Millions:
                    m_Millions.AddDeclinationException(aDecValue, aKind, aExcValue);
                    break;
                case DeclinationType.Milliards:
                    m_Milliards.AddDeclinationException(aDecValue, aKind, aExcValue);
                    break;
            }
        }

        //-----------------------------------------------------------------------------
        public string Get(long aValue) { return m_LookUpList.Get(aValue); }
    }

    //================================================================================
    public class FormatStyleLocale
    {
        //-----------------------------------------------------------------------------
        public static DateTimeFormatter.HourDigitTag TimeFormat
        {
            get
            {
                string sTimeFormat = FormatStyles.ApplicationLocale.DateTimeFormat.ShortTimePattern;

                bool bSeconds = (sTimeFormat.IndexOf("ss") >= 0) || (sTimeFormat.IndexOf("s") >= 0);
                int nHours = 0;
                bool b12Day = false;
                if (sTimeFormat.IndexOf("HH") == 0)
                    nHours = 2;
                else if (sTimeFormat.IndexOf("H") == 0)
                    nHours = 1;
                else if (sTimeFormat.IndexOf("hh") == 0)
                {
                    b12Day = true;
                    nHours = 2;
                }

                DateTimeFormatter.HourDigitTag tempFormat = DateTimeFormatter.HourDigitTag.TIME_NONE;

                tempFormat = (nHours > 1 ? DateTimeFormatter.HourDigitTag.TIME_HF99 : DateTimeFormatter.HourDigitTag.TIME_HF9);

                if (!bSeconds)
                    tempFormat = (tempFormat | DateTimeFormatter.HourDigitTag.TIME_NOSEC);

                if (b12Day)
                    tempFormat = (tempFormat | DateTimeFormatter.HourDigitTag.TIME_AMPM);

                return tempFormat;
            }
        }

        //-----------------------------------------------------------------------------
        public static DateTimeFormatter.YearDigitTag ShortDateYearFormat
        {
            get
            {
                string sYear = FormatStyles.ApplicationLocale.DateTimeFormat.YearMonthPattern;
                return sYear.IndexOf("yyyy") >= 0
                    ? DateTimeFormatter.YearDigitTag.YEAR9999
                    : DateTimeFormatter.YearDigitTag.YEAR99;
            }
        }

        //-----------------------------------------------------------------------------
        public static DateTimeFormatter.MonthDigitTag ShortDateMonthFormat
        {
            get
            {
                string sMonth = FormatStyles.ApplicationLocale.DateTimeFormat.ShortDatePattern;
                return sMonth.IndexOf("MM") >= 0
                    ? DateTimeFormatter.MonthDigitTag.MONTH99
                    : DateTimeFormatter.MonthDigitTag.MONTH9;
            }
        }

        //-----------------------------------------------------------------------------
        public static DateTimeFormatter.DayDigitTag ShortDateDayFormat
        {
            get
            {
                string sDay = FormatStyles.ApplicationLocale.DateTimeFormat.ShortDatePattern;
                return sDay.IndexOf("dd") >= 0
                    ? DateTimeFormatter.DayDigitTag.DAY99
                    : DateTimeFormatter.DayDigitTag.DAY9;
            }
        }

        //-----------------------------------------------------------------------------
        public static DateTimeFormatter.OrderTag ShortDateFormat
        {
            get
            {
                string sep = "-";// TODO rsweb FormatStyles.ApplicationLocale.DateTimeFormat.DateSeparator;
                string dateFormat = FormatStyles.ApplicationLocale.DateTimeFormat.ShortDatePattern;

                string[] tokens = dateFormat.Split(new string[] { sep }, StringSplitOptions.None);
                if (tokens.Length != 3)
                    return DateTimeFormatter.OrderTag.DATE_DMY;

                if (tokens[0].IndexOf("M") == 0 && tokens[1].IndexOf("d") == 0)
                    return DateTimeFormatter.OrderTag.DATE_MDY;
                if (tokens[0].IndexOf("d") == 0 && tokens[1].IndexOf("M") == 0)
                    return DateTimeFormatter.OrderTag.DATE_DMY;
                else
                    return DateTimeFormatter.OrderTag.DATE_YMD;
            }

        }

        //-----------------------------------------------------------------------------
        public static string ThousandLongSeparator
        {
            get
            {
                return FormatStyles.ApplicationLocale.NumberFormat.NumberGroupSeparator;
            }
        }

        //-----------------------------------------------------------------------------
        public static string ThousandDoubleSeparator
        {
            get
            {
                return FormatStyles.ApplicationLocale.NumberFormat.NumberGroupSeparator;
            }
        }

        //-----------------------------------------------------------------------------
        public static string DateSeparator
        {
            get
            {
                return ""; // FormatStyles.ApplicationLocale.DateTimeFormat.;           TODO rsweb
            }
        }
    }
}
