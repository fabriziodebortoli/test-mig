using System.Collections.Generic;
using System.Drawing;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using RSjson;

namespace Microarea.TaskBuilderNet.Woorm.WoormViewer
{
	//==============================================================================
	internal class BodyTable
	{
		private Table table;

		private List<SingleRect>			tableColorRects = new List<SingleRect>();
		private List<CellColumnBorderPen>	columnPenCells	= new List<CellColumnBorderPen>();
		private List<CellBodyDiff>			bodyCells		= new List<CellBodyDiff>();
		private List<CellColumnTitleDiff>	titleCells		= new List<CellColumnTitleDiff>();
		private List<CellSubTotalDiff>		subTotalCells	= new List<CellSubTotalDiff>();
		private List<CellTotalDiff>			totalCells		= new List<CellTotalDiff>();
		private Color[]						coinedCellColor = new Color[(int)ElementColor.MAX];
		
		//------------------------------------------------------------------------------
		public BodyTable(Table table)
		{
			this.table = table;
			BuildUnparseInfo();
		}

		//------------------------------------------------------------------------------
		private void BuildUnparseInfo()
		{
			int				nRow, nStartRow = -1;
			bool			isNumeric = false;
			string			dataType;
			BorderPen		borderPen;

			Color[] currentColor = new Color[(int)ElementColor.MAX];
			Color[] baseColor = new Color[(int)ElementColor.MAX];
			
			Column			column = null;
			CounterColor	counterValue = new CounterColor();
			CounterColor	counterBack = new CounterColor();
			
			for (int i = 0; i < table.Columns.Count; i++)
			{
			    column		= table.Columns[i];
			    borderPen	= column.ColumnPen;

			    dataType	= column.GetDataType();
				isNumeric = !ObjectHelper.IsLeftAligned(dataType);
		
			    AddBodyCellAttrib	(column, isNumeric, i);

			    SetTotalCells		(column, isNumeric, i);
			    SetSubTotalCells	(column, isNumeric, i);
			    SetColumnTitleCells	(column, i);
			    SetColumnPenCells 	(borderPen, i);

				baseColor = column.GetCellColor(0);
				counterBack.AddOrIncrement(baseColor[(int)ElementColor.BACKGROUND]);
				counterValue.AddOrIncrement(baseColor[(int)ElementColor.VALUE]);

			    nStartRow = 0;
			    for (nRow = 1; nRow <= table.LastRow; nRow++)
			    {
			        AddBodyCellAttrib (column, isNumeric, i, nRow);
					currentColor = column.GetCellColor (nRow);
					counterBack.AddOrIncrement(currentColor[(int)ElementColor.BACKGROUND]);
					counterValue.AddOrIncrement(currentColor[(int)ElementColor.VALUE]);

			        if	(
							currentColor[(int)ElementColor.VALUE] == baseColor[(int)ElementColor.VALUE] &&
							currentColor[(int)ElementColor.BACKGROUND] == baseColor[(int)ElementColor.BACKGROUND]
			            )
			            continue;

			        AddBodyCellColors (baseColor, i, nStartRow, nRow - 1);

					baseColor[(int)ElementColor.VALUE] = currentColor[(int)ElementColor.VALUE];
					baseColor[(int)ElementColor.BACKGROUND] = currentColor[(int)ElementColor.BACKGROUND];
			        nStartRow = nRow;
			    }
			    AddBodyCellColors (currentColor, i, nStartRow, nRow - 1);
			}

			SetMoreCoined (counterValue.GetColorMaxCounter (), counterBack.GetColorMaxCounter ());
		}

		//------------------------------------------------------------------------------
		internal int NrRect				{ get { return tableColorRects.Count; } }
		internal int NrCellDiff			{ get { return bodyCells.Count; } }
		internal int ColumnPenCells		{ get { return columnPenCells.Count; } }
		internal int NrTitleCells		{ get { return titleCells.Count; } }
		internal int NrSubTotalCells	{ get { return subTotalCells.Count; } }
		internal int NrTotalCells		{ get { return totalCells.Count; } }
		
