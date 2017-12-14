#include "stdafx.h" 

#include <TBGENERIC\globals.h>
#include <TBGENERIC\parameterssections.h>

#include <TBGENLIB\baseapp.h>
#include <TBGENLIB\TbCommandInterface.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGES\DBT.H>
#include <TBGES\xsltmng.h>
#include <TBGES\eventmng.h>

#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "GenFunc.h"
#include "ProfiliMngWizPage.h"
#include "XMLTransferTags.h"
#include "XMLProfileInfo.h"
#include "ExpCriteriaObj.h"
#include "ExpCriteriaDlg.h"
#include "XMLDataMng.h"

// resource declarations
#include "XMLDataMng.hjson"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szLostAndFound			[] = _T("LostAndFound");
static const TCHAR szAlternativeSearch	[] = _T("AlternativeSearch");

//http://www.microarea.it/Schema/2004/Smart/AppName/ModName/DocumentName/PosType/UserName/ProfileName.xsd
//----------------------------------------------------------------------------------------------
void GetInformationFromNsUri(const CString& strNsUri, CString& strProfile, CPathFinder::PosType& ePosType, CString& strUserName)
{
	CString nsURIReverse = strNsUri;
	nsURIReverse.MakeReverse();
	
	int nCurrPos = 0;
	int i = 0;
	CString strToken (_T("/"));
	CString strRevElem, strElem;
	
	while (nCurrPos < nsURIReverse.GetLength())
	{
		strRevElem = nsURIReverse.Tokenize(strToken, nCurrPos);
		if (strRevElem.IsEmpty())
			break;

		strElem = strRevElem.MakeReverse();
		if (i == 0)
		{
			int nPos = strElem.Find(szXsdExt);
			if (nPos > 0)
				strProfile = strElem.Left(nPos);
		}

		if (i == 1)
		{
			if (strElem.CompareNoCase(szAllUserDirName) == 0)
				ePosType = CPathFinder::ALL_USERS;
			else
			{
				if (strElem.CompareNoCase(szStandard) == 0)
					ePosType = CPathFinder::STANDARD;
				else
				{
					ePosType = CPathFinder::USERS;
					strUserName = strElem;
				}				
			}
		}

		i++;
	}
}

/////////////////////////////////////////////////////////////////////////////
//	CDataFieldEvents implementation
/////////////////////////////////////////////////////////////////////////////
//


/////////////////////////////////////////////////////////////////////////////
//	CXMLRecordInfo implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CXMLRecordInfo::CXMLRecordInfo(SqlRecord* pRec, const CString& strProfileName, const CString& strFileName)
:
	m_strProfileName		(strProfileName),
	m_strFileName			(strFileName)
{
	if (pRec)
	{
		pRec->GetKeyStream(m_aPrimaryKeyValues);
		CString strChiave;
		for (int i = 0; i <= m_aPrimaryKeyValues.GetUpperBound(); i++)
			strChiave += m_aPrimaryKeyValues.GetAt(i)->FormatData();

		TRACE("CXMLRecordInfo::CXMLRecordInfo: processed record with key %ws of the table %ws, profile: %ws file name: %ws\n", (LPCTSTR)strChiave, pRec->GetTableName(), (LPCTSTR)strProfileName, (LPCTSTR)strFileName); 
	}
}

/////////////////////////////////////////////////////////////////////////////
//	CXMLDomDocElement implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CXMLDomDocElement::CXMLDomDocElement(const CString& strUrlData)
:
	m_strUrlData	(strUrlData),
	m_pXMLDomDoc	(NULL)	
{
}

//-----------------------------------------------------------------------------
CXMLDomDocElement::~CXMLDomDocElement()
{
	if (m_pXMLDomDoc)
		delete m_pXMLDomDoc;
}

//-----------------------------------------------------------------------------
void CXMLDomDocElement::SetXMLDocument(const CString& strXMLExpFile, CXMLDocumentObject* pXMLDom)
{
	m_strFileName = strXMLExpFile;
	m_pXMLDomDoc = pXMLDom;
}

//-----------------------------------------------------------------------------
BOOL CXMLDomDocElement::ApplyAndSaveTransformXSLT(CAbstractFormDoc* pDocument)
{
	if (m_strXSLTFileName.IsEmpty())
		return TRUE;

	CXSLTManager XSLTManager(pDocument);
	if (XSLTManager.Transform(m_pXMLDomDoc, m_strXSLTFileName))
	{
		*m_pXMLDomDoc = *XSLTManager.GetOutput();		
		m_pXMLDomDoc->SaveXMLFile((LPCTSTR)m_strFileName, TRUE);
		return TRUE;
	}

	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//	CXMLDocElement implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CXMLDocElement::CXMLDocElement(CAbstractFormDoc* pDoc)
	:
	m_pDocument				(pDoc),
	m_pProcessedRecordArray	(NULL),
	m_pCurrentRecord		(NULL),
	m_pCurrentDomDoc		(NULL),
	m_bOldUnattendedMode	(FALSE),
	m_bIsRootDoc			(TRUE)
{
}	

//-----------------------------------------------------------------------------
CXMLDocElement::CXMLDocElement(const CTBNamespace& aNameSpace, CBaseDocument *pAncestor, BOOL bCanRunOnlyBusinessObject /*= FALSE*/)
	:	
	m_pDocument				(NULL),
	m_pProcessedRecordArray	(NULL),
	m_pCurrentRecord		(NULL),
	m_pCurrentDomDoc		(NULL),
	m_bOldUnattendedMode	(FALSE),
	m_bIsRootDoc			(FALSE)
{
	if (!pAncestor)
	{
		ASSERT(FALSE);
		return;
	}

	//I need to start a new diagnostic session to avoid that possible errors in current level make the document creation failed
	pAncestor->m_pMessages->StartSession();

	//Impr. 6393 Istanziazione documento senza view
	CImportExportParams* pImpExpParams = NULL;
	if (bCanRunOnlyBusinessObject)
		pImpExpParams = new CImportExportParams(TRUE);


	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument
	(
		aNameSpace.ToString(),
		szDefaultViewMode,
		TRUE,
		pAncestor, /*pAncestror*/
		NULL, /*lpAuxInfo*/
		NULL, /*ppExistingDoc*/
		NULL, /*pFailedCode*/
		NULL,/*pControllerInfo*/
		FALSE,/*IsRunningAsADM*/
		NULL,/*pTBContext*/
		pImpExpParams/*pMangedParams*/
	);


	pAncestor->m_pMessages->EndSession();
	TRY
	{
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			m_pDocument = (CAbstractFormDoc*)pDoc;
		//	if (bCanRunOnlyBusinessObject)
		//		m_pDocument->DestroyFrameHandle();
		}
		else
			return;
	}

	if (m_pDocument->GetMasterFrame())
		m_pDocument->GetMasterFrame()->ShowWindow(SW_HIDE);

	m_bOldUnattendedMode = m_pDocument->SetInUnattendedMode();

	//load xml files description
	m_pDocument->LoadXMLDescription(); 

	CATCH_ALL(e)
	{
		if (AfxIsValidAddress(m_pDocument, sizeof(CAbstractFormDoc)))
			m_pDocument->OnCloseDocument();
		m_pDocument = NULL;
	}
	END_CATCH_ALL
}

//-----------------------------------------------------------------------------
CXMLDocElement::~CXMLDocElement()
{
	if (!m_bIsRootDoc && m_pDocument)
	{
		if (m_pDocument->m_pExternalControllerInfo) //the import\export procedure runned from scheduler
			m_pDocument->m_pExternalControllerInfo = NULL;
		
		if (
				!AfxIsInUnattendedMode() && //se sono in MagicLink non effettuo l'ottimizzazione delle tabdialog a causa di largo consumo della memoria in caso di multithread
				m_pDocument->GetXMLDataManager() && m_pDocument->GetXMLDataManager()->IsKindOf(RUNTIME_CLASS(CXMLDataManager))
			)
			((CXMLDataManager*)m_pDocument->GetXMLDataManager())->RestoreActiveTabs();

		m_pDocument->SetInUnattendedMode(m_bOldUnattendedMode);	
		m_pDocument->OnCloseDocument();
	}

	if (m_pProcessedRecordArray)
		delete m_pProcessedRecordArray;
}

//-----------------------------------------------------------------------------
void CXMLDocElement::InitRecordsProcessed()
{
	if (m_pProcessedRecordArray)
		delete m_pProcessedRecordArray;
	m_pProcessedRecordArray = NULL;
}

//-----------------------------------------------------------------------------
BOOL CXMLDocElement::ApplyAndSaveTransformXSLT()
{
	if (!m_pDocument) return TRUE;

	BOOL bOk = TRUE;
	CXMLDomDocElement* pXMLDomElem = NULL;
	for (int i = 0; i < m_aXMLDomDocArray.GetSize() ; i++)
	{
		pXMLDomElem = (CXMLDomDocElement*)m_aXMLDomDocArray.GetAt(i);
		if (pXMLDomElem)
			bOk = pXMLDomElem->ApplyAndSaveTransformXSLT(m_pDocument) && bOk;
	}
	return bOk;
}


//-----------------------------------------------------------------------------
void CXMLDocElement::FormatXMLDocForSmart(CXMLDocumentObject* pXMLDoc, const CXMLProfileInfo* pXMLProfileInfo)
{
	//devo inserire l'URI per il profilo utilizzato
	//se profilo vuoto devo considerare la descrizione del documento
	CString strProfileName = (pXMLProfileInfo->GetName().IsEmpty())
						 ? DESCRI + m_pDocument->GetNamespace().GetObjectName() 
						 : pXMLProfileInfo->GetName();	

	CString strNamespaceURI = pXMLProfileInfo->GetSmartNamespaceURI();
	
	pXMLDoc->SetNameSpaceURI(strNamespaceURI, szNamespacePrefix);
	CXMLNode* pRoot = pXMLDoc->CreateRoot(m_pDocument->GetNamespace().GetObjectNameForTag());
	pRoot->SetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, m_pDocument->GetNamespace().ToString());
	pRoot->SetAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE, strProfileName);
}

