#include "stdafx.h"

#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\TBResourceLocker.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\JsonFormEngine.h>
#include <TbGeneric\TileStyle.h>

#include <TbFrameworkImages\GeneralFunctions.h>

#include "FontsTable.h"

#include "TBThemeManager.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					General Functions
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
TBThemeManager* AFXAPI AfxGetThemeManager()
{
	return AfxGetLoginContext()->GetObject<TBThemeManager>(&CLoginContext::GetThemeManager);
}

//-----------------------------------------------------------------------------
COLORREF AFXAPI AfxGetBkgColor()
{
	return AfxGetThemeManager()->GetBackgroundColor();
}

//-----------------------------------------------------------------------------
CBrush* AFXAPI AfxGetBkgColorBrush()
{
	return AfxGetThemeManager()->GetBackgroundColorBrush();
}

//-----------------------------------------------------------------------------
CFont* AFXAPI AfxGetControlFont()
{
	return AfxGetThemeManager()->GetControlFont();
}

//-----------------------------------------------------------------------------
CFont* AFXAPI AfxGetHyperlinkFont()
{
	return AfxGetThemeManager()->GetHyperlinkFont();
}

//-----------------------------------------------------------------------------
CFont* AFXAPI AfxGetFormFont()
{
	return AfxGetThemeManager()->GetFormFont();
}

//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsOSVista()
{
	return IsOSVista();
}

//-----------------------------------------------------------------------------
int ConvertToFontSize(CString originalFontSize)
{
	return(_ttoi(originalFontSize)); 
}

//-----------------------------------------------------------------------------
BOOL ConvertToBool(CString originalValue)
{
	return (originalValue.CompareNoCase(_T("1")) == 0 || originalValue.CompareNoCase(_T("true")) == 0) ? TRUE : FALSE;
}

//-----------------------------------------------------------------------------
COLORREF ConvertHexColorToColorRef(CString val)
{
	LPCTSTR pszTmp = val;
	pszTmp++; // cut the #

	LPTSTR pStop;
	INT nTmp = _tcstol(pszTmp, &pStop, 16);
	INT nR = (nTmp & 0xFF0000) >> 16;
	INT nG = (nTmp & 0xFF00) >> 8;
	INT nB = (nTmp & 0xFF);

	return RGB(nR, nG, nB);
}

/////////////////////////////////////////////////////////////////////////////
//					TBThemeColor
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
ScalingInfo::ScalingInfo(const TBThemeFont& aFont)
{
	//creo una finestra col font del tema e mi faccio trasformare le coordinate in base ad esso
	DialogTemplate tmp;
	// Write out the extended dialog template header
	tmp.Write<WORD>(1); // dialog version
	tmp.Write<WORD>(0xFFFF); // extended dialog template
	tmp.Write<DWORD>(0); // help ID
	tmp.Write<DWORD>(0); // extended style
	tmp.Write<DWORD>(DS_SETFONT);
	tmp.Write<WORD>(0); // number of controls
	tmp.Write<WORD>((WORD)0); // X
	tmp.Write<WORD>((WORD)0); // Y
	tmp.Write<WORD>(1); // width
	tmp.Write<WORD>(1); // height
	tmp.WriteString(L""); // no menu
	tmp.WriteString(L""); // default dialog class
	tmp.WriteString(L""); // title

	int nPoint = aFont.GetFontSize();
	tmp.Write<WORD>(nPoint); // point
	tmp.Write<WORD>((WORD)0); // weight
	tmp.Write<BYTE>(0); // Italic
	tmp.Write<BYTE>(0); // CharSet
	tmp.WriteString(aFont.GetFontName());
	int size;
	LPCDLGTEMPLATE t = tmp.Template(size);

	HWND hMappingWindow = CreateDialogIndirect(NULL, t, NULL, NULL);
	
	RECT rc;
	rc.left = 0;
	rc.top = 0;
	rc.right = 100;
	rc.bottom = 100;
	::MapDialogRect(hMappingWindow, &rc);

	m_nBaseUnitsHeight	= rc.bottom;
	m_nBaseUnitsWidth	= rc.right;
	
	DestroyWindow(hMappingWindow);
}

/////////////////////////////////////////////////////////////////////////////
//					TBThemeColor
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
TBThemeColor::TBThemeColor()
	:
	m_Color(NULL)
{
}

//-----------------------------------------------------------------------------
TBThemeColor::TBThemeColor(COLORREF color)
	:
	m_Color(NULL)
{
	SetColor(color);
}

//-----------------------------------------------------------------------------
TBThemeColor::~TBThemeColor()
{
	Clear();
}

//-----------------------------------------------------------------------------
inline COLORREF TBThemeColor::GetColor()
{
	return m_Color;
}

//-----------------------------------------------------------------------------
CBrush*	TBThemeColor::GetBrush()
{
	if (m_Color != NULL && m_Brush.m_hObject == NULL)
		m_Brush.CreateSolidBrush(m_Color);

	return &m_Brush;
}

//-----------------------------------------------------------------------------
inline BOOL TBThemeColor::IsEmpty() const
{
	return m_Color == NULL;
}

//-----------------------------------------------------------------------------
void TBThemeColor::Clear()
{
	if (m_Brush.Detach())
		m_Brush.DeleteObject();
	m_Color = NULL;
}

//-----------------------------------------------------------------------------
CString TBThemeColor::GetHexColor()
{
	return m_strColorHex; 
}

//-----------------------------------------------------------------------------
void TBThemeColor::SetColor(CString hexColor)
{
	Clear();
	COLORREF  colorRef = ConvertHexColorToColorRef(hexColor);
	if (colorRef >= 0)
	{
		m_strColorHex = hexColor;
		m_Color = colorRef;
	}
}

//-----------------------------------------------------------------------------
void TBThemeColor::SetColor(COLORREF color)
{
	Clear();
	m_Color = color;
}

/////////////////////////////////////////////////////////////////////////////
//					TBThemeFont
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
TBThemeFont::TBThemeFont()
	:
	m_pFont(NULL),
	m_pSmallFont(NULL)
{
}

//-----------------------------------------------------------------------------
CFont* TBThemeFont::GetFont()
{
	return m_pFont;
}

//-----------------------------------------------------------------------------
TBThemeFont::~TBThemeFont()
{
	if (m_pFont && m_pFont->m_hObject)
		m_pFont->DeleteObject();

	if (m_pSmallFont && m_pSmallFont->m_hObject)
		m_pSmallFont->DeleteObject();

	SAFE_DELETE(m_pFont);
	SAFE_DELETE(m_pSmallFont);
}

//-----------------------------------------------------------------------------
CFont* TBThemeFont::GetSmallFont()
{
	return m_pSmallFont;
}

//-----------------------------------------------------------------------------
void TBThemeFont::SetFont
(
const CString& sFFName,
int nSize,
const CString& sFFChSet,
int nSmallSize,
BOOL bUnderline,
BOOL bItalic,
BOOL bStriked,
int nWeight
)
{
	if (!m_pFont)
		m_pFont = new CFont();

	if (!CreateFont(m_pFont, sFFName, nSize, sFFChSet, bUnderline, bItalic, bStriked, nWeight))
	{
		ASSERT_TRACE1(FALSE, "Failed to create font %s", (LPCTSTR)sFFName);

		CreateFont(m_pFont, AfxGetThemeManager()->GetAlternativeFontFaceName(), nSize, sFFChSet, bUnderline, bItalic, bStriked, nWeight);
	};

	if (nSmallSize > 0)
	{
		if (!m_pSmallFont)
			m_pSmallFont = new CFont();
		CreateFont(m_pSmallFont, sFFName, nSmallSize, sFFChSet, bUnderline, bItalic, bStriked, nWeight);
	}

	m_strFFName = sFFName;
	m_nSize		= nSize;
}

//-----------------------------------------------------------------------------
BOOL TBThemeFont::CreateFont
(
CFont* pFont,
const CString& sFFName,
int nSize, const
CString& sFFChSet,
BOOL bUnderline,
BOOL bItalic,
BOOL bStriked,
int nWeight
)
{
	if (pFont == NULL)
		return FALSE;

	if (pFont->m_hObject)
		pFont->DeleteObject();

	// scaling
	int nFontHeight = (nSize * GetLogPixels()) / 72;

	return pFont->CreateFont
		(
		-nFontHeight, 0, 0, 0,
		nWeight, bItalic, bUnderline, bStriked, GetCharsetFromName(sFFChSet),
		OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
		DEFAULT_PITCH, sFFName
		);
}

//-----------------------------------------------------------------------------
void TBThemeFont::CreateFont(LOGFONT& lf)
{
	if (m_pFont)
	{
		if (m_pFont->m_hObject)
			m_pFont->DeleteObject();
		delete m_pFont;
		m_pFont = NULL;
	}
	m_pFont = new CFont();

	m_pFont->CreateFontIndirect(&lf);
}

//-----------------------------------------------------------------------------
void TBThemeFont::CloneFont(CFont* pFont)
{
	LOGFONT lf;
	pFont->GetLogFont(&lf);

	CreateFont(lf);
}

//-----------------------------------------------------------------------------
BYTE TBThemeFont::GetCharsetFromName(const CString& strCharSetName)
{
	// NOTA: sono solo alcuni di quelli definiti in WINGDI.H, quelli che (forse)
	// logicamente possono essere utili in Europa e dintorni. I charset asiatici
	// sono inutilizzabili in pratica senza UNICODE
	if (strCharSetName.CompareNoCase(L"DEFAULT") == 0)		return DEFAULT_CHARSET;
	if (strCharSetName.CompareNoCase(L"ANSI") == 0)			return ANSI_CHARSET;
	if (strCharSetName.CompareNoCase(L"GREEK") == 0)		return GREEK_CHARSET;
	if (strCharSetName.CompareNoCase(L"EASTEUROPE") == 0)	return EASTEUROPE_CHARSET;
	if (strCharSetName.CompareNoCase(L"RUSSIAN") == 0)		return RUSSIAN_CHARSET;
	if (strCharSetName.CompareNoCase(L"OEM") == 0)			return OEM_CHARSET;
	if (strCharSetName.CompareNoCase(L"BALTIC") == 0)		return BALTIC_CHARSET;

	return DEFAULT_CHARSET;
}

/////////////////////////////////////////////////////////////////////////////
//					class TBThemeToolBarImageList implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
TBThemeToolBarImageList::TBThemeToolBarImageList()
{
	m_sName = _T("");
	m_sValue = _T("");
}

//-----------------------------------------------------------------------------
TBThemeToolBarImageList::TBThemeToolBarImageList(CString sName, CString	sValue)
{
	m_sName = sName;
	m_sValue = sValue;
}

//-----------------------------------------------------------------------------
TBThemeToolBarImageList::~TBThemeToolBarImageList()
{
}

//-----------------------------------------------------------------------------
CString TBThemeToolBarImageList::GetName()
{
	return m_sName;
}

//-----------------------------------------------------------------------------
CString TBThemeToolBarImageList::GetValue()
{
	return m_sValue;
}

//-----------------------------------------------------------------------------
TBThemeColor	TBThemeManager::m_FontDefaultForeColor = CLR_BLACK;

/////////////////////////////////////////////////////////////////////////////
//					class TBThemeManager implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(TBThemeManager, CObject)
//-----------------------------------------------------------------------------
TBThemeManager::TBThemeManager()
	:
	m_pScalingInfo(NULL)

{
	Clear();
	CString sManifestName = AfxGetPathFinder()->GetTBDllPath() + SLASH_CHAR + szTBLoader + _T(".exe.manifest");
	m_bHasManifest = ExistFile(sManifestName);

	m_pTransparentBrush = new CBrush();
	m_pTransparentBrush->FromHandle((HBRUSH)::GetStockObject(NULL_BRUSH));


}

//-----------------------------------------------------------------------------
TBThemeManager::~TBThemeManager()
{
	if (m_pTransparentBrush)
		m_pTransparentBrush->Detach();
	SAFE_DELETE(m_pTransparentBrush);

	RemoveThemeObjects();

	SAFE_DELETE(m_pScalingInfo);
}

