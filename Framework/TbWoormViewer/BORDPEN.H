
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class ViewParser;
class ViewUnparser;

//=========================================================================
class TB_EXPORT BorderPen
{
public:
	COLORREF	m_rgbColor;
	int 		m_nWidth;
	
public:
	BorderPen	();
    BorderPen	(int, COLORREF);
	BorderPen	(COLORREF);
	BorderPen	(const BorderPen&);

    // operators
	BorderPen&	operator=	(const BorderPen&);
	BOOL		operator!=	(const BorderPen&) const;
	BOOL		operator==	(const BorderPen&) const;

	BOOL		IsDefault	() const;
	void		SetColor	(COLORREF rgbColor)	{ m_rgbColor = rgbColor; }
	COLORREF	GetColor	() const			{ return m_rgbColor; }

	void	SetWidth	(int nWidth)	{ m_nWidth = nWidth; }
	int		GetWidth	() const 		{ return m_nWidth; }
};

//===========================================================================
class TB_EXPORT Borders
{
public:
	BOOL	left;
	BOOL	right;
	BOOL	top;
	BOOL	bottom;

public:
	Borders(BOOL = TRUE);
	Borders(BOOL top, BOOL left, BOOL bottom, BOOL right);
	Borders(const Borders&);

	BOOL	IsDefault	() const	{ return left && right && top && bottom; }
	BOOL	Exists		() const	{ return left || right || top || bottom; }

	BOOL		operator ==	(const Borders&) const;
	BOOL		operator !=	(const Borders&) const;
	Borders&	operator =	(const Borders&);

	void Update(CString b);
};

//===========================================================================
class TB_EXPORT CellBorders : public Borders
{
public:
	BorderPen* m_pColumnPen;
	BorderPen* m_pRowSepPen;

	CellBorders(BOOL top, BOOL left, BOOL bottom, BOOL right, BorderPen* pColumnPen, BorderPen* pRowSepPen)
		: 
		Borders(top, left, bottom, right),
		m_pColumnPen (pColumnPen),
		m_pRowSepPen (pRowSepPen)
		{}

	CellBorders(const CellBorders& cb)
		{
			*this = cb;
		}

	CellBorders(const Borders& b)
		: 
		Borders(b.top, b.left, b.bottom, b.right),
		m_pColumnPen (NULL),
		m_pRowSepPen (NULL)
		{}


	CellBorders&	operator =	(const CellBorders& cb)
		{
			(Borders&)*this = (Borders&)cb;
			m_pColumnPen = cb.m_pColumnPen;
			m_pRowSepPen = cb.m_pRowSepPen;
			return *this;
		}

	CellBorders&	operator =	(const Borders& b)
		{
			(Borders&)*this = b;
			return *this;
		}

};

//===========================================================================
class TB_EXPORT TableBorders
{
public:
//TABLE TITLES
	BOOL	m_bTableTitleTop;
	BOOL	m_bTableTitleBottom;
	BOOL	m_bTableTitleLeft;
	BOOL	m_bTableTitleRight;

//COLUMN TITLES
	BOOL	m_bColumnTitleTop;
	BOOL	m_bColumnTitleBottom;
	BOOL	m_bColumnTitleLeft;
	BOOL	m_bColumnTitleRight;

	BOOL	m_bColumnTitleSeparator;

//BODY
	BOOL	m_bBodyTop;
	BOOL	m_bBodyBottom;
	BOOL	m_bBodyLeft;
	BOOL	m_bBodyRight;

	BOOL	m_bColumnSeparator;

	//proprietà ternaria: no, separatore semplice, multiline
	BOOL	m_bRowSeparator;
	BOOL	m_bRowSeparatorDynamic;	//separatore fra righe multiline
	
	//TODO manca editor 
	BorderPen* m_pRowSepPen = NULL;
	BorderPen* m_pColumnSepPen = NULL;
	BorderPen* m_pColumnTitleSepPen = NULL;
	BorderPen* m_pInterlinePen = NULL;

//TOTAL
	BOOL	m_bTotalTop;
	BOOL	m_bTotalBottom;
	BOOL	m_bTotalLeft;
	BOOL	m_bTotalRight;

protected:
	int		Changed		() const;
	BOOL	IsDefault	() const;

	BOOL	ParseBorders	(ViewParser& lex);
	BOOL	ParseBorder		(ViewParser& lex, BOOL blk = TRUE);
	BOOL	ParseBlock		(ViewParser& lex);

public:
	TableBorders ();
	TableBorders (BOOL bEnable);
	TableBorders (const TableBorders&);
	virtual ~TableBorders () 
			{ SAFE_DELETE(m_pRowSepPen); SAFE_DELETE(m_pInterlinePen); 
			 SAFE_DELETE(m_pColumnSepPen); SAFE_DELETE(m_pColumnTitleSepPen); }

	void	Init	(BOOL enabled);
	BOOL	Parse	(ViewParser&);
	void	Unparse	(ViewUnparser&);

	BOOL			operator ==	(const TableBorders&) const;
	BOOL			operator !=	(const TableBorders&) const;
	TableBorders&	operator =	(const TableBorders&);
};
///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
