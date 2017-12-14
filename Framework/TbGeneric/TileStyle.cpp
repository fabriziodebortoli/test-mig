#include "stdafx.h"
#include <TBGeneric\Globals.h>
#include <TBGeneric\TBThemeManager.h>

#include "TileStyle.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 							BaseTileStyle
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT BaseTileStyle : public CObject, public TileStyle
{
	DECLARE_DYNAMIC(BaseTileStyle);

public:
	BaseTileStyle	(CString sName = _T(""));
	virtual ~BaseTileStyle	();

public:
	virtual CString	GetName() { return m_sName; }
	virtual TileStyle::TileAspect	GetAspect					()				{ return m_Aspect; }		
	virtual COLORREF				GetBackgroundColor			()				{ return m_BackgroundColor.GetColor(); }		
	virtual CBrush*					GetBackgroundColorBrush		()				{ return m_BackgroundColor.GetBrush();	}	
	virtual COLORREF				GetStaticAreaBkgColor		()				{ return m_StaticAreaBkgColor.GetColor(); }		
	virtual CBrush*					GetStaticAreaBkgColorBrush	()				{ return m_StaticAreaBkgColor.GetBrush(); }		
	virtual COLORREF				GetTitleBkgColor			()				{ return m_TitleBkgColor.GetColor(); }		
	virtual CBrush*					GetTitleBkgColorBrush		()				{ return m_TitleBkgColor.GetBrush(); }		
	virtual COLORREF				GetTitleForeColor			()				{ return m_TitleForeColor.GetColor(); }		
	virtual CBrush*					GetTitleForeColorBrush		()				{ return m_TitleForeColor.GetBrush(); }		
	virtual COLORREF				GetTitleSeparatorColor		()				{ return m_TitleSeparatorColor.GetColor(); }	
	virtual COLORREF				GetStaticWithLineLineForeColor()			{ return m_StaticWithLineLineForeColor.GetColor(); }
	virtual CBrush*					GetTitleSeparatorColorBrush	()				{ return m_TitleSeparatorColor.GetBrush(); }		
	virtual int						GetTitleAlignment			() const		{ return m_TitleAlignment; }
	virtual int						GetTitlePadding				() const		{ return m_TitlePadding; }
	virtual int						GetTitleTopSeparatorWidth	() const		{ return m_TitleTopSeparatorWidth; }
	virtual int						GetTitleBottomSeparatorWidth() const		{ return m_TitleBottomSeparatorWidth; }
	virtual CFont*					GetTitleFont				()				{ return m_pTitleFont; }			
	virtual BOOL					Collapsible					() const		{ return m_Collapsible; }
	virtual BOOL					HasTitle					() const		{ return m_HasTitle; }
	virtual int						GetTileSpacing				() const		{ return m_TileSpacing; }
	virtual BOOL					Pinnable					() const		{ return m_Pinnable; }
	virtual int						GetMinHeight				() const		{ return m_nMinHeight; }

	// reimplemented, but not meant to be used on "base" styles
	virtual	void	Assign						(TileStyle* pStyle);
	virtual void	SetHasTitle					(BOOL bSet = TRUE);
	virtual void	SetCollapsible				(BOOL bSet = TRUE);
	virtual void	SetPinnable					(BOOL bSet = TRUE);
	virtual void	SetTitleTopSeparatorWidth	(int nValue);
	virtual void	SetTitleBottomSeparatorWidth(int nValue);
	virtual void	SetTileSpacing				(int nValue);
	virtual void	SetAspect(int nValue);
	virtual void	SetTitlePadding(int nValue);
	virtual void	SetBackgroundColor(COLORREF color);
	virtual void	SetStaticAreaBkgColor(COLORREF color);
	virtual void	SetTitleBkgColor(COLORREF color);
	virtual void	SetTitleForeColor(COLORREF color);
	virtual void	SetTitleSeparatorColor(COLORREF color);
	virtual void	SetStaticWithLineLineForeColor(COLORREF color);
	virtual void	SetTitleFont(CFont* pFont, BOOL bOnwsFont =  TRUE);
	
	void	SetMinHeight(int nValue);

	virtual void	OnLoadFromTheme(CXMLNode* pNode);
	virtual void	UseAlternativeColorsOf(TileStyle* pStyle) { }

protected:
	virtual TileStyle*	GetReferenceStyle()		{ return this; }

protected:
	TileStyle::TileAspect	m_Aspect;
	TBThemeColor			m_BackgroundColor;
	TBThemeColor			m_StaticAreaBkgColor;
	TBThemeColor			m_TitleBkgColor;
	TBThemeColor			m_TitleForeColor;
	TBThemeColor			m_TitleSeparatorColor;
	TBThemeColor			m_StaticWithLineLineForeColor;
	int						m_TitleAlignment;
	int						m_TitlePadding;
	int						m_TitleTopSeparatorWidth;
	int						m_TitleBottomSeparatorWidth;
	CFont*					m_pTitleFont;
	BOOL					m_bOwnsFont;
	BOOL					m_Collapsible;
	BOOL					m_HasTitle;
	int						m_TileSpacing;
	BOOL					m_Pinnable;
	int						m_nMinHeight;
	CString					m_sName;
};

IMPLEMENT_DYNAMIC(BaseTileStyle, CObject)

//-----------------------------------------------------------------------------
BaseTileStyle::BaseTileStyle(CString sName /* _T("")*/)
{
	m_bOwnsFont = FALSE;
	m_sName = sName;
	m_Aspect					= TileAspect::FLAT;
	m_nMinHeight = -1;	// AUTO
	m_BackgroundColor			.SetColor(AfxGetThemeManager()->GetBackgroundColor());
	m_StaticAreaBkgColor		.SetColor(AfxGetThemeManager()->GetTileDialogStaticAreaBkgColor());
	m_TitleBkgColor				.SetColor(AfxGetThemeManager()->GetTileDialogTitleBkgColor());
	m_TitleForeColor			.SetColor(AfxGetThemeManager()->GetTileDialogTitleForeColor());
	m_TitleSeparatorColor		.SetColor(AfxGetThemeManager()->GetTileTitleSeparatorColor());
	m_StaticWithLineLineForeColor.SetColor(AfxGetThemeManager()->GetStaticWithLineLineForeColor());

	m_TitleAlignment			= AfxGetThemeManager()->GetTileTitleAlignment();
	m_TitleTopSeparatorWidth	= AfxGetThemeManager()->GetTileTitleTopSeparatorWidth();
	m_TitleBottomSeparatorWidth	= AfxGetThemeManager()->GetTileTitleBottomSeparatorWidth();

	m_pTitleFont				= AfxGetThemeManager()->GetTileDialogTitleFont();

	m_Collapsible				= FALSE;
	m_Pinnable					= FALSE;
	m_HasTitle					= TRUE;

	m_TileSpacing				= AfxGetThemeManager()->GetTileSpacing();
}

//-----------------------------------------------------------------------------
BaseTileStyle::~BaseTileStyle()
{
	SetTitleFont(NULL);
	m_BackgroundColor.Clear();
	m_StaticAreaBkgColor.Clear();
	m_TitleBkgColor.Clear();
	m_TitleForeColor.Clear();
	m_TitleSeparatorColor.Clear();
	m_StaticWithLineLineForeColor.Clear();
}

// TODOBRUNA (trasformare in caricamento automatico)
const TCHAR szTileTitleSeparatorColor[] = _T("TileTitleSeparatorColor");
const TCHAR szTileStyleTitlePadding[] = _T("TitlePadding");
const TCHAR szTileStyleTitleFont[] = _T("TitleFont");

const TCHAR szTileStyleAspect[] = _T("Aspect");
const TCHAR szTileStyleBackgroundColor[] = _T("BackgroundColor");
const TCHAR szTileStyleStaticAreaBkgColor[] = _T("StaticAreaBkgColor");
const TCHAR szTileStyleTitleBkgColor[] = _T("TitleBkgColor");
const TCHAR szTileStyleTitleForeColor[] = _T("TitleForeColor");
const TCHAR szTileStyleTitleSeparatorColor[] = _T("TitleSeparatorColor");
const TCHAR szTileStyleStaticWithLineLineForeColor[] = _T("StaticWithLineLineForeColor");
const TCHAR szTileStyleHasTitle[] = _T("HasTitle");
const TCHAR szTileStyleTileSpacing[] = _T("TileSpacing");
const TCHAR szTileCollapsible[] = _T("Collapsible");
const TCHAR szTilePinnable[] = _T("Pinnable");
const TCHAR szTileTitleTopSeparatorWidth[] = _T("TileTitleTopSeparatorWidth");
const TCHAR szTileTitleBottomSeparatorWidth[] = _T("TileTitleBottomSeparatorWidth");
const TCHAR szTileTitleMinHeight[] = _T("TileMinHeight");