		//------------------------------------------------------------------------------
		internal void SetColumnTitleCells(Column column, int col)
		{
			int	align = column.ColumnTitleAlign;
			string fontName = column.ColumnTitleFontName;
			BorderPen borderPen	= column.ColumnTitlePen;
			Color[] colors = column.ColumnTitleColor;

			if (
					(
						!borderPen.IsDefault ||
						align != Defaults.DefaultAlign ||
						fontName != DefaultFont.TitoloColonna || 
						colors[(int)ElementColor.LABEL] != Defaults.DefaultColumnTitleForeground ||
						colors[(int)ElementColor.BACKGROUND] != Defaults.DefaultColumnTitleBackground
					) &&
					!(
						titleCells.Count > 0 &&
						titleCells[titleCells.Count - 1].MergeCell(align, fontName, borderPen, colors, col)
					)
				)
				titleCells.Add(new CellColumnTitleDiff(align, fontName, borderPen, colors, col));
		}
		
		//------------------------------------------------------------------------------
		internal void SetColumnPenCells(BorderPen aBorderPen, int col)
		{
			if (
				aBorderPen.IsDefault ||
				(columnPenCells.Count > 0 &&
				columnPenCells[columnPenCells.Count - 1].MergeCell(aBorderPen, col))
				)
				return;

			columnPenCells.Add(new CellColumnBorderPen(aBorderPen, col));
		}
		
		//------------------------------------------------------------------------------
		internal void SetSubTotalCells(Column column, bool isNumeric, int col)
		{ 
			string fontName = column.SubTotalFontName;

			Color[] colors = column.SubTotalColor;

			string tempFont = (isNumeric)
					? DefaultFont.SubTotaleNumerico 
					: DefaultFont.SubTotaleStringa; 

			if (
					(
						tempFont != column.SubTotalFontName ||
						colors[(int)ElementColor.VALUE] != Defaults.DefaultSubTotalForeground ||
						colors[(int)ElementColor.BACKGROUND] != Defaults.DefaultSubTotalBackground
					) &&
					!(
						subTotalCells.Count > 0 &&
						subTotalCells[subTotalCells.Count - 1].MergeCell(fontName, colors, col)
					)
				)
				subTotalCells.Add(new CellSubTotalDiff(fontName, isNumeric, colors, col));
		}
		
		//------------------------------------------------------------------------------
		internal void SetTotalCells(Column column, bool isNumeric, int col)
		{
			int	align = column.TotalAlign;
			string fontName = column.TotalFontName;
			BorderPen	borderPen	= column.TotalPen;
			Color[] colors = column.TotalColor;

			bool differDefault = isNumeric
					? align != Defaults.DefaultTotalNumAlign || fontName != DefaultFont.TotaleNumerico 
					: align != Defaults.DefaultTotalStringAlign || fontName != DefaultFont.TotaleStringa;

			if	(
			        (
						differDefault													||
						!borderPen.IsDefault											||
						colors[(int)ElementColor.VALUE] != Defaults.DefaultTotalForeground ||
						colors[(int)ElementColor.BACKGROUND] != Defaults.DefaultTotalBackground
					) &&
					!(
			            totalCells.Count > 0 &&
						totalCells[totalCells.Count - 1].MergeCell(align, fontName, borderPen, colors, col)
			        )
				)
				totalCells.Add(new CellTotalDiff(align, isNumeric, fontName, borderPen, colors, col));			
		}

		//------------------------------------------------------------------------------
		internal Color[] CoinedCellColor { get { return coinedCellColor; } }

		//------------------------------------------------------------------------------
		internal SingleRect GetNextSingleRect(int nIndex) { return tableColorRects[nIndex]; }

		//------------------------------------------------------------------------------
		internal CellColumnTitleDiff GetNextTitleCell(int nIndex) { return titleCells[nIndex]; }

		//------------------------------------------------------------------------------
		internal CellColumnBorderPen GetNextColumnPen(int nIndex) { return columnPenCells[nIndex]; }

		//------------------------------------------------------------------------------
		internal CellBodyDiff GetNextCellDiff(int nIndex) { return bodyCells[nIndex]; }

