
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>

#include "baseobj.h"
#include "textobj.h"
#include "table.h"
#include "woormdoc.h"
#include "cell.h"
#include "rectobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==========================================================================                     
class TableCell;
class TotalCell;
class TableColumn;
class Table;
class RDEManager;
class ViewParser;
class ViewUnparser;
class CTBPicture;
class CBarCode;
class Expression;

// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CCellArray : public CObArray
{
public:
	// overloaded operator helpers
	TableCell* operator[](int nIndex) const	{ return (TableCell*) GetAt(nIndex); }
	TableCell*& operator[](int nIndex)		{ return (TableCell*&) ElementAt(nIndex); }
};                                        

//===========================================================================
class TB_EXPORT TableColumn : public CObject, public GenericDrawObj, public IDisposingSourceImpl
{
	friend	Table;
	friend	TableCell;
	friend	TotalCell;
	friend	CListHiddenColsDlg;
	friend  CWoormDocMng;
	friend	class TextRect;
	friend	class FieldRect;

	DECLARE_DYNAMIC(TableColumn)

public:
//PROPERTIES
	BorderPen		m_ColumnTitlePen;
	BorderPen		m_ColumnPen;

	AlignType		m_nAlignType;

	BOOL			m_bMultipleRow;	// se il testo non sta su una riga suddivide il testo e genera righe successive
	BOOL			m_bMiniHtml = FALSE;

	BOOL			m_bVMergeEmptyCell;	//remove row sep between empty cells
	BOOL			m_bVMergeTailCell;	//remove row sep between breaked tail strings
	BOOL			m_bVMergeEqualCell;	//remove row sep between vertical equal cells

	BOOL			m_bIsHidden;	//*5
	Expression*		m_pHideExpr;	//*5

	int				m_nWidth;				//*20	default width
	int				m_nSavedWidth;			//save column current width when the column has hidden status
	Expression*		m_pDynamicWidthExpr;	//*20	dynamic width (subordinate to hidden status)

	EFieldShowAs	m_ShowAs;			// analogo al FieldRect 
		CTBPicture*		m_pBitmap;		// toggle Draw of data as m_pBitmap
		CBarCode*		m_pBarCode;		// toggle Draw of data as m_pBarCode

	//Print attributes
	BOOL			m_bSplitterColumn;	//usate per partizionare in stampa una tabella (Excel like)
	BOOL			m_bPinnedColumn;	//ripete la colonna durante i partizionamenti

//HEADER
	ColumnTitleText	m_Title;
						/*	eredita: 
							(da GenericText)
								CString		m_strText		//*8  
							(da BasicTextStyle)
								COLORREF	m_rgbTextColor;	//*6
								COLORREF	m_rgbBkgColor;	//*7
								AlignType	m_nAlign;
								FontIdx		m_nFontIdx;
						*/

	Expression*		m_pTitleExpr;			//*8 espressione di tipo stringa: titolo della colonna
	Expression*		m_pTitleTextColorExpr;	//*6
	Expression*		m_pTitleBkgColorExpr;	//*7

	Expression*		m_pTitleTooltipExpr;	//espressione di tipo stringa: tooltip dell'header della colonna


//BODY
	Expression*		m_pTextColorExpr;		// per l'editor associare a m_Cell[0].m_Value.m_rgbTextColor e .m_rgbBkgColor
	Expression*		m_pBkgColorExpr;		/* per accedervici sono disponibili i metodi
												void	GetAllCellsTextColor(COLORREF& crTextColor) const;
												void	GetAllCellsBkgColor	(COLORREF& crBkgColor) const; e
												void	SetAllCellsTextColor(COLORREF crTextColor);
												void	SetAllCellsBkgColor(COLORREF crBkgColor);	
											*/

	FontIdx			m_nFontIdx;				// *1
	Expression*		m_pTextFontExpr;		// *1 espressione di tipo stringa che deve ritornare il nome di un font

	FormatIdx		m_nFormatIdx;			// *2
	Expression*		m_pCellFormatterExpr;	// *2 espressione di tipo stringa: deve ritornare il nome di un formattatore

