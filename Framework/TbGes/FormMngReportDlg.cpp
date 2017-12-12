
#include "stdafx.h"

#include <TbXMLCore\XMLDocObj.h>

#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\TbNamespaces.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\ReportObjectsInfo.h>
#include <TbGeneric\Dibitmap.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\Const.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlibUI\TbExplorer.h>
#include <TbGenlibUI\TbExplorerUtility.h>

#include <TbOleDb\SqlCatalog.h>
#include <TbOleDb\SqlConnect.h>
#include <TbOleDb\OleDbMng.h>
#include <TbOleDb\SqlRec.h>

#include <TbParser\XmlReportObjectsParser.h>

#include <TbGes\XMLGesInfo.h>

#include <TbWoormEngine\report.h>

#include "extdoc.h"
#include "XMLDocGenerator.h"
#include "formmngdlg.h"

//................................. resources
#include "formmng.hjson"
#include "tbges.hrc"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
#define IMAGE_NORMAL	0
#define IMAGE_DEFAULT	1
#define IMAGE_AUTO		2

//per i webmethods dei profili
static const TCHAR szDocumentNamespace[]		= _T("documentNamespace");
static const TCHAR szNewProfileName[]			= _T("newProfileName");
static const TCHAR szPosType[]				= _T("posType");
static const TCHAR szUserName[]				= _T("userName");
static const TCHAR szProfilePath[]			= _T("profilePath");
static const TCHAR szNewName[]				= _T("newName");
static const TCHAR szUserArray[]				= _T("userArray");

/////////////////////////////////////////////////////////////////////////////
//							CFormMngCache
///////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CAdminToolDocReportCache, CObject)
static CAdminToolDocReportCachePtr GetAdminToolDocReportCachePtr() 
{
	return CAdminToolDocReportCachePtr(AfxGetLoginContext()->GetObject<CAdminToolDocReportCache>(), TRUE); 
}

//==========================================================================
//							CTreeFormReportCtrl
//==========================================================================
BEGIN_MESSAGE_MAP(CTreeFormReportCtrl, CTBTreeCtrl)
	ON_WM_KEYDOWN		()	
	ON_WM_KEYUP			()
	ON_COMMAND			(ID_FORMDLGREPORT_DEFAULT,		OnSetAsDefault)
	ON_COMMAND			(ID_FORMDLGREPORT_REMOVE,		OnDelete)
	ON_COMMAND			(ID_FORMDLGREPORT_RENAMETITLE,	OnRename)
	ON_COMMAND			(ID_FORMDLGREPORT_PROPERTY,		OnProperty)
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,			OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,				OnItemEndEdit)
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTreeFormReportCtrl, CTBTreeCtrl)
//--------------------------------------------------------------------------
CTreeFormReportCtrl::CTreeFormReportCtrl()
	:
	m_bAfterCtrl	(FALSE)
{
	m_Menu.CreateMenu();
	CMenu popup;
	popup.CreatePopupMenu();
	popup.AppendMenu(MF_STRING, ID_FORMDLGREPORT_DEFAULT, _TB("Default"));
	popup.AppendMenu(MF_STRING, ID_FORMDLGREPORT_REMOVE, _TB("Delete"));
	popup.AppendMenu(MF_STRING, ID_FORMDLGREPORT_RENAMETITLE, _TB("Rename"));
	popup.AppendMenu(MF_SEPARATOR);
	popup.AppendMenu(MF_STRING, ID_FORMDLGREPORT_PROPERTY, _TB("Properties"));


	m_Menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _T(""));
	/*

	IDR_MENU_FORMMNG_REPORTDLG MENU
		BEGIN
		POPUP ""
		BEGIN
		MENUITEM "Default", ID_FORMDLGREPORT_DEFAULT
		MENUITEM "Delete", ID_FORMDLGREPORT_REMOVE
		MENUITEM "Rename", ID_FORMDLGREPORT_RENAMETITLE
		MENUITEM SEPARATOR
		MENUITEM "Properties", ID_FORMDLGREPORT_PROPERTY
		END
		END
		*/
}


//---------------------------------------------------------------------------
void CTreeFormReportCtrl::OnRButtonDown(UINT nFlags, CPoint point) 
{		
	__super::OnRButtonDown(nFlags, point);

	HTREEITEM hItem = HitTest(point);
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTreeFormReportCtrl::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect		rcTree;	
	HTREEITEM	hItemToSelect = GetSelectedItem();
	if (hItemToSelect)
	{
		CMenu* pPopup	= NULL;
		pPopup = m_Menu.GetSubMenu(0);
		CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();

		if (pPopup)
		{
			CDocumentReportDescription* pReport = (CDocumentReportDescription*) GetItemData(hItemToSelect);
			if (!pDlg->CanDelete())
			{
				pPopup->EnableMenuItem(ID_FORMDLGREPORT_REMOVE,		MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				pPopup->EnableMenuItem(ID_FORMDLGREPORT_RENAMETITLE,	MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			}
			else
			{
				pPopup->EnableMenuItem(ID_FORMDLGREPORT_REMOVE,		MF_BYCOMMAND | MF_ENABLED);
				if (pDlg->m_bIsRadar)
					pPopup->EnableMenuItem(ID_FORMDLGREPORT_RENAMETITLE,	MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				else
					pPopup->EnableMenuItem(ID_FORMDLGREPORT_RENAMETITLE,	MF_BYCOMMAND | MF_ENABLED);
			}

			pPopup->CheckMenuItem(ID_FORMDLGREPORT_DEFAULT, (pReport->IsDefault()) ? MF_CHECKED : MF_UNCHECKED);
		}

		ClientToScreen(&mousePos);	
		pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON,	mousePos.x, mousePos.y, this);		
	}
}

//---------------------------------------------------------------------
void CTreeFormReportCtrl::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{	
	CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();
	if (nChar == 46)
		if (pDlg->CanDelete())
			OnDelete();

	if (nChar == 113)
		if (pDlg->CanDelete())
			OnRename();
	
	if (nChar == VK_CONTROL)
		m_bAfterCtrl = TRUE;
	
	__super::OnKeyDown(nChar, nRepCnt, nFlags);	
}
//---------------------------------------------------------------------
void CTreeFormReportCtrl::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	m_bAfterCtrl = FALSE;
	__super::OnKeyUp(nChar, nRepCnt, nFlags);
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnDelete() 
{
	CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();
	pDlg->DeleteReport();
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnRename() 
{
	CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();
	pDlg->OnRemaneLabel();	
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnProperty() 
{
	HTREEITEM item = GetSelectedItem();
	CDocumentReportDescription* pRepsel = (CDocumentReportDescription*) GetItemData(item); 
	CFormReportDlg* pDlgFormReport = (CFormReportDlg*) GetParent();
	pDlgFormReport->Property(pRepsel);
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnItemBeginEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnItemEndEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	if (!lpDispInfo->item.pszText)
	{
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		return;
	}	

	if (!lpDispInfo->item.pszText[0])
	{
		*pResult = 0;
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		return;
	}

	CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();
	pDlg->RenameTitle(lpDispInfo->item.pszText);
	*pResult = 0;
	SelectItem(lpDispInfo->item.hItem);
}

//----------------------------------------------------------------------------
void CTreeFormReportCtrl::OnSetAsDefault()
{
	CFormReportDlg* pDlg = (CFormReportDlg*) GetParent();
	
	CMenu* submenu = m_Menu.GetSubMenu(0);
	UINT state = submenu->GetMenuState(ID_FORMDLGREPORT_DEFAULT, MF_BYCOMMAND);
	ASSERT(state != 0xFFFFFFFF);
	
	pDlg->SetAsDefault(!(state & MF_CHECKED)); //FALSE: DEVE ESSERE CONSIDERATO IL TRUE
}

/////////////////////////////////////////////////////////////////////////////
//						CFormReportDlg
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CFormReportDlg, CLocalizablePropertyPage)
//-----------------------------------------------------------------------------
CFormReportDlg::CFormReportDlg(CFormSheet* pSheet, CReportManager& aFormReports)
	: 
	CLocalizablePropertyPage	(aFormReports.m_bIsRadars ? IDD_FORMMNG_RADAR : (aFormReports.m_bIsBarcode ? IDD_FORMMNG_BARCODE : IDD_FORMMNG_REPORT  )),
	m_pSheet					(pSheet),
	m_FormReports				(aFormReports),
//	m_bModified					(FALSE),
	m_pRepSheet					(NULL),
	m_bIsRadar					(aFormReports.m_bIsRadars),
	m_bIsBarcode				(aFormReports.m_bIsBarcode),
	m_bSecondApply				(FALSE)
{	
	m_sUserForSave = AfxGetLoginInfos()->m_strUserName;
}

//-----------------------------------------------------------------------------
CFormReportDlg::CFormReportDlg(ReportMngDlg* pSheet, CReportManager& aFormReports)
	: 
	CLocalizablePropertyPage	(aFormReports.m_bIsRadars ? IDD_FORMMNG_RADAR : (aFormReports.m_bIsBarcode ? IDD_FORMMNG_BARCODE : IDD_FORMMNG_REPORT  )),
	m_pRepSheet					(pSheet),
	m_FormReports				(aFormReports),
	//m_bModified					(FALSE),
	m_pSheet					(NULL),
	m_bIsRadar					(aFormReports.m_bIsRadars),
	m_bIsBarcode				(aFormReports.m_bIsBarcode),
	m_bSecondApply				(FALSE)
{
	m_sUserForSave = AfxGetLoginInfos()->m_strUserName;
}

//-----------------------------------------------------------------------------
CFormReportDlg::~CFormReportDlg()
{
	m_ImageList.DeleteImageList();
	m_ImageListState.DeleteImageList();
}

/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CFormReportDlg, CLocalizablePropertyPage)
	//{{AFX_MSG_MAP(CFormReportDlg)
	ON_NOTIFY		(NM_DBLCLK,			IDC_FORMMNG_REPORT_LIST, OnDoubleClickTree)
	ON_NOTIFY		(TVN_SELCHANGED,	IDC_FORMMNG_REPORT_LIST, OnTreeSelchanged)
	ON_BN_CLICKED	(IDC_FORMMNG_REPORT_NEW,		OnAddReport)
	ON_BN_CLICKED	(IDC_FORMMNG_RADAR_NEW,			OnAddReport)
	ON_BN_CLICKED	(IDC_FORMMNG_BARCODE_NEW,		OnAddReport)
//	ON_BN_CLICKED	(IDC_FORMMNG_REPORT_EXEC,		OnRunReport)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
int CFormReportDlg::GetImage(CFunctionDescription* pReportInfo)
{
	int nImage = IMAGE_NORMAL;
	CTBNamespace Ns = pReportInfo->GetNamespace();
		
	CString strPath = AfxGetPathFinder()->GetFileNameFromNamespace(Ns, AfxGetLoginInfos()->m_strUserName);
	switch  (pReportInfo->m_XMLFrom)
	{
		case CDocumentReportDescription::XML_USER:
			nImage = 2;
			break;

		case CDocumentReportDescription::XML_MODIFIED:
			nImage = 2;
			break;

		case CDocumentReportDescription::XML_STANDARD:
			nImage = 0;
			break;

		case CDocumentReportDescription::XML_ALLUSERS:
			nImage = 1;
			break;
	}

	return nImage;
}

//-----------------------------------------------------------------------------
BOOL CFormReportDlg::OnInitDialog() 
{
	BOOL bInit = CLocalizablePropertyPage::OnInitDialog();

	if (m_bIsRadar)
	{
		m_ctrlTree.					SubclassDlgItem(IDC_FORMMNG_RADAR_LIST,	this);
		m_ctrlFormMngReportAdd.		SubclassDlgItem(IDC_FORMMNG_RADAR_NEW,	this);
	}
	else
	{
		if (m_bIsBarcode)
		{
			m_ctrlTree.					SubclassDlgItem(IDC_FORMMNG_BARCODE_LIST,	this);
			m_ctrlFormMngReportAdd.		SubclassDlgItem(IDC_FORMMNG_BARCODE_NEW,	this);
		}
		else
		{
			m_ctrlTree.					SubclassDlgItem(IDC_FORMMNG_REPORT_LIST,	this);
			m_ctrlFormMngReportAdd.		SubclassDlgItem(IDC_FORMMNG_REPORT_NEW,		this);
		}
	}

	LoadImageList();
	m_ctrlTree.SetImageList(&m_ImageList,		TVSIL_NORMAL);
	m_ctrlTree.SetImageList(&m_ImageListState,	TVSIL_STATE);
	
	if (!m_sNewReport.IsEmpty())
	{
		CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(m_sNewReport);
		if (ns.IsValid())
		{
			BOOL bMod = FALSE;
			AddReport(&ns, bMod);
		}
		m_sNewReport.Empty();
	}
	FillTree();
	return bInit;
}

//-----------------------------------------------------------------------------
void CFormReportDlg::LoadImageList()
{
	HICON	hIcon[3];
	int		n;

	m_ImageList.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());

	hIcon[0]	= TBLoadImage (TBGlyph(szIconStandard));
	hIcon[1]	= TBLoadImage(TBGlyph(szIconAllUsers));
	hIcon[2] =	TBLoadImage(TBGlyph(szIconUser));
		
	for (n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}

	HICON checked = TBLoadImage(TBIcon(szIconOk, CONTROL));
	HICON unchecked = TBLoadImage(TBIcon(szIconEmpty, CONTROL));

	m_ImageListState.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageListState.Add(unchecked);
	m_ImageListState.Add(checked);

	::DeleteObject(unchecked);
	::DeleteObject(checked);
}

//-----------------------------------------------------------------------
void CFormReportDlg::AddTreeElement(CDocumentReportDescription* pReportInfo, BOOL bSelect) 
{
	HTREEITEM hItem = m_ctrlTree.InsertItem(pReportInfo->GetTitle().IsEmpty() ? pReportInfo->GetName() : pReportInfo->GetTitle(), 0, 0, TVI_ROOT, TVI_LAST);
	m_ctrlTree.SetItemData(hItem, (DWORD)pReportInfo); 
	m_ctrlTree.SetItemImage(hItem, GetImage(pReportInfo), GetImage(pReportInfo)); 

	CDocumentReportDescription* pReportDescri = (CDocumentReportDescription*) pReportInfo;
	if (pReportDescri->IsDefault())
	{
		m_ctrlTree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(1), TVIS_STATEIMAGEMASK );
		m_ctrlTree.Select(hItem, TVGN_CARET);
		m_ctrlTree.SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
	}
	else
		m_ctrlTree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(2), TVIS_STATEIMAGEMASK );

}

//-----------------------------------------------------------------------
void CFormReportDlg::FillTree() 
{
	HTREEITEM hSel = m_ctrlTree.GetSelectedItem();
	m_ctrlTree.SetItemState(hSel, 0, TVIS_SELECTED );
	m_ctrlTree.SelectItem(NULL);
	m_ctrlTree.DeleteAllItems();

	int ub = m_FormReports.m_arShowReports.GetReports().GetUpperBound();
	for (int j = 0; j <= ub; j++)
	{
		CDocumentReportDescription* pRepDescr = (CDocumentReportDescription*) m_FormReports.m_arShowReports.GetReports().GetAt(j);
		AddTreeElement(pRepDescr, j == ub);
	}
}

//-----------------------------------------------------------------------
BOOL CFormReportDlg::DoSave(BOOL bFromApply)
{
	BOOL bSaved = TRUE;

	if (m_pSheet)
	{
		if (m_pSheet->m_bXMLModified)
		{
			bSaved = m_pSheet->SaveSheet(m_sUserForSave);
			m_FormReports.MakeGeneralArrayReport();
			if (bFromApply)
			{
				m_ctrlTree.SelectItem(NULL);
				FillTree();
			}
		}
	}
	else
	{
		HTREEITEM hSel = m_ctrlTree.GetSelectedItem();
		if (!m_bSecondApply && hSel != NULL)
		{
			CDocumentReportDescription* pDescr = (CDocumentReportDescription*) m_ctrlTree.GetItemData(m_ctrlTree.GetSelectedItem());
			m_sDefaultReportNamespace	= pDescr->GetNamespace();
			m_sDefaultReportTitle		= pDescr->GetTitle();
		}

		ASSERT( GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(ReportMngDlg)) );
		if (!m_pRepSheet->m_bUpdated)
		{
			bSaved = m_pRepSheet->SaveSheet(m_sUserForSave, &m_pRepSheet->m_RepManager);
			if (bSaved)
			{
				m_pRepSheet->SetUpdateable(FALSE);
				m_pRepSheet->m_RepManager.SetUserModified(FALSE);
				m_pRepSheet->m_RepManagerSource = m_pRepSheet->m_RepManager;
			}
		}

		if (bFromApply && bSaved && !m_sDefaultReportNamespace.IsEmpty())
		{
			m_pRepSheet->m_sReportName = m_sDefaultReportNamespace.ToString();
			m_pRepSheet->m_sTitleName  = m_sDefaultReportTitle;
			m_pRepSheet->m_RepManager.m_NsCurrDefault = m_sDefaultReportNamespace;
			m_bSecondApply = !m_bSecondApply;
		}
	}
	return bSaved;
}

//-----------------------------------------------------------------------
void CFormReportDlg::OnOK()
{
	ASSERT_VALID(this);
	DoSave (FALSE);
}

//-----------------------------------------------------------------------
BOOL CFormReportDlg::OnApply()
{
	ASSERT_VALID(this);
	BOOL bSave = DoSave (TRUE);
	return bSave;
}