		//------------------------------------------------------------------------------
		internal CellSubTotalDiff GetNextSubTotalCell(int nIndex) { return subTotalCells[nIndex]; }

		//------------------------------------------------------------------------------
		internal CellTotalDiff GetNextTotalCell(int nIndex) { return totalCells[nIndex]; }

		//------------------------------------------------------------------------------
		internal void AddBodyCellColors(Color[] aColors, int col, int startRow, int endRow)
		{
			for (int index = 0; index < tableColorRects.Count; index++)
				if (tableColorRects[index].MergeRectColors(aColors, col, startRow, endRow))
					return;

			tableColorRects.Add(new SingleRect(aColors, col, startRow, endRow));
		}

		//------------------------------------------------------------------------------
		internal void AddBodyCellAttrib(Column column, bool isNumeric, int col, int row = 0)
		{
			int align = column.GetCellAlign(row);
			string fontName = column.GetCellFontName(row);

			bool differDefault = isNumeric
				? align != Defaults.DefaultCellNumAlign || fontName != DefaultFont.CellaNumerica 
				: align != Defaults.DefaultCellStringAlign || fontName != DefaultFont.CellaStringa; 

			if (
					differDefault &&
					(!(
						bodyCells.Count > 0 &&
						bodyCells[bodyCells.Count - 1].MergeCell(align, fontName, col, row)
					))
				)
				bodyCells.Add(new CellBodyDiff(align, fontName, isNumeric, col, row));
		}

		//------------------------------------------------------------------------------
		internal void SetMoreCoined(Color color, Color background)
		{
			coinedCellColor[(int)ElementColor.VALUE] = color;
			coinedCellColor[(int)ElementColor.BACKGROUND] = background;
			coinedCellColor[(int)ElementColor.BORDER] = color;
		}
	}

	//==============================================================================
	internal class CellColumnBorderPen
	{
		BorderPen borderPen;
		Rectangle rect;
		bool isNumeric = false;

		internal int		PenWidth		{ get { return borderPen.Width; } }
		internal BorderPen	BorderPen		{ get { return borderPen; } }
		internal Rectangle	Rect			{ get { return rect; } }
		internal bool		CellIsNumeric	{ get { return isNumeric; } }

		//------------------------------------------------------------------------------
		public CellColumnBorderPen(BorderPen borderPen, int col)
		{
			this.borderPen = borderPen;
			rect = new Rectangle(col, rect.Top, 0, 0);
		}

		//------------------------------------------------------------------------------
		internal bool MergeCell(BorderPen borderPen, int col)
		{
			if (
				this.borderPen.Width == borderPen.Width &&
				this.borderPen.Color == borderPen.Color &&
				rect.Right == (col - 1)
				)
			{
				rect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height);  
				return true;
			}
			return false;
		}
	}
	
	//==============================================================================
	internal class CellDifferences
	{
		private string fontName;
		private int align;
		private bool isNumeric = false;
		private Rectangle rect;

		//------------------------------------------------------------------------------
		internal int		Align			{ get { return align; } }
		internal string		FontName		{ get { return fontName; } }
		internal Rectangle	Rect			{ get { return rect; } }
		internal bool		CellIsNumeric	{ get { return isNumeric; } }

		//------------------------------------------------------------------------------
		public CellDifferences(int align, string fontName, bool isNumeric, int col, int startRow = 0)
		{
			this.align = align;
			this.fontName = fontName;
			this.isNumeric = isNumeric;

			rect = new Rectangle(col, startRow, 0, 0);
		}
		
		//------------------------------------------------------------------------------
		internal bool IsSameRectRight(int right)
		{
			return (rect.Right == right);
		}
		
		//------------------------------------------------------------------------------
		internal bool IsSameRectLeft(int left)
		{
			return (rect.Left == left);
		}
		
		//------------------------------------------------------------------------------
		internal bool IsSameRectBottom(int bottom)
		{
			return (rect.Bottom == bottom);
		}
		
		//------------------------------------------------------------------------------
		internal bool IncRectRight()
		{
			rect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height); 
			return true;
		}
		
		//------------------------------------------------------------------------------
		internal bool IncRectBottom()
		{
			rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + 1); 
			return true;
		}
	}

	//==============================================================================
	internal class CellBodyDiff : CellDifferences
	{
		//------------------------------------------------------------------------------
		public CellBodyDiff(int align, string fontName, bool isNumeric, int col, int startRow)
			: base(align, fontName, isNumeric, col, startRow)
		{

		}

		//------------------------------------------------------------------------------
		internal bool MergeCell(int align, string fontName, int col, int nRow)
		{
			if (
				Align == align &&
				FontName == fontName &&
				IsSameRectLeft(col) &&
				IsSameRectBottom(nRow - 1)
				)
				return IncRectBottom();

			return false;
		}
	}

	//==============================================================================
	internal class CellSubTotalDiff : CellDifferences
	{
		Color[] colors = new Color[(int)ElementColor.MAX];
		//------------------------------------------------------------------------------
		public CellSubTotalDiff(string fontName, bool isNumeric, Color[] aColors, int col)
			: base(Defaults.DefaultAlign, fontName, isNumeric, col)
		{
			this.colors[(int)ElementColor.VALUE] = aColors[(int)ElementColor.VALUE];
			this.colors[(int)ElementColor.BACKGROUND] = aColors[(int)ElementColor.BACKGROUND];
		}

		//------------------------------------------------------------------------------
		internal Color[] Colors { get { return colors; } }

		//------------------------------------------------------------------------------
		internal bool MergeCell(string fontName, Color[] aColors, int col)
		{ 
			if  (
			    FontName == fontName &&
				colors[(int)ElementColor.VALUE] == aColors[(int)ElementColor.VALUE] &&
				colors[(int)ElementColor.BACKGROUND] == aColors[(int)ElementColor.BACKGROUND] &&
			    IsSameRectRight (col -1)
			    )
			    return IncRectRight ();

			return false;
		}
	}

	//==============================================================================
	internal class  CellGeneralDiff : CellDifferences
	{
	    BorderPen	borderPen;

		internal int		PenWidth	{ get { return borderPen.Width; } }
		internal BorderPen	BorderPen	{ get { return borderPen; } }

		//------------------------------------------------------------------------------
		public CellGeneralDiff(int align, bool isNumeric, string fontName, BorderPen borderPen, int col)
			: base(align, fontName, isNumeric, col)
		{
			this.borderPen = borderPen;
		}

		
		//------------------------------------------------------------------------------
		internal bool MergeCell(int align, string fontName, BorderPen borderPen, int col)
		{
			return
				this.borderPen.Width == borderPen.Width &&
				this.borderPen.Color == borderPen.Color &&
				Align == align &&
				FontName == fontName &&
				IsSameRectRight(col - 1);
		}
	}

	//==============================================================================
	internal class CellColumnTitleDiff : CellGeneralDiff
	{
		Color[] colors = new Color[(int)ElementColor.MAX];

		//------------------------------------------------------------------------------
		internal Color[] Colors { get { return colors; } }

		//------------------------------------------------------------------------------
		public CellColumnTitleDiff(int align, string fontName, BorderPen borderPen, Color[] aColors, int col)
			: base(align, false, fontName, borderPen, col)
		{
			this.colors[(int)ElementColor.LABEL] = aColors[(int)ElementColor.LABEL];
			this.colors[(int)ElementColor.BACKGROUND] = aColors[(int)ElementColor.BACKGROUND];
			this.colors[(int)ElementColor.BORDER] = aColors[(int)ElementColor.BORDER];
		}

		
		//------------------------------------------------------------------------------
		internal bool MergeCell(int align, string fontName, BorderPen borderPen, Color[] aColors, int col)
		{ 
			if  (   
			    base.MergeCell(align, fontName, borderPen, col) &&
				colors[(int)ElementColor.LABEL] == aColors[(int)ElementColor.LABEL] &&
				colors[(int)ElementColor.BACKGROUND] == aColors[(int)ElementColor.BACKGROUND] 
			    )
			    return IncRectRight();

			return false;
		}
	}

	//==============================================================================
	internal class CellTotalDiff : CellGeneralDiff
	{
		Color[] colors = new Color[(int)ElementColor.MAX];

		public CellTotalDiff(int align, bool isNumeric, string fontName, BorderPen borderPen, Color[] aColors, int col)
			: base(align, isNumeric, fontName, borderPen, col)
		{
			this.colors[(int)ElementColor.VALUE] = aColors[(int)ElementColor.VALUE];
			this.colors[(int)ElementColor.BACKGROUND] = aColors[(int)ElementColor.BACKGROUND];
			this.colors[(int)ElementColor.BORDER] = aColors[(int)ElementColor.BORDER];
		}

		//------------------------------------------------------------------------------
		internal Color[] Colors { get { return colors; } }

		//------------------------------------------------------------------------------
		internal bool MergeCell(int align, string fontName, BorderPen borderPen, Color[] aColors, int col)
		{
			if  (   
				base.MergeCell(align, fontName, borderPen, col)				&&
				colors[(int)ElementColor.VALUE] == aColors[(int)ElementColor.VALUE] &&
				colors[(int)ElementColor.BACKGROUND] == aColors[(int)ElementColor.BACKGROUND]
			    )
			    return IncRectRight();

			return false;
		}
	}

	//==============================================================================
	internal class CounterColorElement
	{
		Color color;
		int counter;

		internal int	Counter { get { return counter; } }
		internal Color	Color	{ get { return color; } }

		//------------------------------------------------------------------------------
		internal void IncrementCounter()
		{
			counter++;
		}
		
		//------------------------------------------------------------------------------
		public CounterColorElement(Color color)
		{
			this.color = color;
			counter = 1;
		}
	}

	//==============================================================================
	internal class CounterColor
	{
		List<CounterColorElement> counters = new List<CounterColorElement>();

		//------------------------------------------------------------------------------
		public CounterColor() {}

		//------------------------------------------------------------------------------
		internal void AddOrIncrement(Color color)
		{
			foreach (CounterColorElement item in counters)
			{
				if (item.Color == color)
				{
					item.IncrementCounter();
					return;
				}
			}
			counters.Add(new CounterColorElement(color));
		}

		//------------------------------------------------------------------------------
		internal Color GetColorMaxCounter()
		{
			Color rgbColor = counters[0].Color;
			int nMax = counters[0].Counter;

			foreach (CounterColorElement item in counters)
			{
				if (item.Counter == nMax)
				{
					nMax = item.Counter;
					rgbColor = item.Color;
				}
			}
			return rgbColor;
		}
	}

	// Regione della tabella con stesse caratteristiche di colore
	//==============================================================================
	internal class SingleRect
	{
		Color[] cellColors = new Color[(int)ElementColor.MAX];
		Rectangle rect;

		//------------------------------------------------------------------------------
		public SingleRect(Color[] cellColors, int col, int startRow, int endRow)
		{
			this.cellColors[(int)ElementColor.VALUE] = cellColors[(int)ElementColor.VALUE];
			this.cellColors[(int)ElementColor.BACKGROUND] = cellColors[(int)ElementColor.BACKGROUND];
			this.cellColors[(int)ElementColor.BORDER] = cellColors[(int)ElementColor.BORDER];

			rect = new Rectangle(col, startRow, endRow - startRow, 0);
		}

		//------------------------------------------------------------------------------
		internal Color[] CellColor { get { return cellColors; } }
		
		//------------------------------------------------------------------------------
		internal Rectangle Rectangle { get { return rect; } }
		
		//------------------------------------------------------------------------------
		internal bool MergeRectColors(Color[] aCellColors, int col, int startRow, int endRow)
		{ 
			if	(
				cellColors[(int)ElementColor.VALUE] == aCellColors[(int)ElementColor.VALUE] &&
				cellColors[(int)ElementColor.BACKGROUND] == aCellColors[(int)ElementColor.BACKGROUND] &&
				(rect.Top == startRow && (rect.Right + 1) == col && rect.Bottom == endRow)
			    )
			{
				rect = new Rectangle(rect.X, rect.Y, rect.Width + 1, rect.Height);
				return true;
			}
			return false;
		}
	}
}
