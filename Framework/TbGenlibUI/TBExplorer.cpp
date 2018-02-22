#include "stdafx.h"

#include <TBNameSolver\LoginContext.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\FontsTable.h>
#include <TbGeneric\Dibitmap.h>
#include <TbGenlib\TBToolBar.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "TBExplorer.h"
#include "TBExplorer.hjson" //JSON AUTOMATIC UPDATE
#include "TBExplorerUtility.h"

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

extern int CompareObjDetails(CObject* , CObject* );
extern int CompareObjUser   (CObject* , CObject* );
//=============================================================================

const TaskBuilderToolbarImageSet IMGSET_REFRESH	(		_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(szIconRefresh, TOOLBAR),	0, 0);
const TaskBuilderToolbarImageSet IMGSET_SORT(			_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(szIconOrderAsc, TOOLBAR), 0, 0);
const TaskBuilderToolbarImageSet IMGSET_FILTER_STD(		_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(szIconPin, TOOLBAR), TBIcon(szIconPinned, TOOLBAR), 0, 0);
const TaskBuilderToolbarImageSet IMGSET_FILTER_ALLUSR(	_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(szIconAllUsers, TOOLBAR), 0, 0);
const TaskBuilderToolbarImageSet IMGSET_FILTER_USR(		_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(szIconUser, TOOLBAR), 0, 0);
const TaskBuilderToolbarImageSet IMGSET_STYLE(			_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(_T("Options"), TOOLBAR), 0, 0);
const TaskBuilderToolbarImageSet IMGSET_INSERT(			_T("Framework.TbGenlibUI.TbGenlibUI"), TBIcon(_T("folder_open-add"), TOOLBAR), 0, 0);

static CTBExplorerCachePtr GetExplorerCache() 
{
	return CTBExplorerCachePtr(AfxGetLoginContext()->GetObject<CTBExplorerCache>(), TRUE);
}

IMPLEMENT_DYNCREATE(CTBExplorerCache, CObject)

//============================================================================
//		CItemNoLoc implementation
//			per localizzazione
//============================================================================
CItemNoLoc::CItemNoLoc(CString strName, CString strTitle)
{
	m_strName = strName;
	m_strTitle = strTitle; 
};

//==========================================================================
//							CTreeItemSel
//==========================================================================
//--------------------------------------------------------------------------
CTreeItemSel::CTreeItemSel(const CString& sFullPathName /*= _T("")*/, HTREEITEM hSel/*NULL*/)
{
	m_strFullPathName = sFullPathName;
	m_hSel			  = hSel;	
}	

//==========================================================================
//							CTBMultiSelTreeExplorer
//==========================================================================
BEGIN_MESSAGE_MAP(CTBMultiSelTreeExplorer, CTBTreeCtrl)
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,	OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,		OnItemEndEdit)
	ON_WM_KEYDOWN		()							//per gestione F2
	ON_COMMAND			(ID_OPEN_CMD,		OnOpen)
	ON_COMMAND			(ID_DELETE_CMD,	OnDelete)
	ON_COMMAND			(ID_RENAME_CMD,	OnRenameLabel)
	ON_COMMAND			(ID_CUT_PATH,		OnCutPath)
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBMultiSelTreeExplorer, CTBTreeCtrl)
//--------------------------------------------------------------------------
CTBMultiSelTreeExplorer::CTBMultiSelTreeExplorer(BOOL bSave/* = FALSE*/)
{
	m_bMultiSelect = TRUE;
	m_bSave = bSave;
	m_bEditMode = FALSE;
}


//--------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::GetSelectedTBExplorerObjs(CTBObjDetailsArray* pTBObjDetailsArray)
{
	CHTreeItemsArray aHTreeItemsArray;

	if (!pTBObjDetailsArray)
		pTBObjDetailsArray = new CTBObjDetailsArray();
	else
		pTBObjDetailsArray->RemoveAll();

	GetSelectedHItems(&aHTreeItemsArray);
	for (int i = 0 ; i < aHTreeItemsArray.GetSize() ; i++)
	{
		CHTreeItem* pHTreeItem = aHTreeItemsArray.GetAt(i);
		if (!pHTreeItem || !pHTreeItem->m_hItem)
		{
			ASSERT(FALSE);
			continue;
		}
		pTBObjDetailsArray->Add((CTBObjDetails*)GetItemData(pHTreeItem->m_hItem));
	}
}

//---------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnRButtonDown(UINT nFlags, CPoint point) 
{		
	__super::OnRButtonDown(nFlags, point);

	HTREEITEM hItem = HitTest(point);
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect		rcTree;	
	HTREEITEM	hItemToSelect = GetSelectedItem();
	
	if (hItemToSelect)
	{
		CMenu				menu;
		CTBExplorerUserDlg* pExplorerDlg	= (CTBExplorerUserDlg*) GetParent();

		menu.CreatePopupMenu();
		if(!menu)
			ASSERT(FALSE);

		if (pExplorerDlg->CanDelete())				//User nn può cancellare/rinominare
			menu.AppendMenu(MF_STRING, ID_DELETE_CMD, _TB("Delete"));

		if (pExplorerDlg->CanRename())
			menu.AppendMenu(MF_STRING, ID_RENAME_CMD, _TB("Rename"));
			
		if (pExplorerDlg->CanOpen() && !m_bSave)
			menu.AppendMenu(MF_STRING, ID_OPEN_CMD, _TB("Open"));

		if (pExplorerDlg->CanCutPath())
			menu.AppendMenu(MF_STRING, ID_CUT_PATH , _TB("Copy path"));

		ClientToScreen(&mousePos);			
		menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, mousePos.x, mousePos.y, this);	
	}
}

//----------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnOpen() 
{
	CTBExplorerUserDlg* pExplorerUserDlg = (CTBExplorerUserDlg*) GetParent();
	pExplorerUserDlg->OnDlgOK();
}

//----------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnDelete() 
{
	CTBExplorerUserDlg* pExplorerUserDlg = (CTBExplorerUserDlg*) GetParent();
	pExplorerUserDlg->OnDelete();
}

//----------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnRenameLabel() 
{
	HTREEITEM			hSelItem = GetSelectedItem();
	CHTreeItemsArray	aHTreeItemsArray;

	GetSelectedHItems(&aHTreeItemsArray);
	CTBExplorerUserDlg* pExplorerDlg = (CTBExplorerUserDlg*) GetParent();
	pExplorerDlg->OnRenameLabel(aHTreeItemsArray);
}

//-------------------------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnCutPath()
{
	if (AfxGetLoginInfos()->m_bAdmin)
	{
		CTBExplorerAdminDlg* pExplorerDlg = (CTBExplorerAdminDlg*) GetParent();
 		pExplorerDlg->OnCutPath();
	}	
}

//-------------------------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnItemBeginEdit (NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	CTBExplorerUserDlg* pExplorerDlg = (CTBExplorerUserDlg*) GetParent();
	if (!pExplorerDlg->CanDelete())
	{
		*pResult = 1;
		return;
	}	

	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
	m_bEditMode = TRUE;	
}

//-------------------------------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnItemEndEdit (NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;
	// lpDispInfo->item.pszText punta alla stringa modificata ed è NULL se
	// non sono state apportate modifiche
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

	SetItemText(lpDispInfo->item.hItem, lpDispInfo->item.pszText);

	if (GetParent()->IsKindOf(RUNTIME_CLASS(CTBExplorerUserDlg)))
	{
		CTBExplorerUserDlg* pExplorerUserDlg = (CTBExplorerUserDlg*) GetParent();
		pExplorerUserDlg->OnRename(lpDispInfo->item.pszText);
		pExplorerUserDlg->OnRefresh();
		*pResult = 0;
		SelectItem(lpDispInfo->item.hItem);
		m_bEditMode = FALSE;
	}
}

//---------------------------------------------------------------------
void CTBMultiSelTreeExplorer::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	if (nChar == 113)
		OnRenameLabel();

	if (nChar == 46)
	{
		BOOL bCanDel = ((CTBExplorerUserDlg*) GetParent())->CanDelete();
		if (bCanDel)
			OnDelete();
	}

	if (nChar == 116)
		((CTBExplorerUserDlg*) GetParent())->OnKeyDown(nChar, nRepCnt, nFlags);

	__super::OnKeyDown(nChar, nRepCnt, nFlags);	
}


//==========================================================================
//							CTBExplorer
//==========================================================================
IMPLEMENT_DYNAMIC(CTBExplorer, ITBExplorer)

//--------------------------------------------------------------------------
void CTBExplorer::ClearStoredInfo()
{
	CTBExplorerCachePtr cache = GetExplorerCache();
	//verifica che le due stringhe non siano vuote, se lo sono memorizza le info relative a company e user
	if (cache->m_LastCompanyConnected == _T("") && cache->m_LastUserConnected == _T(""))
	{
		cache->m_LastCompanyConnected = AfxGetLoginInfos()->m_strCompanyName;
		cache->m_LastUserConnected = AfxGetLoginInfos()->m_strUserName ;
	}
	else
	{
		//se company o user sono diversi da quelli memorizzati azzera le info relative all'ultimo modulo
		if (AfxGetLoginInfos()->m_strCompanyName != cache->m_LastCompanyConnected 
			|| AfxGetLoginInfos()->m_strUserName != cache->m_LastUserConnected )
		{
			cache->m_LastCompanyConnected = AfxGetLoginInfos()->m_strCompanyName;
			cache->m_LastUserConnected = AfxGetLoginInfos()->m_strUserName;
			cache->m_LastUsedNameSpace.Clear();
			cache->m_LastUserFiltered = 0;
		}
	}

}
//--------------------------------------------------------------------------
CTBExplorer::CTBExplorer(TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew /*= FALSE*/, BOOL bOnlyStdAndAllusr /*= FALSE*/, ForceExplorerUser forceExplorerUser /*= DEFAULT */, BOOL bSaveForAdmin /*= FALSE */)
	:
	m_pTBExplorerDlg(NULL),
	m_bSaveInCurrentLanguage (FALSE)
{			
	CString			strApp;
	AddOnModsArray* pMods = NULL;

	m_bIsNew			= bIsNew;
	m_bOnlyStdAndAllusr = bOnlyStdAndAllusr;
	m_bIsMultiOpen		= FALSE;
	m_pSelElements		= new CTreeItemSelArray();
	m_ExplorerType		= aType;
	m_ForceExplorerUser	= forceExplorerUser;	// forza l'apertura con modalità user/admin (con differenti possibilità d'azione) anche se la chiamata arriva da amministratore
	m_bSaveForAdmin		= bSaveForAdmin;		// in caso di Admin, con chiamata da SaveAs nasconte la dialog del CTBExplorerAdminDlg
	m_bCanLink			= FALSE;
	
	//Cancella le informazioni relative all'ultimo modulo selezionato
	CTBExplorer::ClearStoredInfo();

	if (!aNameSpace.IsValid())				//costruzione del NameSpace 
	{	
		if (aNameSpace.GetType() == CTBNamespace::NOT_VALID)
			m_NameSpace.SetType(CTBNamespace::REPORT);
        else 
			m_NameSpace.SetType(aNameSpace.GetType());

		if (aNameSpace.GetApplicationName().IsEmpty())
		{
			AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
			m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
		}
		else
			m_NameSpace.SetApplicationName(aNameSpace.GetApplicationName());

		if (aNameSpace.GetObjectName(CTBNamespace::MODULE).IsEmpty())
		{
			AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
			if (pAddOnApp)
				pMods = pAddOnApp->m_pAddOnModules;
			
			if (pMods && pMods->GetSize())
			{
				for (int i = 0; i <= pMods->GetUpperBound(); i++)
				{
					//tra i moduli cerca il primo attivo e lo inizializza come primo namespace
					if (!AfxIsActivated(pMods->GetAt(i)->GetApplicationName(), pMods->GetAt(i)->GetModuleName()))
						continue;

					m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMods->GetAt(i)->GetModuleName());
					break;
				}
			}
		}
		else
			m_NameSpace.SetObjectName(CTBNamespace::MODULE, aNameSpace.GetObjectName(CTBNamespace::MODULE));

		CTBExplorerCachePtr cache = GetExplorerCache();

		//memorizza l'ultimo modulo selezionato
		if (!cache->m_LastUsedNameSpace.IsEmpty() )
			m_NameSpace.SetObjectName(CTBNamespace::MODULE, cache->m_LastUsedNameSpace.GetModuleName());

	}
	else
		m_NameSpace = aNameSpace;
}

//--------------------------------------------------------------------------
CTBExplorer::~CTBExplorer()
{	
	if (m_pSelElements)
	{	
		delete m_pSelElements;
		m_pSelElements = NULL;
	}
	delete m_pTBExplorerDlg;
}

//--------------------------------------------------------------------------
BOOL CTBExplorer::Open()
{
	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	switch(m_ForceExplorerUser)
	{
		case /*ForceExplorerUser::*/FORCE_ADMIN:
		{
			m_pTBExplorerDlg =  new CTBExplorerAdminDlg(m_NameSpace, m_ExplorerType, m_pSelElements, m_bIsNew, m_bIsMultiOpen, m_bCanLink, m_bOnlyStdAndAllusr, m_bSaveForAdmin);
			break;
		}
		case /*ForceExplorerUser::*/FORCE_USER:
		{
			m_pTBExplorerDlg =  new CTBExplorerUserDlg(IDD_EXPLORER_ADMIN, m_NameSpace, m_ExplorerType, m_pSelElements, m_bIsNew, m_bIsMultiOpen, m_bCanLink);
			break;
		}

		case /*ForceExplorerUser::*/DEFAULT:
		default:
		{
			const CLoginInfos* pInfoLog = AfxGetLoginInfos();
			if (AfxGetLoginInfos()->m_bAdmin || (m_NameSpace.GetType() == CTBNamespace::REPORT && (AfxGetLoginInfos()->m_bEasyBuilderDeveloper)))
				m_pTBExplorerDlg =  new CTBExplorerAdminDlg(m_NameSpace, m_ExplorerType, m_pSelElements, m_bIsNew, m_bIsMultiOpen, m_bCanLink, m_bOnlyStdAndAllusr, m_bSaveForAdmin);
			else
				m_pTBExplorerDlg =  new CTBExplorerUserDlg(IDD_EXPLORER_ADMIN, m_NameSpace, m_ExplorerType, m_pSelElements, m_bIsNew, m_bIsMultiOpen, m_bCanLink);
			break;
		}
	}
	
	if (m_pTBExplorerDlg->DoModal() == IDOK)
	{
		m_bSaveInCurrentLanguage = m_pTBExplorerDlg->GetIsSavedInCurrentLanguage();
		
		//ripristina lo stato del macrorecorder
		AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;
		return TRUE;
	}

	//ripristina lo stato del macrorecorder
	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;

	return FALSE;
}

//--------------------------------------------------------------------------
void CTBExplorer::GetSelPathElements(CStringArray* pSelectedPaths)
{
	if (!m_pTBExplorerDlg && !pSelectedPaths)
		return;	

	pSelectedPaths->RemoveAll();
	for (int i = 0; i <= m_pSelElements->GetUpperBound(); i++)
	{	
		CTreeItemSel* pSelElement = m_pSelElements->GetAt(i);
		if (!pSelElement)
			continue;
		pSelectedPaths->Add(pSelElement->m_strFullPathName);  
	}
}

//--------------------------------------------------------------------------
void CTBExplorer::GetSelPathElement(CString& StrSelectedPath)
{
	if (!m_pTBExplorerDlg && !StrSelectedPath)
		return;	
	CTreeItemSel* pSelElement = NULL;
	if (m_pSelElements->GetSize())
		pSelElement = m_pSelElements->GetAt(0);

	if (!pSelElement && !m_bCanLink)
		return;
	if (m_bCanLink && !m_pTBExplorerDlg->m_strLinkFullPath.IsEmpty())
		StrSelectedPath = m_pTBExplorerDlg->m_strLinkFullPath;
	else if (pSelElement)
		StrSelectedPath = pSelElement->m_strFullPathName;	
}

//--------------------------------------------------------------------------
void CTBExplorer::GetSelNameSpace(CTBNamespace& Ns)
{
	CString strSelPath = _T("");
	GetSelPathElement(strSelPath);
	Ns = AfxGetPathFinder()->GetNamespaceFromPath(strSelPath);
}

//--------------------------------------------------------------------------
void CTBExplorer::GetSelArrayNameSpace(CTBNamespaceArray& aNs)
{
	CStringArray aSelPath;
	GetSelPathElements(&aSelPath);
	CTBNamespace Ns;

	for (int i = 0; i <= aSelPath.GetUpperBound(); i++)
	{
		Ns = AfxGetPathFinder()->GetNamespaceFromPath(aSelPath.GetAt(i));
		aNs.Add(new CTBNamespace(Ns));
	}
}

//--------------------------------------------------------------------------
int CTBExplorer::GetSavePath(CStringArray& aSavePath)
{
	for (int i = 0 ; i < m_pSelElements->GetSize(); i++)
	{
		CTreeItemSel* pSelElement = m_pSelElements->GetAt(i);
		if (!pSelElement)
			continue;
		aSavePath.Add(pSelElement->m_strFullPathName);
	}
	return aSavePath.GetSize();
}

//--------------------------------------------------------------------------
void CTBExplorer::SetMultiOpen()
{
	m_bIsMultiOpen = TRUE;
}

//==========================================================================
//							CTBObjLocation
//==========================================================================
//--------------------------------------------------------------------------
CTBObjLocation::CTBObjLocation(const CString& sUserName, const CString& sFullFileName, CPathFinder::PosType enPosType)
{
	m_sFullFileName = sFullFileName;
	m_sUserName		= sUserName;
	m_enPosType		= enPosType;
}

//--------------------------------------------------------------------------
CTBObjLocation::CTBObjLocation(CTBObjLocation& aCopy)
{
	m_sFullFileName = aCopy.m_sFullFileName;
	if (aCopy.m_enPosType == CPathFinder::USERS)
		m_sUserName	= aCopy.m_sUserName;
	else
		m_sUserName = _T("");
		
	m_enPosType = aCopy.m_enPosType;
}

//----------------------------------------------------------------------------------------------
CTBObjLocation& CTBObjLocation::operator = (const CTBObjLocation& aCopy)
{
	m_sFullFileName = aCopy.m_sFullFileName;
	m_sUserName		= aCopy.m_sUserName;
	m_enPosType		= aCopy.m_enPosType;
	return *this;
}

//==========================================================================
//							CTBObjLocationArray
//==========================================================================
//--------------------------------------------------------------------------
CTBObjLocation* CTBObjLocationArray::GetLocation(const CString& userName)
{
	for (int i = 0 ; i < GetSize() ; i++)
	{
		CTBObjLocation* pTBObjLocation = GetAt(i);
		if (pTBObjLocation->m_sUserName == userName)
		return pTBObjLocation;
	}
	return NULL;
}

//--------------------------------------------------------------------------
CTBObjLocation* CTBObjLocationArray::GetLocationForUserDlg()
{	
	if (GetSize())
	{
		CTBObjLocation* pTBObjLocation = GetAt(0);
		return pTBObjLocation;
	}
	return NULL;
}

//--------------------------------------------------------------------------
CTBObjLocation* CTBObjLocationArray::GetLocationForSpecificUser(const CString& strUser)
{
	if(GetSize())
	{
		for (int i = 0; i <= GetUpperBound() ; i++ )
		{
			CTBObjLocation* pTBObjLocation = GetAt(i);
			if (pTBObjLocation->m_sUserName.CompareNoCase(strUser) == 0)
				return pTBObjLocation;		
		}	
	}
	return NULL;
}

//--------------------------------------------------------------------------
CTBObjLocation* CTBObjLocationArray::GetLocationStandard()
{
	if(GetSize())
	{
		for (int i = 0; i <= GetUpperBound() ; i++ )
		{
			CTBObjLocation* pTBObjLocation = GetAt(i);
			if (pTBObjLocation->m_enPosType == CPathFinder::STANDARD)
				return pTBObjLocation;		
		}	
	}
	return NULL;
}

//--------------------------------------------------------------------------
void CTBObjLocationArray::GetAllOwner(CStringArray* pUser)
{
	if(!pUser)
		ASSERT(FALSE);

	CTBObjLocation* pTBObjLocation = NULL;
	if(GetSize() > 0)
	{
		for (int i = 0 ; i <= GetUpperBound(); i++)
		{
			pTBObjLocation = GetAt(i);
			if (pTBObjLocation && pTBObjLocation->m_enPosType == CPathFinder::USERS)
				pUser->Add(pTBObjLocation->m_sUserName);
		}
	}
}

//--------------------------------------------------------------------------
void CTBObjLocationArray::GetAllLocations(CStringArray* pLocation)
{
	if (!pLocation)
		ASSERT(FALSE);

	CTBObjLocation* pTBObjLocation = NULL;
	if(GetSize() > 0)
	{
		for (int i = 0 ; i <= GetUpperBound(); i++)
		{
			pTBObjLocation = GetAt(i);
			if (pTBObjLocation)
				pLocation->Add(pTBObjLocation->m_sFullFileName);
		}
	}
}

//--------------------------------------------------------------------------
CTBObjLocationArray& CTBObjLocationArray::operator = (const CTBObjLocationArray& aCopy)
{
	if (this == &aCopy)
		return *this;

	RemoveAll();

	for (int i = 0; i <= aCopy.GetUpperBound(); i++)
	{
		CTBObjLocation* pTBObjLocation = new CTBObjLocation(*(aCopy.GetAt(i)));
		Add(pTBObjLocation);
	}

	return *this;
}

//--------------------------------------------------------------------------
//==========================================================================
//							CTBObjDetails
//==========================================================================
//--------------------------------------------------------------------------
CTBObjDetails::CTBObjDetails(const CString&	sName) 
{
	m_sName	= sName;	//nome obj con estensione
}

//--------------------------------------------------------------------------
CTBObjDetails::CTBObjDetails(CTBObjDetails& aCopy)
{
	m_sName = aCopy.m_sName;
	m_TBObjLocationArray = aCopy.m_TBObjLocationArray;
}

//----------------------------------------------------------------------------------------------
CTBObjDetails& CTBObjDetails::operator = (const CTBObjDetails& aCopy)
{
	m_sName = aCopy.m_sName;
	return *this;
}

//--------------------------------------------------------------------------
CTBObjDetails::~CTBObjDetails()
{
}	

//--------------------------------------------------------------------------
int CTBObjDetails::AddLocation(const CString& strUserName, const CString& sFullFileName, CPathFinder::PosType enPosType)
{
	CTBObjLocation* pTBObjLocation = new CTBObjLocation(strUserName, sFullFileName, enPosType);
	return m_TBObjLocationArray.Add(pTBObjLocation);
}

//--------------------------------------------------------------------------
int CTBObjDetails::AddLocation(CTBObjLocation* pTBObjLocation)
{
	if (!pTBObjLocation)
	{
		ASSERT(FALSE);
		return -1;
	}
	return m_TBObjLocationArray.Add(pTBObjLocation);
}

//--------------------------------------------------------------------------
BOOL CTBObjDetails::IsAllUser(int n /*= 0*/)
{
	if (m_TBObjLocationArray.GetSize() == 0)
		return FALSE;

	CTBObjLocation* pTBObjLocation = m_TBObjLocationArray.GetAt(n);
	return (pTBObjLocation->m_enPosType == CPathFinder::ALL_USERS);
}

//--------------------------------------------------------------------------
BOOL CTBObjDetails::IsStandard(int n /*= 0*/)
{
	if (m_TBObjLocationArray.GetSize() == 0)
		return FALSE;

	CTBObjLocation* pTBObjLocation = m_TBObjLocationArray.GetAt(n);
	return (pTBObjLocation->m_enPosType == CPathFinder::STANDARD);
}

//--------------------------------------------------------------------------
BOOL CTBObjDetails::IsUser(int n /*= 0*/)
{
	if (m_TBObjLocationArray.GetSize() == 0) 
		return FALSE;

	CTBObjLocation* pTBObjLocation = m_TBObjLocationArray.GetAt(n);
	return (pTBObjLocation->m_enPosType == CPathFinder::USERS);
}

//--------------------------------------------------------------------------
//	IMPOSTARE LA PROPRIETA' TYPE = OWNER DRAW
//
//==========================================================================
//							CTBExplorerStatic
//==========================================================================
//IMPLEMENT_DYNAMIC(CTBExplorerStatic, CStatic)
//--------------------------------------------------------------------------
//--------------------------------------------------------------------------
void CTBExplorerStatic::SetIcon(UINT nIdResource)
{
	m_IDI = nIdResource;
}

//--------------------------------------------------------------------------
void  CTBExplorerStatic::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	CDC aDC; 
	aDC.Attach(lpDrawItemStruct->hDC);

	CRect	rect		(lpDrawItemStruct->rcItem);	
	int		cxIcon		= ::GetSystemMetrics(SM_CXICON);
	int		cyIcon		= ::GetSystemMetrics(SM_CYICON);

	HICON	hicon		= LoadWalkIcon(m_IDI);		
	if (hicon == NULL) 
		return;	

	CBitmap bitmap;
	if (!bitmap.CreateCompatibleBitmap(&aDC, cxIcon, cyIcon)) 
		return;	

	CDC dcMem;
	if (!dcMem.CreateCompatibleDC(&aDC)) 
		return;	

	CBitmap* pBitmapOld = dcMem.SelectObject(&bitmap);
	if (pBitmapOld == NULL) 
		return;

	dcMem.StretchBlt(0, 0, 32, 32, &aDC,1, 1, 32, 32, SRCCOPY);
	dcMem.DrawIcon(0, 0, hicon);
	aDC.StretchBlt(1, 1, 16, 16,&dcMem, 0, 0, 32, 32, SRCCOPY);
}

//==========================================================================
//							CTBExplorerUserDlg
//==========================================================================
IMPLEMENT_DYNAMIC(CTBExplorerUserDlg, CParsedDialog)

//--------------------------------------------------------------------------
CTBExplorerUserDlg::CTBExplorerUserDlg(UINT IDD, CTBNamespace& aNameSpace, CTBExplorer::TBExplorerType aExplorerType, CTreeItemSelArray* pSelElements, BOOL bIsNew, BOOL bIsMultiOpen, BOOL bCanLink)
	: 
	CParsedDialog				(IDD),
	m_TreeObj					(aExplorerType == CTBExplorer::SAVE),
	m_bSaveInCurrentLanguage	(FALSE)
{		
	m_NameSpace			= aNameSpace;
	m_ExplorerType		= aExplorerType;
	m_pSelElements		= pSelElements;
	m_bIsNew			= bIsNew;
	m_bIsMultiOpen		= bIsMultiOpen;
	m_bCanLink			= bCanLink;
	m_pTBExplorer		= NULL;
	m_bFirstFill		= TRUE;
	m_bUsr				= FALSE;
	m_bAllUsrs			= TRUE;
	m_bStd				= TRUE;	
	m_strLinkFullPath	= _T("");
	m_bBtnUsrPressed	= FALSE;
	m_bBtnAllUsrPressed	= TRUE;
	m_bBtnStdPressed	= TRUE;
	m_bSaveForAllUsrs	= FALSE;
	m_bSaveForStandard	= FALSE;

	m_aUsers.Add(AfxGetLoginInfos()->m_strUserName);
	if (!AfxGetLoginInfos()->m_bAdmin)
		m_aSaveUsers.Add(AfxGetLoginInfos()->m_strUserName);	

	m_aFilterUser.Copy(m_aUsers);
}

//--------------------------------------------------------------------------
CTBExplorerUserDlg::~CTBExplorerUserDlg()
{
	m_ImageList.DeleteImageList();
	m_AppsImageList.DeleteImageList();
}

//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBExplorerUserDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CTBFileDlg
	ON_CBN_SELCHANGE	(IDC_COMBO_OBJ,			OnComboObjChanged)
	ON_CBN_SELCHANGE	(IDC_COMBO_MODS,		OnComboModsChanged)
	ON_BN_CLICKED		(IDC_BTN_REFRESH,		OnBtnRefreshClicked)
	ON_BN_CLICKED		(IDC_BTN_INSERT,		OnInsertObj)	  
	ON_BN_CLICKED		(IDC_BTN_FILTER_USR,	OnBtnFilterUsrClicked)
	ON_BN_CLICKED		(IDC_BTN_FILTER_ALLUSR,	OnBtnFilterAllUsrClicked)
	ON_BN_CLICKED		(IDC_BTN_FILTER_STD,	OnBtnFilterStdClicked)
	ON_WM_KEYDOWN		()
	ON_NOTIFY			(TVN_SELCHANGED,	IDC_TREE_OBJ, OnItemSelectionChange)
	ON_NOTIFY			(NM_DBLCLK,			IDC_TREE_OBJ, OnDoubleClickTree)
	ON_NOTIFY			(LVN_ITEMCHANGED,	IDC_LISTAPPS, OnListSelchanged)
	ON_MESSAGE			(UM_GET_WEB_COMMAND_TYPE, OnGetWebCommandType)
	ON_EN_CHANGE		(IDC_EDIT_OBJ ,			OnEditTextChanged)
	ON_UPDATE_COMMAND_UI(IDC_BTN_FILTER_ALLUSR, OnUpdateBtnAllUsr)
	ON_UPDATE_COMMAND_UI(IDC_BTN_FILTER_STD,	OnUpdateBtnStd)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnUpdateBtnAllUsr(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bBtnAllUsrPressed);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnUpdateBtnStd(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bBtnStdPressed);
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();

	SetToolbarStyle(CParsedDialog::TOP, DEFAULT_TOOLBAR_HEIGHT, FALSE, TRUE);

	CString			strApp = m_NameSpace.GetApplicationName();

	VERIFY(m_ComboObj			.SubclassDlgItem	(IDC_COMBO_OBJ,			this));
	VERIFY(m_TreeObj			.SubclassDlgItem	(IDC_TREE_OBJ,			this));
	VERIFY(m_EditObj			.SubclassDlgItem	(IDC_EDIT_OBJ,			this));
	VERIFY(m_Btn_Ok				.SubclassDlgItem	(IDOK,					this));
	VERIFY(m_ListApplications	.SubclassDlgItem	(IDC_LISTAPPS,			this));
	VERIFY(m_LabelObj.SubclassDlgItem(IDC_GROUP_SAVE, this));

	if (m_NameSpace.GetType() != CTBNamespace::REPORT || m_ExplorerType != CTBExplorer::SAVE )
	{
		m_LabelObj.ShowWindow(SW_HIDE);
	}

	int IcoSize = ScalePix(24);

	m_AppsImageList.Create(IcoSize, IcoSize, ILC_COLOR24, IcoSize, IcoSize);
	m_AppsImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
	m_ListApplications.SetImageList(&m_AppsImageList, LVSIL_NORMAL);

	FillListApps();

	POSITION	Pos			= m_ListApplications.GetFirstSelectedItemPosition();
	int			nItem		= m_ListApplications.GetNextSelectedItem(Pos);
	if (nItem < 0) 
		nItem = 0;
	CItemNoLoc* pItemNoLoc	= (CItemNoLoc*) m_ListApplications.GetItemData(nItem);

	if (pItemNoLoc)
		strApp = pItemNoLoc->m_strName;

	LoadImageList	();

	FillModsCombo	(strApp, TRUE);
	SetControlDlg	();

	((CButton*)GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE))->SetCheck(0);

	if (!m_bIsNew && m_ExplorerType == CTBExplorer::SAVE)
	{
		m_pToolBar->EnableButton(IDC_COMBO_MODS, FALSE);
		m_ListApplications.EnableWindow(FALSE);	
	}
	if (m_ExplorerType != CTBExplorer::SAVE)
	{
		GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE)->ShowWindow(SW_HIDE);
	}
	else 
	{
		((CButton*)GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE))->SetCheck(1); 
	}
	m_EditObj.SetWindowText((LPCTSTR)m_strInitEditObj);	
	EnableToolTips(TRUE);
	if (m_ExplorerType != CTBExplorer::SAVE)
		m_TreeObj.SetFocus();

    return FALSE;
}