//-----------------------------------------------------------------------
void CFormReportDlg::SetModified(BOOL bModified /*=TRUE*/)
{
	if (bModified)
		if (m_pSheet != NULL)
		{		
			ASSERT( GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CFormSheet)) );
			((CFormSheet*)GetParent())->m_bXMLModified = TRUE;
		}
		else
		{
			ASSERT( GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(ReportMngDlg)) );
			m_pRepSheet->SetUpdateable();
			((ReportMngDlg*)GetParent())->m_RepManager.SetUserModified();
			//((ReportMngDlg*)GetParent())->SetUpdateable();			
		}	
	
	CLocalizablePropertyPage::SetModified(bModified);
	
}

//--------------------------------------------------------------------------
void CFormReportDlg::OnDoubleClickTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (m_pRepSheet != NULL)
	{
		m_pRepSheet->PressButton(PSBTN_OK);  
		m_pRepSheet->EndDialog(IDOK);
	}	
}

//--------------------------------------------------------------------------
void CFormReportDlg::OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel = NULL;;
	hSel = m_ctrlTree.GetSelectedItem();
	
	if (!hSel)
	{
		((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPNAME))->SetWindowText(_T(""));
		((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPSEL))->SetWindowText(_T(""));
		m_ctrlTree.SetItemState(hSel, 0, TVIS_SELECTED );
		m_ctrlTree.SelectItem(hSel);
		return;
	}

	CDocumentReportDescription* pReportSel	= (CDocumentReportDescription*) m_ctrlTree.GetItemData(hSel);
	if (!pReportSel)
		return;

	if (pReportSel->GetNamespace().IsEmpty())
	{
		((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPNAME))->SetWindowText(_T(""));
		((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPSEL))->SetWindowText(_T(""));
		return;
	}

	((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPNAME))->SetWindowText(pReportSel->GetName());
	((CEdit*)GetDlgItem(IDC_FORMMNG_REPORT_REPSEL))->SetWindowText(pReportSel->GetTitle().IsEmpty() ?  pReportSel->GetName() : pReportSel->GetTitle());
	m_ctrlTree.SetItemState(hSel, TVIS_SELECTED, TVIS_SELECTED );
	m_ctrlTree.Select(hSel, TVGN_CARET);
	m_ctrlTree.SelectItem(hSel);
}

//-----------------------------------------------------------------------
void CFormReportDlg::DeleteReport()
{
	HTREEITEM hSel = m_ctrlTree.GetSelectedItem();
    CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) m_ctrlTree.GetItemData(hSel);
	ASSERT(pReportInfo);

	if (
		pReportInfo->m_XMLFrom == CBaseDescription::XML_STANDARD ||
		pReportInfo->m_XMLFrom == CBaseDescription::XML_ALLUSERS
	   )
		{
			AfxMessageBox(_TB("Warning, user cannot delete this report!"));
			return;
		}

	if (m_FormReports.m_arUserReports.RemoveReport(pReportInfo))
	{
		m_FormReports.SetUserModified();
		SetModified();
		m_FormReports.MakeGeneralArrayReport();
		m_ctrlTree.SetItemState(hSel, 0, TVIS_SELECTED );
		m_ctrlTree.SelectItem(NULL);
		FillTree();
	}	
}

//-----------------------------------------------------------------------------
void CFormReportDlg::OnRemaneLabel()
{
	m_ctrlTree.EditLabel(m_ctrlTree.GetSelectedItem());
	return;
}

//-----------------------------------------------------------------------------
void CFormReportDlg::RenameTitle(const CString& strSelItem)
{	
	HTREEITEM hSel = m_ctrlTree.GetSelectedItem();
    CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) m_ctrlTree.GetItemData(hSel);
	if (!pReportInfo)
		return;

	CDocumentReportDescription* pUserRep = m_FormReports.m_arUserReports.GetReportInfo(pReportInfo->GetNamespace());
	if (!pUserRep)
		return;

	pUserRep->SetNotLocalizedTitle(strSelItem);
	m_FormReports.SetUserModified();
	SetModified();
	m_FormReports.MakeGeneralArrayReport();
	m_ctrlTree.SetItemState(hSel, 0, TVIS_SELECTED );
	m_ctrlTree.SelectItem(NULL);
	FillTree();
}

//-----------------------------------------------------------------------
void CFormReportDlg::OnAddReport()
{
	AddReport();
}

//-----------------------------------------------------------------------------
BOOL CFormReportDlg::CanDelete()
{
	HTREEITEM hSel = m_ctrlTree.GetSelectedItem();
	if (!hSel)
		return FALSE;

    CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) m_ctrlTree.GetItemData(hSel);
	if (!pReportInfo)
		return FALSE;

	CDocumentReportDescription* pUserRep = m_FormReports.m_arUserReports.GetReportInfo(pReportInfo->GetNamespace());
	if (!pUserRep)
		return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------
void CFormReportDlg::AddReport(CTBNamespace* Ns, BOOL& bMod)
{
	if (!ExistReport(Ns))
	{	
		CDocumentReportDescription* pReportInfo = new CDocumentReportDescription();
			pReportInfo->SetNamespace(*Ns);
			pReportInfo->m_XMLFrom = CDocumentReportDescription::XML_USER;
			pReportInfo->SetNotLocalizedTitle(GetName(pReportInfo->GetName()));
		m_FormReports.m_arUserReports.AddReport(pReportInfo);
		m_FormReports.SetUserModified();

		CDocumentReportDescription* pReportInShow = new CDocumentReportDescription(*pReportInfo);
		m_FormReports.m_arShowReports.AddReport(pReportInShow);

		SetModified(); bMod = TRUE;
	}
	else
	{
		AfxMessageBox(_TB("Warning, report already present in the list!"));
	}
}

//-----------------------------------------------------------------------
void CFormReportDlg::AddReport()
{
	CTBNamespace ns;
	CTBExplorer TbExplorer(CTBExplorer::OPEN, ns, FALSE, FALSE, CTBExplorer::FORCE_USER);
	TbExplorer.SetMultiOpen();

	if (TbExplorer.Open() == FALSE)
		return;

	BOOL bMod = FALSE;
	CTBNamespaceArray arNamespace;
	TbExplorer.GetSelArrayNameSpace(arNamespace);

	for (int i = 0; i < arNamespace.GetSize(); i++)
	{
		AddReport(arNamespace.GetAt(i), bMod);
	}

	if (bMod) 
		FillTree();
}

//-----------------------------------------------------------------------
BOOL CFormReportDlg::ExistReport(CTBNamespace* Ns)
{
	CDocumentReportDescription* pReportInfo = m_FormReports.m_arShowReports.GetReportInfo(*Ns);
	if (pReportInfo)
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
void CFormReportDlg::SetAsDefault(BOOL bSetDef /*TRUE*/)
{
	// pulisce eventuali altri default (solo uno puo` esserlo)
	HTREEITEM	hSel		= m_ctrlTree.GetSelectedItem();
	if (!hSel)
		return;
	CDocumentReportDescription* pReportSel	= (CDocumentReportDescription*) m_ctrlTree.GetItemData(hSel);
	CDocumentReportDescription* pOldDefault = m_FormReports.m_arShowReports.GetDefault();

	if (!pReportSel)
		return;

	//if (!pOldDefault)
	//	return;

	//if (pReportSel == pOldDefault)
	//	return;
	
	if (bSetDef)
	{
		if (pReportSel->IsDefault())
			return;
		
		m_FormReports.SetUserModified();
		m_FormReports.m_NsUsr.SetNamespace(pReportSel->GetNamespace());
		if (m_FormReports.m_NsUsr == m_FormReports.m_NsAllUsrs)
			m_FormReports.m_NsUsr.Clear();		

		// elimino il precedente default se di USER ed esistente
		if (pOldDefault)
		{
			CDocumentReportDescription* pUserRep = m_FormReports.m_arUserReports.GetReportInfo(pOldDefault->GetNamespace());
			if (pUserRep)
				pUserRep->SetDefault(FALSE);
		}
	}
	else
	{
		if (pReportSel->m_XMLFrom == CDocumentReportDescription::XML_USER)
		{
			CDocumentReportDescription* pUserRep = m_FormReports.m_arUserReports.GetReportInfo(pOldDefault->GetNamespace());
			if (pUserRep)
				pUserRep->SetDefault(FALSE);
			m_FormReports.SetUserModified();
			m_FormReports.m_NsUsr.Clear();		
		}
	}	

	SetModified();
	m_FormReports.MakeGeneralArrayReport();
	FillTree();		
}

//-----------------------------------------------------------------------
void CFormReportDlg::Property(CDocumentReportDescription* pRepsel)
{
	if (!pRepsel)
		return;

	CPropertyReportDlg pPropDlg(pRepsel, GetImage(pRepsel));
	pPropDlg.DoModal();
}

//=============================================================================

//==========================================================================
//							CPropertyReportDlg
//==========================================================================
IMPLEMENT_DYNAMIC(CPropertyReportDlg, CLocalizableDialog)
//--------------------------------------------------------------------------
CPropertyReportDlg::CPropertyReportDlg(CDocumentReportDescription* pReportDescription, int nImage /*= 0*/ )
	: 
	CLocalizableDialog	 (IDD_FORMMNG_REPPROPERTIES),
	m_pReportDescription (pReportDescription),
	m_nImage			 (nImage) 
{	
}

//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPropertyReportDlg, CLocalizableDialog)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CPropertyReportDlg::~CPropertyReportDlg()
{
}

//--------------------------------------------------------------------------
void CPropertyReportDlg::DoDataExchange(CDataExchange* pDX)
{
	CLocalizableDialog::DoDataExchange(pDX);
}
//--------------------------------------------------------------------------
BOOL CPropertyReportDlg::OnInitDialog()
{
	CLocalizableDialog::OnInitDialog();
	((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_NAME))->SetWindowText(m_pReportDescription->GetName());
	
	//recupera la descrizione del report
	CString strFullPath = AfxGetPathFinder()->GetReportFullName(m_pReportDescription->GetNamespace(), AfxGetLoginInfos()->m_strUserName);
	CString strTitle = NULL;
	CString strSubject = NULL;
	if (!strFullPath.IsEmpty())
		::GetReportTitle(strFullPath,strTitle,strSubject);

	((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_REPORTTITLE))->SetWindowText(strTitle);
	((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_REPORTSUBJECT))->SetWindowText(strSubject);



	HICON hIconUsr =  TBLoadImage(TBGlyph(szIconUser));
	ASSERT(hIconUsr);

	HICON hIconAllUsr = TBLoadImage(TBGlyph(szIconAllUsers));

	ASSERT(hIconAllUsr);

	HICON hIconStd = TBLoadImage(TBGlyph(szIconStandard));
	ASSERT(hIconStd);

	switch (m_nImage)
	{
		case (2):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_XML))->SetWindowText(AfxGetLoginInfos()->m_strUserName);
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_XML))->SetIcon(hIconUsr);
			break;
		case (1):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_XML))->SetWindowText(StaticStrings::ALL_USERS());
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_XML))->SetIcon(hIconAllUsr);
			break;
		case (0):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_XML))->SetWindowText(_TB("Standard"));
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_XML))->SetIcon(hIconStd);
			break;	
	}
	
	switch (GetImageReport())
	{
		case (2):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_OWNER))->SetWindowText(AfxGetLoginInfos()->m_strUserName);
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_REPORT))->SetIcon(hIconUsr);
			break;
		case (1):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_OWNER))->SetWindowText(StaticStrings::ALL_USERS());
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_REPORT))->SetIcon(hIconAllUsr);
			break;
		case (0):
			((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_OWNER))->SetWindowText(_TB("Standard"));
			((CStatic*) GetDlgItem(IDC_MNGREPORT_IMG_REPORT))->SetIcon(hIconStd);
			break;	
	}

	((CButton*) GetDlgItem(IDC_FORMMNG_PROPERTY_DEF))->EnableWindow(FALSE); 
	((CButton*) GetDlgItem(IDC_FORMMNG_PROPERTY_DEF))->SetCheck(m_pReportDescription->IsDefault());
	
	AddOnApplication* pAddOnApplication = AfxGetAddOnApp(m_pReportDescription->GetNamespace().GetApplicationName());
	if (pAddOnApplication != NULL)
	{
		CString strAppTitle = pAddOnApplication->GetTitle();
		((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_APP))->SetWindowText(strAppTitle);
	}

	AddOnModule* pAddOnMod = AfxGetAddOnModule(m_pReportDescription->GetNamespace());
	if (pAddOnMod != NULL)
		((CEdit*) GetDlgItem(IDC_FORMMNG_PROPERTY_MOD))->SetWindowText(pAddOnMod->GetModuleTitle());

	::DeleteObject(hIconUsr);
	::DeleteObject(hIconAllUsr);
	::DeleteObject(hIconStd);
	return TRUE;
}

//-----------------------------------------------------------------------------
int CPropertyReportDlg::GetImageReport()
{
	CTBNamespace Ns = m_pReportDescription->GetNamespace();
		
	CString strPath = AfxGetPathFinder()->GetReportFullName(Ns, AfxGetLoginInfos()->m_strUserName);

	CString strUsr = AfxGetPathFinder()->GetUserNameFromPath(strPath);
	if (strUsr.IsEmpty())
		return 0;
	else
		if (strUsr.CompareNoCase(AfxGetLoginInfos()->m_strUserName) == 0)
			return 1;
	
	return 2;	
}

//==========================================================================
//							CTreeFormReportAdminDlg
//==========================================================================
BEGIN_MESSAGE_MAP(CTreeFormReportAdminDlg, CTBTreeCtrl)
	ON_WM_KEYDOWN		()	
	ON_WM_KEYUP			()
	ON_COMMAND			(ID_FORMDLGREPORT_ADMIN_DEFAULT,		OnSetAsDefault)
	ON_COMMAND			(ID_FORMDLGREPORT_ADMIN_REMOVE,		OnDelete)
	ON_COMMAND			(ID_FORMDLGREPORT_ADMIN_RENAMETITLE,	OnRename)
	ON_COMMAND			(ID_FORMDLGREPORT_ADMIN_PROPERTY,		OnProperty)
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,					OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,						OnItemEndEdit)
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTreeFormReportAdminDlg, CTBTreeCtrl)

//--------------------------------------------------------------------------
CTreeFormReportAdminDlg::CTreeFormReportAdminDlg()
	:
	m_bRename	(FALSE)
{
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnSetAsDefault()
{
	CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();
	pDlg->SetAsDefault();	
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnDelete() 
{
	CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();
	pDlg->DeleteElement();
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnRename() 
{
	CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();
	pDlg->RemaneLabel();	
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnProperty() 
{
	HTREEITEM hItem = GetSelectedItem();
	if (!hItem)
		return;

	if (GetParent()->IsKindOf(RUNTIME_CLASS(CAdminToolDocProfileDlg)))
	{
		CAdminProfileItem* pAdminProfileItem = (CAdminProfileItem*) GetItemData(hItem);
		((CAdminToolDocProfileDlg*) GetParent())->BtnModify(pAdminProfileItem);
		return;
	}

	CDocumentReportDescription* pRepsel = (CDocumentReportDescription*) GetItemData(hItem); 
	((CAdminToolDocReportDlg*) GetParent())->Property(pRepsel);
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnItemBeginEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();

	if (pDlg->m_bBtnStdPressed)
	{
		if (
				!pDlg->IsKindOf(RUNTIME_CLASS(CAdminToolDocProfileDlg)) ||
				!AfxGetBaseApp()->IsDevelopment()				
			)
		{
			*pResult = 1;
			return;
		}
	}
	
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
	
	if (pDlg->m_bBtnAllUsrPressed)
		pDlg->SetAllUsrModified(TRUE);

	if (pDlg->m_bBtnUsrPressed)
		pDlg->SetUsrModified(TRUE);
	
	m_bRename = TRUE;
	
	*pResult = 0;
}

//----------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnItemEndEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();
	if (pDlg->m_bBtnStdPressed)
	{
		if (
				!pDlg->IsKindOf(RUNTIME_CLASS(CAdminToolDocProfileDlg)) ||
				!AfxGetBaseApp()->IsDevelopment()			
			)
			return;
	}

	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	TV_ITEM* pItem= &lpDispInfo->item;
	*pResult = 1;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	if	(
			!pItem->pszText			 || 
			_tcslen(pItem->pszText) == 0 
		)
	{
		m_bRename = FALSE;
		return;
	}

	if (!lpDispInfo->item.pszText)
	{
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		return;
	}	

	if (!lpDispInfo->item.pszText[0])
	{
		*pResult = 0;
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		return;
	}

	pDlg->RenameTitle(lpDispInfo->item.pszText);

	*pResult = 0;
	SelectItem(lpDispInfo->item.hItem);
	m_bRename = FALSE;
}

//---------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{	
	if (nChar == 46 || nChar == 113)
	{
		CAdminToolDocReportDlg* pDlg = (CAdminToolDocReportDlg*) GetParent();
		if (pDlg->CanDelete())
		{
			if (nChar == 46)
				OnDelete();
			else
				OnRename();
		}
	}

	__super::OnKeyDown(nChar, nRepCnt, nFlags);	
}

//---------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	__super::OnKeyUp(nChar, nRepCnt, nFlags);
}

//---------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnRButtonDown(UINT nFlags, CPoint point) 
{		
	__super::OnRButtonDown(nFlags, point);

	HTREEITEM hItem = HitTest(point);
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTreeFormReportAdminDlg::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect		rcTree;	
	HTREEITEM	hItemToSelect = GetSelectedItem();

	if (hItemToSelect)
	{
		CMenu menu;
		menu.CreatePopupMenu();
		if(!menu)
			ASSERT(FALSE);

		CAdminToolDocReportDlg*	pDlg = (CAdminToolDocReportDlg*)	GetParent();

		if (GetParent()->IsKindOf(RUNTIME_CLASS(CAdminToolDocProfileDlg)))
		{
			CAdminProfileItem* pAdminProfileItem = (CAdminProfileItem*) GetItemData(hItemToSelect);
		
			if (!pDlg->m_bBtnStdPressed || AfxGetBaseApp()->IsDevelopment())
			{
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_PROPERTY,	_TB("Edit"));
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_REMOVE,		_TB("Delete"));
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_RENAMETITLE, _TB("Rename"));
				menu.AppendMenu(MF_SEPARATOR);
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_DEFAULT,		_TB("Preferenced"));
				menu.CheckMenuItem(ID_FORMDLGREPORT_ADMIN_DEFAULT, (pAdminProfileItem->IsPreferential()) ? MF_CHECKED : MF_UNCHECKED);
			}
		}
		else
		{
			CDocumentReportDescription* pReport = (CDocumentReportDescription*) GetItemData(hItemToSelect);
			if (!pDlg->m_bBtnStdPressed)
			{
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_REMOVE,		_TB("Delete"));
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_RENAMETITLE, _TB("Rename"));
				menu.AppendMenu(MF_SEPARATOR);
				menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_DEFAULT,		_TB("Default"));
			}
			menu.CheckMenuItem(ID_FORMDLGREPORT_ADMIN_DEFAULT, (pReport->IsDefault()) ? MF_CHECKED : MF_UNCHECKED);
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_FORMDLGREPORT_ADMIN_PROPERTY, _TB("Properties"));
		}
		
	
		ClientToScreen(&mousePos);	
		menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, mousePos.x, mousePos.y, this);		
	}
}

