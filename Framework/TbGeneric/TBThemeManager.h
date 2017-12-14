#pragma once

#include <TbNameSolver\TBResourceLocker.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbXmlCore\XMLDocObj.h>

// Includere alla fine degli include del .H
#include "beginh.dex"

#define CLR_PALEGRAY	RGB(192,192,192)	
#define CLR_WHITE		RGB(255,255,255)	
#define CLR_BLACK		RGB(0,0,0)
#define CLR_LAVANDER	RGB(242,242,250)	
#define CLR_GREY		RGB(171,171,171)	
#define CLR_MAGENTA		RGB(255,0,255)		
#define CLR_GREEN		RGB(0,255,0)		
#define CLR_CORAL		RGB(0,128,128)		
#define CLR_PALEGREEN	RGB(188,210,155)	
#define CLR_ACQUAMARINE	RGB(128,128,128)	
#define CLR_BLUE		RGB(0,0,255)		
#define CLR_RED			RGB(255,0,0)		

class TBThemeFont;
class TileStyle;
//=======================================================================
class TB_EXPORT ScalingInfo
{
public:
	ScalingInfo(const TBThemeFont& aFont);
public:
	// Represent the scaling factors used by the system to convert 100 RC dialog units in pixels
	int		m_nBaseUnitsWidth;
	int		m_nBaseUnitsHeight;
};

//=======================================================================
class TB_EXPORT TBThemeColor : public CObject
{
	COLORREF	m_Color;
	CBrush		m_Brush;
	CString		m_strColorHex;

public:
	TBThemeColor();
	TBThemeColor(COLORREF color);
	~TBThemeColor();

	COLORREF		GetColor();
	CBrush*			GetBrush();
	void			SetColor(COLORREF color);
	void			SetColor(CString hexColor);
	CString			GetHexColor();
	void			Clear();
	BOOL			IsEmpty() const;
};

//=======================================================================
class TBThemeFont : public CObject
{
	CFont*	m_pFont;
	CFont*	m_pSmallFont;

	CString	m_strFFName;
	int		m_nSize;

public:
	TBThemeFont();
	~TBThemeFont();

	CFont*		GetFont();
	CFont*		GetSmallFont();

	void		SetFont
		(
		const CString& sFFName,
		int nSize,
		const CString& sFFChSet,
		int nSmallSize = 0,
		BOOL bUnderline = FALSE,
		BOOL bItalic = FALSE,
		BOOL bStriked = FALSE,
		int nWeight = FW_NORMAL
		);

	void		CreateFont(LOGFONT& lf);
	void		CloneFont(CFont*);

	CString		GetFontName() const 	{ return m_strFFName; }
	int			GetFontSize() const 	{ return m_nSize; }

private:
	static BOOL CreateFont
		(
		CFont* pFont,
		const CString& sFFName,
		int nSize,
		const CString& sFFChSet,
		BOOL bUnderline = FALSE,
		BOOL bItalic = FALSE,
		BOOL bStriked = FALSE,
		int nWeight = FW_NORMAL
		);

	static BYTE GetCharsetFromName(const CString& strCharSetName);
};

//=======================================================================
class TB_EXPORT TBThemeToolBarImageList
{
public:
	TBThemeToolBarImageList();
	TBThemeToolBarImageList(CString	sName, CString	sValue);
	~TBThemeToolBarImageList();

public:
	CString GetName();
	CString GetValue();

private:
	CString	m_sName;		// Name space buttons
	CString	m_sValue;		// Name space immage
};

//=======================================================================
class TB_EXPORT CTileDesignModeParamsObj
{
public:
	virtual int GetTileMaxHeightUnit() const = 0;
	virtual int	 GetTileAnchorSize() const = 0;
	virtual BOOL IsInternalControlMovementEnabled() { return TRUE; }
	virtual BOOL AreZeroSizesAllowed() { return TRUE; }
	virtual BOOL FreeResize() { return FALSE; }
	virtual BOOL HasToolbar() { return TRUE; }
	virtual BOOL HasStatusBar() const { return TRUE; }

	// gestione colore
	virtual CBrush*		GetTileGroupBkgColorBrush() = 0;
};

//=======================================================================
class TB_EXPORT TBThemeManager : public CObject, public CTileDesignModeParamsObj, public CTBLockable
{
	DECLARE_DYNCREATE(TBThemeManager)

	CXMLDocumentObject m_XmlSettingsDoc;
	CString			m_strThemeFullPath;
	CString			m_strThemeCssFullPath;

	// main 
	TBThemeColor	m_BkgColor;
	// brush unico per le transparenze
	CBrush*			m_pTransparentBrush;

	// document 
	BOOL			m_bDocumentBkgColorEnabled;
	TBThemeColor	m_DocumentBkgColor;

