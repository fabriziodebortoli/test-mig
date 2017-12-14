#include "stdafx.h"

#include "DSComponents.h"

static TCHAR szParamTBGuid[] = _T("ParamTBGuid");

//////////////////////////////////////////////////////////////////////////////
//				Class	CBrowserByTBGuid	Implementation					//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(CBrowserByTBGuid, CObject)

//-----------------------------------------------------------------------------
CBrowserByTBGuid::CBrowserByTBGuid(CAbstractFormDoc* pDoc, SqlSession* pSqlSession)
{
	m_pDocument		= pDoc;
	m_pSqlSession	= pSqlSession;
}

//-----------------------------------------------------------------------------
BOOL CBrowserByTBGuid::BrowseOnRecord(DataGuid idTBGuid)
{
	CString strTableName	= m_pDocument->m_pDBTMaster->GetTable()->m_strTableName;
	SqlRecord* pCurrentRec	= m_pDocument->m_pDBTMaster->GetRecord();

	SqlTable* pTable = new SqlTable(pCurrentRec, m_pSqlSession);
	pTable->Open();
	pTable->SelectAll();
	
	pTable->AddParam			(szParamTBGuid,		pCurrentRec->f_TBGuid);
	pTable->AddFilterColumn		(pCurrentRec->f_TBGuid);
	pTable->SetParamValue		(szParamTBGuid,		idTBGuid);
	
	pTable->Query();
	SqlRecord* pResult = NULL;

	while (!pTable->IsEOF())
	{
		pResult = pTable->GetRecord();
		pTable->MoveNext();
	}

	pTable->Close();

	SAFE_DELETE(pTable);

	if (pResult)
	{
		*m_pDocument->m_pDBTMaster->GetRecord() = *pResult;
		m_pDocument->BrowseRecord();

		return TRUE;
	}

	return FALSE;
}
