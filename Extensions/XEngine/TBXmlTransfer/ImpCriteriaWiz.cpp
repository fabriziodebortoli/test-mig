#include "stdafx.h" 

#include <TBGES\DBT.H>
#include <TBGES\XMLGesInfo.h>
#include <TBGES\ExtDocView.h>
#include <TBGES\Tabber.h>

#include <TBGENLIB\baseapp.h>
#include <TBgeneric\globals.h>

#include <tbwebserviceswrappers\loginmanagerinterface.h>

#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include <XEngine\TBXMLEnvelope\TXEParameters.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "XMLDataMng.h"
#include "ImpCriteriaWiz.h"
#include "ImpCriteriaWiz.hjson"
#include "XMLTransferTags.h"
#include "GenFunc.h"

#define CRLF _T("\r\n")

//----------------------------------------------------------------------------
//	Class CAppImportCriteria implementation
//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
CAppImportCriteria::CAppImportCriteria(CAbstractFormDoc* pDocument)
	:
	m_pDocument				(pDocument),
	m_pBaseImportCriteria	(NULL)
{
	
	// se sono nel caso di solo editing del profilo e non sono in fase di esportazione
	// ne istanzio uno pulito per ricevere e salvare le info relative alle variabili eventualmente
	// memorizzate nel file xml
	if (!m_pDocument)
		m_pBaseImportCriteria = new CXMLBaseAppCriteria;
	else
		m_pBaseImportCriteria = m_pDocument->GetBaseImportCriteria();
}

//----------------------------------------------------------------------------
CAppImportCriteria::CAppImportCriteria(const CAppImportCriteria& AppCriteria)
:
	m_pDocument				(NULL),
	m_pBaseImportCriteria	(NULL)
{
	Assign(&AppCriteria);
}

//----------------------------------------------------------------------------
CAppImportCriteria::~CAppImportCriteria()
{
	if (!m_pDocument && m_pBaseImportCriteria)
		delete m_pBaseImportCriteria;
}

//----------------------------------------------------------------------------
void CAppImportCriteria::Assign(const CAppImportCriteria* pAppCriteria)
{
	if (!m_pDocument && m_pBaseImportCriteria)
		delete m_pBaseImportCriteria;
	m_pBaseImportCriteria = NULL;

	m_pDocument	= pAppCriteria->m_pDocument;			

	if (!m_pDocument)
	{
		m_pBaseImportCriteria = new CXMLBaseAppCriteria;
		*m_pBaseImportCriteria = *pAppCriteria->m_pBaseImportCriteria;
	}
	else
		m_pBaseImportCriteria = m_pDocument->GetBaseImportCriteria();
}

//----------------------------------------------------------------------------------------------
BOOL CAppImportCriteria::Parse(CXMLNode* pXMLNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/) 
{ 
	return (m_pBaseImportCriteria) ? m_pBaseImportCriteria->Parse(pXMLNode, pAutoExpressionMng) : TRUE;	
}

//----------------------------------------------------------------------------------------------
BOOL CAppImportCriteria::Unparse(CXMLNode* pXMLNode, CAutoExpressionMng* pAutoExpressionMng /*= NULL*/) 
{
	return (m_pBaseImportCriteria) ? m_pBaseImportCriteria->Unparse(pXMLNode, pAutoExpressionMng) : TRUE;	
}

//----------------------------------------------------------------------------------------------
CAppImportCriteria& CAppImportCriteria::operator =(const CAppImportCriteria& aAppCriteria)
{
	if (this == &aAppCriteria)
		return *this;
	
	Assign(&aAppCriteria);
	
	return *this;
}

//------------------------------------------------------------------------------
BOOL CAppImportCriteria::operator == (const CAppImportCriteria& aAppCriteria) const
{
	if (this == &aAppCriteria)
		return TRUE;
	
	return
		(
			m_pDocument				== aAppCriteria.m_pDocument	&&
			m_pBaseImportCriteria	== aAppCriteria.m_pBaseImportCriteria
		);
}

//------------------------------------------------------------------------------
BOOL CAppImportCriteria::operator != (const CAppImportCriteria& aAppCriteria) const
{
	return !(*this == aAppCriteria);
}

