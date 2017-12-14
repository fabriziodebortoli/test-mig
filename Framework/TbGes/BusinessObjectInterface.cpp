
#include "stdafx.h"

#include "Dbt.h"
#include "BusinessObjectInterface.h"

/////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//								class CInterfaceItem
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (CInterfaceItem, CObject)

//-----------------------------------------------------------------------------	
CInterfaceItem::CInterfaceItem
	(
		DataObj**	pInterfaceDataPtr, 
		BOOL		bOptional
	) 
	: 
	m_pInterfaceDatePtr	(pInterfaceDataPtr),
	m_bOptional			(bOptional)
{
	Detach();
	Unbind();
}

//-----------------------------------------------------------------------------	
void CInterfaceItem::Attach(DataObj* pAttachedDatePtr)
{
	// o si binda un p.tore o una colonna di SqlRecord
	ASSERT(m_nAttachedDateIdx == -1);
	ASSERT(!m_pAttachedDatePtr);

	m_pAttachedDatePtr = pAttachedDatePtr;
}

//-----------------------------------------------------------------------------	
void CInterfaceItem::Attach(SqlRecord* pRec, DataObj* pDataObj)
{
	// o si binda un p.tore o una colonna di SqlRecord
	ASSERT(m_nAttachedDateIdx == -1);
	ASSERT(!m_pAttachedDatePtr);

	m_nAttachedDateIdx	= pRec->Lookup(pDataObj);
	m_pSqlRecordClass	= pRec->GetRuntimeClass();
	// il dataobj deve esserci nel sqlrec
	ASSERT(m_nAttachedDateIdx != -1);
}

//-----------------------------------------------------------------------------	
void CInterfaceItem::Detach()
{
	m_nAttachedDateIdx	= -1;
	m_pSqlRecordClass	= NULL;
	m_pAttachedDatePtr	= NULL;
}

//-----------------------------------------------------------------------------	
void CInterfaceItem::Bind(SqlRecord* pRec)
{
	*m_pInterfaceDatePtr = NULL;
	// se il DataObj e' stato attachato direttamente (es. datamember di classe), ingnora il SqlRec e assegna
	// semplicemente il p.tore
	if (m_pAttachedDatePtr)
		*m_pInterfaceDatePtr = m_pAttachedDatePtr;
	// se l'attach e' con un campo di SqlRec, binda il dataobj del SqlRec attuale recuperandolo a partire dall'indice
	else if (m_nAttachedDateIdx != -1)
	{
		// deve essere lo stesso SqlRecord che ho usato nella Attach
		if (pRec && m_pSqlRecordClass == pRec->GetRuntimeClass())
		{
			*m_pInterfaceDatePtr = pRec->GetDataObjAt(m_nAttachedDateIdx);
			ASSERT(*m_pInterfaceDatePtr);
		}
		else
			ASSERT(FALSE);
	}
	else
		// il p.tore di interfaccia puo' restare vuoto a patto che sia stato dichiarato come opzionale
		ASSERT(m_bOptional);
}

//-----------------------------------------------------------------------------	
void CInterfaceItem::Unbind()
{
	*m_pInterfaceDatePtr = NULL;
}

//-----------------------------------------------------------------------------	
BOOL CInterfaceItem::IsEqual(DataObj** pInterfaceDataPtr)
{
	ASSERT(pInterfaceDataPtr);

	return m_pInterfaceDatePtr == pInterfaceDataPtr;
}

//-----------------------------------------------------------------------------	
BOOL CInterfaceItem::Check()
{
	if (m_bOptional)
		return TRUE;
	else
		return IsAttached();
}

//-----------------------------------------------------------------------------	
BOOL CInterfaceItem::IsAttached()
{
	return	m_pAttachedDatePtr != NULL ||
			m_nAttachedDateIdx != -1;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CInterfaceItem::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n", GetRuntimeClass()->m_lpszClassName);
}

void CInterfaceItem::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//								class CBusinessObjectInterfaceObj
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (CBusinessObjectInterfaceObj, CObject)

//-----------------------------------------------------------------------------	
CBusinessObjectInterfaceObj::CBusinessObjectInterfaceObj(DBTObject* pDBT /*= NULL*/)
	:
	m_pDBT(pDBT)
{
}

