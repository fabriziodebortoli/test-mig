using System;
using System.Collections;
using System.Drawing;
using System.Runtime.Serialization;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.ExpressionManager;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;

namespace RSjson
{
	/// <summary>
	/// Summary description for BaseObj.
	/// </summary>
	//================================================================================
	[Serializable]
	[KnownType(typeof(Rectangle))]
	[KnownType(typeof(Borders))]
	[KnownType(typeof(BasicText))]
	[KnownType(typeof(WoormValue))]
	[KnownType(typeof(Label))]
	abstract public class BaseObj : ISerializable
	{
		const string BASERECT = "BaseRect";
		const string INTERNALID = "InternalID";
		const string HIDDEN = "IsHidden";

		public Rectangle BaseRectangle;
		public bool Transparent;
		public WoormDocument Document;
		public ushort InternalID = 0;

		public bool IsHidden = false;
		public WoormViewerExpression HideExpr = null;	// espressione che se valutata vera nasconde il campo
        public WoormViewerExpression TooltipExpr = null;

		public int DropShadowHeight = 0;
		public Color DropShadowColor = Defaults.DefaultShadowColor; 

		public string ClassName = string.Empty;	//Nome della classe dello stile
		public bool IsTemplate = false;			//Indica che gli attributi grafici di questo oggetto sono usati come template	

		public ushort AnchorRepeaterID = 0;
		public int RepeaterRow = -1;
		
		public bool InheritByTemplate { get; set; }
		public bool IsPersistent { get; set; }

		//------------------------------------------------------------------------------				
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(INTERNALID, InternalID);
			info.AddValue(BASERECT, BaseRectangle);
			info.AddValue(HIDDEN, IsHidden);
		}
		
		//------------------------------------------------------------------------------
		public BaseObj(SerializationInfo info, StreamingContext context)
		{
			InternalID = info.GetUInt16(INTERNALID);
			BaseRectangle = info.GetValue<Rectangle>(BASERECT);
			IsHidden = info.GetBoolean(HIDDEN);
		}
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
			this.BaseRectangle = s.BaseRectangle;
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
			if (lex.Parsed(Token.WHEN))
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

			lex.Parsed(Token.SEP);
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
			lex.Parsed(Token.ASSIGN);

            TooltipExpr = new WoormViewerExpression(Document);
			TooltipExpr.StopTokens = new StopTokens(stopTokens);
			TooltipExpr.ForceSkipTypeChecking = Document.ForLocalizer;
			if (!TooltipExpr.Compile(lex, CheckResultType.Match, "String"))
			{
				lex.SetError(WoormViewerStrings.BadTooltipExpression);
				return false;
			}