//.Theme........................................................
const TCHAR szColors[] = _T("Colors");
const TCHAR szFonts[] = _T("Fonts");
const TCHAR szThemeManagerFlags[] = _T("Flags");
const TCHAR szThemeManagerNumeric[] = _T("Numeric");
const TCHAR szThemeManagerImages[] = _T("Images");
const TCHAR szTileTitleSeparatorColor[] = _T("TileTitleSeparatorColor");
const TCHAR szFocusedControlBkgColorEnabled[] = _T("FocusedControlBkgColorEnabled");
const TCHAR szBkgColor[] = _T("BkgColor");
const TCHAR szDocumentBkgColor[] = _T("DocumentBkgColor");
const TCHAR szFocusedControlBkgColor[] = _T("FocusedControlBkgColor");
const TCHAR szEnabledControlBkgColor[] = _T("EnabledControlBkgColor");
const TCHAR szEnabledControlForeColor[] = _T("EnabledControlForeColor");
const TCHAR szDisabledControlForeColor[] = _T("DisabledControlForeColor");
const TCHAR szDisabledInStaticAreaControlForeColor[] = _T("DisabledInStaticAreaControlForeColor");
const TCHAR szEnabledControlBorderForeColor[] = _T("EnabledControlBorderForeColor");
const TCHAR szStaticControlForeColor[] = _T("StaticControlForeColor");
const TCHAR szTooltipBkgColor[] = _T("TooltipBkgColor");
const TCHAR szTooltipForeColor[] = _T("TooltipForeColor");
const TCHAR szRadarVerticalLineColor[] = _T("RadarVerticalLineColor");
const TCHAR szRadarTitleBarButtonBkgColor[] = _T("RadarTitleBarButtonBkgColor");
const TCHAR szRadarTitleBarSelectedButtonBkgColor[] = _T("RadarTitleBarSelectedButtonBkgColor");
const TCHAR szRadarTitleBarButtonBorderBkgColor[] = _T("RadarTitleBarButtonBorderBkgColor");
const TCHAR szRadarTitleBarLineColor[] = _T("RadarTitleBarLineColor");
const TCHAR szRadarSeparatorColor[] = _T("RadarSeparatorColor");
const TCHAR szRadarColumnBorderColor[] = _T("RadarColumnBorderColor");
const TCHAR szRadarColumnTitleBorderColor[] = _T("RadarColumnTitleBorderColor");
const TCHAR szRadarPageBkgColor[] = _T("RadarPageBkgColor");
const TCHAR szPerformanceAnalyzerBkgColor[] = _T("PerformanceAnalyzerBkgColor");
const TCHAR szPerformanceAnalyzerForeColor[] = _T("PerformanceAnalyzerForeColor");
const TCHAR szTreeViewBkgColor[] = _T("TreeViewBkgColor");
const TCHAR szTreeViewNodeForeColor[] = _T("TreeViewNodeForeColor");
const TCHAR szBCMenuBitmapBkgColor[] = _T("BCMenuBitmapBkgColor");
const TCHAR szBCMenuBkgColor[] = _T("BCMenuBkgColor");
const TCHAR szBCMenuForeColor[] = _T("BCMenuForeColor");
const TCHAR szBETransparentBmpColor[] = _T("BETransparentBmpColor");
const TCHAR szBEToolbarBtnBitmapBkgColor[] = _T("BEToolbarBtnBitmapBkgColor");
const TCHAR szBEToolbarBtnBkgColor[] = _T("BEToolbarBtnBkgColor");
const TCHAR szBEToolbarBtnForeColor[] = _T("BEToolbarBtnForeColor");
const TCHAR szToolbarSeparatorColor[] = _T("ToolbarSeparatorColor");
const TCHAR szCISBitmapBkgColor[] = _T("CISBitmapBkgColor");
const TCHAR szCISBitmapForeColor[] = _T("CISBitmapForeColor");
const TCHAR szTransBmpTransparentDefaultColor[] = _T("TransBmpTransparentDefaultColor");
const TCHAR szTBExtDropBitmapsTransparentColor[] = _T("TBExtDropBitmapsTransparentColor");
const TCHAR szHyperLinkForeColor[] = _T("HyperLinkForeColor");
const TCHAR szHyperLinkBrowsedForeColor[] = _T("HyperLinkBrowsedForeColor");
const TCHAR szDropDownEnabledGrdTopColor[] = _T("DropDownEnabledGrdTopColor");
const TCHAR szDropDownEnabledGrdBottomColor[] = _T("DropDownEnabledGrdBottomColor");
const TCHAR szEditEditableOutBorderBkgColor[] = _T("EditEditableOutBorderBkgColor");
const TCHAR szEditNonEditableOutBorderBkgColor[] = _T("EditNonEditableOutBorderBkgColor");
const TCHAR szEditInBorderBkgColor[] = _T("EditInBorderBkgColor");
const TCHAR szStaticWithLineLineForeColor[] = _T("StaticWithLineLineForeColor");
const TCHAR szStaticControlOutBorderBkgColor[] = _T("StaticControlOutBorderBkgColor");
const TCHAR szStaticControlInBorderBkgColor[] = _T("StaticControlInBorderBkgColor");
const TCHAR szColoredControlBorderColor[] = _T("ColoredControlBorderColor");
const TCHAR szStretchCtrlTransparentColor[] = _T("StretchCtrlTransparentColor");
const TCHAR szStretchCtrlForeColor[] = _T("StretchCtrlForeColor");
const TCHAR szBERowSelectedFillColor[] = _T("BERowSelectedFillColor");
const TCHAR szBETooltipBkgColor[] = _T("BETooltipBkgColor");
const TCHAR szBETooltipForeColor[] = _T("BETooltipForeColor");
const TCHAR szBERowSelectedBkgColor[] = _T("BERowSelectedBkgColor");
const TCHAR szBERowSelectedForeColor[] = _T("BERowSelectedForeColor");
const TCHAR szBERowBkgAlternateColor[] = _T("BERowBkgAlternateColor");
const TCHAR szBESeparatorColor[] = _T("BESeparatorColor");
const TCHAR szBEMultiSelForeColor[] = _T("BEMultiSelForeColor");
const TCHAR szBEMultiSelBkgColor[] = _T("BEMultiSelBkgColor");
const TCHAR szBELockedSeparatorColor[] = _T("BELockedSeparatorColor");
const TCHAR szBEResizeColVerticalLineColor[] = _T("BEResizeColVerticalLineColor");
const TCHAR szBEDisabledTitleForeColor[] = _T("BEDisabledTitleForeColor");
const TCHAR szADMAnimateBkgColor[] = _T("ADMAnimateBkgColor");
const TCHAR szFieldInspectorHighlightForeColor[] = _T("FieldInspectorHighlightForeColor");
const TCHAR szSlaveViewContainerTooltipBkgColor[] = _T("SlaveViewContainerTooltipBkgColor");
const TCHAR szProgressBarColor[] = _T("ProgressBarColor");
const TCHAR szSlaveViewContainerTooltipForeColor[] = _T("SlaveViewContainerTooltipForeColor");
const TCHAR szUseFlatStyle[] = _T("UseFlatStyle");
const TCHAR szAddMoreColor[] = _T("AddMoreColor");
const TCHAR szAutoHideBarAccentColor[] = _T("AutoHideBarAccentColor");
const TCHAR szControlsUseBorders[] = _T("ControlsUseBorders");
const TCHAR szCacheImages[] = _T("CacheImages");
const TCHAR szControlsHighlightForeColor[] = _T("ControlsHighlightForeColor");
const TCHAR szControlsHighlightBkgColor[] = _T("ControlsHighlightBkgColor");
const TCHAR szButtonFaceBkgColor[] = _T("ButtonFaceBkgColor");
const TCHAR szButtonFaceForeColor[] = _T("ButtonFaceForeColor");
const TCHAR szColoredControlLineColor[] = _T("ColoredControlLineColor");
const TCHAR szBEToolbarBtnShadowColor[] = _T("BEToolbarBtnShadowColor");
const TCHAR szBEToolbarBtnHighlightColor[] = _T("BEToolbarBtnHighlightColor");
const TCHAR szBETitlesBorderColor[] = _T("BETitlesBorderColor");
const TCHAR szBETitlesBkgColor[] = _T("BETitlesBkgColor");
const TCHAR szButtonFaceHighLightColor[] = _T("ButtonFaceHighLightColor");
const TCHAR szParsedButtonHoveringColor[] = _T("ParsedButtonHoveringColor");
const TCHAR szParsedButtonCheckedForeColor[] = _T("ParsedButtonCheckedForeColor");
const TCHAR szTabSelectorHoveringForeColor[] = _T("TabSelectorHoveringForeColor");
const TCHAR szTabSelectorSelectedBkgColor[] = _T("TabSelectorSelectedBkgColor");
const TCHAR szTabSelectorBkgColor[] = _T("TabSelectorBkgColor");
const TCHAR szTabSelectorMinWidth[] = _T("TabSelectorMinWidth");
const TCHAR szTabSelectorMaxWidth[] = _T("TabSelectorMaxWidth");
const TCHAR szTabSelectorMinHeight[] = _T("TabSelectorMinHeight");

const TCHAR szBCMenuShadowColor[] = _T("BCMenuShadowColor");
const TCHAR szBCMenu3DFaceColor[] = _T("BCMenu3DFaceColor");
const TCHAR szToolbarBkgColor[] = _T("ToolbarBkgColor");
const TCHAR szDialogToolbarBkgColor[] = _T("DialogToolbarBkgColor");
const TCHAR szDialogToolbarForeColor[] = _T("DialogToolbarForeColor");
const TCHAR szDialogToolbarTextColor[] = _T("DialogToolbarTextColor");
const TCHAR szDialogToolbarTextColorHighlighted[] = _T("DialogToolbarTextColorHighlighted");
const TCHAR szDialogToolbarHighlightedColor[] = _T("DialogToolbarHighlightedColor");
const TCHAR szToolbarBkgSecondaryColor[] = _T("ToolbarBkgSecondaryColor");
const TCHAR szToolbarTextColor[] = _T("ToolbarTextColor");
const TCHAR szToolbarTextColorHighlighted[] = _T("ToolbarTextColorHighlighted");
const TCHAR szToolbarHighlightedColor[] = _T("ToolbarHighlightedColor");

const TCHAR szToolbarHighlightedColorClick[] = _T("ToolbarHighlightedColorClick");
const TCHAR szToolbarHighlightedColorClickEnable[] = _T("ToolbarHighlightedColorClickEnable");
const TCHAR szToolbarInfinity[] = _T("ToolbarInfinity");
const TCHAR szToolbarButtonCheckedColor[] = _T("ToolbarButtonCheckedColor");
const TCHAR szToolbarButtonSetDefaultColor[] = _T("ToolbarButtonSetDefaultColor");
const TCHAR szToolbarButtonArrowFillColor[] = _T("ToolbarButtonArrowFillColor");
const TCHAR szToolbarButtonArrowColor[] = _T("ToolbarButtonArrowColor");
const TCHAR szToolbarLineDown[] = _T("ToolbarLineDownColor");
const TCHAR szToolbarInfinityBckButton[] = _T("ToolbarInfinityBckButtonColor");
const TCHAR szToolbarInfinitySepColor[] = _T("ToolbarInfinitySepColor");

static const TCHAR*	szUseBCGTheme = _T("UseBCGTheme");

const TCHAR szToolbarForeColor[] = _T("ToolbarForeColor");
const TCHAR szStatusbarBkgColor[] = _T("StatusbarBkgColor");
const TCHAR szStatusbarTextColor[] = _T("StatusbarTextColor");
const TCHAR szStatusbarForeColor[] = _T("StatusbarForeColor");
const TCHAR szCaptionBarBkgColor[] = _T("CaptionBarBkgColor");
const TCHAR szCaptionBarForeColor[] = _T("CaptionBarForeColor");
const TCHAR szCaptionBarBorderColor[] = _T("CaptionBarBorderColor");
const TCHAR szEasyReading[] = _T("EasyReading");
const TCHAR szShowRadarFixed[] = _T("ShowRadarFixed");
const TCHAR szShowRadarEdit[] = _T("ShowRadarEdit");
const TCHAR szHideRadarSeparatorHorizontal[] = _T("HideRadarSeparatorHorizontal");
const TCHAR szHideRadarSeparatorVertical[] = _T("HideRadarSeparatorVertical");
const TCHAR szUppercaseGridTitles[] = _T("UppercaseGridTitles");
const TCHAR szEnableToolBarDualColor[] = _T("EnableToolBarDualColor");
const TCHAR szToolBarTab[] = _T("ShowToolBarTab");
const TCHAR szAutoHideToolBarButton[] = _T("AutoHideToolBarButton");
const TCHAR szButtonsOverlap[] = _T("ButtonsOverlap");
const TCHAR szToolBarEditingButtonsFix[] = _T("ToolBarEditingButtonsFix");
const TCHAR szHasStatusBar[] = _T("HasStatusBar");
const TCHAR szAlwaysDropDown[] = _T("AlwaysDropDown");


const TCHAR szShowThumbnails[] = _T("ShowThumbnails");

const TCHAR szActivateDockPaneOnMouseClick[] = _T("ActivateDockPaneOnMouseClick");
const TCHAR szBodyEditScrollBarInToolBar[] = _T("BodyEditScrollBarInToolBar");
const TCHAR szTabberTabStyle[] = _T("TabberTabStyle");
const TCHAR szTabberTabHeight[] = _T("TabberTabHeight");
const TCHAR szTabberTabBkgColor[] = _T("TabberTabBkgColor");
const TCHAR szTabberTabForeColor[] = _T("TabberTabForeColor");
const TCHAR szTabberTabSelectedBkgColor[] = _T("TabberTabSelectedBkgColor");
const TCHAR szTabberTabSelectedForeColor[] = _T("TabberTabSelectedForeColor");
const TCHAR szTabberTabHoveringBkgColor[] = _T("TabberTabHoveringBkgColor");
const TCHAR szTabberTabHoveringForeColor[] = _T("TabberTabHoveringForeColor");
const TCHAR szScrollBarFillBkg[] = _T("ScrollBarFillBkg");
const TCHAR szScrollBarThumbNoPressedColor[] = _T("ScrollBarThumbNoPressedColor");
const TCHAR szScrollBarThumbDisableColor[] = _T("ScrollBarThumbDisableColor");
const TCHAR szScrollBarThumbPressedColor[] = _T("ScrollBarThumbPressedColor");
const TCHAR szScrollBarBkgButtonNoPressedColor[] = _T("ScrollBarBkgButtonNoPressedColor");
const TCHAR szScrollBarBkgButtonPressedColor[] = _T("ScrollBarBkgButtonPressedColor");

const TCHAR szTabSelectorButtonFont[] = _T("TabSelectorButtonFont");
const TCHAR szToolBarTitleFont[] = _T("ToolBarTitleFont");
const TCHAR szTabberTabFont[] = _T("TabberTabFont");

const TCHAR szDefaultApplicationBCGTheme[] = _T("DefaultApplicationBCGTheme");

const TCHAR szTileDialogTitleBkgColor[] = _T("TileDialogTitleBkgColor");
const TCHAR szTileDialogTitleForeColor[] = _T("TileDialogTitleForeColor");
const TCHAR szTileDialogStaticAreaBkgColor[] = _T("TileDialogStaticAreaBkgColor");
const TCHAR szTileGroupBkgColor[] = _T("TileGroupBkgColor");

const TCHAR szDockPaneBkgColor[]		= _T("DockPaneBkgColor");
const TCHAR szDockPaneTitleBkgColor[]	= _T("DockPaneTitleBkgColor");
const TCHAR szDockPaneTitleForeColor[]	= _T("DockPaneTitleForeColor");
const TCHAR szDockPaneTitleHoveringForeColor[] = _T("DockPaneTitleHoveringForeColor");
const TCHAR szDockPaneTabberTabBkgColor[] = _T("DockPaneTabberTabBkgColor");
const TCHAR szDockPaneTabberTabForeColor[] = _T("DockPaneTabberTabForeColor");
const TCHAR szDockPaneTabberTabSelectedBkgColor[] = _T("DockPaneTabberTabSelectedBkgColor");
const TCHAR szDockPaneTabberTabSelectedForeColor[] = _T("DockPaneTabberTabSelectedForeColor");
const TCHAR szDockPaneTabberTabHoveringBkgColor[] = _T("DockPaneTabberTabHoveringBkgColor");
const TCHAR szDockPaneTabberTabHoveringForeColor[] = _T("DockPaneTabberTabHoveringForeColor");

const TCHAR szDefaultMasterFrameWidth[] = _T("DefaultMasterFrameWidth");
const TCHAR szDefaultMasterFrameHeight[]= _T("DefaultMasterFrameHeight");
const TCHAR szDefaultSlaveFrameWidth[]	= _T("DefaultSlaveFrameWidth");
const TCHAR szDefaultSlaveFrameHeight[] = _T("DefaultSlaveFrameHeight");
const TCHAR szStatusBarHeight[] = _T("StatusBarHeight");
const TCHAR szScrollBarThumbSize[] = _T("ScrollBarThumbSize");
const TCHAR szToolbarHighlightedHeight[] = _T("ToolbarHighlightedHeight");
const TCHAR szMenuImageMargin[] =  _T("MenuImageMargin");
const TCHAR szToolbarheight[] = _T("Toolbarheight");
const TCHAR szToolbarLineDownHeight[] = _T("ToolbarLineDownHeight");
const TCHAR szToolbarInfinityBckButtonHeight[] = _T("ToolbarInfinityBckButtonHeight");