	Expression*		m_pCellBordersExpr;		//espressione di tipo stringa che deve ritornare un stringa del tipo  "left, bottom"
	Expression*		m_pCellTooltipExpr;		//espressione di tipo stringa: tooltip delle celle della colonna

//SUBTOTALI
	SubTotalStyle	m_SubTotal;
						/*	eredita da BasicTextStyle:
								COLORREF	m_rgbTextColor;	*3
								COLORREF	m_rgbBkgColor;	*4
								AlignType	m_nAlign;		ATTENZIONE non è esposto!
								FontIdx		m_nFontIdx;
						*/

//TOTALI (mostrare in dipendenza di m_bShowTotal)
	BOOL			m_bShowTotal;	// toggle Draw of column total
	TotalCell*		m_pTotalCell;			// must always exist (also if not m_bShowTotal)

	Expression*		m_pTotalTextColorExpr;	// *: associare a m_pTotalCell->m_Value.m_rgbTextColor;
	Expression*		m_pTotalBkgColorExpr;	// *: associare a m_pTotalCell->m_Value.m_rgbBkgColor;

	Value			m_PreviousValue;
//---- END properties
private:
	WORD	m_wInternalID;

	Table*	m_pTable;
								// CRect::Width -> *20
	CRect	m_ColumnTitleRect;	// only column m_Title
	CRect	m_ColumnCellsRect;	// only column m_Title and column m_Cells
	CRect	m_ColumnRect;		// including column m_Title, column m_Cells, total
								// baseRect include also totals rect

	Formatter*		m_pFormatter;

	CCellArray		m_Cells;

	CArray<BaseRect*, BaseRect*>	m_arAnchoredFields;

	//UNUSED
	BOOL			m_bAnchorColumnLeft;   
	BOOL			m_bAnchorColumnRight;
	BOOL			m_bHideWhenEmpty;
	BOOL			m_bOptimizeWidth;
	//----

	BOOL			m_bTemplate;
	BOOL			m_bTemplateOverridden;
	TableColumn*	m_pDefault;
public:
	//BOOL	m_bMixedColumnFont = FALSE;		// indica che le celle delle colonne contengono font diversi
	//BOOL	m_bMixedColumnAlign = FALSE;	// indica che le celle delle colonne contengono allineamenti diversi
	
	CString			m_sStyleClass;

	Table*	GetTable () { return m_pTable; }
	TableCell*		GetCells(INT nRow) { return m_Cells[nRow]; }
	CCellArray*		GetCellArray() { return &m_Cells; }

private:
	CSize	DefaultSize 				(FontIdx nFontIdx, int nLen = 1);
	int		DefaultCellWidth			(FontIdx nFontIdx, int nLen = 1);
	int		DefaultCellHeight			();
	int		DefaultColumnTitleHeight	(FontIdx nFontIdx);

	void	SetDefaultAlign			(const DataType& dtype);
	void	SetDefaultFontIdx		(const DataType& dtype);

	FontIdx	GetDefaultCellFontIdx	(const DataType& dtype) const;
	FontIdx	GetDefaultTotalFontIdx	(const DataType& dtype) const;

	BOOL	ColumnIsShowedAsString	() const;

public:
	TableColumn		(CPoint, CSize, CSize, Table*, int nRows, WORD wInternalID);
	TableColumn		(CPoint, CSize, CSize, Table*, int nRows);
	TableColumn		(TableColumn&, WORD wInternalID);
	TableColumn		(const TableColumn&);
	virtual	~TableColumn	();

	CRect	GetColumnTitleRect() const { return m_ColumnTitleRect; }	// only column m_Title

	int		SetColumnTitle	();
	int		LastRow			() const;
	BOOL	HasTotal		() const;
	BOOL	IsMultiRow		() const;
	BOOL	IsBitmap		() const;
	BOOL	IsBarCode		() const;
	BOOL	IsTextFile		() const;
	//Getter for printer attributes
	BOOL	IsAnchorLeft	() const;
	BOOL	IsAnchorRight	() const;
	BOOL	IsSplitter		() const;
	BOOL	IsPinned		() const;
	BOOL	IsHiddenWhenEmpy() const;
	BOOL	IsWidthOptimized() const;

