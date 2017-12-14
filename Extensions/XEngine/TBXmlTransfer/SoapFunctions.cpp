
#include "StdAfx.h"

#include  <io.h>
#include  <stdio.h>
#include  <stdlib.h>

#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\Templates.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TBXMLCore\xmldocobj.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGenlib\BaseApp.h>
#include <TbGenlib\TBCommandInterface.h>

#include <TBGES\extdoc.h>
#include <TBWoormEngine\report.h>

#include "xmldatamng.h"
#include "XMLProfileInfo.h"
#include "genfunc.h"
#include "XMLTransferTags.h"
#include "soapfunctions.h"
#include "ProfiliMngWizPage.h"


const TCHAR szXTechSoapFunctions[] = _T("XTech-SoapFunctions");
const TCHAR szDocument[] = _T("Document.xml");
const TCHAR szNs[] = _T("/ns:");

// costanti stringa
BEGIN_TB_STRING_MAP(Messages)
	TB_LOCALIZED(INVALID_DOCUMENT,		"Invalid document type")
	TB_LOCALIZED(NO_DATA_MANAGER,		"This document is not enabled for transfer operations via XTech/MagicLink/MagicDocument")
	TB_LOCALIZED(CANT_OPEN_DOC,			"Impossible open document")
	TB_LOCALIZED(INVALID_DOC_HANDLE,	"Not  exist active document corrispondent to shown identifier")
	TB_LOCALIZED(DOC_NO_BROWSE,			"Document not in visualization state")
	TB_LOCALIZED(XML_NOPARAMS_FOUND,	"Invalid parameters. Impossibile to get data")
	TB_LOCALIZED(XML_INVALID_STRING,	"Invalid xml string. Impossibile to save data")	
	TB_LOCALIZED(XML_INVALID_NAMESPACE_STRING, "Invalid namespace. Impossibile to open the document,")
	TB_LOCALIZED(XML_NO_PARAMETERS,		"This document does not expect parameters")
END_TB_STRING_MAP()

#define CREATE_DOC_FAILED		0
#define CREATE_AND_CACHE_DOC	1
#define USE_CACHED_DOC			2
#define CREATE_NEW_DOC			3


//----------------------------------------------------------------------------------------------
void GetXMLExportParams
				(
					CExternalControllerInfo &info,
					DataInt **pDocSelType, 
					DataInt **pProfileSelType, 
					DataBool **pSendExportedEnvelope, 
					DataStr **pProfileName, 
					DataStr **pExportCriteria,
					DataStr **pExportPath
				)
{
	static TCHAR szDocSelType[]				= _T("DocSelType");
	static TCHAR szProfileSelType[]			= _T("ProfileSelType");
	static TCHAR szSendExportedEnvelope[]	= _T("SendExportedEnvelope");
	static TCHAR szProfileName[]			= _T("ProfileName");
	static TCHAR szExportCriteria[]			= _T("ExportCriteria");
	static TCHAR szExportPath[]				= _T("ExportPath");

	
	*pDocSelType = (DataInt*) info.m_Data.GetParamValue(szDocSelType); 
	
	if (!*pDocSelType)
	{
		info.m_Data.InternalAddParam(szDocSelType, &DataInt(EXPORT_ONLY_CURR_DOC), TRUE);
		*pDocSelType = (DataInt*) info.m_Data.GetParamValue(szDocSelType); 
	}

	*pProfileSelType = (DataInt*) info.m_Data.GetParamValue(szProfileSelType);
	if (!*pProfileSelType)
	{
		info.m_Data.InternalAddParam(szProfileSelType, &DataInt(USE_PREDEFINED_PROFILE), TRUE);
		*pProfileSelType = (DataInt*) info.m_Data.GetParamValue(szProfileSelType);
	}

	*pSendExportedEnvelope	= (DataBool*) info.m_Data.GetParamValue(szSendExportedEnvelope);
	if (!*pSendExportedEnvelope)
	{
		info.m_Data.InternalAddParam(szSendExportedEnvelope, &DataBool(FALSE), TRUE);
		*pSendExportedEnvelope	= (DataBool*) info.m_Data.GetParamValue(szSendExportedEnvelope);	
	}

	*pProfileName = (DataStr*) info.m_Data.GetParamValue(szProfileName);
	if (!*pProfileName)
	{
		info.m_Data.InternalAddParam(szProfileName, &DataStr(), TRUE);
		*pProfileName = (DataStr*) info.m_Data.GetParamValue(szProfileName);
	}

	*pExportCriteria = (DataStr*) info.m_Data.GetParamValue(szExportCriteria);
	if (!*pExportCriteria)
	{
		info.m_Data.InternalAddParam(szExportCriteria, &DataStr(), TRUE);
		*pExportCriteria = (DataStr*) info.m_Data.GetParamValue(szExportCriteria);
	}

	*pExportPath = (DataStr*) info.m_Data.GetParamValue(szExportPath);
	if (!*pExportPath)
	{
		info.m_Data.InternalAddParam(szExportPath, &DataStr(), TRUE);
		*pExportPath = (DataStr*) info.m_Data.GetParamValue(szExportPath);
	}
}