//----------------------------------------------------------------------------------------------
BOOL CAppImportCriteria::LoadAppImportCriteria(const CString& strImpFileName, CAutoExpressionMng* pAutoExpressionMng	/* = NULL*/)
{
    // non è detto che ci sia
	if (strImpFileName.IsEmpty() || !ExistFile(strImpFileName) || !m_pBaseImportCriteria)
		return TRUE;

	BOOL bOk =  TRUE;
	CXMLDocumentObject aXMLCriteriaDoc(FALSE);
	aXMLCriteriaDoc.LoadXMLFile(strImpFileName);
	
	CXMLNode* pnRoot = aXMLCriteriaDoc.GetRoot();
	CXMLNode* pnChild = NULL;
	if (pnRoot)
	{
		pnChild = pnRoot->GetChildByName(CRITERIA_XML_PREDEFINED_CRITERIA);
		if (pnChild)
			bOk = m_pBaseImportCriteria->Parse(pnChild, pAutoExpressionMng);
	}
	
	aXMLCriteriaDoc.Close();
	return bOk;
}

//---------------------------------------------------------------------------
BOOL CAppImportCriteria::SaveAppImportCriteria(const CString& strImpFileName, CAutoExpressionMng* pAutoExpressionMng /* = NULL*/)
{
	if (strImpFileName.IsEmpty() || !m_pBaseImportCriteria)
		return TRUE;

	CXMLDocumentObject aXMLCriteriaDoc(FALSE);
	
	//se esiste il file lo cancello
	if (ExistFile(strImpFileName))
		::DeleteFile(strImpFileName);

	AfxInitWithXEngineEncoding(aXMLCriteriaDoc);
	CXMLNode* pnRoot = aXMLCriteriaDoc.CreateRoot(CRITERIA_XML_SELECTIONS);

	CXMLNode* pnChild = NULL;
	// creo il nodo relativo alle selezioni inserite dal programmatore
	pnChild  = pnRoot->CreateNewChild(CRITERIA_XML_PREDEFINED_CRITERIA);
	if (!m_pBaseImportCriteria->Unparse(pnChild, pAutoExpressionMng))
		return FALSE;

	aXMLCriteriaDoc.SaveXMLFile(strImpFileName, TRUE);
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////////
// 
//		CImpCriteriaWizardDoc implementation
//
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CImpCriteriaWizardDoc, CWizardFormDoc)

//------------------------------------------------------------------
CImpCriteriaWizardDoc::CImpCriteriaWizardDoc()
	:
	m_bDownload				(TRUE),
	m_EnvelopesDownloaded	(FALSE),
	m_pRXSelectedElems		(NULL),
	m_pEnvManager			(NULL),
	m_pDataManagerImport	(NULL),
	m_pAppImportCriteria	(NULL),
	m_bPendingData			(FALSE),
	m_bRXData				(TRUE),
	m_bIsTherePendingData	(FALSE),
	m_bStdSearch			(TRUE),
	m_CurrentThread			(0),
	m_bValidateOnParse		(FALSE)
{
	m_strEnvelopePath = GetXMLImportPath(FALSE);
}
//------------------------------------------------------------------
CImpCriteriaWizardDoc::~CImpCriteriaWizardDoc()
{
	SAFE_DELETE(m_pAppImportCriteria);
	//SAFE_DELETE(m_pRXSelectedElems);
}

//---------------------------------------------------------------------------
BOOL CImpCriteriaWizardDoc::OnOpenDocument(LPCTSTR pParam)
{
    if (pParam)
	{
        m_pDataManagerImport = (CXMLDataManager*) GET_AUXINFO(pParam);
		ASSERT_KINDOF(CXMLDataManager, m_pDataManagerImport);
		if (m_pDataManagerImport)
		{
			m_pEnvManager = m_pDataManagerImport->GetEnvelopeManager();
			if (m_pEnvManager)
				m_bIsTherePendingData = m_pEnvManager->LoadPendingEnvClassArray	(GetEnvelopeClass(), GetImportDocument()->GetNamespace());
				
			if (m_pDataManagerImport->GetDocument()->GetBaseImportCriteria())
				m_pAppImportCriteria = new CAppImportCriteria(m_pDataManagerImport->GetDocument());
		}
    }
	
	if (m_strEnvelopePath.IsEmpty())
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("Unable to create path for import process.\r\nCheck configuration parameters."));
		return FALSE;
	}

	if (!CAbstractFormDoc::OnOpenDocument(pParam)) return FALSE;

	TRXEParameters aTR(this);
	if (aTR.FindRecord () == TableReader::FOUND)
		m_ImportPath = aTR.GetRecord()->f_ImportPath.GetString();
	
	return TRUE;
}

//---------------------------------------------------------------------------
void CImpCriteriaWizardDoc::OnCloseDocument()
{
	CWizardFormDoc::OnCloseDocument();
}

//---------------------------------------------------------------------------
BOOL CImpCriteriaWizardDoc::OnAttachData()
{
	SetFormTitle(_TB("Set import criteria"));

	return TRUE;
}

