
#include "stdafx.h"

#include <TbGeneric\array.h>

#include <TBGeneric\WndObjDescription.h>
#include <TbGenlib\oslinfo.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\commands.hrc>

#include "extdoc.h"
#include "dbt.h"
#include "browser.h"
#include "bodyedit.h"
#include "xmlgesinfo.h"
#include "ParsedPanel.h"
#include "JsonFormEngineEx.h"
#include "tabber.h"
#include "TileManager.h"
#include "TileDialog.h"

#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "FormMng.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//===========================================================================

/////////////////////////////////////////////////////////////////////////////
//					class CTabDlgEmpty implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CTabDlgEmpty, CTabDialog)

/////////////////////////////////////////////////////////////////////////////
//					class CTabDialog implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTabDialog, CBaseTabDialog)
	
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTabDialog, CBaseTabDialog)
	//{{AFX_MSG_MAP(CTabDialog)
	ON_WM_SIZE()
	ON_WM_RBUTTONDOWN()
	ON_WM_DESTROY()
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)

	//}}AFX_MSG_MAP
	
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTabDialog::CTabDialog(const CString& sName, UINT nIDD)
	: 
	CBaseTabDialog		(sName, nIDD),
	m_pFormView			(NULL),
	m_pParentTabManager	(NULL),
	m_pChildTabManagers	(NULL),
	m_pTileGroup		(NULL)
{
}

//-----------------------------------------------------------------------------
CTabDialog::~CTabDialog()
{	
	ASSERT_VALID(this);
	//rimosso per problematiche legate al test manager
	//ASSERT(m_pDlgInfo);
	//if (m_pDlgInfo)
	//	m_pDlgInfo->ClearParsedControl();

	SAFE_DELETE (m_pChildTabManagers)
	SAFE_DELETE (m_pTileGroup);
#ifdef _DEBUG
	if (m_pParentTabManager && m_pParentTabManager->m_bCrtCheckMemoryFailed)
	{
		//già segnalato
	}
	else
	{
		BOOL bFail = ! _CrtCheckMemory();
		if (bFail)
		{
			ASSERT_TRACE(FALSE, cwsprintf(_T("Memory corrupted detected in %s"), CString(this->GetRuntimeClass()->m_lpszClassName)));
			if (m_pParentTabManager)
				m_pParentTabManager->m_bCrtCheckMemoryFailed = TRUE;
		}
	}
#endif
}

//------------------------------------------------------------------------------
void CTabDialog::OnDestroy()
{
	CAbstractFormDoc* pDoc = GetDocument();
	if (pDoc)
		pDoc->OnDestroyTabDialog(this);

	SyncExternalControllerInfo(TRUE);
	
	__super::OnDestroy();
}

//------------------------------------------------------------------------------
BOOL CTabDialog::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	this->GetClientRect(rclientRect);

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame && pFrame->IsLayoutSuspended())
	{
		CWnd* pCtrl = this->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			CRect screen;
			pCtrl->GetWindowRect(&screen);
			this->ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}
	}

	pDC->FillRect(&rclientRect, GetBackgroundBrush());

	return TRUE;
}

//------------------------------------------------------------------------------
CSize MaxBottomRightControlPos(CWnd* pWnd)
{
	CSize	s(0,0);
	CRect	rcChild;	
	
	//salvo in un array i rettangoli di tutte le child wnd e in un altro array il puntatore alla finestra
	CWnd* pwndChild = pWnd->GetWindow (GW_CHILD);
	while (pwndChild)
	{
		if (pwndChild->m_hWnd != pWnd->m_hWnd)
		{

			pwndChild->GetWindowRect (rcChild);
			(pWnd->GetParent())->ScreenToClient (rcChild);
			
			if 	(
					dynamic_cast<ResizableCtrl*>(pwndChild) 
					/*
					dynamic_cast<CGridControlObj*>(pwndChild) ||
					pwndChild->IsKindOf(RUNTIME_CLASS(CResizableTreeCtrl)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CShowFileTextStatic)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CParsedRichCtrl)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CParsedWebCtrl)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CPictureStatic)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CResizableStrEdit)) || 
					pwndChild->IsKindOf(RUNTIME_CLASS(CResizableStrStatic)) ||
					pwndChild->IsKindOf(RUNTIME_CLASS(CTreeViewAdvCtrl)) ||
					pwndChild->IsKindOf(RUNTIME_CLASS(CGanttCtrl))
					*/
				)
			{
				if (s.cx < (rcChild.left + 30))  s.cx = (rcChild.left + 30);
				if (s.cy < (rcChild.top + 30))  s.cy = (rcChild.top + 30);
			}
			else
			{
				if (s.cx < rcChild.right)  s.cx = rcChild.right;
				if (s.cy < rcChild.bottom)  s.cy = rcChild.bottom;
			}
		}
		pwndChild = pwndChild->GetNextWindow();
	}
	return s;
}


//-----------------------------------------------------------------------------
CJsonContextObj* CTabDialog::GetJsonContext()
{
	return (CJsonContextObj*)m_pDlgInfo->GetJsonContext();
}
//------------------------------------------------------------------------------
BOOL CTabDialog::OnInitDialog ()
{
	__super::OnInitDialog ();
	m_nID = m_pDlgInfo->GetDialogID();
#ifdef _DEBUG
	if (m_pParentTabManager)
	{
		for (int i = 0; i < m_pParentTabManager->GetDlgInfoArray()->GetCount(); i++)
		{
			DlgInfoItem* pInfo = m_pParentTabManager->GetDlgInfoArray()->GetAt(i);
			if (m_pDlgInfo == pInfo) continue;
			ASSERT_TRACE(m_pDlgInfo->GetNamespace() != pInfo->GetNamespace(), _T("Found duplicate tab name:") + pInfo->GetNamespace().GetObjectName() + _T(" Title:") + pInfo->m_strTitle + '\n')
		}
	}
#endif

	BuildDataControlLinks();
	CustomizeExternal();

	// possibilità ai clientdoc di intervenire sulla BuildDataControlLinks
	GetDocument()->OnBuildDataControlLinks(this);

	if (!GetParent()->IsKindOf(RUNTIME_CLASS(CTileManager)) && m_pTileGroup)
	{
		if (!GetDocument() || GetDocument()->GetDesignMode() != CBaseDocument::DM_RUNTIME)
			m_pTileGroup->SetSuspendResizeStaticArea(FALSE);
		m_pTileGroup->ResizeStaticArea();
	}
	
	//TODO le CRowFormView DEVONO allineare anche m_DataToCtrlMap valorizzato nelle AddLink
	//if (!IsKindOf(RUNTIME_CLASS(CRowTabDialog)))
	//	m_pControlLinks->AlignToTabOrder (this);	

	// chiama quella locale alla view per dare la possibilita' locale
	// di chiamare hotlink e tblreader non aggiornati quando la view non
	// e' aperta (ad esempio slaveview)	
	PrepareAuxData();
	
	// Abilita i controls sulla base dello stato del documento
	EnableTabDialogControls();
	OnFindHotLinks();
	OnUpdateControls();
	OnResetDataObjs();

#ifdef _DEBUG
	if (m_pParentTabManager)
	{
		CSize csTab = MaxBottomRightControlPos(this);
		CRect rTab;
		GetClientRect(rTab);
			
		//lascio qualche pixel apposta
		BOOL isTabHuge =
			csTab.cx > max(m_pParentTabManager->m_BornSize.cx, 1022) ||
			csTab.cy > (m_pParentTabManager->m_BornSize.cy - (m_pParentTabManager->GetNormalTabber() ? m_pParentTabManager->GetNormalTabber()->GetTabsHeight() : 0));

		//ASSERT_TRACE (!isTabHuge, _T("TabDialog's resorce is greater than its Tabber\n"));
		
		if (isTabHuge)
		{
			TRACE(cwsprintf(_T("TabManager [class:%s,IDC:%d] born with size (%d,%d): child TabDialog [class:%s,IDD:%d] has size (%d,%d)\n"),
					(LPCTSTR)CString(m_pParentTabManager->GetRuntimeClass()->m_lpszClassName),
					m_pParentTabManager->GetDlgCtrlID(),
					m_pParentTabManager->m_BornSize.cx, m_pParentTabManager->m_BornSize.cy,
					(LPCTSTR) CString(GetRuntimeClass()->m_lpszClassName),
					this->GetDialogID(),
					rTab.Width(), rTab.Height()
				));
		}
	}
#endif	

	// aggiornamento del disegno dei controls subito dopo la fine della 
	// BuildDataCtrlLinks, quando tutti i bottoni richiesti sono stati dichiarati
	m_pParentTabManager->RepositionCurrentDlg();

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// NON HO CAPITO A COSA SERVA STA ROBA QUI SOTTO VISTO CHE E' STATA GIA' FATTA SOPRA !!! Germano 07/08/2015 12:08
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//OnUpdateControls();					// 2^ VOLTA !!
	//
	//// in caso si tratti di batch è necessario reinviare ai bottoni appena nati il messaggio di Enable/Disable
	//if (GetDocument()->GetFormMode() == CBaseDocument::BROWSE && GetDocument()->GetType() == VMT_BATCH)
	//{
	//	EnableTabDialogControls();			// 2^ VOLTA !!
	//	GetDocument()->UpdateDataView();	// 3^ VOLTA !!
	//}
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	//se il documento e' predisposto per le autoexpression inizializzo i controlli
	//che hanno un/espressione associata
	if (GetDocument()->m_pAutoExpressionMng && GetDocument()->m_pVariablesArray)
	{
		for (int i= 0; i < GetDocument()->m_pAutoExpressionMng->GetSize(); i++)
		{
			for(int n = 0 ; n < GetDocument()->m_pVariablesArray->GetSize() ; n++)
			{
				if(m_pControlLinks)
				{
					if(GetDocument()->m_pVariablesArray->GetAt(n)->GetName().CompareNoCase(GetDocument()->m_pAutoExpressionMng->GetAt(i)->GetVarName()) == 0)
					{
						for(int m = 0 ; m < m_pControlLinks->GetSize() ; m++)
						{
							if(m_pControlLinks->GetAt(m))
							{
								CParsedCtrl* pControl = GetParsedCtrl(m_pControlLinks->GetAt(m));

								if(pControl && pControl->GetCtrlData() && pControl->GetCtrlData() == GetDocument()->m_pVariablesArray->GetAt(n)->GetDataObj())
								{
									SetControlAutomaticExpression(
										GetDocument()->m_pVariablesArray->GetAt(n)->GetDataObj(), 
										GetDocument()->m_pAutoExpressionMng->GetAt(i)->GetExpression()
										);
									GetDocument()->m_pAutoExpressionMng->EvaluateExpression(GetDocument()->m_pAutoExpressionMng->GetAt(i)->GetExpression(), GetDocument()->m_pVariablesArray->GetAt(n)->GetDataObj());
								}
							}
						}
					}
				}
			}
		}
	}
	
	// deve ritornare FALSE poiche`, se inserita in una AbstractFormView,
	// e` quest'ultima che decide a chi dare il fuoco e non si deve
	// ASSOLUTAMENTE lasciare che il DialogManager decida di dare il fuoco
	// al primo control della dialog (cfr. help di MFC) che potrebbe avere
	// inizialmente un valore non valido intercettato dalla
	// ParsedCtrl::DoKillFocus sulla perdita del fuoco del control stesso
	// per darlo a quello imposto dalla AbstractFormView::SetDefaultFocus()
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTabDialog::AttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber)
{
	ASSERT_VALID(pView);
	ASSERT(IsKindOf(RUNTIME_CLASS(CRowTabDialog)) == pView->IsKindOf(RUNTIME_CLASS(CRowFormView)));
	
	m_pFormView = pView;
	AttachDocument(pDoc);
	m_pParentTabManager = pTabber;

	OnBeforeAttachParents(pDoc, pView, pTabber);

	GetDlgInfoItem()->GetNamespace().SetChildNamespace(CTBNamespace::TABDLG, m_sName, pTabber->GetNamespace());
	GetDlgInfoItem()->GetInfoOSL()->m_pParent = pTabber->GetInfoOSL();

	GetDlgInfoItem()->GetInfoOSL()->SetDefaultGrant();

	*(GetInfoOSL()) = *(GetDlgInfoItem()->GetInfoOSL());

	OnAttachParents(pDoc, pView, pTabber);
}

