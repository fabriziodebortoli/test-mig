//===========================================================================
// module name  : ParsedPanel.H
// author       :
// description  : controls container to be placed inside FormViews
// Copyright (c) MicroArea S.p.A. All rights reserved
//===========================================================================

#pragma once

#include <TbGenlib\Parsobj.h>

#include "extdoc.h"

#include "beginh.dex"

class CPanelContainer;
class HotKeyLink;

//===========================================================================
class TB_EXPORT CParsedPanel : public CParsedDialog
{
	DECLARE_DYNAMIC(CParsedPanel)

public :
	CParsedPanel (const CString& sName, UINT nIDD=0);
	virtual ~CParsedPanel ();

	virtual BOOL Create	(UINT nIDC, LPCTSTR lpszCaption, CWnd* pParentWnd);

	// show the panel caption in the nIDC control (usually a groupbox or a static label) 
	void ShowCaption(UINT nIDC);

	// must be reimplemented in the deived class, and the owner type may differ according to
	// implementation (i.e.: hotfilter, ...)
	virtual void AttachOwner(CObject* pOwner)	=	0;
	
	virtual void	OnOK					()	{ /* MUST do nothing*/ }
	virtual void	OnCancel				()	{ /* MUST do nothing*/ }
	virtual BOOL	OnInitDialog			();

	virtual void	PostNcDestroy			();
	virtual	BOOL	OnCommand				(WPARAM wParam, LPARAM lParam);
	virtual	BOOL	PreTranslateMessage		(MSG* pMsg);

	virtual	void EnableControlLinks (BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

	CAbstractFormDoc*	GetDocument() const	{ return (CAbstractFormDoc*)__super::GetDocument(); }

	virtual void	OnDisableControlsForBatch	() {/* do nothing*/}
	virtual void	OnDisableControlsForAddNew	() {/* do nothing*/}
	virtual void	OnDisableControlsForEdit	() {/* do nothing*/}
	virtual void	OnEnableControlsForFind		() {/* do nothing*/}
	virtual void	OnDisableControlsAlways		() {/* do nothing*/}
	virtual BOOL	OnPrepareAuxData			() { return TRUE; }

	virtual BOOL	PrepareAuxData				();

	virtual	void	BuildDataControlLinks	() {/* to be implemented in the derived classes */ }

	virtual CParsedCtrl*	AddLink
		(
					UINT			nIDC, 
			const	CString&		sName,
					SqlRecord*		pRecord, 
					DataObj*		pDataObj, 
					CRuntimeClass*	pParsedCtrlClass,

					HotKeyLink*		pHotKeyLink			= NULL,
					UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CExtButton*		AddLink
		(
					UINT		nIDC, 
			const	CString&	sName,
					SqlRecord*	pRecord	= NULL, 
					DataObj*	pDataObj = NULL
		);

	virtual CParsedPanel*	AddLink
		(
					UINT			nIDC, 
					CRuntimeClass*	pParsedPanelClass, 
					CObject*		pPanelOwner,
			const	CString&		sName, 
			const	CString&		sCaption = _T(""), 
					BOOL			bCallOnInitialUpdate = TRUE
		);

protected:
	// By default the panel fits the size of its placeholder
	// Setting these flags to false will fit the placeholder to the panel instead
	// (i.e.: to make room for other controls)
	BOOL	m_bFitHeight;
	BOOL	m_bFitWidth;

private:
	void BatchEnableControls	();
	void EnableControls			();
	BOOL IsCtrlBefore			(int nBefore, int nAfter);

private:
	COLORREF			m_colorDlg;
	CBrush				m_brushDlg;
	CPanelContainer*	m_pContainer;
	CString				m_Caption;

protected:
	//{{AFX_MSG(CParsedDialog)
	afx_msg HBRUSH	OnCtlColor		(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


#include "endh.dex"
