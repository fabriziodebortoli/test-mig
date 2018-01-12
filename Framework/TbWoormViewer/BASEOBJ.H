#pragma once

#include <TbGeneric\pictures.h>
#include <TbWoormEngine\multilayout.h>

#include "WoormIni.h"
#include <TbGeneric\FontsTable.h>
//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CMicroEntry;
class CWoormDocMng;
class RDEManager;
class ViewParser;
class ViewUnparser;
class BorderPen;
class Borders;
class CTBPicture;
class FontStyleTable;
class WoormTable;
class CXSDGenerator;
class Expression;

//abstract class used for object selection
//===========================================================================
class TB_EXPORT ActiveRect
{
protected:
	CRect			m_rectActive;
	CWoormDocMng*	m_pDocument;
	BOOL			m_bSingleSelection;
	BOOL			m_bLast;
	CRect			m_originalRect;

protected:
	virtual void 	Invalidate		(CRect rect, BOOL bEraseBkg = FALSE) = 0;

public:
	ActiveRect(BOOL bSingleSelection = TRUE);
	virtual ~ActiveRect() {}

	virtual void	Attach(CWoormDocMng*)					= 0;
	virtual BOOL	SetActive(CRect, BOOL m_bLast = TRUE)	= 0;
	virtual CRect	GetActiveRect()							= 0;
	virtual CRect	GetOriginalRect()						= 0;
	virtual void	Paint(CDC&, CPrintInfo*)				= 0;
	virtual void	Clear(BOOL bUpdate = TRUE)				= 0;
	virtual void	Redraw()								= 0;
	void			EnsureVisible();
	virtual void	UpdateColor()							= 0;
};

//Mago3 ActiveRect Version, fucksia brush from pattern with fucksia elements
//===========================================================================
class TB_EXPORT OldActiveRect : public ActiveRect
{
protected:

	CBrush*		m_pBrushBorder;
	CBrush*		m_pBrushCorner;
	CBrush*		m_pBrushLast;
	CPen*		m_pPen;
	
protected:
	virtual void 	Invalidate(CRect rect, BOOL bEraseBkg = FALSE);

public:
	OldActiveRect(BOOL bSingleSelection = TRUE);
	virtual ~OldActiveRect();

	virtual void	Attach			(CWoormDocMng*);
	virtual BOOL	SetActive		(CRect, BOOL m_bLast = TRUE);
	virtual CRect	GetActiveRect()		{ return m_rectActive; }
	virtual CRect	GetOriginalRect()	{ return m_originalRect; }
	virtual void	Paint			(CDC&, CPrintInfo*);
	virtual void	Clear			(BOOL bUpdate = TRUE);
	virtual void	Redraw			();
	virtual void	UpdateColor		() {/*do nothing*/}
};

//Simplified M4go ActiveRect Version, user configurable colors from property list (under RS Setting menu's voice)
//===========================================================================
class TB_EXPORT NewActiveRect : public ActiveRect
{
private:
	WoormIni*	m_pWoormIni;//mi serve per recuperare le impostazioni
	COLORREF	m_crSelectionColor;
protected:
	virtual void 	Invalidate(CRect rect, BOOL bEraseBkg = FALSE);

public:
	NewActiveRect(WoormIni* pWoormIni,BOOL bSingleSelection = TRUE );
	NewActiveRect(WoormIni* pWoormIni, CWoormDocMng* pDocument, COLORREF rgb);
	virtual ~NewActiveRect();

	virtual void	Attach(CWoormDocMng*);
	virtual BOOL	SetActive(CRect, BOOL m_bLast = TRUE);
	virtual CRect	GetActiveRect()		{ return m_rectActive; }
	virtual CRect	GetOriginalRect()	{ return m_originalRect; }
	virtual void	Paint(CDC&, CPrintInfo*);
	virtual void	Clear(BOOL bUpdate = TRUE);
	virtual void	Redraw();
	virtual void	UpdateColor();

	void SetColor(COLORREF rgb);
};

//===========================================================================
class TB_EXPORT GenericDrawObj
{
public:	
	//void	DrawRect		(CDC&, CRect, const BorderPen&, COLORREF, const Borders&, BOOL, BOOL);
	void	DrawLine		(CDC&, CPoint, int, const BorderPen&);
	void	DrawForExport	(CDC&, CRect, COLORREF clrText = DEFAULT_TEXTCOLOR , BOOL bDrawAsTitleCol = FALSE);
	void	DrawInside		(CDC&, CRect, COLORREF);
	void	DrawBorder		(CDC&, CRect, const BorderPen&, CBrush* pBrush = NULL);
	void	DrawBorders		(CDC&, CRect, const BorderPen&, Borders*, BOOL);

