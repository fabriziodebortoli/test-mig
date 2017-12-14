#include "stdafx.h"

#include "IFileSystemDriver.h"
#include "IFileSystemManager.h"
#include "ApplicationContext.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//				Static Objects
///////////////////////////////////////////////////////////////////////////////
//


//-----------------------------------------------------------------------------
TB_EXPORT IFileSystemManager* AFXAPI AfxGetFileSystemManager()
{ 
	return AfxGetApplicationContext()->GetFileSystemManager(); 
}          

///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemOperation implementation
///////////////////////////////////////////////////////////////////////////////
// No lock on this object as lock is performed to the manager classes
//-----------------------------------------------------------------------------
CFileSystemOperation::CFileSystemOperation ()
	:
	m_lTime (0),
	m_nCalls(0)
{
	m_sName.Empty ();
}

//-----------------------------------------------------------------------------
CFileSystemOperation::CFileSystemOperation (const CString& sName)
	:
	m_lTime (0),
	m_nCalls(0)
{
	m_sName = sName;
}

//-----------------------------------------------------------------------------
void CFileSystemOperation::Set (const CString& sName, const long& lTime, const int& nCalls)
{
	m_sName		= sName;
	m_lTime		= lTime;
	m_nCalls	= nCalls;
}

//-----------------------------------------------------------------------------
const CString& CFileSystemOperation::GetName () const 
{ 
	return  m_sName; 
}

//-----------------------------------------------------------------------------
const long&	CFileSystemOperation::GetTime () const 
{ 
	return  m_lTime; 
}

//-----------------------------------------------------------------------------
const int& CFileSystemOperation::GetCalls() const 
{ 
	return  m_nCalls; 
}

//-----------------------------------------------------------------------------
void CFileSystemOperation::Increment (const long& lTime)
{ 
	m_nCalls++; 
	m_lTime += lTime;
}

//-----------------------------------------------------------------------------
void CFileSystemOperation::Decrement (const long& lTime)
{ 
	m_nCalls--; 
	m_lTime -= lTime;
}

///////////////////////////////////////////////////////////////////////////////
// 						CFileSystemOperations
///////////////////////////////////////////////////////////////////////////////
// No lock on this object as lock is performed to the manager classes
//-----------------------------------------------------------------------------
CFileSystemOperations::CFileSystemOperations()
{
}

//-----------------------------------------------------------------------------
CFileSystemOperations::~CFileSystemOperations()
{
	RemoveAll();
}

//----------------------------------------------------------------------------
CFileSystemOperation* CFileSystemOperations::GetAt	(int nIndex) const	
{ 
	return (CFileSystemOperation*) CObArray::GetAt(nIndex);	
}

//----------------------------------------------------------------------------
CFileSystemOperation* CFileSystemOperations::GetAt	(const CString& sName)
{ 
	for (int i=0 ; i <= GetUpperBound(); i++)
		if (sName.CompareNoCase(GetAt(i)->GetName()) == 0)
			return GetAt(i);

	return NULL;
}

//----------------------------------------------------------------------------
CFileSystemOperation*& CFileSystemOperations::ElementAt (int nIndex)
{ 
	return (CFileSystemOperation*&) CObArray::ElementAt(nIndex); 
}
	
//----------------------------------------------------------------------------
CFileSystemOperation* CFileSystemOperations::operator[] (int nIndex) const
{ 
	return GetAt(nIndex);	
}

//----------------------------------------------------------------------------
CFileSystemOperation*& CFileSystemOperations::operator[] (int nIndex)
{ 
	return ElementAt(nIndex);
}

//----------------------------------------------------------------------------
void CFileSystemOperations::RemoveAt(int nIndex, int nCount)
{
	int n = GetSize();
	int j = nCount;
	for (int i = nIndex; (i < n) && (j-- > 0); i++)
		if (GetAt(i)) delete GetAt(i);

	CObArray::RemoveAt(nIndex, nCount);
}

//-----------------------------------------------------------------------------
void CFileSystemOperations::RemoveAll()
{
	int n = GetSize();
	CObject* pO;
	for (int i = 0; i < n; i++) 
		if (pO = GetAt(i)) 
		{
			ASSERT_VALID(pO);
			delete pO;
		}

	CObArray::RemoveAll();
}

//-----------------------------------------------------------------------------
int	CFileSystemOperations::GetIndex (const CFileSystemOperation& aOperation)
{
	for (int i=0 ; i <= GetUpperBound(); i++)
		if (aOperation.GetName().CompareNoCase(GetAt(i)->GetName()) == 0)
			return i;

	return -1;
}

#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CFileSystemOperations::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCFileSystemOperations");
}
#endif //_DEBUG

///////////////////////////////////////////////////////////////////////////////
//				class IFileSystemManager implementation
///////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
IFileSystemManager::IFileSystemManager(BOOL bCacheEnabled /*TRUE*/)
	:
	m_pFileSystemDriver			(NULL),
	m_pAlternativeDriver		(NULL)
{
	m_Cacher.m_bEnabled = bCacheEnabled;
}