			lex.Parsed(Token.SEP);
			return true;
		}

		//-------------------------------------------------------------------------------
		public string GetTooltip
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
		public virtual bool Parse       (WoormParser lex) { return true; }
		public virtual bool Unparse     (Unparser unparser) { return true; }

        public virtual void SetStyle    (BaseRect r) {}
        public virtual void ClearStyle  () {}
        public virtual void RemoveStyle () {}
 	}

	/// <summary>
	/// Summary description for BaseRect.
	/// </summary>
	//================================================================================
	[Serializable]
	public abstract class BaseRect : BaseObj
	{
		public BorderPen BorderPen = new BorderPen();
		public Borders Borders = new Borders(true);

		public int HRatio;
		public int VRatio;

		protected BaseRect Default = null;
        public WoormViewerExpression TextColorExpr = null;
        public WoormViewerExpression BkgColorExpr = null;

		//attributes not used in Easylook, used only for Z-print in woorm c++. Here they are only parsed
		public bool IsAnchorPageLeft = false;
		public bool IsAnchorPageRight = false;
		public ushort AnchorLeftColumnID = 0;
		public ushort AnchorRightColumnID = 0;
		const string BORDERS = "Borders";
		public bool TemplateOverridden = false;
        public int Layer = 0;   //only design mode

        //------------------------------------------------------------------------------
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(BORDERS, Borders);
		}
		//------------------------------------------------------------------------------
		public BaseRect(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Borders = info.GetValue<Borders>(BORDERS);
		}
		
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
		public Rectangle InsideRect
		{
			get
			{
				if (Document.Options.ConNoBorders) 
					return BaseRectangle;

				return CalculateInsideRect(BaseRectangle, Borders, BorderPen);
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
			BaseRectangle = Rectangle.FromLTRB(x1, y1, x2, y2);
		}

		//------------------------------------------------------------------------------
        public override void MoveBaseRect(int xOffset, int yOffset, bool bIgnoreBorder = false)
		{
            MoveBaseRect(BaseRectangle.Left + xOffset, BaseRectangle.Top + yOffset, BaseRectangle.Right + xOffset, BaseRectangle.Bottom + yOffset, bIgnoreBorder);
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
			aColor = Color.Black;  //inizializzo con un valore che non sara utilizzato perche e' param out
			if (!lex.ParseTag(token))
				return false;

			if (lex.Parsed(Token.ASSIGN))
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

			if (DropShadowHeight == 0 && DropShadowColor.Equals(Color.AliceBlue))
			{
				DropShadowHeight = templateRect.DropShadowHeight;
				DropShadowColor = templateRect.DropShadowColor;
			}
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
			DropShadowColor = Default != null ? Default.DropShadowColor : Color.Black; 
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
				DropShadowColor = Color.Black;
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
	[Serializable]
	public class SqrRect : BaseRect
	{
		public Color BkgColor = Defaults.DefaultBackColor;
		public SqrRect(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
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
			this.BkgColor = s.BkgColor;
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
				case Token.BKGCOLOR		: ok = lex.ParseBkgColor(out BkgColor);		break;
				case Token.PEN			: ok = lex.ParsePen     (BorderPen);		break;
				case Token.BORDERS		: ok = lex.ParseBorders (Borders);			break;
				case Token.TRANSPARENT : 
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

				case Token.ANCHOR_PAGE_LEFT :
				{
					lex.SkipToken();
					IsAnchorPageLeft = true;
					break;
				}

				case Token.ANCHOR_PAGE_RIGHT :
				{
					lex.SkipToken();
					IsAnchorPageRight= true;
					break;
				}
		
                case Token.ANCHOR_COLUMN_ID:
                {
                    lex.SkipToken();
                    ok = lex.ParseAlias(out AnchorLeftColumnID);
                    if (!ok)
                        break;

                    if (lex.Parsed(Token.COMMA))
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
				BkgColor = Color.Transparent;
			return ok;
		}

		//------------------------------------------------------------------------------
		public override bool Parse(WoormParser lex)
		{
			Token token = lex.LookAhead(Token.RNDRECT) ?
					Token.RNDRECT :
					Token.SQRRECT;

			return
				lex.ParseRect(token, out BaseRectangle) &&
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

			unparser.WriteRect(Token.SQRRECT, BaseRectangle, false);

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
				unparser.WriteColor(Token.BKGCOLOR, BkgColor, false);
				unparser.WriteSep(true);
			}
		}

		//------------------------------------------------------------------------------
		bool IsNotDefaultBkgColor() 
		{
			if (Default != null)
				return BkgColor != ((SqrRect)Default).BkgColor;

			return BkgColor != Defaults.DefaultBackColor;
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

			if (BkgColor == Defaults.DefaultBackColor)
				BkgColor = ((SqrRect)Default).BkgColor;
		}

		//------------------------------------------------------------------------------
        public override void ClearStyle()
		{
			if (InheritByTemplate) 
				return;

			BkgColor = Default != null
				? ((SqrRect)Default).BkgColor
				: Defaults.DefaultBackColor;

			base.ClearStyle();
		}

		//------------------------------------------------------------------------------
        public override void RemoveStyle()
		{
			if (InheritByTemplate || Default == null) 
				return;

			if (BkgColor == ((SqrRect)Default).BkgColor)
				BkgColor = Defaults.DefaultBackColor;

			base.RemoveStyle();
		}
	}

	/// <summary>
	/// Summary description for TextRect.
	/// </summary>
	//================================================================================
	[Serializable]
	[KnownType(typeof(Color))]
	public class TextRect : BaseRect
	{
		public BasicText Label = null;
		public bool Special = false;
        public bool IsHtml = false;
        public BarCode BarCode = null;
		private string localizedText = String.Empty;
		const string LABEL = "Label";
		const string LOCALIZEDTEXT = "LocalizedText";
		const string TEXTCOLOR = "TextColor";
		const string BACKCOLOR = "BackColor";

		//-------------------------------------------------------------------------------
		public Color TextColor
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
				return Label.TextColor;
			}
		}
		//-------------------------------------------------------------------------------
		public Color BkgColor
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
				return Label.BkgColor;
			}
		}

		//Tiene conto anche del colore del font che prevale su quello dell'oggetto
		//-------------------------------------------------------------------------------
		public string ForeColorToSerialize
		{
			get
			{
				return Document!= null ?
							ColorTranslator.ToHtml(Document.TrueColor(BoxType.Text,TextColor,Label.FontStyleName))
							:
							ColorTranslator.ToHtml(TextColor);
			}
		}

		//Tiene conto anche del colore del font che prevale su quello dell'oggetto
		//-------------------------------------------------------------------------------
		public string BackColorToSerialize
		{
			get
			{
				return Document!= null ?
							ColorTranslator.ToHtml(Document.TrueColor(BoxType.Text, BkgColor, Label.FontStyleName))
							:
							ColorTranslator.ToHtml(BkgColor);
			}
		}

		//------------------------------------------------------------------------------
		public string Text
		{
			get
			{
				if (!Special) 
					return Label.Text;

				SpecialField sf = new SpecialField(Document);
				return sf.Expand(Label.Text);
			}
		}
		
		//------------------------------------------------------------------------------
		public string LocalizedText 
		{ 
			get 
			{ 
				if ((String.IsNullOrEmpty(localizedText) || Special)  && Document != null)
				{ 
					localizedText = Document.Localizer.Translate(Label.Text);
                    if (Special)
					{
						SpecialField sf = new SpecialField(Document);
                        localizedText = sf.Expand(localizedText); 
					}
				}
				return localizedText;
			}

			set { localizedText = value; }
		}

		//------------------------------------------------------------------------------
		public TextRect(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
				Label = info.GetValue<BasicText>(LABEL);
				LocalizedText = info.GetString(LOCALIZEDTEXT);
				//TODO SILVANO recuperare il colore
		}

		//------------------------------------------------------------------------------
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(LABEL, Label);
			info.AddValue(LOCALIZEDTEXT, LocalizedText);
			info.AddValue(TEXTCOLOR, ForeColorToSerialize);
			info.AddValue(BACKCOLOR, BackColorToSerialize);
		}

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
				case Token.TRANSPARENT  : 
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

				case Token.ANCHOR_PAGE_RIGHT :
				{
					lex.SkipToken();
					IsAnchorPageRight= true;
					break;
				}
		
				case Token.ANCHOR_COLUMN_ID :
				{
					lex.SkipToken();
					ok = lex.ParseAlias(out AnchorLeftColumnID);
					if (!ok)
						break;

					if (lex.Parsed(Token.COMMA))
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
				case Token.BARCODE: ok = ParseBarCode(lex); break;

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
				Label.BkgColor = Color.Transparent;

			return ok;
		}

		//------------------------------------------------------------------------------
		public override bool Parse(WoormParser lex)
		{
			string text = "";

			bool ok =
				lex.ParseRect(Token.TEXT, out BaseRectangle) &&
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

			unparser.WriteRect(Token.TEXT, this.BaseRectangle, false);

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
				unparser.WriteColor(Token.BKGCOLOR, BkgColor, false);
				unparser.WriteSep(true);
			}

			if (TextColorExpr != null)
				UnparseDynamicColor(unparser, Token.TEXTCOLOR, TextColorExpr);
			else if (IsNotDefaultTextColor())
			{
				unparser.WriteColor(Token.TEXTCOLOR, TextColor, false);
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
	[Serializable]
	public class GraphRect : SqrRect, IImage
	{
		public string ImageFileName;
		public bool IsCut = false;
		public bool ShowProportional = false;
        public bool ShowNativeImageSize = false;
		public Rectangle RectCutted;
        public int Align = BaseObjConsts.DT_CENTER | BaseObjConsts.DT_VCENTER;

        const string ISCUT = "IsCut";
        const string ALIGN = "Align";

        //------------------------------------------------------------------------------
        public GraphRect(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			IsCut = info.GetBoolean(ISCUT);
		}
		//------------------------------------------------------------------------------
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(ISCUT, IsCut);
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
			this.IsCut = s.IsCut;
			this.ShowProportional = s.ShowProportional;
            this.ShowNativeImageSize = s.ShowNativeImageSize;
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

			bool ok = lex.ParseRect(Token.BITMAP, out BaseRectangle);
            if (!ok)
                return false;

			if (lex.Parsed(Token.PROPORTIONAL))
			{
                ShowProportional = true;
			}
            else if (lex.Parsed(Token.NATIVE))
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
                return lex.ParseAlign(out Align);

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

			unparser.WriteRect(Token.BITMAP, BaseRectangle, false);

			if (ShowProportional)
				unparser.WriteTag(Token.PROPORTIONAL, false);
            else if (ShowNativeImageSize)
                unparser.WriteTag(Token.NATIVE, false);

            if (IsCut)
				unparser.WriteRect(RectCutted, false);

			INameSpace ns = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(ImageFile);
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
            if (Align != (BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_CENTER))
                unparser.WriteAlign(Align, false);
            base.UnparseAuxProp(unparser);
        }


    #region IImage Members

    public Rectangle ImageRect
		{
			get { return BaseRectangle; }
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
	[Serializable]
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
		public FileRect(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

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
				lex.ParseRect(Token.FILE, out BaseRectangle) &&
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

			unparser.WriteRect(Token.FILE, BaseRectangle, false);

			if (IsNotDefaultRatio())
				unparser.WriteRatio(HRatio, VRatio, false);

			//TODOLUCA fileName è vuota, non ancora valorizzata
			INameSpace ns = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(fileName);
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
	[Serializable]

	public class FieldRect : BaseRect, IImage
	{
		public enum EmailParameter
        { 
            None = 0, 
            From = 1, Subject = 2, Body = 3, Cc = 4, Bcc = 5, Attachment = 6, Identity = 7, To = 8, 
            AttachmentReportName=9, TemplateFileName=10, 
        	Fax=11, Addressee=12, Address=13, City=14, 
		    County=15, Country=16, ZipCode=17, ISOCode=18,
		    DeliveryType=19, PrintType=20,
		    To_by_Certified=21,
            Last = To_by_Certified
        };

		public BarCode BarCode = null;
		public WoormValue Value;
		public Label Label;
		public bool IsTextFile;
		public bool IsUrlFile;
		public bool IsImage;
		public bool IsCutted;
		public bool ShowProportional;
        public bool ShowNativeImageSize;
        public bool Bookmark;
		public Rectangle RectCutted;
		public EmailParameter Email;
        public bool AppendMailPart = false;
		public string FormatStyleName = DefaultFormat.None;
        public bool IsHtml = false;

        public WoormViewerExpression LabelTextColorExpr = null;
        public WoormViewerExpression LabelTextExpr = null;
        public WoormViewerExpression FormatStyleExpr = null;

		const string VALUE = "Value";
		const string LABEL = "Label";
		const string LOCALIZEDTEXT = "LocalizedText";

		//-------------------------------------------------------------------------------
		public Color TextColor
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
				return Value.TextColor;
			}
		}
		//-------------------------------------------------------------------------------
		public Color BkgColor
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
				return Value.BkgColor;
			}
		}
		//-------------------------------------------------------------------------------
		public Color LabelTextColor
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
				return Label.TextColor;
			}
		}

		//------------------------------------------------------------------------------
		public string LocalizedText
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
				return Document.Localizer.Translate(Label.Text);
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
		public bool IsBarCode { get { return BarCode != null; } }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(VALUE, Value);
			info.AddValue(LABEL, Label);
			info.AddValue(LOCALIZEDTEXT, LocalizedText);
		}
		public FieldRect(SerializationInfo info, StreamingContext context)

			: base(info, context)
		{
			Value = info.GetValue<WoormValue>(VALUE);
			Label = info.GetValue<Label>(LABEL);

			//TODO silvano: deserializzare localized text
				
		}
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
			Value.Align |= BaseObjConsts.DT_EX_VCENTER_LABEL;
		}

		//------------------------------------------------------------------------------
		public FieldRect(FieldRect s)
			: base(s)
		{
			this.Value = s.Value.Clone();   //DEEP clone !

			this.Label = s.Label;   //? deep

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
				lex.Parsed(Token.ASSIGN);
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

			if (lex.Parsed(Token.PROPORTIONAL) )
			{
				ShowProportional = true;
			}
            else if (lex.Parsed(Token.NATIVE))
            {
                ShowNativeImageSize = true;
            }

            if (lex.LookAhead(Token.RECT) )
			{
				ok = lex.ParseRect (out RectCutted);
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

				case Token.PEN          : ok = lex.ParsePen         (BorderPen);	break;
				case Token.BORDERS      : ok = lex.ParseBorders     (Borders);		break;
				case Token.LABEL        : ok = ParseLabel           (lex);			break;
				case Token.BITMAP        : ok = ParseBitmap          (lex);			break;
				case Token.BARCODE      : ok = ParseBarCode         (lex);			break;

				case Token.FILE			: lex.SkipToken(); IsTextFile = true;		break;
				case Token.LINKURL		: lex.SkipToken(); IsUrlFile = true;		break;
				case Token.TRANSPARENT	: lex.SkipToken(); Transparent = true;		break;
                case Token.HTML         : lex.SkipToken(); IsHtml = true;           break;
                case Token.LAYER        : lex.SkipToken();
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

				case Token.ANCHOR_PAGE_LEFT :
				{
					lex.SkipToken();
					IsAnchorPageLeft = true;
					break;
				}

				case Token.ANCHOR_PAGE_RIGHT :
				{
					lex.SkipToken();
					IsAnchorPageRight= true;
					break;
				}
                case Token.ANCHOR_COLUMN_ID:
                {
                    lex.SkipToken();
                    ok = lex.ParseAlias(out AnchorLeftColumnID);
                    if (!ok)
                        break;

                    if (lex.Parsed(Token.COMMA))
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

					Email = (EmailParameter) i;
	                if (ok && lex.Parsed(Token.COMMA))
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
				Value.BkgColor = Color.Transparent;
			return ok;
		}

		//------------------------------------------------------------------------------
		public override bool Parse(WoormParser lex)
		{
			bool ok =
				lex.ParseRect(Token.FIELD, out BaseRectangle) &&
				lex.ParseRatio(out HRatio, out VRatio) &&
				lex.ParseAlias(out InternalID);

			FormatStyleName = DefaultFormat.None; ;
			if (ok && lex.Parsed(Token.FORMATSTYLE))
			{
				if (lex.Parsed(Token.ASSIGN))
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

			unparser.WriteRect(Token.FIELD, BaseRectangle, false);

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

			if (Label.TextColor ==((FieldRect)Default).Label.TextColor)
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
		public int GetDefaultAlign()
		{
			return (Defaults.DefaultAlign | BaseObjConsts.DT_EX_VCENTER_LABEL);
		}

		#region IImage Members

		public Rectangle ImageRect
		{
			get { return BaseRectangle; }
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
	public class MetafileRect : GraphRect
	{

		//------------------------------------------------------------------------------
		public MetafileRect(WoormDocument document)
			: base(document)
		{
		}

		//------------------------------------------------------------------------------
		public override bool Parse(WoormParser lex)
		{
			return
				lex.ParseRect(Token.METAFILE, out BaseRectangle) &&
				lex.ParseString(out ImageFileName) &&
				ParseBlock(lex);
		}

		//------------------------------------------------------------------------------
		public override bool Unparse(Unparser unparser)
		{
			return true;
		}
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
				Value v1 = (Value) paramStack.Pop();
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
	public interface IImage
	{
		Rectangle ImageRect { get; }

		string ImageFile { get; }
	}

	//================================================================================
	public static class Extensions
	{
		public static T GetValue<T>(this SerializationInfo info, string name)
		{
			return (T)info.GetValue(name, typeof(T));
		}
	}

}
