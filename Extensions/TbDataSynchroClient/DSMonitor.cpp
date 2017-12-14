#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbGenlib\TBLinearGauge.h>
#include <TbGes\BODYEDIT.H>
#include <MsXml2.h>
#include "DSMonitor.h"
#include "UIDSMonitor.h"
#include "UIDSMonitor.hjson"
#include "UICommon.h"
#include "TbDataSynchroClientEnums.h"
#include "DSManager.h"
#include "DSComponents.h"

static TCHAR szParamProvider		[] = _T("P1");
static TCHAR szParamDocNS			[] = _T("P2");
static TCHAR szParamStatus			[] = _T("P3");
static TCHAR szAllStatus			[] = _T("P4");
static TCHAR szAllDate				[] = _T("P5");
static TCHAR szFromDate				[] = _T("P6");
static TCHAR szToDate				[] = _T("P7");
static TCHAR szParamStatusSynch		[] = _T("P8");
static TCHAR szParamStatusErr		[] = _T("P9");   
static TCHAR szParamProvider1		[] = _T("P10");

#define PAGE_ELEMENT_SIZE			100

//==============================================================================
const TCHAR szTbDataSynchroClient[] = _T("Module.Extensions.TbDataSynchroClient");
const CTBNamespace snsTbDataSynchroClient = szTbDataSynchroClient;
//==============================================================================
const TCHAR szDataSynchronizerMonitor[] = _T("DataSynchronizerMonitor");
const TCHAR szRefreshTimer[] = _T("RefreshTimer");

static TCHAR szNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");

static TCHAR szGlyphF[] = _T("Glyph");

///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_SynchroInfoDocSummary implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_SynchroInfoDocSummary, TDS_SynchronizationInfo)

//-----------------------------------------------------------------------------
TEnhDS_SynchroInfoDocSummary::TEnhDS_SynchroInfoDocSummary(BOOL bCallInit)
{
    BindRecord();	
    if (bCallInit) 
        Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_SynchroInfoDocSummary::BindRecord()
{
    BEGIN_BIND_DATA	();
        LOCAL_STR(_NS_LFLD("TEnhDSDocSum_SynchStatusBmp"),		l_SynchStatusBmp,		512);
        LOCAL_STR (_NS_LFLD("TEnhDSDocSum_NoDocStatusSynchro"),	l_NoDocStatusSynchro,	10);
        LOCAL_STR (_NS_LFLD("TEnhDSDocSum_NoDocStatusError"),	l_NoDocStatusError,		10);
    END_BIND_DATA();
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTSynchroInfoMonitor implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTSynchroInfoMonitor, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTSynchroInfoMonitor::DBTSynchroInfoMonitor
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument,
	DataBool bDelta,
	DataDate startSynchroDate
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("SynchronizationInfoMonitor"), TRUE)
{
	m_bDelta = bDelta;
	m_startSynchroDate.SetFullDate(TRUE);
	m_startSynchroDate = startSynchroDate;
}

//-----------------------------------------------------------------------------
void DBTSynchroInfoMonitor::OnDefineQuery	()
{
    m_pTable->SelectAll();

    //se non arrivo dalla batch
    //select * from DS_SynchronizationInfo 
    //where ProviderName = ?		 AND 
    //      DocNamespace = ?		 AND 
    //      (1=? OR SynchStatus = ?) AND
    //      (1=? OR (SynchDate >= ? AND SynchDate <= ?))

    //se invece arrivo dalla batch
    //select * from DS_SynchronizationInfo 
    //where ProviderName = ? AND 
    //      DocNamespace = ? AND 
    //      SynchStatus <> ?

	//questo permette di gestire il flag All Status in modo da non dover aprire e chiudere ogni volta il cursore per rifare la OnDefineQuery
	if (!m_bDelta)
	{
		m_pTable->m_strFilter = (GetDocument()->m_bFromBatch)
															? cwsprintf(
																_T("(%s = ? OR %s = ?) AND %s = ? AND %s <> ?"),  // l'OR sul provider è dovuto all'integrazione CRMinfinity+DMSInfinity
																TDS_SynchronizationInfo::szProviderName,
																TDS_SynchronizationInfo::szProviderName,
																TDS_SynchronizationInfo::szDocNamespace,
																TDS_SynchronizationInfo::szSynchStatus
															)
															: cwsprintf(
																_T("%s = ? AND %s = ? AND ( '1' = ? OR %s = ? ) AND ('1' = ? OR (%s >= ? AND %s <= ?))"),
																TDS_SynchronizationInfo::szProviderName,
																TDS_SynchronizationInfo::szDocNamespace,
																TDS_SynchronizationInfo::szSynchStatus,
																TDS_SynchronizationInfo::szSynchDate,
																TDS_SynchronizationInfo::szSynchDate
															);


		if (!GetDocument()->m_bFromBatch)
		{
			m_pTable->AddParam(szParamProvider, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamDocNS, GetSynchroInfoRec()->f_DocNamespace);
			m_pTable->AddParam(szAllStatus, DATA_BOOL_TYPE, 1);
			m_pTable->AddParam(szParamStatus, GetSynchroInfoRec()->f_SynchStatus);
			m_pTable->AddParam(szAllDate, DATA_BOOL_TYPE, 1);
			m_pTable->AddParam(szFromDate, GetSynchroInfoRec()->f_SynchDate);
			m_pTable->AddParam(szToDate, GetSynchroInfoRec()->f_SynchDate);
		}
		else
		{
			m_pTable->AddParam(szParamProvider, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamProvider1, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamDocNS, GetSynchroInfoRec()->f_DocNamespace);
			m_pTable->AddParam(szParamStatus, GetSynchroInfoRec()->f_SynchStatus);
		}
	}
	else 
	{
		m_pTable->m_strFilter = (GetDocument()->m_bFromBatch)
			? cwsprintf(
				_T("(%s = ? OR %s = ?) AND %s = ? AND %s <> ? AND %s = ?"),  // l'OR sul provider è dovuto all'integrazione CRMinfinity+DMSInfinity
				TDS_SynchronizationInfo::szProviderName,
				TDS_SynchronizationInfo::szProviderName,
				TDS_SynchronizationInfo::szDocNamespace,
				TDS_SynchronizationInfo::szSynchStatus,
				TDS_SynchronizationInfo::szStartSynchDate
			)
			: cwsprintf(
				_T("%s = ? AND %s = ? AND ( '1' = ? OR %s = ? ) AND ('1' = ? OR (%s >= ? AND %s <= ?)) AND %s = ?"),
				TDS_SynchronizationInfo::szProviderName,
				TDS_SynchronizationInfo::szDocNamespace,
				TDS_SynchronizationInfo::szSynchStatus,
				TDS_SynchronizationInfo::szSynchDate,
				TDS_SynchronizationInfo::szSynchDate,
				TDS_SynchronizationInfo::szStartSynchDate
			);


		if (!GetDocument()->m_bFromBatch)
		{
			m_pTable->AddParam(szParamProvider, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamDocNS, GetSynchroInfoRec()->f_DocNamespace);
			m_pTable->AddParam(szAllStatus, DATA_BOOL_TYPE, 1);
			m_pTable->AddParam(szParamStatus, GetSynchroInfoRec()->f_SynchStatus);
			m_pTable->AddParam(szAllDate, DATA_BOOL_TYPE, 1);
			m_pTable->AddParam(szFromDate, GetSynchroInfoRec()->f_SynchDate);
			m_pTable->AddParam(szToDate, GetSynchroInfoRec()->f_SynchDate);
			m_pTable->AddParam(TDS_SynchronizationInfo::szStartSynchDate, GetSynchroInfoRec()->f_StartSynchDate);
		}
		else
		{
			m_pTable->AddParam(szParamProvider, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamProvider1, GetSynchroInfoRec()->f_ProviderName);
			m_pTable->AddParam(szParamDocNS, GetSynchroInfoRec()->f_DocNamespace);
			m_pTable->AddParam(szParamStatus, GetSynchroInfoRec()->f_SynchStatus);
			m_pTable->AddParam(TDS_SynchronizationInfo::szStartSynchDate, GetSynchroInfoRec()->f_StartSynchDate);
		}

	}
	

	m_pTable->AddSortColumn(GetSynchroInfoRec()->f_SynchDate, TRUE);
	m_pTable->AddSortColumn(GetSynchroInfoRec()->f_ProviderName);
}

//-----------------------------------------------------------------------------
void DBTSynchroInfoMonitor::OnPrepareQuery()
{

	DataStr str=GetSynchroInfoRec()->f_ProviderName;
	TEnhDS_SynchronizationInfo* d=GetSynchroInfoRec();
	
	if (!m_bDelta)
	{
		if (!GetDocument()->m_bFromBatch)
		{
			m_pTable->SetParamValue(szParamProvider, GetDocument()->m_ProviderName);
			m_pTable->SetParamValue(szParamDocNS, GetDocument()->m_DocToSynch);
			m_pTable->SetParamValue(szAllStatus, GetDocument()->m_SynchStatusAll);
			m_pTable->SetParamValue(szParamStatus, GetDocument()->m_SynchStatus);
			m_pTable->SetParamValue(szAllDate, GetDocument()->m_SynchDateAll);
			m_pTable->SetParamValue(szFromDate, DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0));
			m_pTable->SetParamValue(szToDate, DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59));
		}
		else
		{
			m_pTable->SetParamValue(szParamProvider, GetDocument()->m_ProviderName);
			if (GetDocument()->m_ProviderName.IsEqual(DataStr(CRMInfinityProvider)))
				m_pTable->SetParamValue(szParamProvider1, DataStr(DMSInfinityProvider));
			else
				m_pTable->SetParamValue(szParamProvider1, GetDocument()->m_ProviderName);

			m_pTable->SetParamValue(szParamDocNS, GetDocument()->m_DocToSynch);
			m_pTable->SetParamValue(szParamStatus, GetDocument()->m_SynchStatus);
		}
	}
	else 
	{
		if (!GetDocument()->m_bFromBatch)
		{
			m_pTable->SetParamValue(szParamProvider, GetDocument()->m_ProviderName);
			m_pTable->SetParamValue(szParamDocNS, GetDocument()->m_DocToSynch);
			m_pTable->SetParamValue(szAllStatus, GetDocument()->m_SynchStatusAll);
			m_pTable->SetParamValue(szParamStatus, GetDocument()->m_SynchStatus);
			m_pTable->SetParamValue(szAllDate, GetDocument()->m_SynchDateAll);
			m_pTable->SetParamValue(szFromDate, DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0));
			m_pTable->SetParamValue(szToDate, DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59));
			m_pTable->SetParamValue(TDS_SynchronizationInfo::szStartSynchDate, m_startSynchroDate);
		}
		else
		{
			m_pTable->SetParamValue(szParamProvider, GetDocument()->m_ProviderName);
			if (GetDocument()->m_ProviderName.IsEqual(DataStr(CRMInfinityProvider)))
				m_pTable->SetParamValue(szParamProvider1, DataStr(DMSInfinityProvider));
			else
				m_pTable->SetParamValue(szParamProvider1, GetDocument()->m_ProviderName);

			m_pTable->SetParamValue(szParamDocNS, GetDocument()->m_DocToSynch);
			m_pTable->SetParamValue(szParamStatus, GetDocument()->m_SynchStatus);
			m_pTable->SetParamValue(TDS_SynchronizationInfo::szStartSynchDate, m_startSynchroDate);
		}
	}
}

