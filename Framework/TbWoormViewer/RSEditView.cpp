#include "stdafx.h"

#include <TbFrameworkImages/CommonImages.h>
#include <TbFrameworkImages/GeneralFunctions.h>

#include <TbGenlib/TBPropertyGrid.h>
#include <TbGenlib/BaseApp.h>

#include <TbGenlibUI/FontsDialog.h>
#include <TbGenlibUI/FormatDialog.h>
#include <TbGenlib/TBPropertyGrid.h>
#include <TbOleDb/SqlCatalog.h>
#include <TbOleDb/SqlRec.h>

#include <TbWoormEngine/rpsymtbl.h>
#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/QueryObject.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/edtmng.h>
#include <TbWoormEngine/rpsymtbl.h>
#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/events.h>

#include "WoormDoc.h"
#include "WoormFrm.h"

#include "RSEditView.h"
#include "RSEditorUI.h"
#include "RSEditor_Property.h"

//#include "RSEditorUI.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#define RSEDIT_NS	L"Framework.TbWoormViewer.TbWoormViewer.RSEdit.Button"
#define RSREFRESH_NS	L"Framework.TbWoormViewer.TbWoormViewer.RSRefresh.Button"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CRSGridDockPane, CRSDockPane);
IMPLEMENT_DYNCREATE(CRSParametersDockPane, CRSDockPane);
IMPLEMENT_DYNCREATE(CRSSymbolTableDockPaneDebug, CRSDockPane);
IMPLEMENT_DYNCREATE(CRSBreakpointDockPaneDebug, CRSDockPane);
IMPLEMENT_DYNCREATE(CRSEditorDiagnosticDockPane, CRSDockPane);

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorPreviewDockPane, CRSDockPane);
BEGIN_MESSAGE_MAP(CRSEditorPreviewDockPane, CRSDockPane)
	ON_BN_CLICKED(ID_RS_REFRESH, OnPreview)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CRSEditorPreviewDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_REFRESH, RSREFRESH_NS, TBIcon(szIconRefresh, TOOLBAR), _TB("Preview"));
}

//-----------------------------------------------------------------------------
void CRSEditorPreviewDockPane::OnPreview()
{
	CRSEditorPreviewView* previewView = (CRSEditorPreviewView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorPreviewView));
	if (!previewView)
		return;
	previewView->OnPreview();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorToolDockPane, CRSDockPane);
IMPLEMENT_DYNCREATE(CRSEditorToolDockDebugPane, CRSEditorToolDockPane);

//-----------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(CRSEditorToolDockPane, CRSDockPane)

	ON_BN_CLICKED(ID_RS_EDIT, OnEdit)
	ON_BN_CLICKED(ID_RS_MORE, OnMore)
	ON_BN_CLICKED(ID_RS_FILTER, OnFilter)
	ON_BN_CLICKED(ID_RS_ADD_HIDDEN_VAR_FROM_DB, OnAdd)
	ON_BN_CLICKED(ID_RS_DELETE, OnDelete)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_EDIT, RSEDIT_NS, TBIcon(szIconEdit, TOOLBAR), _TB("Edit"));

	m_pToolBar->AddButton(ID_RS_MORE, RSEDIT_NS, TBIcon(szIconMore, TOOLBAR), _TB("Group"), _TB("\nDB Tables/Views grouped by modules"));
	m_pToolBar->PressButton(ID_RS_MORE, TRUE);

	m_pToolBar->AddButton(ID_RS_FILTER, RSEDIT_NS, TBIcon(szIconFilterRS, TOOLBAR), _TB("Filter"), _TB("\nShow only matching table name"));

	m_pToolBar->AddButtonToRight(ID_RS_ADD_HIDDEN_VAR_FROM_DB, RSEDIT_NS, TBIcon(szIconAdd, TOOLBAR), _TB("Add Hidden"), _TB("\nSelect columns to create new hidden variables"));

	m_pToolBar->AddButton(ID_RS_DELETE, RSEDIT_NS, TBIcon(szIconDelete, TOOLBAR), _TB("Delete"), _TB("\nDelete the selected object"));
	m_pToolBar->AddSeparatorBefore(ID_RS_DELETE);
}

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnEdit()
{
	CRSEditorToolView* toolBoxView = (CRSEditorToolView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorToolView));
	if (!toolBoxView)
		return;
	toolBoxView->OnEdit();
}

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnMore()
{
	CRSEditorToolView* toolBoxView = (CRSEditorToolView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorToolView));
	if (!toolBoxView)
		return;
	toolBoxView->OnMore();
}

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnFilter()
{
	CRSEditorToolView* toolBoxView = (CRSEditorToolView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorToolView));
	if (!toolBoxView)
		return;
	toolBoxView->OnFilter();
}

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnAdd()
{
	CRSEditorToolView* toolBoxView = (CRSEditorToolView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorToolView));
	if (!toolBoxView)
		return;
	toolBoxView->OnAdd();
}

//-----------------------------------------------------------------------------
void CRSEditorToolDockPane::OnDelete()
{
	CRSEditorToolView* toolBoxView = (CRSEditorToolView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSEditorToolView));
	if (!toolBoxView)
		return;
	toolBoxView->OnDelete();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorFrame, CAbstractFrame);

BEGIN_MESSAGE_MAP(CRSEditorFrame, CAbstractFrame)

	ON_WM_CREATE()

	ON_COMMAND(ID_RS_EDITOR_CAPTIONBAR_BUTTON, OnClickCaptionBarButton)
	ON_REGISTERED_MESSAGE(BCGM_ON_CLICK_CAPTIONBAR_HYPERLINK, OnClickCaptionBarHyperlink)

	ON_COMMAND(ID_EDIT_TOGGLEBOOKMARK, OnEditTogglebookmark)
	ON_COMMAND(ID_EDIT_CLEAR_ALLBOOKMARKS, OnClearAllBookmarks)
	ON_UPDATE_COMMAND_UI(ID_EDIT_CLEAR_ALLBOOKMARKS, OnUpdateBookmarkBtns)
	ON_COMMAND(ID_EDIT_NEXTBOOKMARK, OnEditNextbookmark)
	ON_UPDATE_COMMAND_UI(ID_EDIT_NEXTBOOKMARK, OnUpdateBookmarkBtns)
	ON_COMMAND(ID_EDIT_PREVIOUSBOOKMARK, OnEditPreviousbookmark)
	ON_UPDATE_COMMAND_UI(ID_EDIT_PREVIOUSBOOKMARK, OnUpdateBookmarkBtns)

	ON_COMMAND(ID_EDIT_CHECK, OnCheckPressed)
	ON_COMMAND(ID_EDIT_EXEC, OnExecPressed)

	ON_COMMAND(ID_EDIT_FIND_COMBO, OnFindNext)
	ON_COMMAND(ID_EDIT_REPLACE_COMBO, OnReplace)
	ON_COMMAND(ID_EDIT_FINDPREV, OnFindPrev)
	ON_COMMAND(ID_EDIT_FINDNEXT, OnFindNext)
	ON_COMMAND(ID_EDIT_REPLACEONE, OnReplace)
	ON_COMMAND(ID_EDIT_REPLACEALL, OnReplaceAll)

	//---------------------------------------
	ON_WM_CLOSE	()
	ON_COMMAND	(ID_RS_CLOSE, OnClose)

	ON_COMMAND	(ID_RS_SAVE, OnSave)

END_MESSAGE_MAP()
//-----------------------------------------------------------------------------

CRSEditorFrame::CRSEditorFrame()
	:
	m_pToolPane(NULL),
	m_pToolTreeView(NULL),
	m_pDiagnosticView(NULL),
	m_pEditView(NULL),
	m_pGridView(NULL),
	m_pMainToolBar(NULL),
	m_pParametersPane(NULL),
	m_pGridPane(NULL)
{
	ASSERT(m_pTabbedToolBar == NULL);

	SetDockable(FALSE);	
}

CRSEditorFrame::~CRSEditorFrame()
{
	//no è deletata da ~CAbstractFrame () tramite la Tabbedtoolbar SAFE_DELETE(m_pMainToolBar);

	if (m_pEditView)
	{
		ASSERT(FALSE);
	}
}

