#pragma once

#include "beginh.dex"

////////////////////////////////////////////////////////////////////////////////
//				class TileStyle definition
////////////////////////////////////////////////////////////////////////////////
//


enum TileDialogStyle	{
	TDS_NONE = 0,
	TDS_NORMAL = 1,
	TDS_FILTER = 2,
	TDS_HEADER = 3,
	TDS_FOOTER = 4,
	TDS_WIZARD = 5,
	TDS_PARAMETERS = 6,
	TDS_BATCH = 7
};
class TB_EXPORT TileStyle
{
public:
	
	enum TileAspect { FLAT, EDGE, TOP };

public:
	virtual ~TileStyle() {}

public:
	virtual CString		GetName			() = 0;
	virtual TileAspect	GetAspect() = 0;
	virtual COLORREF	GetBackgroundColor			()			= 0;		
	virtual CBrush*		GetBackgroundColorBrush		()			= 0;		
	virtual COLORREF	GetStaticAreaBkgColor		()			= 0;		
	virtual CBrush*		GetStaticAreaBkgColorBrush	()			= 0;		
	virtual COLORREF	GetTitleBkgColor			()			= 0;		
	virtual CBrush*		GetTitleBkgColorBrush		()			= 0;		
	virtual COLORREF	GetTitleForeColor			()			= 0;		
	virtual CBrush*		GetTitleForeColorBrush		()			= 0;		
	virtual COLORREF	GetTitleSeparatorColor		()			= 0;	
	virtual COLORREF	GetStaticWithLineLineForeColor()		= 0;
	virtual CBrush*		GetTitleSeparatorColorBrush	()			= 0;		
	virtual int			GetTitleAlignment			() const	= 0;
	virtual int			GetTitlePadding				() const	= 0;
	virtual int			GetTitleTopSeparatorWidth	() const	= 0;
	virtual int			GetTitleBottomSeparatorWidth() const	= 0;
	virtual CFont*		GetTitleFont				()			= 0;		
	virtual BOOL		Collapsible					() const	= 0;
	virtual BOOL		HasTitle					() const	= 0;
	virtual int			GetTileSpacing				() const	= 0;
	virtual BOOL		Pinnable					() const	= 0;
	virtual int			GetMinHeight				() const = 0;

public:
	virtual	void		Assign				(TileStyle* pStyle)	= 0;
	virtual TileStyle*	GetReferenceStyle	()					= 0;
	virtual void	SetHasTitle(BOOL bSet = TRUE) = 0;
	virtual void	SetCollapsible(BOOL bSet = TRUE) = 0;
	virtual void	SetPinnable(BOOL bSet = TRUE) = 0;
	virtual void	SetTitleTopSeparatorWidth(int nValue) = 0;
	virtual void	SetTitleBottomSeparatorWidth(int nValue) = 0;
	virtual void	SetTileSpacing(int nvalue) = 0;
	virtual void	SetTitlePadding(int nValue) = 0;

	virtual void	OnLoadFromTheme(CXMLNode* pNode) = 0;
	virtual void	UseAlternativeColorsOf(TileStyle* pStyle) = 0;


public:
	// factory: use this method to obtain a customizable style
	static TileStyle* Inherit(TileStyle* pStyle);
};

TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleNormal		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleFilter		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleHeader		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleFooter		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleWizard		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleBatch		();
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyleParameters	();

TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyle(TileDialogStyle style);
TB_EXPORT TileStyle* AFXAPI AfxGetTileDialogStyle(CString sName, BOOL bCreate = TRUE);

TB_EXPORT TileStyle* AFXAPI AfxGetTilePanelStyleNormal();

#include "endh.dex"
