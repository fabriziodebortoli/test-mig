#pragma once

#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbGenlib\TbPropertyGrid.h>

#include <TbOleDb\SqlCatalog.h>
#include <TbOledb\HelperExternalReferences.h>

#include <TbGes\ExtDocView.h>
#include <TbGes\ExtDocFrame.h>

#include "TbWoormEngine\ASKDATA.H"
#include "TbWoormEngine\RULEDATA.H"

#include "WoormDoc.h"
#include "RECTOBJ.h"

#include "RSEditor_Property.h"

//includere alla fine degli include del .H
#include "beginh.dex"
///////////////////////////////////////////////////////////////////////////////
class EventsData;

//===========================================================================
enum CRSTreeCtrlImgIdx 
{	
	NoGLyph = -2, //niente immagine
	Nothing = -1, //trattini

	ColumnGlyph,	ColumnHiddenGlyph,		ColumnHiddenExprGlyph,
	ColumnTotalGlyph,
	ImageGlyph,		ImageHiddenGlyph,		ImageHiddenExprGlyph,
	RectangleGlyph, RectangleHiddenGlyph,	RectangleHiddenExprGlyph,
	RepeaterGlyph,	RepeaterHiddenGlyph,	RepeaterHiddenExprGlyph,

	ChartGlyph,		ChartHiddenGlyph,		ChartHiddenExprGlyph,
	ChartCategoryGlyph, /*ChartCategoryHiddenGlyph, ChartCategoryHiddenExprGlyph,*/
	ChartSeriesGlyph, ChartSeriesHiddenGlyph, ChartSeriesHiddenExprGlyph,

	TableGlyph,		TableHiddenGlyph,		TableHiddenExprGlyph,
	TextGlyph,		TextHiddenGlyph,		TextHiddenExprGlyph,
	TextFileGlyph,	TextFileHiddenGlyph,	TextFileHiddenExprGlyph,
	FieldGlyph,		FieldGlyphHidden,		FieldGlyphHiddenExpr,
	BarcodeGlyph,	
	FuncArrayGlyph,

	LinkToRadar, LinkToForm, LinkToReport, LinkToFunction,
	LinkToFile, LinkToUrl, MailTo, CallTo, GoogleMap,

	PrimaryKey, ForeignKey, Total, 
	
	DataGlyph, DataPrimaryKeyGlyph, ExprGlyph, FuncGlyph, InputVarGlyph, InputAndAskVarGlyph,

	BreakPoint, BreakPointCondition, BreakPointAction, BreakPointConditionAction, BreakPointCurrent, BreakPointDisabled,

	MAXGlyph 
};

//===========================================================================
class TB_EXPORT CNodeTree : public CObject, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC(CNodeTree)
protected:
	BOOL		m_bCustomPaint	= FALSE;
	COLORREF	m_crItemColor	= 0;
	CFont*		m_pItemFont		= NULL;

