
#include "stdafx.h"

#include <TbNameSolver\IFileSystemManager.h>

#include <TbClientCore\GlobalFunctions.h>
#include <TbGes\Dbt.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <XEngine\TBXmlTransfer\XMLDataMng.h>
#include <XEngine\TBXmlTransfer\XMLProfileInfo.h>

#include "CSubscriptionData.h"
#include "CDTrigger.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//             				CDTrigger
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CDTrigger, CClientDoc)

//-----------------------------------------------------------------------------
CDTrigger::CDTrigger()
	:
	CClientDoc()
{
}

//-----------------------------------------------------------------------------
CDTrigger::~CDTrigger()
{
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::OnOkTransaction()
{
	int dAction;
	
	if (m_pServerDocument->GetFormMode() == CBaseDocument::EDIT)
		dAction = 2;
	else
		dAction = 1;

	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, dAction, _T("BEFORE"), FALSE);

	if (!pEventManagementData)
		return TRUE;

	return ProcessSubscriptions(pEventManagementData, dAction, _T("Before"), TRUE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::OnOkDelete()
{
	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 1, _T("AFTER"), FALSE);

	if (pEventManagementData)
		ProcessSubscriptions(pEventManagementData, 3, _T("AFTER"), TRUE, FALSE);

	pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 3, _T("BEFORE"), FALSE);

	if (!pEventManagementData)
		return TRUE;

	return ProcessSubscriptions(pEventManagementData, 3, _T("BEFORE"), TRUE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::OnExtraNewTransaction()
{
	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 1, _T("AFTER"), FALSE);

	if (!pEventManagementData)
		return TRUE;

	return ProcessSubscriptions(pEventManagementData, 1, _T("AFTER"), TRUE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::OnExtraEditTransaction()
{
	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 2, _T("AFTER"), FALSE);

	if (!pEventManagementData)
		return TRUE;

	return ProcessSubscriptions(pEventManagementData, 2, _T("AFTER"), TRUE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::OnExtraDeleteTransaction()
{
	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 3, _T("AFTER"), FALSE);

	if (!pEventManagementData)
		return TRUE;

	return ProcessSubscriptions(pEventManagementData, 3, _T("AFTER"), FALSE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::ProcessSubscriptions (CEventManagementData* pEventManagementData, int dAction, CString aWhen, BOOL bExport, BOOL bCallWS)
{
	BOOL bOk = TRUE;
	
	if (AfxIsInUnattendedMode())
		return TRUE;

	CString aKey;
	CSubscriptionData* pItem;
	POSITION aPos = pEventManagementData->GetStartPosition();
	while (aPos != NULL)
	{
		pEventManagementData->GetNextAssoc(aPos,aKey,pItem);
		if	(
				 (m_pServerDocument->GetType() == VMT_BATCH ) && 
				(
					!pItem->IsActiveInBackgroundMode() ||
					(m_pServerDocument->GetXMLDataManager() && m_pServerDocument->GetXMLDataManager()->GetStatus() != CXMLDataManagerObj::XML_MNG_IDLE)
				)
			)
			continue;

		if	(
				(m_pServerDocument->GetType() == VMT_DATAENTRY) && 
				!pItem->IsActiveInForegroundMode()
			)
			continue;

		//Questo controllo evita che un documento attualmente in fase di importazione (marcato come export)
		//chiami a sua volta la callwebservice con la relativa esportazione causando problemi
		if (
			bExport &&
			m_pServerDocument->GetXMLDataManager() &&
			m_pServerDocument->GetXMLDataManager()->GetStatus() == CXMLDataManagerObj::XML_MNG_IMPORTING_DATA
			)
			continue;


		bOk =	(bExport ? XmlExport(pItem) : TRUE) && 
				(bCallWS ? CallWebService(pItem, dAction, aWhen) : TRUE) && 
				bOk;
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::XmlExport (CSubscriptionData* pSubscriptionData)
{
	if (!m_pServerDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		TRACE("CDTrigger::XmlExport: document %s is not derived from CAbstractFormDoc, XML export not allowed\n", m_pServerDocument->GetRuntimeClass()->m_lpszClassName);
		return FALSE;
	}
	
	CAbstractFormDoc* pFormDoc = (CAbstractFormDoc*)m_pServerDocument;
	if (pFormDoc->CanLoadXMLDescription())
		pFormDoc->LoadXMLDescription();

	if (!pFormDoc->GetXMLDocInfo())
	{
		TRACE("CDTrigger::XmlExport: error during document %s file xml loading\n", pFormDoc->GetRuntimeClass()->m_lpszClassName);
		return FALSE;
	}


	if (!m_pServerDocument->GetXMLDataManager())
		m_pServerDocument->SetXMLDataManager (new CXMLDataManager(m_pServerDocument));
	
	if (!m_pServerDocument->GetXMLDataManager() || !m_pServerDocument->GetXMLDataManager()->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLDocumentObject* pDocument = CreateExportParametersFile(pSubscriptionData->m_strExportProfile, pSubscriptionData->m_ePosType);

	DataArray arResult(DATA_STR_TYPE);
	CSmartExportParams expPar(&arResult, TRUE);

	//check for existing profile for user
	expPar.m_ePosType = pSubscriptionData->m_ePosType;
	if (expPar.m_ePosType == CPathFinder::USERS)
		expPar.m_strUserName = AfxGetLoginInfos()->m_strUserName;

	expPar.m_bExportCurrentDocument = TRUE;

	CXMLDataManager * pXMLDataMng = (CXMLDataManager*) m_pServerDocument->GetXMLDataManager();
	AfxGetThreadContext()->m_bSendDocumentEventsToMenu = FALSE;
	
	BOOL bSuccess = pXMLDataMng->ExportSmartDocument(pDocument, &expPar);
	
	if (bSuccess)
	{
		pSubscriptionData->m_tmpExportResult.RemoveAll();	
		arResult.ToStringArray(pSubscriptionData->m_tmpExportResult);
	}
	else	
		m_pServerDocument->m_pMessages->Add(cwsprintf(_TB("Error exporting document for trigger using the profile {0-%s}."), pSubscriptionData->m_strExportProfile));

	pDocument->Close();
	delete pDocument;
	AfxGetThreadContext()->m_bSendDocumentEventsToMenu = TRUE;	

	return bSuccess;	
}

//-----------------------------------------------------------------------------
//creo il file dei parametri così fatto
//<Contacts xTechProfile="SchedaRegistrazioneContatti"
//			postBack="true"
//			tbNamespace="Document.ERP.Contacts.Documents.Contacts"
//			postable="true" 
//			xmlns="http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/AllUsers/SchedaRegistrazioneContatti.xsd">
//	<Parameters master="true">
//		<DefaultDialog>
//			<DefaultGroup>
//				<Contact>0000</Contact> 
//			</DefaultGroup>
//		</DefaultDialog>
//	</Parameters>
//</Contacts>
//-----------------------------------------------------------------------------
CXMLDocumentObject* CDTrigger::CreateExportParametersFile (CString strProfileName, CPathFinder::PosType ePosType)
{
	CXMLDocumentObject* pXMLDoc = new CXMLDocumentObject();

	CXMLNode* pRoot = pXMLDoc->CreateRoot(m_pServerDocument->GetNamespace().GetObjectNameForTag());
	
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
			m_pServerDocument->GetNamespace().GetApplicationName() + URL_SLASH_CHAR +
			m_pServerDocument->GetNamespace().GetModuleName() + URL_SLASH_CHAR +
			m_pServerDocument->GetNamespace().GetObjectName() +  URL_SLASH_CHAR  +
			strPostType + URL_SLASH_CHAR +					
			strProfileName + szXsdExt;
	
	pRoot->SetAttribute(_T("xTechProfile"), strProfileName);
	pRoot->SetAttribute(_T("postBack"), DataBool(TRUE).FormatDataForXML());
	pRoot->SetAttribute(_T("tbNamespace"), m_pServerDocument->GetNamespace().ToString());
	pRoot->SetAttribute(_T("postable"), DataBool(TRUE).FormatDataForXML());
	pXMLDoc->SetNameSpaceURI(strNamespaceURI, szNamespacePrefix);	
	
	/*CXMLNode* pNodeParameters = pRootNode->CreateNewChild(_T("Parameters"));
	pNodeParameters->SetAttribute(_T("master"), DataBool(TRUE).FormatDataForXML());
	CXMLNode* pNodeDefaultDialog = pNodeParameters->CreateNewChild(_T("DefaultDialog"));
	CXMLNode* pNodeDefaultGroup = pNodeDefaultDialog->CreateNewChild(_T("DefaultGroup"));
	SqlRecord* pCurrentRecord = ((CAbstractFormDoc*)m_pServerDocument)->m_pDBTMaster->GetRecord();

	for (int nIdx = 0; nIdx <= pCurrentRecord->GetUpperBound(); nIdx++)
		{
			if (pCurrentRecord->IsVirtual(nIdx) || !pCurrentRecord->IsSpecial(nIdx))
				continue;
			SqlRecordItem* keySegment = pCurrentRecord->GetAt(nIdx);
			CXMLNode* pNodeChild = pNodeDefaultGroup->CreateNewChild(keySegment->GetColumnName());
			pNodeChild->SetText(keySegment->GetDataObj()->FormatDataForXML());
		}*/

	return pXMLDoc;
}

//-----------------------------------------------------------------------------
BOOL CDTrigger::CallWebService (CSubscriptionData*	pSubscriptionData, int dAction, CString aWhen)
{
	//invoco il webmethod specificato nel file di subscriptions
	CFunctionDescription aFunctionDescription	(pSubscriptionData->m_strWebMethod);
	aFunctionDescription.SetServer				(pSubscriptionData->m_strWebServer);
	aFunctionDescription.SetService				(pSubscriptionData->m_strWebService);
	aFunctionDescription.SetServiceNamespace	(pSubscriptionData->m_strWebNS);
	aFunctionDescription.SetPort				(pSubscriptionData->m_Port);

	aFunctionDescription.AddStrParam(_T("callerNS"),	m_pServerDocument->GetNamespace().ToString());
	aFunctionDescription.AddIntParam(_T("action"),		dAction);
	aFunctionDescription.AddStrParam(_T("when"),		aWhen);
	CDataObjDescription* pDataExport = new CDataObjDescription(_T("dataExport"),	pSubscriptionData->m_tmpExportResult, XML_DATATYPE_ARRAY_VALUE);
	((DataArray*)pDataExport->GetValue())->SetBaseDataType(DataType::String);
	aFunctionDescription.AddParam(pDataExport);


	CStringArray strReturnMessages;
	CDataObjDescription* pReturnMessages = new CDataObjDescription(_T("returnMessages"), strReturnMessages, XML_DATATYPE_ARRAY_VALUE);
	((DataArray*)pReturnMessages->GetValue())->SetBaseDataType(DataType::String);
	aFunctionDescription.AddOutParam(pReturnMessages);

	//indico che il valore di ritorno del webmethod è booleano
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!AfxInvokeSoapMethod(&aFunctionDescription)) //, &m_arMessages))
	{
		GetReturnMessages(aFunctionDescription.GetParamDescription(_T("returnMessages")));
		m_pServerDocument->m_pMessages->Add(aFunctionDescription.m_strError);
		pSubscriptionData->m_tmpExportResult.RemoveAll();
		return TRUE;
	}

	GetReturnMessages(aFunctionDescription.GetParamDescription(_T("returnMessages")));

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	if (!pdbVal)
	{
		pSubscriptionData->m_tmpExportResult.RemoveAll();
		m_pServerDocument->m_pMessages->Add(_TB("Error calling Web Service for trigger."));
		return FALSE;
	}

	pSubscriptionData->m_tmpExportResult.RemoveAll();
	if (!*pdbVal && m_pServerDocument->m_pMessages && m_pServerDocument->m_pMessages->MessageFound())
	{
		m_pServerDocument->m_pMessages->Show(TRUE);
	}
	return *pdbVal;
}

//-----------------------------------------------------------------------------
void CDTrigger::GetReturnMessages(CDataObjDescription* pReturnMessages)
{
	CStringArray* strReturnMessages = new CStringArray();
	((DataArray*) pReturnMessages->GetValue())->ToStringArray(*strReturnMessages);
	GetDiagnostic()->Add(*strReturnMessages, CDiagnostic::Warning);
	delete strReturnMessages;
}