BOOL CRSEditorFrame::PreTranslateMessage(MSG* pMsg)
{
	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000 ;

	if (pMsg->wParam == VK_ESCAPE && (pMsg->message == WM_KEYDOWN))
	{

		if (m_pEditView->GetEditCtrl()->GetIntellisenseWnd())
		{
			CPoint pt = m_pEditView->GetEditCtrl()->GetCaretPos();

			m_pEditView->GetEditCtrl()->GetIntellisenseWnd()->DestroyWindow();
			m_pEditView->SetFocus();

			m_pEditView->GetEditCtrl()->SetCaretPos(pt);

			return TRUE;
		}
	}

	if (shiftPressed)
	{
		CBCGPIntelliSenseWnd * wnd = m_pEditView->GetEditCtrl()->GetIntellisenseWnd();
		if (m_pEditView->GetEditCtrl()->IsIntellisenseActive() && !m_pEditView->GetEditCtrl()->m_bForceIntellisense)
			m_pEditView->GetEditCtrl()-> m_bForceIntellisense = TRUE;

	}
	

	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
BOOL CRSEditorFrame::CreateAuxObjects(CCreateContext* pCreateContext)
{
	m_pToolPane = (CRSEditorToolDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSEditorToolDockPane), IDC_RS_EditorToolDockPane, _T("Tool"), _T("Tool"), CBRS_LEFT , CSize(300, 700));
	m_pToolPane->EnableToolbar(TOOLBAR_HEIGHT, FALSE);	//GetDocument()->GetWoormFrame()->ShowPanelToolbarText());
	m_pToolPane->SetAutoHideMode(TRUE, CBRS_LEFT | CBRS_HIDE_INPLACE);

	m_pDiagnosticPane = (CRSEditorDiagnosticDockPane*) CreateDockingPane(RUNTIME_CLASS(CRSEditorDiagnosticDockPane), IDC_RS_EditorDiagnosticDockPane, _T("Output"), _T("Output"), CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE, CSize(500, 200));
	if (m_pDiagnosticPane)
	{
		m_pDiagnosticPane->SetAutoHideMode(TRUE, CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE);
		m_pDiagnosticPane->CanBeClosed();
		m_pDiagnosticPane->EnableDocking(CBRS_ALIGN_BOTTOM);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnFrameCreated()
{
	SetWoormEditorVisible(false);

	__super::OnFrameCreated();

	ScaleFrame((CFrameWnd*) this, TRUE);

	if (m_pTabbedToolBar)
		m_pTabbedToolBar->AdjustLayout();
}

//-----------------------------------------------------------------------------
int CRSEditorFrame::OnCreate(LPCREATESTRUCT lpcs)
{
	if (__super::OnCreate(lpcs) == -1)
	{
		ASSERT_TRACE(FALSE, "CRSEditorFrame::OnCreate\n\tframe class not created.\n");
		return -1;
	}

	m_pTabbedToolBar = new CTBTabbedToolbar();
	if (!m_pTabbedToolBar->Create(this))
		return -1;
	m_pTabbedToolBar->SetWindowText(_T("Tabbed"));

	m_pMainToolBar = new CTBToolBar();
	if (!m_pMainToolBar->CreateEmptyTabbedToolbar(this, szToolbarNameMain, _TB("Main")))
	{
		ASSERT_TRACE(FALSE, "Failed to create the main toolBar.\n");
		return -1;
	}
	//----
	m_pMainToolBar->AddButton(ID_RS_CLOSE, RSEDIT_NS, TBIcon(szIconUndoFilled, TOOLBAR), _TB("Back"), _TB("Esc"));
	m_pMainToolBar->AddSeparator();

	m_pMainToolBar->AddButton(ID_RS_SAVE,		RSEDIT_NS, TBIcon(szIconSaveFilled, TOOLBAR), _TB("Apply"), _TB("Syntax Check and apply on success"));

	m_pMainToolBar->AddButton(ID_EDIT_CHECK,  	RSEDIT_NS, TBIcon(szIconCheckFilled, TOOLBAR), _TB("Test"), _TB("Syntax Check and prepare test on success"));
	m_pMainToolBar->HideButton(ID_EDIT_CHECK);

	m_pMainToolBar->AddButton(ID_EDIT_EXEC,		RSEDIT_NS, TBIcon(szIconPlayFilled, TOOLBAR), _TB("Execute"), _TB("Execute query"));
	m_pMainToolBar->HideButton(ID_EDIT_EXEC);

	m_pMainToolBar->AddSeparator();

	//----
	m_pMainToolBar->AddComboBox(ID_EDIT_FIND_COMBO, RSEDIT_NS, 110, CBS_DROPDOWN | (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | WS_VSCROLL | CBS_SORT), _TB("Search:"), -1, _TB("Search..."));

	m_pMainToolBar->AddComboBox(ID_EDIT_REPLACE_COMBO, RSEDIT_NS, 110, CBS_DROPDOWN | (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | WS_VSCROLL | CBS_SORT), _TB("Replace:"), -1, _TB("Replace with..."));

	m_pMainToolBar->AddButton(ID_EDIT_FINDPREV,		RSEDIT_NS, TBIcon(szIconArrowPrevFilled,	TOOLBAR), _TB("Previous"), L"Ctrl+F3");
	m_pMainToolBar->AddButton(ID_EDIT_FINDNEXT,		RSEDIT_NS, TBIcon(szIconArrowNextFilled,	TOOLBAR), _TB("Next"), L"F3");
	m_pMainToolBar->AddButton(ID_EDIT_REPLACEONE,	RSEDIT_NS, TBIcon(szIconReplaceFilled,		TOOLBAR), _TB("Replace"), L"Ctrl+H");
	m_pMainToolBar->AddButton(ID_EDIT_REPLACEALL,	RSEDIT_NS, TBIcon(szIconReplaceAllFilled,	TOOLBAR), _TB("Replace All"));

	//----

	if (IsKindOf(RUNTIME_CLASS(CRSEditorFrameFullText)))
	{
		m_pMainToolBar->HideButton(ID_EDIT_CHECK, FALSE);

		m_pMainToolBar->AddButtonToRight(ID_EDIT_TOGGLEBOOKMARK, RSEDIT_NS, TBIcon(szIconBookmarkFilled, TOOLBAR), _TB("Toggle bookmark"));
		m_pMainToolBar->AddButtonToRight(ID_EDIT_CLEAR_ALLBOOKMARKS, RSEDIT_NS, TBIcon(szIconBookmarkCancelFilled, TOOLBAR), _TB("Clear all bookmark"));
		m_pMainToolBar->AddButtonToRight(ID_EDIT_PREVIOUSBOOKMARK, RSEDIT_NS, TBIcon(szIconBookmarkPrevFilled, TOOLBAR), _TB("Previous bookmark"));
		m_pMainToolBar->AddButtonToRight(ID_EDIT_NEXTBOOKMARK, RSEDIT_NS, TBIcon(szIconBookmarkNextFilled, TOOLBAR), _TB("Next bookmark"));

		m_pMainToolBar->AddSeparatorAfter(ID_EDIT_NEXTBOOKMARK);
	}

	m_pMainToolBar->SetAutoHideToolBarButton(TRUE);

	m_pTabbedToolBar->AddTab(m_pMainToolBar);
	m_pTabbedToolBar->AdjustLayout();

	CreateCaptionBar();
	return 0;
}

//-----------------------------------------------------------------------------
CBCGPToolbarComboBoxButton* CRSEditorFrame::GetFindCombo()
{
	CBCGPToolbarComboBoxButton* pFindCombo = NULL;

	CObList listButtons;
	if (CBCGPToolBar::GetCommandButtons(ID_EDIT_FIND_COMBO, listButtons) > 0)
	{
		for (POSITION posCombo = listButtons.GetHeadPosition();
			pFindCombo == NULL && posCombo != NULL;)
		{
			CBCGPToolbarComboBoxButton* pCombo =
				DYNAMIC_DOWNCAST(CBCGPToolbarComboBoxButton, listButtons.GetNext(posCombo));

			if (pCombo)
			{
				pFindCombo = pCombo;
			}
		}
	}

	return pFindCombo;
}

//-----------------------------------------------------------------------------
CBCGPToolbarComboBoxButton* CRSEditorFrame::GetReplaceCombo()
{
	CBCGPToolbarComboBoxButton* pRepCombo = NULL;

	CObList listButtons;
	if (CBCGPToolBar::GetCommandButtons(ID_EDIT_REPLACE_COMBO, listButtons) > 0)
	{
		for (POSITION posCombo = listButtons.GetHeadPosition();
		pRepCombo == NULL && posCombo != NULL;)
		{
			CBCGPToolbarComboBoxButton* pCombo =
				DYNAMIC_DOWNCAST(CBCGPToolbarComboBoxButton, listButtons.GetNext(posCombo));

			if (pCombo)
			{
				pRepCombo = pCombo;
			}
		}
	}

	return pRepCombo;
}

//-----------------------------------------------------------------------------
BOOL CRSEditorFrame::CreateCaptionBar()
{
	/*if (!m_CaptionBar.Create(WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS,
		this, ID_RS_EDITOR_CAPTIONBAR, -1, TRUE))
	{
		TRACE0("Failed to create caption bar\n");
		return FALSE;
	}

	m_CaptionBar.SetButton(_T("TODO..."), ID_RS_EDITOR_CAPTIONBAR_BUTTON, CBCGPCaptionBar::ALIGN_LEFT, FALSE);
	m_CaptionBar.SetButtonToolTip(_T("Click here to do something"));

	m_CaptionBar.SetText(_T("Edit something! \aClick here\a to visit Microarea HelpCenter Web site."), CBCGPCaptionBar::ALIGN_LEFT);*/

	//m_CaptionBar.SetBitmap(IDB_INFO, (COLORREF)-1, FALSE, CBCGPCaptionBar::ALIGN_LEFT, TRUE);
	//m_CaptionBar.SetImageToolTip(_T("Important"), _T("Please take a look at BCGPMSOfficeDemo source code to learn how to create advanced user interface in minutes."));

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnClickCaptionBarButton()
{
	//TODO
}

//-----------------------------------------------------------------------------
LRESULT CRSEditorFrame::OnClickCaptionBarHyperlink(WPARAM, LPARAM)
{
	::ShellExecute(NULL, NULL, _T("http://www.microarea.it/MicroareaHelpCenter/Home-Framework-TBWoormEngine(it-IT).ashx"), NULL, NULL, NULL);
	return 0;
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnEditTogglebookmark()
{
	m_pEditView->OnEditTogglebookmark();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnClearAllBookmarks()
{
	m_pEditView->OnClearAllBookmarks();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnEditNextbookmark()
{
	m_pEditView->OnEditNextbookmark();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnCheckPressed()
{
	if (m_pEditView->m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_FULL_REPORT)
	{
		m_pEditView->CheckFullReport();
	}
	else if (
		m_pEditView->m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE
		&&
		(
			m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_NAMED_QUERY
			||
			m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
			)
		)
	{
		if (!m_pParametersPane)
		{
			m_pParametersPane = (CRSParametersDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSParametersDockPane), IDC_RS_EditorParametersDockPane, _T("Parameters"), _T("Parameters"), CBRS_ALIGN_RIGHT, CSize(300, 700), NULL);
			if (m_pParametersPane)
				m_pParametersPane->SetAutoHideMode(FALSE, CBRS_ALIGN_RIGHT);
			UpdateWindow();
		}

		if (m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_NAMED_QUERY)
			m_pEditView->CheckNamedQuery();
		else if (m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
			m_pEditView->CheckRuleQuery();
	}
	else
		OnSave();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::EnablePreviewPanel()
{
	if (!m_pPreviewPane && m_pEditView && m_pEditView->m_bStringPreviewEnabled)
	{
		m_pPreviewPane = (CRSEditorPreviewDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSEditorPreviewDockPane), IDC_RS_EditorPreviewDockPane, _T("Preview"), _T("Preview"), CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE, CSize(500, 200)/*, pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE*/);
		if (m_pPreviewPane)
		{
			m_pPreviewPane->EnableToolbar(TOOLBAR_HEIGHT, FALSE);
			m_pPreviewPane->SetAutoHideMode(FALSE, CBRS_ALIGN_BOTTOM);
			m_pPreviewPane->CanBeClosed();
			m_pPreviewPane->EnableDocking(CBRS_ALIGN_BOTTOM);
		}

		if (m_pDiagnosticPane && m_pPreviewPane)
		{
			CBCGPDockingControlBar* pDownTabbedBar = NULL;
			m_pPreviewPane->AttachToTabWnd(m_pDiagnosticPane, BCGP_DM_SHOW, FALSE, &pDownTabbedBar);
			pDownTabbedBar->SetBCGStyle(pDownTabbedBar->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
			pDownTabbedBar->UpdateWindow();
		}
		UpdateWindow();
	}
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnExecPressed()
{
	if (!m_pGridPane)
	{
		m_pGridPane = (CRSGridDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSGridDockPane), IDC_RS_EditorGridDockPane, _T("Data:"), _T("Data:"), CBRS_ALIGN_BOTTOM, CSize(500, 200));
		if (m_pGridPane)
			m_pGridPane->SetAutoHideMode(FALSE, CBRS_ALIGN_BOTTOM);
		UpdateWindow();
	}

	if (m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_NAMED_QUERY)
		m_pEditView->ExecuteNamedQuery();
	else if (m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
		m_pEditView->ExecuteRuleQuery();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnEditPreviousbookmark()
{
	m_pEditView->OnEditPreviousbookmark();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnUpdateBookmarkBtns(CCmdUI* pCmdUI)
{
	BOOL bExistBookmarks = m_pEditView->GetEditCtrl()->GetMarkerList().GetCount() > 0;

	pCmdUI->Enable(bExistBookmarks);
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnReplace()
{
	m_pEditView->OnReplace();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnReplaceAll()
{
	m_pEditView->OnReplaceAll();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnFindNext()
{
	m_pEditView->OnFindNext();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnFindPrev()
{
	m_pEditView->OnFindPrev();
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnSave()
{
	if (m_pEditView->DoSave())
	{
		if (m_pEditView->DoClose())
		{
			m_pEditView->m_bBusy = FALSE;
			__super::OnClose();
		}
	}	
}

//-----------------------------------------------------------------------------
void CRSEditorFrame::OnClose()
{
	CWoormDocMng* pDoc = (CWoormDocMng*) GetDocument();
	ASSERT_VALID(pDoc);
	BOOL isDocModified = pDoc->IsModified();

	// NO REFACTORING
	// Perche la frame di EditView non ha il suo documento, 
	//al momento della distruzine va controllato il CWoormdoc IsModified() e non va bene
	if (m_pEditView->m_Context.m_eType != CRSEditViewParameters::EditorMode::EM_WRONG)
	{
		if (m_pEditView->GetEditCtrl()->IsModified())
		{
			int result = AfxMessageBox(_TB("Do you want to apply changes before exit?"), MB_YESNOCANCEL);
			if (result == IDYES)
			{
				if (!m_pEditView->DoSave())
					return;
			}
			else if (result == IDCANCEL)
				return;
		}
	}

	//pDoc->SetModifiedFlag(FALSE);

	if (m_pEditView->DoClose())
	{
		m_pEditView->m_bBusy = FALSE;
		__super::OnClose();
	}

	//ASSERT_VALID(pDoc);
	//pDoc->SetModifiedFlag(isDocModified);
}

//////////////////////////////////////////////////////////////////////////////
//			CEditViewPanel								//
//////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class CEditViewPanel : public CTaskBuilderDockPane
{
	DECLARE_DYNCREATE(CEditViewPanel)

public:
	CEditViewPanel();

protected:
	virtual BOOL CanBeClosed() const { return FALSE; }
};

//////////////////////////////////////////////////////////////////////////
//					CEditViewPanel				    //
//////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEditViewPanel, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
CEditViewPanel::CEditViewPanel()
:
CTaskBuilderDockPane(RUNTIME_CLASS(CRSEditorToolView))
{
}

///////////////////////////////////////////////////////////////////////////////
//CRSEditorFrameFullText
//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorFrameFullText, CRSEditorFrame);

BEGIN_MESSAGE_MAP(CRSEditorFrameFullText, CRSEditorFrame)

	ON_WM_CREATE()
	/*ON_COMMAND(ID_RS_EDITOR_CAPTIONBAR_BUTTON, OnClickCaptionBarButton)
	ON_REGISTERED_MESSAGE(BCGM_ON_CLICK_CAPTIONBAR_HYPERLINK, OnClickCaptionBarHyperlink)*/

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
int CRSEditorFrameFullText::OnCreate(LPCREATESTRUCT lpcs)
{
	int ret=  __super::OnCreate(lpcs);

	//m_pTabbedToolBar->AdjustLayout();

	return ret;
}

//=============================================================================
//============================CRSEditorDebugFrame===============================

IMPLEMENT_DYNCREATE(CRSEditorDebugFrame, CRSEditorFrame);

BEGIN_MESSAGE_MAP(CRSEditorDebugFrame, CRSEditorFrame)
	ON_WM_CREATE()

	ON_COMMAND(ID_DEBUG_RUN, OnRunDebug)
	ON_COMMAND(ID_DEBUG_STOP, OnStopDebug)
	ON_COMMAND(ID_DEBUG_STEP, OnStepOver)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
int CRSEditorDebugFrame::OnCreate(LPCREATESTRUCT lpcs)
{
	//int ret = __super::OnCreate(lpcs);

	if (CAbstractFrame::OnCreate(lpcs) == -1)
	{
		ASSERT_TRACE(FALSE, "CRSEditorFrame::OnCreate\n\tframe class not created.\n");
		return -1;
	}

	m_pTabbedToolBar = new CTBTabbedToolbar();
	if (!m_pTabbedToolBar->Create(this))
		return -1;
	m_pTabbedToolBar->SetWindowText(_T("Tabbed"));

	m_pMainToolBar = new CTBToolBar();
	if (!m_pMainToolBar->CreateEmptyTabbedToolbar(this, szToolbarNameMain, _TB("Main")))
	{
		ASSERT_TRACE(FALSE, "Failed to create the main toolBar.\n");
		return -1;
	}
	//----
	m_pMainToolBar->AddButton(ID_RS_CLOSE, RSEDIT_NS, TBIcon(szIconUndoFilled, TOOLBAR), _TB("Back"), _TB("Esc"));
	m_pMainToolBar->AddSeparator();

	m_pMainToolBar->AddSeparator();

	m_pMainToolBar->AddButton(ID_DEBUG_RUN, RSEDIT_NS, TBIcon(szIconPlayFilled, TOOLBAR), _TB("Run"), _TB("F5"));
	m_pMainToolBar->AddButton(ID_DEBUG_STOP, RSEDIT_NS, TBIcon(szIconStop, TOOLBAR), _TB("Stop"), _TB("F6"));
	m_pMainToolBar->AddButton(ID_DEBUG_STEP, RSEDIT_NS, TBIcon(szIconPlayFilled, TOOLBAR), _TB("Step over"), _TB("F10"));

	m_pMainToolBar->AddSeparator();

	//----
	m_pMainToolBar->AddComboBox(ID_EDIT_FIND_COMBO, RSEDIT_NS, 110, CBS_DROPDOWN | (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | WS_VSCROLL | CBS_SORT), _TB("Search:"), -1, _TB("Search..."));

	m_pMainToolBar->AddButton(ID_EDIT_FINDPREV, RSEDIT_NS, TBIcon(szIconArrowPrevFilled, TOOLBAR), _TB("Previous"), L"Ctrl+F3");
	m_pMainToolBar->AddButton(ID_EDIT_FINDNEXT, RSEDIT_NS, TBIcon(szIconArrowNextFilled, TOOLBAR), _TB("Next"), L"F3");


	m_pMainToolBar->SetAutoHideToolBarButton(TRUE);

	m_pTabbedToolBar->AddTab(m_pMainToolBar);
	m_pTabbedToolBar->AdjustLayout();

	CreateCaptionBar();

	return 0;
}

//-----------------------------------------------------------------------------
BOOL CRSEditorDebugFrame::CreateAuxObjects(CCreateContext* pCreateContext)
{ 
	m_pToolPane = (CRSEditorToolDockDebugPane*)CreateDockingPane(RUNTIME_CLASS(CRSEditorToolDockDebugPane), IDC_RS_EditorToolDockPane, _T("Tool"), _T("Tool"), CBRS_LEFT, CSize(300, 700));
	m_pToolPane->EnableToolbar(TOOLBAR_HEIGHT, FALSE);	//GetDocument()->GetWoormFrame()->ShowPanelToolbarText());
	m_pToolPane->SetAutoHideMode(TRUE, CBRS_LEFT | CBRS_HIDE_INPLACE);

	//-------------------------------
	m_pGridPane = (CRSGridDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSGridDockPane), IDC_RS_EditorGridDockPane, _T("Traced rows"), _T("Traced rows"), CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE, CSize(500, 200));
	m_pGridPane->SetAutoHideMode(FALSE, CBRS_ALIGN_BOTTOM);

	m_pDiagnosticPane = (CRSEditorDiagnosticDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSEditorDiagnosticDockPane), IDC_RS_EditorDiagnosticDockPane, _T("Output"), _T("Output"), CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE, CSize(500, 200));
	m_pDiagnosticPane->SetAutoHideMode(FALSE, CBRS_ALIGN_BOTTOM);

	CBCGPDockingControlBar* pBottomTabbedBar = NULL;
	m_pDiagnosticPane->AttachToTabWnd(m_pGridPane, BCGP_DM_SHOW, TRUE, &pBottomTabbedBar);
	pBottomTabbedBar->SetBCGStyle(pBottomTabbedBar->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	pBottomTabbedBar->UpdateWindow();

	//-------------------------------
	m_pBreakpointsPane = (CRSBreakpointDockPaneDebug*)CreateDockingPane(RUNTIME_CLASS(CRSBreakpointDockPaneDebug), IDC_RS_EditorParametersDockPane, _T("Breakpoints"), _T("Breakpoints"), CBRS_ALIGN_RIGHT, CSize(300, 700), NULL);
	m_pBreakpointsPane->SetAutoHideMode(FALSE, CBRS_ALIGN_RIGHT);

	m_pParametersPane = (CRSSymbolTableDockPaneDebug*)CreateDockingPane(RUNTIME_CLASS(CRSSymbolTableDockPaneDebug), IDC_RS_EditorParametersDockPane, _T("Variables"), _T("Variables"), CBRS_ALIGN_RIGHT, CSize(300, 700), NULL);
	m_pParametersPane->SetAutoHideMode(FALSE, CBRS_ALIGN_RIGHT);
	
	CBCGPDockingControlBar* pRightTabbedBar = NULL;
	m_pParametersPane->AttachToTabWnd(m_pBreakpointsPane, BCGP_DM_SHOW, TRUE, &pRightTabbedBar);
	pRightTabbedBar->SetBCGStyle(pRightTabbedBar->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	pRightTabbedBar->UpdateWindow();

	return TRUE; 
}

//-----------------------------------------------------------------------------
void CRSEditorDebugFrame::OnRunDebug()
{
	OnClose();
}

//-----------------------------------------------------------------------------
void CRSEditorDebugFrame::OnStopDebug()
{
	CWoormDocMng* pWDoc = dynamic_cast<CWoormDocMng*>(GetDocument());
	if (pWDoc)
		pWDoc->OnRunStop();

	OnClose();
}

//-----------------------------------------------------------------------------
void CRSEditorDebugFrame::OnStepOver()
{
	ASSERT_VALID(m_pEditView);

	m_pEditView->StepOver();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorToolView, CRSDockedView);

BEGIN_MESSAGE_MAP(CRSEditorToolView, CRSDockedView)

	ON_NOTIFY(NM_DBLCLK, IDC_RS_Tree, OnDblclkTree)
	ON_NOTIFY(TVN_SELCHANGED, IDC_RS_Tree, OnSelchangedTree)

	ON_COMMAND(IDC_RS_Tree_Finder, OnFindTree)

	ON_UPDATE_COMMAND_UI(ID_RS_DELETE, OnUpdateDelete)
	ON_UPDATE_COMMAND_UI(ID_RS_EDIT, OnUpdateEdit)
	ON_UPDATE_COMMAND_UI(ID_RS_MORE, OnUpdateMore)
	ON_UPDATE_COMMAND_UI(ID_RS_FILTER, OnUpdateFilter)

	ON_UPDATE_COMMAND_UI(ID_RS_ADD_HIDDEN_VAR_FROM_DB, OnUpdateAdd)
	
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRSEditorToolView::CRSEditorToolView()
	:
	CRSDockedView(_T("CRSEditorToolView"), IDD_RS_EditorToolView)
{
}

CRSEditorToolView::CRSEditorToolView(const CString& sName, UINT id)
	:
	CRSDockedView(sName, id)
{
}

CRSEditorToolView::~CRSEditorToolView()
{
	if (GetFrame())
		GetFrame()->m_pToolTreeView = NULL;
}

// ---------------------------------------------------------------------------- -
CWoormDocMng* CRSEditorToolView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorFrame* CRSEditorToolView::GetFrame()
{
	return dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSEditorToolView::GetToolBar()
{
	return GetFrame()->m_pToolPane->GetToolBar();
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	SetScrollSizes(MM_TEXT, CSize(0, 0));
}

// ---------------------------------------------------------------------------- -
void CRSEditorToolView::BuildDataControlLinks()
{
	GetFrame()->m_pToolTreeView = this;

	ModifyStyle(0, WS_CLIPCHILDREN);

	m_TreeCtrl.SubclassDlgItem(IDC_RS_Tree, this);
	m_TreeCtrl.Attach(GetDocument());
	m_TreeCtrl.InitializeImageList();
	m_TreeCtrl.m_bShowNodeEnums = TRUE;

	CWnd* pWnd = GetDlgItem(IDC_RS_Tree_Finder);
	if (pWnd)
		pWnd->Detach();
	m_edtFinder.SubclassEdit(IDC_RS_Tree_Finder, this);
	m_edtFinder.EnableFindBrowseButton(TRUE, L"", FALSE);
	m_edtFinder.SetPrompt(_TB("Search node..."));
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolView::FillTree(BOOL bViewMode, CRSEditView* editView/* = NULL*/)
{
	CWaitCursor wc;

	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;

	edit->GetEditCtrl()->EmptyIntellisense();

	m_TreeCtrl.FillAllVariables(TRUE, bViewMode, FALSE, FALSE, FALSE, editView);
	
	if (!bViewMode)
	{
		m_TreeCtrl.FillProcedures();
		m_TreeCtrl.FillQueries();
	}

	m_TreeCtrl.FillEnums(edit);
	m_TreeCtrl.FillFunctions(edit);
	m_TreeCtrl.FillWebMethods(edit);
	m_TreeCtrl.FillCommands(edit,!bViewMode);
	if(edit->m_bStringPreviewEnabled)
		m_TreeCtrl.FillHtmlTags(edit, FALSE);

	if (bViewMode)
	{
		if (!m_TreeCtrl.m_htFontTable)
			m_TreeCtrl.m_htFontTable = m_TreeCtrl.AddNode(_TB("Fonts Style"), CNodeTree::ENodeType::NT_ROOT_FONTS_TABLE);
		if (!m_TreeCtrl.m_htFormatterTable)
			m_TreeCtrl.m_htFormatterTable = m_TreeCtrl.AddNode(_TB("Formatters Style"), CNodeTree::ENodeType::NT_ROOT_FORMATTERS_TABLE);
	}

	return TRUE;
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolView::FillTreeGroupingRule(CRSEditView* editView)
{
	CWaitCursor wc;

	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;

	edit->GetEditCtrl()->EmptyIntellisense();

	m_TreeCtrl.FillVariablesGroupingRules(edit);
						 
	m_TreeCtrl.FillEnums(edit);
	m_TreeCtrl.FillFunctions(edit);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditorToolView::FillFullTextTree(CRSEditView* editView)
{
	CWaitCursor wc;
	
	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;

	edit->GetEditCtrl()->EmptyIntellisense();

	m_TreeCtrl.FillTables(edit);

	m_TreeCtrl.FillEnums(edit);
	m_TreeCtrl.FillFunctions(edit);
	m_TreeCtrl.FillWebMethods(edit);
	m_TreeCtrl.FillCommands(edit);
	m_TreeCtrl.FillQueriesTags(edit, FALSE);
	m_TreeCtrl.FillSpecialTextRect(FALSE);
	m_TreeCtrl.FillHtmlTags(edit, FALSE);

	if (!m_TreeCtrl.m_htFontTable)
		m_TreeCtrl.m_htFontTable = m_TreeCtrl.AddNode(_TB("Fonts Style") + L"...", CNodeTree::ENodeType::NT_ROOT_FONTS_TABLE);
	if (!m_TreeCtrl.m_htFormatterTable)
		m_TreeCtrl.m_htFormatterTable = m_TreeCtrl.AddNode(_TB("Formatters Style") + L"...", CNodeTree::ENodeType::NT_ROOT_FORMATTERS_TABLE);

	return TRUE;
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolView::FillTreeForSql(TblRuleData* parTables, CRSEditView* editView)
{
	CWaitCursor wc;

	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;
	edit->GetEditCtrl()->EmptyIntellisense();

	if (parTables == NULL)
		m_TreeCtrl.FillQueriesTags(edit, TRUE);

	m_TreeCtrl.FillAllVariables(TRUE, FALSE, TRUE, FALSE, TRUE, edit);

	if (parTables)
		m_TreeCtrl.FillRuleTables(parTables);

	m_TreeCtrl.FillTables(edit);

	m_TreeCtrl.FillEnums(edit);
	m_TreeCtrl.FillFunctions(edit);
	//m_TreeCtrl.FillWebMethods();

	if (parTables && m_TreeCtrl.m_htRuleTables)
		m_TreeCtrl.SelectRSTreeItemData(parTables, m_TreeCtrl.m_htRuleTables);

	return TRUE;
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolView::FillTreeForTextRect(CRSEditView* editView)
{
	CWaitCursor wc;
/*
	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;
	edit->GetEditCtrl()->EmptyIntellisense();*/

	m_TreeCtrl.FillSpecialTextRect(FALSE);
	m_TreeCtrl.FillHtmlTags(editView, FALSE);
	return TRUE;
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolView::FillTreeForGroup(CRSEditView* editView)
{
	CWaitCursor wc;

	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
		edit = editView;
	edit->GetEditCtrl()->EmptyIntellisense();

	m_TreeCtrl.FillAllVariables(TRUE, FALSE, FALSE, TRUE,FALSE, edit);

	m_TreeCtrl.FillEnums(edit);
	m_TreeCtrl.FillFunctions(edit);

	return TRUE;
}

// ---------------------------------------------------------------------------- -
void CRSEditorToolView::OnFindTree()
{
	CString sMatchText;
	this->m_edtFinder.GetWindowText(sMatchText);
	sMatchText.Trim();
	if (sMatchText.IsEmpty())
		return;

	m_TreeCtrl.SelectRSTreeItemByMatchingText(sMatchText);
}

//-----------------------------------------------------------------------
void CRSEditorToolView::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;

	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	if (!hCurrentItem)
	{
		return;
	}

	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)m_TreeCtrl.GetItemData(hCurrentItem));
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);

	ASSERT_VALID(GetFrame());
	ASSERT_VALID(GetFrame()->m_pEditView);
	CCustomEditCtrl* pCtrl = GetFrame()->m_pEditView->GetEditCtrl();
	pCtrl->EnableBreakpoints(FALSE);
	m_pBlock = NULL;

	switch(pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_LIST_FUNCTION:
			{
				CDecoratedFunctionDescription* pFunc = dynamic_cast<CDecoratedFunctionDescription*>(pNode->m_pItemData);
				ASSERT_VALID(pFunc);
				CString strHelp = pFunc->GetHelpSignature();

				strHelp += _T("\r\n");
				if (!pFunc->GetRemarks().IsEmpty())
				{
					CString r = pFunc->GetRemarks();
					int n = r.Replace(L"\n", L"\r\n");

					strHelp += _TB("Remarks: ") + r;
					strHelp += _T("\r\n");
				}

				if (!pFunc->GetResult().IsEmpty())
				{
					CString r = pFunc->GetResult();
					int n = r.Replace(L"\n", L"\r\n");

					strHelp += _TB("Result: ") +  r;
					strHelp += _T("\r\n");
				}

				if (!pFunc->GetExample().IsEmpty())
				{
					CString r = pFunc->GetExample();
					int n = r.Replace(L"\n", L"\r\n");

					strHelp += _TB("Example: ") + r;
					strHelp += _T("\r\n");
				}

				GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(strHelp);
			}
			break;
		case CNodeTree::ENodeType::NT_LIST_WEBMETHOD:
			{
				CFunctionDescription* pFunc = dynamic_cast<CFunctionDescription*>(pNode->m_pItemData);
				ASSERT_VALID(pFunc);

				BOOL bIsRunReport = pFunc->GetName().CompareNoCase(_T("RunReport")) == 0;
				
				CString strHelp = pFunc->GetHelpSignature();

				if (bIsRunReport)
				{
					strHelp += _T("\r\n");
					strHelp += _TB("RunReport function supports optional parameters: it allows parametrization of called report");
					strHelp += _T("\r\n");
					strHelp += _TB("Optional parameters must be pairs: the first is the field name of called report to valorize with the value of the second parameter");
				}

				GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(strHelp);
				break;
			}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
			ASSERT_VALID(pField);
			if (!pField)
				return;

			CString strToInsert =  _TB("Data type: ") + pField->GetDataType().ToString();
			if (pField->GetDataType() == DataType::String)
				strToInsert += L" - " + _TB("length:") + cwsprintf(L" %d", pField->GetLen()/*, pField->GetNumDec()*/);
			//if (pField->IsTableRuleField())
			//	strToInsert += L" - " + _TB("database field");,
			//if (pField->IsHidden())
			//	strToInsert += L" - " + _TB("hidden variable"); ,
			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(strToInsert);
			break;
		}

		case CNodeTree::ENodeType::NT_LIST_ENUM_TYPE:
		{
			EnumTag* pTag = dynamic_cast<EnumTag*>(pNode->m_pItemData);
			ASSERT_VALID(pTag);
			if (!pTag)
				return;

			CString str = pTag->GetTagName();
			CString st  = pTag->GetTagTitle();
			if (str.CompareNoCase(st))
				str += L" - " + st;

			str = cwsprintf(L"Enum[%d]", pTag->GetTagValue()) + L" - " + str;

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(str);

			break;
		}
		case CNodeTree::ENodeType::NT_LIST_ENUM_VALUE:
		{
			EnumTag* pTag = dynamic_cast<EnumTag*>(pNode->m_pParentItemData);
			ASSERT_VALID(pTag);
			EnumItem* pItem = dynamic_cast<EnumItem*>(pNode->m_pItemData);
			ASSERT_VALID(pItem);

			if (!pTag || !pItem)
				return;

			DataEnum de(pTag->GetTagValue(), pItem->GetItemValue());

			CString str = L"{" + pTag->GetTagName() + L" : " + pItem->GetItemName() + L"}";
			CString st	= L"{" + pTag->GetTagTitle() + L" : " + pItem->GetTitle() + L"}";
			if (str.CompareNoCase(st))
				str += L" - " + st;

			str = de.ToString() + cwsprintf(L" %d", de.GetValue()) + L" - " + str;

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(str);
			break;
		}

		case CNodeTree::ENodeType::NT_LIST_DBTABLE:
		case CNodeTree::ENodeType::NT_LIST_DBVIEW:
		{
			const SqlCatalogEntry* pCatEntry = dynamic_cast<const SqlCatalogEntry*>(pNode->m_pItemData);

			CString str = pCatEntry->m_strTableName;
			CString st = AfxLoadDatabaseString(str, str);
			if (str.CompareNoCase(st))
				str += L" - " + st;

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(str);
			break;
		}
		case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
		{
			SqlColumnInfo* pCol = (SqlColumnInfo*)(pNode->m_pItemData);
			ASSERT_VALID(pCol);
			if (!pCol)
				return;
			SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
			ASSERT_VALID(pCatEntry);
			if (!pCatEntry)
				return;

			CString str = pCatEntry->m_strTableName + '.' + pCol->GetColumnName();
			CString st = AfxLoadDatabaseString(pCatEntry->m_strTableName, pCatEntry->m_strTableName) + '.' +
				AfxLoadDatabaseString(pCol->GetColumnName(), pCatEntry->m_strTableName);
			if (str.CompareNoCase(st))
				str += L" - " + st;

			str += L"\r\n" + _TB("Data type: ") + pCol->GetDataObjType().ToString();
			if (pCol->GetDataObjType() == DataType::String)
				str += L" - " + _TB("length:") + cwsprintf(L" %d", pCol->GetColumnLength()/*, pCol->GetColumnDecimal()*/);

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(str);
			break;
		}

		case CNodeTree::ENodeType::NT_EXPR:
		{
			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(L"");
			GetFrame()->m_pEditView->SetText(L"");
			pCtrl->RemoveAllBreakpoints();
			//----

			ASSERT(pNode->m_ppExpr);
			if (!pNode->m_ppExpr)
				break;

			Expression* pExpr = *pNode->m_ppExpr;
			if (!pExpr)
			{
				break;
			}
			ASSERT_VALID(pExpr);

			CString sExpr = pExpr->ToString();
			GetFrame()->m_pEditView->SetText(sExpr);
			break;
		}
		case CNodeTree::ENodeType::NT_BLOCK:
		{
			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(L"");
			GetFrame()->m_pEditView->SetText(L"");
			pCtrl->RemoveAllBreakpoints();
			pCtrl->EnableBreakpoints(TRUE);
			//----

			ASSERT(pNode->m_ppBlock);
			if (!pNode->m_ppBlock) 
				break;

			Block* pBlock = *pNode->m_ppBlock;
			if (!pBlock)
			{
				break;
			}
			ASSERT_VALID(pBlock);
			m_pBlock = pBlock;

			CString sBlock = pBlock->Unparse();
			GetFrame()->m_pEditView->SetText(sBlock);

			ActionObj* pCurrAct =  GetFrame()->m_pEditView->m_Context.m_pCurrActionObj;
			if (pCurrAct) ASSERT_VALID(pCurrAct);

			CArray<int> arRows;
			if (pBlock->GetBreakpointRows(arRows))
			{
				for (int i = 0; i < arRows.GetSize(); i++)
				{
					int r = arRows[i];

					if (!pCurrAct || pCurrAct->GetBlockParent() != pBlock || pCurrAct->m_nDebugUnparseRow != r)
						pCtrl->SetMarker(r);
				}
			}

			if (pCurrAct && pCurrAct->GetBlockParent() == pBlock)
			{
				if (pCurrAct->HasBreakpoint())
					pCtrl->SetMarker(pCurrAct->m_nDebugUnparseRow, TRUE);

				if (pCurrAct->HasBreakpoint() && pCurrAct->GetBreakpoint()->m_bStepOverBreakpoint)
				{
					pCurrAct->RemoveBreakpoint();
				}
			}

			break;
		}

		case CNodeTree::ENodeType::NT_LIST_COMMAND:
		{
			CString str = m_TreeCtrl.GetItemText(hCurrentItem);

			str = m_TreeCtrl.GetCommandDescription(str);

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(str);
			break;
		}
		case CNodeTree::ENodeType::NT_LIST_HTML_TAGS:
		{
			CHtmlTag* pHtmlTag = dynamic_cast<CHtmlTag*>(pNode->m_pItemData);
			ASSERT_VALID(pHtmlTag);
			if (!pHtmlTag)
				break;
			CString strHelp = pHtmlTag->GetDescription();

			strHelp += _T("\r\n");
			if (!pHtmlTag->GetExample().IsEmpty())
			{
				strHelp += _TB("Example: ") + pHtmlTag->GetExample();
				strHelp += _T("\r\n");
			}

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(strHelp);
			break;
		}
		case CNodeTree::ENodeType::NT_LIST_QUERY_TAGS:
		{
			CQueryTag* pTag = dynamic_cast<CQueryTag*>(pNode->m_pItemData);
			ASSERT_VALID(pTag);
			if (!pTag)
				break;
			CString strHelp = pTag->GetDescription();

			strHelp += _T("\r\n");
			if (!pTag->GetExample().IsEmpty())
			{
				strHelp += _TB("Example: ") + pTag->GetExample();
				strHelp += _T("\r\n");
			}

			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(strHelp);
			break;
		}

		default:
			GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(L"");
			break;
	}
}

// ---------------------------------------------------------------------------- -
void CRSEditorToolView::OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	BOOL textSelected = !GetFrame()->m_pEditView->GetEditCtrl()->GetSelText().IsEmpty();
	GetFrame()->m_pEditView->GetEditCtrl()->DeleteSelectedText(TRUE, FALSE, FALSE);
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	//--------------------------------------
	OnEdit(textSelected);
}

// -----------------------------------------------------------------------------
BOOL CRSEditorToolView::PreTranslateMessage(MSG* pMsg)
{
	if (pMsg->message == WM_KEYDOWN && m_bJustInserted)
	{
		GetFrame()->m_pEditView->GetEditCtrl()->SetFocus();

		m_bJustInserted = FALSE;
		GetFrame()->m_pEditView->GetEditCtrl()->ShowCaret();
		return	  TRUE;
	}

	if (pMsg->wParam == VK_RETURN && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus())
	{
		OnFilter();
		return TRUE;
	}
	
	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
/*
void CRSEditorToolView::OnRefresh()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);
	CWaitCursor wc;

	switch (pNode->m_NodeType)
	{
	}
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateRefresh(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;

	switch (pNode->m_NodeType)
	{
	//case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
	//case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	//case CNodeTree::ENodeType::NT_ROOT_QUERIES:

		//bEnable = TRUE;
		break;
	}

	pCmdUI->Enable(bEnable);
}
*/

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnMore()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);
	CWaitCursor wc;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_TABLES:
	{
		m_TreeCtrl.m_bShowAllTables = !m_TreeCtrl.m_bShowAllTables;

		m_TreeCtrl.RemoveTreeChilds(m_TreeCtrl.m_htTables);

		if (m_TreeCtrl.m_bShowAllTables)
			m_TreeCtrl.FillAllTables();
		else
		{
			m_TreeCtrl.m_bShowFilteredTables = FALSE;
			m_TreeCtrl.m_FilterTablePattern.Empty();
			GetToolBar()->CheckButton(ID_RS_FILTER, m_TreeCtrl.m_bShowFilteredTables);

			m_TreeCtrl.FillTables();
		}

		m_TreeCtrl.Expand(m_TreeCtrl.m_htTables, TVE_EXPAND);
		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateMore(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		pCmdUI->Enable(FALSE);
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_TABLES:

		bEnable = TRUE;
		break;
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnFilter()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);
	CWaitCursor wc;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_TABLES:
	{
		CString filter;
		m_edtFinder.GetWindowText(filter); filter.Trim();
		if (!filter.IsEmpty())
		{
			filter.Trim('*'); filter = '*' + filter + '*';
		}
		//--------------------------------------------------------------------aggiorno il flag
		if (filter.IsEmpty())
		{
			m_TreeCtrl.m_bShowFilteredTables = FALSE;
		}
		else if (m_TreeCtrl.m_bShowFilteredTables && filter.CompareNoCase(m_TreeCtrl.m_FilterTablePattern))
		{
			//C'era un filtro, ma la stringa è cambiata,  riapplico il nuovo filtro
			m_TreeCtrl.m_bShowFilteredTables = TRUE; //---> non serve ma lo metto per chiarezza
		}
		else
		{
			m_TreeCtrl.m_bShowFilteredTables = !m_TreeCtrl.m_bShowFilteredTables;
		}
		//------------------------------------------------------------------------------------
		if (m_TreeCtrl.m_bShowFilteredTables)
		{
			m_TreeCtrl.m_FilterTablePattern = filter;
		}
		else
		{
			m_TreeCtrl.m_FilterTablePattern.Empty();
		}

		m_TreeCtrl.RemoveTreeChilds(m_TreeCtrl.m_htTables);

		if (m_TreeCtrl.m_bShowAllTables)
			m_TreeCtrl.FillAllTables();
		else
			m_TreeCtrl.FillTables();

		m_TreeCtrl.Expand(m_TreeCtrl.m_htTables, TVE_EXPAND);
		break;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateFilter(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		pCmdUI->Enable(FALSE);
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_ROOT_TABLES:
		{
			CString filter;
			m_edtFinder.GetWindowText(filter); filter.Trim();

			//se deve essere checckato
			if (m_TreeCtrl.m_bShowFilteredTables)
			{
				//lo faccio diventare una checkbox selezionata
				if (GetDocument() && GetDocument()->GetWoormFrame())
				{
					GetFrame()->m_pToolPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_CHECKBOX);
					GetFrame()->m_pToolPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_CHECKED);
					bEnable = TRUE;
				}
			}
			//altrimenti
			else
			{
				//lo faccio tornare bottone
				if (GetDocument() && GetDocument()->GetWoormFrame())
					GetFrame()->m_pToolPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_BUTTON);
				bEnable = m_TreeCtrl.m_bShowFilteredTables || !filter.IsEmpty();
			}
		}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnEdit(BOOL textSelected)
{
	//TODO Open Fonts and Formatters dialogs

	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);


	CString strToInsert;
	BOOL ifTableInserted = FALSE;
	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_ROOT_ENUMS:
			AfxRunEnumsViewer();
			break;

		case CNodeTree::ENodeType::NT_ROOT_FONTS_TABLE:
		{
			FontIdx   nIdx = -1;
			CFontStylesDlg	dialog(*(GetDocument()->m_pFontStyles), nIdx, FALSE, this, GetDocument()->GetNamespace(), FALSE);

			if (dialog.DoModal() != IDOK)
			{
				if (!GetDocument()->IsModified())
					GetDocument()->SetModifiedFlag(GetDocument()->m_pFontStyles->IsModified());
				return;
			}

			if (!GetDocument()->IsModified())
				GetDocument()->SetModifiedFlag(GetDocument()->m_pFontStyles->IsModified());

			if (nIdx != FNT_ERROR)
			{
				strToInsert = GetDocument()->m_pFontStyles->GetStyleName(nIdx);
			}
			break;
		}
		case CNodeTree::ENodeType::NT_ROOT_FORMATTERS_TABLE:
		{
			FormatIdx   nIdx = -1;
			CFormatDlg	dialog(*(GetDocument()->m_pFormatStyles), nIdx, FALSE, this, GetDocument()->GetNamespace(), FALSE);

			if (dialog.DoModal() != IDOK)
			{
				if (!GetDocument()->IsModified())
					GetDocument()->SetModifiedFlag(GetDocument()->m_pFormatStyles->IsModified());
				return;
			}

			if (!GetDocument()->IsModified())
				GetDocument()->SetModifiedFlag(GetDocument()->m_pFormatStyles->IsModified());

			if (nIdx != FNT_ERROR)
			{
				strToInsert = GetDocument()->m_pFormatStyles->GetStyleName(nIdx);
			}
			break;
		}
		case CNodeTree::ENodeType::NT_LIST_ENUM_TYPE:
		{
			EnumTag* pTag = dynamic_cast<EnumTag*>(pNode->m_pItemData);
			ASSERT_VALID(pTag);
			if (!pTag)
				return;

			//strToInsert = cwsprintf(L"Enum[%d]", pTag->GetTagValue()) + L" /* " + pTag->GetTagName() + L" */ ";
			break;
		}
		case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
		{
			SqlColumnInfo* pCol = (SqlColumnInfo*)(pNode->m_pItemData);
			ASSERT_VALID(pCol);
			if (!pCol)
				return;
			SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
			ASSERT_VALID(pCatEntry);
			if (!pCatEntry)
				return;

			BOOL isSelected = GetFrame()->m_pEditView->GetEditCtrl()->MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_PREV_WORD);
			CString prevInserted;
			if (isSelected)
				prevInserted = GetFrame()->m_pEditView->GetEditCtrl()->GetSelText();
			GetFrame()->m_pEditView->GetEditCtrl()->RemoveSelection(0);

			if (prevInserted.CompareNoCase(pCatEntry->m_strTableName) == 0)
			{
				strToInsert = '.' + pCol->GetColumnName();
				ifTableInserted = TRUE;
			}

			else
				strToInsert = pCatEntry->m_strTableName + '.' + pCol->GetColumnName();

			break;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		case CNodeTree::ENodeType::NT_PROCEDURE:
		case CNodeTree::ENodeType::NT_NAMED_QUERY:
		case CNodeTree::ENodeType::NT_LIST_ENUM_VALUE:
		case CNodeTree::ENodeType::NT_LIST_WEBMETHOD:
		case CNodeTree::ENodeType::NT_LIST_FUNCTION:
		case CNodeTree::ENodeType::NT_LIST_DBTABLE:
		case CNodeTree::ENodeType::NT_LIST_DBVIEW:
		case CNodeTree::ENodeType::NT_LIST_COMMAND:
		case CNodeTree::ENodeType::NT_LIST_HTML_TAGS:
		case CNodeTree::ENodeType::NT_LIST_QUERY_TAGS:
		{
			strToInsert = m_TreeCtrl.GetNodeString(pNode);
			break;
		}

		case CNodeTree::ENodeType::NT_LIST_SPECIAL_TEXT:
		{
			strToInsert = m_TreeCtrl.GetNodeString(pNode);
			CFunctionDescription* pF = dynamic_cast<CFunctionDescription*>(pNode->m_pParentItemData);
			if (pF)
			{
				strToInsert.Trim();
				strToInsert.TrimLeft(L"{");
				strToInsert.TrimRight(L"}");
				strToInsert = L'"' + strToInsert + L'"';
			}
			break;
		}
		default:
			return;
	}

	if (strToInsert.IsEmpty())
		return;

	// If the line is not empty i'll insert a blank between two words or else they remain attached
	// NO REFACTORING
	GetFrame()->m_pEditView->GetEditCtrl()->MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_HOME);
	if (!GetFrame()->m_pEditView->GetEditCtrl()->GetSelText().IsEmpty() && !ifTableInserted && !textSelected)
		strToInsert = L" " + strToInsert;
	GetFrame()->m_pEditView->GetEditCtrl()->RemoveSelection(FALSE);	
	///////////////////////////////////////////////////////////////////

	GetFrame()->m_pEditView->GetEditCtrl()->HideCaret();

	GetFrame()->m_pEditView->GetEditCtrl()->ChangeSelectedText(strToInsert);

	m_bJustInserted = TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateEdit(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		pCmdUI->Enable(FALSE);
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;
	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_ROOT_ENUMS:
		case CNodeTree::ENodeType::NT_ROOT_FONTS_TABLE:
		case CNodeTree::ENodeType::NT_ROOT_FORMATTERS_TABLE:

			bEnable = TRUE;
			break;
	}
	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateDelete(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		pCmdUI->Enable(FALSE);
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_BREAKPOINT_ACTION:
		{
			ActionObj* pCurrAct = dynamic_cast<ActionObj*>(pNode->m_pItemData);
			if (!pCurrAct)
			{
				GetFrame()->m_pEditView->SetText(L"");
				break;
			}
			ASSERT_VALID(pCurrAct);

			bEnable = pCurrAct->HasBreakpoint();
			break;
		}
		case CNodeTree::ENodeType::NT_ROOT_BREAKPOINTS:
		{
			bEnable = m_TreeCtrl.ItemHasChildren(hCurrentItem);
			break;
		}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnUpdateAdd(CCmdUI* pCmdUI)
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		pCmdUI->Enable(FALSE);
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	BOOL bEnable = FALSE;

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
		{
			TblRuleData* pTblRule = NULL;
			if (GetFrame()->m_pEditView->m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE)
			{
				if (GetFrame()->m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
				{
					pTblRule = dynamic_cast<TblRuleData*>(GetFrame()->m_pEditView->m_Context.m_pNode->m_pItemData);
					ASSERT_VALID(pTblRule);
					if (!pTblRule) break;
					
					SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
					ASSERT_VALID(pCatEntry);
					if (!pCatEntry) break;
					
					//NON posso aggiungere colonne alla current rule in modifica completa
					if (pTblRule->m_arSqlTableJoinInfoArray.GetTableInfo(pCatEntry->m_strTableName))
						break;
				}
			}

			bEnable = TRUE;
			break;
		}
/*
		case CNodeTree::ENodeType::NT_BLOCK:
		{
			ASSERT(pNode->m_ppBlock);
			if (!pNode->m_ppBlock) break;

			Block* pBlock = *pNode->m_ppBlock;
			if (!pBlock) break;
			ASSERT_VALID(pBlock);

			bEnable = !pBlock->IsEmpty();
			break;
	}
*/
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnDelete()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_BREAKPOINT_ACTION:
		{
			ActionObj* pCurrAct = dynamic_cast<ActionObj*>(pNode->m_pItemData);
			if (!pCurrAct)
			{
				break;
			}
			ASSERT_VALID(pCurrAct);
			if (!pCurrAct->HasBreakpoint())
			{
				break;
			}
			
			pCurrAct->RemoveBreakpoint();

			FillBreakpoints(NULL);

			if (this->m_pEditView && this->m_pEditView->m_Context.m_pCurrActionObj)
			{
				ASSERT_VALID(this->m_pEditView->m_Context.m_pCurrActionObj);
				Block* pBlock = this->m_pEditView->m_Context.m_pCurrActionObj->GetBlockParent();

				BOOL bSelect =  m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htEvents, FALSE) ||
								m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htRules, FALSE) ||
								m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htProcedures, FALSE);
			}

			break;
		}
		case CNodeTree::ENodeType::NT_ROOT_BREAKPOINTS:
		{
			RepEngine* pEngine = dynamic_cast<RepEngine*>(pNode->m_pParentItemData);
			if (!pEngine)
			{
				break;
			}
			ASSERT_VALID(pEngine);
			
			if (!pEngine->m_arBreakpoints.GetCount())
			{
				break;
			}
			for (int i = pEngine->m_arBreakpoints.GetCount() - 1; i >= 0; i--)
			{
				CBreakpoint* pB = pEngine->m_arBreakpoints.GetAt(i);
				if (pB)
				{
					ASSERT_VALID(pB);
					ActionObj* pAction = pB->m_pAction;
					ASSERT_VALID (pAction) ;
					pAction->RemoveBreakpoint();
				}
			}
			pEngine->m_arBreakpoints.RemoveAll();

			FillBreakpoints(NULL);

			if (this->m_pEditView)
			{
				ASSERT_VALID(this->m_pEditView);
				if (this->m_pEditView->m_Context.m_pCurrActionObj)
				{
					ASSERT_VALID(this->m_pEditView->m_Context.m_pCurrActionObj);
					Block* pBlock = this->m_pEditView->m_Context.m_pCurrActionObj->GetBlockParent();

					BOOL bSelect =  m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htEvents, FALSE) ||
									m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htRules, FALSE) ||
									m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htProcedures, FALSE);
				}
				else
				{
					m_pEditView->SetText(L"");
					CCustomEditCtrl* pCtrl = m_pEditView->GetEditCtrl();

					pCtrl->RemoveAllBreakpoints();
					pCtrl->EnableBreakpoints(TRUE);
				}
			}
			break;
		}
	}
}

//-----------------------------------------------------------------------------
void CRSEditorToolView::OnAdd()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
		{
			TblRuleData* pTblRuleToSkip = NULL;
			if (GetFrame()->m_pEditView->m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE)
			{
				if (GetFrame()->m_pEditView->m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
				{
					pTblRuleToSkip = dynamic_cast<TblRuleData*>(GetFrame()->m_pEditView->m_Context.m_pNode->m_pItemData);
				}
			}

			TblRuleData* pTblRule = GetDocument()->AddDBColumns_GetOrCreateTblRule(pNode);
			if (!pTblRule)
			{
				return;
			}
			if (pTblRuleToSkip == pTblRule)
			{
				return;
			}

			SqlColumnInfo* pColInfo = (SqlColumnInfo*)(pNode->m_pItemData);
			ASSERT_VALID(pColInfo);
			if (!pColInfo)
			{
				return;
			}

			int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pColInfo->GetTableName());
			ASSERT(idx >= 0);

			WoormField* pNewHiddenField = NULL;
			CStringArray* varsForColoring = new CStringArray();
			if (GetDocument()->m_pNodesSelection && GetDocument()->m_pNodesSelection->GetCount() > 0)
			{
				pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(GetDocument()->m_pNodesSelection, FALSE, pTblRule, idx, varsForColoring);
			}
			else
			{
				pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(pNode, FALSE, pTblRule, idx, FALSE, varsForColoring);
			}

			if (pNewHiddenField)
			{
				m_TreeCtrl.FillAllVariables(TRUE, FALSE, TRUE, FALSE, TRUE);
				m_TreeCtrl.FillRuleTables(pTblRule);

				m_TreeCtrl.SelectRSTreeItemData(pNewHiddenField, m_TreeCtrl.m_htVariables);
			}

			for (int i = 0;i < varsForColoring->GetCount();i++)
				GetFrame()->m_pEditView->GetEditCtrl()->SetWordColor(varsForColoring->GetAt(i), RS_COLOR_VARIABLE, -1, FALSE);
			
			SAFE_DELETE(varsForColoring);
			return;
		}

		case CNodeTree::ENodeType::NT_BLOCK:
		{
			ASSERT(pNode->m_ppBlock);
			if (!pNode->m_ppBlock) break;

			Block* pBlock = *pNode->m_ppBlock;
			if (!pBlock) break;
			ASSERT_VALID(pBlock);
/*			
			pBlock->m_bHasBreakpoint = !pBlock->m_bHasBreakpoint;

			pNode->SetItemColor(pBlock->m_bHasBreakpoint ? RS_COLOR_BREAKPOINT : RGB(0,0,0));
			pNode->m_eImgIndex = pBlock->m_bHasBreakpoint ? CRSTreeCtrlImgIdx::BreakPoint : CRSTreeCtrlImgIdx::NoGLyph;
			m_TreeCtrl.SetItemImage(pNode->m_ht, pNode->m_eImgIndex, pNode->m_eImgIndex);
			m_TreeCtrl.Invalidate();

			if (pBlock->m_bHasBreakpoint)
				GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(_TB("A breakpoint was set at the start of this event"));
			else
				GetFrame()->m_pDiagnosticView->m_edtErrors.SetWindowText(_TB("A breakpoint was removed from the start of this event"));
*/
			break;
	}
}
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorToolDebugView, CRSEditorToolView)

BEGIN_MESSAGE_MAP(CRSEditorToolDebugView, CRSEditorToolView)

	ON_NOTIFY(TVN_SELCHANGED, IDC_RS_Tree, OnSelchangedTree)

END_MESSAGE_MAP()
//-----------------------------------------------------------------------------

CRSEditorToolDebugView::CRSEditorToolDebugView()
	:
	CRSEditorToolView(_T("CRSEditorToolDebugView"), IDD_RS_EditorToolView)
{
}

//-----------------------------------------------------------------------------
CRSEditorDebugFrame* CRSEditorToolDebugView::GetFrame()
{
	return dynamic_cast<CRSEditorDebugFrame*>(__super::GetFrame());
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditorToolDebugView::FillForDebug(CRSEditView* editView)
{
	CWaitCursor wc;
	/*
	CRSEditView* edit = GetFrame()->m_pEditView;
	if (!edit)
	edit = editView;
	edit->GetEditCtrl()->EmptyIntellisense();*/

	Block* pBlock = NULL;
	if (editView)
	{
		ASSERT_VALID(editView);
		if (editView->m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_DEBUG_ACTIONS && editView->m_Context.m_pCurrActionObj)
		{
			ASSERT_VALID(editView->m_Context.m_pCurrActionObj);
			pBlock = editView->m_Context.m_pCurrActionObj->GetBlockParent();
		}
	}

	//problema per le AskDialog: sono eseguite sul document thread 
	m_TreeCtrl.FillDialogsForDebug(pBlock);

	m_TreeCtrl.FillRulesForDebug(pBlock);
	m_TreeCtrl.FillTupleRulesForDebug(pBlock);

	m_TreeCtrl.FillEventsForDebug(pBlock);

	m_TreeCtrl.FillProceduresForDebug(pBlock);

	FillBreakpoints(editView->m_Context.m_pCurrActionObj);	

	if (pBlock)
	{	//con TVI_ROOT si schianta
		HTREEITEM ht = NULL;
		if (!ht && m_TreeCtrl.m_htEvents)
			m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htEvents);
		if (!ht && m_TreeCtrl.m_htRules)
			ht = m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htRules);
		if (!ht && m_TreeCtrl.m_htProcedures)
			ht = m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htProcedures);
		if (!ht && m_TreeCtrl.m_htAskDialogs)
			ht = m_TreeCtrl.SelectRSTreeItemData(pBlock, m_TreeCtrl.m_htAskDialogs);
	}

	return TRUE;
}

// -----------------------------------------------------------------------------
BOOL CRSEditorToolDebugView::FillBreakpoints(ActionObj* pCurrent)
{
	m_TreeCtrl.FillBreakpoints(pCurrent);

	GetFrame()->m_pBreakpointsView->LoadBreakpoints(pCurrent);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditorToolDebugView::ToggleBreakpoint(int nRow, BOOL bSet)
{
	Block* pBlock = m_pBlock;
	if (!pBlock)
	{
		return;
	}

	//-----
	ASSERT_VALID(pBlock);
	ActionObj* pActionChild = pBlock->GetActionByRow(nRow);
	if (pActionChild)
	{
		if (bSet) 
			pActionChild->AddBreakpoint();
		else 
			pActionChild->RemoveBreakpoint();

		FillBreakpoints(bSet ? pActionChild : NULL);
	}
}

//-----------------------------------------------------------------------
void CRSEditorToolDebugView::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;

	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	if (!hCurrentItem)
	{
		return;
	}

	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)m_TreeCtrl.GetItemData(hCurrentItem));
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);

	ASSERT_VALID(GetFrame());
	ASSERT_VALID(GetFrame()->m_pEditView);
	CCustomEditCtrl* pCtrl = GetFrame()->m_pEditView->GetEditCtrl();
	pCtrl->EnableBreakpoints(FALSE);
	m_pBlock = NULL;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_DEBUG_RULE:
	{
		GetFrame()->m_pDiagnosticView->SetText(L"");
		GetFrame()->m_pEditView->SetText(L"");
		pCtrl->RemoveAllBreakpoints();
		//----

		RuleObj* pRule = dynamic_cast<RuleObj*>(pNode->m_pItemData);
		if (!pRule)
		{
			break;
		}
		ASSERT_VALID(pRule);
		CString sRule;

		if (pRule->IsKindOf(RUNTIME_CLASS(DataTableRule)))
		{
			DataTableRule* pTableRule = (DataTableRule*)pRule;
			if (pTableRule->GetDataTable())
				sRule = pTableRule->ToSqlString();
			if (!sRule.IsEmpty())
			{
				GetFrame()->m_pDiagnosticView->SetText(sRule);
			}
			sRule = pTableRule->ToString();
		}
		else if (pRule->IsKindOf(RUNTIME_CLASS(QueryRule)))
		{
			QueryRule* pQueryRule = (QueryRule*)pRule;
			if (pQueryRule->GetQuery())
				sRule = pQueryRule->GetQuery()->ToSqlString();
			if (!sRule.IsEmpty())
			{
				GetFrame()->m_pDiagnosticView->SetText(sRule);
			}
			sRule = pQueryRule->GetQuery()->Unparse();
		}
		else if (pRule->IsKindOf(RUNTIME_CLASS(CondRule)))
		{
			CondRule* pCondRule = (CondRule*)pRule;
			sRule = pCondRule->ToString();
		}
		else if (pRule->IsKindOf(RUNTIME_CLASS(ExpRule)))
		{
			ExpRule* pExpRule = (ExpRule*)pRule;
			sRule = pExpRule->ToString();
		}
		else if (pRule->IsKindOf(RUNTIME_CLASS(WhileRule)))
		{
			WhileRule* pWRule = (WhileRule*)pRule;
			sRule = pWRule->ToString();
		}

		GetFrame()->m_pEditView->SetText(sRule);
		break;
	}
	case CNodeTree::ENodeType::NT_BREAKPOINT_ACTION:
	{
		CTBGridControlResizable*  pGrdTable = NULL;
		if (GetFrame()->m_pGridView)
		{
			pGrdTable = GetFrame()->m_pGridView->m_pGrdTable;
			ASSERT_VALID(pGrdTable);
			pGrdTable->RemoveAll();
			pGrdTable->DeleteAllColumns();
			pGrdTable->AdjustLayout();
		}
		//--------------------------
		GetFrame()->m_pDiagnosticView->SetText(L"");
		GetFrame()->m_pEditView->SetText(L"");
		pCtrl->RemoveAllBreakpoints();
		//----

		ActionObj* pCurrAct = dynamic_cast<ActionObj*>(pNode->m_pItemData);
		if (!pCurrAct)
		{
			break;
		}
		ASSERT_VALID(pCurrAct);

		Block* pBlock = dynamic_cast<Block*>(pNode->m_pParentItemData);
		if (!pBlock)
		{
			break;
		}
		ASSERT_VALID(pBlock);
		m_pBlock = pBlock;

		CString sBlock = pBlock->Unparse();
		GetFrame()->m_pEditView->SetText(sBlock);

		CArray<int> arRows;
		if (pBlock->GetBreakpointRows(arRows))
		{
			for (int i = 0; i < arRows.GetSize(); i++)
			{
				int r = arRows[i];

				if (!pCurrAct || pCurrAct->m_nDebugUnparseRow != r)
					pCtrl->SetMarker(r);
			}
		}

		if (pCurrAct)
		{
			pCtrl->PointOutBreakpointMarker(pCurrAct->m_nDebugUnparseRow);

			CBreakpoint* pB =  pCurrAct->GetBreakpoint();

			if (pB->m_arTracedValues.GetCount() && pGrdTable)
			{
				for (int i = 0; i < pB->m_arTracedNames.GetSize(); i++)
				{
					CString sFName = pB->m_arTracedNames[i];
					pGrdTable->InsertColumn(i, sFName, 100);
				}
				for (int r = 0; r < pB->m_arTracedValues.GetSize(); r++)
				{
					CStringArray* pValueRow =  pB->m_arTracedValues[r];

					CBCGPGridRow* pGridRow = pGrdTable->CreateRow(pValueRow->GetSize());

					for (int c = 0; c < pValueRow->GetSize(); c++)
					{
						CString sValue = pValueRow->GetAt(c);

						pGridRow->GetItem(c)->SetValue((LPCTSTR)sValue);
					}
					// Add row to grid:
					pGrdTable->AddRow(pGridRow, FALSE /* Don't recal. layout */);
				}
				pGrdTable->AdjustLayout();
			}
		}
		break;
	}
	default:
		return __super::OnSelchangedTree(pNMHDR, pResult);
	}
}
//-----------------------------------------------------------------------

//=============================================================================
IMPLEMENT_DYNCREATE(CustomParametersPropertyGrid ,CTBPropertyGrid)

BEGIN_MESSAGE_MAP(CustomParametersPropertyGrid, CTBPropertyGrid)
END_MESSAGE_MAP()

BOOL CustomParametersPropertyGrid::PreTranslateMessage(MSG* pMsg)
{
	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000;

	if (pMsg->message == WM_RBUTTONUP /*|| pMsg->message == WM_LBUTTONUP*/)
	{
		return TRUE;
	}

	if (pMsg->message == WM_CONTEXTMENU)
		return TRUE;

	if (pMsg->wParam == VK_TAB && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*> (GetCurSel());
		CBCGPProp* nextProp = NULL;
		if (prop)
			nextProp = GetNextProperty(prop);
		if (nextProp)
		{
			SetCurSel(nextProp, TRUE);

			if (nextProp->IsAllowEdit())
			{
				nextProp->DoEdit();
				CEdit* pEdit= DYNAMIC_DOWNCAST(CEdit, ((CTBProperty*)nextProp)->GetWndInPlace());
				if (::IsWindow(pEdit->GetSafeHwnd()))
				{
					CString str = nextProp->GetValue();
					pEdit->SetSel(str.GetLength(), str.GetLength(), FALSE);
				}
			}
			return TRUE;
		}
	}

	if (pMsg->wParam == VK_TAB && pMsg->message == WM_KEYDOWN && shiftPressed)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*> (GetCurSel());
		CBCGPProp* nextProp = NULL;
		if (prop)
			nextProp = GetPrevProperty(prop);
		if (nextProp)
		{
			SetCurSel(nextProp, TRUE);

			if (nextProp->IsAllowEdit())
			{
				nextProp->DoEdit();
				CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, ((CTBProperty*)nextProp)->GetWndInPlace());
				if (::IsWindow(pEdit->GetSafeHwnd()))
				{
					CString str = nextProp->GetValue();
					pEdit->SetSel(str.GetLength(), str.GetLength(), FALSE);
				}
			}
			return TRUE;
		}
	}

	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*> (GetCurSel());
		if (prop && prop->IsAllowEdit())
		{
			CEdit* pEdit =DYNAMIC_DOWNCAST(CEdit, prop->GetWndInPlace());
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int chars = pEdit->CharFromPos(pEdit->GetCaretPos());
				CString subStrStart = str.Mid(0, chars);
				CString subStrEnd;
				if (str.GetLength() == chars)
					subStrEnd = L"";
				else
					subStrEnd = str.Mid(chars + 1);
				CString finaStr = subStrStart + subStrEnd;

				prop->SetValue((LPCTSTR)finaStr);
				prop->DoEdit();
				pEdit->SetSel(chars, chars, FALSE);

				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->GetWndInPlace());
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int chars = pEdit->CharFromPos(pEdit->GetCaretPos());
				if (chars != 0)
					pEdit->SetSel(chars - 1, chars - 1);
				else
					pEdit->SetSel(0, 0);

				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*>(GetCurSel());
		if (prop)
		{
			CEdit* pEdit =  DYNAMIC_DOWNCAST(CEdit, prop->GetWndInPlace());
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				int chars = pEdit->CharFromPos(pEdit->GetCaretPos());
				if (chars < str.GetLength())
					pEdit->SetSel(chars + 1, chars + 1);
				else
					pEdit->SetSel(str.GetLength(), str.GetLength());

				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_HOME && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*>(GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->GetWndInPlace());
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				pEdit->SetSel(0, 0);
				return TRUE;
			}
		}
	}

	if (pMsg->wParam == VK_END && pMsg->message == WM_KEYDOWN && !shiftPressed)
	{
		CTBProperty* prop = dynamic_cast<CTBProperty*> (GetCurSel());
		if (prop)
		{
			CEdit* pEdit = DYNAMIC_DOWNCAST(CEdit, prop->GetWndInPlace());
			if (::IsWindow(pEdit->GetSafeHwnd()))
			{
				CString str;
				pEdit->GetWindowText(str);
				pEdit->SetSel(str.GetLength(), str.GetLength());
				return TRUE;
			}
		}
	}	
	return __super::PreTranslateMessage(pMsg);
}