public:
	enum ENodeType {
		NT_WRONG, NT_DUMMY_NODE,

		NT_SUBROOT_APP, NT_SUBROOT_MODULE,
	//nodi releativi ad oggetti esterni al report
		NT_LIST_ENUM_VALUE, NT_LIST_ENUM_TYPE,
		NT_LIST_WEBMETHOD, NT_LIST_FUNCTION, 
		NT_SUBROOT_DB_MODULE, NT_LIST_DBTABLE, NT_LIST_DBVIEW, NT_LIST_COLUMN_INFO, //NT_LIST_DBPROC,
		NT_ROOT_COMMANDS,NT_LIST_COMMAND,
		
		NT_GROUP_DB_FOREIGN_KEY, NT_GROUP_DB_EXTERNAL_REFERENCES,
		NT_LIST_DB_FOREIGN_KEY, NT_LIST_DB_EXTERNAL_REFERENCES, NT_LIST_DB_EXTERNAL_REFERENCES_INVERSE,  NT_LIST_EXTERNAL_REFERENCES_GENERIC,

		NT_LIST_SPECIAL_TEXT,
	//nodi di primo livello esterni al report
		NT_ROOT_ENUMS,
		NT_ROOT_FUNCTIONS, NT_GROUP_FUNCTIONS,
		NT_ROOT_WEBMETHODS,
		NT_ROOT_TABLES,
		NT_ROOT_USED_TABLES,
		NT_ROOT_MACRO_TEXT,
		NT_ROOT_FONTS_TABLE,
		NT_ROOT_FORMATTERS_TABLE,

		NT_ROOT_HTML_TAGS,
		NT_LIST_HTML_TAGS,

		NT_ROOT_QUERY_TAGS,
		NT_LIST_QUERY_TAGS,

	//nodi di primo livello
		NT_ROOT_LAYOUTS,
		NT_ROOT_LINKS, 
		NT_ROOT_VARIABLES,
		NT_ROOT_RULES, 
		NT_ROOT_TUPLE_RULES,
		NT_ROOT_MODULE,
		NT_ROOT_EVENTS,
		NT_ROOT_PROCEDURES, 
		NT_ROOT_QUERIES, 
		NT_ROOT_DIALOGS,

		NT_PAGE,
		NT_PROPERTIES,
		NT_SETTINGS,
		NT_PARAMETERS,
	//nodi generic
		NT_EXPR, NT_BLOCK,
	//elementi del report
		NT_LAYOUT,
			NT_OBJ_FIELDRECT, NT_OBJ_TEXTRECT, NT_OBJ_SQRRECT, NT_OBJ_GRAPHRECT, NT_OBJ_FILERECT,
			NT_OBJ_TABLE, NT_OBJ_COLUMN, NT_OBJ_TOTAL, 
			NT_OBJ_REPEATER, NT_OBJ_CHART, NT_OBJ_CATEGORY, NT_OBJ_SERIES,

		NT_VARIABLE, 
		NT_PROCEDURE, 
		NT_NAMED_QUERY, 
		//NT_RULE_QUERY_CALC_COLUMN,
		NT_ASKDIALOG, NT_ASKCONTROLS, NT_ASKGROUP, NT_ASKFIELD,

		NT_LINK, NT_LINK_PARAM, NT_LINK_PARAMETERS,

		NT_SUBROOT_REPORT_EVENTS,
		NT_SUBROOT_FORMFEED_EVENTS,
		NT_SUBROOT_FILLTABLE_EVENTS,
		NT_SUBROOT_TRIGGER_EVENTS, NT_TRIGGER_EVENT, NT_EVENT_BREAKING_LIST, NT_EVENT_SUBTOTAL_LIST,
		NT_FILLTABLE_EVENT,

		NT_TUPLE_FILTER,
		NT_TUPLE_GROUPING,
			NT_TUPLE_GROUPING_ACTIONS,
		NT_TUPLE_HAVING_FILTER,

		//NT_RULE_EXPR, 

		NT_RULE_QUERY_FULL_TABLE, 
			NT_RULE_QUERY_GROUP_FROM,
			NT_RULE_QUERY_TABLEINFO, NT_RULE_QUERY_JOIN_ON, 
			NT_RULE_QUERY_GROUP_COLUMNS, NT_RULE_QUERY_GROUP_CALC_COLUMNS, NT_RULE_QUERY_GROUP_UNSELECTED_COLUMN,
			NT_RULE_QUERY_WHERE, NT_RULE_QUERY_GROUPBY, 
			NT_RULE_QUERY_HAVING, NT_RULE_QUERY_ORDERBY, 
			NT_RULE_QUERY_GROUP_SELECT, NT_RULE_QUERY_GROUP_PARAMETERS,

		NT_RULE_NAMED_QUERY, NT_RULE_LOOP,

		NT_TOOLBOX_ROOT_OBJECTS, NT_TOOLBOX_OBJECT,

		NT_ROOT_BREAKPOINTS, NT_BREAKPOINT_ACTION,
		NT_DEBUG_RULE, NT_DEBUG_FILTER_TUPLE_RULE, NT_DEBUG_GROUPING_RULE, NT_DEBUG_HAVINGGROUP_RULE,

		//TODO
		NT_ROOT_UNDO_ACTIONS, NT_UNDO_ACTION
	};

	ENodeType			m_NodeType = NT_WRONG;
	HTREEITEM			m_ht = NULL;
	CRSTreeCtrlImgIdx	m_eImgIndex = CRSTreeCtrlImgIdx::NoGLyph;

	CObject*			m_pItemData = NULL;
	
	CObject*			m_pParentItemData = NULL;
	CObject*			m_pAncestorItemData = NULL;

	SymTable*			m_pSymTable = NULL;

	Expression**		m_ppExpr = NULL;
	BOOL				m_bViewMode = FALSE;
	DataType			m_ReturnType = DataType::Null;

	Block**				m_ppBlock = NULL;
	BOOL				m_bRaiseEvents = FALSE;

public:
	CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, ENodeType eNodeType, 
							CObject* pItemData = NULL, CObject* pParentItemData = NULL, CObject* pAncestorItemData = NULL);
	CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode);
	CNodeTree::CNodeTree(HTREEITEM ht, CRSTreeCtrlImgIdx eImgIndex, SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents);

	virtual CNodeTree::~CNodeTree() {}

	operator HTREEITEM () { return m_ht; }
	void Empty();

	void SetItemFont(CFont* pFont) { m_pItemFont = pFont; m_bCustomPaint = TRUE; }
	void SetItemColor(COLORREF crColor) { m_crItemColor = crColor; m_bCustomPaint = TRUE; }
	CFont*  GetItemFont() const { return m_pItemFont; }
	COLORREF GetItemColor() const { return m_crItemColor; }
	BOOL NeedCustomPaint() const { return m_bCustomPaint; }
};

// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CNodeTreeArray : public CObArray
{
public:
	CNodeTreeArray() {}
	CNodeTreeArray(CNodeTreeArray& src) { Copy(src); }
	//BOOL m_bChecked = FALSE;
public:
	// overloaded operator helpers
	CNodeTree*	operator[](int nIndex) const	{ return (CNodeTree*)GetAt(nIndex); }
	CNodeTree*& operator[](int nIndex)			{ return (CNodeTree*&)ElementAt(nIndex); }

	INT_PTR AddUnique(CNodeTree* newNode);
	INT_PTR Find(CNodeTree* newNode);
	//BOOL CheckOneSel(CNodeTree* newNode);
};

