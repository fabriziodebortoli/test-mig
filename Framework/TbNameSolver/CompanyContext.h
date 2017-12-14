#pragma once

#include "GenericContext.h"

#include "beginh.dex"

// CCompanyContext command target

class TB_EXPORT CCompanyContext : public CObject, public CGenericContext<CCompanyContext>, public CTBLockable
{
	CObject*	m_pSqlCatalog;
	CObject*	m_pEnumsTable;	
	CObject*	m_pRowSecurityManager;	
	CObject*	m_pDataSynchroManager;

public:
	CCompanyContext();
	virtual ~CCompanyContext();

	CObject*	GetEnumsTable			()	{ return m_pEnumsTable; }
	CObject*	GetRowSecurityManager	()	{ return m_pRowSecurityManager; }
	CObject*	GetDataSynchroManager	()	{ return m_pDataSynchroManager; }


	void AttachEnumsTable(CObject* pObj)			{ delete m_pEnumsTable; m_pEnumsTable = pObj; }
	void AttachRowSecurityManager(CObject* pObj)	{ delete m_pRowSecurityManager; m_pRowSecurityManager = pObj; }
	void AttachDataSynchroManager(CObject* pObj)	{ delete m_pDataSynchroManager; m_pDataSynchroManager = pObj; }
	

	virtual LPCSTR  GetObjectName()	const { return "CCompanyContext"; }
	virtual BOOL	IsSingleton()	const { return FALSE; }

private:
	virtual CObject* InternalGetObject(GetMethod getMethod) { return ((this)->*getMethod)(); }
	virtual BOOL IsValid() { return this != NULL; }
};

//-----------------------------------------------------------------------------
TB_EXPORT CCompanyContext* AFXAPI AfxGetCompanyContext (const CString& strKey);
TB_EXPORT CCompanyContext* AFXAPI AfxGetCompanyContext ();

#include "endh.dex"