//----------------------------------------------------------------------------------------------
void GetXMLImportParams(CExternalControllerInfo &info,	DataStr **pImportCriteria)
{
	static TCHAR szImportCriteria[]	= _T("ImportCriteria");

	*pImportCriteria = (DataStr*) info.m_Data.GetParamValue(szImportCriteria);
	if (!*pImportCriteria)
	{
		info.m_Data.InternalAddParam(szImportCriteria, &DataStr(), TRUE);
		*pImportCriteria = (DataStr*) info.m_Data.GetParamValue(szImportCriteria);
	}
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostInitDocument(const CBaseDocument* pBaseDoc, CExternalControllerInfo* pInfo, CAbstractFormDoc *&pDocument, CXMLDataManager *&pXMLDataMng, DataObjArray& arMessages)
{
	if (!pBaseDoc) 
		return FALSE;

	if (!pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		arMessages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		AfxGetTbCmdManager()->CloseDocument(pBaseDoc);
		return FALSE;
	}

	pDocument = (CAbstractFormDoc*)pBaseDoc;
	if (pDocument->GetType() == VMT_BATCH)
	{
		arMessages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		AfxGetTbCmdManager()->CloseDocument(pDocument);
		return FALSE;
	}

	pDocument->LoadXMLDescription();
	if (pInfo && pInfo->IsEditing())
		pDocument->GetMasterFrame()->ShowWindow(SW_HIDE);

	CXMLDataManagerObj *pXMLDataMngObj = pDocument->GetXMLDataManager();

	if (!pXMLDataMngObj || !pXMLDataMngObj->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
	{
		arMessages.Add(new DataStr(Messages::NO_DATA_MANAGER()));
		AfxGetTbCmdManager()->CloseDocument(pDocument);
		return FALSE;
	}

	pXMLDataMng = (CXMLDataManager*) pXMLDataMngObj;

	return TRUE;
}





//----------------------------------------------------------------------------------------------
BOOL InitDocument
			(
				const DataStr &documentNamespace, 
				const DataStr& xmlParams, 
				CExternalControllerInfo* pInfo, 
				CAbstractFormDoc*&	pDocument,
				CXMLDataManager*&	pXMLDataMng,
				DataObjArray&		arMessages,
				BOOL bCanRunOnlyBusinessObject = FALSE
			)
{
	
	if (!xmlParams.IsEmpty() && pInfo)
		pInfo->SetFromXmlString(xmlParams.GetString()); 
	
	CTBNamespace aNsDocument(documentNamespace.GetString());
	const CDocumentDescription* docDescription = AfxGetDocumentDescription(aNsDocument);
	if (!docDescription || docDescription->IsTransferDisabled())
	{ 
		arMessages.Add(new DataStr(Messages::NO_DATA_MANAGER()));
		return FALSE;
	}

	//Impr 6393
	CImportExportParams* pImpExpParams = NULL;
	if (bCanRunOnlyBusinessObject)	
		pImpExpParams = new CImportExportParams(TRUE);
		

	CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunDocument
														(
															documentNamespace.GetString(), 
															szDefaultViewMode, 
															FALSE, 
															NULL, /*pAncestror*/
															NULL, /*lpAuxInfo*/
															NULL, /*ppExistingDoc*/
															NULL, /*pFailedCode*/
															pInfo,/*pControllerInfo*/
															FALSE,/*IsRunningAsADM*/
															NULL,/*pTBContext*/
														    pImpExpParams/*pMangedParams*/
															);

	
	if (!pBaseDoc)
	{
		CLoginContext* pContext = AfxGetLoginContext();
		CStringArray messageArray;
		if (pContext)
		{
			DataBool clearMessages = TRUE;
			CDiagnostic* pDiagnostic =  NULL;
			pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(pContext->m_nThreadID, &CloneDiagnostic, clearMessages);
			pDiagnostic->ToStringArray(messageArray);	
			for (int i = 0; i < messageArray.GetSize(); i++)
				arMessages.Add(new DataStr(messageArray.GetAt(i)));
			delete pDiagnostic;
		}
		AfxGetDefaultSqlConnection()->GetDiagnostic()->ToStringArray(messageArray);	
		AfxGetDefaultSqlConnection()->GetDiagnostic()->ClearMessages(TRUE);	
		for (int i = 0; i < messageArray.GetSize(); i++)
			arMessages.Add(new DataStr(messageArray.GetAt(i)));
	

		return FALSE;
	}
	
	return AfxInvokeThreadGlobalFunction<BOOL, const CBaseDocument*, CExternalControllerInfo*, CAbstractFormDoc*&, CXMLDataManager*&, DataObjArray&>
				(
					pBaseDoc->GetThreadId(),
					&PostInitDocument,
					pBaseDoc,
					pInfo,
					pDocument,
					pXMLDataMng,
					arMessages
				);
}

//----------------------------------------------------------------------------
///<summary>
///Closes latest document used by XTech
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void CloseLatestDocument()
{
	return AfxGetXEngineObject()->CloseLatestDocument();
}

//----------------------------------------------------------------------------
///<summary>
///Returns code of operating web site
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataStr GetSiteCode()
{
	return AfxGetSiteCode();
}

//----------------------------------------------------------------------------
///<summary>
///Returns profiles list of a document
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataObjArray/*[ciString]*/ GetExportProfileList(DataStr/*[ciString]*/ documentNamespace, DataInt posType, DataStr/*[ciString]*/ userName)
{
	CTBNamespace aNamespace(documentNamespace.GetString());
	CStringArray profilesArray;
	GetAllExportProfiles(aNamespace, &profilesArray, (CPathFinder::PosType)((int)posType), userName.Str());

	DataObjArray aProfileList;

	for(int i = 0 ; i < profilesArray.GetSize() ; i++)
		aProfileList.Add(new DataStr(profilesArray.GetAt(i)));

	return aProfileList;
}

//----------------------------------------------------------------------------
///<summary>
/// Returns the folder of the export profiles of the document with specified namespace. 
///</summary>
// If
//  aPathType = 1 returns the standard path
//  aPathType = 2 returns the custom path of allusers
//  aPathType = 3 returns the custom path of specified user 
// of the current company. 
// If companyName is empty it will be consider the current connected company
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataStr	GetExportProfilesPath(DataStr/*[ciString]*/ documentNamespace, DataInt posType, DataStr/*[ciString]*/ userName)
{	
	CTBNamespace aNamespace(documentNamespace.GetString());
	return AfxGetPathFinder()->GetDocumentExportProfilesPath(aNamespace, (CPathFinder::PosType)((int)posType), userName.Str());
}

//----------------------------------------------------------------------------
///<summary>
///Opens a document and import an envelope drowing it from a specified path
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool OpenDocumentAndImport(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ envelopeFolder, DataStr& resultDescription)
{
	const CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(documentNamespace.GetString());

	if (pDoc == NULL)
	{
		resultDescription = Messages::CANT_OPEN_DOC();
		return FALSE;
	}

	BOOL bResult = AfxInvokeThreadGlobalFunction<DataBool, DataLng, DataStr, DataStr&>(pDoc->GetThreadId(), &Import, (long)pDoc, envelopeFolder, resultDescription);
	
	return AfxGetTbCmdManager()->DestroyDocument(pDoc) && bResult;
}

//----------------------------------------------------------------------------
///<summary>
///Imports an envelope drowing it from a specifed path
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool Import(DataLng documentHandle, DataStr/*[ciString]*/ envelopeFolder, DataStr& resultDescription)
{
	CObject* pObj = (CObject*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pObj) || !pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		resultDescription = Messages::INVALID_DOC_HANDLE();
		return FALSE;
	}

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pObj;
	if (pDoc->GetFormMode() != CBaseDocument::BROWSE)
	{
		resultDescription = Messages::DOC_NO_BROWSE();
		return FALSE;
	}

	CXMLDataManager *pDataMng = (CXMLDataManager*) pDoc->GetXMLDataManager();

	CString strRetVal;
	BOOL bResult = pDataMng->Import(envelopeFolder.GetString(), strRetVal);

	resultDescription = strRetVal;
	
	return bResult;
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostGetXMLExportParameters(CAbstractFormDoc *pDocument, CExternalControllerInfo& info, CXMLDataManager* pDataManager, DataStr& xmlParams, DataObjArray& messages)
{
	DataInt 	*pDocSelType = NULL; 
	DataInt 	*pProfileSelType = NULL; 
	DataBool	*pSendExportedEnvelope = NULL; 
	DataStr 	*pProfileName = NULL; 
	DataStr 	*pExportCriteria = NULL;
	DataStr 	*pExportPath = NULL;


	GetXMLExportParams
				(
					info,
					&pDocSelType, 
					&pProfileSelType, 
					&pSendExportedEnvelope, 
					&pProfileName, 
					&pExportCriteria,
					&pExportPath
				);

	CXMLProfileInfo* lpCurrentProfile = NULL; 
	BOOL bResult = pDataManager->RunExportWizard
											(
												&lpCurrentProfile,
												pProfileName->GetString(),
												pExportCriteria->GetString(),
												pExportPath->GetString(),
												*pDocSelType,
												*pProfileSelType,
												*pSendExportedEnvelope,
												pDocument
											);

	
	CXMLExportDocSelection*	pExportDocSelection = pDataManager->GetXMLExportDocSelection();
	if (pExportDocSelection)
	{
		*pSendExportedEnvelope	= pExportDocSelection->m_bSendEnvelopeNow;
		*pDocSelType			= pExportDocSelection->m_nDocSelType;
		*pProfileSelType		= pExportDocSelection->m_nProfileSelType;
		*pExportPath			= pExportDocSelection->m_strAlternativePath;
	}
 
	// mi stacco dal documento
	pDocument->m_pExternalControllerInfo = NULL;

	if (!bResult)
	{
		pDocument->GetMessages()->ToArray(messages);
		AfxGetTbCmdManager()->CloseDocument(pDocument);
		return FALSE;
	}
	
	if (!lpCurrentProfile)
	{
		pProfileName->Clear(); 
		pExportCriteria->Clear();

		// chiudo il documento	
		AfxGetTbCmdManager()->CloseDocument(pDocument);
		return TRUE;
	}

	// Se deve venire caricato il profilo preferenziale la stringa lpszProfileName 
	// deve essere vuota
	if (pExportDocSelection->AreSelProfilePresent())
		*pProfileName = lpCurrentProfile->m_strProfileName;
	else 
		pProfileName->Clear();

	// La funzione non restituisce nel nome del file sul quale sono stati salvati
	// i criteri di esportazione anche il percorso, dato che esso è comunque
	// determinato dal profilo prescelto
	*pExportCriteria = GetNameWithExtension(lpCurrentProfile->m_strExpCriteriaFileName);

	// mi stacco dal documento
	info.m_ControllingMode = CExternalControllerInfo::NONE;
	// quindi lo chiudo
	AfxGetTbCmdManager()->CloseDocument(pDocument);

	xmlParams = info.GetXmlString();
	
	return TRUE;
}



//----------------------------------------------------------------------------------------------
///<summary>
///Sets export parameters
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetXMLExportParameters	(DataStr/*[ciString]*/	documentNamespace, DataStr& xmlParams, DataObjArray&/*[string]*/ messages, DataStr code)
{
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::EDITING;
	info.m_code = code;
	CAbstractFormDoc *pDocument = NULL;
	CXMLDataManager *pDataManager = NULL;

	if (!InitDocument(documentNamespace, xmlParams, &info, pDocument, pDataManager, messages))
		return FALSE; 


	return AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, CExternalControllerInfo&, CXMLDataManager*, DataStr&, DataObjArray&>
									(
										pDocument->GetThreadId(), 
										&PostGetXMLExportParameters,
										pDocument,
										info,
										pDataManager,
										xmlParams,
										messages
									 );
}	

//----------------------------------------------------------------------------------------------
BOOL PostGetXMLImportParameters(CAbstractFormDoc *pDocument, CExternalControllerInfo& info, CXMLDataManager* pDataManager, DataStr& xmlParams, DataObjArray& messages)
{
	BOOL bResult = TRUE;

	//there is import criteria to set
	if (pDocument->GetBaseImportCriteria())
	{
		DataStr* pImportCriteria = NULL;
		GetXMLImportParams(info, &pImportCriteria);

		BOOL bResult = pDataManager->RunImportWizard(NULL, pImportCriteria->GetString(), pDocument);
		*pImportCriteria = GetNameWithExtension(pDataManager->GetAppImportCriteriaFileName());

		// mi stacco dal documento
		info.m_ControllingMode = CExternalControllerInfo::NONE;
		// quindi lo chiudo
		pDocument->OnCloseDocument();
	
		if (bResult && !pImportCriteria->IsEmpty())
			xmlParams = info.GetXmlString();
	}
	else
	{
		// mi stacco dal documento
		info.m_ControllingMode = CExternalControllerInfo::NONE;
		// quindi lo chiudo
		pDocument->OnCloseDocument();
		xmlParams.Clear();
		messages.Add(new DataStr(Messages::XML_NO_PARAMETERS()));
	}

	return bResult;
}

//----------------------------------------------------------------------------------------------
///<summary>
///Sets export parameters
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetXMLImportParameters	(DataStr/*[ciString]*/ documentNamespace, DataStr& xmlParams, DataObjArray&/*[string]*/ messages, DataStr code)
{	
	
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::EDITING;
	info.m_code = code;

	CAbstractFormDoc *pDocument = NULL;
	CXMLDataManager *pDataManager = NULL;
	
	if (!InitDocument(documentNamespace, xmlParams, &info, pDocument, pDataManager, messages))
		return FALSE; 
	
	return AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, CExternalControllerInfo&, CXMLDataManager*, DataStr&, DataObjArray&>
									(
										pDocument->GetThreadId(), 
										&PostGetXMLImportParameters,
										pDocument,
										info,
										pDataManager,
										xmlParams,
										messages
									 );
	
}


// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostRunXMLExportInUnattendedMode (BOOL bInitDocumentDone, CAbstractFormDoc *pDocument, DataObjArray&/*[string]*/ messages, CExternalControllerInfo& info, CXMLDataManager* pDataManager, DataLng& documentHandle)
{
	if (!bInitDocumentDone)
	{
		if (pDocument)
			pDocument->GetMessages()->ToArray(messages);
		messages.Add(new DataStr(Messages::CANT_OPEN_DOC()));
		return FALSE; 
	}

	DataInt 	*pDocSelType = NULL; 
	DataInt 	*pProfileSelType = NULL; 
	DataBool	*pSendExportedEnvelope = NULL; 
	DataStr 	*pProfileName = NULL; 
	DataStr 	*pExportCriteria = NULL;
	DataStr		*pExportPath = NULL;

	GetXMLExportParams
				(
					info,
					&pDocSelType, 
					&pProfileSelType, 
					&pSendExportedEnvelope, 
					&pProfileName, 
					&pExportCriteria,
					&pExportPath
				);

	pDataManager->SetUnattendedExportParams
									(
										pProfileName->GetString(), 
										pExportCriteria->GetString(), 
										pExportPath->GetString(),
										*pDocSelType, 
										*pProfileSelType, 
										*pSendExportedEnvelope										
									);


	BOOL retVal = pDataManager->Export() && 
				(
					(info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS) ||
					(info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS_WITH_INFO)
				);

	documentHandle = (long) pDocument;
	pDocument->GetMessages()->ToArray(messages);
	pDocument->UpdateDataView();

	return retVal;
}

//----------------------------------------------------------------------------------------------
///<summary>
///Performs XML import
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool RunXMLExportInUnattendedMode (DataStr/*[ciString]*/ documentNamespace, DataStr xmlParams, DataLng& documentHandle, DataObjArray&/*[string]*/ messages)
{
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::RUNNING;

	CAbstractFormDoc *pDocument = NULL;
	CXMLDataManager *pDataManager = NULL;

	
	BOOL bInitDocumentDone = InitDocument(documentNamespace, xmlParams, &info, pDocument, pDataManager, messages);
	
	DataBool b = AfxInvokeThreadGlobalFunction<BOOL, BOOL, CAbstractFormDoc*, DataObjArray&, CExternalControllerInfo&, CXMLDataManager*, DataLng&>
									(
										pDocument->GetThreadId(), 
										&PostRunXMLExportInUnattendedMode,
										bInitDocumentDone,
										pDocument,
										messages,
										info, 
										pDataManager,
										documentHandle
									 );
	pDocument->m_pExternalControllerInfo = NULL;

	return b;
}


// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostRunXMLImportInUnattendedMode(BOOL bInitDocumentDone, CAbstractFormDoc *pDocument, DataObjArray&/*[string]*/ messages, CExternalControllerInfo& info, CXMLDataManager* pDataManager, DataBool downloadEnvelopes, DataBool validateData, DataLng& documentHandle)
{

	if (!bInitDocumentDone)
	{
		if (pDocument)
			pDocument->GetMessages()->ToArray(messages);
		messages.Add(new DataStr(Messages::CANT_OPEN_DOC()));
		return FALSE; 
	}

	DataStr* pImportCriteria = NULL;
	GetXMLImportParams(info, &pImportCriteria);


	pDataManager->SetUnattendedImportParams
									(
										downloadEnvelopes,
										validateData,
										pImportCriteria->GetString()
									);

	BOOL retVal = pDataManager->Import() && 
				(
					(info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS) ||
					(info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS_WITH_INFO)
				);

	documentHandle = (long) pDocument;
	pDocument->GetMessages()->ToArray(messages);
	pDocument->UpdateDataView();

	return retVal;
}


//----------------------------------------------------------------------------------------------
///<summary>
///Performs XML Export
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool RunXMLImportInUnattendedMode (DataStr/*[ciString]*/ documentNamespace, DataBool downloadEnvelopes, DataBool validateData, DataStr xmlParams, DataLng& documentHandle, DataObjArray&/*[string]*/ messages)
{
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::RUNNING;

	CAbstractFormDoc *pDocument = NULL;
	CXMLDataManager *pDataManager = NULL;

	BOOL bInitDocumentDone = InitDocument(documentNamespace, xmlParams, &info, pDocument, pDataManager, messages);
	
	DataBool b = AfxInvokeThreadGlobalFunction<BOOL, BOOL, CAbstractFormDoc*, DataObjArray&, CExternalControllerInfo&, CXMLDataManager*,DataBool, DataBool, DataLng&>
									(
										pDocument->GetThreadId(), 
										&PostRunXMLImportInUnattendedMode,
										bInitDocumentDone,
										pDocument,
										messages,
										info, 
										pDataManager,
										downloadEnvelopes,
										validateData,
										documentHandle
									 );

	pDocument->m_pExternalControllerInfo = NULL;
	
	return b;
}

//----------------------------------------------------------------------------------------------
//		Gestione SMART IMPORT\EXPORT
//----------------------------------------------------------------------------------------------
//


//----------------------------------------------------------------------------------------------
void GetDocumentInformation(CXMLDocumentObject* pXmlDocument, CString& strNamespace,  CString& strProfile, CPathFinder::PosType& ePosType, CString& strUserName)
{	
	
	if (!pXmlDocument)
		return;

	CXMLNode* root = pXmlDocument->GetRoot();
	if (!root || !root->GetAttribute(DOC_XML_TBNAMESPACE_ATTRIBUTE, strNamespace))
		return;

	GetInformationFromNsUri(pXmlDocument->GetNamespaceURI(), strProfile, ePosType, strUserName);	
}

//----------------------------------------------------------------------------------------------
void GetXMLFilesFormPath(CStringArray* pXMLParamsFiles, const CString& strFilePath)
{
	CString strFileName;

	CStringArray arFiles;
	AfxGetFileSystemManager ()->GetFiles (strFilePath, _T("*.xml"), &arFiles);

	for (int i=0; i <= arFiles.GetUpperBound(); i++)
	{    
		strFileName = arFiles.GetAt(i);
		if (!strFileName.IsEmpty())
			pXMLParamsFiles->Add(strFileName);
	}
}

//----------------------------------------------------------------------------------------------
int GetTBDocument(CAbstractFormDoc*& pDocument, CXMLDataManager*& pDataManager, const CString& strDocNamespace, CSmartXMLDiagnosticMng& xmlDiagnosticMng, BOOL bCanRunOnlyBusinessObject = FALSE)
{
	int bRetVal = CREATE_DOC_FAILED;

	DataObjArray messages;
	CBaseDocument* pExistingDoc = AfxGetXEngineObject()->GetCachedDocument(strDocNamespace);
	if (pExistingDoc)
	{
		pDocument = (CAbstractFormDoc*)pExistingDoc;
		pDataManager = (CXMLDataManager*)pExistingDoc->GetXMLDataManager();
		bRetVal = USE_CACHED_DOC;
		TRACE1 ("Using existing document: %s. \n", strDocNamespace);
	}
	else
	{
		if (InitDocument(strDocNamespace, _T(""), NULL, pDocument, pDataManager, messages, bCanRunOnlyBusinessObject))
		{
			bRetVal = (AfxGetXEngineObject()->AddCachedDocument(pDocument))? CREATE_AND_CACHE_DOC : CREATE_NEW_DOC;
			TRACE1 ("Creating document: %s. \n", strDocNamespace);
		}
		else
			bRetVal = CREATE_DOC_FAILED;
	}

	if (bRetVal == CREATE_DOC_FAILED)
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::CANT_OPEN_DOC());
		CString strMsg;
		for (int nIdx = 0; nIdx < messages.GetSize(); nIdx++)
		{
			strMsg += messages.GetAt(nIdx)->Str();
			strMsg += _T("\n");
		}
		if (!strMsg.IsEmpty())
			xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, strMsg);
	}

	return bRetVal;
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostLoadSingleDocument(CAbstractFormDoc* pDocument, CXMLDataManager* pDataManager, CSmartXMLDiagnosticMng& xmlDiagnosticMng, int nRetVal, CXMLDocumentObject* pXMLParamDoc, CSmartExportParams* pSmartExpParams)
{
	BOOL bOK = FALSE;
		
#ifndef DEBUG
	try
	{
#endif
		if (nRetVal != CREATE_DOC_FAILED)
		{
			pDocument->EnableControlsForFind();
			bOK = pDataManager->ExportSmartDocument(pXMLParamDoc, pSmartExpParams);
			pDocument->UpdateDataView();
			if (nRetVal == CREATE_NEW_DOC)
			{
				pDocument->m_pExternalControllerInfo = NULL;
				AfxGetTbCmdManager()->CloseDocument(pDocument);	
				pDocument = NULL;
				return bOK;
			}
		}	

		if (pDocument)
			pDocument->GetXMLDataManager()->SetCachedDocumentBusy(false);
#ifndef DEBUG
	}
	catch(CException *e)
	{
		AfxGetDiagnostic()->Add(e);
		e->Delete();
		bOK = FALSE;
	}
	catch(...)
	{
		AfxGetDiagnostic()->Add((CException*) NULL);
		bOK = FALSE;
	}
#endif
	
	return bOK;
}