//-----------------------------------------------------------------------------
void CTabDialog::AttachParents(CAbstractFormDoc* pDoc, CParsedDialog* pParentDialog, CTabManager* pTabber)
{
	ASSERT_VALID(pParentDialog);

	m_pFormView = NULL;
	AttachDocument(pDoc);
	m_pParentTabManager = pTabber;

	GetDlgInfoItem()->GetNamespace().SetChildNamespace(CTBNamespace::TABDLG, m_sName, pTabber->GetNamespace());
	GetDlgInfoItem()->GetInfoOSL()->m_pParent = pTabber->GetInfoOSL();

	GetDlgInfoItem()->GetInfoOSL()->SetDefaultGrant();

	*(GetInfoOSL()) = *(GetDlgInfoItem()->GetInfoOSL());
}

//-----------------------------------------------------------------------------
BOOL CTabDialog::PrepareAuxData()
{
	// ruota l'azione alle eventuali TabManager presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY( pTab = m_pChildTabManagers->GetActiveDlg(i) );
			if ( ! pTab || ! pTab->m_hWnd) continue;

			if ( ! pTab->PrepareAuxData() )
				return FALSE;
		}

	m_pControlLinks->OnPrepareAuxData();

	if (m_pTileGroup && !m_pTileGroup->PrepareAuxData())
	{
		return FALSE;
	}

	OnPrepareAuxData();

	if (GetDocument()->IsExternalControlled())
		SyncExternalControllerInfo(FALSE);

	GetDocument()->OnPrepareAuxData(this);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTabDialog::SyncExternalControllerInfo(BOOL bSave)
{
	if (bSave)
		GetDocument()->RetrieveControlData(m_pControlLinks);
	else
		GetDocument()->ValorizeControlData(m_pControlLinks, TRUE);

	//TODO
	//this->m_pTileGroup;

	if (this->m_pTileGroup == NULL)
		return;

		for (int ii = 0; ii < this->m_pTileGroup->GetTileDialogs()->GetSize(); ii++)
		{

			CTileDialog* dialog = dynamic_cast<CTileDialog*>(this->m_pTileGroup->GetTileDialogs()->GetAt(ii));
			ASSERT_VALID(dialog);

			if (bSave)
				dialog->GetDocument()->RetrieveControlData(dialog->GetControlLinks());
			else
				dialog->GetDocument()->ValorizeControlData(dialog->GetControlLinks(), FALSE);

		}

		for (int j = 0; j < this->m_pTileGroup->GetTilePanels()->GetSize(); j++)
		{
			CTilePanel* tilePanel = dynamic_cast<CTilePanel*>(this->m_pTileGroup->GetTilePanels()->GetAt(j));
			ASSERT(tilePanel);
		}

}


//-----------------------------------------------------------------------------
CTBPropertyGrid* CTabDialog::AddLinkPropertyGrid(UINT nIDC, CString	sName, CRuntimeClass*	pRuntimeClass /*NULL*/)
{
	return ::AddLinkPropertyGrid(::GetParsedForm(this), this, m_pControlLinks, nIDC, sName, pRuntimeClass);
}

//-----------------------------------------------------------------------------
void CTabDialog::EnableTabDialogControlLinks (BOOL bEnable /* = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CInfoOSL* pInfoOSL = m_pDlgInfo->GetInfoOSL();
	switch (GetDocument()->GetFormMode())
	{
			case CBaseDocument::NEW:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_NEW) == 0);
				break;

			case CBaseDocument::EDIT:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_EDIT) == 0);
				break;
	}

	::EnableControlLinks (m_pControlLinks, bEnable, bMustSetOSLReadOnly);

	if (GetDocument()->GetType() == VMT_BATCH || GetDocument()->GetType() == VMT_FINDER)
	{
		OnDisableControlsForBatch();
		OnDisableControlsAlways();
	}
	else switch (GetDocument()->GetFormMode())
	{
			case CBaseDocument::NEW:
				OnDisableControlsForAddNew();
				OnDisableControlsAlways();
				break;

			case CBaseDocument::EDIT:
				OnDisableControlsForEdit();
				OnDisableControlsAlways();
				break;

			case CBaseDocument::FIND:
				OnEnableControlsForFind();
				break;
	}

	if (GetDocument()->GetFormMode() != CBaseDocument::BROWSE)
		GetDocument()->m_pClientDocs->OnDisableControlsAlways(this);

	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY( pTab = m_pChildTabManagers->GetActiveDlg(i) );
			if(pTab && pTab->m_hWnd)
				pTab->EnableTabDialogControlLinks(bEnable, bMustSetOSLReadOnly);
		}
	if (m_pTileGroup)
	{
		m_pTileGroup->EnableViewControlLinks(bEnable, bMustSetOSLReadOnly); 
	}
}
		
// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CTabDialog::GetWndLinkedCtrl(UINT nIDC)
{   
	CWnd* pWnd = ::GetWndLinkedCtrl(m_pControlLinks, nIDC);
	if (pWnd) 
		return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
	{
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pWnd = m_pChildTabManagers->GetActiveDlg(i)->GetWndLinkedCtrl(nIDC);
			if (pWnd) return pWnd;
		}
	}

	if (m_pTileGroup)
	{
		if (m_pTileGroup->GetDlgCtrlID() == nIDC)
			return m_pTileGroup;

		for (int j = 0; j <= m_pTileGroup->GetTileDialogs()->GetUpperBound(); j++)
		{
			pWnd = m_pTileGroup->GetTileDialogs()->GetAt(j)->GetWndLinkedCtrl(nIDC);
			if (pWnd) 
				return pWnd;
		}
		for (int j = 0; j <= m_pTileGroup->GetTilePanels()->GetUpperBound(); j++)
		{
			CTilePanel* pTilePanel = m_pTileGroup->GetTilePanels()->GetAt(j);
			if (pTilePanel->GetDlgCtrlID() == nIDC)
				return pTilePanel;
			pWnd = pTilePanel->GetWndLinkedCtrl(nIDC);
			if (pWnd)
				return pWnd;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* CTabDialog::GetWndLinkedCtrl(const CTBNamespace& aNS)
{   
	CWnd* pWnd = ::GetWndLinkedCtrl(m_pControlLinks, aNS);
	if (pWnd) return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pWnd = m_pChildTabManagers->GetActiveDlg(i)->GetWndLinkedCtrl(aNS);
			if (pWnd) return pWnd;
		}
	
	return NULL;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CBodyEdit* CTabDialog::GetBodyEdits (int* pnStartIdx/* = NULL*/)
{   
	CBodyEdit* pWnd = ::GetBodyEdits(m_pControlLinks, pnStartIdx);
	if (pWnd) return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pWnd = m_pChildTabManagers->GetActiveDlg(i)->GetBodyEdits(pnStartIdx);
			if (pWnd) return pWnd;
		}

	if (m_pTileGroup)
	{
		pWnd = m_pTileGroup->GetBodyEdits(pnStartIdx);
		if (pWnd) 
			return pWnd;
	}
	
	return NULL;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CBodyEdit* CTabDialog::GetBodyEdits(const CTBNamespace& aNS)
{   
	CBodyEdit* pWnd = ::GetBodyEdits(m_pControlLinks, aNS);
	if (pWnd) return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pWnd = m_pChildTabManagers->GetActiveDlg(i)->GetBodyEdits(aNS);
			if (pWnd) return pWnd;
		}
	if (m_pTileGroup)
	{
		pWnd = m_pTileGroup->GetBodyEdits(aNS);
		if (pWnd) 
			return pWnd;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog:: GetLinkedParsedCtrl(UINT nIDC)
{
	CParsedCtrl* pParsedCtrl = ::GetLinkedParsedCtrl(m_pControlLinks, nIDC);
	if (pParsedCtrl) return pParsedCtrl;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pParsedCtrl = m_pChildTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(nIDC);
			if (pParsedCtrl) return pParsedCtrl;
		}

	if (m_pTileGroup)
	{
		pParsedCtrl = m_pTileGroup->GetLinkedParsedCtrl(nIDC);
		if (pParsedCtrl) 
			return pParsedCtrl;	
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog:: GetLinkedParsedCtrl(DataObj* pDataObj)
{
	CParsedCtrl* pParsedCtrl = ::GetLinkedParsedCtrl(m_pControlLinks, pDataObj);
	if (pParsedCtrl) return pParsedCtrl;
			
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pParsedCtrl = m_pChildTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(pDataObj);
			if (pParsedCtrl) return pParsedCtrl;
		}
	
	if (m_pTileGroup)
	{
		pParsedCtrl = m_pTileGroup->GetLinkedParsedCtrl(pDataObj);
		if (pParsedCtrl) 
			return pParsedCtrl;	
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog:: GetLinkedParsedCtrl(const CTBNamespace& aNS)
{
	CParsedCtrl* pParsedCtrl = ::GetLinkedParsedCtrl(m_pControlLinks, aNS);
	if (pParsedCtrl) return pParsedCtrl;
			
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pParsedCtrl = m_pChildTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(aNS);
			if (pParsedCtrl) return pParsedCtrl;
		}
	
	if (m_pTileGroup)
	{
		pParsedCtrl = m_pTileGroup->GetLinkedParsedCtrl(aNS);
		if (pParsedCtrl) 
			return pParsedCtrl;	
	}
	return NULL;
}



// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CTabDialog::GetWndCtrl(UINT nIDC)
{   
	CWnd* pWnd = GetDlgItem(nIDC);
	if (pWnd) return pWnd;
	
	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pWnd = m_pChildTabManagers->GetActiveDlg(i)->GetWndCtrl(nIDC);
			if (pWnd) return pWnd;
		}

	if (m_pTileGroup)
	{
		m_pTileGroup->GetWndCtrl(nIDC);
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CBaseTabManager* CTabDialog::GetTabber (UINT nIDC)
{
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pTabManager = m_pChildTabManagers->GetAt(i);
			if (pTabManager->GetDlgCtrlID() == nIDC)
				return pTabManager;

			CBaseTabManager* pChildTabManager = NULL;
			if (pTabManager->GetActiveDlg())
				pChildTabManager = pTabManager->GetActiveDlg()->GetTabber(nIDC);
			
			if (pChildTabManager)
				return pTabManager;
		}

	return NULL;
}

//-----------------------------------------------------------------------------
CBaseTileGroup*	CTabDialog::GetChildTileGroup() const
{ 
	return m_pTileGroup; 
}

//-----------------------------------------------------------------------------
CBaseTabManager* CTabDialog::GetChildTabManager	() const
{
	if (m_pChildTabManagers && m_pChildTabManagers->GetCount() > 0)
	{
		return m_pChildTabManagers->GetAt(0);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CTabDialog::MoveControls(CSize offset)
{
	::MoveControls(this, offset, m_pControlLinks);
}

//-----------------------------------------------------------------------------
BOOL CTabDialog::SetControlValue(UINT nIDC, const DataObj& val)
{
	return m_pControlLinks->SetControlValue(nIDC, val);
}

//------------------------------------------------------------------------------
void CTabDialog::OnUpdateControls(BOOL bParentIsVisible)
{
	__super::OnUpdateControls(bParentIsVisible);

	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY( (pTab = m_pChildTabManagers->GetActiveDlg(i)) != NULL );
			if (pTab && pTab->m_hWnd)
				pTab->OnUpdateControls(bParentIsVisible);
		}
	if (m_pTileGroup)
	{
		m_pTileGroup->OnUpdateControls(bParentIsVisible); 
	}
}

//------------------------------------------------------------------------------
void CTabDialog::OnFindHotLinks()
{
	::OnFindHotLinks(m_pControlLinks);

	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY((pTab = m_pChildTabManagers->GetActiveDlg(i)) != NULL);
			if (pTab && pTab->m_hWnd)
				pTab->OnFindHotLinks();
		}
	if (m_pTileGroup)
	{
		m_pTileGroup->OnFindHotLinks();
	}
}
//------------------------------------------------------------------------------
void CTabDialog::OnResetDataObjs()
{
	::OnResetDataObjs(m_pControlLinks);

	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY( pTab = m_pChildTabManagers->GetActiveDlg(i) );
			if (pTab && pTab->m_hWnd)
				pTab->OnResetDataObjs();
		}

	if (m_pTileGroup)
	{
		m_pTileGroup->OnResetDataObjs(); 
	}
}

//-----------------------------------------------------------------------------
CTabManager* CTabDialog::AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	CTabManager* pTabManager = NULL;

	// already addlinked
	if (m_pChildTabManagers)
		for (int i=0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			pTabManager = m_pChildTabManagers->GetAt(i);
			if (pTabManager && pTabManager->GetDlgCtrlID() == nIDC)
				return pTabManager;
		}

	pTabManager = (CTabManager*) __super::AddBaseTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);

	if (!m_pChildTabManagers)
		m_pChildTabManagers = new TabManagers;

	m_pChildTabManagers->Add(pTabManager);
	
	if (m_pLayoutContainer)
		m_pLayoutContainer->AddChildElement(pTabManager);

	if  (!bCallOnInitialUpdate)
		pTabManager->ClearActiveDlg();

	return pTabManager;
}

