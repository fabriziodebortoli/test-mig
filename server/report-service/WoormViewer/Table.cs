using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
//using System.Runtime.Serialization;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using Microarea.Common.ExpressionManager;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.WoormWebControl;
//using Microarea.RSWeb.Temp;

namespace Microarea.RSWeb.Objects
{
    internal enum ElementColor { LABEL, VALUE, BACKGROUND, BORDER, MAX };

    /// <summary>
    /// Summary description for TableCell.
    /// </summary>
    /// 
    /// ================================================================================
    //[Serializable]
    //[KnownType(typeof(BasicText))]
    public class Cell //: ISerializable
    {
        public Column column;

        //const string SUBTOTAL		= "SubTotal";
        //const string VALUE			= "Value";
        //const string RECTCELL		= "RectCell";
        //const string ATROWNUMBER	= "AtRowNumber";

        public WoormValue Value;
        public Rectangle RectCell;
        public bool SubTotal = false;   // dinamicamente dice se la cella contiene un subtotal
        public int AtRowNumber = -1;

        //------------------------------------------------------------------------------
        public Cell() { }

        //-------------------------------------------------------------------------------
        //public Cell(SerializationInfo info, StreamingContext context)
        //{
        //	SubTotal = info.GetBoolean(SUBTOTAL);
        //	Value = info.GetValue<WoormValue>(VALUE);
        //	RectCell = info.GetValue<Rectangle>(RECTCELL);
        //	AtRowNumber = info.GetInt32(ATROWNUMBER);
        //}

        //------------------------------------------------------------------------------
        public Cell(Column col, Point origin, Size size)
        {
            Value = new WoormValue(col.Table.Document);
            Value.FontStyleName = DefaultFont.CellaStringa;
            Value.Align = Defaults.DefaultCellStringAlign;
            RectCell = new Rectangle(origin, size);
            column = col;
        }

        //------------------------------------------------------------------------------
        public Cell(Cell source)
        {
            this.Value = source.Value;
            this.RectCell = source.RectCell;
            this.AtRowNumber = source.AtRowNumber;  //== m_nCurrRow?
            this.column = source.column;
        }

        //------------------------------------------------------------------------------
        public void Clear()
        {
            this.Value.Clear();
            SubTotal = false;
        }

        //------------------------------------------------------------------------------
        public bool HasBkgColorExpr { get { return (column.BkgColorExpr != null); } }
        public bool HasSubTotBkgColorExpr { get { return (column.SubTotalBkgColorExpr != null); } }
        public bool HasTextFontStyleExpr { get { return (column.TextFontStyleExpr != null); } }
        public bool HasFormatStyleExpr { get { return (column.FormatStyleExpr != null); } }
        public bool HasCellBordersExpr { get { return (column.CellBordersExpr != null); } }
        internal bool IsFirstRow { get { return AtRowNumber == 0; } }
        internal bool IsLastRow { get { return AtRowNumber == column.Table.RowNumber - 1; } }
        internal AlignType CellAlign { get { return Value.Align; } }

        //-------------------------------------------------------------------------------
        public Color TemplateSubTotalTextColor
        {
            get
            {
                return column.SubTotal.TextColor;
            }
        }
        public Color DynamicSubTotalTextColor
        {
            get
            {
                if (column.SubTotalTextColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.SubTotalTextColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateSubTotalTextColor;
            }
        }

        //-------------------------------------------------------------------------------
        public Color TemplateTextColor { get { return Value.TextColor; } }

        public Color DynamicTextColor
        {
            get
            {
                if (column.TextColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.TextColorExpr.Eval();

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
                if (column.BkgColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.BkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateBkgColor;
            }
        }

        public Color GetDynamicBkgColor(Color cr)
        {
            if (column.BkgColorExpr != null)
            {
                column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                Value val = column.BkgColorExpr.Eval();

                if (val != null && val.Valid)
                    return Color.FromArgb(255, Color.FromArgb((int)val.Data));   //setta c.A = 255; necessario per il colore nel PDF;
            }
            return cr;
        }

        //-------------------------------------------------------------------------------
        public Color TemplateSubTotalBkgColor
        {
            get
            {
                return column.SubTotal.BkgColor;
            }
        }

        public Color DynamicSubTotalBkgColor
        {
            get
            {
                if (column.SubTotalBkgColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.SubTotalBkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateSubTotalBkgColor;
            }
        }

        public Color GetDynamicSubTotalBkgColor(Color cr)
        {
            if (column.SubTotalBkgColorExpr != null)
            {
                column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                Value val = column.SubTotalBkgColorExpr.Eval();

                if (val != null && val.Valid)
                    return Color.FromArgb(255, Color.FromArgb((int)val.Data));
            }
            return cr;
        }


        //-------------------------------------------------------------------------------
        public string DynamicTextFontStyleName
        {
            get
            {
                if (column.TextFontStyleExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.TextFontStyleExpr.Eval();

                    if (val != null && val.Valid)
                        return val.Data as string;
                }
                return string.Empty;
            }
        }

        //-------------------------------------------------------------------------------
        public string DynamicFormatStyleName
        {
            get
            {
                if (column.FormatStyleExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.FormatStyleExpr.Eval();

                    if (val != null && val.Valid)
                        return val.Data as string;
                }
                return column.FormatStyleName;
            }
        }

        //-------------------------------------------------------------------------------
        public string DynamicTooltip
        {
            get
            {
                if (column.TooltipExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

                    Value val = column.TooltipExpr.Eval();

                    if (val != null && val.Valid)
                        return (string)val.Data;
                }
                return string.Empty;
            }
        }

        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Borders DynamicCellBorders(Borders colBorders)
        {
            if (column.CellBordersExpr == null)
                return colBorders;

            column.Table.Document.SynchronizeSymbolTable(AtRowNumber);

            Value val = column.CellBordersExpr.Eval();

            if (val != null && val.Valid)
            {
                Borders border = new Borders(colBorders.Top, colBorders.Left, colBorders.Bottom, colBorders.Right);

                string s = (val.Data as string).ToUpper();
                s = s.Remove(' ');

                int idx = s.IndexOf("LEFT");
                if (idx >= 0)
                    border.Left = idx == 0 ? true : s[idx - 1] != '-';

                idx = s.IndexOf("RIGHT");
                if (idx >= 0)
                    border.Right = idx == 0 ? true : s[idx - 1] != '-';

                idx = s.IndexOf("TOP");
                if (idx >= 0)
                    border.Top = idx == 0 ? true : s[idx - 1] != '-';

                idx = s.IndexOf("BOTTOM");
                if (idx >= 0)
                    border.Bottom = idx == 0 ? true : s[idx - 1] != '-';
                return border;
            }
            return colBorders;
        }

        //-------------------------------------------------------------------------------
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //	info.AddValue(SUBTOTAL, SubTotal);
        //	info.AddValue(VALUE, Value);
        //	info.AddValue(RECTCELL, RectCell);
        //	info.AddValue(ATROWNUMBER, AtRowNumber);
        //}

        //------------------------------------------------------------------------------
        internal Color[] CellColor
        {
            get
            {
                Color[] colors = new Color[(int)ElementColor.MAX];
                colors[(int)ElementColor.VALUE] = Value.TextColor;
                colors[(int)ElementColor.BACKGROUND] = Value.BkgColor;
                return colors;
            }
            set
            {
                Value.TextColor = (value[(int)ElementColor.VALUE]);
                Value.BkgColor = (value[(int)ElementColor.BACKGROUND]);
            }
        }

        //------------------------------------------------------------------------------
        internal void HMoveCell(int width)
        {
            RectCell.Offset(width, 0);
        }

        //------------------------------------------------------------------------------
        public string FormattedDataForWrite
        {
            get
            {
                string s = this.Value.FormattedData;

                if (
                       !column.VMergeEqualCell ||
                        string.IsNullOrEmpty(s)
                    )
                {
                    //System.Diagnostics.Debug.Assert(this.Value.RDEData == null, "rde not null");
                    return s;
                }
                if (!string.IsNullOrEmpty(column.PreviousValue))
                {
                    if (column.PreviousValue == s)
                    {
                        return string.Empty;
                    }
                }

                if (!this.Value.CellTail)
                    column.PreviousValue = s;

                return s;
            }
        }
        //---------------------------------------------------------------------
        public string ToJsonTemplate(Borders borders, bool useAlternateColor)
        {
            Borders cellBorders = this.DynamicCellBorders(borders);

            string s = "{\"id" + this.column.InternalID.ToString() + "\":{";

            s += this.TemplateTextColor.ToJson("textcolor") + ',';

            s += (useAlternateColor ? this.column.Table.EasyviewColor : this.TemplateBkgColor).ToJson("bkgcolor") + ',';

            s += this.CellAlign.ToHtml_align() + ',';

            s += this.Value.FontData.ToJson() + ',';

            s += cellBorders.ToJson() + ',' + column.ColumnPen.ToJson();

            //LINK
            string link = BaseObj.GetLink(true, this.column.Table.Document, this.column.InternalID, this.AtRowNumber);
            if (!link.IsNullOrEmpty())
                s += ',' + link;

            //TODO opzionali
            //s += ',' + (string.Empty).ToJson("tooltip", false, true);
            //s += ',' + (string.Empty).ToJson("value", false, true);

            s += "}}";
            return s;
        }

        public string ToJsonData(Borders borders, bool useAlternateColor)
        {
            Borders cellBorders = this.DynamicCellBorders(borders);

            string s = "{\"id" + this.column.InternalID.ToString() + "\":{";

            //VALUE

            this.Value.FormattedData = string.Empty;
            if (this.Value.RDEData != null)
            {
                string formatStyleName = this.DynamicFormatStyleName;
                if (formatStyleName.Length <= 0)
                    formatStyleName = "string";

                this.Value.FormattedData = column.Table.Document.FormatFromSoapData(formatStyleName, this.column.InternalID, this.Value.RDEData);

            }

            s += (this.SubTotal ?
                            this.Value.FormattedData.ToJson("value", false, true)
                            :
                            this.FormattedDataForWrite.ToJson("value", false, true)
                        );
            //BORDERS
            if (column.Table.HasDynamicHiddenColumns() || column.Table.HasDynamicBorders() || column.Table.Borders.DynamicRowSeparator)
                s += ',' + cellBorders.ToJson();

            //TEXTCOLOR
            if (!this.SubTotal && column.TextColorExpr != null)
                s += ',' + this.DynamicTextColor.ToJson("textcolor");
            else if (this.SubTotal)
                s += ',' + this.DynamicSubTotalTextColor.ToJson("textcolor");

            //BKGCOLOR
            if (!this.SubTotal && column.BkgColorExpr != null)
                s += ',' + this.GetDynamicBkgColor(useAlternateColor ? this.column.Table.EasyviewColor : this.TemplateBkgColor).ToJson("bkgcolor");
            else if (this.SubTotal)
                s += ',' + this.GetDynamicSubTotalBkgColor(useAlternateColor ? this.column.Table.EasyviewColor : this.TemplateSubTotalBkgColor).ToJson("bkgcolor");

            //FONT
            if (!this.SubTotal && column.TextFontStyleExpr != null)
            {
                string fontstyle = this.DynamicTextFontStyleName;

                if (!fontstyle.CompareNoCase(this.Value.FontStyleName))
                {
                    FontElement fe = column.Table.Document.GetFontElement(fontstyle);
                    if (fe != null)
                    {
                        FontData fontData = new FontData(fe);
                        s += ',' + fontData.ToJson();
                    }
                }
            }
            else if (this.SubTotal)
            {
                s += ',' + this.column.SubTotal.FontData.ToJson();
            }

            //TOOLTIP
            if (column.TooltipExpr != null)
                s += ',' + this.DynamicTooltip.ToJson("tooltip", false, true);

            //LINK
            string link = BaseObj.GetLink(false, this.column.Table.Document, this.column.InternalID, this.AtRowNumber);
            if (!link.IsNullOrEmpty())
                s += ',' + link;

            s += "}}";
            return s;
        }
    }
    /// <summary>
    /// SubTotalCell : 
    /// servono solo gli stili dei font e gli align. da sostituire a quelli della cella
    /// </summary>
    // ================================================================================
    //[Serializable]
    public class SubTotalCell : BasicText
    {
        public SubTotalCell(WoormDocument doc) : base(doc)
        {
            FontStyleName = DefaultFont.SubTotaleNumerico;
            Align = Defaults.DefaultTotalStringAlign;
        }
    }

    /// <summary>
    /// Summary description for TableCell.
    /// </summary>
    // ================================================================================
    //[Serializable]
    public class TotalCell : Cell
    {
        public BorderPen TotalPen = new BorderPen();

        //-------------------------------------------------------------------------------
        public TotalCell() { }

        //-------------------------------------------------------------------------------
        public TotalCell(Column col, Point origin, Size size)
            : base(col, origin, size)
        {
            Value.FontStyleName = DefaultFont.TotaleNumerico;
            Value.Align = Defaults.DefaultTotalStringAlign;
        }

        //-------------------------------------------------------------------------------
        public TotalCell(TotalCell source)
            : base(source)
        {
            this.TotalPen = source.TotalPen;
        }

        //-------------------------------------------------------------------------------
        public Color TemplateTotalTextColor
        {
            get
            {
                return Value.TextColor;
            }
        }
        public Color DynamicTotalTextColor
        {
            get
            {
                if (column.TotalTextColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(-1);

                    Value val = column.TotalTextColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));
                }
                return TemplateTotalTextColor;
            }
        }

        //-------------------------------------------------------------------------------
        public Color TemplateTotalBkgColor
        {
            get
            {
                return Value.BkgColor;
            }
        }

        public Color DynamicTotalBkgColor
        {
            get
            {
                if (column.TotalBkgColorExpr != null)
                {
                    column.Table.Document.SynchronizeSymbolTable(-1);

                    Value val = column.TotalBkgColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));

                }
                return TemplateTotalBkgColor;
            }
        }

        //-------------------------------------------------------------------------------
        public AlignType Align { get { return Value.Align; } }
        public FontData FontData { get { return Value.FontData; } }

        //-------------------------------------------------------------------------------
        internal Color[] TotalColor
        {
            get
            {
                Color[] colors = new Color[(int)ElementColor.MAX];
                colors[(int)ElementColor.VALUE] = Value.TextColor;
                colors[(int)ElementColor.BACKGROUND] = Value.BkgColor;
                colors[(int)ElementColor.BORDER] = TotalPen.Color;
                return colors;
            }
        }

        public string ToJsonTemplate(Borders border, BorderPen pen)
        {
            string s = "{\"id" + this.column.InternalID.ToString() + "\":{";

            //if (column.ShowTotal)
            {
                s +=
                    this.TemplateTotalTextColor.ToJson("textcolor") + ',' +
                    this.TemplateTotalBkgColor.ToJson("bkgcolor") + ',' +

                    this.Align.ToHtml_align() + ',' +
                    this.FontData.ToJson() + ',';
            }

            s += border.ToJson() + ',' + pen.ToJson() + "}}";
            return s;
        }

        public string ToJsonData(Borders border, BorderPen pen)
        {
            string s = "{\"id" + this.column.InternalID.ToString() + "\":{";

            if (column.ShowTotal)
            {
                s +=
                   (column.TotalTextColorExpr != null ? this.DynamicTotalTextColor.ToJson("textcolor") + ',' : "") +
                   (column.TotalBkgColorExpr != null ? this.DynamicTotalBkgColor.ToJson("bkgcolor") + ',' : "");

                this.Value.FormattedData = string.Empty;
                if (this.Value.RDEData != null)
                {
                    string formatStyleName = this.DynamicFormatStyleName;
                    if (formatStyleName.Length > 0)
                    {
                        this.Value.FormattedData = column.Table.Document.FormatFromSoapData(formatStyleName, this.column.InternalID, this.Value.RDEData);
                    }
                }

                s += this.Value.FormattedData.ToJson("value", false, true) + ',';
            }


            if (column.Table.HasDynamicHiddenColumns())
                s += border.ToJson() + ',' + pen.ToJson();
            else
                s = s.TrimEnd(new char[] { ',' });

            s += "}}";
            return s;
        }
    }

    /// <summary>
    /// Summary description for TableCell.
    /// </summary>
    /// ================================================================================
    //[Serializable]
    //[KnownType(typeof(Rectangle))]
    //[KnownType(typeof(BasicText))]
    //[KnownType(typeof(Column))]
    //[KnownType(typeof(Cell))]
    //[KnownType(typeof(List<Cell>))]
    public class Column //: ISerializable
    {
        public Table Table;                     // parent container

        public Column Default = null;           //template

        //const string COLUMNTITLERECT	= "ColumnTitleRect";
        //const string COLUMNCELLSRECT	= "ColumnCellsRect";
        //const string COLUMNRECT			= "ColumnRect";
        //const string TITLE				= "Title";
        //const string CELLS				= "Cells";
        //const string ISHIDDEN			= "IsHidden";

        public ushort InternalID = 0;

        public BasicText Title;
        public BorderPen ColumnTitlePen = new BorderPen();
        public Rectangle ColumnTitleRect;       // only column title

        // baseRect include also totals rect
        public Rectangle ColumnCellsRect;       // only column title and column ells
        public Rectangle ColumnRect;            // including column title, column cells, total
        public BorderPen ColumnPen = new BorderPen();

        public bool IsHidden = false;   // colonna nascosta

        public int SavedWidth = 0;		// dimensione della colonna prima di essere nascosta
        public int Width = 0;

        public bool ShowTotal = false;          // mostra il totale colonna
        public TotalCell TotalCell;

        public bool MultipleRow = false;		// se il testo non sta su una riga usa le righe successive

        public bool VMergeEmptyCell = false;	// In presenza di RowSep sulla tabella, non visualizza il bordo di separazione orizzontale se la riga successiva è vuota
        public bool VMergeEqualCell = false;	// Quando delle celle contigue verticalmente hanno lo stesso valore non visualizza il bordo di separazione orizzontale ed il valore nelle righe sucessive
        public string PreviousValue = null;
        public bool VMergeTailCell = false;     // se il testo non sta su una riga e c'e' un RowSep, non lo disegna per visualizzare che la cella come dato e' una sola

        public bool ShowAsBitmap = false;       // mostra il dato come immagine
        public bool ShowProportional = false;   // lo mostra in modo proporzionale
        public bool ShowNativeImageSize = false;   // lo mostra nella dimensione originale

        public BarCode BarCode = null;                  // mostra il dato come barcode
        public bool ShowAsBarCode { get { return BarCode != null; } }

        public string FormatStyleName = DefaultFormat.Testo;
        public string FontSyleName;

        public SubTotalCell SubTotal;

        public List<Cell> Cells;

        public bool IsTextFile = false;

        //attributes not used in Easylook, used only for Z-print in woorm c++. Here they are only parsed
        public bool IsFixed = false;
        public bool IsSplitter = false;
        public bool IsAnchorLeft = false;
        public bool IsAnchorRight = false;
        public bool IsHideWhenEmpty = false;
        public bool IsOptimizedWidth = false;

        public Expression HideExpr = null;  // espressione che se valutata vera nasconde la colonna
        public Expression WidthExpr = null;  // espressione che determina la larghezza della colonna

