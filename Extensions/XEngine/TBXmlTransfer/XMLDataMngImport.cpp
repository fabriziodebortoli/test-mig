#include "stdafx.h" 
#include <Vfw.h>
#include <digitalv.h>

#include <TBXMLCore\xmldocobj.h>
#include <TBXMLCore\xmlgeneric.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <tbgeneric\dataobj.h>
#include <tbgeneric\globals.h>

#include <tbgeneric\formatstable.h>

#include <TBGENLIB\generic.h>
#include <TBGENLIB\baseapp.h>
#include <TBGenlibManaged\GlobalFunctions.h>

#include <TbNameSolver\PathFinder.h>

#include <tboledb\sqltable.h>

#include <TBGES\dbt.h>
#include <TBGES\browser.h>
#include <TBGES\extdoc.hjson> //JSON AUTOMATIC UPDATE
#include <TBGES\eventmng.h>
#include <TBGES\bodyedit.h>

#include <tbwebserviceswrappers\loginmanagerinterface.h>

#include <XEngine\TBXMLEnvelope\XMLEnvMng.h>
#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "ExpCriteriaDlg.h"
#include "XMLTransferTags.h"
#include "GenFunc.h"
#include "XMLProfileInfo.h"
#include "XMLDataMng.h"
#include "ImpCriteriaWiz.h"
#include "XMLEvents.h"
#include "XMLCodingRules.h"

// resource declarations
#include "XMLDataMng.hjson"

#include "..\ModuleObjects\CDImportWizard\JsonForms\IDD_IMPORT_TOOLBAR.hjson"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szDefaultImportLogFile	[] = _T("Import.xml");
static const TCHAR szOpenSquareBrace		[] = _T("[");
static const TCHAR szCloseSquareBrace		[] = _T("]");
static const TCHAR szOpenRoundBrace		[] = _T("(");
static const TCHAR szCloseRoundBrace		[] = _T(")");
static const TCHAR szAttributeSimbol		[] = _T("@");
static const TCHAR szEqualSimbol			[] = _T("=");
static const TCHAR szApex					[] = _T("'");
static const TCHAR szAnd					[] = _T(" && ");
static const TCHAR szOr					[] = _T(" || ");



//restituisce le informazioni di base di un nodo 
//------------------------------------------------------------------------------------------
BOOL GetSlaveNodeInfo(CXMLNode*pSlaveNode, CString& strSlaveType, CString& strSlaveTable, CString& strNameSpace)
{
	pSlaveNode->GetBaseName(strSlaveType);

	if (
			strSlaveType.CompareNoCase((LPCTSTR)DOC_XML_SLAVEBUFF_TAG) &&
			strSlaveType.CompareNoCase((LPCTSTR)DOC_XML_SLAVE_TAG) 
		)
		return FALSE;
	
	pSlaveNode->GetAttribute(DOC_XML_TABLE_ATTRIBUTE,strSlaveTable) &&
	pSlaveNode->GetAttribute(DOC_XML_NAMESPACE_ATTRIBUTE, strNameSpace);

	return TRUE;
}

//----------------------------------------------------------------------------
int CXMLDataManager::GetImportCmdMsg() const 
{
	return ID_EXTDOC_IMPORT_XML_DATA;
}

//----------------------------------------------------------------------------
void CXMLDataManager::SetAppImportCriteriaFileName(const CString& strFileName) 
{ 
	if (!strFileName.IsEmpty())
		m_strAppImportCriteriaFileName = MakeImpCriteriaVarFile
											(
												m_pDoc->GetNamespace(), 
												strFileName, 
												CPathFinder::USERS, 
												AfxGetLoginInfos()->m_strUserName
											);
}

//----------------------------------------------------------------------------
void CXMLDataManager::SetUnattendedImportParams (BOOL bImportDownload, BOOL bImportValidate, LPCTSTR lpszImpCriteriaFilename)
{	
	// Questo metodo va chiamato solo se il documento è già stato aperto!!!
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return;
	}
	m_bImportDownload = bImportDownload;
	m_bImportValidate = bImportValidate;	

	SetAppImportCriteriaFileName(lpszImpCriteriaFilename);
}


