
#pragma once

#include <TbGeneric\FontsTable.h>	// a causa di FontIdx
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>	// a causa di FormatIdx

#include "baseobj.h"
#include "textobj.h"
#include "listdlg.h"
#include "bordpen.h"
#include "Chart.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ViewParser;
class ViewUnparser;
class TableCell;
class TotalCell;
class TotalColumn;
class Table;
class BodyTable;
class RDEManager;
class CellDifferences;
class CellSubTotalDiff;
class CellTotalDiff;
class CellColumnTitleDiff;
class CellColumnBorderPen;
class ActiveColumn;

//===========================================================================
enum EEasyReading { ER_NONE, ER_COLOR, ER_LINE };
	
enum EnumDynamicExpr		
	{ 
	TextColorTitle, TextColorCell, TextColorSubTotal, TextColorTotal, 
	BkgColorTitle, BkgColorCell, BkgColorSubTotal, BkgColorTotal,
	TitleExpr, TitleTooltipExpr, TextFontExpr, CellBordersExpr, CellTooltipExpr
	};



// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CColumnArray : public CObArray
{
public:
	CColumnArray ()					 {}
	CColumnArray (CColumnArray& src) {Copy(src);}

public:
	// overloaded operator helpers
	TableColumn* operator[](int nIndex) const	{ return (TableColumn*) GetAt(nIndex); }
	TableColumn*& operator[](int nIndex)		{ return (TableColumn*&) ElementAt(nIndex); }
};     

// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CActiveColumnArray : public CObArray
{
public:
	CActiveColumnArray() {}
	CActiveColumnArray(CActiveColumnArray& src) { Copy(src); }

public:
	// overloaded operator helpers
	ActiveColumn* operator[](int nIndex) const		{ return (ActiveColumn*)GetAt(nIndex); }
	ActiveColumn*& operator[](int nIndex)			{ return (ActiveColumn*&)ElementAt(nIndex); }
	
};


//===========================================================================
class CBCGPChartCtrl;

class TB_EXPORT Table : public BaseObj
{
	friend	TableColumn;
	friend	TableCell;
	friend	TotalCell;
	friend	BodyTable;
	friend	CWoormDocMng;
	friend class  CRSTableHeightsProp;
	friend class  CRSTableHeightsPropMM;
	friend class  CRSHiddenProp;
	friend class  CRSTableMultiColumnsBoolProp;

	DECLARE_DYNAMIC (Table)
public:
	// public enum for borland 3.1 bugs
	enum ActionType
		{
		LEAVE,		MOVE,		ADD_ROW,	/* Table movement 					*/
		CELL_HSIZE,	CELL_VSIZE,	CELL_SIZE,	/* cell Resize						*/
		COL_MOVW,	COL_MOVE,				/* move column west-est				*/
		T_VSIZE,							/* Table Title Resize (nord-sud)	*/
		CT_VSIZE,							/* column Title Resize (nord-sud)	*/
		TOT_VSIZE							/* column Title Resize (nord-sud)	*/
		};
		
	enum SelPosition
		{
		POS_WHOLE_TABLE,					/* Table 							*/
		POS_COLUMN,							/* Column							*/
		POS_ROW								/* Row								*/
		};
		
	// positive value indidcate correct column or nRow
	static const int NO_ACTIVE_COLUMN;
	static const int ACTIVE_IS_TITLE;
	static const int ACTIVE_IS_TOTAL;

private:
	// private static data
	static const int	CELL_HEIGHT;
	static const int	CELL_WIDTH;
	static const int	COLUMN_TITLE_HEIGHT;
	static const int	TITLE_HEIGHT;
public:
//PROPERTIES
	CRect			m_BaseCellsRect; 	// include table title, columns titles and all cells, only DOES NOT include TotalHeight
										//come property interessa solo la posizione

	CRect			m_TitleRect;		//come property interessa solo l'altezza
	
	TableBorders	m_Borders;
	BorderPen		m_TitlePen;

	TableTitleText	m_Title;	
							/* eredita:
									CString		m_strTitle - m_strText //TODO (Riccardo) continene due stringhe, capire se servono entrambe
												//valorizzare m_strTitle con il metodo SetText (o chiamarlo comunque dopo)
									COLORREF	m_rgbTextColor;
									COLORREF	m_rgbBkgColor;
									AlignType	m_nAlign;
									FontIdx		m_nFontIdx;
							*/
	SubTotalStyle	m_SubTitle;

	Expression*		m_pHideExpr;

	BOOL			m_bHideTableTitle;
	BOOL			m_bHideColumnsTitle;

	BOOL			m_bFiscalEnd;	// abilita la tracciatura  di una z a fine tabella