//-----------------------------------------------------------------------------
void CXMLDocElement::FormatXMLDocForStandard(CXMLDocumentObject* pXMLDoc, const CXMLProfileInfo* pXMLProfileInfo)
{
	if (!pXMLDoc)
		return;

	USES_CONVERSION;
	pXMLDoc->SetNameSpaceURI(XTECH_NAMESPACE);

	CXMLNode* pRoot = pXMLDoc->CreateRoot(DOC_XML_DOCUMENT_TAG);
	ASSERT (pRoot);
	
	pRoot->SetAttribute(DOC_XML_DOCNAME_ATTRIBUTE, A2T(m_pDocument->GetRuntimeClass()->m_lpszClassName));

	CXMLNode* pRootChild = pRoot->CreateNewChild(DOC_XML_DOCINFO_TAG);
	ASSERT (pRootChild);

	CXMLNode* pNode;
	pNode = pRootChild->CreateNewChild((LPCTSTR) DOC_XML_NAMESPACE_TAG);
	if (pNode)
		pNode->SetText(m_pDocument->GetNamespace().ToString());

	pNode = pRootChild->CreateNewChild((LPCTSTR) DOC_XML_PROFILE_TAG);
	if (pNode)
		pNode->SetText((LPCTSTR)pXMLProfileInfo->GetName());

	pNode = pRootChild->CreateNewChild((LPCTSTR) DOC_XML_TITLE_TAG);
	if (pNode)
		pNode->SetText((LPCTSTR)m_pDocument->GetTitle());

	pNode = pRootChild->CreateNewChild((LPCTSTR) DOC_XML_DESCRIPTION_TAG);

	CXMLNode* pCreationNode = pRootChild->CreateNewChild((LPCTSTR) DOC_XML_CREATION_TAG);
	if (pCreationNode)
	{
		pNode = pCreationNode->CreateNewChild((LPCTSTR) DOC_XML_DOMAIN_TAG);
		if (pNode)
			pNode->SetText((LPCTSTR)AfxGetDomainName());
				
		pNode = pCreationNode->CreateNewChild((LPCTSTR) DOC_XML_SITE_TAG);
		if (pNode)
			pNode->SetText((LPCTSTR)AfxGetSiteName());

		pNode = pCreationNode->CreateNewChild((LPCTSTR) DOC_XML_USER_TAG);
		if (pNode)
			pNode->SetText((LPCTSTR)AfxGetLoginInfos()->m_strUserName);			

		pNode = pCreationNode->CreateNewChild((LPCTSTR) DOC_XML_DATETIME_TAG);
		if (pNode)
		{
			SYSTEMTIME sysTime;
			// The GetSystemTime function retrieves the current system date and 
			// time. The system time is expressed in Coordinated Universal Time (UTC). 
			//::GetSystemTime (&sysTime); 

			// The GetLocalTime function retrieves the current local date and time. 
			::GetLocalTime (&sysTime);

			pNode->SetText((LPCTSTR)FormatDateTimeForXML(sysTime));
		}
	}
	pRoot->CreateNewChild(DOC_XML_DOCUMENTS_TAG);
}

//-----------------------------------------------------------------------------
CXMLDocumentObject* CXMLDocElement::GetXMLDomDocument
									(
										const CString& strUrlData, 
										const CString& strXMLExpFile,
										const CXMLProfileInfo* pXMLProfileInfo,
										BOOL bNewXMLFile, 
										BOOL bDisplayMsgBox,
										BOOL bNextFile /*=FALSE*/,
										BOOL bForXMLSmartDocument
									)
{	
	if (!m_pDocument || strUrlData.IsEmpty() || strXMLExpFile.IsEmpty())
		return NULL;

	CXMLDomDocElement* pXMLDomElem = NULL;
	CXMLDocumentObject* pXMLDoc = NULL;
	CXMLDomDocElement* pSingleDom;
	for (int i = 0; i < m_aXMLDomDocArray.GetSize() ; i++)
	{
		pSingleDom = (CXMLDomDocElement*) m_aXMLDomDocArray.GetAt(i);
		if	(
				pSingleDom && 
				pSingleDom->m_strUrlData.CompareNoCase(strUrlData) == 0 &&
				(bNewXMLFile || pSingleDom->m_strFileName.CompareNoCase(strXMLExpFile) == 0)
			)
		{
			pXMLDomElem = pSingleDom;
			break;
		}
	}
		
	if (pXMLDomElem)
	{
		pXMLDoc = pXMLDomElem->m_pXMLDomDoc;
		// restituisco il dom trovato
		if (!bNewXMLFile)
		{
			pXMLDomElem->SetXMLDocument(strXMLExpFile, pXMLDoc);
			m_strCurrFileName = strXMLExpFile;
			return pXMLDoc;
		}
		
		// altrimenti devo inserire in fondo il TAG: <Continued> ed il nome del file su cui 
		// continua l'esportazione. Che è il file che sto per creare  (se richiesto)
		CXMLNode* pRootChild = pXMLDoc->GetRootChildByName(DOC_XML_DOCINFO_TAG);
		if (bNextFile && pRootChild)
		{
			CXMLNode* pNode = pRootChild->CreateNewChild(DOC_XML_NEXTFILE_TAG);
			pNode->SetText((LPCTSTR)::GetName(strXMLExpFile));
			pXMLDoc->SaveXMLFile((LPCTSTR)pXMLDomElem->m_strFileName, TRUE);

			// prima di creare il file successivo applico le eventuali trasformazioni previste
			// per il foglio 
			pXMLDomElem->ApplyAndSaveTransformXSLT(m_pDocument);
		}	
		
		delete pXMLDoc;
		pXMLDoc = NULL;
	}
	else
	{
		pXMLDomElem = new CXMLDomDocElement(strUrlData);
		pXMLDomElem->m_strXSLTFileName = (pXMLProfileInfo->IsTransformProfile()) ? pXMLProfileInfo->GetXSLTFileName() : _T("") ;
		m_aXMLDomDocArray.Add(pXMLDomElem);		
	}

	// vuol dire che o non l'ho ancora creato oppure è stato temporaneamente scaricato
	// dalla memoria per la gestione dell'LRU
	pXMLDoc = new CXMLDocumentObject(FALSE, bDisplayMsgBox);	
		
	if (!bNewXMLFile)
	{
		pXMLDoc->EnableMsgMode(FALSE);
		if (!pXMLDoc->LoadXMLFile(strXMLExpFile))
			bNewXMLFile = TRUE; // se non si è riusciti a caricare il file esistente al quale
								// "appendere" le nuove informazioni, lo creo ex-novo
		else
			pXMLDoc->EnableMsgMode(bDisplayMsgBox);
	}

	if (bNewXMLFile)
	{
		AfxInitWithXEngineEncoding(*pXMLDoc);
	
		if (bForXMLSmartDocument)
			FormatXMLDocForSmart(pXMLDoc, pXMLProfileInfo);
		else
			FormatXMLDocForStandard(pXMLDoc, pXMLProfileInfo);
	}

	pXMLDomElem->SetXMLDocument(strXMLExpFile, pXMLDoc);
	m_strCurrFileName = strXMLExpFile;

	return pXMLDoc;
}


//-----------------------------------------------------------------------------
CXMLRecordInfo* CXMLDocElement::GetExportedRecord(SqlRecord* pRec, const CString& strProfile)
{
	return GetProcessedRecord(pRec, strProfile, _T(""));
}

//-----------------------------------------------------------------------------
CXMLRecordInfo* CXMLDocElement::GetImportedRecord(SqlRecord* pRec, const CString& strFilename)
{
	return GetProcessedRecord(pRec, _T(""), strFilename);
}

//-----------------------------------------------------------------------------
CXMLRecordInfo* CXMLDocElement::GetProcessedRecord(SqlRecord* pRec, const CString& strProfileName, const CString& strFileName)
{
	if (!m_pProcessedRecordArray || !pRec)
		return NULL;

	CXMLRecordInfo* pXMLExpRec = NULL;
	DataObjArray aPkSegments;
	pRec->GetKeyStream (aPkSegments);
	
	for (int i = 0; i <= m_pProcessedRecordArray->GetUpperBound(); i++)
	{
		pXMLExpRec = (CXMLRecordInfo*)m_pProcessedRecordArray->GetAt(i);
		if (pXMLExpRec)
		{
			if (
					pXMLExpRec->m_aPrimaryKeyValues == aPkSegments && 
					(strFileName.IsEmpty() || !pXMLExpRec->m_strFileName.CompareNoCase(strFileName)) &&
					(strProfileName.IsEmpty() || !pXMLExpRec->m_strProfileName.CompareNoCase(strProfileName))
				)
			{
				CString strChiave;
				for (int i = 0; i <= aPkSegments.GetUpperBound(); i++)
					strChiave += _T(" ") +aPkSegments.GetAt(i)->FormatData();

				TRACE("CXMLDocElement::GetProcessedRecord: record with key %ws of the table %ws, already processed\n", (LPCTSTR)strChiave, pRec->GetTableName() ); 

				return pXMLExpRec;
			}
		}					
	}

	return NULL;
}


//-----------------------------------------------------------------------------
BOOL CXMLDocElement::InsertExportedRecord(SqlRecord* pRec, const CString& strProfile)
{
	return InsertProcessedRecord(pRec, strProfile, _T(""));
}

//-----------------------------------------------------------------------------
BOOL CXMLDocElement::InsertFailedRecord(SqlRecord* pRec, const CString& strFile)
{
	return InsertProcessedRecord(pRec, _T(""), strFile);
}

//-----------------------------------------------------------------------------
BOOL CXMLDocElement::InsertProcessedRecord(SqlRecord* pRec, const CString& strProfileName, const CString& strFileName)
{
	if (!pRec) return FALSE;

	if (!m_pProcessedRecordArray) m_pProcessedRecordArray = new Array;

	CXMLRecordInfo* pXMLExpRec = new CXMLRecordInfo (pRec, strProfileName, strFileName);
	m_pProcessedRecordArray->Add(pXMLExpRec);
	m_pCurrentRecord = pXMLExpRec;
	return TRUE;
}


//----------------------------------------------------------------------------
void CXMLDocElement::SetFileReferences(long lBookmark, const CString &strFileName)
{ 
	if (m_pCurrentRecord) 
	{ 
		m_pCurrentRecord->m_strFileName = ::GetName(strFileName); 
		m_pCurrentRecord->m_lBookmark   = lBookmark;
	}
}


/////////////////////////////////////////////////////////////////////////////
// CXMLExportImportManager implementation
/////////////////////////////////////////////////////////////////////////////
//
// pDoc è il documento che da origine al processo di esportazione/importazione
//-----------------------------------------------------------------------------
CXMLExportImportManager::CXMLExportImportManager(CXMLDataManager* pDataManager)
:
	m_pEnvelopeMng			(NULL),
	m_pXMLDataExpImpArray	(NULL),
	m_pXMLDocumentArray		(NULL),
	m_pSmartXMLDiagnosticMng(NULL),
	m_pXMLDataManager		(pDataManager),
	m_bValidateOnParse		(FALSE),
	m_bCreateSchemaFiles	(FALSE),
	m_bCreateDataFiles		(TRUE),
	m_bLoggingEnabled		(TRUE),	
	m_bExistErrors			(FALSE),
	m_pCurrentDocElem		(NULL)
{
	ASSERT(pDataManager);
	m_LogSession.SetUseDialog();
}