//-----------------------------------------------------------------------------
void DBTSynchroInfoMonitor::ReloadData()
{
    DataStr sTitle = _T("");

    RemoveAll();
    LocalFindData(FALSE);

}

//-----------------------------------------------------------------------------
void DBTSynchroInfoMonitor:: OnPrepareAuxColumns(SqlRecord* pRecord)
{
    TEnhDS_SynchronizationInfo* pEnhSynchRec = (TEnhDS_SynchronizationInfo*)pRecord;
    GetDocument()->GetDecodingInfo(pEnhSynchRec->f_DocTBGuid, pEnhSynchRec->l_Code, pEnhSynchRec->l_Description);

    CWorker* pWorker = (pRecord && pRecord->f_TBCreatedID >= 0) ? AfxGetWorkersTable()->GetWorker(pRecord->f_TBCreatedID) : NULL;
    pEnhSynchRec->l_WorkerDescri = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%s"), pEnhSynchRec->f_WorkerID.Str());

    pEnhSynchRec->l_SynchStatusBmp    = OnGetSyncStatusBmp(pEnhSynchRec->f_SynchStatus);	
    pEnhSynchRec->l_SynchDirectionBmp = OnGetSyncDirectionBmp(pEnhSynchRec->f_SynchDirection);	

    if	(
            GetDocument()->m_pTRDS_ActionsLog->FindRecord(pEnhSynchRec->f_DocTBGuid, pEnhSynchRec->f_ProviderName) == TableReader::FOUND &&
            !GetDocument()->m_pTRDS_ActionsLog->GetRecord()->f_SynchMessage.IsEmpty()
        )
        pEnhSynchRec->l_SynchMessage = GetDocument()->m_pTRDS_ActionsLog->GetRecord()->f_SynchMessage;
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTVSynchroMonitorDocSummay implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTSynchroMonitorDocSummay, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTSynchroMonitorDocSummay::DBTSynchroMonitorDocSummay
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("SynchroMonitorDocSummary"), TRUE)
{
}

///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(VSynchroInfoDocSummary, VSynchronizationInfo)

//-----------------------------------------------------------------------------
VSynchroInfoDocSummary::VSynchroInfoDocSummary(BOOL bCallInit)
{
	BindRecord();
	if (bCallInit)
		Init();
}

//-----------------------------------------------------------------------------
void VSynchroInfoDocSummary::BindRecord()
{
	BEGIN_BIND_DATA();
	LOCAL_STR(_NS_LFLD("SynchStatusBmp"),		l_SynchStatusBmp, 512);
	LOCAL_STR(_NS_LFLD("NoDocStatusSynchro"),	l_NoDocStatusSynchro, 10);
	LOCAL_STR(_NS_LFLD("NoDocStatusError"),		l_NoDocStatusError, 10);
	LOCAL_STR(_NS_LFLD("NsWithFlows"),			l_NsWithFlows, 128);
	LOCAL_STR(_NS_LFLD("Flow"),					l_Flow, 128);
	END_BIND_DATA();
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTVSynchroInfoMonitor implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTVSynchroInfoMonitor, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTVSynchroInfoMonitor::DBTVSynchroInfoMonitor
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument,
	DataBool bDelta,
	DataDate startSynchroDate
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("VSynchroInfoMonitor"), ALLOW_EMPTY_BODY, FALSE)
{
	m_bDelta = bDelta;
	m_startSynchroDate.SetFullDate(TRUE);
	m_startSynchroDate = startSynchroDate;
}

//-----------------------------------------------------------------------------
void DBTVSynchroInfoMonitor::ReloadData()
{
	DataStr sTitle = _T("");
	CXMLDocumentObject aXMLModDoc;
	VEnh_SynchronizationInfo* pSynchroRec = NULL;
	CString XmlLog;

	if (!GetDocument()->m_pDBVDetailDocSummary) return;

	VSynchroInfoDocSummary * pRec = (VSynchroInfoDocSummary*)GetDocument()->m_pDBVDetailDocSummary->GetCurrentRow();
	if (!pRec) return;

	RemoveAll();

	if (GetDocument()->m_bNamespaceChanged)
	{
		GetDocument()->m_nDetailPage = 0;
		GetDocument()->m_nDetailPageStr = _TB("Pag.") + (GetDocument()->m_nPageTot == 0 ? _T("0") : (GetDocument()->m_nDetailPage +1).ToString()) + _T("/") + GetDocument()->m_nPageTot.ToString();
	}

	//CString XmlLog = AfxGetDataSynchroManager()->GetLogsByNamespace(GetDocument()->m_ProviderName, GetDocument()->m_DocToSynch);
	if (!m_bDelta)
	{
		if (!GetDocument()->m_bFromBatch)
			XmlLog = AfxGetDataSynchroManager()->GetSynchroLogsByFilters(
				GetDocument()->m_ProviderName,
				GetDocument()->m_DocToSynch,
				GetDocument()->m_bDelta,
				GetDocument()->m_bFromBatch,
				GetDocument()->m_SynchStatusAll,
				DataBool(GetDocument()->m_SynchStatus == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
				GetDocument()->m_SynchDateAll,
				DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0),
				DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59),
				DataDate(),
				pRec->l_Flow,
				GetDocument()->m_nDetailPage * PAGE_ELEMENT_SIZE
			);
		else
		{
			XmlLog = AfxGetDataSynchroManager()->GetSynchroLogsByFilters(
				GetDocument()->m_ProviderName,
				GetDocument()->m_DocToSynch,
				GetDocument()->m_bDelta,
				GetDocument()->m_bFromBatch,
				DataBool(FALSE),
				DataBool(GetDocument()->m_SynchStatus == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
				DataBool(TRUE),
				DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0),
				DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59),
				DataDate(),
				pRec->l_Flow,
				GetDocument()->m_nDetailPage * PAGE_ELEMENT_SIZE
			);
		}

	}
	else
	{
		if (!GetDocument()->m_bFromBatch)
			XmlLog = AfxGetDataSynchroManager()->GetSynchroLogsByFilters(
				GetDocument()->m_ProviderName,
				GetDocument()->m_DocToSynch,
				GetDocument()->m_bDelta,
				GetDocument()->m_bFromBatch,
				GetDocument()->m_SynchStatusAll,
				DataBool(GetDocument()->m_SynchStatus == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
				GetDocument()->m_SynchDateAll,
				DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0),
				DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59),
				GetDocument()->m_SynchDateFrom,
				pRec->l_Flow,
				GetDocument()->m_nDetailPage * PAGE_ELEMENT_SIZE
			);
		else
			XmlLog = AfxGetDataSynchroManager()->GetSynchroLogsByFilters(
				GetDocument()->m_ProviderName,
				GetDocument()->m_DocToSynch,
				GetDocument()->m_bDelta,
				GetDocument()->m_bFromBatch,
				DataBool(FALSE),
				DataBool(GetDocument()->m_SynchStatus == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
				DataBool(TRUE),
				DataDate(GetDocument()->m_SynchDateFrom.Day(), GetDocument()->m_SynchDateFrom.Month(), GetDocument()->m_SynchDateFrom.Year(), 0, 0, 0),
				DataDate(GetDocument()->m_SynchDateTo.Day(), GetDocument()->m_SynchDateTo.Month(), GetDocument()->m_SynchDateTo.Year(), 23, 59, 59),
				GetDocument()->m_SynchDateFrom,
				pRec->l_Flow,
				GetDocument()->m_nDetailPage * PAGE_ELEMENT_SIZE
			);
	}
	TRY
	{
			if (XmlLog != _T(""))
			{
				aXMLModDoc.EnableMsgMode(FALSE);

				if (!aXMLModDoc.LoadXML(XmlLog))
				{
					if (!aXMLModDoc.LoadXML(XmlLog))
					{
						pSynchroRec = (VEnh_SynchronizationInfo*)AddRecord();
						pSynchroRec->l_DocNamespace = _T("");
						pSynchroRec->l_DocTBGuid = DataGuid(_T("-1"));
						pSynchroRec->l_SynchMessage = _TB("Unable to parse xml logs.");
						return;
					}
				}

				// root SynchroProfiles
				CXMLNode* pRoot = aXMLModDoc.GetRoot();
				// Provider
				CString strValue;
				BOOL				bOK = FALSE;

				CXMLNodeChildsList* pLogsNode = pRoot->GetChilds();
				CXMLNode* pNode = pRoot->GetChildByName(_T("Count"));

				if (pNode)
				{
					bOK = pNode->GetText(strValue);
					int a = _ttoi(strValue);
					((DSMonitor*)GetDocument())->m_nPageTot.Assign( (_ttoi(strValue) % PAGE_ELEMENT_SIZE) > 0 ? (_ttoi(strValue) / PAGE_ELEMENT_SIZE) + 1 : _ttoi(strValue) / PAGE_ELEMENT_SIZE );
					((DSMonitor*)GetDocument())->m_nDetailPageStr = _TB("Pag.") + (((DSMonitor*)GetDocument())->m_nPageTot == 0 ? _T("0") : (((DSMonitor*)GetDocument())->m_nDetailPage+1).ToString()) + _T("/") + ((DSMonitor*)GetDocument())->m_nPageTot.ToString();
				}

				pNode = pRoot->GetChildByName(_T("Logs"));

				if (pNode)
				{

					for (int i = 0; i < pNode->GetChilds()->GetSize(); i++)
					{
						CXMLNode* pLog = pNode->GetChilds()->GetAt(i);

						if (pLog)
						{
							CString sAttrValue;
							CString strStatus;

							CString strTexts;
							CString strTimeStamp;
							DataEnum SyncStatus;
							CString strDir;

							CXMLNode * pOrigin = pLog->GetChildByName(_T("OriginId"));
							bOK = pOrigin->GetText(sAttrValue);

							if (bOK)
							{
								pSynchroRec = (VEnh_SynchronizationInfo*)AddRecord();

								//pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO));
								pSynchroRec->l_DocTBGuid.AssignFromXMLString(sAttrValue.MakeUpper());

								bOK = pLog->GetChildByName(_T("Status"))->GetText(strStatus);
								if (bOK)
									pSynchroRec->l_SynchStatus = strStatus == _T("True") ? DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO) : DataEnum(E_SYNCHROSTATUS_TYPE_ERROR);
								CXMLNode * pMessage = pLog->GetChildByName(_T("Message"));

								if (pMessage)
								{
									//IXMLDOMCDATASectionPtr pCData = pMessage;
									BSTR bstrToConvert;
									//HRESULT hr = pCData->get_data(&bstrToConvert);
									//if (SUCCEEDED(hr)) {
									//	if (bOK)
											////pSynchroRec->l_SynchMessage.Assign(bstrToConvert);
									//}
									pMessage->GetIXMLDOMNodePtr()->get_text(&bstrToConvert);
									pSynchroRec->l_SynchMessage.Assign(bstrToConvert);
								}
								bOK = pLog->GetChildByName(_T("Date"))->GetText(strTimeStamp);
								if (bOK)
									pSynchroRec->l_SynchDate.AssignFromXMLString(strTimeStamp);

								bOK = pLog->GetChildByName(_T("Direction"))->GetText(strDir);
								if (bOK)
								{
									if (strDir == _T("Outbound"))
										pSynchroRec->l_SynchDirection = DataEnum(E_SYNCHRODIRECTION_TYPE_OUTBOUND);
									else
										pSynchroRec->l_SynchDirection = DataEnum(E_SYNCHRODIRECTION_TYPE_INBOUND);
								}
								pSynchroRec->l_SynchDirectionBmp = OnGetSyncDirectionBmp(pSynchroRec->l_SynchDirection);

								pSynchroRec->l_DocNamespace = GetDocument()->m_DocToSynch;
								GetDocument()->GetDecodingInfo(pSynchroRec->l_DocTBGuid, pSynchroRec->l_Code, pSynchroRec->l_Description);
								pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(pSynchroRec->l_SynchStatus);
								pSynchroRec->l_SynchDirectionBmp = OnGetSyncDirectionBmp(pSynchroRec->l_SynchDirection);
							}
						}
					}
			}
		}
	}	
	CATCH(CException, e)
	{
		pSynchroRec = (VEnh_SynchronizationInfo*)AddRecord();
		pSynchroRec->l_DocNamespace = _T("");
		pSynchroRec->l_DocTBGuid = DataGuid(_T("-1"));
		pSynchroRec->l_SynchMessage = _TB("Unable to parse xml logs.");
		return;
	}
	END_CATCH

}

