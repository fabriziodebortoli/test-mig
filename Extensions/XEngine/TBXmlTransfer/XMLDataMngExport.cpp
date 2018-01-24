#include "stdafx.h" 

#include <TBXMLCore\xmldocobj.h>
#include <TBXMLCore\xmlgeneric.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <tbgeneric\dataobj.h>

#include <tbgeneric\formatstable.h>
#include <tbgeneric\globals.h>

#include <TBGENLIB\generic.h>
#include <TBGENLIB\baseapp.h>

#include <TBOleDB\sqltable.h>
#include <TBOleDB\sqlmark.h>

#include <TBGES\dbt.h>
#include <TBGES\browser.h>
#include <TBGES\extdoc.hjson> //JSON AUTOMATIC UPDATE
#include <TBGES\xsltmng.h>
#include <TBGES\eventmng.h>

#include <tbwebserviceswrappers\loginmanagerinterface.h>

#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\TXEParameters.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "XMLTransferTags.h"
#include "GenFunc.h"
#include "ExpCriteriaObj.h"
#include "ExpCriteriaDlg.h"
#include "ExpCriteriaWiz.h"
#include "XMLProfileInfo.h"
#include "XMLEvents.h"
#include "XMLDataMng.h"

// resource declarations
#include "XMLDataMng.hjson"
#include "ExpCriteriaDlg.hjson"
#include "..\ModuleObjects\CDExportWizard\JsonForms\IDD_EXPORT_TOOLBAR.hjson"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szDefaultExportLogFile	[] = _T("Export.log");
static const TCHAR szDescriDocument		[] = _T("Descrizione Documento");

/////////////////////////////////////////////////////////////////////////////
// CXMLDataManager
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------
void CXMLDataManager::InitExportDocSelection
						(
							LPCTSTR				lpszProfileName,		/* = NULL*/
							LPCTSTR				lpszExpCriteriaFilename,/* = NULL*/
							LPCTSTR				lpszExportPath,			/* = NULL*/
							int					nDocSelType,			/* = EXPORT_ONLY_CURR_DOC*/
							int					nProfileSelType,		/* = USE_PREFERRED_PROFILE*/
							BOOL				bSendExportedEnvelope,	/* = FALSE*/
							CAutoExpressionMng*	pAutoExpressionMng		/* = NULL*/
						)
{
	if (m_pExportDocSelection)
		delete m_pExportDocSelection;
	
	m_pExportDocSelection = new CXMLExportDocSelection(m_pDoc);

	ASSERT(m_pExportDocSelection);

	m_pExportDocSelection->m_bSendEnvelopeNow = bSendExportedEnvelope;

	if (nDocSelType == EXPORT_ONLY_CURR_DOC)
	{
		if (m_pDoc && m_pDoc->ValidCurrentRecord())
			m_pExportDocSelection->m_nDocSelType = EXPORT_ONLY_CURR_DOC;
		else
			m_pExportDocSelection->m_nDocSelType = EXPORT_ALL_DOCS;
	}
	else
		m_pExportDocSelection->m_nDocSelType = nDocSelType;
	

	m_pExportDocSelection->m_nProfileSelType = nProfileSelType;

	CString strProfile;
 	switch(nProfileSelType)
	{
		case USE_PREFERRED_PROFILE:
			strProfile =	m_pDoc->GetXMLDocInfo()
							? m_pDoc->GetXMLDocInfo()->GetPreferredProfile()
							: _T("");
			break;

		case USE_SELECTED_PROFILE:
			strProfile = lpszProfileName ? lpszProfileName : _T("");
			break;

		default:
			strProfile = szPredefined;
			break;
	}

	if (!ExistProfile(m_pDoc->GetNamespace(), strProfile))
		strProfile = "";

	m_pExportDocSelection->SetCurrentProfileInfo(strProfile, pAutoExpressionMng);

	if (lpszExpCriteriaFilename	 && lpszExpCriteriaFilename[0])
	{
		m_pExportDocSelection->m_strExpCriteriaFileName = lpszExpCriteriaFilename;

		CXMLProfileInfo* pCurrentProfile = m_pExportDocSelection->GetProfileInfo();
		if (pCurrentProfile)
		{
			// Se il nome file puntato da lpszExpCriteriaFilename è privo di path 
			// (o se in esso è specificata solo una path relativa) prendo quella
			// determinata dal profilo corrente e ci concateno lpszExpCriteriaFilename
			if (m_pDoc && !IsDriveName(lpszExpCriteriaFilename))
				pCurrentProfile->m_strExpCriteriaFileName = GetExpCriteriaVarFile(pCurrentProfile->GetNamespaceDoc(), pCurrentProfile->GetName(), lpszExpCriteriaFilename);
			else
				pCurrentProfile->m_strExpCriteriaFileName = lpszExpCriteriaFilename;
			
			pCurrentProfile->LoadAllFiles(pAutoExpressionMng);
		}
		else
			ASSERT(FALSE);
	}

	if (lpszExportPath && lpszExportPath[0])
	{
		m_strAlternativeExportPath = m_pExportDocSelection->m_strAlternativePath = lpszExportPath ;
		m_pExportDocSelection->m_bUseAlternativePath =  !m_strAlternativeExportPath.IsEmpty();
	}
	else
		m_strAlternativeExportPath.Empty();
}