	BOOL	CanBeMultiRow	();
	BOOL	CanBeBitmap		();
	BOOL	CanBeBarCode	();
	BOOL	CanBeTextFile	();

	void	ToggleMultiRow	();
	void	ToggleBitmap	();
	void	ToggleBarCode	();
	void	ToggleTextFile	();

	// boolean for z-printing
	void	ToggleSplitter			();	
	void	ToggleFixed				();
	void	ToggleAnchorLeft		();
	void	ToggleAnchorRight		();
	void	ToggleVMergeEmptyCell	();
	void	ToggleVMergeEqualCell	();
	void	ToggleVMergeTailCell	();
	void	ToggleHiddenWhenEmpy	();
	void	ToggleOptimizedWidth	();

	void	UpdateTypeInfo			();

	BOOL	IsHidden				() const;
	void	SetHidden				(BOOL bIsHidden);

	BOOL	HasDynamicAttributeOnRow () const;


	//------------------------------------------------------------------------------
	TableBorders GetBorders() const { return m_pTable->m_Borders; }
	BOOL ShowTitles(BOOL bPrinting)	{ return m_pTable->m_pDocument->m_pOptions->ShowTitles(m_pTable->m_pDocument, bPrinting); }
	BOOL NoBorders(BOOL bPrinting)	{return m_pTable->m_pDocument->m_pOptions->NoBorders(m_pTable->m_pDocument, bPrinting);	}

	// key accelerator management
	void	VKReturn			();

	CRect	GetColumnCellsRect	() const;	
	CRect	GetColumnRect     	() const;
	CRect	TotalRect			() const;
	CRect	CellRect			(int nRow) const;

	void	VExpandRow		(int nRow, int height);
	void	VExpandTitle	(int height);
	void	VMoveTotal		(int height);
	void	VExpandTotal	(int height);
	void	HExpandColumn	(int width);
	void	HResizeColumn	(int width);
	void	HMoveColumn		(int width);
	void	MoveColumn		(CSize csDelta);

	//clear all printer column attributes when size changes
	void	UpdateSplitterInfo		(LONG oldValue);
	void	MoveAnchoredFields		(int xOffset, int yOffset = 0, BOOL bSkipTopField= FALSE);
	void	HResizeAnchoredFields	(int newWidth);
	void	HExpandAnchoredFields	(int increment);
	void	RemoveSplitter			();

	// column total manipulation
	void	AddColumnTotal		();
	void	DeleteColumnTotal	();
	void	ModifyColumnTotal	();

	BOOL	CanVExpandRow		(int nRow, int height) const;
	BOOL	CanVExpandTitle		(int height) const;
	BOOL	CanVExpandTotal		(int height) const;
	BOOL	CanHExpandColumn	(int width) const;

	void	RemoveCells		(int);
	void	AddCells		(int);

	void	SetColumnPen		(const BorderPen&);
	void	SetTotalPen			(const BorderPen&);
	void	SetColumnTitlePen	(const BorderPen&);
	
// Colors
	void	GetColumnTitleColor	(COLORREF*) const;
	void	GetTotalColor		(COLORREF*) const;
	void	GetSubTotalColor	(COLORREF*) const;
	void	GetCellColor		(COLORREF*, int nRow) const;
	
	void		GetAllCellsTextColor(COLORREF& crTextColor) const;
	COLORREF	GetAllCellsTextColor();
	void		GetAllCellsBkgColor	(COLORREF& crBkgColor) const;
	COLORREF	GetAllCellsBkgColor	();

	void	SetAllCellsTextColor(COLORREF crTextColor);	/*Set text colors to all cells, cell total too*/
	void	SetAllCellsBkgColor	(COLORREF crBkgColor);	/*Set background colors to all cells, cell total too*/

	void	SetCellColor 		(COLORREF*, int nRow);
	void	SetColumnCellsColor	(COLORREF*);
	//void	SetRowColor			(COLORREF*, int nRow);

	void	SetTotalColor				(COLORREF*);
	void	SetTotalTextColor			(COLORREF color);
	void	SetTotalBkgColor			(COLORREF color);
	void	SetTotalBorderColor			(COLORREF color);
	void	SetTotalBorderWidth			(int width);
	
	void	SetSubTotalColor			(COLORREF*);

	void	SetColumnTitleColor			(COLORREF*);
	void	SetColumnTitleTextColor		(COLORREF color);//todo da fare inline
	void	SetColumnTitleBkgColor		(COLORREF color);
	void	SetColumnTitleBorderColor	(COLORREF color);
	void	SetColumnTitleBorderWidth	(int width);
	//-------------------------

	void	SetCellAlign		(AlignType, int nRow);
	void	SetColumnAlign		(AlignType);
	//void	SetColumnAlignFromProp(AlignType oldAlign);
	void	SetTotalAlign		(AlignType);
	void	SetColumnTitleAlign	(AlignType);

	void	SetCellFontIdx		(FontIdx, int nRow);
	//void	SetRowFontIdx		(FontIdx, int nRow);

	void	SetColumnFontIdx		(FontIdx);
	void	SetTotalFontIdx			(FontIdx);
	void	SetSubTotalFontIdx		(FontIdx);
	void	SetColumnTitleFontIdx	(FontIdx);

	void	SetColumnFormatIdx		(FormatIdx);

	void	SetFieldWidth			();

	int		CalculateFieldWidth		(const CString& strText) const ;
	BOOL	CalculateFieldWidthFactors	(FieldWidthFactors& fieldWidthFactors, BOOL bSubTotal = FALSE) const ;

	BorderPen	GetColumnPen		() const;
	BorderPen	GetTotalPen			() const;
	BorderPen	GetColumnTitlePen	() const;

	AlignType	GetColumnTitleAlign	() const;
	AlignType	GetColumnAlign		() const;
	AlignType	GetTotalAlign		() const;
	AlignType	GetCellAlign		(int nRow) const;

	FontIdx		GetColumnTitleFontIdx	() const;
	FontIdx		GetColumnFontIdx		() const;
	FontIdx		GetSubTotalFontIdx		() const;
	FontIdx		GetTotalFontIdx			() const;
	FontIdx		GetCellFontIdx			(int nRow) const;
	FontIdx		GetCellDynamicFontIdx	() const;
	BOOL		GetCellDynamicBorders	(Borders&) const;
	FormatIdx	GetCellDynamicFormatIdx	() const;
	Formatter*	GetCellDynamicFormatter	() const;

	FormatIdx	GetColumnFormatIdx		() const;

	virtual WORD	GetInternalID		() const;

	CBarCode*	GetBarCode				() const;

	// resizing routines
	void	ResizeTotalCell (CPoint ptOrigin, CSize size);
	void	ResizeCells		(CPoint ptOrigin, CSize size);

	// drawing routines
	void	DrawColumnTitle	(CDC&, CRect, BOOL, BOOL, const CRect&	rectColumnTitleRect, int nRow = -1);
	void	DrawTotal		(CDC&, CRect, BOOL, BOOL, int);
	void	DrawCell		(CDC&, CRect, 
								int nRow, 
								BOOL bFirstCol, 
								BOOL bLastCol, 
								BOOL bPreview, 
								BOOL bMarkExportable, 
								BOOL bIsExpTitles, 
								BOOL bRowSep,
								BOOL bCurrRowHasTail
							);
	void	DrawColumnIcons	(CDC& DC, CRect& inside);

	void	Format			();
	Formatter*	GetFormatter	();

	// clipboard management
	virtual	void CopyClipboard ();

	// Parse and Unparse function (from file)
	BOOL	Parse	(ViewParser&);
	void	Unparse	(ViewUnparser&);

	// for RDEManager manipulation
	void	AssignData	(WORD wID, RDEManager*);
	BOOL	ExecCommand	(WORD wID, RDEManager*);
	void	DisableData	();
	BOOL	CanSearched	();
	
	virtual BOOL	CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const;

	// for EditorManager manipulation
	BOOL	DeleteEditorEntry	();
	BOOL	CheckTypeConsistence(CString& sError);

	CWoormDocMng*	GetDocument	() const	{ return m_pTable->m_pDocument; }
	CString		GetTitle () const;
	DataType	GetDataType() const;
	CString		GetDescription(BOOL bShowLabel = TRUE) const;
	CString		GetCaption() const;
	CString		GetFieldName () const;
	
	BOOL	AddAnchoredField	(BaseRect* pRect);
	BOOL	RemoveAnchoredField	(BaseRect* pRect);
	
	void ClearCellDynamicAttributes (BOOL bBrowsed = TRUE, BOOL bSelected = TRUE, BOOL bError = TRUE);
	void ResetCells();

	Expression*&	GetDynamicExpr	(EnumDynamicExpr);

	CString		GetTitleTooltip () const;

	int			Width() const;	//It returns current width
	void		InitializeWidth(int width);

	virtual void	CopyAttributes			(CObject* pObjSource);
	virtual void	CopyHeaderAttributes	(CObject* pObjSource);

	virtual void	SetStyle		(TableColumn*);
	virtual void	RemoveStyle		();
	virtual void	ClearStyle		();

	void			UpdateDocument			(BOOL bUpdateProperty = FALSE);	
	//pure virtual redraw method for udating to the property grid
	virtual void		Redraw				();
	//pure virtual methods for common properties
	virtual COLORREF*	GetBkgColor			()						{ return NULL; }
	virtual Expression** GetBkgColorExpr	()						{ return &m_pBkgColorExpr;}
	virtual COLORREF*	GetLabelForeColor	()						{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()						{ return NULL; }
	virtual int*		GetShadowSize		()						{ return NULL; }
	virtual COLORREF*	GetShadowColor		()						{ return NULL; }
	virtual int*		GetBorderSize		()						{ return NULL; }
	virtual COLORREF*	GetBorderColor		()						{ return NULL; }
	virtual Borders*	GetBordersObj		()						{ return NULL; }
	virtual FontIdx		GetCaptionFontIdx	()						{ return NULL; }
	virtual void		SetCaptionFontIdx	(FontIdx fontIdx)		{/*do notthing*/ }
	virtual FontIdx		GetValueFontIdx		()						{ return NULL; }
	virtual void		SetValueFontIdx		(FontIdx fontIdx)		{/*do notthing*/ }
	virtual CString		GetWrmStyleClass	()						{ return m_sStyleClass; }
	virtual void		SetWrmStyleClass	(CString styleClass)	{ m_sStyleClass = styleClass; }
	virtual void		SetWrmStyleClass	(CObject* templateObj)	{ SetStyle(dynamic_cast<TableColumn*>(templateObj)); }

	DataArray* GetDataArrayFromId(WORD nColumnID) const;
	BOOL FillSeries (CBCGPChartSeries* pSeries, WORD nColumnID) const;
	BOOL FillSeries (CStringArray& pCategories, WORD nColumnID) const;
};

//----------------------------------------------------------------------------

inline int	TableColumn::LastRow		() const	{ return m_Cells.GetUpperBound(); }
inline BOOL	TableColumn::HasTotal		() const	{ return m_bShowTotal; }
inline BOOL TableColumn::CanSearched	()			{ return GetFormatter()->GetDataType() == DATA_STR_TYPE; }

inline CRect	TableColumn::GetColumnCellsRect	() const	{ return m_ColumnCellsRect; }
inline CRect	TableColumn::TotalRect			() const	{ return m_pTotalCell->GetCellRect(); }
inline CRect	TableColumn::CellRect	(int nRow) const	{ return m_Cells[nRow]->GetCellRect(); }

inline BOOL TableColumn::CanVExpandRow		(int nRow, int height) const	{ return m_Cells[nRow]->CanVExpandCell(height); }
inline BOOL TableColumn::CanVExpandTitle	(int height) const				{ return ((m_ColumnTitleRect.Height() - height) > MINCELL_Y); }
inline BOOL TableColumn::CanVExpandTotal	(int height) const				{ return ((TotalRect().Height() + height) > MINCELL_Y); }
inline BOOL TableColumn::CanHExpandColumn	(int width) const				{ return m_Cells[0]->CanHExpandCell(width); }

inline void	TableColumn::ToggleMultiRow		()							{ m_bMultipleRow = !m_bMultipleRow; }
inline void TableColumn::SetColumnPen		(const BorderPen& aPen)		{ m_ColumnPen = aPen; }
inline void TableColumn::SetColumnTitlePen	(const BorderPen& aPen)		{ m_ColumnTitlePen = aPen; }
inline void TableColumn::SetTotalPen		(const BorderPen& aPen)		{ m_pTotalCell->SetTotalPen(aPen); }

inline void TableColumn::SetColumnTitleAlign	(AlignType nAlign)		{ m_Title.SetAlign(nAlign); }
inline void TableColumn::SetTotalAlign			(AlignType nAlign)		{ m_pTotalCell->SetCellAlign(nAlign); }

inline void TableColumn::SetColumnTitleFontIdx	(FontIdx nIdx)			{ m_Title.SetFontIdx(nIdx); }
inline void TableColumn::SetTotalFontIdx		(FontIdx nIdx)			{ m_pTotalCell->SetCellFontIdx(nIdx); }
inline void TableColumn::SetSubTotalFontIdx		(FontIdx nIdx)			{ m_SubTotal.SetFontIdx(nIdx); }
inline void TableColumn::SetCellFontIdx			(FontIdx nIdx, int nRow) { m_Cells[nRow]->SetCellFontIdx(nIdx); }

inline void			TableColumn::SetColumnFormatIdx		(FormatIdx nIdx)	{ m_pFormatter = NULL; m_nFormatIdx = nIdx; }
inline FormatIdx	TableColumn::GetColumnFormatIdx		() const			{ return m_nFormatIdx;			}

inline BorderPen	TableColumn::GetColumnPen			() const	{ return m_ColumnPen; }
inline BorderPen	TableColumn::GetColumnTitlePen		() const	{ return m_ColumnTitlePen; }
inline BorderPen	TableColumn::GetTotalPen			() const	{ return m_pTotalCell->GetTotalPen(); }

inline AlignType	TableColumn::GetColumnTitleAlign	() const	{ return m_Title.GetAlign(); }
inline AlignType	TableColumn::GetColumnAlign			() const	{ return m_nAlignType; }
inline AlignType	TableColumn::GetTotalAlign			() const	{ return m_pTotalCell->GetCellAlign(); }

inline FontIdx		TableColumn::GetColumnTitleFontIdx	() const	{ return m_Title.GetFontIdx(); }
inline FontIdx		TableColumn::GetColumnFontIdx		() const	{ return m_nFontIdx; }
inline FontIdx		TableColumn::GetSubTotalFontIdx		() const	{ return m_SubTotal.GetFontIdx(); }
inline FontIdx		TableColumn::GetTotalFontIdx		() const	{ return m_pTotalCell->GetCellFontIdx(); }

inline WORD			TableColumn::GetInternalID			() const	{ return m_wInternalID;		}
inline BOOL			TableColumn::IsMultiRow				() const	{ return m_bMultipleRow;	}

inline BOOL			TableColumn::IsBitmap				() const	{ return m_ShowAs == EFieldShowAs::FT_IMAGE && m_pBitmap != NULL; }
inline BOOL			TableColumn::IsBarCode				() const	{ return m_ShowAs == EFieldShowAs::FT_BARCODE && m_pBarCode != NULL; }
inline BOOL			TableColumn::IsTextFile				() const	{ return m_ShowAs == EFieldShowAs::FT_TEXTFILE; }

inline BOOL			TableColumn::CanBeMultiRow			() 	{ return GetFormatter()->GetDataType() == DATA_STR_TYPE || 
																		GetFormatter()->GetDataType() == DATA_TXT_TYPE; 
																	}	
inline BOOL			TableColumn::CanBeBitmap			() 	{ return GetFormatter()->GetDataType() == DATA_STR_TYPE ||
																		GetFormatter()->GetDataType() == DATA_BOOL_TYPE; }	
inline BOOL			TableColumn::CanBeBarCode			() 	{ return GetFormatter()->GetDataType() == DATA_STR_TYPE; }	
inline BOOL			TableColumn::CanBeTextFile			() 	{ return GetFormatter()->GetDataType() == DATA_STR_TYPE; }	
inline CBarCode*	TableColumn::GetBarCode				() const	{ return m_pBarCode; }


inline BOOL			TableColumn::IsAnchorLeft			() const	{ return m_bAnchorColumnLeft; }  
inline BOOL			TableColumn::IsAnchorRight			() const	{ return m_bAnchorColumnRight; }
inline BOOL			TableColumn::IsSplitter				() const	{ return m_bSplitterColumn; }
inline BOOL			TableColumn::IsPinned				() const	{ return m_bPinnedColumn; }
inline BOOL			TableColumn::IsHiddenWhenEmpy		() const	{ return m_bHideWhenEmpty; }
inline BOOL			TableColumn::IsWidthOptimized		() const	{ return m_bOptimizeWidth; }

//------------------------------------------------------------------------------
inline FontIdx TableColumn::GetDefaultCellFontIdx (const DataType& dtype) const
{
	return IsShowedAsString(dtype) ? 
			m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_STRING) :
			m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_NUM);
}

//------------------------------------------------------------------------------
inline FontIdx TableColumn::GetDefaultTotalFontIdx (const DataType& dtype) const
{
	return IsShowedAsString(dtype) ? 
			m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_STRING) :
			m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_NUM);
}

//---------------------------------------------------------------------------
inline void TableColumn::GetTotalColor (COLORREF* pColors) const
{
	m_pTotalCell->GetTotalColor (pColors);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetCellColor	(COLORREF* pColors, int nRow)
{
	m_Cells[nRow]->SetCellColor(pColors);
}

//---------------------------------------------------------------------------
inline AlignType TableColumn::GetCellAlign (int nRow) const
{
	return m_Cells[nRow]->GetCellAlign();
}

//---------------------------------------------------------------------------
inline FontIdx TableColumn::GetCellFontIdx (int nRow) const
{
	return m_Cells[nRow]->GetCellFontIdx();
}

//---------------------------------------------------------------------------
inline void TableColumn::SetTotalColor (COLORREF* pColors)
{
	m_pTotalCell->SetTotalColor(pColors);
}

//---------------------------------------------------------------------------
inline void TableColumn::VMoveTotal (int height)
{
	m_pTotalCell->MoveCell(CSize(0, height));
}

//------------------------------------------------------------------------------
inline void TableColumn::ResizeTotalCell (CPoint ptOrigin, CSize size)
{
	m_pTotalCell->SetCellRect (CRect(ptOrigin, size));
}

//---------------------------------------------------------------------------
inline void TableColumn::SetColumnTitleTextColor(COLORREF color)
{
	m_Title.SetTextColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetColumnTitleBkgColor(COLORREF color)
{
	m_Title.SetBkgColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetColumnTitleBorderColor(COLORREF color)
{
	m_ColumnTitlePen.SetColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetColumnTitleBorderWidth(int width)
{
	m_ColumnTitlePen.SetWidth(width);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetTotalTextColor(COLORREF color)
{
	m_pTotalCell->m_Value.SetTextColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetTotalBkgColor(COLORREF color)
{
	m_pTotalCell->m_Value.SetBkgColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetTotalBorderColor(COLORREF color)
{
	m_pTotalCell->SetBorderTotalColor(color);
}

//---------------------------------------------------------------------------
inline void TableColumn::SetTotalBorderWidth(int width)
{
	m_pTotalCell->m_TotalPen.SetWidth(width);
}

//===========================================================================
class TB_EXPORT ActiveColumn : public CObject, public IDisposingSourceImpl
{
public:
	TDisposablePtr<TableColumn>	m_pColumn;
	ActiveRect*	m_pActive;	//è owner

public:
	ActiveColumn(TableColumn* pCol, CWoormDocMng* pDocument);
	virtual ~ActiveColumn();

	void	SetActive();
	void	Paint(CDC& DC, CPrintInfo* pInfo);
};
//-----------------------------------------------------------------------------
#include "endh.dex"

