// CompanyContext.cpp : implementation file
//
#include "stdafx.h"
#include "CompanyContext.h"
#include "LoginContext.h"

// CCompanyContext

CCompanyContext::CCompanyContext()
	: 
	m_pSqlCatalog			(NULL),
	m_pEnumsTable			(NULL),
	m_pRowSecurityManager	(NULL),
	m_pDataSynchroManager	(NULL)
{
}

CCompanyContext::~CCompanyContext()
{
	FreeObjects();
	delete m_pEnumsTable;
	delete m_pDataSynchroManager;
	if (m_pRowSecurityManager)
		delete m_pRowSecurityManager;	
}


CCompanyContext* AFXAPI AfxGetCompanyContext (const CString& strKey)
{
	return AfxGetApplicationContext()->GetCompanyContext(strKey);
}

CCompanyContext* AFXAPI AfxGetCompanyContext ()
{
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	if (!pInfos)
		return NULL;

	CString strKey;
	strKey.Format(_T("%s@%s@%s"), pInfos->m_strDBName, pInfos->m_strDBServer, pInfos->m_strDatabaseType);
	strKey.MakeLower();

	return AfxGetApplicationContext()->GetCompanyContext(strKey);
}
// CCompanyContext member functions