//----------------------------------------------------------------------------------------------
BOOL CanRunOnlyBusinessObject(const CString& strDocNamespace, const CString& strProfile, CPathFinder::PosType ePosType, const CString& strUserName)
{
	CString strProfilePath = ::GetProfilePath(strDocNamespace, strProfile, ePosType, strUserName);

	if (!::ExistPath(strProfilePath))
		return false;
	/*{
	xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, cwsprintf(_TB("Export profile {0-%s} not present in the current installation."), strProfile));
	return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	};
	*/

	CXMLProfileInfo profileInfo(strDocNamespace, strProfilePath);
	return profileInfo.CanRunOnlyBusinessObject();
}

//----------------------------------------------------------------------------------------------
BOOL LoadSingleDocument(const CString& strParam, CSmartExportParams* pSmartExpParams)
{
	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	CXMLDocumentObject xmlDocument;
	CXMLNode* pRoot = NULL;
	
#ifndef DEBUG
	try
	{
#endif

	// considero la root del xmldocument, root che rappresenta l'entità documento di MagoNet
	// alla root è associato l'attributo namespace che fornisce il namespace del documento
	if (xmlDocument.LoadXML(strParam))
		pRoot = xmlDocument.GetRoot();

	if (!pRoot)
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_NOPARAMS_FOUND());
		pSmartExpParams->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(NULL, FALSE));
		return FALSE;
	}
	
	CString strDocNamespace;
	CString strProfile, strUserName;
	CPathFinder::PosType ePosType;

	GetDocumentInformation(&xmlDocument, strDocNamespace, strProfile, ePosType, strUserName);
	if (!strDocNamespace.IsEmpty())
	{	
		CAbstractFormDoc* pDocument = NULL;
		CXMLDataManager*  pDataManager = NULL;
		
		int nRetVal = GetTBDocument(pDocument, pDataManager, strDocNamespace, xmlDiagnosticMng, CanRunOnlyBusinessObject(strDocNamespace, strProfile, ePosType, strUserName));
		if (pDocument)
		{
			BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, CXMLDataManager*, CSmartXMLDiagnosticMng&, int, CXMLDocumentObject*, CSmartExportParams*>
						(
							pDocument->GetThreadId(),
							&PostLoadSingleDocument,
							pDocument,
							pDataManager,
							xmlDiagnosticMng,
							nRetVal,
							&xmlDocument,
							pSmartExpParams
						);
			
			return bOk;
		}
		else
		{
			pSmartExpParams->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, FALSE));
			return FALSE;	
		}
	}	
	xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_NAMESPACE_STRING());
	pSmartExpParams->AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, FALSE));
	return FALSE;

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

