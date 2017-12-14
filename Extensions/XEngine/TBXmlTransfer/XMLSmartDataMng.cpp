#include "stdafx.h" 
#include <Vfw.h>
#include <digitalv.h>

#include <TBXMLCore\xmldocobj.h>
#include <TBXMLCore\xmlgeneric.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <tbgeneric\dataobj.h>
#include <TBGeneric\EnumsTable.h>
#include <tbgeneric\formatstable.h>
#include <tbgeneric\globals.h>
 
#include <TBGENLIB\generic.h>
#include <TBGENLIB\baseapp.h>

#include <TBOleDB\sqltable.h>

#include <TBWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\inputmng.h>
#include <TBWoormEngine\prgdata.h>

#include <TBGES\dbt.h>
#include <TBGES\browser.h>
#include <TBGES\extdoc.hjson> //JSON AUTOMATIC UPDATE
#include <TBGES\xsltmng.h>
#include <TBGES\eventmng.h>
#include <TBGES\barquery.h>

#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "XMLTransferTags.h"
#include "GenFunc.h"
#include "ExpCriteriaObj.h"
#include "ExpCriteriaDlg.h"
#include "ExpCriteriaWiz.h"
#include "XMLProfileInfo.h"
#include "XMLDataMng.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szData		[]	= _T("Data");
static const TCHAR szParams	[]	= _T("Params");
static const TCHAR szPostBack	[]	= _T("PostBack");
static const TCHAR szError	[]	= _T("Error");
const TCHAR szTBBusinessObject[] = _T("TaskBuilder-BusinessObject");
const TCHAR szXTechDataManager[] = _T("XTech-DataManager");


/////////////////////////////////////////////////////////////////////////////
//			class CSmartCommonParams definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSmartCommonParams::CSmartCommonParams(DataArray* pResults)
:
	m_pResults(pResults),
	m_ePosType(CPathFinder::STANDARD)
{
}

//----------------------------------------------------------------------------
void CSmartCommonParams::SetProfileInfoByNsUri(const CString strNsUri)
{
	CString nsURIReverse = strNsUri;
	nsURIReverse.MakeReverse();

	int nCurrPos = 0;
	int i = 0;
	CString strToken(_T("/"));
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
				m_strProfile = strElem.Left(nPos);
		}

		if (i == 1)
		{
			if (strElem.CompareNoCase(szAllUserDirName) == 0)
				m_ePosType = CPathFinder::ALL_USERS;
			else
			{
				if (strElem.CompareNoCase(szStandard) == 0)
					m_ePosType = CPathFinder::STANDARD;
				else
				{
					m_ePosType = CPathFinder::USERS;
					m_strUserName = strElem;
				}
			}
		}

		i++;
	}
}

/////////////////////////////////////////////////////////////////////////////
//			class CSmartExportParams definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSmartExportParams::CSmartExportParams
							(
								DataArray* pResults,
								BOOL bUseApproximation, 
								int loadAction							
							)
:
	CSmartCommonParams			(pResults),
	m_bUseApproximation			(bUseApproximation),
	m_loadAction				(loadAction),
	m_bExportCurrentDocument	(FALSE)
{
}

/////////////////////////////////////////////////////////////////////////////
//			class CSmartImportParams definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSmartImportParams::CSmartImportParams(DataArray* pResults, int saveAction, BOOL bOnlySetData)
:
	CSmartCommonParams	(pResults),
	m_saveAction		(saveAction),
	m_bOnlySetData		(bOnlySetData)		
{
}

// classe per la gestione di un file contenente errori
////////////////////////////////////////////////////////////////////////////
//			class CSmartMessageElement definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSmartMessageElement::CSmartMessageElement(CXMLLogSpace::XMLMsgType nMsgType, int nCode, const CString& strSource, const CString& strMessage)
:
	m_nMsgType	(nMsgType),
	m_nCode		(nCode),
	m_strSource	(strSource),
	m_strMessage(strMessage)
{
}


//----------------------------------------------------------------------------
void CSmartMessageElement::Unparse(CXMLNode* pParentNode)
{
	if (!pParentNode)
		return;
	
	CXMLNode* pMsgNode = (m_nMsgType == CXMLLogSpace::XML_ERROR)
						? pParentNode->CreateNewChild(DOC_XML_ERROR_TAG)
						:  pParentNode->CreateNewChild(DOC_XML_WARNING_TAG);

	CXMLNode* pChildNode = pMsgNode->CreateNewChild(DOC_XML_CODE_TAG);
	pChildNode->SetText(cwsprintf(_T("%d"),m_nCode));
	pChildNode = pMsgNode->CreateNewChild(DOC_XML_SOURCE_TAG);
	pChildNode->SetText(m_strSource);
	pChildNode = pMsgNode->CreateNewChild(DOC_XML_MESSAGE_TAG);
	pChildNode->SetText(m_strMessage);
}

////////////////////////////////////////////////////////////////////////////
//			class CSmartXMLDiagnosticMng definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSmartXMLDiagnosticMng::CSmartXMLDiagnosticMng()
:
	m_pXMLDocument		(NULL),
	m_pErrorList		(NULL),
	m_pWarningList		(NULL)	
{
}

//----------------------------------------------------------------------------
CSmartXMLDiagnosticMng::~CSmartXMLDiagnosticMng()
{
	if (m_pErrorList)
		delete m_pErrorList;

	if (m_pWarningList)
		delete m_pWarningList;

	if (m_pXMLDocument)
		delete m_pXMLDocument;
}

//----------------------------------------------------------------------------
void CSmartXMLDiagnosticMng::AddMessage(CXMLLogSpace::XMLMsgType nMsgType, int nCode, LPCTSTR lpszSource, LPCTSTR lpszMessage)
{
	if (nMsgType == CXMLLogSpace::XML_ERROR)
	{
		if (!m_pErrorList)
			m_pErrorList = new Array;

		m_pErrorList->Add(new CSmartMessageElement(nMsgType, nCode, lpszSource, lpszMessage));
	}

	if (nMsgType == CXMLLogSpace::XML_WARNING)
	{
		if (!m_pWarningList)
			m_pWarningList = new Array;

		m_pWarningList->Add(new CSmartMessageElement(nMsgType, nCode, lpszSource, lpszMessage));
	}
}

//----------------------------------------------------------------------------
void CSmartXMLDiagnosticMng::ClearMessages()
{
	if (m_pErrorList)
		m_pErrorList->RemoveAll();

	if (m_pWarningList)
		m_pWarningList->RemoveAll();
}



//----------------------------------------------------------------------------
BOOL CSmartXMLDiagnosticMng::CreateHeader(CXMLNode* pRoot, BOOL bMessageBoxEnable)
{
	CString strRootName;
	CString strNamespaceURI;
	CString strTBNamespace;
	CString strProfileName;
	//considero i valori degli attributi della root passatomi che fa riferimento al file su cui il processo ha generato un errore
	if (m_pXMLDocument)
		delete m_pXMLDocument;

	m_pXMLDocument = new CXMLDocumentObject(TRUE, bMessageBoxEnable);

	if (pRoot)
	{
		pRoot->GetBaseName(strRootName);	
		strNamespaceURI = pRoot->GetNamespaceURI();
		pRoot->GetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, strTBNamespace);
		pRoot->GetAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE, strProfileName);
	}
	else
		strRootName = _TB("DiagnosticDocument");

	if (!strNamespaceURI.IsEmpty())
		m_pXMLDocument->SetNameSpaceURI(pRoot->GetNamespaceURI(), szNamespacePrefix);

	CXMLNode* pNewRoot = m_pXMLDocument->CreateRoot(strRootName);
	if (!pNewRoot)
		return FALSE;
	

	if (!strTBNamespace.IsEmpty())
		pNewRoot->SetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, strTBNamespace);

	if (!strProfileName.IsEmpty())
		pNewRoot->SetAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE, strProfileName);

	return TRUE;
}

//----------------------------------------------------------------------------
void CSmartXMLDiagnosticMng::InsertDocumentMessage(CDiagnostic* pDiagnostic)
{
	if (!pDiagnostic || (!pDiagnostic->ErrorFound() && !pDiagnostic->WarningFound()))
		return;

	CXMLLogSpace::XMLMsgType aMsgType = (pDiagnostic->ErrorFound()) ? CXMLLogSpace::XML_ERROR : CXMLLogSpace::XML_WARNING;

	AddMessage(aMsgType, DOC_DIAGNOSTIC_CODE, szTBBusinessObject, pDiagnostic->ToString());
}


	// crea il nodo dignostic contenente le info di error e/o warning. Verr?inserito in un file gi?esistente
//----------------------------------------------------------------------------
void CSmartXMLDiagnosticMng::AppendDiagnosticNode(CXMLNode* pRoot)
{
	if (!HasMessages())
		return;

	CXMLNode* pDiagnosticNode = pRoot->GetChildByName(DOC_XML_DIAGNOSTIC_TAG);
	if (!pDiagnosticNode)
		 pDiagnosticNode = pRoot->CreateNewChild(DOC_XML_DIAGNOSTIC_TAG);

	// creo gli eventuli errori presenti nella lista dei messaggi 
	if (m_pErrorList && m_pErrorList->GetSize() > 0)
	{
		CXMLNode* pErrorsNode = pRoot->GetChildByName(DOC_XML_ERRORS_TAG);
		if (!pErrorsNode)
			 pErrorsNode = pDiagnosticNode->CreateNewChild(DOC_XML_ERRORS_TAG);
		
		for(int nErr = 0; nErr <= m_pErrorList->GetUpperBound(); nErr++)
		{
			CSmartMessageElement* pElement = (CSmartMessageElement*)m_pErrorList->GetAt(nErr);
			if (pElement)
				pElement->Unparse(pErrorsNode);
		}
	}
	// scorro gli eventuli warning presenti nella diagnostica 
	if (m_pWarningList && m_pWarningList->GetSize() > 0)
	{
		CXMLNode* pWarningsNode = pRoot->GetChildByName(DOC_XML_WARNINGS_TAG);
		if (!pWarningsNode)
			pWarningsNode = pDiagnosticNode->CreateNewChild(DOC_XML_WARNINGS_TAG);

		for(int nWrn = 0; nWrn <= m_pWarningList->GetUpperBound(); nWrn++)
		{
			CSmartMessageElement* pElement = (CSmartMessageElement*)m_pWarningList->GetAt(nWrn);
			if (pElement)
				pElement->Unparse(pWarningsNode);
		}
	}
}

//----------------------------------------------------------------------------
CString CSmartXMLDiagnosticMng::CreateXMLErrorString(CXMLNode* pRoot, BOOL bMessageBoxEnable /*=FALSE*/)
{
	if (!HasMessages()|| !CreateHeader(pRoot, bMessageBoxEnable)) 
		return _T("");	
		
	AppendDiagnosticNode(m_pXMLDocument->GetRoot());
	CString strValue;
	if (m_pXMLDocument->GetRoot()->GetXML(strValue))
		return strValue;
	return _T("");	
}


