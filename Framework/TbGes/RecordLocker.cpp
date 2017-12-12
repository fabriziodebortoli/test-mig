#include "stdafx.h"

#include <TbGeneric\array.h>

#include <TbGes\DBT.h>
#include <TbGes\Extdoc.h>

#include "RecordLocker.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					class CRecordLockerFK implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CRecordLockerColumn::CRecordLockerColumn (CString sPKColName, CString sFKColName)
{
	m_sPKColName = sPKColName;
	m_sFKColName = sFKColName;
}

/////////////////////////////////////////////////////////////////////////////
//					class CRecordLockerFK implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRecordLockerColumns, Array) 

/////////////////////////////////////////////////////////////////////////////
//					class CRecordLocker implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRecordLocker, CObject)

//----------------------------------------------------------------------------
CRecordLocker::CRecordLocker (SqlSession* pSqlSession, CDiagnostic*	pDiagnostic)
	:
	m_pSqlSession		(pSqlSession),
	m_pDiagnostic		(pDiagnostic),
	m_eLockBehaviour	(StopIfOneLockAndUnlockAll)
{
	ASSERT(m_pSqlSession);
}

//----------------------------------------------------------------------------
CRecordLocker::CRecordLocker (CAbstractFormDoc* pDoc)
	:
	m_pSqlSession		(NULL),
	m_pDiagnostic		(NULL),
	m_eLockBehaviour	(StopIfOneLockAndUnlockAll)
{
	ASSERT(pDoc);
	if (pDoc)
	{
		m_pSqlSession	= pDoc->GetReadOnlySqlSession();
		m_pDiagnostic	= pDoc->m_pMessages;
	}
}