//---------------------------------------------------------------------------
CXMLBaseAppCriteria* CImpCriteriaWizardDoc::GetBaseImportCriteria() const
{
	return  (GetImportDocument())
			? GetImportDocument()->GetBaseImportCriteria()
			: NULL; 
}

//---------------------------------------------------------------------------
CString CImpCriteriaWizardDoc::GetEnvelopeClass()
{
	CString strEnvClass = GetImportDocument()->GetXMLDocInfo()->GetEnvClass();
	if (strEnvClass.IsEmpty())
		strEnvClass = GetImportDocument()->GetTitle();
	return strEnvClass;
}

//---------------------------------------------------------------------------
BOOL CImpCriteriaWizardDoc::IsValidEnvelope(BOOL bCheckContents /*=FALSE*/)
{
	if (!m_pEnvManager )
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CString strEnvFile = m_pEnvManager->CreateFullEnvFileName(m_strEnvelopePath.GetString ());

	BOOL bOk = !m_strEnvelopePath.IsEmpty () &&
				ExistPath (m_strEnvelopePath.GetString ()) &&
				ExistFile (strEnvFile);

	if (bOk && bCheckContents)
	{
		CXMLDocumentObject envObj(FALSE,FALSE,FALSE);
		if (!envObj.LoadXMLFile (strEnvFile))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		
		//cerco "//DocumentInfo/DocNameSpace"
		CString strPrefix = GET_NAMESPACE_PREFIX((&envObj));
		CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + strPrefix + ENV_XML_DOC_INFO_TAG + URL_SLASH_CHAR + strPrefix + XML_NAMESPACE_TAG;
		
		CXMLNode *pNode = envObj.SelectSingleNode (strFilter, strPrefix);
		if (!pNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		CString strNameSpace;
		pNode->GetText (strNameSpace);
		CTBNamespace aNameSpace(strNameSpace);

		SAFE_DELETE(pNode);
		
		CAbstractFormDoc *pDoc = GetImportDocument ();
		if (!pDoc) 
		{
			ASSERT(FALSE);
			return FALSE;
		}

		bOk = bOk && (aNameSpace == pDoc->GetNamespace());
	}

	return bOk;
}

//////////////////////////////////////////////////////////////////////////////////
// 
//		CImpCriteriaWizardFrame implementation
//
//////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CImpCriteriaWizardFrame, CWizardFrame)



//////////////////////////////////////////////////////////////////////////////////
// 
//		CImpCriteriaWizardView implementation
//
//////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CImpCriteriaWizardView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImpCriteriaWizardView, CWizardFormView)
	//{{AFX_MSG_MAP(CImpCriteriaWizardView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP() 

//-----------------------------------------------------------------------------
CAbstractFormDoc* CImpCriteriaWizardView::GetImportDocument	()
{
	ASSERT(GetImpCriteriaWizardDoc());
	ASSERT(GetImpCriteriaWizardDoc()->m_pDataManagerImport);

	return (CAbstractFormDoc*) GetImpCriteriaWizardDoc()->m_pDataManagerImport->GetDocument();
}

//-----------------------------------------------------------------------------
CImpCriteriaWizardDoc* CImpCriteriaWizardView::GetImpCriteriaWizardDoc() const
{
	ASSERT(GetDocument() && GetDocument()->IsKindOf(RUNTIME_CLASS(CImpCriteriaWizardDoc)));
	return (CImpCriteriaWizardDoc*)GetDocument();
}

//-----------------------------------------------------------------------------
void CImpCriteriaWizardView::CustomizeTabWizard(CTabManager* pTabWizard)
{    
	if (!pTabWizard)
	{
		ASSERT(FALSE);
		return;
	}

	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLImportPresentationPage), IDD_WIZARD_IMPORT_PRESENTATION_PAGE);
	
	if (GetImpCriteriaWizardDoc()->IsTherePendingData())
		pTabWizard->AddDialog(RUNTIME_CLASS(CXMLImportSelPendingPage), IDD_WIZARD_IMPORT_SEL_PENDING_PAGE);
	
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLImportSelDocsPage), IDD_WIZARD_IMPORT_SEL_DOCS_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLImportSummaryPage), IDD_WIZARD_IMPORT_SUMMARY_PAGE);
}

