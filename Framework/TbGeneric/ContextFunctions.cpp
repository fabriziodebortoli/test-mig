#include "StdAfx.h"

#include <TbNameSolver\ThreadContext.h>

#include "ContextFunctions.h"

static TCHAR szUndefined[] = _T("");

//-----------------------------------------------------------------------------
BOOL AfxCheckContextObject(const CString& strName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return FALSE; 
	}

	return ::CheckContextObject(AfxGetThreadContextBag(), strName);
}

//-----------------------------------------------------------------------------
DataStr* AfxAddContextString(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL; 
	}

	return ::AddContextString(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataInt* AfxAddContextInt(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::AddContextInt(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataDate* AfxAddContextDate(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::AddContextDate(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataStr* AfxLookupContextString(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL; 
	}

	return ::LookupContextString(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataInt* AfxLookupContextInt(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::LookupContextInt(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataDate* AfxLookupContextDate(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return NULL;
	}

	return ::LookupContextDate(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataStr AfxReadContextString(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return szUndefined;
	}

	return ::ReadContextString(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataInt AfxReadContextInt(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return -1;
	}

	return ::ReadContextInt(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
DataDate AfxReadContextDate(const DataStr& aName)
{
	if (!AfxGetThreadContextBag())
	{
		ASSERT_TRACE(FALSE, _T("Thread context not available here!"));
		return DataDate::NULLDATE;
	}

	return ::ReadContextDate(AfxGetThreadContextBag(), aName);
}

//-----------------------------------------------------------------------------
BOOL CheckContextObject(CContextBag* pContextBag, const CString& strName)
{
	return pContextBag->LookupContextObject(strName) != NULL;
}

//-----------------------------------------------------------------------------
DataStr* AddContextString(CContextBag* pContextBag, const DataStr& aName)
{
	return (DataStr*)pContextBag->AddContextObject(aName, RUNTIME_CLASS(DataStr));
}

//-----------------------------------------------------------------------------
DataInt* AddContextInt(CContextBag* pContextBag, const DataStr& aName)
{
	return (DataInt*)pContextBag->AddContextObject(aName, RUNTIME_CLASS(DataInt));
}

//-----------------------------------------------------------------------------
DataDate* AddContextDate(CContextBag* pContextBag, const DataStr& aName)
{
	return (DataDate*)pContextBag->AddContextObject(aName, RUNTIME_CLASS(DataDate));
}

//-----------------------------------------------------------------------------
DataObj* AddContextDataObj(CContextBag* pContextBag, const DataStr& aName, CRuntimeClass* pClass, BOOL bAssertWhenExists)
{
	ASSERT(pClass);
	ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(DataObj)));

	return (DataObj*)pContextBag->AddContextObject(aName, pClass, bAssertWhenExists);
}

//-----------------------------------------------------------------------------
DataStr* LookupContextString(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pObject = pContextBag->LookupContextObject(aName);

	if (!pObject)
		return NULL;

	if (!pObject->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataStr", aName.FormatData());
		return NULL; 
	}

	return (DataStr*)pObject;
}

//-----------------------------------------------------------------------------
DataInt* LookupContextInt(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pObject = pContextBag->LookupContextObject(aName);

	if (!pObject)
		return NULL;

	if (!pObject->IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataInt", aName.FormatData());
		return NULL; 
	}

	return (DataInt*)pObject;
}

//-----------------------------------------------------------------------------
DataDate* LookupContextDate(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pObject = pContextBag->LookupContextObject(aName);

	if (!pObject)
		return NULL;

	if (!pObject->IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataDate", aName.FormatData());
		return NULL; 
	}

	return (DataDate*)pObject;
}

//-----------------------------------------------------------------------------
DataStr ReadContextString(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pObject = pContextBag->LookupContextObject(aName);

	if (!pObject)
		return szUndefined;

	if (!pObject->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataStr", aName.FormatData());
		return szUndefined; 
	}

	return *(DataStr*)pObject;
}

//-----------------------------------------------------------------------------
DataInt ReadContextInt(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pObject = pContextBag->LookupContextObject(aName);

	if (!pObject)
		return -1;

	if (!pObject->IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataInt", aName.FormatData());
		return -1; 
	}

	return *(DataInt*)pObject;
}

//-----------------------------------------------------------------------------
DataDate ReadContextDate(CContextBag* pContextBag, const DataStr& aName)
{
	CContextObject* pInfo = pContextBag->LookupContextObject(aName);

	if (!pInfo)
		return DataDate::NULLDATE;

	if (!pInfo->IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		ASSERT_TRACE1(FALSE,"Context Object %s is not a DataDate", aName.FormatData());
		return DataDate::NULLDATE; 
	}

	return *(DataDate*)pInfo;
}