//----------------------------------------------------------------------------
CRecordLocker::~CRecordLocker()
{
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::LockRecords (
								CRuntimeClass* pLockRecRuntimeClass, 
								const DBTObject* pSourceDBT, 
								const CRecordLockerColumns& arColumnsNames,
								const int& nFromRow /*-1*/, 
								const int& nToRow /*1*/ 
							)
{
	return LockUnlockRecords (pLockRecRuntimeClass, pSourceDBT, arColumnsNames, nFromRow, nToRow, Lock);
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::UnlockRecords(
								CRuntimeClass* pLockRecRuntimeClass, 
								const DBTObject* pSourceDBT, 
								const CRecordLockerColumns& arColumnsNames,
								const int& nFromRow /*-1*/, 
								const int& nToRow /*1*/ 
							)
{
	return LockUnlockRecords (pLockRecRuntimeClass, pSourceDBT, arColumnsNames, nFromRow, nToRow, Unlock);
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::LockUnlockRecords(
										CRuntimeClass* pLockRecRuntimeClass, 
										const DBTObject* pSourceDBT, 
										const CRecordLockerColumns& arColumnsNames,
										const int& nFromRow /*-1*/, 
										const int& nToRow /*1*/ ,
										const LockOperation eOperation
									)
{
	if (!pLockRecRuntimeClass || !pSourceDBT || !arColumnsNames.GetSize())
	{
		ASSERT(FALSE);
		if (m_pDiagnostic)
			m_pDiagnostic->Add (_TB("CRecordLocker::LockUnLockDBT call with NULL parameters!"));
		return FALSE;
	}

	SqlRecord* pRecord = (SqlRecord*) pLockRecRuntimeClass->CreateObject();

	if (!pRecord)
	{
		ASSERT(FALSE);
		if (m_pDiagnostic)
			m_pDiagnostic->Add (cwsprintf(_TB("CRecordLocker::LockUnLockDBT: cannot instantiate SqlRecord class %s!"), CString(pLockRecRuntimeClass->m_lpszClassName))); 
		delete pRecord;
 		return FALSE;
	}

	SqlTable aSqlTable(pRecord, m_pSqlSession);
	aSqlTable.EnableLocksCache (TRUE);

	BOOL bOk = TRUE;
	if (pSourceDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		DBTSlaveBuffered* pDBTSlaveBuffered = (DBTSlaveBuffered*) pSourceDBT;
		SqlRecord* pDBTRec;
		int nFrom	= nFromRow >= 0 ? nFromRow : 0;
		int nTo		= nToRow >= 0 ? nToRow : pDBTSlaveBuffered->GetUpperBound();
		
		CString sLockContextKey = cwsprintf(_T("%lp %d %d"), pDBTSlaveBuffered, nFrom, nTo);

		for (int i=nFrom; i <= nTo; i++)
		{
			pDBTRec = pDBTSlaveBuffered->GetRow(i);
			if (!pDBTRec)
				continue;

			bOk = LockUnlockRecord (aSqlTable, pDBTRec, sLockContextKey, arColumnsNames, eOperation) && bOk;

			if (m_eLockBehaviour == StopIfOneLock && !bOk)
				break;

			if (m_eLockBehaviour == StopIfOneLockAndUnlockAll && !bOk)
			{
				aSqlTable.UnlockAllLockContextKeys (sLockContextKey, &aSqlTable);
				break;
			}
		}
		delete pRecord;
		return bOk;
	}

	CString sLockContextKey = cwsprintf(_T("%lp"), pSourceDBT);
	bOk = LockUnlockRecord (aSqlTable, pSourceDBT->GetRecord(), sLockContextKey, arColumnsNames, eOperation);
	delete pRecord;
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::LockRecords (CRuntimeClass* pLockRecRuntimeClass, const RecordArray* pSourceRecords, const CRecordLockerColumns& arColumnsNames)
{
	return LockUnlockRecords (pLockRecRuntimeClass, pSourceRecords, arColumnsNames, Lock);
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::UnlockRecords (CRuntimeClass* pLockRecRuntimeClass, const RecordArray* pSourceRecords, const CRecordLockerColumns& arColumnsNames)
{
	return LockUnlockRecords (pLockRecRuntimeClass, pSourceRecords, arColumnsNames, Unlock);
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::LockUnlockRecords (
											CRuntimeClass* pLockRecRuntimeClass, 
											const RecordArray* pSourceRecords, 
											const CRecordLockerColumns& arColumnsNames,
											const LockOperation eOperation
										)
{
	if (!pLockRecRuntimeClass || !pSourceRecords || !arColumnsNames.GetSize())
	{
		ASSERT(FALSE);
		if (m_pDiagnostic)
			m_pDiagnostic->Add (_TB("CRecordLocker::LockUnlockRecords call with NULL parameters!"));
		return FALSE;
	}

	SqlRecord* pRecord = (SqlRecord*) pLockRecRuntimeClass->CreateObject();

	if (pRecord)
	{
		ASSERT(FALSE);
		if (m_pDiagnostic)
			m_pDiagnostic->Add (cwsprintf(_TB("CRecordLocker::LockUnlockRecords: cannot instantiate SqlRecord class %s!"), CString(pLockRecRuntimeClass->m_lpszClassName))); 
 		return FALSE;
	}

	SqlTable aSqlTable(pRecord, m_pSqlSession);
	aSqlTable.EnableLocksCache (TRUE);

	BOOL bOk = FALSE;
	SqlRecord* pRec;
	CString sLockContextKey = cwsprintf(_T("%lp"), arColumnsNames);

	for (int i=0; i <= pSourceRecords->GetUpperBound(); i++)
	{
		pRec = pSourceRecords->GetAt(i);
		if (!pRec)
			continue;

		bOk = LockUnlockRecord (aSqlTable, pRec, sLockContextKey, arColumnsNames, eOperation) && bOk;
		
		if (m_eLockBehaviour == StopIfOneLock && !bOk)
			break;

		if (m_eLockBehaviour == StopIfOneLockAndUnlockAll && !bOk)
		{
			aSqlTable.UnlockAllLockContextKeys (sLockContextKey, &aSqlTable);
			break;
		}
	}

	return bOk;
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::LockUnlockRecord	(	
											SqlTable& aTable, 
											SqlRecord* pRecord, 
											const CString& sContextKey,
											const CRecordLockerColumns& arColumnsNames,
											const LockOperation eOperation
										)
{
	if (! AssignFKToPK(*aTable.GetRecord(), pRecord, arColumnsNames))
		return FALSE;

	if (eOperation == Lock)
		return aTable.LockTableKey (aTable.GetRecord());

	return aTable.UnlockTableKey (aTable.GetRecord());
}

//----------------------------------------------------------------------------
BOOL CRecordLocker::AssignFKToPK (SqlRecord& aLockRec, SqlRecord* pDBTRecord, const CRecordLockerColumns& arColumnsNames)
{
	ASSERT(pDBTRecord);

	CRecordLockerColumn* pColumn;  
	for (int i=0; i <= arColumnsNames.GetUpperBound(); i++)
	{
		pColumn = arColumnsNames.GetAt(i);
		if (!pColumn || pColumn->GetPKColName().IsEmpty() || pColumn->GetFKColName().IsEmpty())
			continue;

		DataObj* pFKDataObj = pDBTRecord->GetDataObjFromColumnName (pColumn->GetFKColName());
		if (!pFKDataObj)
		{
			ASSERT(FALSE);
			if (m_pDiagnostic)
				m_pDiagnostic->Add (cwsprintf(_TB("CRecordLocker::LockDBT: foreign key column {0-%s} not found in SqlRecord {1-%s}!"), pColumn->GetFKColName(), pDBTRecord->GetTableName())); 
		}

		DataObj* pPKDataObj = aLockRec.GetDataObjFromColumnName (pColumn->GetPKColName());
		if (!pPKDataObj)
		{
			ASSERT(FALSE);
			if (m_pDiagnostic)
				m_pDiagnostic->Add (cwsprintf(_TB("CRecordLocker::LockDBT: primary key column {0-%s} not found in SqlRecord {1-%s}!"), pColumn->GetPKColName(), aLockRec.GetTableName())); 
		}

		if (pFKDataObj->GetDataType () != pPKDataObj->GetDataType ())
		{
			ASSERT(FALSE);
			if (m_pDiagnostic)
				m_pDiagnostic->Add (cwsprintf
				(
					_TB("CRecordLocker::LockDBT: primary key column {0-%s} in {1-%s} has different data type from foreign key column {2-%s} found in {3-%s}!"), 
					pColumn->GetPKColName(), 
					aLockRec.GetTableName(), 
					pColumn->GetFKColName(), 
					pDBTRecord->GetTableName()
				)); 
		}

		pPKDataObj->Assign(*pFKDataObj);
	}

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CRecordLocker::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCRecordLocker");
	
	CObject::Dump(dc);
}

void CRecordLocker::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG