
#pragma once

#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\TBScrollBar.h>
#include "radardef.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CriticalArea;
class CWalkBitmap;

class CRadarDoc;
class CRadarFrame;
class RadarBody;

// Definizione di CRadarView
//=============================================================================
class TB_EXPORT CRadarView : public CBCGPScrollView
{
	DECLARE_DYNCREATE(CRadarView)
protected:
	int m_nOldActiveRow;
	int	m_nLPOldPosHScrollBar;
	int	m_nRowWidth;
	int	m_nRowHeight;
	int	m_nNrVisibleRows;
	int	m_nRadVersion;

	BOOL m_bInvertEasyReadingCr;
	BOOL m_bEasyReading;

	BOOL m_bShowVerticalLine;
	BOOL m_bShowHorizontalLine;

	BOOL m_bIsJustActivated;
	BOOL m_bCtrlKeyDown;
	BOOL m_bShiftKeyDown;
	BOOL m_bDrawXORFirstTime;
	BOOL m_bMousePosChanged;

	CFont*	m_pRVFont;
	CPoint	m_LPPrevMousePos;

	CriticalArea*	m_pProcessingMouse;
	CBitmap			m_MultiLines;
	RadarBody*		m_pRadarBody;

	SelectCursor m_nAction;

	CTBScrollBar*		m_pVScrollBar;
	int				m_iLastXOnSize;
	int				m_iLastYOnSize;
	
	int				m_iScrollPos;
	LONG			m_iScrollMax;

	UINT			m_lDragScroll;

public:
	CRadarView	();
	~CRadarView	();

public:
	CRadarDoc*		GetDocument		();
	CRadarFrame*	GetRadarFrame	();

public:
	// graphic attributes
	void AttachBodyToView (RadarBody*);

	int GetAvailableVisibleRows	();
	int GetVisibleRows			();
	int GetRowHeight			();
	int GetRadVersion			();

	// update the title bar position
	void	TLBScroll	(BOOL bRedraw = TRUE);
	void	TLBUpdate	();
	int		UpdateOldPosHScrollBar	();

	// mouse and cursor action and status
	BOOL LeftButtonDownExAction	(CPoint&);
	void LeftButtonUp			(CPoint&);
	void MouseMove				(CPoint& LPMousePos);

	void ResetActionAndCursor	();
	void ResetMouse				() { m_pProcessingMouse->Unlock();  } 			
	SelectCursor GetCursorAction(CPoint& LPMousePos);

	// insert delete fields
	void InsertColumnWithMouse		();
	BOOL ResetInsertDeleteColumns	();

	void FontChanged		(LOGFONT);
	void CreateFontIndirect	(LOGFONT);

	void ChangeSelVisibleRow (int yCoord);
	void SetSelectedVisibleRow(int iRow);

	void WriteNoAskConfirm	(LPCTSTR lpszSection, LPCTSTR lpszRadarFile);
	void ReadNoAskConfirm	(LPCTSTR lpszSection, LPCTSTR lpszRadarFile);

	void InvertEasyReadingCr();
	void SetRowHeight		();

	void OnKeyScrollRowDown	();
	void OnKeyScrollRowUp		();
	void OnKeyScrollPageDown	();
	void OnKeyScrollPageUp		();
	void OnKeyScrollPageEnd	();
	void OnKeyScrollPageHome	();


private:
	void RedrawRow		(int nIdxRow, CRect&);
	void ScrollRowDown	();
	void ScrollRowUp 	();
	void SetVScrollBar	();

	BOOL WriteRadarProfileInt (LPCTSTR lpszSection, LPCTSTR lpszEntry, int nValue, LPCTSTR lpszRadarFile);

	int			DrawRow			(CDC*, int nRow, const CRect& rectClip, COLORREF crBkgnd);
	COLORREF	GetBkgndColor	(int nRow);

protected:
	// drawing functions
	void DoDraw				(CDC*, CRect rectInvalid);
	void DoDrawEmptyBody	(CDC*, CRect rectInvalid);

	void DPToLP				(CPoint&);
	int	 DrawRowActive		(CDC*, int nRow, const CRect& rectClip);
	int	 DrawRowDeactive	(CDC*, int nRow, const CRect& rectClip);
	void DrawSepLine		(CDC*, int nXStart, int nYStart, int nXEnd, int nYEnd);
	void DrawVerticalLine 	(CPoint&, COLORREF, int nPenStyle);
	void LPToDP				(CPoint&);
	void TrackingDraw		(CDC&, int nDrawMode = 0);

	// set the graphic attributes and action
	void ClipActionCursor	();
	void StartAction		(CPoint LPMousePos, int nRightBorder = 0);
	void UpdateScrollSizes	();

protected:
	void UpdateRowHeight (BOOL bUpdate);

