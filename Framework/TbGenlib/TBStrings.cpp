
#include "StdAfx.h"

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\TbStrings.h>

#include "messages.h"
#include "tbstrings.h"
#include "baseapp.h"
#include "basedoc.h"

#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//****************************************************************
//*****************       CDictionaryPathFinder **********************
//****************************************************************
//-----------------------------------------------------------------------------
void CDictionaryPathFinder::GetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths)
{
	AddOnModsArray arModules;
	AfxGetAddOnModules(hDllInstance, arModules);
	for (int i = 0; i < arModules.GetCount(); i++)
	{
		CString sPath = m_pPathFinder->GetModuleDictionaryFilePath(arModules.GetAt(i)->m_Namespace, FALSE, AfxGetCulture());			
		if (!sPath.IsEmpty())
			paths.Add(sPath);
	}
}

//-----------------------------------------------------------------------------
void CDictionaryPathFinder::GetModulePathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths)
{
	AddOnModsArray arModules;
	AfxGetAddOnModules(hDllInstance, arModules);
	for (int i = 0; i < arModules.GetCount(); i++)
	{
		CString sPath = m_pPathFinder->GetModulePath(arModules.GetAt(i)->m_Namespace, CPathFinder::STANDARD);			
		if (!sPath.IsEmpty())
			paths.Add(sPath);
	}
}

//-----------------------------------------------------------------------------
void CDictionaryPathFinder::GetDictionaryPathsFromString(LPCTSTR lpcszString, CStringArray &paths)
{
	MEMORY_BASIC_INFORMATION mbi;
	VirtualQuery(lpcszString, &mbi, sizeof(mbi));
    HINSTANCE hDllInstance = (HINSTANCE)mbi.AllocationBase;

	GetDictionaryPathsFormDllInstance(hDllInstance, paths);
}



//-----------------------------------------------------------------------------
void CDictionaryPathFinder::GetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths)
{		
	HINSTANCE	hDllInstance = (lpszType == RT_STRING)
						? AfxFindStringResourceHandle(nIDD)			
						: AfxFindResourceHandle(MAKEINTRESOURCE (nIDD), lpszType);

	GetDictionaryPathsFormDllInstance(hDllInstance, paths);
}

//-----------------------------------------------------------------------------
CString	CDictionaryPathFinder::GetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard)
{
	return m_pPathFinder->GetModuleDictionaryFilePath(aNamespace, bStandard, AfxGetCulture());
}

//-----------------------------------------------------------------------------
CString	CDictionaryPathFinder::GetDictionaryPathFromTableName(const CString& strTableName)
{
	const CDbObjectDescription* pDescription = AfxGetDbObjectDescription(strTableName);
	if (!pDescription)
		return _T("");

	return GetDictionaryPathFromNamespace(pDescription->GetNamespace(), TRUE);
}

//-----------------------------------------------------------------------------
CString	CDictionaryPathFinder::GetDllNameFromNamespace(const CTBNamespace& aNamespace)
{
	AfxGetAddOnLibrary(aNamespace);

	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNamespace);
	if (!pAddOnMod)
		return _T("");

	return pAddOnMod->ResolveLibrary(aNamespace);
}

//****************************************************************
//*****************       GLOBAL FUNCTIONS  **********************
//****************************************************************
//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, CBaseDocument* pDoc)
{
	if (!pDoc || !lpcszBaseString || !lpcszBaseString[0])
		return lpcszBaseString;
	
	CString sCulture = AfxGetThreadContext()->SetUICulture(pDoc->GetUICulture());
	
	CString sTrad =  AfxLoadReportString(lpcszBaseString, ::GetName (pDoc->GetPathName()), pDoc->GetDictionaryPath(TRUE));
	
	AfxGetThreadContext()->SetUICulture(sCulture);
	
	return sTrad;
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath)
{
	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
	{
		return pStrLoader->LoadReportString (lpcszBaseString, lpcstrFileName, lpcstrDictionaryPath);
	}
	
	return lpcszBaseString;
}

//-----------------------------------------------------------------------------
CString	AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, LPCTSTR lpcstrReportPath)
{
	CString strFileName = ::GetName(lpcstrReportPath);
	CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(lpcstrReportPath);
	CString strDictionaryPath = AfxGetDictionaryPathFromNamespace(ns, TRUE);
	return AfxLoadReportString(lpcszBaseString, strFileName, strDictionaryPath);
}

//-----------------------------------------------------------------------------
#define GUID_COL_NAME		"TBGuid"
#define	CREATED_COL_NAME	"TBCreated"
#define MODIFIED_COL_NAME	"TBModified"

CString	AfxLoadDatabaseString(LPCTSTR lpcszBaseString, LPCTSTR lpszTableName)
{
	if (_tcsicmp(lpcszBaseString, _T(GUID_COL_NAME)) == 0)
		return _TB(GUID_COL_NAME);
	if (_tcsicmp(lpcszBaseString, _T(CREATED_COL_NAME)) == 0)
		return _TB(CREATED_COL_NAME);
	if (_tcsicmp(lpcszBaseString, _T(MODIFIED_COL_NAME)) == 0)
		return _TB(MODIFIED_COL_NAME);

	CStringLoader *pStrLoader = AfxGetStringLoader();  
	if (pStrLoader)
	{
		 CString sTarget = pStrLoader->LoadDatabaseString
								(
									lpcszBaseString, 
									lpszTableName, 
									AfxGetDictionaryPathFromTableName(lpszTableName) 
								);

		if (sTarget != lpcszBaseString)		 
			return sTarget;

		if (_tcsicmp(lpcszBaseString, lpszTableName) == 0)
			return lpcszBaseString;

		const CDbObjectDescription* pDBDescr = AfxGetDbObjectDescription(lpszTableName); 
		if (pDBDescr == NULL)
			return lpcszBaseString;

		const CAddColsTableDescription* pDescri =  AfxGetAddOnFieldsOnTable(pDBDescr->GetNamespace());
		if (pDescri)
		{
			for (int i=0; i < pDescri->m_arAlterTables.GetSize(); i++)
			{
				const CTBNamespace& aLibNs = pDescri->m_arAlterTables.GetAt(i)->GetNamespace();
				CString sDictionaryPath = AfxGetDictionaryPathFromNamespace(aLibNs, TRUE);
				sTarget = pStrLoader->LoadDatabaseString
								(
									lpcszBaseString, 
									lpszTableName, 
									sDictionaryPath
								);
				if (sTarget != lpcszBaseString)		 
					return sTarget;
			}
		}
	}
	return lpcszBaseString;
}
static const BOOL bInited = AfxInitLoadDatabaseStringFunction(&AfxLoadDatabaseString);