//----------------------------------------------------------------------------
CWizardFormDoc* CXMLDataManager::CreateImportWizard(CAbstractFormDoc* pDocToPutInWaitState /* = NULL */)
{
	CBaseDocument *pBaseDoc = AfxGetTbCmdManager()->RunDocument
														(
															_NS_DOC("Document.Extensions.XEngine.TbXmlTransfer.ImportCriteria"),
															szDefaultViewMode, 
															FALSE,
															NULL,
															this
														); 
	if (!pBaseDoc) return NULL;

	CImpCriteriaWizardDoc	*pDoc = (CImpCriteriaWizardDoc*)pBaseDoc;
	pDoc->m_pWaitingDoc = pDocToPutInWaitState;
	
	return pDoc;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::RunImportWizard 
						(
						   CXMLEnvElemArray* pRXSelectedElems, 
						   LPCTSTR			 lpszImpCriteriaFilename,/* = NULL*/
						   CAbstractFormDoc* pDocToPutInWaitState /* = NULL */
						 )
{
	if (!m_pDoc->IsEditingParamsFromExternalController() && (!pRXSelectedElems || !m_pDoc))
		return FALSE;

	if (!pDocToPutInWaitState)
		pDocToPutInWaitState = m_pDoc;

	CAutoExpressionMng* pAutoExpressionMng = new CAutoExpressionMng();

	m_pWizardDocument = CreateImportWizard(pDocToPutInWaitState);

	if (!m_pWizardDocument)

		return FALSE;


	SetAppImportCriteriaFileName(lpszImpCriteriaFilename);
	((CImpCriteriaWizardDoc*)m_pWizardDocument)->m_pRXSelectedElems = pRXSelectedElems;
	((CImpCriteriaWizardDoc*)m_pWizardDocument)->LoadAppImportCriteria(GetAppImportCriteriaFileName(), pAutoExpressionMng);

	WaitWizardDocument();

	delete pAutoExpressionMng;
	return m_bContinueExpImp;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::Import()
{		
	// Questo metodo va chiamato solo se il documento è già stato aperto!!!
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	m_pDoc->EnableCounterCaching(TRUE);
	m_bContinueExpImp = FALSE;
	BOOL bOk = FALSE;

	CreateXMLExpImpManager();
	m_bIsRootDoc = TRUE;

	CXMLEnvElemArray RXSelectedElems;
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	ASSERT(pEnvMng);
	if (pEnvMng) pEnvMng->MoveEnvelopesToRXPath();

	if (!IsInUnattendedMode())
	{
		if (!RunImportWizard(&RXSelectedElems))
		{	
			m_nProcessStatus = XML_IMPORT_ABORTED;
			m_nStatus = XML_MNG_IDLE;
			
			SAFE_DELETE(m_pXMLExpImpMng);
			return FALSE;
		}
		m_pWizardDocument = NULL;
	}
	else
	{
		CString strEnvClass = m_pDoc->GetXMLDocInfo()->GetEnvClass();
		if (strEnvClass.IsEmpty())
			strEnvClass = m_pDoc->GetNamespace().ToString();
		
		SetValidateOnParse(m_bImportValidate);
		if (!m_strAppImportCriteriaFileName.IsEmpty())
		{
			CAppImportCriteria appImportCriteria(m_pDoc);
			CAutoExpressionMng autoExpressionMng;
			appImportCriteria.LoadAppImportCriteria(m_strAppImportCriteriaFileName, &autoExpressionMng);
		}

		if (pEnvMng)
		{
			CString	strLastError;
			if (!m_bImportDownload )
			{
				pEnvMng->LoadBothEnvClassArray((LPCTSTR)strEnvClass, m_pDoc->GetNamespace());
				pEnvMng->GetEnvelopeToImport(&RXSelectedElems);
				m_bContinueExpImp = TRUE;
			}
		}
	}

	if (m_bContinueExpImp)
	{
		if (!m_pObserverContext)
			m_pObserverContext = new CObserverContext();

		bOk = Import(&RXSelectedElems);
	}
	else if (m_pDoc->IsRunningFromExternalController())
	{
		m_pDoc->SetRunningTaskStatus(CExternalControllerInfo::TASK_FAILED);
		m_pDoc->m_pExternalControllerInfo->m_Finished.Set();
	}												

	return bOk;
}

// questa funzione viene chiamata da interfaccia SOAP
//----------------------------------------------------------------------------
BOOL CXMLDataManager::Import(const CString& strEnvFolder, CString &strRetVal)
{
	// Questo metodo va chiamato solo se il documento è già stato aperto!!!
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if(AfxGetSiteName().IsEmpty())
	{
		strRetVal = _TB("Web site name not set");
		return FALSE;
	}

	if (!ExistPath(strEnvFolder))
	{
		strRetVal = cwsprintf(_TB("Folder {0-%s} not exist."), strEnvFolder);
		return FALSE;
	}
	
	SetUnattendedMode();
	CreateXMLExpImpManager();

	CXMLEnvElemArray RXSelectedElems;
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if(!pEnvMng)
	{
		SAFE_DELETE(m_pXMLExpImpMng);
		ASSERT(FALSE);
		return FALSE;
	}
	
	pEnvMng->FillSelection(&RXSelectedElems, strEnvFolder, m_pDoc);
	return Import(&RXSelectedElems, &strRetVal);
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::Import(CXMLEnvElemArray* pRXSelectedElems, CString *pstrRetVal /*=NULL*/)
{
	DWORD dStartTick = GetTickCount();
	DWORD dElapsedTick = 0;

	CString strMessage;
	int nDocsFailed = 0, nDocsImported=0;	
	Array arChanges;	

	m_nProcessStatus =  XML_IMPORT_SUCCEEDED;

	if (pRXSelectedElems && pRXSelectedElems->GetSize() > 0) 
	{
		m_nProcessStatus = XML_IMPORTING_DATA;
		m_nStatus = XML_MNG_IMPORTING_DATA;	

		BOOL bOldUnattendedMode = m_pDoc->SetInUnattendedMode(TRUE);
		m_pDoc->EnableCounterCaching(TRUE);
		BOOL bOldBatch = m_pDoc->m_bBatch;
		m_pDoc->m_bBatch = TRUE;			
		if (!AfxGetXMLImpExpController()->ShowDialog())
		{
			OutputMessage(_TB("Another import/export process is running. Unable to continue."));
			return FALSE;
		}
		AfxGetXMLImpExpController()->SetMessage(_TB("Import procedure starting..."));		
		
		SqlRecord* pOlDBTMasterRec = AfxCreateRecord(m_pDoc->m_pDBTMaster->GetRecord()->GetTableName());
		if (pOlDBTMasterRec)
			*pOlDBTMasterRec = *m_pDoc->m_pDBTMaster->GetRecord();
		
		//Optimization
		if (!UseOldXTechMode())
		{
			PrepareRequiredTabDlgs(); 
			RegisterObservableDataField();
		}
	
		for (int i = 0; i <= pRXSelectedElems->GetUpperBound(); i++)
		{
			CXMLEnvElem* pElem = pRXSelectedElems->GetAt(i);
			if (pElem && pElem->m_pSiteAncestor && 
				pElem->m_pSiteAncestor->m_pAncestor && !pElem->m_strEnvFileName.IsEmpty())
			{
				CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
				if (pEnvMng)
				{
					pEnvMng->SetRXEnvFolderPath(GetPath(pElem->m_strEnvFileName));
					pEnvMng->SetEnvName(pElem->m_strEnvName);
					pEnvMng->SetEnvClass(pElem->m_pSiteAncestor->m_pAncestor->m_strEnvClass);
					pEnvMng->SetSenderSite(pElem->m_pSiteAncestor->m_strSiteName);
					if (!ImportEnvelope() && m_nDocsFailed==0)
						m_nDocsFailed++;

					nDocsFailed += m_nDocsFailed;
					nDocsImported += m_nDocsImported;
					
					m_pXMLExpImpMng->FlushLogSpace();
				}
			}
		}

		DWORD dStopTick = GetTickCount();
		dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
	
		AfxGetXMLImpExpController()->CloseDialog();
		
		ASSERT(nDocsImported!=0 || nDocsFailed!=0);

		if (nDocsImported == 0)
			m_nProcessStatus = XML_IMPORT_FAILED;
		else if (nDocsFailed == 0 && !m_pXMLExpImpMng->m_bExistErrors)
			m_nProcessStatus = XML_IMPORT_SUCCEEDED;
		else
			m_nProcessStatus = XML_IMPORT_SUCCEEDED_WITH_ERRORS;

		switch(m_nProcessStatus)
		{
			case XML_IMPORT_SUCCEEDED:
				strMessage = (m_bTuningEnable) ? _TB("Data import and tuned successfully completed.") : _TB("Data import successfully completed.");
				break;
			case XML_IMPORT_SUCCEEDED_WITH_ERRORS:
				strMessage = (m_bTuningEnable) ? _TB("Data import and tuned completed with errors.") :  _TB("Data import completed with errors.");
				break;
			case XML_IMPORT_FAILED:
			case XML_IMPORT_ABORTED:
				strMessage = (m_bTuningEnable) ? _TB("Failed to import and tuning data.") :_TB("Failed to import data.");
				break;
			default:
				break;
		}

				
		if (pstrRetVal)
		{
			*pstrRetVal = strMessage;
			
			CStringArray strArray;
			m_pXMLExpImpMng->m_LogSession.GetLogSpaceFileList(&strArray);
			for (int i=0; i<strArray.GetSize (); i++)
				*pstrRetVal += strArray.GetAt (i);
		}
		m_pDoc->SetInUnattendedMode(bOldUnattendedMode);
		m_pDoc->m_bBatch = bOldBatch;	
		m_nStatus = XML_MNG_IDLE;
		
		if ( m_nProcessStatus == XML_IMPORT_FAILED ||  m_nProcessStatus == XML_IMPORT_ABORTED)			
			*(m_pDoc->m_pDBTMaster->GetRecord()) = *pOlDBTMasterRec;
		if (m_bTuningEnable && (m_nProcessStatus == XML_IMPORT_SUCCEEDED || m_nProcessStatus == XML_IMPORT_SUCCEEDED_WITH_ERRORS))
			m_pXMLExpImpMng->SaveEvents(&arChanges);
		
		m_pDoc->OnRadarRecordSelected(FALSE);
		delete(pOlDBTMasterRec);
	}
	
	
	if (!IsInUnattendedMode() && !strMessage.IsEmpty())
	{		
		if (AfxGetBaseApp()->IsDevelopment())
		{
			CTickTimeFormatter aTickFormatter;
			strMessage += cwsprintf(_TB("\nElapsed time: {0-%s}\n"), aTickFormatter.FormatTime(dElapsedTick));
		}
		strMessage += _TB("Do you want to view the messages?");

		UINT nLogSpaces = m_pXMLExpImpMng->m_LogSession.GetLogSpacesNumber(TRUE);
		if (nLogSpaces >1)
			strMessage += cwsprintf(_TB("(Number of logging files: {0-%d})"), nLogSpaces);
			
		if (AfxMessageBox (strMessage, MB_YESNO) == IDYES)
		{
			if (m_bTuningEnable)
			{
				CXMLImportTuningResult aDlg 
						(
							AfxGetMainWnd(), 
							m_pXMLExpImpMng->m_LogSession.GetCurrentLogFile(),
							&arChanges, 
							m_nProcessStatus == XML_IMPORT_SUCCEEDED
						);
				aDlg.DoModal ();
			}
			else
				m_pXMLExpImpMng->ShowLogSpaces();
		}
	}		
	
	if ( m_pDoc->IsRunningFromExternalController())
	{
		m_pDoc->SetRunningTaskStatus ((m_nProcessStatus == XML_IMPORT_SUCCEEDED)
														? CExternalControllerInfo::TASK_SUCCESS
														: CExternalControllerInfo::TASK_FAILED);
		m_pDoc->m_pExternalControllerInfo->m_Finished.Set();
	}


	SAFE_DELETE(m_pXMLExpImpMng)
	
	return m_nProcessStatus == XML_IMPORT_SUCCEEDED;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportEnvelope()
{
	m_nDocsImported = m_nDocsFailed = 0;

	if (!m_pXMLExpImpMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}


	//pulisce gli eventuali record per cui e' fallita l'importazione in precedenza
	m_pXMLExpImpMng->InitRecordsProcessed();
	//pulisce eventuali DOMDocument in memoria
	m_pXMLExpImpMng->InitXMLDocumentArray();
	//pulisce l'array degli external reference già processati
	m_pXMLExpImpMng->m_arProcessedNodes.RemoveAll();

	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
			
	//inizializza lo spazio di logging
	m_pXMLExpImpMng->InitLogSpace(pEnvMng->GetEnvName(), pEnvMng->GetRXEnvFolderLoggingPath());
	
	// Questo metodo va chiamato solo se il documento è già stato aperto!!!
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	

	CWnd *pMasterFrameWnd = m_pDoc->GetMasterFrame();
	if (pMasterFrameWnd) 
	{
		pMasterFrameWnd->EnableWindow(FALSE);
		pMasterFrameWnd->BeginWaitCursor();
	}

	CString rxPath = pEnvMng->GetRXEnvFolderPath();
	
	//crea la directory se non esiste
	pEnvMng->GetPendingSenderSitePath(TRUE);
	
	CString pendingPath = pEnvMng->GetPendingEnvFolderPath(TRUE, FALSE);
	
	// sposto tutto nella pending (se non c'e' gia')
	// m_bImportPendingData mi serve per sapere se devo applicare la trasformazione XSLT o e'
	// gia' stata applicata
	if (rxPath.CompareNoCase (pendingPath) )
	{
		if (::ExistPath(pendingPath))
			RemoveFolderTree(pendingPath);

		if (!CopyFolderTree(rxPath, pendingPath, TRUE))
		{
			OutputMessage(cwsprintf(_TB("Unable to copy the folder: {0-%s} to the new path: {1-%s}.\nIt is possibile that complete source files names or destination files names exceed 255 characters. Reduce generated names modifying export profile names and names of the data files."), rxPath, pendingPath));
			if (pMasterFrameWnd) 
			{
				pMasterFrameWnd->EnableWindow(TRUE);
				pMasterFrameWnd->EndWaitCursor();
			}
			return FALSE;
		}
	
		if (!RemoveFolderTree(rxPath))
		{
			OutputMessage(cwsprintf(_TB("Unable to delete the folder: {0-%s} or part of its content."), rxPath));
			if (pMasterFrameWnd) 
			{
				pMasterFrameWnd->EnableWindow(TRUE);
				pMasterFrameWnd->EndWaitCursor();
			}
			return FALSE;
		}

		m_bImportPendingData = FALSE;
	}
	else
		m_bImportPendingData = TRUE;

	m_pXMLExpImpMng->m_LogSession.SetLogPath(pEnvMng->GetPendingEnvFolderLoggingPath());		
	
	CXMLDocumentObject *pXMLDocObj = m_pXMLExpImpMng->GetXMLImportDomDocument(
																	pEnvMng->GetEnvFileName(),  
																	pEnvMng->GetPendingEnvFolderPath(),
																	FALSE);
	if (!pXMLDocObj)
	{
		ASSERT(FALSE);
		if (pMasterFrameWnd) 
		{
			pMasterFrameWnd->EnableWindow(TRUE);
			pMasterFrameWnd->EndWaitCursor();
		}
		return FALSE;
	}

	CString strRootTagName;
	pXMLDocObj->GetRootName(strRootTagName);

	BOOL bOk = TRUE;

	if (!strRootTagName.CompareNoCase((LPCTSTR)ENV_XML_ENVELOPE_ID_TAG))
	{
		if (!pEnvMng->ReadEnvelope(pXMLDocObj))
		{
			OutputMessage(_TB("Error reading the envelope file."));
			if (pMasterFrameWnd) pMasterFrameWnd->EnableWindow(TRUE);
			return FALSE;
		}
		
		m_pXMLExpImpMng->m_strSiteCode = pEnvMng->m_aXMLEnvInfo.m_aEnvDocInfo.m_strSiteCode;

		CXMLEnvContentsArray*	pEnvContArray = pEnvMng->GetEnvContensArray();
		if (!pEnvContArray)
		{
			OutputMessage(_TB("The envelope file contains no items."));
			if (pMasterFrameWnd) pMasterFrameWnd->EnableWindow(TRUE);
			return FALSE;
		}

		
		BOOL bFoundRoot = FALSE;
		for (int i =0; i < pEnvContArray->GetSize(); i++)
		{
			if (pEnvContArray->GetAt(i) && (pEnvContArray->GetAt(i)->IsRootFile()))
			{
				bFoundRoot = TRUE;
				bOk = ImportRecordsFromXMLFile(pEnvContArray->GetUrlDataAt(i), _T(""), TRUE);				
				break;
			}
		}
		RestoreActiveTabs(); //optimization @@BAUZI

		if (!bFoundRoot)
		{
			OutputMessage(_TB("Envelope file not include any file of type 'Root'"));
			bOk = FALSE;
		}
	}

	//se tutto e' andato bene, i DOMDocs in memoria non devono piu'
	//contenere tag DOC_XML_MASTER_TAG (perche' ogni volta che ne importo uno, lo rimuovo)
	Array *pArray = m_pXMLExpImpMng->m_pXMLDocumentArray;
	if (pArray)
	{
		for (int i=0; i<pArray->GetSize (); i++)
		{
			CQualifiedXMLDocument* pQualifiedDoc = (CQualifiedXMLDocument*) pArray->GetAt(i);
			if (pQualifiedDoc && 
				pQualifiedDoc->m_pXMLDomDoc &&
				bOk && 
				m_nDocsFailed == 0)
			{
				if (!IsDocumentEmpty(pQualifiedDoc->m_pXMLDomDoc))
				{
					bOk=FALSE;
					OutputMessage(_TB("At least one document in the envelope files cannot be located by the import procedure. The envelope you are attempting to export is not consistent."));
					break;
				}
			}
		}
	}
	
	if (bOk && m_nDocsFailed == 0)
	{
		bOk = MoveSuccessEnvelope();
		m_pXMLExpImpMng->m_LogSession.SetLogPath(pEnvMng->GetSuccessEnvFolderLoggingPath());					
	}
	else 
	{
		MoveFailureEnvelope();
		m_pXMLExpImpMng->m_LogSession.SetLogPath(pEnvMng->GetPartialSuccessEnvFolderLoggingPath());
	}

	if (pMasterFrameWnd) 
	{
		pMasterFrameWnd->EnableWindow(TRUE);
		pMasterFrameWnd->EndWaitCursor();
	}
	
    return bOk && m_nDocsFailed == 0;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::MoveSuccessEnvelope()
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
			
	//crea la directory di arrivo (success) e vi sposta l'eventuale contenuto della partial success
	pEnvMng->GetSuccessSenderSitePath();
	CString strDestinationPath = pEnvMng->GetSuccessEnvFolderPath(TRUE, FALSE);

	CString strOriginPath = pEnvMng->GetPartialSuccessEnvFolderPath(TRUE, FALSE);
	CString strPendingPath = pEnvMng->GetPendingEnvFolderPath(TRUE, FALSE);

	// se c'e' la partial success, ci sono stati fallimenti in precedenza: l'envelope di origine e' li
	// e i files della pending non mi servono
	// se non c'e', non ci sono stati fallimenti in precedenza, quindi l'envelope di origine e' nella pending
	if (!ExistPath (strOriginPath))
		strOriginPath = strPendingPath;
	else
	{
		if (!RemoveFolderTree(strPendingPath))
		{
			ASSERT(FALSE);
			OutputMessage(cwsprintf(_TB("Unable to delete the folder: {0-%s} or part of its content."), strPendingPath));
		}
	}

	if (!CopyFolderTree(strOriginPath, strDestinationPath, TRUE))
	{
		ASSERT(FALSE);
		OutputMessage(cwsprintf(_TB("Unable to copy the folder: {0-%s} to the new path: {1-%s}.\nIt is possibile that complete source files names or destination files names exceed 255 characters. Reduce generated names modifying export profile names and names of the data files."), strOriginPath, strDestinationPath));
		return FALSE;
	}
	
	if (!RemoveFolderTree(strOriginPath))
	{
		ASSERT(FALSE);
		OutputMessage(cwsprintf(_TB("Unable to delete the folder: {0-%s} or part of its content."), strOriginPath));
		return FALSE;
	}

	return TRUE;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::MoveFailureEnvelope()
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
			
	CXMLEnvContentsArray* pEnvContArray = pEnvMng->GetEnvContensArray();
	ASSERT(pEnvContArray);

	//carico in memoria eventuali DOM non raggiunti dalla procedura di importazione
	for (int i =0; i < pEnvContArray->GetSize(); i++)
	{
		if (pEnvContArray->GetAt(i) && (pEnvContArray->GetAt(i)->IsDataFile()))
		{
			m_pXMLExpImpMng->GetXMLImportDomDocument(pEnvContArray->GetUrlDataAt(i), pEnvMng->GetPendingEnvFolderDataPath(), !m_bImportPendingData, TRUE);
		}
	}
	
	//creo la directory di arrivo (partial success)
	pEnvMng->GetPartialSuccessSenderSitePath();
	CString strDestinationPath = pEnvMng->GetPartialSuccessEnvFolderPath(TRUE, FALSE);
	CString strPendingPath = pEnvMng->GetPendingEnvFolderPath(TRUE, FALSE);
	
	//la prima volta, sposto tutto nella partial success
	//da li faro' poi il backup
	if (!::ExistPath (strDestinationPath))
	{
		if (!::CopyFolderTree(strPendingPath, strDestinationPath))
		{
			ASSERT(FALSE);
			OutputMessage(cwsprintf(_TB("Unable to copy the folder: {0-%s} to the new path: {1-%s}.\nIt is possibile that complete source files names or destination files names exceed 255 characters. Reduce generated names modifying export profile names and names of the data files."), strPendingPath, strDestinationPath));
			return FALSE;
		}
	}
	// tolgo i file di dato dalla pending
	if (!RemoveFolderTree(pEnvMng->GetPendingEnvFolderDataPath(TRUE, FALSE), FALSE))
	{
		OutputMessage(cwsprintf(_TB("Unable to delete the folder: {0-%s} or part of its content."), strPendingPath));
		return FALSE;
	}
	
	return BackupRecords ();

}

//----------------------------------------------------------------------------
CXMLNode* CXMLDataManager::GetEnvelopeBackupNode(const CString& strFileName, CXMLDocumentObject* pDoc)
{
	if (!pDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	CString strPrefix = GET_NAMESPACE_PREFIX(pDoc);
	CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + strPrefix + ENV_XML_FILE_TAG;
	CXMLNodeChildsList *pNodeList = pDoc->SelectNodes (strFilter, strPrefix);
	for(int i=0; pNodeList && i<pNodeList->GetSize (); i++)
	{
		pDoc->RemoveNode (pNodeList->GetAt(i));
	}
	SAFE_DELETE(pNodeList);
	
	CXMLNode* pDocsNode = pDoc->GetRootChildByName(ENV_XML_CONTENTS_TAG);
	if (!pDocsNode)
	{
		OutputMessage(
						cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_DOCUMENTS_TAG) +
						cwsprintf(_TB("File: {0-%s}. "), strFileName)
					);
		return NULL;
	}

	return pDocsNode;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportRecordsFromXMLFile
							(
								const CString& strFileToImport,
								const CString& strBookMark	/*= ""*/,
								BOOL bIsFirstFile /*=FALSE*/
							)
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strMessage = cwsprintf(_TB("Start importing record from file: {0-%s}."), strFileToImport); 
	if (!strBookMark.IsEmpty())
		strMessage += cwsprintf(_TB(" Bookmark: {0-%s}."), strBookMark);
	OutputMessage	(strMessage, CXMLLogSpace::XML_INFO);

	CXMLDocumentObject *pXMLDocObj = m_pXMLExpImpMng->GetXMLImportDomDocument(strFileToImport, pEnvMng->GetPendingEnvFolderDataPath(), !m_bImportPendingData);
	if (!pXMLDocObj)
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileToImport));
		return FALSE;
	}

	CString strRootTagName;
	pXMLDocObj->GetRootName(strRootTagName);

	if (strRootTagName.CompareNoCase((LPCTSTR)DOC_XML_DOCUMENT_TAG))
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileToImport));
		return FALSE;
	}	
	
	CXMLNode* pDocInfoNode = pXMLDocObj->GetRootChildByName(DOC_XML_DOCINFO_TAG);
	if (!pDocInfoNode)
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileToImport));
		return FALSE;
	}

	CXMLNode* pNSNode = pDocInfoNode->GetChildByName(DOC_XML_NAMESPACE_TAG);
	if (!pNSNode)
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileToImport));
		return FALSE;
	}

	CString strNameSpace;
	pNSNode->GetText(strNameSpace);
	if (strNameSpace.CompareNoCase(m_pDoc->GetNamespace().ToString()))
	{
		OutputMessage(cwsprintf(_TB("Invalid document NameSpace: {0-%s}."), strNameSpace));
		return FALSE;
	}
	
	
	CXMLNode* pDocsNode = pXMLDocObj->GetRootChildByName(DOC_XML_DOCUMENTS_TAG);
	if (!pDocsNode)
	{
		OutputMessage(
						cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_DOCUMENTS_TAG) +
						cwsprintf(_TB("File: {0-%s}. "), strFileToImport)
					);
		return FALSE;
	}

	CXMLNode* pMastersNode = pDocsNode->GetChildByName(DOC_XML_MASTERS_TAG);
	if (!pMastersNode)
	{
		OutputMessage(
						cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_MASTERS_TAG) +
						cwsprintf(_TB("File: {0-%s}. "), strFileToImport)
					);
		return FALSE;
	}

	//Optimization
	//inizializzazione eventmanager
	if (!UseOldXTechMode())
	{
		if (m_pXMLExpImpMng)
			m_pXMLExpImpMng->InitTablesEvents();
	}

	if (!ImportDocumentList(pMastersNode, strFileToImport, strBookMark) && m_bIsExtRef)
		return FALSE;	

	CXMLNode* pNext = pDocInfoNode->GetChildByName (DOC_XML_NEXTFILE_TAG);
	if (pNext)
	{
		CString strNextFile;
		pNext->GetText (strNextFile);
		if (strNextFile.IsEmpty())
		{
			OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileToImport));
			ASSERT(FALSE);
			return FALSE;
		}
		return ImportRecordsFromXMLFile(strNextFile, strBookMark);
	}

	return TRUE;	
}

