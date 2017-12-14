#include "stdafx.h" 

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbOleDb\OledbMng.h>
#include <TbOleDb\Sqlconnect.h>
#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\SqlTable.h>
#include <TbOleDb\RIChecker.h>
#include <TbGes\EXTDOC.H>
#include <TbGes\XMLGesInfo.h>
#include <TbGenlib\XMLModuleObjectsInfo.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGenLibManaged\GlobalFunctions.h>
#include <TbNameSolver\CompanyContext.h>
#include <TbWebServicesWrappers\DataSynchronizerWrapper.h>

#include "TbDataSynchroClientEnums.h"
#include "DSManager.h"

//=========================================================================
//	class CSynchroProvider: informazioni sul singolo provider
//=========================================================================
//
//=========================================================================================
CSynchroProvider::CSynchroProvider()
	:
	m_pDocumentsToSynch		(NULL),
	m_pParentSynchroProvider(NULL),
	m_bIsValid				(FALSE),
	m_IsDMSProvider			(FALSE),
	m_pParameters			(FALSE)
{
	m_pParameters =  new RecordArray();
	m_pDocumentsToSynch = new CSynchroDocInfoArray();
}

//=========================================================================================
CSynchroProvider::~CSynchroProvider()
{
	if (m_pDocumentsToSynch)
		delete m_pDocumentsToSynch;

	m_pParentSynchroProvider = NULL;

	if (m_pParameters)
		delete m_pParameters;
}

//=========================================================================================
BOOL CSynchroProvider::IsActivated() const
{
	return (m_Application.IsEmpty() && m_Functionality.IsEmpty()) || AfxIsActivated(m_Application,m_Functionality);				
}

//=========================================================================================
BOOL CSynchroProvider::Parse(const CString& strSynchroProfilesFile, BOOL isXmlContent/* = FALSE*/)
{
	CXMLDocumentObject aXMLModDoc;
	aXMLModDoc.EnableMsgMode(FALSE);

	if(!isXmlContent)
	{
		if (!aXMLModDoc.LoadXMLFile(strSynchroProfilesFile))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("The file {0-%s} cannot be read. File not loaded."), (LPCTSTR)strSynchroProfilesFile), CDiagnostic::Warning);
			return FALSE;
		}
	}
	else
	{
		if (!aXMLModDoc.LoadXML(strSynchroProfilesFile))
		{
			AfxGetDiagnostic()->Add(_TB("Xml Provider description cannot be read. Check if the RuntimeServer is running"), CDiagnostic::Warning);
			return FALSE;
		}
	}
	

	// root SynchroProfiles
	CXMLNode* pRoot = aXMLModDoc.GetRoot();
	if (!pRoot)
	{
		AfxGetDiagnostic()->Add(_TB("Exchange data connector: No Flow Planning."), CDiagnostic::Warning);
		return FALSE;
	}
	// Provider
	CString strValue;
	
	CXMLNode* pProvNode = pRoot->GetChildByName(_T("Provider"));
	if (!pProvNode)
	{
		AfxGetDiagnostic()->Add(_TB("Exchange data connector: No Flow Planning."), CDiagnostic::Warning);
		return FALSE;
	}
	if (pProvNode)
	{
		pProvNode->GetAttribute(_T("name"), m_Name);
		pProvNode->GetAttribute(_T("description"), m_Description);

		//se il provider è un nostro internal allora l'attivazione la cablo del codice
		if (m_Name.CompareNoCase(CRMInfinityProvider) == 0)
		{
			m_Application = MAGONET_APP;
			m_Functionality = CRMINFINITY_FUNCTIONALITY;
		}
		else
		{
			if (m_Name.CompareNoCase(DMSInfinityProvider) == 0)
			{
				m_Application = MAGONET_APP;
				m_Functionality = DMSINFINITY_FUNCTIONALITY;				
				m_IsDMSProvider = TRUE;
			}
			else
				{
					if (m_Name.CompareNoCase(InfiniteCRMProvider) == 0)
					{
						m_Application = MAGONET_APP;
						m_Functionality = INFINITECRM_FUNCTIONALITY;
					}
					else
					{
						CXMLNode* pActNode = pProvNode->GetChildByName(_T("ActivationInfo"));
						if (pActNode)
						{
							pActNode->GetAttribute(_T("application"), m_Application);
							pActNode->GetAttribute(_T("functionality"), m_Functionality);
						}
					}
				}
		}
		CXMLNode* pParamsNode = pProvNode->GetChildByName(_T("Parameters"));
		CXMLNode* pParamNode= NULL;
		if (!pProvNode)
		{
			AfxGetDiagnostic()->Add(_TB("Exchange data connector: No Flow Planning."), CDiagnostic::Warning);
			AfxGetDiagnostic()->Show();
			return FALSE;
		}
		if (pParamsNode)
		{
			CXMLNodeChildsList* pChildren = pParamsNode->GetChilds();
			if (!pChildren)
			{
				AfxGetDiagnostic()->Add(_TB("Xml Provider description cannot be read. Missing Parameters Elements"), CDiagnostic::Warning);
				AfxGetDiagnostic()->Show();
				return FALSE;
			}

			for (int i = 0; i <= pParamsNode->GetChilds()->GetUpperBound(); i++)
			{
				pParamNode = pParamsNode->GetChilds()->GetAt(i);
				if (pParamNode)
				{
					VProviderParams* pParam = new VProviderParams();
					pParamNode->GetAttribute(_T("name"), strValue);
					pParam->l_Name = strValue;
					pParamNode->GetAttribute(_T("description"), strValue);
					pParam->l_Description = strValue;
					if (pParam->l_Name.IsEmpty())
						delete pParam;
					else
						m_pParameters->Add(pParam);
				}
			}
		}
	}	

	CXMLNode* pDocumentsNode = pRoot->GetChildByName(_T("Documents"));
	CXMLNode* pActionNode(NULL);
	CXMLNode* pDocNode = NULL;
	if (pDocumentsNode)
	{
		CXMLNodeChildsList* pChildren = pDocumentsNode->GetChilds();
		if (!pChildren)
		{
			AfxGetDiagnostic()->Add(_TB("DataSynchronizer: No Flow Planned"), CDiagnostic::Warning);
			AfxGetDiagnostic()->Show();
			return TRUE;
		}

		for (int i = 0; i <= pChildren->GetUpperBound(); i++)
		{
			pDocNode = pDocumentsNode->GetChilds()->GetAt(i);
			if (pDocNode)
			{
				DOMNodeType type;
				if (pDocNode->GetType(type) && type == NODE_COMMENT)
					continue;

				pDocNode->GetAttribute(_T("namespace"), strValue);
				if (strValue.IsEmpty())
					continue;
				CSynchroDocInfo* pSynchroDocInfo = new CSynchroDocInfo(strValue);
				pDocNode->GetAttribute(_T("actions"), strValue);
				if (!strValue.IsEmpty())
					pSynchroDocInfo->SetActionMode(strValue);

				pActionNode = pDocNode->GetChildByName(_T("Action"));
				if (pActionNode)
				{
					if (pActionNode->GetAttribute(_T("onlyForDMS"), strValue))
						pSynchroDocInfo->m_OnlyForDMS = strValue;
					if (pActionNode->GetAttribute(_T("iMagoConfigurations"), strValue))
						pSynchroDocInfo->m_iMagoConfigurations = strValue;
				}
				BOOL bFound = FALSE;
				for (int i=0; i < m_pDocumentsToSynch->GetCount();i++)
				{
					CSynchroDocInfo* p_tmpSynchroDocInfo = m_pDocumentsToSynch->GetAt(i);
					if (p_tmpSynchroDocInfo->m_strDocNamespace == pSynchroDocInfo->m_strDocNamespace)
					{
						bFound = TRUE;
						break;
					}
				}
				if (!bFound)
					m_pDocumentsToSynch->Add(pSynchroDocInfo);
				else
					SAFE_DELETE(pSynchroDocInfo);
			}
		}
	}	

	aXMLModDoc.Close();

	return TRUE;
}