//-----------------------------------------------------------------------------
void BaseTileStyle::OnLoadFromTheme(CXMLNode* pNode)
{
	TBThemeManager* pManager = AfxGetThemeManager();

	for (int n = 0; n < pNode->GetChilds()->GetSize(); n++)
	{
		CXMLNode* pCurrentSubNode = pNode->GetChilds()->GetAt(n);
		CString sSubName;
		if (!pCurrentSubNode->GetName(sSubName) || sSubName.CompareNoCase(pManager->GetElementTag()) != 0)
			continue;

		int nBool = pManager->GetBoolSetting(pCurrentSubNode, szTileStyleHasTitle);
		if (nBool != -1)
			SetHasTitle(nBool);

		nBool = pManager->GetBoolSetting(pCurrentSubNode, szTileCollapsible);
		if (nBool != -1)
			SetCollapsible(nBool);

		nBool = pManager->GetBoolSetting(pCurrentSubNode, szTilePinnable);
		if (nBool != -1)
			SetPinnable(nBool);

		
		int nAspect = pManager->GetIntSetting(pCurrentSubNode, szTileStyleAspect);
		if (nAspect >= 0)
			SetAspect(nAspect);

		int nMinHeight = pManager->GetIntSetting(pCurrentSubNode, szTileTitleMinHeight);
		if (nMinHeight != -1)
			SetMinHeight(nMinHeight);

		int nPadding = pManager->GetIntSetting(pCurrentSubNode, szTileStyleTitlePadding);
		if (nPadding >= 0)
			SetTitlePadding(nPadding);

		COLORREF color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleBackgroundColor);
		if (color != -1)
			SetBackgroundColor(color);

		color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleStaticAreaBkgColor);
		if (color != -1)
			SetStaticAreaBkgColor(color);

		color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleTitleBkgColor);
		if (color != -1)
			SetTitleBkgColor(color);

		color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleTitleForeColor);
		if (color != -1)
			SetTitleForeColor(color);

		color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleTitleSeparatorColor);
		if (color != -1)
			SetTitleSeparatorColor(color);

		color = pManager->GetColorSetting(pCurrentSubNode, szTileStyleStaticWithLineLineForeColor);
		if (color != -1)
			SetStaticWithLineLineForeColor(color);

		int nSpacing = pManager->GetColorSetting(pCurrentSubNode, szTileStyleTileSpacing);
		if (nSpacing >= 0)
			SetTileSpacing(nSpacing);

		int nWidth = pManager->GetIntSetting(pCurrentSubNode, szTileTitleTopSeparatorWidth);
		if (nWidth >= 0)
			SetTitleTopSeparatorWidth(nWidth);

		nWidth = pManager->GetIntSetting(pCurrentSubNode, szTileTitleBottomSeparatorWidth);
		if (nWidth >= 0)
			SetTitleBottomSeparatorWidth(nWidth);

		TBThemeFont aFont; //okkio
		if (pManager->GetFontSetting(pCurrentSubNode, aFont, szTileStyleTitleFont))
		{
			LOGFONT aLogFont;
			aFont.GetFont()->GetLogFont(&aLogFont);

			CFont* pFont = new CFont();

			pFont->CreateFontIndirect(&aLogFont);
			SetTitleFont(pFont, TRUE);
		}
	}

}

//-----------------------------------------------------------------------------
void BaseTileStyle::Assign(TileStyle* pStyle)
{
	ASSERT_TRACE(FALSE, "Assigning to a base style is not allowed. Inherit a customizable copy instead");
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetMinHeight(int nValue)
{
	m_nMinHeight = nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetHasTitle(BOOL bSet /*= TRUE*/)
{
	m_HasTitle = bSet;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetCollapsible(BOOL bSet /*= TRUE*/)
{
	m_Collapsible = bSet;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetPinnable(BOOL bSet /*= TRUE*/)
{
	m_Pinnable = bSet;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleTopSeparatorWidth(int nValue)
{
	m_TitleTopSeparatorWidth = nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleBottomSeparatorWidth(int nValue)
{
	m_TitleBottomSeparatorWidth = nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTileSpacing(int nValue)
{
	m_TileSpacing = nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleFont(CFont* pFont, BOOL bOnwsFont /*TRUE*/)
{
	if (m_bOwnsFont)
	{
		m_pTitleFont->DeleteObject();
		SAFE_DELETE(m_pTitleFont);
	}

	m_bOwnsFont = bOnwsFont;
	m_pTitleFont = pFont;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetAspect(int nValue)
{
	m_Aspect = (TileAspect)nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitlePadding(int nValue)
{
	m_TitlePadding = nValue;
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetBackgroundColor(COLORREF color)
{
	m_BackgroundColor.SetColor(color);
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetStaticAreaBkgColor(COLORREF color)
{
	m_StaticAreaBkgColor.SetColor(color);
}
//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleBkgColor(COLORREF color)
{
	m_TitleBkgColor.SetColor(color);
}
//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleForeColor(COLORREF color)
{
	m_TitleForeColor.SetColor(color);
}

//-----------------------------------------------------------------------------
void BaseTileStyle::SetTitleSeparatorColor(COLORREF color)
{
	m_TitleSeparatorColor.SetColor(color);
}
//-----------------------------------------------------------------------------
void BaseTileStyle::SetStaticWithLineLineForeColor(COLORREF color)
{
	m_StaticWithLineLineForeColor.SetColor(color);
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleNormal()
{
	return AfxGetTileDialogStyle(_T("Normal"));
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleFilter()
{
	return AfxGetTileDialogStyle(_T("Filters"));
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleHeader()
{
	return AfxGetTileDialogStyle(_T("Header"));
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleFooter()
{
	return AfxGetTileDialogStyle(_T("Footer"));
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleBatch()
{
	return AfxGetTileDialogStyle(_T("Batch"));
}


//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleWizard()
{
	return AfxGetTileDialogStyle(_T("Wizard"));
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyleParameters()
{
	return AfxGetTileDialogStyle(_T("Parameters"));
}


////////////////////////////////////////////////////////////////////////////////
//				class CustomizableTileStyle definition
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CustomizableTileStyle : public CObject, public TileStyle
{
	DECLARE_DYNAMIC(CustomizableTileStyle);

public:
	CustomizableTileStyle(TileStyle* pReferenceStyle);
	virtual ~CustomizableTileStyle();

	void Assign(TileStyle* pReferenceStyle);

public:
	virtual CString					GetName						()			{ return m_pReferenceStyle->GetName(); }
	virtual TileStyle::TileAspect	GetAspect					()			{ return m_pReferenceStyle->GetAspect(); }
	virtual COLORREF				GetBackgroundColor			()			{ return m_pAlternativeStyle ? m_pAlternativeStyle ->GetBackgroundColor() : m_pReferenceStyle->GetBackgroundColor(); }
	virtual CBrush*					GetBackgroundColorBrush		()			{ return m_pAlternativeStyle ? m_pAlternativeStyle->GetBackgroundColorBrush() : m_pReferenceStyle->GetBackgroundColorBrush(); }
	virtual COLORREF				GetStaticAreaBkgColor		()			{ return m_pAlternativeStyle ? m_pAlternativeStyle->GetStaticAreaBkgColor() : m_pReferenceStyle->GetStaticAreaBkgColor(); }
	virtual CBrush*					GetStaticAreaBkgColorBrush	()			{ return m_pAlternativeStyle ? m_pAlternativeStyle->GetStaticAreaBkgColorBrush() : m_pReferenceStyle->GetStaticAreaBkgColorBrush(); }
	virtual COLORREF				GetTitleBkgColor			()			{ return m_pAlternativeStyle ? m_pAlternativeStyle->GetTitleBkgColor() : m_pReferenceStyle->GetTitleBkgColor(); }
	virtual CBrush*					GetTitleBkgColorBrush		()			{ return m_pAlternativeStyle ? m_pAlternativeStyle->GetTitleBkgColorBrush() : m_pReferenceStyle->GetTitleBkgColorBrush(); }
	virtual COLORREF				GetTitleForeColor			()			{ return m_pReferenceStyle->GetTitleForeColor(); }
	virtual CBrush*					GetTitleForeColorBrush		()			{ return m_pReferenceStyle->GetTitleForeColorBrush(); }
	virtual COLORREF				GetTitleSeparatorColor		()			{ return m_pReferenceStyle->GetTitleSeparatorColor(); }
	virtual CBrush*					GetTitleSeparatorColorBrush	()			{ return m_pReferenceStyle->GetTitleSeparatorColorBrush(); }
	virtual int						GetTitleAlignment			() const	{ return m_pReferenceStyle->GetTitleAlignment(); }
	virtual int						GetTitlePadding				() const	{ return m_pReferenceStyle->GetTitlePadding(); }
	virtual CFont*					GetTitleFont				()			{ return m_pReferenceStyle->GetTitleFont(); }
	virtual COLORREF				GetStaticWithLineLineForeColor()		{ return m_pReferenceStyle->GetStaticWithLineLineForeColor(); }
	virtual int		GetMinHeight() const		{ return m_pReferenceStyle->GetMinHeight(); }

	// locally customizable properties
	virtual int						GetTitleTopSeparatorWidth	() const;
	virtual int						GetTitleBottomSeparatorWidth() const;
	virtual BOOL					Collapsible					() const;
	virtual BOOL					Pinnable					() const;
	virtual BOOL					HasTitle					() const;
	virtual int						GetTileSpacing				() const;

public:
	virtual void	SetHasTitle					(BOOL bSet = TRUE);
	virtual void	SetCollapsible				(BOOL bSet = TRUE);
	virtual void	SetPinnable					(BOOL bSet = TRUE);
	virtual void	SetTitleTopSeparatorWidth	(int nValue);
	virtual void	SetTitleBottomSeparatorWidth(int nValue);
	virtual void	SetTileSpacing				(int nvalue);
	virtual void	SetTitlePadding				(int nvalue);
	virtual void	OnLoadFromTheme				(CXMLNode* pNode) {}
	virtual void	UseAlternativeColorsOf(TileStyle* pStyle);

protected:
	virtual TileStyle*	GetReferenceStyle()		{ return m_pReferenceStyle; }

private:
	TileStyle*	m_pReferenceStyle;
	TileStyle*	m_pAlternativeStyle;
	BOOL*		m_pbHasTitle;
	BOOL*		m_pbCollapsible;
	BOOL*		m_pbPinnable;
	int*		m_pnTitleTopSeparatorWidth;
	int*		m_pnTitleBottomSeparatorWidth;
	int*		m_pnTileSpacing;
};

IMPLEMENT_DYNAMIC(CustomizableTileStyle, CObject)

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
TileStyle* TileStyle::Inherit(TileStyle* pReferenceStyle)
{
	return new CustomizableTileStyle(pReferenceStyle);
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
CustomizableTileStyle::CustomizableTileStyle(TileStyle* pStyle)
	:
	m_pbHasTitle					(NULL),
	m_pbCollapsible					(NULL),
	m_pbPinnable					(NULL),
	m_pnTitleTopSeparatorWidth		(NULL),
	m_pnTitleBottomSeparatorWidth	(NULL),
	m_pnTileSpacing					(NULL),
	m_pAlternativeStyle				(NULL)
{ 
	Assign(pStyle);
}

//-----------------------------------------------------------------------------
CustomizableTileStyle::~CustomizableTileStyle()
{
	SAFE_DELETE(m_pbHasTitle);
	SAFE_DELETE(m_pbCollapsible);
	SAFE_DELETE(m_pbPinnable);
	SAFE_DELETE(m_pnTitleTopSeparatorWidth);
	SAFE_DELETE(m_pnTitleBottomSeparatorWidth);
	SAFE_DELETE(m_pnTileSpacing);
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::Assign(TileStyle* pStyle)
{
	m_pReferenceStyle = pStyle->GetReferenceStyle();

	if (!m_pbHasTitle && pStyle->HasTitle() != m_pReferenceStyle->HasTitle())
		SetHasTitle(pStyle->HasTitle());
	if (!m_pbCollapsible && pStyle->Collapsible() != m_pReferenceStyle->Collapsible())
		SetCollapsible(pStyle->Collapsible());
	if (!m_pbPinnable && pStyle->Pinnable() != m_pReferenceStyle->Pinnable())
		SetPinnable(pStyle->Pinnable());
	if (!m_pnTitleTopSeparatorWidth && pStyle->GetTitleTopSeparatorWidth() != m_pReferenceStyle->GetTitleTopSeparatorWidth())
		SetTitleTopSeparatorWidth(pStyle->GetTitleTopSeparatorWidth());
	if (!m_pnTitleBottomSeparatorWidth && pStyle->GetTitleBottomSeparatorWidth() != m_pReferenceStyle->GetTitleBottomSeparatorWidth())
		SetTitleBottomSeparatorWidth(pStyle->GetTitleBottomSeparatorWidth());
	if (!m_pnTileSpacing && pStyle->GetTileSpacing() != m_pReferenceStyle->GetTileSpacing())
		SetTileSpacing(pStyle->GetTileSpacing());
}

//-----------------------------------------------------------------------------
BOOL CustomizableTileStyle::Collapsible() const
{
	if (m_pbCollapsible)
		return *m_pbCollapsible;
	else
		return m_pReferenceStyle->Collapsible();
}

//-----------------------------------------------------------------------------
BOOL CustomizableTileStyle::Pinnable() const
{
	if (m_pbPinnable)
		return *m_pbPinnable;
	else
		return m_pReferenceStyle->Pinnable();
}

//-----------------------------------------------------------------------------
BOOL CustomizableTileStyle::HasTitle() const
{
	if (m_pbHasTitle)
		return *m_pbHasTitle;
	else
		return m_pReferenceStyle->HasTitle();
}

//-----------------------------------------------------------------------------
int CustomizableTileStyle::GetTitleTopSeparatorWidth() const
{
	if (m_pnTitleTopSeparatorWidth)
		return *m_pnTitleTopSeparatorWidth;
	else
		return m_pReferenceStyle->GetTitleTopSeparatorWidth();
}

//-----------------------------------------------------------------------------
int CustomizableTileStyle::GetTitleBottomSeparatorWidth() const
{
	if (m_pnTitleBottomSeparatorWidth)
		return *m_pnTitleBottomSeparatorWidth;
	else
		return m_pReferenceStyle->GetTitleBottomSeparatorWidth();
}

//-----------------------------------------------------------------------------
int CustomizableTileStyle::GetTileSpacing() const
{
	if (m_pnTileSpacing)
		return *m_pnTileSpacing;
	else
		return m_pReferenceStyle->GetTileSpacing();
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetCollapsible(BOOL bSet /*= TRUE*/)
{
	if (!m_pbCollapsible)
		m_pbCollapsible = new BOOL(bSet);
	else
		*m_pbCollapsible = bSet;
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetPinnable(BOOL bSet /*= TRUE*/)
{
	if (!m_pbPinnable)
		m_pbPinnable = new BOOL(bSet);
	else
		*m_pbPinnable = bSet;
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetHasTitle(BOOL bSet /*= TRUE*/)
{
	if (!m_pbHasTitle)
		m_pbHasTitle = new BOOL(bSet);
	else
		*m_pbHasTitle = bSet;
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetTitleTopSeparatorWidth(int nValue)
{
	if (!m_pnTitleTopSeparatorWidth)
		m_pnTitleTopSeparatorWidth = new int(nValue);
	else
		*m_pnTitleTopSeparatorWidth = nValue;
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetTitleBottomSeparatorWidth(int nValue)
{
	if (!m_pnTitleBottomSeparatorWidth)
		m_pnTitleBottomSeparatorWidth = new int(nValue);
	else
		*m_pnTitleBottomSeparatorWidth = nValue;
}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::SetTileSpacing(int nValue)
{
	if (!m_pnTileSpacing)
		m_pnTileSpacing = new int(nValue);
	else
		*m_pnTileSpacing = nValue;
}

void CustomizableTileStyle::SetTitlePadding(int nvalue)
{

}

//-----------------------------------------------------------------------------
void CustomizableTileStyle::UseAlternativeColorsOf(TileStyle* pStyle)
{
	m_pAlternativeStyle = pStyle;
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTilePanelStyleNormal()
{
	return AfxGetTileDialogStyle(_T("TilePanel"));
}

TileStyle* AFXAPI AfxGetTileDialogStyle(TileDialogStyle style)
{
	switch (style)
	{
	case TileDialogStyle::TDS_NORMAL:
		return AfxGetTileDialogStyleNormal();
	case TileDialogStyle::TDS_FILTER:
		return AfxGetTileDialogStyleFilter();
	case TileDialogStyle::TDS_HEADER:
		return AfxGetTileDialogStyleHeader();
	case TileDialogStyle::TDS_FOOTER:
		return AfxGetTileDialogStyleFooter();
	case TileDialogStyle::TDS_WIZARD:
		return AfxGetTileDialogStyleWizard();
	case TileDialogStyle::TDS_PARAMETERS:
		return AfxGetTileDialogStyleParameters();
	case TileDialogStyle::TDS_BATCH:
		return AfxGetTileDialogStyleBatch();
	default:
		return AfxGetTileDialogStyleNormal();
	}
}

//-----------------------------------------------------------------------------
TileStyle* AFXAPI AfxGetTileDialogStyle(CString sName, BOOL bCreate /*TRUE*/)
{
	TileStyle* pStyle = dynamic_cast<TileStyle*>(AfxGetThemeManager()->GetThemeObject(sName));
	if (pStyle)
		return pStyle;
	
	if (bCreate)
	{
		pStyle = dynamic_cast<TileStyle*>(new BaseTileStyle(sName));

		CObject* pThemeObject = dynamic_cast<CObject*>(pStyle);
		AfxGetThemeManager()->AddThemeObject(sName, pThemeObject);
	}
	return pStyle;
}

/*

RGB <-> HSV
source> http://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both

typedef struct {
    double r;       // percent
    double g;       // percent
    double b;       // percent
} rgb;

    typedef struct {
    double h;       // angle in degrees
    double s;       // percent
    double v;       // percent
} hsv;

    static hsv      rgb2hsv(rgb in);
    static rgb      hsv2rgb(hsv in);

hsv rgb2hsv(rgb in)
{
    hsv         out;
    double      min, max, delta;

    min = in.r < in.g ? in.r : in.g;
    min = min  < in.b ? min  : in.b;

    max = in.r > in.g ? in.r : in.g;
    max = max  > in.b ? max  : in.b;

    out.v = max;                                // v
    delta = max - min;
    if( max > 0.0 ) { // NOTE: if Max is == 0, this divide would cause a crash
        out.s = (delta / max);                  // s
    } else {
        // if max is 0, then r = g = b = 0              
            // s = 0, v is undefined
        out.s = 0.0;
        out.h = NAN;                            // its now undefined
        return out;
    }
    if( in.r >= max )                           // > is bogus, just keeps compilor happy
        out.h = ( in.g - in.b ) / delta;        // between yellow & magenta
    else
    if( in.g >= max )
        out.h = 2.0 + ( in.b - in.r ) / delta;  // between cyan & yellow
    else
        out.h = 4.0 + ( in.r - in.g ) / delta;  // between magenta & cyan

    out.h *= 60.0;                              // degrees

    if( out.h < 0.0 )
        out.h += 360.0;

    return out;
}


rgb hsv2rgb(hsv in)
{
    double      hh, p, q, t, ff;
    long        i;
    rgb         out;

    if(in.s <= 0.0) {       // < is bogus, just shuts up warnings
        out.r = in.v;
        out.g = in.v;
        out.b = in.v;
        return out;
    }
    hh = in.h;
    if(hh >= 360.0) hh = 0.0;
    hh /= 60.0;
    i = (long)hh;
    ff = hh - i;
    p = in.v * (1.0 - in.s);
    q = in.v * (1.0 - (in.s * ff));
    t = in.v * (1.0 - (in.s * (1.0 - ff)));

    switch(i) {
    case 0:
        out.r = in.v;
        out.g = t;
        out.b = p;
        break;
    case 1:
        out.r = q;
        out.g = in.v;
        out.b = p;
        break;
    case 2:
        out.r = p;
        out.g = in.v;
        out.b = t;
        break;

    case 3:
        out.r = p;
        out.g = q;
        out.b = in.v;
        break;
    case 4:
        out.r = t;
        out.g = p;
        out.b = in.v;
        break;
    case 5:
    default:
        out.r = in.v;
        out.g = p;
        out.b = q;
        break;
    }
    return out;     
}
*/