//----------------------------------------------------------------------------
// sostituisce le chiavi dell'external reference con quelle derivanti
// dalla traslazione con universal key
// 
BOOL CXMLDataManager::GetNewKey(CXMLNode* pFieldsNode, CXMLNode* pUniversalKey, CAbstractFormDoc* pDoc, SqlRecord* pRecord)
{
	if (!pFieldsNode || !pUniversalKey || !pDoc || !pRecord)
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	// per evitare di processare due volte i documenti della pending
	CString strProcessed;
	if (pUniversalKey->GetAttribute (DOC_XML_PROCESSED_ATTRIBUTE, strProcessed))
		return TRUE;

	CString strFuncName;
	if (!pUniversalKey->GetAttribute (XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE, strFuncName))
	{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
		OutputMessage(_TB("The name of the converting function is not specified."));
		return FALSE;
	}

	int nResult = pDoc->FireAction (strFuncName, (void*)pUniversalKey);
	
	if (nResult == CEventManager::FUNCTION_NOT_FOUND ||
		nResult == CEventManager::FUNCTION_ERROR)
	{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
		if (pUniversalKey && AfxIsValidAddress(pUniversalKey, sizeof(CXMLNode)))
		{
			CXMLNode *pError =  pUniversalKey->GetChildByName (DOC_XML_ERROR_TAG);
			if (pError)
			{
				CString strError;
				pError->GetText(strError);
				OutputMessage(strError);
			}	
		}
		return FALSE;
	}
	
	if (!pUniversalKey || !AfxIsValidAddress(pUniversalKey, sizeof(CXMLNode)))
	{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
		return FALSE;
	}	
	

	CString strXml;
	pUniversalKey->GetXML (strXml);
	
	if (pUniversalKey->GetChildByName (DOC_XML_ERROR_TAG))
	{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
		AppendMessageDetail(strXml);
		return FALSE;
	}	

	CString strPrefix = GET_NAMESPACE_PREFIX(pUniversalKey);
	CXMLNodeChildsList *pKeyList = pUniversalKey->SelectNodes (strPrefix+DOC_XML_KEY_TAG, strPrefix);
	if (!pKeyList)
	{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
		AppendMessageDetail(strXml);
		return FALSE;
	}

	CString strNodeName, strNodeValue;
	CXMLNode *pNodeToChange = NULL;
	CXMLNode *pSourceNoce = NULL;
	CString strOldKeys, strNewKeys;
	for (int i=0; i<pKeyList->GetSize (); i++)
	{			
		pSourceNoce = pKeyList->GetAt (i);

		// prima cerco l'attributo Fk; se non c'è, 
		// cerco l'attributo Pk
		// la regola è: se sono in un external reference, ho sia Fk che Pk, 
		// e devo modificare il campo indicato da Fk;
		// se invece sono in un master (sto modificando al chiave primaria)
		// allora devo modificare il campo indicato da Pk;
		if (!pSourceNoce->GetAttribute (DOC_XML_FK_ATTRIBUTE, strNodeName) &&
			!pSourceNoce->GetAttribute (DOC_XML_PK_ATTRIBUTE, strNodeName) )
		{
		OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
			AppendMessageDetail(strXml);
			SAFE_DELETE(pKeyList);
			return FALSE;
		}
		if (!(pNodeToChange = pFieldsNode->GetChildByName (strNodeName, FALSE)))
		{
			OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
			AppendMessageDetail(strXml);
			SAFE_DELETE(pKeyList);
			return FALSE;
		}
		
		if (!pSourceNoce->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strNodeValue))
		{
			OutputMessage(cwsprintf(_TB("Error managing the UniversalKey for the document: {0-%s}."), pDoc->GetTitle()));
			AppendMessageDetail(strXml);
			SAFE_DELETE(pKeyList);
			return FALSE;
		}

		// se il valore è espresso come attributo, setto quello
		// altrimenti setto il testo del nodo
		CString strOldKey;
		if (pNodeToChange->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strOldKey))
		{
			pNodeToChange->SetAttribute  (DOC_XML_VALUE_ATTRIBUTE, strNodeValue); 
		}
		else
		{
			pNodeToChange->GetText (strOldKey);
			pNodeToChange->SetText (strNodeValue);
		}

		// modifico anche il valore del sqlrecord, in modo che se i campi da xml sono
		// già stati importati i sqlrecord viene aggiornato
		DataObj* pDataObj = pRecord->GetDataObjFromColumnName (strNodeName);	
		ASSERT(pDataObj);
		if (pDataObj)
		{
			pDataObj->SetValueLocked (FALSE);
			pDataObj->AssignFromXMLString((LPCTSTR)strNodeValue);			
			pDataObj->SetValueLocked ();
		}
		
		strOldKeys += (strNodeName + _T(": ") + strOldKey + _T(" "));
		strNewKeys += (strNodeName + _T(": ") + strNodeValue + _T(" "));
	}
	
	pUniversalKey->SetAttribute (DOC_XML_PROCESSED_ATTRIBUTE, FormatBoolForXML(TRUE));
	OutputMessage(
		cwsprintf(_TB("Key convertion through UniversalKey for the document: {0-%s} completed; original key: {1-%s}; modified key: {2-%s}."), pDoc->GetTitle(), strOldKeys, strNewKeys),
		CXMLLogSpace::XML_INFO);

	SAFE_DELETE(pKeyList);		
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportExtRef(CXMLNode* pDBTNode, SqlRecord *pRecord)
{	
	if (!pDBTNode)
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	//il nodo di external reference può essere vuoto
	CXMLNode *pRefsNode = pDBTNode->GetChildByName (DOC_XML_EXTREFS_TAG);
	if (!pRefsNode) return TRUE;	

	CXMLNode *pFieldsNode = pDBTNode->GetChildByName (DOC_XML_FIELDS_TAG);
	if (!pFieldsNode)
	{
		OutputMessage(cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_FIELDS_TAG));
		ASSERT(FALSE);
		return FALSE;
	}	

	
	CXMLNodeChildsList *pNodeList = pRefsNode->GetChilds();
	for (int i=0; pNodeList && i<pNodeList->GetSize(); i++)
	{
		CXMLNode* pChildNode = pNodeList->GetAt (i);
		if (!pChildNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		CString strFileName, strBookmark;	

		BOOL bExistExtRefFile = 
				pChildNode->GetAttribute ((LPCTSTR)DOC_XML_EXTREF_FILE_ATTRIBUTE, strFileName) &&
				pChildNode->GetAttribute ((LPCTSTR)DOC_XML_BOOKMARK_ATTRIBUTE, strBookmark) &&
				!strFileName.IsEmpty() &&
				!strBookmark.IsEmpty ();
		
		CXMLNode *pUniversalKey = pChildNode->GetChildByName (XML_UNIVERSAL_KEYS_TAG);
		CXMLDocElement* pDocElement = NULL;
		
		// possono darsi tre casi: 
		// 1) c'è il file di ext ref da importare ma non il nodo di UK: chiedo al documento di extref se è
		//	obbligatoria la presenta di UK
		// 2) c'è il nodo di UK ma non il file di ext ref: no problem, gestisco l'UK
		// 3) non c'è nessuno dei due elementi: l'ext ref è privo di significato
		if (!bExistExtRefFile && !pUniversalKey) continue;

		//se l'extref è già stato importato allora segnalo questo nel log ma non faccio niente
		BOOL bImportResult = FALSE;
		if (m_pXMLExpImpMng->IsProcessedNode(strFileName, strBookmark, bImportResult))
		{
			CString strMessage = cwsprintf(_TB("Skip external reference in file {0-%s} with bookmark {1-%s}. Already {2-%s}."), 
				strFileName,
				strBookmark,
				(bImportResult) ? _TB("successfully processed") : _TB("processed with error"));
		
			OutputMessage(strMessage, (bImportResult) ? CXMLLogSpace::XML_INFO : CXMLLogSpace::XML_ERROR);					
			AfxGetXMLImpExpController()->SetMessage(strMessage);

			if (!bImportResult)
				return FALSE;
		}
		else
		{
			// se esiste il file di external reference, lo importo; il documento
			// ad esso associato è quello corrente del m_pXMLExpImpMng
			// e lo recupero per gestire la universal key (eventuale) del mio external reference
			if (bExistExtRefFile)
			{
				if (!ImportExtRef(strFileName, strBookmark))
					return FALSE;
				
				pDocElement = m_pXMLExpImpMng->GetCurrentDocElem();
				
				ASSERT(pDocElement);
				ASSERT(pDocElement->m_pDocument);	
			}
		}

		if (pUniversalKey && !pDocElement)
		{
			CString strDocNS;
			pUniversalKey->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strDocNS); 
			CTBNamespace aDocumentNameSpace(strDocNS);
			if (!aDocumentNameSpace.IsValid())
			{
				OutputMessage(cwsprintf(_TB("Invalid document NameSpace: {0-%s}."), strDocNS));
				ASSERT(FALSE);
				return FALSE;
			}
			
			pDocElement = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc);
			if (!pDocElement || !pDocElement->m_pDocument)
			{
				OutputMessage(cwsprintf(_TB("Invalid document NameSpace: {0-%s}."), strDocNS));
				ASSERT(FALSE);
				return FALSE;
			}
			
		}

		if (pUniversalKey && pDocElement  && !CheckUniversalKey(pChildNode, pDocElement->m_pDocument))
			return FALSE;

		//la rileggo perché il metodo potrebbe avermela rimossa
		pUniversalKey = pChildNode->GetChildByName (XML_UNIVERSAL_KEYS_TAG);
		//se c'e' una universal key
		if (pUniversalKey)
		{					
			pUniversalKey->SetAttribute (DOC_XML_EXTREF_ATTRIBUTE, FormatBoolForXML(TRUE));

			// se esiste una universal key, opero la sostituzione nei campi del nodo fields 
			if (!GetNewKey(pFieldsNode, pUniversalKey, pDocElement->m_pDocument, pRecord))
				return FALSE;

			// se il metodo OnPreparePKForXMLImport ha assegnato un counter, lo aveva bloccato per tenermelo per la procedura di import
			// ma se il documento non deve essere importato ma è instanziato solo per 
			// processare la universal key, devo sbloccarlo qui
			// (normalmente, lo sblocco viene invece effettuato dopo che il documento è stato importato)
			DataObj* pDataObj = pDocElement->m_pDocument->GetAssignedCounter();
			if (pDataObj)
				pDataObj->SetValueLocked (FALSE);
		}
	}
	
	return TRUE;
}