//-----------------------------------------------------------------------------	
CBusinessObjectInterfaceObj::~CBusinessObjectInterfaceObj()
{
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::AddItem
	(			
		DataObj**	pInterfaceDataPtr, 
		BOOL		bOptional
	)
{
	if (LookUp(pInterfaceDataPtr))
	{
		ASSERT(FALSE); // un campo di interfaccia puo' essere bindato una sola volta
		return;
	}
	m_Items.Add(new CInterfaceItem(pInterfaceDataPtr,bOptional));
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::RemoveAllItems()
{
	m_Items.RemoveAll();
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::AttachDataObj
	(
		DataObj**	pInterfaceDataPtr, 
		DataObj*	pAttachedDatePtr
	)
{
	CInterfaceItem* pItem = LookUp(pInterfaceDataPtr);
	if (!pItem)
	{
		ASSERT(FALSE); // per essere attachato un campo di interfaccia deve prima essere aggiunto
		return;
	}
	// controlla che il tipo di dataobj passato sia lo stesso
	//ASSERT(pInterfaceData->IsKindOf(pAttachedDataPtr->GetRuntimeClass()));

	pItem->Attach(pAttachedDatePtr);
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::AttachDataObj	
	(
		DataObj**	pInterfaceDataPtr, 
		SqlRecord*	pRec, 
		DataObj*	pDataObj
	)
{
	CInterfaceItem* pItem = LookUp(pInterfaceDataPtr);
	if (!pItem)
	{
		ASSERT(FALSE); // per essere attachato un campo di interfaccia deve prima essere aggiunto
		return;
	}
	// controlla che il tipo di dataobj passato sia lo stesso, e, se ho un DBT, che il tipo di SQLRecord passato
	// sia lo stesso legato al DBT
	//ASSERT(pInterfaceData->IsKindOf(pDataObj->GetRuntimeClass()));
	if (m_pDBT)
	{
		ASSERT(pRec->IsKindOf(m_pDBT->GetRecord()->GetRuntimeClass()));
	}

	pItem->Attach(pRec,pDataObj);
}

//-----------------------------------------------------------------------------	
BOOL CBusinessObjectInterfaceObj::IsAttached(DataObj** pInterfaceDataPtr)
{
	CInterfaceItem* pItem = LookUp(pInterfaceDataPtr);
	if (!pItem)
	{
		ASSERT(FALSE); // per essere attachato un campo di interfaccia deve prima essere aggiunto
		return FALSE;
	}

	return pItem->IsAttached();
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::DetachAllDataObjs()
{
	for (int i = 0; i <= m_Items.GetUpperBound(); i++)
		m_Items.GetAt(i)->Detach();
}

//-----------------------------------------------------------------------------	
BOOL CBusinessObjectInterfaceObj::Check()
{
	for (int i = 0; i <= m_Items.GetUpperBound(); i++)
		if (!m_Items.GetAt(i)->Check())
			return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::BindDBT(SqlRecord* pRec)
{
	// le singole Bind assertano se il p.tore di interfaccia risulta vuoto ma non
	// e' opzionale
	for (int i = 0; i <= m_Items.GetUpperBound(); i++)
		m_Items.GetAt(i)->Bind(pRec);
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::BindDBT(BOOL bOld /*FALSE*/)
{
	if	(
			!m_pDBT ||
			m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) // se SB, obbligatorio il nr. di riga
		)
	{
		ASSERT(FALSE);
		return;
	}
	
	if (!bOld)
		BindDBT(m_pDBT->GetRecord());
	else
		BindDBT(m_pDBT->GetOldRecord());
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::BindLine(int i, BOOL bOld /*= FALSE*/)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) ||
			i < 0 ||
			(!bOld && i > ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound()) ||
			(bOld && i > ((DBTSlaveBuffered*)m_pDBT)->GetOldUpperBound())
		)
	{
		ASSERT(FALSE);
		return;
	}

	if (!bOld)
		BindDBT(((DBTSlaveBuffered*)m_pDBT)->GetRow(i));
	else
		BindDBT(((DBTSlaveBuffered*)m_pDBT)->GetOldRow(i));
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::UnbindAll()
{
	for (int i = 0; i <= m_Items.GetUpperBound(); i++)
		m_Items.GetAt(i)->Unbind();
}

//-----------------------------------------------------------------------------	
BOOL CBusinessObjectInterfaceObj::SameDBT(DBTObject* pDBT)
{
	if	(!m_pDBT)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return m_pDBT == pDBT;
}

//-----------------------------------------------------------------------------	
int CBusinessObjectInterfaceObj::GetUpperBound(BOOL bOld /*= FALSE*/)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return -1;
	}

	if (!bOld)
		return ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound();
	else
		return ((DBTSlaveBuffered*)m_pDBT)->GetOldUpperBound();
}

//-----------------------------------------------------------------------------	
int CBusinessObjectInterfaceObj::GetSize()
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return -1;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetSize();
}

//-----------------------------------------------------------------------------	
int CBusinessObjectInterfaceObj::GetOldSize()
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return -1;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetOldSize();
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::SetCurrentRow(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))||
			i < 0 ||
			i > ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound()
		)
	{
		ASSERT(FALSE);
		return;
	}

	((DBTSlaveBuffered*)m_pDBT)->SetCurrentRow(i);
}

