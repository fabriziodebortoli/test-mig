#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric/SettingsTable.h>
#include <TbGeneric/ParametersSections.h>
#include <TbGeneric/TBThemeManager.h>

#include <TbGenlib/TBPropertyGrid.h>
#include <TbGenlib/BaseApp.h>

#include <TbGenlibUI/SettingsTableManager.h>

#include <TbGenlibManaged\HelpManager.h>

#include <TbParser/XmlFunctionObjectsParser.h>

#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/QueryObject.h>
#include <TbWoormEngine/rpsymtbl.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/events.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/repdata.h>
#include <TbWoormEngine/qrydata.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/edtmng.h>
#include <TbWoormEngine/edtcmm.h>
#include <TbWoormEngine/inputmng.h>

#include "ListDlg.h"
#include "WoormFrm.h"
#include "WoormVw.h"
#include "Docproperties.h"
#include "Column.h"
#include "Table.h"
#include "Repeater.h"
#include "mulselob.h"
#include "PredefinedWoormFunctions.h"

#include "RSEditorUI.h"
#include "RSEditView.h"
#include "CustomEditCtrl.h"

#include "woormdoc.hjson" 
#include "RSEditorUI.hjson" 

#include "rectobj.hrc"	//per il test della toolbox
#include "Chart.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#define RS_DOCK_NS	L"Framework.TbWoormViewer.TbWoormViewer.RSDockPanel.Button"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
///////////////////////////////////////////////////////////////////////////////

#define TREE_SCROLL_HEIGHT_AREA	10

IMPLEMENT_DYNAMIC(CNodeTree, CObject)

CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, ENodeType eNodeType,
	CObject* pItemData/*= NULL*/, CObject* pParentItemData/*= NULL*/, CObject* pAncestorItemData /*= NULL*/)
	:
	IDisposingSourceImpl(this),

	m_ht(ht),
	m_eImgIndex(eImgIndex),
	m_NodeType(eNodeType),

	m_pItemData(pItemData),
	m_pParentItemData(pParentItemData),
	m_pAncestorItemData(pAncestorItemData),

	m_pItemFont(AfxGetThemeManager()->GetControlFont())
{}

CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode)
	:
	IDisposingSourceImpl(this),

	m_ht(ht),
	m_eImgIndex(eImgIndex),
	m_NodeType(CNodeTree::ENodeType::NT_EXPR),

	m_pSymTable(pSymTable),

	m_ppExpr(ppExpr),
	m_ReturnType(dtReturnType),
	m_bViewMode(bViewMode),

	m_pItemFont(AfxGetThemeManager()->GetControlFont())
{}

CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents)
	:
	IDisposingSourceImpl(this),

	m_ht(ht),
	m_eImgIndex(eImgIndex),
	m_NodeType(CNodeTree::ENodeType::NT_BLOCK),

	m_pSymTable(pSymTable),

	m_ppBlock(ppBlock),
	m_bRaiseEvents(bRaiseEvents),

	m_pItemFont(AfxGetThemeManager()->GetControlFont())
{}

//-----------------------------------------------------------------------------
void CNodeTree::Empty()
{
	m_NodeType = CNodeTree::ENodeType::NT_WRONG;
	m_bCustomPaint = FALSE;

	m_pItemData = NULL;
	m_pParentItemData = NULL;

	m_pSymTable = NULL;
	m_ppExpr = NULL;
	m_ppBlock = NULL;
	m_pItemFont = NULL;
}

//////////////////////////////////////////////////////////////////////////////
INT_PTR CNodeTreeArray::AddUnique(CNodeTree* newNode)
{
	for (int i = GetUpperBound(); i >= 0; i--)
		if (this->GetAt(i) == newNode)
			return i;

	return Add(newNode);
}

INT_PTR CNodeTreeArray::Find(CNodeTree* pNode)
{
	for (int i = GetUpperBound(); i >= 0; i--)
		if (this->GetAt(i) == pNode)
			return i;

	return -1;
}

/*
BOOL CNodeTreeArray::CheckOneSel(CNodeTree* pNode)
{
	int idx = Find(pNode);
	if (idx >= 0 && this->m_bChecked)
	{
		return FALSE;
	}
	else if (idx >= 0)
	{
		this->m_bChecked = TRUE;
		return TRUE;
	}
	return FALSE;
}
*/
///////////////////////////////////////////////////////////////////////////////
BOOL CWoormFrame::ShowNewEditor()
{
	return TRUE;
}

void CWoormFrame::SetReportTreeView(CRSReportTreeView* pView) { m_pEngineTreeView = pView; m_pLayoutTreeView = pView; }

void CWoormFrame::SetEngineTreeView(CRSReportTreeView* pView) { m_pEngineTreeView = pView; }
void CWoormFrame::SetLayoutTreeView(CRSReportTreeView* pView) { m_pLayoutTreeView = pView; }

//-----------------------------------------------------------------------------

BOOL CWoormFrame::CreateAuxObjects(CCreateContext* pCreateContext)
{
	if (!ShowNewEditor())
		return TRUE;

	m_BornContext = *pCreateContext;
	return TRUE;
}

// -----------------------------------------------------------------------------
CRSEditView* CWoormFrame::CreateEditView()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument())
		return NULL;

	CRSEditView* pEdtView = dynamic_cast<CRSEditView*>(GetDocument()->CreateSlaveView(RUNTIME_CLASS(CRSEditView)));
	ASSERT_VALID(pEdtView);

	m_pEditView = pEdtView;
	m_pEditView->GetEditCtrl()->SelectLine(1);
	return pEdtView;
}

// -----------------------------------------------------------------------------
CRSEditViewDebug* CWoormFrame::CreateEditViewDebug()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument())
		return NULL;

	CRSEditViewDebug* pEdtViewDebug = dynamic_cast<CRSEditViewDebug*>(GetDocument()->CreateSlaveView(RUNTIME_CLASS(CRSEditViewDebug)));
	ASSERT_VALID(pEdtViewDebug);

	pEdtViewDebug->SetLanguage(L"WRM", FALSE);

	m_pEditView = pEdtViewDebug;

	return pEdtViewDebug;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::ShowPanelToolbarText()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::CreatePropertyPanel()
{
	if (!m_pPropertyPane)
	{
		CCreateContext* pCreateContext = &m_BornContext;

		m_pPropertyPane = (CRSObjectPropertyDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSObjectPropertyDockPane), IDC_RS_PropertyDockPane, _T("Property"), _T("Property"), CBRS_ALIGN_RIGHT | CBRS_FLOATING | CBRS_BORDER_BOTTOM, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
		m_pPropertyPane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());
	}
}

void CWoormFrame::ShowPropertyPanel(BOOL bShow)
{
	if (m_pPropertyPane)
	{
		m_pPropertyPane->ShowPanel(bShow, FALSE, CBRS_ALIGN_RIGHT | CBRS_BORDER_BOTTOM);
		m_pPropertyPane->DockToFrameWindow(CBRS_RIGHT, NULL, CBRS_BORDER_BOTTOM);
		m_pPropertyPane->SetBCGStyle(m_pPropertyPane->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	}
}

void CWoormFrame::CreateAndShowPropertyPanel()
{
	if (!m_pPropertyPane)
	{
		CreatePropertyPanel();
		ShowPropertyPanel(TRUE);
	}
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateDockingPanel()
{
	if (!GetDocument()->CanDoEditReport())
		return TRUE;

	CCreateContext* pCreateContext = &m_BornContext;

	// Non muovere queste line in questo ordine

	CreatePropertyPanel();

	CWoormDocMng* pDoc = dynamic_cast<CWoormDocMng*>(pCreateContext->m_pCurrentDoc);
	ASSERT_VALID(pDoc);

	m_pEditorPane = (CRSEditorDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSEditorDockPane), IDC_RS_EditorDockPane, _T("ScriptPreview"), _T("Script Preview"), CBRS_ALIGN_BOTTOM | CBRS_HIDE_INPLACE, CSize(700, 200), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);

	if (!pDoc || !pDoc->m_pWoormIni || !pDoc->m_pWoormIni->m_bShowReportTree)
	{
		m_pEnginePane = (CRSEngineDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSEngineDockPane), IDC_RS_EngineDockPane, _T("Engine"), _T("Engine"), CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
		m_pEnginePane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());

		m_pLayoutPane = (CRSLayoutDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSLayoutDockPane), IDC_RS_LayoutDockPane, _T("Layout"), _T("Layout"), CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
		m_pLayoutPane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());
	}
	else
	{
		m_pFullReportPane = (CRSFullReportDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSFullReportDockPane), IDC_RS_ReportDockPane, _T("Report"), _T("Report"), CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
		m_pFullReportPane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());
	}

	m_pToolBoxDBPane = (CRSToolBoxDBDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSToolBoxDBDockPane), IDC_RS_ToolBoxDBDockPane, _T("ToolBoxDB"), _T("Database"), CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
	m_pToolBoxDBPane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());

	m_pToolBoxPane = (CRSToolBoxDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSToolBoxDockPane), IDC_RS_ToolBoxDockPane, _T("ToolBox"), _T("ToolBox - Objects"), CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);
	m_pToolBoxPane->EnableToolbar(TOOLBAR_HEIGHT, ShowPanelToolbarText());

	m_pToolBarPane = (CRSToolBarDockPane*)CreateDockingPane(RUNTIME_CLASS(CRSToolBarDockPane), IDC_RS_ToolBarDockPane, _T("ToolBarBox"), _T("ToolBox - Actions"), CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE, CSize(300, 700), pCreateContext, dwDefaultBCGDockingBarStyle & ~(CBRS_BCGP_CLOSE), FALSE);

	//----
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::ShowDockingPanels(BOOL bShow)
{
	if (!GetDocument()->CanDoEditReport())
		return;

	if (bShow)
	{
		if (!m_pToolBoxPane)	//TODO test su uno dei tanti
		{
			VERIFY(CreateDockingPanel());
		}
	}

	//-------------
	if (m_pToolBoxPane)
	{
		m_pToolBoxPane->ShowPanel(bShow, TRUE, CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE);
		m_pToolBarPane->SetAutoHideMode(TRUE, CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE);
		m_pToolBoxPane->SetBCGStyle(m_pToolBoxPane->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	}

	if (m_pToolBarPane)
	{
		m_pToolBarPane->ShowPanel(bShow, FALSE, CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE);
		m_pToolBarPane->SetAutoHideMode(TRUE, CBRS_ALIGN_LEFT | CBRS_HIDE_INPLACE);
		m_pToolBarPane->SetBCGStyle(m_pToolBarPane->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	}

	if (m_pToolBoxDBPane)
	{
		m_pToolBoxDBPane->ShowPanel(bShow, FALSE, CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE);
		m_pToolBoxDBPane->SetAutoHideMode(TRUE, CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE);
		m_pToolBoxDBPane->SetBCGStyle(m_pToolBoxDBPane->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
	}

	ShowPropertyPanel(bShow);

	//----

	if (bShow)
	{
		if (m_pToolBoxTreeView)
		{
			ASSERT_VALID(m_pToolBoxTreeView);
			m_pToolBoxTreeView->FillTree();
		}
		if (m_pToolBoxDBView)
		{
			ASSERT_VALID(m_pToolBoxDBView);
			m_pToolBoxDBView->FillTree();
		}
	}

	//----------------------
	if (m_pFullReportPane)
	{
		m_pFullReportPane->ShowPanel(bShow, FALSE, CBRS_LEFT | CBRS_HIDE_INPLACE);
		if (bShow)
		{
			m_pFullReportPane->DockToFrameWindow(CBRS_LEFT | CBRS_HIDE_INPLACE);

			if (m_pLayoutTreeView)
			{
				ASSERT_VALID(m_pLayoutTreeView);
				ASSERT(m_pLayoutTreeView == m_pEngineTreeView);
				m_pLayoutTreeView->CRSReportTreeView::FillTree();
			}
		}
	}
	if (m_pEnginePane)
	{
		m_pEnginePane->ShowPanel(bShow, FALSE, CBRS_LEFT | CBRS_HIDE_INPLACE);

		if (bShow)
		{
			if (m_pEngineTreeView)
			{
				ASSERT_VALID(m_pEngineTreeView);
				m_pEngineTreeView->FillTree();
			}
		}
	}
	if (m_pLayoutPane)
	{
		m_pLayoutPane->ShowPanel(bShow, FALSE, CBRS_LEFT | CBRS_HIDE_INPLACE);

		if (bShow)
		{
			if (m_pLayoutTreeView)
			{
				ASSERT_VALID(m_pLayoutTreeView);
				m_pLayoutTreeView->FillTree();
			}
		}
	}

	//----

	if (bShow)
	{
		if (m_pLayoutPane && m_pEnginePane)
		{
			CBCGPDockingControlBar* pLeftTabbedBar = NULL;
			m_pLayoutPane->AttachToTabWnd(m_pEnginePane, BCGP_DM_SHOW, TRUE, &pLeftTabbedBar);
			pLeftTabbedBar->SetBCGStyle(pLeftTabbedBar->GetBCGStyle() & ~(CBRS_BCGP_CLOSE));
			pLeftTabbedBar->UpdateWindow();
		}
	}


	//----
	if (m_pEditorPane)
	{
		m_pEditorPane->ShowPanel(bShow, FALSE, CBRS_BOTTOM | CBRS_HIDE_INPLACE);
		m_pEditorPane->DockToFrameWindow(CBRS_BOTTOM);
	}

	//TODO SaveDockingLayout();
}

//-----------------------------------------------------------------------------
void CWoormFrame::ClosePanels()
{
	if (m_pEditView)
	{
		if (m_pEditView->GetEditCtrl())
			m_pEditView->GetEditCtrl()->SetModified(FALSE);
		m_pEditView->GetEditorFrame(TRUE)->SendMessage(WM_CLOSE, 0, 0);
	}

	if (m_pEnginePane)
		m_pEnginePane->PrepareForClose();
	if (m_pLayoutPane)
		m_pLayoutPane->PrepareForClose();
	if (m_pFullReportPane)
		m_pFullReportPane->PrepareForClose();

	if (m_pToolBoxPane)
		m_pToolBoxPane->PrepareForClose();

	if (m_pToolBarPane)
		m_pToolBarPane->PrepareForClose();

	if (m_pPropertyPane)
	{
		if (m_pObjectPropertyView)
			m_pObjectPropertyView->GetPropertyGrid()->RemoveAll();

		m_pPropertyPane->PrepareForClose();
	}
	if (m_pToolBoxDBPane)
		m_pToolBoxDBPane->PrepareForClose();

	if (m_pEditorPane)
		m_pEditorPane->PrepareForClose();

	m_DockingPanes.RemoveAll();
	/*
	try
	{
		ShowDockingPanels(FALSE);
		m_DockingPanes.DestroyPanes();
	} catch(...)
	{}
	*/
}

//----------------------------------------------------------------------------
BOOL CWoormFrame::DestroyWindow()
{
	//ClosePanels();

	this->SuspendLayout();

	//for (int i = m_DockingPanes.GetUpperBound(); i >= 0; i--)
	//{
	//	try
	//	{
	//		CTaskBuilderDockPane* pPane = m_DockingPanes.GetAt(i);
	//		ASSERT_VALID(pPane);

	//		SAFE_DELETE(pPane);
	//	}
	//	catch (...)
	//	{
	//		ASSERT(FALSE);
	//	}
	//}
	m_DockingPanes.RemoveAll();

	return __super::DestroyWindow();
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::SelectLayoutObject(CObject* pObj, BOOL bPassive/* = TRUE*/)
{
	if (GetEngineTreeView() && GetEngineTreeView()->m_TreeCtrl)
		GetEngineTreeView()->m_TreeCtrl.ClearSelection();

	if (GetLayoutTreeView())
		GetLayoutTreeView()->SelectLayoutObject(pObj, bPassive); //TODO LAYOUT : aggiungere layout corrente come parametro

	if (m_pLayoutPane)
		m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnDeactivateFrame()
{
	if (m_pEnginePane)
		m_pEnginePane->OnDeactivateFrame(CBRS_LEFT);

	if (m_pLayoutPane)
		m_pLayoutPane->OnDeactivateFrame(CBRS_LEFT);
	if (m_pFullReportPane)
		m_pFullReportPane->OnDeactivateFrame(CBRS_LEFT);

	if (m_pToolBoxPane)
		m_pToolBoxPane->OnDeactivateFrame(CBRS_LEFT);

	if (m_pToolBarPane)
		m_pToolBarPane->OnDeactivateFrame(CBRS_LEFT);

	if (m_pPropertyPane)
		m_pPropertyPane->OnDeactivateFrame(CBRS_RIGHT);

	if (m_pToolBoxDBPane)
		m_pToolBoxDBPane->OnDeactivateFrame(CBRS_RIGHT);

	if (m_pEditorPane)
		m_pEditorPane->OnDeactivateFrame(CBRS_BOTTOM);
}

//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnActivateTab(WPARAM wParam, LPARAM lParam)
{
	if (!wParam)	//la finestra passa in secondo piano
	{
		OnDeactivateFrame();
	}

	return __super::OnActivateTab(wParam, lParam);
}

///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CRSTreeCtrl, CTBTreeCtrl)

BEGIN_MESSAGE_MAP(CRSTreeCtrl, CTBTreeCtrl)

	ON_WM_PAINT()

	//ON_WM_CONTEXTMENU()
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	ON_WM_RBUTTONUP()
	ON_WM_MOUSEMOVE()

	ON_NOTIFY_REFLECT(TVN_BEGINDRAG, OnBeginDrag)
	ON_NOTIFY_REFLECT(TVN_ITEMEXPANDING, OnItemExpanding)
	ON_NOTIFY_REFLECT(TVN_DELETEITEM, OnItemDeleted)

END_MESSAGE_MAP()

CRSTreeCtrl::CRSTreeCtrl()
	:
	m_pWDoc(NULL),
	m_bPassive(FALSE),

	m_pBold(NULL),
	m_pItalic(NULL),
	m_pBoldItalic(NULL),

	m_htWebMethods(NULL),
	m_htFunctions(NULL),
	m_htEnums(NULL),
	m_htSpecialText(NULL),

	m_bShowAllTables(FALSE),
	m_htTables(NULL),

	m_htRuleTables(NULL),

	m_htLayouts(NULL),
	m_htLayoutDefault(NULL),

	m_htLinks(NULL),

	m_htVariables(NULL),
	m_htHiddenGroupVariables(NULL),

	m_htRules(NULL),
	m_htTupleRules(NULL),

	m_htEvents(NULL),
	m_htReportEvents(NULL),
	m_htReportAlwaysEvent(NULL),
	m_htReportBeforeEvent(NULL),
	m_htReportAfterEvent(NULL),
	m_htReportFinalizeEvent(NULL),
	m_htFormFeedEvents(NULL),
	m_htFormFeedBeforeEvent(NULL),
	m_htFormFeedAfterEvent(NULL),
	m_htFillTableEvents(NULL),
	m_htTriggerEvents(NULL),

	m_htProcedures(NULL),
	m_htQueries(NULL),
	m_htAskDialogs(NULL),

	/*m_htPageInfo(NULL),
	m_htProperties(NULL),
	m_htSettings(NULL),*/

	m_htFontTable(NULL),
	m_htFormatterTable(NULL),

	m_htToolBox(NULL),

	m_htUndoActions(NULL)
{
	CFont* pControlFont = AfxGetThemeManager()->GetControlFont();
	LOGFONT lf;
	pControlFont->GetLogFont(&lf);

	m_pBold = new CFont(); m_arGarbage.Add(m_pBold);
	lf.lfWeight = FW_BOLD;
	m_pBold->CreateFontIndirect(&lf);

	m_pBoldItalic = new CFont(); m_arGarbage.Add(m_pBoldItalic);
	lf.lfItalic = (BYTE)TRUE;
	m_pBoldItalic->CreateFontIndirect(&lf);

	pControlFont->GetLogFont(&lf);
	m_pItalic = new CFont(); m_arGarbage.Add(m_pItalic);
	//lf.lfWeight = FW_NORMAL;
	lf.lfItalic = (BYTE)TRUE;
	m_pItalic->CreateFontIndirect(&lf);

	m_bMultiSelectCustom = FALSE;

	m_bMultiSelect = TRUE;

	InitializeImageList();

	m_strDragCommand = _T("");
}

//-----------------------------------------------------------------------------
//Funzioni per problema icone con sfondo nero
// aggiunto secondo parameto alla FromFile
Gdiplus::Bitmap* _LoadPNG(CString sImgName, bool forceLoad /*=false*/)
{
	if (AfxIsRemoteInterface() && !forceLoad)
		return NULL;

	return Gdiplus::Bitmap::FromFile(sImgName, TRUE);
}


HICON _TBLoadPng(CString strImageNS)
{
	return TBLoadImage(strImageNS, NULL, 20, RGB(255, 255, 255));
	/*
		if (AfxIsRemoteInterface())
		{
			return NULL;
		}

		HICON hIco = NULL;

		CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(CTBNamespace(strImageNS), AfxGetLoginInfos()->m_strUserName);
		CString	sFileExtension = sImagePath.Right(4);
		if (sImagePath.Right(4).CompareNoCase(_T(".PNG")) != 0)
		{
			ASSERT(FALSE);
			return hIco;
		}

		Gdiplus::Bitmap* gdibitmap = _LoadPNG(sImagePath, TRUE);
		ASSERT(gdibitmap);
		if (!gdibitmap) return hIco;

		gdibitmap->GetHICON(&hIco);
		delete gdibitmap;
		return hIco;
	*/
}
//-----------------------------------------------------------------------------

void CRSTreeCtrl::InitializeImageList()
{
	if (!::IsWindow(m_hWnd))
		return;

	HICON	hIcon[CRSTreeCtrlImgIdx::MAXGlyph];

	m_ImageList.DeleteImageList();

	CAbstractFormView* pParent = dynamic_cast<CAbstractFormView*>(GetParent());
	if (pParent) pParent->SetBackgroundColor(RGB(255, 255, 255));

	SetBkColor(RGB(255, 255, 255));
	m_ImageList.Create(ScalePix(20), ScalePix(20), ILC_COLOR32, ILC_MASK | ILC_COLOR, CRSTreeCtrlImgIdx::MAXGlyph);
	m_ImageList.SetBkColor(RGB(255, 255, 255));

	//hIcon[CRSTreeCtrlImgIdx::ColumnGlyph]			= TBLoadImage(TBGlyph(szGlyphColumn), NULL , 20, RGB(255, 255, 255));

	hIcon[CRSTreeCtrlImgIdx::ColumnGlyph] = TBLoadPng(TBGlyph(szGlyphColumn));
	hIcon[CRSTreeCtrlImgIdx::ColumnHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphColumnHidden));
	hIcon[CRSTreeCtrlImgIdx::ColumnHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphColumnExpr));
	hIcon[CRSTreeCtrlImgIdx::ColumnTotalGlyph] = TBLoadPng(TBGlyph(szGlyphColumnTotal));
	hIcon[CRSTreeCtrlImgIdx::DataGlyph] = TBLoadPng(TBGlyph(szGlyphData2));
	hIcon[CRSTreeCtrlImgIdx::DataPrimaryKeyGlyph] = TBLoadPng(TBGlyph(szGlyphDataPrimaryKey));
	hIcon[CRSTreeCtrlImgIdx::ImageGlyph] = TBLoadPng(TBGlyph(szGlyphImage));
	hIcon[CRSTreeCtrlImgIdx::ImageHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphImageHidden));
	hIcon[CRSTreeCtrlImgIdx::ImageHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphImageExpr));
	hIcon[CRSTreeCtrlImgIdx::RectangleGlyph] = TBLoadPng(TBGlyph(szGlyphRectangle));
	hIcon[CRSTreeCtrlImgIdx::RectangleHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphRectangleHidden));
	hIcon[CRSTreeCtrlImgIdx::RectangleHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphRectangleExpr));
	hIcon[CRSTreeCtrlImgIdx::RepeaterGlyph] = TBLoadPng(TBGlyph(szGlyphRepeater));
	hIcon[CRSTreeCtrlImgIdx::RepeaterHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphRepeaterHidden));
	hIcon[CRSTreeCtrlImgIdx::RepeaterHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphRepeaterExpr));

	hIcon[CRSTreeCtrlImgIdx::ChartGlyph] = TBLoadPng(TBGlyph(szGlyphChart));
	hIcon[CRSTreeCtrlImgIdx::ChartHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphChartHidden));
	hIcon[CRSTreeCtrlImgIdx::ChartHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphChartExpr));

	hIcon[CRSTreeCtrlImgIdx::ChartCategoryGlyph] = TBLoadPng(TBGlyph(szGlyphChartCategory));
	//hIcon[CRSTreeCtrlImgIdx::ChartCategoryHiddenGlyph]	= TBLoadPng(TBGlyph(szGlyphChartCategoryHidden));
	//hIcon[CRSTreeCtrlImgIdx::ChartCategoryHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphChartCategoryExpr));

	hIcon[CRSTreeCtrlImgIdx::ChartSeriesGlyph] = TBLoadPng(TBGlyph(szGlyphChartSeries));
	hIcon[CRSTreeCtrlImgIdx::ChartSeriesHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphChartSeriesHidden));
	hIcon[CRSTreeCtrlImgIdx::ChartSeriesHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphChartSeriesExpr));

	hIcon[CRSTreeCtrlImgIdx::TableGlyph] = TBLoadPng(TBGlyph(szGlyphTable));
	hIcon[CRSTreeCtrlImgIdx::TableHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphTableHidden));
	hIcon[CRSTreeCtrlImgIdx::TableHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphTableExpr));
	hIcon[CRSTreeCtrlImgIdx::TextGlyph] = TBLoadPng(TBGlyph(szGlyphText));
	hIcon[CRSTreeCtrlImgIdx::TextHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphTextHidden));
	hIcon[CRSTreeCtrlImgIdx::TextHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphTextExpr));
	hIcon[CRSTreeCtrlImgIdx::TextFileGlyph] = TBLoadPng(TBGlyph(szGlyphTextFile));
	hIcon[CRSTreeCtrlImgIdx::TextFileHiddenGlyph] = TBLoadPng(TBGlyph(szGlyphTextFileHidden));
	hIcon[CRSTreeCtrlImgIdx::TextFileHiddenExprGlyph] = TBLoadPng(TBGlyph(szGlyphTextFileExpr));
	hIcon[CRSTreeCtrlImgIdx::ExprGlyph] = TBLoadPng(TBGlyph(szGlyphExpression));
	hIcon[CRSTreeCtrlImgIdx::FuncGlyph] = TBLoadPng(TBGlyph(szGlyphFunction));
	hIcon[CRSTreeCtrlImgIdx::FieldGlyph] = TBLoadPng(TBGlyph(szGlyphField));
	hIcon[CRSTreeCtrlImgIdx::FieldGlyphHidden] = TBLoadPng(TBGlyph(szGlyphFieldHidden));
	hIcon[CRSTreeCtrlImgIdx::FieldGlyphHiddenExpr] = TBLoadPng(TBGlyph(szGlyphFieldExpr));
	hIcon[CRSTreeCtrlImgIdx::PrimaryKey] = TBLoadPng(TBGlyph(szGlyphPrimaryKey));
	hIcon[CRSTreeCtrlImgIdx::ForeignKey] = TBLoadPng(TBGlyph(szGlyphForeignKey));

	hIcon[CRSTreeCtrlImgIdx::InputVarGlyph] = TBLoadPng(TBGlyph(szGlyphInputVar));
	hIcon[CRSTreeCtrlImgIdx::InputAndAskVarGlyph] = TBLoadPng(TBGlyph(szGlyphInputAndAskVar));
	hIcon[CRSTreeCtrlImgIdx::Total] = TBLoadPng(TBIcon(szIconSigma, CONTROL));

	hIcon[CRSTreeCtrlImgIdx::LinkToRadar] = TBLoadPng(TBIcon(szIconRadar, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::LinkToForm] = TBLoadPng(TBIcon(szIconLinkToForm, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::LinkToReport] = TBLoadPng(TBIcon(szIconLinkToReport, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::LinkToFunction] = TBLoadPng(TBIcon(szIconLinkToFunction, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::LinkToFile] = TBLoadPng(TBIcon(szIconLinkToFile, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::LinkToUrl] = TBLoadPng(TBIcon(szIconLinkToUrl, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::MailTo] = TBLoadPng(TBIcon(szIconMail, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::CallTo] = TBLoadPng(TBIcon(szIconPhone, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::GoogleMap] = TBLoadPng(TBIcon(szIconAddress, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BarcodeGlyph] = TBLoadPng(TBIcon(szIconRSBarcode, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::FuncArrayGlyph] = TBLoadPng(TBGlyph(szGlyphFuncArray));

	hIcon[CRSTreeCtrlImgIdx::BreakPoint] = TBLoadPng(TBIcon(szIconBreakpoint, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BreakPointCondition] = TBLoadPng(TBIcon(szIconBreakpointCondition, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BreakPointAction] = TBLoadPng(TBIcon(szIconBreakpointAction, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BreakPointConditionAction] = TBLoadPng(TBIcon(szIconBreakpointConditionAction, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BreakPointCurrent] = TBLoadPng(TBIcon(szIconBreakpointCurrent, CONTROL));
	hIcon[CRSTreeCtrlImgIdx::BreakPointDisabled] = TBLoadPng(TBIcon(szIconBreakpointDisabled, CONTROL));

	hIcon[CRSTreeCtrlImgIdx::NoGLyph] = TBLoadPng(TBGlyph(szGlyphNoImgTreeCtrl));

	for (int n = 0; n < CRSTreeCtrlImgIdx::MAXGlyph; n++)
	{
		int index = m_ImageList.Add(hIcon[n]);
		ASSERT(index == n);
		if (index >= 0)
			::DestroyIcon(hIcon[n]);
	}

	SetImageList(&m_ImageList, TVSIL_NORMAL);
	m_ImageList.SetBkColor(RGB(255, 255, 255));
}

BOOL CRSTreeCtrl::PreTranslateMessage(MSG* pMsg)
{
	/*SetCapture();
	if(pMsg->message==WM_RBUTTONUP)
		return TRUE;
	if (pMsg->message == WM_CONTEXTMENU)
		return TRUE;
*/
	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::IsLayoutNodeType(CNodeTree::ENodeType nodeType)
{
	return
		nodeType == CNodeTree::ENodeType::NT_OBJ_FIELDRECT ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_TEXTRECT ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_FILERECT ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_SQRRECT ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_GRAPHRECT ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_TABLE ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_COLUMN ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_REPEATER ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_TOTAL ||
		nodeType == CNodeTree::ENodeType::NT_OBJ_CHART /*||
		nodeType == CNodeTree::ENodeType::NT_OBJ_SERIES*/;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::IsHiddenVariable(CNodeTree* pNode)
{
	if (pNode->m_NodeType == CNodeTree::ENodeType::NT_VARIABLE)
	{
		WoormField* pWrmField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (pWrmField && pWrmField->IsHidden())
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnRButtonDown(UINT nFlags, CPoint point)
{
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	//prima di selezionare un elemento dei engine/layout-tree, ripulisco gli altri
	// In particolare si vuole evitare questo comportamento nell'EditView
	if (m_pWDoc && GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CRSReportTreeView)))
	{
		ASSERT_VALID(m_pWDoc);
		//m_pWDoc->ClearMultiSelection();
		m_pWDoc->ClearSelectionFromAllTrees(this);
	}

	try
	{
		__super::OnLButtonDown(nFlags, point);
	}
	catch (...)
	{	//crash su BCGP scroolbar ?
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnMouseMove(UINT nFlags, CPoint point)
{
	//do nothing to prevent flickering
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnRButtonUp(UINT nFlags, CPoint point)
{
	//Togliendolo si attiva il menu di contesto del Docking Panel

	//HTREEITEM hItem = HitTest(point);
	//SelectItem(hItem);
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::Attach(CWoormDocMng* pWDoc)
{
	m_pWDoc = pWDoc;
	ASSERT_VALID(m_pWDoc);
}

// When assingning a LPCSTR to a CStringW or a LPCWSTR to a CStringA,
// ANSI / Unicode conversion is performed.
//-----------------------------------------------------------------------------
void CRSTreeCtrl::EnableDrag(LPCTSTR lpszText)
{
	m_strDragCommand = lpszText;
}

//-----------------------------------------------------------------------------
DROPEFFECT CRSTreeCtrl::DragText(LPCTSTR lpszText, DROPEFFECT dwEffect)
{
	// Allocate global memory and copy text
	size_t nSize = (_tcslen(lpszText) + 1) * sizeof(TCHAR);
	HGLOBAL hGlobal = ::GlobalAlloc(GMEM_MOVEABLE, static_cast<SIZE_T>(nSize));
	LPTSTR lpDst = static_cast<LPTSTR>(::GlobalLock(hGlobal));
	::CopyMemory(lpDst, lpszText, nSize);
	::GlobalUnlock(hGlobal);
	// With DragDrop, we may allocate COleDataSource on the stack.
	// When using SetClipboard(), it must be allocated on the heap.
	COleDataSource Data;
#ifdef _UNICODE
	Data.CacheGlobalData(CF_UNICODETEXT, hGlobal);
#else
	Data.CacheGlobalData(CF_TEXT, hGlobal);
#endif
	return Data.DoDragDrop(); //rimosso il dropeffect precedentemente passato come parametro perchè era limitante
}

// Begin drag
//-----------------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::GetDragItem()
{
	return m_hDragItem;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnBeginDrag(NMHDR* pNMHDR, LRESULT* pResult)
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

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pF);
		if (!pF->IsHidden())
		{
			m_hDragItem = NULL;
			return;
		}
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DBTABLE:
	case CNodeTree::ENodeType::NT_LIST_DBVIEW:
	case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:

	case CNodeTree::ENodeType::NT_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:

	case CNodeTree::ENodeType::NT_TOOLBOX_OBJECT:

	case CNodeTree::ENodeType::NT_OBJ_FIELDRECT:
	case CNodeTree::ENodeType::NT_OBJ_TEXTRECT:
	case CNodeTree::ENodeType::NT_OBJ_FILERECT:
	case CNodeTree::ENodeType::NT_OBJ_SQRRECT:
	case CNodeTree::ENodeType::NT_OBJ_GRAPHRECT:
	case CNodeTree::ENodeType::NT_OBJ_TABLE:
	case CNodeTree::ENodeType::NT_OBJ_REPEATER:
	case CNodeTree::ENodeType::NT_OBJ_CHART:
	case CNodeTree::ENodeType::NT_OBJ_CATEGORY:
	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		break;
	}

	default:
	{
		m_hDragItem = NULL;
		return;
	}
	}

	if (DragText(m_strDragCommand, DROPEFFECT_MOVE) == DROPEFFECT_MOVE)
	{
		//SelectItem(m_hDragItem);
	}

	*pResult = 0;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnItemExpanding(NMHDR* pNMHDR, LRESULT* pResult)
{
	NM_TREEVIEW* pNMTreeView = (NM_TREEVIEW*)pNMHDR;
	if (!pNMTreeView)
		return;


	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	if (!hCurrentItem)
		hCurrentItem = GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = GetNode(hCurrentItem);
	if (!pNode)
		return;

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_TABLES:
	{
		if ((pNMTreeView->hdr.code == TVN_ITEMEXPANDING) && (pNMTreeView->action == TVE_EXPAND))
		{
			if (m_bShowAllTables)
				FillAllTables();
			else
				FillTables();
		}
		else  if (pNMTreeView->action == TVE_COLLAPSE)
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_SUBROOT_DB_MODULE:
	{
		if (pNMTreeView->action == TVE_EXPAND)
			this->FillSubModuleTables(pNode, hCurrentItem);
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DBTABLE:
	case CNodeTree::ENodeType::NT_LIST_DBVIEW:
	{
		if (pNMTreeView->action == TVE_EXPAND)
		{
			CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pNode->m_pParentItemData);

			DataFieldLinkArray* pdfLink = NULL;
			HTREEITEM htParent = GetParentItem(hCurrentItem);
			if (htParent)
			{
				CNodeTree* pParentNode = GetNode(htParent);
				ASSERT_VALID(pParentNode);
				ASSERT_KINDOF(CNodeTree, pParentNode);
				if (pParentNode)
				{
					if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_USED_TABLES)
					{
						TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pParentNode->m_pItemData);
						int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pTC->m_pCatalogEntry->m_strTableName);
						if (idx > -1)
							pdfLink = pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[idx];
					}
				}
			}

			FillTableColumns(pTC, hCurrentItem, pdfLink);
		}
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN:
	{
		if (pNMTreeView->action == TVE_EXPAND)
		{
			FillUnselectedColumns(pNode);
		}
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY:
	{
		if (pNMTreeView->action == TVE_EXPAND)
		{
			CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pNode->m_pItemData);
			FillForeignKey(pTC, hCurrentItem);
		}
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES:
	{
		if (pNMTreeView->action == TVE_EXPAND)
		{
			CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pNode->m_pItemData);
			FillExternalReference(pTC, hCurrentItem);
		}
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_ROOT_COMMANDS:
		if (pNMTreeView->action == TVE_EXPAND)
			this->FillCommands(GetDocument()->GetWoormFrame()->m_pEditView);
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;

	case CNodeTree::ENodeType::NT_ROOT_WEBMETHODS:
		if (pNMTreeView->action == TVE_EXPAND)
			this->FillWebMethods(GetDocument()->GetWoormFrame()->m_pEditView);
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;

	case CNodeTree::ENodeType::NT_ROOT_FUNCTIONS:
		if (pNMTreeView->action == TVE_EXPAND)
			this->FillFunctions(GetDocument()->GetWoormFrame()->m_pEditView);
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;

	case CNodeTree::ENodeType::NT_ROOT_ENUMS:
		if (pNMTreeView->action == TVE_EXPAND)
			this->FillEnums(GetDocument()->GetWoormFrame()->m_pEditView);
		else
		{
			RemoveTreeChilds(hCurrentItem);
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, hCurrentItem);
		}
		break;

	case CNodeTree::ENodeType::NT_ROOT_MACRO_TEXT:
		this->FillSpecialTextRect(FALSE);
		break;
	case CNodeTree::ENodeType::NT_ROOT_HTML_TAGS:
		this->FillHtmlTags(GetDocument()->GetWoormFrame()->m_pEditView, FALSE);
		break;
	case CNodeTree::ENodeType::NT_ROOT_QUERY_TAGS:
		this->FillQueriesTags(GetDocument()->GetWoormFrame()->m_pEditView, FALSE);
		break;

	default:
		return;
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::CheckAndClearEdge(HTREEITEM ht)
{
	if (!ht)
		return FALSE;

	RemoveTreeChilds(ht);

	ASSERT_VALID(m_pWDoc);
	ASSERT_VALID(m_pWDoc->m_pEditorManager);
	if (!m_pWDoc->m_pEditorManager)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData());
	if (!m_pWDoc->m_pEditorManager->GetPrgData())
		return FALSE;

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CRSTreeCtrl::IsEqualItemData(DWORD dwItemData, DWORD dwFindData)
{
	ASSERT(dwFindData);
	if (dwItemData == 0) return FALSE;

	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)dwItemData);
	if (!pNode)
		return __super::IsEqualItemData(dwItemData, dwFindData);
	ASSERT_VALID(pNode);

	CNodeTree* pFindNode = dynamic_cast<CNodeTree*>((CObject*)dwFindData);
	if (pFindNode)
	{
		ASSERT_VALID(pFindNode);
		return pNode == pFindNode;
	}

	if (pNode->m_ppBlock && dwFindData == (DWORD)*pNode->m_ppBlock)
		return TRUE;
	if (pNode->m_ppExpr && dwFindData == (DWORD)*pNode->m_ppExpr)
		return TRUE;

	return dwFindData == (DWORD)pNode->m_pItemData;
}

//-----------------------------------------------------------------------------
//void CRSTreeCtrl::SetItemImage(HTREEITEM htItem, int nImage)
//{
//	this->SetItemImage(htItem, nImage);
//}

//-----------------------------------------------------------------------------
CNodeTree& CRSTreeCtrl::AddNode(const CString& sTitle, CNodeTree::ENodeType eType, HTREEITEM htParent,
	CObject* pItem, CObject* pParentItem, CObject* pAncestorItem, BOOL bIsHidden)
{
	ASSERT(bIsHidden == FALSE || bIsHidden == TRUE);	//per il cambio del numero dei parametri del metodo

	int nImage = GetImgIndex(eType);
	if (eType == CNodeTree::ENodeType::NT_OBJ_COLUMN)
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(pItem);
		if (pCol)
		{
			if (pCol->m_pHideExpr && !pCol->m_pHideExpr->IsEmpty())
				nImage = CRSTreeCtrlImgIdx::ColumnHiddenExprGlyph;
			else if (pCol->IsHidden())
				nImage = CRSTreeCtrlImgIdx::ColumnHiddenGlyph;
			else
				nImage = CRSTreeCtrlImgIdx::ColumnGlyph;
		}
	}
	if (eType == CNodeTree::ENodeType::NT_OBJ_TABLE)
	{
		Table* pTable = dynamic_cast<Table*>(pItem);
		if (pTable)
		{
			if (pTable->m_pHideExpr && !pTable->m_pHideExpr->IsEmpty())
				nImage = CRSTreeCtrlImgIdx::TableHiddenExprGlyph;
			else if (pTable->IsAlwaysHidden())
				nImage = CRSTreeCtrlImgIdx::TableHiddenGlyph;
			else
				nImage = CRSTreeCtrlImgIdx::TableGlyph;
		}
	}
	if (eType == CNodeTree::ENodeType::NT_VARIABLE)
	{
		if (nImage == CRSTreeCtrlImgIdx::NoGLyph)
		{
			WoormField* pF = dynamic_cast<WoormField*>(pItem);
			if (pF)
			{
				if (pF->IsColTotal())
					nImage = CRSTreeCtrlImgIdx::Total;
				else if (pF->IsSpecialField())
					nImage = CRSTreeCtrlImgIdx::DataPrimaryKeyGlyph;
				else if (pF->IsTableRuleField())
					nImage = CRSTreeCtrlImgIdx::DataGlyph;
				else if (pF->IsExprRuleField())
					nImage = CRSTreeCtrlImgIdx::ExprGlyph;
				else if (pF->IsArray())
					nImage = CRSTreeCtrlImgIdx::FuncArrayGlyph;
				else if (pF->IsInput())
					nImage = CRSTreeCtrlImgIdx::InputVarGlyph;
				else
					nImage = CRSTreeCtrlImgIdx::FuncGlyph;
			}
		}
	}
	if (eType == CNodeTree::ENodeType::NT_LINK && nImage < 0)
	{
		WoormLink* pL = dynamic_cast<WoormLink*>(pItem);
		if (pL)
		{
			switch (pL->m_LinkType)
			{
			case WoormLink::WoormLinkType::ConnectionReport:
				nImage = CRSTreeCtrlImgIdx::LinkToReport;
				break;
			case WoormLink::WoormLinkType::ConnectionForm:
				nImage = CRSTreeCtrlImgIdx::LinkToForm;
				break;
			case WoormLink::WoormLinkType::ConnectionFunction:
				nImage = CRSTreeCtrlImgIdx::LinkToFunction;
				break;
			case WoormLink::WoormLinkType::ConnectionURL:
			{
				switch (pL->m_SubType)
				{
				case WoormLink::WoormLinkSubType::CallTo:
					nImage = CRSTreeCtrlImgIdx::CallTo;
					break;
				case WoormLink::WoormLinkSubType::MailTo:
					nImage = CRSTreeCtrlImgIdx::MailTo;
					break;
				case WoormLink::WoormLinkSubType::File:
					nImage = CRSTreeCtrlImgIdx::LinkToFile;
					break;
				case WoormLink::WoormLinkSubType::GoogleMap:
					nImage = CRSTreeCtrlImgIdx::GoogleMap;
					break;
				case WoormLink::WoormLinkSubType::Url:
					nImage = CRSTreeCtrlImgIdx::LinkToUrl;
					break;
				}
				break;
			}
			}
		}
	}
	else if (eType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO)
	{
		const SqlColumnInfoObject* pCol = (SqlColumnInfoObject*)(pItem);
		if (pCol->m_bSpecial)
			nImage = CRSTreeCtrlImgIdx::PrimaryKey;
	}
	else if (
		eType == CNodeTree::ENodeType::NT_LIST_DB_FOREIGN_KEY
		||
		eType == CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES
		||
		eType == CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE
		)
	{
		nImage = CRSTreeCtrlImgIdx::ForeignKey;
	}

	HTREEITEM ht = InsertItem(sTitle, nImage, nImage, htParent);
	CNodeTree* pNode = new CNodeTree(ht, (CRSTreeCtrlImgIdx)nImage, eType, pItem, pParentItem, pAncestorItem);
	m_arGarbage.Add(pNode);
	SetItemData(ht, (DWORD)pNode);

	switch (eType)
	{
	case CNodeTree::ENodeType::NT_RULE_QUERY_WHERE:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY:
	case CNodeTree::ENodeType::NT_RULE_QUERY_HAVING:
	case CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY:
	case CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON:

	case CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS:

	{
		pNode->SetItemFont(m_pItalic);
		pNode->SetItemColor(RS_COLOR_ACTIONS);
		break;
	}

	case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
	case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:

	case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
	case CNodeTree::ENodeType::NT_LAYOUT:

	case CNodeTree::ENodeType::NT_ROOT_LINKS:
	case CNodeTree::ENodeType::NT_LINK_PARAMETERS:

	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
	case CNodeTree::ENodeType::NT_ROOT_MODULE:
	case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:

	case CNodeTree::ENodeType::NT_ROOT_EVENTS:
	case CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS:
	case CNodeTree::ENodeType::NT_SUBROOT_REPORT_EVENTS:
	case CNodeTree::ENodeType::NT_SUBROOT_FORMFEED_EVENTS:
	case CNodeTree::ENodeType::NT_SUBROOT_FILLTABLE_EVENTS:

	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:

	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
	case CNodeTree::ENodeType::NT_ASKCONTROLS:

	case CNodeTree::ENodeType::NT_PAGE:
	case CNodeTree::ENodeType::NT_PROPERTIES:
	case CNodeTree::ENodeType::NT_SETTINGS:

	case CNodeTree::ENodeType::NT_ROOT_RULES:

	case CNodeTree::ENodeType::NT_SUBROOT_DB_MODULE:
	case CNodeTree::ENodeType::NT_SUBROOT_MODULE:
	case CNodeTree::ENodeType::NT_SUBROOT_APP:

	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_SELECT:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_PARAMETERS:
	case CNodeTree::ENodeType::NT_ROOT_USED_TABLES:
	case CNodeTree::ENodeType::NT_GROUP_FUNCTIONS:
	{
		pNode->SetItemFont(m_pBold);
		break;
	}

	case CNodeTree::ENodeType::NT_DEBUG_FILTER_TUPLE_RULE:
	case CNodeTree::ENodeType::NT_DEBUG_GROUPING_RULE:
	case CNodeTree::ENodeType::NT_DEBUG_HAVINGGROUP_RULE:

	{
		pNode->SetItemFont(m_pBoldItalic);
		break;
	}

	case CNodeTree::ENodeType::NT_ROOT_ENUMS:
	case CNodeTree::ENodeType::NT_ROOT_FUNCTIONS:
	case CNodeTree::ENodeType::NT_ROOT_WEBMETHODS:
	case CNodeTree::ENodeType::NT_ROOT_MACRO_TEXT:
	case CNodeTree::ENodeType::NT_ROOT_COMMANDS:
	case CNodeTree::ENodeType::NT_ROOT_TABLES:
	case CNodeTree::ENodeType::NT_ROOT_FONTS_TABLE:
	case CNodeTree::ENodeType::NT_ROOT_FORMATTERS_TABLE:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN:
	case CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY:
	case CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES:
	case CNodeTree::ENodeType::NT_ROOT_BREAKPOINTS:
	case CNodeTree::ENodeType::NT_ROOT_HTML_TAGS:
	case CNodeTree::ENodeType::NT_ROOT_QUERY_TAGS:
	{
		pNode->SetItemFont(m_pBold);
		pNode->SetItemColor(RS_COLOR_FRAMEWORK);
		break;
	}

	case CNodeTree::ENodeType::NT_DUMMY_NODE:
	{
		pNode->SetItemFont(m_pItalic);
		pNode->SetItemColor(RS_COLOR_EMPTY);
		break;
	}

	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pF = dynamic_cast<WoormField*>(pItem);
		if (pF && pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			pNode->SetItemColor(RS_COLOR_LIGHT_BLUE);
		break;
	}
	}

	return *pNode;
}

//-----------------------------------------------------------------------------
CNodeTree&  CRSTreeCtrl::AddNode(int nImage, const CString& sTitle, HTREEITEM htParent, SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode)
{
	HTREEITEM ht = InsertItem(sTitle, nImage, nImage, htParent);
	CNodeTree* pNode = new CNodeTree(ht, (CRSTreeCtrlImgIdx)nImage, pSymTable, ppExpr, dtReturnType, bViewMode);
	m_arGarbage.Add(pNode);
	SetItemData(ht, (DWORD)pNode);

	pNode->SetItemFont(m_pItalic);
	pNode->SetItemColor(RS_COLOR_ACTIONS);
	return *pNode;
}

//-----------------------------------------------------------------------------
CNodeTree&  CRSTreeCtrl::AddNode(int nImage, const CString& sTitle, HTREEITEM htParent, SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents)
{
	HTREEITEM ht = InsertItem(sTitle, nImage, nImage, htParent);
	CNodeTree* pNode = new CNodeTree(ht, (CRSTreeCtrlImgIdx)nImage, pSymTable, ppBlock, bRaiseEvents);
	m_arGarbage.Add(pNode);
	SetItemData(ht, (DWORD)pNode);

	pNode->SetItemFont(m_pItalic);
	pNode->SetItemColor(RS_COLOR_ACTIONS);
	return *pNode;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::AddDelayedNode(HTREEITEM& ht, const CString& sTitle, CNodeTree::ENodeType eType, HTREEITEM htParent/* = NULL*/,
	CObject* pItem /*= NULL*/, CObject* pParentItem /*= NULL*/, CObject* pAncestorItem /*= NULL*/, BOOL bIsHidden /*= FALSE*/)
{
	if (!ht)
	{
		ht = AddNode(sTitle, eType, htParent, pItem, pParentItem, pAncestorItem, bIsHidden);
		AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, ht);
		return TRUE;
	}

	HTREEITEM htChild = this->GetChildItem(ht);
	if (htChild)
	{
		CNodeTree* pNode = GetNode(htChild);
		if (!pNode)
			return FALSE;
		if (pNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
		{
			DeleteItem(htChild);
			return FALSE;
		}
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void  CRSTreeCtrl::SetItemImage(HTREEITEM ht, CRSTreeCtrlImgIdx imgIdx)
{
	CNodeTree* pNode = GetNode(ht);
	if (pNode)
		pNode->m_eImgIndex = imgIdx;
	__super::SetItemImage(ht, imgIdx, imgIdx);
}

void  CRSTreeCtrl::SetItemImage(CNodeTree* pNode, CRSTreeCtrlImgIdx imgIdx)
{
	ASSERT_VALID(pNode);
	pNode->m_eImgIndex = imgIdx;
	__super::SetItemImage(pNode->m_ht, imgIdx, imgIdx);
}

//-----------------------------------------------------------------------------
CNodeTree* CRSTreeCtrl::GetNode(HTREEITEM htItem)
{
	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)GetItemData(htItem));
	ASSERT_VALID(pNode);
	return pNode;
}

//-----------------------------------------------------------------------------
CNodeTree* CRSTreeCtrl::GetParentNode(HTREEITEM htItem)
{
	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)GetItemData(GetParentItem(htItem)));
	ASSERT_VALID(pNode);
	return pNode;
}

//---------------------------------------------------------------------------
//loop on parents until type was found
CNodeTree* CRSTreeCtrl::GetAncientNode(HTREEITEM htItem, CNodeTree::ENodeType type)
{
	HTREEITEM ht = htItem;

	while (ht = GetParentItem(ht))
	{
		CNodeTree* nd = GetNode(ht);
		if (nd && nd->m_NodeType == type)
			return nd;
	}
	return NULL;
}

//---------------------------------------------------------------------------
//loop on children until type was found
CNodeTree* CRSTreeCtrl::GetDescendantNode(HTREEITEM htItem, CNodeTree::ENodeType type)
{
	if (ItemHasChildren(htItem))
	{
		for (HTREEITEM htChildItem = GetChildItem(htItem); htChildItem != NULL; htChildItem = GetNextItem(htChildItem, TVGN_NEXT))
		{
			CNodeTree* pNode = GetNode(htChildItem);
			if (pNode->m_NodeType == type)
				return pNode;

			pNode = GetDescendantNode(htChildItem, type);
			if (pNode)
				return pNode;
		}
	}
	return NULL;
}

//---------------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::Move(HTREEITEM htCurrent, BOOL bNext)
{
	//TODO deep clone : tanto vale ricrearlo
	ASSERT(FALSE);
	return NULL;
}

//---------------------------------------------------------------------------

BOOL CRSTreeCtrl::OnFindItemTextOnFirstChild(HTREEITEM htItem)
{
	Expand(htItem, TVE_EXPAND);
	return TRUE;
}

void CRSTreeCtrl::OnFindItemTextOnLastChild(HTREEITEM htItem)
{
	Expand(htItem, TVE_COLLAPSE);
}

BOOL CRSTreeCtrl::OnFindItemTextOnChild(HTREEITEM htItem, const CString& str)
{
	CNodeTree* pNode = GetNode(htItem);
	if (pNode)
	{
		if (
			(
				this == &GetDocument()->GetWoormFrame()->GetEngineTreeView()->m_TreeCtrl
				||
				this == &GetDocument()->GetWoormFrame()->GetLayoutTreeView()->m_TreeCtrl
				)
			&&
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(pNode)
			)
		{
			CString sText = GetDocument()->GetWoormFrame()->m_pEditorDockedView->GetText();
			CString s = str;
			s.Trim('*'); s = '*' + s + '*';
			if (::WildcardMatch(sText, str))
				return TRUE;
		}
	}
	return FALSE;
}

//--------------------------------------------------------------------------
CNodeTree*	CRSTreeCtrl::FindNode(CObject* pItemData, HTREEITEM htRoot /*= TVI_ROOT*/)
{
	if (!pItemData)
		return NULL;
	HTREEITEM ht = FindItemData((DWORD)pItemData, htRoot);
	if (!ht)
		return NULL;
	return GetNode(ht);
}

//-----------------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::SelectRSTreeItemData(CObject* pObj, HTREEITEM htRoot, BOOL appendToSelected)
{
	if (!pObj)
		return NULL;

	if (!appendToSelected)
		ClearSelection();

	HTREEITEM ht = FindItemData((DWORD)pObj, htRoot);
	if (ht)
	{
#ifdef _DEBUG
		CString strName = GetItemText(ht);
#endif
		//deseleziono quello precedente per far scattare correttamente la onselchanged
		//qualora fosse rimasto selezionato lo stesso elemento che sto tentando di 
		//selezionare ora
		if (!appendToSelected)
		{
			SelectItem(NULL);
			SelectItem(ht);
			EnsureVisible(ht);
			Expand(ht, TVE_EXPAND);
		}
		else
			if (!(GetItemState(ht, TVIS_SELECTED) & TVIS_SELECTED))
				SetItemState(ht, TVIS_SELECTED, TVIS_SELECTED);
	}
	return ht;
}

//-----------------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::DeselectRSTreeItemData(CObject* pObj, HTREEITEM htRoot)
{
	HTREEITEM ht = FindItemData((DWORD)pObj, htRoot);
	if (ht)
		DeselectItem(ht);
	return ht;
}

//----------------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::SelectRSTreeItemByMatchingText(const CString& sMatchText, HTREEITEM hCurrentItem)
{
	CWaitCursor wc;

	CString s = sMatchText;
	s.Trim(); s.Trim(L"*%");
	if (s.IsEmpty())
		return NULL;
	s = '*' + s + '*';

	HTREEITEM ht = FindItemText(s, hCurrentItem);
	if (ht)
	{
		SelectItem(ht);
		EnsureVisible(ht);
	}
	else
	{
		SelectItem(NULL);
		ExpandAll(TVE_COLLAPSE);
	}
	return ht;
}

HTREEITEM CRSTreeCtrl::FindItem(const CString& name, HTREEITEM hRoot)
{
	// check whether the current item is the searched one
	CString text = GetItemText(hRoot);
	if (text.Compare(name) == 0)
		return hRoot;

	// get a handle to the first child item
	HTREEITEM hSub = GetChildItem(hRoot);
	// iterate as long a new item is found
	while (hSub)
	{
		// check the children of the current item
		HTREEITEM hFound = FindItem(name, hSub);
		if (hFound)
			return hFound;

		// get the next sibling of the current item
		hSub = GetNextSiblingItem(hSub);
	}

	// return NULL if nothing was found
	return NULL;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnPaint()
{
	CTreeCtrl::Invalidate(TRUE);

	CTreeCtrl::OnPaint();

	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	UINT uCount = GetVisibleCount();
	HTREEITEM hItem = GetFirstVisibleItem();
	for (UINT i = 0; i < uCount && hItem; i++)
	{
		PaintItem(hItem);

		hItem = GetNextVisibleItem(hItem);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::IsEmptyNodeGroup(CNodeTree* pNode)
{
	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_LAYOUT:

	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:

	case CNodeTree::ENodeType::NT_ROOT_RULES:
	case CNodeTree::ENodeType::NT_SUBROOT_FILLTABLE_EVENTS:
	case CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS:
	case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
	case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:

	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:
	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:

	case CNodeTree::ENodeType::NT_LINK_PARAMETERS:

	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_SELECT:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_PARAMETERS:

	case CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY:
	case CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES:

		return this->GetChildItem(pNode->m_ht) == NULL;

	case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
	{
		GroupByData* pGroupBy = dynamic_cast<GroupByData*>(pNode->m_pItemData);
		if (pGroupBy && !pGroupBy->IsEmpty())
			return FALSE;

		QueryEngine* pQEngine = dynamic_cast<RepEngine*>(pNode->m_pItemData);
		if (pQEngine)
		{
			if (pQEngine->m_pTupleFilterEngine && !pQEngine->m_pTupleFilterEngine->IsEmpty())
				return FALSE;
			if (pQEngine->m_pGroupingTupleEngine && !pQEngine->m_pGroupingTupleEngine->IsEmpty())
				return FALSE;
			if (pQEngine->m_pHavingTupleFilterEngine && !pQEngine->m_pHavingTupleFilterEngine->IsEmpty())
				return FALSE;
		}
		return TRUE;
	}

	case CNodeTree::ENodeType::NT_DEBUG_FILTER_TUPLE_RULE:
	{
		QueryEngine* pQEngine = dynamic_cast<QueryEngine*>(pNode->m_pParentItemData);
		if (pQEngine)
		{
			if (pQEngine->m_pTupleFilterEngine && !pQEngine->m_pTupleFilterEngine->IsEmpty())
				return FALSE;
		}
		return TRUE;
	}
	case CNodeTree::ENodeType::NT_DEBUG_GROUPING_RULE:
	{
		QueryEngine* pQEngine = dynamic_cast<QueryEngine*>(pNode->m_pParentItemData);
		if (pQEngine)
		{
			if (pQEngine->m_pGroupingTupleEngine && !pQEngine->m_pGroupingTupleEngine->IsEmpty())
				return FALSE;
		}
		return TRUE;
	}
	case CNodeTree::ENodeType::NT_DEBUG_HAVINGGROUP_RULE:
	{
		QueryEngine* pQEngine = dynamic_cast<QueryEngine*>(pNode->m_pParentItemData);
		if (pQEngine)
		{
			if (pQEngine->m_pHavingTupleFilterEngine && !pQEngine->m_pHavingTupleFilterEngine->IsEmpty())
				return FALSE;
		}
		return TRUE;
	}

	case CNodeTree::ENodeType::NT_TUPLE_GROUPING:
	case CNodeTree::ENodeType::NT_TUPLE_FILTER:
	case CNodeTree::ENodeType::NT_TUPLE_HAVING_FILTER:
	case CNodeTree::ENodeType::NT_EXPR:
	{
		ASSERT(pNode->m_ppExpr);
		if (*pNode->m_ppExpr)
			ASSERT_VALID(*pNode->m_ppExpr);

		Expression* pExpr = dynamic_cast<Expression*>(*pNode->m_ppExpr);

		if (!pExpr || pExpr->IsEmpty())
			return TRUE;
		else
			return FALSE;
	}

	case CNodeTree::ENodeType::NT_BLOCK:
	{
		ASSERT(pNode->m_ppBlock);
		if (*pNode->m_ppBlock)
			ASSERT_VALID(*pNode->m_ppBlock);

		Block* pBlock = dynamic_cast<Block*>(*pNode->m_ppBlock);

		if (!pBlock || pBlock->GetCount() == 0)
			return TRUE;
		else
			return FALSE;
	}

	case CNodeTree::ENodeType::NT_RULE_QUERY_WHERE:
	case CNodeTree::ENodeType::NT_RULE_QUERY_HAVING:
	case CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON:
	{
		WClause* pWC = dynamic_cast<WClause*>(pNode->m_pItemData);
		if (!pWC || pWC->IsEmpty())
			return TRUE;
		else
			return FALSE;
	}

	case CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY:
	{
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pItemData);
		ASSERT_VALID(pTblRule);
		if (pTblRule->m_strOrderBy.IsEmpty())
			return TRUE;
		else
			return FALSE;
	}
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY:
	{
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pItemData);
		ASSERT_VALID(pTblRule);
		if (pTblRule->m_strGroupBy.IsEmpty())
			return TRUE;
		else
			return FALSE;
	}

	case CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS:
	{
		GroupByData* pGD = dynamic_cast<GroupByData*>(pNode->m_pItemData);
		ASSERT_VALID(pGD);
		if (pGD->m_ActionsArray.IsEmpty())
			return TRUE;
		else
			return FALSE;
	}

	default:
		break;
	}
	return FALSE;
}


//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::NeedCustomPaint()
{
	return m_bNeedCustomPaint;
}

void CRSTreeCtrl::PaintItem(HTREEITEM ht)
{
	if (!NeedCustomPaint())
		return;

	CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)GetItemData(ht));
	if (!pNode)
		return;
	ASSERT_VALID(pNode);
	ASSERT(pNode->m_ht == ht);

	//Get the label rectangle area of the item
	CRect labelRect;
	GetItemRect(ht, &labelRect, true);

	//GetItemRect works only for expanded items.So if it is not shown/collpased then the
	//rect will carry junk value
	if (labelRect.left < 0 || labelRect.right < 0 || labelRect.top < 0 || labelRect.bottom < 0)
		return;

	//Check if the node hasn't an image
	if (pNode->m_eImgIndex == CRSTreeCtrlImgIdx::NoGLyph)
	{
		int imgW, imgH;
		ImageList_GetIconSize(GetImageList(TVSIL_NORMAL)->GetSafeHandle(), &imgW, &imgH);
		labelRect.left = labelRect.left - (imgW);
	}

	//Get the text of the item to be drawn
	CString sItem = GetItemText(ht);

	//Create client dc to draw
	CClientDC pDC(this);

	//Text color black is default
	BOOL bEmpty = IsEmptyNodeGroup(pNode);
	COLORREF cItemColor = bEmpty ? RS_COLOR_EMPTY : pNode->GetItemColor();

	//Set the text color to the item text color
	pDC.SetTextColor(pNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG ? RGB(255, 0, 0) : cItemColor);

	//Check Whether the item is highlighted or selected
	UINT selflag = TVIS_DROPHILITED | TVIS_SELECTED;
	UINT uItemSelState = GetItemState(ht, selflag);

	//Get the node font (Theme Control font is default)
	CFont* pFont = pNode->GetItemFont();
	pDC.SelectObject(pFont);

	if (pFont == m_pBold)
		labelRect.right = labelRect.right + 10;

	//Delete the text drawn by the system by drawing a rectangle with the color of the 
	//tree control's background color and we are going to draw the label again
	CRect bkRect = labelRect;

	//Add some padding to completely cover the text which is drawn by default by the tree
	bkRect.right = bkRect.right + bkRect.Width() / 3;

	//TAPPULLO
		//COLORREF bkColor = RGB(255,255,255); //GetBkColor();
	COLORREF bkColor = GetBkColor();

	CBrush bkbrush(bkColor);
	CGdiObject* pOldBrush = pDC.SelectObject(&bkbrush);

	CPen bkPen(PS_SOLID, 1, bkColor);
	CPen* pOldpen = pDC.SelectObject(&bkPen);

	if (pNode->NeedCustomPaint() || pNode->m_eImgIndex == CRSTreeCtrlImgIdx::NoGLyph)
		pDC.Rectangle(bkRect);
	pDC.SelectObject(pOldBrush);
	pDC.SelectObject(pOldpen);

	if ((uItemSelState & selflag) || IsNodeInMultiSelection(pNode))
	{
		//If so draw the highlighting rectangle.But already a highlighting rectangle is drawn
		//when CTreeCtrl::OnPaint() is being called.After that we are drawing the text for each
		//item.So the text is drawn over the highlighting rect.So we have to draw a new highlighting
		//rect.Inflate the rect by 1 or 2 pixlel so that it completely hides the highlighting
		//rectangle drawn by the system

		//If the item is selected, as the background goes to the COLOR_HIGHLIGHT the text
		//should be in COLOR_HIGHLIGHTTEXT to have clear visibility.
		pDC.SetTextColor(RGB(0, 0, 0));	//(::GetSysColor(COLOR_HIGHLIGHTTEXT));

		//Get the extents [Width,Height] of the label of the item in pixels
		CSize sz = pDC.GetTextExtent(sItem);

		//Convert the ordinates of the label rectangle into screen coordinates
		ClientToScreen(&labelRect);

		//Compute the bounding rectangle
		CRect rc = labelRect;

		//Create a pen of highlight color to draw the border of the highlighting rectangle
		CPen highlightPen;
		//highlightPen.CreatePen(PS_ALTERNATE, 1, RGB(0,0,0)); //::GetSysColor(COLOR_HIGHLIGHT));
		LOGBRUSH LogBrush;
		LogBrush.lbColor = GetDocument()->GetObjectSelectionColor();	//RGB(0,0,0);
		LogBrush.lbStyle = PS_SOLID;
		highlightPen.CreatePen(PS_COSMETIC | PS_ALTERNATE/*PS_SOLID*/, 1, &LogBrush, 0, NULL);

		pDC.SelectObject(&highlightPen);

		//Create brush with highlighting color to draw the highlight rectangle
		CBrush highlightBrush(RS_COLOR_HighlightItemTree); //::GetSysColor(COLOR_HIGHLIGHT));

		//If the tree has the focus then draw the background inthe highlight color
		if (GetFocus() ? GetFocus()->m_hWnd == GetSafeHwnd() : FALSE)
		{
			//Select the brush with highlighting color
			pOldBrush = pDC.SelectObject(&highlightBrush);
		}
		else
		{
			//Set the text color to the item text color
			pDC.SetTextColor(cItemColor);
		}

		//We computed the rc using the cooridinates of the labelRect which is in screen 
		//coordinates.Convert  back to client coordinates.Draw the highlighting rectangle
		ScreenToClient(&rc);
		pDC.Rectangle(rc);

		//Similarly conver the labelRect to client cooridnates
		ScreenToClient(&labelRect);
		if (pNode->m_eImgIndex != CRSTreeCtrlImgIdx::NoGLyph)
			labelRect.left = labelRect.left + 2;//perchè sennò altrimenti risulta spostata di 2 pixel

		//Set the background mode to transparent so that highlighting rectangle
		//is not overwritten by bounding rectangle of the text while doing TextOut
		pDC.SetBkMode(TRANSPARENT);

		bool bIgnoreDrawText = false;
		if (GetEditControl())
		{
			bIgnoreDrawText = true;
		}

		//Draw the text
		if (!bIgnoreDrawText)
		{
			CString s = L"****";
			s += sItem;
			pDC.DrawText(sItem, labelRect, DT_SINGLELINE | DT_LEFT | DT_VCENTER);
		}
		//Restore the old objects
		pDC.SelectObject(&pOldpen);
		pDC.SelectObject(&pOldBrush);
	}
	else if (pNode->NeedCustomPaint() || pNode->m_eImgIndex == CRSTreeCtrlImgIdx::NoGLyph)
	{
		CString s = L"+++";
		s += sItem;
		pDC.DrawText(sItem, labelRect, DT_SINGLELINE | DT_LEFT | DT_VCENTER);
	}
}

//-----------------------------------------------------------------------------

BOOL CRSTreeCtrl::IsNodeInMultiSelection(CNodeTree* pNode)
{
	ASSERT_VALID(GetDocument());
	ASSERT_VALID(pNode);
	if (!pNode) return FALSE;

	if (GetDocument()->m_pNodesSelection && GetDocument()->m_pNodesSelection->Find(pNode) != -1)
		return TRUE;

	if (GetDocument()->m_pMultipleSelObj && IsLayoutNodeType(pNode->m_NodeType) && GetDocument()->m_pMultipleSelObj->Find(pNode) != -1)
		return TRUE;

	if (GetDocument()->m_pMultiColumns && IsLayoutNodeType(pNode->m_NodeType) && GetDocument()->m_pMultiColumns->Find(pNode) != -1)
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillProcedures()
{
	ProcedureData*	pProcedures = m_pWDoc->m_pEditorManager->GetPrgData()->GetProcedureData();
	ASSERT_VALID(pProcedures);

	if (!m_htProcedures)
		m_htProcedures = AddNode(_T("Procedures"), CNodeTree::ENodeType::NT_ROOT_PROCEDURES, NULL, pProcedures);

	if (!CheckAndClearEdge(m_htProcedures))
		return FALSE;

	for (int i = 0; i < pProcedures->GetSize(); i++)
	{
		ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pProcedures->GetAt(i));
		ASSERT_VALID(pProc);

		/*HTREEITEM htProc = */AddNode(pProc->GetName() /*+ L"..."*/, CNodeTree::ENodeType::NT_PROCEDURE, m_htProcedures, pProc, pProcedures);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillProceduresForDebug(Block* pCurrent)
{
	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	const Array& arProcedures = pEngine->GetSymTable().GetProcedures();
	if (arProcedures.GetSize() == 0)
		return FALSE;

	if (!m_htProcedures)
		m_htProcedures = AddNode(_T("Procedures"), CNodeTree::ENodeType::NT_ROOT_PROCEDURES);

	if (!CheckAndClearEdge(m_htProcedures))
		return FALSE;

	BOOL bExpand = FALSE;
	for (int i = 0; i < arProcedures.GetSize(); i++)
	{
		ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(arProcedures.GetAt(i));
		ASSERT_VALID(pProc);
		ASSERT_VALID(pProc->m_pProcedure);
		ASSERT_VALID(pProc->m_pProcedure->m_pBlock);

		bExpand = bExpand || (pProc->m_pProcedure->m_pBlock == pCurrent);
		CNodeTree& nt = AddNode(pProc->m_pProcedure->m_pBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph, pProc->GetName(), m_htProcedures, &pEngine->m_SymTable, &pProc->m_pProcedure->m_pBlock, TRUE);
		//if (pProc->m_pProcedure->m_pBlock->m_bHasBreakpoint) nt.SetItemColor(RS_COLOR_BREAKPOINT);
	}
	if (bExpand)
		Expand(m_htProcedures, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillQueries()
{
	QueryObjectData* pQueries = m_pWDoc->m_pEditorManager->GetPrgData()->GetQueryObjectData();
	ASSERT_VALID(pQueries);

	if (!m_htQueries)
		m_htQueries = AddNode(_T("Queries"), CNodeTree::ENodeType::NT_ROOT_QUERIES, NULL, pQueries);

	if (!CheckAndClearEdge(m_htQueries))
		return FALSE;

	for (int i = 0; i < pQueries->GetSize(); i++)
	{
		QueryObjItem* pQry = dynamic_cast<QueryObjItem*>(pQueries->GetAt(i));
		ASSERT_VALID(pQry);

		HTREEITEM htQ = AddNode(pQry->GetName() /*+ L"..."*/, CNodeTree::ENodeType::NT_NAMED_QUERY, m_htQueries, pQry, pQueries);

		FillChildNamedQuery(htQ, pQry, NULL);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillVariables(BOOL bSort, BOOL bViewMode, BOOL bSkipSpecial, BOOL bSkipInput, BOOL bSkipTotal)
{
	if (m_bShowMoreVariable)
	{
		return FillAllVariables(bSort, bViewMode, bSkipSpecial, bSkipInput, bSkipTotal);
	}

	if (!m_htVariables)
	{
		m_htHiddenGroupVariables = m_htVariables = AddNode(_T("Hidden variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES);
	}
	else this->SetItemText(m_htVariables, _T("Hidden variables"));

	if (!CheckAndClearEdge(m_htVariables))
		return FALSE;

	//CArray<WoormField*, WoormField*> arHiddenInputGroup;
	CArray<WoormField*, WoormField*> arSpecialGroup;

	WoormTable*	pProgSymTable = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable();
	ASSERT_VALID(pProgSymTable);

	WoormTable*	pSymTable = pProgSymTable;

	if (bViewMode)
	{
		m_pWDoc->SyncronizeViewSymbolTable(pProgSymTable);

		pSymTable = &m_pWDoc->m_ViewSymbolTable;
	}

	ASSERT_VALID(pSymTable);
	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);

		if (bSkipSpecial && pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		if (bSkipTotal && (pF->IsColTotal() || pF->IsSubTotal()))
		{
			continue;
		}

		if (bSkipInput && pF->IsInput())
		{
			continue;
		}

		HTREEITEM htParent = m_htVariables;
		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
		{
			arSpecialGroup.Add(pF);
			continue;
		}
		else if (pF->IsAsk())
			continue;
		else if (pF->IsInput() && pF->IsHidden())
		{
			//arHiddenInputGroup.Add(pF);
			//continue;
			htParent = m_htVariables;
		}
		else if (pF->IsInput())
			continue;
		else if (pF->IsHidden())
			htParent = m_htVariables;
		else if (pF->IsColumn() || pF->IsColTotal() || pF->IsSubTotal())
			continue;
		else
			continue;

		HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htParent, pF, pProgSymTable);

		FillEnumsValue(ht, pF->GetDataType(), GetDocument()->GetWoormFrame()->m_pEditView);
	}

	if (bSort)
	{
		SortChildren(m_htVariables);
	}
	/*
		if (arHiddenInputGroup.GetSize())
		{
			HTREEITEM htHiddenInputGroup = AddNode(_T("Input variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);

			for (int i=0; i < arHiddenInputGroup.GetSize(); i++)
			{
				WoormField* pF = arHiddenInputGroup[i];
				HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htHiddenInputGroup, pF, pProgSymTable);

				FillEnumsValue(ht, pF->GetDataType());
			}

			if (bSort)
			{
				SortChildren(htHiddenInputGroup);
			}
		}
	*/
	if (!bSkipSpecial && arSpecialGroup.GetSize())
	{
		HTREEITEM htSpecialGroup = AddNode(_T("Special variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
		if (arSpecialGroup.GetSize())
		{
			for (int i = 0; i < arSpecialGroup.GetSize(); i++)
			{
				WoormField* pF = arSpecialGroup[i];
				/*HTREEITEM ht = */AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htSpecialGroup, pF, pProgSymTable);
			}

			if (bSort)
			{
				SortChildren(htSpecialGroup);
			}
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillAllVariables(BOOL bSort, BOOL bViewMode, BOOL bSkipSpecial, BOOL bSkipInput, BOOL bSkipTotal, CRSEditView* editView)
{
	if (!m_htVariables)
	{
		m_htVariables = AddNode(_T("Variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES);
	}
	else this->SetItemText(m_htVariables, _T("Variables"));

	if (!CheckAndClearEdge(m_htVariables))
		return FALSE;

	CNodeTree& ndCols = AddNode(_T("Columns"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	SetItemImage(&ndCols, CRSTreeCtrlImgIdx::ColumnGlyph);	//CRSTreeCtrlImgIdx::TableGlyph;
	HTREEITEM htColumnGroup = ndCols;

	CNodeTree& ndFields = AddNode(_T("Fields"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	SetItemImage(&ndFields, CRSTreeCtrlImgIdx::FieldGlyph);
	HTREEITEM htFieldGroup = ndFields;
	/*
		CNodeTree& ndInputFields = AddNode(_T("Input fields"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
		ndInputFields.m_eImgIndex = CRSTreeCtrlImgIdx::FieldGlyph;
		SetItemImage(ndInputFields.m_ht, ndInputFields.m_eImgIndex, ndInputFields.m_eImgIndex);
		HTREEITEM htVisibleInputGroup = ndInputFields;
	*/
	CNodeTree& ndAskFields = AddNode(_T("Request fields"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	SetItemImage(&ndAskFields, CRSTreeCtrlImgIdx::InputAndAskVarGlyph);
	HTREEITEM htAskFieldGroup = ndAskFields;

	m_htHiddenGroupVariables = AddNode(_T("Hidden variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	HTREEITEM htSpecialGroup = bSkipSpecial ? NULL : (HTREEITEM)AddNode(_T("Special variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);

	WoormTable*	pProgSymTable = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable();
	ASSERT_VALID(pProgSymTable);

	WoormTable*	pSymTable = pProgSymTable;

	if (bViewMode)
	{
		m_pWDoc->SyncronizeViewSymbolTable(pProgSymTable);

		pSymTable = &m_pWDoc->m_ViewSymbolTable;
	}

	ASSERT_VALID(pSymTable);

	//CArray<WoormField*, WoormField*> arHiddenInputGroup;

	ASSERT(pSymTable);
	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);

		if (bSkipSpecial && pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		if (bSkipTotal && (pF->IsColTotal() || pF->IsSubTotal()))
		{
			continue;
		}

		if (bSkipInput && pF->IsInput())
		{
			continue;
		}

		HTREEITEM htParent = m_htVariables;
		HTREEITEM htSecondParent = NULL;

		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			htParent = htSpecialGroup;
		else if (pF->IsAsk())
		{
			htParent = htAskFieldGroup;
			if (!pF->IsHidden())
				htSecondParent = htFieldGroup;
		}
		else if (pF->IsInput() && pF->IsHidden())
		{
			//arHiddenInputGroup.Add(pF);
			//continue;
			htParent = m_htHiddenGroupVariables;
		}
		else if (pF->IsInput() && !pF->IsHidden())
		{
			htParent = htFieldGroup;
			//htSecondParent =  htVisibleInputGroup;
		}
		else if (pF->IsHidden())
			htParent = m_htHiddenGroupVariables;
		else if (pF->IsColumn() || pF->IsColTotal() || pF->IsSubTotal())
			htParent = htColumnGroup;
		else
			htParent = htFieldGroup;

		if (!bSkipTotal && (pF->IsColTotal() || pF->IsSubTotal()))
		{
			WoormField* pOwn = pF->GetOwnRepField();
			if (pOwn)
			{
				HTREEITEM ht = FindItemData((DWORD)pOwn, m_htVariables);
				if (ht)
					htParent = ht;
			}
		}

		HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htParent, pF, pSymTable);

		//Fill Intellisense
		if (this->IsKindOf(RUNTIME_CLASS(CRSEditViewTreeCtrl)) && editView)
		{

			editView->GetEditCtrl()->AddIntellisenseWord(pF->GetName(), pF->GetName(), pF->GetName(), L"", L"");
		}
		//

		FillEnumsValue(ht, pF->GetDataType(), editView);

		if (htSecondParent)
		{
			HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htSecondParent, pF, pSymTable);
			FillEnumsValue(ht, pF->GetDataType(), editView);
		}
	}

	if (m_htVariables && bSort)
	{
		SortChildren(htColumnGroup);
		SortChildren(htFieldGroup);
		SortChildren(htAskFieldGroup);
		//SortChildren(htVisibleInputGroup);
		SortChildren(m_htHiddenGroupVariables);
	}
	/*
		if (arHiddenInputGroup.GetSize())
		{
			HTREEITEM htHiddenInputGroup = AddNode(_T("Input variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htHiddenGroupVariables);

			for (int i = 0; i < arHiddenInputGroup.GetSize(); i++)
			{
				WoormField* pF = arHiddenInputGroup[i];

				CNodeTree& ndHiddenInputFields = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htHiddenInputGroup, pF, pSymTable);
				ndHiddenInputFields.m_eImgIndex = CRSTreeCtrlImgIdx::InputVarGlyph;
				SetItemImage(ndHiddenInputFields.m_ht, ndHiddenInputFields.m_eImgIndex, ndHiddenInputFields.m_eImgIndex);
				HTREEITEM ht = ndHiddenInputFields;

				FillEnumsValue(ht, pF->GetDataType());
			}

			if (bSort)
			{
				SortChildren(htHiddenInputGroup);
			}
		}
	*/
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillVariablesGroupingRules(CRSEditView* editView)
{
	if (!m_htVariables)
	{
		m_htVariables = AddNode(_T("Variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES);
	}
	else this->SetItemText(m_htVariables, _T("Variables"));

	if (!CheckAndClearEdge(m_htVariables))
		return FALSE;

	CNodeTree& ndCols = AddNode(_T("Columns"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	SetItemImage(&ndCols, CRSTreeCtrlImgIdx::ColumnGlyph);
	HTREEITEM htColumnGroup = ndCols;

	CNodeTree& ndFields = AddNode(_T("Fields"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	SetItemImage(&ndFields, CRSTreeCtrlImgIdx::FieldGlyph);
	HTREEITEM htFieldGroup = ndFields;

	m_htHiddenGroupVariables = AddNode(_T("Hidden variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);
	HTREEITEM htSpecialGroup = (HTREEITEM)AddNode(_T("Special variables"), CNodeTree::ENodeType::NT_ROOT_VARIABLES, m_htVariables);

	WoormTable*	pSymTable =
		m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable();

	//CArray<WoormField*, WoormField*> arHiddenInputGroup;

	ASSERT(pSymTable);
	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = pSymTable->GetAt(i);
		ASSERT_VALID(pF);

		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			continue;

		if (pF->IsColTotal() || pF->IsSubTotal())
		{
			continue;
		}

		if (pF->IsInput())
		{
			continue;
		}
		if (editView)
		{
			if (!pF->IsExprRuleField() && !pF->IsTableRuleField())
			{
				/*std::pair <std::multimap<CString, IntellisenseData*>::iterator, std::multimap<CString, IntellisenseData>::iterator> ret;

				for (std::multimap<CString, IntellisenseData*>::iterator it = editView->GetEditCtrl()->m_mIntelliString.begin(); it != editView->GetEditCtrl()->m_mIntelliString.end(); ++it)
				{
					if (it->second->m_strItemName.CompareNoCase(pF->GetName()) == 0)
					{
						editView->GetEditCtrl()->m_mIntelliString.erase(it);
						break;
					}
				}
				continue;	*/
			}
			else
			{
				editView->GetEditCtrl()->AddIntellisenseWord(pF->GetName(), pF->GetName(), pF->GetName(), L"", L"");
			}
		}

		HTREEITEM htParent = m_htVariables;
		HTREEITEM htSecondParent = NULL;

		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
			htParent = htSpecialGroup;
		else if (pF->IsAsk())
		{
			continue;
		}
		else if (pF->IsInput() && pF->IsHidden())
		{
			//arHiddenInputGroup.Add(pF);
			//continue;
			htParent = m_htHiddenGroupVariables;
		}
		else if (pF->IsInput() && !pF->IsHidden())
		{
			htParent = htFieldGroup;
			//htSecondParent =  htVisibleInputGroup;
		}
		else if (pF->IsHidden())
			htParent = m_htHiddenGroupVariables;
		else if (pF->IsColumn() || pF->IsColTotal() || pF->IsSubTotal())
			htParent = htColumnGroup;
		else
			htParent = htFieldGroup;

		HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htParent, pF, pSymTable);
		FillEnumsValue(ht, pF->GetDataType(), GetDocument()->GetWoormFrame()->m_pEditView);

		if (htSecondParent)
		{
			HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htSecondParent, pF, pSymTable);
			FillEnumsValue(ht, pF->GetDataType(), GetDocument()->GetWoormFrame()->m_pEditView);
		}
	}

	if (m_htVariables)
	{
		SortChildren(htColumnGroup);
		SortChildren(htFieldGroup);
		//SortChildren(htVisibleInputGroup);
		SortChildren(m_htHiddenGroupVariables);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillDialogs()
{
	AskRuleData* pAskDialogs = m_pWDoc->m_pEditorManager->GetPrgData()->GetAskRuleData();
	ASSERT_VALID(pAskDialogs);

	if (!m_htAskDialogs)
		m_htAskDialogs = AddNode(_T("Dialogs"), CNodeTree::ENodeType::NT_ROOT_DIALOGS, NULL, pAskDialogs, NULL);

	if (!CheckAndClearEdge(m_htAskDialogs))
		return FALSE;

	for (int k = 0; k < pAskDialogs->GetCount(); k++)
	{
		AskDialogData*	pAskDlg = pAskDialogs->GetAskDialog(k);
		ASSERT_VALID(pAskDlg);

		HTREEITEM htDialog = AddNode(pAskDlg->GetName(), CNodeTree::ENodeType::NT_ASKDIALOG, m_htAskDialogs, pAskDlg, pAskDialogs);

		if (!pAskDlg->IsOnAsk())
		{
			AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pBeforeBlock, FALSE);
			AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Show when")/*+ L"..."*/, htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pWhenExpr, DataType::Bool, FALSE);
		}

		HTREEITEM htControls = AddNode(_T("Controls"), CNodeTree::ENodeType::NT_ASKCONTROLS, htDialog, pAskDlg->GetAskControls(), pAskDlg);
		for (int i = 0; i < pAskDlg->GetAskGroupSize(); i++)
		{
			AskGroupData* pAskGroup = pAskDlg->GetAskGroup(i);
			ASSERT_VALID(pAskGroup);

			CString sGroupTitle = pAskGroup->m_strTitle;
			if (sGroupTitle.CompareNoCase(AskGroupData::GetEmptyTitle()) == 0)
				sGroupTitle = '<' + sGroupTitle + '>';
			else
				sGroupTitle = '"' + sGroupTitle + '"';

			if (pAskGroup->m_bHiddenTitle)
				sGroupTitle = '[' + sGroupTitle + ']';

			HTREEITEM htGroup = AddNode(sGroupTitle, CNodeTree::ENodeType::NT_ASKGROUP, htControls, pAskGroup, pAskDlg);

			for (int j = 0; j < pAskGroup->GetAskFieldSize(); j++)
			{
				AskFieldData* pAskField = pAskGroup->GetAskField(j);
				ASSERT_VALID(pAskField);

				CString sTitle = pAskField->GetCaption();
				if (sTitle.IsEmpty())
					sTitle = '<' + _TB("Field Without Caption") + '>';
				else
					sTitle = '"' + sTitle + '"';

				HTREEITEM htAskField = AddNode(sTitle, CNodeTree::ENodeType::NT_ASKFIELD, htGroup, pAskField, pAskGroup);

				//---- PROVA - il field appare anche nel gruppo delle variables
				ASSERT_VALID(m_pWDoc->m_pEditorManager);
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData());
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable());

				WoormField* pField = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable()->GetField(pAskField->GetPublicName());
				ASSERT_VALID(pField);

				HTREEITEM htField = AddNode(pField->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htAskField, pField, m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable());
				//----
			}
		}

		if (!pAskDlg->IsOnAsk())
		{
			HTREEITEM htOn = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Check input")/*+ L"..."*/, htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pOnExpr, DataType::Bool, FALSE);
			AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Abort message")/*+ L"..."*/, htOn, pAskDlg->GetSymTable(), &pAskDlg->m_pAbortExpr, DataType::String, FALSE);

			AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pAfterBlock, FALSE);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillDialogsForDebug(Block* pCurrent)
{
	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	AskRuleData* pAskDialogs = &pEngine->m_AskingRules;
	ASSERT_VALID(pAskDialogs);

	if (!m_htAskDialogs)
		m_htAskDialogs = AddNode(_T("Dialogs"), CNodeTree::ENodeType::NT_ROOT_DIALOGS, NULL, pAskDialogs, NULL);

	if (!CheckAndClearEdge(m_htAskDialogs))
		return FALSE;

	HTREEITEM htDialogCurrent = NULL;
	for (int k = 0; k < pAskDialogs->GetCount(); k++)
	{
		AskDialogData*	pAskDlg = pAskDialogs->GetAskDialog(k);
		ASSERT_VALID(pAskDlg);

		HTREEITEM htDialog = AddNode(pAskDlg->GetName(), CNodeTree::ENodeType::NT_ASKDIALOG, m_htAskDialogs, pAskDlg, pAskDialogs);

		if (!pAskDlg->IsOnAsk())
		{
			AddNode(pCurrent && pAskDlg->m_pBeforeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph, _T("Before"), htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pBeforeBlock, FALSE);
			//AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Show when"),	htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pWhenExpr, DataType::Bool, FALSE);

			AddNode(pCurrent && pAskDlg->m_pAfterBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph, _T("After"), htDialog, pAskDlg->GetSymTable(), &pAskDlg->m_pAfterBlock, FALSE);

			if ((pCurrent && pAskDlg->m_pBeforeBlock == pCurrent) || (pCurrent && pAskDlg->m_pAfterBlock == pCurrent))
			{
				htDialogCurrent = htDialog;
			}
		}
	}
	if (htDialogCurrent)
	{
		Expand(m_htAskDialogs, TVE_EXPAND);
		Expand(htDialogCurrent, TVE_EXPAND);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillLink(HTREEITEM htLink, WoormLink*	pLink)
{
	/*HTREEITEM htWhen = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Enable when")/*+ L"..."*/, htLink, pLink->m_pViewSymbolTable, &pLink->m_pEnableLinkWhenExpr, DataType::Bool, TRUE);

	if (pLink->m_pLocalSymbolTable && pLink->m_pLocalSymbolTable->GetCount())
	{
		HTREEITEM htParameters = AddNode(_T("Link parameters"), CNodeTree::ENodeType::NT_LINK_PARAMETERS, htLink, pLink);

		for (int i = 0; i < pLink->m_pLocalSymbolTable->GetCount(); i++)
		{
			WoormField* pF = pLink->m_pLocalSymbolTable->GetAt(i);
			if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
				continue;

			/*HTREEITEM htParam = */AddNode(pF->GetName(), CNodeTree::ENodeType::NT_LINK_PARAM, htParameters, pF, pLink);
		}
	}

	/*HTREEITEM htBefore = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, htLink, pLink->m_pLocalSymbolTable, &pLink->m_pBeforeLink, FALSE);
	/*HTREEITEM htAfter = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, htLink, pLink->m_pLocalSymbolTable, &pLink->m_pAfterLink, FALSE);

	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillLinks()
{
	if (!m_htLinks)
		m_htLinks = AddNode(_T("Hyperlinks"), CNodeTree::ENodeType::NT_ROOT_LINKS);

	if (!CheckAndClearEdge(m_htLinks))
		return FALSE;

	for (int k = 0; k < m_pWDoc->m_arWoormLinks.GetCount(); k++)
	{
		WoormLink*	pLink = m_pWDoc->m_arWoormLinks.GetAt(k);
		ASSERT_VALID(pLink);
		if (pLink->m_LinkType == WoormLink::WoormLinkType::ConnectionRadar) continue;

		HTREEITEM htLink = AddNode(pLink->m_strLinkOwner, CNodeTree::ENodeType::NT_LINK, m_htLinks, pLink, &(m_pWDoc->m_arWoormLinks));

		FillLink(htLink, pLink);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::UpdateTriggerEvent(HTREEITEM htTrigger, EventsData* pEventsData, TriggEventData* pTriggerEvent)
{
	ASSERT_VALID(pEventsData);
	ASSERT_VALID(pTriggerEvent);

	RemoveTreeChilds(htTrigger);

	HTREEITEM htFL = AddNode(_T("Fields list"), CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST, htTrigger, pTriggerEvent, pEventsData);
	CStringArray arFields;
	pTriggerEvent->GetBreakingFields(arFields);
	for (int i = 0; i < arFields.GetSize(); i++)
	{
		WoormField* pF = pTriggerEvent->m_SymTable.GetField(arFields[i]);
		if (!pF)
			continue;
		/*HTREEITEM ht = */AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htFL, pF, &pTriggerEvent->m_SymTable);
	}

	/*HTREEITEM htW = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Breaking when")/*+ L"..."*/, htTrigger, &pEventsData->m_SymTable, &pTriggerEvent->m_pWhenExpr, DataType::Bool, FALSE);

	HTREEITEM htST = AddNode(_T("Sub totals"), CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST, htTrigger, pTriggerEvent, pEventsData);


	arFields.RemoveAll();
	pTriggerEvent->GetSubtotalFields(arFields);
	for (int i = 0; i < arFields.GetSize(); i++)
	{
		WoormField* pF = pTriggerEvent->m_SymTable.GetField(arFields[i]);
		if (!pF)
			continue;
		/*HTREEITEM ht = */AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htST, pF, &pTriggerEvent->m_SymTable);
	}

	/*HTREEITEM htB = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, htTrigger, &pEventsData->m_SymTable, &pTriggerEvent->m_pBeforeBlock, TRUE);
	/*HTREEITEM htA = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, htTrigger, &pEventsData->m_SymTable, &pTriggerEvent->m_pAfterBlock, TRUE);
}

//-----------------------------------------------------------------------------
CNodeTree& CRSTreeCtrl::FillTriggerEvent(EventsData* pEventsData, TriggEventData* pTriggerEvent, BOOL bSelect /*= FALSE*/)
{
	ASSERT_VALID(pEventsData);
	ASSERT_VALID(pTriggerEvent);

	CNodeTree& node = AddNode(pTriggerEvent->m_strEventName /*+ L"..."*/, CNodeTree::ENodeType::NT_TRIGGER_EVENT, m_htTriggerEvents, pTriggerEvent, pEventsData);
	HTREEITEM htTrigger = node;

	UpdateTriggerEvent(htTrigger, pEventsData, pTriggerEvent);

	if (bSelect)
	{
		SelectItem(node.m_ht);
	}
	return node;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillEvents()
{
	EventsData* pEventsData = m_pWDoc->m_pEditorManager->GetPrgData()->GetEventsData();
	ASSERT_VALID(pEventsData);

	if (!pEventsData->m_pReportActions)
		pEventsData->m_pReportActions = new ReportEventData(*m_pWDoc->GetEngineSymTable());
	if (!pEventsData->m_pNewPageActions)
		pEventsData->m_pNewPageActions = new NewPageActionData(*m_pWDoc->GetEngineSymTable());

	if (m_htEvents == NULL)
	{
		m_htEvents = AddNode(_T("Events"), CNodeTree::ENodeType::NT_ROOT_EVENTS);
		m_htReportEvents = AddNode(_T("Report Events"), CNodeTree::ENodeType::NT_SUBROOT_REPORT_EVENTS, m_htEvents, pEventsData->m_pReportActions, pEventsData);
		m_htFormFeedEvents = AddNode(_T("FormFeed Events"), CNodeTree::ENodeType::NT_SUBROOT_FORMFEED_EVENTS, m_htEvents, pEventsData->m_pNewPageActions, pEventsData);
		m_htFillTableEvents = AddNode(_T("Fill Table Events"), CNodeTree::ENodeType::NT_SUBROOT_FILLTABLE_EVENTS, m_htEvents, pEventsData);
		m_htTriggerEvents = AddNode(_T("Breaking Events"), CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS, m_htEvents, pEventsData);
	}

	//---- Report
	if (!CheckAndClearEdge(m_htReportEvents))
		return FALSE;
	ASSERT_VALID(pEventsData->m_pReportActions);

	m_htReportBeforeEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, m_htReportEvents, &pEventsData->m_SymTable, &(pEventsData->m_pReportActions->m_pBeforeBlock), TRUE);

	m_htReportAlwaysEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Always")/*+ L"..."*/, m_htReportEvents, &pEventsData->m_SymTable, &pEventsData->m_pReportActions->m_pAlwaysBlock, TRUE);

	m_htReportAfterEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, m_htReportEvents, &pEventsData->m_SymTable, &pEventsData->m_pReportActions->m_pAfterBlock, TRUE);

	m_htReportFinalizeEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Finalize")/*+ L"..."*/, m_htReportEvents, &pEventsData->m_SymTable, &pEventsData->m_pReportActions->m_pFinalizeBlock, TRUE);

	//---- New page (FormFeed)
	if (!CheckAndClearEdge(m_htFormFeedEvents))
		return FALSE;
	ASSERT_VALID(pEventsData->m_pNewPageActions);

	m_htFormFeedBeforeEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, m_htFormFeedEvents, &pEventsData->m_SymTable, &pEventsData->m_pNewPageActions->m_pBeforeBlock, TRUE);
	m_htFormFeedAfterEvent = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, m_htFormFeedEvents, &pEventsData->m_SymTable, &pEventsData->m_pNewPageActions->m_pAfterBlock, TRUE);

	//---- Fill Table 
	if (!CheckAndClearEdge(m_htFillTableEvents))
		return FALSE;

	//può essere vuoto
	//for (int i = 0; i < pEventsData->m_TableActions.GetSize(); i++)
	//{
	//	TableActionData* pTableEvent = dynamic_cast<TableActionData*>(pEventsData->m_TableActions.GetAt(i));
	//	ASSERT_VALID(pTableEvent);
	//	if (!pTableEvent) continue;

	//	HTREEITEM htFillTable = AddNode(Img::None, _T("Fill table") + ' ' + pTableEvent->GetDisplayTableName(), CNodeTree::ENodeType::NT_FILLTABLE_EVENT, m_htFillTableEvents, pTableEvent);

	//	AddNode(Img::None, _T("Before..."), htFillTable, &pEventsData->m_SymTable, &pTableEvent->m_pBeforeBlock, TRUE);
	//	AddNode(Img::None, _T("After..."), htFillTable, &pEventsData->m_SymTable, &pTableEvent->m_pAfterBlock, TRUE);
	//}

	ASSERT_VALID(pEventsData->m_SymTable.GetDisplayTables());
	Array* parDT = &(pEventsData->m_SymTable.GetDisplayTables()->m_DispTables);
	for (int i = 0; i < parDT->GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = dynamic_cast<DisplayTableEntry*>((*parDT)[i]);
		ASSERT_VALID(pDispTblObject);

		CString descr = CString(_T("Fill table")) + ' ' + pDispTblObject->m_dsName.GetString();

		HTREEITEM htFillTable = AddNode(descr, CNodeTree::ENodeType::NT_FILLTABLE_EVENT, m_htFillTableEvents);

		TableActionData* pTableEvent = pEventsData->GetTableEvent(pDispTblObject->m_dsName.GetString());
		if (!pTableEvent)
		{
			pTableEvent = new TableActionData(pEventsData->m_SymTable);
			pTableEvent->SetDisplayTableName(pDispTblObject->m_dsName.GetString());
			pEventsData->m_TableActions.Add(pTableEvent);
		}

		AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Before")/*+ L"..."*/, htFillTable, &pEventsData->m_SymTable, &pTableEvent->m_pBeforeBlock, TRUE);
		AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("After")/*+ L"..."*/, htFillTable, &pEventsData->m_SymTable, &pTableEvent->m_pAfterBlock, TRUE);
	}
	ASSERT(pEventsData->m_TableActions.GetSize() == parDT->GetSize());

	//---- Trigger - Breacking 
	if (!CheckAndClearEdge(m_htTriggerEvents))
		return FALSE;

	for (int i = 0; i < pEventsData->m_TriggEvents.GetSize(); i++)
	{
		TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pEventsData->m_TriggEvents.GetAt(i));
		ASSERT_VALID(pTriggerEvent);
		if (!pTriggerEvent) continue;

		FillTriggerEvent(pEventsData, pTriggerEvent);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillEventsForDebug(Block* pCurrent)
{
	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	if (m_htEvents == NULL)
	{
		m_htEvents = AddNode(_T("Events"), CNodeTree::ENodeType::NT_ROOT_EVENTS);
	}
	else
		if (!CheckAndClearEdge(m_htEvents))
			return FALSE;

	ReportEvents*		pReportEvents = pEngine->m_pReportEvents;
	ASSERT_VALID(pReportEvents);
	if (!pReportEvents)
		return FALSE;

	FormFeedEvents*		pOnFormFeedEvents = pEngine->m_pOnFormFeedEvents;
	ASSERT_VALID(pOnFormFeedEvents);

	CNodeTree& nt1 = AddNode(
		(pReportEvents->m_pBeforeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph),
		pReportEvents->m_pBeforeBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&pReportEvents->m_pBeforeBlock, TRUE);
	//if (pReportActions->m_pBeforeBlock->m_bHasBreakpoint) nt1.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htReportBeforeEvent = nt1;

	CNodeTree& nt2 = AddNode(
		(pOnFormFeedEvents->m_pAfterBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph),
		pOnFormFeedEvents->m_pAfterBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&pOnFormFeedEvents->m_pAfterBlock, TRUE);
	//if (pOnFormFeedActions->m_pAfterBlock->m_bHasBreakpoint) nt2.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htFormFeedAfterEvent = nt2;

	for (int i = 0; i < pEngine->m_SymTable.GetDisplayTablesNum(); i++)
	{
		ASSERT_VALID(pEngine->m_SymTable.GetDisplayTables());
		DisplayTableEntryEngine* pDisplayTable = dynamic_cast<DisplayTableEntryEngine*>(pEngine->m_SymTable.GetDisplayTables()->GetAt(i));
		ASSERT_VALID(pDisplayTable);
		if (!pDisplayTable)
			continue;

		CNodeTree& nt3 = AddNode(
			(pDisplayTable->TableActions()->m_pAfterBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph),
			pDisplayTable->TableActions()->m_pAfterBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
			&pDisplayTable->TableActions()->m_pAfterBlock, TRUE);
		//if (pDisplayTable->TableActions()->m_pAfterBlock->m_bHasBreakpoint) nt3.SetItemColor(RS_COLOR_BREAKPOINT);
	}

	for (int i = 0; i < pEngine->m_TriggeredEvents.GetSize(); i++)
	{
		TriggeredEvent* pBreakingEvent = dynamic_cast<TriggeredEvent*>(pEngine->m_TriggeredEvents.GetAt(i));
		ASSERT_VALID(pBreakingEvent);
		if (!pBreakingEvent)
			continue;

		CNodeTree& nt4 = AddNode(
			pBreakingEvent->m_pAfterBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
			pBreakingEvent->m_pAfterBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
			&pBreakingEvent->m_pAfterBlock, TRUE);
		//if (pBreakingEvent->m_pAfterBlock->m_bHasBreakpoint) nt4.SetItemColor(RS_COLOR_BREAKPOINT);
	}

	CNodeTree& nt5 = AddNode(
		pReportEvents->m_pAlwaysBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
		pReportEvents->m_pAlwaysBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&(pReportEvents->m_pAlwaysBlock), TRUE);
	//if (pReportActions->m_pAlwaysBlock->m_bHasBreakpoint) nt5.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htReportAlwaysEvent = nt5;

	for (int i = 0; i < pEngine->m_TriggeredEvents.GetSize(); i++)
	{
		TriggeredEvent* pBreakingEvent = dynamic_cast<TriggeredEvent*>(pEngine->m_TriggeredEvents.GetAt(i));
		if (!pBreakingEvent)
			continue;

		CNodeTree& nt6 = AddNode(
			pBreakingEvent->m_pBeforeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
			pBreakingEvent->m_pBeforeBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
			&pBreakingEvent->m_pBeforeBlock, TRUE);
		//if (pBreakingEvent->m_pBeforeBlock->m_bHasBreakpoint) nt6.SetItemColor(RS_COLOR_BREAKPOINT);
	}

	for (int i = 0; i < pEngine->m_SymTable.GetDisplayTablesNum(); i++)
	{
		DisplayTableEntryEngine* pDisplayTable = dynamic_cast<DisplayTableEntryEngine*>(pEngine->m_SymTable.GetDisplayTables()->GetAt(i));
		if (!pDisplayTable)
			continue;

		CNodeTree& nt7 = AddNode(
			pDisplayTable->TableActions()->m_pBeforeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
			pDisplayTable->TableActions()->m_pBeforeBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
			&pDisplayTable->TableActions()->m_pBeforeBlock, TRUE);
		//if (pDisplayTable->TableActions()->m_pBeforeBlock->m_bHasBreakpoint) nt7.SetItemColor(RS_COLOR_BREAKPOINT);
	}

	CNodeTree& nt8 = AddNode(
		pOnFormFeedEvents->m_pBeforeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
		pOnFormFeedEvents->m_pBeforeBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&pOnFormFeedEvents->m_pBeforeBlock, TRUE);
	//if (pOnFormFeedActions->m_pBeforeBlock->m_bHasBreakpoint) nt8.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htFormFeedBeforeEvent = nt8;

	CNodeTree& nt9 = AddNode(
		pReportEvents->m_pAfterBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
		pReportEvents->m_pAfterBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&(pReportEvents->m_pAfterBlock), TRUE);
	//if (pReportActions->m_pAfterBlock->m_bHasBreakpoint) nt9.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htReportAfterEvent = nt9;

	CNodeTree& nt10 = AddNode(
		pReportEvents->m_pFinalizeBlock == pCurrent ? CRSTreeCtrlImgIdx::BreakPointCurrent : CRSTreeCtrlImgIdx::NoGLyph,
		pReportEvents->m_pFinalizeBlock->m_strOwnerName, m_htEvents, &pEngine->m_SymTable,
		&pReportEvents->m_pFinalizeBlock, TRUE);
	//if (pReportActions->m_pFinalizeBlock->m_bHasBreakpoint) nt10.SetItemColor(RS_COLOR_BREAKPOINT);
	m_htReportFinalizeEvent = nt10;

	/* TODO
	RepEngineStatus		m_EngineStatus;
	FormFeedAction*		m_pAutoFormFeed;
	*/
	Expand(m_htEvents, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillBreakpoints(ActionObj* pCurrentAct)
{
	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	if (m_htBreakpoints == NULL)
	{
		m_htBreakpoints = AddNode(_T("Breakpoints"), CNodeTree::ENodeType::NT_ROOT_BREAKPOINTS, NULL, &pEngine->m_arBreakpoints, pEngine);
	}
	else
		if (!CheckAndClearEdge(m_htBreakpoints))
			return FALSE;

	for (int i = 0; i < pEngine->m_arBreakpoints.GetCount(); i++)
	{
		CBreakpoint* pB = pEngine->m_arBreakpoints.GetAt(i);
		if (!pB) continue;
		ASSERT_VALID(pB);
		ASSERT_VALID(pB->m_pAction);
		if (!pB->m_pAction) continue;
		if (pB->m_bStepOverBreakpoint) continue;

		Block* pBlock = pB->m_pAction->GetBlockParent();
		if (!pBlock) continue;

		CString sTitle = pBlock->m_strOwnerName + ::cwsprintf(L" - line: %d", pB->m_pAction->m_nDebugUnparseRow - pBlock->m_nDebugUnparseRow + 1);

		CNodeTree& nd = AddNode(sTitle, CNodeTree::ENodeType::NT_BREAKPOINT_ACTION, m_htBreakpoints, pB->m_pAction, pBlock);
		if (pCurrentAct == pB->m_pAction)
		{
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPointCurrent);
		}
		else if (!pB->m_bEnabled)
		{
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPointDisabled);
		}
		else if (pB->m_erprCondition && !pB->m_erprCondition->IsEmpty() &&
			pB->m_erprAction && !pB->m_erprAction->IsEmpty()
			)
		{
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPointConditionAction);
		}
		else if (pB->m_erprCondition && !pB->m_erprCondition->IsEmpty())
		{
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPointCondition);
		}
		else if (pB->m_erprAction && !pB->m_erprAction->IsEmpty())
		{
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPointAction);
		}
		else
			SetItemImage(&nd, CRSTreeCtrlImgIdx::BreakPoint);
	}

	if (ItemHasChildren(m_htBreakpoints))
	{
		//SetItemImage(m_htBreakpoints, CRSTreeCtrlImgIdx::BreakPoint);
		SortChildren(m_htBreakpoints);
		Expand(m_htBreakpoints, TVE_EXPAND);
	}
	//else
	//	SetItemImage(m_htBreakpoints, CRSTreeCtrlImgIdx::NoGLyph);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillTupleRules()
{
	QueryData* pQueryData = m_pWDoc->m_pEditorManager->GetPrgData()->GetQueryData();
	ASSERT_VALID(pQueryData);

	if (!m_htTupleRules)
		m_htTupleRules = AddNode(_T("Tuple Rules") /*+ L"..."*/, CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES, NULL, pQueryData->m_pGroupBy, pQueryData);

	if (!CheckAndClearEdge(m_htTupleRules))
		return FALSE;
	//----
	ASSERT_VALID(pQueryData->m_pGroupBy);
	ASSERT_VALID(pQueryData->m_pGroupBy->m_pTupleFilter);
	ASSERT_VALID(pQueryData->m_pGroupBy->m_pGroupingTuple);
	ASSERT_VALID(pQueryData->m_pGroupBy->m_pHavingTupleFilter);

	if (m_pWDoc->m_bBetaFeatures)
	{
		CNodeTree& nodeWhere = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Filter"), m_htTupleRules, &pQueryData->m_SymTable, &pQueryData->m_pGroupBy->m_pTupleFilter, DataType::Bool, FALSE);
		nodeWhere.m_NodeType = CNodeTree::ENodeType::NT_TUPLE_FILTER;
	}

	CNodeTree& nodeBy = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Grouping"), m_htTupleRules, &pQueryData->m_SymTable, ((Expression**)&pQueryData->m_pGroupBy->m_pGroupingTuple), DataType::String, FALSE);
	nodeBy.m_NodeType = CNodeTree::ENodeType::NT_TUPLE_GROUPING;

	CNodeTree& nodeDo = AddNode(_T("Do") /*+ L"..."*/, CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS, nodeBy.m_ht, pQueryData->m_pGroupBy/*->m_ActionsArray*/, pQueryData);

	CNodeTree& nodeHaving = AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("Having"), m_htTupleRules, &pQueryData->m_SymTable, &pQueryData->m_pGroupBy->m_pHavingTupleFilter, DataType::Bool, FALSE);
	nodeHaving.m_NodeType = CNodeTree::ENodeType::NT_TUPLE_HAVING_FILTER;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::FillTblRule(HTREEITEM htRule, TblRuleData* pTblRule)
{
	WoormTable*	pSymTable = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable();

	//----
	ASSERT(pTblRule->m_arSqlTableJoinInfoArray.GetSize() == pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize());

	HTREEITEM htFrom = AddNode(L"From", CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM, htRule, pTblRule);

	for (int i = 0; i < pTblRule->m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		const SqlTableInfo* pTableInfo = pTblRule->m_arSqlTableJoinInfoArray.GetAt(i);

		HTREEITEM htTable = AddNode(pTableInfo->GetTableName(), CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO, htFrom, const_cast<SqlTableInfo*>(pTableInfo), pTblRule);

		if (i > 0)
		{
			//la prima tabella non ha attributi di join
			WClause* pWJoinOn = pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[i];
			if (pWJoinOn)
			{
				ASSERT_VALID(pWJoinOn);
				/*HTREEITEM htOn = */AddNode(L"On"/*+ L"..."*/, CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON, htTable, pWJoinOn, pTblRule);
			}
		}

		{
			HTREEITEM htColumns = AddNode(_T("Columns"), CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS, htTable, const_cast<SqlTableInfo*>(pTableInfo), pTblRule);

			DataFieldLinkArray* arLinks = pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(i);

			for (int j = 0; j < arLinks->GetSize(); j++)
			{
				DataFieldLink*  pObjLink = arLinks->GetAt(j);

				CString sColumnName = pObjLink->m_strPhysicalName;
				int idx = sColumnName.Find('.');
				if (idx > -1)
					sColumnName = sColumnName.Mid(idx + 1);

				WoormField* pF = pSymTable->GetField(pObjLink->m_strPublicName);
				//ASSERT_VALID(pF);
				/* if (!pF)
				sColumn += pObjLink->m_strPhysicalName + L" Into " + pObjLink->m_strPublicName +_TB(" : UNKNOWN FIELD!");
				HTREEITEM htCol = AddNode(sColumn, CNodeTree::ENodeType::NT_RULE_QUERY_COLUMN, htColumns, pObjLink, pTblRule);*/

				if (pF)
					AddNode(/*pF->GetName()*/sColumnName /*+ L" Into " + pObjLink->m_strPublicName*/,
						CNodeTree::ENodeType::NT_VARIABLE, htColumns, pF, pSymTable);
			}
			{
				HTREEITEM htUnselectedColumns = AddNode(_T("Unselected Columns"), CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN, htTable,
					const_cast<SqlTableInfo*>(pTableInfo), pTblRule);
				AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htUnselectedColumns);
			}
			//if (m_bShowRelatedTables)
			{
				ASSERT_VALID(pTableInfo->GetSqlCatalogEntry());
				if (!pTableInfo->GetSqlCatalogEntry())
					continue;

				if (pTableInfo->GetSqlCatalogEntry()->m_nType != TABLE_TYPE)
					continue;

				CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
				ASSERT_VALID(pHelperSqlCatalog);

				CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pTableInfo->GetSqlCatalogEntry());
				ASSERT_VALID(pTC);
				if (!pTC)
					continue;

				HTREEITEM htFKTable = AddNode(_T("Foreign Key"), CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY, htTable,
					pTC, pTblRule);
				AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htFKTable);

				HTREEITEM htExtRefTable = AddNode(_T("External References"), CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES, htTable,
					pTC, pTblRule);
				AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htExtRefTable);
			}
		}
	}

	//----
	HTREEITEM htCalcColumns = AddNode(L"Calculated Columns", CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS, htRule, pTblRule, pTblRule);	//necessario due volte per uniformità con il gruppo colonne
	for (int i = 0; i < pTblRule->m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink = pTblRule->m_CalcColumnLinks.GetAt(i);

		WoormField* pF = pSymTable->GetField(pObjLink->m_strPublicName);
		ASSERT_VALID(pF);
		if (pF)
		{
			CNodeTree& nt = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htCalcColumns, pF, pSymTable, pTblRule);
		}
	}

	if (pTblRule->m_pWhereClause == NULL)
	{
		pTblRule->AddWhereClause();
	}
	/*HTREEITEM htWhere = */AddNode(L"Where"/*+ L"..."*/, CNodeTree::ENodeType::NT_RULE_QUERY_WHERE, htRule, pTblRule->m_pWhereClause, pTblRule);

	/*HTREEITEM htGroupBy = */AddNode(L"Group by"/*+ L"..."*/, CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY, htRule, pTblRule);

	if (pTblRule->m_pHavingClause == NULL)
	{
		pTblRule->AddHavingClause();
	}
	/*HTREEITEM htHaving = */AddNode(L"Having"/*+ L"..."*/, CNodeTree::ENodeType::NT_RULE_QUERY_HAVING, htRule, pTblRule->m_pHavingClause, pTblRule);

	/*HTREEITEM htOrderBy = */AddNode(L"Order by"/*+ L"..."*/, CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY, htRule, pTblRule);

	/*HTREEITEM htWhen = */AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("When")/*+ L"..."*/, htRule, pTblRule->GetSymTable(), &pTblRule->m_pWhenExpr, DataType::Bool, FALSE);
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::FillLoopRule(HTREEITEM htParent, WhileRuleData* pRule)
{
	AddNode(CRSTreeCtrlImgIdx::NoGLyph, L"Init", htParent, pRule->GetSymTable(), &(pRule->m_pBefore), FALSE);

	AddNode(CRSTreeCtrlImgIdx::NoGLyph, _T("While"), htParent, pRule->GetSymTable(), &pRule->m_pCondExpr, DataType::Bool, FALSE);
	AddNode(CRSTreeCtrlImgIdx::NoGLyph, L"Do", htParent, pRule->GetSymTable(), &(pRule->m_pBody), FALSE);

	AddNode(CRSTreeCtrlImgIdx::NoGLyph, L"Repeat", htParent, pRule->GetSymTable(), &(pRule->m_pAfter), FALSE);
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::FillChildNamedQuery(HTREEITEM htParent, QueryObjItem* pQueryItem, QueryRuleData* pQueryRule/*=NULL*/)
{
	ASSERT_VALID(pQueryItem);
	WoormTable*	pSymTable = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable();
	ASSERT_VALID(pSymTable);

	QueryObject* pQuery = pQueryItem->GetQueryObject();
	ASSERT_VALID(pQuery);

	{
		const CStringArray& arColumns = pQuery->AllQueryColumns();
		HTREEITEM htSelect = AddNode(L"Select", CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_SELECT, htParent, pQueryItem, pQueryRule);
		for (int i = 0; i < arColumns.GetSize(); i++)
		{
			CString sName = arColumns.GetAt(i);
			WoormField* pF = pSymTable->GetField(sName);
			ASSERT_VALID(pF);
			if (!pF) continue;

			HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htSelect, pF, pSymTable);
		}
	}
	{
		const CStringArray& arParameters = pQuery->AllQueryParameters();
		HTREEITEM htParameters = AddNode(L"Parameters", CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_PARAMETERS, htParent, pQueryItem, pQueryRule);
		for (int i = 0; i < arParameters.GetSize(); i++)
		{
			CString sName = arParameters.GetAt(i);
			WoormField* pF = pSymTable->GetField(sName);
			ASSERT_VALID(pF);
			if (!pF) continue;

			HTREEITEM ht = AddNode(pF->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htParameters, pF, pSymTable);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillRules()
{
	if (!m_htRules)
		m_htRules = AddNode(_T("Rules"), CNodeTree::ENodeType::NT_ROOT_RULES);

	if (!CheckAndClearEdge(m_htRules))
		return FALSE;

	RuleDataArray* pRules = m_pWDoc->m_pEditorManager->GetPrgData()->GetRuleData();

	for (int r = 0; r < pRules->GetSize(); r++)
	{
		RuleDataObj* pR = dynamic_cast<RuleDataObj*>(pRules->GetAt(r));
		ASSERT_VALID(pR);

		CString sDescr = pR->GetRuleDescription();

		HTREEITEM htRule = NULL;

		CNodeTree::ENodeType nType = CNodeTree::ENodeType::NT_WRONG;
		switch (pR->IsARule())
		{
		case RULE_DATA_TABLE:
			nType = CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE;

			sDescr = sDescr /*+ L"..."*/;
			break;

		case RULE_EXPR:
		case RULE_COND_EXPR:
		{
			ExpRuleData* pRE = dynamic_cast<ExpRuleData*>(pR);
			ASSERT_VALID(pRE);
			WoormField* pF = pRE->GetSymTable()->GetField(pRE->GetPublicName());
			ASSERT_VALID(pF);
			if (!pF) continue;

			htRule = AddNode(sDescr, CNodeTree::ENodeType::NT_VARIABLE, m_htRules, pF, pRE->GetSymTable(), pRE);
			break;
		}

		case RULE_NAMED_QUERY:
			nType = CNodeTree::ENodeType::NT_RULE_NAMED_QUERY;
			sDescr = pR->GetRuleDescription() /*+ L"..."*/;
			break;

		case RULE_LOOP_WHILE:
			nType = CNodeTree::ENodeType::NT_RULE_LOOP;
			sDescr = pR->GetRuleDescription() /*+ L"..."*/;
			break;
		default:
			continue;
		}

		if (!htRule)
			htRule = AddNode(sDescr, nType, m_htRules, pR, pRules);

		if (pR->IsARule() == RULE_DATA_TABLE)
		{
			TblRuleData* pRule = dynamic_cast<TblRuleData*>(pR);

			FillTblRule(htRule, pRule);
		}
		else if (pR->IsARule() == RULE_NAMED_QUERY)
		{
			QueryRuleData* pRule = dynamic_cast<QueryRuleData*>(pR);

			FillChildNamedQuery(htRule, pRule->GetQueryItem(), pRule);
		}
		else if (pR->IsARule() == RULE_LOOP_WHILE)
		{
			WhileRuleData* pRule = dynamic_cast<WhileRuleData*>(pR);

			FillLoopRule(htRule, pRule);
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillHiddenBlockForDebug(HTREEITEM htParent, Block** ppBefore, Block** ppAfter, SymTable* pSymTable, Block* pCurrent)
{
	ASSERT_VALID(pSymTable);
	BOOL expand = FALSE;

	ASSERT(ppBefore);
	if (*ppBefore)
	{
		ASSERT_VALID(*ppBefore);
		CNodeTree& ndBefore = AddNode(CRSTreeCtrlImgIdx::NoGLyph, L"Before", htParent, pSymTable, ppBefore, FALSE);

		if (*ppBefore == pCurrent)
		{
			SetItemImage(&ndBefore, CRSTreeCtrlImgIdx::BreakPointCurrent);
			expand = TRUE;
		}
		else if ((*ppBefore)->HasBreakpoint())
			SetItemImage(&ndBefore, CRSTreeCtrlImgIdx::BreakPoint);
	}

	ASSERT(ppAfter);
	if (*ppAfter)
	{
		ASSERT_VALID(*ppAfter);
		CNodeTree& ndAfter = AddNode(CRSTreeCtrlImgIdx::NoGLyph, L"After", htParent, pSymTable, ppAfter, FALSE);

		if (*ppAfter == pCurrent)
		{
			SetItemImage(&ndAfter, CRSTreeCtrlImgIdx::BreakPointCurrent);
			expand = TRUE;
		}
		else if ((*ppAfter)->HasBreakpoint())
			SetItemImage(&ndAfter, CRSTreeCtrlImgIdx::BreakPoint);
	}

	if (expand)
	{
		Expand(htParent, TVE_EXPAND);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillRulesForDebug(Block* pCurrent)
{
	if (!m_htRules)
		m_htRules = AddNode(_T("Rules"), CNodeTree::ENodeType::NT_ROOT_RULES);

	if (!CheckAndClearEdge(m_htRules))
		return FALSE;

	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	BOOL bExpand = FALSE;
	for (int r = 0; r < pEngine->m_SortedRules.GetSize(); r++)
	{
		RuleObj* pR = dynamic_cast<RuleObj*>(pEngine->m_SortedRules.GetAt(r));
		if (!pR) continue;
		ASSERT_VALID(pR);

		CString sDescr = pR->GetDescription();

		HTREEITEM htRule = AddNode(sDescr, CNodeTree::ENodeType::NT_DEBUG_RULE, m_htRules, pR, pEngine);

		bExpand = FillHiddenBlockForDebug(htRule, (Block**)&pR->m_pBefore, (Block**)&pR->m_pAfter, &pEngine->m_SymTable, pCurrent) || bExpand;
	}

	if (bExpand) Expand(m_htRules, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillTupleRulesForDebug(Block* pCurrent)
{
	if (!m_pWDoc->m_pEngine)
		return FALSE;
	ASSERT_VALID(m_pWDoc->m_pEngine);
	RepEngine* pEngine = m_pWDoc->m_pEngine->GetEngine();
	ASSERT_VALID(pEngine);

	QueryEngine* pQEngine = dynamic_cast<QueryEngine*>(pEngine);
	ASSERT_VALID(pQEngine);

	if (!m_htTupleRules)
		m_htTupleRules = AddNode(_T("Tuple Rules"), CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES, NULL, pQEngine);

	if (!CheckAndClearEdge(m_htTupleRules))
		return FALSE;

	BOOL bExpand = FALSE;

	if (
		pQEngine->m_pTupleFilterEngine == NULL &&
		pQEngine->m_pGroupingTupleEngine == NULL &&
		pQEngine->m_pHavingTupleFilterEngine == NULL
		)
	{
		pQEngine->m_pTupleFilterEngine = new ExpressionWithDebugBlocks(pQEngine);
		SAFE_DELETE(pQEngine->m_pTupleFilterEngine->m_pAfter);
	}

	if (pQEngine->m_pTupleFilterEngine)
	{
		HTREEITEM htRule = AddNode(L"Filter", CNodeTree::ENodeType::NT_DEBUG_FILTER_TUPLE_RULE, m_htTupleRules, pQEngine->m_pTupleFilterEngine, pQEngine);

		bExpand = FillHiddenBlockForDebug(htRule, (Block**)&(pQEngine->m_pTupleFilterEngine->m_pBefore), (Block**)&(pQEngine->m_pTupleFilterEngine->m_pAfter), &pEngine->m_SymTable, pCurrent) || bExpand;
	}

	if (pQEngine->m_pGroupingTupleEngine)
	{
		HTREEITEM htRule = AddNode(L"Grouping", CNodeTree::ENodeType::NT_DEBUG_GROUPING_RULE, m_htTupleRules, pQEngine->m_pGroupingTupleEngine, pQEngine);

		bExpand = FillHiddenBlockForDebug(htRule, (Block**)&(pQEngine->m_pGroupingTupleEngine->m_pBefore), (Block**)&(pQEngine->m_pGroupingTupleEngine->m_pAfter), &pEngine->m_SymTable, pCurrent) || bExpand;
	}

	if (pQEngine->m_pHavingTupleFilterEngine)
	{
		HTREEITEM htRule = AddNode(L"Having", CNodeTree::ENodeType::NT_DEBUG_HAVINGGROUP_RULE, m_htTupleRules, pQEngine->m_pHavingTupleFilterEngine, pQEngine);

		bExpand = FillHiddenBlockForDebug(htRule, (Block**)&(pQEngine->m_pHavingTupleFilterEngine->m_pBefore), (Block**)&(pQEngine->m_pHavingTupleFilterEngine->m_pAfter), &pEngine->m_SymTable, pCurrent) || bExpand;
	}

	if (bExpand) Expand(m_htTupleRules, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillEnumsValue(HTREEITEM htParent, DataType dt, CRSEditView* editView)
{
	if (m_bShowNodeEnums && dt.m_wType == DATA_ENUM_TYPE)
	{
		EnumTag* pTag = AfxGetEnumsTable()->GetEnumTag(dt.m_wTag);
		ASSERT_VALID(pTag);

		return FillEnumsValue(htParent, pTag, editView, TRUE);
	}
	return FALSE;
}

BOOL CRSTreeCtrl::FillEnumsValue(HTREEITEM htParent, EnumTag* pTag, CRSEditView* editView, BOOL bColored, BOOL delayed)
{
	for (int k = 0; k < pTag->GetEnumItems()->GetSize(); k++)
	{
		EnumItem* pItem = pTag->GetEnumItems()->GetAt(k);
		DataEnum de(pTag->GetTagValue(), pItem->GetItemValue());
		CString title = pItem->GetTitle();
		if (m_bShowEnumValues)
			title += L" " + de.ToString() + cwsprintf(L" %d", de.GetValue());

		if (!delayed && htParent)
		{
			CNodeTree& nt = AddNode(title, CNodeTree::ENodeType::NT_LIST_ENUM_VALUE, htParent, pItem, pTag);

			if (bColored)
			{
				nt.SetItemColor(RGB(0, RS_COLOR_FRAMEWORK, 0));
			}
		}

		if (delayed && editView)
		{
			CString key = pTag->GetTagTitle();
			key.Replace(L" ", L"_");
			key.Replace(L"/", L"_");
			//editView->GetEditCtrl()->AddIntellisenseWord(key,L"key",L"",L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"enum." + key, key, de.ToString() + L"/*" + pTag->GetTagTitle() + L" : " + pItem->GetTitle() + L"*/", L"enum", L"");
		}
	}
	return TRUE;
}

BOOL CRSTreeCtrl::FillEnums(CRSEditView* editView)
{
	BOOL delayed = AddDelayedNode(m_htEnums, _T("Enums"), CNodeTree::ENodeType::NT_ROOT_ENUMS);

	CWaitCursor wc;

	const EnumTagArray* pTags = m_pWDoc->m_pEditorManager->GetEnumsArray();

	editView->GetEditCtrl()->AddIntellisenseWord(L"enum", L"enum", L"enum", L"", L"");

	for (int j = 0; j < pTags->GetSize(); j++)
	{
		EnumTag* pTag = pTags->GetAt(j);

		HTREEITEM htTag = NULL;
		if (!delayed)
		{
			htTag = AddNode(pTag->GetTagTitle(), CNodeTree::ENodeType::NT_LIST_ENUM_TYPE, m_htEnums, pTag);
		}
		/*else if (editView)
		{
			CString replacedValue = pTag->GetTagTitle();
			replacedValue.Replace(L" ", L"_");
			replacedValue.Replace(L"/", L"_");

			editView->GetEditCtrl()->AddIntellisenseWord(L"enum." + replacedValue, replacedValue, replacedValue, L"", L"");
		}  */

		FillEnumsValue(htTag, pTag, editView, FALSE, delayed);
	}

	//sono già ordinati per titolo SortChildren(m_htEnums);
	SelectItem(m_htEnums);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillCommands(CRSEditView* editView, BOOL bRaiseEvents /*=TRUE*/)
{
	BOOL delayed = AddDelayedNode(m_htCommands, _T("Commands"), CNodeTree::ENodeType::NT_ROOT_COMMANDS);

	if (!delayed)
	{
		if (bRaiseEvents) AddNode(cwsprintf(T_ASK), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_ABORT), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_BEGIN), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_BREAK), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_CALL), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_CONTINUE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_DISPLAY), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_DISPLAY_FREE_FIELDS), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_DISPLAY_TABLE_ROW), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_DO), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_ELSE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_END), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_EVAL), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_FORMFEED), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_IF), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_INTERLINE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_MESSAGE_BOX), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_NEXTLINE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_QUIT), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_RESET), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_RETURN), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_THEN), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_TITLELINE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		if (bRaiseEvents) AddNode(cwsprintf(T_SUBTITLELINE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
		AddNode(cwsprintf(T_WHILE), CNodeTree::ENodeType::NT_LIST_COMMAND, m_htCommands);
	}

	if (delayed && editView)
	{
		editView->GetEditCtrl()->AddIntellisenseWord(L"cmd", L"cmd", L"cmd", L"", L"");
		if (bRaiseEvents)
		{
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_EVAL), cwsprintf(T_EVAL), cwsprintf(T_EVAL), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_RESET), cwsprintf(T_RESET), cwsprintf(T_RESET), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_DISPLAY), cwsprintf(T_DISPLAY), cwsprintf(T_DISPLAY), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_DISPLAY_TABLE_ROW), cwsprintf(T_DISPLAY_TABLE_ROW), cwsprintf(T_DISPLAY_TABLE_ROW), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_DISPLAY_FREE_FIELDS), cwsprintf(T_DISPLAY_FREE_FIELDS), cwsprintf(T_DISPLAY_FREE_FIELDS), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_INTERLINE), cwsprintf(T_INTERLINE), cwsprintf(T_INTERLINE), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_NEXTLINE), cwsprintf(T_NEXTLINE), cwsprintf(T_NEXTLINE), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_SPACELINE), cwsprintf(T_SPACELINE), cwsprintf(T_SPACELINE), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_TITLELINE), cwsprintf(T_TITLELINE), cwsprintf(T_TITLELINE), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_SUBTITLELINE), cwsprintf(T_SUBTITLELINE), cwsprintf(T_SUBTITLELINE), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_FORMFEED), cwsprintf(T_FORMFEED), cwsprintf(T_FORMFEED), L"", L"");
			editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_ASK), cwsprintf(T_ASK), cwsprintf(T_ASK), L"", L"");
		}

		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_CALL), cwsprintf(T_CALL), cwsprintf(T_CALL), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_MESSAGE_BOX), cwsprintf(T_MESSAGE_BOX), cwsprintf(T_MESSAGE_BOX), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_ABORT), cwsprintf(T_ABORT), cwsprintf(T_ABORT), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_BEGIN), cwsprintf(T_BEGIN), cwsprintf(T_BEGIN), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_END), cwsprintf(T_END), cwsprintf(T_END), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_IF), cwsprintf(T_IF), cwsprintf(T_IF), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_THEN), cwsprintf(T_THEN), cwsprintf(T_THEN), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_ELSE), cwsprintf(T_ELSE), cwsprintf(T_ELSE), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_WHILE), cwsprintf(T_WHILE), cwsprintf(T_WHILE), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_DO), cwsprintf(T_DO), cwsprintf(T_DO), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_BREAK), cwsprintf(T_BREAK), cwsprintf(T_BREAK), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_CONTINUE), cwsprintf(T_CONTINUE), cwsprintf(T_CONTINUE), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_RETURN), cwsprintf(T_RETURN), cwsprintf(T_RETURN), L"", L"");
		editView->GetEditCtrl()->AddIntellisenseWord(L"CMD." + cwsprintf(T_QUIT), cwsprintf(T_QUIT), cwsprintf(T_QUIT), L"", L"");
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CRSTreeCtrl::GetCommandDescription(const CString& sCmd)
{
	CString sDescr;
	Token tk = AfxGetTokensTable()->GetKeywordsToken(sCmd);
	//if (tk == T_NOTOKEN)
	//	return sDescr;

	switch (tk)
	{
	case Token::T_EVAL:
	{
		sDescr = _TB("It evaluates the expression entered in the function field");
		break;
	}
	case Token::T_RESET:
	{
		sDescr = _TB("It resets the value of the function field to its initial value.\n"
			"If the field has no initial value, the value set matches to the one which identify the type of the field\n"
			"Example: string --> \"\", numeric--> 0, boolean: FALSE");
		break;
	}

	case Token::T_DISPLAY:
	{
		sDescr = _TB("It displays the data contained in the specific field (also column)");
		break;
	}
	case Token::T_DISPLAY_TABLE_ROW:
	{
		sDescr = _TB("It displays an entire row of data in the table");
		break;
	}

	case Token::T_DISPLAY_FREE_FIELDS:
	{
		sDescr = _TB("It displays the data of all single fields (not column)");
		break;
	}
	case Token::T_INTERLINE:
	{
		sDescr = _TB("It shows a row separator on the current line");
		break;
	}
	case Token::T_NEXTLINE:
	{
		sDescr = _TB("It performs a return (new line) in the table object. If there are more then one table in the report, the name must be specify.");
		break;
	}
	case Token::T_SPACELINE:
	{
		sDescr = _TB("It performs a couple of return (new line) in the table object to leave an empty line. If there are more then one table in the report, the name must be specify.");
		break;
	}
	case Token::T_TITLELINE:
	{
		sDescr = _TB("It shows the column titles on the current line");
		break;
	}
	case Token::T_SUBTITLELINE:
	{
		sDescr = _TB("It shows the custom group title on the current line");
		break;
	}

	case Token::T_FORMFEED:
	{
		sDescr = _TB("It performs a formfeed");
		break;
	}

	case Token::T_CALL:
	{
		sDescr = _TB("It calls a procedure defined in the current report");
		break;
	}

	case Token::T_ABORT:
	{
		sDescr = _TB("It ends the report and displays a message with the text specified as a parameter");
		break;
	}
	case Token::T_MESSAGE_BOX:
	{
		sDescr = _TB("It displays a message in a box and it waits for the user who has to confirm the message by clicking on the Ok button");
		break;
	}

	case Token::T_ASK:
	{
		sDescr = _TB("It opens an ask dialog on demand");
		break;
	}

	case Token::T_BEGIN:
	{
		sDescr = _TB("It is the statement that opens a group of commands");
		break;
	}
	case Token::T_END:
	{
		sDescr = _TB("It is the statement that closes a group of commands");
		break;
	}
	case Token::T_IF:
	{
		sDescr = _TB("It is the statement that starts to test a condition");
		break;
	}
	case Token::T_THEN:
	{
		sDescr = _TB("It is the statement that indicates what to do when a condition is TRUE");
		break;
	}
	case Token::T_ELSE:
	{
		sDescr = _TB("It is the statement that indicates what to do when a condition is FALSE");
		break;
	}
	case Token::T_WHILE:
	{
		sDescr = _TB("It is a loop that allows to repeat a block of statements until the condition is TRUE");
		break;
	}
	case Token::T_BREAK:
	{
		sDescr = _TB("It exits unconditionally from a while loop");
		break;
	}
	case Token::T_DO:
	{
		sDescr = _TB("DO + FUNCTION NAME: It executes the specified function discarding the return value from the same");
		break;
	}
	case Token::T_RETURN:
	{
		sDescr = _TB("It immediately exit by events or by procedures");
		break;
	}
	case Token::T_QUIT:
	{
		sDescr = _TB("Force the closure of the report");
		break;
	}
	}

	return sDescr;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillFunctions(CRSEditView* editView)
{
	CInternalFunctionObjectsParser* pFunctions = m_pWDoc->m_pEditorManager->GetInternalFunctions();
	if (!pFunctions)
		return FALSE;

	BOOL delayed = AddDelayedNode(m_htFunctions, _T("Base Functions"), CNodeTree::ENodeType::NT_ROOT_FUNCTIONS, NULL, pFunctions);
	//return TRUE;

	CString sFCONTENTOF = ::cwsprintf(Token::T_FCONTENTOF);
	CString sFVALUEOF = ::cwsprintf(Token::T_FVALUEOF);

	for (int i = 0; i < pFunctions->m_arFunctionGroups.GetCount(); i++)
	{
		CInternalFunctionObjectsParser::CGroupFunctions* pGroup = pFunctions->m_arFunctionGroups.GetAt(i);

		CFunctionObjectsDescription* pGroupFunc = pGroup->m_parFunctions;
		if (!pGroup->m_parFunctions || pGroup->m_parFunctions->m_arFunctions.GetCount() == 0)
			continue;

		HTREEITEM htGroup = NULL;
		if (!delayed)
			htGroup = AddNode(pGroup->m_sTitle, CNodeTree::ENodeType::NT_GROUP_FUNCTIONS, m_htFunctions, pGroupFunc, pFunctions);

		for (int j = 0; j < pGroup->m_parFunctions->m_arFunctions.GetSize(); j++)
		{
			CDecoratedFunctionDescription * pFunDesc = dynamic_cast<CDecoratedFunctionDescription*>(pGroup->m_parFunctions->m_arFunctions.GetAt(j));

			if (!delayed && htGroup)
			{
				CNodeTree& node = AddNode(pFunDesc->GetName(), CNodeTree::ENodeType::NT_LIST_FUNCTION, htGroup, pFunDesc, pGroupFunc);

				if (pFunDesc->GetName().CompareNoCase(sFCONTENTOF) == 0 || pFunDesc->GetName().CompareNoCase(sFVALUEOF) == 0)
				{
					node.SetItemColor(RS_COLOR_LIGHT_BLUE);
				}
			}

			// fill intellisense
			if (editView && delayed)
			{
				CString key;
				CString sDescrName = pFunDesc->GetName();
				if (pGroup->m_sTitle.CompareNoCase(L"dateandtime") == 0)
					key = L"date";
				else if (pGroup->m_sTitle.CompareNoCase(L"miscellaneous") == 0)
					key = L"misc";
				else if (pGroup->m_sTitle.CompareNoCase(L"math") == 0)
					key = L"math";
				else if (pGroup->m_sTitle.CompareNoCase(L"information") == 0)
					key = L"info";
				else if (pGroup->m_sTitle.CompareNoCase(L"Text") == 0)
					key = L"text";
				else if (pGroup->m_sTitle.CompareNoCase(L"Array") == 0)
				{
					key = L"array";
					sDescrName = sDescrName.Mid(sDescrName.Find('_') + 1);
				}

				//editView->GetEditCtrl()->AddIntellisenseWord(key.MakeUpper(), key, L"", L"");
				editView->GetEditCtrl()->AddIntellisenseWord(key.MakeUpper() + '.' + sDescrName, sDescrName, pFunDesc->GetName() + L" ( )", L"", L"");
			}
		}
	}

	SelectItem(m_htFunctions);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillHtmlTags(CRSEditView* editView, BOOL bExpand)
{
	if (!m_pWDoc->m_bBetaFeatures)
		return FALSE;

	BOOL delayed = FALSE;
	if (bExpand)
	{
		//force load
		if (!m_htHTMLTags)
		{
			m_htHTMLTags = AddNode(_T("Html tags"), CNodeTree::ENodeType::NT_ROOT_HTML_TAGS, NULL);
		}
		if (GetChildItem(m_htHTMLTags))
			return TRUE;
	}
	else
		delayed = AddDelayedNode(m_htHTMLTags, _T("Html tags"), CNodeTree::ENodeType::NT_ROOT_HTML_TAGS, NULL);

	HTREEITEM hTreeItem;
	HTREEITEM hTreeAttributeItem;

	editView->GetEditCtrl()->AddIntellisenseWord(L"html", L"html", L"html", L"", L"");

	for (int i = CHtmlTag::EHtmlTag::HTML_TAG_NEWLINE; i < CHtmlTag::EHtmlTag::HTML_TAG_LAST; i++)
	{
		CHtmlTag::EHtmlTag myTag = static_cast<CHtmlTag::EHtmlTag>(i);

		if (!delayed)
		{
			CHtmlTag* myNodeItem = new CHtmlTag(myTag);
			m_arGarbage.Add(myNodeItem);
			if (myNodeItem->IsAttribute())
				hTreeAttributeItem = AddNode(myNodeItem->GetHtmlName(), CNodeTree::NT_LIST_HTML_TAGS, hTreeItem, myNodeItem);
			else if (myNodeItem->IsStyleProperty())
				AddNode(myNodeItem->GetHtmlName(), CNodeTree::NT_LIST_HTML_TAGS, hTreeAttributeItem, myNodeItem);
			else
				hTreeItem = AddNode(myNodeItem->GetHtmlName(), CNodeTree::NT_LIST_HTML_TAGS, m_htHTMLTags, myNodeItem);
		}
		if (editView)
			editView->GetEditCtrl()->AddIntellisenseWord(L"HTML." + CHtmlTag::GetHtmlName(myTag), CHtmlTag::GetHtmlName(myTag), CHtmlTag::GetHtmlFragment(myTag), L"", CHtmlTag::GetExample(myTag));
	}

	SelectItem(m_htHTMLTags);
	if (bExpand)
		Expand(m_htHTMLTags, TVE_EXPAND);

	return TRUE;
}

//--------------------------------------------------------------------
BOOL CRSTreeCtrl::FillQueriesTags(CRSEditView* editView, BOOL bExpand)
{
	BOOL delayed = FALSE;
	if (bExpand)
	{
		//force load
		if (!m_htQueriesTags)
		{
			m_htQueriesTags = AddNode(_T("Queries tags"), CNodeTree::ENodeType::NT_ROOT_QUERY_TAGS, NULL);
		}
		if (GetChildItem(m_htQueriesTags))
			return TRUE;
	}
	else
		delayed = AddDelayedNode(m_htQueriesTags, _T("Queries tags"), CNodeTree::ENodeType::NT_ROOT_QUERY_TAGS, NULL);

	HTREEITEM hTreeItem;
	HTREEITEM hTreeAttributeItem;
	editView->GetEditCtrl()->AddIntellisenseWord(L"query", L"query", L"query", L"", L"");
	for (int i = CQueryTag::EQueryTag::QUERY_TAG_COL; i != CQueryTag::EQueryTag::QUERY_TAG_LAST; i++)
	{
		CQueryTag::EQueryTag myTag = static_cast<CQueryTag::EQueryTag>(i);

		if (!delayed)
		{
			CQueryTag* myNodeItem = new CQueryTag(myTag);
			m_arGarbage.Add(myNodeItem);
			if (myNodeItem->IsAttribute())
				hTreeAttributeItem = AddNode(myNodeItem->GetName(), CNodeTree::NT_LIST_QUERY_TAGS, hTreeItem, myNodeItem);
			else if (myNodeItem->IsStyleProperty())
				AddNode(myNodeItem->GetName(), CNodeTree::NT_LIST_QUERY_TAGS, hTreeAttributeItem, myNodeItem);
			else
				hTreeItem = AddNode(myNodeItem->GetName(), CNodeTree::NT_LIST_QUERY_TAGS, m_htQueriesTags, myNodeItem);
		}
		if (editView)
		{
			editView->GetEditCtrl()->AddIntellisenseWord(L"query." + CQueryTag::GetName(myTag), CQueryTag::GetName(myTag), CQueryTag::GetFragment(myTag), L"", CQueryTag::GetExample(myTag));
		}
	}

	SelectItem(m_htQueriesTags);
	if (bExpand)
		Expand(m_htQueriesTags, TVE_EXPAND);

	return TRUE;
}

//---------------------------------------------------------------------------- -
BOOL CRSTreeCtrl::FillWebMethods(CRSEditView* editView)
{
	BOOL delayed = AddDelayedNode(m_htWebMethods, _T("Application functions"), CNodeTree::ENodeType::NT_ROOT_WEBMETHODS);

	CWaitCursor wc;
	HTREEITEM htMail = NULL;
	HTREEITEM htPostaLite = NULL;
	HTREEITEM htMiniHtml = NULL;
	HTREEITEM htAdvanced = NULL;

	for (int a = 0; a < AfxGetAddOnAppsTable()->GetSize(); a++)
	{
		AddOnApplication* pAddOnApplication = AfxGetAddOnAppsTable()->GetAt(a);

		if (pAddOnApplication->m_strAddOnAppName.CompareNoCase(L"TBF") == 0 ||
			pAddOnApplication->m_strAddOnAppName.CompareNoCase(L"TBS") == 0)
			continue;

		BOOL bIsFramework = pAddOnApplication->m_strAddOnAppName.CompareNoCase(L"Framework") == 0;

		HTREEITEM htApp = NULL;
		if (pAddOnApplication->m_pAddOnModules->GetSize() > 0 && !delayed)
			htApp = AddNode(pAddOnApplication->m_strAddOnAppName, CNodeTree::ENodeType::NT_ROOT_MODULE, m_htWebMethods, pAddOnApplication);
		if (delayed && editView)
		{
			editView->GetEditCtrl()->AddIntellisenseWord(pAddOnApplication->m_strAddOnAppName, pAddOnApplication->m_strAddOnAppName, pAddOnApplication->m_strAddOnAppName, L"", L"");
		}

		for (int m = 0; m < pAddOnApplication->m_pAddOnModules->GetSize(); m++)
		{
			AddOnModule* pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(m);

			if (bIsFramework)
			{
				if (
					pAddOnMod->GetModuleName().CompareNoCase(L"TbWoormViewer") &&
					pAddOnMod->GetModuleName().CompareNoCase(L"TbResourcesMng")
					)
					continue;
			}

			BOOL bModInserted = FALSE;
			HTREEITEM htMod = NULL;

			const CBaseDescriptionArray &arFunctions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().GetFunctions();
			for (int k = 0; k < arFunctions.GetSize(); k++)
			{
				CFunctionDescription* pFun = (CFunctionDescription*)arFunctions.GetAt(k);

				if (!pFun->IsPublished())
					continue;
				if (!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
					continue;

				if (editView && delayed)
				{
					CString fullFuncionName, iValue;
					CTBNamespace ns = pFun->GetNamespace();

					iValue = fullFuncionName = pAddOnApplication->m_strAddOnAppName + L".";

					editView->GetEditCtrl()->AddIntellisenseWord(pAddOnApplication->m_strAddOnAppName, pAddOnApplication->m_strAddOnAppName, pAddOnApplication->m_strAddOnAppName, L"", L"");

					for (int j = 2; j < ns.GetTokenArray()->GetCount(); j++)
					{
						if (j == ns.GetTokenArray()->GetUpperBound())
							editView->GetEditCtrl()->AddIntellisenseWord(fullFuncionName + ns.GetTokenArray()->GetAt(j), ns.GetTokenArray()->GetAt(j), iValue + ns.GetTokenArray()->GetAt(j) + L" ( )", L"", L"");
						else
							editView->GetEditCtrl()->AddIntellisenseWord(fullFuncionName + ns.GetTokenArray()->GetAt(j), ns.GetTokenArray()->GetAt(j), iValue + ns.GetTokenArray()->GetAt(j), L"", L"");

						fullFuncionName += ns.GetTokenArray()->GetAt(j) + L".";
						iValue += ns.GetTokenArray()->GetAt(j) + L".";
					}
				}

				if (!bModInserted)
				{
					CString strMod = pAddOnMod->GetModuleName() + L" - " + pAddOnMod->GetModuleTitle();
					if (!delayed)
					{
						htMod = AddNode(strMod, CNodeTree::ENodeType::NT_SUBROOT_MODULE, htApp, pAddOnMod, pAddOnApplication);
						if (bIsFramework &&  pAddOnMod->GetModuleName().CompareNoCase(L"TbWoormViewer") == 0)
						{
							htMail = AddNode(IDF_WOORM_GROUP_MAIL, CNodeTree::ENodeType::NT_SUBROOT_MODULE, htMod, pAddOnMod, pAddOnApplication);
							htPostaLite = AddNode(IDF_WOORM_GROUP_POSTALITE, CNodeTree::ENodeType::NT_SUBROOT_MODULE, htMod, pAddOnMod, pAddOnApplication);
							htMiniHtml = AddNode(IDF_WOORM_GROUP_MINIHTML, CNodeTree::ENodeType::NT_SUBROOT_MODULE, htMod, pAddOnMod, pAddOnApplication);
							htAdvanced = AddNode(IDF_WOORM_GROUP_ADVANCED, CNodeTree::ENodeType::NT_SUBROOT_MODULE, htMod, pAddOnMod, pAddOnApplication);
						}
					}
					bModInserted = TRUE;
				}

				if (!delayed)
				{
					HTREEITEM htParent = htMod;
					if (bIsFramework && !pFun->m_strGroup.IsEmpty())
					{
						if (pFun->m_strGroup.CompareNoCase(IDF_WOORM_GROUP_MAIL) == 0)
							htParent = htMail;
						else if (pFun->m_strGroup.CompareNoCase(IDF_WOORM_GROUP_POSTALITE) == 0)
							htParent = htPostaLite;
						else if (pFun->m_strGroup.CompareNoCase(IDF_WOORM_GROUP_MINIHTML) == 0)
							htParent = htMiniHtml;
						else if (pFun->m_strGroup.CompareNoCase(IDF_WOORM_GROUP_ADVANCED) == 0)
							htParent = htAdvanced;
					}
					if (htParent)
					{
						HTREEITEM htF = AddNode(pFun->GetName(), CNodeTree::ENodeType::NT_LIST_WEBMETHOD, htParent, pFun, pAddOnMod);

						if (bIsFramework)
						{
							if (pFun->GetName().CompareNoCase(IDF_WOORM_GetCompanyInfo) == 0)
							{
								int ni = AfxGetLoginContext()->GetCompanyInfoCount();
								for (int i = 0; i < ni; i++)
								{
									CSpecialField* f = new CSpecialField(AfxGetLoginContext()->GetCompanyTagInfo(i), AfxGetLoginContext()->GetCompanyTagInfo(i));
									m_arGarbage.Add(f);
									AddNode(f->GetDescription(), CNodeTree::NT_LIST_SPECIAL_TEXT, htF, f, pFun, pAddOnMod);
								}
							}
						}
					}
				}
			}
			if (htMod)
				SortChildren(htMod);
		}
		if (htApp)
			SortChildren(htApp);
	}
	SortChildren(m_htWebMethods);

	SelectItem(m_htWebMethods);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillUnselectedColumns(CNodeTree* pNode)
{
	ASSERT(pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN);
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pParentItemData);
	ASSERT_VALID(pTblRule);
	SqlTableInfo* pTableInfo = dynamic_cast<SqlTableInfo*>(pNode->m_pItemData);
	ASSERT_VALID(pTableInfo);

	int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pTableInfo->GetTableName());
	ASSERT(idx >= 0);
	if (idx < 0) return FALSE;

	DataFieldLinkArray* parLinks = pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[idx];
	ASSERT_VALID(parLinks);

	CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pTableInfo->GetSqlCatalogEntry());
	ASSERT_VALID(pTC);
	if (!pTC)
		return FALSE;

	FillColumns(pTC, pNode->m_ht, parLinks, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillTable(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent, DataFieldLinkArray* parLinks)
{
	ASSERT_VALID(pTC);
	ASSERT_VALID(pTC->m_pCatalogEntry);

	if (
		m_bShowFilteredTables &&
		!m_FilterTablePattern.IsEmpty() &&
		!::WildcardMatch(pTC->m_pCatalogEntry->m_strTableName, m_FilterTablePattern)
		)
		return FALSE;


	CNodeTree::ENodeType nType = CNodeTree::ENodeType::NT_WRONG;
	switch (pTC->m_pCatalogEntry->m_nType)
	{
	case TABLE_TYPE:
		nType = CNodeTree::ENodeType::NT_LIST_DBTABLE;
		break;
	case VIEW_TYPE:
		nType = CNodeTree::ENodeType::NT_LIST_DBVIEW;
		break;
	default:
		return FALSE;
	}

	HTREEITEM htTable = AddNode(pTC->m_pCatalogEntry->m_strTableName, nType, htParent, const_cast<SqlCatalogEntry*>(pTC->m_pCatalogEntry), pTC);

	AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htTable);
	//return FillTableColumns(pTC, htTable, parLinks);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::FillColumns(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent, DataFieldLinkArray* parLinks/* = NULL*/, BOOL bSkipLinked /*= FALSE*/)
{
	HTREEITEM htChild = this->GetChildItem(htParent);
	if (htChild)
	{
		CNodeTree* pTmpNode = GetNode(htChild);
		if (pTmpNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
			DeleteItem(htChild);
	}

	ASSERT_VALID(pTC);
	for (int c = 0; c < pTC->m_arSortedColumns.GetSize(); c++)
	{
		SqlColumnInfoObject* pCol = (SqlColumnInfoObject*)(pTC->m_arSortedColumns.GetAt(c));
		BOOL bLinked = FALSE;
		if (parLinks)
		{
			if (parLinks->Find(pCol->GetColumnName()))
			{
				if (bSkipLinked)
					continue;
				bLinked = TRUE;
			}
		}

		CNodeTree& nt = AddNode(pCol->GetColumnName(), CNodeTree::ENodeType::NT_LIST_COLUMN_INFO, htParent, pCol, const_cast<SqlCatalogEntry*>(pTC->m_pCatalogEntry));

		if (bLinked)
			nt.SetItemColor(RS_COLOR_VARIABLE);
		else if (bSkipLinked)
			nt.SetItemColor(RS_COLOR_EMPTY);

		FillEnumsValue(nt.m_ht, pCol->GetDataObjType(), GetDocument()->GetWoormFrame()->m_pEditView);
	}
}

BOOL CRSTreeCtrl::FillTableColumns(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htTable, DataFieldLinkArray* parLinks/*=NULL*/)
{
	HTREEITEM htChild = this->GetChildItem(htTable);
	if (htChild)
	{
		CNodeTree* pTmpNode = GetNode(htChild);
		if (pTmpNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
			DeleteItem(htChild);
	}

	ASSERT_VALID(pTC->m_pCatalogEntry);
	if (pTC->m_pCatalogEntry && pTC->m_pCatalogEntry->m_nType == TABLE_TYPE)
	{
		HTREEITEM htFKTable = AddNode(_T("Foreign Key"), CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY, htTable, pTC);
		AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htFKTable);

		HTREEITEM htExtRefTable = AddNode(_T("External References"), CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES, htTable, pTC);
		AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htExtRefTable);
	}

	FillColumns(pTC, htTable, parLinks);

	return TRUE;
}
//-----------------------------------------------------------------------------

BOOL CRSTreeCtrl::FillTable(const SqlCatalogEntry* pCatalogEntry, HTREEITEM htParent, DataFieldLinkArray* parLinks)
{
	ASSERT_VALID(pCatalogEntry);
	if (!pCatalogEntry)
		return FALSE;

	CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pCatalogEntry);
	ASSERT_VALID(pTC);
	if (!pTC)
		return FALSE;

	return FillTable(pTC, htParent, parLinks);
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillTables(CRSEditView* editView)
{
	m_bShowAllTables = FALSE;

	if (AddDelayedNode(m_htTables, _T("Database"), CNodeTree::ENodeType::NT_ROOT_TABLES, NULL))
	{

		// fill Intellisense  with all tables
		if (this->IsKindOf(RUNTIME_CLASS(CRSEditViewTreeCtrl)))
		{
			if (editView)
			{
				SqlCatalogConstPtr pCatalog = AfxGetDefaultSqlConnection()->GetCatalog();
				CString key;
				SqlCatalogEntry* pCatalogEntry;

				for (POSITION pos = pCatalog->GetStartPosition(); pos != NULL;)
				{
					pCatalog->GetNextAssoc(pos, key, (CObject*&)pCatalogEntry);

					if (pCatalogEntry)
					{
						if (pCatalogEntry->m_nType == VIRTUAL_TYPE || pCatalogEntry->m_nType == PROC_TYPE)
							continue;

						CString tableName(pCatalogEntry->m_strTableName);


						editView->GetEditCtrl()->AddIntellisenseWord(tableName, tableName, tableName, L"", L"");


						if (pCatalogEntry->m_pTableInfo)
						{
							CString tableNameKey = tableName;
							tableNameKey += '.';
							for (int i = 0; i < pCatalogEntry->m_pTableInfo->GetSizePhisycalColumns(); i++)
							{
								const SqlColumnInfoObject* pCol = pCatalogEntry->m_pTableInfo->GetAt(i);
								ASSERT_VALID(pCol);
								if (!pCol || pCol->m_bVirtual)
									continue;

								editView->GetEditCtrl()->AddIntellisenseWord(tableNameKey + pCol->GetColumnName(), pCol->GetColumnName(), tableNameKey + pCol->GetColumnName(), L"", L"");
							}
						}
					}
				}
			}
		}
		//
		return TRUE;
	}


	CWaitCursor wc;

	CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	for (int m = 0; m < pHelperSqlCatalog->m_arModules.GetSize(); m++)
	{
		CHelperSqlCatalog::CModuleTables* pMT = dynamic_cast<CHelperSqlCatalog::CModuleTables*>(pHelperSqlCatalog->m_arModules.GetAt(m));
		ASSERT_VALID(pMT);

		HTREEITEM htMod = AddNode(pMT->m_sTitle, CNodeTree::ENodeType::NT_SUBROOT_DB_MODULE, m_htTables, pMT, pHelperSqlCatalog);
		if (m_bShowFilteredTables)
		{
			CNodeTree* pNode = GetNode(htMod);
			FillSubModuleTables(pNode, htMod);
			if (!GetChildItem(htMod))
				DeleteItem(htMod);
		}
		else
		{
			AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htMod);
		}
	}

	if (pHelperSqlCatalog->m_arExternalTables.GetSize())
	{
		CString sExternalTableTitle(_T("<External tables>"));
		HTREEITEM htExternalMod = AddNode(sExternalTableTitle, CNodeTree::ENodeType::NT_SUBROOT_DB_MODULE, m_htTables, &pHelperSqlCatalog->m_arExternalTables, pHelperSqlCatalog);
		AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htExternalMod);
	}

	SelectItem(m_htTables);




	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillAllTables()
{
	m_bShowAllTables = TRUE;

	if (AddDelayedNode(m_htTables, _T("Database"), CNodeTree::ENodeType::NT_ROOT_TABLES, NULL))
		return TRUE;

	CWaitCursor wc;

	CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	for (int m = 0; m < pHelperSqlCatalog->m_arAllTables.GetSize(); m++)
	{
		CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pHelperSqlCatalog->m_arAllTables.GetAt(m));
		ASSERT_VALID(pTC);
		ASSERT_VALID(pTC->m_pCatalogEntry);

		if (
			m_bShowFilteredTables &&
			!m_FilterTablePattern.IsEmpty() &&
			!::WildcardMatch(pTC->m_pCatalogEntry->m_strTableName, m_FilterTablePattern)
			)
			continue;

		CNodeTree::ENodeType nType = CNodeTree::ENodeType::NT_WRONG;
		switch (pTC->m_pCatalogEntry->m_nType)
		{
		case TABLE_TYPE:
			nType = CNodeTree::ENodeType::NT_LIST_DBTABLE;
			break;
		case VIEW_TYPE:
			nType = CNodeTree::ENodeType::NT_LIST_DBVIEW;
			break;
		default:
			ASSERT(FALSE);
			continue;
		}

		HTREEITEM htTable = AddNode(pTC->m_pCatalogEntry->m_strTableName, nType, m_htTables, const_cast<SqlCatalogEntry*>(pTC->m_pCatalogEntry), pTC);
		AddNode(_T("<Load On Demand>"), CNodeTree::ENodeType::NT_WRONG, htTable);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillForeignKey(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent)
{
	HTREEITEM htChild = this->GetChildItem(htParent);
	if (htChild)
	{
		CNodeTree* pTmpNode = GetNode(htChild);
		if (pTmpNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
			DeleteItem(htChild);
	}
	//----
	Array& arFK = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog()->GetForeignKeys(pTC);
	if (arFK.GetCount() == 0)
		return FALSE;

	HTREEITEM htFK = NULL;
	CString columnName;

	for (int i = 0; i < arFK.GetCount(); i++)
	{
		CHelperSqlCatalog::CTableForeignTables* pFK = dynamic_cast<CHelperSqlCatalog::CTableForeignTables*>(arFK.GetAt(i));
		if (pFK->m_arForeignKeys.GetCount() == 0)
			continue;

		if (pFK->m_arForeignKeys.GetCount() > 1)
		{
			columnName.Empty();
			htFK = AddNode(pFK->m_sForeignTableName, CNodeTree::ENodeType::NT_LIST_DB_FOREIGN_KEY, htParent, pFK->m_arForeignKeys.GetAt(0), pTC);

			for (int k = 0; k < pFK->m_arForeignKeys.GetCount(); k++)
			{
				CHelperSqlCatalog::CTableForeignTablesKeys* pK = dynamic_cast<CHelperSqlCatalog::CTableForeignTablesKeys*>(pFK->m_arForeignKeys.GetAt(k));

				HTREEITEM htSeg = AddNode(pTC->m_pCatalogEntry->m_strTableName + '.' + pK->m_sColumnName, CNodeTree::ENodeType::NT_DUMMY_NODE, htFK);
				AddNode(pFK->m_sForeignTableName + '.' + pK->m_sForeignColumnName, CNodeTree::ENodeType::NT_DUMMY_NODE, htSeg);
			}
		}
		else
		{
			CHelperSqlCatalog::CTableForeignTablesKeys* pK = dynamic_cast<CHelperSqlCatalog::CTableForeignTablesKeys*>(pFK->m_arForeignKeys.GetAt(0));

			if (columnName.CompareNoCase(pK->m_sColumnName))
			{
				columnName = pK->m_sColumnName;
				htFK = AddNode(pK->m_sColumnName, CNodeTree::ENodeType::NT_DUMMY_NODE, htParent);
			}

			AddNode(pFK->m_sForeignTableName + '.' + pK->m_sForeignColumnName, CNodeTree::ENodeType::NT_LIST_DB_FOREIGN_KEY, htFK, pK, pTC);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillExternalReference(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent)
{
	HTREEITEM htChild = this->GetChildItem(htParent);
	if (htChild)
	{
		CNodeTree* pTmpNode = GetNode(htChild);
		if (pTmpNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
			DeleteItem(htChild);
	}
	//----
	CHelperExternalReferences* pH = m_pWDoc->m_pEditorManager->GetHelperExternalReferences();
	CHelperExternalReferences::CTableExtRefs* pER = pH->GetTableExtRefs(pTC->m_pCatalogEntry->m_strTableName);
	if (!pER)
		return FALSE;

	HTREEITEM htSER = NULL;
	HTREEITEM htAux = NULL;
	CString columnName;
	CString sFK;

	for (int i = 0; i < pER->m_arExtRefs.GetCount(); i++)
	{
		CHelperExternalReferences::CTableSingleExtRef* pSER = dynamic_cast<CHelperExternalReferences::CTableSingleExtRef*>(pER->m_arExtRefs.GetAt(i));

		columnName = pSER->m_sForeignKey;

		if (!pTC->m_pCatalogEntry->m_pTableInfo->ExistColumn(columnName))
			continue;

		sFK.Empty();

		HTREEITEM htSER = FindItem(pSER->m_sForeignKey, htParent);
		if (!htSER)
		{
			htSER = AddNode(pSER->m_sForeignKey, CNodeTree::ENodeType::NT_DUMMY_NODE, htParent, pSER, pTC);
		}

		CString sCurrFK = pSER->m_sExtTableName + '.' + pSER->m_sExtPrimaryKey;
		sFK = sCurrFK;

		htAux = FindItem(sFK, htSER);
		if (!htAux)
		{
			htAux = AddNode(sFK, CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES, htSER, pSER, pTC);
		}

#ifdef  DEBUG
		if (!AfxGetTbCmdManager()->TableExists(pER->m_sTableName, pSER->m_sForeignKey) || !AfxGetTbCmdManager()->TableExists(pSER->m_sExtTableName, pSER->m_sExtPrimaryKey))
		{
			GetNode(htAux)->SetItemColor(RGB(255, 0, 0));
			GetNode(htSER)->SetItemColor(RGB(255, 0, 0));
			GetNode(htParent)->SetItemColor(RGB(255, 0, 0));
		}


#endif //  DEBUG

		HTREEITEM htDoc = AddNode(pSER->m_sExtDocNS, CNodeTree::ENodeType::NT_DUMMY_NODE, htAux, pSER, pTC);

		if (!pSER->m_sExpression.IsEmpty())
			AddNode(pSER->m_sExpression, CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES, htDoc, pSER, pTC);
	}

	//TODO da COMPLETARE: non si riesce poi a creare la join		

	Array* arr = pH->GetExtRefsToTable(pTC->m_pCatalogEntry->m_strTableName);

	CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	HTREEITEM refs = htParent; //AddNode(_TB("Referenced In"), CNodeTree::ENodeType::NT_LIST_EXTERNAL_REFERENCES_GENERIC, htParent, pTC);
	int i = 0;
	for (; i < arr->GetCount(); i++)
	{
		CHelperExternalReferences::CTableExtRefs* pER = (CHelperExternalReferences::CTableExtRefs*)arr->GetAt(i);

		if (!pER || pER->m_arExtRefs.GetSize() == 0)
			continue;

		CHelperExternalReferences::CTableSingleExtRef* pSER = dynamic_cast<CHelperExternalReferences::CTableSingleExtRef*>(pER->m_arExtRefs.GetAt(0));

		CHelperSqlCatalog::CTableColumns* pTC_Target = pHelperSqlCatalog->FindEntryByName(pER->m_sTableName);
		ASSERT_VALID(pTC_Target);
		if (!pTC_Target)
			continue;

		HTREEITEM tName = FindItem(pSER->m_sExtPrimaryKey, refs);
		if (!tName)
		{
			tName = AddNode(pSER->m_sExtPrimaryKey, CNodeTree::ENodeType::NT_DUMMY_NODE, refs, pER, pTC);
		}

		CString sCurrFK = pER->m_sTableName + '.' + pSER->m_sForeignKey;

		HTREEITEM fullName = FindItem(sCurrFK, tName);
		if (!fullName)
			fullName = AddNode(sCurrFK, CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE, tName, pER, pTC);

#ifdef  DEBUG
		if (!AfxGetTbCmdManager()->TableExists(pER->m_sTableName, pSER->m_sForeignKey))
		{
			GetNode(fullName)->SetItemColor(RGB(255, 0, 0));
			GetNode(tName)->SetItemColor(RGB(255, 0, 0));
			GetNode(refs)->SetItemColor(RGB(255, 0, 0));
		}


#endif //  DEBUG

		HTREEITEM nameSp = AddNode(pER->m_sCurrDocNS, CNodeTree::ENodeType::NT_DUMMY_NODE, fullName, pER, pTC);

		if (!pSER->m_sExpression.IsEmpty())
			AddNode(pSER->m_sExpression, CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE, nameSp, pER, pTC);
	}
	if (i)
		this->SortChildren(htParent);

	m_arGarbage.Add(arr);	//TODO aggiungere nell'helper

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillSubModuleTables(CNodeTree * pNode, HTREEITEM htParent)
{
	HTREEITEM htChild = this->GetChildItem(htParent);
	if (htChild)
	{
		CNodeTree* pTmpNode = GetNode(htChild);
		if (pTmpNode->m_NodeType == CNodeTree::ENodeType::NT_WRONG)
			DeleteItem(htChild);
	}

	CHelperSqlCatalog*	pHelperSqlCatalog = m_pWDoc->m_pEditorManager->GetHelperSqlCatalog();
	ASSERT_VALID(pHelperSqlCatalog);

	int idx = pHelperSqlCatalog->m_arModules.FindPtr(pNode->m_pItemData);

	if (idx == -1)
	{
		for (int t = 0; t < pHelperSqlCatalog->m_arExternalTables.GetSize(); t++)
		{
			CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pHelperSqlCatalog->m_arExternalTables.GetAt(t));
			ASSERT_VALID(pTC);

			FillTable(pTC, htParent);
		}
	}
	else
	{
		CHelperSqlCatalog::CModuleTables* pMT = dynamic_cast<CHelperSqlCatalog::CModuleTables*>(pHelperSqlCatalog->m_arModules.GetAt(idx));
		ASSERT_VALID(pMT);
		for (int t = 0; t < pMT->m_arModTables.GetSize(); t++)
		{
			CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pMT->m_arModTables.GetAt(t));
			ASSERT_VALID(pTC);

			FillTable(pTC, htParent);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillRuleTables(TblRuleData* pTblRule)
{
	if (m_htRuleTables)
		RemoveTreeChilds(m_htRuleTables);

	ASSERT_VALID(pTblRule);

	SqlTableJoinInfoArray* parTables = &pTblRule->m_arSqlTableJoinInfoArray;

	if (!parTables || parTables->GetSize() == 0)
		return FALSE;

	if (!m_htRuleTables)
		m_htRuleTables = AddNode(_T("Current rule tables"), CNodeTree::ENodeType::NT_ROOT_USED_TABLES, NULL, pTblRule);

	//ASSERT(parTables->GetSize() == pTblRule->m_arFieldLinks.GetSize());
	for (int i = 0; i < parTables->GetSize(); i++)
	{
		const SqlTableInfo* pT = (*parTables)[i];

		ASSERT_VALID(pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[i]);
		FillTable(pT->GetSqlCatalogEntry(), m_htRuleTables, pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[i]);
	}

	Expand(m_htRuleTables, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
#define ADD_SPECIAL(a) {CSpecialField* f = new CSpecialField(CSpecialField::a);\
m_arGarbage.Add(f);AddNode(f->GetDescription(),CNodeTree::NT_LIST_SPECIAL_TEXT,m_htSpecialText,f);}

BOOL CRSTreeCtrl::FillSpecialTextRect(BOOL bExpand)
{
	if (bExpand)
	{
		//force load
		if (!m_htSpecialText)
		{
			m_htSpecialText = AddNode(_T("Text macro"), CNodeTree::ENodeType::NT_ROOT_MACRO_TEXT, NULL);
		}
		if (GetChildItem(m_htSpecialText))
			return TRUE;
	}
	else
	{
		if (AddDelayedNode(m_htSpecialText, _T("Text macro"), CNodeTree::ENodeType::NT_ROOT_MACRO_TEXT, NULL))
		{
			return TRUE;
		}
	}

	ADD_SPECIAL(SPECIAL_PAGE)
		ADD_SPECIAL(SPECIAL_TOT_PAGE)
		ADD_SPECIAL(SPECIAL_PAGE_SPLITTER)
		ADD_SPECIAL(SPECIAL_SPLITTER)
		ADD_SPECIAL(SPECIAL_TOT_SPLITTER)
		ADD_SPECIAL(SPECIAL_APPDATE)
		ADD_SPECIAL(SPECIAL_TODAY)
		ADD_SPECIAL(SPECIAL_TODAY2)
		ADD_SPECIAL(SPECIAL_YEAR)
		ADD_SPECIAL(SPECIAL_MONTH)
		ADD_SPECIAL(SPECIAL_MONTH2)
		ADD_SPECIAL(SPECIAL_DAY)
		ADD_SPECIAL(SPECIAL_HH_MM)
		ADD_SPECIAL(SPECIAL_HH)
		ADD_SPECIAL(SPECIAL_MM)
		ADD_SPECIAL(SPECIAL_SEC)

		ADD_SPECIAL(SPECIAL_USER)
		ADD_SPECIAL(SPECIAL_LOGINUSER)
		ADD_SPECIAL(SPECIAL_COMPUTER)

		ADD_SPECIAL(SPECIAL_APP_TITLE)
		ADD_SPECIAL(SPECIAL_APP_REL)
		ADD_SPECIAL(SPECIAL_TB_REL)
		ADD_SPECIAL(SPECIAL_LICENSEE)
		ADD_SPECIAL(SPECIAL_PRODUCER_NAME)
		ADD_SPECIAL(SPECIAL_PRODUCT_DATE)

		ADD_SPECIAL(SPECIAL_REPORTNAME)
		ADD_SPECIAL(SPECIAL_REPORT_TITLE)
		ADD_SPECIAL(SPECIAL_REPORT_SUBJECT)
		ADD_SPECIAL(SPECIAL_REPORT_AUTHOR)
		ADD_SPECIAL(SPECIAL_REPORT_COMPANY)
		ADD_SPECIAL(SPECIAL_REPORT_COMMENTS)

		ADD_SPECIAL(SPECIAL_SYS_COMPANY_NAME)
		ADD_SPECIAL(SPECIAL_DB_COMPANY_NAME)

		ADD_SPECIAL(SPECIAL_EVAL_EXPR)

		/* TODO Special macro from CompanyInfo
			int ni = AfxGetLoginContext()->GetCompanyInfoCount();
			for (int i = 0; i < ni; i++)
			{
				CSpecialField* f = new CSpecialField(AfxGetLoginContext()->GetCompanyTagInfo(i), AfxGetLoginContext()->GetCompanyTagInfo(i));
				m_arGarbage.Add(f);
				AddNode(f->GetDescription(), CNodeTree::NT_LIST_SPECIAL_TEXT, m_htSpecialText, f);
			}
		*/
		//----------
		SelectItem(m_htSpecialText);

	if (bExpand)
		Expand(m_htSpecialText, TVE_EXPAND);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillUndoActions()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillToolBox(BOOL forceReload)
{
	if (m_htToolBox && !forceReload)
		return TRUE;

	if (forceReload)
		RemoveTreeChilds(m_htToolBox);
	else
	{
		CNodeTree& ntRoot = AddNode(_T("Report Objects"), CNodeTree::ENodeType::NT_TOOLBOX_ROOT_OBJECTS);
		ntRoot.SetItemFont(m_pBold);
		m_htToolBox = ntRoot;
	}

	{
		CNodeTree& nt = AddNode(_T("Rectangle"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_SQR_RECT);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::RectangleGlyph);
	}

	{
		CNodeTree& nt = AddNode(_T("Image"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_GRAPH_RECT);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::ImageGlyph);
	}
	{
		CNodeTree& nt = AddNode(_T("Text"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_TEXT_RECT);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::TextGlyph);
	}
	{
		CNodeTree& nt = AddNode(_T("File"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_FILE_RECT);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::TextFileGlyph);
	}
	{
		CNodeTree& nt = AddNode(_T("Repeater"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_REPEATER);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::RepeaterGlyph);
	}

	{
		CNodeTree& nt = AddNode(_T("Hyperlink"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_RS_ADD_LINK);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::LinkToFile);
	}
	{
		CNodeTree& nt = AddNode(_T("Column Total"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_COL_ADD_TOTAL);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::Total);
	}

	{
		CNodeTree& nt = AddNode(_T("Formula"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_RS_ADD_FIELD_NEW_FUNCEXPR);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::FuncGlyph);
	}

	{
		CNodeTree& nt = AddNode(_T("Array"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_ARRAY);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::FuncArrayGlyph);
	}

	{
		CNodeTree& nt = AddNode(_T("DMS Barcode"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_EA_BARCODE);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::BarcodeGlyph);
	}

	if (GetDocument()->m_bBetaFeatures)
	{
		CNodeTree& nt = AddNode(_T("Chart"), CNodeTree::ENodeType::NT_TOOLBOX_OBJECT, m_htToolBox, (CObject*)ID_ADD_CHART);
		SetItemImage(&nt, CRSTreeCtrlImgIdx::ChartGlyph);
	}

	Expand(m_htToolBox, TVE_EXPAND);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillLayouts(BOOL bSort/* = TRUE*/)
{
	if (bSort)
	{
		m_pWDoc->SortLayoutObjectsOnPosition();
	}

	if (!m_htLayouts)
	{
		m_htLayouts = AddNode(_T("Page layouts"), CNodeTree::ENodeType::NT_ROOT_LAYOUTS, NULL, &m_pWDoc->m_Layouts);
	}
	else
		RemoveTreeChilds(m_htLayouts);

	m_htLayoutDefault = NULL;

	CString strName; CObject* pObj = NULL;
	for (POSITION pos = m_pWDoc->m_Layouts.GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		m_pWDoc->m_Layouts.GetNextAssoc(pos, strName, pObj);
		CLayout* pObjects = (CLayout*)pObj;

		HTREEITEM htLayout = AddNode(strName, CNodeTree::ENodeType::NT_LAYOUT, m_htLayouts, pObjects, &(m_pWDoc->m_Layouts));

		if (strName.CompareNoCase(REPORT_DEFAULT_LAYOUT_NAME) == 0)
		{
			m_htLayoutDefault = htLayout;
		}

		FillLayout(pObjects, htLayout);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
int  CRSTreeCtrl::GetImgIndex(CNodeTree::ENodeType eType)
{
	switch (eType)
	{
	case CNodeTree::ENodeType::NT_ASKFIELD:
		return CRSTreeCtrlImgIdx::InputAndAskVarGlyph;

	case  CNodeTree::ENodeType::NT_OBJ_FILERECT:
		return CRSTreeCtrlImgIdx::TextFileGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_TEXTRECT:
		return CRSTreeCtrlImgIdx::TextGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_FIELDRECT:
		return CRSTreeCtrlImgIdx::FieldGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_GRAPHRECT:
		return CRSTreeCtrlImgIdx::ImageGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_SQRRECT:
		return CRSTreeCtrlImgIdx::RectangleGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_TABLE:
		return CRSTreeCtrlImgIdx::TableGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_COLUMN:
		return CRSTreeCtrlImgIdx::ColumnGlyph;
	case CNodeTree::ENodeType::NT_OBJ_TOTAL:
		return CRSTreeCtrlImgIdx::ColumnTotalGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_REPEATER:
		return CRSTreeCtrlImgIdx::RepeaterGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_CHART:
		return CRSTreeCtrlImgIdx::ChartGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_CATEGORY:
		return CRSTreeCtrlImgIdx::ChartCategoryGlyph;
	case  CNodeTree::ENodeType::NT_OBJ_SERIES:
		return CRSTreeCtrlImgIdx::ChartSeriesGlyph;

	default:
		return CRSTreeCtrlImgIdx::NoGLyph;
	}
}

//-----------------------------------------------------------------------------
//usato quando aggiungo i base rect ma non ho ancora il ENodeType da passare alla addNode
int  CRSTreeCtrl::GetImgIndex(CObject* pObj)
{
	if (!pObj)
		return CRSTreeCtrlImgIdx::NoGLyph;

	ASSERT_VALID(pObj);

	if (pObj->IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		Repeater* pRep = (Repeater*)pObj;
		if (pRep->m_pHideExpr && !pRep->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::RepeaterHiddenExprGlyph;
		if (pRep->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::RepeaterHiddenGlyph;

		return CRSTreeCtrlImgIdx::RepeaterGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(Chart)))
	{
		Chart* pRep = (Chart*)pObj;
		if (pRep->m_pHideExpr && !pRep->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::ChartHiddenExprGlyph;
		if (pRep->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::ChartHiddenGlyph;

		return CRSTreeCtrlImgIdx::ChartGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(FileRect)))
	{
		FileRect* pFile = (FileRect*)pObj;
		if (pFile->m_pHideExpr && !pFile->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::TextFileHiddenExprGlyph;
		if (pFile->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::TextFileHiddenGlyph;

		return CRSTreeCtrlImgIdx::TextFileGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(TextRect)))
	{
		TextRect* pText = (TextRect*)pObj;
		if (pText->m_pHideExpr && !pText->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::TextHiddenExprGlyph;
		if (pText->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::TextHiddenGlyph;

		return CRSTreeCtrlImgIdx::TextGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(GraphRect)))
	{
		GraphRect* pGraph = (GraphRect*)pObj;
		if (pGraph->m_pHideExpr && !pGraph->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::ImageHiddenExprGlyph;
		if (pGraph->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::ImageHiddenGlyph;

		return CRSTreeCtrlImgIdx::ImageGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(SqrRect)))
	{
		SqrRect* pSqr = (SqrRect*)pObj;
		if (pSqr->m_pHideExpr && !pSqr->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::RectangleHiddenExprGlyph;
		if (pSqr->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::RectangleHiddenGlyph;

		return CRSTreeCtrlImgIdx::RectangleGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
	{
		Table* pTable = (Table*)pObj;
		if (pTable->m_pHideExpr && !pTable->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::TableHiddenExprGlyph;
		if (pTable->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::TableHiddenGlyph;

		return CRSTreeCtrlImgIdx::TableGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pCol = (TableColumn*)pObj;
		if (pCol->m_pHideExpr && !pCol->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::ColumnHiddenExprGlyph;
		if (pCol->IsHidden())
			return CRSTreeCtrlImgIdx::ColumnHiddenGlyph;

		return CRSTreeCtrlImgIdx::ColumnGlyph;
	}
	if (pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pField = (FieldRect*)pObj;
		if (pField->m_pHideExpr && !pField->m_pHideExpr->IsEmpty())
			return CRSTreeCtrlImgIdx::FieldGlyphHiddenExpr;
		if (pField->IsAlwaysHidden())
			return CRSTreeCtrlImgIdx::FieldGlyphHidden;

		return CRSTreeCtrlImgIdx::FieldGlyph;
	}

	return CRSTreeCtrlImgIdx::NoGLyph;
}

//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::FillLayout(CLayout* pObjects, HTREEITEM htLayout, BOOL bSkipAnchored/*=TRUE*/)
{
	if (!pObjects)
		return FALSE;
	ASSERT_VALID(pObjects);

	for (int i = 0; i < pObjects->GetCount(); i++)
	{
		BaseObj* pB = (BaseObj*)(pObjects->GetAt(i));

		if (pB->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			Repeater* pRepeater = (Repeater*)pB;

			CNodeTree& arepeaterNode = AddNode(pRepeater->GetDescription(), CNodeTree::ENodeType::NT_OBJ_REPEATER, htLayout, pRepeater, pObjects);
			arepeaterNode.SetItemFont(m_pBold);
			HTREEITEM htRepeater = arepeaterNode.m_ht;

			CLayout* pChilds = pRepeater->GetChildObjects();

			FillLayout(pChilds, htRepeater, FALSE);
		}
		else if (pB->IsKindOf(RUNTIME_CLASS(Chart)))
		{
			Chart* pChart = (Chart*)pB;

			CNodeTree& aChartNode = AddNode(pChart->GetDescription(), CNodeTree::ENodeType::NT_OBJ_CHART, htLayout, pChart, pObjects);
			aChartNode.SetItemFont(m_pBold);
			HTREEITEM htChart = aChartNode.m_ht;
			if (pChart->HasCategory() && pChart->m_pCategory)
			{
				Chart::CCategories* pCategory = pChart->m_pCategory;
				CString descr = pCategory->GetTreeNodeDescription();
				HTREEITEM htCol = AddNode(descr, CNodeTree::ENodeType::NT_OBJ_CATEGORY, htChart, pCategory, pChart, pObjects);
			}

			for (int c = 0; c < pChart->GetSeries()->GetSize(); c++)
			{
				if (!pChart->AllowMultipleSeries() && c > 0)
					break;
				Chart::CSeries* pSeries = (Chart::CSeries*)pChart->GetSeries()->GetAt(c);

				CString descr = pSeries->GetTreeNodeDescription();

				HTREEITEM htCol = AddNode(descr, CNodeTree::ENodeType::NT_OBJ_SERIES, htChart, pSeries, pChart, pObjects, pSeries->m_bHidden);
			}

			//TODO CHART
		}
		else if (pB->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			if (pB->m_bInheritByTemplate)
				continue;
			/*if (!pB->m_bPersistent)
				continue;*/
			if (pB->m_AnchorRepeaterID && bSkipAnchored)
				continue;

			CString descr = ((BaseRect*)pB)->GetDescription();

			FieldRect* pFR = dynamic_cast<FieldRect*>(pB);
			if (pFR)
			{
				if (pFR->GetInternalID() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
					continue;

				descr = pFR->GetCaption();
				if (descr.IsEmpty())
					descr = '<' + pFR->GetFieldName() + '>';
				else
					descr = '"' + descr + '"';
			}
			else if (pB->IsKindOf(RUNTIME_CLASS(TextRect)))
			{
				descr = '"' + descr + '"';
			}

			HTREEITEM htBaseRect = InsertItem(descr, GetImgIndex(pB), GetImgIndex(pB), htLayout);

			CNodeTree::ENodeType nt = CNodeTree::ENodeType::NT_WRONG;

			if (pFR)
			{
				nt = CNodeTree::ENodeType::NT_OBJ_FIELDRECT;

				//---- PROVA - il field appare anche nel gruppo delle variables
				ASSERT_VALID(m_pWDoc->m_pEditorManager);
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData());
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable());

				WoormField* pField = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable()->GetFieldByID(pFR->GetInternalID());
				if (pField)
				{
					HTREEITEM htField = AddNode(pField->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htBaseRect, pField, m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable());

					//---- PROVA - il link appare anche nel gruppo dei Links
					for (int k = 0; k < m_pWDoc->m_arWoormLinks.GetCount(); k++)
					{
						WoormLink*	pLink = m_pWDoc->m_arWoormLinks.GetAt(k);
						ASSERT_VALID(pLink);
						if (pLink->m_LinkType == WoormLink::WoormLinkType::ConnectionRadar) continue;

						if (pLink->m_strLinkOwner.CompareNoCase(pField->GetName()) != 0) continue;

						CNodeTree& nodeLink = AddNode(pLink->m_strLinkOwner, CNodeTree::ENodeType::NT_LINK, htBaseRect, pLink, &(m_pWDoc->m_arWoormLinks));
						nodeLink.SetItemFont(m_pBold);

						FillLink(nodeLink, pLink);
					}
				}
			}
			else if (pB->IsKindOf(RUNTIME_CLASS(FileRect)))
				nt = CNodeTree::ENodeType::NT_OBJ_FILERECT;
			else if (pB->IsKindOf(RUNTIME_CLASS(TextRect)))
				nt = CNodeTree::ENodeType::NT_OBJ_TEXTRECT;
			else if (pB->IsKindOf(RUNTIME_CLASS(GraphRect)))
				nt = CNodeTree::ENodeType::NT_OBJ_GRAPHRECT;
			else if (pB->IsKindOf(RUNTIME_CLASS(SqrRect)))
				nt = CNodeTree::ENodeType::NT_OBJ_SQRRECT;

			CNodeTree* pNode = new CNodeTree(htBaseRect, (CRSTreeCtrlImgIdx)GetImgIndex(pB), nt, pB, pObjects);
			m_arGarbage.Add(pNode);
			SetItemData(htBaseRect, (DWORD)pNode);
			if (!pB->m_bPersistent)
				pNode->SetItemColor(RS_COLOR_FRAMEWORK);
		}
		else if (pB->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pT = (Table*)pB;

			CNodeTree& aTableNode = AddNode(pT->GetDescription(), CNodeTree::ENodeType::NT_OBJ_TABLE, htLayout, pT, pObjects);
			aTableNode.SetItemFont(m_pBold);
			if (!pT->m_bPersistent)
				aTableNode.SetItemColor(RS_COLOR_FRAMEWORK);
			HTREEITEM htTable = aTableNode.m_ht;

			for (int c = 0; c < pT->GetColumns().GetCount(); c++)
			{
				TableColumn* pCol = pT->GetColumns()[c];

				CString descr = pCol->GetCaption();
				if (descr.IsEmpty())
					descr = '<' + pCol->GetFieldName() + '>';
				else
					descr = '"' + descr + '"';

				HTREEITEM htCol = AddNode(descr, CNodeTree::ENodeType::NT_OBJ_COLUMN, htTable, pCol, pT, pObjects, pCol->IsHidden());

				//---- PROVA - il field appare anche nel gruppo delle variables
				ASSERT_VALID(m_pWDoc->m_pEditorManager);
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData());
				ASSERT_VALID(m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable());

				WoormField* pField = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable()->GetFieldByID(pCol->GetInternalID());
				if (pField)
					AddNode(pField->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htCol, pField, m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable(), pCol);

				//caricamento totale
				if (pCol->HasTotal())
				{
					WoormTable*	pSymTable = m_pWDoc->GetEditorSymTable();
					CWordArray	idsColTotal;
					WORD		idColTotal;
					pSymTable->GetTotalOf(pCol->GetInternalID(), idsColTotal, WoormField::FIELD_COLTOTAL);

					if (idsColTotal.GetSize() > 0)
					{
						idColTotal = idsColTotal[0];
						WoormField* pTotalField = m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable()->GetFieldByID(idColTotal);
						TableCell* pTotalCell = pCol->m_pTotalCell;
						//Cella del totale
						HTREEITEM htTot = AddNode(_TB("Total"), CNodeTree::ENodeType::NT_OBJ_TOTAL, htCol, pTotalCell, m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable(), pObjects);
						//variabile totale
						AddNode(pTotalField->GetName(), CNodeTree::ENodeType::NT_VARIABLE, htTot, pTotalField, m_pWDoc->m_pEditorManager->GetPrgData()->GetSymTable(), pCol);
					}
				}

				//---- il link appare anche nel gruppo dei Links
				for (int k = 0; k < m_pWDoc->m_arWoormLinks.GetCount(); k++)
				{
					WoormLink*	pLink = m_pWDoc->m_arWoormLinks.GetAt(k);
					ASSERT_VALID(pLink);
					if (pLink->m_LinkType == WoormLink::WoormLinkType::ConnectionRadar) continue;

					if (pLink->m_strLinkOwner.CompareNoCase(pField->GetName()) != 0) continue;

					CNodeTree& nodeLink = AddNode(pLink->m_strLinkOwner, CNodeTree::ENodeType::NT_LINK, htCol, pLink, &(m_pWDoc->m_arWoormLinks));
					nodeLink.SetItemFont(m_pBold);

					FillLink(nodeLink, pLink);
				}
			}
		}
	}

	if (m_pWDoc->m_dsCurrentLayoutView.CompareNoCase(pObjects->m_strLayoutName) == 0)
	{
		Expand(htLayout, TVE_EXPAND);
	}

	return TRUE;
}

//-----------------------------------------------------------------------
void CRSTreeCtrl::UpdateRSTreeNode(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT(pNode->m_ht);

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	{
		AskDialogData*	pAskDlg = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
		if (pAskDlg)
		{
			CString sDescr = pAskDlg->GetName();

			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_ASKGROUP:
	{
		AskGroupData* pAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pItemData);
		if (pAskGroup)
		{
			CString sTitle = pAskGroup->m_strTitle;
			if (sTitle.CompareNoCase(AskGroupData::GetEmptyTitle()) == 0)
				sTitle = '<' + sTitle + '>';
			else
				sTitle = '"' + sTitle + '"';

			if (pAskGroup->m_bHiddenTitle)
				sTitle = '[' + sTitle + ']';

			SetItemText(pNode->m_ht, sTitle);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_ASKFIELD:
	{
		AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
		if (pAskField)
		{
			CString sTitle = pAskField->GetCaption();
			if (sTitle.IsEmpty())
				sTitle = '<' + _TB("Conditioned Caption") + '>';
			else
				sTitle = '"' + sTitle + '"';

			SetItemText(pNode->m_ht, sTitle);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_FIELDRECT:
	{
		FieldRect* pFR = dynamic_cast<FieldRect*>(pNode->m_pItemData);
		if (pFR)
		{
			CString descr = pFR->GetCaption();
			if (descr.IsEmpty())
				descr = '<' + pFR->GetFieldName() + '>';
			else
				descr = '"' + descr + '"';

			SetItemText(pNode->m_ht, descr);
			int imgIdx = GetImgIndex(pFR);

			IMAGEINFO* pImage = NULL;
			GetImageList(TVSIL_NORMAL)->GetImageInfo(imgIdx, pImage);

			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);
		}

		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_GRAPHRECT:
	case CNodeTree::ENodeType::NT_OBJ_SQRRECT:
	{
		BaseRect* pBR = dynamic_cast<BaseRect*>(pNode->m_pItemData);
		if (pBR)
		{
			CString descr = pBR->GetDescription();

			SetItemText(pNode->m_ht, descr);
			int imgIdx = GetImgIndex(pBR);
			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_TEXTRECT:
	case CNodeTree::ENodeType::NT_OBJ_FILERECT:
	{
		BaseRect* pBR = dynamic_cast<BaseRect*>(pNode->m_pItemData);
		if (pBR)
		{
			CString descr = pBR->GetDescription();

			SetItemText(pNode->m_ht, '"' + descr + '"');
			int imgIdx = GetImgIndex(pBR);
			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_REPEATER:
	{
		Repeater* pRep = dynamic_cast<Repeater*>(pNode->m_pItemData);
		if (pRep)
		{
			int imgIdx = GetImgIndex(pRep);
			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_COLUMN:
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
		if (pCol)
		{
			CString descr = pCol->GetCaption();
			if (descr.IsEmpty())
				descr = '<' + pCol->GetFieldName() + '>';
			else
				descr = '"' + descr + '"';

			int imgIdx = GetImgIndex(pCol);
			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);

			SetItemText(pNode->m_ht, descr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_TABLE:
	{
		Table* pTab = dynamic_cast<Table*>(pNode->m_pItemData);
		if (pTab)
		{
			CString descr = pTab->GetDescription();

			int imgIdx = GetImgIndex(pTab);
			SetItemImage(pNode, (CRSTreeCtrlImgIdx)imgIdx);

			SetItemText(pNode->m_ht, descr);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
	{
		TriggEventData*	pEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
		if (pEvent)
		{
			CString sDescr = pEvent->m_strEventName /*+ L"..."*/;
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}

	case CNodeTree::ENodeType::NT_PROCEDURE:
	{
		ProcedureObjItem*	pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
		if (pProc)
		{
			CString sDescr = pProc->GetName();
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_NAMED_QUERY:
	{
		QueryObjItem*	pQuery = dynamic_cast<QueryObjItem*>(pNode->m_pItemData);
		if (pQuery)
		{
			CString sDescr = pQuery->GetName();
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_CHART:
	{
		Chart*	pChart = dynamic_cast<Chart*>(pNode->m_pItemData);
		if (pChart)
		{
			CString sDescr = pChart->GetDescription();
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_CATEGORY:
	{
		Chart::CCategories*	pCat = dynamic_cast<Chart::CCategories*>(pNode->m_pItemData);
		if (pCat)
		{
			CString sDescr = pCat->GetTreeNodeDescription();
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		Chart::CSeries*	pSeries = dynamic_cast<Chart::CSeries*>(pNode->m_pItemData);
		if (pSeries)
		{
			CString sDescr = pSeries->GetTreeNodeDescription();
			SetItemText(pNode->m_ht, sDescr);
		}
		break;
	}
	//TODO
	}
}

//-----------------------------------------------------------------------
void CRSTreeCtrl::RemoveNode(CNodeTree* pNode)
{
	ASSERT_VALID(pNode);
	ASSERT(pNode->m_ht);

	DeleteItem(pNode->m_ht);
}

//----------------------------------------------------------------------------
BOOL CRSTreeCtrl::DeleteItem(_In_ HTREEITEM hItem)
{
	//CNodeTree* pNode = dynamic_cast<CNodeTree*>((CObject*)GetItemData(hItem));
	//if (pNode)
	//{
	//	ASSERT_VALID(pNode);
	//	pNode->Empty();
	//}

	return __super::DeleteItem(hItem);
}

//-----------------------------------------------------------------------------
void CRSTreeCtrl::OnItemDeleted(NMHDR* pNMHDR, LRESULT* pResult)
{
	NM_TREEVIEW* pNMTreeView = (NM_TREEVIEW*)pNMHDR;
	if (!pNMTreeView)
		return;

	CNodeTree* pNode = dynamic_cast<CNodeTree*> ((CObject*)(pNMTreeView->itemOld.lParam));
	if (pNode)
	{
		ASSERT_VALID(pNode);
		pNode->Empty();
	}
}
//-----------------------------------------------------------------------------
BOOL CRSTreeCtrl::SortChildren(_In_opt_ HTREEITEM hItem)
{
	ASSERT(hItem != NULL && hItem != TVI_ROOT);
	if (hItem)
		return __super::SortChildren(hItem);
	else
		return FALSE;
}

//------------------------------------------------------------------------------
void CRSTreeCtrl::RenameVariableNode(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	for (int i = 0; i < this->m_arGarbage.GetSize(); i++)
	{
		CNodeTree* pNode = dynamic_cast<CNodeTree*>(m_arGarbage[i]);

		if (!pNode || !pNode->m_pItemData)
			continue;
		ASSERT_VALID(pNode);

		if (pNode->m_NodeType != CNodeTree::ENodeType::NT_VARIABLE)
			continue;

		if (!GetParentItem(pNode->m_ht))
			continue;

		WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (!pField)
			continue;
		ASSERT_VALID(pField);

		if (pField->GetName().CompareNoCase(pszOldFieldName) == 0)
			SetItemText(pNode->m_ht, pszNewFieldName);
	}
}

//------------------------------------------------------------------------------
void CRSTreeCtrl::RenameVariableNode(WoormField* pField)
{
	ASSERT_VALID(pField);
	for (int i = 0; i < this->m_arGarbage.GetSize(); i++)
	{
		CNodeTree* pNode = dynamic_cast<CNodeTree*>(m_arGarbage[i]);

		if (!pNode || !pNode->m_pItemData)
			continue;
		ASSERT_VALID(pNode);

		if (pNode->m_NodeType != CNodeTree::ENodeType::NT_VARIABLE)
			continue;

		if (!GetParentItem(pNode->m_ht))
			continue;

		if (pField != pNode->m_pItemData)
			continue;

		SetItemText(pNode->m_ht, pField->GetName());
	}
}

//------------------------------------------------------------------------------
void CRSTreeCtrl::RemoveFieldFromItemData(LPCTSTR pszFieldName)
{
	for (int i = 0; i < this->m_arGarbage.GetSize(); i++)
	{
		CNodeTree* pNode = dynamic_cast<CNodeTree*>(m_arGarbage[i]);

		if (!pNode || !pNode->m_pItemData)
			continue;
		ASSERT_VALID(pNode);

		if (pNode->m_NodeType != CNodeTree::ENodeType::NT_VARIABLE)
			continue;

		if (!GetParentItem(pNode->m_ht))
			continue;

		WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (!pField)
			continue;
		ASSERT_VALID(pField);

		if (pField->GetName().CompareNoCase(pszFieldName) == 0)
			pNode->Empty();
	}
}

//-----------------------------------------------------------------------
HTREEITEM CRSTreeCtrl::RemoveObjectFromItemData(CObject* pItemData)
{
	HTREEITEM ht = 0;
	if (!pItemData) return ht;

	for (int i = 0; i < this->m_arGarbage.GetSize(); i++)
	{
		CNodeTree* pNode = dynamic_cast<CNodeTree*>(m_arGarbage[i]);

		if (!pNode || !pNode->m_pItemData)
			continue;
		ASSERT_VALID(pNode);

		if (pNode->m_pItemData == pItemData)
			ht = pNode->m_ht;

		if (pNode->m_pItemData == pItemData || pNode->m_pParentItemData == pItemData || pNode->m_pAncestorItemData == pItemData)
			pNode->Empty();
	}
	return ht;
}

//-----------------------------------------------------------------------
BOOL CRSTreeCtrl::OnMultiSelect()
{
	CHTreeItemsArray hItemsArray;
	GetSelectedHItems(&hItemsArray);

	if (!m_bMultiSelect ||
		hItemsArray.IsEmpty() ||
		!m_pWDoc ||
		!m_pWDoc->GetWoormFrame() ||
		!m_pWDoc->GetWoormFrame()->m_pObjectPropertyView)
		return FALSE;

	int HitemSize = hItemsArray.GetSize();

	if (HitemSize == 1)
	{
		//fix per rimanente elemento della selezione dopo la rimozione del penultimo: rimuovo tutta la selezione del tree e seleziono solo l'ultimo
		HTREEITEM selectedItem = hItemsArray.GetAt(0)->m_hItem;

		ClearSelection();


		m_pWDoc->ClearMultiSelection(this->m_bMultiSelectCustom);

		CWoormView* pWView = dynamic_cast<CWoormView*>(m_pWDoc->GetFirstView());
		//sblocco il mouse
		pWView->m_pProcessingMouse->Unlock();
		//seleziono il singolo elemento
		SelectItem(selectedItem);
		return TRUE;
	}
	if (HitemSize == 0)
		return FALSE;

	HTREEITEM hFirstItem = hItemsArray.GetAt(0)->m_hItem;

	//controllo il primo elemento della multiselezione 
	CNodeTree* pNode = (CNodeTree*)GetItemData(hFirstItem);

	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bFirstNodeIsOfLayoutType = IsLayoutNodeType(pNode->m_NodeType);

	//controllo che siano tutti di tipo compatibile altrimenti seleziono l'ultimo e basta
	for (int i = 1; i < HitemSize; i++)
	{
		HTREEITEM hCurrentItem = hItemsArray.GetAt(i)->m_hItem;
		if (!hCurrentItem)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		CNodeTree* pCurrentNode = (CNodeTree*)GetItemData(hCurrentItem);
		if (!pCurrentNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		//controlla che abbiano lo stesso padre lo stesso padre
		if (GetParentItem(hFirstItem) != GetParentItem(hCurrentItem))
			return FALSE;

		if (bFirstNodeIsOfLayoutType && IsLayoutNodeType(pCurrentNode->m_NodeType))
			continue;
		else if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO && pCurrentNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO)
			continue;
		else if (IsHiddenVariable(pNode) && IsHiddenVariable(pCurrentNode))
			continue;

		else
			// se sono arrivato qui, in questi casi, non è gestita la multiselezione
			return  FALSE;
	}

	//multiselezione degli oggetti di layout
	if (bFirstNodeIsOfLayoutType)
	{
		//guardo se sto selezionando solo colonne o oggetti "generici"
		TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);

		if (pCol)	//colonne
		{
			if (m_pWDoc->m_pMultiColumns)
				m_pWDoc->m_pMultiColumns->RemoveAllSelectedColumns();	//todo andrea: vedere se può avere senso cercare di ottimizzarlo
			else
				m_pWDoc->m_pMultiColumns = new MultiColumnSelection(m_pWDoc/*, pCol->GetTable()*/);

			//deselezione vecchio current obj (tabella)------
			CPoint pt;
			m_pWDoc->SetCurrentObj(0, pt);
			m_pWDoc->m_pCurrentObj = NULL;
			if (m_pWDoc->m_pActiveRect)
				m_pWDoc->m_pActiveRect->Clear();
			//-----------------------------------------------

			for (int i = 0; i < HitemSize; i++)
			{
				HTREEITEM hCurrentItem = hItemsArray.GetAt(i)->m_hItem;
				if (!hCurrentItem)
				{
					ASSERT(FALSE);
					return FALSE;
				}

				CNodeTree* pNode = (CNodeTree*)GetItemData(hCurrentItem);
				if (!pNode)
				{
					ASSERT(FALSE);
					return FALSE;
				}

				TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
				if (pCol)
				{
					m_pWDoc->m_pMultiColumns->AddToSelectedColumns(pCol);
					Table* pTable = pCol->GetTable();
					if (!pTable->NoActiveColumn())
						pTable->m_nActiveColumn = pTable->NO_ACTIVE_COLUMN;
				}
				else
					SetItemState(hCurrentItem, 0, TVIS_SELECTED);
			}

			// se la selezione fallisce allora rimuove anche il vettore
			if (!m_pWDoc->m_pMultiColumns->GetSize() || m_pWDoc->m_pMultiColumns->GetSize() == 0)
			{
				delete m_pWDoc->m_pMultiColumns;
				m_pWDoc->m_pMultiColumns = NULL;
			}
			else
			{
				//altrimenti se non è fallita la multiselezione carico la property grid per gli oggetti selezionati
				m_pWDoc->GetWoormFrame()->m_pObjectPropertyView->LoadMultiColumnProperties(m_pWDoc->m_pMultiColumns);
				//TODO VEDERE SE SERVE FARE LA REDRAW
				// refresh video status
				if (m_pWDoc->m_pMultiColumns)
					m_pWDoc->m_pMultiColumns->Redraw();
			}
		}
		else // altri oggetti di layout
		{
			if (m_pWDoc->m_pMultipleSelObj)
				m_pWDoc->m_pMultipleSelObj->ClearMultipleSelObjects();
			else
				m_pWDoc->m_pMultipleSelObj = new SelectionRect(m_pWDoc);

			for (int i = 0; i < HitemSize; i++)
			{
				HTREEITEM hCurrentItem = hItemsArray.GetAt(i)->m_hItem;

				if (!hCurrentItem)
				{
					ASSERT(FALSE);
					return FALSE;
				}

				CNodeTree* pNode = (CNodeTree*)GetItemData(hCurrentItem);
				if (!pNode)
				{
					ASSERT(FALSE);
					return FALSE;
				}

				BaseObj* pBaseObj = dynamic_cast<BaseObj*>(pNode->m_pItemData);
				if (pBaseObj)
					m_pWDoc->m_pMultipleSelObj->AddToSelectedObjects(pBaseObj, pBaseObj->GetBaseRect());
				else
					SetItemState(hCurrentItem, 0, TVIS_SELECTED);
			}

			// se la selezione fallisce allora rimuove anche il vettore
			if (!m_pWDoc->m_pMultipleSelObj->GetSize() || m_pWDoc->m_pMultipleSelObj->GetSize() == 0)
			{
				delete m_pWDoc->m_pMultipleSelObj;
				m_pWDoc->m_pMultipleSelObj = NULL;
			}
			else
			{
				//altrimenti se non è fallita la multiselezione carico la property grid per gli oggetti selezionati
				m_pWDoc->GetWoormFrame()->m_pObjectPropertyView->LoadMultipleSelectionProperties(m_pWDoc->m_pMultipleSelObj);
				// refresh video status
				if (m_pWDoc->m_pMultipleSelObj)
					m_pWDoc->m_pMultipleSelObj->Redraw();
			}
		}
	}
	//multiselezione delle colonne del database
	else if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO || IsHiddenVariable(pNode))
	{
		if (m_pWDoc->m_pNodesSelection)
			m_pWDoc->m_pNodesSelection->RemoveAll();
		else
			m_pWDoc->m_pNodesSelection = new CNodeTreeArray();

		for (int i = 0; i < HitemSize; i++)
		{
			HTREEITEM hCurrentItem = hItemsArray.GetAt(i)->m_hItem;

			if (!hCurrentItem)
			{
				ASSERT(FALSE);
				return FALSE;
			}

			CNodeTree* pNode = (CNodeTree*)GetItemData(hCurrentItem);
			if (!pNode)
			{
				ASSERT(FALSE);
				return FALSE;
			}

			m_pWDoc->m_pNodesSelection->AddUnique(pNode);

			// se la selezione fallisce allora rimuove anche il vettore
			if (!m_pWDoc->m_pNodesSelection->GetSize())
			{
				delete m_pWDoc->m_pNodesSelection;
				m_pWDoc->m_pNodesSelection = NULL;
			}
		}
	}

	return TRUE;
}

//=============================================================================
IMPLEMENT_DYNAMIC(CRSDockPane, CTaskBuilderDockPane)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRSDockPane, CTaskBuilderDockPane)

	ON_WM_HELPINFO()

END_MESSAGE_MAP()

CRSDockPane::CRSDockPane(CRuntimeClass* rc)
	:
	CTaskBuilderDockPane(rc)
{
	SetMinWidth(40);
}

CRSDockPane::~CRSDockPane()
{
}

//-----------------------------------------------------------------------------
void CRSDockPane::PrepareForClose()
{
	//OnDeactivateFrame(CBRS_RIGHT);

	//ShowWindow(SW_HIDE);
	//PostMessage(WM_CLOSE,0,0);

	//delete this;
	CBCGPDockManager* pDockManager = globalUtils.GetDockManager(this);
	pDockManager;
}

//-----------------------------------------------------------------------------
void CRSDockPane::OnDeactivateFrame(DWORD dw)
{
	if (::IsWindow(m_hWnd) && (IsFloating() || IsFloatingMulti()))
	{
		DockToFrameWindow(dw);
	}
}


//-----------------------------------------------------------------------------
BOOL CRSDockPane::OnHelpInfo(HELPINFO* pHelpInfo)
{
	if (!m_sNsHelp.IsEmpty())
	{
		CString sh(RS_HELP_MAIN);	//szWoormNamespace

		sh += '.' + m_sNsHelp;

		::ShowHelp(sh);
		return TRUE;
	}

	if (!GetDocument())
		return FALSE;

	return GetDocument()->GetWoormFrame()->OnHelpInfo(pHelpInfo);
}

//-----------------------------------------------------------------------------
void CRSDockPane::OnCustomizeToolbar()
{
	ASSERT_VALID(m_pToolBar);
	m_pToolBar->SuspendLayout();

	m_pToolBar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
	m_pToolBar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());
	m_pToolBar->SetTextColor(AfxGetThemeManager()->GetDialogToolbarTextColor());
	m_pToolBar->SetTextColorHighlighted(AfxGetThemeManager()->GetDialogToolbarTextHighlightedColor());
	m_pToolBar->SetHighlightedColor(AfxGetThemeManager()->GetDialogToolbarHighlightedColor());

	OnAddToolbarButtons();

	if (GetDocument() && GetDocument()->GetWoormFrame())
		m_pToolBar->EnableTextLabels(FALSE); // remove the text in toolbar

	m_pToolBar->SetAutoHideToolBarButton(TRUE);

	m_pToolBar->ResumeLayout(TRUE);
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSEngineDockPane, CRSFullReportDockPane)

CRSEngineDockPane::~CRSEngineDockPane()
{
}

CRSEngineDockPane::CRSEngineDockPane()
	:
	CRSFullReportDockPane(RUNTIME_CLASS(CRSEngineTreeView))
{
	m_sNsHelp = RS_HELP_PANEL_ENGINE;
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSLayoutDockPane, CRSFullReportDockPane)

CRSLayoutDockPane::CRSLayoutDockPane()
	:
	CRSFullReportDockPane(RUNTIME_CLASS(CRSLayoutTreeView))
{
	m_sNsHelp = RS_HELP_PANEL_LAYOUT;
}

CRSLayoutDockPane::~CRSLayoutDockPane()
{
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSFullReportDockPane, CRSDockPane)

CRSFullReportDockPane::CRSFullReportDockPane()
	:
	CRSDockPane(RUNTIME_CLASS(CRSReportTreeView))
{
	m_sNsHelp = RS_HELP_PANEL_REPORT;
}

CRSFullReportDockPane::CRSFullReportDockPane(CRuntimeClass* rc)
	:
	CRSDockPane(rc)
{
}

CRSFullReportDockPane::~CRSFullReportDockPane()
{
}

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRSFullReportDockPane, CRSDockPane)

	ON_BN_CLICKED(ID_RS_REFRESH, OnRefresh)
	ON_BN_CLICKED(ID_RS_EDIT, OnOpenEditor)
	ON_BN_CLICKED(ID_RS_NEW, OnNew)
	ON_BN_CLICKED(ID_RS_MORE, OnMore)
	ON_BN_CLICKED(ID_RS_DELETE, OnDelete)
	ON_BN_CLICKED(ID_RS_UP, OnUp)
	ON_BN_CLICKED(ID_RS_DOWN, OnDown)
	ON_BN_CLICKED(ID_RS_DLGPREVIEW, OnDialogPreview)

	ON_BN_CLICKED(ID_RS_COLLAPSEALLTREE, OnCollapseAll)
	ON_BN_CLICKED(ID_RS_EXPANDALLTREE, OnExpandAll)

	ON_BN_CLICKED(ID_RS_CHECK_TABLE_FROM_DB, OnCheckAddTable)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_EDIT, RS_DOCK_NS, TBIcon(szIconEdit, TOOLBAR), _TB("Edit"), _TB("\nEdit current node"));
	m_pToolBar->AddButton(ID_RS_NEW, RS_DOCK_NS, TBIcon(szIconAdd, TOOLBAR), _TB("Add"), _TB("\nAdd new element"));
	m_pToolBar->AddButton(ID_RS_UP, RS_DOCK_NS, TBIcon(szIconUp, TOOLBAR), _TB("Up"), _TB("\nMove to previous/left position"));
	m_pToolBar->AddButton(ID_RS_DOWN, RS_DOCK_NS, TBIcon(szIconDown, TOOLBAR), _TB("Down"), _TB("\nMove to next/right position"));

	m_pToolBar->AddButton(ID_RS_MORE, RS_DOCK_NS, TBIcon(szIconMore, TOOLBAR), _TB("Show All"), _TB("\nShow all variables grouped by type"));
	m_pToolBar->PressButton(ID_RS_MORE, TRUE, FALSE);

	m_pToolBar->AddButton(ID_RS_CHECK_TABLE_FROM_DB, RS_DOCK_NS, TBIcon(szIconTable, TOOLBAR), _TB("New Table"), _TB("\nDrag hidden variables to create a new Table"));
	m_pToolBar->PressButton(ID_RS_CHECK_TABLE_FROM_DB, TRUE, FALSE);

	m_pToolBar->AddButton(ID_RS_DLGPREVIEW, RS_DOCK_NS, TBIcon(szIconDialogPreview, TOOLBAR), _TB("Preview"), _TB("Opens the dialog preview"));

	m_pToolBar->AddButton(ID_RS_DELETE, RS_DOCK_NS, TBIcon(szIconDelete, TOOLBAR), _TB("Delete"), _TB("\nDelete the selected object"));
	m_pToolBar->AddSeparatorBefore(ID_RS_DELETE);

	m_pToolBar->AddButtonToRight(ID_RS_REFRESH, RS_DOCK_NS, TBIcon(szIconRefresh, TOOLBAR), _TB("Refresh"), _TB("\nRefresh tree branch"));
	m_pToolBar->AddButtonToRight(ID_RS_COLLAPSEALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeCollapseAll, TOOLBAR), _TB("Collapse All"), _TB("\nnCollapse all tree"));
	m_pToolBar->AddButtonToRight(ID_RS_EXPANDALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeExpand, TOOLBAR), _TB("Expand current node"), _TB("\nExpand all children of current node"));
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnRefresh()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnRefresh();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnOpenEditor()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnOpenEditor();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnNew()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnNew();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnMore()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnMore();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnDelete()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnDelete();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnUp()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnUp();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnDown()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnDown();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnCollapseAll()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnCollapseAll();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnExpandAll()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnExpandAll();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnCheckAddTable()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnCheckAddTable();
}

//-----------------------------------------------------------------------------
void CRSFullReportDockPane::OnDialogPreview()
{
	CRSReportTreeView* reportTreeView = (CRSReportTreeView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSReportTreeView));
	if (!reportTreeView)
		return;
	reportTreeView->OnDialogPreview();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSObjectPropertyDockPane, CRSDockPane)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRSObjectPropertyDockPane, CRSDockPane)

	ON_BN_CLICKED(ID_RS_COLLAPSEALLTREE, OnCollapseAll)
	ON_BN_CLICKED(ID_RS_EXPANDALLTREE, OnExpand)

	ON_BN_CLICKED(ID_RS_SAVE, OnApply)
	//ON_BN_CLICKED(ID_RS_DISCARD,		OnDiscard)
	ON_BN_CLICKED(ID_RS_REFRESH, OnRefresh)

	ON_BN_CLICKED(ID_RS_LAYOUT, OnLayoutBtn)
	ON_BN_CLICKED(ID_RS_VARIABLE, OnVariableBtn)
	ON_BN_CLICKED(ID_RS_FINDRULE, OnFindRuleBtn)
	ON_BN_CLICKED(ID_RS_REQUESTFIELD, OnRequestFieldBtn)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRSObjectPropertyDockPane::CRSObjectPropertyDockPane()
	:
	CRSDockPane(RUNTIME_CLASS(CRS_ObjectPropertyView))
{
	m_sNsHelp = RS_HELP_PANEL_PROPERTY;
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_SAVE, RS_DOCK_NS, TBIcon(szIconSave2, TOOLBAR), _TB("Apply"));
	m_pToolBar->AddButton(ID_RS_LAYOUT, RS_DOCK_NS, TBIcon(szIconColors, TOOLBAR), _TB("Layout"), _TB("Link to layout object properties"));
	m_pToolBar->AddButton(ID_RS_VARIABLE, RS_DOCK_NS, TBIcon(szIconRSFuncExprField, TOOLBAR), _TB("Variable"), _TB("Link to internal data properties"));
	m_pToolBar->AddButton(ID_RS_REQUESTFIELD, RS_DOCK_NS, TBIcon(szIconRequestField, TOOLBAR), _TB("Request Field"), _TB("Link to request field properties"));
	m_pToolBar->AddButton(ID_RS_FINDRULE, RS_DOCK_NS, TBIcon(szIconTreeSearch, TOOLBAR), _TB("Find Rule"), _TB("Ensure the variable visibility in the Report tree"));

	m_pToolBar->AddButtonToRight(ID_RS_COLLAPSEALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeCollapseAll, TOOLBAR), _TB("Collapse All"), _TB("\nCollapse all properties"));
	m_pToolBar->AddButtonToRight(ID_RS_EXPANDALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeExpand, TOOLBAR), _TB("Expand current"), _TB("\nExpand current property"));
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnCollapseAll()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnCollapseAll();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnExpand()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnExpand();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnApply()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnApply();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnDiscard()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnDiscard();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnRefresh()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnRefresh();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnLayoutBtn()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnLayoutBtn();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnVariableBtn()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnVariableBtn();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnFindRuleBtn()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnFindRuleBtn();
}

//-----------------------------------------------------------------------------
void CRSObjectPropertyDockPane::OnRequestFieldBtn()
{
	CRS_ObjectPropertyView* objPropView = (CRS_ObjectPropertyView*)GetFormWnd(RUNTIME_CLASS(CRS_ObjectPropertyView));
	if (!objPropView)
		return;
	objPropView->OnRequestFieldBtn();
}

//=============================================================================
IMPLEMENT_DYNAMIC(CRSDockedView, CAbstractFormView)

BEGIN_MESSAGE_MAP(CRSDockedView, CAbstractFormView)

	ON_WM_MOUSEACTIVATE()

END_MESSAGE_MAP()

CRSDockedView::CRSDockedView(const CString& sName, UINT nIDD)
	:
	CAbstractFormView(sName, nIDD)
{
}

//------------------------------------------------------------------------------
int CRSDockedView::OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message)
{
	try {
		CFrameWnd* pParentFrame = GetParentFrame();
		if (pParentFrame && pDesktopWnd)
		{
			// eat it if this will cause activation
			if (pParentFrame != pDesktopWnd && !pDesktopWnd->IsChild(pParentFrame))
				return CWnd::OnMouseActivate(pDesktopWnd, nHitTest, message);
		}

		return __super::OnMouseActivate(pDesktopWnd, nHitTest, message);
	}
	catch (...)
	{
		return 0;
	}
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSReportTreeView, CRSDockedView)

BEGIN_MESSAGE_MAP(CRSReportTreeView, CRSDockedView)

	ON_NOTIFY(NM_DBLCLK, IDC_RS_Tree, OnDblclkTree)
	ON_NOTIFY(TVN_SELCHANGING, IDC_RS_Tree, OnSelchangingTree)
	ON_NOTIFY(TVN_SELCHANGED, IDC_RS_Tree, OnSelchangedTree)

	ON_UPDATE_COMMAND_UI(ID_RS_REFRESH, OnUpdateRefresh)
	ON_UPDATE_COMMAND_UI(ID_RS_EDIT, OnUpdateEdit)
	ON_UPDATE_COMMAND_UI(ID_RS_NEW, OnUpdateNew)
	ON_UPDATE_COMMAND_UI(ID_RS_MORE, OnUpdateMore)
	ON_UPDATE_COMMAND_UI(ID_RS_DELETE, OnUpdateDelete)

	ON_UPDATE_COMMAND_UI(ID_RS_UP, OnUpdateUpDown)
	ON_UPDATE_COMMAND_UI(ID_RS_DOWN, OnUpdateUpDown)

	ON_UPDATE_COMMAND_UI(ID_RS_DLGPREVIEW, OnUpdateDialogPreview)

	ON_UPDATE_COMMAND_UI(ID_VK_LEFT, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_RIGHT, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_UP, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_DOWN, OnUpdateVKMove)

	ON_COMMAND(ID_VK_UP, OnVKUp)
	ON_COMMAND(ID_VK_DOWN, OnVKDown)
	ON_COMMAND(ID_VK_LEFT, OnVKLeft)
	ON_COMMAND(ID_VK_RIGHT, OnVKRight)

	ON_UPDATE_COMMAND_UI(ID_RS_COLLAPSEALLTREE, OnUpdateCollapseAll)
	ON_UPDATE_COMMAND_UI(ID_RS_EXPANDALLTREE, OnUpdateExpandAll)

	ON_UPDATE_COMMAND_UI(ID_RS_CHECK_TABLE_FROM_DB, OnUpdateCheckAddTable)

	ON_COMMAND(IDC_RS_Tree_Finder, OnFindTree)

END_MESSAGE_MAP()

CRSReportTreeView::CRSReportTreeView()
	:
	CRSDockedView(L"Report", IDD_RS_ReportTreeView)
{
	m_TreeCtrl.EnableDrag(DRAGDROP_WOORM_ENGINE_VIEW);
}

CRSReportTreeView::CRSReportTreeView(const CString& sName, UINT nIDD)
	:
	CRSDockedView(sName, nIDD)
{
}

CRSReportTreeView::~CRSReportTreeView()
{

}

//------------------------------------------------------------------------------
BOOL CRSReportTreeView::PreTranslateMessage(MSG* pMsg)
{
	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN)
	{
		OnDelete();
		return TRUE;
	}
	return __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	m_DropTarget.Register(this);
	SetScrollSizes(MM_TEXT, CSize(0, 0));
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSReportTreeView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pFullReportPane->GetToolBar();
}

// -----------------------------------------------------------------------------
CWoormDocMng* CRSReportTreeView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// -----------------------------------------------------------------------------
CRSEditView* CRSReportTreeView::CreateEditView()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument())
		return NULL;
	if (!GetDocument()->GetWoormFrame())
		return NULL;

	CRSEditView* pEdtView = GetDocument()->GetWoormFrame()->CreateEditView();
	return pEdtView;
}

// -----------------------------------------------------------------------------
void CRSReportTreeView::BuildDataControlLinks()
{
	ModifyStyle(0, WS_CLIPCHILDREN);

	CWnd* pWnd = GetDlgItem(IDC_RS_Tree);
	if (pWnd)
		pWnd->Detach();
	m_TreeCtrl.SubclassDlgItem(IDC_RS_Tree, this);
	m_TreeCtrl.Attach(GetDocument());
	m_TreeCtrl.InitializeImageList();

	pWnd = GetDlgItem(IDC_RS_Tree_Finder);
	if (pWnd)
		pWnd->Detach();
	m_edtFinder.SubclassEdit(IDC_RS_Tree_Finder, this);
	m_edtFinder.EnableFindBrowseButton(TRUE, L"", TRUE);
	m_edtFinder.SetPrompt(_TB("Search node..."));

	OnBuildDataControlLinks();
}

// -----------------------------------------------------------------------------
void CRSReportTreeView::OnBuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->SetReportTreeView(this);

	FillTree();
}

// ---------------------------------------------------------------------------- -
void CRSReportTreeView::OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;

	OnOpenEditor();
}

// -----------------------------------------------------------------------------
void CRSReportTreeView::OnFindTree()
{
	CString sMatchText;
	this->m_edtFinder.GetWindowText(sMatchText);
	sMatchText.Trim();
	if (sMatchText.IsEmpty())
		return;

	m_TreeCtrl.SelectRSTreeItemByMatchingText(sMatchText);
}

//-----------------------------------------------------------------------------
BOOL CRSReportTreeView::CanOpenEditor(CNodeTree* pNode)
{
	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_PROCEDURE:
	case CNodeTree::ENodeType::NT_NAMED_QUERY:

	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
	case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_LOOP:

	case CNodeTree::ENodeType::NT_RULE_QUERY_WHERE:
	case CNodeTree::ENodeType::NT_RULE_QUERY_HAVING:
	case CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON:

	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY:
	case CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY:

	case CNodeTree::ENodeType::NT_EXPR:
	case CNodeTree::ENodeType::NT_BLOCK:

	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:

	case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
	case CNodeTree::ENodeType::NT_TUPLE_FILTER:
	case CNodeTree::ENodeType::NT_TUPLE_GROUPING:
	case CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS:
	case CNodeTree::ENodeType::NT_TUPLE_HAVING_FILTER:

		return TRUE;

	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField*  pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pF);
		if (!pF) return FALSE;

		if (pF->IsTableRuleField() && pF->IsNativeColumnExpr())
		{
			return TRUE;
		}
		else if (pF->IsExprRuleField())
		{
			return TRUE;
		}
		return FALSE;
	}
	default:;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnOpenEditor()
{
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = m_TreeCtrl.GetNode(hCurrentItem);

	OnOpenEditor(pNode);
}

void CRSReportTreeView::OnOpenEditor(CNodeTree* pNode)
{
	if (!pNode)
	{
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	if (!CanOpenEditor(pNode))
		return;
	//--------------------------------------

	CRSEditView* pEdtView = CreateEditView(); //dynamic_cast<CRSEditViewDebug*>(GetDocument()->CreateSlaveView(RUNTIME_CLASS(CRSEditViewDebug)));//
	if (!pEdtView)
		return;

	TDisposablePtr<CNodeTree> spNode = pNode;
	//pEdtView->HideControl(pEdtView->GetDialogID(), TRUE);
	pEdtView->LoadElementFromTree(pNode);
	pEdtView->DoEvent();

	if (spNode)
	{
		if (
			GetDocument() && GetDocument()->GetWoormFrame() &&
			::IsWindow(GetDocument()->GetWoormFrame()->m_hWnd) &&
			GetDocument()->GetWoormFrame()->m_pEditorDockedView
			)
		{
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(spNode);
			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(spNode);
		}
	}
}

//-----------------------------------------------------------------------
void CRSReportTreeView::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	ASSERT_VALID(GetDocument());
	ASSERT_VALID(GetDocument()->GetWoormFrame());

	if (!m_TreeCtrl.m_bPassive)
		GetDocument()->m_pActiveRect->Clear();

	*pResult = 0;

	if (m_TreeCtrl.m_bDeleting)
		return;

	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;

	SAFE_DELETE(this->GetDocument()->m_pNodesSelection);	/*MULTISEL*/
	SAFE_DELETE(this->GetDocument()->m_pMultiColumns);

	//imposto i bottoni nella toolbar relativi ai field rect invece che alle colonne
	ChangeFieldsButtons(FALSE);

	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	if (!hCurrentItem)
	{
		if (GetDocument()->GetWoormFrame()->m_pObjectPropertyView)
			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);

	if (!pNode)
	{
		if (GetDocument()->GetWoormFrame()->m_pObjectPropertyView)
			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();
		return;
	}

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);
	/* MULTISEL
		if (this->GetDocument()->m_pNodesSelection)
		{
			if (!this->GetDocument()->m_pNodesSelection->CheckOneSel(pNode))
				SAFE_DELETE(this->GetDocument()->m_pNodesSelection);
		}
	*/
	if (!m_TreeCtrl.m_bPassive)
	{
		CWoormView* pWView = dynamic_cast<CWoormView*>(GetDocument()->GetFirstView());
		if (pWView)
		{
			if (m_TreeCtrl.IsLayoutNodeType(pNode->m_NodeType))
			{
				if (GetDocument()->m_Layouts.GetCount() > 1)
				{
					CLayout* pLayout = dynamic_cast<CLayout*>(pNode->m_pParentItemData);
					if (!pLayout)
						pLayout = dynamic_cast<CLayout*>(pNode->m_pAncestorItemData);
					if (!pLayout)
					{
						TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
						if (pCol)
						{
							HTREEITEM htTable = m_TreeCtrl.GetParentItem(pNode->m_ht);
							ASSERT(htTable);
							CNodeTree* pTableNode = dynamic_cast<CNodeTree*>((CNodeTree*)m_TreeCtrl.GetItemData(htTable));
							ASSERT_VALID(pTableNode);
							pLayout = dynamic_cast<CLayout*>(pTableNode->m_pParentItemData);
						}
					}
					ASSERT_VALID(pLayout);
					if (pLayout && pLayout != &GetDocument()->GetObjects())
					{
						GetDocument()->ChangeLayout(pLayout->m_strLayoutName);
					}
				}

				m_TreeCtrl.m_bPassive = TRUE;
				pWView->OnObjectsSelChanged(pNode->m_pItemData);
				m_TreeCtrl.m_bPassive = FALSE;
				GetDocument()->m_pActiveRect->EnsureVisible();
			}
		}
	}

	if (pNode->m_NodeType == CNodeTree::ENodeType::NT_LAYOUT)
	{
		CLayout*	pLayout = dynamic_cast<CLayout*>(pNode->m_pItemData);

		GetDocument()->ChangeLayout(pLayout->m_strLayoutName);
		if (GetDocument()->GetWoormFrame()->m_pObjectPropertyView)
			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);
	}

	if (!GetDocument()->m_pMultipleSelObj)
	{
		if (GetDocument()->GetWoormFrame()->m_pObjectPropertyView)
			//carico la property grid per un oggetto solo se ho la multiselezione disattivata
			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->LoadPropertyGrid(pNode);

		//carico l'editview docked (se serve)
		if (GetDocument()->GetWoormFrame()->m_pEditorDockedView)
		{
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->GetEditCtrl()->ColorVariables(GetDocument(), FALSE);
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(pNode);
		}
	}

	if (pNode->m_NodeType == CNodeTree::ENodeType::NT_OBJ_TABLE || pNode->m_NodeType == CNodeTree::ENodeType::NT_OBJ_COLUMN)
		ChangeFieldsButtons(TRUE);

	//traduzione campi
	switch (pNode->m_NodeType)
	{
	case  CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO:
	{
		SqlTableInfo*  pSqlTableInfo = dynamic_cast<SqlTableInfo*>(pNode->m_pItemData);
		if (pSqlTableInfo)
		{
			CString sName = pSqlTableInfo->GetTableName();
			CString sNameTrad = AfxLoadDatabaseString(sName, sName);
			GetDocument()->SetMessageInStatusBar(sNameTrad, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
		}

		break;
	}
	case  CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* field = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (!field)
			return;
		ASSERT_VALID(field);
		if (field->IsTableRuleField())
		{
			HTREEITEM parentHt = this->m_TreeCtrl.GetParentItem(pNode->m_ht);
			CNodeTree* pParentNode = (CNodeTree*)m_TreeCtrl.GetItemData(parentHt);

			SqlTableInfo* pSqlTableInfo = dynamic_cast<SqlTableInfo*>(pParentNode->m_pItemData);
			if (!pSqlTableInfo)
			{
				//sto selezionando una variabile dal pannello del layout e allora recupero il parent nelle rules
				parentHt = this->m_TreeCtrl.GetParentItem(GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, field));
				pParentNode = (CNodeTree*)m_TreeCtrl.GetItemData(parentHt);
				pSqlTableInfo = dynamic_cast<SqlTableInfo*>(pParentNode->m_pItemData);
			}
			//rivaluto il puntatore
			if (pSqlTableInfo)
			{
				const SqlColumnInfoObject* pSqlColumnInfo = pSqlTableInfo->GetColumnInfo(field->GetPhysicalName());
				if (pSqlColumnInfo)
				{
					CString sNameTrad = pSqlColumnInfo->GetColumnTitle();
					CString	strBuffer(pSqlColumnInfo->GetDataObjType().ToString());
					strBuffer += cwsprintf(_T("( %d"), pSqlColumnInfo->GetColumnLength());
					if (pSqlColumnInfo->GetColumnDecimal() > 0)
						strBuffer += cwsprintf(_T(", %d"), pSqlColumnInfo->GetColumnDecimal());
					strBuffer += _T(" )");

					GetDocument()->SetMessageInStatusBar(sNameTrad + L", " + strBuffer, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
				}
			}
		}
		break;
	}
	case  CNodeTree::ENodeType::NT_LIST_DBTABLE:
	{
		SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pItemData);
		if (pCatEntry)
		{
			CString sNameTrad = AfxLoadDatabaseString(pCatEntry->m_strTableName, pCatEntry->m_strTableName);
			GetDocument()->SetMessageInStatusBar(sNameTrad, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
		}
		break;
	}
	case  CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
	{
		const SqlColumnInfoObject* pCol = (SqlColumnInfoObject*)(pNode->m_pItemData);
		if (pCol)
		{
			CString sNameTrad = pCol->GetColumnTitle();
			CString	strBuffer(pCol->GetDataObjType().ToString());
			strBuffer += cwsprintf(_T("( %d"), pCol->GetColumnLength());
			if (pCol->GetColumnDecimal() > 0)
				strBuffer += cwsprintf(_T(", %d"), pCol->GetColumnDecimal());
			strBuffer += _T(" )");

			GetDocument()->SetMessageInStatusBar(sNameTrad + L", " + strBuffer, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
		}
		break;
	}
	default:
		GetDocument()->ClearMessageInStatusBar();
		break;
	}
}

//-----------------------------------------------------------------------
//Cambio l'img dei pulsanti relativi alla creazione di field o colonne (da db e derivati da funzioni o espressioni)
//e quello per rimuovere l'oggetto grafico di una variabile che verrà messa nell'elenco di quelle nascoste
void CRSReportTreeView::ChangeFieldsButtons(BOOL isActiveColumn)
{
	GetDocument()->GetWoormFrame()->ChangeFieldsButtons(isActiveColumn);
}

//-----------------------------------------------------------------------
// Mi serve per aggiornare il dato all'interno del contesto perche`
// il killfocus arriva dopo la selezione del tree
void CRSReportTreeView::OnSelchangingTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;

	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	LPARAM lParamCurrentItem = pNMTreeView->itemNew.lParam;

	// autorizzo la prosecuzione della selezione
	*pResult = FALSE;
}

//-----------------------------------------------------------------------
BOOL CRSReportTreeView::SelectLayoutObject(CObject* pObj, BOOL bPassive/* = TRUE*/)
{
	if (!pObj)
	{
		m_TreeCtrl.m_bPassive = bPassive;
		m_TreeCtrl.SelectItem(NULL);
		m_TreeCtrl.ClearSelection();
		m_TreeCtrl.m_bPassive = FALSE;
		GetDocument()->m_pActiveRect->Clear();
		return TRUE;
	}
	if (!dynamic_cast<TotalCell*>(pObj) && pObj->GetRuntimeClass() == RUNTIME_CLASS(TableCell))
	{
		ASSERT_VALID(GetDocument());
		ASSERT_VALID(GetDocument()->GetWoormFrame());
		ASSERT_VALID(GetDocument()->GetWoormFrame()->m_pObjectPropertyView);
		if (GetDocument()->GetWoormFrame()->m_pObjectPropertyView)
		{
			TableCell* pTCell = dynamic_cast<TableCell*>(pObj);

			pTCell->m_pColumn->GetTable()->m_nActiveRow = pTCell->m_nCurrRow;

			GetDocument()->GetWoormFrame()->m_pObjectPropertyView->LoadTableCellProperties(pTCell);

			return TRUE;
		}
	}
	HTREEITEM ht = m_TreeCtrl.FindItemData((DWORD)pObj, m_TreeCtrl.m_htLayouts);
	if (ht)
	{
		m_TreeCtrl.m_bPassive = bPassive;
		m_TreeCtrl.SelectItem(ht);
		m_TreeCtrl.EnsureVisible(ht);
		m_TreeCtrl.m_bPassive = FALSE;
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------
void CRSReportTreeView::RemoveNode(CNodeTree* pNode)
{
	this->m_TreeCtrl.RemoveNode(pNode);
}

//------------------------------------------------------------------------------
void CRSReportTreeView::OnCheckAddTable()
{
	m_bAddTable = !m_bAddTable;

	GetToolBar()->CheckButton(ID_RS_CHECK_TABLE_FROM_DB, m_bAddTable);
}

// -----------------------------------------------------------------------------
BOOL CRSReportTreeView::FillTree()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument()->m_bAllowEditing)
		return FALSE;

	CWaitCursor wc;


	/*if (!m_TreeCtrl.m_htProperties)
		m_TreeCtrl.m_htProperties = m_TreeCtrl.AddNode(_T("Properties"), CNodeTree::ENodeType::NT_PROPERTIES, NULL, GetDocument()->m_pDocProperties);
	if (!m_TreeCtrl.m_htPageInfo)
		m_TreeCtrl.m_htPageInfo = m_TreeCtrl.AddNode(_T("Page info"), CNodeTree::ENodeType::NT_PAGE, NULL, &GetDocument()->m_PageInfo);*/

	m_TreeCtrl.FillLayouts();
	m_TreeCtrl.Expand(m_TreeCtrl.m_htLayouts, TVE_EXPAND);
	//m_TreeCtrl.Expand(m_TreeCtrl.m_htLayoutDefault, TVE_EXPAND);

	m_TreeCtrl.FillLinks();
	//-----------------------------------

	m_TreeCtrl.FillVariables(TRUE, FALSE, TRUE, FALSE, FALSE);
	m_TreeCtrl.FillRules();
	m_TreeCtrl.FillTupleRules();
	m_TreeCtrl.FillEvents();
	m_TreeCtrl.FillProcedures();
	m_TreeCtrl.FillQueries();
	m_TreeCtrl.FillDialogs();

	/*if (!m_TreeCtrl.m_htSettings)
		m_TreeCtrl.m_htSettings = m_TreeCtrl.AddNode(_T("General Settings"), CNodeTree::ENodeType::NT_SETTINGS, NULL, GetDocument()->m_pWoormIni);*/

	return TRUE;
}

// ---------------------------------------------------------------------------- -
void CRSReportTreeView::OnUpdateCheckAddTable(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (!pField)
			break;

		bEnable = pField->IsHidden();
		break;
	}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnRefresh()
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
	case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
	case CNodeTree::ENodeType::NT_LAYOUT:
		m_TreeCtrl.FillLayouts();
		break;
	case CNodeTree::ENodeType::NT_ROOT_LINKS:
		m_TreeCtrl.FillLinks();
		break;
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
		m_TreeCtrl.FillVariables(TRUE, FALSE, TRUE, FALSE, FALSE);
		break;
	case CNodeTree::ENodeType::NT_ROOT_RULES:
		m_TreeCtrl.FillRules();
		break;
	case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
		m_TreeCtrl.FillTupleRules();
		break;
	case CNodeTree::ENodeType::NT_ROOT_EVENTS:
		m_TreeCtrl.FillEvents();
		break;
	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
		m_TreeCtrl.FillProcedures();
		break;
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:
		m_TreeCtrl.FillQueries();
		break;
	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
		m_TreeCtrl.FillDialogs();
		break;
	}
	m_TreeCtrl.Expand(hCurrentItem, TVE_EXPAND);
}

void CRSReportTreeView::OnUpdateRefresh(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
	case CNodeTree::ENodeType::NT_LAYOUT:
	case CNodeTree::ENodeType::NT_ROOT_LINKS:
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
	case CNodeTree::ENodeType::NT_ROOT_RULES:
	case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
	case CNodeTree::ENodeType::NT_ROOT_EVENTS:
	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:
	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:

		bEnable = TRUE;
		break;
	}
	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnNew()
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

	ASSERT_VALID(GetDocument());
	ASSERT_VALID(GetDocument()->GetWoormFrame());
	ASSERT_VALID(GetDocument()->GetWoormFrame()->m_pObjectPropertyView);

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
	case CNodeTree::ENodeType::NT_ROOT_RULES:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM:

	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:
	case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:
	case CNodeTree::ENodeType::NT_ASKGROUP:

	case CNodeTree::ENodeType::NT_LINK_PARAMETERS:
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewObjectPropertyGrid(pNode);
		break;
	}
	case CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS:
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewBreakingEvent(pNode, TRUE, TRUE, TRUE);
		break;
	}
	case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewBreakingEvent(pNode, TRUE, FALSE);
		break;
	}
	case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewBreakingEvent(pNode, FALSE, TRUE);
		break;
	}

	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
	{
		WoormTable*	pSymTable = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable();

		AskRuleData* pAskDialogs = dynamic_cast<AskRuleData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskDialogs);

		AskDialogData* pNewAskDlg = new AskDialogData(*pSymTable, GetDocument());
		pNewAskDlg->SetName(pAskDialogs->GetAdviseName(NULL));
		pNewAskDlg->SetTitle(AskDialogData::GetEmptyTitle());
		pAskDialogs->Add(pNewAskDlg);

		AskGroupData* pNewAskGroup = new AskGroupData(*pSymTable, GetDocument());
		pNewAskGroup->SetTitle(AskGroupData::GetEmptyTitle());
		pNewAskDlg->AddAskGroup(pNewAskGroup);

		m_TreeCtrl.FillDialogs();

		HTREEITEM htNewGrp = this->m_TreeCtrl.FindItemData((DWORD)pNewAskGroup, m_TreeCtrl.m_htAskDialogs);
		ASSERT(htNewGrp);
		this->m_TreeCtrl.SelectItem(htNewGrp);

		CNodeTree* pGrp = (CNodeTree*)m_TreeCtrl.GetItemData(htNewGrp);
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewObjectPropertyGrid(pGrp);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKCONTROLS:
	{
		WoormTable*	pSymTable = GetDocument()->m_pEditorManager->GetPrgData()->GetSymTable();

		AskDialogData* pAskDlg = dynamic_cast<AskDialogData*>(pNode->m_pParentItemData);
		ASSERT_VALID(pAskDlg);

		Array* pArGroups = dynamic_cast<Array*>(pNode->m_pItemData);
		ASSERT_VALID(pArGroups);

		AskGroupData* pNewAskGroup = new AskGroupData(*pSymTable, GetDocument());
		pNewAskGroup->SetTitle(AskGroupData::GetEmptyTitle());
		pAskDlg->AddAskGroup(pNewAskGroup);

		HTREEITEM htControls = this->m_TreeCtrl.FindItemData((DWORD)pArGroups, m_TreeCtrl.m_htAskDialogs);
		ASSERT(htControls);

		CNodeTree& pGrp = this->m_TreeCtrl.AddNode(AskGroupData::GetEmptyTitle(), CNodeTree::ENodeType::NT_ASKGROUP, htControls, pNewAskGroup, pAskDlg);
		this->m_TreeCtrl.SelectItem(pGrp.m_ht);

		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->NewObjectPropertyGrid(&pGrp);
		break;
	}

	case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
	{
		CNodeTree* pParentNode = m_TreeCtrl.GetNode(m_TreeCtrl.GetParentItem(pNode->m_ht));
		ASSERT_VALID(pParentNode);
		ASSERT(pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN);

		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pParentNode->m_pParentItemData);
		ASSERT_VALID(pTblRule);
		SqlTableInfo* pTableInfo = dynamic_cast<SqlTableInfo*>(pParentNode->m_pItemData);
		ASSERT_VALID(pTableInfo);

		int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pTableInfo->GetTableName());
		ASSERT(idx >= 0);
		//if (idx < 0) return FALSE;

		WoormField* pNewHiddenField = NULL;
		if (GetDocument()->m_pNodesSelection && GetDocument()->m_pNodesSelection->GetCount() > 0)
		{
			pNewHiddenField = pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(GetDocument()->m_pNodesSelection, TRUE, pTblRule, idx);
		}
		else
			pNewHiddenField = pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(pNode, TRUE, pTblRule, idx, TRUE);

		break;
	}

	case CNodeTree::ENodeType::NT_LIST_DB_FOREIGN_KEY:
	{
		CNodeTree* pAncientNode = m_TreeCtrl.GetAncientNode(pNode->m_ht, CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY);
		ASSERT_VALID(pAncientNode);

		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pAncientNode->m_pParentItemData);
		ASSERT_VALID(pTblRule);
		if (!pTblRule)
			return;

		CHelperSqlCatalog::CTableColumns* pTargetTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pAncientNode->m_pItemData);
		ASSERT_VALID(pTargetTC);
		if (!pTargetTC)
			return;

		CHelperSqlCatalog::CTableForeignTablesKeys* pFTK = dynamic_cast<CHelperSqlCatalog::CTableForeignTablesKeys*>(pNode->m_pItemData);
		ASSERT_VALID(pFTK);
		if (!pFTK)
			return;

		CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
		ASSERT_VALID(pHelperSqlCatalog);
		CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pFTK->m_pParent->m_sForeignTableName);
		ASSERT_VALID(pTC);
		if (!pTC)
			return;

		AddJoin(pTblRule, pTC->m_pCatalogEntry->m_pTableInfo, pTC->m_pCatalogEntry->m_strTableName, pTargetTC->m_pCatalogEntry->m_strTableName, pFTK);
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES:
	{
		CNodeTree* pAncientNode = m_TreeCtrl.GetAncientNode(pNode->m_ht, CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES);
		ASSERT_VALID(pAncientNode);

		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pAncientNode->m_pParentItemData);
		ASSERT_VALID(pTblRule);
		if (!pTblRule)
			return;

		CHelperSqlCatalog::CTableColumns* pTargetTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pAncientNode->m_pItemData);
		ASSERT_VALID(pTargetTC);
		if (!pTargetTC)
			return;

		CHelperExternalReferences::CTableSingleExtRef* pSER = dynamic_cast<CHelperExternalReferences::CTableSingleExtRef*>(pNode->m_pItemData);
		ASSERT_VALID(pSER);
		if (!pSER)
			return;

		CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
		ASSERT_VALID(pHelperSqlCatalog);
		CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pSER->m_sExtTableName);
		ASSERT_VALID(pTC);
		if (!pTC)
			return;


		AddJoin(pTblRule, pTC->m_pCatalogEntry->m_pTableInfo, pTC->m_pCatalogEntry->m_strTableName, pTargetTC->m_pCatalogEntry->m_strTableName, NULL, pSER);
		break;
	}

	case CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE:
	{
		CNodeTree* pAncientNode = m_TreeCtrl.GetAncientNode(pNode->m_ht, CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES);
		ASSERT_VALID(pAncientNode);

		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pAncientNode->m_pParentItemData);
		ASSERT_VALID(pTblRule);
		if (!pTblRule)
			return;

		/*CHelperSqlCatalog::CTableColumns* pTargetTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pAncientNode->m_pItemData);
		ASSERT_VALID(pTargetTC);
		if (!pTargetTC)
			return;*/

		CHelperExternalReferences::CTableExtRefs* pER = dynamic_cast<CHelperExternalReferences::CTableExtRefs*>(pNode->m_pItemData);
		ASSERT_VALID(pER);
		if (!pER)
			return;

		CHelperExternalReferences::CTableSingleExtRef* pSER = dynamic_cast<CHelperExternalReferences::CTableSingleExtRef*>(pER->m_arExtRefs.GetAt(0));

		CHelperSqlCatalog*	pHelperSqlCatalog = GetDocument()->m_pEditorManager->GetHelperSqlCatalog();
		ASSERT_VALID(pHelperSqlCatalog);
		CHelperSqlCatalog::CTableColumns* pTC = pHelperSqlCatalog->FindEntryByName(pER->m_sTableName);

		ASSERT_VALID(pTC);
		if (!pTC)
			return;

		CHelperExternalReferences::CTableSingleExtRef* newRef = new CHelperExternalReferences::CTableSingleExtRef(pSER->m_sExtPrimaryKey, pER->m_sTableName, pSER->m_sForeignKey);

		AddJoin(pTblRule, pTC->m_pCatalogEntry->m_pTableInfo, pER->m_sTableName, pSER->m_sExtTableName, NULL, newRef);
		break;
	}

	case  CNodeTree::ENodeType::NT_OBJ_CHART:
	{
		Chart* pChart = dynamic_cast<Chart*>(pNode->m_pItemData);
		ASSERT_VALID(pChart);
		if (!pChart)
			return;
		Chart::CSeries* newSeries = new Chart::CSeries(pChart);

		int index = 0;
		if (pChart->GetSeries()->GetSize() > 0)
			newSeries->SetIndex(pChart->GetSeries()->GetSize());
		pChart->GetSeries()->Add(newSeries);

		m_TreeCtrl.FillLayouts();

		HTREEITEM htNewSeries = this->m_TreeCtrl.FindItemData((DWORD)newSeries, m_TreeCtrl.m_htLayouts);
		ASSERT(htNewSeries);
		this->m_TreeCtrl.SelectItem(htNewSeries);

		break;
	}

	default:;
	}

	return;
}

void CRSReportTreeView::OnUpdateNew(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
	{
		//HTREEITEM hParentItem = m_TreeCtrl.GetParentItem(hCurrentItem);
		bEnable = hCurrentItem == m_TreeCtrl.m_htHiddenGroupVariables;
		break;
	}

	case CNodeTree::ENodeType::NT_ROOT_LAYOUTS:

	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
	case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:

	case CNodeTree::ENodeType::NT_ROOT_RULES:

	case CNodeTree::ENodeType::NT_ROOT_PROCEDURES:
	case CNodeTree::ENodeType::NT_ROOT_QUERIES:

	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
	case CNodeTree::ENodeType::NT_ASKCONTROLS:
	case CNodeTree::ENodeType::NT_ASKGROUP:

	case CNodeTree::ENodeType::NT_SUBROOT_TRIGGER_EVENTS:
	case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
	case CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST:

	case CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
	{
		bEnable = TRUE;
		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_CHART:
	{
		Chart* pChart = dynamic_cast<Chart*>(pNode->m_pItemData);
		ASSERT_VALID(pChart);
		bEnable = pChart &&
			pChart->m_eChartType != EnumChartType::Chart_None &&
			(pChart->AllowMultipleSeries() || pChart->GetSeries()->GetSize() == 0);
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DB_FOREIGN_KEY:
	{
		CNodeTree* pAncientNode = m_TreeCtrl.GetAncientNode(pNode->m_ht, CNodeTree::ENodeType::NT_GROUP_DB_FOREIGN_KEY);
		ASSERT_VALID(pAncientNode);
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pAncientNode->m_pParentItemData);
		//ASSERT_VALID(pTblRule);
		if (pTblRule)
			bEnable = TRUE;
		break;
	}
	case CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES:
	case CNodeTree::ENodeType::NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE:
	{
		CNodeTree* pAncientNode = m_TreeCtrl.GetAncientNode(pNode->m_ht, CNodeTree::ENodeType::NT_GROUP_DB_EXTERNAL_REFERENCES);
		ASSERT_VALID(pAncientNode);
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pAncientNode->m_pParentItemData);
		//ASSERT_VALID(pTblRule);
		if (pTblRule)
			bEnable = TRUE;
		break;
	}

	case CNodeTree::ENodeType::NT_LINK_PARAMETERS:
	{
		WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pItemData);
		//ASSERT_VALID(pLink);
		if (pLink)
			bEnable = pLink->m_LinkType == WoormLink::ConnectionReport
			|| (pLink->m_LinkType == WoormLink::ConnectionURL && pLink->m_SubType == WoormLink::WoormLinkSubType::Url);
		break;
	}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnMore()
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
	CWaitCursor wc;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:
		m_TreeCtrl.m_bShowMoreVariable = !m_TreeCtrl.m_bShowMoreVariable;
		m_TreeCtrl.FillVariables(TRUE, FALSE, TRUE, FALSE, FALSE);
		m_TreeCtrl.Expand(hCurrentItem, TVE_EXPAND);
		break;
	}
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateMore(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_ROOT_VARIABLES:

		bEnable = TRUE;
		break;
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUp()
{
	//all'interno del metodo, vengono chiamate le MoveDown, per rispecchiare il movimento sul tree percepito dall'utente
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
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	{
		AskDialogData* pAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskDialog);
		AskRuleData*	m_pAskRule = GetDocument()->m_pEditorManager->GetPrgData()->GetAskRuleData();
		ASSERT_VALID(m_pAskRule);

		if (!m_pAskRule->m_AskDialogs.MoveDown(pAskDialog))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskDialog);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKGROUP:
	{
		AskGroupData* pAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskGroup);
		AskDialogData* m_pCurAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pParentItemData);
		ASSERT_VALID(m_pCurAskDialog);

		if (!m_pCurAskDialog->m_AskGroups.MoveDown(pAskGroup))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskGroup);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKFIELD:
	{
		AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskField);
		AskGroupData* m_pCurAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pParentItemData);
		ASSERT_VALID(m_pCurAskGroup);

		if (!m_pCurAskGroup->m_AskFields.MoveDown(pAskField))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskField);
		break;
	}
	case CNodeTree::ENodeType::NT_PROCEDURE:
	{
		ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
		if (!pProc)
			break;

		ProcedureData*	pProcedures = dynamic_cast<ProcedureData*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pProcedures);
		if (!pProcedures)
			break;

		if (!pProcedures->GetArray().MoveDown(pProc))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Procedures, pProc);
		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_COLUMN:
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
		ASSERT_VALID(pCol);
		if (!pCol)
			break;

		Table*	pTable = dynamic_cast<Table*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pTable);
		if (!pTable)
			break;

		pTable->MoveColumn(TRUE);

		//CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Layouts);
		//pTree->Move(pNode->m_ht, FALSE);
		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pCol);
		break;
	}

	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pF);
		if (!pF)
			break;
		if (!pF->IsExprRuleField())
			break;

		CNodeTree* pParentNode = m_TreeCtrl.GetNode(m_TreeCtrl.GetParentItem(pNode->m_ht));
		if (pParentNode->m_NodeType != CNodeTree::ENodeType::NT_ROOT_RULES)
			break;

		RuleDataObj* pRule = dynamic_cast<RuleDataObj*>(pNode->m_pAncestorItemData);
		ASSERT_VALID(pRule);
		if (!pRule)
			break;

		RuleDataArray* pRules = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
		ASSERT_VALID(pRules);

		pRules->MoveDown(pRule);

		GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pF);
		break;
	}

	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
	case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_LOOP:
	{
		RuleDataObj* pRule = dynamic_cast<RuleDataObj*>(pNode->m_pItemData);
		ASSERT_VALID(pRule);
		if (!pRule)
			break;

		RuleDataArray*	pRules = dynamic_cast<RuleDataArray*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pRules);
		if (!pRules)
			break;

		pRules->MoveDown(pRule);

		GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pRule);
		break;
	}

	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
	{
		TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
		ASSERT_VALID(pTriggerEvent);
		if (!pTriggerEvent)
			break;

		EventsData* pEventsData = dynamic_cast<EventsData*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pEventsData);
		if (!pEventsData)
			break;

		pEventsData->m_TriggEvents.MoveDown(pTriggerEvent);

		GetDocument()->RefreshRSTree(ERefreshEditor::Events, pTriggerEvent);
		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		Chart::CSeries* pSeries = dynamic_cast<Chart::CSeries*>(pNode->m_pItemData);
		ASSERT_VALID(pSeries);
		if (!pSeries)
			break;

		if (!pSeries->GetParent() || !pSeries->GetParent()->AllowMultipleSeries())
			break;

		pSeries->GetParent()->GetSeries()->MoveDown(pSeries);

		//allineo posizione e index
		int newIndex = pSeries->GetParent()->GetSeries()->Find(pSeries);
		int currIndex = pSeries->GetIndex();

		if (currIndex != newIndex)
		{
			pSeries->GetParent()->GetSeries()->GetSeriesAt(currIndex)->SetIndex(currIndex);
			pSeries->SetIndex(newIndex);
		}

		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pSeries->GetParent());
		break;
	}
	}

	GetDocument()->SetModifiedFlag();
}

void CRSReportTreeView::OnDown()
{
	//all'interno del metodo, vengono chiamate le MoveUp, per rispecchiare il movimento sul tree percepito dall'utente
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
		return;

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	{
		AskDialogData* pAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskDialog);
		AskRuleData*	m_pAskRule = GetDocument()->m_pEditorManager->GetPrgData()->GetAskRuleData();
		ASSERT_VALID(m_pAskRule);

		if (!m_pAskRule->m_AskDialogs.MoveUp(pAskDialog))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskDialog);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKGROUP:
	{
		AskGroupData* pAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskGroup);
		AskDialogData* m_pCurAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pParentItemData);
		ASSERT_VALID(m_pCurAskDialog);

		if (!m_pCurAskDialog->m_AskGroups.MoveUp(pAskGroup))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskGroup);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKFIELD:
	{
		AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskField);
		AskGroupData* m_pCurAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pParentItemData);
		ASSERT_VALID(m_pCurAskGroup);

		if (!m_pCurAskGroup->m_AskFields.MoveUp(pAskField))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, pAskField);
		break;
	}
	case CNodeTree::ENodeType::NT_PROCEDURE:
	{
		ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
		if (!pProc)
			break;

		ProcedureData*	pProcedures = dynamic_cast<ProcedureData*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pProcedures);
		if (!pProcedures)
			break;

		if (!pProcedures->GetArray().MoveUp(pProc))
			break;

		GetDocument()->RefreshRSTree(ERefreshEditor::Procedures, pProc);
		break;
	}
	case CNodeTree::ENodeType::NT_OBJ_COLUMN:
	{
		TableColumn* pCol = dynamic_cast<TableColumn*>(pNode->m_pItemData);
		ASSERT_VALID(pCol);
		if (!pCol)
			break;

		Table*	pTable = dynamic_cast<Table*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pTable);
		if (!pTable)
			break;

		pTable->MoveColumn(FALSE);

		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pCol);
		//CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Layouts);
		//pTree->Move(pNode->m_ht, TRUE);
		break;
	}

	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pF);
		if (!pF)
			break;
		if (!pF->IsExprRuleField())
			break;

		CNodeTree* pParentNode = m_TreeCtrl.GetNode(m_TreeCtrl.GetParentItem(pNode->m_ht));
		if (pParentNode->m_NodeType != CNodeTree::ENodeType::NT_ROOT_RULES)
			break;

		RuleDataObj* pRule = dynamic_cast<RuleDataObj*>(pNode->m_pAncestorItemData);
		ASSERT_VALID(pRule);
		if (!pRule)
			break;

		RuleDataArray* pRules = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
		ASSERT_VALID(pRules);

		pRules->MoveUp(pRule);

		GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pF);
		break;
	}

	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
	case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_LOOP:
	{
		RuleDataObj* pRule = dynamic_cast<RuleDataObj*>(pNode->m_pItemData);
		ASSERT_VALID(pRule);
		if (!pRule)
			break;

		RuleDataArray*	pRules = dynamic_cast<RuleDataArray*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pRules);
		if (!pRules)
			break;

		pRules->MoveUp(pRule);

		GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pRule);
		break;
	}
	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
	{
		TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
		ASSERT_VALID(pTriggerEvent);
		if (!pTriggerEvent)
			break;

		EventsData* pEventsData = dynamic_cast<EventsData*>(pNode->m_pParentItemData);;
		ASSERT_VALID(pEventsData);
		if (!pEventsData)
			break;

		pEventsData->m_TriggEvents.MoveUp(pTriggerEvent);

		GetDocument()->RefreshRSTree(ERefreshEditor::Events, pTriggerEvent);
		break;
	}

	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		Chart::CSeries* pSeries = dynamic_cast<Chart::CSeries*>(pNode->m_pItemData);
		ASSERT_VALID(pSeries);
		if (!pSeries)
			break;

		if (!pSeries->GetParent() || !pSeries->GetParent()->AllowMultipleSeries())
			break;

		pSeries->GetParent()->GetSeries()->MoveUp(pSeries);

		//allineo posizione e index
		int newIndex = pSeries->GetParent()->GetSeries()->Find(pSeries);
		int currIndex = pSeries->GetIndex();

		if (currIndex != newIndex)
		{
			pSeries->GetParent()->GetSeries()->GetSeriesAt(currIndex)->SetIndex(currIndex);
			pSeries->SetIndex(newIndex);
		}

		GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pSeries->GetParent());
		break;
	}
	}

	GetDocument()->SetModifiedFlag();
}

void CRSReportTreeView::OnUpdateUpDown(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_PROCEDURE:
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	case CNodeTree::ENodeType::NT_ASKGROUP:
	case CNodeTree::ENodeType::NT_ASKFIELD:
	case CNodeTree::ENodeType::NT_OBJ_COLUMN:

	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
	case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_LOOP:

	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		bEnable = TRUE;
		break;
	}

	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
		ASSERT_VALID(pF);
		if (!pF)
			break;
		if (!pF->IsExprRuleField())
			break;

		CNodeTree* pParentNode = m_TreeCtrl.GetNode(m_TreeCtrl.GetParentItem(pNode->m_ht));
		if (pParentNode->m_NodeType != CNodeTree::ENodeType::NT_ROOT_RULES)
			break;

		RuleDataObj* pRule = dynamic_cast<RuleDataObj*>(pNode->m_pAncestorItemData);
		ASSERT_VALID(pRule);
		if (!pRule)
			break;

		bEnable = TRUE;
		break;
	}
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnDialogPreview()
{
	//all'interno del metodo, vengono chiamate le MoveUp, per rispecchiare il movimento sul tree percepito dall'utente
	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
		return;

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
		return;

	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	AskRuleData* pAskRule = GetDocument()->m_pEditorManager->GetPrgData()->GetAskRuleData();
	ASSERT_VALID(pAskRule);
	if (pAskRule->GetCount() == 0)
		return;

	BOOL bSingleAsk = TRUE;
	AskDialogData*	pAskDialog = NULL;

	switch (pNode->m_NodeType)
	{
	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
	{
		bSingleAsk = FALSE;
		break;
	}
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	{
		pAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskDialog);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKGROUP:
	{
		pAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pParentItemData);
		ASSERT_VALID(pAskDialog);
		break;
	}
	case CNodeTree::ENodeType::NT_ASKFIELD:
	{
		AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
		ASSERT_VALID(pAskField);

		pAskDialog = pAskRule->GetAskDialog(pAskField);
		break;
	}
	}

	//-------------------------codice da cambiare con le nuove dialogs
	if (pAskDialog || !bSingleAsk)
	{
		AskDialogInputMng aAskMng
		(
			GetDocument()->m_pEditorManager->GetPrgData()->GetAskRuleData(),
			GetDocument()->GetEditorSymTable(),
			GetDocument()
		);

		if (bSingleAsk)
			aAskMng.ExecAsk(GetDocument()->GetWoormFrame(), pAskDialog, GetDocument()->m_pEngine, TRUE);
		else
			aAskMng.ExecAllAsk(GetDocument()->GetWoormFrame(), GetDocument()->m_pEngine, TRUE);
	}

	return;
}

//----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateDialogPreview(CCmdUI* pCmdUI)
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
	case CNodeTree::ENodeType::NT_ROOT_DIALOGS:
	case CNodeTree::ENodeType::NT_ASKDIALOG:
	case CNodeTree::ENodeType::NT_ASKGROUP:
	case CNodeTree::ENodeType::NT_ASKFIELD:
		bEnable = TRUE;
		break;
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnDelete()
{
	ASSERT_VALID(GetDocument()->GetEditorManager());
	ASSERT_VALID(GetDocument()->GetEditorManager()->GetPrgData());

	HTREEITEM hCurrentItem = m_TreeCtrl.GetSelectedItem();
	if (!hCurrentItem)
	{
		return;
	}
	CNodeTree* pCurrNode = m_TreeCtrl.GetNode(hCurrentItem);
	if (!pCurrNode)
		return;
	ASSERT_VALID(pCurrNode);
	ASSERT_KINDOF(CNodeTree, pCurrNode);

	//la cancellazione degli oggetti grafici è demandata al documento-------------------------
	if (m_TreeCtrl.IsLayoutNodeType(pCurrNode->m_NodeType))
	{
		GetDocument()->OnObjectCut();
		return;
	}
	//----------------------------------------------------------------------------------------

	if (!GetDocument()->m_pNodesSelection || GetDocument()->m_pNodesSelection->GetCount() == 0)
	{
		if (!GetDocument()->m_pNodesSelection)
			GetDocument()->m_pNodesSelection = new  CNodeTreeArray();

		GetDocument()->m_pNodesSelection->AddUnique(pCurrNode);
	}

	ASSERT_VALID(GetDocument()->m_pNodesSelection);
	if (GetDocument()->m_pNodesSelection == NULL || GetDocument()->m_pNodesSelection->GetCount() == 0)
		return;

	if (AfxMessageBox(_TB("Do you confirm the deletion of the selected objects? "), MB_YESNO) != IDYES)
	{
		return;
	}

	CString sDelMsg = _TB("The field {0-%s} cannot be delete because it is still in use.");

	pCurrNode = (CNodeTree*)GetDocument()->m_pNodesSelection->GetAt(0);
	CNodeTree nRefreshNode(pCurrNode->m_ht, pCurrNode->m_eImgIndex, pCurrNode->m_NodeType);
	BOOL bNeedsRefreshRule = FALSE;
	BOOL bNeedsRefreshLayout = FALSE;
	BOOL bNeedsRefresh = FALSE;

	int count = GetDocument()->m_pNodesSelection->GetCount();
	for (int i = 0; i < count; i++)
	{
		ASSERT_VALID(GetDocument()->m_pNodesSelection);
		if (!GetDocument()->m_pNodesSelection) break;

		if (i >= GetDocument()->m_pNodesSelection->GetCount())
		{
			ASSERT(FALSE);
			break;
		}

		CNodeTree* pNode = (CNodeTree*)GetDocument()->m_pNodesSelection->GetAt(i);
		if (!pNode)
			continue;
		ASSERT_VALID(pNode);

		this->m_TreeCtrl.Expand(pNode->m_ht, TVE_COLLAPSE);

		switch (pNode->m_NodeType)
		{
		case CNodeTree::ENodeType::NT_ASKDIALOG:
		{
			AskDialogData* pAskDlg = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
			if (!pAskDlg)
				break;
			AskRuleData* pAskRule = dynamic_cast<AskRuleData*>(pNode->m_pParentItemData);
			if (!pAskRule)
				break;

			if (!pAskDlg->CanDelete())
				continue;

			int nCurPos = pAskRule->m_AskDialogs.FindPtr(pAskDlg);

			pAskRule->DelAskDialog(nCurPos, TRUE); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_ASKGROUP:
		{
			AskGroupData* pAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pItemData);
			if (!pAskGroup)
				break;
			AskDialogData* pCurAskDialog = dynamic_cast<AskDialogData*>(pNode->m_pParentItemData);
			if (!pCurAskDialog)
				break;

			if (!pAskGroup->CanDelete())
				continue;

			int nCurPos = pCurAskDialog->m_AskGroups.FindPtr(pAskGroup);

			pCurAskDialog->DelAskGroup(nCurPos, TRUE); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_ASKFIELD:
		{
			AskFieldData* pAskField = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
			if (!pAskField)
				break;
			AskGroupData* pCurAskGroup = dynamic_cast<AskGroupData*>(pNode->m_pParentItemData);
			if (!pCurAskGroup)
				break;

			if (!pAskField->CanDelete())
				continue;

			int nCurPos = pCurAskGroup->m_AskFields.FindPtr(pAskField);

			pCurAskGroup->DelAskField(nCurPos, TRUE); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_PROCEDURE:
		{
			ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
			if (!pProc)
				break;

			ProcedureData*	pProcedures = dynamic_cast<ProcedureData*>(pNode->m_pParentItemData);;
			ASSERT_VALID(pProcedures);
			if (!pProcedures)
				break;

			//TODO BOOL res = pProcedures->CanDeleteField(pData->GetName());
			pProcedures->Delete(pProc->GetName()); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_NAMED_QUERY:
		{
			QueryObjItem* pQry = dynamic_cast<QueryObjItem*>(pNode->m_pItemData);
			if (!pQry)
				break;

			QueryObjectData* pQueries = dynamic_cast<QueryObjectData*>(pNode->m_pParentItemData);;
			ASSERT_VALID(pQueries);
			if (!pQueries)
				break;

			//TODO BOOL res = pQueries->CanDeleteField(pData->GetName());

			RuleDataArray* pRules = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
			int idx = pRules->GetQueryRuleDataIndex(pQry);
			ASSERT(idx > -1);
			if (idx > -1)
			{
				CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
				HTREEITEM htRule = pTree->RemoveObjectFromItemData(pRules->GetAt(idx));

				pTree->DeleteItem(htRule);
				pRules->RemoveAt(idx);
				bNeedsRefreshRule = TRUE;
			}

			pQueries->Delete(pQry->GetName()); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_LINK:
		{
			WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pItemData);
			if (!pLink)
				break;

			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Links);

			pTree->RemoveObjectFromItemData(pLink);

			HTREEITEM ht = pTree->FindItemData((DWORD)pLink, pTree->GetParentItem(pNode->m_ht) == pTree->m_htLinks ? pTree->m_htLayouts : pTree->m_htLinks);
			if (ht)
				pTree->DeleteItem(ht);
			pTree->DeleteItem(pNode->m_ht);

			int idx = GetDocument()->m_arWoormLinks.FindPtr(pLink);
			GetDocument()->m_arWoormLinks.RemoveAt(idx); bNeedsRefresh = TRUE;
			break;
		}
		case CNodeTree::ENodeType::NT_LINK_PARAM:
		{
			WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pParentItemData);
			if (!pLink)
				break;
			ASSERT_VALID(pLink);

			WoormField* pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
			if (!pF)
				break;
			ASSERT_VALID(pF);

			CRSTreeCtrl* pTree1 = GetDocument()->GetRSTree(ERefreshEditor::Links);
			HTREEITEM ht1 = pTree1->FindItemData((DWORD)pF, pTree1->m_htLinks);
			if (ht1)
				pTree1->DeleteItem(ht1);

			CRSTreeCtrl* pTree2 = GetDocument()->GetRSTree(ERefreshEditor::Layouts);
			HTREEITEM ht2 = pTree2->FindItemData((DWORD)pF, pTree2->m_htLayouts);
			if (ht2)
				pTree2->DeleteItem(ht2);

			if (pTree1 != pTree2)
			{
				pTree2->RemoveObjectFromItemData(pF);
			}
			pTree1->RemoveObjectFromItemData(pF);

			VERIFY(pLink->m_pLocalSymbolTable->DelField(pF->GetName()));

			bNeedsRefresh = TRUE;
			break;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
			ASSERT_VALID(pField);
			if (!pField)
				break;

			bNeedsRefreshRule = pField->IsRuleField();
			bNeedsRefreshLayout = !pField->IsHidden();

			HTREEITEM htParent = this->m_TreeCtrl.GetParentItem(pNode->m_ht);
			CNodeTree* pParentNode = dynamic_cast<CNodeTree*>((CObject*)m_TreeCtrl.GetItemData(htParent));
			ASSERT_VALID(pParentNode);

			if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST)
			{
				TriggEventData* pEvent = dynamic_cast<TriggEventData*>(pParentNode->m_pItemData);
				ASSERT_VALID(pEvent);
				if (!pEvent)
					break;
				if (pEvent->m_BreakList.GetCount() == 1 && (!pEvent->m_pWhenExpr || pEvent->m_pWhenExpr->IsEmpty()))
					break;

				::CStringArray_Remove(pEvent->m_BreakList, pField->GetName()); bNeedsRefresh = TRUE;
				break;
			}
			else if (
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST ||
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_SELECT ||
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_PARAMETERS
				)
			{
				//TODO
				ASSERT(FALSE);
				break;
			}
			else if (
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS ||
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS
				)
			{
				TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pParentNode->m_pParentItemData);
				ASSERT_VALID(pTblRule);
				if (!pTblRule)
					break;

				if (pTblRule->GetTotFieldLinks() < 2)
					break;

				CString sLog;
				if (!GetDocument()->GetEditorManager()->GetPrgData()->CanDeleteField(pField->GetName(), sLog))
				{
					CString msg = cwsprintf(sDelMsg, pField->GetName());
					if (!sLog.IsEmpty())
						msg += '\n' + sLog;
					AfxTBMessageBox(msg, MB_OK | MB_ICONEXCLAMATION);
					continue;
				}

				nRefreshNode.m_NodeType = pParentNode->m_NodeType;

				pTblRule->RemoveLink(pField->GetName(), TRUE); bNeedsRefresh = TRUE;
				break;
			}

			//---------------
			BOOL bIsHidden = pField->IsHidden();
			if (!bIsHidden)
				break;
			CString sName = pField->GetName();

			if (pField->IsTableRuleField())
			{
				TblRuleData* pTblRule = GetDocument()->GetEditorManager()->GetPrgData()->GetRuleData()->GetTblRuleDataFromField(sName);
				if (pTblRule && pTblRule->GetTotFieldLinks() < 2)
					break;
			}

			CString sLog;
			if (!GetDocument()->GetEditorManager()->GetPrgData()->CanDeleteField(sName, sLog))
			{
				CString msg = cwsprintf(sDelMsg, sName);
				if (!sLog.IsEmpty())
					msg += '\n' + sLog;
				AfxTBMessageBox(msg, MB_OK | MB_ICONEXCLAMATION);
				continue;
			}

			GetDocument()->m_pEditorManager->GetPrgData()->DeleteField(sName); bNeedsRefresh = TRUE;
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
		{
			QueryRuleData* pQueryRule = dynamic_cast<QueryRuleData*>(pNode->m_pItemData);
			ASSERT_VALID(pQueryRule);
			if (!pQueryRule)
				break;

			RuleDataArray* pRules = dynamic_cast<RuleDataArray*>(pNode->m_pParentItemData);
			ASSERT_VALID(pRules);
			if (!pRules)
				break;

			int idx = pRules->FindPtr(pQueryRule);
			ASSERT(idx > -1);
			if (idx > -1)
			{
				pQueryRule->SetRuleFields(FALSE);

				CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
				pTree->RemoveObjectFromItemData(pQueryRule);

				pTree->DeleteItem(pNode->m_ht);

				pRules->RemoveAt(idx); bNeedsRefresh = TRUE;
			}
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_LOOP:
		{
			WhileRuleData* pRule = dynamic_cast<WhileRuleData*>(pNode->m_pItemData);
			ASSERT_VALID(pRule);
			if (!pRule)
				break;

			RuleDataArray* pRules = dynamic_cast<RuleDataArray*>(pNode->m_pParentItemData);
			ASSERT_VALID(pRules);
			if (!pRules)
				break;

			int idx = pRules->FindPtr(pRule);
			ASSERT(idx > -1);
			if (idx > -1)
			{
				pRule->SetRuleFields(FALSE);

				CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
				pTree->RemoveObjectFromItemData(pRule);

				pTree->DeleteItem(pNode->m_ht);

				pRules->RemoveAt(idx); bNeedsRefresh = TRUE;
			}
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
		{
			TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pItemData);
			ASSERT_VALID(pTblRule);
			if (!pTblRule)
				break;

			RuleDataArray* pRules = dynamic_cast<RuleDataArray*>(pNode->m_pParentItemData);
			ASSERT_VALID(pRules);
			if (!pRules)
				break;

			CString sUsedFieldName, sLog;
			BOOL bDel = pTblRule->CanDeleteRule(sUsedFieldName, sLog);
			if (!bDel)
			{
				CString msg = cwsprintf(_TB("The rule cannot be delete because at least the field {0-%s} is still in use."), sUsedFieldName);
				if (!sLog.IsEmpty())
					msg += '\n' + sLog;

				msg += '\n' + _TB("Do you want delete the rule and mantains fields ?");

				if (AfxTBMessageBox(msg, MB_OKCANCEL | MB_ICONWARNING) == IDCANCEL)
					continue;
			}

			int idx = pRules->FindPtr(pTblRule);
			ASSERT(idx > -1);
			if (idx > -1)
			{
				CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
				pTree->RemoveObjectFromItemData(pTblRule);

				pTree->DeleteItem(pNode->m_ht);

				pTblRule->RemoveAllLinks(bDel);

				pRules->RemoveAt(idx); bNeedsRefresh = TRUE;
			}
			break;
		}

		case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
		{
			TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
			ASSERT_VALID(pTriggerEvent);
			if (!pTriggerEvent)
				break;

			CStringArray strArr;
			pTriggerEvent->GetSubtotalFields(strArr);
			for (int k = 0; k < strArr.GetCount(); k++)
			{
				CString strNewName = strArr.GetAt(k);

				if (strNewName.Left(2).CompareNoCase(L"w_") == 0)
					strNewName = strNewName.Mid(2);

				strNewName = L"SubTotal_" + strNewName;

				CString sSubTotalName = strNewName;
				int		index = 0;
				while (pTriggerEvent->m_SymTable.ExistField(sSubTotalName))
				{
					pTriggerEvent->DeleteField(sSubTotalName);

					WoormField* field = GetDocument()->GetEditorSymTable()->GetField(sSubTotalName);
					if (field)
					{
						field->SetHidden(TRUE);
						field->SetFieldType(WoormField::FIELD_NORMAL);

					}

					GetDocument()->SyncronizeViewSymbolTable(field);

					sSubTotalName = strNewName + cwsprintf(_T("_%d"), index);
				}
			}

			EventsData* pEventsData = dynamic_cast<EventsData*>(pNode->m_pParentItemData);
			ASSERT_VALID(pEventsData);
			if (!pEventsData)
				break;

			int nEventIdx = pEventsData->GetEventIdx(pTriggerEvent->GetEventName());
			ASSERT(nEventIdx >= 0);

			if (nEventIdx >= 0)
			{
				CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Events);
				//pTree->RemoveTreeChilds(pNode->m_ht); nRefreshNode.m_ht = pNode->m_ht;
				//pNode->Empty();
				pTree->DeleteItem(pNode->m_ht);

				pEventsData->m_TriggEvents.RemoveAt(nEventIdx);
				bNeedsRefresh = TRUE;
			}
			break;
		}

		/*case CNodeTree::ENodeType::NT_OBJ_CHART:
		{
			Chart* pChart = dynamic_cast<Chart*>(pNode->m_pItemData);
			ASSERT_VALID(pChart);
			if (!pChart)
				break;

			Chart* pChart = GetDocument()->GetRSTree(ERefreshEditor::Layout);
			pChart->RemoveObjectFromItemData(pChart);

			pTree->DeleteItem(pNode->m_ht);

			pRules->RemoveAt(idx); bNeedsRefresh = TRUE;

			SAFE_DELETE(pChart);
			bNeedsRefreshLayout = TRUE;
			break;
		}*/
		case CNodeTree::ENodeType::NT_OBJ_SERIES:
		{
			Chart::CSeries* pSeries = dynamic_cast<Chart::CSeries*>(pNode->m_pItemData);
			Chart* pChart = pSeries->GetParent();
			ASSERT_VALID(pSeries);
			if (!pSeries)
				break;

			int index = 0;
			for (; index < pChart->GetSeries()->GetSize(); index++)
			{
				if (pSeries == pChart->GetSeries()->GetSeriesAt(index))
					break;
			}

			int indexDel = index + 1;
			for (; indexDel < pChart->GetSeries()->GetSize(); indexDel++)
			{
				Chart::CSeries* currSeries = pChart->GetSeries()->GetSeriesAt(indexDel);
				currSeries->SetIndex(currSeries->GetIndex() - 1);
			}

			if (index < pChart->GetSeries()->GetSize())
				pChart->GetSeries()->RemoveAt(index);

			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Layouts);
			pTree->RemoveObjectFromItemData(pSeries);
			pTree->DeleteItem(pNode->m_ht);

			GetDocument()->RefreshRSTree(ERefreshEditor::Layouts, pChart);

			break;
		}
		}
	}

	if (bNeedsRefresh)
	{
		switch (nRefreshNode.m_NodeType)
		{
		case CNodeTree::ENodeType::NT_ASKDIALOG:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
			GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, NULL, TRUE);
			break;
		}
		case CNodeTree::ENodeType::NT_ASKGROUP:
		case CNodeTree::ENodeType::NT_ASKFIELD:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
			GetDocument()->RefreshRSTree(ERefreshEditor::Dialogs, NULL, TRUE);
			break;
		}

		case CNodeTree::ENodeType::NT_PROCEDURE:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Procedures, NULL, TRUE);
			break;
		}

		case CNodeTree::ENodeType::NT_NAMED_QUERY:
		{
			if (bNeedsRefreshRule)
				GetDocument()->RefreshRSTree(ERefreshEditor::Rules);
			GetDocument()->RefreshRSTree(ERefreshEditor::Queries, NULL, TRUE);
			break;
		}

		case CNodeTree::ENodeType::NT_LINK:
		case CNodeTree::ENodeType::NT_LINK_PARAM:
		{
			GetDocument()->UpdateWindow();
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS:
		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);
			GetDocument()->RefreshRSTree(ERefreshEditor::Variables);

			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
			HTREEITEM htParent = pTree->GetParentItem(nRefreshNode.m_ht);
			pTree->DeleteItem(nRefreshNode.m_ht);
			if (htParent) pTree->SelectItem(htParent);
			break;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			if (bNeedsRefreshLayout)
				GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);
			if (bNeedsRefreshRule)
				GetDocument()->RefreshRSTree(ERefreshEditor::Rules);
			GetDocument()->RefreshRSTree(ERefreshEditor::Variables, NULL, TRUE);
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_LOOP:
		case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Rules, NULL, TRUE);
			break;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
		{
			GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);
			GetDocument()->RefreshRSTree(ERefreshEditor::Variables);

			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
			pTree->SelectItem(pTree->m_htRules);
			//pTree->DeleteItem(nRefreshNode.m_ht);
			break;
		}

		case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
		{
			//GetDocument()->RefreshRSTree(ERefreshEditor::Events, NULL, TRUE);
			CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Events);
			pTree->SelectItem(pTree->m_htTriggerEvents);
			//pTree->DeleteItem(nRefreshNode.m_ht);
			break;
		}
		}
	}

	GetDocument()->m_pNodesSelection = NULL;
	GetDocument()->SetModifiedFlag();
}

void CRSReportTreeView::OnUpdateDelete(CCmdUI* pCmdUI)
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

	case CNodeTree::ENodeType::NT_ASKDIALOG:
	{
		AskDialogData* pData = dynamic_cast<AskDialogData*>(pNode->m_pItemData);
		if (!pData)
			break;
		bEnable = TRUE;
		break;
	}
	case CNodeTree::ENodeType::NT_ASKGROUP:
	{
		AskGroupData* pData = dynamic_cast<AskGroupData*>(pNode->m_pItemData);
		if (!pData)
			break;
		bEnable = TRUE;
		break;
	}
	case CNodeTree::ENodeType::NT_ASKFIELD:
	{
		AskFieldData* pData = dynamic_cast<AskFieldData*>(pNode->m_pItemData);
		if (!pData)
			break;
		bEnable = TRUE;
		break;
	}
	case CNodeTree::ENodeType::NT_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_PROCEDURE:
	{
		bEnable = TRUE;
		break;
	}
	case CNodeTree::ENodeType::NT_VARIABLE:
	{
		WoormField* pField = dynamic_cast<WoormField*>(pNode->m_pItemData);
		if (!pField)
			break;

		HTREEITEM htParent = this->m_TreeCtrl.GetParentItem(pNode->m_ht);
		CNodeTree* pParentNode = dynamic_cast<CNodeTree*>((CObject*)m_TreeCtrl.GetItemData(htParent));
		ASSERT_VALID(pParentNode);
		if (pParentNode)
		{
			if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST)
				//TODO		/*	 pParentNode->m_NodeType == CNodeTree::ENodeType::NT_EVENT_SUBTOTAL_LIST*/
			{
				TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pParentNode->m_pItemData);
				ASSERT_VALID(pTriggerEvent);
				if (!pTriggerEvent)
					break;
				bEnable = (pTriggerEvent->m_pWhenExpr && !pTriggerEvent->m_pWhenExpr->IsEmpty())
					||
					pTriggerEvent->m_BreakList.GetCount() > 1
					;
				break;
			}
			else if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS ||
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_CALC_COLUMNS)
			{
				ASSERT(pField->IsTableRuleField());

				TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pParentNode->m_pParentItemData);
				ASSERT_VALID(pTblRule);
				if (!pTblRule)
					break;

				bEnable = (pTblRule->GetTotFieldLinks() > 1);
				break;
			}
			else if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_SELECT ||
				pParentNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_PARAMETERS)
			{
				bEnable = FALSE;
				break;
			}

		}

		BOOL bIsAsk = pField->IsAsk();
		if (bIsAsk)
			break;

		bEnable = GetDocument()->GetEditorSymTable()->CanDeleteField(pField->GetName());

		BOOL bIsHidden = pField->IsHidden();
		if (!bIsHidden)
			break;
		else if (pParentNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_VARIABLES)
		{
			RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
			for (int r = 0; r < pRuleData->GetSize(); r++)
			{
				TblRuleData* pR = dynamic_cast<TblRuleData*>(pRuleData->GetAt(r));
				if (!pR)
					continue;

				CStringArray names, tables;
				pR->GetColumns(names);
				pR->GetTableNames(tables);
				if (pR->ExistPublicName(pField->GetName()))
				{
					if (names.GetCount() == 1 && tables.GetCount() == 1)
					{
						bEnable = FALSE;
						break;
					}
				}
			}
		}
		break;
	}

	case CNodeTree::ENodeType::NT_RULE_LOOP:
	case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
	case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
	case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
	case CNodeTree::ENodeType::NT_OBJ_CHART:
	case CNodeTree::ENodeType::NT_OBJ_SERIES:
	{
		bEnable = TRUE;
		break;
	}

	case CNodeTree::ENodeType::NT_LINK:
	{
		WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pItemData);
		ASSERT_VALID(pLink);
		if (!pLink)
			break;

		bEnable = pLink->m_LinkType != WoormLink::WoormLinkType::ConnectionRadar;
		break;
	}
	case CNodeTree::ENodeType::NT_LINK_PARAM:
	{
		WoormLink* pLink = dynamic_cast<WoormLink*>(pNode->m_pParentItemData);
		ASSERT_VALID(pLink);
		if (!pLink)
			break;

		bEnable = pLink->m_LinkType != WoormLink::WoormLinkType::ConnectionForm &&
			pLink->m_LinkType != WoormLink::WoormLinkType::ConnectionFunction  &&
			pLink->m_LinkType != WoormLink::WoormLinkType::ConnectionRadar;
		break;
	}
	}

	if (m_TreeCtrl.IsLayoutNodeType(pNode->m_NodeType))
		bEnable = TRUE;

	pCmdUI->Enable(bEnable);
}

//----------------------------------------------------------------------------
BOOL CRSReportTreeView::CanDeleteField(AskFieldData* pAskField)
{
	CString sLog;
	if (!GetDocument()->m_pEditorManager->GetPrgData()->CanDeleteField(pAskField->GetPublicName(), sLog))
	{
		/*CString msg = cwsprintf(_TB("The field {0-%s} cannot be deleted!"), pAskField->GetPublicName());
		if (!sLog.IsEmpty())
			msg += '\n' + _TB("It is used by an other object:") + ' ' + sLog;

		AfxMessageBox(msg, MB_OK | MB_ICONEXCLAMATION);*/
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateEdit(CCmdUI* pCmdUI)
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

	BOOL bEnable = CanOpenEditor(pNode);

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnVKUp()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM prevItem = m_TreeCtrl.GetPrevVisibleItem(currItem);
		if (prevItem)
			m_TreeCtrl.SelectItem(prevItem);
	}
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnVKDown()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM nextItem = m_TreeCtrl.GetNextVisibleItem(currItem);
		if (nextItem)
			m_TreeCtrl.SelectItem(nextItem);
	}
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnVKLeft()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM parentItem = m_TreeCtrl.GetParentItem(currItem);
		if (m_TreeCtrl.GetItemState(currItem, TVIS_EXPANDED) & TVIS_EXPANDED)
			m_TreeCtrl.Expand(currItem, TVE_COLLAPSE);
		else if (parentItem)
			m_TreeCtrl.SelectItem(parentItem);
	}
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnVKRight()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM childItem = m_TreeCtrl.GetChildItem(currItem);
		if (!(m_TreeCtrl.GetItemState(currItem, TVIS_EXPANDED) & TVIS_EXPANDED))
			m_TreeCtrl.Expand(currItem, TVE_EXPAND);
		else if (childItem)
			m_TreeCtrl.SelectItem(childItem);
	}
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateVKMove(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateCollapseAll(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnUpdateExpandAll(CCmdUI* pCmdUI)
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();

	pCmdUI->Enable(currItem != NULL);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnCollapseAll()
{
	m_TreeCtrl.ExpandAll(TVE_COLLAPSE);
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnExpandAll()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		m_TreeCtrl.ExpandAll(currItem, TVE_EXPAND);
	}
	else m_TreeCtrl.ExpandAll(TVE_EXPAND);
}

// Dropped accept
//-----------------------------------------------------------------------------
BOOL CRSReportTreeView::IsDropText(COleDataObject* pDataObject)
{
	return pDataObject->IsDataAvailable(CF_UNICODETEXT) || pDataObject->IsDataAvailable(CF_TEXT);
}

// When assingning a LPCSTR to a CStringW or a LPCWSTR to a CStringA,
// ANSI / Unicode conversion is performed.
//-----------------------------------------------------------------------------
void CRSReportTreeView::GetDropText(CString& str, COleDataObject* pDataObject)
{
	str = _T("");
	if (pDataObject->IsDataAvailable(CF_UNICODETEXT))
	{
		HGLOBAL hGlobal = pDataObject->GetGlobalData(CF_UNICODETEXT);
		LPCWSTR lpSrc = static_cast<LPCWSTR>(::GlobalLock(hGlobal));
		str = lpSrc;
		::GlobalUnlock(hGlobal);
	}
	else if (pDataObject->IsDataAvailable(CF_TEXT))
	{
		HGLOBAL hGlobal = pDataObject->GetGlobalData(CF_TEXT);
		LPCSTR lpSrc = static_cast<LPCSTR>(::GlobalLock(hGlobal));
		str = lpSrc;
		::GlobalUnlock(hGlobal);
	}
}

//-----------------------------------------------------------------------------
DROPEFFECT CRSReportTreeView::OnDragEnter(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	if (IsDropText(pDataObject))
	{
		m_PointDrop = point;

		if ((dwKeyState & MK_CONTROL) == MK_CONTROL)
			return DROPEFFECT_COPY;
		else
			return DROPEFFECT_MOVE;
	}
	return DROPEFFECT_NONE;
}

//-----------------------------------------------------------------------------
DROPEFFECT CRSReportTreeView::OnDragOver(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point)
{
	DROPEFFECT dropEffect = DROPEFFECT_MOVE;
	if (IsDropText(pDataObject))
	{
		ClientToScreen(&point);
		m_TreeCtrl.ScreenToClient(&point);
		m_PointDrop = point;

		if ((dwKeyState & MK_CONTROL) == MK_CONTROL)
			dropEffect = DROPEFFECT_COPY;
		else
			dropEffect = DROPEFFECT_MOVE;

		// Expand and highlight the item under the mouse and 
		UINT uFlags;
		HTREEITEM hTItem = m_TreeCtrl.HitTest(point, &uFlags);
		if (hTItem != NULL && (TVHT_ONITEMBUTTON & uFlags))
		{
			m_TreeCtrl.Expand(hTItem, TVE_EXPAND);
		}

		// Scroll Tree control depending on mouse position
		CRect rectClient;
		m_TreeCtrl.GetClientRect(&rectClient);
		ClientToScreen(&rectClient);
		ScreenToClient(&rectClient);

		int nScrollDir = -1;
		if (point.y >= rectClient.bottom - TREE_SCROLL_HEIGHT_AREA)
			nScrollDir = SB_LINEDOWN;
		else
			if ((point.y <= rectClient.top + TREE_SCROLL_HEIGHT_AREA))
				nScrollDir = SB_LINEUP;

		if (nScrollDir != -1)
		{
			int nScrollPos = GetScrollPos(SB_VERT);
			WPARAM wParam = MAKELONG(nScrollDir, nScrollPos);
			SendMessageToDescendants(WM_VSCROLL, wParam);
		}

		nScrollDir = -1;
		if (point.x <= rectClient.left + TREE_SCROLL_HEIGHT_AREA)
			nScrollDir = SB_LINELEFT;
		else
			if (point.x >= rectClient.right - TREE_SCROLL_HEIGHT_AREA)
				nScrollDir = SB_LINERIGHT;

		if (nScrollDir != -1)
		{
			int nScrollPos = GetScrollPos(SB_VERT);
			WPARAM wParam = MAKELONG(nScrollDir, nScrollPos);
			SendMessageToDescendants(WM_HSCROLL, wParam);
		}
	}
	return dropEffect;
}

DROPEFFECT CRSReportTreeView::OnDragScroll(CWnd* pWnd, DWORD dwKeyState, CPoint /*point*/)
{
	//    TRACE("In CGridDropTarget::OnDragScroll\n");   
	if (pWnd->GetSafeHwnd() == m_TreeCtrl.GetSafeHwnd())
	{
		if ((dwKeyState & MK_CONTROL) == MK_CONTROL)
			return DROPEFFECT_COPY;
		else
			return DROPEFFECT_MOVE;
	}
	else
		return DROPEFFECT_NONE;
}

//-----------------------------------------------------------------------------
BOOL CRSReportTreeView::OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point)
{
	BOOL bRet;
	bRet = ReadHdropData(pDataObject, dropEffect);
	return bRet;
}

//-----------------------------------------------------------------------------
void CRSReportTreeView::OnDragLeave()
{

}

// Drag & Drop View Target
//-----------------------------------------------------------------------------
BOOL CRSReportTreeView::ReadHdropData(COleDataObject* pDataObject, DROPEFFECT dropEffect)
{
	CString strCommand;
	GetDropText(strCommand, pDataObject);

	CRSTreeCtrl*		sourceTreeCtrl = NULL;

	//guardo da che view mi arriva il drag & drop

	if (strCommand.Compare(DRAGDROP_WOORM_ENGINE_VIEW) == 0)		//engine
	{
		CRSReportTreeView*	sourceTreeView = GetDocument()->GetWoormFrame()->GetEngineTreeView();
		if (!sourceTreeView)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		sourceTreeCtrl = &(sourceTreeView->m_TreeCtrl);
		ASSERT_VALID(sourceTreeCtrl);
	}
	else if (strCommand.Compare(DRAGDROP_WOORM_DB_VIEW) == 0)		//database
	{
		CRSToolBoxDBView* sourceTreeView = GetDocument()->GetWoormFrame()->m_pToolBoxDBView;
		if (!sourceTreeView)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		sourceTreeCtrl = &(sourceTreeView->m_TreeCtrl);
		ASSERT_VALID(sourceTreeCtrl);
	}
	else if (strCommand.Compare(DRAGDROP_WOORM_OBJECTS_VIEW) == 0)	//toolbox
	{
		CRSToolBoxObjectsView* sourceTreeView = GetDocument()->GetWoormFrame()->m_pToolBoxTreeView;
		if (!sourceTreeView)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		sourceTreeCtrl = &(sourceTreeView->m_TreeCtrl);
		ASSERT_VALID(sourceTreeCtrl);
	}
	else if (strCommand.Compare(DRAGDROP_WOORM_LAYOUT_VIEW) == 0)	//layout
	{
		CRSReportTreeView*	sourceTreeView = GetDocument()->GetWoormFrame()->GetLayoutTreeView();
		if (!sourceTreeView)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		sourceTreeCtrl = &(sourceTreeView->m_TreeCtrl);
		ASSERT_VALID(sourceTreeCtrl);
	}

	if (sourceTreeCtrl)
	{
		UINT uFlags2;

		/*ClientToScreen(&m_PointDrop);
		m_TreeCtrl.ScreenToClient(&m_PointDrop);*/

		HTREEITEM targetHt = m_TreeCtrl.HitTest(m_PointDrop, &uFlags2);
		HTREEITEM sourceHt = sourceTreeCtrl->GetDragItem();
#ifdef _DEBUG
		CString strTarget = m_TreeCtrl.GetItemText(targetHt);
		CString strSource = sourceTreeCtrl->GetItemText(sourceHt);
#endif
		if (targetHt != NULL &&
			((uFlags2 & TVHT_ONITEM) || (uFlags2 & TVHT_ONITEMINDENT) || (uFlags2 & TVHT_ONITEMRIGHT)))
		{
			OnDropAction(sourceTreeCtrl, sourceHt, targetHt, dropEffect);
		}
		//m_PointDrop
		BOOL bCommand = FALSE;
	}

	return TRUE;
}

void CRSReportTreeView::MoveOrCloneLayoutObjects(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect)
{
	//target parent
	HTREEITEM targetParentHt = m_TreeCtrl.GetParentItem(targetHt);
	if (!targetParentHt) return;//se non ha parent vuol dire che non è layout
	CNodeTree* pTargetNodeParent = (CNodeTree*)m_TreeCtrl.GetItemData(targetParentHt);
	//source parent
	HTREEITEM sourceParentHt = sourceTreeCtrl->GetParentItem(sourceHt);
	ASSERT(sourceParentHt);
	if (!sourceParentHt) return;
	CNodeTree* pSourceNodeParent = (CNodeTree*)sourceTreeCtrl->GetItemData(sourceParentHt);
	//select obj
	BaseObj* pSourceObj = dynamic_cast<BaseObj*>(pSourceNode->m_pItemData);
	ASSERT_VALID(pSourceObj);
	if (!pSourceObj)
		return;
	//source layout
	CLayout* pSourceLayout = dynamic_cast<CLayout*>(pSourceNodeParent->m_pItemData);
	ASSERT_VALID(pSourceLayout);
	if (!pSourceLayout)
		return;
	//target layout
	CLayout* pTargetLayout;

	//MOVING on layout node
	if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_LAYOUT && pTargetNode != pSourceNodeParent)
	{
		pTargetLayout = dynamic_cast<CLayout*>(pTargetNode->m_pItemData);
		ASSERT_VALID(pTargetLayout);
		if (!pTargetLayout)	return;
	}
	//MOVING on layout node's child
	else if (pTargetNodeParent && pTargetNodeParent->m_NodeType == CNodeTree::ENodeType::NT_LAYOUT && pTargetNodeParent != pSourceNodeParent)
	{
		pTargetLayout = dynamic_cast<CLayout*>(pTargetNodeParent->m_pItemData);
		ASSERT_VALID(pTargetLayout);
		if (!pTargetLayout)	return;
	}
	//NO MOVING
	else
		return;
#ifdef _DEBUG
	//target and source info
	CString strLayoutTarget = m_TreeCtrl.GetItemText(targetParentHt);
	CString strLayoutSource = sourceTreeCtrl->GetItemText(sourceParentHt);
#endif
	//sposto la selezione multipla (se c'è)
	if (GetDocument()->m_pMultipleSelObj)
		for (int i = 0; i < GetDocument()->m_pMultipleSelObj->GetSize(); i++)
		{
			if (dropEffect == DROPEFFECT_MOVE)
				GetDocument()->LayoutMoveObject(pSourceLayout, pTargetLayout, GetDocument()->m_pMultipleSelObj->GetObjAt(i));
			else if (dropEffect == DROPEFFECT_COPY)
				GetDocument()->LayoutCopyObject(pSourceLayout, pTargetLayout, GetDocument()->m_pMultipleSelObj->GetObjAt(i));

		}

	//o la selezione singola
	else
	{
		if (dropEffect == DROPEFFECT_MOVE)
			GetDocument()->LayoutMoveObject(pSourceLayout, pTargetLayout, pSourceObj);
		else if (dropEffect == DROPEFFECT_COPY)
			GetDocument()->LayoutCopyObject(pSourceLayout, pTargetLayout, pSourceObj);
	}
	//aggiorno
	GetDocument()->RefreshRSTree(ERefreshEditor::Layouts);
	//riespando i nodi interessati (layout sorgente e item destinatari
	GetDocument()->SelectRSTreeItemData(ERefreshEditor::Layouts, pSourceLayout);
	GetDocument()->SelectRSTreeItemData(ERefreshEditor::Layouts, pSourceObj);

}

//-----------------------------------------------------------------------------
void CRSReportTreeView::AddRuleFromDrop(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect)
{
	ASSERT_VALID(pTargetNode);
	if (
		pTargetNode->m_NodeType != CNodeTree::ENodeType::NT_ROOT_RULES
		&&
		pTargetNode->m_NodeType != CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
		)
	{
		ASSERT(FALSE);
		return;
	}

	//---------

	ASSERT_VALID(pSourceNode);
	if (pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBTABLE && pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBVIEW)
	{
		ASSERT(FALSE);
		return;
	}
	SqlCatalogEntry* pCatalogEntry = dynamic_cast<SqlCatalogEntry*>(pSourceNode->m_pItemData);
	ASSERT_VALID(pCatalogEntry);
	if (!pCatalogEntry)
	{
		return;
	}
	//-----
	CHelperSqlCatalog::CTableColumns* pTargetTC = NULL;
	CHelperSqlCatalog::CTableForeignTablesKeys* pFTK = NULL;
	CHelperExternalReferences::CTableSingleExtRef* pSER = NULL;

	CString sTargetTableName;
	TblRuleData* pDroppedTblRule = NULL;
	if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
	{
		pDroppedTblRule = dynamic_cast<TblRuleData*>(pTargetNode->m_pItemData);
		ASSERT_VALID(pDroppedTblRule);
		if (pDroppedTblRule)
		{
			ASSERT(pDroppedTblRule->m_arSqlTableJoinInfoArray.GetSize());

			//tentiamo la join
			sTargetTableName = pDroppedTblRule->m_arSqlTableJoinInfoArray[0]->GetTableName();

			FindJoinReferences
			(
				pCatalogEntry->m_strTableName,
				sTargetTableName,
				pTargetTC,
				pFTK,
				pSER
			);
		}
	}

	//-----
	TblRuleData* pTblRule = new TblRuleData(*GetDocument()->GetEditorSymTable(), AfxGetDefaultSqlConnection(), pCatalogEntry->m_strTableName);

	RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
	pRuleData->Add(pTblRule);

	//----
	if (pDroppedTblRule && (pFTK || pSER))
	{
		ASSERT_VALID(pTargetTC);
		if (pFTK)
		{
			ASSERT_VALID(pFTK);
			CString sWhere;

			for (int k = 0; k < pFTK->m_pParent->m_arForeignKeys.GetSize(); k++)
			{
				CHelperSqlCatalog::CTableForeignTablesKeys* pFTKaux = dynamic_cast<CHelperSqlCatalog::CTableForeignTablesKeys*>(pFTK->m_pParent->m_arForeignKeys[k]);
				if (pFTKaux)
				{
					CString sTargetVar = pDroppedTblRule->GetPublicNameOf
					(
						pDroppedTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 1 ?
						sTargetTableName + '.' + pFTKaux->m_sColumnName :
						pFTKaux->m_sColumnName
					);
					if (sTargetVar.IsEmpty())
					{
						DataStr dummy; SqlColumnInfoObject sci(sTargetTableName, pFTKaux->m_sColumnName, dummy);

						int idx = pTargetTC->m_arSortedColumns.BinarySearch(&sci);
						if (idx < 0)
						{
							goto l_after_join;
						}
						SqlColumnInfoObject* pColInfo = (SqlColumnInfoObject*)(pTargetTC->m_arSortedColumns[idx]);
						if (!pColInfo)
						{
							goto l_after_join;
						}
						WoormField* pF = GetDocument()->AddDBColumns_CreateField(pColInfo, pDroppedTblRule, 0);
						if (!pF)
						{
							goto l_after_join;
						}
						sTargetVar = pF->GetName();
					}

					if (!sWhere.IsEmpty())
						sWhere += L" AND ";

					sWhere += pFTKaux->m_pParent->m_sForeignTableName + '.' + pFTKaux->m_sForeignColumnName + L" = " + sTargetVar;
				}
			}
			if (sWhere.IsEmpty())
				goto l_after_join;

			pTblRule->AddWhereClause();
			Parser lex(sWhere);
			if (!pTblRule->GetWhereClause()->Parse(lex))
			{
				CString sError = lex.BuildErrMsg(TRUE);
				TRACE(sError);
				pTblRule->GetWhereClause()->Reset(FALSE);
				goto l_after_join;
			}
		}
		else if (pSER)
		{
			ASSERT_VALID(pSER);

			CString sTargetVar = pDroppedTblRule->GetPublicNameOf
			(
				pDroppedTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 1 ?
				sTargetTableName + '.' + pSER->m_sExtPrimaryKey :
				pSER->m_sExtPrimaryKey
			);
			if (sTargetVar.IsEmpty())
			{
				DataStr dummy; SqlColumnInfoObject sci(sTargetTableName, pSER->m_sExtPrimaryKey, dummy);

				int idx = pTargetTC->m_arSortedColumns.BinarySearch(&sci);
				if (idx < 0)
				{
					goto l_after_join;
				}
				SqlColumnInfoObject* pColInfo = (SqlColumnInfoObject*)(pTargetTC->m_arSortedColumns[idx]);
				if (!pColInfo)
				{
					goto l_after_join;
				}
				WoormField* pF = GetDocument()->AddDBColumns_CreateField(pColInfo, pDroppedTblRule, 0);
				if (!pF)
				{
					goto l_after_join;
				}
				sTargetVar = pF->GetName();
			}

			CString sWhere = pSER->m_sExtTableName + '.' + pSER->m_sForeignKey + L" = " +
				sTargetVar;

			pTblRule->AddWhereClause();
			Parser lex(sWhere);
			if (!pTblRule->GetWhereClause()->Parse(lex))
			{
				CString sError = lex.BuildErrMsg(TRUE);
				TRACE(sError);
				pTblRule->GetWhereClause()->Reset(FALSE);
			}
		}
	}

l_after_join:
	//----
	HTREEITEM ht = GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pTblRule);
	if (ht)
	{
		CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
		ht = pTree->FindItemText(_T("Unselected Columns"), ht);
		if (ht)
		{
			pTree->SelectItem(ht);
			pTree->Expand(ht, TVE_EXPAND);
			pTree->EnsureVisible(ht);
		}
	}
}

//--------------------------------------------------------------------------------------------------------
void CRSReportTreeView::FindJoinReferences
(
	const CString& sDraggedTableName,
	const CString& sTargetTableName,
	CHelperSqlCatalog::CTableColumns*& pTargetTC,
	CHelperSqlCatalog::CTableForeignTablesKeys*& pFTK,
	CHelperExternalReferences::CTableSingleExtRef*& pSER
)
{
	pFTK = NULL; pSER = NULL;
	pTargetTC = GetDocument()->m_pEditorManager->GetHelperSqlCatalog()->FindEntryByName(sTargetTableName);
	ASSERT_VALID(pTargetTC);
	if (!pTargetTC)
	{
		return;
	}

	Array& arFK = GetDocument()->m_pEditorManager->GetHelperSqlCatalog()->GetForeignKeys(pTargetTC);
	if (arFK.GetCount())
	{
		for (int i = 0; i < arFK.GetCount(); i++)
		{
			CHelperSqlCatalog::CTableForeignTables* pFK = dynamic_cast<CHelperSqlCatalog::CTableForeignTables*>(arFK.GetAt(i));
			if (pFK->m_sForeignTableName.CompareNoCase(sDraggedTableName) == 0)
			{
				pFTK = (CHelperSqlCatalog::CTableForeignTablesKeys*) pFK->m_arForeignKeys[0];
				break;
			}
		}
	}

	if (!pFTK)
	{
		CHelperExternalReferences* pH = GetDocument()->m_pEditorManager->GetHelperExternalReferences();
		CHelperExternalReferences::CTableExtRefs* pER = pH->GetTableExtRefs(pTargetTC->m_pCatalogEntry->m_strTableName);

		if (pER)
		{
			for (int i = 0; i < pER->m_arExtRefs.GetCount(); i++)
			{
				CHelperExternalReferences::CTableSingleExtRef* pauxSER = dynamic_cast<CHelperExternalReferences::CTableSingleExtRef*>(pER->m_arExtRefs.GetAt(i));
				if (pauxSER->m_sExtTableName.CompareNoCase(sDraggedTableName) == 0)
				{
					pSER = pauxSER;
					break;
				}
			}
		}
	}
}

//--------------------------------------------------------------------------------------------------------
void CRSReportTreeView::AddJoinFromDrop(CRSTreeCtrl*, CNodeTree* pSourceNode, HTREEITEM, CNodeTree* pTargetNode, HTREEITEM, DROPEFFECT)
{
	ASSERT_VALID(pTargetNode);
	if (
		//pTargetNode->m_NodeType != CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
		//&&
		pTargetNode->m_NodeType != CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM
		&&
		pTargetNode->m_NodeType != CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO
		)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pSourceNode);
	if (
		pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBTABLE
		&&
		pSourceNode->m_NodeType != CNodeTree::NT_LIST_DBVIEW)
	{
		ASSERT(FALSE);
		return;
	}

	TblRuleData* pTblRule = NULL;

	if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
		pTblRule = dynamic_cast<TblRuleData*>(pTargetNode->m_pItemData);
	else if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM)
		pTblRule = dynamic_cast<TblRuleData*>(pTargetNode->m_pItemData);
	else if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO)
		pTblRule = dynamic_cast<TblRuleData*>(pTargetNode->m_pParentItemData);

	CString sTargetTableName;
	int size = pTblRule->m_arSqlTableJoinInfoArray.GetSize();

	for (int i = 0; i < size; i++)
	{
		if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE)
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
		else if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM)
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
		else if (pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO)
		{
			SqlTableInfo* pTI = dynamic_cast<SqlTableInfo*>(pTargetNode->m_pItemData);
			ASSERT_VALID(pTI);
			if (!pTI)
				sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[i]->GetTableName();
			else
				sTargetTableName = pTI->GetTableName();
		}

		ASSERT_VALID(pTblRule);
		if (!pTblRule)
			continue;

		CHelperSqlCatalog::CTableColumns* pDraggedTC = dynamic_cast<CHelperSqlCatalog::CTableColumns*>(pSourceNode->m_pParentItemData);
		ASSERT_VALID(pDraggedTC);
		if (!pDraggedTC)
		{
			continue;
		}

		CHelperSqlCatalog::CTableColumns* pTargetTC = NULL;
		CHelperSqlCatalog::CTableForeignTablesKeys* pFTK = NULL;
		CHelperExternalReferences::CTableSingleExtRef* pSER = NULL;

		FindJoinReferences
		(
			pDraggedTC->m_pCatalogEntry->m_strTableName,
			sTargetTableName,
			pTargetTC,
			pFTK,
			pSER
		);

		AddJoin(pTblRule, pDraggedTC->m_pCatalogEntry->m_pTableInfo, pDraggedTC->m_pCatalogEntry->m_strTableName, sTargetTableName, pFTK, pSER);
	}
}

//--------------------------------------------------------------------------------------------------------
void CRSReportTreeView::AddJoin
(
	TblRuleData* pTblRule,
	SqlTableInfo* tableInfo,
	CString sourceTableName,
	CString sTargetTableName,
	CHelperSqlCatalog::CTableForeignTablesKeys* pFTK/* = NULL*/,
	CHelperExternalReferences::CTableSingleExtRef* pSER/* = NULL*/,
	BOOL reloadTree/*=TRUE*/
)
{
	ASSERT_VALID(pTblRule);

	int pos = -1;
	pos = pTblRule->m_arSqlTableJoinInfoArray.Find(sourceTableName);
	if (pos == -1)
	{
		pos = pTblRule->AddJoinTable(tableInfo);
	}

	if (pFTK)
	{
		ASSERT(pSER == NULL);
		ASSERT_VALID(pFTK);
		ASSERT(pos > 0);

		WClause* pJoinOn = new WClause(pTblRule->GetConnection(), pTblRule->GetSymTable(), &pTblRule->m_arSqlTableJoinInfoArray);
		pJoinOn->SetJoinOnClause();
		pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos] = pJoinOn;

		pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::INNER;

		if (sTargetTableName.IsEmpty())
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[pos - 1]->GetTableName();

		CString sOn = sTargetTableName + '.' + pFTK->m_sColumnName +
			L" = " +
			pFTK->m_pParent->m_sForeignTableName + '.' + pFTK->m_sForeignColumnName;

		if (pFTK->m_pParent->m_arForeignKeys.GetSize() > 1)
		{
			for (int k = 1; k < pFTK->m_pParent->m_arForeignKeys.GetSize(); k++)
			{
				CHelperSqlCatalog::CTableForeignTablesKeys* pFTKaux = dynamic_cast<CHelperSqlCatalog::CTableForeignTablesKeys*>(pFTK->m_pParent->m_arForeignKeys[k]);
				if (pFTKaux)
				{
					sOn += L" AND " + sTargetTableName + '.' + pFTKaux->m_sColumnName +
						L" = " +
						pFTK->m_pParent->m_sForeignTableName + '.' + pFTKaux->m_sForeignColumnName;
				}
			}
		}

		Parser lex(sOn);
		if (!pJoinOn->Parse(lex))
		{
			CString sError = lex.BuildErrMsg(TRUE);
			AfxMessageBox(sError);
			pJoinOn->Reset(FALSE);
		}
	}
	else if (pSER)
	{
		ASSERT_VALID(pSER);
		ASSERT(pos > 0);

		WClause* pJoinOn = new WClause(pTblRule->GetConnection(), pTblRule->GetSymTable(), &pTblRule->m_arSqlTableJoinInfoArray);
		pJoinOn->SetJoinOnClause();
		pTblRule->m_arSqlTableJoinInfoArray.m_arJoinOn[pos] = pJoinOn;

		pTblRule->m_arSqlTableJoinInfoArray.m_arJoinType[pos] = SqlTableJoinInfoArray::EJoinType::LEFT_OUTER;

		if (sTargetTableName.IsEmpty())
			sTargetTableName = pTblRule->m_arSqlTableJoinInfoArray[pos - 1]->GetTableName();

		CString sOn = sTargetTableName + '.' + pSER->m_sForeignKey +
			L" = " +
			pSER->m_sExtTableName + '.' + pSER->m_sExtPrimaryKey;

		Parser lex(sOn);
		if (!pJoinOn->Parse(lex))
		{
			CString sError = lex.BuildErrMsg(TRUE);
			AfxMessageBox(sError);
			pJoinOn->Reset(FALSE);
		}
	}

	pTblRule->m_arSqlTableJoinInfoArray.QualifiedLinks();

	GetDocument()->SetModifiedFlag();

	if (!reloadTree)
		return;

	HTREEITEM ht = GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pTblRule);
	if (ht)
	{
		CRSTreeCtrl* pTree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
		ht = pTree->SelectRSTreeItemData(tableInfo, ht);
		if (ht)
			ht = pTree->FindItemText(_T("Unselected Columns"), ht);
		if (ht)
		{
			pTree->SelectItem(ht);
			pTree->Expand(ht, TVE_EXPAND);
			pTree->EnsureVisible(ht);
		}
	}
}

//--------------------------------------------------------------------------------------------------------
void CRSReportTreeView::AddColumnsFromDrop(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect)
{
	ASSERT_VALID(pTargetNode);
	if (pTargetNode->m_NodeType != CNodeTree::NT_RULE_QUERY_GROUP_COLUMNS)
	{
		ASSERT(FALSE);
		return;
	}

	ASSERT_VALID(pSourceNode);
	if (pSourceNode->m_NodeType != CNodeTree::NT_LIST_COLUMN_INFO)
	{
		ASSERT(FALSE);
		return;
	}

	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pTargetNode->m_pParentItemData);
	ASSERT_VALID(pTblRule);
	if (!pTblRule)
	{
		return;
	}
	SqlTableInfo* pTableInfo = dynamic_cast<SqlTableInfo*>(pTargetNode->m_pItemData);
	ASSERT_VALID(pTableInfo);

	int idx = pTblRule->m_arSqlTableJoinInfoArray.Find(pTableInfo->GetTableName());
	ASSERT(idx >= 0);
	//if (idx < 0) return FALSE;

	//-----

	WoormField* pNewHiddenField = NULL;
	if (GetDocument()->m_pNodesSelection && GetDocument()->m_pNodesSelection->GetCount() > 0)
	{
		pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(GetDocument()->m_pNodesSelection, TRUE, pTblRule, idx);
	}
	else
		pNewHiddenField = GetDocument()->AddDBColumns_FromToolBar(pSourceNode, TRUE, pTblRule, idx, TRUE);

	/*HTREEITEM ht = */GetDocument()->SelectRSTreeItemData(ERefreshEditor::Rules, pNewHiddenField);
}
//-----------------------------------------------------------------------------

void CRSReportTreeView::OnDropAction(CRSTreeCtrl* sourceTreeCtrl, HTREEITEM sourceHt, HTREEITEM targetHt, DROPEFFECT dropEffect)
{
#ifdef _DEBUG
	//target and source info
	CString strTarget = m_TreeCtrl.GetItemText(targetHt);
	CString strSource = sourceTreeCtrl->GetItemText(sourceHt);
#endif
	//target 
	CNodeTree* pTargetNode = (CNodeTree*)m_TreeCtrl.GetItemData(targetHt);
	//source
	CNodeTree* pSourceNode = (CNodeTree*)sourceTreeCtrl->GetItemData(sourceHt);

	if (pTargetNode && pSourceNode)
	{
		//MOVE OR CLONE LAYOUT OBJECTS
		if (
			sourceTreeCtrl->IsLayoutNodeType(pSourceNode->m_NodeType)
			&&
			(
				m_TreeCtrl.IsLayoutNodeType(pTargetNode->m_NodeType)
				||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_LAYOUT
				)
			)
		{
			MoveOrCloneLayoutObjects(sourceTreeCtrl, pSourceNode, sourceHt, pTargetNode, targetHt, dropEffect);
			return;
		}

		//---------------------------------------
		//ADD RULE
		if (
			(
				pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBTABLE
				||
				pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBVIEW
				)
			&&
			(
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_RULES
				||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
				)
			)
		{
			AddRuleFromDrop(sourceTreeCtrl, pSourceNode, sourceHt, pTargetNode, targetHt, dropEffect);
			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

		//ADD RULE AND select columns
		if (
			pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO
			&&
			(
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_RULES
				||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
				)
			)
		{
			do {
				CNodeTreeArray* pSavedSelection = GetDocument()->m_pNodesSelection;
				GetDocument()->m_pNodesSelection = NULL;

				CRSToolBoxDBView* sourceTreeView = GetDocument()->GetWoormFrame()->m_pToolBoxDBView;
				if (!sourceTreeView)
					break;
				if (sourceTreeCtrl != &(sourceTreeView->m_TreeCtrl))
					break;

				CNodeTree* pSourceTable = sourceTreeCtrl->GetParentNode(pSourceNode->m_ht);
				if (!pSourceTable)
					break;

				RuleDataArray* pRuleData = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
				TblRuleData* pOldTblRule = pRuleData->m_pLastTblRule;

				AddRuleFromDrop(sourceTreeCtrl, pSourceTable, sourceHt, pTargetNode, targetHt, dropEffect);

				if (pSavedSelection)
					ASSERT_VALID(pSavedSelection);
				GetDocument()->m_pNodesSelection = pSavedSelection;

				if (pOldTblRule == pRuleData->m_pLastTblRule)
					break;

				//il tree è cambiato! devo ricercare i nodi
				CNodeTree* pRuleNode = m_TreeCtrl.FindNode(pRuleData->m_pLastTblRule, m_TreeCtrl.m_htRules);
				if (!pRuleNode)
					break;

				CNodeTree* pColumnsNode = m_TreeCtrl.GetDescendantNode(pRuleNode->m_ht, CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS);
				if (!pColumnsNode)
					break;

				AddColumnsFromDrop(sourceTreeCtrl, pSourceNode, pSourceNode->m_ht, pColumnsNode, pColumnsNode->m_ht, dropEffect);

			} while (false);
			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

		//---------------------------------------
		//ADD JOIN
		if (
			(
				pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBTABLE
				||
				pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_DBVIEW
				)
			&&
			(
				//pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
				//||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM
				||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO
				)
			)
		{
			AddJoinFromDrop(sourceTreeCtrl, pSourceNode, sourceHt, pTargetNode, targetHt, dropEffect);
			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

		//ADD SQL COLUMN INFO
		if (
			pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO
			&&
			pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS
			)
		{
			AddColumnsFromDrop(sourceTreeCtrl, pSourceNode, sourceHt, pTargetNode, targetHt, dropEffect);
			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

		//ADD JOIN and SELECT columns
		if (
			pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_LIST_COLUMN_INFO
			&&
			(
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM
				||
				pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_TABLEINFO
				)
			)
		{
			do {
				CNodeTreeArray* pSavedSelection = GetDocument()->m_pNodesSelection;
				GetDocument()->m_pNodesSelection = NULL;

				CRSToolBoxDBView* sourceTreeView = GetDocument()->GetWoormFrame()->m_pToolBoxDBView;
				if (!sourceTreeView)
					break;
				if (sourceTreeCtrl != &(sourceTreeView->m_TreeCtrl))
					break;

				CNodeTree* pSourceTable = sourceTreeCtrl->GetParentNode(pSourceNode->m_ht);
				if (!pSourceTable)
					break;

				TblRuleData* pTbl = dynamic_cast<TblRuleData*>(pTargetNode->m_pItemData);
				if (!pTbl)
					pTbl = dynamic_cast<TblRuleData*>(pTargetNode->m_pParentItemData);
				if (!pTbl)
					pTbl = dynamic_cast<TblRuleData*>(pTargetNode->m_pAncestorItemData);

				AddJoinFromDrop(sourceTreeCtrl, pSourceTable, sourceHt, pTargetNode, targetHt, dropEffect);

				if (pSavedSelection)
					ASSERT_VALID(pSavedSelection);
				GetDocument()->m_pNodesSelection = pSavedSelection;

				if (!pTbl)
					break;

				//il tree è cambiato! devo ricercare i nodi
				CNodeTree* pRuleNode = m_TreeCtrl.FindNode(pTbl, m_TreeCtrl.m_htRules);
				if (!pRuleNode)
					break;

				CNodeTree* pFromNode = m_TreeCtrl.GetDescendantNode(pRuleNode->m_ht, CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_FROM);
				if (!pFromNode)
					break;

				HTREEITEM htNewTable = m_TreeCtrl.GetLastChild(pFromNode->m_ht);
				if (!htNewTable)
					break;

				CNodeTree* pColumnsNode = m_TreeCtrl.GetDescendantNode(htNewTable, CNodeTree::ENodeType::NT_RULE_QUERY_GROUP_COLUMNS);
				if (!pColumnsNode)
					break;

				AddColumnsFromDrop(sourceTreeCtrl, pSourceNode, pSourceNode->m_ht, pColumnsNode, pColumnsNode->m_ht, dropEffect);
			} while (false);
			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

		//ADD NAMED QUERY RULE
		if (
			pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_NAMED_QUERY
			&&
			pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_RULES
			)
		{
			QueryObjItem* pobjQ = dynamic_cast<QueryObjItem*>(pSourceNode->m_pItemData);
			ASSERT_VALID(pobjQ);
			if (!pobjQ) return;

			QueryObject* pQuery = pobjQ->GetQueryObject();
			ASSERT_VALID(pQuery);
			if (!pQuery) return;

			if (pQuery->AllQueryColumns().GetSize() == 0)
				return;

			QueryRuleData*  pQueryRule = new QueryRuleData(*GetDocument()->m_pEditorManager->GetSymTable());
			pQueryRule->SetQueryItem(pobjQ);

			RuleDataArray* pRules = GetDocument()->m_pEditorManager->GetPrgData()->GetRuleData();
			pRules->Add(pQueryRule);

			SAFE_DELETE(GetDocument()->m_pNodesSelection);

			GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pQueryRule);
			return;
		}

		//CREATE NAMED QUERY FROM TableRule
		if (
			pSourceNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE
			&&
			pTargetNode->m_NodeType == CNodeTree::ENodeType::NT_ROOT_QUERIES
			)
		{
			QueryObjectData* pQueries = dynamic_cast<QueryObjectData*>(pTargetNode->m_pItemData);
			ASSERT_VALID(pQueries);
			if (!pQueries) return;

			TblRuleData* pTRule = dynamic_cast<TblRuleData*>(pSourceNode->m_pItemData);
			ASSERT_VALID(pTRule);
			if (!pTRule) return;

			CString sQuery = pTRule->ToNamedQuery();
			if (!sQuery.IsEmpty())
			{
				CString sName = pTRule->GetTableNames();
				int idx = sName.Find(',');
				if (idx > -1)
				{
					//sName.Replace(L", ", L"_");
					sName = sName.Left(idx);
				}

				sName = L"Query_" + sName;
				if (pQueries->Get(sName))
				{
					int n = 1;
					for (; pQueries->Get(cwsprintf(L"%s%d", sName, n)); n++);

					sName += cwsprintf(L"%d", n);
				}

				QueryObjItem* pNewQry = new QueryObjItem(pTRule->GetSymTable(), sName);
				if (!pNewQry->Parse(sQuery))
				{
					//ShowDiagnostic(pNewQry->GetError());

					delete pNewQry;
					return;
				}
				pQueries->AddNew(pNewQry);
				GetDocument()->RefreshRSTree(ERefreshEditor::Queries, pNewQry);
			}

			SAFE_DELETE(GetDocument()->m_pNodesSelection);
			return;
		}

	}
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSLayoutTreeView, CRSReportTreeView)

CRSLayoutTreeView::CRSLayoutTreeView()
	:
	CRSReportTreeView(L"Layout", IDD_RS_LayoutTreeView)
{
	m_TreeCtrl.EnableDrag(DRAGDROP_WOORM_LAYOUT_VIEW);
}

// -----------------------------------------------------------------------------
void CRSLayoutTreeView::OnBuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->SetLayoutTreeView(this);

	FillTree();
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSLayoutTreeView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pLayoutPane->GetToolBar();
}

//------------------------------------------------------------------------------
BOOL CRSLayoutTreeView::PreTranslateMessage(MSG* pMsg)
{
	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN)
	{
		GetDocument()->OnObjectCut();
		return TRUE;
	}
	return __super::PreTranslateMessage(pMsg);
}

// -----------------------------------------------------------------------------
BOOL CRSLayoutTreeView::FillTree()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument()->m_bAllowEditing)
		return FALSE;

	CWaitCursor wc;

	//if (!m_TreeCtrl.m_htProperties)
	//	m_TreeCtrl.m_htProperties = m_TreeCtrl.AddNode(_T("Properties"), CNodeTree::ENodeType::NT_PROPERTIES, NULL, GetDocument()->m_pDocProperties);
	//if (!m_TreeCtrl.m_htPageInfo)
	//	m_TreeCtrl.m_htPageInfo = m_TreeCtrl.AddNode(_T("Page info"), CNodeTree::ENodeType::NT_PAGE, NULL, &GetDocument()->m_PageInfo);

	m_TreeCtrl.FillLayouts();
	m_TreeCtrl.Expand(m_TreeCtrl.m_htLayouts, TVE_EXPAND);
	//m_TreeCtrl.Expand(m_TreeCtrl.m_htLayoutDefault, TVE_EXPAND);

	m_TreeCtrl.FillLinks();

	//m_TreeCtrl.FillVariables(TRUE, FALSE);
	//m_TreeCtrl.FillRules();
	//m_TreeCtrl.FillGroupHaving();
	//m_TreeCtrl.FillEvents();
	//m_TreeCtrl.FillProcedures();
	//m_TreeCtrl.FillQueries();
	//m_TreeCtrl.FillDialogs();

	//if (!m_TreeCtrl.m_htSettings)
	//	m_TreeCtrl.m_htSettings = m_TreeCtrl.AddNode(_T("General Settings"), CNodeTree::ENodeType::NT_SETTINGS, NULL, GetDocument()->m_pWoormIni);

	return TRUE;
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSEngineTreeView, CRSReportTreeView)

CRSEngineTreeView::CRSEngineTreeView()
	:
	CRSReportTreeView(L"Engine", IDD_RS_EngineTreeView)
{
	m_TreeCtrl.EnableDrag(DRAGDROP_WOORM_ENGINE_VIEW);
}

// -----------------------------------------------------------------------------
void CRSEngineTreeView::OnBuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->SetEngineTreeView(this);

	FillTree();
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSEngineTreeView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pEnginePane->GetToolBar();
}

// -----------------------------------------------------------------------------
BOOL CRSEngineTreeView::FillTree()
{
	ASSERT_VALID(GetDocument());
	if (!GetDocument()->m_bAllowEditing)
		return FALSE;

	CWaitCursor wc;

	//if (!m_TreeCtrl.m_htProperties)
	//	m_TreeCtrl.m_htProperties = m_TreeCtrl.AddNode(_T("Properties"), CNodeTree::ENodeType::NT_PROPERTIES, NULL, GetDocument()->m_pDocProperties);
	//if (!m_TreeCtrl.m_htPageInfo)
	//	m_TreeCtrl.m_htPageInfo = m_TreeCtrl.AddNode(_T("Page info"), CNodeTree::ENodeType::NT_PAGE, NULL, &GetDocument()->m_PageInfo);

	//m_TreeCtrl.FillLayouts();
	//	m_TreeCtrl.Expand(m_TreeCtrl.m_htLayouts, TVE_EXPAND);
	//	//m_TreeCtrl.Expand(m_TreeCtrl.m_htLayoutDefault, TVE_EXPAND);

	//m_TreeCtrl.FillLinks();

	m_TreeCtrl.FillVariables(TRUE, FALSE, TRUE, FALSE, FALSE);
	m_TreeCtrl.FillRules();
	m_TreeCtrl.FillTupleRules();
	m_TreeCtrl.FillEvents();
	m_TreeCtrl.FillProcedures();
	m_TreeCtrl.FillQueries();
	m_TreeCtrl.FillDialogs();

	/*if (!m_TreeCtrl.m_htSettings)
		m_TreeCtrl.m_htSettings = m_TreeCtrl.AddNode(_T("General Settings"), CNodeTree::ENodeType::NT_SETTINGS, NULL, GetDocument()->m_pWoormIni);*/

	return TRUE;
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBoxDockPane, CRSDockPane)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRSToolBoxDockPane, CRSDockPane)

	ON_BN_CLICKED(ID_RS_CHECK_TABLE_FROM_DB, OnAddTable)

END_MESSAGE_MAP()

CRSToolBoxDockPane::CRSToolBoxDockPane()
	:
	CRSDockPane(RUNTIME_CLASS(CRSToolBoxObjectsView))
{
	this->m_sNsHelp = RS_HELP_PANEL_TOOLBOX_OBJECT;
}

//------------------------------------------------------------------------------
void CRSToolBoxDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_CHECK_TABLE_FROM_DB, RS_DOCK_NS, TBIcon(szIconTable, TOOLBAR), _TB("New Table"), _TB("\nDrag columns to create a new Table"));
	m_pToolBar->PressButton(ID_RS_CHECK_TABLE_FROM_DB, TRUE, FALSE);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDockPane::OnAddTable()
{
	CRSToolBoxObjectsView* toolBoxView = (CRSToolBoxObjectsView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxObjectsView));
	if (!toolBoxView)
		return;
	toolBoxView->OnAddTable();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBoxObjectsView, CRSDockedView)

BEGIN_MESSAGE_MAP(CRSToolBoxObjectsView, CRSDockedView)

	ON_NOTIFY(NM_DBLCLK, IDC_RS_Tree, OnDblclkTree)

	ON_UPDATE_COMMAND_UI(ID_RS_CHECK_TABLE_FROM_DB, OnUpdateAddTable)

END_MESSAGE_MAP()

//----------------------------------------------------------------------
CRSToolBoxObjectsView::CRSToolBoxObjectsView()
	:
	CRSDockedView(L"ToolBox", IDD_RS_ToolBoxObjectsView)
{
	m_TreeCtrl.EnableDrag(DRAGDROP_WOORM_OBJECTS_VIEW);
}

CRSToolBoxObjectsView::~CRSToolBoxObjectsView()
{
	GetDocument()->GetWoormFrame()->m_pToolBoxTreeView = NULL;
}

//-----------------------------------------------------------------------------
void CRSToolBoxObjectsView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	__super::SetScrollSizes(MM_TEXT, CSize(0, 0));
}

// -----------------------------------------------------------------------------
CWoormDocMng* CRSToolBoxObjectsView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// -----------------------------------------------------------------------------
void CRSToolBoxObjectsView::BuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->m_pToolBoxTreeView = this;

	ModifyStyle(WS_BORDER, WS_CLIPCHILDREN);

	m_TreeCtrl.SubclassDlgItem(IDC_RS_Tree, this);
	m_TreeCtrl.Attach(GetDocument());
	m_TreeCtrl.InitializeImageList();

	m_TreeCtrl.ModifyStyle(TVS_HASLINES | TVS_HASBUTTONS, 0/*TVS_FULLROWSELECT*/);//TODO rimangono dei pixel bianchi dopo la selezione

	this->FillTree();
}

// -----------------------------------------------------------------------------
BOOL CRSToolBoxObjectsView::FillTree()
{
	m_TreeCtrl.FillToolBox();
	return TRUE;
}

// ---------------------------------------------------------------------------- -
void CRSToolBoxObjectsView::OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;	//block expand/collapse effectL"", 
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSToolBoxObjectsView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pToolBoxPane->GetToolBar();
}

//------------------------------------------------------------------------------
void CRSToolBoxObjectsView::OnAddTable()
{
	m_bAddTable = !m_bAddTable;

	GetToolBar()->CheckButton(ID_RS_CHECK_TABLE_FROM_DB, m_bAddTable);
}

void CRSToolBoxObjectsView::OnUpdateAddTable(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bAddTable);

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

	if (pNode->m_NodeType == CNodeTree::ENodeType::NT_TOOLBOX_OBJECT)
	{
		int id_obj = (int)pNode->m_pItemData;
		bEnable = id_obj == ID_RS_ADD_FIELD_NEW_FUNCEXPR;
	}
	pCmdUI->Enable(bEnable);
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBoxDBDockPane, CRSDockPane)

BEGIN_MESSAGE_MAP(CRSToolBoxDBDockPane, CRSDockPane)

	ON_BN_CLICKED(ID_RS_MORE, OnMore)
	ON_BN_CLICKED(ID_RS_FILTER, OnFilter)

	ON_BN_CLICKED(ID_RS_CHECK_TABLE_FROM_DB, OnCheckAddTable)
	//ON_BN_CLICKED(ID_RS_CHECK_HIDDEN_VAR_FROM_DB, OnCheckAddHidden)

	ON_BN_CLICKED(ID_RS_ADD_HIDDEN_VAR_FROM_DB, OnAddHiddenVar)

	ON_BN_CLICKED(ID_RS_COLLAPSEALLTREE, OnCollapseAll)
	ON_BN_CLICKED(ID_RS_EXPANDALLTREE, OnExpand)

END_MESSAGE_MAP()

CRSToolBoxDBDockPane::CRSToolBoxDBDockPane()
	:
	CRSDockPane(RUNTIME_CLASS(CRSToolBoxDBView))
{
	this->m_sNsHelp = RS_HELP_PANEL_TOOLBOX_DB;
}

//------------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnAddToolbarButtons()
{
	m_pToolBar->AddButton(ID_RS_CHECK_TABLE_FROM_DB, RS_DOCK_NS, TBIcon(szIconTable, TOOLBAR), _TB("New Table"), _TB("\nDrag columns to create a new Table"));
	m_pToolBar->PressButton(ID_RS_CHECK_TABLE_FROM_DB, TRUE, FALSE);

	m_pToolBar->AddButton(ID_RS_MORE, RS_DOCK_NS, TBIcon(szIconMore, TOOLBAR), _TB("Group"), _TB("\nDB Tables/Viewes grouped by module"));
	m_pToolBar->PressButton(ID_RS_MORE, TRUE, FALSE);

	m_pToolBar->AddButton(ID_RS_FILTER, RS_DOCK_NS, TBIcon(szIconFilterRS, TOOLBAR), _TB("Filter"), _TB("\nShow only matching table name"));

	m_pToolBar->AddButtonToRight(ID_RS_ADD_HIDDEN_VAR_FROM_DB, RS_DOCK_NS, TBIcon(szIconAdd, TOOLBAR), _TB("Add Hidden"), _TB("\nSelect columns to create new hidden variables"));
	m_pToolBar->AddButtonToRight(ID_RS_COLLAPSEALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeCollapseAll, TOOLBAR), _TB("Collapse All"), _TB("\nCollapse all properties"));
	m_pToolBar->AddButtonToRight(ID_RS_EXPANDALLTREE, RS_DOCK_NS, TBIcon(szIconBeTreeExpand, TOOLBAR), _TB("Expand current"), _TB("\nExpand current property"));
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnMore()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnMore();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnFilter()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnFilter();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnCheckAddTable()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnCheckAddTable();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnCheckAddHidden()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnCheckAddHidden();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnAddHiddenVar()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnAddHiddenVar();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnCollapseAll()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnCollapseAll();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBDockPane::OnExpand()
{
	CRSToolBoxDBView* toolBoxView = (CRSToolBoxDBView*)GetDerivedFormWnd(RUNTIME_CLASS(CRSToolBoxDBView));
	if (!toolBoxView)
		return;
	toolBoxView->OnExpand();
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBoxDBView, CRSDockedView)

BEGIN_MESSAGE_MAP(CRSToolBoxDBView, CRSDockedView)

	ON_COMMAND(ID_VK_UP, OnVKUp)
	ON_COMMAND(ID_VK_DOWN, OnVKDown)
	ON_COMMAND(ID_VK_LEFT, OnVKLeft)
	ON_COMMAND(ID_VK_RIGHT, OnVKRight)

	ON_COMMAND(IDC_RS_Tree_Finder, OnFindTree)

	ON_UPDATE_COMMAND_UI(ID_RS_MORE, OnUpdateMore)
	ON_UPDATE_COMMAND_UI(ID_RS_FILTER, OnUpdateFilter)

	ON_UPDATE_COMMAND_UI(ID_RS_CHECK_TABLE_FROM_DB, OnUpdateCheckAddTable)
	//ON_UPDATE_COMMAND_UI(ID_RS_CHECK_HIDDEN_VAR_FROM_DB,	OnUpdateCheckAddHidden)

	ON_UPDATE_COMMAND_UI(ID_RS_ADD_HIDDEN_VAR_FROM_DB, OnUpdateAddHiddenVar)

	ON_UPDATE_COMMAND_UI(ID_RS_COLLAPSEALLTREE, OnUpdateCollapseAll)
	ON_UPDATE_COMMAND_UI(ID_RS_EXPANDALLTREE, OnUpdateExpand)

	ON_NOTIFY(TVN_SELCHANGED, IDC_RS_Tree, OnSelchangedTree)
	ON_NOTIFY(NM_KILLFOCUS, IDC_RS_Tree, OnKillFocus)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CRSToolBoxDBView::CRSToolBoxDBView()
	:
	CRSDockedView(L"ToolBox", IDD_RS_ToolBoxDBView)
{
	m_TreeCtrl.EnableDrag(DRAGDROP_WOORM_DB_VIEW);
}

CRSToolBoxDBView::~CRSToolBoxDBView()
{
	GetDocument()->GetWoormFrame()->m_pToolBoxDBView = NULL;
}
//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnVKUp()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM prevItem = m_TreeCtrl.GetPrevVisibleItem(currItem);
		if (prevItem)
			m_TreeCtrl.SelectItem(prevItem);
	}
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnVKDown()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM nextItem = m_TreeCtrl.GetNextVisibleItem(currItem);
		if (nextItem)
			m_TreeCtrl.SelectItem(nextItem);
	}
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnVKLeft()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM parentItem = m_TreeCtrl.GetParentItem(currItem);
		if (m_TreeCtrl.GetItemState(currItem, TVIS_EXPANDED) & TVIS_EXPANDED)
			m_TreeCtrl.Expand(currItem, TVE_COLLAPSE);
		else if (parentItem)
			m_TreeCtrl.SelectItem(parentItem);
	}
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnVKRight()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		HTREEITEM childItem = m_TreeCtrl.GetChildItem(currItem);
		if (!(m_TreeCtrl.GetItemState(currItem, TVIS_EXPANDED) & TVIS_EXPANDED))
			m_TreeCtrl.Expand(currItem, TVE_EXPAND);
		else if (childItem)
			m_TreeCtrl.SelectItem(childItem);
	}
}

// -----------------------------------------------------------------------------
CWoormDocMng* CRSToolBoxDBView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSToolBoxDBView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pToolBoxDBPane->GetToolBar();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	SetScrollSizes(MM_TEXT, CSize(0, 0));
}

// -----------------------------------------------------------------------------
void CRSToolBoxDBView::BuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->m_pToolBoxDBView = this;

	ModifyStyle(WS_BORDER, WS_CLIPCHILDREN);

	m_TreeCtrl.SubclassDlgItem(IDC_RS_Tree, this);
	m_TreeCtrl.Attach(GetDocument());
	m_TreeCtrl.InitializeImageList();
	//m_TreeCtrl.ModifyStyle(TVS_HASLINES | TVS_HASBUTTONS, TVS_FULLROWSELECT);

	CWnd* pWnd = GetDlgItem(IDC_RS_Tree_Finder);
	if (pWnd)
		pWnd->Detach();
	m_edtFinder.SubclassEdit(IDC_RS_Tree_Finder, this);
	m_edtFinder.EnableFindBrowseButton(TRUE, L"", TRUE);
	m_edtFinder.SetPrompt(_TB("Search node..."));

	this->FillTree();
}

// -----------------------------------------------------------------------------
BOOL CRSToolBoxDBView::FillTree()
{
	m_TreeCtrl.FillAllTables();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSToolBoxDBView::PreTranslateMessage(MSG* pMsg)
{

	BOOL shiftPressed = GetKeyState(VK_SHIFT) & 0x8000;

	if (pMsg->wParam == VK_RETURN && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus())
	{
		OnFilter();
		return TRUE;
	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int chars = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		if (chars < str.GetLength())
			m_edtFinder.SetSel(chars + 1, chars + 1);
		else
			m_edtFinder.SetSel(str.GetLength(), str.GetLength());
		return TRUE;
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int chars = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		if (chars != 0)
			m_edtFinder.SetSel(chars - 1, chars - 1);
		else
			m_edtFinder.SetSel(0, 0);
		return TRUE;
	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int cPos = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		int pos = 0;
		if (cPos == charsEnd || cPos == -1)
		{
			pos = charsEnd < str.GetLength() ? charsEnd + 1 : str.GetLength();
			m_edtFinder.SetSel(charsStart, pos);
		}

		else
		{
			pos = charsStart < charsEnd ? charsStart + 1 : charsEnd;
			m_edtFinder.SetSel(pos, charsEnd);
		}

		m_edtFinder.SetCaretPos(m_edtFinder.PosFromChar(pos));
		return TRUE;
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int cPos = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		int pos = 0;

		if (cPos == charsStart)
		{
			pos = charsStart != 0 ? charsStart - 1 : 0;
			m_edtFinder.SetSel(pos, charsEnd);
		}
		else
		{
			pos = charsEnd > charsStart ? charsEnd - 1 : charsStart;
			m_edtFinder.SetSel(charsStart, pos);
		}

		m_edtFinder.SetCaretPos(m_edtFinder.PosFromChar(pos));
		return TRUE;
	}

	if (pMsg->wParam == VK_HOME && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{

		CString str;
		m_edtFinder.GetWindowText(str);
		m_edtFinder.SetSel(0, 0);
		return TRUE;
	}

	if (pMsg->wParam == VK_END && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{

		CString str;
		m_edtFinder.GetWindowText(str);
		m_edtFinder.SetSel(str.GetLength(), str.GetLength());
		return TRUE;

	}

	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus())
	{

		CString str;
		m_edtFinder.GetWindowText(str);

		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		if (charsStart < charsEnd)
		{
			m_edtFinder.Clear();
			return TRUE;
		}

		CString subStrStart = str.Mid(0, charsStart);
		CString subStrEnd;
		if (str.GetLength() == charsStart)
			subStrEnd = L"";
		else
			subStrEnd = str.Mid(charsStart + 1);
		CString finaStr = subStrStart + subStrEnd;

		m_edtFinder.SetWindowTextW((LPCTSTR)finaStr);
		m_edtFinder.SetSel(charsStart, charsStart, FALSE);

		return TRUE;

	}

	if (pMsg->wParam == VK_RIGHT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int cPos = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		int pos = 0;
		if (cPos == charsEnd || cPos == -1)
		{
			pos = charsEnd < str.GetLength() ? charsEnd + 1 : str.GetLength();
			m_edtFinder.SetSel(charsStart, pos);
		}

		else
		{
			pos = charsStart < charsEnd ? charsStart + 1 : charsEnd;
			m_edtFinder.SetSel(pos, charsEnd);
		}

		m_edtFinder.SetCaretPos(m_edtFinder.PosFromChar(pos));
		return TRUE;
	}

	if (pMsg->wParam == VK_LEFT && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && shiftPressed)
	{
		CString str;
		m_edtFinder.GetWindowText(str);
		int cPos = m_edtFinder.CharFromPos(m_edtFinder.GetCaretPos());
		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		int pos = 0;

		if (cPos == charsStart)
		{
			pos = charsStart != 0 ? charsStart - 1 : 0;
			m_edtFinder.SetSel(pos, charsEnd);
		}
		else
		{
			pos = charsEnd > charsStart ? charsEnd - 1 : charsStart;
			m_edtFinder.SetSel(charsStart, pos);
		}

		m_edtFinder.SetCaretPos(m_edtFinder.PosFromChar(pos));
		return TRUE;
	}

	if (pMsg->wParam == VK_HOME && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{

		CString str;
		m_edtFinder.GetWindowText(str);
		m_edtFinder.SetSel(0, 0);
		return TRUE;
	}

	if (pMsg->wParam == VK_END && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus() && !shiftPressed)
	{

		CString str;
		m_edtFinder.GetWindowText(str);
		m_edtFinder.SetSel(str.GetLength(), str.GetLength());
		return TRUE;

	}

	if (pMsg->wParam == VK_DELETE && pMsg->message == WM_KEYDOWN && m_edtFinder.HasFocus())
	{

		CString str;
		m_edtFinder.GetWindowText(str);

		int charsStart = 0, charsEnd = 0;
		m_edtFinder.GetSel(charsStart, charsEnd);
		if (charsStart < charsEnd)
		{
			m_edtFinder.Clear();
			return TRUE;
		}

		CString subStrStart = str.Mid(0, charsStart);
		CString subStrEnd;
		if (str.GetLength() == charsStart)
			subStrEnd = L"";
		else
			subStrEnd = str.Mid(charsStart + 1);
		CString finaStr = subStrStart + subStrEnd;

		m_edtFinder.SetWindowTextW((LPCTSTR)finaStr);
		m_edtFinder.SetSel(charsStart, charsStart, FALSE);

		return TRUE;
	}

	return __super::PreTranslateMessage(pMsg);
}

// -----------------------------------------------------------------------------
void CRSToolBoxDBView::OnFindTree()
{
	CString sMatchText;
	this->m_edtFinder.GetWindowText(sMatchText);
	sMatchText.Trim();
	if (sMatchText.IsEmpty())
		return;

	m_TreeCtrl.SelectRSTreeItemByMatchingText(sMatchText);
}

//------------------------------------------------------------------------------
void CRSToolBoxDBView::OnCheckAddTable()
{
	m_bAddTable = !m_bAddTable;

	if (m_bAddTable)
	{
		m_bAddHidden = FALSE;
	}

	GetToolBar()->CheckButton(ID_RS_CHECK_TABLE_FROM_DB, m_bAddTable);
	//GetToolBar()->CheckButton(ID_RS_CHECK_HIDDEN_VAR_FROM_DB, m_bAddHidden);
}

//------------------------------------------------------------------------------
void CRSToolBoxDBView::OnCheckAddHidden()
{
	m_bAddHidden = !m_bAddHidden;

	if (m_bAddHidden)
	{
		m_bAddTable = FALSE;
	}

	GetToolBar()->CheckButton(ID_RS_CHECK_TABLE_FROM_DB, m_bAddTable);
	//GetToolBar()->CheckButton(ID_RS_CHECK_HIDDEN_VAR_FROM_DB, m_bAddHidden);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateCheckAddHidden(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bAddHidden);
}

void CRSToolBoxDBView::OnUpdateCheckAddTable(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(m_bAddTable);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateAddHiddenVar(CCmdUI* pCmdUI)
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

		bEnable = TRUE;
		break;
	}

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnAddHiddenVar()
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
		TblRuleData* pTblRule = GetDocument()->AddDBColumns_GetOrCreateTblRule(pNode);
		if (!pTblRule)
		{
			return;
		}
		if (GetDocument()->m_pNodesSelection && GetDocument()->m_pNodesSelection->GetCount() > 0)
		{
			GetDocument()->AddDBColumns_FromToolBar(GetDocument()->m_pNodesSelection, TRUE, pTblRule, 0);
		}
		else
			GetDocument()->AddDBColumns_FromToolBar(pNode, TRUE, pTblRule, 0);
		return;
	}
	}
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnMore()
{
	m_TreeCtrl.m_bShowAllTables = !m_TreeCtrl.m_bShowAllTables;

	m_TreeCtrl.RemoveTreeChilds(m_TreeCtrl.m_htTables);

	if (m_TreeCtrl.m_bShowAllTables)
		m_TreeCtrl.FillAllTables();
	else
	{
		m_TreeCtrl.m_bShowFilteredTables = FALSE;
		m_TreeCtrl.m_FilterTablePattern.Empty();
		//GetToolBar()->CheckButton(ID_RS_FILTER, m_TreeCtrl.m_bShowFilteredTables);

		m_TreeCtrl.FillTables();
	}

	m_TreeCtrl.Expand(m_TreeCtrl.m_htTables, TVE_EXPAND);
}

////-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnFilter()
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
	else if (m_TreeCtrl.m_bShowFilteredTables && filter.Compare(m_TreeCtrl.m_FilterTablePattern))
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
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateFilter(CCmdUI* pCmdUI)
{
	CString filter;
	m_edtFinder.GetWindowText(filter); filter.Trim();

	//se deve essere checckato
	if (m_TreeCtrl.m_bShowFilteredTables)
	{
		//lo faccio diventare una checkbox selezionata
		if (GetDocument() && GetDocument()->GetWoormFrame())
		{
			GetDocument()->GetWoormFrame()->m_pToolBoxDBPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_CHECKBOX);
			GetDocument()->GetWoormFrame()->m_pToolBoxDBPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_CHECKED);
			pCmdUI->Enable(TRUE);
		}
	}
	//altrimenti
	else
	{
		//lo faccio tornare bottone
		if (GetDocument() && GetDocument()->GetWoormFrame())
			GetDocument()->GetWoormFrame()->m_pToolBoxDBPane->m_pToolBar->SetButtonStyleByIdc(ID_RS_FILTER, TBBS_BUTTON);
		pCmdUI->Enable(m_TreeCtrl.m_bShowFilteredTables || !filter.IsEmpty());
	}
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;

	if (m_TreeCtrl.m_bDeleting)
		return;

	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;

	HTREEITEM hCurrentItem = pNMTreeView->itemNew.hItem;
	if (!hCurrentItem)
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();
		return;
	}

	CNodeTree* pNode = (CNodeTree*)m_TreeCtrl.GetItemData(hCurrentItem);
	if (!pNode)
	{
		GetDocument()->GetWoormFrame()->m_pObjectPropertyView->ClearPropertyGrid();
		return;
	}
	ASSERT_VALID(pNode);
	ASSERT_KINDOF(CNodeTree, pNode);

	ASSERT_VALID(GetDocument());
	ASSERT_VALID(GetDocument()->GetWoormFrame());

	switch (pNode->m_NodeType)
	{
	case  CNodeTree::ENodeType::NT_LIST_DBTABLE:
	{
		SqlCatalogEntry* pCatEntry = dynamic_cast<SqlCatalogEntry*>(pNode->m_pItemData);
		CString sNameTrad = AfxLoadDatabaseString(pCatEntry->m_strTableName, pCatEntry->m_strTableName);
		GetDocument()->SetMessageInStatusBar(sNameTrad, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
		break;
	}
	case  CNodeTree::ENodeType::NT_LIST_COLUMN_INFO:
	{
		const SqlColumnInfoObject* pCol = (SqlColumnInfoObject*)(pNode->m_pItemData);
		CString sNameTrad = pCol->GetColumnTitle();
		CString	strBuffer(pCol->GetDataObjType().ToString());
		strBuffer += cwsprintf(_T("( %d"), pCol->GetColumnLength());
		if (pCol->GetColumnDecimal() > 0)
			strBuffer += cwsprintf(_T(", %d"), pCol->GetColumnDecimal());
		strBuffer += _T(" )");

		GetDocument()->SetMessageInStatusBar(sNameTrad + L", " + strBuffer, CWoormDocMng::MSGSBType::MSG_SB_NORMAL);
		break;
	}
	default:
		GetDocument()->ClearMessageInStatusBar();
		break;
	}

}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnKillFocus(NMHDR* pNMHDR, LRESULT* pResult)
{
	GetDocument()->ClearMessageInStatusBar();
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateMore(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck(!this->m_TreeCtrl.m_bShowAllTables);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateCollapseAll(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnUpdateExpand(CCmdUI* pCmdUI)
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();

	pCmdUI->Enable(currItem != NULL);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnCollapseAll()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		NM_TREEVIEW NMV;
		NMV.action = TVE_COLLAPSE;
		NMV.itemNew.hItem = currItem;
		LRESULT* pResult = NULL;

		m_TreeCtrl.Expand(currItem, TVE_COLLAPSE | TVE_COLLAPSERESET);
		m_TreeCtrl.OnItemExpanding(&(NMV.hdr), pResult);
	}
	else
		m_TreeCtrl.ExpandAll(TVE_COLLAPSE);
}

//-----------------------------------------------------------------------------
void CRSToolBoxDBView::OnExpand()
{
	HTREEITEM currItem = m_TreeCtrl.GetSelectedItem();
	if (currItem)
	{
		NM_TREEVIEW NMV;
		NMV.action = TVE_EXPAND;
		NMV.itemNew.hItem = currItem;
		LRESULT* pResult = NULL;

		m_TreeCtrl.Expand(currItem, TVE_EXPAND | TVE_COLLAPSERESET);
		m_TreeCtrl.OnItemExpanding(&(NMV.hdr), pResult);
	}
	else
		m_TreeCtrl.ExpandAll(TVE_EXPAND);
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBarDockPane, CRSDockPane)

CRSToolBarDockPane::CRSToolBarDockPane()
	:
	CRSDockPane(RUNTIME_CLASS(CRSToolBarView))
{
	this->m_sNsHelp = RS_HELP_PANEL_TOOLBOX_BAR;
}

//=============================================================================
IMPLEMENT_DYNCREATE(CRSToolBarView, CRSDockedView)

BEGIN_MESSAGE_MAP(CRSToolBarView, CRSDockedView)

	ON_MESSAGE_VOID(WM_IDLEUPDATECMDUI, OnIdleUpdateCmdUI)

	ON_UPDATE_COMMAND_UI(ID_RS_REMOVE_LAYOUT_OBJECT, OnUpdateCut)

	ON_UPDATE_COMMAND_UI(ID_SNAP_TO_GRID, OnUpdateSnapToGrid)
	ON_UPDATE_COMMAND_UI(ID_TOGGLE_TRANSPARENT, OnUpdateToggleTransparent)

	ON_UPDATE_COMMAND_UI(ID_VK_LEFT, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_RIGHT, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_UP, OnUpdateVKMove)
	ON_UPDATE_COMMAND_UI(ID_VK_DOWN, OnUpdateVKMove)

	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_LEFT, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_RIGHT, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_UP, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_SHIFT_DOWN, OnUpdateVKSize)

	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_LEFT, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_RIGHT, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_UP, OnUpdateVKSize)
	ON_UPDATE_COMMAND_UI(ID_VK_CTRL_DOWN, OnUpdateVKSize)

	ON_UPDATE_COMMAND_UI(ID_COL_LMOVE, OnUpdateColMove)
	ON_UPDATE_COMMAND_UI(ID_COL_RMOVE, OnUpdateColMove)

	ON_UPDATE_COMMAND_UI(ID_ALIGN_HLEFT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_HRIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_HSPACE_EQUAL, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VTOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VBOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_VSPACE_EQUAL, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_LEFT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_RIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_TOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_STACK_BOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_H_LEFT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_H_RIGHT, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_V_TOP, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_ALIGN_CUT_V_BOTTOM, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_LAST_LARGE, OnUpdateAlignmentBar)
	ON_UPDATE_COMMAND_UI(ID_LAST_HIGH, OnUpdateAlignmentBar)

END_MESSAGE_MAP()

//----------------------------------------------------------------------
CRSToolBarView::CRSToolBarView()
	:
	CRSDockedView(L"ToolBar", IDD_RS_ToolBarView)
{
}

CRSToolBarView::~CRSToolBarView()
{
}

//-----------------------------------------------------------------------------
void CRSToolBarView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	//SetScrollSizes(MM_TEXT, CSize(0, 1024));
}

// -----------------------------------------------------------------------------
CWoormDocMng* CRSToolBarView::GetDocument()
{
	return dynamic_cast<CWoormDocMng*>(CView::GetDocument());
}

// -----------------------------------------------------------------------------
void CRSToolBarView::addbtn(CBCGPButton& btn, UINT id, CString nsIcon, CString tooltip)
{
	btn.SubclassDlgItem(id, this);
	btn.m_bVisualManagerStyle = TRUE;
	btn.SetFaceColor(AfxGetThemeManager()->GetBackgroundColor(), FALSE);
	//btn.m_clrHover = AfxGetThemeManager()->GetTabSelectorHoveringForeColor();
	btn.m_nFlatStyle = CBCGPButton::BUTTONSTYLE_NOBORDERS;

	HICON hIcon = TBLoadPng(nsIcon);
	btn.SetImage(hIcon, 1, NULL, hIcon);
	btn.SetTooltip(tooltip);
	btn.SizeToContent(FALSE);
}

// -----------------------------------------------------------------------------
void CRSToolBarView::BuildDataControlLinks()
{
	GetDocument()->GetWoormFrame()->m_pToolBarView = this;

	ModifyStyle(WS_BORDER, WS_CLIPCHILDREN);

	//MISCELLANEOUS
	addbtn(m_btToggleTrasparent, ID_TOGGLE_TRANSPARENT, TBIcon(szIconTransparent, TOOLBAR), _TB("Transparent"));
	addbtn(m_btRemoveLayoutObject, ID_RS_REMOVE_LAYOUT_OBJECT, TBIcon(szIconRemoveField, TOOLBAR), _TB("Remove layout object. The variable will become hidden."));

	//BORDERS
	addbtn(m_btBorderAll, ID_TOGGLE_BORDER_ALL, TBIcon(szIconBorderAll, TOOLBAR), _TB("Add all borders"));
	addbtn(m_btBorderTop, ID_TOGGLE_BORDER_UP, TBIcon(szIconBorderTop, TOOLBAR), _TB("Add top border"));
	addbtn(m_btBorderBottom, ID_TOGGLE_BORDER_DOWN, TBIcon(szIconBorderBottom, TOOLBAR), _TB("Add bottom border"));
	addbtn(m_btBorderLeft, ID_TOGGLE_BORDER_LEFT, TBIcon(szIconBorderLeft, TOOLBAR), _TB("Add left border"));
	addbtn(m_btBorderRight, ID_TOGGLE_BORDER_RIGHT, TBIcon(szIconBorderRight, TOOLBAR), _TB("Add right border"));
	//SIZE
	addbtn(m_btEnlargeTop, ID_VK_SHIFT_UP, TBIcon(szIconEnlargeTop, TOOLBAR), _TB("Enlarge object toward top"));
	addbtn(m_btNarrowTop, ID_VK_SHIFT_DOWN, TBIcon(szIconNarrowTop, TOOLBAR), _TB("Narrow object from top"));
	addbtn(m_btNarrowRight, ID_VK_SHIFT_LEFT, TBIcon(szIconNarrowRight, TOOLBAR), _TB("Narrow object from right"));
	addbtn(m_btEnlargeRight, ID_VK_SHIFT_RIGHT, TBIcon(szIconEnlargeRight, TOOLBAR), _TB("Enlarge object toward right"));

	addbtn(m_btNarrowBottom, ID_VK_CTRL_UP, TBIcon(szIconNarrowBottom, TOOLBAR), _TB("Narrow object from bottom"));
	addbtn(m_btEnlargeBottom, ID_VK_CTRL_DOWN, TBIcon(szIconEnlargeBottom, TOOLBAR), _TB("Enlarge object toward bottom"));
	addbtn(m_btEnlargeLeft, ID_VK_CTRL_LEFT, TBIcon(szIconEnlargeLeft, TOOLBAR), _TB("Enlarge object toward left"));
	addbtn(m_btNarrowLeft, ID_VK_CTRL_RIGHT, TBIcon(szIconNarrowLeft, TOOLBAR), _TB("Narrow object from left"));

	/*MOVE*/
	addbtn(m_btArrowUp, ID_VK_UP, TBIcon(szIconArrowUp, TOOLBAR), _TB("Move up"));
	addbtn(m_btArrowDown, ID_VK_DOWN, TBIcon(szIconArrowDown, TOOLBAR), _TB("Move down"));
	addbtn(m_btArrowLeft, ID_VK_LEFT, TBIcon(szIconArrowLeft, TOOLBAR), _TB("Move left"));
	addbtn(m_btArrowRight, ID_VK_RIGHT, TBIcon(szIconArrowRight, TOOLBAR), _TB("Move right"));

	addbtn(m_btMoveColLeft, ID_COL_LMOVE, TBIcon(szIconMoveColLeft, TOOLBAR), _TB("Move column to left"));
	addbtn(m_btMoveColRight, ID_COL_RMOVE, TBIcon(szIconMoveColRight, TOOLBAR), _TB("Move column to right"));

	addbtn(m_btMoveOnGrid, ID_SNAP_TO_GRID, TBIcon(szIconMoveOnGrid, TOOLBAR), _TB("Move on grid"));

	/*ALIGMENT*/
	addbtn(m_btAlignLeft, ID_ALIGN_HLEFT, TBIcon(szIconAlignLeft, TOOLBAR), _TB("Align left"));
	addbtn(m_btAlignRight, ID_ALIGN_HRIGHT, TBIcon(szIconAlignRight, TOOLBAR), _TB("Align right"));
	addbtn(m_btAlignTop, ID_ALIGN_VTOP, TBIcon(szIconAlignTop, TOOLBAR), _TB("Align top"));
	addbtn(m_btAlignBottom, ID_ALIGN_VBOTTOM, TBIcon(szIconAlignBottom, TOOLBAR), _TB("Align bottom"));

	addbtn(m_btEqualHorizontalSpacing, ID_ALIGN_HSPACE_EQUAL, TBIcon(szIconEqualHorizontalSpacing, TOOLBAR), _TB("Equal horizontal spacing"));
	addbtn(m_btEqualVerticalSpacing, ID_ALIGN_VSPACE_EQUAL, TBIcon(szIconEqualVerticalSpacing, TOOLBAR), _TB("Equal vertical spacing"));

	addbtn(m_btSameWidth, ID_LAST_LARGE, TBIcon(szIconSameWidth, TOOLBAR), _TB("Same width"));
	addbtn(m_btSameHeight, ID_LAST_HIGH, TBIcon(szIconSameHeight, TOOLBAR), _TB("Same height"));

	addbtn(m_btTileLeft, ID_ALIGN_STACK_LEFT, TBIcon(szIconTileLeft, TOOLBAR), _TB("Tile on the left"));
	addbtn(m_btTileRight, ID_ALIGN_STACK_RIGHT, TBIcon(szIconTileRight, TOOLBAR), _TB("Tile on the right"));
	addbtn(m_btTileTop, ID_ALIGN_STACK_TOP, TBIcon(szIconTileTop, TOOLBAR), _TB("Tile on top"));
	addbtn(m_btTileBottom, ID_ALIGN_STACK_BOTTOM, TBIcon(szIconTileBottom, TOOLBAR), _TB("Tile on bottom"));

	addbtn(m_btJustifyLeft, ID_ALIGN_CUT_H_LEFT, TBIcon(szIconJustifyLeft, TOOLBAR), _TB("Justify left"));
	addbtn(m_btJustifyRight, ID_ALIGN_CUT_H_RIGHT, TBIcon(szIconJustifyRight, TOOLBAR), _TB("Justify right"));
	addbtn(m_btJustifyTop, ID_ALIGN_CUT_V_TOP, TBIcon(szIconJustifyTop, TOOLBAR), _TB("Justify top"));
	addbtn(m_btJustifyBottom, ID_ALIGN_CUT_V_BOTTOM, TBIcon(szIconJustifyBottom, TOOLBAR), _TB("Justify bottom"));

}

//------------------------------------------------------------------------------
CTBToolBar*	 CRSToolBarView::GetToolBar()
{
	return GetDocument()->GetWoormFrame()->m_pToolBarPane->GetToolBar();
}

//----------------------------------------------------------------------
void CRSToolBarView::OnIdleUpdateCmdUI()
{
	UpdateDialogControls(this, FALSE);
}

//----------------------------------------------------------------------
void CRSToolBarView::OnUpdateVKSize(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateVKSize(pCmdUI);
}

void CRSToolBarView::OnUpdateVKMove(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateVKMove(pCmdUI);
}

void CRSToolBarView::OnUpdateSnapToGrid(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateSnapToGrid(pCmdUI);
}

void CRSToolBarView::OnUpdateToggleTransparent(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateToggleTransparent(pCmdUI);
}

void CRSToolBarView::OnUpdateAlignmentBar(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateAlignmentBar(pCmdUI);
}

void CRSToolBarView::OnUpdateColMove(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateColMove(pCmdUI);
}

void CRSToolBarView::OnUpdateCut(CCmdUI* pCmdUI)
{
	GetDocument()->OnUpdateCut(pCmdUI);
}

//------------------------------------------------------------------------------

///////////////////////////////////////////////////////////////////////////////

