
#pragma once

#include <TBGENLIB\BASEDOC.H>
#include "ExpCriteriaObj.h"


//includere alla fine degli include del .H
#include "beginh.dex"

class CPreferencesCriteria;
class CXMLProfileInfo;
class CXMLExportDocSelection; 
class CAppExportCriteria;		
class COSLExportCriteria;		
class CUserExportCriteria;	
class CTabManager;
 

//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardFrame declaration
//
//////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CExpCriteriaWizardFrame : public CWizardFrame
{
	DECLARE_DYNCREATE(CExpCriteriaWizardFrame)

public:
	virtual void OnAdjustFrameSize(CSize& size) {}

};


//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardDoc declaration
//
//////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CExpCriteriaWizardDoc : public CWizardFormDoc
{
	DECLARE_DYNCREATE(CExpCriteriaWizardDoc)

public:
	CPreferencesCriteria*	m_pPreferences;
	CXMLExportCriteria*		m_pExportCriteria;
	CXMLExportDocSelection*	m_pXMLExportDocSel;
	CXMLProfileInfo*		m_pCurrentProfile;
	
	CXMLDataManager*		m_pDataManagerExport;

	DataBool				m_bSaveCriteria;
	BOOL					m_bIsInputMngToInit;

public:
	CExpCriteriaWizardDoc();
	~CExpCriteriaWizardDoc(){m_pBaseExportCriteria = NULL;}

public:
	virtual BOOL OnOpenDocument(LPCTSTR);
	virtual CXMLBaseAppCriteria* GetBaseExportCriteria() const;
	virtual BOOL OnAttachData		();
	virtual HotKeyLink*	GetHotLink(const CString& sName, const CTBNamespace& aNameSpace /*= CTBNamespace(_T(""))*/);
public:
	CAbstractFormDoc*		GetExportedDocument			() const; 	
	void					UpdateCurrentProfile		();
	CXMLExportCriteria*		AllocNewExportCriteria		();
	BOOL					CheckDate					() const;
	BOOL					IsTracedDBTMasterTable		() const;

	CXMLExportDocSelection* GetXMLExportDocSel			() const { return m_pXMLExportDocSel;}
	CXMLExportCriteria*		GetXMLExportCriteria		() const { return m_pExportCriteria;}	
	CPreferencesCriteria*	GetPreferencesCriteria		() const { return m_pExportCriteria ? m_pExportCriteria->GetPreferencesCriteria() : NULL;}	
	CAppExportCriteria*		GetAppExportCriteria		() const { return m_pExportCriteria ? m_pExportCriteria->GetAppExportCriteria() : NULL; }
	COSLExportCriteria*		GetOSLExportCriteria		() const { return m_pExportCriteria ? m_pExportCriteria->GetOSLExportCriteria() : NULL;	}
	CUserExportCriteria*	GetUserExportCriteria		() const { return m_pExportCriteria ? m_pExportCriteria->GetUserExportCriteria() : NULL; }

	BOOL			CreateAppExpCriteriaTabDlgs	(CTabManager* pTabMng, int nPos) const { return (m_pExportCriteria && m_pExportCriteria->m_pAppExportCriteria) ? m_pExportCriteria->m_pAppExportCriteria->CreateAppExpCriteriaTabDlgs(pTabMng, nPos) : NULL;}
	UINT			GetFirstAppTabIDD			() const { return (m_pExportCriteria && m_pExportCriteria->m_pAppExportCriteria) ? m_pExportCriteria->m_pAppExportCriteria->GetFirstAppTabIDD()	: 0;	}
	UINT			GetLastAppTabIDD			() const { return (m_pExportCriteria && m_pExportCriteria->m_pAppExportCriteria) ? m_pExportCriteria->m_pAppExportCriteria->GetLastAppTabIDD()	: 0;	}
};



//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardView declaration
//
//////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CExpCriteriaWizardView : public CWizardFormView
{
	DECLARE_DYNCREATE(CExpCriteriaWizardView)

public:	
	CExpCriteriaWizardView(){ m_bUseOldButtonStyle = TRUE; }

private:
	CExpCriteriaWizardDoc* GetExpCriteriaWizardDoc	() const;

	BOOL	CheckViewPage							(UINT nIDD);
	LRESULT OnSelProfilePageWizardNext				();
	BOOL	ExistOneCriteria						();
	void	BindingVariable							();
	
	LRESULT OnSelCriteriaPageWizardNext				();

public:
	virtual	void	CustomizeTabWizard				(CTabManager* pTabManager);

	virtual LRESULT	OnWizardNext					(UINT nIDD);

	CXMLExportDocSelection* GetXMLExportDocSel		() const { CExpCriteriaWizardDoc* pDoc = GetExpCriteriaWizardDoc(); return pDoc ? pDoc->GetXMLExportDocSel() : NULL;}
	CAbstractFormDoc*		GetExportedDocument		() const { CExpCriteriaWizardDoc* pDoc = GetExpCriteriaWizardDoc(); return pDoc ? pDoc->GetExportedDocument() : NULL;} 	

protected:
	//{{AFX_MSG(CExpCriteriaWizardView)             
		afx_msg	void OnWizardFinish					();
		afx_msg	void OnWizardCancel					();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
