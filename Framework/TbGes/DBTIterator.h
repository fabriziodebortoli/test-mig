#pragma once
#include "dbt.h"
//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================
class DBTIteratorObj
{
public:
	DBTIteratorObj* m_pParent;

	int m_nIndex;
	DBTObject* m_pDBT;
public:
	DBTIteratorObj(DBTObject* pDBT) 
		: 
		m_pDBT(pDBT),
		m_nIndex(0)
	{
	}

	virtual DBTObject* GetNextDbtObject(DBTIterator& iterator) = 0;
};

//=============================================================================
class DBTSlaveBufferedIterator : public DBTIteratorObj
{
	int m_nIndex;
public:
	DBTSlaveBufferedIterator(DBTObject* pDBT) 
		: DBTIteratorObj(pDBT), m_nIndex(0)
	{
	}
	DBTObject* GetNextDbtObject(DBTIterator& iterator);
};
//=============================================================================
class DBTSlaveDataIterator : public DBTIteratorObj
{
	POSITION m_nPos;
	DBTSlaveMap* m_pData;
public:
	DBTSlaveDataIterator(DBTObject* pDBT, DBTSlaveMap* pData) 
		: DBTIteratorObj(pDBT), m_nPos(pData->m_Slaves.GetStartPosition()), m_pData(pData)
	{
	}
	DBTObject* GetNextDbtObject(DBTIterator& iterator);
};
//=============================================================================
class DBTMasterIterator : public DBTIteratorObj
{
public:
	DBTMasterIterator(DBTObject* pDBT) 
		: DBTIteratorObj(pDBT)
	{
	}
	DBTObject* GetNextDbtObject(DBTIterator& iterator);
};
//=============================================================================
class DBTIterator
{
	int m_nLevel;
public:
	DBTIteratorObj* m_pCurrent;
	DBTIterator(DBTMaster* pRoot) : m_nLevel(0)
	{
		m_pCurrent = new DBTMasterIterator(pRoot);
	}
	~DBTIterator()
	{
		ASSERT(m_nLevel==0);
		delete m_pCurrent;
	}
	DBTObject* GetNextDbtObject()
	{
		return m_pCurrent->GetNextDbtObject(*this);

	}
	void Pop();
	void Push(DBTIteratorObj* pNew);

};
//includere alla fine degli include del .H
#include "endh.dex"