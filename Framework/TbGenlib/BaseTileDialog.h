#pragma once

#include "parsobj.h"
#include <TbGeneric\TileStyle.h>
#include "TileButtons.h"

#include <TbGenlib\Tile.h>
#include <TbGenlib\LayoutContainer.h>

#include "beginh.dex"

enum TB_EXPORT TileDialogSize
	{								//  _
		TILE_MICRO =	0,			//  ||   
									//   _
		TILE_MINI =		1,			//  | |   
									//   _ _
		TILE_STANDARD =	2,			//	|   |
									//   _ _ _
		TILE_LARGE =	3,			//	|     |
									//	 _ _ _ _ 
		TILE_WIDE =		4,			//  |       |

		TILE_AUTOFILL =	5			//  si autoingrandisce in altezza e larghezza
	};

class CTileDialogPart;
class ScalingInfo;


//=============================================================================
//			Class EnumTileDescriptionAssociations
//=============================================================================
// Singleton containing all the enum-description associations
class EnumTileDescriptionAssociations
{
public:
	EnumDescriptionCollection<TileDialogSize>	m_arTileDialogSize;
	EnumDescriptionCollection<TileDialogStyle>	m_arTileDialogStyle;


public:
	EnumTileDescriptionAssociations()
	{
		InitEnumTileDescriptionStructures();
	}

	void InitEnumTileDescriptionStructures();
};


//=============================================================================
//			Class CWndTileDescription
//=============================================================================
class TB_EXPORT CWndTileDescription : public CWndPanelDescription
{
	DECLARE_DYNCREATE(CWndTileDescription);

	static EnumTileDescriptionAssociations singletonEnumTileDescription;

protected:
	CWndTileDescription()
	{
		m_Type = CWndObjDescription::Tile;
		m_bOverlapped = false;

	}

public:
	bool  m_bIsCollapsed		= false;
	bool  m_bIsPinned			= true;
	bool  m_bWrapTileParts		= true;
	bool  m_bHasStaticArea		= false;
	bool  m_bResetValuesAfterUnpin = false;

	Bool3 m_bHasTitle			= B_UNDEFINED;
	Bool3 m_bIsCollapsible		= B_UNDEFINED;
	Bool3 m_bIsPinnable			= B_UNDEFINED;
	int   m_nFlex				= -1;
	int   m_nCol2Margin			= NULL_COORD;
	int	  m_nMinWidth			= NULL_COORD;
	int	  m_nMinHeight			= NULL_COORD;
	int	  m_nMaxWidth			= NULL_COORD;

	TileDialogSize m_Size		= TILE_STANDARD;
	TileDialogStyle m_Style		= TDS_NONE;
	
	CWndTileDescription(CWndObjDescription* pParent)
		: CWndPanelDescription(pParent)
	{
		m_Type = CWndObjDescription::Tile;
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);

	virtual void UpdateWindowText(CWnd *pWnd);
	
	static	CString		GetEnumDescription(TileDialogSize value);
	static	void		GetEnumValue(CString description, TileDialogSize& retVal);
	
	static	CString		GetEnumDescription(TileDialogStyle value);
	static	void		GetEnumValue(CString description, TileDialogStyle& retVal);
};

class CDesignModeManipulatorObj;
//===========================================================================
class TB_EXPORT CBaseTileDialog : public CParsedDialog, public Tile, public LayoutElement
{
	friend class CBaseTileGroup;
	friend class CLayoutContainer;
	friend class CTileDialogPart;
	friend class StripeManager;

	DECLARE_DYNAMIC(CBaseTileDialog)

private:
	CBaseTileGroup*				m_pParentTileGroup		= NULL;

	int							m_MaxStaticWidth		= 0;		// max width, in pixels, of controls in the static area, after translation
	BOOL						m_bHasToLayInNewLine	= FALSE;
	// rect at startup, usefult for resizing purposes
	CRect						m_rectOriginal;

	//  used in case of tile parts (multiple static areas)
	CArray<CTileDialogPart*>	m_TileDialogParts;
	int							m_TilePartsFlex[3];					// flex for tile parts; it is normally set in the costructor, where tile parts do not exist yet

	TileDialogSize				m_TileSize;
	int							m_nIDCStaticArea;
	CArray<HWND>				m_Anchored;
	// min and max acceptable width in AUTO mode, calculated at startup
	int							m_nAutoMinWidth			= 0;
	int							m_nAutoMaxWidth			= 0;