//-----------------------------------------------------------------------------
CTileGroup* CTabDialog::AddTileGroup(
	UINT nIDC, 
	CRuntimeClass* pClass, 
	const CString& sName, 
	BOOL bCallOnInitialUpdate /*TRUE*/, 
	TileGroupInfoItem* pDlgInfoItem /*= NULL*/,
	CRect rectWnd /*= CRect(0, 0, 0, 0)*/)
{
	m_pTileGroup = (CTileGroup*) __super::AddBaseTileGroup(nIDC, pClass, sName, bCallOnInitialUpdate, pDlgInfoItem, rectWnd);
	return m_pTileGroup;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog::AddLink
							(
								UINT			nIDC, 
								const CString&	sName,
								SqlRecord*		pRecord, 
								DataObj*		pDataObj, 
								CRuntimeClass*	pParsedCtrlClass,
								CString			sNsHotKeyLink,
								UINT			nBtnID /*= BTN_DEFAULT*/
							)
{
	CTBNamespace nsHKL(CTBNamespace::HOTLINK, sNsHotKeyLink); 
	CRuntimeClass* pControlClass = NULL;
	HotKeyLink*		pHotKeyLink = (HotKeyLink*) AfxGetTbCmdManager()->RunHotlink(nsHKL, NULL, &pControlClass);

	CRuntimeClass* pPCClass = pParsedCtrlClass;
	if (pControlClass == NULL) 
		pPCClass = pParsedCtrlClass;
	else if (pParsedCtrlClass == NULL) 
		pPCClass = pControlClass;
	else if (pControlClass->IsDerivedFrom(pParsedCtrlClass))
		pPCClass = pControlClass;
	else
	{
		TRACE(_T("Incompatible runtime class for parsed control: hotlink has rtc named %s but AddLink has rtc named %s\n"), pControlClass->m_lpszClassName, pParsedCtrlClass->m_lpszClassName);
		//ASSERT(FALSE);
	}
	
	CParsedCtrl* pCtrl = this->AddLink(nIDC, sName, pRecord, pDataObj, pPCClass, pHotKeyLink, nBtnID);

	if (pCtrl && pHotKeyLink)
	{
		if (pHotKeyLink != pCtrl->GetHotLink())
			delete pHotKeyLink;
		else
			pCtrl->SetOwnHotKeyLink(TRUE);
	}
	return pCtrl;
}
//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog::AddLink
							(
								UINT			nIDC, 
								const CString&	sName,
								SqlRecord*		pRecord, 
								DataObj*		pDataObj, 
								CRuntimeClass*	pParsedCtrlClass,
								HotKeyLink*		pHotKeyLink /*= NULL*/,
								UINT			nBtnID /*= BTN_DEFAULT*/
							)
{
	if (sName.IsEmpty())
	{
		TRACE("CTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CParsedCtrl* pCtrl = ::AddLink
				(
					sName,
					this, 
					m_pControlLinks, 
					nIDC, 
					pRecord, 
					pDataObj, 
					pParsedCtrlClass,
					pHotKeyLink, 
					nBtnID
				);

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CExtButton*	CTabDialog::AddLink
							(
								UINT			nIDC, 
								const CString&	sName,
								SqlRecord*		pRecord,
								DataObj*		pDataObj
							)
{
	if (sName.IsEmpty())
	{
		TRACE("CTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CExtButtonExtendedInfo* pExtInfo = new CExtButtonExtendedInfo(this, GetInfoOSL());
	CExtButton* pBtn = ::AddLink
							(
								sName,
								pExtInfo, 
								m_pControlLinks, 
								nIDC, 
								pRecord, 
								pDataObj
							);

	if (pBtn && pBtn->IsKindOf(RUNTIME_CLASS(CParsedButton)) && pDataObj)
	{
		CParsedButton* pCtrl = (CParsedButton*) pBtn;

		SetChildControlNamespace(sName, pCtrl);

		delete pExtInfo;
	}
	return pBtn;
}
							
//-----------------------------------------------------------------------------
CBodyEdit* CTabDialog::AddLink
							(
								UINT				nIDC, 
								DBTSlaveBuffered*	pDBT, 
								CRuntimeClass*		pBodyEditClass,
								CRuntimeClass*		pRowFormViewClass,
								CString				strRowFormViewTitle,
								CString				sName 
							)
{
	CBodyEdit* pBody = ::AddLink
							(	
								this, 
								this,
								m_pControlLinks, 
								nIDC, 
								pDBT, 
								pBodyEditClass,
								pRowFormViewClass, 
								strRowFormViewTitle,
								sName
							);

	Register(pBody);
	return pBody;
}

//-----------------------------------------------------------------------------
CTBGridControl* CTabDialog::AddLinkGrid
							(
								UINT				nIDC, 
								DBTSlaveBuffered*	pDBT, 
								CRuntimeClass*		pGridControlClass,
								CString				sTitle,
								CString				sName
							)
{
	CTBGridControl* grid = ::AddLinkGridInternal
		(
			this,
			this, 
			m_pControlLinks, 
			nIDC,
			pDBT,
			pGridControlClass,
			sName
		);

	//Register(pBody);
	return grid;
}


//-----------------------------------------------------------------------------
CParsedPanel* CTabDialog::AddLink
	(
				UINT			nIDC, 
				CRuntimeClass*	pParsedPanelClass, 
				CObject*		pPanelOwner,
		const	CString&		sName, 
		const	CString&		sCaption, 
				BOOL			bCallOnInitialUpdate /*TRUE*/
	)
{
	return ::AddLink
		(
			this,
			m_pControlLinks,
			nIDC, 
			pParsedPanelClass, 
			pPanelOwner,
			sName, 
			sCaption, 
			bCallOnInitialUpdate
	);
}

//-----------------------------------------------------------------------------
CBodyEdit* CTabDialog::AddLinkAndCreateBodyEdit
	(
		CRect				rect,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass /*= NULL*/,
		CString				strRowFormViewTitle/* = _T("")*/,
		CString				sBodyName  /*= _T("")*/,
		CString				sRowViewName/*  = _T("")*/
	)
{
	CBodyEdit* pBody = ::AddLinkAndCreateBodyEdit
		(
			rect,
			this,
			this,
			this->GetControlLinks(),
			nIDC, 
			pDBT, 
			pBodyEditClass,
			pRowFormViewClass,
			strRowFormViewTitle,
			sBodyName,
			sRowViewName
		);
		
	//if (pBody)
	//{
	//	ASSERT(pBody->GetInfoOSL()->m_pParent == GetInfoOSL());
	//	//pBody->GetInfoOSL()->m_pParent = GetInfoOSL();

	//	//Register(pBody);
	//}

	return pBody;
}

//-----------------------------------------------------------------------------
CDBTTreeEdit* CTabDialog::AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CString				sName,
			CRuntimeClass*		pTreeClass /*= NULL*/ 
		)
{
	return  ::AddLink
							(	
								this, 
								this,
								m_pControlLinks, 
								nIDC, 
								pDBT, 
								sName,
								pTreeClass
							);
}
//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog::AddLinkAndCreateControl
		(
			const CString&	sName,
			DWORD			dwStyle, 
			const CRect&	rect,
			UINT			nIDC, 
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink			/*= NULL*/,
			UINT			nBtnID				/*= BTN_DEFAULT*/
		)
{
	CParsedCtrl* pCtrl = ::AddLinkAndCreateControl
		(
			sName,
			dwStyle, 
			rect,
			this, //parent
            this->GetControlLinks(),
			nIDC, 
			pRecord, 
			pDataObj, 
			pParsedCtrlClass,
			pHotKeyLink,
			FALSE,
			nBtnID
		);

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CLabelStatic* CTabDialog::AddLabelLink (UINT nIDC)
{
	CLabelStatic* p = ::AddLabelLink(this, nIDC);
	m_pControlLinks->Add(p);	
	return p;
}

//-----------------------------------------------------------------------------
CLabelStatic* CTabDialog::AddLabelLinkWithLine(UINT nIDC, int nSizePen /*= 1*/, int pos /*= CLabelStatic::LP_TOP*/)
{
	CLabelStatic* p = ::AddLabelLinkWithLine(this, nIDC, AfxGetThemeManager()->GetStaticWithLineLineForeColor(), nSizePen, pos);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CLabelStatic* CTabDialog::AddSeparatorLink (UINT nIDC, COLORREF crBorder, int nSizePen/* = 1*/, BOOL  bVertical/* = FALSE*/, CLabelStatic::ELinePos pos/* = CLabelStatic::LP_VCENTER*/)
{
	CLabelStatic* p = ::AddSeparatorLink(this, nIDC, crBorder, nSizePen, bVertical, pos);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CGroupBoxBtn* CTabDialog::AddGroupBoxLink (UINT nIDC)
{
	CGroupBoxBtn* p = ::AddGroupBoxLink(this, nIDC);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTabDialog::ReplaceAddLink
	(
		UINT			nIDC, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink			/* = NULL */,
		UINT			nBtnID				/* = BTN_DEFAULT */
	)
{
	return ::ReplaceAddLink
	(
		this,
		this->m_pControlLinks,
		nIDC, 
		pParsedCtrlClass,
		pHotKeyLink,			
		nBtnID
	);
}

//-----------------------------------------------------------------------------
void CTabDialog::Register(CBodyEdit* pBody)
{
	//memorizzo una sola volta il namespace del BE nei children del DlgInfoItem relativo alla TabDialog corrente
	//la DlgInfoItem ha scope di documento mentre la TabDialog e le relative AddLink vengono chiamate ad ogni attivazione/istanziazione della TabDialog
	if (pBody)
	{
		CString strNameBE = pBody->GetInfoOSL()->m_Namespace.ToString();
		if (m_pDlgInfo->m_strlistChildren.Find(strNameBE) == NULL)
			m_pDlgInfo->m_strlistChildren.AddHead(strNameBE);
	}
}
							
//-----------------------------------------------------------------------------
CWnd* CTabDialog::AddLink
							(
								UINT			nIDC, 
								const CString&	sName,
								CRuntimeClass*  prtCtrl
							)
{
	if (sName.IsEmpty())
	{
		TRACE("CTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	CWnd* pCtrl = (CWnd*) prtCtrl->CreateObject();

	if (pCtrl && pCtrl->IsKindOf(RUNTIME_CLASS(CMultiSelectionListBox)))
	{
		CMultiSelectionListBox* pList = (CMultiSelectionListBox*) pCtrl;
		pList->SubclassDlgItem(nIDC, this);

		pList->GetInfoOSL()->m_pParent = GetInfoOSL();
		pList->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetNamespace());
	}

	m_pControlLinks->Add(pCtrl);
	return pCtrl;
}
//-----------------------------------------------------------------------------
BOOL CTabDialog::BatchEnableTabDialogControls()
{
	if (GetDocument()->GetType() != VMT_BATCH)
		return FALSE;
		
	EnableTabDialogControlLinks (!GetDocument()->m_bBatchRunning);
	if (!GetDocument()->m_bBatchRunning)
	{
		OnDisableControlsForBatch();
		GetDocument()->DispatchDisableControlsForBatch();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
CWndObjDescription* CTabDialog::GetControlStructure(CWndObjDescriptionContainer* pContainer, DlgInfoItem* pItem, CBaseTabDialog* pDialog, CTabManager* pParentTabManager)
{
	CString strId = cwsprintf(_T("%d_%d"), pParentTabManager->m_hWnd, pItem->GetDialogID());
	CTabDescription *pTabDesc = (CTabDescription*)pContainer->GetWindowDescription(pDialog, RUNTIME_CLASS(CTabDescription), strId);

	//il titolo lo prendo direttamente dal pDialog se non nullo, perche nei wizard quello del DlgInfoItem passato risulta 
	//disallineato (es. vendite->procedure->procedura evasione ordini)
	CString strTitle = (pDialog != NULL) ? pDialog->GetDlgInfoItem()->m_strTitle : pItem->m_strTitle;
	if (pTabDesc->m_strText != strTitle)	
	{
		pTabDesc->m_strText = strTitle;
		pTabDesc->SetUpdated(&pTabDesc->m_strText);
	}
	pTabDesc->SetID(strId);
	if (pTabDesc->m_bEnabled != (TRUE == pItem->IsEnabled()))	
	{
		pTabDesc->m_bEnabled = (TRUE == pItem->IsEnabled());
		pTabDesc->SetUpdated(&pTabDesc->m_bEnabled);
		//Se cambia una tab da selezionabile a non selezionabile(tab con X rossa) o viceversa, 
		//devo segnare come aggiornato anche il tabber
		pContainer->GetParent()->SetUpdated(NULL);
	}

	//Se la tab e' disabilitata, controllo se e' disabilitata causa protezione security
	bool bIsSecurityPotected = 	!pTabDesc->m_bEnabled &&
								!OSL_CAN_DO(pItem->GetInfoOSL(), OSL_GRANT_EXECUTE); 
	
	if (pTabDesc->m_bProtected != bIsSecurityPotected)	
	{
		pTabDesc->m_bProtected = bIsSecurityPotected;
		pTabDesc->SetUpdated(&pTabDesc->m_bProtected);
		//Se cambia una tab da selezionabile a non selezionabile(tab con lucchetto, perche protetta da security) o viceversa, 
		//devo segnare come aggiornato anche il tabber
		pContainer->GetParent()->SetUpdated(NULL);
	}

	bool bActive = (pDialog != NULL); //se pDialog != NULL vuol dire che e' la tab attiva
	if (pTabDesc->m_bActive != bActive) 
	{
		pTabDesc->m_bActive = bActive;
		pTabDesc->SetUpdated(&pTabDesc->m_bActive);
	}
	if (bActive) //solo per la tab attiva chiedo alle sue finestre figlie la loro descrizione(come ottimizzazione)
	{
	
		CRect rect;
		pDialog->GetWindowRect(rect);
		if (pTabDesc->GetRect() != rect)
		{
			pTabDesc->SetRect(rect, TRUE);
		}
#ifdef TBWEB
		
		if (pItem->m_pBaseTabDlg)
		{
			pTabDesc->AddChildWindows(pItem->m_pBaseTabDlg);
		}
#endif //TBWEB
#ifndef TBWEB
		pTabDesc->AddChildWindows(pDialog);
#endif //TBWEB
	}
	
#ifdef TBWEB

	if (pParentTabManager->m_ShowMode == CTabSelector::VERTICAL_TILE)
	{
		pTabDesc->m_strIconSource = pParentTabManager->GetTabIconSource(pItem->GetDialogID());
	}

#endif //TBWEB
	if (pDialog != NULL && pDialog->m_pBkgnd)
	{
		CString sName = cwsprintf(_T("tabbkg%s%ud.png"), strId, pDialog->m_pBkgnd);
				
		if (pTabDesc->m_ImageBuffer.Assign(pDialog->m_pBkgnd, sName))
			pTabDesc->SetUpdated(&pTabDesc->m_ImageBuffer);
	}

	return pTabDesc;
}

//-----------------------------------------------------------------------------
LRESULT CTabDialog::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	return (LRESULT) GetControlStructure(pContainer, GetDlgInfoItem(), this, GetParentTabManager());
	
}

//----------------------------------------------------------------------------
void CTabDialog::EnableTabControls()
{
	EnableTabDialogControls();
	GetDocument()->UpdateDataView();
}

//----------------------------------------------------------------------------
void CTabDialog::EnableTabDialogControls()
{
	if (BatchEnableTabDialogControls())
		return;

	if (!m_pDlgInfo->IsEnabled())
	{
		EnableTabDialogControlLinks	(FALSE);
		return;
	}

	// normal processing in interctive mode
	switch (GetDocument()->GetFormMode())
	{
		case CBaseDocument::BROWSE:
			EnableTabDialogControlLinks	(FALSE);
			break;
			
		case CBaseDocument::NEW:
			EnableTabDialogControlLinks	(TRUE);
			OnDisableControlsForAddNew();
			GetDocument()->DispatchDisableControlsForAddNew();
			break;

		case CBaseDocument::EDIT:
			EnableTabDialogControlLinks	(TRUE);
			OnDisableControlsForEdit();
			GetDocument()->DispatchDisableControlsForEdit();
			break;

		case CBaseDocument::FIND:
			EnableTabDialogControlLinks	(FALSE);
			OnEnableControlsForFind();
			GetDocument()->DispatchEnableControlsForFind();
			break;			
	}

	if (GetDocument()->GetFormMode() != CBaseDocument::BROWSE)
		GetDocument()->m_pClientDocs->OnDisableControlsAlways(this);
}

//------------------------------------------------------------------------------
BOOL CTabDialog::SetControlAutomaticExpression(DataObj* pDataObj, const CString& strExp)
{
	for (int i = 0; i < m_pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = m_pControlLinks->GetAt(i);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// i body edit sono ignorati
		if (!pControl || pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)) || !pControl->GetCtrlData())
			continue;

		if (pControl->GetCtrlData() == pDataObj)
			return pControl->SetAutomaticExpression(strExp);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CTabDialog::SetDefaultFocus()
{
	if (m_pControlLinks && m_pControlLinks->GetSize() > 0)
	{
		m_pControlLinks->SetDefaultFocus(this, m_phLastCtrlFocused);
		return;
	}

	if (m_pTileGroup)
	{
		m_pTileGroup->SetDefaultFocus();
	}
}

// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL IsCatchedByMsgMap(const AFX_MSGMAP* pMsgMap, UINT nID, UINT nCode, const AFX_MSGMAP* pMsgMapRoot)
{
	if (pMsgMap == pMsgMapRoot)
		return FALSE;

	for (int e = 0; ; e++)
	{
		if (pMsgMap->lpEntries[e].nID == nID && pMsgMap->lpEntries[e].nCode == nCode )
			return TRUE;

		if (
			pMsgMap->lpEntries[e].nMessage == 0 && pMsgMap->lpEntries[e].nID == 0 &&
			pMsgMap->lpEntries[e].nCode == 0 && pMsgMap->lpEntries[e].nLastID == 0
			)
			break;
	}
	return ::IsCatchedByMsgMap(pMsgMap->pfnGetBaseMap(), nID, nCode, pMsgMapRoot);
}

//------------------------------------------------------------------------------
BOOL CTabDialog::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	BOOL bIsHandled = ::IsCatchedByMsgMap(this->GetMessageMap(), nID, nCode, CTabDialog::GetMessageMap());

	if (!bIsHandled)
		return __super::OnCommand(wParam, lParam);

	if (!GetDocument() || !GetDocument()->m_pClientDocs)
		return FALSE;

	BOOL bHandledBefore = FALSE;
	BOOL bHandledAfter = FALSE;

	for (int i = 0; i <= GetDocument()->m_pClientDocs->GetUpperBound(); i++)
	{
		if (GetDocument()->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
			GetDocument()->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BEFORE )
		{
			GetDocument()->m_pClientDocs->GetAt(i)->SetMsgState(CClientDoc::ON_BEFORE_MSG);
			bHandledBefore = GetDocument()->m_pClientDocs->GetAt(i)->OnCmdMsg(nID, nCode, NULL, NULL) || bHandledBefore;
		}
	}

	BOOL bTabHandled = __super::OnCommand(wParam, lParam); 

	if (AfxGetThreadContext()->IsValidObject(GetDocument())) 	//removed the ExistDocument check because of performance issues
		for (int i = 0; i <= GetDocument()->m_pClientDocs->GetUpperBound(); i++)
		{
			if (GetDocument()->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
				GetDocument()->m_pClientDocs->GetAt(i)->GetMsgRoutingMode() == CClientDoc::CD_MSG_AFTER )
			{
				GetDocument()->m_pClientDocs->GetAt(i)->SetMsgState(CClientDoc::ON_AFTER_MSG);
				bHandledAfter = GetDocument()->m_pClientDocs->GetAt(i)->OnCmdMsg(nID, nCode, NULL, NULL) || bHandledAfter;
			}
		}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTabDialog::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	CBaseTabDialog::OnRButtonDown(nFlag, mousePos);
	CWnd* pWnd = ChildWindowFromPoint(mousePos);
	if (pWnd)
	{
		CParsedCtrl* pParsed = GetParsedCtrl(pWnd);
		if (GetDocument() && pParsed)
		{
			CMenu   menu;
			menu.CreatePopupMenu();

			if (
					GetDocument()->ShowingPopupMenu(pParsed->GetCtrlID(), &menu) &&
					menu.GetMenuItemCount() > 0
				)	
			{
				CRect ItemRect;
				GetWindowRect(ItemRect);
				CPoint point = ItemRect.TopLeft();
				point += mousePos;
				menu.TrackPopupMenu (TPM_LEFTBUTTON, point.x, point.y, this);
			}
		}
	}
}

//------------------------------------------------------------------------------
void  CTabDialog::OnSize(UINT nType, int cx, int cy) 
{	
	__super::OnSize(nType, cx, cy);
	
	if (m_pChildTabManagers && m_pChildTabManagers->GetSize() > 0)
		return;

	if (IsCenterControlsCustomized() ? GetCenterControls() : (m_pFormView && m_pFormView->GetCenterControls()))
		CenterControls(this, cx, cy);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTabDialog::GetTileDialog(UINT nIDD)
{
	CBaseTileDialog* pTile = NULL;
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY(pTab = m_pChildTabManagers->GetActiveDlg(i));
			if (pTab && pTab->m_hWnd)
			{
				pTile = pTab->GetTileDialog(nIDD);
				if (pTile)
					return pTile;
			}
		}
	if (m_pTileGroup)
	{
		pTile = m_pTileGroup->GetTileDialog(nIDD);
		if (pTile)
			return pTile;
	}

	return NULL;
}


/////////////////////////////////////////////////////////////////////////////
// CTabDialog diagnostics

#ifdef _DEBUG
void CTabDialog::AssertValid() const
{
	CBaseTabDialog::AssertValid();
}

void CTabDialog::Dump(CDumpContext& dc) const
{
	CBaseTabDialog::Dump(dc);
	AFX_DUMP0(dc, "\nCTabDialog");
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
//					class CRowTabDialog implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRowTabDialog, CTabDialog)
	
//-----------------------------------------------------------------------------
CRowTabDialog::CRowTabDialog(const CString& sName, UINT nIDD)
	: 
	CTabDialog	(sName, nIDD)
{
}

//------------------------------------------------------------------------------
BOOL CRowTabDialog::OnInitDialog ()
{
	ASSERT(m_pFormView);
	if (!m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)))
	{
		TRACE("CRowTabDialog::OnInitDialog: the parent isn't a CRowFormView derived class\n");
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bSetFocus = CTabDialog::OnInitDialog ();

	CRowFormView* pRView = (CRowFormView*)m_pFormView;
	if (pRView->m_pBodyEdit)
		pRView->m_pBodyEdit->UpdateCtrlBody();

	return bSetFocus;
}

//-----------------------------------------------------------------------------
void CRowTabDialog::EnableTabDialogControlLinks	(BOOL bEnable/* = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	ASSERT(m_pFormView);
	ASSERT(m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)));

	CInfoOSL* pInfoOSL = m_pDlgInfo->GetInfoOSL();

	switch (GetDocument()->GetFormMode())
	{
			//case CBaseDocument::BROWSE:
			//case CBaseDocument::FIND:
							
			case CBaseDocument::NEW:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_NEW) == 0);
				break;

			case CBaseDocument::EDIT:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( pInfoOSL, OSL_GRANT_EDIT) == 0);
				break;
	}

	((CRowFormView*)m_pFormView)->EnableMappedControlLinks (m_pControlLinks, m_DataToCtrlMap, bEnable, bMustSetOSLReadOnly);

	// ruota l'azione alle eventuali TabDialog presenti
	if (m_pChildTabManagers)
		for (int i = 0; i <= m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabDialog* pTab = NULL;
			VERIFY( pTab = m_pChildTabManagers->GetActiveDlg(i) );
			if(pTab && pTab->m_hWnd)
				pTab->EnableTabDialogControlLinks(bEnable, bMustSetOSLReadOnly);
		}
}

//------------------------------------------------------------------------------
void CRowTabDialog::OnUpdateControls(BOOL bParentIsVisible)
{
	ASSERT(m_pFormView);
	ASSERT(m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)));

	((CRowFormView*)m_pFormView)->OnUpdateMappedControls(m_pControlLinks, m_DataToCtrlMap);
	// ruota l'azione alle eventuali TileDialog presenti
	if (m_pTileGroup)
	{
		m_pTileGroup->OnUpdateControls(bParentIsVisible);
	}
}

//-----------------------------------------------------------------------------
void CRowTabDialog::RebuildLinks (SqlRecord* pRecord)
{
	if (!m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)))
	{
		TRACE("CRowTabDialog::OnInitDialog: the parent isn't a CRowFormView derived class\n");
		ASSERT(FALSE);
		return;
	}

	((CRowFormView*)m_pFormView)->RebuildMappedLinks (pRecord, m_pControlLinks, m_DataToCtrlMap);
	
	if (m_pTileGroup)
        m_pTileGroup->RebuildLinks(pRecord);

	//---- cambiano i dataobj e quindi perdo gli state-flag iniziali: se il B.E non e' protetto si abilita tutto
	CInfoOSL* pInfoOSL = m_pDlgInfo->GetInfoOSL();
	BOOL bMustSetOSLReadOnly = FALSE;
	if (GetDocument())
	{
		switch (GetDocument()->GetFormMode())
		{
			//case CBaseDocument::BROWSE:
			//case CBaseDocument::FIND:

		case CBaseDocument::NEW:
			bMustSetOSLReadOnly = (OSL_CAN_DO(pInfoOSL, OSL_GRANT_NEW) == 0);
			break;

		case CBaseDocument::EDIT:
			bMustSetOSLReadOnly = (OSL_CAN_DO(pInfoOSL, OSL_GRANT_EDIT) == 0);
			break;
		}
	}
	if (bMustSetOSLReadOnly)
		SetOSLReadOnlyOnControlLinks(m_pControlLinks, m_DataToCtrlMap);//TODO RICCARDO
	//----

}

//-----------------------------------------------------------------------------
CParsedCtrl* CRowTabDialog::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink /*=NULL*/,
		UINT			nBtnID /*=BTN_DEFAULT*/
	)
{
	ASSERT(m_pFormView);
	if (!m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)))
	{
		TRACE("CRowTabDialog::OnInitDialog: the parent isn't a CRowFormView derived class\n");
		ASSERT(FALSE);
		return NULL;
	}
	if (pRecord == NULL)
		TRACE("CRowTabDialog::AddLink: link of the control id=%d and name=%s without SqlRecord \n", nIDC, (LPCTSTR)sName);
	
	if (sName.IsEmpty())
	{
		TRACE("CRowTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	if (!pParsedCtrlClass->IsDerivedFrom(RUNTIME_CLASS(CBoolCheckListBox)))
		((CRowFormView*)m_pFormView)->BuildMappedDataToCtrlLink
			(
				pRecord,
				pDataObj,
				m_DataToCtrlMap,
				m_pControlLinks->GetSize()
			);

	return CTabDialog::AddLink
		(
			nIDC, 
			sName, 
			pRecord, 
			pDataObj, 
			pParsedCtrlClass,
			pHotKeyLink, 
			nBtnID
		);
}

//-----------------------------------------------------------------------------
CExtButton*	CRowTabDialog::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord ,
		DataObj*		pDataObj
	)
{
	ASSERT(m_pFormView);
	if (!m_pFormView->IsKindOf(RUNTIME_CLASS(CRowFormView)))
	{
		TRACE("CRowTabDialog::OnInitDialog: the parent isn't a CRowFormView derived class\n");
		ASSERT(FALSE);
		return NULL;
	}
	if (pRecord == NULL)
		TRACE("CRowTabDialog::AddLink: link of the control id=%d and name=%s without SqlRecord \n", nIDC, (LPCTSTR)sName);

	if (sName.IsEmpty())
	{
		TRACE("CRowTabDialog::AddLink: the control idc=%d has empty name\n", nIDC);
		ASSERT(FALSE);
	}

	((CRowFormView*)m_pFormView)->BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetSize()
		);

	return CTabDialog::AddLink
		(
			nIDC, 
			sName, 
			pRecord, 
			pDataObj
		);
}
							
//-----------------------------------------------------------------------------
int CRowTabDialog::AddDataBoolToCheckLB
	(
		CBoolCheckListBox*	pBCLB,
		LPCTSTR				lpszAssoc,
		SqlRecord*			pRecord, 
		DataObj*			pDataObj
	)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)));

	if (!pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
		return -1;

	if (pRecord == NULL)
		TRACE(_T("CRowTabDialog::AddDataBoolToCheckLB in a CRowTabDialog without SqlRecord\n"));

	int nIdxLB = pBCLB->AddDataBool(lpszAssoc, (DataBool&)*pDataObj);
	((CRowFormView*)m_pFormView)->BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetUpperBound(),
			nIdxLB
		);

	return nIdxLB;
}


/////////////////////////////////////////////////////////////////////////////
// CTabDialog diagnostics

#ifdef _DEBUG
void CRowTabDialog::AssertValid() const
{
	CTabDialog::AssertValid();
}

void CRowTabDialog::Dump(CDumpContext& dc) const
{
	CTabDialog::Dump(dc);
	AFX_DUMP0(dc, "\nCRowTabDialog");
}
#endif //_DEBUG


/////////////////////////////////////////////////////////////////////////////
//							CWizardTabDialog
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CWizardTabDialog, CTabDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWizardTabDialog, CTabDialog)
	//{{AFX_MSG_MAP(CWizardTabDialog)
	ON_WM_WINDOWPOSCHANGED	()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//----------------------------------------------------------------------------
CWizardTabDialog::CWizardTabDialog(const CString& sName, UINT nIDD /*= 0*/)
: 
	CTabDialog		(sName, nIDD),
	m_bLast			(FALSE)
{
}

//----------------------------------------------------------------------------
BOOL CWizardTabDialog::OnInitDialog()
{
	Activate();
	BOOL bRet = CTabDialog::OnInitDialog();

	UINT nIDRes = GetBitmapID();
	if (nIDRes)
	{
		CAbstractFormView *pView = m_pParentTabManager->GetFormView();
		ASSERT_KINDOF(CWizardFormView, pView);
	
		((CWizardFormView*)pView)->SetWizardBitmap(nIDRes);
	}
	return bRet;
}

//-----------------------------------------------------------------------------
void CWizardTabDialog::CustomizeExternal()
{
	if (!m_pTileGroup)
		return;

	if (GetDocument())
		GetDocument()->AddClientDocTileDialog(m_pTileGroup);

}

//----------------------------------------------------------------------------
void CWizardTabDialog::Activate()
{
	OnActivate();
	((CWizardFormDoc*)GetDocument())->DispatchActivate(m_pDlgInfo->GetDialogID());
}

//----------------------------------------------------------------------------
void CWizardTabDialog::Deactivate ()
{
	((CWizardFormDoc*)GetDocument())->DispatchDeactivate(m_pDlgInfo->GetDialogID());
	OnDeactivate();
}

//----------------------------------------------------------------------------
BOOL	CWizardTabDialog::IsLast()
{
	return m_bLast;
}

//----------------------------------------------------------------------------
void	CWizardTabDialog::SetLast(BOOL bLast /*= TRUE*/)
{
	m_bLast = bLast;
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::OnWizardNext()
{
	// nell'override, restituire l'IDD della tab da attivare,
	// WIZARD_SAME_TAB in caso di errore (per cui non si deve cambiare tab)
	return WIZARD_DEFAULT_TAB; 
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::OnWizardBack()
{
	// nell'override, restituire l'IDD della tab da attivare,
	// WIZARD_SAME_TAB in caso di errore (per cui non si deve cambiare tab)
	return WIZARD_DEFAULT_TAB;
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::OnWizardFinish()
{
	if (GetDocument() && GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame)))
		return WIZARD_DEFAULT_TAB;

	//Questa OnCloseDocument Implicita sulla finish non deve essere registrata, verrà eseguita dal test
	//manager semplicemente replicando la OnWizardFinish
	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	CAbstractFormDoc * pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if(pDoc)
		pDoc->CloseDocument ();

	//ripristina lo stato del macrorecorder
	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;

	return WIZARD_DEFAULT_TAB;
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::GetBitmapID()
{
	LRESULT nIDC = ((CWizardFormDoc*)GetDocument())->DispatchGetBitmapID(GetDialogID());
	if (nIDC == 0)
		nIDC = OnGetBitmapID();
	
	return nIDC;
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::OnGetBitmapID()
{
	// nell'override, restituire l'identificatore della bitmap da 
	// visualizzare nel corrente passo del wizard
	return 0;
}

//----------------------------------------------------------------------------
LRESULT CWizardTabDialog::OnWizardCancel()
{
	//Questa OnCloseDocument Implicita sulla finish non deve essere registrata, verrà eseguita dal test
	//manager semplicemente replicando la OnWizardCancel
	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	CAbstractFormDoc * pDoc = GetDocument();
	ASSERT(pDoc);
	if(pDoc)
		pDoc->OnCloseDocument ();

	//ripristina lo stato del macrorecorder
	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;

	return WIZARD_DEFAULT_TAB;
}

//------------------------------------------------------------------------------
void CWizardTabDialog::OnSize (UINT nType, int cx, int cy)
{
    __super::OnSize (nType, cx, cy);	

	if (!m_pTileGroup)
		CAbstractFormView::CenterControls(this, cx, cy);
}

//---------------------------------------------------------------------------------------
void CWizardTabDialog::OnWindowPosChanged( WINDOWPOS* lpwndpos)
{
	if (m_pTileGroup)
		m_pTileGroup->MoveWindow(0, 0, lpwndpos->cx, lpwndpos->cy);
}

//----------------------------------------------------------------------------------------------
// CXMLAppCriteriaTabDlg 
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXMLAppCriteriaTabDlg, CWizardTabDialog)

//----------------------------------------------------------------------------
CXMLAppCriteriaTabDlg::CXMLAppCriteriaTabDlg (const CString& sName /*= _T("")*/, UINT nIDD /* = -1*/)
	:
	CWizardTabDialog	(sName, nIDD),
	m_pExportCriteria	(NULL),
	m_nDialogID			(nIDD)
{
}

//----------------------------------------------------------------------------
BOOL CXMLAppCriteriaTabDlg::OnInitDialog()
{
	if (GetDocument())
		m_pExportCriteria = GetDocument()->GetBaseExportCriteria();

	return CWizardTabDialog::OnInitDialog();
}


//-----------------------------------------------------------------------------
LRESULT CXMLAppCriteriaTabDlg::OnGetBitmapID()
{
	return IDB_WIZARD_DOC_DEFAULT;
}

/////////////////////////////////////////////////////////////////////////////
// 							CTitleButton
/////////////////////////////////////////////////////////////////////////////
//

//IMPLEMENT_DYNAMIC(CTitleButton, CButton)
 
//BEGIN_MESSAGE_MAP(CTitleButton, CButton)
//END_MESSAGE_MAP()

CTitleButton::CTitleButton() 
	:
	m_crTextColor(0)
{
	
}

void CTitleButton::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	CDC dc;
	dc.Attach(lpDrawItemStruct->hDC);     //Get device context object

	CRect rt(lpDrawItemStruct->rcItem);   //Get button rect
	
	CWnd* pParent = GetParent();
	ASSERT(pParent);
	CFont* pFont = pParent->GetFont();
	ASSERT(pFont);
		
	COLORREF crTextOld = dc.SetTextColor(m_crTextColor);
	CGdiObject* pOldFont = dc.SelectObject(pFont);
	
	rt.DeflateRect(0, 0, 3, 0);

	dc.DrawText(m_sTitle, rt, DT_LEFT|DT_VCENTER|DT_NOCLIP|DT_SINGLELINE);

	dc.SelectObject(pOldFont);
	dc.SetTextColor(crTextOld);

	dc.Detach();
}

/////////////////////////////////////////////////////////////////////////////
// 							CTabManager
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTabManager, CBaseTabManager)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTabManager, CBaseTabManager)

	ON_WM_RBUTTONDOWN	()
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION,						OnGetControlDescription)
	ON_BN_CLICKED		(ID_MT_DROPDOWN_BUTTON,								OnComboButtonClick)
	ON_COMMAND_RANGE	((UINT)(ID_MT_MENU_START), (UINT)(ID_MT_MENU_END),	OnTabActivateFromMenu)
	ON_MESSAGE			(UM_EXTDOC_BATCH_COMPLETED,							OnBatchCompleted)
	
	ON_WM_SIZE			()
	ON_WM_ERASEBKGND	()

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CTabManager::CTabManager()
	:
	m_bFirstTabAfterBatchCompleted(FALSE),
	m_FirstFocusedTab(0)
#ifdef _DEBUG
	  ,m_bCrtCheckMemoryFailed (FALSE), m_nLatestTabMadeVisible(-1)
#endif
{
}

//------------------------------------------------------------------------------
BOOL CTabManager::OnEraseBkgnd(CDC* pDC)
{
	
	CRect rclientRect;
	this->GetClientRect(rclientRect);
	
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());

	if (pFrame && pFrame->IsLayoutSuspended())
	{
		if (GetShowMode() == NORMAL)
		{
			CRect tempRect;
			CRect intersectRect;
			for (int i = 0; i < this->GetItemCount(); i++)
			{
				this->GetItemRect(i, tempRect);

				CRect frameRect;
				pFrame->GetClientRect(frameRect);

				BOOL inters = intersectRect.IntersectRect(tempRect, frameRect);
				if (!inters || intersectRect == tempRect)
					pDC->ExcludeClipRect(&tempRect);
			}
		}
	}
	if (GetShowMode() == VERTICAL_TILE)
	{
		CWnd* pCtrl = this->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			if (!pCtrl->IsWindowVisible())
				continue;

			CRect intersectRect;
			CRect screen;
			pCtrl->GetWindowRect(&screen);
			this->ScreenToClient(&screen);

			pDC->ExcludeClipRect(&screen);
		}
	}
	
	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());
	if (pParsedForm)
		pDC->FillRect(&rclientRect, pParsedForm->GetBackgroundBrush());
	else
		pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetTileDialogTitleBkgColorBrush());

	return TRUE;
}

