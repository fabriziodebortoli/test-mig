
#pragma once

#include <afxtempl.h>
#include <TBNameSolver\tbresourcelocker.h>

#include "dataobj.h"

#include "beginh.dex"

typedef CArray<long> LongArray;
class TB_EXPORT CWebServiceStateObject : public CObject
{
	DECLARE_DYNAMIC(CWebServiceStateObject)
public:
	virtual HWND GetThreadMainWnd() = 0;
};

class TB_EXPORT CWebServiceStateObjects : public CObject, public CTBLockable
{
	DECLARE_DYNAMIC(CWebServiceStateObjects);
	CMap<CObject*, CObject*, DWORD, DWORD> m_Objects;
public:
	~CWebServiceStateObjects	();
	virtual LPCSTR  GetObjectName() const { return "CWebServiceStateObjects"; }
	void	GetObjectHandles	(CRuntimeClass* pRuntimeClass, LongArray& handles);
	void	GetObjects			(CRuntimeClass* pRuntimeClass, CObArray& arObjects);
	BOOL	ExistObject			(CObject* pObject);
	void	AddObject			(CObject* pObject);
	void	RemoveObject		(CObject* pObject, BOOL bDelete = FALSE);
	DWORD	GetThreadId			(CObject* pObject);

	void	Dispose				();
};

DECLARE_SMART_LOCK_PTR(CWebServiceStateObjects);

TB_EXPORT BOOL AfxExistWebServiceStateObject	(CObject* pObject);
TB_EXPORT void AfxAddWebServiceStateObject		(CObject* pObject);
TB_EXPORT void AfxAddWebServiceStateObject		(CObject* pObject, HWND hwndThreadWindow);
TB_EXPORT void AfxRemoveWebServiceStateObject	(CObject* pObject, BOOL bDelete = FALSE);

TB_EXPORT void AfxGetWebServiceStateObjectsHandles(LongArray& handles, CRuntimeClass* pRuntimeClass = NULL);
TB_EXPORT CWebServiceStateObjectsPtr AfxGetWebServiceStateObjects(BOOL bForWriting = FALSE);
TB_EXPORT BOOL AfxGetWebServiceStateObjectThreadWnd	(CObject* pObject, HWND& hwnd);

#include "endh.dex"