// ----------------------------------------------------------------------------
CBCGPProp* CustomParametersPropertyGrid::GetNextProperty(CBCGPProp* prop, BOOL cycleFinished /*FALSE*/)
{
	if (!prop->IsExpanded() && !cycleFinished)
		return GetNextProperty(prop, TRUE);

	CBCGPProp* parent = prop->GetParent();

	// if prop has suitems it will return the first subitem
	if (!cycleFinished && prop->IsExpanded())
	{
		for (int i = 0;i < prop->GetSubItemsCount();i++)
		{
			if (prop->GetSubItem(i)->IsVisible())
				return prop->GetSubItem(i);
		}
	}

	// if parent is not property grid
	if (parent) {
		//if it doesnt have subitems it seraches the current property
		for (int i = 0;i < parent->GetSubItemsCount();i++)
		{
			if (parent->GetSubItem(i) == prop)
			{
				//if the property is  the last subitem of parent
				if (i == parent->GetSubItemsCount() - 1)
					return 	GetNextProperty(parent, TRUE);
				else // visit next subitem
					return parent->GetSubItem(i + 1);
			}
		}
	}
	else
	{
		for (int i = 0;i < GetPropertyCount();i++)
		{
			if (GetProperty(i) == prop)
			{
				//if the property is  the last subitem of parent
				if (i == GetPropertyCount() - 1)
					return GetProperty(0);


				else // return next subitem
					return GetProperty(i + 1);

			}
		}
	}

	return prop;
}

