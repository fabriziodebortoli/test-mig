
#include "stdafx.h"

//................................. library
#include <TbGeneric\linefile.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\WebServiceStateObjects.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\commands.hrc>
#include <TbGenlibUI\TbExplorer.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

//................................. locals
#include "paddoc.h"

//................................. resources
#include "paddoc.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define DEFAULT_TABSTOPS 4
//==============================================================================
//          Class CEditorTabStopDlg definition
//==============================================================================
//
//==============================================================================
class CEditorTabStopDlg: public CParsedDialog
{
	DECLARE_DYNCREATE(CEditorTabStopDlg)
protected:
	CIntEdit	m_TabStopEdit;

public:
	CEditorTabStopDlg(CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
};
IMPLEMENT_DYNCREATE(CEditorTabStopDlg, CParsedDialog)

//==============================================================================
//          Class CEditorTabStopDlg implementation
//==============================================================================

//------------------------------------------------------------------------------
CEditorTabStopDlg::CEditorTabStopDlg(CWnd* aParent)
	:
	CParsedDialog	(IDD_EDITOR_TAB_STOP, aParent),
	m_TabStopEdit	(BTN_SPIN_ID)
{
}


//------------------------------------------------------------------------------
void CEditorTabStopDlg::OnOK()
{
	EndDialog(IDOK);
}


//------------------------------------------------------------------------------
BOOL CEditorTabStopDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	if (!m_TabStopEdit.SubclassEdit(IDC_EDITOR_TAB_STOP, this))
		return FALSE;

	m_TabStopEdit.SetRange(2, 20);
	m_TabStopEdit.SetValue(DEFAULT_TABSTOPS);

	return TRUE;  // return TRUE  unless you set the focus to a control
}




/////////////////////////////////////////////////////////////////////////////
// 							CPadDoc
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------                      
IMPLEMENT_DYNCREATE(CPadDoc, CBaseDocument)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPadDoc, CBaseDocument)
	//{{AFX_MSG_MAP(CPadDoc)
	ON_COMMAND(ID_FILE_SAVE,  				OnFileSave)
	ON_COMMAND(ID_FILE_SAVE_AS,				OnFileSaveAs)
	ON_COMMAND(ID_EDITOR_TAB_STOP,			OnTabStop)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE,		OnUpdateFileSave)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_AS,	OnUpdateFileSave)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CPadDoc::OnNewDocument()
{
	m_bSaveAs = TRUE;
	m_FileFormat = CLineFile::ANSI;

	// Ok se ci sono abbastanza risorse e se l'ancestor e' bOk
	m_bAborted = 
			!ResourceAvailable() ||
			!CBaseDocument::OnNewDocument();

	return !m_bAborted;
}

//la classe CArchive (che si basa a sua volta su CFile) non ragiona in UNICODE,
//pertanto occorre avvalersi della classe 
//CLineFile per effettuare lettura e salvataggio del file
//direttamente nelle due funzioni OnSaveDocument e OnOpenDocument.
//In questo modo è anche possibile gestire formati UTF-8, UTF-16 little e big endian
//-----------------------------------------------------------------------------
#define LINE_SIZE 256

BOOL CPadDoc::OnOpenDocument(LPCTSTR pszPathName)
{ 
	if (pszPathName != NULL)
		m_pDocInvocationInfo = (DocInvocationInfo*) pszPathName;

	// Ok se ci sono abbastanza risorse e se l'ancestor e' bOk 
	m_bAborted = !ResourceAvailable();
	if(!m_bAborted && m_pDocInvocationInfo)
 	{
		TRY
		{
			CLineFile aFile(m_pDocInvocationInfo->m_lpszFileName, CFile::modeRead | CFile::typeText);
			CEdit& Edit = ((CPadView*)m_viewList.GetHead())->GetEditCtrl();
			CString strText;
			TCHAR buff[LINE_SIZE];
			while(aFile.ReadString(buff, LINE_SIZE, TRUE))
				strText+=buff;
			Edit.SetWindowText (strText);
			m_FileFormat = aFile.GetFormat();
			aFile.Close();
		}
		CATCH(CException, e)
		{
			m_bAborted = TRUE;
		}
		END_CATCH
	}

	// mi tengo il namespace
	if (!m_bAborted && pszPathName)
		GetNamespace() = AfxGetPathFinder()->GetNamespaceFromPath(pszPathName);

	m_bSaveAs = FALSE;

	return !m_bAborted;
}


//la classe CArchive (che si basa a sua volta su CFile) non ragiona in UNICODE,
//pertanto occorre avvalersi della classe 
//CLineFile per effettuare lettura e salvataggio del file
//direttamente nelle due funzioni OnSaveDocument e OnOpenDocument.
//In questo modo è anche possibile gestire formati UTF-8, UTF-16 little e big endian
//-----------------------------------------------------------------------------
BOOL CPadDoc::OnSaveDocument(LPCTSTR pszPathName)
{	
	TRY
	{
		CLineFile aFile(pszPathName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
		aFile.SetFormat(m_FileFormat);
		CEdit& Edit = ((CPadView*)m_viewList.GetHead())->GetEditCtrl();
		CString strText;
		Edit.GetWindowText (strText);
		aFile.WriteString(strText);
		aFile.Close();
	}
	CATCH(CException, e)
	{
		return FALSE;
	}
	END_CATCH	
	
	SetModifiedFlag(FALSE);

	return TRUE;
}

//------------------------------------------------------------------------------
void CPadDoc::OnUpdateFileSave(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//------------------------------------------------------------------------------
void CPadDoc::OnFileSave()
{	
	m_bSaveAs = FALSE;
	CBaseDocument::OnFileSave();
}

//------------------------------------------------------------------------------
void CPadDoc::OnFileSaveAs()
{
	m_bSaveAs = TRUE;
	CBaseDocument::OnFileSave();
	//CBaseDocument::OnFileSaveAs();
}

//la classe CArchive (che si basa a sua volta su CFile) non ragiona in UNICODE,
//pertanto occorre avvalersi della classe 
//CLineFile per effettuare lettura e salvataggio del file
//direttamente nelle due funzioni OnSaveDocument e OnOpenDocument.
//In questo modo è anche possibile gestire formati UTF-8, UTF-16 little e big endian
//-----------------------------------------------------------------------------
void CPadDoc::Serialize(CArchive& ar)
{
	//((CPadView*)m_viewList.GetHead())->SerializeRaw(ar);
}

//------------------------------------------------------------------------------
BOOL CPadDoc::DoSave(LPCTSTR pszPathName, BOOL bReplace /*=TRUE*/)
{
	// il save semplice non chiede la dialog
	if (pszPathName && GetNamespace().IsEmpty())
		GetNamespace() = AfxGetPathFinder()->GetNamespaceFromPath(pszPathName);

	if (!GetNamespace().IsValid() || !pszPathName)
	{
		AddOnApplication*	pAddOnApp	= AfxGetBaseApp()->GetMasterAddOnApp();
		ASSERT(pAddOnApp);

		AddOnModsArray*		pMods		= NULL;
		
		if (pAddOnApp)
			pMods = pAddOnApp->m_pAddOnModules;
		ASSERT (pMods);

		CTBNamespace aNs (CTBNamespace::TEXT, pAddOnApp->m_strAddOnAppName + CTBNamespace::GetSeparator () +  pMods->GetAt(0)->GetModuleName());
		CString sDefaultPath (AfxGetPathFinder()->GetModuleFilesPath (aNs, CPathFinder::STANDARD));
		aNs.SetPathInside(GetName(sDefaultPath));
		
		CStringArray aExts;
		AfxGetPathFinder()->GetObjectSearchExtensions(aNs, &aExts, TRUE);
		CString sName = GetTitle() + (aExts.GetSize() ? aExts.GetAt(0) : _T(""));
		aNs.SetObjectName(CTBNamespace::TEXT, sName);
		
		GetNamespace() = aNs;	
	}

	if (AfxGetLoginInfos()->m_bAdmin || !pszPathName || m_bSaveAs)
	{
		BOOL bShowDialog = m_bSaveAs || ( AfxGetLoginInfos()->m_bAdmin && !pszPathName);

		CTBExplorer tbExplorer(CTBExplorer::SAVE, GetNamespace(), TRUE, 0, CTBExplorer::DEFAULT, !bShowDialog);
		if(!tbExplorer.Open())
			return FALSE;

		CStringArray aNewName;
		tbExplorer.GetSavePath(aNewName);

		if (!aNewName.GetSize())
			return TRUE;

		for (int n = 0 ; n < aNewName.GetSize() ; n++	)
		{
			if (aNewName.GetAt(n).IsEmpty())
				return FALSE;

			OnSaveDocument(aNewName.GetAt(n));

			GetNamespace() = AfxGetPathFinder()->GetNamespaceFromPath(aNewName.GetAt(n));;	
		}

		// Reset the title and change the document name
		if (bReplace)
			SetPathName(aNewName.GetAt(aNewName.GetUpperBound()));
	}
	else
		OnSaveDocument(pszPathName);

	return TRUE;
}

//------------------------------------------------------------------------------
void CPadDoc::SetPathName(LPCTSTR lpszPathName, BOOL /*TRUE*/)
{
	if (lpszPathName == NULL)
		return;

	DocInvocationInfo* pInfo = (DocInvocationInfo*) lpszPathName;

	__super::SetPathName(pInfo->m_lpszFileName);
}

//------------------------------------------------------------------------------
BOOL CPadDoc::SaveModified()
{
	return CBaseDocument::SaveModified();
}


//------------------------------------------------------------------------------
void CPadDoc::OnTabStop()
{
	CEditorTabStopDlg tabDlg;

	if (tabDlg.DoModal() == IDOK)
		UpdateAllViews (NULL);
}

/////////////////////////////////////////////////////////////////////////////
// 							CPadView
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------                      
IMPLEMENT_DYNCREATE(CPadView, CEditView)

// in dialog unit (1/4 della dimensione del carattere medio)
//-----------------------------------------------------------------------------                      
CPadView::CPadView()
{
}

// default 4 character positions	
//-----------------------------------------------------------------------------                      
void CPadView::OnInitialUpdate()
{
	SetTabStops(DEFAULT_TABSTOPS);
}

//-----------------------------------------------------------------------------                      
void CPadView::OnUpdate (CView* pSender, LPARAM lHint, CObject* pHint)
{
	OnInitialUpdate();
}


/////////////////////////////////////////////////////////////////////////////
// 							CPadFrame
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BOOL CPadFrame::LoadFrame(UINT nIDResource, DWORD dwDefaultStyle /*= WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE*/, CWnd* pParentWnd /*= NULL*/, CCreateContext* pContext /*= NULL*/)
{
	if (!__super::LoadFrame(nIDResource, dwDefaultStyle, pParentWnd, pContext))
		return FALSE;
	CreateMenu();
	return TRUE;
}
//-----------------------------------------------------------------------------
void CPadFrame::CreateMenu()
{
	CMenu menu;
	menu.CreateMenu();
	/*
	 POPUP "&File"
    BEGIN
        MENUITEM "&New Report",               ID_APP_NEW_REPORT
        MENUITEM "&Open Report",                ID_APP_OPEN_REPORT
        MENUITEM "&Close\tCtrl+F4",            ID_FILE_CLOSE
        MENUITEM SEPARATOR
        MENUITEM "&Save\tCtrl+4",              ID_FILE_SAVE
        MENUITEM "Sa&ve as...",          ID_FILE_SAVE_AS
        MENUITEM SEPARATOR
        MENUITEM "Print p&review",        ID_FILE_PRINT_PREVIEW
        MENUITEM "Pr&int\tCtrl+P",          ID_FILE_PRINT
        MENUITEM "Pri&nt setup...",       ID_FILE_PRINT_SETUP
        MENUITEM "Page &setup...",          ID_FILE_PAGE_SETUP
        MENUITEM SEPARATOR
        MENUITEM "E&xit",               ID_APP_EXIT
    END
	*/
	CMenu fileMenu;
	fileMenu.CreatePopupMenu();
	fileMenu.AppendMenu(MF_STRING, ID_APP_NEW_REPORT, _TB("&New Report"));
	fileMenu.AppendMenu(MF_STRING, ID_APP_OPEN_REPORT, _TB("&Open Report"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_CLOSE, _TB("&Close\tCtrl+F4"));
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_FILE_SAVE, _TB("&Save\tCtrl+4"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_SAVE_AS, _TB("Sa&ve as..."));
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PRINT_PREVIEW, _TB("Print p&review"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PRINT, _TB("Pr&int\tCtrl+P"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PRINT_SETUP, _TB("Pri&nt setup..."));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PAGE_SETUP, _TB("Page &setup..."));
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_APP_EXIT, _TB("E&xit"));
	/*
	 POPUP "&Edit"
    BEGIN
        MENUITEM "&Undo\tCtrl+Z",    ID_EDIT_UNDO
        MENUITEM SEPARATOR
        MENUITEM "&Cut\tCtrl+X",             ID_EDIT_CUT
        MENUITEM "&Copy\tCtrl+C",              ID_EDIT_COPY
        MENUITEM "&Paste\tCtrl+V",            ID_EDIT_PASTE
        MENUITEM "&Delete",              ID_EDIT_CLEAR
        MENUITEM SEPARATOR
        MENUITEM "&Find...",                   ID_EDIT_FIND
        MENUITEM "&Find Next\tF3",       ID_EDIT_REPEAT
        MENUITEM "&Replace",            ID_EDIT_REPLACE
        MENUITEM SEPARATOR
        MENUITEM "&Select all",            ID_EDIT_SELECT_ALL
    END
	*/
	CMenu editMenu;
	editMenu.CreatePopupMenu();
	editMenu.AppendMenu(MF_STRING, ID_EDIT_UNDO, _TB("&Undo\tCtrl+Z"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_EDIT_CUT, _TB("&Cut\tCtrl+X"));
	editMenu.AppendMenu(MF_STRING, ID_EDIT_COPY, _TB("&Copy\tCtrl+C"));
	editMenu.AppendMenu(MF_STRING, ID_EDIT_PASTE, _TB("&Paste\tCtrl+V"));
	editMenu.AppendMenu(MF_STRING, ID_EDIT_CLEAR, _TB("&Delete"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_EDIT_FIND, _TB("&Find..."));
	editMenu.AppendMenu(MF_STRING, ID_EDIT_REPEAT, _TB("&Find Next\tF3"));
	editMenu.AppendMenu(MF_STRING, ID_EDIT_REPLACE, _TB("&Replace"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_EDIT_SELECT_ALL, _TB("&Select all"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)editMenu.Detach(), _TB("&Edit"));


	/*
	POPUP "&Options"
    BEGIN
        MENUITEM "&Tabulation",               ID_EDITOR_TAB_STOP
    END
	*/

	CMenu optionsMenu;
	optionsMenu.CreatePopupMenu();
	optionsMenu.AppendMenu(MF_STRING, ID_EDITOR_TAB_STOP, _TB("&Tabulation"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)optionsMenu.Detach(), _TB("&Options"));


	/*
	 POPUP "&Windows"
    BEGIN
        MENUITEM "Tool&Bar",            ID_VIEW_TOOLBAR
        MENUITEM "StatusB&ar",                ID_VIEW_STATUS_BAR
    END
	*/
	CMenu windowsMenu;
	windowsMenu.CreatePopupMenu();
	windowsMenu.AppendMenu(MF_STRING, ID_VIEW_STATUS_BAR, _TB("S&tatus Bar"));
	windowsMenu.AppendMenu(MF_STRING, ID_VIEW_TOOLBAR, _TB("Tool&Bar"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)windowsMenu.Detach(), _TB("W&indows"));
	/*
	  
    POPUP "&?"
    BEGIN
        MENUITEM "&Index",                     ID_HELP_INDEX
	    POPUP "Help&s on line"
		BEGIN
		   MENUITEM "-",	ID_HELP_LIST
		END
    END
	*/
	CMenu qMenu;
	qMenu.CreatePopupMenu();
	qMenu.AppendMenu(MF_STRING, ID_HELP_INDEX, _TB("&Index"));

	CMenu popup;
	popup.CreatePopupMenu();
	popup.AppendMenu(MF_STRING, ID_HELP_LIST, _T("-"));
	qMenu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _TB("Help&s on line"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)qMenu.Detach(), _TB("&?"));

	VERIFY(SetMenu(&menu));
	m_hMenu = menu.Detach();
}