//------------------------------------------------------------------------------
void CTabManager::OnComboButtonClick()
{
	CMenu   menu;

	menu.CreatePopupMenu();

	int start, end;
	GetVisibleTabs(start, end);

	for (int i = 0; i < m_pDlgInfoAr->GetSize(); i++)
	{
		UINT nOptions = 0;
		if (i > (int)ID_MT_MENU_END)
		{
			ASSERT_TRACE(FALSE, "Numero di tab dialog troppo elevato");
			continue;
		}

		DlgInfoItem* pItem = m_pDlgInfoAr->GetAt(i);
		int tabPos = GetTabIndexFromItemPos(i);
		if (pItem->IsVisible() && tabPos == GetActiveTab())
			nOptions |= MF_CHECKED;
		if (!pItem->IsEnabled())
			nOptions |= MF_DISABLED;
		if (pItem->IsVisible() && tabPos >= start && tabPos < end)
			nOptions |= MF_HILITE;
		menu.AppendMenu(MF_STRING | nOptions, ID_MT_MENU_START + i, pItem->m_strTitle);
	}
}
//------------------------------------------------------------------------------
void CTabManager::OnTabActivateFromMenu(UINT nID)
{
	int idx = nID - ID_MT_MENU_START;

	DlgInfoItem* pItem = GetDlgInfoArray()->GetAt(idx);
	if (!pItem->IsVisible())
	{
		if (m_nLatestTabMadeVisible != -1)
		{
			ShowTab(m_nLatestTabMadeVisible, FALSE);
		}
		m_nLatestTabMadeVisible = idx;
		ShowTab(idx, TRUE);
	}
	CBaseTabManager::TabDialogActivate(pItem->GetDialogID());
}
//------------------------------------------------------------------------------
BOOL CTabManager::CreateEx(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect,
	_In_ CWnd* pParentWnd, _In_ UINT nID)
{
	BOOL b = __super::CreateEx(dwExStyle, dwStyle, rect, pParentWnd, nID); 
	if (!b)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if ( GetShowMode() == NORMAL)
 	{
		//m_Button.Create(L"", WS_CHILD|WS_VISIBLE|BS_PUSHBUTTON|BS_OWNERDRAW, CRect(0, 0, 0, 0), this, ID_MT_DROPDOWN_BUTTON);
	}
	//problemi di paint m_ctrlTitle.Create(WS_CHILD, CRect(0, 0, 0, 0), this, ID_MT_LABEL);
	//anche ... m_ctrlTitle.Create(L"", WS_CHILD|BS_PUSHBUTTON|BS_OWNERDRAW, CRect(0, 0, 0, 0), this, ID_MT_LABEL);
	return b;
}
//------------------------------------------------------------------------------
BOOL CTabManager::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (__super::PreTranslateMessage(pMsg))
		return TRUE;

	if (GetDocument() && !GetDocument()->m_bForwardingSysKeydownToChild && !GetDocument()->m_bForwardingSysKeydownToParent)
	{
		if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		{
			BOOL bHoldForwardingSysKeydownToChild = GetDocument()->m_bForwardingSysKeydownToChild;
			GetDocument()->m_bForwardingSysKeydownToChild = TRUE;

			BOOL bOk = FALSE;
			for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound() && !bOk; i++)
			{
				CTabManager* pTabber = GetActiveDlg()->m_pChildTabManagers->GetAt(i);
				if (!pTabber)
					continue;

				if (GetDocument()->m_bForwardingSysKeydownToParent && (LPARAM)pTabber->GetSafeHwnd() == pMsg->lParam)
					continue;

				bOk = pTabber->PreTranslateMessage(pMsg);
			}

			GetDocument()->m_bForwardingSysKeydownToChild = bHoldForwardingSysKeydownToChild;

			if (bOk)
				return TRUE;
		}
	}

	if (GetDocument() && GetDocument()->m_bForwardingSysKeydownToChild)
		return FALSE;

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetDocument(), this);

