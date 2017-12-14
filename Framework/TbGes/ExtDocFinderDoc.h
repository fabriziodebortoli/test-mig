
#pragma once

//------------------------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//==========================================================================================

class TB_EXPORT CFinderDoc : public CAbstractFormDoc
{
protected:
	DECLARE_DYNAMIC(CFinderDoc)

public:
	enum AttacchedDocumentManagement { NONE, AUXINFO, ANCESTOR };

protected:
	BOOL m_bStayAlive;
	AttacchedDocumentManagement m_bAttachedDocumentManagement;
	CAbstractFormDoc* m_pAttachedDocument;

public:
	CFinderDoc();

public:
	//Utilizzata nella FinderDoc delle Offerte fino a che non sparirà la classe
	void SetAttachedDocumentManagement(AttacchedDocumentManagement bAttachedDocumentManagement){m_bAttachedDocumentManagement = bAttachedDocumentManagement;}
	CAbstractFormDoc* GetAttachedDocument() const { return m_pAttachedDocument; }
	BOOL ChangeStayAlive();
	BOOL IsStayAlive()	{ return  m_bStayAlive; }

	virtual BOOL OnNewDocument	();
	virtual BOOL OnOpenDocument(LPCTSTR);
	virtual	void OnLoadAttachedDocument() = 0;
	virtual BOOL CanDoSaveRecord();

	virtual BOOL CanDoEditRecord	() { return FALSE; }
	virtual BOOL CanDoDeleteRecord	() { return FALSE; }
	virtual BOOL CanDoNewRecord		() { return FALSE; }

protected:
	virtual void ToolBarButtonsHideGhost(int nCase) {}

protected:
	//{{AFX_MSG(CFinderDoc)
	afx_msg void OnSaveRecord		();
	afx_msg void OnRequery			();
	afx_msg	void OnUpdateStayAlive	(CCmdUI*);
	afx_msg	void OnStayAlive		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CFinderFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CFinderFrame)

public:		//	Constructor
	CFinderFrame();

public:
	// reimplementata per cambiare lo standard behaviour, 
	// la toolbar e` diversa, contiene un bottone di StayAlive e meno cose

protected:
		virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);
		virtual BOOL OnCustomizeJsonToolBar();
	
protected:
	//{{AFX_MSG(CFinderFrame)
	afx_msg LRESULT OnChangeVisualManager(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
};

//==========================================================================================
#include "endh.dex"