// ----------------------------------------------------------------------------
CBCGPProp* CustomParametersPropertyGrid::GetPrevProperty(CBCGPProp* prop, BOOL justStarted /*FALSE*/)
{

	if ((prop->GetSubItemsCount() == 0 || !prop->IsExpanded()) && !justStarted)
		return prop;

	CBCGPProp* parent = prop->GetParent();

	// if prop has suitems it will return the first subitem
	if (!justStarted && prop->IsExpanded())
	{
		for (int i = prop->GetSubItemsCount() - 1;i >= 0;i--)
		{
			if (prop->GetSubItem(i)->IsVisible())
				return GetPrevProperty(prop->GetSubItem(i), FALSE);
		}
	}

	// if parent is not property grid
	if (parent) {
		//if it doesnt have subitems it seraches the current property
		for (int i = parent->GetSubItemsCount() - 1;i >= 0;i--)
		{
			if (parent->GetSubItem(i) == prop)
			{
				//if the property is  the first subitem of parent
				if (i == 0)
					return parent;
				else // visit prev subitem
					return GetPrevProperty(parent->GetSubItem(i - 1), FALSE);
			}
		}
	}
	else
	{
		for (int i = GetPropertyCount() - 1;i >= 0;i--)
		{
			if (GetProperty(i) == prop)
			{
				//if the property is  the first subitem of parent
				if (i == 0)
					return GetPrevProperty(GetProperty(GetPropertyCount() - 1), FALSE);


				else // return prev subitem
					return GetPrevProperty(GetProperty(i - 1), FALSE);

			}
		}
	}

	return prop;
}