	int				m_nDropShadowHeight;
	COLORREF		m_crDropShadowColor;

	//righe con sfondo di colore alternato: tre valori: no, color, color dynamic 
	BOOL			m_bAlternateBkgColorOnRow;
	BOOL			m_bAlternateBkgColorOnMultiLineRow;
	COLORREF		m_crAlternateBkgColorOnRow;

	CString			m_sStyleClass;		

//----- END PROPERTIES

	CColumnArray	m_Columns;  

//-----     
	CWordArray		m_Interlines;
	CWordArray		m_LineWithTitles;
	CStringArray	m_LineWithCustomTitles;

	BOOL			m_bAlternateEasyview;
	CArray<int, int> m_arAlternateEasyviewOnPage;

	CDWordArray		m_arRowAlignInitial;

	int		m_nRowHeightZoom;
	int		m_nLastTitleHeight;
	int		m_nActiveColumn;
	int		m_nActiveRow;
	int		m_nCurrentRow;		// RDE - indicate active nRow for reading data
	CPoint	m_ptCellOrigin;		// cell ptOrigin when starting Resize
	int		m_nTotalCounter;	// indicata se la tabella a m_Columns con totali (0 = nototal)
	
	BOOL			m_bTemplate;
	BOOL			m_bTemplateOverridden;
	Table*			m_pDefault;

public:
	int				m_nViewCurrentRow;
	
	virtual int		GetViewCurrentRow	() const { return m_nViewCurrentRow; }

	BOOL EasyReadingByLineEnabled ();
	BOOL EasyReadingByLineDynamicEnabled ();
	BOOL EasyReadingByColorEnabled ();
	BOOL EasyReadingByColorDynamicEnabled ();

	BOOL ShowTitles(BOOL bPrinting);
	BOOL NoBorders(BOOL bPrinting);
	BOOL isMarkExportable();

protected:
	// protected function
	ActionType	m_nAction;
	ActionType	TitleAction		(const CRect&, CPoint);
	ActionType	ColumnAction	(const CRect&, CPoint, int nCol);
	ActionType	CellAction		(const CRect&, CPoint, int nRow, int nCol);
	ActionType	TableAction		(CPoint);

	// protected function
	void	ChangeHSize		(int nSize);
	void	ChangeVSize		(int nSize);

	CRect	TitleActionRect		(ActionType);
	CRect	ColumnActionRect	(ActionType, int nCol);
	CRect	TotalActionRect		(ActionType, int nCol);
	CRect	CellActionRect		(ActionType, int nRow, int nCol);
	CRect	NormalizedColumnRect (int nCol) const;

	// protected function
	void	SetObjCursor		(ActionType);
	void	SetObjCursor		();
	BOOL	Action				(CPoint);
	void	BeforeAction		();
	void	AfterAction			();
	void 	ClipActionCursor	(CDC&, CPoint, CBCGPScrollView*);
	void	MyClipCursor		(CDC& dc, CBCGPScrollView* pView, CRect rect);
	void 	SetActionCursor		(CPoint);

	// showing flags
	
	BOOL	ColumnIsMultiRow	() const;
	BOOL	ColumnIsBitmap		() const;
	BOOL	ColumnIsBarCode		() const;
	BOOL	ColumnIsTextFile	() const;
	BOOL	ColumnCanBeMultiRow	() const;
	BOOL	ColumnCanBeBitmap	() const;
	BOOL	ColumnCanBeTextFile	() const;
	BOOL	ColumnCanBeBarCode	() const;

	WORD			GetActiveColumnId	() const;// { return m_Columns[m_nActiveColumn]->GetInternalID(); }
	TableColumn*	GetActiveColumn		() const { return m_Columns[m_nActiveColumn]; }

	//Printer Attributes
	BOOL	ColumnIsAnchorLeft		() const;
	BOOL	ColumnIsAnchorRight		() const;
	BOOL	ColumnIsSplitter		() const;
	BOOL	ColumnIsPinned			() const;
	BOOL	ColumnIsHiddenWhenEmpy	() const;
	BOOL	ColumnIsWidthOptimized	() const;

	// manage creation of objects (datafield etc..) in table
	BOOL	EditorManagerCreateField(int nIDM, CWordArray& ids, TblRuleData* pTblRule = NULL);

	// generic function
	int			NormalizedRow			() const;
	int			NormalizedColumn		() const;
	void		RebuildTableSizes		(CPoint ptOrigin, int hTableTitle, int hColTitle, int hRow, int hTotal);

