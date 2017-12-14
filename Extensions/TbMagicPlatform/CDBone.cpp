
#include "stdafx.h"

#include <TbNameSolver\IFileSystemManager.h>

#include <TbGes\Dbt.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <XEngine\TBXmlTransfer\XMLDataMng.h>
#include <XEngine\TBXmlTransfer\XMLProfileInfo.h>

#include "CSubscriptionData.h"
#include "CDBone.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//             				CDBone
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CDBone, CClientDoc)

//-----------------------------------------------------------------------------
CDBone::CDBone()
	:
	CClientDoc			(),
	m_bManageDataGuid	(FALSE),
	m_pTRExtGuid		(NULL),
	m_pTUExtGuid		(NULL)
{
	
}

//-----------------------------------------------------------------------------
CDBone::~CDBone()
{
	SAFE_DELETE(m_pTRExtGuid);
	SAFE_DELETE(m_pTUExtGuid);
}


//-----------------------------------------------------------------------------
BOOL CDBone::OnAttachData()
{
	if (!((CAbstractFormDoc*) m_pServerDocument)->m_pDBTMaster->GetTable()->GetRecord()->HasGUID())
	{
		ASSERT(FALSE);
		TRACE(_T("Trying to trace a table w/o the column TbGuid"));
		return TRUE;
	}
	
	m_bManageDataGuid = TRUE;
	m_pTRExtGuid = new TRExtGuid((CAbstractFormDoc*) m_pServerDocument);
	m_pTUExtGuid = new TUExtGuid((CAbstractFormDoc*) m_pServerDocument);
	m_pTUExtGuid->SetAutocommit();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnBeforeNewRecord()
{
	if (!m_bManageDataGuid || m_pServerDocument->IsRunningFromExternalController())
		return TRUE;

	CEventManagementData* pEventManagementData =
		AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 1, _T("CHECK"), TRUE);

	if (pEventManagementData)
		return ProcessSubscriptions(pEventManagementData);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnBeforeEditRecord()
{
	if (!m_bManageDataGuid || m_pServerDocument->IsRunningFromExternalController())
		return TRUE;

	if (FindExtGuid())
	{
		CEventManagementData* pEventManagementData =
			AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 2, _T("CHECK"), TRUE);

		if (pEventManagementData)
			return ProcessSubscriptions(pEventManagementData);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnBeforeDeleteRecord()
{
	if (!m_bManageDataGuid || m_pServerDocument->IsRunningFromExternalController())
		return TRUE;

	if (FindExtGuid())
	{
		CEventManagementData* pEventManagementData =
			AfxGetSubscriptionInfo()->GetEventManagementData((CAbstractFormDoc*) m_pServerDocument, 3, _T("CHECK"), TRUE);

		if (pEventManagementData)
			return ProcessSubscriptions(pEventManagementData);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnExtraNewTransaction()
{
	if (!m_bManageDataGuid || !m_pServerDocument->IsRunningFromExternalController())
		return TRUE;

	SqlRecord* pRec = ((CAbstractFormDoc*) m_pServerDocument)->m_pDBTMaster->GetRecord();
	if (m_pTUExtGuid->FindRecord(pRec->f_TBGuid) == TableUpdater::NOT_FOUND)
	{
		m_pTUExtGuid->GetRecord()->f_DocGuid = pRec->f_TBGuid;
		m_pTUExtGuid->UpdateRecord();
	}
	else
	{
		GetDiagnostic()->Add(_TB("Trying to add a TbGuid already present on the table TB_ExtGuid"), CDiagnostic::Error);
		GetDiagnostic()->Show(TRUE);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnExtraEditTransaction()
{
	if (!m_bManageDataGuid || !m_pServerDocument->IsRunningFromExternalController())
		return TRUE;

	SqlRecord* pRec = ((CAbstractFormDoc*) m_pServerDocument)->m_pDBTMaster->GetRecord();
	if (m_pTUExtGuid->FindRecord(pRec->f_TBGuid) == TableUpdater::FOUND)
		m_pTUExtGuid->UpdateRecord();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::OnExtraDeleteTransaction()
{
	SqlRecord* pRec = ((CAbstractFormDoc*) m_pServerDocument)->m_pDBTMaster->GetOldRecord();
	if (m_pTUExtGuid->FindRecord(pRec->f_TBGuid) == TableUpdater::FOUND)
		m_pTUExtGuid->DeleteRecord();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::FindExtGuid()
{
	SqlRecord* pRec = ((CAbstractFormDoc*) m_pServerDocument)->m_pDBTMaster->GetRecord();
	return m_pTRExtGuid->FindRecord(pRec->f_TBGuid) == TableReader::FOUND;
}

//-----------------------------------------------------------------------------
BOOL CDBone::ShowMessage(CSubscriptionData* pSubscriptionData)
{
	if (pSubscriptionData->m_strOperation == _T("DENY"))
	{
		GetDiagnostic()->Add(pSubscriptionData->m_strMessage, CDiagnostic::Error);
		GetDiagnostic()->Show(TRUE);
		return FALSE;
	}

	GetDiagnostic()->Add(pSubscriptionData->m_strMessage, CDiagnostic::Warning);
	GetDiagnostic()->Show(TRUE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDBone::ProcessSubscriptions (CEventManagementData* pEventManagementData)
{
	BOOL bOk = TRUE;

	CString aKey;
	CSubscriptionData* pItem = NULL;
	POSITION aPos = pEventManagementData->GetStartPosition();
	while (aPos != NULL)
	{
		pEventManagementData->GetNextAssoc(aPos,aKey,pItem);
		bOk = ShowMessage(pItem) && bOk;
	}

	return bOk;
}