//-----------------------------------------------------------------------------
LRESULT CImpCriteriaWizardView::OnWizardNext(UINT nIDD)
{
	CImpCriteriaWizardDoc*	pWizDoc = GetImpCriteriaWizardDoc();
	CAbstractFormDoc* pImportDocument = pWizDoc->GetImportDocument();

	if (nIDD == IDD_WIZARD_IMPORT_PRESENTATION_PAGE)
	{
		if (pImportDocument->IsEditingParamsFromExternalController() && pWizDoc->m_pAppImportCriteria)
			return (pWizDoc->CreateAppImpCriteriaTabDlgs(GetTabManager(), GetTabManager()->GetTabDialogPos(IDD_WIZARD_IMPORT_SEL_DOCS_PAGE)))
			? pWizDoc->GetFirstAppTabIDD()
			: IDD_WIZARD_IMPORT_SUMMARY_PAGE;

		return WIZARD_DEFAULT_TAB;
	}

	if (nIDD == IDD_WIZARD_IMPORT_SEL_DOCS_PAGE)
	{
		if (pWizDoc->m_bTuningEnabled)
			return IDD_WIZARD_IMPORT_SUMMARY_PAGE;

		if (
			pWizDoc->m_pRXSelectedElems && pWizDoc->m_pRXSelectedElems->GetSize() > 0 &&
			pWizDoc->m_pAppImportCriteria && pWizDoc->CreateAppImpCriteriaTabDlgs(GetTabManager(), GetTabManager()->GetTabDialogPos(IDD_WIZARD_IMPORT_SEL_DOCS_PAGE))
			)
			return pWizDoc->GetFirstAppTabIDD();

		return WIZARD_DEFAULT_TAB;
	}
	return WIZARD_DEFAULT_TAB;
}

