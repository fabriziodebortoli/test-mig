
#include "stdafx.h"
#include <afxmt.h>


#include <TbNameSolver\Chars.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbParser\TokensTable.h>

#include "generic.h"
#include "parsobj.h"
#include "dirtreectrl.h"
#include "baseapp.h"
// resources
#include "generic.hjson" //JSON AUTOMATIC UPDATE
#include "dirtreectrl.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------
int CALLBACK BrowseCallbackProc(HWND hwnd,UINT uMsg,LPARAM lp, LPARAM pData) 
{
	//Browse for folders from the current Directory
	TCHAR szDir[MAX_PATH];         
	switch(uMsg)
	{
		case BFFM_INITIALIZED: 
			if (::GetCurrentDirectory(MAX_PATH, szDir))
				// WParam is TRUE since you are passing a path.
				// It would be FALSE if you were passing a pidl.
				::SendMessage(hwnd,BFFM_SETSELECTION,TRUE,(LPARAM)szDir);
			break;

		case BFFM_SELCHANGED: 
			// Set the status window to the currently selected path.
			if (SHGetPathFromIDList((LPITEMIDLIST) lp ,szDir))
				::SendMessage(hwnd,BFFM_SETSTATUSTEXT,0,(LPARAM)szDir);
			break;
		default:
			break;         
	}
	return 0;
}

//-------------------------------------------------------------------------------------------
CString DoBrowseForFolderDlg(const CString& strDlgText, CWnd* pOwnerWnd /*= NULL*/, LPCTSTR lpszRootDir/*= NULL*/)
{
	LPITEMIDLIST	pidl = NULL;
	TCHAR			szDir[MAX_PATH];

	if (lpszRootDir && IsPathName(lpszRootDir, TRUE))
	{
		LPSHELLFOLDER	pDesktopFolder;
		LPOLESTR		polePath;   
		ULONG			cEaten;
		ULONG			dwAttributes;

		// Get a pointer to the Desktop's IShellFolder interface.
		if (SUCCEEDED(::SHGetDesktopFolder(&pDesktopFolder)))   
		{
			
			USES_CONVERSION;
			// IShellFolder::ParseDisplayName requires the file name be in Unicode.
			polePath = T2W((LPTSTR)lpszRootDir);
			// Convert the path to an ITEMIDLIST.       //
			if (FAILED(pDesktopFolder->ParseDisplayName
										(
											NULL,
											NULL,
											polePath,
											&cEaten,
											&pidl,
											&dwAttributes
										)))
			{
				lpszRootDir = NULL;
				pidl = NULL;
			}
			//release the desktop folder object
			pDesktopFolder->Release();
		}
	}

	
	BROWSEINFO bi;
	LPMALLOC pMalloc;
	if (SUCCEEDED(::SHGetMalloc(&pMalloc)))
	{
		ZeroMemory(&bi,sizeof(bi));            
		bi.hwndOwner = pOwnerWnd ? pOwnerWnd->m_hWnd : NULL;
		bi.lpszTitle = (LPCTSTR)strDlgText;
		bi.pszDisplayName = 0;
		bi.pidlRoot = pidl;
		bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_STATUSTEXT;
		bi.lpfn = pidl ? NULL : BrowseCallbackProc; 

        pidl = ::SHBrowseForFolder(&bi);
		if (pidl) 
		{
			if (!::SHGetPathFromIDList(pidl,szDir)) 
				szDir[0] = NULL_CHAR;
			pMalloc->Free(pidl);
			pMalloc->Release();            
		}
	}
	return pidl ? szDir: _T("");      
}

/////////////////////////////////////////////////////////////////////////////
// CDirTreeCtrl definition
/////////////////////////////////////////////////////////////////////////////
#define MAX_DRIVE_VOLUME_LENGTH 256

// Image list indexes
#define ILI_HARD_DISK				0
#define ILI_FLOPPY					1
#define ILI_CD_ROM					2
#define ILI_MAPPED_DRIVE			3
#define ILI_NET_DRIVE			    4
#define ILI_FILE					5
#define ILI_CLOSED_FOLDER			6
#define ILI_OPEN_FOLDER				7
#define ILI_HIDDEN_FILE				8
#define ILI_NETWORK_MYCOMPUTER		9
#define ILI_NETWORK_NEIGHBORHOOD	10

#define ITEM_TYPE_DUMMY				0x00000000
#define ITEM_TYPE_FIXEDDRIVE		0x00000001
#define ITEM_TYPE_REMOVEABLEDRV		0x00000002
#define ITEM_TYPE_CDDRIVE			0x00000003
#define ITEM_TYPE_MAPPEDDRIVE		0x00000004
#define ITEM_TYPE_NETWKDRIVE		0x00000005
#define ITEM_TYPE_PATH				0x00000006
#define ITEM_TYPE_REMOTE_PATH		0x00000007
#define ITEM_TYPE_FILE				0x00000008
#define ITEM_TYPE_MYCOMPUTER		0x00000009
#define ITEM_TYPE_NETWKNEIGHBORHOOD	0x0000000A

//-------------------------------------------------------------------------------------------
static int CALLBACK DirTreeCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// The comparison function must return a negative value if the first item should 
	// precede the second, a positive value if the first item should follow the second,
	// or zero if the two items are equivalent.
	CDirTreeCtrl*	pTreeCtrl = (CDirTreeCtrl*) lParamSort;
	ASSERT_VALID(pTreeCtrl);

	if 
		(
			!pTreeCtrl->AreSubDirsShownBeforeFiles () || 
			lParam1 == lParam2
		)
		return 0;

	return (lParam1 == ITEM_TYPE_FILE) ? 1 : -1;
}

//-------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CDirTreeCtrl, CTBTreeCtrl)

BEGIN_MESSAGE_MAP(CDirTreeCtrl, CTBTreeCtrl)
	//{{AFX_MSG_MAP(CDirTreeCtrl)
	ON_NOTIFY_REFLECT	(TVN_ITEMEXPANDING,	OnItemExpanding)
	ON_NOTIFY_REFLECT	(TVN_ITEMEXPANDED,	OnItemExpanded)
	
	ON_MESSAGE			(WM_USER_DIRTREE_LOCAL_CHANGE,	OnLocalDrvChange)
	ON_MESSAGE			(WM_USER_DIRTREE_REMOTE_CHANGE,	OnNetwkDrvChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------
CDirTreeCtrl::CDirTreeCtrl (UINT nShowFlags, BOOL bMonitoringThreads /*=FALSE*/)
	:
	m_nShowFlags			(nShowFlags),
	m_bMonitoringThreads	(bMonitoringThreads),
	m_nThreadCount			(0),
	m_pThreadsHandleArray	(NULL),
	m_strFileFilter			("*.*")
{
	m_pFileChangeEvent = new ::CEvent(FALSE, TRUE);
}

//-------------------------------------------------------------------------------------------
CDirTreeCtrl::~CDirTreeCtrl ()
{
	delete m_pFileChangeEvent;

	ClearControl(FALSE);
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::ClearControl(BOOL bDelItems /*= TRUE*/)
{
    // Kill all running file change notification threads.
    if (m_nThreadCount && m_pThreadsHandleArray)
	{
        m_pFileChangeEvent->SetEvent ();
		
        ::WaitForMultipleObjects (m_nThreadCount, m_pThreadsHandleArray, TRUE,INFINITE);
		delete[] (HANDLE*)m_pThreadsHandleArray;
		
		m_pThreadsHandleArray= NULL;
        
		m_nThreadCount = 0;

		int nThreadNum = m_ThreadsArray.GetSize();
		if (nThreadNum > 0)
		{
			for (int i = 0; i < nThreadNum; i++)
				if (m_ThreadsArray.GetAt(i)) delete m_ThreadsArray.GetAt(i);
		}
		m_ThreadsArray.RemoveAll();
    }

	if (bDelItems && ::IsWindow(m_hWnd))
		DeleteAllItems();
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::InitializeCtrl (UINT nShowFlags /*= 0*/, LPCTSTR lpszInitialPath /*= NULL*/, LPCTSTR szFileFilter /*= NULL*/)
{
	CString asPaths[11];
	asPaths[0] = TBGlyph(szIconDirTreeCtrl0);
	asPaths[1] = TBGlyph(szIconDirTreeCtrl1);
	asPaths[2] = TBGlyph(szIconDirTreeCtrl2);
	asPaths[3] = TBGlyph(szIconDirTreeCtrl3);
	asPaths[4] = TBGlyph(szIconDirTreeCtrl4);
	asPaths[5] = TBGlyph(szIconDirTreeCtrl5);
	asPaths[6] = TBGlyph(szIconFolder);
	asPaths[7] = TBGlyph(szIconDirTreeCtrl7);
	asPaths[8] = TBGlyph(szIconDirTreeCtrl8);
	asPaths[9] = TBGlyph(szIconDirTreeCtrl9);
	asPaths[10] = TBGlyph(szIconDirTreeCtrl10);

	for (size_t i = 0; i < 11; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_ImageList.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 16, 16);
			m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
		}

		m_ImageList.Add(hIcon);
	}

	SetImageList(&m_ImageList, TVSIL_NORMAL /*TVSIL_STATE*/);

	m_nShowFlags |= nShowFlags;
	
	m_strFileFilter = szFileFilter;

	LoadDrivesInfo(lpszInitialPath);
}
    
//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::LoadDrivesInfo (LPCTSTR lpszInitialPath /*= NULL*/)
{
	CString strInitialPath;

	if (!lpszInitialPath || !lpszInitialPath[0])
	{
		HTREEITEM hSelItem = GetSelectedItem();
		if (hSelItem)
			strInitialPath = GetPathFromItem(hSelItem);
	}
	
	ClearControl();

	HTREEITEM hMyComputerItem = InsertItem (_TB("My Computer"), ILI_NETWORK_MYCOMPUTER,ILI_NETWORK_MYCOMPUTER);
	SetItemData(hMyComputerItem, ITEM_TYPE_MYCOMPUTER);
	AddDummyNode (hMyComputerItem);

	if (AreNetDrivesShown())
	{
		HTREEITEM hItem = InsertItem (_TB("Network Resources"), ILI_NETWORK_NEIGHBORHOOD,ILI_NETWORK_NEIGHBORHOOD);
		SetItemData(hItem, ITEM_TYPE_NETWKNEIGHBORHOOD);
		AddDummyNode (hItem);
	}
	SelectItemFromPath((lpszInitialPath || strInitialPath.IsEmpty()) ? lpszInitialPath : (LPCTSTR)strInitialPath);
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::ChangeShowFlags(UINT nShowFlags/* = DIRTREECTRL_SHOW_FILES */)
{
	if (m_nShowFlags == nShowFlags)
		return;
	
	m_nShowFlags = nShowFlags;
	
	LoadDrivesInfo();
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::ShowRemoteDrive(BOOL bShow /*= TRUE*/)
{
	BOOL bReload = (AreMappedNetDrivesShown() != bShow);
	if (bShow)
		m_nShowFlags |= DIRTREECTRL_SHOW_MAPPEDNETDRIVE;
	else
		m_nShowFlags &= ~(DIRTREECTRL_SHOW_MAPPEDNETDRIVE);

	if (bReload)
		LoadDrivesInfo();
}

//-------------------------------------------------------------------------------------------
UINT CDirTreeCtrl::AddMyComputerDriveNodes(HTREEITEM hMyComputerItem)
{
	int nPos = 0;
	UINT nCount = 0;
    
	CString strDrive = _T("?:\\");
	DWORD dwDriveList = ::GetLogicalDrives ();

    while (dwDriveList)
	{
        if (dwDriveList & 1)
		{
            strDrive.SetAt (0, 0x41 + nPos);
            if (AddDriveNode (hMyComputerItem, strDrive))
				nCount++;
		}
        dwDriveList >>= 1;
		nPos++;
	}
	return nCount;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::AddDriveNode (HTREEITEM hMyComputerItem, const CString& strDrive)
{
	// Can access to folder 
	if (!CanAccessFolder(strDrive, GENERIC_READ | GENERIC_WRITE))
	{
		return FALSE;
	}

	HTREEITEM hItem;
    UINT nType = ::GetDriveType ((LPCTSTR) strDrive);
    UINT nDrive = (UINT) strDrive[0] - 0x41;
	
	// Get the volume label:
	// The following characters in directory names or filenames cannot
	// be used, because they are reserved for Windows: < > : " / \ |
	// Therefore CDirTreeCtrl uses < > for writing the volume label....
	DWORD dwSerialNumber;
	CString strVolumeLabel;
	strVolumeLabel = "<";
	TCHAR* pVolumeLabelBuffer = strVolumeLabel.GetBuffer(MAX_DRIVE_VOLUME_LENGTH);
    if(!::GetVolumeInformation ((LPCTSTR) strDrive, pVolumeLabelBuffer + strVolumeLabel.GetLength(), MAX_DRIVE_VOLUME_LENGTH - 1, &dwSerialNumber, NULL, NULL, NULL, 0))
        dwSerialNumber = 0xFFFFFFFF;
	if(*(pVolumeLabelBuffer + strVolumeLabel.GetLength()) == NULL_CHAR)
		*pVolumeLabelBuffer = NULL_CHAR;
	strVolumeLabel.ReleaseBuffer();
	if (!strVolumeLabel.IsEmpty())
		strVolumeLabel += ">";

	switch (nType)
	{
		case DRIVE_REMOVABLE:
			hItem = InsertItem (strDrive + strVolumeLabel, ILI_FLOPPY,ILI_FLOPPY, hMyComputerItem);
			SetItemData(hItem, ITEM_TYPE_REMOVEABLEDRV);
			AddDummyNode (hItem);
			m_dwMediaIDArray.SetAtGrow(nDrive, dwSerialNumber);
			CreateMonitoringThread (strDrive);
			break;

		case DRIVE_FIXED:
			hItem = InsertItem (strDrive + strVolumeLabel, ILI_HARD_DISK,ILI_HARD_DISK, hMyComputerItem);
			SetItemData(hItem, ITEM_TYPE_FIXEDDRIVE);
			SetButtonState (hItem, strDrive);
			CreateMonitoringThread (strDrive);
			break;
		
		case DRIVE_REMOTE:
			if (AreMappedNetDrivesShown())
			{
				hItem = InsertItem (strDrive + strVolumeLabel, ILI_MAPPED_DRIVE,ILI_MAPPED_DRIVE, hMyComputerItem);
				SetItemData(hItem, ITEM_TYPE_MAPPEDDRIVE);
				SetButtonState (hItem, strDrive);
				CreateMonitoringThread (strDrive);
			}
			break;
		case DRIVE_CDROM:
			hItem = InsertItem (strDrive + strVolumeLabel, ILI_CD_ROM, ILI_CD_ROM, hMyComputerItem);
			SetItemData(hItem, ITEM_TYPE_CDDRIVE);
			AddDummyNode (hItem);
			m_dwMediaIDArray.SetAtGrow(nDrive, dwSerialNumber);
			CreateMonitoringThread (strDrive);
			break;

		default:
			return FALSE;
	}
    return TRUE;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::CanAccessFolder(LPCTSTR folderName, DWORD genericAccessRights)
{
	// Test if sstem is Workstation
	if (IsWorkStation()) 
		return TRUE;
		
	bool bRet = false;
	DWORD length = 0;
	if	(
			!::GetFileSecurity(folderName, OWNER_SECURITY_INFORMATION | GROUP_SECURITY_INFORMATION | DACL_SECURITY_INFORMATION, NULL, NULL, &length) &&
			ERROR_INSUFFICIENT_BUFFER == ::GetLastError()
		)
	{
		PSECURITY_DESCRIPTOR security = static_cast< PSECURITY_DESCRIPTOR >(::malloc(length));
		if (security && ::GetFileSecurity(folderName, OWNER_SECURITY_INFORMATION | GROUP_SECURITY_INFORMATION | DACL_SECURITY_INFORMATION, security, length, &length))
		{
			HANDLE hToken = NULL;
			if (::OpenProcessToken(::GetCurrentProcess(), TOKEN_IMPERSONATE | TOKEN_QUERY |	TOKEN_DUPLICATE | STANDARD_RIGHTS_READ, &hToken))
			{
				HANDLE hImpersonatedToken = NULL;
				if (::DuplicateToken(hToken, SecurityImpersonation, &hImpersonatedToken))
				{
					GENERIC_MAPPING mapping = { 0xFFFFFFFF };
					PRIVILEGE_SET privileges = { 0 };
					DWORD grantedAccess = 0, privilegesLength = sizeof(privileges);
					BOOL result = FALSE;

					mapping.GenericRead = FILE_GENERIC_READ;
					mapping.GenericWrite = FILE_GENERIC_WRITE;
					mapping.GenericExecute = FILE_GENERIC_EXECUTE;
					mapping.GenericAll = FILE_ALL_ACCESS;

					::MapGenericMask(&genericAccessRights, &mapping);
					if (::AccessCheck(security, hImpersonatedToken, genericAccessRights, &mapping, &privileges, &privilegesLength, &grantedAccess, &result))
					{
						bRet = (result == TRUE);
					}
					::CloseHandle(hImpersonatedToken);
				}
				::CloseHandle(hToken);
			}
			::free(security);
		}
	}
	return bRet;
}

//-------------------------------------------------------------------------------------------
UINT CDirTreeCtrl::AddDirectoryNodes (HTREEITEM hItem, CString& strPathName)
{
    HANDLE hFind;
	WIN32_FIND_DATA fd;
	CString strPathSpec = strPathName;
	
	UINT nCount = 0;
	
	if (strPathSpec.Left(2) == _T("\\\\"))
		strPathSpec = _T("\\\\?\\UNC\\") + strPathSpec.Mid(2);

	if (!IsDirSeparator(strPathSpec.Right (1))) strPathSpec += SLASH_CHAR;    
	
	CString strFileSpec = strPathSpec + _T("*.*");
	if ((hFind = ::FindFirstFile ((LPCTSTR) strFileSpec, &fd)) == INVALID_HANDLE_VALUE)
	{
		if (IsDriveNode (hItem))
			AddDummyNode (hItem);
		return 0;
	}
	do {
		CString strFileName = (LPCTSTR) &fd.cFileName;
		if ((strFileName != ".") && (strFileName != ".."))
		{
			if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				HTREEITEM hChild = InsertItem
										(
											(LPCTSTR) &fd.cFileName,
											ILI_CLOSED_FOLDER,
											ILI_CLOSED_FOLDER,
											hItem,
											TVI_SORT
										);
				CString strNewPathName = strPathName;
				if (!IsDirSeparator(strNewPathName.Right (1))) strNewPathName += SLASH_CHAR;

				strNewPathName += (LPCTSTR) &fd.cFileName;
				SetButtonState (hChild, strNewPathName);
				SetItemData(hChild, ITEM_TYPE_PATH);
				nCount++;
			}
			else if
			(
				AreFilesShown()					&&
				m_strFileFilter.IsEmpty()		&&
				(AreHiddenFilesShown() || !(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN))
			)
			{
				HTREEITEM hChild = InsertItem
										(
											(LPCTSTR) &fd.cFileName,
											(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN) ? ILI_HIDDEN_FILE : ILI_FILE,
											(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN) ? ILI_HIDDEN_FILE : ILI_FILE,
											hItem,
											TVI_SORT
										);
				SetItemData(hChild, ITEM_TYPE_FILE);
				nCount++;
			}
		}
	} while (::FindNextFile (hFind, &fd));
	::FindClose (hFind);

	if (AreFilesShown() && !m_strFileFilter.IsEmpty() && m_strFileFilter.Compare(_T("*.*")))
	{
		WIN32_FIND_DATA fd;
		TCHAR	seps[] = _T("|");
		LPTSTR	lpszDupStr = _tcsdup((LPCTSTR)m_strFileFilter);
		TCHAR* nextToken;
		TCHAR*	token = _tcstok_s(lpszDupStr, seps, &nextToken);
		while( token != NULL)
		{
			CString strFileSpec = token;
			strFileSpec.TrimLeft();
			strFileSpec.TrimRight();
			strFileSpec = strPathSpec + strFileSpec;

			if ((hFind = ::FindFirstFile ((LPCTSTR) strFileSpec, &fd)) != INVALID_HANDLE_VALUE)
			{
				do {
					CString strFileName = (LPCTSTR) &fd.cFileName;
					if ((strFileName != ".") && (strFileName != ".."))
					{
						if
						(
							!(fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) &&
							(AreHiddenFilesShown() || !(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN))
						)
						{
							BOOL bAlreadyInserted = FALSE;
							HTREEITEM hSiblingItem = GetChildItem(hItem);   
							while (hSiblingItem != NULL)
							{
								if (GetItemData(hSiblingItem) == ITEM_TYPE_FILE && !GetItemText(hSiblingItem).CompareNoCase((LPCTSTR)fd.cFileName))
								{
									bAlreadyInserted = TRUE;
									break;
								}
								hSiblingItem = GetNextSiblingItem(hSiblingItem);   
							}
							if (!bAlreadyInserted)
							{
								HTREEITEM hChild = InsertItem
														(
															(LPCTSTR) &fd.cFileName,
															(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN) ? ILI_HIDDEN_FILE : ILI_FILE,
															(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN) ? ILI_HIDDEN_FILE : ILI_FILE,
															hItem,
															TVI_SORT
														);
								SetItemData(hChild, ITEM_TYPE_FILE);
								nCount++;
							}
						}
					}
				} while (::FindNextFile (hFind, &fd));
				::FindClose (hFind);
			}
			token = _tcstok_s(NULL, seps, &nextToken);
		}
		delete lpszDupStr;
	}

	return nCount;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::AddNetwkDriveNodes (HTREEITEM hNetwkNeighborhoodItem, DWORD dwScope)
{
	if (!AreNetDrivesShown())
		return FALSE;

	DWORD	dwResult = NO_ERROR;
	HANDLE	hEnum = 0;
	
    if 
		(
			WNetOpenEnum
					(
						dwScope, 
						RESOURCETYPE_ANY,
						0,
						NULL,
						&hEnum
					) != NO_ERROR
		)
	{
		return FALSE;
	}
	
	DWORD cbBuffer = 16384; // 16 KB
	DWORD cEntries = 0xFFFFFFFF;
	LPNETRESOURCE lpnrDrv;
	DWORD i;

	CList<CString, CString> m_lstNetDevice; // For remove the Key sensitivi NT systems

	do
	{
		lpnrDrv = (LPNETRESOURCE) GlobalAlloc( GPTR, cbBuffer );
		dwResult = WNetEnumResource(hEnum, &cEntries, lpnrDrv, &cbBuffer);
		if (dwResult == NO_ERROR)
		{
			for( i = 0; i < cEntries; i++ )
			{
			   if( lpnrDrv[i].lpRemoteName != NULL )
			   {
				   LPWSTR pRemoteName = lpnrDrv[i].lpRemoteName;
				   CString cStRemoteName(pRemoteName);
				   cStRemoteName.MakeUpper();
				   if (CanAccessFolder(pRemoteName, GENERIC_READ | GENERIC_WRITE) && m_lstNetDevice.Find(cStRemoteName) == NULL)
				   {   
						m_lstNetDevice.AddTail(cStRemoteName);
						HTREEITEM hItem = InsertItem(cStRemoteName, ILI_NET_DRIVE, ILI_NET_DRIVE, hNetwkNeighborhoodItem);
						SetItemData(hItem, ITEM_TYPE_NETWKDRIVE);
						SetButtonState(hItem, pRemoteName);
				   }
			   }
			}
		}
		GlobalFree( (HGLOBAL) lpnrDrv );
		
		if (dwResult == ERROR_MORE_DATA)
			continue;
		
		if (dwResult != NO_ERROR && dwResult != ERROR_NO_MORE_ITEMS)
		{
			WNetCloseEnum(hEnum);
			return FALSE;
		}
	}
	while( dwResult != ERROR_NO_MORE_ITEMS );

	WNetCloseEnum(hEnum);
	return TRUE;
}

//-------------------------------------------------------------------------------------------
UINT CDirTreeCtrl::AddNetwkSubNodes (HTREEITEM hItem)
{
	if (!AreNetDrivesShown() || (GetItemData(hItem) != ITEM_TYPE_NETWKDRIVE))
		return 0;

	DWORD	dwResult = NO_ERROR;
	HANDLE	hEnum = 0;
	

    NETRESOURCE netResDrv;
	netResDrv.dwScope = RESOURCE_GLOBALNET;
	netResDrv.dwType = RESOURCETYPE_ANY;
	netResDrv.dwDisplayType = RESOURCEDISPLAYTYPE_GENERIC;
	netResDrv.dwUsage = RESOURCEUSAGE_CONTAINER;
	CString strItemText = GetItemText(hItem);
	netResDrv.lpRemoteName = strItemText.GetBuffer(strItemText.GetLength()+1);
	strItemText.ReleaseBuffer();
	netResDrv.lpLocalName = NULL;
	netResDrv.lpProvider = NULL;

    dwResult = WNetOpenEnum
					(
						RESOURCE_GLOBALNET,
						RESOURCETYPE_ANY,
						RESOURCEUSAGE_CONNECTABLE,
						&netResDrv,
						&hEnum
					);
	if (dwResult != NO_ERROR)
		return 0;
	
	DWORD cbBuffer = 16384; // 16 KB
	DWORD cEntries = 0xFFFFFFFF;
	LPNETRESOURCE lpnrDrv;
	DWORD i;
	UINT  nCount= 0;

	do
	{
		lpnrDrv = (LPNETRESOURCE) GlobalAlloc( GPTR, cbBuffer );

		dwResult = WNetEnumResource(hEnum, &cEntries, lpnrDrv, &cbBuffer);

		if (dwResult == NO_ERROR)
		{
			for( i = 0; i < cEntries; i++ )
			{
			   if( lpnrDrv[i].lpRemoteName != NULL )
			   {
					CString strRemotePathName = lpnrDrv[i].lpRemoteName;
					int nLastSlash = strRemotePathName.ReverseFind('\\'); 
					if (nLastSlash >= 2 && nLastSlash < strRemotePathName.GetLength())
						strRemotePathName = strRemotePathName.Mid(nLastSlash + 1);
					HTREEITEM hChild = InsertItem
										(
											(LPCTSTR)strRemotePathName,
											ILI_CLOSED_FOLDER,
											ILI_CLOSED_FOLDER,
											hItem,
											TVI_SORT
										);
					SetItemData(hChild, ITEM_TYPE_REMOTE_PATH);
					SetButtonState (hChild, lpnrDrv[i].lpRemoteName);
					CreateMonitoringThread (lpnrDrv[i].lpRemoteName, TRUE);
					nCount++;
			   }
			}
		}
		GlobalFree( (HGLOBAL) lpnrDrv );
		
		if (dwResult == ERROR_MORE_DATA)
			continue;
		
		if (dwResult != NO_ERROR && dwResult != ERROR_NO_MORE_ITEMS)
		{
			WNetCloseEnum(hEnum);
			return 0;
		}
	}
	while( dwResult != ERROR_NO_MORE_ITEMS );

	WNetCloseEnum(hEnum);
	return nCount;
}


//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::SetButtonState (HTREEITEM hItem, const CString& strPathName)
{
	if
		(
			(GetItemData(hItem) == ITEM_TYPE_NETWKDRIVE)||
			HasSubNodes (strPathName)
		)
        AddDummyNode (hItem);
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::UpdateButtonState (HTREEITEM hItem, const CString& strPathName)
{
    if (HasSubNodes (strPathName))
	{
        if (!ItemHasChildren (hItem))
		{
            AddDummyNode (hItem);
			Invalidate ();
		}
	}
    else
	{
        if (ItemHasChildren (hItem))
            DeleteChildren (hItem);
	}
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::DeleteChildren (HTREEITEM hItem)
{
    HTREEITEM hChild = GetChildItem (hItem);
    while (hChild != NULL)
	{
        HTREEITEM hNextItem = GetNextSiblingItem (hChild);
		DeleteItem (hChild);
        hChild = hNextItem;
    }
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::HasSubdirectory ( const CString& strPathName)
{
    HANDLE hFind;
    WIN32_FIND_DATA fd;
	BOOL bResult = FALSE;
	CString strFileSpec = strPathName;
	
	if (strFileSpec.Left(2) == _T("\\\\"))
		strFileSpec = _T("\\\\?\\UNC\\") + strFileSpec.Mid(2);

	if (!IsDirSeparator((strFileSpec.Right (1)))) strFileSpec += SLASH_CHAR;

	strFileSpec += _T("*.*");
    
	if ((hFind = ::FindFirstFile ((LPCTSTR) strFileSpec, &fd)) != INVALID_HANDLE_VALUE)
	{
        do {
            if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
                CString strFileName = (LPCTSTR) &fd.cFileName;
                if ((strFileName != ".") && (strFileName != ".."))
				{
					bResult = TRUE;
					break;
				}
			}
        } while (::FindNextFile (hFind, &fd) && !bResult);
        ::FindClose (hFind);
	}
	return bResult;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::HasSubNodes ( const CString& strPathName)
{
	if (HasSubdirectory(strPathName))
		return TRUE;
	
	if (!AreFilesShown())
		return FALSE;

    HANDLE hFind;
    WIN32_FIND_DATA fd;
	BOOL bResult = FALSE;
	CString strPathSpec = strPathName;
	
	if (strPathSpec.Left(2) == _T("\\\\"))
		strPathSpec = _T("\\\\?\\UNC\\") + strPathSpec.Mid(2);

	if (!IsDirSeparator(strPathSpec.Right (1))) strPathSpec += SLASH_CHAR;    

	CString strFilterSpec = m_strFileFilter.IsEmpty() ? _T("*.*") : m_strFileFilter;
	TCHAR	seps[] = _T("|");
	TCHAR* nextToken;
	TCHAR*	token = _tcstok_s(strFilterSpec.GetBuffer(strFilterSpec.GetLength()), seps, &nextToken);
	while( token != NULL)
	{
		CString strFileSpec = token;
		strFileSpec.TrimLeft();
		strFileSpec.TrimRight();
		strFileSpec = strPathSpec + strFileSpec;
    
		if ((hFind = ::FindFirstFile ((LPCTSTR) strFileSpec, &fd)) != INVALID_HANDLE_VALUE)
		{
			do {
				if 
				(
					!(fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)	&&
					AreFilesShown()										&&
					(AreHiddenFilesShown() || !(fd.dwFileAttributes & FILE_ATTRIBUTE_HIDDEN))
				)
				{
					bResult = TRUE;
					break;
				}
			} while (::FindNextFile (hFind, &fd) && !bResult);
			::FindClose (hFind);
		}
		token = _tcstok_s(NULL, seps, &nextToken);
	}
	strFilterSpec.ReleaseBuffer();
	return bResult;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::IsDriveNode (HTREEITEM hItem)
{
    return (GetParentItem (hItem) == NULL) ? TRUE : FALSE;
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::AddDummyNode (HTREEITEM hItem)
{
    HTREEITEM hDummyItem = InsertItem (_T(""), 0, 0, hItem);
	SetItemData(hDummyItem, ITEM_TYPE_DUMMY);
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::RemoveDummyNode (HTREEITEM hItem)
{
    HTREEITEM hChild = GetChildItem (hItem);
    while (hChild != NULL)
	{
        HTREEITEM hNextItem = GetNextSiblingItem (hChild);
		if (GetItemData(hChild) == ITEM_TYPE_DUMMY)
			DeleteItem (hChild);
        hChild = hNextItem;
    }
}

//-------------------------------------------------------------------------------------------
HTREEITEM CDirTreeCtrl::GetDriveNode (HTREEITEM hItem)
{
	HTREEITEM hParent;
    do {
		hParent = GetParentItem (hItem);
        if (hParent != NULL)
			hItem = hParent;
    } while (hParent != NULL);
    return hItem;
}

//-------------------------------------------------------------------------------------------
CString CDirTreeCtrl::GetPathFromItem (HTREEITEM hItem)
{
    CString strPathName;
    while (hItem != NULL)
	{
        LONG nItemType = GetItemData(hItem);
		if (nItemType == ITEM_TYPE_MYCOMPUTER || nItemType == ITEM_TYPE_NETWKNEIGHBORHOOD)
			break;

		CString string = GetItemText (hItem);
		// The following characters in directory names or filenames cannot
		// be used, because they are reserved for Windows: < > : " / \ |
		// CDirTreeCtrl uses < > for the volume label....
		int nIdx = string.Find(_T('<'));
		if (nIdx >= 0)
			string = string.Left(nIdx);

        if (!IsDirSeparator(string.Right (1)) && !strPathName.IsEmpty ()) string += SLASH_CHAR;

        strPathName = string + strPathName;
        hItem = GetParentItem (hItem);
	}
    return strPathName;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::SetSelectedDirectory	(const CString& strPath)
{
	return SelectItemFromPath((LPCTSTR) strPath);
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::SelectItemFromPath(LPCTSTR lpszPath)
{
	if (!lpszPath || ::_tcslen (lpszPath) < 3)
		return FALSE;
	
	// Devo scandire la path e espandere man mano l'albero
	CString strPath = lpszPath;
    HTREEITEM hItem = NULL;
	int nIndex = -1;

	if (strPath.Left(2) == _T("\\\\")) // UNC Path
	{
		hItem = SearchSiblingItems (GetNextItem (NULL, TVGN_ROOT), _TB("Network Resources"));
		nIndex = 2 + _tcscspn((LPCTSTR)lpszPath + 2, _T("\\"));
	}
	else
	{
		hItem = SearchSiblingItems (GetNextItem (NULL, TVGN_ROOT), _TB("My Computer"));
		nIndex = 3;
	}
	if (hItem == NULL)
		return FALSE;
	Expand(hItem, TVE_EXPAND);
	hItem = GetChildItem (hItem);
	if (hItem == NULL)
		return FALSE;

    while (strPath.GetLength () > 0) 
	{
		HTREEITEM hNewItem = SearchSiblingItems (hItem, nIndex == -1 ? strPath : strPath.Left (nIndex));
		if (hNewItem == NULL)
		{
			hItem = GetParentItem (hItem);
			break;
		}
		hItem = hNewItem;
        Expand (hItem, TVE_EXPAND);
		if (nIndex == -1)
            strPath.Empty ();
		else
		{
			hItem = GetChildItem (hItem);
			if (hItem == NULL)
				break;

            if (!IsDirSeparator(strPath[nIndex-1])) nIndex++;

			strPath = strPath.Right (strPath.GetLength () - nIndex);    
        }

		nIndex = strPath.FindOneOf(UNC_SLASH_CHARS);		
	}

	Expand (hItem, TVE_EXPAND);
	Select (hItem, TVGN_CARET);
	EnsureVisible(hItem);
    return TRUE;
}

//-------------------------------------------------------------------------------------------
HTREEITEM CDirTreeCtrl::SearchSiblingItems (HTREEITEM hItem, const CString& strPathToSearch)
{
    while (hItem != NULL) 
	{
		CString strItemPath = GetItemText(hItem);
		int nIdx = strItemPath.Find(_T('<')); // Salto un'eventuale volume label
		if (nIdx >= 0)
			strItemPath = strItemPath.Left(nIdx);
		if (!strPathToSearch.CompareNoCase(strItemPath))
			break;
		hItem = GetNextSiblingItem (hItem);
	}
	return hItem;
}

//-------------------------------------------------------------------------------------------
CString CDirTreeCtrl::GetSelectedDirectory()
{
	HTREEITEM hItem = GetSelectedItem();

	if (!hItem)
		return _T("");
	
	if (GetItemData(hItem) == ITEM_TYPE_FILE)
        hItem = GetParentItem (hItem);

	return GetPathFromItem(hItem);
}

//-------------------------------------------------------------------------------------------
CString CDirTreeCtrl::GetSelectedFile()
{
	if (!AreFilesShown())
		return _T("");

	HTREEITEM hItem = GetSelectedItem();

	if (!hItem || GetItemData(hItem) != ITEM_TYPE_FILE)
		return _T("");

	return GetPathFromItem(hItem);
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::IsMediaValid (const CString& strPathName)
{
    // Return TRUE if the drive doesn't support removable media.
    UINT nDriveType = ::GetDriveType ((LPCTSTR) strPathName);
    if ((nDriveType != DRIVE_REMOVABLE) && (nDriveType != DRIVE_CDROM))
        return TRUE;

    // Return FALSE if the drive is empty (::GetVolumeInformation fails).
    DWORD dwSerialNumber;    
	CString strDrive = strPathName.Left (3);
    UINT nDrive = (UINT) strDrive[0] - 0x41;
    if (!::GetVolumeInformation ((LPCTSTR) strDrive, NULL, 0,
        &dwSerialNumber, NULL, NULL, NULL, 0))
	{
		m_dwMediaIDArray.SetAtGrow(nDrive, 0xFFFFFFFF);
		return FALSE;
	}
    // Also return FALSE if the disk's serial number has changed.
    if ((m_dwMediaIDArray[nDrive] != dwSerialNumber) && (m_dwMediaIDArray[nDrive] != 0xFFFFFFFF))
	{
		m_dwMediaIDArray.SetAtGrow(nDrive, dwSerialNumber);
		return FALSE;
	}
    // Update our record of the serial number and return TRUE.
	m_dwMediaIDArray.SetAtGrow(nDrive, dwSerialNumber);
	return TRUE;
}

//-------------------------------------------------------------------------------------------
BOOL CDirTreeCtrl::IsPathValid (const CString& strPathName)
{
    if (strPathName.GetLength() == 3)
		return TRUE;
	
	HANDLE hFind;
    WIN32_FIND_DATA fd;
	BOOL bResult = FALSE;
	CString strTmpPath = strPathName;
	if (strTmpPath.Left(2) == _T("\\\\"))
		strTmpPath = _T("\\\\?\\UNC\\") + strTmpPath.Mid(2);

	if (!IsDirSeparator(strTmpPath.Right (1))) strTmpPath += SLASH_CHAR;

	strTmpPath += _T("*.*");
    
	if ((hFind = ::FindFirstFile ((LPCTSTR) strTmpPath, &fd)) != INVALID_HANDLE_VALUE)
	{
		if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			bResult = TRUE;
		::FindClose (hFind);
	}
    return bResult;
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::OnItemExpanding (NMHDR* pnmh, LRESULT* pResult)
{   
    NM_TREEVIEW* pnmtv = (NM_TREEVIEW*) pnmh;
    HTREEITEM hItem = pnmtv->itemNew.hItem;

    EnsureVisible(hItem);

	CString strPathName = GetPathFromItem (hItem);
	*pResult = FALSE;
    
    LONG nItemType = GetItemData(hItem);
	if (nItemType != ITEM_TYPE_MYCOMPUTER && nItemType != ITEM_TYPE_NETWKNEIGHBORHOOD)
	{
		while (TRUE)
		{
			if (IsMediaValid (strPathName))
				break;
			// Reset the drive node if the drive is empty or the media changed.
			if (AfxMessageBox(cwsprintf(_TB("Unable to access unit {0-%s}."), strPathName.Left (3)),MB_RETRYCANCEL) == IDCANCEL)
			{
				HTREEITEM hRoot = GetDriveNode (hItem);
				Expand (hRoot, TVE_COLLAPSE);
				DeleteChildren (hRoot);
				AddDummyNode (hRoot);
				*pResult = TRUE;
				return;
			}
		}
		// Delete the item if strPathName no longer specifies a valid path.
		if (nItemType != ITEM_TYPE_NETWKDRIVE && !IsPathValid (strPathName))
		{
			DeleteItem (hItem);
			*pResult = TRUE;
			return;
		}
    }
	BeginWaitCursor();
    // If the item is expanding, delete the dummy item attached to it
    // and add folder items. If the item is collapsing instead, delete
    // its folder items and add a dummy item if appropriate.
    if (pnmtv->action == TVE_EXPAND)
	{
        DeleteChildren (hItem);
		switch (nItemType)
		{
			case ITEM_TYPE_MYCOMPUTER:
				if (!AddMyComputerDriveNodes(hItem))
				{
					*pResult = TRUE;
					DeleteItem(hItem);
				}
				break;

			case ITEM_TYPE_NETWKNEIGHBORHOOD:
				if (!AddNetwkDriveNodes(hItem, RESOURCE_CONTEXT))
					*pResult = TRUE;
				if (!AddNetwkDriveNodes(hItem, RESOURCE_CONNECTED))
					*pResult = TRUE;
				break;
			
			default:
				if (!IsDriveNode(hItem))
					SetItemImage(hItem, ILI_OPEN_FOLDER, ILI_OPEN_FOLDER);
				
				if (nItemType == ITEM_TYPE_NETWKDRIVE)
				{
					if (!AddNetwkSubNodes(hItem))
					{
						// Wolking in directory
						if (!AddDirectoryNodes(hItem, strPathName))
						{
							RemoveDummyNode(hItem);
							*pResult = TRUE;
						}
					}
				}
				else if (!AddDirectoryNodes (hItem, strPathName))
					*pResult = TRUE;
				break;
		}
    }
	else
	{
        DeleteChildren (hItem);
		if (IsDriveNode (hItem) || (nItemType == ITEM_TYPE_MYCOMPUTER) || (nItemType == ITEM_TYPE_NETWKNEIGHBORHOOD))
			AddDummyNode (hItem);
		else
		{
	        SetItemImage(hItem, ILI_CLOSED_FOLDER, ILI_CLOSED_FOLDER);
			SetButtonState (hItem, strPathName);
		}
    }
	EndWaitCursor();
}

//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::OnItemExpanded (NMHDR* pnmh, LRESULT* pResult)
{   
    NM_TREEVIEW* pnmtv = (NM_TREEVIEW*) pnmh;

	// Sort the tree control's items using the DirTreeCompareProc callback procedure.
	TVSORTCB tvs;
	tvs.hParent = pnmtv->itemNew.hItem;
	tvs.lpfnCompare = DirTreeCompareProc;
	tvs.lParam = (LPARAM) this;

	SortChildrenCB(&tvs);
}

//-------------------------------------------------------------------------------------------
LRESULT CDirTreeCtrl::OnLocalDrvChange (WPARAM wp, LPARAM)
{
	CString strDrive = _T("?:\\");
	strDrive.SetAt (0, 0x41 + (UINT)wp);    
	
	//@@TODO: Refresh del nodo del drive se aperto

	OnLocalDrvChangeMsg(strDrive);
	
	return 0;
}

//-------------------------------------------------------------------------------------------
LRESULT CDirTreeCtrl::OnNetwkDrvChange (WPARAM, LPARAM lp)
{
	CString strDrive = (LPCTSTR)lp;

	//@@TODO: Refresh del nodo del drive se aperto

	OnNetwkDrvChangeMsg(strDrive);

	return 0;
}

/////////////////////////////////////////////////////////////////////////////////////////////
UINT DirTreeCtrlThreadFunc	(LPVOID);
//-------------------------------------------------------------------------------------------
void CDirTreeCtrl::CreateMonitoringThread (const CString& strDrive, BOOL bNetwkDrv /*= FALSE*/)
{
    if (!m_bMonitoringThreads)
		return;

	PTHREADINFO pThreadInfo = new THREADINFO; // Thread will delete
    if(!bNetwkDrv)
	{
		pThreadInfo->strDrive = "?:\\";
		pThreadInfo->strDrive.SetAt (0, strDrive[0]);    
    }
	else
	{
		pThreadInfo->strDrive = strDrive;
		if (!IsDirSeparator(pThreadInfo->strDrive.Right (1)))
			pThreadInfo->strDrive += SLASH_CHAR;
    }
	pThreadInfo->bNetwkDrv = bNetwkDrv;
    pThreadInfo->hEvent = m_pFileChangeEvent->m_hObject;
	pThreadInfo->hWnd = m_hWnd;
 
	CTBWinThread* pThread = AfxBeginTBThread (DirTreeCtrlThreadFunc, pThreadInfo, NULL, THREAD_PRIORITY_IDLE);
	if (!pThread)
		return;
	pThread->SetThreadName ("DirTreeCtrlThreadFunc");

	//TRACE ("===> CDirTreeCtrl: Starting thread 0x%.02LX.\n", pThread->m_nThreadID);
    pThread->m_bAutoDelete = FALSE;
    m_ThreadsArray.Add(pThread);

	HANDLE* pTmpHandle = (HANDLE*) new HANDLE[(m_nThreadCount + 1) * sizeof(HANDLE)];
	if (m_pThreadsHandleArray)
	{
	    for (UINT i =0; i < m_nThreadCount; i++)
			pTmpHandle[i] = m_pThreadsHandleArray[i];
		delete[] (HANDLE*)m_pThreadsHandleArray;
	}
	m_pThreadsHandleArray = pTmpHandle;

    m_pThreadsHandleArray[m_nThreadCount] = pThread->m_hThread;
	m_nThreadCount++;
}

//-------------------------------------------------------------------------------------------
// Thread function for detecting file system changes
//-------------------------------------------------------------------------------------------
UINT DirTreeCtrlThreadFunc (LPVOID pParam)
{
    PTHREADINFO pThreadInfo = (PTHREADINFO) pParam;

	LPTSTR lpszDrive = NULL;
	//int nDriveLen = _tcslen ((LPCTSTR)pThreadInfo->strDrive);
	int nDriveLen = pThreadInfo->strDrive.GetLength();
	if (nDriveLen)
	{
		lpszDrive = new TCHAR [nDriveLen + 1];
		TB_TCSCPY(lpszDrive, (LPCTSTR)pThreadInfo->strDrive);
	}
	
	BOOL	bNetwkDrv = pThreadInfo->bNetwkDrv;
	HANDLE hEvent = pThreadInfo->hEvent;
    HWND hWnd = pThreadInfo->hWnd;
	delete pThreadInfo;

	// Get a handle to a file change notification object.
    HANDLE hChange = ::FindFirstChangeNotification ((LPCTSTR)lpszDrive, TRUE, FILE_NOTIFY_CHANGE_DIR_NAME | FILE_NOTIFY_CHANGE_FILE_NAME);
    
	// Return now if ::FindFirstChangeNotification failed.
    if (hChange == INVALID_HANDLE_VALUE)
	{
		TRACE1("FindFirstChangeNotification failed: %s\n", lpszDrive);
		return 1;
	}

	HANDLE aHandles[2];
    aHandles[0] = hChange;
	aHandles[1] = hEvent;
	BOOL bContinue = TRUE;
    
	// Sleep until a file change notification wakes this thread or
    // m_pFileChangeEvent becomes set indicating it's time for 
	// the thread to end.
    while (bContinue)
	{
        if (::WaitForMultipleObjects(2, aHandles, FALSE, INFINITE) - WAIT_OBJECT_0 == 0)
		{
			// Respond to a change notification.
			if (!bNetwkDrv)
			{
				UINT nDrive = lpszDrive[0] - 0x41;
				::PostMessage (hWnd, WM_USER_DIRTREE_LOCAL_CHANGE, (WPARAM) nDrive, 0);
			}
			else
				::PostMessage (hWnd, WM_USER_DIRTREE_REMOTE_CHANGE, 0, (LPARAM)(LPCTSTR)lpszDrive);
			
			::FindNextChangeNotification (hChange);
        }
        else // Kill this thread (m_pFileChangeEvent became signaled).
            bContinue = FALSE;
	}
    // Close the file change notification handle and return.
    ::FindCloseChangeNotification (hChange);
	return 0;
}

//-------------------------------------------------------------------------------------------


/////////////////////////////////////////////////////////////////////////////
//							CChooseDirDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CChooseDirDlg::CChooseDirDlg(LPCTSTR lpszInitialPath) 
: 
	CParsedDialog (IDD_CHOOSE_DIR),
	m_strInitialPath(lpszInitialPath)
{
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CChooseDirDlg, CParsedDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CChooseDirDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CChooseFileDlg)
    ON_NOTIFY		(TVN_SELCHANGED, IDC_DIRTREE_CTRL, OnDirTreeSelChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BOOL CChooseDirDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY (m_DirTreeCtrl.SubclassDlgItem(IDC_DIRTREE_CTRL,	this));
	m_DirTreeCtrl.InitializeCtrl(DIRTREECTRL_SHOW_REMOTEDRIVE | DIRTREECTRL_SHOW_MAPPEDNETDRIVE, m_strInitialPath);

	return FALSE;
}

//-------------------------------------------------------------------------------------------
void CChooseDirDlg::OnDirTreeSelChanged (NMHDR* , LRESULT*)
{
	m_strSelectedDir = m_DirTreeCtrl.GetSelectedDirectory();
	SetDlgItemText(IDC_SEL_DIR_NAME, m_strSelectedDir);
}


/////////////////////////////////////////////////////////////////////////////
//							CChooseFileDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CChooseFileDlg::CChooseFileDlg(LPCTSTR lpszInitialFile) 
: 
	CParsedDialog( IDD_CHOOSE_DIR),
	m_lpszInitialFile(lpszInitialFile)
{
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CChooseFileDlg, CParsedDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CChooseFileDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CChooseFileDlg)
    ON_NOTIFY		(TVN_SELCHANGED, IDC_DIRTREE_CTRL, OnDirTreeSelChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BOOL CChooseFileDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY (m_DirTreeCtrl.SubclassDlgItem(IDC_DIRTREE_CTRL,	this));
	m_DirTreeCtrl.InitializeCtrl(DIRTREECTRL_SHOW_FILES | DIRTREECTRL_SHOW_SUBDIRSBEFOREFILES| DIRTREECTRL_SHOW_REMOTEDRIVE | DIRTREECTRL_SHOW_MAPPEDNETDRIVE,m_lpszInitialFile);

	return FALSE;
}

//-------------------------------------------------------------------------------------------
void CChooseFileDlg::OnDirTreeSelChanged (NMHDR* , LRESULT*)
{
	m_strSelectedFile = m_DirTreeCtrl.GetSelectedFile();
	if (m_strSelectedFile.IsEmpty())
		m_strSelectedFile = m_DirTreeCtrl.GetSelectedDirectory();	
	
	SetDlgItemText(IDC_SEL_DIR_NAME, m_strSelectedFile);
}