	void	DrawBitmap		(CDC&, CRect& , CRect&, CTBPicture*, BOOL bIsCutted, AlignType align = (DT_LEFT | DT_TOP));
	void	DrawBitmap		(CDC&, CRect& , CTBPicture*, AlignType align = (DT_LEFT | DT_TOP));
	void	DrawBitmap		(CDC&, CRect& , CRect&, CTBPicture*, AlignType align = (DT_LEFT | DT_TOP));
	BOOL	DrawBitmap		(CDC&, CRect& , CTBPicture*, const CString& sImageFile, AlignType align = (DT_LEFT | DT_TOP));

	void	CalculateBitmapWithFitMode(CRect&, CRect& rectCuttedHereToFit, BOOL& bHaveToCutted, CTBPicture*, BOOL bTransparent, AlignType align, BOOL bIsCutted, CRect& rectCutted);
	void	DrawBitmapWithFitMode(CDC&, CRect&, CTBPicture*, BOOL bTransparent, COLORREF	rgbBkgColor, AlignType align, BOOL bIsCutted, CRect& rectCutted);
	
	static void	RectForDrawBitmap
		(
					CDC*		pDC,
					CRect&		rect,
					CRect&		rectSrc,
					CTBPicture*	pBitmap,
					BOOL		bIsCutted
		);

	CRect	InsideRect		(CDC&, CRect, const BorderPen&, const Borders&, BOOL bNoBorders);
	CRect	InsideRect		(CRect, const BorderPen&, const Borders&, BOOL bNoBorders);

	//pure virtual redraw method for udating to the property grid
	virtual void		Redraw				()						= 0;
	//pure virtual methods for common properties
	virtual COLORREF*	GetBkgColor			()						= 0;
	virtual Expression** GetBkgColorExpr	()						= 0;
	virtual COLORREF*	GetLabelForeColor	()						= 0;
	virtual COLORREF*	GetValueForeColor	()						= 0;
	virtual int*		GetShadowSize		()						= 0;
	virtual COLORREF*	GetShadowColor		()						= 0;
	virtual int*		GetBorderSize		()						= 0;
	virtual COLORREF*	GetBorderColor		()						= 0;
	virtual FontIdx		GetCaptionFontIdx	()						= 0;
	virtual void		SetCaptionFontIdx	(FontIdx fontIdx)		= 0;
	virtual FontIdx		GetValueFontIdx		()						= 0;
	virtual void		SetValueFontIdx		(FontIdx fontIdx)		= 0;
	virtual CString		GetWrmStyleClass	()						= 0;
	virtual void		SetWrmStyleClass	(CString styleClass)	= 0;
	virtual void		SetWrmStyleClass	(CObject* templateObj)	= 0;
};


//===========================================================================
class TB_EXPORT BaseObj : public IBaseObj, public GenericDrawObj, public IDisposingSourceImpl
{
	friend class CWoormDocMng;

	friend int BaseObj_CompareYX(CObject*arg1, CObject* arg2);
	friend int BaseObj_CompareZYX(CObject*arg1, CObject* arg2);

	DECLARE_DYNAMIC (BaseObj)

public:
//PROPERTIES
	BOOL			m_bTransparent;
	BOOL			m_nLayer = 0;
//END Properties

	CWoormDocMng*	m_pDocument = NULL;
	BOOL			m_bInheritByTemplate = FALSE;
	BOOL			m_bPersistent = TRUE;
	WORD			m_AnchorRepeaterID = 0;
	int				m_nRepeaterRow = -1;
	CRect			m_BaseRect;

protected:

	BOOL			m_bPostedCopyClipboard;// user require copy in clipboard but must be done by Paint
	BOOL			m_bTrackMode;			// indicate start of sequence mouse down-move-up
	BOOL			m_bInhibitPaint;		// inhibit Paint during mousemove message in track mode
	BOOL			m_bFirstTime;
	BOOL			m_bChanged;
	CPoint			m_ptPrevPos;
	CMicroEntry*	m_pRectSE;
	CRect			m_rectStartMouseMove;

protected:
	void	TrackingDraw		(CDC&, CBCGPScrollView* pView, BOOL bShowRect = TRUE);
	void	TrackingCrossDraw	(CDC&, CBCGPScrollView* pView);

