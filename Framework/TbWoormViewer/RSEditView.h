#pragma once

//#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\TbTreeCtrl.h>

#include <TbGes\ExtDocView.h>
#include <TbGes\ExtDocFrame.h>
#include <TbGes\TBGridControl.h>

#include "RSEditorUI.h"

#include "CustomEditCtrl.h"
#include "RSEditorUI.hjson" //JSON AUTOMATIC UPDATE
//includere alla fine degli include del .H
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
class ExpRuleData;
class TblRuleData;
class CWoormDocMng;

class CRSEditorToolDockPane;
class CRSEditorDiagnosticDockPane;
class CRSGridDockPane;
class CRSParametersDockPane;
class CRSEditorPreviewDockPane;


class CRSEditView;
class CRSEditorToolView;
class CRSEditorDiagnosticView;
class CRSEditorParametersView;
class CRSEditorGridView;
class CRSEditorPreviewView;

class CUndoButton;
class CCustomEditCtrl;
class CRSEditViewTreeCtrl;
//===========================================================================

///////////////////////////////////////////////////////////////////////////////



//=============================================================================
class TB_EXPORT CRSEditorFrame : public CAbstractFrame
{
	friend class CRSEditView;
	friend class CRSEditorDiagnosticView;
	friend class CRSEditorGridView;
	friend class CRSEditorPreviewView;

	DECLARE_DYNCREATE(CRSEditorFrame)
public:
	CRSEditorToolDockPane*		m_pToolPane = NULL;
	CRSEditorDiagnosticDockPane* m_pDiagnosticPane = NULL;
	CRSGridDockPane*			m_pGridPane = NULL;
	CRSDockPane*				m_pParametersPane = NULL;
	CRSEditorPreviewDockPane*	m_pPreviewPane = NULL;

protected:
	CTBToolBar*					m_pMainToolBar = NULL;
	CBCGPCaptionBar				m_CaptionBar;

public:
	CRSEditorToolView*			m_pToolTreeView = NULL;
	CRSEditorDiagnosticView*	m_pDiagnosticView = NULL;
	CRSEditorGridView*			m_pGridView = NULL;
	CAbstractFormView*			m_pParametersView = NULL;	
	CRSEditorPreviewView*		m_pPreviewView = NULL;

	TDisposablePtr<CRSEditView>	m_pEditView;

	CBCGPToolbarComboBoxButton* GetFindCombo();
	CBCGPToolbarComboBoxButton* GetReplaceCombo();


public:
	CRSEditorFrame::CRSEditorFrame();
	virtual CRSEditorFrame::~CRSEditorFrame();
	virtual void	OnFrameCreated();
	void EnablePreviewPanel();

protected:
	virtual BOOL CreateAuxObjects(CCreateContext* pCreateContext);

	int OnCreate(LPCREATESTRUCT lpcs);

	BOOL CreateCaptionBar();
	void OnClickCaptionBarButton();
	LRESULT OnClickCaptionBarHyperlink(WPARAM, LPARAM);

	BOOL PreTranslateMessage(MSG* pMsg);
	
public:
	afx_msg void OnEditTogglebookmark();
	afx_msg void OnClearAllBookmarks();

	afx_msg void OnEditNextbookmark();
	afx_msg void OnEditPreviousbookmark();

	afx_msg void OnUpdateBookmarkBtns(CCmdUI* pCmdUI);

	afx_msg void OnReplace();
	afx_msg void OnReplaceAll();
	afx_msg void OnFindNext();
	afx_msg void OnFindPrev();
	afx_msg void OnCheckPressed();
	afx_msg void OnExecPressed();

	afx_msg void OnSave();
	afx_msg void OnClose();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================

class TB_EXPORT CRSEditorFrameFullText :public CRSEditorFrame
{
	DECLARE_DYNCREATE(CRSEditorFrameFullText)
public:
	CRSEditorFrameFullText() :CRSEditorFrame(){};
	virtual BOOL CreateAuxObjects(CCreateContext* pCreateContext){return __super::CreateAuxObjects(pCreateContext); }
	int OnCreate(LPCREATESTRUCT lpcs);// { return __super::OnCreate(lpcs); }