//-----------------------------------------------------------------------------
void DBTVSynchroInfoMonitor::OnPrepareAuxColumns(SqlRecord* pRecord)
{

}

///////////////////////////////////////////////////////////////////////////////
//             class DBTVSynchroMonitorDocSummay implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTVSynchroMonitorDocSummay, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTVSynchroMonitorDocSummay::DBTVSynchroMonitorDocSummay
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("SynchroMonitorDocSummaryIMS"),  ALLOW_EMPTY_BODY, FALSE)
{
}


//-----------------------------------------------------------------------------	
DBTVSynchroMonitorDocSummay::DBTVSynchroMonitorDocSummay()
{
}

//-----------------------------------------------------------------------------
void DBTVSynchroMonitorDocSummay::Init()
{
	DBTSlaveBuffered::Init();
}

//-----------------------------------------------------------------------------
void DBTVSynchroMonitorDocSummay::OnDefineQuery()
{
}

//-----------------------------------------------------------------------------
void DBTVSynchroMonitorDocSummay::OnPrepareQuery()
{
}

//-----------------------------------------------------------------------------
DataObj* DBTVSynchroMonitorDocSummay::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTVSynchroMonitorDocSummay::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VSynchroInfoDocSummary)));
}

//-----------------------------------------------------------------------------	
DataObj* DBTVSynchroMonitorDocSummay::GetDuplicateKeyPos(SqlRecord* pRec)
{
	return NULL;
}


//////////////////////////////////////////////////////////////////////////////
//									DSMonitor	                            //
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(DSMonitor, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DSMonitor, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DSMonitor)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SHOW_ONLY_DOC_WITH_ERR,		OnShowOnlyDocWithErrChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_DOCUMENTS_COMBO,			OnDocNamespaceChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_PROVIDERS_COMBO,			OnProviderChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSTATUS_ALL,			OnSynchStatusAllChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSTATUS_COMBO,			OnSynchStatusChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSDATE_ALL,				OnSynchDateSelChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSDATE_SEL,				OnSynchDateSelChanged)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSDATE_FROM,			OnSynchDateFromEntered)
		ON_EN_VALUE_CHANGED		(IDC_DS_MONITOR_SYNCHSDATE_TO,				OnSynchDateToEntered)

		ON_COMMAND				(ID_DS_MONITOR_ERR_START_RECOVERY_BUTTON,   OnErrorsRecoveryClick)
		ON_COMMAND				(ID_DS_MONITOR_PAUSE_BUTTON,			    PauseMassiveSynchro)
		ON_COMMAND				(ID_DS_MONITOR_CONTINUE_BUTTON,				ContinueMassiveSynchro)
		ON_COMMAND				(ID_DS_MONITOR_ABORT_BUTTON,				AbortMassiveSynchro)
		ON_COMMAND				(ID_DS_MONITOR_REFRESH,						OnMonitorRefresh)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_ERR_START_RECOVERY_BUTTON,	OnErrorsRecoveryUpdate)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_PAUSE_BUTTON,				PauseMassiveSynchroEnable)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_CONTINUE_BUTTON,				ContinueMassiveSynchroEnable)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_ABORT_BUTTON,				AbortMassiveSynchroEnable)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_REFRESH,						OnMonitorRefreshUpdate)

		ON_COMMAND				(ID_MONITOR_OPEN_DOCUMENT,					OnMonitorDetailOpenDocument)
		ON_UPDATE_COMMAND_UI	(ID_MONITOR_OPEN_DOCUMENT,					OnUpdateDetailOpenDocument)
		ON_COMMAND				(ID_MONITOR_COPY_MSG,						OnMonitorDetailCopyMessage)	
		ON_UPDATE_COMMAND_UI	(ID_MONITOR_COPY_MSG,						OnUpdateDetailCopyMessage)

		ON_CONTROL				(BEN_ROW_CHANGED, IDC_DS_MONITOR_RECOVERY_ERR_BE, OnMonitorSummaryRowChanged)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DSMonitor::DSMonitor()
	:
	m_pDBTDetail						(NULL),
	m_pDBTDetailDocSummary				(NULL),
	m_pDocumentsToMonitor				(NULL),
	m_pOnlyDocWithErr					(NULL),
	m_pMassiveSynchroInfo				(NULL),
	m_SynchStatusAll					(FALSE),
	m_SynchDateAll						(TRUE),
	m_SynchDateSel						(FALSE),
	m_SynchStatus						(E_SYNCHROSTATUS_TYPE_SYNCHRO),
	m_bFromBatch						(FALSE),
	m_bAutoRefresh						(TRUE),
	m_bShowOnlyDocWithErr				(FALSE),
	m_bIsMassiveSynchronizing			(TRUE),
	m_bSynchronizationEnded				(FALSE),
	m_nSynchronizationCounter			(3),
	m_bSynchronizationStarted			(FALSE),
	m_bNeedMassiveSynchro				(FALSE),
	m_bErrorsOccurred					(FALSE),
	m_pTRDS_ActionsLog					(NULL),
	m_pRRDS_SynchronizationInfoByStatus	(NULL),
	m_pResultsTilePanel					(NULL),
	m_pSummaryTile						(NULL),
	m_pMonitorTile						(NULL),
	m_MonitorRefresh					(0),
	m_GaugeRefresh						(300),
	m_nValueGauge						(0.0),
	m_GaugeDescription					(_T("")),
	m_nGaugeUpperRange					(100.0),
	m_pGaugeLabel						(NULL),
	m_pGauge							(NULL),
	m_bDelta							(FALSE),
	m_bFirstTimeRunning					(TRUE),
	m_Pause								(FALSE),
	m_Abort								(FALSE),
	m_bIsActivatedImagoStudio			(FALSE),
	m_pDBVTDetail						(NULL),
	m_pDBVDetailDocSummary				(NULL),
	m_bNamespaceChanged					(FALSE)
{
    int		nResult;
    TCHAR	buffer[512];

    m_bBatch = TRUE;

	m_LineRequested = 0;

	m_pDocumentsToMonitor = new CSynchroDocInfoArray();
		
	m_SynchDateFrom.	SetFullDate();
	m_SynchDateTo.		SetFullDate();

    m_pTRDS_ActionsLog = new TRDS_ActionsLog(this);

    m_bIsActivatedCRMInfinity = AfxIsActivated(MAGONET_APP, CRMINFINITY_FUNCTIONALITY);

	m_bIsActivatedImagoStudio = AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled();

    //refresh monitor
    int monitorSetting = (*((DataInt*)AfxGetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szRefreshTimer, DataInt(10))));
    // Per trasformarlo in secondi va' moltiplicato per 1000 (cosi vuole il SetTimer())
    m_MonitorRefresh = monitorSetting * 1000;

    nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphInbound);
    m_InboundPic = buffer;
    
    nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphOutbound);
    m_OutboundPic = buffer;

    nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphOk);
    m_StatusOkPic = buffer;

    nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphWait);
    m_StatusWaitPic = buffer;

    nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconError);
    m_StatusErrorPic = buffer;

	nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphRemove);
	m_StatusExcludedPic = buffer;

	nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconWarning);
	m_StatusWarningPic = buffer;

	m_nDetailPage = 0;
	m_nPageTot = 0;
	m_nDetailPageStr = _TB("Pag.") + (m_nPageTot == 0 ? _T("0") : (m_nDetailPage+1).ToString()) + _T("/") + m_nPageTot.ToString();
}

