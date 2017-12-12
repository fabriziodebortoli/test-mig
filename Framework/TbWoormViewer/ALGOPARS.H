
#pragma once

#include "mclrdlg.h" 
#include "woormini.h" 
#include "column.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class Table;
class CounterColorElement;
class SingleRect;
class CellColumnBorderPen;
class CellBodyDiff;
class CellColumnTitleDiff;
class CellSubTotalDiff;
class CellTotalDiff;

//==============================================================================
class TB_EXPORT CCounterColorElementArray : public Array
{
public:
	// overloaded operator helpers
	CounterColorElement* operator[](int nIndex) const	{ return (CounterColorElement*) GetAt(nIndex); }
	CounterColorElement*& operator[](int nIndex)		{ return (CounterColorElement*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CSingleRectArray : public Array
{
public:
	// overloaded operator helpers
	SingleRect* operator[](int nIndex) const	{ return (SingleRect*) GetAt(nIndex); }
	SingleRect*& operator[](int nIndex)			{ return (SingleRect*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CCellColumnBorderPenArray : public Array
{
public:
	// overloaded operator helpers
	CellColumnBorderPen* operator[](int nIndex) const	{ return (CellColumnBorderPen*) GetAt(nIndex); }
	CellColumnBorderPen*& operator[](int nIndex)		{ return (CellColumnBorderPen*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CCellBodyDiffArray : public Array
{
public:
	// overloaded operator helpers
	CellBodyDiff* operator[](int nIndex) const	{ return (CellBodyDiff*) GetAt(nIndex); }
	CellBodyDiff*& operator[](int nIndex)		{ return (CellBodyDiff*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CCellColumnTitleDiffArray : public Array
{
public:
	// overloaded operator helpers
	CellColumnTitleDiff* operator[](int nIndex) const	{ return (CellColumnTitleDiff*) GetAt(nIndex); }
	CellColumnTitleDiff*& operator[](int nIndex)		{ return (CellColumnTitleDiff*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CCellSubTotalDiffArray : public Array
{
public:
	// overloaded operator helpers
	CellSubTotalDiff* operator[](int nIndex) const	{ return (CellSubTotalDiff*) GetAt(nIndex); }
	CellSubTotalDiff*& operator[](int nIndex)		{ return (CellSubTotalDiff*&) ElementAt(nIndex); }
};                                        

//==============================================================================
class TB_EXPORT CCellTotalDiffArray : public Array
{
public:
	// overloaded operator helpers
	CellTotalDiff* operator[](int nIndex) const	{ return (CellTotalDiff*) GetAt(nIndex); }
	CellTotalDiff*& operator[](int nIndex)		{ return (CellTotalDiff*&) ElementAt(nIndex); }
};       

//==============================================================================
class TB_EXPORT CellColumnBorderPen : public CObject
{
private:
	BorderPen	m_BorderPen;
	CRect		m_Rect;
	BOOL		m_bIsNumeric;

public:
	CellColumnBorderPen	(BorderPen&, int nCol);

	BOOL MergeCell	(BorderPen&, int nCol);

	int			GetPenWidth		()	const 	{ return m_BorderPen.m_nWidth; }
	BorderPen	GetBorderPen	()	const	{ return m_BorderPen; }
	CRect		GetRect			()	const	{ return m_Rect; }
	BOOL		CellIsNumeric	()	const	{ return m_bIsNumeric; }
	
};

//==============================================================================
class TB_EXPORT CellDifferences : public CObject
{
private:
	FontIdx		m_nFontIdx;
	AlignType	m_nAlign;
	BOOL		m_bIsNumeric;
	CRect		m_Rect;

public:
	CellDifferences (AlignType, FontIdx, BOOL bNumeric, int nCol, int nStartRow = 0);

	AlignType		GetAlign	()	const	{ return m_nAlign; }
	FontIdx			GetFontIdx	()	const	{ return m_nFontIdx; }
	CRect	GetRect		()	const	{ return m_Rect; }

	BOOL IsSameRectRight	(int right) const	{ return (m_Rect.right	== right); }
	BOOL IsSameRectLeft		(int left) const	{ return (m_Rect.left	== left); }
	BOOL IsSameRectBottom	(int bottom) const	{ return (m_Rect.bottom	== bottom); }

	BOOL CellIsNumeric	() const	{ return m_bIsNumeric; }
	BOOL IncRectRight	()			{ m_Rect.right++; return TRUE; }
	BOOL IncRectBottom	()			{ m_Rect.bottom++; return TRUE; }
};

//==============================================================================
class TB_EXPORT CellBodyDiff : public CellDifferences
{
public:
	CellBodyDiff	(AlignType, FontIdx, BOOL bNumeric, int nCol, int nStartRow);

	BOOL MergeCell	(AlignType, FontIdx, int nCol, int nRow);
};

//==============================================================================
class TB_EXPORT CellSubTotalDiff : public CellDifferences
{
private:
	COLORREF	m_rgbColor[CSubTotalColorsDlg::MAX];

public:
	CellSubTotalDiff (FontIdx, BOOL bNumeric, COLORREF* rgbColor, int nCol);

	COLORREF*	GetColor	()	const	{ return (COLORREF*)m_rgbColor; }
	BOOL		MergeCell	(FontIdx, COLORREF* rgbColor, int nCol);
};

//==============================================================================
class TB_EXPORT CellGeneralDiff : public CellDifferences
{
private:
	BorderPen	m_BorderPen;
	
public:
	CellGeneralDiff (AlignType, BOOL bNumeric, FontIdx, BorderPen&, int nCol);

	int			GetPenWidth	() const	{ return m_BorderPen.m_nWidth; }
	BorderPen	GetBorderPen() const 	{ return m_BorderPen; }
	BOOL		MergeCell	(AlignType, FontIdx, BorderPen&, int nCol);
};

//==============================================================================
class TB_EXPORT CellColumnTitleDiff : public CellGeneralDiff
{
private:
	COLORREF	m_rgbColor[CColumnTitleColorsDlg::MAX];

public:
	CellColumnTitleDiff		(AlignType, FontIdx, BorderPen&, COLORREF*, int nCol);

	COLORREF*	GetColor	()	const	{ return (COLORREF*)m_rgbColor; }
	BOOL		MergeCell	(AlignType, FontIdx, BorderPen&, COLORREF*, int nCol);
};

//==============================================================================
class TB_EXPORT CellTotalDiff : public CellGeneralDiff
{
private:
	COLORREF	m_rgbColor[CTotalColorsDlg::MAX];

public:
	CellTotalDiff (AlignType, BOOL bNumeric, FontIdx, BorderPen&, COLORREF*, int nCol);

	COLORREF*	GetColor	()	const	{ return (COLORREF*)m_rgbColor; }
	BOOL		MergeCell	(AlignType, FontIdx, BorderPen&, COLORREF*, int nCol);
};

//==============================================================================
class TB_EXPORT CounterColorElement : public CObject
{
private:
	COLORREF	m_rgbColor;
	int			m_nCounter;
		
public:
	CounterColorElement (COLORREF);
	
	int			GetCounter			()	const { return m_nCounter; }
	COLORREF	GetColor			()	const { return m_rgbColor; }
	void		IncrementCounter	()	{ m_nCounter++; }
};

//==============================================================================
class TB_EXPORT CounterColor
{
private:
	CCounterColorElementArray m_Counters;
		
public:
	CounterColor ();
	
	void		AddOrIncrement		(COLORREF aColor);
	COLORREF	GetColorMaxCounter	() const;
};

//==============================================================================
// Regione della tabella con stesse caratteristiche di colore
class TB_EXPORT SingleRect : public CObject
{
private:
	CRect		m_Rect;	
	COLORREF	m_dwCellColor[CCellColorsDlg::MAX];

public:
	SingleRect (COLORREF* aCellColor, int nCol, int nStartRow, int nEndRow);

	COLORREF*	GetCellColor	()	const	{ return (COLORREF*)m_dwCellColor; }
	CRect		GetRect			()	const	{ return m_Rect; }
	
	BOOL		MergeRectColors		(COLORREF* aCellColor, int aStartCol, int nStartRow, int nEndRow);
};

//==============================================================================
// Tabella contenete le regioni con differenti colori e celle con diverso FontIdx 
// e stile di Alineamento
class TB_EXPORT BodyTable
{
protected:
	Table*		m_pTable;

private:
	COLORREF	m_dwCoinedCellColor[CCellColorsDlg::MAX];
	
	CSingleRectArray			m_TableColorRects;        
	CCellColumnBorderPenArray	m_ColumnPenCells;
	CCellBodyDiffArray			m_BodyCells;
	CCellColumnTitleDiffArray	m_TitleCells;
	CCellSubTotalDiffArray		m_SubTotalCells;
	CCellTotalDiffArray			m_TotalCells;
	
public:
	BodyTable (Table*);
	
	void SetColumnTitleCells	(TableColumn* pColumn, int nCol);
	void SetColumnPenCells		(BorderPen& borderPen, int nCol);
	void SetSubTotalCells		(TableColumn* pColumn, BOOL bIsNumeric, int nCol);
	void SetTotalCells			(TableColumn* pColumn, BOOL bIsNumeric, int nCol);
	
	int GetNrRect				()	const { return m_TableColorRects.GetSize(); }
	int GetNrCellDiff			()	const { return m_BodyCells.GetSize(); }
	int GetNrColumnPenCells		()	const { return m_ColumnPenCells.GetSize(); }
	int	GetNrTitleCells			()	const { return m_TitleCells.GetSize(); }
	int	GetNrSubTotalCells		()	const { return m_SubTotalCells.GetSize(); }
	int	GetNrTotalCells			()	const { return m_TotalCells.GetSize(); }

	COLORREF*				GetCoinedCellColor	()				const { return (COLORREF*)m_dwCoinedCellColor; }
	SingleRect*				GetNextSingleRect	(int nIndex)	const { return m_TableColorRects[nIndex]; }
	CellColumnTitleDiff*	GetNextTitleCell	(int nIndex)	const { return m_TitleCells[nIndex]; }
	CellColumnBorderPen*	GetNextColumnPen	(int nIndex)	const { return m_ColumnPenCells[nIndex]; }
	CellBodyDiff*			GetNextCellDiff		(int nIndex)	const { return m_BodyCells[nIndex]; }
	CellSubTotalDiff*		GetNextSubTotalCell	(int nIndex)	const { return m_SubTotalCells[nIndex]; }
	CellTotalDiff*			GetNextTotalCell	(int nIndex)	const { return m_TotalCells[nIndex]; }
	
	void AddBodyCellColors	(COLORREF* aColor, int nCol, int nStartRow, int nEndRow);
	void AddBodyCellAttrib	(TableColumn* pColumn, BOOL bIsNumeric, int nCol, int nRow = 0);
	void SetMoreCoined		(COLORREF aValue, COLORREF dwBackground);
	void BuildUnparseInfo	();
};


#include "endh.dex"