        //Estensione sintattica titolo colonne [TITLE BEGIN [TEXT = string-expr;][TEXTCOLOR = int-expr;] [BKGCOLOR = int-exp;]
        //                                                  [TOOLTIP = string-exp;] END]
        public WoormViewerExpression TitleExpr = null;           //dynamic column title text (string)
        public WoormViewerExpression TitleTextColorExpr = null;
        public WoormViewerExpression TitleBkgColorExpr = null;
        public WoormViewerExpression TitleTooltipExpr = null;
        //Estensione sintattica celle {ALIAS} [TEXTCOLOR = int-expr]  [BKGCOLOR = int-expr]
        //                                      [FONTSTYLE = string-expr] [BORDERS = string-expr] [TOOLTIP = string-exp]  {WIDTH}
        public WoormViewerExpression TextColorExpr = null;       //dynamic cell text color (RGB value)
        public WoormViewerExpression BkgColorExpr = null;        //dynamic cell background color 
        public WoormViewerExpression TextFontStyleExpr = null;   //dynamic cell Font Style name (string)
        public WoormViewerExpression FormatStyleExpr = null;     //dynamic cell Format Style name (string)
        public WoormViewerExpression CellBordersExpr = null;     //dynamic comma separated bordered edges: "Left, Right, Top, Bottom" shows a full bordered cell
        public WoormViewerExpression TooltipExpr = null;         //dynamic string expression

        //Estensione sintattica Totali celle [TOTAL BEGIN [TEXTCOLOR = int-expr]  [BKGCOLOR = int-expr] END]
        public WoormViewerExpression TotalTextColorExpr = null;
        public WoormViewerExpression TotalBkgColorExpr = null;
        //Estensione sintattica SubTotali celle [SUBTOTAL BEGIN [TEXTCOLOR = int-expr]  [BKGCOLOR = int-expr] END]
        public WoormViewerExpression SubTotalTextColorExpr = null;
        public WoormViewerExpression SubTotalBkgColorExpr = null;

        public string ClassName = string.Empty;     //Nome della classe dello stile
        public bool IsTemplate = false;             //Indica che gli attributi grafici di questo oggetto sono usati come template	

        public bool IsHtml = false;

        internal Rectangle CellRect(int row) { return Cells[row].RectCell; }
        internal Rectangle TotalRect() { return TotalCell.RectCell; }

        // Non è possibile rideterminare la size corretta di una colonna hidden perchè non ho l'informazione
        // del tipo della colonna in quanto in fase di parsing grafico non ho a disposizione l'EditorManager
        // che non ha ancora parsato la zona di report (variables)
        private const int HIDDEN_DEFAULT_WIDTH = 100;
        public bool TemplateOverridden = false;

        //------------------------------------------------------------------------------
        public Column()
        {
        }

        //------------------------------------------------------------------------------
        //public Column(SerializationInfo info, StreamingContext context)
        //{
        //	ColumnTitleRect = info.GetValue<Rectangle>(COLUMNTITLERECT);
        //	ColumnCellsRect = info.GetValue<Rectangle>(COLUMNCELLSRECT);
        //	ColumnRect = info.GetValue<Rectangle>(COLUMNRECT);
        //	Title = info.GetValue<BasicText>(TITLE);
        //	object[] arCells = info.GetValue<object[]>(CELLS);
        //	if (arCells != null)
        //	{
        //		Cells = new List<Cell>();
        //		foreach (object item in arCells)
        //			Cells.Add(item as Cell);
        //	}
        //	IsHidden = info.GetBoolean(ISHIDDEN);
        //}

        //------------------------------------------------------------------------------
        public Column
                    (
                        Table table,
                        Point columnOrigin,
                        Size titleSize,
                        Size cellSize,
                        int rows
                    )
        {
            Table = table;
            Title = new BasicText(Table.Document);
            SubTotal = new SubTotalCell(Table.Document);

            Title.TextColor = Defaults.DefaultColumnTitleForeground;
            Title.BkgColor = Defaults.DefaultColumnTitleBackground;
            Title.FontStyleName = DefaultFont.TitoloColonna;
            Title.Align = Defaults.DefaultColumnTitleAlign;

            Rectangle titleRect = new Rectangle(columnOrigin, titleSize);
            Point cellOrigin = new Point(columnOrigin.X, columnOrigin.Y + titleSize.Height);

            Cells = new List<Cell>();
            for (int i = 0; i < rows; i++)
            {
                Cells.Add(new Cell(this, cellOrigin, cellSize));
                cellOrigin.Y += cellSize.Height;
            }

            // la cella dei totali è sempre presente.
            TotalCell = new TotalCell(this, cellOrigin, cellSize);

            ColumnTitleRect = titleRect;
            ColumnCellsRect = new Rectangle(columnOrigin, new Size(titleSize.Width, titleSize.Height + rows * cellSize.Height));
            ColumnRect = new Rectangle(columnOrigin, new Size(titleSize.Width, ColumnCellsRect.Height + TotalCell.RectCell.Height));

            // il resto viene inizializzato dal parsing 
        }

        //------------------------------------------------------------------------------
        public Column(Column source)
        {
            this.Table = source.Table;
            this.ColumnTitleRect = source.ColumnTitleRect;
            this.ColumnCellsRect = source.ColumnCellsRect;
            this.ColumnRect = source.ColumnRect;
            this.InternalID = source.InternalID;
            this.TotalCell = null;
            this.ShowTotal = source.ShowTotal;
            this.ShowAsBitmap = source.ShowAsBitmap;
            this.ShowProportional = source.ShowProportional;
            this.ShowNativeImageSize = source.ShowNativeImageSize;
            this.BarCode = source.BarCode;
            this.IsTextFile = source.IsTextFile;
            this.IsHidden = source.IsHidden;
            this.FormatStyleName = source.FormatStyleName;
            this.FontSyleName = source.FontSyleName;
            this.MultipleRow = source.MultipleRow;
            this.Title = source.Title;
            this.ColumnTitlePen = source.ColumnTitlePen;
            this.ColumnPen = source.ColumnPen;
            this.SubTotal = source.SubTotal;
            this.Cells = source.Cells;
            this.IsTextFile = source.IsTextFile;
            this.IsHidden = source.IsHidden;
            this.SavedWidth = source.SavedWidth;
            this.Width = source.Width;
            this.SubTotal = source.SubTotal;
            this.IsFixed = source.IsFixed;
            this.IsHideWhenEmpty = source.IsHideWhenEmpty;
            this.IsOptimizedWidth = source.IsOptimizedWidth;

            //qui erano tutte "new Expression", sostituite dal rispettivo "Clone"
            this.HideExpr = source.HideExpr != null ? source.HideExpr.Clone() : null;
            this.WidthExpr = source.WidthExpr != null ? source.WidthExpr.Clone() : null;
            this.TitleExpr = source.TitleExpr != null ? source.TitleExpr.Clone() : null;
            this.TitleTextColorExpr = source.TitleTextColorExpr != null ? source.TitleTextColorExpr.Clone() : null;
            this.TitleBkgColorExpr = source.TitleBkgColorExpr != null ? source.TitleBkgColorExpr.Clone() : null;
            this.TextColorExpr = source.TextColorExpr != null ? source.TextColorExpr.Clone() : null;
            this.BkgColorExpr = source.BkgColorExpr != null ? source.BkgColorExpr.Clone() : null;
            this.TotalTextColorExpr = source.TotalTextColorExpr != null ? source.TotalTextColorExpr.Clone() : null;
            this.TotalBkgColorExpr = source.TotalBkgColorExpr != null ? source.TotalBkgColorExpr.Clone() : null;
            this.SubTotalTextColorExpr = source.SubTotalTextColorExpr != null ? source.SubTotalTextColorExpr.Clone() : null;
            this.SubTotalBkgColorExpr = source.SubTotalBkgColorExpr != null ? source.SubTotalBkgColorExpr.Clone() : null;
            this.TextFontStyleExpr = source.TextFontStyleExpr != null ? source.TextFontStyleExpr.Clone() : null;
            this.CellBordersExpr = source.CellBordersExpr != null ? source.CellBordersExpr.Clone() : null;
            this.TooltipExpr = source.TooltipExpr != null ? source.TooltipExpr.Clone() : null;  //TODOLUCA  TooltipExpr = CellTooltipExpr
            this.TitleTooltipExpr = source.TitleTooltipExpr != null ? source.TitleTooltipExpr.Clone() : null;
            this.FormatStyleExpr = source.FormatStyleExpr != null ? source.FormatStyleExpr.Clone() : null; //TODOLUCA FormatStyleExpr = CellFormatterExpr
            this.IsAnchorLeft = source.IsAnchorLeft;
            this.IsAnchorRight = source.IsAnchorRight;
            this.IsSplitter = source.IsSplitter;
            this.ClassName = source.ClassName;
            this.IsTemplate = source.IsTemplate;
            this.Default = source.Default;
            this.TemplateOverridden = source.TemplateOverridden;
            this.VMergeEmptyCell = source.VMergeEmptyCell;
            this.VMergeEqualCell = source.VMergeEqualCell;
            this.VMergeTailCell = source.VMergeTailCell;
            this.IsHideWhenEmpty = source.IsHideWhenEmpty;
            this.IsHtml = source.IsHtml;

            for (int i = 0; i <= source.Cells.Count; i++)
            {
                Cell source_cell = source.Cells[i];
                Cell pCell = new Cell(source_cell);
                pCell.column = this;
                Cells.Add(pCell);
            }

            if (source.TotalCell != null)
            {
                TotalCell = new TotalCell(source.TotalCell);
                TotalCell.column = this;
            }
        }

        //-------------------------------------------------------------------------------
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //	info.AddValue(COLUMNTITLERECT, ColumnTitleRect);
        //	info.AddValue(COLUMNCELLSRECT, ColumnCellsRect);
        //	info.AddValue(COLUMNRECT, ColumnRect);
        //	info.AddValue(TITLE, Title);
        //	info.AddValue(CELLS, Cells);
        //	info.AddValue(ISHIDDEN, IsHidden);
        //}

        //---------------------------------------------------------------------
        public string ToJsonTemplateHeader(bool firstColumn, int col)
        {
            int lastColumn = Table.LastVisibleColumn();
            bool showTitle = ((this.Table.HideTableTitle && Table.HideColumnsTitle) || Table.HideColumnsTitle);
            bool last = col == lastColumn;

            Borders borders = new Borders
                (
                    Table.Borders.ColumnTitle.Top && !showTitle,
                    firstColumn && Table.Borders.ColumnTitle.Left && !showTitle,
                    Table.Borders.Body.Top,
                    !showTitle &&
                    (
                    (last && Table.Borders.ColumnTitle.Right) ||
                    (!last && Table.Borders.ColumnSeparator && Table.Borders.ColumnTitleSeparator)
                    )
                );

            string s = "{" +

                this.InternalID.ToJson("id", "id") + ',' +

               (DynamicIsHidden ? this.IsHidden.ToJson("hidden") + ',' : "") +

                //this.ColumnRect.ToJson("rect") + ',' +
                this.Width.ToJson("width");

            if (!this.Table.HideColumnsTitle)
                s += ",\"title\":{" +
                            this.ColumnTitleRect.ToJson() + ',' +

                            this.TemplateTitleLocalizedText.ToJson("caption", false, true) + ',' +

                            borders.ToJson() + ',' +
                            this.ColumnTitlePen.ToJson("pen") + ',' +

                            this.Title.TextColor.ToJson("textcolor") + ',' +
                            this.Title.BkgColor.ToJson("bkgcolor") + ',' +
                            this.Title.Align.ToHtml_align() + ',' +
                            this.Title.FontData.ToJson() +
                    '}';

            s +=
                // (/*this.MultipleRow*/true   ? ',' + this.MultipleRow    .ToJson("value_is_multiline")   : "") +
                (/*this.IsHtml*/true ? ',' + this.IsHtml.ToJson("value_is_html") : "") +
                (/*this.ShowAsBitmap*/true ? ',' + this.ShowAsBitmap.ToJson("value_is_image") : "") +
                (/*this.ShowAsBarCode*/true ? ',' + this.ShowAsBarCode.ToJson("value_is_barcode") : "") +
                (this.ShowAsBarCode ? "," + this.BarCode.ToJson() : "");

            //s += (/*this.ShowTotal*/true ? ',' + this.ShowTotal.ToJson("show_total") : "");
            //s += (this.ShowTotal ? ',' + this.TotalCell.ToJsonTemplate() : "");

            s += '}';

            return s;
        }

        public string ToJsonHiddenData()
        {
            string s = "{" +

                this.InternalID.ToJson("id", "id") + ',' +

                true.ToJson("hidden") + '}';

            return s;
        }

        public string ToJsonDataHeader(bool firstColumn, int col)
        {
            int lastColumn = Table.LastVisibleColumn();
            bool showTitle = ((this.Table.HideTableTitle && Table.HideColumnsTitle) || Table.HideColumnsTitle);
            bool last = col == lastColumn;

            Borders borders = null;

            if (Table.HasDynamicHiddenColumns())
            {
                borders = new Borders
                    (
                        Table.Borders.ColumnTitle.Top && !showTitle,
                        firstColumn && Table.Borders.ColumnTitle.Left && !showTitle,
                        Table.Borders.Body.Top,
                        !showTitle &&
                        (
                        (last && Table.Borders.ColumnTitle.Right) ||
                        (!last && Table.Borders.ColumnSeparator && Table.Borders.ColumnTitleSeparator)
                        )
                    );
            }

            string s = "{" +

                this.InternalID.ToJson("id", "id") +

                ',' + this.DynamicIsHidden.ToJson("hidden") +

                ',' + this.DynamicWidth.ToJson("width") +

                (borders != null ? ',' + borders.ToJson() : "");

            if (!this.Table.HideColumnsTitle
                 &&
                 (this.TitleExpr != null || this.TitleTextColorExpr != null ||
                 this.TitleBkgColorExpr != null || this.TitleTooltipExpr != null)
                 )
            {
                string t =
                        (this.TitleExpr != null ? this.DynamicTitleLocalizedText.ToJson("caption", false, true) + ',' : "") +
                        (this.TitleTextColorExpr != null ? this.DynamicTitleTextColor.ToJson("textcolor") + ',' : "") +
                        (this.TitleBkgColorExpr != null ? this.DynamicTitleBkgColor.ToJson("bkgcolor") + ',' : "") +
                        (this.TitleTooltipExpr != null ? this.DynamicTitleTooltip.ToJson("tooltip") : "");

                t = t.TrimEnd(new char[] { ',' });

                s += ", \"title\":{" + t + "}";
            }

            //s += (this.ShowTotal ? ',' + this.ShowTotal.ToJson("show_total") : "");
            //s += (this.ShowTotal ? ',' + this.TotalCell.ToJsonData() : "");

            s += '}';
            return s;
        }

        //------------------------------------------------------------------------------
        public DataArray GetColumnData()
        {
            DataArray ar = new DataArray();

            for (int row = 0; row < this.Cells.Count; row++)
            {
                Cell cell = Cells[row];
                object v = cell.Value.RDEData;
                if (v == null) continue;

                ar.Add(v);
            }
            return ar;
        }

        //-------------------------------------------------------------------------------
        public int TemplateWidth
        {
            get
            {
                return this.Width;
            }
        }

        public int DynamicWidth
        {
            get
            {
                if (this.WidthExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = WidthExpr.Eval();

                    if (val != null && val.Valid)
                    {
                        int newWidth = (int)val.Data;
                        //TODO RSWEB apply dynamic column width
                        return newWidth;
                    }
                }
                return TemplateWidth;
            }
        }

        //-------------------------------------------------------------------------------
        public string TemplateTitleLocalizedText
        {
            get
            {
                return Table.Document.Localizer.Translate(Title.Text.Replace("\\n", "\n"));
            }
        }
        public string DynamicTitleLocalizedText
        {
            get
            {
                if (TitleExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = TitleExpr.Eval();
                    if (val != null && val.Valid)
                        return Table.Document.Localizer.Translate(val.Data.ToString());
                }

                return TemplateTitleLocalizedText;
            }
        }

        //-------------------------------------------------------------------------------
        public Color TemplateTitleTextColor
        {
            get
            {
                return Title.TextColor;
            }
        }

