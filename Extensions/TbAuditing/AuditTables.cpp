
#include "stdafx.h" 

#include <TbOleDb\OleDbMng.h>

#include "audittables.h"
#include "auditingmanager.h"

//////////////////////////////////////////////////////////////////////////////
//								TAuditingRecord
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
static const TCHAR szAU_ID			[]	=	_T("AU_ID");
static const TCHAR szOperData		[]	=	_T("AU_OperationData");
static const TCHAR szOperType		[]	=	_T("AU_OperationType");
static const TCHAR szLoginName		[]	=	_T("AU_LoginName");
static const TCHAR szNamespaceID	[]	=	_T("AU_NameSpaceID");

//-----------------------------------------------------------------------------
TAuditingRecord::TAuditingRecord(CString strTableName, CString strAuditTableName)
	:
	SqlRecord(strAuditTableName)
{
	f_OperData.SetFullDate();

	m_strTracedTable = strTableName;
	BindRecord();		
	BOOL bValid = IsValid(); 
	SetConnection(AfxGetAuditingSqlConnection()); //il cambio di connessione ripone lo stato a TRUE e 
												  // viene messo a FALSE solo se c'è problema nella struttura fisica
	SetValid(bValid);
}

//-----------------------------------------------------------------------------
TAuditingRecord::~TAuditingRecord()
{
	SqlRecordItem* pRecItem = NULL;
	DataObj* pDataObj = NULL;

	//devo togliere dal bind del sqlrecord i campi aggiunti e devo deletare i puntatori dei DataObj
	for (int nCol = GetUpperBound(); nCol >= m_nStartVarFieldsPos; nCol--)
	{
		pRecItem = GetAt(nCol);
		if (pRecItem)
			pDataObj = pRecItem->GetDataObj();
		RemoveAt(nCol);
		if (pDataObj)
		{
			delete pDataObj;
			pDataObj = NULL;
		}
	}	
}

//-----------------------------------------------------------------------------
void TAuditingRecord::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_DATA	(szAU_ID,		f_ID)
		BIND_DATA	(szOperData,	f_OperData)
		BIND_DATA	(szOperType,	f_OperType)
		BIND_DATA	(szLoginName,	f_LoginName)
		BIND_DATA	(szNamespaceID,	f_NamespaceID)

		BindVariableFields(nPos);
	};
}
	
//-----------------------------------------------------------------------------
void TAuditingRecord::BindVariableFields(int& nPos)
{
	m_nStartVarFieldsPos = nPos;
	SqlRecord* pRecord = AfxGetDefaultSqlConnection()->GetCatalogEntry(m_strTracedTable)->CreateRecord();
	if (!pRecord)
	{
		SetValid(FALSE);
		return;
	}

	DataObj* pAuditData = NULL;
	DataObj* pRecData = NULL;
	SqlColumnInfo* pColumnInfo = NULL;	

	const SqlTableInfo *pInfo = GetTableInfo();
	const  Array* pColums = pInfo->GetPhysicalColumns();
	BOOL bIsAutoincrement = FALSE;
	for (nPos; nPos < pColums->GetSize(); nPos++)
	{
       	SqlColumnInfo* pColumnInfo = (SqlColumnInfo*) pColums->GetAt(nPos);
		DataObj* pRecData = pRecord->GetDataObjFromColumnName(pColumnInfo->GetColumnName());
		if (pRecData)
		{
			pAuditData = pRecData->DataObjClone();	
			BindGenericDataObj
							(
								nPos, 
								pColumnInfo->GetColumnName(), 
								*pAuditData, 	
								FALSE//è giusto che non sia mai autoincrement, altrimenti nella tabella di auditing ci andrebbe un valore dato dal database e non quello del record di erp
							);
		}	
	}
	delete pRecord;
	
	SetValid(nPos > m_nStartVarFieldsPos); //il record é valido se é presente almeno una colonna della tabella da tracciare
}

//////////////////////////////////////////////////////////////////////////////
//								TAuditTables
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
static const TCHAR szAuditTableName	[]	=	_T("AUDIT_Tables");
static const TCHAR szTableName		[]	=	_T("TableName");
static const TCHAR szStartTrace		[]	=	_T("StartTrace");
static const TCHAR szSuspended		[]	=	_T("Suspended");

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TAuditTables, SqlRecord) 

//-----------------------------------------------------------------------------
TAuditTables::TAuditTables(BOOL bCallInit)
	:
	SqlRecord(szAuditTableName, AfxGetAuditingSqlConnection()),
	f_Suspended(FALSE)
{
	f_TableName.SetUpperCase();

	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TAuditTables::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_DATA	(szTableName,	f_TableName )
		BIND_DATA	(szStartTrace,	f_StartTrace)
		BIND_DATA	(szSuspended,	f_Suspended	)
	END_BIND_DATA()
}
	
//-----------------------------------------------------------------------------
LPCTSTR TAuditTables::GetStaticName() { return szAuditTableName; }

//////////////////////////////////////////////////////////////////////////////
//								TNamespaces
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
static const TCHAR szNamespacesTableName[]	=	_T("AUDIT_Namespaces");
static const TCHAR szID				  []	=	_T("ID");
static const TCHAR szType				  []	=	_T("TypeNs");
static const TCHAR szNamespace		  []	=	_T("Namespace");


//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TNamespaces, SqlRecord) 

//-----------------------------------------------------------------------------
TNamespaces::TNamespaces(BOOL bCallInit)
	:
	SqlRecord(szNamespacesTableName, AfxGetAuditingSqlConnection())
{
	BindRecord();	
	if (bCallInit) Init(); 
}
//-----------------------------------------------------------------------------
void TNamespaces::Init()
{
	SqlRecord::Init();
	f_Type.Assign(REPORT_NSTYPE);
}

//-----------------------------------------------------------------------------
void TNamespaces::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_AUTOINCREMENT	(szID,			f_ID )
		BIND_DATA			(szNamespace,	f_Namespace)
		BIND_DATA			(szType,		f_Type)
	END_BIND_DATA()
}
	
//-----------------------------------------------------------------------------
LPCTSTR TNamespaces::GetStaticName() { return szNamespacesTableName; }