	DECLARE_MESSAGE_MAP()
};


//=============================================================================
class CRSBreakpointDockPaneDebug;
class CRSEditorBreakpointViewDebug;

class TB_EXPORT CRSEditorDebugFrame :public  CRSEditorFrame
{
	DECLARE_DYNCREATE(CRSEditorDebugFrame)
public:
	CRSBreakpointDockPaneDebug*		m_pBreakpointsPane = NULL;
	CRSEditorBreakpointViewDebug*	m_pBreakpointsView = NULL;

	CRSEditorDebugFrame() :CRSEditorFrame() {};

	virtual BOOL CreateAuxObjects(CCreateContext* pCreateContext);
	int OnCreate(LPCREATESTRUCT lpcs);
	
	afx_msg void OnRunDebug();
	afx_msg void OnStopDebug();
	afx_msg void OnStepOver();
	BOOL justCreated = TRUE;
	DECLARE_MESSAGE_MAP()
};

//===========================================================================

class TB_EXPORT CRSEditViewTreeCtrl : public CRSTreeCtrl
{
	friend class CRSReportTreeView;
	DECLARE_DYNAMIC(CRSEditViewTreeCtrl)

protected:
	// Drag & Drop OLE message
	afx_msg virtual void OnBeginDrag(NMHDR* pNMHDR, LRESULT* pResult);
public:
	CString GetNodeString(CNodeTree* pNode);

};


//===========================================================================
class TB_EXPORT CRSEditorToolView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorToolView)
public:
	CRSEditViewTreeCtrl m_TreeCtrl;
	Block*				m_pBlock = NULL;
	CRSEditView*		m_pEditView = NULL;

protected:
	CResizableStrEdit m_edtFinder;
	// --------------------- Drag & Drop ----------------------
	CPoint			m_PointDrop;
	COleDropTarget	m_DropTarget;
	// --------------------------------------------------------
	BOOL m_bJustInserted = FALSE;
public:
	CRSEditorToolView();
	CRSEditorToolView(const CString& sName, UINT id);
	virtual ~CRSEditorToolView();

	CWoormDocMng* GetDocument();
	CRSEditorFrame* GetFrame();
	virtual	CTBToolBar*	 GetToolBar();

	afx_msg void OnFindTree();

	BOOL FillTree(BOOL bViewMode, CRSEditView* editView);
	BOOL FillTreeGroupingRule(CRSEditView* editView);
	BOOL FillTreeForSql(TblRuleData*, CRSEditView* editView);
	BOOL FillTreeForTextRect(CRSEditView* editView );
	BOOL FillTreeForGroup(CRSEditView* editView);
	BOOL FillFullTextTree(CRSEditView* editView);

	virtual BOOL FillForDebug	(CRSEditView*) { return FALSE; }
	virtual BOOL FillBreakpoints(ActionObj*) { return FALSE; }

protected:
	virtual void OnInitialUpdate();
	virtual	void BuildDataControlLinks();

	virtual	BOOL PreTranslateMessage(MSG* pMsg);

	void OnDblclkTree		(NMHDR* pNMHDR, LRESULT* pResult);
	void OnSelchangedTree	(NMHDR* pNMHDR, LRESULT* pResult);

	//afx_msg void OnRefresh();
	//afx_msg void OnUpdateRefresh(CCmdUI* pCmdUI);

	afx_msg void OnUpdateEdit(CCmdUI* pCmdUI);
	afx_msg void OnUpdateMore(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFilter(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAdd(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDelete(CCmdUI* pCmdUI);

public:
	void OnEdit(BOOL textSelected=FALSE);
	afx_msg void OnMore();
	afx_msg void OnFilter();
	afx_msg void OnAdd();
	afx_msg void OnDelete();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSEditorToolDebugView : public CRSEditorToolView
{
	DECLARE_DYNCREATE(CRSEditorToolDebugView)
public:

	CRSEditorToolDebugView();

	CRSEditorDebugFrame* GetFrame();

	virtual BOOL FillForDebug(CRSEditView* editView);
	virtual BOOL FillBreakpoints(ActionObj*);

	void ToggleBreakpoint(int nCurrRow, BOOL bSet);

	void OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CTBGridControlResizable : public CTBGridControlObj//, public CCustomFont
{
	friend class CRSEditorGridView;
	friend class CRSEditorDiagnosticView;

	DECLARE_DYNAMIC(CTBGridControlResizable)

public:
	CTBGridControlResizable() :CTBGridControlObj(){};

	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);// { DoRecalcCtrlSize(); AdjustLayout(); return 0L; }

	DECLARE_MESSAGE_MAP()
};

//===========================================================================

class TB_EXPORT CRSResizableErrorsEdit : public CResizableStrEdit
{
	friend class CRSEditorDiagnosticView;
	friend class CRSEditorGridView;

	DECLARE_DYNCREATE(CRSResizableErrorsEdit)
public:
	CRSResizableErrorsEdit() :CResizableStrEdit(){};
	BOOL PreTranslateMessage (MSG* pMsg);
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);
	
	DECLARE_MESSAGE_MAP()
};

//===========================================================================

class TB_EXPORT CRSEditorDiagnosticView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorDiagnosticView)
public:
	//CBCGPEdit m_edtErrors;
	CRSResizableErrorsEdit m_edtErrors;
	
public:
	CRSEditorDiagnosticView::CRSEditorDiagnosticView();
	virtual CRSEditorDiagnosticView::~CRSEditorDiagnosticView();

	void SetText(const CString& sText, BOOL bAppend = TRUE);
protected:
	CWoormDocMng* GetDocument();
	CRSEditorFrame* GetFrame();
	BOOL PreTranslateMessage(MSG* pMsg);

	virtual void OnInitialUpdate();

	virtual	void BuildDataControlLinks();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSEditorGridView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorGridView)
public:
	//CBCGPEdit m_edtErrors;
	CTBGridControlResizable*  m_pGrdTable;

	CRSEditorGridView::CRSEditorGridView();
	virtual CRSEditorGridView::~CRSEditorGridView();

	CWoormDocMng* GetDocument();
	CRSEditorFrame* GetFrame();

protected:
	virtual	void BuildDataControlLinks();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSEditorPreviewView : public CAbstractFormView
{
	DECLARE_DYNCREATE(CRSEditorPreviewView)

public:

	CRSEditorPreviewView::CRSEditorPreviewView();
	virtual CRSEditorPreviewView::~CRSEditorPreviewView();

	CWoormDocMng* GetDocument();
	CRSEditorFrame* GetFrame();

	virtual void OnDraw(CDC* pDC);
	void OnPreview();

	BOOL PreTranslateMessage(MSG* pMsg);
	virtual void OnInitialUpdate();

	virtual	void BuildDataControlLinks();
};

//===========================================================================
class TB_EXPORT CustomParametersPropertyGrid : public CTBPropertyGrid
{
	DECLARE_DYNCREATE(CustomParametersPropertyGrid)
	CustomParametersPropertyGrid(const CString sName = _T("")) :CTBPropertyGrid(sName) {};
	
	BOOL PreTranslateMessage(MSG* pMessage);
	CBCGPProp* GetNextProperty(CBCGPProp* prop, BOOL cycleFinished = FALSE);
	CBCGPProp* GetPrevProperty(CBCGPProp* prop, BOOL justStarted =TRUE);

	DECLARE_MESSAGE_MAP()

};

//===========================================================================
class TB_EXPORT CRSEditorParametersView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorParametersView)
public:
	//CBCGPEdit m_edtErrors;
	CustomParametersPropertyGrid* m_pPropGridParams;

	CRSEditorParametersView::CRSEditorParametersView();
	virtual CRSEditorParametersView::~CRSEditorParametersView();
	CWoormDocMng* GetDocument();
	CRSEditorFrame* GetFrame();

public:
	virtual void OnInitialUpdate();

protected:
	virtual	void BuildDataControlLinks();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSEditorSymbolTableViewDebug : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorSymbolTableViewDebug)
public:
	CustomParametersPropertyGrid* m_pPropGridParams;

	CRSEditorSymbolTableViewDebug();
	virtual ~CRSEditorSymbolTableViewDebug();

	CWoormDocMng* GetDocument();
	CRSEditorDebugFrame* GetFrame();
	
protected:
	virtual	void BuildDataControlLinks();

	void CreateGrid();
	void LoadSymbolTable();

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSEditorBreakpointViewDebug : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSEditorBreakpointViewDebug)

	Array m_arGarbage;	//all'uscita deletera tutti i nodi (gli ItemData) del tree
protected:
	CRS_PropertyGrid*	m_pPropGrid = NULL;

public:
	CRSEditorBreakpointViewDebug();
	virtual ~CRSEditorBreakpointViewDebug();

	CWoormDocMng* GetDocument();
	CRSEditorDebugFrame* GetFrame();

	void LoadBreakpoints(ActionObj* pCurrent);

protected:
	virtual	void BuildDataControlLinks();

	void CreateGrid();

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSEditorToolDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSEditorToolDockPane);
public:
	CRSEditorToolDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditorToolView)) { }
	CRSEditorToolDockPane(CRuntimeClass* rc) : CRSDockPane(rc) { }

	virtual void OnAddToolbarButtons();

	afx_msg void OnEdit();
	afx_msg void OnMore();
	afx_msg void OnFilter();
	afx_msg void OnAdd();
	afx_msg void OnDelete();

	DECLARE_MESSAGE_MAP()
};

class TB_EXPORT CRSEditorToolDockDebugPane : public CRSEditorToolDockPane
{
	DECLARE_DYNCREATE(CRSEditorToolDockDebugPane);
public:
	CRSEditorToolDockDebugPane() : CRSEditorToolDockPane(RUNTIME_CLASS(CRSEditorToolDebugView)) { }
};

//=============================================================================
class TB_EXPORT CRSEditorDiagnosticDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSEditorDiagnosticDockPane);
public:
	CRSEditorDiagnosticDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditorDiagnosticView)) { }
};

//=============================================================================
class TB_EXPORT CRSEditorPreviewDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSEditorPreviewDockPane);
public:
	CRSEditorPreviewDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditorPreviewView)) { }
	CRSEditorPreviewDockPane(CRuntimeClass* rc) : CRSDockPane(rc) { }

	virtual void OnAddToolbarButtons();

	afx_msg void OnPreview();
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSGridDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSGridDockPane);
public:
	CRSGridDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditorGridView)) { }
};

//=============================================================================
class TB_EXPORT CRSParametersDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSParametersDockPane);
public:	
	
	CRSParametersDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditorParametersView)) {}
};

//=============================================================================
class TB_EXPORT CRSSymbolTableDockPaneDebug : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSSymbolTableDockPaneDebug);
public:
	CRSSymbolTableDockPaneDebug() : CRSDockPane(RUNTIME_CLASS(CRSEditorSymbolTableViewDebug)) {}
};

//=============================================================================
class TB_EXPORT CRSBreakpointDockPaneDebug : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSBreakpointDockPaneDebug);
public:
	CRSBreakpointDockPaneDebug() : CRSDockPane(RUNTIME_CLASS(CRSEditorBreakpointViewDebug)) {}
};

//===========================================================================
class TB_EXPORT CRSEditViewParameters 
{
public:
	enum EditorMode { EM_WRONG, EM_NODE_TREE, EM_BLOCK, EM_EXPR, EM_TEXT, EM_RULE_EXPR, EM_CALC_COLUMN, EM_FULL_REPORT, EM_DEBUG_ACTIONS, EM_FUNC_EXPR };

	EditorMode	m_eType = EM_WRONG;
	BOOL m_bAllowEmpty = TRUE;

	CNodeTree* m_pNode = NULL;
	CString		m_strReportPath;
	CString* m_pText = NULL;

	SymTable* m_pSymTable = NULL;

	Block** m_ppBlock = NULL;
	BOOL m_bRaiseEvents = TRUE;

	Expression** m_ppExpr = NULL;
	EventFunction** m_ppEventFunc = NULL;
	WoormField* m_pWoormField = NULL;
	DataType m_eReturnType = DataType::Null;
	BOOL m_bViewMode = FALSE;;
	
	ExpRuleData* m_pRuleExpr = NULL;
	TblRuleData* m_pTableRule = NULL;

