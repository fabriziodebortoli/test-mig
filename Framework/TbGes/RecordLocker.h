
#pragma once

#include <TbGenlib\parsobj.h>
#include <TbGenlib\tabcore.h>

#include "extdoc.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class CRecordLockerColumn : public  CObject
{
private:
	CString  m_sPKColName;
	CString  m_sFKColName;

public:
	CRecordLockerColumn (CString sPKColName, CString sFKColName); 

public:
	const CString& GetPKColName () const { return m_sPKColName; } 
	const CString& GetFKColName () const { return m_sFKColName; } 
};

//=============================================================================
class TB_EXPORT CRecordLockerColumns : public Array
{
	DECLARE_DYNAMIC(CRecordLockerColumns) 

public:
	CRecordLockerColumns (CString sPKColName, CString sFKColName); 

public:
	CRecordLockerColumns () {};

public:
	int Add (const CString& sPKColName, const CString& sFKColName) { return Array::Add (new CRecordLockerColumn(sPKColName, sFKColName)); }

	CRecordLockerColumn* GetAt(int nIndex) const		{ return (CRecordLockerColumn*) Array::GetAt(nIndex); }
	CRecordLockerColumn*&	ElementAt	(int nIndex)	{ return (CRecordLockerColumn*&) Array::ElementAt(nIndex); }

	CRecordLockerColumn* operator[](int nIndex) const	{ return GetAt(nIndex); }
	CRecordLockerColumn*& operator[](int nIndex)		{ return ElementAt(nIndex); }
};

//=============================================================================
class TB_EXPORT CRecordLocker : public  CObject
{
	DECLARE_DYNAMIC(CRecordLocker)

public:
	enum LockOperation { Lock, Unlock };
	enum LockBehaviour { StopIfOneLock, StopIfOneLockAndUnlockAll,  LockAll  };

protected:
	SqlSession*		m_pSqlSession;
	CDiagnostic*	m_pDiagnostic;
	LockBehaviour	m_eLockBehaviour;

public:
	CRecordLocker	(SqlSession* pSqlSession, CDiagnostic*	pDiagnostic);
	CRecordLocker	(CAbstractFormDoc* pDoc);
	~CRecordLocker	();

public:
	BOOL	LockRecords		(
								CRuntimeClass* pLockRecRuntimeClass, 
								const DBTObject* pSourceDBT, 
								const CRecordLockerColumns& arColumnsNames, 
								const int& nFromRow = -1, 
								const int& nToRow = -1
							);
	BOOL	LockRecords		(
								CRuntimeClass* pLockRecRuntimeClass, 
								const RecordArray* pSourceRecords, 
								const CRecordLockerColumns& arColumnsNames
							);

	BOOL	UnlockRecords	(
								CRuntimeClass* pLockRecRuntimeClass, 
								const DBTObject* pSourceDBT, 
								const CRecordLockerColumns& arColumnsNames,
								const int& nFromRow = -1, 
								const int& nToRow = -1
							);
	BOOL	UnlockRecords	(
								CRuntimeClass* pLockRecRuntimeClass, 
								const RecordArray* pSourceRecords, 
								const CRecordLockerColumns& arColumnsNames
							);

	void	SetLockBehaviour (LockBehaviour eBehaviour) { m_eLockBehaviour = eBehaviour; }

private:
	BOOL	LockUnlockRecords	(
									CRuntimeClass* pLockRecRuntimeClass, 
									const DBTObject* pSourceDBT, 
									const CRecordLockerColumns& arColumnsNames,
									const int& nFromRow = -1, 
									const int& nToRow = -1,
									const LockOperation eOperation = Lock
								);
	BOOL	LockUnlockRecords	(
									CRuntimeClass* pRecRuntimeClass, 
									const RecordArray* pSourceRecords, 
									const CRecordLockerColumns& arColumnsNames,
									const LockOperation eOperation
								);

	BOOL	LockUnlockRecord	(
									SqlTable& aTable, 
									SqlRecord* pRecord, 
									const CString& sContextKey,
									const CRecordLockerColumns& arColumnsNames,
									const LockOperation eOperation = Lock
								);
	BOOL	AssignFKToPK		(SqlRecord& aLockRec, SqlRecord* pDBTRecord, const CRecordLockerColumns& arColumnsNames);
	

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

#include "endh.dex"