//-----------------------------------------------------------------------------
void CTBExplorerUserDlg::OnCustomizeToolbar()
{
	CString sNs = _T("Framework.TbGenlibUI.TbGenlibUI");

	//m_pToolBar->AddLabel(-1, _TB("Find_in:"));
	m_pToolBar->AddComboBox(IDC_COMBO_MODS, sNs, _T("ComboMods"),300, (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), _T("Find:"));
	m_pToolBar->AddSeparator();

	m_pToolBar->AddButton(IDC_BTN_REFRESH,		sNs,	TBIcon(szIconRefresh, TOOLBAR),			_TB("Refresh"), FALSE);
	m_pToolBar->AddButton(IDC_BTN_INSERT,		sNs,	TBIcon(_T("FolderOpenAdd"), TOOLBAR),	_TB("Insert"), FALSE);
	m_pToolBar->AddButton(IDC_BTN_FILTER_STD,	sNs,	TBIcon(szIconPin, TOOLBAR),				_TB("Filter STD"), FALSE);
	m_pToolBar->AddButton(IDC_BTN_FILTER_ALLUSR, sNs,	TBIcon(szIconAllUsers, TOOLBAR),		_TB("All Users"), FALSE);
	
	m_pToolBar->AddSeparator();
}

//-----------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::OnToolbarDropDown (UINT id, CMenu& menu)
{
	/*BOOL bPressed = 0;
	switch(id)
	{
		case (IDC_BTN_STYLE):
			//tipo di stile
			break;
		case (IDC_BTN_SORT):
			//sort per nome o proprietario+nome
			break;			
	}*/

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTBExplorerUserDlg::OnEditTextChanged()
{
	//evento sul cambiamento della edit  box: disabilita il tasto se vuota
	CString strResult; 
	m_EditObj.GetWindowText(strResult);
	// Special characters check in object name
	BOOL bIsValidName = strResult.IsEmpty() ? TRUE : IsValidObjName(GetName(strResult));
	if (!bIsValidName)
		AfxMessageBox(_TB("A Tb name cannot contain any of the following characters:\r\n /?*\\<>|\" \".\r\nUnable to rename."), MB_OK | MB_ICONEXCLAMATION  );

	if (strResult.IsEmpty() || !bIsValidName)
		m_Btn_Ok.EnableWindow(FALSE);
	else	
		m_Btn_Ok.EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::GetToolTipProperties(CTooltipProperties& tp)
{
	tp.m_strText.Empty();
	if (tp.m_nControlID == IDC_COMBO_MODS)
		tp.m_strText = _TB("Modules list");
	else if (tp.m_nControlID == IDC_BTN_STYLE)
		tp.m_strText = _TB("Styles");
	else if (tp.m_nControlID == IDC_BTN_REFRESH)
		tp.m_strText = _TB("Update");
	else if (tp.m_nControlID == IDC_BTN_INSERT)
		tp.m_strText = _TB("Add");
	else if (tp.m_nControlID == IDC_BTN_FILTER_USR)
		tp.m_strText = _TB("Filter for user");
	else if (tp.m_nControlID == IDC_BTN_FILTER_ALLUSR)
		tp.m_strText = _TB("Filter for all users");
	else if (tp.m_nControlID == IDC_BTN_FILTER_STD)
		tp.m_strText = _TB("Filter for original");
	else if (tp.m_nControlID == IDC_COMBO_USER)
		tp.m_strText = _TB("User list");
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTBExplorerUserDlg::OnBtnFilterUsrClicked()
{
	m_bBtnUsrPressed	= m_bUsr = !m_bBtnUsrPressed;

	if (m_bBtnUsrPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, TOOLBAR), NULL);
		//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_USR), TBSTATE_PRESSED | TBSTATE_ENABLED);
	}

	CString strEdit = m_strEditTreeObj;
	OnBtnFilterApply();
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
}

//----------------------------------------------------------------------------
void CTBExplorerUserDlg::OnBtnFilterStdClicked()
{	
	m_bBtnStdPressed = m_bStd = !m_bBtnStdPressed;
	/*CTBToolbarButton* pButton;*/

	//if (m_bBtnStdPressed)
	//{
	//	//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconPin, TOOLBAR), NULL);
	//	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_STD), TBBS_CHECKED);
	//}
	//else
	//{
	//	//m_pToolBar->SetButtonInfo((UINT) IDC_BTN_FILTER_STD, TBSTATE_ENABLED, TBIcon(szIconPinned, TOOLBAR), NULL);
	//	pButton = m_pToolBar->FindButtonPtr(IDC_BTN_FILTER_STD);
	//	pButton->SetStyle(TBBS_BUTTON);
	//}

	CString strEdit = m_strEditTreeObj;
	OnBtnFilterApply();
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
}

//-----------------------------------------------------------------------------
void CTBExplorerUserDlg::OnBtnFilterAllUsrClicked()
{
	m_bBtnAllUsrPressed = m_bAllUsrs = !m_bBtnAllUsrPressed;

	//if (m_bBtnAllUsrPressed)
	//{
	//	/*m_pToolBar->SetButtonInfo((UINT) IDC_BTN_FILTER_ALLUSR, 
	//								TBSTATE_PRESSED | TBSTATE_ENABLED, 
	//								TBIcon(szIconAllUsers, TOOLBAR),
	//								NULL);*/
	//	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_ALLUSR),
	//		TBSTATE_PRESSED | TBSTATE_ENABLED);
	//}
	//else
	//{
	//	/*	m_pToolBar->SetButtonInfo((UINT) IDC_BTN_FILTER_ALLUSR,
	//								TBSTATE_ENABLED, 
	//								TBIcon(szIconAllUsers, TOOLBAR),
	//								NULL);*/
	//	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_ALLUSR),
	//								TBSTATE_ENABLED);
	//}

	CString strEdit = m_strEditTreeObj;
	OnBtnFilterApply();
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::SetControlDlg()
{
	CString strTipoCaption;
	switch (m_NameSpace.GetType())
	{
		case (CTBNamespace::REPORT):
			strTipoCaption = _TB("Report");
			break;
		case (CTBNamespace::IMAGE):
			strTipoCaption =  _TB("Image");
			break;
		case (CTBNamespace::TEXT):
			strTipoCaption = _TB("Text");
			break;		
		case (CTBNamespace::PDF):
			strTipoCaption = _TB("Pdf");
			break;		
		case (CTBNamespace::RTF):
			strTipoCaption = _TB("Rtf");
			break;		
		case (CTBNamespace::FILE):
			strTipoCaption = _TB("Other");
			break;	

		case (CTBNamespace::ODT):
			strTipoCaption = _TB("Open Office ODT documents");
			break;		
		case (CTBNamespace::ODS):
			strTipoCaption = _TB("Open Office ODS documents");
			break;		

		case (CTBNamespace::WORDDOCUMENT):
		case (CTBNamespace::WORDTEMPLATE):
			strTipoCaption = _TB("Microsoft Word documents");
			break;		
		case (CTBNamespace::EXCELDOCUMENT):
		case (CTBNamespace::EXCELTEMPLATE):
			strTipoCaption = _TB("Microsoft Excel documents");
			break;		
	}

	switch (m_ExplorerType)
	{
		case CTBExplorer::EXPLORE:
			FillObjCombo();
			m_EditObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_SALVA))->ShowWindow(SW_HIDE);
			SetWindowText(cwsprintf(_TB("Task Builder Explore - User: '{0-%s}'"), AfxGetLoginInfos()->m_strUserName ));
			m_Btn_Ok.EnableWindow(FALSE);
			break;
		case CTBExplorer::OPEN:
			m_EditObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_SALVA))->ShowWindow(SW_HIDE);
			m_ComboObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_OBJ))->ShowWindow(SW_HIDE);
			SetWindowText(cwsprintf(_TB("Task Builder Open {0-%s}- User: {1-%s}"), (LPCTSTR) strTipoCaption, AfxGetLoginInfos()->m_strUserName ) );
			m_Btn_Ok.EnableWindow(FALSE);
			break;
		case CTBExplorer::SAVE:
			m_ComboObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_OBJ))->ShowWindow(SW_HIDE);
			m_Btn_Ok.SetWindowText((LPCTSTR) _TB("Save"));
			m_strEditTreeObj =  m_NameSpace.GetObjectName();
			m_EditObj.SetWindowText((LPCTSTR)m_strEditTreeObj);	
			m_strInitEditObj = m_strEditTreeObj;
			SetWindowText(cwsprintf(_TB("Task Builder Save {0-%s} - User: {1-%s}"), strTipoCaption, AfxGetLoginInfos()->m_strUserName ));
			break;
	}

	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_ALLUSR), TBBS_CHECKED);
	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_STD), TBBS_CHECKED);

	OnBtnFilterUsrClicked();
	//OnBtnFilterStdClicked();
	//OnBtnFilterAllUsrClicked();
}

