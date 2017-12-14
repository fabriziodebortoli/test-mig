#pragma once

#include "parsobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CWizardMasterDialog;
/////////////////////////////////////////////////////////////////////////////
// CWizardPage dialog

class TB_EXPORT CWizardPage : public CParsedDialog
{
	friend class CWizardMasterDialog;
	
	DECLARE_DYNAMIC(CWizardPage)

protected:
	CBrush					m_brSolidWhite; // brush for white background
	CWizardMasterDialog*	m_pParent;		// Parent dialog

private:
	BOOL m_bCreated;	// flag to tell us if the dialog has been created
	BOOL m_bActive;		// flag to tell is if the dialog is the active page
	UINT m_nDialogID;	// resource ID for the page

// Construction
public:
	CWizardPage(UINT, CWnd* = NULL);
	virtual ~CWizardPage();

// Operations
public:
	virtual BOOL OnCreatePage()	{ return TRUE;}
	virtual void OnDestroyPage(){};

	// these functions are the same as CPropertyPage
	virtual void	OnCancel		() {}
	virtual void	OnActivate		() { m_bActive = TRUE; }
	virtual BOOL	OnDeactivate	() { return TRUE;}
	virtual BOOL	OnQueryCancel	() { return TRUE;}
	virtual LRESULT OnWizardBack	() { return 0;}
	virtual LRESULT OnWizardNext	() { return 0;}
	virtual BOOL	OnWizardFinish	() { return TRUE;}

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CWizardPage)
	virtual BOOL	OnInitDialog();
	virtual BOOL	OnCmdMsg	(UINT, int, void*, AFX_CMDHANDLERINFO*);
	afx_msg HBRUSH	OnCtlColor	(CDC*, CWnd*, UINT);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CWizardMasterDialog dialog
class TB_EXPORT CWizardMasterDialog : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CWizardMasterDialog)

protected:
	UINT	m_nDlgID;
	CWnd*	m_pParent;
	UINT	m_nPlaceholderID;	
	CObList	m_PageList;		// list of CWizardPage objects

// Construction
public:
	CWizardMasterDialog (UINT, CWnd* pParent = NULL);
	virtual ~CWizardMasterDialog ();

// Operations
public:
	void	AddPage(CWizardPage*);

	void	SetActivePageByResource(UINT);
	
	BOOL	SetFirstPage();
	void	SetNextPage	();
	
	void	SetTitle(LPCTSTR);
	void	SetTitle(UINT);
	
	void	SetFinishBtnText(LPCTSTR);
	void	SetFinishBtnText(UINT);

	void	EnableFinishBtn	(BOOL = TRUE, BOOL bSetFocus = FALSE);
	void	EnableBackBtn	(BOOL = TRUE, BOOL bSetFocus = FALSE);
	void	EnableNextBtn	(BOOL = TRUE, BOOL bSetFocus = FALSE);

	
	int				GetActiveIndex	() const;
	int				GetPageIndex	(CWizardPage*) const;
	int				GetPageCount	() const;
	CWizardPage*	GetPage			(int) const;
	BOOL			SetActivePage	(int);
	BOOL			SetActivePage	(CWizardPage*);
	
protected:
	BOOL			DeactivateCurrentPage	();
	void			SetPlaceholderID		(int);
	CWizardPage*	GetPageByResourceID		(UINT);

	virtual BOOL	OnQueryCancel	() { return TRUE;}
	virtual BOOL	OnWizardFinish	() { return TRUE;}

private:
  void			DestroyPage		(CWizardPage* pPage);
  CWizardPage*	GetFirstPage	() const;
  CWizardPage*	GetLastPage		() const;
  CWizardPage*	GetActivePage	() const;
  CWizardPage*	GetNextPage		() const;


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CWizardMasterDialog)
	protected:
	virtual BOOL OnInitDialog	();
	virtual void OnCancel		();
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CWizardMasterDialog)
	afx_msg void OnDestroy					();
	afx_msg void OnWizardFinishBtnClicked	();
	afx_msg void OnWizardBackBtnClicked		(); 
	afx_msg void OnWizardNextBtnClicked		(); 
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//=============================================================================
#include "endh.dex"