const TCHAR szCanCloseDockPanes[]		= _T("CanCloseDockPanes");
const TCHAR szDockPaneAutoHideBarSize[] = _T("DockPaneAutoHideBarSize");

const TCHAR szWizardStepperBkgColor[]	= _T("WizardStepperBkgColor");
const TCHAR szWizardStepperForeColor[]	= _T("WizardStepperForeColor");

const TCHAR szTileSpacing[] = _T("TileSpacing");
const TCHAR szTileStaticMinWidthUnit[] = _T("TileStaticMinWidthUnit");
const TCHAR szTileStaticMaxWidthUnit[] = _T("TileStaticMaxWidthUnit");
const TCHAR szTileMaxHeightUnit[] = _T("TileMaxHeightUnit");
const TCHAR szTilePreferredHeightUnit[] = _T("TilePreferredHeightUnit");
const TCHAR szTileProportionalFactor[] = _T("TileProportionalFactor");
const TCHAR szTileTitleHeight[] = _T("TileTitleHeight");
const TCHAR szTileTitleAlignment[] = _T("TileTitleAlignment");
const TCHAR szTileTitleTopSeparatorWidth[] = _T("TileTitleTopSeparatorWidth");
const TCHAR szTileTitleBottomSeparatorWidth[] = _T("TileTitleBottomSeparatorWidth");
const TCHAR szTileSelectorButtonStyle[] = _T("TileSelectorButtonStyle");

const TCHAR szTileAnchorSize[] = _T("TileAnchorSize");
const TCHAR szTileRightPadding[] = _T("TileRightPadding");
const TCHAR szTileInnerLeftPadding[] = _T("TileInnerLeftPadding");
const TCHAR szTileStaticAreaInnerLeftPadding[] = _T("TileStaticAreaInnerLeftPadding");
const TCHAR szTileStaticAreaInnerRightPadding[] = _T("TileStaticAreaInnerRightPadding");

const TCHAR szTileDialogExpandImage[] = _T("TileExpandImage");
const TCHAR szTileDialogCollapseImage[] = _T("TileCollapseImage");
const TCHAR szWizardStepperHeight[] = _T("WizardStepperHeight");
const TCHAR szThemeElementXpathQuery[] = _T("//ThemeElement[@name='{0-%s}']");
const TCHAR szThemeCategoryImagesXpathQuery[] = _T("//Category[@name='Images']");
const TCHAR szThemeCategoryTileStylesXpathQuery[] = _T("//Category[@type='TileStyle']");

const TCHAR szXmlTypeConst[] = _T("type");
const TCHAR szXmlNameConst[] = _T("name");
const TCHAR szXmlValueConst[] = _T("value");
const TCHAR szXmlRgbLongConst[] = _T("rgbLong");
const TCHAR szXmlRgbHexConst[] = _T("rgbHex");

const TCHAR szXmlStringConst[] = _T("string");
const TCHAR szXmlBoolConst[] = _T("bool");
const TCHAR szXmlColorConst[] = _T("color");
const TCHAR szXmlIntegerConst[] = _T("integer");
const TCHAR szXmlFontConst[] = _T("font");

const TCHAR szXmlAttributeFontName[] = _T("fontName");
const TCHAR szXmlAttributeCharset[] = _T("fontCharset");
const TCHAR szXmlAttributeFontSize[] = _T("fontSize");
const TCHAR szXmlAttributeFontSmallSize[] = _T("fontSmallSize");
const TCHAR szXmlAttributeIsUnderline[] = _T("isUnderline");
const TCHAR szXmlAttributeIsStriked[] = _T("isStriked");
const TCHAR szXmlAttributeIsItalic[] = _T("isItalic");
const TCHAR szXmlAttributeFontWeight[] = _T("fontWeight");

const TCHAR szPropertyGridGroupBkgColor[] = _T("PropertyGridGroupBkgColor");
const TCHAR szPropertyGridGroupForeColor[] = _T("PropertyGridGroupForeColor");
const TCHAR szPropertyGridSubGroupsBkgColor[] = _T("PropertyGridSubGroupsBkgColor");
const TCHAR szPropertyGridSubGroupsForeColor[] = _T("PropertyGridSubGroupsForeColor");
const TCHAR szPropertyGridGroupExpandImage[] = _T("PropertyGridGroupExpandImage");
const TCHAR szPropertyGridGroupCollapseImage[] = _T("PropertyGridGroupCollapseImage");


const TCHAR szLinearGaugeBkgColor[] = _T("LinearGaugeBkgColor");
const TCHAR szLinearGaugeBkgGradientColor[] = _T("LinearGaugeBkgGradientColor");
const TCHAR szLinearGaugeFrameOutLineColor[] = _T("LinearGaugeFrameOutLineColor");
const TCHAR szLinearGaugePointerBkgColor[] = _T("LinearGaugePointerBkgColor");
const TCHAR szLinearGaugePointerOutLineColor[] = _T("LinearGaugePointerOutLineColor");
const TCHAR szLinearGaugeForeColor[] = _T("LinearGaugeForeColor");
const TCHAR szLinearGaugeTickMarkBkgColor[] = _T("LinearGaugeTickMarkBkgColor");
const TCHAR szLinearGaugeTickMarkOutLineColor[] = _T("LinearGaugeTickMarkOutLineColor");
const TCHAR szLinearGaugeLeftBorderColor[] = _T("LinearGaugeLeftBorderColor");
const TCHAR szLinearGaugeMinorMarkSize[] = _T("LinearGaugeMinorMarkSize");
const TCHAR szLinearGaugeMajorMarkSize[] = _T("LinearGaugeMajorMarkSize");
const TCHAR szLinearGaugeMajorStepMarkSize[] = _T("LinearGaugeMajorStepMarkSize");
const TCHAR szLinearGaugeMinorStepMarkSize[] = _T("LinearGaugeMinorStepMarkSize");
const TCHAR szLinearGaugeTextLabelFormat[] = _T("LinearGaugeTextLabelFormat");

const TCHAR szAlternativeFontFaceName[] = _T("AlternativeFontFaceName");

//-----------------------------------------------------------------------------
void TBThemeManager::SetTileImageSetting(CString& strImageElement, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (!pNode)
		return;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlStringConst) != 0)
	{
		ASSERT(FALSE);
		SAFE_DELETE(pNode);
		return;
	}

	CString val;
	pNode->GetAttribute(szXmlValueConst, val);
	strImageElement = val;
	SAFE_DELETE(pNode);
}

//-----------------------------------------------------------------------------------------------------------
void TBThemeManager::SetTextAndFormatsSetting(CString& strFormatElement, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (!pNode)
		return;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlStringConst) != 0)
	{
		ASSERT(FALSE);
		SAFE_DELETE(pNode);
		return;
	}

	CString val;
	pNode->GetAttribute(szXmlValueConst, val);
	strFormatElement = val;
	SAFE_DELETE(pNode);
}

//-----------------------------------------------------------------------------
void TBThemeManager::SetColorSetting(TBThemeColor& tbThemeColor, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (pNode)
	{
		COLORREF color = GetColorSetting(pNode, settingName);
		if (color != -1)
			tbThemeColor.SetColor(color);
	}
		
	SAFE_DELETE(pNode);
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetColorSetting(CXMLNode* pNode, CString settingName)
{
	CString aName;
	pNode->GetAttribute(szXmlNameConst, aName);

	if (aName.IsEmpty() || aName.CompareNoCase(settingName) != 0)
		return -1;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	// not found
	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlColorConst) != 0)
		return -1;

	long colorRef = -1;

	CString valLong;
	pNode->GetAttribute(szXmlRgbLongConst, valLong);
	if (!valLong.IsEmpty())
	{
		colorRef = _ttol(valLong);
		if (colorRef >= 0)
			return colorRef;

	}

	CString valRgbHex;
	pNode->GetAttribute(szXmlRgbHexConst, valRgbHex);
	if (!valRgbHex.IsEmpty())
		return ConvertHexColorToColorRef(valRgbHex);
	
	return -1;
}

//-----------------------------------------------------------------------------
void TBThemeManager::SetBoolSetting(BOOL& aValue, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (!pNode)
		return;

	int nValue = GetBoolSetting(pNode, settingName);
	if (nValue != -1)
		aValue = nValue;

	SAFE_DELETE(pNode);
}
//-----------------------------------------------------------------------------
int TBThemeManager::GetBoolSetting(CXMLNode* pNode, CString settingName)
{
	CString aName;
	pNode->GetAttribute(szXmlNameConst, aName);

	if (aName.IsEmpty() || aName.CompareNoCase(settingName) != 0)
		return -1;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	// not found
	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlBoolConst) != 0)
		return -1;

	CString val;
	pNode->GetAttribute(szXmlValueConst, val);
	BOOL aValue = ConvertToBool(val);
	return aValue;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetIntSetting(CXMLNode* pNode, CString settingName)
{
	CString aName;
	pNode->GetAttribute(szXmlNameConst, aName);

	if (aName.IsEmpty() || aName.CompareNoCase(settingName) != 0)
		return -1;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlIntegerConst) != 0)
	{
		ASSERT(FALSE);
		return -1;
	}

	CString val;
	pNode->GetAttribute(szXmlValueConst, val);

	return _ttoi(val);
}

//-----------------------------------------------------------------------------
void TBThemeManager::SetIntSetting(int& aValue, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (!pNode)
		return;

	aValue = GetIntSetting(pNode, settingName);

	SAFE_DELETE(pNode);
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetFontSetting(CXMLNode* pNode, TBThemeFont& font, CString settingName)
{
	CString aName;
	pNode->GetAttribute(szXmlNameConst, aName);

	if (aName.IsEmpty() || aName.CompareNoCase(settingName) != 0)
		return FALSE;

	CString nodeType;
	pNode->GetAttribute(szXmlTypeConst, nodeType);

	if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlFontConst) != 0)
		return FALSE;

	//<ThemeElement name="HyperlinkFont" type="font" fontName="Segoe UI" fontCharset="DEFAULT" fontSize="9" fontSmallSize="3" fontWeight="700" isUnderline="1" isStriked="0" isItalic="0" />

	CString strFontName(_T("Segoe UI"));
	CString strFontCharset(_T("DEFAULT"));
	CString sFFName, sFFChSet;
	int	nSizeDefault(9);
	int	nSmallSizeDefault(3);

	int nWeight = FW_NORMAL;
	BOOL bIsItalic = FALSE;
	BOOL bIsStriked = FALSE;
	BOOL bIsUnderline = FALSE;

	CString strFontSize, strFontSmallSize, strFontWeight, strIsUnderline, strIsStriked, strIsItalic;

	pNode->GetAttribute(szXmlAttributeFontName, strFontName);
	pNode->GetAttribute(szXmlAttributeCharset, strFontCharset);

	pNode->GetAttribute(szXmlAttributeFontSize, strFontSize);
	if (!strFontSize.IsEmpty())
		nSizeDefault = ConvertToFontSize(strFontSize);

	pNode->GetAttribute(szXmlAttributeFontSmallSize, strFontSmallSize);
	if (!strFontSmallSize.IsEmpty())
		nSmallSizeDefault = ConvertToFontSize(strFontSmallSize);

	pNode->GetAttribute(szXmlAttributeIsUnderline, strIsUnderline);
	if (!strIsUnderline.IsEmpty())
		bIsUnderline = ConvertToBool(strIsUnderline);

	pNode->GetAttribute(szXmlAttributeIsStriked, strIsStriked);
	if (!strIsStriked.IsEmpty())
		bIsStriked = ConvertToBool(strIsStriked);

	pNode->GetAttribute(szXmlAttributeIsItalic, strIsItalic);
	if (!strIsItalic.IsEmpty())
		bIsItalic = ConvertToBool(strIsItalic);

	pNode->GetAttribute(szXmlAttributeFontWeight, strFontWeight);
	if (!strFontWeight.IsEmpty())
		nWeight = _ttoi(strFontWeight);
	if (!IsFontInstalled(strFontName))
		strFontName = "Segoe UI";

	font.SetFont(strFontName, nSizeDefault, strFontCharset, nSmallSizeDefault, bIsUnderline, bIsItalic, bIsStriked, nWeight);
	return TRUE;
}

//-----------------------------------------------------------------------------
void TBThemeManager::SetFontSetting(TBThemeFont& font, CString settingName)
{
	CXMLNode* pNode = m_XmlSettingsDoc.SelectSingleNode(cwsprintf(szThemeElementXpathQuery, settingName));
	if (pNode)
	{
		GetFontSetting(pNode, font, settingName);
		SAFE_DELETE(pNode);
	}
	else
	{
		// di default mette il font di form
		if (GetFormFont())
		{
			LOGFONT lf;
			GetFormFont()->GetLogFont(&lf);
			font.CreateFont(lf);
		}
		else
		{
			font.SetFont(_T("Segoe UI"), 9, _T("DEFAULT"), 3, FALSE, FALSE, FALSE, FW_NORMAL);
		}
	}
}

