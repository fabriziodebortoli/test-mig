using System;
using System.Collections;
using System.Drawing;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.NameSolver;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.WoormWebControl;
using System.Net;

//using Microarea.RSWeb.Temp;

namespace Microarea.RSWeb.Objects
{

    /// <summary>
    /// Summary description for BaseObj.
    /// </summary>
    //================================================================================
    //   [Serializable]
    //[KnownType(typeof(Rectangle))]
    //[KnownType(typeof(BasicText))]
    //[KnownType(typeof(WoormValue))]
    //[KnownType(typeof(Label))]
    //   [KnownType(typeof(Borders))]
    //   [KnownType(typeof(BorderPen))]
    //   [KnownType(typeof(SqrRect))]
    //   [KnownType(typeof(BaseObj))]

    abstract public class BaseObj //: ISerializable
    {
        //[IgnoreDataMember]
        public WoormDocument Document;

        //const string BASERECT = "BaseRect";
        //const string INTERNALID = "InternalID";
        //const string HIDDEN = "IsHidden";

        public ushort InternalID = 0;

        public Rectangle Rect;
        public bool Transparent;
        public bool IsHidden = false;

        public int DropShadowHeight = 0;
        public Color DropShadowColor = Defaults.DefaultShadowColor;

        //[IgnoreDataMember]
        public WoormViewerExpression HideExpr = null;    // dynamic UI se valutata vera nasconde il campo
        //[IgnoreDataMember]
        public WoormViewerExpression TooltipExpr = null;  // dynamic UI (potrebbe utilizzare altri campi)

        //[IgnoreDataMember]
        public string ClassName = string.Empty; //Nome della classe dello stile
        //[IgnoreDataMember]
        public bool IsTemplate = false;         //Indica che gli attributi grafici di questo oggetto sono usati come template	

        //[IgnoreDataMember]
        public ushort AnchorRepeaterID = 0;
        //[IgnoreDataMember]
        public int RepeaterRow = -1;

        //[IgnoreDataMember]
        public bool InheritByTemplate { get; set; }
        //[IgnoreDataMember]
        public bool IsPersistent { get; set; }

        public virtual bool IsDynamic()
        {
            return HideExpr != null || TooltipExpr != null;
        }

        //------------------------------------------------------------------------------				
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //  info.AddValue(INTERNALID, InternalID);
        //	info.AddValue(HIDDEN, IsHidden);
        //	info.AddValue(BASERECT, Rect);
        //}