//-----------------------------------------------------------------------------
DSMonitor::~DSMonitor()
{
    if (m_bIsActivatedCRMInfinity) // La gestonione "Recovery Errors" è gestita solo per CRM Infinity
    {
        if (m_pOnlyDocWithErr)
        {
            m_pOnlyDocWithErr->RemoveAll();
            SAFE_DELETE(m_pOnlyDocWithErr);
        }
        SAFE_DELETE(m_pRRDS_SynchronizationInfoByStatus);
    }
        
    SAFE_DELETE(m_pDocumentsToMonitor);
    SAFE_DELETE(m_pTRDS_ActionsLog);
}

//-----------------------------------------------------------------------------
BOOL DSMonitor::CanRunDocument()
{    
    if (!AfxDataSynchronizeEnabled())
    {
        if (!IsInUnattendedMode())
            AfxMessageBox(_TB("The company in use is not enabled to the exchange data connector. Check Synchronization Provider information in Environment\\Data Synchronization\\Providers."));
        
        return FALSE;
    }

	m_Pause = AfxGetDataSynchroManager()->ReadPause();

    CSynchroProvider* pSynchroProvider = NULL;
    BOOL canRunDocuments=FALSE;

    if (m_ProviderName.IsEmpty())
    {
        for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
        {	
            pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
            if (pSynchroProvider->IsValid())
            {
                canRunDocuments = TRUE;
                break;
            }
        }
    }

    if	(canRunDocuments==FALSE) 
    {
        if (!IsInUnattendedMode())
            AfxMessageBox(_TB("Connection data not found! Set Synchronization Provider information in Environment\\Data Synchronization\\Providers."));
        
        return FALSE;
    }

    return TRUE;
}

//-----------------------------------------------------------------------------
void DSMonitor::OnCloseDocument()
{            
	EndTimer();

	CAbstractFormDoc::OnCloseDocument();
}

//------------------------------------------------------------------------------ 
TDS_SynchronizationInfo* DSMonitor::GetSynchroInfoRec()	const {return m_pDBTDetail->GetSynchroInfoRec(); }

//-----------------------------------------------------------------------------
BOOL DSMonitor::InitDocument()
{
    m_SynchDateFrom.Clear();
    m_SynchDateTo.	Clear();

	return CAbstractFormDoc::InitDocument();
	
}

//-------------------------------------------------------------------------------------
void DSMonitor::InitGauge()
{
    if (!m_pGauge)
        return;

    m_nValueGauge.Clear();
    m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
    m_pGauge->RemoveAllColoredRanges();
    m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DSMonitor::CompleteGauge()
{
    if (!m_pGauge)
        return;

	if (m_bNeedMassiveSynchro)
		m_nValueGauge = 0;
	else
		m_nValueGauge = m_nGaugeUpperRange;

	m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);

	m_pGauge->AddColoredRange(
		0,
		m_nValueGauge,
		AfxGetTileDialogStyleNormal()->GetTitleSeparatorColor()
		);
	m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DSMonitor::UpdateGauge(double nStep /*= 1.0*/)
{
	UpdateGauge(nStep, FALSE);
}

//-------------------------------------------------------------------------------------------
void DSMonitor::UpdateGauge(double nStep /*= 1.0*/, BOOL bIsIndeterminate /* = FALSE*/)
{
	if (!m_pGauge)
		return;

	m_nValueGauge += nStep;
	
	if (!bIsIndeterminate && m_nValueGauge >= m_nGaugeUpperRange)
		m_nValueGauge = m_nValueGauge / 2;

	m_pGauge->RemoveAllColoredRanges();
	m_pGauge->AddColoredRange(
		0,
		m_nValueGauge,
		AfxGetTileDialogStyleNormal()->GetTitleSeparatorColor()
		);
	m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
	m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DSMonitor::UpdateGaugeIndeterminate()
{
	if (m_nValueGauge >= m_nGaugeUpperRange)
		m_nValueGauge = 0;

	if(!m_Pause && !m_Abort)
		UpdateGauge(1.0, TRUE);	
}

//--------------------------------------------------------------------------------
void DSMonitor::ManageJsonVars()
{
	DECLARE_VAR_JSON(ProviderName);
	DECLARE_VAR_JSON(DocToSynch);
	DECLARE_VAR_JSON(bShowOnlyDocWithErr);
	DECLARE_VAR_JSON(bAutoRefresh);
	DECLARE_VAR_JSON(bIsActivatedCRMInfinity);
	DECLARE_VAR_JSON(SynchStatusAll);
	DECLARE_VAR_JSON(SynchStatus);
	DECLARE_VAR_JSON(SynchDateAll);
	DECLARE_VAR_JSON(SynchDateSel);
	DECLARE_VAR_JSON(SynchDateFrom);
	DECLARE_VAR_JSON(SynchDateTo);
	DECLARE_VAR_JSON(InboundPic);
	DECLARE_VAR_JSON(OutboundPic);
	DECLARE_VAR_JSON(StatusOkPic);
	DECLARE_VAR_JSON(StatusWaitPic);
	DECLARE_VAR_JSON(StatusErrorPic);
	DECLARE_VAR_JSON(StatusExcludedPic);
	//manage gauge status
	DECLARE_VAR_JSON(nValueGauge);
	DECLARE_VAR_JSON(GaugeDescription);
	DECLARE_VAR_JSON(PictureStatus);
	DECLARE_VAR_JSON(bDelta);
	DECLARE_VAR_JSON(bIsActivatedImagoStudio);
	RegisterControl(IDC_DS_MONITOR_BE,				RUNTIME_CLASS(CMonitorDetailBodyEdit));
	RegisterControl(IDC_DS_MONITOR_RECOVERY_ERR_BE,	RUNTIME_CLASS(CMonitorSummaryDetailBodyEdit));
}

//-----------------------------------------------------------------------------
BOOL DSMonitor::OnAttachData()
{           
	SetFormTitle(_TB("Synchronization Monitor"));
	SetDocAccel(IDR_DATASYNCHRO_MONITOR);

	//manage json variables
	ManageJsonVars();
	
	if (m_pMassiveSynchroInfo) // Ci entro solo se arrivo da batch
	{
	
		m_ProviderName = m_pMassiveSynchroInfo->m_ProviderName;	

        if (m_pMassiveSynchroInfo->m_pDocumentsToSynch && m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetSize() > 0)
        {
            for (int i = 0; i < m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetSize(); i++)
            {
                CSynchroDocInfo* pSynchroDocInfo = new CSynchroDocInfo(m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetAt(i));
                pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
                if (pSynchroDocInfo->IsValid())
                    m_pDocumentsToMonitor->Add(pSynchroDocInfo);		
                else 
                    delete pSynchroDocInfo;

			}	
		
			m_DocToSynch = (m_pDocumentsToMonitor->GetSize() > 0) ? m_pDocumentsToMonitor->GetAt(0)->m_strDocNamespace : _T("");
		}
		m_SynchStatus = E_SYNCHROSTATUS_TYPE_EXCLUDED;
		m_bDelta = m_pMassiveSynchroInfo->m_bDeltaSynch;
	}

	m_StartSynchDateForDelta.SetFullDate(TRUE);
	SetStartSynchDateForDelta();


	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		if (m_bIsActivatedCRMInfinity)
			m_pRRDS_SynchronizationInfoByStatus = new RRDS_SynchronizationInfoByStatus(m_bDelta, m_StartSynchDateForDelta, this);
		m_pDBTDetail = new DBTSynchroInfoMonitor(RUNTIME_CLASS(TEnhDS_SynchronizationInfo), this, m_bDelta, m_StartSynchDateForDelta);

	}
	else
		m_pDBVTDetail = new DBTVSynchroInfoMonitor(RUNTIME_CLASS(VEnh_SynchronizationInfo), this, m_bDelta, m_StartSynchDateForDelta);

	CSynchroProvider* pSynchroProvider = NULL;

    if (m_ProviderName.IsEmpty())
    {
        for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
        {	
            pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
            if (pSynchroProvider->IsValid())
            {
                m_ProviderName = pSynchroProvider->m_Name;
                break;
            }
        }
    }
    
    pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(m_ProviderName);
    
    if (m_pDocumentsToMonitor->GetSize() ==  0 && pSynchroProvider)
    {
        for (int i = 0; i < pSynchroProvider->GetDocumentsToSynch()->GetSize(); i++)
        {
            CSynchroDocInfo* pSynchroDocInfo = new CSynchroDocInfo(pSynchroProvider->GetDocumentsToSynch()->GetAt(i)->m_strDocNamespace);
            pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
            if (pSynchroDocInfo->IsValid())
                m_pDocumentsToMonitor->Add(pSynchroDocInfo);	
            else
                delete pSynchroDocInfo;
        
        }
    }

    if (m_DocToSynch.IsEmpty() && m_pDocumentsToMonitor->GetSize() > 0)
        m_DocToSynch  = m_pDocumentsToMonitor->GetAt(0)->m_strDocNamespace;

	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->Open();
	else
		m_pDBVTDetail->Open();

	if (!m_bFromBatch)
	{
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			m_pDBTDetail->ReloadData();
		else
			m_pDBVTDetail->ReloadData();
	}

	m_bIsMassiveSynchronizing = m_bFromBatch;
	m_bSynchronizationStarted = m_bFromBatch;

	if (m_bIsActivatedCRMInfinity) // La gestonione "Recovery Errors" è gestita solo per CRM Infinity
	{
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			m_pDBTDetailDocSummary = new DBTSynchroMonitorDocSummay(RUNTIME_CLASS(TEnhDS_SynchroInfoDocSummary), this); 
		else
			m_pDBVDetailDocSummary = new DBTVSynchroMonitorDocSummay(RUNTIME_CLASS(VSynchroInfoDocSummary), this);

		m_pOnlyDocWithErr      = new CSynchroDocInfoArray();

		m_bNeedMassiveSynchro	  = !m_bFromBatch && AfxGetDataSynchroManager()->NeedMassiveSynchro(pSynchroProvider->m_Name);

		DoGaugeManagement();

		SynchroDocSummaryManagement();
	}

	StartGaugeTimer();
	InitGauge();
	
	StartTimer();
	
	return TRUE;
}

//----------------------------------------------------------------------------------
BOOL DSMonitor::OnOpenDocument(LPCTSTR pParam)
{
    if (pParam)	
    {
        m_bFromBatch   = TRUE;
        m_bAutoRefresh = TRUE;

		m_pMassiveSynchroInfo = (MassiveSynchroInfo*) GET_AUXINFO(pParam);
	}

	SetFiltersEnable(!m_bFromBatch);
	return CAbstractFormDoc::OnOpenDocument(pParam);
}

//---------------------------------------------------------------------------------------------
void DSMonitor::StartTimer()
{
    if (m_MonitorRefresh > 0.0)
        GetFirstView()->SetTimer(CHECK_DS_MONITOR_TIMER, (UINT)m_MonitorRefresh, NULL);
}

//-----------------------------------------------------------------------------------------------
void DSMonitor::EndTimer()
{
	if (GetFirstView())
		GetFirstView()->KillTimer(CHECK_DS_MONITOR_TIMER);
}

//---------------------------------------------------------------------------------------------
void DSMonitor::StartGaugeTimer()
{
	GetFirstView()->SetTimer(CHECK_DS_GAUGE_TIMER, (UINT)m_GaugeRefresh, NULL);
}

//-----------------------------------------------------------------------------------------------
void DSMonitor::EndGaugeTimer()
{
	GetFirstView()->KillTimer(CHECK_DS_GAUGE_TIMER);
}

//------------------------------------------------------------------------------
void DSMonitor::DoOnTimer()
{
    DoRefresh();

    UpdateDataView();
}

//------------------------------------------------------------------------------
void DSMonitor::DoOnGaugeTimer()
{
	DoGaugeRefresh();

	UpdateDataView();
}

//------------------------------------------------------------------------------------
BOOL DSMonitor::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		if (pTooltip->m_nControlID == IDC_DS_MONITOR_ERR_BE_SYNCHSTATUS_BTM)
		{
			TEnhDS_SynchroInfoDocSummary* pRec = (TEnhDS_SynchroInfoDocSummary*)m_pDBTDetailDocSummary->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Status");
				if (pRec->l_NoDocStatusError > _T("0"))
					pTooltip->m_strText = (DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)).FormatData();
				else
					pTooltip->m_strText = (DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)).FormatData();
			}

			return TRUE;
		}

		if (pTooltip->m_nControlID == IDC_DS_MONITOR_BE_SYNCHDIRECTION_BTM)
		{
			TEnhDS_SynchronizationInfo* pRec = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Direction");
				pTooltip->m_strText = pRec->f_SynchDirection.FormatData();
			}

			return TRUE;
		}

		if (pTooltip->m_nControlID == IDC_DS_MONITOR_BE_SYNCHSTATUS_BTM)
		{
			TEnhDS_SynchronizationInfo* pRec = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Status");
				pTooltip->m_strText = pRec->f_SynchStatus.FormatData();
			}

			return TRUE;
		}
	}
	else
	{
		if (pTooltip->m_nControlID == IDC_DS_MONITOR_ERR_BE_SYNCHSTATUS_BTM)
		{
			VSynchroInfoDocSummary* pRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Status");
				if (pRec->l_NoDocStatusError > _T("0"))
					pTooltip->m_strText = (DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)).FormatData();
				else
					pTooltip->m_strText = (DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)).FormatData();
			}

			return TRUE;
		}

		if (pTooltip->m_nControlID == IDC_DS_MONITOR_BE_SYNCHDIRECTION_BTM)
		{
			VSynchroInfoDocSummary* pRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Direction");
				pTooltip->m_strText = pRec->l_SynchDirection.FormatData();
			}

			return TRUE;
		}

		if (pTooltip->m_nControlID == IDC_DS_MONITOR_BE_SYNCHSTATUS_BTM)
		{
			VSynchroInfoDocSummary* pRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetRow(pTooltip->m_nRowDbt);
			if (pRec)
			{
				pTooltip->m_strTitle = _TB("Status");
				pTooltip->m_strText = pRec->l_SynchStatus.FormatData();
			}

			return TRUE;
		}


	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void DSMonitor::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
    UINT nIDC = (UINT)pCtrl->GetCtrlID();

    if (nIDC == IDC_DATASYNCHRO_MONITOR_STATUS_DESCRIPTION)
    {
        m_pGaugeLabel = dynamic_cast<CLabelStatic*>(pCtrl);

        if (!m_pGaugeLabel)
            return;

        m_pGaugeLabel->SetOwnFont(FALSE, FALSE, FALSE, 10);
        m_pGaugeLabel->SetTextColor(AfxGetThemeManager()->GetTileDialogTitleForeColor());
        m_pGaugeLabel->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
    }

    if (nIDC == IDC_DATASYNCHRO_MONITOR_STATUS_GAUGE)
    {
        m_pGauge = dynamic_cast<CTBLinearGaugeCtrl*>(pCtrl);

        if (!m_pGauge)
            return;

        m_pGauge->SetMajorTickMarkSize(0);
        m_pGauge->SetMinorTickMarkSize(0);
        m_pGauge->SetMajorTickMarkStep(0);
        m_pGauge->RemovePointer(0);
        m_pGauge->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
        m_pGauge->SetTextLabelFormat(_T(""));
        m_pGauge->RemoveAllColoredRanges();
        m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
    }
}