	BOOL CanDoLButtonDown		();
	BOOL CanDoMouseMove			();
	BOOL CanDoLButtonUp			();
	BOOL CanDoUpdateScrollView	();
	BOOL CanDoSetCursor			();

	// overrides of CBCGPScrollView
	virtual	void OnDraw			(CDC*);
	virtual	void OnUpdate		(CView*, LPARAM, CObject*);
	virtual	void OnInitialUpdate();
	virtual	void OnActivateView (BOOL, CView*, CView*);

	// Generated message map functions
	//{{AFX_MSG(CRadarView)
		afx_msg BOOL OnEraseBkgnd	(CDC*)		{ return TRUE; }
		afx_msg void OnKeyDown		(UINT, UINT, UINT);
		afx_msg void OnKeyUp		(UINT, UINT, UINT);
		afx_msg void OnHScroll		(UINT, UINT, CScrollBar*);
		afx_msg void OnVScroll		(UINT, UINT, CScrollBar*);
		afx_msg void OnLButtonDown	(UINT, CPoint);
		afx_msg void OnLButtonDblClk(UINT, CPoint);
		afx_msg void OnLButtonUp	(UINT, CPoint);
		afx_msg void OnMouseMove	(UINT, CPoint);
		afx_msg BOOL OnSetCursor	(CWnd*, UINT, UINT);
		afx_msg void OnSize			(UINT, int, int);

		afx_msg void OnMinusMultiLines	();
		afx_msg void OnPlusMultiLines	();
		afx_msg void OnEasyReading		();

		afx_msg void OnUpdateMinusMultiLines(CCmdUI*);
		afx_msg void OnUpdatePlusMultiLines	(CCmdUI*);
		afx_msg	void OnUpdateEasyReading	(CCmdUI*);

		// Accelerator
		afx_msg	void OnEnd		();
		afx_msg	void OnHome		();
		afx_msg	void OnPageLeft	();
		afx_msg	void OnPageRight();
		afx_msg	void OnLeft		();
		afx_msg	void OnRight	();
		LRESULT OnRadarSelect(WPARAM wParam, LPARAM lParam);
		LRESULT OnRadarMoveRow(WPARAM wParam, LPARAM lParam);
		
		BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBCGPScrollView::AssertValid(); }
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
//	Inlines
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
inline void CRadarView::OnEnd		() { OnHScroll (SB_THUMBTRACK,	m_nRowWidth, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::OnHome		() { OnHScroll (SB_THUMBTRACK,	0, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::OnPageLeft	() { OnHScroll (SB_PAGELEFT,	0, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::OnPageRight () { OnHScroll (SB_PAGERIGHT,	0, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::OnLeft		() { OnHScroll (SB_LINELEFT,	0, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::OnRight 	() { OnHScroll (SB_LINERIGHT,	0, GetScrollBarCtrl(SB_HORZ));}
inline void CRadarView::MouseMove	(CPoint& LPMousePos)
{ if (CanDoMouseMove ()) DrawVerticalLine (LPMousePos, AfxGetThemeManager()->GetRadarVerticalLineColor(), PS_DOT); }
inline void CRadarView::InvertEasyReadingCr () { m_bInvertEasyReadingCr = !m_bInvertEasyReadingCr; }

inline int CRadarView::DrawRowDeactive	(CDC* pDC, int nRow, const CRect& rectClip)
	{ return DrawRow (pDC, nRow, rectClip, GetBkgndColor (nRow));}
inline int CRadarView::GetVisibleRows	()	{ return m_nNrVisibleRows;	}
inline int CRadarView::GetRadVersion	()	{ return m_nRadVersion;	}
inline int CRadarView::GetRowHeight		()	{ return m_nRowHeight;		}

inline BOOL CRadarView::CanDoLButtonDown		() { return TRUE;			}
inline BOOL CRadarView::CanDoLButtonUp			() { return CanDoMouseMove();			}
inline BOOL CRadarView::CanDoMouseMove			() { return m_nAction	== COL_RESIZE;	}
inline BOOL CRadarView::CanDoUpdateScrollView	() { return m_pRadarBody != NULL;		}
inline BOOL CRadarView::CanDoSetCursor			() { return m_nAction	 != INS_DEL_COL;} 

inline CRadarDoc*	CRadarView::GetDocument		()
	{ ASSERT (m_pDocument); return (CRadarDoc*) m_pDocument; }
inline CRadarFrame* CRadarView::GetRadarFrame	()
	{ CRadarFrame *pFrame = (CRadarFrame*) GetParentFrame(); ASSERT (pFrame); return pFrame; }

#define RAD_VERSION	2

#include "endh.dex"