#else

	if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound(); i++)
			if (GetActiveDlg()->m_pChildTabManagers->GetAt(i)->PreTranslateMessage(pMsg))
				return TRUE;

	return __super::PreTranslateMessage(pMsg);

#endif
}

//------------------------------------------------------------------------------
void CTabManager::OnSize (UINT nType, int cx, int cy)
{
	__super::OnSize (nType, cx, cy);	
	CWnd *pWnd = GetDlgItem(1);
	if (pWnd)
	{
		pWnd->DestroyWindow();
	}
} 

#define BUTTON_WIDTH_LARGE 38
#define BUTTON_WIDTH_NARROW 19
//------------------------------------------------------------------------------
void CTabManager::GetVisibleTabs(int &start, int &end)
{
	CRect cr, ir;
	GetClientRect(cr);
	GetItemRect(GetItemCount()-1, ir);
	start = -1;
	for (end = 0; end < GetItemCount(); end++)
	{
		GetItemRect(end, ir);
		if (ir.right < 3)
			continue;
		if (start == -1)
			start = end;//prima tab visibile
		if (ir.left + BUTTON_WIDTH_LARGE > cr.right)
			break;
	}
}

//-----------------------------------------------------------------------------
void CTabManager::SetTitle (const CString& s, COLORREF crTextColor )
{ 
	static int n = 0;
	n++;
	CString sText(s + (n % 2 ? L" - stringa LUUUUUUGGGGAAAAAAA" : L""));
	 
	ASSERT(m_ctrlTitle.m_hWnd);

	m_ctrlTitle.m_crTextColor = crTextColor;
	m_ctrlTitle.m_sTitle = sText;

	CRect rect;
	m_ctrlTitle.GetWindowRect(rect);
	this->ScreenToClient(rect);

	CPaintDC dc(this);
	int w = ::GetTextSize(&dc, sText, m_ctrlTitle.GetFont()).cx;
	rect.right = rect.left + w + 2;
	m_ctrlTitle.MoveWindow(rect);

	m_ctrlTitle.ShowWindow(sText.IsEmpty() ? SW_HIDE : SW_NORMAL);
}