	BOOL m_bOwnObject = FALSE;;

	ActionObj*	m_pCurrActionObj = NULL;

public:
	CRSEditViewParameters()
	{}

	void Clear()
	{
		m_pNode = NULL;
		m_ppExpr = NULL;
		m_ppEventFunc = NULL;
		m_ppBlock = NULL;
		m_eType = EM_WRONG;
		m_eReturnType = DataType::Null;
		m_pText = NULL;
		m_pSymTable = NULL;
		m_bAllowEmpty = TRUE;
		m_bRaiseEvents = TRUE;
		m_bViewMode = FALSE;
		m_pRuleExpr = NULL;
		m_pTableRule = NULL;
		m_bOwnObject = FALSE;
		m_strReportPath.Empty();
		m_pCurrActionObj = NULL;
	}

	void SetNodeTree(CNodeTree* pNode)
	{
		Clear();
		
		m_eType = EM_NODE_TREE;
		m_pNode = pNode;
	}

	void SetPtrText(CString* pText)
	{
		Clear();

		m_eType = EM_TEXT;
		m_pText = pText;
		m_eReturnType = DataType::String;
	}

	void SetFullReport(CString strReportPath)
	{
		Clear();

		m_eType = EM_FULL_REPORT;
		m_strReportPath = strReportPath;
	}

	void SetExpr(SymTable* pSymTable, Expression** ppExpr, DataType eReturnType, BOOL bViewMode)
	{
		Clear();

		m_eType = EM_EXPR;

		m_pSymTable = pSymTable;
		m_ppExpr = ppExpr;
		m_eReturnType = eReturnType;
		m_bViewMode = bViewMode;
	}

	void SetExpr(WoormField* woormField, BOOL bViewMode)
	{
		Clear();

		m_eType = EM_FUNC_EXPR;

		m_pWoormField = woormField;
		m_pSymTable = woormField->GetSymTable();
		m_ppEventFunc = &woormField->GetEventFunction();
		m_eReturnType = woormField->GetDataType();
		m_bViewMode = bViewMode;
	}

	void SetBlock(SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents)
	{
		Clear();

		m_eType = EM_BLOCK;

		m_pSymTable = pSymTable;
		m_ppBlock = ppBlock;
		m_bRaiseEvents = bRaiseEvents;
	}

	void SetExprRule(SymTable* pSymTable, ExpRuleData* pRuleExpr)
	{
		Clear();

		m_eType = EM_RULE_EXPR;

		m_pSymTable = pSymTable;
		m_pRuleExpr = pRuleExpr;
	}

	void SetTableRuleForCalcColumn(SymTable* pSymTable, TblRuleData* pTableRule)
	{
		Clear();

		m_eType = EM_CALC_COLUMN;

		m_pSymTable = pSymTable;
		m_pTableRule = pTableRule;
	}

	void SetDebugAction(ActionObj*	pCurrActionObj)
	{
		Clear();

		m_eType = EM_DEBUG_ACTIONS;
		m_pCurrActionObj = pCurrActionObj;
	}

};

//===========================================================================

class TB_EXPORT CRSEditView : public CBCGPEditView, public CParsedForm, public IDisposingSourceImpl
{
	friend class CRSEditorFrame;
	DECLARE_DYNCREATE(CRSEditView)
public:
	CRSEditViewParameters	m_Context;
	BOOL*					m_pbSaved;
	BOOL*					m_pbSavedWithoutErrors;
	TextRect*				m_pEditTextRect;
	BOOL					m_bStringPreviewEnabled = FALSE;

	BOOL					m_bBusy = TRUE;

protected:
	CFont				m_Font;
	CString				m_strCaretPosFmt;
	CImageList			m_imgList;

	BOOL				m_bXMLSettings;
	void				ResetFindCombo();
	void				ResetReplaceCombo();

	//Constructor/destructor
public:

	CRSEditView();
	virtual ~CRSEditView();

	CWoormDocMng* GetDocument();

	CRSEditorFrame* GetEditorFrame(BOOL bMustExists);
	
//Operators
public:

	CCustomEditCtrl* GetEditCtrl() const;