        public Color DynamicTitleTextColor
        {
            get
            {
                if (TitleTextColorExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = TitleTextColorExpr.Eval();

                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));

                }
                return TemplateTitleTextColor;
            }
        }

        //-------------------------------------------------------------------------------
        public Color TemplateTitleBkgColor
        {
            get
            {
                return Table.Transparent ? Color.FromArgb(0, 0, 0, 0) : Title.BkgColor;
            }
        }

        public Color DynamicTitleBkgColor
        {
            get
            {
                if (TitleBkgColorExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = TitleBkgColorExpr.Eval();
                    if (val != null && val.Valid)
                        return Color.FromArgb(255, Color.FromArgb((int)val.Data));

                }
                return TemplateTitleBkgColor;
            }
        }

        //-------------------------------------------------------------------------------
        public string DynamicTitleTooltip
        {
            get
            {
                if (TitleTooltipExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = TitleTooltipExpr.Eval();

                    if (val != null && val.Valid)
                        return (string)val.Data;
                }
                return string.Empty;
            }
        }

        //-------------------------------------------------------------------------------
        public bool DynamicIsHidden
        {
            get
            {
                if (HideExpr != null)
                {
                    Table.Document.SynchronizeSymbolTable();

                    Value val = HideExpr.Eval();
                    if (val != null && val.Valid)
                        return (bool)val.Data;
                }
                return IsHidden;
            }
        }
        //-------------------------------------------------------------------------------
        public bool HasDynamicAttributeOnRows
        {
            get
            {
                return
                   TextColorExpr != null ||
                   BkgColorExpr != null ||
                   TextFontStyleExpr != null ||
                   FormatStyleExpr != null ||
                   CellBordersExpr != null ||
                   SubTotalTextColorExpr != null ||
                   SubTotalBkgColorExpr != null;
            }
        }

        //-------------------------------------------------------------------------------
        public int RowNumber { get { return Cells.Count; } }
        public bool IsBarCode { get { return BarCode != null; } }

        // reinizializza le colonne sulla base del tipo di dato che ci andrà dentro
        // numerico o stringa determinato dal motore di report e passato al viewer nella
        // reportSession (file xml)
        //------------------------------------------------------------------------------
        internal void SetAttributes(string aDataType)
        {
            foreach (Cell cell in Cells)
            {
                cell.Value.DataType = aDataType;
                SubTotal.DataType = aDataType;

                if (ObjectHelper.IsLeftAligned(aDataType))
                {
                    cell.Value.FontStyleName = DefaultFont.CellaStringa;
                    cell.Value.Align = Defaults.DefaultCellStringAlign;

                    SubTotal.FontStyleName = DefaultFont.SubTotaleStringa;
                    SubTotal.Align = Defaults.DefaultTotalStringAlign;
                }
                else
                {
                    cell.Value.FontStyleName = DefaultFont.CellaNumerica;
                    cell.Value.Align = Defaults.DefaultCellNumAlign;

                    SubTotal.FontStyleName = DefaultFont.SubTotaleNumerico;
                    SubTotal.Align = Defaults.DefaultTotalNumAlign;
                }
            }

            TotalCell.Value.DataType = aDataType;
            if (ObjectHelper.IsLeftAligned(aDataType))
            {
                TotalCell.Value.FontStyleName = DefaultFont.TotaleStringa;
                TotalCell.Value.Align = Defaults.DefaultTotalStringAlign;
            }
            else
            {
                TotalCell.Value.FontStyleName = DefaultFont.TotaleNumerico;
                TotalCell.Value.Align = Defaults.DefaultTotalNumAlign;
            }
        }

        //------------------------------------------------------------------------------
        internal void ClearData()
        {
            foreach (Cell cell in Cells)
                cell.Value.Clear();

            TotalCell.Value.Clear();
        }

        //------------------------------------------------------------------------------
        internal void ResizeCells(int x, int y, int w, int h)
        {
            for (int row = 0; row < Cells.Count; row++)
            {
                Cells[row].RectCell = new Rectangle(x, y, w, h);
                y += h;
            }
        }

        // solo il Localizer deve disabilitare il controllo di sintassi della espressione di Hide
        //------------------------------------------------------------------------------
        public bool Parse(WoormParser lex)
        {
            int width;
            string title = "";

            if (lex.LookAhead(Token.TEXTSTRING) && !lex.ParseString(out title))
                return false;

            if (lex.Matched(Token.TITLE))
            {
                lex.ParseTag(Token.BEGIN);
                if (lex.Matched(Token.TEXT))
                {
                    lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                    TitleExpr = new WoormViewerExpression(Table.Document);
                    TitleExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                    TitleExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                    if (!TitleExpr.Compile(lex, CheckResultType.Match, "String"))
                    {
                        lex.SetError(WoormViewerStrings.BadTextExpression);
                        return false;
                    }

                    lex.ParseTag(Token.SEP);
                }
                if (lex.Matched(Token.TEXTCOLOR))
                {
                    lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                    TitleTextColorExpr = new WoormViewerExpression(Table.Document);
                    TitleTextColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                    TitleTextColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                    if (!TitleTextColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                    {
                        lex.SetError(WoormViewerStrings.BadColorExpression);
                        return false;
                    }

                    lex.ParseTag(Token.SEP);
                }
                if (lex.Matched(Token.BKGCOLOR))
                {
                    lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                    TitleBkgColorExpr = new WoormViewerExpression(Table.Document);
                    TitleBkgColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                    TitleBkgColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                    if (!TitleBkgColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                    {
                        lex.SetError(WoormViewerStrings.BadColorExpression);
                        return false;
                    }

                    lex.ParseTag(Token.SEP);
                }
                if (lex.Matched(Token.TOOLTIP))
                {
                    lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                    TitleTooltipExpr = new WoormViewerExpression(Table.Document);
                    TitleTooltipExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                    TitleTooltipExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                    if (!TitleTooltipExpr.Compile(lex, CheckResultType.Match, "String"))
                    {
                        lex.SetError(WoormViewerStrings.BadColorExpression);
                        return false;
                    }

                    lex.ParseTag(Token.SEP);
                }
                lex.ParseTag(Token.END);
            }

            if (!lex.ParseAlias(out InternalID))
                return false;

            if (lex.Matched(Token.STYLE))
            {
                if (!lex.ParseString(out ClassName))
                    return false;
            }
            if (lex.Matched(Token.TEMPLATE))
            {
                IsTemplate = true;
            }

            if (lex.Matched(Token.TEXTCOLOR))
            {
                lex.ParseTag(Token.ASSIGN);

                TextColorExpr = new WoormViewerExpression(Table.Document);
                TextColorExpr.StopTokens = new StopTokens(new Token[] { Token.BKGCOLOR, Token.FONTSTYLE, Token.BORDERS, Token.WIDTH, Token.TOOLTIP });
                TextColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                if (!TextColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                {
                    lex.SetError(WoormViewerStrings.BadColorExpression);
                    return false;
                }
            }

            if (lex.Matched(Token.BKGCOLOR))
            {
                lex.ParseTag(Token.ASSIGN);

                BkgColorExpr = new WoormViewerExpression(Table.Document);
                BkgColorExpr.StopTokens = new StopTokens(new Token[] { Token.FONTSTYLE, Token.BORDERS, Token.WIDTH, Token.TOOLTIP });
                BkgColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                if (!BkgColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                {
                    lex.SetError(WoormViewerStrings.BadColorExpression);
                    return false;
                }
            }

            if (lex.Matched(Token.FONTSTYLE))
            {
                lex.ParseTag(Token.ASSIGN);

                TextFontStyleExpr = new WoormViewerExpression(Table.Document);
                TextFontStyleExpr.StopTokens = new StopTokens(new Token[] { Token.BORDERS, Token.WIDTH, Token.TOOLTIP });
                TextFontStyleExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                if (!TextFontStyleExpr.Compile(lex, CheckResultType.Match, "String"))
                {
                    lex.SetError(WoormViewerStrings.BadFontStyleNameExpression);
                    return false;
                }
            }

            if (lex.Matched(Token.BORDERS))
            {
                lex.ParseTag(Token.ASSIGN);

                CellBordersExpr = new WoormViewerExpression(Table.Document);
                CellBordersExpr.StopTokens = new StopTokens(new Token[] { Token.WIDTH, Token.TOOLTIP });
                CellBordersExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                if (!CellBordersExpr.Compile(lex, CheckResultType.Match, "String"))
                {
                    lex.SetError(WoormViewerStrings.BadCellBordersExpression);
                    return false;
                }
            }

            if (lex.Matched(Token.TOOLTIP))
            {
                lex.ParseTag(Token.ASSIGN);

                TooltipExpr = new WoormViewerExpression(Table.Document);
                TooltipExpr.StopTokens = new StopTokens(new Token[] { Token.WIDTH });
                TooltipExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                if (!TooltipExpr.Compile(lex, CheckResultType.Match, "String"))
                {
                    lex.SetError(WoormViewerStrings.BadTooltipExpression);
                    return false;
                }
            }

            if (!lex.ParseWidth(out width))
                return false;
            Width = width;
            if (Width < 0) Width = 0;
            int nCurWidth = Width;
            SavedWidth = Width;
            if (Width == 0)
            {
                IsHidden = true;
                Width = SavedWidth = HIDDEN_DEFAULT_WIDTH;
            }
            if (lex.Matched(Token.COMMA))
            {
                WidthExpr = new Expression(Table.Document.ReportSession, Table.Document.SymbolTable);
                WidthExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;

                WidthExpr.StopTokens = new StopTokens(new Token[]
                            {
                            Token.FORMATSTYLE,
                            Token.HIDDEN, Token.SEP, Token.BREAK,
                            Token.TOTAL, Token.SUBTOTAL,
                            Token.BITMAP, Token.BARCODE,Token.FILE,
                            Token.VMERGE_EMPTY_CELL, Token.VMERGE_TAIL_CELL,
                            Token.COLUMN_FIXED, Token.COLUMN_SPLITTER,
                            Token.COLUMN_ANCHOR_LEFT, Token.COLUMN_ANCHOR_RIGHT,
                            Token.COLUMN_HIDE_WHEN_EMPTY, Token.COLUMN_OPTIMIZE_WIDTH
                            });

                if (!(WidthExpr.Compile(lex, CheckResultType.Match, "Int32")))
                {
                    lex.SetError(WoormViewerStrings.BadExpression);
                    return false;
                }
                if (!WidthExpr.hasField)
                {
                    nCurWidth = this.DynamicWidth;
                }
            }

            FormatStyleName = DefaultFormat.Testo; ;
            if (lex.Matched(Token.FORMATSTYLE))
            {
                if (lex.Matched(Token.ASSIGN))
                {
                    FormatStyleExpr = new WoormViewerExpression(Table.Document);
                    FormatStyleExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                    FormatStyleExpr.StopTokens = new StopTokens(
                        new Token[]
                        {
                            Token.HIDDEN, Token.SEP, Token.BREAK,
                            Token.TOTAL, Token.SUBTOTAL,
                            Token.BITMAP, Token.BARCODE,Token.FILE,
                            Token.VMERGE_EMPTY_CELL, Token.VMERGE_TAIL_CELL,
                            Token.COLUMN_FIXED, Token.COLUMN_SPLITTER,
                            Token.COLUMN_ANCHOR_LEFT, Token.COLUMN_ANCHOR_RIGHT,
                            Token.COLUMN_HIDE_WHEN_EMPTY, Token.COLUMN_OPTIMIZE_WIDTH
                        });
                    if (!FormatStyleExpr.Compile(lex, CheckResultType.Match, "String"))
                    {
                        lex.SetError(WoormViewerStrings.BadTooltipExpression);
                        return false;
                    }
                }
                else if (!lex.ParseFormat(out FormatStyleName, false))
                    return false;
            }

            Title.Text = title;

            // abilita il word break su celle troppo lunghe
            if (lex.LookAhead(Token.BREAK))
            {
                lex.SkipToken();
                MultipleRow = true;
            }
            // In presenza di RowSep sulla tabella, non visualizza il bordo di separazione orizzontale se la riga successiva è vuota
            if (lex.LookAhead(Token.VMERGE_EMPTY_CELL))
            {
                lex.SkipToken();
                VMergeEmptyCell = true;
            }
            if (lex.LookAhead(Token.VMERGE_EQUAL_CELL))
            {
                lex.SkipToken();
                VMergeEqualCell = true;
            }
            if (lex.LookAhead(Token.VMERGE_TAIL_CELL))
            {
                lex.SkipToken();
                VMergeTailCell = true;
            }
            if (lex.LookAhead(Token.HTML))
            {
                lex.SkipToken();
                IsHtml = true;
            }

            // abilita la visibilità del totale
            if (lex.LookAhead(Token.TOTAL))
            {
                lex.SkipToken();
                ShowTotal = true;

                if (lex.Matched(Token.BEGIN))
                {
                    if (lex.Matched(Token.TEXTCOLOR))
                    {
                        lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                        TotalTextColorExpr = new WoormViewerExpression(Table.Document);
                        TotalTextColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                        TotalTextColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                        if (!TotalTextColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                        {
                            lex.SetError(WoormViewerStrings.BadColorExpression);
                            return false;
                        }

                        lex.ParseTag(Token.SEP);
                    }
                    if (lex.Matched(Token.BKGCOLOR))
                    {
                        lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                        TotalBkgColorExpr = new WoormViewerExpression(Table.Document);
                        TotalBkgColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                        TotalBkgColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                        if (!TotalBkgColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                        {
                            lex.SetError(WoormViewerStrings.BadColorExpression);
                            return false;
                        }

                        lex.ParseTag(Token.SEP);
                    }
                    lex.ParseTag(Token.END);
                }
            }
            //colorazione dinamica subTotali
            if (lex.LookAhead(Token.SUBTOTAL))
            {
                lex.SkipToken();
                if (lex.Matched(Token.BEGIN))
                {
                    if (lex.Matched(Token.TEXTCOLOR))
                    {
                        lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                        SubTotalTextColorExpr = new WoormViewerExpression(Table.Document);
                        SubTotalTextColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                        SubTotalTextColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                        if (!SubTotalTextColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                        {
                            lex.SetError(WoormViewerStrings.BadColorExpression);
                            return false;
                        }

                        lex.ParseTag(Token.SEP);
                    }
                    if (lex.Matched(Token.BKGCOLOR))
                    {
                        lex.ParseTag(Token.ASSIGN); //qui non testo valore ritorno per evitare di andare avanti?

                        SubTotalBkgColorExpr = new WoormViewerExpression(Table.Document);
                        SubTotalBkgColorExpr.StopTokens = new StopTokens(new Token[] { Token.SEP });
                        SubTotalBkgColorExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;
                        if (!SubTotalBkgColorExpr.Compile(lex, CheckResultType.Match, "Int32"))
                        {
                            lex.SetError(WoormViewerStrings.BadColorExpression);
                            return false;
                        }

                        lex.ParseTag(Token.SEP);
                    }
                    lex.ParseTag(Token.END);
                }
            }

            // visualizza il grafico come immagine e non come dato stringa
            if (lex.Matched(Token.BITMAP))
            {
                ShowAsBitmap = true;

                if (lex.Matched(Token.PROPORTIONAL))
                {
                    ShowProportional = true;
                }
                else if (lex.Matched(Token.NATIVE))
                    ShowNativeImageSize = true;
            }

            // Considera il dato come il nome di un tipo di BARCODE
            if (lex.LookAhead(Token.BARCODE))
            {
                BarCode = new BarCode(Table.Document.ReportSession.PathFinder);
                lex.ParseBarCode(BarCode);
            }

            // Considera il dato come il nome di un file testo
            if (lex.LookAhead(Token.FILE))
            {
                lex.SkipToken();
                IsTextFile = true;
            }

            // Colonna nascosta ?
            if (lex.LookAhead(Token.HIDDEN))
            {
                lex.SkipToken();
                IsHidden = true;



                if (!lex.ParseWidth(out SavedWidth))
                    return false;
                // nel caso la width salvata sia sballata o non settata setto il valore di default
                if (SavedWidth <= 0)
                    SavedWidth = HIDDEN_DEFAULT_WIDTH;
                Width = SavedWidth;

                if (lex.Matched(Token.WHEN))
                {
                    HideExpr = new Expression(Table.Document.ReportSession, Table.Document.SymbolTable);
                    HideExpr.ForceSkipTypeChecking = Table.Document.ForLocalizer;

                    HideExpr.StopTokens = new StopTokens(new Token[]{   Token.COLUMN_FIXED, Token.COLUMN_SPLITTER,
                                                                        Token.COLUMN_ANCHOR_LEFT,   Token.COLUMN_ANCHOR_RIGHT,
                                                                        Token.COLUMN_HIDE_WHEN_EMPTY, Token.COLUMN_OPTIMIZE_WIDTH
                                                                    });

                    if (!(HideExpr.Compile(lex, CheckResultType.Match, "Boolean")))
                    {
                        lex.SetError(WoormViewerStrings.BadHiddenExpression);
                        return false;
                    }

                    // Nel caso di Localizer non posso valutare l'espresione perchè non ho la simbol table 
                    // valorizzata dal run delle AskDialog di default rimane visibile e posso tradurre tutto
                    if (Table.Document.ForLocalizer)
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

                nCurWidth = IsHidden ? 0 : SavedWidth;
            }
            else if (Width <= 0)
            {
                IsHidden = true;
                SavedWidth = HIDDEN_DEFAULT_WIDTH;
            }

            //Parsing Column Printer Attributes
            if (lex.Matched(Token.COLUMN_FIXED))
            {
                IsFixed = true;
            }

            if (lex.Matched(Token.COLUMN_SPLITTER))
            {
                IsSplitter = true;
            }

            if (lex.Matched(Token.COLUMN_ANCHOR_LEFT))
            {
                IsAnchorLeft = true;
            }

            if (lex.Matched(Token.COLUMN_ANCHOR_RIGHT))
            {
                IsAnchorRight = true;
            }

            if (lex.Matched(Token.COLUMN_HIDE_WHEN_EMPTY))
            {
                IsHideWhenEmpty = true;
            }

            if (lex.Matched(Token.COLUMN_OPTIMIZE_WIDTH))
            {
                IsOptimizedWidth = true;
            }

            if (!lex.ParseSep())
                return false;


            InitializeWidth(IsHidden ? 0 : nCurWidth);

            SetAttributes();
            return true;
        }

        //---------------------------------------------------------------------------
        void InitializeWidth(int width)
        {
            // TRUCCO importante, salva la larghezza nel  titolo
            ColumnTitleRect.X = 0;
            ColumnTitleRect.Width = width;
        }

        //---------------------------------------------------------------------------
        internal AlignType GetCellAlign(int row)
        {
            return Cells[row].Value.Align;
        }

        //---------------------------------------------------------------------------
        internal string GetCellFontName(int row)
        {
            return Cells[row].Value.FontStyleName;
        }

        // bisogna cambiare L'allinemento e ed il font per tutte le celle i totali ed i subtotali
        // tenendo conto anche conto del tipo di dato che è nella cella (numerico o stringa).
        // Il tipo lo chiedo alla sessione di RDE che lo riceve dal motore WoormEngine.
        //------------------------------------------------------------------------------
        internal void SetAttributes()
        {
            string dataType = this.Table.Document.RdeReader.GetVariableTypeFromId(InternalID);
            if (dataType == null)
                return;

            SetAttributes(dataType);
        }

        //------------------------------------------------------------------------------
        internal virtual void ClearStyle()
        {
            ColumnPen = Default != null ? Default.ColumnPen : new BorderPen(Defaults.DefaultColumnBorderColor);
            ColumnTitlePen = Default != null ? Default.ColumnTitlePen : new BorderPen(Defaults.DefaultColumnTitleBorderColor);
            TotalPen = (Default != null ? Default.TotalPen : new BorderPen());

            Title.TextColor = (Default != null ? Default.Title.TextColor : Defaults.DefaultColumnTitleForeground);
            Title.BkgColor = (Default != null ? Default.Title.BkgColor : Defaults.DefaultColumnTitleBackground);

            BaseRect.ClearTemplateFont(Table.Document, ref Title.FontStyleName, Default != null ? Default.Title.FontStyleName : null, DefaultFont.TitoloColonna);

            Title.Align = (Default != null ? Default.Title.Align : Defaults.DefaultAlign);

            string fnt_cell, fnt_subtotal, fnt_total = string.Empty;

            if (ObjectHelper.IsLeftAligned(this.GetDataType()))
            {
                fnt_cell = DefaultFont.CellaStringa;
                fnt_subtotal = DefaultFont.SubTotaleStringa;
                fnt_total = DefaultFont.TotaleStringa;
            }
            else
            {
                fnt_cell = DefaultFont.CellaNumerica;
                fnt_subtotal = DefaultFont.SubTotaleNumerico;
                fnt_total = DefaultFont.TotaleNumerico;
            }

            BaseRect.ClearTemplateFont(Table.Document, ref this.FontSyleName, Default != null ? Default.FontSyleName : null, fnt_cell);
            BaseRect.ClearTemplateFont(Table.Document, ref SubTotal.FontStyleName, Default != null ? Default.SubTotal.FontStyleName : null, fnt_subtotal);
            BaseRect.ClearTemplateFont(Table.Document, ref TotalCell.Value.FontStyleName, Default != null ? Default.TotalCell.Value.FontStyleName : null, fnt_total);

            Color[] colors = new Color[(int)ElementColor.MAX];
            if (Default != null)
                colors = Default.Cells[0].CellColor;
            else
            {
                colors[(int)ElementColor.VALUE] = Defaults.DefaultCellForeground;
                colors[(int)ElementColor.BACKGROUND] = Defaults.DefaultCellBackground;
            }
            colors[(int)ElementColor.BORDER] = ColumnPen.Color;
            SetColumnCellsColor(colors);
        }

        //------------------------------------------------------------------------------
        private void SetColumnCellsColor(Color[] colors)
        {
            ColumnPen.Color = colors[(int)ElementColor.BORDER];

            // set foregrond and background for all column m_Cells
            for (int nRow = 0; nRow < Cells.Count; nRow++)
                Cells[nRow].CellColor = colors;
        }

        //------------------------------------------------------------------------------
        internal virtual void RemoveStyle()
        {
            if (Default == null)
                return;

            if (ColumnPen == Default.ColumnPen)
                ColumnPen = new BorderPen(Defaults.DefaultColumnBorderColor);

            if (ColumnTitlePen == Default.ColumnTitlePen)
                ColumnTitlePen = new BorderPen(Defaults.DefaultColumnTitleBorderColor);

            if (TotalPen == Default.TotalPen)
                TotalPen = new BorderPen();

            if (Title.TextColor == Default.Title.TextColor)
                Title.TextColor = Defaults.DefaultColumnTitleForeground;

            if (Title.BkgColor == Default.Title.BkgColor)
                Title.BkgColor = Defaults.DefaultColumnTitleBackground;

            BaseRect.RemoveTemplateFont(Table.Document, ref Title.FontStyleName, Default.Title.FontStyleName, DefaultFont.TitoloColonna);

            if (Title.Align == Default.Title.Align)
                Title.Align = Defaults.DefaultAlign;

            string fnt_cell, fnt_subtotal, fnt_total = string.Empty;

            if (ObjectHelper.IsLeftAligned(this.GetDataType()))
            {
                fnt_cell = DefaultFont.CellaStringa;
                fnt_subtotal = DefaultFont.SubTotaleStringa;
                fnt_total = DefaultFont.TotaleStringa;
            }
            else
            {
                fnt_cell = DefaultFont.CellaNumerica;
                fnt_subtotal = DefaultFont.SubTotaleNumerico;
                fnt_total = DefaultFont.TotaleNumerico;
            }

            BaseRect.RemoveTemplateFont(Table.Document, ref FontSyleName, Default.FontSyleName, fnt_cell);
            BaseRect.RemoveTemplateFont(Table.Document, ref SubTotal.FontStyleName, Default.SubTotal.FontStyleName, fnt_subtotal);
            BaseRect.RemoveTemplateFont(Table.Document, ref TotalCell.Value.FontStyleName, Default.TotalCell.Value.FontStyleName, fnt_total);

            Color[] colors = new Color[(int)ElementColor.MAX];
            colors = Cells[0].CellColor;
            Color[] defaultColors = new Color[(int)ElementColor.MAX];
            defaultColors = Default.Cells[0].CellColor;

            if (colors[(int)ElementColor.VALUE] == defaultColors[(int)ElementColor.VALUE])
                colors[(int)ElementColor.VALUE] = Defaults.DefaultCellForeground;
            if (colors[(int)ElementColor.BACKGROUND] == defaultColors[(int)ElementColor.BACKGROUND])
                colors[(int)ElementColor.BACKGROUND] = Defaults.DefaultCellBackground;

            colors[(int)ElementColor.BORDER] = ColumnPen.Color;
            SetColumnCellsColor(colors);

            Default = null;
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        internal void SetStyle(Column templateCol)
        {
            RemoveStyle();

            if (templateCol == null)
                return;

            Default = templateCol;

            if (ColumnPen == new BorderPen())
                ColumnPen = templateCol.ColumnPen;

            if (ColumnTitlePen == new BorderPen())
                ColumnTitlePen = templateCol.ColumnTitlePen;

            if (TotalCell.TotalPen == new BorderPen())
                TotalCell.TotalPen = templateCol.TotalCell.TotalPen;

            if (Title.TextColor == Defaults.DefaultColumnTitleForeground)
                Title.TextColor = templateCol.Title.TextColor;

            if (Title.BkgColor == Defaults.DefaultColumnTitleBackground)
                Title.BkgColor = templateCol.Title.BkgColor;

            BaseRect.SetTemplateFont(Table.Document, ref Title.FontStyleName, templateCol.Title.FontStyleName, DefaultFont.TitoloColonna);

            if (Title.Align == Defaults.DefaultColumnTitleAlign)
                Title.Align = templateCol.Title.Align;

            string fontCell;
            string fontSubtotal;
            string fontTotal;

            string dataType = this.Table.Document.RdeReader.GetVariableTypeFromId(InternalID);
            if (ObjectHelper.IsLeftAligned(dataType))
            {
                fontSubtotal = DefaultFont.SubTotaleStringa;
                fontTotal = DefaultFont.TotaleStringa;
                fontCell = DefaultFont.CellaStringa;
            }
            else
            {
                fontSubtotal = DefaultFont.SubTotaleNumerico;
                fontTotal = DefaultFont.TotaleNumerico;
                fontCell = DefaultFont.CellaNumerica;
            }

            //se il FontStyleName di colonna e' nullo di default applico cuello di cella (non e' nullo se e' definito un font per tutte
            //le celle della colonna)
            if (string.IsNullOrWhiteSpace(FontSyleName))
                FontSyleName = fontCell;

            BaseRect.SubstituteTemplateFont(Table.Document, ref FontSyleName, templateCol.FontSyleName, fontCell);
            //propago il font a tutte le celle della colonna
            SetCellsFont(FontSyleName);

            BaseRect.SubstituteTemplateFont(Table.Document, ref SubTotal.FontStyleName, templateCol.SubTotal.FontStyleName, fontSubtotal);
            BaseRect.SubstituteTemplateFont(Table.Document, ref TotalCell.Value.FontStyleName, templateCol.TotalCell.Value.FontStyleName, fontTotal);

            //TODO/compromesso viene controllato solo il colore della prima cella e viene assegnato a tutte le celle come 
            //fa il renderizzatore C++
            if (Cells.Count > 0 && templateCol.Cells.Count > 0)
            {
                //propaga colore a tutte le celle se nel template c'e' un colore specificato 
                if (Cells[0].TemplateBkgColor == Color.FromArgb(255, 0, 0, 0))
                    SetCellsBkgColor(templateCol.Cells[0].TemplateBkgColor);

                if (Cells[0].TemplateTextColor == Color.FromArgb(255, 255, 255, 255))
                    SetCellsTextColor(templateCol.Cells[0].TemplateTextColor);
            }
        }

        /// <summary>
        /// Assegna il font a tutte le celle della colonna
        /// </summary>
        //------------------------------------------------------------------------------
        private void SetCellsFont(string fontSyleName)
        {
            foreach (Cell cell in Cells)
                cell.Value.FontStyleName = fontSyleName;
        }

        /// <summary>
        /// Assegna il colore del testo a tutte le celle della colonna
        /// </summary>
        //------------------------------------------------------------------------------
        private void SetCellsTextColor(Color textColor)
        {
            foreach (Cell cell in Cells)
                cell.Value.TextColor = textColor;
        }

        /// <summary>
        /// Assegna il colore dello sfondo a tutte le celle della colonna 
        /// </summary>
        //------------------------------------------------------------------------------
        private void SetCellsBkgColor(Color bkgColor)
        {
            foreach (Cell cell in Cells)
                cell.Value.BkgColor = bkgColor;
        }

        //------------------------------------------------------------------------------
        internal int CalculateFieldWidth(string strText)
        {
            string fontStyleName = Cells[0].Value.FontStyleName;
            if (string.IsNullOrEmpty(fontStyleName))
                return 0;

            SizeF size;

            //TODO RSWEB - SplitString
            //using (Form tempForm = new Form())                                               Non esiste graphics, lavoro di frontend
            //using (Graphics g = tempForm.CreateGraphics())
            //Bitmap myBitmap = new Bitmap(1980, 1080);
            //Graphics g = Graphics.FromImage(myBitmap);
            //{
            //    FontElement fe = this.Table.Document.GetFontElement(fontStyleName);
            //    using (Font font = new Font(fe.FaceName, (fe.Size * (float)72 / 100)))
            //    {
            //        size = g.MeasureString(strText, font);
            //    }
            //}

            int nCharWidth = (int)(size.Width / strText.Length + 0.5);
            int nWidth = this.DynamicWidth;
 
            return nCharWidth != 0
                ? Math.Max(1, (int)((nWidth - 2 * 2/*MARGIN_FROM_BORDER*/) / nCharWidth))
                : 1;
        }

        //------------------------------------------------------------------------------
        public bool GetFieldWidthFactors(ref FieldWidthFactors fwf, bool isSubTotal = false)
        {
            string fontStyleName = isSubTotal ? this.SubTotal.FontStyleName : Cells[0].Value.FontStyleName;

            if (string.IsNullOrEmpty(fontStyleName))
                return false;

            FontElement fe = this.Table.Document.GetFontElement(fontStyleName);
            //fwf.font = new Font(fe.FaceName, (fe.Size * (float)72 / 100));     TODO rsweb
            //	fieldWidthFactors.m_pFormatStyle = m_Table.m_pDocument.m_pFormatStyles.GetFormatter(m_nFormatIdx, &m_Table.m_pDocument.GetNamespace());

            int width = this.DynamicWidth;

            if (this.HideExpr != null || IsHidden)
            {
                Variable bHidden = new Variable("__hidden");
                bHidden.Data = true;
                if (this.MultipleRow && this.HideExpr != null)
                    HideExpr.Eval(ref bHidden);

                bool b = ObjectHelper.CastBool(bHidden.Data);
                width = b ? 32767 : this.SavedWidth;
            }
            //fwf.m_nWidth = Math.Max(1, (width - 2 * 2/*MARGIN_FROM_BORDER*/));   TODO rsweb
            return true;
        }

        //------------------------------------------------------------------------------
        public bool Unparse(Unparser unparser)
        {
            FormatStyles formatStyles = Table.Document.FormatStyles;

            bool hidden = IsHidden || ColumnTitleRect.Width <= 0 || HideExpr != null;

            if (Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteString(
                    unparser.IsLocalizableTextInCurrentLanguage()
                    ? unparser.LoadReportString(Title.Text)
                    : Title.Text, false);

                unparser.WriteBlank();
            }
            else
            {
                if (TitleExpr == null)
                    unparser.WriteString
                        (
                        unparser.IsLocalizableTextInCurrentLanguage()
                            ? unparser.LoadReportString(Title.Text)
                            : Title.Text, false
                        );

                unparser.WriteBlank();

                if (TitleExpr != null || TitleTextColorExpr != null || TitleBkgColorExpr != null || TitleTooltipExpr != null)
                {
                    unparser.WriteTag(Token.TITLE, true);
                    unparser.WriteBegin();

                    if (TitleExpr != null)
                    {
                        unparser.WriteTag(Token.TEXT, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TitleExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    if (TitleTextColorExpr != null)
                    {
                        unparser.WriteTag(Token.TEXTCOLOR, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TitleTextColorExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    if (TitleBkgColorExpr != null)
                    {
                        unparser.WriteTag(Token.BKGCOLOR, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TitleBkgColorExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    if (TitleTooltipExpr != null)
                    {
                        unparser.WriteTag(Token.TOOLTIP, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TitleTooltipExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    unparser.WriteEnd(true);
                }
            }
            unparser.WriteAlias(InternalID, false);
            unparser.Write(" /* " + GetFieldName() + " */ ");

            if (!ClassName.IsNullOrEmpty())
            {
                unparser.WriteTag(Token.STYLE, false);
                unparser.WriteString(ClassName, false);
            }
            if (IsTemplate)
                unparser.WriteTag(Token.TEMPLATE, false);

            if (TextColorExpr != null && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.TEXTCOLOR, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(TextColorExpr.ToString(), false);
            }
            if (BkgColorExpr != null && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.BKGCOLOR, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(BkgColorExpr.ToString(), false);
            }
            if (TextFontStyleExpr != null && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.FONTSTYLE, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(TextFontStyleExpr.ToString(), false);
            }
            if (CellBordersExpr != null && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.BORDERS, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(CellBordersExpr.ToString(), false);
            }

            if (TooltipExpr != null && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.TOOLTIP, false);
                unparser.WriteTag(Token.ASSIGN, false);

                unparser.WriteExpr(TooltipExpr.ToString(), false);
            }

            unparser.WriteWidth(hidden ? 0 : ColumnTitleRect.Width, false);

            if (FormatStyleExpr != null && !FormatStyleExpr.ToString().IsNullOrEmpty() && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.FORMATSTYLE, false);
                unparser.WriteTag(Token.ASSIGN, false);
                unparser.WriteExpr(FormatStyleExpr.ToString(), false);
            }
            else if (!FormatStyleName.IsNullOrEmpty())
                unparser.WriteFormat(FormatStyleName, false);

            if (MultipleRow)
                unparser.WriteTag(Token.BREAK, false);
            if (VMergeEmptyCell)
                unparser.WriteTag(Token.VMERGE_EMPTY_CELL, false);
            if (VMergeEqualCell)
                unparser.WriteTag(Token.VMERGE_EQUAL_CELL, false);
            if (VMergeTailCell)
                unparser.WriteTag(Token.VMERGE_TAIL_CELL, false);

            if (IsHtml)
                unparser.WriteTag(Token.HTML, false);

            if (ShowTotal)
            {
                unparser.WriteTag(Token.TOTAL, false);

                if ((TotalTextColorExpr != null || TotalBkgColorExpr != null) && !Table.Document.Template.IsSavingTemplate)
                {
                    unparser.WriteLine();
                    unparser.WriteBegin();

                    if (TotalTextColorExpr != null)
                    {
                        unparser.WriteTag(Token.TEXTCOLOR, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TotalTextColorExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    if (TotalBkgColorExpr != null)
                    {
                        unparser.WriteTag(Token.BKGCOLOR, false);
                        unparser.WriteTag(Token.ASSIGN, false);

                        unparser.WriteExpr(TotalBkgColorExpr.ToString(), false);
                        unparser.WriteSep(true);
                    }
                    unparser.WriteEnd();
                }
            }

            if ((SubTotalTextColorExpr != null || SubTotalBkgColorExpr != null) && !Table.Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.SUBTOTAL, true);
                unparser.WriteBegin();

                if (SubTotalTextColorExpr != null)
                {
                    unparser.WriteTag(Token.TEXTCOLOR, false);
                    unparser.WriteTag(Token.ASSIGN, false);
                    unparser.WriteExpr(SubTotalTextColorExpr.ToString(), false);
                    unparser.WriteSep(true);
                }
                if (SubTotalBkgColorExpr != null)
                {
                    unparser.WriteTag(Token.BKGCOLOR, false);
                    unparser.WriteTag(Token.ASSIGN, false);

                    unparser.WriteExpr(SubTotalBkgColorExpr.ToString(), false);
                    unparser.WriteSep(true);
                }
                unparser.WriteEnd();
            }

            if (ShowAsBitmap)
            {
                unparser.WriteTag(Token.BITMAP, false);

                if (ShowProportional)
                    unparser.WriteTag(Token.PROPORTIONAL, false);
                else if (ShowNativeImageSize)
                    unparser.WriteTag(Token.NATIVE, false);
            }
            else if (IsBarCode)
                BarCode.Unparse(unparser, false);
            else if (IsTextFile)
                unparser.WriteTag(Token.FILE, false);

            if (hidden)
            {
                unparser.WriteTag(Token.HIDDEN, false);
                unparser.WriteWidth(hidden ? SavedWidth : ColumnTitleRect.Width, false);
                if (HideExpr != null && !Table.Document.Template.IsSavingTemplate)
                {
                    unparser.WriteTag(Token.WHEN, false);

                    if (Table.Document.ReplaceHiddenWhenExpr)
                        unparser.WriteTag(hidden ? Token.TRUE : Token.FALSE);
                    else
                        unparser.WriteExpr(HideExpr.ToString(), false);
                }
            }

            if (IsFixed)
                unparser.WriteTag(Token.COLUMN_FIXED, false);

            if (IsSplitter)
                unparser.WriteTag(Token.COLUMN_SPLITTER, false);

            if (IsAnchorLeft)
                unparser.WriteTag(Token.COLUMN_ANCHOR_LEFT, false);

            if (IsAnchorRight)
                unparser.WriteTag(Token.COLUMN_ANCHOR_RIGHT, false);

            if (IsHideWhenEmpty)
                unparser.WriteTag(Token.COLUMN_HIDE_WHEN_EMPTY, false);

            if (IsOptimizedWidth)
                unparser.WriteTag(Token.COLUMN_OPTIMIZE_WIDTH, false);

            unparser.WriteSep();
            return true;
        }

        //---------------------------------------------------------------------------
        private string GetFieldName()
        {
            Variable v = Table.Document.SymbolTable.FindById(InternalID);
            if (v != null)
                return v.Name;

            return "<UNKNOWN COLUMN FIELD>";
        }

        //---------------------------------------------------------------------------
        internal AlignType TotalAlign { get { return TotalCell.Value.Align; } }
        internal string TotalFontName { get { return TotalCell.Value.FontStyleName; } }
        internal BorderPen TotalPen { get { return TotalCell.TotalPen; } set { TotalCell.TotalPen = value; } }
        internal Color[] TotalColor { get { return TotalCell.TotalColor; } }
        internal string SubTotalFontName { get { return SubTotal.FontStyleName; } }
        internal AlignType ColumnTitleAlign { get { return Title.Align; } }
        internal string ColumnTitleFontName { get { return Title.FontStyleName; } }
        internal int LastRow { get { return Cells.Count - 1; } }

        //---------------------------------------------------------------------------
        internal Color[] SubTotalColor
        {
            get
            {
                Color[] colors = new Color[(int)ElementColor.MAX];
                colors[(int)ElementColor.VALUE] = SubTotal.TextColor;
                colors[(int)ElementColor.BACKGROUND] = SubTotal.BkgColor;
                return colors;
            }
        }

        //---------------------------------------------------------------------------
        internal Color[] ColumnTitleColor
        {
            get
            {
                Color[] colors = new Color[(int)ElementColor.MAX];
                colors[(int)ElementColor.LABEL] = Title.TextColor;
                colors[(int)ElementColor.BACKGROUND] = Title.BkgColor;
                colors[(int)ElementColor.BORDER] = ColumnTitlePen.Color;
                return colors;
            }
        }

        //---------------------------------------------------------------------------
        internal Color[] GetCellColor(int row)
        {
            Color[] colors = new Color[(int)ElementColor.MAX];
            colors = Cells[row].CellColor;
            colors[(int)ElementColor.BORDER] = ColumnPen.Color;
            return colors;
        }

        //---------------------------------------------------------------------------
        internal string GetDataType()
        {
            Variable variable = Table.Document.SymbolTable.FindById(InternalID);
            if (variable != null)
                return variable.WoormType;

            return string.Empty;
        }

        //---------------------------------------------------------------------------
        internal bool RemoveAnchoredField(BaseRect baseRect)
        {
            //TODOLUCA
            //for (int i = 0; i < m_arAnchoredFields.GetSize(); i++)
            //{
            //    if (m_arAnchoredFields.GetAt(i) == baseRect)
            //    {
            //        m_arAnchoredFields.RemoveAt(i);
            //        return true;
            //    }
            //}
            return false;
        }

        //---------------------------------------------------------------------------
        internal void HMoveColumn(int width)
        {
            int oldRight = ColumnTitleRect.Right;

            ColumnTitleRect.Offset(width, 0);
            ColumnCellsRect.Offset(width, 0);
            ColumnRect.Offset(width, 0);

            // move m_Cells horizontal
            for (int nRow = 0; nRow <= LastRow; nRow++)
                Cells[nRow].HMoveCell(width);

            // move m_Cells horizontal
            TotalCell.HMoveCell(width);

            //if (this.IsSplitter)
            //    UpdateSplitterInfo(oldRight);

            //TODOLUCA
            //if (m_arAnchoredFields.GetSize() > 0)
            //    MoveAnchoredFields(width);
        }
    }

    //================================================================================
    //[Serializable]
    //[KnownType(typeof(Column))]
    //[KnownType(typeof(List<Column>))]
    public class Table : BaseObj
    {
        //public Rectangle BaseCellsRect { get { return base.Rect; } set { base.Rect = value; } } 	// include table title, columns titles and all cells
        public Rectangle BaseCellsRect;

        public Rectangle TitleRect;
        public TableBorders Borders = new TableBorders();

        public bool HideTableTitle = false;
        public bool HideColumnsTitle = false;

        public BorderPen TitlePen = new BorderPen();
        public BasicText Title = null;

        public List<Column> Columns;
        public bool[] Interlines;

        public bool FiscalEnd = false;  // abilita la tracciatura  di una z a fine tabella

        public bool Easyview = false;
        public bool EasyviewDynamic = false;
        public bool UseEasyviewColor = false;
        public Color EasyviewColor = Defaults.AlternateColor;   //Defaults.DefaultEasyviewColor;
        public ArrayList EasyViewDynamicOnPage = new ArrayList(); //memorizza con quale colore di sfondo deve iniziare la riga in caso di easyview dynamic (per mantenere coerenza sul cambio pagina)

        public int CurrentRow = 0; // riga dove viene valorizzata la cella quando leggo da RDE
        public int ViewCurrentRow = -1; // riga corrente in fase di renderizzazione (per attributi dinamici)

        public int Layer = 0;   //only design mode

        Table Default = null;
        //const string COLUMNS = "Columns";
        // private static data
        private const int CELL_HEIGHT = 14;
        private const int CELL_WIDTH = 60;
        private const int COLUMN_TITLE_HEIGHT = 14;
        private const int TITLE_HEIGHT = 14;

        public int ColumnNumber { get { return Columns.Count; } }
        public string LocalizedText { get { return Document.Localizer.Translate(Title.Text); } }
        public int RowNumber { get { return Columns[0].RowNumber; } }
        public int LastRow { get { return Columns[0].LastRow; } }

        private Rectangle ColumnTitleRect(int col) { return Columns[col].ColumnTitleRect; }
        private Rectangle CellRect(int row, int col) { return Columns[col].CellRect(row); }
        private Rectangle TotalRect(int col) { return Columns[col].TotalRect(); }

        private bool existsCellTail = false;
        public bool TemplateOverridden = false;

        //------------------------------------------------------------------------------
        public Table()
            : this(null, 0, 0)
        {

        }

        //---------------------------------------------------------------------------
        //public Table(SerializationInfo info, StreamingContext context)
        //	: base(info, context)
        //{
        //	object[] arCols = info.GetValue<object[]>(COLUMNS);
        //	if (arCols != null)
        //	{
        //		Columns = new List<Column>();
        //		foreach (object item in arCols)
        //			Columns.Add(item as Column);	
        //	}
        //}

        //------------------------------------------------------------------------------
        public Table(WoormDocument document, int rows, int cols)
            : base(document)
        {
            Title = new BasicText(document)
            {
                TextColor = Defaults.DefaultTableTitleForeground,
                BkgColor = Defaults.DefaultTableTitleBackground,
                FontStyleName = DefaultFont.TitoloTabella,
                Align = Defaults.DefaultAlign
            };
            // dimensione di default per celle e titolo
            Size defaultCell = new Size(CELL_WIDTH, CELL_HEIGHT);
            Size defaultColumnTitle = new Size(defaultCell.Width, COLUMN_TITLE_HEIGHT);

            // origine e dimensione iniziale
            Point origin = new Point(0, TITLE_HEIGHT);
            int width = 0;

            // creo le colonne
            Columns = new List<Column>();
            for (int i = 0; i < cols; i++)
            {
                Columns.Add(new Column(this, origin, defaultColumnTitle, defaultCell, rows));

                // aggiorna l'origine e la dimensione globale della tabella
                origin.X += Columns[i].ColumnRect.Width;
                width += Columns[i].ColumnRect.Width;
            }

            // creo i flags di interlinea
            Interlines = new bool[rows];
            ClearInterlines();

            // modifico i valori dei rettangoli (la dimensione della colonna e' gia' ok)
            // posso usare la posizione 0 perche' tutte le colonne hanno un totale anche se non mostrato
            TitleRect = new Rectangle(0, 0, width, TITLE_HEIGHT);
            BaseCellsRect = new Rectangle(0, 0, width, TITLE_HEIGHT + (cols == 0 ? 0 : Columns[0].ColumnCellsRect.Height));
            Rect = new Rectangle(0, 0, width, TITLE_HEIGHT + (cols == 0 ? 0 : Columns[0].ColumnRect.Height));
        }

        //---------------------------------------------------------------------------
        public Table(Table source)
        : base(source)
        {
            this.BaseCellsRect = source.BaseCellsRect;
            this.TitleRect = source.TitleRect;
            this.Borders = source.Borders;
            this.HideTableTitle = source.HideTableTitle;
            this.HideColumnsTitle = source.HideColumnsTitle;
            this.Easyview = source.Easyview;
            this.EasyviewColor = source.EasyviewColor;

            this.DropShadowHeight = source.DropShadowHeight;
            this.DropShadowColor = source.DropShadowColor;
            this.FiscalEnd = source.FiscalEnd;
            this.TitlePen = source.TitlePen;
            this.Title = source.Title;
            this.ViewCurrentRow = source.ViewCurrentRow;

            //LastTitleHeight = source.LastTitleHeight;
            //m_nActiveColumn			(source.m_nActiveColumn),
            //m_nActiveRow			(source.m_nActiveRow),
            //m_nCurrentRow			(source.m_nCurrentRow),
            //m_ptCellOrigin			(source.m_ptCellOrigin),
            //m_nTotalCounter			(source.m_nTotalCounter),
            //m_nAction				(source.m_nAction),
            //m_bAlternateEasyview	(source.m_bAlternateEasyview),

            this.ClassName = source.ClassName;
            this.IsTemplate = source.IsTemplate;
            this.Default = source.Default;
            this.HideExpr = source.HideExpr != null ? source.HideExpr.Clone() : null;
            this.TemplateOverridden = source.TemplateOverridden;
            this.EasyviewDynamic = source.EasyviewDynamic;
            this.InternalID = source.InternalID;

            for (int i = 0; i < source.Columns.Count; i++)
            {
                Column pSrcColumn = source.Columns[i];
                Column pColumn = new Column(pSrcColumn);
                pColumn.Table = this;   //CHANGE OWNER TABLE
                Columns.Add(pColumn);
            }
            source.Interlines.CopyTo(this.Interlines, 0);
            //m_arAlternateEasyviewOnPage.Copy(source.m_arAlternateEasyviewOnPage);
        }

        //---------------------------------------------------------------------------
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //	base.GetObjectData(info, context);

        //	info.AddValue(COLUMNS, Columns);
        //}

        //------------------------------------------------------------------------------
        override public string ToJsonTemplate(bool bracket)
        {
            string name = "table";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            int rows = this.RowNumber;
            if (this.ExistsTotals())
                rows++;

            int nc = this.VisibleColumnNumber();

            s += '{' +
                base.ToJsonTemplate(false) + ',' +

                nc.ToJson("column_number") + ',' +
                rows.ToJson("row_number") + ',' +

                this.Columns[0].Cells[0].RectCell.Height.ToJson("row_height") + ',' +

                (!this.HideTableTitle ? (
                    "\"title\":{" +
                        this.TitleRect.ToJson("rect") + ',' +

                        this.LocalizedText.ToJson("caption", false, true) + ',' +
                        this.Title.FontData.ToJson() + ',' +
                        this.Title.Align.ToHtml_align() + ',' +
                        this.Title.TextColor.ToJson("textcolor") + ',' +
                        this.Title.BkgColor.ToJson("bkgcolor") + ',' +

                        this.Borders.TableTitle.ToJson() + ',' +
                        this.TitlePen.ToJson() +
                       "},"
                     ) : "")
                    +

               this.HideColumnsTitle.ToJson("hide_columns_title") + ',' +
               this.FiscalEnd.ToJson("fiscal_end") + ',';
            //this.EasyviewColor.ToJson("alternate_color") + ',' +

            s += ToJsonColumns(true) + ',';
            s += ToJsonRowsTemplate() + '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        override public string ToJsonData(bool bracket)
        {
            string name = "table";

            string s = string.Empty;
            if (!name.IsNullOrEmpty())
                s = '\"' + name + "\":";

            s += '{' +
                base.ToJsonData(false) + ',' +

                ToJsonColumns(false) + ',' +

                ToJsonRowsData() +
            '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //---------------------------------------------------------------------
        public string ToJsonColumns(bool template)
        {
            string s = "\"columns\":[";
            bool first = true;
            for (int i = 0; i < Columns.Count; i++)
            {
                Column column = this.Columns[i];

                if (column.IsHidden && column.HideExpr == null)
                    continue;

                if (first) first = false; else s += ',';

                if (column.DynamicIsHidden && column.HideExpr != null)
                {
                    s += column.ToJsonHiddenData();
                    continue;
                }
                s += template ?
                    column.ToJsonTemplateHeader(i == 0, i)
                    :
                    column.ToJsonDataHeader(i == 0, i);
            }
            s += ']';

            return s;
        }

        //---------------------------------------------------------------------
        public string ToJsonRowsTemplate()
        {
            string s = "\"rows\":[";

            //predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
            this.InitEasyview();

            int lastColumn = this.ColumnNumber - 1; // LastVisibleColumn();

            for (int row = 0; row < this.RowNumber; row++)
            {
                string r = "[";

                bool firstCol = true;
                for (int col = 0; col <= lastColumn; col++)
                {
                    Column column = this.Columns[col];

                    if (row == 0)
                        column.PreviousValue = null;

                    if (column.IsHidden && column.HideExpr == null)
                        continue;

                    Cell cell = column.Cells[row];
                    cell.AtRowNumber = row;

                    bool lastCol = (col == lastColumn);

                    Borders borders = new Borders
                                            (
                                                false,
                                                firstCol && this.Borders.Body.Left,
                                                this.HasBottomBorderAtCell(cell),
                                                (!lastCol && this.Borders.ColumnSeparator) || (lastCol && this.Borders.Body.Right)
                                            );

                    if (!firstCol) r += ',';

                    r += cell.ToJsonTemplate(borders, UseColorEasyview(row));

                    firstCol = false;
                }

                r += "]";
                if (row < (this.RowNumber - 1)) r += ',';
                s += r;

                this.EasyViewNextRow(row);
            }

            s += ToJsonTotals(true); //when totals exists, it add a row with totals

            return s + "]";
        }

        public string ToJsonRowsData()
        {
            string s = "\"rows\":[";

            //predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
            this.InitEasyview();

            int lastColumn = this.LastVisibleColumn();

            for (int row = 0; row < this.RowNumber; row++)
            {
                string r = "[";

                bool firstCol = true;
                for (int col = 0; col <= lastColumn; col++)
                {
                    Column column = this.Columns[col];

                    if (row == 0)
                        column.PreviousValue = null;

                    if (column.DynamicIsHidden)
                        continue;

                    Cell cell = column.Cells[row];
                    cell.AtRowNumber = row;

                    bool lastCol = (col == lastColumn);

                    Borders borders = new Borders
                                             (
                                                 false,
                                                 firstCol && this.Borders.Body.Left,
                                                 this.HasBottomBorderAtCell(cell),
                                                 (!lastCol && this.Borders.ColumnSeparator) || (lastCol && this.Borders.Body.Right)
                                             );

                    //if (this.FiscalEnd && row >= this.CurrentRow)
                    //{
                    //    fore = Color.FromArgb(255, 112, 128, 144);
                    //    bkg = Color.FromArgb(255, 105, 105, 105);
                    //}

                    if (cell.SubTotal)
                    {
                        column.PreviousValue = null;
                    }

                    if (!firstCol)
                    {
                        r += ',';
                    }

                    r += cell.ToJsonData(borders, UseColorEasyview(row));

                    firstCol = false;
                }
                r += "]";

                if (row < (this.RowNumber - 1))
                {
                    r += ',';
                }

                s += r;

                this.EasyViewNextRow(row);
            }

            s += ToJsonTotals(false); //when totals exists, it add a row with totals

            s += "]";
            return s;
        }

        //---------------------------------------------------------------------
        public string ToJsonTotals(bool template)
        {
            if (!this.ExistsTotals())
                return string.Empty;

            int lastColumn = template ? this.ColumnNumber - 1 : this.LastVisibleColumn();

            string r = ",[";
            bool first = true;
            for (int col = 0; col <= lastColumn; col++)
            {
                Column column = this.Columns[col];

                if (template)
                {
                    if (column.IsHidden && column.HideExpr == null)
                        continue;
                }
                else
                {
                    if (column.IsHidden)
                        continue;
                }

                TotalCell total = column.TotalCell;

                bool last = (col == this.ColumnNumber - 1);

                int nextVisibleColumn = this.NextVisibleColumn(col);

                bool nextColumnHasTotal =
                        (
                            (col < lastColumn) &&
                            (nextVisibleColumn >= 0) &&
                            this.HasTotal(nextVisibleColumn)
                        );

                BorderPen pen = total.TotalPen;
                Borders borders;
                if (column.ShowTotal)
                {
                    borders = new Borders
                                    (
                                        false,
                                        first,
                                        this.Borders.Total.Bottom,
                                        this.Borders.Total.Right
                                    );
                }
                else
                {   // serve per scrivere il bordo del successivo totale
                    borders = new Borders
                                    (
                                        false,
                                        false,
                                        false,
                                        !last && nextColumnHasTotal && this.Borders.Total.Left
                                    );
                    //disegno il bordo sx del prossimo totale con il suo Pen e non con quello della cella corrente
                    //(allinemento con comportamento di woorm c++)
                    if (col + 1 <= lastColumn)
                    {
                        Column nextColumn = this.Columns[col + 1];
                        pen = nextColumn.TotalCell.TotalPen;
                    }
                }

                if (first) first = false; else r += ',';

                r += template ?
                        column.TotalCell.ToJsonTemplate(borders, pen)
                        :
                        column.TotalCell.ToJsonData(borders, pen);
            }
            r += ']';
            return r;
        }

        //---------------------------------------------------------------------------
        public bool ExistsColumnWithDynamicAttributeOnRow()
        {
            for (int i = 0; i < ColumnNumber; i++)
                if (Columns[i].HasDynamicAttributeOnRows)
                    return true;

            return false;
        }

        //---------------------------------------------------------------------------
        public bool HasDynamicHiddenColumns()
        {
            for (int i = 0; i < ColumnNumber; i++)
                if (Columns[i].HideExpr != null)
                    return true;

            return false;
        }
        public bool HasDynamicBorders()
        {
            for (int i = 0; i < ColumnNumber; i++)
                if (Columns[i].CellBordersExpr != null)
                    return true;

            return false;
        }

        //---------------------------------------------------------------------------
        public bool ExistsTotals()
        {
            for (int i = 0; i < ColumnNumber; i++)
                if (Columns[i].ShowTotal)
                    return true;

            return false;
        }

        //---------------------------------------------------------------------------
        public int NextVisibleColumn(int col)
        {
            for (int i = col + 1; i < ColumnNumber; i++)
                if (!Columns[i].IsHidden)
                    return i;

            return -1;
        }

        //---------------------------------------------------------------------------
        public int LastVisibleColumn()
        {
            for (int col = ColumnNumber - 1; col >= 0; col--)
            {
                if (!Columns[col].IsHidden)
                    return col;
            }

            return 0;
        }

        //---------------------------------------------------------------------------
        public int VisibleColumnNumber()
        {
            int count = ColumnNumber;
            for (int col = 0; col < ColumnNumber; col++)
            {
                if (Columns[col].IsHidden && Columns[col].HideExpr == null)
                    count--;
            }
            return count;
        }

        //---------------------------------------------------------------------------
        public bool HasTotal(int col)
        {
            return Columns[col].ShowTotal;
        }

        //---------------------------------------------------------------------------
        public int ColumnIndexFromID(int id)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].InternalID == id)
                    return i;
            }
            return -1;
        }

        //------------------------------------------------------------------------------
        public override void ClearData()
        {
            CurrentRow = 0;
            foreach (Column col in Columns)
                col.ClearData();

            ClearInterlines();
        }

        //------------------------------------------------------------------------------
        public void ClearInterlines()
        {
            for (int row = 0; row < Interlines.Length; row++) Interlines[row] = false;
        }


        // si assume che tutti le dimensioni dei titoli delle colonne siano stati
        // già correttamente settari e tutte le righe hanno la stessa altezza
        //
        //  occore ricostruire:
        //
        //      cell.CellRect;
        //      total.Cellrect;
        //
        //      column.ColumnTitleRect; // solo il titolo
        //      column.ColumnCellsRect; // titolo e tutte le celle
        //      column.ColumnRect;      // titolo,celle e totale
        //
        //      table.BaseRect
        //      table.BaseCellsRect;    // include table title, columns titles and all cells
        //      table.TitleRect;
        //
        //------------------------------------------------------------------------------
        private void RebuildTableSizes
            (
            Point origin,
            int tableTitleHeight,
            int columnTitleHeight,
            int rowHeight,
            int totalHeight
            )
        {
            // calculate table title width and make It
            int row = 0;
            for (int i = 0; i < ColumnNumber; i++)
                row += Columns[i].ColumnTitleRect.Width;

            Rect = new Rectangle
                (
                origin,
                new Size(row, tableTitleHeight + columnTitleHeight + rowHeight * RowNumber + totalHeight)
                );
            BaseCellsRect = new Rectangle
                (
                origin,
                new Size(row, tableTitleHeight + columnTitleHeight + rowHeight * RowNumber)
                );
            TitleRect = new Rectangle(origin, new Size(row, tableTitleHeight));

            // inital value;
            int x = TitleRect.Left;
            int y = TitleRect.Bottom;
            int h = columnTitleHeight;

            // update Columns
            for (int col = 0; col < ColumnNumber; col++)
            {
                Column column = Columns[col];
                int w = column.ColumnTitleRect.Width;

                column.ColumnTitleRect = new Rectangle(x, y, w, h);
                column.ColumnCellsRect = new Rectangle(x, y, w, h + RowNumber * rowHeight);
                column.ColumnRect = new Rectangle(x, y, w, h + RowNumber * rowHeight + totalHeight);

                column.ResizeCells(x, y + h, w, rowHeight);
                column.TotalCell.RectCell = new Rectangle(x, y + h + RowNumber * rowHeight, w, totalHeight);
                x += w;
            }
        }

        //------------------------------------------------------------------------------
        public override bool Parse(WoormParser lex)
        {
            Point origin;
            int tableTitleHeight;
            int columnTitleHeight;
            int rowHeight;
            int totalHeight;
            string title;

            if (!(
                lex.ParseString(out title) &&
                lex.ParseAlias(out InternalID) &&
                lex.ParseOrigin(out origin) &&
                lex.ParseHeights
                (
                out tableTitleHeight,
                out columnTitleHeight,
                out rowHeight,
                out totalHeight
                )
                ))
                return false;

            if (lex.Matched(Token.LAYER))
            {
                if (!lex.ParseInt(out this.Layer))
                    return false;
            }

            Title.Text = title;

            // parse table title and borders                         
            if (!ParseTitleAndBorders(lex))
                return false;

            // parse all Columns
            if (!ParseColumnsBlock(lex))
                return false;

            //Analogo a Woorm
            if (this.HideTableTitle)
                origin.Y += tableTitleHeight;
            /*if (!this.HideColumnsTitle)
                origin.Y += columnTitleHeight;*/

            // ricostruisce il layout della tabella
            RebuildTableSizes
                (
                origin,
                tableTitleHeight,
                columnTitleHeight,
                rowHeight,
                totalHeight
                );


            return true;
        }


        //------------------------------------------------------------------------------
        private bool ParseAllColumns(WoormParser lex)
        {
            for (int col = 0; col < ColumnNumber; col++)
            {
                if (!(Columns[col].Parse(lex)))
                    return false;
            }
            return true;
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnsBlock(WoormParser lex)
        {
            return
                lex.ParseBegin() &&
                ParseAllColumns(lex) &&
                ParseTableAttrib(lex) &&
                lex.ParseEnd();
        }

        //------------------------------------------------------------------------------
        private bool ParseTitleAndBorders(WoormParser lex)
        {
            // parse general table info not ever present
            for (; ; ) switch (lex.LookAhead())
                {
                    case Token.HIDE_TABLE_TITLE:
                        lex.SkipToken();
                        HideTableTitle = true;
                        break;

                    case Token.HIDE_ALL_TABLE_TITLE:
                        lex.SkipToken();
                        HideTableTitle = true;
                        HideColumnsTitle = true;
                        break;

                    case Token.FISCAL_END:
                        lex.SkipToken();
                        FiscalEnd = true;
                        break;

                    case Token.TRANSPARENT:
                        lex.SkipToken();
                        Transparent = true;
                        break;

                    case Token.TITLE:
                        if (!ParseTableTitle(lex))
                            return false;
                        break;

                    case Token.BORDERS:
                    case Token.NO_BORDERS:
                        if (!Borders.Parse(lex))
                            return false;
                        break;

                    case Token.EASYVIEW:
                        lex.SkipToken();
                        Easyview = true;

                        if (lex.Matched(Token.DYNAMIC))
                            EasyviewDynamic = true;

                        if (lex.LookAhead(Token.COLOR))
                            lex.ParseColor(Token.COLOR, out EasyviewColor);

                        break;

                    case Token.DROPSHADOW:
                        lex.SkipToken();
                        if (!lex.ParseInt(out DropShadowHeight))
                            return false;
                        if (!lex.ParseColor(Token.COLOR, out DropShadowColor))
                            return false;
                        break;

                    case Token.STYLE:
                        lex.SkipToken();
                        if (!lex.ParseString(out ClassName))
                            return false;
                        break;

                    case Token.TEMPLATE:
                        lex.SkipToken();
                        IsTemplate = true;
                        break;

                    case Token.HIDDEN:
                        {
                            Token[] stopTokens = { Token.SEP };
                            if (!ParseHidden(lex, stopTokens))
                                return false;
                            break;
                        }

                    default:
                        return true;
                }
        }


        //------------------------------------------------------------------------------
        private bool ParseTableTitleOption(WoormParser lex, bool blk)
        {
            bool ok = true;

            do
            {
                switch (lex.LookAhead())
                {
                    case Token.TEXTCOLOR:
                        ok = lex.ParseTextColor(out Title.TextColor);
                        break;

                    case Token.ALIGN:
                        ok = lex.ParseAlign(out Title.Align);
                        break;

                    case Token.BKGCOLOR:
                        ok = lex.ParseBkgColor(out Title.BkgColor);
                        break;

                    case Token.FONTSTYLE:
                        ok = lex.ParseFont(out Title.FontStyleName);
                        break;

                    case Token.PEN:
                        ok = lex.ParsePen(TitlePen);
                        break;

                    case Token.END:
                        if (blk) return ok;
                        lex.SetError(WoormViewerStrings.UnexpectedEnd);
                        return false;

                    default:
                        if (blk)
                        {
                            lex.SetError(WoormViewerStrings.UnexpectedEnd);
                            ok = false;
                        }
                        break;
                }
            }
            while (ok && blk);
            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseTableTitleOptions(WoormParser lex)
        {
            bool ok = true;
            do { ok = ParseTableTitleOption(lex, true) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseTableTitleBlock(WoormParser lex)
        {
            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseTableTitleOptions(lex) &&
                    lex.ParseEnd();

            return ParseTableTitleOption(lex, false);
        }

        //------------------------------------------------------------------------------
        private bool ParseTableTitle(WoormParser lex)
        {
            bool ok = true;

            if (lex.LookAhead(Token.TITLE))
                ok =
                    (
                    lex.ParseTag(Token.TITLE) &&
                    ParseTableTitleBlock(lex)
                    );

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseTableAttrib(WoormParser lex)
        {
            Token token;

            while ((token = lex.LookAhead()) != Token.END)
            {
                switch (token)
                {
                    case Token.COLUMN_PEN:
                        lex.SkipToken();
                        if (!ParseColumnPens(lex))
                            return false;
                        break;
                    case Token.BODY:
                        lex.SkipToken();
                        if (!ParseBodyVariations(lex))
                            return false;
                        break;
                    case Token.SUBTOTAL:
                        lex.SkipToken();
                        if (!ParseSubTotalCells(lex))
                            return false;
                        break;
                    case Token.COLTOTAL:
                        lex.SkipToken();
                        if (!ParseTotalCells(lex))
                            return false;
                        break;
                    case Token.TITLE:
                        lex.SkipToken();
                        if (!ParseColumnTitleCells(lex))
                            return false;
                        break;
                    default:
                        lex.SetError(WoormViewerStrings.IllegalTableAttribute);
                        return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------------------------                
        private bool ParseColumnPens(WoormParser lex)
        {
            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParsePens(lex) &&
                    lex.ParseEnd();

            return ParseSingleColumnPen(lex);
        }

        //------------------------------------------------------------------------------                
        private bool ParseSubTotalCells(WoormParser lex)
        {
            Color[] allColors = new Color[(int)ElementColor.MAX];
            allColors[(int)ElementColor.VALUE] = Color.FromArgb(255, 255, 255, 255);
            allColors[(int)ElementColor.BACKGROUND] = Color.FromArgb(255, 0, 0, 0);

            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseSubTotals(allColors, lex) &&
                    lex.ParseEnd();

            return ParseSingleSubTotal(allColors, lex);
        }

        //------------------------------------------------------------------------------                
        private bool ParseTotalCells(WoormParser lex)
        {
            Color[] allColors = new Color[(int)ElementColor.MAX];
            allColors[(int)ElementColor.VALUE] = Color.FromArgb(255, 255, 255, 255);
            allColors[(int)ElementColor.BACKGROUND] = Color.FromArgb(255, 0, 0, 0);

            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseTotals(allColors, lex) &&
                    lex.ParseEnd();

            return ParseSingleTotal(allColors, lex);
        }

        //------------------------------------------------------------------------------
        //
        private bool ParseColumnTitleCells(WoormParser lex)
        {
            Color[] allColors = new Color[(int)ElementColor.MAX];
            allColors[(int)ElementColor.VALUE] = Color.FromArgb(255, 255, 255, 255); ;
            allColors[(int)ElementColor.BACKGROUND] = Color.FromArgb(255, 169, 169, 169); ;

            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseColumnTitles(allColors, lex) &&
                    lex.ParseEnd();

            return ParseSingleColumnTitle(allColors, lex);
        }

        //------------------------------------------------------------------------------
        private bool ParsePens(WoormParser lex)
        {
            bool ok;

            do { ok = ParseSingleColumnPen(lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseSubTotals(Color[] allColors, WoormParser lex)
        {
            bool ok;

            do { ok = ParseSingleSubTotal(allColors, lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseTotals(Color[] allColors, WoormParser lex)
        {
            bool ok;

            do { ok = ParseSingleTotal(allColors, lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnTitles(Color[] allColors, WoormParser lex)
        {
            bool ok;

            do { ok = ParseSingleColumnTitle(allColors, lex) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseSingleColumnPen(WoormParser lex)
        {
            Rectangle rect;
            BorderPen aPen = new BorderPen();
            if (!ParseColumnIndex(out rect, lex) || !ParseColumnPen(aPen, lex))
                return false;

            // checks whether the columns belong to the table	
            int colSize = Columns.Count;// Columns.GetLength(0);

            if ((rect.Left >= colSize) || (rect.Right >= colSize))
            {
                lex.SetError(WoormViewerStrings.BadColumnsSize);
                return false;
            }
            for (int col = rect.Left; col <= rect.Right; col++)
                Columns[col].ColumnPen = aPen;
            return true;
        }

        //------------------------------------------------------------------------------
        private bool ParseSingleSubTotal(Color[] allColors, WoormParser lex)
        {
            Rectangle rect;
            Token token;

            int col;
            bool isAny = false;

            if (!ParseColumnIndex(out rect, lex)) return false;
            while ((token = lex.LookAhead()) != Token.SEP)
            {
                switch (token)
                {
                    case Token.TEXTCOLOR:
                        if (!lex.ParseColor(Token.TEXTCOLOR, out allColors[(int)ElementColor.VALUE]))
                            return false;
                        isAny = true;
                        for (col = rect.Left; col <= rect.Right; col++)
                            (Columns[col]).SubTotal.TextColor = allColors[(int)ElementColor.VALUE];
                        break;

                    case Token.BKGCOLOR:
                        if (!lex.ParseColor(Token.BKGCOLOR, out allColors[(int)ElementColor.BACKGROUND]))
                            return false;
                        for (col = rect.Left; col <= rect.Right; col++)
                            Columns[col].SubTotal.BkgColor = allColors[(int)ElementColor.BACKGROUND];
                        isAny = true;
                        break;

                    case Token.FONTSTYLE:
                        isAny = true;
                        return (ParseSubTotalDifferences(rect, lex));

                    default:
                        lex.SetError(WoormViewerStrings.BadSubTotalAttribute);
                        return false;
                }
            }
            // checks wheter there is no description
            if (!isAny)
            {
                lex.SetError(WoormViewerStrings.NoSubTotalAttribute);
                return false;
            }

            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseSingleTotal(Color[] allColors, WoormParser lex)
        {
            Rectangle rect;
            Token token;

            int col;
            bool isAny = false;

            if (!ParseColumnIndex(out rect, lex)) return false;
            while ((token = lex.LookAhead()) != Token.SEP)
            {
                Color color;
                switch (token)
                {
                    case Token.TEXTCOLOR:
                        if (!lex.ParseColor(Token.TEXTCOLOR, out color))
                            return false;
                        allColors[(int)ElementColor.VALUE] = color;

                        isAny = true;
                        for (col = rect.Left; col <= rect.Right; col++)
                            Columns[col].TotalCell.Value.TextColor = allColors[(int)ElementColor.VALUE];
                        break;

                    case Token.BKGCOLOR:
                        if (!lex.ParseColor(Token.BKGCOLOR, out allColors[(int)ElementColor.BACKGROUND]))
                            return false;
                        for (col = rect.Left; col <= rect.Right; col++)
                            Columns[col].TotalCell.Value.BkgColor = allColors[(int)ElementColor.BACKGROUND];
                        isAny = true;
                        break;

                    case Token.PEN:
                        {
                            if (!lex.ParseColor(Token.PEN, out allColors[(int)ElementColor.BORDER]))
                                return false;
                            for (col = rect.Left; col <= rect.Right; col++)
                                Columns[col].TotalCell.TotalPen.Color = allColors[(int)ElementColor.BORDER];
                            isAny = true;
                            break;
                        }

                    case Token.SIZE:
                        {
                            int penWidth;
                            if (!lex.ParseSize(out penWidth))
                                return false;
                            for (col = rect.Left; col <= rect.Right; col++)
                                Columns[col].TotalCell.TotalPen.Width = penWidth;
                            isAny = true;
                            break;
                        }

                    case Token.FONTSTYLE:
                    case Token.ALIGN:
                        isAny = true;
                        return (ParseTotalDifferences(rect, token, lex));

                    default:
                        lex.SetError(WoormViewerStrings.BadTotalAttribute);
                        return false;

                }
            }
            // checks whether there is no description
            if (!isAny)
            {
                lex.SetError(WoormViewerStrings.NoTotalAttribute);
                return false;
            }
            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseSingleColumnTitle(Color[] allColors, WoormParser lex)
        {
            Rectangle rect;
            Token token;
            int col;
            bool isAny = false;

            if (!ParseColumnIndex(out rect, lex)) return false;
            while ((token = lex.LookAhead()) != Token.SEP)
            {
                switch (token)
                {
                    case Token.TEXTCOLOR:
                        if (!lex.ParseColor(Token.TEXTCOLOR, out allColors[(int)ElementColor.LABEL]))
                            return false;
                        isAny = true;
                        for (col = rect.Left; col <= rect.Right; col++)
                            Columns[col].Title.TextColor = allColors[(int)ElementColor.LABEL];
                        break;

                    case Token.BKGCOLOR:
                        if (!lex.ParseColor(Token.BKGCOLOR, out allColors[(int)ElementColor.BACKGROUND]))
                            return false;
                        for (col = rect.Left; col <= rect.Right; col++)
                            Columns[col].Title.BkgColor = allColors[(int)ElementColor.BACKGROUND];
                        isAny = true;
                        break;

                    case Token.PEN:
                        {
                            if (!lex.ParseColor(Token.PEN, out allColors[(int)ElementColor.BORDER]))
                                return false;
                            for (col = rect.Left; col <= rect.Right; col++)
                                Columns[col].ColumnTitlePen.Color = allColors[(int)ElementColor.BORDER];
                            isAny = true;
                            break;
                        }

                    case Token.SIZE:
                        {
                            int penWidth;
                            if (!(lex.ParseSize(out penWidth)))
                                return false;
                            for (col = rect.Left; col <= rect.Right; col++)
                                Columns[col].ColumnTitlePen.Width = penWidth;
                            isAny = true;
                            break;
                        }
                    case Token.FONTSTYLE:
                    case Token.ALIGN:
                        isAny = true;
                        return (ParseColumnTitleDifferences(rect, token, lex));

                    default:
                        lex.SetError(WoormViewerStrings.BadColumnTitleAttribute);
                        return false;
                }
            }
            // checks whether there is no description
            if (!isAny)
            {
                lex.SetError(WoormViewerStrings.NoColumnTitleAttribute);
                return false;
            }
            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnPen(BorderPen pen, WoormParser lex)
        {
            Token token;
            bool isAny = false;

            Color penColor = Color.FromArgb(255, 255, 255, 255);
            int penWidth = 1;

            while ((token = lex.LookAhead()) != Token.SEP)
            {
                switch (token)
                {
                    case Token.PEN:
                        if (!lex.ParseColor(Token.PEN, out penColor))
                            return false;
                        isAny = true;
                        break;
                    case Token.SIZE:
                        if (!lex.ParseSize(out penWidth))
                            return false;
                        isAny = true;
                        break;
                    default:
                        lex.SetError(WoormViewerStrings.BadColumnPenAttribute);
                        return false;
                }
            }
            pen.Color = penColor;
            pen.Width = penWidth;

            // checks whether there is no description
            if (!isAny)
            {
                lex.SetError(WoormViewerStrings.NoColumnPenAttribute);
                return false;
            }
            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseSubTotalDifferences(Rectangle cellRect, WoormParser lex)
        {
            if (!ParseSubTotalFontIdx(cellRect, lex))
                return false;
            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseTotalDifferences(Rectangle cellRect, Token token, WoormParser lex)
        {
            if (token == Token.FONTSTYLE)
            {
                if (!ParseTotalFontIdx(cellRect, lex))
                    return false;
                token = lex.LookAhead();
            }
            if (token == Token.ALIGN)
                return ParseTotalAlign(cellRect, lex);

            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnTitleDifferences(Rectangle cellRect, Token token, WoormParser lex)
        {
            if (token == Token.FONTSTYLE)
            {
                if (!ParseColumnTitleFontIdx(cellRect, lex))
                    return false;
                token = lex.LookAhead();
            }
            if (token == Token.ALIGN)
                return ParseColumnTitleAlign(cellRect, lex);

            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseSubTotalFontIdx(Rectangle cellRect, WoormParser lex)
        {
            string fontStyleName;
            if (ParseFontStyle(out fontStyleName, lex))
            {
                for (int col = cellRect.Left; col <= cellRect.Right; col++)
                    Columns[col].SubTotal.FontStyleName = fontStyleName;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------
        private bool ParseTotalFontIdx(Rectangle cellRect, WoormParser lex)
        {
            string fontStyleName;
            if (ParseFontStyle(out fontStyleName, lex))
            {
                for (int col = cellRect.Left; col <= cellRect.Right; col++)
                    Columns[col].TotalCell.Value.FontStyleName = fontStyleName;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnTitleFontIdx(Rectangle cellRect, WoormParser lex)
        {
            string fontStyleName;
            if (ParseFontStyle(out fontStyleName, lex))
            {
                for (int col = cellRect.Left; col <= cellRect.Right; col++)
                    Columns[col].Title.FontStyleName = fontStyleName;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------                
        private bool ParseTotalAlign(Rectangle cellRect, WoormParser lex)
        {
            AlignType align;
            if (lex.ParseAlign(out align))
            {
                for (int col = cellRect.Left; col <= cellRect.Right; col++)
                    Columns[col].TotalCell.Value.Align = align;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------                
        private bool ParseColumnTitleAlign(Rectangle cellRect, WoormParser lex)
        {
            AlignType align;

            if (lex.ParseAlign(out align))
            {
                for (int col = cellRect.Left; col <= cellRect.Right; col++)
                    Columns[col].Title.Align = align;
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------                
        private bool ParseBodyVariations(WoormParser lex)
        {
            Color[] allColors = new Color[(int)ElementColor.MAX];
            allColors[(int)ElementColor.VALUE] = Color.FromArgb(255, 0, 0, 0); ;
            allColors[(int)ElementColor.BACKGROUND] = Color.FromArgb(255, 255, 255, 255); ;

            if (lex.LookAhead(Token.BEGIN))
                return
                    lex.ParseBegin() &&
                    ParseVariations(allColors, lex) &&
                    lex.ParseEnd();

            return ParseVariation(allColors, lex, false);
        }

        //------------------------------------------------------------------------------
        private bool ParseVariations(Color[] allColors, WoormParser lex)
        {
            bool ok;

            do { ok = ParseVariation(allColors, lex, true) && !lex.Error && !lex.Eof; }
            while (ok && !lex.LookAhead(Token.END));

            return ok;
        }
        //------------------------------------------------------------------------------
        private bool ParseVariation(Color[] allColors, WoormParser lex, bool blk)
        {
            switch (lex.LookAhead())
            {
                case Token.ALL:
                    {
                        bool ok;

                        ok = ParseMoreCoined(allColors, lex);
                        for (int col = 0; col < ColumnNumber; col++)
                            for (int row = 0; row < RowNumber; row++)
                            {
                                Columns[col].Cells[row].Value.TextColor = allColors[(int)ElementColor.VALUE];
                                Columns[col].Cells[row].Value.BkgColor = allColors[(int)ElementColor.BACKGROUND];
                            }
                        return ok;
                    }

                case Token.RECT: return ParseRectSet(allColors, lex);
                case Token.CELL: return ParseCellDiff(lex);

                default: lex.SetError(WoormViewerStrings.UnknownTableObject); return false;

            }
        }

        //------------------------------------------------------------------------------
        private bool ParseMoreCoined(Color[] allColors, WoormParser lex)
        {
            return
                lex.ParseTag(Token.ALL) &&
                ParseColorAttributes(allColors, lex);
        }

        //------------------------------------------------------------------------------
        private bool ParseColorAttributes(Color[] color, WoormParser lex)
        {
            Token token;
            bool anyDiff = false;

            while ((token = lex.LookAhead()) != Token.SEP)
            {
                switch (token)
                {
                    case Token.TEXTCOLOR:
                        if (!lex.ParseColor(Token.TEXTCOLOR, out color[(int)ElementColor.VALUE]))
                            return false;
                        anyDiff = true;
                        break;
                    case Token.BKGCOLOR:
                        if (!lex.ParseColor(Token.BKGCOLOR, out color[(int)ElementColor.BACKGROUND]))
                            return false;
                        anyDiff = true;
                        break;
                    default: lex.SetError(WoormViewerStrings.BadColorToken); return false;
                }
            }
            // checks whether there is no description
            if (!anyDiff)
            {
                lex.SetError(WoormViewerStrings.NoColorToken);
                return false;
            }
            return lex.ParseSep();
        }

        //------------------------------------------------------------------------------
        private bool ParseRectSet(Color[] allColors, WoormParser lex)
        {
            Rectangle rect;
            return
                lex.ParseRect(Token.RECT, out rect) &&
                ParseRectColorAttributes(allColors, rect, lex);
        }

        //------------------------------------------------------------------------------
        private bool ParseRectColorAttributes(Color[] allColors, Rectangle rect, WoormParser lex)
        {
            Color[] myColors = new Color[(int)ElementColor.MAX];
            myColors[(int)ElementColor.VALUE] = allColors[(int)ElementColor.VALUE];
            myColors[(int)ElementColor.BACKGROUND] = allColors[(int)ElementColor.BACKGROUND];

            int colSize = Columns.Count;
            int rowSize = Columns[0].Cells.Count;
            if (
                (rect.Left >= colSize) || (rect.Right >= colSize) ||
                (rect.Top >= rowSize) || (rect.Bottom >= rowSize)
                )
            {
                lex.SetError(WoormViewerStrings.BadCellSize);
                return false;
            }

            if (!ParseColorAttributes(myColors, lex))
                return false;

            for (int col = rect.Left; col <= rect.Right; col++)
                for (int row = rect.Top; row <= rect.Bottom; row++)
                {
                    Columns[col].Cells[row].Value.TextColor = myColors[(int)ElementColor.VALUE];
                    Columns[col].Cells[row].Value.BkgColor = myColors[(int)ElementColor.BACKGROUND];
                }
            return true;
        }

        //------------------------------------------------------------------------------
        private bool ParseCellDiff(WoormParser lex)
        {
            Rectangle cellRect;

            return
                ParseCellIndex(out cellRect, lex) &&
                ParseDifferences(cellRect, lex);
        }

        //------------------------------------------------------------------------------
        private bool ParseCellIndex(out Rectangle rect, WoormParser lex)
        {
            bool ok;
            int top = 0;
            int left = 0;
            int bottom = 0;
            int right = 0;

            ok = lex.ParseTag(Token.CELL) &&
                lex.ParseOpen() &&
                lex.ParseInt(out top) &&
                lex.ParseComma() &&
                lex.ParseInt(out left);

            if (ok)
                switch (lex.LookAhead())
                {
                    case Token.COMMA:
                        ok = lex.ParseComma() &&
                            lex.ParseInt(out bottom) &&
                            lex.ParseComma() &&
                            lex.ParseInt(out right) &&
                            lex.ParseClose();
                        break;

                    default:
                        ok = lex.ParseClose();
                        break;
                }

            rect = new Rectangle(left, top, right - left, bottom - top);
            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseColumnIndex(out Rectangle rect, WoormParser lex)
        {
            bool ok;
            int left = 0;
            int right = 0;
            rect = Rectangle.Empty;

            ok = lex.ParseTag(Token.COLUMN) &&
                lex.ParseOpen() &&
                lex.ParseInt(out left);

            if (!ok) return ok;

            switch (lex.LookAhead())
            {
                case Token.COMMA:
                    ok =
                        lex.ParseComma() &&
                        lex.ParseInt(out right) &&
                        lex.ParseClose();
                    break;

                default:
                    right = left;
                    ok = lex.ParseClose();
                    break;
            }

            rect = new Rectangle(left, 0, right - left, 0);
            return ok;
        }

        //------------------------------------------------------------------------------
        private bool ParseDifferences(Rectangle cellRect, WoormParser lex)
        {
            Token token = lex.LookAhead();

            if (token == Token.FONTSTYLE)
                if (!ParseCellFontIdx(cellRect, lex))
                    return false;
                else token = lex.LookAhead();

            switch (token)
            {
                case Token.SEP: return lex.ParseSep();
                case Token.ALIGN: return ParseCellAlign(cellRect, lex);
                default: lex.SetError(WoormViewerStrings.DifferenceError); return false;
            }

        }

        /**
		* Quella originaria di woorm c++ se il rettangolo indica tutta la colonna
		* mette il font solo sulla colonna perchè è il renderizzatore che controllava se 
		* quello della cella è default e quello della colonna è diverso da default allora
		* usa quello della colonna. Qui in C# non si tiene conto dei default e pertanto non va bene
		* ma deve fare come per gli align		
		* 
		**/
        //------------------------------------------------------------------------------
        private bool ParseCellFontIdx(Rectangle cellRect, WoormParser lex)
        {
            string fontStyleName;
            if (!ParseFontStyle(out fontStyleName, lex))
                return false;

            //aggiunto per andare in contro alla logica dei report template, nel template lo stile di tutte 
            //le celle di una colonna e' memorizzato solo nel fontstyle di colonna
            bool saveFontInColumn = cellRect.Top == 0 && cellRect.Bottom == RowNumber - 1;

            for (int col = cellRect.Left; col <= cellRect.Right; col++)
            {
                Column currentColumn = Columns[col];
                for (int row = cellRect.Top; row <= cellRect.Bottom; row++)
                    currentColumn.Cells[row].Value.FontStyleName = fontStyleName;

                if (saveFontInColumn)
                    currentColumn.FontSyleName = fontStyleName;
            }

            return true;
        }

        //------------------------------------------------------------------------------
        private bool ParseFontStyle(out string styleName, WoormParser lex)
        {
            styleName = "";
            return lex.ParseTag(Token.FONTSTYLE) &&
                lex.ParseString(out styleName);
        }

        //------------------------------------------------------------------------------
        public override bool Unparse(Unparser unparser)
        {
            //---- Template Override 
            Table tempDefault = Default;
            if (IsTemplate && Document.Template.IsSavingTemplate)
                Default = null;
            //----

            unparser.WriteTable(RowNumber, ColumnNumber, false);

            unparser.WriteString
                    (
                        unparser.IsLocalizableTextInCurrentLanguage()
                            ? unparser.LoadReportString(Title.Text)
                            : Title.Text,
                        false
                    );
            unparser.WriteBlank();

            unparser.WriteAlias(InternalID, false);

            unparser.Write(" /* " + GetTableName() + " */ ");

            unparser.WriteOrigin(new Point(TitleRect.Left, TitleRect.Top), false);

            unparser.WriteHeights
            (
                TitleRect.Height,
                ColumnTitleRect(0).Height,
                CellRect(0, 0).Height,
                TotalRect(0).Height
            );

            UnparseTitleAndBorders(unparser);
            UnparseColumnsBlock(unparser);

            unparser.WriteLine();

            //----
            Default = tempDefault;
            return true;
        }

        //------------------------------------------------------------------------------
        public string GetTableName(bool stringName = false)
        {
            string strName = string.Empty;
            if (stringName && Document.SymbolTable != null)
            {
                //    WoormTable* pST = m_pDocument->m_pEngine->GetSymTable();
                //    int idx = pST && pST->GetDisplayTables() ?
                //        pST->GetDisplayTables()->Find(GetInternalID(), m_pDocument->m_dsCurrentLayoutView)
                //        : - 1;
                //    if (idx > -1)
                //    {
                //        strName = pST->GetDisplayTables()->GetAt(idx)->GetTableName();
                //    }
            }

            if (strName.IsNullOrEmpty())
                strName = string.Format(("Table{0}"), InternalID);
            return strName;
        }

        //------------------------------------------------------------------------------
        private void UnparseColumnsBlock(Unparser unparser)
        {
            unparser.IncTab();
            unparser.WriteBegin();
            unparser.IncTab();

            UnparseAllColumns(unparser);

            //attributi statici grafici della tabella
            UnparseTableAttrib(unparser);

            unparser.DecTab();
            unparser.WriteEnd();
            unparser.DecTab();
        }

        //------------------------------------------------------------------------------
        private void UnparseTableAttrib(Unparser unparser)
        {
            BodyTable bodyTable = new BodyTable(this);

            UnparseColumnPens(unparser, bodyTable);
            UnparseColumnTitleCells(unparser, bodyTable);
            UnparseSubTotalCells(unparser, bodyTable);
            UnparseTotalCells(unparser, bodyTable);

            UnparseBodyVariations(unparser, bodyTable);
        }

        //------------------------------------------------------------------------------
        private void UnparseTotalCells(Unparser unparser, BodyTable bodyTable)
        {
            int nrTotals = bodyTable.NrTotalCells;
            if (nrTotals == 0)
                return;

            if (nrTotals == 1)
            {
                unparser.WriteTag(Token.COLTOTAL, false);
                UnparseSingleTotal(bodyTable.GetNextTotalCell(0), unparser);
            }
            else
            {
                unparser.WriteTag(Token.COLTOTAL, true);

                unparser.IncTab();
                unparser.WriteBegin();
                unparser.IncTab();

                for (int index = 0; index < nrTotals; index++)
                    UnparseSingleTotal(bodyTable.GetNextTotalCell(index), unparser);

                unparser.DecTab();
                unparser.WriteEnd();
                unparser.DecTab();
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleTotal(CellTotalDiff aTotalDiff, Unparser unparser)
        {
            UnparseColumnIndex(aTotalDiff.Rect, unparser);
            Color[] colors = aTotalDiff.Colors;

            bool fnt, aln;
            bool txt = (colors[(int)ElementColor.VALUE] != Defaults.DefaultTotalForeground);
            bool bkg = (colors[(int)ElementColor.BACKGROUND] != Defaults.DefaultTotalBackground);
            bool brd = (colors[(int)ElementColor.BORDER] != Color.FromArgb(255, 255, 255, 255));

            UnparseAttribute
            (
                txt, bkg, brd,
                colors[(int)ElementColor.VALUE],
                colors[(int)ElementColor.BACKGROUND],
                colors[(int)ElementColor.BORDER],
                unparser
            );

            if (aTotalDiff.PenWidth != Defaults.DefaultPenWidth)
            {
                unparser.WriteTag(Token.SIZE, false);
                unparser.Write(aTotalDiff.PenWidth, false);
                unparser.WriteBlank();
            }

            if (aTotalDiff.CellIsNumeric)
            {
                fnt = (aTotalDiff.FontName != DefaultFont.TotaleNumerico);
                aln = (aTotalDiff.Align != Defaults.DefaultTotalNumAlign);
            }
            else
            {
                fnt = (aTotalDiff.FontName != DefaultFont.TotaleStringa);
                aln = (aTotalDiff.Align != Defaults.DefaultTotalStringAlign);
            }

            UnparseDifferences(fnt, aln, aTotalDiff.FontName, aTotalDiff.Align, unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseSubTotalCells(Unparser unparser, BodyTable bodyTable)
        {
            int nrSubTotals = bodyTable.NrSubTotalCells;
            if (nrSubTotals == 0)
                return;

            if (nrSubTotals == 1)
            {
                unparser.WriteTag(Token.SUBTOTAL, false);
                UnparseSingleSubTotal(bodyTable.GetNextSubTotalCell(0), unparser);
            }
            else
            {
                unparser.WriteTag(Token.SUBTOTAL, true);

                unparser.IncTab();
                unparser.WriteBegin();
                unparser.IncTab();

                for (int index = 0; index < nrSubTotals; index++)
                    UnparseSingleSubTotal(bodyTable.GetNextSubTotalCell(index), unparser);

                unparser.DecTab();
                unparser.WriteEnd();
                unparser.DecTab();
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleSubTotal(CellSubTotalDiff aSubTotal, Unparser unparser)
        {
            UnparseColumnIndex(aSubTotal.Rect, unparser);
            Color[] colors = aSubTotal.Colors;

            bool fnt;
            bool txt = (colors[(int)ElementColor.VALUE] != Defaults.DefaultSubTotalForeground);
            bool bkg = (colors[(int)ElementColor.BACKGROUND] != Defaults.DefaultSubTotalBackground);

            UnparseAttribute
            (
                txt, bkg, false,
                colors[(int)ElementColor.VALUE],
                colors[(int)ElementColor.BACKGROUND],
                Color.FromArgb(255, 255, 255, 255),
                unparser
            );

            fnt = aSubTotal.CellIsNumeric
                ? (aSubTotal.FontName != DefaultFont.SubTotaleNumerico)
                : (aSubTotal.FontName != DefaultFont.SubTotaleStringa);

            UnparseDifferences(fnt, false, aSubTotal.FontName, aSubTotal.Align, unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseColumnTitleCells(Unparser unparser, BodyTable bodyTable)
        {
            int nrColumnTitles = bodyTable.NrTitleCells;
            if (nrColumnTitles == 0)
                return;

            if (nrColumnTitles == 1)
            {
                unparser.WriteTag(Token.TITLE, false);
                UnparseSingleColumnTitle(bodyTable.GetNextTitleCell(0), unparser);
            }
            else
            {
                unparser.WriteTag(Token.TITLE, true);

                unparser.IncTab();
                unparser.WriteBegin();
                unparser.IncTab();

                for (int index = 0; index < nrColumnTitles; index++)
                    UnparseSingleColumnTitle(bodyTable.GetNextTitleCell(index), unparser);

                unparser.DecTab();
                unparser.WriteEnd();
                unparser.DecTab();
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleColumnTitle(CellColumnTitleDiff aColumnTitle, Unparser unparser)
        {
            Color[] rectColors = aColumnTitle.Colors;
            UnparseColumnIndex(aColumnTitle.Rect, unparser);

            bool txt = (rectColors[(int)ElementColor.LABEL] != Defaults.DefaultColumnTitleForeground);
            bool bkg = (rectColors[(int)ElementColor.BACKGROUND] != Defaults.DefaultColumnTitleBackground);
            bool brd = (rectColors[(int)ElementColor.BORDER] != Color.FromArgb(255, 255, 255, 255));

            UnparseAttribute
            (
                txt, bkg, brd,
                rectColors[(int)ElementColor.LABEL],
                rectColors[(int)ElementColor.BACKGROUND],
                rectColors[(int)ElementColor.BORDER],
                unparser
            );

            if (aColumnTitle.PenWidth != Defaults.DefaultPenWidth)
            {
                unparser.WriteTag(Token.SIZE, false);
                unparser.Write(aColumnTitle.PenWidth, false);
                unparser.WriteBlank();
            }

            bool fnt = (aColumnTitle.FontName != DefaultFont.TitoloColonna);
            bool aln = (aColumnTitle.Align != Defaults.DefaultColumnTitleAlign);

            UnparseDifferences(fnt, aln, aColumnTitle.FontName, aColumnTitle.Align, unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseAttribute(bool txt, bool bkg, bool brd, Color txtColor, Color bkgColor, Color brdColor, Unparser unparser)
        {
            if (txt)
                unparser.WriteColor(Token.TEXTCOLOR, txtColor, false);
            if (bkg)
                unparser.WriteColor(Token.BKGCOLOR, bkgColor, false);
            if (brd)
                unparser.WriteColor(Token.PEN, brdColor, false);
        }

        //------------------------------------------------------------------------------
        private void UnparseDifferences(bool fnt, bool aln, string fontName, AlignType align, Unparser unparser)
        {
            if (fnt)
                UnparseSingleFont(fontName, unparser);
            if (aln)
                unparser.WriteAlign(align, true);
            else
                unparser.WriteSep(true);
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleFont(string fontName, Unparser unparser)
        {
            unparser.WriteTag(Token.FONTSTYLE, false);
            unparser.WriteString(fontName, false);
            unparser.WriteBlank();
        }

        //------------------------------------------------------------------------------
        private void UnparseColumnPens(Unparser unparser, BodyTable bodyTable)
        {
            int nrColumnPens = bodyTable.ColumnPenCells;
            if (nrColumnPens == 0)
                return;

            if (nrColumnPens == 1)
            {
                unparser.WriteTag(Token.COLUMN_PEN, false);
                UnparseColumnIndex(bodyTable.GetNextColumnPen(0).Rect, unparser);
                UnparseSingleColumnPen(bodyTable.GetNextColumnPen(0).BorderPen, unparser);
                unparser.WriteSep(true);
            }
            else
            {
                unparser.WriteTag(Token.COLUMN_PEN, true);

                unparser.IncTab();
                unparser.WriteBegin();
                unparser.IncTab();

                for (int index = 0; index < nrColumnPens; index++)
                {
                    UnparseColumnIndex(bodyTable.GetNextColumnPen(index).Rect, unparser);
                    UnparseSingleColumnPen(bodyTable.GetNextColumnPen(index).BorderPen, unparser);
                    unparser.WriteSep(true);
                }

                unparser.DecTab();
                unparser.WriteEnd();
                unparser.DecTab();

            }
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleColumnPen(BorderPen aColumnPen, Unparser unparser)
        {
            if (aColumnPen.Color != Color.FromArgb(255, 255, 255, 255))
                unparser.WriteColor(Token.PEN, aColumnPen.Color, false);

            if (aColumnPen.Width != Defaults.DefaultPenWidth)
            {
                unparser.WriteTag(Token.SIZE, false);
                unparser.Write(aColumnPen.Width, false);
            }
            unparser.WriteBlank();
        }

        //------------------------------------------------------------------------------
        private void UnparseColumnIndex(Rectangle rectangle, Unparser unparser)
        {
            unparser.WriteTag(Token.COLUMN, false);
            unparser.WriteOpen(false);

            unparser.Write(rectangle.Left, false);

            if (rectangle.Right != rectangle.Left)
            {
                unparser.WriteComma(false);
                unparser.Write(rectangle.Right, false);
            }
            unparser.WriteClose(false);
            unparser.WriteBlank();
        }

        //------------------------------------------------------------------------------
        private void UnparseBodyVariations(Unparser unparser, BodyTable bodyTable)
        {
            Color[] cellColors = new Color[(int)ElementColor.MAX];
            Color[] coinedColors = bodyTable.CoinedCellColor;

            bool txt = (coinedColors[(int)ElementColor.VALUE] != Defaults.DefaultCellForeground);
            bool bkg = (coinedColors[(int)ElementColor.BACKGROUND] != Defaults.DefaultCellBackground);

            int nRectIndex = 0;
            int nFixIndex = -1;

            int nrVariations = bodyTable.NrCellDiff;
            if (txt || bkg)
                nrVariations++;

            while ((nrVariations < 2) && (nRectIndex < bodyTable.NrRect))
            {
                cellColors = bodyTable.GetNextSingleRect(nRectIndex).CellColor;
                if (
                    cellColors[(int)ElementColor.VALUE] != coinedColors[(int)ElementColor.VALUE] ||
                    cellColors[(int)ElementColor.BACKGROUND] != coinedColors[(int)ElementColor.BACKGROUND]
                    )
                {
                    nrVariations++;
                    if (nFixIndex >= 0)
                        break;

                    nFixIndex = nRectIndex;
                }
                nRectIndex++;
            }

            if (nrVariations == 0)
                return;

            unparser.WriteLine();
            unparser.WriteTag(Token.BODY, (nrVariations > 1));

            if (nrVariations < 2)
                UnparseVariations(txt, bkg, nFixIndex, nRectIndex, bodyTable, unparser);
            else
            {
                unparser.IncTab();
                unparser.WriteBegin();
                unparser.IncTab();

                UnparseVariations(txt, bkg, nFixIndex, nRectIndex, bodyTable, unparser);

                unparser.DecTab();
                unparser.WriteEnd();
                unparser.DecTab();

            }
            unparser.WriteLine();
        }

        //------------------------------------------------------------------------------
        private void UnparseVariations(bool txt, bool bkg, int nFixIndex, int nRectIndex, BodyTable bodyTable, Unparser unparser)
        {
            if (txt || bkg)
                UnparseMoreCoined(txt, bkg, bodyTable.CoinedCellColor, unparser);

            if (bodyTable.NrRect > 0)
                UnparseRectSet(nFixIndex, nRectIndex, bodyTable, unparser);

            if (bodyTable.NrCellDiff > 0)
                UnparseDifferentCells(bodyTable, unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseDifferentCells(BodyTable bodyTable, Unparser unparser)
        {
            bool bFnt, bAln;
            int nrDifferences = bodyTable.NrCellDiff;

            for (int diffIndex = 0; diffIndex < nrDifferences; diffIndex++)
            {
                CellBodyDiff singleDiff = bodyTable.GetNextCellDiff(diffIndex);
                UnparseCellIndex(singleDiff.Rect, unparser);

                if (singleDiff.CellIsNumeric)
                {
                    bFnt = (singleDiff.FontName != DefaultFont.CellaNumerica);
                    bAln = (singleDiff.Align != Defaults.DefaultCellNumAlign);
                }
                else
                {
                    bFnt = (singleDiff.FontName != DefaultFont.CellaStringa);
                    bAln = (singleDiff.Align != Defaults.DefaultCellStringAlign);
                }
                UnparseDifferences
                (
                    bFnt, bAln,
                    singleDiff.FontName,
                    singleDiff.Align,
                    unparser
                );
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseCellIndex(Rectangle singleCell, Unparser unparser)
        {
            unparser.WriteTag(Token.CELL, false);
            unparser.WriteOpen(false);
            unparser.Write(singleCell.Top, false);
            unparser.WriteComma(false);
            unparser.Write(singleCell.Left, false);

            if (singleCell.Top != singleCell.Bottom)
            {
                unparser.WriteComma(false);
                unparser.Write(singleCell.Bottom, false);
                unparser.WriteComma(false);
                unparser.Write(singleCell.Right, false);
            }
            unparser.WriteClose(false);
            unparser.WriteBlank();
        }

        //------------------------------------------------------------------------------
        private void UnparseRectSet(int nFixIndex, int nRectIndex, BodyTable bodyTable, Unparser unparser)
        {
            if (nFixIndex >= 0)
                UnparseSingleRect(bodyTable, nFixIndex, unparser);

            int nrRect = bodyTable.NrRect;
            for (int index = nRectIndex; index < nrRect; index++)
                UnparseSingleRect(bodyTable, index, unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseSingleRect(BodyTable bodyTable, int index, Unparser unparser)
        {
            SingleRect singleRect = bodyTable.GetNextSingleRect(index);

            Color[] coinedColors = bodyTable.CoinedCellColor;
            Color[] singleCellColor = singleRect.CellColor;

            bool rectTxt = (singleCellColor[(int)ElementColor.VALUE] != coinedColors[(int)ElementColor.VALUE]);
            bool rectBkg = (singleCellColor[(int)ElementColor.BACKGROUND] != coinedColors[(int)ElementColor.BACKGROUND]);

            if (rectTxt || rectBkg)
            {
                unparser.WriteRect(Token.RECT, singleRect.Rectangle, false);

                UnparseAttribute
                (
                    rectTxt, rectBkg, false,
                    singleCellColor[(int)ElementColor.VALUE],
                    singleCellColor[(int)ElementColor.BACKGROUND],
                    Color.FromArgb(255, 255, 255, 255),
                    unparser
                );
                unparser.WriteSep(true);
            }
        }

        //------------------------------------------------------------------------------
        private void UnparseMoreCoined(bool txt, bool bkg, Color[] colors, Unparser unparser)
        {
            unparser.WriteTag(Token.ALL, false);
            UnparseAttribute
            (
                txt, bkg, false,
                colors[(int)ElementColor.VALUE],
                colors[(int)ElementColor.BACKGROUND],
                Color.FromArgb(255, 255, 255, 255),
                unparser
            );
            unparser.WriteSep(true);
        }

        //------------------------------------------------------------------------------
        private void UnparseAllColumns(Unparser unparser)
        {
            foreach (Column col in Columns)
                col.Unparse(unparser);
        }

        //------------------------------------------------------------------------------
        private void UnparseTitleAndBorders(Unparser unparser)
        {
            if (IsTemplate)
                unparser.WriteTag(Token.TEMPLATE);

            if (!ClassName.IsNullOrEmpty())
            {
                unparser.WriteTag(Token.STYLE, false);
                unparser.WriteString(ClassName);
            }

            if (HideTableTitle && !HideColumnsTitle)
                unparser.WriteTag(Token.HIDE_TABLE_TITLE);

            if (HideColumnsTitle)
                unparser.WriteTag(Token.HIDE_ALL_TABLE_TITLE);

            if (Easyview || EasyviewDynamic)
            {
                unparser.WriteTag(Token.EASYVIEW, false);

                if (EasyviewDynamic)
                    unparser.WriteTag(Token.DYNAMIC, false);

                if (EasyviewColor != Defaults.AlternateColor)
                    unparser.WriteColor(Token.COLOR, EasyviewColor, false);

                unparser.WriteLine();
            }

            if (DropShadowHeight > 0)
            {
                unparser.WriteTag(Token.DROPSHADOW, false);
                unparser.Write(DropShadowHeight, false);
                unparser.WriteBlank();
                unparser.WriteColor(Token.COLOR, DropShadowColor);
            }

            if (Transparent)
                unparser.WriteTag(Token.TRANSPARENT);

            if (FiscalEnd)
                unparser.WriteTag(Token.FISCAL_END);

            WriteHidden(unparser);

            int col = Title.TextColor != Defaults.DefaultTextColor ? 1 : 0;
            int bkg = Title.BkgColor != Defaults.DefaultBackColor ? 1 : 0;
            int alg = Title.Align != Defaults.DefaultAlign ? 1 : 0;
            int fnt = Title.FontStyleName != DefaultFont.TitoloTabella ? 1 : 0;
            int pen = !TitlePen.IsDefault ? 1 : 0;
            int wbr = col + bkg + alg + fnt + pen;
            bool blk = wbr > 1;

            if (wbr > 0) unparser.WriteTag(Token.TITLE, blk);
            if (blk) unparser.WriteBegin();
            if (col > 0) unparser.WriteColor(Token.TEXTCOLOR, Title.TextColor);
            if (bkg > 0) unparser.WriteColor(Token.BKGCOLOR, Title.BkgColor);
            if (alg > 0) unparser.WriteAlign(Title.Align);
            if (fnt > 0) unparser.WriteFont(Title.FontStyleName);
            if (pen > 0) unparser.WritePen(TitlePen);
            if (blk) unparser.WriteEnd();

            // unparsa i bordi della tabella
            Borders.Unparse(unparser);
        }

        //------------------------------------------------------------------------------
        private void WriteHidden(Unparser unparser)
        {
            if (HideExpr != null && !Document.Template.IsSavingTemplate)
            {
                unparser.WriteTag(Token.HIDDEN);
                unparser.WriteBlank();
                unparser.WriteTag(Token.WHEN);

                if (Document.ReplaceHiddenWhenExpr)
                    unparser.WriteTag(Token.FALSE);
                else
                    unparser.WriteExpr(HideExpr.ToString(), false);

                unparser.WriteSep(true);
            }
        }

        //------------------------------------------------------------------------------
        private bool ParseCellAlign(Rectangle cellRect, WoormParser lex)
        {
            AlignType align;

            if (!lex.ParseAlign(out align))
                return false;

            for (int col = cellRect.Left; col <= cellRect.Right; col++)
                for (int row = cellRect.Top; row <= cellRect.Bottom; row++)
                    Columns[col].Cells[row].Value.Align = align;

            return true;
        }

        //------------------------------------------------------------------------------
        public override void ClearStyle()
        {
            Transparent = Default != null ? Default.Transparent : false;

            Borders = Default != null ? Default.Borders : new TableBorders();
            TitlePen = Default != null ? Default.TitlePen : new BorderPen();

            HideTableTitle = Default != null ? Default.HideTableTitle : false;
            HideColumnsTitle = Default != null ? Default.HideColumnsTitle : false;

            Easyview = Default != null ? Default.Easyview : false;
            EasyviewDynamic = Default != null ? Default.EasyviewDynamic : false;
            EasyviewColor = Default != null ? Default.EasyviewColor : Defaults.AlternateColor;

            DropShadowHeight = Default != null ? Default.DropShadowHeight : 0;
            DropShadowColor = Default != null ? Default.DropShadowColor : Color.FromArgb(255, 255, 255, 255);

            FiscalEnd = Default != null ? Default.FiscalEnd : false;

            Title.Align = Default != null ? Default.Title.Align : Defaults.DefaultAlign;

            BaseRect.ClearTemplateFont(Document, ref Title.FontStyleName, Default != null ? Default.Title.FontStyleName : null, DefaultFont.Testo);

            Title.TextColor = Default != null ? Default.Title.TextColor : Defaults.DefaultTextColor;
            Title.BkgColor = Default != null ? Default.Title.BkgColor : Defaults.DefaultBackColor;
        }

        //------------------------------------------------------------------------------
        public override void RemoveStyle()
        {
            if (Default == null)
                return;

            if (Transparent == Default.Transparent)
                Transparent = false;

            if (Borders == Default.Borders)
                Borders = new TableBorders();

            if (TitlePen == Default.TitlePen)
                TitlePen = new BorderPen();

            if (HideTableTitle == Default.HideTableTitle)
                HideTableTitle = false;
            if (HideColumnsTitle == Default.HideColumnsTitle)
                HideColumnsTitle = false;

            if (
                    Easyview == Default.Easyview &&
                    EasyviewDynamic == Default.EasyviewDynamic &&
                    EasyviewColor == Default.EasyviewColor
                )
            {
                Easyview = false;
                EasyviewDynamic = false;
                EasyviewColor = Defaults.AlternateColor;
            }

            if (DropShadowHeight == Default.DropShadowHeight && DropShadowColor == Default.DropShadowColor)
            {
                DropShadowHeight = 0;
                DropShadowColor = Color.FromArgb(255, 255, 255, 255); ;
            }

            if (FiscalEnd == Default.FiscalEnd)
                FiscalEnd = false;

            if (Title.Align == Default.Title.Align)
                Title.Align = Defaults.DefaultAlign;

            BaseRect.RemoveTemplateFont(Document, ref Title.FontStyleName, Default.Title.FontStyleName, DefaultFont.Testo);

            if (Title.TextColor == Default.Title.TextColor)
                Title.TextColor = Defaults.DefaultTextColor;
            if (Title.BkgColor == Default.Title.BkgColor)
                Title.BkgColor = Defaults.DefaultBackColor;

            Default = null;
        }

        /// <summary>
        /// Ricopia lo stile grafico dall'equivalente oggetto nel template
        /// </summary>
        //------------------------------------------------------------------------------
        internal void SetStyle(Table templateTable)
        {
            RemoveStyle();
            if (templateTable == null)
                return;

            Default = templateTable;

            if (!Transparent)
                Transparent = templateTable.Transparent;

            if (Borders.IsDefault())
                Borders = templateTable.Borders;

            if (TitlePen == new BorderPen())
                TitlePen = templateTable.TitlePen;

            if (!HideTableTitle)
                HideTableTitle = templateTable.HideTableTitle;

            if (!HideColumnsTitle)
                HideColumnsTitle = templateTable.HideColumnsTitle;

            if (!Easyview && !EasyviewDynamic && EasyviewColor == Defaults.AlternateColor)
            {
                Easyview = templateTable.Easyview;
                EasyviewDynamic = templateTable.EasyviewDynamic;
                EasyviewColor = templateTable.EasyviewColor;
            }

            if (DropShadowHeight == 0 && DropShadowColor == Defaults.DefaultShadowColor)
            {
                DropShadowHeight = templateTable.DropShadowHeight;
                DropShadowColor = templateTable.DropShadowColor;
            }

            if (!FiscalEnd)
                FiscalEnd = templateTable.FiscalEnd;

            if (Title.Align == Defaults.DefaultAlign)
                Title.Align = templateTable.Title.Align;

            if (Title.FontStyleName == DefaultFont.TitoloTabella)
                Title.FontStyleName = templateTable.Title.FontStyleName;

            BaseRect.SetTemplateFont(Document, ref Title.FontStyleName, templateTable.Title.FontStyleName, DefaultFont.Testo);

            if (Title.TextColor == Defaults.DefaultTableTitleForeground) //DEFAULT_TEXTCOLOR
                Title.TextColor = templateTable.Title.TextColor;

            if (Title.BkgColor == Defaults.DefaultTableTitleBackground) //DEFAULT_BKGCOLOR
                Title.BkgColor = templateTable.Title.BkgColor;
        }


        /// <summary>
        /// Predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
        /// </summary>
        //---------------------------------------------------------------------------
        internal bool HasBottomBorderAtCell(Cell cell)
        {
            bool lastRow = cell.IsLastRow;
            //il bordo inferiore tiene conto della logica del rowseparator di tabella, statico o dinamico 
            //(dinamico: che considera unica riga di dati spezzata su piu' righe)
            bool rowSeparator = false;
            if (Borders.DynamicRowSeparator && !lastRow)
                rowSeparator = !ExistsCellTail(cell.AtRowNumber + 1);
            else
                rowSeparator = Borders.RowSeparator;

            //gestione VMergeEmptyCell e VMergeTailCell
            //controllo preventivamente se la colonna ha uno dei due modi di visualizzazione attivo per evitare 
            //di entrare nell'if e fare operazioni inutili
            if (
                (cell.column.VMergeEmptyCell || cell.column.VMergeTailCell || cell.column.VMergeEqualCell)
                &&
                !lastRow
                )
            {
                Cell belowCell = (Cell)cell.column.Cells[cell.AtRowNumber + 1];

                if (
                        cell.column.VMergeEmptyCell && string.IsNullOrEmpty(belowCell.Value.FormattedData)
                        ||
                        cell.column.VMergeTailCell && belowCell.Value.CellTail
                    )
                    rowSeparator = false;

                if (
                        cell.column.VMergeEqualCell
                        &&
                        !cell.SubTotal
                        &&
                        !belowCell.SubTotal
                        &&
                        (
                            (
                                !string.IsNullOrEmpty(cell.Value.FormattedData)
                                &&
                                cell.Value.FormattedData == belowCell.Value.FormattedData
                            )
                            ||
                            (
                                string.IsNullOrEmpty(cell.Value.FormattedData) && cell.Value.CellTail &&
                                string.IsNullOrEmpty(belowCell.Value.FormattedData) && belowCell.Value.CellTail &&
                                !string.IsNullOrEmpty(cell.column.PreviousValue)
                            )
                            ||
                            (
                                string.IsNullOrEmpty(cell.Value.FormattedData)
                                &&
                                !string.IsNullOrEmpty(belowCell.Value.FormattedData)
                                &&
                                !string.IsNullOrEmpty(cell.column.PreviousValue)
                                &&
                                cell.column.PreviousValue == belowCell.Value.FormattedData
                            )
                        )
                    )
                    rowSeparator = false;

            }

            return (!lastRow && (rowSeparator || Interlines[cell.AtRowNumber])) || (lastRow && Borders.Body.Bottom);
        }

        /// <summary>
        /// Predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
        /// </summary>
        //---------------------------------------------------------------------------
        internal void InitEasyview()
        {
            existsCellTail = false;

            if (Document != null && Document.RdeReader != null)
            {
                Document.SynchronizeSymbolTable(0);
            }

            //in presenza di EasyviewDynamic devo tenere traccia se la prima riga di ogni pagina
            //deve essere evidenziata o meno
            if (EasyviewDynamic && Document != null && Document.RdeReader != null)
            {
                int pageZeroBased = Document.RdeReader.CurrentPage - 1;
                if (pageZeroBased == EasyViewDynamicOnPage.Count)
                {
                    //sono su una pagina nuova (e non sulla prima), se non c'e' una cella tail alla prima riga (riga 0)
                    //devo invertire la colorazione rispetto all'ultima riga della pagina precedente
                    existsCellTail = ExistsCellTail(0);

                    if (!existsCellTail && pageZeroBased > 0)
                        UseEasyviewColor = !UseEasyviewColor;

                    EasyViewDynamicOnPage.Add(UseEasyviewColor);
                }
                else if (pageZeroBased < EasyViewDynamicOnPage.Count)
                    UseEasyviewColor = (bool)EasyViewDynamicOnPage[pageZeroBased];
            }
        }

        /// <summary>
        /// Determina se la riga passata come argomento deve avere lo sfondo Easyview o meno 
        /// (stile di colore alternato)
        /// </summary>
        //------------------------------------------------------------------------------
        internal bool UseColorEasyview(int row)
        {
            if (!Easyview && !EasyviewDynamic)
                return false;

            if (!EasyviewDynamic)
                return (row % 2) != 0;

            return UseEasyviewColor;
        }

        /// <summary>
        /// Metodo che dice se alla riga passata come argomento esiste la prosecuzione di un dato iniziato su una riga precedente
        /// (cieo esiste almeno una cella "tail")
        /// </summary>
        //---------------------------------------------------------------------------
        internal bool ExistsCellTail(int nRow)
        {
            if (nRow >= RowNumber)
                return false;

            int nLastColumn = LastVisibleColumn();
            for (int nCol = 0; nCol <= nLastColumn; nCol++)
            {
                Column column = Columns[nCol];
                if (column.IsHidden)
                    continue;

                Cell cell = column.Cells[nRow];
                if (cell.Value == null || cell.Value.RDEData == null)
                    continue;

                if (cell.Value.CellTail)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Predispone la table per la modalita di Easyview dinamica (nel caso sia presente)
        /// per la riga successiva
        /// </summary>
        //---------------------------------------------------------------------------
        internal void EasyViewNextRow(int row)
        {
            if (row == (RowNumber - 1))
                return;

            existsCellTail = ExistsCellTail(row + 1);
            Document.SynchronizeSymbolTable(row + 1, existsCellTail);

            if (!EasyviewDynamic)
                return;

            //se la riga successiva non ha una cella coda della riga corrente,
            //cambio colore di sfondo dell'easyview
            if (!existsCellTail)
                UseEasyviewColor = !UseEasyviewColor;
        }

        //---------------------------------------------------------------------------
        internal void MarkTemplateOverridden()
        {
            if (IsTemplate && Default != null)
                Default.TemplateOverridden = true;

            foreach (Column col in Columns)
            {
                if (col.IsTemplate && col.Default != null)
                    col.Default.TemplateOverridden = true;
            }
        }

        //---------------------------------------------------------------------------
        internal void MergeTemplateColumns()
        {
            if (IsTemplate && Default != null)
                return;

            for (int nCol = 0; nCol <= Default.Columns.Count; nCol++)
            {
                Column column = Default.Columns[nCol];

                if (column.IsTemplate && !column.TemplateOverridden)
                {
                    Column pNewTplCol = new Column(column);

                    //pNewTplCol.InternalID = Document.m_pEditorManager.GetNextId(); //TODOLUCA
                    pNewTplCol.Table = this;

                    column.TemplateOverridden = true;

                    Columns.Add(pNewTplCol);

                    // must be done after creation of new column
                    int width = pNewTplCol.ColumnCellsRect.Width;
                    int XOffset = Rect.Width - (pNewTplCol.ColumnCellsRect.Left - Default.Rect.Left);

                    // adjust table title and global size and shift
                    TitleRect = new Rectangle(TitleRect.X, TitleRect.Y, TitleRect.Width + width, TitleRect.Height);
                    BaseCellsRect = new Rectangle(BaseCellsRect.X, BaseCellsRect.Y, BaseCellsRect.Width + width, BaseCellsRect.Height);
                    Rect = new Rectangle(Rect.X, Rect.Y, Rect.Width + width, Rect.Height);

                    // move new column for width of previosus column
                    pNewTplCol.HMoveColumn(XOffset);
                }
            }
        }

        //---------------------------------------------------------------------------
        internal void PurgeTemplateColumns()
        {
            //TODOLUCA
            //for (int nCol = 0; nCol <= m_Columns.GetUpperBound(); )
            //{
            //    //deve rimanere almeno una colonna
            //    if (m_Columns.GetSize() == 1)
            //        break;

            //    TableColumn* pColumn = m_Columns[nCol];
            //    if (!pColumn.m_bTemplate || pColumn.m_bTemplateOverridden)
            //    {
            //        //---- Extract from DeleteColumn(int)
            //        int width = ColumnRect(nCol).Width();
            //        CRect old_rect = m_BaseRect;
            //        if (HasTotal(nCol)) m_nTotalCounter--;
            //        // remove columm from array and delete it
            //        m_Columns.RemoveAt(nCol);
            //        delete pColumn;

            //        // adjust size and position of left positioned columns and modify global rect
            //        // (i.e. m_BaseRect m_BaseCellsRect titleRec)
            //        LeftShiftColumn(nCol, width);
            //        //----
            //        continue;
            //    }
            //    nCol++;
            //}
        }

        //---------------------------------------------------------------------------
        internal void ClearAllStyles()
        {
            foreach (Column col in Columns)
                col.ClearStyle();

            ClearStyle();
        }

        //---------------------------------------------------------------------------
        internal void RemoveAllStyles()
        {
            foreach (Column col in Columns)
                col.RemoveStyle();

            RemoveStyle();
        }

        public object List { get; set; }

        //---------------------------------------------------------------------------
        internal void RenameAlias(int offset)
        {
            InternalID += (ushort)offset;

            foreach (Column col in Columns)
                col.InternalID += (ushort)offset;
        }
    }
}
