
#pragma once

#include <TBGENLIB\BASEDOC.H>
#include <TBGENLIB\baseapp.h>


#include <TBGenlib\toolbarbutton.h>
#include <TBGES\ExtDocAbstract.h>
#include <TBGES\ExtDocView.h>
#include <TBGES\Tabber.h>

#include <XEngine\TBXMLEnvelope\EnvelopeTree.h>


//includere alla fine degli include del .H
#include "beginh.dex"

class CPreferencesCriteria;
class CXMLProfileInfo;
class CXMLExportDocSelection; 
class CAppExportCriteria;		
class COSLExportCriteria;		
class CUserExportCriteria;	
class CImpCriteriaWizardView;


/////////////////////////////////////////////////////////////////////////////
//		Dichiarazione di CAppImportCriteria
/////////////////////////////////////////////////////////////////////////////
//
class CAppImportCriteria
{
public:
	CAbstractFormDoc*		m_pDocument;
	CXMLBaseAppCriteria*	m_pBaseImportCriteria;		

public:
	CAppImportCriteria	(CAbstractFormDoc* pDocument);
	CAppImportCriteria	(const CAppImportCriteria&);
	~CAppImportCriteria	();	

private:
	void Assign(const CAppImportCriteria* pAppCriteria);

public:
	CXMLVariableArray*	GetVariablesArray			()			const	{ return m_pBaseImportCriteria ? m_pBaseImportCriteria->GetVariablesArray()			: NULL;}
	int					GetVariableArraySize		()			const	{ return GetVariablesArray() ? GetVariablesArray()->GetSize()	: 0;	}
	CXMLVariable*		GetVariable					(int nIdx)	const	{ return GetVariablesArray() ? GetVariablesArray()->GetAt(nIdx) : NULL;	}

	BOOL			CreateAppImpCriteriaTabDlgs	(CTabManager* pTabMng, int nPos) const { return m_pBaseImportCriteria ? m_pBaseImportCriteria->CreateAppExpCriteriaTabDlgs(pTabMng, nPos)	: NULL;	}
	UINT			GetFirstAppTabIDD			() const { return m_pBaseImportCriteria ? m_pBaseImportCriteria->GetFirstDialogIDD()			: 0;	}
	UINT			GetLastAppTabIDD			() const { return m_pBaseImportCriteria ? m_pBaseImportCriteria->GetLastDialogIDD()				: 0;	}

	
public:
	BOOL	Parse	(CXMLNode*, CAutoExpressionMng* = NULL);
	BOOL	Unparse	(CXMLNode*, CAutoExpressionMng* = NULL);

	// per gestire i parametri di import dallo scheduler
	BOOL SaveAppImportCriteria(const CString& strImpCriteriaName, CAutoExpressionMng* pAutoExpressionMng = NULL);
	BOOL LoadAppImportCriteria(const CString& strImpCriteriaName, CAutoExpressionMng* pAutoExpressionMng = NULL);


public: //operator
	CAppImportCriteria&	operator =	(const CAppImportCriteria&);
	BOOL				operator ==	(const CAppImportCriteria&)	const;
	BOOL				operator !=	(const CAppImportCriteria&)	const;
};


/////////////////////////////////////////////////////////////////////////////
//		CImpCriteriaWizardDoc Declaration
/////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CImpCriteriaWizardDoc : public CWizardFormDoc
{
	DECLARE_DYNCREATE(CImpCriteriaWizardDoc)

public:
	CXMLDataManager*	m_pDataManagerImport;
	CAppImportCriteria*	m_pAppImportCriteria;	

	CXMLEnvelopeManager* m_pEnvManager;
	DataBool			m_bDownload;
	DataBool			m_bPendingData;
	DataBool			m_bRXData;
	DataBool			m_bStdSearch;
	DataStr				m_strEnvelopePath;
	
	BOOL				m_EnvelopesDownloaded;
	BOOL				m_bIsTherePendingData;	

	CString				m_strLastError;
	CString				m_strDownloadSummary;
	DWORD				m_CurrentThread;
	CString				m_ImportPath;

	DataBool			m_bValidateOnParse;
	CXMLEnvElemArray*	m_pRXSelectedElems;

	DataBool			m_bTuningEnabled;

public:
	CImpCriteriaWizardDoc();
	~CImpCriteriaWizardDoc();

public:
	virtual BOOL OnOpenDocument		(LPCTSTR);
	virtual void OnCloseDocument	();
	virtual BOOL OnAttachData		();
	virtual CXMLBaseAppCriteria* GetBaseImportCriteria() const;

	CString GetEnvelopeClass		();
	CAbstractFormDoc* GetImportDocument	() const {return (CAbstractFormDoc*)m_pDataManagerImport->GetDocument();}
	BOOL IsTherePendingData			()		{return m_bIsTherePendingData;}
	CImpCriteriaWizardView*	GetWizardView (){return (CImpCriteriaWizardView*)GetFirstView();}
	BOOL IsValidEnvelope			(BOOL bCheckContents =FALSE);

	BOOL	CreateAppImpCriteriaTabDlgs	(CTabManager* pTabMng, int nPos) const { return (m_pAppImportCriteria) ? m_pAppImportCriteria->CreateAppImpCriteriaTabDlgs(pTabMng, nPos) : NULL;}
	UINT	GetFirstAppTabIDD			() const { return (m_pAppImportCriteria) ? m_pAppImportCriteria->GetFirstAppTabIDD(): 0;	}
	UINT	GetLastAppTabIDD			() const { return (m_pAppImportCriteria) ? m_pAppImportCriteria->GetLastAppTabIDD()	: 0;	}

	// per gestire i parametri di import dallo scheduler
	BOOL SaveAppImportCriteria(const CString& strImpCriteriaName, CAutoExpressionMng* pAutoExpressionMng = NULL) 
	{ return (m_pAppImportCriteria) ? m_pAppImportCriteria->SaveAppImportCriteria(strImpCriteriaName, pAutoExpressionMng) : TRUE; }
	BOOL LoadAppImportCriteria(const CString& strImpCriteriaName, CAutoExpressionMng* pAutoExpressionMng = NULL) 
	{ return (m_pAppImportCriteria) ? m_pAppImportCriteria->LoadAppImportCriteria(strImpCriteriaName, pAutoExpressionMng) : TRUE; }


};

