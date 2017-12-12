#pragma once

// CGenericContext command target
#include "TBResourceLocker.h"

#include "beginh.dex"


typedef CMap<CRuntimeClass*, CRuntimeClass*, CObject*, CObject*> MapRTCToObject;

MapRTCToObject* GetProxyObjectMap();

//*****************************************************************************
template<class TDerived> 
class CGenericContext
{
protected:
	DECLARE_LOCKABLE(MapRTCToObject, m_ObjectMap);

public:
	typedef CObject* (TDerived::*GetMethod)();
	virtual CObject* InternalGetObject(GetMethod getMethod) = 0;
	virtual BOOL IsValid() = 0;
	virtual BOOL IsSingleton() const { return TRUE; }

	//----------------------------------------------------------------------------
	template<class T> T* GetObject(GetMethod getMethod)
	{
		CObject *pObject = InternalGetObject(getMethod);
		if (pObject == NULL)
			return NULL;
#ifdef DEBUG
		if (!TestRuntimeClass(pObject, RUNTIME_CLASS(T)))
			return NULL;
#endif
		return (T*) pObject;
	}

#ifdef DEBUG
	//----------------------------------------------------------------------------
	BOOL TestRuntimeClass(const CObject* pObject, CRuntimeClass* pRTM)
	{
		if (pObject->IsKindOf(pRTM))
			return TRUE;
		ASSERT(FALSE);
		USES_CONVERSION;
		const TCHAR* classA = A2T(pObject->GetRuntimeClass()->m_lpszClassName);
		const TCHAR* classB = A2T(pRTM->m_lpszClassName);
		TRACE2("Cannot cast object from type %s to type %s", classA, classB);
		return FALSE;
	}
#endif

	template<class T> T* GetObject(BOOL bCreate = TRUE)
	{
		return (T*) GetObject(RUNTIME_CLASS(T), bCreate);
	}

	//----------------------------------------------------------------------------
	CObject* GetObject(CRuntimeClass* pClass, BOOL bCreate = TRUE)
	{
		CObject *pObject = NULL;
		//MapRTCToObject* pContextMap =  NULL;

		//if (IsSingleton())
		//{
		//	//prima vedo se nella cache del thread corrente esiste l'oggetto (per evitare di fare lock inutili)
		//	pContextMap = GetProxyObjectMap();

		//	pObject = (*pContextMap)[pClass];

		//	if (pObject) 
		//		return pObject;
		//}

		//prima lock in lettura per vedere se l'oggetto e' nella mappa
		BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_ObjectMap);
			pObject = m_ObjectMap[pClass];
		END_TB_OBJECT_LOCK();
		//rilascio il lock, se l'oggeto non esiste verra` creato in assenza di lock
		//su questo oggetto (per evitare deadlock)
		if (pObject != NULL)
		{
#ifdef DEBUG
			if (!TestRuntimeClass(pObject, pClass))
				return NULL;
#endif		
			//if (IsSingleton())
			//	(*pContextMap)[pClass] = pObject;
			return pObject;
		}
		if (!bCreate)
			return NULL;

		//acquisisco un lock di scrittura per creare l'oggetto
		TB_OBJECT_LOCK(&m_ObjectMap);
		
		//se nel frattempo qualcuno ne ha gia` inserito un altro, restituisco quello
		pObject = m_ObjectMap[pClass];
		if (pObject)
			return pObject;
		
		//creo l'oggetto 
		pObject = pClass->CreateObject();
		if (pObject == NULL)
		{
			ASSERT(FALSE);
			USES_CONVERSION;
			TRACE1("Object not found: %s", A2T(pClass->m_lpszClassName));
			return NULL;
		}
		
		m_ObjectMap[pClass] = pObject;
		//if (IsSingleton())
		//	(*pContextMap)[pClass] = pObject;
		return pObject;
	}

protected:
	//----------------------------------------------------------------------------
	void FreeObjects()
	{
		POSITION pos = m_ObjectMap.GetStartPosition();
		while (pos)
		{
			CObject *pObject;
			CRuntimeClass* pKey;
			m_ObjectMap.GetNextAssoc(pos, pKey, pObject);
			try
			{
				delete pObject;
			}
			catch (...)
			{
			}
		}
	}

};


#include "endh.dex"