//----------------------------------------------------------------------------
CWizardFormDoc* CXMLDataManager::CreateExportWizard
									(
										CAutoExpressionMng* pAutoExpressionMng,	 /* = NULL*/
										CAbstractFormDoc*	pDocToPutInWaitState /* = NULL */
									)
{
	
	CBaseDocument *pBaseDoc	= AfxGetTbCmdManager()->RunDocument
														(
															_NS_DOC("Document.Extensions.XEngine.TbXmlTransfer.ExportCriteria"),
															szDefaultViewMode, 
															FALSE,
															NULL,
															this
														); 
	if (!pBaseDoc) return NULL;

	CExpCriteriaWizardDoc* pDoc = (CExpCriteriaWizardDoc*)pBaseDoc;
	pDoc->m_pWaitingDoc = pDocToPutInWaitState;

	SAFE_DELETE (pDoc->m_pAutoExpressionMng);

	if (pAutoExpressionMng)
		ASSERT_VALID(pAutoExpressionMng);

	ASSERT_VALID(pDoc);
	pDoc->m_pAutoExpressionMng = pAutoExpressionMng;

	return pDoc;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::RunExportWizard
						(
							CXMLProfileInfo**	lppCurrentProfile,		/* = NULL*/
							LPCTSTR				lpszProfileName,		/* = NULL*/
							LPCTSTR				lpszExpCriteriaFilename,/* = NULL*/
							LPCTSTR				lpszExportPath,			/* = NULL*/
							int					nDocSelType,			/* = EXPORT_ONLY_CURR_DOC*/
							int					nProfileSelType,		/* = USE_PREFERRED_PROFILE*/
							BOOL				bSendExportedEnvelope,	/* = FALSE*/
							CAbstractFormDoc*	pDocToPutInWaitState	/* = NULL */
						)
{
	if (!pDocToPutInWaitState)
		pDocToPutInWaitState = m_pDoc;
			
	CAutoExpressionMng* pAutoExpressionMng = new CAutoExpressionMng();
	InitExportDocSelection(lpszProfileName, lpszExpCriteriaFilename, lpszExportPath, nDocSelType, nProfileSelType, bSendExportedEnvelope, pAutoExpressionMng);

	m_pWizardDocument = CreateExportWizard(pAutoExpressionMng, pDocToPutInWaitState);
	if (!m_pWizardDocument)
	{
		SAFE_DELETE(pAutoExpressionMng);
		return FALSE;
	}

	WaitWizardDocument();

	if (m_bContinueExpImp && lppCurrentProfile)
		*lppCurrentProfile = m_pExportDocSelection ? m_pExportDocSelection->GetProfileInfo() : NULL;

	return m_bContinueExpImp;
}

//----------------------------------------------------------------------------
int CXMLDataManager::GetExportCmdMsg() const
{
	return ID_EXTDOC_EXPORT_XML_DATA;
}

//----------------------------------------------------------------------------
void CXMLDataManager::SetUnattendedExportParams
							(
								LPCTSTR	lpszProfileName	,
								LPCTSTR	lpszExpCriteriaFilename	,
								LPCTSTR	lpszExportPath	,
								int		nDocSelType		,
								int		nProfileSelType	,		
								BOOL	bSendExportedEnvelope
							)
{
	CAutoExpressionMng* pAutoExpressionMng = new CAutoExpressionMng;
	InitExportDocSelection(lpszProfileName, lpszExpCriteriaFilename, lpszExportPath, nDocSelType, nProfileSelType, bSendExportedEnvelope, pAutoExpressionMng);

	if (!m_pExportDocSelection)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLProfileInfo* lpCurrentProfile = m_pExportDocSelection->GetProfileInfo();// Caricamento del profilo di esportazione corrente
	delete pAutoExpressionMng;

	if (!lpCurrentProfile)
	{
		ASSERT(FALSE);
		return;
	}	
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::Export()
{
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
		return FALSE;

	// il documento deve essere silente
	BOOL bOldUnattendedMode = m_pDoc->SetInUnattendedMode();

	m_bExpDelRecord = FALSE;
	
	m_nProcessStatus = XML_EXPORT_SUCCEEDED;

	if (!m_pDoc->OnOkXMLExport())
	{
		m_nProcessStatus = XML_EXPORT_ABORTED;
		m_nStatus = XML_MNG_IDLE;

		if ( m_pDoc->IsRunningFromExternalController() )
			m_pDoc->SetRunningTaskStatus (CExternalControllerInfo::TASK_FAILED);
							//(void*)(LPCTSTR)cwsprintf(_TB("Failed to export current record."))

		// ripristino l'interattività del documento
		m_pDoc->SetInUnattendedMode(bOldUnattendedMode);
		return FALSE;
	}
	
	CXMLProfileInfo* lpCurrentProfile = NULL;
	// Se l'esportazione è di tipo unattended si dovrebbe chiamare prima il
	// metodo SetUnattendedExportParams e, quindi, non c'è bisogno di impostare 
	// nuovamente i parametri di esportazione...
	if (!IsInUnattendedMode())
	{
		m_bContinueExpImp = FALSE;
		if (!RunExportWizard(&lpCurrentProfile))
		{	
			m_nProcessStatus = XML_EXPORT_ABORTED;
			m_nStatus = XML_MNG_IDLE;

			// ripristino l'interattività del documento
			m_pDoc->SetInUnattendedMode(bOldUnattendedMode);				
			return FALSE;
		}
		m_pWizardDocument = NULL;
	}
	else
		m_bContinueExpImp = TRUE;
	
	if (m_bContinueExpImp) 
	{
		m_nProcessStatus = XML_EXPORTING_DATA;
		m_nStatus = XML_MNG_EXPORTING_DATA;
		
		FinishExportWizard();
	}
	else if (m_pDoc->IsRunningFromExternalController())
		m_pDoc->SetRunningTaskStatus (CExternalControllerInfo::TASK_FAILED);
			//	(void*)(LPCTSTR)cwsprintf(_TB("Failed to export current record."))
	
	// ripristino l'interattività del documento
	m_pDoc->SetInUnattendedMode(bOldUnattendedMode);
	m_nStatus = XML_MNG_IDLE;
	
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL CXMLDataManager::SetEnvelopeManager(CXMLProfileInfo* lpCurrentProfile)
{
	//creo la path dell'envelope
	// se non ce la faccio visualizzo un errore
	CString strEnvClass, strXSLTDescri, strDescription;
	CString strProfile =  _TB("Document description");

	CTBNamespace  nsTransDoc;
	if (lpCurrentProfile)
	{	
		strProfile =  cwsprintf(_TB("Export profile {0-%s}"), lpCurrentProfile->m_strProfileName);
		if (lpCurrentProfile->IsTransformProfile() && ExistFile(lpCurrentProfile->GetXSLTFileName())) 
		{
			GetXSLTInformation(lpCurrentProfile->GetXSLTFileName(), strXSLTDescri, nsTransDoc);
			strDescription = cwsprintf( _TB("Document {0-%s} data files transformed to {1-%s} using the {2-%s} and XSLT file {3-%s}"),
											m_pDoc->GetTitle(),  
											strXSLTDescri,
											strProfile,
											lpCurrentProfile->m_pHeaderInfo->m_strTransformXSLT
											);	
			CXMLDocInfo* pTransDoc = new CXMLDocInfo(nsTransDoc);
			if (pTransDoc)
			{
				pTransDoc->LoadAllFiles();			
				strEnvClass = pTransDoc->GetHeaderInfo()->m_strEnvClass;	
				delete pTransDoc;
			}
		}
		else
			strEnvClass = lpCurrentProfile->GetEnvClassWithExt();
	}
	else
		if (m_pDoc->GetXMLDocInfo())
			strEnvClass = m_pDoc->GetXMLDocInfo()->GetEnvClassWithExt();

	if (strEnvClass.IsEmpty())
		strEnvClass = m_pDoc->GetNamespace().GetObjectName();

	if (!GetEnvelopeManager()->SetTXEnvFolderPath(strEnvClass, 
												  m_pExportDocSelection->m_strAlternativePath.GetString(),
												 (nsTransDoc.IsEmpty() || !nsTransDoc.IsValid()) ? m_pDoc->GetNamespace() : nsTransDoc))
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("Unable to create path for export process.\r\nCheck export parameters."));

		SAFE_DELETE(m_pXMLExpImpMng);
		AfxGetXMLImpExpController()->CloseDialog();
		CWnd *pMasterFrameWnd = m_pDoc->GetMasterFrame();
		if (pMasterFrameWnd) 
		{
			pMasterFrameWnd->EnableWindow(TRUE);
			pMasterFrameWnd->EndWaitCursor();
		}
		return FALSE;
	}
	//inizializza lo spazio di logging
	m_pXMLExpImpMng->InitLogSpace(GetEnvelopeManager()->GetEnvName(), GetEnvelopeManager()->GetTXEnvFolderLoggingPath());

	

	if (strDescription.IsEmpty()) 
		strDescription = cwsprintf( m_pExportDocSelection->IsOnlySchemaToExport()
									? _TB("Document {0-%s} schema files generated with {1-%s}.")
									: _TB("Document {0-%s} data files generated with {1-%s}."), 
									m_pDoc->GetTitle(),  strProfile);
	
	GetEnvelopeManager()->SetDescription(strDescription);							

	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::ShowExportResult(BOOL bShowLog)
{
	if (!IsInUnattendedMode())
	{
		CString strMessage;
		UINT nIcon = MB_ICONINFORMATION;
		switch(m_nProcessStatus)
		{
			case XML_EXPORT_SUCCEEDED:
				strMessage = cwsprintf (_TB("Data export successfully completed.\n"));
				break;
			case XML_EXPORT_SUCCEEDED_WITH_ERRORS:
				strMessage = cwsprintf (_TB("Data export completed with errors.\n"));
				nIcon = MB_ICONEXCLAMATION;
				break;
			case XML_EXPORT_FAILED:
			case XML_EXPORT_ABORTED:
				strMessage = cwsprintf (_TB("Data export failed.\n"));
				nIcon = MB_ICONSTOP;
				break;
			default:
				break;
		}
		if (!bShowLog)
		{
			AfxMessageBox(strMessage);
			return;
		}

		strMessage += cwsprintf(_TB("Do you want to view the messages?"));
		UINT nLogSpaces = m_pXMLExpImpMng->m_LogSession.GetLogSpacesNumber(TRUE);
		if (nLogSpaces >1)
			strMessage += cwsprintf(_TB("(Number of logging files: {0-%d})"), nLogSpaces);
					
		if (AfxMessageBox (strMessage, MB_YESNO | nIcon) == IDYES)
			m_pXMLExpImpMng->ShowLogSpaces();
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::ExportXMLData(CXMLProfileInfo* lpCurrentProfile)
{
	if (!SetEnvelopeManager(lpCurrentProfile))
		return;

	if (!m_pExportDocSelection || m_pExportDocSelection->IsOnlyCurrentDocToExport())
	{
		m_pDoc->m_pDBTMaster->SetNoPreloadStep();
		m_pDoc->BrowseRecord(FALSE);
		ExportCurrRecord(lpCurrentProfile);
	}
	else
	{
		SqlRecord* pCurrentRecord = NULL ;
		// se il record corrente è valido mi conservo il suo valore
		if (m_pDoc->ValidCurrentRecord())
		{
			pCurrentRecord = m_pDoc->m_pCurrentRec->Create();
			*pCurrentRecord = *(m_pDoc->m_pCurrentRec);
		}

		ExportRecordSet(lpCurrentProfile);
		
		// ripropongo i dati originari del documento prima del processo di export
		if (pCurrentRecord)
		{
			if (*(m_pDoc->m_pCurrentRec) != *pCurrentRecord)
			{
				*(m_pDoc->m_pDBTMaster->GetRecord()) = *pCurrentRecord;
				m_pDoc->m_pDBTMaster->SetNoPreloadStep();
				m_pDoc->BrowseRecord(FALSE);
			}
			SAFE_DELETE(pCurrentRecord);
		}
		else
			SAFE_DELETE(m_pDoc->m_pCurrentRec);
	}

	//se previsto trasformo tutti i dom ancora presenti in memoria. Quelli già completi sono stati scaricati dalla memoria
	if (AfxGetParameters()->f_UseAttribute && lpCurrentProfile && lpCurrentProfile->IsTransformProfile())
		OutputMessage(_TB("Use Attribute Syntax has been temporary ignored in order to use correctly XSLT transformation profile."), CXMLLogSpace::XML_WARNING);

	m_pXMLExpImpMng->ApplyAndSaveTransformXSLT();
	
	if (m_pDoc->GetMasterFrame()) m_pDoc->GetMasterFrame()->EnableWindow(TRUE);

	if (
			m_pXMLExpImpMng &&
			(
				m_nProcessStatus == XML_EXPORT_SUCCEEDED ||
				m_nProcessStatus == XML_EXPORT_SUCCEEDED_WITH_ERRORS
			)
		) 
		
	m_pXMLExpImpMng->CreateEnvelope(IsDisplayingMsgBoxesEnabled());
 
	m_pXMLExpImpMng->FlushLogSpace();	
	AfxGetXMLImpExpController()->CloseDialog();

	ShowExportResult();
}	

//----------------------------------------------------------------------------
void CXMLDataManager::ExportXMLSchema(CXMLProfileInfo* lpCurrentProfile)
{
	if (!SetEnvelopeManager(lpCurrentProfile))
		return;

	CStringArray strArray;
	if (ExportXMLSchemas (lpCurrentProfile, m_pDoc, &strArray, FALSE))
		m_nProcessStatus = XML_EXPORT_SUCCEEDED;
	else
		m_nProcessStatus = XML_EXPORT_FAILED;
	
	if (m_pDoc->GetMasterFrame()) m_pDoc->GetMasterFrame()->EnableWindow(TRUE);
	
	if (
			m_pXMLExpImpMng &&
			(
				m_nProcessStatus == XML_EXPORT_SUCCEEDED ||
				m_nProcessStatus == XML_EXPORT_SUCCEEDED_WITH_ERRORS
			)
		) 
		m_pXMLExpImpMng->CreateEnvelope(IsDisplayingMsgBoxesEnabled());
	m_pXMLExpImpMng->FlushLogSpace();
	AfxGetXMLImpExpController()->CloseDialog();

	ShowExportResult();
}

//----------------------------------------------------------------------------
void CXMLDataManager::ExportSmartXMLSchema(CXMLProfileInfo* lpCurrentProfile)
{
	if (!SetEnvelopeManager(lpCurrentProfile))
		return;

	CStringArray strArray;
	if (ExportXMLSchemas (lpCurrentProfile, m_pDoc, &strArray, TRUE))
		m_nProcessStatus = XML_EXPORT_SUCCEEDED;
	else
		m_nProcessStatus = XML_EXPORT_FAILED;

	if (m_pDoc->GetMasterFrame()) m_pDoc->GetMasterFrame()->EnableWindow(TRUE);
	
	m_pXMLExpImpMng->FlushLogSpace();
	AfxGetXMLImpExpController()->CloseDialog();
	ShowExportResult();
	
	// nel caso di esportazione schema smartdocument devo cancellare il logging creato
	m_pXMLExpImpMng->DropEnvelope(); 
}

//Dato che il wizard e' un documento, quando viene invocato lascia che il flusso del codice continui.
//Quindi quando viene istanziato il doc. viene anche interrotta l'esportazione.
//Quando si fa Finish il wizard chiama questo metodo che continua l'esportazione
//----------------------------------------------------------------------------
void CXMLDataManager::FinishExportWizard()
{
	CXMLProfileInfo* lpCurrentProfile = NULL;
	m_bExpDelRecord = FALSE;
	m_bIsRootDoc = TRUE;
	m_bBusy = TRUE;

	if (m_pExportDocSelection)
	{
		CWnd *pMasterFrameWnd = m_pDoc->GetMasterFrame();
		if (pMasterFrameWnd)
		{
			pMasterFrameWnd->EnableWindow(FALSE);
			pMasterFrameWnd->BeginWaitCursor();
		}
		
		lpCurrentProfile = m_pExportDocSelection->GetProfileInfo();

		if (!AfxGetXMLImpExpController()->ShowDialog())
		{
			OutputMessage(_TB("Another import/export process is running. Unable to continue."));
			if (pMasterFrameWnd) 
			{
				pMasterFrameWnd->EnableWindow(TRUE);
				pMasterFrameWnd->EndWaitCursor();
			}
			return;
		}
		AfxGetXMLImpExpController()->SetMessage(_TB("Export procedure starting..."));

		SAFE_DELETE(m_pXMLExpImpMng)
		CreateXMLExpImpManager();

		if (m_pExportDocSelection->IsOnlyDocToExport())
			ExportXMLData(lpCurrentProfile);
		else
		{
			if (m_pExportDocSelection->IsOnlySchemaToExport())
					ExportXMLSchema(lpCurrentProfile);
			else
				if (m_pExportDocSelection->IsSmartSchemaToExport())
					ExportSmartXMLSchema(lpCurrentProfile);
		}		
	
		
		if (pMasterFrameWnd)
		{
			pMasterFrameWnd->EndWaitCursor();
		}
				
		if (m_pDoc->IsRunningFromExternalController())
			m_pDoc->SetRunningTaskStatus ((m_nProcessStatus == XML_EXPORT_SUCCEEDED)
															? CExternalControllerInfo::TASK_SUCCESS
															: CExternalControllerInfo::TASK_FAILED);
										/*				(m_nProcessStatus == XML_EXPORT_SUCCEEDED)
															? (void*)(LPCTSTR)m_pXMLExpImpMng->m_LogSession.GetCurrentLogFile()
															: (void*)(LPCTSTR)cwsprintf(_TB("Failed to export current record."))
														);*/
	}

	m_bBusy = FALSE;
	SAFE_DELETE(m_pXMLExpImpMng)
}

// viene chiamato quando l'esportazione viene annullata
// serve per liberare la memoria
//----------------------------------------------------------------------------
void CXMLDataManager::CancelExportWizard()
{
	if (m_pExportDocSelection)
	{
		delete m_pExportDocSelection;
		m_pExportDocSelection = NULL;
	}
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportRecordSet(const CXMLProfileInfo* lpProfileInfo	/* = NULL*/)
{
	if (!m_pDoc->m_pDBTMaster) 
	{
		OutputMessage(_TB("Internal procedure error."));
		return FALSE;
	}

	int  nRet = EXTRACT_RECORD_NO_DATA;

	if (!m_pExportDocSelection || m_pExportDocSelection->AreAllDocumentsToExport())	
	{
		SqlTable aTable(m_pDoc->m_pDBTMaster->GetRecord(), m_pDoc->GetReadOnlySqlSession());
		aTable.Open();
		m_pDoc->m_pDBTMaster->OnPrepareForXImportExport(&aTable);
		TRY
		{
			aTable.Query();
			if (aTable.IsEmpty())
			{
				OutputMessage(_TB("No record matches the set export criteria"));
				m_nProcessStatus = XML_EXPORT_FAILED;
				aTable.Close();
				return FALSE;
			}

			while (!aTable.IsEOF())
			{
				if (AfxGetXMLImpExpController()->IsAborted())
				{
					AfxGetXMLImpExpController()->CloseDialog();
					OutputMessage(_TB("Procedure interrupted by the user"));
					aTable.Close();
					m_nProcessStatus = XML_EXPORT_ABORTED;
					return FALSE;
				}

				// catturo i messaggi del documento
				BeginListeningToDoc(m_pDoc);
				m_pDoc->m_pDBTMaster->SetNoPreloadStep();
				m_pDoc->BrowseRecord(FALSE, TRUE);
			
				if (!ExportCurrRecord(lpProfileInfo))
					m_nProcessStatus = XML_EXPORT_FAILED;
				
				// prendo i messaggi di errore o warning del documento
				EndListeningToDoc(m_pDoc);

				aTable.MoveNext();
			}
		}
		CATCH(SqlException, e)
		{
			OutputMessage(e->m_strError);
			m_nProcessStatus = XML_EXPORT_FAILED;
			aTable.Close();
			return FALSE;
		}
		END_CATCH
		aTable.Close();
		return TRUE;
	}
	
	
	BOOL bExtDelRec = FALSE;
	CXMLExportCriteria*	pExportCriteria = lpProfileInfo ? lpProfileInfo->GetCurrentXMLExportCriteria() : NULL;
	if (pExportCriteria)
	{
		if (pExportCriteria->m_pUserExportCriteria)
			pExportCriteria->m_pUserExportCriteria->m_bOverrideDefaultQuery = FALSE; //used only for MagicDocument

		// inserisco eventualmente prima i record deletati. Se l'utente ha 
		// richiesto ciò
		nRet = pExportCriteria->DeletedQuery();
		if (nRet == EXTRACT_RECORD_ERROR)
		{
			m_nProcessStatus = XML_EXPORT_FAILED;
			//TODOOSL 
			//if (!AfxGetInterfaceOSL()->IsTraceReporterEnabled())
			//	OutputMessage(_TB("Connected user is not enabled to access the tracking information of OSL Activity Monitor"));
			return FALSE;
		}

		m_bExpDelRecord = TRUE;
		while (nRet == EXTRACT_RECORD_SUCCEEDED )
		{
			if (AfxGetXMLImpExpController()->IsAborted())
			{
				AfxGetXMLImpExpController()->CloseDialog();
				OutputMessage(_TB("Procedure interrupted by the user"));
				m_nProcessStatus = XML_EXPORT_ABORTED;
				return FALSE;
			}

			// catturo i messaggi del documento
			BeginListeningToDoc(m_pDoc);
			
			if (!ExportCurrRecord(lpProfileInfo))
				m_nProcessStatus = XML_EXPORT_FAILED;
			else
				bExtDelRec = TRUE;

			// prendo i messaggi di errore o warning del documento
			EndListeningToDoc(m_pDoc);

			nRet = pExportCriteria->GetNextRecord();			
		} 
		
		m_bExpDelRecord = FALSE;

		// estraggo i record che soddisfano i criteri di selezione
		CString strErrMessage;
		nRet = pExportCriteria->ExportQuery(strErrMessage);
		
		if (nRet == EXTRACT_RECORD_ERROR)
		{
			m_nProcessStatus = XML_EXPORT_FAILED;
			OutputMessage(cwsprintf(_TB("An error occured executing export criteria: {0-%s}"), strErrMessage));
			return FALSE;
		}
		
		if (nRet == EXTRACT_RECORD_NO_DATA)
		{
			//se ho esportato dei record cancellati allora non do nessun messaggio
			if (bExtDelRec) return TRUE;
			OutputMessage(_TB("No record matches the set export criteria"));
			m_nProcessStatus = XML_EXPORT_FAILED;
			return FALSE;			
		}

		while (nRet == EXTRACT_RECORD_SUCCEEDED)
		{
			if (AfxGetXMLImpExpController()->IsAborted())
			{
				AfxGetXMLImpExpController()->CloseDialog();
				OutputMessage(_TB("Procedure interrupted by the user"));
				m_nProcessStatus = XML_EXPORT_ABORTED;
				return FALSE;
			}
			
			// catturo i messaggi del documento
			BeginListeningToDoc(m_pDoc);
			m_pDoc->m_pDBTMaster->SetNoPreloadStep();
			m_pDoc->BrowseRecord(FALSE, TRUE);
			
			if (!ExportCurrRecord(lpProfileInfo))
				m_nProcessStatus = XML_EXPORT_FAILED;
			
			// prendo i messaggi di errore o warning del documento
			EndListeningToDoc(m_pDoc);

			nRet = pExportCriteria->GetNextRecord();			
		} 
	}	
	
	return TRUE;
}


//----------------------------------------------------------------------------
CXMLExpFileElem* CXMLDataManager::GetXMLExpFileName(CString& strXMLExpFile, const CXMLProfileInfo* lpProfileInfo, BOOL& bNewXMLFile)
{
	// genero il nome vero del file. Questo serve se sto effettuatndo + esportazioni
	// e devo generare diversi file. Ad ognuno viene dato un numero progressivo
	// Viene anche controllato che l'urldata assegnata non vada in conflitto con
	// urldata di altri documenti. In tal caso al nome del file viene postfisso il
	// nome del documento
	CXMLExpFileElem* pExpElem = m_pXMLExpImpMng->GetXMLExpFileName(m_pDoc->GetNamespace().ToString(), strXMLExpFile, AfxGetParameters()->f_EnvPaddingNum, bNewXMLFile);

	if (bNewXMLFile)
	{
		// max num documenti in un file
		int nMaxRec = (lpProfileInfo) ? lpProfileInfo->GetMaxDocument() : AfxGetParameters()->f_MaxDoc;
	
		// max kbyte di un file
		int nMaxKB = (lpProfileInfo) ? lpProfileInfo->GetMaxDimension() : AfxGetParameters()->f_MaxKByte;
		pExpElem->SetMaxDim(nMaxRec, nMaxKB);
	}
	return pExpElem;
}

// il tag next_file viene inserito in fase di splitting del file solo se
// si tratta del root document oppure di un external-reference con relazione 1:n
// e nel file successivo devono essere salvati ancora dei record relativi
// al bookmark corrente
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportCurrRecord
						(
							const CXMLProfileInfo*		lpProfileInfo	/* = NULL*/,
							CXMLXRefInfo*				pXRefInfo		/* = NULL*/,
							BOOL						bNextFile		/* = FALSE*/ //se devo inserire il tag next_file
						)
{
	if (!m_pDoc)
	{
		OutputMessage(_TB("Internal procedure error."));
		ASSERT(FALSE);
		return FALSE;
	}

	// rilascio la coda dei messaggi per rendere responsiva l'applicazione
	if (!AfxIsInUnattendedMode())
		m_pDoc->m_BatchScheduler.CheckMessage();

	DBTMaster* pDBTMaster = m_pDoc->m_pDBTMaster;
	ASSERT(pDBTMaster);

	if (!m_bExpDelRecord && HasToCreateFileData() && !m_pDoc->ValidCurrentRecord())
	{
		OutputMessage(_TB("Export failed.\r\nNo document\twas selected."));
		return FALSE;
	}

	pDBTMaster->SetNoPreloadStep();

	CString strDocKey;
	pDBTMaster->GetRecord()->GetKeyInXMLFormat(strDocKey, TRUE);

	//INIZIO ESPORTAZIONE DEL DOCUMENTO
	m_pDoc->OnBeforeXMLExport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnBeforeXMLExport();

	CString strMessage = cwsprintf
							(
								m_bIsExtRef 
									? _TB("Start exporting the referenced document:\r\n{0-%s}\r\nwith key:\r\n{1-%s}")
									: _TB("Starting document export:\r\n{0-%s}\r\nwith key:\r\n{1-%s}"), 
								m_pDoc->GetTitle(),
								strDocKey
							);

	OutputMessage(strMessage, CXMLLogSpace::XML_INFO);

	RaiseLoggingLevel ();

	m_nDataInstancesNumb = 0;

	CString strProfileName = (lpProfileInfo) ? lpProfileInfo->GetName() : _T("");
	
	CString strUrlData;
	
	if (pXRefInfo)
		strUrlData = pXRefInfo->GetUrlDati();
	else
		if (lpProfileInfo)
			strUrlData = lpProfileInfo->GetUrlData();
		
	if (strUrlData.IsEmpty())
		strUrlData = GetDocTitle();

	if (strUrlData.IsEmpty())
	{
		OutputMessage(_TB("Export failed.\r\nThe name of the data export file is not defined."));
		LowerLoggingLevel ();
		return FALSE;
	}
	
	BOOL bNewXMLFile;
	CString strXMLExpFile = GetName(strUrlData) + strProfileName;	
	CXMLExpFileElem* pExpElem = GetXMLExpFileName(strXMLExpFile, lpProfileInfo, bNewXMLFile);

	// chiedo all'exportimport manager il documento dom da scrivere
	// se non esiste lo istanzia
	CXMLDocumentObject* pXMLDomDoc = m_pXMLExpImpMng->GetXMLExportDomDocument(strUrlData, strXMLExpFile, lpProfileInfo, bNewXMLFile, IsDisplayingMsgBoxesEnabled(), (bNextFile || !m_bIsExtRef));

	if (!pXMLDomDoc)
	{
		OutputMessage(cwsprintf(_TB("Error creating the XML file {0-%s}"), ::GetNameWithExtension(strXMLExpFile)));
		LowerLoggingLevel ();
		return FALSE;
	}

	if (pXRefInfo)
		pXRefInfo->m_strExportedInFile = GetName(strXMLExpFile); // serve per gli external-reference

	CXMLNode* pDocumentsNode = pXMLDomDoc->GetRootChildByName(DOC_XML_DOCUMENTS_TAG);

	if (!pDocumentsNode)
	{
		OutputMessage(cwsprintf(_TB("Error creating the XML file {0-%s}"), ::GetNameWithExtension(strXMLExpFile)));
		LowerLoggingLevel ();
		return FALSE;
	}
	
	
	CXMLNode* pDBTMasterNode = ExportDBT(pDBTMaster, pDocumentsNode, lpProfileInfo, 1, pXRefInfo);
	
	BOOL bOk = TRUE;
	//se è un record deletato non devo andare avanti
	if (pDBTMasterNode && !m_bExpDelRecord)
	{
		DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
		if (pDBTSlaves && pDBTSlaves->GetSize())
		{
			CXMLNode* pSlavesNode = pDBTMasterNode->GetChildByName((LPCTSTR)DOC_XML_SLAVES_TAG);
			if (!pSlavesNode)
				pSlavesNode = pDBTMasterNode->CreateNewChild((LPCTSTR)DOC_XML_SLAVES_TAG);
			for (int i = 0; i < pDBTSlaves->GetSize(); i++)
			{
				DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
				ASSERT(pDBTSlave);
				CXMLDBTInfo* pXMLDBTInfo = 	lpProfileInfo
											? lpProfileInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace())
											: pDBTSlave->GetXMLDBTInfo();

				if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport())
					continue;

				// Se sono in stato di BROWSE e per il DBT è stata impostata la 
				// lettura ritardata devo forzare il caricamento dei dati
				pDBTSlave->Reload();
				
				if (ExportDBT(pDBTSlave, pSlavesNode, lpProfileInfo) == NULL)
				{	
					OutputMessage(cwsprintf(_TB("The DBT {0-%s} was not exported"), pDBTSlave->GetNamespace().ToString()), CXMLLogSpace::XML_INFO);
					if  (m_nProcessStatus == XML_EXPORT_FAILED)
					{
						bOk = FALSE;
						break;
					}
				}
			}
		}
	}
	else
		if (!pDBTMasterNode)
		{
			bOk = FALSE;
			OutputMessage(cwsprintf(_TB("The DBT {0-%s} was not exported"), pDBTMaster->GetNamespace().ToString()), CXMLLogSpace::XML_INFO);
		}

	// Salvataggio del file XML
	bOk =  bOk && !IsDocumentEmpty (pXMLDomDoc) && 
		   (HasToCreateFileData() ? pXMLDomDoc->SaveXMLFile((LPCTSTR)strXMLExpFile, TRUE) : TRUE);
	
	if (bOk)
	{
		if (bNewXMLFile)
		{
			CXMLEnvFile::ContentFileType eFileType;
			if (m_bIsExtRef)
				eFileType = CXMLEnvFile::XREF_FILE;				
			else
				eFileType = (m_pXMLExpImpMng->IsRootFilePresent()) ? CXMLEnvFile::NEXT_ROOT_FILE :	CXMLEnvFile::ROOT_FILE;
			

			
			CXMLDocInfo* pTransDoc = NULL;
			CString strEnvClass;
			CString strDocTitle = GetDocTitle();
			CTBNamespace nsTransDoc;
			CString strXSLTDescri, strNsTransDoc;
					
			if (lpProfileInfo && lpProfileInfo->IsTransformProfile())				
			{
				GetXSLTInformation(lpProfileInfo->GetXSLTFileName(), strXSLTDescri, nsTransDoc);
				if (nsTransDoc.IsValid())
				{
					pTransDoc = new CXMLDocInfo(nsTransDoc);
					pTransDoc->LoadAllFiles();			
					strEnvClass = pTransDoc->GetHeaderInfo()->m_strEnvClass;	
					strDocTitle = pTransDoc->GetDocumentTitle();
				}
			}
			else
			{
				if (lpProfileInfo)
					strEnvClass = lpProfileInfo->GetEnvClassWithExt();
				else
					if (m_pDoc->GetXMLDocInfo())
						strEnvClass = m_pDoc->GetXMLDocInfo()->GetEnvClassWithExt();
			}
			if (strEnvClass.IsEmpty())
				strEnvClass = m_pDoc->GetNamespace().ToString();
	
			if (eFileType == CXMLEnvFile::ROOT_FILE)
			{
				if (lpProfileInfo && lpProfileInfo->IsTransformProfile() && nsTransDoc.IsValid())
				{
					GetEnvelopeManager()->SetRootDocNameSpace(nsTransDoc);				
					GetEnvelopeManager()->m_aXMLEnvInfo.SetEnvClass((strEnvClass.IsEmpty()) ? nsTransDoc.GetObjectName() : strEnvClass); 
				}
				else
					GetEnvelopeManager()->SetRootDocNameSpace(m_pDoc->GetNamespace());				
				
			}
			if (pTransDoc) 
				delete pTransDoc;


			if (HasToCreateFileData ())
				m_pXMLExpImpMng->AddEnvFile
				(
					eFileType, 
					(LPCTSTR)GetNameWithExtension(strXMLExpFile),
					(LPCTSTR)strProfileName, 
					(LPCTSTR)strEnvClass,				
					strDocTitle
				);
		}
		m_pXMLExpImpMng->IncrementExpRecordCount(pExpElem, strXMLExpFile, m_nDataInstancesNumb);
		// grazie alle prime due esportazioni verifico di quanto si incrementa il file
		// in termini di KByte per ogni record esportato
		if (m_nDataInstancesNumb <= 2)
		{
			if (ExistFile (strXMLExpFile))
			{
				CLineFile aFile;
				if (aFile.Open (strXMLExpFile, CFile::typeText | CFile::modeRead | CFile::normal))
				{
					DWORD dwFileSize = aFile.GetLength();
					dwFileSize = (dwFileSize < 1024)
								? 1
								: dwFileSize/1024;

					pExpElem->SetIncrementalKByte((int)dwFileSize, m_nDataInstancesNumb == 2);
					aFile.Close();
				}		
			}
		}
	}
	
	//FINE ESPORTAZIONE DEL DOCUMENTO
	m_pDoc->OnAfterXMLExport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnAfterXMLExport();

	LowerLoggingLevel ();
	if (bOk)
	{
		strMessage = cwsprintf
			(	
				m_bIsExtRef
					? _TB("Export of the referenced document successfully completed:\r\n{0-%s}\r\nwith key:\r\n{1-%s}") 
					: _TB("Export of the document:{0-%s} successfully completed\r\nwith key:{1-%s}") , 
				m_pDoc->GetTitle(), 
				strDocKey
			);
	}
	else
	{
		strMessage = cwsprintf
			(	
				m_bIsExtRef
					? _TB("Export of the referenced document:\r\n{0-%s}\r\ncompleted with errors\r\nwith key:\r\n{1-%s}") 
					: _TB("Export of the document:\r\n{0-%s}\r\ncompleted with errors\r\nwith key:\r\n{1-%s}") , 
				m_pDoc->GetTitle(), 
				strDocKey
			);
	}
				
	OutputMessage (strMessage, (bOk ? CXMLLogSpace::XML_INFO : CXMLLogSpace::XML_ERROR));
	if (!m_bIsExtRef)
		AfxGetXMLImpExpController()->SetMessage(strMessage);

	if ( bOk)
	{
		if (m_nProcessStatus == XML_EXPORTING_DATA) 
			m_nProcessStatus = XML_EXPORT_SUCCEEDED;
		
		//se in precedenza c'è stato un errore non metto come stato FAILED XML_EXPORT_SUCCEEDED_WITH_ERRORS
		// poichè qualcosa è stato esportato
		if (m_nProcessStatus == XML_EXPORT_FAILED) 
			m_nProcessStatus = XML_EXPORT_SUCCEEDED_WITH_ERRORS;
	}
	return bOk;
}

//----------------------------------------------------------------------------
CString CXMLDataManager::FormatData(DataObj* pDataObj) const
{
	if (!pDataObj)
		return _T("");

	if (pDataObj->GetDataType() == DATA_ENUM_TYPE)
		return pDataObj->FormatDataForXML(AfxGetParameters()->f_UseEnumAsNum);
	
	return pDataObj->FormatDataForXML(pDataObj->GetDataType()!= DATA_DATE_TYPE);
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDataManager::ExportDBT
							(
								DBTObject*					pDBT, 
								CXMLNode*					pParentNode,
								const CXMLProfileInfo*		lpProfileInfo	/* = NULL*/,
								int							nDBTSize		/* = 1*/,
								CXMLXRefInfo*				pXRefInfo		/* = NULL*/
							)
{
	if (!pDBT || !pParentNode)
	{
		ASSERT (FALSE);
		return NULL;
	}

	CXMLDBTInfo* pXMLDBTInfo = lpProfileInfo 
								? lpProfileInfo->GetDBTFromNamespace(pDBT->GetNamespace()) 
								: pDBT->GetXMLDBTInfo();

	CString strProfileName = lpProfileInfo ? lpProfileInfo->GetName() : szDescriDocument;

	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport())
		return NULL;

	SqlTable*	pDBTTable = pDBT->GetTable();
	ASSERT(pDBTTable);
	
	if (!pDBTTable)
		return NULL;

	CXMLNode* pDBTNode = NULL;
	CUIntArray SlaveBuffIndexes;

	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
	{
		if (nDBTSize <= 0 || (!m_bExpDelRecord && !((DBTMaster*) pDBT)->OnOkXMLExport()))
			return NULL;

		pDBTNode = pParentNode->GetChildByName((LPCTSTR)DOC_XML_MASTERS_TAG);
		if (!pDBTNode)
			pDBTNode = pParentNode->CreateNewChild((LPCTSTR)DOC_XML_MASTERS_TAG);
	}
	else if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		SlaveBuffIndexes.RemoveAll();
		for (int nDBTIdx = 0; nDBTIdx < ((DBTSlaveBuffered*) pDBT)->GetSize(); nDBTIdx++)
		{
			// se fallisce la OnOkXMLExport di riga, prosefuo
			if (((DBTSlaveBuffered*) pDBT)->OnOkXMLExport(nDBTIdx))
				SlaveBuffIndexes.Add(nDBTIdx);
		}

		// se fallisce la OnOkXMLExport di DBT, fallisce l'esportazione
		if (!((DBTSlave*) pDBT)->OnOkXMLExport())
			return NULL;

		nDBTSize = SlaveBuffIndexes.GetSize();
		pDBTNode = pParentNode->CreateNewChild((LPCTSTR)DOC_XML_SLAVEBUFF_TAG);		
	}
	else // Slave 1 : 1
	{
		if (!pDBT->GetRecord() || !((DBTSlave*) pDBT)->OnOkXMLExport())
			return NULL;
		nDBTSize = (pDBT->GetRecord()->IsEmpty()) ? 0 : 1;

		pDBTNode = pParentNode->CreateNewChild((LPCTSTR)DOC_XML_SLAVE_TAG);
	}
	
	if (!pDBTNode)
	{
		ASSERT (FALSE);
		return NULL;
	}
	
	CXMLNode* pRecordNode = NULL;
	int nExistingInstances = 0;

	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
	{
		CString strDataInstances;
		nExistingInstances = 
				(
					pDBTNode->GetAttribute(DOC_XML_DATAINSTANCES_ATTRIBUTE,strDataInstances) 
					&& !strDataInstances.IsEmpty()
				) ? _ttoi((LPCTSTR)strDataInstances)
				: 0;
	}
	else if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		CString strDBTSize;
		nExistingInstances = 
				(
					pDBTNode->GetAttribute(DOC_XML_ROWSNUMBER_ATTRIBUTE, strDBTSize) 
					&& !strDBTSize.IsEmpty()
				) ? _ttoi((LPCTSTR)strDBTSize)
				: 0;
		pDBTNode->SetAttribute(DOC_XML_ROWSNUMBER_ATTRIBUTE, (LPCTSTR)cwsprintf(_T("%d"), nExistingInstances));
	}
	
	CString strDBTNameSpace = pDBT->GetNamespace().ToString();
	
	if (!strDBTNameSpace.IsEmpty())
		pDBTNode->SetAttribute(DOC_XML_NAMESPACE_ATTRIBUTE, (LPCTSTR)strDBTNameSpace);

	pDBTNode->SetAttribute(DOC_XML_TABLE_ATTRIBUTE, (LPCTSTR)pDBTTable->GetTableName());
	if (pXMLDBTInfo->GetChooseUpdate())
		pDBTNode->SetAttribute(DOC_XML_UPDATE_ATTRIBUTE, (LPCTSTR)pXMLDBTInfo->GetStrUpdateType());

	for (int nDBTIdx = 0; nDBTIdx < nDBTSize; nDBTIdx++)
	{
		SqlRecord* pDBTRecord = NULL;
		
		if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
		{
			pDBTRecord = pDBT->GetRecord();
			ASSERT(pDBTRecord);
			((DBTMaster*) pDBT)->OnBeforeXMLExport();
			
			pRecordNode = pDBTNode->CreateNewChild((LPCTSTR)DOC_XML_MASTER_TAG);
			if (m_bExpDelRecord)
				pRecordNode->SetAttribute(DOC_XML_DELETED_ATTRIBUTE, (LPCTSTR)FormatBoolForXML(TRUE));

			if (pRecordNode)
			{
				// lo inserisco come record esportato
				CString strProfileName = (lpProfileInfo)
										 ? lpProfileInfo->GetName()
										 : _T("");
				m_pXMLExpImpMng->InsertExportedRecord(pDBTRecord, strProfileName);
				if (pXRefInfo) //se sto esportando il record xchè external-reference. Metto il bookmark
				{
					long nBookmark = 0;
					if (pXRefInfo->IsNotDocQueryToUse() && pXRefInfo->m_lBookmark > 0)
						nBookmark = pXRefInfo->m_lBookmark;
					else
					{
						// inserisco il bookmark e il nome del file al record corrente
						CString strBookmark;
						nBookmark = 
								(
									pDBTNode->GetAttribute(DOC_XML_BOOKMARK_ATTRIBUTE,strBookmark) 
									&& !strBookmark.IsEmpty()
								) 
								? _ttoi((LPCTSTR)strBookmark)
								: m_pXMLExpImpMng->GetNextBookmark();
						pXRefInfo->m_lBookmark = nBookmark;
					}
					pRecordNode->SetAttribute(DOC_XML_BOOKMARK_ATTRIBUTE, (LPCTSTR)cwsprintf(_T("%d"), nBookmark));		
					m_pXMLExpImpMng->SetFileReferences(nBookmark, pXRefInfo->m_strExportedInFile);
				}
			}
		}
		else if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			((DBTSlaveBuffered*) pDBT)->SetCurrentRow(nDBTIdx);
			pDBTRecord = ((DBTSlaveBuffered*) pDBT)->GetRow(SlaveBuffIndexes[nDBTIdx]);
			ASSERT(pDBTRecord);
			
			if (!OnOkToExportRow (pDBTRecord))
				continue;

			((DBTSlaveBuffered*) pDBT)->OnBeforeXMLExport(SlaveBuffIndexes[nDBTIdx]);
			
			pRecordNode = pDBTNode->CreateNewChild((LPCTSTR)XML_ROW_TAG);
			if (pRecordNode)
			{
				pDBTNode->SetAttribute(DOC_XML_ROWSNUMBER_ATTRIBUTE, (LPCTSTR)cwsprintf(_T("%d"), ++nExistingInstances));
				if (pRecordNode)
					pRecordNode->SetAttribute(DOC_XML_NUMBER_ATTRIBUTE, (LPCTSTR)cwsprintf(_T("%d"), nExistingInstances - 1));
			}
		}
		else
		{
			pDBTRecord = pDBT->GetRecord();
			ASSERT(pDBTRecord);
			((DBTSlave*) pDBT)->OnBeforeXMLExport();
			pRecordNode = pDBTNode;
		}
	
		// se è il dbtmaster inserisco prima il suo UniversalKey group (se definito).
		// se tutte le universakey sono vuote non esporto il documento
		// poi tutti i campi
		// poi gli external reference  
		if (pRecordNode && pDBTRecord->GetSize() > 0)
		{
			// inserisco l'eventuale UniversalKey del DBTMaster
			if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
			{
				if (!InsertMasterUniversalKey((DBTMaster*)pDBT, pRecordNode, lpProfileInfo))
				{
					pDBTNode->RemoveChild(pRecordNode);
					m_nProcessStatus = XML_EXPORT_FAILED;
					return NULL;
				}
				else
				{
					//incremento il numero di istanze esportate
					pDBTNode->SetAttribute(DOC_XML_DATAINSTANCES_ATTRIBUTE, (LPCTSTR)cwsprintf(_T("%d"), ++nExistingInstances));
					m_nDataInstancesNumb = nExistingInstances;
				}
			}

			CXMLNode* pFieldsNode = NULL;
			pFieldsNode = pRecordNode->CreateNewChild(DOC_XML_FIELDS_TAG);	
			// inserisco tutti i campi 
			// Se sono nel caso di delete solo i campi di chiave primaria
			int nColIdx = 0;
			for (nColIdx = 0; nColIdx <= pDBTRecord->GetUpperBound(); nColIdx++)
			{
				CString strColumnName = pDBTRecord->GetColumnName(nColIdx);
				if (
						pDBTRecord->IsVirtual(nColIdx) || 
						(!m_bExpDelRecord && !pXMLDBTInfo->IsFieldToExport(strColumnName))
					)
					continue;
				
				DataObj* pDataObj = pDBTRecord->GetDataObjAt(nColIdx);
				if (
						pDataObj && 
						(!m_bExpDelRecord || 
						 (m_bExpDelRecord && pDBTRecord->IsSpecial(nColIdx) && IsNotEmptyDataObj(pDataObj)))
					)
				{
					CXMLNode* pFieldNode = pFieldsNode->CreateNewChild((LPCTSTR)strColumnName);
				
					if (AfxGetParameters()->f_UseAttribute && (!lpProfileInfo || !lpProfileInfo->IsTransformProfile()))
						pFieldNode->SetAttribute(DOC_XML_VALUE_ATTRIBUTE, FormatData(pDataObj));
					else
						pFieldNode->SetText(FormatData(pDataObj));
				}
			}

			// se sto scrivendo i record da deletare non proseguo con le informazioni
			// legati agli extref
			if (m_bExpDelRecord) 
				break;

			//array che contiene il nome degli external reference già
			//processati per il dbt corrente
			CStringArray aProcessedExtRefs;
			CXMLNode* pExtRefsNode = pRecordNode->CreateNewChild((LPCTSTR)DOC_XML_EXTREFS_TAG);
			//inserirsco gli eventuali external-reference ed effettuo l'esportazione dei documenti correlati
			for (nColIdx = 0; nColIdx <= pDBTRecord->GetUpperBound(); nColIdx++)
			{
				CXMLXRefInfo* pXRefInfo = NULL;
				CXMLXRefInfoArray aXMLXRefInfoArray;
				aXMLXRefInfoArray.SetOwns(FALSE);

				CString strColumnName = pDBTRecord->GetColumnName(nColIdx);
				if (pDBTRecord->IsVirtual(nColIdx))
					continue;
				
				DataObj* pDataObj = pDBTRecord->GetDataObjAt(nColIdx);
				if (!(pDataObj && pXMLDBTInfo->IsFieldToExport(strColumnName) && IsNotEmptyDataObj(pDataObj) )) continue;
				
				//array di tutti gli xref che hanno il segmento
				if (pXMLDBTInfo->m_pXRefsArray)
					pXMLDBTInfo->m_pXRefsArray->GetXRefArrayByFK((LPCTSTR)strColumnName, &aXMLXRefInfoArray);
				
				// non ho external-references
				if (aXMLXRefInfoArray.GetSize() <= 0)
					continue;

				// mi salvo lo stato corrente prima di controllare il documento di ext-ref
				CXMLDocElement* pOldXMLDocElem = m_pXMLExpImpMng->GetCurrentDocElem();
				//ciclo sugli eventuali external reference definiti per il campo
				for (int nRef = 0; nRef < aXMLXRefInfoArray.GetSize(); nRef++)
				{
					pXRefInfo = aXMLXRefInfoArray.GetAt(nRef);
					if (!pXRefInfo)
						continue;

					BOOL bContinue = FALSE;
					for (int i = 0; i <= aProcessedExtRefs.GetUpperBound(); i++)
					{	
						if (pXRefInfo->GetName() == aProcessedExtRefs.GetAt(i))
						{
							bContinue = TRUE;
							continue;
						}
					}
					if (bContinue) continue;

					aProcessedExtRefs.Add(pXRefInfo->GetName());
					if (pXRefInfo->IsToUse() && !ProcessExtRef(pXRefInfo, (LPCTSTR)strColumnName, pDBTRecord, pExtRefsNode))
					{
						if (m_nProcessStatus == XML_EXPORT_FAILED)
						{
							m_pXMLExpImpMng->SetCurrentDocElem(pOldXMLDocElem);	
							return NULL;
						}
						else
							if (m_bErrorFound)
							{
								m_nProcessStatus = XML_EXPORT_SUCCEEDED_WITH_ERRORS;
								OutputMessage(cwsprintf(_TB("Error exporting the external reference for the field {0-%s} = {1-%s} of the table '{2-%s}'."), strColumnName, (LPCTSTR)pDataObj->FormatData(), (LPCTSTR)pDBTTable->GetTableName()));
							}
					}
					
					m_pXMLExpImpMng->SetCurrentDocElem(pOldXMLDocElem);	
					m_bErrorFound = FALSE;
				}
			}
			if (pExtRefsNode && pExtRefsNode->GetChildsNum() == 0)
				pRecordNode->RemoveChild(pExtRefsNode);
		}

		if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
			((DBTMaster*) pDBT)->OnAfterXMLExport();
		else if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			((DBTSlaveBuffered*) pDBT)->OnAfterXMLExport(SlaveBuffIndexes[nDBTIdx]);
		else
			((DBTSlave*) pDBT)->OnAfterXMLExport();
	}

	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		((DBTSlaveBuffered*) pDBT)->SetCurrentRow(-1);

	if (pRecordNode)
		OutputMessage(cwsprintf(_TB("The DBT {0-%s} containing information was exported"), pDBT->GetNamespace().ToString()), CXMLLogSpace::XML_INFO);
	else
	{
		if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) || pDBT->IsKindOf(RUNTIME_CLASS(DBTSlave)))
		{
			OutputMessage(cwsprintf(_TB("The DBT {0-%s} was exported without information"), pDBT->GetNamespace().ToString()), CXMLLogSpace::XML_INFO);
			return pDBTNode;
		}
	}

	return pRecordNode;
}

