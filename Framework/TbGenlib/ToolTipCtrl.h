#pragma once

#include <TbGenlib\OslInfo.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGenlib\ParsCbx.h>
#include <TbGenlib\TBToolBar.h>
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBToolTipCtrl : public CBCGPToolTipCtrl
{
	DECLARE_DYNCREATE(CTBToolTipCtrl)

	public:
		CTBToolTipCtrl();
		virtual ~CTBToolTipCtrl();
		virtual BOOL OnDrawIcon (CDC* pDC, CRect rectImage);
		virtual CString GetLabel ();
		BOOL AddIcon(HICON hImg, UINT nID, LPCTSTR lpszText = NULL);
		void ShowIcons(BOOL bShow = TRUE);
		void SetCWND(CWnd* pWnd);

	private:
		CList<CIconList, CIconList&> m_iconsList;

		HICON GetIconById(UINT nID);
		CString GetTextById(UINT nID);

	// Attributes
	protected:
		CBCGPRibbonPanelMenuBar*	m_pParentMenuBar;
		CBCGPRibbonBar*				m_pParentRibbon;
		CWnd*						m_pParentCWND;
		UINT						m_nID;
		CString						m_TextToolTip;
		BOOL						m_bShowIcons;

	protected:
		//{{AFX_MSG(CRibbonTooltipCtrl)
		afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
		//}}AFX_MSG
		afx_msg void OnPaint();
		afx_msg void OnShow(NMHDR* pNMHDR, LRESULT* pResult);
		DECLARE_MESSAGE_MAP()
};