//----------------------------------------------------------------------------------------------
///<summary>
///Performs XML export using the document namespace and the research values presents in the specified file.
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetData(DataStr/*[ciString]*/ param, DataBool useApproximation, DataStr/*[ciString]*/ loginName, DataArray&/*[string]*/ result)
{
	AfxGetThreadContext()->SetSessionLoginName(loginName.Str());

	CSmartExportParams smartExpParams(&result, useApproximation);

	return LoadSingleDocument(param.Str(), &smartExpParams);
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
BOOL PostSaveSingleDocument(CAbstractFormDoc* pDocument, CXMLDataManager* pDataManager, CSmartXMLDiagnosticMng& xmlDiagnosticMng, int nRetVal, const CString& strData, CSmartImportParams* pSmartImpParams)
{
	BOOL bOK = FALSE;
#ifndef DEBUG
	try
	{
#endif
		if (nRetVal != CREATE_DOC_FAILED)	
		{
			pDocument->EnableControlsForFind();
			bOK = pDataManager->ImportSmartDocument(strData, pSmartImpParams);
			if (nRetVal == CREATE_NEW_DOC)
			{
				//Se la chiamata è fallita, aggiunge la diagnostica di errore
				if (!bOK && pDocument)
					xmlDiagnosticMng.InsertDocumentMessage(pDocument->GetMessages());

				AfxGetTbCmdManager()->CloseDocument(pDocument);	
				pDocument->m_pExternalControllerInfo = NULL;
				pDocument = NULL;
				return bOK;
			}
		}
		else
			xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::CANT_OPEN_DOC());
		
		//Se la chiamata è fallita, aggiunge la diagnostica di errore
		if (!bOK && pDocument)
			xmlDiagnosticMng.InsertDocumentMessage(pDocument->GetMessages());

		//In ogni caso pulisce i messaggi e setta il documento come riusabile
		if (pDocument)
		{
			pDocument->GetMessages()->ClearMessages();
			pDocument->GetXMLDataManager()->SetCachedDocumentBusy(false);
		}
	
#ifndef DEBUG
	}
	catch(CException *e)
	{
		AfxGetDiagnostic()->Add(e);
		e->Delete();
		bOK = FALSE;
	}
	catch(...)
	{
		AfxGetDiagnostic()->Add((CException*) NULL);
		bOK = FALSE;
	}
#endif

	return bOK;
}