//============================================================================
//		CItemNoLocInDocRep implementation
//			per localizzazione
//============================================================================
CItemNoLocInDocRep::CItemNoLocInDocRep(const CString& strName)
{
	m_strName = strName;
}

//--------------------------------------------------------------------------
CItemNoLocInDocRep::CItemNoLocInDocRep (const CString& strName, const CTBNamespace& Ns)
{
	m_strName = strName;
	m_DocNamespace.SetNamespace(Ns);
}

//--------------------------------------------------------------------------
CItemNoLocInDocRep* CItemNoLocInDocRep::Create(Array& garbageCollector, const CString& strName)
{
	CItemNoLocInDocRep* pItem = new CItemNoLocInDocRep(strName);
	garbageCollector.Add(pItem);
	return pItem;
}

//--------------------------------------------------------------------------
CItemNoLocInDocRep* CItemNoLocInDocRep::Create(Array& garbageCollector, const CString& strName, const CTBNamespace& Ns)
{
	CItemNoLocInDocRep* pItem = new CItemNoLocInDocRep(strName, Ns);
	garbageCollector.Add(pItem);
	return pItem;
}
//==========================================================================
//							CAdminToolDocReportDlg
//==========================================================================
IMPLEMENT_DYNAMIC(CAdminToolDocReportDlg, CParsedDialog)
//--------------------------------------------------------------------------
CAdminToolDocReportDlg::CAdminToolDocReportDlg()
	: 
	CParsedDialog		(IDD_FORMMNG_REPORT_ADMIN, NULL),
	m_pWndParent		(NULL),
	m_bModifiedUsr		(FALSE),
	m_bModifiedAllUsr	(FALSE),
	m_bUsrModConditioned(FALSE),
	m_Tree				(),
	m_strUsr			(_T("")),
	m_bBtnUsrPressed	(FALSE),
	m_bBtnAllUsrPressed	(FALSE),
	m_bBtnStdPressed	(TRUE)
{	
	m_nPrevModSel = 0;
	m_nPrevAppSel = -1;
	m_nPrevDocSel = 0;
	//@@AUDITING
	m_bUseAuditing = AfxGetLoginInfos()->m_bAuditing &&  AfxIsActivated(TBEXT_APP, TBAUDITING_ACT);
}

//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAdminToolDocReportDlg, CParsedDialog)
	ON_WM_CLOSE			()
	ON_CBN_SELCHANGE	(IDC_FORMMNG_ADM_COMBO_USR,					OnComboUsrChanged)
	ON_LBN_SELCHANGE	(IDC_FORMMNG_ADM_LIST_DOC,					OnListDocChanged)
	ON_CBN_SELCHANGE	(IDC_FORMMNG_ADM_COMBO_MOD,					OnComboModsChanged)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_SAVE,					OnSave)	
	ON_COMMAND			(IDC_BTN_MNGREP_FILTER_STD,					OnBtnFilterStd)
	ON_COMMAND			(IDC_BTN_MNGREP_FILTER_ALLUSR,				OnBtnFilterAllUsrs)
	ON_COMMAND			(IDC_BTN_MNGREP_FILTER_USR,					OnBtnFilterUsr)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_ADD_AUDIT,				OnBtnAddAuditReport) //@@AUDITING
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_DELETE,				OnBtnDelete) 
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_RENAME,				OnBtnRename)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_ADD,					OnBtnAdd) 
	ON_NOTIFY			(LVN_ITEMCHANGED,	IDC_FORMMNG_LISTAPPS,	OnListAppSelchanged)
	ON_NOTIFY			(TVN_SELCHANGED,	IDC_FORMMNG_ADM_TREE,	OnTreeSelchanged)
	ON_NOTIFY			(NM_DBLCLK,			IDC_FORMMNG_ADM_TREE,	OnNMDblclkTree)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CAdminToolDocReportDlg::~CAdminToolDocReportDlg()
{
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::DoDataExchange(CDataExchange* pDX)
{
	CParsedDialog::DoDataExchange(pDX);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::ClearStoredInfo()
{
	CAdminToolDocReportCachePtr pCache = GetAdminToolDocReportCachePtr();
	//verifica che le due stringhe non siano vuote, se lo sono memorizza le info relative a company e user
	if (pCache->m_LastCompanyConnected == _T("") && pCache->m_LastUserConnected == _T(""))
	{
		pCache->m_LastCompanyConnected = AfxGetLoginInfos()->m_strCompanyName;
		pCache->m_LastUserConnected = AfxGetLoginInfos()->m_strUserName ;
	}
	else
	{
		//se company o user sono diversi da quelli memorizzati azzera le info relative all'ultimo modulo
		if (AfxGetLoginInfos()->m_strCompanyName != pCache->m_LastCompanyConnected 
			|| AfxGetLoginInfos()->m_strUserName != pCache->m_LastUserConnected )
		{
			pCache->m_LastCompanyConnected = AfxGetLoginInfos()->m_strCompanyName;
			pCache->m_LastUserConnected = AfxGetLoginInfos()->m_strUserName;
			pCache->m_LastUsedNameSpace.Clear();
		}
	}

}

//--------------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
	
	//Cancella le informazioni relative all'ultimo modulo selezionato
	CAdminToolDocReportDlg::ClearStoredInfo();

	VERIFY(m_ListDoc			.SubclassDlgItem	(IDC_FORMMNG_ADM_LIST_DOC,		this));
	VERIFY(m_Tree				.SubclassDlgItem	(IDC_FORMMNG_ADM_TREE,			this));
	VERIFY(m_BtnSave			.SubclassDlgItem	(IDC_FORMMNG_ADM_BTN_SAVE,		this));
	VERIFY(m_ApplicationsList	.SubclassDlgItem	(IDC_FORMMNG_LISTAPPS,			this));

	m_AppsImageList.Create(32, 32, ILC_COLOR32, 32, 32);
	m_AppsImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
	m_ApplicationsList.SetImageList(&m_AppsImageList, LVSIL_NORMAL);

	BeginWaitCursor();	
	SetStartNamespace();
	LoadImageList();
	FillAppsList();
	FillComboUsr();

	ASSERT(m_pToolBar);
	m_pToolBar->EnableButton(IDC_FORMMNG_ADM_COMBO_USR, FALSE);
	m_BtnSave.EnableWindow(FALSE);

	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_NEW))	->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MODIFY))	->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_CLONE))	->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_COPYIN))	->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MOVEIN))->ShowWindow(SW_HIDE);
	
	
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconStandard, IconSize::TOOLBAR));
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_USR, TBSTATE_ENABLED, TBIcon(szIconUser, IconSize::TOOLBAR));
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_ALLUSR, TBSTATE_ENABLED, TBIcon(szIconAllUsers, IconSize::TOOLBAR));

	//@@AUDITING
	if (m_bUseAuditing)
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->ShowWindow(SW_SHOW);
		
	EndWaitCursor();

	return bInit;
}

//-----------------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::OnCreateToolbar ()
{
	ASSERT(m_pToolBar == NULL);

	// Create Tabbed ToolBar
	ASSERT(m_pTabbedToolBar == NULL);
	m_pToolBar = new  CTBToolBar();
	if (!m_pToolBar->CreateEmpty(this, m_sName + _T("_Toolbar")))
		{ TRACE(L"Failed to create toolbar\n");	return FALSE; }
	
	m_pToolBar->AddEdit(IDC_MNGREP_TEXT, STANDARD_IMAGE_LIBRARY_NS, _T("MNGREP"));
	m_pToolBar->AddSeparator();
	m_pToolBar->AddComboBox(IDC_FORMMNG_ADM_COMBO_MOD, STANDARD_IMAGE_LIBRARY_NS, _T("FORMMNG_ADM"), 200,
		WS_CHILD | WS_VSCROLL | WS_VISIBLE | WS_TABSTOP | CBS_SORT | CBS_DROPDOWNLIST | CBS_AUTOHSCROLL | CBS_HASSTRINGS);
	m_pToolBar->AddSeparator();
	m_pToolBar->AddButton(IDC_BTN_MNGREP_FILTER_STD, _T("STD"), TBIcon(szIconStandard, IconSize::TOOLBAR), _TB("STD"));
	m_pToolBar->AddButton(IDC_BTN_MNGREP_FILTER_ALLUSR, _T("AllUsr"), TBIcon(szIconAllUsers, IconSize::TOOLBAR), _TB("All User"));
	m_pToolBar->AddButton(IDC_BTN_MNGREP_FILTER_USR, _T("USR"), TBIcon(szIconUser, IconSize::TOOLBAR), _TB("User"));

	m_pToolBar->AddSeparator();
	m_pToolBar->AddComboBox(IDC_FORMMNG_ADM_COMBO_USR, STANDARD_IMAGE_LIBRARY_NS, _T("FORMMNG_USR"), 200,
		WS_CHILD | WS_VSCROLL | WS_VISIBLE | WS_TABSTOP | CBS_SORT | CBS_DROPDOWNLIST | CBS_AUTOHSCROLL | CBS_HASSTRINGS);
	m_pToolBar->ShowInDialog(this);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::GetToolTipProperties(CTooltipProperties& tp)
{
	tp.m_strText.Empty();
	if (tp.m_nControlID == IDC_BTN_MNGREP_FILTER_USR)
		tp.m_strText = _TB("Filter by user");
	else if (tp.m_nControlID == IDC_BTN_MNGREP_FILTER_ALLUSR)
		tp.m_strText = _TB("Filter by All users");
	else if (tp.m_nControlID == IDC_BTN_MNGREP_FILTER_STD)
		tp.m_strText = _TB("Filter by Standard");
	return TRUE;
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::LoadImageList()
{
	HICON	hIcon[3];
	int		n;

	m_ImageList.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());

	hIcon[0] = TBLoadImage(TBGlyph(szIconStandard));
	hIcon[1] = TBLoadImage(TBGlyph(szIconAllUsers));
	hIcon[2] = TBLoadImage(TBGlyph(szIconUser));
		
	for (n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}

	HICON unchecked = TBLoadImage(TBIcon(szIconEmpty, CONTROL));
	HICON checked = TBLoadImage(TBIcon(szIconOk, CONTROL));

	m_ImageListState.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageListState.Add(unchecked);
	m_ImageListState.Add(checked);

	m_Tree.SetImageList(&m_ImageList,		TVSIL_NORMAL);
	m_Tree.SetImageList(&m_ImageListState,	TVSIL_STATE);

	::DeleteObject(unchecked);
	::DeleteObject(checked);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::SetStartNamespace()
{
	CString			strApp;
	AddOnModsArray* pMods = NULL;

	AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
	if (pApp)
		m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
	
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
	if (pAddOnApp)
		pMods = pAddOnApp->m_pAddOnModules;
	if (pMods)
	{
		for (int i=0; i <= pMods->GetUpperBound(); i++)
		{
			AddOnModule* pMod  = pMods->GetAt(i);
			if (!AfxIsActivated(pMod->GetApplicationName(), pMod->GetModuleName()))
				continue;
			m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMod->GetModuleName());
			break;
		}
	}

	m_NameSpace.SetType(CTBNamespace::DOCUMENT);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillAppsList()
{
	int					nApp			= 0;
	CString				strApps			= _T("");
	CString				strDefaultApp	= _T("");
	CItemNoLocInDocRep*	pItemNoLoc		= NULL;
	
	m_ApplicationsList.DeleteAllItems();
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;		
		BOOL bSysApp = AfxGetPathFinder()->IsASystemApplication(strApps);
		if (bSysApp)
			continue;
		
		if 	(
				(!AfxGetBaseApp()->IsDevelopment() &&
				 strApps.CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0)
			)
			continue;

		if (!AfxGetAddOnApp(strApps))
			continue;

		CString strTitle = AfxGetAddOnApp(strApps)->GetTitle();
		pItemNoLoc = CItemNoLocInDocRep::Create(m_arAppItemLoc, strApps);

		CString appIcon = TBGlyph(szIconTBFramework);

		AddOnApplication* pAddOnApplicationExt = AfxGetBaseApp()->GetMasterAddOnApp();
		if (strDefaultApp.IsEmpty() && strApps.CompareNoCase( pAddOnApplicationExt->m_strAddOnAppName) == 0)
			strDefaultApp = strApps;

		CString strIcon = appIcon;

		HICON hb = TBLoadImage(strIcon);
		ASSERT(hb);
		if (i == 0)
		m_AppsImageList.Create(20, 20, ILC_COLOR32, 20,20);
		m_AppsImageList.Add(hb);

		m_ApplicationsList.InsertItem(nApp, strTitle, nApp);
		m_ApplicationsList.SetItemData(nApp, (DWORD) pItemNoLoc);
		nApp++;
		DeleteObject(hb);
	}

	if (m_NameSpace.GetApplicationName().CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0)
	{	
		CStringArray	aModules;
		m_NameSpace.SetApplicationName(strDefaultApp);

		AddOnApplication* pAddOnApplication =  AfxGetAddOnApp(m_NameSpace.GetApplicationName());
		AddOnModule* pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(0);

		m_NameSpace.SetObjectName(CTBNamespace::MODULE, pAddOnMod->GetModuleName());
	}

	AddOnApplication* pSelApp = NULL;
	if (!m_NameSpace.IsEmpty())
		pSelApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());

	if (pSelApp)
		for (int nIdx = 0; nIdx <= m_ApplicationsList.GetItemCount(); nIdx++)
			if (m_ApplicationsList.GetItemText(nIdx, 0).CompareNoCase(pSelApp->GetTitle()) == 0)
			{
				m_ApplicationsList.SetItemState(nIdx, LVIS_SELECTED, LVIS_SELECTED | LVIS_FOCUSED);
				break;
			}

	FillComboMods();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillComboMods(const CString& strLabel/*= _TB("")*/, bool bFirst /*= FALSE */)
{
	CItemNoLocInDocRep*	pItemNoLocInDocRep = NULL;
	AddOnApplication*	pAddOnApplication = NULL;
	AddOnModule*		pAddOnModule = NULL;
	CString				strMods = _T("");
	CStringArray		aModules;
	CTBNamespace		Ns;
	int					nIdx;

	ASSERT(m_pToolBar);
	m_pToolBar->RemoveAllComboItems(IDC_FORMMNG_ADM_COMBO_MOD);

	if (strLabel.IsEmpty())
	{		
		pAddOnApplication = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
		for (int i = 0; i <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); i++)
		{
			pAddOnModule = pAddOnApplication->m_pAddOnModules->GetAt(i);

			//seleziona solo i moduli licenziati
			if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
				continue;

			pItemNoLocInDocRep = CItemNoLocInDocRep::Create(m_arAppItemLoc, pAddOnModule->GetModuleName());

			nIdx = m_pToolBar->AddComboSortedItem(IDC_FORMMNG_ADM_COMBO_MOD, pAddOnModule->GetModuleTitle(), (DWORD)pItemNoLocInDocRep);
		}

		pAddOnModule = AfxGetAddOnModule(m_NameSpace);
		if (pAddOnModule != NULL)
		{
			int n = m_pToolBar->FindComboStringExact(IDC_FORMMNG_ADM_COMBO_MOD, (LPCTSTR)pAddOnModule->GetModuleTitle());
			m_pToolBar->SetComboItemSel(IDC_FORMMNG_ADM_COMBO_MOD, n);
		}
	}
	else
	{
		pAddOnApplication =  AfxGetAddOnApp(strLabel);
		for (int a = 0; a <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); a++)
		{
			pAddOnModule = pAddOnApplication->m_pAddOnModules->GetAt(a);

			//seleziona solo i moduli licenziati
			if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
				continue;

			pItemNoLocInDocRep = CItemNoLocInDocRep::Create(m_arAppItemLoc, pAddOnModule->GetModuleName());
			nIdx = m_pToolBar->AddComboSortedItem(IDC_FORMMNG_ADM_COMBO_MOD, pAddOnModule->GetModuleTitle(), (DWORD)pItemNoLocInDocRep);
		}	

		m_pToolBar->SetComboItemSel(IDC_FORMMNG_ADM_COMBO_MOD, m_nPrevModSel);

		if (pAddOnApplication->m_pAddOnModules->GetSize())
			m_NameSpace = pAddOnApplication->m_pAddOnModules->GetAt(0)->m_Namespace;
	}

	FillListDoc();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillListDoc()
{	

	AddOnModule*			pAddOnMod			= AfxGetAddOnModule(m_NameSpace);
	if (!pAddOnMod)
		return;

	CBaseDescriptionArray* pDocArray = AfxGetDocumentDescriptionsOf(m_NameSpace);
	
	m_ListDoc.ResetContent();

	if (pDocArray->IsEmpty())
	{
		ClearTree();
		m_pToolBar->EnableButton(IDC_FORMMNG_ADM_COMBO_USR, FALSE);
		SetButtonsState();
		delete pDocArray;
		return;
	}

	m_pToolBar->EnableButton(IDC_FORMMNG_ADM_COMBO_USR, m_bBtnUsrPressed);

	FillDoc(pDocArray);
	
	if (pAddOnMod != NULL)
	{	
		if (m_ListDoc.GetCount() > 0)
		{
			m_ListDoc.SetCurSel(0);
			CItemNoLocInDocRep* pDocument = (CItemNoLocInDocRep*) m_ListDoc.GetItemData(0);
			m_NameSpace.SetNamespace(pDocument->m_DocNamespace/*->m_strName*/);
		}
	}

	FillTree();
	delete pDocArray;
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillDoc(const CBaseDescriptionArray* pDocArray)
{	
	if (pDocArray == NULL)
	{
		ASSERT (FALSE);
		return;
	}

	int						nIdx				= 0;
	CItemNoLocInDocRep*		pItemNoLocInDocRep	= NULL;
	for (int i = 0; i <= pDocArray->GetUpperBound(); i++)
	{
		CDocumentDescription* pDoc = (CDocumentDescription*) pDocArray->GetAt(i);

		//A.18471 - S.218524
		if (IsKindOf(RUNTIME_CLASS(CAdminToolDocProfileDlg)))
		{
			CString path = AfxGetPathFinder()->GetDocumentDocumentFullName(pDoc->GetNamespace());
			if (!ExistFile(path))
				continue;
		}

		nIdx = m_ListDoc.AddString(pDoc->GetTitle());
		pItemNoLocInDocRep = CItemNoLocInDocRep::Create(m_arAppItemLoc, pDoc->GetName(), pDoc->GetNamespace());
		m_ListDoc.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);
	}
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillTree(BOOL bModified /*FALSE*/)
{
	Fill();

	if (m_bBtnAllUsrPressed)
		return FillTreeAllUsrs();
	if (m_bBtnStdPressed)
		return FillTreeStd();
	
	FillTreeUserIfNecessary(bModified);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::Fill()
{
	m_ReportsMng.Parse(m_NameSpace, TRUE);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillTreeAllUsrs(BOOL bModified /*FALSE*/)
{
	ClearTree();
	FillForAllUsrs();
	SetButtonsState();

	if (bModified)
		m_BtnSave.EnableWindow(TRUE);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillForAllUsrs()
{
	int z = m_ReportsMng.m_arAllUsersReports.GetReports().GetUpperBound();
	for (int q = 0; q <= z; q++)
	{
		CDocumentReportDescription* pRepDescr = (CDocumentReportDescription*) m_ReportsMng.m_arAllUsersReports.GetReports().GetAt(q);
		AddTreeElement(pRepDescr, q == z);
	}
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillTreeStd()
{
	ClearTree();
	FillForStd();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::ClearTree()
{
	if (m_Tree.GetCount() == 0)
		return;

	m_Tree.DeleteAllItems();
	
	/*HTREEITEM					hItem						= m_Tree.GetFirstVisibleItem();

	while (hItem != NULL)
	{
		CDocumentReportDescription* pDocumentReportDescription = (CDocumentReportDescription*) m_Tree.GetItemData(hItem);
		SAFE_DELETE(pDocumentReportDescription);
		m_Tree.DeleteItem(hItem);
		hItem = m_Tree.GetFirstVisibleItem();
	}*/
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillForStd()
{
	int z = m_ReportsMng.m_arStandardReports.GetReports().GetUpperBound();
	for (int i = 0; i <= z; i++)
	{
		CDocumentReportDescription* pRepDescr = (CDocumentReportDescription*) m_ReportsMng.m_arStandardReports.GetReports().GetAt(i);
		AddTreeElement(pRepDescr, i == z);
	}

	SetButtonsState();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillComboUsr()
{
	CString			strUser;
	CStringArray	aUsers;
	int				nUserCount = AfxGetLoginInfos()->m_CompanyUsers.GetSize();
	
	m_pToolBar->RemoveAllComboItems(IDC_FORMMNG_ADM_COMBO_USR);
	m_pToolBar->AddComboSortedItem(IDC_FORMMNG_ADM_COMBO_USR, _TB("<No selection>"));

	for (int i = 0 ; i < nUserCount ; i++)
	{
		strUser = AfxGetLoginInfos()->m_CompanyUsers.GetAt(i);
		m_pToolBar->AddComboSortedItem(IDC_FORMMNG_ADM_COMBO_USR, strUser);
	}
		
	m_pToolBar->SetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR, 0);
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillTreeUsr(const CString& strUsr, BOOL bModified /*FALSE*/)
{
	ClearTree		();
	FillForUsr		(strUsr, bModified);	
	SetButtonsState	();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillForUsr(const CString& strUsr, BOOL bModified /*FALSE*/)
{
	if (bModified)
		m_ReportsMng.ParseUser(m_NameSpace, strUsr);
		
	int w = m_ReportsMng.m_arUserReports.GetReports().GetUpperBound();
	for (int f = 0; f <= w; f++)
	{
		CDocumentReportDescription* pRepDescr = (CDocumentReportDescription*) m_ReportsMng.m_arUserReports.GetReports().GetAt(f);
		AddTreeElement(pRepDescr, f == w);
	}
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::SetButtonsState()
{
	if (m_bBtnStdPressed || m_ListDoc.GetCount() == 0)
	{
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))	  ->EnableWindow(FALSE); 
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->EnableWindow(FALSE); 
		if (m_bUseAuditing)
			((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_DELETE))	  ->EnableWindow(FALSE); 
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_RENAME))	  ->EnableWindow(FALSE);
		return;
	}

	BOOL bEmpty = (m_Tree.GetCount() == 0);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_DELETE))		->EnableWindow(!bEmpty); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_RENAME))		->EnableWindow(!bEmpty);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))		->EnableWindow(TRUE); 
	if (m_bUseAuditing)
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->EnableWindow(TRUE); 
}


