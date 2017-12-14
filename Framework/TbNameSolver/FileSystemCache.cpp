#include "stdafx.h"

#include "chars.h"
#include "FileSystemCache.h"
#include "PathFinder.h"
#include "FileSystemFunctions.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//	TODOBRUNA cose da sistemare (su metodi non usati):
//			CreateFolder in ricorsione
//			RemoveFolder ricorsione su folder e file
///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemCachedItem implementation
///////////////////////////////////////////////////////////////////////////////
//
// never locked as the class in managed and used by CFileSystemCache when 
// lock has already been taken
//-----------------------------------------------------------------------------
CFileSystemCachedItem::CFileSystemCachedItem ()
{
	m_sPathName.Empty ();
	m_sFileName.Empty ();
}

//-----------------------------------------------------------------------------
CFileSystemCachedItem::CFileSystemCachedItem(
												const CString& sPathName, 
												const CString& sFileName /*_T("")*/ 
											)
{
	Set (sPathName, sFileName);
}

//-----------------------------------------------------------------------------
void CFileSystemCachedItem::Set (const CString& sPathName, const CString& sFileName)
{
	m_sPathName = sPathName;
	m_sFileName = sFileName;
}

//-----------------------------------------------------------------------------
const CString& CFileSystemCachedItem::GetPathName () const 
{ 
	return  m_sPathName; 
}

//-----------------------------------------------------------------------------
const CString& CFileSystemCachedItem::GetFileName () const 
{ 
	return  m_sFileName; 
}

//-----------------------------------------------------------------------------
const CString CFileSystemCachedItem::GetFullName () const 
{ 
	return  m_sPathName	+ 
			(
				m_sFileName.IsEmpty() ? 
				_T("") : 
				SLASH_CHAR + m_sFileName
			); 
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCachedItem::IsAFolder () const
{
	return m_sFileName.IsEmpty ();
}

#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CFileSystemCachedItem::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CFileSystemCachedItem" + this->GetFullName());
}
#endif //_DEBUG

///////////////////////////////////////////////////////////////////////////////
// 						CFileSystemCache
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CFileSystemCache::CFileSystemCache()
{
	m_sRemovedPath = AfxGetPathFinder()->GetInstallationPath();
}

//-----------------------------------------------------------------------------
CFileSystemCache::CFileSystemCache (const CString& sCachedPath)
{
	m_sRemovedPath = AfxGetPathFinder()->GetInstallationPath();
	m_sCachedPath = sCachedPath;
}

//-----------------------------------------------------------------------------
CFileSystemCache::~CFileSystemCache()
{
	RemoveAll();
}

//----------------------------------------------------------------------------
void CFileSystemCache::RemoveAll()
{
	TB_LOCK_FOR_WRITE();

	POSITION startPos;
	CString sKey;
	for (startPos = m_Files.GetStartPosition(); startPos != NULL;)
	{
		CFileSystemCachedItem* pItem = NULL;
		m_Files.GetNextAssoc (startPos, sKey, (CObject*&) pItem);
		if (pItem)
			delete pItem;
	}	
	m_Files.RemoveAll ();
	
	for (startPos = m_Folders.GetStartPosition(); startPos != NULL;)
	{
		CFileSystemCachedItem* pItem = NULL;
		m_Folders.GetNextAssoc (startPos, sKey, (CObject*&) pItem);
		if (pItem)
			delete pItem;
	}	

	m_Folders.RemoveAll ();
}

//----------------------------------------------------------------------------
const BOOL CFileSystemCache::IsCachedPath	(const CString& sFullName) const
{
	// no lock is needed as it is used only when lock has already been taken
	CString sFullCachedPath = m_sRemovedPath + m_sCachedPath;
	CString sStartingFullPath = sFullName.Left (sFullCachedPath.GetLength ());
	
	return sFullCachedPath.CompareNoCase(sStartingFullPath) == 0;
}

// it removes cached path from full path only if it matches
//----------------------------------------------------------------------------
const CString CFileSystemCache::RemoveCachedPath (const CString& sFullName) const
{
	// no lock is needed as it is used only when lock has already been taken
	CString sFullCachedPath = m_sRemovedPath;
	if (!m_sCachedPath.IsEmpty())
		sFullCachedPath +=  m_sCachedPath;

	// it is not the same path
	if (sFullCachedPath.CompareNoCase(sFullName.Left(sFullCachedPath.GetLength ())) != 0)
		return sFullName;
	
	CString sRelativePath = sFullName.Mid (sFullCachedPath.GetLength());

	if (!sRelativePath.IsEmpty() && sRelativePath[sRelativePath.GetLength ()-1] == SLASH_CHAR)
		sRelativePath = sRelativePath.Left (sRelativePath.GetLength ()-1);

	return sRelativePath;
}

