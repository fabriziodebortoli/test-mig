
#pragma once

#include <TbGeneric\Array.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbGeneric\dibitmap.h>
#include <TbGenlib\AddOnMng.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\TbExplorerInterface.h>
#include <TbGenlib\PARSLBX.H>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNameSpaces.h>
#include <TbGenlib\tbWizardMaster.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CManageFileWizMasterDlg;

/////////////////////////////////////////////////////////////////////////////
// CXRefWizardPage
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CManageFileWizardPage : public CWizardPage
{
	DECLARE_DYNAMIC(CManageFileWizardPage)

private:
	CTranspBmpCtrl	m_Image;

public:
	CManageFileWizardPage(UINT, CWnd* =NULL);
	~CManageFileWizardPage();

public:
	CManageFileWizMasterDlg*	m_pManageFileSheet;
	CToolTipCtrl*				m_pToolTip;
	
// Implementation
protected:
	virtual BOOL OnInitDialog();
	virtual BOOL OnCreatePage();
	
	// Generated message map functions
	//{{AFX_MSG(CXRefWizardPage)
	virtual BOOL PreTranslateMessage	(MSG* pMsg);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CManageWizPresentationPage dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CManageWizPresentationPage : public CManageFileWizardPage
{
	DECLARE_DYNCREATE(CManageWizPresentationPage)

public:
	CManageWizPresentationPage();

public:
	virtual void	OnActivate			();
	virtual	LRESULT OnWizardNext		();
	virtual void	DoDataExchange		(CDataExchange* pDX);    // DDX/DDV support

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CManageWizPresentationPage)
	afx_msg void OnSelInt();
	afx_msg void OnSelExt();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------------------
// CManageWizSearchPage dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CManageWizSearchPage : public CManageFileWizardPage
{
private:
	CBCGPComboBox	m_ComboObj;
	CTBListBox		m_ListSelected;
	CStringArray	m_aPaths;

public:	
	CManageWizSearchPage();   
	virtual ~CManageWizSearchPage();

private:
	void	FillComboObj	();
	void	FillList		();
	void	SelectExternObj	();
	void	SelectInternObj	();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    
    
protected:
	virtual	LRESULT OnWizardNext	();
	virtual BOOL	OnInitDialog	();
	virtual void	OnActivate		();
	virtual LRESULT OnWizardBack	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CManageWizSearchPage)
	afx_msg	void	OnBtnSelClicked		();
	afx_msg	void	OnComboObjChanged	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class CItemNoLocSaveDlg : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;

public: 
	CItemNoLocSaveDlg (CString strName);
};

//----------------------------------------------------------------------------------
// CManageWizSavePage dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CManageWizSavePage : public CManageFileWizardPage
{
private:
	private:
	CBCGPComboBox				m_ComboApp;
	CTBListBox					m_ListMod;
	CTBListBox					m_ListUsr;
	Array						m_arAppItemLoc;
	Array						m_arModItemLoc;

public:	
	CManageWizSavePage();   
	virtual ~CManageWizSavePage();

private:
	void	GetUserForSave	();

	void	FillComboApp();
	void	FillListMod	(bool bFirst = FALSE);
	void	FillListUsr	();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    
  
protected:
	virtual	LRESULT OnWizardNext	();
	virtual BOOL	OnInitDialog	();
	virtual void	OnActivate		();
	virtual LRESULT OnWizardBack	();

protected:
//{{AFX_MSGCTBManageFileCopyDlg)
	afx_msg	void	OnBtnSelClicked		();
	afx_msg	void	OnComboAppChanged	();
	afx_msg	void	OnListModsChanged	();
	afx_msg void	OnSelAllUsr			();
	afx_msg void	OnSelUsr			();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CManageWizFinishPage dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CManageWizFinishPage : public CManageFileWizardPage
{
	DECLARE_DYNCREATE(CManageWizFinishPage)
private:
	CTBListBox	m_ListSelected;
public:
	CManageWizFinishPage();

private:
	void FillListPath	();

public:
	virtual void	OnActivate			();
	virtual LRESULT OnWizardBack		();
	virtual void	DoDataExchange		(CDataExchange* pDX);    // DDX/DDV support

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CManageWizPresentationPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CManageFileWizMasterDlg dialog
//---------------------------------------------------------------------------
class TB_EXPORT CManageFileWizMasterDlg : public CWizardMasterDialog
{
	DECLARE_DYNAMIC(CManageFileWizMasterDlg)
	
public:
	CManageWizPresentationPage	m_ManageWizPresentationPage;
	CManageWizSearchPage		m_ManageSearchPage;
	CManageWizSavePage			m_ManageSavePage;
	CManageWizFinishPage		m_ManageFinishPage;

	BOOL						m_bExternalCopy;
	BOOL						m_bAllUsers;
	CStringArray				m_aPaths;
	CStringArray				m_aUsrForSave;
	CTBNamespace::NSObjectType	m_Type;
	CTBNamespace				m_Namespace;

// Construction
public:
	CManageFileWizMasterDlg(CWnd* pParent = NULL);

private:
	int	ImportCopyFile	(const CString& strPath, const CString& strUsr, CStringArray* aMsg);

public:
	virtual BOOL OnWizardFinish	();

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CProfileWizMasterDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