//-----------------------------------------------------------------------------
void DSMonitor::GetDecodingInfo(const DataGuid& tbGuid, DataStr& recordKey, DataStr& recordDescription)
{
			recordKey			= _T("");
			recordDescription	= _T("");
	BOOL	bShowOnlyDocWithErr = m_bIsActivatedCRMInfinity && m_bShowOnlyDocWithErr;

    // Posso visualizzare solo i documenti che hanno errori o tutti
    CSynchroDocInfo* pSynchroDocInfo = bShowOnlyDocWithErr ? m_pOnlyDocWithErr->GetDocumentByNs(m_DocToSynch) : m_pDocumentsToMonitor->GetDocumentByNs(m_DocToSynch);

    if (pSynchroDocInfo)
        pSynchroDocInfo->GetDecodingInfo(tbGuid, recordKey, recordDescription);
}

//-----------------------------------------------------------------------------
void DSMonitor::DeleteContents()
{
	SAFE_DELETE(m_pDBTDetail);
	SAFE_DELETE(m_pDBVTDetail);
	if (m_bIsActivatedCRMInfinity)
	{
		SAFE_DELETE(m_pDBTDetailDocSummary);
		SAFE_DELETE(m_pDBVDetailDocSummary);
	}
	CAbstractFormDoc::DeleteContents();
}

//-----------------------------------------------------------------------------
void DSMonitor::DisableControlsForBatch()
{
	//m_ProviderName.	SetReadOnly(m_bFromBatch);
	m_SynchStatus.	SetReadOnly(m_SynchStatusAll);	
	m_SynchDateFrom.SetReadOnly(m_SynchDateAll);
	m_SynchDateTo.	SetReadOnly(m_SynchDateAll);
	m_bDelta.		SetReadOnly(TRUE);

}

//----------------------------------------------------------------------------
void DSMonitor::CheckSynchroDate()
{
	if (m_SynchDateSel && (m_SynchDateFrom.IsEmpty() || m_SynchDateTo.IsEmpty() || m_SynchDateTo < m_SynchDateFrom))
	{
		if (!IsInUnattendedMode())
		{
			m_pMessages->Add(_TB("Wrong data selection."));
			m_pMessages->Show(TRUE);
		}
	}
}

//----------------------------------------------------------------------------
void DSMonitor::DoGaugeManagement()
{
	int		nResult;
	TCHAR	buffer[512];

	if(m_pMassiveSynchroInfo != NULL)
		if(m_pMassiveSynchroInfo->m_bDeltaSynch == 1)
		m_bDelta = m_pMassiveSynchroInfo->m_bDeltaSynch;

	if(m_bSynchronizationEnded || m_nSynchronizationCounter == 0)
	{
		if (m_bNeedMassiveSynchro)
		{
			m_GaugeDescription = _TB("Massive synchronization is needed");
			nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconWarning);
		}
		else
		{
			if (!m_bDelta)
				m_GaugeDescription = _TB("Massive synchronization ended");
			else
				m_GaugeDescription = _TB("Incremental synchronization ended");

			m_bErrorsOccurred = ErrorsOccurred();	

			if (m_bErrorsOccurred)
			{
				m_GaugeDescription += _T(" ") + _TB("with errors.");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconError);
			}
			else
			{
				m_GaugeDescription += _T(" ") + _TB("successfully.");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphOk);
			}
		}
	}
	else
	{
		if (m_bSynchronizationStarted)
		{
			if (!m_bDelta)
			{
				m_GaugeDescription = _TB("Massive synchronization in progress.");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphWait);
			}
		}
		else
		{
			if (m_bNeedMassiveSynchro)
			{
				m_GaugeDescription = _TB("Massive synchronization is needed");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconWarning);
			}
			else 
			{
				m_GaugeDescription = _TB("Retrieving information for current synchronization...");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphWait);
			}
		}
	}

	m_PictureStatus = buffer;
}

//----------------------------------------------------------------------------
void DSMonitor::SynchroDocSummaryManagement()
{
	SetMonitorParameters(m_bErrorsOccurred);
	LoadDocSummaryDBT();
}

//----------------------------------------------------------------------------
void  DSMonitor::SetMonitorParameters(BOOL bErrorsOccurred /*FALSE*/)
{
    if (bErrorsOccurred)
    {
        m_bShowOnlyDocWithErr = TRUE;

		if (!m_bFromBatch)
		{
			if (m_bSynchronizationEnded)
			{
				m_SynchStatusAll = FALSE;
				m_SynchStatus	 = E_SYNCHROSTATUS_TYPE_ERROR;
			}
			else
				m_SynchStatusAll = TRUE;
			
		}

		if (m_pOnlyDocWithErr->GetSize() > 0)
		{
			m_DocToSynch.Clear();
			m_DocToSynch = m_pOnlyDocWithErr->GetAt(0)->m_strDocNamespace;
		}

		DoSynchStatusAllChanged();
	}
	else
	{
		m_bShowOnlyDocWithErr = FALSE;

        if (!m_bFromBatch)
        {
            m_SynchStatusAll = TRUE;
            m_SynchStatus	 = E_SYNCHROSTATUS_TYPE_SYNCHRO;
        }
    
        if (m_DocToSynch.IsEmpty() && m_pDocumentsToMonitor->GetSize() > 0)
            m_DocToSynch = m_pDocumentsToMonitor->GetAt(0)->m_strDocNamespace;

        DoSynchStatusAllChanged();
    }
}

