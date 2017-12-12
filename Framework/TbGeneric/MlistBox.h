
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CCheckBitmap;

//============================================================================
class TB_EXPORT CMultiListBox : public CListBox
{
	DECLARE_DYNAMIC (CMultiListBox)

public:
	enum ItemStatus 	{ UNCHECKED, CHECK_ONE, CHECK_TWO, CHECK_THREE, CHECK_FOUR, CHECK_FIVE };
	enum MListBoxStyles
	{
		UNSORTED				= 0x0000,
		SORT_ON_FIRST_STRING	= 0x0001,
		SORT_ON_SECOND_STRING	= 0x0002,
		SORT_ON_BOTH_STRINGS	= 0x0003,
		HIDE_SECOND_STRING		= 0x0004
	};
	
protected:
	CStringArray	m_List1;
	CStringArray	m_List2;
	CWordArray		m_Flags;
	CCheckBitmap*	m_pBitmap1;
	CCheckBitmap*	m_pBitmap2;
	CCheckBitmap*	m_pBitmap3;
	CCheckBitmap*	m_pBitmap4;
	CCheckBitmap*	m_pBitmap5;
	int				m_n1stOffset;
	int				m_n2ndOffset;
	MListBoxStyles	m_Style;

public:
	CMultiListBox (UINT nID1 = 0, UINT nID2 = 0, UINT nID3 = 0, UINT nID4 = 0, UINT nID5 = 0);
	CMultiListBox(CString namespace1, CString namespace2 = _T(""), CString namespace3 = _T(""), CString namespace4 = _T(""), CString namespace5 = _T(""));
	~CMultiListBox ();
	
public:
	void	AddString		(int nAt, LPCTSTR, LPCTSTR, ItemStatus Flag = UNCHECKED);
	int		AddString		(LPCTSTR, LPCTSTR, ItemStatus Flag = UNCHECKED);
	void	DelString		(int nIndex);
	int		DelString		(LPCTSTR, LPCTSTR);
	int		SearchString	(LPCTSTR, LPCTSTR);
	void 	ResetContent	();

	CString	GetString1		(int nIndex) const;
	CString	GetString2		(int nIndex) const;

	void		SetFlag			(LPCTSTR, LPCTSTR, ItemStatus Flag);
	void		SetFlag			(int nIndex, ItemStatus Flag);
	ItemStatus	GetFlag			(int nIndex);
	void		SetAllFlags		(ItemStatus Flag);
	void		SetOffsets		(int n1stOffset, int n2ndOffest);
	void		SetStyle		(MListBoxStyles style) { m_Style = style; }

protected:
	int		GetTabWidth		(CString&);
	void	Recalc2ndOffset	(LPCTSTR);
	
protected:
	virtual void 	DrawItem		(LPDRAWITEMSTRUCT);
	virtual void	MeasureItem		(LPMEASUREITEMSTRUCT);
	
	//{{AFX_MSG( CMultiListBox )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