//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportExtRef(const CString& strFileName, const CString& strBookmark)
{	
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strPath = pEnvMng->GetPendingEnvFolderDataPath();
	CString strFileRootName = GetExtension(strFileName).IsEmpty() 
														? strFileName + szXmlExt 
														: strFileName;
	if (IsRelativePath(strFileRootName))
		strFileRootName = strPath + strFileRootName;

	// se non è stato caricato perché non esiste, tutto OK (solo warning di avvertimento,
	// potrebbe già essere stato caricato in precedenza
	if (!::ExistFile(strFileRootName))
		return TRUE;

	CXMLDocumentObject* pXMLDocObj = m_pXMLExpImpMng->GetXMLImportDomDocument(strFileName, strPath, !m_bImportPendingData);
	if (!pXMLDocObj)
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileName));
		return FALSE;
	}

	CXMLNode* pDocInfoNode = pXMLDocObj->GetRootChildByName(DOC_XML_DOCINFO_TAG);
	if (!pDocInfoNode)
	{
		OutputMessage(cwsprintf(_TB("Import file {0-%s} not found or invalid."), strFileName));
		return FALSE;
	}

	CString strDocNS;
	CXMLNode* pNamespaceNode = pDocInfoNode->GetChildByName(DOC_XML_NAMESPACE_TAG);
	if (pNamespaceNode) pNamespaceNode->GetText(strDocNS);

	CTBNamespace aDocumentNameSpace(strDocNS);

	if (!aDocumentNameSpace.IsValid())
	{
		cwsprintf(_TB("Invalid document NameSpace: {0-%s}."), strDocNS);
		return FALSE;
	}
	
	CXMLDocElement* pDocElement = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc);
	if (!pDocElement)
	{
		OutputMessage(cwsprintf(_TB("Error importing external reference for the field '{0-%s}'.\r\nFailed to open document with Namespace '{1-%s}'."), strFileName, strDocNS));
		return FALSE;
	}

	m_pXMLExpImpMng->SetCurrentDocElem(pDocElement);
	
	CAbstractFormDoc* pExtRefDoc = pDocElement->m_pDocument;

	if (!pExtRefDoc)
	{
		OutputMessage(cwsprintf(_TB("Error importing external reference for the field '{0-%s}'.\r\nFailed to open document with Namespace '{1-%s}'."), strFileName, strDocNS));
		return FALSE;
	}
	
	CXMLDataManagerObj* pExtRefXMLDataMng = pExtRefDoc->GetXMLDataManager();
	if (!pExtRefXMLDataMng || !pExtRefXMLDataMng->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	BOOL bRetVal = ((CXMLDataManager*)pExtRefXMLDataMng)->ImportRecordsFromXMLFile(strFileName, strBookmark);
	
	//lo reimposto perché potrebbe essermi stato cambiato dalla ImportRecordsFromXMLFile
	m_pXMLExpImpMng->SetCurrentDocElem(pDocElement);
	return bRetVal;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportDocumentList
							(
								CXMLNode*		pMastersNode,
								const CString&	strFileName, 
								const CString&	strBookmark /*=""*/
							)
{
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}
	
	ASSERT_VALID(pMastersNode);
	
	DBTMaster* pDBTMaster = m_pDoc->m_pDBTMaster;
	ASSERT_VALID(pDBTMaster);

	SqlTable* pMasterTable = pDBTMaster->GetTable();
	ASSERT_VALID(pMasterTable);
	
	CString strMasterTable;
	if
		(
			!pMastersNode->GetAttribute(DOC_XML_TABLE_ATTRIBUTE,strMasterTable)||
			(!strMasterTable.IsEmpty() && strMasterTable.CompareNoCase(pMasterTable->GetTableName()) != 0)
		)
	{
		OutputMessage(cwsprintf(_TB("The table '{0-%s}' is not managed by the current document."), strMasterTable));
		return FALSE;
	}

	// Se nel file di dati viene riportato il namespace del DBT allora controllo che 
	// coincida altrimenti mi baso solo sul nome della tabella
	CString strNameSpace;
	pMastersNode->GetAttribute(XML_NAMESPACE_ATTRIBUTE, strNameSpace);
	if (strNameSpace.CompareNoCase(pDBTMaster->GetNamespace().ToString()))
	{
		OutputMessage(cwsprintf(_TB("Invalid DBT NameSpace for the table '{0-%s}'."), strMasterTable));
		return FALSE;
	}
	
	CString strDataInstances;
	int nDataInstancesNum = 
			(
				pMastersNode->GetAttribute(DOC_XML_DATAINSTANCES_ATTRIBUTE,strDataInstances) 
				&& !strDataInstances.IsEmpty()
			) 
				? _ttoi((LPCTSTR)strDataInstances)
				: 1;

	BOOL	bErrorOccurred = FALSE;
	
	int		nProcessedMasterRecs = 0;
	
	CString strPrefix = GET_NAMESPACE_PREFIX(pMastersNode);
	CString strFilter = strPrefix + DOC_XML_MASTER_TAG;
	if (!strBookmark.IsEmpty())
	{
		strFilter = strFilter +
						szOpenSquareBrace + 
						szAttributeSimbol + 
						DOC_XML_BOOKMARK_ATTRIBUTE + 
						szEqualSimbol + 
						strBookmark + 
						szCloseSquareBrace; 
	}
	
	CXMLNodeChildsList *pList = pMastersNode->SelectNodes(strFilter, strPrefix); 

	if (pList && !pList->GetSize())
	{
		CString strDoc = strFileName;
		if (!strBookmark.IsEmpty())
			strDoc += cwsprintf(_TB(" Bookmark: {0-%s}."), strBookmark);

		OutputMessage(cwsprintf(_TB("No document found in the file: {0-%s}."), strDoc), CXMLLogSpace::XML_WARNING); 
	}

	//AfxGetXMLImpExpController()->SetMessage(cwsprintf( _TB("Importing file : {0-%s}."), strFileName));

	CXMLNode* pNode = NULL;
	for (int i = 0; pList && i < pList->GetSize(); i++)
	{
		BOOL bResult = FALSE;
		pNode = pList->GetAt (i);
		
		if(!m_bIsExtRef && AfxGetXMLImpExpController()->IsAborted())
		{
			// se l'utente ha abortito, devo comunque proseguire il ciclo per 
			// tenere traccia dei documenti ancora da processare
			AfxGetXMLImpExpController()->CloseDialog();
			OutputMessage(_TB("Procedure interrupted by the user"));
			EnableLogging(FALSE);
			m_nProcessStatus = XML_EXPORT_ABORTED;
		}
		else
		{
			// per evitare di processare due volte i nodi nella stessa sessione di import (ciclicità)
			// effettuo il 'lock' sul nodo
			CString strProcessed;
			if (pNode->GetAttribute(DOC_XML_PROCESSED_ATTRIBUTE, strProcessed) && 
				strProcessed.CompareNoCase(FormatBoolForXML(TRUE)) == 0)
				continue;	
			pNode->SetAttribute(DOC_XML_PROCESSED_ATTRIBUTE, FormatBoolForXML(TRUE));		

			CString strMessage = cwsprintf(_TB("Starting import of the document:\r\n{0-%s}\r\nfrom the file:\r\n{1-%s};"), m_pDoc->GetTitle(), strFileName);
			if (!strBookmark.IsEmpty()) 
			{
				strMessage += (_T("\r\n") + cwsprintf(_TB(" Bookmark: {0-%s}."), strBookmark));
			}
		
			OutputMessage(strMessage, CXMLLogSpace::XML_INFO);
			AfxGetXMLImpExpController()->SetMessage(strMessage);
		
			RaiseLoggingLevel ();		
			bResult = ImportDocument(pNode, pDBTMaster);		

			// se il metodo OnPreparePKForXMLImport ha assegnato un counter, lo aveva bloccato per tenermelo per la procedura di import
			DataObj* pDataObj = m_pDoc->GetAssignedCounter();
			if (pDataObj)
				pDataObj->SetValueLocked (FALSE);

			LowerLoggingLevel ();

			strMessage = cwsprintf	(
										bResult 
											? _TB("Import of document:\r\n{0-%s}\r\nsuccessfully completed.\r\nDocument keys:\r\n{1-%s}.")
											: _TB("Import for document:\r\n{0-%s}\r\nfailed.\r\nDocument keys:\r\n{1-%s}."),			
										m_pDoc->GetTitle(), 
										pDBTMaster->GetRecord()->GetPrimaryKeyDescription ()
									);
			OutputMessage(strMessage, (bResult ? CXMLLogSpace::XML_INFO : CXMLLogSpace::XML_ERROR));
			AfxGetXMLImpExpController()->SetMessage(strMessage);
			if (m_pDoc->m_pTbContext->TransactionPending()) //BugFix #20495
				m_pDoc->m_pTbContext->Rollback();
		}

		// aggiungo il nodo alla lista di quelli processati
		// (li rimuovo solo se tutta la transazione va a buon fine, ossia 
		// quando ho salvato il root document)
		m_pXMLExpImpMng->AddProcessedNode(strFileName, strBookmark, new CXMLNode(pNode->GetIXMLDOMNodePtr()), bResult);
			
		if (bResult)
		{
			nProcessedMasterRecs++;
			m_nDocsImported ++;
			pNode->SetAttribute(DOC_XML_PROCESSED_ATTRIBUTE, FormatBoolForXML(TRUE));

			// se sono un root document, provvedo a cancellare tutti i nodi che ho importato
			if (!m_bIsExtRef)
				m_pXMLExpImpMng->ManageProcessedNodes(TRUE);			
			
		}
		else
		{
			bErrorOccurred = TRUE;
			m_nDocsFailed ++;

			// se sono un root document, provvedo reintegrare tutti i nodi che ho 
			// segnato come processati (ma che non ho in effetti importato)
			if (!m_bIsExtRef)
			{
				m_pDoc->m_pTbContext->Rollback();				 
				m_pXMLExpImpMng->ManageProcessedNodes(FALSE);
				OutputMessage
						(
							_TB("Changes related to external references have been removed."), 
							CXMLLogSpace::XML_WARNING
						);				
			}

			//questo mi serve per il backup dei root document non importati correttamente
			if (!m_bIsExtRef && strBookmark.IsEmpty())
			{
				m_pXMLExpImpMng->SetCurrentDocElem(m_pDoc);
				//tengo traccia del record processato per l'importazione ma non andato a buon fine
				m_pXMLExpImpMng->InsertFailedRecord(pDBTMaster->GetRecord (), strFileName);
			}

			if (m_bIsExtRef)
			{
				SAFE_DELETE(pList);
				return FALSE;				
			}
		}
	}
	
	SAFE_DELETE(pList);

	return !(bErrorOccurred || (strBookmark.IsEmpty() && nProcessedMasterRecs != nDataInstancesNum));
}

///----------------------------------------------------------------------------
BOOL CXMLDataManager::BackupRecords ()
{
	RemoveSchemaFiles();

	m_pXMLExpImpMng->SetCurrentDocElem(m_pDoc);
	
	Array* pArray = m_pXMLExpImpMng->GetCurrentDocElem()->m_pProcessedRecordArray;
	if (!pArray || pArray->GetSize()==0)
	{
		BackupEnvelope (_T(""));
		return SavePendingDOMS();
	}

	CString strFileName;
	for (int i=0; i<pArray->GetSize(); i++)
	{
		CXMLRecordInfo* pFailedRecord = (CXMLRecordInfo*)pArray->GetAt(i);
		if (!pFailedRecord)
		{
			ASSERT(FALSE);
			OutputMessage(_TB("Internal procedure error."));
			return FALSE;
		}
		
		//il primo file lo metto nel backup dell'envelope
		if (strFileName.IsEmpty ())
		{
			strFileName = pFailedRecord->m_strFileName;
			BackupEnvelope (strFileName);
		}
		else
		{
			//se e' cambiato il file, devo collegare il vecchio al nuovo col tag NextFile
			if (strFileName.Compare (pFailedRecord->m_strFileName))
			{
				LinkFiles(strFileName, pFailedRecord->m_strFileName);
				strFileName = pFailedRecord->m_strFileName;
			}
		}
	}
	// rimuovo l'eventuale tag nextfile dall'ultimo file
	if (!strFileName.IsEmpty ())
		LinkFiles(strFileName, _T(""));

	return SavePendingDOMS(); 

}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::SavePendingDOMS ()
{
	Array *pArray = m_pXMLExpImpMng->m_pXMLDocumentArray;
	BOOL bOk = TRUE;
	if (pArray)
	{
		for (int i=0; i<pArray->GetSize (); i++)
		{
			CQualifiedXMLDocument* pQualifiedDoc = (CQualifiedXMLDocument*) pArray->GetAt(i);
			if (pQualifiedDoc && pQualifiedDoc->m_pXMLDomDoc)
			{
				//se c'è almeno un nodo master devo salvare nella pending
				if (!IsDocumentEmpty(pQualifiedDoc->m_pXMLDomDoc))
					bOk = pQualifiedDoc->m_pXMLDomDoc->SaveXMLFile (pQualifiedDoc->m_strFileName) && bOk;
			}
		}
	}

	bOk = TestCanRetryImport() && bOk;

	return 	bOk;
}

//----------------------------------------------------------------------------
///carica il file backuppato e inserisce il link al successivo
BOOL CXMLDataManager::LinkFiles (const CString& strOldFileName, const CString& strFileName)
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CXMLDocumentObject *pOldDom = m_pXMLExpImpMng ? m_pXMLExpImpMng->GetXMLImportDomDocument(strOldFileName, pEnvMng->GetPendingEnvFolderDataPath(), !m_bImportPendingData, TRUE) : NULL;
	if (!pOldDom)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CXMLNode* pDocInfoNode = pOldDom->GetRootChildByName(DOC_XML_DOCINFO_TAG);
	if (!pDocInfoNode) return FALSE;

	CXMLNode* pNext = pDocInfoNode->GetChildByName (DOC_XML_NEXTFILE_TAG);
	if (strFileName.IsEmpty())
	{
		if (pNext) 	return pDocInfoNode->RemoveChild (pNext);
		return TRUE;
	}

	if (!pNext) pNext = pDocInfoNode->CreateNewChild (DOC_XML_NEXTFILE_TAG);
	
	if (!pNext->SetText (strFileName)) return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::BackupEnvelope (CString strFileName)
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
			
	CXMLEnvContentsArray* pEnvContArray = pEnvMng->GetEnvContensArray();
	ASSERT(pEnvContArray);
	if (!strFileName.IsEmpty())
	{
		for (int i =0; i < pEnvContArray->GetSize(); i++)
		{
			CXMLEnvFile *pEnvFile = pEnvContArray->GetAt(i);
			if (!pEnvFile)
				continue;
			
			if ((pEnvFile->IsDataFile()))
			{
				CXMLDocumentObject* pDoc = m_pXMLExpImpMng ? m_pXMLExpImpMng->GetXMLImportDomDocument(pEnvContArray->GetUrlDataAt(i), pEnvMng->GetPendingEnvFolderDataPath(), !m_bImportPendingData, TRUE) : NULL;
				if (pDoc)
				{
					if (!IsDocumentEmpty(pDoc))
					{
						if (GetName(pEnvFile->m_strUrlData) == GetName(strFileName))
							pEnvFile->m_eFileType = CXMLEnvFile::ROOT_FILE;
						else if (pEnvFile->m_eFileType == CXMLEnvFile::ROOT_FILE)
							pEnvFile->m_eFileType = CXMLEnvFile::NEXT_ROOT_FILE;
					}
					else
					{
						pEnvContArray->RemoveAt (i);
						i--;
					}
				}
			}
			else
			{
				pEnvContArray->RemoveAt (i);
				i--;
			}
		}
	}
	
	return pEnvMng->CreateEnvelope(!IsInUnattendedMode(), pEnvMng->GetPendingEnvFolderPath());

}