	void OnChangeVisualStyle();
	void DoEvent(BOOL bWaitBusy = FALSE);

	void SetText(CString sText);
	CString GetText();

	//Overrides
	virtual void OnInitialUpdate();

	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

	virtual CBCGPEditCtrl* CreateEdit();
	virtual void OnTextNotFound(LPCTSTR lpszFind);

	// Implementation
public:
	virtual void AttachXMLSettings(const CString& strXMLFileName);
	virtual void SetLanguage(const CString& language, BOOL viewMode);

	//void DoUndo(CUndoButton* pUndoBtn);

	virtual void GetUndoActions(CStringList& lstActions) const;
	virtual void GetRedoActions(CStringList& lstActions) const;

	BOOL PreTranslateMessage(MSG* pMsg);
	
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:
	virtual BOOL UATToString(DWORD dwUAT, CString& strAction) const;

	// Generated message map functions
protected:
	void ResetDefaultFont();

	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	void DoCreate();

	/*afx_msg*/ void OnEditTogglebookmark();
	/*afx_msg*/ void OnClearAllBookmarks();
	/*afx_msg*/ void OnUpdateClearAllBookmarks(CCmdUI* pCmdUI);
	/*afx_msg*/ void OnEditNextbookmark();
	/*afx_msg*/ void OnEditPreviousbookmark();

