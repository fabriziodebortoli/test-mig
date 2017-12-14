#pragma once

#include "LocalizableObjs.h"


#include "beginh.dex"

// CTBDownloadFileButtonOk
class CTBDownloadFileButton : public CBCGPButton
{
	DECLARE_MESSAGE_MAP();
public:
	CString	m_strFolderPath; //folder univoco (l'ultima sub folder ha come nome un guid) dove verra salvato il file

};

// CTBUploadFileButtonOk
class CTBUploadFileButton : public CBCGPButton
{
	afx_msg	LRESULT OnSetUploadedFilePath(WPARAM wParam, LPARAM lParam);
	
	DECLARE_MESSAGE_MAP();

public:
	CString	m_strFullFilePath; //path dove e' stato messo il file uploadato da web
};

////CTBFileDialog
class TB_EXPORT CTBFileDialog : public CFileDialog
{
	DECLARE_DYNAMIC(CTBFileDialog)
	LPCTSTR m_lpszDefExt;
	CString m_sFileName;
	CString m_sFullPath;

public:
	CTBFileDialog(	BOOL bOpenFileDialog, // TRUE for FileOpen, FALSE for FileSaveAs
					LPCTSTR lpszDefExt = NULL,
					LPCTSTR lpszFileName = NULL,
					DWORD dwFlags = OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT,
					LPCTSTR lpszFilter = NULL,
					CWnd* pParentWnd = NULL,
					DWORD dwSize = 0,
					BOOL bVistaStyle = TRUE);

	INT_PTR DoModal();
	CString GetPathName() const;
};

//CTBFileDownloadDialog
class TB_EXPORT CTBFileDownloadDialog : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CTBFileDownloadDialog)
	
	CTBDownloadFileButton		m_bOkButton;
	CBCGPEdit					m_editFilename;
	CString						m_sFileExt;
	CString						m_sFileName;
	CString						m_sFileNameWithExt;
	CString						m_sFullDirectoryPath;

public:
	CTBFileDownloadDialog(CString sFileExt, CString sFileName);

	CString GetFileName();
	
	DECLARE_MESSAGE_MAP();

	virtual BOOL OnInitDialog	();
	virtual void	EndDialog	(int nResult);
};


//CTBFileDownloadDialog
class TB_EXPORT CTBFileUploadDialog : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CTBFileUploadDialog)
	
	CTBUploadFileButton		m_UploadButton;

public:
	CTBFileUploadDialog();

	virtual BOOL OnInitDialog	();
};


#include "endh.dex"