//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::AddTreeElement(CDocumentReportDescription* pReportInfo, BOOL bSelect) 
{
	HTREEITEM hItem = m_Tree.InsertItem(pReportInfo->GetTitle(), 0, 0, TVI_ROOT, TVI_LAST);
	m_Tree.SetItemData	(hItem, (DWORD)pReportInfo); 
	m_Tree.SetItemImage	(hItem, GetImage(pReportInfo), GetImage(pReportInfo)); 

	CDocumentReportDescription* pReportDescri = (CDocumentReportDescription*) pReportInfo;
	if (pReportDescri->IsDefault())
		m_Tree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(1), TVIS_STATEIMAGEMASK );
	else
		m_Tree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(2), TVIS_STATEIMAGEMASK );

	if (bSelect) 
		m_Tree.Select(hItem, TVGN_CARET); 
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::SetUsrModified(BOOL bMod)
{
	m_ReportsMng.SetUserModified(bMod);
	m_BtnSave.EnableWindow(bMod);
	m_bModifiedUsr = bMod;

	if (!bMod)
		m_bUsrModConditioned = FALSE;
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::SetAllUsrModified(BOOL bMod)
{
	m_ReportsMng.SetAllUsersModified(bMod);
	m_BtnSave.EnableWindow(bMod);
	m_bModifiedAllUsr = bMod;
}

//-----------------------------------------------------------------------------
int CAdminToolDocReportDlg::GetImage(CFunctionDescription* pReportInfo)
{
	int nImage = IMAGE_NORMAL;
	switch  (pReportInfo->m_XMLFrom)
	{
		case CDocumentReportDescription::XML_USER:
			nImage = 2;
			break;

		case CDocumentReportDescription::XML_MODIFIED:
			nImage = 1;
			break;

		case CDocumentReportDescription::XML_STANDARD:
			nImage = 0;
			break;

		case CDocumentReportDescription::XML_ALLUSERS:
			nImage = 1;
			break;
	}

	return nImage;
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnListAppSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	POSITION		Pos				= m_ApplicationsList.GetFirstSelectedItemPosition();
		
	BeginWaitCursor();
	if (m_ReportsMng.IsAllUsersModified() || m_ReportsMng.IsUserModified())
	{
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDYES)
		{
			SetAllUsrModified	(FALSE);
			SetUsrModified		(FALSE);
		}
		else
			return;
	}

	int				nItem			= 0;
	CString			strLabel		= _T("");
	CString			strApps			= _T("");
	CItemNoLoc*		pItemNoLoc		= NULL;

	while (Pos)
	{
		nItem		= m_ApplicationsList.GetNextSelectedItem(Pos);
		pItemNoLoc	= (CItemNoLoc*) m_ApplicationsList.GetItemData(nItem);
		strLabel	= pItemNoLoc->m_strName;

		m_pToolBar->RemoveAllComboItems(IDC_FORMMNG_ADM_COMBO_MOD);
			
		BOOL bFind = FALSE;
		for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
		{
			strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
			if (strApps.CompareNoCase(strLabel) == 0)
				bFind = TRUE; 
		}

		if (!bFind)
			strLabel = _T(""); 

		m_NameSpace.SetApplicationName(strLabel);
		m_NameSpace.SetObjectName(_T(""));
		FillComboMods(strLabel);			
	}
	EndWaitCursor();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnComboModsChanged()
{
	BeginWaitCursor();
	if (m_ReportsMng.IsAllUsersModified() || m_ReportsMng.IsUserModified())
	{
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDYES)
		{
			SetAllUsrModified(FALSE);
			SetUsrModified(FALSE);
		}
		else
			return;
	}

	int nCurSel = m_pToolBar->GetComboItemSel(IDC_FORMMNG_ADM_COMBO_MOD);
	CItemNoLocInDocRep* pItemNoLocInDocRep = (CItemNoLocInDocRep*)m_pToolBar->GetComboItemData(IDC_FORMMNG_ADM_COMBO_MOD, nCurSel);

	CString				strModulename		= pItemNoLocInDocRep->m_strName;
	
	m_NameSpace.SetObjectName(CTBNamespace::MODULE, strModulename);

	//memorizza l'ultimo modulo selezionato dall'utente
	CAdminToolDocReportCachePtr pCache = GetAdminToolDocReportCachePtr();
	pCache->m_LastUsedNameSpace = m_NameSpace;
	pCache->m_LastUsedNameSpace.SetObjectName(CTBNamespace::MODULE, strModulename);

	m_nPrevModSel = nCurSel;
	FillListDoc();
	EndWaitCursor();

}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnListDocChanged()
{
	BeginWaitCursor();
	
	if (m_ReportsMng.IsAllUsersModified() || m_ReportsMng.IsUserModified())
	{
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDYES)
		{
			SetAllUsrModified(FALSE);
			SetUsrModified(FALSE);
		}
		else
		{
			int						nIndex		= 0;
			const CDocumentDescription*	pXmlDocInfo = NULL;
            AddOnModule*			pAddOnMod	= AfxGetAddOnModule(m_NameSpace);
			CString					sTitle;
            if (pAddOnMod)
				pXmlDocInfo = AfxGetDocumentDescription(m_NameSpace);

            if (pXmlDocInfo)
                sTitle = pXmlDocInfo ->GetTitle();

			nIndex = m_ListDoc.FindStringExact(0, sTitle);
			m_ListDoc.SetCurSel(nIndex);
			return;
		}
	}

	int					nCurSel				= m_ListDoc.GetCurSel();
	CItemNoLocInDocRep* pItemNoLocInDocRep	= (CItemNoLocInDocRep*) m_ListDoc.GetItemData(nCurSel);
		
	m_NameSpace.SetNamespace(pItemNoLocInDocRep->m_DocNamespace);
	m_nPrevDocSel = nCurSel;
	if (m_bBtnUsrPressed)
		FillTreeUserIfNecessary(TRUE);
	else
		FillTree();
	EndWaitCursor();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnComboUsrChanged()
{
	BeginWaitCursor();
	FillTreeUserIfNecessary(TRUE);
	EndWaitCursor();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::FillTreeUserIfNecessary(BOOL bModified /* FALSE*/)
{
	if (!m_pToolBar->GetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR))
	{
		ClearTree();

		SetAllButtonsState(FALSE);
		m_strUsr = _T("");
		return;
	}

	m_strUsr = GetSelectedUsr();
	FillTreeUsr(m_strUsr, bModified);
	SetButtonsState();
}

//--------------------------------------------------------------------------
CString CAdminToolDocReportDlg::GetSelectedUsr()
{
	CString strUsr = _T("");

	int		nPos = m_pToolBar->GetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR);
	if (nPos >= 0)
		strUsr = m_pToolBar->GetComboItemSelText(IDC_FORMMNG_ADM_COMBO_USR);

	return strUsr;

}

//@@AUDITING
//-----------------------------------------------------------------------
CTBNamespace* CAdminToolDocReportDlg::CreateAuditReport()
{
	// dynamic documents can be in custom and in standard
	CString strDBTFileName = AfxGetPathFinder()->GetDocumentDbtsFullName(m_NameSpace);
	
	if (strDBTFileName.IsEmpty()) 
		return NULL;

	CString strDoc;
	m_ListDoc.GetText(m_ListDoc.GetCurSel(), strDoc);	

	CTBNamespace* pNewReport = NULL;
	CXMLDBTInfoArray aDBTArray;
	CLocalizableXMLDocument aXMLDBTDoc(m_NameSpace, AfxGetPathFinder());
	aXMLDBTDoc.EnableMsgMode(FALSE);	

	CString strTableName;
	if (aXMLDBTDoc.LoadXMLFile(strDBTFileName) && aDBTArray.Parse(&aXMLDBTDoc,  NULL))
	{
		for (int i = 0; i <= aDBTArray.GetUpperBound(); i++)
		{	
			if (aDBTArray.GetAt(i) && aDBTArray.GetAt(i)->GetType() == CXMLDBTInfo::MASTER_TYPE)
			{
				//prendo il nome della tabella
				strTableName = aDBTArray.GetAt(i)->GetTableName();
				const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(strTableName);
                if (pEntry)
				{
					SqlRecord* pRec = pEntry->CreateRecord();
					delete pRec; //istanziando il SqlRecord se la dll della tabella non è ancora caricata viene caricata ed il SqlRecord registrato
								 //durante la registrazione viene controllato se è sotto tracciatura.
					if (pEntry->IsTraced())
						pNewReport = (AfxMessageBox(cwsprintf(
										_TB("A new auditing data report for document {1-%s} will be created for {0-%s}.\nDo you want to continue?"), 
										(m_bBtnUsrPressed) ? cwsprintf(_TB("user {0-%s}"), m_strUsr) : _TB("all users"), 
										strDoc),
										MB_YESNO) == IDYES)
										? pEntry->CreateAuditingReport(&m_NameSpace, aDBTArray.GetAt(i)->GetXMLFixedKeyArray(), m_bBtnAllUsrPressed , m_strUsr)
										: NULL;
					else
							AfxMessageBox(cwsprintf(_TB("The selected document {0-%s} is not under audit management, so the report will not be created"), strDoc));
				}
				break;
			}
		}
	}
	else
		AfxMessageBox(cwsprintf(_TB("The document {0-%s} doesn't have the xml description so it is not possible to know the name of master table. The report will not be created."), strDoc));

	
	return pNewReport;
}