//-----------------------------------------------------------------------------
CBaseTileDialog* DSMonitor::GetTile(UINT nIDD)
{
    CMasterFormView* pView = (CMasterFormView*)GetFirstView();
    CBaseTileDialog* pTile = NULL;

    if (!pView)
        return NULL;

    for (int i = 0; i < pView->m_pTileGroups->GetSize(); i++)
    {
        CTileGroup* pGroup = pView->m_pTileGroups->GetAt(i);
        pTile = pGroup->GetTileDialog(nIDD);
        if (pTile)
            return pTile;
    }

    return NULL;
}

//----------------------------------------------------------------------------
void DSMonitor::SetCollapsedResultsPanel()
{
	if (!m_pResultsTilePanel || !m_pSummaryTile || !m_pMonitorTile)
		return;

	m_pResultsTilePanel->SetCollapsed	(FALSE);
	m_pSummaryTile->SetCollapsed		(FALSE);
	m_pMonitorTile->SetCollapsed		(TRUE);
}

//----------------------------------------------------------------------------
BOOL DSMonitor::ErrorsOccurred()
{
    BOOL bErrorOccurred = FALSE;

	CString			 sDocNamespace;
	CSynchroDocInfo* pSynchroDocInfo;
	
	m_pOnlyDocWithErr->RemoveAll();
	
	for (int i = 0; i < m_pDocumentsToMonitor->GetSize(); i++)
	{
		sDocNamespace = m_pDocumentsToMonitor->GetAt(i)->m_strDocNamespace;
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		{
			if (m_pRRDS_SynchronizationInfoByStatus)
			{
				m_pRRDS_SynchronizationInfoByStatus->FindRecord(m_ProviderName, sDocNamespace, DataEnum(E_SYNCHROSTATUS_TYPE_ERROR));
				if (!m_pRRDS_SynchronizationInfoByStatus->IsEOF())
				{
					bErrorOccurred = TRUE;

					pSynchroDocInfo = new CSynchroDocInfo(sDocNamespace);
					pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
					if (pSynchroDocInfo->IsValid())
						m_pOnlyDocWithErr->Add(pSynchroDocInfo);
					else
						delete pSynchroDocInfo;
				}
			}
		}
		else
		{
			CString strXml = _T("");
			if (m_bDelta)
				strXml = AfxGetDataSynchroManager()->GetLogsByNamespaceDelta(m_ProviderName, sDocNamespace, TRUE, TRUE);
			else
				strXml = AfxGetDataSynchroManager()->GetLogsByNamespace(m_ProviderName, sDocNamespace,TRUE);

			if (strXml != _T(""))
			{
				bErrorOccurred = TRUE;

				pSynchroDocInfo = new CSynchroDocInfo(sDocNamespace);
				pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
				if (pSynchroDocInfo->IsValid())
					m_pOnlyDocWithErr->Add(pSynchroDocInfo);
				else
					delete pSynchroDocInfo;
			}
		}
	}

	return bErrorOccurred;
}

//----------------------------------------------------------------------------
DataDate DSMonitor::GetStartSynchDateForDelta() 
{
	TDS_SynchronizationInfo aRec;
	SqlTable aTbl(&aRec, GetReadOnlySqlSession());
	DataDate res;
	TRY
	{
		aTbl.Open();

		aTbl.SelectSqlFun(_T("MAX(%s)"), aRec.f_StartSynchDate, res);

		aTbl.AddParam(szParamProvider,	aRec.f_ProviderName);
		aTbl.AddFilterColumn(aRec.f_ProviderName);
		aTbl.SetParamValue(szParamProvider, m_ProviderName);

		aTbl.Query();

		aTbl.Close();
	}
	CATCH(SqlException, e)
	{
		ASSERT(FALSE);
		e->ShowError();
		TRACE(cwsprintf(_T("DSMonitor::GetStartSynchDateForDelta SqlException.\n%s"), e->m_strError));
	}
	END_CATCH

	return res;
}

void DSMonitor::SetStartSynchDateForDelta() 
{
	if (!m_pMassiveSynchroInfo)
	{
		if (m_bDelta && m_StartSynchDateForDelta.IsEmpty())
			m_StartSynchDateForDelta = GetStartSynchDateForDelta();
	}
	else
	{
		m_bDelta = m_pMassiveSynchroInfo->m_bDeltaSynch;
		m_StartSynchDateForDelta = m_pMassiveSynchroInfo->m_StartSynchDate;
	}
}

//----------------------------------------------------------------------------
void DSMonitor::LoadDocSummaryDBT()
{
	int indexCurrentDoc = -1;
	DataLng nCount = 0;

	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		SqlTable* pTable(NULL);
		TDS_SynchronizationInfo aRec;

		pTable = new SqlTable(&aRec, GetReadOnlySqlSession());
		pTable->Open();
		pTable->Select(aRec.f_DocNamespace);
		pTable->Select(aRec.f_SynchStatus);
		pTable->SelectSqlFun(_T("COUNT(*)"), &nCount);

		/*
			select DocNamespace, SynchStatus, COUNT(*)
			from DS_SynchronizationInfo
			where SynchStatus = 31457284 se m_bShowOnlyDocWithErr = FALSE aggiungo anche: or SynchStatus = 31457282
			group by DocNamespace, SynchStatus
			order by DocNamespace, SynchStatus
		*/

		if (m_bFirstTimeRunning)
		{
			SetStartSynchDateForDelta();
			m_bFirstTimeRunning = FALSE;
		}

		if (m_bShowOnlyDocWithErr)
		{
			if (m_bDelta)
			{
				pTable->m_strFilter = cwsprintf(_T("%s = %s AND %s = %s AND %s = %s GROUP BY %s, %s"),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_ProviderName),
					m_pSqlConnection->NativeConvert(&m_ProviderName),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_StartSynchDate),
					m_pSqlConnection->NativeConvert(&m_StartSynchDateForDelta),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_DocNamespace),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus));
			}
			else
			{
				pTable->m_strFilter = cwsprintf(_T("%s = %s AND %s = %s GROUP BY %s, %s"),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_ProviderName),
					m_pSqlConnection->NativeConvert(&m_ProviderName),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_DocNamespace),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus));
			}
		}

		else
		{
			if (m_bDelta)
			{
				pTable->m_strFilter = cwsprintf(_T("%s = %s AND (%s = %s OR %s = %s) AND %s = %s GROUP BY %s, %s"),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_ProviderName),
					m_pSqlConnection->NativeConvert(&m_ProviderName),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_StartSynchDate),
					m_pSqlConnection->NativeConvert(&m_StartSynchDateForDelta),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_DocNamespace),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus));
			}
			else
			{
				pTable->m_strFilter = cwsprintf(_T("%s = %s AND (%s = %s OR %s = %s) GROUP BY %s, %s"),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_ProviderName),
					m_pSqlConnection->NativeConvert(&m_ProviderName),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus),
					m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_ERROR)),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_DocNamespace),
					(LPCTSTR)aRec.GetColumnName(&aRec.f_SynchStatus));
			}
		}


		pTable->AddSortColumn(aRec.f_DocNamespace);
		pTable->AddSortColumn(aRec.f_SynchStatus);

		TRY
		{
			m_pDBTDetailDocSummary->RemoveAll();

			pTable->Query();

			TEnhDS_SynchroInfoDocSummary* pSynchroRec;

			DataStr sNamespace = _T("");
			DataStr sOldNamespace = _T("");
			while (!pTable->IsEOF())
			{
				sNamespace = aRec.f_DocNamespace;

				if (sNamespace != sOldNamespace)
				{
					sOldNamespace = sNamespace;
					pSynchroRec = (TEnhDS_SynchroInfoDocSummary*)m_pDBTDetailDocSummary->AddRecord();
					pSynchroRec->f_DocNamespace = aRec.f_DocNamespace;
					pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO));
					pSynchroRec->l_NoDocStatusError = _T("0");
					pSynchroRec->l_NoDocStatusSynchro = _T("0");

					if (pSynchroRec->f_DocNamespace == m_DocToSynch)
						indexCurrentDoc = m_pDBTDetailDocSummary->GetCurrentRowIdx();
				}

				if (aRec.f_SynchStatus == DataEnum(E_SYNCHROSTATUS_TYPE_ERROR))
				{
					pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_ERROR));
					pSynchroRec->l_NoDocStatusError = nCount.Str();
				}
				else
					pSynchroRec->l_NoDocStatusSynchro = nCount.Str();

				pTable->MoveNext();
			}

			if (pTable && pTable->IsOpen())
			{
				pTable->Close();
				SAFE_DELETE(pTable);
			}
		}
			CATCH(SqlException, e)
		{
			TRACE(cwsprintf(_T("DSMonitor::LoadDocSummaryDBT SqlException.\n%s"), e->m_strError));
			if (pTable->IsOpen())
				pTable->Close();

			SAFE_DELETE(pTable);
			return;
		}
		END_CATCH
	

		//InitGauge();

		if (indexCurrentDoc == -1)
			indexCurrentDoc = 0;

		if (m_pDBTDetailDocSummary->GetSize() > 0)
		{
			m_DocToSynch = ((TEnhDS_SynchroInfoDocSummary*)m_pDBTDetailDocSummary->GetRow(indexCurrentDoc))->f_DocNamespace;
			m_pDBTDetailDocSummary->SetCurrentRow(indexCurrentDoc);
		}
		else
			m_DocToSynch = _T("");
	}
	else
	{
		if (m_bFirstTimeRunning)
		{
			SetStartSynchDateForDelta();
			m_bFirstTimeRunning = FALSE;
		}

		//m_bDelta = FALSE;
		//m_bShowOnlyDocWithErr = FALSE;

		CString XmlLogs = AfxGetDataSynchroManager()->GetMassiveSynchroLogs(m_ProviderName, m_bDelta.ToString(), m_bShowOnlyDocWithErr.ToString());

		VSynchroInfoDocSummary* pSynchroRec = NULL;
		DataStr sNamespace = _T("");
		DataStr sOldNamespace = _T("");
		CXMLDocumentObject aXMLModDoc;

		m_pDBVDetailDocSummary->RemoveAll();

		if (XmlLogs == _T("")) return;
		
		aXMLModDoc.EnableMsgMode(FALSE);

		if (!aXMLModDoc.LoadXML(XmlLogs))
		{
			pSynchroRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->AddRecord();
			pSynchroRec->l_DocNamespace = _T("");

			pSynchroRec->l_NsWithFlows = _TB("Unable to parse xml logs.");
			pSynchroRec->l_Flow = _T("");

			pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_ERROR));
			pSynchroRec->l_NoDocStatusError = _T("0");
			pSynchroRec->l_NoDocStatusSynchro = _T("0");
			return;
		}
		
		TRY
		{
			// root SynchroProfiles
			CXMLNode* pRoot = aXMLModDoc.GetRoot();
			// Provider
			CString strValue;
			BOOL				bOK = FALSE;

			CXMLNodeChildsList* pLogsNode = pRoot->GetChilds();
			CXMLNode* pNode = pRoot->GetChildByName(_T("Logs"));

			if (pNode)
			{
				for (int i = 0; i < pNode->GetChilds()->GetSize(); i++)
				{
					CXMLNode* pLog = pNode->GetChilds()->GetAt(i);

					if (pLog)
					{
						CString sAttrValue;
						CString strStatus;
						CString strCount;
						DataLng nCount;
						CString strFlowName;
						DataEnum SyncStatus;
						CString NsWithFlowName;

						bOK = pLog->GetAttribute(_T("Namespace"), sAttrValue);
						sNamespace = sAttrValue;
						bOK = pLog->GetAttribute(_T("Status"), strStatus);
						if (bOK)
							SyncStatus = strStatus == _T("True") ? DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO) : DataEnum(E_SYNCHROSTATUS_TYPE_ERROR);

						bOK = pLog->GetAttribute(_T("Count"), strCount);
						if (bOK)
							nCount.AssignFromXMLString(strCount);


						bOK = pLog->GetAttribute(_T("FlowName"), strFlowName);

						NsWithFlowName = sAttrValue + _T(" - ") + strFlowName;

						if (bOK && NsWithFlowName != sOldNamespace)
						{
							sOldNamespace = NsWithFlowName;
							pSynchroRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->AddRecord();
							pSynchroRec->l_DocNamespace = sAttrValue;

							pSynchroRec->l_NsWithFlows = NsWithFlowName;
							pSynchroRec->l_Flow = strFlowName;

							pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO));
							pSynchroRec->l_NoDocStatusError = _T("0");
							pSynchroRec->l_NoDocStatusSynchro = _T("0");

							if (pSynchroRec->l_DocNamespace == m_DocToSynch)
								indexCurrentDoc = m_pDBVDetailDocSummary->GetCurrentRowIdx();
							//m_bNamespaceChanged = TRUE;
							m_nDetailPage = 0;
							m_nDetailPageStr = _TB("Pag.") + (m_nPageTot == 0 ? _T("0") : (m_nDetailPage + 1).ToString()) + _T("/") + m_nPageTot.ToString();
						}
						if (pSynchroRec)
						{
							if (SyncStatus == DataEnum(E_SYNCHROSTATUS_TYPE_ERROR))
							{
								pSynchroRec->l_SynchStatusBmp = OnGetSyncStatusBmp(DataEnum(E_SYNCHROSTATUS_TYPE_ERROR));
								pSynchroRec->l_NoDocStatusError = nCount.Str();
							}
							else
								pSynchroRec->l_NoDocStatusSynchro = nCount.Str();
						}
					}
				}
			}
		}		
		CATCH(CException, e)
		{
			TCHAR szError[1024];
			AfxMessageBox(e->GetErrorMessage(szError, 1024));
			TRACE(cwsprintf(_T("DSMonitor::LoadDocSummaryDBT Exception.\n%s"), szError));
			return;
		}
		END_CATCH

		if (indexCurrentDoc == -1)
				indexCurrentDoc = 0;

		if (m_pDBVDetailDocSummary->GetSize() > 0)
		{
			m_DocToSynch = ((VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetRow(indexCurrentDoc))->l_DocNamespace;
			m_pDBVDetailDocSummary->SetCurrentRow(indexCurrentDoc);
		}
		else
			m_DocToSynch = _T("");
	}
}
	