//----------------------------------------------------------------------------------------------
///<summary>
///Performs the XML import of the files in the folder fromFolder\\Data
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool SetData(DataStr/*[ciString]*/ data, DataInt saveAction, DataStr loginName, DataStr& result)
{
	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	DataArray arResult;
	CSmartImportParams smartImpParams(&arResult, saveAction);
#ifndef DEBUG
	try
	{
#endif
		AfxGetThreadContext()->SetSessionLoginName(loginName.Str());

		// considero la root del xmldocument, root che rappresenta l'entità documento di MagoNet
		// alla root è associato l'attributo namespace che fornisce il namespace del documento
		CXMLDocumentObject xmlDocument;
		CXMLNode* pRoot = NULL;
		if (xmlDocument.LoadXML(data.Str()))
			pRoot = xmlDocument.GetRoot();
		if (!pRoot)
		{
			xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_STRING());
			smartImpParams.AddToResult(xmlDiagnosticMng.CreateXMLErrorString(NULL, FALSE));
			return FALSE;
		}

		CString strDocNamespace;
		CString strProfile, strUserName;
		CPathFinder::PosType ePosType;

		GetDocumentInformation(&xmlDocument, strDocNamespace, strProfile, ePosType, strUserName);
	
		if (!strDocNamespace.IsEmpty())
		{
			CAbstractFormDoc *pDocument = NULL;
			CXMLDataManager*  pDataManager = NULL;
			int nRetVal = GetTBDocument(pDocument, pDataManager, strDocNamespace, xmlDiagnosticMng, CanRunOnlyBusinessObject(strDocNamespace, strProfile, ePosType, strUserName));
			if (pDocument)
			{
				BOOL bOk = AfxInvokeThreadGlobalFunction<BOOL, CAbstractFormDoc*, CXMLDataManager*, CSmartXMLDiagnosticMng&, int, const CString&, CSmartImportParams*>
							(
								pDocument->GetThreadId(),
								&PostSaveSingleDocument,
								pDocument,
								pDataManager,
								xmlDiagnosticMng,
								nRetVal,
								data,
								&smartImpParams							
							);
				result.Assign(smartImpParams.GetFirstStringResult());
				DWORD d = GetGuiResources(GetCurrentProcess(), GR_GDIOBJECTS);
				TRACE(_T("----- resources: %d \r\n"), d);
				return bOk;
			}			
			else
			{
				smartImpParams.AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, FALSE));
				result.Assign(smartImpParams.GetFirstStringResult());
				DWORD d = GetGuiResources(GetCurrentProcess(), GR_GDIOBJECTS);
				TRACE(_T("----- resources failed 1: %d \r\n"), d);
				return FALSE;
			}
			
		}
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_NAMESPACE_STRING());
		smartImpParams.AddToResult(xmlDiagnosticMng.CreateXMLErrorString(pRoot, FALSE));	
		result.Assign(smartImpParams.GetFirstStringResult());
		DWORD d = GetGuiResources(GetCurrentProcess(), GR_GDIOBJECTS);
		TRACE(_T("----- resources failed 2: %d \r\n"), d);
		return FALSE;
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

//----------------------------------------------------------------------------------------------

///  Metodo per la richiesta dei parametri in formato xml associati ad un profilo. I parametri sono salvati con il loro
// valore attuale. All'interno di paramsFolder\\Params ci possono essere uno o più file xml, ciascuno contenente il namespace e il profilo
// da utilizzare per la singola richiesta di parametri. La procedura crea i file di parametri e li inserisce nella
// directory paramsFolder\\Data
///<summary>
///Returns the research parameters linked to the specified export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetXMLParameters(DataStr/*[ciString]*/ param, DataBool useApproximation, DataStr/*[ciString]*/ loginName, DataStr& result)
{
	AfxGetThreadContext()->SetSessionLoginName(loginName.Str());
	DataArray arResult;
	CSmartExportParams smartExpParams(&arResult, useApproximation, EXPORT_ACTION_ONLY_PARAMS);
	BOOL bRet = LoadSingleDocument(param, &smartExpParams);	
	result.Assign(smartExpParams.GetFirstStringResult());	
	return bRet;
}


//----------------------------------------------------------------------------------------------
void ChangeDocumentFields(CXMLNodeChildsList* pChilds, CXMLDBTInfo* pDBTInfo, CXMLXRefInfo* pExtRefInfo, const CString& strPrefix)
{
	if (pChilds)
	{
		CString strXRefPrefix = strPrefix;
		CString strComplex;
		CString strDocField;
		CXMLNode* pNode = NULL;
		
		if (pExtRefInfo)
		{
			CString strDBTName = pDBTInfo->GetNamespace().GetObjectNameForTag();
			if (pDBTInfo->GetType() == CXMLDBTInfo::BUFFERED_TYPE)
			{
				CString strSlave = strDBTName;
				strDBTName += szNs;
				strDBTName +=  strSlave + XML_ROW_TAG;
			}
			strXRefPrefix += szNs + strDBTName + szNs + pExtRefInfo->GetName();
		}

		CString strXRefField;
		for (int nIdx = 0; nIdx <= pChilds->GetUpperBound(); nIdx++)
		{
			
			pNode = pChilds->GetAt(nIdx);
			pNode->GetAttribute(XML_HKL_DOCFIELD_ATTRIBUTE, strDocField);
			strDocField.Replace(_T("/"), szNs);
			pNode->GetAttribute(XML_HKL_FROMXREF_ATTRIBUTE, strXRefField);
			strComplex = (GetBoolFromXML(strXRefField)) ? strXRefPrefix : strPrefix;
			strComplex += szNs + strDocField;
			pNode->SetAttribute(XML_HKL_DOCFIELD_ATTRIBUTE, strComplex);				
		}
		SAFE_DELETE(pChilds); 
	}	
}

//----------------------------------------------------------------------------------------------
DataStr CreateXMLHKLString(CXMLHotKeyLink* pHKLInfo, CXMLDBTInfo* pDBTInfo, CXMLXRefInfo* pExtRefInfo, const CString& strXPath, const CString& strField)
{
	if (!pHKLInfo)
		return _T("");

	CXMLDocumentObject	aDoc(TRUE, FALSE);
	CXMLNode* pRoot = aDoc.CreateRoot(XML_HKL_FIELD_TAG);
	if (!pRoot)
		return _T("");

	CXMLNodeChildsList* pChilds = NULL;
	CString strValue;
	CString strComplex = strXPath;

	if (pHKLInfo->UnParse(pRoot))
	{
		pRoot->SetAttribute(XML_HKL_NAME_ATTRIBUTE, strField);
		pChilds = pRoot->SelectNodes(cwsprintf(_T("{0-%s}/{1-%s}"), XML_HKL_FILTERS_TAG, XML_HKL_FILTER_TAG));
		ChangeDocumentFields(pChilds, pDBTInfo, pExtRefInfo, strComplex);
		pChilds = pRoot->SelectNodes(cwsprintf(_T("{0-%s}/{1-%s}"), XML_HKL_RESULTS_TAG, XML_HKL_RESULT_TAG));
		ChangeDocumentFields(pChilds, pDBTInfo, pExtRefInfo, strComplex);

		if (pRoot->GetXML(strValue))
			return DataStr(strValue);
	}

	return _T("");
}