//-----------------------------------------------------------------------------
CXMLExportImportManager::~CXMLExportImportManager()
{
	if (m_pEnvelopeMng)
		delete m_pEnvelopeMng;

	if (m_pXMLDataExpImpArray)
		delete m_pXMLDataExpImpArray;

	if (m_pXMLDocumentArray)
		delete m_pXMLDocumentArray;

	if (m_pSmartXMLDiagnosticMng)
		delete m_pSmartXMLDiagnosticMng;
	
	m_arProcessedNodes.RemoveAll();
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::InitXMLDocumentArray()
{
	if (m_pXMLDocumentArray)
		delete m_pXMLDocumentArray;
	m_pXMLDocumentArray = new Array;
}

//azzera il count dei record processati
//-----------------------------------------------------------------------------
void CXMLExportImportManager::InitRecordsProcessed()
{
	if (!m_pXMLDataExpImpArray)
		return;
	for (int i=0; i<m_pXMLDataExpImpArray->GetSize (); i++)
	{
		CXMLDocElement* pEl = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt (i);
		ASSERT(pEl);
		if (pEl)
			pEl->InitRecordsProcessed();
	}
}

//azzera la messaggistica

//-----------------------------------------------------------------------------
void CXMLExportImportManager::InitLogSpace(const CString& strName /*=""*/, const CString& strLogFolder /*=""*/)
{
	m_LogSession.Init(strName, strLogFolder);
}


//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::ShowLogSpaces (CXMLLogSpace::XMLMsgType *pRequiredType)
{
	return m_LogSession.ShowLogSpaces (pRequiredType);
}


//-----------------------------------------------------------------------------
void CXMLExportImportManager::FlushLogSpace()
{
	m_LogSession.FlushCurrentLogSpace();
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::OutputMessage
					(
						const CString&				strMessageToLog,
						CXMLLogSpace::XMLMsgType	eMsgType /*= CXMLLogSpace::XML_ERROR */,
						int							nCode	/*= 0*/,
						const CString&				strSource /*= _T("")*/
					) 
{
	if (eMsgType == CXMLLogSpace::XML_ERROR)
		m_bExistErrors = TRUE;

	if (m_bLoggingEnabled)
		return m_LogSession.AddMessage(strMessageToLog, eMsgType);
		
	if (m_pSmartXMLDiagnosticMng)
	{
		m_pSmartXMLDiagnosticMng->AddMessage(eMsgType, nCode, strSource, strMessageToLog);
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::OutputMessage
					(
						UINT						nMsgStringID,
						CXMLLogSpace::XMLMsgType	eMsgType /*= CXMLLogSpace::XML_ERROR */,
						int							nCode	/*= 0*/,
						const CString&				strSource /*= _T("")*/
					) 
{
	if (m_bLoggingEnabled)
		return m_LogSession.AddMessage(nMsgStringID, eMsgType);
		
	// nel caso di smart document non effettuo la tracciatura delle info ma salvo solo errori e warning
	if (m_pSmartXMLDiagnosticMng && eMsgType != CXMLLogSpace::XML_INFO)
	{
		m_pSmartXMLDiagnosticMng->AddMessage(eMsgType, nCode, strSource, cwsprintf(nMsgStringID));
		return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CXMLExportImportManager::RaiseLoggingLevel ()
{
	m_LogSession.RaiseLoggingLevel();
}

//----------------------------------------------------------------------------
void CXMLExportImportManager::LowerLoggingLevel ()
{
	m_LogSession.LowerLoggingLevel();
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::AppendMessageDetail
					(
						const CString&	strMessageToLog
					) 
{
	return m_bLoggingEnabled ? m_LogSession.AppendDetail(strMessageToLog) : FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::AppendMessageDetail
					(
						UINT	nMsgStringID
					) 
{
	return m_bLoggingEnabled ? m_LogSession.AppendDetail(nMsgStringID) : FALSE;
}

//----------------------------------------------------------------------------
void CXMLExportImportManager::UseSmartXMLDiagnosticMng()
{
	//disabilito l'utilizzo del log
	EnableLogging(FALSE);
	//se non ancora istanziata creo la diagnostica
	if (!m_pSmartXMLDiagnosticMng)
		m_pSmartXMLDiagnosticMng = new CSmartXMLDiagnosticMng();
}

//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::Init(CAbstractFormDoc* pDoc)
{
	if (!pDoc)
		return FALSE;

	if (m_pXMLDataExpImpArray)
		delete m_pXMLDataExpImpArray;


	m_bExistErrors = FALSE;
	m_pXMLDataExpImpArray = new Array;

	if (m_pEnvelopeMng)
		delete m_pEnvelopeMng;

	m_pEnvelopeMng = new CXMLEnvelopeManager(pDoc);
						
	InitXMLDocumentArray();

	if (!pDoc->GetXMLDocInfo())
	{
		ASSERT(FALSE);
		return FALSE;
	}


	// creo il primo CXMLDocElement e lo metto come corrente
	CXMLDocElement* pDocElem = new CXMLDocElement(pDoc);
	m_pXMLDataExpImpArray->Add(pDocElem);
	SetCurrentDocElem(pDocElem);

	if (m_pXMLDataManager && m_pXMLDataManager->GetXMLExportDocSelection())
	{
		m_bCreateSchemaFiles = !m_pXMLDataManager->GetXMLExportDocSelection()->IsOnlyDocToExport();
		m_bCreateDataFiles = m_pXMLDataManager->GetXMLExportDocSelection()->IsOnlyDocToExport() || 
							 m_pXMLDataManager->GetXMLExportDocSelection()->IsDocAndSchemaToExport();
	}

	return TRUE;
}
//-----------------------------------------------------------------------------
void CXMLExportImportManager::CreateEnvelope(BOOL bDisplayMsgBox /*= TRUE*/)
{
	if (m_pEnvelopeMng) 
		m_pEnvelopeMng->CreateEnvelope(bDisplayMsgBox, _T(""), m_bCreateSchemaFiles); 
}

//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::DropEnvelope()
{
	return (m_pEnvelopeMng) 
			? m_pEnvelopeMng->DropEnvelope()
			: FALSE; 
}



//-----------------------------------------------------------------------------
void CXMLExportImportManager::GetDocsInEditMode(const CTBNamespace& aNameSpace, Array& arDocElement)
{
	arDocElement.SetOwns (FALSE);
	arDocElement.RemoveAll ();

	if (!aNameSpace.IsValid() || !m_pXMLDataExpImpArray)
		return;

	CXMLDocElement* pDocElem = NULL;
	for (int i = 0; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);
		if (pDocElem											&& 
			pDocElem->m_pDocument								&&
			pDocElem->m_pDocument->GetNamespace() == aNameSpace	&&
			pDocElem->m_pDocument->GetFormMode() == CBaseDocument::EDIT )
		{
			arDocElement.Add (pDocElem->m_pDocument);
		}
	}
}

//-----------------------------------------------------------------------------
CXMLDocElement* CXMLExportImportManager::GetXMLDocElement(const CTBNamespace& aNameSpace, CBaseDocument* pAncestor, BOOL bCanRunOnlyBusinessObject /*= FALSE*/)
{
	if (!aNameSpace.IsValid() || !m_pXMLDataExpImpArray)
		return NULL;

	CXMLDocElement* pDocElem = NULL;

	for (int i = -1; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (i == -1) ? m_pCurrentDocElem : (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);

		if (pDocElem																			&& 
			pDocElem->m_pDocument																&&
			pDocElem->m_pDocument->GetNamespace() == aNameSpace									&&
			pDocElem->m_pDocument->GetXMLDataManager()											&&
			!((CXMLDataManager*) pDocElem->m_pDocument->GetXMLDataManager())->m_bBusy	&&	//il documento è impegnato
			pDocElem->m_pDocument->GetFormMode() != CBaseDocument::NEW							&&
			pDocElem->m_pDocument->GetFormMode() != CBaseDocument::EDIT )
		{
			return pDocElem;
		}
	}
	// se non esiste lo crea 
	pDocElem = new CXMLDocElement(aNameSpace, pAncestor, bCanRunOnlyBusinessObject);
	if (!pDocElem->m_pDocument)
	{
		delete pDocElem;
		return NULL;
	}
	CAbstractFormDoc* pExtRefDocument = pDocElem->m_pDocument;

	m_pXMLDataExpImpArray->Add(pDocElem);

	//initialize documet for import/export procedure
	if (pAncestor)
	{
		pExtRefDocument->m_pExternalControllerInfo = pAncestor->m_pExternalControllerInfo;	

		// se sto importando, devo attivare il caching dei counter
		if (pAncestor->IsImporting())
			pDocElem->m_pDocument->EnableCounterCaching(TRUE);

		CXMLDataManager* pAncestorXMLDataMng = (CXMLDataManager*)pAncestor->GetXMLDataManager();
		CXMLDataManager* pDocXMLDataMng = (CXMLDataManager*)pExtRefDocument->GetXMLDataManager();
	
		ASSERT(pAncestorXMLDataMng);
		ASSERT(pDocXMLDataMng);
		
		pDocXMLDataMng->SetStatus(pAncestorXMLDataMng->GetStatus());
		pDocXMLDataMng->m_pXMLExpImpMng		= pAncestorXMLDataMng->m_pXMLExpImpMng;
		pDocXMLDataMng->m_pXMLSmartParam	= pAncestorXMLDataMng->m_pXMLSmartParam;
		pDocXMLDataMng->m_bExpDelRecord		= pAncestorXMLDataMng->m_bExpDelRecord;
		pDocXMLDataMng->m_bErrorFound		= pAncestorXMLDataMng->m_bErrorFound;
		pDocXMLDataMng->m_pSmartProfile		= pAncestorXMLDataMng->m_pSmartProfile;
		pDocXMLDataMng->m_bIsExtRef			= TRUE; 
		pDocXMLDataMng->m_bIsPostBack		= pAncestorXMLDataMng->m_bIsPostBack;
		pDocXMLDataMng->m_pObserverContext  = pAncestorXMLDataMng->m_pObserverContext;
		pDocXMLDataMng->m_bTuningEnable		= pAncestorXMLDataMng->m_bTuningEnable;

		//Optimization
		if (!pDocXMLDataMng->UseOldXTechMode() && pAncestor->IsImporting())
		{
			CXMLEventManager* pMng = NULL;
			if ((CXMLEventManager*)pExtRefDocument->m_pEventManager  && pExtRefDocument->m_pEventManager->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
				pMng = (CXMLEventManager*)pExtRefDocument->m_pEventManager;
	
			if (!AfxIsInUnattendedMode()) //nel caso di MagicLink non effettuo l'ottimizzazione
				pDocXMLDataMng->PrepareRequiredTabDlgs(); //prepare the tabdialog only it is not a deleted record importing
			
			pDocXMLDataMng->RegisterObservableDataField();
			
			//inizializzo l'XMLEventManager del Externalreference document
			if (pExtRefDocument->m_pEventManager  && pExtRefDocument->m_pEventManager->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
				((CXMLEventManager*)pExtRefDocument->m_pEventManager)->InitTablesEvents();				
		}
	}

	return pDocElem;
}

//restituisce l'oggetto CXMLDocElement associato a pDoc (se c'e')
//-----------------------------------------------------------------------------
CXMLDocElement* CXMLExportImportManager::GetXMLDocElement(CAbstractFormDoc* pDoc)
{
	if (!m_pXMLDataExpImpArray)
		return NULL;

	if (!pDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	CXMLDocElement* pDocElem = NULL;

	for (int i = 0; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);
		if (pDocElem && pDocElem->m_pDocument == pDoc)
			return pDocElem;
	}

	return NULL;
}

//controllo se ho gia' in memoria il DOM associato al file
//se si, restituisco quello, altrimenti ne creo uno nuovo
//-----------------------------------------------------------------------------
CXMLDocumentObject* CXMLExportImportManager::GetXMLImportDomDocument
										(
											const CString& strFileName, 
											const CString& strPath, 
											BOOL bTransform /*=FALSE*/,
											BOOL bAvoidValidation /*=FALSE*/
										)
{
	if (strFileName.IsEmpty() || !m_pXMLDocumentArray)
	{
		OutputMessage(_TB("Internal procedure error."));
		return NULL;
	}

	CString strFileRootName = GetExtension(strFileName).IsEmpty() ? 
															strFileName + szXmlExt : 
															strFileName;
	if (IsRelativePath(strFileRootName))
		strFileRootName = strPath + strFileRootName;
	
	CQualifiedXMLDocument* pQualifiedDoc = NULL;

	for (int i = 0; i <= m_pXMLDocumentArray->GetUpperBound(); i++)
	{
		pQualifiedDoc = (CQualifiedXMLDocument*) m_pXMLDocumentArray->GetAt(i);
		if (pQualifiedDoc && !pQualifiedDoc->m_strFileName.CompareNoCase (strFileRootName))
		{
			return pQualifiedDoc->m_pXMLDomDoc;
		}
	}
	
	if (!::ExistFile(strFileRootName))
	{
		OutputMessage(cwsprintf(_TB("File {0-%s} not found."), strFileRootName), CXMLLogSpace::XML_WARNING);
		return NULL;
	}

	// se non esiste lo crea 
	CXMLDocumentObject* pDoc = new CXMLDocumentObject(FALSE, FALSE);
	pDoc->SetValidateOnParse(FALSE); //devo validare solo dopo che ho apportato l'eventuale trasformazione XSLT
	if (!pDoc->LoadXMLFile(strFileRootName))
	{
		OutputMessage (cwsprintf(_TB("Error loading the file: {0-%s}."), strFileRootName));
		CString strError;
		if (pDoc->GetParseErrorString(strError) && !strError.IsEmpty())
			AppendMessageDetail (strError);
		delete pDoc;
		return NULL;
	}

	//if (bTransform && !ApplyImportXSLT(pDoc))
	//	OutputMessage(cwsprintf(_TB("The file {0-% n} was not properly converted with the XSLT sheet."), strFileRootName));	
	
	if (m_bValidateOnParse && !bAvoidValidation && !Validate(pDoc, strFileName))
	{
		delete pDoc;
		return NULL;
	}

	m_pXMLDocumentArray->Add(new CQualifiedXMLDocument(strFileRootName, pDoc));

	return pDoc;
}

//----------------------------------------------------------------------------
void CXMLExportImportManager::AddProcessedNode(const CString& strFileName, const CString& strBookmark, CXMLNode* pNode, BOOL bSucceded)
{
	m_arProcessedNodes.Add(new CProcessedNode(strFileName, strBookmark, pNode, bSucceded));
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::IsProcessedNode(const CString& strFileName, const CString& strBookmark, BOOL& bImportResult) const
{
	bImportResult = FALSE;
	for (int i = 0; i <m_arProcessedNodes.GetSize(); i++)
	{
		CProcessedNode* pProcessedNode = (CProcessedNode*)m_arProcessedNodes[i];
		if (!pProcessedNode) continue;

		if (
			!strFileName.CompareNoCase(pProcessedNode->m_strFileName) &&
			!strBookmark.CompareNoCase(pProcessedNode->m_strBookmark)
			)
		{
			bImportResult = TRUE;
			return TRUE;
		}
	}

	return FALSE;

}

// se tutto è andato a buon fine, elimina i nodi dai DOM
// altrimenti li segna come 'da processare' e lo elimina dai nodi processati
//----------------------------------------------------------------------------
void CXMLExportImportManager::ManageProcessedNodes(BOOL bSuccess)
{
	for (int i = m_arProcessedNodes.GetUpperBound(); i >= 0; i--)
	{
		CProcessedNode* pProcessedNode = (CProcessedNode*)m_arProcessedNodes[i];
		if (!pProcessedNode) continue;
		
		CXMLNode* pNode = pProcessedNode->m_pNode;
		if (!pNode)	continue;

		if (!bSuccess)
		{
			pNode->SetAttribute(DOC_XML_PROCESSED_ATTRIBUTE, FormatBoolForXML(FALSE));
			m_arProcessedNodes.RemoveAt(i);	
			continue;
		}

		CXMLNode* pMastersNode = pNode->GetParentNode();

		pMastersNode->RemoveChild(pNode);

		CString strDataInstances;
		int nDataInstancesNum = 
		(
			pMastersNode->GetAttribute(DOC_XML_DATAINSTANCES_ATTRIBUTE, strDataInstances) 
			&& !strDataInstances.IsEmpty()
		) 
			? _ttoi((LPCTSTR)strDataInstances)
			: 1;

		// correggo il nome delle istanze per l'eventuale backup del DOM
		nDataInstancesNum--;
		strDataInstances.Format (_T("%d"), nDataInstancesNum);
		pMastersNode->SetAttribute(DOC_XML_DATAINSTANCES_ATTRIBUTE, strDataInstances);
		
		delete pNode;
		pProcessedNode->m_pNode = NULL;
	}
}


//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::Validate(CXMLDocumentObject* pXMLDocObj, const CString strFileName)
{	
	if (!pXMLDocObj) 
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strSchemaFile;
	if (GetName(strFileName).CompareNoCase (ENV_XML_FILE_NAME) == 0)
	{
		strSchemaFile = MakeFilePath(GetXMLSchemasPath(), MakeSchemaName(strFileName));
		if (!::ExistFile(strSchemaFile))
			m_pEnvelopeMng->m_aXMLEnvInfo.CreateEnvelopeSchema(strSchemaFile, FALSE);
	}
	else
	{

		CXMLNode* pDocInfoNode = pXMLDocObj->GetRootChildByName(DOC_XML_DOCINFO_TAG);
		if (!pDocInfoNode)
		{
			OutputMessage (cwsprintf(_TB("Validation failed for the file: {0-%s}."), strFileName));
			return FALSE;
		}

		CString strNameSpace;
		CXMLNode *pNode = pDocInfoNode->GetChildByName(XML_NAMESPACE_TAG);
		if (pNode)
			pNode->GetText(strNameSpace);
		
		CString strProfileName;
		pNode = pDocInfoNode->GetChildByName(DOC_XML_PROFILE_TAG);
		if (pNode)
			pNode->GetText(strProfileName);
	
		CTBNamespace aNameSpace(strNameSpace);
		// se il profilo non esiste, provo con la descrizione del documento
		if (!strProfileName.IsEmpty() && !ExistProfile(aNameSpace, strProfileName))
		{
			OutputMessage(
				cwsprintf(_TB("Profile {0-%s} not found; the document description will be used to generate the schema file."), strProfileName),
				CXMLLogSpace::XML_WARNING);
			strProfileName.Empty();
		}

		CXMLProfileInfo aProfileInfo(aNameSpace, strProfileName);
		strSchemaFile = GetSchemaProfileFile(&aProfileInfo);
		
		if (!aNameSpace.IsValid()) 
		{
			OutputMessage (cwsprintf(_TB("Validation failed for the file: {0-%s}."), strFileName));
			return FALSE;
		}
		
		if (!::ExistFile(strSchemaFile))
		{
			CXMLDocElement* pDocElement = GetXMLDocElement(aNameSpace, m_pXMLDataManager->GetDocument());
			if (pDocElement && pDocElement->m_pDocument)
				m_pXMLDataManager->CreateXMLSchemaFile(strProfileName, pDocElement->m_pDocument);
		}
	}

	OutputMessage(cwsprintf(_TB("Validation of the file {0-%s} using the schema file: {1-%s}."), strFileName, strSchemaFile), CXMLLogSpace::XML_INFO);

	CString strError;
	if (!pXMLDocObj->Validate(strError, XTECH_NAMESPACE, strSchemaFile))
	{
		OutputMessage (cwsprintf(_TB("Validation failed for the file: {0-%s}."), strFileName));
		if (!strError.IsEmpty())
			AppendMessageDetail (strError);
		return FALSE;
	}

	return TRUE;
}
// se ho la necessita di utilizzare i fogli di trasformazione in export
// vedi problema documenti di riferimento: iddoc e namespace
//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::ApplyAndSaveTransformXSLT()
{
	CXMLDocElement* pDocElem;
	BOOL bOk = TRUE;
	for (int i = 0; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);
		if (pDocElem)
		 	bOk = pDocElem->ApplyAndSaveTransformXSLT() && bOk;
	}
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLExportImportManager::ApplyImportXSLT(CXMLDocumentObject*  pXMLDocObj)
{
	if (!pXMLDocObj) 
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}

	//cerco "//DocumentInfo/Namespace"
	CString strPrefix = GET_NAMESPACE_PREFIX(pXMLDocObj);
	CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + strPrefix + DOC_XML_DOCINFO_TAG + URL_SLASH_CHAR + strPrefix + XML_NAMESPACE_TAG;

	CXMLNode *pNode = pXMLDocObj->SelectSingleNode(strFilter, strPrefix);
	if (!pNode)
	{
		OutputMessage(_TB("Error during the XSLT conversion: the content of the XML source document is not consistent."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strNameSpace;
	pNode->GetText(strNameSpace);
	
	delete pNode;

	//ottengo il documento associato al namespace (lui sa come fare la trasformazione)
	
	CTBNamespace aNameSpace(strNameSpace);
	if (!aNameSpace.IsValid())
	{
		OutputMessage(_TB("Error during the XSLT conversion: the content of the XML source document is not consistent."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	CXMLDocElement *pDocEl = GetXMLDocElement(aNameSpace, m_pXMLDataManager->GetDocument());
	if (!pDocEl)
	{
		OutputMessage(_TB("Error during the XSLT conversion: the content of the XML source document is not consistent."));
		ASSERT(FALSE);
		return FALSE;
	}

	CXSLTManager XSLTManager(pDocEl->m_pDocument);
	if (XSLTManager.TransformForImport (pXMLDocObj))
		*pXMLDocObj = *XSLTManager.GetOutput();
	
	return TRUE;
}

// mi permette di controllare se il record è stato già esportato
//-----------------------------------------------------------------------------
CXMLRecordInfo* CXMLExportImportManager::GetExportedRecord(SqlRecord* pRec, const CString& strProfile)
{
	if (!pRec || !m_pCurrentDocElem)
		return NULL;

	return m_pCurrentDocElem->GetExportedRecord(pRec, strProfile);
}

// mi permette di controllare se il record è stato già importato
//-----------------------------------------------------------------------------
CXMLRecordInfo* CXMLExportImportManager::GetImportedRecord(SqlRecord* pRec, const CString& strFile)
{
	if (!pRec || !m_pCurrentDocElem)
		return NULL;

	return m_pCurrentDocElem->GetImportedRecord(pRec, strFile);
}

// inserisce il record esportato
//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::InsertExportedRecord(SqlRecord* pRec, const CString& strProfile)
{
	if (!pRec) return FALSE;

	if (!m_pCurrentDocElem) return FALSE; // non posso inserire

	return m_pCurrentDocElem->InsertExportedRecord(pRec, strProfile);
}

// inserisce il record importato
//-----------------------------------------------------------------------------
BOOL CXMLExportImportManager::InsertFailedRecord(SqlRecord* pRec, const CString& strFile)
{
	if (!pRec) return FALSE;

	if (!m_pCurrentDocElem) return FALSE; // non posso inserire

	return m_pCurrentDocElem->InsertFailedRecord(pRec, strFile);
}


//-----------------------------------------------------------------------------
void CXMLExportImportManager::SetCurrentDocElem(CXMLDocElement* pXMLDocElem)
{
	if (!pXMLDocElem) return;
	m_pCurrentDocElem = pXMLDocElem;
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::SetCurrentDocElem(const CTBNamespace& aNameSpace)
{
	CXMLDocElement* pDocElement = GetXMLDocElement(aNameSpace, m_pXMLDataManager->GetDocument());
	if (!pDocElement)
	{
		ASSERT(FALSE);
		return;
	}
	SetCurrentDocElem(pDocElement);
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::SetCurrentDocElem(CAbstractFormDoc* pDoc)
{
	CXMLDocElement* pDocElement = GetXMLDocElement(pDoc);
	if (!pDocElement)
	{
		ASSERT(FALSE);
		return;
	}
	SetCurrentDocElem(pDocElement);
}	

//-----------------------------------------------------------------------------
void CXMLExportImportManager::SetCurrentRecord(CXMLRecordInfo* pXMLRec)
{
	if (!m_pCurrentDocElem)
		return;

	m_pCurrentDocElem->SetCurrentRecord(pXMLRec);
}

//-----------------------------------------------------------------------------
long CXMLExportImportManager::GetNextBookmark()
{ 
	return (
				m_pCurrentDocElem && 
				m_pCurrentDocElem->m_pDocument &&
				!m_pCurrentDocElem->m_pDocument->GetNamespace().ToString().IsEmpty() &&
				!m_pCurrentDocElem->m_strCurrFileName.IsEmpty()
			)
			? m_pEnvelopeMng->GetNextBookmark(
												m_pCurrentDocElem->m_pDocument->GetNamespace().ToString(),
												m_pCurrentDocElem->m_strCurrFileName
											)
			: 0;
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::SetFileReferences(long lBookmark, const CString &strFileName) 
{ 
	if (m_pCurrentDocElem  && !strFileName.IsEmpty()) 
		m_pCurrentDocElem->SetFileReferences(lBookmark, strFileName); 
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::SaveEvents(Array* pChanges)
{ 
	CXMLDocElement* pDocElem = NULL;

	for (int i = 0; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);
		if (
				pDocElem && pDocElem->m_pDocument &&
				pDocElem->m_pDocument->m_pEventManager  && 
				pDocElem->m_pDocument->m_pEventManager->IsKindOf(RUNTIME_CLASS(CXMLEventManager))
			)
			((CXMLEventManager*)pDocElem->m_pDocument->m_pEventManager)->SaveEvents(pChanges);
	}
}

//-----------------------------------------------------------------------------
void CXMLExportImportManager::InitTablesEvents()
{
	CXMLDocElement* pDocElem = NULL;

	for (int i = 0; i <= m_pXMLDataExpImpArray->GetUpperBound(); i++)
	{
		pDocElem = (CXMLDocElement*) m_pXMLDataExpImpArray->GetAt(i);
		if (
				pDocElem && pDocElem->m_pDocument &&
				pDocElem->m_pDocument->m_pEventManager  && 
				pDocElem->m_pDocument->m_pEventManager->IsKindOf(RUNTIME_CLASS(CXMLEventManager))
			)
			((CXMLEventManager*)pDocElem->m_pDocument->m_pEventManager)->InitTablesEvents();
	}
}

///////////////////////////////////////////////////////////////////////////////
//							CXMLRowFilters
///////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------------------------
void CXMLRowFilters::Clear ()
{
	m_sXRefTableName.Empty();
	m_RowFilters.RemoveAll();
}

//----------------------------------------------------------------------------------------------
CXMLRowFilters& CXMLRowFilters::operator =(const CXMLRowFilters& aFilterRows)
{
	if (this == &aFilterRows)
		return *this;
	
	m_sXRefTableName	= aFilterRows.m_sXRefTableName;
	m_RowFilters		= aFilterRows.m_RowFilters;

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//METODI COMUNI PROCEDURE IMPORT/EXPORT DELLA CLASSE CXMLDataManager
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CXMLDataManager, CXMLDataManagerObj)

//----------------------------------------------------------------------------
CXMLDataManager::CXMLDataManager(CBaseDocument* pDoc, short nMode /*= XMLDATAMNG_MODE_DEFAULT*/)
	:
	m_pDoc					(NULL),
	m_pExportDocSelection	(NULL),
	m_pXMLExpImpMng			(NULL),
	m_bIsExtRef				(FALSE),
	m_bIsRootDoc			(FALSE),
	m_bIsPostBack			(FALSE),
	m_bContinueExpImp		(FALSE),
	m_nDataInstancesNumb	(0),
	m_nMode					(nMode),
	m_pWizardDocument		(NULL),
	m_bImportPendingData	(FALSE),
	m_bExpDelRecord			(FALSE),
	m_nDocsImported			(0),
	m_nDocsFailed			(0),
	m_bImportDownload		(FALSE),
	m_bImportValidate		(FALSE),
	m_bErrorFound			(FALSE),
	m_bBusy					(FALSE),
	m_pXMLSmartParam		(NULL),
	m_pSmartProfile			(NULL),
	m_pObserverContext		(NULL),
	m_pTableEvents			(NULL),
	m_bTuningEnable			(FALSE),
	m_bForceUseOldXTechMode	(FALSE),
	m_bCachedDocumentBusy	(false)

{
	m_nStatus = XML_MNG_IDLE;	
	m_nProcessStatus = XML_PROCESS_IDLE;

	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		m_pDoc = (CAbstractFormDoc*)pDoc;
		if (m_pDoc->IsRunningFromExternalController())
			SetUnattendedMode();
	}

	m_sLastEventsTableName.Empty();
}

//----------------------------------------------------------------------------
CXMLDataManager::~CXMLDataManager()
{
	if (m_pExportDocSelection)
		delete m_pExportDocSelection;

	// il gestore dell'export/import è di proprietà del root-document. I documenti istanziati da
	// external reference utizzano gestore dell'export/import del root document.
	// anche lo SmartProfile è di proprietà del root-document. Bug 12188
	if (m_bIsRootDoc)
	{
		if (m_pXMLExpImpMng)
			delete m_pXMLExpImpMng;
		
		if (m_pSmartProfile)
			delete m_pSmartProfile;

		if (m_pObserverContext)
			delete m_pObserverContext;
	}

	m_RowFilters.Clear();
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateXMLExpImpManager()
{
	if (!m_pDoc || !m_pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (m_pXMLExpImpMng)
		delete m_pXMLExpImpMng;

	m_pXMLExpImpMng = new CXMLExportImportManager(this);
	
	return m_pXMLExpImpMng->Init(m_pDoc);
}

//----------------------------------------------------------------------------
void CXMLDataManager::WaitWizardDocument()
{
	if (!m_pWizardDocument)
		return;
	ASSERT_VALID(m_pWizardDocument);
	m_pWizardDocument->WaitDocument();
	m_pWizardDocument = NULL;
}

//----------------------------------------------------------------------------
void CXMLDataManager::SetUnattendedMode (BOOL bSet /*= TRUE*/)
{
	if (bSet)
	{
		m_nMode &= ~XMLDATAMNG_MODE_DISPLAY_MSGBOXES; 
		m_nMode |= XMLDATAMNG_MODE_UNATTENDED; 
	}
	else
		m_nMode &= ~XMLDATAMNG_MODE_UNATTENDED; 
}

//-----------------------------------------------------------------------------
BOOL CXMLDataManager::IsNotEmptyDataObj(DataObj* pDataObj) 
{
	return (
				pDataObj &&
				(
					!pDataObj->IsEmpty() ||
					pDataObj->GetDataType() == DATA_ENUM_TYPE ||  
					pDataObj->GetDataType() == DATA_BOOL_TYPE  
				)
			);
}

//----------------------------------------------------------------------------
CString CXMLDataManager::GetDocTitle() const
{
	if (!m_pDoc)
	{
		ASSERT(FALSE);
		return _T("");
	}
	return m_pDoc->GetTitle();
}

//----------------------------------------------------------------------------
CTBNamespace CXMLDataManager::GetDocumentNamespace() const
{
	if (!m_pDoc)
	{
		ASSERT(FALSE);
		return CTBNamespace();
	}
	return m_pDoc->GetNamespace();
}


//----------------------------------------------------------------------------
CString	CXMLDataManager::GetProfileName() const
{
	if (m_pSmartProfile)
		return m_pSmartProfile->m_strProfileName;

	return _T("");
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::OutputMessage
					(
						const CString&				strMessageToLog,
						CXMLLogSpace::XMLMsgType	eMsgType /*= CXMLLogSpace::XML_ERROR */,
						int							nCode  /*= 0*/, 
						const CString&				strSource  /*= _T("")*/
					) const
{
	return m_pXMLExpImpMng ? m_pXMLExpImpMng->OutputMessage(strMessageToLog, eMsgType, nCode, strSource) : FALSE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::RaiseLoggingLevel ()
{
	if (!m_pXMLExpImpMng)
	{
		ASSERT(FALSE);
		return;
	}
	
	m_pXMLExpImpMng->RaiseLoggingLevel();
}

//----------------------------------------------------------------------------
void CXMLDataManager::LowerLoggingLevel ()
{
	if (!m_pXMLExpImpMng)
	{
		ASSERT(FALSE);
		return;
	}
	
	m_pXMLExpImpMng->LowerLoggingLevel();
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::OutputMessage
					(
						UINT						nMsgStringID,
						CXMLLogSpace::XMLMsgType	eMsgType /*= CXMLLogSpace::XML_ERROR */,
						int							nCode  /*= 0*/, 
						const CString&				strSource  /*= _T("")*/
					) const
{
	return m_pXMLExpImpMng ? m_pXMLExpImpMng->OutputMessage(nMsgStringID, eMsgType, nCode, strSource) : FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::AppendMessageDetail
					(
						const CString&	strMessageToLog
					) const
{
	return m_pXMLExpImpMng ? m_pXMLExpImpMng->AppendMessageDetail(strMessageToLog) : FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::AppendMessageDetail
					(
						UINT	nMsgStringID
					) const
{
	return m_pXMLExpImpMng ? m_pXMLExpImpMng->AppendMessageDetail(nMsgStringID) : FALSE;
}
//----------------------------------------------------------------------------
void CXMLDataManager::BeginListeningToDoc (CAbstractFormDoc *pDoc)
{
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}

	CDiagnostic* pDiagnostic = pDoc->GetMessages ();
	ASSERT(pDiagnostic);
	pDiagnostic->StartSession ();
}

//----------------------------------------------------------------------------
void CXMLDataManager::EndListeningToDoc (CAbstractFormDoc *pDoc, BOOL bGetMsg /*= TRUE*/)
{
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}

	CDiagnostic* pDiagnostic = pDoc->GetMessages ();
	if (pDiagnostic->MessageFound ())
	{
		if (bGetMsg)
		{
			CXMLLogSpace::XMLMsgType aMsgType;
			if (pDiagnostic->ErrorFound ())
				aMsgType = CXMLLogSpace::XML_ERROR;
			else if (pDiagnostic->WarningFound ())
				aMsgType = CXMLLogSpace::XML_WARNING;
			else
				aMsgType = CXMLLogSpace::XML_INFO;

			OutputMessage(
				cwsprintf(_TB("Messages reported by the document {0-%s}"), (LPCTSTR)pDoc->GetTitle()),
				aMsgType);
			for (int i=0; i<=pDiagnostic->GetUpperBound (); i++)
				AppendMessageDetail(pDiagnostic->GetMessageLine (i));
		}
		pDiagnostic->ClearMessages(TRUE);
	}	
	
	pDiagnostic->EndSession ();

	
}
 
//---------------------------------------------------------------------------
BOOL CXMLDataManager::IsDocumentEmpty(CXMLDocumentObject *pDOMDoc)
{
	if (!pDOMDoc) 
		return TRUE;
	
	CString strPrefix = GET_NAMESPACE_PREFIX(pDOMDoc);
	CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + strPrefix + DOC_XML_MASTER_TAG;
	CXMLNode *pNode = pDOMDoc->SelectSingleNode(strFilter, strPrefix); 
	return pNode ? delete pNode, FALSE : TRUE;

}

//----------------------------------------------------------------------------
CPropertyPage* CXMLDataManager::CreateProfilesWizardPropPage(const CTBNamespace& aNameSpace) const
{
	m_pDoc->LoadXMLDescription();
	return new CProfiliMngWizPage(aNameSpace, m_pDoc->GetXMLDocInfo());
}


//---------------------------------------------------------------------------
BOOL CXMLDataManager::GetUKCommonFunctionList(CStringArray* pUKFuncList) const
{
	if (!pUKFuncList)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bLostAndFound = FALSE, bAlternativeSearch = FALSE;
	CString tempStr;
	for (int i=0; !(bLostAndFound && bAlternativeSearch) && i<pUKFuncList->GetSize (); i++)
	{
		tempStr = pUKFuncList->GetAt (i);
		if (tempStr == szLostAndFound)
			bLostAndFound = TRUE;
		else if (tempStr == szAlternativeSearch)
			bAlternativeSearch = TRUE;
	}
		
	if (!bLostAndFound)
		pUKFuncList->Add (szLostAndFound);
	if (!bAlternativeSearch)
		pUKFuncList->Add (szAlternativeSearch);

	return TRUE;
}

//---------------------------------------------------------------------------
///verifico se almeno una universal key dell'array è presente 
// quindi riordino il nodo delle universal key in base alla priorità
// assegnata (eventualmente rimuovendo quelle che non devo considerare)
BOOL CXMLDataManager::RearrangeUniversalKey (CXMLNode *pUniversalKey, const CStringArray &strArray)
{
	if (!pUniversalKey)
		return FALSE;
	
	CString strPrefix = GET_NAMESPACE_PREFIX(pUniversalKey);
	CXMLNodeChildsList *pUnKeyList = pUniversalKey->SelectNodes (strPrefix + XML_UNIVERSAL_KEY_TAG, strPrefix);
	if (!pUnKeyList)
		return FALSE;

	CXMLNode *pNode = NULL;
	for (int j=0; j<pUnKeyList->GetSize(); j++)
	{
		pNode = pUnKeyList->GetAt(j);
		pUniversalKey->RemoveChild(pNode);
	}

	BOOL bResult = FALSE;

	for (int i=0; i<strArray.GetSize(); i++)
	{
		CString strName = strArray.GetAt (i);
		CString strUKName;
		for (int j=0; j<pUnKeyList->GetSize(); j++)
		{
			pNode = pUnKeyList->GetAt(j);
			if (pNode && pNode->GetAttribute (XML_UNIVERSAL_KEY_NAME_ATTRIBUTE, strUKName))
			{
				if (strName.CompareNoCase (strUKName) == 0)
				{
					pUniversalKey->AppendChild (pNode);
					pUnKeyList->RemoveAt (j);
					bResult = TRUE;
					break;
				}
			}	
		}		
	}

	SAFE_DELETE(pUnKeyList);

	return bResult;
}

//----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema 
//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateXMLSchemaFile
							(
								const CString&			strProfileName,
								const CTBNamespace&		aDocumentNameSpace
							) 
{
	if (!m_pXMLExpImpMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLDocElement *pDocElement = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc);
	if (!pDocElement || !pDocElement->m_pDocument)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	return CreateXMLSchemaFile(strProfileName, pDocElement->m_pDocument);
}

//----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema
//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateXMLSchemaFile
							(
								const CString&			strProfileName,
								CAbstractFormDoc*		pDoc								
							) 
{
	if (!pDoc)
		pDoc = m_pDoc;

	if (!pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLProfileInfo Info (pDoc->GetNamespace(), (LPCTSTR)strProfileName);
	return Info.LoadAllFiles() && CreateXMLSchemaFile(&Info, pDoc);

}



//----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema
//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateXMLSchemaFile
							(
								CXMLProfileInfo*	pProfileInfo,
								CAbstractFormDoc*	pDoc
							) 
{
	if (!pProfileInfo)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!pDoc)
		pDoc = m_pDoc;

	if (!pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DBTMaster *pDBTMaster = pDoc->m_pDBTMaster;
	DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
	int nDBTSlaveBuff = 0;
	int nDBTSlave = 0;
	
	CString strSchemaFile = GetSchemaProfileFile(pProfileInfo);
	if (strSchemaFile.IsEmpty())
		return FALSE;


	// altrimenti lo genero
	CXSDGenerator XMLSchema(XTECH_NAMESPACE, IsDisplayingMsgBoxesEnabled());

	XMLSchema.InsertElement(DOC_XML_DOCUMENT_TAG, DOC_XML_DOCUMENT_TAG);

	XMLSchema.BeginComplexType(DOC_XML_DOCUMENT_TAG);
		XMLSchema.InsertAttribute	(DOC_XML_DOCNAME_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_DOCINFO_TAG,			DOC_XML_DOCINFO_TAG);
		XMLSchema.InsertElement		(DOC_XML_DOCUMENTS_TAG,			DOC_XML_DOCUMENTS_TAG);
	XMLSchema.EndComplexType();

// DOC_XML_DOCINFO_TAG e derivati////////////////////////////////////////////////////////////////////
	XMLSchema.BeginComplexType(DOC_XML_DOCINFO_TAG); 
		XMLSchema.InsertElement		(DOC_XML_NAMESPACE_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), _T(""), pDoc->GetNamespace().ToString());
		XMLSchema.InsertElement		(DOC_XML_PROFILE_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_TITLE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_DESCRIPTION_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_CREATION_TAG,			DOC_XML_CREATION_TAG);
		XMLSchema.InsertElement		(DOC_XML_NEXTFILE_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("0"));
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_CREATION_TAG);
		XMLSchema.InsertElement		(DOC_XML_DOMAIN_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_SITE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_USER_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(DOC_XML_DATETIME_TAG,			SCHEMA_XSD_DATATYPE_DATETIME_VALUE);
	XMLSchema.EndComplexType();

// DOC_XML_DOCUMENTS_TAG e derivati////////////////////////////////////////////////////////////////////
	XMLSchema.BeginComplexType(DOC_XML_DOCUMENTS_TAG);
		XMLSchema.InsertElement		(DOC_XML_MASTERS_TAG,			DOC_XML_MASTERS_TAG);
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_MASTERS_TAG);
		XMLSchema.InsertAttribute	(DOC_XML_DATAINSTANCES_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_INTEGER_VALUE);
		XMLSchema.InsertAttribute	(DOC_XML_TABLE_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pDBTMaster->GetTable()->GetTableName());
		XMLSchema.InsertAttribute	(DOC_XML_NAMESPACE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pDBTMaster->GetNamespace().ToString());
		XMLSchema.InsertElement		(DOC_XML_MASTER_TAG,				DOC_XML_MASTER_TAG,	_T("1"), SCHEMA_XSD_UNBOUNDED_VALUE);
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_MASTER_TAG);
		XMLSchema.InsertAttribute	(DOC_XML_NUMBER_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_INTEGER_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute	(DOC_XML_BOOKMARK_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute	(DOC_XML_DELETED_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);	
		XMLSchema.InsertAttribute 	(DOC_XML_PROCESSED_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);	//aggiunta dall'algoritmo di import
		XMLSchema.InsertElement		(XML_UNIVERSAL_KEYS_TAG,			XML_UNIVERSAL_KEYS_TAG, _T("0"));
		XMLSchema.InsertElement		(DOC_XML_FIELDS_TAG,				DOC_XML_FIELDS_TAG, _T("0"));
		XMLSchema.InsertElement		(DOC_XML_EXTREFS_TAG,				DOC_XML_EXTREFS_TAG, _T("0"));
		XMLSchema.InsertElement		(DOC_XML_SLAVES_TAG,				DOC_XML_SLAVES_TAG, _T("0"));
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_SLAVES_TAG);
		if (pDBTSlaves)
		{
			for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
			{
				DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
			
				CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
									? pProfileInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace()) 
									: pDBTSlave->GetXMLDBTInfo();

				if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport()) 
					continue;

				if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				{
					XMLSchema.InsertElement (DOC_XML_SLAVEBUFF_TAG, DOC_XML_SLAVEBUFF_TAG);
					nDBTSlaveBuff++;
				}
				else
				{	
					XMLSchema.InsertElement (DOC_XML_SLAVE_TAG, DOC_XML_SLAVE_TAG);
					nDBTSlave++;
				}
			}
		}
	XMLSchema.EndComplexType();

	if (nDBTSlave> 0)
	{
		XMLSchema.BeginComplexType(DOC_XML_SLAVE_TAG);
			XMLSchema.InsertAttribute 		(DOC_XML_NAMESPACE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE);
			XMLSchema.InsertAttribute 		(DOC_XML_TABLE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE);	
			XMLSchema.InsertAttribute 		(DOC_XML_UPDATE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);	
			XMLSchema.InsertElement			(DOC_XML_FIELDS_TAG,			DOC_XML_FIELDS_TAG, _T("0"));
			XMLSchema.InsertElement			(DOC_XML_EXTREFS_TAG,			DOC_XML_EXTREFS_TAG, _T("0"));	
		XMLSchema.EndComplexType();
	}

	if (nDBTSlaveBuff> 0)
	{
		XMLSchema.BeginComplexType(DOC_XML_SLAVEBUFF_TAG);
			XMLSchema.InsertAttribute 		(DOC_XML_ROWSNUMBER_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_INTEGER_VALUE);
			XMLSchema.InsertAttribute 		(DOC_XML_NAMESPACE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE);	
			XMLSchema.InsertAttribute 		(DOC_XML_TABLE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE);	
			XMLSchema.InsertAttribute 		(DOC_XML_UPDATE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);	
			XMLSchema.InsertElement			(XML_ROW_TAG,					XML_ROW_TAG, _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
		XMLSchema.EndComplexType();

		
		XMLSchema.BeginComplexType(XML_ROW_TAG);
			XMLSchema.InsertAttribute 		(DOC_XML_NUMBER_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_INTEGER_VALUE);
			XMLSchema.InsertElement			(DOC_XML_FIELDS_TAG,			DOC_XML_FIELDS_TAG);
			XMLSchema.InsertElement			(DOC_XML_EXTREFS_TAG,			DOC_XML_EXTREFS_TAG, _T("0"));
		XMLSchema.EndComplexType();
	}


	//@@------------- Gestione degli EXTERNAL_REFERENCES -------------------------------
	XMLSchema.BeginComplexType(DOC_XML_EXTREFS_TAG);
		XMLSchema.InsertElement		(DOC_XML_EXTREF_TAG,	DOC_XML_EXTREF_TAG,	_T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_EXTREF_TAG);
		XMLSchema.InsertAttribute 		(DOC_XML_NAME_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute 		(DOC_XML_EXTREF_FILE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute 		(DOC_XML_BOOKMARK_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertElement			(XML_UNIVERSAL_KEYS_TAG,		XML_UNIVERSAL_KEYS_TAG, _T("0"));		
	XMLSchema.EndComplexType();


	//@@------------- Gestione dell'UNIVERSALKEYGROUP -------------------------------
	XMLSchema.BeginComplexType(XML_UNIVERSAL_KEYS_TAG);
			XMLSchema.InsertAttribute 		(XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE);
			XMLSchema.InsertAttribute 		(XML_UNIVERSAL_KEY_TABLENAME_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE);	
			XMLSchema.InsertAttribute 		(XML_NAMESPACE_ATTRIBUTE,					SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);	
			XMLSchema.InsertAttribute 		(DOC_XML_PROCESSED_ATTRIBUTE,				SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE, SCHEMA_XSD_OPTIONAL_VALUE); //aggiunta dall'algoritmo di import
			XMLSchema.InsertElement			(DOC_XML_KEY_TAG,							DOC_XML_KEY_TAG, _T("1"), SCHEMA_XSD_UNBOUNDED_VALUE);
			XMLSchema.InsertElement			(XML_UNIVERSAL_KEY_TAG,						XML_UNIVERSAL_KEY_TAG, _T("1"), SCHEMA_XSD_UNBOUNDED_VALUE);
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(XML_UNIVERSAL_KEY_TAG);
			XMLSchema.InsertAttribute 		(XML_UNIVERSAL_KEY_NAME_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_STRING_VALUE);
			if (pDBTMaster)
			{
				InsertMasterUKInXMLSchema(pDBTMaster, &XMLSchema, pProfileInfo);
				InsertExtRefUKInXMLSchema(pDBTMaster, &XMLSchema, pProfileInfo);
				DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
				if (pDBTSlaves)
					for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
					{
						DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
						ASSERT(pDBTSlave);
						InsertExtRefUKInXMLSchema(pDBTSlave, &XMLSchema, pProfileInfo);
					}
			}
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(DOC_XML_KEY_TAG);
		XMLSchema.InsertAttribute 		(DOC_XML_PK_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertAttribute 		(DOC_XML_FK_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute 		(DOC_XML_VALUE_ATTRIBUTE);		
	XMLSchema.EndComplexType();


	//@@------------- Gestione dei FIELDS  -------------------------------
	// elenco tutti i campi dei sqlrecord associati ai dbt esportabili
	XMLSchema.BeginComplexType(DOC_XML_FIELDS_TAG, SCHEMA_XSD_TYPE_ALL);
		InsertFieldsInXMLSchema(pDBTMaster, &XMLSchema, pProfileInfo);
		if (pDBTSlaves)
			for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
			{
				DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
				ASSERT(pDBTSlave);
				InsertFieldsInXMLSchema(pDBTSlave, &XMLSchema, pProfileInfo);
			}	
	XMLSchema.EndComplexType();

	ExtendDateType(XMLSchema, SCHEMA_DATATYPE_DATE_VALUE, SCHEMA_XSD_DATATYPE_DATE_VALUE);
	ExtendDateType(XMLSchema, SCHEMA_DATATYPE_TIME_VALUE, SCHEMA_XSD_DATATYPE_TIME_VALUE);
	ExtendDateType(XMLSchema, SCHEMA_DATATYPE_DATETIME_VALUE, SCHEMA_XSD_DATATYPE_DATETIME_VALUE);

	// Salvataggio del file XML
	return XMLSchema.SaveXMLFile((LPCTSTR)strSchemaFile, TRUE);
}

//----------------------------------------------------------------------------
void CXMLDataManager::ExtendDateType(CXSDGenerator &XMLSchema, LPCTSTR lpszExtendedType, LPCTSTR lpszBaseType)
{
	// definizione del tipo esteso date che permette l'esistenza del campo vuoto
	// date
	XMLSchema.BeginSimpleType (lpszExtendedType);
		XMLSchema.BeginGenericElement(SCHEMA_XSD_UNION_TAG);
			XMLSchema.InsertGenericElementAttribute(SCHEMA_XSD_MEMBER_TYPES_ATTRIBUTE, lpszBaseType);
				XMLSchema.BeginSimpleType ();
					XMLSchema.BeginGenericElement(SCHEMA_XSD_RESTRICTION_TAG);
						XMLSchema.InsertGenericElementAttribute(SCHEMA_XSD_BASE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE);
						XMLSchema.BeginGenericElement(SCHEMA_XSD_ENUMERATION_TAG);
							XMLSchema.InsertGenericElementAttribute(SCHEMA_XSD_VALUE_ATTRIBUTE, _T(""));							
						XMLSchema.EndGenericElement();
					XMLSchema.EndGenericElement();
				XMLSchema.EndSimpleType();
		XMLSchema.EndGenericElement();
	XMLSchema.EndSimpleType();

}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertDBTSlavesInXMLSchema
									(
										DBTMaster*				pDBTMaster, 
										CXSDGenerator*			pSchema, 
										const CXMLProfileInfo*	pProfileInfo
									)
{
	if (!pDBTMaster || !pSchema )
	{
		ASSERT(FALSE);
		return;
	}
	int nDBTSlave = 0;
	int nDBTSlaveBuff = 0;

	DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
	if (pDBTSlaves)
	{
		for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
		{
			DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
			
			CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace()) 
								: pDBTSlave->GetXMLDBTInfo();

			if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport()) 
				continue;

			if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				nDBTSlaveBuff++;
			else
				nDBTSlave++;

			if (nDBTSlave > 0 && nDBTSlaveBuff > 0)
				break;
		}

		if (nDBTSlave > 0)
			pSchema->InsertElement (DOC_XML_SLAVE_TAG, DOC_XML_SLAVE_TAG, _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
		if (nDBTSlaveBuff > 0)
			pSchema->InsertElement (DOC_XML_SLAVEBUFF_TAG, DOC_XML_SLAVEBUFF_TAG, _T("0"), SCHEMA_XSD_UNBOUNDED_VALUE);
	}			
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertFieldsInXMLSchema
									(
										DBTObject*				pDBTObj, 
										CXSDGenerator*			pSchema, 
										const CXMLProfileInfo*	pProfileInfo
									)
{
	if (!pDBTObj || !pSchema )
	{
		ASSERT(FALSE);
		return;
	}

	CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTObj->GetNamespace()) 
								: pDBTObj->GetXMLDBTInfo();

	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport())
		return;

	SqlRecord* pRecord = pDBTObj->GetRecord();
	if (pRecord)
	{
		pSchema->SetCheckForDuplicates();
		for (int nColIdx = 0; nColIdx < pRecord->GetSize(); nColIdx++)
		{
			const SqlColumnInfo* pColInfo = pRecord->GetColumnInfo(nColIdx);
			CString strColumnName = pColInfo->GetColumnName();
			if (pColInfo->m_bVirtual || !pXMLDBTInfo->IsFieldToExport(strColumnName))
				continue;

			DataObj* pDataObj = pRecord->GetDataObjAt(nColIdx);
			CString strType = GetDataObjType(pDataObj);
			if (!strType.IsEmpty())
			{
				if (AfxGetParameters()->f_UseAttribute && (!pProfileInfo || !pProfileInfo->IsTransformProfile()))
				{
					pSchema->BeginComplexElement(strColumnName, (pColInfo->m_bSpecial && pDBTObj->IsKindOf(RUNTIME_CLASS(DBTMaster))) ? _T("1") : _T("0"), _T("1"));
					pSchema->InsertAttribute (DOC_XML_VALUE_ATTRIBUTE, strType);
					pSchema->EndComplexElement();
				}
				else
					pSchema->InsertElement (strColumnName, strType, _T("0"), _T("1"));
			}
		}
		pSchema->SetCheckForDuplicates(FALSE);
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertMasterUKInXMLSchema
									(
										DBTMaster*				pDBTMaster, 
										CXSDGenerator*			pSchema,	
										const CXMLProfileInfo*	pProfileInfo
									)
{
	if (!pDBTMaster || !pSchema)
		return;

	CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTMaster->GetNamespace()) 
								: pDBTMaster->GetXMLDBTInfo();

	// il DBTMaster è sempre esportato
	if (!pXMLDBTInfo) return;

	CString strKeySegm;

	SqlRecord* pRecord = pDBTMaster->GetRecord();
	// inserisco l'eventuale gruppo di universalkey associato al dbtmaster
	CXMLUniversalKeyGroup* pUniversalKeyGroup = pXMLDBTInfo->GetXMLUniversalKeyGroup();	
	if (pRecord && pUniversalKeyGroup) 
	{
		pSchema->SetCheckForDuplicates();
		//creo un attributo per ogni segmento di universalkey
		for (int nUk = 0; nUk < pUniversalKeyGroup->GetSize(); nUk++)
		{
			CXMLUniversalKey* pUniversalKey = pUniversalKeyGroup->GetAt(nUk);			
			for (int nSegm = 0; pUniversalKey && nSegm<pUniversalKey->GetSegmentNumber(); nSegm++)
			{
				strKeySegm = pUniversalKey->GetSegmentAt(nSegm);
				if (!strKeySegm.IsEmpty())
				{
					DataObj* pDataObj = pRecord->GetDataObjFromColumnName(strKeySegm);
					if (pDataObj)
					{
						CString strType = GetDataObjType(pDataObj);
						pSchema->InsertAttribute(strKeySegm, strType, SCHEMA_XSD_OPTIONAL_VALUE);
					}
				}
			}
		}
		pSchema->SetCheckForDuplicates(FALSE);
	}			
}


//----------------------------------------------------------------------------
void CXMLDataManager::InsertExtRefUKInXMLSchema
									(
										DBTObject*				pDBTObj, 
										CXSDGenerator*			pSchema,	
										const CXMLProfileInfo*	pProfileInfo
									)

{
	if (!pDBTObj || !pSchema)
		return;

	CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTObj->GetNamespace()) 
								: pDBTObj->GetXMLDBTInfo();

	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport())
		return;

	CXMLXRefInfoArray*	pXRefInfoArray = pXMLDBTInfo->GetXMLXRefInfoArray();

	// non ho external-references
	if (!pXRefInfoArray || pXRefInfoArray->GetSize() <= 0)
		return;

	CString strKeySegm;
	DataObj* pDataObj = NULL;	
	SqlRecord* pRec = NULL;

	// devo inserire delle informazioni solo se l'ext-ref utilizza la gestione dell'UniversalKey
	pSchema->SetCheckForDuplicates();
	for (int i = 0; i < pXRefInfoArray->GetSize(); i++)
	{
		CXMLXRefInfo* pXRefInfo = pXRefInfoArray->GetAt(i);
		
		if (!pXRefInfo->IsToUse())
			continue;
			
		CXMLUniversalKeyGroup* pMasterUKGroup = NULL;
		CXMLUniversalKeyGroup* pUniversalKeyGroup = pXRefInfo ? pXRefInfo->GetXMLUniversalKeyGroup() : NULL;

		CAbstractFormDoc* pExtRefDoc = NULL;
		CXMLDocElement* pXMLDocElem = NULL;
		if (pUniversalKeyGroup)
		{
			// chiamo la funzione che si occupa di valorizzare i segmenti
			// viene istanziato se non è stato ancora istanziato 
			pXMLDocElem = m_pXMLExpImpMng->GetXMLDocElement(pXRefInfo->GetDocumentNamespace(), m_pDoc);

			pExtRefDoc = pXMLDocElem ? pXMLDocElem->m_pDocument : NULL;
			if (!pExtRefDoc) continue;

			pMasterUKGroup = pExtRefDoc->m_pDBTMaster->GetXMLDBTInfo()->GetXMLUniversalKeyGroup();
			if (!pMasterUKGroup) continue;

			pRec = pExtRefDoc->m_pDBTMaster->GetRecord();
			if (!pRec) continue;

			//creo un attributo per ogni segmento di universalkey
			for (int nUk = 0; nUk < pMasterUKGroup->GetSize(); nUk++)
			{
				CXMLUniversalKey* pUniversalKey = pMasterUKGroup->GetAt(nUk);
				if (pUniversalKey)
				{ 
					for (int nSegm = 0; nSegm < pUniversalKey->GetSegmentNumber(); nSegm++)
					{
						strKeySegm = pUniversalKey->GetSegmentAt(nSegm);
						if (!strKeySegm.IsEmpty())
						{
							pDataObj = pRec->GetDataObjFromColumnName(strKeySegm);
							if (pDataObj)
							{
								CString strType = GetDataObjType (pDataObj);
								pSchema->InsertAttribute(strKeySegm, strType, SCHEMA_XSD_OPTIONAL_VALUE);
							}
						}
					}
				}
			}
		}
	}
	pSchema->SetCheckForDuplicates(FALSE);
}

//----------------------------------------------------------------------------
CString CXMLDataManager::GetDataObjType (DataObj* pDataObj)
{
	ASSERT(pDataObj);
	CString strType;
	if (pDataObj)
	{
		strType = SCHEMA_XSD_DATATYPE_STRING_VALUE;
		switch (pDataObj->GetDataType().m_wType)
		{
			case DATA_STR_TYPE:
				strType = SCHEMA_XSD_DATATYPE_STRING_VALUE;						
				break;
			case DATA_INT_TYPE:
			case DATA_LNG_TYPE:		
			case DATA_ENUM_TYPE:
				strType = SCHEMA_XSD_DATATYPE_INTEGER_VALUE;						
				break;
			case DATA_DBL_TYPE:
			case DATA_MON_TYPE:
			case DATA_QTA_TYPE:
			case DATA_PERC_TYPE:
				strType = SCHEMA_XSD_DATATYPE_FLOAT_VALUE;						
				break;
			case DATA_DATE_TYPE:
				if (pDataObj->IsFullDate())
				{
					if (pDataObj->IsATime()) // XML Data Type: time
						strType = SCHEMA_DATATYPE_TIME_VALUE;		//è il tipo modificato che prevede il campo vuoto
					else // XML Data Type: dateTime
						strType = SCHEMA_DATATYPE_DATETIME_VALUE;	//è il tipo modificato che prevede il campo vuoto
				}
				else
				{
					strType = SCHEMA_DATATYPE_DATE_VALUE;			//è il tipo modificato che prevede il campo vuoto
				}
				break;
			case DATA_BOOL_TYPE:
				strType = SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE;						
				break;
			case DATA_GUID_TYPE:
				strType = SCHEMA_XSD_DATATYPE_STRING_VALUE;						
				break;
			default:
				break;
		}
	}
	
	return strType;
}

//----------------------------------------------------------------------------
void CXMLDataManager::RegisterObservableDataField()
{
	if (!m_pDoc || !m_pDoc->GetMaster() || !m_bTuningEnable)
		return;

	//I register all dataobjs of Document's DBTs
	//so I also consider the local field 
	SqlRecord* pRecord = m_pDoc->GetMaster()->GetRecord();
	SqlRecordItem* pRecItem = NULL;

	//I register all dataobjs of Document's DBTs
	//so I also consider the local field 
	for (int nIdx = 0; nIdx < pRecord->GetSize(); nIdx++)
	{
		pRecItem = pRecord->GetAt(nIdx);
		if (pRecItem && pRecItem->GetDataObj())
			pRecItem->GetDataObj()->AttachEvents(new CDataFieldEvents(pRecItem->GetColumnInfo(), m_pObserverContext));
		
	}	
	DBTSlave* pDBTSlave = NULL;
	for (int i = 0; i < m_pDoc->GetMaster()->GetDBTSlaves()->GetSize(); i++)
	{ 
		pDBTSlave = m_pDoc->GetMaster()->GetDBTSlaves()->GetAt(i);
		pRecord = pDBTSlave->GetRecord();

		//I register all dataobjs of Document's DBTs
		//so I also consider the local field 
		for (int nIdx = 0; nIdx < pRecord->GetSize(); nIdx++)
		{
			SqlRecordItem* pRecItem = pRecord->GetAt(nIdx);
			if (pRecItem && pRecItem->GetDataObj())
				pRecItem->GetDataObj()->AttachEvents(new CDataFieldEvents(pRecItem->GetColumnInfo(), m_pObserverContext));
			
		}
	}
}

//impr. 5320


//----------------------------------------------------------------------------
BOOL CXMLDataManager::SetDataFromXMLString(CString strXML, const CString& strXSLTFileName)
{
	DataArray arResult;
	CSmartImportParams smartImpParams(&arResult, IMPORT_ACTION_INSERT_UPDATE);

	if (m_pDoc->CanLoadXMLDescription())
		m_pDoc->LoadXMLDescription();

	if (!m_pDoc->GetXMLDocInfo())
	{
		TRACE("CXMLDataManager::SetDataFromXMLString: error during document %s file xml loading\n", m_pDoc->GetRuntimeClass()->m_lpszClassName);
		return FALSE;
	}

	CString strToImport = strXML;

	if (!strXSLTFileName.IsEmpty())
	{
		CXMLDocumentObject* pXMLDocObj;
		pXMLDocObj = new CXMLDocumentObject();
		if(pXMLDocObj->LoadXMLFile(strXML))
		{
			CXSLTManager XSLTManager(m_pDoc);
			XSLTManager.Initialize(strXSLTFileName);
			if (XSLTManager.TransformForExport(pXMLDocObj))
			{
				*pXMLDocObj = *XSLTManager.GetOutput();
				pXMLDocObj->GetXML(strToImport);
			}
			pXMLDocObj->Close();				
		}
		delete pXMLDocObj;
	}
	m_pDoc->EnableControlsForFind();
	BOOL bResult = ImportSmartDocument(strToImport, &smartImpParams, TRUE);
	if (!bResult)
		m_pDoc->m_pMessages->Add(cwsprintf(_TB("The following error occurred importing data: {0-%s}"), smartImpParams.GetFirstStringResult()));

	return bResult;
}


//----------------------------------------------------------------------------
CXMLDocumentObject* CXMLDataManager::CreateExportParametersFile (CString strProfileName, CPathFinder::PosType ePosType)
{
	CXMLDocumentObject* pXMLDoc = new CXMLDocumentObject();

	CXMLNode* pRoot = pXMLDoc->CreateRoot(m_pDoc->GetNamespace().GetObjectNameForTag());
	
	CString strPostType = szStandard;
	if (ePosType != CPathFinder::STANDARD)
	{
		if (ePosType == CPathFinder::USERS)
		{
			strPostType = _T("Users");
			strPostType += URL_SLASH_CHAR + AfxGetPathFinder()->ToUserDirectory(AfxGetLoginInfos()->m_strUserName);
		}
		else
			strPostType = szAllUserDirName;
	}
	
	CString strNamespaceURI =  _T("http://www.microarea.it/Schema/2004/Smart/") +
			m_pDoc->GetNamespace().GetApplicationName() + URL_SLASH_CHAR +
			m_pDoc->GetNamespace().GetModuleName() + URL_SLASH_CHAR +
			m_pDoc->GetNamespace().GetObjectName() +  URL_SLASH_CHAR  +
			strPostType + URL_SLASH_CHAR +					
			strProfileName + szXsdExt;
	
	pRoot->SetAttribute(_T("xTechProfile"), strProfileName);
	pRoot->SetAttribute(_T("postBack"), DataBool(TRUE).FormatDataForXML());
	pRoot->SetAttribute(_T("tbNamespace"), m_pDoc->GetNamespace().ToString());
	pRoot->SetAttribute(_T("postable"), DataBool(TRUE).FormatDataForXML());
	pXMLDoc->SetNameSpaceURI(strNamespaceURI, szNamespacePrefix);	
	
	return pXMLDoc;
}
//----------------------------------------------------------------------------
CString	CXMLDataManager::GetDataToXMLString(const CString& strProfileName, const CString& strXSLTFileName)
{
	if (m_pDoc->CanLoadXMLDescription())
		m_pDoc->LoadXMLDescription();

	if (!m_pDoc->GetXMLDocInfo())
	{
		TRACE("CXMLDataManager::GetDataToXMLString: error during document %s file xml loading\n", m_pDoc->GetRuntimeClass()->m_lpszClassName);
		return _T("");
	}

	CTBNamespace nsProfile;
	nsProfile.AutoCompleteNamespace(CTBNamespace::PROFILE, strProfileName, m_pDoc->GetNamespace());
	CString profileFileName = AfxGetPathFinder()->GetFileNameFromNamespace(nsProfile, AfxGetLoginInfos()->m_strUserName);
	CPathFinder::PosType posType = AfxGetPathFinder()->GetPosTypeFromPath(profileFileName);
	CXMLDocumentObject* pDocument = CreateExportParametersFile(strProfileName, posType);

	DataArray arResult(DATA_STR_TYPE);
	CSmartExportParams expPar(&arResult, TRUE);

	//check for existing profile for user
	expPar.m_ePosType = posType;
	if (expPar.m_ePosType == CPathFinder::USERS)
		expPar.m_strUserName = AfxGetLoginInfos()->m_strUserName;

	expPar.m_bExportCurrentDocument = TRUE;
	
	BOOL bSuccess = ExportSmartDocument(pDocument, &expPar);
	CString strResult;
	if (bSuccess && arResult.GetSize() >=1)
	{
		strResult = expPar.GetFirstStringResult();
		if (!strXSLTFileName.IsEmpty())
		{
			CXMLDocumentObject* pXMLDocObj;
			pXMLDocObj = new CXMLDocumentObject();
			if(pXMLDocObj->LoadXMLFile(strResult))
			{
				CXSLTManager XSLTManager(m_pDoc);
				XSLTManager.Initialize(strXSLTFileName);
				if (XSLTManager.TransformForExport(pXMLDocObj))
				{
					*pXMLDocObj = *XSLTManager.GetOutput();
					pXMLDocObj->GetXML(strResult);
				}
				pXMLDocObj->Close();				
			}
			delete pXMLDocObj;
		}
	}
	
	else	
		m_pDoc->m_pMessages->Add(cwsprintf(_TB("Error exporting document using the profile {0-%s}."), strProfileName));

	pDocument->Close();
	delete pDocument;
	AfxGetThreadContext()->m_bSendDocumentEventsToMenu = TRUE;	

	return strResult;	
}

//----------------------------------------------------------------------------
//	Class CXMLDataManagerClientMDoc definition
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDataManagerClientDoc, CClientDoc)

BEGIN_MESSAGE_MAP(CXMLDataManagerClientDoc, CClientDoc)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CXMLDataManagerClientDoc::CXMLDataManagerClientDoc()
{
}

//----------------------------------------------------------------------------
BOOL CXMLDataManagerClientDoc::OnAttachData () 
{
	CBaseDocument*	pServerDoc = GetMasterDocument();
	pServerDoc->SetXMLDataManager (new CXMLDataManager(pServerDoc));
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataManagerClientDoc::Customize()
{
	
}


