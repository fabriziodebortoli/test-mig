#pragma once

#include "baseobj.h"
#include "textobj.h"
#include "bordpen.h"
#include <TbWoormEngine\RdeProtocolCode.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//==========================================================================                     
class TableCell;
class TableColumn;
class Table;
class RDEManager;


//===============================================================================
class TB_EXPORT TableCell : public CObject, public GenericDrawObj
{
public:
	Value		m_Value;
	CRect		m_rectCell;
	int			m_nCurrRow;
	TableColumn* m_pColumn;

	CArray<int, int> m_arBrowsedOnPage;
	CArray<int, int> m_arSelectedOnPage;
	CArray<int, int> m_arWrongOnPage;
	CStringArray m_arTooltip;

private:
	void	DrawSubTotal	(CDC&, CRect inside, CWoormDocMng*, BasicTextStyle&, BOOL bOpaque);
	void	DrawNormalCell	(CDC&, CRect inside, CWoormDocMng*, BOOL bOpaque, BOOL bIsTextFile = FALSE);
	void	DrawBarCodeCell	(CDC&, CRect inside, CWoormDocMng*, CBarCode*,	BOOL bPreview);

public:
	TableCell	(TableColumn*, CPoint, CSize);
	TableCell	(TableCell&, TableColumn*);
	TableCell	(const TableCell&);

 	virtual		~TableCell ();

	BOOL		ClearCellDynamicAttributes (BOOL bBrowsed = TRUE, BOOL bSelected = TRUE, BOOL bError = TRUE);
	BOOL		IsBrowsed				(int page);
	BOOL		IsWrong					(int page);
	BOOL		IsSelected				(int page);
	CString		GetTooltip (int nPage);

    CString		GetText () const;
	void		SetCellText(LPCTSTR pszText);

	void*	 	GetRDEData () const	{ return m_Value.m_RDEdata.GetData(); }
	RDEData*	GetRDEValue ()  { return &(m_Value.m_RDEdata); }

	BOOL		IsEnabledRDEData () const { return m_Value.m_RDEdata.IsEnabled(); }

	BOOL		IsTailMultiLineString () const;	
	BOOL		IsSubTotal() const;

    CRect	GetCellRect	() const;			
	void	SetCellRect	(const CRect&);

	void	HExpandCell	(int nWidth);		
	void	HResizeCell	(int nWidth);
	void	VExpandCell	(int nHeight);

	BOOL	CanHExpandCell (int nWidth) const;
	BOOL	CanVExpandCell (int nHeight) const;

	void	MoveCell	(CSize csDelta);	
	void	HMoveCell	(int nWidth);		
	void	VMoveCell	(int nHeight);	

	virtual	void	SetCellColor 	(COLORREF*);
	virtual	void	GetCellColor	(COLORREF*) const;

	COLORREF GetValueTextColor (BOOL* pbEvalOk = NULL) const;
	COLORREF GetValueBkgColor (COLORREF cr) const;

	void		SetCellAlign	(AlignType);
	AlignType	GetCellAlign	() const;

	void	SetCellFontIdx	(FontIdx);
	FontIdx	GetCellFontIdx	() const;

    // virtual drawing and formatting member function
	virtual void	Format	(WORD wID, Formatter*, CWoormDocMng*);
	/*virtual */void	Draw
							(
								CDC&, 
								BOOL bPreview,
								CRect,
								CWoormDocMng*,
								BasicTextStyle&,
								CellBorders&, 
								BOOL, 
								CObject*,
								BOOL bMarkExportable,
								BOOL bOpaque,
								BOOL bIsExpTitles = FALSE,
								BOOL bIsTextFile = FALSE
							);

    // for RDEManager manipulation
	virtual void	AssignData	(RDEManager*);
	virtual void	ExecCommand	(RDEManager*);
	virtual void	DisableData	();

	//implementation of baseobj's virtual method to update layout object
	virtual void		Redraw();
	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*	GetBkgColor			()						{ return &m_Value.m_rgbBkgColor; }
	virtual Expression** GetBkgColorExpr	()						{ return NULL; }
	virtual COLORREF*	GetLabelForeColor	()						{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()						{ return &m_Value.m_rgbTextColor; }
	virtual int*		GetShadowSize		()						{ return NULL; }
	virtual COLORREF*	GetShadowColor		()						{ return NULL; }
	virtual int*		GetBorderSize		()						{ return NULL; }
	virtual COLORREF*	GetBorderColor		()						{ return NULL; }
	virtual FontIdx		GetCaptionFontIdx	()						{ return NULL; }
	virtual void		SetCaptionFontIdx	(FontIdx fontIdx)		{/*do nothing*/}
	virtual FontIdx		GetValueFontIdx		()						{ return NULL; }
	virtual void		SetValueFontIdx		(FontIdx fontIdx)		{/*do nothing*/}
	virtual CString		GetWrmStyleClass	()						{ return NULL; }
	virtual void		SetWrmStyleClass	(CString styleClass)	{/*do nothing*/}
	virtual void		SetWrmStyleClass	(CObject* templateObj)	{/*do nothing*/}

};

//===============================================================================
class TB_EXPORT TotalCell : public TableCell
{
public:
	BorderPen	m_TotalPen;
public:
	TotalCell	(TableColumn*, CPoint, CSize);
	//TotalCell	(TotalCell&);
	TotalCell	(TotalCell&, TableColumn*);
	TotalCell	(const TotalCell&);

	// read routines
	BorderPen	GetTotalPen		() const;
	void		GetTotalColor	(COLORREF* rgbColors) const;

	// Set routines
	void	SetTotalPen		(const BorderPen&);
	void	SetTotalColor	(COLORREF* rgbColors);

	void	SetTextTotalColor	(COLORREF rgbColor);
	void	SetBkgTotalColor	(COLORREF rgbColor);
	void	SetBorderTotalColor	(COLORREF rgbColor);

	virtual	void	SetCellColor 	(COLORREF*);
	virtual	void	GetCellColor	(COLORREF*) const;

	// virtual drawing and formatting member function
	virtual void	DrawTotal	(CDC&, CRect, CWoormDocMng*, Borders, BOOL, BOOL bOpaque);
	virtual void	HideTotal	(CDC&, CRect, Borders, BOOL, BOOL, int);
	
	void DrawDropShadow(CDC& DC);

	COLORREF GetValueTextColor () const;
	COLORREF GetValueBkgColor () const;
	//implementation of baseobj's virtual method to update layout object
	virtual void		Redraw();
};


//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//                                                                          //
//                 I N L I N E    S T A R T    D E F I N I T I O N          //
//                                                                          //
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
inline CString	TableCell::GetText		() const	{ return m_Value.GetText(); }
inline void		TableCell::SetCellText		(LPCTSTR pszText) { m_Value.SetText(pszText); }

inline BOOL		TableCell::IsTailMultiLineString () const	{ return m_Value.m_RDEdata.IsTailMultiLineString(); }
inline BOOL		TableCell::IsSubTotal() const	{ return m_Value.m_RDEdata.IsSubTotal(); }

inline void	TableCell::HExpandCell	(int nWidth)	{ m_rectCell.right += nWidth; }
inline void	TableCell::VExpandCell	(int nHeight)	{ m_rectCell.bottom += nHeight; }
inline void	TableCell::HResizeCell	(int nWidth)	{ m_rectCell.right = m_rectCell.left + nWidth; }

inline BOOL TableCell::CanHExpandCell (int nWidth) const	{ return ((m_rectCell.Width() + nWidth) > MINCELL_X); }
inline BOOL TableCell::CanVExpandCell (int nHeight) const	{ return ((m_rectCell.Height() + nHeight) > MINCELL_Y); }

inline void	TableCell::MoveCell		(CSize csDelta)	{ m_rectCell.OffsetRect(csDelta); }
inline void	TableCell::HMoveCell	(int nWidth)	{ m_rectCell.OffsetRect (nWidth,0); }
inline void	TableCell::VMoveCell	(int nHeight)	{ m_rectCell.OffsetRect (0,nHeight); }

inline void 	TableCell::SetCellFontIdx	(FontIdx anIdx)		{ m_Value.SetFontIdx(anIdx); }
inline FontIdx	TableCell::GetCellFontIdx	() const			{ return m_Value.GetFontIdx(); }

inline BorderPen	TotalCell::GetTotalPen	() const				{ return m_TotalPen; }
inline void			TotalCell::SetTotalPen	(const BorderPen& aPen)	{ m_TotalPen = aPen; }

inline CRect	TableCell::GetCellRect		() const			{ return m_rectCell; }
inline void		TableCell::SetCellRect		(const CRect& rect)	{ m_rectCell = rect; }

inline void 		TableCell::SetCellAlign	(AlignType nAlign)	{ m_Value.SetAlign(nAlign); }
inline AlignType	TableCell::GetCellAlign	() const			{ return m_Value.GetAlign(); }

//------------------------------------------------------------------------------
inline void TableCell::Format(WORD wID, Formatter* pFormatter, CWoormDocMng* pDocument)
{
	m_Value.Format (wID, pFormatter, pDocument);
}

//------------------------------------------------------------------------------
inline void TableCell::AssignData (RDEManager* pRDEmanager)
{
	pRDEmanager->GetDataValue(m_Value.m_RDEdata);
}

//------------------------------------------------------------------------------
inline void TableCell::ExecCommand (RDEManager*)
{
//@@ TODO itri (future implementazioni)
}

//------------------------------------------------------------------------------
inline void TableCell::DisableData ()
{
	m_Value.DisableData();
}

//------------------------------------------------------------------------------
inline void TotalCell::SetTextTotalColor	(COLORREF rgbColor)	{ m_Value.SetTextColor	(rgbColor); }
inline void TotalCell::SetBkgTotalColor		(COLORREF rgbColor)	{ m_Value.SetBkgColor	(rgbColor); }
inline void TotalCell::SetBorderTotalColor	(COLORREF rgbColor)	{ m_TotalPen.SetColor	(rgbColor); }

#include "endh.dex"