//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorParametersView, CRSDockedView);

BEGIN_MESSAGE_MAP(CRSEditorParametersView, CRSDockedView)
END_MESSAGE_MAP()

CRSEditorParametersView::CRSEditorParametersView()
	:
	CRSDockedView(_T("CRSEditorParametersView"), IDD_RS_EditorParametersView),
	m_pPropGridParams(NULL)
{

}

CRSEditorParametersView::~CRSEditorParametersView()
{
	SAFE_DELETE(m_pPropGridParams);
	if (GetFrame())
		GetFrame()->m_pParametersView = NULL;
}


CWoormDocMng* CRSEditorParametersView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorFrame* CRSEditorParametersView::GetFrame()
{
	return dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
}

// ---------------------------------------------------------------------------- -
void CRSEditorParametersView::BuildDataControlLinks()
{
	GetFrame()->m_pParametersView = this;
	m_pPropGridParams = new CustomParametersPropertyGrid;
	m_pPropGridParams->m_pParentForm = this;
	

	if (!m_pPropGridParams->Create(WS_MAXIMIZE | WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP, CRect(0,0,0,0), this, IDC_RS_EditorParametersGrid))
	{
		ASSERT(FALSE);
		delete m_pPropGridParams;
		return;
	}
	m_pPropGridParams->EnableDescriptionArea(FALSE);
}

//-----------------------------------------------------------------------------
void CRSEditorParametersView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	CRect rect;
	GetWindowRect(rect);	
	SetScrollSizes(MM_TEXT, CSize(rect.Width(), 0));
}


//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorSymbolTableViewDebug, CRSDockedView);

BEGIN_MESSAGE_MAP(CRSEditorSymbolTableViewDebug, CRSDockedView)
END_MESSAGE_MAP()

CRSEditorSymbolTableViewDebug::CRSEditorSymbolTableViewDebug()
	:
	CRSDockedView(_T("CRSEditorSymbolTableViewDebug"), IDD_RS_EditorParametersView),
	m_pPropGridParams(NULL)
{

}

CRSEditorSymbolTableViewDebug::~CRSEditorSymbolTableViewDebug()
{
	SAFE_DELETE(m_pPropGridParams);
	if (GetFrame())
		GetFrame()->m_pParametersView = NULL;
}

CWoormDocMng* CRSEditorSymbolTableViewDebug::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorDebugFrame* CRSEditorSymbolTableViewDebug::GetFrame()
{
	return dynamic_cast<CRSEditorDebugFrame*>(__super::GetParentFrame());
}

// ---------------------------------------------------------------------------- -
void CRSEditorSymbolTableViewDebug::BuildDataControlLinks()
{
	GetFrame()->m_pParametersView = this;

	CRect rect;
	GetWindowRect(rect);
	SetScrollSizes(MM_TEXT, CSize(rect.Width(), 0));

	CreateGrid();
	LoadSymbolTable();
}

//-----------------------------------------------------------------------------
void CRSEditorSymbolTableViewDebug::CreateGrid()
{
	ASSERT(m_pPropGridParams == NULL);
	m_pPropGridParams = new CustomParametersPropertyGrid;
	m_pPropGridParams->SetParentForm( this);

	CRect rect;
	GetWindowRect(rect);
	if (!m_pPropGridParams->Create(WS_MAXIMIZE | WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP, CRect(0, 0, 0, 0), this, IDC_RS_EditorParametersGrid))
	{
		ASSERT(FALSE);
		SAFE_DELETE(m_pPropGridParams);
		return;
	}
	m_pPropGridParams->EnableDescriptionArea(FALSE);
	m_pPropGridParams->AdjustLayout();
}

//-----------------------------------------------------------------------------
UINT GetDebuggerIDC(UINT id)
{ return AfxGetTBResourcesMap()->GetTbResourceID(::cwsprintf(L"RS_debugger_%d", id), TbControls); }

