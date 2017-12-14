
#pragma once

#include <TBGENLIB\basedoc.h>
#include <TBGENLIB\parsedt.h>
#include <TBGES\tabber.h>
#include <TBGES\extdoc.h>
#include <TBGES\extdocview.h>

#include <TBGES\XMLControls.h>

#include "xmldatamng.h"


//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLDataManager;
class CExpCriteriaWizardDoc;
class InputMng;

// classe padre per le pagine del wizard dei criteri di esportazione
/////////////////////////////////////////////////////////////////////////////
//		class CExportWizardPage definition
/////////////////////////////////////////////////////////////////////////////
class CExportWizardPage : public CWizardTabDialog
{
	DECLARE_DYNAMIC(CExportWizardPage)

protected:
	CXMLDataManager*		m_pXMLDataMng;
	CXMLExportDocSelection*	m_pXMLExportDocSel;

// Construction
public:
	CExportWizardPage(UINT, CWnd* = NULL, const CString& sName = _T(""));

public:
	CExpCriteriaWizardDoc*	GetWizardDoc()		const { return (CExpCriteriaWizardDoc*)GetDocument(); }
	CAbstractFormDoc*		GetExportDocument()	const { return (CAbstractFormDoc*)m_pXMLDataMng->GetDocument();}
			
public:	
	// Generated message map functions
	//{{AFX_MSG(CExportWizardPage)
		virtual BOOL	OnInitDialog	();
		virtual LRESULT OnWizardCancel	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportPresentationPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportPresentationPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportPresentationPage)

// Construction
public:
	CXMLExportPresentationPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual LRESULT OnGetBitmapID	();

public:	
	// Generated message map functions
	//{{AFX_MSG(CXMLExportPresentationPage)
	//}}AFX_MSG
};

//pagina per la selezione delle opzioni relative ai file di schema
// esporto solo i file di dati, solo i file di schema, entrambi
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportSchemaPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportSchemaPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportSchemaPage)

// Construction
public:
	CXMLExportSchemaPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual LRESULT OnGetBitmapID	();
	virtual void BuildDataControlLinks		();
	virtual	LRESULT OnWizardNext	();

public:	
	// Generated message map functions
	//{{AFX_MSG(CXMLExportSchemaPage)
	//}}AFX_MSG
};

// pagina per la selezione della modalità di esportazione: il documento corrente oppure
// un insieme di documenti che verificano le regole di richiesta
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportSelDocPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportSelDocPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportSelDocPage)

// Construction
public:
	CXMLExportSelDocPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void BuildDataControlLinks		();
	
	virtual LRESULT OnGetBitmapID	();

	virtual	LRESULT OnWizardNext	();
};

// pagina per la selezione del profilo
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportSelProfilePage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportSelProfilePage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportSelProfilePage)
// Construction
public:
	CXMLExportSelProfilePage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void	BuildDataControlLinks		();
	virtual void	OnDisableControlsForBatch	();
	virtual LRESULT OnWizardNext				();
	virtual LRESULT OnGetBitmapID				();

	// Generated message map functions
	//{{AFX_MSG(CXMLExportSelProfilePage)
	afx_msg void OnSelDefaultProfileChanged	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};



// pagina per la selezione dei tre tipi di criteri di esportazione.
// Predefiniti
// Personalizzati
// OSL
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportCriteriaPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportCriteriaPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportCriteriaPage)


// Construction
public:
	CXMLExportCriteriaPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void	BuildDataControlLinks	();
	virtual void	OnDisableControlsForBatch();

	virtual LRESULT OnGetBitmapID	();

	// Generated message map functions
	//{{AFX_MSG(CXMLExportCriteriaPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


// pagina contenente le regole di esportazione di OSL
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportOSLCriteriaPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportOSLCriteriaPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportOSLCriteriaPage)


// Construction
public:
	CXMLExportOSLCriteriaPage(CWnd* = NULL);   // default constructor

// Implementation
protected:
	virtual void	BuildDataControlLinks();

	virtual LRESULT OnGetBitmapID	();
	virtual LRESULT OnWizardBack	();


	// Generated message map functions
	//{{AFX_MSG(CXMLExportOSLCriteriaPage)
	afx_msg void OnOSLFromDateChanged	();
	afx_msg void OnOSLToDateChanged		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

// pagina contenente le regole di richiesta create dall'utente e collegate al profilo
// prescelto
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportUserCriteriaPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportUserCriteriaPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportUserCriteriaPage)

private:
	InputMng*	m_pInputMng;

// Construction
public:
	CXMLExportUserCriteriaPage	(CWnd* = NULL);   // default constructor
	~CXMLExportUserCriteriaPage	();

// Implementation
protected:
	virtual LRESULT OnGetBitmapID	();
	virtual LRESULT OnWizardNext	();
	virtual	BOOL	OnCommand		(WPARAM wParam, LPARAM lParam);

	// Generated message map functions
	//{{AFX_MSG(CXMLExportUserCriteriaPage)
	afx_msg void	OnSize				(UINT nType, int cx, int cy);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

// pagina di opzioni di esportazione. 
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportOptionPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportOptionPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportOptionPage)

// Construction
public:
	CXMLExportOptionPage(CWnd* = NULL);   // default constructor

public:
	virtual void	BuildDataControlLinks();
	virtual void	OnDisableControlsForBatch();
public:	
	// Generated message map functions
	//{{AFX_MSG(CXMLExportOptionPage)
		afx_msg void OnUsingPathChanged		();
		afx_msg void OnSelectExportPath		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

// pagina di riepilogo delle selezioni effettuate dall'utente. 
// E' solo su questa pagina che viene abilitato il pulsante di Finish
/////////////////////////////////////////////////////////////////////////////
//		class CXMLExportSummaryPage definition
/////////////////////////////////////////////////////////////////////////////
class CXMLExportSummaryPage : public CExportWizardPage
{
	DECLARE_DYNCREATE(CXMLExportSummaryPage)

// Construction
public:
	CXMLExportSummaryPage(CWnd* = NULL);   // default constructor

public:
	virtual void	BuildDataControlLinks();

// Implementation
protected:
	virtual LRESULT	OnWizardFinish	();

public:	
	// Generated message map functions
	//{{AFX_MSG(CXMLExportSummaryPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"