//=========================================================================================
void CSynchroProvider::ParseValuesFromXMLString(const CString xmlParamsValue)
{
	if (xmlParamsValue.IsEmpty())
		return;

	CXMLDocumentObject aDoc;
	aDoc.LoadXML(xmlParamsValue);

	CXMLNode*	pRoot = aDoc.GetRoot();
	VProviderParams* pParam =  NULL;
	CXMLNode* pParNode = NULL;

	for (int nIdx = 0; nIdx < m_pParameters->GetSize(); nIdx++)		
	{
		pParam = (VProviderParams*)m_pParameters->GetAt(nIdx);
		if (pParam)
		{
			pParam->l_Value.Clear();
			pParNode = pRoot->GetChildByName(pParam->l_Name.Str());
			if (pParNode)
			{
				CString strValue;
				pParNode->GetText(strValue);
				pParam->l_Value.AssignFromXMLString(strValue);
			}
		}
	}
	aDoc.Close();
}


//=========================================================================================
CString CSynchroProvider::UnparseValuesToXMLString()
{
	CXMLNode* pRoot;
	CXMLDocumentObject aDoc;
	pRoot = aDoc.CreateRoot(_T("Parameters"));
	
	VProviderParams* pParam =  NULL;
	CXMLNode* pParNode = NULL;
	for ( int nIdx = 0 ; nIdx < m_pParameters->GetSize() ; nIdx++)
	{	
		pParam = (VProviderParams*)m_pParameters->GetAt(nIdx);		
		if (pParam && !pParam->l_Name.IsEmpty())
		{
			CXMLNode* pParNode = pRoot->CreateNewChild(pParam->l_Name.Str());
			pParNode->SetText(pParam->l_Value.FormatDataForXML());
		}
	}

	CString strXML;
	aDoc.GetXML(strXML);
	aDoc.Close();
	return strXML;
}

//=========================================================================
//	class CSynchroProviderArray: array dei provider
//=========================================================================
//
//=========================================================================================
CSynchroProvider* CSynchroProviderArray::GetProvider(const DataStr& providerName) const
{
	CSynchroProvider* synchroProvider = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		synchroProvider = GetAt(i);
		if (synchroProvider->m_Name == providerName)
			return synchroProvider;
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//						CDataSynchroManager implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CDataSynchroManager::CDataSynchroManager()
	:
	m_pDocuments		(NULL),
	m_pVersionDlg		(NULL),
	m_bIsMagoRuntimeInstalled	(FALSE),
	m_bIsAlive					(TRUE)
{
	m_nSynchroStatus = IDLE;

	CString strDataSynchronizerName = AfxGetPathFinder()->GetInstallationName() + _T("/DataSynchronizer/DataSynchronizer.asmx");
	m_pDataSynchronizer = new CDataSynchronizerWrapper( 
								strDataSynchronizerName,
								_T("http://microarea.it/DataSynchronizer/"),
								AfxGetLoginManager()->GetServer(),
								AfxGetCommonClientObjects()->GetServerConnectionInfo ()->m_nWebServicesPort
							);
	m_bIsMagoRuntimeInstalled = m_pDataSynchronizer->ImagoStudioRuntimeInstalled();
	LoadSynchroProviders();	
	PurgeSynchroConnectorLog(); 
}

//-----------------------------------------------------------------------------
CDataSynchroManager::~CDataSynchroManager()
{
	SAFE_DELETE(m_pDataSynchronizer);
	SAFE_DELETE(m_pDocuments);
	SAFE_DELETE(m_pVersionDlg);
}

//-----------------------------------------------------------------------------
BOOL CDataSynchroManager::IsValid()	const
{
	BOOL bIsValid = FALSE;
//per far funzionare il sistema di sincronizzazione basta avere solo un provider valido
	for (int j = 0; j < m_aSynchroProviders.GetSize(); j++)
		bIsValid = m_aSynchroProviders.GetAt(j)->IsValid() || bIsValid;

	return bIsValid;
}

//-----------------------------------------------------------------------------
BOOL CDataSynchroManager::IsProviderDisabled(const CString& providerName) const
{
	TRDS_Providers aTRDS_Providers;
	aTRDS_Providers.SetSqlSession(AfxGetDefaultSqlSession());
	TableReader::FindResult result = aTRDS_Providers.FindRecord(providerName);

	if (result == TableReader::FOUND)
	{
		TDS_Providers* pRec = aTRDS_Providers.GetRecord();
		return pRec->f_Disabled;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
 BOOL CDataSynchroManager::IsProviderValid(const CString& providerName)	const
 {
	CSynchroProvider* pSynchroProvider = m_aSynchroProviders.GetProvider(providerName);
	 if (pSynchroProvider)
		return pSynchroProvider->IsValid();
	 return FALSE;
 }

//-----------------------------------------------------------------------------
BOOL CDataSynchroManager::IsDocumentToSynchronize(const CTBNamespace& docNamespace)
{
	if (docNamespace.ToString() == _T("Document.Extensions.TbDataSynchroClient.TbDataSynchroClient.Providers"))
		return FALSE;

	if (!m_pDocuments || m_pDocuments->GetSize() <= 0)
		return FALSE;
	
	CString strDocNamespace = docNamespace.ToString();

	for (int i = 0; i < m_pDocuments->GetSize(); i++)
	{
		CString strns = m_pDocuments->GetAt(i);
		if (strDocNamespace.CompareNoCase(m_pDocuments->GetAt(i)) == 0)
			return TRUE;
	}
	return FALSE;
}

//=========================================================================================
BOOL CDataSynchroManager::NeedMassiveSynchro(const CString& providerName) const
{
	CSynchroProvider* pSynchroProvider = m_aSynchroProviders.GetProvider(providerName);

	// TODO: da sistemare
	//if (AfxIsActivated(MAGONET_APP, _T("IMagoStudio")))
	//	return false;

	if (!pSynchroProvider->IsValid())
		return FALSE;

	//obbligo la massiva solo al CRMInfinityProvider e CRMInfiniteProvider
	if (providerName.CompareNoCase(CRMInfinityProvider) != 0  && (providerName.CompareNoCase(InfiniteCRMProvider) != 0 ) && (providerName.CompareNoCase(DMSInfinityProvider)!= 0 ))
		return FALSE;
	
	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_aSynchroProviders)
	if (!pSynchroProvider->NeedMassiveSynchro())
		return FALSE;
	END_TB_OBJECT_LOCK()

	BOOL    bNeedMassive = FALSE;
	CString sProviderName = providerName;
	
	if (providerName.CompareNoCase(DMSInfinityProvider) == 0)
		sProviderName = CRMInfinityProvider;
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		//se la tabella DS_SynchronizationInfo è vuota vuol dire che non è stata eseguita nessuna sincronizzazione massiva
		//(oppure è stata eseguita ma non è andata a buon fine)
		SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();


		TDS_SynchronizationInfo aSynchroInfo;
		SqlTable aTable(&aSynchroInfo, pNewSession);
		TRY
		{
			//Select TOP(1) DocTBGuid from DS_SynchronizationInfo where ProviderName = 'CRMInfinityProvider' AND  DocNamespace <> 'Extensions.TbDataSynchroClient.TbDataSynchroClient.Providers'
			//comunque anche se il providerName e' 'DMSInfinityProvider' controllo con ProviderName = 'CRMInfinityProvider'
			aTable.AddSelectKeyword(SqlTable::TOP);
			aTable.Select(aSynchroInfo.f_DocTBGuid);
			aTable.m_strFilter = cwsprintf(_T("%s = %s AND %s <> %s"),
											TDS_SynchronizationInfo::szProviderName, AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(sProviderName)),
											TDS_SynchronizationInfo::szDocNamespace, AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(_T("Extensions.TbDataSynchroClient.TbDataSynchroClient.Providers")))
											);
			aTable.Open();
			aTable.Query();
			bNeedMassive = aTable.IsEmpty();
			aTable.Close();
		}
			CATCH(SqlException, e)
		{
			THROW(e);
		}
		END_CATCH
	}
	else
		bNeedMassive = m_pDataSynchronizer->NeedMassiveSynchro(sProviderName);

	TB_OBJECT_LOCK(&m_aSynchroProviders)
	pSynchroProvider->SetNeedMassiveSynchro(bNeedMassive);
	return bNeedMassive;
}