void CRSEditorSymbolTableViewDebug::LoadSymbolTable()
{
	CustomParametersPropertyGrid* grid = this->m_pPropGridParams;

	WoormTable* pSymTable = GetDocument()->GetEngineSymTable();

	UINT nFieldIDC = 1;	//
	for (int i = 0; i < pSymTable->GetCount(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		if (!pF->GetData()) continue;

		DataType dt = pF->GetData()->GetDataType();
		if (dt == DataType::Object) dt = DataType::Long;
		else if (dt == DataType::Array) dt = ((DataArray*)pF->GetData())->GetBaseDataType() ;

		const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(dt);
		ASSERT_VALID(pFamily); if (!pFamily) continue;
		CRegisteredParsedCtrl * ctrl = pFamily->GetRegisteredControl(dt);
		ASSERT_VALID(ctrl); if (!ctrl) continue;

		DWORD style = 0;
		if (pF->GetData()->GetDataType() == DataType::Bool)
			style = BS_AUTOCHECKBOX;
		else if (pF->GetData()->GetDataType().m_wType == DATA_ENUM_TYPE)
			style = CBS_DROPDOWNLIST;

		HotKeyLink* pHKL = NULL;

		if (pF->IsInput() || (*pF->GetData(0) == *pF->GetData(1) && *pF->GetData(0) == *pF->GetData(2)))
		{
			if (pF->GetData()->GetDataType() == DataType::Array)
			{
				DataArray* pAr = dynamic_cast<DataArray*>(pF->GetData());
				CTBProperty* propAr = grid->AddProperty(pF->GetName(), pF->GetName(), pF->GetName());
				for (int i = 0; i < min(pAr->GetSize(), 100); i++)
				{
					CString s = cwsprintf(pF->GetName()+L"[%d]", i);
					/*CTBProperty* pChild = */grid->AddSubItem(propAr, s, s, s, pAr->GetAt(i), GetDebuggerIDC(nFieldIDC++), style, ctrl->GetClass());
				}
				propAr->Expand(FALSE);
				continue;
			}
			else if (pF->IsAsk())
			{
				AskFieldData* askField = GetDocument()->GetEditorManager()->GetPrgData()->GetAskRuleData()->GetAskField(pF->GetName());

				BOOL isDynamic, isXml;
				FunctionDataInterface* pDescri = AfxGetTbCmdManager()->GetHotlinkDescription(askField->m_nsHotLink, isDynamic, isXml);

				if (pDescri)
				{
					pHKL = (HotKeyLink*)pDescri->m_pComponentClass->CreateObject();
					pHKL->EnableAddOnFly(FALSE);
					pHKL->MustExistData(FALSE);
				}
			}

			CTBProperty* prop = grid->AddProperty(pF->GetName(), pF->GetName() + L" (" + pF->GetData()->GetDataType().ToString() + L")", L"", pF->GetData(), GetDebuggerIDC(nFieldIDC++), style, ctrl->GetClass(), pHKL);
			if (pF->GetId() > SpecialReportField::REPORT_LOWER_SPECIAL_ID) prop->Enable(FALSE);
			if (!pF->GetData()->IsValid())
				prop->SetState(_TB("Invalid value"));
			continue;
		}
		else
		{	
			CTBProperty* prop = grid->AddProperty(pF->GetName(), pF->GetName() + L" (" + pF->GetData()->GetDataType().ToString() + L")", _TB("Multivalue"));
			for (int i = 0; i < 3; i++)
			{
				CString sName, sDescr, sCaption;
				switch (i)
				{
				case 0:
					sName = L"Rule";
					sCaption = sDescr = _TB("Rule data");
					break;
				case 1:
					sName = L"Query";
					sCaption = sDescr = _TB("Query data");
					break;
				case 2:
					sName = L"Report";
					sCaption = sDescr = _TB("Report data");
					break;
				}
				CTBProperty* pChild = grid->AddSubItem(prop, pF->GetName() + L" - " + sName, sCaption, sDescr, pF->GetData(i), GetDebuggerIDC(nFieldIDC++), style, ctrl->GetClass(), pHKL);
				if (i == pSymTable->GetDataLevel())
					pChild->SetState(_TB("Current value"), _T('>'), RS_COLOR_PROP_IMPORTANT);
			}
		}
	}
	grid->AdjustLayout();
	GetFrame()->UpdateWindow();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorBreakpointViewDebug, CRSDockedView)

BEGIN_MESSAGE_MAP(CRSEditorBreakpointViewDebug, CRSDockedView)
END_MESSAGE_MAP()

CRSEditorBreakpointViewDebug::CRSEditorBreakpointViewDebug()
	:
	CRSDockedView(_T("CRSEditorBreakpointViewDebug"), IDD_RS_EditorParametersView)
{
}

CRSEditorBreakpointViewDebug::~CRSEditorBreakpointViewDebug()
{
	SAFE_DELETE(m_pPropGrid);
	if (GetFrame())
		GetFrame()->m_pBreakpointsView = NULL;
}

CWoormDocMng* CRSEditorBreakpointViewDebug::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorDebugFrame* CRSEditorBreakpointViewDebug::GetFrame()
{
	return dynamic_cast<CRSEditorDebugFrame*>(__super::GetParentFrame());
}

// ---------------------------------------------------------------------------- -
void CRSEditorBreakpointViewDebug::BuildDataControlLinks()
{
	GetFrame()->m_pBreakpointsView = this;

	CRect rect;
	GetWindowRect(rect);
	SetScrollSizes(MM_TEXT, CSize(rect.Width(), 0));

	CreateGrid();

	LoadBreakpoints(NULL);
}

//-----------------------------------------------------------------------------
void CRSEditorBreakpointViewDebug::CreateGrid()
{
	CRect rect;
	CWnd* pPlaceHolder = this->GetDlgItem(IDC_RS_PropertyGrid);
	if (pPlaceHolder)
	{
		pPlaceHolder->GetWindowRect(&rect);
		pPlaceHolder->UnsubclassWindow();
		pPlaceHolder->Detach();
		pPlaceHolder->DestroyWindow();
	}
	else
		GetWindowRect(&rect);

	HWND wnd = m_hWnd;

	ScreenToClient(&rect);

	m_pPropGrid = new CRS_PropertyGrid();

	if (!m_pPropGrid->Create(WS_CHILD | WS_VISIBLE | BS_DEFPUSHBUTTON | WS_BORDER, rect, this, IDC_RS_PropertyGrid))
	{
		ASSERT(FALSE);
		delete m_pPropGrid;
		return;
	}

	// Obbligatorio per impedire di far sparire la toolbar (search, order..) quando viene effettuata la ReLayout. 
	// Se non dovesse bastare, copiare OnEraseBkgnd(CDC* pDC) da TBPropertyGrid.cpp e inserirlo nella message map
	m_pPropGrid->ModifyStyle(WS_CLIPCHILDREN, 0);

	m_pPropGrid->SetCommandsVisible(TRUE);
	m_pPropGrid->EnableSearchBox(TRUE, _TB("Search"));
	m_pPropGrid->EnableToolBar();		  // abilita la toolbar per ordine alfabetico delle properties oppure categorizzato a tree
	m_pPropGrid->EnableHeaderCtrl(FALSE); // Property e Value in testa alla property Grid (eliminati perchè mi sembrano inutili)
	m_pPropGrid->EnableDescriptionArea(); // area sottostante la property grid per la descrizione delle properties
	m_pPropGrid->SetVSDotNetLook();
	m_pPropGrid->MarkModifiedProperties(TRUE);
	m_pPropGrid->SetNameAlign(DT_LEFT);
	m_pPropGrid->SetFont(AfxGetThemeManager()->GetFormFont());
	m_pPropGrid->InitSizeInfo(m_pPropGrid);
}

//-----------------------------------------------------------------------------
void CRSEditorBreakpointViewDebug::LoadBreakpoints(ActionObj* pCurrent)
{
	CWoormDocMng* pDocument = dynamic_cast<CWoormDocMng*>(m_pDocument);
	if (!pDocument->m_pEngine)
		return;

	if (!pDocument->m_pEngine)
		return;
	ASSERT_VALID(pDocument->m_pEngine);
	RepEngine* pEngine = pDocument->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	if (pEngine->m_arTraceActions.GetCount())
	{
		CString s;
		::CStringArray_Concat(pEngine->m_arTraceActions, s, L"\n");
		GetFrame()->m_pDiagnosticView->SetText(s, FALSE);
	}

	m_pPropGrid->RemoveAll();

	for (int i = 0; i < pEngine->m_arBreakpoints.GetCount(); i++)
	{
		CBreakpoint* pB = pEngine->m_arBreakpoints.GetAt(i);
		if (!pB) continue;
		ASSERT_VALID(pB);
		ASSERT_VALID(pB->m_pAction);

		CString sTitle (L"<Breakpoint>");
		Block* pBlock = pB->m_pAction->GetBlockParent();
		if (pBlock)
		{
			sTitle = pBlock->m_strOwnerName + ::cwsprintf(L" - line: %d", pB->m_pAction->m_nDebugUnparseRow - pBlock->m_nDebugUnparseRow + 1);
		}
		CrsProp* propBrk = new CrsProp( (LPCTSTR)sTitle);
		//CRSCheckBoxProp* propBrk = new CRSCheckBoxProp(sTitle,&pB->m_bEnabled);
		m_pPropGrid->AddProperty(propBrk);

		CRS_PropertyGrid::Img iconIdx = CRS_PropertyGrid::Img::BreakPoint;
		if (!pB->m_bEnabled)
		{
			iconIdx = CRS_PropertyGrid::Img::BreakPointDisabled;
		}
		else if (FALSE /*pB is active*/)
		{
			iconIdx = CRS_PropertyGrid::Img::BreakPointCurrent;
		}
		else if (pB->m_erprCondition && !pB->m_erprCondition->IsEmpty() && pB->m_erprAction && !pB->m_erprAction->IsEmpty())
		{
			iconIdx = CRS_PropertyGrid::Img::BreakPointConditionAction;
		}
		else if (pB->m_erprCondition && !pB->m_erprCondition->IsEmpty())
		{
			iconIdx = CRS_PropertyGrid::Img::BreakPointCondition;
		}
		else if (pB->m_erprAction && !pB->m_erprAction->IsEmpty())
		{
			iconIdx = CRS_PropertyGrid::Img::BreakPointAction;
		}
		//propBrk->SetStateImg(iconIdx);

		//TODO Checkbox sulla prima
		CRSBoolProp* propBool = new CRSBoolProp(pB, _TB("Enabled"), &pB->m_bEnabled, L"");
		if (!pB->m_bEnabled) propBool->SetColoredState(CrsProp::State::Mandatory);
		propBrk->AddSubItem(propBool);

		CRSExpressionProp* peCond = new CRSExpressionProp(_TB("Condition"), &pB->m_erprCondition, DataType::Bool,
			 pB->m_pAction->GetSymTable(), this->GetDocument(),
			_TB("The breakpoint will be fired when the condition is true"), TRUE, TRUE);
		peCond->m_bViewMode = FALSE;
		propBrk->AddSubItem(peCond);

		CRSExpressionProp* peAct = new CRSExpressionProp(_TB("Action"), &pB->m_erprAction, DataType::String,
			 pB->m_pAction->GetSymTable(), this->GetDocument(),
			_TB("When the  breakpoint will be hit, it will trace the result of this string expression"), TRUE, TRUE);
		peAct->m_bViewMode = FALSE;
		propBrk->AddSubItem(peAct);

		CRSIntProp* propInt1 = new CRSIntProp(pB, _TB("Hit Count"), &pB->m_nHitCount, _TB("Current breakpoint hit count"), 0, -1);
		propInt1->SetEnable(FALSE); //AllowEdit(FALSE);
		propBrk->AddSubItem(propInt1);

		CRSIntProp* propInt2 = new CRSIntProp(pB, _TB("Activate after hit Count"), &pB->m_nActivateAfterHitCount, _TB("Breakpoint will be fired after this hit count"), 0, -1);
		if (pB->m_nActivateAfterHitCount) propInt2->SetColoredState(CrsProp::State::Important);
		propBrk->AddSubItem(propInt2);

		CRSBoolProp* propBool2 = new CRSBoolProp(pB, _TB("Continue Execution"), &pB->m_bContinueExecution, L"");
		if (pB->m_bContinueExecution) propBool2->SetColoredState(CrsProp::State::Important);
		propBrk->AddSubItem(propBool2);

		CRSIntProp* propInt3 = new CRSIntProp(pB, _TB("Max traced rows"), &pB->m_nTraceRows, _TB("Max number of traced rows"), 0, -1);
		if (pB->m_nTraceRows) propInt3->SetColoredState(CrsProp::State::Important);
		propBrk->AddSubItem(propInt3);

		//---------------------------
		if (pB->m_pAction == pCurrent || (pCurrent == NULL && i == (pEngine->m_arBreakpoints.GetCount()-1)))
			propBrk->Expand(TRUE);
		else propBrk->Expand(FALSE);
	}
	//----
	m_pPropGrid->AdjustLayout();
	GetFrame()->UpdateWindow();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorDiagnosticView, CRSDockedView);

BEGIN_MESSAGE_MAP(CRSEditorDiagnosticView, CRSDockedView)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRSEditorDiagnosticView::CRSEditorDiagnosticView()
	:
	CRSDockedView(_T("CRSEditorDiagnosticView"), IDD_RS_EditorDiagnosticView){
}

CRSEditorDiagnosticView::~CRSEditorDiagnosticView()
{
	if (GetFrame())
		GetFrame()->m_pDiagnosticView = NULL;
}

// ---------------------------------------------------------------------------- -
CWoormDocMng* CRSEditorDiagnosticView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorFrame* CRSEditorDiagnosticView::GetFrame()
{
	return dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
}

BOOL CRSEditorDiagnosticView::PreTranslateMessage(MSG* pMsg)
{

	if (pMsg->message == WM_MOUSEWHEEL)
	{	
		CString text;
		m_edtErrors.GetWindowText(text);
		if (text.IsEmpty())
			return TRUE;

		short direction = GET_WHEEL_DELTA_WPARAM(pMsg->wParam);		
		if (direction < 0)
			OnScrollDown();
		else
			OnScrollUp();
	}

	return __super::PreTranslateMessage(pMsg);
}

// ---------------------------------------------------------------------------- -
void CRSEditorDiagnosticView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	
	ShowScrollBar(SB_BOTH, FALSE);
	m_edtErrors.ShowScrollBar(SB_BOTH, FALSE);
	m_edtErrors.SetTextColor(0);
}

// ---------------------------------------------------------------------------- -
void CRSEditorDiagnosticView::BuildDataControlLinks()
{
	GetFrame()->m_pDiagnosticView = this;
	m_edtErrors.SubclassEdit(IDC_RS_EditorDiagnosticEdit, this);
}

// ---------------------------------------------------------------------------- -
void CRSEditorDiagnosticView::SetText(const CString& sText, BOOL bAppend/* = TRUE*/)
{
	if (sText.IsEmpty()) 
		return;

	CString s;
	if (bAppend)
	{
		m_edtErrors.GetWindowText(s);
		if (!s.IsEmpty()) s += L"\r\n";
	}
	s += sText;
	m_edtErrors.SetWindowText(s);
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorGridView, CRSDockedView);

BEGIN_MESSAGE_MAP(CRSEditorGridView, CRSDockedView)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------

CRSEditorGridView::CRSEditorGridView()
	:
	CRSDockedView(_T("CRSEditorGridView"), IDD_RS_EditorGridView),
	m_pGrdTable(NULL)
{
}

CRSEditorGridView::~CRSEditorGridView()
{
	SAFE_DELETE(m_pGrdTable);
	if (GetFrame())
		GetFrame()->m_pGridView = NULL;
}

// ---------------------------------------------------------------------------- -
CWoormDocMng* CRSEditorGridView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorFrame* CRSEditorGridView::GetFrame()
{
	return dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
}

// ---------------------------------------------------------------------------- -
void CRSEditorGridView::BuildDataControlLinks()
{
	GetFrame()->m_pGridView = this;
	m_pGrdTable = new CTBGridControlResizable;
	m_pGrdTable->Create(WS_CHILD | BF_FLAT | WS_VISIBLE, CRect(0,0,0,0), this, IDC_RS_EditorDiagnosticGrid);
	m_pGrdTable->OnRecalcCtrlSize(WM_USER, WM_KEYDOWN);
}

//=============================================================================

/////////////////////////////////////////////////////////////////////////////
// CRSEditView

IMPLEMENT_DYNCREATE(CRSEditView, CBCGPEditView);

BEGIN_MESSAGE_MAP(CRSEditView, CBCGPEditView)

	ON_WM_CREATE()
	ON_WM_NCPAINT()
	ON_WM_CONTEXTMENU()

	//ON_COMMAND(ID_EDIT_TOGGLEBOOKMARK, OnEditTogglebookmark)
	//ON_COMMAND(ID_EDIT_CLEAR_ALLBOOKMARKS, OnClearAllBookmarks)
	////ON_UPDATE_COMMAND_UI(ID_EDIT_CLEAR_ALLBOOKMARKS, OnUpdateClearAllBookmarks)
	//ON_COMMAND(ID_EDIT_NEXTBOOKMARK, OnEditNextbookmark)
	//ON_COMMAND(ID_EDIT_PREVIOUSBOOKMARK, OnEditPreviousbookmark)

	//ON_COMMAND(ID_EDIT_CHECK, OnCheckPressed)
	//ON_COMMAND(ID_EDIT_EXEC, OnExecPressed)

	ON_COMMAND(ID_EDIT_LISTMEMBERS, OnEditListmembers)
	ON_COMMAND(ID_EDIT_INCREASE_INDENT, OnEditIncreaseIndent)
	ON_UPDATE_COMMAND_UI(ID_EDIT_INCREASE_INDENT, OnUpdateEditIncreaseIndent)
	ON_COMMAND(ID_EDIT_DECREASE_INDENT, OnEditDecreaseIndent)
	ON_UPDATE_COMMAND_UI(ID_EDIT_DECREASE_INDENT, OnUpdateEditDecreaseIndent)
	ON_UPDATE_COMMAND_UI(ID_EDIT_LISTMEMBERS, OnUpdateEditListmembers)
	ON_COMMAND(ID_EDIT_HIDESELECTION, OnEditHideselection)
	ON_COMMAND(ID_EDIT_STOPHIDINGCURRENT, OnEditStophidingcurrent)
	ON_COMMAND(ID_EDIT_TOGGLEOUTLINING, OnEditToggleoutlining)
	ON_COMMAND(ID_EDIT_TOGGLEALLOUTLINING, OnEditTogglealloutlining)
	ON_COMMAND(ID_EDIT_COLLAPSETODEFINITIONS, OnEditCollapsetodefinitions)
	ON_COMMAND(ID_EDIT_STOPOUTLINING, OnEditStopoutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_STOPHIDINGCURRENT, OnUpdateEditStophidingcurrent)
	ON_UPDATE_COMMAND_UI(ID_EDIT_HIDESELECTION, OnUpdateEditHideselection)
	ON_COMMAND(ID_EDIT_AUTOOUTLINING, OnEditAutooutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_AUTOOUTLINING, OnUpdateEditAutooutlining)
	ON_COMMAND(ID_EDIT_ENABLEOUTLINING, OnEditEnableoutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_ENABLEOUTLINING, OnUpdateEditEnableoutlining)
	ON_COMMAND(ID_EDIT_LINENUMBERS, OnEditLinenumbers)
	ON_UPDATE_COMMAND_UI(ID_EDIT_LINENUMBERS, OnUpdateEditLinenumbers)
	ON_UPDATE_COMMAND_UI(ID_EDIT_STOPOUTLINING, OnUpdateEditStopoutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_TOGGLEALLOUTLINING, OnUpdateEditTogglealloutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_TOGGLEOUTLINING, OnUpdateEditToggleoutlining)
	ON_UPDATE_COMMAND_UI(ID_EDIT_COLLAPSETODEFINITIONS, OnUpdateEditCollapsetodefinitions)
	
	ON_UPDATE_COMMAND_UI(ID_CARET_POS, OnUpdateCaretPos)
	ON_COMMAND(ID_EDIT_SHOWHINT, OnShowHint)

END_MESSAGE_MAP()
//-----------------------------------------------------------------------------

CRSEditView::CRSEditView()
	:
	CParsedForm(this, IDD_RS_EditView, L"EditView"),
	IDisposingSourceImpl(this),
	m_pbSaved (NULL),
	m_pEditTextRect(NULL)
{
	m_bXMLSettings = FALSE;
}

CRSEditView::~CRSEditView()
{
	//if (GetEditorFrame(FALSE))
	//	GetEditorFrame(TRUE)->m_pEditView = NULL;
	SAFE_DELETE(m_pEditTextRect);
}

//-----------------------------------------------------------------------------
CWoormDocMng* CRSEditView::GetDocument()
{
	CWoormDocMng* pWDoc = dynamic_cast<CWoormDocMng*>(CView::GetDocument());
	ASSERT_VALID(pWDoc);
	return pWDoc;
}

CRSEditorFrame* CRSEditView::GetEditorFrame(BOOL bMustExists)
{
	CRSEditorFrame* pFr = dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
	if (bMustExists)
	{
		ASSERT_VALID(pFr);
	}
	return pFr;
}

CCustomEditCtrl* CRSEditView::GetEditCtrl() const
{
	CCustomEditCtrl* pCtrlEdit = dynamic_cast<CCustomEditCtrl*>(__super::GetEditCtrl());
	ASSERT_VALID(pCtrlEdit);
	return pCtrlEdit;
}

// ---------------------------------------------------------------------------- -
void CRSEditView::DoEvent(BOOL bWaitBusy/* = FALSE*/)
{
	GetEditorFrame(TRUE)->SetWoormEditorVisible(true);

	TDisposablePtr<CRSEditView> pSView = this;

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();

	CWoormFrame* pWFrame = GetDocument()->GetWoormFrame();
	if (pWFrame && ::IsWindow(pWFrame->m_hWnd) && pWFrame->IsWindowEnabled() && ::IsWindowVisible(pWFrame->m_hWnd)) // because if a window is already disabled, its child windows are implicitly disabled
	{	
		pWFrame->EnableWindow(FALSE);

		//NO REFACTORING
		GetEditorFrame(TRUE)->ShowWindow(SW_SHOWMINNOACTIVE);
		CTBWinThread::PumpThreadMessages();
		GetEditorFrame(TRUE)->ShowWindow(SW_SHOWNORMAL);
		CTBWinThread::PumpThreadMessages();
	}

	//NO REFACTORING
	CCustomEditCtrl* d = GetEditCtrl();
	d->SendMessage(WM_LBUTTONDOWN, MK_LBUTTON, MAKELPARAM(d->GetCaretPos().x, d->GetCaretPos().y));
	d->SendMessage(WM_LBUTTONUP, MK_LBUTTON, MAKELPARAM(d->GetCaretPos().x, d->GetCaretPos().y));

	d->ReplaceSel(L"  ", TRUE);
	d->OnUndo();
		
	while (		
			pSView && pSView->m_hWnd &&
			(bWaitBusy ? pSView->m_bBusy : TRUE) &&
			//::IsWindow(pSView->m_hWnd) &&
			//::IsWindowVisible(pSView->m_hWnd) && 
			CTBWinThread::PumpThreadMessages()
		)
		Sleep(10);
	
	if (pWFrame)
		pWFrame->EnableWindow(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// CRSEditView diagnostics

#ifdef _DEBUG
void CRSEditView::AssertValid() const
{
	CBCGPEditView::AssertValid();
}

//-----------------------------------------------------------------------------
void CRSEditView::Dump(CDumpContext& dc) const
{
	CBCGPEditView::Dump(dc);
}

#endif 
//////_DEBUG//////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////
// CRSEditView message handlers

//-----------------------------------------------------------------------------
CBCGPEditCtrl* CRSEditView::CreateEdit()
{
	return new CCustomEditCtrl;
}

//-----------------------------------------------------------------------------
void CRSEditView::DoCreate()
{
	ASSERT_VALID(GetEditCtrl());

	m_strCaretPosFmt = AfxLoadTBString(ID_CARET_POS);

	ResetDefaultFont();

	OnChangeVisualStyle();

	GetEditCtrl()->EnableToolTips();

	GetEditCtrl()->EnableOutlining();
	GetEditCtrl()->EnableAutoOutlining();
	GetEditCtrl()->SetOutlineMargin();
	GetEditCtrl()->SetLineNumbersMargin(TRUE, 30, 10);
	GetEditCtrl()->EnableOutlineParser(FALSE);

	DisableMainframeForFindDlg(FALSE);

	ResetFindCombo();
	ResetReplaceCombo();

	//CWoormDocMng* pDoc = GetDocument();
	 /*if (pDoc != NULL)
	{
		ASSERT_VALID(pDoc);
		pDoc->SetModifiedflag(GetEditCtrl()->IsModified());
	}*/

	ModifyStyle(0, WS_VISIBLE);
}

int CRSEditView::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CBCGPEditView::OnCreate(lpCreateStruct) == -1)
		return -1;

	DoCreate();

	return 0;
}