//-----------------------------------------------------------------------------
IFileSystemManager::IFileSystemManager (IFileSystemDriver* pFileSystem, IFileSystemDriver* pAlternative /*NULL*/, BOOL bCacheEnabled /*TRUE*/)
	:
	m_pFileSystemDriver			(NULL),
	m_pAlternativeDriver		(NULL),
	m_bFileSystemDriverOwner	(TRUE),
	m_bAlternativeDriverOwner	(TRUE)
{
	m_Cacher.m_bEnabled = bCacheEnabled;

	AttachFileSystemDriver	(pFileSystem,	FALSE);
	AttachAlternativeDriver (pAlternative,	FALSE);
}

//-----------------------------------------------------------------------------
IFileSystemManager::~IFileSystemManager	()
{
	if (m_pFileSystemDriver && m_bFileSystemDriverOwner)
		delete m_pFileSystemDriver;

	m_pFileSystemDriver = NULL;

	if (m_pAlternativeDriver && m_bAlternativeDriverOwner)
		delete m_pAlternativeDriver;

	m_pAlternativeDriver = NULL;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::IsAlternativeDriverEnabled	() const
{
	// no lock is required as pointer newed on InitInstance and never changed
	return m_pAlternativeDriver != NULL;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::IsFileSystemDriverEnabled	() const
{
	// no lock is required as pointer newed on InitInstance and never changed
	return m_pFileSystemDriver != NULL;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::IsManagedByAlternativeDriver (const CString& sName) const
{
	// no lock is required as it checks object and conditions that are defined 
	// in InitInstance and never changed
	return IsAlternativeDriverEnabled () && m_pAlternativeDriver->IsAManagedObject(sName);
}

//-----------------------------------------------------------------------------
void IFileSystemManager::AttachAlternativeDriver (IFileSystemDriver* pDriver, BOOL bReloadCache /*TRUE*/, BOOL bDriverOwner /*TRUE*/)
{
	// no lock is required as pointer newed on InitInstance and never changed
	m_pAlternativeDriver = pDriver;
	
	if (!m_pAlternativeDriver)
	{
		m_bAlternativeDriverOwner = FALSE;
		return;
	}

	m_bAlternativeDriverOwner = bDriverOwner;
	
	if (bReloadCache)
		LoadCaches ();
}

//-----------------------------------------------------------------------------
void IFileSystemManager::AttachFileSystemDriver (IFileSystemDriver* pDriver, BOOL bReloadCache /*TRUE*/, BOOL bDriverOwner /*TRUE*/)
{
	// no lock is required as pointer newed on InitInstance and never changed
	m_pFileSystemDriver = pDriver;

	if (!m_pFileSystemDriver)
	{
		m_bFileSystemDriverOwner = FALSE;
		return;
	}

	m_bFileSystemDriverOwner = bDriverOwner;

	if (bReloadCache)
		LoadCaches ();
}

//-----------------------------------------------------------------------------
IFileSystemDriver* IFileSystemManager::GetFileSystemDriver ()
{
	// no lock is required as pointer to the object are newed in InitInstance
	// and never changed. The objects pointer are wrapper class to operations
	// (see comment on IFileSystemDriver object)
	return m_pFileSystemDriver;
}

//-----------------------------------------------------------------------------
IFileSystemDriver* IFileSystemManager::GetAlternativeDriver ()
{
	// no lock is required as pointer to the object are newed in InitInstance
	// and never changed. The objects pointer are wrapper class to operations
	// (see comment on IFileSystemDriver object)
	return m_pAlternativeDriver;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::Start (BOOL bLoadCaches  /*TRUE*/)
{
	// no lock is required as invoked in InitInstance
	if (m_pFileSystemDriver)
		m_pFileSystemDriver->Start ();

	if (m_pAlternativeDriver)
		m_pAlternativeDriver->Start ();

	if (m_Cacher.IsEnabled())
		LoadCaches ();

	 return TRUE;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::Stop (BOOL bLoadCaches  /*TRUE*/)
{
	// no lock is required as invoked in ExitInstance
	if (m_pFileSystemDriver)
		m_pFileSystemDriver->Stop ();

	if (m_pAlternativeDriver)
		m_pAlternativeDriver->Stop ();

	if (m_Cacher.IsEnabled ())
		m_Cacher.ClearCaches ();
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::LoadCaches ()
{
	// no lock is required as invoked in InitInstance
	BOOL bOk = TRUE;

	// prepares the caches needed
	m_Cacher.ClearCaches ();

	if (!m_Cacher.IsEnabled())
		return TRUE;

	if (m_pAlternativeDriver && m_pAlternativeDriver->CanCache ())
		m_pAlternativeDriver->LoadCache (&m_Cacher);
	else if (m_pFileSystemDriver && m_pFileSystemDriver->CanCache())
		m_pFileSystemDriver->LoadCache (&m_Cacher);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemManager::AreCachesLoaded () const
{
	// no lock is required as overload 
	return m_Cacher.AreCachesLoaded();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void IFileSystemManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "IFileSystemManager\n");
}

void IFileSystemManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG


