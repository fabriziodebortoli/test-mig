
#pragma once

#include <tbgeneric\CISBitmap.h>
#include <tbgenlib\parsctrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define STRETCHCTRL_CLASSNAME _T("MFCStretchCtrl")  // Window class name
#define IXW_FSSTRETCHABLE_STRETCHBAR_HEIGHT 17

//-----------------------------------------------------------------------------
class TB_EXPORT CStretchCtrl : public CWnd
{
	DECLARE_DYNCREATE(CStretchCtrl)

protected:
	// grip
	CCISBitmap		m_Grip;
	CRect			m_GripRect;
	BOOL			m_bStretching;
	CWnd*			m_pLinkedWnd;
	CPoint			m_ptStartPos;

	// pushpin
	CCISBitmap		m_Pushpin;
	CCISBitmap		m_PushpinLocked;
	CCISBitmap		m_PushpinMouseOver;
	CRect			m_PushpinRect;
	BOOL			m_bPushpinLocked;
	BOOL			m_bMouseOver;

public:
	CStretchCtrl();
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

	void	AttachLinkedWnd(CWnd*	pWnd) { m_pLinkedWnd = pWnd; }
	CWnd*	GetLinkedWnd	() const { return m_pLinkedWnd; }
	BOOL	IsPushpinLocked	() const { return m_bPushpinLocked; }

protected:
			BOOL		RegisterWindowClass();

protected:
	//{{AFX
		afx_msg	void	OnLButtonDown		(UINT nFlags, CPoint point);
		afx_msg	void	OnLButtonUp			(UINT nFlags, CPoint point);
		afx_msg	void	OnMouseMove			(UINT nFlags, CPoint point);

		afx_msg void	OnPaint				( );
		afx_msg BOOL	OnSetCursor			( CWnd* pWnd, UINT nHitTest, UINT message );
	//}}AFX
	DECLARE_MESSAGE_MAP();
};

//-----------------------------------------------------------------------------
//			class CFSStretchableStrEdit
//-----------------------------------------------------------------------------
class TB_EXPORT CStretchableStrEdit : public CStrEdit
{
	DECLARE_DYNCREATE(CStretchableStrEdit);

protected:
	CStretchCtrl* m_pStretch;

public:
			CStretchableStrEdit		();
	virtual ~CStretchableStrEdit		();

	virtual	BOOL Create					(DWORD, const RECT&, CWnd*, UINT);
	virtual	BOOL SubclassEdit			(UINT, CWnd*, const CString& strName = _T(""));
	virtual BOOL ShowCtrl				(int nCmdShow);
	
	CStretchCtrl* GetStretch		() { return m_pStretch; }

protected:
			BOOL CreateAssociatedStretch(CWnd*);

protected:      
	afx_msg void OnWindowPosChanging	(WINDOWPOS FAR* lpwndpos);

	DECLARE_MESSAGE_MAP() 
};


#include "endh.dex"


   