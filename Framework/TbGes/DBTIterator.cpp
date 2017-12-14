#include "StdAfx.h"
#include "DBTIterator.h"

void DBTIterator::Pop()
{
	m_nLevel--;
	DBTIteratorObj* pOld = m_pCurrent;
	m_pCurrent = m_pCurrent->m_pParent;
	delete pOld;
}

void DBTIterator::Push(DBTIteratorObj* pNew)
{
	m_nLevel++;
	DBTIteratorObj* pOld = m_pCurrent;
	m_pCurrent = pNew;
	m_pCurrent->m_pParent = pOld;
}

DBTObject* DBTSlaveBufferedIterator::GetNextDbtObject(DBTIterator& iterator)
{
	DBTSlaveBuffered* pSlave = (DBTSlaveBuffered*)m_pDBT;
	if (m_nIndex > pSlave->m_DBTSlaveData.GetUpperBound())
	{
		//POP
		iterator.Pop();
		return iterator.m_pCurrent->GetNextDbtObject(iterator);
	}
	DBTSlaveMap* pData = pSlave->m_DBTSlaveData.GetAt(m_nIndex++);

	//PUSH 
	iterator.Push(new DBTSlaveDataIterator(pSlave, pData));
	//PUSH
	if (pData->m_pDBTSlavePrototype->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		iterator.Push(new DBTSlaveBufferedIterator(pData->m_pDBTSlavePrototype));
	return pData->m_pDBTSlavePrototype;
}

DBTObject* DBTSlaveDataIterator::GetNextDbtObject(DBTIterator& iterator)
{
	if (!m_nPos)
	{
		//POP
		iterator.Pop();
		return iterator.m_pCurrent->GetNextDbtObject(iterator);
	}
	SqlRecord* pRec=NULL;
	DBTSlave* pSlave=NULL;
	m_pData->m_Slaves.GetNextAssoc(m_nPos, pRec, pSlave);
	if (pSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		iterator.Push(new DBTSlaveBufferedIterator(pSlave));
	return pSlave;
}
DBTObject* DBTMasterIterator::GetNextDbtObject(DBTIterator& iterator)
{
	DBTMaster* pMaster = (DBTMaster*)m_pDBT;
	if (m_nIndex > pMaster->m_pDBTSlaves->GetUpperBound())
		return NULL;//ho finito

	DBTObject* pObject = pMaster->m_pDBTSlaves->GetAt(m_nIndex++);
	if (pObject->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		iterator.Push(new DBTSlaveBufferedIterator(pObject));
	
	return pObject;
}