	// general controls 
	BOOL			m_bFocusedControlBkgColorEnabled;
	TBThemeColor	m_FocusedControlBkgColor;
	TBThemeColor	m_EnabledControlBkgColor;
	TBThemeColor	m_EnabledControlForeColor;
	TBThemeColor	m_EnabledControlBorderForeColor;
	TBThemeColor	m_DisabledControlForeColor;
	TBThemeColor	m_DisabledInStaticAreaControlForeColor;
	TBThemeColor	m_ControlsHighlightForeColor;
	TBThemeColor	m_ControlsHighlightBkgColor;
	TBThemeColor	m_HyperLinkForeColor;
	TBThemeColor	m_HyperLinkBrowsedForeColor;
	
	// specific controls
	TBThemeColor	m_DropDownEnabledGrdTopColor;
	TBThemeColor	m_DropDownEnabledGrdBottomColor;
	TBThemeColor	m_EditEditableOutBorderBkgColor;
	TBThemeColor	m_EditNonEditableOutBorderBkgColor;
	TBThemeColor	m_EditInBorderBkgColor;
	TBThemeColor	m_StaticControlForeColor;
	TBThemeColor	m_StaticControlOutBorderBkgColor;
	TBThemeColor	m_StaticControlInBorderBkgColor;
	TBThemeColor	m_ColoredControlBorderColor;
	TBThemeColor	m_ButtonFaceBkgColor;
	TBThemeColor	m_ButtonFaceForeColor;
	TBThemeColor	m_ButtonFaceHighLightColor;
	TBThemeColor	m_ColoredControlLineColor;
	TBThemeColor	m_ParsedButtonHoveringColor;
	TBThemeColor	m_ParsedButtonCheckedForeColor;
	TBThemeColor	m_TabSelectorHoveringForeColor;
	TBThemeColor	m_TabSelectorSelectedBkgColor;
	TBThemeColor	m_TabSelectorBkgColor;

	TBThemeColor	m_StaticWithLineLineForeColor;
	BOOL			m_ControlsUseBorders;
	BOOL			m_bShowThumbnails;
	BOOL			m_CacheImages;

	TBThemeColor	m_TooltipBkgColor;
	TBThemeColor	m_TooltipForeColor;

	TBThemeColor	m_AlternateColor;
	TBThemeColor	m_CurrentRowColor;
	TBThemeColor	m_SepLineColor;

	// Radar
	TBThemeColor	m_RadarVerticalLineColor;
	TBThemeColor	m_RadarTitleBarButtonBkgColor;
	TBThemeColor	m_RadarTitleBarLineColor;
	TBThemeColor	m_RadarTitleBarSelectedButtonBkgColor;
	TBThemeColor	m_RadarTitleBarButtonBorderBkgColor;
	TBThemeColor	m_RadarSeparatorColor;
	TBThemeColor	m_RadarColumnBorderColor;
	TBThemeColor	m_RadarColumnTitleBorderColor;
	TBThemeColor	m_RadarPageBkgColor;

	// performance analyzer
	TBThemeColor	m_PerformanceAnalyzerBkgColor;
	TBThemeColor	m_PerformanceAnalyzerForeColor;

	// tree view
	TBThemeColor	m_TreeViewBkgColor;
	TBThemeColor	m_TreeViewNodeForeColor;

	// BCMenu
	TBThemeColor	m_BCMenuBitmapBkgColor;
	TBThemeColor	m_BCMenuBkgColor;
	TBThemeColor	m_BCMenuForeColor;
	TBThemeColor	m_BCMenuShadowColor;
	TBThemeColor	m_BCMenu3DFaceColor;

	// BodyEdit
	TBThemeColor	m_BETransparentBmpColor;
	TBThemeColor	m_BEToolbarBtnBitmapBkgColor;
	TBThemeColor	m_BEToolbarBtnBkgColor;
	TBThemeColor	m_BEToolbarBtnForeColor;
	TBThemeColor	m_ToolbarSeparatorColor;
	TBThemeColor	m_BERowSelectedBkgColor;
	TBThemeColor	m_BERowSelectedForeColor;
	TBThemeColor	m_BERowSelectedFillColor;
	TBThemeColor	m_BESeparatorColor;
	TBThemeColor	m_BELockedSeparatorColor;
	TBThemeColor	m_BERowBkgAlternateColor;
	TBThemeColor	m_BEMultiSelForeColor;
	TBThemeColor	m_BEMultiSelBkgColor;
	TBThemeColor	m_BEResizeColVerticalLineColor;
	TBThemeColor	m_BEDisabledTitleForeColor;
	TBThemeColor	m_BETitlesBorderColor;
	TBThemeColor	m_BETitlesBkgColor;
	TBThemeColor	m_BETooltipBkgColor;
	TBThemeColor	m_BETooltipForeColor;
	TBThemeColor	m_BEToolbarBtnShadowColor;
	TBThemeColor	m_BEToolbarBtnHighlightColor;