//-----------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::InsertNewReports(CTBNamespaceArray* pNamespaces)
{
	BOOL bMod = FALSE;

	if (pNamespaces->GetSize() <= 0)
		return FALSE;
	
	for (int i = 0; i < pNamespaces->GetSize(); i++)
	{
		CTBNamespace* Ns = pNamespaces->GetAt(i);
		if (!ExistReportInTot(Ns))
		{	
			CDocumentReportDescription* pReportInfo = new CDocumentReportDescription();
			pReportInfo->SetNamespace(*Ns);
			pReportInfo->SetNotLocalizedTitle(GetName(pReportInfo->GetName()));
			if (m_bBtnUsrPressed)
			{
				pReportInfo->m_XMLFrom = CDocumentReportDescription::XML_USER;
				m_ReportsMng.m_arUserReports.AddReport(pReportInfo);
			}
			else
			{
				//	if (ExistReportInUsr(Ns))
				//{
				//	//andrebbero cancellato in tutti i reports.xml degli utent in cui è presente
				//	AfxMessageBox(_TB("Warning, report already present in the user list!"));
				//}
				pReportInfo->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
				m_ReportsMng.m_arAllUsersReports.AddReport(pReportInfo);
			}
			bMod = TRUE;
		}
		else
		{
			if (AfxMessageBox(_TB("Warning, report already available in general list.\nDo you want to set as default?"), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
			{
				CDocumentReportDescription* pDef = m_ReportsMng.m_arAllUsersReports.GetDefault();
				if (!pDef || pDef->GetNamespace() != *Ns)
				{
					CDocumentReportDescription* pReportNew = new CDocumentReportDescription();
					pReportNew->SetNamespace(*Ns);
					pReportNew->SetNotLocalizedTitle(GetName(pReportNew->GetName()));
					if (m_bBtnUsrPressed)
					{
						pReportNew->m_XMLFrom = CDocumentReportDescription::XML_USER;
						m_ReportsMng.m_arUserReports.AddReport(pReportNew);
					}
					else
					{
						pReportNew->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
						m_ReportsMng.m_arAllUsersReports.AddReport(pReportNew);
					}

					// devo eliminare i precedente default e controllare che non provenga da lista totale....
					CDocumentReportDescription* pRepControl = m_ReportsMng.m_arUserReports.GetDefault();
					if (pRepControl)
					{
						if (
								m_ReportsMng.m_arStandardReports.GetReportInfo(pRepControl->GetNamespace()) ||
								m_ReportsMng.m_arAllUsersReports.GetReportInfo(pRepControl->GetNamespace())							
							)
						{
							m_ReportsMng.m_arUserReports.RemoveReport(pRepControl);
						}	
						else
						{
							if (m_ReportsMng.m_arUserReports.GetDefault())
								m_ReportsMng.m_arUserReports.GetDefault()->SetDefault(FALSE);
						}
							
					}

					pReportNew->SetDefault(TRUE);
					if (m_bBtnUsrPressed)
						m_ReportsMng.m_NsUsr.SetNamespace(*Ns);
					else
					{
						m_bUsrModConditioned = TRUE;
						m_ReportsMng.m_NsAllUsrs.SetNamespace(*Ns);
					}
					bMod = TRUE;
				}
				else 
				{
					if (m_ReportsMng.m_arUserReports.GetDefault())
						m_ReportsMng.m_arUserReports.GetDefault()->SetDefault(FALSE);
					m_ReportsMng.m_NsUsr.Clear();
					bMod = TRUE;
				}
			}			
		}
	}

	if (bMod)
	{
		if (m_bBtnUsrPressed)
		{
			SetUsrModified(bMod);
			FillTreeUsr(FALSE);
		}
		else
		{
			SetAllUsrModified(bMod);
			FillTreeAllUsrs(FALSE);
		}
	}

	return bMod;
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnAddAuditReport()
{
	CTBNamespaceArray	arNamespace;
	HTREEITEM hSel = m_Tree.GetSelectedItem();

	//chiedo conferma all'amministratore se vuole creare un nuovo report di auditing per l'utente selezionato oppure
	// per tutti gli utenti
	CTBNamespace* auditRepNamespace = CreateAuditReport();
	if (auditRepNamespace)
		arNamespace.Add(auditRepNamespace);

	BOOL bNew = InsertNewReports(&arNamespace);

	if (!bNew && hSel)
	{
		m_Tree.SetItemState(hSel, TVIS_SELECTED, TVIS_SELECTED);
		m_Tree.SetFocus();
	}
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnAdd()
{
	CTBNamespace ns;
	ns.SetApplicationName(m_NameSpace.GetApplicationName());
	ns.SetObjectName(CTBNamespace::MODULE, m_NameSpace.GetModuleName());

	BOOL bNew =  FALSE;
	CTBExplorer TbExplorer(CTBExplorer::OPEN, ns, 0, 0, CTBExplorer::DEFAULT);
	TbExplorer.SetMultiOpen();

	HTREEITEM hSel = m_Tree.GetSelectedItem();

	CTBNamespaceArray arNamespace;	
	if (TbExplorer.Open())
	{
		TbExplorer.GetSelArrayNameSpace(arNamespace);
		bNew = InsertNewReports(&arNamespace);		
	}

	if (!bNew && hSel)
	{
		m_Tree.SetItemState(hSel, TVIS_SELECTED, TVIS_SELECTED);
		m_Tree.SetFocus();
	}
}

//-----------------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::CanDelete()
{
	if (m_bBtnStdPressed)
		return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::ExistReportInTot(CTBNamespace* Ns)
{
	CDocumentReportDescription* pReportInfo = m_ReportsMng.m_arStandardReports.GetReportInfo(*Ns);
	if (pReportInfo)
		return TRUE;

	pReportInfo = m_ReportsMng.m_arAllUsersReports.GetReportInfo(*Ns);
	if (pReportInfo)
		return TRUE;
	return FALSE;	
}

//-----------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::ExistReportInUsr(CTBNamespace* Ns)
{
	CDocumentReportDescription* pReportInfo = m_ReportsMng.m_arUserReports.GetReportInfo(*Ns);
	if (pReportInfo)
		return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::DeleteElement()
{
	HTREEITEM hSelItem = m_Tree.GetSelectedItem();
	if (!hSelItem)
		return;

	CDocumentReportDescription* pRep = (CDocumentReportDescription*) m_Tree.GetItemData(hSelItem);
	if (pRep == NULL)
		return;

	if (pRep->m_XMLFrom == CBaseDescription::XML_STANDARD)
	{
		AfxMessageBox(_TB("Warning, impossible delete because it belong to a standard XML!"));
		return;
	}

	if (m_bBtnUsrPressed)
	{
		if(m_ReportsMng.m_arUserReports.RemoveReport(pRep))
		{
			FillTreeUserIfNecessary(FALSE);
			SetUsrModified(TRUE);
		}
		return;
	}


	// eliminare il report in usr come solo default.....
	CDocumentReportDescription* pRepUsr = (CDocumentReportDescription*) m_ReportsMng.m_arUserReports.GetReportInfo(pRep->GetNamespace());
	if (pRepUsr)
	{
		m_bUsrModConditioned = TRUE;
		SetUsrModified(TRUE);
	}

	if (m_ReportsMng.m_arAllUsersReports.RemoveReport(pRep))
	{
		SetAllUsrModified(TRUE);
		FillTreeAllUsrs(TRUE);
	}
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::SetAsDefaultUsr(BOOL bSetDef /*TRUE*/)
{
	// TODO CONTROLLO PER COERENZA CON ALLUSERS E STANDARD
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (hSel == NULL)
		return;
	
	CDocumentReportDescription* pReportSel = (CDocumentReportDescription*) m_Tree.GetItemData(hSel);
	if (!pReportSel)
		return;
	
	CDocumentReportDescription* pDefRep	= m_ReportsMng.m_arUserReports.GetDefault();
	if (pDefRep != NULL) // se nullo vuol dire che non ho un default	
		pDefRep->SetDefault(FALSE);
	
	if (bSetDef)
	{
		m_ReportsMng.m_NsUsr.SetNamespace(pReportSel->GetNamespace());
		pReportSel->SetDefault(TRUE);
	}

	SetUsrModified(TRUE);
	FillTreeUsr(GetSelectedUsr(), FALSE);
	
	return;
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::SetAsDefault()
{
	if (m_bBtnAllUsrPressed)
		SetAsDefaultAllUsr();
	if (m_bBtnUsrPressed)
		SetAsDefaultUsr();
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::SetAsDefaultAllUsr()
{
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (hSel == NULL)
		return;

	CDocumentReportDescription* pReportSel = (CDocumentReportDescription*) m_Tree.GetItemData(hSel);
	if (!pReportSel)
		return;

//	CDocumentReportDescription* pPreviousRepDef	= m_ReportsMng.m_arStandardReports.GetReportInfo(NsPrev);

	if (m_ReportsMng.m_NsAllUsrs.IsEmpty()) // se non è definito lo inserisco come tale
	{
		// controllo che non esiste nella std
		if (m_ReportsMng.m_NsStd == pReportSel->GetNamespace())
		{
			AfxMessageBox(_TB("Warning, report already set as default in general list!"));
			return;
		}
		else
		{
			// devo controllare che il report appartenga all'array allusr o std
			CDocumentReportDescription* pStd = (CDocumentReportDescription*) m_ReportsMng.m_arStandardReports.GetReportInfo(pReportSel->GetNamespace());
			if (pStd != NULL)
			{
				CDocumentReportDescription* pNew = new CDocumentReportDescription(*pStd);
				pNew->SetDefault(TRUE);
				pNew->m_XMLFrom = CDocumentReportDescription::XML_MODIFIED;
				m_ReportsMng.m_arAllUsersReports.AddReport(pNew);
			}
			else
				pReportSel->SetDefault(TRUE);
			m_ReportsMng.m_NsAllUsrs.SetNamespace(pReportSel->GetNamespace());		
		}
	}
	else // lo devo cambiare od annullare...
	{
		CDocumentReportDescription* pDefAllUsrRep = m_ReportsMng.m_arAllUsersReports.GetDefault();
		//se coincide con il precedente lo devo annullare....
		if (m_ReportsMng.m_NsAllUsrs == pReportSel->GetNamespace())
		{
			m_ReportsMng.m_NsAllUsrs.Clear();
			if (pDefAllUsrRep)
				pDefAllUsrRep->SetDefault(FALSE);
		}
		else
		{
			if (pDefAllUsrRep)
				pDefAllUsrRep->SetDefault(FALSE);

			// setto il nuovo default
			m_ReportsMng.m_NsAllUsrs.SetNamespace(pReportSel->GetNamespace());
			if (pDefAllUsrRep != NULL)
				pDefAllUsrRep->SetDefault(FALSE);

			if (m_ReportsMng.m_NsAllUsrs == m_ReportsMng.m_NsStd)		//caso in cui default di alluser coincide con quello std
			{
				m_ReportsMng.m_arAllUsersReports.RemoveReport(m_ReportsMng.m_arStandardReports.GetReportInfo(m_ReportsMng.m_NsStd));
				m_ReportsMng.m_NsAllUsrs.Clear();
			}
			else
			{
				// devo controllare che il report appartenga all'array allusr o std
				CDocumentReportDescription* pStd = (CDocumentReportDescription*) m_ReportsMng.m_arStandardReports.GetReportInfo(pReportSel->GetNamespace());
				if (pStd != NULL)
				{
					CDocumentReportDescription* pNew = new CDocumentReportDescription(*pStd);
					pNew->SetDefault(TRUE);
					pNew->m_XMLFrom = CDocumentReportDescription::XML_MODIFIED;
					m_ReportsMng.m_arAllUsersReports.AddReport(pNew);
				}
				else
					pReportSel->SetDefault(TRUE);
				m_ReportsMng.m_NsAllUsrs.SetNamespace(pReportSel->GetNamespace());		
			}
		}
	}	

	// devo fare i controlli su usr per rindondanza.....
/*	CDocumentReportDescription* pUsr = m_ReportsMng.m_arUserReports.GetDefault();
	if (pUsr && pUsr->GetNamespace() == m_ReportsMng.m_NsAllUsrs)
	{
		m_ReportsMng.m_arUserReports.RemoveReport(pUsr);
		m_ReportsMng.m_NsUsr.Clear();
		m_bUsrModConditioned = TRUE;
		SetUsrModified(TRUE);
		FillTreeUserIfNecessary(TRUE);
	}*/

	SetAllUsrModified(TRUE);
	FillTreeAllUsrs(TRUE);
	return;
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnSave()
{
	if (m_bBtnAllUsrPressed && m_ReportsMng.IsAllUsersModified())
		Save(TRUE, _T(""));

	if (m_bBtnUsrPressed && m_ReportsMng.IsUserModified())
		Save(FALSE, GetSelectedUsr());

	CParsedDialog::EndDialog(IDOK);
}


//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnFilterStd()
{
	if (m_bBtnStdPressed)
	{
		m_pToolBar->SetButtonInfo((UINT) IDC_BTN_MNGREP_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconStandard, IconSize::TOOLBAR));
		return;
	}

	if (m_ReportsMng.IsAllUsersModified() || m_ReportsMng.IsUserModified())
	{
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDYES)
		{
			SetAllUsrModified(FALSE);
			SetUsrModified(FALSE);
		}
		else
			return;
	}
	
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconStandard, IconSize::TOOLBAR));

	m_bBtnStdPressed	= TRUE;	
	m_bBtnAllUsrPressed = m_bBtnUsrPressed = FALSE;
	FilterUsrClicked	(FALSE);
	FilterAllUsrClicked	(FALSE);
	FillTree			(TRUE);		
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnFilterAllUsrs()
{
	if (m_ReportsMng.IsUserModified())
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDNO)
			return;

	if (m_bBtnAllUsrPressed)
	{
		m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, IconSize::TOOLBAR));
		return;
	}

	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))->EnableWindow(TRUE); 
	if (m_bUseAuditing)
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->EnableWindow(TRUE); 
	
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, IconSize::TOOLBAR));

	m_bBtnAllUsrPressed = TRUE;
	m_bBtnStdPressed	= m_bBtnUsrPressed = FALSE;
	FilterUsrClicked(FALSE);
	FilterStdClicked(FALSE);

	BOOL bMod = TRUE;
	if (m_ReportsMng.IsUserModified())
	{
		bMod = FALSE;
		SetUsrModified(bMod);
	}
	FillTree(bMod);
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnFilterUsr()
{
	if (m_bBtnUsrPressed)
	{
		m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, IconSize::TOOLBAR));
		return;
	}	
	
	if (m_ReportsMng.IsAllUsersModified())
		if (AfxMessageBox(_TB("Warning, with this choice the changes will be lost!\nDo you want to continue?"), MB_YESNO) == IDNO)
			return;

	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))->EnableWindow(FALSE);
	if (m_bUseAuditing)
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->EnableWindow(FALSE); 

	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, IconSize::TOOLBAR));

	m_BtnSave.EnableWindow(FALSE);
	m_bBtnUsrPressed = !m_bBtnUsrPressed;
	m_bBtnAllUsrPressed = m_bBtnStdPressed = FALSE;
	FilterStdClicked	(FALSE);
	FilterAllUsrClicked	(FALSE);

	m_pToolBar->EnableButton(IDC_FORMMNG_ADM_COMBO_USR);
	if (m_strUsr.IsEmpty())
		m_pToolBar->SetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR, 0);
	else
		m_strUsr = m_pToolBar->GetComboItemSelText(IDC_FORMMNG_ADM_COMBO_USR);
	
	BOOL bMod = TRUE;
	if (m_ReportsMng.IsAllUsersModified())
	{
		bMod = FALSE;
		m_ReportsMng.SetAllUsersModified(bMod);
	}

	FillTree(bMod);	
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::FilterUsrClicked(BOOL bPressed)
{
	if (bPressed)
	{
		m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, IconSize::TOOLBAR));
		return;
	}

	m_bBtnUsrPressed = bPressed;
	m_pToolBar->EnableButton(IDC_BTN_MNGREP_FILTER_USR);
	m_pToolBar->SetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR, 0);
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_USR, TBSTATE_ENABLED, TBIcon(szIconUser, IconSize::TOOLBAR));
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::FilterAllUsrClicked(BOOL bPressed)
{
	if (bPressed)
	{
		m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, IconSize::TOOLBAR));
		return;
	}

	m_bBtnAllUsrPressed = FALSE;
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_ALLUSR, TBSTATE_ENABLED, TBIcon(szIconAllUsers, IconSize::TOOLBAR));
}

//----------------------------------------------------------------------------
void CAdminToolDocReportDlg::FilterStdClicked(BOOL bPressed)
{
	if (bPressed)
	{
		m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconStandard, IconSize::TOOLBAR));
		return;
	}

	m_bBtnStdPressed = FALSE;
	m_pToolBar->SetButtonInfo((UINT)IDC_BTN_MNGREP_FILTER_STD, TBSTATE_ENABLED, TBIcon(szIconStandard, IconSize::TOOLBAR));
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (hSel == NULL)
		return;
	
	CDocumentReportDescription* pReportSel	= (CDocumentReportDescription*) m_Tree.GetItemData(hSel);
	if (!pReportSel)
		return;

	m_Tree.SetItemState(hSel, TVIS_SELECTED, TVIS_SELECTED);
	if (m_bBtnStdPressed)
		SetAllButtonsState(FALSE);
	else
		SetAllButtonsState(TRUE);
	
	m_Tree.SetFocus();
}