//------------------------------------------------------------------------------------------
BOOL CXMLDataManager::OnOkToExportRow (SqlRecord* pRecord)
{
	ASSERT (pRecord);

	if (!m_bIsExtRef || pRecord->GetTableName().CompareNoCase(m_RowFilters.m_sXRefTableName) != 0)
		return TRUE;

	CXMLVariable*  pFilter;
	DataObj* pDataObj;
	for (int i=0; i <= m_RowFilters.m_RowFilters.GetUpperBound(); i++)
	{
		pFilter = m_RowFilters.m_RowFilters.GetAt(i);
		if (!pFilter)
			continue;

		pDataObj = pRecord->GetDataObjFromColumnName(pFilter->GetName());
		if (!pDataObj || *pDataObj != *pFilter->GetDataObj()) 
			return FALSE;
	}
	
	return TRUE;
}

//------------------------------------------------------------------------------------------
BOOL CXMLDataManager::InsertMasterUniversalKey
						(
							DBTMaster*				pDBTMaster, 
							CXMLNode*				pRecordNode, 
							const CXMLProfileInfo*	pProfileInfo
						)
{

	if (!pDBTMaster || !pRecordNode)
		return FALSE;

	CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTMaster->GetNamespace()) 
								: pDBTMaster->GetXMLDBTInfo();

	if (!pXMLDBTInfo) return TRUE;

	// inserisco l'eventuale gruppo di universalkey associato al dbtmaster
	CXMLUniversalKeyGroup* pUniversalKeyGroup = pXMLDBTInfo->GetXMLUniversalKeyGroup();
	if (!pUniversalKeyGroup) return TRUE;

	CStringArray strUKArray;
	m_pDoc->GetUniversalKeyList(strUKArray);
	
	if (strUKArray.GetSize() == 0)
	{
		if (!m_pDoc->OnOkXMLExportUniversalKeyEmpty())
		{
			OutputMessage(cwsprintf(_TB("The alternative search key (UniversalKey) of the document {0-%s} is empty.\r\nThe document was not exported."), m_pDoc->GetTitle()));
			return FALSE;
		}
		return TRUE;
	}

	SqlRecord* pRecord = pDBTMaster->GetRecord();
	if (pRecord) 
	{
		CString strAllKey;
		CXMLNode* pUKGroupNode = pRecordNode->CreateNewChild((LPCTSTR)XML_UNIVERSAL_KEYS_TAG);
		pUKGroupNode->SetAttribute(XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE, pUniversalKeyGroup->GetFunctionName());
		
		CString strKeySegm;
		DataObj* pDataObj = NULL;
		CXMLNode* pNode  = NULL;
		CString strValueSegm;
		// inserisco i segmenti di chiave primaria
		int nSegm = 0;
		for (nSegm = 0; nSegm <= pRecord->GetUpperBound(); nSegm++)
		{
			if (!pRecord->IsSpecial(nSegm))
				continue;
			pNode = pUKGroupNode->CreateNewChild((LPCTSTR)DOC_XML_KEY_TAG);
	
			pDataObj = pRecord->GetDataObjAt(nSegm);
			if (pDataObj && IsNotEmptyDataObj(pDataObj))
			{
				strKeySegm = pRecord->GetColumnName(nSegm);
				if (!strKeySegm.IsEmpty())
				{
					pNode->SetAttribute(DOC_XML_PK_ATTRIBUTE,	(LPCTSTR)strKeySegm);
					strValueSegm = FormatData(pDataObj);
					pNode->SetAttribute(DOC_XML_VALUE_ATTRIBUTE, (LPCTSTR)strValueSegm );
					strAllKey += strValueSegm;
				}
			}
		}
		
		CXMLUniversalKey* pUniversalKey;
		int nEmptyKey = 0;
		// faccio il loop sull'Array di stringhe contenenti il nome
		// delle universalkey da utilizzare
		for (int nUk = 0; nUk < strUKArray.GetSize(); nUk++)
		{
			pUniversalKey = pUniversalKeyGroup->GetUKByName(strUKArray.GetAt(nUk));
			if (pUniversalKey)
			{
				pNode = pUKGroupNode->CreateNewChild((LPCTSTR)XML_UNIVERSAL_KEY_TAG);
				// inserisco il nome dell'universal key
				pNode->SetAttribute((LPCTSTR)XML_UNIVERSAL_KEY_NAME_ATTRIBUTE, (LPCTSTR)pUniversalKey->GetName());								
				// inserisco i segmenti di universal key
				int nEmptySegm = 0;
				for (nSegm = 0; nSegm < pUniversalKey->GetSegmentNumber(); nSegm++)
				{
					strKeySegm = pUniversalKey->GetSegmentAt(nSegm);
					if (!strKeySegm.IsEmpty())
					{
						pDataObj = pRecord->GetDataObjFromColumnName(strKeySegm);
						if (pDataObj)
						{
							if (IsNotEmptyDataObj(pDataObj))
								pNode->SetAttribute((LPCTSTR)strKeySegm, FormatData(pDataObj));
							else
								nEmptySegm++;
						}
					}
				}
				if (pUniversalKey->GetSegmentNumber() > 0 && nEmptySegm == pUniversalKey->GetSegmentNumber())
				{
					nEmptyKey++;
					pUKGroupNode->RemoveChild(pNode);
				}
			}
		}
		if (
				strUKArray.GetSize() > 0 && 
				nEmptyKey == strUKArray.GetSize()				
			)
		{
			pRecordNode->RemoveChild(pUKGroupNode);
			if (!m_pDoc->OnOkXMLExportUniversalKeyEmpty())
			{
				OutputMessage(cwsprintf(_TB("The alternative search key (UniversalKey) of the document {0-%s} is empty.\r\nThe document was not exported."), m_pDoc->GetTitle()));
				return FALSE;
			}	
		}
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXMLDataManager::ValorizeExtRefNode(CXMLNode* pExtRefsNode, CXMLXRefInfo* pXRefInfo, SqlRecord* pDBTRecord, BOOL bAddBookMark)
{
	CXMLUniversalKeyGroup* pUniversalKeyGroup = pXRefInfo->GetXMLUniversalKeyGroup();

	CXMLNode* pExtRefNode = pExtRefsNode->CreateNewChild((LPCTSTR)DOC_XML_EXTREF_TAG); 
	// Se il documento correlato è stato esportato
	// inserisco come attributo dell'extref il nome del file in cui è stato esportato
	// cerco se esiste già nel file l'extref
	if (bAddBookMark)
	{
		pExtRefNode->SetAttribute(DOC_XML_NAME_ATTRIBUTE, (LPCTSTR)pXRefInfo->m_strName);

		if (!pXRefInfo->m_strExportedInFile.IsEmpty())
			pExtRefNode->SetAttribute(DOC_XML_EXTREF_FILE_ATTRIBUTE, (LPCTSTR)pXRefInfo->m_strExportedInFile);
		if (pXRefInfo->m_lBookmark > 0)
			pExtRefNode->SetAttribute(DOC_XML_BOOKMARK_ATTRIBUTE,	(LPCTSTR)cwsprintf(_T("%d"),pXRefInfo->m_lBookmark));								
	}

	// Gestione dell'eventuale UniversalKey
	if (pUniversalKeyGroup && !pXRefInfo->IsNotDocQueryToUse())
	{ 
		CAbstractFormDoc* pExtRefDoc = NULL;
		// chiamo la funzione che si occupa di valorizzare i segmenti
		// viene istanziato se non è stato ancora istanziato 
		CXMLDocElement* pXMLDocElem = m_pXMLExpImpMng->GetXMLDocElement(pXRefInfo->GetDocumentNamespace(), m_pDoc);
		if (pXMLDocElem)
			pExtRefDoc = pXMLDocElem->m_pDocument;

		if (!pExtRefDoc)
		{
			m_nProcessStatus = XML_EXPORT_FAILED;
			OutputMessage(cwsprintf(_TB("Error exporting the external reference '{0-%s}'.\r\nFailed to open document with NameSpace '{1-%s}'."), pXRefInfo->GetName(), pXRefInfo->GetDocumentNamespace().ToString()));
			return FALSE;
		}

		CXMLUniversalKeyGroup* pMasterUKGroup = pExtRefDoc->m_pDBTMaster->GetXMLDBTInfo()->GetXMLUniversalKeyGroup();
		if (!pMasterUKGroup)
			return TRUE;

		((CXMLDataManager*) pExtRefDoc->GetXMLDataManager())->m_RowFilters = m_RowFilters;
		//InitDocumentForImpExp(pExtRefDoc);
		CStringArray strUKArray;
		pExtRefDoc->GetUniversalKeyList(strUKArray);
	
		if (strUKArray.GetSize() == 0) 
		{
			if (!pExtRefDoc->OnOkXMLExportUniversalKeyEmpty())
		  	{
				OutputMessage(cwsprintf(_TB("The alternative search key (UniversalKey) of the external reference {0-%s} in the table {1-%s} is empty.{2-%s}\n"), pXRefInfo->GetName(), pDBTRecord->GetTableName(), ""));
				m_bErrorFound = TRUE;
				return FALSE;
			}
			return TRUE;
		}

		CXMLNode* pUKGroupNode = pExtRefNode->CreateNewChild((LPCTSTR)XML_UNIVERSAL_KEYS_TAG);
		pUKGroupNode->SetAttribute(XML_UNIVERSAL_KEY_FUNCNAME_ATTRIBUTE, pMasterUKGroup->GetFunctionName());
		pUKGroupNode->SetAttribute(XML_UNIVERSAL_KEY_TABLENAME_ATTRIBUTE, pXRefInfo->GetReferencedTableName());
		pUKGroupNode->SetAttribute(XML_NAMESPACE_ATTRIBUTE, pXRefInfo->GetDocumentNamespace().ToString());

		CString strKeySegm, strFKSegm;
		CXMLNode* pSegmNode = NULL;
		DataObj* pDataObj = NULL;
		CXMLNode* pNode = NULL;
		// per ogni coppia di segmento della Primary-Foreign Key
		// creo un nodo <Key PK = "PKSegmName" FK = "FKSegmName" Value = \>
		// Ogni segmento è un'attributo con il valore del campo
		for (int i = 0; i < pXRefInfo->GetSegmentsNum(); i++)
		{
			pNode = pUKGroupNode->CreateNewChild((LPCTSTR)DOC_XML_KEY_TAG);	
			strKeySegm = pXRefInfo->GetSegmentAt(i)->GetReferencedSegment();
			strFKSegm = pXRefInfo->GetSegmentAt(i)->GetFKSegment();
			if (!strKeySegm.IsEmpty() && !strFKSegm.IsEmpty())
			{
				pNode->SetAttribute(DOC_XML_FK_ATTRIBUTE, (LPCTSTR)strFKSegm);
				pNode->SetAttribute(DOC_XML_PK_ATTRIBUTE, (LPCTSTR)strKeySegm);
				pDataObj = pDBTRecord->GetDataObjFromColumnName(strFKSegm);
				if (pDataObj && IsNotEmptyDataObj(pDataObj))
					pNode->SetAttribute(DOC_XML_VALUE_ATTRIBUTE, FormatData(pDataObj));								
			}
		}
		
		// i segmenti li inserisco vuoti è la funzione associata all'UniversalKey
		// che si preoccupa di valorizzarli

		CXMLUniversalKey* pUniversalKey;

		int nUk = 0;
		for (nUk = 0; nUk < strUKArray.GetSize(); nUk++)
		{
			pUniversalKey = pMasterUKGroup->GetUKByName(strUKArray.GetAt(nUk));
			if (pUniversalKey)
			{
				pNode = pUKGroupNode->CreateNewChild((LPCTSTR)XML_UNIVERSAL_KEY_TAG);
				// inserisco il nome dell'universal key
				pNode->SetAttribute((LPCTSTR)XML_UNIVERSAL_KEY_NAME_ATTRIBUTE, (LPCTSTR)pUniversalKey->GetName());								
				for (int nSegm = 0; nSegm < pUniversalKey->GetSegmentNumber(); nSegm++)
				{
					strKeySegm = pUniversalKey->GetSegmentAt(nSegm);
					pNode->SetAttribute((LPCTSTR)strKeySegm, _T(""));
				}
			}
		}


		int nResult = CEventManager::FUNCTION_ERROR;
		CString strFuncMsg;

		// chiamo la funzione che si occupa di valorizzare i segmenti
		if (!pMasterUKGroup->GetFunctionName().IsEmpty())
		{
			nResult = pExtRefDoc->FireAction(pMasterUKGroup->GetFunctionName(), (void*)pUKGroupNode);
			switch (nResult)
			{
				case CEventManager::FUNCTION_NOT_FOUND: 	
					strFuncMsg = cwsprintf(_TB("\r\nThe function {0-%s} is not implemented"), pMasterUKGroup->GetFunctionName()); break;
				
				case CEventManager::FUNCTION_ERROR:
					strFuncMsg = cwsprintf(_TB("\r\nError running the fuction {0-%s}"), pMasterUKGroup->GetFunctionName()); break;
				default:
					strFuncMsg.Empty();
			}
		}
		
		// se ho i segmenti di tutte le universal key vuote allora chiedo al documento riferito
		// se è possibile utilizzarlo anche senza UK 
		int nEmptyKey = 0, nTotalKeys = 0;
		if (nResult == CEventManager::FUNCTION_OK ||nResult == CEventManager::FUNCTION_WARNING)
		{			
			for (nUk = 0; nUk < pUKGroupNode->GetChildsNum(); nUk++)
			{
				pNode = pUKGroupNode->GetChildAt(nUk);
				if (pNode && pNode->IsNamed((LPCTSTR)XML_UNIVERSAL_KEY_TAG))
				{
					nTotalKeys ++;
					for (int nAtt = 0; nAtt < pNode->GetAttributesNum(); nAtt++)
					{
						pSegmNode = pNode->GetAttributeAt(nAtt);
						pSegmNode->GetText(strKeySegm);
						if (strKeySegm.IsEmpty())
						{
							pUKGroupNode->RemoveChild (pNode);
							nEmptyKey++;
							nUk--;
							break;
						}
					}
				}
			}
		}

		if (
				nResult == CEventManager::FUNCTION_ERROR || 
				nResult == CEventManager::FUNCTION_NOT_FOUND ||
				(nTotalKeys > 0 && nEmptyKey ==  nTotalKeys)
			)
		{
			pExtRefNode->RemoveChild(pUKGroupNode);
			if (!pExtRefDoc->OnOkXMLExportUniversalKeyEmpty() && pXRefInfo->m_bMustExist)
			{
				OutputMessage(cwsprintf(_TB("The alternative search key (UniversalKey) of the external reference {0-%s} in the table {1-%s} is empty.{2-%s}\n"), pXRefInfo->GetName(), pDBTRecord->GetTableName(), strFuncMsg));
				m_bErrorFound = TRUE;
				return FALSE;
			}
		}
	}
	return TRUE;
}

//------------------------------------------------------------------------------------------
BOOL CXMLDataManager::ProcessExtRef(
										CXMLXRefInfo*	pXRefInfo, 
										LPCTSTR			lpszColumnName, 
										SqlRecord*		pDBTRecord,
										CXMLNode*		pExtRefsNode,
										BOOL			bForSmartDocument
									)

{
	// se è l'esportazione è condizionata verifico la condizione
	CString strMessage; 
	if (pXRefInfo->m_bSubjectTo && !pXRefInfo->EvalExpression(pDBTRecord, strMessage))
	{
		if (!strMessage.IsEmpty())
		{
			OutputMessage(cwsprintf(_TB("Error during evaluation of the conditional external reference {0-%s} present in the table {1-%s}."), pXRefInfo->GetName(), pDBTRecord->GetTableName()));
			m_bErrorFound = TRUE;
			return FALSE;
		}
		return TRUE;						
	}

	CXMLUniversalKeyGroup* pUniversalKeyGroup = pXRefInfo->GetXMLUniversalKeyGroup();
	
	// devo esportare anche il record correlato nella tabella riferita 
	//(se non già esportato)
	// controllo se l'externalReference gestisce l'UniversalKey. 
	// Se si controllo se devo esportare il documento
	BOOL bToExport = (!pUniversalKeyGroup || pUniversalKeyGroup->IsExportData());

	
	CXMLDocElement* pXMLDocElem = m_pXMLExpImpMng->GetCurrentDocElem();
	SqlRecord* pTmpRec = NULL;
	CXMLRecordInfo* pOldRecInfo = NULL;
	if (
			pXMLDocElem					&&
			pXMLDocElem->m_pDocument	&&
			pDBTRecord					&&
			pXMLDocElem->m_pDocument->GetNamespace() == pXRefInfo->GetDocumentNamespace()
		)
	{
		// sto esportando un external-reference ricorsivo
		// devo conservarmi il valore attuale presente nel documento
		pTmpRec = pDBTRecord->Create();
		ASSERT(pTmpRec);
		*pTmpRec = *pDBTRecord;	
		pOldRecInfo = pXMLDocElem->GetCurrentRecord();
	}
	
	if (bToExport && !ExportExtRef(pXRefInfo, lpszColumnName, pDBTRecord, pExtRefsNode, bForSmartDocument))
	{
		if (pTmpRec)
		{
			*pDBTRecord = *pTmpRec;
			delete pTmpRec;
		}
		return FALSE;
	}

	if (pTmpRec)
	{
		*pDBTRecord = *pTmpRec;
		delete pTmpRec;
	}

	if (bForSmartDocument)
		return bToExport;

	return ValorizeExtRefNode (pExtRefsNode, pXRefInfo, pDBTRecord, bToExport);
}


//-----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportExtRef
							(
								CXMLXRefInfo*		pXRefInfo, 
								LPCTSTR				lpszColumnName, 
								SqlRecord*			pDBTRecord,
								CXMLNode*			pExtRefsNode,
								BOOL				bForSmartDocument
							) 
{              
	ASSERT_VALID(pXRefInfo);

	if (!m_pXMLExpImpMng) 
		return FALSE;

	
	CTBNamespace aDocumentNameSpace = pXRefInfo->GetDocumentNamespace();
	if (!aDocumentNameSpace.IsValid())
	{
		OutputMessage(cwsprintf(_TB("Error exporting the external reference for the field '{0-%s}'.\r\nInvalid document NameSpace = '{1-%s}'."), lpszColumnName, aDocumentNameSpace.ToString()));
		return FALSE;
	}

	CAbstractFormDoc* pExtRefDoc = NULL;
	CXMLDocElement* pXMLDocElem = NULL;
	CXMLProfileInfo* pCurrentProfile = NULL;
	//Impr. 6393
	if (bForSmartDocument)
	{
		pCurrentProfile = GetXMLProfileInfo(NULL, pXRefInfo->m_strProfile, aDocumentNameSpace.ToString());
		pXMLDocElem = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc, (pCurrentProfile) ? pCurrentProfile->CanRunOnlyBusinessObject() : FALSE);
		
	}
	else
		pXMLDocElem = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc);

	if (pXMLDocElem)
	{
		m_pXMLExpImpMng->SetCurrentDocElem(pXMLDocElem);
		pExtRefDoc = pXMLDocElem->m_pDocument;
	}
	
	if (!pExtRefDoc)
	{
		OutputMessage(cwsprintf(_TB("Error exporting the external reference for the field '{0-%s}'.\r\nFailed to open document with NameSpace '{1-%s}'."), lpszColumnName, aDocumentNameSpace.ToString()));
		m_bErrorFound = TRUE;
		return FALSE;
	}
	
	return ExportExtRefRecords(pXRefInfo, pDBTRecord, pExtRefsNode, bForSmartDocument, pCurrentProfile);
}