//=========================================================================================
BOOL IsDocumentToSynch(const CString& docNamespace, CXMLDocumentObject* pDocObj, CString& strFilter)
{
	CXMLNode* pRoot = pDocObj->GetRoot();
	strFilter.Empty();
	CXMLNode* pNode = pRoot->GetChildByAttributeValue(_T("Document"), _T("namespace"), docNamespace);
	if (pNode)
	{
		CXMLNode* pFilter = pNode->GetChildByName(_T("Filter"));
		if (pFilter)
			pFilter->GetText(strFilter);

		return TRUE;
	}

	return FALSE;
}

//=========================================================================================
void CDataSynchroManager::CheckAndUpdateTBGuid(const CString& strDocNamespace, SqlSession* pSqlSession)
{
	CTBNamespace docNamespace(strDocNamespace);
	CString strTableNamespace = CXMLDocInfo::GetMasterTableNamespace(docNamespace);
	if (strTableNamespace.IsEmpty())
		return;
	CTBNamespace tableNamespace(strTableNamespace);
	if (!tableNamespace.IsValid())
		return;
	const SqlCatalogEntry* pCatalogEntry = pSqlSession->GetSqlConnection()->GetCatalogEntry(tableNamespace.GetObjectName());
	if (!pCatalogEntry)
		return;

	SqlTable aUpdateTable(pSqlSession);
	aUpdateTable.SetAutocommit();
	
	TRY
	{
		aUpdateTable.Open();
		CString strSql = cwsprintf
			(
			_T("UPDATE %s SET TBGuid = NEWID() WHERE TBGuid = '00000000-0000-0000-0000-000000000000' OR TBGuid = '00000000-0000-0000-0008-000000000000'"), 
			tableNamespace.GetObjectName()
			);
		aUpdateTable.ExecuteQuery(strSql);
	}
	CATCH(SqlException, e)
	{
		if (aUpdateTable.IsOpen())
			aUpdateTable.Close();	
		//strMessage = cwsprintf(_T("The following error occurred writing in the DS_ActionsQueue table: {0-%s}."), e->m_strError);
		return;
	}
	END_CATCH

	if (aUpdateTable.IsOpen())
		aUpdateTable.Close();
}

//=========================================================================================
BOOL CDataSynchroManager::SynchronizeOutbound(const CString& providerName, const CString& xmlParameters, CString& strMessage, DataDate& startSynchroDate, BOOL bDelta /*= FALSE*/)
{
	
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		//devo inserire una riga per ogni documento che partecipa al processo di sincronizzazione
		CString strInsertCmd = cwsprintf(_T("INSERT INTO %s (%s, %s, %s, %s, %s, %s, %s)"),
			TDS_ActionsQueue::GetStaticName(),
			TDS_ActionsQueue::szProviderName, TDS_ActionsQueue::szActionName, TDS_ActionsQueue::szSynchDirection, TDS_ActionsQueue::szSynchStatus,
			TDS_ActionsQueue::szSynchFilters, CREATED_ID_COL_NAME, MODIFIED_ID_COL_NAME);

		CSynchroProvider* pProvider = m_aSynchroProviders.GetProvider(providerName);
		if (!pProvider)
		{
			strMessage = _TB("No provider is configured in the company.");
			return FALSE;
		}

		if (!pProvider->IsValid())
		{
			strMessage = cwsprintf(_TB("The provider {0-%s} is invalid. Please check the provider parameters."), providerName);
			return FALSE;
		}

		CString strInsertQuery;
		CString docNamespace;

		SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
		CString strStartSynchroDate = pNewSession->GetSqlConnection()->NativeConvert(&startSynchroDate);

			SqlTable aTable(pNewSession);

			CString strFilter;
			CXMLDocumentObject aDocObj;
			aDocObj.LoadXML(xmlParameters);
			aTable.Open();
			for (int i = 0; i < pProvider->GetDocumentsToSynch()->GetSize(); i++)
			{
				docNamespace = pProvider->GetDocumentsToSynch()->GetAt(i)->m_strDocNamespace;
				if (IsDocumentToSynch(docNamespace, &aDocObj, strFilter))
				{
					//il check del TBGuid non viene fatto se il provider dipende da un altro provider (vedi DMSInfinity con CRMInfinity)
					if (!pProvider->m_pParentSynchroProvider)
						CheckAndUpdateTBGuid(docNamespace, pNewSession);

					strInsertQuery = cwsprintf(_T(" %s VALUES ( %s,  %s,  %s,  %s,  %s,  %s,  %s)"),
						strInsertCmd,
						AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(providerName)),
						AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(docNamespace)),
						AfxGetDefaultSqlConnection()->NativeConvert(&DataEnum(E_SYNCHRODIRECTION_TYPE_OUTBOUND)),
						AfxGetDefaultSqlConnection()->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_TOSYNCHRO)),
						AfxGetDefaultSqlConnection()->NativeConvert(&DataStr(strFilter)), //filtri
						AfxGetDefaultSqlConnection()->NativeConvert(&DataLng(AfxGetWorkerId())),
						AfxGetDefaultSqlConnection()->NativeConvert(&DataLng(AfxGetWorkerId())));
					TRY
					{
						aTable.ExecuteQuery(strInsertQuery);

					}
						CATCH(SqlException, e)
					{
						if (aTable.IsOpen())
							aTable.Close();
						aDocObj.Close();
						pNewSession->Close();
						delete pNewSession;
						strMessage = cwsprintf(_T("The following error occurred writing in the DS_ActionsQueue table: {0-%s}."), e->m_strError);
						return FALSE;
					}
					END_CATCH
				}
			}
			aTable.Close();
			aDocObj.Close();
			pNewSession->Close();
			delete pNewSession;

		if (!bDelta)
		{
			if (!m_pDataSynchronizer->SynchronizeOutbound(providerName))
			{
				strMessage = cwsprintf(_T("An error occurred calling the SynchronizeOutbound web method"));
				return FALSE;
			}
		}
		else
		{
			/*Sincronizzazione massiva Differenziale*/
			if (!m_pDataSynchronizer->SynchronizeOutboundDelta(providerName, strStartSynchroDate))
			{
				strMessage = cwsprintf(_T("An error occurred calling the SynchronizeOutboundDelta web method"));
				return FALSE;
			}
		}
	}
	else
	{
		CString strInsertQuery;
		CString docNamespace;

		SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
		CString strStartSynchroDate = pNewSession->GetSqlConnection()->NativeConvert(&startSynchroDate);

		SqlTable aTable(pNewSession);

		CString strFilter;
		CXMLDocumentObject aDocObj;
		aDocObj.LoadXML(xmlParameters);

		if (!bDelta)
		{
			if (!m_pDataSynchronizer->SynchronizeOutbound(providerName, xmlParameters))
			{
				strMessage = cwsprintf(_T("An error occurred calling the SynchronizeOutbound web method"));
				return FALSE;
			}
		}
		else
		{
			/*Sincronizzazione massiva Differenziale*/
			if (!m_pDataSynchronizer->SynchronizeOutboundDelta(providerName, strStartSynchroDate, xmlParameters))
			{
				strMessage = cwsprintf(_T("An error occurred calling the SynchronizeOutboundDelta web method"));
				return FALSE;
			}
		}
	}
	return TRUE;

}