//////////////////////////////////////////////////////////////////////////////////
// 
//		CImpCriteriaWizardFrame declaration
//
//////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CImpCriteriaWizardFrame : public CWizardFrame
{
	DECLARE_DYNCREATE(CImpCriteriaWizardFrame)

public:
	virtual void OnAdjustFrameSize(CSize& size) {}

};


/////////////////////////////////////////////////////////////////////////////
//	CImpCriteriaWizardView Declaration
/////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------
class CImpCriteriaWizardView : public CWizardFormView
{
	DECLARE_DYNCREATE(CImpCriteriaWizardView)

friend class CXMLImportWaitPage;
friend class CXMLImportProgressPage;

public:	
	CImpCriteriaWizardView()
		:CWizardFormView()
	{ 
		m_bEnableImage = FALSE;
		m_bUseOldButtonStyle = TRUE;
	}

private:
	CImpCriteriaWizardDoc* GetImpCriteriaWizardDoc() const;
	CAbstractFormDoc* GetImportDocument	();

public:
	virtual	void	CustomizeTabWizard		(CTabManager* pTabManager);
	virtual LRESULT	OnWizardNext			(UINT nIDD);

protected:
	//{{AFX_MSG(CExpCriteriaWizardView)             
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


// classe padre per le pagine del wizard dei criteri di importazione
/////////////////////////////////////////////////////////////////////////////
//		class CImportWizardPage definition
/////////////////////////////////////////////////////////////////////////////
class CImportWizardPage : public CWizardTabDialog
{
	DECLARE_DYNAMIC(CImportWizardPage)

protected:
	CXMLDataManager*		m_pXMLDataMng;

// Construction
public:
	CImportWizardPage(UINT, CWnd* = NULL, const CString& sName = _T(""));

public:
	CImpCriteriaWizardDoc*	GetWizardDoc()		const { return (CImpCriteriaWizardDoc*)GetDocument(); }
	CAbstractFormDoc*		GetImportDocument()	const { return (CAbstractFormDoc*)m_pXMLDataMng->GetDocument();}
			
	virtual BOOL	OnInitDialog	();
	virtual LRESULT OnWizardCancel ();
	
	DECLARE_MESSAGE_MAP()
};

// pagina per la selezione della modalità di importazione: effettuo il download
// o passo direttamente ad importare da file system

/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportPresentationPage definition
/////////////////////////////////////////////////////////////////////////////

class CXMLImportPresentationPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportPresentationPage)
	
	// Construction
public:
	CXMLImportPresentationPage(CWnd* = NULL);   // default constructor

private:
	void CheckAdjust ();

// Implementation
protected:
	virtual void	BuildDataControlLinks	();
	virtual LRESULT OnGetBitmapID			();
	virtual BOOL	OnInitDialog			();

	afx_msg void OnImportTuningChanged ();
	DECLARE_MESSAGE_MAP()

};

/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportStartPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLImportStartPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportStartPage)
	
// Construction
public:
	CXMLImportStartPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void BuildDataControlLinks		();
	virtual LRESULT OnGetBitmapID	();
	virtual LRESULT OnWizardNext	();

};


/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportWaitPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLImportWaitPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportWaitPage)
	
	BOOL	m_bFirstPaint;

// Construction
public:
	CXMLImportWaitPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void BuildDataControlLinks		();
	virtual LRESULT OnGetBitmapID	();
	virtual LRESULT OnWizardNext	();
	
	void			OnUpdateWizardButtons ();

public:	
	// Generated message map functions
	//{{AFX_MSG(CXMLImportWaitPage)
		afx_msg void OnPaint();
	//}}AFX_MSG	
	DECLARE_MESSAGE_MAP()
	afx_msg void OnSize(UINT nType, int cx, int cy);
};


/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportSelPendingPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLImportSelPendingPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportSelPendingPage)


// Construction
public:
	CXMLImportSelPendingPage(CWnd* = NULL);   // default constructor
	
	~CXMLImportSelPendingPage();


// Implementation
protected:
	virtual void BuildDataControlLinks		();
	virtual LRESULT OnGetBitmapID			();
};

/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportSelDocsPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLImportSelDocsPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportSelDocsPage)

	CRxEnvelopeTree*		m_pEnvTree;

// Construction
public:
	CXMLImportSelDocsPage(CWnd* = NULL);   // default constructor
	
	~CXMLImportSelDocsPage();

	virtual BOOL OnInitDialog();

// Implementation
protected:
	virtual LRESULT OnGetBitmapID			();
	LRESULT OnWizardNext					();

};

/////////////////////////////////////////////////////////////////////////////
//		class CXMLImportSummaryPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLImportSummaryPage : public CImportWizardPage
{
	DECLARE_DYNCREATE(CXMLImportSummaryPage)

// Construction
public:
	CXMLImportSummaryPage(CWnd* = NULL);   // default constructor
	
	~CXMLImportSummaryPage();

	virtual BOOL OnInitDialog();

// Implementation
protected:
	virtual void BuildDataControlLinks		();
	virtual LRESULT OnGetBitmapID			();
	LRESULT	OnWizardFinish					();
	void	FillSelection					();

};

/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"