//===========================================================================
class TB_EXPORT CRSTreeCtrl : public CTBTreeCtrl
{
	friend class CRSReportTreeView;
	friend class CRSEditorToolView;
	friend class CRS_ObjectPropertyView;

	DECLARE_DYNAMIC(CRSTreeCtrl)
	Array m_arGarbage;	//all'uscita deletera tutti i nodi (gli ItemData) del tree

protected:
	CWoormDocMng*	m_pWDoc = NULL;
	
	CFont*	m_pBold = NULL;
	CFont*	m_pItalic = NULL;
	CFont*	m_pBoldItalic = NULL;
	BOOL m_bNeedCustomPaint = TRUE;

	LPCTSTR	m_strDragCommand = NULL;

public:
	BOOL		m_bShowMoreVariable = FALSE;
	BOOL		m_bShowAllTables = FALSE;
	BOOL		m_bShowNodeEnums = FALSE;
	BOOL		m_bShowEnumValues = TRUE;
	BOOL		m_bShowRelatedTables = TRUE;
	BOOL		m_bShowFilteredTables = FALSE;
	CString		m_FilterTablePattern;
	BOOL		m_bPassive = FALSE;

	//------------------------------
	HTREEITEM m_htVariables = NULL;
		HTREEITEM m_htHiddenGroupVariables = NULL;
	HTREEITEM m_htRules = NULL;

	HTREEITEM m_htTupleRules = NULL;

	HTREEITEM m_htEvents = NULL;
		HTREEITEM m_htReportEvents = NULL;
			HTREEITEM m_htReportAlwaysEvent = NULL;
			HTREEITEM m_htReportBeforeEvent = NULL;
			HTREEITEM m_htReportAfterEvent = NULL;
			HTREEITEM m_htReportFinalizeEvent = NULL;
		HTREEITEM m_htFormFeedEvents = NULL;
			HTREEITEM m_htFormFeedBeforeEvent = NULL;
			HTREEITEM m_htFormFeedAfterEvent = NULL;
		HTREEITEM m_htFillTableEvents = NULL;
		HTREEITEM m_htTriggerEvents = NULL;

	HTREEITEM m_htProcedures = NULL;
	HTREEITEM m_htQueries = NULL;
	HTREEITEM m_htAskDialogs = NULL;
	HTREEITEM m_htLinks = NULL;

	HTREEITEM m_htEnums = NULL;
	HTREEITEM m_htWebMethods = NULL;
	HTREEITEM m_htFunctions = NULL;
	HTREEITEM m_htCommands = NULL;
	HTREEITEM m_htFontTable = NULL;
	HTREEITEM m_htFormatterTable = NULL;
	HTREEITEM m_htHTMLTags = NULL;
	HTREEITEM m_htQueriesTags = NULL;

	HTREEITEM m_htLayouts = NULL;
	HTREEITEM m_htLayoutDefault = NULL;

	HTREEITEM m_htSpecialText = NULL;
	HTREEITEM m_htTables = NULL;
	HTREEITEM m_htRuleTables = NULL;

	/*HTREEITEM m_htPageInfo = NULL;
	HTREEITEM m_htProperties = NULL;
	HTREEITEM m_htSettings = NULL;*/

	HTREEITEM m_htUndoActions = NULL;
	HTREEITEM m_htToolBox = NULL;
	HTREEITEM m_htBreakpoints = NULL;

public:
	CRSTreeCtrl::CRSTreeCtrl();

	virtual CRSTreeCtrl::~CRSTreeCtrl() {}

	void Attach(CWoormDocMng* pWDoc);
	CWoormDocMng* GetDocument() { return m_pWDoc; }

	BOOL FillEnums(CRSEditView* editView);
	BOOL FillFunctions(CRSEditView* editView);
	BOOL FillWebMethods(CRSEditView* editView);
	BOOL FillCommands(CRSEditView* editView, BOOL bRaiseEvents = TRUE);
	CString GetCommandDescription(const CString& sCmd);

	BOOL FillVariables(BOOL bSort, BOOL bViewMode, BOOL bSkipSpecial = TRUE, BOOL bSkipInput = FALSE, BOOL bSkipTotal = TRUE);
	BOOL FillAllVariables(BOOL bSort, BOOL bViewMode, BOOL bSkipSpecial = FALSE, BOOL bSkipInput = FALSE, BOOL bSkipTotal = FALSE, CRSEditView* editView=NULL );
	BOOL FillVariablesGroupingRules(CRSEditView* editView);
	BOOL FillRules();
		void FillTblRule(HTREEITEM htParent, TblRuleData* pRule);
		void FillChildNamedQuery(HTREEITEM htParent, QueryObjItem*, QueryRuleData* pRule);
		void FillLoopRule(HTREEITEM htParent, WhileRuleData* pRule);

	BOOL FillTupleRules();