	// drawing functions
	void	DrawTitles			(CDC&, CRect, BOOL bPreview);
	void	DrawRows			(CDC&, CRect, BOOL bPreview, BOOL bMarkExportable);
	void	DrawTotals			(CDC&, CRect, BOOL bPreview);
	void	DrawFiscalEnd		(CDC&, CRect, BOOL bPreview);
	void	DrawColumns			(CDC&, CRect, BOOL bPreview);
	

	// virtual private functioon
	virtual void OnTrackingDraw	(CDC&, CBCGPScrollView* pView, BOOL bShowRect);
	virtual void Resize			(CPoint, CPoint);
	
	// file conversion functions
	void	CopyTable				();
	void	CopyActiveColumn		();

	BOOL	CheckHiddenStatus		(int nCol);
	BOOL	CheckDynamicWidth		(int nCol);
	void	SetColumnHiddenStatus	(int nCol, BOOL bHidden);
	
	CString	NumToString				(TableCell&, const DataType&);

	// parse & Unparse local methods
	BOOL 	ParseTableTitleOption	(ViewParser&, BOOL blk = TRUE);
	BOOL	ParseTableTitle			(ViewParser&);

	BOOL	ParseTableSubTitleOption	(ViewParser&, BOOL blk = TRUE);
	BOOL	ParseTableSubTitle			(ViewParser&);

	BOOL	ParseTitleAndBorders	(ViewParser&);
	BOOL	ParseAllColumns			(ViewParser&);
	BOOL	ParseColumnsBlock		(ViewParser&);
	BOOL	ParseTableAttrib		(ViewParser&); 

	BOOL	ParseColumnPens 		(ViewParser&); 
	BOOL	ParsePens	 			(ViewParser&); 
	BOOL	ParseSingleColumnPen 	(ViewParser&);
	BOOL	ParseColumnPen		 	(BorderPen*, ViewParser&);

	BOOL	ParseSubTotalCells 		(ViewParser&);
	BOOL	ParseTotalCells 		(ViewParser&);
	BOOL	ParseColumnTitleCells 	(ViewParser&); 

	BOOL	ParseSubTotals 			(COLORREF*, ViewParser&);
	BOOL	ParseTotals 			(COLORREF*, ViewParser&);
	BOOL	ParseColumnTitles 		(COLORREF*, ViewParser&);  
	
	BOOL	ParseSingleSubTotal 	(COLORREF*, ViewParser&);
	BOOL	ParseSingleTotal	 	(COLORREF*, ViewParser&);
	BOOL	ParseSingleColumnTitle	(COLORREF*, ViewParser&); 
	
	BOOL	ParseSubTotalDifferences(const CRect&, ViewParser&);
	BOOL	ParseTotalDifferences	(const CRect&, Token, ViewParser&);
	BOOL	ParseColumnTitleDifferences	(const CRect&, Token, ViewParser&);  
	
	BOOL	ParseSubTotalFontIdx 	(const CRect&, ViewParser&);
	BOOL	ParseTotalFontIdx	 	(const CRect&, ViewParser&);
	BOOL	ParseColumnTitleFontIdx	(const CRect&, ViewParser&);
	BOOL	ParseTotalAlign 		(const CRect&, ViewParser&);
	BOOL	ParseColumnTitleAlign 	(const CRect&, ViewParser&);
	
	BOOL	ParseBodyVariations		(ViewParser&);
	BOOL	ParseVariations			(COLORREF*, ViewParser&);
	BOOL	ParseVariation			(COLORREF*, ViewParser&, BOOL);
	BOOL	ParseColorAttributes	(COLORREF*, ViewParser&);
	BOOL	ParseRectSet			(COLORREF*, ViewParser&);
	BOOL	ParseRectColorAttributes(COLORREF*, CRect&, ViewParser&); 
	BOOL	ParseCellDiff			(ViewParser&);
	BOOL	ParseCellIndex			(CRect*, ViewParser&);
	BOOL	ParseColumnIndex		(CRect*, ViewParser&); 
	
	BOOL	ParseDifferences		(const CRect&, ViewParser&);
	BOOL	ParseCellFontIdx		(const CRect&, ViewParser&);
	BOOL	ParseFontIdx			(CString&, ViewParser&);
	BOOL	ParseCellAlign			(const CRect&, ViewParser&);
		
	void	UnparseTitle			(ViewUnparser&);
	void	UnparseSubTitle			(ViewUnparser&);
	void	UnparseTitleAndBorders	(ViewUnparser&);

	void	UnparseAllColumns		(ViewUnparser&);
	void	UnparseColumnsBlock		(ViewUnparser&);
	void	UnparseTableAttrib		(ViewUnparser&);