//----------------------------------------------------------------------------
// Questa funzione controlla se il record del master esiste ed in tal caso
// lo aggiorna, altrimenti lo inserisce
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportDocument
							(
								CXMLNode*	pRecordNode,
								DBTMaster*	pDBTMaster,
								BOOL		bWithExtref /*=TRUE*/
							)
{
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}

	ASSERT(!m_bBusy);	// il documento è impegnato ad importare un altro documento!
	m_bBusy = TRUE;

	// rilascio la coda dei messaggi per rendere responsiva l'applicazione
	//m_pDoc->m_BatchScheduler.CheckMessage();

	ASSERT_VALID(pRecordNode);
	ASSERT_VALID(pDBTMaster);

	//pulisco il record 
	// uso l'init del DBT anzich?quello del SqlRecord poich?ci sono delle tabelle condivisi tra pi?DBT. 
	// Vedi Customer e Supplier
	pDBTMaster->Init();


	//prima della valorizzazione dei campi
	m_pDoc->OnBeforeXMLImport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnBeforeXMLImport();

	pDBTMaster->OnBeforeXMLImport ();

	BOOL bIsToDelete = FALSE;
	CString strIsToDelete;
	if (pRecordNode->GetAttribute (DOC_XML_DELETED_ATTRIBUTE, strIsToDelete))
		bIsToDelete = !strIsToDelete.CompareNoCase(FormatBoolForXML(TRUE));
	
	SqlRecord* pNewMasterRec = pDBTMaster->GetRecord();
	ASSERT(pNewMasterRec);

	if (!bIsToDelete)
		m_pDoc->m_pTbContext->StartTransaction();
			
	if (!CheckUniversalKey(pRecordNode, m_pDoc))
	{
		m_bBusy = FALSE;
		return FALSE;
	}

	CXMLNode *pUniversalKey = pRecordNode->GetChildByName (XML_UNIVERSAL_KEYS_TAG);
	CXMLNode *pFieldsNode = pRecordNode->GetChildByName (DOC_XML_FIELDS_TAG);
	// se esiste una universal key, opero la sostituzione nei campi del nodo fields	
	if (pUniversalKey && pFieldsNode &&
		!GetNewKey (pFieldsNode, pUniversalKey, m_pDoc, pNewMasterRec))
	{
		m_bBusy = FALSE;
		return FALSE;
	}

	if (!ImportRecordFields(pRecordNode, pNewMasterRec))
	{
		m_bBusy = FALSE;
		return FALSE;
	}
			 
	if (bIsToDelete)
	{
		BOOL b = DeleteDocument(pDBTMaster, pNewMasterRec);
		m_bBusy = FALSE;
		return b;
	}


	/* 
	se in futuro si vorranno gestire decodifiche di campi
	diversi dalla chiave primaria	
	
	CCodeManager aCodeManager (m_pDoc, GetCurrentSiteCode()); 
	aCodeManager.GetNewFields(TRUE);*/

	if (!pDBTMaster->OnOkXMLImport())
	{
		UnlockFields (pDBTMaster);
		m_bBusy = FALSE;
		return FALSE;
	}

	//dopo la valorizzazione dei campi
	pDBTMaster->OnAfterXMLImport ();
	
	// Salvo i dati caricati in un record temporaneo
	SqlRecord* pTmpRec = pNewMasterRec->Create();
	ASSERT(pTmpRec);
	*pTmpRec = *pNewMasterRec;

	// catturo i messaggi del documento
	BeginListeningToDoc(m_pDoc);
		
	//se il documento esiste già ma ho impostato i parametri in modo da 
	//evitare l'aggiornamento, esco
	if (pDBTMaster->FindData(FALSE) && !m_pDoc->OnOkXMLUpdateImport())
	{
		OutputMessage	(	cwsprintf(_TB("The document already exists: unable to update. Check import parameteres. Document: {0-%s}; keys: {1-%s}."),
									(LPCTSTR)m_pDoc->GetTitle(),
									(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()),
								CXMLLogSpace::XML_WARNING
						);
		delete pTmpRec;
		UnlockFields (pDBTMaster);
		m_bBusy = FALSE;
		return TRUE;
	}

	//importo i documenti referenziati
	if (bWithExtref && !ImportExtRef(pRecordNode, pTmpRec))
	{
		delete pTmpRec;
		UnlockFields (pDBTMaster);
		EndListeningToDoc(m_pDoc);
		m_bBusy = FALSE;
		return FALSE;
	}
	// metto da parte la lista degli indici di dataobj locckati e li unloccko
	// (dopo si riloccko)
	CUIntArray arLockedObjsIndexes;
	for (int i = 0; i <= pNewMasterRec->GetUpperBound(); i++)
	{
		DataObj *pCurrDataObj = pNewMasterRec->GetDataObjAt(i);
		if (pCurrDataObj->IsValueLocked())
		{
			pCurrDataObj->SetValueLocked (FALSE);
			arLockedObjsIndexes.Add(i);
		}
	}

	// Caricamento del record nel documento
	// Se il record non viene trovato lo si deve inserire
	// devo fare di nuovo la finddata perché il documento 
	// potrebbe essere stato inserito da un external reference.
	SqlTable* pTableToRelock = NULL;
	if (pDBTMaster->FindData(FALSE))
	{
		// Aggiornamento record esistente...
		m_pDoc->OnRadarRecordSelected(TRUE);
		if (!m_pDoc->ValidCurrentRecord() || m_pDoc->GetFormMode()!=CAbstractFormDoc::EDIT)
		{
			// il record è locckato; lo sto locckando io col 
			// processo di importazione?
			if ((pTableToRelock=GetUnlockedTable()) == NULL)
			{
				delete pTmpRec;
				UnlockFields (pDBTMaster);
				EndListeningToDoc(m_pDoc);
				m_pDoc->EscapeImportDocument();
				m_bBusy = FALSE;
				return FALSE;
			}
		}
		
		OutputMessage	(	cwsprintf(_TB("The document already exists: it will be updated. Document: {0-%s}; keys: {1-%s}."),
										(LPCTSTR)m_pDoc->GetTitle(),
										(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()),
								CXMLLogSpace::XML_WARNING
							);
	}
	else // Inserimento nuovo record...prima controllo che sia possibile
	{
		if (!m_pDoc->OnRadarRecordNew()) 
		{
			OutputMessage(cwsprintf(_TB("Failed to enter a new record for the document '{0-%s}' in the table '{1-%s}'."),
										(LPCTSTR)m_pDoc->GetTitle(),
										(LPCTSTR)pNewMasterRec->GetTableName()));
			delete pTmpRec;
			EndListeningToDoc(m_pDoc);
			m_pDoc->EscapeImportDocument();
			UnlockFields (pDBTMaster);
			m_bBusy = FALSE;
			return FALSE;
		}
		
		OutputMessage (cwsprintf(_TB("Entering new document. Document: {0-%s}; keys: {1-%s}."),
										(LPCTSTR)m_pDoc->GetTitle(),
										(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()),
								CXMLLogSpace::XML_INFO
							);
		
	}

	// devo fare così perché ho locckato i valori con l'assegnazione da XML
	// riassegno solo i campi che ho importato da xml (ossia quelli locckati)
	for (int i = 0; i <= arLockedObjsIndexes.GetUpperBound(); i++)
	{
		UINT index = arLockedObjsIndexes.GetAt(i);
		*pNewMasterRec->GetAt(index) = *pTmpRec->GetAt(index);
		pNewMasterRec->GetDataObjAt(index)->SetValueLocked (TRUE);
	}

	delete pTmpRec;

	for(int nSlave = 0; nSlave <= m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetUpperBound(); nSlave++)
		m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetAt(nSlave)->Reload();


	// Carico i dati relativi allo slave: va fatto necessariamente dopo aver 
	// caricato il record nel documento (sia in aggiornamento che in inserimento)
	CXMLNode* pSlavesNode = pRecordNode->GetChildByName((LPCTSTR)DOC_XML_SLAVES_TAG);

	// I have to raise master events only after fields unlock
	// otherwise they are unuseful
	FireRecEvents (pNewMasterRec, pSlavesNode);

	// caso in cui devo inserire o aggiornare il record
	
	if (pSlavesNode)
	{
		if (!ImportDBTSlaves(pSlavesNode, bWithExtref))
		{
			EndListeningToDoc(m_pDoc);
			UnlockFields (pDBTMaster);
			m_pDoc->EscapeImportDocument();
			if (pTableToRelock) pTableToRelock->LockCurrent();
			m_bBusy = FALSE;
			return FALSE;
		}
	}

	pNewMasterRec->SetStorable();
	m_pDoc->OnPrepareAuxData();	
	
	// se sono instato di new ma il documento è già presente,
	// ripeto la procedura, senza external references
	if (m_pDoc->GetFormMode() == CAbstractFormDoc::NEW && pDBTMaster->Exist())
	{
		EndListeningToDoc(m_pDoc);
		UnlockFields (pDBTMaster);
		m_pDoc->EscapeImportDocument();
		if (pTableToRelock) pTableToRelock->LockCurrent();
		m_bBusy = FALSE;
		return ImportDocument (pRecordNode, pDBTMaster, FALSE);
	}

	if (!m_pDoc->OnOkXMLImport())
	{
		EndListeningToDoc(m_pDoc);
		UnlockFields (pDBTMaster);
		if (pTableToRelock) pTableToRelock->LockCurrent();
		m_pDoc->EscapeImportDocument();
		m_bBusy = FALSE;
		return FALSE;
	}
	
	if (UseOldXTechMode())
	{
		CUIntArray tabManagers, tabs;
		
		if (m_pDoc->GetXMLDocInfo()->GetHeaderInfo()->IsFullPrepare())
		{
			//congelo lo stato di attivazione delle tab
			FreezeActiveTabs(&tabManagers, &tabs);

			// attivo tutte la tab per richiamare le OnPrepareAuxData ed altre eventuali inizializzazioni
			ActivateAllTabs();

			//ripristino lo stato di attivazione delle tab per attivare la tab originariamente attiva
			//in modo da scatenarne la OnPrepareAuxData (prima del firing degli eventi) 
			RestoreActiveTabs(&tabManagers, &tabs);
		}
	}
	else
		ActivateRequiredTabDlgs();	
	
	m_pDoc->OnAfterXMLImport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnAfterXMLImport();

	BOOL bOk = TRUE;

	bOk = m_pDoc->SaveImportDocument();

	if (!bOk)
	{
		OutputMessage(cwsprintf
			(
				_TB("Failed to import document: {0-%s} when saving.\r\n Document keys: {1-%s}"), 
				(LPCTSTR)m_pDoc->GetTitle(),
				(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription())
			);
	}
	else
	{
		// se esiste una universal key nella LostAndFound, devo rimuoverla
		CXMLNode *pUniversalKey = pRecordNode->GetChildByName (XML_UNIVERSAL_KEYS_TAG);
		if (pUniversalKey)
		{
			CXMLDataImportDoc* pClient = (CXMLDataImportDoc*) m_pDoc->GetClientDoc(RUNTIME_CLASS(CXMLDataImportDoc));
			if (pClient && pClient->m_pEventManager)
			{
				ASSERT_KINDOF(CImportEvents, pClient->m_pEventManager);
				((CImportEvents*)pClient->m_pEventManager)->RemoveFromLostAndFound (pUniversalKey);
			}			
		}

		OutputMessage(cwsprintf
			(
				 _TB("Document: {0-%s} successfully saved.\r\n Document keys: {1-%s}"), 
				(LPCTSTR)m_pDoc->GetTitle(),
				(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()),
				CXMLLogSpace::XML_INFO
			);
	}

	if (pTableToRelock) pTableToRelock->LockCurrent();
	// prendo i messaggi di errore o warning del documento
	EndListeningToDoc(m_pDoc);
	UnlockFields (pDBTMaster);
	m_bBusy = FALSE;


	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::DeleteDocument(DBTMaster *pDBTMaster, SqlRecord* pNewMasterRec)
{
	// caso in cui devo eliminare il record
	if (!pDBTMaster->FindData(FALSE))
	{
		OutputMessage(cwsprintf
			(
				_TB("Document to be deleted {0-%s} not found. Document keys: {1-%s}."), 
				(LPCTSTR)m_pDoc->GetTitle(),
				(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()
			),
			CXMLLogSpace::XML_WARNING);		

		UnlockFields (pDBTMaster);
		return TRUE;
	}

	if (!m_pDoc->OnOkXMLDeleteImport())
	{
		UnlockFields (pDBTMaster);
		return FALSE;
	}

	// catturo i messaggi del documento
	BeginListeningToDoc(m_pDoc);
	UnlockFields (pDBTMaster); // effettuo l'unlock altrimenti quando il browser effettua il MoveNext (nel metodo CAbstractFormDoc::DeleteRecord()) non riesce a scrivere i campi chiave del DBTMaster
	BOOL bOk = m_pDoc->DeleteImportDocument ();

	if (bOk)
		OutputMessage(cwsprintf
			(
				_TB("Document: {0-%s} deletd.\r\n Document keys: {1-%s}."), 
				(LPCTSTR)m_pDoc->GetTitle(),
				(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription()
			),
				CXMLLogSpace::XML_INFO);
	else
		OutputMessage(cwsprintf
			(
				_TB("Failed to delete document: {0-%s}.\r\n Document keys: {1-%s}."), 
				(LPCTSTR)m_pDoc->GetTitle(),
				(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription())
			);
	
	// prendo i messaggi di errore o warning del documento
	EndListeningToDoc(m_pDoc);
	return bOk;
}

//----------------------------------------------------------------------------
// sto entrando in modifica, ho trovato il record locckato; 
// lo sto locckando io col processo di import (allora devo fregarmene del lock)
// oppure lo sta locckando qualcun altro?
// opero in questo modo:
// 1) pesco tutti i documenti in EDIT mode nel mio buffer dei documenti di importazione 
//		il cui namespace è uguale a quello del documento corrente
// 2) per ognuno, effettuo la unlock e provo a prendere il lock per il documeto corrente
// 3) se non mi riesce, vuol dire che il lock non appartiene alla procedura di importazione
// 4) se mi riesce, di fatto ho sostituito il lock precedente con quello attuale:
//		restituisco la SqlTable 'slocckata' in modo da ripristinare il lock quando
//		ho finito di importare questo documento
// NOTA: può sembrare inefficiente (in effetti lo è) ma questa eventualità accade molto raramente:
// accade cioè in presenza di documenti ricorsivi (cioè che hanno un external reference che importa
// un loro duplicato ma con un diverso profilo). Anche il numero di documenti nel buffer con lo stesso namespace 
// ed in stato di EDIT nella maggior parte dei casi è limitato (e di solito uguale ad 1, cioè quello
// che effettivamente mi sta impedendo di entrare in EDIT mode
SqlTable* CXMLDataManager::GetUnlockedTable()
{
	Array arDocs;
	CAbstractFormDoc *pDoc=NULL;
	m_pXMLExpImpMng->GetDocsInEditMode(m_pDoc->GetNamespace(), arDocs);

	SqlTable* pTable = NULL;
	for (int i=0; i<arDocs.GetSize(); i++)
	{
		pDoc = (CAbstractFormDoc*) arDocs.GetAt (i);
		ASSERT_KINDOF(CAbstractFormDoc, pDoc);
		
		pTable = pDoc->m_pDBTMaster->GetTable();
		pTable->UnlockCurrent();

		m_pDoc->m_pMessages->ClearMessages();
		
		m_pDoc->OnRadarRecordSelected(TRUE);
		if (m_pDoc->ValidCurrentRecord() && m_pDoc->GetFormMode()==CAbstractFormDoc::EDIT)
		{
			return pTable;
		}
		
		pTable->LockCurrent(FALSE);
	}
	return NULL;
}

//START OLDBEHAVIOUR
//----------------------------------------------------------------------------
void CXMLDataManager::FreezeActiveTabs (CUIntArray* pTabberList, CUIntArray* pTabList)
{
	ASSERT(pTabberList);
	ASSERT(pTabList);

	POSITION pos = m_pDoc->GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = m_pDoc->GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			TabManagers* pTabs = ((CAbstractFormView*) pView)->m_pTabManagers;
			if (!pTabs)
			{
				ASSERT(FALSE);
				continue;
			}
			for (int i=0; i<pTabs->GetSize(); i++)
			{
				CTabManager *pTab = pTabs->GetAt (i);
				if (!pTab)
				{
					ASSERT(FALSE);
					continue;
				}
				
				pTabberList->Add (pTab->GetDlgCtrlID ());
				pTabList->Add (pTab->GetActiveTabID());
			}
		}
	}
}


//----------------------------------------------------------------------------
void CXMLDataManager::RestoreActiveTabs (CUIntArray* pTabberList, CUIntArray* pTabList)
{
	ASSERT(pTabberList);
	ASSERT(pTabList);

	ASSERT(pTabberList->GetSize () ==  pTabList->GetSize ());

	for (int i=0; i<pTabberList->GetSize (); i++)
		m_pDoc->TabDialogActivate (pTabberList->GetAt (i), pTabList->GetAt (i));
}

//----------------------------------------------------------------------------
void CXMLDataManager::ActivateAllTabs ()
{
	POSITION pos = m_pDoc->GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = m_pDoc->GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			TabManagers* pTabs = ((CAbstractFormView*) pView)->m_pTabManagers;
			if (!pTabs)
			{
				ASSERT(FALSE);
				continue;
			}
			for (int i=0; i<pTabs->GetSize(); i++)
			{
				CTabManager *pTab = pTabs->GetAt (i);
				if (!pTab)
				{
					ASSERT(FALSE);
					continue;
				}

				DlgInfoArray* pInfoArray = pTab->GetDlgInfoArray();
				if (!pInfoArray)
				{
					ASSERT(FALSE);
					continue;
				}
				for (int j=0; j<pInfoArray->GetSize(); j++)
				{
					DlgInfoItem* pDlgItem = pInfoArray->GetAt(j);
					if (!pDlgItem)
					{
						ASSERT(FALSE);
						continue;
					}

					pTab->TabDialogActivate(pTab->GetDlgCtrlID(), pDlgItem->GetDialogID());
				}
			}
		}
	}
}
//END OLDBEHAVIOUR

//optimization  
//----------------------------------------------------------------------------
void CXMLDataManager::PrepareRequiredTabDlgs()
{
	POSITION pos = m_pDoc->GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = m_pDoc->GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CMasterFormView)))
		{
			TabManagers* pTManagers = ((CAbstractFormView*) pView)->m_pTabManagers;
			if (!pTManagers)
				continue;

			for (int i = 0; i < pTManagers->GetSize(); i++)
			{
				CTabManager* pTabMng = pTManagers->GetAt (i);
				if (!pTabMng)
					continue;
				
				DlgInfoArray* pInfoArray = pTabMng->GetDlgInfoArray();
				if (!pInfoArray)
					continue;
			
				CRequiredTabManager* pReqTabMng = new CRequiredTabManager(pTabMng);
				m_requiredTabManagers.Add(pReqTabMng);
				pReqTabMng->Add(pTabMng->GetActiveDlg());
				TRACE(cwsprintf(_T("ImpWiz-Req TM- Tab: %s\n"), pTabMng->GetActiveDlg()->GetFormName()));

				pTabMng->SetKeepTabDlgAlive(TRUE);	//I avoid to destroy the tabdialogs 

				for (int j=0; j < pInfoArray->GetSize(); j++)
				{
					DlgInfoItem* pDlgItem = pInfoArray->GetAt(j);
					if (!pDlgItem)
						continue;
					if (pTabMng->GetActiveDlg() == pDlgItem->m_pBaseTabDlg)
						continue;	

					//evito di inserire nell'array o un ptr a NULL o un alias di un ptr
					//quando l'attivazione di una tab fallisce ad esempio per la security
					UINT uiPrevIDD = pTabMng->GetActiveTabID();

					int ok = pTabMng->TabDialogActivate(pTabMng->GetDlgCtrlID(), pDlgItem->GetDialogID());
					ASSERT_VALID(pTabMng->GetActiveDlg());

					if (ok > 0 && pTabMng->GetActiveDlg() && uiPrevIDD != pTabMng->GetActiveTabID())
					{
						pReqTabMng->Add(pTabMng->GetActiveDlg());
						TRACE(cwsprintf(_T("ImpWiz-Req TM- Tab: %s\n"), pTabMng->GetActiveDlg()->GetFormName()));
					}
				}
			}
		}
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::RestoreActiveTabs()
{
	for (int tm = 0; tm < m_requiredTabManagers.GetSize(); tm++)
	{
		CRequiredTabManager* pReqTabMng = (CRequiredTabManager*)m_requiredTabManagers.GetAt(tm);
		if (pReqTabMng && pReqTabMng->m_pTabMng)
		{
			int nSize = pReqTabMng->m_arRequiredTabDlgs.GetSize();
			if (nSize > 0)
			{
				//activate the original actived tabdialog
				int ok = m_pDoc->TabDialogActivate(pReqTabMng->m_pTabMng->GetDlgCtrlID(), pReqTabMng->m_pActivedTabDlg->GetDlgInfoItem()->GetDialogID());
				if (ok < 1)
				{
					ASSERT_TRACE(FALSE, _T("CXMLDataManager::RestoreActiveTabs() fails to restore original Tab "));
					//try to avoid a crash
					ASSERT_VALID(pReqTabMng->m_pTabMng->GetActiveDlg());
					pReqTabMng->m_pActivedTabDlg = pReqTabMng->m_pTabMng->GetActiveDlg();
				}

				for (int i = 0; i < nSize; i++)
				{
					CTabDialog* pTabDlg =  (CTabDialog*)(pReqTabMng->m_arRequiredTabDlgs.GetAt(i));
					ASSERT_VALID(pTabDlg);
						
					if (pTabDlg && pTabDlg != pReqTabMng->m_pActivedTabDlg)
					{
						if (pReqTabMng->m_pTabMng->GetNormalTabber())
							pReqTabMng->m_pTabMng->GetNormalTabber()->CleanTabState(pTabDlg);
						pTabDlg->DestroyWindow();					
					}

					pReqTabMng->m_arRequiredTabDlgs.SetAt(i, NULL);
				}	
			}
			pReqTabMng->m_pTabMng->SetKeepTabDlgAlive(FALSE);
		}
		if (pReqTabMng) pReqTabMng->m_arRequiredTabDlgs.RemoveAll();
	}
	m_requiredTabManagers.RemoveAll();
}

//----------------------------------------------------------------------------
void CXMLDataManager::ActivateRequiredTabDlgs()
{
	for (int nTabM = 0; nTabM < m_requiredTabManagers.GetSize(); nTabM++)
	{
		CRequiredTabManager* pReqTabMng = (CRequiredTabManager*) (m_requiredTabManagers.GetAt(nTabM));
		if (pReqTabMng && pReqTabMng->m_pTabMng)
		{
			for (int i = 0; i < pReqTabMng->m_arRequiredTabDlgs.GetSize(); i++)
			{
				CTabDialog* pTabDlg =  (CTabDialog*)(pReqTabMng->m_arRequiredTabDlgs.GetAt(i));
				if (pTabDlg)
				{
					pTabDlg->OnPrepareAuxData();

					if (m_pDoc->IsExternalControlled())
						pTabDlg->SyncExternalControllerInfo(FALSE);

					m_pDoc->OnPrepareAuxData(pTabDlg);
				}
			}
		}
	}
}

//---------------------------------------------------------------------------
void CXMLDataManager::AddFieldToEventTable(const CString& strTableName, CXMLNode* pFieldsNode)
{
	CXMLEventManager* pMng = (CXMLEventManager*)m_pDoc->m_pEventManager;
	if (!pMng || !pMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
		return;

	AddFieldToEventTable(pMng->GetEvents(strTableName, CXMLEventManager::AFTER_CHANGE), pFieldsNode);
}

//---------------------------------------------------------------------------
void CXMLDataManager::AddFieldToEventTable(CTableEvents* pTableEvents, CXMLNode* pFieldsNode)
{
	if (!pTableEvents)
		return;

	CXMLNode* pChildNode = pFieldsNode->GetFirstChild();
	while (pChildNode)
	{
		// Carico solo i dati relativi al master per poter controllare 
		// l'esistenza o meno del record
		CString strFieldName;
		pChildNode->GetBaseName(strFieldName);
		pTableEvents->m_arImportingFields.Add(strFieldName);

		pChildNode = pFieldsNode->GetNextChild();
	}
	pTableEvents->m_bImportingFieldsLoaded = TRUE;
}

//verifica se il campo della funzione è presente nel documento da importare
//ovvero se la funzione poi verrà effettivamente chiamata
//#bugfix 18457 
//---------------------------------------------------------------------------
BOOL CXMLDataManager::CanFireFunction(CFieldFunction* pFunction, SqlRecord* pRecord, CXMLNode* pSlavesNode)
{
	if (!pFunction || !pFunction->m_pFieldEvent)
		return FALSE;
	//prima verifico se l'evento che devo chiamare è della stessa tabella del pRecord passato
	//se così fosse guardo semplicemente se il DataObj è locked (ovvero è stato importato) senza
	//dover andare a leggere l'xmlDom in memoria (più oneroso)
	if (!pFunction->m_pFieldEvent->m_strTableName.CompareNoCase(pRecord->GetTableName()))
	{
		DataObj* pDataObj = pRecord->GetDataObjFromColumnName(pFunction->m_pFieldEvent->m_strFieldName);
		return (pDataObj && pDataObj->IsValueLocked());
	}
	
	// Caso 1: l'evento è di un campo della tabella master
	if (!pFunction->m_pFieldEvent->m_strTableName.CompareNoCase(m_pDoc->GetMaster()->GetRecord()->GetTableName()))
	{
		DataObj* pDataObj = m_pDoc->GetMaster()->GetRecord()->GetDataObjFromColumnName(pFunction->m_pFieldEvent->m_strFieldName);
		return (pDataObj && pDataObj->IsValueLocked());
	}

	//Caso2: l'evento è di un campo della tabella slave. 

	//Se i campi da imporare sono già conosciuti (pTableEvents->m_bImportingFieldsLoaded == TRUE) allora verifico
	//l'esistenza del campo in memoria (pTableEvents->m_arImportingField) e non lo leggo dall'xml
	//E' possibile che pTableEvents->m_arImportingField sia già stato caricato perchè il dbtslave corrispondente
	//l'ho già importato (ed in fase di imporazione riempio se esisteno degli eventi ad esso accociato la pTableEvents->m_arImportingField)
	CTableEvents* pTableEvents = pFunction->m_pFieldEvent->m_pTableEvents;
	if (pTableEvents->m_bImportingFieldsLoaded)
		return pTableEvents->IsAnImportingField(pFunction->m_pFieldEvent->m_strFieldName);

	//se non ancora caricata faccio un lookhead e leggo le informazioni dal nodo xml del dbtslave corrispondente 
	//e mi carico la m_arImportingFields
	
	CXMLNode* pSlaveNode = pSlavesNode->GetFirstChild();
	CString strSlaveType;
	CString strSlaveTable;
	CString strNameSpace;		
	while (pSlaveNode)
	{
		if (!GetSlaveNodeInfo(pSlaveNode, strSlaveType, strSlaveTable, strNameSpace))
			return FALSE;

		//ho trovato il dbtslave corrispondente alla tabella dell'evento
		if (!pTableEvents->m_strTableName.CompareNoCase(strSlaveTable))
		{
			//carico i fields presenti
			// Slave
			if (!strSlaveType.CompareNoCase((LPCTSTR)DOC_XML_SLAVE_TAG))
			{
				AddFieldToEventTable(pTableEvents, pSlaveNode);
				return pTableEvents->IsAnImportingField(pFunction->m_pFieldEvent->m_strFieldName);
			}
			else
			{
				CXMLNode* pRowNode = pSlaveNode->GetFirstChild(); 
				if (pRowNode!=NULL)
				{
					CString strRowTag;
					pRowNode->GetBaseName(strRowTag);
					if (!strRowTag.CompareNoCase(XML_ROW_TAG))
						AddFieldToEventTable(pTableEvents, pRowNode);
					return pTableEvents->IsAnImportingField(pFunction->m_pFieldEvent->m_strFieldName);
				}
			}
		}
		pSlaveNode = pSlavesNode->GetNextChild();
	}
	//se sono arrivata a questa istruzione vuol dire che il dbt non è presente  nell'envelope per cui è inutile 
	//che ogni volta cerchi di caricare i suoi campi. 
	pTableEvents->m_bImportingFieldsLoaded = TRUE;
	return FALSE;
}

//---------------------------------------------------------------------------
void CXMLDataManager::ComposeEventsToFire(CTableEvents* pTableEvents, SqlRecord* pRecord, CXMLNode* pSlavesdNode)
{
	// I consider only the fields with an event name linked in Action.xml file	
	CXMLEventManager* pMng = (CXMLEventManager*)m_pDoc->m_pEventManager;
	if (!pMng || !pMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
		return;

	if (pTableEvents->m_bPrepared)
		return;

	
	CFieldEvent* pFieldEvent = NULL;
	CFieldFunction* pFieldFunction = NULL;

	for (int k = 0; k < pTableEvents->m_arFieldEvents.GetSize(); k++)
	{
		pFieldEvent = (CFieldEvent*)pTableEvents->m_arFieldEvents.GetAt(k);
		if (pFieldEvent)
		{
			for (int j = 0; j < pFieldEvent->m_arFunctions.GetSize(); j++)
			{
				pFieldFunction = (CFieldFunction*)pFieldEvent->m_arFunctions.GetAt(j);
				if (pFieldFunction && pFieldFunction->m_arReleatedFields.GetSize() <= 0 || m_bTuningEnable)
					pFieldFunction->m_ToBeFired = TRUE; //in fase di tuning devono scattare tutti gli eventi
			}
		}
	}
	
	if (m_bTuningEnable)	
		return;	

	CReleatedField* pReleatedField = NULL;
	SqlRecordItem* pItemRec = NULL;
	
	for (int i = 0; i <= pRecord->GetUpperBound(); i++)
	{
		pItemRec = pRecord->GetAt(i);
		if (pItemRec && !pItemRec->GetDataObj()->IsValueLocked())
		{
			//verifico se il campo deve essere modificato
			for (int j = 0; j <= pMng->m_arReleatedFields.GetUpperBound(); j++)
			{
				pReleatedField = (CReleatedField*)pMng->m_arReleatedFields.GetAt(j);
				if (
						pReleatedField  &&
						!pReleatedField->m_strTableName.CompareNoCase(pRecord->GetTableName()) &&
						!pReleatedField->m_strFieldName.CompareNoCase(pItemRec->GetColumnName())
					)
					//#bugfix 18457
					//secondo il LIFO considero l'ultima funzione letta corrispondente ad un campo effettivamente
					//importato
					//ExistFieldInEnvelope(pReleatedField->m_pChangingFunct->m_pFieldEvent);
					//devo considerare l'ultimo evento che modifica il campo. 
					for (int i = 0 ; i <= pReleatedField->m_arChangingFuncts.GetUpperBound(); i++)
					{
						CFieldFunction* pFunction = (CFieldFunction*)pReleatedField->m_arChangingFuncts.GetAt(i);
						
						//già qualche altro ha bisogno del fire della funzione allora non eseguo gli altri controlli
						if (pFunction->m_ToBeFired)
							break;

						if (CanFireFunction(pFunction, pRecord, pSlavesdNode))
						{
							pReleatedField->SetFunction(pFunction);
							pFunction->m_ToBeFired = TRUE;							
							break;
						}
					}
			}
		}
	}

	pTableEvents->m_bPrepared = TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::FireRecEvents(SqlRecord* pRecord, CXMLNode* pSlavesNode)
{
	ASSERT_KINDOF(CAbstractFormDoc, m_pDoc);
	ASSERT(pRecord);

	// I consider only the fields with an event name linked in Action.xml file	
	CXMLEventManager* pMng = (CXMLEventManager*)m_pDoc->m_pEventManager;
	if (!pMng || !pMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
		return;

	if (pRecord->GetTableName().CompareNoCase(m_sLastEventsTableName))
	{
		m_pTableEvents = pMng->GetEvents(pRecord->GetTableName(), CXMLEventManager::AFTER_CHANGE);
		m_sLastEventsTableName = pRecord->GetTableName();
	}
	
	if (!m_pTableEvents)
		return;

	if (UseOldXTechMode())
	{
		pMng->OnAfterChangeField (pRecord, m_pTableEvents);
		return;
	}
	
	ComposeEventsToFire(m_pTableEvents, pRecord, pSlavesNode);
	
	CFieldEvent* pFieldEvents = NULL;
	CFieldFunction* pFieldFunction =  NULL;
	for (int i = 0; i <= m_pTableEvents->m_arFieldEvents.GetUpperBound(); i++)
	{
		pFieldEvents = (CFieldEvent*)m_pTableEvents->m_arFieldEvents.GetAt(i);
		if (!pFieldEvents || pFieldEvents->m_strFieldName.IsEmpty() || pFieldEvents->m_arFunctions.GetCount() == 0)
			continue;
				
		DataObj *pDataObj = pRecord->GetDataObjFromColumnName(pFieldEvents->m_strFieldName);			
		if (pDataObj  && (pDataObj->IsValueLocked() || m_bTuningEnable))
		{

			for (int j = 0; j < pFieldEvents->m_arFunctions.GetCount(); j++)
			{
				pFieldFunction = (CFieldFunction*)pFieldEvents->m_arFunctions.GetAt(j);
				if (pFieldFunction && pFieldFunction->m_ToBeFired)
				{
					if (m_bTuningEnable)
						m_pObserverContext->StartObserving(ON_CHANGING);

					pDataObj->Fire(ON_CHANGED);
					if (m_bTuningEnable)
					{
						CheckChangingDataObj(pFieldFunction);
						m_pObserverContext->EndObserving(ON_CHANGING);	 
					}
					else	
						break; // if i don't tune only one changed fired event is enough		
				}
			}
		}				
	}	
}


//I have to fire event on the prototype record but I have to consider the currentrow to test if the dataobj is locked or not
//----------------------------------------------------------------------------
void CXMLDataManager::FireDBTSlaveRecEvents(SqlRecord* pPrototypeRecord, SqlRecord* pCurrentRow, CXMLNode* pChildNode)
{
	DataObj* pDataObj = NULL;
	
	CXMLEventManager* pMng = (CXMLEventManager*)m_pDoc->m_pEventManager;
	if (!pMng || !pMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
		return;

	if (pPrototypeRecord->GetTableName().CompareNoCase(m_sLastEventsTableName))
	{
		m_pTableEvents = pMng->GetEvents(pPrototypeRecord->GetTableName(), CXMLEventManager::AFTER_CHANGE );
		m_sLastEventsTableName = pPrototypeRecord->GetTableName();
	}

	if (!m_pTableEvents)
		return;

	if (UseOldXTechMode())
	{
		pMng->OnAfterChangeField (pCurrentRow, m_pTableEvents);
		return;
	}

	ComposeEventsToFire(m_pTableEvents, pCurrentRow, pChildNode);	

	CFieldFunction* pFieldFunction =  NULL;
	CFieldEvent* pFieldEvents = NULL;

	// I consider only the fields with an event name linked in Action.xml file	
	for (int i = 0; i <= m_pTableEvents->m_arFieldEvents.GetUpperBound(); i++)
	{
		pFieldEvents = (CFieldEvent*)m_pTableEvents->m_arFieldEvents.GetAt(i);

		if (!pFieldEvents || pFieldEvents->m_strFieldName.IsEmpty() || pFieldEvents->m_arFunctions.GetCount() == 0)
			continue;
				
		int nIdx = pCurrentRow->GetIndexFromColumnName(pFieldEvents->m_strFieldName);
		if (nIdx < 0)
			continue;

		pDataObj = pCurrentRow->GetDataObjAt(nIdx);
		if (pDataObj && (pDataObj->IsValueLocked() || m_bTuningEnable))
		{
			for (int j = 0; j < pFieldEvents->m_arFunctions.GetCount(); j++)
			{
				pFieldFunction = (CFieldFunction*)pFieldEvents->m_arFunctions.GetAt(j);
				if (pFieldFunction && pFieldFunction->m_ToBeFired)
				{
	
					if (m_bTuningEnable)
						m_pObserverContext->StartObserving(ON_CHANGING);
		
					//fire the events
					pDataObj->Fire(ON_CHANGED);		
			 
					// if i don't tune only one changed fired event is enough
					if (!m_bTuningEnable)
						break;
					
					CheckChangingDataObj(pFieldFunction); 	
					m_pObserverContext->EndObserving(ON_CHANGING);
				}
			}
		}	
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::CheckChangingDataObj(CFieldFunction* pFieldFunction)
{
	if (!m_pDoc->m_pEventManager || !m_pObserverContext)
		return;
			
	ObservableMap* pObservableMap = m_pObserverContext->GetObservableMap(ON_CHANGING);

	if (!pObservableMap)
		return;

	POSITION pos = pObservableMap->GetStartPosition();
	CObservable* pObservable;
	CDataFieldEvents* pDataEvents = NULL;
	CDataEventsObj* pDataEventsObj = NULL;;
	while (pos)
	{
		pObservableMap->GetNextAssoc(pos, pObservable, pDataEventsObj);
		if (pDataEventsObj)
		{
			pDataEvents = (CDataFieldEvents*)pDataEventsObj;
			if (pFieldFunction &&  pDataEvents->m_pColumnInfo)
			{
				((CXMLEventManager*)m_pDoc->m_pEventManager)->AddReleatedField(pFieldFunction, pDataEvents->m_pColumnInfo->m_strTableName, pDataEvents->m_pColumnInfo->m_strColumnName); 
				if (m_pTableEvents)
					m_pTableEvents->m_bModified = TRUE;
			}
		}
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::UnlockFields (DBTMaster* pDBTMaster)
{
	ASSERT(pDBTMaster);
	UnlockFields (pDBTMaster->GetRecord ());
		
	for (int i=0; i<pDBTMaster->GetDBTSlaves()->GetSize (); i++)
	{
		DBTSlave *pSlave = pDBTMaster->GetDBTSlaves()->GetAt (i);
		ASSERT(pSlave);
		if (pSlave->IsKindOf (RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			DBTSlaveBuffered *pSlaveBuff = (DBTSlaveBuffered*)pSlave;
			for (int j=0; j<pSlaveBuff->GetSize (); j++)
				UnlockFields(pSlaveBuff->GetRow (j));
		}
		else if (pSlave->IsKindOf (RUNTIME_CLASS(DBTSlave)))
			UnlockFields(pSlave->GetRecord());	
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::UnlockFields (SqlRecord* pRecord)
{
	ASSERT(pRecord);
	for (int i=0; i<pRecord->GetSize (); i++)
		pRecord->GetDataObjAt (i)->SetValueLocked (FALSE);

}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportRecordFields
							(
								CXMLNode*		pRecordNode,
								SqlRecord*		pDBTRecord
							) 
{
	if (!pRecordNode || !pRecordNode)
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bErrorOccurred = FALSE;

	CXMLNode *pFieldsNode = pRecordNode->GetChildByName (DOC_XML_FIELDS_TAG);
	if (!pFieldsNode) 
	{
		// se non c'è il nodo fields, significa che devo inizializzare il record
		// cancellando eventuali dati presenti
		pDBTRecord->Init();
		return TRUE;
	}

	CXMLNode* pChildNode = pFieldsNode->GetFirstChild();
	while (pChildNode)
	{
		// Carico solo i dati relativi al master per poter controllare 
		// l'esistenza o meno del record
		CString strFieldName;
		pChildNode->GetBaseName(strFieldName);

		int nImportFldRc = ImportRecordField(pChildNode, pDBTRecord, strFieldName);
		
		if (nImportFldRc == IMPORT_RECFLD_ERROR_FIELD_NOTFOUND)
			OutputMessage(cwsprintf(_TB("Error importing the field '{0-%s}' in the table '{1-%s}'."), (LPCTSTR)strFieldName, (LPCTSTR)pDBTRecord->GetTableName()), CXMLLogSpace::XML_WARNING);
		
		if (nImportFldRc == IMPORT_RECFLD_ERROR_INVALID_VALUE)
		{
			OutputMessage(cwsprintf(_TB("Error importing the field '{0-%s}' in the table '{1-%s}'."), (LPCTSTR)strFieldName, (LPCTSTR)pDBTRecord->GetTableName()), CXMLLogSpace::XML_WARNING);
			bErrorOccurred = TRUE;
		}

		if (nImportFldRc == IMPORT_RECFLD_ERROR_EXT_REFERENCE)
			bErrorOccurred = TRUE;

		pChildNode = pFieldsNode->GetNextChild();
	}
	
	if (!bErrorOccurred && !UseOldXTechMode())
		 AddFieldToEventTable(pDBTRecord->GetTableName(), pFieldsNode);

	return !bErrorOccurred;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::CheckUniversalKey(CXMLNode *pNode, CAbstractFormDoc *pDoc)
{
	if (!pNode || !pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode *pUniversalKey = pNode->GetChildByName (XML_UNIVERSAL_KEYS_TAG);

	CStringArray strArray;
	pDoc->GetUniversalKeyList(strArray);
	BOOL bUseUniversalKey = strArray.GetSize()>0;

	// se il documento gestisce universal key ma l'envelope non ce l'ha
	// chiedo al documento se posso andare aventi lo stesso (di default, proseguo con la chiave
	// originaria)
	BOOL bIsThereUniversalKey = FALSE;
	if (bUseUniversalKey)
		bIsThereUniversalKey = RearrangeUniversalKey(pUniversalKey, strArray);

	// se non devo usarla, oppure è vuota, la rimuovo
	if ((!bUseUniversalKey || !bIsThereUniversalKey) && pUniversalKey)
		pNode->RemoveChild (pUniversalKey);
		
	// se devo usarla, è vuota e il documento mi dice che deve esserci, vado in errore
	if (bUseUniversalKey && 
		!bIsThereUniversalKey&& 
		!m_pDoc->OnOkXMLImportUniversalKeyEmpty())
	{
		OutputMessage(cwsprintf(_TB("The document {0-%s} requires the Universal Key management, but this is not present in the envelope you want to import."), m_pDoc->GetTitle()));
		return FALSE;
	}

	return TRUE;
}

 //----------------------------------------------------------------------------
SqlRecord* CXMLDataManager::GetCurrentRecord(DataObjArray& arSegs, DBTSlaveBuffered* pDBT, CXMLDBTInfo::UpdateType eUpdateType, CXMLNode* pRowNode, int& nCurrentRow)
{
	SqlRecord* pSlaveRec = pDBT->AddRecord();
	nCurrentRow = pDBT->GetUpperBound();
	if (pSlaveRec && pRowNode)
	{
		GetForeignKeySegments(arSegs, pSlaveRec, pDBT, nCurrentRow);
		if (eUpdateType == CXMLDBTInfo::REPLACE)
			return pSlaveRec;				
		
		DataObjArray keys;
		CString strFieldValue;
		CString strColName;
							
		pSlaveRec->GetKeyStream (keys, FALSE);
		for (int i =0 ; i < keys.GetSize(); i++)
		{
			//se non è una FK la valorizza con il dato letto dall'XML
			if (!keys.GetAt(i)->IsModified ())
			{
				strColName = pSlaveRec->GetColumnName(keys.GetAt(i));
				CXMLNode* pFieldNode = pRowNode->GetChildByName(pSlaveRec->GetColumnName(keys.GetAt(i)), FALSE);
				if (!pFieldNode) continue;
				// se il valore è espresso come attributo, prendo quello
				// altrimenti prendo il testo del nodo
				if (!pFieldNode->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strFieldValue))
					pFieldNode->GetText(strFieldValue);
				keys.GetAt(i)->AssignFromXMLString(strFieldValue);
			}
		}
	
		for (int nIdx = 0; nIdx < pDBT->GetUpperBound(); nIdx++)
		{
			if (pDBT->IsDuplicateKey(pSlaveRec, pDBT->GetRow(nIdx))	)			
			{
				if (eUpdateType == CXMLDBTInfo::ONLY_INSERT)
				{
					pSlaveRec = NULL;
					nCurrentRow = -1;
				}
				else
				{
					//devo considerare i segmenti di FK del record già esistente
					for (int nSeg = 0; nSeg < arSegs.GetSize(); nSeg++)
					{
						strColName = pSlaveRec->GetColumnName(arSegs.GetAt(nSeg));
						DataObj* pDataObj = pDBT->GetRow(nIdx)->GetDataObjFromColumnName(strColName);
						pDataObj->SetModified();
						arSegs.SetAt(nSeg, pDataObj);
					}
					pSlaveRec = pDBT->GetRow(nIdx);							
					nCurrentRow = nIdx;					
				}
				pDBT->DeleteRecord(pDBT->GetUpperBound());					
				break;
			}
		}
	}
	return pSlaveRec;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportDBTSlaves(CXMLNode* pChildNode, BOOL bWithExtref /*=TRUE*/ )
{
	if (!pChildNode->GetChildsNum())
		return TRUE; 

	CXMLNode* pSlaveNode = pChildNode->GetFirstChild();
	
	DBTMaster* pDBTMaster = m_pDoc->m_pDBTMaster;
	ASSERT_VALID(pDBTMaster);
	
	DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
	
	if (!pDBTSlaves)
	{
		if (pSlaveNode)
		{
			OutputMessage(_TB("Data import in the related tables failed."));
			return FALSE;
		}
		else
			return TRUE;
	}

	CXMLDBTInfo::UpdateType eUpdateType;
	// Carico i dati degli slave
	while (pSlaveNode)
	{
		CString strSlaveType;
		CString strSlaveTable;
		CString strNameSpace;		
		if (!GetSlaveNodeInfo(pSlaveNode, strSlaveType, strSlaveTable, strNameSpace))
		{
			OutputMessage(cwsprintf (_TB("Syntax error in {0-%s} file. Unexpected {1-%s} node instead of a Slave or SlaveBuff node"), (LPCTSTR)pSlaveNode->GetXMLDocument()->GetFileName(), (LPCTSTR)strSlaveType));
			return FALSE;
		}
					
					
		if(!strSlaveTable.IsEmpty())
		{
			DBTSlave* pDBTSlave = NULL;
			// Cerco il DBT corrispondente tra quelli attaccati al master
			for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
			{
				SqlTable* pSlaveTable = pDBTSlaves->GetAt(i)->GetTable();
				if (strSlaveTable.CompareNoCase((LPCTSTR)pSlaveTable->GetTableName()) == 0)
				{
					// Se nel file di dati viene riportato il namespace del DBT allora controllo che 
					// coincida altrimenti mi baso solo sul nome della tabella
					if (strNameSpace.CompareNoCase(pDBTSlaves->GetAt(i)->GetNamespace().ToString()))
						continue;

					pDBTSlave = pDBTSlaves->GetAt(i);
					break;
				}
			}
			
			if (!pDBTSlave)
			{
				OutputMessage(cwsprintf(_TB("The slave {0-%s} on the table {1-%s} is not attached in the document {2-%s}"), strNameSpace, strSlaveTable, m_pDoc->GetNamespace().ToString()));		
				return FALSE;
			}

			CString strUpdateType;
			pSlaveNode->GetAttribute(DOC_XML_UPDATE_ATTRIBUTE, strUpdateType);
			
			CXMLDBTInfo* pDBTInfo = pDBTSlave->GetXMLDBTInfo();
			if (!pDBTInfo)
			{
				pSlaveNode = pChildNode->GetNextChild();
				continue;
			}
			
			eUpdateType = pDBTInfo->GetUpdateTypeFromString(strUpdateType);	
			
			// Slave
			if (!strSlaveType.CompareNoCase((LPCTSTR)DOC_XML_SLAVE_TAG))
			{
				ASSERT(pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlave)));
				SqlRecord* pSlaveRec = pDBTSlave->GetRecord();
				
				if (pSlaveRec)
				{
					if (eUpdateType == CXMLDBTInfo::REPLACE)
						pSlaveRec->Init(); 
					else
						if (eUpdateType == CXMLDBTInfo::ONLY_INSERT && !pSlaveRec->IsEmpty())
						{
							pSlaveNode = pChildNode->GetNextChild();
							continue;
						}
				
					DataObjArray keys;
					GetForeignKeySegments(keys, pSlaveRec, pDBTSlave);
							
					//prima della valorizzazione dei campi
					pDBTSlave->OnBeforeXMLImport ();
					
					//importo i documenti referenziati
					if (bWithExtref && !ImportExtRef(pSlaveNode, pSlaveRec))
						return FALSE;
					
					if (!ImportRecordFields(pSlaveNode, pSlaveRec))
						return FALSE;
					
					if (!pDBTSlave->OnOkXMLImport())
						return FALSE;
										
					//dopo la valorizzazione dei campi
					pDBTSlave->OnAfterXMLImport ();
					
					// records events
					FireRecEvents(pSlaveRec, pChildNode);

					pSlaveRec->SetStorable();

					//rilascio il lock sui campi cha dovranno essere riallineati al master
					for(int i =0; i<keys.GetSize (); i++)
						keys.GetAt (i)->SetValueLocked (FALSE);

				}
			}
			// Slave Buffered
			else if (!strSlaveType.CompareNoCase((LPCTSTR)DOC_XML_SLAVEBUFF_TAG))
			{
				ASSERT(pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)));
				//CString strRowsNum;
				//int nRowsNum = 
				//		(
				//			pSlaveNode->GetAttribute(DOC_XML_ROWSNUMBER_ATTRIBUTE, strRowsNum) 
				//			&& !strRowsNum.IsEmpty()
				//		) 
				//			? _ttoi((LPCTSTR)strRowsNum)
				//			: 1;
				int nProcessedSlaveBuffRows = 0;
				
				//prima di operare sulle righe
				pDBTSlave->OnBeforeXMLImport ();
				
				// cancello gli elementi esistenti				
				if (eUpdateType == CXMLDBTInfo::REPLACE)
					((DBTSlaveBuffered*)pDBTSlave)->RemoveAll();
				
				for (CXMLNode* pRowNode = pSlaveNode->GetFirstChild(); pRowNode!=NULL; pRowNode = pSlaveNode->GetNextChild())
				{
					CString strRowTag;
					pRowNode->GetBaseName(strRowTag);
					if (!strRowTag.CompareNoCase(XML_ROW_TAG))
					{
						DataObjArray keys;			
						int nCurrentRow = -1;
						SqlRecord* pSlaveRec = GetCurrentRecord(keys, (DBTSlaveBuffered*)pDBTSlave, eUpdateType, pRowNode->GetChildByName(DOC_XML_FIELDS_TAG), nCurrentRow);

						if (pSlaveRec)
						{						
							//prima della valorizzazione dei campi
							((DBTSlaveBuffered*)pDBTSlave)->OnBeforeXMLImport(nCurrentRow);
					
							//importo i documenti referenziati
							if (bWithExtref && !ImportExtRef(pRowNode, pSlaveRec))
								return FALSE;

							((DBTSlaveBuffered*)pDBTSlave)->SetCurrentRow (nCurrentRow);

							if (!ImportRecordFields(pRowNode, pSlaveRec))
								return FALSE;
							
							nProcessedSlaveBuffRows++;

							if (!((DBTSlaveBuffered*)pDBTSlave)->OnOkXMLImport(nCurrentRow))
								// il programmatore ha invalidato l'importazione della singola riga: passo 
								// alla successiva (se l'importazione deve fallire per questo motivo, 
								// sarà cura dello stesso programmatore stabilirlo nella altre OnOkXMLImport
								// - di DBT Slave o di Documento)
								continue; 
							
							//dopo la valorizzazione dei campi
							((DBTSlaveBuffered*)pDBTSlave)->OnAfterXMLImport(nCurrentRow);

							// records events
							AfxGetXMLImpExpController()->SetMessage(_TB("Start invoking events for current row"));
							FireDBTSlaveRecEvents(((DBTSlaveBuffered*)pDBTSlave)->GetRecord(), pSlaveRec, pChildNode);
							AfxGetXMLImpExpController()->SetMessage(_TB("Stop invoking events for current row"));

							//verifico che il programmatore non abbia distrutto il record
							ASSERT(AfxIsValidAddress(pSlaveRec, sizeof(SqlRecord)));

							pSlaveRec->SetStorable();
							
							//rilascio il lock sui campi cha dovranno essere riallineati al master
							for(int i =0; i<keys.GetSize (); i++)
								keys.GetAt (i)->SetValueLocked (FALSE);							
						}
					}
				
				}
				
				if (!pDBTSlave->OnOkXMLImport())
					// se il programmatore, a livello globale per il DBT, decide che 
					// qualcosa non va, abortisco l'importazione
					return FALSE;
				
				//dopo la valorizzazione delle righe
				pDBTSlave->OnAfterXMLImport ();	
			}
		}
		pSlaveNode = pChildNode->GetNextChild();
	}
	return TRUE;
}

//----------------------------------------------------------------------------
/// analizzando i campi modificati dalla OnPreparePrimaryKey capisco
//  quali sono i campi di foreigh key che puntano al master
void CXMLDataManager::GetForeignKeySegments(DataObjArray &arSegs, SqlRecord *pRec, DBTSlave *pDBT, int nRow /*=-1*/)
{
	if (!pRec || !pDBT)
	{
		ASSERT(FALSE);
		return;
	}

	pRec->SetFlags (FALSE, FALSE);

	if (pDBT->IsKindOf (RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		if (nRow == -1)
		{
			ASSERT(FALSE);
			return;
		}

		((DBTSlaveBuffered*)pDBT)->OnPreparePrimaryKey (nRow, pRec);
	}
	else
		pDBT->OnPreparePrimaryKey();

	pRec->GetKeyStream (arSegs, FALSE);
	for (int i=0; i<arSegs.GetSize (); i++)
	{
		if (!arSegs.GetAt (i)->IsModified ())
		{
			arSegs.RemoveAt (i);
			i--;
		}
	}
}

//----------------------------------------------------------------------------
int CXMLDataManager::ImportRecordField
							(
								CXMLNode*		pFieldNode, 
								SqlRecord*		pRecord, 
								CString&		strFieldName
							)
{
	strFieldName.Empty();

	if 
		(
			!pFieldNode || !AfxIsValidAddress(pFieldNode, sizeof(CXMLNode)) ||
			!pRecord || !AfxIsValidAddress(pRecord, sizeof(CXMLNode)) 
		)
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return IMPORT_RECFLD_ERROR_INVALID_CALL;
	}
	
	pFieldNode->GetBaseName(strFieldName);
	
	CString strFieldValue;
	// se il valore è espresso come attributo, prendo quello
	// altrimenti prendo il testo del nodo
	if (!pFieldNode->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strFieldValue))
		pFieldNode->GetText (strFieldValue); 
	if
		(
			!strFieldName.CompareNoCase((LPCTSTR)DOC_XML_MASTER_TAG)||
			!strFieldName.CompareNoCase((LPCTSTR)DOC_XML_SLAVES_TAG) ||
			!strFieldName.CompareNoCase((LPCTSTR)DOC_XML_SLAVE_TAG) ||
 			!strFieldName.CompareNoCase((LPCTSTR)XML_ROW_TAG)
		)
		return IMPORT_RECFLD_ERROR_NO_FIELD_NODE;

	if (!strFieldName.IsEmpty())
	{
		DataObj* pDataObj = pRecord->GetDataObjFromColumnName (strFieldName);	
		//ASSERT(pDataObj);
		if (pDataObj)
		{
			if (pDataObj->GetDataType() == DATA_ENUM_TYPE)
			{
				DataEnum* pClone = (DataEnum*)pDataObj->DataObjClone(); 
				pClone->AssignFromXMLString((LPCTSTR)strFieldValue);	
				if (pClone->GetTagValue() == 0 || pClone->GetTagValue() != ((DataEnum*)pDataObj)->GetTagValue())
				{
					pDataObj->Clear();
					delete pClone;
					return IMPORT_RECFLD_ERROR_INVALID_VALUE;
				}
				else
				{
					pDataObj->Assign(*pClone);
					pDataObj->SetValueLocked();
					delete pClone;
					return IMPORT_RECFLD_SUCCEEDED;
				}
			}			
			else
			{
				pDataObj->AssignFromXMLString((LPCTSTR)strFieldValue);	
				pDataObj->SetValueLocked();
				return IMPORT_RECFLD_SUCCEEDED;
			}
		}
	}

	return IMPORT_RECFLD_ERROR_FIELD_NOTFOUND;
}


//-----------------------------------------------------------------------------
BOOL CXMLDataManager::RemoveSchemaFiles ()
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strPath = pEnvMng->GetPendingEnvFolderPath(TRUE, FALSE) + MakeSchemaName (ENV_XML_FILE_NAME);
	if (ExistFile (strPath))
		DeleteFile (strPath);

	strPath = pEnvMng->GetPendingEnvFolderSchemaPath(TRUE, FALSE);
	if (ExistPath (strPath))
		return RemoveFolderTree (strPath);
	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CXMLDataManager::TestCanRetryImport ()
{
	CXMLEnvelopeManager* pEnvMng = GetEnvelopeManager();
	if (!pEnvMng)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CXMLDocumentObject aCookie(FALSE, FALSE);

	CString strPendingPath = pEnvMng->GetPendingEnvFolderPath (TRUE);
	CString strFile  =	strPendingPath + COOKIE_NAME;
	CXMLNode *pNode = NULL;
	BOOL bCanRetryImport = TRUE;;

	if (::ExistFile (strFile) && aCookie.LoadXMLFile (strFile))
		pNode = aCookie.GetRoot();
	else
	{
		AfxInitWithXEngineEncoding(aCookie);
		pNode = aCookie.CreateRoot (TAG_ATTEMPTS);
	}
	
	if (!pNode)
	{
		DeleteFile (strFile);
		return TRUE;
	}

	CString strAttempts;
	pNode->GetText (strAttempts );

	int nAttempts = _ttoi((const TCHAR*)strAttempts);
	
	if (++nAttempts > MAX_ATTEMPTS)
	{
		DeleteFile (strFile);
		bCanRetryImport = FALSE;
	}
	else
	{
		strAttempts.Format (_T("%d"), nAttempts);

		pNode->SetText (strAttempts );
		aCookie.SaveXMLFile (strFile);
		bCanRetryImport = TRUE;

	}

	if (!bCanRetryImport)
	{
		//crea la cartella se non esiste
		pEnvMng->GetFailureSenderSitePath (TRUE);
		CString strFailurePath = pEnvMng->GetFailureEnvFolderPath (TRUE, FALSE);

		if (!CopyFolderTree(strPendingPath, strFailurePath, TRUE))
		{
			ASSERT(FALSE);
			OutputMessage(cwsprintf(_TB("Unable to copy the folder: {0-%s} to the new path: {1-%s}.\nIt is possibile that complete source files names or destination files names exceed 255 characters. Reduce generated names modifying export profile names and names of the data files."), strPendingPath, strFailurePath));
			return FALSE;
		}
		
		if (!RemoveFolderTree(strPendingPath))
		{
			ASSERT(FALSE);
			OutputMessage(cwsprintf(_TB("Unable to delete the folder: {0-%s} or part of its content."), strPendingPath));
			return FALSE;
		}
	}

	return TRUE;
}

//----------------------------------------------------------------------------
//	Class CXMLDataImportDoc definition
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDataImportDoc, CClientDoc)

BEGIN_MESSAGE_MAP(CXMLDataImportDoc, CClientDoc)
	//{{AFX_MSG_MAP(CXMLDataImportDoc)
	ON_COMMAND	(ID_EXTDOC_IMPORT_XML_DATA, OnDataImport)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_IMPORT_XML_DATA, OnUpdateImportXMLData)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CXMLDataImportDoc::CXMLDataImportDoc()
{
}


//----------------------------------------------------------------------------
CAbstractFormDoc* CXMLDataImportDoc::GetServerDoc()
{
	CBaseDocument*	pServerDoc = GetMasterDocument();
	ASSERT(pServerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));
	return (CAbstractFormDoc*)pServerDoc;
}

//----------------------------------------------------------------------------
BOOL CXMLDataImportDoc::OnAttachData () 
{
	Attach (new CImportEvents());
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataImportDoc::Customize()
{
	SetDocAccel(IDR_TB_XMLTRANSFER);
	
	CreateJsonToolbar(IDD_IMPORT_TOOLBAR);

}

//----------------------------------------------------------------------------
void CXMLDataImportDoc::OnDataImport()
{
	ImportData();
}
//----------------------------------------------------------------------------
void CXMLDataImportDoc::ImportData()
{
	if (!GetServerDoc())
	{
		ASSERT(FALSE);
		return;
	}

	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;
			
	GetServerDoc()->LoadXMLDescription(); //load xml files description

	CXMLDataManagerObj* pXMLDataMng = GetServerDoc()->GetXMLDataManager();
	if (pXMLDataMng)
		pXMLDataMng->Import();

	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;
}
//----------------------------------------------------------------------------
bool CXMLDataImportDoc::CanImportData()
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	if (pServerDoc==NULL)
	{
		return false;
	}

	return 
		AfxIsActivated(TBEXT_APP, INTERACTIVE_IMP_EXP) &&
		pServerDoc->CanDoImportXMLData() &&
		pServerDoc->GetType() != VMT_FINDER && 
		pServerDoc->GetType() != VMT_BATCH && 
		!AfxGetXMLImpExpController()->IsBusy() &&
		pServerDoc->GetXMLDataManager() &&
		pServerDoc->CanLoadXMLDescription() &&
		pServerDoc->GetFormMode() == CAbstractFormDoc::BROWSE &&
		!AfxGetSiteName().IsEmpty() &&
		!AfxGetDomainName().IsEmpty() &&
		OSL_CAN_DO(pServerDoc->GetInfoOSL(), OSL_GRANT_XMLIMPORT);
}

//-----------------------------------------------------------------------------
void CXMLDataImportDoc::OnUpdateImportXMLData(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanImportData());
}