	// CISBitmap
	TBThemeColor	m_CISBitmapBkgColor;
	TBThemeColor	m_CISBitmapForeColor;

	// CTransBmp
	TBThemeColor	m_TransBmpTransparentDefaultColor;
	// TBExtDropBitmaps
	TBThemeColor	m_TBExtDropBitmapsTransparentColor;

	// oggetti di frame
	TBThemeColor	m_ToolbarBkgColor;
	TBThemeColor	m_ToolbarBkgSecondaryColor;
	TBThemeColor	m_ToolbarTextColor;
	TBThemeColor	m_ToolbarTextColorHighlighted;
	TBThemeColor	m_ToolbarHighlightedColor;
	TBThemeColor	m_ToolbarHighlightedColorClick;

	TBThemeColor	m_ToolbarButtonCheckedColor;
	TBThemeColor	m_ToolbarButtonSetDefaultColor;
	TBThemeColor	m_ToolbarForeColor;
	TBThemeColor	m_ToolbarButtonArrowFillColor;
	TBThemeColor	m_ToolbarButtonArrowColor;
	TBThemeColor	m_ToolbarLineDownColor;
	TBThemeColor	m_ToolbarInfinityBckButtonColor;
	TBThemeColor	m_ToolbarInfinitySepColor;

	TBThemeColor	m_DialogToolbarBkgColor;
	TBThemeColor	m_DialogToolbarForeColor;

	TBThemeColor	m_DialogToolbarTextColor;
	TBThemeColor	m_DialogToolbarTextColorHighlighted;
	TBThemeColor	m_DialogToolbarHighlightedColor;

	
	
	TBThemeColor	m_StatusbarBkgColor;
	TBThemeColor	m_StatusbarTextColor;

	TBThemeColor	m_StatusbarForeColor;
	TBThemeColor	m_CaptionBarBkgColor;
	TBThemeColor	m_CaptionBarForeColor;
	TBThemeColor	m_CaptionBarBorderColor;
	TBThemeColor	m_DockPaneBkgColor;
	TBThemeColor	m_DockPaneTitleBkgColor;
	TBThemeColor	m_DockPaneTitleForeColor;
	TBThemeColor	m_DockPaneTitleHoveringForeColor;
	BOOL			m_bCanCloseDockPanes;
	int				m_nDockPaneAutoHideBarSize;

	TBThemeColor	m_DockPaneTabberTabBkgColor;
	TBThemeColor	m_DockPaneTabberTabForeColor;
	TBThemeColor	m_DockPaneTabberTabSelectedBkgColor;
	TBThemeColor	m_DockPaneTabberTabSelectedForeColor;
	TBThemeColor	m_DockPaneTabberTabHoveringBkgColor;
	TBThemeColor	m_DockPaneTabberTabHoveringForeColor;


	int				m_nDefaultMasterFrameWidth;
	int				m_nDefaultMasterFrameHeight;
	int				m_nDefaultSlaveFrameWidth;
	int				m_nDefaultSlaveFrameHeight;
	int				m_nStatusBarHeight;
	int				m_nScrollBarThumbSize;
	int				m_nToolbarHighlightedHeight;
	int				m_nToolbarheight;
	int				m_nToolbarLineDownHeight;
	int				m_nToolbarInfinityBckButtonHeight;
	int				m_nTabSelectorMinWidth;
	int				m_nTabSelectorMaxWidth;
	int				m_nTabSelectorMinHeight;
	int				m_nMenuImageMargin;

	// stepper color
	TBThemeColor	m_WizardStepperBkgColor;
	TBThemeColor	m_WizardStepperForeColor;

	// Fonts
	static TBThemeColor	m_FontDefaultForeColor;

	// StretchCtrl
	TBThemeColor	m_StretchCtrlTransparentColor;
	TBThemeColor	m_StretchCtrlForeColor;

	// ADM
	TBThemeColor	m_ADMAnimateBkgColor;

	// FieldInspector
	TBThemeColor	m_FieldInspectorHighlightForeColor;

	// SlaveView
	TBThemeColor	m_SlaveViewContainerTooltipBkgColor;
	TBThemeColor	m_SlaveViewContainerTooltipForeColor;

	// ProgressBar
	TBThemeColor	m_ProgressBarColor;

	//Autohide bar accent color
	TBThemeColor	m_AutoHideBarAccentColor;

	// application fonts
	TBThemeFont		m_FormFont;
	TBThemeFont		m_ControlFont;
	TBThemeFont 	m_HyperlinkFont;
	TBThemeFont		m_TileDialogTitleFont;
	TBThemeFont		m_WizardStepperFont;
	TBThemeFont		m_TabSelectorButtonFont;
	TBThemeFont		m_RadarSearchFont;
	TBThemeFont		m_TileStripFont;
	TBThemeFont		m_StaticWithLineFont;
	TBThemeFont		m_TabberTabFont;
	TBThemeFont		m_ToolBarTitleFont;

	CString			m_AlternativeFontFaceName;

	// settigns and utility members
	CVisualStylesXP	m_xpStyle;

	BOOL		m_bAddMoreColor;
	BOOL		m_bUseFlatStyle;
	BOOL		m_bUseEasyReading;
	BOOL		m_bShowRadarFixed;
	BOOL		m_bShowRadarEdit;
	BOOL		m_bHideRadarSeparatorHorizontal;
	BOOL		m_bHideRadarSeparatorVertical;
	BOOL		m_bUppercaseGridTitles;
	BOOL		m_bShowToolBarTab;
	BOOL		m_bAutoHideToolBarButton;
	BOOL		m_bButtonsOverlap;
	BOOL		m_bToolBarEditingButtonsFix;
	BOOL		m_bHasStatusBar;
	BOOL		m_AlwaysDropDown;
	BOOL		m_bEnableToolBarDualColor;

	BOOL		m_bUseColoredFocusedControl;
	BOOL		m_bHasManifest;
	BOOL		m_bUseBCGTheme;

	BOOL		m_bActivateDockPaneOnMouseClick;
	BOOL		m_bBodyEditScrollBarInToolBar;

	BOOL		m_bToolbarHighlightedColorClickEnable;
	BOOL		m_bToolbarInfinity;

	//
	int				m_nTabberTabStyle;
	int				m_nTabberTabHeight;
	TBThemeColor	m_TabberTabBkgColor;
	TBThemeColor	m_TabberTabForeColor;
	TBThemeColor	m_TabberTabSelectedBkgColor;
	TBThemeColor	m_TabberTabSelectedForeColor;
	TBThemeColor	m_TabberTabHoveringBkgColor;
	TBThemeColor	m_TabberTabHoveringForeColor;
	TBThemeColor	m_ScrollBarFillBkg;
	TBThemeColor	m_ScrollBarThumbNoPressedColor;
	TBThemeColor	m_ScrollBarThumbDisableColor;
	TBThemeColor	m_ScrollBarThumbPressedColor;
	TBThemeColor	m_ScrollBarBkgButtonNoPressedColor;
	TBThemeColor	m_ScrollBarBkgButtonPressedColor;

	//Oggetti di Tile
	TBThemeColor m_TileDialogTitleBkgColor;
	TBThemeColor m_TileDialogTitleForeColor;
	TBThemeColor m_TileDialogStaticAreaBkgColor;
	TBThemeColor m_TileGroupBkgColor;
	TBThemeColor m_TileTitleSeparatorColor;

	int			m_nTileSpacing;
	int			m_nTileStaticMinWidthUnit;
	int			m_nTileStaticMaxWidthUnit;
	int			m_nTileMaxHeightUnit;
	int			m_nTilePreferredHeightUnit;
	int			m_nTileProportionalFactor;
	int			m_nTileTitleHeight;
	int			m_nTileTitleAlignment;
	int			m_nTileTitleTopSeparatorWidth;
	int			m_nTileTitleBottomSeparatorWidth;
	int			m_nTileSelectorButtonStyle;
	int			m_nTileAnchorSize;
	int			m_nTileRightPadding;
	int			m_nTileInnerLeftPadding;
	int			m_nTileStaticAreaInnerLeftPadding;
	int			m_nTileStaticAreaInnerRightPadding;

	CString		m_strTileExpandImage;
	CString		m_strTileCollapseImage;

	int			m_nWizardStepperHeight;
	int			m_nDefaultApplicationBCGTheme;

	

	// Property Grid
	TBThemeColor	m_PropertyGridGroupBkgColor;
	TBThemeColor	m_PropertyGridGroupForeColor;
	CString			m_strPropertyGridGroupExpandImage;
	CString			m_strPropertyGridGroupCollapseImage;
	TBThemeColor	m_PropertyGridSubGroupsBkgColor;
	TBThemeColor	m_PropertyGridSubGroupsForeColor;


	// TBLinearGauge
	TBThemeColor	m_LinearGaugeBkgColor;
	TBThemeColor	m_LinearGaugeBkgGradientColor;
	TBThemeColor	m_LinearGaugeFrameOutLineColor;
	TBThemeColor	m_LinearGaugePointerBkgColor;
	TBThemeColor	m_LinearGaugePointerOutLineColor;
	TBThemeColor	m_LinearGaugeForeColor;
	TBThemeColor	m_LinearGaugeTickMarkBkgColor;
	TBThemeColor	m_LinearGaugeTickMarkOutLineColor;
	TBThemeColor	m_LinearGaugeLeftBorderColor;


	int				m_LinearGaugeMinorMarkSize;
	int				m_LinearGaugeMajorMarkSize;
	int				m_LinearGaugeMajorStepMarkSize;
	int				m_LinearGaugeMinorStepMarkSize;
	CString			m_LinearGaugeTextLabelFormat;