//-----------------------------------------------------------------------------
void CRSEditView::OnInitialUpdate()
{
	__super::OnInitialUpdate();

	if (GetEditorFrame(FALSE))
		GetEditorFrame(TRUE)->m_pEditView = this;

	ASSERT_VALID(GetEditCtrl());
}

//-----------------------------------------------------------------------------
void CRSEditView::SetLanguage(const CString& language, BOOL viewMode)
{
	GetEditCtrl()->UseXmlParser(TRUE);
	
	CString filename;
	if (language.CompareNoCase(L"WRM") == 0)
		filename = L"WoormLanguage.xml";
	else if (language.CompareNoCase(L"SQL") == 0)
		filename = L"SqlLanguage.xml";
	else return;

	CString m_uiExampleAddress = AfxGetPathFinder()->GetModuleXmlPath
				(CTBNamespace(L"module.framework.tbwoormviewer"), CPathFinder::PosType::STANDARD) + "\\" + filename;
	/*if (m_uiExampleAddress.IsEmpty())
	{
		GetDocument()->SetModifiedFlag(GetEditCtrl()->IsModified());
	}*/

	GetEditCtrl()->LoadXMLSettings(m_uiExampleAddress);
	GetEditCtrl()->EnableIntelliSense(TRUE);

	GetEditCtrl()->EnableOutlineParser(TRUE);
	GetEditCtrl()->UpdateAutoOutlining();

	GetEditCtrl()->ColorVariables(GetDocument(), viewMode);
}

//-----------------------------------------------------------------------------
void CRSEditView::ResetFindCombo()
{
	if (!GetEditorFrame(FALSE)) return;

	CBCGPToolbarComboBoxButton* pFindCombo = GetEditorFrame(TRUE)->GetFindCombo();

	if ((pFindCombo != NULL) && (pFindCombo->GetCount()))
	{
		CString strText;
		pFindCombo->GetComboBox()->SetCurSel(0);
		pFindCombo->GetComboBox()->GetLBText(0, strText);

		if (!strText.IsEmpty())
		{
			pFindCombo->SetText(strText);
			m_strFindText = strText;
		}
	}
}

//-----------------------------------------------------------------------------
void CRSEditView::ResetReplaceCombo()
{
	if (!GetEditorFrame(FALSE)) return;

	CBCGPToolbarComboBoxButton* pRepCombo = GetEditorFrame(TRUE)->GetReplaceCombo();

	if ((pRepCombo != NULL) && (pRepCombo->GetCount()))
	{
		CString strText;
		pRepCombo->GetComboBox()->SetCurSel(0);
		pRepCombo->GetComboBox()->GetLBText(0, strText);

		if (!strText.IsEmpty())
		{
			pRepCombo->SetText(strText);
			m_strFindText = strText;
		}
	}
}

//-----------------------------------------------------------------------------
void CRSEditView::OnUpdateCaretPos(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());

	CString str;
	str.Format(m_strCaretPosFmt, GetEditCtrl()->GetCurRow() + 1, GetEditCtrl()->GetCurColumn() + 1);

	pCmdUI->SetText(str);

	pCmdUI->Enable();
}

//-----------------------------------------------------------------------------
void CRSEditView::ResetDefaultFont()
{
	//IL FONT VA CARICATO DINAMICAMENTE!!!!!!!!
	HFONT hFont = (HFONT)::GetStockObject(DEFAULT_GUI_FONT);
	CDC* pDC = GetDC();

	CFont* pFont = pDC->SelectObject(CFont::FromHandle(hFont));
	pDC->SelectObject(pFont);
	::DeleteObject(hFont);

	LOGFONT lf;
	pFont->GetLogFont(&lf);

	CString strFontName(_T("Courier New"));
	CopyMemory(lf.lfFaceName, (LPCTSTR)strFontName, (strFontName.GetLength() + 1) * sizeof(TCHAR));
	lf.lfWidth = 0;
	lf.lfEscapement = 0;
	lf.lfOrientation = 0;
	lf.lfWeight = FW_NORMAL;
	lf.lfItalic = 0;
	lf.lfUnderline = 0;
	lf.lfStrikeOut = 0;
	lf.lfHeight = ScalePix(lf.lfHeight);

	m_Font.CreateFontIndirect(&lf);
	GetEditCtrl()->SetFont(&m_Font);
	ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CRSEditView::VerifyFindString(CBCGPToolbarComboBoxButton* pFindCombo, CString& strFindText)
{
	if (pFindCombo == NULL)
	{
		return;
	}

	BOOL bIsLastCommandFromButton = CBCGPToolBar::IsLastCommandFromButton(pFindCombo);

	if (bIsLastCommandFromButton)
	{
		strFindText = pFindCombo->GetText();
	}

	CComboBox* pCombo = pFindCombo->GetComboBox();

	if (!strFindText.IsEmpty())
	{
		const int nCount = pCombo->GetCount();
		int ind = 0;
		CString strCmpText;

		while (ind < nCount)
		{
			pCombo->GetLBText(ind, strCmpText);

			if (strCmpText.GetLength() == strFindText.GetLength())
			{
				if (strCmpText == strFindText)
					break;
			}

			ind++;
		}

		if (ind < nCount)
		{
			pCombo->DeleteString(ind);
		}

		pCombo->InsertString(0, strFindText);
		pCombo->SetCurSel(0);

		if (!bIsLastCommandFromButton)
		{
			pFindCombo->SetText(strFindText);
		}
	}
}

void CRSEditView::OnShowHint()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->ShowContextTip();
}

void CRSEditView::OnReplace()
{
	if (!GetEditorFrame(FALSE)) return;

	if (GetEditCtrl()->GetSelText().IsEmpty())
		OnFindNext();
	else
	{
		CString replaceText;
		GetEditorFrame(TRUE)->GetReplaceCombo()->GetEditCtrl()->GetWindowTextW(replaceText);
		VerifyFindString(GetEditorFrame(TRUE)->GetReplaceCombo(), replaceText);
		GetEditCtrl()->ReplaceSel(replaceText, TRUE);
	}
}

void CRSEditView::OnReplaceAll()
{
	if (!GetEditorFrame(FALSE)) return;

	GetEditorFrame(TRUE)->GetFindCombo()->GetEditCtrl()->GetWindowTextW(m_strFindText);
	CString replaceText;
	GetEditorFrame(TRUE)->GetReplaceCombo()->GetEditCtrl()->GetWindowTextW(replaceText);
	if (m_strFindText.IsEmpty())
	{
		GetEditorFrame(TRUE)->GetFindCombo()->NotifyCommand(CBN_SETFOCUS);
		return;
	}
	VerifyFindString(GetEditorFrame(TRUE)->GetReplaceCombo(), replaceText);
	GetEditCtrl()->ReplaceAll(m_strFindText, replaceText,1,0,0);
	m_strFindText.Empty();
}

void CRSEditView::OnFind()
{	
	if (!GetEditorFrame(FALSE)) return;

	
	if (m_strFindText.IsEmpty())
		GetEditorFrame(TRUE)->GetFindCombo()->GetEditCtrl()->GetWindowTextW(m_strFindText);
	if (m_strFindText.IsEmpty())
		return;

	VerifyFindString(GetEditorFrame(TRUE)->GetFindCombo(),
		m_strFindText);
	
	GetEditCtrl()->SetActiveWindow();
	OnFindReplace(0, 0);
}

void CRSEditView::OnFindNext()		  
{
	m_dwFindMask |= FR_DOWN;
	OnFind();
	m_strFindText.Empty();
}

void CRSEditView::OnFindPrev()
{
	m_dwFindMask &= ~FR_DOWN;
	OnFind();
	m_strFindText.Empty();
}

void CRSEditView::OnTextNotFound(LPCTSTR lpszFind)
{
	CString strError = lpszFind == NULL ? _T("") : (lpszFind + _TB(" Not Found!")) ;

	BCGPMessageBox(strError);
}

void CRSEditView::OnEditListmembers()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->InvokeIntelliSense();
}

void CRSEditView::OnUpdateEditListmembers(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());

	if (GetFocus() != GetEditCtrl())
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	CObList lstIntelliSenseData;

	GetEditCtrl()->FillIntelliSenseList(lstIntelliSenseData);
	pCmdUI->Enable(!lstIntelliSenseData.IsEmpty());

	GetEditCtrl()->ReleaseIntelliSenseList(lstIntelliSenseData);
}

void CRSEditView::AttachXMLSettings(const CString& /*strXMLFileName*/)
{
	ASSERT_VALID(GetEditCtrl());

	GetEditCtrl()->RemoveXMLSettings();
}

void CRSEditView::GetUndoActions(CStringList& lstActions) const
{
	GetUndoRedoActions(lstActions, TRUE);
}

void CRSEditView::GetRedoActions(CStringList& lstActions) const
{
	GetUndoRedoActions(lstActions, FALSE);
}

void CRSEditView::GetUndoRedoActions(CStringList& lstActions, BOOL bUndo) const
{
	ASSERT_VALID(GetEditCtrl());

	CDWordArray dwaUAT;

	//	Get undo/redo actions:
	if (bUndo)
	{
		GetEditCtrl()->GetUndoActions(dwaUAT);
	}
	else
	{
		GetEditCtrl()->GetRedoActions(dwaUAT);
	}

	//	Setup undo/redo actions:
	lstActions.RemoveAll();

	int nIndex = (int)dwaUAT.GetSize();
	if (nIndex != 0)
	{
		CString strAction;

		while (nIndex--)
		{
			strAction.Empty();

			if (!UATToString(dwaUAT.GetAt(nIndex), strAction))
			{
				ASSERT(FALSE);
				strAction = AfxLoadTBString(ID_UAT_UNKNOWN);
			}

			if (strAction.IsEmpty())
			{
				ASSERT(FALSE);
				strAction = _T("<?>");
			}

			lstActions.AddHead(strAction);
		}
	}
}

BOOL CRSEditView::UATToString(DWORD dwUAT, CString& strAction) const
{
	switch (dwUAT & UAT_REASON)
	{
	case g_dwUATUndefined:
		strAction = AfxLoadTBString(ID_UAT_UNDEFINED);
		break;

	case g_dwUATTyping:
		strAction = AfxLoadTBString(ID_UAT_TYPING);
		break;

	case g_dwUATCut:
		strAction = AfxLoadTBString(ID_UAT_CUT);
		break;

	case g_dwUATPaste:
		strAction = AfxLoadTBString(ID_UAT_PASTE);
		break;

	case g_dwUATDelete:
		strAction = AfxLoadTBString(ID_UAT_DELETE);
		break;

	case g_dwUATBackspace:
		strAction = AfxLoadTBString(ID_UAT_BACKSPACE);
		break;

	case g_dwUATDragDrop:
		strAction = AfxLoadTBString(ID_UAT_DRAGDROP);
		break;

	case g_dwUATEnter:
		strAction = AfxLoadTBString(ID_UAT_ENTER);
		break;

	case g_dwUATIndent:
		strAction = AfxLoadTBString(ID_UAT_INDENT);
		break;

	case g_dwUATUnindent:
		strAction = AfxLoadTBString(ID_UAT_UNINDENT);
		break;

	case g_dwUATTab:
		strAction = AfxLoadTBString(ID_UAT_TAB);
		break;

	case g_dwUATReplace:
		strAction = AfxLoadTBString(ID_UAT_REPLACE);
		break;

	default:
		return FALSE;
	}

	return TRUE;
}


void CRSEditView::OnEditIncreaseIndent()
{
	ASSERT_VALID(GetEditCtrl());
	VERIFY(GetEditCtrl()->IndentSelection(TRUE));
}

void CRSEditView::OnUpdateEditIncreaseIndent(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(GetEditCtrl()->IsIndentEnabled(TRUE));
}

void CRSEditView::OnEditDecreaseIndent()
{
	ASSERT_VALID(GetEditCtrl());
	VERIFY(GetEditCtrl()->IndentSelection(FALSE));
}

void CRSEditView::OnUpdateEditDecreaseIndent(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(GetEditCtrl()->IsIndentEnabled(FALSE));
}

BOOL CRSEditView::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	ASSERT_VALID(GetEditCtrl());

	return CBCGPEditView::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

void CRSEditView::OnEditFindNextWord()
{
	ASSERT_VALID(GetEditCtrl());

	CString strWord;
	if (GetEditCtrl()->GetWordFromOffset(GetEditCtrl()->GetCurOffset(), strWord))
	{
		GetEditCtrl()->FindText(strWord, TRUE, TRUE, TRUE);
	}
}

void CRSEditView::OnEditHideselection()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->HideSelection();
}

void CRSEditView::OnUpdateEditHideselection(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());

	int nStart = 0;
	int nEnd = 0;
	GetEditCtrl()->GetSel(nStart, nEnd);

	pCmdUI->Enable(
		nStart >= 0 && nEnd >= 0 && (nStart != nEnd) &&
		GetEditCtrl()->IsOutliningEnabled() &&
		!GetEditCtrl()->IsAutoOutliningEnabled());
}

void CRSEditView::OnEditStophidingcurrent()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->StopHidingCurrent();
}

void CRSEditView::OnUpdateEditStophidingcurrent(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(
		GetEditCtrl()->IsOutliningEnabled() &&
		!GetEditCtrl()->IsAutoOutliningEnabled());
}

void CRSEditView::OnEditToggleoutlining()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->ToggleOutlining();
}

void CRSEditView::OnUpdateEditToggleoutlining(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(GetEditCtrl()->IsOutliningEnabled());
}

void CRSEditView::OnEditTogglealloutlining()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->ToggleAllOutlining();
}

void CRSEditView::OnUpdateEditTogglealloutlining(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(GetEditCtrl()->IsOutliningEnabled());
}

void CRSEditView::OnEditCollapsetodefinitions()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->CollapseToDefinitions();
}

void CRSEditView::OnUpdateEditCollapsetodefinitions(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(GetEditCtrl()->IsOutliningEnabled());
}

void CRSEditView::OnEditStopoutlining()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->StopOutlining();
}

void CRSEditView::OnUpdateEditStopoutlining(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->Enable(
		GetEditCtrl()->IsOutliningEnabled() &&
		GetEditCtrl()->IsAutoOutliningEnabled());
}

void CRSEditView::OnEditAutooutlining()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->EnableAutoOutlining(!GetEditCtrl()->IsAutoOutliningEnabled());
}

void CRSEditView::OnUpdateEditAutooutlining(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->SetCheck(GetEditCtrl()->IsAutoOutliningEnabled() ? 1 : 0);
	pCmdUI->Enable(GetEditCtrl()->IsOutliningEnabled());
}

void CRSEditView::OnEditEnableoutlining()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->EnableOutlining(!GetEditCtrl()->IsOutliningEnabled());
	GetEditCtrl()->SetOutlineMargin(GetEditCtrl()->IsOutliningEnabled());
}

void CRSEditView::OnUpdateEditEnableoutlining(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->SetCheck(GetEditCtrl()->IsOutliningEnabled() ? 1 : 0);
}

void CRSEditView::OnEditLinenumbers()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->SetLineNumbersMargin(!GetEditCtrl()->IsLineNumbersMarginVisible(), 30, 10);
}

void CRSEditView::OnUpdateEditLinenumbers(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	pCmdUI->SetCheck(GetEditCtrl()->IsLineNumbersMarginVisible() ? 1 : 0);
}

