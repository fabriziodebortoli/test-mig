
#pragma once

#include <afxmt.h>

#include "parsobj.h"
#include "TbTreeCtrl.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
TB_EXPORT CString DoBrowseForFolderDlg(const CString& strDlgText, CWnd* pOwnerWnd = NULL, LPCTSTR lpszRootDir = NULL);
/////////////////////////////////////////////////////////////////////////////
// CDirTreeCtrl declarations

#define DIRTREECTRL_SHOW_FILES				0x00000001
#define DIRTREECTRL_SHOW_HIDDENFILES		0x00000002
#define DIRTREECTRL_SHOW_SUBDIRSBEFOREFILES 0x00000004
#define DIRTREECTRL_SHOW_MAPPEDNETDRIVE		0x00000008
#define DIRTREECTRL_SHOW_REMOTEDRIVE		0x00000010

/////////////////////////////////////////////////////////////////////////////
typedef struct { 
	CString	strDrive;
	BOOL	bNetwkDrv;
	HANDLE	hEvent;
	HWND	hWnd;

} THREADINFO;
typedef THREADINFO* PTHREADINFO;

/////////////////////////////////////////////////////////////////////////////
// CDirTreeCtrl control

class TB_EXPORT CDirTreeCtrl : public CTBTreeCtrl
{
	DECLARE_DYNCREATE (CDirTreeCtrl)

protected:
	CImageList	m_ImageList;

	UINT		m_nShowFlags;
	CDWordArray	m_dwMediaIDArray;

	::CEvent*	m_pFileChangeEvent; //usare il risolutore di scopo globale per evitare conflitti con ATL

	BOOL		m_bMonitoringThreads;
	UINT		m_nThreadCount;
	HANDLE*		m_pThreadsHandleArray;
	CObArray	m_ThreadsArray;
	CString		m_strFileFilter;

public:
	CDirTreeCtrl(UINT nShowFlags = DIRTREECTRL_SHOW_MAPPEDNETDRIVE, BOOL bMonitoringThreads = FALSE);
	~CDirTreeCtrl();

	void		InitializeCtrl			(UINT nShowFlags = 0, LPCTSTR lpszInitialPath = NULL, LPCTSTR szFileFilter = NULL);
	void		ChangeShowFlags			(UINT nShowFlags = DIRTREECTRL_SHOW_FILES);
	UINT		GetShowFlags			() const { return m_nShowFlags;}
	CString		GetSelectedDirectory	();
	BOOL		SetSelectedDirectory	(const CString&);
	CString		GetSelectedFile			();

	virtual void OnLocalDrvChangeMsg(const CString&) {}
	virtual void OnNetwkDrvChangeMsg(const CString&) {}

	void		ShowRemoteDrive			(BOOL = TRUE);

	BOOL		AreMappedNetDrivesShown		() const { return (m_nShowFlags & DIRTREECTRL_SHOW_MAPPEDNETDRIVE);}
	BOOL		AreNetDrivesShown			() const { return (m_nShowFlags & DIRTREECTRL_SHOW_REMOTEDRIVE);}
	BOOL		AreFilesShown				() const { return (m_nShowFlags & DIRTREECTRL_SHOW_FILES);}
	BOOL		AreHiddenFilesShown			() const { return AreFilesShown() && (m_nShowFlags & DIRTREECTRL_SHOW_HIDDENFILES);}
	BOOL		AreSubDirsShownBeforeFiles	() const { return AreFilesShown() && (m_nShowFlags & DIRTREECTRL_SHOW_SUBDIRSBEFOREFILES);}

private:
	void		CreateMonitoringThread	(const CString&, BOOL bNetwkDrv = FALSE);

protected:
	void		LoadDrivesInfo			(LPCTSTR lpszInitialPath = NULL);
	void		ClearControl			(BOOL = TRUE);

	UINT		AddMyComputerDriveNodes	(HTREEITEM);
	BOOL		AddDriveNode			(HTREEITEM, const CString&);
	BOOL		AddNetwkDriveNodes		(HTREEITEM, DWORD dwScope);
	UINT		AddNetwkSubNodes		(HTREEITEM);
	UINT		AddDirectoryNodes		(HTREEITEM, CString&);
	void		SetButtonState			(HTREEITEM, const CString&);
	void		UpdateButtonState		(HTREEITEM, const CString&);
	BOOL		HasSubdirectory			(const CString&);
	BOOL		HasSubNodes				(const CString&);
	BOOL		IsDriveNode				(HTREEITEM);
	void		AddDummyNode			(HTREEITEM);
	void		RemoveDummyNode			(HTREEITEM);
	void		DeleteChildren			(HTREEITEM);
	BOOL		CanAccessFolder(LPCTSTR folderName, DWORD genericAccessRights);

	HTREEITEM	GetDriveNode			(HTREEITEM);

	BOOL		IsMediaValid			(const CString&);
	BOOL		IsPathValid				(const CString&);

	CString		GetPathFromItem			(HTREEITEM);
	BOOL		SelectItemFromPath		(LPCTSTR);
	HTREEITEM	SearchSiblingItems		(HTREEITEM, const CString&);

	// Generated message map functions
	//{{AFX_MSG(CDirTreeCtrl)
	afx_msg void	OnItemExpanding		(NMHDR*, LRESULT*);
	afx_msg void	OnItemExpanded		(NMHDR*, LRESULT*);
	afx_msg LRESULT OnLocalDrvChange	(WPARAM, LPARAM);
	afx_msg LRESULT OnNetwkDrvChange	(WPARAM, LPARAM);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CChooseFileDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CChooseDirDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CChooseDirDlg)
private:
	CString			m_strInitialPath;
	CString			m_strSelectedDir;

protected:
	CDirTreeCtrl	m_DirTreeCtrl;

public:
	CChooseDirDlg(LPCTSTR lpszInitialPath = NULL); 

public:
	CString	GetSelectedDir() {return m_strSelectedDir; }
	virtual BOOL OnInitDialog	();

	// Generated message map functions
	//{{AFX_MSG(CChooseFileDlg)
	afx_msg void OnDirTreeSelChanged(NMHDR*, LRESULT*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CChooseFileDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CChooseFileDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CChooseFileDlg)
private:
	LPCTSTR			m_lpszInitialFile;
	CString			m_strSelectedFile;

protected:
	CDirTreeCtrl	m_DirTreeCtrl;

public:
	CChooseFileDlg(LPCTSTR lpszInitialFile = NULL); 

public:
	CString	GetSelectedFile() {return m_strSelectedFile; }
	virtual BOOL OnInitDialog	();

	// Generated message map functions
	//{{AFX_MSG(CChooseFileDlg)
	afx_msg void OnDirTreeSelChanged(NMHDR*, LRESULT*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
#include "endh.dex"