//----------------------------------------------------------------------------
void DSMonitor::DoSynchStatusAllChanged()
{
	m_SynchStatus.SetReadOnly(m_SynchStatusAll);
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnShowOnlyDocWithErrChanged()
{
	LoadDocSummaryDBT();

	m_bErrorsOccurred = ErrorsOccurred();
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();
	UpdateDataView();
}


//-----------------------------------------------------------------------------
void DSMonitor::OnDocNamespaceChanged()
{
	//m_bNamespaceChanged = TRUE;
	m_nDetailPage = 0;
	//m_LineRequested = 0;
	m_pMonitorTile->SetCollapsed(FALSE);
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		m_pDBTDetail->ReloadData();

		for (int i = 0; i < m_pDBTDetailDocSummary->GetSize(); i++)
		{
			TEnhDS_SynchroInfoDocSummary* pRec = (TEnhDS_SynchroInfoDocSummary*)m_pDBTDetailDocSummary->GetRow(i);
			if (pRec->f_DocNamespace == m_DocToSynch)
			{
				m_pDBTDetailDocSummary->SetCurrentRow(i);
				break;
			}
		}
	}
	else
	{

		for (int i = 0; i < m_pDBVDetailDocSummary->GetSize(); i++)
		{
			VSynchroInfoDocSummary* pRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetRow(i);
			if (pRec->l_DocNamespace == m_DocToSynch)
			{
				m_pDBVDetailDocSummary->SetCurrentRow(i);
				break;
			}
		}

		m_pDBVTDetail->ReloadData();
	}

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnProviderChanged()
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();
	m_pSummaryTile->SetCollapsed(FALSE);	
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnSynchStatusAllChanged()
{
	DoSynchStatusAllChanged();
	m_pMonitorTile->SetCollapsed(FALSE);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnSynchStatusChanged()
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();
	m_pMonitorTile->SetCollapsed(FALSE);	
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnSynchDateSelChanged()
{
    if (m_SynchDateAll)
    {
        m_SynchDateFrom.Clear();
        m_SynchDateTo.Clear();
    }

    m_SynchDateFrom	.SetReadOnly(!m_SynchDateSel);
    m_SynchDateTo	.SetReadOnly(!m_SynchDateSel);

	if (!m_SynchDateSel)
	{
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			m_pDBTDetail->ReloadData();
		else
			m_pDBVTDetail->ReloadData();
	}

	m_pMonitorTile->SetCollapsed(FALSE);

    m_pMonitorTile->SetCollapsed(FALSE);

    UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnSynchDateFromEntered()
{
    if 
        (
            !m_SynchDateFrom.IsEmpty() &&
            (m_SynchDateTo.IsEmpty() || m_SynchDateTo < m_SynchDateFrom)
        )
        m_SynchDateTo = m_SynchDateFrom;

	CheckSynchroDate();
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();

	m_pMonitorTile->SetCollapsed(FALSE);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSMonitor::OnSynchDateToEntered()
{
	CheckSynchroDate();
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		m_pDBTDetail->ReloadData();
	else
		m_pDBVTDetail->ReloadData();

	m_pMonitorTile->SetCollapsed(FALSE);
	UpdateDataView();
}

//----------------------------------------------------------------------------
void DSMonitor::StartRecoveryErr()
{
    BeginWaitCursor();

    SetFiltersEnable(FALSE);

	m_GaugeDescription = _TB("Synchronization started...");
	m_bIsMassiveSynchronizing = TRUE;
	m_bSynchronizationStarted = TRUE;
	
	if (!(AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled()))
		AfxGetDataSynchroManager()->SynchronizeErrorsRecovery(m_ProviderName);
	else
	{
		AfxGetDataSynchroManager()->SynchronizeErrorsRecoveryImago(m_ProviderName, m_strRecoveryGuid);
		m_bSynchronizationEnded = FALSE;
		m_nSynchronizationCounter = 3;
		InitGauge();
		m_Pause = FALSE;
	}

	SetMonitorParameters(TRUE);
	EndWaitCursor();
}

//--------------------------------------------------------------------------------
void DSMonitor::OnErrorsRecoveryClick()
{
    GetNotValidView(TRUE);

	StartGaugeTimer();
	StartRecoveryErr();
	
	UpdateDataView();
}

//---------------------------------------------------------------------------------
void DSMonitor::OnMonitorRefresh()
{
    GetNotValidView(TRUE);

    BeginWaitCursor();
    DoRefresh();
    EndWaitCursor();
    //TODO - corina
    //ShowHideControl();
    UpdateDataView();
}

//-----------------------------------------------------------------------------------
void DSMonitor::OnErrorsRecoveryUpdate(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bSynchronizationEnded && m_bErrorsOccurred);
}

//-----------------------------------------------------------------------------------
void DSMonitor::PauseMassiveSynchroEnable(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_Pause && !m_bSynchronizationEnded);
}

//-----------------------------------------------------------------------------------
void DSMonitor::ContinueMassiveSynchroEnable(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_Pause && !m_bSynchronizationEnded);
}


//-----------------------------------------------------------------------------------
void DSMonitor::AbortMassiveSynchroEnable(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bSynchronizationEnded);
}

//---------------------------------------------------------------------------------------
void DSMonitor::PauseMassiveSynchro()
{
	if (m_Pause)
	{
		m_Pause = FALSE;
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{
			AfxGetDataSynchroManager()->ChangedPause(m_Pause, AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i)->m_Name);
		}
	}
	else
	{
		m_Pause = TRUE;
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{
			AfxGetDataSynchroManager()->ChangedPause(m_Pause, AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i)->m_Name);
		}
	}
		
}

//---------------------------------------------------------------------------------------
void DSMonitor::ContinueMassiveSynchro()
{
	if (m_Pause)
	{
		m_Pause = FALSE;
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{
			AfxGetDataSynchroManager()->ChangedPause(m_Pause, AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i)->m_Name);
		}
	}
	else
	{
		m_Pause = TRUE;
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{
			AfxGetDataSynchroManager()->ChangedPause(m_Pause, AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i)->m_Name);
		}
	}
}

//---------------------------------------------------------------------------------------
void DSMonitor::AbortMassiveSynchro()
{
	m_Abort = True;

	for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
	{
		AfxGetDataSynchroManager()->Abort(AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i)->m_Name);
	}
	
}

//---------------------------------------------------------------------------------------
BOOL DSMonitor::InPause()
{
	return m_Pause;
}


