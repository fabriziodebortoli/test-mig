
#include "stdafx.h"

#include <TbGeneric\globals.h>
#include <TbGeneric\array.h>
#include <TbNameSolver\PathFinder.h>

//#include <TbOledb\sqltable.h>

#include "baseeventmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//============================================================================

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// MappedFunction
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(MappedFunction, CObject);


/////////////////////////////////////////////////////////////////////////////
//				CBaseEventManager Declaration
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CBaseEventManager, CObject)

//---------------------------------------------------------------------------
CBaseEventManager::CBaseEventManager()
	:
	IDisposingSourceImpl(this),
	m_pParentEventMng(NULL)
{
}

//---------------------------------------------------------------------------
CBaseEventManager::~CBaseEventManager()
{
	MappedFunction *pFunc = NULL;
	POSITION pos = m_FunctionList.GetHeadPosition();
	while (pos)
	{
		pFunc = (MappedFunction*)m_FunctionList.GetAt(pos);
		if (pFunc)
			delete pFunc;
		m_FunctionList.GetNext(pos);
	}

	m_FunctionList.RemoveAll();

	SAFE_DELETE(m_pParentEventMng);
}

//---------------------------------------------------------------------------
///scorre la lista delle funzioni e restituisce quella cercata
MappedFunction* CBaseEventManager::GetFunctionPointer(const CString& className, const CString& funcName)
{
	MappedFunction* pFunc = NULL;
	POSITION pos = m_FunctionList.GetHeadPosition();
	while (pos)
	{
		pFunc = (MappedFunction*)m_FunctionList.GetAt(pos);
		if (pFunc &&
			pFunc->m_className == className &&
			pFunc->m_FuncName == funcName)
			return pFunc;
		m_FunctionList.GetNext(pos);
	}

	return NULL;
}

//---------------------------------------------------------------------------
MappedFunction* CBaseEventManager::GetFunctionPointer(const CString& funcName, CObject** pClass, const CString& strClassName /*= ""*/)
{
	MappedFunction* ptr = NULL;

	if ((ptr = SearchFunctionInEventManager(funcName, pClass)) && IsValidClass(pClass, strClassName))
		return ptr;

	return NULL;
}

//---------------------------------------------------------------------------
// cerca la funzione nell'event manager 
MappedFunction* CBaseEventManager::SearchFunctionInEventManager(const CString& funcName, CObject** pClass)
{
	USES_CONVERSION;
	MappedFunction* ptr = GetFunctionPointer(A2T((LPSTR)GetRuntimeClass()->m_lpszClassName), funcName);

	// se per caso non lo ha trovato, cerca prima nelle classi EventManager da cui deriva 
	// per inheritance C++
	CRuntimeClass* pRuntimeClass = GetRuntimeClass();
	ASSERT(pRuntimeClass);
	while (!ptr && pRuntimeClass)
	{
		ptr = GetFunctionPointer(A2T((LPSTR)pRuntimeClass->m_lpszClassName), funcName);
		if (!pRuntimeClass->m_pfnGetBaseClass)
			break;
		pRuntimeClass = (*pRuntimeClass->m_pfnGetBaseClass)();
	}

	if (ptr)
	{
		*pClass = this;
		return ptr;
	}

	// se non l'ha trovato, creca negli event manager "collaterali" (derivazione per contenimento)
	return m_pParentEventMng ? m_pParentEventMng->GetFunctionPointer(funcName, pClass) : NULL;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CBaseEventManager::FireAction(const CString& funcName, CString *pstrInputOutput)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer(funcName, &pObj);
	if (!pMappedFunc || !pObj || !pMappedFunc->m_strPtrFunc)
		currentResult = FUNCTION_NOT_FOUND;
	else
		currentResult = (pObj->*(pMappedFunc->m_strPtrFunc))(pstrInputOutput);
	if (currentResult < result) result = currentResult;

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CBaseEventManager::FireAction(const CString& funcName, void* pVoidInputOutput)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer(funcName, &pObj);
	if (!pMappedFunc || !pObj || !pMappedFunc->m_voidPtrFunc)
		currentResult = FUNCTION_NOT_FOUND;
	else
		currentResult = (pObj->*(pMappedFunc->m_voidPtrFunc))(pVoidInputOutput);
	if (currentResult < result) result = currentResult;

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CBaseEventManager::FireAction(const CString& funcName)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer(funcName, &pObj);
	if (!pMappedFunc || !pObj || !pMappedFunc->m_voidFunc)
		currentResult = FUNCTION_NOT_FOUND;
	else
	{
		(pObj->*(pMappedFunc->m_voidFunc))();
		currentResult = FUNCTION_OK;
	}

	if (currentResult < result) result = currentResult;

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CBaseEventManager::FireAction(const CString& funcName, CFunctionDescription* pRDI)
{
	int result = FUNCTION_NOT_FOUND, currentResult = FUNCTION_NOT_FOUND;

	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer(funcName, &pObj);
	if (!pMappedFunc || !pObj)
		currentResult = FUNCTION_NOT_FOUND;
	else if (pMappedFunc->m_TBFunc)
	{
		currentResult =
			(pObj->*(pMappedFunc->m_TBFunc))(pRDI)
			? FUNCTION_OK
			: FUNCTION_NOT_FOUND;

		if (currentResult == FUNCTION_OK)
			return FUNCTION_OK;
	}
	else if (pMappedFunc->m_voidBoolFunc)
	{
		BOOL b = (pObj->*(pMappedFunc->m_voidBoolFunc))();
		pRDI->SetReturnValue(DataBool(b));
		return FUNCTION_OK;
	}
	if (currentResult < result)
		result = currentResult;

	return result;
}

//---------------------------------------------------------------------------
BOOL CBaseEventManager::ExistAction(const CString& funcName, MappedFunction** ppMappedFunction /*= NULL*/, CObject** ppObj /*= NULL*/)
{
	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer(funcName, &pObj);
	if (!pMappedFunc || !pObj)
	{
		return FALSE;
	}
	if (ppMappedFunction)
		*ppMappedFunction = pMappedFunc;
	if (ppObj)
		*ppObj = pObj;

	return TRUE;
}

//---------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBaseEventManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CBaseEventManager\n");
}

void CBaseEventManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG



