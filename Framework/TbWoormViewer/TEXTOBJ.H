
#pragma once

#include <TbGeneric\FontsTable.h>
#include <TbGeneric\RdeProtocol.h>

#include <TbGenlib\BarCode.h>

#include "WoormIni.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class Formatter;
class Value;
class CWoormDocMng;
class CBaseDocument;
class SymTable;

//=============================================================================
class TB_EXPORT BasicTextStyle : public CObject
{
public:
	COLORREF	m_rgbTextColor;
	COLORREF	m_rgbBkgColor;
	AlignType	m_nAlign;
	FontIdx		m_nFontIdx;

public:
	// constructors
	BasicTextStyle();
	BasicTextStyle(const BasicTextStyle&);

    // operators
	BasicTextStyle& operator = (const BasicTextStyle&);
	void Assign (const BasicTextStyle&);

	void		SetTextColor	(COLORREF aColor)	{ m_rgbTextColor = aColor;	}
	void		SetBkgColor		(COLORREF aColor)	{ m_rgbBkgColor = aColor;	}
	void		SetAlign		(AlignType anAlign)	{ m_nAlign 	= anAlign;	}
	void		SetFontIdx		(FontIdx aFontIdx)	{ m_nFontIdx	= aFontIdx;	}

	COLORREF	GetTextColor	() const	{ return m_rgbTextColor;	}
	COLORREF	GetBkgColor		() const	{ return m_rgbBkgColor;		}
	AlignType	GetAlign		() const	{ return m_nAlign;			}
	FontIdx		GetFontIdx		() const	{ return m_nFontIdx;		}
};

//=============================================================================
class TB_EXPORT SubTotalStyle : public BasicTextStyle
{
	// subtotal has only style caratteristics
public:
	// constructors
	SubTotalStyle();
	SubTotalStyle(const SubTotalStyle&);
	~SubTotalStyle();

	Expression*		m_pTextColorExpr = NULL;	
	Expression*		m_pBkgColorExpr = NULL;
	BOOL			m_bMiniHtml = FALSE;
	// operators
	SubTotalStyle& operator = (const SubTotalStyle&);
};

//=============================================================================
class TB_EXPORT GenericText : public BasicTextStyle
{
friend class CRS_ObjectPropertyView;
public:
	struct {
		AlignType					m_nAlign;
		CRect						m_rect;
		CString						m_strText;
		CStringArray				m_arTextLine;
		CArray<CRect,const CRect&>	m_arRectLine;
	} m_PreDraw;

protected:
	CString	m_strText;
	BOOL	m_bLocalizable = TRUE;

public:
	typedef enum { NORMAL=0, BROWSED=1, SELECTED=2, WRONG=4, LINKED=8, WRONG_LINK=16, LAST_SELECTED=32 } EnumGenericTextFlags;

	void DrawGText 
			(
				CDC&, const CRect&, 
				CWoormDocMng*, 
				BOOL bOpaque, 
				int flags = NORMAL,	
				CSize Ecc = CSize(0,0),
				BOOL bCalcRect = FALSE
			);
	void SetLocalizable(BOOL b) { m_bLocalizable  = b; }
private:
	void InternalDrawGText
			(
				CDC&		DC,
				CFont*		pFont,
				const CString& str,
				CRect&		rect,
				BOOL		bForceHorizontalAlignOnRotation,
				BOOL		bCalcRect
			);

public:
	// constructors
	GenericText ();
	GenericText (const GenericText&);

    // operators
	GenericText& operator = (const GenericText&);

    // virtual member
	virtual	void	SetText	(const CString& strText);
	virtual	CString	GetText	() const	{ return m_strText; }

	virtual	CString	GetLocalizedText	(CBaseDocument* pDocument) const;

	// base drawing function
	virtual	void Draw (CDC& cdc, CRect& rect, CWoormDocMng* pDoc, BOOL bOpaque = TRUE, int flags = NORMAL, CSize Ecc = CSize(0,0), BOOL bMiniHtml = FALSE, BOOL bPreview = FALSE);

	void DrawMiniHtml (CWoormDocMng* pWDoc, CDC& cdc, CRect& rect, BOOL bTrasparent, BOOL bPreview = FALSE);
};


//===========================================================================
class TB_EXPORT TableTitleText : public GenericText
{
	friend class CRSTableTitlTextProp;

protected:
	CString	m_strTitle;
protected:
	void	FormatWithId		(WORD nID, int nPosition);
	void	FormatWithName		(WORD nID, CWoormDocMng*);
	void	FormatWithValue		();

public:
	// constructors
	TableTitleText();
	TableTitleText(const TableTitleText&);

    // operators
	TableTitleText& operator = (const TableTitleText&);

    // member functions
	int		SetTextDlg	();
	void	Format	(WORD nID, CWoormDocMng*);

	virtual	void	SetText	(const CString& strText);
	virtual	CString	GetText	() const;
};

//===========================================================================
class TB_EXPORT Value : public GenericText
{
protected:
	void	FormatWithId		(WORD nID, int nPosition);
	void	FormatWithName		(WORD nID, CWoormDocMng* pDocument);
public:
	void	FormatWithValue		(Formatter*, CWoormDocMng* pDocument);

public:
	RDEData	m_RDEdata;

public:
	// constructors
	Value	();
	Value	(const Value&);

    // operators
	Value& operator = (const Value&);

    // member functions
	void	Format		(WORD nID, Formatter*, CWoormDocMng*);
	void	DisableData	();
	
	// values must not be translated!!!
	virtual	CString	GetLocalizedText	(CBaseDocument* /*pDocument*/) const	{ return GetText(); }

};

//===========================================================================
class TB_EXPORT StaticText : public GenericText
{
public:
	StaticText	();
	StaticText	(const StaticText&);

    // operators
	StaticText& operator = (const StaticText&);

	int	Query();
};

//===========================================================================
class TB_EXPORT ColumnTitleText : public StaticText
{
public:
	ColumnTitleText	();
	ColumnTitleText	(const ColumnTitleText&);

    // operators
	ColumnTitleText& operator = (const ColumnTitleText&);
};

//===========================================================================
class TB_EXPORT Label : public GenericText
{
public:
	Label	();

	int	Query();

	// base drawing function
	virtual	void Draw (CDC&, CRect&, CWoormDocMng*, BOOL bOpaque = FALSE, int flags = NORMAL, CSize Ecc = CSize(0,0));
};

//===========================================================================

TB_EXPORT BOOL 	CBarCode_Draw(CBarCode* m_pBarCode, CDC&, CRect, CWoormDocMng*, const GenericText&, CString&, int nCurrRow/* = 0*/, BOOL bPreview/* = FALSE*/);


//=============================================================================

#include "endh.dex"