//-----------------------------------------------------------------------------
// CImportWizardPage dialog
//---------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CImportWizardPage, CWizardTabDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImportWizardPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CImportWizardPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CImportWizardPage::CImportWizardPage(UINT nIDTemplate, CWnd* pParent /*=NULL*/, const CString& sName /*= _T("")*/)
	:
	CWizardTabDialog	(sName, nIDTemplate),
	m_pXMLDataMng		(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL CImportWizardPage::OnInitDialog()
{
	if (!GetDocument() || !m_pParentTabManager || !m_pParentTabManager->IsKindOf(RUNTIME_CLASS(CTabWizard)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pXMLDataMng = GetWizardDoc()->m_pDataManagerImport;
	if (!m_pXMLDataMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return  CWizardTabDialog::OnInitDialog();
}

//----------------------------------------------------------------------------
LRESULT CImportWizardPage::OnWizardCancel()
{
	if (m_pXMLDataMng)
		m_pXMLDataMng->SetContinueImportExport (FALSE);

	return CWizardTabDialog::OnWizardCancel();
}

//-----------------------------------------------------------------------------
// CXMLImportStartPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportStartPage, CImportWizardPage)

//-----------------------------------------------------------------------------
CXMLImportStartPage::CXMLImportStartPage(CWnd* pParent /*=NULL*/)
	:
	CImportWizardPage (IDD_WIZARD_IMPORT_START_PAGE, pParent, _T("StartPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLImportStartPage::BuildDataControlLinks()
{
	AddLink (IDC_IMPORT_DOWLOAD_BTN, _T("Import"), NULL,	&GetWizardDoc()->m_bDownload,	RUNTIME_CLASS(CBoolButton));
	((CButton*)GetDlgItem(IDC_IMPORT_NO_DOWLOAD_BTN))->SetCheck(!GetWizardDoc()->m_bDownload);
}


//-----------------------------------------------------------------------------
LRESULT CXMLImportStartPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_START;
}

//--------------------------------------------------------------------
LRESULT CXMLImportStartPage::OnWizardNext()
{
	return GetWizardDoc()->m_bDownload ? 
							IDD_WIZARD_IMPORT_WAIT_PAGE : 
							(GetWizardDoc()->IsTherePendingData() ?
												IDD_WIZARD_IMPORT_SEL_PENDING_PAGE:
												IDD_WIZARD_IMPORT_SEL_DOCS_PAGE);
				
}

//-----------------------------------------------------------------------------
// CXMLImportPresentationPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportPresentationPage, CImportWizardPage)

BEGIN_MESSAGE_MAP(CXMLImportPresentationPage, CImportWizardPage)
	ON_EN_VALUE_CHANGED(IDC_IMPORT_IMPORT, OnImportTuningChanged)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLImportPresentationPage::CXMLImportPresentationPage(CWnd* pParent /*=NULL*/)
	:
	CImportWizardPage (IDD_WIZARD_IMPORT_PRESENTATION_PAGE, pParent, _T("PresentationPage"))
{
}

//-----------------------------------------------------------------------------
BOOL CXMLImportPresentationPage::OnInitDialog()
{
	CImpCriteriaWizardDoc *pDoc = GetWizardDoc();
	ASSERT(pDoc);

	BOOL bOk = CImportWizardPage::OnInitDialog();
	
	pDoc->m_bTuningEnabled = FALSE;

	if (AfxGetXEngineObject()->UseOldXTechMode())
	{
		CWnd* pWnd = GetDlgItem (IDC_PRESENTATION_DESCRI1);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem (IDC_IMPORT_IMPORT);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem (IDC_IMPORT_DATA_DESCRI);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem (IDC_IMPORT_TUNING);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem (IDC_IMPORT_TUNING_FILE);
		pWnd->ShowWindow(SW_HIDE);
	}
	else
	{

		CWnd* pWnd = GetDlgItem (IDC_PRESENTATION_DESCRI2);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem (IDC_PRESENTATION_DESCRI3);
		pWnd->ShowWindow(SW_HIDE);
		CString sFilePath;
		if (pDoc->m_pDataManagerImport && pDoc->m_pDataManagerImport->GetDocument())
		{
			CPathFinder::PosType aPos = AfxGetBaseApp()->IsDevelopment() ? 
										CPathFinder::STANDARD :
										CPathFinder::CUSTOM;
			sFilePath = AfxGetPathFinder()->GetDocumentActionsFullName(pDoc->m_pDataManagerImport->GetDocument()->GetNamespace(), aPos);
		}
		
		pWnd = GetDlgItem (IDC_IMPORT_TUNING_FILE);
		if (pWnd)
		{
			CString sText;
			pWnd->GetWindowText(sText);
			sText = cwsprintf(sText, sFilePath);
			pWnd->SetWindowText(sText);
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void CXMLImportPresentationPage::BuildDataControlLinks()
{
/*	AddLink
	(
		IDC_IMPORT_IMPORT,
		_T("EnableImport"),
		NULL,
		&GetWizardDoc()->m_bTuningEnabled,
		RUNTIME_CLASS(CBoolButton)
	);*/
	if (!AfxGetXEngineObject()->UseOldXTechMode())
	{
		AddLink
		(
			IDC_IMPORT_TUNING,
			_T("EnableTuning"),
			NULL,
			&GetWizardDoc()->m_bTuningEnabled,
			RUNTIME_CLASS(CBoolButton)
		);

		CheckAdjust ();
	}
}

//-----------------------------------------------------------------------------
void CXMLImportPresentationPage::CheckAdjust ()
{
	CWnd* pWnd = GetDlgItem (IDC_IMPORT_IMPORT);
	if (!pWnd)
		return;

	((CButton*) pWnd)->SetCheck(!GetWizardDoc()->m_bTuningEnabled);
	pWnd->UpdateWindow();
}

//-----------------------------------------------------------------------------
LRESULT CXMLImportPresentationPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_START;
}

//-----------------------------------------------------------------------------
void CXMLImportPresentationPage::OnImportTuningChanged ()
{
	CheckAdjust ();
	GetWizardDoc()->UpdateDataView();
}

//-----------------------------------------------------------------------------
// CXMLImportWaitPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportWaitPage, CImportWizardPage)

BEGIN_MESSAGE_MAP(CXMLImportWaitPage, CImportWizardPage)
	//{{AFX_MSG_MAP(CXMLExportSelProfilePage)
	ON_WM_PAINT	()
	//}}AFX_MSG_MAP
	ON_WM_SIZE()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLImportWaitPage::CXMLImportWaitPage(CWnd* pParent /*=NULL*/)
	:
	m_bFirstPaint(TRUE),
	CImportWizardPage (IDD_WIZARD_IMPORT_WAIT_PAGE, pParent, _T("WaitPage"))
{
}

//-----------------------------------------------------------------------------
void CXMLImportWaitPage::BuildDataControlLinks()
{
	SetDlgItemText(IDC_IMPORT_DOWNLOAD_MESSAGES, (_TB("Downloading, please wait...")));
}

//----------------------------------------------------------------------------
void CXMLImportWaitPage::OnSize(UINT nType, int cx, int cy)
{
	CImportWizardPage::OnSize(nType, cx, cy);

	CWnd *pWnd = GetDlgItem(IDC_IMPORT_DOWNLOAD_MESSAGES);
	if (pWnd)
	{
		pWnd->SetWindowPos(NULL, 0, 0, cx - 10, cy - 10, SWP_NOMOVE);
		pWnd->CenterWindow();
	}
}

//-----------------------------------------------------------------------------
LRESULT CXMLImportWaitPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_WAIT;
}

// appena visualizzato la dialog, contatto il Repository.
// è un po' una porcata fatto così, ma non ho trovato altro punto 
// in cui inserirmi subito dopo che la dialog era stata disegnata
void CXMLImportWaitPage::OnPaint()
{
	CImportWizardPage::OnPaint ();

	if (m_bFirstPaint)
		m_bFirstPaint = FALSE;
}

//--------------------------------------------------------------------
LRESULT CXMLImportWaitPage::OnWizardNext()
{
	return GetWizardDoc()->IsTherePendingData() 
		? IDD_WIZARD_IMPORT_SEL_PENDING_PAGE
		: IDD_WIZARD_IMPORT_SEL_DOCS_PAGE;
}

//----------------------------------------------------------------------------
void CXMLImportWaitPage::OnUpdateWizardButtons ()
{
	SetWizardButtons(0);
}

//-----------------------------------------------------------------------------
// CXMLImportSelPendingPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportSelPendingPage, CImportWizardPage)

//-----------------------------------------------------------------------------
CXMLImportSelPendingPage::CXMLImportSelPendingPage(CWnd* pParent /*=NULL*/)
	:
	CImportWizardPage (IDD_WIZARD_IMPORT_SEL_PENDING_PAGE, pParent, _T("SelPendingPage"))
{
}

//-----------------------------------------------------------------------------
CXMLImportSelPendingPage::~CXMLImportSelPendingPage()
{
}


//-----------------------------------------------------------------------------
void CXMLImportSelPendingPage::BuildDataControlLinks()
{
	AddLink (IDC_IMPORT_PENDING_BTN, _T("Pending"),	NULL,	&GetWizardDoc()->m_bPendingData,	RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_IMPORT_RX_BTN, _T("Ricezione"),			NULL,	&GetWizardDoc()->m_bRXData,			RUNTIME_CLASS(CBoolButton));
	((CButton*)GetDlgItem(IDC_IMPORT_BOTH_BTN))->SetCheck(!GetWizardDoc()->m_bPendingData && !GetWizardDoc()->m_bRXData);
}


//-----------------------------------------------------------------------------
LRESULT CXMLImportSelPendingPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_SEL_PENDING;
}


//-----------------------------------------------------------------------------
// CXMLImportSelDocsPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportSelDocsPage, CImportWizardPage)

//-----------------------------------------------------------------------------
CXMLImportSelDocsPage::CXMLImportSelDocsPage(CWnd* pParent /*=NULL*/)
	:
	CImportWizardPage (IDD_WIZARD_IMPORT_SEL_DOCS_PAGE, pParent, _T("SelDocsPage")),
	m_pEnvTree(NULL)
{
}

//-----------------------------------------------------------------------------
CXMLImportSelDocsPage::~CXMLImportSelDocsPage()
{
	if (m_pEnvTree)
		delete m_pEnvTree;
}

//----------------------------------------------------------------------------
BOOL CXMLImportSelDocsPage::OnInitDialog() 
{
	CImpCriteriaWizardDoc *pDoc = GetWizardDoc();
	ASSERT(pDoc);

	if (!pDoc->m_pEnvManager)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bOk = CImportWizardPage::OnInitDialog();

	if (pDoc->m_bPendingData)
		pDoc->m_pEnvManager->LoadPendingEnvClassArray	(
															pDoc->GetEnvelopeClass(),
															GetImportDocument()->GetNamespace()
														);
	else if (pDoc->m_bRXData)
		pDoc->m_pEnvManager->LoadRXEnvClassArray		(
																pDoc->GetEnvelopeClass(),
																GetImportDocument()->GetNamespace()
															);
	else
		pDoc->m_pEnvManager->LoadBothEnvClassArray	(
																pDoc->GetEnvelopeClass(),
																GetImportDocument()->GetNamespace()
															);

	if (pDoc->m_pEnvManager->m_pXMLRXEnvClasses &&
		pDoc->m_pEnvManager->m_pXMLRXEnvClasses->GetSize ())
	{
		m_pEnvTree = new CRxEnvelopeTree(pDoc->m_pEnvManager->m_pXMLRXEnvClasses,!pDoc->m_bTuningEnabled);
		m_pEnvTree->SubclassDlgItem(IDC_IMPORT_SEL_TREE, this);

		m_pEnvTree->FillTree();
		m_pEnvTree->ExpandAll(TVE_EXPAND);

		CWnd* pWnd = GetDlgItem (IDC_IMPORT_NO_ENVELOPE_STATIC);
		if (pWnd)
			pWnd->ShowWindow (SW_HIDE);
	}
	else
	{
		CWnd* pWnd = GetDlgItem (IDC_IMPORT_SEL_TREE);
		if (pWnd) 
			pWnd->ShowWindow (SW_HIDE);
		pWnd = GetDlgItem (IDC_IMPORT_SELECT_ENVELOPE_STATIC);
		if (pWnd)
			pWnd->ShowWindow (SW_HIDE);
		
		pWnd = GetDlgItem (IDC_IMPORT_NO_ENVELOPE_STATIC);
		if (pWnd)
		{
			CString strFolder = pDoc->m_ImportPath.IsEmpty() ? 
									cwsprintf(_TB("NONE")) : 
									pDoc->m_ImportPath;
			pWnd->SetWindowText (cwsprintf(_TB("THERE ARE CURRENTLY NO DOCUMENTS TO BE IMPORTED IN THE FILE SYSTEM.\r\nDownload the envelopes from the network or\r\nenter the envelopes manually in the alternative import path you can configure in the site parameters."), strFolder));
			pWnd->ShowWindow (SW_SHOW);
		}
	}
	
	return bOk;
}



//-----------------------------------------------------------------------------
LRESULT CXMLImportSelDocsPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_SEL_DOCS;
}


//----------------------------------------------------------------------------
LRESULT CXMLImportSelDocsPage::OnWizardNext()
{
	if (m_pEnvTree && GetWizardDoc()->m_pRXSelectedElems)
	{
		m_pEnvTree->GetEnvSelArray(GetWizardDoc()->m_pRXSelectedElems);
		return CImportWizardPage::OnWizardNext();
	}

	return IDD_WIZARD_IMPORT_SUMMARY_PAGE;
}

//-----------------------------------------------------------------------------
// CXMLImportSummaryPage dialog
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLImportSummaryPage, CImportWizardPage)

//-----------------------------------------------------------------------------
CXMLImportSummaryPage::CXMLImportSummaryPage(CWnd* pParent /*=NULL*/)
	:
	CImportWizardPage (IDD_WIZARD_IMPORT_SUMMARY_PAGE, pParent, _T("SummaryPage"))
{
}

//-----------------------------------------------------------------------------
CXMLImportSummaryPage::~CXMLImportSummaryPage()
{

}

//----------------------------------------------------------------------------
BOOL CXMLImportSummaryPage::OnInitDialog() 
{
	BOOL bOk = CImportWizardPage::OnInitDialog();
	
	FillSelection();

	return bOk;
}

//---------------------------------------------------------------------------
void CXMLImportSummaryPage::FillSelection()
{
	CImpCriteriaWizardDoc* pWizDoc = GetWizardDoc();
	if (!pWizDoc)
	{
		ASSERT(FALSE);
		return;
	}
	CAbstractFormDoc*  pImportDoc = pWizDoc->GetImportDocument();

	// il wizard è stato lanciato dallo scheduler per editare i parametri 
	if (pImportDoc->IsEditingParamsFromExternalController() && pWizDoc->GetBaseImportCriteria())
	{
		SetDlgItemText(IDC_IMPORT_SUMMARY_STATIC, _TB("You set application import parameters for scheduled import task"));
		return;
	}

	if (!pWizDoc->m_pEnvManager)
		return;
	
	CXMLEnvElemArray* pXMLRXEnvElemArray = pWizDoc->m_pRXSelectedElems;
	
	if (!pWizDoc->m_bStdSearch)
	{
		if (pWizDoc->IsValidEnvelope (TRUE))
			pWizDoc->m_pEnvManager->FillSelection(pXMLRXEnvElemArray, pWizDoc->m_strEnvelopePath.GetString(), GetImportDocument());
		else
		{
			SetDlgItemText(IDC_IMPORT_SUMMARY_STATIC, cwsprintf(_TB("The selected envelope is not available for the current document.")));
			return;
		}
	}

	if (!pXMLRXEnvElemArray || !pXMLRXEnvElemArray->GetSize ())
	{
		SetDlgItemText(IDC_IMPORT_SUMMARY_STATIC, cwsprintf(_TB("No item selected.")));
		return;
	}
	
	CString strCurrentSelection;
	CString strEnvClass;
	CString strSenderSite;

	for(int i = 0; i < pXMLRXEnvElemArray->GetSize() ; i++)
	{
		CXMLEnvElem* pElem = pXMLRXEnvElemArray->GetAt(i);

		if (!pElem ||
			!pElem->m_pSiteAncestor || 
			!pElem->m_pSiteAncestor->m_pAncestor)
			continue;

		// rottura di classe
		if (strEnvClass.CompareNoCase (pElem->m_pSiteAncestor->m_pAncestor->m_strEnvClass))
		{
			if (!strEnvClass.IsEmpty ())
				strCurrentSelection += CRLF; //spazio separatore di gruppo
			strEnvClass = pElem->m_pSiteAncestor->m_pAncestor->m_strEnvClass;
			strCurrentSelection += cwsprintf(_TB("Envelope class: {0-%s} \r\n."), strEnvClass);
			strSenderSite.Empty();
		}

		// rottura di sender site
		if (strSenderSite.CompareNoCase (pElem->m_pSiteAncestor->m_strSiteName))
		{
			if (!strSenderSite.IsEmpty ())
				strCurrentSelection += CRLF; //spazio separatore di gruppo
			strSenderSite = pElem->m_pSiteAncestor->m_strSiteName;
			strCurrentSelection += cwsprintf(_TB("Source site: {0-%s} \r\n."), strSenderSite);
		}

		strCurrentSelection += cwsprintf(_T("- {0-%s} \r\n."), pElem->m_strEnvName);
	}
	if (pWizDoc->GetBaseImportCriteria())
		strCurrentSelection += _TB("Application import parameters are set");    

	SetDlgItemText(IDC_IMPORT_SUMMARY_STATIC, strCurrentSelection);
}

//-----------------------------------------------------------------------------
LRESULT CXMLImportSummaryPage::OnGetBitmapID()
{
	return IDB_WIZARD_IMPORT_SUMMARY;
}

//-----------------------------------------------------------------------------
void CXMLImportSummaryPage::BuildDataControlLinks()
{
	CImpCriteriaWizardDoc*	pWizDoc		= GetWizardDoc();		
	CAbstractFormDoc*		pImportDoc	= pWizDoc->GetImportDocument();

	CParsedCtrl* pBtn = AddLink (IDC_IMPORT_VALIDATE_CHK, _T("Valida"),	NULL,	&pWizDoc->m_bValidateOnParse,	RUNTIME_CLASS(CBoolButton));
	if (pImportDoc->IsEditingParamsFromExternalController())
		pBtn->ShowCtrl(SW_HIDE);
}

//----------------------------------------------------------------------------
LRESULT CXMLImportSummaryPage::OnWizardFinish()
{
	CImpCriteriaWizardDoc*	pWizDoc		= GetWizardDoc();		
	CAbstractFormDoc*		pImportDoc	= pWizDoc->GetImportDocument();
	
	//salvo i parametri per lo scheduler					
	if (pImportDoc->IsEditingParamsFromExternalController() && pWizDoc->GetBaseImportCriteria())
	{
		m_pXMLDataMng->SetContinueImportExport(TRUE);
		m_pXMLDataMng->SetValidateOnParse(FALSE);
		m_pXMLDataMng->SetTuningEnable(FALSE);
		m_pXMLDataMng->SetTuningEMailFrom(_T(""));
		m_pXMLDataMng->SetTuningEMailTo(_T(""));		

		if (m_pXMLDataMng->GetAppImportCriteriaFileName().IsEmpty())
		{
			CString strFileName, strImpCriteriaFullName; 						
			int n = 1;
			BOOL bExist = TRUE;
			while (bExist)
			{
				strFileName.Format(_T("TASK_ImpCriteriaVars_%d.xml"), n);
				strImpCriteriaFullName = MakeImpCriteriaVarFile
							(
								pImportDoc->GetNamespace(), 
								strFileName, 
								CPathFinder::USERS, 
								AfxGetLoginInfos()->m_strUserName
							);
				bExist = ::ExistFile(strImpCriteriaFullName);
				n++;
			}
			m_pXMLDataMng->SetAppImportCriteriaFileName(strFileName);
		}
		
		pWizDoc->SaveAppImportCriteria(m_pXMLDataMng->GetAppImportCriteriaFileName(), pWizDoc->m_pAutoExpressionMng);	
	}
	else
	{
		if (pWizDoc->m_pRXSelectedElems && pWizDoc->m_pRXSelectedElems->GetSize ())
		{
			m_pXMLDataMng->SetContinueImportExport (TRUE);
			m_pXMLDataMng->SetValidateOnParse(GetWizardDoc()->m_bValidateOnParse);
			m_pXMLDataMng->SetTuningEnable(pWizDoc->m_bTuningEnabled);
			/*m_pXMLDataMng->SetTuningEMailTo(pWizDoc->m_TuningEmailTo.Str());
			m_pXMLDataMng->SetTuningEMailFrom(pWizDoc->m_TuningEmailFrom.Str());*/
		}
	}

	return CImportWizardPage::OnWizardFinish();
}