//-----------------------------------------------------------------------------	
int CBusinessObjectInterfaceObj::GetCurrentRowIdx()
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return -1;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetCurrentRowIdx();
}

//-----------------------------------------------------------------------------	
BOOL CBusinessObjectInterfaceObj::IsStorable(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) ||
			i < 0 ||
			i > ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound()
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetRow(i)->IsStorable();
}

//-----------------------------------------------------------------------------	
SqlRecord* CBusinessObjectInterfaceObj::GetRow(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetRow(i);
}

//-----------------------------------------------------------------------------	
SqlRecord* CBusinessObjectInterfaceObj::GetOldRow(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->GetOldRow(i);
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::SetStorable(int i, bool bSet /*= TRUE*/)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) ||
			i < 0 ||
			i > ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound()
		)
	{
		ASSERT(FALSE);
		return;
	}
	
	((DBTSlaveBuffered*)m_pDBT)->GetRow(i)->SetStorable(bSet);
}

//-----------------------------------------------------------------------------	
SqlRecord* CBusinessObjectInterfaceObj::AddRecord()
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->AddRecord();
}

//-----------------------------------------------------------------------------	
SqlRecord* CBusinessObjectInterfaceObj::InsertRecord(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return ((DBTSlaveBuffered*)m_pDBT)->InsertRecord(i);
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::DeleteRecord(int i)
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) ||
			i < 0 ||
			i > ((DBTSlaveBuffered*)m_pDBT)->GetUpperBound()
		)
	{
		ASSERT(FALSE);
		return;
	}

	// la cancellazione di un record dal DBT SB forza un unbinding, per prevenire l'accesso a
	// memoria disallocata. Il p.tore a NULL rende l'eventuale crash piu' esplicito
	UnbindAll();
	((DBTSlaveBuffered*)m_pDBT)->DeleteRecord(i);
}

//-----------------------------------------------------------------------------	
void CBusinessObjectInterfaceObj::RemoveAll()
{
	if	(
			!m_pDBT ||
			!m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))
		)
	{
		ASSERT(FALSE);
		return;
	}

	// la cancellazione di tutte le righe del DBT SB forza un unbinding, per prevenire l'accesso a
	// memoria disallocata. Il p.tore a NULL rende l'eventuale crash piu' esplicito
	UnbindAll();
	((DBTSlaveBuffered*)m_pDBT)->RemoveAll();
}

//-----------------------------------------------------------------------------	
CInterfaceItem*	CBusinessObjectInterfaceObj::LookUp(DataObj** pInterfaceDataPtr)
{
	for (int i = 0; i <= m_Items.GetUpperBound(); i++)
	{
		CInterfaceItem* pItem = m_Items.GetAt(i);
		if (pItem->IsEqual(pInterfaceDataPtr))
			return pItem;
	}

	return NULL;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBusinessObjectInterfaceObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n", GetRuntimeClass()->m_lpszClassName);
}

void CBusinessObjectInterfaceObj::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG
