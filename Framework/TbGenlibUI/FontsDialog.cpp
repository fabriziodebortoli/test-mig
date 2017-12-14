#include "stdafx.h"

#include <stdlib.h>
#include <time.h>
#include <float.h>

#include <TbGeneric\FontsTable.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbClientCore\ClientObjects.h>
#include <TbParser\FontsParser.h>
#include <TbGeneric\SettingsTable.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\dirtreectrl.h>
#include <TbGenlibManaged\HelpManager.h>

#include <TbGenlibUI\ContextSelectionDialog.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "FontsDialog.h"
#include "FontsDialog.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
static const TCHAR szCurrentReport[] = _T("<Report>");	//Local styles of current report
static const TCHAR szTemplateCurrentReport[] = _T("<Template>"); //Local styles loaded from of current report template

static const TCHAR szHelpNamespace[] = _T("Document.Framework.TbGenlibUI.TbGenlibUI.ExecOpenFont");

//////////////////////////////////////////////////////////////////////////////
//============================================================================
//		CTreeFontItemRef implementation
//			per localizzazione (parte non localizzata)
//============================================================================
CTreeFontItemRef::CTreeFontItemRef(CString strName)
{
	m_strName = strName;
};

//==========================================================================
//							CTreeFontDialog
//==========================================================================
BEGIN_MESSAGE_MAP(CTreeFontDialog, CTBTreeCtrl)
	ON_WM_KEYDOWN		()	
	ON_WM_KEYUP			()
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,		OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,			OnItemEndEdit)
	ON_COMMAND			(ID_OPENFONT,				OnOpen)
	ON_COMMAND			(ID_ESCAPEMENT_FONT,		OnSetEscapement)
	ON_COMMAND			(ID_DELETEFONT,			OnDelete)
	ON_COMMAND			(ID_COPYFONT,				OnCopy)
	ON_COMMAND			(ID_PASTEFONT,				OnPaste)
	ON_COMMAND			(ID_RENAMEFONT,			OnRename)
	ON_COMMAND			(ID_CUTFONT,				OnCut)
	ON_COMMAND			(ID_APPAREAFONT,			OnContextArea)
	ON_COMMAND			(ID_REFRESHFONT,			OnRefreshFont)
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
END_MESSAGE_MAP()
//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTreeFontDialog, CTBTreeCtrl)
//--------------------------------------------------------------------------
CTreeFontDialog::CTreeFontDialog()
	:
	m_bAfterCtrl	(FALSE)
{}

//---------------------------------------------------------------------------
void CTreeFontDialog::ExpandAll(HTREEITEM hItem, UINT nCode)
{
	Expand(hItem, nCode);
	HTREEITEM hChildItem = GetChildItem(hItem);
	while (hChildItem) 
	{
		ExpandAll(hChildItem, nCode);
		hChildItem = GetNextSiblingItem(hChildItem);
	}
}

//---------------------------------------------------------------------------
void CTreeFontDialog::ExpandAll(UINT nCode)
{
	HTREEITEM hItem = GetRootItem();

	while(hItem)
	{
		Expand(hItem, nCode);
		HTREEITEM hChildItem = GetChildItem(hItem);
		while (hChildItem) 
		{
			ExpandAll(hChildItem, nCode);
			hChildItem = GetNextSiblingItem(hChildItem);
		}
		hItem = GetNextSiblingItem(hItem);
	}
}

//---------------------------------------------------------------------------
void CTreeFontDialog::OnRButtonDown(UINT nFlags, CPoint point) 
{		
	__super::OnRButtonDown(nFlags, point);
	HTREEITEM hItem = HitTest(point);
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTreeFontDialog::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	HTREEITEM hItemToSelect = GetSelectedItem();
	
	if (!hItemToSelect)
		return;

	CLocalizableMenu menu;
	CMenu*			pPopup	= NULL;
	CFontStylesDlg* pDlg	= (CFontStylesDlg*) GetParent();
	/*
IDR_MENU_FONTDIALOG MENU
BEGIN
    POPUP "Menu"
    BEGIN
        MENUITEM "Properties",                  IDM_OPENFONT
        MENUITEM "Apply in ...",                IDM_APPAREAFONT
        MENUITEM "Cut      (Ctrl+X)",           IDM_CUTFONT
        MENUITEM "Copy   (Ctrl+C)",             IDM_COPYFONT
        MENUITEM "Paste (Ctrl+V)",              IDM_PASTEFONT
        MENUITEM SEPARATOR
        MENUITEM "Delete",                      IDM_DELETEFONT
        MENUITEM "Rename",                      IDM_RENAMEFONT
        MENUITEM SEPARATOR
        MENUITEM "Refresh",                     IDM_REFRESHFONT
    END
END
	*/
	menu.CreateMenu();
	CMenu popup;
	popup.CreatePopupMenu();
	popup.AppendMenu(MF_STRING, ID_OPENFONT, _TB("Properties"));
	popup.AppendMenu(MF_STRING, ID_APPAREAFONT, _TB("Apply in ..."));
	popup.AppendMenu(MF_STRING, ID_CUTFONT, _TB("Cut (Ctrl+X)"));
	popup.AppendMenu(MF_STRING, ID_COPYFONT, _TB("Copy (Ctrl+C)"));
	popup.AppendMenu(MF_STRING, ID_PASTEFONT, _TB("Paste (Ctrl+V)"));
	popup.AppendMenu(MF_SEPARATOR);
	popup.AppendMenu(MF_STRING, ID_DELETEFONT, _TB("Delete"));
	popup.AppendMenu(MF_STRING, ID_RENAMEFONT, _TB("Rename"));
	popup.AppendMenu(MF_SEPARATOR);
	popup.AppendMenu(MF_STRING, ID_REFRESHFONT, _TB("Refresh"));
	menu.AppendMenu(MF_STRING | MF_POPUP, (UINT_PTR)popup.Detach(), _TB("Menu"));
	ASSERT(menu);	
	pPopup = menu.GetSubMenu(0);
	ASSERT(pPopup);		
	if (!pPopup)
		return;

	if (!pDlg->CanCopy())
		pPopup->EnableMenuItem(ID_COPYFONT,	MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	if (!pDlg->CanPaste())
		pPopup->EnableMenuItem(ID_PASTEFONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
	
	if (!pDlg->CanOpen())
	{
		pPopup->EnableMenuItem(ID_OPENFONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		//pPopup->EnableMenuItem(IDM_ESCAPEMENT_FONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
	}

	if (!pDlg->CanDelete())
	{
		pPopup->EnableMenuItem(ID_DELETEFONT,	MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		pPopup->EnableMenuItem(ID_CUTFONT,		MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		pPopup->EnableMenuItem(ID_APPAREAFONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		pPopup->EnableMenuItem(ID_RENAMEFONT,	MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
	}
	
	if (!pDlg->CanApplyContextArea())
		pPopup->EnableMenuItem(ID_APPAREAFONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	if (pDlg->GetItemLevel(hItemToSelect) > 0)
		pPopup->EnableMenuItem(ID_REFRESHFONT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	ClientToScreen(&mousePos);			

	pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON,	mousePos.x, mousePos.y, this);		
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnItemBeginEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	CFontStylesDlg* pDlg = (CFontStylesDlg*) GetParent();
	if (!pDlg->CanCopy())		// se posso copiare allora...
	{
		*pResult = 1;
		return;
	}	

	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnItemEndEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	if (!lpDispInfo->item.pszText)
	{
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		SetFocus	();
		return;
	}	

	if (!lpDispInfo->item.pszText[0])
	{
		*pResult = 0;
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		SetFocus	();
		return;
	}

	CFontStylesDlg* pDlg = (CFontStylesDlg*) GetParent();
	pDlg->RenameStyle(lpDispInfo->item.pszText);

	*pResult = 0;
	SelectItem	(lpDispInfo->item.hItem);
	SetFocus	();
}

//---------------------------------------------------------------------
void CTreeFontDialog::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	CFontStylesDlg* pDlg = (CFontStylesDlg*) GetParent();
	
	if (nChar == 113 && pDlg->CanPaste())
		OnRename();

	if (nChar == 46 && pDlg->CanDelete())
		OnDelete();

	if (nChar == VK_CONTROL)
		m_bAfterCtrl = TRUE;
	
	if (nChar == 67 && m_bAfterCtrl)
	{
		OnCopy();
		m_bAfterCtrl = FALSE;
	}

	if (nChar == 86 && m_bAfterCtrl && pDlg->CanPaste())
	{
		OnPaste();
		m_bAfterCtrl = FALSE;
	}

	if (nChar == 88 && m_bAfterCtrl && pDlg->CanDelete())
	{
		OnCut();
		m_bAfterCtrl = FALSE;
	}

	__super::OnKeyDown(nChar, nRepCnt, nFlags);	
}

//---------------------------------------------------------------------
void CTreeFontDialog::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	m_bAfterCtrl = FALSE;
	__super::OnKeyUp(nChar, nRepCnt, nFlags);
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnOpen() 
{
	CFontStylesDlg* pFontDlg =  (CFontStylesDlg*) GetParent();
	pFontDlg->OpenStyle();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnSetEscapement() 
{
	//TODO
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnContextArea() 
{
	CFontStylesDlg* pFontDlg =  (CFontStylesDlg*) GetParent();
	pFontDlg->ContextArea();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnRefreshFont()
{
	CFontStylesDlg* pFontDlg =  (CFontStylesDlg*) GetParent();

	pFontDlg->RefreshFontsTable();
	pFontDlg->FillTreeCtrlStyle();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnDelete() 
{
	CFontStylesDlg* pFontDlg =  (CFontStylesDlg*) GetParent();
	pFontDlg->DeleteStyle();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnCopy() 
{
	CFontStylesDlg* pFontDlg =  (CFontStylesDlg*) GetParent();
	pFontDlg->CopyStyle();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnPaste() 
{
	CFontStylesDlg* pFontDlg = (CFontStylesDlg*) GetParent();
	pFontDlg->PasteStyle();
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnRename() 
{
	CFontStylesDlg* pFontDlg = (CFontStylesDlg*) GetParent();
	pFontDlg->OnRemaneLabel();	
}

//----------------------------------------------------------------------------
void CTreeFontDialog::OnCut() 
{
	CFontStylesDlg* pFontDlg = (CFontStylesDlg*) GetParent();
	pFontDlg->m_bFormCut = TRUE;
	pFontDlg->m_hSelCut = pFontDlg->m_treeStyle.GetSelectedItem();
	pFontDlg->CopyStyle();
}

//============================================================================
//		CFontStylesDlg implementation
//============================================================================
IMPLEMENT_DYNAMIC(CFontStylesDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CFontStylesDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CFontStylesDlg)

	ON_NOTIFY			(NM_DBLCLK,			IDC_TREE_FONT,	OnNMDblclkTree)
	ON_NOTIFY			(TVN_SELCHANGED,	IDC_TREE_FONT,	OnTreeSelchanged)	

	ON_CBN_SELCHANGE	(IDC_FONT_COMBOFILTER,	OnComboStyleChanged)

	ON_BN_CLICKED		(IDC_FONT_SAVE,			OnSaveFontStyles)
    ON_BN_CLICKED		(IDC_FONT_USE_PRINTER,	OnShowPrinterFont)
	ON_BN_CLICKED		(ID_FONT_OPEN,			OnOpen)
	ON_BN_CLICKED		(ID_FONT_APPLYIN,		OnContextArea)
	ON_BN_CLICKED		(ID_FONT_CUT,			OnCut)
	ON_BN_CLICKED		(ID_FONT_COPY,			OnCopy)
	ON_BN_CLICKED		(ID_FONT_PASTE,			OnPaste)
	ON_BN_CLICKED		(ID_FONT_DELETE,		OnDelete)
	ON_BN_CLICKED		(ID_FONT_RENAME,		OnRename)
	ON_BN_CLICKED		(ID_FONT_FILTERTREE,	OnFilterTree)
	
	ON_UPDATE_COMMAND_UI	(IDC_FONT_USE_PRINTER,	OnUpdatePrinterFont)
	
	ON_COMMAND			(ID_FONT_HELP, 			OnHelp)
	ON_COMMAND			(ID_APPLY_ALLCOLUM,		OnApplyAllColum)

	ON_WM_CONTEXTMENU	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CFontStylesDlg::CFontStylesDlg
	(
		FontStyleTable&			StyleTable,
		FontIdx&				FontIndex,
		BOOL					bIgnoreIdx,
		CWnd*					pWndParent,
		const CTBNamespace&		NsForWoorm,
		BOOL					bSelezionaAsOk,
		BOOL					bShowDefaultFont /*= FALSE*/
	)
    :
	CParsedDialog				(IDD_FONT_STYLE, pWndParent),
    m_pWndParent				(pWndParent),
	m_SourceStyleTable			(StyleTable),
	m_StyleTable				(StyleTable),
	m_FontIdx					(FontIndex),
	m_bIgnoreIdx				(bIgnoreIdx),
	m_bModified					(TRUE),
	m_bFormCut					(FALSE),
	m_NsForWoorm				(NsForWoorm),
	m_bFromTree					(FALSE),
	m_bShowPrinterFont			(FALSE),
	m_hSelCut					(NULL),
	m_DefaultSel				(NULL),
	m_bFilterTree				(FALSE),
	m_bSelezionaAsOk			(bSelezionaAsOk),
	m_bEnableApplyAll			(FALSE),
	m_bApplyAll					(FALSE),
	m_bShowDefaultFont			(bShowDefaultFont)
{
	// sono in nuovo report
	if (m_NsForWoorm.GetType() == CTBNamespace::DOCUMENT)
		m_NsForWoorm.SetType(CTBNamespace::REPORT);

	m_pFontCopy		= NULL;
	m_sFilterStyle	= _T("");
	m_strPreferredPrinter = _T("");
}

//------------------------------------------------------------------------------
CFontStylesDlg::~CFontStylesDlg()
{
	SAFE_DELETE(m_pFontCopy);
	m_ImageList.DeleteImageList();
}

//----------------------------------------------------------------------------
void CFontStylesDlg::OnCustomizeToolbar()
{
	CString sNs = _T("Framework.TbGenlibUI.TbGenlibUI.FontStyles.");

	m_pToolBar->AddButton(ID_FONT_FILTERTREE, sNs + _T("FilTerTree"), TBIcon(szIconFilterTree, IconSize::TOOLBAR), _TB("Filter"));
	m_pToolBar->AddButton(ID_FONT_OPEN, sNs + _T("Open"), TBIcon(szIconEdit, IconSize::TOOLBAR), _TB("Open"));
	m_pToolBar->AddButton(ID_FONT_APPLYIN, sNs + _T("Applyin"), TBIcon(szIconApplyIn, IconSize::TOOLBAR), _TB("Apply in"));
	m_pToolBar->AddButton(ID_FONT_CUT, sNs + _T("Cut"), TBIcon(szIconCut, IconSize::TOOLBAR), _TB("Cut"));
	m_pToolBar->AddButton(ID_FONT_COPY, sNs + _T("Copy"), TBIcon(szIconCopy, IconSize::TOOLBAR), _TB("Copy"));
	m_pToolBar->AddButton(ID_FONT_PASTE, sNs + _T("Paste"), TBIcon(szIconPaste, IconSize::TOOLBAR), _TB("Paste"));
	m_pToolBar->AddButton(ID_FONT_DELETE, sNs + _T("Delete"), TBIcon(szIconDelete, IconSize::TOOLBAR), _TB("Delete"));
	m_pToolBar->AddButton(ID_FONT_RENAME, sNs + _T("Rename"), TBIcon(szIconRename, IconSize::TOOLBAR), _TB("Rename"));

	m_pToolBar->AddButtonToRight(IDC_FONT_USE_PRINTER,	sNs + _T("PrinterFonts"),	TBIcon(szIconPrinterFont, IconSize::TOOLBAR),		_TB("Printer fonts"), _TB("Printer fonts"));
	
	m_pToolBar->AddButtonToRight(IDOK,					sNs + _T("Select"),			TBIcon(szIconOk, IconSize::TOOLBAR),		_TB("Select"), _TB("Select (Alt + S)" ));
	m_pToolBar->AddButtonToRight(IDC_FONT_SAVE,			sNs + _T("Savestyles"),		TBIcon(szIconSave, IconSize::TOOLBAR),		_TB("Save styles"), _TB("Select (Alt + Enter)"));
	m_pToolBar->AddButtonToRight(IDCANCEL,				sNs + _T("Cancel"),			TBIcon(szIconCancel, IconSize::TOOLBAR),	_TB("Cancel"));

	// Append acceleretor
	AppendAccelerator(IDOK, FALT, 0x73);
	AppendAccelerator(IDC_FONT_SAVE, FALT, VK_RETURN);
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
	EnableToolTips(TRUE);

	RefreshFontsTable();

	BOOL bEnableSave = m_StyleTable.IsModified();
	
	SetToolbarStyle(CParsedDialog::TOP, DEFAULT_TOOLBAR_HEIGHT, FALSE, TRUE);

	// se sono nel caso di woorm
	if (!m_bIgnoreIdx)
	{
		SetWindowText(_TB("Font styles Customizations"));
		if (m_bSelezionaAsOk)
			m_pToolBar->SetText(IDOK, _TB("Ok"));
	}
	else
	{
		m_pToolBar->EnableButton(IDOK);
	}	
	
	m_treeStyle	.SubclassDlgItem(IDC_TREE_FONT,			this);
	m_CmbStyle	.SubclassDlgItem(IDC_FONT_COMBOFILTER,	this);

	LoadImageList	();
	FillComboStyle	();
	OnFilterTree	();

	m_treeStyle.ExpandAll(TVE_EXPAND); 
	
	EnableDisableToolbar();
	m_bFontSave = FALSE;
	m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
	
	m_treeStyle.SetFocus();


	if (!m_NsForWoorm.IsEmpty() && m_NsForWoorm.IsValid() )
	{
		//finding setting file reportname.wrm.config

		CTBNamespace aModNs(CTBNamespace::MODULE, m_NsForWoorm.GetApplicationName() + CTBNamespace::GetSeparator() + m_NsForWoorm.GetModuleName());
		CString sFilename = m_NsForWoorm.GetObjectName() + szSettingsExt;

		DataObj* pSetting = AfxGetSettingValue(aModNs, _T("DefaultPrinter"), _T("DefaultPrinter"), DataStr(), sFilename);
		m_strPreferredPrinter	= pSetting ? pSetting->Str() : _T(""); 
	}
	
	CString strFontUsePrinterText =	m_pToolBar->GetTextToolTip(IDC_FONT_USE_PRINTER);
	CString strPrinterName = m_strPreferredPrinter;

	if (strPrinterName.IsEmpty())
	{
		strPrinterName = GetDefaultPrinter();
	}
	CString strFontUsePrinterTextPrinterName = cwsprintf( _T("%s (%s)"), strFontUsePrinterText, strPrinterName);

	m_pToolBar->SetTextToolTip(IDC_FONT_USE_PRINTER, strFontUsePrinterTextPrinterName);
	
	if (m_bEnableApplyAll)
	{
		INT nId = ID_APPLY_ALLCOLUM;

		CTBToolBarMenu menu;
		menu.CreateMenu();
		menu.AppendMenu(MF_STRING, ID_APPLY_ALLCOLUM, _TB("Apply to all columns"));
		//m_pToolBar->UpdateDropdownMenu(IDOK, &menu);
		

		m_pToolBar->SetDropdown(IDOK, &menu);
		m_pToolBar->EnableAlwaysDropDown(IDOK, FALSE);
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CFontStylesDlg::OnApplyAllColum()
{
	m_bApplyAll = TRUE;
	OnOK();
}

//----------------------------------------------------------------------------
BOOL CFontStylesDlg::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (nID == ID_APPLY_ALLCOLUM && nCode == CN_COMMAND)
		BOOL b = TRUE;

	return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

//----------------------------------------------------------------------------
void CFontStylesDlg::SetDefaultSelection ()
{
	if (m_bIgnoreIdx || !m_DefaultSel)
		return;

	m_treeStyle.SelectItem(m_DefaultSel);
	m_treeStyle.SetItemState(m_DefaultSel, TVIS_SELECTED | TVIS_BOLD , TVIS_SELECTED | TVIS_BOLD);
	m_treeStyle.SetFocus ();
}

//------------------------------------------------------------------------------
void CFontStylesDlg::OnUpdatePrinterFont(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bShowPrinterFont);
}

//------------------------------------------------------------------------------
void CFontStylesDlg::OnShowPrinterFont()
{
	m_bShowPrinterFont = !m_bShowPrinterFont;
}

//------------------------------------------------------------------------------
void CFontStylesDlg::OnComboStyleChanged()
{
	BOOL	bFind	= FALSE;
	int		nCurSel	= m_CmbStyle.GetCurSel();
	
	if (!nCurSel)
	{
		m_sFilterStyle = _T("");
		FillTreeCtrlStyle();
		return;
	}

	FontStyle* pItem = (FontStyle*) m_CmbStyle.GetItemData(nCurSel);
	m_sFilterStyle = pItem->m_strStyleName;

	FillTreeCtrlStyle	();
	SetDefaultSelection ();
}

//----------------------------------------------------------------------------
void CFontStylesDlg::OnNMDblclkTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (CanOpen())
		OpenStyle();
	*pResult = 0;
}

//--------------------------------------------------------------------------
void CFontStylesDlg::OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel;
	hSel = m_treeStyle.GetSelectedItem();
	
	m_pToolBar->EnableButton(IDOK);

	EnableDisableToolbar ();

	if (m_treeStyle.ItemHasChildren(hSel))
		return;

	FontStyle* pTmpFont = (FontStyle*) m_treeStyle.GetItemData(hSel);
	//CTreeFontItemRef* pRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
	FontIdx nIdx =	m_StyleTable.GetFontIdx(pTmpFont->GetStyleName(), FALSE);

	if (nIdx < 0)
		return;

	m_pToolBar->EnableButton(IDOK, TRUE);
}

//----------------------------------------------------------------------------
void CFontStylesDlg::OnSaveFontStyles()
{
	if (AfxMessageBox(_TB("Are you sure to save changes\nto general font styles?\nDo you want continue?"), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
		DoSaveFontStyles ();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnHelp()
{
	ShowHelp(szHelpNamespace);
}

//----------------------------------------------------------------------------
void CFontStylesDlg::DoSaveFontStyles ()
{
	FontStyleTablePtr ptrFontStyleTable = AfxGetWritableFontStyleTable();
	if (m_bIgnoreIdx)
		*ptrFontStyleTable = m_StyleTable;
	else
	{
		*ptrFontStyleTable = m_StyleTable;
		m_SourceStyleTable = m_StyleTable;
		DeleteWoormFont(ptrFontStyleTable);
	}

	m_bFontSave = FALSE;
	m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
	CTBNamespaceArray aNs;
	ptrFontStyleTable->CheckFontTable(aNs);

	if (aNs.GetSize() == 0)
		return;	

	FontsParser	aParser;	

	for (int i = 0; i <= aNs.GetUpperBound(); i++)
	{
		// check se ns valido e correttezza di module
		CTBNamespace* ns = aNs.GetAt(i);
		if (!ns->IsValid() ||ns->GetType() != CTBNamespace::MODULE)
			continue;
		
		aParser.SaveFonts(*ns, AfxGetPathFinder());
	}	

	// devo farlo per fare il Refresh delle date dei files
	m_StyleTable.CopyFileLoaded (*ptrFontStyleTable);

	ptrFontStyleTable->SetModified(FALSE);
}	

//----------------------------------------------------------------------------
void CFontStylesDlg::DeleteWoormFont(FontStyleTablePtr StyleTable)
{
	for (int i = 0; i <= StyleTable->GetUpperBound(); i++)
	{
		FontStylesGroup* FontGrp = (FontStylesGroup*) StyleTable->GetAt(i);
		for (int n = 0; n <= FontGrp->m_FontsStyles.GetUpperBound(); n++ )
		{
			FontStyle* pFont = (FontStyle*) FontGrp->m_FontsStyles.GetAt(n);
			if (pFont->GetSource() == FontStyle::FROM_WOORM)
				 FontGrp->DeleteFont(pFont);
		}
	}
}

//----------------------------------------------------------------------------
void CFontStylesDlg::UpdateSourceStyleTable()
{
	DeleteWoormFont(FontStyleTablePtr(&m_SourceStyleTable, FALSE));
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FontStylesGroup* FontGrp = (FontStylesGroup*) m_StyleTable.GetAt(i);
		for (int n = 0; n <= FontGrp->m_FontsStyles.GetUpperBound(); n++ )
		{
			FontStyle* pFont = (FontStyle*) FontGrp->m_FontsStyles.GetAt(n);
			if (pFont->GetSource() == FontStyle::FROM_WOORM)
			{
				FontStyle* pNewF = new FontStyle(*pFont);
				m_SourceStyleTable.AddFont(pNewF);
				m_SourceStyleTable.SetModified(TRUE);
			}
		}
	}
}

//----------------------------------------------------------------------------
void CFontStylesDlg::EnableDisableToolbar ()
{
	if (AfxGetThemeManager()->AutoHideToolBarButton())
	{
		m_pToolBar->HideButton(ID_FONT_CUT, !CanDelete());
		m_pToolBar->HideButton(ID_FONT_COPY, !CanCopy());
		m_pToolBar->HideButton(ID_FONT_PASTE, !CanPaste());
		m_pToolBar->HideButton(ID_FONT_OPEN, !CanOpen());
		m_pToolBar->HideButton(ID_FONT_APPLYIN, !CanApplyContextArea());
		m_pToolBar->HideButton(ID_FONT_RENAME, !CanDelete());
		m_pToolBar->HideButton(ID_FONT_DELETE, !CanDelete());
		m_pToolBar->HideButton(ID_FONT_HELP, FALSE);

		m_pToolBar->RepositionRightButtons();
		m_pToolBar->AdjustSizeImmediate();
		m_pToolBar->AdjustLayout();
		return;
	}

	m_pToolBar->EnableButton(ID_FONT_CUT,			CanDelete());
	m_pToolBar->EnableButton(ID_FONT_COPY,			CanCopy());
	m_pToolBar->EnableButton(ID_FONT_PASTE,			CanPaste());
	m_pToolBar->EnableButton(ID_FONT_OPEN,			CanOpen());
	m_pToolBar->EnableButton(ID_FONT_APPLYIN,		CanApplyContextArea());
	m_pToolBar->EnableButton(ID_FONT_RENAME,		CanDelete());
	m_pToolBar->EnableButton(ID_FONT_DELETE,		CanDelete());
	m_pToolBar->EnableButton(ID_FONT_HELP,			TRUE);

	m_pToolBar->RepositionRightButtons();
	m_pToolBar->AdjustSizeImmediate();
	m_pToolBar->AdjustLayout();
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::OnOkFontForSelect (FontStyle* pSelFont, BOOL bSave)
{
	if (!pSelFont)
	{
		AfxMessageBox(_TB("This style is not utilizable by this report.\nVerify criterion of Apply In ..."), MB_APPLMODAL);
		return FALSE;
	}
	if (!m_bShowDefaultFont && pSelFont->GetStyleName().CompareNoCase(FontStyle::s_szFontDefault) == 0)
	{
		AfxMessageBox(_TB("Sorry, this style is usables only by report template"), MB_APPLMODAL);
		return FALSE;
	}

	// controllo l'usabilità del font
	int nIdx = m_StyleTable.GetFontIdx(pSelFont->GetStyleName());
	FontStyle* pFont = m_StyleTable.GetFontStyle(nIdx, &m_NsForWoorm);
	if (!pFont)
	{
		AfxMessageBox(_TB("This style is not utilizable by this report.\nVerify criterion of Apply In ..."), MB_APPLMODAL);
		return FALSE;
	}
	
	// avviso l'utente che non ha salvato gli stili generali
	
	
	if	(bSave && m_bFontSave)
	{
		if (AfxMessageBox(_TB("General application styles has been modified!\nModifications will be automatically saved.\nDo you want continue?"), MB_APPLMODAL | MB_OKCANCEL) == IDCANCEL)
			return FALSE;
		
		DoSaveFontStyles();
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnOK()
{
	if (!IsTreeCtrlEditMessage(VK_RETURN) && !m_bFromTree)
	{
		if (m_bModified)
			AfxGetMainWnd()->PostMessage(UM_FONT_STYLE_CHANGED); 

		if (m_bIgnoreIdx || m_bSelezionaAsOk)
		{
			// avviso l'utente che non ha salvato gli stili generali
			if	(m_bSelezionaAsOk && m_bFontSave)
			{
				if (AfxMessageBox(_TB("General application styles has been modified!\nModifications will be automatically saved.\nDo you want continue?"), MB_APPLMODAL | MB_OKCANCEL) == IDCANCEL)
					return;
				
				DoSaveFontStyles();
			}
			
			m_SourceStyleTable = m_StyleTable;
			EndDialog (IDOK);
			return;
		}
		
		HTREEITEM hSel = m_treeStyle.GetSelectedItem();	
		if (!hSel)
		{
			AfxMessageBox(_TB("No font style has been selected!"), MB_APPLMODAL);
			return;
		}
	
		FontStyle* pSelFont = (FontStyle*) m_treeStyle.GetItemData(hSel);
		
		if (!OnOkFontForSelect(pSelFont, TRUE))
			return;

		m_FontIdx = m_StyleTable.GetFontIdx(pSelFont->GetStyleName());
		m_SourceStyleTable = m_StyleTable;
			
		EndDialog(IDOK);
		return;
	}

	if (m_bFromTree)
	{
		OpenStyle();
		m_bFromTree = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnCancel()
{
	if (m_bFontSave && AfxMessageBox (_TB("Modifications to general styles will be losed!\nAre you sure to exit?"), MB_APPLMODAL | MB_YESNO ) == IDNO)
		return;

	if (!m_bIgnoreIdx)	//devo riportare solo i woorm
	{
		// controllo che non abbiano cambiato le caratteristiche del preesistente
		FontStyle* pSelFont = m_StyleTable.GetFontStyle(m_FontIdx, &m_NsForWoorm);
		if (pSelFont)
		{
			if (!OnOkFontForSelect (pSelFont, FALSE))
				return;

			UpdateSourceStyleTable();
		}
	}

	CLocalizableDialog::OnCancel();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::DeleteStyle()
{
	HTREEITEM	hSel	= m_treeStyle.GetSelectedItem();
	FontStyle*	pFont	= (FontStyle*) m_treeStyle.GetItemData(hSel);
	if (!pFont)
		return;

	if (RemoveCustomStyle(pFont))
		m_treeStyle.DeleteItem(hSel);
	if (pFont->m_FromAndTo == FontStyle::FROM_CUSTOM)
	{
		m_bFontSave = TRUE;
		m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
	}

	BOOL bOne = FALSE;
	FontIdx nIdx = m_StyleTable.GetFontIdx(pFont->GetStyleName());

	for (int p = 0; p <= m_StyleTable.GetAt(nIdx)->m_FontsStyles.GetUpperBound(); p++)
	{
		FontStyle* pFontNotDel = (FontStyle*) m_StyleTable.GetAt(nIdx)->m_FontsStyles.GetAt(p);
		if (!pFontNotDel->m_bDeleted)
		{
			bOne = TRUE;
			break;			
		}
	}	
	
	FillComboStyle();
	if (!bOne)
		OnComboStyleChanged();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnOpen	()
{
	m_treeStyle.OnOpen();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnDelete()
{
	m_treeStyle.OnDelete();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnCopy()
{
	m_treeStyle.OnCopy();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnPaste()
{
	m_treeStyle.OnPaste();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnRename()
{
	m_treeStyle.OnRename();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnCut()
{
	m_treeStyle.OnCut ();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnContextArea()
{
	m_treeStyle.OnContextArea();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnFilterTree()
{
	m_bFilterTree = !m_bFilterTree;

	m_pToolBar->PressButton(ID_FONT_FILTERTREE, m_bFilterTree);
	m_pToolBar->UpdateWindow();

	FillTreeCtrlStyle();
	SetDefaultSelection ();
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OnRemaneLabel()
{
	m_treeStyle.EditLabel(m_treeStyle.GetSelectedItem());
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::RenameStyle(const CString& strSelItem)
{
	HTREEITEM	hSel  = m_treeStyle.GetSelectedItem();
	FontStyle*	pFont = (FontStyle*) m_treeStyle.GetItemData(hSel);

	if (strSelItem.CompareNoCase(pFont->GetStyleName()) == 0 && pFont->m_FromAndTo == FontStyle::FROM_CUSTOM)
		return;

	FontStyle* pNewFont = new FontStyle(*pFont);
	
	if (pNewFont->m_FromAndTo != FontStyle::FROM_WOORM)
		pNewFont->m_FromAndTo = FontStyle::FROM_CUSTOM;
	
	pNewFont->m_strStyleName = strSelItem;
	pNewFont->m_bChanged = TRUE;

	if (pNewFont->m_FromAndTo != FontStyle::FROM_STANDARD)
	{
		FontStyle* pOldFontCopy = m_pFontCopy;

		m_pFontCopy = pNewFont;
		m_pFontCopy->SetStandardFont(NULL);
		AddNewStyle(&pNewFont->m_OwnerModule);
		m_pFontCopy = pOldFontCopy;
	}

	if (RemoveCustomStyle(pFont, TRUE))
		m_treeStyle.DeleteItem(hSel);

	if (!m_StyleTable.IsModified())
		m_StyleTable.SetModified(TRUE);
	
	if (pNewFont->m_FromAndTo == FontStyle::FROM_CUSTOM)
	{
		m_bFontSave = TRUE;
		m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
	}

	SAFE_DELETE(pNewFont);
}

//-----------------------------------------------------------------------------
CString CFontStylesDlg::GetNewFontName(FontStyle* pFont)
{
	FontStyle* pExistingFont = m_StyleTable.GetFontStyle(pFont, pFont->GetSource());
	// ne esiste uno contrassegnato deleted quindi lo riuso
	if (!pExistingFont || pExistingFont->IsDeleted())
		return pFont->m_strStyleName;

	CString strName	= pFont->GetStyleName();
	CString sCopiaDi= _TB("Copy of");
	CString sDi = _TB("of");
	CString sNewName;

	int	nLast  = -1;
	int	nFirst = strName.Find('(');
	int nCopy  = 0;
	
	// provo a cercare copia di
	BOOL bIsCopiaDi = strName.Left(sCopiaDi.GetLength()).CompareNoCase(sCopiaDi) == 0;

	if (bIsCopiaDi)
		nLast = sCopiaDi.GetLength();
	else
	{
		// c'è anche la parola dopo
		nLast = strName.ReverseFind(')');
		if (nLast >=0)
			nLast += sDi.GetLength() + 1;
	}
	
	if (nLast)
		sNewName = strName.Mid(nLast+1);

	if (nFirst < nLast)
	{
		if (nFirst >= 0 && nLast >= 0)
			strName = strName.Mid(nFirst + 1, nLast - nFirst);
		nCopy = _tstoi(strName);
		if (nCopy <= 0)
			nCopy = 1;
	}
	nCopy++;

	if (nCopy == 1)
	{
		pFont->m_strStyleName = sCopiaDi;
		sNewName = strName;
	}
	else
		pFont->m_strStyleName = cwsprintf(_TB("Copy ({0-%d})") + sDi, nCopy);

	pFont->m_strStyleName += _T (" ") + sNewName;

	// esiste già quindi provo in ricorsione
	if (m_StyleTable.GetFontIdx(pFont, pFont->GetSource()) >= 0)
		GetNewFontName(pFont);
	
	return pFont->m_strStyleName;
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::OpenStyle()
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	int			nLev = GetItemLevel(hSel);

	if (nLev == 0)
		return;
	else if (nLev == 1)
	{
		//controllo se il parent è Application
		HTREEITEM			hParentSel	= m_treeStyle.GetParentItem(hSel);
		CTreeFontItemRef*	pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hParentSel);

		if 
			(
				pItemRef && 
				pItemRef->m_strName.CompareNoCase(szCurrentReport) &&
				pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport) &&
				(
					!AfxGetPathFinder()->IsASystemApplication(pItemRef->m_strName)
					||
					!AfxGetAddOnApp(pItemRef->m_strName))
				)
			return;

		pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
		if 
			(
				pItemRef && 
				pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport) == 0
			)
			return;
	}	

	FontStyle*  pOldFont = (FontStyle*) m_treeStyle.GetItemData(hSel);
	FontStyle*  pNewFont = new FontStyle(*pOldFont);	

	// se sono nel report di default lo creo come woorm
	BOOL bInWoorm = FALSE;
	if (!m_bIgnoreIdx && pNewFont->GetSource() == FontStyle::FROM_STANDARD)
	{
		pNewFont->SetOwner(m_NsForWoorm);
		pNewFont->m_FromAndTo = FontStyle::FROM_WOORM;
		bInWoorm = TRUE;
	}

	CDC* pDC = NULL;
	CDC dcPrint;
	DWORD dwFlags = CF_EFFECTS | CF_SCREENFONTS | CF_INITTOLOGFONTSTRUCT;

	// la richeista alla stampante puo' essere un processo lungo
	BeginWaitCursor();

	// magic to get printer dialog that would be used if we were printing!
	if (m_bShowPrinterFont)
	{
		HDC hdcPrint = NULL;
		CPrintDialog dlgPrint(FALSE);

		//if is a fontDialog open from a Report Woorm,check if it has preferred printer
		if (!m_strPreferredPrinter.IsEmpty())
		{
			//preferred printer
			HGLOBAL hDevMode;
			HGLOBAL hDevNames;

			if (GetPrinterDevice(m_strPreferredPrinter.GetString(), &hDevNames, &hDevMode))
			{
				dlgPrint.m_pd.hDevMode = hDevMode;
				dlgPrint.m_pd.hDevNames = hDevNames;
			}
			else
			{
				ASSERT(FALSE);
			}
		}
		else if (!AfxGetApp()->GetPrinterDeviceDefaults(&dlgPrint.m_pd))
		{
			AfxMessageBox(_TB("Unable to read the printer default values"), MB_APPLMODAL);
			EndWaitCursor();
			return;
		}

		hdcPrint = dlgPrint.CreatePrinterDC();
		if (hdcPrint == NULL)
		{
			AfxMessageBox(_TB("Unable to create the (DC) context of the printer "), MB_APPLMODAL);
			return;
		}

		dcPrint.Attach(hdcPrint);
		pDC = &dcPrint;
		dwFlags |= CF_BOTH;
	}

    // modify existing font style
	LOGFONT lf = pNewFont->GetLogFont();

    // CFontDialog use device point
	CFontDialog dialog(&lf, dwFlags, pDC, this);

	// this is a trick for set style combo in font dialog (afx bugs)
	dialog.m_cf.Flags		= dialog.m_cf.Flags & ~CF_USESTYLE;
	dialog.m_cf.rgbColors	= pNewFont->GetColor();

	EndWaitCursor();

	if (dialog.DoModal() == IDOK) 
	{
		if (!AfxGetLoginInfos()->m_bAdmin && pNewFont->GetSource() != FontStyle::FROM_WOORM)
		{
			AfxMessageBox(_TB("Warning, only administrator is enabled to edit this no report style!"), MB_APPLMODAL);
			SAFE_DELETE(pNewFont);
			return;
		}
		if (pNewFont->GetSource() == FontStyle::FROM_WOORM_TEMPLATE)
		{
			AfxMessageBox(_TB("Sorry, you cannot modify template's report style! (You can do it by editing directly the report template)"), MB_APPLMODAL);
			SAFE_DELETE(pNewFont);
			return;
		}

		pNewFont->SetLogFont(dialog.m_lf);
		pNewFont->SetColor(dialog.m_cf.rgbColors);

		// la comparazione delle modifiche la faccio a prescindere dai cambi
		// di namespace o di provenienza imposti dalla customizzazione.
		FontStyle aCompareFont (*pNewFont);
		aCompareFont.m_FromAndTo	= pOldFont->m_FromAndTo;
		aCompareFont.m_OwnerModule	= pOldFont->m_OwnerModule;
		if (pOldFont->Compare(aCompareFont) == 0)
		{
			SAFE_DELETE(pNewFont);
			return;
		}

		ApplyFontChange(pNewFont, pOldFont);	
	}
	
	SAFE_DELETE(pNewFont);

	if (!bInWoorm)
	{
		m_treeStyle.SelectItem	(hSel);
		m_treeStyle.SetFocus	();
	}
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::ContextArea()
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	//int			nLev = GetItemLevel(hSel);
	FontStyle*  pFont= (FontStyle*) m_treeStyle.GetItemData(hSel);

	if (!pFont)
		return;
	
	// inizializzo il default
	CStringArray aSelections;
	for (int i=0; i <= pFont->GetLimitedAreas().GetUpperBound(); i++)
		aSelections.Add(pFont->GetLimitedAreas().GetAt(i));
	
	// editazione e travaso dei dati
	CContextSelectionDialog aDlg(&aSelections);
	if (aDlg.DoModal() == IDCANCEL)
		return;

	BOOL bModified = aSelections.GetSize() != pFont->GetLimitedAreas().GetSize();
	if (!bModified)
		for (int i=0; i <= aSelections.GetUpperBound(); i++)
			if (aSelections.GetAt(i).CompareNoCase(pFont->GetLimitedAreas().GetAt(i)))
			{
				bModified = TRUE;
				break;
			}
	
	if (bModified)
	{
		pFont->SetLimitedAreas(aSelections);
		pFont->SetChanged(TRUE);
		m_StyleTable.SetModified(TRUE);
	}

	// applica in è selezionabile solo sui font custom
	m_bFontSave = TRUE;
	m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::ApplyFontChange(FontStyle* pNewFont, FontStyle* pOldFont)
{
	if (!pOldFont || !pNewFont)
		return;

	// mi assicuro di non modificare mai lo standard
	if (pNewFont->m_FromAndTo == FontStyle::FROM_STANDARD)
		pNewFont->m_FromAndTo = FontStyle::FROM_CUSTOM;

	FontStyle* pExistingFont = (FontStyle*) m_StyleTable.GetFontStyle(pNewFont, pNewFont->GetSource());
	if (!pExistingFont)
	{
		FontStyle* pFont = new FontStyle(*pNewFont);
		pFont->SetChanged(TRUE);
		m_StyleTable.AddFont(pFont);
		InsertInTree(pFont);

		if (pNewFont->m_FromAndTo == FontStyle::FROM_WOORM)
			AfxMessageBox(cwsprintf(_TB("A local report style has been generated\nwith name {0-%s}"), pFont->GetTitle()), MB_APPLMODAL);
	}
	else
	{
		// se è cancellato lo ripristino
		if (pExistingFont->m_bDeleted)
		{
			pExistingFont->Assign(*pNewFont);
			InsertInTree(pNewFont);

			// ripeto la message box visto che per l'utente l'effetto è uguale all'insert
			if (pNewFont->m_FromAndTo == FontStyle::FROM_WOORM)
				AfxMessageBox(cwsprintf(_TB("A local report style has been generated\nwith name {0-%s}"), pNewFont->GetTitle()), MB_APPLMODAL);
		}
		else
		{
			CString sType = (pNewFont->m_FromAndTo == FontStyle::FROM_WOORM || pNewFont->m_FromAndTo == FontStyle::FROM_WOORM_TEMPLATE) ? _TB("Report") : _TB("Customized");
			// do avvertimento di sovrascrittura solo se ho cambiato natura al font
			if (
					pNewFont->m_FromAndTo == pOldFont->m_FromAndTo ||
					AfxMessageBox(cwsprintf(_TB("Font style {0-%s} '{1-%s}' already exists.\nWould you like to overwrite it? "), sType, pNewFont->GetStyleName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK
				)
				pExistingFont->Assign(*pNewFont);
			else
				return;
		}
	}

	// aggiornamento stato della tabella
	pNewFont->SetChanged(TRUE);
	pOldFont->SetChanged(TRUE);

	if (!m_StyleTable.IsModified())
		m_StyleTable.SetModified(TRUE);	

	if (pNewFont->m_FromAndTo == FontStyle::FROM_CUSTOM)
	{
		m_bFontSave = TRUE;
		m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
	}
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::CopyStyle()	//per ora solo font (solo un custom)
{
	HTREEITEM hSel = m_treeStyle.GetSelectedItem();
	int		  nLev = GetItemLevel(hSel);

	if (nLev == 0)
		return;

	HTREEITEM hMod = m_treeStyle.GetParentItem(hSel);
	HTREEITEM hApp = m_treeStyle.GetParentItem(hMod);

	switch (nLev)
	{
		case (2):
		{
			FontStyle* pFontSel = (FontStyle*) m_treeStyle.GetItemData(hSel);

			if (m_pFontCopy)
				SAFE_DELETE(m_pFontCopy);

			m_pFontCopy = new FontStyle(*pFontSel);
			break;
		}
		case (1):
		{
			//controllo se il parent è Application
			HTREEITEM			hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeFontItemRef*	pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString				strParent	= pItemRef->m_strName;
			
			if (AfxGetPathFinder()->IsASystemApplication(strParent) || strParent.CompareNoCase(szCurrentReport) == 0)
			{
				FontStyle* pFontSelect = (FontStyle*) m_treeStyle.GetItemData(hSel);

				if (m_pFontCopy)
					SAFE_DELETE(m_pFontCopy);

				m_pFontCopy = new FontStyle(*pFontSelect);
				break;
			}

			if (!AfxGetAddOnApp(strParent))
				return;

			return;
		}
	}
}

// per ora solo font (solo un custom)
//-----------------------------------------------------------------------------
void CFontStylesDlg::PasteStyle()	
{
	CTBNamespace		NsNew; 
	HTREEITEM			hSel		= m_treeStyle.GetSelectedItem();
	CTreeFontItemRef*	pTreeItemRef= (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
	CString				strSel		= pTreeItemRef->m_strName;
	CString				strParent	= _T("");
	//int					nLev		= GetItemLevel(hSel);

	HTREEITEM hParent  = m_treeStyle.GetParentItem(hSel);
	if (hParent)	// caso del modulo
	{
		pTreeItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hParent);
		strParent	 = pTreeItemRef->m_strName;

		NsNew.SetObjectName(CTBNamespace::MODULE, strSel);
		NsNew.SetApplicationName(strParent);
		
		if (strParent.CompareNoCase(szCurrentReport) == 0)
		{
			NsNew.SetType(CTBNamespace::REPORT);
			NsNew.SetObjectName(CTBNamespace::REPORT, m_pFontCopy->m_OwnerModule.GetObjectName(CTBNamespace::REPORT));
		}
		else
			NsNew.SetType(CTBNamespace::MODULE);
	}
	else		// caso application
	{
		if (strSel.CompareNoCase(szCurrentReport) == 0)
			NsNew = m_NsForWoorm;
		else
		{
			NsNew.SetType(CTBNamespace::MODULE);
			NsNew.SetApplicationName(strSel);

			HTREEITEM hChild = m_treeStyle.GetChildItem(hSel);
			if (hChild == NULL)
				hChild = m_treeStyle.GetPrevSiblingItem(hSel);
		
			FontStyle*	pFont	= (FontStyle*) m_treeStyle.GetItemData(hChild);
			CString		strMod	= pFont->m_OwnerModule.GetObjectName(CTBNamespace::MODULE);

			NsNew.SetObjectName(CTBNamespace::MODULE, strMod);
		}
	} 

	if(!AddNewStyle(&NsNew))
		return;

	if (!m_StyleTable.IsModified())
		m_StyleTable.SetModified(TRUE);

	if (m_bFormCut)
	{
		if (RemoveCustomStyle(m_pFontCopy))
			m_treeStyle.DeleteItem(m_hSelCut);

		if (m_pFontCopy->m_FromAndTo == FontStyle::FROM_CUSTOM)
		{
			m_bFontSave = TRUE;
			m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
		}

		m_hSelCut = NULL;
		m_bFormCut = FALSE;
	}

		if (NsNew.GetType() != CTBNamespace::REPORT)
		{
			m_bFontSave = TRUE;
			m_pToolBar->EnableButton(IDC_FONT_SAVE, m_bFontSave);
		}
	
	EnableDisableToolbar();
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::AddNewStyle(CTBNamespace* Ns)
{	
	FontStyle* pNewStyle = new FontStyle(*m_pFontCopy);
	pNewStyle->SetChanged(TRUE);
	pNewStyle->m_OwnerModule.SetNamespace(*Ns);
	pNewStyle->m_strStyleName = pNewStyle->GetTitle();
	
	// limito l'applicabilità di default in caso di copia/incolla,
	// ma solo se c'è un cambio di posizione o se non sono sugli stili Tb
	if (	
			!m_bFormCut && 
			Ns->GetType() != CTBNamespace::REPORT && 
			m_pFontCopy->GetOwner() != *Ns &&
			Ns->GetApplicationName().CompareNoCase(szTaskBuilderApp) 
		)
		pNewStyle->SetLimitedArea(Ns->ToString());

	if (Ns->GetType() == CTBNamespace::REPORT)
		pNewStyle->m_FromAndTo = FontStyle::FROM_WOORM;
	else
		pNewStyle->m_FromAndTo = FontStyle::FROM_CUSTOM;

	// mi faccio dare il nuovo nome
	pNewStyle->m_strStyleName = GetNewFontName(pNewStyle);

	// se esiste già cancellato riattivo il precedente, altrimenti lo aggiungo
    FontStyle* pFontSpecLoc = m_StyleTable.GetFontStyle(pNewStyle, pNewStyle->GetSource());
	if (pFontSpecLoc)
	{
		BOOL bDeleted = pFontSpecLoc->m_bDeleted;
		if (bDeleted)

		{
			pFontSpecLoc->Assign(*pNewStyle);
			InsertInTree(pFontSpecLoc);
			SAFE_DELETE(pNewStyle);
			return TRUE;
		}
	}
	
	m_StyleTable.AddFont(pNewStyle);
	InsertInTree(pNewStyle);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::InsertInTree(FontStyle* pFont)
{
	FillComboStyle();

	// se il nome non corrisponde al filtro indicato non lo visualizzo
	if (!m_sFilterStyle.IsEmpty()&& pFont->GetStyleName().CompareNoCase(m_sFilterStyle))
		return;

	HTREEITEM	 hAppItem = m_treeStyle.GetRootItem();
	CTBNamespace ns (pFont->GetOwner());
	FontStyle*	 pItemData = m_StyleTable.GetFontStyle(pFont, pFont->GetSource());

	while(hAppItem)
	{
		CString sLev0 = m_treeStyle.GetItemText(hAppItem);
		CTreeFontItemRef* pItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hAppItem);
		CString strXml = AfxGetAddOnApp(ns.GetApplicationName())->GetTitle();

		// sono nella cartella Report ed i child sono dei fontstyle
		if (pItemRef->m_strName.CompareNoCase(szCurrentReport) == 0 &&
			(pFont->m_FromAndTo == FontStyle::FROM_WOORM || pFont->m_FromAndTo == FontStyle::FROM_WOORM_TEMPLATE)
			)
		{
			HTREEITEM hInsert = m_treeStyle.InsertItem(pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle(), 1, 1, hAppItem);
			m_treeStyle.EnsureVisible(hInsert);
			BOOL n = m_treeStyle.SetItemData(hInsert, (DWORD_PTR) pItemData);
			m_treeStyle.SelectItem(hInsert);
			m_treeStyle.SetFocus();
			return;
		}

		if (AfxGetPathFinder()->IsASystemApplication(ns.GetApplicationName()))
		{
			if (sLev0.CompareNoCase(strXml) == 0)
			{
				HTREEITEM hInsert = m_treeStyle.InsertItem(pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle(), 1, 1, hAppItem);
				m_treeStyle.EnsureVisible(hInsert);
				BOOL n = m_treeStyle.SetItemData(hInsert, (DWORD_PTR) pItemData);
				m_treeStyle.SelectItem(hInsert);
				m_treeStyle.SetFocus();
				return;
			}
		}
		else
		{
			if (sLev0.CompareNoCase(strXml) == 0)
			{
				HTREEITEM	hModItem	= m_treeStyle.GetChildItem(hAppItem);
				
				while(hModItem)
				{
					CString		sLev1		= m_treeStyle.GetItemText(hModItem);
					CString		strModXml	= AfxGetAddOnModule(ns)->GetModuleTitle();

					if (sLev1.CompareNoCase(strModXml) == 0)
					{
						HTREEITEM hInsertItem = m_treeStyle.InsertItem(pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle(), 1, 1, hModItem);
						BOOL n = m_treeStyle.SetItemData(hInsertItem, (DWORD_PTR) pItemData);
						m_treeStyle.SelectItem(hInsertItem);
						m_treeStyle.SetFocus();
						return;
					}
					hModItem = m_treeStyle.GetNextSiblingItem(hModItem);
				}				
			}			
		}
		hAppItem = m_treeStyle.GetNextSiblingItem(hAppItem);
	}
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::RemoveCustomStyle(FontStyle* pFont, BOOL bFromRename /*= FALSE*/)
{
	// la cancellazione di uno standard non è possibile
	if (pFont->m_FromAndTo == FontStyle::FROM_STANDARD)
		return FALSE;

	FontIdx nIdx = m_StyleTable.GetFontIdx(pFont->GetStyleName());
	
	// altrimenti non esiste
	if (nIdx < 0)
		return TRUE;
	
	for (int i = 0; i <= m_StyleTable.GetAt(nIdx)->m_FontsStyles.GetUpperBound(); i++)
	{
		FontStyle* pFontOld = (FontStyle*) m_StyleTable.GetAt(nIdx)->m_FontsStyles.GetAt(i);

		// non è quello che devo cancellare
		if (pFontOld->m_OwnerModule != pFont->m_OwnerModule || pFontOld->m_FromAndTo != pFont->m_FromAndTo)
			continue;

		FontStyle* pFontToDel = m_StyleTable.GetFontStyle(pFont, pFont->GetSource());
		if (!pFontToDel)
			return FALSE;

		// se sono in Woorm e non vengo da un rename, chiedo conferma
		if	(	
				pFont->m_FromAndTo == FontStyle::FROM_WOORM && 
				!bFromRename && 
				AfxMessageBox(cwsprintf(_TB("Warning, report font style '{0-%s}' will be deleted. Do you want permanently delete  it?"), pFont->GetStyleName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK
			)
			return FALSE;

		// lo dichiaro deleted
		pFontToDel->SetChanged(TRUE);
		pFontToDel->SetDeleted(TRUE);
		if (!m_StyleTable.IsModified())
			m_StyleTable.SetModified(TRUE);

		break; 
	}
	
	FillComboStyle();
	return TRUE;
}

//-----------------------------------------------------------------------------
int CFontStylesDlg::GetItemLevel(HTREEITEM hItem)
{
	if (!hItem)
		return -1;

	int nLevel = 0;

	HTREEITEM hParent = m_treeStyle.GetParentItem(hItem);
	if (!hParent)
		return nLevel;

	nLevel++;
	hItem = hParent;

	while(hParent = m_treeStyle.GetParentItem(hItem))
	{
		nLevel++;
		hItem = hParent;
	}

	return nLevel;
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::LoadImageList()
{
	HICON	hIcon[3];
	int		n;

	m_ImageList.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());

	hIcon[0] = TBLoadImage(TBGlyph(szIconFolder));
	hIcon[1] = TBLoadImage(TBGlyph(szIconAllUsers));
	hIcon[2] = TBLoadImage(TBGlyph(szIconStandard));

	for (n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}
	m_treeStyle.SetImageList(&m_ImageList, TVSIL_NORMAL);
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::FillTreeCtrlStyle()
{
	CTreeFontItemRef*	pItemRefLocaliz = NULL;
	HTREEITEM 			hItemReport		= 0;
	HTREEITEM 			hItemApp		= 0;
	CString				strApps			= _T("");
	CString				strXml			= _T("");
	HTREEITEM			hItemStartSel	= 0; 
	BOOL				bIsSystemApp	= FALSE;
	BOOL				bHasFonts		= FALSE;

	m_DefaultSel = NULL;

	//if (!m_treeStyle.GetImageList(TVSIL_NORMAL))
	//	m_treeStyle.SetImageList(&m_ImageList, TVSIL_NORMAL);
	
	BeginWaitCursor();
	m_treeStyle.SetRedraw(FALSE);
	m_treeStyle.DeleteAllItems();
	m_arTreeItemRef.RemoveAll();

	//IF FROM WOORM (CONSIDERATA COME APPLICAZIONE)
	if (!m_bIgnoreIdx)
	{
		hItemReport = m_treeStyle.InsertItem(_TB("Local styles of current report"));
		pItemRefLocaliz = new CTreeFontItemRef(szCurrentReport);
		m_arTreeItemRef.Add(pItemRefLocaliz);
		m_treeStyle.SetItemData(hItemReport,  (DWORD) pItemRefLocaliz);

		HTREEITEM hItemReportTpl = m_treeStyle.InsertItem(_TB("Local styles loaded from of current report template"), 0,0, hItemReport);
		CTreeFontItemRef* pItemRefLocalizTpl = new CTreeFontItemRef(szTemplateCurrentReport);
		m_arTreeItemRef.Add(pItemRefLocalizTpl);
		m_treeStyle.SetItemData(hItemReportTpl,  (DWORD) pItemRefLocalizTpl);
	}		
	
	BOOL bReport = HasReportFonts (strApps, _T(""), m_sFilterStyle);
	if (bReport)
		FillTreeAddReportFonts();

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		
		if (!AfxGetAddOnApp(strApps))
			continue;

		strXml = AfxGetAddOnApp(strApps)->GetTitle();

		// se sono in parte sistemistica
		// controllo se esiste almeno un font in quella parte
		bIsSystemApp = AfxGetPathFinder()->IsASystemApplication(strApps);
		bHasFonts = HasApplicationFonts (strApps, _T(""), m_sFilterStyle);

		if (!bIsSystemApp || bHasFonts)
		{
			if (bIsSystemApp)
				hItemApp = 0;
			// filtro di visualizzazione
			else if (!bHasFonts && m_bFilterTree)
				continue;

			hItemApp = m_treeStyle.InsertItem(strXml);
			pItemRefLocaliz = new CTreeFontItemRef(strApps);
			m_arTreeItemRef.Add(pItemRefLocaliz);
			m_treeStyle.SetItemData(hItemApp, (DWORD) pItemRefLocaliz);

			if (bIsSystemApp)
 				FillTreeAddFonts (strApps, _T(""), hItemApp);
			else
				FillTreeAddModules (strApps, hItemApp);
		}
	}

	m_treeStyle.SetRedraw(TRUE);
	m_treeStyle.Invalidate(FALSE);
	m_treeStyle.ExpandAll(TVE_EXPAND);

	EnableDisableToolbar ();
	EndWaitCursor();
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::HasApplicationFonts(const CString& strApp, const CString& strMod, const CString& sFilterStyle)
{
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FontStylesGroup* pFontGrp = m_StyleTable.GetAt(i);
		for (int n = 0; n <= pFontGrp->m_FontsStyles.GetUpperBound(); n++)
		{
			FontStyle* pFont = (FontStyle*) pFontGrp->m_FontsStyles.GetAt(n);
			
			// confronta solo l'applicazione
			if	(
					strMod.IsEmpty()&& 
					pFont->m_OwnerModule.GetType() != CTBNamespace::REPORT &&
					pFont->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 && 
					(sFilterStyle.IsEmpty() || pFont->GetStyleName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;

			// confronta applicazione e modulo
			if	(
					pFont->m_OwnerModule.GetType() != CTBNamespace::REPORT &&
					pFont->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 &&
					pFont->m_OwnerModule.GetObjectName(CTBNamespace::MODULE).CompareNoCase(strMod) == 0 && 
					(sFilterStyle.IsEmpty() || pFont->GetStyleName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::HasReportFonts(const CString& strApp, const CString& strMod, const CString& sFilterStyle)
{
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FontStylesGroup* pFontGrp = m_StyleTable.GetAt(i);
		for (int n = 0; n <= pFontGrp->m_FontsStyles.GetUpperBound(); n++)
		{
			FontStyle* pFont = (FontStyle*) pFontGrp->m_FontsStyles.GetAt(n);

			if (pFont->m_FromAndTo == FontStyle::FROM_WOORM || pFont->m_FromAndTo == FontStyle::FROM_WOORM_TEMPLATE)
				return TRUE;

			// confronta solo l'applicazione
			//if	(
			//		strMod.IsEmpty()&& 
			//		pFont->m_OwnerModule.GetType() == CTBNamespace::REPORT &&
			//		pFont->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 && 
			//		(sFilterStyle.IsEmpty() || pFont->GetStyleName().CompareNoCase(sFilterStyle) == 0)
			//	)
			//	return TRUE;

			// confronta applicazione e modulo
			//if	(
			//		pFont->m_OwnerModule.GetType() == CTBNamespace::REPORT &&
			//		pFont->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 &&
			//		pFont->m_OwnerModule.GetObjectName(CTBNamespace::MODULE).CompareNoCase(strMod) == 0 && 
			//		(sFilterStyle.IsEmpty() || pFont->GetStyleName().CompareNoCase(sFilterStyle) == 0)
			//	)
			//	return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::CanCopy()		//anche per CanCut
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	int			nLev = GetItemLevel(hSel);
	
	switch (nLev)
	{
		case (2):
		{
			return TRUE;
			break;
		}
		case (1):
		{
			HTREEITEM			hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeFontItemRef*	pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString				strParent	= pItemRef->m_strName;
			
			pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);

			if (pItemRef && pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport) == 0)
				return FALSE;
			if (pItemRef && pItemRef->m_strName.CompareNoCase(FontStyle::s_szFontDefault) == 0)
				return FALSE;

			if (strParent.CompareNoCase(szCurrentReport) == 0)
				return TRUE;

			if (!AfxGetAddOnApp(strParent))
				return FALSE;

			if (AfxGetPathFinder()->IsASystemApplication(strParent))
				return TRUE;

			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::CanDelete()
{
	// solo se custom
	CTreeFontItemRef*	pItemRef	= NULL;
	HTREEITEM			hSel		= m_treeStyle.GetSelectedItem();
	HTREEITEM			hParentSel	= NULL;
	FontStyle*			pFont		= NULL;
	int					nLev		= GetItemLevel(hSel);

	switch (nLev)
	{
		case (2):
		{
			if (!AfxGetLoginInfos()->m_bAdmin)
				return FALSE;
			pFont = (FontStyle*) m_treeStyle.GetItemData(hSel);
			if (pFont->m_FromAndTo ==  FontStyle::FROM_CUSTOM)
				return TRUE;
			break;
		}
		case (1):
		{
			hParentSel	= m_treeStyle.GetParentItem(hSel);
			pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString	strParent	= pItemRef->m_strName;
			
			pItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
			if (pItemRef && pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport) == 0)
				return FALSE;

			if (strParent.CompareNoCase(szCurrentReport) == 0)
				return TRUE;

			if (!AfxGetAddOnApp(strParent))
				return FALSE;

			if (AfxGetPathFinder()->IsASystemApplication(strParent))
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				pFont = (FontStyle*) m_treeStyle.GetItemData(hSel);
				if (pFont->m_FromAndTo ==  FontStyle::FROM_CUSTOM)
					return TRUE;
			}	

			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::CanPaste()
{
	HTREEITEM	hSel	= m_treeStyle.GetSelectedItem();
	CString		strApp	= _T("");
	int			nLev	= GetItemLevel(hSel);

	if (!m_pFontCopy)
		return FALSE;

	switch (nLev)
	{
		case (0):
		{
			CTreeFontItemRef*	pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
			strApp = pItemRef->m_strName;

			if (strApp.CompareNoCase(szCurrentReport) == 0)
				return TRUE;
			if (!AfxGetAddOnApp(strApp))
				return FALSE;
			if (AfxGetPathFinder()->IsASystemApplication(strApp))
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				return TRUE;
			}				
			break;
		}
		case (1):
		{
			HTREEITEM			hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeFontItemRef*	pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hParentSel);
			strApp	= pItemRef->m_strName;
			
			if (strApp.CompareNoCase(szCurrentReport) == 0)
				return FALSE;

			if (!AfxGetAddOnApp(strApp))
				return FALSE;

			pItemRef	= (CTreeFontItemRef*) m_treeStyle.GetItemData(hSel);
			if (pItemRef && pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport) == 0)
				return FALSE;

			if (!AfxGetPathFinder()->IsASystemApplication(strApp)) 
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				return TRUE;
			}

			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::GetToolTipProperties(CTooltipProperties& tp)
{
	tp.m_strText.Empty();
	if (tp.m_nControlID == ID_FONT_CUT)
		tp.m_strText = _TB("Cut");
	else if (tp.m_nControlID == ID_FONT_COPY)
		tp.m_strText = _TB("Copy");
	else if (tp.m_nControlID == ID_FONT_PASTE)
		tp.m_strText = _TB("Property");
	else if (tp.m_nControlID == ID_FONT_APPLYIN)
		tp.m_strText = _TB("Apply in...");
	else if (tp.m_nControlID == ID_FONT_RENAME)
		tp.m_strText = _TB("Rename");
	else if (tp.m_nControlID == ID_FONT_DELETE)
		tp.m_strText = _TB("Delete");
	else if (tp.m_nControlID == ID_FONT_FILTERTREE)
	{
		if (m_bFilterTree)
			tp.m_strText = _TB("Show all modules tree");
		else
			tp.m_strText = _TB("Show only modules with styles");
	}
	else if (tp.m_nControlID == ID_FONT_HELP)
		tp.m_strText = _TB("Help on line (F1)");
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFontStylesDlg::CanApplyContextArea()
{
	if (!AfxGetLoginInfos()->m_bAdmin)
		return FALSE;

	HTREEITEM hSel = m_treeStyle.GetSelectedItem();

	if (hSel == NULL)
		return FALSE;

	int	nLev = GetItemLevel(hSel);

	if (nLev == 0)
		return FALSE;

	FontStyle* pFont = (FontStyle*) m_treeStyle.GetItemData(hSel);

	return !pFont || pFont->GetSource() == FontStyle::FROM_CUSTOM;
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::FillTreeAddModules(const CString strApps, HTREEITEM hItemApp)
{
	HTREEITEM 			hItemMod	= 0;
	CString				strMods		= _T("");
	CStringArray		aModules;
	CTreeFontItemRef*	pItemRefLocaliz = NULL;

	AddOnApplication* pAddOnApp = AfxGetAddOnApp(strApps);

	if (!pAddOnApp)
		return;

	AddOnModule* pAddOnMod;
	for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
	{
		pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
		if (!pAddOnMod)
			continue;

		strMods = pAddOnMod->GetModuleName();
		BOOL bAppFonts = HasApplicationFonts (strApps, pAddOnMod->GetModuleName(), m_sFilterStyle);

		// filtro di visualizzazione
		if (m_bFilterTree && !bAppFonts)
			continue;

		if (bAppFonts || !m_bFilterTree)
		{
			hItemMod = m_treeStyle.InsertItem(pAddOnMod->GetModuleTitle(), 0,0, hItemApp);
			pItemRefLocaliz = new CTreeFontItemRef(strMods);
			m_arTreeItemRef.Add(pItemRefLocaliz);
			m_treeStyle.SetItemData(hItemMod, (DWORD) pItemRefLocaliz);
			if (bAppFonts)
				FillTreeAddFonts (strApps, pAddOnMod->GetModuleName(), hItemMod);
		}
	}
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::FillComboStyle()
{
	// mi salvo la vecchia selezione se esisteva
	int nOldPos = m_CmbStyle.GetCurSel();
	if (nOldPos < 0 || nOldPos >= m_CmbStyle.GetCount())
		nOldPos = 0;

	m_CmbStyle.ResetContent();
	m_CmbStyle.AddString(_TB("<All styles>"));
	for (int n = 0; n <= m_StyleTable.GetUpperBound(); n++)
	{
		FontStylesGroup* pFontGrp = m_StyleTable.GetAt(n);

		for (int i = 0; i <= pFontGrp->m_FontsStyles.GetUpperBound(); i++)
		{
			FontStyle* pFont = (FontStyle*) pFontGrp->m_FontsStyles.GetAt(i);
			if (pFont->m_bDeleted)
				continue;
			
			if (m_CmbStyle.FindStringExact(-1, pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle()) < 0 )
			{
				int nIdx = m_CmbStyle.AddString(pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle());
				m_CmbStyle.SetItemData(nIdx, (DWORD_PTR) pFont);
			}
		}
	}
	m_CmbStyle.SetCurSel(nOldPos);
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::FillTreeAddFonts (const CString& strApp, const CString& strMod, HTREEITEM hParentItem)
{
	HTREEITEM 		hItemIni		= 0;
	HTREEITEM		hCurrParent;
	int				nIco			= 1;	// custom

	// default di entrata
	FontStyle* pFontDefault = NULL;

	if (m_DefaultSel == NULL && !m_bIgnoreIdx)
		pFontDefault = m_StyleTable.GetFontStyle(m_FontIdx, !m_bIgnoreIdx ? &m_NsForWoorm : NULL);

	for (int n = 0; n <= m_StyleTable.GetUpperBound(); n++)
	{
		FontStylesGroup* pFontGrp = m_StyleTable.GetAt(n);

		for (int i = 0; i <= pFontGrp->m_FontsStyles.GetUpperBound(); i++)
		{
			FontStyle* pFont = (FontStyle*) pFontGrp->m_FontsStyles.GetAt(i);
			
			// cancellato o non dell'applicazione
			if (pFont->m_bDeleted || strApp.CompareNoCase(pFont->GetOwner().GetApplicationName()))
				continue;
	
			// filtro di visualizzazione
			if (!m_sFilterStyle.IsEmpty() && pFont->GetStyleName().CompareNoCase(m_sFilterStyle))
				continue;

			if (!m_bShowDefaultFont && pFont->GetStyleName().CompareNoCase(FontStyle::s_szFontDefault) == 0)
				continue;

			CString strModFontTable = pFont->GetOwner().GetObjectName(CTBNamespace::MODULE);
			
			// modulo se gestito
			if (!strMod.IsEmpty() && strMod.CompareNoCase(strModFontTable))
				continue;

			if (pFont->m_FromAndTo == FontStyle::FROM_WOORM)
				hCurrParent = NULL;//GetReportTreeItem();
			else if (pFont->m_FromAndTo == FontStyle::FROM_WOORM_TEMPLATE)
				hCurrParent = NULL;//GetTplReportTreeItem();
			else
				hCurrParent = hParentItem;

			if (!hCurrParent)
				continue;

			nIco = pFont->m_FromAndTo == FontStyle::FROM_STANDARD ?  2 : 1;
			hItemIni = m_treeStyle.InsertItem
					(
						pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle(), 
						nIco, 
						nIco, 
						hCurrParent
					);
			m_treeStyle.SetItemData(hItemIni, (DWORD_PTR) m_StyleTable.GetAt(n)->m_FontsStyles.GetAt(i));
			
			// se è il default di entrata lo memorizzo		
			FontStyle* pFnt = (FontStyle*) m_StyleTable.GetAt(n)->m_FontsStyles.GetAt(i);
			if (pFontDefault && pFontDefault == pFnt)
				m_DefaultSel = hItemIni;
		}
	}
}

//-----------------------------------------------------------------------------
void CFontStylesDlg::FillTreeAddReportFonts ()
{
	HTREEITEM 		hItemIni		= 0;
	HTREEITEM		hCurrParent;
	int				nIco			= 1;	// custom

	// default di entrata
	FontStyle* pFontDefault = NULL;

	if (m_DefaultSel == NULL && !m_bIgnoreIdx)
		pFontDefault = m_StyleTable.GetFontStyle(m_FontIdx, !m_bIgnoreIdx ? &m_NsForWoorm : NULL);

	for (int n = 0; n <= m_StyleTable.GetUpperBound(); n++)
	{
		FontStylesGroup* pFontGrp = m_StyleTable.GetAt(n);

		for (int i = 0; i <= pFontGrp->m_FontsStyles.GetUpperBound(); i++)
		{
			FontStyle* pFont = (FontStyle*) pFontGrp->m_FontsStyles.GetAt(i);
			
			// cancellato o non dell'applicazione
			if (pFont->m_bDeleted)
				continue;
			TRACE(pFont->GetStyleName()+L"\n");

			// filtro di visualizzazione
			if (!m_sFilterStyle.IsEmpty() && pFont->GetStyleName().CompareNoCase(m_sFilterStyle))
				continue;
			
			if (pFont->m_FromAndTo == FontStyle::FROM_WOORM)
				hCurrParent = GetReportTreeItem();
			else if (pFont->m_FromAndTo == FontStyle::FROM_WOORM_TEMPLATE)
				hCurrParent = GetTplReportTreeItem();
			else
				continue;

			if (!hCurrParent)
				continue;

			nIco = pFont->m_FromAndTo == FontStyle::FROM_STANDARD ?  2 : 1;
			hItemIni = m_treeStyle.InsertItem
					(
						pFont->GetTitle().IsEmpty() ? pFont->GetStyleName() : pFont->GetTitle(), 
						nIco, 
						nIco, 
						hCurrParent
					);
			m_treeStyle.SetItemData(hItemIni, (DWORD_PTR) m_StyleTable.GetAt(n)->m_FontsStyles.GetAt(i));
			
			// se è il default di entrata lo memorizzo		
			FontStyle* pFnt = (FontStyle*) m_StyleTable.GetAt(n)->m_FontsStyles.GetAt(i);
			if (pFontDefault && pFontDefault == pFnt)
				m_DefaultSel = hItemIni;
		}
	}
}

//-----------------------------------------------------------------------------
HTREEITEM CFontStylesDlg::GetReportTreeItem()
{
	HTREEITEM hItemReport = m_treeStyle.GetRootItem();
	CTreeFontItemRef* pItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hItemReport);
	if (pItemRef->m_strName.CompareNoCase(szCurrentReport) == 0)
		return hItemReport;

	return 0;
}

//-----------------------------------------------------------------------------
HTREEITEM CFontStylesDlg::GetTplReportTreeItem()
{
	HTREEITEM hItemReport = m_treeStyle.GetRootItem();
	CTreeFontItemRef* pItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hItemReport);
	if (pItemRef->m_strName.CompareNoCase(szCurrentReport))
		return 0;

	HTREEITEM hItemTplReport = m_treeStyle.GetChildItem(hItemReport);
	if (!hItemTplReport)
		return 0;

	pItemRef = (CTreeFontItemRef*) m_treeStyle.GetItemData(hItemTplReport);
	if (pItemRef->m_strName.CompareNoCase(szTemplateCurrentReport))
		return 0;

	return hItemTplReport;
}

//--------------------------------------------------------------------------
// If edit control is visible in tree view control, when you send a
// WM_KEYDOWN message to the edit control it will dismiss the edit
// control. When the ENTER key was sent to the edit control, the
// parent window of the tree view control is responsible for updating
// the item's label in TVN_ENDLABELEDIT notification code.
//--------------------------------------------------------------------------
BOOL CFontStylesDlg::PreTranslateMessage(MSG* pMsg)
{
	if (
		pMsg->message == WM_KEYDOWN &&
		pMsg->wParam == VK_RETURN || pMsg->wParam == VK_ESCAPE 
		)
    {
		CEdit* edit =  m_treeStyle.GetEditControl();
        if (edit)
        {
           edit->SendMessage(WM_KEYDOWN, pMsg->wParam, pMsg->lParam);
           return TRUE;
        }
     }
	
	if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
	{
		m_treeStyle.SendMessage(VK_RETURN);
		m_bFromTree = TRUE;
	}
	
	return CParsedDialog::PreTranslateMessage(pMsg);
}

//--------------------------------------------------------------------------
// If the edit control of the tree view control has the input focus,
// sending a WM_KEYDOWN message to the edit control will dismiss the
// edit control.  When ENTER key was sent to the edit control, the
// parentwindow of the tree view control is responsible for updating
// the item's label in TVN_ENDLABELEDIT notification code.
//--------------------------------------------------------------------------
BOOL CFontStylesDlg::IsTreeCtrlEditMessage(WPARAM KeyCode)
{
	BOOL	bValue	= FALSE;
	CWnd*   pWnd	= this;

	if (!pWnd)
		ASSERT(FALSE);

	CTreeFontDialog* pTreeCtrl = (CTreeFontDialog*) pWnd->GetDlgItem(IDC_TREE_FONT);
	if (!pTreeCtrl)
		return bValue;
	
	CWnd*  Focus = GetFocus();
	CEdit* Edit  = pTreeCtrl->GetEditControl();

	if ((CEdit *) Focus == Edit)
	{
		Edit->SendMessage(WM_KEYDOWN, KeyCode); 
		bValue = TRUE;
	}
	return bValue;
}

//----------------------------------------------------------------------------
BOOL CFontStylesDlg::RefreshFontsTable ()
{
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;
	FontsParser         aParser;
	CStringArray		arModulesRefreshed;

	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			if (!pAddOnMod)
				continue;

			// lo comunico anche se non dovesse funzionare il , 
			// LoadFonts comunque qualcuno mi ha toccato la tabella.
			if (aParser.RefreshFonts( FontStyleTablePtr(&m_StyleTable, TRUE), pAddOnMod->m_Namespace, AfxGetPathFinder()))
				arModulesRefreshed.Add(pAddOnApp->GetTitle() + _T(" : ") + pAddOnMod->GetModuleTitle() + _T("\n"));
		}
	}

	if (arModulesRefreshed.GetSize())
	{
		CString sMessage;
		sMessage = _TB("Warning: Fonts.ini files in the disk are modified!\nProgram has update tree with new changes.\n\nModules updates are:\n\n");
		for (int i = 0; i <= arModulesRefreshed.GetUpperBound(); i++)
			sMessage += arModulesRefreshed.GetAt(i);
		sMessage += _TB("\n\nVerify customizations before save!");
		AfxMessageBox(sMessage, MB_APPLMODAL);
	}

	return arModulesRefreshed.GetSize();
}