//--------------------------------------------------------------------------
void CAdminToolDocReportDlg::SetAllButtonsState(BOOL bEnabled /*TRUE*/)
{
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_DELETE))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_RENAME))	->EnableWindow(bEnabled);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_NEW))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MODIFY))	->EnableWindow(bEnabled);
	if (m_bUseAuditing)
		((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->EnableWindow(bEnabled); 
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::OnNMDblclkTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hItem = m_Tree.GetSelectedItem();
	if (!hItem )
		return;

	CDocumentReportDescription* pRepsel = (CDocumentReportDescription*) m_Tree.GetItemData(hItem); 
	Property(pRepsel);
}

//-----------------------------------------------------------------------
void CAdminToolDocReportDlg::Property(CDocumentReportDescription* pRepsel)
{
	if (!pRepsel)
		return;

	CPropertyReportDlg pPropDlg(pRepsel, GetImage(pRepsel));
	pPropDlg.DoModal();
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::RemaneLabel()
{
	HTREEITEM hItem = m_Tree.GetSelectedItem();
	if (!hItem)
		return;

	m_Tree.EditLabel(hItem);
}

//-----------------------------------------------------------------------------
void CAdminToolDocReportDlg::RenameTitle(const CString& strSelItem)
{	
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (!hSel)
		return;

    CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) m_Tree.GetItemData(hSel);
	if (!pReportInfo)
		return;

	CDocumentReportDescription* pRepRename = NULL;
	if (m_bBtnAllUsrPressed)
		pRepRename = m_ReportsMng.m_arAllUsersReports.GetReportInfo(pReportInfo->GetNamespace());
	else
		pRepRename = m_ReportsMng.m_arUserReports.GetReportInfo(pReportInfo->GetNamespace());

	if (!pRepRename)
		return;

	pRepRename->SetNotLocalizedTitle(strSelItem);
	if (m_bBtnAllUsrPressed)
		SetAllUsrModified(TRUE);
	else
		SetUsrModified(TRUE);

	m_ReportsMng.MakeGeneralArrayReport();
	m_Tree.SetItemState(hSel, 0, TVIS_SELECTED);
	m_Tree.SelectItem(NULL);
	//FillTree(FALSE);
	if (m_bBtnUsrPressed)
		FillTreeUserIfNecessary(FALSE);
	if (m_bBtnAllUsrPressed)
		FillTreeAllUsrs();
}

//-----------------------------------------------------------------------
BOOL CAdminToolDocReportDlg::Save(BOOL bAllUsers, const CString& strUsr)
{
	CDocumentReportDescription* pDefDefined;
	CXMLReportObjectsParser		aParserUsr; 
	CXMLDocumentObject			aXMLDocUsr;
	CString						sFileName;

	if (bAllUsers && m_bModifiedAllUsr)
	{
		sFileName	= AfxGetPathFinder()->GetDocumentReportsFile(m_NameSpace, CPathFinder::ALL_USERS);
		pDefDefined = m_ReportsMng.m_arAllUsersReports.GetDefault();
		
		if (!m_ReportsMng.m_arAllUsersReports.GetReports().IsEmpty() || pDefDefined)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &m_ReportsMng.m_arAllUsersReports, pDefDefined);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			DeleteFile(sFileName);

		SetAllUsrModified(FALSE);
		m_bUsrModConditioned = FALSE;
		return TRUE;
	}
	
	if (!strUsr.IsEmpty())	
	{
		sFileName = AfxGetPathFinder()->GetDocumentReportsFile(m_NameSpace, CPathFinder::USERS, strUsr);
		if (m_bUsrModConditioned)
		{
			AfxMessageBox(_TB("Warning, for saving this user configuration you must before save generic configuration."));
			return TRUE;
		}

		pDefDefined = m_ReportsMng.m_arUserReports.GetDefault();

		if (!m_ReportsMng.m_arUserReports.GetReports().IsEmpty() || pDefDefined)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &m_ReportsMng.m_arUserReports, pDefDefined);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			DeleteFile(sFileName);

		SetUsrModified(FALSE);
		return TRUE;
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnDelete() 
{
	DeleteElement();
}

//----------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnBtnRename()
{
	RemaneLabel();
}


//----------------------------------------------------------------------------
void CAdminToolDocReportDlg::OnClose()
{
	if ((m_ReportsMng.IsAllUsersModified() || m_ReportsMng.IsUserModified()) &&
		 AfxMessageBox(_TB("Warning, closing window the changes will be lost.\nDo you want to close?"), MB_YESNO) == IDNO)
	return; 
	
	CParsedDialog::OnClose();
}


//============================================================================
//		CAdminProfileItem 
//============================================================================
CAdminProfileItem::CAdminProfileItem(CString strPath, BOOL bIsPreferential /*FALSE*/, CPathFinder::PosType aPosType /*CPathFinder::PosType::STANDARD*/, const CString& strUsrName /*""*/)
{
	if (strPath.IsEmpty())
		ASSERT(FALSE);

	m_strPath			= strPath;
	m_strProfileName	= GetName(m_strPath);
	m_bIsPreferential	= bIsPreferential;
	m_PosType			= aPosType;
	m_strUsrName		= strUsrName;
}

//--------------------------------------------------------------------------
CAdminProfileItem::CAdminProfileItem (CString strPath, CPathFinder::PosType aPosType /*CPathFinder::PosType::STANDARD*/, const CString& strUsrName)
{
	if (strPath.IsEmpty())
		ASSERT(FALSE);

	m_strPath			= strPath;
	m_strProfileName	= GetName(m_strPath);	
	m_bIsPreferential	= FALSE;
	m_PosType			= aPosType;
	m_strUsrName		= strUsrName;
}

//==========================================================================
//							CAdminToolDocProfileDlg
//==========================================================================
IMPLEMENT_DYNAMIC(CAdminToolDocProfileDlg, CAdminToolDocReportDlg)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAdminToolDocProfileDlg, CAdminToolDocReportDlg)
	ON_WM_CLOSE			()
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_NEW,					OnBtnNew)	
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_MODIFY,				OnBtnModify) 
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_RENAME,				OnBtnRename)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_DELETE,				OnBtnDelete)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_CLONE,					OnBtnClone)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_COPYIN,				OnBtnCopyIn)
	ON_COMMAND			(IDC_FORMMNG_ADM_BTN_MOVEIN,				OnBtnMoveIn)
	ON_NOTIFY			(NM_DBLCLK,			IDC_FORMMNG_ADM_TREE,	OnNMDblclkTree)
	ON_NOTIFY			(TVN_SELCHANGED,	IDC_FORMMNG_ADM_TREE,	OnTreeSelchanged)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------
CAdminToolDocProfileDlg::CAdminToolDocProfileDlg()
	: 
	CAdminToolDocReportDlg	()
{	
}

//--------------------------------------------------------------------------
CAdminToolDocProfileDlg:: ~CAdminToolDocProfileDlg()
{
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FindAllProfiles()
{
	FindStdProfiles();
	FindAllUsersProfiles();
	FindUserProfiles();
}

//--------------------------------------------------------------------------
BOOL CAdminToolDocProfileDlg::OnInitDialog()
{
	BOOL bResult = CAdminToolDocReportDlg::OnInitDialog();
	((CStatic*) GetDlgItem(IDC_STATIC_ELEMENT))->SetWindowText(_TB("Profiles"));
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD_AUDIT))->ShowWindow(SW_HIDE);	
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_NEW))->ShowWindow(SW_SHOW);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MODIFY))->ShowWindow(SW_SHOW);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_CLONE))->ShowWindow(SW_SHOW);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_COPYIN))->ShowWindow(SW_SHOW);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MOVEIN))->ShowWindow(SW_SHOW);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_SAVE))->ShowWindow(SW_HIDE);
	((CButton*) GetDlgItem(IDCANCEL))->SetWindowText(_TB("Close"));

	SetWindowText(_TB("Administration tools - Document profiles"));
	return bResult;
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FindStdProfiles()
{
	m_arStdProfile.RemoveAll();
	AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::STANDARD),(CStringArray&) m_arStdProfile, FALSE);
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FindAllUsersProfiles()
{
	m_arAllUsersProfile.RemoveAll();
	AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::ALL_USERS), (CStringArray&) m_arAllUsersProfile, FALSE);
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FindUserProfiles()
{
	CString strUsr = GetSelectedUsr();

	m_arUserProfile.RemoveAll();
	if (!strUsr.IsEmpty())
		AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::USERS, strUsr), (CStringArray&) m_arUserProfile, FALSE);
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FillDoc(const CBaseDescriptionArray* pDocArray)
{	
	if (pDocArray == NULL)
	{
		ASSERT (FALSE);
		return;
	}

	int						nIdx				= 0;
	CString 				strXMLDocDescr;
	CItemNoLocInDocRep*		pItemNoLocInDocRep	= NULL;
	CDocumentDescription*	pDoc				= NULL;
	for (int i = 0; i <= pDocArray->GetUpperBound(); i++)
	{
		pDoc			= (CDocumentDescription*) pDocArray->GetAt(i);
		// dynamic documents can be in custom or in standard
		strXMLDocDescr	= AfxGetPathFinder()->GetDocumentDocumentFullName(pDoc->GetNamespace());
		if (pDoc->IsTransferDisabled() || strXMLDocDescr.IsEmpty() || !ExistFile(strXMLDocDescr))
			continue;
		
		pItemNoLocInDocRep	= CItemNoLocInDocRep::Create(m_arAppItemLoc, pDoc->GetName(), pDoc->GetNamespace());
		nIdx				= m_ListDoc.AddString(pDoc->GetTitle());
		m_ListDoc.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);
	}	
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::Fill()
{
	FindAllProfiles();
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::ClearTree()
{
	if (m_Tree.GetCount() == 0)
		return;

	HTREEITEM			hItem	= NULL;
	CAdminProfileItem*	pAdProf = NULL;

	hItem = m_Tree.GetFirstVisibleItem();
	while (hItem != NULL)
	{
		pAdProf = (CAdminProfileItem*) m_Tree.GetItemData(hItem);
		SAFE_DELETE(pAdProf);
		m_Tree.DeleteItem(hItem);
		hItem = m_Tree.GetFirstVisibleItem();
	}
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FillForStd()
{
	int i = m_arStdProfile.GetUpperBound();
	for (int n = 0; n <= i; n++)
		AddTreeStdEl((LPCTSTR)m_arStdProfile.GetAt(n), n == i);

	SetButtonsState();		
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FillForAllUsrs()
{
	int i = m_arAllUsersProfile.GetUpperBound();
	for (int n = 0; n <= i; n++)
		AddTreeAllUsrsEl((LPCTSTR)m_arAllUsersProfile.GetAt(n), n == i);

	SetButtonsState();	
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::FillForUsr(const CString& strUsr, BOOL bModified /*FALSE*/)
{
	FindUserProfiles();
    int	n = m_arUserProfile.GetUpperBound(); 
	for (int i = 0; i <= n; i++)
		AddTreeUsrEl((LPCTSTR) m_arUserProfile.GetAt(i), n == i);

	SetButtonsState();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::AddTreeStdEl(const CString& strProfilePath, BOOL bSelect/*FALSE*/) 
{
	CAdminProfileItem* pAdminProfileItem = new CAdminProfileItem(strProfilePath, CPathFinder::STANDARD);
	AddTreeEl(pAdminProfileItem, bSelect);
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::AddTreeAllUsrsEl(const CString& strProfilePath, BOOL bSelect/*FALSE*/) 
{
	CAdminProfileItem* pAdminProfileItem = new CAdminProfileItem(strProfilePath, CPathFinder::ALL_USERS);
	AddTreeEl(pAdminProfileItem, bSelect); 
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::AddTreeUsrEl (const CString& strProfilePath, BOOL bSelect/*FALSE*/) 
{
	CAdminProfileItem* pAdminProfileItem = new CAdminProfileItem(strProfilePath, CPathFinder::USERS, GetSelectedUsr());
	AddTreeEl(pAdminProfileItem, bSelect);
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::AddTreeEl (CAdminProfileItem* pAdminProfileItem, BOOL bSelect /*= FALSE*/) 
{
	HTREEITEM hItem = m_Tree.InsertItem((LPCTSTR) pAdminProfileItem->GetProfileName(), 0, 0, TVI_ROOT, TVI_LAST);
	m_Tree.SetItemData	(hItem, (DWORD) pAdminProfileItem); 
	m_Tree.SetItemImage	(hItem, GetImage(pAdminProfileItem->GetPosType()), GetImage(pAdminProfileItem->GetPosType())); 

	CXMLDefaultInfo aDefaultInfo(m_NameSpace);
	aDefaultInfo.Parse(pAdminProfileItem->GetPosType(), pAdminProfileItem->GetUserName());

	CString strPrefProfileName	= aDefaultInfo.GetPreferredProfile();
	CString strProfileName		= pAdminProfileItem->GetProfileName();
	if (!strPrefProfileName.IsEmpty() && strProfileName.CompareNoCase(strPrefProfileName) == 0)
		pAdminProfileItem->SetPreferential(TRUE);

	if (pAdminProfileItem->IsPreferential())
		m_Tree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(1), TVIS_STATEIMAGEMASK);
	else
		m_Tree.SetItemState(hItem, INDEXTOSTATEIMAGEMASK(2), TVIS_STATEIMAGEMASK);

	if (bSelect) 
		m_Tree.Select(hItem, TVGN_CARET); 
}

//-----------------------------------------------------------------------------
int CAdminToolDocProfileDlg::GetImage(CPathFinder::PosType aType)
{
	int nImage = IMAGE_NORMAL;
	switch  (aType)
	{
		case CPathFinder::USERS:
			nImage = 2;
			break;

		case CPathFinder::ALL_USERS:
			nImage = 1;
			break;

		case CPathFinder::STANDARD:
			nImage = 0;
			break;
	}
	return nImage;
}
//-----------------------------------------------------------------------------
void CAdminToolDocProfileDlg::SetUsrModified(BOOL bMod)
{
	m_BtnSave.EnableWindow(bMod);
	m_bModifiedUsr = bMod;

	if (!bMod)
		m_bUsrModConditioned = FALSE;
}

//-----------------------------------------------------------------------------
void CAdminToolDocProfileDlg::SetAllUsrModified(BOOL bMod)
{
	m_BtnSave.EnableWindow(bMod);
	m_bModifiedAllUsr = bMod;
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnNew()
{
	CString					strNewProfName = _TB("New");
	CFunctionDescription	aFuncDescri;

	CPathFinder::PosType aPos;
	if (m_bBtnStdPressed)
		aPos = CPathFinder::STANDARD;
	else if (m_bBtnAllUsrPressed)
		aPos = CPathFinder::ALL_USERS;
	else
		aPos = CPathFinder::USERS;

	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.NewExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(DataStr(m_NameSpace.ToString()));
	aFuncDescri.GetParamValue(szNewProfileName)->Assign(strNewProfName);
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)aPos);
	aFuncDescri.GetParamValue(szUserName)->Assign(GetSelectedUsr());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(DataStr(_T("")));
	
	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		RefreshAfterModifies();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnModify()
{
	CAdminProfileItem* pAdminProfileItem = GetTreeHItemValue();
	if (!pAdminProfileItem)
		return;

	BtnModify(pAdminProfileItem);
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::BtnModify(CAdminProfileItem* pAdminProfileItem)
{
	if (pAdminProfileItem->GetProfileName().IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}
	CFunctionDescription aFuncDescri;
	BOOL bOnlyShow = pAdminProfileItem->GetPosType() == CPathFinder::STANDARD && !AfxGetBaseApp()->IsDevelopment();
	if (bOnlyShow)
		AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.ShowExportProfile"), aFuncDescri); 
	else
	{
		AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.ModifyExportProfile"), aFuncDescri); 
		((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)(pAdminProfileItem->GetPosType()));
		aFuncDescri.GetParamValue(szUserName)->Assign(pAdminProfileItem->GetUserName());
	}

 	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_NameSpace.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(pAdminProfileItem->GetProfilePath());

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0) && !bOnlyShow)
		RefreshAfterModifies();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnRename()
{
	RemaneLabel();
}

//-----------------------------------------------------------------------------
void CAdminToolDocProfileDlg::RenameTitle(const CString& strSelItem)
{	
	CAdminProfileItem* pAdminProfileItem = GetTreeHItemValue();
	
	if (!pAdminProfileItem && pAdminProfileItem->GetProfileName().CollateNoCase(szPredefined) == 0)
	{
		AfxMessageBox(_TB("Attention please! It is not possible to delete or rename the Predefined profile."));
		return;
	}	

	CString strProfilePath = pAdminProfileItem->GetProfilePath(); 
	CString strNewProfName = strSelItem; 

	CFunctionDescription aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.RenameExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_NameSpace.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(strProfilePath);
	aFuncDescri.GetParamValue(szNewName)->Assign(strNewProfName);

	if (pAdminProfileItem->IsPreferential())
	{
		CXMLDefaultInfo aDefaultInfo(m_NameSpace);
		aDefaultInfo.SetPreferredProfile(strNewProfName);
		aDefaultInfo.UnParse(pAdminProfileItem->GetPosType(), pAdminProfileItem->GetUserName());
	}

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		RefreshAfterModifies();
	else
		AfxMessageBox(_TB("Warning, unable to delete read-only profile."));
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnDelete()
{	
	DeleteElement();
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::DeleteElement()
{
	CAdminProfileItem* pAdminProfileItem = 	GetTreeHItemValue();
	
	if (!pAdminProfileItem && pAdminProfileItem->GetProfileName().CollateNoCase(szPredefined) == 0)
	{
		AfxMessageBox(_TB("Attention please! It is not possible to delete or rename the Predefined profile."));
		return ;
	}	

	if (pAdminProfileItem->GetProfilePath().IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	CString					strProfilePath = pAdminProfileItem->GetProfilePath(); 
	CFunctionDescription	aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.DeleteExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_NameSpace.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(DataStr(strProfilePath));

	if (pAdminProfileItem->IsPreferential())
	{
		CXMLDefaultInfo aDefaultInfo(m_NameSpace);
		aDefaultInfo.SetPreferredProfile(_T(""));
		aDefaultInfo.UnParse(pAdminProfileItem->GetPosType(), pAdminProfileItem->GetUserName());
	}

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0)) 
		RefreshAfterModifies();
	else
		AfxMessageBox(_TB("Warning, unable to delete read-only profile."));
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnClone()
{	
	CAdminProfileItem* pAdminProfileItem = GetTreeHItemValue();
	if (!pAdminProfileItem)
		return;
	
	CFunctionDescription aFuncDescri;
	
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.CloneExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(DataStr(m_NameSpace.ToString()));
	aFuncDescri.GetParamValue(szProfilePath)->Assign(pAdminProfileItem->GetProfilePath());
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)pAdminProfileItem->GetPosType());
	aFuncDescri.GetParamValue(szUserName)->Assign(pAdminProfileItem->GetUserName());
	
	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		RefreshAfterModifies();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnCopyIn()
{
	CPathFinder::PosType aPos;
	if (m_bBtnUsrPressed)
		aPos = CPathFinder::USERS;
	else if (m_bBtnAllUsrPressed)
		aPos = CPathFinder::ALL_USERS;
	else
		aPos = CPathFinder::STANDARD;

	CCopyMoveProfileDialog copyMoveDlg(TRUE, aPos, m_NameSpace, m_arStdProfile, m_arAllUsersProfile, m_arUserProfile);
	if (copyMoveDlg.DoModal() != IDOK)
		return;

	CAdminProfileItem*	pAdminProfileItem			= (CAdminProfileItem*) m_Tree.GetItemData(m_Tree.GetSelectedItem());

	aPos = copyMoveDlg.m_PosTypeSelected;
	
	CFunctionDescription aFuncDescri;	
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.CopyExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(DataStr(m_NameSpace.ToString()));
	aFuncDescri.GetParamValue(szProfilePath)->Assign(pAdminProfileItem->GetProfilePath());
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)aPos);
	CDataObjDescription* pDescri = aFuncDescri.GetParamDescription(szUserArray);
	if (pDescri)
	{
		DataArray* pUsers = (DataArray*) pDescri->GetValue();
		if (!pUsers)		
		{
			pDescri->SetValue(DataArray());
			pUsers = (DataArray*) pDescri->GetValue();
		}
	
		for (int nIdx = 0; nIdx < copyMoveDlg.m_arUsrsSel.GetSize(); nIdx++)
			pUsers->Add(new DataStr(copyMoveDlg.m_arUsrsSel.GetAt(nIdx)));
	}

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		RefreshAfterModifies();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnBtnMoveIn()
{
	CPathFinder::PosType aPos = CPathFinder::USERS;
	if (m_bBtnAllUsrPressed)
		aPos = CPathFinder::ALL_USERS;
	if (m_bBtnStdPressed)
		aPos = CPathFinder::STANDARD;

	CCopyMoveProfileDialog copyMoveDlg(FALSE, aPos, m_NameSpace, m_arStdProfile, m_arAllUsersProfile, m_arUserProfile);
	if (copyMoveDlg.DoModal() != IDOK)
		return;

	CAdminProfileItem*	pAdminProfileItem			= (CAdminProfileItem*) m_Tree.GetItemData(m_Tree.GetSelectedItem());
	
	aPos = copyMoveDlg.m_PosTypeSelected;
	
	CFunctionDescription aFuncDescri;	
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.MoveExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(DataStr(m_NameSpace.ToString()));
	aFuncDescri.GetParamValue(szProfilePath)->Assign(pAdminProfileItem->GetProfilePath());
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)aPos);
	CDataObjDescription* pDescri = aFuncDescri.GetParamDescription(szUserArray);
	if (pDescri)
	{
		DataArray* pUsers = (DataArray*) pDescri->GetValue();
		if (!pUsers)		
		{
			pDescri->SetValue(DataArray());
			pUsers = (DataArray*) pDescri->GetValue();
		}
		for (int nIdx = 0; nIdx < copyMoveDlg.m_arUsrsSel.GetSize(); nIdx++)
			pUsers->Add(new DataStr(copyMoveDlg.m_arUsrsSel.GetAt(nIdx)));
	}

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		RefreshAfterModifies();
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::SetAllButtonsState(BOOL bEnabled /*TRUE*/)
{
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_DELETE))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_RENAME))	->EnableWindow(bEnabled);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_NEW))	->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MODIFY))	->EnableWindow(bEnabled);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_CLONE))  ->EnableWindow(bEnabled); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MOVEIN))	->EnableWindow(bEnabled);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_COPYIN))	->EnableWindow(bEnabled);
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::SetButtonsState()
{
	int num = m_ListDoc.GetCount();

	if  (num == 0 || (m_bBtnUsrPressed && m_pToolBar->GetComboItemSel(IDC_FORMMNG_ADM_COMBO_USR) == 0))
	{
		SetAllButtonsState(FALSE);
		return;
	}

	BOOL bEmpty = (m_Tree.GetCount() == 0);
	
	BOOL bEnable = ((m_bBtnStdPressed && AfxGetBaseApp()->IsDevelopment()) || !m_bBtnStdPressed);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_ADD))    ->EnableWindow(bEnable); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_NEW))    ->EnableWindow(bEnable); 
	
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_CLONE))  ->EnableWindow(bEnable && !bEmpty); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_DELETE))	->EnableWindow(bEnable && !bEmpty); 
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_RENAME))	->EnableWindow(bEnable && !bEmpty);
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MOVEIN))	->EnableWindow(bEnable && !bEmpty);
	
	((CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_COPYIN))	->EnableWindow(!bEmpty);
	CButton* pBtn =	(CButton*) GetDlgItem(IDC_FORMMNG_ADM_BTN_MODIFY);
	pBtn->EnableWindow(!bEmpty);
    if (m_bBtnStdPressed && !AfxGetBaseApp()->IsDevelopment())
		pBtn->SetWindowText(_TB("Show"));
	else
		pBtn->SetWindowText(_TB("Edit"));	

}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnNMDblclkTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	CAdminProfileItem* pAdminProfileItem = GetTreeHItemValue();
	if (!pAdminProfileItem || (m_bBtnStdPressed && !AfxGetBaseApp()->IsDevelopment()))
		return;
    BtnModify(pAdminProfileItem);
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (hSel == NULL)
		return;

	m_Tree.SetItemState(hSel, TVIS_SELECTED, TVIS_SELECTED);
	SetButtonsState();
	m_Tree.SetFocus();
}