	void 	UnparseColumnPens		(const BodyTable&, ViewUnparser&);
	void	UnparseSingleColumnPen	(const BorderPen&, ViewUnparser&);
	void	UnparseColumnTitleCells	(const BodyTable&, ViewUnparser&);
	void 	UnparseSubTotalCells	(const BodyTable&, ViewUnparser&);
	void	UnparseTotalCells		(const BodyTable&, ViewUnparser&);

	void	UnparseSingleSubTotal	(CellSubTotalDiff&, ViewUnparser&);
	void	UnparseSingleTotal		(CellTotalDiff&, ViewUnparser&);
	void	UnparseSingleColumnTitle(CellColumnTitleDiff&, ViewUnparser&);
	void	UnparseBodyVariations	(const BodyTable&, ViewUnparser&);
	void	UnparseVariations		(BOOL, BOOL, int, int, const BodyTable&, ViewUnparser&);
	void	UnparseMoreCoined		(BOOL, BOOL, COLORREF*, ViewUnparser&); 
	void	UnparseAttribute		(BOOL, BOOL, BOOL, COLORREF txt, COLORREF bkg, COLORREF brd, ViewUnparser&);	       
	void	UnparseRectSet			(int, int, const BodyTable&, ViewUnparser&);
	void	UnparseSingleRect		(const BodyTable&, int index, ViewUnparser&);
	void	UnparseDifferentCells	(const BodyTable&, ViewUnparser&);
	void	UnparseCellIndex		(const CRect&, ViewUnparser&);              
	void	UnparseColumnIndex		(const CRect&, ViewUnparser&);
	void	UnparseDifferences		(BOOL, BOOL, FontIdx, AlignType, ViewUnparser&); 
	void	UnparseSingleFont		(FontIdx, ViewUnparser&); 

	BOOL	ParseHidden				(ViewParser& lex);
	void	UnparseHidden			(ViewUnparser& ofile);

public:
	// constructor for new table
	Table	(
				CPoint, CWoormDocMng*,
				int nRows, int nCols,
				WORD wTableId , CWordArray& wColumnIds
			);

	// constructor for table added by parsing them from file
	Table	(CPoint, CWoormDocMng*, int nRows, int nCols);
	Table	(const Table&);
	virtual	~Table	();
	virtual BaseObj* Clone() const { return new Table(*this); }

	void ClearCellDynamicAttributes (BOOL bBrowsed = TRUE, BOOL bSelected = TRUE, BOOL bError = TRUE);
	
	CRect		NormalizedBaseRect		(BOOL bCheckTitle = FALSE) const;

	// public functions
	int		NextVisibleColumn	(int nCol) const;
	int		TitlesHeight		() const;	// Table Title and column Title height
	int		HiddenColumnNumber	() const;	// totale delle colonne nascoste
	int		VisibleColumnNumber	() const;	// totale delle colonne visibili
	int		LastVisibleColumn	() const;	// tiene conto delle colonne Hidden in fondo alla tabella
	int		FirstVisibleColumn	() const;	// tiene conto delle colonne Hidden in fondo alla tabella
	BOOL	ExistsVisibleColumn	() const;
	int		LastColumn			() const;
	int		LastRow				() const;
	virtual int 	RowsNumber			() const;	
	int 	RowHeight			() const;
	int		GetCurrentRow		() const { return m_nCurrentRow + 1; }	
	int		GetActiveRow		() const { return m_nActiveRow + 1;	}	

	CRect	CellRect			(int nRow, int nCol) const;	
	CRect	ColumnTitleRect		(int nCol) const;
	CRect	ColumnCellsRect		(int nCol) const;
	CRect	TotalRect			(int nCol) const;
	CRect	ColumnRect			(int nCol) const;
	CRect	RowRect				(int nRow) const;
	CRect	AllTotalsRect		() const;
	CRect	AllColumnsTitleRect	() const;
	CRect	BodyRect			() const;
	CRect	TitleRect			() const;

	void 	MoveTable			(CSize delta);
	void 	VMoveTotals			(int height);
	void	VExpandRow			(int nRow, int height);
	void	HExpandColumn		(int nCol, int width, BOOL bUpdateDefaultWidth);
	void	VExpandTotal		(int height);
	void	VExpandColumnTitle	(int height);
	void	VExpandTitle		(int height);

	void	RightShiftColumn	(int nCol, int width);
	void	LeftShiftColumn		(int nCol, int width);

	BOOL	CanVExpandRow			(int nRow, int height) const;
	BOOL	CanHExpandColumn		(int nCol, int width) const;
	BOOL	CanVExpandTotal			(int height) const;
	BOOL	CanVExpandColumnTitle	(int height) const;
	BOOL	CanVExpandTitle			(int height) const;

