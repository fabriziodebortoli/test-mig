//*******************************************************************************
// COPYRIGHT NOTES
// ---------------
// This is a part of BCGControlBar Library Professional Edition
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//
// BCGPDlgImpl.h: interface for the CBCGPDlgImpl class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_BCGPDLGIMPL_H__18772215_4E74_4900_82E4_288CA46AB7E0__INCLUDED_)
#define AFX_BCGPDLGIMPL_H__18772215_4E74_4900_82E4_288CA46AB7E0__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "BCGCBPro.h"
#include "BCGGlobals.h"
#include "BCGPVisualManager.h"
#include "BCGPLayout.h"

#if (!defined _BCGSUITE_) && (!defined _BCGSUITE_INC_)
#include "BCGPShadowManager.h"
#endif

class CBCGPPopupMenu;
class CBCGPFrameCaptionButton;

struct BCGCBPRODLLEXPORT CBCGPControlInfoTip
{
	CBCGPControlInfoTip(CWnd* pWnd = NULL, LPCTSTR lpszText = NULL, DWORD dwVertAlign = DT_TOP)
	{
		m_hWnd = pWnd->GetSafeHwnd();
		m_dwVertAlign = dwVertAlign;
		m_rect.SetRectEmpty();

		m_strText = (lpszText == NULL) ? _T("") : lpszText;

		int nIndex = m_strText.Find (_T('\n'));
		if (nIndex >= 0)
		{
			m_strDescription = m_strText.Mid (nIndex + 1);
			m_strText = m_strText.Left(nIndex);
		}
	}

	HWND	m_hWnd;
	CString	m_strText;
	CString	m_strDescription;
	DWORD	m_dwVertAlign;
	CRect	m_rect;
};

class BCGCBPRODLLEXPORT CBCGPDlgImpl  
{
	friend class CBCGPDialog;
    friend class CBCGPMessageBox;
	friend class CBCGPPropertyPage;
	friend class CBCGPPropertySheet;
	friend class CBCGPDialogBar;
	friend class CBCGPFormView;
	friend class CBCGPShadowManager;
	friend class CBCGPProgressDlg;
	friend class CBCGPVisualManager2007;
	friend class CBCGPVisualManagerCarbon;

protected:
	CBCGPDlgImpl(CWnd& dlg);
	virtual ~CBCGPDlgImpl();

	static LRESULT CALLBACK BCGDlgMouseProc (int nCode, WPARAM wParam, LPARAM lParam);

	void SetActiveMenu (CBCGPPopupMenu* pMenu);

	BOOL ProcessMouseClick (POINT pt);
	BOOL ProcessMouseMove (POINT pt);

	BOOL PreTranslateMessage(MSG* pMsg);
	BOOL OnCommand (WPARAM wParam, LPARAM lParam);
	void OnNcActivate (BOOL& bActive);
	void OnActivate(UINT nState, CWnd* pWndOther);

	void EnableVisualManagerStyle (BOOL bEnable, BOOL bNCArea = FALSE, const CList<UINT,UINT>* plstNonSubclassedItems = NULL);

	void OnDestroy ();
	void OnDWMCompositionChanged ();

	BOOL EnableAero (BCGPMARGINS& margins);
	void GetAeroMargins (BCGPMARGINS& margins) const;
	BOOL HasAeroMargins () const;
	void ClearAeroAreas (CDC* pDC);

	HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);

	BOOL OnNcPaint();
	BOOL OnNcCalcSize(BOOL bCalcValidRects, NCCALCSIZE_PARAMS FAR* lpncsp);
	void UpdateCaption ();
	void UpdateCaptionButtons ();
	UINT OnNcHitTest (CPoint point);
	void OnNcMouseMove(UINT nHitTest, CPoint point);
	void OnLButtonDown(CPoint point);
	void OnLButtonUp(CPoint point);
	void OnMouseMove(CPoint point);
	CBCGPFrameCaptionButton* GetSysButton (UINT nHit);

	void SetHighlightedSysButton (UINT nHitTest);
	void OnTrackCaptionButtons (CPoint point);
	void StopCaptionButtonsTracking ();
	void RedrawCaptionButton (CBCGPFrameCaptionButton* pBtn);

	void SetControlInfoTip(UINT nCtrlID, LPCTSTR lpszInfoTip, DWORD dwVertAlign = DT_TOP, BOOL bRedrawInfoTip = FALSE);
	void SetControlInfoTip(CWnd* pWndCtrl, LPCTSTR lpszInfoTip, DWORD dwVertAlign = DT_TOP, BOOL bRedrawInfoTip = FALSE);

	void GetControlInfoTipRect(CWnd* pWndCtrl, CRect& rect, DWORD dwVertAlign);
	void DrawControlInfoTips(CDC* pDC);

	void CreateTooltipInfo();
	void UpdateToolTipsRect();

	HWND GetControlInfoTipFromPoint(CPoint point, CString& strInfo, CString& strDescription);
	BOOL OnNeedTipText(UINT id, NMHDR* pNMH, LRESULT* pResult);

	BOOL IsOwnerDrawCaption ()
	{
		if (m_Dlg.GetSafeHwnd() != NULL && m_Dlg.GetMenu() != NULL)
		{
			return FALSE;
		}

#if (!defined _BCGSUITE_) && (!defined _BCGSUITE_INC_)
		return m_bVisualManagerStyle && m_bVisualManagerNCArea && CBCGPVisualManager::GetInstance ()->IsOwnerDrawCaption ();
#else
		return m_bVisualManagerStyle && m_bVisualManagerNCArea && CMFCVisualManager::GetInstance ()->IsOwnerDrawCaption ();
#endif
	}

	CRect GetCaptionRect ();
	void OnChangeVisualManager ();
	void OnWindowPosChanged(WINDOWPOS FAR* lpwndpos);
	void OnGetMinMaxInfo (MINMAXINFO FAR* lpMMI);

	int GetCaptionHeight ();

	void EnableLayout(BOOL bEnable = TRUE, CRuntimeClass* pRTC = NULL, BOOL bResizeBox = TRUE);
	BOOL IsLayoutEnabled() const
	{
		return m_pLayout != NULL;
	}

	void EnableDragClientArea(BOOL bEnable = TRUE);
	BOOL IsDragClientAreaEnabled() const
	{
		return m_bDragClientArea;
	}

	int OnCreate();

	void DrawResizeBox(CDC* pDC);
	void AdjustControlsLayout();

	void EnableBackstageMode();

	BOOL LoadPlacement(LPCTSTR lpszProfileName = NULL);
	BOOL SavePlacement(LPCTSTR lpszProfileName = NULL);
	BOOL SetPlacement(WINDOWPLACEMENT& wp);

	BOOL GetPlacementSection(LPCTSTR lpszProfileName, CString& strSection);

	CWnd&					m_Dlg;
	static HHOOK			m_hookMouse;
	static CBCGPDlgImpl*	m_pMenuDlgImpl;
	BOOL					m_bVisualManagerStyle;
	BOOL					m_bVisualManagerNCArea;
	CArray<CWnd*, CWnd*>	m_arSubclassedCtrls;
	BCGPMARGINS				m_AeroMargins;
	BOOL					m_bTransparentStaticCtrls;
	CObList					m_lstCaptionSysButtons;
	UINT					m_nHotSysButton;
	UINT					m_nHitSysButton;
	CRect					m_rectRedraw;
	BOOL					m_bWindowPosChanging;
	BOOL					m_bIsWindowRgn;
	BOOL					m_bHasBorder;
	BOOL					m_bHasCaption;
	BOOL					m_bIsWhiteBackground;
	BOOL					m_bDragClientArea;
	CToolTipCtrl*			m_pToolTipInfo;

	CList<UINT,UINT>		m_lstNonSubclassedItems;
	CStringList				m_lstNonSubclassedWndClasses;

	CMap<UINT,UINT,CBCGPControlInfoTip, const CBCGPControlInfoTip&>	m_mapCtrlInfoTipDelayed;
	CMap<HWND,HWND,CBCGPControlInfoTip, const CBCGPControlInfoTip&>	m_mapCtrlInfoTip;
	HWND					m_hwndInfoTipCurr;

	CBCGPControlsLayout*	m_pLayout;
	MINMAXINFO				m_LayoutMMI;
	CRect					m_rectResizeBox;
	BOOL					m_bResizeBox;
	BOOL					m_bBackstageMode;
	BOOL					m_bLoadWindowPlacement;
	BOOL					m_bWindowPlacementIsSet;

	void AdjustShadow(BOOL bActive);
	CBCGPShadowManager*	m_pShadow;

	void OnSysCommand(UINT nID, LPARAM lParam);
};

/////////////////////////////////////////////////////////////////////////////
// CBCGPClosePopupDialogImpl

class BCGCBPRODLLEXPORT CBCGPClosePopupDialogImpl
{
public:
	virtual void ClosePopupDlg(LPCTSTR lpszEditValue, BOOL bOK, DWORD_PTR dwUserData = 0) = 0;
	
	virtual void OnDestroyPopupDlg() = 0;
	virtual CWnd* GetParentArea(CRect& rectParentBtn) = 0;
	virtual void GetParentText(CString& strEditValue) = 0;
	virtual void SetParentText(const CString& strEditValue) = 0;
};

class CBCGPEdit;
class CBCGPGridPopupDlgItem;

class BCGCBPRODLLEXPORT CBCGPParentEditPtr : public CBCGPClosePopupDialogImpl
{
	friend class CBCGPEdit;
	
public:
	CBCGPParentEditPtr (CBCGPEdit* pParentEdit) : m_pParentEdit (pParentEdit) {}
	
	virtual void ClosePopupDlg(LPCTSTR lpszEditValue, BOOL bOK, DWORD_PTR dwUserData = 0);
	virtual void OnDestroyPopupDlg();
	virtual CWnd* GetParentArea(CRect& rectParentBtn);
	virtual void GetParentText(CString& strEditValue);
	virtual void SetParentText(const CString& strEditValue);
	
protected:
	CBCGPEdit* m_pParentEdit;
};

#ifndef BCGP_EXCLUDE_GRID_CTRL

class CBCGPParentGridItemPtr : public CBCGPClosePopupDialogImpl
{
	friend class CBCGPGridPopupDlgItem;
	
public:
	CBCGPParentGridItemPtr (CBCGPGridPopupDlgItem* pParentItem) : m_pParentGridItem (pParentItem) {}
	
	virtual void ClosePopupDlg(LPCTSTR lpszEditValue, BOOL bOK, DWORD_PTR dwUserData = 0);
	virtual void OnDestroyPopupDlg();
	virtual CWnd* GetParentArea(CRect& rectParentBtn);
	virtual void GetParentText(CString& strEditValue);
	virtual void SetParentText(const CString& strEditValue);
	
protected:
	CBCGPGridPopupDlgItem* m_pParentGridItem;
};

#endif

class CBCGPParentMenuButtonPtr : public CBCGPClosePopupDialogImpl
{
	friend class CBCGPMenuButton;
	
public:
	CBCGPParentMenuButtonPtr(CBCGPMenuButton* pParentButton) : m_pParentMenuButton(pParentButton) {}
	
	virtual void ClosePopupDlg(LPCTSTR lpszEditValue, BOOL bOK, DWORD_PTR dwUserData = 0);
	virtual void OnDestroyPopupDlg();
	virtual CWnd* GetParentArea(CRect& rectParentBtn);
	virtual void GetParentText(CString& strEditValue);
	virtual void SetParentText(const CString& strEditValue);
	
protected:
	CBCGPMenuButton* m_pParentMenuButton;
};

#if (!defined _BCGSUITE_) && (!defined _BCGSUITE_INC_) && (!defined BCGP_EXCLUDE_RIBBON)

class BCGCBPRODLLEXPORT CBCGPParentRibbonButtonPtr : public CBCGPClosePopupDialogImpl
{
	friend class CBCGPRibbonButton;
	
public:
	CBCGPParentRibbonButtonPtr(CBCGPRibbonButton* pParentRibbonButton) : m_pParentRibbonButton (pParentRibbonButton) {}
	
	virtual void ClosePopupDlg(LPCTSTR lpszEditValue, BOOL bOK, DWORD_PTR dwUserData = 0);
	virtual void OnDestroyPopupDlg();
	virtual CWnd* GetParentArea(CRect& rectParentBtn);
	virtual void GetParentText(CString& strEditValue);
	virtual void SetParentText(const CString& strEditValue);
	
protected:
	CBCGPRibbonButton* m_pParentRibbonButton;
};

#endif

extern BCGCBPRODLLEXPORT UINT BCGM_ONSETCONTROLAERO;
extern BCGCBPRODLLEXPORT UINT BCGM_ONSETCONTROLVMMODE;
extern BCGCBPRODLLEXPORT UINT BCGM_ONSETCONTROLBACKSTAGEMODE;

#endif // !defined(AFX_BCGPDLGIMPL_H__18772215_4E74_4900_82E4_288CA46AB7E0__INCLUDED_)