	BOOL FillEvents();
	CNodeTree&	FillTriggerEvent(EventsData* pEventsData, TriggEventData* pTriggerEvent, BOOL bSelect = FALSE);
	void		UpdateTriggerEvent(HTREEITEM htTriggerEvent, EventsData* pEventsData, TriggEventData* pTriggerEvent);

	BOOL FillProcedures();

	BOOL FillQueries();
	BOOL FillQueriesTags(CRSEditView* editView, BOOL bExpand);

	BOOL FillDialogs();
	BOOL FillLinks();

	BOOL FillLayouts(BOOL bSort = TRUE);
	BOOL FillLayout(CLayout* pObjects, HTREEITEM htLayout, BOOL bSkipAnchored =TRUE);

	BOOL FillSpecialTextRect(BOOL bExpand);
	BOOL FillHtmlTags(CRSEditView* editView, BOOL bExpand);

	BOOL FillTables(CRSEditView* editView=NULL);
	BOOL FillAllTables();

	BOOL FillRuleTables(TblRuleData*);

	BOOL FillToolBox(BOOL forceReload=FALSE);
	BOOL FillUndoActions();

	BOOL FillHiddenBlockForDebug	(HTREEITEM htParent, Block** ppBefore, Block** ppAfter, SymTable* pSymTable, Block* pCurrent);
	BOOL FillEventsForDebug			(Block* pCurrent = NULL);
	BOOL FillProceduresForDebug		(Block* pCurrent);
	BOOL FillDialogsForDebug		(Block* pCurrent);
	BOOL FillRulesForDebug			(Block* pCurrent);
	BOOL FillTupleRulesForDebug		(Block* pCurrent);
	BOOL FillBreakpoints			(ActionObj* pCurrentAct);

	CNodeTree& AddNode(const CString& sTitle, CNodeTree::ENodeType eType, HTREEITEM htParent = NULL, 
						CObject* pItem = NULL, CObject* pParentItem = NULL, CObject* pAncestorItem = NULL, BOOL bIsHidden = FALSE);
	CNodeTree& AddNode(int nImage, const CString& sTitle, HTREEITEM htParent, SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode);
	CNodeTree& AddNode(int nImage, const CString& sTitle, HTREEITEM htParent, SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents);
	
	BOOL AddDelayedNode(HTREEITEM& ht, const CString& sTitle, CNodeTree::ENodeType eType, HTREEITEM htParent = NULL, 
						CObject* pItem = NULL, CObject* pParentItem = NULL, CObject* pAncestorItem = NULL, BOOL bIsHidden = FALSE);

	void UpdateRSTreeNode(CNodeTree* pNode);
	void RemoveNode(CNodeTree* pNode);
	virtual BOOL	DeleteItem(_In_ HTREEITEM hItem);

	void  SetItemImage(HTREEITEM ht, CRSTreeCtrlImgIdx eImage);
	void  SetItemImage(CNodeTree*, CRSTreeCtrlImgIdx eImage);

	virtual HTREEITEM	Move(HTREEITEM hItem, BOOL bNext);

	CNodeTree* GetNode(HTREEITEM htItem);
	CNodeTree* GetParentNode(HTREEITEM htItem);
	CNodeTree* GetAncientNode(HTREEITEM htItem, CNodeTree::ENodeType type); //loop on parents until type was found
	CNodeTree* GetDescendantNode(HTREEITEM htItem, CNodeTree::ENodeType type); //loop on children until type was found

	virtual BOOL	OnFindItemTextOnFirstChild(HTREEITEM htItem);
	virtual BOOL	OnFindItemTextOnChild(HTREEITEM htItem, const CString&);
	virtual void	OnFindItemTextOnLastChild(HTREEITEM htItem);

	CNodeTree*	FindNode(CObject* pItemData, HTREEITEM htRoot = TVI_ROOT);

	HTREEITEM SelectRSTreeItemData(CObject* pObj, HTREEITEM htRoot, BOOL appendToSelected = FALSE);
	HTREEITEM DeselectRSTreeItemData(CObject* pObj, HTREEITEM htRoot);

	HTREEITEM SelectRSTreeItemByMatchingText(const CString& sMatchText, HTREEITEM hStartItem = NULL);

	HTREEITEM FindItem(const CString& name, HTREEITEM hRoot);

	virtual	void	InitializeImageList();
	BOOL			IsLayoutNodeType(CNodeTree::ENodeType nodeType);
	BOOL 			IsHiddenVariable(CNodeTree* pNode);
	virtual BOOL	PreTranslateMessage(MSG* pMsg);

	HTREEITEM GetDragItem();

	void EnableDrag(LPCTSTR lpszText);

	virtual BOOL OnMultiSelect();

	void RenameVariableNode(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);
	void RenameVariableNode(WoormField* pField);

	void RemoveFieldFromItemData(LPCTSTR pszFieldName);
	HTREEITEM RemoveObjectFromItemData(CObject* pItemData);

	afx_msg void OnItemExpanding(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnItemDeleted(NMHDR* pNMHDR, LRESULT* pResult);
	
	BOOL SortChildren(_In_opt_ HTREEITEM hItem);
protected:

	BOOL FillLink(HTREEITEM htLink, WoormLink*	pLink);

	BOOL FillTable(const SqlCatalogEntry* pCatalogEntry, HTREEITEM htParent, DataFieldLinkArray* = NULL);
	BOOL FillTable(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent, DataFieldLinkArray* = NULL);
	BOOL FillTableColumns(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent, DataFieldLinkArray* = NULL);
	void FillColumns(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent, DataFieldLinkArray* = NULL, BOOL bSkipLinked = FALSE);

	BOOL FillSubModuleTables	(CNodeTree * pNode, HTREEITEM htParent);

	BOOL FillForeignKey			(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent);
	BOOL FillExternalReference	(CHelperSqlCatalog::CTableColumns* pTC, HTREEITEM htParent);

	BOOL FillEnumsValue			(HTREEITEM htParent, DataType, CRSEditView* editView);
	BOOL FillEnumsValue			(HTREEITEM htParent,  EnumTag*,CRSEditView* editView, BOOL bColored = FALSE, BOOL delayed=FALSE);

	BOOL FillUnselectedColumns(CNodeTree* pNode);

	BOOL CheckAndClearEdge(HTREEITEM ht);

	virtual BOOL IsEqualItemData(DWORD dwItemData, DWORD dwExternalData);

	BOOL NeedCustomPaint();
	afx_msg void OnPaint();
	void PaintItem(HTREEITEM ht);
	BOOL IsEmptyNodeGroup(CNodeTree* pNode);
	BOOL IsNodeInMultiSelection(CNodeTree* pNode);

	int GetImgIndex(CNodeTree::ENodeType eType);
	int GetImgIndex(CObject* pObj);

	DROPEFFECT DragText(LPCTSTR lpszText, DROPEFFECT dwEffect);
	
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint point);
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint point);
	afx_msg void OnRButtonUp	(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove	(UINT nFlags, CPoint point);

	// Drag & Drop OLE message
	afx_msg virtual void OnBeginDrag(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CRSDockedView : public CAbstractFormView
{
	DECLARE_DYNAMIC(CRSDockedView);

	DECLARE_MESSAGE_MAP()

	afx_msg int OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message);

public:
	CRSDockedView(const CString& sName, UINT nIDD); 
};

//===========================================================================

class CRSEditView;

class TB_EXPORT CRSReportTreeView : public CRSDockedView
{
	friend class CRSFullReportDockPane;

	DECLARE_DYNCREATE(CRSReportTreeView);

public:
	CRSTreeCtrl m_TreeCtrl;
	BOOL m_bAddTable = FALSE;
	CResizableStrEdit m_edtFinder;
protected:
// --------------------- Drag & Drop ----------------------
	CPoint			m_PointDrop;
	COleDropTarget	m_DropTarget;
// --------------------------------------------------------
protected:
	virtual	void BuildDataControlLinks();
	virtual	void OnBuildDataControlLinks();

public:
	CRSReportTreeView();
	CRSReportTreeView(const CString& sName, UINT nIDD); //per le derivate
	virtual ~CRSReportTreeView();

	virtual BOOL FillTree();

	virtual	CTBToolBar*	 GetToolBar();

	CWoormDocMng* GetDocument();

	CRSEditView* CreateEditView();

	BOOL SelectLayoutObject(CObject*, BOOL bPassive = TRUE);

	void RemoveNode(CNodeTree* pNode);

	void OnOpenEditor();
	void OnOpenEditor(CNodeTree*);

	virtual void OnInitialUpdate();

	// Drag & Drop
	virtual DROPEFFECT OnDragEnter(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual DROPEFFECT OnDragOver(COleDataObject* pDataObject, DWORD dwKeyState, CPoint point);
	virtual DROPEFFECT OnDragScroll(CWnd* pWnd, DWORD dwKeyState, CPoint /*point*/);
	virtual BOOL OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point);
	void OnDropAction(CRSTreeCtrl* sourceTreeCtrl, HTREEITEM sourceHt, HTREEITEM targetHt, DROPEFFECT dropEffect);

	void MoveOrCloneLayoutObjects(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect);

	void AddRuleFromDrop(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect);
	void AddJoinFromDrop(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect);
	void AddColumnsFromDrop(CRSTreeCtrl* sourceTreeCtrl, CNodeTree* pSourceNode, HTREEITEM sourceHt, CNodeTree* pTargetNode, HTREEITEM targetHt, DROPEFFECT dropEffect);
	void AddJoin
				(	
					TblRuleData* pTblRule, 
					SqlTableInfo* tableInfo,
					CString sourceTableName,
					CString sTargetTableName = L"",
					CHelperSqlCatalog::CTableForeignTablesKeys* pFTK = NULL,
					CHelperExternalReferences::CTableSingleExtRef* pSER = NULL,
					BOOL reloadTree=TRUE
				);
	void FindJoinReferences
				(
					const CString& sDraggedTableName,
					const CString& sTargetTableName,
					CHelperSqlCatalog::CTableColumns*& pTargetTC,
					CHelperSqlCatalog::CTableForeignTablesKeys*& pFTK,
					CHelperExternalReferences::CTableSingleExtRef*& pSER
				);
	virtual void OnDragLeave();

protected:
	afx_msg void OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult);

	afx_msg void OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSelchangingTree(NMHDR* pNMHDR, LRESULT* pResult);

	afx_msg void OnFindTree();

	afx_msg void OnUpdateVKMove	(CCmdUI* pCmdUI);

	afx_msg	void OnVKUp			();
	afx_msg	void OnVKDown		();
	afx_msg	void OnVKLeft		();
	afx_msg	void OnVKRight		();

	BOOL PreTranslateMessage(MSG* pMsg);

	void OnUpdateCheckAddTable(CCmdUI* pCmdUI);
public:
	void OnCheckAddTable();
	BOOL CanOpenEditor(CNodeTree* pNode);
protected:
	void OnUpdateEdit(CCmdUI* pCmdUI);

	void OnRefresh();
	void OnUpdateRefresh(CCmdUI* pCmdUI);
	
	void OnNew();
	void OnUpdateNew(CCmdUI* pCmdUI);

	void OnMore();
	void OnUpdateMore(CCmdUI* pCmdUI);

	void OnDelete();
	void OnUpdateDelete(CCmdUI* pCmdUI);

	void OnUp();
	void OnDown();
	void OnUpdateUpDown(CCmdUI* pCmdUI);

	void OnDialogPreview();
	void OnUpdateDialogPreview(CCmdUI* pCmdUI);

	void OnCollapseAll();
	void OnUpdateCollapseAll(CCmdUI* pCmdUI);

	void OnExpandAll();
	void OnUpdateExpandAll(CCmdUI* pCmdUI);

private:

	BOOL IsDropText		(COleDataObject* pDataObject);
	BOOL ReadHdropData	(COleDataObject* pDataObject, DROPEFFECT dropEffect);
	void GetDropText	(CString& str, COleDataObject* pDataObject);

	void ChangeFieldsButtons(BOOL isActiveColumn);
	BOOL CanDeleteField(AskFieldData* pAskField);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSEngineTreeView : public CRSReportTreeView
{
	//friend class CRSFullReportDockPane;
	DECLARE_DYNCREATE(CRSEngineTreeView);

protected:
	virtual	void OnBuildDataControlLinks();

public:
	CRSEngineTreeView();
	//virtual ~CRSEngineTreeView();
	virtual	CTBToolBar*	 GetToolBar();

	virtual BOOL FillTree();
};

//=============================================================================
class TB_EXPORT CRSLayoutTreeView : public CRSReportTreeView
{
	//friend class CRSFullReportDockPane;
	DECLARE_DYNCREATE(CRSLayoutTreeView);

protected:
	virtual	void OnBuildDataControlLinks();

public:
	CRSLayoutTreeView();
	//virtual ~CRSLayoutTreeView();
	virtual	CTBToolBar*	 GetToolBar();

	virtual BOOL FillTree();	 

	BOOL PreTranslateMessage(MSG* pMgs);

};

//=============================================================================
class TB_EXPORT CRSDockPane : public CTaskBuilderDockPane
{
	DECLARE_DYNAMIC(CRSDockPane);
protected:
	CString m_sNsHelp;
public:
	CRSDockPane(CRuntimeClass* rc);
	virtual ~CRSDockPane();

	virtual BOOL CanFloat() const { return TRUE; }
	virtual BOOL CanBeClosed() const { return FALSE; }

	void ShowPanel(BOOL bShow, BOOL bAutoHideMode, DWORD dwAlignment)
	{
		if (bShow)
		{
			SetAutoHideMode(bAutoHideMode, dwAlignment);
			ShowWindow(SW_SHOW);
			return;
		}
		
		SetAutoHideMode(FALSE, dwAlignment);
		ShowWindow(SW_HIDE);
	}

	CWoormDocMng* GetDocument() { return dynamic_cast<CWoormDocMng*>(__super::GetDocument()); }

	void PrepareForClose();
	void OnDeactivateFrame(DWORD dwParkingDock);

	virtual void OnCustomizeToolbar();
	virtual void OnAddToolbarButtons() {}

	afx_msg BOOL OnHelpInfo(HELPINFO* pHelpInfo);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSFullReportDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSFullReportDockPane);

	DECLARE_MESSAGE_MAP()
public:
	CRSFullReportDockPane();
	CRSFullReportDockPane(CRuntimeClass* rc);
	virtual ~CRSFullReportDockPane();

	virtual void OnAddToolbarButtons();

protected:
	void OnRefresh		();
	void OnOpenEditor	();
	void OnNew			();
	void OnMore			();
	void OnDelete		();
	void OnUp			();
	void OnDown			();
	void OnDialogPreview();
	void OnCollapseAll	();
	void OnExpandAll	();

	afx_msg void OnCheckAddTable();
};

//=============================================================================
class TB_EXPORT CRSEngineDockPane : public CRSFullReportDockPane
{
	DECLARE_DYNCREATE(CRSEngineDockPane);

public:
	CRSEngineDockPane();
	virtual ~CRSEngineDockPane();
};

class TB_EXPORT CRSLayoutDockPane : public CRSFullReportDockPane
{
	DECLARE_DYNCREATE(CRSLayoutDockPane);

public:
	CRSLayoutDockPane();
	virtual ~CRSLayoutDockPane();
};

//=============================================================================
class TB_EXPORT CRSObjectPropertyDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSObjectPropertyDockPane);

	DECLARE_MESSAGE_MAP()
public:
	CRSObjectPropertyDockPane();

	virtual void OnAddToolbarButtons();

protected:
	void OnCollapseAll		();
	void OnExpand			();

	void OnLayoutBtn		();
	void OnVariableBtn		();
	void OnFindRuleBtn		();
	void OnRequestFieldBtn	();
	void OnRefresh			();
	void OnApply			();
	void OnDiscard			();
};

//=============================================================================

class TB_EXPORT CRSToolBoxObjectsView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSToolBoxObjectsView);
public:
	CRSTreeCtrl m_TreeCtrl;

	BOOL m_bAddTable = FALSE;

