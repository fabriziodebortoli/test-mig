//*******************************************************************************
// COPYRIGHT NOTES
// ---------------
// This is a part of the BCGControlBar Library
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//
// BCGPComboBox.h : header file
//

#if !defined(AFX_BCGPCOMBOBOX_H__B809A10B_3085_419E_8ADA_6AA9A852CA73__INCLUDED_)
#define AFX_BCGPCOMBOBOX_H__B809A10B_3085_419E_8ADA_6AA9A852CA73__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "BCGCBPro.h"
#include "BCGPEdit.h"
#include "BCGPScrollBar.h"

/////////////////////////////////////////////////////////////////////////////
// CBCGPDropDownListBox window

class BCGCBPRODLLEXPORT CBCGPDropDownListBox :	public TBCGPInternalScrollBarWrapperWnd<CListBox>
{
	DECLARE_DYNAMIC(CBCGPDropDownListBox)

	friend class CBCGPComboBox;

	CBCGPDropDownListBox();

	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual void MeasureItem(LPMEASUREITEMSTRUCT lpMeasureItemStruct);

	afx_msg LRESULT OnPrintClient(WPARAM wp, LPARAM lp);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnNcPaint();
	DECLARE_MESSAGE_MAP()

public:
	CBCGPComboBox*	m_pWndCombo;
};

/////////////////////////////////////////////////////////////////////////////
// CBCGPComboBox window

class BCGCBPRODLLEXPORT CBCGPComboBox : public CComboBox
{
	friend class CBCGPEdit;

	DECLARE_DYNAMIC(CBCGPComboBox)

// Construction
public:
	CBCGPComboBox();

// Attributes
public:
	BOOL					m_bOnGlass;
	BOOL					m_bVisualManagerStyle;

protected:
	CBCGPEdit				m_wndEdit;
	CBCGPDropDownListBox	m_wndList;
	BOOL					m_bIsDroppedDown;
	CRect					m_rectBtn;
	BOOL					m_bIsButtonHighlighted;
	BOOL					m_bTracked;
	CString					m_strPrompt;
	COLORREF				m_clrPrompt;
	CString					m_strErrorMessage;
	COLORREF				m_clrErrorText;
	BOOL					m_bDefaultPrintClient;
	BOOL					m_bAutoComplete;

// Operations
public:
	void SetPrompt(LPCTSTR lpszPrompt, COLORREF clrText = (COLORREF)-1, BOOL bRedraw = TRUE);
	CString GetPrompt() const
	{
		return m_strPrompt;
	};

	void SetErrorMessage(LPCTSTR lpszMessage, COLORREF clrText = (COLORREF)-1, BOOL bRedraw = TRUE);
	CString GetErrorMessage() const
	{
		return m_strErrorMessage;
	}

	virtual BOOL IsThemedDropDownList() const;
	virtual int GetDropDownItemMinHeight() const;

	void EnableAutoComplete(BOOL bEnable = TRUE);	// For CBS_DROPDOWN style only
	BOOL IsAutoCompleteEnabled() const
	{
		return m_bAutoComplete;
	}

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CBCGPComboBox)
	public:
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual void MeasureItem(LPMEASUREITEMSTRUCT lpMeasureItemStruct);
	protected:
	virtual void PreSubclassWindow();
	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

	virtual void OnDraw(CDC* pDC, BOOL bDrawPrompt);
	virtual COLORREF OnFillLbItem(CDC* pDC, int nIndex, CRect rect, BOOL bIsHighlihted, BOOL bIsSelected);

	virtual int OnAutoComplete(const CString& strText, CString& strOut);

// Implementation
public:
	virtual ~CBCGPComboBox();

	// Generated message map functions
protected:
	//{{AFX_MSG(CBCGPComboBox)
	afx_msg void OnNcPaint();
	afx_msg void OnPaint();
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnCancelMode();
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnKillFocus(CWnd* pNewWnd);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnSetFocus(CWnd* pOldWnd);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG
	afx_msg BOOL OnEditupdate();
	afx_msg BOOL OnSelchange();
	afx_msg BOOL OnCloseup();
	afx_msg BOOL OnDropdown();
	afx_msg LRESULT OnBCGSetControlVMMode (WPARAM, LPARAM);
	afx_msg LRESULT OnBCGSetControlAero (WPARAM, LPARAM);
	afx_msg LRESULT OnSetText (WPARAM, LPARAM);
	afx_msg LRESULT OnPrintClient(WPARAM wp, LPARAM lp);
	afx_msg LRESULT OnRedrawFrame(WPARAM, LPARAM);
	afx_msg LRESULT OnChangeVisualManager (WPARAM, LPARAM);
	DECLARE_MESSAGE_MAP()

	void SubclassEditBox();
	void SubclassListBox();
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_BCGPCOMBOBOX_H__B809A10B_3085_419E_8ADA_6AA9A852CA73__INCLUDED_)
