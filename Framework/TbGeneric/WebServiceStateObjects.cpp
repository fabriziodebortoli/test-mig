
#include "StdAfx.h"

#include <TBNamesolver\LoginContext.h>
#include <TBNamesolver\ThreadContext.h>

#include ".\webservicestateobjects.h"
#include ".\generalfunctions.h"


/////////////////////////////////////////////////////////////////////////////////////////////
/////////				GLOBAL FUNCTIONS											/////////
/////////////////////////////////////////////////////////////////////////////////////////////
//-------------------------------------------------------------------------------------------
CWebServiceStateObjectsPtr AfxGetWebServiceStateObjects(BOOL bForWriting /*= FALSE*/)
{
	CWebServiceStateObjects* pObjects = AfxGetLoginContext()->GetObject<CWebServiceStateObjects>(&CLoginContext::GetWebServiceStateObjects);
	return CWebServiceStateObjectsPtr(pObjects, bForWriting);
}
//-------------------------------------------------------------------------------------------
BOOL AfxExistWebServiceStateObject(CObject* pObject)
{
	 return AfxGetApplicationContext()->ExistWebServiceStateObject(pObject);	// another thread running
}

//-------------------------------------------------------------------------------------------
void AfxAddWebServiceStateObject (CObject* pObject, HWND hwndThreadWindow)
{
	AfxGetApplicationContext()->AddWebServiceStateObject(pObject, hwndThreadWindow);
}
//-------------------------------------------------------------------------------------------
void AfxAddWebServiceStateObject(CObject* pObject)
{
	//vero gestore del ciclo di vita dell'oggetto (memorizzato nel login context)
	AfxGetWebServiceStateObjects(TRUE)->AddObject(pObject);
	
	//usato per recuperare il thread associato all'oggetto (deve essere nell'application context
	//perche' finche non so il thread, non conosco il login context
	AfxGetApplicationContext()->AddWebServiceStateObject(pObject, GetThreadMainWnd());
	
	//usato come reference counter, cosi' il thread puo pulire i propri oggetti in uscita
	AfxGetThreadContext()->AddWebServiceStateObject(pObject);
}

//-------------------------------------------------------------------------------------------
void AfxRemoveWebServiceStateObject(CObject* pObject, BOOL bDelete /*= FALSE*/)
{
	AfxGetApplicationContext()->RemoveWebServiceStateObject(pObject);
	AfxGetWebServiceStateObjects(TRUE)->RemoveObject(pObject, bDelete);
	AfxGetThreadContext()->RemoveWebServiceStateObject(pObject);
}

//-------------------------------------------------------------------------------------------

void AfxGetWebServiceStateObjectsHandles(LongArray& handles, CRuntimeClass* pRuntimeClass /*= NULL*/)
{
	AfxGetWebServiceStateObjects()->GetObjectHandles(pRuntimeClass, handles);
}

//-------------------------------------------------------------------------------------------
BOOL AfxGetWebServiceStateObjectThreadWnd (CObject* pObject, HWND& hwnd)
{
	//prima verifico che l'oggetto sia presente fra quelli registrati
	hwnd = AfxGetApplicationContext()->GetWebServiceStateObjectWnd(pObject);
	if (!hwnd)
		return FALSE;

	//quindi, nel caso si tratti di un oggetto che lo prevede, chiedo all'oggetto a quale finestra 'appartiene'
	if (pObject->IsKindOf(RUNTIME_CLASS(CWebServiceStateObject)))
		hwnd = ((CWebServiceStateObject*)pObject)->GetThreadMainWnd();
	
	//potrebbe essere restituito NULL (allora la chiamata va fatta sul thread corrente)
	//oppure un handle di finestra (in tal caso, va controllato che sia buono)
	if (hwnd && !::IsWindow(hwnd))
		return FALSE;

	return TRUE;
}

IMPLEMENT_DYNAMIC(CWebServiceStateObject, CObject)
/////////////////////////////////////////////////////////////////////////////////////////////
/////////				CWebServiceStateObjects										/////////
/////////////////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CWebServiceStateObjects, CObject);

//-------------------------------------------------------------------------------------------
CWebServiceStateObjects::~CWebServiceStateObjects()
{
	Dispose();
}

//-------------------------------------------------------------------------------------------
void CWebServiceStateObjects::Dispose()
{
	POSITION pos = m_Objects.GetStartPosition();

	while (pos)
	{
		CObject* pKey = NULL;
		DWORD dwId = 0;
		m_Objects.GetNextAssoc(pos, pKey, dwId);
		delete pKey;
	}

	m_Objects.RemoveAll();
}

//-------------------------------------------------------------------------------------------
void CWebServiceStateObjects::GetObjects(CRuntimeClass* pRuntimeClass, CObArray& arObjects)
{
	POSITION pos = m_Objects.GetStartPosition();
	while (pos)
	{
		CObject* pKey = NULL;
		DWORD dwId = 0;
		m_Objects.GetNextAssoc(pos, pKey, dwId);
		if (!pKey) continue;

		if (pRuntimeClass && !pKey->IsKindOf(pRuntimeClass))
			continue;

		arObjects.Add(pKey);
	}

}
//-------------------------------------------------------------------------------------------
void CWebServiceStateObjects::GetObjectHandles(CRuntimeClass* pRuntimeClass, LongArray& handles)
{
	handles.RemoveAll();
	
	CObArray arObjects;
	GetObjects(pRuntimeClass, arObjects);
	for (int i = 0; i < arObjects.GetCount(); i++)
		handles.Add((long)arObjects[i]);
}

//-------------------------------------------------------------------------------------------
DWORD CWebServiceStateObjects::GetThreadId (CObject* pObject)
{
	DWORD dwId = 0;
	m_Objects.Lookup(pObject, dwId);
	return dwId;
}

//-------------------------------------------------------------------------------------------
BOOL CWebServiceStateObjects::ExistObject(CObject* pObject)
{
	DWORD dwId = 0;
	return m_Objects.Lookup(pObject, dwId);
}

//-------------------------------------------------------------------------------------------
void CWebServiceStateObjects::AddObject(CObject* pObject)
{
	ASSERT_TRACE(pObject,"parameter pObject cannot be null");
	
	m_Objects[pObject] = GetCurrentThreadId();
}

//-------------------------------------------------------------------------------------------
void CWebServiceStateObjects::RemoveObject(CObject* pObject, BOOL bDelete /*= FALSE*/)
{
	ASSERT_TRACE(pObject,"parameter pObject cannot be null");
	if (m_Objects.RemoveKey(pObject) && bDelete)
		delete pObject;
}