	CMap<CString, LPCTSTR, CObject*, CObject*> m_ThemeObjects;

	//Icons image for toolbar
	CList<TBThemeToolBarImageList, TBThemeToolBarImageList&> m_MapToolBarImages;

	// scaling info for dialog relayout
	ScalingInfo*		m_pScalingInfo;

public:
	TBThemeManager();
	~TBThemeManager();

public:
	void				Initialize				(const CString& strName);
	void				InitializeFromFullPath	(const CString& strFullPath);

	BOOL				IsXpThemed();

	CBrush*				GetTransparentColorBrush();
	CString				GetThemeFullPath() { return m_strThemeFullPath; }
	CString				GetThemeCssFullPath() { return m_strThemeCssFullPath; }

	

	// main
	COLORREF			GetBackgroundColor();
	CBrush*				GetBackgroundColorBrush();

	// document
	BOOL				IsDocumentBkgColorEnabled() const;
	BOOL				UseBCGTheme() const;

	// controls
	BOOL				IsFocusedControlBkgColorEnabled() const;

	COLORREF			GetFocusedControlBkgColor();
	CBrush*				GetFocusedControlBkgColorBrush();
	COLORREF			GetEnabledControlBkgColor();
	CBrush*				GetEnabledControlBkgColorBrush();
	COLORREF			GetDisabledControlForeColor();
	COLORREF			GetDisabledInStaticAreaControlForeColor();
	COLORREF			GetEnabledControlForeColor();
	COLORREF			GetEnabledControlBorderForeColor();
	COLORREF			GetHyperLinkForeColor();
	COLORREF			GetHyperLinkBrowsedForeColor();
	CBrush*				GetDropDownEnabledGrdTopColorBrush();
	CBrush*				GetDropDownEnabledGrdBottomColorBrush();
	COLORREF			GetEditEditableOutBorderBkgColor();
	COLORREF			GetEditNonEditableOutBorderBkgColor();
	COLORREF			GetEditInBorderBkgColor();
	COLORREF			GetStaticWithLineLineForeColor();

	BOOL				GetControlsUseBorders();
	BOOL				GetCacheImages();

	COLORREF			GetControlsHighlightForeColor();
	COLORREF			GetControlsHighlightBkgColor();
	COLORREF			GetButtonFaceBkgColor();
	COLORREF			GetButtonFaceForeColor();
	COLORREF			GetColoredControlLineColor();
	COLORREF			GetButtonFaceHighLightColor();
	COLORREF			GetParsedButtonHoveringColor();
	CBrush*				GetParsedButtonHoveringBrush();
	COLORREF			GetParsedButtonCheckedForeColor();
	COLORREF			GetTabSelectorHoveringForeColor();
	COLORREF			GetTabSelectorSelectedBkgColor();
	COLORREF			GetTabSelectorBkgColor();
	CBrush*				GetTabSelectorBkgColorBrush();


	COLORREF			GetStaticControlForeColor();
	COLORREF			GetStaticControlOutBorderBkgColor();
	CBrush*				GetStaticControlInBorderBkgBrush();
	COLORREF			GetStaticControlInBorderBkgColor();

	COLORREF			GetColoredControlBorderColor();

	COLORREF			GetTooltipForeColor();
	COLORREF			GetTooltipBkgColor();

	COLORREF			GetAlternateColor();

	// radar
	COLORREF			GetRadarVerticalLineColor();
	COLORREF			GetRadarTitleBarButtonBkgColor();
	COLORREF			GetRadarTitleBarSelectedButtonBkgColor();
	COLORREF			GetRadarTitleBarButtonBorderBkgColor();
	COLORREF			GetRadarTitleBarLineColor();
	COLORREF			GetRadarSeparatorColor();
	COLORREF			GetRadarColumnBorderColor();
	COLORREF			GetRadarColumnTitleBorderColor();
	COLORREF			GetRadarPageBkgColor();

	// performance analyzer
	COLORREF			GetPerformanceAnalyzerBkgColor();
	COLORREF			GetPerformanceAnalyzerForeColor();

	// tree View
	COLORREF			GetTreeViewBkgColor();
	COLORREF			GetTreeViewNodeForeColor();

	// BCMenu
	COLORREF			GetBCMenuBitmapBkgColor();
	COLORREF			GetBCMenuBkgColor();
	COLORREF			GetBCMenuForeColor();
	COLORREF			GetBCMenuShadowColor();
	COLORREF			GetBCMenu3DFaceColor();

	// bodyedit
	COLORREF			GetBETransparentBmpColor();
	COLORREF			GetBEToolbarBtnBitmapBkgColor();
	COLORREF			GetBEToolbarBtnBkgColor();
	COLORREF			GetBEToolbarBtnForeColor();
	COLORREF			GetToolbarSeparatorColor();
	COLORREF			GetBERowSelectedBkgColor();
	COLORREF			GetBERowSelectedForeColor();
	COLORREF			GetBERowSelectedFillColor();
	COLORREF			GetBETooltipBkgColor();
	COLORREF			GetBETooltipForeColor();
	COLORREF			GetBERowBkgAlternateColor();
	COLORREF			GetBESeparatorColor();
	COLORREF			GetBELockedSeparatorColor();
	COLORREF			GetBEMultiSelForeColor();
	COLORREF			GetBEMultiSelBkgColor();
	COLORREF			GetBEResizeColVerticalLineColor();
	COLORREF			GetBEDisabledTitleForeColor();
	COLORREF			GetBETitlesBorderColor();
	COLORREF			GetBETitlesBkgColor();
	COLORREF			GetBEToolbarBtnShadowColor();
	COLORREF			GetBEToolbarBtnHighlightColor();

	// CISBitmap
	COLORREF			GetCISBitmapBkgColor();
	COLORREF			GetCISBitmapForeColor();

	// TransBmp
	COLORREF			GetTransBmpTransparentDefaultColor();

	// TBExtDropBitmaps
	COLORREF			GetTBExtDropBitmapsTransparentColor();

	//Toolbar
	COLORREF			GetToolbarBkgColor();
	COLORREF			GetToolbarBkgSecondaryColor();
	COLORREF			GetToolbarTextColor();
	COLORREF			GetToolbarTextColorHighlighted();
	COLORREF			GetToolbarHighlightedColor();
	COLORREF			GetToolbarHighlightedClickColor();
	COLORREF			GetToolbarButtonCheckedColor();
	COLORREF			GetToolbarButtonSetDefaultColor();
	COLORREF			GetToolbarButtonArrowFillColor();
	COLORREF			GetToolbarButtonArrowColor();
	COLORREF			GetToolbarLineDownColor();
	COLORREF			GetToolbarInfinityBckButtonColor();
	COLORREF			GetToolbarInfinitySepColor();

	COLORREF			GetToolbarForeColor();
	COLORREF			GetDialogToolbarBkgColor();
	COLORREF			GetDialogToolbarForeColor();
	COLORREF			GetDialogToolbarTextColor();
	COLORREF			GetDialogToolbarTextHighlightedColor();
	COLORREF			GetDialogToolbarHighlightedColor();

	CBrush*				GetToolbarBkgBrush();
	COLORREF			GetCaptionBarBkgColor();
	COLORREF			GetCaptionBarForeColor();
	COLORREF			GetCaptionBarBorderColor();

	// dock panes
	COLORREF			GetDockPaneBkgColor();
	COLORREF			GetDockPaneTitleBkgColor();
	COLORREF			GetDockPaneTitleForeColor();
	COLORREF			GetDockPaneTitleHoveringForeColor();

	BOOL				CanCloseDockPanes() const;
	int					GetDockPaneAutoHideBarSize() const;


	COLORREF			GetDockPaneTabberTabBkgColor();
	COLORREF			GetDockPaneTabberTabForeColor();
	COLORREF			GetDockPaneTabberTabSelectedBkgColor();
	COLORREF			GetDockPaneTabberTabSelectedForeColor();
	COLORREF			GetDockPaneTabberTabHoveringBkgColor();
	COLORREF			GetDockPaneTabberTabHoveringForeColor();

	// wizard stepper
	COLORREF			GetWizardStepperBkgColor();
	COLORREF			GetWizardStepperForeColor();

	//StatusBar
	COLORREF			GetStatusbarBkgColor();
	COLORREF			GetStatusbarTextColor();
	COLORREF			GetStatusbarForeColor();
	CBrush*				GetStatusbarBkgBrush();

	// fonts
	static COLORREF		GetFontDefaultForeColor();

	// StretchCtrl
	COLORREF			GetStretchCtrlTransparentColor();
	COLORREF			GetStretchCtrlForeColor();

	// ADM
	COLORREF			GetADMAnimateBkgColor();

	// Field Inspector
	COLORREF			GetFieldInspectorHighlightForeColor();

	// slaveview
	COLORREF			GetSlaveViewContainerTooltipBkgColor();
	COLORREF			GetSlaveViewContainerTooltipForeColor();

	//progress bar

	COLORREF			GetProgressBarColor();
	CBrush*				GetProgressBarColorBrush();

	// fonts
	CFont*				GetFormFont(BOOL bSmallFont = FALSE);
	CFont*				GetControlFont(BOOL bSmallFont = FALSE);
	CFont*				GetHyperlinkFont(BOOL bSmallFont = FALSE);
	CFont*				GetTileDialogTitleFont(BOOL bSmallFont = FALSE);
	CFont*				GetTabberTabFont(BOOL bSmallFont = FALSE);
	CFont*				GetRadarSearchFont(BOOL bSmallFont = FALSE);
	CFont*				GetTabSelectorButtonFont(BOOL bSmallFont = FALSE);
	CFont*				GetToolBarTitleFont(BOOL bSmallFont = FALSE);

	CFont*				GetWizardStepperFont();
	CFont*				GetTileStripFont();
	CFont*				GetStaticWithLineFont();

	const CString&		GetAlternativeFontFaceName() const { return m_AlternativeFontFaceName; }

	// settings
	BOOL				UseEasyReading() const;
	BOOL				ShowRadarFixed() const;
	BOOL				ShowRadarEdit() const;
	BOOL				HideRadarSeparatorHorizontal()	const;
	BOOL				HideRadarSeparatorVertical()	const;
	BOOL				UppercaseGridTitles()	const;
	BOOL				EnableToolBarDualColor()		const;
	BOOL				IsToAddMoreColor() const;
	BOOL				UseFlatStyle() const;
	BOOL				HasManifest() const;
	BOOL				ShowToolBarTab() const;
	BOOL				AutoHideToolBarButton() const;
	BOOL				ButtonsOverlap() const;
	BOOL				ToolBarEditingButtonsFix() const;
	BOOL				HasStatusBar() const;
	BOOL				AlwaysDropDown() const;
	BOOL				ToolbarHighlightedColorClickEnable() const;
	BOOL				IsToolbarInfinity() const;

	COLORREF			GetAutoHideBarAccentColor();

	//Tile
	COLORREF			GetTileDialogTitleBkgColor();
	COLORREF			GetTileDialogTitleForeColor();
	CBrush*				GetTileDialogTitleBkgColorBrush();
	CBrush*				GetTileDialogTitleForeColorBrush();
	COLORREF			GetTileDialogStaticAreaBkgColor();
	CBrush*				GetTileDialogStaticAreaBkgColorBrush();
	COLORREF			GetTileGroupBkgColor();
	CBrush*				GetTileGroupBkgColorBrush();
	COLORREF			GetTileTitleSeparatorColor();
	CBrush*				GetTileTitleSeparatorColorBrush();

	int					GetDefaultMasterFrameWidth() const;
	int					GetDefaultMasterFrameHeight() const;
	int					GetDefaultSlaveFrameWidth() const;
	int					GetDefaultSlaveFrameHeight() const;
	int					GetStatusBarHeight() const;
	int					GetScrollBarThumbSize() const;
	int					GetToolbarHighlightedHeight() const;
	int					GetMenuImageMargin() const;
	int					GetToolbarHeight() const;
	int					GetToolbarLineDownHeight() const;
	int					GetToolbarInfinityBckButtonHeight() const;
	int					GetTabSelectorMinWidth() const;
	int					GetTabSelectorMaxWidth() const;
	int					GetTabSelectorMinHeight() const;

	int					GetTileSpacing() const;
	int					GetTileStaticMinWidthUnit() const;
	int					GetTileStaticMaxWidthUnit() const;
	int					GetTileMaxHeightUnit() const;
	int					GetTilePreferredHeightUnit() const;
	int					GetTileProportionalFactor() const;
	int					GetTileTitleHeight() const;
	int					GetTileTitleAlignment() const;
	int					GetTileTitleTopSeparatorWidth() const;
	int					GetTileTitleBottomSeparatorWidth() const;
	int					GetTileSelectorButtonStyle() const;
	int					GetTileAnchorSize() const;
	int					GetTileRightPadding() const;
	int					GetTileInnerLeftPadding() const;
	int					GetTileStaticAreaInnerLeftPadding() const;
	int					GetTileStaticAreaInnerRightPadding() const;
	BOOL				FillsVerticalEmptySpaceInColumnLayout() const { return TRUE; }

	CString				GetTileCollapseImage() const;
	CString				GetTileExpandImage() const;
	CString				GetNameSpaceToolBarImage(CString sNameSpaceButton);

	BOOL				GetShowThumbnails() const;

	BOOL				GetActivateDockPaneOnMouseClick() const;
	BOOL				GetBodyEditScrollBarInToolBar() const;

	void				DrawOldXPButton(HWND hWnd, HDC hDC, DWORD styles, DWORD addOnStyles, LPRECT pRect);

	int					GetWizardStepperHeight() const;
	int					GetDefaultApplicationBCGTheme() const;
	
	
	// tabber
	int					GetTabberTabStyle() const;
	int					GetTabberTabHeight() const;
	COLORREF			GetTabberTabBkgColor();
	COLORREF			GetTabberTabForeColor();
	COLORREF			GetTabberTabSelectedBkgColor();
	COLORREF			GetScrollBarFillBkgColor();
	COLORREF			GetScrollBarThumbNoPressedColor();
	COLORREF			GetScrollBarThumbDisableColor();
	COLORREF			GetScrollBarThumbPressedColor();
	COLORREF			GetTabberTabSelectedForeColor();
	COLORREF			GetScrollBarBkgButtonNoPressedColor();
	COLORREF			GetScrollBarBkgButtonPressedColor();
	COLORREF			GetTabberTabHoveringBkgColor();
	COLORREF			GetTabberTabHoveringForeColor();