//returns the HotLink definition of the specified field and exportprofile
//fieldXPath can be:
// Document/Data/DBT/	DBTNameRow/			ExtRefField/	ExtRefDBT/		DBTNameRow/		Field
// Document/Data/DBT/	DBTNameRow/			ExtRefField/	ExtRefDBT/		DBTNameRow
// Document/Data/DBT/	DBTNameRow/			ExtRefField/	ExtRefDBT/		Field
// Document/Data/DBT/	DBTNameRow/			ExtRefField/	ExtRefDBT
// Document/Data/DBT/	ExtRefField/		ExtRefDBT/		DBTNameRow/		Field
// Document/Data/DBT/	ExtRefField/		ExtRefDBT/		DBTNameRow
// Document/Data/DBT/	ExtRefField/		ExtRefDBT/		Field
// Document/Data/DBT/	ExtRefField/		ExtRefDBT
// Document/Data/DBT/	DBTNameRow/			Field
// Document/Data/DBT/	DBTNameRow
// Document/Data/DBT/	Field
// Document/Data/DBT

//----------------------------------------------------------------------------------------------
CXMLHotKeyLink* GetHKLInfoForExtReference(CXMLXRefInfo* pExtRefInfo, CXMLDBTInfo* pDBTInfo, CStringArray* pTokens)
{
	if (!pExtRefInfo || !pDBTInfo)
		return NULL;

	CString strComplete;
	for (int i = 0; i <= pTokens->GetUpperBound(); i++)
		strComplete += URL_SLASH_CHAR + pTokens->GetAt(i);
	
	strComplete = pExtRefInfo->GetName() + strComplete;
	CXMLHotKeyLink::HKLFieldType eSubType = CXMLHotKeyLink::XREF;

	if (pTokens->GetSize() == 1)
		eSubType = CXMLHotKeyLink::DBT;
	else
	{
		CString strDBTName = pTokens->GetAt(0);
		if (pTokens->GetSize() == 2)
			eSubType = (pTokens->GetAt(1).CompareNoCase(strDBTName + XML_ROW_TAG) == 0) ? CXMLHotKeyLink::DBT : CXMLHotKeyLink::FIELD;
		else
			if (pTokens->GetSize() == 3)
				eSubType = CXMLHotKeyLink::FIELD;	
	}
	return pDBTInfo->GetHKLByFieldName(strComplete, CXMLHotKeyLink::XREF,  eSubType);
}
///<summary>
///Returns the HotLink description linked to the specified field.
///</summary>
//----------------------------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataStr GetXMLHotLink(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ nsUri, DataStr/*[ciString]*/ fieldXPath, DataStr/*[ciString]*/ loginName)
{
	AfxGetThreadContext()->SetSessionLoginName(loginName.Str());

	CSmartXMLDiagnosticMng xmlDiagnosticMng;

	CTBNamespace nsDocument(documentNamespace.Str()); 	
	if (!nsDocument.IsValid())
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_NAMESPACE_STRING());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	CString strField(fieldXPath.Str());
	
	int nCurrPos = 0;
	CString strToken (_T("/"));
	CString strElem;
	CStringArray arTokens;

	while (nCurrPos < strField.GetLength())
	{
		strElem = strField.Tokenize(strToken, nCurrPos);
		int nPos = strElem.Find(_T("ns:"));
		if (nPos >= 0)
			strElem = strElem.Right(strElem.GetLength() - 3);
		arTokens.Add(strElem);
	}
	CString strProfile, strUserName;
	CPathFinder::PosType ePosType;

	GetInformationFromNsUri(nsUri.Str(), strProfile, ePosType, strUserName);

	CString strProfilePath = ::GetProfilePath(nsDocument, strProfile, ePosType, strUserName);
	
	if (arTokens.GetSize() < 2 || arTokens.GetAt(2).IsEmpty() || !::ExistPath(strProfilePath))
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, cwsprintf(_TB("Export profile {0-%s} not present in the current installation."), strProfile));
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	};

	AfxGetTbCmdManager()->LoadNeededLibraries(nsDocument);
	if (AfxGetDiagnostic()->ErrorFound())
	{
		xmlDiagnosticMng.InsertDocumentMessage(AfxGetDiagnostic());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}
			
	CXMLProfileInfo profileInfo(nsDocument, strProfilePath);
	profileInfo.LoadAllFiles();
	if (!profileInfo.GetDBTInfoArray())
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, cwsprintf(_TB("The document {0-%s} has a wrong description."), nsDocument.ToString()));
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	CXMLDBTInfo* pDBTInfo = profileInfo.GetDBTInfoArray()->GetDBTByName(arTokens.GetAt(2));
	if (!pDBTInfo)
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, cwsprintf(_TB("The document {0-%s} has a wrong description."), nsDocument.ToString()));
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}


    CXMLHotKeyLink* pHKLInfo = NULL;
	CXMLXRefInfo* pExtRefInfo = NULL;
	CString strFieldName;
	CString strXPath = szNs + arTokens[0] + szNs + _T("Data");
	
	if (arTokens.GetSize() == 3 || (arTokens.GetSize() == 4 && pDBTInfo->m_eType == CXMLDBTInfo::BUFFERED_TYPE))
		pHKLInfo = pDBTInfo->GetHKLByFieldName(pDBTInfo->GetNamespace().GetObjectNameForTag(), CXMLHotKeyLink::DBT);
	else
	{
		int nStart = (pDBTInfo->m_eType == CXMLDBTInfo::BUFFERED_TYPE) ? 4 : 3;
		strFieldName =  arTokens[nStart];
		for (int nIdx = nStart; nIdx >= 0; nIdx--)
			arTokens.RemoveAt(nIdx);
		if (arTokens.GetSize() <= 0 )
		{
			pExtRefInfo = pDBTInfo->GetXRefByName(strFieldName);
			if (pExtRefInfo && pExtRefInfo->IsToUse())				
				pHKLInfo = pDBTInfo->GetHKLByFieldName(strFieldName, CXMLHotKeyLink::XREF, CXMLHotKeyLink::XREF);
			else
				pHKLInfo = pDBTInfo->GetHKLByFieldName(strFieldName, CXMLHotKeyLink::FIELD);
		}
		else
		{
			pExtRefInfo = pDBTInfo->GetXRefByName(strFieldName);
			if (pExtRefInfo && pExtRefInfo->IsToUse())
				pHKLInfo = GetHKLInfoForExtReference(pExtRefInfo, pDBTInfo, &arTokens);
		}
	}
	return CreateXMLHKLString(pHKLInfo, pDBTInfo, pExtRefInfo, strXPath, strField);
}