//=========================================================================================
BOOL CDataSynchroManager::ValidateOutbound(RICheckNode* pCheckerProviderNode, BOOL bCheckFK, BOOL bCheckXSD, CString filters, CString& message)
{
	CString providerName = pCheckerProviderNode->GetName();
	CSynchroProvider* pProvider = m_aSynchroProviders.GetProvider(providerName);
	CString strMessage(_T(""));

	if (!pProvider)
	{
		strMessage = _TB("No provider is configured in the company.");
		return FALSE;
	}

	if (!pProvider->IsValid())
	{
		strMessage = cwsprintf(_TB("The provider {0-%s} is invalid. Please check the provider parameters."), providerName);
		return FALSE;
	}

	if (!m_pDataSynchronizer->ValidateOutbound(pCheckerProviderNode, bCheckFK, bCheckXSD, filters, message))
	{
		strMessage = cwsprintf(_T("An error occurred calling the ValidateOutbound web method"));
		return FALSE;
	}

	return TRUE;
}

//=========================================================================================
IMassiveValidationSettings*	CDataSynchroManager::GetMassiveValidationSettings()
{
	ValidationMonitorSettings* pSettings = new ValidationMonitorSettings();

	return pSettings;
}

//=========================================================================================
BOOL CDataSynchroManager::SaveSynchroFilter(const CString& docNamespace, const CString& providerName, const CString& xmlParameters, CString& strMessage)
{
	TUDS_SynchroFilter aSynchroFilterTU;

	SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
	aSynchroFilterTU.SetSqlSession(pNewSession);
	aSynchroFilterTU.SetAutocommit();
	TRY
	{
		if (aSynchroFilterTU.FindRecord(docNamespace, providerName, TRUE) == TableUpdater::LOCKED)
		{
			strMessage = _TB("Locked record");
			pNewSession->Close();
			delete pNewSession;
			return FALSE;
		}
		if (aSynchroFilterTU.FindRecord(docNamespace, providerName, TRUE) == TableUpdater::FOUND)
		{
			aSynchroFilterTU.GetRecord()->f_SynchroFilter = xmlParameters;
			aSynchroFilterTU.UpdateRecord();
		}
		else
		{
			aSynchroFilterTU.GetRecord()->f_DocNamespace = docNamespace;
			aSynchroFilterTU.GetRecord()->f_ProviderName = providerName;
			aSynchroFilterTU.GetRecord()->f_SynchroFilter = xmlParameters;
			aSynchroFilterTU.UpdateRecord();
		}
		aSynchroFilterTU.UnlockCurrent();
		pNewSession->Close();
		delete pNewSession;
		return TRUE;

	}
	CATCH(SqlException, e)
	{
		if (pNewSession)
		{
			pNewSession->Close();
			delete pNewSession;
		}
		strMessage = cwsprintf(_TB("An error occurred updating DS_SynchroFilter table: {0-%s}"), e->m_strError);
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

//=========================================================================================
CString	CDataSynchroManager::GetSynchroFilter(const CString& docNamespace, const CString& providerName)	
{
	TRDS_SynchroFilter aTRSynchroFilter;
	aTRSynchroFilter.SetSqlSession(AfxGetDefaultSqlSession());
	TRY
	{
		if (aTRSynchroFilter.FindRecord(docNamespace, providerName) == TableUpdater::FOUND)
			return aTRSynchroFilter.GetRecord()->f_SynchroFilter;
	}
	CATCH(SqlException, e)
	{
		return _T("");
	}
	END_CATCH

	return _T("");
}

//=========================================================================================
BOOL CDataSynchroManager::CheckProvider(TDS_Providers* pProviderRec, CString& strErrMsg)
{
	if (pProviderRec->f_Name.IsEmpty() || pProviderRec->f_ProviderUrl.IsEmpty() || pProviderRec->f_ProviderUser.IsEmpty())
	{
		if (AfxIsActivated(MAGONET_APP, _NS_ACT("iMago")))
			strErrMsg = cwsprintf(_TB("In order to use the synchronization provider {0-%s} please run the Configuration Wizard in Services - Connectors - I.Mago."), pProviderRec->f_Description.Str());
		else
			strErrMsg = cwsprintf(_TB("In order to use the synchronization provider {0-%s} please set its data using the Providers form in Task Builder Framework - Data Synchronization menu or put it in disable mode."), pProviderRec->f_Description.Str());
		return FALSE;
	}

	CString strProviderMsg;
	
	BOOL res = TestProviderParameters(pProviderRec->f_Name, pProviderRec->f_ProviderUrl, pProviderRec->f_ProviderUser, pProviderRec->f_ProviderPassword, pProviderRec->f_SkipCrtValidation, pProviderRec->f_ProviderParameters, strProviderMsg);

	if (!res)
		strErrMsg = cwsprintf(_TB("The following error occured checking the synchro provider {0-%s}: {1-%s}"), pProviderRec->f_Description.Str(), strProviderMsg);
	
	if (
			res && 
			pProviderRec->f_Name.CompareNoCase(CRMInfinityProvider) == 0 && 
			strProviderMsg.IsEmpty() && 
			!IsMassiveSynchronizing()
		) 
	{
		CString strBuildVersion = AfxGetLoginManager()->GetMasterProductBrandedName()+ _T(";") + AfxGetLoginManager()->GetInstallationVersion();
		BOOL check = CheckVersion(pProviderRec->f_Name, strBuildVersion, strProviderMsg);

		if (!check)
		{
		/*	CSynchroProvider* pSynchroProvider = NULL;
			TUDS_Providers aProviderTU;
			TDS_Providers pCRMInfinityRec;
			SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
			aProviderTU.SetSqlSession(pNewSession);
			aProviderTU.SetAutocommit();*/

			strErrMsg = cwsprintf(_TB("Warning! Mago version is not compatible with Infinity patch number: {0-%s}"), strProviderMsg);
		
			// creo la dialog
			m_pVersionDlg = new CVersionControlTabDialog();
			m_pVersionDlg->Create();
			m_pVersionDlg->SetErrorMsg(strErrMsg);
			
			//if (aProviderTU.FindRecord(pProviderRec->f_Name, FALSE) == TableUpdater::FOUND)
			//{
			//	pSynchroProvider = m_aSynchroProviders.GetProvider(pProviderRec->f_Name);
			//	//valorizzo i parametri con il dato letto dalla tabella
			//	aProviderTU.GetRecord()->f_Disabled = TRUE;
			//	aProviderTU.UpdateRecord();
			//}
		}
	}
	
	return res;
}

//=========================================================================================
void CDataSynchroManager::UpdateProviderRec(CSynchroProvider* pSynchroProvider, TUDS_Providers* pProviderTU)
{
	//devo verificare se nel campo f_ProviderParameters vi è ancora la vecchia sintassi (<XMLVariables><registeredapp>MAGO</registeredapp><wscode>001</wscode></XMLVariables>). 
	// se così fosse devo convertirla alla nuova sintassi ne leggo prima i valori e poi scrivo la nuova sintassi (Parameters al posto di XMLVariables)
	if (
			!pProviderTU->GetRecord()->f_TBGuid.IsEmpty() && 
			(pProviderTU->GetRecord()->f_ProviderParameters.IsEmpty() || pProviderTU->GetRecord()->f_ProviderParameters.Str().Find(_T("Parameters")))
		)
		return;

	//per prima cosa vado ad inserire il TBGuid se il campo è vuoto
	if (pProviderTU->GetRecord()->f_TBGuid.IsEmpty())
		pProviderTU->GetRecord()->f_TBGuid.AssignNewGuid();
		
	/*
	<Parameters>
		<registeredapp>MAGO</registeredapp>
		<wscode>001</wscode>>
	</Parameters>*/
	//vecchia sintassi: la devo convertire nella nuova
	if (!pProviderTU->GetRecord()->f_ProviderParameters.IsEmpty() && pProviderTU->GetRecord()->f_ProviderParameters.Str().Find(_T("XMLVariables")) > 0)
	{
		CXMLVariableArray aXMLVariables;
		//per leggere i valori presenti nella vecchia stringa creo una CXMLVariableArray a partire dai nomi dei parametri letti dal file SynchroProfiles.xml
		for (int i = 0; i < pSynchroProvider->m_pParameters->GetSize(); i++)
		{
			VProviderParams* vProviderParam = (VProviderParams*)pSynchroProvider->m_pParameters->GetAt(i);
			aXMLVariables.Add(new CXMLVariable(vProviderParam->l_Name, new DataStr(), TRUE));
		}
		//ho letto i valori dei parametri dalla vecchia string xml
		aXMLVariables.ParseFromXMLString(pProviderTU->GetRecord()->f_ProviderParameters);

		//ora valorizzo i parameti considerando aXMLVariable
		for (int i = 0; i < pSynchroProvider->m_pParameters->GetSize(); i++)
		{
			VProviderParams* vProviderParam = (VProviderParams*)pSynchroProvider->m_pParameters->GetAt(i);
			CXMLVariable* pXMLVar = aXMLVariables.GetVariableByName(vProviderParam->l_Name);
			vProviderParam->l_Value = pXMLVar->GetDataObjValue();
		}			
		pProviderTU->GetRecord()->f_ProviderParameters = pSynchroProvider->UnparseValuesToXMLString();
		
	}			
	pProviderTU->UpdateRecord();	
}

//=========================================================================================
CString CDataSynchroManager::GetRuntimeFlows(CString providerName)
{
	return m_pDataSynchronizer->GetRuntimeFlows(providerName);
}

//=========================================================================================
CString CDataSynchroManager::GetLogsByNamespace(CString providerName, CString flow, DataBool bOnlyError)
{
	return m_pDataSynchronizer->GetLogsByNamespace(providerName, flow, bOnlyError);
}

//=========================================================================================
CString CDataSynchroManager::GetLogsByNamespaceDelta(CString providerName, CString flow, DataBool bOnlyError, DataBool bDelta)
{
	return m_pDataSynchronizer->GetLogsByNamespaceDelta(providerName, flow, bOnlyError, bDelta);
}

//=========================================================================================
CString CDataSynchroManager::GetLogsByDocId(CString providerName, CString TbDocGuid)
{
	return m_pDataSynchronizer->GetLogsByDocId(providerName, TbDocGuid);
}

//=========================================================================================
CString CDataSynchroManager::GetMassiveSynchroLogs(CString providerName, CString bFromDelta, CString bOnlyErrors)
{
	return m_pDataSynchronizer->GetMassiveSynchroLogs(providerName, bFromDelta, bOnlyErrors);
}

//=========================================================================================
CString CDataSynchroManager::GetSynchroLogsByFilters(
	const CString& providerName,
	const CString& strNamespace,
	DataBool& FromDelta,
	DataBool& FromBatch,
	DataBool& AllStatus,
	DataBool& Status,
	DataBool& AllDate,
	DataDate& FromDate,
	DataDate& ToDate,
	DataDate& SynchDate,
	const CString& FlowName,
	DataInt&       Offset
)
{
	return m_pDataSynchronizer->GetSynchroLogsByFilters(
		providerName,
		strNamespace,
		FromDelta,
		FromBatch,
		AllStatus,
		Status,
		AllDate,
		FromDate,
		ToDate,
		SynchDate,
		FlowName,
		Offset
	);
}


//=========================================================================================
void CDataSynchroManager::LoadSynchroProviders()
{	
	m_aSynchroProviders.RemoveAll();

	if (ImagoStudioRuntimeInstalled()) 
	{
		CString crmInfinityFlows = GetRuntimeFlows(CRMInfinityProvider);
		CSynchroProvider* pCrmSynchroProvider = new CSynchroProvider();
		if (pCrmSynchroProvider->Parse(crmInfinityFlows, TRUE) &&
			pCrmSynchroProvider->IsActivated() &&
			(pCrmSynchroProvider->m_Name.CompareNoCase(DMSInfinityProvider) != 0 || AfxGetOleDbMng()->EasyAttachmentEnable()))
			m_aSynchroProviders.Add(pCrmSynchroProvider);
		else
			delete pCrmSynchroProvider;

		CString dmsInfinityFlows = GetRuntimeFlows(DMSInfinityProvider);
		CSynchroProvider* pDmsSynchroProvider = new CSynchroProvider();
		if (pDmsSynchroProvider->Parse(dmsInfinityFlows, TRUE) &&
			pDmsSynchroProvider->IsActivated() &&
			AfxGetOleDbMng()->EasyAttachmentEnable())
			m_aSynchroProviders.Add(pDmsSynchroProvider);
		else
			delete pDmsSynchroProvider;
		
	}
	else
	{
		AfxGetDiagnostic()->Add(_TB("Unable to find ImagoStudioRuntimeProxy. Please verify if Runtime Server is installed"), CDiagnostic::Warning);
		return;
		
		/*
		//scorro tutte le app alla ricerca del modulo SynchroConnector ed al suo interno della cartella SynchroProviders. 
		// Per ogni SynchroProvider vado a fare il parsing nel file SynchroProfiles.xml se non esiste non lo carico
		AddOnApplication*	pAddOnApp;
		AddOnModule*		pAddOnMod;
		for (int a = 0; a < AfxGetAddOnAppsTable()->GetSize(); a++)
		{
			pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
			if (!pAddOnApp)
				continue;

			for (int m = 0; m < pAddOnApp->m_pAddOnModules->GetSize(); m++)
			{
				pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
				if (!pAddOnMod || pAddOnMod->GetModuleName().CompareNoCase(_T("SynchroConnector")) != 0)
					continue;

				CString strSynchroProviderPath = AfxGetPathFinder()->GetSynchroProvidersPath(pAddOnMod->m_Namespace);
				if (!ExistPath(strSynchroProviderPath))
					break;

				CFileFind finder;
				CString strSynchroProfilesFile;
				BOOL bWorking = finder.FindFile(strSynchroProviderPath + URL_SLASH_CHAR + _T("*.*"));
				BOOL bResult = TRUE;
				while (bWorking)
				{
					bWorking = finder.FindNextFile();
					// evito "." e ".." per evitare ricorsione
					if (finder.IsDots())
						continue;
					//ogni folder è un provider
					if (finder.IsDirectory())
					{
						strSynchroProfilesFile = MakeFilePath(finder.GetFilePath(), _T("SynchroProfiles"), _T(".xml"));

						if (ExistFile(strSynchroProfilesFile))
						{
							CSynchroProvider* pSynchroProvider = new CSynchroProvider();
							//aggiungo il provider solo se è attivato e nel caso di DMSInfinity se è anche abilitato EasyAttachment
							if (
								pSynchroProvider->Parse(strSynchroProfilesFile)
								&& pSynchroProvider->IsActivated() &&
								(pSynchroProvider->m_Name.CompareNoCase(DMSInfinityProvider) != 0 || AfxGetOleDbMng()->EasyAttachmentEnable())
								)
								m_aSynchroProviders.Add(pSynchroProvider);
							else
								delete pSynchroProvider;
						}
					}
				}
			}
		}
		*/
	}

	CSynchroProvider* pSynchroProvider = NULL;
	TUDS_Providers aProviderTU;
	TDS_Providers pCRMInfinityRec;
	SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
	aProviderTU.SetSqlSession(pNewSession);
	aProviderTU.SetAutocommit();
	
	//per ogni providerverifico se esiste la riga nella tabella DS_Provider. Due casi
	//1. riga esistente: leggo le informazioni ed effettuo il test di validità: provider attivato + TestProviderData; 
	//2. riga non esistente: se provider attivato inserisco riga in DS_Provider e metto il provider come invalido in attesa che l'utente inserisca i dati di connessione
	TRY
	{
		for (int i =0; i < m_aSynchroProviders.GetSize(); i++)
		{
			BOOL bCheck = TRUE;

			pSynchroProvider = m_aSynchroProviders.GetAt(i);
			//devo considerare come parent CRMInfinityProvider, questo per evitare di sincronizzare allegati di documenti che non fanno parte del processo di sincronizzazione del CRM
			if (pSynchroProvider->m_Name.CompareNoCase(DMSInfinityProvider) == 0)
				pSynchroProvider->m_pParentSynchroProvider = m_aSynchroProviders.GetProvider(CRMInfinityProvider);

			if (aProviderTU.FindRecord(pSynchroProvider->m_Name, TRUE) == TableUpdater::FOUND)
			{
				UpdateProviderRec(pSynchroProvider, &aProviderTU);
				CString strErrMsg;
				//valorizzo i parametri con il dato letto dalla tabella
				pSynchroProvider->ParseValuesFromXMLString(aProviderTU.GetRecord()->f_ProviderParameters);
				pSynchroProvider->m_IsDMSProvider = aProviderTU.GetRecord()->f_IsEAProvider;
				pSynchroProvider->m_Url = aProviderTU.GetRecord()->f_ProviderUrl;
				pSynchroProvider->m_Username = aProviderTU.GetRecord()->f_ProviderUser;
				pSynchroProvider->m_Password = aProviderTU.GetRecord()->f_ProviderPassword;
				pSynchroProvider->m_SkipCrtValidation = aProviderTU.GetRecord()->f_SkipCrtValidation;
				if (pSynchroProvider->m_IsDMSProvider && !pSynchroProvider->m_pParentSynchroProvider->IsValid())
					pSynchroProvider->SetValid(FALSE);
				else
					pSynchroProvider->SetValid(pSynchroProvider->IsActivated() && (!aProviderTU.GetRecord()->f_Disabled && (bCheck = CheckProvider(aProviderTU.GetRecord(), strErrMsg))));	
				
				if (!bCheck)
					AfxGetDiagnostic()->Add(strErrMsg, CDiagnostic::Warning);
				aProviderTU.UnlockCurrent();
			}
			else
			{
				pSynchroProvider->SetValid(FALSE);					
				aProviderTU.GetRecord()->f_Name = pSynchroProvider->m_Name;
				aProviderTU.GetRecord()->f_Description = pSynchroProvider->m_Description;
				aProviderTU.GetRecord()->f_IsEAProvider = pSynchroProvider->m_IsDMSProvider; //per il momento cablato poi verrà messo nel SynchroProfiles
				aProviderTU.GetRecord()->f_ProviderParameters = pSynchroProvider->UnparseValuesToXMLString();
				pSynchroProvider->SetValid(FALSE);
				aProviderTU.UpdateRecord();
				aProviderTU.UnlockCurrent();
				if (AfxIsActivated(MAGONET_APP, _NS_ACT("iMago")))
					AfxGetDiagnostic()->Add(cwsprintf(_TB("In order to use the synchronization provider {0-%s} please run the Configuration Wizard in Services - Connectors - I.Mago."), pSynchroProvider->m_Description), CDiagnostic::Warning);
				else
					AfxGetDiagnostic()->Add(cwsprintf(_TB("In order to use the synchronization provider {0-%s} please set its data using the Providers form in Task Builder Framework - Data Synchronization menu or put it in disable mode."), pSynchroProvider->m_Description), CDiagnostic::Warning);
			}
		}
		pNewSession->Close();
		SAFE_DELETE(pNewSession);
	}		
	CATCH(SqlException, e)
	{
		if (pNewSession)
		{
			pNewSession->Close();
			SAFE_DELETE(pNewSession);
		}
		AfxGetDiagnostic()->Add(e->m_strError, CDiagnostic::Error);
	}
	END_CATCH
		m_pDocuments = new CStringArray();

		if (!ImagoStudioRuntimeInstalled())
		{
			//per ogni provider carico l'elenco (già con il numero di sequenza di sincronizzazione giusto) dei documenti che partecipano alla sincronizzazione dati
			
			for (int i = 0; i < m_aSynchroProviders.GetSize(); i++)
			{
				pSynchroProvider = m_aSynchroProviders.GetAt(i);
				if (pSynchroProvider->m_pParentSynchroProvider)
					continue;
				//per ottimizzazione faccio l'union dei documenti in modo da avere l'insieme dei documenti da sincronizzare indipendentemente dal provider (serve per il clientdoc)
				for (int j = 0; j < pSynchroProvider->GetDocumentsToSynch()->GetSize(); j++)
				{
					CString docNamespace = pSynchroProvider->GetDocumentsToSynch()->GetAt(j)->m_strDocNamespace;
					BOOL bToAdd = (i == 0); //se è il primo provider allora non faccio il controllo di esistenza nell'array m_pDocuments
					if (i > 0)
					{
						bToAdd = TRUE;
						for (int k = 0; k < m_pDocuments->GetSize(); k++)
						{
							if (docNamespace.CompareNoCase(m_pDocuments->GetAt(k)) == 0)
							{
								bToAdd = FALSE;
								break;
							}
						}
					}
					if (bToAdd)
						m_pDocuments->Add(docNamespace);
				}
			}
		}
		else
		{
			for (int i = 0; i < m_aSynchroProviders.GetSize(); i++)
			{
				pSynchroProvider = m_aSynchroProviders.GetAt(i);
				//per ottimizzazione faccio l'union dei documenti in modo da avere l'insieme dei documenti da sincronizzare indipendentemente dal provider (serve per il clientdoc)
				for (int j = 0; j < pSynchroProvider->GetDocumentsToSynchImago()->GetSize(); j++)
				{
					CString docNamespace = pSynchroProvider->GetDocumentsToSynchImago()->GetAt(j)->m_strDocNamespace;
					BOOL bToAdd = (i == 0); //se è il primo provider allora non faccio il controllo di esistenza nell'array m_pDocuments
					if (i > 0)
					{
						bToAdd = TRUE;
						for (int k = 0; k < m_pDocuments->GetSize(); k++)
						{
							if (docNamespace.CompareNoCase(m_pDocuments->GetAt(k)) == 0)
							{
								bToAdd = FALSE;
								break;
							}
						}
					}
					if (bToAdd)
						m_pDocuments->Add(docNamespace);
				}
			}
		}
		
}

//=========================================================================================
BOOL CDataSynchroManager::GetSynchroProvider(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters)
{
	SAFE_DELETE(m_pDocuments);
	LoadSynchroProviders();

	CSynchroProvider* pSynchroProvider = m_aSynchroProviders.GetProvider(providerName);
	if (pSynchroProvider)
	{
		providerUrl = pSynchroProvider->m_Url;
		providerUser = pSynchroProvider->m_Username;
		providerPwd = pSynchroProvider->m_Password;

		for (int i = 0; i < pSynchroProvider->m_pParameters->GetSize(); i++)
		{
			VProviderParams* vProviderParam = (VProviderParams*)pSynchroProvider->m_pParameters->GetAt(i);
			if (vProviderParam->l_Name == _T("WSCode"))
				parameters = vProviderParam->l_Value;
		}
	} 
	return pSynchroProvider != NULL;
}

//=========================================================================================
BOOL CDataSynchroManager::CreateExternalServer(const CString& providerName, const CString& exteservername, const CString& connstr, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->CreateExternalServer(providerName, exteservername, connstr, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::CheckCompaniesToBeMapped(const CString& providerName, CString& strCompanyList, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->CheckCompaniesToBeMapped(providerName, strCompanyList, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::MapCompany(const CString& providerName, const CString& strAppReg, const int& strMagoCompany, const CString& strInfinityCompany, const CString& strTaxId, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->MapCompany(providerName, strAppReg, strMagoCompany, strInfinityCompany, strTaxId, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::UploadActionPackage(const CString& providerName, const CString& strActionPath, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->UploadActionPackage(providerName, strActionPath, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::SetProviderParameters(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& IAFModule, const CString& parameters, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->SetProviderParameters(providerName, providerUrl, providerUser, providerPwd, skipCrtValidation, IAFModule, parameters, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::TestProviderParameters(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, CString& strErrMsg, BOOL bDisabled /*= FALSE*/)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->TestProviderParameters(providerName, providerUrl, providerUser, providerPwd, skipCrtValidation, parameters, strErrMsg, bDisabled);
}

//=========================================================================================
BOOL CDataSynchroManager::SynchronizeErrorsRecovery(const CString&  providerName)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->SynchronizeErrorsRecovery(providerName);
}

//=========================================================================================
BOOL CDataSynchroManager::IsMassiveSynchronizing(/*const CString&  providerName*/)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->IsMassiveSynchronizing(/*providerName*/);
}

//=========================================================================================
BOOL CDataSynchroManager::IsMassiveValidating()
{
	return m_pDataSynchronizer && m_pDataSynchronizer->IsMassiveValidating();
}

//=========================================================================================
BOOL CDataSynchroManager::ReadPause()
{
	return m_Pause;
}

//=========================================================================================
void CDataSynchroManager::ChangedPause(BOOL m_PauseParam, const CString& providerName)
{
	m_pDataSynchronizer->PauseResume(providerName, m_PauseParam);
	m_Pause = m_PauseParam;
}


//=========================================================================================
void CDataSynchroManager::Abort(const CString& providerName)
{
	BOOL N = m_pDataSynchronizer->MassiveAbort(providerName);
}


//=========================================================================================
BOOL CDataSynchroManager::TestProviderParameters(const CString& providerName, CString& strErrMsg)
{
	TRDS_Providers _TRDS_Providers;
	_TRDS_Providers.SetSqlSession(AfxGetDefaultSqlSession());
	TableReader::FindResult result = _TRDS_Providers.FindRecord(providerName);

	if (result == TableReader::FOUND)
	{
		TDS_Providers* pRec = _TRDS_Providers.GetRecord();
		return TestProviderParameters(pRec->f_Name, pRec->f_ProviderUrl, pRec->f_ProviderUser, pRec->f_ProviderPassword, pRec->f_SkipCrtValidation, pRec->f_ProviderParameters, strErrMsg, FALSE);
	}

	return false;
}

//=========================================================================================
BOOL CDataSynchroManager::SaveDataSynchProviderInfo(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, const CString& iafmodules, CString& strErrMsg)
{
	TUDS_Providers aProviderTU;
	TDS_Providers pCRMInfinityRec;
	SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
	aProviderTU.SetSqlSession(pNewSession);
	aProviderTU.SetAutocommit();
	BOOL bCheck = TRUE;

	CSynchroProvider* pSynchroProvider = m_aSynchroProviders.GetProvider(providerName);
	TRY
	{
		if (pSynchroProvider && aProviderTU.FindRecord(providerName, TRUE) == TableUpdater::FOUND)
		{
			aProviderTU.GetRecord()->f_ProviderParameters = parameters;
			aProviderTU.GetRecord()->f_ProviderUrl = providerUrl;
			aProviderTU.GetRecord()->f_ProviderUser = providerUser;
			aProviderTU.GetRecord()->f_ProviderPassword = providerPwd;
			aProviderTU.GetRecord()->f_SkipCrtValidation = skipCrtValidation;
			aProviderTU.GetRecord()->f_IAFModules		= iafmodules;
			pSynchroProvider->ParseValuesFromXMLString(parameters);
			pSynchroProvider->m_Url = providerUrl;
			pSynchroProvider->m_Username = providerUser;
			pSynchroProvider->m_Password = providerPwd;
			pSynchroProvider->m_SkipCrtValidation = skipCrtValidation;
			pSynchroProvider->m_IAFModules = iafmodules;
			pSynchroProvider->SetValid(TRUE);
			aProviderTU.UpdateRecord();
			aProviderTU.UnlockCurrent();
			SetProviderParameters(providerName, providerUrl, providerUser, providerPwd, skipCrtValidation, iafmodules, parameters, strErrMsg);
		}
		pNewSession->Close();
		SAFE_DELETE(pNewSession);
	}
	CATCH(SqlException, e)
	{
		if (pNewSession)
		{
			pNewSession->Close();
			SAFE_DELETE(pNewSession);
		}
		AfxGetDiagnostic()->Add(e->m_strError, CDiagnostic::Error);
		bCheck = FALSE;
	}
	END_CATCH
	return bCheck;
}

//=========================================================================================
BOOL CDataSynchroManager::SetConvergenceCriteria(const CString& providerName, const CString& xmlCriteria, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->SetConvergenceCriteria(providerName, xmlCriteria,strErrMsg);
}

//=========================================================================================
BOOL  CDataSynchroManager::GetConvergenceCriteria(const CString& providerName, const CString& actionName, CString& xmlCriteria, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->GetConvergenceCriteria(providerName, actionName, xmlCriteria, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::GetSynchroProviderInfo(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters, CString& iafmodules, BOOL& skipcrtvalid)
{
	TUDS_Providers aProviderTU;
	TDS_Providers pCRMInfinityRec;
	SqlSession* pNewSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
	aProviderTU.SetSqlSession(pNewSession);
	aProviderTU.SetAutocommit();
	BOOL bCheck = TRUE;

	CSynchroProvider* pSynchroProvider = m_aSynchroProviders.GetProvider(providerName);
	TRY
	{
		if (pSynchroProvider && aProviderTU.FindRecord(providerName, FALSE) == TableUpdater::FOUND)
		{
			parameters = aProviderTU.GetRecord()->f_ProviderParameters;
			providerUrl = aProviderTU.GetRecord()->f_ProviderUrl;
			providerUser = aProviderTU.GetRecord()->f_ProviderUser;
			providerPwd = aProviderTU.GetRecord()->f_ProviderPassword;
			//TODO
			iafmodules = aProviderTU.GetRecord()->f_IAFModules;
			skipcrtvalid = aProviderTU.GetRecord()->f_SkipCrtValidation;
		}
		else
			return FALSE;
		pNewSession->Close();
		SAFE_DELETE(pNewSession);
	}
	CATCH(SqlException, e)
	{
		if (pNewSession)
		{
			pNewSession->Close();
			SAFE_DELETE(pNewSession);
		}
		AfxGetDiagnostic()->Add(e->m_strError, CDiagnostic::Error);
		bCheck = FALSE;
	}
	END_CATCH

	return bCheck;
}

//=========================================================================================
BOOL CDataSynchroManager::SetGadgetPerm(const CString& providerName, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->SetGadgetPerm(providerName, strErrMsg);
}

//=========================================================================================
BOOL CDataSynchroManager::PurgeSynchroConnectorLog()
{
	return m_pDataSynchronizer && m_pDataSynchronizer->PurgeSynchroConnectorLog();
}

//=========================================================================================
BOOL CDataSynchroManager::CheckVersion(const CString& providerName, CString& strCompanyList, CString& strErrMsg)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->CheckVersion(providerName, strCompanyList, strErrMsg);
}


//=========================================================================================
void CDataSynchroManager::DestroyVersionDlg()
{
	m_pVersionDlg = NULL;
}

//=========================================================================================
BOOL CDataSynchroManager::SynchronizeErrorsRecoveryImago(const CString& m_ProviderName, CString& m_strRecoveryGuid)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->SynchronizeErrorsRecoveryImago(m_ProviderName, m_strRecoveryGuid);
}

//========= == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == ==
BOOL CDataSynchroManager::IsActionQueued(const CString& m_strRecoveryGuid)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->IsActionQueued(m_strRecoveryGuid);
}

//========= == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == ==
BOOL CDataSynchroManager::IsActionRunning(const CString& m_strRecoveryGuid)
{
	return m_pDataSynchronizer && m_pDataSynchronizer->IsActionRunning(m_strRecoveryGuid);
}

//========= == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == ==
BOOL CDataSynchroManager::IsAlive()
{
	return m_pDataSynchronizer && m_pDataSynchronizer->IsAlive();
}


//=========================================================================================
CDataSynchroManager* AFXAPI AfxGetDataSynchroManager()
{
	CCompanyContext* pContext = AfxGetCompanyContext();

	return (CDataSynchroManager*)pContext->GetDataSynchroManager();	
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CVersionControlTabDialog, CParsedDialog)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CVersionControlTabDialog, CParsedDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CVersionControlTabDialog::Create()
{
	return CParsedDialog::Create(IDD_DS_PROVIDERS_DIALOG, AfxGetMainWnd(), _NS_DLG("Extensions.TbDataSynchroClient.Providers"));
}

//----------------------------------------------------------------------------
void CVersionControlTabDialog::OnOK()
{
	CParsedDialog::OnOK();
	AfxGetDataSynchroManager()->DestroyVersionDlg();
}


//----------------------------------------------------------------------------
void CVersionControlTabDialog::OnCancel()
{
	CParsedDialog::OnCancel();
	AfxGetDataSynchroManager()->DestroyVersionDlg();
}

//----------------------------------------------------------------------------
BOOL CVersionControlTabDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_CtrlErrorMsg.SubclassEdit(IDC_PROVIDERS_CONTROL_TEXT, this, _NS_CTRL("ERROR"));

	CenterWindow();
	SetToolbarStyle(CParsedDialog::BOTTOM, 32, TRUE, TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CVersionControlTabDialog::PostNcDestroy()
{
	delete this;
}