	// protected pure virtual function
	virtual	void	OnTrackingDraw		(CDC&, CBCGPScrollView* pView, BOOL bShowRect) = 0;
	virtual	void 	Resize				(CPoint, CPoint)	= 0;
	virtual	void	SetObjCursor		()					= 0;
	virtual	BOOL	Action				(CPoint)			= 0;
	virtual	void	BeforeAction		()					= 0;
	virtual	void	AfterAction			()					= 0;
	virtual	void	SetActionCursor		(CPoint)			= 0;
	virtual	void 	ClipActionCursor	(CDC&, CPoint, CBCGPScrollView*) = 0;

	// protected overridable virtual function
	virtual	void	CopyClipboard		(CRect rect);
	virtual	void	ChangedAction		();

	void			UpdateRepeater();

public:
	BaseObj	(CPoint, CWoormDocMng*);
 	BaseObj	(const BaseObj&);
	virtual ~BaseObj();

	virtual BaseObj* Clone() const { return NULL; }
	virtual	void	AddCreateUndoInfo();
	virtual	void	AddUndoInfo();

	void			Create		(CWoormDocMng* pDocument);	
	BOOL	IsTransparent	() const { return m_bTransparent; }
	BOOL	TrackInProgress	();
	const CRect& GetBaseRect () const;
	void	SetBaseRect		(const CRect&);
	int		Top				();
	int		Bottom			();
	int		Left			();
	int		Right			();
	int		Width			();
	int		Height			();
	
	// static public data member
	static const	CSize	DEFAULTSIZE;

	// base object class virtual member function
	virtual void	MouseMove		(CDC&, CPoint, CBCGPScrollView*, BOOL bTransMode = FALSE);
	virtual void	LeftButtonUp	(CDC&, CBCGPScrollView*);
	virtual void	LeftButtonDown	(CDC&, CPoint, CBCGPScrollView*);
	virtual BOOL	InTrackMode		();
	virtual CSize	MovingOffset	();

    // intersection function
	virtual	BOOL	InMe			(CPoint);
	virtual	BOOL	InMoveArea		(CPoint) = 0;

	// function for determinate which rectangle show when object is m_rectActive
	virtual	CRect	GetActiveRect	() const;

	// base object class pure virtual member function
	virtual	void 	Paint			(CDC&, CPrintInfo*) = 0;
	virtual	void 	Draw			(CDC&, BOOL) = 0;
	virtual void	GetObjDescription	() {}
	virtual	CRect	GetRectToInvalidate () { return m_BaseRect; }
	virtual	CRect	GetBaseRect			() { return m_BaseRect; }
	virtual	void	ShowPopup		(CPoint, CBCGPScrollView*) = 0;
	virtual	void	Format			() = 0;
	virtual void	SnapToGrid		()				{/* default do nothing*/}

	virtual void	ToggleBorder	(UINT, BOOL)	{/* default do nothing*/}
	virtual void	ToggleBorder	(UINT)			{/* default do nothing*/}
	virtual BOOL	IsBorderOn		(UINT)			{return FALSE; /* default do nothing*/}
	
	virtual void	ToggleTransparent	();
	virtual void	ToggleTransparent	(BOOL);
	virtual BOOL	IsTransparentOn		()		{ return FALSE; /* default do nothing*/}

	// clipboard management
	virtual void	PostCopyClipboard	();
	virtual	void	CopyClipboard		();
	
	// moving object method
	virtual	void	LeftObject			(int x) = 0;
	virtual	void	TopObject			(int y) = 0;
	virtual	void	MoveObject			(CSize) = 0;
	virtual	void	SizeObject			(CSize) = 0;

	virtual	void	RightSizeObject		(int) = 0;
	virtual	void	BottomSizeObject	(int) = 0;
	virtual	void	LeftSizeObject		(int) = 0;
	virtual	void	TopSizeObject		(int) = 0;

	virtual	BOOL	MovingObject		() const = 0;
	
	// undo moving and sizing object method
    virtual	void	UndoMoveAndSize	(CRect);
    
	// definition file manager base class function
	virtual	BOOL	Parse	(ViewParser&)	= 0;
	virtual	void	Unparse	(ViewUnparser&)	= 0;

    // for RDEManager manipulation
	virtual	BOOL	AssignData		(WORD wID, RDEManager*);
	virtual	BOOL	ExecCommand		(WORD wID, RDEManager*);
	virtual	void	DisableData		()	{}
	virtual void	ResetCounters	()	{}

	virtual	void	ClearDynamicAttributes() {}

	virtual	BOOL	CanSearched		() const = 0;

    // for EditorManager manipulation
	virtual	BOOL	DeleteEditorEntry	();
	virtual	BOOL	CheckTypeConsistence(CString& sError);

	// starting mouse position after creating object
	virtual CPoint	StartTrackPoint	();
	
	// virtual key accelerator management
	virtual	void	VKReturn	() = 0;

	virtual BOOL	GetSchema(CXSDGenerator*, WoormTable*) { return FALSE; }
	virtual RDEData*	GetRDEValue ()  { return NULL; }
	
	virtual	BOOL	IsAnchored() const { return FALSE; }
	virtual	BOOL	IsAlwaysHidden() const { return FALSE; }

	virtual CString GetDescription (BOOL = TRUE) const { return _T("<BaseRect>"); }

	virtual BOOL	CanDeleteField		(LPCTSTR, CString&) const  { return TRUE; }

	static void DrawDropShadow(CDC& DC, CRect& rect, int nDropShadowHeight, COLORREF crDropShadowColor);

	//virtual redraw method for updating from the property grid
	virtual void		Redraw				()						{/*do nothing*/}
	//virtual methods for common properties
	virtual COLORREF*	GetBkgColor			()						{ return NULL; }
	virtual Expression** GetBkgColorExpr	()						{ return NULL; }
	virtual COLORREF*	GetLabelForeColor	()						{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()						{ return NULL; }
	virtual int*		GetShadowSize		()						{ return NULL; }
	virtual COLORREF*	GetShadowColor		()						{ return NULL; }
	virtual int*		GetBorderSize		()						{ return NULL; }
	virtual COLORREF*	GetBorderColor		()						{ return NULL; }
	//virtual Borders*	GetBorders			()						{ return NULL; }
	virtual FontIdx		GetCaptionFontIdx	()						{ return NULL; }
	virtual void		SetCaptionFontIdx	(FontIdx fontIdx)		{/*do nothing*/}
	virtual FontIdx		GetValueFontIdx		()						{ return NULL; }
	virtual void		SetValueFontIdx		(FontIdx fontIdx)		{/*do nothing*/}
	virtual CString		GetWrmStyleClass	()						{ return NULL; }
	virtual void		SetWrmStyleClass	(CString styleClass)	{/*do nothing*/}
	virtual void		SetWrmStyleClass	(CObject* templateObj)	{/*do nothing*/}

};

//===========================================================================
class TB_EXPORT UndoInfo : public CObject
{
public:    
	TDisposablePtr<BaseObj>	m_pObj;
	CRect					m_rect;
	
public:
	UndoInfo (BaseObj* pObj, CRect rect) { m_pObj = pObj; m_rect = rect; }
};


//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//                                                                          //
//                 I N L I N E    S T A R T    D E F I N I T I O N          //
//                                                                          //
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

//==============================================================================
//	class BaseObj		inline function implementation
//==============================================================================
inline BOOL	 BaseObj::TrackInProgress()				{ return m_bTrackMode; }
inline CRect BaseObj::GetActiveRect	 ()	const		{ return m_BaseRect; }
inline const CRect& BaseObj::GetBaseRect	 ()	const		{ return m_BaseRect; }
inline void	 BaseObj::SetBaseRect	 (const CRect& rect)	{ m_BaseRect = rect; }
                                  	
inline int	 BaseObj::Top		()  { return m_BaseRect.top; }
inline int	 BaseObj::Bottom	()  { return m_BaseRect.bottom; }
inline int	 BaseObj::Left		()  { return m_BaseRect.left; }
inline int	 BaseObj::Right		()  { return m_BaseRect.right; }
inline int	 BaseObj::Width		()  { return m_BaseRect.Width(); }
inline int	 BaseObj::Height	()  { return m_BaseRect.Height(); }

inline BOOL	 BaseObj::InTrackMode	()	{ return m_bTrackMode; }
inline CSize BaseObj::MovingOffset	()	{ return m_BaseRect.TopLeft() - m_rectStartMouseMove.TopLeft(); }

inline void	 BaseObj::PostCopyClipboard		()	{ m_bPostedCopyClipboard = TRUE; }
inline BOOL	 BaseObj::DeleteEditorEntry		()	{ return TRUE; /* do nothing */ }
inline BOOL	 BaseObj::CheckTypeConsistence	(CString&)	{ return TRUE; /* do nothing */ }



#include "endh.dex"