	// property grid
	COLORREF			GetPropertyGridGroupBkgColor	();
	COLORREF			GetPropertyGridGroupForeColor	();
	CString				GetPropertyGridGroupCollapseImage() const;
	CString				GetPropertyGridGroupExpandImage	() const;
	COLORREF			GetPropertyGridSubGroupsBkgColor();
	COLORREF			GetPropertyGridSubGroupsForeColor();


	// TBLinearGauge
	COLORREF			GetLinearGaugeBkgColor();
	COLORREF			GetLinearGaugeBkgGradientColor();
	COLORREF			GetLinearGaugeFrameOutLineColor();
	COLORREF			GetLinearGaugePointerBkgColor();
	COLORREF			GetLinearGaugePointerOutLineColor();
	COLORREF			GetLinearGaugeForeColor();
	COLORREF			GetLinearGaugeTickMarkBkgColor();
	COLORREF			GetLinearGaugeTickMarkOutLineColor();
	COLORREF			GetLinearGaugeLeftBorderColor();
	int					GetLinearGaugeMinorMarkSize();
	int					GetLinearGaugeMajorMarkSize();
	int					GetLinearGaugeMajorStepMarkSize();
	int					GetLinearGaugeMinorStepMarkSize();
	CString				GetLinearGaugeTextLabelFormat();


	// gestione degli oggetti aggiuntivi
	void	 AddThemeObject(LPCTSTR sName, CObject* pObject);
	CObject* AddThemeObject(CRuntimeClass* pClass);
	CObject* GetThemeObject(LPCTSTR sName);
	CObject* GetThemeObject(CRuntimeClass* pClass);
	void RemoveThemeObjects();

	virtual LPCSTR GetObjectName() const { return "TBThemeManager"; }


	// utility members
	static COLORREF LightenColor(COLORREF col, double factor);
	static COLORREF DarkenColor(COLORREF col, double factor);
	static void		MakeFlat(CWnd* pWnd);

	// scaling info for dialog layout
	int	GetBaseUnitsHeight	()	{ ASSERT(m_pScalingInfo); return m_pScalingInfo->m_nBaseUnitsHeight; }
	int	GetBaseUnitsWidth	()	{ ASSERT(m_pScalingInfo);  return m_pScalingInfo->m_nBaseUnitsWidth; }

	int GetIntSetting(CXMLNode* pNode, CString settingName);
	COLORREF GetColorSetting(CXMLNode* pNode, CString settingName);
	int GetBoolSetting(CXMLNode* pNode, CString settingName);
	BOOL GetFontSetting(CXMLNode* pNode, TBThemeFont& font, CString settingName);
	static CString GetElementTag() { return _T("ThemeElement"); }
private:
	void Clear			();
	void LoadSettings	();
	void LoadBCGTheme	();
	void LoadThemeImages();
	void LoadTileStyles	();
	void LoadTileStyle(CXMLNode* pNode, TileStyle* pStyle);

	void SetBoolSetting				(BOOL& aValue, CString settingName);
	void SetIntSetting				(int& aValue, CString settingName);
	void SetColorSetting			(TBThemeColor& color, CString settingName);
	void SetFontSetting				(TBThemeFont& font, CString settingName);
	void SetTileImageSetting		(CString&strImageElement, CString settingName);
	void SetTextAndFormatsSetting	(CString& strFormatElement, CString settingName);
	void SetHyperLinkFont			(TBThemeFont& font);
};

//----------------------------------------------------------------------------------------------

DECLARE_SMART_LOCK_PTR(TBThemeManager)
DECLARE_CONST_SMART_LOCK_PTR(TBThemeManager)

TB_EXPORT TBThemeManager* AFXAPI AfxGetThemeManager();

// TODOERP quest sono per compatibilità di compilazione di ERP
TB_EXPORT COLORREF	AFXAPI	AfxGetBkgColor();
TB_EXPORT CBrush*	AFXAPI	AfxGetBkgColorBrush();
TB_EXPORT CFont*	AFXAPI	AfxGetControlFont();
TB_EXPORT CFont*	AFXAPI	AfxGetFormFont();
TB_EXPORT CFont*	AFXAPI	AfxGetHyperlinkFont();
TB_EXPORT BOOL		AFXAPI 	AfxIsOSVista();

extern const TB_EXPORT TCHAR szDefaultThemeConfig[];
extern const TB_EXPORT TCHAR szThemeManagerFlags[];
extern const TB_EXPORT TCHAR szDefaultApplicationBCGTheme[];
//----------------------------------------------------------------------------------------------
#include "endh.dex"