///<summary>
///Allows to create a new export profile
///</summary>
//----------------------------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool NewExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ newProfileName, DataInt posType, DataStr/*[ciString]*/ userName, DataStr&/*[ciString]*/ profilePath)
{
	profilePath.Assign(_T(""));

	CTBNamespace nsDoc(documentNamespace.Str());
	if (!nsDoc.IsValid())
		return FALSE;

	CXMLProfileInfo* pXMLEmptyProfile = new CXMLProfileInfo(nsDoc);
	pXMLEmptyProfile->LoadAllFiles();
	pXMLEmptyProfile->SetXRefExportFlag(FALSE);
	pXMLEmptyProfile->SetMaxDocument(::AfxGetParameters()->f_MaxDoc);
	pXMLEmptyProfile->SetMaxDimension(::AfxGetParameters()->f_MaxKByte);

	pXMLEmptyProfile->m_strProfileName = newProfileName.Str();
	pXMLEmptyProfile->m_strDocProfilePath = ::GetProfilePath(nsDoc, newProfileName.Str(), (CPathFinder::PosType)((int)posType), userName.Str());
	pXMLEmptyProfile->m_ePosType = (CPathFinder::PosType)((int)posType);
	pXMLEmptyProfile->m_strUserName = userName.Str();

	pXMLEmptyProfile->m_bIsLoaded = TRUE;
	pXMLEmptyProfile->m_bNewProfile = TRUE;
	pXMLEmptyProfile->SetModified(TRUE);

	CProfileWizMasterDlg dlg(pXMLEmptyProfile);
	if (dlg.DoModal() != IDCANCEL)
	{
		pXMLEmptyProfile->m_bNewProfile = FALSE;
		//set the output parameter
		profilePath.Assign(pXMLEmptyProfile->m_strDocProfilePath);
		delete pXMLEmptyProfile;
		return TRUE;
	}
	delete pXMLEmptyProfile;
	
	return FALSE;
}
///<summary>
///Allows to clone an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool CloneExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr&/*[ciString]*/ profilePath, DataInt posType, DataStr/*[ciString]*/ userName)
{	
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	//carico le informazioni del profilo
	CXMLProfileInfo* pXMLProfile = new CXMLProfileInfo(nsDocument, strPath);
	pXMLProfile->LoadAllFiles();
	CString strOldSchemaName = pXMLProfile->m_strSchemaFileName;

	//calcolo la path di salvataggio del profilo clonato considerando posType e usrUser
	CString	strClonePath = ::GetProfilePath(nsDocument, ::GetName(strPath), (CPathFinder::PosType)((int)posType), userName.Str(), FALSE);

	if (strClonePath.IsEmpty())
		return FALSE;	
	
	strClonePath.Format(_T("%s_Clone"), strClonePath);
	CString strNewPath = strClonePath;
	CString strNewName, strTempName;
	strTempName.Format(_T("%s_Clone"), pXMLProfile->GetName());
	
	int nCount = 0;
	//calcolo il nuovo nome partendo dal nome del profilo e controllando gli eventuali profili presenti nella directory clonata
	while(ExistPath(strNewPath))
		strNewPath.Format(_T("%s_%d"), strClonePath, ++nCount);	
	if (nCount > 0)
		strNewName.Format(_T("%s_%d"), strTempName, nCount);
	else
		strNewName = strTempName;

	pXMLProfile->SetModified();
	pXMLProfile->SaveProfile(strNewPath, strNewName);
	
	pXMLProfile->ModifySmartDocumentSchema(strOldSchemaName);
	profilePath.Assign(pXMLProfile->m_strDocProfilePath);
	delete pXMLProfile;			
	return TRUE;
}
///<summary>
///Allows to remove an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool DeleteExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ profilePath)
{	
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	//load the profile information using the specified profilePath
	CXMLProfileInfo* pXMLProfile = new CXMLProfileInfo(nsDocument, strPath);
	pXMLProfile->LoadAllFiles();
	CString strProfileName = pXMLProfile->GetProfileNameFromPath(strPath);

	BOOL bSuccess = pXMLProfile->RemoveProfilePath();
	
	//gestione del profilo preferenziale
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strPath);
	CString strUserName;
	if (ePosType == CPathFinder::USERS)
		strUserName = AfxGetPathFinder()->GetUserNameFromPath(strPath);

	if (!ExistProfile(nsDocument, strProfileName))
	{
		CXMLDefaultInfo* pDefaultInfo = new CXMLDefaultInfo(nsDocument);
		pDefaultInfo->Parse(ePosType, strUserName);
		if (pDefaultInfo->IsLoaded())
		{
			if (strProfileName.CompareNoCase(pDefaultInfo->GetPreferredProfile()) == 0)
			{
				if ( ePosType == CPathFinder::STANDARD)
				{
					pDefaultInfo->SetPreferredProfile(_T(""));
					pDefaultInfo->UnParse();				
				}
				else //cancello il file
					DeleteFile (pDefaultInfo->GetFileName());
			}
		}
		delete pDefaultInfo;
	}
	
	delete pXMLProfile;
	return bSuccess;
}
///<summary>
///Allows to rename an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool RenameExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr&/*[ciString]*/ profilePath, DataStr/*[ciString]*/ newName)
{	
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	//load the profile information using the specified profilePath
	CXMLProfileInfo* pXMLProfile = new CXMLProfileInfo(nsDocument, strPath);
	pXMLProfile->LoadAllFiles();
	CString strOldProfName = pXMLProfile->GetName();

	BOOL bSuccess = pXMLProfile->RenameProfile(newName.Str());

	//gestione profilo preferenziale
	CXMLDefaultInfo* pDefaultInfo = new CXMLDefaultInfo(nsDocument);
	pDefaultInfo->Parse(pXMLProfile->m_ePosType, pXMLProfile->m_strUserName);
	CString strPrefProf = pDefaultInfo->GetPreferredProfile();
	if (!strPrefProf.IsEmpty() && strOldProfName.CompareNoCase(strPrefProf) == 0)
	{
		pDefaultInfo->SetPreferredProfile(newName.Str());
		pDefaultInfo->UnParse();
	}
	delete pDefaultInfo;

	profilePath.Assign(pXMLProfile->m_strDocProfilePath);
	delete pXMLProfile;
	return bSuccess;
}
///<summary>
///Allows to modify an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool ModifyExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr&/*[ciString]*/ profilePath, DataInt posType, DataStr/*[ciString]*/ userName)
{
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	CXMLProfileInfo* pProfileInfo = new CXMLProfileInfo(nsDocument, strPath);
	pProfileInfo->LoadAllFiles();
	
	// modifico la path di memorizzazione in base al pos type e allo strUser mandatomi
	pProfileInfo->m_strDocProfilePath = ::GetProfilePath(nsDocument, pProfileInfo->GetName(), (CPathFinder::PosType)((int)posType), userName.Str());

	CProfileWizMasterDlg dlg(pProfileInfo);
	if (dlg.DoModal() != IDCANCEL)
	{
		profilePath.Assign(pProfileInfo->m_strDocProfilePath);
		delete pProfileInfo;
		return TRUE;
	}
	delete pProfileInfo;

	return FALSE;
}

///<summary>
///Allows to display an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool ShowExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ profilePath)
{
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	CXMLProfileInfo* pProfileInfo = new CXMLProfileInfo(nsDocument, strPath);
	pProfileInfo->LoadAllFiles();
	pProfileInfo->SetReadOnly(TRUE);
	CProfileWizMasterDlg dlg(pProfileInfo);
	dlg.DoModal();
	delete pProfileInfo;

	return TRUE;

}

//----------------------------------------------------------------------------------------------
void CopySingleProfile(const CTBNamespace& nsDocument, CXMLProfileInfo* pProfileInfo, CPathFinder::PosType ePosType, const CString& userName)
{
	if (!pProfileInfo)
		return;

	CString strSourcePath = ::GetProfilePath(nsDocument, pProfileInfo->GetName(), ePosType, userName);
	if (
			!::ExistFile(strSourcePath + SLASH_CHAR + szDocument) ||
			AfxMessageBox(cwsprintf(_TB("The export profile {0-%s} already exist. Would you like to overwrite it?"), pProfileInfo->GetName()), MB_YESNO) == IDYES
		)
	{
		pProfileInfo->SetModified();
		pProfileInfo->SaveProfile(strSourcePath, pProfileInfo->GetName());
	}
}

///<summary>
///Allows to copy an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool CopyExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ profilePath, DataInt posType, DataObjArray/*[ciString]*/ userArray)
{
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	CXMLProfileInfo* pProfileInfo = new CXMLProfileInfo(nsDocument, strPath);
	pProfileInfo->LoadAllFiles();

	CPathFinder::PosType ePosType = (CPathFinder::PosType)((int)posType);

	CString strUserName;
	switch(ePosType)
	{
		case CPathFinder::USERS:
			for (int nIdx = 0; nIdx <= userArray.GetUpperBound(); nIdx++)
			{
				strUserName = userArray.GetAt(nIdx)->Str();
				if (!strUserName.IsEmpty())
					CopySingleProfile(nsDocument, pProfileInfo, ePosType, strUserName);
					
			}
			break;
		case CPathFinder::ALL_USERS:
		case CPathFinder::STANDARD:
			CopySingleProfile(nsDocument, pProfileInfo, ePosType, _T("")); break;
	}

	delete pProfileInfo;

	return TRUE;
}