	afx_msg void OnEditListmembers();
	afx_msg void OnEditIncreaseIndent();
	afx_msg void OnUpdateEditIncreaseIndent(CCmdUI* pCmdUI);
	afx_msg void OnEditDecreaseIndent();
	afx_msg void OnUpdateEditDecreaseIndent(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditListmembers(CCmdUI* pCmdUI);
	afx_msg void OnEditHideselection();
	afx_msg void OnEditStophidingcurrent();
	afx_msg void OnEditToggleoutlining();
	afx_msg void OnEditTogglealloutlining();
	afx_msg void OnEditCollapsetodefinitions();
	afx_msg void OnEditStopoutlining();
	afx_msg void OnUpdateEditStophidingcurrent(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditHideselection(CCmdUI* pCmdUI);
	afx_msg void OnEditAutooutlining();
	afx_msg void OnUpdateEditAutooutlining(CCmdUI* pCmdUI);
	afx_msg void OnEditEnableoutlining();
	afx_msg void OnUpdateEditEnableoutlining(CCmdUI* pCmdUI);
	afx_msg void OnEditLinenumbers();
	afx_msg void OnUpdateEditLinenumbers(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditStopoutlining(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditTogglealloutlining(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditToggleoutlining(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditCollapsetodefinitions(CCmdUI* pCmdUI);
	afx_msg void OnNcPaint();
			void CreateSymbolTableView();

	afx_msg void OnUpdateCaretPos(CCmdUI* pCmdUI);
	afx_msg void OnShowHint();
	/*afx_msg*/ void OnFind();
				void OnReplace();
				void OnReplaceAll();
	/*afx_msg*/ void OnFindNext();
	/*afx_msg*/ void OnFindPrev();
	/*afx_msg*/ void OnEditFindNextWord();

	DECLARE_MESSAGE_MAP()

private:
	virtual void GetUndoRedoActions(CStringList& lstActions, BOOL bUndo) const;
	void VerifyFindString(CBCGPToolbarComboBoxButton* pFindCombo, CString& strFindText);

// Load / Save
public:
	BOOL LoadElementFromTree(CNodeTree* pNode, BOOL* pbSaved = NULL);
	void LoadElement(CString* pText, BOOL* pbSaved = NULL);
	void LoadElement(SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode, BOOL* pbSaved = NULL, BOOL bAllowEmpty = TRUE, CString descr=L"");
	void LoadElement(WoormField* woormField, BOOL bViewMode, BOOL* pbSaved = NULL, BOOL bAllowEmpty = TRUE, CString descr = L"");
	void LoadElementGroupingRule(SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode, BOOL* pbSaved = NULL, BOOL bAllowEmpty = TRUE, CString descr = L"");
	void LoadElement(SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvent, BOOL* pbSaved = NULL);
	void LoadElement(ExpRuleData* pRule, BOOL* pbSaved = NULL);

	void EnableStringPreview(BOOL bEnabled = TRUE);

	void LoadFullReport1(CString sFileName, BOOL* pbSaved = NULL);
	void LoadFullReport(CString sFileName, BOOL* pbSaved = NULL, CString sError = L"", int line = -1, int pos = -1);

	BOOL OpenExprRuleEditor(CNodeTree* pNode);
	void OpenProcedureEditor(CNodeTree* pNode);
	void OpenQueryEditor(CNodeTree* pNode);
	void OpenRuleQueryEditor(CNodeTree* pNode);
	void OpenRuleLoopEditor(CNodeTree* pNode);

	void OpenFullTableRuleEditor(CNodeTree* pNode);
	void OpenWhereClauseEditor(CNodeTree* pNode);		//Having/Join-On too
	void OpenOrderByClauseEditor(CNodeTree* pNode);		//Group By too
	BOOL OpenCalcColumnEditor(CNodeTree* pNode);

	void OpenBreakingEventEditor(CNodeTree* pNode);
	//void OpenEventBreakingListEditor(CNodeTree* pNode);

	void OpenGroupActionsEditor(CNodeTree* pNode);
	void OpenGroupingRuleEditor(CNodeTree* pNode);

	//----------------------------
	BOOL DoSave();
	BOOL DoClose();
	//----------------------------

	void ShowDiagnostic(Parser&, const CString& = L"", BOOL bSkipLine = FALSE);
	void ShowDiagnostic(CString sError, int line = -1, int col = -1);
	virtual void ShowErrorText(CString sError);

	BOOL SaveProcedure();
	BOOL SaveQuery();
	BOOL SaveRuleQuery();
	BOOL SaveRuleLoop();

	BOOL SaveBreakingEvent();
	//BOOL SaveEventBreakingList();

	BOOL SaveGroupingRule();
	BOOL SaveGroupActionsList();

	BOOL SaveFunction();
	BOOL SaveRuleExpression();
	BOOL SaveFullTableRule();

	BOOL SaveOrderByClause();
	BOOL SaveWClause();
	BOOL SaveCalcColumn();

	BOOL SaveExpression();
	BOOL SaveBlock();

	//----------------------------
	BOOL SaveFullReport();

	void CheckFullReport();

	//----------------------------
	void CheckNamedQuery();
		void CheckQuery_AddParameters(const CStringArray&, SymTable* pSymTable, UINT& nLastUI);

	void CheckRuleQuery();

	void ExecuteNamedQuery();
	void ExecuteRuleQuery();
	//----------------------------

	virtual void StepOver() {}
};

//----------------------------------------------------------------------------
class TB_EXPORT CRSEditViewFullText: public CRSEditView 
{
	DECLARE_DYNCREATE(CRSEditViewFullText)
public:
	CRSEditViewFullText();

	//DECLARE_MESSAGE_MAP();
};

//----------------------------------------------------------------------------
class TB_EXPORT CRSEditViewDebug : public CRSEditView
{
	DECLARE_DYNCREATE(CRSEditViewDebug)
public:
	CRSEditViewDebug();

	CRSEditorDebugFrame* GetEditorFrame(BOOL bMustExists);

	BOOL OpenDebugger(ActionObj* pCurrCmd);
	virtual void StepOver();

	//DECLARE_MESSAGE_MAP();
};

//=============================================================================
class TB_EXPORT CRSEditViewDocked : public CRSEditView
{
	DECLARE_DYNCREATE(CRSEditViewDocked)
public:
	CRSEditViewDocked();
	virtual ~CRSEditViewDocked();

protected:
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	virtual void  OnDrawLineNumbersBar(CDC* pDC, CRect rect) {};

	virtual	void OnInitialUpdate();

	virtual void ShowErrorText(CString sError);

	BOOL PreTranslateMessage(MSG* pMsg);

	DECLARE_MESSAGE_MAP();
};

//---------------------------------------------------------------------------
class TB_EXPORT CRSEditorDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSEditorDockPane);
public:
	CRSEditorDockPane() : CRSDockPane(RUNTIME_CLASS(CRSEditViewDocked)) { }
};

///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