	void	ColumnToggleHidden		(int nCol);  //drawing function
	void	OnColumnManageLink		(int nCol);

	void	OnUpdatePlusMultiLines	();
	void	OnUpdateMinusMultiLines ();

	//implementation of baseobj's virtual method to update layout object
	void				Redraw			();
	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*	GetBkgColor			()								{ return NULL; }
	virtual COLORREF*	GetLabelForeColor	()								{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()								{ return NULL; }
	virtual CString		GetWrmStyleClass	()								{ return m_sStyleClass; }
	virtual void		SetWrmStyleClass	(CString styleClass)			{ m_sStyleClass = styleClass; }
	virtual void		SetWrmStyleClass	(GenericDrawObj* templateObj)	{ SetStyle(dynamic_cast<Table*>(templateObj)); }

	// column manipulation functions
	void	AddColumn			(CWordArray& ids, TblRuleData* pTblRule = NULL);
	void	AddColumn			(int nIDM, TblRuleData* pTblRule = NULL);
	BOOL	MoveColumn			(BOOL bLeft);
	BOOL	MoveColumn			(int nFromAlias, int nToAlias);
	void	AddColumnTotal		();

	void	DeleteColumnComplete	(int nCol, BOOL bUpdateDoc = TRUE);
		void	DeleteColumnAll		(int nCol);
		void	DeleteColumnTotal	(int nCol);
		BOOL	DeleteColumnBody	(int nCol);

	BOOL	NoActiveColumn		() const;
	BOOL	MoreThanOneColumn	() const;
	
	BOOL		SetTotalRectActiveById(int nID);
	TotalCell*	GetTotalCellById(int nID, int& nCol);


	BOOL	ActiveColumnHasTotal		() const { return HasTotal(m_nActiveColumn); }
	BOOL	ActiveColumnCanHaveTotal	() const;

	BOOL	HasTotal	() const;
	BOOL	HasTotal	(int nCol) const;

	int		CalcRowsNumber		();
	void	ModifyRowsNumber	(int);

	// clipboard management
	virtual	void CopyClipboard	();

	//	reading attribute function member
	BorderPen	GetColumnPen		(int nCol) const;
	BorderPen	GetTotalPen			(int nCol) const;
	BorderPen	GetColumnTitlePen	(int nCol) const;

	void		GetColumnTitleColor	(COLORREF*, int nCol) const;
	void		GetSubTotalColor	(COLORREF*, int nCol) const;
	void		GetTotalColor		(COLORREF*, int nCol) const;
	void		GetTableTitleColor	(COLORREF*) const;
	void		GetCellColor		(COLORREF*, int nRow, int nCol) const;

	AlignType	GetColumnTitleAlign	(int nCol) const;
	AlignType	GetTableTitleAlign	() const;
	AlignType	GetColumnAlign		(int nCol) const;
	AlignType	GetTotalAlign		(int nCol) const;
	AlignType	GetCellAlign		(int nRow, int nCol) const;

	FontIdx		GetColumnTitleFontIdx	(int nCol) const;
	FontIdx		GetTableTitleFontIdx	() const;
	FontIdx		GetColumnFontIdx		(int nCol) const;
	FontIdx		GetSubTotalFontIdx		(int nCol) const;
	FontIdx		GetTotalFontIdx			(int nCol) const;
	FontIdx		GetCellFontIdx			(int nRow, int nCol) const;

	FormatIdx	GetColumnFormatIdx		(int nCol) const;
	CString		GetColumnTitleText		(int nCol) const;
	CString		GetColumnLocalizedTitleText (int nCol, CBaseDocument*) const;
	CString		GetDynamicColumnLocalizedTitleText (int nCol);

	CString		GetTableTitleText		() const;
	CString		GetTableLocalizedTitleText (CBaseDocument*) const;

	TableCell*	GetTableCell 		(int nRow, int nCol) const;
	TableCell*	GetCellFromID		(int nRow, WORD wID) const;
	TotalCell*	GetTotalCell 		(int nCol) const;
	
	const CColumnArray&		GetColumns				()	{ return m_Columns; }       
	const TableColumn*		GetColumn				(int nCol) const;
	TableColumn*			GetColumnByAlias		(WORD nAlias, int* pIdx = NULL) const;
	int						GetColumnIndexByAlias	(WORD nAlias) const;
	int						GetColumnIndexByPtr		(TableColumn* pCol) const;

	void	SetColumnPen			(const BorderPen&, int nCol);
	void	SetTotalPen				(const BorderPen&, int nCol);
	void	SetColumnTitlePen		(const BorderPen&, int nCol);
	void	SetAllColumnsPen		(const BorderPen&);
	void	SetAllTotalsPen			(const BorderPen&);
	void	SetAllColumnsTitlePen	(const BorderPen&);

	void	SetTotalColor				(COLORREF*, int nCol);
	void	SetTotalTextColor			(COLORREF color, int nCol);
	void	SetTotalBkgColor			(COLORREF color, int nCol);
	void	SetTotalBorderColor			(COLORREF color, int nCol);
	void	SetTotalBorderWidth			(int width, int nCol);

	void	SetColumnTitleColor			(COLORREF*, int nCol);
	void	SetColumnTitleTextColor		(COLORREF color, int nCol);
	void	SetColumnTitleBkgColor		(COLORREF color, int nCol);
	void	SetColumnTitleBorderColor	(COLORREF color, int nCol);
	void	SetColumnTitleBorderWidth	(int width, int nCol);

	void	SetSubTotalColor			(COLORREF*, int nCol);
	void	SetAllSubTotalsBkgColor		(COLORREF color);
	void	SetAllSubTotalsTextColor	(COLORREF color);
	void	SetAllSubTotalsColor		(COLORREF*);
	
	void	SetAllTotalsColor			(COLORREF*);
	void	SetAllTotalsTextColor		(COLORREF color);
	void	SetAllTotalsBkgColor		(COLORREF color);
	void	SetAllTotalsBorderColor		(COLORREF color);
	void	SetAllTotalsBorderWidth		(int width);

	void	SetAllColumnsTitleColor		(COLORREF*);
	void	SetAllColumnTitleTextColor	(COLORREF color);
	void	SetAllColumnTitleBkgColor	(COLORREF color);
	void	SetAllColumnTitleBorderColor(COLORREF color);
	void	SetAllColumnTitleBorderWidth(int width);

	void	SetTableTitleColor			(COLORREF*);

	void	SetBodyBkgColor				(COLORREF color, int nCol);
	void	SetBodyTextColor			(COLORREF color, int nCol);
	void	SetBodyBorderWidth			(int width);

	void	SetAllBodyBkgColor			(COLORREF color);
	void	SetAllBodyTextColor			(COLORREF color);
	
	void	SetAllColumnDynamicExpr		(int nActiveColumn, EnumDynamicExpr eObject, Expression* pExpr, BOOL& bAborted);

	void	SetCellColor			(COLORREF*, int nRow, int nCol);
	void 	SetRowColor				(COLORREF*, int nRow);
	void	SetColumnCellsColor		(COLORREF*, int nCol);
	void	SetAllCellsColor		(COLORREF*);

	void	SetColumnAlign			(AlignType, int nCol);
	void	SetTotalAlign			(AlignType, int nCol);
	void	SetColumnTitleAlign		(AlignType, int nCol);
	void	SetAllColumnsAlign		(AlignType);
	void	SetAllTotalsAlign		(AlignType);
	void	SetAllColumnsTitleAlign	(AlignType);
	void	SetTableTitleAlign		(AlignType);

	void	SetCellAlign			(AlignType, int nRow, int nCol);
	void 	SetRowAlign				(AlignType, int nRow);

	void	SetColumnFontIdx			(FontIdx, int nCol);
	void	SetSubTotalFontIdx			(FontIdx, int nCol);
	void	SetTotalFontIdx				(FontIdx, int nCol);
	void	SetColumnTitleFontIdx		(FontIdx, int nCol);
	void	SetAllColumnsFontIdx		(FontIdx);
	void	SetAllSubTotalsFontIdx		(FontIdx);
	void	SetAllTotalsFontIdx			(FontIdx);
	void	SetAllColumnsTitleFontIdx	(FontIdx);
	void	SetTableTitleFontIdx		(FontIdx);

	void	SetColumnFormatIdx			(FormatIdx, int nCol);

	void	SetCellFontIdx	(FontIdx, int nRow, int nCol);
	void 	SetRowFontIdx	(FontIdx, int nRow);

	//	attribute modifyer menu
	void	ShowTablePopup			(CPoint, CBCGPScrollView*);
	void	ShowColumnPopup			(CPoint, CBCGPScrollView*);
	void	ShowTotalPopup			(CPoint, CBCGPScrollView*);
	void	ShowCellPopup			(CPoint, CBCGPScrollView*);

	void	ShowTablePopup			(CMenu& menu);
	void	ShowColumnPopup			(CMenu& menu);

	// initializators
	virtual void 	ResetCounters ();

	SelPosition		GetPosition		(CPoint, int& nRowOrCol, int* pnColumnAlias = NULL) const;
	int				GetIdxColFromAlias (int nAliasCol) const;

	// function for determinate which rectangle show when object is active
	virtual	CRect		GetActiveRect		() const;
	virtual BOOL		InMe				(CPoint);
	int					GetColumnIdxByPoint	(CPoint);

	// mouse clicked in object move area
	virtual	BOOL	InMoveArea		(CPoint);
	virtual void	LeftButtonUp	(CDC&, CBCGPScrollView*);

	// list-object virtual member function
			BOOL	CheckIsHidden	();
	virtual	void 	Paint			(CDC&, CPrintInfo*);
	virtual	void	Draw			(CDC&, CRect, BOOL bPreview);
	virtual	void	Draw			(CDC&, BOOL bPreview);

	virtual	void	ShowPopup		(CPoint, CBCGPScrollView*);
	virtual	void	Format			();

	// moving related method
	virtual	void	LeftObject			(int x);
	virtual	void	TopObject			(int y);
	virtual	void	MoveObject			(CSize);
	virtual	void	SizeObject			(CSize);
	virtual	void	RightSizeObject		(int);
	virtual	void	BottomSizeObject	(int);
	virtual	void	LeftSizeObject		(int);
	virtual	void	TopSizeObject		(int);
	virtual	BOOL	MovingObject		() const;
	virtual void	SnapToGrid			();
	virtual void	ToggleBorder		(UINT nID);

	// definition file manager function 
	virtual	BOOL	Parse			(ViewParser&);
	virtual	void	Unparse			(ViewUnparser&);

	// for RDEManager manipulation
	virtual	BOOL	AssignData		(WORD wID, RDEManager*);
	virtual	BOOL	ExecCommand		(WORD wID, RDEManager*);
	virtual	void	DisableData		();
	virtual	BOOL	CanSearched		() const;
	virtual	WORD	GetRDESearchID	() const;

	// for EditorManager manipulation
	virtual	BOOL	DeleteEditorEntry	();
	virtual BOOL	CheckTypeConsistence(CString& sError);

	// starting mouse position after creating object
	virtual CPoint	StartTrackPoint	();
	
	// virtual key accelerator management
	virtual	void	VKReturn	();

	virtual BOOL ExistChildID (WORD wID);

	virtual BOOL	GetSchema	(CXSDGenerator*, WoormTable*);
	virtual BOOL	CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const;

	BOOL CheckColumnsHiddenStatus ();
	BOOL SetActiveColumn (int nID, CPoint& ptPoint);
	BOOL SetActiveColumn(TableColumn* pCol);
	TableColumn* GetActiveColumn();

	void GetAnchoredFields(CArray<BaseRect*, BaseRect*>&	arAnchoredFields);
	void RemoveAnchoredFields();

	virtual CString GetDescription (BOOL = TRUE) const;
	CString GetName (BOOL bStringName = FALSE) const; //It tries lookup on Engine

	BOOL UseColorEasyview (/*BOOL bOdd, */int nRow/*, BOOL bIsTail*/);
	
	BOOL ExistsCellTail (int nRow);
	BOOL ExistsCellSubTotal (int nRow);
	BOOL IsEmptyRow(int nRow);
	void CheckRowValue(int nRow, BOOL& bTail, BOOL& bSubTotal, BOOL& bIsEmpty);

	BOOL ExistsColumnWithDynamicAttributeOnRow () const;

	virtual void	SetStyle		(Table*);
	virtual void	RemoveStyle		();
	virtual void	RemoveAllStyles	();
	virtual void	ClearStyle		();
	virtual void	ClearAllStyles	();

	BOOL	IsTemplate() const;
	BOOL	IsTemplateOverridden() const;
	void	PurgeTemplateColumns();
	void	MarkTemplateOverridden();
	void	MergeTemplateColumns();
	void	RenameAlias(int offset);

	//Redraw method for PropertyGrid
	void Redraw(BOOL repaint, int prevShadowHeight = 0, int movedOf=0);

	virtual void	MouseMove(CDC&, CPoint, CBCGPScrollView*, BOOL bTransMode = FALSE);

	INT GetColumnTitleHeight();
	
public:
	//{{AFX_MSG(Table)
	// afx message handler member function
	afx_msg	void	OnEasyReadingByColor		();
	afx_msg	void	OnEasyReadingByColorDynamic	();
	afx_msg	void	OnEasyReadingByLine			();
	afx_msg	void	OnEasyReadingByLineDynamic	();
	afx_msg	void	OnTableTitleColors			();
	afx_msg	void	OnTableTitleLineWidth		();
	afx_msg void	OnTableTitleAlign			();

	afx_msg void	OnTableTitleFontStyle		();
	afx_msg void	OnTableSubTitleFontStyle	();
			void	SetTableTitleFontStyle(BOOL bSubTitle);

	afx_msg void	OnHideTableTitle			();
	afx_msg void	OnHideColumnsTitle			();
	afx_msg void	OnFiscalEnd					();
	//afx_msg void	OnSubTotalDefinition		();
	afx_msg void	OnSubTotalFontStyle			();
	afx_msg void	OnSubTotalColors			();
	afx_msg void	OnSetTrasparent				();

	afx_msg	void	OnColumnTitleColors			();
	afx_msg	void	OnColumnTitleLineWidth		();
	afx_msg void	OnColumnTitleAlign			();
	afx_msg void	OnColumnTitleFontStyle		();
	afx_msg void	OnColumnToggleMultiRow		();
	afx_msg void	OnColumnToggleBitmap		();
	afx_msg void	OnColumnToggleBarCode		();
	afx_msg void	OnColumnToggleTextFile		();
	afx_msg void	OnColumnToggleHidden		();

	afx_msg	void	OnColumnColor				();
	afx_msg	void	OnColumnLineWidth			();
	//afx_msg void	OnColumnAlign				();
	afx_msg void	OnColumnFontStyle			();
	afx_msg void	OnColumnFormatStyle			();

	afx_msg	void	OnCellColor					();
	afx_msg	void	OnCellAlign					();
	afx_msg	void	OnCellFontStyle				();
	afx_msg	void	OnCellBarCodeAttrs			();

	afx_msg	void	OnBorders					();
	afx_msg void	OnTableMove					();
	afx_msg void	OnTableTitle				();
	afx_msg void	OnColumnTitle				();

	afx_msg void	OnModifyField				();
	afx_msg void	OnTotalColors				();
	afx_msg void	OnTotalLineWidth			();
	afx_msg void	OnTotalAlign				();
	afx_msg void	OnTotalFontStyle			();
	afx_msg void	OnTotalModify				();
	afx_msg void	OnUpdateShowProportional	();

	//afx_msg void	OnShowAllHiddenColumn		();
	//afx_msg void	OnShowAllLinks				();
	//afx_msg void	OnColumnToggleLink			();

	afx_msg void	OnColumnToggleSplitter			();
	afx_msg void	OnColumnToggleFixed				();
	afx_msg void	OnColumnToggleAnchorLeft		();
	afx_msg void	OnColumnToggleAnchorRight		();
	
	afx_msg void	OnColumnToggleVMergeEmptyCell		();
	afx_msg void	OnColumnToggleVMergeEqualCell		();
	afx_msg void	OnColumnToggleVMergeTailCell		();

	afx_msg void	OnColumnToggleHiddenWhenEmpy	();
	afx_msg void	OnColumnToggleOptimizedWidth	();

	afx_msg void	OnColumnDynamicFontStyle	();
	afx_msg void	OnColumnDynamicBorders		();
	afx_msg void	OnColumnDynamicFormatStyle	();

	afx_msg void	OnColumnSetWidth			();
			void	OnColumnSetWidth			(TableColumn* pCol, int nNewWidth, BOOL bRefreshUI, BOOL bSetActive = TRUE);

	afx_msg void	OnSetTableRows();

	afx_msg void	OnCopyActiveColumnAttributes			();
	afx_msg void	OnCopyActiveColumnTitleAttributes		();

	afx_msg void	OnSetDropShadow				();
	afx_msg void	OnSetHeights				();

	afx_msg void	OnSetColumnTooltip			();
	afx_msg void	OnSetColumnTitleTooltip		();

	afx_msg void	OnSetHidden					();

	afx_msg	void	OnToggleTemplate			();
	afx_msg	void	OnColumnToggleTemplate		();

	afx_msg	void	OnTableSetStyle				();
	afx_msg	void	OnColumnSetStyle			();
	afx_msg	void	OnTableSetStyleName			();
	afx_msg	void	OnColumnSetStyleName		();

	afx_msg	void	OnClearColumnCustomStyles	();
	afx_msg	void	OnClearTableCustomStyles	();
	afx_msg	void	OnClearAllCustomStyles		();

	afx_msg	void	OnBringToFront				(UINT nID);
	afx_msg	void	OnSendToBack				();

	afx_msg void	OnLayoutCopyObject			();
	afx_msg void	OnLayoutMoveObject			();
	//}}AFX_MSG

private:
	BOOL m_bTransMode = FALSE;

public:
	virtual BOOL FillSeries(CBCGPChartSeries* pSeries, WORD nColumnID);

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"