///<summary>
///Allows to move an export profile
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool MoveExportProfile(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ profilePath, DataInt posType, DataObjArray/*[ciString]*/ userArray)
{
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;

	CXMLProfileInfo* pProfileInfo = new CXMLProfileInfo(nsDocument, strPath);
	pProfileInfo->LoadAllFiles();

	CPathFinder::PosType ePosType = (CPathFinder::PosType)((int)posType);

	CString strUserName;
	CString strSourcePath;
	switch(ePosType)
	{
		case CPathFinder::USERS:
			for (int nIdx = 0; nIdx <= userArray.GetUpperBound(); nIdx++)
			{
				strUserName = userArray.GetAt(nIdx)->Str();
				if (!strUserName.IsEmpty())
					CopySingleProfile(nsDocument, pProfileInfo, ePosType, strUserName);
					
			}
			break;
		case CPathFinder::ALL_USERS:
		case CPathFinder::STANDARD:
			CopySingleProfile(nsDocument, pProfileInfo, ePosType, _T("")); break;
	}

    delete pProfileInfo;

	//cancello il profilo originario
	pProfileInfo = new CXMLProfileInfo(nsDocument, strPath);
	pProfileInfo->RemoveProfilePath();
	delete pProfileInfo;

	return TRUE;
}

// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
DataBool PostCreateSmartXSD(CAbstractFormDoc* pDocument, CXMLDataManager* pDataManager, DataStr& profilePath)
{
	if (!pDocument || !pDataManager)
		return FALSE;
	
	pDataManager->SetUnattendedExportParams
							(
								profilePath.Str(), 
								_T(""), 
								_T(""), 										
								EXPORT_ONLY_CURR_DOC, 
								USE_SELECTED_PROFILE, 
								FALSE										
							);

	pDataManager->GetXMLExportDocSelection()->m_nSchemaSelType = EXPORT_SMART_SCHEMA;

	BOOL retVal = pDataManager->Export() && 
			(
				(pDocument->m_pExternalControllerInfo->m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS) ||
				(pDocument->m_pExternalControllerInfo->m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS_WITH_INFO)
			);

	pDocument->m_pExternalControllerInfo = NULL;
	AfxGetTbCmdManager()->CloseDocument(pDocument);
	
	return retVal;
}


///<summary>
///Allows to create the schema XSD used for Magic Document Integration of the profile with the specified path and of the specified document
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataBool CreateSmartXSD(DataStr/*[ciString]*/ documentNamespace, DataStr&/*[ciString]*/ profilePath)
{
	CString strPath = profilePath.Str();
	CTBNamespace nsDocument(documentNamespace.Str());
	if (strPath.IsEmpty() || !ExistPath(strPath) || !nsDocument.IsValid())
		return FALSE;
	
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::RUNNING;

	DataObjArray messages;
	if (!nsDocument.IsEmpty())
	{
		CAbstractFormDoc* pDocument = NULL;
		CXMLDataManager* pDataManager = NULL;
		if (InitDocument(documentNamespace, _T(""), &info, pDocument, pDataManager, messages))
			return AfxInvokeThreadGlobalFunction<DataBool, CAbstractFormDoc*, CXMLDataManager*, DataStr&>
						(
							pDocument->GetThreadId(),
							&PostCreateSmartXSD,
							pDocument,
							pDataManager,
							profilePath
						);
		else
			return FALSE;    

	}

	return FALSE;     
}


// Function called on document's thread by AfxInvokeThreadGlobalFunction to serialize operations on document
//----------------------------------------------------------------------------------------------
DataStr PostGetDocumentSchema(CAbstractFormDoc *pDocument, CXMLDataManager* pDataManager, CTBNamespace nsDocument, DataStr profileName, DataStr forUser, CPathFinder::PosType ePosType)
{
	CString result = _T("");
	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	
	if (!pDocument || !pDataManager)
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::CANT_OPEN_DOC());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	CString strProfilePath = ::GetProfilePath(nsDocument, profileName.Str(), ePosType, forUser.Str());
	if (!::ExistPath(strProfilePath))
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::CANT_OPEN_DOC());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}
	
	CXMLProfileInfo profileInfo(nsDocument, strProfilePath);
	if (!profileInfo.LoadAllFiles())
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::CANT_OPEN_DOC());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	pDataManager->CreateXMLExpImpManager();

	CXSDGenerator* gen = pDataManager->GetSmartXMLSchemaString(&profileInfo, pDocument);
	gen->GetSchema()->GetXML(result);
	pDocument->OnCloseDocument();
	delete gen;

	return result;
}

///<summary>
///return XSD document schema in string format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataStr GetDocumentSchema(DataStr/*[ciString]*/ documentNamespace, DataStr/*[ciString]*/ profileName, DataStr/*[ciString]*/ forUser)
{
	CPathFinder::PosType ePosType;
	if (forUser.Str().CompareNoCase(szAllUserDirName) == 0)
		ePosType = CPathFinder::ALL_USERS;
	else
	{
		if (forUser.Str().CompareNoCase(szStandard) == 0)
			ePosType = CPathFinder::STANDARD;
		else
		{
			ePosType = CPathFinder::USERS;
			AfxGetThreadContext()->SetSessionLoginName(forUser.Str());
		}
	}

	CTBNamespace nsDocument(documentNamespace.Str()); 
	CSmartXMLDiagnosticMng xmlDiagnosticMng;
	if (!nsDocument.IsValid())
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_NAMESPACE_STRING());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	if (profileName.IsEmpty())
	{
		xmlDiagnosticMng.AddMessage(CXMLLogSpace::XML_ERROR, INIT_DIAGNOSTIC_CODE, szXTechSoapFunctions, Messages::XML_INVALID_NAMESPACE_STRING());
		return xmlDiagnosticMng.CreateXMLErrorString(NULL);
	}

	
	DataObjArray messages;
	CAbstractFormDoc* pDocument = NULL;
	CXMLDataManager* pDataManager = NULL;
	if (InitDocument(documentNamespace, _T(""), NULL, pDocument, pDataManager, messages))
		return AfxInvokeThreadGlobalFunction<DataStr, CAbstractFormDoc*, CXMLDataManager*, CTBNamespace, DataStr, DataStr, CPathFinder::PosType>
									(
										pDocument->GetThreadId(),
										&PostGetDocumentSchema,
										pDocument,
										pDataManager,
										nsDocument,
										profileName,
										forUser,
										ePosType
									);
	

	return _T("");
}


// Function called on report's thread by AfxInvokeThreadGlobalFunction to serialize operations on report
//----------------------------------------------------------------------------------------------
DataStr PostGetReportSchema(CWoormDoc* pWDoc, DataStr forUser )
{
	CString result = pWDoc->GetReportSchemaString(forUser.Str());
	AfxGetTbCmdManager()->CloseDocument(pWDoc);
	
	return result;
}

///<summary>
///return XSD report schema in string format
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
//----------------------------------------------------------------------------------------------
DataStr GetReportSchema(DataStr/*[ciString]*/ reportNamespace, DataStr/*[ciString]*/ forUser)
{
	if (forUser.Str().CompareNoCase(szAllUserDirName) != 0 && forUser.Str().CompareNoCase(szStandard) != 0)
		AfxGetThreadContext()->SetSessionLoginName(forUser.Str());
	
	CWoormDoc* pWDoc = AfxGetTbCmdManager()->RunWoormReport(reportNamespace.GetString(), NULL, FALSE, FALSE);
	if (!pWDoc)
		return _T("");
	
	return AfxInvokeThreadGlobalFunction<DataStr, CWoormDoc*, DataStr>
				(
					pWDoc->GetThreadId(),
					&PostGetReportSchema,
					pWDoc,
					forUser
				);
}