void CRSEditView::OnChangeVisualStyle()
{
	// CARICATO DINAMICAMENTE
	/*if (m_imgList.GetSafeHandle() != NULL)
	{
		m_imgList.DeleteImageList();
	}

	UINT uiBmpId = theApp.m_bHiColorIcons ?
	IDB_IMG_INTELLISENSE_HC : IDB_IMG_INTELLISENSE;

	CBitmap bmp;
	if (!bmp.LoadBitmap(uiBmpId))
	{
		TRACE(_T("Can't load bitmap: %x\n"), uiBmpId);
		ASSERT(FALSE);
		return;
	}

	BITMAP bmpObj;
	bmp.GetBitmap(&bmpObj);

	UINT nFlags = ILC_MASK;

	switch (bmpObj.bmBitsPixel)
	{
	case 4:
	default:
		nFlags |= ILC_COLOR4;
		break;

	case 8:
		nFlags |= ILC_COLOR8;
		break;

	case 16:
		nFlags |= ILC_COLOR16;
		break;

	case 24:
		nFlags |= ILC_COLOR24;
		break;

	case 32:
		nFlags |= ILC_COLOR32;
		break;
	}

	m_imgList.Create(16, bmpObj.bmHeight, nFlags, 0, 0);
	m_imgList.Add(&bmp, RGB(255, 255, 255));

	GetEditCtrl()->SetIntelliSenseImgList(&m_imgList);*/
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
void CRSEditView::OnNcPaint()
{
	if ((GetExStyle() & WS_EX_CLIENTEDGE) == 0)
	{
		Default();
		return;
	}

	CWindowDC dc(this);

	CRect rect;
	GetWindowRect(rect);

	rect.OffsetRect(-rect.TopLeft());

	dc.Draw3dRect(rect, globalData.clrBarFace, globalData.clrBarFace);

	rect.DeflateRect(1, 1);
	dc.Draw3dRect(rect, globalData.clrBarFace, globalData.clrBarFace);

	//__super::OnNcPaint();
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::PreTranslateMessage(MSG* pMsg)
{
	BOOL ctrlPressed = GetKeyState(VK_CONTROL) & 0x8000;
	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000;

	if (ctrlPressed && (pMsg->message == WM_KEYDOWN)){

		if (pMsg->wParam == 'H')
		{
			if (!GetEditorFrame(FALSE)) return FALSE;

			CString selText = GetEditCtrl()->GetSelText();
			selText.Trim();
			if (!selText.IsEmpty())
				GetEditorFrame(TRUE)->GetFindCombo()->GetEditCtrl()->SetWindowTextW(selText);

			OnReplace();
			return TRUE;
		}
	}

	if (pMsg->wParam == 0x53 && (pMsg->message == WM_KEYDOWN) && ctrlPressed)
	{
		if (!DoSave())
			return FALSE;
		return TRUE;
	}

	if (pMsg->wParam == VK_F3 && (pMsg->message == WM_KEYDOWN))
	{
		if (!GetEditorFrame(FALSE)) return FALSE;

		m_strFindText = GetEditCtrl()->GetSelText();
		m_strFindText.Trim();
		GetEditorFrame(TRUE)->GetFindCombo()->GetEditCtrl()->SetWindowTextW(m_strFindText);

		if (shiftPressed)
			OnFindPrev();
		else
			OnFindNext();
	}

	if (pMsg->wParam == VK_RETURN && (pMsg->message == WM_KEYDOWN))
	{
		if (!GetEditorFrame(FALSE)) return FALSE;

		CString searchText;
		GetEditorFrame(TRUE)->GetFindCombo()->GetEditCtrl()->GetWindowTextW(searchText);
		CString selText = GetEditCtrl()->GetSelText();
		if (!searchText.IsEmpty() && !selText.IsEmpty() && searchText.CompareNoCase(selText) == 0)
		{
			 OnFindNext();
			 return TRUE;
		}
	}

	if (pMsg->wParam == VK_ESCAPE && (pMsg->message == WM_KEYDOWN))
	{
		if (!GetEditorFrame(FALSE)) return FALSE; 

		/*if (GetEditCtrl()->GetIntellisenseWnd())
		{
		   GetEditCtrl()->GetIntellisenseWnd()->DestroyWindow();
		   return TRUE;
		}*/
			

		GetEditorFrame(TRUE)->OnClose();
		return TRUE;
		
	}
	//Select word with double mouse click
	// Dont move into CCustomEditCtrl
	if (pMsg->message == WM_LBUTTONDBLCLK)
	{
		GetEditCtrl()->MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_PREV_WORD);
		int offset = GetEditCtrl()->GetCurOffset();
		GetEditCtrl()->SetSel(offset, offset);
		GetEditCtrl()->MakeSelection(CBCGPEditCtrl::BCGP_EDIT_SEL_TYPE::ST_NEXT_WORD);
		CString selText = GetEditCtrl()->GetSelText();
		if (selText.Right(1).Compare(L" ") == 0)
			GetEditCtrl()->SetSel(offset, GetEditCtrl()->GetCurOffset() - 1);
		return TRUE;
	}

	if (pMsg->message == WM_MOUSEWHEEL)
	{
		CString text = GetEditCtrl()->GetText();

		if (text.IsEmpty())
			return TRUE;

		short direction = GET_WHEEL_DELTA_WPARAM(pMsg->wParam);
		if (direction < 0)
			GetEditCtrl()->ScrollDown(SB_VERT,TRUE);
		else
			GetEditCtrl()->ScrollUp(SB_VERT,TRUE);
		
	}
	
	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void CRSEditView::OnEditTogglebookmark()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->ToggleMarker(GetEditCtrl()->GetCurRow(), g_dwBCGPEdit_FirstUserDefinedMarker);
}

//-----------------------------------------------------------------------------
void CRSEditView::OnClearAllBookmarks()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->DeleteAllMarkers(g_dwBCGPEdit_FirstUserDefinedMarker);
}

//-----------------------------------------------------------------------------
void CRSEditView::OnEditNextbookmark()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->GoToNextMarker(g_dwBCGPEdit_FirstUserDefinedMarker, TRUE);
}

//-----------------------------------------------------------------------------
void CRSEditView::OnEditPreviousbookmark()
{
	ASSERT_VALID(GetEditCtrl());
	GetEditCtrl()->GoToNextMarker(g_dwBCGPEdit_FirstUserDefinedMarker, FALSE);
}

//-----------------------------------------------------------------------------
void CRSEditView::OnUpdateClearAllBookmarks(CCmdUI* pCmdUI)
{
	ASSERT_VALID(GetEditCtrl());
	//pCmdUI->Enable(GetEditCtrl()->HasMarkers(g_dwBCGPEdit_BookMark));
}

//=============================================================================

/////////////////////////////////////////////////////////////////////////////
// CRSEditViewFullText

IMPLEMENT_DYNCREATE(CRSEditViewFullText, CRSEditView);
//-----------------------------------------------------------------------------
CRSEditViewFullText::CRSEditViewFullText():CRSEditView()
{
	m_Context.SetFullReport(L"");
}

//=============================================================================

/////////////////////////////////////////////////////////////////////////////
// CRSEditViewDebug

IMPLEMENT_DYNCREATE(CRSEditViewDebug, CRSEditView);
//-----------------------------------------------------------------------------
CRSEditViewDebug::CRSEditViewDebug()
	:
	CRSEditView()
{
}

//-----------------------------------------------------------------------------
CRSEditorDebugFrame* CRSEditViewDebug::GetEditorFrame(BOOL bMustExists)
{
	return dynamic_cast<CRSEditorDebugFrame*>(__super::GetEditorFrame(bMustExists));
}

//-----------------------------------------------------------------------------
BOOL CRSEditViewDebug::OpenDebugger(ActionObj* pCurrCmd)
{
	if (!GetEditorFrame(FALSE))
		return FALSE;

	GetEditCtrl()->EnableBreakpoints();

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pToolTreeView);

	if (pCurrCmd)
	{
		ASSERT_VALID(pCurrCmd);

		m_Context.SetDebugAction(pCurrCmd);
	}

	CRSEditorToolDebugView* pTDView = dynamic_cast<CRSEditorToolDebugView*>(GetEditorFrame(TRUE)->m_pToolTreeView);
	ASSERT_VALID(pTDView);
	pTDView->m_pEditView = this;

	pTDView->FillForDebug(this);

	ShowDiagnostic(L"");

	if (pCurrCmd)
	{
		ActionObj* pRootCmd = pCurrCmd->GetRootParent();
		ASSERT_VALID(pRootCmd);

		GetEditorFrame(TRUE)->m_pToolTreeView->m_TreeCtrl.SelectRSTreeItemData(pRootCmd, GetEditorFrame(TRUE)->m_pToolTreeView->m_TreeCtrl.m_htEvents, FALSE);
	}

	DoEvent(TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditViewDebug::StepOver()
{
	if (m_Context.m_eType != CRSEditViewParameters::EM_DEBUG_ACTIONS)
	{
		GetEditorFrame(TRUE)->OnClose();
		return;
	}
	ASSERT_VALID(m_Context.m_pCurrActionObj);
	if(!m_Context.m_pCurrActionObj)
		return;

	ActionObj* pNextCmd = m_Context.m_pCurrActionObj->GetNextAction();
	if (pNextCmd && !pNextCmd->HasBreakpoint())
	{
		pNextCmd->AddBreakpoint(TRUE);
	}

	GetEditorFrame(TRUE)->OnClose();
}

/////////////////////////////////////////////////////////////////////////////
// CRSEditorDockPane

IMPLEMENT_DYNCREATE(CRSEditorDockPane, CRSDockPane);
//-----------------------------------------------------------------------------


/////////////////////////////////////////////////////////////////////////////
// CRSEditViewDocked

IMPLEMENT_DYNCREATE(CRSEditViewDocked, CRSEditView);

BEGIN_MESSAGE_MAP(CRSEditViewDocked, CRSEditView)

	ON_WM_CREATE()
	
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRSEditViewDocked::CRSEditViewDocked() 
	: CRSEditView()
{	
}

CRSEditViewDocked::~CRSEditViewDocked()
{
	GetDocument()->GetWoormFrame()->m_pEditorDockedView = NULL;
}

//-----------------------------------------------------------------------------
int CRSEditViewDocked::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (__super::OnCreate(lpCreateStruct) == -1)
		return -1;

	return 0;
}

BOOL CRSEditViewDocked::PreTranslateMessage(MSG* pMsg)
{
	if (pMsg->message == WM_LBUTTONDBLCLK)
	{
		CRSReportTreeView* engine = GetDocument()->GetWoormFrame()->GetEngineTreeView();
	
		if (engine)
			engine->OnOpenEditor();
		return TRUE;

	}

	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void CRSEditViewDocked::OnInitialUpdate()
{
	__super::OnInitialUpdate(); 

	GetDocument()->GetWoormFrame()->m_pEditorDockedView = this;

	ModifyStyle(WS_BORDER, WS_CLIPCHILDREN);

	SetLanguage(L"WRM", FALSE);

	GetEditCtrl()->SetReadOnly();
	GetEditCtrl()->HideCaret();
	GetEditCtrl()->EnableLineNumbersMargin(FALSE);
	GetEditCtrl()->EnableIntelliSense(FALSE);
	GetEditCtrl()->m_clrBack = RGB(250, 250, 250);
}

// -----------------------------------------------------------------------------
void CRSEditViewDocked::ShowErrorText(CString /*sError*/)
{
	//TODO
}


/////////////////////////////////////////////////////////////////////////////
// CRSResizableErrorsEditor

IMPLEMENT_DYNCREATE(CRSResizableErrorsEdit, CResizableStrEdit)

BEGIN_MESSAGE_MAP(CRSResizableErrorsEdit, CResizableStrEdit)
	//{{AFX_MSG_MAP(CResizableStrEdit)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

LRESULT	CRSResizableErrorsEdit::OnRecalcCtrlSize(WPARAM, LPARAM)
{

	/*SetAutoSizeCtrl(0);

	CRect r;
	CWnd* parent = GetParent();
	parent->GetWindowRect(r);

	SetWindowPos(NULL, 2, 2, r.Width() - 20, r.Height() - 2, 0);

	SetAutoSizeCtrl(2);
	UpdateWindow();
	*/

	DoRecalcCtrlSize();


	return 0L;
}

BOOL CRSResizableErrorsEdit::PreTranslateMessage(MSG* pMsg)
{
	BOOL ctrlPressed = GetKeyState(VK_CONTROL) & 0x8000;

	if (ctrlPressed && (pMsg->message == WM_KEYDOWN)) {

		if (pMsg->wParam == 'C')
		{
			Copy();
			return TRUE;
		}
	}
	/*if (pMsg->message == WM_MOUSEWHEEL)
	{
		short direction=GET_WHEEL_DELTA_WPARAM(pMsg->wParam);

		if (direction<0)
			ScrollWindow(0, -10);
		else 
			ScrollWindow(0, 10);

		UpdateWindow();
		
	
	}*/
	return __super::PreTranslateMessage(pMsg);
}


//=============================================================================
//			Class CTBGridControlObj
//=============================================================================
IMPLEMENT_DYNAMIC(CTBGridControlResizable, CTBGridControlObj)

BEGIN_MESSAGE_MAP(CTBGridControlResizable, CTBGridControlObj)
	//{{AFX_MSG_MAP(CResizableStrEdit)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
LRESULT	CTBGridControlResizable::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	//DoRecalcCtrlSize();
	SetAutoSizeCtrl(0);

	CRect r;
	CWnd* parent = GetParent();
	parent->GetWindowRect(r);

	SetWindowPos(NULL, 2,2, r.Width() - 20, r.Height() - 15, 0);

	SetAutoSizeCtrl(2);
	AdjustLayout();
	return 0L;
}

//=============================================================================
//			Class CRSEditViewTreeCtrl
//=============================================================================
IMPLEMENT_DYNAMIC(CRSEditViewTreeCtrl, CRSTreeCtrl)

//-----------------------------------------------------------------------------
void CRSEditViewTreeCtrl::OnBeginDrag(NMHDR* pNMHDR, LRESULT* pResult)
{
	NM_TREEVIEW* pNMTreeView = (NM_TREEVIEW*)pNMHDR;
	m_hDragItem = pNMTreeView->itemNew.hItem;

	if (m_hDragItem == NULL)
		return;

	CNodeTree* pNode = GetNode(m_hDragItem);
	if (!pNode)
	{
		m_hDragItem = NULL;
		return;
	}

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	DragText(GetNodeString(pNode) , DROPEFFECT_MOVE);

	*pResult = 0;
}


CString CRSEditViewTreeCtrl::GetNodeString(CNodeTree* pNode)
{
	CString str = L"";

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		SymField* pField = dynamic_cast<SymField*>(pNode->m_pItemData);
		ASSERT_VALID(pField);
		if (!pField)
			break;

		str = pField->GetName();
		break;
	}
	case CNodeTree::ENodeType::NT_PROCEDURE:
	{
		ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
		ASSERT_VALID(pProc);
		if (!pProc)
			break;

		str = L"CALL " + pProc->GetName();
		break;
	}
	case CNodeTree::ENodeType::NT_NAMED_QUERY:
	{
		QueryObjItem* pQry = dynamic_cast<QueryObjItem*>(pNode->m_pItemData);
		ASSERT_VALID(pQry);
		if (!pQry)
			break;

		str = L"Woorm.QueryRead(\"" + pQry->GetName() + L"\")";;
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_ENUM_VALUE:
	{
		EnumTag* pTag = dynamic_cast<EnumTag*>(pNode->m_pParentItemData);
		ASSERT_VALID(pTag);
		EnumItem* pItem = dynamic_cast<EnumItem*>(pNode->m_pItemData);
		ASSERT_VALID(pItem);

		if (!pTag || !pItem)
			break;

		DataEnum de(pTag->GetTagValue(), pItem->GetItemValue());

		//CString sTooltip = de.FormatData();

		str = de.ToString() + L" /*" + pTag->GetTagName() + L" : " + pItem->GetItemName() + L"*/";
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_WEBMETHOD:
	{
		CFunctionDescription* pFun = dynamic_cast<CFunctionDescription*>(pNode->m_pItemData);
		ASSERT_VALID(pFun);
		if (!pFun)
			break;

		/*TODO*/CString sSignature = pFun->GetHelpSignature();

		str = pFun->GetNamespace().ToUnparsedString() + L" ( )";
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_FUNCTION:
	{
		CDecoratedFunctionDescription* pFun = dynamic_cast<CDecoratedFunctionDescription*>(pNode->m_pItemData);
		ASSERT_VALID(pFun);
		if (!pFun)
			break;

		str = cwsprintf(pFun->GetName()) + L" ( )";
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DBTABLE:
	case CNodeTree::ENodeType::NT_LIST_DBVIEW:
	{
		const SqlCatalogEntry* pCatEntry = dynamic_cast<const SqlCatalogEntry*>(pNode->m_pItemData);

		str = pCatEntry->m_strTableName;
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
	{
		SqlColumnInfo* pCol = (SqlColumnInfo*)(pNode->m_pItemData);
		ASSERT_VALID(pCol);
		if (!pCol)
			break;
		SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pParentItemData);
		ASSERT_VALID(pCatEntry);
		if (!pCatEntry)
			break;

		str = pCatEntry->m_strTableName + '.' + pCol->GetColumnName();

		break;
	}
	case CNodeTree::ENodeType::NT_LIST_SPECIAL_TEXT:
	{
		CSpecialField* pField = dynamic_cast<CSpecialField*>(pNode->m_pItemData);
		ASSERT_VALID(pField);
		if (!pField)
			break;

		str = SPECIAL_FIELD_SEP_START + pField->GetKeyword() + SPECIAL_FIELD_SEP_END;
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_COMMAND:
	{
		str = this->GetItemText(pNode->m_ht);
		break;
	}

	case CNodeTree::ENodeType::NT_LIST_HTML_TAGS:
	{
		CHtmlTag* pTag = dynamic_cast<CHtmlTag*>(pNode->m_pItemData);
		ASSERT_VALID(pTag);
		if (!pTag)
			break;

		str = pTag->GetHtmlFragment();
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_QUERY_TAGS:
	{
		CQueryTag* pTag = dynamic_cast<CQueryTag*>(pNode->m_pItemData);
		ASSERT_VALID(pTag);
		if (!pTag)
			break;

		str = pTag->GetFragment();
		break;
	}
	}
	return str;
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEditorPreviewView, CAbstractFormView)

//-----------------------------------------------------------------------------

CRSEditorPreviewView::CRSEditorPreviewView()
	:
	CAbstractFormView(_T("CRSEditorPreviewView"), IDD_RS_EditorPreviewView)
{
}

CRSEditorPreviewView::~CRSEditorPreviewView()
{
	if (GetFrame())
		GetFrame()->m_pPreviewView = NULL;
}

// ---------------------------------------------------------------------------- -
CWoormDocMng* CRSEditorPreviewView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// ---------------------------------------------------------------------------- -
CRSEditorFrame* CRSEditorPreviewView::GetFrame()
{
	return dynamic_cast<CRSEditorFrame*>(__super::GetParentFrame());
}


// ---------------------------------------------------------------------------- -
void CRSEditorPreviewView::BuildDataControlLinks()
{
	GetFrame()->m_pPreviewView = this;
}

BOOL CRSEditorPreviewView::PreTranslateMessage(MSG* pMsg)
{

	if (pMsg->message == WM_MOUSEWHEEL)
	{
		short direction = GET_WHEEL_DELTA_WPARAM(pMsg->wParam);
		if (direction < 0)
			OnScrollDown();
		else
			OnScrollUp();

	}

	return __super::PreTranslateMessage(pMsg);
}

// ---------------------------------------------------------------------------- -
void CRSEditorPreviewView::OnInitialUpdate()
{
	__super::OnInitialUpdate();

	ShowScrollBar(SB_BOTH, FALSE);
}

//------------------------------------------------------------------------------
void CRSEditorPreviewView::OnPreview()
{
	GetFrame()->m_pEditView->m_pEditTextRect->Invalidate();
	RedrawWindow();
}

//------------------------------------------------------------------------------
void CRSEditorPreviewView::OnDraw(CDC* pDC)
{
	CRect rect;
	CWnd::GetClientRect(&rect);
	//CWnd::GetWindowRect(&rect);
	GetFrame()->m_pEditView->GetClientRect(&rect);
	
	if(GetFrame()->m_pEditView && GetFrame()->m_pEditView->m_pEditTextRect)
	{
		GetFrame()->m_pEditView->m_pEditTextRect->SetText(GetFrame()->m_pEditView->GetText());
		GetFrame()->m_pEditView->m_pEditTextRect->DrawHtmlPreview(*pDC, rect);
	}
}