//---------------------------------------------------------------------------------------
void DSMonitor::OnMonitorRefreshUpdate(CCmdUI* pCmdUI)
{
    BOOL bEnable = !m_bAutoRefresh && (m_SynchDateAll || m_SynchDateSel && !m_SynchDateFrom.IsEmpty() && !m_SynchDateTo.IsEmpty() && m_SynchDateFrom <= m_SynchDateTo);
     
    pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void DSMonitor::SetFiltersEnable(BOOL bSet)
{
    CBaseTileDialog* pTile = NULL;
    
    pTile = GetTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_STATUS);
    if (pTile)
        pTile->EnableWindow(bSet);

    pTile = GetTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_DATE);
    if (pTile)
        pTile->EnableWindow(bSet);
}

//----------------------------------------------------------------------------
void DSMonitor::DoRefresh()
{
	if (!m_bIsActivatedCRMInfinity)
		return;

	/*if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled() && !m_strRecoveryGuid.IsEmpty() && !AfxGetDataSynchroManager()->IsActionRunning(m_strRecoveryGuid))
		return;
	else
	{
		if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			m_strRecoveryGuid.Empty();
	}
	*/
	BeginWaitCursor();

	if (m_bSynchronizationEnded)
	{
		// ho finito di sincronizzare
		if (m_bAutoRefresh)
		{
			LoadDocSummaryDBT();
			if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
				m_pDBTDetail->ReloadData();
			else
				m_pDBVTDetail->ReloadData();
			
			
		}
	}
	else
	{
		// NON ho finito di sincronizzare
		if (m_bAutoRefresh)
			SynchroDocSummaryManagement();

		BOOL bCurrentIsMassiveSynchronizing = AfxGetDataSynchroManager()->IsMassiveSynchronizing();
		m_bSynchronizationEnded = (m_bIsMassiveSynchronizing && !bCurrentIsMassiveSynchronizing) || m_nSynchronizationCounter == 0;

		if (m_bFromBatch)
		{
			if(m_bSynchronizationEnded && m_nSynchronizationCounter > 0)
				m_nSynchronizationCounter--;
		}
		else
		{
			if(!m_bSynchronizationStarted && m_nSynchronizationCounter > 0)
				m_nSynchronizationCounter--;
		}

		if (bCurrentIsMassiveSynchronizing)
			m_bSynchronizationStarted = TRUE;

		m_bIsMassiveSynchronizing = bCurrentIsMassiveSynchronizing;
	}
	
	EndWaitCursor();
}

//---------------------------------------------------------------------------------------
void DSMonitor::DoGaugeRefresh()
{
	
	if (m_bSynchronizationEnded)
	{
		EndGaugeTimer();
		CompleteGauge();
	}
	else
	{
		if (!m_bSynchronizationStarted || m_nSynchronizationCounter == 0)
			CompleteGauge();
		else
			UpdateGaugeIndeterminate();
	}
	
	DoGaugeManagement();
}

//---------------------------------------------------------------------------------------
void DSMonitor::OnMonitorSummaryRowChanged()
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		TEnhDS_SynchroInfoDocSummary* pRec = (TEnhDS_SynchroInfoDocSummary*)m_pDBTDetailDocSummary->GetCurrentRow();
		m_DocToSynch = pRec->f_DocNamespace;
		m_pDBTDetail->ReloadData();
	}
	else
	{
		VSynchroInfoDocSummary* pRec = (VSynchroInfoDocSummary*)m_pDBVDetailDocSummary->GetCurrentRow();
		m_DocToSynch = pRec->l_DocNamespace;
		//m_bNamespaceChanged = TRUE;
		m_nDetailPage = 0;
		m_pDBVTDetail->ReloadData();
	}

	UpdateDataView();
}

//---------------------------------------------------------------------------------
void DSMonitor::OnMonitorDetailOpenDocument()
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		TEnhDS_SynchronizationInfo* pRecDetail = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetCurrentRow();
		if (!pRecDetail)
			return;

		CString sDocNamespace = pRecDetail->f_DocNamespace;
		CAbstractFormDoc* pDoc = NULL;
		if (!sDocNamespace.IsEmpty())
		{
			sDocNamespace.Replace(_T("Document."), _T(""));
			pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(sDocNamespace);
		}
		if (!pDoc)
			return;

		CBrowserByTBGuid* pBrowserByTBGuid = new CBrowserByTBGuid(pDoc, GetReadOnlySqlSession());
		if (!pBrowserByTBGuid->BrowseOnRecord(pRecDetail->f_DocTBGuid))
			AfxMessageBox(cwsprintf(_T("The document with TBGuid {%s} doesn't exist."), pRecDetail->f_DocTBGuid.FormatData()));

		SAFE_DELETE(pBrowserByTBGuid);
	}
	else
	{
		VEnh_SynchronizationInfo* pRecDetail = (VEnh_SynchronizationInfo*)m_pDBVTDetail->GetCurrentRow();
		if (!pRecDetail)
			return;

		CString sDocNamespace = pRecDetail->l_DocNamespace;
		CAbstractFormDoc* pDoc = NULL;
		if (!sDocNamespace.IsEmpty())
		{
			sDocNamespace.Replace(_T("Document."), _T(""));
			pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(sDocNamespace);
		}
		if (!pDoc)
			return;

		CBrowserByTBGuid* pBrowserByTBGuid = new CBrowserByTBGuid(pDoc, GetReadOnlySqlSession());
		if (!pBrowserByTBGuid->BrowseOnRecord(pRecDetail->l_DocTBGuid))
			AfxMessageBox(cwsprintf(_T("The document with TBGuid {%s} doesn't exist."), pRecDetail->l_DocTBGuid.FormatData()));

		SAFE_DELETE(pBrowserByTBGuid);
	}
}

//-----------------------------------------------------------------------------
void DSMonitor::OnUpdateDetailOpenDocument(CCmdUI* pCmdUI)
{
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		BOOL bEnabled = FALSE;
		
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		{
			TEnhDS_SynchronizationInfo* pRecDetail = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetCurrentRow();

			if (pRecDetail)
				bEnabled = TRUE;
		}
		else
		{
			VEnh_SynchronizationInfo* pRecDetail = (VEnh_SynchronizationInfo*)m_pDBVTDetail->GetCurrentRow();

			if (pRecDetail)
				bEnabled = TRUE;
		}
		pCmdUI->Enable(bEnabled);
	}
}

//---------------------------------------------------------------------------------
void DSMonitor::OnMonitorDetailCopyMessage()
{
	
		if (OpenClipboard(NULL))
		{
			EmptyClipboard();
			CString aStr;
			if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			{
				TEnhDS_SynchronizationInfo* pRecDetail = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetCurrentRow();
				if (!pRecDetail)
					return;
				aStr = pRecDetail->l_SynchMessage;
			}
			else
			{
				VEnh_SynchronizationInfo* pRecDetail = (VEnh_SynchronizationInfo*)m_pDBVTDetail->GetCurrentRow();
				if (!pRecDetail)
					return;
				aStr = pRecDetail->l_SynchMessage;
			}

			HLOCAL hMem = GlobalAlloc(LHND, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
			char* cptr = (char*)GlobalLock(hMem);
			memcpy(cptr, aStr, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
			GlobalUnlock(hMem);
			if (SetClipboardData(CF_UNICODETEXT, hMem) == NULL)
			{
				AfxMessageBox(_TB("Impossible copy to Clipboard"));
				CloseClipboard();
				return;
			}

			CloseClipboard();
		}
}

//-----------------------------------------------------------------------------
void DSMonitor::OnUpdateDetailCopyMessage(CCmdUI* pCmdUI)
{
	BOOL bEnabled = FALSE;
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		TEnhDS_SynchronizationInfo* pRecDetail = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetCurrentRow();

		if (pRecDetail)
			bEnabled = !pRecDetail->l_SynchMessage.IsEmpty();
	}
	else
	{
		VEnh_SynchronizationInfo* pRecDetail = (VEnh_SynchronizationInfo*)m_pDBVTDetail->GetCurrentRow();

		if (pRecDetail)
			bEnabled = !pRecDetail->l_SynchMessage.IsEmpty();
	}
	pCmdUI->Enable(bEnabled);
}

//-----------------------------------------------------------------------------
void DSMonitor::CustomizeBodyEdit(CBodyEdit* pBE)
{
	if (pBE->GetNamespace().GetObjectName() == _T("SynchronizationInfoMonitor"))
	{
		CBEButton* pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("OpenDocument"),
			ID_MONITOR_OPEN_DOCUMENT,
			TBIcon(szIconOpen, MINI),
			_TB("Open Document"),
			_TB("Open Document")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;

		pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("CopyMessage"),
			ID_MONITOR_COPY_MSG,
			TBIcon(szIconCopy, MINI),
			_TB("Copy Message"),
			_TB("Copy Message")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;

		pBE->EnableAddRow	(FALSE);
		pBE->EnableDeleteRow(FALSE);
	}
	else if(pBE->GetNamespace().GetObjectName() == _T("Detail"))
	{
		pBE->EnableAddRow(FALSE);
		pBE->EnableDeleteRow(FALSE);
	}
}

//-----------------------------------------------------------------------------
CString	DSMonitor::OnGetCaption(CAbstractFormView* pView)
{
	if (pView->GetNamespace().GetObjectName() == _T("DSMonitorDetailRowView"))
	{
		if (!m_pDBTDetail)
			return _T("");
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		{
			TEnhDS_SynchronizationInfo* pCurrentLine = (TEnhDS_SynchronizationInfo*)m_pDBTDetail->GetCurrentRow();
			CString m_Header = _T("");

			if (!pCurrentLine)
				return m_Header;

			CString sDescription = pCurrentLine->l_Description.FormatData();
			CString sCode = pCurrentLine->l_Code;

			if (!sDescription.IsEmpty())
				m_Header = sDescription + _T(": ");

			if (!sCode.IsEmpty())
				m_Header += _T("[") + sCode + _T("] ");

			return m_Header;
		}
		else
		{
			VEnh_SynchronizationInfo* pCurrentLine = (VEnh_SynchronizationInfo*)m_pDBVTDetail->GetCurrentRow();
			CString m_Header = _T("");

			if (!pCurrentLine)
				return m_Header;

			CString sDescription = pCurrentLine->l_Description.FormatData();
			CString sCode = pCurrentLine->l_Code;

			if (!sDescription.IsEmpty())
				m_Header = sDescription + _T(": ");

			if (!sCode.IsEmpty())
				m_Header += _T("[") + sCode + _T("] ");

			return m_Header;
		}
	}

	return _T("");
}

//----------------------------------------------------------------------------
void DSMonitor::ExpandDetail()
{
	if (m_pMonitorTile)
		m_pMonitorTile->SetCollapsed(!m_pMonitorTile->IsCollapsed());
}