        //------------------------------------------------------------------------------
        virtual public string ToJsonTemplate(bool bracket)
        {
            string s = "\"baseobj\":{" +

                IdToJson() + ',' +

                IsHidden.ToJson("hidden") + ',' +
                Transparent.ToJson("transparent") + ',' +
                Rect.ToJson("rect") +

                (this.TooltipExpr != null ? ',' + DynamicTooltip.ToJson("tooltip", false, true) : "") +

                (/*DropShadowHeight != 0*/ true ?
                    ',' + DropShadowHeight.ToJson("shadow_height") +
                    ',' + DropShadowColor.ToJson("shadow_color")
                    : "") +
               '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        string RepeaterRowId()
        {
            if (this.AnchorRepeaterID == 0)
                return string.Empty;
            return '_' + this.RepeaterRow.ToString();

        }
        string IdToJson()
        {
            return InternalID.ToJson("id", "id", false, RepeaterRowId());
        }

        virtual public string ToJsonHiddenData(bool bracket)
        {
             string s = "\"baseobj\":{" +
                            IdToJson() + ',' +
                            false.ToJson("hidden") +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        virtual public string ToJsonData(bool bracket)
        {
            string s = "\"baseobj\":{" +

                IdToJson() +

                (this.HideExpr != null ? ',' + this.DynamicIsHidden.ToJson("hidden") : "") +
                (this.TooltipExpr != null ? ',' + this.DynamicTooltip.ToJson("tooltip", false, true) : "") +

              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        //      public BaseObj(SerializationInfo info, StreamingContext context)
        //{
        //	InternalID = info.GetUInt16(INTERNALID);
        //	Rect = info.GetValue<Rectangle>(BASERECT);
        //	IsHidden = info.GetBoolean(HIDDEN);
        //}

        //------------------------------------------------------------------------------	
        public virtual BaseObj Clone()
        {
            return null;
        }

        //------------------------------------------------------------------------------
        public BaseObj(WoormDocument document)
        {
            Document = document;
        }

        //------------------------------------------------------------------------------
        public static WoormViewerExpression CloneExpr(WoormViewerExpression e)
        {
            return e != null ? e.Clone() : null;
        }

        //------------------------------------------------------------------------------
        public BaseObj(BaseObj s)
        {
            this.Document = s.Document;
            this.InternalID = s.InternalID;
            this.Rect = s.Rect;
            this.Transparent = s.Transparent;

            this.IsHidden = s.IsHidden;
            this.HideExpr = CloneExpr(s.HideExpr);
            this.TooltipExpr = CloneExpr(s.TooltipExpr);

            this.DropShadowHeight = s.DropShadowHeight;
            this.DropShadowColor = s.DropShadowColor;

            this.ClassName = s.ClassName;
            this.IsTemplate = s.IsTemplate;

            this.AnchorRepeaterID = s.AnchorRepeaterID;
            this.RepeaterRow = s.RepeaterRow;
        }

        //------------------------------------------------------------------------------
        virtual public void ClearData() { }

        //------------------------------------------------------------------------------
        public virtual void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false) { }

        //------------------------------------------------------------------------------
        virtual protected bool ParseProp(WoormParser lex, bool block) { return true; }

        //------------------------------------------------------------------------------
        virtual protected bool ParseBlock(WoormParser lex)
        {
            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseProps(lex) &&
                    lex.ParseEnd();

            return ParseProp(lex, false);
        }

        //------------------------------------------------------------------------------
        virtual protected bool ParseProps(WoormParser lex)
        {
            bool ok = true;

            do { ok = ParseProp(lex, true) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        // solo il Localizer deve disabilitare il controllo di sintassi della espressione di Hide
        //------------------------------------------------------------------------------
        protected bool ParseHidden(WoormParser lex, Token[] stopTokens)
        {
            lex.SkipToken();
            IsHidden = true;
            if (lex.Matched(Token.WHEN))
            {
                HideExpr = new WoormViewerExpression(Document);
                HideExpr.StopTokens = new StopTokens(stopTokens);
                HideExpr.ForceSkipTypeChecking = Document.ForLocalizer;
                if (!HideExpr.Compile(lex, CheckResultType.Match, "Boolean"))
                {
                    lex.SetError(WoormViewerStrings.BadHiddenExpression);
                    return false;
                }

                // Nel caso di Localizer non posso valutare l'espresione perchè non ho la simbol table 
                // valorizzata dal run delle AskDialog di default rimane visibile e posso tradurre tutto
                if (Document.ForLocalizer)
                    IsHidden = false;
                else
                {
                    Value v = HideExpr.Eval();
                    if (HideExpr.Error)
                        IsHidden = false;
                    else
                        IsHidden = (bool)v.Data;
                }
            }

            lex.Matched(Token.SEP);
            return true;
        }

        //-------------------------------------------------------------------------------
        public bool CheckIsHidden
        {
            get
            {
                bool h = this.IsHidden;
                if (HideExpr != null)
                {
                    Value val = this.HideExpr.Eval();

                    if (val != null && val.Valid)
                        h = (bool)val.Data;
                }
                return h;
            }
        }

        public bool DynamicIsHidden
        {
            get
            {
                if (HideExpr != null && AnchorRepeaterID > 0)
                    Document.SynchronizeSymbolTable(RepeaterRow);

                bool h = CheckIsHidden;
                
                if (!h && AnchorRepeaterID > 0)
                {
                    Repeater rep = Document.Objects.FindBaseObj(AnchorRepeaterID) as Repeater;
                    if (rep != null)
                        h = rep.CheckIsHidden;
                }

                return h;
            }
        }

       //------------------------------------------------------------------------------
        protected bool ParseTooltip(WoormParser lex, Token[] stopTokens)
        {
            lex.SkipToken();
            lex.Matched(Token.ASSIGN);

            TooltipExpr = new WoormViewerExpression(Document);
            TooltipExpr.StopTokens = new StopTokens(stopTokens);
            TooltipExpr.ForceSkipTypeChecking = Document.ForLocalizer;
            if (!TooltipExpr.Compile(lex, CheckResultType.Match, "String"))
            {
                lex.SetError(WoormViewerStrings.BadTooltipExpression);
                return false;
            }

            lex.Matched(Token.SEP);
            return true;
        }
        //-------------------------------------------------------------------------------
        public string DynamicTooltip
        {
            get
            {
                if (TooltipExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = TooltipExpr.Eval();

                    if (val != null && val.Valid)
                        return (string)val.Data;
                }
                return string.Empty;
            }
        }

        //------------------------------------------------------------------------------
        protected bool ParseDropShadow(WoormParser lex, Token[] stopTokens)
        {
            lex.SkipToken();

            if (!lex.ParseInt(out DropShadowHeight))
                return false;

            if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                return false;

            return true;
        }

        //------------------------------------------------------------------------------
        public virtual bool Parse(WoormParser lex) { return true; }
        public virtual bool Unparse(Unparser unparser) { return true; }

        public virtual void SetStyle(BaseRect r) { }
        public virtual void ClearStyle() { }
        public virtual void RemoveStyle() { }

        //---------------------------------------------------------------------
        public enum LinkType { report, document, url, file, function }  //deve essere allineato con ...\web-form\src\app\reporting-studio\reporting-studio.component.ts

        static public string GetLink(bool template, WoormDocument woorm, int alias, int atRowNumber = -1)
        {
            if (woorm.Connections == null || woorm.Connections.Count == 0)
                return string.Empty;

            string navigateURL = string.Empty;
            string arguments = string.Empty;
            ConnectionLink conn = null;

            woorm.SynchronizeSymbolTable(atRowNumber);
            if (template)
            {
                conn = woorm.Connections.ExistsConnectionOnAlias(alias, woorm);
            }
            else
            {
                conn = woorm.Connections.GetConnectionOnAlias(alias, woorm, atRowNumber);
            }
            if (conn == null)
                return null;

            navigateURL = conn.Namespace;

            if (
                conn.ConnectionType == ConnectionLinkType.ReportByAlias ||
                conn.ConnectionType == ConnectionLinkType.FormByAlias ||
                conn.ConnectionType == ConnectionLinkType.URLByAlias ||
                conn.ConnectionType == ConnectionLinkType.FunctionByAlias
                )
            {
                string strVar = conn.Namespace;
                Variable v = woorm.RdeReader.SymbolTable.Find(strVar);
                if (v != null && v.Data != null)
                    navigateURL = v.Data.ToString();
                else
                    if (!template)
                    return null;
            }

            switch (conn.ConnectionType)
            {
                case ConnectionLinkType.Report:
                case ConnectionLinkType.ReportByAlias:
                    {
                        navigateURL = navigateURL.RemoveExtension(".wrm");
                        navigateURL = navigateURL.RemovePrefix("report.");

                        if (!template)
                        {
                            arguments = conn.GetArgumentsOuterXml(woorm, atRowNumber);
                            //arguments = WebUtility.UrlEncode(arguments);
                        }

                        string js = "\"link\":{" + navigateURL.ToJson("ns", false, true) + ',' +
                                                arguments.ToJson("arguments", false, true) + ',';

                        js += ((int)LinkType.report).ToJson("type") + '}';
                        return js;
                    }
                case ConnectionLinkType.Form:
                case ConnectionLinkType.FormByAlias:
                    {
                        if (!template)
                        {
                            arguments = conn.GetArgumentsOuterXml(woorm, atRowNumber);
                            //arguments = WebUtility.UrlEncode(arguments);
                        }

                        string js = "\"link\":{" + navigateURL.ToJson("ns", false, true) + ',' +
                                                arguments.ToJson("arguments", false, true) + ',';

                        js += ((int)LinkType.document).ToJson("type") + '}';
                        return js;
                    }

                case ConnectionLinkType.URL:
                case ConnectionLinkType.URLByAlias:
                    {
                        if (conn.ConnectionSubType == ConnectionLinkSubType.File)
                        {
                            break;
                        }
                        else if (conn.ConnectionSubType == ConnectionLinkSubType.MailTo)
                        {
                            break;
                        }
                        else if (conn.ConnectionSubType == ConnectionLinkSubType.CallTo)
                        {
                            break;
                        }
                        else if (conn.ConnectionSubType == ConnectionLinkSubType.Url)
                        {
                            navigateURL = conn.GetHttpGetRequest(navigateURL, atRowNumber);
                            navigateURL = navigateURL.AddPrefix("http://", "https://");
                            navigateURL = WebUtility.UrlEncode(navigateURL);

                            string js = "\"link\":{" + navigateURL.ToJson("ns", false, true) + ',';

                            js += ((int)LinkType.url).ToJson("type") + '}';
                            return js;
                        }
                        else if (conn.ConnectionSubType == ConnectionLinkSubType.GoogleMap)
                        {
                            navigateURL = conn.GetGoogleMapURL(navigateURL);
                            navigateURL = WebUtility.UrlEncode(navigateURL);

                            string js = "\"link\":{" + navigateURL.ToJson("ns", false, true) + ',';

                            js += ((int)LinkType.url).ToJson("type") + '}';
                            return js;
                        }
                        break;
                    }

                case ConnectionLinkType.Function:
                case ConnectionLinkType.FunctionByAlias:
                    {
                        break;
                    }

            }
            return null;
        }
    }

    /// <summary>
    /// Summary description for BaseRect.
    /// </summary>
    //================================================================================
    //[Serializable]
    //   [KnownType(typeof(Borders))]
    //   [KnownType(typeof(BorderPen))]
    //   [KnownType(typeof(SqrRect))]
    //   [KnownType(typeof(BaseObj))]
    public abstract class BaseRect : BaseObj
    {
        //const string BORDERS = "Borders";

        public Borders Borders = new Borders(true);
        public BorderPen BorderPen = new BorderPen();

        public int HRatio = 0;
        public int VRatio = 0;

        //[IgnoreDataMember]
        protected BaseRect Default = null;

        //[IgnoreDataMember]
        public WoormViewerExpression TextColorExpr = null; // dynamic UI
        //[IgnoreDataMember]
        public WoormViewerExpression BkgColorExpr = null; // dynamic UI

        //[IgnoreDataMember]
        public bool IsAnchorPageLeft = false;
        //[IgnoreDataMember]
        public bool IsAnchorPageRight = false;
        //[IgnoreDataMember]
        public ushort AnchorLeftColumnID = 0;
        //[IgnoreDataMember]
        public ushort AnchorRightColumnID = 0;

        //[IgnoreDataMember]
        public bool TemplateOverridden = false;

        //attributes not used in Easylook, used only for Z-print in woorm c++. Here they are only parsed
        //[IgnoreDataMember]
        protected int Layer = 0;   //only design mode

        public override bool IsDynamic()
        {
            return base.IsDynamic() || TextColorExpr != null || BkgColorExpr != null;
        }

        //------------------------------------------------------------------------------
        //      public BaseRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //	Borders = info.GetValue<Borders>(BORDERS);
        //}

        //------------------------------------------------------------------------------
        public BaseRect(WoormDocument document)
            : base(document)
        {
        }

        //------------------------------------------------------------------------------
        public BaseRect(BaseRect s)
            : base(s)
        {
            this.BorderPen = s.BorderPen;   //DEEP clone ?
            this.Borders = s.Borders;       //DEEP clone ?

            this.HRatio = s.HRatio;
            this.VRatio = s.VRatio;

            this.TextColorExpr = CloneExpr(s.TextColorExpr);
            this.BkgColorExpr = CloneExpr(s.BkgColorExpr);

            this.IsAnchorPageLeft = s.IsAnchorPageLeft;
            this.IsAnchorPageRight = s.IsAnchorPageRight;

            this.AnchorLeftColumnID = s.AnchorLeftColumnID;
            this.AnchorRightColumnID = s.AnchorRightColumnID;
        }

        //------------------------------------------------------------------------------
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);

        //    info.AddValue(BORDERS, Borders);
        //}

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string s = "\"baserect\":{" +

               base.ToJsonTemplate(false) + ',' +

                (/*this.HRatio != 0*/true ? this.HRatio.ToJson("ratio") + ',' : "") +
                // (/*this.VRatio != 0*/ true  ? this.VRatio.ToJson("vratio") + ',' : "") +

                this.Borders.ToJson() + ',' +
                this.BorderPen.ToJson() +
                '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonHiddenData(bool bracket)
        {
            string s = "\"baserect\":{" +
                           base.ToJsonHiddenData(false) +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string s = "\"baserect\":{" +
               base.ToJsonData(false) +
                  '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        public Rectangle InsideRect
        {
            get
            {
                if (Document.Options.ConNoBorders)
                    return Rect;

                return CalculateInsideRect(Rect, Borders, BorderPen);
            }
        }

        //-------------------------------------------------------------------------------
        internal void RenameAlias(int offset)
        {
            AnchorLeftColumnID += (ushort)offset;
            AnchorRightColumnID += (ushort)offset;
        }

        //-------------------------------------------------------------------------------
        public bool IsNotDefaultRatio()
        {
            if (Default != null)
                return HRatio != Default.HRatio || VRatio != Default.VRatio;

            return (HRatio != 0 || VRatio != 0);
        }

        //-------------------------------------------------------------------------------
        public bool IsNotDefaultBorderPen()
        {
            if (Default != null)
                return BorderPen != Default.BorderPen;

            return !BorderPen.IsDefault;
        }

        //-------------------------------------------------------------------------------
        public bool IsNotDefaultBorders()
        {
            if (Default != null)
                return Borders != Default.Borders;

            return !Borders.IsDefault();
        }

        //-------------------------------------------------------------------------------
        public bool IsNotDefaultTrasparent()
        {
            if (Default != null)
                return Transparent != Default.Transparent;

            return Transparent;
        }

        //-------------------------------------------------------------------------------
        public bool IsNotDefaultDropShadow()
        {
            if (Default != null)
                return DropShadowHeight != Default.DropShadowHeight ||
                       DropShadowColor != Default.DropShadowColor;

            return DropShadowHeight != 0;
        }

        //-------------------------------------------------------------------------------
        public static Rectangle CalculateInsideRect(Rectangle rect, Borders borders, BorderPen borderPen)
        {
            int pen = borderPen.Width;
            int dLeft = 0, dRight = 0, dTop = 0, dBottom = 0;

            if (borders.Left) dLeft += pen;
            if (borders.Top) dTop += pen;
            Size dLocation = new Size(dLeft, dTop);

            if (borders.Right) dRight -= pen;
            if (borders.Bottom) dBottom -= pen;
            Size dSize = new Size(dRight - dLeft, dBottom - dTop);

            return new Rectangle(rect.Location + dLocation, rect.Size + dSize);
        }

        //------------------------------------------------------------------------------
        void MoveBaseRect(int x1, int y1, int x2, int y2, bool bIgnoreBorder = false)
        {
            //Se il campo che sto ancorando ha il bordo sinistro adeguo la x in modo che il contenuto 
            //sia allineato alla x della colonna, e non li bordo
            //(Comportamento uniforme a quello del titolo della colonna)
            if (Borders.Left && !bIgnoreBorder)
                x1 -= BorderPen.Width;

            //BaseRect.SetRect( x1 , y1, x2, y2);	
            Rect = Rectangle.FromLTRB(x1, y1, x2, y2);
        }

        //------------------------------------------------------------------------------
        public override void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false)
        {
            MoveBaseRect(Rect.Left + xOffset, Rect.Top + yOffset, Rect.Right + xOffset, Rect.Bottom + yOffset, bIgnoreBorder);
        }

        //------------------------------------------------------------------------------
        public static void SubstituteTemplateFont(WoormDocument woorm, ref string fontToUpdate, string templateFont, string defaultFont)
        {
            if (string.IsNullOrWhiteSpace(fontToUpdate) || string.IsNullOrWhiteSpace(templateFont))
                return;

            if (fontToUpdate != templateFont)
                return;

            FontStylesGroup fontGroup = woorm.FontStyles[templateFont] as FontStylesGroup;
            if (fontGroup == null)
                return;

            for (int i = 0; i < fontGroup.FontStyles.Count; i++)
            {
                FontElement font = (FontElement)fontGroup.FontStyles[i];
                if (
                    font != null &&
                    font.Source == FontElement.FontSource.STANDARD &&
                    font.IsNoneFont()
                    )
                    continue;

                fontToUpdate = defaultFont;
            }
        }

        //------------------------------------------------------------------------------
        internal static void RemoveTemplateFont(WoormDocument woorm, ref string fontToUpdate, string def, string fnt)
        {
            SubstituteTemplateFont(woorm, ref fontToUpdate, def, fnt);
        }

        //------------------------------------------------------------------------------
        internal static void SetTemplateFont(WoormDocument woorm, ref string fontToUpdate, string def, string fnt)
        {
            SubstituteTemplateFont(woorm, ref fontToUpdate, fnt, def);
        }

        //------------------------------------------------------------------------------
        internal static void ClearTemplateFont(WoormDocument woorm, ref string fontToUpdate, string def, string fnt)
        {
            if (!def.IsNullOrEmpty())
            {

                FontStylesGroup fontGroup = woorm.FontStyles[def] as FontStylesGroup;
                if (fontGroup == null)
                    return;

                for (int i = 0; i < fontGroup.FontStyles.Count; i++)
                {
                    FontElement font = (FontElement)fontGroup.FontStyles[i];
                    if (font.Source != FontElement.FontSource.STANDARD)
                        continue;

                    if (font == null || !font.IsNoneFont())
                        continue;

                    fontToUpdate = def;
                    return;
                }
            }
            fontToUpdate = fnt;
        }

        //-------------------------------------------------------------------------------
        public bool ParseTextColor(WoormParser lex, out Color color)
        {
            bool ok = ParseDynamicColor(lex, Token.TEXTCOLOR, out color, ref TextColorExpr);
            return ok;
        }

        //------------------------------------------------------------------------------
        public bool ParseBkgColor(WoormParser lex, out Color color)
        {
            bool ok = ParseDynamicColor(lex, Token.BKGCOLOR, out color, ref BkgColorExpr);
            return ok;
        }

        //------------------------------------------------------------------------------
        public bool ParseDynamicColor(WoormParser lex, Token token, out Color aColor, ref WoormViewerExpression expr)
        {
            aColor = Color.FromArgb(255, 255, 255, 255);  //inizializzo con un valore che non sara utilizzato perche e' param out
            if (!lex.ParseTag(token))
                return false;

            if (lex.Matched(Token.ASSIGN))
            {
                expr = new WoormViewerExpression(Document);
                expr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                expr.ForceSkipTypeChecking = Document.ForLocalizer;
                if (!expr.Compile(lex, CheckResultType.Match, "Int32"))
                {
                    lex.SetError(WoormViewerStrings.BadColorExpression);
                    return false;
                }
                return lex.ParseSep();
            }

            return lex.ParseColor(Token.NULL, out aColor) && lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        public void UnparseProp(Unparser unparser)
        {
            unparser.IncTab();

            unparser.WriteBegin();
            unparser.IncTab();

            if (IsTemplate)
                unparser.WriteTag(Token.TEMPLATE);

            if (!ClassName.IsNullOrEmpty())
            {
                unparser.WriteTag(Token.STYLE, false);
                unparser.WriteString(ClassName);
            }

            if (IsNotDefaultTrasparent())
                unparser.WriteTag(Token.TRANSPARENT);

            if (IsNotDefaultBorderPen())
                unparser.WritePen(BorderPen);

            if (IsNotDefaultBorders())
                unparser.WriteBorders(Borders);

            if (IsNotDefaultDropShadow())
                UnparseDropShadow(unparser);

            UnparseTooltip(unparser);

            UnparseHidden(unparser);

            UnparsePrintInfo(unparser);

            UnparseAuxProp(unparser);

            unparser.DecTab();
            unparser.WriteEnd();

            unparser.DecTab();
            unparser.WriteLine();
        }

        //------------------------------------------------------------------------------
        public virtual void UnparseAuxProp(Unparser unparser) { }

        //------------------------------------------------------------------------------
        public void UnparseDynamicColor(Unparser unparser, Token tk, Expression expr)
        {
            if (expr == null || Document.Template.IsSavingTemplate)
                return;

            unparser.WriteTag(tk, false);
            unparser.WriteTag(Token.ASSIGN, false);
            unparser.WriteExpr(expr.ToString());
            unparser.WriteSep(true);
        }

        //------------------------------------------------------------------------------
        public void UnparsePrintInfo(Unparser unparser)
        {
            if (IsAnchorPageLeft)
                unparser.WriteTag(Token.ANCHOR_PAGE_LEFT, false);

            if (IsAnchorPageRight)
                unparser.WriteTag(Token.ANCHOR_PAGE_RIGHT, false);

            if (!Document.Template.IsSavingTemplate && AnchorLeftColumnID != 0)
            {
                unparser.WriteTag(Token.ANCHOR_COLUMN_ID, false);
                unparser.WriteAlias(AnchorLeftColumnID, false);

                if (AnchorRightColumnID != 0 && AnchorLeftColumnID != AnchorRightColumnID)
                {
                    unparser.WriteTag(Token.COMMA, false);
                    unparser.WriteAlias(AnchorRightColumnID, false);
                }
            }

            if (IsAnchorPageLeft || IsAnchorPageRight || AnchorLeftColumnID != 0)
                unparser.WriteLine();
        }

        //------------------------------------------------------------------------------
        private void UnparseHidden(Unparser unparser)
        {
            if ((IsHidden == true || HideExpr != null) && !Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.HIDDEN);
                unparser.WriteBlank();
                if (HideExpr != null)
                {
                    unparser.WriteTag(Token.WHEN, false);

                    if (Document.ReplaceHiddenWhenExpr) //TODOLUCA da implementare
                        unparser.WriteTag(IsHidden ? Token.TRUE : Token.FALSE);
                    else
                        unparser.WriteExpr(HideExpr.ToString());

                    unparser.WriteTag(Token.SEP);
                }
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseTooltip(Unparser unparser)
        {
            if (TooltipExpr == null || Document.Template.IsSavingTemplate)
                return;

            unparser.WriteTag(Token.TOOLTIP, false);
            unparser.WriteTag(Token.ASSIGN, false);
            unparser.WriteExpr(TooltipExpr.ToString());
            unparser.WriteSep(true);
        }

        //------------------------------------------------------------------------------
        private void UnparseDropShadow(Unparser unparser)
        {
            if (DropShadowHeight > 0)
            {
                unparser.WriteTag(Token.DROPSHADOW, false);
                unparser.Write(DropShadowHeight, false);
                unparser.WriteBlank();
                unparser.WriteColor(Token.COLOR, DropShadowColor);
            }
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        public override void SetStyle(BaseRect templateRect)
        {
            RemoveStyle();

            if (templateRect == null)
                return;

            Default = templateRect;

            if (Borders == new Borders())
                Borders = templateRect.Borders;

            if (BorderPen == new BorderPen())
                BorderPen = templateRect.BorderPen;

            if (!Transparent)
                Transparent = templateRect.Transparent;

            if (HRatio == 0 && VRatio == 0)
            {
                HRatio = templateRect.HRatio;
                VRatio = templateRect.VRatio;
            }

            //if (DropShadowHeight == 0 && DropShadowColor.Equals(/*Color.AliceBlue*/))                TODO rsweb
            //{
            //	DropShadowHeight = templateRect.DropShadowHeight;
            //	DropShadowColor = templateRect.DropShadowColor;
            //}
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            if (InheritByTemplate)
                return;

            Borders = Default != null ? Default.Borders : new Borders();
            BorderPen = Default != null ? Default.BorderPen : new BorderPen();

            Transparent = Default != null ? Default.Transparent : false;

            HRatio = Default != null ? Default.HRatio : 0;
            VRatio = Default != null ? Default.VRatio : 0;

            DropShadowHeight = Default != null ? Default.DropShadowHeight : 0;
            DropShadowColor = Default != null ? Default.DropShadowColor : Color.FromArgb(255, 255, 255, 255);
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            if (InheritByTemplate || Default == null)
                return;

            if (Borders == Default.Borders)
                Borders = new Borders();

            if (BorderPen == Default.BorderPen)
                BorderPen = new BorderPen();

            if (Transparent == Default.Transparent)
                Transparent = false;

            if (HRatio == Default.HRatio && VRatio == Default.VRatio)
            {
                HRatio = 0;
                VRatio = 0;
            }

            if (DropShadowHeight == Default.DropShadowHeight && DropShadowColor == Default.DropShadowColor)
            {
                DropShadowHeight = 0;
                DropShadowColor = Color.FromArgb(255, 255, 255, 255);
            }

            Default = null;
        }

        //------------------------------------------------------------------------------
        internal void MarkTemplateOverridden()
        {
            if (IsTemplate && Default != null)
                Default.TemplateOverridden = true;
        }

        //------------------------------------------------------------------------------
        internal bool DeleteEditorEntry()
        {
            FreeFieldFromColumn(Document.Objects);
            FreeFieldFromRepeater(Document.Objects);
            return true;
        }

        //------------------------------------------------------------------------------
        private void FreeFieldFromRepeater(Layout layout)
        {
            if (AnchorRepeaterID <= 0)
                return;

            BaseObj obj = layout.FindBaseObj(AnchorRepeaterID);
            if (
                    obj != null &&
                    AnchorRepeaterID == obj.InternalID &&
                    obj is Repeater
                )
            {
                Repeater rep = obj as Repeater;
                rep.Detach(this, true);
            }

            AnchorRepeaterID = 0;
        }

        //------------------------------------------------------------------------------
        internal void FreeFieldFromColumn(BaseObjList list)
        {
            if (AnchorLeftColumnID <= 0)
                return;

            foreach (BaseObj obj in list)
            {
                if (!(obj is Table))
                    continue;

                Table table = obj as Table;

                foreach (Column col in table.Columns)
                {
                    if (AnchorLeftColumnID == col.InternalID)
                    {
                        col.RemoveAnchoredField(this);
                        AnchorLeftColumnID = 0;

                        if (AnchorRightColumnID == 0)
                            goto l_end_FreeFieldFromColumn;
                    }
                    if (AnchorRightColumnID == col.InternalID)
                    {
                        col.RemoveAnchoredField(this);
                        AnchorRightColumnID = 0;
                        goto l_end_FreeFieldFromColumn;
                    }
                    if (AnchorLeftColumnID == 0) //colonne intermedie
                        col.RemoveAnchoredField(this);
                }
            }

            l_end_FreeFieldFromColumn:
            AnchorLeftColumnID = AnchorRightColumnID = 0;

            //TODOLUCA
            //UpdateDocument();
        }
    }

    /// <summary>
    /// Summary description for SqrRect.
    /// </summary>
    //================================================================================
    //[Serializable]
    //   [KnownType(typeof(BaseRect))]
    public class SqrRect : BaseRect
    {
        protected Color bkgColor = Defaults.DefaultBackColor;

        //public SqrRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //}
        public SqrRect()
            : this((WoormDocument)null)
        {

        }
        //------------------------------------------------------------------------------
        public SqrRect(WoormDocument document)
            : base(document)
        {
        }

        //------------------------------------------------------------------------------
        public SqrRect(SqrRect s)
            : base(s)
        {
            this.bkgColor = s.TemplateBkgColor;
        }

        //------------------------------------------------------------------------------
        public Color TemplateBkgColor
        {
            get
            {
                return bkgColor;
            }
        }

        public Color DynamicBkgColor
        {
            get
            {
                if (BkgColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = BkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateBkgColor;
            }
        }

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string s = "\"sqrrect\":{" +

                    base.ToJsonTemplate(false) + ',' +

                    this.DynamicBkgColor.ToJson("bkgcolor") +
                 '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonHiddenData(bool bracket)
        {
            string s = "\"sqrrect\":{" +
                           base.ToJsonHiddenData(false) +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string s = "\"sqrrect\":{" +

                   base.ToJsonData(false) +

                    (this.BkgColorExpr != null ? ',' + this.DynamicBkgColor.ToJson("bkgcolor") : "") +
                '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }


        //------------------------------------------------------------------------------
        public override BaseObj Clone()
        {
            return new SqrRect(this);
        }

        //------------------------------------------------------------------------------
        protected override bool ParseProp(WoormParser lex, bool block)
        {
            bool ok = true;

            switch (lex.LookAhead())
            {
                case Token.BKGCOLOR: ok = ParseBkgColor(lex, out bkgColor); break;
                case Token.PEN: ok = lex.ParsePen(BorderPen); break;
                case Token.BORDERS: ok = lex.ParseBorders(Borders); break;
                case Token.TRANSPARENT:
                    lex.SkipToken();
                    Transparent = true;
                    break;

                case Token.HIDDEN:
                    {
                        Token[] stopTokens =
                        {
                            Token.SEP, Token.END,
                            Token.BKGCOLOR, Token.PEN, Token.BORDERS, Token.TRANSPARENT,
                            Token.TOOLTIP, Token.STYLE, Token.TEMPLATE, Token.DROPSHADOW,
                            Token.ANCHOR_COLUMN_ID, Token.ANCHOR_PAGE_LEFT, Token.ANCHOR_PAGE_RIGHT,
                            Token.LAYER
                        };
                        ok = ParseHidden(lex, stopTokens);
                        break;
                    }

                case Token.ANCHOR_PAGE_LEFT:
                    {
                        lex.SkipToken();
                        IsAnchorPageLeft = true;
                        break;
                    }

                case Token.ANCHOR_PAGE_RIGHT:
                    {
                        lex.SkipToken();
                        IsAnchorPageRight = true;
                        break;
                    }

                case Token.ANCHOR_COLUMN_ID:
                    {
                        lex.SkipToken();
                        ok = lex.ParseAlias(out AnchorLeftColumnID);
                        if (!ok)
                            break;

                        if (lex.Matched(Token.COMMA))
                            ok = lex.ParseAlias(out AnchorRightColumnID);

                        break;
                    }

                case Token.TOOLTIP:
                    {
                        Token[] stopTokens =
                                    {
                                        Token.SEP
                                    };
                        ok = ParseTooltip(lex, stopTokens);
                        break;
                    }

                case Token.DROPSHADOW:
                    {
                        lex.SkipToken();
                        if (!lex.ParseInt(out DropShadowHeight))
                            return false;
                        if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                            return false;
                        break;
                    }

                case Token.STYLE:
                    {
                        lex.SkipToken();
                        if (!lex.ParseString(out ClassName))
                            return false;
                        break;
                    }

                case Token.TEMPLATE:
                    {
                        lex.SkipToken();
                        IsTemplate = true;
                        break;
                    }

                case Token.LAYER:
                    lex.SkipToken();
                    ok = lex.ParseInt(out this.Layer);
                    break;

                case Token.END:
                    return ok;

                default:
                    if (block)
                    {
                        lex.SetError(WoormViewerStrings.BadGeneralProperties);
                        ok = false;
                    }
                    break;
            }

            if (Transparent)
                bkgColor = Color.FromArgb(0, 0, 0, 0);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            Token token = lex.LookAhead(Token.RNDRECT) ?
                    Token.RNDRECT :
                    Token.SQRRECT;

            return
                lex.ParseRect(token, out Rect) &&
                lex.ParseRatio(out HRatio, out VRatio) &&
                ParseBlock(lex);
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            //---- Template Override  
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteRect(Token.SQRRECT, Rect, false);

            if (IsNotDefaultRatio())
                unparser.WriteRatio(HRatio, VRatio, false);

            UnparseProp(unparser);

            //----
            Default = tempDefault;
            return true;
        }

        //------------------------------------------------------------------------------
        public override void UnparseAuxProp(Unparser unparser)
        {
            if (BkgColorExpr != null)
                UnparseDynamicColor(unparser, Token.BKGCOLOR, BkgColorExpr);
            else if (IsNotDefaultBkgColor())
            {
                unparser.WriteColor(Token.BKGCOLOR, TemplateBkgColor, false);
                unparser.WriteSep(true);
            }
        }

        //------------------------------------------------------------------------------
        bool IsNotDefaultBkgColor()
        {
            if (Default != null)
                return TemplateBkgColor != ((SqrRect)Default).TemplateBkgColor;

            return TemplateBkgColor != Defaults.DefaultBackColor;
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        public override void SetStyle(BaseRect templateRect)
        {
            if (InheritByTemplate)
                return;

            RemoveStyle();
            if (templateRect == null)
                return;

            base.SetStyle(templateRect);

            if (TemplateBkgColor == Defaults.DefaultBackColor)
                bkgColor = ((SqrRect)Default).TemplateBkgColor;
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            if (InheritByTemplate)
                return;

            bkgColor = Default != null
                ? ((SqrRect)Default).TemplateBkgColor
                : Defaults.DefaultBackColor;

            base.ClearStyle();
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            if (InheritByTemplate || Default == null)
                return;

            if (TemplateBkgColor == ((SqrRect)Default).TemplateBkgColor)
                bkgColor = Defaults.DefaultBackColor;

            base.RemoveStyle();
        }
    }

    /// <summary>
    /// Summary description for TextRect.
    /// </summary>
    //================================================================================
    //[Serializable]
    //[KnownType(typeof(Color))]
    public class TextRect : BaseRect
    {
        public BasicText Label = null;

        public bool Special = false; // dynamic UI per alcuni tag
        public bool IsHtml = false;

        public BarCode BarCode = null;

        public bool IsBarCode
        {
            get
            {
                return BarCode != null;
            }
        }

        //const string LABEL = "Label";
        //const string LOCALIZEDTEXT = "LocalizedText";
        //const string TEXTCOLOR = "TextColor";
        //const string BACKCOLOR = "BackColor";

        //-------------------------------------------------------------------------------
        public Color TemplateTextColor
        {
            get
            {
                return Label.TextColor;
            }
        }

        public Color DynamicTextColor
        {
            get
            {
                if (TextColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = TextColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateTextColor;
            }
        }
        //-------------------------------------------------------------------------------
        public Color TemplateBkgColor
        {
            get
            {
                return Label.BkgColor;
            }
        }

        public Color DynamicBkgColor
        {
            get
            {
                if (BkgColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = BkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateBkgColor;
            }
        }

        //------------------------------------------------------------------------------
        public string LocalizedText
        {
            get
            {
                if (Special)
                {
                    SpecialField sf = new SpecialField(Document);
                    return Document.Localizer.Translate(sf.Expand(Label.Text));
                }
                return Document.Localizer.Translate(Label.Text);
            }
        }

        //------------------------------------------------------------------------------
        //public TextRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //		Label = info.GetValue<BasicText>(LABEL);
        //		LocalizedText = info.GetString(LOCALIZEDTEXT);
        //		//TODO SILVANO recuperare il colore
        //}

        //------------------------------------------------------------------------------
        public TextRect()
            : this((WoormDocument)null)
        {

        }

        //------------------------------------------------------------------------------
        public TextRect(WoormDocument document)
            : base(document)
        {
            Label = new BasicText(document);
        }

        //------------------------------------------------------------------------------
        public TextRect(TextRect s)
            : base(s)
        {
            this.Label = s.Label;   //DEEP CLONE ?

            this.Special = s.Special;

            this.InternalID = s.InternalID;
            this.BarCode = s.BarCode;
        }

        //------------------------------------------------------------------------------
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);

        //    info.AddValue(LABEL, Label);
        //    info.AddValue(LOCALIZEDTEXT, LocalizedText);
        //    //info.AddValue(TEXTCOLOR, ForeColorToSerialize);
        //    //info.AddValue(BACKCOLOR, BackColorToSerialize);
        //}

        //------------------------------------------------------------------------------
        public override bool IsDynamic()
        {
            return base.IsDynamic() || this.Special;
        }

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string s = "\"textrect\":{" +

                base.ToJsonTemplate(false) + ',' +

                this.LocalizedText.ToJson("value", false, true) + ',' +

                this.DynamicBkgColor.ToJson("bkgcolor") + ',' +
                this.DynamicTextColor.ToJson("textcolor") + ',' +

                this.Label.Align.ToHtml_align() + ',' +
                this.Label.FontData.ToJson() + ',' +

                this.IsHtml.ToJson("value_is_html") + ',' +
                this.IsBarCode.ToJson("value_is_barcode") +
                (this.IsBarCode?',' + this.BarCode.ToJson() : "") +
             '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonHiddenData(bool bracket)
        {
            string s = "\"textrect\":{" +
                           base.ToJsonHiddenData(false) +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string s = "\"textrect\":{" +

               base.ToJsonData(false) +

               (this.Special ? ',' + LocalizedText.ToJson("value", false, true) : "") +

               (this.TextColorExpr != null ? ',' + this.DynamicTextColor.ToJson("textcolor") : "") +
               (this.BkgColorExpr != null ? ',' + this.DynamicBkgColor.ToJson("bkgcolor") : "") +

             '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        public override BaseObj Clone()
        {
            return new TextRect(this);
        }

        //------------------------------------------------------------------------------
        internal bool ParseBarCode(WoormParser lex)
        {
            BarCode = new BarCode(Document.ReportSession.PathFinder);
            return lex.ParseBarCode(BarCode);
        }

        //------------------------------------------------------------------------------
        protected override bool ParseProp(WoormParser lex, bool block)
        {
            bool ok = true;

            switch (lex.LookAhead())
            {
                case Token.TRANSPARENT:
                    lex.SkipToken(); Transparent = true;
                    break;

                case Token.SPECIAL_FIELD:
                    lex.SkipToken(); Special = true;
                    break;

                case Token.HTML:
                    lex.SkipToken(); IsHtml = true;
                    break;

                case Token.LAYER:
                    lex.SkipToken();
                    ok = lex.ParseInt(out this.Layer);
                    break;

                case Token.TEXTCOLOR:
                    ok = ParseTextColor(lex, out Label.TextColor);
                    break;

                case Token.ALIGN:
                    ok = lex.ParseAlign(out Label.Align);
                    break;

                case Token.BKGCOLOR:
                    ok = ParseBkgColor(lex, out Label.BkgColor);
                    break;

                case Token.PEN: ok = lex.ParsePen(BorderPen); break;
                case Token.BORDERS: ok = lex.ParseBorders(Borders); break;
                case Token.FONTSTYLE:
                    ok = lex.ParseFont(out Label.FontStyleName);
                    break;

                case Token.HIDDEN:
                    {
                        Token[] stopTokens =
                        {
                            Token.SEP, Token.END,
                            Token.TEXTCOLOR, Token.ALIGN, Token.FONTSTYLE, Token.SPECIAL_FIELD,
                            Token.BKGCOLOR, Token.PEN, Token.BORDERS, Token.TRANSPARENT,
                            Token.TOOLTIP, Token.STYLE, Token.TEMPLATE, Token.DROPSHADOW,
                            Token.ANCHOR_COLUMN_ID, Token.ANCHOR_PAGE_LEFT, Token.ANCHOR_PAGE_RIGHT,
                            Token.BARCODE, Token.HTML, Token.LAYER
                        };
                        ok = ParseHidden(lex, stopTokens);
                        break;
                    }

                case Token.ANCHOR_PAGE_LEFT:
                    {
                        lex.SkipToken();
                        IsAnchorPageLeft = true;
                        break;
                    }

                case Token.ANCHOR_PAGE_RIGHT:
                    {
                        lex.SkipToken();
                        IsAnchorPageRight = true;
                        break;
                    }

                case Token.ANCHOR_COLUMN_ID:
                    {
                        lex.SkipToken();
                        ok = lex.ParseAlias(out AnchorLeftColumnID);
                        if (!ok)
                            break;

                        if (lex.Matched(Token.COMMA))
                            ok = lex.ParseAlias(out AnchorRightColumnID);

                        break;
                    }

                case Token.TOOLTIP:
                    {
                        Token[] stopTokens =
                        {
                            Token.SEP
                        };
                        ok = ParseTooltip(lex, stopTokens);
                        break;
                    }

                case Token.DROPSHADOW:
                    {
                        lex.SkipToken();
                        if (!lex.ParseInt(out DropShadowHeight))
                            return false;
                        if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                            return false;
                        break;
                    }

                case Token.STYLE:
                    {
                        lex.SkipToken();
                        if (!lex.ParseString(out ClassName))
                            return false;
                        break;
                    }

                case Token.TEMPLATE:
                    {
                        lex.SkipToken();
                        IsTemplate = true;
                        break;
                    }
                case Token.BARCODE:
                    ok = ParseBarCode(lex); break;

                case Token.END: return ok;
                default:
                    if (block)
                    {
                        lex.SetError(WoormViewerStrings.BadTextRectProperty);
                        ok = false;
                    }
                    break;
            }
            if (Transparent)
                Label.BkgColor = Color.FromArgb(0, 255, 255, 255);

            return ok;
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            string text = "";

            bool ok =
                lex.ParseRect(Token.TEXT, out Rect) &&
                lex.ParseRatio(out HRatio, out VRatio);

            if (ok && lex.LookAhead(Token.ALIAS))
                ok = lex.ParseAlias(out InternalID);

            ok = ok &&
                lex.ParseCEdit(out text) &&
                ParseBlock(lex);

            Label.Text = text;
            return ok;
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            //---- Template Override  
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteRect(Token.TEXT, this.Rect, false);

            if (IsNotDefaultRatio())
                unparser.WriteRatio(HRatio, VRatio, false);

            if (InternalID != 0)
                unparser.WriteAlias(InternalID, false);

            unparser.WriteCEdit(
                                    unparser.IsLocalizableTextInCurrentLanguage()
                                        ? unparser.LoadReportString(LocalizedText)
                                        : this.Label.Text,
                                    true
                                );

            UnparseProp(unparser);

            Default = tempDefault;
            return true;
        }

        //------------------------------------------------------------------------------
        public override void UnparseAuxProp(Unparser unparser)
        {
            if (BkgColorExpr != null)
                UnparseDynamicColor(unparser, Token.BKGCOLOR, BkgColorExpr);
            else if (IsNotDefaultBkgColor())
            {
                unparser.WriteColor(Token.BKGCOLOR, DynamicBkgColor, false);
                unparser.WriteSep(true);
            }

            if (TextColorExpr != null)
                UnparseDynamicColor(unparser, Token.TEXTCOLOR, TextColorExpr);
            else if (IsNotDefaultTextColor())
            {
                unparser.WriteColor(Token.TEXTCOLOR, DynamicTextColor, false);
                unparser.WriteSep(true);
            }

            if (Special)
                unparser.WriteTag(Token.SPECIAL_FIELD);

            if (BarCode != null)
                BarCode.Unparse(unparser, true);

            if (IsNotDefaultAlignment())
                unparser.WriteAlign(Label.Align);

            if (IsNotDefaultFontStyle())
                unparser.WriteFont(Label.FontStyleName);

            if (IsHtml)
                unparser.WriteTag(Token.HTML);
        }

        //------------------------------------------------------------------------------
        public bool IsNotDefaultBkgColor()
        {
            if (Default != null)
                return Label.BkgColor != ((TextRect)Default).Label.BkgColor;

            return Label.BkgColor != Defaults.DefaultBackColor;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultFontStyle()
        {
            if (Default != null)
                return Label.FontStyleName != ((TextRect)Default).Label.FontStyleName;

            return Label.FontStyleName != DefaultFont.Testo;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultAlignment()
        {
            if (Default != null)
                return Label.Align != ((TextRect)Default).Label.Align;

            return Label.Align != Defaults.DefaultTextAlign;
        }


        //------------------------------------------------------------------------------
        public bool IsNotDefaultTextColor()
        {
            if (Default != null)
                return Label.TextColor != ((TextRect)Default).Label.TextColor;

            return Label.TextColor != Defaults.DefaultTextColor;
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        public override void SetStyle(BaseRect templateRect)
        {
            RemoveStyle();

            if (templateRect == null)
                return;

            base.SetStyle(templateRect);

            if (Label.BkgColor == Defaults.DefaultBackColor)
                Label.BkgColor = ((TextRect)Default).Label.BkgColor;

            if (Label.TextColor == Defaults.DefaultTextColor)
                Label.TextColor = ((TextRect)Default).Label.TextColor;

            if (Label.Align == Defaults.DefaultTextAlign)
                Label.Align = ((TextRect)Default).Label.Align;

            if (Label.FontStyleName == DefaultFont.Testo)
                Label.FontStyleName = ((TextRect)Default).Label.FontStyleName;
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            if (InheritByTemplate)
                return;

            Label.BkgColor = Default != null ? ((TextRect)Default).Label.BkgColor : Defaults.DefaultBackColor;
            Label.TextColor = Default != null ? ((TextRect)Default).Label.TextColor : Defaults.DefaultTextColor;
            Label.Align = Default != null ? ((TextRect)Default).Label.Align : Defaults.DefaultTextAlign;

            ClearTemplateFont(Document, ref Label.FontStyleName, Default != null ? ((TextRect)Default).Label.FontStyleName : null, DefaultFont.Testo);

            base.ClearStyle();
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            if (InheritByTemplate || Default == null)
                return;

            if (Label.BkgColor == ((TextRect)Default).Label.BkgColor)
                Label.BkgColor = Defaults.DefaultBackColor;

            if (Label.TextColor == ((TextRect)Default).Label.TextColor)
                Label.TextColor = Defaults.DefaultTextColor;

            if (Label.Align == ((TextRect)Default).Label.Align)
                Label.Align = Defaults.DefaultTextAlign;

            BaseRect.RemoveTemplateFont(Document, ref Label.FontStyleName, ((TextRect)Default).Label.FontStyleName, DefaultFont.Testo);

            base.RemoveStyle();
        }
    }

    /// <summary>
    /// Summary description for GraphRect.
    /// </summary>
    //================================================================================
    //[Serializable]
    public class GraphRect : SqrRect, IImage
    {
        public string ImageFileName;
        public bool IsCut = false;
        public bool ShowProportional = false;
        public bool ShowNativeImageSize = false;
        public Rectangle RectCutted;
        public AlignType Align = AlignType.DT_CENTER | AlignType.DT_VCENTER;

        //const string ISCUT = "IsCut";
        //const string ALIGN = "Align";

        //------------------------------------------------------------------------------
        //      public GraphRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //	IsCut = info.GetBoolean(ISCUT);
        //}
        //------------------------------------------------------------------------------
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //	base.GetObjectData(info, context);
        //	info.AddValue(ISCUT, IsCut);
        //}

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string s = "\"graphrect\":{" +

                base.ToJsonTemplate(false) + ',' +

                this.ImageFileName.ToJson("image", false, true) + ',' +
                this.Align.ToHtml_align() +

              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonHiddenData(bool bracket)
        {
            string s = "\"graphrect\":{" +
                           base.ToJsonHiddenData(false) +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string s = "\"graphrect\":{" +
                 base.ToJsonData(false) +
              '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        public GraphRect()
            : this((WoormDocument)null)
        {

        }
        //------------------------------------------------------------------------------
        public GraphRect(WoormDocument document)
            : base(document)
        {
        }

        //------------------------------------------------------------------------------
        public GraphRect(GraphRect s)
            : base(s)
        {
            this.ImageFileName = s.ImageFileName;
            this.ShowProportional = s.ShowProportional;
            this.ShowNativeImageSize = s.ShowNativeImageSize;
            this.IsCut = s.IsCut;
            this.RectCutted = s.RectCutted;
        }

        //------------------------------------------------------------------------------
        public override BaseObj Clone()
        {
            return new GraphRect(this);
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            ShowProportional = false;
            ShowNativeImageSize = false;
            IsCut = false;

            bool ok = lex.LookAhead(Token.METAFILE) ? lex.ParseRect(Token.METAFILE, out Rect) : lex.ParseRect(Token.BITMAP, out Rect);
            if (!ok)
                return false;

            if (lex.Matched(Token.PROPORTIONAL))
            {
                ShowProportional = true;
            }
            else if (lex.Matched(Token.NATIVE))
            {
                ShowNativeImageSize = true;
            }

            if (lex.LookAhead(Token.RECT))
            {
                ok = lex.ParseRect(out RectCutted);
                if (ok)
                    IsCut = true;
            }

            // Potrebbe avere sintassi di file o sintassi di namespace
            ok = ok && lex.ParseString(out ImageFileName);

            ok = ok && ParseBlock(lex);
            return ok;
        }

        protected override bool ParseProp(WoormParser lex, bool block)
        {
            if (lex.LookAhead(Token.ALIGN))
            {
                if (!lex.ParseAlign(out Align))
                    return false;
                return true;
            }
            return base.ParseProp(lex, block);
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            //---- Template Override  
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteRect(Token.BITMAP, Rect, false);

            if (ShowProportional)
                unparser.WriteTag(Token.PROPORTIONAL, false);
            else if (ShowNativeImageSize)
                unparser.WriteTag(Token.NATIVE, false);

            if (IsCut)
                unparser.WriteRect(RectCutted, false);

            INameSpace ns = PathFinder.PathFinderInstance.GetNamespaceFromPath(ImageFile);
            if (
                ns.IsValid() &&
                (ns.NameSpaceType.Type == NameSpaceObjectType.Image || ns.NameSpaceType.Type == NameSpaceObjectType.File)
                )
                unparser.Write(ns.ToString());
            else
                unparser.Write(ImageFile);

            UnparseProp(unparser);

            //----
            Default = tempDefault;
            return true;
        }

        public override void UnparseAuxProp(Unparser unparser)
        {
            if (Align != (AlignType.DT_VCENTER | AlignType.DT_CENTER))
                unparser.WriteAlign(Align, false);
            base.UnparseAuxProp(unparser);
        }


        #region IImage Members

        public Rectangle ImageRect
        {
            get { return Rect; }
        }

        public string ImageFile
        {
            get { return ImageFileName; }
        }

        #endregion
    }

    /// <summary>
    /// Summary description for FileRect.
    /// </summary>
    //================================================================================
    //[Serializable]
    public class FileRect : TextRect
    {
        protected string fileName = string.Empty;
        public FileRect()
            : this((WoormDocument)null)
        {

        }
        //------------------------------------------------------------------------------
        public FileRect(WoormDocument document)
            : base(document)
        {
        }

        //------------------------------------------------------------------------------
        //public FileRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //}

        //------------------------------------------------------------------------------
        public FileRect(FileRect s)
            : base(s)
        {
        }

        //------------------------------------------------------------------------------
        public override BaseObj Clone()
        {
            return new FileRect(this);
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            return
                lex.ParseRect(Token.FILE, out Rect) &&
                lex.ParseString(out Label.Text) &&
                lex.ParseRatio(out HRatio, out VRatio) &&
                ParseBlock(lex);
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            //---- Template Override  
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteRect(Token.FILE, Rect, false);

            if (IsNotDefaultRatio())
                unparser.WriteRatio(HRatio, VRatio, false);

            //TODOLUCA fileName è vuota, non ancora valorizzata
            INameSpace ns = PathFinder.PathFinderInstance.GetNamespaceFromPath(fileName);
            if (
                ns.IsValid() &&
                (ns.NameSpaceType.Type == NameSpaceObjectType.Text || ns.NameSpaceType.Type == NameSpaceObjectType.File)
                )
                unparser.Write(ns.ToString());
            else
                unparser.Write(fileName);

            UnparseProp(unparser);

            //----
            Default = tempDefault;
            return true;
        }
    }

    /// <summary>
    /// Summary description for FieldRect.
    /// </summary>
    //================================================================================
    //[Serializable]

    public class FieldRect : BaseRect, IImage
    {
        public enum EmailParameter
        {
            None = 0,
            From = 1, Subject = 2, Body = 3, Cc = 4, Bcc = 5, Attachment = 6, Identity = 7, To = 8,
            AttachmentReportName = 9, TemplateFileName = 10,
            Fax = 11, Addressee = 12, Address = 13, City = 14,
            County = 15, Country = 16, ZipCode = 17, ISOCode = 18,
            DeliveryType = 19, PrintType = 20,
            To_by_Certified = 21,
            Last = To_by_Certified
        };

        public Label Label;
        public WoormValue Value;

        public string FormatStyleName = DefaultFormat.Testo;
        public bool IsHtml = false;
        public EmailParameter Email;
        public bool AppendMailPart = false;

        public BarCode BarCode = null;
        public bool IsTextFile;
        public bool IsUrlFile;
        public bool IsImage;
        public bool IsCutted;
        public Rectangle RectCutted;
        public bool ShowProportional;
        public bool ShowNativeImageSize;

        public bool Bookmark;

        public WoormViewerExpression LabelTextColorExpr = null; // dynamic UI
        public WoormViewerExpression LabelTextExpr = null;      // dynamic UI
        public WoormViewerExpression FormatStyleExpr = null;    // server-side si applica al value

        //-------------------------------------------------------------------------------
        public Color TemplateTextColor
        {
            get
            {
                return Value.TextColor;
            }
        }

        public Color DynamicTextColor
        {
            get
            {
                if (TextColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = TextColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateTextColor;
            }
        }

        //-------------------------------------------------------------------------------
        public Color TemplateBkgColor
        {
            get
            {
                return Value.BkgColor;
            }
        }

        public Color DynamicBkgColor
        {
            get
            {
                if (BkgColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = BkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateBkgColor;
            }
        }
        //-------------------------------------------------------------------------------
        public Color TemplateLabelTextColor
        {
            get
            {
                return Label.TextColor;
            }
        }
        public Color DynamicLabelTextColor
        {
            get
            {
                if (LabelTextColorExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = LabelTextColorExpr.Eval();
                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));

                }
                return TemplateLabelTextColor;
            }
        }

        //------------------------------------------------------------------------------
        public string TemplateLabelLocalizedText
        {
            get
            {
                return Document.Localizer.Translate(Label.Text);
            }
        }

        public string DynamicLabelLocalizedText
        {
            get
            {
                if (Document == null)
                    return string.Empty;

                if (LabelTextExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = LabelTextExpr.Eval();

                    if (val != null && val.Valid)
                        return Document.Localizer.Translate(val.Data.ToString());
                }
                return TemplateLabelLocalizedText;
            }
        }

        //------------------------------------------------------------------------------
        public string DynamicFormatStyleName
        {
            get
            {
                if (FormatStyleExpr != null)
                {
                    Document.SynchronizeSymbolTable(RepeaterRow);

                    Value val = FormatStyleExpr.Eval();

                    if (val != null && val.Valid)
                        return (val.Data.ToString());
                }
                return FormatStyleName;
            }
        }
        //------------------------------------------------------------------------------
        public bool HasFormatStyleExpr
        {
            get
            {
                return (FormatStyleExpr != null);
            }
        }

        //------------------------------------------------------------------------------
        public override bool IsDynamic()
        {
            return base.IsDynamic() || LabelTextColorExpr != null || this.LabelTextExpr != null;
        }

        //------------------------------------------------------------------------------
        public bool IsBarCode { get { return BarCode != null; } }

        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //	base.GetObjectData(info, context);

        //	info.AddValue(VALUE, Value);
        //	info.AddValue(LABEL, Label);
        //	info.AddValue(LOCALIZEDTEXT, LocalizedText);
        //}

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string s = "\"fieldrect\":{" +

               base.ToJsonTemplate(false) + ',';

            if (this.LabelTextExpr != null || !this.Label.Text.IsNullOrEmpty())
            {
                s += "\"label\":{" +
                        this.TemplateLabelLocalizedText.ToJson("caption", false, true) + ',' +
                        this.TemplateLabelTextColor.ToJson("textcolor") + ',' +
                        this.Label.FontData.ToJson() + ',' +
                        this.Label.Align.ToHtml_align() +
                    "},";
            }
            s +=
                this.Value.FontData.ToJson() + ',' +
                this.Value.Align.ToHtml_align() + ',' +

                this.DynamicBkgColor.ToJson("bkgcolor") + ',' +
                this.DynamicTextColor.ToJson("textcolor") +

                //this.Value.FormattedData    .ToJson("value", false, true) + 

                (this.IsHtml ? ',' + this.IsHtml.ToJson("value_is_html") : "") +
                (this.IsImage ? ',' + this.IsImage.ToJson("value_is_image") : "") +
                (this.IsBarCode ? ',' + this.IsBarCode.ToJson("value_is_barcode") : "") +
                (this.IsBarCode ? ',' + this.BarCode.ToJson() : "");

            string link = BaseObj.GetLink(true, this.Document, this.InternalID);
            if (!link.IsNullOrEmpty())
                s += ',' + link;

            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonHiddenData(bool bracket)
        {
            string s = "\"fieldrect\":{" +
                           base.ToJsonHiddenData(false) +
                            '}';
            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string s = "\"fieldrect\":{" +

                base.ToJsonData(false) + ',';

            if (this.LabelTextColorExpr != null || this.LabelTextExpr != null)
                s +=
                    "\"label\":{" +
                        (this.LabelTextExpr != null ? this.DynamicLabelLocalizedText.ToJson("caption", false, true) + "," : "") +
                        (this.LabelTextColorExpr != null ? this.DynamicLabelTextColor.ToJson("textcolor") : "") +
                    "},";

            s +=
                (this.TextColorExpr != null ? this.DynamicTextColor.ToJson("textcolor") + ',' : "") +
                (this.BkgColorExpr != null ? this.DynamicBkgColor.ToJson("bkgcolor") + ',' : "");

            
            if (HasFormatStyleExpr)
            {
                if (DynamicFormatStyleName.Length > 0)
                {
                    this.Value.FormattedData = this.Document.FormatFromSoapData(DynamicFormatStyleName, InternalID, Value.RDEData);
                }
            }

            s += this.Value.FormattedData.ToJson("value", false, true);

            string link = BaseObj.GetLink(false, this.Document, this.InternalID);
            if (!link.IsNullOrEmpty())
                s += ',' + link;

            s += '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //      public FieldRect(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //	Value = info.GetValue<WoormValue>(VALUE);
        //	Label = info.GetValue<Label>(LABEL);

        //	//TODO silvano: deserializzare localized text

        //}
        //------------------------------------------------------------------------------
        public FieldRect()
            : this((WoormDocument)null)
        {
        }

        //------------------------------------------------------------------------------
        public FieldRect(WoormDocument document)
            : base(document)
        {
            Label = new Label(document);
            Value = new WoormValue(document);
            Value.Align |= AlignType.DT_EX_VCENTER_LABEL;
        }

        //------------------------------------------------------------------------------
        public FieldRect(FieldRect s)
            : base(s)
        {
            this.Value = s.Value.Clone();   
            this.Label = s.Label.Clone();

            this.BarCode = s.BarCode;
            this.IsTextFile = s.IsTextFile;
            this.IsImage = s.IsImage;
            this.IsCutted = s.IsCutted;
            this.ShowProportional = s.ShowProportional;
            this.ShowNativeImageSize = s.ShowNativeImageSize;

            this.Bookmark = s.Bookmark;
            this.RectCutted = s.RectCutted;

            this.Email = s.Email;
            this.AppendMailPart = s.AppendMailPart;
            this.FormatStyleName = s.FormatStyleName;

            this.LabelTextColorExpr = CloneExpr(s.LabelTextColorExpr);
            this.LabelTextExpr = CloneExpr(s.LabelTextExpr);
            this.FormatStyleExpr = CloneExpr(s.FormatStyleExpr);
        }

        //------------------------------------------------------------------------------
        public override BaseObj Clone()
        {
            return new FieldRect(this);
        }

        //------------------------------------------------------------------------------
        override public void ClearData() { Value.Clear(); }

        //------------------------------------------------------------------------------
        internal bool ParseLabelBlock(WoormParser lex)
        {
            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseLabelProps(lex) &&
                    lex.ParseEnd();

            return ParseLabelProp(lex, false);
        }

        //------------------------------------------------------------------------------
        internal bool ParseLabelProps(WoormParser lex)
        {
            bool ok = true;

            do { ok = ParseLabelProp(lex, true) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        internal bool ParseLabelProp(WoormParser lex, bool block)
        {
            bool ok = true;

            switch (lex.LookAhead())
            {
                case Token.TEXTCOLOR:
                    ok = ParseDynamicColor(lex, Token.TEXTCOLOR, out Label.TextColor, ref LabelTextColorExpr);
                    break;

                case Token.ALIGN:
                    ok = lex.ParseAlign(out Label.Align);
                    break;

                case Token.FONTSTYLE:
                    ok = lex.ParseFont(out Label.FontStyleName);
                    break;

                case Token.END: return ok;
                default:
                    if (block)
                    {
                        lex.SetError(WoormViewerStrings.BadLabelProperty);
                        ok = false;
                    }
                    break;
            }

            return ok;
        }


        //------------------------------------------------------------------------------
        internal bool ParseLabel(WoormParser lex)
        {
            string text = "";

            bool ok = lex.ParseTag(Token.LABEL);
            if (!ok) return false;
            if (lex.LookAhead(Token.ASSIGN))
            {
                lex.Matched(Token.ASSIGN);
                LabelTextExpr = new WoormViewerExpression(Document);
                LabelTextExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                LabelTextExpr.ForceSkipTypeChecking = Document.ForLocalizer;
                if (!LabelTextExpr.Compile(lex, CheckResultType.Match, "String"))
                {
                    lex.SetError(WoormViewerStrings.BadTextExpression);
                    return false;
                }
                if (!lex.ParseSep())
                    return false;
            }
            else
                ok = lex.ParseCEdit(out text);

            ok = ok && ParseLabelBlock(lex);

            Label.Text = text;
            return ok;
        }

        //------------------------------------------------------------------------------
        internal bool ParseBitmap(WoormParser lex)
        {
            bool ok = true;

            lex.SkipToken();

            if (lex.Matched(Token.PROPORTIONAL))
            {
                ShowProportional = true;
            }
            else if (lex.Matched(Token.NATIVE))
            {
                ShowNativeImageSize = true;
            }

            if (lex.LookAhead(Token.RECT))
            {
                ok = lex.ParseRect(out RectCutted);
                if (ok)
                    IsCutted = true;
            }

            IsImage = true;
            return ok;
        }

        //------------------------------------------------------------------------------
        internal bool ParseBarCode(WoormParser lex)
        {
            BarCode = new BarCode(Document.ReportSession.PathFinder);
            return lex.ParseBarCode(BarCode);
        }

        //------------------------------------------------------------------------------
        protected override bool ParseProp(WoormParser lex, bool block)
        {
            bool ok = true;
            switch (lex.LookAhead())
            {
                case Token.TEXTCOLOR:
                    ok = ParseTextColor(lex, out Value.TextColor);
                    break;

                case Token.BKGCOLOR:
                    ok = ParseBkgColor(lex, out Value.BkgColor);
                    break;

                case Token.ALIGN:
                    ok = lex.ParseExtendedAlign(out Value.Align);
                    break;

                case Token.PEN: ok = lex.ParsePen(BorderPen); break;
                case Token.BORDERS: ok = lex.ParseBorders(Borders); break;
                case Token.LABEL: ok = ParseLabel(lex); break;
                case Token.BITMAP: ok = ParseBitmap(lex); break;
                case Token.BARCODE:
                    ok = ParseBarCode(lex); break;

                case Token.FILE: lex.SkipToken(); IsTextFile = true; break;
                case Token.LINKURL: lex.SkipToken(); IsUrlFile = true; break;
                case Token.TRANSPARENT: lex.SkipToken(); Transparent = true; break;
                case Token.HTML: lex.SkipToken(); IsHtml = true; break;
                case Token.LAYER:
                    lex.SkipToken();
                    ok = lex.ParseInt(out this.Layer);
                    break;

                case Token.FONTSTYLE:
                    ok = lex.ParseFont(out Value.FontStyleName);
                    break;

                case Token.HIDDEN:
                    {
                        Token[] stopTokens =
                        {
                            Token.SEP, Token.END,
                            Token.LABEL, Token.BARCODE, Token.FILE, Token.MAIL, Token.BITMAP,
                            Token.TEXTCOLOR, Token.ALIGN, Token.FONTSTYLE,
                            Token.BKGCOLOR, Token.PEN, Token.BORDERS, Token.TRANSPARENT, Token.SPECIAL_FIELD,
                            Token.TOOLTIP, Token.STYLE, Token.TEMPLATE, Token.DROPSHADOW,
                            Token.ANCHOR_COLUMN_ID, Token.ANCHOR_PAGE_LEFT, Token.ANCHOR_PAGE_RIGHT,
                            Token.HTML, Token.LAYER
                        };
                        ok = ParseHidden(lex, stopTokens);
                        break;
                    }

                case Token.ANCHOR_PAGE_LEFT:
                    {
                        lex.SkipToken();
                        IsAnchorPageLeft = true;
                        break;
                    }

                case Token.ANCHOR_PAGE_RIGHT:
                    {
                        lex.SkipToken();
                        IsAnchorPageRight = true;
                        break;
                    }
                case Token.ANCHOR_COLUMN_ID:
                    {
                        lex.SkipToken();
                        ok = lex.ParseAlias(out AnchorLeftColumnID);
                        if (!ok)
                            break;

                        if (lex.Matched(Token.COMMA))
                            ok = lex.ParseAlias(out AnchorRightColumnID);

                        break;
                    }

                case Token.MAIL:
                    {
                        int i = 0;
                        lex.SkipToken();
                        ok = lex.ParseInt(out i);
                        if (i < 0 || i > (int)EmailParameter.Last)
                            i = 0;

                        Email = (EmailParameter)i;
                        if (ok && lex.Matched(Token.COMMA))
                        {
                            ok = lex.ParseBool(out AppendMailPart);
                        }

                        break;
                    }

                case Token.TOOLTIP:
                    {
                        Token[] stopTokens = { Token.SEP };
                        ok = ParseTooltip(lex, stopTokens);
                        break;
                    }

                case Token.DROPSHADOW:
                    {
                        lex.SkipToken();
                        if (!lex.ParseInt(out DropShadowHeight))
                            return false;
                        if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                            return false;
                        break;
                    }

                case Token.STYLE:
                    {
                        lex.SkipToken();
                        if (!lex.ParseString(out ClassName))
                            return false;
                        break;
                    }

                case Token.TEMPLATE:
                    {
                        lex.SkipToken();
                        IsTemplate = true;
                        break;
                    }

                case Token.END: return ok;
                default:
                    if (block)
                    {
                        lex.SetError(WoormViewerStrings.BadFieldProperty);
                        ok = false;
                    }
                    break;
            }

            if (Transparent)
                Value.BkgColor = Color.FromArgb(0, 255, 255, 255);
            return ok;
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            bool ok =
                lex.ParseRect(Token.FIELD, out Rect) &&
                lex.ParseRatio(out HRatio, out VRatio) &&
                lex.ParseAlias(out InternalID);

            FormatStyleName = DefaultFormat.None; ;
            if (ok && lex.Matched(Token.FORMATSTYLE))
            {
                if (lex.Matched(Token.ASSIGN))
                {
                    FormatStyleExpr = new WoormViewerExpression(Document);
                    FormatStyleExpr.ForceSkipTypeChecking = Document.ForLocalizer;
                    FormatStyleExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });

                    if (!FormatStyleExpr.Compile(lex, CheckResultType.Match, "String"))
                    {
                        lex.SetError(WoormViewerStrings.BadTooltipExpression);
                        return false;
                    }
                    ok = lex.ParseSep();
                }
                else if (!lex.ParseFormat(out FormatStyleName, false))
                    return false;
            }

            return ok && ParseBlock(lex);
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            if (InternalID >= SpecialReportField.REPORT_LOWER_SPECIAL_ID)
                return true;

            //---- Template Override  
            BaseRect tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteRect(Token.FIELD, Rect, false);

            if (IsNotDefaultRatio())
                unparser.WriteRatio(HRatio, VRatio, false);

            unparser.WriteAlias(InternalID, false);
            unparser.Write(" /* " + this.GetFieldName() + " */ ");

            if (FormatStyleExpr != null && !FormatStyleExpr.ToString().IsNullOrEmpty() && !Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.FORMATSTYLE, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(FormatStyleExpr.ToString());
                unparser.WriteSep(false);
            }
            else if (!FormatStyleName.IsNullOrEmpty())
                unparser.WriteFormat(FormatStyleName, false);

            unparser.WriteLine();
            UnparseProp(unparser);

            //----
            Default = tempDefault;
            return true;
        }

        //------------------------------------------------------------------------------
        private string GetFieldName()
        {
            Variable v = Document.SymbolTable.FindById(InternalID);
            if (v != null)
                return v.Name;

            return "<UNKNOWN FIELD>";
        }

        //------------------------------------------------------------------------------
        public override void UnparseAuxProp(Unparser unparser)
        {
            int col = (TextColorExpr != null || IsNotDefaultTextColor()) ? 1 : 0;
            int bkg = (BkgColorExpr != null || IsNotDefaultBkgColor()) ? 1 : 0;
            int alg = IsNotDefaultAlignment() ? 1 : 0;
            int fnt = IsNotDefaultFontStyle() ? 1 : 0;

            int lab = (LabelTextExpr != null || !Label.Text.IsNullOrEmpty()) ? 1 : 0;
            int lcol = (lab > 0 && (LabelTextExpr != null || IsNotDefaultLabelTextColor())) ? 1 : 0;
            int lalg = (lab > 0 && IsNotDefaultLabelAlignment()) ? 1 : 0;
            int lfnt = (lab > 0 && IsNotDefaultLabelFontStyle()) ? 1 : 0;

            bool bLblk = (lcol + lalg + lfnt) > 0;

            if (IsImage)
            {
                unparser.WriteTag(Token.BITMAP);

                if (ShowProportional)
                    unparser.WriteTag(Token.PROPORTIONAL, false);
                else if (ShowNativeImageSize)
                    unparser.WriteTag(Token.NATIVE, false);

                if (IsCutted)
                    unparser.WriteRect(RectCutted, false);
            }

            else if (BarCode != null)
                BarCode.Unparse(unparser, true);

            else if (IsTextFile)
                unparser.WriteTag(Token.FILE);

            else if (IsUrlFile)
                unparser.WriteTag(Token.LINKURL);

            if (IsEmailParameter)
            {
                unparser.WriteTag(Token.MAIL, false);
                unparser.WriteBlank();
                unparser.Write((int)Email, false);
                if (AppendMailPart)
                {
                    unparser.WriteTag(Token.COMMA, false);
                    unparser.Write(true, false);
                }
                unparser.WriteBlank();
            }

            if (col > 0)
            {
                if (TextColorExpr != null)
                    UnparseDynamicColor(unparser, Token.TEXTCOLOR, TextColorExpr);
                else
                {
                    unparser.WriteColor(Token.TEXTCOLOR, Value.TextColor, false);
                    unparser.WriteSep(true);
                }
            }
            if (bkg > 0)
            {
                if (BkgColorExpr != null)
                    UnparseDynamicColor(unparser, Token.BKGCOLOR, BkgColorExpr);
                else //if (IsNotDefaultBkgColor())
                {
                    unparser.WriteColor(Token.BKGCOLOR, Value.BkgColor, false);
                    unparser.WriteSep(true);
                }
            }

            if (alg > 0)
                unparser.WriteAlign(Value.Align);

            if (fnt > 0)
                unparser.WriteFont(Value.FontStyleName);

            if (IsHtml)
                unparser.WriteTag(Token.HTML);

            if (lab > 0)
            {
                unparser.WriteTag(Token.LABEL, false);
                if (LabelTextExpr != null && !Document.Template.IsSavingTemplate)
                {
                    unparser.WriteTag(Token.ASSIGN, false);
                    unparser.WriteExpr(LabelTextExpr.ToString(), false);
                    unparser.WriteTag(Token.SEP, true);
                }
                else
                    unparser.WriteString(
                        unparser.IsLocalizableTextInCurrentLanguage()
                        ? unparser.LoadReportString(Label.Text)
                        : Label.Text, false);
            }
            unparser.WriteBlank();

            if (bLblk) unparser.WriteBegin();

            if (lcol > 0)
            {
                if (LabelTextColorExpr != null)
                    UnparseDynamicColor(unparser, Token.TEXTCOLOR, LabelTextColorExpr);
                else //if (IsNotDefaultLabelTextColor())
                {
                    unparser.WriteColor(Token.TEXTCOLOR, Label.TextColor, false);
                    unparser.WriteSep(true);
                }
            }

            if (lalg > 0) unparser.WriteAlign(Label.Align);
            if (lfnt > 0) unparser.WriteFont(Label.FontStyleName);
            if (bLblk) unparser.WriteEnd();
        }

        private bool IsEmailParameter { get { return Email != EmailParameter.None; } }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultBkgColor()
        {
            if (Default != null)
                return Value.BkgColor != ((FieldRect)Default).Value.BkgColor;

            return Value.BkgColor != Defaults.DefaultBackColor;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultLabelFontStyle()
        {
            if (Default != null)
                return Label.FontStyleName != ((FieldRect)Default).Label.FontStyleName;

            return Label.FontStyleName != DefaultFont.Descrizione;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultLabelAlignment()
        {
            if (Default != null)
                return Label.Align != ((FieldRect)Default).Label.Align;

            return Label.Align != Defaults.DefaultFieldAlign;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultLabelTextColor()
        {
            if (Default != null)
                return Label.TextColor != ((FieldRect)Default).Label.TextColor;

            return Label.TextColor != Defaults.DefaultTextColor;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultFontStyle()
        {
            if (Default != null)
                return Value.FontStyleName != ((FieldRect)Default).Value.FontStyleName;

            return Value.FontStyleName != DefaultFont.Testo;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultAlignment()
        {
            if (Default != null)
                return (Value.Align != ((FieldRect)Default).Value.Align);

            return Value.Align != Defaults.DefaultFieldAlign;
        }

        //------------------------------------------------------------------------------
        private bool IsNotDefaultTextColor()
        {
            if (Default != null)
                return Value.TextColor != ((FieldRect)Default).Value.TextColor;

            return Value.TextColor != Defaults.DefaultTextColor;
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        public override void SetStyle(BaseRect templateRect)
        {
            RemoveStyle();

            if (templateRect == null || !(templateRect is FieldRect))
                return;

            base.SetStyle(templateRect);

            if (Value.BkgColor == Defaults.DefaultBackColor)
                Value.BkgColor = ((FieldRect)Default).Value.BkgColor;

            if (Value.TextColor == Defaults.DefaultTextColor)
                Value.TextColor = ((FieldRect)Default).Value.TextColor;

            if (Value.Align == GetDefaultAlign())
                Value.Align = ((FieldRect)Default).Value.Align;

            BaseRect.SetTemplateFont(this.Document, ref Value.FontStyleName, ((FieldRect)Default).Value.FontStyleName, DefaultFont.Testo);

            if (Label.TextColor == Defaults.DefaultTextColor)
                Label.TextColor = ((FieldRect)Default).Label.TextColor;

            if (Label.Align == Defaults.DefaultFieldAlign)
                Label.Align = ((FieldRect)Default).Label.Align;

            BaseRect.SetTemplateFont(this.Document, ref Label.FontStyleName, ((FieldRect)Default).Label.FontStyleName, DefaultFont.Descrizione);
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            Value.BkgColor = Default != null ? ((FieldRect)Default).Value.BkgColor : Defaults.DefaultBackColor;
            Value.TextColor = Default != null ? ((FieldRect)Default).Value.TextColor : Defaults.DefaultTextColor;
            Value.Align = Default != null ? ((FieldRect)Default).Value.Align : Defaults.DefaultFieldAlign;

            ClearTemplateFont(Document, ref Value.FontStyleName, Default != null ? ((FieldRect)Default).Value.FontStyleName : null, DefaultFont.Testo);

            Label.TextColor = Default != null ? ((FieldRect)Default).Label.TextColor : Defaults.DefaultTextColor;
            Label.Align = Default != null ? ((FieldRect)Default).Label.Align : Defaults.DefaultFieldAlign;

            ClearTemplateFont(Document, ref Label.FontStyleName, Default != null ? ((FieldRect)Default).Label.FontStyleName : null, DefaultFont.Descrizione);

            base.ClearStyle();
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            if (Default == null)
                return;

            if (Value.BkgColor == ((FieldRect)Default).Value.BkgColor)
                Value.BkgColor = Defaults.DefaultBackColor;

            if (Value.TextColor == ((FieldRect)Default).Value.TextColor)
                Value.TextColor = Defaults.DefaultTextColor;

            if (Value.Align == ((FieldRect)Default).Value.Align)
                Value.Align = Defaults.DefaultFieldAlign;

            BaseRect.RemoveTemplateFont(Document, ref Value.FontStyleName, ((FieldRect)Default).Value.FontStyleName, DefaultFont.Testo);

            if (Label.TextColor == ((FieldRect)Default).Label.TextColor)
                Label.TextColor = Defaults.DefaultTextColor;

            if (Label.Align == ((FieldRect)Default).Label.Align)
                Label.Align = Defaults.DefaultFieldAlign;

            BaseRect.RemoveTemplateFont(Document, ref Label.FontStyleName, ((FieldRect)Default).Label.FontStyleName, DefaultFont.Descrizione);

            base.RemoveStyle();
        }
        /// <summary>
        /// Allineamento di Default per il FieldRect
        /// </summary>
        //------------------------------------------------------------------------------
        public AlignType GetDefaultAlign()
        {
            return (Defaults.DefaultAlign | AlignType.DT_EX_VCENTER_LABEL);
        }

        #region IImage Members

        public Rectangle ImageRect
        {
            get { return Rect; }
        }

        public string ImageFile
        {
            get { return Value.FormattedData; }
        }

        #endregion
    }


    /// <summary>
    /// Summary description for SqrRect.
    /// </summary>
    //================================================================================
    public class WoormViewerExpression : Expression
    {
        public WoormDocument Document;

        //-----------------------------------------------------------------------------
        public WoormViewerExpression(WoormDocument doc)
            : base(doc.ReportSession, doc.SymbolTable)
        {
            this.Document = doc;
        }

        public new WoormViewerExpression Clone()
        {
            return this.MemberwiseClone() as WoormViewerExpression;
        }

        //-----------------------------------------------------------------------------
        override public Value ApplySpecializedFunction(FunctionItem function, Stack paramStack)
        {
            if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetRows", true) == 0)
            {
                Value v1 = (Value)paramStack.Pop();
                ushort id = CastUShort(v1);

                BaseObj b = Document.Objects.FindBaseObj(id);
                if (b == null)
                    return new Value(0);

                if (b is Table)
                {
                    Table t = b as Table;
                    return new Value(t.RowNumber);
                }
                else if (b is Repeater)
                {
                    Repeater r = b as Repeater;
                    return new Value(r.RowsNumber);
                }

                return new Value(0);
            }
            else if (string.Compare(function.Name, "Framework.TbWoormViewer.TbWoormViewer.GetCurrentRow", true) == 0)
            {
                Value v1 = (Value)paramStack.Pop();
                ushort id = CastUShort(v1);

                BaseObj b = Document.Objects.FindBaseObj(id);
                if (b == null)
                    return new Value(0);

                if (b is Table)
                {
                    Table t = b as Table;
                    return new Value(t.ViewCurrentRow);
                }
                else if (b is Repeater)
                {
                    Repeater r = b as Repeater;
                    return new Value(r.ViewCurrentRow);
                }
            }

            return null;
        }
    }

    //================================================================================
    //public static class Extensions
    //{
    //	public static T GetValue<T>(this SerializationInfo info, string name)
    //	{
    //		return (T)info.GetValue(name, typeof(T));
    //	}
    //}

}