//-----------------------------------------------------------------------------
CWnd* CTabManager::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	if (GetNamespace() == aNS)
		return this;
	CTabDialog* pActiveDialog = GetActiveDlg();
	if (pActiveDialog)
	{
		if (pActiveDialog->GetNamespace() == aNS)
			return pActiveDialog;
		
		CWnd *pWnd = pActiveDialog->GetWndLinkedCtrl(aNS);
		if (pWnd) 
			return pWnd;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CTabManager::OnRButtonDown(UINT nFlags, CPoint point)
{
	TCHITTESTINFO HitTestInfo;
	HitTestInfo.pt = point;
	HitTestInfo.flags = TCHT_ONITEM;

	int nPos = HitTest(&HitTestInfo);

	if (nPos < 0 || nPos > m_pDlgInfoAr->GetUpperBound() || m_pDlgInfoAr->GetUpperBound() < 0)
		 return;
	
	int nRealPos = nPos;
	for (int i=0; i <= (int)nPos; i++)
	{
		if (!m_pDlgInfoAr->GetAt(i)->IsVisible()) 
		{
			nRealPos++;
		}
	}
	if (
			nRealPos > m_pDlgInfoAr->GetUpperBound()
			||
			! m_pDlgInfoAr->GetAt(nRealPos)->IsEnabled()
		)
	{
		return;
	}

	DlgInfoItem* pDlgInfoItem = m_pDlgInfoAr->GetAt(nRealPos);

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocument();
	pDoc->OnTabRButtonDown(pDlgInfoItem->GetDialogID());
}

//-----------------------------------------------------------------------------
BOOL CTabManager::DispatchOnEnableTabSelChanging (UINT nBeforeTabIDD, UINT nAfterTabIDD)
{ 
	CWnd* pParent = GetParent();
	if (pParent->IsKindOf(RUNTIME_CLASS(CAbstractFormView)) && !IsChild(GetFocus()))
	{
		if (!((CAbstractFormView*) pParent)->CheckForm(TRUE))
			return FALSE;
	}

	return 
		OnEnableTabSelChanging (nBeforeTabIDD, nAfterTabIDD) && 
		GetDocument() &&
		GetDocument()->DispatchOnEnableTabSelChanging (GetDlgCtrlID(), nBeforeTabIDD, nAfterTabIDD); 
}

//-----------------------------------------------------------------------------
void CTabManager::DispatchOnAfterTabSelChanged(UINT nTabIDD) 
{
	if (!GetDocument())
		return;

	CTabDialog *pDlg = GetActiveDlg();
	if (pDlg && pDlg->IsKindOf(RUNTIME_CLASS(CWizardTabDialog)))
	{
		//da una chance al programmatore per impostare diversamente l'abilitazione dei pulsanti
		((CWizardTabDialog*)pDlg)->OnUpdateWizardButtons();
		if (GetDocument()->IsKindOf(RUNTIME_CLASS(CWizardFormDoc)))
			((CWizardFormDoc*)GetDocument())->DispatchUpdateWizardButtons(((CWizardTabDialog*)pDlg)->GetDlgCtrlID());

	}
	OnTabSelChanged(nTabIDD);

	GetDocument()->DispatchOnTabSelChanged (GetDlgCtrlID(), nTabIDD); 
}
//-----------------------------------------------------------------------------
void CTabManager::OnTabSelChanged(UINT nTabIDD)
{
	OnUpdateTabStates();
	if (GetShowMode() != NORMAL)
	{
		ArrangeItems();
		CTabDialog* pActiveDialog = GetActiveDlg();
		if (pActiveDialog && pActiveDialog->GetChildTabManager())
		{
			pActiveDialog->GetChildTabManager()->SendMessage(UM_RECALC_CTRL_SIZE);
		}
	}
}

//-----------------------------------------------------------------------------
void CTabManager::OnAttachParents(CBaseTabDialog* pDlg)
{
	ASSERT_VALID(this);
	ASSERT_VALID(pDlg);
	ASSERT_KINDOF(CTabDialog, pDlg);

	CTabDialog* pTabDlg = dynamic_cast<CTabDialog*>(pDlg);
	if (pTabDlg)
	{
		if (GetFormView())
			pTabDlg->AttachParents(GetDocument(), GetFormView(), this);
		else if (GetParentParsedDialog())
			pTabDlg->AttachParents(GetDocument(), GetParentParsedDialog(), this);
	}

	else 
	{
		CTileDialog* pTileDlg = dynamic_cast<CTileDialog*>(pDlg);
		if (pTileDlg)
			pTileDlg->OnAttachParents();
	}
}

//-----------------------------------------------------------------------------
void CTabManager::PrepareTabDialogNamespaces()
{
	if (!GetDlgInfoArray())
	{
		ASSERT_TRACE(FALSE, "CTabManager::PrepareTabDialogNamespaces() called without DlgInfoArray elements");
		return;
	}
	if (GetDlgInfoArray()->m_bPrepared)
		return;

	DlgInfoItem* pDlgItem;
	CBaseTabDialog* pTabDialog;
	for (int i=0; i <= GetDlgInfoArray()->GetUpperBound(); i++)
	{
		pDlgItem = GetDlgInfoArray()->GetAt(i);
		
		if (!pDlgItem || !pDlgItem->GetDialogClass())
			continue;

		pTabDialog = (CBaseTabDialog*) pDlgItem->GetDialogClass()->CreateObject();
	
		if (pDlgItem->GetNamespace().IsEmpty())  //An.20839
		{
			pDlgItem->GetNamespace().SetChildNamespace(CTBNamespace::TABDLG, pTabDialog->GetFormName(), GetNamespace());
		}

		AfxGetSecurityInterface()->GetObjectGrant ( pDlgItem->GetInfoOSL() );
		delete pTabDialog;
	}

	GetDlgInfoArray()->m_bPrepared = TRUE;
}

//-----------------------------------------------------------------------------
CAbstractFormView* CTabManager::GetFormView()
{
	CWnd* pParent = GetParent();
	ASSERT_VALID(pParent);

	if (pParent->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		return (CAbstractFormView*) pParent;
	
	if (pParent->IsKindOf(RUNTIME_CLASS(CTabDialog)))
		return ((CTabDialog*)pParent)->m_pFormView;

	CParsedForm* pParsedForm = ::GetParsedForm(pParent);
	return pParsedForm ? (CAbstractFormView* ) pParsedForm->GetBaseFormView(RUNTIME_CLASS(CAbstractFormView)) : NULL;
}

//-----------------------------------------------------------------------------
CParsedDialog* CTabManager::GetParentParsedDialog()
{
	CWnd* pParent = GetParent();
	ASSERT_VALID(pParent);

	return dynamic_cast<CParsedDialog*>(pParent);
}

//-----------------------------------------------------------------------------
CAbstractFormDoc* CTabManager::GetDocument()	
{ 
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(m_pDocument);
	if (pDoc)
		return pDoc;

	if (GetFormView())
		return GetFormView()->GetDocument(); 

	if (GetParentParsedDialog())
	{
		CBaseDocument* pDoc = GetParentParsedDialog()->GetDocument();
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			return (CAbstractFormDoc*)pDoc;
	}

	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------------
void CTabManager::DeleteTab(DlgInfoItem* pInfo, BOOL bActivateNextTab/*= TRUE*/)
{
	if (!this->GetDlgInfoArray())
		return;

	int pos = -1;
	for (int i = this->GetDlgInfoArray()->GetCount() - 1; i >= 0; i--)
	{
		DlgInfoItem* pItem = (DlgInfoItem*)this->GetDlgInfoArray()->GetAt(i);
		if (pItem && pItem == pInfo)
		{
			pos = i;
			break;
		}
	}

	ClearActiveDlg();

	if (pos >= 0)
	{
		DlgInfoItem* pInfo = GetDlgInfoArray()->GetAt(pos);
		if (pInfo && pInfo->m_pBaseTabDlg != NULL)
		{
			pInfo->m_pBaseTabDlg->DestroyWindow();
			SAFE_DELETE(pInfo->m_pBaseTabDlg);
		}
		
		this->DeleteItem(pos);
		this->GetDlgInfoArray()->RemoveAt(pos);
	}

	if (bActivateNextTab && GetDlgInfoArray()->GetCount() > 0)
	{
		DlgInfoItem* pInfo = GetDlgInfoArray()->GetAt(0);
		if (pInfo->IsVisible() && pInfo->IsEnabled())
			TabDialogActivate(this->GetDlgCtrlID(), pInfo->GetDialogID());

		AdjustTabManager();
	}
}

//-----------------------------------------------------------------------------
DlgInfoItem*  CTabManager::AddDialog
	(	
		CRuntimeClass* pClass, 
		UINT nIDTitle, 
		int nOrdPos/*= -1*/,
		UINT nBeforeIDD /*=0*/,
		const CString nsSelectorImage /*_T("")*/, 
		const CString sSelectorTooltip /*_T("")*/
	)
{
	if (!pClass->IsDerivedFrom(RUNTIME_CLASS(CTabDialog)))
	{
		ASSERT_TRACE1(FALSE, "Runtime class parameter %s must be a CTabDialog!\n", (LPCTSTR)CString(pClass->m_lpszClassName));
		return NULL;
	}

	int nDlgPos = nOrdPos;

	// guardo in che posizione è la dialog identificata da nBeforeIDD e inserisco la
	// tabdlg pDialogClass al suo posto
	if (nBeforeIDD > 0)
	{
		nDlgPos = GetTabDialogPos(nBeforeIDD);
		if (nDlgPos == -1)
			nDlgPos = nOrdPos;
	}

	int nPos = CBaseTabManager::AddDialog(pClass, nIDTitle, nDlgPos, nsSelectorImage, sSelectorTooltip);

	DlgInfoItem* pDlgInfo = m_pDlgInfoAr->GetAt(nPos);

	return pDlgInfo;
}

//-----------------------------------------------------------------------------
DlgInfoItem*  CTabManager::AddDialog
	(	
		UINT					nIDTitle, 
		const	CTBNamespace&	aNs,
		const	CString&		sTitle,
		int		nOrdPos			/*= -1*/,
		UINT	nBeforeIDD		/*= 0*/,
		const CString nsSelectorImage /*_T("")*/, 
		const CString sSelectorTooltip /*_T("")*/
	)
{
	int nDlgPos = nOrdPos;

	// guardo in che posizione è la dialog identificata da nBeforeIDD e inserisco la
	// tabdlg al suo posto
	if (nBeforeIDD > 0)
	{
		nDlgPos = GetTabDialogPos(nBeforeIDD);
		if (nDlgPos == -1)
			nDlgPos = nOrdPos;
	}

	int nPos = CBaseTabManager::AddDialog (RUNTIME_CLASS(CTabDlgEmpty), nIDTitle, aNs, sTitle, nDlgPos, nsSelectorImage, sSelectorTooltip);

	DlgInfoItem* pDlgInfo = m_pDlgInfoAr->GetAt(nPos);
	//InsertDlgInfoItem(nPos, pDlgInfo);
	
	return pDlgInfo;
}

//---------------------------------------------------------------------------
void CTabManager::CustomizeExternal()		
{ 
	if (GetDocument())
		GetDocument()->AddClientDocTabDlg(this);
	if (GetFormView())
		GetFormView()->CustomizeTabber(this);
}

//---------------------------------------------------------------------------
UINT CTabManager::GetTabDialogID(UINT nIDDTileGroup)
{
	for (int a = 0; a <= GetDlgInfoArray()->GetUpperBound(); a++)
	{
		DlgInfoItem* pItem = GetDlgInfoArray()->GetAt(a);
		if (!pItem || !pItem->IsKindOf(RUNTIME_CLASS(TileGroupInfoItem)))
			continue;
				
		TileGroupInfoItem* pTileInfoItem = (TileGroupInfoItem*) pItem;
		if (pTileInfoItem->GetTileGroupID() == nIDDTileGroup) 
		{
			return pTileInfoItem->GetDialogID();
		}
	}

	// se non e' nel gruppo allora provo a vedere se e' nei controlli 
	// della finestra attivi (fatto a mano)
	CTabDialog* pDlg = GetActiveDlg();
	return pDlg && pDlg->GetWndCtrl(nIDDTileGroup) ? pDlg->GetDialogID() : 0;
}

//-----------------------------------------------------------------------------
BOOL CTabManager::TabDialogActivate(const CString& sNsTab)
{
	for (int i = 0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
	{
		DlgInfoItem* pDlgInfo = m_pDlgInfoAr->GetAt(i);

		if (!pDlgInfo->GetNamespace().ToUnparsedString().Compare(sNsTab))
			return CBaseTabManager::TabDialogActivate(pDlgInfo->GetDialogID()) > 0;
	}

	if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pChildTabber = GetActiveDlg()->m_pChildTabManagers->GetAt(i);
			if (pChildTabber->TabDialogActivate(sNsTab) > 0)
				return TRUE;
		}

	return FALSE;
}

//-----------------------------------------------------------------------------
int CTabManager::TabDialogActivate(UINT nTabberID, UINT nIDD)
{
	if (GetDlgCtrlID() == (int)nTabberID)
		return CBaseTabManager::TabDialogActivate(nIDD);

	if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pChildTabber = GetActiveDlg()->m_pChildTabManagers->GetAt(i);
			return pChildTabber->TabDialogActivate(pChildTabber->GetDlgCtrlID(), nIDD);
		}
	
	return 0;
}

//-----------------------------------------------------------------------------
int CTabManager::TabDialogShow(UINT nTabIDC, UINT nIDD, BOOL bShow)
{
	if (GetDlgCtrlID() == (int) nTabIDC)
		return CBaseTabManager::TabDialogShow(nIDD, bShow);

	if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pChildTabber = GetActiveDlg()->m_pChildTabManagers->GetAt(i);
			return pChildTabber->TabDialogShow(pChildTabber->GetDlgCtrlID(), nIDD, bShow);
		}
		
	return 0;
}

//-----------------------------------------------------------------------------
int CTabManager::TabDialogEnable(UINT nTabIDC, UINT nIDD, BOOL bEnable)
{

	if (GetDlgCtrlID() == (int)nTabIDC)
	{
		BOOL b = CBaseTabManager::TabDialogEnable(nIDD, bEnable);
		OnUpdateTabStates();
		return b;
	}

	if (GetActiveDlg() && GetActiveDlg()->m_pChildTabManagers)
		for (int i = 0; i <= GetActiveDlg()->m_pChildTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pChildTabber = GetActiveDlg()->m_pChildTabManagers->GetAt(i);
			return pChildTabber->TabDialogEnable(pChildTabber->GetDlgCtrlID(), nIDD, bEnable);
		}
	
	OnUpdateTabStates();
	return 0;
}

//-----------------------------------------------------------------------------
LRESULT CTabManager::OnBatchCompleted(WPARAM wParam, LPARAM lParam)
{
	if (!m_bFirstTabAfterBatchCompleted)
		return 0L;

	for (int i = 0; i < GetDlgInfoArray()->GetCount(); i++)
	{
		DlgInfoItem* pInfo = GetDlgInfoArray()->GetAt(i);
		if (pInfo->IsVisible() &&  pInfo->IsEnabled() && pInfo->GetDialogID() != m_pActiveDlg->GetDialogID()) 
		{
			TabDialogActivate(this->GetDlgCtrlID(), pInfo->GetDialogID());
			break;
		}
	}

	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CTabManager::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CTabberDescription* pDesc = (CTabberDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CTabberDescription), strId);
	pDesc->UpdateAttributes(this); 

	Bool3 bIsVertical = (m_ShowMode == VERTICAL_TILE) ? Bool3::B_TRUE : Bool3::B_FALSE;
	if (pDesc->m_bIsVertical != bIsVertical)
	{
		pDesc->m_bIsVertical = bIsVertical;
		pDesc->SetUpdated(&pDesc->m_bIsVertical);
	}
	if (pDesc->m_nIconWidth != m_nIconWidth)
	{
		pDesc->m_nIconWidth = m_nIconWidth;
		pDesc->SetUpdated(&pDesc->m_nIconWidth);
	}
	if (pDesc->m_nIconHeight != m_nIconHeight)
	{
		pDesc->m_nIconHeight = m_nIconHeight;
		pDesc->SetUpdated(&pDesc->m_nIconHeight);
	}
	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
void CTabManager::SetFirstTabAfterBatchCompleted(BOOL bEnable)
{
	m_bFirstTabAfterBatchCompleted = bEnable;
}

//-----------------------------------------------------------------------------
const BOOL& CTabManager::IsFirstTabAfterBatchCompletedEnabled() const
{
	return m_bFirstTabAfterBatchCompleted;
}

//-----------------------------------------------------------------------------
CTileGroup* CTabManager::GetActiveTileGroup ()
{
	CTabDialog* pDialog = GetActiveDlg();
	return pDialog ? pDialog->m_pTileGroup : NULL;
}

//-----------------------------------------------------------------------------
void CTabManager::SetDefaultFocus()
{
	if (GetDocument() && GetDocument()->GetFormMode() == CBaseDocument::NEW)
	{
		int nVisiblePos = GetDlgItemPos(m_FirstFocusedTab);
		if (nVisiblePos >= 0 && nVisiblePos < GetDlgInfoArray()->GetSize())
		{
			DlgInfoItem* pItem = GetDlgInfoArray()->GetAt(nVisiblePos);
			TabDialogActivate(pItem->GetNamespace().ToUnparsedString());
		}
	}
	GetActiveDlg()->SetDefaultFocus();
}

//-----------------------------------------------------------------------------
BOOL CTabManager::PrepareAuxData()
{
	OnUpdateTabStates();

	CTabDialog* pTab = NULL;
	if (GetActiveDlg())
		VERIFY(pTab = GetActiveDlg());
	
	if (pTab && pTab->m_hWnd)
		return pTab->PrepareAuxData();

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CTabManager diagnostics

#ifdef _DEBUG
void CTabManager::AssertValid() const
{
	CBaseTabManager::AssertValid();
}

void CTabManager::Dump(CDumpContext& dc) const
{
	CBaseTabManager::Dump(dc);
	AFX_DUMP0(dc, "\nCTabManager");
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// 							TabManagers
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TabManagers, Array)

/////////////////////////////////////////////////////////////////////////////
// TabManagers diagnostics

#ifdef _DEBUG
void TabManagers::AssertValid() const
{
	Array::AssertValid();
}

void TabManagers::Dump(CDumpContext& dc) const
{
	Array::Dump(dc);
	AFX_DUMP0(dc, "\nTabManagers");
}
#endif //_DEBUG


/////////////////////////////////////////////////////////////////////////////
// 							CTabWizard
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTabWizard, CTabManager)

BEGIN_MESSAGE_MAP(CTabWizard, CTabManager)
END_MESSAGE_MAP() 

CTabWizard::CTabWizard()
{ 
	SetShowMode(NONE);
	SetSelectorAppearance(TEXT_ONLY);
}
//-----------------------------------------------------------------------------
void CTabWizard::Customize()
{
	CView* pView = GetFormView();
	ASSERT_KINDOF(CWizardFormView, pView);
	
	((CWizardFormView*)pView)->CustomizeTabWizard (this); 

	ASSERT(m_pDlgInfoAr->GetSize () >0);
}

//-----------------------------------------------------------------------------
BOOL CTabWizard::CreateEx(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect, _In_ CWnd* pParentWnd, _In_ UINT nID)
{
	//devo chiamare quella del nonno! per non creare la combo del bottone delle tab
	return CBaseTabManager::CreateEx(dwExStyle, dwStyle, rect, pParentWnd, nID);
}

//-----------------------------------------------------------------------------
void CTabWizard::SetWizardButtons(DWORD dwFlags)
{
	CView* pView = GetFormView();
	ASSERT_KINDOF(CWizardFormView, pView);
	
	((CWizardFormView*)pView)->SetWizardButtons(dwFlags);
}

//-----------------------------------------------------------------------------
void CTabWizard::OnCustomize ()
{
	for (int i=GetDlgInfoArray()->GetSize ()-1; i>=0; i--)
	{
		ShowTab(i, FALSE);
	}
}

//-----------------------------------------------------------------------------
DlgInfoItem* CTabWizard::AddDialog
	(	
		CRuntimeClass* pDialogClass, 
		UINT nIDTitle, 
		int nOrdPos /*= -1*/,
		UINT nBeforeIDD /*= 0*/
	)
{
	DlgInfoItem* pItem = CTabManager::AddDialog(pDialogClass, nIDTitle, nOrdPos, nBeforeIDD);
	
	for (int i = GetDlgInfoArray()->GetSize () - 1; i >= 0; i--)
	{
		ShowTab(i, FALSE);
	}
	return pItem;
}

//--------------------------------------------------------------------------
BOOL CTabWizard::PreTranslateMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);
	ASSERT(m_hWnd != NULL);
	
	if(CTabManager::PreTranslateMessage(pMsg))
		return TRUE;

	// gestisco solo acceleratori di tab & selettori
	if (!pMsg || pMsg->message != WM_SYSKEYDOWN || (pMsg->wParam != VK_PRIOR && pMsg->wParam != VK_NEXT && pMsg->wParam != VK_F11 && (pMsg->wParam < 0x30 || pMsg->wParam > 0x5A)))
		return FALSE;

	// Attenzione LPARAM è inutilizzabile in questo contesto (vedi CTaskBuilderTabWnd::PreProcessSysKeyMessage)
	CString strPattern("&");
	strPattern += (TCHAR) pMsg->wParam;
			
	if (HandleAcceleratorInCtrl(strPattern, IDC_WIZARD_BACK)) return TRUE;
	if (HandleAcceleratorInCtrl(strPattern, IDC_WIZARD_NEXT)) return TRUE;
	if (HandleAcceleratorInCtrl(strPattern, IDCANCEL)) return TRUE;
	if (HandleAcceleratorInCtrl(strPattern, IDC_WIZARD_FINISH)) return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
int CTabWizard::GetFirstTab(int nStartPos)
{
	int idx = __super::GetFirstTab(nStartPos);
	if (idx < 0)
		idx = 0;

	return idx;
}

//--------------------------------------------------------------------------
BOOL CTabWizard::HandleAcceleratorInCtrl(const CString& strPattern, UINT nIDC)
{
	CString strCaption;

	CWizardFormView *pView = (CWizardFormView*) GetFormView ();
	ASSERT_VALID(pView);
	
	CWnd* pCtrl = pView->GetDlgItem (nIDC);
	if(pCtrl)
	{
		pCtrl->GetWindowText (strCaption);
		strCaption.MakeUpper ();
		if(strCaption.Find(strPattern)!=-1  && pCtrl->IsWindowEnabled ())
		{
			pView->SendMessage(WM_COMMAND, nIDC);
			return TRUE; 
		}
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTabWizard::GetUsedRect(CRect &rectUsed)
{
	if (m_pActiveDlg)
	{
		m_pActiveDlg->GetUsedRect(rectUsed);
	}
}