//----------------------------------------------------------------------------
void CFileSystemCache::Add (CFileSystemCachedItem* pItem)
{
	if (!pItem)
		return;

	TB_LOCK_FOR_WRITE();

	// removing cached path
	pItem->Set (RemoveCachedPath(pItem->GetPathName()), pItem->GetFileName());

	CString sKey (pItem->GetFullName());

	if (pItem->GetFileName().IsEmpty ())
		m_Folders.SetAt (sKey.MakeLower(), pItem);
	else
		m_Files.SetAt (sKey.MakeLower(), pItem);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::ExistPath (const CString& sPathName)
{
	TB_LOCK_FOR_READ();

	CString sPathToSearch = RemoveCachedPath(sPathName);

	CFileSystemCachedItem* pItem = NULL;
	m_Folders.Lookup(sPathToSearch.MakeLower(), ( CObject*& ) pItem);

	return pItem != NULL;
 }

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::ExistFile (const CString& sPathName, const CString& sFileName)
{
	TB_LOCK_FOR_READ();

	CString sPathToSearch = RemoveCachedPath(sPathName) + SLASH_CHAR + sFileName;

	CFileSystemCachedItem* pItem = NULL;
	m_Files.Lookup(sPathToSearch.MakeLower(), (CObject*&) pItem);

	return pItem != NULL;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::RemoveFile (const CString& sFileName)
{
	TB_LOCK_FOR_WRITE();

	CString sPathToSearch = RemoveCachedPath(sFileName);

	return m_Files.RemoveKey (sPathToSearch.MakeLower());
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::CreateFolder	(const CString& sPathName, const BOOL& bRecursive)
{
	// no lock as olverload
	Add (new CFileSystemCachedItem(sPathName));
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::RemoveFolder	(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents)
{
	TB_LOCK_FOR_WRITE();

	CString sPathToSearch = RemoveCachedPath(sPathName);

	return m_Files.RemoveKey (sPathToSearch.MakeLower ());
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::GetSubFolders (const CString& sPathName, CStringArray* pSubFolders)
{
	TB_LOCK_FOR_READ();

	if (!m_Folders.GetSize())
		return TRUE;

	CString sRelativePath = RemoveCachedPath (sPathName);
	CString sFoundPath;
	POSITION startPos;
	
	for (startPos = m_Folders.GetStartPosition(); startPos != NULL;)
	{
		CFileSystemCachedItem* pItem = NULL;
		m_Folders.GetNextAssoc (startPos, sFoundPath, (CObject*&) pItem);

		if (!pItem)
			continue;

		sFoundPath = pItem->GetPathName();

		if (sFoundPath.GetLength () < sRelativePath.GetLength())
			continue;

		int nExpectedSlash	= sRelativePath.GetLength();
		int nLastSlash		= sFoundPath.ReverseFind (SLASH_CHAR);
		
		if (nLastSlash != nExpectedSlash)
			continue;

		// the same parent
		if	(
				sFoundPath.GetLength () == sRelativePath.GetLength() &&
				sFoundPath.Left (sRelativePath.GetLength()).CompareNoCase (sRelativePath) == 0 &&
				pItem->IsAFolder ()
			)
		{
			pSubFolders->RemoveAll ();
			return FALSE;
		}


		if (sFoundPath.Left (sRelativePath.GetLength()).CompareNoCase(sRelativePath) != 0)
			continue;
		
		sFoundPath = sFoundPath.Mid (sRelativePath.GetLength());

		if (!sFoundPath.IsEmpty() && sFoundPath.Right (1) != SLASH_CHAR)
		{
			if (sFoundPath.Left (1) == SLASH_CHAR)
				sFoundPath = sFoundPath.Mid (1);
			
			pSubFolders->Add (sFoundPath);
		}
	}
	
	return pSubFolders->GetSize();
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCache::GetFiles	(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	TB_LOCK_FOR_READ();

	if (!m_Files.GetSize())
		return TRUE;

	CString sRelativePath = RemoveCachedPath (sPathName);
	CString sFoundPath;

	POSITION startPos;
	for (startPos = m_Files.GetStartPosition(); startPos != NULL;)
	{
		CFileSystemCachedItem* pItem = NULL;
		m_Files.GetNextAssoc (startPos, sFoundPath, (CObject*&) pItem);

		if (!pItem)
			continue;

		sFoundPath = pItem->GetPathName();
		if (sFoundPath.GetLength () < sRelativePath.GetLength())
			continue;

		if (sFoundPath.Left (sRelativePath.GetLength()).CompareNoCase (sRelativePath) != 0)
			continue;
		
		sFoundPath = sFoundPath.Mid (sRelativePath.GetLength());

		if (sFoundPath.IsEmpty() && !pItem->IsAFolder())
			pFiles->Add (sPathName + SLASH_CHAR + pItem->GetFileName());
	}

	return pFiles->GetSize();

}

#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CFileSystemCache::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CFileSystemCache " + this->m_sCachedPath);
}
#endif //_DEBUG


///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemCacher implementation
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CFileSystemCacher::CFileSystemCacher()
	:
	m_bEnabled (TRUE)
{
}

//-----------------------------------------------------------------------------
CFileSystemCacher::~CFileSystemCacher()
{
	ClearCaches ();
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::IsEnabled () const
{
	// no lock required as setted only during InitInstance
	return m_bEnabled;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::AreCachesLoaded () const
{
	// no lock required as m_Cashes values are loaded 
	// only in InitInstance and they never change
	return m_Caches.GetSize ();
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::IsAManagedObject (const CString& sFileName) const
{
	// no lock is required as both m_bEnabled and m_Cashes values are
	// loaded only in InitInstance and they never change. Lock on PathFinder
	// object is not needed as the methods invoke donn't need lock.

	// caching is disabled
	if (!m_bEnabled || !AreCachesLoaded ())
		return FALSE;

	// client installation object
	if (sFileName.Find (AfxGetPathFinder()->GetTBDllPath(), 0) >= 0)
		return FALSE;

	// custom installation object
	if (sFileName.Find (AfxGetPathFinder()->GetCustomPath(), 0) >= 0)
		return FALSE;

	// server connection.config object
	if (sFileName.CompareNoCase (AfxGetPathFinder ()->GetServerConnectionConfigFullName()) == 0)
		return FALSE;

	return (sFileName.Find (AfxGetPathFinder()->GetStandardPath(), 0) >= 0);
}

//-----------------------------------------------------------------------------
void CFileSystemCacher::ClearCaches()
{
	// no lock is requireed as it is performed on application start and exit 
	for (int i = m_Caches.GetUpperBound(); i >= 0; i--) 
	{
		CFileSystemCache* pO;
		if (pO = (CFileSystemCache*) m_Caches.GetAt(i)) 
		{
			ASSERT_VALID(pO);
			delete pO;
			m_Caches.RemoveAt (i);
		}
	}
}

//-----------------------------------------------------------------------------
CFileSystemCache* CFileSystemCacher::GetCacheInvolved (const CString& sName) const
{
	// no lock is requireed as the content of the m_Caches array never change
	// It is loaded on InitInstance and lock on operation is performed on 
	// CFileSystemCache object, that contains dynamic maps
	if (sName.IsEmpty ())
		return NULL;

	for (int i=0; i <= m_Caches.GetUpperBound (); i++)
	{
		CFileSystemCache* pCache = (CFileSystemCache*) m_Caches.GetAt(i);
		if (pCache->IsCachedPath (sName))
			return pCache;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
void CFileSystemCacher::AddCache (const CString& sPathName)
{
	// no lock is required as performed only in InitInstance
	CFileSystemCache* pCache = new CFileSystemCache();
	pCache->m_sCachedPath = pCache->RemoveCachedPath (sPathName);
	m_Caches.Add (pCache);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::AddInCache (const CString& sPathName, const CString& sFileName)
{
	TB_LOCK_FOR_WRITE();

	CFileSystemCache* pCache = GetCacheInvolved(sPathName);

	if (!pCache)
		return FALSE;
	
	pCache->Add (new CFileSystemCachedItem (sPathName, sFileName));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::AddInCache (const CString& sPathName)
{
	// no lock is required as overload 
	return AddInCache (sPathName, _T(""));
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::ExistPath (const CString& sPathName)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sPathName);
	return  pCache && pCache->ExistPath (sPathName);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::ExistFile (const CString& sFileName)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sFileName);
	if (pCache)
	{
		CString sPath = GetPath	(sFileName);
		// I have to preserve multiple extensions
		CString sFile = GetNameWithExtension(sFileName); 
		return pCache->ExistFile (sPath, sFile);
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::GetSubFolders (const CString& sPathName, CStringArray* pSubFolders)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sPathName);
	return  pCache && pCache->GetSubFolders (sPathName, pSubFolders);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::GetFiles (const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sPathName);
	return  pCache && pCache->GetFiles (sPathName, sFileExt, pFiles);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::RemoveFile (const CString& sFileName)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sFileName);
	return  pCache && pCache->RemoveFile (sFileName);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::RenameFile (const CString& sOldFileName, const CString& sNewFileName)
{
	// no lock is required as demanded to the single methods if needed	// caching
	BOOL bOk = TRUE;
	CFileSystemCache* pCache = NULL;
	if (IsAManagedObject (sOldFileName))
	{
		CFileSystemCache* pCache = GetCacheInvolved(sOldFileName);
		bOk = pCache && pCache->RemoveFile (sOldFileName);
	}

	if (IsAManagedObject (sNewFileName))
	{
		CString sPath = GetPath	(sNewFileName);
		// I have to preserve multiple extensions
		CString sFile = GetNameWithExtension(sNewFileName);

		bOk = AddInCache (sPath, sFile);
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::CreateFolder (const CString& sPathName, const BOOL& bRecursive)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sPathName);
	return pCache && pCache->CreateFolder (sPathName, bRecursive);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacher::RemoveFolder (const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents)
{
	// no lock is required as demanded to the single methods if needed
	CFileSystemCache* pCache = GetCacheInvolved(sPathName);
	return pCache && pCache->RemoveFolder (sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);
}

#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CFileSystemCacher::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CFileSystemCacher");
}
#endif //_DEBUG
