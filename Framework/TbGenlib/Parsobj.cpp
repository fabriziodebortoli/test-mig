#include "stdafx.h"

#include <float.h>
#include <ctype.h>
#include <math.h>
#include <atlimage.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\spin.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\CMapi.h>
#include <TbGeneric\ISqlRecord.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\TBWebFriendlyMenu.h>
#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\BaseTileManager.h>
#include <TbGenlib\TBSplitterWnd.h>
#include <TbGenlib\HotlinkController.h>
#include <TbGenlib\TBLinearGauge.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGenlib\TBDockPane.h>
#include <TBNameSolver\ThreadContext.h>

#include "TBToolBar.h"
#include "baseapp.h"
#include "messages.h"
#include "basedoc.h"
#include "hlinkobj.h"
#include "HyperLink.h"
#include "tabcore.h"
#include "parsobj.h"		// base ParsedControl class
#include "parsedt.h"		// Edit Control
#include "parslbx.h"		// ListBox
#include "parscbx.h"		// ComboBox
#include "parsbtn.h"		// Push Button & Enumerations
#include "parsobjManaged.h"	// Wrappers to Managed Controls
#include "NumbererInfo.h"
#include "AutoExpressionMng.h"
#include "AutoExprDlg.h"
#include "TBSplitterWnd.h"
#include "TBBreadCrumb.h"
#include "TBParsedProgressBar.h"
#include "ParsObjManaged.h"		// Push Button & Enumerations

#include "commands.hrc"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Style for spin button (see resource file)
#define RADAR_STYLE				BS_OWNERDRAW
#define MAX_SPIN_WIDTH			18
#define MAX_SPIN_HEIGHT			20
#define INTER_CAPTION_GAP		2

static const CString szCommaSeparator = _T(",");
static const CString szNamespaceExtraChars = _T(".-_'");
static const CString szFileSystemExtraChars = _T("/\\:'");
static const CString szBaseAllowedLetters = _T("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
static const TCHAR szDefaultVersioning[] = _T("Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
static const TCHAR szParsedCtrlDefaultFamily[] = _T("DefaultBaseControl");



//-----------------------------------------------------------------------------
class ResetValueChanging
{
	int m_nCount = 0;
	int& m_nValueChanging;
public:
	ResetValueChanging(int& nValueChanging)
		: m_nValueChanging(nValueChanging)
	{
	}
	void ValueChanging()
	{
		m_nValueChanging++;
		m_nCount++;
	}
	~ResetValueChanging()
	{
		m_nValueChanging -= m_nCount;
	}

};
class CPushRoutingView
{
protected:
	CView* pOldRoutingView;
	_AFX_THREAD_STATE* pThreadState;
	CPushRoutingView* pOldPushRoutingView;

public:
	CPushRoutingView(CView* pNewRoutingView)
	{
		pThreadState = AfxGetThreadState();
		if (pThreadState != NULL)
		{
			pOldPushRoutingView = pThreadState->m_pPushRoutingView;
			pOldRoutingView = pThreadState->m_pRoutingView;
			pThreadState->m_pRoutingView = pNewRoutingView;
			pThreadState->m_pPushRoutingView = this;
		}
	}
	~CPushRoutingView()
	{
		if (pThreadState != NULL)
		{
			ASSERT(pThreadState->m_pPushRoutingView == this);
			pThreadState->m_pRoutingView = pOldRoutingView;
			pThreadState->m_pPushRoutingView = pOldPushRoutingView;
		}
	}
	void Pop()
	{
		ASSERT(pThreadState != NULL);
		if (pThreadState != NULL)
		{
			ASSERT(pThreadState->m_pPushRoutingView == this);
			pThreadState->m_pRoutingView = pOldRoutingView;
			pThreadState->m_pPushRoutingView = pOldPushRoutingView;
			pThreadState = NULL;
		}
	}
};
//-----------------------------------------------------------------------------
BOOL IsEmptyMessage(CParsedCtrl::MessageID id)
{
	return (id == CParsedCtrl::DUMMY) || (id == CParsedCtrl::EMPTY_MESSAGE);
}

//-----------------------------------------------------------------------------
static const IndexIDTag BASED_CODE IDFormatTable[] = {
	{ 0,	ID_FORMAT_STYLE_MENU_CMD_0},
	{ 1,	ID_FORMAT_STYLE_MENU_CMD_1},
	{ 2,	ID_FORMAT_STYLE_MENU_CMD_2},
	{ 3,	ID_FORMAT_STYLE_MENU_CMD_3},
	{ 4,	ID_FORMAT_STYLE_MENU_CMD_4},
	{ 5,	ID_FORMAT_STYLE_MENU_CMD_5},
	{ 6,	ID_FORMAT_STYLE_MENU_CMD_6},
	{ 7,	ID_FORMAT_STYLE_MENU_CMD_7},
	{ 8,	ID_FORMAT_STYLE_MENU_CMD_8},
	{ 9,	ID_FORMAT_STYLE_MENU_CMD_9},
	{ 10,	ID_FORMAT_STYLE_MENU_CMD_10},
	{ 11,	ID_FORMAT_STYLE_MENU_CMD_11},
	{ 12,	ID_FORMAT_STYLE_MENU_CMD_12},
	{ 13,	ID_FORMAT_STYLE_MENU_CMD_13},
	{ 14,	ID_FORMAT_STYLE_MENU_CMD_14},
	{ 15,	ID_FORMAT_STYLE_MENU_CMD_15},
	{ 16,	ID_FORMAT_STYLE_MENU_CMD_16},
	{ 17,	ID_FORMAT_STYLE_MENU_CMD_17},
	{ 18,	ID_FORMAT_STYLE_MENU_CMD_18},
	{ 19,	ID_FORMAT_STYLE_MENU_CMD_19},
	{ -1, 0 }
};

//=============================================================================
//	external exported Functions
//=============================================================================

// Ritorna la relazione di parentela fra due controls
//
//-----------------------------------------------------------------------------
int GetRelationship(CWnd* pWnd1, CWnd* pWnd2)
{
	CWnd* pParentWnd = pWnd1->GetParent();
	ASSERT(pParentWnd);

	if (pParentWnd->m_hWnd == pWnd2->m_hWnd)
		return PARENT_FOCUSED;

	if (pParentWnd->IsChild(pWnd2))
		return BROTHER_FOCUSED;

	CWnd* pGrandParentWnd = pParentWnd;

	while ((pGrandParentWnd = pGrandParentWnd->GetParent()) != NULL)
		if (
			pGrandParentWnd->IsChild(pWnd2) &&
			(
				pGrandParentWnd->IsKindOf(RUNTIME_CLASS(CFormView)) ||
				pGrandParentWnd->IsKindOf(RUNTIME_CLASS(CDialog)) ||
				pGrandParentWnd->IsKindOf(RUNTIME_CLASS(CBaseTabManager)) ||
				pGrandParentWnd->IsKindOf(RUNTIME_CLASS(CBaseTileGroup))
				)
			)
			return RELATIVE_FOCUSED;

	return FOREIGN_FOCUSED;
}

//
//	This function cast the pObject at a generic CParsedCtrl 
//	throught the correct derived class
//
//-----------------------------------------------------------------------------
CParsedCtrl* GetParsedCtrl(CObject* pObject)
{
	if (pObject == NULL)
		return NULL;

	//     is a class child of a CCOMBOBOX
	//
	CParsedCombo* pCombo = CParsedCombo::IsChildEditCombo(pObject);
	if (pCombo)
		return dynamic_cast<CParsedCtrl*>(pCombo);

	//     is a class "child" of a CGROUPBUTTON
	//
	if (pObject->IsKindOf(RUNTIME_CLASS(CChildButton)))
		return ((CChildButton*)pObject)->GetOwnerGroupBtn();

	return dynamic_cast<CParsedCtrl*>(pObject);
}

//-----------------------------------------------------------------------------
CParsedForm* GetParsedForm(CWnd* pWnd)
{
	if (!pWnd)
		return NULL;

	return dynamic_cast<CParsedForm*>(pWnd);
}

//---------------------------------------------------------------------------
CParsedForm* GetParentForm(CWnd* pWnd)
{
	do
	{
		CWnd* pParent = pWnd->GetParent();
		if (!pParent)
			break;

		CParsedForm* pForm = dynamic_cast<CParsedForm*>(pParent);
		if (pForm)
			return pForm;
		pWnd = pParent;
	} while (pWnd != NULL);

	return NULL;
}

//-----------------------------------------------------------------------------
void HideControlGroup
(
	CWnd*	pParentWnd,
	UINT	nGbxID,
	BOOL	bHide /*= TRUE*/
)
{
	ASSERT(pParentWnd);
	CWnd* pGbx = pParentWnd->GetDlgItem(nGbxID);

	if (!pGbx)
	{
		ASSERT(FALSE);
		return;
	}

	int nCmd = (bHide ? SW_HIDE : SW_SHOW);

	CRect rectGbx;
	CRect rectCtrl;
	CRect rectInter;

	pGbx->GetWindowRect(&rectGbx);

	// scorre tutte le child window della dialog
	CWnd* pCtrl = pParentWnd->GetWindow(GW_CHILD);
	for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
	{
		// salta la groupbox stessa, su questa applica una logica diversa
		if (pCtrl != pGbx)
		{
			pCtrl->GetWindowRect(&rectCtrl);

			if (
				rectCtrl.left <= rectGbx.left &&
				rectCtrl.top <= rectGbx.top &&
				rectCtrl.right >= rectGbx.right &&
				rectCtrl.bottom >= rectGbx.bottom
				)
				continue;	//il controllo contiene la GroupBox

			// se il control interseca (anche solo parzialmente) la groupbox, viene
			// nascosto (o visualizzato)
			if (rectInter.IntersectRect(&rectGbx, &rectCtrl))
			{
				// TODO dynamic_cast
				// se il control è un ParsedControl, usa il metodo di show specifico, per gestire anche
				// altri oggetti collegati (es.: hyperlink)
				if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
					((CParsedEdit*)pCtrl)->ShowCtrl(nCmd);
				else if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
					((CParsedStatic*)pCtrl)->ShowCtrl(nCmd);
				else if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
					((CParsedCombo*)pCtrl)->ShowCtrl(nCmd);
				else if (pCtrl->IsKindOf(RUNTIME_CLASS(CParsedListBox)))
					((CParsedListBox*)pCtrl)->ShowCtrl(nCmd);
				else
					pCtrl->ShowWindow(nCmd);
			}
		}
	}

	// se la Groupbox non è abilitata, non la tocca, significa che va sempre nascosta 
	if (pGbx->IsWindowEnabled())
		pGbx->ShowWindow(nCmd);
}

//---------------------------------------------------------------------------
void MoveControls(CWnd*	pParentWnd, CSize offset, ControlLinks* pControlLinks /*= NULL*/)
{
	if (pParentWnd == NULL || pParentWnd->m_hWnd == 0 || !::IsWindow(pParentWnd->m_hWnd))
		return;

	//---- disabilito momentaneamente il resize automatico dei control dei control che lo prevedono
	int i = 0;
	if (pControlLinks)
		for (i = 0; i < pControlLinks->GetSize(); i++)
		{
			ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(pControlLinks->GetAt(i));
			if (pwndChild)
				pwndChild->InitSizeInfo(NULL);
		}
	//----

	CRect  rcChild;
	for (CWnd* pwndChild = pParentWnd->GetWindow(GW_CHILD); pwndChild; pwndChild = pwndChild->GetNextWindow())
	{
		ASSERT(pwndChild->m_hWnd != pParentWnd->m_hWnd);

		pwndChild->GetWindowRect(rcChild);
		pParentWnd->ScreenToClient(rcChild);

		pwndChild->SetWindowPos(NULL, rcChild.left + offset.cx, rcChild.top + offset.cy, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
	}

	//---- Riabilito ridemensionamento automatico dei control che lo prevedono
	if (pControlLinks)
		for (i = 0; i < pControlLinks->GetSize(); i++)
		{
			ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(pControlLinks->GetAt(i));
			if (pwndChild)
				pwndChild->InitSizeInfo(pControlLinks->GetAt(i));
		}
	//----
}

//-----------------------------------------------------------------------------
void RedrawInnerControls(UINT	nID, CWnd*	pParentWnd)
{
	ASSERT_VALID(pParentWnd);
	if (!pParentWnd)
		return;

	CWnd* pGbx = pParentWnd->GetDlgItem(nID);
	if (!pGbx)
	{
		ASSERT(FALSE);
		return;
	}

	RedrawInnerControls(pGbx, pParentWnd);
}

void RedrawInnerControls(CWnd*	pGbx, CWnd*	pParentWnd /*= NULL*/)
{
	ASSERT_VALID(pGbx);
	if (!pParentWnd)
		pParentWnd = pGbx->GetParent();

	CRect rectGbx;
	CRect rectCtrl;
	CRect rectInter;

	pGbx->GetWindowRect(&rectGbx);

	// scorre tutte le child window della dialog
	CWnd* pCtrl = pParentWnd->GetWindow(GW_CHILD);
	while (pCtrl)
	{
		// salta la groupbox stessa, su questa applica una logica diversa
		if (pCtrl != pGbx)
		{
			pCtrl->GetWindowRect(&rectCtrl);
			// se il control interseca (anche solo parzialmente) la groupbox, viene ridisegnato
			if (pCtrl->IsWindowVisible() && rectInter.IntersectRect(&rectGbx, &rectCtrl))
			{
				//TODO ne funzionasse uno ... senza andare in ricorsione sigh !!!

				//pParentWnd->ScreenToClient(&rectCtrl);
				//pParentWnd->InvalidateRect(&rectCtrl);

				//VERIFY(pCtrl->RedrawWindow(NULL,NULL,RDW_INVALIDATE|RDW_UPDATENOW));
				//pCtrl->UpdateWindow();
				//pCtrl->SendMessage(WM_PAINT);

			}
		}
		pCtrl = pCtrl->GetNextWindow();
	}
}

//-----------------------------------------------------------------------------

BOOL SetAutomaticExpressionAndData(const CString& strIn, CString& strOut, DataObj* pData, BOOL bRunning)
{
	//in teoria andrebbe messa se si e' in stato di scheduler attivo
	//ma da qui non posso saperlo.
	//ASSERT(AfxGetBaseApp()->m_pSchedulerRunTasksDoc);

	if (strIn.IsEmpty())
		strOut.Empty();
	else
	{
		if (strIn.GetLength() > 1)
			strOut = strIn.Mid(1);
		else
			strOut.Empty();
	}

	if (IsAutoExpression(strIn))
	{
		if (bRunning)
		{
			if (!GetAutoExpressionValue(strOut, pData))
				return FALSE;
		}
		else
			pData->Clear();
	}
	else
	{
		pData->Assign(strOut);
		strOut.Empty();
	}

	return TRUE;
}

// per alcuni tipi di control (enumerativi e bottoni) va usato il form font, 
// in quanto il contenuto non dipende da quanto digitato dall'utente
// Stesso dicasi per tabber (è il font usato per le caption)
//-----------------------------------------------------------------------------
BOOL UseFormFont(CWnd* pWnd)
{
	//CString rtc(pWnd->GetRuntimeClass()->m_lpszClassName);
	//manca il riconoscimento di controlli ParsedStatic usati come label modificabili

	return
		pWnd->GetRuntimeClass() == RUNTIME_CLASS(CWnd) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CLabelStatic)) ||

		//pWnd->GetRuntimeClass() == RUNTIME_CLASS(CStatic) 	||
		//pWnd->GetRuntimeClass() == RUNTIME_CLASS(CBCGPStatic) ||

		//pWnd->IsKindOf(RUNTIME_CLASS(CEnumStatic))		||
		//pWnd->IsKindOf(RUNTIME_CLASS(CEnumCombo))			||
		//pWnd->IsKindOf(RUNTIME_CLASS(CEnumListBox))		||

		pWnd->IsKindOf(RUNTIME_CLASS(CParsedButton)) ||
		pWnd->IsKindOf(RUNTIME_CLASS(CParsedGroupBtn)) ||

		pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabManager));
}

//-----------------------------------------------------------------------------
void SetDefaultFontControl(CWnd* pWnd, CParsedCtrl* pCtrl)
{
	CFont* pF = UseFormFont(pWnd) ? AfxGetThemeManager()->GetFormFont() : AfxGetThemeManager()->GetControlFont();
	ASSERT_VALID(pF);

	CWnd* pParent = pWnd->GetParent();
	if (pParent)
	{
		CParsedForm* pForm = ::GetParsedForm(pParent);
		if (pForm)
		{
			if (pForm->GetOwnFont())
				pF = pForm->GetOwnFont();
		}
		else if (pParent->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
		{
			CTBPropertyGrid* pDlg = (CTBPropertyGrid*)pParent;
			if (pDlg->GetFont())
				pF = pDlg->GetFont();
		}
	}

	if (pWnd->m_hWnd)
		pWnd->SetFont(pF, FALSE);
	if (pCtrl)
		pCtrl->SetCtrlFont(pF, FALSE);
}


//==============================================================================
//	CParsedCtrlEvents class
//==============================================================================


//-----------------------------------------------------------------------------
void CParsedCtrlEvents::Fire(CObservable* pSender, EventType /*eType*/)
{
	HotKeyLinkObj* pHotLink = m_pControl->GetHotLink();
	if (pHotLink)
		pHotLink->FindDataRecord((DataObj*)pSender);
	m_pControl->NotifyToParent(EN_VALUE_CHANGED);
}

//-----------------------------------------------------------------------------
CObserverContext* CParsedCtrlEvents::GetContext() const
{
	if (m_pControl->GetDocument() && m_pControl->GetDocument()->m_ObserverContext.IsObserving(ON_CHANGED))
		return &m_pControl->GetDocument()->m_ObserverContext;
	return NULL;
}

//-----------------------------------------------------------------------------
void CParsedCtrlEvents::OnDeletingObservable(CObservable* pSender)
{
	ASSERT(pSender == m_pControl->m_pData);
	m_pControl->m_pData = NULL;
}

//------------------------------------------------------------------------------
void OnFindHotLinks(ControlLinks* pControlLinks)
{
	ASSERT_VALID(pControlLinks);
	if (!pControlLinks)
		return;
	for (int i = 0; i < pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		//ASSERT_VALID(pWnd);
		if (!pWnd) continue;

		CAbstractCtrl* pControl = dynamic_cast<CAbstractCtrl*>(pWnd);

		if (pControl)
			pControl->FindHotLink();

	}
}

//------------------------------------------------------------------------------
void OnUpdateControls(ControlLinks* pControlLinks, BOOL bParentIsVisible)
{
	ASSERT_VALID(pControlLinks);
	if (!pControlLinks)
		return;

	for (int i = 0; i < pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		//ASSERT_VALID(pWnd);
		if (!pWnd) continue;

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		if (pControl)
		{
			pControl->UpdateViewModel(bParentIsVisible);
		}
		else if (pWnd->IsKindOf(RUNTIME_CLASS(CExtButton)))	// CExtButton NON è un CParsedCtrl !!!!!
		{
			CExtButton* pB = (CExtButton*)pWnd;
			if (pB->ForceUpdateCtrlView())
			{
				pB->RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
			}
		}
		// TODOBRUNA scommentare piano piano
		else if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog)) && !pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)) && !pWnd->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		{
			((CParsedDialog*)pWnd)->OnUpdateControls(bParentIsVisible);
		}
		else
		{
			CTBGridControlObj* gridObj = dynamic_cast<CTBGridControlObj*>(pWnd);
			if (gridObj)
				gridObj->OnUpdateControls(bParentIsVisible);

			CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
			if (pGrid)
				pGrid->OnUpdateControls(bParentIsVisible);
		}
		//pWnd->Invalidate(); pWnd->UpdateWindow();
	}
}
//------------------------------------------------------------------------------
void OnModifyDataObjs(ControlLinks* pControlLinks, BOOL bModify)
{
	ASSERT_VALID(pControlLinks);
	if (!pControlLinks)
		return;

	for (int i = 0; i < pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// Se si tratta di ParsedControl resetta il flag di modificato del
		// dataobj associato.
		//can reset status: ci sono dei casi in cui la UpdateDataView non rinfresca il controllo (es. durante una SendMessage legata al change dello
		//stesso, per evitare deadlocks. In questo caso, non va impostato come Modified = FALSE, altrimenti non viene più aggiornato
		if (pControl && pControl->CanResetStatus())
		{
			pControl->SetDataModified(bModify);
			continue;
		}

		// Se si tratta di una griglia resetta il flag di modificato del DBT a 
		// causa della variazione eventiuale dello stato di readonly e demanda al
		// DBT il reset dei dataobj contenuti al suo interno
		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid)
		{
			pGrid->SetDataModified(bModify);
			continue;
		}
	}
}
//------------------------------------------------------------------------------
void OnResetDataObjs(ControlLinks* pControlLinks)
{
	OnModifyDataObjs(pControlLinks, FALSE);
}

// Nel caso non si volesse dare un DefaultFocus anche ai CTaskBuilderDockPane del documento (Germano & Silvano)
//-----------------------------------------------------------------------------
//BOOL InternalIsWindowVisible(CWnd* pWnd)
//{
//	CTaskBuilderDockPane* pTaskBuilderDockPane = dynamic_cast<CTaskBuilderDockPane*>(pWnd);
//	if (pTaskBuilderDockPane && !pTaskBuilderDockPane->IsVisible())
//		return FALSE;
//
//	CWnd* pParent = pWnd->GetParent();
//	if (!pParent)
//		return TRUE;
//
//	return InternalIsWindowVisible(pParent);
//}

//==============================================================================
//	ControlLinks class
//==============================================================================


//-----------------------------------------------------------------------------
int CompareWnd(CObject* arg1, CObject* arg2)
{
	CWnd*p1 = (CWnd*)arg1;
	CWnd*p2 = (CWnd*)arg2;

	return p1 == p2 ? 0 : (p1 < p2 ? 1 : -1);
}
//-----------------------------------------------------------------------------
ControlLinks::ControlLinks()
{
	SetCompareFunction(CompareWnd);
}


//-----------------------------------------------------------------------------
/*static*/ void ControlLinks::SetDefaultFocus(CWnd *pOwnerWnd, HWND* phWndFocus)
{
	// set focus on first edit control not disabled
	CWnd* pWnd = pOwnerWnd->GetWindow(GW_CHILD);
	CWnd* pWndFirst = pWnd;
	while (pWnd)
	{
		// Nel caso non si volesse dare un DefaultFocus anche ai CTaskBuilderDockPane del documento (Germano & Silvano)
		//if (InternalIsWindowVisible(pWnd))
		{
			// Si determina se e` possibile dare il fuoco al control trovato:
			if (pWnd->IsWindowEnabled()
				&& ((pWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE)
				&& ((pWnd->GetStyle() & WS_TABSTOP) == WS_TABSTOP))
			{
				CWnd* pFocus = CWnd::GetFocus();
				// per gestire correttamente il fuoco al control contenuto in un grid
				if (pFocus->GetSafeHwnd() == pWnd->GetSafeHwnd() && pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
					pWnd->SendMessage(WM_SETFOCUS, pFocus ? (WPARAM)pFocus->m_hWnd : NULL, NULL);
				else
					if (pWnd->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)) && ((CBaseTileGroup*)pWnd)->SetDefaultFocus())
						return;
					else
						pWnd->SetFocus();

				//if (phWndFocus)
				//	*phWndFocus = pWnd->GetSafeHwnd();
				return;
			}
		}

		pWnd = pOwnerWnd->GetNextDlgTabItem(pWnd);
		if (pWnd == pWndFirst)
		{
			// Nel caso non si volesse dare un DefaultFocus anche ai CTaskBuilderDockPane del documento (Germano & Silvano)
			//CTaskBuilderDockPane* pTaskBuilderDockPane = dynamic_cast<CTaskBuilderDockPane*>(pWnd);
			//if (!pWnd->IsWindowVisible() || pTaskBuilderDockPane && !pTaskBuilderDockPane->IsVisible())
			//	return;

			//se non ho trovato un campo valido per l'assegnazione
			//del fuoco, lo do alla view (serve per gestire correttamente gli acceleratori)
			pOwnerWnd->SetFocus();

			// if (phWndFocus)
			//	*phWndFocus = NULL;	@@@@@@@@
			return;
		}
	}
}

//-----------------------------------------------------------------------------
/*static*/ bool ControlLinks::HasFocusableControl(CWnd *pOwnerWnd)
{
	// set focus on first edit control not disabled
	CWnd* pWnd = pOwnerWnd->GetWindow(GW_CHILD);
	CWnd* pWndFirst = pWnd;
	while (pWnd)
	{
		// Si determina se e` possibile dare il fuoco al control trovato:
		if (pWnd->IsWindowEnabled()
			&& ((pWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE)
			&& ((pWnd->GetStyle() & WS_TABSTOP) == WS_TABSTOP))
		{
			return true;
		}

		CWnd* pNextWnd = pOwnerWnd->GetNextDlgTabItem(pWnd);
		if (pNextWnd == pWnd || pNextWnd == pOwnerWnd/*PERASSO: per qualche inspiegabile motivo, capita che la funzione ritorni il parent (caso di unico controllo managed presente nella dialog)*/)
		{
			return false;
		}
		pWnd = pNextWnd;
		if (pWnd == pWndFirst)
		{
			//non ho trovato un campo valido per l'assegnazione
			return false;
		}
	}
	return false;
}

//-----------------------------------------------------------------------------
void ControlLinks::Substitute(CWnd* pOldWnd, CWnd* pNewWnd)
{
	if (pOldWnd)
		for (int i = GetUpperBound(); i >= 0; i--)
		{
			CWnd* pWnd = GetAt(i);
			if (pWnd == pOldWnd)
			{
				RemoveAt(i);
				return;
			}
		}

	if (pNewWnd)
		Add(pNewWnd);
}

//-----------------------------------------------------------------------------
void ControlLinks::Remove(CWnd* pWndToDelete)
{
	if (!pWndToDelete)
		return;

	for (int i = GetUpperBound(); i >= 0; i--)
	{
		CWnd* pWnd = GetAt(i);
		if (pWnd == pWndToDelete)
		{
			//Rilevo il valore di Owns e setto m_bOwnElements a false in modo che la 
			//rimozione del control dall'array non deleta anche il control
			BOOL bOwns = IsOwnsElements();
			SetOwns(FALSE);

			RemoveAt(i);

			//risetto m_bOwnElements al valore precedente
			SetOwns(bOwns);
			return;
		}
	}
}

//-----------------------------------------------------------------------------
CParsedCtrl* ControlLinks::GetLinkedParsedCtrl(UINT nIDC)
{
	CParsedCtrl* pControl = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid)
		{
			// il grid non è un parsedcontrol
			if ((UINT)(pWnd->GetDlgCtrlID()) == nIDC)
			{
				ASSERT_TRACE(FALSE, _TB("GetLinkedParsedCtrl cannot returns Grid pointer; instead use GetWndLinkedCtrl\n"));
				return NULL;
			}

			// altrimenti chiedo al grid di guardare nei suoi
			pControl = pGrid->GetCurrentParsedCtrl(nIDC);

			if (pControl)
				return pControl;
		}
		else
		{
			pControl = GetParsedCtrl(pWnd);
			if (pControl && pControl->GetCtrlID() == (int)nIDC)
				return pControl;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* ControlLinks::GetLinkedParsedCtrl(DataObj* pDataObj)
{
	ASSERT_VALID(pDataObj);
	CParsedCtrl* pControl = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid)
		{
			// altrimenti chiedo al grid di guardare nei suoi
			pControl = pGrid->GetCurrentParsedCtrl(pDataObj);

			if (pControl)
				return pControl;
		}
		else
		{
			pControl = GetParsedCtrl(pWnd);
			if (pControl && pControl->GetCtrlData() == pDataObj)
				return pControl;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* ControlLinks::GetLinkedParsedCtrl(const CTBNamespace& aNS)
{
	CParsedCtrl* pControl = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid)
		{
			// mi è stato chiesto proprio il grid, mi spiace non posso
			if (pGrid->GetNamespace() == aNS)
				return NULL;

			pControl = pGrid->GetCurrentParsedCtrl(aNS);

			if (pControl)
				return pControl;
		}
		else
		{
			pControl = GetParsedCtrl(pWnd);
			if (pControl && pControl->GetNamespace() == aNS)
				return pControl;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* ControlLinks::GetWndLinkedCtrl(UINT nIDC)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		if (pWnd)
		{
			CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);
			if (pCtrl)
			{
				if (pCtrl->GetCtrlID() == nIDC)
					return pWnd;
			}
			else
			{
				if (pWnd->GetDlgCtrlID() == nIDC)
					return pWnd;
			}
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* ControlLinks::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);
		if (pControl && pControl->GetNamespace() == aNS)
			return pWnd;

		//ricerca gli extbutton (che non sono parsed ctrl)
		if (
			pWnd &&
			pWnd->IsKindOf(RUNTIME_CLASS(CExtButton)) &&
			((CExtButton*)pWnd)->GetExtInfo()
			)
		{
			CExtButton* pButton = ((CExtButton*)pWnd);
			if (pButton && pButton->GetExtInfo()->GetInfoOSL()->m_Namespace == aNS)
				return pWnd;
		}

		// devo stare attenta quanto meno al tipo grid che non è un ParsedCtrl
		CGridControlObj* pGridCtrl = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGridCtrl && pGridCtrl->GetNamespace() == aNS)
			return pWnd;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* ControlLinks::GetWndLinkedCtrlByName(const CString& sName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);
		if (pControl && pControl->GetNamespace().GetObjectName() == sName)
			return pWnd;

		//ricerca gli extbutton (che non sono parsed ctrl)
		if (
			pWnd &&
			pWnd->IsKindOf(RUNTIME_CLASS(CExtButton)) &&
			((CExtButton*)pWnd)->GetExtInfo() &&
			((CExtButton*)pWnd)->GetExtInfo()->GetInfoOSL()->m_Namespace.GetObjectName() == sName
			)
			return pWnd;

		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid && pGrid->GetNamespace().GetObjectName() == sName)
			return pWnd;
	}
	return NULL;
}

//---------------------------------------------------------------------------------------------
CGridControl* ControlLinks::GetBodyEdits(int* pnStartIdx/*= NULL*/)
{
	for (int i = max(0, (pnStartIdx ? *pnStartIdx : 0)); i <= GetUpperBound(); i++)
	{
		CWnd* pWnd = GetAt(i);
		if (pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
		{
			if (pnStartIdx) *pnStartIdx = i;
			return (CGridControl*)pWnd;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CGridControl* ControlLinks::GetBodyEdits(const CTBNamespace& aNS)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CWnd* pW = GetAt(i);
		if (pW->IsKindOf(RUNTIME_CLASS(CGridControl)))
		{
			CGridControl* pBody = (CGridControl*)pW;
			if (pBody && pBody->GetNamespace() == aNS)
				return pBody;
		}
	}
	return NULL;
}

//---------------------------------------------------------------------------------------------
BOOL ControlLinks::SetControlValue(UINT nIDC, const DataObj& val)
{
	CParsedCtrl* pCtrl = GetLinkedParsedCtrl(nIDC);
	if (!pCtrl)
		return FALSE;

	pCtrl->SetValue(val);

	pCtrl->UpdateCtrlStatus();
	pCtrl->UpdateCtrlView();

	return TRUE;
}

//-----------------------------------------------------------------------------
int ControlLinks::FindIndexByHWnd(HWND h)
{
	for (int i = 0; i < GetSize(); i++)
	{
		CWnd* pW = GetAt(i);

		if (pW->m_hWnd == h)
		{
			return i;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
int ControlLinks::FindIndexByIDC(UINT nIDC)
{
	ASSERT(nIDC);
	for (int i = 0; i < GetSize(); i++)
	{
		CWnd* pW = GetAt(i);

		if (pW->GetDlgCtrlID() == nIDC)
		{
			return i;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
void ControlLinks::AlignToTabOrder(CWnd* pParentWnd)
{
	ASSERT_TRACE(FALSE, "AlignToTabOrder NON funziona ancora");

	ControlLinks ar; ar.SetOwns(FALSE);
	SetOwns(FALSE);

	for (CWnd* pW = pParentWnd->GetWindow(GW_CHILD); pW; pW = pW->GetNextWindow())
	{
		ASSERT(pW->m_hWnd != pParentWnd->m_hWnd);

		//Bug1 : MI PERDO controlli che hanno parent differente: ad esempio i Panel
		int idx = FindIndexByHWnd(pW->m_hWnd);
		if (idx > -1)
		{
			ar.Add(this->GetAt(idx));

			this->RemoveAt(idx);
		}
	}

	this->Append(ar);	//Append al posto di Copy risolve Bug1 ma stravolge eventuale logica di "TabOrder" extra panel
	SetOwns(TRUE);
}

//-----------------------------------------------------------------------------
void ControlLinks::AddToNotifyMap(CWnd* pParsedCtrl)
{
	m_NotifyMap.Add(pParsedCtrl);
}

//-----------------------------------------------------------------------------
void ControlLinks::RemoveToNotifyMap(CWnd* pOwnerWnd)
{
	for (int i = m_NotifyMap.GetUpperBound(); i >= 0; i--)
	{
		CWnd* pWnd = (CWnd*)m_NotifyMap.GetAt(i);
		if (pWnd == pWnd)
		{
			m_NotifyMap.RemoveAt(i);
			return;
		}
	}
}

//-----------------------------------------------------------------------------
void ControlLinks::OnPrepareAuxData()
{
	for (int i = 0; i <= m_NotifyMap.GetUpperBound(); i++)
	{
		CWnd* pWnd = (CWnd*)m_NotifyMap.GetAt(i);
		CParsedCtrl* pParsedCtrl = GetParsedCtrl(pWnd);
		if (pParsedCtrl)
			pParsedCtrl->OnPrepareAuxData();
	}
}

//-----------------------------------------------------------------------------
void ControlLinks::AddToStaticsMap(CWnd* pWnd)
{
	m_StaticsMap.Add(pWnd);
}

//-----------------------------------------------------------------------------
CWnd* GetWndLinkedCtrlByName(ControlLinks* pControlLinks, const CString& sName)
{
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetWndLinkedCtrlByName(sName);
}

//-----------------------------------------------------------------------------
CWnd* GetWndLinkedCtrl(ControlLinks* pControlLinks, const CTBNamespace& aNS)
{
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetWndLinkedCtrl(aNS);
}

//-----------------------------------------------------------------------------
CWnd* GetWndLinkedCtrl(ControlLinks* pControlLinks, UINT nIDC)
{
	// Questo metodo permette di risalire alla CWnd di un control IDC, funziona anche per i bodyedit
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetWndLinkedCtrl(nIDC);
}

//-----------------------------------------------------------------------------
CParsedCtrl* GetLinkedParsedCtrl(ControlLinks* pControlLinks, UINT nIDC)
{
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetLinkedParsedCtrl(nIDC);
}

//-----------------------------------------------------------------------------
CParsedCtrl* GetLinkedParsedCtrl(ControlLinks* pControlLinks, const CTBNamespace& aNS)
{
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetLinkedParsedCtrl(aNS);
}

//-----------------------------------------------------------------------------
CParsedCtrl* GetLinkedParsedCtrl(ControlLinks* pControlLinks, DataObj* pDataObj)
{
	// it searches the parsed control associated to a specific DataObj
	ASSERT_VALID(pControlLinks);
	return pControlLinks->GetLinkedParsedCtrl(pDataObj);
}

//-----------------------------------------------------------------------------
CParsedCtrl* CreateControl
(
	const CString&	sName,
	DWORD			dwStyle,
	const CRect&	rect,
	CWnd*			pWnd,
	CBaseDocument*	pDocument,
	ControlLinks*	pControlLinks,
	UINT			nIDC,
	SqlRecord*		pRecord,
	DataObj*		pDataObj,
	CRuntimeClass*	pParsedCtrlClass,
	void*			pHotKeyLink,			/* = NULL */
	BOOL			bIsARuntimeClass,		/* = FALSE */
	UINT			nBtnID					/* = BTN_DEFAULT */
)
{
	if (pControlLinks)
		CHECK_UNIQUE_NAME(pControlLinks, sName);

	CParsedCtrl* pControl = GetParsedCtrl(pParsedCtrlClass->CreateObject());

	ASSERT(pControl);
	pControl->Attach(nBtnID);
	//if (!pControl->CheckDataObjType(pDataObj))
	//{
	//	delete pControl;
	//	return NULL;
	//}
	pControl->Attach(pDataObj);
	pControl->AttachDocument(pDocument);

	// Se esiste attacca l'hotlink che diventa proprieta`
	// del control e quindi viene disallocato dal control stesso
	HotKeyLinkObj* pHKL = NULL;
	if (pHotKeyLink)
	{

		if (bIsARuntimeClass)
			pHKL = (HotKeyLinkObj*)((CRuntimeClass*)pHotKeyLink)->CreateObject();
		else
			pHKL = (HotKeyLinkObj*)pHotKeyLink;

		ASSERT(pHKL->IsKindOf(RUNTIME_CLASS(HotKeyLinkObj)));

		pHKL->AttachDocument(pDocument);

		pControl->AttachHotKeyLink(pHKL, bIsARuntimeClass);
	}

	// pRecord can be NULL for DataObj not conneted to any SqlRecord
	// use default len and precision
	if (pDataObj)
	{
		pControl->SetCtrlMaxLen(pDataObj->GetColumnLen(), FALSE);
		if (pRecord)
			pControl->AttachRecord(pRecord);
	}

	if (pHKL)
	{
		pHKL->PreCreateOwnerCtrl(pControl, dwStyle);
	}

	if (!pControl->Create(dwStyle, rect, pWnd, nIDC))
	{
		delete pControl;

		TRACE("CParsedCtrl* AddLinkAndCreateControl: fail to create control %d\n", nIDC);
		ASSERT(FALSE);
		return NULL;
	}

	if (GetParsedForm(pWnd))
		GetParsedForm(pWnd)->SetChildControlNamespace(sName, pControl);

	if (pHKL)
	{
		pHKL->DoOnCreatedOwnerCtrl();
	}

	if (pControlLinks)
		pControlLinks->Add(pControl->GetCtrlCWnd());
	return pControl;
}

//=============================================================================
//					CSplittedForm
//=============================================================================
class TB_EXPORT CSplittedFormFrame : public CBCGPFrameWnd
{
	DECLARE_DYNCREATE(CSplittedFormFrame)
public:
	CSplittedFormFrame() {}
};

IMPLEMENT_DYNCREATE(CSplittedFormFrame, CBCGPFrameWnd)

//-----------------------------------------------------------------------------
CSplittedForm::CSplittedForm(CFrameWnd* pFrame)
	:
	m_pFrame(pFrame),
	m_pForm(NULL)
{
}

//-----------------------------------------------------------------------------
CSplittedForm::CSplittedForm(CDialog* pDialog)
	:
	m_pFrame(NULL),
	m_pForm(pDialog)
{
}
//-----------------------------------------------------------------------------
CSplittedForm::~CSplittedForm()
{
	if (m_Splitters.GetCount() > 0)
		for (int i = m_Splitters.GetCount() - 1; i >= 0; i--)
		{
			CTaskBuilderSplitterWnd* pWnd = (CTaskBuilderSplitterWnd*)m_Splitters.GetAt(i);
			delete pWnd;
		}

	m_Splitters.RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL CSplittedForm::UseSplitters()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CSplittedForm::CreateFrame()
{
	// Create the frame window with "this" as the parent
	m_pFrame = new CSplittedFormFrame();
	if (!m_pFrame->Create(NULL, _T(""), WS_CHILD | WS_VISIBLE, CRect(0, 0, 1, 1), m_pForm))
	{
		ASSERT_TRACE(FALSE, "CSplittedForm::CreateFrame: cannot Create auxiliary frame for splitters management!")
			return FALSE;
	}

	CParsedForm* pForm = dynamic_cast<CParsedDialog*>(m_pForm);
	ASSERT(pForm);

	CCreateContext context;
	context.m_pNewViewClass = m_pForm->GetRuntimeClass();
	context.m_pCurrentDoc = pForm->GetDocument();
	context.m_pNewDocTemplate = NULL;
	context.m_pLastView = NULL;

	OnCreateSplitters(&context);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CSplittedForm::CreateSplitterEnvironment(CWnd* pParentWnd)
{
	if (!UseSplitters() || !m_pForm->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		return TRUE;

	if (!m_pFrame && !CreateFrame())
		return FALSE;

	CTaskBuilderSplitterWnd* pSplitter = GetSplitter();
	if (!pSplitter)
		return TRUE;

	m_pForm->SetParent(pParentWnd);

	CRect rectParent;
	m_pForm->GetWindowRect(&rectParent);
	// Move the splitter
	m_pForm->ScreenToClient(&rectParent);
	m_pFrame->ShowWindow(SW_SHOW);

	pSplitter->ShowWindow(SW_SHOW);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CSplittedForm::DoResize()
{
	if (m_pFrame && m_pForm)
	{
		CRect rect;
		m_pForm->GetClientRect(rect);
		m_pFrame->MoveWindow(0, 0, rect.Width(), rect.Height(), TRUE);
	}
}

//-----------------------------------------------------------------------------
CFrameWnd*	CSplittedForm::GetFrame()
{
	return m_pFrame;
}

//-----------------------------------------------------------------------------
CTaskBuilderSplitterWnd* CSplittedForm::GetSplitter(int nIdx)
{
	if (nIdx < 0 || nIdx >= m_Splitters.GetCount())
		return NULL;

	return (CTaskBuilderSplitterWnd*)m_Splitters.GetAt(nIdx);
}

//-----------------------------------------------------------------------------
CTaskBuilderSplitterWnd* CSplittedForm::GetSplitter()
{
	return GetSplitter(0);
}

//----------------------------------------------------------------------------
CTaskBuilderSplitterWnd* CSplittedForm::CreateSplitter(int nRows, int nCols, CWnd* pParent /*NULL*/, UINT nID)
{
	return CreateSplitter(RUNTIME_CLASS(CTaskBuilderSplitterWnd), nRows, nCols, pParent, nID);
}

//----------------------------------------------------------------------------
CTaskBuilderSplitterWnd* CSplittedForm::CreateSplitter(CRuntimeClass* pClass, int nRows, int nCols, CWnd* pParent /*NULL*/, UINT nID)
{
	CObject* pObject = pClass->CreateObject();
	if (!pObject->IsKindOf(RUNTIME_CLASS(CTaskBuilderSplitterWnd)))
	{
		ASSERT_TRACE(FALSE, "Splitter class is not derived from CTaskBuilderSplitterWnd!");
		delete pObject;
		return FALSE;
	}

	CTaskBuilderSplitterWnd* pSplitter = (CTaskBuilderSplitterWnd*)pObject;
	pSplitter->CreateStatic(pParent ? pParent : m_pFrame, nRows, nCols, WS_CHILD | WS_VISIBLE, nID);
	m_Splitters.Add(pSplitter);
	return pSplitter;
}

//=============================================================================
//			Class CColoredControl implementation
//=============================================================================

//-----------------------------------------------------------------------------
CColoredControl::CColoredControl(CWnd* pWnd)
	:
	m_pWnd(pWnd),
	m_pBrush(NULL),
	m_crText(AfxGetThemeManager()->GetEnabledControlForeColor()),
	m_crBkgnd(AfxGetThemeManager()->GetBackgroundColor()),
	m_bColored(FALSE),
	m_bCustomDraw(FALSE),
	m_crLine(AfxGetThemeManager()->GetColoredControlBorderColor()),
	m_nSizeLinePen(1)
{
	ASSERT_VALID(pWnd);

	SetBkgColor(m_crBkgnd, FALSE, FALSE);
}

//-----------------------------------------------------------------------------
CColoredControl::~CColoredControl()
{
	if (m_pBrush && m_pBrush != AfxGetThemeManager()->GetBackgroundColorBrush())
	{
		m_pBrush->DeleteObject();
		delete m_pBrush;
		m_pBrush = NULL;
	}
}

//-----------------------------------------------------------------------------
void CColoredControl::SetColored(BOOL b, BOOL bRedraw/* = TRUE*/)
{
	if (m_bColored == b)
		return;

	if (m_bColored && !b)
	{
		m_bColored = FALSE;

		if (m_pBrush && m_pBrush != AfxGetThemeManager()->GetBackgroundColorBrush())
		{
			m_pBrush->DeleteObject();
			delete m_pBrush;
		}
		m_pBrush = NULL;

		if (bRedraw && m_pWnd && ::IsWindow(m_pWnd->m_hWnd))
			m_pWnd->RedrawWindow();

		return;
	}

	SetBkgColor(m_crBkgnd, bRedraw, TRUE);
}

//-----------------------------------------------------------------------------
HBRUSH CColoredControl::GetBkgBrushHandle()
{
	if (m_pBrush)
		return (HBRUSH)*m_pBrush;
	return (HBRUSH)(m_pWnd->IsWindowEnabled() ?
		AfxGetThemeManager()->GetEnabledControlBkgColorBrush()->GetSafeHandle()
		:
		AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle());
}

//-----------------------------------------------------------------------------
CBrush* CColoredControl::GetBkgBrush()
{
	BOOL bEnable = m_pWnd->IsWindowEnabled();

	if (m_pBrush)
		return m_pBrush;

	return const_cast<CBrush*>
		(
			bEnable ?
			AfxGetThemeManager()->GetEnabledControlBkgColorBrush()
			:
			AfxGetThemeManager()->GetBackgroundColorBrush()
			);
}

//-----------------------------------------------------------------------------
HBRUSH CColoredControl::CtlColor(CDC* pDC, UINT nCtlColor)
{
	pDC->SetTextColor(m_crText);    // text

	if (m_pBrush != NULL && m_bColored)
	{
		pDC->SetBkColor(m_crBkgnd);    // text background

		return (HBRUSH)*m_pBrush;
	}

	CWnd* pParent = m_pWnd->GetParent();
	CParsedForm* pForm = ::GetParsedForm(pParent);

	if (
		m_pWnd->IsKindOf(RUNTIME_CLASS(CStatic))
		||
		m_pWnd->IsKindOf(RUNTIME_CLASS(CButton))
		)
	{
		COLORREF crBkgnd = AfxGetThemeManager()->GetBackgroundColor();
		HBRUSH hBrush = (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();

		if (pForm && pForm->GetBackgroundBrush())
		{
			crBkgnd = pForm->GetBackgroundColor();
			hBrush = (HBRUSH)pForm->GetBackgroundBrush()->GetSafeHandle();
		}

		pDC->SetBkColor(crBkgnd);
		return hBrush;
	}

	BOOL bReadOnly = FALSE;
	if (m_pWnd->IsKindOf(RUNTIME_CLASS(CBCGPEdit)))
	{
		bReadOnly = (m_pWnd->GetStyle() & ES_READONLY) != 0;
	}

	if (
		m_pWnd->IsWindowEnabled() &&
		!bReadOnly
		)
	{
		CWnd* pFocused = CWnd::GetFocus();
		if (AfxGetThemeManager()->IsFocusedControlBkgColorEnabled() && pFocused && pFocused->m_hWnd == m_pWnd->m_hWnd)
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetFocusedControlBkgColor());
			return (HBRUSH)AfxGetThemeManager()->GetFocusedControlBkgColorBrush()->GetSafeHandle();
		}
		else
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetEnabledControlBkgColor());
			return (HBRUSH)AfxGetThemeManager()->GetEnabledControlBkgColorBrush()->GetSafeHandle();
		}
	}
	else
	{
		if (m_pWnd->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
			pDC->SetTextColor(AfxGetThemeManager()->GetEnabledControlForeColor());
		else if (m_pWnd->IsKindOf(RUNTIME_CLASS(CLinkEdit)))
			pDC->SetTextColor(AfxGetThemeManager()->GetHyperLinkForeColor());
		else
			pDC->SetTextColor(AfxGetThemeManager()->GetDisabledControlForeColor());

		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		{
			CBaseTileDialog* pTile = (CBaseTileDialog*)pParent;
			pDC->SetBkColor(pTile->GetBackgroundColor());
			return (HBRUSH)pTile->GetBackgroundBrush()->GetSafeHandle();
		}
		else
		{
			pDC->SetBkColor(AfxGetThemeManager()->GetBackgroundColor());
			return (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();
		}
	}
}

//-----------------------------------------------------------------------------
void CColoredControl::SetBkgColor(COLORREF crBkg, BOOL bRedraw/* = */, BOOL bColored /*= TRUE*/)
{
	m_bColored = bColored;

	if (m_crBkgnd == crBkg && m_pBrush)
		return;

	m_crBkgnd = crBkg;

	if (m_pBrush && m_pBrush != AfxGetThemeManager()->GetBackgroundColorBrush())
	{
		m_pBrush->DeleteObject();
		delete m_pBrush;
	}
	m_pBrush = NULL;

	// small optimization: if the selected color is the normal background brush, avoid to allocate a new one
	if (m_crBkgnd == AfxGetThemeManager()->GetBackgroundColor())
		m_pBrush = (CBrush*)AfxGetThemeManager()->GetBackgroundColorBrush();
	else
		m_pBrush = new CBrush(crBkg);

	if (bRedraw && m_pWnd && ::IsWindow(m_pWnd->m_hWnd))
		m_pWnd->RedrawWindow();
}

//-----------------------------------------------------------------------------
void CColoredControl::SetTextColor(COLORREF crText, BOOL bRedraw/* = */, BOOL bColored /*= TRUE*/)
{
	m_bColored = bColored;

	m_crText = crText;

	if (bRedraw && m_pWnd && ::IsWindow(m_pWnd->m_hWnd))
		m_pWnd->RedrawWindow();
}

//-----------------------------------------------------------------------------
BOOL CColoredControl::EraseBkgnd(CDC* pDC)
{
	COLORREF crBkg = 0;

	if (m_pBrush != NULL && m_bColored)
		crBkg = m_crBkgnd;
	else
	{
		if (m_pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
			crBkg = AfxGetThemeManager()->GetBackgroundColor();
		else
		{
			BOOL bReadOnly = FALSE;
			if (m_pWnd->IsKindOf(RUNTIME_CLASS(CBCGPEdit)))
			{
				bReadOnly = (m_pWnd->GetStyle() & ES_READONLY) != 0;
			}

			if (
				m_pWnd->IsWindowEnabled() &&
				!bReadOnly
				)
				crBkg = AfxGetThemeManager()->GetEnabledControlBkgColor();
			else
				crBkg = AfxGetThemeManager()->GetBackgroundColor();
		}
	}
	pDC->SetBkColor(crBkg);

	CRect rect;
	m_pWnd->GetClientRect(&rect);
	pDC->FillSolidRect(&rect, crBkg);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CColoredControl::SetLineColor(COLORREF crBorder, int nSizePen /*= 1*/)
{
	m_bCustomDraw = TRUE;

	m_crLine = crBorder;
	m_nSizeLinePen = nSizePen;

	if (m_pWnd && ::IsWindow(m_pWnd->m_hWnd))
		m_pWnd->RedrawWindow();
}

//-----------------------------------------------------------------------------
void CColoredControl::DrawHLine(CDC& dc, int y, int left, int right, COLORREF cr, BOOL bHighlight)
{
	CPen pen(PS_SOLID, m_nSizeLinePen, cr);
	dc.SelectObject(&pen);

	dc.MoveTo(left, y);
	dc.LineTo(right, y);

	if (bHighlight)
	{
		CPen penTop(PS_SOLID, 1, AfxGetThemeManager()->GetColoredControlLineColor());
		dc.SelectObject(&penTop);

		dc.MoveTo(left, y + m_nSizeLinePen);
		dc.LineTo(right, y + m_nSizeLinePen);
	}
}

//-----------------------------------------------------------------------------
void CColoredControl::DrawVLine(CDC& dc, int x, int top, int bottom, COLORREF cr, BOOL bHighlight, BOOL bLeft)
{
	CPen pen(PS_SOLID, m_nSizeLinePen, cr);
	dc.SelectObject(&pen);

	dc.MoveTo(x, top);
	dc.LineTo(x, bottom);

	if (bHighlight)
	{
		CPen penH(PS_SOLID, 1, AfxGetThemeManager()->GetColoredControlLineColor());
		dc.SelectObject(&penH);

		x = bLeft ? x + m_nSizeLinePen : x - m_nSizeLinePen;

		dc.MoveTo(x, top + m_nSizeLinePen);
		dc.LineTo(x, bottom - m_nSizeLinePen);
	}
}

//-----------------------------------------------------------------------------
BOOL CColoredControl::DoStaticEraseBkgnd(CDC*)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void CColoredControl::PopulateColorDescription(CWndColoredObjDescription* pDesc)
{
	if (m_bColored)
	{
		if (pDesc->m_crBkgColor != GetBkgColor() && GetBkgColor() != AfxGetThemeManager()->GetBackgroundColor())/*se e' diverso dal default*/
		{
			pDesc->m_crBkgColor = GetBkgColor();
			pDesc->SetUpdated(&pDesc->m_crBkgColor);
		}
		if (pDesc->m_crTextColor != GetTextColor() && GetTextColor() != AfxGetThemeManager()->GetEnabledControlForeColor())/*se e' diverso dal default*/
		{
			pDesc->m_crTextColor = GetTextColor();
			pDesc->SetUpdated(&pDesc->m_crTextColor);
		}
	}
}

//=============================================================================
//			Class CLinkButton implementation
//=============================================================================
IMPLEMENT_DYNAMIC(CLinkButton, CButton)

BEGIN_MESSAGE_MAP(CLinkButton, CButton)

	//{{AFX_MSG_MAP(CLinkButton)
	ON_WM_LBUTTONUP()
	ON_WM_LBUTTONDOWN()
	ON_WM_MOUSEMOVE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CLinkButton::CLinkButton(CWnd* pCWnd)
	:
	m_pOwner(pCWnd),
	m_bTriState(FALSE),
	m_WherePressed(OUTSIDE),
	m_bCaptured(FALSE),
	m_IsHKL(HKLButton_NO)
{
	ASSERT(pCWnd);
}

// LoadBitmaps will load in one, two, three or all four bitmaps
// returns TRUE if all specified images are loaded
//-----------------------------------------------------------------------------
BOOL CLinkButton::LoadBitmaps(UINT nBitmapResourceID)
{
	// delete old bitmaps (if present)
	m_BitmapUp.DeleteObject();
	m_BitmapDownSideUp.DeleteObject();
	m_BitmapDownSideDown.DeleteObject();
	m_BitmapDisabled.DeleteObject();

	if (nBitmapResourceID == BTN_DOUBLE_ID)
	{
		m_IsHKL = HKLButton_YES;

		if (!m_BitmapUp.LoadBmpOrPng(TBIcon(szIconHotLink, CONTROL)))
		{
			TRACE0("Failed to load bitmap for normal image\n");
			return FALSE;   // need this one image
		}

		if (!m_BitmapDownSideUp.LoadBmpOrPng(TBIcon(szIconHotLinkUp, CONTROL)))
			m_BitmapDownSideUp.LoadBmpOrPng(TBIcon(szIconHotLink, CONTROL));

		if (!m_BitmapDownSideDown.LoadBmpOrPng(TBIcon(szIconHotLinkDown, CONTROL)))
			m_BitmapDownSideDown.LoadBmpOrPng(TBIcon(szIconHotLinkUp, CONTROL));
		else
			m_bTriState = TRUE;

		if (!m_BitmapDisabled.LoadBmpOrPng(TBIcon(szIconHotLinkDisabled, CONTROL)))
			m_BitmapDisabled.LoadBmpOrPng(TBIcon(szIconHotLink, CONTROL));

		return TRUE;
	}

	if (!m_BitmapUp.LoadBitmap(nBitmapResourceID))
	{
		TRACE0("Failed to load bitmap for normal image\n");
		return FALSE;   // need this one image
	}

	if (!m_BitmapDownSideUp.LoadBitmap(nBitmapResourceID + 1))
		m_BitmapDownSideUp.LoadBitmap(nBitmapResourceID);

	if (!m_BitmapDownSideDown.LoadBitmap(nBitmapResourceID + 2))
		m_BitmapDownSideDown.LoadBitmap(nBitmapResourceID + 1);
	else
		m_bTriState = TRUE;

	if (!m_BitmapDisabled.LoadBitmap(nBitmapResourceID + 3))
		m_BitmapDisabled.LoadBitmap(nBitmapResourceID);

	return TRUE;
}

// SizeToContent will resize the button to the size of the bitmap
//-----------------------------------------------------------------------------
void CLinkButton::SizeToContent()
{
	if (AfxIsRemoteInterface())
		return;

	ASSERT(m_BitmapUp.m_hObject != NULL);
	CSize bitmapSize;
	BITMAP bmInfo;
	VERIFY(m_BitmapUp.GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));
	VERIFY(SetWindowPos(NULL, -1, -1, bmInfo.bmWidth, bmInfo.bmHeight,
		SWP_NOMOVE | SWP_NOZORDER | SWP_NOREDRAW | SWP_NOACTIVATE));
}

// Draw the appropriate bitmap
//-----------------------------------------------------------------------------
void CLinkButton::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	ASSERT(lpDIS != NULL);
	// must have at least the first bitmap loaded before calling DrawItem

	//5457 BCG controls
	if (m_BitmapUp.m_hObject == NULL)
	{
		/*__super::DrawItem*/(lpDIS);
		return;
	}

	// use the main bitmap for up, the selected bitmap for down
	CWalkBitmap* pBitmap = &m_BitmapUp;

	UINT state = lpDIS->itemState;
	if (
		(state & ODS_SELECTED) &&
		(m_BitmapDownSideUp.m_hObject != NULL) &&
		(m_BitmapDownSideDown.m_hObject != NULL)
		)
	{
		CPoint ptMousePos;
		CRect	rect;

		GetWindowRect(rect);
		GetCursorPos(&ptMousePos);
		if (rect.PtInRect(ptMousePos))
		{
			ScreenToClient(&ptMousePos);
			switch (WhereDown(ptMousePos))
			{
			case UPSIDE:
			{
				pBitmap = &m_BitmapDownSideUp;
				break;
			}
			case DOWNSIDE:
			{
				pBitmap = &m_BitmapDownSideDown;
				break;
			}
			}
		}
	}

	if ((state & ODS_DISABLED) && m_BitmapDisabled.m_hObject != NULL)
		pBitmap = &m_BitmapDisabled;   // image for disabled

	// draw the whole button
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	CDC memDC;
	memDC.CreateCompatibleDC(pDC);
	CBitmap* pOld = memDC.SelectObject(pBitmap);
	if (pOld == NULL)
	{
		memDC.DeleteDC();
		return;     // destructors will clean up
	}


	CRect rect;
	rect.CopyRect(&lpDIS->rcItem);
	pDC->BitBlt(rect.left, rect.top, rect.Width(), rect.Height(), &memDC, 0, 0, SRCCOPY);
	memDC.SelectObject(pOld);
	memDC.DeleteDC();
}

//-----------------------------------------------------------------------------
CLinkButton::MousePosition
CLinkButton::WhereDown(CPoint ptMousePos)
{
	CRect rect;
	GetClientRect(rect);

	CRect rectUpSide(rect);
	CRect rectDownSide(rect);

	rectUpSide.bottom = rectUpSide.top + rect.Height() / 2;
	rectDownSide.top = rectUpSide.bottom;

	// call overridable HotKeyLinkObj methods
	if (rectUpSide.PtInRect(ptMousePos))	return UPSIDE;
	if (rectDownSide.PtInRect(ptMousePos))	return DOWNSIDE;

	return OUTSIDE;
}

//-----------------------------------------------------------------------------
void CLinkButton::OnLButtonDown(UINT, CPoint ptMousePos)
{
	m_WherePressed = WhereDown(ptMousePos);

	SetCapture();
	m_bCaptured = TRUE;

	SetState(TRUE);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//-----------------------------------------------------------------------------
void CLinkButton::OnLButtonUp(UINT, CPoint ptMousePos)
{
	if (!m_bCaptured) return;

	ReleaseCapture();
	m_bCaptured = FALSE;

	SetState(FALSE);
	Invalidate();
	UpdateWindow(); // immediate feedback

	CRect rect;
	GetClientRect(rect);

	if (!rect.PtInRect(ptMousePos))
		return;

	if (m_WherePressed == OUTSIDE)
		return;

	MousePosition	pos = WhereDown(ptMousePos);
	BOOL			bLower = FALSE;

	if (!m_bTriState)
	{
		bLower = FALSE;
	}
	else
	{
		// TriState button (downside bitmap resource present)
		switch (pos)
		{
		case UPSIDE:
			if (m_WherePressed == UPSIDE)
				bLower = FALSE;
			break;

		case DOWNSIDE:
			if (m_WherePressed == DOWNSIDE)
				bLower = TRUE;
			break;
		}
	}
	// clear entry position status	
	m_WherePressed = OUTSIDE;

	DoCLick(bLower);
}
//-----------------------------------------------------------------------------
void CLinkButton::DoCLick(BOOL bLower)
{
	BOOL bOk = TRUE;

	CWnd* pFocusWnd = GetFocus();
	// risolve il problema di pigiare i bottoni dell'hotlink (o altro)
	// senza prima perdere il fuoco dal control correne (che potrebbe essere
	// dato di input al control di cui si premono i bottoncini)
	if (
		pFocusWnd								&&
		pFocusWnd->m_hWnd != m_pOwner->m_hWnd &&
		!m_pOwner->IsChild(pFocusWnd)	// fuoco nell'edit delle combo

		)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(pFocusWnd);
		if (pCtrl)
			bOk = pCtrl->UpdateCtrlData(TRUE, TRUE);
	}
	if (bOk)
		m_pOwner->PostMessage(UM_PUSH_BUTTON_CTRL, bLower, MAKE_COMPATIBLE_HANDLE(0));
}

//-----------------------------------------------------------------------------
void CLinkButton::OnMouseMove(UINT, CPoint point)
{
	if (!m_bCaptured) return;

	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (((GetState() & 0x0004) != NULL) == bInside)
		return;

	SetState(bInside);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//=============================================================================
//			Class CStateButton implementation
//=============================================================================

IMPLEMENT_DYNAMIC(CStateButton, CButton)

BEGIN_MESSAGE_MAP(CStateButton, CButton)
	//{{AFX_MSG_MAP(CStateButton)
	ON_WM_LBUTTONUP()
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	ON_WM_MOUSEMOVE()
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CStateButton::CStateButton(CWnd* pCWnd, CParsedCtrl* pCtrl /*NULL*/)
	:
	m_pOwner(pCWnd),
	m_pParsedCtrl(pCtrl),
	m_nCurrentState(0),
	m_bCaptured(FALSE),
	m_nBmpWitdh(0)
{
	ASSERT(pCWnd);
	m_StateBitmaps.RemoveAll();
}

//-----------------------------------------------------------------------------
void  CStateButton::SetStateBitmaps(int nState, CString strImageNS)
{
	LoadStateBitmaps(nState, 0, 0, strImageNS);
}

// consente di impostare i bitmap abilitati/disabilitati per uno stato
//-----------------------------------------------------------------------------
void CStateButton::SetStateBitmaps(int nState, UINT nBmpNormal)
{
	LoadStateBitmaps(nState, nBmpNormal);
}

// consente di impostare i bitmap abilitati/disabilitati per uno stato
//-----------------------------------------------------------------------------
void CStateButton::SetStateBitmaps(int nState, UINT nBmpNormal, UINT nBmpDisabled)
{
	LoadStateBitmaps(nState, nBmpNormal, nBmpDisabled);
}

//-----------------------------------------------------------------------------
void CStateButton::LoadStateBitmaps(int nState, UINT nBmpNormal, UINT nBmpDisabled, CString strImageNS)
{
	if (AfxIsRemoteInterface())
		return;
	int nBitmapNormal = nState * 2;
	int nBitmapDisabled = nBitmapNormal + 1;

	// bitmap normale
	CWalkBitmap* pBitmapNormal = NULL;

	if (m_StateBitmaps.GetUpperBound() < nBitmapNormal)
	{
		pBitmapNormal = new CWalkBitmap();
		m_StateBitmaps.InsertAt(nBitmapNormal, pBitmapNormal);
	}
	else
		pBitmapNormal = dynamic_cast<CWalkBitmap*>(m_StateBitmaps.GetAt(nBitmapNormal));

	ASSERT(pBitmapNormal);
	pBitmapNormal->DeleteObject();

	if (nBmpNormal > 0)
	{
		if (!pBitmapNormal->LoadBmpOrPng(nBmpNormal))
		{
			ASSERT(FALSE);
			TRACE1("CStateButton: Failed to load bitmap for normal image state %d\n", nState);
			return;
		}
	}
	else if (!strImageNS.IsEmpty())
	{
		if (!pBitmapNormal->LoadBmpOrPng(strImageNS))
		{
			ASSERT(FALSE);
			TRACE1("CStateButton: Failed to load from nameSpace state %d\n", nState);
			return;
		}
	}
	else
	{
		ASSERT(FALSE);
	}

	// bitmap disabilitato
	CWalkBitmap* pBitmapDisabled = NULL;
	if (m_StateBitmaps.GetUpperBound() < nBitmapDisabled)
	{
		pBitmapDisabled = new CWalkBitmap();
		m_StateBitmaps.InsertAt(nBitmapDisabled, pBitmapDisabled);
	}
	else
		pBitmapDisabled = dynamic_cast<CWalkBitmap*>(m_StateBitmaps.GetAt(nBitmapDisabled));

	ASSERT(pBitmapDisabled);
	pBitmapDisabled->DeleteObject();

	if (nBmpDisabled == 0)
	{
		CBitmapClone(pBitmapNormal, pBitmapDisabled);
		CDC* pDC = GetDC();
		BITMAP bm;
		pBitmapDisabled->GetObject(sizeof(BITMAP), &bm);
		if (pDC)
		{
			CDC memDC1;
			memDC1.CreateCompatibleDC(pDC);
			CBitmap* pDest = memDC1.SelectObject(pBitmapDisabled);
			// convert a colour image to grayscale
			for (int x = 0; x < bm.bmWidth; x++)
			{
				for (int y = 0; y < bm.bmHeight; y++)
				{
					COLORREF cl = memDC1.GetPixel(x, y);
					int luma = (int)(GetRValue(cl)*0.3 + GetGValue(cl)*0.59 + GetBValue(cl)*0.11);
					cl = RGB(luma, luma, luma);
					memDC1.SetPixel(x, y, cl);
				}
			}
			if (pDest)
			{
				memDC1.SelectObject(pDest);
			}
			memDC1.DeleteDC();
		}
		ReleaseDC(pDC);
	}
	else
	{
		if (!pBitmapDisabled->LoadBmpOrPng(nBmpDisabled))
		{
			ASSERT(FALSE);
			TRACE1("CStateButton: Failed to load bitmap for disable image state %d\n", nState);
			return;
		}
	}
}
//-----------------------------------------------------------------------------
void CStateButton::SetCurrentState(int nState)
{
	m_nCurrentState = nState;
}

//-----------------------------------------------------------------------------
void CStateButton::ClearAllBitmaps()
{
	for (int i = 0; i <= m_StateBitmaps.GetUpperBound(); i++)
		((CWalkBitmap*)m_StateBitmaps.GetAt(i))->DeleteObject();

	m_StateBitmaps.RemoveAll();
}

//-----------------------------------------------------------------------------
CWalkBitmap* CStateButton::GetCurrentStateBmp(BOOL bDisableBmp /*FALSE*/)
{
	if (m_nCurrentState < 0)
		return NULL;

	int nBitmap = (m_nCurrentState * 2) + bDisableBmp;
	if (nBitmap >= m_StateBitmaps.GetCount())
		return NULL;
	return (CWalkBitmap*)m_StateBitmaps.GetAt(nBitmap);
}

//-----------------------------------------------------------------------------
int	CStateButton::GetButtonWidth()
{
	return m_nBmpWitdh;
}

// SizeToContent si ridimensiona sulla base del bitmap normale
//-----------------------------------------------------------------------------
void CStateButton::SizeToContent()
{
	if (AfxIsRemoteInterface() || AfxIsInUnattendedMode())
		return;

	ASSERT(GetCurrentStateBmp()->m_hObject != NULL);
	CSize bitmapSize;
	BITMAP bmInfo;

	VERIFY(GetCurrentStateBmp()->GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));
	VERIFY(SetWindowPos(NULL, -1, -1, bmInfo.bmWidth, bmInfo.bmHeight,
		SWP_NOMOVE | SWP_NOZORDER | SWP_NOREDRAW | SWP_NOACTIVATE));

	m_nBmpWitdh = bmInfo.bmWidth;
}

// Draw the appropriate bitmap
//-----------------------------------------------------------------------------
void CStateButton::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	if (!m_StateBitmaps.GetSize())
		return __super::DrawItem(lpDIS);

	ASSERT(lpDIS != NULL);

	CWalkBitmap* pBitmap = GetCurrentStateBmp();
	CWalkBitmap* pDisabledBitmap = GetCurrentStateBmp(TRUE);

	// must have at least the first bitmap loaded before calling DrawItem
	ASSERT(pBitmap->m_hObject != NULL);     // required

	UINT state = lpDIS->itemState;

	if ((state & ODS_DISABLED) && pDisabledBitmap->m_hObject != NULL)
		pBitmap = pDisabledBitmap;   // stato di disabilitato

	// draw the whole button
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	CDC memDC;
	memDC.CreateCompatibleDC(pDC);
	CBitmap* pOld = dynamic_cast<CBitmap*>(memDC.SelectObject(pBitmap));
	if (pOld == NULL)
		return;     // destructors will clean up

	CRect rect;
	rect.CopyRect(&lpDIS->rcItem);
	pDC->TransparentBlt
	(
		rect.left, rect.top, rect.Width(), rect.Height(), &memDC,
		rect.left, rect.top, rect.Width(), rect.Height(), memDC.GetPixel(0, 0)
	);

	memDC.SelectObject(pOld);
	memDC.DeleteDC();
}

//-----------------------------------------------------------------------------
void CStateButton::OnLButtonDown(UINT, CPoint ptMousePos)
{
	SetCapture();
	m_bCaptured = TRUE;

	SetState(TRUE);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//-----------------------------------------------------------------------------
void CStateButton::OnLButtonUp(UINT, CPoint ptMousePos)
{
	if (!m_bCaptured) return;

	ReleaseCapture();
	m_bCaptured = FALSE;

	SetState(FALSE);
	Invalidate();
	UpdateWindow(); // immediate feedback

	CRect rect;
	GetClientRect(rect);

	if (!rect.PtInRect(ptMousePos))
		return;

	WPARAM	wParam = m_nCurrentState;

	BOOL bOk = TRUE;
	CWnd* pWnd = GetFocus();
	// risolve il problema di pigiare i bottoni dell'hotlink (o altro)
	// senza prima perdere il fuoco dal control correne (che potrebbe essere
	// dato di input al control di cui si premono i bottoncini)
	if (
		pWnd								&&
		pWnd->m_hWnd != m_pOwner->m_hWnd &&
		!m_pOwner->IsChild(pWnd)	// fuoco nell'edit delle combo

		)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);
		if (pCtrl)
			bOk = pCtrl->UpdateCtrlData(TRUE, TRUE);
	}
	if (bOk)
	{
		m_pOwner->PostMessage(UM_PUSH_BUTTON_CTRL, wParam, (LPARAM)this->m_hWnd);
	}

}

//---------------------------------------------------------------------------
void CStateButton::DoContextMenu(CPoint ptMousePos)
{
	if (m_pOwner == NULL || m_pParsedCtrl == NULL)
		return;

	//chiedo al ParsedControl associato il menu da visualizzare
	CTBWebFriendlyMenu menu;
	CMenu* pmenu = m_pParsedCtrl->GetMenu();
	if (pmenu == NULL)
	{
		pmenu = &menu;
	}
	else
		pmenu->DestroyMenu();

	pmenu->CreatePopupMenu();

	if (!m_pParsedCtrl->GetMenuButton(pmenu))
		return;

	// Notify the menu is going to be opened.
	// AfxGetThreadContext()->OnBeforeMenuOpen(pmenu, this->m_hWnd);

	// get the point where the menu is to be displayed.
	// this is hte lower left corner of the control (button)
	CRect rect;
	GetWindowRect(rect);
	POINT point;
	point.x = rect.left;
	point.y = rect.bottom;

	AfxGetThreadContext()->RaiseCallBreakEvent();
	DWORD dwSelectionMade;
	CTBWebFriendlyMenu* tempMenu = dynamic_cast<CTBWebFriendlyMenu*>(pmenu);
	if (tempMenu)
	{
		// show and track the menu
		dwSelectionMade = tempMenu->TBTrackPopupMenu
		(
			(TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD),
			point.x, point.y, this
		);
	}
	else
	{
		// show and track the menu
		dwSelectionMade = pmenu->TrackPopupMenu
		(
			(TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD),
			point.x, point.y, this
		);
	}

	if (pmenu == &menu)
		pmenu->DestroyMenu();

	m_pOwner->PostMessage(UM_PUSH_BUTTON_CTRL, dwSelectionMade, m_pParsedCtrl->m_nButtonIDBmp);

}

//-----------------------------------------------------------------------------
BOOL CStateButton::OnCommand(WPARAM wParam, LPARAM lParam)
{
	//Routing del messaggio per gestire il click su un elemento del menu di contesto da web
	if (m_pOwner && m_pParsedCtrl)
		m_pOwner->PostMessage(UM_PUSH_BUTTON_CTRL, wParam, m_pParsedCtrl->m_nButtonIDBmp);

	return TRUE;
}
//-----------------------------------------------------------------------------
void CStateButton::OnMouseMove(UINT, CPoint point)
{
	if (!m_bCaptured) return;

	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (((GetState() & 0x0004) != NULL) == bInside)
		return;

	SetState(bInside);
	Invalidate();
	UpdateWindow(); // immediate feedback
}

//---------------------------------------------------------------------------
void CStateButton::OnRButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl)
		DoContextMenu(ptMousePos);
	else
		__super::OnRButtonDown(nFlags, ptMousePos);
}

//-----------------------------------------------------------------------------
LRESULT CStateButton::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CPushButtonDescription* pDesc = (CPushButtonDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CPushButtonDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::Button;

	CWalkBitmap* pBitmap = GetCurrentStateBmp();
	if (pBitmap)
	{
		CString sName = cwsprintf(_T("stbtn%ud.png"), (HBITMAP)*pBitmap);

		if (pDesc->m_ImageBuffer.Assign((HBITMAP)*pBitmap, sName, this))
			pDesc->SetUpdated(&pDesc->m_ImageBuffer);
	}
	return (LRESULT)pDesc;
}

//=============================================================================
//			Class CMenuButton
//=============================================================================
IMPLEMENT_DYNAMIC(CMenuButton, CStateButton)

//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CMenuButton, CStateButton)
	//{{AFX_MSG_MAP(CMenuButton)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CMenuButton::CMenuButton(CWnd* pWndOwner, CParsedCtrl* pCtrlOwner)
	:
	CStateButton(pWndOwner, pCtrlOwner)
{
	ASSERT(pCtrlOwner);
}

//---------------------------------------------------------------------------
void CMenuButton::OpenContextMenu()
{
	if (m_pOwner == NULL || m_pParsedCtrl == NULL)
		return;

	//chiedo al ParsedControl associato il menu da visualizzare
	CTBWebFriendlyMenu menu;
	CMenu* pmenu = m_pParsedCtrl->GetMenu();
	if (pmenu == NULL)
	{
		pmenu = &menu;
	}
	else
		pmenu->DestroyMenu();

	pmenu->CreatePopupMenu();

	if (!m_pParsedCtrl->GetMenuButton(pmenu))
		return;

	// Notify the menu is going to be opened.
	// AfxGetThreadContext()->OnBeforeMenuOpen(pmenu, this->m_hWnd);

	// get the point where the menu is to be displayed.
	// this is hte lower left corner of the control (button)
	CRect rect;
	GetWindowRect(rect);
	POINT point;
	point.x = rect.left;
	point.y = rect.bottom;

	AfxGetThreadContext()->RaiseCallBreakEvent();

	DWORD dwSelectionMade;
	CTBWebFriendlyMenu* tempMenu = dynamic_cast<CTBWebFriendlyMenu*>(pmenu);
	if (tempMenu)
	{
		// show and track the menu
		dwSelectionMade = tempMenu->TBTrackPopupMenu
		(
			(TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD),
			point.x, point.y, this
		);
	}
	else
	{
		// show and track the menu
		dwSelectionMade = pmenu->TrackPopupMenu
		(
			(TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_NONOTIFY | TPM_RETURNCMD),
			point.x, point.y, this
		);
	}

	if (pmenu == &menu)
		pmenu->DestroyMenu();

	// m_pOwner->PostMessage(UM_PUSH_BUTTON_CTRL, dwSelectionMade, m_pParsedCtrl->m_nButtonIDBmp);	
}
//---------------------------------------------------------------------------
void CMenuButton::OnLButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl)
		DoContextMenu(ptMousePos);
	else
		CStateButton::OnLButtonDown(nFlags, ptMousePos);
}

//=============================================================================
//			Class CControlLabel
//=============================================================================
IMPLEMENT_DYNCREATE(CControlLabel, CStatic)

//---------------------------------------------------------------------------
CControlLabel::CControlLabel()
	:
	CCustomFont(this)
{
}

//=============================================================================
//			Class CCalendarButton
//=============================================================================
IMPLEMENT_DYNAMIC(CCalendarButton, CStateButton)

//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCalendarButton, CStateButton)
	//{{AFX_MSG_MAP(CCalendarButton)
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	//ON_MESSAGE(UM_DESTROY_CALENDAR, OnDestroyCalendar)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CCalendarButton::CCalendarButton(CWnd* pWndOwner, CParsedCtrl* pCtrlOwner)
	:
	CStateButton(pWndOwner, pCtrlOwner),
	m_pCalWnd(NULL),
	m_pManagedWndWrapper(NULL)
{
	ASSERT(pCtrlOwner);
}

//---------------------------------------------------------------------------
CCalendarButton::~CCalendarButton()
{
	SAFE_DELETE(m_pManagedWndWrapper);
}

//---------------------------------------------------------------------------
void CCalendarButton::OnLButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl && m_pParsedCtrl->GetCtrlCWnd())
	{
		// sets the focus on the associated control
		// if something goes wrong, checking the message queue
		// will restore the original focus
		// only if the associated control can gain the focus, show the calendar
		m_pParsedCtrl->SetCtrlFocus();
		m_BatchScheduler.CheckMessage();
		if (!m_pParsedCtrl->HasFocus())
			return;

		//apro calendario c#
		if (m_pManagedWndWrapper == NULL)
		{
			m_pManagedWndWrapper = new ManagedWindowWrapper(m_pParsedCtrl->GetCtrlCWnd()->m_hWnd);

			//Se sono la finestra di impostazione di data applicazione parto con il calendario impostato su today,
			//altrimenti lo imposto sulla data di applicazione
			CWnd* pParentWnd = m_pParsedCtrl->GetCtrlCWnd()->GetParent();
			CRuntimeClass* pParentClass = pParentWnd ? pParentWnd->GetRuntimeClass() : NULL;

			DataDate aNow;
			DataDate aParsedCtrlDate;
			if (m_pParsedCtrl->GetCtrlData() && m_pParsedCtrl->GetCtrlData()->IsFullDate())
			{
				aParsedCtrlDate.SetFullDate();
				aNow.SetFullDate();
			}

			if (pParentClass && strcmp(pParentClass->m_lpszClassName, "CApplicationDateDialog") == 0)
				aNow.SetTodayDate();
			else
				aNow = AfxGetApplicationDate();

			m_pParsedCtrl->GetValue(aParsedCtrlDate);
			if (aParsedCtrlDate.IsEmpty())
				aParsedCtrlDate = aNow;

			HWND hwndParent = m_pParsedCtrl->GetCtrlCWnd()->GetParentFrame() ? m_pParsedCtrl->GetCtrlCWnd()->GetParentFrame()->m_hWnd : m_pParsedCtrl->GetCtrlCWnd()->m_hWnd;
			m_pManagedWndWrapper->ShowMonthCalendar(hwndParent, aNow.Day(), aNow.Month(), aNow.Year(), aParsedCtrlDate.Day(), aParsedCtrlDate.Month(), aParsedCtrlDate.Year());
		}
	}
	else
		CStateButton::OnLButtonDown(nFlags, ptMousePos);
}


//---------------------------------------------------------------------------
void CCalendarButton::OnRButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl && m_pParsedCtrl->GetCtrlCWnd())
	{
		// sets the focus on the associated control
		// if something goes wrong, checking the message queue
		// will restore the original focus
		// only if the associated control can gain the focus, show the calendar
		m_pParsedCtrl->SetCtrlFocus();
		m_BatchScheduler.CheckMessage();
		if (!m_pParsedCtrl->HasFocus())
			return;

		if (m_pManagedWndWrapper == NULL)
			m_pManagedWndWrapper = new ManagedWindowWrapper(m_pParsedCtrl->GetCtrlCWnd()->m_hWnd);

		DataDate applicationDate = AfxGetApplicationDate();
		HWND hwndParent = m_pParsedCtrl->GetCtrlCWnd()->GetParentOwner() ? m_pParsedCtrl->GetCtrlCWnd()->GetParentOwner()->m_hWnd : m_pParsedCtrl->GetCtrlCWnd()->m_hWnd;
		m_pManagedWndWrapper->ShowRangeSelector(hwndParent, applicationDate.Day(), applicationDate.Month(), applicationDate.Year(), AfxGetPathFinder()->GetDateRangesFilePath());
	}
}

//-----------------------------------------------------------------------------
LRESULT CCalendarButton::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	return (LRESULT)CWndObjDescription::GetDummyDescription(); //to avoid to add this button to windows description
}
/*
/////////////////////////////////////////////////////////////////////////////
// CMonthCalCtrlEx window

class CMonthCalCtrlEx : public CMonthCalCtrl
{
// Construction
public:
	CMonthCalCtrlEx();

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMonthCalCtrlEx)
	protected:
	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CMonthCalCtrlEx();

public:
	BOOL	IsDestroyEnabled	()	{ return m_bEnableDestroy; }
	void	EnableDestroy		(BOOL bEnable) { m_bEnableDestroy = bEnable; }

	// Generated message map functions
protected:
	DWORD	m_nIgnoreNextMessage;
	BOOL	m_bEnableDestroy;


	//{{AFX_MSG(CMonthCalCtrlEx)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CMonthCalCtrlExWnd window

class CMonthCalCtrlExWnd : public CWnd
{
// Construction
public:
  CMonthCalCtrlExWnd(CWnd* pParent, DWORD dwMCStyle = 0);

// Attributes
public:
	CMonthCalCtrlEx* GetMonthCalCtrl	()	{ return m_pCalendar; }

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMonthCalCtrlExWnd)
	public:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	virtual BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext = NULL);
	virtual BOOL DestroyWindow();
	protected:
	virtual BOOL OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CMonthCalCtrlExWnd();

	// Generated message map functions
protected:
	//{{AFX_MSG(CMonthCalCtrlExWnd)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnActivateApp(BOOL bActive, DWORD hTask);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	CMonthCalCtrlEx* m_pCalendar;

	CWnd*		m_pParent;
	DWORD		m_dwMCStyle;
};

/////////////////////////////////////////////////////////////////////////////
// CMonthCalCtrlEx

CMonthCalCtrlEx::CMonthCalCtrlEx()
	:
	m_bEnableDestroy (TRUE)

{
  m_nIgnoreNextMessage = 0;
}

CMonthCalCtrlEx::~CMonthCalCtrlEx()
{
}

BEGIN_MESSAGE_MAP(CMonthCalCtrlEx, CMonthCalCtrl)
	//{{AFX_MSG_MAP(CMonthCalCtrlEx)
	ON_WM_CREATE()
	ON_WM_KEYDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CMonthCalCtrlEx message handlers

LRESULT CMonthCalCtrlEx::WindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
  if (m_nIgnoreNextMessage != message)
  {
	if (message == WM_LBUTTONDOWN ||
	message == WM_MBUTTONDOWN ||
	message == WM_RBUTTONDOWN)
	{
	  // Is mouse within control
	  CPoint point(lParam);
	  CRect rcClient;
	  GetClientRect(rcClient);
	  if (! rcClient.PtInRect(point))
	  {
	ReleaseCapture();
	GetOwner()->PostMessage(UM_DESTROY_CALENDAR);
	  }
	  else
		SetCapture();
		EnableDestroy (FALSE);
	}
	else if (message == WM_LBUTTONUP ||
		 message == WM_MBUTTONUP ||
		 message == WM_RBUTTONUP)
	{
	  CMonthCalCtrl::WindowProc(message, wParam, lParam);
	  // we seem to lose capture on Xbuttonup, which stops us catching
	  // out-of-rect messages after changing, for instance, the month
	  // so we need to re-capture messages. However, if the Xbuttondown
	  // was out-of-rect, then we won't exist by this point, so test validity
	  if (::IsWindow(m_hWnd))
	SetCapture();
	  return 0;
	}
	else if (message == WM_PARENTNOTIFY)
	{
	  if (LOWORD(wParam) == WM_DESTROY)
	// just destroyed the 'year' edit/updown, but this makes us lose capture
	SetCapture();
	}
	else if (message == WM_MENUSELECT)
	{
	  if (HIWORD(wParam) == 0xffff && lParam == 0)
	  {
	// the month menu has been closed, so re-take capture
	SetCapture();
	// if the menu was closed by clicking outside of the client area,
	// then by retaining the capture the mouse event will close the
	// calendar, which is not what we want, so we need to ignore the
	// next click of that mouse button in our code above
	m_nIgnoreNextMessage = 0;
	if (GetAsyncKeyState(MK_LBUTTON) & 0x80000000)
	  m_nIgnoreNextMessage = WM_LBUTTONDOWN;
	else if (GetAsyncKeyState(MK_MBUTTON) & 0x80000000)
	  m_nIgnoreNextMessage = WM_MBUTTONDOWN;
	else if (GetAsyncKeyState(MK_RBUTTON) & 0x80000000)
	  m_nIgnoreNextMessage = WM_RBUTTONDOWN;
	  }
	}
  }
  if (message == m_nIgnoreNextMessage)
	m_nIgnoreNextMessage = 0; // don't ignore it again
  return CMonthCalCtrl::WindowProc(message, wParam, lParam);
}

/////////////////////////////////////////////////////////////////////////////
// CDateTimeEditCtrlCalendarWnd

CMonthCalCtrlExWnd::CMonthCalCtrlExWnd(CWnd* pParent, DWORD dwMCStyle)
{
  m_pParent = pParent;
  m_dwMCStyle = dwMCStyle;
  m_pCalendar = NULL;
}

CMonthCalCtrlExWnd::~CMonthCalCtrlExWnd()
{
  delete m_pCalendar;
}

BEGIN_MESSAGE_MAP(CMonthCalCtrlExWnd, CWnd)
	//{{AFX_MSG_MAP(CMonthCalCtrlExWnd)
	ON_WM_CREATE()
	ON_WM_SIZE()
	ON_WM_ACTIVATEAPP()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// CMonthCalCtrlExWnd message handlers

BOOL CMonthCalCtrlExWnd::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext)
{
  return CWnd::Create(0, 0, dwStyle, rect, pParentWnd, nID, pContext);
}


BOOL CMonthCalCtrlExWnd::PreCreateWindow(CREATESTRUCT& cs)
{
	LONG oldStyle = cs.style;
	cs.style = cs.style | WS_CHILD;

	BOOL bOk = __super::PreCreateWindow(cs);

	cs.style = oldStyle;
	cs.hMenu = NULL;

	return bOk;
}

int CMonthCalCtrlExWnd::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
  if (CWnd::OnCreate(lpCreateStruct) == -1)
	return -1;

  // hide the taskbar button
  if (!(lpCreateStruct->style & WS_POPUP))
	ModifyStyleEx(0, WS_EX_TOOLWINDOW);

  // Create calendar control
	m_pCalendar = new CMonthCalCtrlEx;
	DWORD dwStyle = m_dwMCStyle & ~(WS_VISIBLE);
	dwStyle |= MCS_WEEKNUMBERS;
	VERIFY(m_pCalendar->Create(dwStyle | WS_CHILD, CPoint(0, 0), this, ID_BTN_CALENDAR));
	SetFont(AfxGetControlFont());
	m_pCalendar->SetFont(AfxGetControlFont());
	m_pCalendar->SizeMinReq();
	m_pCalendar->SetOwner(m_pParent);

	// size self to fit calendar
	// and make us top-most, so we're seen
	CRect rcCal;
	m_pCalendar->GetWindowRect(&rcCal);
	CalcWindowRect(&rcCal);
	SetWindowPos(&wndTopMost, 0, 0, rcCal.Width(), rcCal.Height(), SWP_NOMOVE | SWP_NOACTIVATE);

	m_pCalendar->ShowWindow(SW_SHOW);

	// the calendar needs to catch all mouse messages, so it can respond to
	// changes in the visible month etc
	m_pCalendar->SetCapture();

	return 0;
}

BOOL CMonthCalCtrlExWnd::DestroyWindow()
{
  ReleaseCapture();
  m_pCalendar->DestroyWindow();
  return CWnd::DestroyWindow();
}

void CMonthCalCtrlExWnd::OnActivateApp(BOOL bActive, DWORD hTask)
{
  CWnd::OnActivateApp(bActive, hTask);

  if (! bActive && m_pParent != NULL)
	m_pParent->PostMessage(UM_DESTROY_CALENDAR, TRUE);
}

BOOL CMonthCalCtrlExWnd::OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
  LPNMHDR lpnmhdr = (LPNMHDR)lParam;
  if (lpnmhdr != NULL && m_pParent != NULL &&
		(lpnmhdr->code == MCN_SELECT || lpnmhdr->code == MCN_SELCHANGE))
  {
	  m_pCalendar->EnableDestroy(TRUE);
	*pResult = m_pParent->SendMessage(WM_NOTIFY, wParam, lParam);
	return TRUE;
  }
  return CWnd::OnNotify(wParam, lParam, pResult);
}

///////////////////////////////////////////////////////////////////////////////
void CCalendarButton::CreateCalendar ()
{
	CRect rc;
	CMonthCalCtrlExWnd* pCalWnd = new CMonthCalCtrlExWnd(this);
	m_pCalWnd = pCalWnd;
	this->m_pOwner->GetWindowRect(&rc);

	rc.top = rc.bottom + 1;

	// Get screen size
	CRect rcDummy, rcWorkArea;
	GetMonitorRect(m_pOwner, rcWorkArea, rcDummy);

	if (rc.bottom >= rcWorkArea.bottom)
		rc.bottom = rcWorkArea.bottom;

	pCalWnd->CreateEx(0, NULL, NULL, WS_VISIBLE | WS_POPUP | WS_BORDER, rc, this, 0);
	CMonthCalCtrlEx* pCal = pCalWnd->GetMonthCalCtrl();
	CRect rcCalCtrl;
	if (pCal)
	{
		pCalWnd->SetFont(AfxGetControlFont());
		pCal->GetClientRect(&rcCalCtrl);
	}
	// line calendar window up with appropriate edge of control
	CRect rcCal, rcEdit;
	this->m_pOwner->GetClientRect(&rc);
	this->m_pOwner->ClientToScreen(&rc);
	rc.top = rc.bottom + 1;
	pCalWnd->GetWindowRect(&rcCal);
	this->m_pOwner->GetClientRect(&rcEdit);
	this->m_pOwner->ClientToScreen(&rcEdit);
	rc.bottom = rc.top + rcCal.Height() + 2;//scale for ControlFont: + 30;

	rc.left = rcEdit.left;
	rc.right = rc.left + (max(rcCalCtrl.Width(), rcCal.Width())) + 2; //scale for ControlFont: + 40;

	// if it goes off the bottom of the screen, then put it above this control
	if (rc.bottom > rcWorkArea.bottom)
	{
		CRect rcWnd;
		this->m_pOwner->GetWindowRect(&rcWnd);
		rc.OffsetRect(0, -(rcCal.Height() + rcWnd.Height()));
	}
	// if it's off the left, then nudge it over
	if (rc.left < rcWorkArea.left)
		rc.OffsetRect(rcWorkArea.left - rc.left, 0);

	if (rc.right > rcWorkArea.right)
		rc.OffsetRect(rcWorkArea.right - rc.right, 0);

	if (pCal != NULL)
	{
		CWnd* pParentWnd = m_pParsedCtrl->GetCtrlCWnd()->GetParent();
		CRuntimeClass* pParentClass = pParentWnd ? pParentWnd->GetRuntimeClass() : NULL;
		DataDate aNow;

		// I prefere use runtimeClass name as it's bettere the dialog is not exposed
		if (pParentClass && strcmp(pParentClass->m_lpszClassName, "CApplicationDateDialog") == 0)
			aNow.SetTodayDate();
		else
			aNow = AfxGetApplicationDate();

		COleDateTime today = (COleDateTime) aNow;
		ASSERT(today.GetStatus() == GDT_VALID);
		if (today.GetStatus() == GDT_VALID)
			pCal->SetToday(today);

		ASSERT(m_pParsedCtrl->GetCtrlCWnd());
		DataDate dd; dd.SetFullDate();
		if (m_pParsedCtrl->UpdateCtrlData(FALSE, FALSE))
			m_pParsedCtrl->GetValue(dd);
		else
			m_pParsedCtrl->ClearCtrl();

		if (!dd.IsEmpty())
		{
			COleDateTime dateInit = (COleDateTime)dd;
			if (dateInit.GetStatus() == GDT_VALID)
				pCal->SetCurSel(dateInit);
			else
				pCal->SetCurSel(today);
		}
		else
		{
			pCal->SetCurSel(today);
		}

		pCalWnd->SetWindowPos(NULL, rc.left, rc.top, rc.Width(), rc.Height(), SWP_NOZORDER | SWP_NOACTIVATE);
		pCal->SetWindowPos(NULL, 0, 0, rc.Width(), rc.Height(), SWP_NOMOVE | SWP_SHOWWINDOW);
		pCalWnd->ShowWindow(SW_SHOWNA);
	}

	//bug fix: when used in woorm ask dialog (in tabbed mode), ask dialog disappears
	CWnd* pParentWnd = m_pParsedCtrl->GetCtrlCWnd()->GetParent();
	if (pParentWnd)
		pParentWnd->ShowWindow(SW_SHOWNA);
}

//-----------------------------------------------------------------------------
LONG CCalendarButton::OnDestroyCalendar(WPARAM wParam, LPARAM lParam)
{
	// destroy the cal ctrl if shown
	// returns TRUE if destroyed, else FALSE if not shown
	BOOL bDiscard = wParam != 0;
	if (m_pCalWnd == NULL)
		return FALSE;
	if (::IsWindow(m_pCalWnd->m_hWnd))
		m_pCalWnd->DestroyWindow();
	delete m_pCalWnd;
	m_pCalWnd = NULL;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CCalendarButton::OnNotify(WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
  NMHDR* pnmhdr = (NMHDR*)lParam;
  if (pnmhdr != NULL)
  {
	// if notification from cal ctrl then act on them, and destroy cal ctrl
	if (m_pCalWnd != NULL && pnmhdr->idFrom == ID_BTN_CALENDAR)
	{
	  if
		(
			pnmhdr->code == MCN_SELECT ||
			pnmhdr->code == MCN_SELCHANGE ||
			pnmhdr->code == MCN_GETDAYSTATE
		)
	  {
		if (pnmhdr->code == MCN_SELCHANGE || pnmhdr->code == MCN_SELECT)
		{
			ASSERT(m_pParsedCtrl);
			// get date, and put in edit ctrl
			LPNMSELCHANGE lpnmsc = (LPNMSELCHANGE)pnmhdr;
			COleDateTime date(lpnmsc->stSelStart);
			ASSERT(date.GetStatus() == COleDateTime::valid);

			if (pnmhdr->code == MCN_SELECT)
			{
				// we want to close the calendar when the user selects a date
				PostMessage(UM_DESTROY_CALENDAR, NULL, NULL);
			}

			if (pnmhdr->code == MCN_SELECT || !m_pParsedCtrl->GetCtrlParent()->IsKindOf(RUNTIME_CLASS(CGridControl)))
			{
				DataDate dd; dd.Assign(date);
				if (m_pParsedCtrl->GetCtrlData() && m_pParsedCtrl->GetCtrlData()->IsFullDate())
					dd.SetFullDate();
				m_pParsedCtrl->SetValue(dd);
				m_pParsedCtrl->SetModifyFlag(TRUE);
			}

			if (pnmhdr->code == MCN_SELECT)
				m_pParsedCtrl->SetCtrlFocus(TRUE);
		}
		return TRUE;
	  }
	}
	// pass generic notifications from the controls to the parent
	CWnd* pParent = GetParent();
	if (pParent != NULL)
	{
		pnmhdr->idFrom = GetDlgCtrlID();
		pnmhdr->hwndFrom = GetSafeHwnd();
		*pResult = pParent->SendMessage(WM_NOTIFY, (WPARAM)pnmhdr->idFrom, lParam);
		return TRUE;
	}
  }
  return __super::OnNotify(wParam, lParam, pResult);
}

//---------------------------------------------------------------------------
BOOL CCalendarButton::OnCommand(WPARAM wParam, LPARAM lParam)
{
  // if button clicked, show calendar control
  if (HIWORD(wParam) == BN_CLICKED && LOWORD(wParam) == ID_BTN_CALENDAR)
  {
	  if (m_pCalWnd && m_pCalWnd->m_hWnd != NULL)
	  OnDestroyCalendar();
	else
	  CreateCalendar();
	return TRUE;
  }
  return __super::OnCommand(wParam, lParam);
}

//---------------------------------------------------------------------------
BOOL CCalendarButton::IsDestroyCalendarEnabled()
{
	CMonthCalCtrlExWnd* pCalWnd = (CMonthCalCtrlExWnd*) m_pCalWnd;
	if (!pCalWnd || !pCalWnd->GetMonthCalCtrl())
		return FALSE;

	return pCalWnd->GetMonthCalCtrl()->IsDestroyEnabled();
}
*/
//---------------------------------------------------------------------------
BOOL CCalendarButton::IsDestroyCalendarEnabled()
{
	return FALSE;
}

//=============================================================================
//			Class COutlookButton
//=============================================================================
IMPLEMENT_DYNAMIC(COutlookButton, CStateButton)

//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(COutlookButton, CStateButton)
	//{{AFX_MSG_MAP(COutlookButton)
	ON_WM_LBUTTONDOWN()
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
COutlookButton::COutlookButton(CWnd* pWndOwner, CParsedCtrl* pCtrlOwner)
	:
	CStateButton(pWndOwner, pCtrlOwner)
{
	ASSERT(pCtrlOwner);
}

//---------------------------------------------------------------------------
void COutlookButton::OnLButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl)
		ShowAddressLists();
	else
		CStateButton::OnLButtonDown(nFlags, ptMousePos);
}

//-----------------------------------------------------------------------------
LRESULT COutlookButton::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	return (LRESULT)CWndObjDescription::GetDummyDescription(); //to avoid to add this button to windows decription
}

//---------------------------------------------------------------------------
void COutlookButton::ShowAddressLists()
{
	ASSERT(m_pParsedCtrl);

	CString strAddress;
	m_pParsedCtrl->GetValue(strAddress);

	CString strAddressNew;
	if (AfxGetIMailConnector()->MapiShowAddressBook(this->m_hWnd, strAddressNew))
	{
		m_pParsedCtrl->SetValue(strAddress.IsEmpty() ? strAddressNew : strAddress + ';' + strAddressNew);
		m_pParsedCtrl->SetModifyFlag(TRUE);
		m_pParsedCtrl->SetCtrlFocus(TRUE);
	}
}

//=============================================================================
//			Class CColorButton
//=============================================================================
IMPLEMENT_DYNAMIC(CColorButton, CStateButton)

//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CColorButton, CStateButton)
	//{{AFX_MSG_MAP(CColorButton)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CColorButton::CColorButton(CWnd* pWndOwner, CParsedCtrl* pCtrlOwner)
	:
	CStateButton(pWndOwner, pCtrlOwner)
{
	ASSERT(pCtrlOwner);
}

//---------------------------------------------------------------------------
void CColorButton::OnLButtonDown(UINT nFlags, CPoint ptMousePos)
{
	if (m_pParsedCtrl)
		ShowColorDlg();
	else
		CStateButton::OnLButtonDown(nFlags, ptMousePos);
}

//---------------------------------------------------------------------------
void CColorButton::ShowColorDlg()
{
	if (!m_pParsedCtrl)
		return;
	if (!m_pOwner)
		return;

	ASSERT_KINDOF(CColorEdit, m_pOwner);
	m_pOwner->SetFocus();
	CColorEdit* pColEdit = dynamic_cast<CColorEdit*>(m_pOwner);

	DataLng dl;
	m_pParsedCtrl->GetValue(dl);

	pColEdit->m_bIsRunning = TRUE;

	//simple code
	CColorDialog dlg((long)dl, 0, GetParent());

	if (dlg.DoModal() == IDOK)
	{
		COLORREF color = dlg.GetColor();

		if (pColEdit)
		{
			pColEdit->SetValue(color);

			//vedi CDateEdit::OnRangeSelectorSelected
			pColEdit->SetModifyFlag(TRUE);

			pColEdit->m_bIsRunning = FALSE;

			pColEdit->UpdateCtrlData(TRUE);
			pColEdit->SetFocus();
		}
	}
	/*
	// Get screen size
	CRect rcDummy, rcWorkArea;
	GetMonitorRect(m_pOwner, rcWorkArea, rcDummy);

	CRect rc;
	GetWindowRect(rc);

	if (rc.left < rcWorkArea.left)
		rc.OffsetRect(rcWorkArea.left - rc.left, 0);

	if (rc.right > rcWorkArea.right)
		rc.OffsetRect(rcWorkArea.right - rc.right, 0);

	CColourPopup* pdlg = new CColourPopup
		(
			CPoint(rc.left, rc.bottom),
			(long)dl,
			m_pParsedCtrl->GetCtrlCWnd()
		);

	pdlg->SetActiveWindow();
	pdlg->SetFocus();
	*/
}

//===========================================================================
//							class CStateCtrlState
//===========================================================================

//-------------------------------------------------------------------------------------
CStateCtrlState::CStateCtrlState(UINT nBmpNormalId, BOOL bEnableCtrl)
{
	m_nBmpNormalId = nBmpNormalId;
	m_nBmpDisabledId = 0;
	m_bEnableCtrl = bEnableCtrl;
	m_imageNS = _T("");
}

//-------------------------------------------------------------------------------------
CStateCtrlState::CStateCtrlState(const CString&  strImageNS, BOOL bEnableCtrl)
{
	m_nBmpNormalId = 0;
	m_nBmpDisabledId = 0;
	m_imageNS = strImageNS;
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
CStateCtrlState::CStateCtrlState(const CString&  strImageNS, const CString& strImageNSDisabled, BOOL bEnableCtrl)
{
	m_nBmpNormalId = 0;
	m_nBmpDisabledId = 0;
	m_imageNS = strImageNS;
	m_imageNSDisabled = strImageNSDisabled;
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
CStateCtrlState::CStateCtrlState(UINT nBmpNormalId, UINT nBmpDisabledId, BOOL bEnableCtrl)
{
	m_nBmpNormalId = nBmpNormalId;
	m_nBmpDisabledId = nBmpDisabledId;
	m_imageNS = _T("");
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
void CStateCtrlState::Set(const CString& strImageNS, BOOL bEnableCtrl)
{
	m_nBmpNormalId = 0;
	m_nBmpDisabledId = 0;
	m_imageNS = strImageNS;
	m_imageNSDisabled.Empty();
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
void CStateCtrlState::Set(const CString& strImageNS, const CString& strImageNSDisabled, BOOL bEnableCtrl)
{
	m_nBmpNormalId = 0;
	m_nBmpDisabledId = 0;
	m_imageNS = strImageNS;
	m_imageNSDisabled = strImageNSDisabled;
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
void CStateCtrlState::Set(UINT nBmpNormalId, BOOL bEnableCtrl)
{
	m_nBmpNormalId = nBmpNormalId;
	m_nBmpDisabledId = 0;
	m_imageNS = _T("");
	m_bEnableCtrl = bEnableCtrl;
}

//-------------------------------------------------------------------------------------
void CStateCtrlState::Set(UINT nBmpNormalId, UINT nBmpDisabledId, BOOL bEnableCtrl)
{
	m_nBmpNormalId = nBmpNormalId;
	m_nBmpDisabledId = nBmpDisabledId;
	m_bEnableCtrl = bEnableCtrl;
	m_imageNS = _T("");
}

//===========================================================================
//							class CStateCtrlObj
//===========================================================================

//-----------------------------------------------------------------------------
CStateCtrlObj::CStateCtrlObj()
	:
	m_pParsedCtrl(NULL),
	m_pDataObj(NULL),
	m_pOldDataObj(NULL),
	m_pButton(NULL),
	m_bClearDataWhenEditable(FALSE),
	m_pNumberingRequest(NULL),
	m_nOldLen(0),
	m_bIsCommandBtn(FALSE),
	m_bSetColorInEditableState(FALSE),
	m_crTextColor(0),
	m_crBkgColor(AfxGetThemeManager()->GetBackgroundColor()),
	m_crSaveTextColor(0),
	m_crSaveBkgColor(AfxGetThemeManager()->GetBackgroundColor())
{
	Init();
}

//-----------------------------------------------------------------------------
CStateCtrlObj::~CStateCtrlObj()
{
	m_States.RemoveAll();
	m_pParsedCtrl = NULL;
	DetachDataObj();
	m_pNumberingRequest = NULL;
	SAFE_DELETE(m_pButton);
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::Init()
{
	m_bBtnManuallyDisabled = FALSE;
	m_bPushButtonNotify = TRUE;
	m_bCtrlInEditModeEnabled = TRUE;
	m_bStateInEditModeEnabled = TRUE;
	m_bManualReadOnly = FALSE;
	m_bCtrlChangedNotify = TRUE;
	m_bIsCommandBtn = FALSE;
	m_nDataInfoIdx = -1;


	m_States.RemoveAll();

	m_nCurrErrorID = CParsedCtrl::EMPTY_MESSAGE;
	m_nCurrWarningID = CParsedCtrl::EMPTY_MESSAGE;
	m_strCurrMessage.Empty();

	// inizializzo di default i due stati più diffusi
	SetCtrlStateSingle(0, TBIcon(szIconExecute, CONTROL), FALSE);
	SetCtrlStateSingle(1, TBIcon(szIconEdit, CONTROL), TRUE);
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::InitMessages()
{
	m_pParsedCtrl->SetErrorID(m_nCurrErrorID);
	m_pParsedCtrl->SetWarningID(m_nCurrWarningID);

	m_pParsedCtrl->m_strMessage = m_strCurrMessage;

	m_nCurrErrorID = CParsedCtrl::EMPTY_MESSAGE;
	m_nCurrWarningID = CParsedCtrl::EMPTY_MESSAGE;

	m_strCurrMessage.Empty();
}

// Consente di indicare quale dataobj è preposto alla gestione automatica dello stato
//-----------------------------------------------------------------------------
void CStateCtrlObj::AttachDataObj(DataObj* pData, BOOL bInvertDefaultStates /*FALSE*/)
{
	// per ora gestisco solo il DataInt e DataBool
	ASSERT(pData->IsKindOf(RUNTIME_CLASS(DataInt)) || pData->IsKindOf(RUNTIME_CLASS(DataBool)));
	m_pDataObj = pData;
	SAFE_DELETE(m_pOldDataObj);
	m_pOldDataObj = pData->DataObjClone();

	if (m_nDataInfoIdx < 0)
		m_nDataInfoIdx = m_pParsedCtrl->m_pSqlRecord ? ((ISqlRecord*)m_pParsedCtrl->m_pSqlRecord)->GetIndexFromDataObj(pData) : -1;

	if (bInvertDefaultStates)
	{
		// inverto i default dei due stati più diffusi
		SetCtrlStateSingle(0, TBIcon(szIconEdit, CONTROL), TRUE);
		SetCtrlStateSingle(1, TBIcon(szIconExecute, CONTROL), FALSE);
	}
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::AttachNumberingRequest(CNumbererRequest* pRequest)
{
	m_pNumberingRequest = pRequest;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::DetachDataObj()
{
	m_pDataObj = NULL;
	SAFE_DELETE(m_pOldDataObj);
}

// Ci sono però casi in cui il programmatore vuole forzare lo stato di
// disabilitazione/abilitazione del bottone.
//-----------------------------------------------------------------------------
void CStateCtrlObj::DisableButton(BOOL bDisabled /*= FALSE*/)
{
	// da adesso in poi sarà sempre disabilitato/abilitato
	m_bBtnManuallyDisabled = bDisabled;

	m_pParsedCtrl->DoEnable(m_bBtnManuallyDisabled);
}

// Indica al control che deve abilitarsi o meno in stato di EDIT del doc.
//-----------------------------------------------------------------------------
void CStateCtrlObj::EnableCtrlInEditMode(BOOL bEnable /*= TRUE*/)
{
	m_bCtrlInEditModeEnabled = bEnable;
}

// Indica al bottone che deve abilitarsi o meno in stato di EDIT del doc.
//-----------------------------------------------------------------------------
void CStateCtrlObj::EnableStateInEditMode(BOOL bEnable /*= TRUE*/)
{
	m_bStateInEditModeEnabled = bEnable;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetManualReadOnly(BOOL bReadOnly /*TRUE*/)
{
	m_bManualReadOnly = bReadOnly;
}

// Abilita/Disabilita la notifica dei messaggi di cambio stato del bottone 
//-----------------------------------------------------------------------------
void CStateCtrlObj::EnablePushButtonNotify(BOOL bEnable /*= TRUE*/)
{
	m_bPushButtonNotify = bEnable;
}

// Abilita/Disabilita la notifica dei messaggi di control changed
//-----------------------------------------------------------------------------
void CStateCtrlObj::EnableCtrlChangedNotify(BOOL bEnable /*= TRUE*/)
{
	m_bCtrlChangedNotify = bEnable;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetClearDataWhenEditable(BOOL bClear /*TRUE*/)
{
	m_bClearDataWhenEditable = bClear;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetCommandBtn(BOOL b /*TRUE*/)
{
	m_bIsCommandBtn = b;
}

//-----------------------------------------------------------------------------
BOOL CStateCtrlObj::IsInEditableState()
{
	if (!m_pButton)
		return TRUE;

	CStateCtrlState* pState = GetCtrlState(m_pButton->GetButtonCurrentState());
	return pState ? pState->IsToEnableCtrl() : FALSE;
}

//-----------------------------------------------------------------------------
const int CStateCtrlObj::GetCurrentCtrlState() const
{
	if (!m_pDataObj)
		return 0;

	// definisce il prossimo stato in cui mettersi
	if (m_pDataObj->GetDataType() == DataType::Bool)
		return (int) *((DataBool*)m_pDataObj);

	return *((DataInt*)m_pDataObj);
}

//-----------------------------------------------------------------------------
DataObj* CStateCtrlObj::GetDataObj() const
{
	return m_pDataObj;
}

//-----------------------------------------------------------------------------
const DataObj* CStateCtrlObj::GetOldDataObj() const
{
	return m_pOldDataObj;
}

//-----------------------------------------------------------------------------
CStateCtrlState* CStateCtrlObj::GetCtrlState(int nState)
{
	return nState >= 0 && nState <= m_States.GetUpperBound() ? (CStateCtrlState*)m_States.GetAt(nState) : NULL;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetCtrlStateSingle(int nState, const CString& strImageNS, BOOL bEnableCtrl/* = TRUE*/)
{
	CStateCtrlState* pState = GetCtrlState(nState);
	if (pState)
		pState->Set(strImageNS, bEnableCtrl);
	else
	{
		pState = new CStateCtrlState(strImageNS, bEnableCtrl);
		m_States.InsertAt(nState, pState);
	}

	if (m_pButton)
		m_pButton->SetStateBitmaps(nState, strImageNS);

}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetCtrlStateSingle(int nState, UINT nBmpNormal, BOOL bEnableCtrl /*TRUE*/)
{
	CStateCtrlState* pState = GetCtrlState(nState);
	if (pState)
		pState->Set(nBmpNormal, bEnableCtrl);
	else
	{
		pState = new CStateCtrlState(nBmpNormal, bEnableCtrl);
		m_States.InsertAt(nState, pState);
	}

	if (m_pButton)
		m_pButton->SetStateBitmaps(nState, nBmpNormal);

}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetCtrlState(int nState, UINT nBmpNormal, UINT nBmpDisabled, BOOL bEnableCtrl /*TRUE*/)
{
	CStateCtrlState* pState = GetCtrlState(nState);
	if (pState)
		pState->Set(nBmpNormal, nBmpDisabled, bEnableCtrl);
	else
	{
		pState = new CStateCtrlState(nBmpNormal, nBmpDisabled, bEnableCtrl);
		m_States.InsertAt(nState, pState);
	}
	if (m_pButton)
		m_pButton->SetStateBitmaps(nState, nBmpNormal, nBmpDisabled);
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetCtrlState(int nState, const CString& nBmpNormal, const CString& nBmpDisabled, BOOL bEnableCtrl /*TRUE*/)
{
	CStateCtrlState* pState = GetCtrlState(nState);
	if (pState)
		pState->Set(nBmpNormal, nBmpDisabled, bEnableCtrl);
	else
	{
		pState = new CStateCtrlState(nBmpNormal, nBmpDisabled, bEnableCtrl);
		m_States.InsertAt(nState, pState);
	}
	if (m_pButton)
		m_pButton->SetStateBitmaps(nState, nBmpNormal/*TODO , nBmpDisabled*/);
}

//-----------------------------------------------------------------------------
int	CStateCtrlObj::GetButtonWidth() const
{
	CStateButton* pButton = (CStateButton*)GetButton();

	if (pButton)
		return pButton->GetButtonWidth();

	return 20;
}

// Si occupa di abilitare/disabilitare il control e non il bottone
// i possibili valori di ritorno sono: 
// 1=abilitato	0=disabilitato 2=readonly true 
//-----------------------------------------------------------------------------
int CStateCtrlObj::IsToEnableCtrl()
{
	ASSERT(m_pParsedCtrl);

	CBaseDocument* pDoc = (CBaseDocument*)m_pParsedCtrl->GetDocument();

	if (!m_pParsedCtrl || !pDoc || !m_pDataObj)
		return 0;

	// se il programmatore vuole comandare lui il readonly o se il documento
	// è in browse o se sono in edit e non può essere abilitato
	if (
		m_bManualReadOnly ||
		(pDoc->GetFormMode() == CBaseDocument::BROWSE && pDoc->GetType() != VMT_BATCH) ||
		(pDoc->GetFormMode() == CBaseDocument::EDIT && !m_bCtrlInEditModeEnabled)
		)
	{
		ChangeCtrlStatus();
		return 0;
	}

	if (pDoc->GetFormMode() == CBaseDocument::FIND && m_pParsedCtrl && m_pParsedCtrl->m_pData && m_pParsedCtrl->m_pData->IsFindable())
		return 1;

	// stato corrente definito dal valore del dataObj
	int nCurrentState = GetCurrentCtrlState();
	CStateCtrlState* pCurrentState = GetCtrlState(nCurrentState);

	int nValue = 0;
	if (pCurrentState && (pDoc->GetFormMode() == CBaseDocument::NEW || pDoc->GetFormMode() == CBaseDocument::EDIT || (pDoc->GetFormMode() == CBaseDocument::BROWSE && pDoc->GetType() == VMT_BATCH)))
		nValue = pCurrentState->IsToEnableCtrl() ? 1 : 2;

	// in parte ediabile a causa della msdchera di formattazione
	if (nValue != 1 && m_pParsedCtrl->GetFormatMask() && !IsInEditableState() && m_pParsedCtrl->GetFormatMask()->GetEditableZoneStart() >= 0)
		nValue = 1;

	ChangeCtrlStatus();

	return nValue;
}

// Il programmatore potrebbe aver fatto della diagnostica, quindi è necessario 
// testare localmente gli stati di errore e salvarli prima di lasciare operare
// la ModifiedCtrlData, la quale come prima azione azzera nell'IsValid gli er=
// rori del control.
//-----------------------------------------------------------------------------
void CStateCtrlObj::ChangeCtrlStatus(BOOL bNotify)
{
	// devo avere il valore dello stato
	if (!m_pDataObj || !m_pButton)
		return;

	BOOL bCanChangeStatus = !m_pNumberingRequest || (m_pNumberingRequest && m_pNumberingRequest->GetEnabled());

	// definisce il prossimo stato in cui mettersi
	BOOL bDataBool = m_pDataObj->GetDataType() == DataType::Bool;
	bool bStatusChanged = false;
	if (bNotify)
	{
		if (bCanChangeStatus)
		{
			m_pOldDataObj->Assign(*m_pDataObj);

			if (bDataBool)
				*((DataBool*)m_pDataObj) = !(*((DataBool*)m_pDataObj));
			else
			{
				DataInt* pDataInt = (DataInt*)m_pDataObj;
				if (*pDataInt < DataInt::MAXVALUE) (*pDataInt)++;
				else *pDataInt = DataInt::MINVALUE;
			}
			bStatusChanged = true;
		}

		if (m_bPushButtonNotify)
			m_pParsedCtrl->NotifyToParent(EN_CTRL_STATE_CHANGED);

		BOOL bOk = !m_pParsedCtrl->HasBeenInvalidated();

		m_nCurrErrorID = m_pParsedCtrl->GetErrorID();
		m_nCurrWarningID = m_pParsedCtrl->GetWarningID();
		m_strCurrMessage = m_pParsedCtrl->m_strMessage;

		// riporta lo stato al valore precedente
		if (!bOk)
		{
			m_pDataObj->Assign(*m_pOldDataObj);
			bStatusChanged = false;
			if (m_bPushButtonNotify)
				m_pParsedCtrl->NotifyToParent(EN_CTRL_STATE_CHANGED);
		}

		if (m_bCtrlChangedNotify)
			m_pParsedCtrl->ModifiedCtrlData();
	}

	if (m_pDataObj)
	{
		int nCurrentState = bDataBool ? (int) *((DataBool*)m_pDataObj) : *((DataInt*)m_pDataObj);
		m_pButton->SetCurrentState(nCurrentState);
		ExecuteDefaultBehaviours(bStatusChanged);

		m_pButton->SizeToContent();
		m_pButton->Invalidate();
	}

	if (m_bSetColorInEditableState)
	{
		ASSERT_VALID(m_pParsedCtrl->m_pOwnerWnd);
		if (m_pParsedCtrl->m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
		{
			CParsedEdit* pEdit = (CParsedEdit*)m_pParsedCtrl->m_pOwnerWnd;

			if (IsInEditableState())
			{
				pEdit->SetTextColor(m_crTextColor);
				pEdit->SetBkgColor(m_crBkgColor);
			}
			else
			{
				pEdit->SetTextColor(m_crSaveTextColor);
				pEdit->SetBkgColor(m_crSaveBkgColor);
			}
		}
	}

	// Devo avvisare il control di fare refresh, altrimenti la
	// UpdateDataView successiva non farebbe mai refresh in caso 
	// il changed stesso del campo abbia cambiato il control.
	m_pParsedCtrl->UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetColorInEditableState(COLORREF crBkg, COLORREF crText)
{
	ASSERT_VALID(m_pParsedCtrl->m_pOwnerWnd);
	if (!m_pParsedCtrl->m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
	{
		ASSERT_TRACE(FALSE, "CStateCtrlObj::SetColorInEditableState is allows only on controls derived from CParsedEdit class\n");
		return;
	}

	CParsedEdit* pEdit = dynamic_cast<CParsedEdit*>(m_pParsedCtrl->m_pOwnerWnd);

	m_bSetColorInEditableState = TRUE;

	m_crSaveTextColor = pEdit->GetTextColor();
	m_crSaveBkgColor = pEdit->GetBkgColor();

	m_crTextColor = crText;
	m_crBkgColor = crBkg;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::ExecuteDefaultBehaviours(bool statusHasChanged)
{
	// in browse non fa nulla
	if (!m_pParsedCtrl || !m_pParsedCtrl->m_pDocument || m_pParsedCtrl->m_pDocument->GetFormMode() == CBaseDocument::BROWSE)
		return;

	// in edit se è disabilitato non fa nulla e mantiene il valore
	if (m_pParsedCtrl->m_pDocument->GetFormMode() == CBaseDocument::EDIT && !m_bCtrlInEditModeEnabled)
		return;

	// sono in manuale
	if (IsInEditableState())
	{
		if (IsClearDataWhenEditable() && statusHasChanged)
			m_pParsedCtrl->m_pData->Clear();

		// ripristino la lunghezza originaria del control
		if (m_nOldLen)
			m_pParsedCtrl->SetCtrlMaxLen(m_nOldLen);
		return;
	}

	CFormatMask* pFormatMask = m_pParsedCtrl->GetFormatMask();
	// se ho un formattatore metto il control lungo come la maschera
	// e mi salvo la vecchia lunghezza
	if (pFormatMask)
		m_pParsedCtrl->SetCtrlMaxLen(pFormatMask->GetMask().GetLength());

	if (!statusHasChanged)
		return;

	// sono in automatic quindi chiedo al servizio di numerare
	IBehaviourContext* pContext = m_pParsedCtrl->m_pDocument->GetBehaviourContext();
	if (pContext && m_pNumberingRequest)
	{
		INumbererService* pService = dynamic_cast<INumbererService*>(pContext->GetService(m_pNumberingRequest));
		if (pService)
			pService->ReadNumber(m_pNumberingRequest);

	}
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::ShowButton(int nCmdShow /*SW_SHOW*/)
{
	if (m_pButton)
		m_pButton->ShowWindow(nCmdShow);
}

//-----------------------------------------------------------------------------
BOOL CStateCtrlObj::CreateButton(CWnd* pParentWnd)
{
	ASSERT(m_pParsedCtrl);

	DWORD dwBtnStyle = (m_pParsedCtrl->GetCtrlCWnd()->GetStyle() & (WS_VISIBLE | WS_DISABLED)) | WS_CHILD | RADAR_STYLE;

	// creo l'oggetto
	m_pButton = new CStateButton(m_pParsedCtrl->GetCtrlCWnd());

	int nCurrentState = 0;

	BOOL bBoolDataObj = m_pDataObj && m_pDataObj->IsKindOf(RUNTIME_CLASS(DataBool));
	BOOL bIntDataObj = m_pDataObj && m_pDataObj->IsKindOf(RUNTIME_CLASS(DataInt));

	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(_T("StateButton"), TbResourceType::TbControls);
	if (!m_pButton->Create(NULL, dwBtnStyle, CRect(0, 0, 0, 0), pParentWnd->GetParent(), nIDC))
		return FALSE;

	if (m_pButton && !AfxGetThemeManager()->GetControlsUseBorders())
		m_pButton->ModifyStyle(WS_BORDER, 0);

	// travaso le richieste inzializzate del programmatore 
	CStateCtrlState* pState;
	for (int i = 0; i <= m_States.GetUpperBound(); i++)
	{
		pState = GetCtrlState(i);
		if (pState)
		{
			if (pState->GetBmpNormalId() > 0)
			{
				m_pButton->SetStateBitmaps(i, pState->GetBmpNormalId(), pState->GetBmpDisabledId());
			}
			else
			{
				m_pButton->SetStateBitmaps(i, pState->GetBmpNS());
			}
		}
	}

	// se sono comandato da un boolean mi metto Automatico/Manuale di default
	if (bBoolDataObj)
		nCurrentState = (int) *((DataBool*)m_pDataObj);

	m_pButton->SetCurrentState(nCurrentState);
	m_pButton->SizeToContent();

	if (m_bBtnManuallyDisabled)
		m_pButton->EnableWindow(FALSE);

	m_pButton->Invalidate();

	m_pParsedCtrl->ReserveSpaceForButton(m_pButton->GetButtonWidth(), m_pButton);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::DeleteButton()
{
	if (!m_pButton)
		return;
	int nWidth = GetButtonWidth();

	CWnd* pWnd = m_pParsedCtrl->GetCtrlCWnd();
	CRect rect;
	if (pWnd)
	{
		pWnd->GetWindowRect(rect);
		CWnd* pParent = pWnd->GetParent();
		pParent->ScreenToClient(rect);
	}

	m_pButton->DestroyWindow();
	delete m_pButton;
	m_pButton = NULL;
	// ricalcolo del control
	if (nWidth > 0 && pWnd)
	{
		int newWidth = rect.Width() + nWidth + BTN_OFFSET;
		pWnd->SetWindowPos(NULL, 0, 0, newWidth, rect.Height(), SWP_NOZORDER | SWP_NOMOVE);
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AdjustButtonsVisualizationUpdateControls()
{
	AdjustButtonsVisualization();
	UpdateCtrlStatus();
	UpdateCtrlView();
	if (GetCtrlCWnd())
		GetCtrlCWnd()->UpdateWindow();
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AdjustButtonsVisualization()
{
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		CStateCtrlObj* pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		CStateButton* pStateButton = pStateCtrl->GetButton();
		if (!pStateCtrl || !pStateButton)
			continue;

		if (pStateButton)
		{
			ReserveSpaceForButton(pStateButton->GetButtonWidth(), pStateButton);
		}
	}
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetButtonPos(CRect& rectBtn, UINT nFlags)
{
	m_pButton->SetWindowPos
	(
		NULL,
		rectBtn.left, rectBtn.top, 0, 0,
		(nFlags & ~SWP_NOMOVE) | SWP_NOSIZE | SWP_NOACTIVATE
	);
}

// Non posso settare il fuoco sul ctrl quando l'utente preme il bottone a causa
// del rischio di repaint ricorsivo se faccio SetFocus().
//-----------------------------------------------------------------------------
void CStateCtrlObj::DoPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	// l'azione del bottone non avviene se il flag 
	// è stato messo read-only dal programmatore.
	if (!m_pDataObj || (m_pDataObj && m_pDataObj->IsReadOnly()))
		return;

	ChangeCtrlStatus(TRUE);

	CWnd* pOwnerWnd = m_pParsedCtrl->GetCtrlCWnd();
	BOOL bIsEdit = pOwnerWnd->IsKindOf(RUNTIME_CLASS(CEdit));
	BOOL bIsComboBox = pOwnerWnd->IsKindOf(RUNTIME_CLASS(CComboBox));

	int nAction = IsToEnableCtrl();

	BOOL bManageAsReadOnly = (nAction == 2 || nAction == 1) && (bIsEdit || bIsComboBox);

	// gestione del readonly
	if (bManageAsReadOnly)
		m_pParsedCtrl->SetEditReadOnly(nAction == 2);
	else			// disabilitato/abilitato
		pOwnerWnd->EnableWindow(nAction == 1);

	if (pOwnerWnd->IsWindowEnabled())
		m_pParsedCtrl->SetCtrlFocus(TRUE);
}

// Il bottone resta comunque disabilitato se è m_bBtnManuallyDisabled 
//-----------------------------------------------------------------------------
void CStateCtrlObj::DoEnable(BOOL bEnable)
{
	if (!m_pButton)
		return;

	// se il programmatore vuole comandare lui il readonly
	// non ci sono logiche aggiuntive
	if (
		m_bManualReadOnly ||
		(m_pParsedCtrl && m_pParsedCtrl->m_pData && m_pParsedCtrl->m_pData->IsReadOnly()) ||
		(m_pDataObj && m_pDataObj->IsReadOnly())
		)
	{
		m_pButton->EnableWindow(FALSE);
		return;
	}

	CBaseDocument* pDoc = m_pParsedCtrl->GetDocument();

	if (pDoc && pDoc->GetFormMode() == CBaseDocument::FIND && m_pParsedCtrl && m_pParsedCtrl->m_pData && m_pParsedCtrl->m_pData->IsFindable())
		bEnable = FALSE;

	if (pDoc && pDoc->GetFormMode() == CBaseDocument::BROWSE && pDoc->GetType() != VMT_BATCH)
		bEnable = FALSE;

	if (pDoc &&
		(
			pDoc->GetFormMode() == CBaseDocument::NEW ||
			pDoc->GetFormMode() == CBaseDocument::EDIT ||
			(pDoc->GetFormMode() == CBaseDocument::BROWSE && pDoc->GetType() == VMT_BATCH)
			)
		)
		bEnable = pDoc->GetFormMode() != CBaseDocument::EDIT || (m_bCtrlInEditModeEnabled && m_bStateInEditModeEnabled);

	m_pButton->EnableWindow(!m_bBtnManuallyDisabled && bEnable);

	if (m_pParsedCtrl->m_pHyperLink)
		m_pParsedCtrl->m_pHyperLink->DoEnable(bEnable);
}

// E` reimplementata per gestire l'eventuale invalidazione nella gestione dei
// messaggi NDEN_MANUAL e NDEN_AUTOMATIC: vedi GENLIB\EXTRES.hrc
//-----------------------------------------------------------------------------
BOOL CStateCtrlObj::IsValid()
{
	InitMessages();

	return m_pParsedCtrl->GetErrorID() == 0;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::SetDataObj(DataObj* pDataObj)
{
	m_pDataObj = pDataObj;
}

//-----------------------------------------------------------------------------
BOOL CStateCtrlObj::DoChar(UINT nChar)
{
	// caratteristiche per cui è sempre abilitata la digitazione di tutto
	if (
		!m_pButton || !m_pParsedCtrl || IsInEditableState() || !m_pParsedCtrl->GetFormatMask()
		|| m_pParsedCtrl->m_pDocument->GetFormMode() == CBaseDocument::FIND
		)
		return TRUE;

	int nSuffixStart = m_pParsedCtrl->GetFormatMask()->GetEditableZoneStart();
	if (nSuffixStart < 0)
		return TRUE;

	int nStartChar, nEndChar;

	switch (nChar)
	{
	case VK_LEFT:
	case VK_UP:
	case VK_BACK:
		m_pParsedCtrl->GetCtrlSel(nStartChar, nEndChar);
		if (nStartChar <= nSuffixStart)
			return FALSE;

		break;

	case VK_HOME:
		m_pParsedCtrl->SetCtrlSel(nSuffixStart, nSuffixStart);
		return FALSE;

	default:
		m_pParsedCtrl->GetCtrlSel(nStartChar, nEndChar);
		if (nEndChar < nSuffixStart || nStartChar < nSuffixStart)
			m_pParsedCtrl->SetCtrlSel(nSuffixStart, nSuffixStart);

		break;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::AssignEditableValueToDataObj()
{
	if (!m_pParsedCtrl->GetDocument())
		return;

	if (m_pDataObj->GetDataType() == DataType::Bool)
	{
		if (m_pParsedCtrl->GetDocument()->GetFormMode() == CBaseDocument::EDIT)
			*((DataBool*)m_pDataObj) = GetEditaleStateValue() > 0;
		else
			*((DataBool*)m_pDataObj) = m_pNumberingRequest->GetDatabaseNumberingDisabled();
		return;
	}

	if (m_pDataObj->GetDataType() == DataType::Integer)
	{
		DataInt* pDataInt = (DataInt*)m_pDataObj;
		if (m_pParsedCtrl->GetDocument()->GetFormMode() == CBaseDocument::EDIT)
			*((DataInt*)m_pDataObj) = GetEditaleStateValue();
		else
			*((DataInt*)m_pDataObj) = (int)m_pNumberingRequest->GetDatabaseNumberingDisabled();
	}
}

//-----------------------------------------------------------------------------
void CStateCtrlObj::ProcessFormModeChanged(CNumbererRequest* pNumberingRequest /*NULL*/)
{
	if (pNumberingRequest && pNumberingRequest->GetDatabaseNumberingDisabled())
		m_bBtnManuallyDisabled = TRUE;

	if (m_pNumberingRequest && !IsStateEnabledInEditMode() && IsCtrlEnabledInEditMode())
		AssignEditableValueToDataObj();


}

//-----------------------------------------------------------------------------
int CStateCtrlObj::GetEditaleStateValue()
{
	for (int i = 0; i < m_States.GetSize(); i++)
	{
		CStateCtrlState* pState = (CStateCtrlState*)m_States.GetAt(i);
		if (pState && pState->IsToEnableCtrl())
			return i;
	}
	return -1;
}

//=============================================================================
//			Class CParsedCtrl
//=============================================================================

//-----------------------------------------------------------------------------
CParsedCtrl::CParsedCtrl(DataObj *pData /*=NULL*/)
	:
	IOSLObjectManager(OSLType_Control),
	m_pOwnerWnd(NULL),
	m_pOwnerWndDescription(NULL),
	m_pData(pData),
	m_pOldData(NULL),
	m_bModifyFlag(FALSE),
	m_pDocument(NULL),
	m_pHotKeyLink(NULL),
	m_nFormatIdx(UNDEF_FORMAT),
	m_pFormatter(NULL),
	m_pPrivacyFormatter(NULL),
	m_nCtrlLimit(0),
	m_pButton(NULL),
	m_pHyperLink(NULL),
	m_nButtonIDBmp(NO_BUTTON),
	m_nButtonID(DUMMY_ID),
	m_nErrorID(EMPTY_MESSAGE),
	m_nWarningID(EMPTY_MESSAGE),
	m_nMsgBoxStyle(MB_ICONEXCLAMATION | MB_OKCANCEL),
	m_bAttached(FALSE),
	m_bOwnHotKeyLink(FALSE),
	m_nValueChanging(0),
	m_dwCtrlStyle(0),
	m_bUserRunning(FALSE),
	m_pCaption(NULL),
	m_nCaptionPos(T_NULL_TOKEN),
	m_nUseSpecialCaption(0),
	m_nNewFormatIdx(UNDEF_FORMAT),
	m_pFormatContext(NULL),
	m_pSqlRecord(NULL),
	m_nLastStateCtrlChanged(-1),
	m_nButtonsXOffset(0),
	m_pEvents(NULL),
	m_pCustomFont(NULL),
	m_pNumbererRequest(NULL),
	m_wKeyState(0),
	m_pHotLinkController(NULL),
	m_nUseComponentToFormat(-1),
	m_cPadChar(' '),
	m_pDropTarget(NULL),
	m_bShowErrorBox(FALSE)
{
	AddConsumer(this);
	m_pPrivacyFormatter = new CPrivacyFormatter();
}

//-----------------------------------------------------------------------------
CParsedCtrl::~CParsedCtrl()
{
	SAFE_DELETE(m_pButton)
		SAFE_DELETE(m_pHyperLink)
		SAFE_DELETE(m_pOldData)
		SAFE_DELETE(m_pFormatter)
		SAFE_DELETE(m_pPrivacyFormatter)

		RemoveConsumer(this);

	if (m_pEvents)
	{
		if (m_pData)
			m_pData->DetachEvents(m_pEvents);
		SAFE_DELETE(m_pEvents);
	}

	if (m_pHotKeyLink)
	{
		// si sconnette dall'hotlink
		m_pHotKeyLink->RemoveOwnerCtrl(this);

		if (m_bOwnHotKeyLink)
			SAFE_DELETE(m_pHotKeyLink)
	}

	SAFE_DELETE(m_pHotLinkController);

	if (m_pCaption)
	{
		m_pCaption->DestroyWindow();
		SAFE_DELETE(m_pCaption);
	}

	SAFE_DELETE(m_pDropTarget);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlCaption(const CString& strCaption)
{
	SetCtrlCaption(strCaption, TARIGHT, VATOP, Left, NULL_COORD);
}


//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlCaption(const CString& strCaption,
	VerticalAlignment vAlign,
	CaptionPosition ePosition,
	int nCaptionWidth)
{
	SetCtrlCaption(strCaption, TARIGHT, vAlign, ePosition, nCaptionWidth);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlCaption(const CString& strCaption,
	TextAlignment hAlign,
	VerticalAlignment vAlign,
	CaptionPosition ePosition,
	int nCaptionWidth,
	BOOL bSetPosition /*= TRUE*/)
{
	if (strCaption.IsEmpty())
	{
		if (m_pCaption)
		{
			m_pCaption->DestroyWindow();
			SAFE_DELETE(m_pCaption);
		}
	}
	else
	{
		if (!m_pCaption)
		{
			m_pCaption = new CControlLabel();
			VERIFY(m_pCaption->Create(strCaption, WS_CHILD | WS_VISIBLE, CRect(0, 0, 0, 0), GetCtrlParent()));
			m_pCaption->SetFont(AfxGetThemeManager()->GetFormFont()); // GetCtrlParent()->GetFont());
		}
		m_pCaption->SetWindowText(strCaption);
		if (bSetPosition)
			SetCtrlLabelDefaultPosition(hAlign, vAlign, ePosition, nCaptionWidth);//aggiusto le dimensioni in base al testo
		return;
	}

}

#define CAPTION_ROUNDING_FIX 2 //per aggirare problemi di arrotondamento nel calcolo delle dimensioni

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlLabelDefaultPosition(TextAlignment hAlign, VerticalAlignment vAlign, CaptionPosition ePosition, int nCaptionWidth)
{
	if (m_pCaption)
	{
		CWnd* pParent = GetCtrlParent();
		CRect ownerRect;
		m_pOwnerWnd->GetWindowRect(ownerRect);

		CWindowDC dc(m_pCaption);
		CString strCaption;
		m_pCaption->GetWindowText(strCaption);

		CSize sz = GetMultilineTextSize(&dc, m_pCaption->GetFont(), strCaption);
		if (nCaptionWidth != NULL_COORD)
			sz.cx = nCaptionWidth;
		else
			sz.cx += CAPTION_ROUNDING_FIX + AfxGetThemeManager()->GetTileStaticAreaInnerRightPadding();
		//se sono multiline, probabilmente avrò un'altezza maggiore del controllo a cui sono associato, 
		//quindi mi metto al centro, sbordando sopra e sotto
		if (sz.cy > ownerRect.Height())
			vAlign = VACENTER;
		CRect labelRect;
		switch (ePosition)
		{
		case CParsedCtrl::Upper:
		{
			labelRect = CRect(CPoint(ownerRect.left, ownerRect.top - (sz.cy + INTER_CAPTION_GAP)), CSize(sz.cx, sz.cy));
			break;
		}
		case CParsedCtrl::Left:
		{

			int offsetY;
			switch (vAlign)
			{
			case VATOP:
			{
				CPoint pt(0, 2);
				::SafeMapDialog(pParent->m_hWnd, pt);
				offsetY = pt.y;
				break;
			}
			case VACENTER:
			{
				offsetY = (ownerRect.Height() - sz.cy) / 2;//metà del controllo a cui si riferisce
				break;
			}
			case VABOTTOM:
			{
				offsetY = ownerRect.Height() - sz.cy;
				break;
			}
			}
			int x = ownerRect.left - sz.cx;
			sz.cx -= AfxGetThemeManager()->GetTileStaticAreaInnerRightPadding();
			labelRect = CRect(CPoint(x, ownerRect.top + offsetY), CSize(sz.cx, sz.cy));
			break;
		}
		case CParsedCtrl::Right:
		{
			labelRect = CRect(CPoint(ownerRect.right + INTER_CAPTION_GAP, ownerRect.top), CSize(sz.cx, sz.cy));
			break;
		}
		}

		if (pParent)
			pParent->ScreenToClient(labelRect);

		//sbordo a sinistra: mi metto su due linee per andare a capo
		if (labelRect.left < 0)
		{
			if (pParent)
				pParent->ScreenToClient(ownerRect);
			labelRect.left = 0;
			int newH = 2 * labelRect.Height();//raddoppio (due linee)
			//ricalcolo l'offset rispetto al parent
			int offsetY = (ownerRect.Height() - newH) / 2;//metà del controllo a cui si riferisce
			labelRect.top = ownerRect.top + offsetY;
			labelRect.bottom = labelRect.top + newH;
		}
		switch (hAlign)
		{
		case TextAlignment::TALEFT:
			m_pCaption->ModifyStyle(0, SS_LEFT);
			break;
		case TextAlignment::TACENTER:
			m_pCaption->ModifyStyle(0, SS_CENTER);
			break;
		case TextAlignment::TARIGHT:
			m_pCaption->ModifyStyle(0, SS_RIGHT);
			break;
		}
		m_pCaption->SetWindowPos(NULL, labelRect.left, labelRect.top, labelRect.Width(), labelRect.Height(), SWP_NOZORDER);
		m_pCaption->Invalidate();
	}
}

//-----------------------------------------------------------------------------
CString	CParsedCtrl::GetCtrlCaption()
{
	if (!m_pCaption)
		return _T("");
	CString sCaption;
	m_pCaption->GetWindowText(sCaption);
	return sCaption;
}

//-----------------------------------------------------------------------------
CParsedCtrlEvents* CParsedCtrl::GetParsedCtrlEvents()
{
	if (m_pEvents == NULL)
		m_pEvents = new CParsedCtrlEvents(this);
	return m_pEvents;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::Attach(CWnd* pOwnerWnd)
{
	ASSERT(pOwnerWnd);
	m_pOwnerWnd = pOwnerWnd;
	if (m_pOwnerWnd)
		SetContextClass(m_pOwnerWnd->GetRuntimeClass());
}

//-----------------------------------------------------------------------------
IBehaviourContext* CParsedCtrl::GetContext()
{
	return this;//(IBehaviourContext*) (m_pOwnerWnd ? m_pOwnerWnd : NULL);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::Attach(DataObj* pDataObj)
{
	ASSERT(CheckDataObjType(pDataObj));

	//sgancio il listener dal dataobj corrente...
	if (m_pData)
		m_pData->DetachEvents(GetParsedCtrlEvents());

	m_pData = pDataObj;

	//...e lo aggancio al nuovo
	if (m_pData)
	{
		m_pData->AttachEvents(GetParsedCtrlEvents());
		if (m_pControlBehaviour)
			m_pControlBehaviour->m_pControlData = pDataObj;
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::Attach(UINT nBtnID)
{
	m_nButtonIDBmp = nBtnID;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AttachRecord(SqlRecord* pRecord)
{
	m_pSqlRecord = pRecord;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::SetSpecialCaption(UINT nIDCaption)
{
	ASSERT(m_pOwnerWnd->m_hWnd);

	if (dynamic_cast<CGridControlObj*>(GetCtrlParent()) != NULL)
		return FALSE;

	if (nIDCaption == 0 || (m_pCaption && m_pCaption->GetDlgCtrlID() != 0xFFFF))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CWnd* pWndStatic = GetCtrlParent()->GetDlgItem(nIDCaption);
	TCHAR className[MAX_CLASS_NAME + 1];
	VERIFY(::GetClassName(pWndStatic->m_hWnd, className, MAX_CLASS_NAME));
	if (_tcsicmp(className, _T("STATIC")) != 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (m_pCaption)
		SetCaption(T_NULL_TOKEN);	// il control ritorna allo stato originale

	m_pCaption = new CControlLabel();
	if (!m_pCaption->SubclassDlgItem(nIDCaption, GetCtrlParent()))
	{
		SAFE_DELETE(m_pCaption);

		ASSERT(FALSE);
		return FALSE;
	}

	CRect rectE;
	m_pOwnerWnd->GetWindowRect(rectE);
	GetCtrlParent()->ScreenToClient(rectE);

	CRect rectC;
	m_pCaption->GetWindowRect(rectC);
	GetCtrlParent()->ScreenToClient(rectC);

	m_nCaptionPos = T_DEFAULT;

	if (
		(rectC.top >= rectE.top && rectC.top <= rectE.bottom) ||
		(rectC.bottom >= rectE.top && rectC.bottom <= rectE.bottom)
		)
		m_nCaptionPos = rectC.left <= rectE.left ? T_LEFT : T_RIGHT;

	m_pCaption->GetWindowText(m_strUserCaption);
	m_nUseSpecialCaption = 1;

	return SetCaption();
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetToolTipBuffer(const CString& strTooltip)
{
	m_strToolTipBuffer = strTooltip;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::GetToolTipProperties(CTooltipProperties& tp)
{
	BOOL bRetValue = FALSE;
	if (!m_strToolTipBuffer.IsEmpty())
	{
		tp.m_strText = m_strToolTipBuffer;
		bRetValue = TRUE;
	}
	else  if (m_pHotKeyLink)
		bRetValue = m_pHotKeyLink->GetToolTipProperties(&tp);

	if (!tp.m_strText.IsEmpty() && tp.m_nWidth == -1)
		tp.m_nWidth = 0;

	return bRetValue;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::SetCaption
(
	Token nPos,			// = T_DEFAULT
	LPCTSTR pszCaption,	// = NULL
	int nUseSpecial		// = -1
)
{
	ASSERT(m_pOwnerWnd->m_hWnd);

	if (dynamic_cast<CGridControlObj*>(GetCtrlParent()) != NULL)
		return FALSE;

	Token nOldCaptionPos = m_nCaptionPos;

	switch (nPos)
	{
	case T_DEFAULT:
	case T_LEFT:
	case T_RIGHT:
	{
		if (nPos != T_DEFAULT)
		{
			if (m_pCaption && m_pCaption->GetDlgCtrlID() != 0xFFFF)
			{
				ASSERT(FALSE);	// STATIC in risorsa : inamovibile
				return FALSE;
			}

			m_nCaptionPos = nPos;
		}
		else
			if (m_nCaptionPos == 0)
			{
				ASSERT(FALSE);	// nessun default disponibile
				return FALSE;
			}

		if (pszCaption)
			m_strUserCaption = pszCaption;

		if (nUseSpecial != -1)
			m_nUseSpecialCaption = nUseSpecial;

		break;
	}
	case 0:	break;
	default:	ASSERT(FALSE); return FALSE;
	}

	// potrebbe contenere il testo originale salvato dalla SetSpecialCaption
	CString strCaption = m_strUserCaption;

	if (nPos != 0 && m_nUseSpecialCaption == 1)
	{
		if (!strCaption.IsEmpty()) strCaption += " ";

		strCaption += GetSpecialCaption();
	}

	if ((nPos == 0 || strCaption.IsEmpty()) && m_pCaption == NULL)
		return TRUE;

	CDC* pDC = GetCtrlParent()->GetDC();
	int nSize = GetEditSize(pDC, GetCtrlParent()->GetFont(), strCaption).cx + INTER_CAPTION_GAP;
	GetCtrlParent()->ReleaseDC(pDC);

	CRect rectC;
	CRect rectE;
	m_pOwnerWnd->GetWindowRect(rectE);
	GetCtrlParent()->ScreenToClient(rectE);

	if (m_pCaption)
	{
		m_pCaption->GetWindowRect(rectC);
		GetCtrlParent()->ScreenToClient(rectC);

		if (m_pCaption->GetDlgCtrlID() != 0xFFFF)
		{
			rectC.right = rectC.left + nSize;
			if (m_nCaptionPos == T_LEFT && rectC.right > rectE.left)
				return FALSE;	// si sovrappone al control

			if (!m_pCaption->SetWindowPos(NULL, 0, 0, rectC.Width(), rectC.Height(), SWP_NOZORDER | SWP_NOMOVE))
				return FALSE;

			GetCtrlParent()->InvalidateRect(rectC);
			m_pCaption->SetWindowText(strCaption);

			if (nPos == 0)
			{
				delete m_pCaption;
				m_pCaption = NULL;
				m_nCaptionPos = T_NULL_TOKEN;
				m_nUseSpecialCaption = 0;
				m_strUserCaption.Empty();
			}

			return TRUE;
		}

		if (nOldCaptionPos == T_LEFT)
			rectE.left = rectC.left;
		else
			if (nOldCaptionPos == T_RIGHT)
				rectE.right = rectC.right;
	}

	if (nPos == 0 || strCaption.IsEmpty())		// caption empty
	{
		m_pCaption->ShowWindow(SW_HIDE);

		if (nPos == 0)
		{
			delete m_pCaption;
			m_pCaption = NULL;
			m_nCaptionPos = T_NULL_TOKEN;
			m_nUseSpecialCaption = 0;
			m_strUserCaption.Empty();
		}

		if (!m_pOwnerWnd->SetWindowPos(NULL, rectE.left, rectE.top, rectE.Width(), rectE.Height(), SWP_NOZORDER))
			return FALSE;

		GetCtrlParent()->InvalidateRect(rectE);

		return TRUE;
	}

	if (nSize >= rectE.Width())
		return FALSE;

	rectC = rectE;

	if (m_nCaptionPos == T_RIGHT)
	{
		rectE.right -= nSize;
		rectC.left = rectE.right + INTER_CAPTION_GAP;
	}
	else
	{
		rectE.left += nSize;
		rectC.right = rectC.left + nSize - INTER_CAPTION_GAP;
	}

	if (m_pCaption)
	{
		if (!m_pCaption->SetWindowPos(NULL, rectC.left, rectC.top, rectC.Width(), rectC.Height(), SWP_NOZORDER | SWP_SHOWWINDOW))
			return FALSE;

		GetCtrlParent()->InvalidateRect(rectC);
		m_pCaption->SetWindowText(strCaption);
	}
	else
	{
		m_pCaption = new CControlLabel();
		if (!m_pCaption->Create(strCaption, WS_CHILD | WS_VISIBLE, rectC, GetCtrlParent()))
			return FALSE;

		m_pCaption->SetFont(AfxGetThemeManager()->GetFormFont()); //GetCtrlParent()->GetFont());
	}

	GetCtrlParent()->InvalidateRect(rectE);
	return m_pOwnerWnd->SetWindowPos(NULL, rectE.left, rectE.top, rectE.Width(), rectE.Height(), SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::GetStateCtrl(DataObj* pData) const
{
	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);

		// se è NULL recupero il posto
		if (pStateCtrl && (pStateCtrl->m_pDataObj == pData || pStateCtrl->m_pDataObj == NULL))
			return pStateCtrl;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CButton* CParsedCtrl::GetButton()
{
	return (CButton*)m_pButton;
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::GetLastStateCtrlChanged() const
{
	if (
		m_nLastStateCtrlChanged < 0 ||
		m_nLastStateCtrlChanged > m_StateCtrls.GetUpperBound()
		)
		return NULL;

	return (CStateCtrlObj*)m_StateCtrls.GetAt(m_nLastStateCtrlChanged);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::UpdateStateButtons(int nCmdShow /*SW_SHOW*/)
{
	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (!pStateCtrl)
			continue;

		if (!pStateCtrl->GetButton() && GetCtrlCWnd()->m_hWnd)
			pStateCtrl->CreateButton(GetCtrlCWnd());

		pStateCtrl->ShowButton(nCmdShow);
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::OnAttachStateData(DataObj* pData, BOOL bInvertDefaultStates /*FALSE*/)
{
	AttachStateData(pData, bInvertDefaultStates);
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::AttachStateData(DataObj* pData, BOOL bInvertDefaultStates /*FALSE*/)
{
	ASSERT_VALID(pData);
	// se non era usato lo creo
	CStateCtrlObj* pStateCtrl = GetStateCtrl(pData);
	if (!pStateCtrl)
	{
		pStateCtrl = new CStateCtrlObj();
		pStateCtrl->m_pParsedCtrl = this;
		pStateCtrl->m_nOldLen = GetCtrlMaxLen();
		m_StateCtrls.Add(pStateCtrl);
	}

	pStateCtrl->AttachDataObj(pData, bInvertDefaultStates);
	return pStateCtrl;
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::AttachStateCmd(const CString& sNs)
{
	static DataBool db = TRUE;
	CStateCtrlObj* pStateCtrl = AttachStateData(&db);

	pStateCtrl->SetCommandBtn();

	CStateCtrlState* pState = pStateCtrl->GetCtrlState(TRUE);
	if (pState)
		pState->Set(sNs, TRUE);

	pState = pStateCtrl->GetCtrlState(FALSE);
	if (pState)
		pState->Set(sNs, TRUE);

	return pStateCtrl;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AttachRecordToStateData(ISqlRecord* pRecord)
{
	if (m_StateCtrls.GetSize() == 0)
		return;

	DetachAllStateData();
	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (!pStateCtrl || pStateCtrl->m_nDataInfoIdx < 0)
			continue;

		DataObj* pDataObj = pRecord->GetDataObjAt(pStateCtrl->m_nDataInfoIdx);
		pStateCtrl->AttachDataObj(pDataObj);
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DetachAllStateData()
{
	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (pStateCtrl)
			pStateCtrl->DetachDataObj();
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DetachStateData(DataObj* pData)
{
	CStateCtrlObj* pStateCtrl;
	for (int i = m_StateCtrls.GetUpperBound(); i >= 0; i--)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (!pStateCtrl || pStateCtrl->GetDataObj() != pData)
			continue;

		pStateCtrl->DeleteButton();
		pStateCtrl->DetachDataObj();
		m_StateCtrls.RemoveAt(i);
		m_pOwnerWnd->Invalidate();
		UpdateCtrlView();
		break;
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetEditReadOnly(const BOOL bValue)
{
	m_pOwnerWnd && m_pOwnerWnd->EnableWindow(!bValue);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsEditReadOnly() const
{
	return m_pOwnerWnd && (m_pOwnerWnd->GetStyle() & ES_READONLY) != 0;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AttachHotKeyLink(HotKeyLinkObj* pHotKeyLink, BOOL bOwned /* = FALSE */)
{
	ASSERT(pHotKeyLink);
	//ASSERT(pHotKeyLink->m_pOwnerCtrl == NULL);
	ASSERT(m_pHotKeyLink == NULL);
	ASSERT(m_pOwnerWnd);
	ASSERT(m_pOwnerWnd->m_hWnd == NULL);	// not yet subclassed

	m_pHotKeyLink = pHotKeyLink;
	m_bOwnHotKeyLink = bOwned;
	m_pHotKeyLink->EnableFillListBox(m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)) || m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CListBox)));

	// da` all'hotlink conoscenza di chi lo possiede
	m_pHotKeyLink->SetOwnerCtrl(this);

	if (!m_pDocument && !m_pHotKeyLink->IsHotLinkEnabled())
		return;

	if (m_nButtonIDBmp == BTN_DEFAULT)
		Attach(BTN_DOUBLE_ID);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::ReAttachButton(UINT nBmpID)
{
	if (m_pButton)
	{
		RemoveFromSpaceReservedArray(m_pButton);
		delete m_pButton;
		m_pButton = NULL;
	}
	SAFE_DELETE(m_pHyperLink);

	if (nBmpID != NO_BUTTON)
	{
		Attach(nBmpID);
		CreateAssociatedButton(GetCtrlCWnd()->GetParent());
	}
	UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CParsedCtrl::ReattachHotKeyLink(HotKeyLinkObj* pHotKeyLink, BOOL bOwned /* = FALSE */, BOOL bCopyAttributes /* = TRUE */)
{
	if (m_pButton)
	{
		if (m_pHotKeyLink == NULL &&  pHotKeyLink != NULL && dynamic_cast<CGridControlObj*>(GetCtrlParent()) == NULL)
			m_pButton->ShowWindow(SW_SHOW);

		if (m_pHotKeyLink != NULL &&  pHotKeyLink == NULL)
		{
			m_pButton->ShowWindow(SW_HIDE);

			m_pButton->DestroyWindow();
			delete m_pButton;
			m_pButton = NULL;
		}
	}

	BOOL bHyperLinkEnabledWindow = m_pHyperLink && m_pHyperLink->IsWindowEnabled();

	// stacca il corrente hotlink dal control
	if (m_pHotKeyLink)
	{
		if (bCopyAttributes && pHotKeyLink)
		{
			pHotKeyLink->EnableAddOnFly(m_pHotKeyLink->IsEnabledAddOnFly());
			pHotKeyLink->MustExistData(m_pHotKeyLink->IsMustExistData());
		}
		m_pHotKeyLink->RemoveOwnerCtrl(this);
		if (m_bOwnHotKeyLink)
			SAFE_DELETE(m_pHotKeyLink);
		//non la distruggo, il motore di relayout si è tenuto da parte l'handle della finestra
		//SAFE_DELETE(m_pHyperLink);
	}

	// attacca il nuovo hotlink (che puo` essere NULL)
	m_pHotKeyLink = pHotKeyLink;

	if (m_pHotKeyLink)
	{
		m_pHotKeyLink->SetOwnerCtrl(this);

		m_bOwnHotKeyLink = bOwned;

		m_pHotKeyLink->EnableFillListBox(m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)));
		//prima veniva distrutto e ricreato, ma questo causava problemi all'algoritmo di resize delle tile parts,
		//perché si tiene da parte gli handle delle finestre in partenza
		//adesso mi limito a nasconderlo se non serve
		if (m_pHyperLink)
			m_pHyperLink->ShowWindow(m_pHotKeyLink->GetAddOnFlyNamespace().IsEmpty() ? SW_HIDE : SW_SHOW);

		if (m_pButton == NULL)
		{
			if (m_nButtonIDBmp == BTN_DEFAULT)
				Attach(BTN_DOUBLE_ID);
			CreateAssociatedButton(GetCtrlCWnd()->GetParent());
		}
	}
	else
	{
		m_bOwnHotKeyLink = FALSE;
		DoEnable(FALSE);
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DetachHotKeyLink(HotKeyLinkObj*, BOOL)
{
	DetachHotKeyLink();
}

//-----------------------------------------------------------------------------
//versione ripulita
void CParsedCtrl::DetachHotKeyLink()
{
	// stacca il corrente hotlink dal control
	if (m_pHotKeyLink)
	{
		m_pHotKeyLink->RemoveOwnerCtrl(this);
		if (m_bOwnHotKeyLink)
			delete m_pHotKeyLink;
		m_pHotKeyLink = NULL;
	}

	if (m_pButton)
	{
		m_pButton->DestroyWindow();
		delete m_pButton;
		m_pButton = NULL;
	}

	SAFE_DELETE(m_pHyperLink);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::CheckControl(UINT nID, CWnd* pParentWnd, LPCTSTR ctrlClassName /* = NULL */)
{
	ASSERT(m_pButton == NULL);
	ASSERT(m_pHyperLink == NULL);
	ASSERT(!m_bAttached);
	ASSERT(CheckDataObjType());

	CWnd* pParent = pParentWnd;
	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pParent);

	// devo controllare in quale contesto mi trovo, se in quello dell'AddOnApplication
	// oppure in quello globale di applicazione
	if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
		m_pFormatContext = &((CBaseFormView*)pParent)->GetNamespace();
	else
		if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
			m_pFormatContext = &((CParsedDialog*)pParent)->GetNamespace();
		else
			if (pGrid)
				m_pFormatContext = &pGrid->GetNamespace();

	if (m_nFormatIdx == UNDEF_FORMAT)
	{
		BOOL bNoOtherContext = FALSE;

		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
			bNoOtherContext = ((CBaseFormView*)pParent)->IsDisableOtherContext();
		else
			if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
				bNoOtherContext = ((CParsedDialog*)pParent)->IsDisableOtherContext();
			else
				if (pGrid)
					bNoOtherContext = pGrid->IsDisableOtherContext();

		m_nFormatIdx = AfxGetFormatStyleTable()->GetFormatIdx(GetDataType());
	}
	m_bAttached = TRUE;

#ifdef _DEBUG
	CWnd* pWndCtrl = pParentWnd->GetDlgItem(nID);
	if (ctrlClassName)
	{
		//ASSERT(pWndCtrl);

		if (pWndCtrl)
		{
			TCHAR className[MAX_CLASS_NAME + 1];
			ASSERT(::GetClassName(pWndCtrl->m_hWnd, className, MAX_CLASS_NAME));
			if (_tcsicmp(className, ctrlClassName) != 0)
			{
				TRACE
				(
					L"The developer is subclassing a %s control on the %s control that is related to the resource ID = %u\n",
					(LPCTSTR)m_pOwnerWnd->GetRuntimeClass()->m_lpszClassName,
					className, nID
				);
				ASSERT(FALSE);
				return FALSE;
			}
		}

	}
	else
		if (pWndCtrl)
		{
			TRACE
			(
				"The developer is creating twice the %s control related to the resource ID = %u\n",
				(LPCTSTR)m_pOwnerWnd->GetRuntimeClass()->m_lpszClassName, nID
			);
			ASSERT(FALSE);
			return FALSE;
		}
#endif
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::InitCtrl()
{
	VERIFY(OnInitCtrl());

	if (ShowZerosInInput())
		m_dwCtrlStyle |= NUM_STYLE_SHOW_ZERO;

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CParsedCtrl::FormatData(const DataObj* pDataObj, BOOL bEnablePadding) const
{
	ASSERT_VALID(pDataObj);

	// Viene formattato il dato
	const Formatter* pFormatter = GetCurrentFormatter();
	if (pFormatter == NULL)
		return pDataObj->Str();

	CString strCell;
	pFormatter->FormatDataObj(*pDataObj, strCell, bEnablePadding);

	return strCell;
}

//-----------------------------------------------------------------------------
const Formatter* CParsedCtrl::GetCurrentFormatter() const
{
	//se il dato è protetto ed l'utente corrente non lo può visualizzare (protetto o da OSL o dalla RowSecurity)	
	if (m_pData && (m_pData->IsOSLHide() || m_pData->IsPrivate()))
		return m_pPrivacyFormatter;

	return m_pFormatter ? m_pFormatter :
		(m_nFormatIdx >= 0 ?
			AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext) :
			NULL);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::OnInitCtrl()
{
	SetCtrlMaxLen(m_nCtrlLimit, TRUE);

	return TRUE;
}

// build associated button based on Style choosed by User. Also calculate real
// edit size reducing it by button size
//-----------------------------------------------------------------------------
BOOL CParsedCtrl::CreateAssociatedButton(CWnd* pParentWnd)
{
	if (m_pOwnerWnd == NULL)
		return FALSE;

	// deve essere fatta come prima cosa
	SetDefaultFontControl(m_pOwnerWnd, this);

	// chiama una metodo virtuale con il quale il programmatore puo` modificare
	// gli ID del bottone
	OnCreateAssociatedButton();

	CRect rectE(0, 0, 0, 0);

	// no button, so don't create it   
	if (m_nButtonIDBmp != NO_BUTTON && m_nButtonIDBmp != BTN_DEFAULT)
	{
		// keep some styles of associated edit
		DWORD dwGenericBtnStyle = (m_pOwnerWnd->GetStyle() & (WS_VISIBLE | WS_DISABLED)) | WS_CHILD;
		// associate button to current edit
		switch (m_nButtonIDBmp)
		{
		case BTN_SPIN_ID:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | SPIN_STYLE;

			// create button object
			m_pButton = new CSpin();

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("SpinButton"), TbResourceType::TbControls);
			if (!((CSpin*)m_pButton)->Create(dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			((CSpin*)m_pButton)->SetAssociate(m_pOwnerWnd);

			break;
		}

		case BTN_MENU_ID:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | BS_OWNERDRAW;

			m_pButton = new CMenuButton(m_pOwnerWnd, this);
			CMenuButton* pButton = (CMenuButton*)m_pButton;

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("MenuButton"), TbResourceType::TbControls);
			if (!pButton->Create(NULL, dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			int nButtonIDBmp = GetMenuButtonImage();
			if (nButtonIDBmp != -1)
				pButton->SetStateBitmaps(0, nButtonIDBmp, nButtonIDBmp + 3);
			else
				pButton->SetStateBitmaps(0, GetMenuButtonImageNS());

			pButton->SizeToContent();
			pButton->Invalidate();

			break;
		}

		case BTN_CALENDAR_ID:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | BS_OWNERDRAW;

			m_pButton = new CCalendarButton(m_pOwnerWnd, this);
			CCalendarButton* pButton = (CCalendarButton*)m_pButton;

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("CalendarButton"), TbResourceType::TbControls);
			if (!pButton->Create(NULL, dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			pButton->SetStateBitmaps(0, TBIcon(szIconCalendar, CONTROL));
			pButton->SizeToContent();

			pButton->Invalidate();

			break;
		}
		case BTN_COLOR_ID:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | BS_OWNERDRAW;

			m_pButton = new CColorButton(m_pOwnerWnd, this);
			CColorButton* pButton = (CColorButton*)m_pButton;

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("ColorButton"), TbResourceType::TbControls);
			if (!pButton->Create(NULL, dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			pButton->SetStateBitmaps(0, TBIcon(szIconColors, CONTROL));
			pButton->SizeToContent();
			pButton->Invalidate();

			break;
		}

		case BTN_OUTLOOK_ID:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | BS_OWNERDRAW;

			m_pButton = new COutlookButton(m_pOwnerWnd, this);
			COutlookButton* pButton = (COutlookButton*)m_pButton;

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("OutlookButton"), TbResourceType::TbControls);
			if (!pButton->Create(NULL, dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			pButton->SetStateBitmaps(0, TBIcon(szIconAddressBook, CONTROL));

			pButton->SizeToContent();
			pButton->Invalidate();

			break;
		}

		default:
		{
			DWORD dwBtnStyle = dwGenericBtnStyle | RADAR_STYLE;

			// create button object
			m_pButton = new CLinkButton(GetCtrlCWnd());

			if (m_nButtonID == DUMMY_ID)
				m_nButtonID = AfxGetTBResourcesMap()->GetTbResourceID(_T("HotLinkButton"), TbResourceType::TbControls);

			if (!((CLinkButton*)m_pButton)->Create(NULL, dwBtnStyle, rectE, pParentWnd, m_nButtonID))
				return FALSE;

			if (!AfxIsRemoteInterface())
			{
				VERIFY(((CLinkButton*)m_pButton)->LoadBitmaps(m_nButtonIDBmp));
				((CLinkButton*)m_pButton)->SizeToContent();
			}
			break;
		}
		}
	}
	if (m_pButton && !AfxGetThemeManager()->GetControlsUseBorders())
		m_pButton->ModifyStyle(WS_BORDER, 0);

	if (!m_pHyperLink && GetHotLink() && !GetHotLink()->GetAddOnFlyNamespace().IsEmpty())
	{
		m_pHyperLink = new CHyperLink(this);
		if (!m_pHyperLink->Create(pParentWnd))
			return FALSE;

		// se il control appartiene ad un bodyedit, l'hyperlink serve solo per aprire il documento
		// collegato e mentenerne il puntatore, non ci sono attività "visibili"
		if (dynamic_cast<CGridControlObj*>(pParentWnd) != NULL)
			m_pHyperLink->SetAlwaysHidden();

		m_pHyperLink->SetHLinkFont(AfxGetThemeManager()->GetHyperlinkFont());
		m_pHyperLink->Init();
	}

	if (m_pButton)
	{
		int expectedWidth = 0;
		CStateButton* pButton = (CStateButton*)m_pButton;
		if (pButton)
			expectedWidth = pButton->GetButtonWidth();

		ReserveSpaceForButton(GetButtonWidth(expectedWidth), (CButton*)m_pButton);

		if (GetHotLink() == NULL && m_nButtonIDBmp == BTN_DOUBLE_ID)
			m_pButton->ShowWindow(SW_HIDE);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::GetMenuButtonImage()
{
	return  -1; // backward compatibility
}

// Immagine da utilizzare per il bottone con menu' contestuale
//-----------------------------------------------------------------------------
CString CParsedCtrl::GetMenuButtonImageNS()
{
	return  TBIcon(szIconContextMenu, CONTROL);
}
//-----------------------------------------------------------------------------
void CParsedCtrl::AddToSpaceReservedArray(CWnd* pButton)
{
	m_arrySpaceForButtonReserved.Add(pButton);
}
//-----------------------------------------------------------------------------
void CParsedCtrl::RemoveFromSpaceReservedArray(CWnd* pButton)
{
	for (int i = 0; i < m_arrySpaceForButtonReserved.GetCount(); i++)
	{
		if (m_arrySpaceForButtonReserved[i] == pButton)
		{
			m_arrySpaceForButtonReserved.RemoveAt(i);
			break;
		}
	}
}

// Si occupa di far rientrare il control dello spazio occupato dal bottone
// mano a mano che i bottoni vengono creati.
//-----------------------------------------------------------------------------
void CParsedCtrl::ReserveSpaceForButton(UINT nWidthBtn, CButton* pButton)
{
	ASSERT_VALID(m_pOwnerWnd);
	ASSERT_VALID(pButton);

	if (!pButton)
		return;

	for (int i = 0; i < m_arrySpaceForButtonReserved.GetCount(); i++)
	{
		if (m_arrySpaceForButtonReserved.GetAt(i) == pButton)
			return;
	}


	if (IsScale())
	{
		// CDateEdit è gia scalato
		if (m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CDateEdit)))
			nWidthBtn = nWidthBtn + ScalePix(5);
		else
			nWidthBtn = ScalePix(nWidthBtn);
	}

	if (!m_pOwnerWnd || !pButton)
		return;
	AddToSpaceReservedArray(pButton);

	// area del control
	CRect rect;
	m_pOwnerWnd->GetWindowRect(rect);
	CWnd* pParent = m_pOwnerWnd->GetParent();
	pParent->ScreenToClient(rect);

	if (m_nButtonsXOffset == 0 || dynamic_cast<CGridControlObj*>(pParent) != NULL)
	{
		// rientro del control
		m_pOwnerWnd->SetWindowPos(NULL, rect.left, rect.top, rect.Width() - nWidthBtn - BTN_OFFSET, rect.Height(), SWP_NOZORDER);

		// se c'è l'hyperlink viene ridimensionato anch'esso
		if (m_pHyperLink)
		{
			CRect hyperRect(rect);
			hyperRect.right -= (nWidthBtn + BTN_OFFSET);
			SetHyperLinkPos(hyperRect, SWP_NOZORDER);
		}

		// bottone assiciato (hotlink)
		if (pButton && pButton == m_pButton)
		{
			pButton->SetWindowPos(NULL, rect.right - nWidthBtn + BTN_OFFSET, rect.top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
			pButton->Invalidate();
		}
		else
		{
			// eventuali altri bottoni di stato
			CRect aRect = rect;
			aRect.left = rect.right - nWidthBtn + BTN_OFFSET;

			// se c'è il bottone associato arretra quello per primo
			if (m_pButton)
			{
				m_pButton->GetWindowRect(&aRect);
				pParent->ScreenToClient(aRect);

				m_pButton->SetWindowPos(NULL, aRect.left - aRect.Width() - BTN_OFFSET, aRect.top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
				m_pButton->Invalidate();
			}

			// altrimenti fa arretrare tutti i bottonici già disegnati in precedenza
			for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
			{
				CStateCtrlObj* pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
				if (!pStateCtrl || !pStateCtrl->GetButton())
					continue;

				if (pStateCtrl->GetButton() == pButton)
					break;

				pStateCtrl->GetButton()->GetWindowRect(&aRect);
				pParent->ScreenToClient(aRect);

				pStateCtrl->GetButton()->SetWindowPos(NULL, aRect.left - aRect.Width() - BTN_OFFSET, rect.top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
				pStateCtrl->GetButton()->Invalidate();
			}

			// infine posiziona il nuovo bottone
			pButton->SetWindowPos(NULL, aRect.left, rect.top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
			pButton->Invalidate();
		}
	}
	else
	{
		int x = rect.right + m_nButtonsXOffset;
		// bottone assiciato (hotlink)
		if (pButton != m_pButton)
		{
			for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
			{
				CStateCtrlObj* pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
				if (!pStateCtrl || !pStateCtrl->GetButton())
					continue;

				if (pStateCtrl->GetButton() == pButton)
					break;

				CRect r;
				pStateCtrl->GetButton()->GetWindowRect(&r);
				pParent->ScreenToClient(r);

				x += r.Width() + BTN_OFFSET;
			}
		}
		pButton->SetWindowPos(NULL, x, rect.top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
		pButton->Invalidate();
	}
}

// SizeToContent will resize the button to the size of the bitmap
//-----------------------------------------------------------------------------
int CParsedCtrl::GetButtonWidth(int nExpectedWidth/*=0*/)
{
	if (m_pButton == NULL) return 0;

	if (m_nButtonIDBmp == BTN_SPIN_ID)
		return nExpectedWidth > 0 ? Min(nExpectedWidth, MAX_SPIN_WIDTH) : MAX_SPIN_WIDTH;

	CWalkBitmap bitmap;
	if (!bitmap.LoadBitmap(MAKEINTRESOURCE(m_nButtonIDBmp)))
		return nExpectedWidth > 0 ? nExpectedWidth : MAX_SPIN_WIDTH;

	ASSERT(bitmap.m_hObject != NULL);

	CSize bitmapSize;
	BITMAP bmInfo;
	VERIFY(bitmap.GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));
	return bmInfo.bmWidth;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetButtonPos(CRect& rectBtn, UINT nFlags)
{
	switch (m_nButtonIDBmp)
	{
	case NO_BUTTON:
	case BTN_DEFAULT:
		break;

	case BTN_SPIN_ID:
	{
		// limit spin height for estetic purpose
		if (rectBtn.Height() > MAX_SPIN_HEIGHT)
			rectBtn.bottom = rectBtn.top + MAX_SPIN_HEIGHT;

		m_pButton->SetWindowPos
		(
			NULL,
			rectBtn.left, rectBtn.top,
			rectBtn.Width(), rectBtn.Height(),
			(nFlags & ~SWP_NOMOVE) | SWP_NOACTIVATE
		);
		break;
	}

	default:
	{
		m_pButton->SetWindowPos
		(
			NULL,
			rectBtn.left, rectBtn.top, 0, 0,
			(nFlags & ~SWP_NOMOVE) | SWP_NOSIZE | SWP_NOACTIVATE
		);
		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetHyperLinkPos(CRect& rectEdit, UINT nFlags)
{
	if (m_pHyperLink)
		m_pHyperLink->OverlapToControl(rectEdit, nFlags);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsValid()
{

	// in find mode fields are not mandatory
	if (
		m_pDocument &&
		m_pDocument->GetFormMode() == CBaseDocument::FIND &&
		(GetCtrlStyle() & STR_STYLE_NO_EMPTY) == STR_STYLE_NO_EMPTY
		)
		return TRUE;

	if (
		AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
		AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::PLAYING &&
		m_nErrorID == DUMMY && m_nWarningID == DUMMY
		)
		return TRUE;

	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		ASSERT(pStateCtrl);
		if (!pStateCtrl->IsValid())
			return FALSE;
	}

	m_nErrorID = EMPTY_MESSAGE;
	m_nWarningID = EMPTY_MESSAGE;
	m_nMsgBoxStyle = MB_ICONEXCLAMATION | MB_OKCANCEL;
	m_strMessage.Empty();

	AfxGetBaseApp()->SetWarning(0);
	AfxGetBaseApp()->SetError(0);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsValid(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	return CParsedCtrl::IsValid();
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::UpdateCtrlData(BOOL bEmitError, BOOL bSendMessage /* = FALSE */)
{
	CBaseDocument* pDoc = GetDocument();
	AfxGetBaseApp()->SetOldCtrlData(NULL, NULL);
	//okkio che qui la faccenda è delicata.
	//la ResetValueChanging serve a ripristinare lo stato di m_nValueChanging a 0, alterato da una SendMessage di EN_VALUE_CHANGED
	//questo stato ha influenza sull'aggiornamento dello stato del parsed control, che non deve essere effettuato durante una change
	//La UpdateDataView, chiamata nel distruttore di CUpdateDataViewLevel, deve essere chiamata PRIMA che il valore di m_nValueChanging venga resettato,
	//altrimenti si verificano casi di cambiamenti di stato non voluti
	//per questo, prima va allocata la variabile _reset, e dopo la __upd
	ResetValueChanging _reset(m_nValueChanging);
	CUpdateDataViewLevel __upd(pDoc);
	BOOL bFindMode = pDoc && pDoc->GetFormMode() == CBaseDocument::FIND;
	BOOL bOk = !m_bUserRunning && (!m_pHotKeyLink || !m_pHotKeyLink->IsHotLinkRunning());
	if (!bOk)
		m_nWarningID = HOTLINK_IS_RUNNING;

	// Va fatto prima della IsValid()

	// TODO: ripristinare in alternativa alla riga successiva? Chiedere a Germano. 
	// BOOL bSaveOldData = !IsEmptyMessage(m_nErrorID) && !IsEmptyMessage(m_nWarningID);
	BOOL bSaveOldData = !m_nErrorID && !m_nWarningID;

	//@@ Prima della Release 2.5.0.0
	//	bOk = bOk && IsValid() && !HasBeenInvalidated(!GetModifyFlag());

	if (bOk)
	{
		
		// In ogni caso si fa il reset degli stati di errore
		CParsedCtrl::IsValid();

		// Se non e` un PRIMO/ULTIMO/AUTO_EXPRESSION oppure l'utente ha digitato qualcosa
		// esegue la validazione del dato
		if (
			GetModifyFlag() ||
			(
				GetCtrlStyle() &
				(
					CTRL_STYLE_SHOW_FIRST |
					CTRL_STYLE_SHOW_LAST |
					CTRL_STYLE_STORED_AUTO_EXPRESSION
					)
				) == 0
			)
		{
			if (m_pDataAdapter)
			{
				DataObj* pData = m_pData->Clone();
				GetValue(*pData);
				if (m_pDataAdapter->ChangeValue(pData))
					SetValue(*pData);
				delete pData;
			}
			// Viene chiamato il metodo virtualmente overloadato
			bOk = IsValid() && IsValidForAllValidators();
			if (HasBeenInvalidated(!GetModifyFlag()))
				bOk = FALSE;
		}
	}

	// Nel caso di stato di find viene trappato solo il caso di errore di sintassi per le Date
	//
	if (!bOk && bFindMode && m_nErrorID != DATE_EDIT_BAD_FORMAT)
	{
		bOk = TRUE;
		m_nErrorID = EMPTY_MESSAGE;
		m_nWarningID = EMPTY_MESSAGE;
		m_strMessage.Empty();
	}

	BOOL bValidModified = bOk && GetModifyFlag();
	if (bValidModified)
	{
		SetModifyFlag(FALSE);

		if (m_pData)
		{
			if (bSaveOldData)
				SaveOldCtrlData();

			// save new data in connected DataObj
			GetValue(*m_pData);

			if (!bFindMode)
			{
				if (
					m_pHotKeyLink &&
					!m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CListBox)) &&
					!m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)) &&
					m_pHotKeyLink->IsHotLinkEnabled() &&
					!m_pHotKeyLink->ExistData(m_pData))
				{
					if (m_nErrorID && m_nErrorID != HOTLINK_DATA_NOT_FOUND)
						RestoreOldCtrlData();

					// viene impedita la perdita del fuoco
					if (
						!bEmitError &&
						(
						(m_nErrorID && m_nErrorID != HOTLINK_DATA_NOT_FOUND) ||
							m_pHotKeyLink->m_bMustExistData
							)
						)
						SetModifyFlag(TRUE);

					// se non e` richiesta l'emissione del messaggio di errore (nella
					// fattispecie il messaggio specifico dell'hotlink associato: vedi
					// CParsedCtrl::ErrorMessage) allora viene spento il segnale di
					// running dell'hotlink
					// 
					if (!bEmitError)
						m_pHotKeyLink->SetRunningMode(0);

					bOk = FALSE;
				}
				else
				{
					// send to the parent a generic value changed message to process
					// the document modification 
					if (AfxGetBaseApp()->SetOldCtrlData(m_pData, m_pOldData))
					{
						_reset.ValueChanging();
						GetCtrlParent()->SendMessage(UM_VALUE_CHANGED, GetCtrlID(), (LPARAM)m_pData);
						// I dispatch the message as command to controllers

						AfxGetBaseApp()->SetOldCtrlData(m_pData, NULL);
					}

					bOk = !HasBeenInvalidated();
					if (!bOk)
						RestoreOldCtrlData();

					if (m_pData->IsUpdateView())
					{
						m_pData->SetUpdateView(FALSE);
						UpdateCtrlView();
					}
				}
			}
		}

		if (bOk)
		{
			// send a control specific message if the value is changed
			if (AfxGetBaseApp()->SetOldCtrlData(m_pData, m_pOldData))
			{
				if (bFindMode)
				{
					NotifyToParent(EN_VALUE_CHANGED_FOR_FIND);
				}
				else
				{
					NotifyToParent(EN_VALUE_CHANGED);
					if (m_pControlBehaviour)
						m_pControlBehaviour->OnValueChanged();
				}

				AfxGetBaseApp()->SetOldCtrlData(m_pData, NULL);
			}

			if (!bFindMode)
			{
				bOk = !HasBeenInvalidated();
				if (!bOk)
					RestoreOldCtrlData();
				else
					if (m_nWarningID == 0)
						NotifyToParent(EN_DATA_UPDATED, bSendMessage);

			}

			if (m_pData && m_pData->IsUpdateView())
			{
				m_pData->SetUpdateView(FALSE);
				UpdateCtrlView();
			}
		}
	}

	if (!bOk)
	{
		if (m_nErrorID == EMPTY_MESSAGE)
			return FALSE;

		LPARAM nMode = bEmitError ? CTRL_IMMEDIATE_FATAL_ERROR : CTRL_FATAL_ERROR;
		SendMessageToAncestor(bSendMessage, UM_BAD_VALUE, GetCtrlID(), nMode);

		return FALSE;
	}

	if (m_nWarningID)
	{
		LPARAM nMode = ((m_nMsgBoxStyle & MB_ICONINFORMATION) == MB_ICONINFORMATION) | bEmitError
			? CTRL_IMMEDIATE_WARNING_ERROR
			: CTRL_WARNING_ERROR;

		SendMessageToAncestor(bSendMessage, UM_BAD_VALUE, GetCtrlID(), nMode);

		return FALSE;
	}

	if (bValidModified)
	{
		if (m_pData && m_pData->IsEmpty())
			if ((GetCtrlStyle() & CTRL_STYLE_SHOW_FIRST) == CTRL_STYLE_SHOW_FIRST)
				SetValue(CParsedCtrl::Strings::FIRST());
			else
				if ((GetCtrlStyle() & CTRL_STYLE_SHOW_LAST) == CTRL_STYLE_SHOW_LAST)
					SetValue(CParsedCtrl::Strings::LAST());

		if (
			GetControllerMode() != CExternalControllerInfo::NONE ||
			(pDoc && pDoc->m_pAutoExpressionMng)
			)
		{
			m_strAutomaticExpression.Empty();
			SetCtrlStyle(GetCtrlStyle() & ~CTRL_STYLE_STORED_AUTO_EXPRESSION);

			if (pDoc && pDoc->m_pAutoExpressionMng)
			{
				//TODO
				//GetDocument()->GetVariableArray()->GetAt(i)->GetDataObj()

				for (int i = 0; i < pDoc->m_pAutoExpressionMng->GetSize(); i++)
				{
					if (pDoc->m_pAutoExpressionMng->GetAt(i)->GetDataObj() == m_pData)
					{
						pDoc->m_pAutoExpressionMng->RemoveAt(i);
						if (pDoc)
							pDoc->ResumeUpdateDataView();

						return TRUE;
					}
				}
			}
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::HasBeenInvalidated(BOOL bCheckDataStatus /* = TRUE */)
{
	if (
		(bCheckDataStatus && m_pData && !m_pData->IsValid()) ||
		m_nErrorID || AfxGetBaseApp()->ErrorFound()
		)
	{
		if (m_nErrorID == EMPTY_MESSAGE)
		{
			// quelli dell'applicazione solo solo errori stringa, non hanno ID
			if (AfxGetBaseApp()->ErrorFound())
			{
				m_nErrorID = USER_STRING_ERROR_MESSAGE;
				m_strMessage = AfxGetBaseApp()->GetError();
				AfxGetBaseApp()->ClearError();
			}

			if (m_nErrorID == EMPTY_MESSAGE)
				m_nErrorID = GENERIC_BAD_VALUE;
		}

		SetModifyFlag(TRUE);
		return TRUE;
	}

	if (m_nWarningID || AfxGetBaseApp()->WarningFound())
	{
		if (m_nWarningID == EMPTY_MESSAGE)
		{
			if (AfxGetBaseApp()->WarningFound())
			{
				m_nWarningID = USER_STRING_ERROR_MESSAGE;
				m_strMessage = AfxGetBaseApp()->GetWarning();
				if (AfxGetBaseApp()->IsNoCancOnWarning())
					m_nMsgBoxStyle = MB_ICONINFORMATION | MB_OK;
				else
					m_nMsgBoxStyle = MB_ICONEXCLAMATION | MB_OKCANCEL;

				// questo resetta anche il flag di IsNoCancOnWarning
				AfxGetBaseApp()->ClearWarning();
			}
		}

		if ((m_nMsgBoxStyle & MB_ICONEXCLAMATION) == MB_ICONEXCLAMATION)
			SetModifyFlag(TRUE);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SaveOldCtrlData()
{
	if (!m_pData) return;

	if (m_pOldData == NULL)
		m_pOldData = m_pData->DataObjClone();
	else
		m_pOldData->Assign(*m_pData);
}

//-----------------------------------------------------------------------------
CString CParsedCtrl::FormatMessage(MessageID ID)
{
	switch (ID)
	{
	case EMPTY_MESSAGE:			return _T("");
	case FIELD_REDEFINED:		return _TB("{0-%s}: field already defined");
	case FIELD_NOT_FOUND:		return _TB("{0-%s}: field not found");
	case FIELD_EMPTY:			return _TB("Empty field not allowed");
	case STR_EDIT_EMPTY:		return _TB("Empty value not allowed");
	case INT_EDIT_OUT_RANGE:	return _TB("The integer value {0-%s} is outside the valid range:\r\n[{1-%d}, {2-%d}] ");
	case LONG_EDIT_OUT_RANGE:	return _TB("The extended value {0-%s} is outside the valid range:\r\n[{1-%ld}, {2-%ld}]");
	case DOUBLE_EDIT_OUT_RANGE:	return _TB("The number {0-%s} is outside the valid range:\r\n[{1-%f}, {2-%f}]");
	case DATE_EDIT_BAD_FORMAT:	return _TB("The date {0-%s} is not valid.\r\nValid format: {1-%s}");
	case DATE_EDIT_DEMO:		return _TB("The program is in the demonstration version. \r\nValid dates are comprised between 1 January 1998 and 31 December 1999.");
	case DATE_EDIT_OUT_RANGE:	return _TB("The date {0-%s} is outside the valid range:\r\n[{1-%s}, {2-%s}]");
	case PATH_EDIT_BAD_PATH:	return _TB("Wrong format of {0-%s} path");
	case PATH_EDIT_BAD_FILE:	return _TB("Wrong format of {0-%s} format\r\n");
	case PATH_EDIT_EMPTY_PATH:	return _TB("The path cannot be empty");
	case PATH_EDIT_EMPTY_FILE:	return _TB("Filename cannot be empty");
	case PATH_EDIT_NO_PATH:		return _TB("The path:\r\n{0-%s}\r\n does not exist\r\n");
	case PATH_EDIT_NO_FILE:		return _TB("The file:\r\n{0-%s}\r\n does not exist");
	case TIME_EDIT_BAD_FORMAT:	return _TB("The time {0-%s} is not valid.\r\nValid format: {1-%s}");
	case TIME_EDIT_OUT_RANGE:	return _TB("The time {0-%s} is outside the valid range:\r\n[{1-%s}, {2-%s}] ");
	case GENERIC_BAD_VALUE:		return _TB("{0-%s}: invalid data");
	case HOTLINK_DATA_NOT_FOUND:return _TB("{0-%s}: data not found");
	case HOTLINK_RECORD_NOT_FOUND:return _TB("{0-%s}: data not found");
	case HOTLINK_DATA_PROTECTED:return _TB("{0-%s}: protected data.\r\nIn order to see it you must have grants.");
	case HOTLINK_IS_RUNNING:	return _TB("Unable to execute the request before the current operation is ended");
	case MAX_ITEM_REACHED:		return _TB("!!! THE NUMBER OF ELEMENTS IS HIGHER THAN THE EXTRACTABLE LIMIT ({0-%d}) SET!");
	case NAMESPACE_EDIT_BAD_NAMESPACE: return _TB("Invalid Namespace name.");
	case DUMMY:					return _T("");
	default:
		ASSERT(FALSE);
		return _T("");
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::RestoreOldCtrlData(BOOL bShow /*= FALSE */)
{
	if (!m_pData || !m_pOldData) return;

	m_pData->Assign(*m_pOldData);

	if (bShow)
		UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CParsedCtrl::UpdateCtrlView()
{
	if (m_nValueChanging || AfxIsRemoteInterface())
		return;

	if (m_pData)
	{
		if (m_pData->IsEmpty() || m_pData->IsLowerValue() || m_pData->IsUpperValue())
		{
			if ((GetCtrlStyle() & CTRL_STYLE_SHOW_FIRST) == CTRL_STYLE_SHOW_FIRST)
			{
				SetValue(CParsedCtrl::Strings::FIRST());
				return;
			}
			else if ((GetCtrlStyle() & CTRL_STYLE_SHOW_LAST) == CTRL_STYLE_SHOW_LAST)
			{
				SetValue(CParsedCtrl::Strings::LAST());
				return;
			}
		}
		if (m_pData->IsEmpty() && IsAutomaticExpression())
		{
			SetValue(AUTO_EXPRESSION_SIGNAL);
			return;
		}

		SetValue(*m_pData);

		if (m_pHyperLink)
			m_pHyperLink->UpdateCtrlView();
	}

	int nCmdShow = SW_SHOW;
	// nascondo gli accessori del control solo se il control non 
	// è visibile, ma il suo parent si. Se non controllo il parent
	// mi si nasconde sempre a causa della OnInitialUpdate in cui
	// il control non è ancora completamente visibile.
	if (
		GetCtrlCWnd() && !IsTBWindowVisible(GetCtrlCWnd()) &&
		GetCtrlCWnd()->GetParent() && IsTBWindowVisible(GetCtrlCWnd()->GetParent())
		)
		nCmdShow = SW_HIDE;

	// bottoni
	UpdateStateButtons(nCmdShow);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::UpdateCtrlStatus()
{
	if (m_nValueChanging)
		return;

	OnUpdateCtrlStatus();

	if (m_pData)
	{
		EnableCtrl(!m_pData->IsReadOnly());
		bool bVisible = (m_pOwnerWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE;
		bool bHide = TRUE == m_pData->IsHide();
		if (bVisible == bHide)
			ShowCtrl (m_pData->IsHide() ? SW_HIDE : SW_SHOW);
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsFindMode() const
{
	return GetDocument() && (GetDocument()->GetFormMode() == CBaseDocument::FIND);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::ModifiedCtrlData()
{
	CBaseDocument* pDoc = GetDocument();
	AfxGetBaseApp()->SetOldCtrlData(NULL, NULL);
	//okkio che qui la faccenda è delicata.
	//la ResetValueChanging serve a ripristinare lo stato di m_nValueChanging a 0, alterato da una SendMessage di EN_VALUE_CHANGED
	//questo stato ha influenza sull'aggiornamento dello stato del parsed control, che non deve essere effettuato durante una change
	//La UpdateDataView, chiamata nel distruttore di CUpdateDataViewLevel, deve essere chiamata PRIMA che il valore di m_nValueChanging venga resettato,
	//altrimenti si verificano casi di cambiamenti di stato non voluti
	//per questo, prima va allocata la variabile _reset, e dopo la __upd
	ResetValueChanging _reset(m_nValueChanging);
	CUpdateDataViewLevel __upd(pDoc);
	BOOL bOk = TRUE;
	BOOL bFindMode = IsFindMode();

	if (m_pData)
	{
		//@@ Prima della Release 2.5.0.0
		//	bOk = IsValid(*m_pData) && !HasBeenInvalidated();

		bOk = IsValid(*m_pData);
		if (HasBeenInvalidated())
			bOk = FALSE;

		if (bOk)
		{
			// prima di tutto visualizza il dato
			SetValue(*m_pData);

			// send to the parent a generic value changed message to process
			// the document modification
			if (!bFindMode)
			{
				if (AfxGetBaseApp()->SetOldCtrlData(m_pData, m_pOldData))
				{
					_reset.ValueChanging();
					GetCtrlParent()->SendMessage(UM_VALUE_CHANGED, GetCtrlID(), (LPARAM)m_pData);

					AfxGetBaseApp()->SetOldCtrlData(m_pData, NULL);
				}
				bOk = !HasBeenInvalidated();
			}
		}
	}

	if (bOk)
	{
		// send a control specific message if the value is changed
		if (AfxGetBaseApp()->SetOldCtrlData(m_pData, m_pOldData))
		{
			if (bFindMode)
			{
				NotifyToParent(EN_VALUE_CHANGED_FOR_FIND);
			}
			else
			{
				NotifyToParent(EN_VALUE_CHANGED);
				if (m_pControlBehaviour)
					m_pControlBehaviour->OnValueChanged();
			}
			AfxGetBaseApp()->SetOldCtrlData(m_pData, NULL);
		}
		if (!bFindMode)
			bOk = !HasBeenInvalidated();
	}

	if (m_pOldData)
	{
		delete m_pOldData;
		m_pOldData = NULL;
	}

	if (!bOk)
	{
		if (m_nErrorID)
			SendMessageToAncestor(FALSE, UM_BAD_VALUE, GetCtrlID(), CTRL_IMMEDIATE_FATAL_ERROR);

		return;
	}

	if (m_nWarningID)
		SendMessageToAncestor(FALSE, UM_BAD_VALUE, GetCtrlID(), CTRL_IMMEDIATE_WARNING_ERROR);
	else
		NotifyToParent(EN_DATA_UPDATED, FALSE);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::ForceUpdateCtrlView(int /*i*/)
{
	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::OnUpdateCtrlStatus(int i)
{
}

//-----------------------------------------------------------------------------
void CParsedCtrl::EnableHotLinkController(const BOOL bEnable /*TRUE*/, const CString& sDecodeNs /*_T("")*/)
{
	if (bEnable)
	{
		if (!m_pHotKeyLink)
			ASSERT_TRACE(FALSE, "EnableHotLinkController invoked without hotkeylink!")
		else if (!m_pHotLinkController)
		{
			m_pHotLinkController = new CHotLinkController(this);
			if (!sDecodeNs.IsEmpty())
				m_pHotLinkController->AddDefaultDecode(sDecodeNs);
		}
	}
	else
	{
		SAFE_DELETE(m_pHotLinkController);
	}
}
//-----------------------------------------------------------------------------
void CParsedCtrl::SetControlBehaviour(CControlBehaviour* pControlBehaviour)
{
	m_pControlBehaviour = pControlBehaviour;
	if (pControlBehaviour)
	{
		//potrei avere più parsed control con lo stesso behaviour (vista di riga)
		if (m_pHotKeyLink)
		{
			ASSERT(pControlBehaviour->m_pControlHotLink == NULL || pControlBehaviour->m_pControlHotLink == (HotKeyLink*)m_pHotKeyLink);
			pControlBehaviour->m_pControlHotLink = (HotKeyLink*)m_pHotKeyLink;
		}
		pControlBehaviour->m_pFormatContext = m_pFormatContext;
		pControlBehaviour->m_nFormatIdx = m_nFormatIdx;
		pControlBehaviour->m_pControlData = m_pData;
		pControlBehaviour->m_arControlIDs.Add(GetCtrlID());
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetItemSource(IItemSource* pItemSource)
{
	if (m_pItemSource)
		m_pItemSource->SetControl(NULL);
	m_pItemSource = pItemSource;

	if (m_pItemSource)
		m_pItemSource->SetControl(this);
}
//-----------------------------------------------------------------------------
BOOL CParsedCtrl::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	if (AfxIsRemoteInterface())
	{
		if (!m_pOwnerWndDescription)
			return FALSE;
		BOOL bOld = m_pOwnerWndDescription->m_bEnabled;
		m_pOwnerWndDescription->m_bEnabled = bEnable == TRUE;
		m_pOwnerWndDescription->SetUpdated(&m_pOwnerWndDescription->m_bEnabled);
		return bOld;
	}
	ASSERT(m_pOwnerWnd);

	if (m_StateCtrls.GetSize())
	{
		int nAction = 0;
		CStateCtrlObj* pStateCtrl;
		for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
		{
			pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
			ASSERT(pStateCtrl);
			nAction = pStateCtrl->IsToEnableCtrl();
		}

		BOOL bIsEdit = m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CEdit));
		BOOL bIsComboBox = m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CComboBox));
		BOOL bManageAsReadOnly = (nAction == 2 || nAction == 1) && (bIsEdit || bIsComboBox);

		// gestione del readonly
		if (bManageAsReadOnly)
			SetEditReadOnly(nAction == 2);
		else
		{
			// disabilitato/abilitato
			bEnable = nAction == 1;
			SetEditReadOnly(!bEnable);
		}
	}

	BOOL bCurrEnabled = m_pOwnerWnd->IsWindowEnabled();
	if ((bCurrEnabled && bEnable) || (!bCurrEnabled && !bEnable))
	{
		// Deve essere chiamata esplicitamente perche` se il control non cambia
		// stato di abilitazione, Windows non invia il messaggio WM_ENABLE e quindi
		// il control non intercetta la presente azione (che invece potrebbe
		// coinvolgere il bottone associato) e di conseguenza non chiama la
		// DoEnable
		DoEnable(bEnable);
	}
	else if (m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
	{
		if (m_pHyperLink || m_pCaption)
			DoEnable(bEnable);

		return TRUE;
	}
	//else if	(
	//		m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)) &&
	//		//m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)) && ((m_pOwnerWnd->GetStyle() & CBS_DROPDOWNLIST) == CBS_DROPDOWNLIST) &&
	//		m_pHyperLink
	//	)
	//{
	//	DoEnable(bEnable);
	//}

	return m_pOwnerWnd->EnableWindow(bEnable);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::ShowCtrl(int nCmdShow)
{
	if (AfxIsRemoteInterface())
	{
		if (!m_pOwnerWndDescription)
			return FALSE;
		bool bShow = nCmdShow != SW_HIDE;

		if (m_pOwnerWndDescription->m_bVisible == bShow)
			return FALSE;
		m_pOwnerWndDescription->m_bVisible = bShow;
		m_pOwnerWndDescription->SetUpdated(&m_pOwnerWndDescription->m_bVisible);
		return TRUE;
	}
	ASSERT(m_pOwnerWnd);

	if (m_pButton)
		m_pButton->ShowWindow(nCmdShow);

	if (m_pHyperLink)
		m_pHyperLink->ShowCtrl(nCmdShow);
	if (m_pCaption)
		m_pCaption->ShowWindow(nCmdShow);
	UpdateStateButtons(nCmdShow);

	return m_pOwnerWnd->ShowWindow(nCmdShow);
}

//-----------------------------------------------------------------------------
CWnd* CParsedCtrl::SetCtrlFocus(BOOL bSetSel/* = FALSE*/)
{
	ASSERT(m_pOwnerWnd);

	CWnd* pWnd = m_pOwnerWnd->SetFocus();

	if (bSetSel)
		SetCtrlSel(0, -1);

	return pWnd;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::SetCtrlPos(const CWnd* pWndInsertAfter, int x, int y, int cx, int cy, UINT nFlags)
{
	ASSERT(m_pOwnerWnd);
	return m_pOwnerWnd->SetWindowPos(pWndInsertAfter, x, y, cx, cy, nFlags);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlSel(int, int)
{
	// Do nothing for default
}

//-----------------------------------------------------------------------------
void CParsedCtrl::ClearCtrl()
{
	SetValue(_T(""));
}

//-----------------------------------------------------------------------------
CSize CParsedCtrl::SetCtrlSize(UINT nCols, UINT nRows, BOOL bButtonsIncluded)
{
	ASSERT(m_pOwnerWnd);

	// per il dimensionamento del control la AdaptNewSize (che e` virtuale e
	// reimplentata per esempio in parscbx.cpp) DEVE essere chiamata con l'ultimo
	// parametro SEMPRE uguale a TRUE
	//
	CSize cs = AdaptNewSize(nCols, nRows, TRUE);
	SetCtrlPos(NULL, 0, 0, cs.cx, cs.cy, SWP_NOZORDER | SWP_NOMOVE | SWP_NOACTIVATE);

	// nel caso si desidera conoscere la larghezza della sola parte editabile viene
	// richiamta la AdaptNewSize con l'ultimo parametro a FALSE per non includere
	// l'eventuale bottone della tendina (se si tratta di una combobox)
	//
	if (!bButtonsIncluded)
		return AdaptNewSize(nCols, nRows, FALSE);

	cs.cx += GetAllButtonsWitdh();

	return cs;
}

//-----------------------------------------------------------------------------
int	CParsedCtrl::GetAllButtonsWitdh()
{
	// calcolo quanto spazio mi occupano i bottoni
	int nBtnWidth = GetButtonWidth(0);

	CStateCtrlObj* pStateCtrl;

	// faccio rientrare il control x il numero di bottoni che disegnerò.
	// ATTENZIONE: 99% potrebbero non essere ancora disegnati, quindi non
	// posso assolutamente controllare la dimensione della loro finestra
	// in quanto GetButton() è ancora NULL. Vado secondo le dimensioni usate

	// per l'hotlink
	int nBtnsStateWidth = 0;
	for (int i = m_StateCtrls.GetUpperBound(); i >= 0; i--)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (pStateCtrl->GetButton())
			nBtnsStateWidth += (pStateCtrl->GetButtonWidth() + BTN_OFFSET);
	}

	return (nBtnWidth ? nBtnWidth + BTN_OFFSET : 0) + nBtnsStateWidth;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetValue(LPCTSTR pszValue)
{
	ASSERT(pszValue);
	ASSERT_VALID(m_pOwnerWnd);

	BOOL bModified = m_pOwnerWnd->SendMessage(EM_GETMODIFY, 0, 0);
	//se il dato è protetto ed l'utente corrente non lo può visualizzare (protetto o da OSL o dalla RowSecurity)
	if (m_pData && (m_pData->IsOSLHide() || m_pData->IsPrivate()))
	{
		CString strValue = _T("*");
		if (m_pPrivacyFormatter)  m_pPrivacyFormatter->FormatDataObj(*m_pData, strValue);
		m_pOwnerWnd->SetWindowText(strValue);
	}
	else
		m_pOwnerWnd->SetWindowText(pszValue);

	//la setwindow text mi fa perdere lo stato di modify
	if (bModified)
		m_pOwnerWnd->SendMessage(EM_SETMODIFY, TRUE, 0);

}

//-----------------------------------------------------------------------------
void CParsedCtrl::GetValue(CString&	strValue)
{
	m_pOwnerWnd->GetWindowText(strValue);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AddValidator(IValidator* pValidator)
{
	if (pValidator)
		m_Validators.Add(pValidator);
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsValidForAllValidators()
{
	if (m_Validators.GetSize() <= 0)
		return TRUE;

	BOOL isValid = TRUE;
	CString errorMessages;
	bool isError = false;
	DataObj* pData = m_pData->Clone();
	GetValue(*pData);
	for (int i = 0; i < m_Validators.GetSize(); i++)
	{
		CString error;
		CDiagnostic::MsgType type = CDiagnostic::Info;
		isValid &= m_Validators.GetAt(i)->IsValid(pData, error, type);
		if (!error.IsEmpty())
		{
			if (!errorMessages.IsEmpty())
				errorMessages += _T("\n");
			errorMessages += error;
			switch (type)
			{
			case CDiagnostic::Error:
			case CDiagnostic::FatalError:
				isError = true;
				break;
			}
		}


	}
	delete pData;
	if (!errorMessages.IsEmpty())
	{
		if (isError)
			SetError(errorMessages);
		else
			SetWarning(errorMessages);
	}


	return isValid;
}

//-----------------------------------------------------------------------------
CSize CParsedCtrl::AdaptNewSize(UINT nCols, UINT nRows, BOOL /* bButtonsIncluded */)
{
	// + H_EXTRA_EDIT_PIXEL sulla x per tener conto dei bordi (vedi GetEditSize(...) in generic.cpp)
	CDC* pDC = m_pOwnerWnd->GetDC();
	CSize sz(
		GetDataSize(nCols) + H_EXTRA_EDIT_PIXEL,
		GetEditSize(pDC, GetPreferredFont(), 1, nRows).cy
	);
	m_pOwnerWnd->ReleaseDC(pDC);
	return sz;
}

//-----------------------------------------------------------------------------

CFont*	CParsedCtrl::GetPreferredFont() const
{
	return (
		m_pCustomFont ?
		m_pCustomFont->GetWndPreferredFont() :
		((m_pOwnerWnd && ::IsWindow(m_pOwnerWnd->m_hWnd)) ? m_pOwnerWnd->GetFont() : AfxGetThemeManager()->GetControlFont())
		);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlFont(CFont* pFont, BOOL bRedraw /* = TRUE */)
{
	ASSERT_VALID(m_pOwnerWnd);
	ASSERT_VALID(pFont);

	if (m_pCustomFont)
		m_pCustomFont->SetWndCtrlFont(pFont, bRedraw);
	else
		m_pOwnerWnd->SetFont(pFont, bRedraw);

	if (m_pHyperLink)
		m_pHyperLink->SetHLinkFont(pFont);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::OnPrepareAuxData()
{
	if (m_pHotLinkController)
		m_pHotLinkController->OnPrepareAuxData();

	// sono in automatic quindi chiedo al servizio di numerare
	IBehaviourContext* pContext = m_pDocument->GetBehaviourContext();
	if (pContext && this->m_pNumbererRequest)
	{
		INumbererService* pService = dynamic_cast<INumbererService*>(pContext->GetService(m_pNumbererRequest));
		if (pService)
		{
			pService->ReadInfo(m_pNumbererRequest);
		}

	}


	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		CStateCtrlObj* pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		if (pStateCtrl)
		{
			pStateCtrl->EnableCtrlInEditMode(m_pNumbererRequest->GetEnableCtrlInEditMode());
			pStateCtrl->ProcessFormModeChanged(m_pNumbererRequest);
		}

	}
}

///////////////////////////////////////////////////////////////////////////////
CCustomFont::CCustomFont(CWnd* pWnd)
	:
	m_pWnd(pWnd),
	m_pOwnFont(NULL),
	m_bOwnsFont(FALSE)
{
}

CCustomFont::~CCustomFont()
{
	if (m_pOwnFont && m_bOwnsFont)
	{
		m_pOwnFont->DeleteObject();
		SAFE_DELETE(m_pOwnFont);
	}
}

//-----------------------------------------------------------------------------
CFont*	CCustomFont::GetWndPreferredFont() const
{
	if (m_pOwnFont)
		return m_pOwnFont;
	CFont* pFont = (m_pWnd && ::IsWindow(m_pWnd->m_hWnd)) ? m_pWnd->GetFont() : NULL;
	return pFont ? pFont : AfxGetThemeManager()->GetControlFont();
}

//-----------------------------------------------------------------------------
void CCustomFont::SetWndCtrlFont(CFont* pFont, BOOL bRedraw /* = TRUE */)
{
	ASSERT_VALID(pFont);
	if (!pFont)
		return;
	ASSERT_VALID(m_pWnd);
	if (!m_pWnd)
		return;
	m_pWnd->SetFont(pFont, bRedraw);
}

//-----------------------------------------------------------------------------
void CCustomFont::SetOwnFont(CFont* pFont, BOOL bOwns/* = FALSE*/)
{
	ASSERT_VALID(pFont);
	if (!pFont)
		return;

	if (m_pOwnFont && m_bOwnsFont)
	{
		m_pOwnFont->DeleteObject();
		SAFE_DELETE(m_pOwnFont);
	}

	m_bOwnsFont = bOwns/* && pFont*/;
	m_pOwnFont = pFont;

	if (this->m_pWnd/* && pFont*/)
	{
		ASSERT_VALID(m_pWnd);
		if (::IsWindow(m_pWnd->m_hWnd))
			SetWndCtrlFont(m_pOwnFont);
	}
}

//-----------------------------------------------------------------------------
void CCustomFont::SetOwnFont(LOGFONT& LogFont)
{
	CFont* pF = new CFont();
	pF->CreateFontIndirect(&LogFont);

	SetOwnFont(pF, TRUE);
}

//-----------------------------------------------------------------------------
void CCustomFont::SetOwnFont(BOOL bBold, BOOL bItalic, BOOL bUnderline, int nPointSize/*=0*/, LPCTSTR lpszFaceName/*=NULL*/)
{
	LOGFONT lf;
	memset(&lf, 0, sizeof(lf));

	CFont* pF = GetWndPreferredFont();
	ASSERT_VALID(pF);

	pF->GetLogFont(&lf);

	lf.lfWeight = bBold ? FW_BOLD : FW_NORMAL;

	lf.lfItalic = bItalic;

	lf.lfUnderline = bUnderline;

	if (nPointSize)
		lf.lfHeight = ::GetDisplayFontHeight(nPointSize);
	if (lpszFaceName && lpszFaceName[0])
		_tcsncpy_s(lf.lfFaceName, lpszFaceName, 32);

	SetOwnFont(lf);
}

//-----------------------------------------------------------------------------
void CCustomFont::PopulateFontDescription(CWndObjDescription* pDesc)
{
	CFont* pFont = GetOwnFont();
	if (pFont)
	{
		LOGFONT logFont;
		if (pFont->GetLogFont(&logFont))
		{
			pDesc->SetFont(logFont.lfFaceName, (float)GetDisplayFontPointSize(logFont.lfHeight), logFont.lfWeight == FW_BOLD, logFont.lfItalic == TRUE, logFont.lfUnderline == TRUE);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
int CParsedCtrl::GetDataSize(UINT nCols)
{
	DataType aDataType = GetDataType();

	if (aDataType.m_wType == DATA_ENUM_TYPE)
	{
		ASSERT(aDataType.m_wTag);

		DataEnum data(aDataType.m_wTag, AfxGetEnumsTable()->GetEnumLongerItemValue(aDataType.m_wTag));
		// gli enumerativi vengono sempre formattati con il font delle form, in quanto non contengono
		// dati inseriti dall'utente
		CString strData = FormatData(&data);
		CDC* pDC = GetCtrlCWnd() ? GetCtrlCWnd()->GetDC() : NULL;
		int n = GetEditSize(pDC, GetPreferredFont(), strData, TRUE).cx;
		if (pDC)
			GetCtrlCWnd()->ReleaseDC(pDC);
		return n;
	}

	// do ancora la possibilità al programmatore
	// di ridefinire la dimensione 
	int nDevelperWidth = GetInputWidth();
	if (nDevelperWidth > 0)
		return nDevelperWidth;

	int nFormatterWidth = 0;
	const Formatter* pFormatter = GetCurrentFormatter();
	if (pFormatter)
	{
		CDC* pDC = GetCtrlCWnd() ? GetCtrlCWnd()->GetDC() : NULL;
		if (pDC)
		{
			nFormatterWidth = const_cast<Formatter*>(pFormatter)->GetInputWidth(pDC, nCols, GetPreferredFont()).cx;
			GetCtrlCWnd()->ReleaseDC(pDC);
		}
	}
	return nFormatterWidth;	// valore precalcolato dal formattatore associato
}

//-----------------------------------------------------------------------------
int CParsedCtrl::GetInputWidth() const
{
	return -1;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::ManageNumericBackKey
(
	const CString& strValue,
	DWORD& dwPos, int& nPos,
	TCHAR ch1000Sep, TCHAR chDecSep /* = 0 */
)
{
	if (strValue.IsEmpty())
		return -1;

	if (LOWORD(dwPos) == HIWORD(dwPos) && nPos > 1 && strValue[nPos - 1] == chDecSep)
	{
		SetCtrlSel(nPos - 1, nPos - 1);
		return -1;
	}

	if (LOWORD(dwPos) == HIWORD(dwPos) && nPos > 1 && strValue[nPos - 1] == ch1000Sep)
	{
		nPos -= 2;
		dwPos = MAKELONG(nPos, nPos + 2);
	}

	int n = LOWORD(dwPos) == HIWORD(dwPos) ? 1 : 0;
	int i = 0;
	for (i = 0; i < nPos - n; i++)
		if (strValue[i] != _T('0') && strValue[i] != ch1000Sep && strValue[i] != _T('-'))
			break;

	if (
		(int)HIWORD(dwPos) != strValue.GetLength() &&
		(
			strValue[(int)HIWORD(dwPos)] == chDecSep ||
			strValue[(int)HIWORD(dwPos)] == _T('0') ||
			(
				strValue[(int)HIWORD(dwPos)] == ch1000Sep &&
				strValue[(int)HIWORD(dwPos) + 1] == _T('0')
				)
			) &&
			(i == nPos - n || nPos - n <= 0)
		)
	{
		SetCtrlSel(min(nPos, strValue[0] == _T('-') ? 1 : 0), HIWORD(dwPos));
		return 0;
	}

	return 1;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::UpdateNumericString
(
	double nVal, CString& strValue,
	DWORD dwPos, int& nPos,
	UINT nChar, TCHAR ch1000Sep
)
{
	/*
		if	(
				floor(fabs(nVal)) == 0.0 && nChar == _T('0')	&&
				!strValue.IsEmpty()							&&
				(
					strValue[0] == _T('-') && (strValue[1] == _T('0') || strValue[1] == ch1000Sep) ||
					(strValue[0] == _T('0') || strValue[0] == ch1000Sep)
				)
			)
			return -1;
	*/
	if (nChar == VK_BACK)
	{
		if (LOWORD(dwPos) == HIWORD(dwPos))
		{
			strValue = strValue.Left(nPos - 1) + strValue.Mid(nPos);
			nPos -= 2;
		}
		else
		{
			strValue = strValue.Left(nPos) + strValue.Mid((int)HIWORD(dwPos));
			nPos -= 1;
		}
	}
	else
		if (LOWORD(dwPos) == HIWORD(dwPos))
			strValue = strValue.Left(nPos) + CString((TCHAR)nChar) + strValue.Mid(nPos);
		else
			strValue = strValue.Left(nPos) + CString((TCHAR)nChar) + (HIWORD(dwPos) <= strValue.GetLength() ? strValue.Mid((int)HIWORD(dwPos)) : _T(""));

	int nCurNr1000Sep = 0;
	int nLB = strValue.GetLength();

	for (int i = 0; i < nLB; i++)
		if (strValue[i] == ch1000Sep) nCurNr1000Sep++;

	return nCurNr1000Sep;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::UpdateNumericInput
(
	double nVal, CString& strValue,
	DWORD dwPos, int nPos,
	TCHAR ch1000Sep, int nCurNr1000Sep,
	CDblFormatter* pFormatter
)
{
	SetValue(strValue);

	if (!strValue.IsEmpty() && strValue.Left(1) == "-" && nVal <= 0.0)
	{
		nPos++;
		SetModifyFlag(TRUE);
		SetCtrlSel(nPos, nPos);
		return;
	}

	// an. #6330 con i valori < 1 le operazioni di formattazione e riposizionamento
	// del cursore non venivano chiamate, causando la perdita del valore digitato.
	//	if (floor(fabs(nVal)) != 0.0 || strValue.IsEmpty() || strValue == "0") {
	char chDecSep = pFormatter && !pFormatter->GetDecSeparator().IsEmpty()
		? pFormatter->GetDecSeparator()[0]
		: pFormatter ? 0 : '.';

	if (strValue == chDecSep)
		nPos++;

	// Il cursore deve risultare sempre vicino alla cifra appena digitata, 
	// A causa della riformattazione (vd. separatori delle migliaia) deve
	// essere ricalcolata correttamente la posizione del cursore.
	if (!strValue.IsEmpty())
		for (int i = 0; i < (int)LOWORD(dwPos); i++)
		{
			if (strValue[i] != _T('0') && strValue[i] != ch1000Sep)
				break;

			if (strValue[i] == ch1000Sep)
				nCurNr1000Sep--;

			if (floor(fabs(nVal)) != 0.0)
				nPos--;
		}

	BOOL bForceShowZero = !strValue.IsEmpty() && (m_dwCtrlStyle & NUM_STYLE_SHOW_ZERO) != NUM_STYLE_SHOW_ZERO;
	if (bForceShowZero)
		m_dwCtrlStyle |= NUM_STYLE_SHOW_ZERO;

	BOOL bShowLSZero;
	BOOL bShowMSZero;
	if (pFormatter)
	{
		bShowLSZero = pFormatter->IsShowLSZero();
		bShowMSZero = pFormatter->IsShowMSZero();
		pFormatter->ShowLSZero(TRUE);
		pFormatter->ShowMSZero(TRUE);
	}

	switch (GetDataType().m_wType)
	{
	case DATA_INT_TYPE:
		SetValue(DataInt((int)nVal));
		break;
	case DATA_LNG_TYPE:
		SetValue(DataLng((long)nVal));
		break;
	case DATA_DBL_TYPE:
		SetValue(DataDbl(nVal));
		break;
	case DATA_MON_TYPE:
		SetValue(DataMon(nVal));
		break;
	case DATA_QTA_TYPE:
		SetValue(DataQta(nVal));
		break;
	case DATA_PERC_TYPE:
		SetValue(DataPerc(nVal));
		break;
	default:
		ASSERT(FALSE);
		return;
	}

	if (bForceShowZero)
		m_dwCtrlStyle &= ~NUM_STYLE_SHOW_ZERO;

	if (pFormatter)
	{
		pFormatter->ShowLSZero(bShowLSZero);
		pFormatter->ShowMSZero(bShowMSZero);
	}

	GetValue(strValue);

	if (
		pFormatter && pFormatter->GetFormat() == CDblFormatter::ZERO_AS_DASH
		&& strValue == pFormatter->GetAsZeroValue()
		)
	{
		strValue = _T("0") + CString(chDecSep);
		SetValue(strValue);
		nPos++;
	}

	int nLA = strValue.GetLength();
	int nPA = 0;
	for (int i = 0; i < nLA; i++)
		if (strValue[i] == ch1000Sep) nPA++;

	if (nPA > nCurNr1000Sep)
		nPos += 2;
	else
		if (nPA == nCurNr1000Sep)
			nPos++;
	// } else nPos++;

	if (!strValue.IsEmpty() && nPos > 0 && nPos <= strValue.GetLength() && strValue[nPos - 1] == ch1000Sep)
		nPos--;

	SetModifyFlag(TRUE);
	SetCtrlSel(nPos, nPos);
}

//-----------------------------------------------------------------------------
static int get_sep_pos(int& nSepPos, const CString& strBuffer, const CString& strPattern)
{
	int nSubSepPos = strBuffer.Mid(nSepPos).Find(strPattern);
	if (nSubSepPos < 0)
		return -1;

	int nRes = nSepPos + nSubSepPos;
	nSepPos = nRes + strPattern.GetLength();

	return nRes;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::FindDateSepPos(int nSepIdx, const CString& strBuffer, CDateFormatter* pFormatter)
{
	ASSERT(pFormatter);

	int nSepPos = 0;
	int nRes;

	if ((pFormatter->GetTimeFormat() & CDateFormatHelper::TIME_ONLY) != CDateFormatHelper::TIME_ONLY)
	{
		nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetFirstSeparator());
		if (nRes < 0 || nSepIdx == FIRST_DATE_SEP)
			return nRes;

		nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetSecondSeparator());
		if (nRes < 0 || nSepIdx == SECOND_DATE_SEP)
			return nRes;

		if (!pFormatter->IsFullDateTimeFormat())
			return -1;

		nRes = get_sep_pos(nSepPos, strBuffer, _T(" "));
		if (nRes < 0 || nSepIdx == DATE_TIME_SEP)
			return nRes;
	}

	ASSERT(pFormatter->IsFullDateTimeFormat());

	nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimeSeparator());
	if (nRes < 0 || nSepIdx == FIRST_TIME_SEP)
		return nRes;

	if ((pFormatter->GetTimeFormat() & CDateFormatHelper::TIME_NOSEC) != CDateFormatHelper::TIME_NOSEC)
	{
		nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimeSeparator());
		if (nRes < 0 || nSepIdx == SECOND_TIME_SEP)
			return nRes;
	}

	if (pFormatter->IsTimeAMPMFormat())
	{
		nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimeAMString());
		if (nRes < 0)
			nRes = get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimePMString());

		if (nRes < 0 || nSepIdx == AMPM_TIME_SEP)
			return nRes;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::DateSepPermitted(const CString& strBuffer, CDateFormatter* pFormatter, CString& strSep)
{
	ASSERT(pFormatter);

	int nSepPos = 0;

	if ((pFormatter->GetTimeFormat() & CDateFormatHelper::TIME_ONLY) != CDateFormatHelper::TIME_ONLY)
	{
		strSep = pFormatter->GetFirstSeparator();
		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return FIRST_DATE_SEP;

		strSep = pFormatter->GetSecondSeparator();
		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return SECOND_DATE_SEP;

		if (!pFormatter->IsFullDateTimeFormat())
		{
			strSep.Empty();
			return NO_MORE_SEP;
		}

		strSep = " ";
		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return DATE_TIME_SEP;
	}

	ASSERT(pFormatter->IsFullDateTimeFormat());

	strSep = pFormatter->GetTimeSeparator();
	if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
		return FIRST_TIME_SEP;

	if ((pFormatter->GetTimeFormat() & CDateFormatHelper::TIME_NOSEC) != CDateFormatHelper::TIME_NOSEC)
	{
		strSep = pFormatter->GetTimeSeparator();
		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return SECOND_TIME_SEP;
	}

	if (pFormatter->IsTimeAMPMFormat())
	{
		if (
			get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimeAMString()) < 0 &&
			get_sep_pos(nSepPos, strBuffer, pFormatter->GetTimePMString()) < 0
			)
		{
			strSep = " ";
			return AMPM_TIME_SEP;
		}
	}

	strSep.Empty();
	return NO_MORE_SEP;
}

//@@ElapsedTime
//------------------------------------------------------------------------------
int CParsedCtrl::TimeSepPermitted(const CString& strBuffer, CElapsedTimeFormatter* pFormatter, CString& strSep)
{
	ASSERT(pFormatter);

	int nSepPos = 0;
	int nFormat = pFormatter->GetFormat() & (CElapsedTimeFormatHelper::TIME_DHMS | CElapsedTimeFormatHelper::TIME_DEC);

	//se sono nel caso dell'ora centesimale
	if (nFormat == CElapsedTimeFormatHelper::TIME_CH)
	{
		strSep = pFormatter->GetDecSeparator();
		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return FIRST_TIME_SEP;
		goto no_more_sep;
	}

	switch (nFormat & ~(CElapsedTimeFormatHelper::TIME_C | CElapsedTimeFormatHelper::TIME_F))
	{
	case CElapsedTimeFormatHelper::TIME_DHMS:
	case CElapsedTimeFormatHelper::TIME_DHM:
	case CElapsedTimeFormatHelper::TIME_DH:
	case CElapsedTimeFormatHelper::TIME_HMS:
	case CElapsedTimeFormatHelper::TIME_HM:
	case CElapsedTimeFormatHelper::TIME_MSEC:
	case CElapsedTimeFormatHelper::TIME_S:
	{
		switch (nFormat)
		{
		case CElapsedTimeFormatHelper::TIME_DCD:
		case CElapsedTimeFormatHelper::TIME_HCH:
		case CElapsedTimeFormatHelper::TIME_MCM:
		case CElapsedTimeFormatHelper::TIME_SF:
			strSep = pFormatter->GetDecSeparator();
			if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
				return FIRST_TIME_SEP;

			goto no_more_sep;

		case CElapsedTimeFormatHelper::TIME_S:
			goto no_more_sep;

		default:
			strSep = pFormatter->GetTimeSeparator();
			break;
		}

		if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
			return FIRST_TIME_SEP;

		switch (nFormat & ~(CElapsedTimeFormatHelper::TIME_C | CElapsedTimeFormatHelper::TIME_F))
		{
		case CElapsedTimeFormatHelper::TIME_DHMS:
		case CElapsedTimeFormatHelper::TIME_DHM:
		case CElapsedTimeFormatHelper::TIME_HMS:
		case CElapsedTimeFormatHelper::TIME_MSEC:
		{
			switch (nFormat)
			{
			case CElapsedTimeFormatHelper::TIME_DHCH:
			case CElapsedTimeFormatHelper::TIME_HMCM:
			case CElapsedTimeFormatHelper::TIME_MSF:
				strSep = pFormatter->GetDecSeparator();
				if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
					return SECOND_TIME_SEP;

				goto no_more_sep;

			case CElapsedTimeFormatHelper::TIME_MSEC:
				goto no_more_sep;

			default:
				strSep = pFormatter->GetTimeSeparator();
				break;
			}

			if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
				return SECOND_TIME_SEP;

			switch (nFormat & ~(CElapsedTimeFormatHelper::TIME_C | CElapsedTimeFormatHelper::TIME_F))
			{
			case CElapsedTimeFormatHelper::TIME_DHMS:
			case CElapsedTimeFormatHelper::TIME_HMS:
			{
				switch (nFormat)
				{
				case CElapsedTimeFormatHelper::TIME_DHMCM:
				case CElapsedTimeFormatHelper::TIME_HMSF:
					strSep = pFormatter->GetDecSeparator();
					if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
						return THIRD_TIME_SEP;

					goto no_more_sep;

				case CElapsedTimeFormatHelper::TIME_HMS:
					goto no_more_sep;

				default:
					strSep = pFormatter->GetTimeSeparator();
					break;
				}

				if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
					return THIRD_TIME_SEP;

				if (nFormat == CElapsedTimeFormatHelper::TIME_DHMSF)
				{
					strSep = pFormatter->GetDecSeparator();
					if (get_sep_pos(nSepPos, strBuffer, strSep) < 0)
						return FOURTH_TIME_SEP;
				}
			}
			}
		}
		}
	}
	}

no_more_sep:
	strSep.Empty();
	return NO_MORE_SEP;
}

//------------------------------------------------------------------------------
CString CParsedCtrl::GetDateTimeTemplate() const
{
	DataDate data(24, 7, 1997, 23, 59, 59);
	return FormatData(&data);
}

//------------------------------------------------------------------------------
CString CParsedCtrl::GetElapsedTimeTemplate() const	//@@ElapsedTime
{
	DataLng data(20, 23, 59, 59);
	return FormatData(&data);
}

//-----------------------------------------------------------------------------
int CParsedCtrl::GetInputCharLen() const
{
	int nVal = -1;
	const Formatter* pFormatter = GetCurrentFormatter();
	if (pFormatter)
		nVal = pFormatter->GetInputCharLen();

	ASSERT(nVal > 0);
	return nVal;
}

//-----------------------------------------------------------------------------
int CParsedCtrl::GetOutputCharLen() const
{
	int nVal = -1;
	const Formatter* pFormatter = GetCurrentFormatter();
	if (pFormatter)
		nVal = pFormatter->GetOutputCharLen();

	ASSERT(nVal > 0);
	return nVal;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::NotifyToParent(UINT mess, BOOL bSend /*= TRUE*/)
{
	HWND hWnd = m_pOwnerWnd ? m_pOwnerWnd->m_hWnd : NULL;
	if (bSend)
	{
		m_nValueChanging++;
		GetCtrlParent()->SEND_WM_COMMAND(GetCtrlID(), mess, hWnd);
		m_nValueChanging--;
	}
	else
	{
		GetCtrlParent()->POST_WM_COMMAND(GetCtrlID(), mess, hWnd);
	}
}

// default bad input handler, beep (unless parent notification returns -1).
// Most parent dialogs will return 0 or 1 for command handlers
// (i.e. Beep is the default)
//
//-----------------------------------------------------------------------------
void CParsedCtrl::BadInput()
{
	MessageBeep(MB_ICONHAND);
}

// default error message formatter
//-----------------------------------------------------------------------------
CString CParsedCtrl::FormatErrorMessage(CParsedCtrl::MessageID nIDP, LPCTSTR pszBadVal)
{
	if (nIDP == DUMMY)
		return _T("");
	return cwsprintf(FormatMessage(nIDP), pszBadVal);
}
//-----------------------------------------------------------------------------
CString CParsedCtrl::GetCtrlClass()
{
	CRegisteredParsedCtrl* pRegistered = AfxGetParsedControlsRegistry()->GetRegisteredControl(m_pOwnerWnd);
	return pRegistered ? pRegistered->GetName() : _T("");
}
//-----------------------------------------------------------------------------
int CParsedCtrl::GetCtrlID() const
{
	return (m_pOwnerWnd->m_hWnd)
		? m_pOwnerWnd->GetDlgCtrlID()
		: CJsonFormEngineObj::GetID(m_pOwnerWndDescription);

	//ASSERT(m_pOwnerWnd); return m_pOwnerWnd->GetDlgCtrlID(); 
}


//-----------------------------------------------------------------------------
LRESULT	CParsedCtrl::SendMessageToAncestor(BOOL bSend, UINT message, WPARAM wParam, LPARAM lParam) const
{
	CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetCtrlParent());
	if (pForm)
		return bSend ?
		pForm->GetFormAncestor()->SendMessage(message, wParam, lParam)
		:
		pForm->GetFormAncestor()->PostMessage(message, wParam, lParam);
	else
	{
		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(GetCtrlParent());
		if (pGrid)
			return bSend ?
			pGrid->m_pControlWnd->SendMessage(message, wParam, lParam)
			:
			pGrid->m_pControlWnd->PostMessage(message, wParam, lParam);
	}

	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::ErrorMessage()
{
	if (m_nErrorID == EMPTY_MESSAGE && m_nWarningID == EMPTY_MESSAGE)
		return FALSE;

	MessageID nErrorID = m_nErrorID;
	MessageID nWarningID = m_nWarningID;

	// Segnalazione da parte dell'hotlink di dato non trovato
	//
	if (m_nErrorID == HOTLINK_DATA_NOT_FOUND)
	{
		// lancia l'azione legata al corrente HotKeyLink
		if (CanDoCallLink())
		{
			// Protezione dalla perdita di fuoco dovuta a MessageBox:
			// non deve essere fatto alcun controllo
			//
			m_nErrorID = DUMMY;
			m_nWarningID = DUMMY;

			m_pHotKeyLink->SetAddOnFlyRunning(TRUE);
			m_pHotKeyLink->DoCallLink(TRUE);
			m_pHotKeyLink->SetAddOnFlyRunning(FALSE);

			// Poiche` nel giro di CallLink potrebbe essere richiamata
			// la ModifiedCtrlData con possibili settaggi di errore da parte
			// del programmatore e` necessario ripristinare il valore originale
			// solo se nessuno lo ha toccato
			//
			if (
				AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
				AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::PLAYING
				)
			{
				m_nErrorID = EMPTY_MESSAGE;
				m_nWarningID = EMPTY_MESSAGE;
			}
			else
			{
				if (m_nErrorID == DUMMY)
					m_nErrorID = nErrorID;

				if (m_nWarningID == DUMMY)
					m_nWarningID = nWarningID;
			}

		}

		return TRUE;
	}

	//
	// Altri tipi di errore
	//
	if (m_nErrorID != USER_STRING_ERROR_MESSAGE && m_nWarningID != USER_STRING_ERROR_MESSAGE)
		GetValue(m_strMessage);

	if (m_nWarningID == HOTLINK_IS_RUNNING)
	{
		ASSERT(FALSE);
		TRACE(FormatMessage(HOTLINK_IS_RUNNING));

		return TRUE;
	}

	if (!IsEmptyMessage(m_nErrorID))
	{
		if (m_nErrorID != USER_STRING_ERROR_MESSAGE)
			m_strMessage = FormatErrorMessage(m_nErrorID, m_strMessage);
		else if (!m_strMessage.IsEmpty())
		{
			CString strMessageArg;
			GetValue(strMessageArg);
			m_strMessage = cwsprintf(m_strMessage, strMessageArg);
		}

		// Protezione dalla perdita di fuoco dovuta a MessageBox:
		// non deve essere fatto alcun controllo
		//
		m_nErrorID = DUMMY;
		m_nWarningID = DUMMY;

		if (
			!m_strMessage.IsEmpty() &&
			(!m_pDocument || !m_pDocument->IsInUnattendedMode()) &&
			!m_bShowErrorBox
			)
		{
			m_bShowErrorBox = TRUE;
			AfxMessageBox(m_strMessage, MB_OK | MB_ICONSTOP);
			m_bShowErrorBox = FALSE;
		}

		m_nErrorID = nErrorID;
		m_nWarningID = nWarningID;

		SetModifyFlag(TRUE);
		SetCtrlFocus(TRUE);

		return TRUE;
	}

	//
	// Gestione dei warning
	//

	if (m_nWarningID != USER_STRING_ERROR_MESSAGE)
		m_strMessage = FormatErrorMessage(m_nWarningID, m_strMessage);
	else if (!m_strMessage.IsEmpty())
	{

		CString strMessageArg;
		GetValue(strMessageArg);
		m_strMessage = cwsprintf(m_strMessage, strMessageArg);
	}

	// Protezione dalla perdita di fuoco dovuta a MessageBox:
	// non deve essere fatto alcun controllo
	//
	m_nErrorID = DUMMY;
	m_nWarningID = DUMMY;

	BOOL bOk = FALSE;
	if (
		!m_strMessage.IsEmpty() &&
		(!m_pDocument || !m_pDocument->IsInUnattendedMode())
		)
	{
		//svuoto la coda dei messaggi per lasciar chiudere un eventuale radar che sta morendo,
		//altrimenti ma messagebox lo prende come owner e muore subito anche lei
		CTBWinThread::PumpThreadMessages();

		bOk = (AfxMessageBox(m_strMessage, m_nMsgBoxStyle) == IDOK);

		if (!bOk && ((m_nMsgBoxStyle & MB_OKCANCEL) == MB_OKCANCEL))
			NotifyToParent(EN_REJECTED_UPDATE_BY_WARNING);
	}

	m_nErrorID = nErrorID;
	m_nWarningID = nWarningID;

	if (bOk)
	{
		// !!TRUCCO!! (non sono riuscito a trovare altro modo - G.T.) : 
		//		se si e` dato il fuoco ad un altro EDIT usando il mouse e provocando il messaggio
		//		di warning appena emesso, al quale si e` risposto CANCEL (cioe` si accetta la
		// 		anomalia), il fuoco ritorna naturalmente al control su cui si e` fatto MouseDOWN:
		//		peccato pero` che la MessageBox ha interrotto la sequenza DOWN -> UP del mouse
		//		lasciando l'edit in uno stato di attesa per il MouseUP, che non e` un SetCapture,
		//		e si aspetta che venga effettuata la selezione della stringa in esso contenuta
		//		muovendo il mouse, inibendo qualsiasi input da tastiera. Di conseguenza gli si
		//		manda un WM_LBUTTONUP per "schiodarlo".
		//
		CWnd* pWnd = CWnd::GetFocus();
		if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CEdit))) pWnd->SendMessage(WM_LBUTTONUP);


		SetModifyFlag(FALSE);

		// Si dice al parent di resettare lo stato di WARNING in cui e`
		// rimasto fino a questo momento (vedi ParsedForm::DoLosingFocus e
		// ParsedForm::DoBadValue)
		SendMessageToAncestor(TRUE, UM_LOSING_FOCUS, GetCtrlID(), 0);

		NotifyToParent(EN_DATA_UPDATED);
		return FALSE;
	}

	// Si eleva il warning a livello di errore (nella combo viene usato come flag
	// in caso di CBN_SELENDCANCEL)
	m_nErrorID = m_nWarningID;

	SetModifyFlag(TRUE);
	SetCtrlFocus(TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoSpinScroll(UINT)
{
	SetCtrlFocus(TRUE);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoEnable(BOOL bEnable)
{
	CStateCtrlObj* pStateCtrl;
	for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
		ASSERT(pStateCtrl);
		pStateCtrl->DoEnable(bEnable);
	}

	if (m_pButton)
	{
		// l'abilitazione del bottone dipende anche dallo stato dell'eventuale
		// HotLink associato
		BOOL bHKLEnabled =
			!m_pHotKeyLink ||
			(m_pHotKeyLink->IsHotLinkEnabled() && m_pHotKeyLink->CanDoSearchOnLink());

		BOOL bE = bEnable && bHKLEnabled && !IsEditReadOnly();

		if (
			m_nButtonIDBmp == BTN_MENU_ID ||
			m_nButtonIDBmp == BTN_CALENDAR_ID ||
			m_nButtonIDBmp == BTN_OUTLOOK_ID ||
			m_nButtonIDBmp == BTN_COLOR_ID
			)
			bE = bEnable;

		m_pButton->EnableWindow(bE);
		m_pButton->Invalidate(FALSE);
	}

	if (m_pHyperLink)
		m_pHyperLink->DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoSetFocus(CWnd* aWnd)
{
	if (GetHotLink())
		GetHotLink()->SetActiveOwnerCtrl(this);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoKillFocus(CWnd* pWnd)
{
	if (
		pWnd == NULL ||
		!IsWindow(pWnd->m_hWnd) ||
		pWnd->m_hWnd == m_pOwnerWnd->m_hWnd ||
		pWnd->GetDlgCtrlID() == IDCANCEL ||
		pWnd->GetDlgCtrlID() == ID_HELP ||
		pWnd->GetDlgCtrlID() == IDC_BE_DELETE ||
		(
			m_nErrorID == DUMMY &&
			m_nWarningID == DUMMY
			)
		)
		// do nothing on Cancel (standard behavior)
		return;

	int nRelationship = GetRelationship(GetCtrlCWnd(), pWnd);

	BOOL bEmitError = nRelationship != FOREIGN_FOCUSED && pWnd->GetDlgCtrlID() != ID_HELP;
	if (!UpdateCtrlData(bEmitError, !bEmitError))
		return;

	LRESULT res = SendMessageToAncestor(TRUE, UM_LOSING_FOCUS, GetCtrlID(), nRelationship);

	// In risposta al messaggio di losing focus si puo` ritornare
	// MAKELRESULT(ID, CTRL_FATAL_ERROR) oppure MAKELRESULT(ID, CTRL_WARNING_ERROR)
	// dove ID puo` essere anche diverso dall'ID del corrente  control (sara` il parent
	// a gestire il messaggio di Bad Value cosi congegnato)
	// Per un esempio vedere CBodyEdit
	//
	if (res)
	{
		if (bEmitError)
			SendMessageToAncestor(FALSE, UM_BAD_VALUE, LOWORD(res), HIWORD(res) | CTRL_IMMEDIATE_NOTIFY | CTRL_FOCUS_LOSE_REJECTED);
		else
			SendMessageToAncestor(TRUE, UM_BAD_VALUE, LOWORD(res), HIWORD(res) | CTRL_FOCUS_LOSE_REJECTED);

		return;
	}
	//quando ha perso il fuoco il controllo era ovviamente visibile; in questo metodo succedono cose, e queste cose potrebberlo
	//nasconderlo (ad es. il bodyedit 'spegne' il controllo)
	//quindi testo se + ancora visibile, se non lo è è dannoso chiamare la UpdateCtrlStatus
	//(potrebbe renderlo nuovamente visibile) pertanto esco
	bool bVisible = (m_pOwnerWnd->GetStyle() & WS_VISIBLE) == WS_VISIBLE;
	if (!bVisible)
		return;
	if (nRelationship != FOREIGN_FOCUSED && !pWnd->IsWindowEnabled())
	{
		// nel caso in cui le azioni fatte a fronte della UpdateCtrlData()
		// abbiano disabilitato il control a cui sarebbe dovuto andare il fuoco,
		// per evitare che il fuoco rimanga indefinito (beep se si fa tab, non si
		// vede il caret, ecc...) viene ridato il fuoco al corrente control
		// dal quale ora si potra` uscire normalmente poiche` il dialog manager
		// rivalutera` a chi dare il fuoco
		//
		GetCtrlParent()->POST_WM_COMMAND(GetCtrlID(), PCN_SET_FOCUS, m_pOwnerWnd->m_hWnd);
		return;
	}
	
	// se nel frattempo qualcuno ha messo ReadOnly il DataObj si cambia lo
	// stato esplicitamente poiche` il control si autoprotegge da variazioni
	// di stato e valore mentre sta eseguendo la UpdateCtrlData()
	//
	if (m_pData && m_pData->IsReadOnly())
		UpdateCtrlStatus();
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::DoOnChar(UINT nChar)
{
	return IsEditReadOnly();
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::DoKeyUp(UINT nChar)
{
	if (
		m_nButtonIDBmp == BTN_SPIN_ID &&
		(nChar == VK_UP || nChar == VK_DOWN || nChar == VK_PRIOR || nChar == VK_NEXT)
		)
	{
		NotifyToParent(EN_SPIN_RELEASED);
		return TRUE;	//eaten
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::DoKeyDown(UINT nChar)
{
	switch (nChar)
	{
		//@@ TODO modificare logica degli acceleratori per non collidere con extdoc
	case VK_F2:
		if (!CanDoCallLink())
			return FALSE;

		DoCallLink();
		return TRUE;

	case VK_F8:	DoPushButtonCtrl(0, 0);	return TRUE;
	case VK_F9:	DoPushButtonCtrl(1, 0);	return TRUE;
	case VK_F11:
		// gestione del primo pulsante di stato
		if (m_StateCtrls.GetSize() == 0)
			return FALSE;
		CStateCtrlObj* pCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(0);

		if (pCtrl && pCtrl->GetButton() && pCtrl->GetButton()->IsWindowEnabled())
		{
			DoPushButtonCtrl(0, (LPARAM)pCtrl->GetButton()->m_hWnd);
			return TRUE;
		}
		return FALSE;
	}

	if (m_nButtonIDBmp == BTN_SPIN_ID)
	{
		switch (nChar)
		{
		case VK_UP: DoSpinScroll(SB_LINEUP); 		return TRUE;
		case VK_DOWN: DoSpinScroll(SB_LINEDOWN);	return TRUE;
		case VK_PRIOR: DoSpinScroll(SB_PAGEUP);		return TRUE;
		case VK_NEXT: DoSpinScroll(SB_PAGEDOWN);	return TRUE;
		}
	}

	if (m_StateCtrls.GetSize())
		for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
		{
			CStateCtrlObj* pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
			if (!pStateCtrl->DoChar(nChar))
			{
				BadInput();
				return TRUE;
			}
		}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::CanDoCallLink() const
{
	return
		m_pData && m_pHotKeyLink &&
		m_pHotKeyLink->IsHotLinkEnabled() && m_pHotKeyLink->CanDoCallLink() &&
		(!GetDocument() || GetDocument()->GetFormMode() != CBaseDocument::FIND) &&
		m_pOwnerWnd->IsWindowEnabled();
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoCallLink()
{
	if (!CanDoCallLink())
		return;

	// non e` stata chiamata dal control in risposta del messaggio UM_CALL_LINK
	// ma su F2 o premendo il bottone destro del mouse quindi bisogna valorizzare
	// il dato
	//
	if (!IsValid())
	{
		SendMessageToAncestor(FALSE, UM_BAD_VALUE, GetCtrlID(), CTRL_IMMEDIATE_FATAL_ERROR);
		return;
	}

	SaveOldCtrlData();

	// save new data in connected DataObj
	GetValue(*m_pData);

	// lancia l'azione legata al corrente HotKeyLink
	m_pHotKeyLink->SetAddOnFlyRunning(TRUE);
	m_pHotKeyLink->DoCallLink(FALSE);
	m_pHotKeyLink->SetAddOnFlyRunning(FALSE);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::ExcecPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	// This is a duplicate of method DoPushButtonCtrl, used when the click comes from WebLook.
	if (lParam == 0)
	{
		if (
			!m_pHotKeyLink ||
			!m_pHotKeyLink->IsHotLinkEnabled() ||
			!m_pHotKeyLink->CanDoSearchOnLink() ||
			IsEditReadOnly()
			)
		{
			SetCtrlFocus(FALSE);
			return;
		}

		// BOOL bSaveOldData = !IsEmptyMessage(m_nErrorID) && !IsEmptyMessage(m_nWarningID);
		//if (!IsEmptyMessage(m_nErrorID) && !IsEmptyMessage(m_nWarningID) && wParam !=2)
		if (!m_nErrorID && !m_nWarningID && wParam != 2)
			SaveOldCtrlData();

		// save new data in connected DataObj
		GetValue(*m_pData);

		SetModifyFlag(FALSE);

		if (wParam == 0)
			m_pHotKeyLink->SearchOnLinkUpper();
		else if (wParam == 1)
			m_pHotKeyLink->SearchOnLinkLower();
	}
	else
	{
		if (lParam == BTN_MENU_ID)
		{
			DoCmdMenuButton(wParam);
		}
		else
		{
			// bottoni di stato
			HWND pHwnd = (HWND)lParam;

			CStateCtrlObj* pStateCtrl;
			for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
			{
				pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
				ASSERT(pStateCtrl);
				// lo giro solo se gli appartiene
				if (pStateCtrl->GetButton() && pStateCtrl->GetButton()->m_hWnd == pHwnd)
				{
					m_nLastStateCtrlChanged = i;

					if (pStateCtrl->m_bIsCommandBtn)
					{
						CBaseDocument* pDoc = GetDocument();
						if (pDoc)
						{
							pDoc->GetFrame()->SendMessage(WM_COMMAND, m_pOwnerWnd->GetDlgCtrlID());

							pDoc->SetModifiedFlag(TRUE);
						}
					}
					else
						pStateCtrl->DoPushButtonCtrl(wParam, lParam);
					break;
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoPushButtonCtrl(WPARAM wParam, LPARAM lParam)
{
	if (lParam == 0)
	{
		if (
			!m_pHotKeyLink ||
			!m_pHotKeyLink->IsHotLinkEnabled() ||
			!m_pHotKeyLink->CanDoSearchOnLink() ||
			IsEditReadOnly()
			)
		{
			SetCtrlFocus(FALSE);
			return;
		}

		// BOOL bSaveOldData = !IsEmptyMessage(m_nErrorID) && !IsEmptyMessage(m_nWarningID);
		//if (!IsEmptyMessage(m_nErrorID) && !IsEmptyMessage(m_nWarningID) && wParam !=2)
		if (!m_nErrorID && !m_nWarningID && wParam != 2)
			SaveOldCtrlData();

		CString strText;
		GetValue(strText);
		if (
			(GetCtrlStyle() & CTRL_STYLE_SHOW_FIRST) == CTRL_STYLE_SHOW_FIRST && AfxGetCultureInfo()->IsEqual(strText, CParsedCtrl::Strings::FIRST()) ||
			(GetCtrlStyle() & CTRL_STYLE_SHOW_LAST) == CTRL_STYLE_SHOW_LAST && AfxGetCultureInfo()->IsEqual(strText, CParsedCtrl::Strings::LAST())
			)
		{
			m_pData->Clear();
			SetValue(*m_pData);
		}
		else
			GetValue(*m_pData);

		SetModifyFlag(FALSE);

		if (wParam == 0)
			m_pHotKeyLink->SearchOnLinkUpper();
		else if (wParam == 1)
			m_pHotKeyLink->SearchOnLinkLower();
	}
	else
	{
		if (lParam == BTN_MENU_ID)
		{
			DoCmdMenuButton(wParam);
		}
		else
		{
			// bottoni di stato
			HWND pHwnd = (HWND)lParam;

			CStateCtrlObj* pStateCtrl;
			for (int i = 0; i <= m_StateCtrls.GetUpperBound(); i++)
			{
				pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);
				ASSERT(pStateCtrl);
				// lo giro solo se gli appartiene
				if (pStateCtrl->GetButton() && pStateCtrl->GetButton()->m_hWnd == pHwnd)
				{
					m_nLastStateCtrlChanged = i;

					if (pStateCtrl->m_bIsCommandBtn)
					{
						CBaseDocument* pDoc = GetDocument();
						if (pDoc)
						{
							pDoc->GetFrame()->SendMessage(WM_COMMAND, m_pOwnerWnd->GetDlgCtrlID());

							pDoc->SetModifiedFlag(TRUE);
						}
					}
					else
						pStateCtrl->DoPushButtonCtrl(wParam, lParam);
					break;
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::DoRButtonDown(UINT, CPoint ptPoint)
{
	// Se e` disabilitata quindi non deve fare nulla
	return !m_pOwnerWnd->IsWindowEnabled();
}

//-----------------------------------------------------------------------------
UINT CParsedCtrl::CreateFormatPopupMenu(CMenu& menuFormat)
{
	//per questi tre tipi ho dei problemi legati alla formattazione.
	DataType eType = GetDataType();
	if (
		eType == DATA_STR_TYPE ||
		eType == DATA_BOOL_TYPE ||
		eType == DATA_ENUM_TYPE ||
		m_nFormatIdx < 0
		) return MF_GRAYED;

	Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(m_nFormatIdx, m_pFormatContext);

	m_nNewFormatIdx = m_nFormatIdx;

	if (!pFormatter) return MF_GRAYED;

	DataType aType = pFormatter->GetDataType();
	int nIndexID = 0;
	m_FmtCmdArray.RemoveAll();

	menuFormat.CreatePopupMenu();

	Formatter* pFmt;
	for (int i = 0; i <= AfxGetFormatStyleTable()->GetUpperBound(); i++)
	{
		if (AfxGetFormatStyleTable()->GetDataType(i) == aType)
		{
			// controllo che sia applicabile per contesto
			pFmt = AfxGetFormatStyleTable()->GetFormatter(i, m_pDocument ? &m_pDocument->GetNamespace() : NULL);
			if (!pFmt)
				continue;

			menuFormat.AppendMenu(MF_STRING, IDFormatTable[nIndexID].uiID, pFmt->GetTitle());
			//visualizzo il simbolo di check vicino al formattatore attualmente assegnato al control
			menuFormat.CheckMenuItem(IDFormatTable[nIndexID].uiID, (m_nFormatIdx == i) ? MF_CHECKED : MF_UNCHECKED);
			nIndexID++;
			m_FmtCmdArray.Add(AfxGetFormatStyleTable()->GetStyleName(i));
		}
	}

	return MF_POPUP;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::DoContextMenu(CWnd* pWnd, CPoint ptPoint)
{
	if (pWnd->m_hWnd != m_pOwnerWnd->m_hWnd)
		return FALSE;

	// Per gestire il caso in cui sia stato utilizzato l'acceleratore
	// <SHIFT>F10 per chiamare il menu popup e` necessario forzare
	// un finto KEYUP del tasto <SHIFT>: infatti quando appare il menu
	// non viene inviato al control il messaggio di WM_KEYUP del
	// tasto <SHIFT> che permetterebbe di resettare lo stato in cui si e`
	// messo il BodyEdit dopo averlo pigiato.
	CWnd* pParent = GetCtrlParent();
	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pParent);
	if (pGrid)
		pGrid->OnKeyHit(GetCtrlID(), VK_SHIFT, WM_KEYUP);

	CMenu   menu;

	UINT nEnabled = MF_GRAYED;
	menu.CreatePopupMenu();

	/*@@TODO NUVOLETTA
		CMenu   menuHelp;
		CString strHelp;

		UINT nID = ID_CTRL_WATHS_THIS;
		if (strHelp.LoadString(GetCtrlID()))
		{
			menuHelp.CreatePopupMenu();
			menuHelp.AppendMenu(MF_STRING, 0, strHelp);
			nEnabled = MF_POPUP;
			nID = (UINT) menuHelp.GetSafeHmenu();
		}

		menu.AppendMenu(MF_STRING | nEnabled, nID, "Che cos'è?");
	*/

	// caso di CBaseDocument quando siamo in stato di FIND utile per i campi che hanno un default come es
	// DataBool e DataEnum.L'utente ha in questo modo la possibilità di considerare anche
	// questa tipologia di campi nella WHERE CLAUSE (prima non venivano considerati)
	if (m_pData)
	{
		if (GetDocument() && GetDocument()->GetFormMode() == CBaseDocument::FIND)
		{
			if (menu.GetMenuItemCount() > 0)
				menu.AppendMenu(MF_SEPARATOR);

			if (!m_pData->IsEmpty() || m_pData->IsValueChanged())
				menu.AppendMenu(MF_STRING, ID_CTRL_BEHAVIOR, _TB("Remove from search"));
			else
				menu.AppendMenu(MF_STRING, ID_CTRL_BEHAVIOR, _TB("Insert in search"));

			//menu.CheckMenuItem(ID_CTRL_BEHAVIOR, m_pData->IsValueChanged() ? MF_CHECKED : MF_UNCHECKED);
		}
		else
			// sono come nel caso delle askdialog di WOORM
			if (!GetDocument())
			{
				menu.AppendMenu(MF_STRING, ID_CTRL_BEHAVIOR, _TB("Clear"));
				menu.CheckMenuItem(ID_CTRL_BEHAVIOR, (m_pData->IsModified()) ? MF_CHECKED : MF_UNCHECKED);
			}
	}


	//Formattazione. Devo creare il PopUp contenente i formattatori disponibili per il control.
	//Questo non lo devo fare se il control è per i DataStr e i DataInt
	CMenu   menuFormat;
	if (CreateFormatPopupMenu(menuFormat) != MF_GRAYED)
	{
		if (menu.GetMenuItemCount() > 0)	menu.AppendMenu(MF_SEPARATOR);
		menu.AppendMenu(MF_STRING | MF_POPUP, (UINT)menuFormat.GetSafeHmenu(), _TB("Formatting"));
	}

	if (m_pHotKeyLink && m_pHotKeyLink->IsHotLinkEnabled() && !m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CMSStrCombo)) && !m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CListBox)))
	{
		if (menu.GetMenuItemCount() > 0) menu.AppendMenu(MF_SEPARATOR);

		nEnabled = m_pHotKeyLink->CanDoSearchOnLink() ? 0 : MF_GRAYED;

		menu.AppendMenu(MF_STRING | nEnabled, ID_CTRL_SEARCH_ON_LINK_UPPER, _TB("Code Radar\tF8"));
		menu.AppendMenu(MF_STRING | nEnabled, ID_CTRL_SEARCH_ON_LINK_LOWER, _TB("Description Radar\tF9"));

		nEnabled =
			(!m_pHotKeyLink->GetAddOnFlyNamespace().IsEmpty() && m_pHotKeyLink->IsHyperLinkEnabled())
			? 0 : MF_GRAYED;
		menu.AppendMenu(MF_STRING | nEnabled, ID_CTRL_BROWSER_LINK, _TB("Open..."));

		nEnabled = CanDoCallLink() && m_pHotKeyLink->IsEnabledAddOnFly() ? 0 : MF_GRAYED;
		menu.AppendMenu(MF_STRING | nEnabled, ID_CTRL_CALL_LINK, _TB("Add on Fly\tF2"));

		if (GetDataType() == DATA_STR_TYPE && m_pHotKeyLink->IsFillListBoxEnabled())
		{
			UINT nChk = m_pHotKeyLink->IsLikeOnDropDownEnabled() ? MF_CHECKED : MF_UNCHECKED;
			menu.AppendMenu(MF_STRING | nChk, ID_CTRL_FILTER_DROPDOWN, _TB("Filter drop down list on partial code"));
		}

		m_pHotKeyLink->OnExtendContextMenu(menu);
	}

	if (
		GetDataType() == DATA_DATE_TYPE && !GetDataType().IsATime() &&
		(
		(GetControllerMode() == CExternalControllerInfo::EDITING)
			||
			GetDocument() && GetDocument()->m_pAutoExpressionMng
			)

		)
	{
		if (menu.GetMenuItemCount() > 0)	menu.AppendMenu(MF_SEPARATOR);
		menu.AppendMenu(MF_STRING, ID_CTRL_EDIT_AUTOMATIC_EXPR, _TB("Automatic values editor"));
	}

	if (m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CEnumListBox)) || m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CEnumCombo)))
	{
		if (menu.GetMenuItemCount() > 0)	menu.AppendMenu(MF_SEPARATOR);
		BOOL bShow = FALSE;
		if (m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CEnumListBox)))
			bShow = ((CEnumListBox*)m_pOwnerWnd)->m_bShowEnumValue;
		else
			bShow = ((CEnumCombo*)m_pOwnerWnd)->m_bShowEnumValue;

		if (bShow)
			menu.AppendMenu(MF_STRING, ID_CTRL_SHOWENUMVALUE, _TB("Hide values"));
		else
			menu.AppendMenu(MF_STRING, ID_CTRL_SHOWENUMVALUE, _TB("Show values"));

		menu.AppendMenu(MF_STRING, ID_CTRL_ENUMSVIEWER, _TB("Enums Viewer"));
		pWnd->ClientToScreen(&ptPoint);
	}

	CBaseDocument* pDocument = GetDocument();
	if (!pDocument && pParent && pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		pDocument = ((CParsedDialog*)pParent)->GetDocument();

	if (
		!OnShowingPopupMenu(menu) ||
		(pDocument && !pDocument->ShowingPopupMenu(GetCtrlID(), &menu)) ||
		menu.GetMenuItemCount() == 0
		)
		return FALSE;

	if (ptPoint.x < 0 && ptPoint.y < 0)
	{
		// caso in cui sia stato dato <Shift>F10
		CRect rect;
		m_pOwnerWnd->GetWindowRect(rect);
		ptPoint = rect.CenterPoint();
	}

	AfxGetThreadContext()->RaiseCallBreakEvent();

	menu.TrackPopupMenu(TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, m_pOwnerWnd);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::OnShowingPopupMenu(CMenu& menu)
{
	if (m_pNumbererRequest)
	{
		if (menu.GetMenuItemCount() > 0)
			menu.AppendMenu(MF_SEPARATOR);

		menu.AppendMenu
		(
			MF_STRING,
			ID_MENU_DATA_FORMATTER_PADDING_ENABLE,
			m_pNumbererRequest->GetFormatMask()->IsEnabled() ?
			_TB("Disable automatic digit padding for number") :
			_TB("Enable automatic digit padding for number")
		);
	}

	if (m_pOwnerWndDescription && m_pOwnerWndDescription->m_pMenu)
	{
		CWnd* pParent = GetCtrlParent();
		CJsonContextObj* pContext = NULL;
		while (pParent)
		{
			if (pParent->IsKindOf(RUNTIME_CLASS(CLocalizableDialog)))
			{
				pContext = ((CLocalizableDialog*)pParent)->GetJsonContext();
				break;
			}
			pParent = pParent->GetParent();
		}
		if (pContext)
			m_pOwnerWndDescription->m_pMenu->EvaluateExpressions(pContext);
		if (menu.GetMenuItemCount() > 0)
			menu.AppendMenu(MF_SEPARATOR);
		for (int k = 0; k < m_pOwnerWndDescription->m_pMenu->m_Children.GetCount(); k++)
		{
			CWndObjDescription* pDescMenuItem = m_pOwnerWndDescription->m_pMenu->m_Children.GetAt(k);
			if (!pDescMenuItem->IsKindOf(RUNTIME_CLASS(CMenuItemDescription)))
			{
				ASSERT(FALSE);
				continue;
			}
			if (((CMenuItemDescription*)pDescMenuItem)->m_bIsSeparator)
				menu.AppendMenu(MF_SEPARATOR);
			else
			{
				UINT nMenuItemId = CJsonFormEngineObj::GetID(pDescMenuItem);
				if (pContext && !pContext->CanCreateControl(pDescMenuItem, nMenuItemId))
				{
					continue;
				}
				menu.AppendMenu(MF_STRING, nMenuItemId, AfxLoadJsonString(pDescMenuItem->m_strText, pDescMenuItem));
			}
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CParsedCtrl::GetAutomaticExpression() const
{
	if (!m_pData)
	{
		ASSERT(FALSE);
		return _T("");
	}

	if (!m_strAutomaticExpression.IsEmpty())
		return	AddAutoExpressionPrefix(m_strAutomaticExpression, FALSE, m_pData->IsReadOnly());

	return AddAutoExpressionPrefix(m_pData->Str(0, 0), TRUE, m_pData->IsReadOnly());
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::SetAutomaticExpression(const CString& str)
{
	if (!m_pData || (GetControllerMode() == CExternalControllerInfo::NONE &&
		GetDocument() && !GetDocument()->m_pAutoExpressionMng
		)
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!SetAutomaticExpressionAndData(str, m_strAutomaticExpression, m_pData, GetControllerMode() == CExternalControllerInfo::RUNNING))
		return FALSE;

	if (!str.IsEmpty())
	{
		m_pData->SetReadOnly(IsReadOnlyAutoExpression(str));
		UpdateCtrlStatus();
	}

	if (IsAutoExpression(str))
	{
		if (GetControllerMode() == CExternalControllerInfo::EDITING ||
			(GetDocument() && GetDocument()->m_pAutoExpressionMng))
		{
			SetCtrlStyle(GetCtrlStyle() | CTRL_STYLE_STORED_AUTO_EXPRESSION);
			SetModifyFlag(FALSE);
		}
	}
	else
		SetCtrlStyle(GetCtrlStyle() & ~CTRL_STYLE_STORED_AUTO_EXPRESSION);

	UpdateCtrlView();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoEditAutomaticExpression()
{
	if (GetControllerMode() != CExternalControllerInfo::EDITING && (!m_pData || (GetDocument() && !GetDocument()->m_pAutoExpressionMng)))
		return;

	CString strExpr;
	if (!m_strAutomaticExpression.IsEmpty())
		strExpr = RemoveAutoExpressionPrefix(m_strAutomaticExpression);

	if (!::ModifyAutoExpressionString(strExpr, GetDataType()))
		return;

	AssignAutomaticExpression(strExpr);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AssignAutomaticExpression(const CString& strExpr)
{
	m_pData->Clear();

	if (strExpr.IsEmpty())
	{
		m_strAutomaticExpression.Empty();
		SetCtrlStyle(GetCtrlStyle() & ~CTRL_STYLE_STORED_AUTO_EXPRESSION);
	}
	else
	{
		m_strAutomaticExpression = strExpr;
		SetCtrlStyle(GetCtrlStyle() | CTRL_STYLE_STORED_AUTO_EXPRESSION);
		SetModifyFlag(FALSE);
	}


	UpdateCtrlView();

	//Controllo se il documento gestisce le auto expression e se possiede un gestore
	//in caso negativo lo genero
	//aggiungo la variabile e l'auto expression al gestore.
	if (GetDocument() && GetDocument()->m_pAutoExpressionMng)
	{
		if (SetAutomaticExpressionAndData(GetAutomaticExpression(), m_strAutomaticExpression, m_pData, TRUE) &&
			GetDocument()->GetVariableArray())
		{
			if (GetDocument()->GetVariableArray())
			{
				CString strVarName;
				for (int i = 0; i < GetDocument()->GetVariableArray()->GetSize(); i++)
				{
					if (GetDocument()->GetVariableArray()->GetAt(i)->GetDataObj() == m_pData)
					{
						strVarName = GetDocument()->GetVariableArray()->GetAt(i)->GetName();
						break;
					}
				}

				if (!strVarName.IsEmpty() && GetDocument())
				{
					CAutoExpressionData* pAutoExpressionData = NULL;

					for (int n = 0; n < GetDocument()->m_pAutoExpressionMng->GetSize(); n++)
					{
						pAutoExpressionData = GetDocument()->m_pAutoExpressionMng->GetAt(n);
						if (pAutoExpressionData->GetVarName().CompareNoCase(strVarName) == 0)
						{
							pAutoExpressionData->SetExpression(AddAutoExpressionPrefix(m_strAutomaticExpression, FALSE, FALSE));
							pAutoExpressionData->SetDataObj(m_pData);
							GetDocument()->m_pAutoExpressionMng->EvaluateExpression(AddAutoExpressionPrefix(m_strAutomaticExpression, FALSE, FALSE), m_pData);
							break;
						}
						else
							pAutoExpressionData = NULL;
					}

					if (!pAutoExpressionData)
					{
						pAutoExpressionData = new CAutoExpressionData(AddAutoExpressionPrefix(m_strAutomaticExpression, FALSE, FALSE), strVarName, m_pData);
						GetDocument()->m_pAutoExpressionMng->EvaluateExpression(AddAutoExpressionPrefix(m_strAutomaticExpression, FALSE, FALSE), m_pData);
						GetDocument()->m_pAutoExpressionMng->Add(pAutoExpressionData);
					}
				}
			}
		}
	}
}


//-----------------------------------------------------------------------------
void CParsedCtrl::DoBehavior()
{
	if (!m_pData) return;

	// se sono nel documento (sono sicuramente in stato di FIND) devo permettere l'inserimento
	// del dataobj nella WHERE CLAUSE in base al flag che ha scelto l'utente
	if (GetDocument() && GetDocument()->GetFormMode() == CBaseDocument::FIND)
	{
		BOOL bFind = (m_pData->IsValueChanged() || !m_pData->IsEmpty());
		m_pData->Clear();
		m_pData->SetValueChanged(!bFind);	//inverte il flag

		UpdateCtrlView();
	}
	else
		if (!GetDocument())
		{
			m_pData->Clear();
			m_pData->SetModified(!m_pData->IsModified());
			UpdateCtrlView();
		}
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsStrChar(UINT nChar, DWORD dwStyle)
{
	if (dwStyle & STR_STYLE_ALL)
		return TRUE;
	DWORD dwType;

	if (_istcntrl(nChar))
		return TRUE;		// always allow control chars

	if ((dwStyle & STR_STYLE_BLANKS) != STR_STYLE_BLANKS && _istspace(nChar))
		return FALSE;

	if (_istdigit(nChar))
		dwType = STR_STYLE_NUMBERS;
	else if (_istalpha(nChar))
	{
		if (
			((dwStyle & STR_STYLE_NAMESPACE) == STR_STYLE_NAMESPACE) ||
			((dwStyle & STR_STYLE_FILESYSTEM) == STR_STYLE_FILESYSTEM)
			)
			dwType = szBaseAllowedLetters.Find((TCHAR)nChar) >= 0 ? STR_STYLE_LETTERS : STR_STYLE_OTHERCHARS;
		else
			dwType = STR_STYLE_LETTERS;
	}
	else if (szNamespaceExtraChars.Find((TCHAR)nChar) >= 0)
		dwType = STR_STYLE_NAMESPACE;
	else if (szFileSystemExtraChars.Find((TCHAR)nChar) >= 0)
		dwType = STR_STYLE_FILESYSTEM;
	else
		dwType = STR_STYLE_OTHERCHARS;

	return (dwStyle & dwType) != 0;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::IsAssociatedButton(CWnd* pWnd)
{
	if (pWnd == NULL)
		return FALSE;

	return (
		m_pButton && pWnd->m_hWnd == m_pButton->m_hWnd);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetCtrlMaxLen(UINT nLen, BOOL)
{
	m_nCtrlLimit = nLen;
}
//-----------------------------------------------------------------------------
void CParsedCtrl::FindHotLink()
{
	HotKeyLinkObj* pHKL = GetHotLink();
	if (pHKL && pHKL->IsAutoFindable())
	{
		DataObj* pData = pHKL->GetAttachedData();
		SqlRecord* pRec = pHKL->GetMasterRecord();
		if (pHKL->FindNeeded(pData, pRec))
		{
			pHKL->OnPrepareForFind(pRec);
			pHKL->DoFindRecord(pData);
		}
	}
}
//-----------------------------------------------------------------------------
BOOL CParsedCtrl::CheckDataObjType(const DataObj* pDataObj /* = NULL */)
{
	if (pDataObj == NULL)
		pDataObj = m_pData;

	if (pDataObj)
	{
		DataType dataDataType = pDataObj->GetDataType();
		DataType ctrlDataType = GetDataType();

		return
			(
				dataDataType == ctrlDataType ||
				(
					dataDataType.IsFullDate() &&
					!dataDataType.IsATime() &&
					ctrlDataType == DATA_DATE_TYPE
					)
				||
				(
					dataDataType == DATA_TXT_TYPE	&&
					ctrlDataType == DATA_STR_TYPE
					)
				||
				(
					dataDataType.IsAHandle() &&
					ctrlDataType == DATA_LNG_TYPE
					)
				);

	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (m_pControlBehaviour)
		m_pControlBehaviour->OnCmdMsg(nID, nCode, NULL, NULL);

	if (nCode == 0)
	{
		if (nID == ID_CTRL_SEARCH_ON_LINK_UPPER)
			DoPushButtonCtrl(0, 0);
		else if (nID == ID_CTRL_SEARCH_ON_LINK_LOWER)
			DoPushButtonCtrl(1, 0);
		else if (nID == ID_CTRL_CALL_LINK)
			DoCallLink();
		else if (nID == ID_CTRL_EDIT_AUTOMATIC_EXPR)
			DoEditAutomaticExpression();
		else if (nID == ID_CTRL_BEHAVIOR)
			DoBehavior();
		else if (nID == ID_CTRL_BROWSER_LINK)
		{
			if (m_pHyperLink && m_pHotKeyLink && m_pHotKeyLink->IsHyperLinkEnabled())
				m_pHyperLink->DoFollowHyperlink();
		}
		else if (nID == ID_CTRL_FILTER_DROPDOWN)
		{
			if (m_pHotKeyLink)
				m_pHotKeyLink->EnableLikeOnDropDown(!m_pHotKeyLink->IsLikeOnDropDownEnabled());
		}
		else if (nID == ID_CTRL_SHOWENUMVALUE)
			DoShowEnumValue();
		else if (nID == ID_CTRL_ENUMSVIEWER)
			AfxRunEnumsViewer();
		else if (nID == ID_MENU_DATA_FORMATTER_PADDING_ENABLE)
			ToggleFormatMask();
		else
		{
			if (m_pHotKeyLink && nID >= ID_CTRL_HKLEXTMENU_START && nID <= ID_CTRL_HKLEXTMENU_END)
				m_pHotKeyLink->DoContextMenuAction(nID - ID_CTRL_HKLEXTMENU_START);
		}
	}
}

// Menage eventual special keys
//-----------------------------------------------------------------------------
BOOL CParsedCtrl::PreProcessMessage(MSG* pMsg)
{
	// CTRL-TAB deve essere ignorato
	if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_TAB && GetKeyState(VK_CONTROL) & 0x8000)
		return FALSE;

	//in design mode solo il form editor gestisce i messaggi
	if (GetDocument() && GetDocument()->IsInDesignMode() && pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST)
		return FALSE;

	if (pMsg->message != WM_KEYUP && pMsg->message != WM_KEYDOWN)
		return FALSE;

	if (pMsg->message == WM_KEYDOWN)
	{
		switch (pMsg->wParam)
		{
		case VK_SHIFT:	m_wKeyState |= KEY_SHIFT_DOWN;	break;
		case VK_CONTROL:	m_wKeyState |= KEY_CTRL_DOWN; 	break;
		case VK_MENU:	m_wKeyState |= KEY_ALT_DOWN;	break;
		case VK_TAB:	m_wKeyState = 0;				break;	// se preme TAB resetto il flag perchè si sposta di campo
		}
	}
	else if (pMsg->message == WM_KEYUP)
	{
		switch (pMsg->wParam)
		{
		case VK_SHIFT:	m_wKeyState &= ~KEY_SHIFT_DOWN;	break;
		case VK_CONTROL:	m_wKeyState &= ~KEY_CTRL_DOWN;	break;
		case VK_MENU:	m_wKeyState &= ~KEY_ALT_DOWN;	break;
		}
	}

	if (pMsg->message == WM_KEYDOWN)
	{
		if (pMsg->wParam == VK_F2)
		{
			if (DoKeyDown(VK_F2))
				return TRUE;
		}
		else
			if (m_pOwnerWnd && m_pOwnerWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
			{
				switch (pMsg->wParam)
				{
				case VK_TAB:
					DoKeyDown(VK_TAB);
					break;

				case VK_RETURN:	// seleziona il corrente item (vedi parscbx.cpp)
					if (DoKeyDown(VK_RETURN))
						return TRUE;
					break;

				case VK_ESCAPE:	// chiude la tendina senza selezionare (vedi parscbx.cpp)
					if (DoKeyDown(VK_ESCAPE))
						return TRUE;
					break;

				case VK_DELETE:	// se abilitato pulisce il control (vedi parscbx.cpp)
					if (DoKeyDown(VK_DELETE))
						return TRUE;
					break;
				}
			}
	}

	CWnd* pParent = GetCtrlParent();
	if (!pParent)
		return FALSE;

	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pParent);

	if (!pGrid)
		return FALSE;

	return pGrid->OnKeyHit
	(
		GetCtrlID(),
		(UINT)pMsg->wParam,
		pMsg->message
	);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetNewFormatIdx()
{
	if (m_nNewFormatIdx >= 0 && m_nFormatIdx != m_nNewFormatIdx)
		AttachFormatter(m_nNewFormatIdx);
	m_nNewFormatIdx = UNDEF_FORMAT;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::GridCellPosChanging(WINDOWPOS* wndPos)

{
	// Corregge la dimensione del control disegnata con il dialog editor
	// per far spazio agli eventuali bottoni. 
	CWnd* pP = m_pOwnerWnd->GetParent();
	CRuntimeClass* pParentClass = pP->GetRuntimeClass();

	if (
		dynamic_cast<CGridControlObj*>(pP) == NULL &&
		(m_dwCtrlStyle & CTRL_STYLE_DO_POS_CHANGING) == 0 &&
		//NO UNICODE !
		//strcmp(pParentClass->m_lpszClassName, "CAskView") &&
		strcmp(pParentClass->m_lpszClassName, "CXMLExportUserCriteriaPage") &&
		//strcmp(pParentClass->m_lpszClassName, "CDynamicDlg") &&
		strcmp(pParentClass->m_lpszClassName, "CDynamicContainerTileDlg")
		)
		return;

	if (
		(!m_pButton && m_StateCtrls.GetSize() == 0) ||
		(wndPos->flags & SWP_NOACTIVATE) == SWP_NOACTIVATE
		)
		return;

	CRect rectEdit(CPoint(wndPos->x, wndPos->y), CSize(wndPos->cx, wndPos->cy));

	// prima ricalcolo l'area di controls e bottoni
	DoCellPosChanging(rectEdit, wndPos->flags);

	// quindi lo comunico al WindowPosChanging
	wndPos->x = rectEdit.left;
	wndPos->y = rectEdit.top;
	wndPos->cx = rectEdit.right - wndPos->x;
	wndPos->cy = rectEdit.bottom - wndPos->y;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoCellPosChanging(CRect& rectEdit, UINT nFlags)
{
	// calcola le coordinate dove disegnare i bottoni
	CRect wndRect;
	m_pOwnerWnd->GetWindowRect(wndRect);
	GetCtrlParent()->ScreenToClient(wndRect);

	// bottone dell'hotlink e bottoni di stato
	int	nBtnWidth = GetButtonWidth(0);
	int nBtnsStateWidth = 0;

	CRect rectBtn(0, 0, 0, 0);
	rectBtn.top = ((nFlags & SWP_NOMOVE) == SWP_NOMOVE) ? wndRect.top : rectEdit.top;

	// calcolo quanto spazio mi occupano i bottoni
	CStateCtrlObj* pStateCtrl;

	// faccio rientrare il control x il numero di bottoni disegnandoli all'indietro
	nBtnsStateWidth = 0;
	for (int i = m_StateCtrls.GetUpperBound(); i >= 0; i--)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);

		if (!pStateCtrl->GetButton())
			continue;

		CRect aRect;
		pStateCtrl->GetButton()->GetClientRect(&aRect);
		nBtnsStateWidth += (aRect.Width() + BTN_OFFSET);
	}

	// parto dal dimensionamento dell'edit o dell'area disegnata
	if ((nFlags & SWP_NOMOVE) == SWP_NOMOVE)
	{
		rectBtn.left = wndRect.left + rectEdit.Width();
		if (rectEdit.left == 0)
			rectEdit.left = wndRect.left;
	}
	else if ((nFlags & SWP_NOSIZE) == SWP_NOSIZE)
		rectBtn.left = rectEdit.left + wndRect.Width();
	else
		rectBtn.left = rectEdit.right;

	// siccome in tutti e tre i casi ho la posizione right dell'area di editing 
	// mi aggiungo lo spazio bottone per riarrivare al termine della colonna.
	rectBtn.left += (nBtnWidth ? (nBtnWidth + BTN_OFFSET) : 0) + nBtnsStateWidth;

	// riparto dalla larghezza massima della colonna e
	// faccio rientrare il control x il numero di bottoni 
	// disegnandoli all'indietro
	for (int i = m_StateCtrls.GetUpperBound(); i >= 0; i--)
	{
		pStateCtrl = (CStateCtrlObj*)m_StateCtrls.GetAt(i);

		if (pStateCtrl->GetButton())
		{
			CRect aRect;
			pStateCtrl->GetButton()->GetWindowRect(&aRect);
			GetCtrlParent()->ScreenToClient(aRect);
			rectBtn.left -= (aRect.Width() + BTN_OFFSET);
			if ((nFlags & SWP_NOMOVE) == SWP_NOMOVE && (nFlags & SWP_NOSIZE) == SWP_NOSIZE)
				;
			else
				pStateCtrl->GetButton()->SetWindowPos(NULL, rectBtn.left, rectBtn.top, 0, 0, SWP_NOSIZE | (nFlags & SWP_SHOWWINDOW));
		}
	}

	// disegno l'hotlink
	if (m_pButton)
	{
		rectBtn.left -= (nBtnWidth);
		m_pButton->SetWindowPos(NULL, rectBtn.left, rectBtn.top, 0, 0, SWP_NOSIZE | (nFlags & SWP_SHOWWINDOW));
	}

	rectEdit.right = rectBtn.left - BTN_OFFSET;

	// l'hyperlink va visualizzato solo se la finestra esiste e se 
	// il control è disabilitato e visibile
	if (

		m_pButton && m_pHyperLink && GetCtrlCWnd() &&
		//m_pHotKeyLink && m_pHotKeyLink->IsHyperLinkEnabled() &&
		IsTBWindowVisible(GetCtrlCWnd()) && !GetCtrlCWnd()->IsWindowEnabled()
		)
		SetHyperLinkPos(rectEdit, nFlags);

	if (m_pHotLinkController)
		m_pHotLinkController->DoCellPosChanging(rectEdit, nFlags);

}

//-----------------------------------------------------------------------------
CExternalControllerInfo::ControllingMode CParsedCtrl::GetControllerMode() const
{
	CBaseDocument *pDoc = GetDocument();
	if (pDoc && pDoc->m_pExternalControllerInfo)
		return pDoc->m_pExternalControllerInfo->m_ControllingMode;

	CWnd* pParent = GetCtrlParent();
	if (pParent && pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		return ((CParsedDialog*)pParent)->m_ControllingMode;

	return CExternalControllerInfo::NONE;
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrl::ShowZerosInInput()
{
	DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szShowZerosInInput, DataBool(TRUE), szTbDefaultSettingFileName);
	if (!pDataObj || !pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
		return TRUE;

	return *((DataBool*)pDataObj);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::SetNamespace(const CString& strName)
{
	if (strName.IsEmpty())
		return;

	CParsedDialog* pDlgParent = dynamic_cast<CParsedDialog*>(GetCtrlParent());
	if (pDlgParent)
	{
		GetInfoOSL()->m_pParent = pDlgParent->GetInfoOSL();
		GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, strName, pDlgParent->GetNamespace());
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::OnHotLinkClosed()
{
	if (m_pDocument) {
		HotKeyLinkObj* pHKL = GetHotLink();
		if (pHKL)

		m_pDocument->OnHotLinkClosed(GetCtrlID());
	}
}

//-----------------------------------------------------------------------------
BOOL	CParsedCtrl::UseEasyReading()	const
{
	return m_pDocument ? m_pDocument->UseEasyReading() : AfxGetThemeManager()->UseEasyReading();
}

// questo metodo consente di ritornare il formatMask solo quando deve essere
// usato per il padding 
//-----------------------------------------------------------------------------
CFormatMask* CParsedCtrl::GetFormatMask()
{
	CFormatMask* pFormatMask = m_pNumbererRequest ? m_pNumbererRequest->GetFormatMask() : NULL;

	if (!pFormatMask || !pFormatMask->IsEnabled() || !m_pDocument)
		return NULL;

	switch (m_pDocument->GetFormMode())
	{
		// in stato di browse la formattazione va solo nei documenti batch
	case  CBaseDocument::BROWSE:	return m_pDocument->IsABatchDocument() ? pFormatMask : NULL;
		// in find ci va sempre
	case  CBaseDocument::FIND:		return pFormatMask;
		// in new e edit dipende se il control ha il bottone di autonumerazione e se lo 
		// state control è in numerazione manuale o automatica
	default:
		CStateCtrlObj* pStateCtrlObj = GetStateCtrl(m_pNumbererRequest->GetNumberingDisabled());
		return !pStateCtrlObj || (pStateCtrlObj->GetButton() || !pStateCtrlObj->IsInEditableState()) ? pFormatMask : NULL;
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrl::AttachDocument(CBaseDocument* pDoc)
{
	m_pDocument = pDoc;
}
//-----------------------------------------------------------------------------
void CParsedCtrl::ToggleFormatMask()
{
	CFormatMask* pFormatMask = m_pNumbererRequest->GetFormatMask();
	if (pFormatMask)
		pFormatMask->SetEnabled(!pFormatMask->IsEnabled());
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::AttachNumberer(CRuntimeClass* pRequestClass)
{
	return AttachNumberer(_T(""), FALSE, TRUE, pRequestClass);
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::AttachNumberer(const CString sEntity /*_T("")*/, BOOL hasButton /*TRUE*/, BOOL useFormatter /*TRUE*/, CRuntimeClass* pRequestClass /*= NULL*/)
{
	CNumbererRequest* pUserRequest = NULL;
	if (pRequestClass != NULL)
	{
		pUserRequest = (CNumbererRequest*)pRequestClass->CreateObject();
		pUserRequest->SetOwner(m_pOwnerWnd);
		pUserRequest->SetData(m_pData);
		pUserRequest->SetEntity(sEntity);
	}

	CStateCtrlObj*	pStateCtrl = AttachNumberer(pUserRequest, sEntity, hasButton, useFormatter);

	if (m_pNumbererRequest != pUserRequest)
		SAFE_DELETE(pUserRequest);

	return pStateCtrl;
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CParsedCtrl::AttachNumberer(CNumbererRequest* pUserRequest /*= NULL*/, const CString sEntity /*_T("")*/, BOOL hasButton /*TRUE*/, BOOL useFormatter /*TRUE*/)
{
	INumbererService* pService = NULL;
	// per primissima cosa definisco se ho la gestione della numerazione 
	if (m_pDocument)
	{
		// recupero la richiesta di numerazione e la aggancio al gestore
		IBehaviourContext* pContext = m_pDocument->GetBehaviourContext();
		if (pContext)
		{
			m_pNumbererRequest = CNumbererRequest::GetRequestFor(m_pData, pContext, sEntity);
			if (m_pNumbererRequest)
				pService = dynamic_cast<INumbererService*>(pContext->GetService(m_pNumbererRequest));
		}
	}

	// non ho la gestione della numerazione, allora mi faccio la mia interna 
	if (!m_pNumbererRequest)
	{
		if (pUserRequest)
			m_pNumbererRequest = pUserRequest;
		else
			m_pNumbererRequest = new CNumbererRequest(m_pOwnerWnd, m_pData, sEntity);
		m_Requests.Add(m_pNumbererRequest);
		pService = dynamic_cast<INumbererService*>(this->GetService(m_pNumbererRequest));
	}

	if (pService)
		pService->ReadInfo(m_pNumbererRequest);

	if (m_pNumbererRequest->GetFormatMask() && !useFormatter)
		m_pNumbererRequest->GetFormatMask()->SetEnabled(FALSE);

	// lo aggiungo alla mappa di notifica a prescindere
	if (GetCtrlParent())
	{
		CParsedForm* pParsedForm = GetParsedForm(GetCtrlParent());
		if (pParsedForm)
			pParsedForm->GetControlLinks()->AddToNotifyMap(m_pOwnerWnd);
	}

	// se non ho il bottone è sola formattazione
	if (!hasButton)
		return NULL;

	// se c'è la richiesta di numerazione nel documento il tastino è legato ad esso, altrimenti alla richiesta di info
	DataBool* pDataDisabled = m_pNumbererRequest->GetNumberingDisabled();

	AttachStateData(pDataDisabled);

	CStateCtrlObj* pStateCtrl = GetStateCtrl(pDataDisabled);
	pStateCtrl->AttachNumberingRequest(m_pNumbererRequest);

	// se ho un informazione di chiave primaria la setto
	pStateCtrl->EnableStateInEditMode(!m_pNumbererRequest->IsPrimaryKey());

	// se è abilitata la numerazione faccio in modo che il control si pulisca in automatico
	if (*pDataDisabled == FALSE)
		pStateCtrl->SetClearDataWhenEditable(TRUE);

	pStateCtrl->EnableCtrlInEditMode(!m_pNumbererRequest->IsPrimaryKey());
	pStateCtrl->DisableButton(*pDataDisabled);

	return pStateCtrl;
}

//-----------------------------------------------------------------------------
void CParsedCtrl::DoMaskedGetValue(CString& aValue, DataObj& aDataObj)
{
	if (GetFormatMask())
	{
		DataDate* pDate = NULL;
		if (m_pNumbererRequest->GetParams() != NULL && m_pNumbererRequest->GetParams()->IsKindOf(RUNTIME_CLASS(CDateNumbererRequestParams)))
			pDate = ((CDateNumbererRequestParams*)m_pNumbererRequest->GetParams())->GetDocDate();

		aDataObj.Assign(GetFormatMask()->ApplyMask
		(
			aValue,
			TRUE,
			pDate && !pDate->IsEmpty() ? pDate->Year() : AfxGetApplicationDate().Year())
		);
		aDataObj.SetUpdateView(TRUE);
	}
	else
		aDataObj.Assign(aValue);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::BindParam(DataObj* pObj, int n/*=-1*/)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->BindParam(pObj, n);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::EnableDrop(BOOL bEnable /*= TRUE*/, CParsedCtrlDropTarget* pDropTarget /*= NULL*/)
{
	if (!m_pDropTarget)
	{
		// creo il drop target...
		m_pDropTarget = (pDropTarget) ? pDropTarget : new CParsedCtrlDropTarget();
		ASSERT(m_pDropTarget->IsKindOf(RUNTIME_CLASS(CParsedCtrlDropTarget)));
	}

	m_pDropTarget->AttachTarget(this);
}

//-----------------------------------------------------------------------------
void CParsedCtrl::UpdateViewModel(BOOL bParentIsVisible)
{
	CWnd* pWnd = GetCtrlCWnd();
	if (!pWnd || !pWnd->m_hWnd)
		return;
	ReadPropertiesFromJson();

	// Is present State controls
	if (GetStateCtrlsArray().GetCount() > 0 && bParentIsVisible)
	{
		AdjustButtonsVisualizationUpdateControls();
	}

	if (
		IsDataModified() ||
		ForceUpdateCtrlView()
		)
	{
		UpdateCtrlStatus();
		if (!AfxIsRemoteInterface())
		{
			UpdateCtrlView();
			if (GetCtrlCWnd())
				GetCtrlCWnd()->UpdateWindow();
		}
	}

}

//-----------------------------------------------------------------------------
CJsonContextObj* CParsedCtrl::GetJsonContext()
{
	CWnd* pWnd = GetCtrlParent();
	CLocalizableDialog* pForm = dynamic_cast<CLocalizableDialog*>(pWnd);
	if (pForm)
		return pForm->GetJsonContext();

	if (dynamic_cast<CTBGridControlObj*>(pWnd) || dynamic_cast<CGridControlObj*>(pWnd))
		pWnd = pWnd->GetParent();

	pForm = dynamic_cast<CLocalizableDialog*>(pWnd);
	return pForm ? pForm->GetJsonContext() : NULL;
}
//-----------------------------------------------------------------------------
void CParsedCtrl::ReadPropertiesFromJson()
{
	if (!m_pOwnerWndDescription)
		return;

	CJsonContextObj* pContext = GetJsonContext();
	if (pContext)
		m_pOwnerWndDescription->EvaluateExpressions(pContext);

	if (m_pOwnerWndDescription->m_nNumberDecimal > -1)
		SetCtrlNumDec(m_pOwnerWndDescription->m_nNumberDecimal);

	if (m_pOwnerWndDescription->m_nTextLimit > 0)
		SetCtrlMaxLen(m_pOwnerWndDescription->m_nTextLimit);
	
	CString strCaption;
	if (m_pCaption)
		m_pCaption->GetWindowText(strCaption);
	CString sNewCaption = AfxLoadJsonString(m_pOwnerWndDescription->m_strControlCaption, m_pOwnerWndDescription);
	if (strCaption != sNewCaption)
	{
		int nCaptionW = m_pOwnerWndDescription->m_CaptionWidth;
		if (nCaptionW == NULL_COORD)
			nCaptionW = m_pOwnerWndDescription->GetParent()->m_CaptionWidth;
		if (nCaptionW != NULL_COORD)
		{
			CRect rCaption = CRect(0, 0, nCaptionW, 0);
			::MapDialogRect(GetCtrlParent()->m_hWnd, rCaption);
			nCaptionW = rCaption.Width();
		}
		SetCtrlCaption(sNewCaption,
			m_pOwnerWndDescription->m_CaptionHorizontalAlign,
			m_pOwnerWndDescription->m_CaptionVerticalAlign,
			CParsedCtrl::Left,
			nCaptionW,
			FALSE);
	}
}

//=============================================================================
//			Class CGridControlObj implementation
//=============================================================================
//------------------------------------------------------------------------------
CGridControlObj::CGridControlObj(CWnd* pWnd, OSLTypeObject eType, const CString& sName)
	:
	IOSLObjectManager(eType),
	m_pControlWnd(pWnd),
	m_pParentForm(NULL),
	m_wKeyState(0),
	m_bIsInspecting(FALSE)
{
	SetName(sName);
}

//------------------------------------------------------------------------------
CTBNamespace* CGridControlObj::GetOwnerModule()
{
	if (m_pControlWnd)
	{
		CWnd* pParent = m_pControlWnd->GetParent();
		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
			return ((CBaseFormView*)pParent)->GetOwnerModule();
		else if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
			return((CParsedDialog*)pParent)->GetOwnerModule();
	}

	return NULL;
}

//------------------------------------------------------------------------------
BOOL CGridControlObj::IsDisableOtherContext()
{
	if (m_pControlWnd)
	{
		CWnd* pParent = m_pControlWnd->GetParent();
		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
			return ((CBaseFormView*)pParent)->IsDisableOtherContext();
		else if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
			return((CParsedDialog*)pParent)->IsDisableOtherContext();
	}

	return NULL;
}

//------------------------------------------------------------------------------
void CGridControlObj::SetParentForm(CParsedForm* pParentForm)
{
	m_pParentForm = pParentForm;

	if (pParentForm)
	{
		GetInfoOSL()->m_pParent = pParentForm->GetInfoOSL();
		GetNamespace().SetChildNamespace(CTBNamespace::GRID, m_sName, pParentForm->GetNamespace());
	}
}

//----------------------------------------------------------------------------------------------
void CGridControlObj::SetName(const CString& sName)
{
	CString strName = sName;
	strName.Trim();

	if (!strName.IsEmpty())
		m_sName = strName;

	if (strName.IsEmpty() && m_sName.IsEmpty() && m_pControlWnd)
	{
		m_sName = m_pControlWnd->GetRuntimeClass()->m_lpszClassName;
		TRACE(_T("Empty name for class %s\n"), (LPCTSTR)m_sName);
	}
}

//------------------------------------------------------------------------------------------
CBaseDocument* CGridControlObj::GetDocument() const
{
	if (m_pParentForm)
		return m_pParentForm->GetDocument();

	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CGridControlObj::DoKeyUp(UINT nKey)
{
	switch (nKey)
	{
	case VK_CONTROL:	m_wKeyState &= ~KEY_CTRL_DOWN;	break;
	case VK_SHIFT:		m_wKeyState &= ~KEY_SHIFT_DOWN;	break;
	case VK_MENU:		m_wKeyState &= ~KEY_ALT_DOWN;	break;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CGridControlObj::DoKeyDown(UINT nKey, BOOL bDoMove)
{
	switch (nKey)
	{
	case VK_CONTROL:	m_wKeyState |= KEY_CTRL_DOWN; 	return FALSE;
	case VK_SHIFT:		m_wKeyState |= KEY_SHIFT_DOWN;	return FALSE;
	case VK_MENU:		m_wKeyState |= KEY_ALT_DOWN;	return FALSE;
	}

	if (bDoMove)
		return DoMovingKey(nKey);

	return TRUE;
}

//------------------------------------------------------------------------------------------
void CGridControlObj::GetKeyDownState(BOOL& shiftDown, BOOL& ctrlDown, BOOL& altDown) const
{
	shiftDown = (m_wKeyState & KEY_SHIFT_DOWN) == KEY_SHIFT_DOWN;
	ctrlDown = (m_wKeyState & KEY_CTRL_DOWN) == KEY_CTRL_DOWN;
	altDown = (m_wKeyState & KEY_ALT_DOWN) == KEY_ALT_DOWN;
}

//-----------------------------------------------------------------------------
CWnd* CGridControlObj::GetNextViewTabItem(BOOL bPrev /* = TRUE */, CWnd* pDef /* = NULL */)
{
	// si determina il successivo control nella sequenza di tab stop
	// non accettando il CBodyEdit corrente come control valido
	// (esempio quando il CBodyEdit e` l'unico control nella dialog
	// o quando tutti i controls sono disabilitati)
	//
	CWnd* pForm = m_pControlWnd->GetParent();
	CWnd* pWnd = pForm->GetNextDlgTabItem(m_pControlWnd, bPrev);

	while (pWnd && pWnd->m_hWnd != m_pControlWnd->m_hWnd)
	{
		if (pWnd->IsWindowEnabled() && !pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
			return pWnd;

		pWnd = pForm->GetNextDlgTabItem(pWnd, bPrev);
	}

	if (pForm->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
	{
		CBaseTileDialog* pTileDialog = (CBaseTileDialog*)pForm;
		pTileDialog->SetNextControlFocus(bPrev); //Set the focus to next control in TileGroup
		return NULL;
	}

	if (!pForm->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
		return pDef;

	CWnd* pTabber = pForm->GetParent();
	pForm = pTabber->GetParent();

	pWnd = pForm->GetNextDlgTabItem(pTabber, bPrev);

	while (pWnd && pWnd->m_hWnd != pTabber->m_hWnd)
	{
		if (pWnd->IsWindowEnabled() && !pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
			return pWnd;

		pWnd = pForm->GetNextDlgTabItem(pWnd, bPrev);
	}

	return pDef;
}
//-----------------------------------------------------------------------------
CGridControlObj* CGridControlObj::FromChild(CWnd* pWnd, CWnd* pWndContainer /*= NULL*/)
{
	CGridControlObj* pGrid = NULL;
	while (pWnd && pWnd != pWndContainer)
	{
		if (pGrid = dynamic_cast<CGridControlObj*>(pWnd))
			return pGrid;
		pWnd = pWnd->GetParent();
	}
	return NULL;
}

//=============================================================================
//			Class CGridControl implementation
//=============================================================================
IMPLEMENT_DYNAMIC(CGridControl, CButton)

BEGIN_MESSAGE_MAP(CGridControl, CButton)
	//{{AFX_MSG_MAP(CGridControl)
	ON_WM_ERASEBKGND()
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BOOL CGridControl::OnEraseBkgnd(CDC*)
{
	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CGridControl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	if (m_nDirStrech > 0)
		DoRecalcCtrlSize();
	return 0L;
}

//------------------------------------------------------------------------------
void CGridControl::OnUpdateControls(BOOL bParentIsVisible)
{
}

//=============================================================================
//			Class CTBGridControlObj
//=============================================================================
IMPLEMENT_DYNAMIC(CTBGridControlObj, CBCGPGridCtrl)

BEGIN_MESSAGE_MAP(CTBGridControlObj, CBCGPGridCtrl)
	//{{AFX_MSG_MAP(CResizableStrEdit)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//------------------------------------------------------------------------------
CTBGridControlObj::CTBGridControlObj()
{
}

//------------------------------------------------------------------------------
void CTBGridControlObj::OnUpdateControls(BOOL bParentIsVisible)
{
}

//------------------------------------------------------------------------------
BOOL  CTBGridControlObj::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk = __super::Create(dwStyle, rect, pParentWnd, nID);
	if (bOk)
		InitSizeInfo(this);
	return bOk;
}

//------------------------------------------------------------------------------
LRESULT	CTBGridControlObj::OnRecalcCtrlSize(WPARAM, LPARAM)
{

	DoRecalcCtrlSize();
	AdjustLayout();
	return 0L;
}

//===========================================================================
// CParsedForm
//===========================================================================

//---------------------------------------------------------------------------
CParsedForm::CParsedForm(CWnd* pOwnerWnd, UINT nIDTemplate, const CString& sName)
	:
	m_sName(sName),
	m_nID(nIDTemplate),
	m_pOwner(NULL),
	m_pOwnerWnd(pOwnerWnd),
	m_phLastCtrlFocused(NULL),
	m_pNotValidCtrlID(NULL),
	m_pErrorMode(NULL),
	m_bNoOtherContext(FALSE),
	m_nDefaultToolTipWidth(-1),
	m_nImgWidth(0),
	m_nImgHeight(0),
	m_hBackgroundImg(NULL),
	m_locBackgroundImg(BACKGR_TILE),
	m_csOrigin(CSize(0, 0)),
	m_pBrushBackground(NULL),
	m_pFont(NULL),
	m_bCenterControls(FALSE),
	m_bCenterControlsCustomized(FALSE),
	m_bTransparent(FALSE),
	m_pLayoutContainer(NULL),
	m_pDocument(NULL),
	m_pControlLinks(new ControlLinks()),
	m_bNativeWindowVisible(TRUE)
{
	m_clrBackground = AfxGetThemeManager()->GetBackgroundColor();

	m_clrDefaultToolTipText = AfxGetThemeManager()->GetTooltipForeColor();
	m_clrDefaultToolTipBackgnd = AfxGetThemeManager()->GetTooltipBkgColor();

	ASSERT_VALID(pOwnerWnd);
	pOwnerWnd->EnableToolTips(TRUE);
}

//-----------------------------------------------------------------------------
CParsedForm::~CParsedForm()
{
	SAFE_DELETE(m_pControlLinks);

	if (m_hBackgroundImg)
		DeleteObject(m_hBackgroundImg);

	if (m_pBrushBackground)
	{
		m_pBrushBackground->DeleteObject();
		SAFE_DELETE(m_pBrushBackground);
	}

	if (m_pFont)
	{
		m_pFont->DeleteObject();
		SAFE_DELETE(m_pFont);
	}
	m_Panel.RemoveAll();
}

//------------------------------------------------------------------------------
void CParsedForm::SetInitialDefaultFocus(UINT nIDC)
{
	CWnd* pCtrl = m_pOwnerWnd->GetDlgItem(nIDC);
	if (!pCtrl)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT(m_phLastCtrlFocused);
	if (m_phLastCtrlFocused)
		*m_phLastCtrlFocused = pCtrl->m_hWnd;
}

//------------------------------------------------------------------------------
/*STATIC*/ void CParsedForm::CenterControls(CWnd* pWnd, int cx, int cy)
{
	if (AfxIsRemoteInterface())
		return;
	CSize	szRect;
	CRect	rcChild;
	CArray<CRect, CRect> arControlsRect;
	CArray<CWnd*, CWnd*> arControlsCWnd;
	int nBorderSize = 5;

	//salvo in un array i rettangoli di tutte le child wnd e in un altro array il puntatore alla finestra
	CWnd* pwndChild = pWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		if (pwndChild->m_hWnd != pWnd->m_hWnd)
		{
			if
				(
					dynamic_cast<ResizableCtrl*>(pwndChild) != NULL
					/*
					pwndChild->IsKindOf(RUNTIME_CLASS(CTBTreeCtrl))	||
					pwndChild->IsKindOf(RUNTIME_CLASS(CShowFileTextStatic)) ||
					pwndChild->IsKindOf(RUNTIME_CLASS(CParsedRichCtrl))		||
					pwndChild->IsKindOf(RUNTIME_CLASS(CParsedWebCtrl))		||
					pwndChild->IsKindOf(RUNTIME_CLASS(CPictureStatic))		||
					pwndChild->IsKindOf(RUNTIME_CLASS(CResizableStrEdit))	||
					pwndChild->IsKindOf(RUNTIME_CLASS(CResizableStrStatic))	||
					pwndChild->IsKindOf(RUNTIME_CLASS(CTreeViewAdvCtrl))	||
					pwndChild->IsKindOf(RUNTIME_CLASS(CGanttCtrl))			||
					pwndChild->IsKindOf(RUNTIME_CLASS(CBaseTileGroup))		||
					dynamic_cast<CGridControlObj*>(pwndChild) != NULL		||
					pwndChild->IsKindOf(RUNTIME_CLASS(CBCGPGridCtrl))
					*/
					)
				return;

			pwndChild->GetWindowRect(rcChild);
			(pWnd->GetParent())->ScreenToClient(rcChild);
			arControlsRect.Add(rcChild);
			arControlsCWnd.Add(pwndChild);
		}
		pwndChild = pwndChild->GetNextWindow();
	}

	if (arControlsRect.GetSize() <= 0)
		return;

	//calcolo la size del rettangolo che contiene tutti i control

	CRect firstControlRect = arControlsRect.GetAt(0);
	int nBoxTop = firstControlRect.top;
	int nBoxRight = firstControlRect.right;
	int nBoxLeft = firstControlRect.left;
	int nBoxBottom = firstControlRect.bottom;

	for (int i = 1; i < arControlsRect.GetSize(); i++)
	{
		CRect controlRect = arControlsRect.GetAt(i);

		if (controlRect.bottom > nBoxBottom)
			nBoxBottom = controlRect.bottom;

		if (controlRect.left < nBoxLeft)
			nBoxLeft = controlRect.left;

		if (controlRect.top < nBoxTop)
			nBoxTop = controlRect.top;

		if (controlRect.right > nBoxRight)
			nBoxRight = controlRect.right;
	}

	szRect.cx = nBoxRight - nBoxLeft;
	szRect.cy = nBoxBottom - nBoxTop;

	CSize szOffset;
	//calcolo l'hoffset

	szOffset.cx = (cx - szRect.cx) / 2;
	szOffset.cy = (cy - szRect.cy) / 2;

	if (szOffset.cx < 0)
		szOffset.cx = 0;

	if (szOffset.cy < 0)
		szOffset.cy = 0;

	if (szOffset.cx < nBorderSize)
		szOffset.cx = nBorderSize;

	if (szOffset.cy < nBorderSize)
		szOffset.cy = nBorderSize;

	//muovo ogni child della distanza tra lei e il rettagolone + l'offset
	ASSERT(arControlsCWnd.GetSize() == arControlsRect.GetSize());

	for (int i = 0; i < arControlsCWnd.GetSize(); i++)
	{
		CRect controlRect = arControlsRect.GetAt(i);
		arControlsCWnd.GetAt(i)->MoveWindow(
			controlRect.left - nBoxLeft + szOffset.cx,
			controlRect.top - nBoxTop + szOffset.cy,
			controlRect.Width(),
			controlRect.Height(),
			TRUE
		);
	}
}

//---------------------------------------------------------------------------
/*static*/ void CParsedForm::GetCandidateRectangles(
	CWnd*		pWnd,
	CTBToolBar*	pToolBar,
	int			nToolbarHeight,
	CRect&		rcOtherComponents,
	CRect&		rcToolbar,
	int			cx /*-1*/,
	int			cy /*-1*/
)
{
	CRect aRect;
	pWnd->GetClientRect(aRect);
	rcOtherComponents = aRect;

	// senza toolbar usa tutta l'area
	if (!pToolBar)
		return;

	pToolBar->GetClientRect(rcToolbar);
	pToolBar->ClientToScreen(rcToolbar);
	pWnd->ScreenToClient(rcToolbar);

	rcToolbar.left = 0;
	rcToolbar.right = aRect.right;

	// toolbar inferiore l'eventuale altro componente si accorcia (vd tilegroup)
	if ((pToolBar->GetEnabledAlignment() & CBRS_ALIGN_BOTTOM) == CBRS_ALIGN_BOTTOM)
	{
		rcToolbar.bottom = aRect.bottom;
		rcToolbar.top = aRect.bottom - nToolbarHeight;
		rcOtherComponents.bottom = rcToolbar.top - 1;
	}
	else
	{
		// toolbar superiore si abbassa l'eventuale altro componente (vd tilegroup)
		rcOtherComponents.top = aRect.top + nToolbarHeight + 1;
		rcOtherComponents.bottom = aRect.bottom;

		// ci sono alcune toolbar top gestite completamente a mano
		// quindi devo ricalcolare solo in caso lo gestisca io
		if (nToolbarHeight)
		{
			rcToolbar.top = 0;
			rcToolbar.bottom = rcToolbar.top + nToolbarHeight;
		}
	}
}

//-----------------------------------------------------------------------------
void CParsedForm::SetBackgroundColor(COLORREF crBackground)
{
	if (m_pBrushBackground)
	{
		m_pBrushBackground->DeleteObject();
		SAFE_DELETE(m_pBrushBackground);
	}
	m_clrBackground = crBackground;
	m_pBrushBackground = new CBrush(crBackground);
}

//-----------------------------------------------------------------------------
void CParsedForm::SetOwnFont(LPCTSTR lpszFaceName, int nPointSize)
{
	if (m_pFont)
	{
		m_pFont->DeleteObject();
		SAFE_DELETE(m_pFont);
	}

	m_pFont = new CFont();

	LOGFONT lf;
	memset(&lf, 0, sizeof(lf));

	AfxGetThemeManager()->GetFormFont()->GetLogFont(&lf);

	lf.lfHeight = ::GetDisplayFontHeight(nPointSize);
	_tcsncpy_s(lf.lfFaceName, lpszFaceName, 32);

	m_pFont->CreateFontIndirect(&lf);

	if (m_pOwnerWnd && m_pOwnerWnd->m_hWnd)
	{
		m_pOwnerWnd->SetFont(m_pFont);
	}
}

//-----------------------------------------------------------------------------
void CParsedForm::AddBkgndImageDescription(CWndImageDescription* pDesc)
{
	if (m_hBackgroundImg)
	{
		CString sName = cwsprintf(_T("viewbkg%ud.png"), m_hBackgroundImg);

		if (pDesc->m_ImageBuffer.Assign(m_hBackgroundImg, sName, m_pOwnerWnd))
			pDesc->SetUpdated(&pDesc->m_ImageBuffer);
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::DoEraseBkgnd(CDC* pDC)
{
	if (m_hBackgroundImg)
	{
		CDC memdc;
		memdc.CreateCompatibleDC(pDC);
		HGDIOBJ hOldBitmap = memdc.SelectObject(m_hBackgroundImg);

		CRect rc;
		m_pOwnerWnd->GetClientRect(&rc);
		int xScroll = 0;
		int yScroll = 0;
		if (pDC->GetMapMode() == MM_TEXT)
		{
			xScroll = m_pOwnerWnd->GetScrollPos(SB_HORZ);
			yScroll = m_pOwnerWnd->GetScrollPos(SB_VERT);
		}
		else
		{
			// @@TODO Da convertire in device coordinates
		}

		switch (m_locBackgroundImg)
		{
		case BACKGR_TILE:

			for (int y = 0; y < rc.Height(); y += m_nImgHeight)
			{ // for each row:
				for (int x = 0; x < rc.Width(); x += m_nImgWidth)
				{ // for each column:
					pDC->BitBlt(x, y, m_nImgWidth, m_nImgHeight, &memdc, 0, 0, SRCCOPY); // copy
				}
			}
			break;

		case BACKGR_TOPLEFT:

			pDC->FillRect(&rc, const_cast<CBrush*>(AfxGetThemeManager()->GetBackgroundColorBrush()));

			pDC->BitBlt(m_csOrigin.cx - xScroll, m_csOrigin.cy - yScroll, m_nImgWidth, m_nImgHeight, &memdc, 0, 0, SRCCOPY); // copy
			break;
		default:
			ASSERT(FALSE); //TODO
		}

		memdc.SelectObject(hOldBitmap);
		memdc.DeleteDC();
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::SetBackgroundImage(const CString& strImage, BackgroundLocation loc/*= BACKGR_TILE*/, CSize csOrigin /*= CSize(0,0)*/)
{
	if (strImage.IsEmpty())
	{
		if (m_hBackgroundImg)
			DeleteObject(m_hBackgroundImg);
		m_hBackgroundImg = NULL;

		m_strBackgroundImg.Empty();

		m_pOwnerWnd->Invalidate();
		return TRUE;
	}

	CString s = GetValidImagePath(strImage);
	if (s.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_strBackgroundImg = strImage;
	m_locBackgroundImg = loc;
	m_csOrigin = csOrigin;

	if (m_hBackgroundImg)
		DeleteObject(m_hBackgroundImg);

	Gdiplus::Bitmap* pBmp = LoadGdiplusBitmapOrPng(AfxGetPathFinder()->GetNamespaceFromPath(s).GetObjectName());
	Gdiplus::Color color = Gdiplus::Color::Transparent;

	VERIFY(Gdiplus::Ok == pBmp->GetHBITMAP(color, &m_hBackgroundImg) && m_hBackgroundImg);
	m_nImgWidth = pBmp->GetWidth();
	m_nImgHeight = pBmp->GetHeight();
	delete pBmp;

	m_pOwnerWnd->Invalidate();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::SetBackgroundImage(UINT nIDBImage, BackgroundLocation loc/*= BACKGR_TILE*/, CSize csOrigin /*= CSize(0,0)*/)
{
	if (nIDBImage == 0)
	{
		if (m_hBackgroundImg)
			DeleteObject(m_hBackgroundImg);
		m_hBackgroundImg = NULL;

		m_pOwnerWnd->Invalidate();
		return TRUE;
	}

	m_strBackgroundImg.Empty();

	m_locBackgroundImg = loc;
	m_csOrigin = csOrigin;

	if (m_hBackgroundImg)
		DeleteObject(m_hBackgroundImg);

	CWalkBitmap wb;
	if (!wb.LoadBitmap(nIDBImage))
	{
		TRACE1("CParsedForm::SetBackgroundImage - LoadBitmaps : failed to load bitmap %d \n", nIDBImage);
		return FALSE;   // need this one image
	}

	BITMAP bmInfo;
	VERIFY(wb.GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));

	m_nImgWidth = bmInfo.bmWidth;
	m_nImgHeight = bmInfo.bmHeight;

	m_hBackgroundImg = (HBITMAP)wb.Detach();
	if (m_hBackgroundImg == NULL)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pOwnerWnd->Invalidate();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedForm::DoFormatStyleChange(WPARAM wParam, LPARAM lParam)
{
	m_pOwnerWnd->SendMessageToDescendants(UM_FORMAT_STYLE_CHANGED, wParam, lParam);
}

//-----------------------------------------------------------------------------
BOOL IsEnterAsTab()
{
	DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szTreatEnterAsTab, DataBool(FALSE), szTbDefaultSettingFileName);

	return pDataObj && (*((DataBool*)pDataObj));
}
//-----------------------------------------------------------------------------
BOOL IsEnterAsTabCached()
{
	static BOOL bEnterAsTab = IsEnterAsTab();
	return bEnterAsTab;
}
// Menage eventual special keys
//-----------------------------------------------------------------------------
BOOL CParsedForm::PreProcessMessage(MSG* pMsg)
{
	// CTRL-TAB deve essere ignorato
	if (pMsg->message != WM_KEYDOWN || pMsg->wParam != VK_TAB || (GetKeyState(VK_CONTROL) & 0x8000) == 0)
	{
		CWnd* pFocusWnd = GetLastFocusedCtrl();
		if (pFocusWnd && m_pOwnerWnd->IsChild(pFocusWnd))
		{
			if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
			{
				if (IsEnterAsTabCached() && !(GetKeyState(VK_CONTROL) & 0x8000))
				{
					::PostMessage(pMsg->hwnd, pMsg->message, VK_TAB, pMsg->lParam);
					return TRUE;
				}
			}

			BOOL bManaged = FALSE;
			CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pFocusWnd);
			if (pGrid)
			{
				CWnd* pCurrFocus = CWnd::FromHandlePermanent(GetFocus());
				if (pCurrFocus && pCurrFocus->m_hWnd != pFocusWnd->m_hWnd && GetParsedCtrl(pCurrFocus) && pFocusWnd->IsChild(pCurrFocus))
				{
					pGrid = NULL;
					pFocusWnd = pCurrFocus;
				}
			}

			if (!pGrid)
			{
				CParsedCtrl* pCtrl = GetParsedCtrl(pFocusWnd);
				if (pCtrl)
					bManaged = pCtrl->PreProcessMessage(pMsg);
				else
					if (IsWindow(pFocusWnd->GetSafeHwnd()))
						pGrid = dynamic_cast<CGridControlObj*>(pFocusWnd->GetParent());
			}

			if (pGrid && (pMsg->message == WM_KEYUP || pMsg->message == WM_KEYDOWN))
				bManaged = pGrid->OnKeyHit
				(
					pFocusWnd->GetDlgCtrlID(),
					(UINT)pMsg->wParam,
					pMsg->message
				);

			if (bManaged)
				return TRUE;
		}
	}

#ifndef _OLD_PTM

	// una popup non deve far fare nulla al topframe 
	CParsedDialog* pDlg = dynamic_cast<CParsedDialog*>(m_pOwnerWnd);
	if (pDlg && (pDlg->GetStyle() & WS_POPUP) == WS_POPUP)
		return FALSE;

	// don't translate dialog messages when in Shift+F1 help mode
	CFrameWnd* pFrameWnd = m_pOwnerWnd ? m_pOwnerWnd->GetTopLevelFrame() : NULL;
	if (pFrameWnd != NULL && pFrameWnd->m_bHelpMode)
		return FALSE;

	if (!dynamic_cast<CView*>(m_pOwnerWnd))
	{
		// since 'CBaseTabDialog::PreTranslateMessage' will eat frame window accelerators,
		//   we call the frame window's PreTranslateMessage first
		if ((pFrameWnd = m_pOwnerWnd->GetParentFrame()) != NULL)
		{
			if (pFrameWnd->PreTranslateMessage(pMsg))
				return TRUE;        // eaten by frame accelerator

			// check for parent of the frame in case of MDI
			if (
				(pFrameWnd = pFrameWnd->GetParentFrame()) != NULL &&
				pFrameWnd->PreTranslateMessage(pMsg)
				)
				return TRUE;        // eaten by frame accelerator
		}
	}

#endif

	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedForm::DoOnScroll(UINT /*nSBCode*/, UINT /*nPos*/, CScrollBar* /*pScrollBar*/, BOOL /*bIsVertScroll*/)
{
	DoResizeControls();
}

//-----------------------------------------------------------------------------
void CParsedForm::DoWindowPosChanged(WINDOWPOS* wndPos)
{
	if (!(wndPos->flags & SWP_NOSIZE))
		DoResizeControls();
}

//-----------------------------------------------------------------------------
/*static */void CParsedForm::DoResizeControls(CWnd* pOwnerWnd)
{
	if (!::IsWindow/*Visible*/(pOwnerWnd->m_hWnd))
		return;

	CWnd* pwndChild = pOwnerWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{

		if (dynamic_cast<CGridControl*>(pwndChild))
			pwndChild->SendMessage(UM_RECALC_CTRL_SIZE);
		else
			pwndChild->PostMessage(UM_RECALC_CTRL_SIZE);

		pwndChild = pwndChild->GetNextWindow();
	}
}

void CParsedForm::DoResizeControls()
{
	if (IsLayoutSuspended())
		return;

	DoResizeControls(m_pOwnerWnd);
}

// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL CParsedForm::DoCommand(WPARAM wParam, LPARAM lParam)
{
	ASSERT(m_pOwnerWnd);
	BOOL bNotFarwardToParent = DoCommand(wParam, lParam, m_pOwnerWnd->m_hWnd, m_phLastCtrlFocused);

	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hCtrl);

	if (!hCtrl || nCode != EN_VALUE_CHANGED)
		return bNotFarwardToParent;

	CWnd* pGrp = NULL;
	if (!m_Panel.GetCount() || !m_Panel.Lookup(hCtrl, pGrp))
		return bNotFarwardToParent;

	ASSERT_VALID(pGrp);
	ASSERT(pGrp->m_hWnd);

	ASSERT(pGrp->IsKindOf(RUNTIME_CLASS(CLabelStatic)) || pGrp->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)));
	if (!pGrp->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)))
		return bNotFarwardToParent;

	// simula l'invio di un changed come se arrivasse dal raggruppatore
	int nIDC = pGrp->GetDlgCtrlID();

	m_pOwnerWnd->POST_WM_COMMAND(nIDC, nCode, pGrp->m_hWnd);

	return bNotFarwardToParent;
}

//creato metodo statico a beneficio di classi managed mixed mode che ospitano ParsedControls
//------------------------------------------------------------------------------
BOOL CParsedForm::DoCommand(WPARAM wParam, LPARAM lParam, HWND hOwnerWnd, HWND* phCtrlFocus)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID != 0 && hWndCtrl != NULL)
	{
		//
		// control notification
		//

		if (nCode == PCN_SET_FOCUS)
		{
			// dato che questo e` un messaggio postato e` necessario
			// verificare se il control e` ancora vivo
			if (::IsWindow(hWndCtrl))
			{
				CWnd::FromHandle(hWndCtrl)->SetFocus();

				return FALSE;
			}

			return FALSE;
		}

		ASSERT(::IsWindow(hWndCtrl));
		CWnd* pCtrl = CWnd::FromHandle(hWndCtrl);
		CWnd* pOwnerWnd = CWnd::FromHandle(hOwnerWnd);

		if (!pCtrl || !pOwnerWnd->IsChild(pCtrl))
			return FALSE;

		TCHAR className[MAX_CLASS_NAME + 1];
		VERIFY(GetClassName(hWndCtrl, className, MAX_CLASS_NAME));

		// cerco se c'è un TabDialog come ancestore del control per far memorizzare al suo
		// DialogInfo l'IDC del control che riceve il fuoco
		CBaseTabDialog* pTabDlg = dynamic_cast<CBaseTabDialog*>(pOwnerWnd);
		CWnd* pTemp = pOwnerWnd;
		while (pTabDlg == NULL && pTemp != NULL)
		{
			pTemp = pTemp->GetParent();
			pTabDlg = dynamic_cast<CBaseTabDialog*>(pTemp);
		}

		int nFocusedID = nID;
		HWND hWndFocusedCtrl = hWndCtrl;
		HWND hWndParent = pCtrl->GetParent()->m_hWnd;

		CGridControlObj* pGridParent = dynamic_cast<CGridControlObj*>(pCtrl->GetParent());
		if (pGridParent)
		{
			hWndFocusedCtrl = hWndParent;
			hWndParent = pCtrl->GetParent()->GetParent()->m_hWnd;
			nFocusedID = pCtrl->GetParent()->GetDlgCtrlID();
		}

		// viene intercettato il SETFOCUS per notificare un generico messaggio 
		// UM_CTRL_FOCUSED alla finestra. (es: nella view per gestire il panning)
		if (_tcsicmp(className, _T("BUTTON")) == 0)
			switch (nCode)
			{
			case BN_SETFOCUS:
				// undocumented metod of CFormView to hold current focused control
				// (see CFormView::OnActivateView())
				*phCtrlFocus = hWndFocusedCtrl;
				if (pTabDlg) pTabDlg->SetLastFocusIDC(nFocusedID);
				::PostMessage(hOwnerWnd, UM_CTRL_FOCUSED, nFocusedID, MAKE_COMPATIBLE_HANDLE(hWndFocusedCtrl));
				return FALSE;

			default: return FALSE;
			}

		if (_tcsicmp(className, _T("EDIT")) == 0)
			switch (nCode)
			{
			case EN_SETFOCUS:
				// undocumented metod of CFormView to hold current focused control
				// (see CFormView::OnActivateView())
				*phCtrlFocus = hWndFocusedCtrl;
				if (pTabDlg) pTabDlg->SetLastFocusIDC(nFocusedID);
				::PostMessage(hOwnerWnd, UM_CTRL_FOCUSED, nFocusedID, MAKE_COMPATIBLE_HANDLE(hWndFocusedCtrl));
				return FALSE;

			default: return FALSE;
			}

		if (_tcsicmp(className, _T("RICHEDIT")) == 0)
			switch (nCode)
			{
			case EN_SETFOCUS:
				// undocumented metod of CFormView to hold current focused control
				// (see CFormView::OnActivateView())
				*phCtrlFocus = hWndFocusedCtrl;
				if (pTabDlg) pTabDlg->SetLastFocusIDC(nFocusedID);
				::PostMessage(hOwnerWnd, UM_CTRL_FOCUSED, nFocusedID, MAKE_COMPATIBLE_HANDLE(hWndFocusedCtrl));
				return FALSE;

			default: return FALSE;
			}

		if (_tcsicmp(className, _T("COMBOBOX")) == 0)
			switch (nCode)
			{
			case CBN_SETFOCUS:
				// undocumented metod of CFormView to hold current focused control
				// (see CFormView::OnActivateView())
				*phCtrlFocus = hWndFocusedCtrl;
				if (pTabDlg) pTabDlg->SetLastFocusIDC(nFocusedID);
				::PostMessage(hOwnerWnd, UM_CTRL_FOCUSED, nFocusedID, MAKE_COMPATIBLE_HANDLE(hWndFocusedCtrl));
				return FALSE;

			case CBN_SELCHANGE:
			case CBN_DBLCLK:
			case CBN_EDITCHANGE:
			case CBN_EDITUPDATE:
			case CBN_DROPDOWN:
			case CBN_CLOSEUP:
			case CBN_SELENDOK:
			case CBN_SELENDCANCEL:
				if (hWndParent == hOwnerWnd)
					::SendMessage(hWndCtrl, WM_COMMAND, wParam, lParam);
				return FALSE;

			default: return FALSE;
			}

		if (_tcsicmp(className, _T("LISTBOX")) == 0)
			switch (nCode)
			{
			case LBN_SETFOCUS:
				// undocumented metod of CFormView to hold current focused control
				// (see CFormView::OnActivateView())
				*phCtrlFocus = hWndFocusedCtrl;
				if (pTabDlg) pTabDlg->SetLastFocusIDC(nFocusedID);
				::PostMessage(hOwnerWnd, UM_CTRL_FOCUSED, nFocusedID, MAKE_COMPATIBLE_HANDLE(hWndFocusedCtrl));
				return TRUE;

			case LBN_SELCHANGE:
			case LBN_DBLCLK:
			case LBN_SELCANCEL:
				if (hWndParent == hOwnerWnd)
					::SendMessage(hWndCtrl, WM_COMMAND, wParam, lParam);
				return FALSE;

			default: return FALSE;
			}
	}

	return FALSE;
}

// manage form activation
//-----------------------------------------------------------------------------
BOOL CParsedForm::DoActivate(BOOL /*bActivate*/)
{
	return TRUE;
}

// Gestisce il messaggio  UM_LOSING_FOCUS che viene "sendato" all'atto della
// perdita di fuoco nel caso in cui il dato non sia BAD o (in caso di warning)
// dopo la diagnostica del warning stesso, allorche` l'utente accetta il valore
// premendo il botton OK della MessageBox. In questo modo si resetta il data member
// m_nNotValidCtrlID che ha funzione di semaforo per inibire messaggistiche
// incrociate (vedi CParsedForm::DoBadValue)
//------------------------------------------------------------------------------
void CParsedForm::DoLosingFocus(UINT nCtrlID, int nRelationship)
{
	ASSERT(nCtrlID);
	ASSERT(m_pNotValidCtrlID);

	if (*m_pNotValidCtrlID && nCtrlID == *m_pNotValidCtrlID)
		InitFormFlags();
}

// Gestisce il messaggio di UM_BAD_VALUE : il data member m_nNotValidCtrlID 
// ha funzione di semaforo per inibire messaggistiche incrociate
// (vedi CParsedForm::DoLosingFocus)
//--------------------------------------------------------------------------
LRESULT CParsedForm::DoBadValue(UINT nCtrlID, WORD wMode)
{
	ASSERT_VALID(m_pOwnerWnd);
	ASSERT_VALID(m_pOwnerWnd->GetParent());
	ASSERT(m_pNotValidCtrlID);
	ASSERT(m_pErrorMode);

	if (*m_pNotValidCtrlID)
	{
		ASSERT(nCtrlID != 0 || (wMode & CTRL_IMMEDIATE_NOTIFY) == CTRL_IMMEDIATE_NOTIFY);

		// si rigetta la richiesta di gestione di messaggistica di errore
		// se il control richiedente non e` lo stesso che ha in precedenza
		// settato lo stato di bad (p.e. si va a finire su un control anche lui
		// bad che invia la richiesta sulla perdita del fuoco a fronte della
		// MessageBox)
		//
		if (nCtrlID && nCtrlID != *m_pNotValidCtrlID)
			return 99L;	// rigetta la richiesta
	}
	else
	{
		ASSERT(nCtrlID);
		SetBadEdit(nCtrlID, wMode & ~CTRL_IMMEDIATE_NOTIFY);
	}

	// si ritorna senza emettere messaggi se non e` una richiesta
	// di immediata notifica (cosi` sono anche le richieste di autorizzazione
	// inviate dai control complessi come i CGridControl o i CBaseTabManager)
	//
	if ((wMode & CTRL_IMMEDIATE_NOTIFY) != CTRL_IMMEDIATE_NOTIFY)
		return 0L;

	CWnd* pWnd = GetChildCtrlWndFromID(m_pAncestorWnd, *m_pNotValidCtrlID);

	if (pWnd == NULL)
	{
		ASSERT(FALSE);

		InitFormFlags();
		return 0L;
	}

	if (dynamic_cast<CGridControlObj*>(pWnd))
	{
		ASSERT(nCtrlID == 0 && (wMode & CTRL_IMMEDIATE_NOTIFY) == CTRL_IMMEDIATE_NOTIFY);

		// si demanda al control complesso la gestione della messaggistica
		//
		pWnd->SendMessage(UM_BAD_VALUE, 0, *m_pErrorMode | CTRL_IMMEDIATE_NOTIFY);

		return 0L;
	}

	// finalmente si puo` dare il messaggio
	//
	CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);
	if (pCtrl) (void)pCtrl->ErrorMessage();

	return 0L;
}

//-----------------------------------------------------------------------------
CWnd* CParsedForm::GetLastFocusedCtrl() const
{
	return m_phLastCtrlFocused && *m_phLastCtrlFocused ? CWnd::FromHandlePermanent(*m_phLastCtrlFocused) : NULL;
}

//-----------------------------------------------------------------------------
void CParsedForm::InitFormFlags()
{
	ASSERT(m_pNotValidCtrlID);
	ASSERT(m_pErrorMode);

	*m_pNotValidCtrlID = 0;
	*m_pErrorMode = 0;
}


//-----------------------------------------------------------------------------
void CParsedForm::SetBadEdit(UINT nCtrlID, WORD wMode)
{
	ASSERT(m_pNotValidCtrlID);
	ASSERT(m_pErrorMode);

	if (*m_pNotValidCtrlID == 0)
	{
		*m_pNotValidCtrlID = nCtrlID;
		*m_pErrorMode = wMode;
	}
}

//-----------------------------------------------------------------------------
CWnd* CParsedForm::GetActiveChild()
{
	ASSERT(m_pNotValidCtrlID);
	ASSERT(m_pErrorMode);

	CWnd* pWnd = NULL;

	if (*m_pNotValidCtrlID == 0)
	{
		pWnd = GetLastFocusedCtrl();
		if (pWnd == NULL)
			return NULL;

		CParsedCtrl* pParsedCtrl = GetParsedCtrl(pWnd);
		if (pParsedCtrl)
		{
			// si cerca il control figlio diretto della corrente
			// form e ancestore del control che possiede il fuoco
			// (potrebbe essere lui stesso)
			if (dynamic_cast<CGridControlObj*>(pWnd->GetParent()))
				return pWnd->GetParent();
			else
				return pWnd;
		}
	}
	else
	{
		pWnd = GetChildCtrlWndFromID(m_pAncestorWnd, *m_pNotValidCtrlID);

		// ora puo` resettare lo stato di bad
		InitFormFlags();
	}

	ASSERT(pWnd);
	return pWnd;
}

//-----------------------------------------------------------------------------
void CParsedForm::AbortForm()
{
	CWnd* pWnd = GetActiveChild();

	if (pWnd == NULL)
		return;

	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
	if (pGrid)
	{
		pGrid->OnAbortForm();
		return;
	}
	else
	{
		CParsedCtrl* pParsedCtrl = GetParsedCtrl(pWnd);
		if (pParsedCtrl)
		{
			pParsedCtrl->UpdateCtrlView();
			pParsedCtrl->SetErrorID(CParsedCtrl::EMPTY_MESSAGE);
			pParsedCtrl->SetModifyFlag(FALSE);
		}
	}
}

//--------------------------------------------------------------------------
BOOL CParsedForm::CheckForm(BOOL bEmitError /*= TRUE */)
{
	ASSERT(m_pNotValidCtrlID);
	ASSERT(m_pErrorMode);

	CWnd* pWnd = GetActiveChild();
	if (pWnd == NULL)
		return TRUE;

	CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
	if (pGrid)
		pGrid->OnCheckForm(FALSE);
	else
	{
		CParsedCtrl* pParsedCtrl = GetParsedCtrl(pWnd);

		if (pParsedCtrl && pWnd->IsWindowEnabled())
		{
			// tenta di updatare il dato : in caso di errore/warning
			// NON da subito il messaggio (FALSE) e viene SENDATO il
			// messaggio di UM_BAD_VALUE (TRUE); in questo modo vengono
			// solamente valorizzati i membri m_nNotValidCtrlID e m_wErrorMode
			// che sono poi utilizzati piu` sotto
			//
			pParsedCtrl->UpdateCtrlData(FALSE, TRUE);
		}
	}

	if (*m_pNotValidCtrlID)
	{
		if (bEmitError)
			DoBadValue(0, CTRL_IMMEDIATE_NOTIFY);

		// se era un warning, nel caso sia stato accettato m_nNotValidCtrlID e` stato
		// azzerato
		return *m_pNotValidCtrlID == 0;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedForm::SetToolTipBuffer(const CString& strTooltip)
{
	m_strToolTipBuffer = strTooltip;
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::DoToolTipNotify(UINT nId, NMHDR* pNMHDR, LRESULT* pResult)
{
	static CToolTipCtrl* pToolTip = NULL;
	// bug di MFC: Q316312 - non posso usare la AfxGetModuleThreadState()
	//CToolTipCtrl* ptt = AfxGetThreadState()->m_pToolTip;
	CToolTipCtrl* ptt = AfxGetModuleThreadState()->m_pToolTip;
	if (ptt && ptt->m_hWnd && ptt != pToolTip)
	{
		m_nDefaultToolTipWidth = ptt->GetMaxTipWidth();
		m_clrDefaultToolTipBackgnd = ptt->GetTipBkColor();
		m_clrDefaultToolTipText = ptt->GetTipTextColor();
		pToolTip = ptt;
	}

	CTooltipProperties tp;
	tp.m_nWidth = m_nDefaultToolTipWidth;
	tp.m_clrText = m_clrDefaultToolTipText;
	tp.m_clrBackgnd = m_clrDefaultToolTipBackgnd;
	tp.m_strText = m_strToolTipBuffer;
	tp.m_strTitle.Empty();
	tp.m_nIcon = 0;
	BOOL bToolTipTextValid = FALSE;

	CWnd* pControl;
	TOOLTIPTEXT* pToolTipText = (TOOLTIPTEXT*)pNMHDR;
	if (pToolTipText->uFlags & TTF_IDISHWND)
	{
		CWnd* pFrom = CWnd::FromHandle((HWND)pNMHDR->idFrom);
		if (pFrom)
			tp.m_nControlID = pFrom->GetDlgCtrlID();
		pControl = CWnd::FromHandlePermanent((HWND)pNMHDR->idFrom);
	}
	else
	{
		tp.m_nControlID = pNMHDR->idFrom;
		pControl = m_pOwnerWnd->GetDlgItem(tp.m_nControlID);
	}

	if (pControl)
	{
		ASSERT_VALID(pControl);

		CParsedCtrl* pParsedControl = GetParsedCtrl(pControl);
		bToolTipTextValid = pParsedControl && pParsedControl->GetToolTipProperties(tp);
		m_strToolTipBuffer = tp.m_strText;

		if (!bToolTipTextValid)
		{
			CWnd* pWnd = pControl;
			if (!pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
			{
				pWnd = pWnd->GetParent();
				if (pWnd && !pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
					pWnd = NULL;
			}

			if (pWnd)
			{
				CRect rect;	int nRowGrid = -1; int nIdxColGrid = -1;
				((CGridControl*)pWnd)->GetCoordCell(rect, nRowGrid, tp.m_nControlID, nIdxColGrid);
				bToolTipTextValid = nRowGrid < 0 && nIdxColGrid < 0 && ((CGridControl*)pWnd)->DoToolTipNotify(tp);

				if (bToolTipTextValid)
				{
					m_strToolTipBuffer = tp.m_strText;
					if (!rect.IsRectEmpty())
					{
						TOOLINFO ti;
						ptt->FillInToolInfo(ti, pControl, tp.m_ti.uId);
						memcpy(&ti.rect, rect, sizeof(RECT));
						ptt->SendMessage(TTM_NEWTOOLRECT, 0, (LPARAM)&ti);
					}

					if (!tp.m_strTitle.IsEmpty())
						ptt->SendMessage(TTM_SETTITLE, tp.m_nIcon, (LPARAM)(LPCTSTR)tp.m_strTitle);
				}
			}
		}
	}
	if (!bToolTipTextValid)
	{
		bToolTipTextValid = GetToolTipProperties(tp);
		m_strToolTipBuffer = tp.m_strText;
	}

	if (bToolTipTextValid)
	{
		ptt->SetMaxTipWidth(tp.m_nWidth);
		ptt->SetTipBkColor(tp.m_clrBackgnd);
		ptt->SetTipTextColor(tp.m_clrText);
		pToolTipText->hinst = NULL;
		pToolTipText->lpszText = m_strToolTipBuffer.GetBuffer(m_strToolTipBuffer.GetLength());
		ptt->SetTitle(tp.m_nIcon, tp.m_strTitle.GetBuffer(tp.m_strTitle.GetLength()));

		*pResult = 0L;
		return TRUE;
	}
	return FALSE;	// not handled
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::DoToolTipHide(UINT nId, NMHDR* pNMHDR, LRESULT* pResult)
{
	m_strToolTipBuffer.ReleaseBuffer();

	CToolTipCtrl* ptt = AfxGetModuleThreadState()->m_pToolTip;
	*pResult = 0L;
	if (ptt)
	{
		ptt->SetMaxTipWidth(m_nDefaultToolTipWidth);
		ptt->SetTipBkColor(m_clrDefaultToolTipBackgnd);
		ptt->SetTipTextColor(m_clrDefaultToolTipText);
		ptt->SetTitle(0, NULL);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
int CParsedForm::DoToolHitTest(CPoint pt, TOOLINFO* pTI) const
{
	int nHint = m_pOwnerWnd->CWnd::OnToolHitTest(pt, pTI);
	if (nHint == -1)
		return -1;

	if (pTI->uFlags & TTF_IDISHWND)
	{
		CWnd* pWnd = CWnd::FromHandlePermanent((HWND)pTI->uId);
		if (pWnd && !pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
		{
			pWnd = pWnd->GetParent();
			if (pWnd && !pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
				pWnd = NULL;
		}

		if (pWnd)
		{
			m_pOwnerWnd->ClientToScreen(&pt);
			pWnd->ScreenToClient(&pt);
			nHint = ((CGridControl*)pWnd)->DoToolHitTest(pt, pTI);
			if (nHint != -1)
				pTI->hwnd = m_pOwnerWnd->m_hWnd;
		}
	}
	return nHint;
}

//---------------------------------------------------------------------------
void CParsedForm::ShiftControl(UINT nTop, UINT nOffset)
{
	if (m_pOwnerWnd == NULL) return;

	CRect  rcChild;
	CWnd* pwndChild = m_pOwnerWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		if (pwndChild->m_hWnd != m_pOwnerWnd->m_hWnd)
		{
			pwndChild->GetWindowRect(rcChild);
			m_pOwnerWnd->GetParent()->ScreenToClient(rcChild);

			if ((UINT)abs(rcChild.top) >= nTop)
			{
				pwndChild->SetWindowPos(NULL, rcChild.left, rcChild.top + nOffset, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
			}
		}
		pwndChild = pwndChild->GetNextWindow();
	}
}

//---------------------------------------------------------------------------
// imposta il font di applicazione a tutti i control (anche statici e groupbox)
// poi i singoli control, se è il caso, si auto-imposteranno il font usato per
// i control. Infatti alcuni control (es.: enumerativi) usano lo stesso font delle
// form.
void CParsedForm::SetDefaultFont()
{
	if (m_pOwnerWnd == NULL)
		return;

	CFont* pF = m_pFont ? m_pFont : AfxGetThemeManager()->GetFormFont();
	m_pOwnerWnd->SetFont(pF);

	CWnd* pwndChild = m_pOwnerWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		if (pwndChild->m_hWnd != m_pOwnerWnd->m_hWnd)
		{
			if (::UseFormFont(pwndChild))
				pwndChild->SetFont(pF, TRUE);
		}
		pwndChild = pwndChild->GetNextWindow();
	}
}

//---------------------------------------------------------------------------
CWnd* CParsedForm::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	return ::GetWndLinkedCtrl(GetControlLinks(), aNS);
}

//---------------------------------------------------------------------------
void CParsedForm::HideControlGroup(UINT nGbxID, BOOL bHide/* = TRUE*/)
{
	if (m_pOwnerWnd == NULL)
		return;
	::HideControlGroup(m_pOwnerWnd, nGbxID, bHide);
}

//---------------------------------------------------------------------------
BOOL CParsedForm::HideControl(UINT nIDC, BOOL bHide/* = TRUE*/)
{
	if (m_pOwnerWnd == NULL)
		return FALSE;
	CWnd* pCtrl = m_pOwnerWnd->GetDlgItem(nIDC);
	return pCtrl ? pCtrl->ShowWindow(bHide ? SW_HIDE : SW_NORMAL) : FALSE;
}

//-----------------------------------------------------------------------------
int CParsedForm::SetZOrderInnerControls(CWnd*	pGbx, CWnd*	pParentWnd /*= NULL*/)
{
	ASSERT_VALID(pGbx);

	CArray<HWND> * parVirtualChilds = NULL;

	BOOL bParsedCtrl = pGbx->IsKindOf(RUNTIME_CLASS(CLabelStatic)) || pGbx->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn));
	if (bParsedCtrl)
	{
		parVirtualChilds = pGbx->IsKindOf(RUNTIME_CLASS(CLabelStatic)) ?
			&(((CLabelStatic*)pGbx)->m_arVirtualChilds)
			:
			&(((CGroupBoxBtn*)pGbx)->m_arVirtualChilds);
	}

	if (!pParentWnd)
		pParentWnd = pGbx->GetParent();
	else
		ASSERT(pParentWnd == pGbx->GetParent());

	CRect rectGbx;
	CRect rectCtrl;
	CRect rectInter;

	pGbx->GetWindowRect(&rectGbx);

	int numCtrl = 0;
	CWnd* pWnd1 = NULL;
	CWnd* pWndPrev = NULL;
	// scorre tutte le child window della dialog
	CWnd* pCtrl = pParentWnd->GetWindow(GW_CHILD);
	while (pCtrl)
	{
		// salta se stesso
		if (pCtrl->m_hWnd != pGbx->m_hWnd)
		{
			ASSERT(pCtrl->m_hWnd != pParentWnd->m_hWnd);

			pCtrl->GetWindowRect(&rectCtrl);
			// se il control interseca (anche solo parzialmente) la groupbox/static, viene mappato
			if
				(
					rectInter.IntersectRect(&rectGbx, &rectCtrl) &&
					!pCtrl->IsKindOf(RUNTIME_CLASS(CLabelStatic))
					)
			{
				ASSERT(pCtrl->m_hWnd != pParentWnd->m_hWnd);
				ASSERT(rectInter.Width() >= 0 || rectInter.Height() >= 0);

				numCtrl++;

				if (bParsedCtrl)
					m_Panel.SetAt(pCtrl->m_hWnd, pGbx);

				if (parVirtualChilds)
				{
					parVirtualChilds->Add(pCtrl->m_hWnd);
				}

				if (!pWnd1)
				{
					pWnd1 = pWndPrev;
				}
			}
			pWndPrev = pCtrl;
		}
		pCtrl = pCtrl->GetNextWindow();
	}
	if (pWnd1)
		pGbx->SetWindowPos(pWnd1, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

	return numCtrl;
}

//-----------------------------------------------------------------------------
COLORREF CParsedForm::GetBackgroundColor(HWND hCtrl/* = NULL*/) const
{
	CWnd* pGrp = NULL;
	if (!hCtrl || !m_Panel.GetCount() || !m_Panel.Lookup(hCtrl, pGrp))
		return m_clrBackground;

	ASSERT_VALID(pGrp);
	ASSERT(pGrp->m_hWnd);

	if (pGrp->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
	{
		CLabelStatic* pLS = (CLabelStatic*)pGrp;
		if (pLS->IsColored())
		{
			return pLS->GetBkgColor();
		}
	}
	else if (pGrp->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)))
	{
		CGroupBoxBtn* pGB = (CGroupBoxBtn*)pGrp;
		if (pGB->IsColored())
		{
			return pGB->GetBkgColor();
		}
	}
	return m_clrBackground;
}

//-----------------------------------------------------------------------------	
CBrush& CParsedForm::GetDlgBackBrush() const
{
	CBrush* pBrush = GetBackgroundBrush();

	if (pBrush)
		return *pBrush;

	return *AfxGetThemeManager()->GetBackgroundColorBrush();
}

//-----------------------------------------------------------------------------	
CBrush* CParsedForm::GetBackgroundBrush(HWND hCtrl/* = NULL*/) const
{
	CWnd* pGrp = NULL;
	if (!hCtrl || !m_Panel.GetCount() || !m_Panel.Lookup(hCtrl, pGrp))
	{
		//ASSERT(m_pBrushBackground);
		return m_pBrushBackground;
	}

	ASSERT_VALID(pGrp);
	ASSERT(pGrp->m_hWnd);

	if (pGrp->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
	{
		CLabelStatic* pLS = (CLabelStatic*)pGrp;
		if (pLS->IsColored())
		{
			return pLS->GetBkgBrush();
		}
	}
	else if (pGrp->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)))
	{
		CGroupBoxBtn* pGB = (CGroupBoxBtn*)pGrp;
		if (pGB->IsColored())
		{
			return pGB->GetBkgBrush();
		}
	}

	//ASSERT(m_pBrushBackground);
	return m_pBrushBackground;
}

//-----------------------------------------------------------------------------
HBRUSH CParsedForm::DoOnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	/*
	#ifdef _DEBUG
		{
			CString sText;
			pWnd->GetWindowText(sText);
			TRACE("%s ctlcode_%d id_%d ", pWnd->GetRuntimeClass()->m_lpszClassName, nCtlColor, pWnd->GetDlgCtrlID());
			TRACE(L"%s\n", (LPCTSTR)sText);
		}
	#endif
	*/
	CWnd* pGrp = NULL;

	//Altrimenti i control disabilitati e gli static non ricevono ON_WM_CTLCOLOR_REFLECT
	//necessario per la colorazione custom 
	LRESULT lResult;
	if (pWnd->SendChildNotifyLastMsg(&lResult))
	{
		if (
			(nCtlColor == CTLCOLOR_BTN) &&
			pWnd->m_hWnd &&
			pWnd->IsKindOf(RUNTIME_CLASS(CBoolButton)) &&
			!((CBoolButton*)pWnd)->IsColored() &&
			m_Panel.GetCount() && m_Panel.Lookup(pWnd->m_hWnd, pGrp)
			)
		{
			goto l_getcolorgrp;
		}

		return (HBRUSH)lResult;     // catched: eat it
	}
	//----

	if ((nCtlColor == CTLCOLOR_DLG) || (nCtlColor == CTLCOLOR_STATIC) || (nCtlColor == CTLCOLOR_BTN))
	{
		if (pWnd->m_hWnd && m_Panel.GetCount() && m_Panel.Lookup(pWnd->m_hWnd, pGrp))
		{
		l_getcolorgrp:
			ASSERT_VALID(pGrp);
			ASSERT(pGrp->m_hWnd);

			if (pGrp->IsKindOf(RUNTIME_CLASS(CLabelStatic)))
			{
				CLabelStatic* pLS = (CLabelStatic*)pGrp;
				if (pLS->IsColored())
				{
					pDC->SetBkColor(pLS->GetBkgColor());
					return pLS->GetBkgBrushHandle();
				}

			}
			else if (pGrp->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)))
			{
				CGroupBoxBtn* pGB = (CGroupBoxBtn*)pGrp;
				if (pGB->IsColored())
				{
					pDC->SetBkColor(pGB->GetBkgColor());
					return pGB->GetBkgBrushHandle();
				}
			}
		}

		pDC->SetBkColor(m_clrBackground);

		return m_pBrushBackground ? (HBRUSH)m_pBrushBackground->GetSafeHandle() : (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle();
	}

	return NULL;
}


//-----------------------------------------------------------------------------
void CParsedForm::PrepareDefaultExclusions(CObArray& arStatics)
{
	AddStaticIDC(IDC_STATIC);

	//Esclude in automatico gli static dalla gestione del VisualManager, in modo che se sono sull'area statica, 
	//non prendano lo sfondo di default
	CWnd* pChild = m_pOwnerWnd->GetWindow(GW_CHILD);
	while (pChild)
	{
		TCHAR szClassName[MAX_CLASS_NAME];
		GetClassName(pChild->m_hWnd, szClassName, MAX_CLASS_NAME);
		if (_tcsicmp(szClassName, _T("Static")) == 0)
		{
			int id = pChild->GetDlgCtrlID();
			arStatics.Add(pChild);
			if (id != IDC_STATIC)
			{
				AddStaticIDC(pChild->GetDlgCtrlID());
			}
		}
		else if (_tcsicmp(szClassName, _T("Button")) == 0)
		{
			if ((pChild->GetStyle() & BS_GROUPBOX) == BS_GROUPBOX)
			{
				int id = pChild->GetDlgCtrlID();
				arStatics.Add(pChild);
				if (id != IDC_STATIC)
				{
					AddStaticIDC(pChild->GetDlgCtrlID());
				}
			}
		}
		pChild = pChild->GetWindow(GW_HWNDNEXT);
	}
}
//-----------------------------------------------------------------------------
void CParsedForm::ApplyStaticSubclassing(CObArray& arStatics)
{
	if (!IsTransparent())
		return;

	ControlLinks* pControlLinks = GetControlLinks();
	for (int i = 0; i < arStatics.GetSize(); i++)
	{
		CWnd* pWnd = (CWnd*)arStatics.GetAt(i);
		if (pWnd->m_hWnd && (pWnd->GetRuntimeClass() == RUNTIME_CLASS(CWnd) || pWnd->GetRuntimeClass() == RUNTIME_CLASS(CStatic)))
		{
			HWND handle = pWnd->m_hWnd;
			pWnd->UnsubclassWindow();
			CLabelStatic* pStatic = new CLabelStatic();
			pStatic->SubclassWindow(handle);
			pStatic->SetCustomDraw(TRUE);
			pControlLinks->AddToStaticsMap(pStatic);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedForm::IsLayoutSuspended(BOOL bDelay /*= FALSE*/) const
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(m_pOwnerWnd->GetParentFrame());
	return pFrame && pFrame->IsLayoutSuspended(bDelay);

}

//---------------------------------------------------------------------------
CBaseTabManager* CParsedForm::AddBaseTabManager(CRuntimeClass* pClass)
{
	ASSERT_TRACE(pClass->IsDerivedFrom(RUNTIME_CLASS(CBaseTabManager)), _T("Runtime class parameter must be a CBaseTabManager!!!"));
	CBaseTabManager* pTabMng = (CBaseTabManager*)pClass->CreateObject();
	if (m_pLayoutContainer)
		m_pLayoutContainer->AddChildElement(pTabMng);

	return pTabMng;
}

//-----------------------------------------------------------------------------
CBaseTabManager* CParsedForm::AddBaseTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	CString strName = sName;
	strName.Trim();
	if (strName.IsEmpty())
	{
		TRACE1("Empty name not allowed %s\n", pClass->m_lpszClassName);
		ASSERT(FALSE);
		strName = pClass->m_lpszClassName;
	}
	CBaseTabManager* pTabManager = AddBaseTabManager(pClass);
	ASSERT_VALID(pTabManager);
	ASSERT_KINDOF(CBaseTabManager, pTabManager);

	pTabManager->AttachDocument(this->GetDocument());
	pTabManager->GetNamespace().SetChildNamespace(CTBNamespace::TABBER, strName, GetNamespace());
	pTabManager->GetInfoOSL()->m_pParent = GetInfoOSL();

	if (bCallOnInitialUpdate)
		pTabManager->OnInitialUpdate(nIDC, m_pOwnerWnd);

	return pTabManager;
}

//-----------------------------------------------------------------------------
CBaseTabManager* CParsedForm::AddBaseTileManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	return AddBaseTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
}

//---------------------------------------------------------------------------
CBaseTileGroup* CParsedForm::AddBaseTileGroup(
	UINT nIDC,
	CRuntimeClass* pClass,
	const CString& sName,
	BOOL bCallOnInitialUpdate /*TRUE*/,
	TileGroupInfoItem* pDlgInfoItem /*= NULL*/,
	CRect rectWnd /*= CRect(0, 0, 0, 0)*/)
{
	ASSERT_TRACE(pClass->IsDerivedFrom(RUNTIME_CLASS(CBaseTileGroup)), _T("Runtime class parameter must be a CBaseTileGroup!!!"));
	CBaseTileGroup* pTileGroup = (CBaseTileGroup*)pClass->CreateObject();
	pTileGroup->m_pDlgInfoItem = pDlgInfoItem;
	pTileGroup->GetInfoOSL()->SetType(OSLTypeObject::OSLType_TabDialog);

	if (pDlgInfoItem)
	{
		pTileGroup->GetNamespace().SetNamespace(((DlgInfoItem*)pTileGroup->m_pDlgInfoItem)->GetInfoOSL()->m_Namespace);
		pTileGroup->GetInfoOSL()->m_pParent = ((DlgInfoItem*)pTileGroup->m_pDlgInfoItem)->GetInfoOSL()->m_pParent;
	}
	else
	{
		pTileGroup->GetNamespace().SetChildNamespace(CTBNamespace::TABDLG, sName, GetNamespace());
		pTileGroup->GetInfoOSL()->m_pParent = GetInfoOSL();
	}

	pTileGroup->AttachDocument(this->GetDocument());
	if (bCallOnInitialUpdate)
		pTileGroup->OnInitialUpdate(nIDC, m_pOwnerWnd, rectWnd);

	if (m_pLayoutContainer)
		m_pLayoutContainer->AddChildElement(pTileGroup);

	return pTileGroup;
}

//---------------------------------------------------------------------------
CBaseFormView*	CParsedForm::GetBaseFormView(CRuntimeClass* pClass)
{
	if (!pClass)
		pClass = RUNTIME_CLASS(CBaseFormView);

	CWnd* pWnd = m_pOwnerWnd;
	do
	{
		CWnd* pParent = pWnd->GetParent();
		if (pParent && pParent->IsKindOf(pClass))
		{
			// per sicurezza controllo anche che sia una baseformview
			CBaseFormView* pView = dynamic_cast<CBaseFormView*>(pParent);
			if (pView)
				return pView;
		}

		pWnd = pParent;
	} while (pWnd != NULL);

	return NULL;
}

//---------------------------------------------------------------------------
CParsedDialog*	CParsedForm::GetAncestorDialog()
{
	CParsedDialog* pAncestor = NULL;
	CWnd* pWnd = m_pOwnerWnd;
	do
	{
		CWnd* pParent = pWnd->GetParent();
		if (pParent && pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
			pAncestor = (CParsedDialog*)pParent;
		pWnd = pParent;
	} while (pWnd != NULL);

	return pAncestor;
}

struct WndInfo { CWnd* m_pWnd; UINT m_nIDC; };

//---------------------------------------------------------------------------
void CParsedForm::NotifyLayoutChanged()
{

	CBaseFormView* pView = dynamic_cast<CBaseFormView*>(m_pOwnerWnd);
	if (!pView)
		pView = GetBaseFormView(RUNTIME_CLASS(CBaseFormView));
	if (pView)
		pView->SendMessage(UM_LAYOUT_CHANGED, (WPARAM)m_pOwnerWnd);
}

//-----------------------------------------------------------------------------
BOOL CALLBACK EnumControlsProc(HWND hwnd, LPARAM lParam)
{
	CWnd* pWnd = CWnd::FromHandlePermanent(hwnd);
	if (!pWnd || pWnd->GetDlgCtrlID() != ((WndInfo*)lParam)->m_nIDC)
		return TRUE;

	((WndInfo*)lParam)->m_pWnd = pWnd;

	return FALSE;
}

//--------------------------------------------------------------------------
/*static*/ CWnd* CParsedForm::GetChildCtrlWndFromID(CWnd* pParent, UINT nID)
{
	ASSERT_VALID(pParent);

	WndInfo aChild;
	aChild.m_pWnd = NULL;
	aChild.m_nIDC = nID;
	EnumChildWindows(pParent->m_hWnd, EnumControlsProc, (LPARAM)&aChild);
	return aChild.m_pWnd;
}

//--------------------------------------------------------------------------
CWnd* CParsedForm::GetWndFromIDC(const DWORD nIDC)
{
	CWnd* pChild = GetChildCtrlWndFromID(m_pOwnerWnd, nIDC);

	return pChild;
}

//--------------------------------------------------------------------------
CParsedCtrl* CParsedForm::GetLinkedParsedCtrl(const CTBNamespace& aNS)
{
	ASSERT_VALID(m_pOwnerWnd);

	CWnd* pwndChild = m_pOwnerWnd->GetWindow(GW_CHILD);

	while (pwndChild)
	{
		CParsedCtrl* pCtrl = GetParsedCtrl(pwndChild);
		if (pCtrl && pCtrl->GetNamespace() == aNS)
			return pCtrl;

		pwndChild = pwndChild->GetNextWindow();
	}

	return NULL;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CParsedForm::GetWndLinkedCtrl(UINT nIDC)
{
	CWnd* pWnd = ::GetWndLinkedCtrl(m_pControlLinks, nIDC);
	if (pWnd)
	{
		return pWnd;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CParsedForm::SetChildControlNamespace(const CString& sName, CParsedCtrl* pCtrl)
{
	if (pCtrl)
	{
		pCtrl->GetInfoOSL()->m_pParent = GetInfoOSL();
		pCtrl->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetNamespace());

		if (GetDocument() &&
			(
			(GetDocument()->GetFormMode() == CBaseDocument::NEW && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_NEW) == 0) ||
				(GetDocument()->GetFormMode() == CBaseDocument::EDIT && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EDIT) == 0)
				)
			)
			pCtrl->SetDataOSLReadOnly(TRUE);

		if (OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EXECUTE) == 0)
		{
			pCtrl->SetDataOSLReadOnly(TRUE);
			pCtrl->SetDataOSLHide(TRUE);
		}
	}
}

//--------------------------------------------------------------------------
LRESULT CParsedForm::DoGetCWnd(WPARAM nID, LPARAM)
{
	CWnd* pwndChild = GetChildCtrlWndFromID(m_pOwnerWnd, nID);
	if (!pwndChild)
		return NULL;

	return (LRESULT)pwndChild;
}

//--------------------------------------------------------------------------
LRESULT CParsedForm::DoGetParsedCtrl(WPARAM nID, LPARAM)
{
	CWnd* pwndChild = GetChildCtrlWndFromID(m_pOwnerWnd, nID);
	if (!pwndChild)
		return NULL;

	CParsedCtrl* pCtrl = ::GetParsedCtrl(pwndChild);
	if (!pCtrl)
		return NULL;

	return (LRESULT)pCtrl;
}

//--------------------------------------------------------------------------
LRESULT CParsedForm::DoGetLinkedParsedCtrl(WPARAM nID, LPARAM nsParam)
{
	CTBNamespace* aNs = (CTBNamespace*)nsParam;
	CParsedCtrl* pCtrl = GetLinkedParsedCtrl(CTBNamespace(aNs->ToString()));
	return (LRESULT)pCtrl;
}

//-----------------------------------------------------------------------------
bool CParsedForm::SetDefaultFocus()
{
	ASSERT_VALID(m_pOwnerWnd);

	if (!m_pControlLinks || m_pControlLinks->GetSize() == 0 || !m_pControlLinks->HasFocusableControl(m_pOwnerWnd))
		return false;

	m_pControlLinks->SetDefaultFocus(m_pOwnerWnd, m_phLastCtrlFocused);

	return true;
}

//--------------------------------------------------------------------------
BOOL CParsedForm::IsCtrlBefore(int nBefore, int nAfter)
{
	ASSERT_VALID(m_pOwnerWnd);

	if (nBefore == nAfter)
		return TRUE;

	CWnd* pCtrl = m_pOwnerWnd->GetWindow(GW_CHILD);
	while (pCtrl)
	{
		int nID = pCtrl->GetDlgCtrlID();
		// actually, the TABSTOP style cannot be checked on all the radiobuttons of a group
		// The Microsoft documentation suggests that "If the group contains radio buttons, the application 
		// should apply the WS_TABSTOP style only to the first control in the group"
		// Apparently, this style is removed from the radiobuttons after the first one of a group
		if (
			nID != IDC_STATIC &&
			(
			(pCtrl->GetStyle() & WS_TABSTOP) == WS_TABSTOP ||
				(pCtrl->GetStyle() & BS_AUTORADIOBUTTON) == BS_AUTORADIOBUTTON
				)
			)
		{
			if (nID == nBefore)
				return TRUE;
			if (nID == nAfter)
				return FALSE;
		}

		pCtrl = pCtrl->GetNextWindow();
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CALLBACK EnumChildrenProc(HWND hwnd, LPARAM lParam)
{
	CWnd* pWnd = CWnd::FromHandlePermanent(hwnd);
	if (
		!pWnd ||
		!pWnd->IsWindowEnabled() ||
		(pWnd->GetStyle() & WS_VISIBLE) != WS_VISIBLE ||
		(pWnd->GetStyle() & WS_TABSTOP) != WS_TABSTOP ||
		pWnd->IsKindOf(RUNTIME_CLASS(CStatic)) ||
		(
			dynamic_cast<CGridControlObj*>(pWnd) == NULL &&
			dynamic_cast<CParsedCtrl*>(pWnd) == NULL
			)
		)
		return TRUE;

	pWnd = pWnd->GetParent();
	if (pWnd && !pWnd->IsWindowVisible())
		return TRUE;

	CBaseTileDialog* pTileDlg = dynamic_cast<CBaseTileDialog*>(pWnd);
	if (pTileDlg && !pTileDlg->IsDisplayed())
		return TRUE;

	*((HWND*)lParam) = hwnd;

	return FALSE;
}

//-----------------------------------------------------------------------------
bool CParsedForm::SetTBFocus(CWnd* pWnd, BOOL bBackward)
{
	ASSERT_VALID(pWnd);
	if (!pWnd)
		return false;

	HWND hFocusableChild = NULL;

	EnumChildWindows(pWnd->m_hWnd, EnumChildrenProc, (LPARAM)&hFocusableChild);
	if (hFocusableChild == NULL)
	{
		if (
			!pWnd->IsWindowEnabled() ||
			(pWnd->GetStyle() & WS_VISIBLE) != WS_VISIBLE ||
			(pWnd->GetStyle() & WS_TABSTOP) != WS_TABSTOP
			)
			return false;

		pWnd->SetFocus();
		return true;
	}

	CWnd* pFocusableChild = CWnd::FromHandlePermanent(hFocusableChild);
	if (bBackward)
	{
		CWnd* pParentWnd = pFocusableChild->GetParent();
		pFocusableChild = pParentWnd->GetNextDlgTabItem(pFocusableChild, TRUE);
		if (!pFocusableChild)
			return false;

		int nFirstCandidateID = pFocusableChild->GetDlgCtrlID();
		int nCurrentID = -1;
		while (pFocusableChild != NULL && nFirstCandidateID != nCurrentID)
		{
			if (pFocusableChild && pFocusableChild->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
			{
				CBaseTileGroup* pBTG = (CBaseTileGroup*)pFocusableChild;
				if (pBTG->SetNextControlFocus(NULL, TRUE, NULL))
					return true;
				else
					return false;
			}

			if (
				pFocusableChild->IsWindowEnabled() &&
				(pFocusableChild->GetStyle() & WS_VISIBLE) == WS_VISIBLE &&
				(pFocusableChild->GetStyle() & WS_TABSTOP) == WS_TABSTOP &&
				!pFocusableChild->IsKindOf(RUNTIME_CLASS(CStatic)) &&
				(
					dynamic_cast<CGridControlObj*>(pFocusableChild) ||
					dynamic_cast<CParsedCtrl*>(pFocusableChild)
					)
				)
				break;

			pFocusableChild = pParentWnd->GetNextDlgTabItem(pFocusableChild, TRUE);
			nCurrentID = pFocusableChild->GetDlgCtrlID();
		}
	}

	if (pFocusableChild)
	{
		pFocusableChild->SetFocus();
		return true;
	}

	return false;
}

//===========================================================================
// CParsedDialog dialog
//===========================================================================
IMPLEMENT_DYNAMIC(CParsedDialog, CLocalizableDialog)

BEGIN_MESSAGE_MAP(CParsedDialog, CLocalizableDialog)

	ON_MESSAGE(UM_BAD_VALUE, OnBadValue)
	ON_MESSAGE(UM_LOSING_FOCUS, OnLosingFocus)
	ON_MESSAGE(UM_CTRL_FOCUSED, OnCtrlFocused)
	ON_MESSAGE(UM_VALUE_CHANGED, OnValueChanged)

	ON_MESSAGE(WM_GETFONT, OnGetFont)
	ON_MESSAGE(UM_GET_DIALOG_ID, OnGetDialogId)
	ON_MESSAGE(UM_GET_PARSED_CTRL, OnGetParsedCtrl)
	ON_MESSAGE(UM_GET_PARSED_CTRL_NS, OnGetLinkedParsedCtrl)
	ON_MESSAGE(UM_GET_CWND, OnGetCWnd)
	ON_MESSAGE(UM_FORMAT_STYLE_CHANGED, OnFormatStyleChange)

	ON_MESSAGE(WM_KICKIDLE, OnKickIdle)

	ON_WM_SHOWWINDOW()
	ON_WM_ACTIVATE()
	ON_WM_SETFOCUS()

	ON_WM_SIZE()
	ON_WM_WINDOWPOSCHANGED()
	ON_WM_HSCROLL()
	ON_WM_VSCROLL()
	ON_WM_MOUSEWHEEL()

	ON_WM_NCPAINT()

	ON_WM_ERASEBKGND()
	ON_WM_CTLCOLOR()
	//----
	ON_NOTIFY(TBN_DROPDOWN, AFX_IDW_TOOLBAR, ToolbarDropDown)

	ON_NOTIFY_EX(TTN_NEEDTEXT, 0, OnToolTipNotify)
	ON_NOTIFY_EX(TTN_POP, 0, OnToolTipHide)

END_MESSAGE_MAP()

//--------------------------------------------------------------------------
void CParsedDialog::OnNcPaint()
{
	__super::OnNcPaint();
}

//--------------------------------------------------------------------------
void CParsedDialog::OnShowWindow(BOOL bShow, UINT nStatus)
{
	__super::OnShowWindow(bShow, nStatus);

	if (m_pOwner && bShow && !nStatus && !IsVisualManagerStyle())
	{
		ApplyTBVisualManager();
		if (!GetJsonContext())
			SetDefaultFont();
	}
}

#pragma warning(disable:4355) // disabilita la warning sull'uso del this del parent
//---------------------------------------------------------------------------
CParsedDialog::CParsedDialog(UINT nIdd, CWnd* pWndParent, const CString& sName)
	:
	CSplittedForm(this),
	CLocalizableDialog((UINT)nIdd, pWndParent),
	CParsedForm(this, nIdd, sName),
	IDisposingSourceImpl(this),
	m_hWndFocusDlg(0),
	m_NotValidCtrlID(0),
	m_ErrorMode(0),
	m_ControllingMode(CExternalControllerInfo::NONE),
	m_pToolBar(NULL),
	m_pTabbedToolBar(NULL),
	m_bHasDefaultOkCancelBar(FALSE),
	m_nToolbarHeight(0),
	m_hAccelTable(NULL),
	m_pParentWnd(pWndParent),
	//gestione scrolling
	m_bEnableHScrolling(FALSE),
	m_bEnableVScrolling(FALSE),
	m_nScrollStepX(0),
	m_nScrollStepY(0),
	m_nPageSizeY(0),
	m_nScrollMaxY(0),
	m_nPageSizeX(0),
	m_nScrollMaxX(0),
	m_nScrollPosX(0),
	m_nScrollPosY(0),
	m_nHeight(-1),
	m_nWidth(-1)
	//----
{
	// eventualmente vengono riscritti dalla OnInitDialog se questa è figlia di una view
	m_pAncestorWnd = this;
	m_phLastCtrlFocused = &m_hWndFocusDlg;
	m_pNotValidCtrlID = &m_NotValidCtrlID;
	m_pErrorMode = &m_ErrorMode;
}

//---------------------------------------------------------------------------
CParsedDialog::CParsedDialog()
	:
	CSplittedForm(this),
	CLocalizableDialog(),
	CParsedForm(this, 0, _T("")),
	IDisposingSourceImpl(this),
	m_hWndFocusDlg(0),
	m_NotValidCtrlID(0),
	m_ErrorMode(0),
	m_ControllingMode(CExternalControllerInfo::NONE),
	m_pToolBar(NULL),
	m_pTabbedToolBar(NULL),
	m_bHasDefaultOkCancelBar(FALSE),
	m_nToolbarHeight(0),
	m_hAccelTable(NULL),
	m_pParentWnd(NULL),
	//gestione scrolling
	m_bEnableHScrolling(FALSE),
	m_bEnableVScrolling(FALSE),
	m_nScrollStepX(0),
	m_nScrollStepY(0),
	m_nPageSizeY(0),
	m_nScrollMaxY(0),
	m_nPageSizeX(0),
	m_nScrollMaxX(0),
	m_nScrollPosX(0),
	m_nScrollPosY(0),
	m_nHeight(-1),
	m_nWidth(-1)
{
	// eventualmente vengono riscritti dalla OnInitDialog se questa è figlia di una view
	m_pAncestorWnd = this;
	m_phLastCtrlFocused = &m_hWndFocusDlg;
	m_pNotValidCtrlID = &m_NotValidCtrlID;
	m_pErrorMode = &m_ErrorMode;
}

#pragma warning(default:4355)

//--------------------------------------------------------------------------
CParsedDialog::~CParsedDialog()
{
	__super::SetBackgroundColor(-1, FALSE);

	DestroyToolbar();
	if (m_pTabbedToolBar)
	{
		SAFE_DELETE(m_pTabbedToolBar);
	}

	if (m_hAccelTable)
		DestroyAcceleratorTable(m_hAccelTable);

	//gdileak in CBCGPDialog, tappullo, segnaleremo l'anomalia a chi di dovere
}

//--------------------------------------------------------------------------
CString  CParsedDialog::GetRanorexNamespace()
{
	return cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
}

//--------------------------------------------------------------------------
void CParsedDialog::DestroyToolbar()
{
	if (m_pToolBar)
	{
		delete m_pToolBar;
		m_pToolBar = NULL;
	}
}
//--------------------------------------------------------------------------
void CParsedDialog::OnDestroy()
{
	DestroyToolbar();
	if (m_pTabbedToolBar)
		SAFE_DELETE(m_pTabbedToolBar);

	__super::OnDestroy();
}

//-----------------------------------------------------------------------------
BOOL CParsedDialog::OnEraseBkgnd(CDC* pDC)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	//if (pFrame && pFrame->IsLayoutSuspended())
	if (pFrame && pFrame->IsLayoutSuspended())
	{
		CWnd* pCtrl = this->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			if ((pCtrl->GetStyle() & BS_GROUPBOX) == BS_GROUPBOX)
				continue;

			if (!pCtrl->IsWindowVisible())
				continue;

			CRect screen;
			pCtrl->GetWindowRect(&screen);
			this->ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}
	}

	CRect rclientRect;
	this->GetClientRect(rclientRect);
	pDC->FillRect(&rclientRect, &GetDlgBackBrush());

	return TRUE;
}

//-----------------------------------------------------------------------------
HBRUSH CParsedDialog::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	HBRUSH lResult = DoOnCtlColor(pDC, pWnd, nCtlColor);
	if (lResult)
		return lResult;     // catched: eat it

	return __super::OnCtlColor(pDC, pWnd, nCtlColor);
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnGetDialogId(WPARAM w, LPARAM)
{
	ASSERT_VALID(this);
	return GetDialogID();
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnGetCWnd(WPARAM nID, LPARAM l)
{
	return DoGetCWnd(nID, l);
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnGetParsedCtrl(WPARAM nID, LPARAM l)
{
	return DoGetParsedCtrl(nID, l);
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnGetLinkedParsedCtrl(WPARAM nID, LPARAM nsParam)
{
	return DoGetLinkedParsedCtrl(nID, nsParam);
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnBadValue(WPARAM nIdc, LPARAM lMode)
{
	return CParsedForm::DoBadValue((UINT)nIdc, LOWORD(lMode));
}

//--------------------------------------------------------------------------
LRESULT CParsedDialog::OnLosingFocus(WPARAM nIdc, LPARAM lParam)
{
	CParsedForm::DoLosingFocus((UINT)nIdc, (int)lParam);

	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT	CParsedDialog::OnCtrlFocused(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_CTRL_FOCUSED, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
LRESULT CParsedDialog::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
LRESULT CParsedDialog::OnGetFont(WPARAM wParam, LPARAM lParam)
{
	HFONT hFont = (HFONT)AfxGetThemeManager()->GetFormFont()->GetSafeHandle();
	if (hFont)
		return (LRESULT)(UINT)hFont;

	return DefWindowProc(WM_GETFONT, wParam, lParam);
}

//-----------------------------------------------------------------------------
void CParsedDialog::OnActivate(UINT nState, CWnd* pWnd, BOOL bFlag)
{
	CLocalizableDialog::OnActivate(nState, pWnd, bFlag);

	DoActivate(nState == WA_ACTIVE || nState == WA_CLICKACTIVE);

	//messaggio al macrorecoder di attivazione;
	int nID = GetDialogID();
	SendActivateToMacroRecorder(this, nID);
}

//-----------------------------------------------------------------------------
void CParsedDialog::OnSetFocus(CWnd* pOldWnd)
{
	CLocalizableDialog::OnSetFocus(pOldWnd);
	DoActivate(TRUE);
}

//-----------------------------------------------------------------------------
LRESULT CParsedDialog::OnFormatStyleChange(WPARAM wParam, LPARAM lParam)
{
	CParsedForm::DoFormatStyleChange(wParam, lParam);
	return 0L;
}

//------------------------------------------------------------------------------
LRESULT CParsedDialog::OnKickIdle(WPARAM w, LPARAM l)
{
	if (m_pToolBar)
	{
		m_pToolBar->OnUpdateCmdUIDialog();
	}

	return	FALSE;
}

// modeless dialog
//------------------------------------------------------------------------------
BOOL CParsedDialog::Create(UINT nIDTemplate, CWnd* pParentWnd, const CString& strName)
{
	ASSERT(m_nID == 0 || m_nID == -1 || m_nID == nIDTemplate);

	m_nID = nIDTemplate;
	m_sName = strName;
	m_lpszTemplateName = MAKEINTRESOURCE(m_nID);
	m_pParentWnd = pParentWnd;
	BOOL bOk = CLocalizableDialog::Create(nIDTemplate, pParentWnd);
	if (bOk)
		CreateSplitterEnvironment(pParentWnd);
	return bOk;

}

// modal dialog
//------------------------------------------------------------------------------
int CParsedDialog::DoModal()
{
	m_lpszTemplateName = MAKEINTRESOURCE(m_nID);
	return CLocalizableDialog::DoModal();
}

//--------------------------------------------------------------------------
void CParsedDialog::OnUpdateControls(BOOL bParentIsVisible)
{
	::OnUpdateControls(m_pControlLinks, bParentIsVisible);
}
//------------------------------------------------------------------------------
void CParsedDialog::OnFindHotLinks()
{
	::OnFindHotLinks(m_pControlLinks);
}
//------------------------------------------------------------------------------
void CParsedDialog::OnResetDataObjs()
{
	::OnResetDataObjs(m_pControlLinks);
}
//--------------------------------------------------------------------------
BOOL CParsedDialog::DoModeless()
{
	if (Create(m_nID))
	{
		m_bAutoDestroy = TRUE; //si autodistruggerà quando l'utente la chiude
		ShowWindow(SW_SHOW);
		return TRUE;
	}
	else
	{
		ASSERT(FALSE);
		delete this;
		return FALSE;
	}
}
//--------------------------------------------------------------------------
void CParsedDialog::BuildJsonControlLinks()
{
	CJsonContextObj* pJsonContext = GetJsonContext();
	if (pJsonContext)
		pJsonContext->BuildDataControlLinks();
}

//--------------------------------------------------------------------------
BOOL CParsedDialog::OnInitDialog()
{
	// Se è figlia di una view uso i datamember della view ancestor
	// Il costruttore mette come default this
	CBaseFormView* pBaseFV = GetBaseFormView(NULL);
	if (pBaseFV)
	{
		m_pAncestorWnd = pBaseFV;
		m_phLastCtrlFocused = pBaseFV->m_phLastCtrlFocused;
		m_pNotValidCtrlID = &pBaseFV->m_NotValidCtrlID;
		m_pErrorMode = &pBaseFV->m_ErrorMode;
	}
	else
		if ((GetStyle() & WS_POPUP) != WS_POPUP)
		{
			// la dialog potrebbe essere a sua volta figlia di una
			// parsed dialog progenitrice (vd popup e docking panes)
			CParsedDialog* pAncestorDialog = GetAncestorDialog();
			if (pAncestorDialog)
			{
				m_pAncestorWnd = pAncestorDialog;
				m_phLastCtrlFocused = &pAncestorDialog->m_hWndFocusDlg;
				m_pNotValidCtrlID = &pAncestorDialog->m_NotValidCtrlID;
				m_pErrorMode = &pAncestorDialog->m_ErrorMode;
			}
		}

	AddOnDll* pAddOn = AfxGetAddOnDll(GetDllInstance(GetRuntimeClass()));
	m_pOwner = pAddOn ? pAddOn->GetNamespace() : NULL;

	if (!m_sName.IsEmpty() && dynamic_cast<ITabDialogTileGroup*>(this) == NULL)
	{
		//il nome è in forma di namespace: uso quello
		if (m_sName.Find(CTBNamespace::GetSeparator()) > 0)
		{
			GetInfoOSL()->m_Namespace.AutoCompleteNamespace(CTBNamespace::FORM, m_sName, GetInfoOSL()->m_Namespace);
		}
		else if (m_pOwner)//altrimenti lo completo usando il contesto
		{
			if (GetDocument() && !IsKindOf(RUNTIME_CLASS(CBaseTabDialog)) && !IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
			{
				CWnd* pParent = GetParent();
				IOSLObjectManager* pOSLParent = dynamic_cast<IOSLObjectManager*>(pParent);
				CTBNamespace namesp;
				if (pOSLParent)
					namesp = pOSLParent->GetInfoOSL()->m_Namespace;
				else
					namesp = GetDocument()->GetNamespace();

				GetInfoOSL()->m_Namespace.AutoCompleteNamespace(CTBNamespace::FORM, m_sName, namesp);
				GetInfoOSL()->m_pParent = GetDocument()->GetInfoOSL();
				GetInfoOSL()->SetType(OSLTypeObject::OSLType_SlaveTemplate);
			}
			else
			{
				GetInfoOSL()->m_Namespace.SetType(CTBNamespace::FORM);
				GetInfoOSL()->m_Namespace.SetApplicationName(m_pOwner->GetApplicationName());
				GetInfoOSL()->m_Namespace.SetObjectName(CTBNamespace::MODULE, m_pOwner->GetModuleName());
				GetInfoOSL()->m_Namespace.SetObjectName(CTBNamespace::DOCUMENT, _T("Dialog"));
				GetInfoOSL()->m_Namespace.SetObjectName(CTBNamespace::TABDLG, m_sName);
			}
		}
	}

	BOOL bRet = CLocalizableDialog::OnInitDialog();
	CJsonContextObj* pJsonContext = GetJsonContext();
	if (m_pOwner && !pJsonContext)//se sono json, il fonto letto dai settings è già impostato
		SetDefaultFont();

	if (OnCreateToolbar())
	{
		RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	}

	// Set background in dialog
	if (IsTransparent())
	{
		m_brBkgr.DeleteObject();
		m_brBkgr.FromHandle((HBRUSH)AfxGetThemeManager()->GetTransparentColorBrush()->m_hObject);
	}
	else
	{
		CParsedForm::SetBackgroundColor(m_clrBackground);
	}

	BuildJsonControlLinks();

	//Tentativo di rimuovere il problema delle popup window
	if ((GetStyle() & WS_POPUP) == WS_POPUP)
	{
		/*ModifyStyle(WS_SYSMENU, 0);

		if (AfxGetThemeManager()->UseFlatStyle())
			ModifyStyleEx(WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE, 0);*/
	}

	return bRet;
}

//------------------------------------------------------------------------------
void CParsedDialog::RepositionBars2()
{
	CRect rcClientStart;
	GetClientRect(rcClientStart);

	// We need to resize the dialog to make room for control bars.
	// First, figure out how big the control bars are.
	CRect rcClientNow;
	RepositionBars
	(
		AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST,
		0, reposQuery, rcClientNow
	);

	// Now move all the controls so they are in the same relative
	// position within the remaining client area as they would be
	// with no control bars.
	CPoint ptOffset(rcClientNow.left - rcClientStart.left,
		rcClientNow.top - rcClientStart.top - 10);

	CRect  rcChild;
	CWnd* pwndChild = GetWindow(GW_CHILD);
	while (pwndChild)
	{
		pwndChild->GetWindowRect(rcChild);
		ScreenToClient(rcChild);
		rcChild.OffsetRect(ptOffset);
		pwndChild->MoveWindow(rcChild, FALSE);
		pwndChild = pwndChild->GetNextWindow();
	}

	// Adjust the dialog window dimensions
	CRect rcWindow;
	GetWindowRect(rcWindow);
	rcWindow.right += rcClientStart.Width() - rcClientNow.Width();
	rcWindow.bottom += rcClientStart.Height() - rcClientNow.Height();
	MoveWindow(rcWindow, FALSE);

	// And position the control bars
	RepositionBars(AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
}

//------------------------------------------------------------------------------------
void CParsedDialog::ToolbarDropDown(NMHDR* pnmh, LRESULT *plr)
{
	if (m_pToolBar == NULL) return;

	NMTOOLBAR* pnmtb = (NMTOOLBAR*)pnmh;

	CMenu menu;
	menu.CreatePopupMenu();

	if (!OnToolbarDropDown(pnmtb->iItem, menu))
		return;

	CRect rc;
	m_pToolBar->SendMessage(TB_GETRECT, pnmtb->iItem, (LPARAM)&rc);
	m_pToolBar->ClientToScreen(&rc);

	menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
		rc.left, rc.bottom, this, &rc);
}

// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL CParsedDialog::OnCommand(WPARAM wParam, LPARAM lParam)
{
	BOOL bNotFarwardToParent = CParsedForm::DoCommand(wParam, lParam);

	DoCommandMacroRecorder(wParam, lParam);

	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	if (!bNotFarwardToParent)
	{
		CWnd* pParent = GetParent();
		if (pParent)
			return pParent->SendMessage(WM_COMMAND, wParam, lParam);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CParsedDialog::DoCommandMacroRecorder(WPARAM wParam, LPARAM lParam)
{
	//se non stiamo registrando esce subito

	if (AfxGetApplicationContext()->m_MacroRecorderStatus != CApplicationContext::RECORDING)
		return;

	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	//cerco l'ID tra i control della finestra corrente, e verifico che la parent non sia una tabdialog
	CWnd* pwndChild = GetChildCtrlWndFromID(m_pOwnerWnd, nID);
	if (!pwndChild || !pwndChild->GetParent())
		return;

	//nel caso di Checklistbox dentro wizard e tab
	//le checklistbox vanno gestite anche se sono dentro tab, difficilmente sarnno in una tab di mago
	//ma probabilmente saranno gestite dentro i wizard
	if (!pwndChild->IsKindOf(RUNTIME_CLASS(CCheckListBox)))
	{
		if (pwndChild->GetParent()->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
			return;
	}

	//in questo caso strValue contiene il valore del controllo, o il numero riga della listbox
	//strText contiene o la descrizione del controllo o il booleano associato
	CString strText = NULL;
	CString strValue = NULL;
	int nBooleanValue = 2;  //undefined
	TCHAR szBuffer[20];

	CString strNameSpace = NULL;
	CParsedCtrl* pCtrl = GetParsedCtrl(pwndChild);
	if (pCtrl && !pCtrl->GetNamespace().IsEmpty())
		strNameSpace = pCtrl->GetNamespace().ToString();

	switch (nCode)
	{
	case LBN_KILLFOCUS:
		//se la parent è una base tab dialog, la CCheckListBox potrebbe essere invece che in una 
		//ParsedDialog in una tab del wizard
		if (pwndChild->IsKindOf(RUNTIME_CLASS(CCheckListBox)))
		{
			int nCurrSel = ((CListBox*)pwndChild)->GetCurSel();
			//qua dovrei prendere il testo della listbox
			if (nCurrSel >= 0)
			{
				((CListBox*)pwndChild)->GetText(nCurrSel, strText);

				//booleano della checkbox
				nBooleanValue = ((CCheckListBox*)pwndChild)->GetCheck(nCurrSel);

				//numero riga
				_itot_s(nCurrSel, szBuffer, 20, 10);
				strValue = szBuffer;

				//nel caso CCheckListBox mi serve numero riga, booleano
				SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
			}
		}
		else if (pwndChild->IsKindOf(RUNTIME_CLASS(CListBox)))
		{
			//qua dovrei prendere il testo della listbox
			int nCurrSel = ((CListBox*)pwndChild)->GetCurSel();
			if (nCurrSel >= 0)
			{
				((CListBox*)pwndChild)->GetText(nCurrSel, strText);

				//numero riga
				_itot_s(nCurrSel, szBuffer, 20, 10);
				strValue = szBuffer;

				//nel caso CCheckListBox mi serve solo il numero riga
				SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
			}
		}
		break;

	case CBN_KILLFOCUS:
		if (pwndChild->IsKindOf(RUNTIME_CLASS(CComboBox)))
		{
			((CComboBox*)pwndChild)->GetWindowText(strValue);
			SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
		}
		break;

	case LBN_DBLCLK:
		SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
		break;

	case CLBN_CHKCHANGE:
		if (pwndChild->IsKindOf(RUNTIME_CLASS(CCheckListBox)))
		{
			int nCurrSel = ((CListBox*)pwndChild)->GetCurSel();
			if (nCurrSel >= 0)
			{
				//booleano della checkbox
				nBooleanValue = ((CCheckListBox*)pwndChild)->GetCheck(nCurrSel);

				//numero riga
				_itot_s(nCurrSel, szBuffer, 20, 10);
				strValue = szBuffer;

				//testo dell'elemento selezionato
				((CCheckListBox*)pwndChild)->GetText(nCurrSel, strText);

				SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
			}
		}
		break;
	case EN_KILLFOCUS:
		if (pCtrl && pCtrl->GetCtrlData())
			strValue = pCtrl->GetCtrlData()->FormatDataForXML();
		else
			pwndChild->GetWindowText(strValue);

		SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
		break;

	case BN_CLICKED:
		if (pwndChild->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		{
			//booleano associato al boolbutton
			nBooleanValue = ((CBoolButton*)pwndChild)->GetCheck();

			SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
		}
		else
		{
			((CPushButton*)pwndChild)->GetWindowText(strText);
			//nel caso di CMessageDialog non dovrebbe registrare niente
			SendCommandToMacroRecorder(nID, nCode, strValue, strText, nBooleanValue, strNameSpace);
		}
	default:
		break;
	}
}

//-----------------------------------------------------------------------------
void CParsedDialog::SendCommandToMacroRecorder(int nID, int nCode, CString strValue, CString strText, int nBooleanValue, CString strNameSpace)
{
	CFunctionDescription fd;
	if (
		AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
		AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordParsedAction"), fd)
		)
	{
		//strValue, nID, nCode, strText
		fd.SetParamValue(_T("strValue"), DataStr(strValue));
		fd.SetParamValue(_T("nID"), DataInt((int)nID));
		fd.SetParamValue(_T("nCode"), DataInt((int)nCode));
		fd.SetParamValue(_T("strText"), DataStr(strText));
		fd.SetParamValue(_T("nBooleanValue"), DataInt((int)nBooleanValue));
		fd.SetParamValue(_T("strNameSpace"), DataStr(strNameSpace));

		AfxGetTbCmdManager()->RunFunction(&fd, 0);
	}
}

//-----------------------------------------------------------------------------
void CParsedDialog::SendActivateToMacroRecorder(CWnd* pWnd, int nID)
{
	if (AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::IDLE)
		return;

	//ignora messaggi provenienti da tabdialog
	if (!pWnd || !pWnd->m_hWnd || pWnd->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
		return;

	//al macro recorder potrebbe valer la pena di passare l'handle della finestra, da ignorare in recording
	//ma da usare in play per valorizzare il puntatore tabdialog
	CFunctionDescription fd;
	if (
		AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
		AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordActivateParsedDialog"), fd)
		)
	{
		fd.SetParamValue(_T("nID"), DataInt((int)nID));
		fd.SetParamValue(_T("pDialog"), DataLng((long)this));
		AfxGetTbCmdManager()->RunFunction(&fd, 0);
	}
}

//-----------------------------------------------------------------------------
void CParsedDialog::SendCloseParsedDialogToMacroRecorder(CWnd* pWnd, int nID)
{
	//in realtà posso anche non registrarla, mi basta che in play mi abbatte il puntatore m_pParsedDialog
	if (AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::IDLE)
		return;

	CFunctionDescription fd;
	if (
		AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
		AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordCloseParsedDialog"), fd)
		)
	{
		fd.SetParamValue(_T("nID"), DataInt((int)nID));
		AfxGetTbCmdManager()->RunFunction(&fd, 0);
	}
}

//-----------------------------------------------------------------------------
void CParsedDialog::OnWindowPosChanged(WINDOWPOS* wndPos)
{
	__super::OnWindowPosChanged(wndPos);

	CParsedForm::DoWindowPosChanged(wndPos);
}

//--------------------------------------------------------------------------
void CParsedDialog::OnOK()
{
	if (!CParsedForm::CheckForm())
		return;

	CLocalizableDialog::OnOK();
}

//--------------------------------------------------------------------------
void CParsedDialog::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
BOOL CParsedDialog::DoActivate(BOOL bActivate)
{
	BOOL bOk = CParsedForm::DoActivate(bActivate);

	if (bActivate)
	{
		CWnd* pWndFocus = GetLastFocusedCtrl();
		if (pWndFocus && IsChild(pWndFocus) && dynamic_cast<CGridControlObj*>(pWndFocus))
			pWndFocus->SetFocus();
	}

	return bOk;
}

//--------------------------------------------------------------------------
void CParsedDialog::EndDialog(int nResult)
{
	//vanno ignorati i messaggi di close provenienti dalle tabdialog
	BOOL bIgnore = this->IsKindOf(RUNTIME_CLASS(CBaseTabDialog));

	if (nResult == IDCANCEL)
		CParsedForm::AbortForm();

	int nID = GetDialogID();

	CLocalizableDialog::EndDialog(nResult);

	//messaggio di chiusura della dialog al macrorecorder
	if (!bIgnore)
		SendCloseParsedDialogToMacroRecorder(this, nID);
}

//--------------------------------------------------------------------------
CParsedCtrl* CParsedDialog::GetParsedCtrlItem(UINT nIDC)
{
	CWnd* pWnd = GetDlgItem(nIDC);
	if (!pWnd)
		return NULL;
	return dynamic_cast<CParsedCtrl*>(pWnd);
}

//--------------------------------------------------------------------------
CParsedDialog* CParsedDialog::GetParentParsedDialog()
{
	CWnd* pParent = GetParent();
	while (pParent)
	{
		if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)) && !pParent->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
			break;

		pParent = pParent->GetParent();
	}

	return (CParsedDialog*)pParent;
}

//--------------------------------------------------------------------------
BOOL CParsedDialog::PreTranslateMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);
	ASSERT(m_hWnd != NULL);

	// Coccretta anomalia (22855 enesimo tentativo) su messaggi duplicati di KayDown qundo 
	// tendina dei BCG di auto complete è aperta e a il fuoco
	if (pMsg->message == WM_KEYDOWN && (pMsg->wParam == VK_RETURN || pMsg->wParam == VK_DOWN || pMsg->wParam == VK_UP))
	{
		CStrEdit* pEdit = dynamic_cast<CStrEdit*> (GetFocus());
		if (pEdit && pEdit->m_pDropDownPopup)
		{
			if (__super::PreTranslateMessage(pMsg))
				return TRUE;
		}
	}

	//in design mode solo il form editor gestisce i messaggi
	if (GetDocument() && GetDocument()->IsInDesignMode() && pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST)
		return FALSE;

#ifndef _OLD_PTM

	if (GetDocument() && GetDocument()->m_bForwardingSysKeydownToChild)
		return FALSE;

#endif

	// Since next statements will eat frame window accelerators,
	//   we call the ParsedForm::PreProcessMessage first
	if (PreProcessMessage(pMsg))
		return TRUE;

	CParsedDialog* pParentDialog = NULL;

	// Dialog is in Dialog ? call the parent
	if (m_pParentWnd && m_pParentWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog)) && !m_pParentWnd->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
		pParentDialog = (CParsedDialog*)m_pParentWnd;

	if (pParentDialog == NULL)
		pParentDialog = GetParentParsedDialog();

	if (
		pParentDialog &&
		pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST &&
		pParentDialog->IsKindOf(RUNTIME_CLASS(CParsedDialogWithTiles)) && pParentDialog->PreProcessMessage(pMsg)
		)
		return TRUE;


	if (
		m_hAccelTable &&
		::TranslateAccelerator(m_hWnd, m_hAccelTable, pMsg)
		||
		pParentDialog &&
		pParentDialog->m_hAccelTable &&
		::TranslateAccelerator(pParentDialog->m_hWnd, pParentDialog->m_hAccelTable, pMsg)
		)
		return TRUE;

#ifndef _OLD_PTM

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetDocument(), this) || __super::PreTranslateMessage(pMsg);

#else
	return __super::PreTranslateMessage(pMsg);

#endif
}

//------------------------------------------------------------------------------
BOOL CParsedDialog::InitScroll()
{
	if (!m_bEnableHScrolling && !m_bEnableVScrolling)
		return TRUE;

	m_nHeight = -1;
	m_nWidth = -1;

	CSize dlgSize;

	CRect r;
	GetWindowRect(&r);
	dlgSize.cx = r.right - r.left;
	dlgSize.cy = r.bottom - r.top;

	if (dlgSize == CSize(0, 0))
		return FALSE;

	m_nHeight = dlgSize.cy;
	m_nWidth = dlgSize.cx;

	return TRUE;
}

//------------------------------------------------------------------------------
void CParsedDialog::PrepareScrollInfo(int cx, int cy)
{
	// nella nuova interfaccia le scrollbar vengono gestite
	// in automatico senza bisogno di far scrollare la wizardTabDialog
	if (!m_bEnableHScrolling && !m_bEnableVScrolling)
		return;

	m_nScrollStepX = 10;
	m_nScrollStepY = 10;

	m_nScrollPosX = 0;
	m_nScrollPosY = 0;

	if (cy < m_nHeight)
	{
		m_nScrollMaxY = m_nHeight;
		m_nPageSizeY = cy;
		m_nScrollPosY = min(m_nScrollPosY, m_nHeight - m_nPageSizeY);
	}
	else
	{
		m_nScrollMaxY = m_nScrollPosY = m_nPageSizeY = 0;
		return;
	}

	SCROLLINFO si;
	si.fMask = SIF_PAGE | SIF_RANGE | SIF_POS | SIF_DISABLENOSCROLL;

	si.nMin = 0;
	si.nMax = m_nScrollMaxY;
	si.nPos = m_nScrollPosY;
	si.nPage = m_nPageSizeY + 20;

	if (m_bEnableVScrolling)
		SetScrollInfo(SB_VERT, &si, TRUE);

	if (cx < m_nWidth)
	{
		m_nScrollMaxX = m_nWidth;
		m_nPageSizeX = cx;
		m_nScrollPosX = min(m_nScrollPosX, m_nWidth - m_nPageSizeX);
	}
	else
		m_nScrollMaxX = m_nScrollPosX = m_nPageSizeX = 0;

	si.nMin = 0;
	si.nMax = m_nScrollMaxX;
	si.nPos = m_nScrollPosX;
	si.nPage = m_nPageSizeX;

	if (m_bEnableHScrolling)
		SetScrollInfo(SB_HORZ, &si, TRUE);
}

//------------------------------------------------------------------------------
void CParsedDialog::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	if (m_bEnableHScrolling && m_nWidth > 0)
	{
		int nDelta;
		int nMaxPos = m_nWidth - m_nPageSizeX;

		switch (nSBCode)
		{
		case SB_LINEUP:
			if (m_nScrollPosX <= 0)
				return;
			nDelta = -(min(m_nScrollStepX, m_nScrollPosX));
			break;

		case SB_PAGEUP:
			if (m_nScrollPosX <= 0)
				return;
			nDelta = -(min(m_nPageSizeX, m_nScrollPosX));
			break;

		case SB_THUMBPOSITION:
			nDelta = (int)nPos - m_nScrollPosX;
			break;

		case SB_PAGEDOWN:
			if (m_nScrollPosX >= nMaxPos)
				return;
			nDelta = min(m_nPageSizeX, nMaxPos - m_nScrollPosX);
			break;

		case SB_LINEDOWN:
			if (m_nScrollPosX >= nMaxPos)
				return;
			nDelta = min(m_nScrollStepX, nMaxPos - m_nScrollPosX);
			break;

		default: // Ignore other scroll bar messages
			return;
		}

		m_nScrollPosX += nDelta;
		SetScrollPos(SB_HORZ, m_nScrollPosX, TRUE);
		ScrollWindow(-nDelta, 0);
	}

	__super::OnHScroll(nSBCode, nPos, pScrollBar);
	DoOnScroll(nSBCode, nPos, pScrollBar, FALSE);
}

//------------------------------------------------------------------------------
void CParsedDialog::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	if (m_bEnableVScrolling && m_nHeight > 0)
	{
		int nDelta;
		int nMaxPos = m_nHeight - m_nPageSizeY;

		switch (nSBCode)
		{

		case SB_LINEUP:
			if (m_nScrollPosY <= 0)
				return;
			nDelta = -(min(m_nScrollStepY, m_nScrollPosY));
			break;

		case SB_PAGEUP:
			if (m_nScrollPosY <= 0)
				return;
			nDelta = -(min(m_nPageSizeY, m_nScrollPosY));
			break;

		case SB_THUMBTRACK:
		case SB_THUMBPOSITION:
			nDelta = (int)nPos - m_nScrollPosY;
			break;

		case SB_PAGEDOWN:
			if (m_nScrollPosY >= nMaxPos)
				return;
			nDelta = min(m_nPageSizeY, nMaxPos - m_nScrollPosY);
			break;

		case SB_LINEDOWN:
			if (m_nScrollPosY >= nMaxPos)
				return;
			nDelta = min(m_nScrollStepY, nMaxPos - m_nScrollPosY);
			break;

		default: // Ignore other scroll bar messages
			return;
		}

		m_nScrollPosY += nDelta;
		SetScrollPos(SB_VERT, m_nScrollPosY, TRUE);
		ScrollWindow(0, -nDelta);
	}

	__super::OnVScroll(nSBCode, nPos, pScrollBar);
	DoOnScroll(nSBCode, nPos, pScrollBar, TRUE);
}

//------------------------------------------------------------------------------
BOOL CParsedDialog::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	if (!m_bEnableVScrolling)
		return FALSE;

	// Don't do anything if the vertical scrollbar is not enabled.
	int scrollMin = 0, scrollMax = 0;
	GetScrollRange(SB_VERT, &scrollMin, &scrollMax);
	if (scrollMin == scrollMax)
		return FALSE;

	// Compute the number of scrolling increments requested.
	int numScrollIncrements = abs(zDelta) / WHEEL_DELTA;

	// Each scrolling increment corresponds to a certain number of
	// scroll lines (one scroll line is like a SB_LINEUP or SB_LINEDOWN).
	// We need to query the system parameters for this value.
	int numScrollLinesPerIncrement = 0;
	::SystemParametersInfo(SPI_GETWHEELSCROLLLINES, 0, &numScrollLinesPerIncrement, 0);

	// Check if a page scroll was requested.
	if (numScrollLinesPerIncrement == WHEEL_PAGESCROLL)
	{
		// Call the vscroll message handler to do the work.
		OnVScroll(zDelta > 0 ? SB_PAGEUP : SB_PAGEDOWN, 0, NULL);
		return TRUE;
	}

	// Compute total number of lines to scroll.
	int numScrollLines = numScrollIncrements * numScrollLinesPerIncrement;

	// Adjust numScrollLines to slow down the scrolling a bit more.
	numScrollLines = max(numScrollLines / 3, 1);

	// Do the scrolling.
	for (int i = 0; i < numScrollLines; ++i)
	{
		// Call the vscroll message handler to do the work.
		OnVScroll(zDelta > 0 ? SB_LINEUP : SB_LINEDOWN, 0, NULL);
	}

	DoResizeControls();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParsedDialog::ApplyTBVisualManager()
{
	CObArray arStatics;
	PrepareDefaultExclusions(arStatics);
	EnableVisualManagerStyle(TRUE, TRUE, &m_Excluded);
	ApplyStaticSubclassing(arStatics);
}

//------------------------------------------------------------------------------
void CParsedDialog::LoadAccelerators(UINT nAccelIDR)
{
	if (nAccelIDR == 0) return;

	if (m_hAccelTable)
	{
		DestroyAcceleratorTable(m_hAccelTable);
		m_hAccelTable = NULL;
	}

	m_hAccelTable = ::TBLoadAccelerators(nAccelIDR);
}

//------------------------------------------------------------------------------
void CParsedDialog::AppendAccelerator(UINT nID, BYTE fVirt, WORD key)
{
	if (m_pToolBar)
		m_pToolBar->AppendAccelerator(m_hAccelTable, nID, fVirt, key);
}

//------------------------------------------------------------------------------
void CParsedDialog::RemoveAccelerator(UINT nID)
{
	if (m_pToolBar)
		m_pToolBar->RemoveAccelerator(m_hAccelTable, nID);
}

//---------------------------------------------------------------------------
void CParsedDialog::SetToolbarStyle(ToolbarStyle style, int nToolbarHeight /*32*/, BOOL bDefaultOkCancelBar /*FALSE*/, BOOL bWithTexts /*FALSE*/)
{
	if (style == NONE)
	{
		DestroyToolbar();
		return;
	}

	ASSERT(!m_pToolBar);
	m_pToolBar = new CTBToolBar();
	m_pToolBar->SuspendLayout();

	m_bHasDefaultOkCancelBar = bDefaultOkCancelBar;

	CString sToolbarName = GetNamespace().ToUnparsedString() + _T("_Toolbar");
	if (m_pToolBar->CreateEmpty(this, sToolbarName))
	{
		m_pToolBar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
		m_pToolBar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());
		m_pToolBar->SetTextColor(AfxGetThemeManager()->GetDialogToolbarTextColor());
		m_pToolBar->SetTextColorHighlighted(AfxGetThemeManager()->GetDialogToolbarTextHighlightedColor());
		m_pToolBar->SetHighlightedColor(AfxGetThemeManager()->GetDialogToolbarHighlightedColor());

		OnCustomizeToolbar();

		m_pToolBar->EnableTextLabels(bWithTexts);
		CSize szButtons(nToolbarHeight, nToolbarHeight);
		m_pToolBar->SetSizes(szButtons, szButtons);
		m_pToolBar->ShowInDialog(this, style == BOTTOM ? CBRS_ALIGN_BOTTOM : CBRS_ALIGN_TOP);

		CRect aRect;
		m_pToolBar->GetClientRect(aRect);
		m_nToolbarHeight = (max(m_pToolBar->CalcMaxButtonHeight(), aRect.Height()) + AfxGetThemeManager()->GetToolbarHighlightedHeight()); // Add px for space of cursor;
		m_pToolBar->SetWindowTextW(_T("ToolBar"));
		m_pToolBar->AttachOSLInfo(GetInfoOSL());
		m_pToolBar->ResumeLayout(TRUE);
	}
}

//---------------------------------------------------------------------------
void CParsedDialog::GetCandidateRectangles(CRect& rcOtherComponents, CRect& rcToolbar, int cx /*-1*/, int cy /*-1*/)
{
	return CParsedForm::GetCandidateRectangles(this, m_pToolBar, m_nToolbarHeight, rcOtherComponents, rcToolbar, cx, cy);
}

//------------------------------------------------------------------------------
void CParsedDialog::DoStretch(int cx /*-1*/, int cy /*-1*/)
{
	CRect aTileGroupRect; CRect aRectToolbar;
	GetCandidateRectangles(aTileGroupRect, aRectToolbar);

	// prima ridimensiona il l'oggetto correlato
	ResizeOtherComponents(aTileGroupRect);

	// poi la toolbar 
	if (m_pToolBar)
	{
		m_pToolBar->SetWindowPos(NULL, 0, aRectToolbar.top, aRectToolbar.Width(), aRectToolbar.Height(), SWP_NOZORDER);
		m_pToolBar->AdjustLayout();
	}
}

//------------------------------------------------------------------------------
void CParsedDialog::OnSize(UINT nType, int cx, int cy)
{
	DoResize();
	DoStretch();

	__super::OnSize(nType, cx, cy);

	if (m_bEnableHScrolling || m_bEnableVScrolling)
	{
		if (m_nHeight < 0 || m_nWidth < 0)
		{
			InitScroll();
		}
		PrepareScrollInfo(cx, cy);
	}
}

//---------------------------------------------------------------------------
void CParsedDialog::OnCustomizeToolbar()
{
	if (m_pToolBar && m_bHasDefaultOkCancelBar)
	{
		m_pToolBar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"));
		m_pToolBar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"));

		m_pToolBar->SetTextToolTip(IDOK, _TB("Ok (Ctrl + O)"));
		m_pToolBar->SetTextToolTip(IDCANCEL, _TB("Cancel (Ctrl + A)"));

		LoadAccelerators(IDR_PARSEDDIALOG);

		// va messa sempre dopo al caricamento degli acceleratori di defualt in quenato aggiunge gli acceleratori per <return> e <escape>
		m_pToolBar->SetDefaultAction(IDOK);
	}
}

////////////////////////////////////////////////////////////////////////////////
//===========================================================================
// CBaseFormView
//===========================================================================
////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CBaseFormView, CBCGPFormView)

BEGIN_MESSAGE_MAP(CBaseFormView, CBCGPFormView)

	ON_MESSAGE(UM_BAD_VALUE, OnBadValue)
	ON_MESSAGE(UM_LOSING_FOCUS, OnLosingFocus)
	ON_MESSAGE(UM_CTRL_FOCUSED, OnCtrlFocused)
	ON_MESSAGE(WM_GETFONT, OnGetFont)
	ON_MESSAGE(UM_GET_DIALOG_ID, OnGetDialogId)
	ON_WM_SETFOCUS()
	ON_WM_WINDOWPOSCHANGED()
	ON_WM_HSCROLL()
	ON_WM_VSCROLL()
	ON_WM_MOUSEWHEEL()
	ON_WM_CREATE()
	ON_WM_CTLCOLOR()
	ON_WM_SIZE()
	ON_NOTIFY_EX(TTN_NEEDTEXT, 0, OnToolTipNotify)
	ON_NOTIFY_EX(TTN_POP, 0, OnToolTipHide)

END_MESSAGE_MAP()

#pragma warning(disable:4355) // disabilita la warning sull'uso del this del parent
//---------------------------------------------------------------------------
CBaseFormView::CBaseFormView(CString sName, UINT nIDTemplate)
	:
	CBCGPFormView((LPCTSTR)NULL),
	CParsedForm(this, nIDTemplate, sName),
	IDisposingSourceImpl(this),
	m_hWndFocusView(0),
	m_NotValidCtrlID(0),
	m_ErrorMode(0),
	m_hResourceModule(NULL),
	m_pTileStyle(NULL),
	m_bIsModal(FALSE)
{
	// La CBaseFormView memorizza qual'è l'ultimo control che ha ricevuto il fuoco
	m_pAncestorWnd = this;
	m_phLastCtrlFocused = &m_hWndFocusView;
	m_pNotValidCtrlID = &m_NotValidCtrlID;
	m_pErrorMode = &m_ErrorMode;

	m_pTileStyle = TileStyle::Inherit(AfxGetTileDialogStyleNormal());
	m_bUnattendedMode = TRUE == AfxIsInUnattendedMode();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

#pragma warning(default:4355)
//-----------------------------------------------------------------------------
CBaseFormView::~CBaseFormView()
{
	if (AfxGetThreadContext()->IsInErrorState()) //to prevent another exception when destroying a document after an exception
		AfxGetThreadState()->m_pRoutingView = NULL;

	SAFE_DELETE(m_pLayoutContainer);
	SAFE_DELETE(m_pTileStyle);
	SAFE_DELETE(m_pJsonContext);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//---------------------------------------------------------------------------
void CBaseFormView::EnableLayout(BOOL bEnable /*= TRUE*/)
{
	if (GetDocument() && GetDocument()->IsInStaticDesignMode())
		return;

	SAFE_DELETE(m_pLayoutContainer);
	if (bEnable)
	{
		m_pLayoutContainer = new CLayoutContainer(this, m_pTileStyle);
		// if layout is managed, by default Views use VBOX
		m_pLayoutContainer->SetLayoutType(CLayoutContainer::VBOX);
		SetCenterControls(FALSE);
	}
}

//------------------------------------------------------------------------------
void CBaseFormView::GetAvailableRect(CRect &rectAvail)
{
	GetClientRect(&rectAvail);
}

//------------------------------------------------------------------------------
void CBaseFormView::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	if (m_pLayoutContainer)
	{
		CRect rcView;
		GetClientRect(&rcView);
		Relayout(rcView);
	}


}

//------------------------------------------------------------------------------
void CBaseFormView::Relayout(CRect &rectNew, HDWP hDWP /*= NULL*/)
{
	if (IsLayoutSuspended())
		return;

	if (!m_pLayoutContainer)
		return;

	if (m_nMapMode > 0)
	{
		CPoint ptActual = GetDeviceScrollPosition();
		if (ptActual.x > 0 || ptActual.y > 0)
			ScrollToPosition(CPoint(0, 0));
	}

	// Avoid calling MoveWindow if just internal repositioning due to collapsing
	CRect rectActual;
	GetClientRect(rectActual);

	if (rectActual != rectNew)
	{
		if (this->GetParent())
			this->GetParent()->InvalidateRect(rectActual);

		SetWindowPos(NULL, rectNew.left, rectNew.top, rectNew.Width(), rectNew.Height(), SWP_NOZORDER | SWP_FRAMECHANGED);
		UpdateWindow();
	}
	else
		m_pLayoutContainer->Relayout(rectNew);

	AdjustScrollSize();

	// _UPDATE_CONTROLS_OPTIMAZED_BEGIN
	//	if (GetDocument())
	//		GetDocument()->UpdateDataView();
	// _UPDATE_CONTROLS_OPTIMAZED_END
}

//------------------------------------------------------------------------------
void CBaseFormView::AdjustScrollSize(BOOL bSizeOnly /*= FALSE*/)
{
	//Se sono da web, non vengono fatte operazioni di scroll
	if (AfxIsRemoteInterface())
		return;

	//ASSERT_TRACE(m_pLayoutContainer, "AdjustScrollSize called without a container!");
	if (!m_pLayoutContainer)
		return;

	// Retain the current scroll position to restore it after adjusting scrollbars
	CPoint ptActual = GetDeviceScrollPosition();


	CRect viewRect;
	GetClientRect(viewRect);

	CRect rectUsed;
	m_pLayoutContainer->GetUsedRect(rectUsed);
	ScreenToClient(rectUsed);

	// checks on possible negative values
	SIZE sz;
	sz.cx = 0;	sz.cy = 0;
	if (rectUsed.Width() > 0)
		sz.cx = rectUsed.Width();
	if (rectUsed.Height() > 0)
		sz.cy = rectUsed.Height();

	SetScrollSizes(MM_TEXT, sz);

	// reposition the scrolled view where it was before. If now it is too low, the view
	// automatically adjusts it to the lowest possible point
	if (!bSizeOnly && ptActual.x > 0 && ptActual.y > 0)
		ScrollToPosition(ptActual);

	DoOnScroll(0, 0, NULL, FALSE);
}

//------------------------------------------------------------------------------
void CBaseFormView::GetUsedRect(CRect &rectUsed)
{
	ASSERT_TRACE(FALSE, "BaseFormView is not supposed to be part of a layout");
	GetClientRect(&rectUsed);
}

//----------------------------------------------------------------------------
CSize CBaseFormView::GetDialogSize() const
{
	int		nMapMode;
	CSize	sizeTotal;
	CSize	sizePage;
	CSize	sizeLine;

	GetDeviceScrollSizes(nMapMode, sizeTotal, sizePage, sizeLine);
	return sizeTotal;
}

//----------------------------------------------------------------------------
void CBaseFormView::ScrollFormViewToCtrl(CWnd* pCtrl)
{
	CFrameWnd* pFrame = (CFrameWnd*)GetParentFrame();

	ASSERT_VALID(pFrame);
	ASSERT_KINDOF(CFrameWnd, pFrame);

	// se e' iconizzata MFC erroneamente setta male l'origine se si chiama una
	// ScrollToPosition. Pertanto ignoro la posizione dei controls e ignoro l'azione
	if (pFrame->IsIconic())
		return;

	// il control potrebbe nel frattempo essere morto o nascosto
	if (!pCtrl || !::IsWindow(pCtrl->m_hWnd) || !IsTBWindowVisible(pCtrl))
		return;

	CWnd* pParent = pCtrl->GetParent();
	while (pParent && !pParent->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
		pParent = pParent->GetParent();

	CBaseTileDialog* pBTD = dynamic_cast<CBaseTileDialog*>(pParent);
	if (pBTD && !pBTD->IsDisplayed())
		return;

	CRect rectForm, rectCtrl;

	if (pCtrl->GetParent()->m_hWnd == m_hWnd)
		GetClientRect(rectForm);
	else
	{
		pCtrl->GetParent()->GetWindowRect(rectForm);
		ScreenToClient(rectForm);
	}

	pCtrl->GetWindowRect(rectCtrl);
	ScreenToClient(rectCtrl);

	BOOL bIsAGrid =
		dynamic_cast<CGridControlObj*>(pCtrl) ||
		dynamic_cast<CGridControlObj*>(pCtrl->GetParent());

	// Prende l'origine relativa della client area
	CPoint ptOrigin = GetScrollPosition();

	// se il control e' per intero nella cliente non scrolla.
	if (
		rectCtrl.right >= 0 &&
		rectCtrl.left >= 0 &&
		rectCtrl.left <= rectForm.right &&
		(bIsAGrid || rectCtrl.right <= rectForm.right) &&

		rectCtrl.top >= 0 &&
		rectCtrl.bottom >= 0 &&
		rectCtrl.top <= rectForm.bottom &&
		(bIsAGrid || rectCtrl.bottom <= rectForm.bottom)
		)
		return;

	// esistono tre algoritmi possibili, il terzo e' il migliore (per me)
	//
	/*****************************************************************************************
	if (rectCtrl.right > rectForm.right)	ptOrigin.x += (rectCtrl.right - rectForm.right);
	if (rectCtrl.left <= 0)					ptOrigin.x += rectCtrl.left;
	if (rectCtrl.left > rectForm.right)		ptOrigin.x += (rectCtrl.left - rectForm.right);

	if (rectCtrl.bottom > rectForm.bottom)	ptOrigin.y += (rectCtrl.bottom - rectForm.bottom);
	if (rectCtrl.top <= 0)					ptOrigin.y += rectCtrl.top;
	if (rectCtrl.top > rectForm.bottom)		ptOrigin.y += (rectCtrl.top - rectForm.bottom);
	*****************************************************************************************/

	/*****************************************************************************************
		if (rectCtrl.right > rectForm.right)	ptOrigin.x += (rectCtrl.right - rectForm.right);
		if (rectCtrl.left <= 0)					ptOrigin.x += (rectCtrl.right - rectForm.right);
		if (rectCtrl.left > rectForm.right)		ptOrigin.x += (rectCtrl.right - rectForm.right);

		if (rectCtrl.bottom > rectForm.bottom)	ptOrigin.y += (rectCtrl.bottom - rectForm.bottom);
		if (rectCtrl.top <= 0)					ptOrigin.y += (rectCtrl.bottom - rectForm.bottom);
		if (rectCtrl.top > rectForm.bottom)		ptOrigin.y += (rectCtrl.bottom - rectForm.bottom);
	*****************************************************************************************/

	/*****************************************************************************************/
	if (
		rectCtrl.right > rectForm.right ||
		rectCtrl.left <= 0 ||
		rectCtrl.left > rectForm.right
		)
		ptOrigin.x += (rectCtrl.right - rectForm.right);

	if (
		rectCtrl.bottom > rectForm.bottom ||
		rectCtrl.top <= 0 ||
		rectCtrl.top > rectForm.bottom
		)
		ptOrigin.y += (rectCtrl.bottom - rectForm.bottom);
	/*****************************************************************************************/

	if (ptOrigin.x < 0)	ptOrigin.x = 0;
	if (ptOrigin.y < 0)	ptOrigin.y = 0;

	// finalmente possiamo scrollare
	ScrollToPosition(ptOrigin);

	CParsedForm::DoResizeControls();
}

//-----------------------------------------------------------------------------
BOOL CBaseFormView::DoActivate(BOOL bActivate)
{
	return CParsedForm::DoActivate(bActivate);
}

// copia modificata di CFormView::Create perché non c'erano punti di iniezione in cui intervenire
// i commenti mostrano il codice originario e quello inserito per i nostri fini
// In pratica invece di creare la dialog a partire da risorsa, la crea a partire da JSON
//-----------------------------------------------------------------------------
BOOL CBaseFormView::JsonCreate(DWORD dwRequestedStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext, BOOL bNotForJson)
{
	ASSERT(m_pJsonContext);
#if (_MFC_VER != 0x0C00) && (_MFC_VER != 0x0E00) // MFC 12.0 (VS2013) o MFC 14.0 (VS2015)
#error La versione di MFC è cambiata: controllare che questo metodo sia ancora equivalente al metodo CFormView::Create, di cui rappresenta una copia modificata
#endif
	ASSERT(pParentWnd != NULL);
	/*** CODICE ORIGINARIO
		ASSERT(m_lpszTemplateName != NULL);
	*/

	m_pCreateContext = pContext;    // save state for later OnCreate
/*** CODICE ORIGINARIO
#ifdef _DEBUG
	// dialog template must exist and be invisible with WS_CHILD set
	if (!_AfxCheckDialogTemplate(m_lpszTemplateName, TRUE))
	{
		ASSERT(FALSE);          // invalid dialog template name
		PostNcDestroy();        // cleanup if Create fails too soon
		return FALSE;
	}
#endif //_DEBUG

	// initialize common controls
	VERIFY(AfxDeferRegisterClass(AFX_WNDCOMMCTLS_REG));		//istruzione presenti nella CreateDialogIndirect
	AfxDeferRegisterClass(AFX_WNDCOMMCTLSNEW_REG);			//istruzione presenti nella CreateDialogIndirect
*/
// call PreCreateWindow to get prefered extended style
	CREATESTRUCT cs; memset(&cs, 0, sizeof(CREATESTRUCT));
	if (dwRequestedStyle == 0)
		dwRequestedStyle = AFX_WS_DEFAULT_VIEW;
	cs.style = dwRequestedStyle;
	if (!PreCreateWindow(cs))
		return FALSE;

	/*** CODICE ORIGINARIO
		// create a modeless dialog
		if (!CreateDlg(m_lpszTemplateName, pParentWnd))
			return FALSE;
	*/
	//***INIZIO CODICE NOSTRO
	if (m_sName.IsEmpty())
		m_sName = CJsonFormEngineObj::GetObjectName(m_pJsonContext->m_pDescription);
	AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)m_pJsonContext->CreateTemplate(false);
	if (!pTemplate || !CreateDlgIndirect(pTemplate, pParentWnd, NULL))
		return FALSE;
	//***FINE CODICE NOSTRO
	m_pCreateContext = NULL;

	SetDlgCtrlID(nID);

	if (bNotForJson)
	{
		// we use the style from the template - but make sure that
		//  the WS_BORDER bit is correct
		// the WS_BORDER bit will be whatever is in dwRequestedStyle
		ModifyStyle(WS_BORDER | WS_CAPTION, cs.style & (WS_BORDER | WS_CAPTION));
		ModifyStyleEx(WS_EX_CLIENTEDGE, cs.dwExStyle & WS_EX_CLIENTEDGE);

		CRect rectTemplate;
		GetWindowRect(rectTemplate);
		SetScrollSizes(MM_TEXT, rectTemplate.Size());
	}
	else
	{
		m_nMapMode = MM_TEXT;
	}
	/*** CODICE ORIGINARIO				non serve, non ci sono RC_INIT nelle nostre risorse
		// initialize controls etc
		if (!ExecuteDlgInit(m_lpszTemplateName))
			return FALSE;
	*/
	/*** CODICE ORIGINARIO				non serve, la size verrà poi impostata alla fine
	// force the size requested
		SetWindowPos(NULL, rect.left, rect.top,
			rect.right - rect.left, rect.bottom - rect.top,
			SWP_NOZORDER | SWP_NOACTIVATE);
	*/
	// make visible if requested
	if (dwRequestedStyle & WS_VISIBLE)
		ShowWindow(SW_NORMAL);

	if (AfxIsRemoteInterface())
	{
		m_pJsonContext->Associate(this);
		return TRUE;
	}

	return CJsonFormEngineObj::GetInstance()->CreateChilds(m_pJsonContext, this);
}


//----------------------------------------------------------------------------
BOOL CBaseFormView::Create
(
	LPCTSTR lpszClassName, LPCTSTR lpszWindowName,
	DWORD dwRequestedStyle, const RECT& rect, CWnd* pParentWnd, UINT nIDD,
	CCreateContext* pContext
)
{

	CBaseDocument* pDoc = dynamic_cast<CBaseDocument*>(pContext->m_pCurrentDoc);
	if (pDoc && pDoc->GetImportExportParams() && pDoc->GetImportExportParams()->IsOnlyBusinessObject())
		return TRUE;

	BOOL bFlatStyle = AfxGetThemeManager()->UseFlatStyle();

	if (bFlatStyle)
		dwRequestedStyle = dwRequestedStyle & ~(WS_EX_CLIENTEDGE | WS_EX_WINDOWEDGE | WS_EX_STATICEDGE);
	if (m_nID == 0)//per le viste json adesso posso indicare l'id nel template
	{
		m_nID = ((CSingleExtDocTemplate*)pContext->m_pNewDocTemplate)->m_nViewID;
	}
	//il contesto json mi può essere passato da fuori
	if (!m_pJsonContext &&
		(CSingleExtDocTemplate*)pContext->m_pNewDocTemplate &&
		((CSingleExtDocTemplate*)pContext->m_pNewDocTemplate)->m_pJsonContext)
		m_pJsonContext = ((CSingleExtDocTemplate*)pContext->m_pNewDocTemplate)->m_pJsonContext;

	BOOL bOk = FALSE;
	CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nID);
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (m_pJsonContext || !res.IsEmpty())
	{
		//potrebbe essere già stato creato da una view derivata (es. EasyStudio Designer)
		if (!m_pJsonContext)
		{
			m_pJsonContext = CJsonFormEngineObj::GetInstance()->CreateContext(res);
			if (!m_pJsonContext || !m_pJsonContext->m_pDescription)
			{
				ASSERT(FALSE);
				return FALSE;
			}
		}
		if (GetFormName().IsEmpty())
			SetFormName(m_pJsonContext->m_pDescription->m_strName);
		bOk = JsonCreate(dwRequestedStyle, rect, pParentWnd, nIDD, pContext, TRUE);
	}
	else
	{
		m_lpszTemplateName = MAKEINTRESOURCE(m_nID);
		CSetResourceHandle h(GetResourceModule());
		bOk = __super::Create
		(
			lpszClassName, lpszWindowName,
			dwRequestedStyle,
			rect, pParentWnd, nIDD,
			pContext
		);
	}
	if (!bOk)
		return FALSE;

	if (bFlatStyle)
		AfxGetThemeManager()->MakeFlat(this);

	CalculateOSLInfo();

	return TRUE;
}

//----------------------------------------------------------------------------
void CBaseFormView::CalculateOSLInfo()
{
	CInfoOSL* pParent = NULL;
	if (GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CTaskBuilderDockPane)))
	{
		pParent = ((CTaskBuilderDockPane*)GetParent())->GetInfoOSL();
		GetInfoOSL()->SetType(OSLType_SlaveTemplate);
	}
	else
	{
		if (GetInfoOSL()->GetType() == OSLType_Null)
			GetInfoOSL()->SetType(OSLType_SlaveTemplate);

		if (GetDocument())
			pParent = GetDocument()->GetInfoOSL();
	}

	GetInfoOSL()->m_pParent = pParent;
	if (pParent)
		GetNamespace().SetChildNamespace(CTBNamespace::FORM, m_sName, pParent->m_Namespace);
}

//--------------------------------------------------------------------------
HINSTANCE CBaseFormView::GetResourceModule()
{
	/*CRuntimeClass* pClass = GetRuntimeClass();
	while (pClass && pClass != RUNTIME_CLASS(CBaseFormView))
	{
		HINSTANCE hInst = GetDllInstance(pClass);
		if (::FindResource(hInst, m_lpszTemplateName, RT_DIALOG))
			return hInst;
		pClass = pClass->m_pfnGetBaseClass();
	}*/

	if (m_hResourceModule == NULL)
		m_hResourceModule = GetDllInstance(GetRuntimeClass());
	return m_hResourceModule;
}
//--------------------------------------------------------------------------
void CBaseFormView::OnInitialUpdate()
{
	if (!m_pJsonContext)//se non sono json, devo tradurre, altrimenti le ha già tradotte l'engine json
	{
		if (m_lpszTemplateName != NULL)
		{
			//imposto la dll di default in cui cercare le risorse uguale a quella che contiene la definizione della tabdialog
			CSetResourceHandle h(GetResourceModule());
			AfxLoadWindowStrings(this, m_lpszTemplateName);
		}
	}

	HINSTANCE hinstance = GetDllInstance(GetRuntimeClass());
	AddOnDll* pAddOn = AfxGetAddOnDll(hinstance);

	m_pOwner = pAddOn ? pAddOn->GetNamespace() : NULL;

	if (!m_pJsonContext)//se sono json, il font letto dai settings è già impostato
		SetDefaultFont();

	CParsedForm::AttachDocument(GetDocument());
	CParsedForm::SetBackgroundColor(m_clrBackground);

	__super::OnInitialUpdate();
}

//-----------------------------------------------------------------------------
void CBaseFormView::OnActivateView(BOOL bActive, CView* pActivateView, CView* pDeactiveView)
{
	__super::OnActivateView(bActive, pActivateView, pDeactiveView);
	DoActivate(bActive);
}
//------------------------------------------------------------------------------
BOOL CBaseFormView::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (nCode == EN_VALUE_CHANGED || nCode == EN_VALUE_CHANGED_FOR_FIND || nCode == BEN_ROW_CHANGED)
	{
		BOOL b = FALSE;
		//chiamo gli eventi della view solo se sono in interattivo
		if (!m_bUnattendedMode)
			b = CWnd::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);

		// quindi chiamo gli eventi di documento (anche se già gestiti dalla view, a differenza di MFC)
		if (m_pDocument != NULL)
		{
			// special state for saving view before routing to document
			CPushRoutingView push(this);
			b = m_pDocument->OnCmdMsg(nID, nCode, pExtra, pHandlerInfo) || b;
			return b;
		}
		return b;
	}
	else
	{
		return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
	}
}

// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL CBaseFormView::OnCommand(WPARAM wParam, LPARAM lParam)
{
	BOOL bNotFarwardToParent = CParsedForm::DoCommand(wParam, lParam);

	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	if (!bNotFarwardToParent)
	{
		CWnd* pParent = GetParent();
		if (pParent)
			return pParent->SendMessage(WM_COMMAND, wParam, lParam);
	}

	return FALSE;
}

//--------------------------------------------------------------------------
LRESULT CBaseFormView::OnBadValue(WPARAM nIdc, LPARAM lMode)
{
	return CParsedForm::DoBadValue((UINT)nIdc, LOWORD(lMode));
}

//--------------------------------------------------------------------------
LRESULT CBaseFormView::OnLosingFocus(WPARAM nIdc, LPARAM nRelationship)
{
	CParsedForm::DoLosingFocus((UINT)nIdc, (int)nRelationship);

	return (LRESULT)0L;
}

//------------------------------------------------------------------------------
LRESULT CBaseFormView::OnCtrlFocused(WPARAM wParam, LPARAM lParam)
{
	//se sto editando in EasyBuilder, lo scroll mi crea casini nella selezione e non lo voglio!
	if (GetDocument() && ((CBaseDocument*)GetDocument())->IsInDesignMode())
		return 0L;

	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	ScrollFormViewToCtrl(FromHandle(hWndCtrl));
	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CBaseFormView::OnGetFont(WPARAM wParam, LPARAM lParam)
{
	HFONT hFont = (HFONT)AfxGetThemeManager()->GetFormFont()->GetSafeHandle();
	if (hFont)
		return (LRESULT)(UINT)hFont;

	return DefWindowProc(WM_GETFONT, wParam, lParam);
}

//--------------------------------------------------------------------------
LRESULT CBaseFormView::OnGetDialogId(WPARAM, LPARAM)
{
	return GetDialogID();
}

// To manage data validation on view focusing
//-----------------------------------------------------------------------------
void CBaseFormView::OnSetFocus(CWnd* pOldWnd)
{
	// Per motivi imperscrutabili dare in questo momento il fuoco ad una combobox ne fa incasinare il paint
	CWnd* pWnd = FromHandlePermanent(m_hWndFocus);
	if (
		dynamic_cast<CGridControlObj*>(GetLastFocusedCtrl()) ||
		!pWnd || !CParsedCombo::IsChildEditCombo(pWnd)
		)
		m_hWndFocus = m_hWndFocusView;

	__super::OnSetFocus(pOldWnd);

	DoActivate(TRUE);
}

//-----------------------------------------------------------------------------
void CBaseFormView::OnWindowPosChanged(WINDOWPOS* wndPos)
{
	__super::OnWindowPosChanged(wndPos);

	CParsedForm::DoWindowPosChanged(wndPos);
}

//------------------------------------------------------------------------------
void CBaseFormView::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	__super::OnHScroll(nSBCode, nPos, pScrollBar);
	DoOnScroll(nSBCode, nPos, pScrollBar, FALSE);
}

//------------------------------------------------------------------------------
void CBaseFormView::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	__super::OnVScroll(nSBCode, nPos, pScrollBar);
	DoOnScroll(nSBCode, nPos, pScrollBar, TRUE);
}

//------------------------------------------------------------------------------
BOOL CBaseFormView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	BOOL b = __super::OnMouseWheel(nFlags, zDelta, pt);

	DoResizeControls();
	return b;
}

//-----------------------------------------------------------------------------
HBRUSH CBaseFormView::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	HBRUSH lResult = DoOnCtlColor(pDC, pWnd, nCtlColor);
	if (lResult)
		return lResult;     // catched: eat it

	return __super::OnCtlColor(pDC, pWnd, nCtlColor);
}

//-----------------------------------------------------------------------------
void CBaseFormView::ApplyTBVisualManager()
{
	CObArray arStatics;
	PrepareDefaultExclusions(arStatics);

	EnableVisualManagerStyle(TRUE, &m_Excluded);

	ApplyStaticSubclassing(arStatics);
}

//-----------------------------------------------------------------------------
HRESULT CBaseFormView::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = GetRanorexNamespace();
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//-----------------------------------------------------------------------------
CString CBaseFormView::GetRanorexNamespace()
{
	return cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
}



/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CBaseFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CBaseFormView\n");
}

void CBaseFormView::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG

//=============================================================================
//			Class ResizableCtrl implementation
//=============================================================================
ResizableCtrl::ResizableCtrl()
	:
	m_nDirStrech(3),
	m_CurSize(-1, -1),
	m_pOwnerWnd(NULL),
	m_nXMargin(0),
	m_nYMargin(0),
	m_BornSize(-1, -1),
	m_bAlreadyWorking(FALSE)
{}

//------------------------------------------------------------------------------
void ResizableCtrl::SetAutoSizeCtrl(int n)
{
	m_nDirStrech = n;

	InitSizeInfo(m_pOwnerWnd);
}

//------------------------------------------------------------------------------
void ResizableCtrl::InitSizeInfo(CWnd* pOwnerWnd)
{
	m_pOwnerWnd = pOwnerWnd;
	if (m_pOwnerWnd == NULL)
	{
		m_CurSize.cx = -1;
		m_CurSize.cy = -1;
		return;
	}

	CRect rectBody;
	m_pOwnerWnd->GetWindowRect(rectBody);
	m_pOwnerWnd->GetParent()->ScreenToClient(rectBody);

	if (m_BornSize.cx == -1 && m_BornSize.cy == -1)
		m_BornSize = rectBody.Size();

	if (m_nDirStrech != 0)
	{
		CRect  rcChild;

		for (CWnd* pwndChild = m_pOwnerWnd->GetParent()->GetWindow(GW_CHILD); pwndChild; pwndChild = pwndChild->GetNextWindow())
		{
			if (pwndChild->m_hWnd != m_pOwnerWnd->m_hWnd)
			{
				pwndChild->GetWindowRect(rcChild);
				m_pOwnerWnd->GetParent()->ScreenToClient(rcChild);
				if (rcChild.top >= rectBody.bottom)
					m_nDirStrech &= 2;

				if (rcChild.left >= rectBody.right)
					m_nDirStrech &= 1;

				if (
					rcChild.PtInRect(CPoint(rectBody.left, rectBody.top))
					&&
					rcChild.PtInRect(CPoint(rectBody.right, rectBody.bottom))
					)
					m_nDirStrech = 0;
			}
		}
	}

	CRect rectParent;
	m_pOwnerWnd->GetParent()->GetClientRect(rectParent);

	//patch 12/01/2000
	if (m_pOwnerWnd->GetParent()->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
	{
		CSize dlgSize = ((CBaseFormView*)m_pOwnerWnd->GetParent())->GetDialogSize();
		rectParent.right = dlgSize.cx;
		rectParent.bottom = dlgSize.cy;
	}
	//----

	m_CurSize.cx = max(rectParent.right - rectBody.right, 0);
	m_CurSize.cy = max(rectParent.bottom - rectBody.bottom, 0);
}

//------------------------------------------------------------------------------
void ResizableCtrl::SetResizableCurSize(int nWidth, int nHeight)
{
	m_CurSize.cx = nWidth;
	m_CurSize.cy = nHeight;
}

//------------------------------------------------------------------------------
BOOL ResizableCtrl::SkipRecalcCtrlSize()
{
	return !m_pOwnerWnd || !::IsWindow(m_pOwnerWnd->m_hWnd) || m_nDirStrech == 0 ||
		//!::IsTBWindowVisible(m_pOwnerWnd) ||
		m_CurSize.cx < 0 || m_CurSize.cy < 0;
}

//------------------------------------------------------------------------------
BOOL ResizableCtrl::DoRecalcCtrlSize()
{
	if (m_bAlreadyWorking)
		return FALSE;

	if (SkipRecalcCtrlSize())
		return FALSE;

	m_bAlreadyWorking = TRUE;

	CRect rectParent;
	m_pOwnerWnd->GetParent()->GetClientRect(rectParent);
	if (rectParent.IsRectNull())//finestra iconizzata
	{
		m_bAlreadyWorking = FALSE;
		return FALSE;
	}
	CRect rectBody;
	m_pOwnerWnd->GetWindowRect(rectBody);
	m_pOwnerWnd->GetParent()->ScreenToClient(rectBody);

	int x = 0, y = 0;
	UINT noMove = SWP_NOMOVE;
	int cx, cy;

	if (m_nDirStrech & 4)
	{
		x = 0;
		y = CalcMinY();
		noMove = 0;
	}

	if (m_nDirStrech & 2)
		cx = rectParent.right - m_CurSize.cx - rectBody.left - m_nXMargin;
	else
		cx = rectBody.Width() - m_nXMargin;

	if (m_nDirStrech & 1)
		cy = rectParent.bottom - m_CurSize.cy - rectBody.top - m_nYMargin;
	else
		cy = rectBody.Height() - m_nYMargin;

	int yTileTitle = 0;
	if (m_nDirStrech == 7)
	{
		CWnd* pWndParent = m_pOwnerWnd->GetParent();
		CBaseTileDialog* pTileParent = dynamic_cast<CBaseTileDialog*>(pWndParent);
		if (pTileParent)
			yTileTitle = pTileParent->GetTitleHeight();

		y += yTileTitle;
		cx = rectParent.right - m_CurSize.cx - m_nXMargin;
		cy = rectParent.bottom - m_CurSize.cy - m_nYMargin - yTileTitle;
	}

	//In web mode, we need to maintain the minimum original size to avoid 
	//wrong drawing of this control
	if (AfxIsRemoteInterface())
	{
		int nMin = 0;
		int nMax = 0;

		if (m_nDirStrech & 2)
		{
			m_pOwnerWnd->GetParent()->GetScrollRange(SB_HORZ, &nMin, &nMax);
			cx = max(cx, nMax - rectBody.left - m_nXMargin);
		}

		if (m_nDirStrech & 1)
		{
			m_pOwnerWnd->GetParent()->GetScrollRange(SB_VERT, &nMin, &nMax);
			cy = max(cy, nMax - rectBody.top - m_nYMargin);
		}
	}

	m_pOwnerWnd->SetWindowPos(NULL, x, y, cx, cy, noMove | SWP_NOZORDER | SWP_NOACTIVATE);
	m_pOwnerWnd->UpdateWindow(); // senza questa non funzionano i draw delle tab
	m_bAlreadyWorking = FALSE;
	return TRUE;
}

//------------------------------------------------------------------------------
int ResizableCtrl::CalcMinY()
{
	CWnd* pParentWnd = m_pOwnerWnd->GetParent();
	HWND hParent = pParentWnd->m_hWnd;
	HWND hHandleToSkip = m_pOwnerWnd->m_hWnd;

	int minY = 0;
	int offset = 0;

	HWND  hChild = GetWindow(hParent, GW_CHILD);
	while (hChild)
	{
		CWnd* pWndChild = CWnd::FromHandle(hChild);
		int res = pWndChild->GetDlgCtrlID();

		BOOL skip = (res == IDC_STATIC_AREA || res == IDC_STATIC_AREA_2);
		
		if (!skip && hChild != hHandleToSkip)
		{
			CRect aRect;
			::GetWindowRect(hChild, &aRect);
			pParentWnd->ScreenToClient(aRect);

			if (aRect.bottom  > minY)
			{
				minY = aRect.bottom;
				offset = aRect.Height();
			}
		}

		hChild = GetNextWindow(hChild, GW_HWNDNEXT);
	}

	return minY + offset;
}

//-----------------------------------------------------------------------------
//TODO
void ResizableCtrl::Anchor(int nIDC)
{
	m_arAnchoredCtrl.Add(nIDC);

	//todo ... differenziare bottom da right
	//get GetDlgItem
	//get Clientrect
	//csize = maxy - miny
}

//------------------------------------------------------------------------------
BOOL MatchType(DataType aDataType1, DataType aDataType2)
{
	if (aDataType1.m_wType != aDataType2.m_wType)
		return FALSE;

	//per gli enumerativi non devo discriminare il tipo, ho sempre Enum per qualsiasi tipo di enumerativo)
	if (aDataType1 == DATA_ENUM_TYPE)
		return TRUE;
	return aDataType1.m_wTag == aDataType2.m_wTag;
}

//===========================================================================
//		class CRegisteredParsedCtrl
//===========================================================================
IMPLEMENT_DYNAMIC(CRegisteredParsedCtrl, CObject)

//------------------------------------------------------------------------------
CRegisteredParsedCtrl::CRegisteredParsedCtrl(
	const CString& sName,
	const CString& sLocalizedText,
	CRuntimeClass* pClass,
	DataType aDataType,
	DWORD wNeededStyle,
	DWORD wNeededExStyle,
	DWORD wNotWantedStyle,
	DWORD wNotWantedExStyle)
	:
	m_sName(sName),
	m_sLocalizedText(sLocalizedText),
	m_DataType(aDataType),
	m_pClass(pClass),
	m_wNeededStyle(wNeededStyle),
	m_wNeededExStyle(wNeededExStyle),
	m_wNotWantedStyle(wNotWantedStyle),
	m_wNotWantedExStyle(wNotWantedExStyle),
	m_pFamily(NULL)
{
}

//------------------------------------------------------------------------------
void CRegisteredParsedCtrl::SetFamily(CParsedCtrlFamily* pFamily)
{
	m_pFamily = pFamily;
}

//===========================================================================
//		class CParsedCtrlFamily
//===========================================================================
IMPLEMENT_DYNAMIC(CParsedCtrlFamily, CMapStringToOb)
//------------------------------------------------------------------------------
CParsedCtrlFamily::CParsedCtrlFamily(const CString& sName, BOOL bIsAParsobjFamily /*TRUE*/)
	: m_DefaultType(DataType::String)
{
	if (IsQualifiedName(sName))
		SplitQualifiedName(sName, m_sName, m_sNamespace, m_sAssemblyName, m_sVersioning);
	else
		m_sName = sName;
	m_bIsAParsObjFamily = bIsAParsobjFamily;
}

//------------------------------------------------------------------------------
CParsedCtrlFamily::~CParsedCtrlFamily()
{
	POSITION pos = GetStartPosition();
	while (pos)
	{
		CRegisteredParsedCtrl *pObject;
		CString sKey;
		GetNextAssoc(pos, sKey, (CObject*&)pObject);
		if (pObject) delete pObject;
	}
}

//------------------------------------------------------------------------------
const CString& CParsedCtrlFamily::GetName() const
{
	return m_sName;
}

//------------------------------------------------------------------------------
const CString& CParsedCtrlFamily::GetNamespace() const
{
	return m_sNamespace;
}

//------------------------------------------------------------------------------
const CString& CParsedCtrlFamily::GetVersioning() const
{
	return m_sVersioning;
}

//------------------------------------------------------------------------------
const CString& CParsedCtrlFamily::GetAssemblyName() const
{
	return m_sAssemblyName;
}

//------------------------------------------------------------------------------
BOOL CParsedCtrlFamily::IsAParsObjFamily() const
{
	return m_bIsAParsObjFamily;
}

//------------------------------------------------------------------------------
const CString& CParsedCtrlFamily::GetCaption() const
{
	return m_sCaption;
}

//------------------------------------------------------------------------------
const CString CParsedCtrlFamily::GetDefaultControl() const
{
	return GetDefaultControl(m_DefaultType);
}
//------------------------------------------------------------------------------
const CString CParsedCtrlFamily::GetDefaultControl(DataType aDataType) const
{
	CRegisteredParsedCtrl* pCtrl = GetRegisteredControl(aDataType);
	return pCtrl ? pCtrl->GetName() : _T("");
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlFamily::GetRegisteredControl(const CString& sName) const
{
	CRegisteredParsedCtrl* pObject = NULL;
	Lookup(sName, (CObject*&)pObject);

	return pObject;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlFamily::GetRegisteredControl(DataType aDataType) const
{
	CString sKey;
	CRegisteredParsedCtrl* pObject = NULL;
	for (int i = 0; i < m_arDefaultControls.GetCount(); i++)
	{
		pObject = m_arDefaultControls[i];
		if (pObject && MatchType(pObject->GetDataType(), aDataType))
			return pObject;
	}

	// se non l'ho trovato tra i default lo cerco in generale
	POSITION pos;
	pos = GetStartPosition();
	while (pos)
	{
		GetNextAssoc(pos, sKey, (CObject*&)pObject);
		if (pObject && pObject->GetDataType() == aDataType)
			return pObject;
	}

	return NULL;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlFamily::GetRegisteredControl(LPCSTR sClassName, DWORD wStyle, DWORD wExStyle) const
{
	POSITION pos;
	CString sKey;
	CRegisteredParsedCtrl* pObject = NULL;

	// i bitmap camblati nel codice li lasciamo a NULL
	if (_stricmp(sClassName, "CStatic") == 0 && (wStyle & SS_BITMAP) == SS_BITMAP)
		return NULL;

	DWORD wNeededStyle = 0, wNeededExStyle = 0, wNotWantedStyle = 0, wNotWantedExStyle = 0;
	pos = GetStartPosition();
	while (pos)
	{
		GetNextAssoc(pos, sKey, (CObject*&)pObject);
		if (!pObject)
			continue;

		wNeededStyle = pObject->GetNeededStyle();
		wNeededExStyle = pObject->GetNeededExStyle();
		wNotWantedStyle = pObject->GetNotWantedExStyle();
		wNotWantedExStyle = pObject->GetNotWantedExStyle();
		if (pObject->GetClass() == NULL || _stricmp(sClassName, pObject->GetClass()->m_lpszClassName) != 0)
			continue;

		if (
			(!wNeededStyle || ((wStyle & wNeededStyle) == wNeededStyle))//è richiesto uno stile
			&&
			(!wNeededExStyle || ((wExStyle & wNeededExStyle) == wNeededExStyle))//è richiesto uno stile esteso
			&&
			(!wNotWantedStyle || ((wStyle & wNotWantedStyle) != wNotWantedStyle))//non voglio uno stile
			&&
			(!wNotWantedExStyle || ((wExStyle & wNotWantedExStyle) != wNotWantedExStyle))//non voglio uno stile esteso
			)
			return pObject;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void CParsedCtrlFamily::GetAllRegisteredControls(CObArray& arControls) const
{
	POSITION pos;
	CString sKey;
	CRegisteredParsedCtrl* pObject = NULL;

	pos = GetStartPosition();
	while (pos)
	{
		GetNextAssoc(pos, sKey, (CObject*&)pObject);
		if (pObject)
			arControls.Add(pObject);
	}
}

//-----------------------------------------------------------------------------
/*static*/ void CParsedCtrlFamily::SplitQualifiedName
(
	const CString& sQualifiedName,
	CString& sName,
	CString& sNamespace,
	CString& sAssemblyName,
	CString& sVersioning
)
{
	// name
	int nFirstSep = sQualifiedName.Find(szCommaSeparator, 0);
	if (nFirstSep > 0)
		sName = sQualifiedName.Left(nFirstSep);
	else
		sName = sQualifiedName;

	if (sName.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	// namespace
	int nSecondSep = sName.ReverseFind('.');
	if (nSecondSep > 0)
	{
		sNamespace = sName.Left(nSecondSep);
		sName = sName.Mid(nSecondSep + 1);
	}

	// assembly name & versioning
	nSecondSep = sQualifiedName.Find(szCommaSeparator, nFirstSep + 1);
	if (nSecondSep > 0)
	{
		sAssemblyName = sQualifiedName.Mid(nFirstSep + 1, nSecondSep);
		sVersioning = sQualifiedName.Mid(nSecondSep + 1);
	}
	else if (nFirstSep >= 0)
		sAssemblyName = sQualifiedName.Mid(nFirstSep + 1);

	sName = sName.Trim();
	sNamespace = sNamespace.Trim();
	sAssemblyName = sAssemblyName.Trim();
	sVersioning = sVersioning.Trim();
}

//-----------------------------------------------------------------------------
/*static*/ BOOL CParsedCtrlFamily::IsQualifiedName(const CString& sQualifiedName)
{
	return sQualifiedName.FindOneOf(szCommaSeparator + CTBNamespace::GetSeparator()) > 0;
}

//-----------------------------------------------------------------------------
const CString CParsedCtrlFamily::GetQualifiedTypeName() const
{
	return	GetFullName() + szCommaSeparator + m_sAssemblyName + szCommaSeparator +
		(m_sVersioning.IsEmpty() ? szDefaultVersioning : m_sVersioning);
}

//-----------------------------------------------------------------------------
const CString CParsedCtrlFamily::GetFullName() const
{
	return (m_sNamespace.IsEmpty() ? m_sName : m_sNamespace + DOT_CHAR + m_sName);
}

//===========================================================================
//		class CParsedCtrlDataTypeDefault
//===========================================================================
class CParsedCtrlDataTypeDefault : public CObject
{
	friend class CParsedCtrlRegistry;

private:
	DataType m_DataType;
	CString	 m_sFamily;

public:
	CParsedCtrlDataTypeDefault(const DataType& aDataType) : m_DataType(aDataType) { }
};

//===========================================================================
//		class CParsedCtrlRegistry
//===========================================================================

//-----------------------------------------------------------------------------
CParsedCtrlRegistry::CParsedCtrlRegistry()
{
	// preparo il default
	m_pDefaultParsedControlFamily = new CParsedCtrlFamily(szMParsedControl);
	CRegisteredParsedCtrl* pCtrl = new CRegisteredParsedCtrl(szParsedCtrlDefaultFamily, szParsedCtrlDefaultFamily, NULL, DataType::String, 0, 0, 0, 0);
	pCtrl->SetFamily(m_pDefaultParsedControlFamily);
	m_pDefaultParsedControlFamily->SetAt(pCtrl->GetName(), pCtrl);

}

//-----------------------------------------------------------------------------
CParsedCtrlRegistry::~CParsedCtrlRegistry()
{
	delete m_pDefaultParsedControlFamily;
}

//-----------------------------------------------------------------------------
CParsedCtrlFamily* CParsedCtrlRegistry::GetFamily(const CString& sFamilyName) const
{
	TB_LOCK_FOR_READ();

	if (!GetSize())
		return NULL;

	CString sName(sFamilyName), sNamespace, sAssemblyName, sVersioning;
	if (CParsedCtrlFamily::IsQualifiedName(sFamilyName))
		CParsedCtrlFamily::SplitQualifiedName(sFamilyName, sName, sNamespace, sAssemblyName, sVersioning);

	CParsedCtrlFamily* pFamily = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pFamily = (CParsedCtrlFamily*)GetAt(i);

		// versioning is not manadory
		if (
			pFamily &&
			pFamily->GetName().CompareNoCase(sName) == 0 &&
			(sNamespace.IsEmpty() || pFamily->GetNamespace().CompareNoCase(sNamespace) == 0) &&
			(sAssemblyName.IsEmpty() || pFamily->GetAssemblyName().CompareNoCase(sAssemblyName) == 0)
			)
			return pFamily;
	}
	return NULL;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlRegistry::GetRegisteredControl(const CString& sFamily, const DataType& aDataType) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = GetFamily(sFamily);
	return pFamily ? pFamily->GetRegisteredControl(aDataType) : NULL;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlRegistry::GetRegisteredControl(const CString& sFamily, const CString& sName) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = GetFamily(sFamily);
	return pFamily ? pFamily->GetRegisteredControl(sName) : NULL;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlRegistry::GetRegisteredControl(CWnd* pOnwerWnd) const
{
	TB_LOCK_FOR_READ();

	if (IsExcludedControl(pOnwerWnd))
		return NULL;

	CRuntimeClass* pClass = pOnwerWnd->GetRuntimeClass();
	DWORD style = pOnwerWnd->GetStyle();
	DWORD exStyle = pOnwerWnd->GetExStyle();
	BOOL isAParsObj = GetParsedCtrl(pOnwerWnd) != NULL;

	if (strcmp(pClass->m_lpszClassName, "CWnd") == 0)
	{
		TCHAR szClassName[MAX_CLASS_NAME + 1];
		VERIFY(::GetClassName(pOnwerWnd->m_hWnd, szClassName, MAX_CLASS_NAME));
		if (_tcscmp(szClassName, _T("Static")) == 0)
			pClass = RUNTIME_CLASS(CStatic);
		else if (_tcscmp(szClassName, _T("Button")) == 0)
		{
			pClass = RUNTIME_CLASS(CButton);
			style = BS_TYPEMASK & ((CButton*)pOnwerWnd)->GetButtonStyle();
		}
		if (_tcscmp(szClassName, _T("ListBox")) == 0)
			pClass = RUNTIME_CLASS(CListBox);
	}

	while (pClass != NULL)
	{
		CParsedCtrlFamily* pFamily = NULL;
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			pFamily = (CParsedCtrlFamily*)GetAt(i);
			if (!pFamily)
				continue;

			if (isAParsObj && !pFamily->IsAParsObjFamily())
				continue;

			CRegisteredParsedCtrl* pCtrl = pFamily->GetRegisteredControl(pClass->m_lpszClassName, style, exStyle);
			if (pCtrl)
				return pCtrl;
		}
		pClass = pClass->m_pfnGetBaseClass
			? pClass->m_pfnGetBaseClass()
			: NULL;
	}

	return isAParsObj ? m_pDefaultParsedControlFamily->GetRegisteredControl(szParsedCtrlDefaultFamily) : NULL;
}

//------------------------------------------------------------------------------
const CParsedCtrlFamily* CParsedCtrlRegistry::GetRegisteredControlFamily(CWnd* pOnwerWnd) const
{
	CRegisteredParsedCtrl* pCtrl = GetRegisteredControl(pOnwerWnd);
	return pCtrl ? pCtrl->GetFamily() : NULL;
}

//------------------------------------------------------------------------------
CRegisteredParsedCtrl* CParsedCtrlRegistry::GetRegisteredControl(const CString& sName) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pFamily = (CParsedCtrlFamily*)GetAt(i);
		if (!pFamily)
			continue;

		CRegisteredParsedCtrl* pCtrl = pFamily->GetRegisteredControl(sName);
		if (pCtrl)
			return pCtrl;
	}

	return NULL;
}

//------------------------------------------------------------------------------
void CParsedCtrlRegistry::GetAllRegisteredControls(CObArray& arControls) const
{
	TB_LOCK_FOR_READ();

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CParsedCtrlFamily* pFamily = (CParsedCtrlFamily*)GetAt(i);
		if (pFamily)
			pFamily->GetAllRegisteredControls(arControls);
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::RegisterControlsFamily(
	const CString& sFamilyName,
	const CString& sCaption,
	const DataType& defaultType,
	const BOOL isAParsedControlFamily /*TRUE*/
)
{
	TB_LOCK_FOR_WRITE();

	CParsedCtrlFamily* pFamily = GetFamily(sFamilyName);
	if (!pFamily)
	{
		pFamily = new CParsedCtrlFamily(sFamilyName, isAParsedControlFamily);
		Add(pFamily);
	}
	pFamily->m_sCaption = sCaption;
	pFamily->m_DefaultType = defaultType;
}

//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::AddFamilyDefaultType(const CString& sFamilyName, CRegisteredParsedCtrl *pCtrl)
{
	TB_LOCK_FOR_WRITE();

	CParsedCtrlFamily* pFamily = GetFamily(sFamilyName);
	if (!pFamily)
	{
		pFamily = new CParsedCtrlFamily(sFamilyName);
		Add(pFamily);
	}
	pFamily->m_arDefaultControls.Add(pCtrl);
}

//-----------------------------------------------------------------------------
const CParsedCtrlFamily* CParsedCtrlRegistry::GetDefaultFamilyInfo(const DataType& aDataType) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlDataTypeDefault* pDefault;

	for (int i = 0; i <= m_DefaultDataTypeFamiles.GetUpperBound(); i++)
	{
		pDefault = (CParsedCtrlDataTypeDefault*)m_DefaultDataTypeFamiles.GetAt(i);

		if (pDefault && MatchType(pDefault->m_DataType, aDataType))
			return GetFamily(pDefault->m_sFamily);
	}

	return NULL;
}

//-----------------------------------------------------------------------------
const CString CParsedCtrlRegistry::GetFamilyDefaultControl(const CString& sFamilyName) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = GetFamily(sFamilyName);
	return pFamily ? pFamily->GetDefaultControl() : _T("");
}

//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::SetTypeDefaultFamily(const DataType& aDataType, const CString& sFamilyName)
{
	TB_LOCK_FOR_WRITE();
	CParsedCtrlDataTypeDefault* pExistingDefault = NULL;

	for (int i = 0; i <= m_DefaultDataTypeFamiles.GetUpperBound(); i++)
	{
		CParsedCtrlDataTypeDefault* pDefault = (CParsedCtrlDataTypeDefault*)m_DefaultDataTypeFamiles.GetAt(i);
		if (pDefault && pDefault->m_DataType == aDataType)
		{
			pExistingDefault = pDefault;
			break;
		}
	}

	if (!pExistingDefault)
	{
		pExistingDefault = new CParsedCtrlDataTypeDefault(aDataType);
		m_DefaultDataTypeFamiles.Add(pExistingDefault);
	}

	pExistingDefault->m_sFamily = sFamilyName;
}


//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::RegisterParsedControl(const CString& sFamilyName, CRegisteredParsedCtrl* pCtrl)
{
	TB_LOCK_FOR_WRITE();

	CParsedCtrlFamily* pFamily = GetFamily(sFamilyName);
	if (!pFamily)
	{
		pFamily = new CParsedCtrlFamily(sFamilyName);
		Add(pFamily);
	}
	pCtrl->SetFamily(pFamily);
	pFamily->SetAt(pCtrl->GetName(), pCtrl);
}

//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::GetRegisteredAssemblies(CStringArray& arNames) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = NULL;
	BOOL bFound = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pFamily = (CParsedCtrlFamily*)GetAt(i);
		if (!pFamily)
			continue;

		bFound = FALSE;
		for (int n = 0; n <= arNames.GetUpperBound(); n++)
		{
			if (arNames.GetAt(n).CompareNoCase(pFamily->GetAssemblyName()) == 0)
			{
				bFound = TRUE;
				break;
			}
		}

		if (!bFound)
			arNames.Add(pFamily->GetAssemblyName());
	}
}

//-----------------------------------------------------------------------------
void CParsedCtrlRegistry::GetRegisteredFamilies(const CString& sAssemblyName, CObArray& arFamiles) const
{
	TB_LOCK_FOR_READ();

	CParsedCtrlFamily* pFamily = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pFamily = (CParsedCtrlFamily*)GetAt(i);

		if (pFamily && pFamily->GetAssemblyName().CompareNoCase(sAssemblyName) == 0)
			arFamiles.Add(pFamily);
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedCtrlRegistry::IsExcludedControl(CWnd* pWnd) const
{
	return pWnd->IsKindOf(RUNTIME_CLASS(CChildButton));
}

//===========================================================================
CParsedCtrlRegistryConstPtr AfxGetParsedControlsRegistry()
{
	return CParsedCtrlRegistryConstPtr(AfxGetApplicationContext()->GetObject<CParsedCtrlRegistry>(&CApplicationContext::GetParsedControlsRegistry), FALSE);
}

//------------------------------------------------------------------------------
CParsedCtrlRegistryPtr AfxGetWritableParsedControlsRegistry()
{
	return CParsedCtrlRegistryPtr(AfxGetApplicationContext()->GetObject<CParsedCtrlRegistry>(&CApplicationContext::GetParsedControlsRegistry), TRUE);
}

///////////////////////////////////////////////////////////////////////////////
//// CParsedCtrlDropTarget 
// classe generica per abilitare il DragDrop su un ParsedControl
// per ora si occupa solo di fare la registrazione del comando
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CParsedCtrlDropTarget, CCmdTarget)

//-----------------------------------------------------------------------------
CParsedCtrlDropTarget::CParsedCtrlDropTarget()
	:
	m_pTarget(NULL)
{
}

//-----------------------------------------------------------------------------
void CParsedCtrlDropTarget::AttachTarget(CParsedCtrl* pCtrl)
{
	m_pTarget = pCtrl;

	ASSERT(m_pTarget);
	Register(m_pTarget->GetCtrlCWnd());
}

///////////////////////////////////////////////////////////////////////////////
// CParsedCtrlDropFilesTarget 
// classe specifica per abilitare il Drag-Drop di files su un ParsedCtrl
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CParsedCtrlDropFilesTarget, CParsedCtrlDropTarget)

//------------------------------------------------------------------------------
CParsedCtrlDropFilesTarget::CParsedCtrlDropFilesTarget()
	:
	m_uiFileGroupDescriptor(NULL),
	m_uiFileContents(NULL)
{
	// This format identifier is used with the CFSTR_FILECONTENTS format to transfer data as a group of files. (va fatto sempre e vale anche per le email!)
	m_uiFileGroupDescriptor = RegisterClipboardFormat(CFSTR_FILEDESCRIPTOR);

	EnableMsgFiles();
	SetTemporaryPath(AfxGetPathFinder()->GetAppDataPath(TRUE));
}

//------------------------------------------------------------------------------
void CParsedCtrlDropFilesTarget::EnableMsgFiles(BOOL bEnable /*= TRUE*/, BOOL bDeleteTempFiles /*= TRUE*/)
{
	m_bEnableMsgFiles = bEnable;
	m_bDeleteTempFiles = bDeleteTempFiles;

	if (m_bEnableMsgFiles)
		m_uiFileContents = RegisterClipboardFormat(CFSTR_FILECONTENTS);
}

//------------------------------------------------------------------------------
void CParsedCtrlDropFilesTarget::SetTemporaryPath(CString sTemporaryPath)
{
	if (sTemporaryPath.IsEmpty())
		ASSERT(FALSE); // il path non puo' essere empty!
	else
		m_sTemporaryPath = sTemporaryPath;
}

//------------------------------------------------------------------------------
BOOL CParsedCtrlDropFilesTarget::OnDrop(CWnd* pWnd, COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point)
{
	BOOL bDeleteFiles = FALSE;

	CStringArray aDroppedFiles;
	m_SubFolderBehaviour = UNDEFINED_DEEP;

	HGLOBAL hg = pDataObject->GetGlobalData(CF_HDROP);
	HDROP hDrop = NULL;

	if (hg != NULL)
	{
		// drop file from filesystem
		hDrop = (HDROP)GlobalLock(hg);
		UINT nNumOfFiles = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, NULL);

		if (nNumOfFiles > 0)
		{
			for (int i = 0; i < (int)nNumOfFiles; ++i)
			{
				CString strFile;
				UINT nFilenameSize = DragQueryFile(hDrop, i, NULL, NULL);
				DragQueryFile(hDrop, i, strFile.GetBuffer(nFilenameSize + 1), nFilenameSize + 1);
				strFile.ReleaseBuffer();
				GetAllFiles(strFile, aDroppedFiles);
			}
		}
	}
	else
		if (m_bEnableMsgFiles)
		{
			// drop file from Outlook
			bDeleteFiles = m_bDeleteTempFiles;

			hg = pDataObject->GetGlobalData(m_uiFileGroupDescriptor);
			if (hg != NULL)
			{
				SIZE_T size = GlobalSize(hg);
				void *pData = GlobalLock(hg);
				FILEGROUPDESCRIPTORW *pgd = (FILEGROUPDESCRIPTORW*)pData;

				FORMATETC fmt;
				ZeroMemory(&fmt, sizeof(fmt));

				fmt.cfFormat = m_uiFileContents;
				fmt.dwAspect = DVASPECT_CONTENT;
				fmt.tymed = TYMED_ISTORAGE | TYMED_ISTREAM;

				for (UINT i = 0; i < pgd->cItems; i++)
				{
					CString sExtension = GetExtension(pgd->fgd[i].cFileName);
					CString sFileName = m_sTemporaryPath + SLASH_CHAR + GetNameWithExtension(pgd->fgd[i].cFileName);
					if (ExistFile(sFileName))
						sFileName = m_sTemporaryPath + SLASH_CHAR + GetName(pgd->fgd[i].cFileName) + cwsprintf(_T("_%d"), i) + sExtension;

					// anche se la struttura rinasce serve ad indicare quale nr di file andare a considerare per estrarre il contenuto (in caso di drag di n file)
					fmt.lindex = i; // deve rimanere cosi!!! 

					STGMEDIUM stg;
					ZeroMemory(&stg, sizeof(stg));

					if (pDataObject->GetData(m_uiFileContents, &stg, &fmt))
					{
						switch (stg.tymed)
						{
							// drop Outlook msg file
						case TYMED_ISTORAGE:
						{
							IStorage *msg = (IStorage*)stg.pstg;
							IStorage *file;

							HRESULT hr = StgCreateDocfile(sFileName, STGM_CREATE | STGM_READWRITE | STGM_SHARE_EXCLUSIVE, 0, &file);
							if SUCCEEDED(hr)
							{
								IID ttid;
								hr = msg->CopyTo(0, &ttid, NULL, file);
								if SUCCEEDED(hr)
									file->Commit(0);
								file->Release();
							}
							break;
						}
						// drop attachment to Outlook msg file
						case TYMED_ISTREAM:
						{
							CFile *pFile = pDataObject->GetFileData(m_uiFileContents, &fmt);
							HRESULT hr = pFile->Open(stg.lpszFileName, CFile::modeReadWrite);

							if SUCCEEDED(hr)
							{
								UINT fileLength = (UINT)pFile->GetLength() * sizeof(TCHAR);
								TCHAR* pBuff = new TCHAR[fileLength + 1];
								int nCount = pFile->Read(pBuff, fileLength);

								if (pBuff)
								{
									CFile fileDest;
									hr = fileDest.Open(sFileName, CFile::modeCreate | CFile::modeReadWrite);
									if SUCCEEDED(hr)
									{
										fileDest.Write(pBuff, nCount);
										fileDest.Flush();
										fileDest.Close();
									}
								}
								// - delete the file object
								delete pFile;
								delete pBuff;
							}
							break;
						}

						default:
							break;
						}

						ReleaseStgMedium(&stg);
					}

					aDroppedFiles.Add(sFileName);
				}
			}
		}

	if (hg != NULL)
		GlobalUnlock(hg);

	m_pTarget->OnDropFiles(&aDroppedFiles);

	if (bDeleteFiles)
		for (int i = 0; i < aDroppedFiles.GetCount(); i++)
			DeleteFile(aDroppedFiles.GetAt(i));

	return TRUE;
}

// metodo ricorsivo che ritorna tutti i file presenti nel folder + subfolder
//------------------------------------------------------------------------------
void CParsedCtrlDropFilesTarget::GetAllFiles(CString strPath, CStringArray& aDroppedFiles)
{
	DWORD dwFileAttr = GetTbFileAttributes(strPath);
	if (dwFileAttr & FILE_ATTRIBUTE_DIRECTORY)
	{
		CStringArray pSubfolders;
		GetSubFolders(strPath, &pSubfolders); // ritorna SOLO il nome del folder!

		int dirCount = pSubfolders.GetCount();
		if (dirCount > 0 && (m_SubFolderBehaviour == UNDEFINED_DEEP))
			m_SubFolderBehaviour = m_pTarget->OnSubFolderFound() ? DEEP : NO_DEEP;

		// vado in ricorsione sui folder figli solo se l'utente mi ha dato l'OK
		if (m_SubFolderBehaviour == DEEP)
		{
			for (int i = 0; i < dirCount; i++)
			{
				// devo comporre il fullpath
				CString fullDirPath = strPath + SLASH_CHAR + pSubfolders.GetAt(i);
				GetAllFiles(fullDirPath, aDroppedFiles);
			}
		}

		CStringArray arFiles;
		GetFiles(strPath, _T("*.*"), &arFiles);
		for (int i = 0; i < arFiles.GetCount(); i++)
			GetAllFiles(arFiles.GetAt(i), aDroppedFiles);
	}
	else
		aDroppedFiles.Add(strPath);
}

//------------------------------------------------------------------------------
DROPEFFECT CParsedCtrlDropFilesTarget::OnDragEnter(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	return DROPEFFECT_COPY;
}

//------------------------------------------------------------------------------
DROPEFFECT CParsedCtrlDropFilesTarget::OnDragOver(CWnd* pWnd, COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	// vedi Register nel costruttore
	if (pDataObject->IsDataAvailable(m_uiFileGroupDescriptor))
		return DROPEFFECT_COPY; // data fits

	// secondo tentativo fix IsDataAvailable che non riconosce il drop dei file in Windows 7
	CFile *pFile = pDataObject->GetFileData(CF_HDROP);
	if (pFile != NULL)
	{
		delete pFile;
		return DROPEFFECT_COPY; // data fits
	}

	return DROPEFFECT_NONE; // data won't fit
}
