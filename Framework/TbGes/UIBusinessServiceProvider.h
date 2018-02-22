
#pragma once

#include "extdoc.h"
#include "BusinessServiceProvider.h"

#include "beginh.dex"

// BusinessServiceProvider - componenti per User Interface
//
// Questo Document, Frame e View sono studiati per fornire UI ad un BSP
// Nel caso standard, e' sufficente derivare dalla View la propria view specializzata,
// utilizzando il Document ed il Frame predefiniti
//

class CBusinessServiceProviderView;
class CBusinessServiceProviderFrame;
class CBusinessServiceProviderClientDoc;
class CBusinessServiceProviderObj;
/////////////////////////////////////////////////////////////////////////////
//					class CBusinessServiceProviderDoc	definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBusinessServiceProviderDoc : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(CBusinessServiceProviderDoc)

	friend class CBusinessServiceProviderView;
	friend class CBusinessServiceProviderFrame;

public:
	CBusinessServiceProviderDoc();

protected:
	virtual BOOL OnAttachData		()			{ return TRUE; }
	virtual BOOL OnOpenDocument		(LPCTSTR);
	virtual void OnCloseDocument	();
	virtual void GetDataSource		(CString sDataManager, CString sField, DBTObject*& pDbt, SqlRecord*& pRecord, DataObj*& pField, bool& isVirtual);
	virtual void OnPrepareForFind	(HotKeyLinkObj* pHKL, SqlRecord* pRec);

public:
	CBusinessServiceProviderObj*	GetBSP()	{ return m_pBSP; }

private:
	void UpdatePinStatus();
	TDisposablePtr<CBusinessServiceProviderObj>	m_pBSP;
public:
	//{{AFX_MSG(CBusinessServiceProviderDoc)
		afx_msg void	OnAlwaysOnFront			();
		afx_msg void	OnSwitchToCaller		();
		afx_msg void 	OnUpdateAlwaysOnFront	(CCmdUI*);
		afx_msg void 	OnUpdateSwitchToCaller	(CCmdUI*);
	//}}AFX_MSG
	
	DECLARE_MESSAGE_MAP()
};

// QUESTA CLASSE NON VIENE USATA PER I DOCUMENTI JSON, USARE CJsonFormView
//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderView definition				//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBusinessServiceProviderView : public CMasterFormView
{
	DECLARE_DYNAMIC(CBusinessServiceProviderView)

protected:	
	CBusinessServiceProviderView
		(
			const	CString&	strName, 
					UINT		nIDTemplate,
			const	CString&	strTitle
		);

protected:
	virtual void OnInitialUpdate();

public:
	CBusinessServiceProviderObj*	GetBSP()	{ return ((CBusinessServiceProviderDoc*)GetDocument())->GetBSP(); }

private:
	CString		m_strTitle;
};

/////////////////////////////////////////////////////////////////////////////
//			class CBusinessServiceProviderJSonView definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CBusinessServiceProviderJSonView : public CJsonFormView
{
	DECLARE_DYNCREATE(CBusinessServiceProviderJSonView)

	CBusinessServiceProviderJSonView() {}
	CBusinessServiceProviderJSonView(UINT nIDTemplate);

public:
	CBusinessServiceProviderObj*	GetBSP(const CString& pNamespace);
};

/////////////////////////////////////////////////////////////////////////////
//			class CBusinessServiceProviderPaneView definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CBusinessServiceProviderPaneView : public CAbstractFormView
{
	friend class CBusinessServiceProviderObj;

	DECLARE_DYNCREATE(CBusinessServiceProviderPaneView)

	CBusinessServiceProviderPaneView
		(
			const	CString&	strName, 
					UINT		nIDTemplate
		);

public:
	CBusinessServiceProviderObj*	GetBSP()	{ return m_pBSP; }
	virtual void OnAttachContext(CWnd* pParent, CObject* pContext);

private:
	void AttachBSP(CBusinessServiceProviderObj* pBSP) { /*if (pBSP) { ASSERT_VALID(pBSP); }*/ m_pBSP = pBSP;}

private:
	TDisposablePtr<CBusinessServiceProviderObj>	m_pBSP;
};

/////////////////////////////////////////////////////////////////////////////
//			class CBusinessServiceProviderDockPane definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CBusinessServiceProviderDockPane : public CTaskBuilderDockPane
{
	friend class CBusinessServiceProviderObj;
	friend class CBusinessServiceProviderClientDoc;

	DECLARE_DYNCREATE(CBusinessServiceProviderDockPane);

public:
	CBusinessServiceProviderDockPane	();
	CBusinessServiceProviderDockPane	(CRuntimeClass* pWndClass, CString sTabPaneTitle =_T(""));

public:
	CBusinessServiceProviderObj*	GetBSP()	{ return m_pBSP; }

private:
	void		AttachBSP(CBusinessServiceProviderObj* pBSP);
private:
	TDisposablePtr<CBusinessServiceProviderObj>	m_pBSP;
	CUIntArray*									m_pIDTabArray;

protected:	
	virtual void	OnSlide					(BOOL bSlideOut);
	virtual BOOL	CheckAutoHideCondition	();
	virtual void	OnTabAdded				(UINT nTabID);
};

#define BDC(m)	NULL, &(GetBSP()->m)

//////////////////////////////////////////////////////////////////////////////
//             class CBusinessServiceProviderFrame definition				//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBusinessServiceProviderFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CBusinessServiceProviderFrame)

public:		
	CBusinessServiceProviderFrame();

public:		
	// reimplementate per cambiare lo standard behaviour, la toolbar standard e la status bar non ci sono
	virtual BOOL CreateStatusBar();

protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);
	//{{AFX_MSG(CBusinessServiceProviderFrame)
	afx_msg void OnClose();
	afx_msg void OnSize(UINT nType, int cx, int cy);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//						class CBSPWithBottomToolbarFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBSPWithBottomToolbarFrame : public CBusinessServiceProviderFrame
{
	DECLARE_DYNCREATE(CBSPWithBottomToolbarFrame)

public:
	CBSPWithBottomToolbarFrame	();
	~CBSPWithBottomToolbarFrame	();

protected:
	CTBToolBar*		m_pBottomToolbar = NULL;
	CTBTabbedToolbar* m_pBottomTabbedToolbar = NULL;
public:
	virtual BOOL	CreateAuxObjects	(CCreateContext* pCreateContext);
	virtual void	AddCustomButtons	() {}

	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//				class CBSPOkFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBSPOkFrame : public CBSPWithBottomToolbarFrame
{
	DECLARE_DYNCREATE(CBSPOkFrame)

protected:
	virtual void	AddCustomButtons	();
	virtual void	OnAdjustFrameSize	(CSize& size);
};

#include "endh.dex"