	BOOL						m_bInheritParentStyle	= TRUE;
	BOOL						m_bCreationCompleted	= FALSE;
	BOOL						m_bAllowCheckBoxInStaticArea = FALSE;
	BOOL						m_bLayoutInitialized;

protected:
	BOOL						m_bWrapTileParts = TRUE;
	CObject*					m_pOwner = NULL;

public:	//helper for flickering's algorithm
	BOOL						m_bCollapseExpandFromClick = FALSE;

private:
	void				SetSize					();
	void				SetSize					(CRect aRect, BOOL bMoveControls, BOOL bSetAsOriginal = FALSE);
	int					GetMaxStaticWidth		()							{ return m_MaxStaticWidth; }
	void				ResizeStaticAreaWidth	(int nNewWidth);

	BOOL				IsAutoFill				() const;
	void				ResizeRightmostControls	(int nNewWidth);

	BOOL				IsStretchableControl	(CWnd* pCtrl);
	void				CheckTileSize			();

public:
			int					GetOwnerPart		(CWnd*);
			CTileDialogPart*	GetPart				(int nIdx);

			void	SetAllowCheckBoxInStaticArea(BOOL bSet) { m_bAllowCheckBoxInStaticArea = bSet; }
			BOOL	GetAllowCheckBoxInStaticArea() { return m_bAllowCheckBoxInStaticArea; }
			BOOL	HasStaticArea			() const;
			BOOL	HasParts				() const;
			int		GetPartSize				() const;
			BOOL	IsDisplayed				();
			void	ChangeSizeTo			(CSize aSize, int nSizeAction = 0);
			void	AddStaticArea			(int nIDC);
			BOOL    IsCreationCompleted		() const { return m_bCreationCompleted; };
	virtual	BOOL	IsGroupCollapsible		();
	virtual	BOOL	IsSecurityChildHidden	()	{ return FALSE; }



	TileDialogSize	GetTileSize() const { return m_TileSize; }
	void			SetTileSize(TileDialogSize size) { m_TileSize = size; }
	
	virtual Tile*	AsATile() { return this; }
	void RecalcParts();

	void ApplyDeltaToSecondStaticArea	(int nDelta);
	BOOL IsLayoutIntialized				() { return m_bLayoutInitialized; }
public:

	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace		() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements	()						{ return NULL;}
	virtual			BOOL				IsVisible				()						{ return m_bVisible; }
	virtual			int					GetRequiredHeight		(CRect& rect);
	virtual			int					GetRequiredWidth		(CRect& rect);
	virtual			void				GetAvailableRect		(CRect &rectAvail);
	virtual			void				Relayout				(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect				(CRect &rectUsed);
	virtual			void				InitializeLayout		();
protected:
	virtual int		GetFlex					(FlexDim dim);
	virtual int		GetMinWidth				();
	virtual int		GetMaxWidth				();
	virtual int		GetMinHeight			(CRect& rect = CRect(0,0,0,0));
	virtual	BOOL	CanDoLastFlex			(FlexDim fd);

	
private:
	// backward compatibility for STRIPES layout only
	virtual CBaseTileDialog*	GetTileDialog			()					{ return this; }
	virtual CWnd*				GetCWnd					()					{ return this; }

	CTileDesignModeParamsObj* GetTileDesignModeParams();

	// algoritmo di ricalcolo delle CTileDialogParts
	void RemoveParts();
	void GenerateParts();
	void LinkControlsToParts(CRect& rectActual, int& bottomLine, int& rightmostColumn, BOOL bMoveControls = TRUE);
	int	 GetIntersectPartNo(CRect r);

protected:

	// Tile interface implementation
	virtual CWnd*				GetTileCWnd			()	{ return this; }
	virtual void				DoCollapseExpand	();
	virtual void				DoPinUnpin			();
	virtual LayoutElement*		AsALayoutElement	()	{ return this; }

public:
	virtual CBaseTileGroup*		GetParentTileGroup()	{ return m_pParentTileGroup; }

public:
	CBaseTileDialog(const CString& sName, int nIDD, CWnd* pParent = NULL);
	~CBaseTileDialog();

	// if pIdx is not null, assign to it the index of the matching static area
	BOOL		IsInStaticArea(CWnd* pWnd, int* pIdx = NULL);
	BOOL		IsInStaticArea(const CRect& rectCtrl, int* pIdx = NULL);

	CBrush*		GetStaticAreaBrush();
	COLORREF	GetStaticAreaColor();
	CBrush*		GetBackgroundBrush();
	COLORREF	GetBackgroundColor();

	CRect	GetStaticAreaRect	(int nIdx = 0);

	virtual void OnAttachParents()	{}
	virtual BOOL Create	(UINT nIDC, const CString& strTitle, CWnd* pParentWnd, TileDialogSize tileSize);
	virtual void Enable	(BOOL bEnable);
	virtual void OnPinUnpin	()		{}
	// can be reimplemented in the derived class, to attach an owner (i.e.: hotfilter, ...)
	virtual void AttachOwner(CObject* pOwner) { m_pOwner = pOwner; }

	void	UpdateTitleView		();
	BOOL	SetNextControlFocus	(BOOL bBackward);

	static int GetTileWidth(TileDialogSize tileSize);

protected:
	void	ResizeStaticAreaHeight(int nNewHeight);
	void	SetTileAlwaysNewLine(BOOL bHasToLayInNewLine) { m_bHasToLayInNewLine = bHasToLayInNewLine; }
	
	// useful to avoid that the halves of a WIDE tile wrap on narrow screen (i.e.: header, footer, etc.) 
	void	SetWrapTileParts(BOOL bSet = TRUE, int flex1 = 0, int flex2 = 0, int flex3 = 0);

protected:
	virtual BOOL	OnInitDialog		();
	virtual void	EndDialog			(int nResult);
	virtual void	OnOK				();
	virtual void	OnCancel			();
	virtual	BOOL	PreProcessMessage	(MSG* pMsg);
	virtual void	SetTileStyle		(TileStyle* pStyle);


	//{{AFX_MSG(CBaseTileDialog)
	afx_msg void	OnPaint					();

	afx_msg	BOOL	OnEraseBkgnd			(CDC* pDC);
	afx_msg HBRUSH	OnCtlColor				(CDC* pDC, CWnd* pWnd, UINT nCtlColor);

	afx_msg	void	OnWindowPosChanged		(WINDOWPOS* lpwndpos);

	afx_msg	void	OnLButtonDown			(UINT nFlags, CPoint point);

	afx_msg	LRESULT OnGetControlDescription	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnCtrlFocused			(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


////////////////////////////////////////////////////////////////////////////////
//				class TileDialogArray definition
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TileDialogArray : public Array
{
	DECLARE_DYNCREATE(TileDialogArray)

public:
	// overloaded operator helpers
	CBaseTileDialog*	GetAt			(int nIndex) const	{ return (CBaseTileDialog*) Array::GetAt(nIndex);}
	CBaseTileDialog*	Get				(int nIDC) const	
													{ 
														for (int i =0; i < GetSize(); i++)
															if (GetAt(i)->GetDlgCtrlID() == nIDC)
																return GetAt(i);
														return NULL;
													}

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};    

// Questa classe consente di inserire un tile group dentro una normale CParsedDialog
//===========================================================================
class TB_EXPORT CParsedDialogWithTiles : public CParsedDialog, public LayoutElement
{
	DECLARE_DYNCREATE(CParsedDialogWithTiles)

public:
	CBaseTileGroup*		m_pTileGroup;
	TileStyle*			m_pTileStyle; // default tile style

public:
	CParsedDialogWithTiles(UINT nIdd, CWnd* pWndParent = NULL, const CString& = _T(""));
	CParsedDialogWithTiles();
	virtual ~CParsedDialogWithTiles();

public:
	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace		() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements	()								{ return m_pLayoutContainer->GetContainedElements();}
	virtual			BOOL				IsVisible				()								{ return TRUE; }
	virtual			int					GetRequiredHeight		(CRect &rectAvail);
	virtual			int					GetRequiredWidth		(CRect &rectAvail)				{ return rectAvail.Width(); }
	virtual			void				GetAvailableRect		(CRect &rectAvail);
	virtual			void				Relayout				(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect				(CRect &rectUsed);
	virtual			void				RequestRelayout			();
	virtual			BOOL				PreProcessMessage		(MSG* pMsg);

public:
	CBaseTileGroup*   AddTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);

	void OnUpdateControls(BOOL bParentIsVisible = TRUE);

private:
	void InitLayout();

protected:
	virtual void ResizeOtherComponents(CRect aRect);

	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	DECLARE_MESSAGE_MAP();
};

#include "endh.dex"