//--------------------------------------------------------------------------
CAdminProfileItem* CAdminToolDocProfileDlg::GetTreeHItemValue()
{
	HTREEITEM hSel = m_Tree.GetSelectedItem();
	if (hSel == NULL)
		return NULL;
	
	CAdminProfileItem* pAdminProfileItem = (CAdminProfileItem*) m_Tree.GetItemData(hSel);
	if (!pAdminProfileItem)
		return NULL;

	return pAdminProfileItem;
}

//--------------------------------------------------------------------------
void CAdminToolDocProfileDlg::RefreshAfterModifies()
{
	if (m_bBtnUsrPressed)
	{
		FindUserProfiles();
		FillTreeUserIfNecessary();
	}
	else if (m_bBtnAllUsrPressed)
	{
		FindAllUsersProfiles();
		FillTreeAllUsrs();
	}
	else
	{
		FindStdProfiles();
		FillTreeStd();
	}
}

//-----------------------------------------------------------------------------
BOOL CAdminToolDocProfileDlg::CanDelete()
{
	if (m_bBtnStdPressed && !AfxGetBaseApp()->IsDevelopment())
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void CAdminToolDocProfileDlg::SetAsDefault()
{
	if (m_bBtnStdPressed && !AfxGetBaseApp()->IsDevelopment())
		return;

	CAdminProfileItem* pAdminProfileItem = GetTreeHItemValue();
	CXMLDefaultInfo		aDefaultInfo(m_NameSpace);
	aDefaultInfo.SetPreferredProfile(pAdminProfileItem->GetProfileName());
	aDefaultInfo.UnParse(pAdminProfileItem->GetPosType(), pAdminProfileItem->GetUserName());
	RefreshAfterModifies();	
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnOK()
{
	OnCancel();
}

//-----------------------------------------------------------------------
void CAdminToolDocProfileDlg::OnCancel()
{
	if (m_Tree.m_bRename)
		return;

	if (m_Tree.GetCount() > 0)
	{
		HTREEITEM			hItem	= NULL;
		CAdminProfileItem*	pAdProf = NULL;
		hItem = m_Tree.GetFirstVisibleItem();
		while (hItem != NULL)
		{
			pAdProf = (CAdminProfileItem*) m_Tree.GetItemData(hItem);
			SAFE_DELETE(pAdProf);
			m_Tree.DeleteItem(hItem);
			hItem = m_Tree.GetFirstVisibleItem();
		}
	}
	CParsedDialog::EndDialog(IDCANCEL);
}

//==========================================================================
//							CCopyMoveProfileDialog
//==========================================================================
IMPLEMENT_DYNAMIC(CCopyMoveProfileDialog, CParsedDialog)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCopyMoveProfileDialog, CParsedDialog)
	ON_COMMAND			(IDC_RADIO_COPYMOVEDLG_STD,		OnCheckRadioButton)
	ON_COMMAND			(IDC_RADIO_COPYMOVEDLG_ALLUSRS,	OnCheckRadioButton)
	ON_COMMAND			(IDC_RADIO_COPYMOVEDLG_USR,		OnCheckRadioButton)
	ON_NOTIFY			(LVN_ITEMCHANGED,	IDC_COPYMOVEPROFILEDLG_LIST_USR,	OnUsrChanged)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
CCopyMoveProfileDialog::CCopyMoveProfileDialog(BOOL bCopy, CPathFinder::PosType aPosTypeExcluded, CTBNamespace Ns, const CStringArray& arStd, const CStringArray& arAllUsrs, const CStringArray& arUsr)
	: 
	CParsedDialog			(IDD_COPY_MOVE_PROFILE),
	m_bCopy					(bCopy)
{	
	m_Namespace			= Ns;
	m_PosTypeSelected	= aPosTypeExcluded;
	m_PosTypeExcluded	= aPosTypeExcluded;
	m_arStdProfile		.Copy(arStd);
	m_arAllUsersProfile	.Copy(arAllUsrs);
	m_arUserProfile		.Copy(arUsr);
}

//--------------------------------------------------------------------------
CCopyMoveProfileDialog::~CCopyMoveProfileDialog()
{

}

//--------------------------------------------------------------------------
BOOL CCopyMoveProfileDialog::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();

	VERIFY(m_ListProfile	.SubclassDlgItem (IDC_COPYMOVEPROFILEDLG_LIST_PROFILE,	this));
	VERIFY(m_ListUsers		.SubclassDlgItem (IDC_COPYMOVEPROFILEDLG_LIST_USR,		this));

	PrepareControls ();
	
	FillProfileList();

	return bInit;
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::PrepareControls()
{
	if (m_bCopy)
		SetWindowText(_TB("Copy profile into ..."));
	else
		SetWindowText(_TB("Move profile into ..."));

	switch (m_PosTypeExcluded)
	{
		case CPathFinder::STANDARD:
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_STD))->EnableWindow(FALSE);
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_USR))->SetCheck(TRUE);
			m_PosTypeSelected = CPathFinder::USERS;
			break;
		case CPathFinder::ALL_USERS:
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_ALLUSRS))->EnableWindow(FALSE);
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_USR))->SetCheck(TRUE);
			m_PosTypeSelected = CPathFinder::USERS;
			break;
		case CPathFinder::USERS:
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_USR))->EnableWindow(TRUE);
			((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_ALLUSRS))->SetCheck(TRUE);
			m_PosTypeSelected = CPathFinder::ALL_USERS;
			m_ListUsers.EnableWindow(FALSE);
			break;
	}

	if (!AfxGetBaseApp()->IsDevelopment())
		((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_STD))->EnableWindow(FALSE);

	
	FillUsr();
	if (m_PosTypeExcluded == CPathFinder::USERS)
	{
		((CButton*) GetDlgItem(IDC_STATIC_USR_PROFILE))->SetWindowText(_TB("All users"));
		m_ListProfile.ResetContent();
		m_ListUsers.SetRedraw(TRUE);
		m_ListUsers.Invalidate(FALSE);
	}
	
	((CButton*)GetDlgItem(IDOK))->EnableWindow(TRUE);
}