public:
	CRSToolBoxObjectsView();
	virtual ~CRSToolBoxObjectsView();

	virtual BOOL FillTree();

protected:
	void  virtual OnInitialUpdate();

	CWoormDocMng* GetDocument();

	virtual	CTBToolBar*	 GetToolBar();

	virtual	void BuildDataControlLinks();

	void OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult);

	afx_msg void OnUpdateAddTable(CCmdUI* pCmdUI);

public:
	afx_msg void OnAddTable();

	DECLARE_MESSAGE_MAP()
};


//=============================================================================
class TB_EXPORT CRSToolBoxDBView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSToolBoxDBView);

public:	
	CRSTreeCtrl m_TreeCtrl;
	CResizableStrEdit m_edtFinder;

	BOOL m_bAddTable = FALSE;
	BOOL m_bAddHidden = FALSE;

public:
	CRSToolBoxDBView();
	virtual ~CRSToolBoxDBView();

	afx_msg void OnFindTree();
	virtual BOOL FillTree();

protected:
	CWoormDocMng* GetDocument();
	virtual	CTBToolBar*	 GetToolBar();

	void  virtual OnInitialUpdate();
	virtual	void BuildDataControlLinks();

	void OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult);

	BOOL PreTranslateMessage(MSG* pMsg);

	afx_msg void OnUpdateCheckAddHidden(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCheckAddTable(CCmdUI* pCmdUI);

	afx_msg void OnUpdateAddHiddenVar(CCmdUI* pCmdUI);

	afx_msg void OnUpdateMore(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFilter(CCmdUI* pCmdUI);

	afx_msg void OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKillFocus(NMHDR* pNMHDR, LRESULT* pResult);

public:
	afx_msg void OnCheckAddHidden();
	afx_msg void OnCheckAddTable();

	afx_msg void OnAddHiddenVar();

	afx_msg void OnMore();
	afx_msg void OnFilter();

	afx_msg	void OnVKUp();
	afx_msg	void OnVKDown();
	afx_msg	void OnVKLeft();
	afx_msg	void OnVKRight();

	void OnCollapseAll();
	void OnUpdateCollapseAll(CCmdUI* pCmdUI);

	void OnExpand();
	void OnUpdateExpand(CCmdUI* pCmdUI);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================

class TB_EXPORT CRSToolBarView : public CRSDockedView
{
	DECLARE_DYNCREATE(CRSToolBarView);

protected:

	//BORDERS
	CBCGPButton m_btBorderAll;
	CBCGPButton m_btBorderTop;
	CBCGPButton m_btBorderBottom;
	CBCGPButton m_btBorderLeft;
	CBCGPButton m_btBorderRight;
	//SIZE
	CBCGPButton m_btEnlargeTop;		//ID_VK_SHIFT_UP
	CBCGPButton m_btNarrowTop;		//ID_VK_SHIFT_DOWN
	CBCGPButton m_btNarrowRight;	//ID_VK_SHIFT_LEFT
	CBCGPButton m_btEnlargeRight;	//ID_VK_SHIFT_RIGHT

	CBCGPButton m_btNarrowBottom;	//ID_VK_CTRL_UP
	CBCGPButton m_btEnlargeBottom;	//ID_VK_CTRL_DOWN
	CBCGPButton m_btEnlargeLeft;	//ID_VK_CTRL_LEFT
	CBCGPButton m_btNarrowLeft;		//ID_VK_CTRL_RIGHT
	//MOVE
	CBCGPButton m_btArrowUp;		//D_VK_UP
	CBCGPButton m_btArrowDown;		//ID_VK_DOWN
	CBCGPButton m_btArrowLeft;		//ID_VK_LEFT
	CBCGPButton m_btArrowRight;		//ID_VK_RIGHT

	CBCGPButton m_btMoveColLeft;	//ID_COL_LMOVE
	CBCGPButton m_btMoveColRight;	//ID_COL_RMOVE

	CBCGPButton m_btMoveOnGrid;		//ID_SNAP_TO_GRID

	//ALIGMENT
	CBCGPButton m_btAlignLeft;		//ID_ALIGN_HLEFT
	CBCGPButton m_btAlignRight;		//ID_ALIGN_HRIGHT
	CBCGPButton m_btAlignTop;		//ID_ALIGN_VTOP
	CBCGPButton m_btAlignBottom;	//ID_ALIGN_VBOTTOM

	CBCGPButton m_btEqualHorizontalSpacing; // ID_ALIGN_HSPACE_EQUAL
	CBCGPButton m_btEqualVerticalSpacing; //ID_ALIGN_VSPACE_EQUAL

	CBCGPButton m_btSameWidth;	// ID_LAST_LARGE
	CBCGPButton m_btSameHeight; //ID_LAST_HIGH

	CBCGPButton m_btTileLeft;	// ID_ALIGN_STACK_LEFT
	CBCGPButton m_btTileRight;	// ID_ALIGN_STACK_RIGHT
	CBCGPButton m_btTileTop;	// ID_ALIGN_STACK_TOP
	CBCGPButton m_btTileBottom;	// ID_ALIGN_STACK_BOTTOM

	CBCGPButton m_btJustifyLeft;	// ID_ALIGN_CUT_H_LEFT
	CBCGPButton m_btJustifyRight;	// ID_ALIGN_CUT_H_RIGHT
	CBCGPButton m_btJustifyTop;		// ID_ALIGN_CUT_V_TOP
	CBCGPButton m_btJustifyBottom;	// ID_ALIGN_CUT_V_BOTTOM

	CBCGPButton m_btToggleTrasparent;	//ID_TOGGLE_TRANSPARENT
	CBCGPButton m_btRemoveLayoutObject;	//ID_RS_REMOVE_LAYOUT_OBJECT

	void addbtn(CBCGPButton& btn, UINT id, CString nsIcon, CString tooltip);

public:
	CRSToolBarView();
	virtual ~CRSToolBarView();

protected:
	void  virtual OnInitialUpdate();

	CWoormDocMng* GetDocument();

	virtual	CTBToolBar*	 GetToolBar();

	virtual	void BuildDataControlLinks();

	//-----
	afx_msg void OnIdleUpdateCmdUI();

	afx_msg void OnUpdateVKSize(CCmdUI* pCmdUI);
	afx_msg void OnUpdateVKMove(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAlignmentBar(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSnapToGrid(CCmdUI* pCmdUI);
	afx_msg void OnUpdateToggleTransparent(CCmdUI* pCmdUI);
	afx_msg void OnUpdateColMove(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCut(CCmdUI* pCmdUI);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSToolBoxDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSToolBoxDockPane);
public:
	CRSToolBoxDockPane();
protected:
	CRSToolBoxDockPane(CRuntimeClass* rc) : CRSDockPane(rc) { }
public:
	virtual void OnAddToolbarButtons();

protected:
	afx_msg void OnAddTable();

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSToolBoxDBDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSToolBoxDBDockPane);

public:
	CRSToolBoxDBDockPane();

	virtual void OnAddToolbarButtons();

protected:
	afx_msg void OnCheckAddTable();
	afx_msg void OnCheckAddHidden();

	afx_msg void OnAddHiddenVar();

	afx_msg void OnMore();
	afx_msg void OnFilter();

	void OnCollapseAll();
	void OnExpand();

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CRSToolBarDockPane : public CRSDockPane
{
	DECLARE_DYNCREATE(CRSToolBarDockPane);
public:
	CRSToolBarDockPane();
};

//=============================================================================
#define TOOLBAR_HEIGHT 28

//TREE
#define RS_COLOR_VARIABLE RGB(163, 73, 164)
#define RS_COLOR_EMPTY RGB(127, 127, 127)
#define RS_COLOR_LIGHT_BLUE RGB(0, 148, 255)
#define RS_COLOR_ACTIONS	RGB(0, 0, 255)
#define RS_COLOR_FRAMEWORK	RGB(0, 128, 0)
#define RS_COLOR_BREAKPOINT	RGB(255, 0, 255)

//PROP
#define RS_COLOR_HighlightItemTree RGB(215, 226, 243)
#define RS_COLOR_PROP_DARKGREY RGB(51, 51, 51)
#define RS_COLOR_PROP_MANDATORY RGB(255, 216, 0)
#define RS_COLOR_PROP_IMPORTANT RS_COLOR_LIGHT_BLUE

//HELP
#define RS_HELP_MAIN				L"Abstract.TBS.ReportingStudio.DesignMode"

#define RS_HELP_PANEL_REPORT		L"ReportPanel"
#define RS_HELP_PANEL_ENGINE		L"EnginePanel"
#define RS_HELP_PANEL_LAYOUT		L"LayoutPanel"

#define RS_HELP_PANEL_PROPERTY		L"PropertyPanel"

#define RS_HELP_PANEL_TOOLBOX_OBJECT	L"ToolBoxObjectPanel"
#define RS_HELP_PANEL_TOOLBOX_DB		L"ToolBoxDBPanel"
#define RS_HELP_PANEL_TOOLBOX_BAR		L"ToolBoxBarPanel"


///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