//--------------------------------------------------------------------------
const CString& CTBExplorerUserDlg::GetTitleFromItemLoc (const CString& strName, BOOL bIsModule)
{
    if (bIsModule)
		for (int i=0; i <= m_arModItemLoc.GetUpperBound(); i++)
		{
			CItemNoLoc* pItem = (CItemNoLoc*) m_arModItemLoc.GetAt(i);
			if (pItem && pItem->m_strName.CompareNoCase(strName) == 0)
				return pItem->m_strTitle;
		}
	else
		for (int i=0; i <= m_arAppItemLoc.GetUpperBound(); i++)
		{
			CItemNoLoc* pItem = (CItemNoLoc*) m_arAppItemLoc.GetAt(i);
			if (pItem && pItem->m_strName.CompareNoCase(strName) == 0)
				return pItem->m_strTitle;
		}
	ASSERT(FALSE);
	CString* pStr = new CString();
	return *pStr;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::FillListApps()
{
	CString			strApps			= _T("");
	CString			strDefaultApp	= _T("");
	CItemNoLoc*		pItemNoLoc		= NULL;
	int				nApp = 0;

	m_ListApplications.DeleteAllItems();

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		
		BOOL bSysApp = AfxGetPathFinder()->IsASystemApplication(strApps);

//#ifndef _DEBUG
//		if (bSysApp/* && !AfxGetBaseApp()->IsDevelopment()*/)
//			continue;
//#endif

		if 
			(
			(!AfxGetBaseApp()->IsDevelopment() &&
		     strApps.CompareNoCase(AfxGetBaseApp()->GetTaskBuilderAddOnApp()->m_strAddOnAppName) == 0)
			 )
			continue;

		if (!AfxGetAddOnApp(strApps))
			continue;

		if (!AfxGetLoginInfos()->m_bAdmin && !AfxIsAppActivated(strApps))
			continue;

		CString strTitle = AfxGetAddOnApp(strApps)->GetTitle();
		pItemNoLoc		= new CItemNoLoc(strApps, strTitle);
		m_arAppItemLoc.Add(pItemNoLoc);

		AddOnApplication* pAddOnApplicationExt = AfxGetBaseApp()->GetMasterAddOnApp();
		if (strDefaultApp.IsEmpty() && strApps.CompareNoCase( pAddOnApplicationExt->m_strAddOnAppName) == 0)
			strDefaultApp = strApps;

		CString strIcon;

		if (strIcon.IsEmpty())
			strIcon = (TBGlyph(szIconTBFramework));		

		CTBNamespace ns(strIcon);

		/*CString strIconFile = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		if (strIconFile.IsEmpty() && strIcon != TBGlyph(szIconTBFramework))
		{
			ns.Clear();
			ns.AutoCompleteNamespace(CTBNamespace::IMAGE, TBGlyph(szIconTBFramework), CTBNamespace());
			strIconFile = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		}
		ASSERT(!strIconFile.IsEmpty());*/
		// if empty return ... vedere 
		HICON hb = TBLoadImage(strIcon);
		m_AppsImageList.Add(hb);
		m_ListApplications.InsertItem(nApp, strTitle, nApp);
		m_ListApplications.SetItemData(nApp, (DWORD) pItemNoLoc);
		nApp++;
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
	{
		LVFINDINFO info;
		int nIndex;

		info.flags = LVFI_STRING;
		info.psz = (LPCTSTR) GetTitleFromItemLoc(pSelApp->m_strAddOnAppName);

		// Delete all of the items that begin with the string lpszmyString.
		if ((nIndex = m_ListApplications.FindItem(&info)) != -1 )
			m_ListApplications.SetItemState(nIndex, LVIS_SELECTED, LVIS_SELECTED | LVIS_FOCUSED);

	}
	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::FillModsCombo(const CString& sLabel/*= _TB("")*/, bool bFirst /*= FALSE */)
{
	CStringArray	aModules;
	CTBNamespace	Ns;
	CItemNoLoc*		pItemNoLoc = NULL;
	CString			strMods		= _T("");

	CString sApp = sLabel;
	//if (sApp.CompareNoCase(L"TBF") == 0 /*|| sApp.CompareNoCase(L"TBS") == 0*/)
	//	sApp = L"Framework";
	
	m_pToolBar->RemoveAllComboItems(IDC_COMBO_MODS);
	m_arModItemLoc.RemoveAll();

	if (sApp.IsEmpty())
	{		
		AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
		if (!pAddOnApp)
			return FALSE;
		
		BOOL bGoodApp = FALSE;
		for (int s = 0; s <= m_ListApplications.GetItemCount(); s++)
		{
			CItemNoLoc* pItemNoLoc = (CItemNoLoc*) m_ListApplications.GetItemData(s);
			if (pItemNoLoc->m_strName.CompareNoCase(pAddOnApp->m_strAddOnAppName) == 0)
			{
				bGoodApp = TRUE;			
				break;
			}	
		}

		if (!bGoodApp)
			return FALSE;

		for (int i = 0; i <= pAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
		{
			AddOnModule* pAddOnModule = pAddOnApp->m_pAddOnModules->GetAt(i);
			if (!AfxIsActivated(pAddOnModule->GetApplicationName(), pAddOnModule->GetModuleName()))
				continue;

			pItemNoLoc = new CItemNoLoc(pAddOnModule->GetModuleName(), pAddOnModule->GetModuleTitle());
			m_arModItemLoc.Add(pItemNoLoc);
			m_pToolBar->AddComboSortedItem(IDC_COMBO_MODS,pAddOnModule->GetModuleTitle(), (DWORD) pItemNoLoc);
		}

		AddOnModule* pAddOnMod = AfxGetAddOnModule(m_NameSpace);
		if (pAddOnMod != NULL)
		{
			int n = m_pToolBar->FindComboStringExact(IDC_COMBO_MODS, (LPCTSTR)pAddOnMod->GetModuleTitle());	
			m_pToolBar->SetComboItemSel(IDC_COMBO_MODS, n);
		}

		CStringArray aUser;
		if (bFirst)
			FillObjTreeInit(m_aFilterUser);
		else
			FillObjTree(m_aFilterUser);
	}
	else
	{
		AddOnApplication* pAddOnApplication =  AfxGetAddOnApp(sApp);
		if (!pAddOnApplication)
			return FALSE;
		for (int a = 0; a <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); a++)
		{
			AddOnModule* pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(a);
			if (!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
				continue;

			pItemNoLoc = new CItemNoLoc(pAddOnMod->GetModuleName(), pAddOnMod->GetModuleTitle());
			m_arModItemLoc.Add(pItemNoLoc);
			m_pToolBar->AddComboSortedItem(IDC_COMBO_MODS, pAddOnMod->GetModuleTitle(), (DWORD) pItemNoLoc);
		}	

		if (pAddOnApplication->m_pAddOnModules->IsEmpty())
		{	
			m_TreeObj.SetRedraw(FALSE);
			m_TreeObj.DeleteAllItems();
			m_TreeObj.SetRedraw(TRUE);
			m_TreeObj.Invalidate(FALSE);
			return TRUE;
		}

		int m = -1; 	
		AddOnModule* pAddOnMod = NULL;
		CTBNamespace aFirstActive; 

		if (bFirst)
		{
			pAddOnApplication =  AfxGetAddOnApp(m_NameSpace.GetApplicationName());
			if (pAddOnApplication)
			{
				for (int s = 0; s <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); s++)
				{
					pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(s);

					if (!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
						continue;

					if (aFirstActive.IsEmpty())
						aFirstActive = pAddOnMod->m_Namespace;
					if (pAddOnMod->GetModuleName().CompareNoCase(m_NameSpace.GetObjectName(CTBNamespace::MODULE)) == 0)
					{
						m = m_pToolBar->FindComboStringExact(IDC_COMBO_MODS, (LPCTSTR)pAddOnMod->GetModuleTitle());
						m_pToolBar->SetComboItemSel(IDC_COMBO_MODS, m);
					}
				}		
			}
		}

		if (!bFirst || m < 0)
		{	
			if (!aFirstActive.IsEmpty())
			{
				pAddOnMod = AfxGetAddOnModule (aFirstActive);
				ASSERT(pAddOnMod);
				m = m_pToolBar->FindComboStringExact(IDC_COMBO_MODS, (LPCTSTR)pAddOnMod->GetModuleTitle());	
				m_NameSpace.SetObjectName(CTBNamespace::MODULE, pAddOnMod->GetModuleName());
				m_pToolBar->SetComboItemSel(IDC_COMBO_MODS, m);
			}
		}

		if (bFirst)
			FillObjTreeInit(m_aUsers);
		
		FillObjTree(m_aFilterUser);
	}

//	if (m_ExplorerType == CTBExplorer::OPEN)
//		m_Btn_Ok.EnableWindow(FALSE);
	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::FillObjCombo()
{
	int nPosReport = m_ComboObj.AddString(_TB("Report"));
	m_ComboObj.SetItemData(nPosReport, CTBNamespace::REPORT);
	int nPosImage = m_ComboObj.AddString(_TB("Image"));
	m_ComboObj.SetItemData(nPosImage, CTBNamespace::IMAGE);
	int nPosText = m_ComboObj.AddString(_TB("Text"));
	m_ComboObj.SetItemData(nPosText, CTBNamespace::TEXT);
	int nPosOther = m_ComboObj.AddString(_TB("Other"));
	m_ComboObj.SetItemData(nPosText, CTBNamespace::FILE);
	int nPosPdf = m_ComboObj.AddString(_TB("Pdf"));
	m_ComboObj.SetItemData(nPosPdf, CTBNamespace::PDF);
	int nPosRtf = m_ComboObj.AddString(_TB("Rtf"));
	m_ComboObj.SetItemData(nPosRtf, CTBNamespace::RTF);
	//int nPosOdf = m_ComboObj.AddString(_TB("Odf"));
	//m_ComboObj.SetItemData(nPosOdf, CTBNamespace::ODF);

	switch (m_NameSpace.GetType())
	{
		case (CTBNamespace::IMAGE):
			m_ComboObj.SetCurSel(nPosImage);
			break;
		case (CTBNamespace::TEXT):
			m_ComboObj.SetCurSel(nPosText);
			break;	
		case (CTBNamespace::REPORT):
			m_ComboObj.SetCurSel(nPosReport);
			break;
		case (CTBNamespace::FILE):
			m_ComboObj.SetCurSel(nPosOther);
			break;
		case (CTBNamespace::PDF):
			m_ComboObj.SetCurSel(nPosPdf);
			break;	
		case (CTBNamespace::RTF):
			m_ComboObj.SetCurSel(nPosRtf);
			break;	
		//case (CTBNamespace::ODF):
		//	m_ComboObj.SetCurSel(nPosOdf);
		//	break;	
		default:
			ASSERT(FALSE);
			m_ComboObj.SetCurSel(nPosReport);
	}

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::LoadImageList()
{
	HICON	hIcon[3];
	int		n;

	m_ImageList.Create(ScalePix(16), ScalePix(16) , ILC_COLOR32, 16, 16);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());

	hIcon[0] = TBLoadImage(TBGlyph(szIconStandard));	
	hIcon[1] = TBLoadImage(TBGlyph(szIconAllUsers));		
	hIcon[2] = TBLoadImage(TBGlyph(szIconUser));	
	
	for (n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::FillObjTreeInit(CStringArray& aOwner)
{
	if (!m_TreeObj.GetImageList(TVSIL_NORMAL))
		m_TreeObj.SetImageList(&m_ImageList, TVSIL_NORMAL);

	m_aPresentObjs.RemoveAll();		
	SearchObjList(m_NameSpace.GetType(), m_aUsers, TRUE, TRUE);

	FillObjTree	 (aOwner);	
	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::FillObjTree(CStringArray& aOwner)
{
	int 		nIco			= 0;
	int 		nIcoSel 		= 0;
	BOOL		bInsert			= FALSE;
	CString 	strObjName		= _T("");
	CString		strOwnerFilter	= _T("");
	CString		sFilter			= _T("");
	HTREEITEM	hItemObj		= 0;

    BeginWaitCursor();
	m_TreeObj.SetRedraw(FALSE);
	m_TreeObj.SelectItem(NULL);
	m_TreeObj.DeleteAllItems();

	for (int u = 0; u <= aOwner.GetUpperBound(); u++)
	{
		CString sOwn = aOwner.GetAt(u);
		if (!sOwn.IsEmpty())
		{
			sFilter = sOwn;
			break;
		}		
	}

	for (int i = 0 ; i <= m_aPresentObjs.GetUpperBound(); i++)
	{	
		CTBObjDetails* aObjDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(i);	
		strObjName = aObjDetails->m_sName;

		if (aObjDetails->IsAllUser() && m_bAllUsrs)
		{
			nIco	= 1;
			nIcoSel = 4;
			
			hItemObj = m_TreeObj.InsertItem(strObjName, nIco, nIco);
			m_TreeObj.SetItemData(hItemObj, (DWORD_PTR) m_aPresentObjs.GetAt(i));
			continue;
		}

		if (aObjDetails->IsStandard() && m_bStd)
		{
			nIco	= 0;
			nIcoSel = 3;
			hItemObj = m_TreeObj.InsertItem(strObjName, nIco, nIco);
			m_TreeObj.SetItemData(hItemObj, (DWORD_PTR) m_aPresentObjs.GetAt(i));
			continue;
		}

		for (int n = 0; n <= aOwner.GetUpperBound(); n++)
		{
			strOwnerFilter = aOwner.GetAt(n);
			if (strOwnerFilter.IsEmpty())
				continue;
					
			if (
				aObjDetails->IsUser()		&& 
				strOwnerFilter.CompareNoCase(sFilter) == 0 // sFilter potrebbe anche coincidere con AfxGetLoginInfos()->m_strUserName
				)
			{
				nIco	= 2;
				nIcoSel = 5;
				hItemObj = m_TreeObj.InsertItem(strObjName, nIco, nIco);
				m_TreeObj.SetItemData(hItemObj, (DWORD_PTR) m_aPresentObjs.GetAt(i));
				bInsert = FALSE;
			}		
		}
	}

	m_TreeObj.SetRedraw(TRUE);
	m_TreeObj.Invalidate(FALSE);
	//if (m_ExplorerType == CTBExplorer::OPEN)
	//	m_Btn_Ok.EnableWindow(FALSE);
	
	EndWaitCursor();
	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::SearchObjList(CTBNamespace::NSObjectType aType, const CStringArray& aUserSel, BOOL bStd /*TRUE*/, BOOL bAllUsrs /*TRUE*/)
{
	CStringArray			aAllObjPaths;
	CStringArray			aUserSearched;
	CString					strFullName;
	CString					sSubObjName; 
	CString					sUserName	= _T(""); 
	CPathFinder::PosType	enPosType;
	BOOL					bIsPresent 	= FALSE;
	BOOL 					bUserDlg	= !IsKindOf(RUNTIME_CLASS(CTBExplorerAdminDlg));

	if (aUserSel.IsEmpty())
		aUserSearched.Copy(m_aUsers);
	//else
	//	aUserSearched.Copy(aUserSel);	

	CStringArray aAllModulesPath;
	CStringArray aAllObjInFolder;
	AfxGetPathFinder()->GetAllModulePath(aAllModulesPath, m_NameSpace, aUserSel, bStd, bAllUsrs, TRUE, TRUE, m_NameSpace.GetObjectName(CTBNamespace::MODULE));	
	AfxGetPathFinder()->GetAllObjInFolder(aAllObjPaths, m_NameSpace, aAllModulesPath);
	for (int s = 0; s <= aAllObjPaths.GetUpperBound(); s++)
	{
		strFullName = aAllObjPaths.GetAt(s);
		sSubObjName = GetNameWithExtension(strFullName);
		// invalid character into object name filter
		if (!IsValidObjName(GetName(sSubObjName)))
			continue;
		
		CTBObjDetails* pDetails = NULL;
		for (int i= 0 ; i <= m_aPresentObjs.GetUpperBound(); i++)
		{
			pDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(i);
			if (pDetails->m_sName.CompareNoCase(sSubObjName) == 0)
			{
				if (bUserDlg)
					bIsPresent = TRUE;					
				break;
			}
		}

		if (!bIsPresent)
		{
			CTBObjDetails* pObjDetails = NULL;
			enPosType = AfxGetPathFinder()->GetPosTypeFromPath(strFullName);

			if (enPosType == CPathFinder::USERS)
				sUserName = AfxGetPathFinder()->GetUserNameFromPath(strFullName);

			if (!bUserDlg)
			{
				for (int n = 0; n <= m_aPresentObjs.GetUpperBound() ; n++ )
				{
					CTBObjDetails* pDetailsAdm = (CTBObjDetails*) m_aPresentObjs.GetAt(n);
					if (pDetailsAdm->m_sName.CompareNoCase(sSubObjName) == 0)
					{
						pObjDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(n); 
						pObjDetails->AddLocation(sUserName, strFullName, enPosType);	
						break;
					}
				}
			}
			//se sono in modalità user o se non è già stato inseito il dato
			//lo creo nuovo e lo aggiungo all'array
			if (bUserDlg || pObjDetails == NULL)
			{
				
				pObjDetails = new CTBObjDetails(sSubObjName);
				pObjDetails->m_sFullName = strFullName;
				m_aPresentObjs.Add((CObject*)pObjDetails);	
				pObjDetails->AddLocation(sUserName, strFullName, enPosType);	
			
			}
		}
		bIsPresent = FALSE;
	}
}

//--------------------------------------------------------------------------
int CompareObjDetails(CObject* pObj1, CObject* pObj2)
{
	CTBObjDetails* pd1 = (CTBObjDetails*) pObj1;
	CTBObjDetails* pd2 = (CTBObjDetails*) pObj2;

	return pd1->m_sName.CompareNoCase(pd2->m_sName);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::SortObj()
{
	m_aPresentObjs.SetCompareFunction(CompareObjDetails);
	m_aPresentObjs.Sort();
}

//--------------------------------------------------------------------------
int CompareObjUser(CObject* pObj1, CObject* pObj2)
{
	int nCompare = 0;
	CTBObjDetails* pd1 = (CTBObjDetails*) pObj1;
	CTBObjDetails* pd2 = (CTBObjDetails*) pObj2;

	CStringArray* pUser1 = new CStringArray();
	pd1->GetAllOwner(pUser1);
	
	CStringArray* pUser2 = new CStringArray();
	pd2->GetAllOwner(pUser2);

	nCompare = pUser1->GetAt(0).CompareNoCase(pUser2->GetAt(0));

	delete pUser1;
	delete pUser2;

	return nCompare;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnComboObjChanged()
{
	int 						nCurSel		= m_ComboObj.GetCurSel();
    CStringArray				aUser;
	CTBNamespace::NSObjectType	aTypeObj	= (CTBNamespace::NSObjectType) m_ComboObj.GetItemData(nCurSel);

	m_NameSpace.SetType(aTypeObj);
	ApplyFilter();	
	FillObjTreeInit(m_aFilterUser);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnListSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	CString			Label			= _T("");
	BOOL			bFind			= FALSE;
	POSITION		Pos				= m_ListApplications.GetFirstSelectedItemPosition();
	CString			sName			= m_strEditTreeObj;

	while (Pos)
	{
		int nItem = m_ListApplications.GetNextSelectedItem(Pos);
		CItemNoLoc* pItemNoLoc = (CItemNoLoc*) m_ListApplications.GetItemData(nItem);
//		if (m_NameSpace.GetType() != CTBNamespace::REPORT || )
//			m_pToolBar->RemoveAllComboItems(IDC_COMBO_USER);
		Label = pItemNoLoc->m_strName;
		
		for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
		{
			CString strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
			if (strApps == Label)
				bFind = TRUE; 
		}

		if (!bFind)
			Label = _T(""); 

		m_NameSpace.SetApplicationName(Label);
		FillModsCombo(Label, TRUE);			
	}

	if (m_ExplorerType != CTBExplorer::SAVE)
		m_Btn_Ok.EnableWindow(FALSE);
	else
	{
		m_strEditTreeObj = sName;
			CButton* pCheckForStd = (CButton*)GetDlgItem(IDC_CHECK_FOR_STD);
			if (!pCheckForStd)
				return;

		if (AfxGetBaseApp()->IsDevelopment() && !IsSelectedApplicationACustomization())
		{
			pCheckForStd->ShowWindow(SW_SHOW);
		}
		else
		{
			pCheckForStd->ShowWindow(SW_HIDE);

			pCheckForStd->SetCheck(FALSE);

			m_bSaveForStandard = FALSE;
		}
	}
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::IsSelectedApplicationACustomization()
{
	//Per valutare se l'applicazione selezionata viva nella Custom
	//basta guardare al flag m_bIsCustom di un modulo qualunque.
	//Infatti un'applicazione o vive nella standard oppure vive nella custom,
	//non è ammesso abbia un pò di moduli nella standard e un pò nella custom.
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
	if (!pAddOnApp)
		return FALSE;

	AddOnModsArray* pMods = pAddOnApp->m_pAddOnModules;
	if (!pMods || pMods->GetSize() == 0)
		return FALSE;

	AddOnModule* pAddOnMod = pMods->GetAt(0);

	return pAddOnMod && pAddOnMod->m_bIsACustomization;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnItemSelectionChange(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel = m_TreeObj.GetSelectedItem();

	if (m_ExplorerType == CTBExplorer::SAVE)
	{
		if (m_bFirstFill)
		{
			m_bFirstFill = FALSE;
			m_TreeObj.SetItemState(hSel, 0, TVIS_SELECTED);

			HTREEITEM hItem = m_TreeObj.GetRootItem();
			while(hItem)
			{
				CString tag = m_TreeObj.GetItemText(hItem);
				if (tag.CompareNoCase(m_strEditTreeObj) == 0)
				{
					m_TreeObj.SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
					m_Btn_Ok.EnableWindow(TRUE);
					return;
				}
				hItem = m_TreeObj.GetNextSiblingItem(hItem);
			}
			return;
		}

		m_strEditTreeObj =  m_TreeObj.GetItemText(hSel);
		if (!m_strEditTreeObj.IsEmpty())
			m_EditObj.SetWindowText(m_strEditTreeObj);	
		m_Btn_Ok.EnableWindow(!m_strEditTreeObj.IsEmpty());
	}
	else
		m_Btn_Ok.EnableWindow(hSel != NULL);

	if (m_NameSpace.GetType() == CTBNamespace::REPORT )
	{
		CString strText = m_TreeObj.GetItemText(hSel);
		CTBObjDetails* pDetails = NULL;

		CString strResult;
		for (int i= 0 ; i <= m_aPresentObjs.GetUpperBound(); i++)
		{
			pDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(i);
			if (pDetails->m_sName.CompareNoCase(strText) == 0)
			{		

				strResult = pDetails->m_sFullName;
				break;
			}
		}
		if (strResult.IsEmpty())
			return;

		CString strTitle;
		CString strSubject;
		::GetReportTitle(strResult,strTitle,strSubject);
	}
		
	*pResult = 0;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnDoubleClickTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (m_ExplorerType == CTBExplorer::SAVE)
		return;

	CHTreeItemsArray	aHTreeItemsArray;
	m_TreeObj.GetSelectedHItems(&aHTreeItemsArray);
	if (aHTreeItemsArray.GetSize() != 1)
	{
		m_Btn_Ok.EnableWindow(FALSE);
		return;
	}

	OnOK();
	*pResult = 0;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnComboModsChanged()
{
	int	nCurSel				= m_pToolBar->GetComboItemSel(IDC_COMBO_MODS);
	CItemNoLoc* ItemNoLoc	= (CItemNoLoc*)  m_pToolBar->GetComboItemData(IDC_COMBO_MODS, nCurSel);

	CString strModulename	= ItemNoLoc->m_strName;
	m_NameSpace.SetObjectName(CTBNamespace::MODULE, strModulename);

	GetExplorerCache()->m_LastUsedNameSpace.SetObjectName(CTBNamespace::MODULE, strModulename);
	//CStringArray aUser;
	ApplyFilter();
	CString str;
	m_EditObj.GetWindowText(str);	
	FillObjTreeInit(m_aFilterUser);	
	m_EditObj.SetWindowText(str);
	if (m_ExplorerType != CTBExplorer::SAVE)
		m_Btn_Ok.EnableWindow(FALSE);

	OnBtnFilterApply();
	
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnRefresh()
{
	ApplyFilter();	
	FillObjTreeInit(m_aFilterUser);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnBtnRefreshClicked()
{
	OnRefresh();
	OnBtnFilterApply();
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnInsertObj()
{
	CString			strExt;
	CString			strAny;
	CString			strFilter;
	CStringArray	aPathForSave;
	
	switch (m_NameSpace.GetType())
	{
		case CTBNamespace::REPORT:
			strExt		= FileExtension::WRM_EXT();
			strAny		= FileExtension::ANY_WRMRDE();
			strFilter	= FileExtension::WRMRDE_FILTER();
			break;
		case CTBNamespace::IMAGE:
			strExt		= FileExtension::BMP_EXT();
			strAny		= FileExtension::ANY_BMP();
			strFilter	= FileExtension::BMP_FILTER();
			break;
		case CTBNamespace::TEXT:
			strExt		= FileExtension::CSV_EXT();
			strAny		= FileExtension::ANY_TXT();
			strFilter	= FileExtension::TXT_FILTER();
			break;
		case CTBNamespace::PDF:
			strExt		= FileExtension::PDF_EXT();
			strAny		= FileExtension::ANY_PDF();
			strFilter	= FileExtension::PDF_FILTER();
			break;
		case CTBNamespace::RTF:
			strExt		= FileExtension::RTF_EXT();
			strAny		= FileExtension::ANY_RTF();
			strFilter	= FileExtension::RTF_FILTER();
			break;
		case CTBNamespace::ODT:
			strExt		= FileExtension::ODT_EXT();
			strAny		= FileExtension::ANY_ODT();
			strFilter	= FileExtension::ODT_FILTER();
			break;
		case CTBNamespace::ODS:
			strExt		= FileExtension::ODS_EXT();
			strAny		= FileExtension::ANY_ODS();
			strFilter	= FileExtension::ODS_FILTER();
			break;
	}

	CFileDialog dlg(TRUE, strExt, strAny, (m_bCanLink ? OFN_HIDEREADONLY : OFN_HIDEREADONLY | OFN_ALLOWMULTISELECT), strFilter);

	dlg.m_pOFN->lpstrInitialDir = m_sPathForCFileDlg;
	//declare my own buffer
	CString strFileName;
	//set a limit -in character-
	int maxChar = 1000;
	//give dialog a hint
	dlg.m_ofn.lpstrFile = strFileName.GetBuffer(maxChar);
	dlg.m_ofn.nMaxFile = maxChar;

//	dlg.m
	//show dialog
	if(dlg.DoModal()  == IDOK)
	{
		strFileName.ReleaseBuffer();//release the buffer, if you will do some CString-functions
		POSITION pos = dlg.GetStartPosition();
		
		CString sName;
		while (pos != NULL)
		{
			sName = dlg.GetNextPathName(pos);
			// invalid character into object name filter
			if (!IsValidObjName(GetName(sName)))
				AfxMessageBox(_TB("A Tb name cannot contain any of the following characters:\r\n /?*\\<>:|\" \".\r\nUnable to add."), MB_OK | MB_ICONEXCLAMATION  );
			else
				aPathForSave.Add(sName);
		}
	}
	else
		strFileName.ReleaseBuffer();//Release anyway	
			
	if (aPathForSave.GetSize() > 0)
	{
		m_sPathForCFileDlg = aPathForSave.GetAt(0);
		SaveImportedObjs(aPathForSave);
		OnRefresh();
	}
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::SaveImportedObjs(CStringArray& aPathForSave)
{
	CString strUser = AfxGetLoginInfos()->m_strUserName;
	SaveObjs(aPathForSave, strUser);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnBtnFilterApply()
{
	ApplyFilter();
	FillObjTree(m_aFilterUser);
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::ApplyFilter()
{
	m_aFilterUser.RemoveAll();
	
	if (m_bUsr)		
		m_aFilterUser.Add(AfxGetLoginInfos()->m_strUserName);
	
	if (!m_bStd && !m_bAllUsrs && m_aFilterUser.IsEmpty())
	{
		m_TreeObj.SetRedraw(FALSE);
		m_TreeObj.DeleteAllItems();
		m_TreeObj.SetRedraw(TRUE);
		m_TreeObj.Invalidate(FALSE);
		return;
	}	
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::GetSelElements()
{
	CStringArray		aSelElements;
	CString				strFullFileName = _T("");
	CHTreeItemsArray	aHTreeItemsArray;
	HTREEITEM			hSel = m_TreeObj.GetRootItem();	//'tutti' gli hitem
	
	if (m_pSelElements)
		m_pSelElements->RemoveAll();
	else
		m_pSelElements = new CTreeItemSelArray(); 	

	m_TreeObj.GetSelectedStrings(&aSelElements, hSel);
	m_TreeObj.GetSelectedHItems(&aHTreeItemsArray); //stesso numero di elementi di 
	
	for (int i = 0; i <= aHTreeItemsArray.GetUpperBound(); i++)
	{
		CHTreeItem* pHTreeItem = aHTreeItemsArray.GetAt(i);
		if (!pHTreeItem || !pHTreeItem->m_hItem)
		{
			ASSERT(FALSE);
			continue;
		}
		GetSelElementsAtLevel(pHTreeItem->m_hItem);
	}
}

//--------------------------------------------------------------------------
void  CTBExplorerUserDlg::GetSelElementsAtLevel(HTREEITEM hSel)
{
	CString			strFullFileName = GetFullPathName(m_TreeObj.GetItemText(hSel));
	CTreeItemSel*	pCTreeItemSel	= new CTreeItemSel();

	pCTreeItemSel->m_strFullPathName = strFullFileName;
	m_pSelElements->Add(pCTreeItemSel);	
}

//--------------------------------------------------------------------------
CString  CTBExplorerUserDlg::GetFullPathName(const CString& sName, const CString& sUser /*= _T("")*/, CPathFinder::PosType enPosType /* USERS*/)
{	
	CString strFullFileName	= _T("");
	for (int n = 0 ; n <= m_aPresentObjs.GetUpperBound() ; n++ )
	{
		CTBObjDetails*	pDetails		= (CTBObjDetails*) m_aPresentObjs.GetAt(n);
		CTBObjLocation* pTBObjLocation	= NULL;
		
		if (pDetails->m_sName.CompareNoCase(sName) == 0)
		{	
			if (sUser.IsEmpty())
			{
				if ((m_bAllUsrs || m_bUsr))
					pTBObjLocation = pDetails->GetLocationForUserDlg();
				if (m_bStd && !pTBObjLocation)
					pTBObjLocation = pDetails->GetLocationStandard();
				if(!pTBObjLocation)
					return _T("");
				strFullFileName = pTBObjLocation->m_sFullFileName;	
				return strFullFileName;
			}
			else
			{
				//TODO gestione se admin
				CString strUser = sUser;
				pTBObjLocation = pDetails->GetLocationForSpecificUser(strUser);
				if(!pTBObjLocation)
					return _T("");
				strFullFileName = pTBObjLocation->m_sFullFileName;
				return strFullFileName;
			}
		}			
	}
	return _T("");
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::IsAllUser(const CString& sName)
{
	for (int n = 0; n <= m_aPresentObjs.GetUpperBound(); n++)
	{
		CTBObjDetails*	pDetails		= (CTBObjDetails*) m_aPresentObjs.GetAt(n);
		
		if (pDetails->m_sName.CompareNoCase(sName) == 0)
			return pDetails->IsAllUser();				
	}
	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::IsStandard(const CString& sName)
{
	for (int n = 0 ; n <= m_aPresentObjs.GetUpperBound(); n++)
	{
		CTBObjDetails*	pDetails		= (CTBObjDetails*) m_aPresentObjs.GetAt(n);
		
		if (pDetails->m_sName.CompareNoCase(sName) == 0)
			return pDetails->IsStandard();					
	}
	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::IsUser(const CString& sName)
{
	for (int n = 0 ; n <= m_aPresentObjs.GetUpperBound(); n++)
	{
		CTBObjDetails* pDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(n);

		CString strName = GetNameWithExtension(sName);
		if (pDetails->m_sName.CompareNoCase(strName) == 0)
			return pDetails->IsUser();					
	}
	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanOpen()
{
	HTREEITEM			hSel	= 0;
	CHTreeItemsArray	aHTreeItemsArray;
	m_TreeObj.GetSelectedHItems(&aHTreeItemsArray);

	if(m_bIsMultiOpen || aHTreeItemsArray.GetSize() == 1 )
		return TRUE;

	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanCutPath()
{
	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanRename()	
{
	GetSelElements();
	if (!m_pSelElements)
	{
		ASSERT(FALSE);
		return FALSE;
	}	

	if (m_pSelElements->GetSize() > 1)
		return FALSE;

	if (!CanDeleteObj(*m_pSelElements->GetAt(0)))
		return FALSE; 

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanDelete()	
{
	BOOL bDelete = TRUE;		//usato per condizione di cancellazione e rinomina
	
	GetSelElements();
	if (!m_pSelElements)
	{
		ASSERT(FALSE);
		return FALSE;
	}	

	for (int i = 0; i <= m_pSelElements->GetUpperBound() ; i++ )
	{
		if (!CanDeleteObj(*m_pSelElements->GetAt(i)))
			return FALSE; 
	}

	return bDelete;	
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanDeleteObj(const CTreeItemSel& aObj)
{
	if (AfxGetPathFinder()->IsStandardPath(aObj.m_strFullPathName))
		return FALSE;
	
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(aObj.m_strFullPathName); //metodo per conoscere il proprietario
	
	if (ePosType == CPathFinder::ALL_USERS)
		return FALSE;

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnDelete()
{
	CString strDelete	= _TB("The following objects have not been deleted:") ;
	CString strObj		= _T("");

	if (!m_pSelElements)
	{
		ASSERT(FALSE);
		return;
	}		

	for (int i = 0; i <= m_pSelElements->GetUpperBound(); i++)
	{
		CTreeItemSel* pSelElement = m_pSelElements->GetAt(i);
		if (!pSelElement)
			continue;

		if (IsUser(pSelElement->m_strFullPathName))
		{
			if (AfxMessageBox(cwsprintf(_TB("Are you sure you want to delete the object: '{0-%s}'?"), GetName(pSelElement->m_strFullPathName)), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
			{
				DWORD dwAttr = GetTbFileAttributes((LPCTSTR) pSelElement->m_strFullPathName);
				if (FILE_ATTRIBUTE_READONLY & dwAttr)
					SetFileAttributes((LPCTSTR)pSelElement->m_strFullPathName , dwAttr & !FILE_ATTRIBUTE_READONLY);
				
				if (!DeleteFile((LPCTSTR)pSelElement->m_strFullPathName))
					AfxMessageBox(cwsprintf(_TB("Failed to delete: '{0-%s}'."), GetName(pSelElement->m_strFullPathName)), MB_OK | MB_ICONEXCLAMATION);
			}
		}
	}
	OnBtnRefreshClicked();
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnRename(const CString& strSelItem)
{  
	if (!m_pSelElements)
	{
		ASSERT(FALSE);
		return;
	}			

	if (m_pSelElements->GetSize() <= 0)
	{
		ASSERT(FALSE);
		return;
	}	

	CTreeItemSel* pSelElement = m_pSelElements->GetAt(0);
	if (!pSelElement)
	{
		ASSERT(FALSE);
		return;
	}

	CString strFileName = pSelElement->m_strFullPathName;	//rinomino solo il primo oggetto della selezione
	CString strNewName	= _T("");
	if (strSelItem.IsEmpty())								//controllo validità del nome
	{
		AfxMessageBox(_TB("Enter the name of the object to rename."), MB_OK | MB_ICONEXCLAMATION  );
		return;
	}

	int		k			= strSelItem.ReverseFind(DOT_CHAR);				//-1 not found
	CString strExtOld	= GetExtension(strFileName);
	CString strExtNew	= _T("");
	if (k != -1)											//trovato il dot
	{
		strExtNew = GetExtension(strSelItem);
		if (strExtNew.CompareNoCase(strExtOld) != 0)
		{
			AfxMessageBox(_TB("Unable to rename: invalid extension."), MB_OK | MB_ICONEXCLAMATION  );
			m_TreeObj.SetItemState(m_pSelElements->GetAt(0)->m_hSel, TVIS_SELECTED, TVIS_SELECTED);
			return;
		}
		strNewName = strSelItem;
	}
	else
		strNewName = strSelItem + strExtOld;

	CString strUserName			= AfxGetPathFinder()->GetUserNameFromPath(strFileName);
	CString strCustomModulePath	= GetPath(strFileName, 0);
	CString strNewPath			= (strCustomModulePath + SLASH_CHAR + strNewName);

	if (IsUser(strFileName))
	{
		if (!IsValidObjName(strNewPath))							//contiene caratteri speciali
		{
			AfxMessageBox(_TB("A Tb name cannot contain any of the following characters:\r\n /?*\\<>:|\"$ \".\r\nUnable to rename."), MB_OK | MB_ICONEXCLAMATION  );
			return;
		}	

		if (CanOverWrite(strNewPath))
		{
			DWORD dwAttr = GetTbFileAttributes((LPCTSTR)strFileName);
			if (FILE_ATTRIBUTE_READONLY & dwAttr)
				SetFileAttributes((LPCTSTR) strFileName, dwAttr & !FILE_ATTRIBUTE_READONLY);
			
			if (!::RenameFilePath((LPCTSTR)strFileName, strNewPath, FALSE ))
			AfxMessageBox(cwsprintf(_TB("Failed to rename: '{0-%s}'."), GetName(strFileName)), MB_OK | MB_ICONEXCLAMATION);
		}		
	}
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnRenameLabel(const CHTreeItemsArray& aHTreeItemsArray)
{
	m_TreeObj.EditLabel(aHTreeItemsArray.GetAt(0)->m_hItem);
	return;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnOK()
{   	
	GetSelElements();
	if (m_ExplorerType == CTBExplorer::SAVE)
	{
		if (!OnSavePath())
			return;
		m_bSaveForAllUsrs	= FALSE;
		m_bSaveForStandard	= FALSE;
		if (IsTBWindowVisible(GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE)))
			m_bSaveInCurrentLanguage = ((CButton*)GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE))->GetCheck();
		CParsedDialog::OnOK();
		return;
	}	

	if (m_pSelElements->IsEmpty() && !m_bCanLink)
	{
		AfxMessageBox(_TB("Warning, no object has been selected!"));
		return;	
	}	

	if (m_pSelElements->GetSize() > 1 && !m_bIsMultiOpen)
	{
		AfxMessageBox(_TB("Warning: unable to multiselect."));
		return;	
	}	

	CParsedDialog::OnOK();
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::OnSavePath()
{
	CStringArray	aFileName;
	CString			strText		= _T("");
	CString			strPathFile	= _T("");

	m_pSelElements->RemoveAll();
	m_EditObj.GetWindowText(strText);

	if (m_strEditTreeObj.CompareNoCase(strText) != 0 )	
		m_strEditTreeObj = strText;

	if (strText.IsEmpty()) 
	{
		AfxMessageBox(_TB("Warning: enter object name to save."), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}
	// controllo estensione ed eventuale inserimento (di defaulf imposto la prima)
	int				k			= m_strEditTreeObj.ReverseFind(DOT_CHAR);		//-1 not found
	BOOL			bExistExt	= FALSE;
	CString			strExtNew	= _T("");
	CStringArray	aExt;
	
	
	int s = m_strEditTreeObj.FindOneOf(UNC_SLASH_CHARS);
	if (s >= 0) 
	{
		AfxMessageBox(cwsprintf(_TB("The name of the object '{0-%s}' cannot contain any of the following characters:\r\n /?*\\<>:|\" \"..\r\nUnable to rename."), GetName(strPathFile)), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	if (m_NameSpace.GetType() != CTBNamespace::PROFILE)
	{
		AfxGetPathFinder()->GetObjectSearchExtensions(m_NameSpace, &aExt, TRUE);

		if (this->m_NameSpace.GetType() == CTBNamespace::REPORT)
		{
			//TODO
		}

		if (k != -1)																//trovato il dot
		{
			strExtNew = GetExtension(m_strEditTreeObj);
			for (int m = 0 ; m < aExt.GetSize() ; m++)
			{
				if (strExtNew.CompareNoCase(aExt.GetAt(m)) == 0)
					bExistExt = TRUE;
			}

			if (!bExistExt)
			{
				AfxMessageBox(_TB("Unable to save: invalid extension."), MB_OK | MB_ICONEXCLAMATION  );
				return FALSE;
			}
		}
		else
		{
			CString strFirstExt = aExt.GetAt(0);
			m_strEditTreeObj += strFirstExt;
		}
	}

	GetObjsNameForSave(aFileName);
	for (int i = 0; i < aFileName.GetSize(); i++)
	{
		strPathFile = aFileName.GetAt(i);
		if (!IsValidObjName(strPathFile)) //contiene caratteri speciali
		{
			AfxMessageBox(cwsprintf(_TB("Name of object '{0-%s}' has not valid character.\nImpossible continue."), strPathFile), MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}

		if (CanOverWrite(strPathFile))
			OverWriteObject(strPathFile);
		else
			return FALSE;
	}

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::SaveObjs(CStringArray& aPathForSave, CString& sUserForSave)
{
	CString			strSourceFile = _T(""); 
	CString			strTargetPath = _T("");
	CString			strTargetFile = _T("");
	CStringArray	aMessage;
	BOOL			bCompleted = TRUE;
	BOOL			bSaveLink = FALSE;

	//to do link here
	if (m_bCanLink)
	{
		CAskCopyLinkDialog AskDlg;
		if (AskDlg.DoModal() == IDOK)
		{
			bSaveLink = AskDlg.m_bSaveLink;	
			if (bSaveLink)
			{
				m_strLinkFullPath = aPathForSave.GetAt(0);
				OnOK();
				return;
			}			
		}
		else
			return;
	}

	switch (m_NameSpace.GetType())
	{
		case (CTBNamespace::REPORT):
			if (IsKindOf(RUNTIME_CLASS(CTBExplorerAdminDlg)))
			{
				if (m_bStd && AfxGetBaseApp()->IsDevelopment())
				{
					//Sono in Sviluppo => se l'applicazione selezionata non è una personalizzazione EB allora tutto come prima,
					//altrimenti salvo nella custom dell'utente che sta sviluppando la customizzazione.
					//Il report dovrà poi essere pubblicato.
					if (!IsSelectedApplicationACustomization())
					{
						strTargetPath	= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::STANDARD, sUserForSave, TRUE);
						sUserForSave	= _TB("Original");
					}
					else
					{
						strTargetPath	= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
						sUserForSave	= _TB("All the Users");
					}
				}
				if (m_bAllUsrs)
				{
					strTargetPath	= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
					sUserForSave	= _TB("All the Users");
				}
				if (m_bUsr)
					strTargetPath = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::USERS, sUserForSave, TRUE);
			}
			else
				strTargetPath = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::USERS, sUserForSave, TRUE);

			if (!bSaveLink)
			{
				bCompleted = ImportCopyFile(strTargetPath, sUserForSave, &aMessage, &aPathForSave);		
				//Aggiunge i report alla customizzazione corrente
				for (int i = 0; i <= (&aPathForSave)->GetUpperBound(); i++)
				{
					strSourceFile	= (&aPathForSave)->GetAt(i);
					strTargetFile = strTargetPath + SLASH_CHAR + GetNameWithExtension(strSourceFile);
					AfxAddFileToCustomizationContext(strTargetFile);
				}
			}
			else
				return;

			break;

		case (CTBNamespace::IMAGE):
		case (CTBNamespace::TEXT):
		case (CTBNamespace::PDF):
		case (CTBNamespace::RTF):
		//case (CTBNamespace::ODF):
		case (CTBNamespace::FILE):

			if (IsKindOf(RUNTIME_CLASS(CTBExplorerAdminDlg)))
			{
				if (m_bStd && AfxGetBaseApp()->IsDevelopment())
				{
					strTargetPath	= AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::STANDARD, sUserForSave, TRUE);
					sUserForSave	= _TB("Original");
				}
				if (m_bAllUsrs)
				{
					strTargetPath	= AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
					sUserForSave	= _TB("All the Users");
				}
				if (m_bUsr)
					strTargetPath = AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::USERS, sUserForSave, TRUE);
			}
			else
				strTargetPath = AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::USERS, sUserForSave, TRUE);

			if (!bSaveLink)
				bCompleted = ImportCopyFile(strTargetPath, sUserForSave, &aMessage, &aPathForSave);		
			else
				return;
			break;
	}	

	CString	str = _T("");
	for (int s = 0; s <= aMessage.GetUpperBound(); s++)
		str += aMessage.GetAt(s) + _T("\n");
	if (str.IsEmpty())
	{
		if (bCompleted)
			AfxMessageBox(_TB("Object copy is successfully completed!"));
	}
	else
		AfxMessageBox(cwsprintf(_TB("Object copy is not successfully completed.\nFollowing files aren't copy: \n{0-%s}"), str));
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::ImportCopyFile(const CString& strTargetPath, const CString& strUsr, CStringArray* aMsg, CStringArray* aPathForSave)
{
	CString			strSourceFile	= _T(""); 
	CString			strTargetFile	= _T("");
	CString			strTargetStd	= _T(""); 
	CString			strTargetAllusr = _T("");
	BOOL			bCompleted = TRUE;

	for (int i = 0; i <= aPathForSave->GetUpperBound(); i++)
	{
		strSourceFile	= aPathForSave->GetAt(i);
        strTargetFile	= strTargetPath + SLASH_CHAR + GetNameWithExtension(strSourceFile);
		
		strTargetStd	= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::STANDARD) + SLASH_CHAR + GetNameWithExtension(strSourceFile);
		strTargetAllusr = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::ALL_USERS) + SLASH_CHAR + GetNameWithExtension(strSourceFile);
		
		if(ExistFile(strTargetStd) && m_bBtnStdPressed && IsKindOf(RUNTIME_CLASS(CTBExplorerAdminDlg)))
		{
			if (AfxMessageBox(cwsprintf(_TB("'{0-%s}' already exists in '{1-%s}'.\nDo you want to save it?"), GetNameWithExtension(strSourceFile), strUsr), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
			{
				bCompleted = FALSE;
				break;
			}

			if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
				aMsg->Add(strSourceFile);

			return bCompleted;
		}

		if(ExistFile(strTargetAllusr) && m_bBtnAllUsrPressed && IsKindOf(RUNTIME_CLASS(CTBExplorerAdminDlg)))
		{
			if (AfxMessageBox(cwsprintf(_TB("'{0-%s}' already exists for 'All Users'. \n Do you want to overwrite it with '{1-%s}'??"), GetNameWithExtension(strSourceFile), strUsr), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
			{
				bCompleted = FALSE;
				break;
			}

			if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
				aMsg->Add(strSourceFile);

			return bCompleted;
		}

		if(ExistFile(strTargetFile))
		{
			CString sUser = _T("");
			if (!strUsr.IsEmpty())
				sUser = _TB("All the Users");

			if (!strUsr.IsEmpty() && AfxMessageBox(cwsprintf(_TB("'{0-%s}' already exists in '{1-%s}'.\nDo you want to overwrite it with '{1-%s}'?"), GetNameWithExtension(strSourceFile), strUsr), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
			{
				if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
					aMsg->Add(strSourceFile);
			}
			else
				bCompleted = FALSE;
		}
		else
		{
			if(!::CopyFile(strSourceFile, strTargetFile, FALSE))
			{
				DWORD dwErr = ::GetLastError();
				aMsg->Add(strSourceFile);
				aMsg->Add(cwsprintf(_TB("LastError returns {0-%d}"), dwErr));
			}	
		}
	}
	return bCompleted;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::GetObjsNameForSave(CStringArray& aFileName)
{   
	CString strFileElement = _T("");
	
	switch(m_NameSpace.GetType())
	{
		case (CTBNamespace::REPORT):
		{
			if (m_bSaveForAllUsrs)
			{
				strFileElement = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			if (m_bSaveForStandard)
			{
				strFileElement = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::STANDARD, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			for (int i = 0; i < m_aSaveUsers.GetSize(); i++)
			{
				if (!m_aSaveUsers.GetAt(i).IsEmpty())
					strFileElement = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::USERS, m_aSaveUsers.GetAt(i), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			break;
		}
		case (CTBNamespace::IMAGE):
		case (CTBNamespace::TEXT):
		case (CTBNamespace::PDF):
		case (CTBNamespace::RTF):
		//case (CTBNamespace::ODF):
		case (CTBNamespace::FILE):
		{
			if (m_bSaveForAllUsrs)
			{
				strFileElement = AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			if (m_bSaveForStandard)
			{
				strFileElement = AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::STANDARD, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			for (int i = 0; i < m_aSaveUsers.GetSize(); i++)
			{
				if (!m_aSaveUsers.GetAt(i).IsEmpty())
					strFileElement = AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::USERS, m_aSaveUsers.GetAt(i), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			break;
		}
		case (CTBNamespace::PROFILE):
		{
			if (m_bSaveForAllUsrs)
			{
				strFileElement = AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::ALL_USERS, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			if (m_bSaveForStandard)
			{
				strFileElement = AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::STANDARD, _T(""), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			for (int i = 0; i < m_aSaveUsers.GetSize(); i++)
			{
				if (!m_aSaveUsers.GetAt(i).IsEmpty())
					strFileElement = AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::USERS, m_aSaveUsers.GetAt(i), TRUE);
				strFileElement += SLASH_CHAR + m_strEditTreeObj;
				aFileName.Add(strFileElement);	
			}
			break;
		}
	}	
}

//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::CanOverWrite(const CString& strPathFile)
{
	BOOL	bOverWrite		= TRUE;
	BOOL	bAlreadyRequest	= FALSE;
	CString strOwner		= AfxGetPathFinder()->GetUserNameFromPath(strPathFile);
	CString strFileAllUsrs;
	CString strFileStd;
	CString strFileUsr;

	switch (m_NameSpace.GetType())
	{
		case (CTBNamespace::REPORT):
		{
			strFileAllUsrs	= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::ALL_USERS) + SLASH_CHAR + m_strEditTreeObj;
			strFileStd		= AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::STANDARD) + SLASH_CHAR + m_strEditTreeObj;
			if (!strOwner.IsEmpty())
				strFileUsr = AfxGetPathFinder()->GetModuleReportPath(m_NameSpace, CPathFinder::USERS, strOwner) + SLASH_CHAR + m_strEditTreeObj;
			break;
		}
		case (CTBNamespace::IMAGE):
		case (CTBNamespace::TEXT):
		case (CTBNamespace::PDF):
		case (CTBNamespace::RTF):
		//case (CTBNamespace::ODF):
		case (CTBNamespace::FILE):
		{
			strFileAllUsrs	= AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::ALL_USERS) + SLASH_CHAR + m_strEditTreeObj;
			strFileStd		= AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::STANDARD) + SLASH_CHAR + m_strEditTreeObj;
			if (!strOwner.IsEmpty())
				strFileUsr	= AfxGetPathFinder()->GetModuleFilesPath(m_NameSpace, CPathFinder::USERS, strOwner) + SLASH_CHAR + m_strEditTreeObj;
			break;
		}
		case (CTBNamespace::PROFILE):
		{
			strFileAllUsrs	= AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::ALL_USERS) + SLASH_CHAR + m_strEditTreeObj;
			strFileStd		= AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::STANDARD) + SLASH_CHAR + m_strEditTreeObj;
			if (!strOwner.IsEmpty())
				strFileUsr	= AfxGetPathFinder()->GetDocumentExportProfilesPath(m_NameSpace, CPathFinder::USERS, strOwner) + SLASH_CHAR + m_strEditTreeObj;
			break;
		}
	}
	if (strOwner.IsEmpty())
	{
		if (m_bAllUsrs || m_bSaveForAllUsrs)
			strOwner = _TB("All the Users");
		else
			strOwner = _TB("Original");
	}

	if (m_NameSpace.GetType() == CTBNamespace::PROFILE)
	{
		if (!strFileUsr.IsEmpty() && ExistPath(strFileUsr))
		{
			bAlreadyRequest = TRUE;
			if (AfxMessageBox(cwsprintf(_TB("Warning, '{0-%s}' exists for  '{1-%s}', do you want to overwrite the existing ?"), m_strEditTreeObj, strOwner), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
				return FALSE;		
		}

		// esistenza in AllUser
		if ((m_bAllUsrs || m_bSaveForAllUsrs) && !bAlreadyRequest && ExistPath(strFileAllUsrs))
		{
			bAlreadyRequest = TRUE;
			if (AfxMessageBox(cwsprintf(_TB("Warning, '{0-%s}' exists for 'All Users', do you want to overwrite the existing ?"), m_strEditTreeObj), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
				return FALSE;		
		}

		// esistenza in Standard
		if (!bAlreadyRequest && ExistPath(strFileStd)) 
			if (AfxMessageBox(cwsprintf(_TB("Warning, object '{0-%s}' exist for 'Standard', do you want save it for '{1-%s}'?"), m_strEditTreeObj, strOwner), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
				return FALSE;	
	}
	else
	{
		if (ExistFile(strFileUsr))
		{
			bAlreadyRequest = TRUE;
			if (AfxMessageBox(cwsprintf(_TB("Warning, '{0-%s}' exists for  '{1-%s}', do you want to overwrite the existing ?"), m_strEditTreeObj, strOwner), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
				return FALSE;		
		}

		// esistenza in AllUser
		if ((m_bAllUsrs || m_bSaveForAllUsrs) && !bAlreadyRequest && ExistFile(strFileAllUsrs))
		{
			bAlreadyRequest = TRUE;
			if (AfxMessageBox(cwsprintf(_TB("Warning, '{0-%s}' exists for 'All Users', do you want to overwrite the existing ?"), m_strEditTreeObj), MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) != IDOK)
				return FALSE;		
		}
	}

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OverWriteObject(const CString& strPathFile)
{
	if (ExistFile(strPathFile)) 
	{
		DWORD dwAttr = GetTbFileAttributes(strPathFile);
		if (FILE_ATTRIBUTE_READONLY & dwAttr)
			SetFileAttributes(strPathFile, dwAttr & !FILE_ATTRIBUTE_READONLY);

		CTreeItemSel* pCTreeItemSel = new CTreeItemSel();
		pCTreeItemSel->m_strFullPathName = strPathFile;
		m_pSelElements->Add(pCTreeItemSel);
	}
	else
	{
		CTreeItemSel* pCTreeItemSel = new CTreeItemSel();
		pCTreeItemSel->m_strFullPathName = strPathFile;
		m_pSelElements->Add(pCTreeItemSel);
	}
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnCancel()
{   
	if (!IsTreeCtrlEditMessage(VK_ESCAPE))
	{
		m_pSelElements->RemoveAll();
		CParsedDialog::OnCancel();
	}
}

//--------------------------------------------------------------------------
// If the edit control of the tree view control has the input focus,
// sending a WM_KEYDOWN message to the edit control will dismiss the
// edit control.  When ENTER key was sent to the edit control, the
// parentwindow of the tree view control is responsible for updating
// the item's label in TVN_ENDLABELEDIT notification code.
//--------------------------------------------------------------------------
BOOL CTBExplorerUserDlg::IsTreeCtrlEditMessage(WPARAM KeyCode)
{
	BOOL	bValue = FALSE;
	CWnd*   pWnd = this;

	CTBMultiSelTreeExplorer* pTreeCtrl = (CTBMultiSelTreeExplorer*) pWnd->GetDlgItem(IDC_TREE_OBJ);
	if (!pTreeCtrl)
		return bValue;
	
	CWnd*  Focus = GetFocus();
	CEdit* Edit  = pTreeCtrl->GetEditControl();

	if ((CEdit *) Focus == Edit && Edit)
	{
		Edit->SendMessage(WM_KEYDOWN, KeyCode);
		bValue = TRUE;
	}
	return bValue;
}

//--------------------------------------------------------------------------
void CTBExplorerUserDlg::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
/*	BOOL b = m_Btn_Refresh.EnableWindow();
	if (b != 0)
	{
		b = FALSE;
		m_Btn_Refresh.EnableWindow(b);
	}

	if (nChar == 116 && b)
		OnRefresh();
	
	CParsedDialog::OnKeyDown(nChar, nRepCnt, nFlags);*/
}

//-----------------------------------------------------------------------------
LRESULT CTBExplorerUserDlg::OnGetWebCommandType(WPARAM wParam, LPARAM lParam)
{
	WebCommandType* type = (WebCommandType*) lParam;
	*type = WEB_UNDEFINED;
	if (wParam == IDC_BTN_INSERT)
		*type = WEB_UNSUPPORTED;
	return 1L;
}

//==========================================================================
//							CTBExplorerAdminDlg
//==========================================================================
IMPLEMENT_DYNAMIC(CTBExplorerAdminDlg, CTBExplorerUserDlg)
//--------------------------------------------------------------------------
CTBExplorerAdminDlg::CTBExplorerAdminDlg(CTBNamespace& aNameSpace, CTBExplorer::TBExplorerType aExplorerType, CTreeItemSelArray* pSelElements, BOOL bIsNew, BOOL bIsMultiOpen, BOOL bCanLink, BOOL bOnlyStdAndAllusr, BOOL bSaveForAdmin)
	: 
	CTBExplorerUserDlg(IDD_EXPLORER_ADMIN, aNameSpace, aExplorerType, pSelElements, bIsNew, bIsMultiOpen, bCanLink)
{
	CStringArray arUsersWithoutEasyLookSystem;
	arUsersWithoutEasyLookSystem.Copy(AfxGetLoginInfos()->m_CompanyUsers);
	AfxGetLoginManager()->GetCompanyUsersWithoutEasyLookSystem(arUsersWithoutEasyLookSystem);
	int userCount = arUsersWithoutEasyLookSystem.GetSize();

	for (int i = 0 ; i < userCount ; i++)
	{
		bool	bExist		= FALSE;
		CString strUsers	= arUsersWithoutEasyLookSystem.GetAt(i);
		for (int n = 0 ; n < m_aUsers.GetSize() ; n++)
		{	
			if (m_aUsers.GetAt(n).CompareNoCase(strUsers) == 0)
			{
				bExist = TRUE;
				break;
			}
		}

		if (!bExist)
			m_aUsers.Add(strUsers);

		m_bFirst			= TRUE;
		m_bFirstFill		= TRUE;
		m_bUsr				= FALSE;
		m_bAllUsrs			= FALSE;
		m_bStd				= FALSE;
		m_bBtnUsrPressed	= FALSE;
		m_bBtnAllUsrPressed = FALSE;
		m_bBtnStdPressed	= FALSE;
		m_bOnlyStdAndAllusr = bOnlyStdAndAllusr;
		m_bSaveForAdmin		= bSaveForAdmin;
	}

}

//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBExplorerAdminDlg, CTBExplorerUserDlg)
	//{{AFX_MSG_MAP(CTBExplorerAdminDlg
	ON_CBN_SELCHANGE	(IDC_COMBO_USER,		OnComboUserChanged)
	ON_BN_CLICKED		(IDC_BTN_FILTER_USR,	OnBtnFilterUsrClicked)
	ON_BN_CLICKED		(IDC_BTN_FILTER_ALLUSR,	OnBtnFilterAllUsrClicked)
	ON_BN_CLICKED		(IDC_BTN_FILTER_STD,	OnBtnFilterStdClicked)	
	ON_BN_CLICKED		(IDC_CHECK_FOR_ALLUSRS,	OnCheckAllUsrs)
	ON_BN_CLICKED		(IDC_CHECK_FOR_STD,		OnCheckStd)
	ON_NOTIFY			(TVN_SELCHANGED, IDC_TREE_OBJ, OnItemSelectionChange)
	ON_UPDATE_COMMAND_UI(IDC_BTN_FILTER_USR,			OnUpdateBtnUsr)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnUpdateBtnUsr(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bBtnUsrPressed);
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::OnInitDialog()
{

	CTBExplorerUserDlg::OnInitDialog();

	//if (m_bSaveForAdmin)
	//{
	//	ShowWindow(SW_HIDE);
	//}

	if (!m_bOnlyStdAndAllusr)
	{
		FillUserCombo();
		m_pToolBar->EnableButton(IDC_COMBO_USER, FALSE);
		m_pToolBar->SetComboItemSel(IDC_COMBO_USER, 0);
	}

	if (m_bSaveForAdmin)
		PostMessage(WM_COMMAND, IDOK);

	m_bStd =	TRUE;

	ASSERT(m_pToolBar);

	m_pToolBar->SetButtonStyleByIdc(IDC_BTN_FILTER_USR,		TBBS_PRESSED);
	m_pToolBar->SetButtonStyleByIdc(IDC_BTN_FILTER_ALLUSR,	TBBS_PRESSED);
	m_pToolBar->SetButtonStyleByIdc(IDC_BTN_FILTER_STD,		TBBS_CHECKED);
	m_pToolBar->HideButton(IDC_BTN_FILTER_USR, m_bOnlyStdAndAllusr);

	OnSelStd();
	m_TreeObj.SetFocus();
	m_EditObj.SetWindowText((LPCTSTR)m_strInitEditObj);

#ifdef DEBUG
	((CButton*)GetDlgItem(IDC_CK_SAVE_IN_CURRENT_LANGUAGE))->SetCheck(FALSE);
#endif

    return TRUE;
}

//-----------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnCustomizeToolbar()
{
	__super::OnCustomizeToolbar();
	if (!m_pToolBar)
		return;
		
	m_pToolBar->SetAutoHideToolBarButton(FALSE);
	if (!m_bOnlyStdAndAllusr)
	{
		m_pToolBar->AddComboBox(IDC_COMBO_USER, STANDARD_IMAGE_LIBRARY_NS, _T("ComboUser"), 150);
		
		CString sNs = STANDARD_IMAGE_LIBRARY_NS;
		m_pToolBar->AddButton(IDC_BTN_FILTER_USR, sNs + _T(".") + _T("FilterUsr"), TBIcon(szIconUser, TOOLBAR), _TB("User"));
	}
}

//-----------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnBtnFilterUsrClicked()
{
	m_pToolBar->SetButtonStyleByIdc(IDC_BTN_INSERT, TBBS_DISABLED);

	if (m_bBtnUsrPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, TOOLBAR), NULL);
		return;
	}
	
	m_bBtnUsrPressed = m_bUsr = !m_bBtnUsrPressed;

	m_bStd = m_bAllUsrs = FALSE;
	
	//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_USR), TBBS_CHECKED);

	CString strEdit = m_strEditTreeObj;
	FilterStdClicked	(m_bStd);
	FilterAllUsrClicked	(m_bAllUsrs);
	
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
	OnSelUsr();

	if (GetExplorerCache()->m_LastUserFiltered != 0)
		OnComboUserChanged();
}

//-----------------------------------------------------------------------------
void CTBExplorerAdminDlg::FilterUsrClicked(BOOL bPressed)
{
	m_bBtnUsrPressed = bPressed;
	if (bPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_USR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconUser, TOOLBAR), NULL);
		//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_USR), TBSTATE_PRESSED | TBSTATE_ENABLED);
		OnSelUsr();
		return;
	}

	if (!m_bOnlyStdAndAllusr)
	{
		m_pToolBar->EnableButton(IDC_COMBO_USER, FALSE);
		m_pToolBar->SetComboItemSel(IDC_COMBO_USER, 0);

		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_USR, TBSTATE_ENABLED, TBIcon(szIconUser, TOOLBAR), NULL);
		//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_USR), TBSTATE_PRESSED | TBSTATE_ENABLED);

	}
	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_USR), TBBS_PRESSED);

	ApplyFilter();
	FillObjTreeInit(m_aFilterUser);	
}

//-----------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnBtnFilterAllUsrClicked()
{
	m_pToolBar->EnableButton(IDC_BTN_INSERT, TRUE);

	if (m_bBtnAllUsrPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, TOOLBAR), NULL);
		return;
	}

	m_bBtnAllUsrPressed = m_bAllUsrs = !m_bBtnAllUsrPressed;
	m_bStd = m_bUsr	= FALSE;

	//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, TOOLBAR), NULL);
	
	
	CString strEdit = m_strEditTreeObj;
	FilterUsrClicked(m_bUsr);
	FilterStdClicked(m_bStd);
	OnSelAllUsr();
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
}

//-----------------------------------------------------------------------------
void CTBExplorerAdminDlg::FilterAllUsrClicked(BOOL bPressed)
{
	m_bBtnAllUsrPressed = bPressed;

	if (bPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_ALLUSR, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconAllUsers, TOOLBAR), NULL);
		//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_ALLUSR), TBSTATE_PRESSED | TBSTATE_ENABLED);
		OnSelAllUsr();
		return;
	}
	
	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_ALLUSR), TBBS_PRESSED);

	ApplyFilter();
	FillObjTreeInit(m_aFilterUser);	
	m_bBtnAllUsrPressed = FALSE;
}

//----------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnBtnFilterStdClicked()
{
	m_pToolBar->EnableButton(IDC_BTN_INSERT, TRUE);
	if (m_bBtnStdPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconPin, TOOLBAR), NULL);
		return;
	}

	m_bBtnStdPressed = m_bStd =	!m_bBtnStdPressed;
	m_bAllUsrs	=  m_bUsr = FALSE;
	
	CString	strEdit = m_strEditTreeObj;
	FilterUsrClicked	(m_bUsr);
	FilterAllUsrClicked	(m_bAllUsrs);
	OnSelStd();
	m_strEditTreeObj = strEdit;
	m_EditObj.SetWindowText(strEdit);
}

//----------------------------------------------------------------------------
void CTBExplorerAdminDlg::FilterStdClicked(BOOL bPressed)
{
	m_bBtnStdPressed = bPressed;
	if (bPressed)
	{
		//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_STD, TBSTATE_PRESSED | TBSTATE_ENABLED, TBIcon(szIconPin, TOOLBAR), NULL);
		//m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_STD), TBSTATE_PRESSED | TBSTATE_ENABLED);
		OnSelStd();
		return;
	}
	
	//m_pToolBar->SetButtonInfo((UINT)IDC_BTN_FILTER_STD, TBSTATE_ENABLED, TBIcon(szIconPinned, TOOLBAR), NULL);
	m_pToolBar->SetButtonStyle(m_pToolBar->FindButton((UINT)IDC_BTN_FILTER_STD), TBBS_PRESSED);

	ApplyFilter();
	FillObjTreeInit(m_aFilterUser);	
	m_bBtnStdPressed = FALSE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnCheckAllUsrs()
{
	if (((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->GetCheck())
	{
		((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->SetCheck(TRUE);
		m_bSaveForAllUsrs = TRUE;
		((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->SetCheck(FALSE);
		m_bSaveForStandard = FALSE;
		return;
	}
	((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->SetCheck(FALSE);
	m_bSaveForAllUsrs = FALSE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnCheckStd()
{
	if (((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->GetCheck())
	{
		((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->SetCheck(TRUE);
		m_bSaveForStandard = TRUE;
		((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->SetCheck(FALSE);
		m_bSaveForAllUsrs = FALSE;
		return;
	}
	((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->SetCheck(FALSE);
	m_bSaveForStandard = FALSE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::SetControlDlg()
{
	CString strTipoCaption = _T("");

	switch (m_NameSpace.GetType())
	{
		case (CTBNamespace::REPORT):
			strTipoCaption = _TB("Report");
			break;
		case (CTBNamespace::IMAGE):
			strTipoCaption =  _TB("Image");
			break;
		case (CTBNamespace::TEXT):
			strTipoCaption = _TB("Text");
			break;		
		case (CTBNamespace::PDF):
			strTipoCaption = _TB("Pdf");
			break;		
		case (CTBNamespace::RTF):
			strTipoCaption = _TB("Rtf");
			break;		
		//case (CTBNamespace::ODF):
		//	strTipoCaption = _TB("Odf");
		//	break;		
		case (CTBNamespace::FILE):
			strTipoCaption = _TB("Other");
			break;		
	}

	switch (m_ExplorerType)
	{
		case CTBExplorer::EXPLORE:
			FillObjCombo();
			m_EditObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_SALVA))->ShowWindow(SW_HIDE);
			//((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->ShowWindow(SW_HIDE);
			((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->ShowWindow(SW_HIDE);
			SetWindowText(cwsprintf(_TB("Task Builder Explore - Administrator: {0-%s}"), AfxGetLoginInfos()->m_strUserName ));
			break;
		case CTBExplorer::OPEN:
			SetWindowText(cwsprintf(_TB("Task Builder Open {0-%s} - Administrator: {1-%s}"), strTipoCaption, AfxGetLoginInfos()->m_strUserName ));
			m_EditObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_SALVA))->ShowWindow(SW_HIDE);
			m_ComboObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_OBJ))->ShowWindow(SW_HIDE);
			//((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->ShowWindow(SW_HIDE);
			((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->ShowWindow(SW_HIDE);
			m_Btn_Ok.EnableWindow(FALSE);
			break;
		case CTBExplorer::SAVE:	
			m_Btn_Ok.SetWindowText((LPCTSTR) _TB("Save"));
			SetWindowText(cwsprintf(_TB("Task Builder Save {0-%s} - Administrator: {1-%s}"), strTipoCaption, AfxGetLoginInfos()->m_strUserName ));
			m_ComboObj.ShowWindow(SW_HIDE);
			((CStatic*)GetDlgItem(IDC_STATIC_OBJ))->ShowWindow(SW_HIDE);
			m_strEditTreeObj = m_NameSpace.GetObjectName();
			SelTreeItem(m_strEditTreeObj);
			m_EditObj.SetWindowText((LPCTSTR)m_strEditTreeObj);	
			m_strInitEditObj = m_strEditTreeObj;
			((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->ShowWindow(SW_SHOW);
			if (AfxGetBaseApp()->IsDevelopment() && !IsSelectedApplicationACustomization())
			{
				((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->ShowWindow(SW_SHOW);
#ifdef DEBUG
				((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->SetCheck(TRUE);
				m_bSaveForStandard = TRUE;
#endif
			}
			break;
	}
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::FillObjTreeInit(CStringArray& aOwner)
{
	if (!m_TreeObj.GetImageList(TVSIL_NORMAL))
		m_TreeObj.SetImageList(&m_ImageList, TVSIL_NORMAL);

	m_aPresentObjs.RemoveAll();	
	ApplyFilter();
	CStringArray aUsrs;
	if (!m_bOnlyStdAndAllusr)
	{
		if (m_pToolBar->GetComboItemSel(IDC_COMBO_USER) > 0)
			aUsrs.Add(m_pToolBar->GetComboItemSelText(IDC_COMBO_USER));	
	}
	
	if (!m_bBtnUsrPressed)
		SearchObjList(m_NameSpace.GetType(), aUsrs, TRUE, TRUE);
	else
		SearchObjList(m_NameSpace.GetType(), aUsrs);
	
	FillObjTree	 (m_aFilterUser);	
	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnComboUserChanged()
{
	int				nSel		= m_pToolBar->GetComboItemSel(IDC_COMBO_USER);

	CString			strOwner	= _T("");
	CStringArray	aOwner;

	if (nSel)
	{
		ApplyFilter();
		FillObjTreeInit(m_aFilterUser);
		GetExplorerCache()->m_LastUserFiltered = nSel;
		m_pToolBar->EnableButton(IDC_BTN_INSERT, TRUE);
		return;
	}
	
	m_TreeObj.DeleteAllItems();

	m_pToolBar->EnableButton(IDC_BTN_INSERT, TRUE);
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::FillUserCombo()
{
	
	m_pToolBar->AddComboSortedItem(IDC_COMBO_USER, _TB("<No selection>"));
	for (int i = 0; i <= m_aUsers.GetUpperBound(); i++)
	{
		CString strUsers = m_aUsers.GetAt(i);
		if (!m_bAllUsrs && !m_bStd)
		{
			m_pToolBar->AddComboSortedItem(IDC_COMBO_USER, strUsers, (DWORD) i);
		}				
	}	
	m_pToolBar->SetComboItemSel(IDC_COMBO_USER, GetExplorerCache()->m_LastUserFiltered);

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnSelAllUsr()
{	
	m_pToolBar->EnableButton(IDC_COMBO_USER, FALSE);
	m_pToolBar->SetComboItemSel(IDC_COMBO_USER, 0);
	ApplyFilter();

	CString sReportName = _T("");
	m_EditObj.GetWindowText(sReportName);

	FillObjTree(m_aFilterUser);

	m_EditObj.SetWindowText(sReportName);
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnSelStd()
{
	if (!m_bOnlyStdAndAllusr)
	{
		m_pToolBar->EnableButton(IDC_COMBO_USER, FALSE);
		m_pToolBar->SetComboItemSel(IDC_COMBO_USER, 0);
	}
	ApplyFilter();
	FillObjTree(m_aFilterUser);
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnSelUsr()
{	
	m_pToolBar->EnableButton(IDC_COMBO_USER, TRUE);
	m_TreeObj.DeleteAllItems();
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::GetAllLocations(const CString& strObjName, CStringArray*	pLocation)
{
	CTBObjDetails*	aObjDetails = NULL;
	if (!pLocation)
		ASSERT(FALSE);

	if (strObjName.IsEmpty())
		ASSERT(FALSE);	

	for (int n = 0; n <= m_aPresentObjs.GetUpperBound() ; n++)
	{
		aObjDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(n);
		if (aObjDetails->m_sName.CompareNoCase(strObjName) == 0)
			break;
	}	

	aObjDetails->GetAllLocations(pLocation); 
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::FillObjTree(CStringArray& aOwner)
{
	int 			nIco		= 0;
	int 			nIcoSel		= 0;
	HTREEITEM 		hItemObj	= 0;
	HTREEITEM 		hOwner		= 0;
	
	BeginWaitCursor();
	m_TreeObj.SetRedraw(FALSE);
	m_TreeObj.SelectItem(NULL);
	m_TreeObj.DeleteAllItems();

	CTBExplorerUserDlg::SortObj();
	
	for (int i = 0 ; i <= m_aPresentObjs.GetUpperBound(); i++)
	{
		CTBObjDetails*	aObjDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(i);
		CStringArray aLocations;
		aObjDetails->GetAllLocations(&aLocations);
		for (int p = 0; p <= aLocations.GetUpperBound(); p++)
		{
			CString sLoc = aLocations.GetAt(p);
			CString sUsr = AfxGetPathFinder()->GetUserNameFromPath(sLoc);
			if (m_bAllUsrs && aObjDetails->IsAllUser(p))
			{
				nIco	= 1;
				nIcoSel = 1;
				hOwner = m_TreeObj.InsertItem(aObjDetails->m_sName, nIco, nIcoSel);
				
				continue;
			}

			if (m_bStd && aObjDetails->IsStandard(p))
			{
				nIco	= 0;
				nIcoSel = 0;
				hOwner = m_TreeObj.InsertItem(aObjDetails->m_sName, nIco, nIcoSel);
				
				continue;
			}

			if (m_bUsr && !sUsr.IsEmpty())
			{
				nIco	= 2;
				nIcoSel = 2;
				hOwner = m_TreeObj.InsertItem(aObjDetails->m_sName, nIco, nIcoSel);
			}
		}
	}
	
	m_TreeObj.ExpandAll(TVE_EXPAND);
	m_TreeObj.SetRedraw(TRUE);
	m_TreeObj.Invalidate(FALSE);
	EndWaitCursor();
	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::CanOpen()		// STESSA GESTIONE ANCHE PER IDM_CUT_PATH
{
	HTREEITEM			hSel	= 0;
	CHTreeItemsArray	aHTreeItemsArray;
	HTREEITEM			hParentExisting;
	HTREEITEM			hParent;				
	m_TreeObj.GetSelectedHItems(&aHTreeItemsArray);

	hSel = aHTreeItemsArray.GetAt(0)->m_hItem;	//mi basta il primo, la multiopen non è gestita
	
	hParent= m_TreeObj.GetParentItem(aHTreeItemsArray.GetAt(0)->m_hItem);
	for (int i = 0; i < aHTreeItemsArray.GetSize(); i++)
	{
		hParentExisting = m_TreeObj.GetParentItem(aHTreeItemsArray.GetAt(i)->m_hItem);
		if (hParentExisting != hParent)
			return FALSE;		
	}
	
	if (aHTreeItemsArray.GetSize() == 1)
        return TRUE;	
	if (m_bIsMultiOpen)
		return TRUE;
	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::CanCutPath()
{
	CHTreeItemsArray	aHTreeItemsArray;
		
	m_TreeObj.GetSelectedHItems(&aHTreeItemsArray);

	if (aHTreeItemsArray.GetSize() == 1)
        return TRUE;	
	
	return FALSE;
}

//--------------------------------------------------------------------------
// non permessa la rinomina se il livello dell'item == 0 e multiselezione
// se nn implemento resta selezionato il primo item
// e non permesso x sibling
//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::CanRename()	
{
	return CanDelete();
}

//--------------------------------------------------------------------------
// posso sempre cancellare e rinominare gli oggetti
// solo successivamente cancello o rinomino quelli nn standard
//--------------------------------------------------------------------------
BOOL CTBExplorerAdminDlg::CanDeleteObj(const CTreeItemSel& aObj)
{
	if (IsStandard(aObj.m_strFullPathName))
		if (!AfxGetBaseApp()->IsDevelopment())
			return FALSE; 

	CString			strObjName		= _T("");
	HTREEITEM		hItem;
	CStringArray*	pLocation		= new CStringArray();

	for (int i = 0 ; i < m_pSelElements->GetSize() ; i++)
	{
		hItem = m_pSelElements->GetAt(i)->m_hSel;
		strObjName = m_pSelElements->GetAt(i)->m_strFullPathName;
		if (!IsStandard(strObjName))
			pLocation->Add(strObjName);		
	}	

	int nTotStd = 0;
	int nItem = pLocation->GetSize();
	for (int n = 0; n < pLocation->GetSize(); n++)		// ora cancello
	{
		if (IsStandard(pLocation->GetAt(n)))
			nTotStd++;
	}
	
	SAFE_DELETE (pLocation);

	if (nItem == nTotStd)
		return FALSE;

	return TRUE;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::GetSelElementsAtLevel(HTREEITEM hSel)
{
	CString			sUsr			= _T("");
	if (m_aFilterUser.GetSize())
		sUsr = m_aFilterUser.GetAt(0);
	CString			strFullFileName = GetFullPathName(m_TreeObj.GetItemText(hSel), sUsr);
	CTreeItemSel*	pCTreeItemSel	= new CTreeItemSel();

	pCTreeItemSel->m_strFullPathName = strFullFileName;
	m_pSelElements->Add(pCTreeItemSel);	
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnRefresh()
{
	CStringArray	aOwner;
	if (m_bBtnUsrPressed && m_pToolBar->GetComboItemSel(IDC_COMBO_USER) == 0)
	{
		m_TreeObj.DeleteAllItems();
		return;
	}
	FillObjTreeInit(aOwner);
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnOK()
{  
	if (!IsTreeCtrlEditMessage(VK_RETURN))
	{
		if (m_ExplorerType == CTBExplorer::SAVE)
		{
			m_aSaveUsers.RemoveAll();
			CString sText = _T("");
			m_EditObj.GetWindowText(sText);
			
			if (m_bSaveForAdmin)
				ShowWindow(SW_HIDE);

			if (sText.IsEmpty())
			{
				AfxMessageBox(_TB("Warning, for save you must assign a name to object!"));
				return;
			}

			if (((CButton*)GetDlgItem(IDC_CHECK_FOR_ALLUSRS))->GetCheck())
			{			
				m_bSaveForAllUsrs = TRUE;
				CTBExplorerUserDlg::OnOK();
				return;
			}

			if (((CButton*)GetDlgItem(IDC_CHECK_FOR_STD))->GetCheck())
			{			
				CTBExplorerUserDlg::OnOK();
				m_bSaveForStandard = FALSE;
				return;
			}

			CSaveAdminDialog SaveDlg (m_aUsers, m_aSaveUsers, sText, m_bSaveForAdmin);
			if (SaveDlg.DoModal() != IDOK)
			{
				CTBExplorerUserDlg::OnCancel();
				return;
			}
			if (m_aSaveUsers.GetSize() == 0)
			{
				m_bSaveForAllUsrs = SaveDlg.m_bSaveAllUsers;
				m_bSaveForStandard = SaveDlg.m_bSaveStandard;
			}
			m_bSaveInCurrentLanguage = SaveDlg.m_bSaveCurrLanguage;

			m_EditObj.SetWindowText(sText);
		}

		CTBExplorerUserDlg::OnOK();
	}
} 

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnCutPath()
{
	CopyToClipboard(m_sStatusBar);
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::ApplyFilter()
{
	CString	strOwner	= _T("");
	m_aFilterUser.RemoveAll();
	if	(m_bUsr)
	{
		int nSel = m_pToolBar->GetComboItemSel(IDC_COMBO_USER);
		if (nSel)
		{
			CString	strOwner = m_pToolBar->GetComboItemSelText(IDC_COMBO_USER);
			m_aFilterUser.Add(strOwner);	
		}
	}
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::SaveImportedObjs(CStringArray& aPathForSave) 
{
	CString strUsr =  _T("");//m_aFilterUser.GetAt(0);
	if (m_bUsr)
		strUsr = m_aFilterUser.GetAt(0);

	SaveObjs(aPathForSave, strUsr);
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::OnBtnFilterApply()
{	
	return;
}

//--------------------------------------------------------------------------
void CTBExplorerAdminDlg::SelTreeItem(const CString& strItemName)
{
	HTREEITEM hItem = m_TreeObj.GetRootItem();
	while (hItem)
	{
		CString strFirst = m_TreeObj.GetItemText(hItem);
		if (GetName(strFirst).CompareNoCase(strItemName) == 0)
		{
			m_TreeObj.SelectItem(hItem);
			return;
		}
		hItem = m_TreeObj.GetNextSiblingItem(hItem);
	}

	HTREEITEM hItemSel = m_TreeObj.GetSelectedItem();
	if (!hItemSel)
		m_TreeObj.SetItemState(hItemSel, 0, TVIS_SELECTED);
}



void CTBExplorerAdminDlg::OnItemSelectionChange(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel = m_TreeObj.GetSelectedItem();

	CString strText = m_TreeObj.GetItemText(hSel);
	CTBObjDetails* pDetails = NULL;

	//se viene selezionato un elemento durante il save , sovrascrive al posto di woorm1 il nome file
	if (m_ExplorerType == CTBExplorer::SAVE)
	{	
		int index = strText.ReverseFind('.');
		if (index < 0) 
			return;
		strText.Insert(index, _T("1"));
		CString strAlternativeName = strText;
		m_EditObj.SetWindowText(strAlternativeName);
		CString strResult; 
		m_EditObj.GetWindowText(strResult);
		m_Btn_Ok.EnableWindow(!strAlternativeName.IsEmpty() || !strResult.IsEmpty());
		return;
	}
	else
		m_Btn_Ok.EnableWindow(hSel != NULL);

	CString strResult;
	for (int i= 0 ; i <= m_aPresentObjs.GetUpperBound(); i++)
	{
		pDetails = (CTBObjDetails*) m_aPresentObjs.GetAt(i);
		if (pDetails->m_sName.CompareNoCase(strText) == 0)
		{		

			strResult = pDetails->m_sFullName;
			m_sStatusBar = strResult;
			break;
		}
	}
	if (strResult.IsEmpty())
		return;

	if ( m_NameSpace.GetType() == CTBNamespace::REPORT)
	{
		CString strTitle;
		CString strSubject;
		
		::GetReportTitle(strResult,strTitle,strSubject);
	}
	
	*pResult = 0;
}


//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBExplorerFactoryUI, CTBExplorerFactory)

//-----------------------------------------------------------------------------
ITBExplorer* CTBExplorerFactoryUI::CreateInstance(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew /*= FALSE*/, BOOL bOnlyStdAndAllusr /*= FALSE*/, BOOL bIsUsr /*= FALSE*/, BOOL bSaveForAdmin /*= FALSE*/)
{
	return new CTBExplorer(aType,aNameSpace, bIsNew, bOnlyStdAndAllusr, (bIsUsr ? CTBExplorer::FORCE_USER : CTBExplorer::DEFAULT), bSaveForAdmin);
}