//--------------------------------------------------------------------------
int CompareUsr(CObject* pObj1, CObject* pObj2)
{
	CTBObjDetails* pd1 = (CTBObjDetails*) pObj1;
	CTBObjDetails* pd2 = (CTBObjDetails*) pObj2;

	return pd1->m_sName.CompareNoCase(pd2->m_sName);
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::FillUsr()
{
	CString			strUser;
	CStringArray	aUsers;
	int				nUserCount = AfxGetLoginInfos()->m_CompanyUsers.GetSize();
	m_ListUsers.InsertColumn(0, _TB("User name"), LVCFMT_LEFT, 200);
	m_ListUsers.DeleteAllItems();

	for (int i = 0 ; i < (nUserCount) ; i++)
	{
		strUser = AfxGetLoginInfos()->m_CompanyUsers.GetAt(i);
		m_ListUsers.InsertItem(i, strUser);
		m_ListUsers.SetItemText(i, 0, strUser);
	}

	m_ListUsers.SetRedraw(TRUE);
	m_ListUsers.Invalidate(FALSE);

	if (m_strUsr.IsEmpty())
		m_strUsr = AfxGetLoginInfos()->m_strUserName;

	int nCount = m_ListUsers.GetItemCount();
	int nToSel = 0;
	for (int n = 0; n <= nCount; n++)
	{
		CString strItem = m_ListUsers.GetItemText(n, 0);
		if (strItem.CompareNoCase(m_strUsr) == 0)
		{
			nToSel = n;
			break;
		}
	}

	m_ListUsers.SetItemState(nToSel, LVIS_SELECTED, LVIS_SELECTED);
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::FillProfileList()
{
	m_ListProfile.ResetContent();
	CStringArray arProf;
	switch (m_PosTypeSelected)
	{
		case CPathFinder::STANDARD:
			arProf.Copy(m_arStdProfile);
			break;
		case CPathFinder::ALL_USERS:
			arProf.Copy(m_arAllUsersProfile);
			break;
		case CPathFinder::USERS:
			m_arUserProfile.RemoveAll();
			if (!m_strUsr.IsEmpty())
				AfxGetPathFinder()->GetProfilesFromPath(AfxGetPathFinder()->GetDocumentExportProfilesPath(m_Namespace, CPathFinder::USERS, m_strUsr), (CStringArray&) m_arUserProfile, FALSE);
			arProf.Copy(m_arUserProfile);
			break;
	}

	CString strProf;
	for (int i = 0; i <= arProf.GetUpperBound(); i++)
	{
		strProf = arProf.GetAt(i);
		if (!strProf.IsEmpty()) 
			m_ListProfile.AddString(GetName(strProf));
	}	

	m_ListProfile.SetCurSel(0);	
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::OnUsrChanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	BeginWaitCursor();
	int nSel = m_ListUsers.GetSelectedCount();
	if (nSel == 0)
		((CButton*) GetDlgItem(IDOK))->EnableWindow(FALSE);
	else
		((CButton*) GetDlgItem(IDOK))->EnableWindow(TRUE);
	
	if (nSel > 1 || nSel == 0 )
	{
		m_strUsr = _T("");
		m_ListProfile.ResetContent();
		((CButton*) GetDlgItem(IDC_STATIC_USR_PROFILE))->SetWindowText(m_strUsr);
		
		FillProfileList();
		EndWaitCursor();
		return;
	}

	POSITION pos = m_ListUsers.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		int nItem = m_ListUsers.GetNextSelectedItem(pos);
		m_strUsr = m_ListUsers.GetItemText(nItem, 0);
	}

	((CButton*) GetDlgItem(IDC_STATIC_USR_PROFILE))->SetWindowText(m_strUsr);
	
	FillProfileList();	
	EndWaitCursor();
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::OnCheckRadioButton()
{
	BOOL bUsrSel = ((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_USR))->GetCheck();
	if (bUsrSel)
	{
		m_ListUsers.EnableWindow(TRUE);
		m_PosTypeSelected = CPathFinder::USERS;
		((CButton*) GetDlgItem(IDC_STATIC_USR_PROFILE))->SetWindowText(m_strUsr);
	}
	else
	{
		m_ListUsers.EnableWindow(FALSE);
		CString strStatic = _TB("Standard");
		m_PosTypeSelected = CPathFinder::STANDARD;
		if (((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_ALLUSRS))->GetCheck())
		{
			strStatic = _TB("All users");
			m_PosTypeSelected = CPathFinder::ALL_USERS;
		}

		((CButton*) GetDlgItem(IDC_STATIC_USR_PROFILE))->SetWindowText(strStatic);		
	}

	if (!bUsrSel || m_ListUsers.GetSelectedCount() > 0)
		((CButton*)GetDlgItem(IDOK))->EnableWindow(TRUE);
	else
		((CButton*)GetDlgItem(IDOK))->EnableWindow(FALSE);


	FillProfileList();
}

//--------------------------------------------------------------------------
void CCopyMoveProfileDialog::OnOK()
{
	m_arUsrsSel.RemoveAll();
	if (((CButton*) GetDlgItem(IDC_RADIO_COPYMOVEDLG_USR))->GetCheck())
	{
		int nPos;
		POSITION posit = m_ListUsers.GetFirstSelectedItemPosition();
		while (posit)
        {
			nPos	= m_ListUsers.GetNextSelectedItem(posit);
			CString strUsr = m_ListUsers.GetItemText(nPos, 0);
			m_arUsrsSel.Add(strUsr);
		}
	}

	CParsedDialog::OnOK();	
}

//==========================================================================
//							DocumentExplorerListCtrl
//==========================================================================

BEGIN_MESSAGE_MAP(DocumentExplorerListCtrl, CBCGPListCtrl)
	
END_MESSAGE_MAP()

IMPLEMENT_DYNCREATE(DocumentExplorerListCtrl, CBCGPListCtrl)

//--------------------------------------------------------------------------
DocumentExplorerListCtrl::DocumentExplorerListCtrl()
{
}

//--------------------------------------------------------------------------
DocumentExplorerListCtrl::~DocumentExplorerListCtrl()
{
}

//--------------------------------------------------------------------------
BOOL DocumentExplorerListCtrl::IsInternalScrollBarThemed() const
{
	return TRUE;
}

//==========================================================================
//							CDocumentExplorerDlg
//==========================================================================
BEGIN_MESSAGE_MAP(CDocumentExplorerDlg, CBaseDocumentExplorerDlg)
	
	ON_BN_CLICKED			(IDC_DOCUMENT_NAMESPACE_SELECT,					 OnSelectNamespace		 )
	ON_BN_CLICKED			(IDC_DOCUMENT_CANCEL,							 OnClose				 )
	ON_CBN_SELCHANGE		(IDC_DOCUMENT_MODULE_COMBO,						 OnModuleComboSelectItem )
	ON_LBN_SELCHANGE		(IDC_DOCUMENT_LIST_DOC,							 OnDocumentListSelectItem)
	ON_NOTIFY				(LVN_ITEMCHANGED, IDC_DOCUMENT_LISTAPPS,		 OnAppsItemChanged		 )
	ON_LBN_DBLCLK			(IDC_DOCUMENT_LIST_DOC,							 OnDocumentDoubleClick	 )

END_MESSAGE_MAP()

//--------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDocumentExplorerDlg, CBaseDocumentExplorerDlg)
//--------------------------------------------------------------------------

CDocumentExplorerDlg::CDocumentExplorerDlg() :
	CBaseDocumentExplorerDlg		(IDD_DOCUMENT_NAMESPACE),
	m_pWndParent		(NULL)
{
}

//--------------------------------------------------------------------------
CDocumentExplorerDlg::~CDocumentExplorerDlg()
{
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::SelectNameSpaceAndClose()
{
	if (m_ListDocument.GetCurSel() <0 || m_ListDocument.GetCaretIndex() != m_ListDocument.GetCurSel())
		return;

	CItemNoLocInDocRep* pItemNoLocInDocRep	= (CItemNoLocInDocRep*) m_ListDocument.GetItemData(m_ListDocument.GetCurSel());
	if (pItemNoLocInDocRep == NULL)
	{
		m_FullNameSpace = _T("");
		CParsedDialog::EndDialog(IDC_DOCUMENT_CANCEL);
		return;
	}
	
	m_FullNameSpace = pItemNoLocInDocRep->m_DocNamespace.ToString();
	SetDlgItemText(IDC_DOCUMENT_NAMESPACE_SHOW , m_FullNameSpace);
	CParsedDialog::EndDialog(IDOK);
}
//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnDocumentDoubleClick()
{
	SelectNameSpaceAndClose();
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnSelectNamespace()
{
	SelectNameSpaceAndClose();
}

//--------------------------------------------------------------------------
BOOL CDocumentExplorerDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
	
	VERIFY(m_ListCtrlApps			.SubclassDlgItem	(IDC_DOCUMENT_LISTAPPS,			this));
	VERIFY(m_ComboModule			.SubclassDlgItem	(IDC_DOCUMENT_MODULE_COMBO,		this));
	VERIFY(m_ListDocument			.SubclassDlgItem	(IDC_DOCUMENT_LIST_DOC,			this));
	VERIFY(m_ButtonSelect			.SubclassDlgItem	(IDC_DOCUMENT_NAMESPACE_SELECT,	this));
	VERIFY(m_ButtonClose			.SubclassDlgItem	(IDC_DOCUMENT_CANCEL,			this));
	VERIFY(m_ShowFullNameSpace		.SubclassDlgItem	(IDC_DOCUMENT_NAMESPACE_SHOW,	this));
	
	((CButton*) GetDlgItem(IDC_DOCUMENT_NAMESPACE_SELECT))	->EnableWindow(FALSE); 

	m_AppsImageList.Create(32, 32, ILC_COLOR32, 32, 32);
	m_AppsImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
	m_ListCtrlApps.SetImageList(&m_AppsImageList, LVSIL_NORMAL);

	SetStartNamespace();
	FillApplicationsListControl();
	FillModuleComboBox();
	
	return bInit;
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnClose()
{
	m_FullNameSpace = _T("");
	CParsedDialog::EndDialog(IDC_DOCUMENT_CANCEL);
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnModuleComboSelectItem()
{
	int					nCurSel				= m_ComboModule.GetCurSel();
	CItemNoLocInDocRep* pItemNoLocInDocRep	= (CItemNoLocInDocRep*) m_ComboModule.GetItemData(nCurSel);
	CString				strModulename		= pItemNoLocInDocRep->m_strName;
	m_FullNameSpace = _T("");
	SetDlgItemText(IDC_DOCUMENT_NAMESPACE_SHOW , m_FullNameSpace);
	((CButton*) GetDlgItem(IDC_DOCUMENT_NAMESPACE_SELECT))	->EnableWindow(FALSE); 
	m_NameSpace.SetObjectName(CTBNamespace::MODULE, strModulename);
	FillDocumentListBox();
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnDocumentListSelectItem()
{
	if (m_ListDocument.GetCount() <= 0)
		return;

	int currentSelected = m_ListDocument.GetCurSel();
	//Se nessun elemento selezionato oppure se si seleziona parti di listbox vuote
	if (currentSelected <0 || m_ListDocument.GetCaretIndex() != currentSelected)
	{
		m_FullNameSpace = _T("");
		SetDlgItemText(IDC_DOCUMENT_NAMESPACE_SHOW , m_FullNameSpace);
		m_ListDocument.SetCurSel(-1);
		((CButton*) GetDlgItem(IDC_DOCUMENT_NAMESPACE_SELECT))	->EnableWindow(FALSE); 
		return;
	}

	//se pItemNoLocInDocRep vuoto
	CItemNoLocInDocRep* pItemNoLocInDocRep	= (CItemNoLocInDocRep*) m_ListDocument.GetItemData(currentSelected);
	if (pItemNoLocInDocRep == NULL)
		m_FullNameSpace = _T("");
	else
		m_FullNameSpace = pItemNoLocInDocRep->m_DocNamespace.ToString();

	//attiva select e scrive namespace nella textbox
	SetDlgItemText(IDC_DOCUMENT_NAMESPACE_SHOW , m_FullNameSpace);
	((CButton*) GetDlgItem(IDC_DOCUMENT_NAMESPACE_SELECT))	->EnableWindow(TRUE); 
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::FillApplicationsListControl()
{
	int					nApp			= 0;
	CString				strApps			= _T("");
	CString				strDefaultApp	= _T("");
	CItemNoLocInDocRep*	pItemNoLoc		= NULL;
	
	m_ListCtrlApps.DeleteAllItems();
	
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		CString strCont = AfxGetPathFinder()->GetAppContainerName(strApps);
		BOOL bSysApp = AfxGetPathFinder()->IsASystemApplication(strApps);

		CTBNamespace appNameSpace = m_NameSpace;
		appNameSpace.SetApplicationName(strApps);
		CString strDbtsFullName = AfxGetPathFinder()->GetDocumentDbtsFullName(appNameSpace );

		if (bSysApp && strDbtsFullName.IsEmpty())
			continue;
		
		if 	(
				(!AfxGetBaseApp()->IsDevelopment() &&
				 strApps.CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0)
			)
			continue;

		if (!AfxGetAddOnApp(strApps))
			continue;

		CString strTitle = AfxGetAddOnApp(strApps)->GetTitle();
		pItemNoLoc = CItemNoLocInDocRep::Create(m_arAppItemLoc, strApps);
		
		AddOnApplication* pAddOnApplicationExt = AfxGetBaseApp()->GetMasterAddOnApp();
		if (strDefaultApp.IsEmpty() && strApps.CompareNoCase( pAddOnApplicationExt->m_strAddOnAppName) == 0)
			strDefaultApp = strApps;

		CString strIcon = TBGlyph(szIconTBFramework); // _T("Framework.TbLoader.Images.Framework.bmp");
		CTBNamespace ns(strIcon);

		CString strIconFile = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		if (strIconFile.IsEmpty() && strIcon != TBGlyph(szIconTBFramework) /*_T("Framework.TbLoader.Images.Framework.bmp")*/)
		{
			ns.Clear();
			ns.AutoCompleteNamespace(CTBNamespace::IMAGE, TBGlyph(szIconTBFramework) /*_T("Framework.TbLoader.Images.Framework.bmp")*/, CTBNamespace());
			strIconFile = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		}
		ASSERT(!strIconFile.IsEmpty());
		// if empty return ... vedere 
		HICON hb = TBLoadImage(strIcon);
		ASSERT(hb);

		m_AppsImageList.Add(hb);
		
		m_ListCtrlApps.InsertItem(nApp, strTitle, nApp);
		m_ListCtrlApps.SetItemData(nApp, (DWORD) pItemNoLoc);
		nApp++;

		DeleteObject(hb);
	}

	if (m_NameSpace.GetApplicationName().CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0)
	{	
		CStringArray	aModules;
		m_NameSpace.SetApplicationName(strDefaultApp);

		AddOnApplication* pAddOnApplication =  AfxGetAddOnApp(m_NameSpace.GetApplicationName());

		if (pAddOnApplication->m_pAddOnModules)
		{
			for (int z=0; z <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); z++)
			{
				AddOnModule* pMod  = pAddOnApplication->m_pAddOnModules->GetAt(z);
				if (!AfxIsActivated(pMod->GetApplicationName(), pMod->GetModuleName()))
					continue;
				m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMod->GetModuleName());
				break;
			}
		}
	}

	AddOnApplication* pSelApp = NULL;
	if (!m_NameSpace.IsEmpty())
		pSelApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());

	if (pSelApp)
		for (int nIdx = 0; nIdx <= m_ListCtrlApps.GetItemCount(); nIdx++)
			if (m_ListCtrlApps.GetItemText(nIdx, 0).CompareNoCase(pSelApp->GetTitle()) == 0)
			{
				m_ListCtrlApps.SetItemState(nIdx, LVIS_SELECTED, LVIS_SELECTED | LVIS_FOCUSED);
				break;
			}
}
//--------------------------------------------------------------------------
void CDocumentExplorerDlg::FillDocumentListBox()
{
	m_ListDocument.ResetContent();

	AddOnModule* pAddOnMod = AfxGetAddOnModule(m_NameSpace);
	if (!pAddOnMod)
		return;

	CBaseDescriptionArray*	pDocArray = AfxGetDocumentDescriptionsOf(pAddOnMod->m_Namespace);
	
	FillDoc(pDocArray);
	
	if (pAddOnMod != NULL)
	{	
		if (m_ListDocument.GetCount() > 0)
		{
			//m_ListDocument.SetCurSel(0);
			CItemNoLocInDocRep* pDocument = (CItemNoLocInDocRep*) m_ListDocument.GetItemData(0);
			m_NameSpace.SetNamespace(pDocument->m_DocNamespace/*->m_strName*/);
		}
	}
	delete pDocArray;
}
//--------------------------------------------------------------------------
void CDocumentExplorerDlg::FillModuleComboBox(const CString& strLabel/*= _TB("")*/, bool bFirst /*= FALSE */)
{
	CItemNoLocInDocRep*	pItemNoLocInDocRep	= NULL;
	AddOnApplication*	pAddOnApplication	= NULL;
	AddOnModule*		pAddOnModule		= NULL;
	CString				strMods				= _T("");
	CStringArray		aModules;
	CTBNamespace		Ns;	
	int					nIdx;

	m_ComboModule.ResetContent();
	if (strLabel.IsEmpty())
	{		
		pAddOnApplication = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
		for (int i = 0; i <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); i++)
		{
			pAddOnModule = pAddOnApplication->m_pAddOnModules->GetAt(i);

			//seleziona solo i moduli licenziati
			if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
				continue;

			nIdx = m_ComboModule.AddString(pAddOnModule->GetModuleTitle());
			pItemNoLocInDocRep = CItemNoLocInDocRep::Create(m_arAppItemLoc, pAddOnModule->GetModuleName());
			m_ComboModule.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);
		}

		pAddOnModule = AfxGetAddOnModule(m_NameSpace);
		if (pAddOnModule != NULL)
		{
			int n = m_ComboModule.FindStringExact(-1, (LPCTSTR)pAddOnModule->GetModuleTitle());	
			m_ComboModule.SetCurSel(n);
		}
	}
	else
	{
		pAddOnApplication =  AfxGetAddOnApp(strLabel);
		for (int a = 0; a <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); a++)
		{
			pAddOnModule = pAddOnApplication->m_pAddOnModules->GetAt(a);

			//seleziona solo i moduli licenziati
			if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
				continue;

			nIdx = m_ComboModule.AddString(pAddOnModule->GetModuleTitle());
			pItemNoLocInDocRep = CItemNoLocInDocRep::Create(m_arAppItemLoc, pAddOnModule->GetModuleName());
			m_ComboModule.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);	
		}	

		m_ComboModule.SetCurSel(0);
		CItemNoLocInDocRep* pLocItem = (CItemNoLocInDocRep*) m_ComboModule.GetItemData(0);
		if (m_ComboModule.GetCount() >= 1)
			m_NameSpace.SetObjectName(CTBNamespace::MODULE, pLocItem->m_strName);
	}

	
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::OnAppsItemChanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

	POSITION		Pos				= m_ListCtrlApps.GetFirstSelectedItemPosition();
		
	int				nItem			= 0;
	CString			strLabel		= _T("");
	CString			strApps			= _T("");
	CItemNoLoc*		pItemNoLoc		= NULL;

	while (Pos)
	{
		m_ComboModule.ResetContent();
		m_ListDocument.ResetContent();
		m_FullNameSpace = _T("");
		SetDlgItemText(IDC_DOCUMENT_NAMESPACE_SHOW , m_FullNameSpace);
		((CButton*) GetDlgItem(IDC_DOCUMENT_NAMESPACE_SELECT))	->EnableWindow(FALSE); 
		

		nItem		= m_ListCtrlApps.GetNextSelectedItem(Pos);
		pItemNoLoc	= (CItemNoLoc*) m_ListCtrlApps.GetItemData(nItem);
		strLabel	= pItemNoLoc->m_strName;
		m_ComboModule.ResetContent();
	
		BOOL bFind = FALSE;
		for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
		{
			strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
			if (strApps.CompareNoCase(strLabel) == 0)
				bFind = TRUE; 
		}

		if (!bFind)
			strLabel = _T(""); 

		m_NameSpace.SetApplicationName(strLabel);
		m_NameSpace.SetObjectName(_T(""));
		FillModuleComboBox(strLabel);
		FillDocumentListBox();
	}
	*pResult = 0;
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::SetStartNamespace()
{
	CString			strApp;
	AddOnModsArray* pMods = NULL;

	m_NameSpace.SetType(CTBNamespace::DOCUMENT);
	AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
	if (pApp)
		m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
	
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
	if (pAddOnApp)
		pMods = pAddOnApp->m_pAddOnModules;
	if (pMods)
	{
		for (int i=0; i <= pMods->GetUpperBound(); i++)
		{
			AddOnModule* pMod  = pMods->GetAt(i);
			if (!AfxIsActivated(pMod->GetApplicationName(), pMod->GetModuleName()))
				continue;
			m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMod->GetModuleName());
			break;
		}
	}

	m_NameSpace.SetType(CTBNamespace::DOCUMENT);
}

//--------------------------------------------------------------------------
void CDocumentExplorerDlg::FillDoc(const CBaseDescriptionArray* pDocArray)
{	
	if (pDocArray == NULL)
	{
		ASSERT (FALSE);
		return;
	}

	int						nIdx				= 0;
	CString 				strXMLDocDescr;
	CItemNoLocInDocRep*		pItemNoLocInDocRep	= NULL;
	CDocumentDescription*	pDoc				= NULL;
	for (int i = 0; i <= pDocArray->GetUpperBound(); i++)
	{
		pDoc = (CDocumentDescription*) pDocArray->GetAt(i);
		ASSERT_VALID(pDoc);

		if (!pDoc->IsPublished() || !pDoc->IsActivated())
			continue;

		if (m_bFilterXmlDescrition)
		{
			strXMLDocDescr	= AfxGetPathFinder()->GetDocumentDocumentFullName(pDoc->GetNamespace(), CPathFinder::CUSTOM);

			if (strXMLDocDescr.IsEmpty() || !ExistFile(strXMLDocDescr))
				strXMLDocDescr	= AfxGetPathFinder()->GetDocumentDocumentFullName(pDoc->GetNamespace(), CPathFinder::STANDARD);
			
			if (!strXMLDocDescr.IsEmpty() && ExistFile(strXMLDocDescr))
			{
				pItemNoLocInDocRep	= CItemNoLocInDocRep::Create(m_arAppItemLoc, pDoc->GetName(), pDoc->GetNamespace());
				nIdx				= m_ListDocument.AddString(pDoc->GetTitle());
				m_ListDocument.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);
			}
			continue;
		}

		//eventuali nuovi filtraggi
		
		pItemNoLocInDocRep	= CItemNoLocInDocRep::Create(m_arAppItemLoc, pDoc->GetName(), pDoc->GetNamespace());
		nIdx				= m_ListDocument.AddString(pDoc->GetTitle());
		m_ListDocument.SetItemData(nIdx, (DWORD) pItemNoLocInDocRep);
	}	
}

