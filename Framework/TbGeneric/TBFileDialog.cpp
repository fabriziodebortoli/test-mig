// TBFileDialog.cpp : implementation file
//

#include "stdafx.h"
#include "TBFileDialog.h"
#include "wndobjdescription.h"
#include "dialogs.hjson" //JSON AUTOMATIC UPDATE



BEGIN_MESSAGE_MAP(CTBDownloadFileButton, CBCGPButton)
	
END_MESSAGE_MAP()



BEGIN_MESSAGE_MAP(CTBUploadFileButton, CBCGPButton)
	ON_MESSAGE		(UM_SET_UPLOADED_FILE_PATH,		OnSetUploadedFilePath)
END_MESSAGE_MAP()



//-----------------------------------------------------------------------------
LRESULT CTBUploadFileButton::OnSetUploadedFilePath(WPARAM wParam, LPARAM lParam)
{
	CString* pString = (CString*)wParam; 
  
	m_strFullFilePath.SetString(pString->GetBuffer());

	delete pString; 
	return 0L;
}



#ifdef CFileDialog
#undef CFileDialog
#endif

IMPLEMENT_DYNAMIC(CTBFileDialog, CFileDialog)

//-----------------------------------------------------------------------------
CTBFileDialog::CTBFileDialog(	BOOL bOpenFileDialog, // TRUE for FileOpen, FALSE for FileSaveAs
								LPCTSTR lpszDefExt,
								LPCTSTR lpszFileName,
								DWORD dwFlags,
								LPCTSTR lpszFilter,
								CWnd* pParentWnd,
								DWORD dwSize,
								BOOL bVistaStyle)
: CFileDialog(bOpenFileDialog, lpszDefExt, lpszFileName, dwFlags, lpszFilter, pParentWnd, dwSize, bVistaStyle),

  m_lpszDefExt(lpszDefExt)
{
	//prendo il nome con l'estensione perche' il alcuni casi dal gestionale passano lpszDefExt vuota
	//e il nome del file con l'estensione (lo fanno perche' altrimenti la fileDialog visualizza nella 
	//folder di salvataggio solo i file con quelal estensione)
	m_sFileName = GetNameWithExtension(lpszFileName);
}

//-----------------------------------------------------------------------------
INT_PTR CTBFileDialog::DoModal()
{
	if ( AfxIsRemoteInterface())
	{
		if (!m_bOpenFileDialog)
		{
			CTBFileDownloadDialog dlg(m_lpszDefExt, m_sFileName);
			INT_PTR res = dlg.DoModal();
			//salvo il nome del file completo con tutto il percorso
			m_sFullPath = dlg.m_sFullDirectoryPath + _T("\\") + dlg.GetFileName();
			return res;
		}
		else //todo uploadFileDialog
		{
			CTBFileUploadDialog dlg;
			INT_PTR res = dlg.DoModal();
			m_sFullPath = dlg.m_UploadButton.m_strFullFilePath;
			return res;
		}
	}
	else
	{
		return __super::DoModal();
	}
	return IDCANCEL;
}

//-----------------------------------------------------------------------------
CString CTBFileDialog::GetPathName() const
{
	if (AfxIsRemoteInterface())
		return m_sFullPath;

	return __super::GetPathName();
}

IMPLEMENT_DYNAMIC(CTBFileDownloadDialog, CLocalizableDialog)

///////////////////////////////////////////////////////////////////////////////
CTBFileDownloadDialog::CTBFileDownloadDialog (CString sFileExt, CString sFileName)
	:
	CLocalizableDialog (IDD_WEB_FILE_SAVE),
	m_sFileExt (sFileExt),
	m_sFileName (sFileName)
{
}

//Ritorna il nome del file (con eventuale estensione) digitato dall'utente nella casella di testo
//-----------------------------------------------------------------------------
CString CTBFileDownloadDialog::GetFileName()
{
	return m_sFileNameWithExt;
}
//-----------------------------------------------------------------------------
BOOL CTBFileDownloadDialog::OnInitDialog	()
{
	BOOL bResult = __super::OnInitDialog();

	m_editFilename.SubclassDlgItem(IDC_EDIT_FILENAME, this);
	
	//controllo se il nome di file ha gia l'estensione, lo uso cosi come e'
	//altrimenti gli concateno l'estensione passata alla filedialog
	CString ext = GetExtension(m_sFileName);
	if (!ext.IsEmpty())
	{
		m_sFileNameWithExt = m_sFileName;
	}
	else
	{
		m_sFileNameWithExt = m_sFileName + _T(".") + m_sFileExt;
	}

	m_editFilename.SetWindowText(m_sFileNameWithExt);

	VERIFY(m_bOkButton.SubclassDlgItem(IDOK, this));

	CGuid aGuid;
	aGuid.GenerateGuid();
	m_sFullDirectoryPath = CWndObjDescription::GetTempImagesPath(CString(aGuid));
	m_bOkButton.m_strFolderPath = CString(aGuid);
	
	//creo la cartella, perche sicuramente non esiste (l'ultima folder del path e' quella generata con nome = Guid) 
	CreateDirectory(m_sFullDirectoryPath);
	
	return bResult;
}

//-----------------------------------------------------------------------------
void CTBFileDownloadDialog::EndDialog(int nResult)
{
	m_editFilename.GetWindowText(m_sFileNameWithExt);
	__super::EndDialog(nResult);
}		

BEGIN_MESSAGE_MAP(CTBFileDownloadDialog, CLocalizableDialog)
END_MESSAGE_MAP()


//=============================================================================

IMPLEMENT_DYNAMIC(CTBFileUploadDialog, CLocalizableDialog)

///////////////////////////////////////////////////////////////////////////////
CTBFileUploadDialog::CTBFileUploadDialog ()
	:
	CLocalizableDialog (IDD_WEB_FILE_UPLOAD)
{
}


//-----------------------------------------------------------------------------
BOOL CTBFileUploadDialog::OnInitDialog	()
{
	BOOL bResult = __super::OnInitDialog();


	VERIFY(m_UploadButton.SubclassDlgItem(IDOK, this));

	
	return bResult;
}


	