//-----------------------------------------------------------------------------
void TBThemeManager::LoadSettings()
{
	Clear();
	
	CStringArray fonts;
	AfxGetPathFinder()->GetAvailableThemeFonts(fonts);

	for (int i = 0; i <= fonts.GetUpperBound(); i++)
	{
		CString font = fonts[i];
	    int a = AddFontResourceEx(font, FR_PRIVATE, 0);
		int b = 0;
	}
	
	DataBool bEnabled(FALSE);
	DataObj* pSetting = NULL;

	// flags
	SetBoolSetting(m_bFocusedControlBkgColorEnabled, szFocusedControlBkgColorEnabled);
	SetBoolSetting(m_ControlsUseBorders, szControlsUseBorders);
	SetBoolSetting(m_CacheImages, szCacheImages);
	m_GlobalCacheImages.SetCacheImages(m_CacheImages);

	SetBoolSetting(m_bUseFlatStyle, szUseFlatStyle);
	SetBoolSetting(m_bAddMoreColor, szAddMoreColor);
	SetBoolSetting(m_bUseEasyReading, szEasyReading);

	SetBoolSetting(m_bShowRadarFixed, szShowRadarFixed);
	SetBoolSetting(m_bShowRadarEdit, szShowRadarEdit);
	SetBoolSetting(m_bHideRadarSeparatorHorizontal, szHideRadarSeparatorHorizontal);
	SetBoolSetting(m_bHideRadarSeparatorVertical, szHideRadarSeparatorVertical);
	SetBoolSetting(m_bEnableToolBarDualColor, szEnableToolBarDualColor);
	SetBoolSetting(m_bShowThumbnails, szShowThumbnails);  // è uno di quelli per il menu html, ma mi serve anche in altre circostanze
	SetBoolSetting(m_bUppercaseGridTitles, szUppercaseGridTitles);
	SetBoolSetting(m_bShowToolBarTab, szToolBarTab);
	SetBoolSetting(m_bAutoHideToolBarButton, szAutoHideToolBarButton);
	SetBoolSetting(m_bButtonsOverlap, szButtonsOverlap);
	
	SetBoolSetting(m_bToolBarEditingButtonsFix, szToolBarEditingButtonsFix);
	SetBoolSetting(m_bHasStatusBar, szHasStatusBar);
	SetBoolSetting(m_AlwaysDropDown, szAlwaysDropDown);
	
	SetBoolSetting(m_bActivateDockPaneOnMouseClick, szActivateDockPaneOnMouseClick); // docking panes show on mouse click instead of hovering
	SetBoolSetting(m_bBodyEditScrollBarInToolBar, szBodyEditScrollBarInToolBar);
	
	SetBoolSetting(m_bToolbarHighlightedColorClickEnable, szToolbarHighlightedColorClickEnable);
	SetBoolSetting(m_bToolbarInfinity, szToolbarInfinity);

	// colors
	SetColorSetting(m_AutoHideBarAccentColor, szAutoHideBarAccentColor);
	SetColorSetting(m_BkgColor, szBkgColor);
	SetColorSetting(m_DocumentBkgColor, szDocumentBkgColor);
	SetColorSetting(m_FocusedControlBkgColor, szFocusedControlBkgColor);
	SetColorSetting(m_AlternateColor, szAlternateColor);
	SetColorSetting(m_EnabledControlBkgColor, szEnabledControlBkgColor);
	SetColorSetting(m_EnabledControlForeColor, szEnabledControlForeColor);
	SetColorSetting(m_DisabledControlForeColor, szDisabledControlForeColor);
	SetColorSetting(m_DisabledInStaticAreaControlForeColor, szDisabledInStaticAreaControlForeColor);
	SetColorSetting(m_EnabledControlBorderForeColor, szEnabledControlBorderForeColor);
	SetColorSetting(m_StaticControlForeColor, szStaticControlForeColor);
	SetColorSetting(m_TooltipBkgColor, szTooltipBkgColor);
	SetColorSetting(m_TooltipForeColor, szTooltipForeColor);
	SetColorSetting(m_RadarVerticalLineColor, szRadarVerticalLineColor);
	SetColorSetting(m_RadarTitleBarButtonBkgColor, szRadarTitleBarButtonBkgColor);
	SetColorSetting(m_RadarTitleBarSelectedButtonBkgColor, szRadarTitleBarSelectedButtonBkgColor);
	SetColorSetting(m_RadarTitleBarButtonBorderBkgColor, szRadarTitleBarButtonBorderBkgColor);
	SetColorSetting(m_RadarTitleBarLineColor, szRadarTitleBarLineColor);
	SetColorSetting(m_RadarSeparatorColor, szRadarSeparatorColor);
	SetColorSetting(m_RadarColumnBorderColor, szRadarColumnBorderColor);
	SetColorSetting(m_RadarColumnTitleBorderColor, szRadarColumnTitleBorderColor);
	SetColorSetting(m_RadarPageBkgColor, szRadarPageBkgColor);
	SetColorSetting(m_PerformanceAnalyzerBkgColor, szPerformanceAnalyzerBkgColor);
	SetColorSetting(m_PerformanceAnalyzerForeColor, szPerformanceAnalyzerForeColor);
	SetColorSetting(m_TreeViewBkgColor, szTreeViewBkgColor);
	SetColorSetting(m_TreeViewNodeForeColor, szTreeViewNodeForeColor);
	SetColorSetting(m_BCMenuBitmapBkgColor, szBCMenuBitmapBkgColor);
	SetColorSetting(m_BCMenuBkgColor, szBCMenuBkgColor);
	SetColorSetting(m_BCMenuForeColor, szBCMenuForeColor);
	SetColorSetting(m_BETransparentBmpColor, szBETransparentBmpColor);
	SetColorSetting(m_BEToolbarBtnBitmapBkgColor, szBEToolbarBtnBitmapBkgColor);
	SetColorSetting(m_BEToolbarBtnBkgColor, szBEToolbarBtnBkgColor);
	SetColorSetting(m_BEToolbarBtnForeColor, szBEToolbarBtnForeColor);
	SetColorSetting(m_ToolbarSeparatorColor, szToolbarSeparatorColor);
	SetColorSetting(m_CISBitmapBkgColor, szCISBitmapBkgColor);
	SetColorSetting(m_CISBitmapForeColor, szCISBitmapForeColor);
	SetColorSetting(m_TransBmpTransparentDefaultColor, szTransBmpTransparentDefaultColor);
	SetColorSetting(m_TBExtDropBitmapsTransparentColor, szTBExtDropBitmapsTransparentColor);
	SetColorSetting(m_HyperLinkForeColor, szHyperLinkForeColor);
	SetColorSetting(m_HyperLinkBrowsedForeColor, szHyperLinkBrowsedForeColor);
	SetColorSetting(m_DropDownEnabledGrdTopColor, szDropDownEnabledGrdTopColor);
	SetColorSetting(m_DropDownEnabledGrdBottomColor, szDropDownEnabledGrdBottomColor);
	SetColorSetting(m_EditEditableOutBorderBkgColor, szEditEditableOutBorderBkgColor);
	SetColorSetting(m_EditNonEditableOutBorderBkgColor, szEditNonEditableOutBorderBkgColor);
	SetColorSetting(m_EditInBorderBkgColor, szEditInBorderBkgColor);
	SetColorSetting(m_StaticWithLineLineForeColor, szStaticWithLineLineForeColor);
	SetColorSetting(m_StaticControlOutBorderBkgColor, szStaticControlOutBorderBkgColor);
	SetColorSetting(m_StaticControlInBorderBkgColor, szStaticControlInBorderBkgColor);
	SetColorSetting(m_ColoredControlBorderColor, szColoredControlBorderColor);
	SetColorSetting(m_StretchCtrlTransparentColor, szStretchCtrlTransparentColor);
	SetColorSetting(m_StretchCtrlForeColor, szStretchCtrlForeColor);
	SetColorSetting(m_BERowSelectedFillColor, szBERowSelectedFillColor);
	SetColorSetting(m_BETooltipBkgColor, szBETooltipBkgColor);
	SetColorSetting(m_BETooltipForeColor, szBETooltipForeColor);
	SetColorSetting(m_BERowSelectedBkgColor, szBERowSelectedBkgColor);
	SetColorSetting(m_BERowSelectedForeColor, szBERowSelectedForeColor);
	SetColorSetting(m_BERowBkgAlternateColor, szBERowBkgAlternateColor);
	SetColorSetting(m_BESeparatorColor, szBESeparatorColor);
	SetColorSetting(m_BEMultiSelForeColor, szBEMultiSelForeColor);
	SetColorSetting(m_BEMultiSelBkgColor, szBEMultiSelBkgColor);
	SetColorSetting(m_BELockedSeparatorColor, szBELockedSeparatorColor);
	SetColorSetting(m_BEResizeColVerticalLineColor, szBEResizeColVerticalLineColor);
	SetColorSetting(m_BEDisabledTitleForeColor, szBEDisabledTitleForeColor);
	SetColorSetting(m_ADMAnimateBkgColor, szADMAnimateBkgColor);
	SetColorSetting(m_FieldInspectorHighlightForeColor, szFieldInspectorHighlightForeColor);
	SetColorSetting(m_SlaveViewContainerTooltipBkgColor, szSlaveViewContainerTooltipBkgColor);
	SetColorSetting(m_ProgressBarColor, szProgressBarColor);
	SetColorSetting(m_SlaveViewContainerTooltipForeColor, szSlaveViewContainerTooltipForeColor);
	SetColorSetting(m_ControlsHighlightForeColor, szControlsHighlightForeColor);
	SetColorSetting(m_ControlsHighlightBkgColor, szControlsHighlightBkgColor);
	SetColorSetting(m_ButtonFaceBkgColor, szButtonFaceBkgColor);
	SetColorSetting(m_ButtonFaceForeColor, szButtonFaceForeColor);
	SetColorSetting(m_ColoredControlLineColor, szColoredControlLineColor);
	SetColorSetting(m_BEToolbarBtnShadowColor, szBEToolbarBtnShadowColor);
	SetColorSetting(m_BEToolbarBtnHighlightColor, szBEToolbarBtnHighlightColor);
	SetColorSetting(m_BETitlesBorderColor, szBETitlesBorderColor);
	SetColorSetting(m_BETitlesBkgColor, szBETitlesBkgColor);
	SetColorSetting(m_ButtonFaceHighLightColor, szButtonFaceHighLightColor);
	SetColorSetting(m_ParsedButtonHoveringColor, szParsedButtonHoveringColor);
	SetColorSetting(m_ParsedButtonCheckedForeColor, szParsedButtonCheckedForeColor);
	SetColorSetting(m_TabSelectorHoveringForeColor, szTabSelectorHoveringForeColor);
	SetColorSetting(m_TabSelectorSelectedBkgColor, szTabSelectorSelectedBkgColor);
	SetColorSetting(m_TabSelectorBkgColor, szTabSelectorBkgColor);
	SetColorSetting(m_BCMenuShadowColor, szBCMenuShadowColor);
	SetColorSetting(m_BCMenu3DFaceColor, szBCMenu3DFaceColor);
	SetColorSetting(m_ToolbarBkgColor, szToolbarBkgColor);
	SetColorSetting(m_ToolbarBkgSecondaryColor, szToolbarBkgSecondaryColor);
	
	SetColorSetting(m_ToolbarTextColor, szToolbarTextColor);
	SetColorSetting(m_ToolbarTextColorHighlighted, szToolbarTextColorHighlighted);
	SetColorSetting(m_ToolbarHighlightedColor, szToolbarHighlightedColor);

	SetColorSetting(m_ToolbarHighlightedColorClick, szToolbarHighlightedColorClick);
	
	SetColorSetting(m_ToolbarButtonCheckedColor, szToolbarButtonCheckedColor);
	SetColorSetting(m_ToolbarButtonSetDefaultColor, szToolbarButtonSetDefaultColor);
	SetColorSetting(m_ToolbarButtonArrowFillColor, szToolbarButtonArrowFillColor);
	SetColorSetting(m_ToolbarButtonArrowColor, szToolbarButtonArrowColor);

	SetColorSetting(m_ToolbarLineDownColor, szToolbarLineDown);
	SetColorSetting(m_ToolbarInfinityBckButtonColor, szToolbarInfinityBckButton);
	SetColorSetting(m_ToolbarInfinitySepColor, szToolbarInfinitySepColor);

	SetColorSetting(m_DialogToolbarBkgColor, szDialogToolbarBkgColor);
	SetColorSetting(m_DialogToolbarForeColor, szDialogToolbarForeColor);
	SetColorSetting(m_DialogToolbarTextColor, szDialogToolbarTextColor);
	SetColorSetting(m_DialogToolbarTextColorHighlighted, szDialogToolbarTextColorHighlighted);
	SetColorSetting(m_DialogToolbarHighlightedColor, szDialogToolbarHighlightedColor);

	SetColorSetting(m_ToolbarForeColor, szToolbarForeColor);
	SetColorSetting(m_StatusbarBkgColor, szStatusbarBkgColor);
	SetColorSetting(m_StatusbarTextColor, szStatusbarTextColor);

	SetColorSetting(m_StatusbarForeColor, szStatusbarForeColor);
	SetColorSetting(m_CaptionBarBkgColor, szCaptionBarBkgColor);
	SetColorSetting(m_CaptionBarForeColor, szCaptionBarForeColor);
	SetColorSetting(m_CaptionBarBorderColor, szCaptionBarBorderColor);
	SetColorSetting(m_TileDialogTitleBkgColor, szTileDialogTitleBkgColor);
	SetColorSetting(m_TileDialogTitleForeColor, szTileDialogTitleForeColor);
	SetColorSetting(m_TileDialogStaticAreaBkgColor, szTileDialogStaticAreaBkgColor);
	SetColorSetting(m_TileGroupBkgColor, szTileGroupBkgColor);
	SetColorSetting(m_TileTitleSeparatorColor, szTileTitleSeparatorColor);
	
	SetColorSetting(m_DockPaneBkgColor, szDockPaneBkgColor);
	SetColorSetting(m_DockPaneTitleBkgColor, szDockPaneTitleBkgColor);
	SetColorSetting(m_DockPaneTitleForeColor, szDockPaneTitleForeColor);
	SetColorSetting(m_DockPaneTitleHoveringForeColor, szDockPaneTitleHoveringForeColor);
	SetColorSetting(m_DockPaneTabberTabBkgColor, szDockPaneTabberTabBkgColor);
	SetColorSetting(m_DockPaneTabberTabForeColor, szDockPaneTabberTabForeColor);
	SetColorSetting(m_DockPaneTabberTabSelectedBkgColor, szDockPaneTabberTabSelectedBkgColor);
	SetColorSetting(m_DockPaneTabberTabSelectedForeColor, szDockPaneTabberTabSelectedForeColor);
	SetColorSetting(m_DockPaneTabberTabHoveringBkgColor, szDockPaneTabberTabHoveringBkgColor);
	SetColorSetting(m_DockPaneTabberTabHoveringForeColor, szDockPaneTabberTabHoveringForeColor);


	SetColorSetting(m_WizardStepperBkgColor, szWizardStepperBkgColor);
	SetColorSetting(m_WizardStepperForeColor, szWizardStepperForeColor);

	//Tile Settings
	SetIntSetting(m_nTileSpacing, szTileSpacing);
	SetIntSetting(m_nTileStaticMinWidthUnit, szTileStaticMinWidthUnit);
	SetIntSetting(m_nTileStaticMaxWidthUnit, szTileStaticMaxWidthUnit);
	SetIntSetting(m_nTileMaxHeightUnit, szTileMaxHeightUnit);
	SetIntSetting(m_nTilePreferredHeightUnit, szTilePreferredHeightUnit);
	SetIntSetting(m_nTileProportionalFactor, szTileProportionalFactor);
	SetIntSetting(m_nTileTitleHeight, szTileTitleHeight);
	SetIntSetting(m_nTileTitleAlignment, szTileTitleAlignment);
	SetIntSetting(m_nTileTitleTopSeparatorWidth, szTileTitleTopSeparatorWidth);
	SetIntSetting(m_nTileTitleBottomSeparatorWidth, szTileTitleBottomSeparatorWidth);
	SetIntSetting(m_nTileSelectorButtonStyle, szTileSelectorButtonStyle);

	SetIntSetting(m_nTileAnchorSize, szTileAnchorSize);
	SetIntSetting(m_nTileRightPadding, szTileRightPadding);
	SetIntSetting(m_nTileInnerLeftPadding, szTileInnerLeftPadding);
	SetIntSetting(m_nTileStaticAreaInnerLeftPadding, szTileStaticAreaInnerLeftPadding);
	SetIntSetting(m_nTileStaticAreaInnerRightPadding, szTileStaticAreaInnerRightPadding);

	SetIntSetting(m_nWizardStepperHeight, szWizardStepperHeight);
	SetIntSetting(m_nDefaultApplicationBCGTheme, szDefaultApplicationBCGTheme);

	SetIntSetting(m_nDefaultMasterFrameWidth, szDefaultMasterFrameWidth);
	SetIntSetting(m_nDefaultMasterFrameHeight, szDefaultMasterFrameHeight);
	SetIntSetting(m_nDefaultSlaveFrameWidth, szDefaultSlaveFrameWidth);
	SetIntSetting(m_nDefaultSlaveFrameHeight, szDefaultSlaveFrameHeight);
	SetIntSetting(m_nStatusBarHeight, szStatusBarHeight);
	SetIntSetting(m_nScrollBarThumbSize, szScrollBarThumbSize);
	SetIntSetting(m_nToolbarHighlightedHeight, szToolbarHighlightedHeight);
	SetIntSetting(m_nMenuImageMargin, szMenuImageMargin);
	SetIntSetting(m_nToolbarheight, szToolbarheight);

	SetIntSetting(m_nToolbarLineDownHeight, szToolbarLineDownHeight);
	SetIntSetting(m_nToolbarInfinityBckButtonHeight, szToolbarInfinityBckButtonHeight);
	SetIntSetting(m_nTabSelectorMinWidth, szTabSelectorMinWidth);
	SetIntSetting(m_nTabSelectorMaxWidth, szTabSelectorMaxWidth);
	SetIntSetting(m_nTabSelectorMinHeight, szTabSelectorMinHeight);	

	SetIntSetting(m_bCanCloseDockPanes, szCanCloseDockPanes);
	SetIntSetting(m_nDockPaneAutoHideBarSize, szDockPaneAutoHideBarSize);

	// tabber
	SetIntSetting(m_nTabberTabStyle, szTabberTabStyle);
	SetIntSetting(m_nTabberTabHeight, szTabberTabHeight);

	SetColorSetting(m_TabberTabBkgColor, szTabberTabBkgColor);
	SetColorSetting(m_TabberTabForeColor, szTabberTabForeColor);
	SetColorSetting(m_TabberTabSelectedBkgColor, szTabberTabSelectedBkgColor);

	SetColorSetting(m_ScrollBarFillBkg, szScrollBarFillBkg);
	SetColorSetting(m_ScrollBarThumbNoPressedColor, szScrollBarThumbNoPressedColor);
	SetColorSetting(m_ScrollBarThumbDisableColor, szScrollBarThumbDisableColor);
	SetColorSetting(m_ScrollBarThumbPressedColor, szScrollBarThumbPressedColor);
	SetColorSetting(m_ScrollBarBkgButtonNoPressedColor, szScrollBarBkgButtonNoPressedColor);
	SetColorSetting(m_ScrollBarBkgButtonPressedColor, szScrollBarBkgButtonPressedColor);

	SetColorSetting(m_TabberTabSelectedForeColor, szTabberTabSelectedForeColor);
	SetColorSetting(m_TabberTabHoveringBkgColor, szTabberTabHoveringBkgColor);
	SetColorSetting(m_TabberTabHoveringForeColor, szTabberTabHoveringForeColor);
	
	SetFontSetting(m_FormFont, szFormFontFace);
	SetFontSetting(m_ControlFont, szControlsFont);
	SetFontSetting(m_TileDialogTitleFont, szTileDialogTitle); //sFFName, nSize, sFFChSet, nSmallSize, FALSE, bIsItalic, FALSE, nWeight);
	SetFontSetting(m_WizardStepperFont, szWizardStepper);
	SetFontSetting(m_TabSelectorButtonFont, szTabSelectorButtonFont);
	SetFontSetting(m_ToolBarTitleFont, szToolBarTitleFont);
	SetFontSetting(m_TabberTabFont, szTabberTabFont);
	SetFontSetting(m_RadarSearchFont, szRadarSearchFont);
	SetFontSetting(m_TileStripFont, szTileStrip);
	SetFontSetting(m_StaticWithLineFont, szStaticWithLineFont);

	SetHyperLinkFont(m_ControlFont);

	SetTextAndFormatsSetting(m_AlternativeFontFaceName, szAlternativeFontFaceName);

	SetTileImageSetting(m_strTileCollapseImage, szTileDialogCollapseImage);
	SetTileImageSetting(m_strTileExpandImage, szTileDialogExpandImage);

	// property grid
	SetColorSetting(m_PropertyGridGroupBkgColor, szPropertyGridGroupBkgColor);
	SetColorSetting(m_PropertyGridGroupForeColor, szPropertyGridGroupForeColor);
	
	m_PropertyGridSubGroupsBkgColor.SetColor((COLORREF)-1);
	SetColorSetting(m_PropertyGridSubGroupsBkgColor, szPropertyGridSubGroupsBkgColor);
	
	m_PropertyGridSubGroupsForeColor.SetColor((COLORREF)-1);
	SetColorSetting(m_PropertyGridSubGroupsForeColor, szPropertyGridSubGroupsForeColor);

	if (m_PropertyGridSubGroupsBkgColor.GetColor() == (COLORREF)-1)
		m_PropertyGridSubGroupsBkgColor.SetColor(m_PropertyGridGroupBkgColor.GetColor());
	if (m_PropertyGridSubGroupsForeColor.GetColor() == (COLORREF)-1)
		m_PropertyGridSubGroupsForeColor.SetColor(m_PropertyGridGroupForeColor.GetColor());

	SetTileImageSetting(m_strPropertyGridGroupExpandImage, szPropertyGridGroupExpandImage);
	SetTileImageSetting(m_strPropertyGridGroupCollapseImage, szPropertyGridGroupCollapseImage);


	// TBLinearGauge
	SetColorSetting(m_LinearGaugeFrameOutLineColor, szLinearGaugeFrameOutLineColor);
	SetColorSetting(m_LinearGaugePointerBkgColor, szLinearGaugePointerBkgColor);
	SetColorSetting(m_LinearGaugePointerOutLineColor, szLinearGaugePointerOutLineColor);
	SetColorSetting(m_LinearGaugeForeColor, szLinearGaugeForeColor);
	SetColorSetting(m_LinearGaugeTickMarkBkgColor, szLinearGaugeTickMarkBkgColor);
	SetColorSetting(m_LinearGaugeTickMarkOutLineColor, szLinearGaugeTickMarkOutLineColor);
	SetColorSetting(m_LinearGaugeLeftBorderColor, szLinearGaugeLeftBorderColor);
	SetColorSetting(m_LinearGaugeBkgColor, szLinearGaugeBkgColor);
	SetColorSetting(m_LinearGaugeBkgGradientColor, szLinearGaugeBkgGradientColor);
	SetIntSetting(m_LinearGaugeMinorMarkSize, szLinearGaugeMinorMarkSize);
	SetIntSetting(m_LinearGaugeMajorMarkSize, szLinearGaugeMajorMarkSize);
	SetIntSetting(m_LinearGaugeMajorStepMarkSize, szLinearGaugeMajorStepMarkSize);
	SetIntSetting(m_LinearGaugeMinorStepMarkSize, szLinearGaugeMinorStepMarkSize);
	SetTextAndFormatsSetting(m_LinearGaugeTextLabelFormat, szLinearGaugeTextLabelFormat);

	// Load castom toolbar images from XML config file
	LoadThemeImages();
	LoadTileStyles();

	if (!GetControlsUseBorders())
	{
		COLORREF bkgColor = GetBackgroundColor();
		m_EditInBorderBkgColor.SetColor(bkgColor);
		m_EditNonEditableOutBorderBkgColor.SetColor(bkgColor);
		m_StaticControlOutBorderBkgColor.SetColor(bkgColor);
		m_StaticControlInBorderBkgColor.SetColor(bkgColor);
		m_ColoredControlBorderColor.SetColor(bkgColor);
	}

	//Override delle impostazioni dal Font.Ini di TB
	FontStyle* fs = AfxGetFontStyleTable()->GetFontStyle(AfxGetFontStyleTable()->GetFontIdx(L"<Hyperlink>"), NULL);
	if (fs)
	{
		fs->SetColor(this->m_HyperLinkForeColor.GetColor());
	}
	
	fs = AfxGetFontStyleTable()->GetFontStyle(AfxGetFontStyleTable()->GetFontIdx(L"<Hyperlink_Browsed>"), NULL);
	if (fs)
	{
		fs->SetColor(this->m_HyperLinkBrowsedForeColor.GetColor());
	}

	fs = AfxGetFontStyleTable()->GetFontStyle(AfxGetFontStyleTable()->GetFontIdx(L"Radar"), NULL);
	if (fs && m_ControlFont.GetFont())
	{
		LOGFONT lf;
		m_ControlFont.GetFont()->GetLogFont(&lf);
		fs->SetLogFont(lf);
	}

	SAFE_DELETE(m_pScalingInfo);
	m_pScalingInfo = new ScalingInfo(m_FormFont);
}

//-----------------------------------------------------------------------------
//Associations BCG - ThemeManager
void TBThemeManager::SetHyperLinkFont(TBThemeFont& font)
{
	LOGFONT hyperLinkFont;
	font.GetFont()->GetLogFont(&hyperLinkFont);
	hyperLinkFont.lfUnderline = TRUE;
	m_HyperlinkFont.CreateFont(hyperLinkFont);
}

//-----------------------------------------------------------------------------
//Associations BCG - ThemeManager
void TBThemeManager::LoadThemeImages()
{
	//<Category name = "Images" description = "Images management">
	//cerco in tutti i sotto nodi images
	CString nodeType;
	CString nodeValue;
	CString nodeName;

	CXMLNodeChildsList* pNodeList = m_XmlSettingsDoc.SelectNodes(szThemeCategoryImagesXpathQuery);
	if (!pNodeList)
		return;

	CXMLNode* pCurrent;
	for (int i = 0; i < pNodeList->GetSize(); i++)
	{
		pCurrent = pNodeList->GetAt(i);
		if (!pCurrent)
			continue;

		pCurrent->GetAttribute(szXmlTypeConst, nodeType);
		pCurrent->GetAttribute(szXmlValueConst, nodeValue);
		pCurrent->GetAttribute(szXmlNameConst, nodeName);

		if (nodeType.IsEmpty() || nodeType.CompareNoCase(szXmlStringConst) != 0)
			continue;

		if (nodeValue.IsEmpty())
			continue;

		if (nodeName.IsEmpty())
			continue;

		m_MapToolBarImages.AddTail(TBThemeToolBarImageList(nodeName, nodeValue));
	}

	SAFE_DELETE(pNodeList);
}