//----------------------------------------------------------------------------
// Metodi necessari all'esportazione dei dati necessari all'integrazione con Office2003
//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
BOOL CXMLDataManager::LoadSmartProfile(const CString& strProfilePath)
{
	if (!m_pSmartProfile || m_pSmartProfile->m_strDocProfilePath.CompareNoCase(strProfilePath) != 0)
	{
		// se il profilo non esiste segnalo errore, perch?se utilizzo la descrizione del documento (come in XTech standard) rischio
		// di creare un file di dati non compatibile con lo schema mappato in office2003
		if (!ExistProfile(strProfilePath))
			return FALSE;

		if (m_pSmartProfile)
			delete m_pSmartProfile;
		// Caricamento del profilo di esportazione utilizzato per la creazione del file xml da esportare/importare
		m_pSmartProfile =  new CXMLProfileInfo(m_pDoc, strProfilePath);
		m_pSmartProfile->LoadAllFiles();
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartExternalReference(CXMLProfileInfo* pCurrentProfile, CXMLXRefInfo* pXRefInfo, CXMLNode* pExtRefNode)
{
	if (!pExtRefNode || !pXRefInfo || !pCurrentProfile)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	if (!ExportSmartSingleDBT(pExtRefNode, m_pDoc->m_pDBTMaster, pCurrentProfile))
		return FALSE;

	//SqlRecord* pRecord = m_pDoc->m_pDBTMaster->GetRecord();
	// itero sugli slave
	DBTArray* pDBTSlaves = m_pDoc->m_pDBTMaster->GetDBTSlaves();
	if (pDBTSlaves && pDBTSlaves->GetSize())
	{
		for (int i = 0; i < pDBTSlaves->GetSize(); i++)
		{
			DBTSlave* pDBTSlave = pDBTSlaves->GetAt(i);
			ASSERT(pDBTSlave);

			// Se sono in stato di BROWSE e per il DBT ?stata impostata la 
			// lettura ritardata devo forzare il caricamento dei dati
			pDBTSlave->Reload(TRUE);

			ExportSmartSingleDBT(pExtRefNode, pDBTSlave, pCurrentProfile);
		}
	}	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ProcessSmartExtRef(
											CXMLXRefInfo*	pXRefInfo, 
											LPCTSTR			lpszColumnName, 
											SqlRecord*		pDBTRecord,
											CXMLNode*		pRecordNode
										)
{

	CXMLDocElement* pOldXMLDocElem = m_pXMLExpImpMng->GetCurrentDocElem();
				
	CXMLNode* pExtRefNode = pRecordNode->CreateNewChild(pXRefInfo->GetName());

	//se non ?andata a buon fine allora inserisco solo l'elemento colonna identificato da lpszColumnName
	if (!ProcessExtRef(pXRefInfo, lpszColumnName, pDBTRecord, pExtRefNode, TRUE))
	{
		m_pXMLExpImpMng->SetCurrentDocElem(pOldXMLDocElem);	
		pRecordNode->RemoveChild(pExtRefNode);
		return FALSE;
	}
    //Inserisco le informazioni di external-reference
	pExtRefNode->SetAttribute(DOC_XML_EXTREF_ATTRIBUTE, _T("true"));
	m_pXMLExpImpMng->SetCurrentDocElem(pOldXMLDocElem);	
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::ExportSmartRecordFields(CXMLNode* pRecordNode, SqlRecord* pRecord, CXMLDBTInfo* pXMLDBTInfo)
{
	if (!pRecordNode || !pRecord)
	{
		ASSERT(FALSE);
		return;
	}
	
	//array che contiene il nome degli external reference gi?
	//processati per il dbt corrente
	CStringArray aProcessedExtRefs;
	CXMLNode* pFieldNode = NULL;
	BOOL bUseSoapType = TRUE;

	//inserisco i soli campi del dbt che sono da esportare 
	for (int nColIdx = 0; nColIdx < pRecord->GetSize(); nColIdx++)
	{
		SqlRecordItem* pRecItem = pRecord->GetAt(nColIdx);
		CString strColumnName = pRecItem->GetColumnName();
		if (pRecord->IsVirtual(nColIdx) || !pXMLDBTInfo->IsFieldToExport(strColumnName) || !pRecItem->GetDataObj())
			continue;
		
		bUseSoapType = pRecItem->GetDataObj()->GetDataType()!= DATA_DATE_TYPE &&
					(!pRecItem->GetDataObj()->IsKindOf(RUNTIME_CLASS(DataDbl)) ||
					(!(((CSmartExportParams*)m_pXMLSmartParam)->m_bUseApproximation)));		
		
		CXMLXRefInfo* pXRefInfo = NULL;
		CXMLXRefInfoArray aXMLXRefInfoArray;
		aXMLXRefInfoArray.SetOwns(FALSE);
		if (pXMLDBTInfo->m_pXRefsArray)
			pXMLDBTInfo->m_pXRefsArray->GetXRefArrayByFK((LPCTSTR)strColumnName, &aXMLXRefInfoArray);
	
		
		// se sto esportando campi di un documento external-reference oppure
		// non ho external-references inserisco solo l'elemento field
		if (m_bIsExtRef || aXMLXRefInfoArray.GetSize() <= 0 ||  (m_bIsPostBack && m_pSmartProfile->IsNoExtRefPostBack()))
		{
			pFieldNode = pRecordNode->CreateNewChild((LPCTSTR)strColumnName);	
			pFieldNode->SetText(pRecItem->GetDataObj()->FormatDataForXML(bUseSoapType));
			continue;
		}
		
		//ciclo sugli external reference definiti per il campo
		for (int nRef = 0; nRef < aXMLXRefInfoArray.GetSize(); nRef++)
		{
			pXRefInfo = aXMLXRefInfoArray.GetAt(nRef);
			if (!pXRefInfo)
				continue;

			BOOL bContinue = FALSE;				
			// se ?un external-reference che ho gi?inserito non faccio niente
			for (int i = 0; i <= aProcessedExtRefs.GetUpperBound(); i++)
			{	
				if (pXRefInfo->GetName() == aProcessedExtRefs.GetAt(i))
				{
					bContinue = TRUE;
					break;
				}
					
			}
			if (bContinue) continue;

			aProcessedExtRefs.Add(pXRefInfo->GetName());
			// inserisco l'ext-reference ovvero inserisco del documento corrispondente con i campi
			// che verificano il profilo associato all'ext-ref. 
			if (!pXRefInfo->IsToUse() || !ProcessSmartExtRef(pXRefInfo, strColumnName, pRecord, pRecordNode))
			{
				pFieldNode = pRecordNode->CreateNewChild((LPCTSTR)strColumnName);	
				pFieldNode->SetText(pRecItem->GetDataObj()->FormatDataForXML(bUseSoapType));
			}
		}
	}
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartSingleDBT(CXMLNode* pParentNode, DBTObject* pDBT, CXMLProfileInfo* pProfileInfo)	
{
	if (!pDBT || !pParentNode)
	{
		ASSERT (FALSE);
		return FALSE;
	}

	CXMLDBTInfo* pXMLDBTInfo = 	pProfileInfo  
							? pProfileInfo->GetDBTFromNamespace(pDBT->GetNamespace()) 
							: pDBT->GetXMLDBTInfo();

	
	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport())
		return FALSE;

	CString strDBTName = pXMLDBTInfo->GetNamespace().GetObjectNameForTag();
	CXMLNode* pDBTNode = pParentNode->CreateNewChild(strDBTName);
	
	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)) && ((DBTMaster*)pDBT)->OnOkXMLExport())
	{
		pDBTNode->SetAttribute(DOC_XML_MASTER_ATTRIBUTE, _T("true"));
		((DBTMaster*)pDBT)->OnBeforeXMLExport();
		ExportSmartRecordFields(pDBTNode, pDBT->GetRecord(), pXMLDBTInfo);
	}

	//nel caso di dbtslavebuffer, per ogni record creo il nodo Row
	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		DBTSlaveBuffered* pDBTBuffered = (DBTSlaveBuffered*)pDBT;
		for(int nRow = 0; nRow < pDBTBuffered->GetSize(); nRow++)
		{
			if (!pDBTBuffered->OnOkXMLExport(nRow))
				continue;		
			if (pXMLDBTInfo->GetChooseUpdate())
				pDBTNode->SetAttribute(DOC_XML_UPDATE_ATTRIBUTE, (LPCTSTR)pXMLDBTInfo->GetStrUpdateType());
			pDBTBuffered->SetCurrentRow(nRow);			
			pDBTBuffered->OnBeforeXMLExport(nRow);
			CXMLNode* pRowNode = pDBTNode->CreateNewChild(strDBTName + XML_ROW_TAG);
			ExportSmartRecordFields(pRowNode,pDBTBuffered->GetCurrentRow(), pXMLDBTInfo);
		}
	}
	else
	{
		if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlave)) && (!pDBT->GetRecord()->IsEmpty()) && ((DBTSlave*)pDBT)->OnOkXMLExport())
		{
			((DBTSlave*)pDBT)->OnBeforeXMLExport();
			ExportSmartRecordFields(pDBTNode, pDBT->GetRecord(), pXMLDBTInfo);
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartDataSection(CXMLNode* pDocumentNode, DBTMaster* pDBTMaster, CXMLProfileInfo* pProfileInfo)	
{
	BOOL bOk = TRUE;

	CXMLNode* pDataNode = pDocumentNode->CreateNewChild(DOC_XML_DATA_TAG);
	if (!pDataNode)
		return FALSE;

	//se ?un record deletato non devo andare avanti
	if (!ExportSmartSingleDBT(pDataNode, pDBTMaster, pProfileInfo))
	{
		m_nProcessStatus = XML_EXPORT_FAILED;
		return FALSE;
	}
	
	DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
	if (pDBTSlaves && pDBTSlaves->GetSize())
	{
		for (int i = 0; i < pDBTSlaves->GetSize(); i++)
		{
			DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);
			ASSERT(pDBTSlave);

			// Se sono in stato di BROWSE e per il DBT ?stata impostata la 
			// lettura ritardata devo forzare il caricamento dei dati
			pDBTSlave->Reload(TRUE);
			
			if (!ExportSmartSingleDBT(pDataNode, pDBTSlave, pProfileInfo))
			{	
				if  (m_nProcessStatus == XML_EXPORT_FAILED)
				{
					bOk = FALSE;
					break;
				}
			}
		}
	}
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartCurrentRecord(CXMLProfileInfo* pProfileInfo)
{
#ifndef DEBUG
	try
	{
#endif
	CString strMessage;
	// rilascio la coda dei messaggi per rendere responsiva l'applicazione
	if (!AfxIsInUnattendedMode())
		m_pDoc->m_BatchScheduler.CheckMessage();

	DBTMaster* pDBTMaster = m_pDoc->m_pDBTMaster;
	ASSERT(pDBTMaster);

	CString strDocKey;
	pDBTMaster->GetRecord()->GetKeyInXMLFormat(strDocKey, TRUE);

	//INIZIO ESPORTAZIONE DEL DOCUMENTO
	m_pDoc->OnBeforeXMLExport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnBeforeXMLExport();

	CString strProfileName = (pProfileInfo) ? pProfileInfo->GetName() : _T("");
	
	CString strUrlData;
	if (pProfileInfo)
		strUrlData = pProfileInfo->GetUrlData();
		
	if (strUrlData.IsEmpty())
		strUrlData = GetDocTitle();

	if (strUrlData.IsEmpty())
	{
		OutputMessage(_TB("Export failed.\r\nThe name of the data export file is not defined."), 
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		return FALSE;
	}

	CString strXMLExpFile = GetName(strUrlData) + strProfileName;

	BOOL bNewXMLFile;
	CXMLExpFileElem* pExpElem = m_pXMLExpImpMng->GetXMLExpFileName(m_pDoc->GetNamespace().ToString(), strXMLExpFile, AfxGetParameters()->f_EnvPaddingNum, bNewXMLFile);
	if (bNewXMLFile)
		pExpElem->SetMaxDim(1, HEADER_MAX_DOC_DIMENSION);


	// chiedo all'exportimport manager il documento dom da scrivere
	// se non esiste lo istanzia
	CXMLDocumentObject* pXMLDomDoc = m_pXMLExpImpMng->GetXMLExportDomDocument(strUrlData, strXMLExpFile, (const CXMLProfileInfo*)pProfileInfo, TRUE, FALSE, FALSE, TRUE);
	
	BOOL bOk = ExportSmartDataSection(pXMLDomDoc->GetRoot(), pDBTMaster, pProfileInfo);
	
	if ( bOk && (m_nProcessStatus == XML_EXPORTING_DATA)) 
		m_nProcessStatus = XML_EXPORT_SUCCEEDED;

	//FINE ESPORTAZIONE DEL DOCUMENTO
	m_pDoc->OnAfterXMLExport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnAfterXMLExport();

	m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->InsertDocumentMessage(m_pDoc->GetMessages());
	m_pDoc->GetMessages()->ClearMessages(TRUE);
	// se ci sono dei messaggi (errori o warning) appendo il nodo CDiagnostic alla root del file
	// inoltre se sono presenti errori elimino  il nodo data
	if (m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->HasMessages())
	{
		// 28/06/2012 (Luca e Anna): si e' deciso di mantenere il nodo <Data> anche se ci sono errori
		// in modo da avere un xml di output per ulteriori verifiche.
		/*if (m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->HasErrors())
		{
			CXMLNode* pXMLNode = pXMLDomDoc->GetRootChildByName(DOC_XML_DATA_TAG);
			if (pXMLNode) pXMLDomDoc->RemoveNode(pXMLNode);
		}*/

		m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->AppendDiagnosticNode(pXMLDomDoc->GetRoot());
		m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->ClearMessages();
	}

	pExpElem->IncrementExpRecordCount();
	
	CString strValue;
	pXMLDomDoc->GetXML(strValue);
	m_pXMLSmartParam->AddToResult(strValue);
	
	return bOk;
#ifndef DEBUG
	}
	catch(CException *e)
	{
		AfxGetDiagnostic()->Add(e);
		e->Delete();
		return FALSE;
	}
	catch(...)
	{
		AfxGetDiagnostic()->Add((CException*) NULL);
		return FALSE;
	}
#endif
}

// nRet rappresenta il risultato della prima estrazione che pu?avvenire con query differenti
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExtractAndExportRecords(int nRet, CXMLProfileInfo* pProfileInfo, const CString& strErrMessage)
{		
	if (nRet == EXTRACT_RECORD_ERROR)
	{
		OutputMessage(cwsprintf(_TB("An error occured executing export criteria: {0-%s}"), strErrMessage),
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);		
		m_nProcessStatus = XML_EXPORT_FAILED;

		return FALSE;
	}
	
	if (nRet == EXTRACT_RECORD_NO_DATA)
	{
		OutputMessage(_TB("No record matches the set export criteria"), 
					  CXMLLogSpace::XML_WARNING, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		m_nProcessStatus = XML_EXPORT_FAILED;
		return FALSE;			
	}

	while (nRet == EXTRACT_RECORD_SUCCEEDED)
	{
		// catturo i messaggi del documento
		m_pDoc->m_pDBTMaster->SetNoPreloadStep();
		m_pDoc->BrowseRecord(FALSE, TRUE);
	
		if (!ExportSmartCurrentRecord(pProfileInfo))
			m_nProcessStatus = XML_EXPORT_FAILED;
		
		nRet = pProfileInfo->GetXMLExportCriteria()->GetNextRecord();		
	} 

	m_nProcessStatus = XML_EXPORT_SUCCEEDED;
	return TRUE;	
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartWithFindQuery(CXMLNode* pParamsNode, CXMLProfileInfo* pProfileInfo)
{
	if (!pProfileInfo || !pProfileInfo->GetXMLExportCriteria())
	{
		OutputMessage(_TB("No export criteria defined. Impossible export data."), 
			          CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}

	//<Parameters>
	//	<DefaultDialog>
	//		<DefaultGroup>
	//			<FindableField1> Value1 <FindableField1/>
	//			<FindableField2> Value2 <FindableField2/>
	//		<DefaultGroup/>
		//	<ExtRefNameGroup title = 'ExtRefName'> <!--Gestione campo di externale refernce--!>
	//			<FKSegmentField1> Value1 <FKSegmentField1/>
	//			<FKSegmentField2> Value2 <FKSegmentField2/>
	//		<ExtRefNameGroup/>
	//	<DefaultDialog/>
	//<Parameters/>

	if (pParamsNode)
	{
		SqlRecord* pMasterRec = m_pDoc->m_pDBTMaster->GetRecord();
		CXMLNode* pNode = NULL;
		CXMLNode* pFieldNode = NULL;
		CString strFieldName;
		CString strValue;

		//valorizzo i campi del dbt
		//scorro le dialog presenti. Possiamo avere:
		//- dialog di default per la valorizzazione dei campi che non definiscono external reference
		//- una dialog per ogni external reference
		for (int nChild = 0; nChild < pParamsNode->GetChildsNum(); nChild++)
		{
			pNode = pParamsNode->GetChildAt(nChild); //Dialog
			pNode = pNode->GetFirstChild(); //Group	
		
			for (int nField = 0; nField < pNode->GetChildsNum(); nField++)
			{
				pFieldNode = pNode->GetChildAt(nField);
				if (pFieldNode)
				{
					pFieldNode->GetBaseName(strFieldName);
					DataObj* pDataObj = pMasterRec->GetDataObjFromColumnName(strFieldName);
					if (pDataObj)
					{
						pFieldNode->GetText(strValue);
						pDataObj->AssignFromXMLString(strValue);
						pDataObj->SetValueLocked();
					}
				}
			}
		}
	}

	//estraggo i record che soddisfano i criteri di find
	CString strMessage;
	int nRet = pProfileInfo->GetXMLExportCriteria()->MakeFindableExportQuery(strMessage);

	//effettuo l'unlock dei campi lockati
	UnlockFields(m_pDoc->m_pDBTMaster);
	
	return ExtractAndExportRecords(nRet, pProfileInfo, strMessage);
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartWithExportCriteria(CXMLNode* pParamsNode, CXMLProfileInfo* pProfileInfo)
{
	if (!pProfileInfo || !pProfileInfo->GetXMLExportCriteria())
	{
		OutputMessage(_TB("No export criteria defined. Impossible export data."), 
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}
	
	CUserExportCriteria* pUserCrit =  (pProfileInfo && pProfileInfo->GetXMLExportCriteria() && pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()) 
		? pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria() : NULL;
	
	if (pUserCrit && pUserCrit->m_pQueryInfo && pUserCrit->m_pQueryInfo->m_pPrgData)
	{
		AskRuleData* pAskData = pUserCrit->m_pQueryInfo->m_pPrgData->GetAskRuleData();
        if (pAskData && pParamsNode)
		// carico i parametri presenti nel file 		
			pAskData->AssignFromXml(pParamsNode->GetParentNode());			
	}

	CPreferencesCriteria* pPreferences = pProfileInfo->GetXMLExportCriteria()->GetPreferencesCriteria();
	pPreferences->SetCriteriaModeApp(FALSE);
	pPreferences->SetCriteriaModeOSL(FALSE);
	pPreferences->SetCriteriaModeUsr(pUserCrit != NULL);

	// estraggo i record che soddisfano i criteri 
	CString strMessage;
	int nRet = pProfileInfo->GetXMLExportCriteria()->ExportQuery(strMessage);
	
	return ExtractAndExportRecords(nRet, pProfileInfo, strMessage);
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartSingleRecord(CXMLNode* pDBTNode, CXMLProfileInfo* pProfileInfo)
{
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)))
	{
		OutputMessage(_TB("Internal error in  ExportSmartSingleRecord procedure."), 
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}

	if (!pProfileInfo)
	{
		OutputMessage(_TB("No export criteria defined. Impossible export data."), 
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}

	if (pDBTNode)	
	{
		SqlRecord* pMasterRec = m_pDoc->m_pDBTMaster->GetRecord();
		ASSERT(pMasterRec);

		//pulisco il record 
		pMasterRec->Init();
		
		if (ImportSmartRecordFields(pDBTNode, m_pDoc->m_pDBTMaster, m_pDoc->m_pDBTMaster->GetRecord(), pProfileInfo))
		{
			UnlockFields(m_pDoc->m_pDBTMaster);
			m_pDoc->m_pDBTMaster->SetNoPreloadStep();
			m_pDoc->BrowseRecord(FALSE);
			return m_pDoc->ValidCurrentRecord() && ExportSmartCurrentRecord(pProfileInfo);
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartData(CXMLNode* pRoot, CXMLProfileInfo* pProfileInfo)
{
	//nel file di parametri passato possiamo avere i seguenti 4 casi:
	//
	// 1)nodo Data con figlio DBTMaster -> se utente mi ha passato come resultType = EXPORT_RESULT_ONE_RECORD
	//   eseguo query considerando come filtraggio i soli campi/valori passatomi altrimenti segnalo errore
	//
	// 2)nodo Parameters e nel profilo non sono definiti dei criteri personalizzati -> eseguo query composta da 
	//	 OnPrepareForXImportExport del DBTMaster + criterio di findable utilizzando i soli campi/valori passatomi
	//
	// 3)nodo Parameters e nel profilo sono definiti dei criteri personalizzati -> eseguo query composta da 
	//	 OnPrepareForXImportExport del DBTMaster + criterio personalizzato valorizzando i parametri con i valori 
	//   passati
	// 4) nessun nodo figlio: se utente mi ha passato come resultType = EXPORT_RESULT_MORE_RECORD eseguo la query di 
	//   OnPrepareForXImportExport del DBTMaster + se esiste il criterio personalizzato altrimenti segnalo errore

	if (((CSmartExportParams*)m_pXMLSmartParam)->m_bExportCurrentDocument)
			return ExportSmartCurrentRecord(pProfileInfo);

	// verifico punto 1)
	CXMLNode* pDBTNode = NULL;
	CXMLNode* pFindNode = pRoot->GetChildByName(DOC_XML_DATA_TAG);
	if (pFindNode)
		pDBTNode = pFindNode->SelectSingleNode(_T("*[@master]"), szNamespacePrefix);
	if (pDBTNode)
	{
		/*if (((CSmartExportParams*)m_pXMLSmartParam)->m_resultType == EXPORT_RESULT_MORE_RECORD)
		{
			OutputMessage(_TB("Requested result type (more documents) it is not compatible with selected search criteria (search only one document)."), 
						  CXMLLogSpace::XML_ERROR, 
						  IMPEXP_DIAGNOSTIC_CODE, 
						  szXTechDataManager);
			delete pDBTNode;
			return FALSE;
		}*/

		BOOL bOK = ExportSmartSingleRecord(pDBTNode, pProfileInfo);
		delete pDBTNode;
		return bOK;
	}

	// verifico punto 2) o punto 3)
	CXMLNode* pParamsNode = pRoot->GetChildByName(XML_PARAMETERS_TAG);
	if (pParamsNode)
	{
		if (pProfileInfo && !pProfileInfo->GetXMLExportCriteria())
			pProfileInfo->m_pExportCriteria = new CXMLExportCriteria(pProfileInfo, m_pDoc);
		CUserExportCriteria* pUserCrit =	
			(
				pProfileInfo && 
				pProfileInfo->GetXMLExportCriteria() && 
				pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()
			) 
			? pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()
			: NULL;

		BOOL bUserCriteriaFileExists	=	
			pUserCrit											&&
			!pProfileInfo->m_strUsrCriteriaFileName.IsEmpty()	&&
			ExistFile(pProfileInfo->m_strUsrCriteriaFileName)	&&
			((pUserCrit->m_pQueryInfo && !pUserCrit->m_pQueryInfo->m_TableInfo.m_strFilter.IsEmpty()) || pUserCrit->m_bOverrideDefaultQuery);

		BOOL bOK = TRUE;
		if (pUserCrit && bUserCriteriaFileExists)
			bOK = ExportSmartWithExportCriteria(pParamsNode, pProfileInfo);
		else 
			bOK = ExportSmartWithFindQuery(pParamsNode, pProfileInfo);
		return bOK;
	}

	// verifico punto 4)
	/*if (((CSmartExportParams*)m_pXMLSmartParam)->m_resultType == EXPORT_RESULT_ONE_RECORD)
	{
		OutputMessage(_TB("Requested result type (only one document) it is not compatible with selected search criteria (search more documents)."), 
					  CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}*/

	return ExportSmartWithExportCriteria(NULL, pProfileInfo);
}

//----------------------------------------------------------------------------
void CXMLDataManager::CreateExtRefParameters(SqlRecord* pRec, CXMLXRefInfoArray* pXRefInfoArray, CStringArray* pProcessedExtRefs, CXMLNode* pDlgNode)
{ 
	if (!pRec ||!pDlgNode)
		return;

	CXMLXRefInfo* pXRefInfo = NULL;
	CXMLSegmentInfo* pSegment = NULL;
	DataObj* pDataObj = NULL;
	CXMLNode* pFieldNode = NULL;

	//ciclo sugli external reference definiti per il campo
	for (int nRef = 0; nRef < pXRefInfoArray->GetSize(); nRef++)
	{
		pXRefInfo = pXRefInfoArray->GetAt(nRef);
		if (!pXRefInfo)
			continue;

		BOOL bContinue = FALSE;				
		// se ?un external-reference che ho gi?inserito non faccio niente
		for (int i = 0; i <= pProcessedExtRefs->GetUpperBound(); i++)
		{	
			if (pXRefInfo->GetName() == pProcessedExtRefs->GetAt(i))
			{
				bContinue = TRUE;
				break;
			}
		}
		if (bContinue) continue;

		pProcessedExtRefs->Add(pXRefInfo->GetName());
		if (
				pXRefInfo->IsToUse() && 
				(!pXRefInfo->GetXMLUniversalKeyGroup() || pXRefInfo->GetXMLUniversalKeyGroup()->IsExportData()) &&
				pXRefInfo->GetSegmentsNum() > 0)
		{
			//devo considerare il DBTMaster del documento referenziato
			CXMLDocInfo* pXMLDocInfo = new CXMLDocInfo(pXRefInfo->GetDocumentNamespace());
			CXMLDBTInfo* pXMLDBTMasterInfo = NULL;
			if (pXMLDocInfo && pXMLDocInfo->LoadAllFiles())
				pXMLDBTMasterInfo = pXMLDocInfo->GetDBTMaster();

			if (!pXMLDBTMasterInfo)
				continue;

			//NEW GROUP
			CXMLNode* pNode = pDlgNode->CreateNewChild(cwsprintf(_T("%sGroup"), pXRefInfo->GetName()));
			pNode->SetAttribute(XML_TITLE_ATTRIBUTE, pXRefInfo->GetName());
			pNode->SetAttribute(XML_NAME_ATTRIBUTE, (cwsprintf(_T("ns:%s/ns:%s"), pXRefInfo->GetName(), pXMLDBTMasterInfo->GetNamespace().GetObjectNameForTag())));
			for (int nSeg = 0; nSeg < pXRefInfo->GetSegmentsNum(); nSeg++)
			{
				
				pSegment = pXRefInfo->GetSegmentAt(nSeg);
				if (!pSegment || pSegment->GetFKSegment().IsEmpty())
					continue;
				pDataObj = pRec->GetDataObjFromColumnName(pSegment->GetFKSegment());
				if (!pDataObj)
					continue;
			
				pFieldNode =  pNode->CreateNewChild(pSegment->GetFKSegment());
				pFieldNode->SetText(pDataObj->FormatDataForXML(pDataObj->GetDataType()!= DATA_DATE_TYPE &&
															   (!pDataObj->IsKindOf(RUNTIME_CLASS(DataDbl)) ||
																(!(((CSmartExportParams*)m_pXMLSmartParam)->m_bUseApproximation)))));
				pFieldNode->SetAttribute(XML_TYPE_ATTRIBUTE,  ::FromTBTypeToNetType(pDataObj->GetDataType()));
			}
		}
	}
}

// creo il file xml contenente i soli parametri di ricerca valorizzati con il valore attuale
//---------------------------------------------------------------------------
void CXMLDataManager::ExportSmartOnlyParameters(CXMLDocumentObject* pXMLDoc, CXMLProfileInfo* pProfileInfo)
{ 
	if (!pXMLDoc || !pXMLDoc->GetRoot())
		return;

	// "forza" l'utilizzo del namespace alias (prefix) gi?presente sul documento passato da fuori
	pXMLDoc->SetNameSpaceURI(pXMLDoc->GetNamespaceURI(),pXMLDoc->GetPrefix());

	CXMLNode* pRoot = pXMLDoc->GetRoot();
	CUserExportCriteria* pUserCrit =  (pProfileInfo->GetXMLExportCriteria() && pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()) 
										? pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()
										: NULL;


	if (pUserCrit && pUserCrit->m_pQueryInfo && pUserCrit->m_pQueryInfo->m_pPrgData)
	{
		AskRuleData* pAskData = pUserCrit->m_pQueryInfo->m_pPrgData->GetAskRuleData();
        if (pAskData)
		// prima carico gli eventuali parametri gi?presenti nel file 		
		{
			CXMLNode* pParamsNode = pRoot->GetChildByName(XML_PARAMETERS_TAG);
			if (pParamsNode)
			{
				pAskData->AssignFromXml(pRoot);
				pRoot->RemoveChild(pParamsNode);
			}

			AskDialogInputMng aAskDialogInputMng
				(
					pAskData,
					pUserCrit->m_pQueryInfo->m_pPrgData->GetSymTable(),
					NULL
				);
			//eseguo le regole di richiesta in modo da avere i parametri attuali
			if (aAskDialogInputMng.ExecAskRules(NULL, TRUE))
				pAskData->GetXmlParameters(pRoot);
		}
	}
	else //considero i soli campi findable e chiave del documento
	{
		if (!m_pDoc->m_pDBTMaster)
			return;
		m_pDoc->m_pDBTMaster->Init();
	
		CXMLDBTInfo* pXMLDBTInfo = pProfileInfo->GetDBTFromNamespace(m_pDoc->m_pDBTMaster->GetNamespace());
		if (!pXMLDBTInfo)
			return;
		SqlRecord* pRec = m_pDoc->m_pDBTMaster->GetRecord();
		CXMLNode* pParamsNode = pRoot->GetChildByName(XML_PARAMETERS_TAG);
		if (!pParamsNode)
			pParamsNode = pRoot->CreateNewChild(XML_PARAMETERS_TAG);
		else if (pParamsNode->HasChildNodes())
		{
			int num =  pParamsNode->GetChildsNum();
			for (int i=0; i < num; i++)
				pParamsNode->RemoveChildAt(0); //it shifts down all the elements above the removed element
		}

		if (pParamsNode)
		{
			//<Parameters>
			//	<DefaultDialog>
			//		<DefaultGroup>
			//			<FindableField1> Value1 <FindableField1/>
			//			<FindableField2> Value2 <FindableField2/>
			//		<DefaultGroup/>
				//	<ExtRefNameGroup title = 'ExtRefName'> <!--Gestione campo di externale refernce--!>
			//			<FKSegmentField1> Value1 <FKSegmentField1/>
			//			<FKSegmentField2> Value2 <FKSegmentField2/>
			//		<ExtRefNameGroup/>
			//	<DefaultDialog/>
			//<Parameters/>

			//AL POSTO DELLA DIALOG
			CXMLNode* pDlgNode = pParamsNode->CreateNewChild(XML_DEFAULTDIALOG_TAG);
			pDlgNode->SetAttribute(XML_TITLE_ATTRIBUTE, _TB("Fields search"));
			//AL POSTO DEL GRUPPO
			CXMLNode*  pGrpNode = pDlgNode->CreateNewChild(XML_DEFAULTGROUP_TAG);
			pGrpNode->SetAttribute(XML_TITLE_ATTRIBUTE,_TB("Search group"));

			CXMLNode* pFieldNode = NULL;
			CXMLXRefInfoArray aXMLXRefInfoArray;
			CStringArray aProcessedExtRefs;

			aXMLXRefInfoArray.SetOwns(FALSE);
				
			if (pRec && pRec->GetSize() > 0)
			{
				for (int j = 0; j <= pRec->GetUpperBound(); j++)
				{
					const SqlColumnInfo* pColumnInfo = pRec->GetColumnInfo(j);
					DataObj* pDataObj = pRec->GetDataObjAt(j);
					CString strColumnName = pColumnInfo->GetColumnName();
					if ((pDataObj->IsFindable() || pColumnInfo->m_bSpecial) && pXMLDBTInfo->IsFieldToExport(strColumnName)) 
					{
						aXMLXRefInfoArray.RemoveAll();

						if (pXMLDBTInfo->m_pXRefsArray)
							pXMLDBTInfo->m_pXRefsArray->GetXRefArrayByFK((LPCTSTR)strColumnName, &aXMLXRefInfoArray);
						
						if (aXMLXRefInfoArray.GetSize() <= 0)
						{
							pFieldNode =  pGrpNode->CreateNewChild(pColumnInfo->GetColumnName());
							pFieldNode->SetText(pDataObj->FormatDataForXML(pDataObj->GetDataType() != DATA_DATE_TYPE));
							pFieldNode->SetAttribute(XML_TYPE_ATTRIBUTE,  ::FromTBTypeToNetType(pDataObj->GetDataType()));							
						}
						else
							CreateExtRefParameters(pRec, &aXMLXRefInfoArray, &aProcessedExtRefs, pDlgNode);
					}
				}
			}
		}
	}

	CString strValue;
	pXMLDoc->GetXML(strValue);
	m_pXMLSmartParam->AddToResult(strValue);
}

//----------------------------------------------------------------------------
void CXMLDataManager::InitializeExportSmart()
{
	if (m_pExportDocSelection)
		delete m_pExportDocSelection;

	m_pExportDocSelection = new CXMLExportDocSelection(m_pDoc);

	ASSERT(m_pExportDocSelection);
	m_pExportDocSelection->m_nDocSelType = (((CSmartExportParams*)m_pXMLSmartParam)->m_loadAction == EXPORT_RESULT_MORE_RECORD) ? EXPORT_DOC_SET : EXPORT_ONLY_CURR_DOC;
	
	m_pExportDocSelection->m_nProfileSelType = USE_SELECTED_PROFILE;

	CreateXMLExpImpManager();	
	// non utilizzo il logging di XTech ma gestisco la diagnostica mediante la classe CSmartXMLDiagnosticMng
	m_pXMLExpImpMng->UseSmartXMLDiagnosticMng();
}

// a partire da una stringa xml contenente i campi di ricerca carico il documento 
// e lo esporto
//----------------------------------------------------------------------------
BOOL CXMLDataManager::ExportSmartDocument(CXMLDocumentObject* pXMLParamDoc, CSmartExportParams* pSmartExpParams)
{
	if (!pXMLParamDoc)
		return FALSE;

	CXMLNode* pRoot = pXMLParamDoc->GetRoot();
	if (!pRoot)
		return FALSE;

	//cerco Namespace del documento e il nome del profilo
	CString strDocNamespace;
	CString strProfile;

	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	m_pXMLSmartParam = pSmartExpParams;
	m_bIsRootDoc = TRUE;	

	// controllo che il namespace presente nella root del file sia uguale a quello associato al corrente documento
	// e che esista il profilo 
	if (!pRoot->GetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, strDocNamespace) ||
		GetDocumentNamespace().ToString().CompareNoCase(strDocNamespace) != 0 )
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("TBNamespace is not equal to istanziated document namespace. Impossible to provide requested data"));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	InitializeExportSmart();	

	GetInformationFromNsUri(pXMLParamDoc->GetNamespaceURI(), strProfile,  m_pXMLSmartParam->m_ePosType, m_pXMLSmartParam->m_strUserName);

	if (strProfile.IsEmpty() || !pRoot->GetAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE, strProfile))
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("No export profile name found. Impossible to provide requested data"));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	// se il profilo non esiste segnalo errore, perch?se utilizzo la descrizione del documento (come in XTech standard) rischio
	// di creare un file di dati non compatibile con lo schema mappato in office2003
	//devo inserire l'URI per il profilo utilizzato
	CString strProfilePath = ::GetProfilePath(m_pDoc->GetNamespace(), strProfile, m_pXMLSmartParam->m_ePosType, m_pXMLSmartParam->m_strUserName);
	if (!LoadSmartProfile(strProfilePath))
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, cwsprintf(_TB("Export profile {0-%s} not present in the current installation."), (strProfile.IsEmpty()) ? m_pDoc->GetNamespace().GetObjectName() : strProfile));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	};

	BOOL bOK = TRUE;

	m_nStatus = XML_MNG_EXPORTING_DATA;
	m_nProcessStatus = XML_EXPORTING_DATA;
	
	//Se siamo già in unattended mode non faccio niente, altrimenti lo imposto a true
	BOOL bOldUnattendedValue = m_pDoc->SetInUnattendedMode();

	if (pSmartExpParams->m_loadAction == EXPORT_ACTION_ONLY_PARAMS)
		ExportSmartOnlyParameters(pXMLParamDoc, m_pSmartProfile);
	else
		bOK = ExportSmartData(pRoot, m_pSmartProfile);

	m_nStatus = XML_MNG_IDLE;
	
	//Se non eravamo in unattended mode prima della procedura, riporto lo stato a false
	if (!bOldUnattendedValue)
		m_pDoc->SetInUnattendedMode(bOldUnattendedValue);

	// se ci sono dei messaggi (errori o warning) aggiungendoli all'XML di esportazione questo pu?avvenire
	// se ci sono stati errori prima di riuscire ad esportare un record, infatti nel caso di errori o warning durante
	// l'esportazione le informazioni di diagnostica sono inserite dentro il file
	if (m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->HasMessages())
		m_pXMLSmartParam->AddToResult(m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
	
	SAFE_DELETE(m_pXMLExpImpMng);	
	return bOK;
}


//----------------------------------------------------------------------------------------------------
// Metodi utilizzati per l'importazione dei dati provenienti dall'integrazione con Office2003
//----------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartExternalReference(CXMLNode* pExtRefNode, CXMLXRefInfo* pXRefInfo)
{
	if (!pXRefInfo || !pExtRefNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	// search the DBTMaster node
	CXMLNode* pDBTNode = pExtRefNode->SelectSingleNode(_T("*[@master]"), szNamespacePrefix);
	if (!pDBTNode)
		return TRUE;


	//prima verifico la presenza di tutti i segmenti di pk del documento referenziato
	// e che il loro valore sia != stringa vuota
	// se non ho tutti i segmenti non proseguo
	for (int nField = 0; nField < pXRefInfo->m_SegmentsArray.GetSize(); nField++)
	{
		CXMLSegmentInfo* pSegmInfo = pXRefInfo->m_SegmentsArray.GetAt(nField);
		if (pSegmInfo)
		{		
			CXMLNode* pSegNode = pDBTNode->GetChildByName(pSegmInfo->GetReferencedSegment(), FALSE);
			CString strValue;
			if (!pSegNode || (pSegNode->GetText(strValue) && strValue.IsEmpty()))
			{
				SAFE_DELETE(pDBTNode);
				return TRUE;
			}			
		}
	}
	//Impr. 6393: istanziazione senza interfaccia grafica
	//ho anticipato la crezione del profilo dell'external reference per verificare se il profilo ammette la run del documento senza interfaccia
	//check for existing profile
	CString strProfilePath = ::GetProfilePath(pXRefInfo->GetDocumentNamespace(), pXRefInfo->GetProfile(), m_pXMLSmartParam->m_ePosType, m_pXMLSmartParam->m_strUserName, FALSE, TRUE);
	if (!ExistProfile(strProfilePath))
	{
		OutputMessage(cwsprintf
		(
			_TB("The export profile {0-%s} used by the external reference {1-%s} doesn't exist for the user {2-%s}."),
			pXRefInfo->GetProfile(),
			pXRefInfo->GetName(),
			m_pXMLSmartParam->m_strUserName
		),
			CXMLLogSpace::XML_ERROR,
			IMPEXP_DIAGNOSTIC_CODE,
			szXTechDataManager);
		ASSERT(FALSE);
		SAFE_DELETE(pDBTNode);
		return FALSE;
	}

	CXMLProfileInfo* pExtProfileInfo = GetXMLProfileInfo(NULL, strProfilePath, pXRefInfo->GetDocumentNamespace().ToString());
	//CXMLProfileInfo* pExtProfileInfo = new CXMLProfileInfo(pXRefInfo->GetDocumentNamespace(), strProfilePath);
	pExtProfileInfo->LoadAllFiles();


	CXMLDocElement* pOldDocElement = m_pXMLExpImpMng->GetCurrentDocElem();

	CXMLDocElement* pDocElement = m_pXMLExpImpMng->GetXMLDocElement(pXRefInfo->GetDocumentNamespace(), m_pDoc, pExtProfileInfo->CanRunOnlyBusinessObject());
	
	if (!pDocElement)
	{
		OutputMessage(cwsprintf(_TB("Invalid document NameSpace: {0-%s}."), pXRefInfo->GetDocumentNamespace().ToString()), 
				      CXMLLogSpace::XML_ERROR, 
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		SAFE_DELETE(pDBTNode);
		return FALSE;
	}

	m_pXMLExpImpMng->SetCurrentDocElem(pDocElement);
	
	CAbstractFormDoc* pExtRefDoc = pDocElement->m_pDocument;

	if (!pExtRefDoc)
	{
		OutputMessage(
						cwsprintf(
							_TB("Error importing external reference of the document with namespace '{0-%s}'"),  
							pXRefInfo->GetDocumentNamespace().ToString()
							), 
						CXMLLogSpace::XML_ERROR, 
						IMPEXP_DIAGNOSTIC_CODE, 
						szXTechDataManager 
					);
		SAFE_DELETE(pDBTNode);
		return FALSE;
	}
	
	CXMLDataManagerObj* pExtRefXMLDataMng = pExtRefDoc->GetXMLDataManager();
	if (!pExtRefXMLDataMng || !pExtRefXMLDataMng->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
	{
		OutputMessage(_TB("Internal error in ImportSmartExternalReference procedure."), 
						CXMLLogSpace::XML_ERROR, 
						IMPEXP_DIAGNOSTIC_CODE, 
						szXTechDataManager);
		ASSERT(FALSE);
		SAFE_DELETE(pDBTNode);
		return FALSE;
	}

	
		
	//import the external reference only if it is postable 
	if (!pExtProfileInfo->IsPostable())
	{
		OutputMessage(cwsprintf
				(
					_TB("The external reference {0-%s} with export profile {1-%s} it is not postable. So it will not be updated."), 
					pXRefInfo->GetName(),
					pXRefInfo->GetProfile()
				),
				CXMLLogSpace::XML_INFO, 
				0, 
				szXTechDataManager);
		SAFE_DELETE(pExtProfileInfo);
		SAFE_DELETE(pDBTNode);
		return TRUE;
	}

	BOOL bRetVal = ((CXMLDataManager*)pExtRefXMLDataMng)->ImportSmartCurrentRecord(pExtRefNode, pExtProfileInfo, IMPORT_ACTION_INSERT_UPDATE);
	m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->InsertDocumentMessage(pExtRefDoc->GetMessages());
	pExtRefDoc->GetMessages()->ClearMessages();
	m_pXMLExpImpMng->SetCurrentDocElem(pOldDocElement);
	SAFE_DELETE(pExtProfileInfo);
	SAFE_DELETE(pDBTNode);
	return bRetVal;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartDBTExtReferences(CXMLNode*	pDBTNode, CXMLDBTInfo* pDBTInfo)
{
	CXMLNodeChildsList* pExtRefList = pDBTNode->SelectNodes(_T("*[@externalReference]"), szNamespacePrefix);

	if (!pExtRefList)
		return TRUE;

	BOOL bOk = TRUE;
	CXMLNode* pExtRef = NULL;
	CString strXRefName;
	for (int nChild = 0; nChild < pExtRefList->GetCount(); nChild++)
	{
		pExtRef = pExtRefList->GetAt(nChild);
		if (!pExtRef)
			continue;
		
		pExtRef->GetBaseName(strXRefName);
		if (!strXRefName.IsEmpty())
		{
			CXMLXRefInfo* pXRefInfo = pDBTInfo->GetXRefByName(strXRefName);
			bOk = ImportSmartExternalReference(pExtRef, pXRefInfo) && bOk;
		}
	}	
	
	// l'array di nodi ritornato dalla SelectNodes ?una copia di quello presente nell'xmldom, per cui va cancellato
	delete pExtRefList;

	return bOk;
}

//devo importare l'external-reference
// A differenza dell'importazione standard di XTech, l'importazione Smart prevede il solo inserimento
// del record di ext-ref e non la sua modifica. Inoltre i valori assegnati ai segmenti di primarykey sono 
// utilizzati per valorizzare i segmenti di foreignkey; se bGetOnlyFKValue= TRUE eseguo solo questa azione
//----------------------------------------------------------------------------
BOOL CXMLDataManager::GetForeingnKeysValue
							(
								CXMLNode*			pExtRefNode,
								DBTObject*			pDBT,
								SqlRecord*			pRecord,
								CXMLProfileInfo*	pProfileInfo,
								BOOL				bLock
							) 
{
	// utilizzo gli attributi dell'extref per determinare il link primarykey-foreignkey in modo
	// da valorizzare la fk a partire dal valore presente nella pk
	//esempio
	/*<Clienti externalreference = "true">
		<Testa>
			<Mag>1</Mag> 
			<Area>2</Area> 
			<Cliente>1345</Cliente> 
			<Nome>1345</Nome> 
			<Cognome>1345</Cognome> 
		</Testa>
	</Clienti>*/
	
	CXMLDBTInfo* pDBTInfo = pProfileInfo->GetDBTFromNamespace(pDBT->GetNamespace());
	if (!pDBTInfo)
		return FALSE;
	CString strRefName;
	pExtRefNode->GetBaseName(strRefName);
	CXMLXRefInfo* pXRefInfo = pDBTInfo->GetXRefByName(strRefName);
	if (!pXRefInfo)
		return FALSE;
	CString fkName, pkName, strValue;
	DataObj* pDataObj = NULL;
	CXMLNode* pPkNode = NULL;

	CXMLNode* pDBTNode = pExtRefNode->SelectSingleNode(_T("*[@master]"), szNamespacePrefix);
	if (!pDBTNode)
		return FALSE;

	for (int nSeg = 0; nSeg < pXRefInfo->GetSegmentsNum(); nSeg++)
	{
		CXMLSegmentInfo* pSegment = pXRefInfo->GetSegmentAt(nSeg);
		if (!pSegment)
			continue;		
		pDataObj = pRecord->GetDataObjFromColumnName(pSegment->GetFKSegment());
		pPkNode = pDBTNode->GetChildByName(pSegment->GetReferencedSegment(), FALSE);
		if (pDataObj && pPkNode)
		{
			pPkNode->GetText(strValue);
			pDataObj->AssignFromXMLString((LPCTSTR)strValue);
			if (bLock)
				pDataObj->SetValueLocked();
		}
	}
	// il nodo ritornato dalla SelectSingleNode ?una copia di quello presente nell'xmldom, per cui va cancellato
	delete pDBTNode;
	return TRUE;
}


//----------------------------------------------------------------------------
int CXMLDataManager::ImportSmartRecordFields
							(
								CXMLNode*			pRecordNode,
								DBTObject*			pDBT, 
								SqlRecord*			pRecord,
								CXMLProfileInfo*	pProfileInfo,
								BOOL				bLock
							) 
{
	if (!pRecordNode || !pDBT)
	{
		OutputMessage(_TB("Internal error in Errore interno della procedura ImportSmartRecordFields. procedure."), 
						CXMLLogSpace::XML_ERROR, 
						IMPEXP_DIAGNOSTIC_CODE, 
						szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}

	if (pRecordNode->GetChildsNum() == 0)
	{
		pRecord->Init();
		return TRUE;
	}

	CString strIsExtRef;
	CXMLNode* pChildNode = pRecordNode->GetFirstChild();
	while (pChildNode)
	{
		//controllo se sto importando un external-reference
		if (
				pChildNode->GetAttribute(DOC_XML_EXTREF_ATTRIBUTE, strIsExtRef) &&
				GetBoolFromXML(strIsExtRef)
			)
		{
			GetForeingnKeysValue(pChildNode, pDBT, pRecord, pProfileInfo, bLock);
			pChildNode = pRecordNode->GetNextChild();
			continue;
		}
	
		CString strFieldName;
		pChildNode->GetBaseName(strFieldName);

		int nImportFldRc = ImportRecordField(pChildNode, pRecord, strFieldName);
		
		if (nImportFldRc == IMPORT_RECFLD_ERROR_FIELD_NOTFOUND)
			OutputMessage(cwsprintf(_TB("Error importing the field '{0-%s}' in the table '{1-%s}'."), (LPCTSTR)strFieldName, (LPCTSTR)pRecord->GetTableName()), 
						  CXMLLogSpace::XML_WARNING,
						  IMPEXP_DIAGNOSTIC_CODE, 
						  szXTechDataManager);

		pChildNode = pRecordNode->GetNextChild();
	}
			
	return TRUE;
}


//----------------------------------------------------------------------------
void CXMLDataManager::GetActionDescription(int importAction, CString& strAction, CString& strDocumentStatus)
{
	switch(importAction)
	{
		case IMPORT_ACTION_INSERT_UPDATE:
			strAction = _TB("insert or update"); strDocumentStatus = _TB("OnOkXMLUpdateImport returned false"); break;
		case IMPORT_ACTION_ONLY_INSERT:
			strAction = _TB("only insert"); strDocumentStatus = _TB("already present"); break;
		case IMPORT_ACTION_ONLY_UPDATE:
			strAction = _TB("only update"); strDocumentStatus = _TB("not exist"); break;
		case IMPORT_ACTION_DELETE:
			strAction = _TB("delete"); strDocumentStatus = _TB("not exist"); break;
	}
}

//----------------------------------------------------------------------------
void CXMLDataManager::LockedIndexes(CUIntArray* pLockedObjsIndexes, BOOL bLock, SqlRecord* pNewMasterRec, SqlRecord* pTmpRec)
{
	if (bLock)
	{
		for (int i = 0; i <= pNewMasterRec->GetUpperBound(); i++)
		{
			DataObj *pCurrDataObj = pNewMasterRec->GetDataObjAt(i);
			if (pCurrDataObj->IsValueLocked())
			{
				pCurrDataObj->SetValueLocked (FALSE);
				pLockedObjsIndexes->Add(i);
			}
		}
	}
	else
	{
		// devo fare cos?perch?ho locckato i valori con l'assegnazione da XML
		// riassegno solo i campi che ho importato da xml (ossia quelli locckati)
		for (int i = 0; i <= pLockedObjsIndexes->GetUpperBound(); i++)
		{
			UINT index = pLockedObjsIndexes->GetAt(i);
			*pNewMasterRec->GetAt(index) = *pTmpRec->GetAt(index);
			pNewMasterRec->GetDataObjAt(index)->SetValueLocked (TRUE);
		}
	}
}

//----------------------------------------------------------------------------
CXMLDBTInfo::UpdateType CXMLDataManager::GetDBTSlaveUpdateType(DBTSlave* pDBTSlave, CString strUpdateType)
{
	CXMLDBTInfo::UpdateType eUpdateType;

	//Inizializzo l'updateType a REPLACE se è uno slavebuffered oppure a INSERT_UPDATE se è slave
	if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		eUpdateType = CXMLDBTInfo::REPLACE;
	else
		eUpdateType = CXMLDBTInfo::INSERT_UPDATE;

	//Cerco comunque l'informazione relativa all'updateType nell'xmlDbtInfo e l'aggiorno, se non c'è ASSERT 
	//vuol dire che la descrizione di documento è da rigenerare
	CXMLDBTInfo* pInfo = pDBTSlave->GetXMLDBTInfo();
	ASSERT(pInfo);
	if (pInfo)
		eUpdateType = pInfo->GetUpdateTypeFromString(strUpdateType);	

	return eUpdateType;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartSingleDBTSlave(CXMLNode* pSlaveNode, DBTSlave* pDBTSlave, CXMLProfileInfo* pProfileInfo)
{
	CString strUpdateType;
	pSlaveNode->GetAttribute(DOC_XML_UPDATE_ATTRIBUTE, strUpdateType);
	
	//Recupero l'update type o dall'XMLDBTInfo oppure un default dal tipo di slave
	CXMLDBTInfo::UpdateType eUpdateType = GetDBTSlaveUpdateType(pDBTSlave, strUpdateType);

	// Slave Buffered
	if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		int nRowsNum = pSlaveNode->GetChildsNum();
		int nProcessedSlaveBuffRows = 0;
	
		//prima di operare sulle righe
		pDBTSlave->OnBeforeXMLImport ();
	
		// cancello gli elementi esistenti				
		if (eUpdateType == CXMLDBTInfo::REPLACE)
			((DBTSlaveBuffered*)pDBTSlave)->RemoveAll();

		for (CXMLNode* pRowNode = pSlaveNode->GetFirstChild(); pRowNode!=NULL; pRowNode = pSlaveNode->GetNextChild())
		{
			if (pRowNode->IsBaseNamed(pDBTSlave->GetNamespace().GetObjectNameForTag() + XML_ROW_TAG))
			{
				DataObjArray keys;			
				int nCurrentRow = -1;
				SqlRecord* pSlaveRec = GetCurrentRecord(keys, (DBTSlaveBuffered*)pDBTSlave, eUpdateType, pRowNode, nCurrentRow);

				if (pSlaveRec)
				{
					//prima della valorizzazione dei campi
					((DBTSlaveBuffered*)pDBTSlave)->OnBeforeXMLImport (nCurrentRow);
			
					((DBTSlaveBuffered*)pDBTSlave)->SetCurrentRow (nCurrentRow);

					if (!ImportSmartRecordFields(pRowNode, pDBTSlave, pSlaveRec, pProfileInfo, TRUE))
						return FALSE;
				
					nProcessedSlaveBuffRows++;

					if (!((DBTSlaveBuffered*)pDBTSlave)->OnOkXMLImport(nCurrentRow))
					{
						// il programmatore ha invalidato l'importazione della singola riga: passo 
						// alla successiva (se l'importazione deve fallire per questo motivo, 
						// sar?cura dello stesso programmatore stabilirlo nella altre OnOkXMLImport
						// - di DBT Slave o di Documento)
						continue; 
					}

					//Mi occupo degli external-reference del singolo dbt slave
					if (!m_bIsExtRef && !ImportSmartDBTExtReferences(pRowNode, pProfileInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace())))
					{
						OutputMessage(cwsprintf(_TB("Error importing external references of the row {0-%d}  in dbt {1-%s}"), nProcessedSlaveBuffRows, pDBTSlave->GetNamespace().GetObjectName()),
									  CXMLLogSpace::XML_ERROR,
									  IMPEXP_DIAGNOSTIC_CODE, 
									  szXTechDataManager);
						return FALSE;
					}
			
					//dopo la valorizzazione dei campi
					((DBTSlaveBuffered*)pDBTSlave)->OnAfterXMLImport(nCurrentRow);

					// records events
					FireDBTSlaveRecEvents(((DBTSlaveBuffered*)pDBTSlave)->GetRecord(), pSlaveRec, pRowNode);

					//verifico che il programmatore non abbia distrutto il record
					ASSERT(AfxIsValidAddress(pSlaveRec, sizeof(SqlRecord)));

					pSlaveRec->SetStorable();
				
					//rilascio il lock sui campi cha dovranno essere riallineati al master
					for(int i =0; i<keys.GetSize (); i++)
						keys.GetAt (i)->SetValueLocked (FALSE);							
				}
			}
		}
		ASSERT(nProcessedSlaveBuffRows == nRowsNum);
	}
	else
	{
		// Slave
		if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlave)))
		{
			SqlRecord* pSlaveRec = pDBTSlave->GetRecord();
			if (pSlaveRec)
			{
				if (eUpdateType == CXMLDBTInfo::REPLACE)
					pSlaveRec->Init(); 
				else
					if (eUpdateType == CXMLDBTInfo::ONLY_INSERT && !pSlaveRec->IsEmpty())
						return TRUE;

				DataObjArray keys;
				GetForeignKeySegments(keys, pSlaveRec, pDBTSlave);
						
				//prima della valorizzazione dei campi
				pDBTSlave->OnBeforeXMLImport ();
				
				if (!ImportSmartRecordFields(pSlaveNode, pDBTSlave, pSlaveRec, pProfileInfo, TRUE))
					return FALSE;
				
				//importo i documenti referenziati
				if (!m_bIsExtRef && !ImportSmartDBTExtReferences(pSlaveNode, pProfileInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace())))
				{
					OutputMessage(cwsprintf(_TB("Error importing external references in dbt {0-%s}"), pDBTSlave->GetNamespace().GetObjectName()),
								  CXMLLogSpace::XML_ERROR,
								  IMPEXP_DIAGNOSTIC_CODE, 
								  szXTechDataManager);
					return FALSE;
				}
				
				
				if (!pDBTSlave->OnOkXMLImport())
					return FALSE;
				
				//dopo la valorizzazione dei campi
				pDBTSlave->OnAfterXMLImport ();				
				
				// records events
				FireRecEvents(pSlaveRec, pSlaveNode);
				
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
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartDBTSlaves(CXMLNode* pDocumentNode, CXMLProfileInfo* pProfileInfo)
{
	if (!pDocumentNode || !pProfileInfo)
	{
		OutputMessage(_TB("Internal error in ImportSmartDBTSlaves procedure."),
					  CXMLLogSpace::XML_ERROR,
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);
		ASSERT(FALSE);
		return FALSE;
	}
	DBTSlave* pSlave = NULL;
	CXMLNode* pDBTNode = NULL;
	//per ciascuno slave esportabile secondo il profiloconsidero l'eventuale nodo presente nell'XML
	for (int nDBT = 0; nDBT <= m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetUpperBound(); nDBT++)
	{
		pSlave = m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetAt(nDBT);
		if (pSlave)
		{
			CXMLDBTInfo* pDBTInfo = pProfileInfo->GetDBTFromNamespace(pSlave->GetNamespace());
			if (pDBTInfo && pDBTInfo->IsToExport())
			{
				pDBTNode = pDocumentNode->GetChildByName(pSlave->GetNamespace().GetObjectNameForTag(), FALSE);
				if (pDBTNode && !ImportSmartSingleDBTSlave(pDBTNode, pSlave, pProfileInfo))
					return FALSE;
			}
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::EscapeDBTMaster(SqlRecord* pTmpRec)
{
	if (pTmpRec) delete pTmpRec;
	UnlockFields(m_pDoc->m_pDBTMaster);
	m_bBusy = FALSE;
	if (m_pDoc->GetFormMode() != CAbstractFormDoc::BROWSE)
		m_pDoc->EscapeImportDocument();
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::PreparePKForSmartImport()
{		
	BOOL retValue = TRUE;
	// se tutti i campi della chiave primaria sono lockati significa che sono stati valorizzati con i valori
	// presenti nell'xml, altrimenti ho un chiave primaria con segmenti vuoti

	SqlRecord* pNewMasterRec = m_pDoc->m_pDBTMaster->GetRecord();
	DataObjArray arPKSegments;
	arPKSegments.SetOwns(FALSE);
	pNewMasterRec->GetKeyStream(arPKSegments,FALSE);
	BOOL bPrepare = FALSE;
	for (int nIdx = 0; nIdx <= arPKSegments.GetUpperBound(); nIdx++)
	{
		DataObj* pSegment = arPKSegments.GetAt(nIdx);
		if (pSegment && pSegment->IsEmpty() && pSegment->GetDataType() != DATA_ENUM_TYPE && pSegment->GetDataType() != DATA_BOOL_TYPE && !pNewMasterRec->IsAutoIncrement(pSegment))
		{
			bPrepare = TRUE;
			break;
		}
	}
	if (bPrepare)
	{
		// devo preparare la chiave, tolgo l'eventuale lock dai segmenti lockati
		for (int nIdx = 0; nIdx <= arPKSegments.GetUpperBound(); nIdx++)
		{
			DataObj* pSegment = arPKSegments.GetAt(nIdx);
			if (pSegment)
				pSegment->SetValueLocked(FALSE); 
		}

		// chiedo al documento di preparare la chiave primaria automatica (se ne ?capace)
		BOOL retValue = m_pDoc->OnPreparePKForXMLImport();

		// effettuo nuovamente il lock della chiave
		for (int nIdx = 0; nIdx <= arPKSegments.GetUpperBound(); nIdx++)
		{
			DataObj* pSegment = arPKSegments.GetAt(nIdx);
			if (pSegment && !pNewMasterRec->IsAutoIncrement(pSegment))
				pSegment->SetValueLocked();
		}
		// se il documento ha assegnato un counter, lo blocco per tenermelo per la procedura di import
		// questo per non farlo scattare due volte
		DataObj* pDataObj = m_pDoc->GetAssignedCounter();
		if (pDataObj)
			pDataObj->SetValueLocked();
	}
	return retValue;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartDBTMaster(CXMLNode* pDBTNode, CXMLProfileInfo*	pProfileInfo, SqlTable* pTableToRelock, int importAction)
{
	if (!pDBTNode || !pProfileInfo)
	{
		OutputMessage(_TB("Internal error in ImportSmartDBTMaster procedure."),
					  CXMLLogSpace::XML_ERROR,
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager);

		return FALSE;
	}

	DBTMaster* pDBTMaster =  m_pDoc->m_pDBTMaster;
	SqlRecord* pNewMasterRec = pDBTMaster->GetRecord();
	ASSERT(pNewMasterRec);

	if (importAction == IMPORT_ACTION_DELETE)
	{
		//carico le sole informazioni della chiave di documento
		BOOL bOk = TRUE;
		for (int nIdx = 0; nIdx < pNewMasterRec->GetUpperBound(); nIdx++)
		{
			SqlRecordItem* pItem = pNewMasterRec->GetAt(nIdx);
			CString strFieldName;
			if (pItem && pItem->IsSpecial())
			{
				CXMLNode* pChildNode = pDBTNode->GetChildByName(pItem->GetColumnName(), FALSE);
				if (pChildNode)
				{
					strFieldName =  pItem->GetColumnName();
					ImportRecordField(pChildNode, pNewMasterRec, strFieldName);
				}
				else
				{
					OutputMessage( cwsprintf(
								_TB("It is not possible to save the document {0-%s}. It has primary key's segments with null values."),
								(LPCTSTR)m_pDoc->GetTitle()), 
								CXMLLogSpace::XML_ERROR,
								IMPEXP_DIAGNOSTIC_CODE, 
								szXTechDataManager
							);
					UnlockFields (pDBTMaster);
					bOk = FALSE;
				}
			}
		}		
		bOk = bOk && DeleteDocument(pDBTMaster, pNewMasterRec);
		m_bBusy = FALSE;
		return bOk;
	}

	//carico le informazioni del master in modo da poter caricare il documento
	if (!ImportSmartRecordFields(pDBTNode, pDBTMaster, pDBTMaster->GetRecord(), pProfileInfo, TRUE))
	{
		m_bBusy = FALSE;
		return FALSE;
	}
		
	// se sono in stato di insert di nuovo documento, devo controllare i campi di chiave primaria
	// se ne ho almeno uno vuoto devo unlockarli e chiamare il metodo OnPreparePKForXMLImport del documento 
	if ((importAction == IMPORT_ACTION_ONLY_INSERT || importAction == IMPORT_ACTION_INSERT_UPDATE) && !PreparePKForSmartImport())
	{
		OutputMessage( cwsprintf(
								_TB("It is not possible to save the document {0-%s}. It has primary key's segments with null values."),
								(LPCTSTR)m_pDoc->GetTitle()), 
								CXMLLogSpace::XML_ERROR,
								IMPEXP_DIAGNOSTIC_CODE, 
								szXTechDataManager
					);
		return FALSE;
	}
	
	
	
	BOOL bOk = FALSE;
	if (!pDBTMaster->OnOkXMLImport())
	{
		UnlockFields (pDBTMaster);
		return FALSE;
	}

	//dopo la valorizzazione dei campi
	pDBTMaster->OnAfterXMLImport();
	
	// Salvo i dati caricati in un record temporaneo
	SqlRecord* pTmpRec = pNewMasterRec->Create();
	ASSERT(pTmpRec);
	*pTmpRec = *pNewMasterRec;
			
	if (importAction == IMPORT_ACTION_ONLY_UPDATE)
	{
		const CUIntArray& aIndexes = pNewMasterRec->GetPrimaryKeyIndexes();
		for (int i = 0; i <= aIndexes.GetUpperBound(); i++)
		{
			UINT index = aIndexes[i];
			if (pNewMasterRec->GetDataObjAt(index) && !pNewMasterRec->GetDataObjAt(index)->IsValueLocked())
			{
				OutputMessage( cwsprintf(
								_TB("It is not possibile update document: {0-%s}; The key has empty values"),
								(LPCTSTR)m_pDoc->GetTitle()
								), 
					   CXMLLogSpace::XML_ERROR,
					   IMPEXP_DIAGNOSTIC_CODE, 
					   szXTechDataManager
					  );
				EscapeDBTMaster(pTmpRec);		
				return FALSE;
			}
		}
	}
	
	// controlloil tipo di azione scelto: solo inserimento, solo modifica, modifica o inserimento
	BOOL bFind = pDBTMaster->FindData(FALSE);
	if ( 
		 (bFind && (importAction == IMPORT_ACTION_ONLY_INSERT || !m_pDoc->OnOkXMLUpdateImport())) || 
		 (!bFind && importAction == IMPORT_ACTION_ONLY_UPDATE)
		)
	{
		//se è un'external reference non devo bloccare l'importazione. Semplicemente faccio skip del documento
		if (m_bIsExtRef)
		{
			EscapeDBTMaster(pTmpRec);		
			return TRUE;
		}

		CString strAction, strDocumentStatus;
		GetActionDescription(importAction, strAction, strDocumentStatus);
		OutputMessage( cwsprintf(
								_TB("It is no possibile to execute  {0-%s} action. Document: {1-%s}; Keys: {2-%s} {3-%s}"),
								strAction,
								(LPCTSTR)m_pDoc->GetTitle(),
								(LPCTSTR)pNewMasterRec->GetPrimaryKeyDescription(),
								strDocumentStatus
								), 
					   CXMLLogSpace::XML_ERROR,
					   IMPEXP_DIAGNOSTIC_CODE, 
					   szXTechDataManager
					  );
		EscapeDBTMaster(pTmpRec);		
		return FALSE;
	}

	//Mi occupo degli external-reference del documento master
	if (!m_bIsExtRef)
	{
		if (!ImportSmartDBTExtReferences(pDBTNode, pProfileInfo->GetDBTFromNamespace(pDBTMaster->GetNamespace())))
		{
			OutputMessage(_TB("Error importing external references"),
						  CXMLLogSpace::XML_ERROR,
					      IMPEXP_DIAGNOSTIC_CODE, 
					      szXTechDataManager);
			EscapeDBTMaster(pTmpRec);
			return FALSE;
		}
	}
	// metto da parte la lista degli indici di dataobj locckati e li unloccko
	// (dopo si riloccko)
	CUIntArray arLockedObjsIndexes;
	LockedIndexes(&arLockedObjsIndexes, TRUE, pNewMasterRec, NULL);	
	// Caricamento del record nel documento
	// Se il record non viene trovato lo si deve inserire devo fare di nuovo la finddata perch?il documento 
	// potrebbe essere stato inserito da un external reference.
	if (pDBTMaster->FindData(FALSE))
	{
		// Aggiornamento record esistente...
		m_pDoc->OnRadarRecordSelected(TRUE);
		if (!m_pDoc->ValidCurrentRecord() || m_pDoc->GetFormMode()!=CAbstractFormDoc::EDIT)
		{
			// il record ?locckato; lo sto locckando io col 
			// processo di importazione?
			if ((pTableToRelock=GetUnlockedTable()) == NULL)
			{
				EscapeDBTMaster(pTmpRec);
				return FALSE;
			}
		}		
	}
	else // Inserimento nuovo record...prima controllo che sia possibile
	{
		if (!m_pDoc->OnRadarRecordNew()) 
		{
			OutputMessage( cwsprintf(_TB("Failed to enter a new record for the document '{0-%s}' in the table '{1-%s}'."),
										(LPCTSTR)m_pDoc->GetTitle(),
										(LPCTSTR)pNewMasterRec->GetTableName()
									),
						   CXMLLogSpace::XML_ERROR,
						   IMPEXP_DIAGNOSTIC_CODE, 
						   szXTechDataManager
						  );
			EscapeDBTMaster(pTmpRec);
			return FALSE;
		}
	}
	LockedIndexes(&arLockedObjsIndexes, FALSE, pNewMasterRec, pTmpRec);	

	delete pTmpRec;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartCurrentRecord
						(
							CXMLNode*			pRecordNode, 
							CXMLProfileInfo*	pProfileInfo, 
							int					importAction
						)
{
	if (!m_pDoc || !AfxIsValidAddress(m_pDoc, sizeof(CAbstractFormDoc)) || !m_pDoc->m_pDBTMaster)
	{
		OutputMessage(_TB("Internal error in ImportSmartCurrentRecord procedure."),
					  CXMLLogSpace::XML_ERROR,
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager
					 );
		ASSERT(FALSE);
		return FALSE;
	}


	ASSERT(!m_bBusy);	// il documento ?impegnato ad importare un altro documento!
	m_bBusy = TRUE;

	if (!pRecordNode)
	{
		OutputMessage(_TB("No data to save"),
					  CXMLLogSpace::XML_WARNING,
					  IMPEXP_DIAGNOSTIC_CODE, 
					  szXTechDataManager
					  );
		return TRUE;
	}

	//pulisco il record 
	// uso l'init del DBT anzich?quello del SqlRecord poich?ci sono delle tabelle condivisi tra pi?DBT. 
	// Vedi Customer e Supplier
	m_pDoc->m_pDBTMaster->Init();

	//prima della valorizzazione dei campi
	m_pDoc->OnBeforeXMLImport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnBeforeXMLImport();
	m_pDoc->m_pDBTMaster->OnBeforeXMLImport ();

	//prendo il nodo del master
	CXMLNode* pDBTNode = pRecordNode->GetChildByName(m_pDoc->m_pDBTMaster->GetNamespace().GetObjectNameForTag(), FALSE);

	if (!pDBTNode)
	{
		m_bBusy = FALSE;
		return FALSE;
	}

	SqlTable* pTableToRelock = NULL;

	BOOL bResult = ImportSmartDBTMaster(pDBTNode, pProfileInfo, pTableToRelock, importAction);	

	if (importAction == IMPORT_ACTION_DELETE)
	{
		if (pTableToRelock) pTableToRelock->LockCurrent();
		EscapeDBTMaster(NULL);
		return bResult;	
	}

	for(int nSlave = 0; nSlave <= m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetUpperBound(); nSlave++)
		m_pDoc->m_pDBTMaster->GetDBTSlaves()->GetAt(nSlave)->Reload();

	// I have to raise master events only after fields unlock
	// otherwise they are unuseful
	FireRecEvents(m_pDoc->m_pDBTMaster->GetRecord(), pDBTNode);

	if (!bResult ||(m_pDoc->m_pDBTMaster->GetDBTSlaves() && !ImportSmartDBTSlaves(pRecordNode, pProfileInfo)))
	{
		if (pTableToRelock) pTableToRelock->LockCurrent();
		EscapeDBTMaster(NULL);
		return FALSE;
	}

	SqlRecord* pDBTMasterRec = m_pDoc->m_pDBTMaster->GetRecord();
	pDBTMasterRec->SetStorable();
	m_pDoc->OnPrepareAuxData();	
	
	if (!m_pDoc->OnOkXMLImport())
	{
		if (pTableToRelock) pTableToRelock->LockCurrent();
		EscapeDBTMaster(NULL);
		return FALSE;
	}
	
	
	//bisogna utilizzare il vecchio modo di attivare le tabdialog perchè con l'ottimizzazione le risorse nel caso di più thread contemporanei
	// saturano la memoria e non è più possibile creare le tabdialog
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
	
	
	m_pDoc->OnAfterXMLImport();
	if (m_pDoc->m_pClientDocs)
		m_pDoc->m_pClientDocs->OnAfterXMLImport();
	

	BOOL bOk = (((CSmartImportParams*)m_pXMLSmartParam)->m_bOnlySetData) ? TRUE : m_pDoc->SaveImportDocument();
	if (pTableToRelock) pTableToRelock->LockCurrent();

	UnlockFields (m_pDoc->m_pDBTMaster);
	m_bBusy = FALSE;
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::ImportSmartDocument(const CString& strData, CSmartImportParams* pImportParams, BOOL bLoadParamFromNsUri)
{

	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	m_pXMLSmartParam = pImportParams;	
	m_bForceUseOldXTechMode = TRUE;
	
	// carico la stringa in un XMLDom in modo da effettuare il parsing dei campi Findable
	CXMLDocumentObject  xmlDocument(FALSE, IsDisplayingMsgBoxesEnabled());
	if (!xmlDocument.LoadXML(strData))
	{
		//xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("The parameters xml string is invalid"));
		//m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	CXMLNode* pRoot = xmlDocument.GetRoot();
	if (!pRoot)
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("The parameters xml string is invalid"));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	m_bIsRootDoc = TRUE;	
	
	//cerco Namespace del documento e il nome del profilo
	CString strDocNamespace;
	CString strProfile;
	// controllo che il namespace presente nella root del file sia uguale a quello associato al corrente documento
	// e che esista il profilo 
	if (!pRoot->GetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, strDocNamespace) ||
		GetDocumentNamespace().ToString().CompareNoCase(strDocNamespace) != 0 )
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("TBNamespace is not equal to istanziated document namespace. Impossible to provide requested data"));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	if (bLoadParamFromNsUri)
		m_pXMLSmartParam->SetProfileInfoByNsUri(xmlDocument.GetNamespaceURI());

	CXMLNode* pChildNode = pRoot->GetFirstChild();
	if (!pChildNode)
	{
		xmlDiagnosticMng.AddMessage(
										CXMLLogSpace::XML_ERROR, 
										READ_DIAGNOSTIC_CODE, 
										szXTechDataManager, 
										cwsprintf(_TB("The received file does not contain data to process"))
									);
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	};

	GetInformationFromNsUri(xmlDocument.GetNamespaceURI(), strProfile,  m_pXMLSmartParam->m_ePosType, m_pXMLSmartParam->m_strUserName);

	if (strProfile.IsEmpty() || !pRoot->GetAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE, strProfile))
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, _TB("No export profile name found. Impossible to provide requested data"));
		m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
		return FALSE;
	}

	// Caricamento del profilo di esportazione utilizzato per la creazione del file xml da importare
	CString strProfilePath = ::GetProfilePath(m_pDoc->GetNamespace(), strProfile, m_pXMLSmartParam->m_ePosType, m_pXMLSmartParam->m_strUserName);
	if (!LoadSmartProfile(strProfilePath))
	{
			xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, READ_DIAGNOSTIC_CODE, szXTechDataManager, cwsprintf(_TB("Export profile {0-%s} not present in the current installation."), (strProfile.IsEmpty()) ? m_pDoc->GetNamespace().GetObjectName() : strProfile));
			m_pXMLSmartParam->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
			return FALSE;
	};

	
	CreateXMLExpImpManager();
	m_pDoc->EnableCounterCaching(TRUE);

	// non utilizzo il logging di XTech ma gestisco la diagnostica mediante la classe CSmartXMLDiagnosticMng
	m_pXMLExpImpMng->UseSmartXMLDiagnosticMng();

	BOOL bOK = FALSE;
	m_nStatus = XML_MNG_IDLE;

	CXMLNode* pDataNode = pRoot->GetChildByName(DOC_XML_DATA_TAG);
	if (pDataNode)
	{	
		m_nStatus = XML_MNG_IMPORTING_DATA;
		m_nProcessStatus = XML_IMPORTING_DATA;
		
		// il documento deve essere silente
		//Se siamo già in unattended mode non faccio niente, altrimenti lo imposto a true
		BOOL bOldUnattendedMode = m_pDoc->SetInUnattendedMode();

		// I have to set m_bBatch flag as some ERP diagnostic
		// behaviour is defined on m_bBatch value
		BOOL bOldBatch = m_pDoc->m_bBatch;
		m_pDoc->m_bBatch = TRUE;
		
		try
		{
			//Carico il documento corrispondete ai parametri passati.Se non esiste esco con un errore
			if (pImportParams->m_saveAction != IMPORT_ACTION_DELETE && !(CSmartImportParams*)pImportParams->m_bOnlySetData )
				m_pDoc->m_pTbContext->StartTransaction();	
		
			bOK = ImportSmartCurrentRecord(pDataNode, m_pSmartProfile, pImportParams->m_saveAction);
		}
		catch (CException* exc)
		{
			TCHAR szError[1024];
			exc->GetErrorMessage(szError, 1024);
			OutputMessage(szError, CXMLLogSpace::XML_WARNING, IMPEXP_DIAGNOSTIC_CODE, szXTechDataManager);		
			exc->Delete();
		}

		m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->InsertDocumentMessage(m_pDoc->GetMessages());
		m_pDoc->GetMessages()->ClearMessages(TRUE);
		
		// se il metodo OnPreparePKForXMLImport ha assegnato un counter, lo aveva bloccato per tenermelo per la procedura di import
		DataObj* pDataObj = m_pDoc->GetAssignedCounter();
		if (pDataObj)
			pDataObj->SetValueLocked (FALSE);

		try
		{
			if (!bOK)
				m_pDoc->m_pTbContext->Rollback();
			else
			{
				if (m_pSmartProfile->IsPostBack() && pImportParams->m_saveAction  != IMPORT_ACTION_DELETE && !((CSmartImportParams*)pImportParams->m_bOnlySetData))
				{
					m_nStatus = XML_MNG_EXPORTING_DATA;
					m_nProcessStatus = XML_EXPORTING_DATA;
					m_bIsPostBack = TRUE;
					//#BugFix 19926 (anomalia conseguente alla correzione della 19396, vedi sotto)
					m_pDoc->m_pDBTMaster->SetNoPreloadStep();
					//cancello il contenuto della directory dei dati (da cui ho importato) ed esporto il documento completo
					//BugFix #19396:  prima effettuo un refresh dei dati per evitare problemi relativi a righe vuote nei dbtslave e dbtslavebuffered			
					m_pDoc->BrowseRecord(FALSE);
					if (!ExportSmartCurrentRecord(m_pSmartProfile))
						OutputMessage(_TB("No postback action has been executed."), CXMLLogSpace::XML_WARNING, IMPEXP_DIAGNOSTIC_CODE, szXTechDataManager);				
					m_bIsPostBack = FALSE;
				}
			}
		}
		catch (CException* exc)
		{
			TCHAR szError[1024];
			exc->GetErrorMessage(szError, 1024);
			OutputMessage(szError, CXMLLogSpace::XML_WARNING, IMPEXP_DIAGNOSTIC_CODE, szXTechDataManager);		
			exc->Delete();
		}
		
		//Se non eravamo in unattended mode prima della procedura, riporto lo stato a false
		m_pDoc->SetInUnattendedMode(bOldUnattendedMode);
		m_pDoc->m_bBatch = bOldBatch;	
		m_nStatus = XML_MNG_IDLE;
	}
	else
		OutputMessage(_TB("Impossible to find data section"), CXMLLogSpace::XML_ERROR, IMPEXP_DIAGNOSTIC_CODE, szXTechDataManager);
	

	// se ci sono dei messaggi (errori o warning)creo il file avente lo stesso nome di quello processato ma 
	// memorizzato nella directory Error
	if (m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->HasMessages())
		m_pXMLSmartParam->AddToResult(m_pXMLExpImpMng->m_pSmartXMLDiagnosticMng->CreateXMLErrorString(pRoot, IsDisplayingMsgBoxesEnabled()));
	
	m_pDoc->SetAssignedCounter (NULL);
	//SAFE_DELETE(m_pXMLExpImpMng);
	return bOK;
}


//----------------------------------------------------------------------------
// Metodi utilizzati per la generazione del file dello schema per lo smart import\export
// utilizzato dall'integrazione con Office2003
//----------------------------------------------------------------------------

//----------------------------------------------------------------------------
void CXMLDataManager::InsertSingleDBTXRefInSmartXMLSchema
									(
										DBTObject*				pDBTObject, 
										CXMLHotKeyLinkArray*	pHKLArray,
										CXSDGenerator*			pSchema, 
										const CXMLProfileInfo*	pProfileInfo,
										CXMLXRefInfo*			pXRefInfo
									)
{
	if (!pDBTObject || !pSchema || !pXRefInfo)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLDBTInfo* pXMLDBTInfo = 	pProfileInfo 
								? pProfileInfo->GetDBTFromNamespace(pDBTObject->GetNamespace()) 
								: pDBTObject->GetXMLDBTInfo();

	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport()) 
		return;

	CXMLHotKeyLink* pHKLInfo = pHKLArray->GetHKLByFieldName(pXRefInfo->GetName() + URL_SLASH_CHAR + pXMLDBTInfo->GetNamespace().GetObjectNameForTag(), CXMLHotKeyLink::XREF, CXMLHotKeyLink::DBT);

	CString strDBTName = pXMLDBTInfo->GetNamespace().GetObjectNameForTag();
	if (pXMLDBTInfo->GetType() == CXMLDBTInfo::BUFFERED_TYPE)
	{
		pSchema->BeginComplexElement(strDBTName, _T("0"), _T("1"));
			pSchema->BeginComplexElement(strDBTName + XML_ROW_TAG, _T("0"), _T("unbounded"));
				InsertDBTFieldsInSmartXMLSchema(pXMLDBTInfo, pHKLArray, pSchema, pProfileInfo, pDBTObject->GetRecord(), pXRefInfo);
				if (pHKLInfo)
					pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pHKLInfo->GetReportNamespace().ToString());
			pSchema->EndComplexElement();
		pSchema->EndComplexElement();	
	}
	else
	{
		pSchema->BeginComplexElement(pXMLDBTInfo->GetNamespace().GetObjectNameForTag(), (pDBTObject->IsKindOf(RUNTIME_CLASS(DBTMaster))) ? _T("1") : _T("0"), _T("1"));
			InsertDBTFieldsInSmartXMLSchema(pXMLDBTInfo, pHKLArray, pSchema, pProfileInfo, pDBTObject->GetRecord(), pXRefInfo);
			if (pDBTObject->IsKindOf(RUNTIME_CLASS(DBTMaster)))
				pSchema->InsertAttribute(DOC_XML_MASTER_ATTRIBUTE, SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE , _T(""), _T("true"));
			if (pHKLInfo)
				pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pHKLInfo->GetReportNamespace().ToString());
		pSchema->EndComplexElement();
	}	
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertHKLInSmartXMLSchema(CXMLHotKeyLink* pHKLInfo, SqlRecordItem* pRecItem, BOOL bUseSoapType, CXSDGenerator* pSchema)
{
	//inserisco le informazioni relative al report da lanciare per ottenere i dati referenziati dall'hotlink
	/*<xs:element name="Commessa" minOccurs="0">
		<xs:complexType>
			<xs:simpleContent>
				<xs:extension base="xs:string">
					<xs:attribute name="hotLink" type="xs:string" fixed="Report.magonet.analitica.Jobs_Data"></xs:attribute>
				</xs:extension>
			</xs:simpleContent>
		</xs:complexType>
	</xs:element>*/

	if (!pHKLInfo || !pSchema || !pRecItem)
		return;

	CString strLower = pRecItem->IsSpecial() ? _T("1") : _T("0");
	pSchema->BeginSimpleElement(pRecItem->GetColumnName(), pRecItem->GetDataObj()->GetXMLType(bUseSoapType), strLower,  _T("1"));
		pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pHKLInfo->GetReportNamespace().ToString());
	pSchema->EndSimpleElement();
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertXRefInSmartXMLSchema(CXMLXRefInfo* pXRefInfo, CXMLHotKeyLinkArray* pHKLArray, CXSDGenerator* pSchema)
{
	//inserisco il dbtmaster del documento referenziato inserendo anche gli attributi relativi al namespace
	// del documento e al profilo associato all'extref
	/*	<xs:element name="AnagraficaFornitore" minOccurs="0" maxOccurs="1>
			<xs:complexType>
				<xs:sequence>
					<xs:element name="Testa" minOccurs="1" maxOccurs="1">
						<xs:complexType>
							<xs:sequence>
								<xs:element name="TipoCliFor" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="CliFor" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="RagioneSociale" type="xs:string" />
								<xs:element name="PartitaIva" type="xs:string" />
								<xs:element name="CodiceFiscale" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="ContoPdC" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="Indirizzo" type="xs:string" />
								<xs:element name="CAP" type="xs:string" />
								<xs:element name="Citta" type="xs:string" />
							</xs:sequence>
						<xs:complexType>
					</xs:element>
					<xs:element name="Altri-Dati" minOccurs="0" maxOccurs="1">
						<xs:complexType>
							<xs:sequence>
								<xs:element name="TipoCliFor" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="CliFor" type="xs:string" minOccurs="0" maxOccurs="1" />
								<xs:element name="UltOrdine" type="xs:int" minOccurs="0"></xs:element>
								<xs:element name="UltDataOrdine" type="xs:dateTime" minOccurs="0"></xs:element>
								<xs:element name="UltTotOrdine" type="xs:double" minOccurs="0"></xs:element>
							</xs:sequence>
						</xs:complexType>
					</xs:element>			
				</xs:sequence>
				<xs:attribute name="externalreference" type="xs:boolean" fixed="true" />
				<xs:attribute name="hotLink" type="xs:string" fixed="reportNamespace" />
			</xs:complexType>	
		</xs:element>
*/

	if (!pXRefInfo || !pSchema )
	{
		ASSERT(FALSE);
		return;
	}
	
	CAbstractFormDoc* pExtRefDoc = NULL;
	CXMLDocElement* pXMLDocElem = NULL;

	// chiamo la funzione che si occupa di valorizzare i segmenti
	pXMLDocElem = m_pXMLExpImpMng->GetXMLDocElement(pXRefInfo->GetDocumentNamespace(), m_pDoc);
	pExtRefDoc = pXMLDocElem ? pXMLDocElem->m_pDocument : NULL;
	if (!pExtRefDoc) 
		return;

	CXMLProfileInfo* pProfileInfo = GetXMLProfileInfo(pExtRefDoc, pXRefInfo->GetProfile());
	
	SqlRecord* pRecord = pExtRefDoc->m_pDBTMaster->GetRecord();

	pSchema->BeginComplexElement(pXRefInfo->GetName(), _T("0"),  _T("1"));
		// inserisco il master
		InsertSingleDBTXRefInSmartXMLSchema(pExtRefDoc->m_pDBTMaster, pHKLArray, pSchema, pProfileInfo, pXRefInfo);			
		// itero sugli slave
		DBTArray* pDBTSlaves = pExtRefDoc->m_pDBTMaster->GetDBTSlaves();
		if (pDBTSlaves && pDBTSlaves->GetSize())
		{
			for (int i = 0; i < pDBTSlaves->GetSize(); i++)
			{
				DBTSlave* pDBTSlave = pDBTSlaves->GetAt(i);
				ASSERT(pDBTSlave);
				InsertSingleDBTXRefInSmartXMLSchema(pDBTSlave, pHKLArray, pSchema, pProfileInfo, pXRefInfo);
			}
		}
		pSchema->InsertAttribute(DOC_XML_EXTREF_ATTRIBUTE, SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE, _T(""), _T("true"));	
		CXMLHotKeyLink* pHKLInfo = pHKLArray->GetHKLByFieldName(pXRefInfo->GetName(), CXMLHotKeyLink::XREF, CXMLHotKeyLink::XREF);
		if (pHKLInfo)
			pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), pHKLInfo->GetReportNamespace().ToString());
		pSchema->EndComplexElement();

	delete pProfileInfo;
}

//@@CHINA : Generate Drop list based on enumerate data automatically
//----------------------------------------------------------------------------
void CXMLDataManager::InsertEnumerationInSmartXMLSchema(SqlRecordItem* pRecItem, WORD enumValue, CString enumName, CXSDGenerator* pSchema)
{
	
	if ( !pSchema )
		return;

	
	CString enumValueStr;
	enumValueStr.Format(_T("%i"), enumValue);
	const EnumItemArray* items= AfxGetEnumsTable()->GetEnumItems(enumValue);
	enumName = items->GetAt(0)->GetOwnerModule().ToString();

	pSchema->BeginSimpleElement(pRecItem->GetColumnName(), pRecItem->GetDataObj()->GetXMLType(false), _T(""),  _T("1"));
	pSchema->InsertAttribute(DOC_XML_ENUMERATION_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), enumValueStr);
	pSchema->InsertAttribute(DOC_XML_ENUMERATIONNAMESPACE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), enumName);
	pSchema->EndSimpleElement();
}

//@@CHINA : Generate Drop list based on enumerate data automatically
//----------------------------------------------------------------------------
void CXMLDataManager::InsertPrimaryKeyInSmartXMLSchema(SqlRecordItem* pRecItem, CXSDGenerator* pSchema)
{
	if ( !pSchema )
		return;
	pSchema->BeginSimpleElement(pRecItem->GetColumnName(), pRecItem->GetDataObj()->GetXMLType(TRUE), _T(""),  _T("1"));
	
	pSchema->InsertAttribute(DOC_XML_PRIMARYKEY_ATTRIBUTE, SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE, _T(""), _T("true"));
	pSchema->EndSimpleElement();
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertDBTFieldsInSmartXMLSchema
									(
										CXMLDBTInfo*			pXMLDBTInfo, 
										CXMLHotKeyLinkArray*	pHKLArray,
										CXSDGenerator*			pSchema, 
										const CXMLProfileInfo*	pProfileInfo,
										SqlRecord*				pRecord,
										CXMLXRefInfo*			pXRefInfo /*= NULL*/
									)
{
	if (!pXMLDBTInfo || !pSchema || !pRecord)
	{
		ASSERT(FALSE);
		return;
	}
	
	//array che contiene il nome degli external reference gi?
	//processati per il dbt corrente
	CStringArray aProcessedExtRefs;
	BOOL bUseSoapType = TRUE;
	CXMLHotKeyLink* pHKLInfo = NULL;
	//inserisco i soli campi del dbt che sono da esportare 
	for (int nColIdx = 0; nColIdx < pRecord->GetSize(); nColIdx++)
	{
		SqlRecordItem* pRecItem = pRecord->GetAt(nColIdx);
		CString strColumnName = pRecItem->GetColumnName();
		
		if (pRecord->IsVirtual(nColIdx) || !pXMLDBTInfo->IsFieldToExport(strColumnName) || !pRecItem->GetDataObj())
			continue;

		//TODO_MARCO TIPI XML
		bUseSoapType = FALSE;
		

		// se non devo esportare gli externalreference, allora scrivo l'elemento e non controllo la presenza o meno di extref
		// associati al campo. Questo lo faccio solo se al campo non ?associato un hotkeylink
		if (pXRefInfo)
		{
			CString strDBTName = pXMLDBTInfo->GetNamespace().GetObjectNameForTag();
			if (pXMLDBTInfo->GetType() == CXMLDBTInfo::BUFFERED_TYPE)
				strDBTName += _T("/") + strDBTName + XML_ROW_TAG;
			CString strFieldName;
			strFieldName = cwsprintf(_T("{0-%s}/{1-%s}/{2-%s}"), pXRefInfo->GetName(), strDBTName, strColumnName);		
			pHKLInfo = (pHKLArray) ? pHKLArray->GetHKLByFieldName(strFieldName, CXMLHotKeyLink::XREF, CXMLHotKeyLink::FIELD) : NULL ;
			if (pHKLInfo)
				InsertHKLInSmartXMLSchema(pHKLInfo, pRecItem, bUseSoapType, pSchema);
			else
				//@@BAUZI da mettere limite inferiore e superiore a seconda di un parametro associato al campo 
				pSchema->InsertElement(strColumnName, pRecItem->GetDataObj()->GetXMLType(bUseSoapType), pRecItem->IsSpecial() ? _T("1") : _T("0"));
			continue;
		}

		//@@CHINA : Generate Drop list based on enumerate data automatically
		//procedure to handle enumeration type in schema 
		if (pRecItem->GetDataObj()->GetDataType() == DATA_ENUM_TYPE)
		{
			//get enumType value 
			WORD enumType = ((DataEnum*)pRecItem->GetDataObj())->GetTagValue();

			//Search in the enumtable for the enum's namespace
			EnumsTableConstPtr pEnumTable = AfxGetEnumsTable();
			if (pEnumTable)		
				InsertEnumerationInSmartXMLSchema(pRecItem, enumType, pEnumTable->GetEnumTagName(enumType), pSchema);
			continue;
		}
		//Insert the primary key information 
		if (pRecItem->IsSpecial())
		{
			InsertPrimaryKeyInSmartXMLSchema(pRecItem, pSchema);
			//continue;
		}
	
	
		CXMLXRefInfo* pCurrXRefInfo = NULL;
		CXMLXRefInfoArray aXMLXRefInfoArray;
		aXMLXRefInfoArray.SetOwns(FALSE);
		if (pXMLDBTInfo->m_pXRefsArray)
			pXMLDBTInfo->m_pXRefsArray->GetXRefArrayByFK((LPCTSTR)strColumnName, &aXMLXRefInfoArray);
	
		// se non ho external-references inserisco solo l'elemento field
		if (aXMLXRefInfoArray.GetSize() <= 0)
		{
			if (pRecItem->IsSpecial())
				continue; // gia` inserito come descrizione PK

			pHKLInfo = (pHKLArray) ? pHKLArray->GetHKLByFieldName(strColumnName, CXMLHotKeyLink::FIELD) : NULL ;
			if (pHKLInfo)
				InsertHKLInSmartXMLSchema(pHKLInfo, pRecItem, bUseSoapType, pSchema);
			else
			//@@BAUZI da mettere limite inferiore e superiore a seconda di un parametro associato al campo 
			{
				//@@dengxiaobin
				DataType dataType = pRecItem->GetDataObj()->GetDataType();
				CString strNewType ;
				if (  dataType != dataType.String)
					strNewType = pRecItem->GetDataObj()->GetXMLType(bUseSoapType);
				else
				{
					int maxLength = pRecItem->GetColumnInfo()->GetColumnLength();
					CString strMaxLength;
					strMaxLength.Format(_T("%d"), maxLength);
					strNewType.Format(_T("string%d"), maxLength);
					m_maxStrTypes[strNewType] =  strMaxLength;
				}
				//dengxiaobin
				//pSchema->InsertElement(strColumnName, pRecItem->GetDataObj()->GetXMLType(bUseSoapType), pRecItem->IsSpecial() ? _T("1") : _T("0"));
				pSchema->InsertElement(strColumnName, strNewType, pRecItem->IsSpecial() ? _T("1") : _T("0"));
				
			}
			continue;
		}
		
		
		//ciclo sugli external reference definiti per il campo
		BOOL bExtRefFound = FALSE;
		for (int nRef = 0; nRef < aXMLXRefInfoArray.GetSize(); nRef++)
		{
			pCurrXRefInfo = aXMLXRefInfoArray.GetAt(nRef);
			if (!pCurrXRefInfo)
				continue;

			BOOL bContinue = FALSE;				
			// se ?un external-reference che ho gi?inserito non faccio niente
			for (int i = 0; i <= aProcessedExtRefs.GetUpperBound(); i++)
			{	
				if (pCurrXRefInfo->GetName() == aProcessedExtRefs.GetAt(i))
				{
					bContinue = TRUE;
					break;
				}
					
			}
			if (bContinue) continue;

			aProcessedExtRefs.Add(pCurrXRefInfo->GetName());			
			CXMLUniversalKeyGroup* pUniversalKeyGroup = pCurrXRefInfo->GetXMLUniversalKeyGroup();	
			// inserisco l'ext-reference ovvero il dbtmaster e gli slave del documento corrispondente con i campi
			// che verificano il profilo associato all'ext-ref. Una volta fatto questo inserisco il capo ext-ref
			// nella lista dei campi del corrente dbt
			if (pCurrXRefInfo->IsToUse() && (!pUniversalKeyGroup || pUniversalKeyGroup->IsExportData()))
			{
				bExtRefFound = TRUE;
				CXMLHotKeyLinkArray aHKLArray;
				if (pXMLDBTInfo->m_pXMLHotKeyLinkArray)
					pXMLDBTInfo->m_pXMLHotKeyLinkArray->GetAllHKLForType(CXMLHotKeyLink::XREF, &aHKLArray, pCurrXRefInfo->GetName());
					
				InsertXRefInSmartXMLSchema(pCurrXRefInfo, &aHKLArray, pSchema);
			}
		}
		//se non ho nessun external reference esportabile devo inserire solo una volta il campo
		//@@BAUZI da mettere limite inferiore e superiore a seconda di un parametro associato al campo 
		if (!bExtRefFound)
		{			
			pHKLInfo = (pHKLArray) ? pHKLArray->GetHKLByFieldName(strColumnName, CXMLHotKeyLink::FIELD) : NULL ;

			if (pHKLInfo)
				InsertHKLInSmartXMLSchema(pHKLInfo, pRecItem, bUseSoapType, pSchema);
			else
				pSchema->InsertElement(strColumnName, pRecItem->GetDataObj()->GetXMLType(bUseSoapType), pRecItem->IsSpecial() ? _T("1") : _T("0"));
		}
	}
}

//----------------------------------------------------------------------------
CXMLDBTInfo* CXMLDataManager::InsertSingleDBTInSmartXMLSchema
									(
										DBTObject*				pDBTObject, 
										CXSDGenerator*			pSchema, 
										const CXMLProfileInfo*	pProfileInfo
									)
{
	if (!pDBTObject || !pSchema )
	{
		ASSERT(FALSE);
		return NULL;
	}

	//	<xs:complexType name="OrdineFornitore">
	//		<xs:sequence>
	//			<xs:element name="IdOrdFor" type="xs:int" minOccurs="0" maxOccurs="1" />
	//			<xs:element name="NrOrdForInterno" type="xs:string" />
	//			<xs:element name="DataOrdine" type="xs:date" />
	//			<xs:element name="Pagamento" type="xs:string" />
	//			<xs:element name="Agente" type="xs:string" />
	//			<xs:element name="Fornitore" type="FornitoreExternalReference" />
	//		</xs:sequence>
	//		<xs:attribute name="master" type="xs:boolean" fixed="true" />
	//		<xs:attribute name="updateType" type="xs:int" fixed="0" />
	//  </xs:complexType>

	CXMLDBTInfo* pXMLDBTInfo = pProfileInfo 
						? pProfileInfo->GetDBTFromNamespace(pDBTObject->GetNamespace()) 
						: pDBTObject->GetXMLDBTInfo();
		
	if (!pXMLDBTInfo || !pXMLDBTInfo->IsToExport()) 
		return NULL;
	
	CTBNamespace nsDBTReport;
	CXMLHotKeyLink* pXMLHKLInfo = pXMLDBTInfo->GetDBTXMLHotKeyLinkInfo();
	if (pXMLHKLInfo)
		nsDBTReport = pXMLHKLInfo->GetReportNamespace();
	
	if (pXMLDBTInfo->GetType() == CXMLDBTInfo::BUFFERED_TYPE)
	{
		CString strDBTRow = pXMLDBTInfo->GetNamespace().GetObjectNameForTag() + XML_ROW_TAG;
		pSchema->BeginComplexType(strDBTRow);
			InsertDBTFieldsInSmartXMLSchema(pXMLDBTInfo, pXMLDBTInfo->m_pXMLHotKeyLinkArray, pSchema, pProfileInfo, pDBTObject->GetRecord());
			//HOTLINK legato al DBT
			if (!nsDBTReport.IsEmpty())
				pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), nsDBTReport.ToString());
		pSchema->EndComplexType();
		pSchema->BeginComplexType(pXMLDBTInfo->GetNamespace().GetObjectNameForTag());
			pSchema->InsertElement(strDBTRow, strDBTRow, _T("0"), _T("unbounded"));
			pSchema->InsertAttribute(DOC_XML_UPDATE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_INTEGER_VALUE , _T(""), _T("0"));
		pSchema->EndComplexType();	
	}
	else
	{
		pSchema->BeginComplexType(pXMLDBTInfo->GetNamespace().GetObjectNameForTag());
			InsertDBTFieldsInSmartXMLSchema(pXMLDBTInfo, pXMLDBTInfo->m_pXMLHotKeyLinkArray, pSchema, pProfileInfo, pDBTObject->GetRecord());
			//HOTLINK legato al DBT
			if (!nsDBTReport.IsEmpty())
				pSchema->InsertAttribute(DOC_XML_HOTKEYLINK_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), nsDBTReport.ToString());
			if (pDBTObject->IsKindOf(RUNTIME_CLASS(DBTMaster)))
				pSchema->InsertAttribute(DOC_XML_MASTER_ATTRIBUTE, SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE , _T(""), _T("true"));
			else
				pSchema->InsertAttribute(DOC_XML_UPDATE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_INTEGER_VALUE , _T(""), _T("0"));
		pSchema->EndComplexType();
	}	
	return pXMLDBTInfo;
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertDataTagInSmartXMLSchema
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
	CXMLDBTInfoArray dbtInfoArray;
	dbtInfoArray.SetOwns(FALSE);

	// creo lo schema di ogni dbt secondo le selezioni effettuate nel profilo di esportazione scelto
	CXMLDBTInfo* pXMLDBTInfo = InsertSingleDBTInSmartXMLSchema(pDBTMaster, pSchema, pProfileInfo);
	if (pXMLDBTInfo) dbtInfoArray.Add(pXMLDBTInfo);
		
	
	DBTArray* pDBTSlaves = pDBTMaster->GetDBTSlaves();
	if (pDBTSlaves)
	{
		for (int i = 0; i <= pDBTSlaves->GetUpperBound(); i++)
		{
			DBTSlave* pDBTSlave = (DBTSlave*)pDBTSlaves->GetAt(i);			
			pXMLDBTInfo = InsertSingleDBTInSmartXMLSchema(pDBTSlave, pSchema, pProfileInfo);
			if (pXMLDBTInfo) dbtInfoArray.Add(pXMLDBTInfo);
		}
	}	
	//creo il complexType Data che contiene l'elenco di tutti i dbt presenti nello schema
	/*xs:element name="Data">
		<xs:complexType>
			<xs:sequence>
				<xs:element name="OrdineFornitore" type="OrdineFornitore" minOccurs="1" maxOccurs="1" />
				<xs:element name="Righe" type="Righe" minOccurs="0" maxOccurs="unbounded" />
				<xs:element name="Spese" type="Spese" minOccurs="0" maxOccurs="1" />
				<xs:element name="Spedizione" type="Spedizione" minOccurs="0" maxOccurs="1" />
			</xs:sequence>
		</xs:complexType>
	</xs:element>*/
	pSchema->BeginComplexType(DOC_XML_DATA_TAG);
	for(int nDBT = 0; nDBT 	<= dbtInfoArray.GetUpperBound(); nDBT++)
	{

		CXMLDBTInfo* pXMLDBTInfo = dbtInfoArray.GetAt(nDBT);
		pSchema->InsertElement(
								pXMLDBTInfo->GetNamespace().GetObjectNameForTag(), 
								pXMLDBTInfo->GetNamespace().GetObjectNameForTag(), 
								(pXMLDBTInfo->GetType() == CXMLDBTInfo::MASTER_TYPE) ? _T("1") : _T("0"), _T("1")
							   );
	}
	pSchema->EndComplexType();
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::InsertParametersTagInSmartXMLSchema(CXSDGenerator* pSchema, const CXMLProfileInfo* pProfileInfo)
{
	CUserExportCriteria* pUserCrit =  (pProfileInfo->GetXMLExportCriteria() && pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()) 
										? pProfileInfo->GetXMLExportCriteria()->GetUserExportCriteria()
										: NULL;

	if (pUserCrit && pUserCrit->m_pQueryInfo && pUserCrit->m_pQueryInfo->m_pPrgData)
	{
		AskRuleData* pAskData = pUserCrit->m_pQueryInfo->m_pPrgData->GetAskRuleData();
		if (pAskData)
			return pAskData->GetSchema(pSchema, TRUE);
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CXMLDataManager::InsertDiagnosticTagInSmartXMLSchema(CXSDGenerator*	pSchema)
{	
	//	<xs:complexType name="CDiagnostic">
	//		<xs:sequence>
	//			<xs:element name="Errors" minOccurs="0" maxOccurs="1">
	//				<xs:complexType>
	//					<xs:sequence>
	//						<xs:element name="Error" minOccurs="0" maxOccurs="unbounded">
	//							<xs:complexType>
	//								<xs:sequence>
	//									<xs:element name="Code" type="xs:int" minOccurs="0" maxOccurs="1"/>
	//									<xs:element name="Source" type="xs:string" minOccurs="0" maxOccurs="1"/>
	//									<xs:element name="Message" type="xs:string" minOccurs="0" maxOccurs="1"/>
	//								</xs:sequence>
	//							</xs:complexType>
	//						</xs:element>
	//					</xs:sequence>
	//				</xs:complexType>
	//			</xs:element>
	//			<xs:element name="Warnings" minOccurs="0" maxOccurs="1">
	//				<xs:complexType>
	//					<xs:sequence>
	//						<xs:element name="Warning" minOccurs="0" maxOccurs="unbounded">
	//							<xs:complexType>
	//								<xs:sequence>
	//									<xs:element name="Code" type="xs:int" minOccurs="0" maxOccurs="1"/>
	//									<xs:element name="Source" type="xs:string" minOccurs="0" maxOccurs="1"/>
	//									<xs:element name="Message" type="xs:string" minOccurs="0" maxOccurs="1"/>
	//								</xs:sequence>
	//							</xs:complexType>
	//						</xs:element>
	//					</xs:sequence>
	//				</xs:complexType>
	//			</xs:element>
	//		</xs:sequence>
	//	</xs:complexType>


	pSchema->BeginComplexType(DOC_XML_DIAGNOSTIC_TAG);
		pSchema->BeginComplexElement(DOC_XML_ERRORS_TAG, _T("0"), _T("1"));
			pSchema->BeginComplexElement(DOC_XML_ERROR_TAG, _T("0"),	SCHEMA_XSD_UNBOUNDED_VALUE);
				pSchema->InsertElement(DOC_XML_CODE_TAG, 				SCHEMA_XSD_DATATYPE_INT_VALUE,		_T("0"), _T("1")); 
				pSchema->InsertElement(DOC_XML_SOURCE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
				pSchema->InsertElement(DOC_XML_MESSAGE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
			pSchema->EndComplexElement();  
		pSchema->EndComplexElement();  
		pSchema->BeginComplexElement(DOC_XML_WARNINGS_TAG, _T("0"), _T("1"));
			pSchema->BeginComplexElement(DOC_XML_WARNING_TAG, _T("0"),	SCHEMA_XSD_UNBOUNDED_VALUE);
				pSchema->InsertElement(DOC_XML_CODE_TAG, 				SCHEMA_XSD_DATATYPE_INT_VALUE,		_T("0"), _T("1")); 
				pSchema->InsertElement(DOC_XML_SOURCE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
				pSchema->InsertElement(DOC_XML_MESSAGE_TAG,				SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T("0"), _T("1")); 
			pSchema->EndComplexElement();  
		pSchema->EndComplexElement();  
	pSchema->EndComplexType();  
}

//----------------------------------------------------------------------------
BOOL CXMLDataManager::CreateSmartXMLSchemaFile(const CXMLProfileInfo* pProfileInfo, CAbstractFormDoc* pDoc) 
{
	CXSDGenerator* pXMLSchema =  GetSmartXMLSchemaString(pProfileInfo, pDoc);
	
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(pProfileInfo->m_strDocProfilePath);
	CString strUser = AfxGetPathFinder()->GetUserNameFromPath(pProfileInfo->m_strDocProfilePath);
	
	if (ExistFile(pProfileInfo->m_strSchemaFileName))
	{
		if (IsDisplayingMsgBoxesEnabled())
		{
			CString strProfile = (pProfileInfo->GetName().IsEmpty()) ? _TB("Document description") : pProfileInfo->GetName();
			CString strMsg;
			switch (ePosType)
			{
				case CPathFinder::USERS:
					strMsg = cwsprintf(_TB("for the user  '{0-%s}'"), strUser); break;
				case CPathFinder::ALL_USERS:
					strMsg = _TB("in \"AllUsers\""); break;
			}

			if (AfxMessageBox(cwsprintf(_TB("The XSD schema of the profile '{0-%s}' already exists' {1-%s}'. Do you need to overwrite it?"), strProfile, strMsg), MB_YESNO) == IDNO)
				return FALSE;
		}
	}

	// Salvataggio del file XML
	BOOL bRes = pXMLSchema->SaveXMLFile((LPCTSTR)pProfileInfo->m_strSchemaFileName, TRUE);
	delete pXMLSchema;
	return bRes;
}

//----------------------------------------------------------------------------
CXSDGenerator* CXMLDataManager::GetSmartXMLSchemaString(const CXMLProfileInfo* pProfileInfo, CAbstractFormDoc* pDoc) 
{
	if (!pProfileInfo)
	{
		ASSERT(FALSE);
		return NULL;
	}

	if (!pDoc)
		pDoc = m_pDoc;

	if (!pDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	//devo inserire l'URI per il profilo utilizzato
	//se profilo vuoto devo considerare la descrizione del documento
	CString strProfileName = (pProfileInfo->GetName().IsEmpty())
						 ? DESCRI + pDoc->GetNamespace().GetObjectName() 
						 : pProfileInfo->GetName();	


	//http://www.microarea.it/Schema/2004/Smart/AppName/ModName/DocumentName/PosType/UserName/ProfileName.xsd id = "AliasSchemaName"
	CString targetNamespace = pProfileInfo->GetSmartNamespaceURI();

	CString strId = pDoc->GetNamespace().GetObjectName() + pProfileInfo->m_strProfileName;
	strId.Replace(BLANK_CHAR, _T('_'));

	CXSDGenerator* pXMLSchema = new CXSDGenerator(targetNamespace, IsDisplayingMsgBoxesEnabled(), strId);

	//inserisco i DBT
	InsertDataTagInSmartXMLSchema(pDoc->m_pDBTMaster, pXMLSchema, pProfileInfo);
	
	// inserisco gli eventuali criteri di estrazione definiti dall'utente nel profilo
	BOOL bExistParameters = InsertParametersTagInSmartXMLSchema(pXMLSchema, pProfileInfo);

	InsertDiagnosticTagInSmartXMLSchema(pXMLSchema);	

	//<xs:simpleType name="string8">
	//	<xs:restriction base="xs:string">
	//		<xs:maxLength value="8"/>
	//	</xs:restriction>
	//</xs:simpleType>
	//@@dengxiaobin, insert other string type which has maxLength facet	
	for (POSITION pos = m_maxStrTypes.GetStartPosition(); pos!=NULL;)
	{
		CString strNewType;
		CString strMaxLength;
		m_maxStrTypes.GetNextAssoc(pos, strNewType, strMaxLength);

		pXMLSchema->BeginSimpleType(strNewType);
			pXMLSchema->BeginGenericElement(SCHEMA_XSD_RESTRICTION_TAG);
				pXMLSchema->InsertGenericElementAttribute(SCHEMA_XSD_BASE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE);
				pXMLSchema->BeginGenericElement(SCHEMA_XSD_MAXLENGTH_TAG);
						pXMLSchema->InsertGenericElementAttribute(SCHEMA_XSD_VALUE_ATTRIBUTE, strMaxLength);
				pXMLSchema->EndGenericElement();
			pXMLSchema->EndGenericElement();
		pXMLSchema->EndSimpleType();
	}
	//@@

	/*<xs:element name="OrdiniAFornitori">
		<xs:complexType>
			<xs:sequence>
				<xs:element name="Data" type="Data" minOccurs="1" maxOccurs="1" />
				<xs:element name="RiepilogoIva" type="RiepilogoIVA" minOccurs="0" maxOccurs="unbounded" />
				<xs:element name="CDiagnostic" type="CDiagnostic" minOccurs="0" maxOccurs="1" />
			</xs:sequence>
			<xs:attribute name="namespace" type="xs:string" fixed="Document.MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor" />
			<xs:attribute name="profile" type="xs:string" fixed="PredefinedForOffice" />
		</xs:complexType>
	</xs:element>*/

	//Element che rappresenta il documento
	pXMLSchema->BeginComplexElement(pDoc->GetNamespace().GetObjectNameForTag());
		pXMLSchema->InsertElement(DOC_XML_DATA_TAG, DOC_XML_DATA_TAG,		_T("1"), _T("1"));
		if (bExistParameters)
			pXMLSchema->InsertElement(XML_PARAMETERS_TAG, XML_PARAMETERS_TAG,	_T("0"), _T("1"));	
		pXMLSchema->InsertElement(DOC_XML_DIAGNOSTIC_TAG, DOC_XML_DIAGNOSTIC_TAG,	_T("0"), _T("1"));
		pXMLSchema->InsertAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""),	pDoc->GetNamespace().ToString());
		pXMLSchema->InsertAttribute(DOC_XML_XTECHPROFILE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""),	strProfileName);
		pXMLSchema->InsertAttribute(DOC_XML_POSTABLE_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE,	_T(""),	::FormatBoolForXML(pProfileInfo->IsPostable(), TRUE));
		pXMLSchema->InsertAttribute(DOC_XML_POSTBACK_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE,	_T(""),	::FormatBoolForXML(pProfileInfo->IsPostBack(), TRUE));
	pXMLSchema->EndComplexElement();
	
	// Salvataggio del file XML
	return pXMLSchema;
}