// pSmartExtRefNode = TRUE se sto effettuando l'esportazione SMART. In questo caso le informazioni
// di externalReference vanno ad inserirsi in un nodo elemento del dbt su cui è definito l'extRef e non viene 
// creato un nuovo file xml
//-----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportExtRefRecords
								(
									CXMLXRefInfo*		pXRefInfo, 
									SqlRecord*			pDBTRecord,
									CXMLNode*			pExtRefsNode,  
									BOOL				bForSmartDocument,
									CXMLProfileInfo*	pProfileInfo /*=NULL*/ 
								)
{

	if (!pXRefInfo ||!pDBTRecord || !m_pXMLExpImpMng->GetCurrentDocElem())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	// è il documento correlato attraverso l'external reference
	// è stato impostato prima attraverso la chiamata m_pXMLExpImpMng->SetCurrentDocElem
	CAbstractFormDoc* pExtRefDoc = m_pXMLExpImpMng->GetCurrentDocElem()->m_pDocument;
	if (!pExtRefDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DBTMaster* pExtRefDBTMaster = pExtRefDoc->m_pDBTMaster;
	if (!pExtRefDBTMaster)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bXRefOnMasterDBT = pXRefInfo->GetReferencedDBTNs().IsEmpty () || pExtRefDBTMaster->GetNamespace() == pXRefInfo->GetReferencedDBTNs();
	
	if (pXRefInfo->IsNotDocQueryToUse() && bXRefOnMasterDBT)
		return (bForSmartDocument) ? FALSE : ExportExtRefRecordsNotFromDocQuery(pXRefInfo, pDBTRecord);
	else if (!bXRefOnMasterDBT)
		return (bForSmartDocument) ? FALSE : ExportExtRefToDBTSlave(pXRefInfo, pDBTRecord, pExtRefDoc, pExtRefsNode, bForSmartDocument);

	SqlRecord* pExtRefRecord = pExtRefDBTMaster->GetRecord();
	if (!pExtRefRecord)
	{
		ASSERT(FALSE);
		return FALSE;
	}
		
	for (int nSegIdx = 0; nSegIdx < pXRefInfo->GetSegmentsNum(); nSegIdx++)
	{
		CXMLSegmentInfo* pExtRefSegInfo = pXRefInfo->GetSegmentAt(nSegIdx);
		if (!pExtRefSegInfo)
			continue;

		for (int nColIdx = 0; nColIdx <= pExtRefRecord->GetUpperBound(); nColIdx++)
		{
			SqlRecordItem* pItem = pExtRefRecord->GetAt(nColIdx);
			if (!pItem)
				continue;
			if (!_tcsicmp(pExtRefSegInfo->GetReferencedSegment(), (LPCTSTR)pExtRefRecord->GetColumnName(nColIdx)))
			{
				DataObj* pFKSegment = pDBTRecord->GetDataObjFromColumnName(pExtRefSegInfo->GetFKSegment());
				if (pFKSegment && IsNotEmptyDataObj(pFKSegment))
				{
					DataObj* pCurrKeyValue = pFKSegment->Clone();
					if (!pExtRefSegInfo->GetFKStrFixedValue().IsEmpty())
						pCurrKeyValue->Assign(pExtRefSegInfo->GetFKStrFixedValue());
					DataObj* pExtDataObj = pItem->GetDataObj();
					ASSERT(pExtDataObj);
					if (pExtDataObj)
					{
						if (pCurrKeyValue->GetDataType() == DATA_ENUM_TYPE)
							(((DataEnum*)pExtDataObj)->Assign(((DataEnum*)pCurrKeyValue)->GetValue()));
						else
							pExtDataObj->Assign(pCurrKeyValue->Str());
					}
					SAFE_DELETE(pCurrKeyValue);
				}
				// Se ho trovato un segmento della chiave esterna non valorizzato
				// l'external reference non è esportabile, ma devo dare errore solo
				// se l'external reference non ammette valori nulli 
				else 
				{
					if (!pXRefInfo->CanBeNull())
					{
						OutputMessage(cwsprintf(_TB("The External Reference {0-%s} in the table {1-%s} has null key segments.\r\nUnable to export the related document."), pXRefInfo->GetName(), pDBTRecord->GetTableName()));
						m_bErrorFound = TRUE;
					}
					return FALSE;
				}
			}
		}
	}
	
	// prima controllo che non sia stato già esportato con questo profilo
	if (!bForSmartDocument)
	{
		CXMLRecordInfo* pXMLRecordInfo = m_pXMLExpImpMng->GetExportedRecord(pExtRefRecord, pXRefInfo->m_strProfile);
		if (pXMLRecordInfo)
		{
			m_pXMLExpImpMng->SetCurrentRecord(pXMLRecordInfo);
			pXRefInfo->m_strExportedInFile = pXMLRecordInfo->m_strFileName;
			pXRefInfo->m_lBookmark = pXMLRecordInfo->m_lBookmark;
			return TRUE;
		}
	}

	return LoadExtRefRecordInDocument(pXRefInfo, FALSE, bForSmartDocument ? pExtRefsNode: NULL, bForSmartDocument ? pProfileInfo : NULL);
}

//-----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportExtRefToDBTSlave(
												CXMLXRefInfo*		pXRefInfo, 
												SqlRecord*			pDBTRecord, 
												CAbstractFormDoc*	pExtRefDoc, 
												CXMLNode*			pExtRefsNode, 
												BOOL				bForSmartDocument
											)
{
	RecordArray* pRecords = ExecJoinFromSlaves (pXRefInfo, pDBTRecord, pExtRefDoc, pExtRefsNode);
	
	// some errors
	if (!pRecords)
		return FALSE; 

	// no records, I prefer to skip all external references
	if (!pRecords->GetSize() && pXRefInfo->MustExist())
	{
		OutputMessage(cwsprintf(_TB("The referenced document for the external reference {0-%s} in the table {1-%s} does not exist."), pXRefInfo->GetName(), pXRefInfo->GetTableName()));
		m_bErrorFound = TRUE;
		return FALSE;
	}
	
	DBTMaster* pExtRefDBTMaster = pExtRefDoc->m_pDBTMaster;
	if (!pExtRefDBTMaster)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	SqlRecord* pExtRefRecord = pExtRefDBTMaster->GetRecord();
	if (!pExtRefRecord)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	// no records
	if (!pRecords->GetSize())
		return TRUE;

	BOOL bOk = FALSE;
	for (int i=0; i <= pRecords->GetUpperBound(); i++)
	{
		SqlRecord* pXRefMasterRec = pRecords->GetAt(i);
		if (!pXRefMasterRec)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		*pExtRefRecord = *pXRefMasterRec;

		bOk = LoadExtRefRecordInDocument(pXRefInfo, FALSE, bForSmartDocument ? pExtRefsNode: NULL); 
		if (i < pRecords->GetUpperBound())
			ValorizeExtRefNode (pExtRefsNode, pXRefInfo, pDBTRecord, bOk);
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportExtRefRecordsNotFromDocQuery
								(
									CXMLXRefInfo*		pXRefInfo, 
									SqlRecord*			pDBTRecord
								)
{
	if (!pXRefInfo ||!pDBTRecord || !m_pXMLExpImpMng->GetCurrentDocElem())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// è il documento correlato attraverso l'external reference
	CAbstractFormDoc* pExtRefDoc = m_pXMLExpImpMng->GetCurrentDocElem()->m_pDocument;
	if (!pExtRefDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DBTMaster* pExtRefDBTMaster = pExtRefDoc->m_pDBTMaster;
	if (!pExtRefDBTMaster)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	SqlRecord* pExtRefRecord = pExtRefDBTMaster->GetRecord();
	if (!pExtRefRecord)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// In questo caso i segmenti valorizzati dall'external reference non  
	// corrispondono ai parametri della query predefinita del documento,
	// e quindi occorre prima estrarre i record correlati e poi caricarli
	// uno ad uno nel documento per poterli esportare
	// Ogni dbtmaster avrà lo stesso bookmark
	// Nel file del documento che ha generato questo ext-reference sarà indicato
	// solo il nome del primo file di extref utilizzato, l'unico bookmark e il fatto
	// che la relazione sia 1:n
	// In fase di import viene controllato se i record sono stati splittati anche nel 
	// eventuale seguente file generato. I record appartenenti alla stessa estrazione 
	// hanno lo stesso bookmark
	CString strExtRefFilter;
	strExtRefFilter.Empty();
	for (int nSegIdx = 0; nSegIdx < pXRefInfo->GetSegmentsNum(); nSegIdx++)
	{
		CXMLSegmentInfo* pExtRefSegInfo = pXRefInfo->GetSegmentAt(nSegIdx);
		if (!pExtRefSegInfo || pExtRefSegInfo->GetReferencedSegment().IsEmpty())
			continue;

		DataObj* pFKSegment = pDBTRecord->GetDataObjFromColumnName(pExtRefSegInfo->GetFKSegment());
		if (pFKSegment)
		{
			DataObj* pCurrKeyValue = pFKSegment->Clone();
			if (!pExtRefSegInfo->GetFKStrFixedValue().IsEmpty())
				pCurrKeyValue->Assign(pExtRefSegInfo->GetFKStrFixedValue());
			if (!strExtRefFilter.IsEmpty())
				strExtRefFilter += _T(" AND ");
			strExtRefFilter += cwsprintf
									(
										_T("%s = %s"),
										(LPCTSTR)pExtRefSegInfo->GetReferencedSegment(),
										m_pDoc->GetSqlConnection()->NativeConvert(pCurrKeyValue)
									);		
			SAFE_DELETE(pCurrKeyValue);
		}
	}
	if (strExtRefFilter.IsEmpty())
		return FALSE;

	BOOL bOk = TRUE;

	CRuntimeClass* pExtRefRecordClass = pExtRefDoc->GetSqlConnection()->GetSqlRecordClass(pExtRefDBTMaster->GetTable()->GetTableName());
	if (pExtRefRecordClass)
	{
		SqlRecord* pExtRefRecordToExport = (SqlRecord*)pExtRefRecordClass->CreateObject();
		if (pExtRefRecordToExport)
		{
			SqlTable ExtRefTable(pExtRefRecordToExport, pExtRefDoc->GetReadOnlySqlSession());

			TRY
			{
				ExtRefTable.Open();
				ExtRefTable.SelectAll();
				
				ExtRefTable.SetFilter(strExtRefFilter);

				ExtRefTable.Query();

				if (ExtRefTable.IsEmpty())				
				{
					if (pXRefInfo->m_bMustExist)
					{
						CString strKey;
						pDBTRecord->GetKeyInXMLFormat(strKey, TRUE);
						OutputMessage(cwsprintf(_TB("The referenced document with key {0-%s} for the external reference {1-%s} in the table {2-%s} does not exist."), strKey, pXRefInfo->GetName(), pDBTRecord->GetTableName()));
						m_bErrorFound = TRUE;
					}
					
					ExtRefTable.Close();
					delete pExtRefRecordToExport;
					return FALSE;
				}
			}
			CATCH (SqlException, e)
			{
				ExtRefTable.Close();
				delete pExtRefRecordToExport;
				m_bErrorFound = m_bErrorFound || pXRefInfo->m_bMustExist;
				return FALSE;
			}
			END_CATCH

			
			CXMLRecordInfo* pXMLRecordInfo = NULL;
			CXMLRecordInfo* pXMLFirstRecordInfo = NULL;
			CXMLRecordInfo* pXMLNewRecordInfo = NULL;
			
			while (!ExtRefTable.IsEOF())
			{
				*pExtRefRecord = *pExtRefRecordToExport;
				// prima controllo che non sia stato già esportato con questo profilo
				pXMLRecordInfo = m_pXMLExpImpMng->GetExportedRecord(pExtRefRecord, pXRefInfo->m_strProfile);
				if (!pXMLFirstRecordInfo)
					pXMLFirstRecordInfo = pXMLRecordInfo;
				if (!pXMLRecordInfo)
				{
					bOk = bOk && LoadExtRefRecordInDocument(pXRefInfo, TRUE);
					// in questo modo ho le info relative al primo file utilizzato per l'esportazione e il bookmark
					// utilizzato
					if (!pXMLNewRecordInfo)
						pXMLNewRecordInfo = m_pXMLExpImpMng->GetCurrentRecord();
				}

				//devo tenermi le info relative al primo file utilizzato per l'esportazione ed al bookmark
				// che risulta lo stesso per tutti i record esportati
				ExtRefTable.MoveNext();
			}
			
			ExtRefTable.Close();
			
			if (!pXMLNewRecordInfo)
			{
				if (pXMLFirstRecordInfo)
					m_pXMLExpImpMng->SetCurrentRecord(pXMLFirstRecordInfo);
			}
			else 
				m_pXMLExpImpMng->SetCurrentRecord(pXMLNewRecordInfo);

			delete pExtRefRecordToExport;
		}
	}
	return bOk;
}

//---------------------------------------------------------------------------
 CXMLProfileInfo* CXMLDataManager::GetXMLProfileInfo(CAbstractFormDoc* pDoc, const CString& strProfile, const CString& strDocNamespace /*=_T(" ")*/)
{
	CXMLProfileInfo* lpCurrentProfile = NULL;
	CString strProfileName = strProfile;
	CTBNamespace aDocNs = (pDoc) ? pDoc->GetNamespace() : CTBNamespace(strDocNamespace);
	if (aDocNs.IsEmpty())
		return NULL;

	// se è vuoto il profilo o non esiste
	if (strProfileName.IsEmpty() || !ExistProfile(aDocNs, strProfileName))
	{
		// provo per prima cosa con il profilo preferenziale
		CXMLDocInfo* pExtRefDocInfo = (pDoc) ? pDoc->GetXMLDocInfo() : NULL;
		if (!pExtRefDocInfo)
		{
			pExtRefDocInfo = new CXMLDocInfo(aDocNs);
			if (!pExtRefDocInfo->LoadAllFiles())
				delete pExtRefDocInfo;
		}

		if (pExtRefDocInfo)
			strProfileName = pExtRefDocInfo->GetPreferredProfile();
		
		// se non esiste il preferenziale provo con il predefinito e se non esiste
		// infine con la descrizione del documento
		if (strProfileName.IsEmpty() || !ExistProfile(aDocNs, strProfileName))
			strProfileName = (ExistProfile(aDocNs, szPredefined))
						? szPredefined
						: _T("");
	}
						  
	lpCurrentProfile = new CXMLProfileInfo(aDocNs, (LPCTSTR)strProfileName);
	lpCurrentProfile->LoadAllFiles();
	return lpCurrentProfile;
}

//---------------------------------------------------------------------------
BOOL CXMLDataManager::LoadExtRefRecordInDocument(CXMLXRefInfo* pXRefInfo, BOOL bNextFile, CXMLNode*	pSmartExtRefNode /*=NULL*/, CXMLProfileInfo* pProfileInfo/*=NULL*/)
{
	if (!pXRefInfo || !m_pXMLExpImpMng->GetCurrentDocElem())
	{
		ASSERT(FALSE);
		return FALSE;
	}


	CAbstractFormDoc* pExtRefDoc = m_pXMLExpImpMng->GetCurrentDocElem()->m_pDocument;
	if (!pExtRefDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}


	CXMLDataManagerObj* pExtRefXMLDataMng = pExtRefDoc->GetXMLDataManager();
	if (!pExtRefXMLDataMng || !pExtRefXMLDataMng->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	((CXMLDataManager*) pExtRefDoc->GetXMLDataManager())->m_RowFilters = m_RowFilters;
	//InitDocumentForImpExp(pExtRefDoc);
	//((CXMLDataManager*)pExtRefXMLDataMng)->m_bIsExtRef = TRUE; 
	
	ViewModeType aOldType = pExtRefDoc->GetType();
	pExtRefDoc->SetType(VMT_FINDER);

	CString strKey;
	pExtRefDoc->m_pDBTMaster->GetRecord()->GetKeyInXMLFormat(strKey, TRUE);
	// catturo i messaggi del documento
	
	//@@BAUZI per gestione unattended mode
	BeginListeningToDoc(pExtRefDoc);
	pExtRefDoc->m_pDBTMaster->SetNoPreloadStep(); //bugFix #24183
	pExtRefDoc->OnRadarRecordSelected(FALSE);

	// Il record correlato non è stato trovato: se l'external reference
	// prevede il vincolo di esistenza del dato nella tabella correlata
	// devo dare errore, altrimenti salto semplicemente l'esportazione
	if (!pExtRefDoc->ValidCurrentRecord())
	{
		EndListeningToDoc(pExtRefDoc, pXRefInfo->MustExist());
		
		if (pXRefInfo->m_bMustExist)
		{			
			OutputMessage(cwsprintf(_TB("The referenced document with key {0-%s} for the external reference {1-%s} in the table {2-%s} does not exist."), strKey, pXRefInfo->GetName(), pXRefInfo->GetTableName()));
			m_bErrorFound = TRUE;
		}
		return FALSE;
	}
	
	CXMLProfileInfo* pCurrentProfile = (pProfileInfo) ? pProfileInfo : GetXMLProfileInfo(pExtRefDoc, pXRefInfo->m_strProfile);
	
	((CXMLDataManager*)pExtRefDoc->GetXMLDataManager())->m_bBusy = TRUE;
	BOOL bOk = (pSmartExtRefNode)
		? ((CXMLDataManager*)pExtRefXMLDataMng)->ExportSmartExternalReference(pCurrentProfile, pXRefInfo, pSmartExtRefNode)
		: ((CXMLDataManager*)pExtRefXMLDataMng)->ExportCurrRecord(pCurrentProfile, pXRefInfo, bNextFile);
	
	pExtRefDoc->SetType(aOldType);
	((CXMLDataManager*)pExtRefDoc->GetXMLDataManager())->m_bBusy = FALSE;

	//posso avere lo stesso documento sia come root che come extref
	if (m_bIsRootDoc && m_bIsExtRef)
		m_bIsExtRef = FALSE;		

	EndListeningToDoc(pExtRefDoc);

	if (pCurrentProfile)
		delete pCurrentProfile;
	return bOk;
}


///----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema 
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportXMLSchemas 
							(
								CXMLProfileInfo*	pProfileInfo,
								CAbstractFormDoc*	pDoc,								
								CStringArray*		pProcessedArray /*= NULL*/,
								BOOL				bSmartDocument/*= FALSE*/
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
	
	CXMLDBTInfoArray *pDBTInfoArray = pProfileInfo->GetDBTInfoArray();
	if (!pDBTInfoArray)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BOOL bOK = TRUE;
	BOOL bResult = TRUE;

	if (bSmartDocument)
		bOK = CreateSmartXMLSchemaFile(pProfileInfo, pDoc);
	else
	{
		CXMLDBTInfo *pDBTInfo = NULL;
		
		for (int i=0; i < pDBTInfoArray->GetSize (); i++)
		{
			pDBTInfo = pDBTInfoArray->GetAt (i);
			if (!pDBTInfo || !pDBTInfo->IsToExport())
				continue;

			CXMLXRefInfoArray *pXRefs = pDBTInfo->GetXMLXRefInfoArray();
			if (!pXRefs)
				continue;
			
			CXMLXRefInfo *pXRef = NULL;
			for (int y=0; y<pXRefs->GetSize(); y++)
			{
				pXRef = pXRefs->GetAt (y);
				if (!pXRef || !pXRef->IsToUse())
					continue;
				
				CXMLUniversalKeyGroup* pUniversalKeyGroup = pXRef->GetXMLUniversalKeyGroup();
				if (pUniversalKeyGroup && !pUniversalKeyGroup->IsExportData())
					continue;

				bResult = ExportXRefXMLSchemas(pXRef, pProcessedArray) && bResult;
			}
		}

		bOK = CreateExportXMLSchemaFile (pProfileInfo, pDoc);
	}

	CString strProfile = (pProfileInfo->m_strProfileName.IsEmpty())
						? _TB("Document description")
						: pProfileInfo->m_strProfileName;

	if (bOK)
			OutputMessage (
			cwsprintf(_TB("Export of the document {0-%s} schema file with export profile {1-%s} successfully completed."), pDoc->GetTitle(),  strProfile),
					CXMLLogSpace::XML_INFO
					);

	else
		OutputMessage (cwsprintf(_TB("Error exporting ot the document {0-%s} schema file with export profile {1-%s}."), pDoc->GetTitle(), strProfile));

	return bResult && bOK;
}

//----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema 
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportXRefXMLSchemas
							(
								CXMLXRefInfo*			pXRefInfo,
								CStringArray*			pProcessedArray /*= NULL*/
							) 
{
	if (!m_pXMLExpImpMng || !pXRefInfo)
	{
		ASSERT(FALSE);
		return FALSE; 
	}
	
	CString	strProfileName = pXRefInfo->GetProfile();
	CTBNamespace aDocumentNameSpace = pXRefInfo->GetDocumentNamespace();

	CXMLDocElement *pDocElement = m_pXMLExpImpMng->GetXMLDocElement(aDocumentNameSpace, m_pDoc);
	if (!pDocElement || !pDocElement->m_pDocument)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	// se è vuoto il profilo o non esiste
	if (strProfileName.IsEmpty() || !ExistProfile(aDocumentNameSpace, strProfileName))
	{
		// provo per prima cosa con il profilo preferenziale
		CXMLDocInfo* pExtRefDocInfo = pDocElement->m_pDocument->GetXMLDocInfo();
		if (pExtRefDocInfo)
			strProfileName = pExtRefDocInfo->GetPreferredProfile();
		
		// se non esiste il preferenziale provo con il predefinito e se non esiste
		// infine con la descrizione del documento
		if (strProfileName.IsEmpty() || !ExistProfile(aDocumentNameSpace, strProfileName))
			strProfileName = (ExistProfile(aDocumentNameSpace, szPredefined))
						? szPredefined
						: _T("");
	}

	// se presente l'array, lo uso per tenere traccia delle coppie
	// namespace-profilo già processate per evitare la ricorsività
	if (pProcessedArray)
	{
		CString strElem = aDocumentNameSpace.ToString() + strProfileName;
		for (int i=0; i<pProcessedArray->GetSize(); i++)
		{
			if (pProcessedArray->GetAt(i) == strElem)
				return TRUE;
		}
		pProcessedArray->Add ((LPCTSTR)strElem);
	}


	
	CXMLProfileInfo Info (aDocumentNameSpace, (LPCTSTR)strProfileName);
	return Info.LoadAllFiles() && ExportXMLSchemas(&Info, pDocElement->m_pDocument, pProcessedArray);

}

//----------------------------------------------------------------------------
// Metodo che genera in automatico il file dello schema 
//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateExportXMLSchemaFile
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

	CString strOriginalSchemaFile = GetSchemaProfileFile(pProfileInfo);
	if (strOriginalSchemaFile.IsEmpty())
		return FALSE;

	CString strXMLPath = m_pXMLExpImpMng->GetTXEnvFolderSchemaPath(); 
	if (!IsDirSeparator(strXMLPath.Right(1))) 
		strXMLPath += SLASH_CHAR;
	CString strXMLSchemaFile = MakeFilePath(strXMLPath, GetNameWithExtension(strOriginalSchemaFile));
	if (strXMLSchemaFile.IsEmpty())
		return FALSE;
	
	// se c'è già non devo generarlo
	//if (CheckFileExistence(strXMLSchemaFile))
	//	return TRUE;
	
	// se necessario, genero il file di schema
	CreateXMLSchemaFile(pProfileInfo, pDoc);
	
	if (CopyFile(strOriginalSchemaFile, strXMLSchemaFile, FALSE))
	{
		m_pXMLExpImpMng->AddEnvFile
				(
					CXMLEnvFile::SCHEMA_FILE,
					::GetNameWithExtension(strXMLSchemaFile)
				);
		return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::IsDataObjValue(CXMLNode* pColumnNode, DataObj* pDataObj) const
{
	if (!pDataObj)
		return (pColumnNode == NULL);
	
	if (!pColumnNode)
		return (pDataObj == NULL);

	CString strValue;
	pColumnNode->GetText(strValue);
	
	if (!IsNotEmptyDataObj(pDataObj))
		return strValue.IsEmpty();

	DataObj* pTmpDataObj = pDataObj->DataObjClone();

	pTmpDataObj->AssignFromXMLString((LPCTSTR)strValue);

	BOOL bEqual = pTmpDataObj->IsEqual(*pDataObj);

	delete pTmpDataObj;

	return bEqual;
}

// SELECT Testa.* FROM Testa, Righe WHERE Testa.PKKEY = Righe.PKKEY And Righe.ExtRef = ?
//----------------------------------------------------------------------------
RecordArray* CXMLDataManager::ExecJoinFromSlaves 
											(
												CXMLXRefInfo*		pXRefInfo, 
												SqlRecord*			pDBTRecord,
												CAbstractFormDoc*	pExtRefDoc,
												CXMLNode*			pExtRefNode /*=NULL*/ 
											) 
{
	RecordArray* pRecordArray = NULL;
	CString sSelect;
	SqlTableInfo* pTblInfo = pExtRefDoc->GetSqlConnection()->GetTableInfo(pXRefInfo->GetReferencedTableName());
	SqlForeignKeysReader aFKReader;
	if (!pTblInfo)
	{
		ASSERT(FALSE);
		TRACE ("ExecJoinFromSlaves: referenced table name is not found into catalog ");
		return pRecordArray;
	}

	m_RowFilters.Clear ();

	const SqlCatalogEntry*	pCatalogEntry	= pTblInfo->GetSqlCatalogEntry();
	const SqlUniqueColumns*	pUniqueColumns = pTblInfo->GetSqlUniqueColumns();
	
	if (!pUniqueColumns || !pCatalogEntry)
	{
		ASSERT(FALSE);
		TRACE1 ("ExecJoinFromSlaves: referenced %s table name is not found into catalog ", pXRefInfo->GetReferencedTableName());
		return pRecordArray;
	}

	DBTMaster* pExtRefDBTMaster = pExtRefDoc->m_pDBTMaster;

	aFKReader.LoadForeignKeys (pExtRefDBTMaster->GetTable()->GetTableName(), pTblInfo->GetTableName(), pExtRefDoc->GetReadOnlySqlSession());
	if (!aFKReader.GetSize())
	{
		ASSERT(FALSE);
		TRACE ("ExecJoinFromSlaves: No foreign keys found for %s to %s external reference ", pExtRefDBTMaster->GetTable()->GetTableName(), pTblInfo->GetTableName());
		return pRecordArray;
	}

	SqlRecord* pMasterRec	= (SqlRecord*) pExtRefDoc->m_pDBTMaster->GetRecord()->Create();
	SqlRecord* pSlaveRec	= (SqlRecord*) pCatalogEntry->GetSqlRecordClass()->CreateObject();
	SqlTable*  pMasterTable	= new SqlTable(pMasterRec, pExtRefDoc->GetReadOnlySqlSession());

	SqlRecordItem* pItem;
	TRY
	{
		pMasterTable->Open (FALSE, TRUE);
		BOOL bFirst = TRUE;
		for (int i=0; i <= pMasterRec->GetUpperBound(); i++)
		{
			pItem = pMasterRec->GetAt(i);
			if (!pItem->IsSpecial())
				continue;

			if (!bFirst)
				sSelect += _T(",");

			pMasterTable->Select (pItem->GetDataObj());
			sSelect += cwsprintf(_T("%s.%s"), pMasterRec->GetTableName(), pItem->GetColumnName());
			bFirst = FALSE;
		}

		pMasterTable->m_strSQL = 
						cwsprintf(
									_T("SELECT %s FROM %s, %s WHERE "),
									sSelect, 
									pMasterRec->GetTableName(), 
									pSlaveRec->GetTableName()
								);
		DataObj* pMasterDataObj;
		DataObj* pSlaveDataObj;

		// Master and Slaves Foreign keys to create inner join query
		CString sSlaveTable, sSlaveCol, sMasterTable, sMasterCol, subSelect;
		for (int i=0; i <= aFKReader.GetUpperBound(); i++)
		{
			aFKReader.GetForeignKey(i, sSlaveTable, sSlaveCol, sMasterTable, sMasterCol);

			pMasterDataObj	= pMasterRec->GetDataObjFromColumnName(sMasterCol);
			pSlaveDataObj	= pSlaveRec->GetDataObjFromColumnName(sSlaveCol);

			if	(
					!pMasterDataObj || !pSlaveDataObj || 
					pMasterRec->GetTableName().CompareNoCase(sMasterTable) ||
					pSlaveRec->GetTableName().CompareNoCase(sSlaveTable)
				)
			{
				ASSERT(FALSE);
				TRACE2 ("ExecJoinFromSlaves: foreign keys slave to master relation declared does not correspond to SqlRecord structures. %s -> %s ", sSlaveTable + "." + sSlaveCol, sMasterTable + "." + sMasterCol);
				goto CleanUp;
			}

			pMasterTable->m_strSQL += cwsprintf(_T("%s.%s = %s.%s"), sMasterTable, sMasterCol, sSlaveTable, sSlaveCol/*, pSlaveRec->GetTableName()*/);
			if (i < aFKReader.GetUpperBound())
				pMasterTable->m_strSQL += _T(" And ");
		}
	
		SqlRecord* pExtRefRecord = pExtRefDBTMaster->GetRecord();
		if (!pExtRefRecord)
		{
			ASSERT(FALSE);
			goto CleanUp;
		}

		DataObj* pRefValue;
		
		// segments related to the DbtSlaveBuffered external reference
		for (int nSegIdx = 0; nSegIdx < pXRefInfo->GetSegmentsNum(); nSegIdx++)
		{
			CXMLSegmentInfo* pExtRefSegInfo = pXRefInfo->GetSegmentAt(nSegIdx);
			if (!pExtRefSegInfo)
			{
				ASSERT(FALSE);
				TRACE ("ExecJoinFromSlaves: cannot extract external reference segments declaration" );
				goto CleanUp;
			}

			pRefValue = pDBTRecord->GetDataObjFromColumnName (pExtRefSegInfo->GetFKSegment());

			if (!pRefValue)
			{
				ASSERT(FALSE);
				TRACE ("ExecJoinFromSlaves: cannot extract external reference value from exported record" );
				goto CleanUp;
			}

			if (pRefValue->IsEmpty () && !pXRefInfo->CanBeNull())
			{
				OutputMessage(cwsprintf(_TB("The External Reference {0-%s} in the table {1-%s} has null key segments.\r\nUnable to export the related document."), pXRefInfo->GetName(), pDBTRecord->GetTableName()));
				m_bErrorFound = TRUE;
				goto CleanUp;
			}
			
			// I save the data for row filtering on DBTSlaveBuffered export procedure
			if (m_RowFilters.m_sXRefTableName.IsEmpty())
				m_RowFilters.m_sXRefTableName = pXRefInfo->GetReferencedTableName ();
			m_RowFilters.m_RowFilters.Add (new CXMLVariable (pExtRefSegInfo->GetReferencedSegment(), pRefValue->Clone()));

			pMasterTable->m_strSQL += cwsprintf(_T(" And %s.%s = %s"), 
										pXRefInfo->GetReferencedTableName (),
										pExtRefSegInfo->GetReferencedSegment(), 
										pExtRefDoc->GetSqlConnection()->NativeConvert(pRefValue)
									);
		}
		pMasterTable->m_strOldSQL = pMasterTable->m_strSQL;
		
		pMasterTable->Query ();

		while (!pMasterTable->IsEOF())
		{
			if (!pRecordArray)
				pRecordArray = new RecordArray();
		
			AddIfNotExists (pRecordArray, pMasterTable->GetRecord());
			pMasterTable->MoveNext ();
		}
	}
	CATCH (SqlException, e)
	{
		ASSERT(FALSE);
		TRACE1 ("ExecJoinFromSlaves: SQL exception %s ", e->m_strError);
		//goto CleanUp;
	
	}
	END_CATCH

CleanUp:
	if (pMasterTable->IsOpen())
		pMasterTable->Close();

	delete pMasterRec;
	delete pSlaveRec;
	delete pMasterTable;

	return pRecordArray;
}

//----------------------------------------------------------------------------
void CXMLDataManager::AddIfNotExists (Array* pRecordArray, SqlRecord* pRecord)
{
	BOOL bFound = FALSE;

	SqlRecord* pExistingRec;
	for (int i=0; i <= pRecordArray->GetUpperBound(); i++)
	{
		pExistingRec = (SqlRecord*) pRecordArray->GetAt(i);
		if (*pExistingRec == *pRecord)
		{
			bFound = TRUE;
			break;
		}
	}

	if (!bFound)
	{
		SqlRecord* pNewRecord = pRecord->Create();
		*pNewRecord = *pRecord;
		pRecordArray->Add (pNewRecord);
	}
}

//----------------------------------------------------------------------------
//	Class CXMLDataExportDoc definition
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLDataExportDoc, CClientDoc)

BEGIN_MESSAGE_MAP(CXMLDataExportDoc, CClientDoc)
	//{{AFX_MSG_MAP(CXMLDataExportDoc)
	ON_COMMAND(ID_EXTDOC_EXPORT_XML_DATA, OnDataExport)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_EXPORT_XML_DATA, OnUpdateExportXMLData)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CXMLDataExportDoc::CXMLDataExportDoc()
{
}

//----------------------------------------------------------------------------
CAbstractFormDoc* CXMLDataExportDoc::GetServerDoc()
{
	CBaseDocument*	pServerDoc = GetMasterDocument();
	ASSERT(pServerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));
	return (CAbstractFormDoc*)pServerDoc;
}

//----------------------------------------------------------------------------
BOOL CXMLDataExportDoc::OnAttachData () 
{
	Attach (new CExportEvents());
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataExportDoc::Customize()
{
	SetDocAccel(IDR_TB_XMLTRANSFER);
	CreateJsonToolbar(IDD_EXPORT_TOOLBAR);
}

//----------------------------------------------------------------------------
void CXMLDataExportDoc::OnDataExport()
{
	ExportData();
}

//----------------------------------------------------------------------------
void CXMLDataExportDoc::ExportData()
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
		pXMLDataMng->Export();

	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;

	return;
}

//-----------------------------------------------------------------------------
bool CXMLDataExportDoc::CanExportData()
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	if (pServerDoc==NULL)
	{
		return false;
	}

	return 
		AfxIsActivated(TBEXT_APP, INTERACTIVE_IMP_EXP) &&
		pServerDoc->CanDoExportXMLData() &&
		pServerDoc->GetType() != VMT_FINDER && 
		pServerDoc->GetType() != VMT_BATCH && 
		!AfxGetXMLImpExpController()->IsBusy() &&
		pServerDoc->GetXMLDataManager() &&
		pServerDoc->CanLoadXMLDescription() &&
		pServerDoc->GetFormMode() == CAbstractFormDoc::BROWSE &&
		!AfxGetSiteName().IsEmpty() &&
		!AfxGetDomainName().IsEmpty() &&
		OSL_CAN_DO(pServerDoc->GetInfoOSL(), OSL_GRANT_XMLEXPORT);
}
//-----------------------------------------------------------------------------
void CXMLDataExportDoc::OnUpdateExportXMLData(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanExportData());
}