//-----------------------------------------------------------------------------
//Associations BCG - ThemeManager
void TBThemeManager::LoadBCGTheme()
{
	CBCGPVisualManager::GetInstance();

	const BCGPGLOBAL_DATA* BCP = &globalData;

#ifdef _DEBUG
	TRACE("TBThemeManager::LoadBCGThemed dump globalData\n");

	TRACE("clrBtnFace:%d\n", BCP->clrBtnFace);
	TRACE("clrBtnShadow:%d\n", BCP->clrBtnShadow);
	TRACE("clrBtnHilite:%d\n", BCP->clrBtnHilite);

	TRACE("clrBtnText:%d\n", BCP->clrBtnText);
	TRACE("clrWindowFrame:%d\n", BCP->clrWindowFrame);

	TRACE("clrBtnDkShadow:%d\n", BCP->clrBtnDkShadow);
	TRACE("clrBtnLight:%d\n", BCP->clrBtnLight);

	TRACE("clrGrayedText:%d\n", BCP->clrGrayedText);
	TRACE("clrPrompt:%d\n", BCP->clrPrompt);
	TRACE("clrHilite:%d\n", BCP->clrHilite);
	TRACE("clrTextHilite:%d\n", BCP->clrTextHilite);
	TRACE("clrHotText:%d\n", BCP->clrHotText);

	TRACE("clrHotLinkText:%d\n", BCP->clrHotLinkText);
	TRACE("clrBarWindow:%d\n", BCP->clrBarWindow);
	TRACE("clrBarFace:%d\n", BCP->clrBarFace);
	TRACE("clrBarShadow:%d\n", BCP->clrBarShadow);

	TRACE("clrBarHilite:%d\n", BCP->clrBarHilite);
	TRACE("clrBarDkShadow:%d\n", BCP->clrBarDkShadow);

	TRACE("clrBarLight:%d\n", BCP->clrBarLight);
	TRACE("clrBarText:%d\n", BCP->clrBarText);

	TRACE("clrWindow:%d\n", BCP->clrWindow);
	TRACE("clrWindowText:%d\n", BCP->clrWindowText);

	TRACE("clrCaptionText:%d\n", BCP->clrCaptionText);
	TRACE("clrMenuText:%d\n", BCP->clrMenuText);
	TRACE("clrActiveCaption:%d\n", BCP->clrActiveCaption);
	TRACE("clrInactiveCaption:%d\n", BCP->clrInactiveCaption);

	TRACE("clrInactiveCaptionGradient:%d\n", BCP->clrInactiveCaptionGradient);
	TRACE("clrCaptionText:%d\n", BCP->clrCaptionText);
	TRACE("clrInactiveCaptionText:%d\n", BCP->clrInactiveCaptionText);
	TRACE("clrActiveBorder:%d\n", BCP->clrActiveBorder);
	TRACE("clrInactiveBorder:%d\n", BCP->clrInactiveBorder);

#endif

	m_DocumentBkgColor.SetColor(BCP->clrBarFace);

	m_EnabledControlForeColor.SetColor(BCP->clrTextHilite);

	if (m_bUseEasyReading)
	{
		m_DisabledControlForeColor.SetColor(BCP->clrTextHilite);
		m_DisabledInStaticAreaControlForeColor.SetColor(BCP->clrTextHilite);
		const_cast<BCGPGLOBAL_DATA*>(BCP)->clrGrayedText = BCP->clrTextHilite;
	}
	else
	{
		m_DisabledControlForeColor.SetColor(BCP->clrGrayedText);
		m_DisabledInStaticAreaControlForeColor.SetColor(BCP->clrGrayedText);
	}

	m_ButtonFaceForeColor.SetColor(BCP->clrTextHilite);

	m_StaticControlForeColor.SetColor(BCP->clrTextHilite);

	m_FormFont.CloneFont(&globalData.fontRegular);
	m_ControlFont.CloneFont(&globalData.fontRegular);
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::IsDocumentBkgColorEnabled() const
{
	return m_bDocumentBkgColorEnabled;
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetTransparentColorBrush()
{
	return m_pTransparentBrush;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBackgroundColor()
{
	return m_DocumentBkgColor.IsEmpty() ? m_BkgColor.GetColor() : m_DocumentBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetBackgroundColorBrush()
{
	return m_DocumentBkgColor.GetBrush()->m_hObject == NULL ? m_BkgColor.GetBrush() : m_DocumentBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetFocusedControlBkgColor()
{
	return m_FocusedControlBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush*	TBThemeManager::GetFocusedControlBkgColorBrush()
{
	return m_FocusedControlBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetAlternateColor()
{
	return m_AlternateColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEnabledControlBkgColor()
{
	return m_EnabledControlBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush*	TBThemeManager::GetEnabledControlBkgColorBrush()
{
	return m_EnabledControlBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::IsFocusedControlBkgColorEnabled() const
{
	return m_bFocusedControlBkgColorEnabled;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDisabledControlForeColor()
{
	return m_DisabledControlForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDisabledInStaticAreaControlForeColor()
{
	return m_DisabledInStaticAreaControlForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEnabledControlForeColor()
{
	return m_EnabledControlForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEnabledControlBorderForeColor()
{
	return m_EnabledControlBorderForeColor.GetColor();
}


//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPropertyGridGroupBkgColor()
{
	return m_PropertyGridGroupBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPropertyGridGroupForeColor()
{
	return m_PropertyGridGroupForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPropertyGridSubGroupsBkgColor()
{
	return m_PropertyGridSubGroupsBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPropertyGridSubGroupsForeColor()
{
	return m_PropertyGridSubGroupsForeColor.GetColor();
}

//-----------------------------------------------------------------------------
CString	TBThemeManager::GetPropertyGridGroupCollapseImage() const
{
	return  m_strPropertyGridGroupCollapseImage;
}

//-----------------------------------------------------------------------------
CString	TBThemeManager::GetPropertyGridGroupExpandImage() const
{
	return m_strPropertyGridGroupExpandImage;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeBkgColor()
{
	return m_LinearGaugeBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeBkgGradientColor()
{
	return m_LinearGaugeBkgGradientColor.GetColor();
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetLinearGaugeMinorMarkSize()
{
	return m_LinearGaugeMinorMarkSize;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetLinearGaugeMajorMarkSize()
{
	return m_LinearGaugeMajorMarkSize;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetLinearGaugeMajorStepMarkSize()
{
	return m_LinearGaugeMajorStepMarkSize;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetLinearGaugeMinorStepMarkSize()
{
	return m_LinearGaugeMinorStepMarkSize;
}

//----------------------------------------------------------------------------------
CString TBThemeManager::GetLinearGaugeTextLabelFormat()
{
	return m_LinearGaugeTextLabelFormat;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeFrameOutLineColor()
{
	return m_LinearGaugeFrameOutLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugePointerBkgColor()
{
	return m_LinearGaugePointerBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugePointerOutLineColor()
{
	return m_LinearGaugePointerOutLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeForeColor()
{
	return m_LinearGaugeForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeTickMarkBkgColor()
{
	return m_LinearGaugeTickMarkBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeTickMarkOutLineColor()
{
	return m_LinearGaugeTickMarkOutLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetLinearGaugeLeftBorderColor()
{
	return m_LinearGaugeLeftBorderColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStaticControlForeColor()
{
	return m_StaticControlForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTooltipForeColor()
{
	return m_TooltipForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTooltipBkgColor()
{
	return m_TooltipBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarVerticalLineColor()
{
	return m_RadarVerticalLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarTitleBarButtonBkgColor()
{
	return m_RadarTitleBarButtonBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarTitleBarSelectedButtonBkgColor()
{
	return m_RadarTitleBarSelectedButtonBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarTitleBarButtonBorderBkgColor()
{
	return m_RadarTitleBarButtonBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarTitleBarLineColor()
{
	return m_RadarTitleBarLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarSeparatorColor()
{
	return m_RadarSeparatorColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarColumnBorderColor()
{
	return m_RadarColumnBorderColor.GetColor();
}
//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarColumnTitleBorderColor()
{
	return m_RadarColumnTitleBorderColor.GetColor();
}
//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetRadarPageBkgColor()
{
	return m_RadarPageBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBETransparentBmpColor()
{
	return m_BETransparentBmpColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEToolbarBtnBitmapBkgColor()
{
	return m_BEToolbarBtnBitmapBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPerformanceAnalyzerBkgColor()
{
	return m_PerformanceAnalyzerBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetPerformanceAnalyzerForeColor()
{
	return m_PerformanceAnalyzerForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTreeViewBkgColor()
{
	return m_TreeViewBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTreeViewNodeForeColor()
{
	return m_TreeViewNodeForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBCMenuBitmapBkgColor()
{
	return m_BCMenuBitmapBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBCMenuBkgColor()
{
	return m_BCMenuBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBCMenuForeColor()
{
	return m_BCMenuForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEToolbarBtnBkgColor()
{
	return m_BEToolbarBtnBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEToolbarBtnForeColor()
{
	return m_BEToolbarBtnForeColor.GetColor();
}

COLORREF TBThemeManager::GetToolbarSeparatorColor()
{
	return m_ToolbarSeparatorColor.GetColor();
}
//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetCISBitmapBkgColor()
{
	return m_CISBitmapBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetCISBitmapForeColor()
{
	return m_CISBitmapForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTransBmpTransparentDefaultColor()
{
	return m_TransBmpTransparentDefaultColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTBExtDropBitmapsTransparentColor()
{
	return m_TBExtDropBitmapsTransparentColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetFontDefaultForeColor()
{
	return m_FontDefaultForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetHyperLinkForeColor()
{
	return m_HyperLinkForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetHyperLinkBrowsedForeColor	()
{
	return m_HyperLinkBrowsedForeColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetDropDownEnabledGrdTopColorBrush()
{
	return m_DropDownEnabledGrdTopColor.GetBrush();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetDropDownEnabledGrdBottomColorBrush()
{
	return m_DropDownEnabledGrdBottomColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEditEditableOutBorderBkgColor()
{
	return m_EditEditableOutBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEditNonEditableOutBorderBkgColor()
{
	return m_EditNonEditableOutBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetEditInBorderBkgColor()
{
	return m_EditInBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStaticWithLineLineForeColor()
{
	return m_StaticWithLineLineForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStaticControlOutBorderBkgColor()
{
	return m_StaticControlOutBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStaticControlInBorderBkgColor()
{
	return m_StaticControlInBorderBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetStaticControlInBorderBkgBrush()
{
	return m_StaticControlInBorderBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetColoredControlBorderColor()
{
	return m_ColoredControlBorderColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStretchCtrlTransparentColor()
{
	return m_StretchCtrlTransparentColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStretchCtrlForeColor()
{
	return m_StretchCtrlForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBERowSelectedBkgColor()
{
	return m_BERowSelectedBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBERowSelectedForeColor()
{
	return m_BERowSelectedForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBERowSelectedFillColor()
{
	return m_BERowSelectedFillColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBETooltipBkgColor()
{
	return m_BETooltipBkgColor.GetColor();
}
//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBETooltipForeColor()
{
	return m_BETooltipForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBERowBkgAlternateColor()
{
	return m_BERowBkgAlternateColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBESeparatorColor()
{
	return m_BESeparatorColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBELockedSeparatorColor()
{
	return m_BELockedSeparatorColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEMultiSelForeColor()
{
	return m_BEMultiSelForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEMultiSelBkgColor()
{
	return m_BEMultiSelBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEResizeColVerticalLineColor()
{
	return m_BEResizeColVerticalLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEDisabledTitleForeColor()
{
	return m_BEDisabledTitleForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetADMAnimateBkgColor()
{
	return m_ADMAnimateBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetFieldInspectorHighlightForeColor()
{
	return m_FieldInspectorHighlightForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetProgressBarColor()
{
	return m_ProgressBarColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush*	TBThemeManager::GetProgressBarColorBrush()
{
	return m_ProgressBarColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetSlaveViewContainerTooltipBkgColor()
{
	return m_SlaveViewContainerTooltipBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetSlaveViewContainerTooltipForeColor()
{
	return m_SlaveViewContainerTooltipForeColor.GetColor();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetControlsUseBorders()
{
	return m_ControlsUseBorders;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetCacheImages()
{
	return m_CacheImages;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetControlsHighlightForeColor()
{
	return m_ControlsHighlightForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetControlsHighlightBkgColor()
{
	return m_ControlsHighlightBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetButtonFaceBkgColor()
{
	return m_ButtonFaceBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetButtonFaceForeColor()
{
	return m_ButtonFaceForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetButtonFaceHighLightColor()
{
	return m_ButtonFaceHighLightColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetParsedButtonHoveringColor()
{
	return m_ParsedButtonHoveringColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetParsedButtonHoveringBrush()
{
	return m_ParsedButtonHoveringColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabSelectorHoveringForeColor()
{
	return m_TabSelectorHoveringForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetParsedButtonCheckedForeColor()
{
	return m_ParsedButtonCheckedForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabSelectorSelectedBkgColor()
{
	return m_TabSelectorSelectedBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabSelectorBkgColor()
{
	return m_TabSelectorBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetTabSelectorBkgColorBrush()
{
	return m_TabSelectorBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetColoredControlLineColor()
{
	return m_ColoredControlLineColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEToolbarBtnShadowColor()
{
	return m_BEToolbarBtnShadowColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBEToolbarBtnHighlightColor()
{
	return m_BEToolbarBtnHighlightColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBETitlesBorderColor()
{
	return m_BETitlesBorderColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBETitlesBkgColor()
{
	return m_BETitlesBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBCMenuShadowColor()
{
	return m_BCMenuShadowColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetBCMenu3DFaceColor()
{
	return m_BCMenu3DFaceColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarBkgColor()
{
	return m_ToolbarBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarBkgSecondaryColor()
{
	return m_ToolbarBkgSecondaryColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDialogToolbarBkgColor()
{
	return m_DialogToolbarBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDialogToolbarForeColor()
{
	return m_DialogToolbarForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDialogToolbarTextColor()
{
	return m_DialogToolbarTextColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDialogToolbarTextHighlightedColor()
{
	return m_DialogToolbarTextColorHighlighted.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDialogToolbarHighlightedColor()
{
	return m_DialogToolbarHighlightedColor.GetColor();
}
//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarTextColor()
{
	return m_ToolbarTextColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarTextColorHighlighted()
{
	return m_ToolbarTextColorHighlighted.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarHighlightedColor()
{
	return m_ToolbarHighlightedColor.GetColor();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ToolbarHighlightedColorClickEnable() const
{
	return m_bToolbarHighlightedColorClickEnable;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::IsToolbarInfinity() const
{
	return m_bToolbarInfinity;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarHighlightedClickColor()
{
	return m_ToolbarHighlightedColorClick.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarButtonCheckedColor()
{
	return m_ToolbarButtonCheckedColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarButtonSetDefaultColor()
{
	return m_ToolbarButtonSetDefaultColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarButtonArrowFillColor()
{
	return m_ToolbarButtonArrowFillColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarLineDownColor()
{
	return m_ToolbarLineDownColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarInfinityBckButtonColor()
{
	return m_ToolbarInfinityBckButtonColor.GetColor();
}

COLORREF TBThemeManager::GetToolbarInfinitySepColor()
{
	return m_ToolbarInfinitySepColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarButtonArrowColor()
{
	return m_ToolbarButtonArrowColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetToolbarBkgBrush()
{
	return m_ToolbarBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetToolbarForeColor()
{
	return m_ToolbarForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStatusbarBkgColor()
{
	return m_StatusbarBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStatusbarTextColor()
{
	return m_StatusbarTextColor.GetColor();
}

//-----------------------------------------------------------------------------
CBrush* TBThemeManager::GetStatusbarBkgBrush()
{
	return m_StatusbarBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetCaptionBarBkgColor()
{
	return m_CaptionBarBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetCaptionBarForeColor()
{
	return m_CaptionBarForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetCaptionBarBorderColor()
{
	return m_CaptionBarBorderColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneBkgColor()
{
	return m_DockPaneBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTitleBkgColor()
{
	return m_DockPaneTitleBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTitleForeColor()
{
	return m_DockPaneTitleForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTitleHoveringForeColor()
{
	return m_DockPaneTitleHoveringForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabBkgColor()
{
	return m_DockPaneTabberTabBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabForeColor()
{
	return m_DockPaneTabberTabForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabSelectedBkgColor()
{
	return m_DockPaneTabberTabSelectedBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabSelectedForeColor()
{
	return m_DockPaneTabberTabSelectedForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabHoveringBkgColor()
{
	return m_DockPaneTabberTabHoveringBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetDockPaneTabberTabHoveringForeColor()
{
	return m_DockPaneTabberTabHoveringForeColor.GetColor();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::CanCloseDockPanes() const
{
	return m_bCanCloseDockPanes;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetDockPaneAutoHideBarSize() const
{
	return m_nDockPaneAutoHideBarSize;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetWizardStepperBkgColor()
{
	return m_WizardStepperBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetWizardStepperForeColor()
{
	return m_WizardStepperForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetStatusbarForeColor()
{
	return m_StatusbarForeColor.GetColor();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::IsXpThemed()
{
	return m_xpStyle.IsAppThemed();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::UseEasyReading() const
{
	return m_bUseEasyReading;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ShowRadarFixed() const
{
	return m_bShowRadarFixed;

}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ShowRadarEdit() const
{
	return m_bShowRadarEdit;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::HideRadarSeparatorHorizontal() const
{
	return m_bHideRadarSeparatorHorizontal;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::EnableToolBarDualColor() const
{
	return m_bEnableToolBarDualColor;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::HideRadarSeparatorVertical() const
{
	return m_bHideRadarSeparatorVertical;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::UppercaseGridTitles() const
{
	return m_bUppercaseGridTitles;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ShowToolBarTab() const
{
	return m_bShowToolBarTab;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::AutoHideToolBarButton() const
{
	return m_bAutoHideToolBarButton;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ButtonsOverlap() const
{
	return m_bButtonsOverlap;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::ToolBarEditingButtonsFix() const
{
	return m_bToolBarEditingButtonsFix;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::HasStatusBar() const
{
	return m_bHasStatusBar;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::AlwaysDropDown() const
{
	return m_AlwaysDropDown;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::HasManifest() const
{
	return m_bUseEasyReading;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::UseFlatStyle() const
{
	return m_bUseFlatStyle;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::IsToAddMoreColor() const
{
	return m_bAddMoreColor;
}

//-------------------------------------------------------------------------------
COLORREF TBThemeManager::GetAutoHideBarAccentColor()
{
	return m_AutoHideBarAccentColor.GetColor();
}

//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetFormFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_FormFont.GetSmallFont() : m_FormFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont*	TBThemeManager::GetControlFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_ControlFont.GetSmallFont() : m_ControlFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont*	TBThemeManager::GetRadarSearchFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_RadarSearchFont.GetSmallFont() : m_RadarSearchFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont*	TBThemeManager::GetTabberTabFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_TabberTabFont.GetSmallFont() : m_TabberTabFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetTabSelectorButtonFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_TabSelectorButtonFont.GetSmallFont() : m_TabSelectorButtonFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetToolBarTitleFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_ToolBarTitleFont.GetSmallFont() : m_ToolBarTitleFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetHyperlinkFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_HyperlinkFont.GetSmallFont() : m_HyperlinkFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont*	TBThemeManager::GetTileDialogTitleFont(BOOL bSmallFont /*FALSE*/)
{
	return bSmallFont ? m_TileDialogTitleFont.GetSmallFont() : m_TileDialogTitleFont.GetFont();
}


//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetWizardStepperFont()
{
	return m_WizardStepperFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont* TBThemeManager::GetTileStripFont()
{
	return m_TileStripFont.GetFont();
}

//-----------------------------------------------------------------------------
CFont*	TBThemeManager::GetStaticWithLineFont()
{
	return m_StaticWithLineFont.GetFont();
}

//-----------------------------------------------------------------------------
void TBThemeManager::Initialize(const CString& strName)
{
	//devo caricare sia il default theme di taskbuilder, sia quello eventuale di solution, oppure quello di tb sarà cablato?
	CString strThemeFullFileName = AfxGetPathFinder()->GetThemeElementFullName(strName);
	InitializeFromFullPath(strThemeFullFileName);
}

//-----------------------------------------------------------------------------
void TBThemeManager::InitializeFromFullPath(const CString& strFullPath)
{
	if (ExistFile(strFullPath) && m_XmlSettingsDoc.LoadXMLFile(strFullPath))
	{
		LoadSettings();
		m_strThemeFullPath = strFullPath;
	}
	else
		ASSERT_TRACE1(FALSE, "Theme is missing %s", (LPCTSTR)strFullPath);
	
	m_strThemeCssFullPath = AfxGetPathFinder()->GetThemeCssFullNameFromThemeName(strFullPath);
}

//-----------------------------------------------------------------------------
void TBThemeManager::DrawOldXPButton(HWND hWnd, HDC hDC, DWORD styles, DWORD addOnStyles, LPRECT pRect)
{
	HTHEME hTheme = m_xpStyle.OpenThemeData(hWnd, L"button");
	m_xpStyle.DrawThemeBackground(hTheme, hDC, styles, addOnStyles, pRect, 0);
	m_xpStyle.CloseThemeData(hTheme);
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::LightenColor(COLORREF col, double factor)
{
	if (factor > 0.0 && factor <= 1.0)
	{
		BYTE red, green, blue, lightred, lightgreen, lightblue;
		red = GetRValue(col);
		green = GetGValue(col);
		blue = GetBValue(col);
		lightred = (BYTE)((factor*(255 - red)) + red);
		lightgreen = (BYTE)((factor*(255 - green)) + green);
		lightblue = (BYTE)((factor*(255 - blue)) + blue);
		col = RGB(lightred, lightgreen, lightblue);
	}

	return(col);
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::DarkenColor(COLORREF col, double factor)
{
	if (factor > 0.0 && factor <= 1.0)
	{
		BYTE red, green, blue, lightred, lightgreen, lightblue;
		red = GetRValue(col);
		green = GetGValue(col);
		blue = GetBValue(col);
		lightred = (BYTE)(red - (factor*red));
		lightgreen = (BYTE)(green - (factor*green));
		lightblue = (BYTE)(blue - (factor*blue));
		col = RGB(lightred, lightgreen, lightblue);
	}

	return(col);
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTileDialogTitleBkgColor()
{
	return m_TileDialogTitleBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetTileDialogTitleBkgColorBrush()
{
	return m_TileDialogTitleBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTileDialogTitleForeColor()
{
	return m_TileDialogTitleForeColor.GetColor();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetTileDialogTitleForeColorBrush()
{
	return m_TileDialogTitleForeColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTileDialogStaticAreaBkgColor()
{
	return m_TileDialogStaticAreaBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetTileDialogStaticAreaBkgColorBrush()
{
	return m_TileDialogStaticAreaBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTileGroupBkgColor()
{
	return m_TileGroupBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTileTitleSeparatorColor()
{
	return m_TileTitleSeparatorColor.GetColor();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetTileGroupBkgColorBrush()
{
	return m_TileGroupBkgColor.GetBrush();
}

//-----------------------------------------------------------------------------
inline CBrush* TBThemeManager::GetTileTitleSeparatorColorBrush()
{
	return m_TileTitleSeparatorColor.GetBrush();
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetDefaultMasterFrameWidth() const
{
	return m_nDefaultMasterFrameWidth;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetDefaultMasterFrameHeight() const
{
	return m_nDefaultMasterFrameHeight;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetDefaultSlaveFrameWidth() const
{
	return ScalePix(m_nDefaultSlaveFrameWidth);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetDefaultSlaveFrameHeight() const
{
	return ScalePix(m_nDefaultSlaveFrameHeight);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetStatusBarHeight() const
{
	return ScalePix(m_nStatusBarHeight);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetScrollBarThumbSize() const
{
	return ScalePix(m_nScrollBarThumbSize);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetToolbarHighlightedHeight() const
{
	return ScalePix(m_nToolbarHighlightedHeight);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetMenuImageMargin() const
{
	return m_nMenuImageMargin;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetToolbarLineDownHeight() const
{
	return m_nToolbarLineDownHeight;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetToolbarInfinityBckButtonHeight() const
{
	return m_nToolbarInfinityBckButtonHeight;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTabSelectorMinWidth() const
{
	return ScalePix(m_nTabSelectorMinWidth);
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTabSelectorMaxWidth() const
{
	return ScalePix(m_nTabSelectorMaxWidth);
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTabSelectorMinHeight() const
{
	return ScalePix(m_nTabSelectorMinHeight);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetToolbarHeight() const
{
	return ScalePix(m_nToolbarheight);
}
//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileSpacing() const
{
	return m_nTileSpacing;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileStaticMinWidthUnit() const
{
	return m_nTileStaticMinWidthUnit;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileStaticMaxWidthUnit() const
{
	return m_nTileStaticMaxWidthUnit;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileMaxHeightUnit() const
{
	return m_nTileMaxHeightUnit;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTilePreferredHeightUnit() const
{
	return m_nTilePreferredHeightUnit;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileProportionalFactor() const
{
	return m_nTileProportionalFactor;
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileTitleHeight() const
{
	return m_nTileTitleHeight;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileTitleAlignment() const
{
	return m_nTileTitleAlignment;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileTitleTopSeparatorWidth() const
{
	return ScalePix(m_nTileTitleTopSeparatorWidth);
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileTitleBottomSeparatorWidth() const
{
	return ScalePix(m_nTileTitleBottomSeparatorWidth);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileSelectorButtonStyle() const
{
	return m_nTileSelectorButtonStyle;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileAnchorSize() const
{
	return ScalePix(m_nTileAnchorSize);
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileRightPadding() const
{
	return ScalePix(m_nTileRightPadding);
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTileInnerLeftPadding() const
{
	return ScalePix(m_nTileInnerLeftPadding);
}

//-----------------------------------------------------------------------------
int	 TBThemeManager::GetTileStaticAreaInnerLeftPadding() const
{
	return ScalePix(m_nTileStaticAreaInnerLeftPadding);
}

//-----------------------------------------------------------------------------
int	TBThemeManager::GetTileStaticAreaInnerRightPadding() const
{
	return ScalePix(m_nTileStaticAreaInnerRightPadding);
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetShowThumbnails() const
{
	return m_bShowThumbnails;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetActivateDockPaneOnMouseClick() const
{
	return m_bActivateDockPaneOnMouseClick;
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::GetBodyEditScrollBarInToolBar() const
{ 
	return m_bBodyEditScrollBarInToolBar;
}

//-----------------------------------------------------------------------------
void TBThemeManager::MakeFlat(CWnd* pWnd)
{
	//pWnd->ModifyStyleEx(WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE, 0);
	if (pWnd->GetExStyle() & (WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE))
	{
		pWnd->ModifyStyleEx((WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE), 0);
		pWnd->ModifyStyle(0, WS_BORDER);
	}

	pWnd->SetWindowPos(NULL, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
}

//-----------------------------------------------------------------------------
CString TBThemeManager::GetTileCollapseImage() const
{
	return m_strTileCollapseImage;
}

//-----------------------------------------------------------------------------
CString TBThemeManager::GetTileExpandImage() const
{
	return m_strTileExpandImage;
}

//-----------------------------------------------------------------------------
int	 TBThemeManager::GetWizardStepperHeight() const
{
	return m_nWizardStepperHeight;
}

//-----------------------------------------------------------------------------
int	 TBThemeManager::GetDefaultApplicationBCGTheme() const
{
	return m_nDefaultApplicationBCGTheme;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTabberTabStyle() const
{
	return m_nTabberTabStyle;
}

//-----------------------------------------------------------------------------
int TBThemeManager::GetTabberTabHeight() const
{
	return m_nTabberTabHeight;
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabBkgColor()
{
	return m_TabberTabBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabForeColor()
{
	return m_TabberTabForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabSelectedBkgColor()
{
	return m_TabberTabSelectedBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetScrollBarFillBkgColor()
{
	return m_ScrollBarFillBkg.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetScrollBarThumbNoPressedColor()
{
	return m_ScrollBarThumbNoPressedColor.GetColor();
}

COLORREF TBThemeManager::GetScrollBarThumbDisableColor()
{
	return m_ScrollBarThumbDisableColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetScrollBarThumbPressedColor()
{
	return m_ScrollBarThumbPressedColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetScrollBarBkgButtonNoPressedColor()
{
	return m_ScrollBarBkgButtonNoPressedColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetScrollBarBkgButtonPressedColor()
{
	return m_ScrollBarBkgButtonPressedColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabSelectedForeColor()
{
	return m_TabberTabSelectedForeColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabHoveringBkgColor()
{
	return m_TabberTabHoveringBkgColor.GetColor();
}

//-----------------------------------------------------------------------------
COLORREF TBThemeManager::GetTabberTabHoveringForeColor()
{
	return m_TabberTabHoveringForeColor.GetColor();
}

//-----------------------------------------------------------------------------
BOOL TBThemeManager::UseBCGTheme() const
{
	return m_bUseBCGTheme;
}

//-----------------------------------------------------------------------------
CString TBThemeManager::GetNameSpaceToolBarImage(CString sNameSpaceButton)
{
	for (POSITION pos = m_MapToolBarImages.GetHeadPosition(); pos != NULL;)
	{
		TBThemeToolBarImageList  lImage = m_MapToolBarImages.GetNext(pos);

		if (lImage.GetName().CompareNoCase(sNameSpaceButton) == 0)
		{
			return lImage.GetValue();
		}
	}
	return _T("");
}

//-----------------------------------------------------------------------------
void TBThemeManager::AddThemeObject(LPCTSTR sName, CObject* pObject)
{
	TB_LOCK_FOR_WRITE();

	m_ThemeObjects[sName] = pObject;
}

//-----------------------------------------------------------------------------
CObject* TBThemeManager::AddThemeObject(CRuntimeClass* pClass)
{
	ASSERT(pClass);

	CString sName(pClass->m_lpszClassName);
	CObject* pObject = GetThemeObject(sName);
	if (!pObject)
		pObject = pClass->CreateObject();

	AddThemeObject(sName, pObject);
	return pObject;
}

//-----------------------------------------------------------------------------
CObject* TBThemeManager::GetThemeObject(LPCTSTR sName)
{
	TB_LOCK_FOR_READ();
	CObject* pObject = NULL;
	
	m_ThemeObjects.Lookup(sName, pObject);
	return pObject;
}

//-----------------------------------------------------------------------------
CObject* TBThemeManager::GetThemeObject(CRuntimeClass* pClass)
{
	CString sName(pClass->m_lpszClassName);
	
	CObject* pObject = AfxGetThemeManager()->GetThemeObject(sName);
	
	if (!pObject)
		pObject = AfxGetThemeManager()->AddThemeObject(pClass);
	
	return pObject;
}


//-----------------------------------------------------------------------------
void TBThemeManager::RemoveThemeObjects()
{
	TB_LOCK_FOR_WRITE();

	if (!m_ThemeObjects.GetSize())
		return;

	POSITION pos = m_ThemeObjects.GetStartPosition();
	while (pos)
	{
		CObject *pObject;
		CString sKey;
		m_ThemeObjects.GetNextAssoc(pos, sKey, pObject);
		try
		{
			delete pObject;
		}
		catch (...)
		{
		}
	}
	m_ThemeObjects.RemoveAll();

	CStringArray fonts;
	AfxGetPathFinder()->GetAvailableThemeFonts(fonts);
	for (int i = 0; i <= fonts.GetUpperBound(); i++)
	{
		RemoveFontResourceEx(fonts[i], FR_PRIVATE, 0);
	}
}

//-----------------------------------------------------------------------------
void TBThemeManager::LoadTileStyles()
{
	CXMLNodeChildsList* pStylesList = m_XmlSettingsDoc.SelectNodes(szThemeCategoryTileStylesXpathQuery);
	if (!pStylesList)
		return;

	CString aName;

	CXMLNode* pCurrent;
	for (int i = 0; i < pStylesList->GetSize(); i++)
	{
		pCurrent = pStylesList->GetAt(i);
		if (!pCurrent)
			continue;

		pCurrent->GetAttribute(szXmlNameConst, aName);

		if (aName.IsEmpty() || pCurrent->GetChildsNum() == 0)
			continue;

		TileStyle* pStyle = AfxGetTileDialogStyle(aName);
		pStyle->OnLoadFromTheme(pCurrent);
	}

	SAFE_DELETE(pStylesList);
}

//-----------------------------------------------------------------------------
void TBThemeManager::Clear()
{
	RemoveThemeObjects();
	m_bDocumentBkgColorEnabled = TRUE;
	m_strThemeFullPath = _T("");
	m_strThemeCssFullPath = _T("");
	m_bFocusedControlBkgColorEnabled = TRUE;
	m_BkgColor.SetColor(CLR_LAVANDER) ;
	m_DocumentBkgColor.SetColor(CLR_LAVANDER);
	m_FocusedControlBkgColor.SetColor(CLR_WHITE);
	m_AlternateColor.SetColor(CLR_PALEGREEN);
	m_EnabledControlBkgColor.SetColor(CLR_WHITE);
	m_EnabledControlForeColor.SetColor(CLR_BLACK);
	m_DisabledControlForeColor.SetColor(GetSysColor(COLOR_GRAYTEXT));
	m_DisabledInStaticAreaControlForeColor.SetColor(GetSysColor(COLOR_GRAYTEXT));
	m_EnabledControlBorderForeColor.SetColor(RGB(198, 198, 198));
	m_StaticControlForeColor.SetColor(CLR_BLACK);
	m_TooltipBkgColor.SetColor(::GetSysColor(COLOR_INFOBK));
	m_TooltipForeColor.SetColor(::GetSysColor(COLOR_INFOTEXT));
	m_RadarVerticalLineColor.SetColor(CLR_BLACK);
	m_RadarTitleBarButtonBkgColor.SetColor(RGB(127, 157, 185));
	m_RadarTitleBarSelectedButtonBkgColor.SetColor(RGB(255, 127, 39));
	m_RadarTitleBarButtonBorderBkgColor.SetColor(CLR_WHITE);
	m_RadarTitleBarLineColor.SetColor(CLR_BLACK);
	m_RadarSeparatorColor.SetColor((RGB(127, 157, 185)));
	m_RadarColumnBorderColor.SetColor((RGB(196, 196, 196)));
	m_RadarColumnTitleBorderColor.SetColor((RGB(196, 196, 196)));
	m_RadarPageBkgColor.SetColor(CLR_WHITE);
	m_PerformanceAnalyzerBkgColor.SetColor(CLR_WHITE);
	m_PerformanceAnalyzerForeColor.SetColor(CLR_WHITE);
	m_TreeViewBkgColor.SetColor(CLR_WHITE);
	m_TreeViewNodeForeColor.SetColor(CLR_BLACK);
	m_BCMenuBitmapBkgColor.SetColor(CLR_WHITE);
	m_BCMenuBkgColor.SetColor(GetSysColor(COLOR_MENUTEXT));
	m_BCMenuForeColor.SetColor(GetSysColor(COLOR_MENU));
	m_BETransparentBmpColor.SetColor(CLR_PALEGRAY);
	m_BEToolbarBtnBitmapBkgColor.SetColor(CLR_WHITE);
	m_BEToolbarBtnBkgColor.SetColor(CLR_BLACK);
	m_BEToolbarBtnForeColor.SetColor(CLR_WHITE);
	m_ToolbarSeparatorColor.SetColor(COLORREF(0xC6C6C6));
	m_BEToolbarBtnShadowColor.SetColor((GetSysColor(COLOR_BTNSHADOW)));
	m_BEToolbarBtnHighlightColor.SetColor((GetSysColor(COLOR_BTNHIGHLIGHT)));
	m_BETitlesBorderColor.SetColor((GetSysColor(COLOR_ACTIVEBORDER)));
	m_BETitlesBkgColor.SetColor((-1));
	m_CISBitmapBkgColor.SetColor(CLR_WHITE);
	m_CISBitmapForeColor.SetColor(CLR_BLACK);
	m_TransBmpTransparentDefaultColor.SetColor(CLR_WHITE);
	m_TBExtDropBitmapsTransparentColor.SetColor(CLR_GREEN);
	m_HyperLinkForeColor.SetColor(RGB(0, 0, 255));
	m_HyperLinkBrowsedForeColor.SetColor((RGB(128, 0, 128)));
	m_DropDownEnabledGrdTopColor.SetColor(RGB(237, 237, 237));
	m_DropDownEnabledGrdBottomColor.SetColor(RGB(216, 216, 216));
	m_EditEditableOutBorderBkgColor.SetColor(RGB(127, 157, 185));
	m_EditNonEditableOutBorderBkgColor.SetColor(CLR_GREY);
	m_EditInBorderBkgColor.SetColor(CLR_WHITE);
	m_StaticControlOutBorderBkgColor.SetColor(CLR_GREY);
	m_StaticControlInBorderBkgColor.SetColor(CLR_WHITE);
	m_ColoredControlBorderColor.SetColor(RGB(213, 223, 229));
	m_StretchCtrlTransparentColor.SetColor(CLR_MAGENTA);
	m_StretchCtrlForeColor.SetColor(CLR_BLACK);
	m_BERowSelectedFillColor.SetColor(CLR_WHITE);
	m_BETooltipBkgColor.SetColor(CLR_BLACK);
	m_BETooltipForeColor.SetColor(CLR_GREEN);
	m_BERowSelectedBkgColor.SetColor(RGB(255, 235, 110));
	m_BERowSelectedForeColor.SetColor(RGB(0, 0, 0));
	m_BERowBkgAlternateColor.SetColor(RGB(238, 245, 252));
	m_BESeparatorColor.SetColor(RGB(127, 157, 185));
	m_BEMultiSelForeColor.SetColor(CLR_WHITE);
	m_BEMultiSelBkgColor.SetColor(RGB(160, 0, 66));
	m_BELockedSeparatorColor.SetColor(CLR_BLACK);
	m_BEResizeColVerticalLineColor.SetColor(CLR_BLACK);
	m_BEDisabledTitleForeColor.SetColor(CLR_ACQUAMARINE);
	m_ADMAnimateBkgColor.SetColor(CLR_WHITE);
	m_FieldInspectorHighlightForeColor.SetColor(CLR_BLACK);
	m_SlaveViewContainerTooltipBkgColor.SetColor(RGB(236, 225, 215));
	m_ProgressBarColor.SetColor(RGB(43, 87, 154));
	m_SlaveViewContainerTooltipForeColor.SetColor(CLR_BLACK);
	m_bUseFlatStyle = FALSE;
	m_bAddMoreColor = FALSE;
	m_AutoHideBarAccentColor.SetColor(-1);
	m_ControlsUseBorders = TRUE;
	m_CacheImages = FALSE;
	m_ControlsHighlightForeColor.SetColor(GetSysColor(COLOR_HIGHLIGHTTEXT));
	m_ControlsHighlightBkgColor.SetColor(GetSysColor(COLOR_HIGHLIGHT));
	m_ButtonFaceBkgColor.SetColor(GetSysColor(COLOR_BTNFACE));
	m_ButtonFaceForeColor.SetColor(GetSysColor(COLOR_BTNTEXT));
	m_ButtonFaceHighLightColor.SetColor(GetSysColor(COLOR_BTNHILIGHT));
	m_ColoredControlLineColor.SetColor(GetSysColor(COLOR_3DHIGHLIGHT));
	m_BCMenuShadowColor.SetColor(COLOR_3DSHADOW);
	m_BCMenu3DFaceColor.SetColor(COLOR_3DFACE);
	m_ToolbarBkgColor.SetColor(m_BkgColor.GetColor());
	m_ToolbarBkgSecondaryColor.SetColor(m_BkgColor.GetColor());
	m_ToolbarTextColor.SetColor(globalData.clrBarText);
	m_ToolbarTextColorHighlighted.SetColor(globalData.clrBarText);
	m_ToolbarHighlightedColor.SetColor(globalData.clrBarText);
	m_ToolbarHighlightedColorClick.SetColor(RGB(255, 0, 0));
	m_bToolbarHighlightedColorClickEnable = TRUE;
	m_bToolbarInfinity = FALSE;
	m_ToolbarButtonCheckedColor.SetColor(RGB(255, 0, 0));
	m_ToolbarButtonSetDefaultColor.SetColor(RGB(255, 0, 0));
	m_ToolbarButtonArrowFillColor.SetColor(RGB(255, 255, 255));
	m_ToolbarButtonArrowColor.SetColor(RGB(0, 0, 0));
	m_ToolbarForeColor.SetColor(CLR_BLACK);
	m_StatusbarBkgColor.SetColor(m_BkgColor.GetColor());
	m_StatusbarTextColor.SetColor(CLR_BLUE);
	m_StatusbarForeColor.SetColor(CLR_BLACK);
	m_ToolbarLineDownColor.SetColor(CLR_BLUE);
	m_ToolbarInfinityBckButtonColor.SetColor(CLR_WHITE);
	m_ToolbarInfinitySepColor.SetColor(CLR_BLACK);
	m_CaptionBarBkgColor.SetColor(m_ToolbarBkgColor.GetColor());
	m_CaptionBarForeColor.SetColor(m_ToolbarForeColor.GetColor());
	m_CaptionBarBorderColor.SetColor(m_ToolbarForeColor.GetColor());
	m_DockPaneBkgColor.SetColor(m_ToolbarBkgColor.GetColor());
	m_DockPaneTitleBkgColor.SetColor(m_ToolbarBkgColor.GetColor());
	m_DockPaneTitleForeColor.SetColor(m_ToolbarForeColor.GetColor());
	m_DockPaneTitleHoveringForeColor.SetColor(RGB(13, 100, 200));
	m_nDefaultMasterFrameWidth = -1;
	m_nDefaultMasterFrameHeight = -1;
	m_nDefaultSlaveFrameWidth = -1;
	m_nDefaultSlaveFrameHeight = -1;
	m_nStatusBarHeight = 40;
	m_nScrollBarThumbSize = 12;
	m_nToolbarHighlightedHeight = 3;
	m_nMenuImageMargin = 0;
	m_nToolbarLineDownHeight = 20;
	m_nToolbarInfinityBckButtonHeight = 5;
	m_nTabSelectorMinWidth = 100;
	m_nTabSelectorMaxWidth = m_nTabSelectorMaxWidth;
	m_nTabSelectorMinHeight = 50;
	m_nToolbarheight = 25;
	m_WizardStepperBkgColor.SetColor(m_TileDialogTitleBkgColor.GetColor());
	m_WizardStepperForeColor.SetColor(m_TileDialogTitleForeColor.GetColor());
	m_nWizardStepperHeight = 40,
	m_nTileSpacing = 0;
	m_nTileMaxHeightUnit = 174;
	m_nTilePreferredHeightUnit = 131;
	m_nTileProportionalFactor = 2;
	m_nTileTitleHeight = 12;
	m_nTileTitleAlignment = 0;
	m_nTileTitleTopSeparatorWidth = 0;
	m_nTileTitleBottomSeparatorWidth = 0;
	m_nTileSelectorButtonStyle = 1;
	m_nTileStaticMinWidthUnit = 0;
	m_nTileStaticMaxWidthUnit = 0;
	m_nDefaultApplicationBCGTheme = 0;
	m_bUseBCGTheme = FALSE;
	m_LinearGaugeBkgColor.SetColor(RGB(159, 197, 248));
	m_LinearGaugeBkgGradientColor.SetColor(RGB(222, 235, 245));
	m_LinearGaugeFrameOutLineColor.SetColor(RGB(159, 197, 248));
	m_LinearGaugePointerBkgColor.SetColor(RGB(0, 0, 255));
	m_LinearGaugePointerOutLineColor.SetColor(RGB(0, 0, 255));
	m_LinearGaugeForeColor.SetColor(RGB(128, 128, 128));
	m_LinearGaugeTickMarkBkgColor.SetColor(RGB(211, 211, 211));
	m_LinearGaugeTickMarkOutLineColor.SetColor(RGB(128, 128, 128));
	m_LinearGaugeLeftBorderColor.SetColor(RGB(128, 128, 128));
	m_LinearGaugeMinorMarkSize = 2;
	m_LinearGaugeMajorMarkSize = 5;
	m_LinearGaugeMajorStepMarkSize = 10;
	m_LinearGaugeMinorStepMarkSize = 1;
	m_LinearGaugeTextLabelFormat = _T("%.0f");
	m_ParsedButtonHoveringColor.SetColor(RGB(201, 222, 245));
	m_ParsedButtonCheckedForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_TabSelectorHoveringForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_TabSelectorSelectedBkgColor.SetColor(m_BkgColor.GetColor());
	m_TabSelectorBkgColor.SetColor(m_BkgColor.GetColor());
	m_bActivateDockPaneOnMouseClick = FALSE;
	m_bBodyEditScrollBarInToolBar = TRUE;
	m_bHasStatusBar = TRUE;
	m_AlwaysDropDown = TRUE;
	m_bShowThumbnails = TRUE;
	m_nTileStaticAreaInnerRightPadding = 4;
	m_nTileStaticAreaInnerLeftPadding = 0;
	m_nTileInnerLeftPadding = 0;
	m_nTileRightPadding = 2;
	m_AlternativeFontFaceName = L"Segoe UI";
	m_StaticWithLineLineForeColor.SetColor(m_TileTitleSeparatorColor.GetColor());
	m_bUppercaseGridTitles = FALSE;
	m_bShowToolBarTab = TRUE;
	m_PropertyGridGroupBkgColor.SetColor(m_BkgColor.GetColor());
	m_PropertyGridGroupForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_PropertyGridSubGroupsBkgColor.SetColor(-1);
	m_PropertyGridSubGroupsForeColor.SetColor(-1);
	m_nTabberTabStyle = 0;
	m_nTabberTabHeight = 23;
	m_TabberTabBkgColor.SetColor(m_BkgColor.GetColor());
	m_TabberTabForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_TabberTabSelectedBkgColor.SetColor(CLR_WHITE);
	m_ScrollBarFillBkg.SetColor(CLR_WHITE);
	m_ScrollBarThumbNoPressedColor.SetColor(CLR_BLACK);
	m_ScrollBarThumbDisableColor.SetColor(RGB(0xC0, 0xC0, 0xC0));
	m_ScrollBarThumbPressedColor.SetColor(CLR_WHITE);
	m_ScrollBarBkgButtonPressedColor.SetColor(CLR_WHITE);
	m_ScrollBarBkgButtonNoPressedColor.SetColor(CLR_WHITE);
	m_TabberTabSelectedForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_TabberTabHoveringBkgColor.SetColor(CLR_BLACK);
	m_TabberTabHoveringForeColor.SetColor(CLR_BLACK);
	m_DialogToolbarBkgColor.SetColor(m_ToolbarBkgColor.GetColor());
	m_DialogToolbarForeColor.SetColor(m_ToolbarForeColor.GetColor());
	m_DialogToolbarTextColor.SetColor(RGB(0, 0, 0));
	m_DialogToolbarTextColorHighlighted.SetColor(RGB(255, 255, 255));
	m_DialogToolbarHighlightedColor.SetColor(RGB(0, 0, 0));
	m_bCanCloseDockPanes = FALSE;
	m_DockPaneTabberTabBkgColor.SetColor(m_BkgColor.GetColor());
	m_DockPaneTabberTabForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_DockPaneTabberTabSelectedBkgColor.SetColor(CLR_WHITE);
	m_DockPaneTabberTabSelectedForeColor.SetColor(m_EnabledControlForeColor.GetColor());
	m_DockPaneTabberTabHoveringBkgColor.SetColor(CLR_BLACK);
	m_DockPaneTabberTabHoveringForeColor.SetColor(CLR_BLACK);
	
	m_nDockPaneAutoHideBarSize = -1;
	m_bAutoHideToolBarButton = FALSE;
	m_bButtonsOverlap = FALSE;

	CBCGPVisualManager* pVSManager = CBCGPVisualManager::GetInstance();
	m_TabberTabHoveringBkgColor.SetColor(pVSManager->GetToolbarHighlightColor());
	m_DockPaneTabberTabHoveringBkgColor.SetColor(m_TabberTabHoveringBkgColor.GetColor()); 
	m_GlobalCacheImages.ClearImageMap();
}