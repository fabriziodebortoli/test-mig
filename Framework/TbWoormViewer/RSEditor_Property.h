#pragma once

#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbGenlib\TbPropertyGrid.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\ExtDocFrame.h>

#include "TbWoormEngine\ASKDATA.H"
#include "TbWoormEngine\ASKDLG.H"
#include "TbWoormEngine\RULEDATA.H"

#include "WoormDoc.h"
#include "RECTOBJ.h"
#include "MULSELOB.H"

//#include "RSEditorUI.h"

//includere alla fine degli include del .H
#include "beginh.dex"
///////////////////////////////////////////////////////////////////////////////

class CRSEditView;
class CNodeTree;
class Repeater;
class TriggEventData;
class CRS_ObjectPropertyView;

CString FormatExprAndDescription(CString description, CString expr);

//=============================================================================
class TB_EXPORT CRS_PropertyGrid : public CBCGPPropList, public ResizableCtrl
{
	DECLARE_DYNCREATE(CRS_PropertyGrid)
public:
	enum NewElementType{ 
		NET_NONE, NEW_ELEMENT, NEW_DB_ELEMENT, NEW_FUNCTION_LINK , NEW_URL_LINK, NEW_FORM_LINK, NEW_REPORT_LINK
	};
	NewElementType m_NewElement_Type = NET_NONE;

	enum Img { NoImg = -1, QuestionMark, OpenFolder, CenterPoint, 
				Barcode, Image, Textfile, 
				PrimaryKey, ForeignKey, 
				DBVar, FuncVar, ExprVar, InputVar,
				Rotate,
				BreakPoint, BreakPointCondition, BreakPointAction, BreakPointConditionAction, BreakPointCurrent, BreakPointDisabled,
				Max };

	CBCGPToolBarImages	m_imageList;

	CRS_PropertyGrid();
	virtual ~CRS_PropertyGrid();

	virtual	BOOL	SubclassDlgItem(UINT, CWnd*);

	CRS_ObjectPropertyView*	GetPropertyView();

	void SetFocusOnSearch() { if (m_wndFilter.m_hWnd) m_wndFilter.SetFocus(); }

	 BOOL			PreTranslateMessage(MSG* pMsg);

	 CBCGPProp*		GetNextProperty(CBCGPProp* prop, BOOL cycleFinished = FALSE);
	 CBCGPProp*		GetNextVisibleInFilterProperty(CBCGPProp* prop, BOOL cycleFinished = FALSE);

	 CBCGPProp*		GetPrevProperty(CBCGPProp* prop, BOOL justStarted = TRUE);
	 CBCGPProp*		GetPrevVisibleInFilterProperty(CBCGPProp* prop, BOOL justStarted = TRUE);

	 virtual void	SetPropertiesFilter(LPCTSTR lpszFilter);

	 void			ClearSearchBox();

	 void			ExpandCurrentProperty(BOOL bExpand = TRUE, BOOL bDeep = TRUE);

private: 
	void InitializeImgList();
	void EnsureParentPropertiesVisible(CBCGPProp* pProp);

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM)			{ DoRecalcCtrlSize(); return 0L; }

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CRSAskFieldProp;
class CrsProp;
class CRSPageProp;
class CRSColumnAlignBitwiseProp;
class CRSMultiColumnsAlignmentStyleBitWiseProp;

class TB_EXPORT CRS_ObjectPropertyView : public CAbstractFormView
{
	friend class CRSFieldTypeProp;
	friend class CRSColumnTypeProp;
	friend class CRSBarCodeComboProp;
	friend class CRSAskFieldProp;
	friend class CRSStringWithExprProp;
	friend class CRSShowColumnTotalProp;
	friend class CRSPageProp;
	friend class CRSTblRuleProp;
	friend class CRSCommonProp;
	friend class CRSPageProp;
	friend class CRSTriggerEventProp;
	friend class CRSSearchTbDialogProp;
	friend class CRSTableAsChartProp;
	friend class CRSVariableProp;
	friend class CRSNewLinkFunction;
	friend class CRSAlignBitwiseProp;
	friend class CRSNewLinkParam;
	friend class CRSNewLinkExplorer;
	friend class CRSCheckBoxProp;
	friend class CWoormView;
	friend class CRSBarCodeSizeProp;
	friend class CRSBarCodeShowTextProp;
	friend class CRSChartFieldComboProp;
	friend class CRSChartTypeComboProp;
	friend class CRSChartBoolProp;

	DECLARE_DYNCREATE(CRS_ObjectPropertyView);
private:
	Array m_arGarbage;	//all'uscita deletera tutti i nodi (gli ItemData) del tree

protected:
	CRS_PropertyGrid*	m_pPropGrid;

	//Puntatore alla multiselezione (se è presente)
	TDisposablePtr<SelectionRect>			m_pMulSel;
	//Puntatore alla multiselezione delle colonne (se è presente)
	TDisposablePtr<MultiColumnSelection>	m_pMulCol;

	//Puntatore alla tablecell selezionata (se è presente)
	TableCell* m_pSelectedTableCell;

	CString m_NewName;
	DataType m_NewType;
	BOOL m_bIsHidden;
	BOOL m_bIsArray;
	WoormField::RepFieldType m_fieldType;

	void SetCheckLayoutBtn		(BOOL checked);
	void SetCheckVariableBtn	(BOOL checked);
	void SetCheckRequestFieldBtn(BOOL checked);

public:
	BOOL		m_bNeedsApply;
	BOOL		m_bCreatingNewTable;
	BOOL		m_bAllFieldsAreHidden;
	BOOL		m_bFromDragnDrop;
	CNodeTree*	m_pTreeNode;

	BOOL		m_bShowLayoutBtn;
	BOOL		m_bCheckLayoutBtn;
	BOOL		m_bCheckLayoutBtnChanged;		//per ottimizzazione dell'on_update_command_ui

	//BOOL		m_bShowVariableBtn;
	WoormField::SourceFieldType m_eShowVariableTypeBtn;
	BOOL		m_bCheckVariableBtn;
	BOOL		m_bCheckVariableBtnChanged;		//per ottimizzazione dell'on_update_command_ui	

	BOOL		m_bShowRequestFieldBtn;
	BOOL		m_bCheckRequestFieldBtn;
	BOOL		m_bCheckRequestFieldBtnChanged;	//per ottimizzazione dell'on_update_command_ui	

	BOOL		m_bShowFindRuleBtn;
	BOOL		m_bAddNewReportLinkParams;

public:
	CRS_ObjectPropertyView();
	virtual ~CRS_ObjectPropertyView();

	CWoormDocMng*		GetDocument();
	CRSEditView*		CreateEditView();
	CRS_PropertyGrid*	GetPropertyGrid() { return m_pPropGrid; }

	void OnInitialUpdate();

	BOOL IsNeedsApply() const  { return m_bNeedsApply; }
	void SetNeedsApply(BOOL b)   { m_bNeedsApply = b; }

	void  SetPanelTitle(const CString& sTitle);
	void  SetFocusOnFirstProperty();

	BOOL ClearPropertyGrid();

	void LoadPropertyGrid				(CNodeTree* pNode);
	void ReLoadPropertyGrid				();
	void NewObjectPropertyGrid			(CNodeTree* pNode);	//Create new object, set m_bNeedsApply = TRUE
	//Multiple selection
	BOOL LoadMultipleSelectionProperties(SelectionRect* pMulSel);
	void LoadMultiColumnProperties		(MultiColumnSelection* pMulCol);
	//TableCell selection
	void LoadTableCellProperties		(TableCell* pCell);

	void NewDBElement	(BOOL newTable=FALSE, BOOL isAllFieldsAreHidden=FALSE, TblRuleData* pTblRule=NULL);
	void NewElement		(BOOL newTable=FALSE, BOOL FromHiddenVariable=FALSE, BOOL fromDragnDrop=FALSE, BOOL defaultHidden = FALSE);
	
	void NewHyperLink();
	void NewFunctionLink();
	void CreateNewFunctionLink();	
	void NewURLLink();
	void CreateNewUrlLink();
	void NewFormLink();
	void CreateNewFormLink();
	void NewReportLink();
	void CreateNewReportLink();
	void AddReportLinkParameters();

	void NewBreakingEvent(CNodeTree* pNode, BOOL addNewFields=FALSE, BOOL addNewSubtotal = FALSE, BOOL addNew=FALSE);
	void CreateNewBreakingEvent(BOOL onlyColumns=FALSE);

	void LoadTypeProp(CBCGPProp* prop);
	void LoadTypeProp(CBCGPProp* prop, CWordArray& typeArray);
	void LoadEnumTypeProp(CBCGPProp* prop, CString filter=L"");
	void LoadTableModules(CBCGPProp* prop, CString filter=L"");
		void LoadTables(CBCGPProp* prop, AddOnModule* = NULL, BOOL bExternalTable = FALSE, CString filter=L"");
	void LoadHotlinkModules(CBCGPProp* prop, DataType retVal = DataType::Variant);
		void LoadHotlinks(CBCGPProp* prop, AddOnModule*, DataType retVal = DataType::Variant);
	void LoadWebMethodModules(CBCGPProp* prop);
		void LoadWebMethods(CBCGPProp* prop, AddOnModule*);

	void OnCollapseAll		();
	void OnExpand			();
	void OnLayoutBtn		();
	void OnVariableBtn		();
	void OnFindRuleBtn		();
	void OnRequestFieldBtn	();
	void OnDialogPreviewBtn	();

protected:
	void LoadLinkPropertyGrid(CNodeTree* pNode, BOOL addParams=FALSE);
	void LoadLinkParamPropertyGrid(CNodeTree* pNode);

	void LoadPagePropertyGrid(CNodeTree* pNode);
	void LoadPropertiesPropertyGrid(CNodeTree* pNode);
	void LoadRSSettingsPropertyGrid(CNodeTree* pNode);
	void LoadWoormParametersPropertyGrid(CNodeTree* pNode);
	
	void LoadVariablePropertyGrid(CNodeTree* pNode);

	void LoadAskDialogPropertyGrid(CNodeTree* pNode);
	void LoadAskGroupPropertyGrid(CNodeTree* pNode);
	void LoadAskFieldPropertyGrid(CNodeTree* pNode);

	void NewAskFieldPropertyGrid(CNodeTree* pNode);
	
	void NewLayoutPropertyGrid(CNodeTree* pNode);
	void NewProcedurePropertyGrid(CNodeTree* pNode);
	void NewNamedQueryPropertyGrid(CNodeTree* pNode);

	void InsertColumnBlock(CBCGPProp* father, CBCGPProp* caller, CString tblName, TblRuleData* pTblRule);

	void InsertLinkParameterBlock(CBCGPProp* caller=NULL, BOOL ifUrl=FALSE);
	void InsertReportLinkParameterBlock(WoormLink* pLink, CString nameSpace, CBCGPProp* caller = NULL);

	void LoadNamedQueryRulePropertyGrid(CNodeTree* pNode);
	//TableRuleData
	void LoadTableRulePropertyGrid(CNodeTree* pNode);

	void NewJoinTablePropertyGrid(CNodeTree* pNode);
	void LoadJoinTablePropertyGrid(CNodeTree* pNode);

	void NewCalcColumnPropertyGrid(CNodeTree* pNode);
	void NewColumnsPropertyGrid(CNodeTree* pNode);

	void LoadCalcColumnPropertyGrid(CNodeTree* pNode);
	void LoadColumnPropertyGrid(CNodeTree* pNode);

	void LoadProcedurePropertyGrid(CNodeTree* pNode);
	void LoadNamedQueryPropertyGrid(CNodeTree* pNode);

	//chart category
	void LoadChartCategoryPropertyGrid(CNodeTree* pNode);
	//chart series
	void LoadChartSeriesPropertyGrid(CNodeTree* pNode);

	void NewRulesPropertyGrid(CNodeTree* pNode);
public:
	void NewRulesFromDropTablePropertyGrid(CNodeTree* pSourceNode);
protected:
	void NewVariablePropertyGrid(CNodeTree* pNode);

	CString	SetImageVarType(WoormField* pF, BOOL bIsColumn, CrsProp* prop);

	//Layout
	void LoadLayoutsPropertyGrid(CNodeTree* pNode);
	void LoadSingleLayoutPropertyGrid(CNodeTree* pNode);
	//Layout Objects
	void LoadLayoutObjectPropertyGrid(CNodeTree* pNode);
	//base rect
	void LoadBaseProperties(CNodeTree* pNode);
	void LoadBaseAppearenceProperties(BaseRect* pBase);
	void LoadBaseBehaviorProperties(BaseRect* pBase);
	void LoadBaseLayoutProperties(BaseRect* pBase);
	//text rect
	void LoadTextProperties(CNodeTree* pNode);
	void LoadTextAppearenceProperties(TextRect* pText);
	void LoadTextBehaviorProperties(TextRect* pText);
	//field rect
	void LoadFieldProperties(CNodeTree* pNode);
	void LoadFieldAppearenceProperties(FieldRect* pField);
	void LoadFieldBehaviorProperties(FieldRect* pField);
	//file rect
	void LoadFileProperties(CNodeTree* pNode);
	void LoadFileAppearenceProperties(FileRect* pFile);
	void LoadFileBehaviorProperties(FileRect* pFile);
	//graph rect
	void LoadGraphProperties(CNodeTree* pNode);
	void LoadGraphAppearenceProperties(GraphRect* pFile);
	void LoadGraphBehaviorProperties(GraphRect* pFile);
	//sqr rect
	void LoadSqrProperties(CNodeTree* pNode);
	//chart
	void LoadChartProperties(CNodeTree* pNode);
	void LoadChartAppearenceProperties(Chart* pChart);
	void LoadChartBehaviorProperties(Chart* pChart);
	void LoadChartLayoutProperties(Chart* pChart);
	
	//table
	void LoadTableProperties(CNodeTree* pNode);
	void LoadTableAppearenceProperties(Table* pTable);
	void LoadTableBehaviorProperties(Table* pTable);
	void LoadTableLayoutProperties(Table* pTable);
	//table cell methods
	void CopyTableCellPropertiesToAllRow();
	//column
	void LoadColumnProperties(CNodeTree* pNode);
	void LoadColumnAppearenceProperties(TableColumn* pCol);
	void LoadColumnBehaviorProperties(TableColumn* pCol);
	void LoadColumnLayoutProperties(TableColumn* pCol);
	//tablecell
	void LoadTableCellProperties(CNodeTree* pNode);
	//multi column
	void LoadMultiColumnAppearenceProperties(MultiColumnSelection* pMulCol);
	void LoadMultiColumnBehaviorProperties(MultiColumnSelection* pMulCol);
	void LoadMultiColumnLayoutProperties(MultiColumnSelection* pMulCol);
	//repeater
	void LoadRepeaterProperties(CNodeTree* pNode);
	void LoadRepeaterLayoutProperties(Repeater* pRep);
	//Multiple selection
	void LoadMultipleSelectionAllProperties(SelectionRect* pMulSel);
	void LoadMultipleSelectionAppearenceProperties(SelectionRect* pMulSel);
	void LoadMultipleSelectionBehaviorProperties(SelectionRect* pMulSel);
	void LoadMultipleSelectionLayoutProperties(SelectionRect* pMulSel);
	// Variable Properties		
	void LoadVariableGeneralSettings(WoormField* wrmField);
	void LoadAttributesForExporting(WoormField* wrmField);

	// Ask dialog properties
	void LoadAskDialogSettings(AskDialogData* askDialogField);

	// Ask group properties
	void LoadAskGroupGeneralSettings(AskGroupData* askDialogField);

	// Ask field properties
	void LoadAskFieldGeneralSettings(AskFieldData* field);
	void LoadAskFieldCaption(AskFieldData* field);
	void LoadAskFieldBehavior(AskFieldData* field, CRSAskFieldProp*& pControlType);
	void LoadAskFieldHotLinkSettings(AskFieldData* field, CRSAskFieldProp* pControlType);

	//Link properties
	void LoadLinkSettings(WoormLink* wrmLink);
	void LoadLinkParamSettings(WoormField* wrmField);

	//PageInfo
	void LoadPageInfoSettings(PageInfo*pgInfo);
	PrinterInfoItem* GetDefaultPaperSize(PrinterInfo* printerInfo, int paperWidth, int paperLength);

	//Properties page settings
	void LoadPropertiesInfo(CDocProperties* properties, BOOL allowEdit=TRUE);

	//RS Settings
	void LoadRSSettingsInfo(WoormIni* wrmIni);
	WoormLink* CreateNewLinkObject(WoormLink::WoormLinkType type, WoormLink::WoormLinkSubType subType, CString owner, CString target, int alias, BOOL linkedByVar);
	void AddLinkParameters(CNodeTree* pNode);

	//Woorm parameters settings
	void LoadWoormParameters(CWoormInfo* info);

private:
	void AddBasicCommands();

protected:
	afx_msg LRESULT OnCommandClicked(WPARAM, LPARAM);
public:
	void OnRefresh();
	void OnDiscard();
	void OnApply();

private:
	//gruppi per la propertyGrid
	TDisposablePtr<CrsProp> m_pAppearenceGroup;
	TDisposablePtr<CrsProp> m_pBehaviorGroup;
	TDisposablePtr<CrsProp> m_pLayoutGroup;

	//AskField - Hotlink
	TDisposablePtr<CRSAskFieldProp>	hotLinkGroup;
	TDisposablePtr<CRSAskFieldProp>	hotlinkNamespace;
	TDisposablePtr<CRSAskFieldProp>	hotLinkName;
	TDisposablePtr<CrsProp>			hotlinkCustParameters;
	TDisposablePtr<CRSAskFieldProp>	showHotlinkDescription;
	TDisposablePtr<CRSAskFieldProp>	hotlinkDescriptionCombo;
	TDisposablePtr<CRSAskFieldProp>	hotlinkMultiselectionCombo;
	
	//Page settings(paper, size etc)
	TDisposablePtr<CRSPageProp> wrmStyle;
	TDisposablePtr<CRSPageProp> wrmWidth;
	TDisposablePtr<CRSPageProp> wrmLength;
	TDisposablePtr<CRSPageProp> printerStyle;
	TDisposablePtr<CRSPageProp> printerWidth;
	TDisposablePtr<CRSPageProp> printerLength;
	TDisposablePtr<CRSPageProp> marginLeft;
	TDisposablePtr<CRSPageProp> marginRight;
	TDisposablePtr<CRSPageProp> marginTop;
	TDisposablePtr<CRSPageProp> marginBottom;

	//Trigger Events
	CStringArray m_arNewBreakingFields;
	TDisposablePtr<CrsProp> m_pBreakingFieldListGroup;
	CStringArray m_arNewSubTotalFields;
	TDisposablePtr<CrsProp> m_pSubTotalGroup;

	//Variable
	TDisposablePtr<CRSVariableProp> isArray;

	//----------------------------------------
	virtual	void BuildDataControlLinks();
	void InitCRS_PropertyGrid();

	//void UpdateReportTree();

	void OnUpdateRefresh		(CCmdUI* pCmdUI);
	
	void OnUpdateCollapseAll	(CCmdUI* pCmdUI);
	void OnUpdateExpandAll		(CCmdUI* pCmdUI);
	void OnUpdateApply			(CCmdUI* pCmdUI);
	void OnUpdateLayout			(CCmdUI* pCmdUI);
	void OnUpdateVariable		(CCmdUI* pCmdUI);
	void OnUpdateRequestField	(CCmdUI* pCmdUI);
	void OnUpdateFindRule		(CCmdUI* pCmdUI);

	void CreateNewAskField();
	
	void CreateNewLayout();

	void CreateNewProcedure();
	void CreateNewNamedQuery();

	void CreateNewJoinTable();
	void CreateNewCalcColumn();

	void CreateNewElement();
	void CreateNewDBElement();

	void ChangeVariableType();

	void LoadVariablesIntoProperty(CBCGPProp* prop);
	BOOL ReadUserParametersIntoLink(CBCGPProp* addParamProp /*head property for parameters sub-blocks non editable*/, WoormLink* newLink);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CRSColorProp;
class CRSMMProp;

class CrackProp : public CBCGPProp
{
public:
	using	CBCGPProp::m_pWndList;
	using	CBCGPProp::m_lstSubItems;
	using	CBCGPProp::SetFilter;
	using	CBCGPProp::m_bInFilter;
	using	CBCGPProp::ExpandDeep;
};

//Classe derivata che implementa la possibilità di rendere o meno visibile una property
class CrsProp : public CBCGPProp, public IDisposingSourceImpl
{
	friend class CRS_PropertyGrid;
	friend class CRS_ObjectPropertyView;
	friend class CRSAlignBitwiseProp;
public:
	enum State { Normal, Error/*rosso*/, Mandatory/*arancione*/, Important/*blue*/, Marked/*nero*/	};

	//default constructors
	CrsProp(const CString& strGroupName, DWORD_PTR dwData = 0,
		BOOL bIsValueList = FALSE);

	CrsProp(const CString& strName, const _variant_t& varValue,
		LPCTSTR lpszDescr = NULL, DWORD_PTR dwData = 0,
		LPCTSTR lpszEditMask = NULL, LPCTSTR lpszEditTemplate = NULL,
		LPCTSTR lpszValidChars = NULL);

	CrsProp(const CString& strName, UINT nID, const _variant_t& varValue,
		LPCTSTR lpszDescr = NULL, DWORD_PTR dwData = 0,
		LPCTSTR lpszEditMask = NULL, LPCTSTR lpszEditTemplate = NULL,
		LPCTSTR lpszValidChars = NULL);
public:
	void	SetVisible				(BOOL bVisible);
	void	SetEnable				(BOOL bEnable, BOOL bIncludeChildren = TRUE);
	void	SetColoredState			(State eState);
	BOOL	SelectOption			(int index, BOOL bSetModifiedFlag = TRUE);
	
	void	SetOriginalDescription	();
	void	SetNewDescription		(CString strNewDescription, BOOL bAppendOriginalDescr=TRUE);
	CString GetOriginalDescription	()															{ return m_strOriginalDescr; }

	CRS_PropertyGrid::Img	 m_eImg;
	void					SetStateImg		(CRS_PropertyGrid::Img eImg)						{ m_eImg = eImg; m_bHasState = TRUE; }
	void					RemoveStateImg	()													{ m_eImg = CRS_PropertyGrid::Img::NoImg; m_bHasState = FALSE;}

	void	OnDrawStateIndicator(CDC* pDC, CRect rect);

	void	SetGroup(BOOL bIsGroup);
	void	SetOwnerList(CBCGPPropList* pWndList) { ASSERT_VALID(pWndList); __super::SetOwnerList(pWndList); }
	BOOL	AddSubItem(CBCGPProp* pSubProp, CBCGPPropList* pWndList = NULL);

	int GetOptionDataIndex(DWORD_PTR) const;
	BOOL				InsertOption	(int index, LPCTSTR lpszOption, BOOL bInsertUnique/* = TRUE*/, DWORD_PTR dwData/* = 0*/);
	BOOL				CheckPropValue(BOOL bAllowEmpty, CString &errMsg);
	CRS_PropertyGrid*	GetPropertyGrid();
	void				UpdateMmProp	();
	virtual void		UpdateIntValue	(int previousValue) {};

protected:
	CWoormDocMng*			GetDocument			();
	CRS_ObjectPropertyView*	GetPropertyView		();
	CNodeTree*				GetCurrentNode		();

	virtual void			SetFilter			(const CString& strFilter);
	BOOL					AddMMProp			(int minValueInPixel=1, int maxValueInPixel=300);

private:
	CString		m_strOriginalDescr;
	TDisposablePtr<CRSMMProp>	m_pMmProp;

public:
	TDisposablePtr<CrsProp>	m_pChildProp;

	void AllowEdit(BOOL bAllow = TRUE);

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const { ASSERT_VALID(this); AFX_DUMP0(dc, " CrsProp... "); CObject::Dump(dc); }
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};

//=============================================================================
//Classe derivata da BCG per implementare il metodo OnClick sul pulsante che fa apparire la dialog per settare il font
class CRSSetFontDlgProp : public CrsProp
{
public:
	enum PropertyType { TextRectDescription, FieldRectValue, FieldRectLabel, FileRectText, 
		TableTitle, ColumnTitle, ColumnSubTotal, TableCellValue, AllColumnTitles, AllSubTotals, AllColumnsBody, TableSubTitle
	};

	CRSSetFontDlgProp(
		CObject*, const CString& strName, PropertyType propType);

protected:
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	CObject*		m_pOwner;
	PropertyType	m_pPropertyType;

private:
	CString GetFontName(FontIdx fontIdx);
	FontIdx GetFontIdx(CString fontName);
	void UpdatePropertyValue();
};

//=============================================================================
//Classe derivata da BCG per implementare la  property che apre un broser per cercare un file nel filesystem
class CRSImgOriginProp : public CrsProp
{
public:
	enum PropertyType{ XP, YP ,XM, YM};

	CRSImgOriginProp(CWoormDocMng*, PropertyType propType);

protected:
	BOOL	HasButton() const	{ return TRUE; }
	void	OnDrawButton(CDC* pDC, CRect rectButton);
	void	OnClickButton(CPoint point);
	void	UpdatePropertyValue();
	BOOL	OnUpdateValue();

protected:
	CWoormDocMng*		m_pWrmDocMng;
	PropertyType		m_propType;
	CBCGPToolBarImages	m_image; 
};

//=============================================================================
//Classe derivata da BCG per implementare la  property che apre un browser per cercare un file nel filesystem
//TODO andrea: vedere se unificare con CRSValueFileProp
class CRSFileNameProp : public CBCGPFileProp
{
public:
	enum Filter{ None, Img, Txt, WoormTemplate, OnlyOpenFileLocation };

	CRSFileNameProp(CObject*, const CString& strName, CString* strFileName, const CString& description, Filter filter = Filter::None);
	CRSFileNameProp(CObject*, const CString& strName, const CString& strFileName, const CString& description, Filter filter = Filter::None);

protected:
	void				OnClickButton	(CPoint point);
	virtual void		UpdateValue		();
	BOOL				OnEndEdit		();
	CRS_PropertyGrid*	GetPropertyGrid	();
	void				OnDrawButton	(CDC* pDC, CRect rectButton);

private: 
	void		InitProperty(Filter filter);
	void		CheckIfIsAccessibleNameSpace();

protected:
	CObject*			m_pOwner;
	CString*			m_pStrFileName;
	Filter				m_eFilter;
	CBCGPToolBarImages	m_image;
};


//=============================================================================
//Classe derivata da BCG per implementare la  property che apre un browser per cercare un file nel filesystem
//TODO andrea: vedere se unificare con CRSValueFileProp
class CRSFileExplorerProp : public CrsProp
{
public:
	CRSFileExplorerProp(const CString& strName, CString& strDirName, const CString& description);

protected:
	BOOL				HasButton() const { return TRUE; }
	void				OnClickButton(CPoint point);
	void				OnDrawButton(CDC* pDC, CRect rectButton);

protected:
	CBCGPToolBarImages	m_image;
};

//=============================================================================
//Classe derivata da BCG per implementare il metodo OnClick per cercare un file e allo stesso tempo, vedere il nome del file nella property
//progetto per il 6/10/15: avere due comandi: tb explorer e browse nel file system
class CRSSearchTbDialogProp : public CRSFileNameProp
{
	friend class CRSFieldTypeProp;

public:

	enum PropertyType{ FileRectFileName, GraphRectImgName, GenericImgName, FieldValueImgName, FieldValueFileName, GenericTextFileName };

	CRSSearchTbDialogProp(CObject* pObj, CString* pStrFileName, PropertyType propType, CRSFileNameProp::Filter filter, CRS_ObjectPropertyView* propertyView);
	CRSSearchTbDialogProp(CObject* pObj, Value* pValue, PropertyType propType, CRSFileNameProp::Filter filter, CRS_ObjectPropertyView* propertyView);

protected:
	void		AdjustButtonRect(); //serve per visualizzare correttamente i due pulsanti. todo andrea: fare classe specifica da ereditare?
	void		OnDrawButton(CDC* pDC, CRect rectButton);
	BOOL		HasButton() const	{ return TRUE; }
	void		OnClickButton(CPoint point);
	void		UpdatePropertyValue();

private:
	void		OnLeftButtonClick();	//Open TbExplorer
	void		InitProperty(PropertyType propType);

private:
	CRSFileNameProp::Filter m_eFilter;
	PropertyType			m_propType;
	CRS_ObjectPropertyView*	m_pPropertyView;
};


//=============================================================================
//Classe derivata da BCG per implementare il metodo OnClick per cercare un file e allo stesso tempo, vedere il nome del file nella property
//progetto per il 6/10/15: avere due comandi: tb explorer e browse nel file system
class CRSLayoutTemplateProp : public CRSFileNameProp
{
public:

	CRSLayoutTemplateProp(CObject* pObj, CString* strFileName, CRS_ObjectPropertyView* propertyView, CRSImportStaticObjects* pImportStaticObjectsProp);
	~CRSLayoutTemplateProp();

protected:
	void			AdjustButtonRect(); //serve per visualizzare correttamente i due pulsanti. todo andrea: fare classe specifica da ereditare?
	void			OnDrawButton(CDC* pDC, CRect rectButton);
	BOOL			HasButton() const	{ return TRUE; }
	void			OnClickButton(CPoint point);
	void			UpdatePropertyValue();
	virtual void	UpdateValue();

private:
	void		AddTemplates();	//fill dropdown
	void		AddTemplatesFromModule(LPCTSTR sNs);
	void		OnSelectCombo();
	void		SetTemplate(CString strPathName);

private:
	CBCGPToolBarImages		m_image;
	CRS_ObjectPropertyView*	m_pPropertyView;
	CStringArray			m_arStrFullNameTmpl;
	CRSImportStaticObjects*	m_pImportStaticObjects;
};

//=============================================================================
//Classe derivata per decidere se importare oggetti statici di un template
class CRSImportStaticObjects : public CrsProp
{
public:

	CRSImportStaticObjects(CRS_ObjectPropertyView* propertyView);

public: 
	void SetAsFalse();

private:
	void OnSelectCombo();

private:
	CRS_ObjectPropertyView*	m_pPropertyView;
};


//=============================================================================
//todo: vedere se potrebbe convenire estendere CRSStringProp e unificare con le altre property con image state, dialog e altre opzioni associate
//Classe derivata da BCG per implementare il metodo OnClick per editare la description di un campo TextRect
class TB_EXPORT CRSEditDescriptionText : public CrsProp
{
public:
	CRSEditDescriptionText(
		const CString& strName, TextRect* textRect, CRS_ObjectPropertyView* propertyView);
	CRSEditDescriptionText();
	~CRSEditDescriptionText();

protected:
	BOOL HasButton() const	{ return TRUE; }
	BOOL OnUpdateValue();
	void OnClickButton(CPoint point);

private:
	void UpdateText(CString newValue);

private:
	TextRect*				m_textRect = NULL;
	CRS_ObjectPropertyView*	m_pPropertyView;

};

//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSBoolProp : public CrsProp
{
public:
	CRSBoolProp(CObject*, const CString& strName, BOOL* value, const CString& description=L"");
	CRSBoolProp(MultiColumnSelection* pColumns, const CString& strName, BOOL* value, const CString& description = L"");
	BOOL*		m_pBValue;
protected:
	
	virtual BOOL		OnUpdateValue();
	CObject*				m_pOwner;

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
};

//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSBoolPropWithDepListToDisable : public CRSBoolProp
{
public:
	CRSBoolPropWithDepListToDisable(CObject*, const CString& strName, BOOL* pValue, CList<CBCGPProp*>* pDepList, const CString& description = L"", BOOL bInvertBehaviour = FALSE);
	~CRSBoolPropWithDepListToDisable();
protected:
	BOOL				OnUpdateValue();
private:
	void				SetDepPropsVisibile();
private:
	CList<CBCGPProp*>*	m_pDepList;
	BOOL				m_bInvertBehaviour;
};


//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSBoolPropWithLayoutRefresh : public CRSBoolProp
{
public:
	CRSBoolPropWithLayoutRefresh(CObject*, const CString& strName, BOOL* pValue, const CString& description = L"");
	CRSBoolPropWithLayoutRefresh(MultiColumnSelection* pColumns, const CString& strName, BOOL* pValue, const CString& description = L"");

protected:
	BOOL		OnUpdateValue();

};

//=============================================================================
//Classe che aggiorna il flag per distribuire i campi di tipo stringa o testo su più righe e insieme controlla il valore dell'alignment per segnalare se ci sono incompatibilità
class CRSMultilineColumnProp : public CrsProp
{
public:
	CRSMultilineColumnProp(TableColumn* pCol, BOOL bValue, CRSColumnAlignBitwiseProp* pAlignment, CRSColumnTypeProp* pType);

protected:
	BOOL	OnUpdateValue();
	void	UpdatePropertyValue();

private:
	CRSColumnAlignBitwiseProp*					m_pAlignment;
	CRSColumnTypeProp*							m_pType;
	TableColumn*								m_pCol;
};

//=============================================================================
//Classe che aggiorna il flag per distribuire i campi di tipo stringa o testo su più righe e insieme controlla il valore dell'alignment per segnalare se ci sono incompatibilità
//E' stato scelto di creare una classe distinta da quella similare ma per la colonna singola, in modo da poter settare stringa vuota quando le colonne hanno settato valori diversi per la proprietà.
class CRSMultilineMultiColumnsProp : public CrsProp
{
public:
	CRSMultilineMultiColumnsProp(MultiColumnSelection* pMulCols, CRSMultiColumnsAlignmentStyleBitWiseProp* pAlignment);

protected:
	BOOL	OnUpdateValue();
	void	UpdatePropertyValue();

private:
	TDisposablePtr<CRSMultiColumnsAlignmentStyleBitWiseProp>	m_pMulAlignment;
	TDisposablePtr<MultiColumnSelection>		m_pMulCols;
};

//=============================================================================
//Classe derivata da BCG per aggiornare la visibilità dei totali e mostrare alcune subproperty
class CRSShowColumnTotalProp : public CRSBoolProp
{
public:
	CRSShowColumnTotalProp(TableColumn* pCol, CRS_ObjectPropertyView* propertyView);

private:
	TableColumn*		m_pCol;
	CRS_ObjectPropertyView*			m_pPropertyView;

protected:
	BOOL OnUpdateValue();
	void DrawProperties(int index);
};

//=============================================================================
//Classe derivata da BCG per aggiornare i valori inter della property e successivamente il documento di Woorm
class CRSIntProp : public CrsProp
{
	friend class CRSShowColumnTotalProp;

public:
	enum CooType { XP, YP, XM, YM, DEFAULT};
	CRSIntProp(CObject*, const CString& strName, int* value, const CString& description, int fromValue = 0, int toValue = 1000, CooType type = DEFAULT);
	
protected:
	virtual BOOL OnUpdateValue();

	int*			m_pValue = NULL;

private:	
	CObject*		m_pOwner = NULL;
	CooType			type;
};

//=============================================================================
class CRSRepeaterCount : public CRSIntProp
{
public:
	enum PropertyType {Row, Col};

	CRSRepeaterCount(Repeater* pRepeater, int* pCount, PropertyType ePropType);

protected:
	virtual BOOL OnUpdateValue();

private:
	Repeater*		m_pRepeater;
	PropertyType	m_ePropType;
};


//=============================================================================
//Classe derivata da BCG per aggiornare i valori inter della property e successivamente il documento di Woorm
class CRSShortProp : public CrsProp
{
	friend class CRSShowColumnTotalProp;

public:
	CRSShortProp(CObject*, const CString& strName, short* value, const CString& description, int fromValue = 0, int toValue = 1000);

protected:
	BOOL OnUpdateValue();

private:
	short*			m_pValue;
	CObject*		m_pOwner;
};
//=============================================================================
//Classe derivata da BCG per aggiornare i valori double della property e successivamente il documento di Woorm
class CRSDoubleProp : public CrsProp
{
	friend class CRSShowColumnTotalProp;

public:
	CRSDoubleProp(CObject*, const CString& strName, double* value, const CString& description, int fromValue = 0, int toValue = 1000);

protected:
	BOOL OnUpdateValue();

protected:
	double*			m_pValue;
	CObject*		m_pOwner;
};

//=============================================================================
//Classe derivata dalla ..int per aggiornare la dimensione dell'ombra di una table
class CRSTableShadowProp : public CrsProp
{
public:
	CRSTableShadowProp(Table* pTable);

protected:
	BOOL OnUpdateValue();

private:
	int*	m_pValue;
	Table*	m_pTable;
};

//=============================================================================
//Classe derivata da BCG per aggiornare i colori di una property e successivamente il documento di Woorm
class CRSColorProp : public CBCGPColorProp
{
	friend class CRSShowColumnTotalProp;
	friend class CrsProp;
	friend class CRSBoolPropWithDepListToDisable;

public:
	CRSColorProp(CObject*, const CString& strName, COLORREF* value, const CString& description);
	CRSColorProp(CObject*, const CString& strName, COLORREF value, const CString& description);
	CRSColorProp(MultiColumnSelection*, const CString& strName, COLORREF value, const CString& description);

protected:
	virtual BOOL			OnUpdateValue();
	void					SetEnable(BOOL bEnable, BOOL bIncludeChildren = TRUE);
	CRS_PropertyGrid*		GetPropertyGrid();
	CRS_ObjectPropertyView*	GetPropertyView();
	void					SetOriginalDescription();
	CString					GetOriginalDescription() { return m_strOriginalDescr; }

protected:
	COLORREF*				m_pValue;
	CObject*				m_pOwner;
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
private:
	CString					m_strOriginalDescr;
};

//=============================================================================
//todo andrea: vedere se unificare con la CRSHiddenProp
//Classe derivata da BCG per ottenere una property con dropdown, immagine e pulsante di apertura dialog per un campo di tipo colore
class CRSColorWithExprProp : public CRSColorProp
{
	friend class CrsProp;
public:
	CRSColorWithExprProp(CObject*, const CString& strName, COLORREF* value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView);
	CRSColorWithExprProp(CObject*, const CString& strName, COLORREF value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView);
	CRSColorWithExprProp(MultiColumnSelection*, const CString& strName, COLORREF value, Expression** ppExp, const CString& description, CRS_ObjectPropertyView* propertyView);
	~CRSColorWithExprProp();
protected:
	void			AdjustButtonRect	();
	BOOL			HasButton			() const					{ return TRUE; }
	virtual void	OnClickButton		(CPoint point);
	virtual void	OnRightButtonClick	();
	virtual void	OnDrawButton		(CDC* pDC, CRect rectButton);
	virtual void	OnDrawStateIndicator(CDC* pDC, CRect rect);
	virtual BOOL	HasExpression		();
	void			SetVisible			(BOOL visible);

protected:
	//expression Props
	Expression**	m_ppExp;
	CRS_ObjectPropertyView* m_pPropertyView;
	//original description
	CString m_strDescr;
	//img state props
	CBCGPToolBarImages	m_imageExpr;

private:
	void InitProp();
};


//=============================================================================
//Classe derivata da BCG per aggiornare i colori di una property e successivamente il documento di Woorm
class CRSTableColumnColorWithExprProp : public CRSColorWithExprProp
{
public:
	enum PropertyType{
		BackColor, ForeColor
	};

public:
	CRSTableColumnColorWithExprProp(TableColumn* pCol, PropertyType propType, Expression** ppExp, CRS_ObjectPropertyView* propertyView);

protected:
	BOOL OnUpdateValue();

protected:

	TableColumn*	m_pCol;
	PropertyType	m_propType;
};

//=============================================================================
//Class extending property for page (paper options, margins etc)
class TB_EXPORT CRSPageProp : public CrsProp
{
public:
	enum PageInfoType{
		REPORT_STYLES, REPORT_WIDTH, REPORT_LENGTH, PRINTER_STYLES, PRINTER_WIDTH, PRINTER_LENGTH, PAGE_ORIENT, NUMBER_COPIES, COLLATE_COPIES, PRINTABLE_AREA, MARGIN_LEFT, MARGIN_RIGHT, MARGIN_TOP, MARGIN_BOTTOM,
		PRINTER
	};

	CRSPageProp(PageInfo* pageInfo, CString name, LPCTSTR value, PageInfoType pageInfoType, CRS_ObjectPropertyView* propertyView);
	CRSPageProp(PageInfo* pageInfo, CString name, variant_t value, PageInfoType pageInfoType, CRS_ObjectPropertyView* propertyView);
	BOOL OnUpdateValue();

private:
	PageInfo* m_pageInfo;
	PageInfoType m_pageInfoType;
	CRS_ObjectPropertyView* m_propertyView;

	void SwapOrientation();
};

//=============================================================================
//Class extending property for page (default printer setting)
class TB_EXPORT CRSPageBoolProp : public CrsProp
{
public:
	BOOL*		m_pBValue;
	CRSPageBoolProp(PageInfo* pageInfo, CString name, BOOL* pValue, CRS_ObjectPropertyView* propertyView);
	BOOL OnUpdateValue();

private:
	PageInfo* m_pageInfo;
	CRS_ObjectPropertyView* m_propertyView;

};

//=============================================================================
//Class extending property for WoormInin =
class CRSIniProp : public CrsProp{
public:
	enum IniLineType{
		TOLERANCE, PITCH_X, PITCH_Y, PERCENTAGE, SHOW_ON_FIELD, LANDSCAPE, PORTRAIT
	};

	CRSIniProp(WoormIni* wrmIni, CString name, LPCTSTR value, IniLineType iniLine);
	CRSIniProp(WoormIni* wrmIni, CString name, variant_t value, IniLineType iniLine);
	BOOL OnUpdateValue();

private:
	WoormIni* m_wrmIni;
	IniLineType m_iniLine;
};

//=============================================================================
//Class extending property for variables
class TB_EXPORT CRSVariableProp : public CrsProp
{
public:
	enum VariableType{
		LENGTH, PRECISION, DO_NOT_EXPORT, EXPORT_ALIAS, FIELD_TYPE, FIELD_IS_INPUT, REINIT_INPUT, FIELD_ISARRAY, FIELD_NAME
	};

	enum ReinitType{
		REINIT_ALWAYS, REINIT_NEVER, REINIT_NORMAL
	};

	CRSVariableProp(WoormField* wrmField, CString name, LPCTSTR value, VariableType varType, BOOL isFieldProp = FALSE, LPCTSTR originDescr = 0);
	CRSVariableProp(WoormField* wrmField, CString name, variant_t value, VariableType varType, BOOL isFieldProp = FALSE, LPCTSTR originDescr = 0);

protected:
	BOOL OnUpdateValue();

private:
	WoormField* m_pWrmField = NULL;
	VariableType  m_pVarType;
	BOOL m_pIsFieldProp;
};

//=============================================================================
//Class For ask dialog properties
class TB_EXPORT CRSAskDialogProp : public CrsProp
{
public:
	enum DialogType{
		NAME, TITLE, POSITION_OF_FIELDS, ONLY_ON_EVENT
	};

	CRSAskDialogProp(AskDialogData* askDialog, CString name, LPCTSTR value, DialogType dType, CRS_ObjectPropertyView* m_pView);
	CRSAskDialogProp(AskDialogData* askDialog, CString name, variant_t value, DialogType dType, CRS_ObjectPropertyView* m_pView);

protected:
	BOOL OnUpdateValue();

private:
	AskDialogData* m_pAskDialog = NULL;
	DialogType  m_pDType;
	CRS_ObjectPropertyView* m_pView;
};

//=============================================================================
//Class for ask field
class CRSAskFieldProp : public CrsProp
{
public:
	CString m_pOldValue;

	enum AskFieldName{
		NAME, TYPE, HOTLINK_GROUP, HOTLINK_NAMESPACE, HOTLINK_NAME, HOTLINK_PARAMETERS, SHOW_HOTLINK_DESCRIPTION, 
		DESCRIPTION_COMBO, MULTI_SELECTION_COMBO, CAPTION_POSITION, CONTROL_STYLE, NEAR_THE_BORDER, BOOL_LEFT_TEXT, RANGE_LIMIT
	};

	CRSAskFieldProp(AskFieldData* askField, CString name, LPCTSTR value, AskFieldName dFieldType, CRS_ObjectPropertyView* propertyView, WoormField* woormField = NULL);
	CRSAskFieldProp(AskFieldData* askField, CString name, variant_t value, AskFieldName dFieldType, CRS_ObjectPropertyView* propertyView,  WoormField* woormField = NULL);

protected:
	BOOL OnUpdateValue();

	void HotLinkGroupChanged(CString value, AddOnModule* pMod);
	void HotLinkNameChanged(CString value, CHotlinkDescription* pHklDescr);

private:
	AskFieldData*	m_askField;
	AskFieldName	m_dFieldType;
	WoormField*		m_dWoormField;
	CRS_ObjectPropertyView* m_pPropertyView;
};

//=============================================================================
//Class for group field
class CRSGroupFieldProp : public CrsProp
{
public:
	enum AskGroupType{
		CAPTION, CAPTION_POSITION
	};

	CRSGroupFieldProp(AskGroupData* groupField, CString name, LPCTSTR value, AskGroupType dGroupType);
	CRSGroupFieldProp(AskGroupData* groupField, CString name, variant_t value, AskGroupType dGroupType);

protected:
	BOOL OnUpdateValue();

private:
	AskGroupData*	m_askGroupfield;
	AskGroupType	m_pGroupType;
};

//=============================================================================
class TB_EXPORT CRSCommonProp : public CrsProp
{
	friend class CRS_ObjectPropertyView;
	friend class CRS_PropertyGrid;

public:
	enum PropType {
		NEW_TYPE, NEW_ENUMTYPE, 
		NEW_NAME, 
		SELECT_TABLE_NAME, SELECT_TABLE_MODULE_NAME,
		NEW_HOTLINK_NAME, NEW_HOTLINK_MODULE_NAME,
		NEW_VAR_TYPE, NEW_FIELD_TYPE, NEW_FIELD_ISHIDDEN,
		NEW_COLUMN_TYPE, NEW_COLUMN_ENUM,
		EXISTING_RULE_OR_NEW_RULE, NEW_RULE_MODULE, EXISTING_RULE_TABLES, NEW_RULE_TABLES ,
		FROM_HIDDEN_FIELD, 
		NEW_HYPERLINK, NEW_HYPERLINK_VAR_FLAG, COLUMN_BLOCK_NAME 
	};

	CRSCommonProp(CString name, LPCTSTR value, CRSCommonProp::PropType eType, CRS_ObjectPropertyView* propertyView, LPCTSTR originDescription = 0);
	CRSCommonProp(CString name, variant_t value, CRSCommonProp::PropType eType, CRS_ObjectPropertyView* propertyView, LPCTSTR originDescription = 0);

protected:
	BOOL OnUpdateValue();

private:
	PropType				m_eType;

	CRS_ObjectPropertyView*	m_pPropertyView;
};

//=============================================================================
class CRSTblRuleProp : public CrsProp
{
public:
	CString m_pOldValue;

	enum PropType 
	{
		JOIN_TYPE, TABLE_NAME,
		SELECT_CONSTRAINT, DISTINCT, TOP
	};

	CRSTblRuleProp(TblRuleData* pRule, CString name, LPCTSTR value, CRSTblRuleProp::PropType eType, CRS_ObjectPropertyView* propertyView);
	CRSTblRuleProp(TblRuleData* pRule, CString name, variant_t value, CRSTblRuleProp::PropType eType, CRS_ObjectPropertyView* propertyView);

protected:
	BOOL OnUpdateValue();

private:
	TblRuleData*	m_pTblRule = NULL;

	PropType				m_eType;
	CRS_ObjectPropertyView*	m_pPropertyView = NULL;
};

//------------------------------------------------------------------------------
class CRSNamedQueryRuleProp : public CrsProp
{
public:
	CString m_pOldValue;

	CRSNamedQueryRuleProp(QueryRuleData* pRule, CString name, LPCTSTR value, CRS_ObjectPropertyView* propertyView);
	CRSNamedQueryRuleProp(QueryRuleData* pRule, CString name, variant_t value, CRS_ObjectPropertyView* propertyView);

protected:
	BOOL OnUpdateValue();

private:
	QueryRuleData*	m_pRule = NULL;

	CRS_ObjectPropertyView*	m_pPropertyView = NULL;
};

//=============================================================================
class CRSTriggerEventProp : public CrsProp
{
public:
	CString m_sOldValue;

	enum PropType
	{
		EVENT_NAME,
		MustTrueTogether,
		NewBreakingField, 
		NewSubTotalField
	};

	CRSTriggerEventProp(TriggEventData* m_pEvent, CString name, LPCTSTR value, CRSTriggerEventProp::PropType eType, CRS_ObjectPropertyView* pPropView);
	CRSTriggerEventProp(TriggEventData* m_pEvent, CString name, variant_t value, CRSTriggerEventProp::PropType eType, CRS_ObjectPropertyView* pPropView);

protected:
	BOOL OnUpdateValue();

	void LoadUsableBreakingFields(CRS_ObjectPropertyView* pPropView);
	void LoadUsableSubTotalFields(CRS_ObjectPropertyView* pPropView);

private:
	TriggEventData*	m_pEvent;

	PropType				m_eType;
};

//=============================================================================
//Class for rule expression with parsing
class TB_EXPORT CRSRuleExpressionProp : public CrsProp
{
public:
	CRSRuleExpressionProp(const CString& strName, ExpRuleData* expRuleData, DataType dataType, const CString& description = L"");

protected:
	virtual BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	ExpRuleData*			m_expRuleData;

protected:
	DataType		m_dataType;
};

//=============================================================================
//Class for expression with block
class CRSBlockExpressionProp : public CrsProp
{
public:

	CRSBlockExpressionProp(
		const CString& strName, Block** block, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"");

protected:
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	Block**	m_block;
	SymTable*		m_pSymTable;
	CRS_ObjectPropertyView* m_pPropertyView;
};

//=============================================================================
//Class for expression
class CRSExpressionProp : public CrsProp
{
	friend class CRSAskFieldProp;
public:
	enum InitialValue {StringValue, IntValue};

	CRSExpressionProp(
		const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"", BOOL bAllowEmpty = TRUE, BOOL bEditInPlace = FALSE);
	CRSExpressionProp(
		InitialValue eInitialValue, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"", BOOL bEditInPlace = FALSE);
	CRSExpressionProp(
		const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CWoormDocMng* pWDoc, const CString& description = L"", BOOL bAllowEmpty = TRUE, BOOL bEditInPlace = FALSE);

protected:
	virtual BOOL	HasButton			() const		{ return TRUE; }
	virtual void	OnClickButton		(CPoint point);
	virtual void	UpdateDocument		()				{/*DO NOTHING*/ };
	virtual void	UpdatePropertyValue	();
	virtual BOOL	OnEndEdit			();
	
protected:
	Expression**	m_ppExp = NULL;
	DataType		m_dataType;
	SymTable*		m_psymTable = NULL;
	BOOL			m_bAllowEmpty;
	BOOL			m_bEditInPlace = FALSE;
	
	CRS_ObjectPropertyView*	m_pPropertyView = NULL;
	CWoormDocMng*		m_pWDoc = NULL;

public:
	BOOL			m_bViewMode;

	CRS_ObjectPropertyView* GetPropertyView() { return dynamic_cast<CRS_ObjectPropertyView*>(m_pPropertyView); }
};

//=============================================================================
//Class for expression calculated column
class CRSSqlExpressionProp : public CrsProp
{
public:
	CRSSqlExpressionProp(WoormField* pWoormField, TblRuleData* pTblRule, DataFieldLink*	pObjLink, CRS_ObjectPropertyView* pPropertyView, CNodeTree* pNode);

protected:
	virtual BOOL	HasButton			() const { return TRUE; }
	virtual void	OnClickButton		(CPoint point);
	virtual void	UpdatePropertyValue	();

protected:
	CRS_ObjectPropertyView* m_pPropertyView;
	WoormField*				m_pWoormField;
	TblRuleData*			m_pTblRule;
	DataFieldLink*			m_pObjLink;
	CNodeTree*				m_pTreeNode;
};

//=============================================================================
//Class for expression with object-layout redraw
class CRSExpressionExtendedProp : public CRSExpressionProp
{
public:
	CRSExpressionExtendedProp(
		CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"", BOOL bEditInPlace = FALSE);
	CRSExpressionExtendedProp(
		InitialValue eInitialValue, CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"", BOOL bEditInPlace = FALSE);
	~CRSExpressionExtendedProp();
protected:
	virtual void UpdateDocument();

protected:
	CObject*	m_pOwner;
};

//=============================================================================
//Class for column width (int type with spin control) and expression
class CRSColumnWidthWithExprProp : public CRSExpressionExtendedProp
{
public:
	enum WidthCoor {WidthP, WidthM, DEFAULT};
	CRSColumnWidthWithExprProp(
		TableColumn* pCol, const CString& strName, int width, Expression** ppExp, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"", InitialValue eInitialValue = CRSExpressionProp::InitialValue::IntValue, WidthCoor type = DEFAULT, BOOL hasButton=TRUE);
protected:
	virtual void	UpdatePropertyValue		()						{/*DO NOTHING*/}
	virtual void 	OnDrawStateIndicator	(CDC* pDC, CRect rect);
	virtual BOOL	OnUpdateValue			();
	virtual void	UpdateDocument			();
	virtual BOOL	HasButton() const { return hasButton; }
private:
	Table*			m_pTable;
	TableColumn*	m_pCol;
	WidthCoor type;
	BOOL hasButton;
};

//=============================================================================
//todo andrea: vedere se generalizzare
//Classe derivata da BCG per ottenere una property con dropdown, immagine e pulsante di apertura dialog per un campo di tipo booleano
class CRSHiddenProp : public CRSExpressionExtendedProp
{
public:
	CRSHiddenProp(CObject* pOwner, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, CRS_ObjectPropertyView* pPropertyView, const CString& description = L"");

protected:
	void			AdjustButtonRect();
	BOOL			HasButton() const { return TRUE; }
	void			OnClickButton(CPoint point);
	void			OnDrawButton(CDC* pDC, CRect rectButton);
	void			OnDrawStateIndicator(CDC* pDC, CRect rect);
	virtual void	UpdatePropertyValue();
	virtual void	UpdateDocument();
	virtual BOOL	OnUpdateValue();

};


//=============================================================================
//Classe derivata da BCG per aggiornare i campi di tipo testo di una property e successivamente il documento di Woorm
class CRSStringProp : public CrsProp
{
public:
	CRSStringProp(CObject*, const CString& strName, CString* value, const CString& description);

protected:
	BOOL OnUpdateValue();

protected:
	CString*		m_pValue;
	CObject*		m_pOwner;
};

//=============================================================================
//todo andrea: vedere se unificare con la CRSHiddenProp e la CRSColorWithExprProp (fattorizzando la gestione dell'immagine e del click in due punti diversi)
//Classe derivata da BCG per ottenere una property di tipo testo a cui associare un espressione
class CRSStringWithExprProp : public CRSStringProp
{
public:
	CRSStringWithExprProp(CObject*, const CString& strName, CString* value, Expression** ppExp, SymTable* pSymTable, const CString& description, CRS_ObjectPropertyView* propertyView, BOOL bUpdateNodeTree = FALSE);

public:
	void ValorizePropertyIfNecessary();

protected:
	void AdjustButtonRect();
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);
	void OnDrawButton(CDC* pDC, CRect rectButton);
	void OnDrawStateIndicator(CDC* pDC, CRect rect);
	virtual BOOL OnUpdateValue();

private:
	void UpdateRect();

private:
	CRS_ObjectPropertyView* m_pPropertyView;

	//expression Props
	Expression**		m_ppExp;
	SymTable*			m_psymTable;

	BOOL				m_bUpdateNodeTree;

	BOOL				m_bImgVisible;
	CBCGPToolBarImages	m_imageExpr;
};

//=============================================================================
//todo andrea: vedere se unificare con la CRSHiddenProp ,CRSColorWithExprProp e CRSStringWithExprProp(fattorizzando la gestione dell'immagine e del click in due punti diversi)
//Classe derivata da BCG per ottenere una property a cui associare una dialog e una espressione
class CRSDialogWithExprProp : public CrsProp
{
public:
	enum PropertyType {
		FieldValueFormat,
		TableColumnBodyFormat,
		TableColumnBodyFont
	};

	CRSDialogWithExprProp(CObject*, const CString& strName, Expression** ppExp, DataType dataType, SymTable* pSymTable, PropertyType propType, const CString& description, CRS_ObjectPropertyView* propertyView);

protected:
	void AdjustButtonRect();
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);
	void OnDrawButton(CDC* pDC, CRect rectButton);
	void OnDrawStateIndicator(CDC* pDC, CRect rect);

private:
	CObject*				m_pOwner;
	Expression**			m_ppExp;
	DataType				m_dataType;
	SymTable*				m_psymTable;
	PropertyType			m_propType;
	CRS_ObjectPropertyView*	m_pPropertyView;
	CBCGPToolBarImages		m_imageExpr;

private:
	void UpdatePropertyValue();
	void OnLeftClick();
	void OnRightClick();

	CString GetFormatName(FormatIdx formatIdx);
	CString GetFontName(FontIdx fontIdx);
};

//=============================================================================
//Classe derivata da BCG per aggiornare Location e Size di un Rect
class CRSRectProp : public CrsProp
{
public:
	enum PropertyType{ LocationXP, LocationYP, LocationXM, LocationYM, WidthP, HeightP, WidthM, HeightM
	};

	CRSRectProp(CObject*, const CString& strName, const CString& description, PropertyType propType, int fromValue = 0, int toValue = 2000);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

	void UpdateObjectValue(LONG previousValue = 0);
	void UpdateLocationX(LONG previousValue, LONG currValue);
	void UpdateLocationY(LONG previousValue, LONG currValue);
	void UpdateWidth(LONG currValue);
	void UpdateHeight(LONG currValue);

private:
	
	CObject*		m_pOwner;
	PropertyType	m_propType;
	
};

//=============================================================================
//Classe derivata da BCG per mostare e nascondere alcune properties in base al tipo del FieldRect
class CRSFieldTypeProp : public CrsProp
{
	friend class CRSBarCodeProp;
	//friend class CRSBarCodeFieldComboProp;
public:
	CRSFieldTypeProp(FieldRect*, const CString& strName, const CString& description, CRS_ObjectPropertyView* PropertyView, CBCGPProp* parentGroup, CList<CrsProp*>* dependentPropList);
	~CRSFieldTypeProp();
	//properties not dependant but to be drawn after
	CList<CrsProp*>* m_pPropToRedraw;
	void DrawProperties();
	
protected:
	BOOL OnUpdateValue();

private:
	CRS_ObjectPropertyView*	m_pPropertyView;
	FieldRect*				m_pFieldRect;

	//rimozione properties inutili in base al tipo di fieldrect - Example: if barcode type -> remove label group
	CBCGPProp*				m_pParentProp;
	CList<CrsProp*>*		m_lstDependentProp;
	//property currently drawn 
	CrsProp*	m_pShowAsProp;

private:
	void DrawProperties(int index);
	void SetDepPropsVisibile(BOOL visible);
};

//=============================================================================
//Classe derivata da BCG per mostare e nascondere alcune properties in base al tipo del body della colonna
class CRSColumnTypeProp : public CrsProp
{
public:
	CRSColumnTypeProp(TableColumn*, CRS_ObjectPropertyView* PropertyView);
	~CRSColumnTypeProp();
	void DrawProperties();

protected:
	BOOL OnUpdateValue();

private:
	CRS_ObjectPropertyView*	m_pPropertyView;
	TableColumn*			m_pCol;
	//property currently drawn 
	CrsProp*	m_pShowAsProp;

private:
	void DrawProperties(int index);
};

//=============================================================================
//Classe derivata da BCG per selezionare se un FieldRect deve essere o meno un EmailParameter e in tal caso, quale
class CRSEmailParameterProp : public CrsProp
{
public:
	CRSEmailParameterProp(const CString& strName, FieldRect* pFieldRect, const CString& description, CRS_PropertyGrid* pPropGrid);

protected:
	BOOL OnUpdateValue();

private:
	FieldRect* m_pFieldRect;
	void UpdatePropertyValue();
};

class CRSAnchorToColumnProp;
//=============================================================================
class CRSAnchorToProp : public CrsProp
{
	friend class CRSAnchorToColumnProp;

public:
	CRSAnchorToProp(BaseRect* pBaseRect, CRS_PropertyGrid* propGrid);
	CRSAnchorToProp(SelectionRect* pMulSel, CRS_PropertyGrid* propGrid);
	virtual ~CRSAnchorToProp();

protected:
	BOOL OnUpdateValue		();
private:
	void Initialize			();
	void InitializeSubItem	();
	void AddTables			();
	void AddColumns			(Table* pTable);
	void AddColumnsToSubItem(Table* pTable, CRSAnchorToColumnProp* pSubItem, WORD fromID);
	void SelectCurrentTable	();
	void SetLeftColumn		(WORD leftId);
	void SetRightColumn		(WORD rightId);
	void Anchor				();

private:
	BaseRect*				m_pBaseRect;
	TDisposablePtr<SelectionRect>			m_pMulSel;
	CList<Table*>*			m_pTables;

	CRSAnchorToColumnProp* m_pLeftColumn;
	WORD				   m_leftId;
	CRSAnchorToColumnProp* m_pRightColumn;
	WORD				   m_rightId;
};

//=============================================================================
class CRSAnchorToColumnProp : public CrsProp
{
public:
	enum Side{Left, Right};
	CRSAnchorToColumnProp(CRSAnchorToProp* pParent, Side eSide);
	
protected:
	BOOL OnUpdateValue();

private:
	CRSAnchorToProp* m_pParent;
	Side m_eSide;
};

//=============================================================================
//Classe derivata da BCG per implementare properties con expression 
/*
class CRSDialogProp : public CrsProp
{
public:
	//--------------------------------------------------------------------------------------------------------------------*************
	//TODO ANDREA: probabilmente sono da rimuovere tutti gli enumerativi, con la loro gestione, riguardanti l'allineamento!!!!!!!!!!!!
	//--------------------------------------------------------------------------------------------------------------------*************
	enum PropertyType{
		//BaseRect properties
			BaseRectAnchorToColumn,								//Anchor to Column
			
		//FieldRect properties
			FielRectValueAlign,									//Value
			FieldRectLabelAlign,								//Label
			ResizeKeepingAspectRatio, CutImage, SetOriginalSize,//Bitmap

		//TextRect properties
			TextRectAlign,

		//Table
			TableTitleAlign,									//title
			AllColumnTitlesAlign,
			
		//Column
			ColumnTitleAlign,									//Title
			ColumnBodyAlign,									//Body
			SubtotalAlign,										//SubTotal

		//TableCell
			TableCellValueAlign
	};

	CRSDialogProp(CObject*, const CString& strName, const _variant_t defValue, PropertyType propType, const CString& description = L"");

protected:
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	CObject*		m_pOwner;
	PropertyType	m_pPropertyType;
private:
	void UpdatePropertyValue();
};*/

//=============================================================================
//Classe derivata da BCG per implementare la  property a cui viene associato il value di un field rect
class CRSValueProp : public CrsProp
{
public:
	CRSValueProp(CObject*, const CString& strName, Value* value, const CString& description);

protected:
	BOOL OnUpdateValue();

protected:
	Value*		m_pValue;
	CObject*	m_pOwner;
};

//=============================================================================
//Classe derivata da BCG per implementare la  property a cui viene associato il value di un field rec
//todo andrea->da rimuovere perchè non gestita in woorm
class CRSImageFitProp : public CrsProp
{
public:
	CRSImageFitProp(CObject*, const CString& strName, CTBPicture::ImageFitMode* value, const CString& description);

protected:
	BOOL OnUpdateValue();

protected:
	CTBPicture::ImageFitMode*	m_pValue;
	CObject*					m_pOwner;

private:
	void UpdatePropertyValue();
};

//=============================================================================
//Classe derivata da BCG per implementare la  property a cui viene associato lo stile di in obj
class CRSStyleProp : public CrsProp
{
public:
	CRSStyleProp(CObject*, CRS_ObjectPropertyView* PropertyView);

protected:
	CObject*					m_pOwner;
	CRS_ObjectPropertyView*		m_pPropertyView;

private:
	void UpdatePropertyValue();
	void AddOptions();
	void OnSelectCombo();
};

class CRSBarCodeProp;
//=============================================================================
//Classe derivata da CrsProp per implementare i metodi per gestire i tipi barcode
class CRSBarCodeGroupProp : public CrsProp
{

public:
	CRSBarCodeGroupProp();
	~CRSBarCodeGroupProp();


protected:
	CObject*				m_pOwner;
public:
	CList<CRSBarCodeProp*>*	m_lstDependentProp;
	void UpdateDependantProp(CRSBarCodeProp* fromProp = NULL);
};

//=============================================================================
//Classe derivata da CrsProp per implementare i metodi per gestire i tipi barcode
class CRSBarCodeProp : public CrsProp
{

public:
	enum PropertyType {
		TypeFromField,
		TextFromField,
		EncodingTypeFromField,
		VersionFromField,
		ErrorCorrectionLevelFromField
	};

public:
	enum PropertySizeType {
		BarWidth_ModuleSize,
		BarHeight,
		RowsNo,
		ColumnsNo
	};

public:
	CRSBarCodeProp(CObject*, const CString& strName, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description);
	CRSBarCodeProp(CObject*, const CString& strName, BOOL* value, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description);
	//CRSBarCodeProp(MultiColumnSelection* pColumns, const CString& strName, BOOL* value, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description = L"");

protected:
	CObject*					m_pOwner;
	CRS_ObjectPropertyView*		m_pPropertyView;
	CBarCode*					m_pBarCode;
	Value*						m_pValue;

protected:
	virtual BOOL OnUpdateValue();

public:
	virtual void UpdatePropertyLayout(BOOL bDefaultValue = TRUE) {};
	virtual void DrawProperties(BOOL bDefaultValue = TRUE) {};
};

//=============================================================================
//Classe derivata implementare la checkbox da cui dipende la scelta per selezionare poi il tipo di barcode
class CRSBarCodeComboProp : public CRSBarCodeProp
{
	friend class CRSFieldTypeProp;
	friend class CRSColumnTypeProp;

private:
	int m_nCurrOption;
public:
	PropertyType	m_propType;

public:
	CRSBarCodeComboProp(CObject*, const CString& strName, CBarCode* pBarCode, const CString& description, CRS_ObjectPropertyView* propertyView, CRSBarCodeProp::PropertyType propTyp);

public:
	virtual void UpdatePropertyLayout(BOOL defaultValue = TRUE);
	virtual void DrawProperties(BOOL bDefaultValue = TRUE);
protected:
	virtual BOOL OnUpdateValue();
};


//=============================================================================
//Classe derivata implementare la combo box con i tipi di bar code riconosciuti
class CRSBarCodeTypeComboProp : public CRSBarCodeProp
{
	friend class CRSBarCodeComboProp;

public:
	CRSBarCodeTypeComboProp(CObject*, CBarCode* pBarCode);

public:
	virtual void UpdatePropertyLayout(BOOL defaultValue = TRUE) {};
	virtual void DrawProperties(BOOL bDefaultValue = TRUE) {};
protected:
	virtual BOOL OnUpdateValue();

};

//=============================================================================
//Classe derivata implementare la combo box con i tipi di encoding type/checksum riconosciuti
class CRSBarCodeEncodingComboProp : public CrsProp
{
	friend class CRSBarCodeComboProp;

public:
	CRSBarCodeEncodingComboProp(CObject*, CBarCode* pBarCode, BOOL bDefaultValue = TRUE);

protected:
	BOOL OnUpdateValue();

protected:
	CBarCode*	m_pBarCode;

private:
	CObject*	m_pOwner;
};

//=============================================================================
//Classe derivata implementare la combo box delle 'version' dei barcode 2D riconosciute
class CRSBarCodeVersionComboProp : public CrsProp
{
	friend class CRSBarCodeComboProp;

public:
	CRSBarCodeVersionComboProp(CObject*, CBarCode* pBarCode, BOOL bDefaultValue = TRUE);

protected:
	BOOL OnUpdateValue();

protected:
	CBarCode*	m_pBarCode;

private:
	CObject*	m_pOwner;
};

//=============================================================================
//Classe derivata implementare la combo box con i tipi di error correction level riconosciuti
class CRSBarCodeErrCorrLevelComboProp : public CrsProp
{
	friend class CRSBarCodeComboProp;

public:
	CRSBarCodeErrCorrLevelComboProp(CObject*, CBarCode* pBarCode, BOOL bDefaultValue = TRUE );

protected:
	BOOL OnUpdateValue();

protected:
	CBarCode*	m_pBarCode;

private:
	CObject*	m_pOwner;
};

//=============================================================================
//Classe derivata implementare la combo box con i campi da cui è possibile ricavare il tipo di bar code 
class CRSBarCodeFieldComboProp : public CrsProp
{
	friend class CRSBarCodeComboProp;
	friend class CRSBarCodeProp;

public:
	CRSBarCodeFieldComboProp(CObject*, CBarCode* pBarCode, SymTable* pSymTable, CRSBarCodeProp::PropertyType propType, BOOL bDefaultValue = TRUE);

protected:
	BOOL OnUpdateValue();

protected:
	CBarCode*		m_pBarCode;
	SymTable*		m_pSymTable;

private:
	CObject*						m_pOwner;
	CRSBarCodeProp::PropertyType	m_propType;
};

//=============================================================================

class CRSChartProp : public CrsProp
{

public:
	CRSChartProp(CObject* pOwner, Chart* pChart);

protected:
	virtual BOOL OnUpdateValue();
	virtual void DrawProperties(WoormField*) {};

protected:
	Chart*		m_pChart;
	CObject*	m_pOwner;

public:
	CArray<CrsProp*> m_arDependandProp;

	
};


//=============================================================================

class CRSChartStringProp : public CRSStringProp
{

public:
	CRSChartStringProp(CObject* ownerObj, const CString& strName, CString* pValue, const CString& description, EnumChartObject eChartObjectType);

private:
	EnumChartObject m_eCharObjType;

protected:
	virtual BOOL OnUpdateValue();
};

//=================================CRSChartDoubleProp================================
class CRSChartDoubleProp : public CRSDoubleProp
{

public:
	CRSChartDoubleProp(CObject* ownerObj, const CString& strName, double* value, const CString& description, EnumChartObject eChartObjectType, int fromValue = 0, int toValue = 1000);

private:
	EnumChartObject m_eCharObjType;

protected:
	virtual BOOL OnUpdateValue();
};

//=================================CRSChartColorProp================================
class CRSChartColorProp : public CRSColorProp
{

public:
	CRSChartColorProp(CObject* pOwnerObj, CRS_PropertyGrid* pPropertyGrid, const CString& strName, COLORREF* value, const CString& description, EnumChartObject eChartObjectType);
	CWoormDocMng*			GetDocument();
private:
	EnumChartObject m_eCharObjType;
	CRS_PropertyGrid* m_pPropertyGrid;

protected:
	virtual BOOL OnUpdateValue();
};

//=================================CRSChartBoolProp================================
class CRSChartBoolProp : public CRSBoolProp
{

public:
	CRSChartBoolProp(CObject* ownerObj, const CString& strName, BOOL* value, const CString& description, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, EnumChartObject eChartObjectType);

private:
	EnumChartObject m_eCharObjType;
	SymTable*		m_pSymTable;
	CRS_ObjectPropertyView* m_pPropertyView;

protected:
	virtual void DrawProperties();
	virtual BOOL OnUpdateValue();
};


//=============================================================================
//Classe derivata per implementare la combo box con i tipi di chart riconosciuti
class CRSChartTypeComboProp : public CRSChartProp
{
public:
	CRSChartTypeComboProp(Chart* pChart, CRS_ObjectPropertyView* propertyView, CrsProp* pDSProp);
	CRSChartTypeComboProp(Chart::CSeries* pSeries, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, CrsProp* pDSProp);

protected:
	virtual BOOL OnUpdateValue();
public:
	virtual void DrawProperties();

private:
	Chart::CSeries*			m_pSeries;
	CRS_ObjectPropertyView* m_pPropertyView;
	BOOL m_bIsSeries = FALSE;
	SymTable* m_pSymTable;
	CrsProp* m_pDSProp;
	CBCGPProp* m_pColorChartProp;
};

//=============================================================================
//Classe derivata implementare la combo box con i campi da cui è possibile ricavare il datasource di categorie/serie
class CRSChartFieldComboProp : public CRSChartProp
{

public:
	CRSChartFieldComboProp(CObject* pOwner, Chart* pChart, CRS_ObjectPropertyView* propertyView, SymTable* pSymTable, EnumChartObject objType = EnumChartObject::CATEGORY, int nBindedFieldIndex = -1 );

protected:
	virtual BOOL OnUpdateValue();
	virtual void DrawProperties(WoormField*);

protected:
	SymTable*		m_pSymTable;
	int m_nBindedFieldIndex;
	CRS_ObjectPropertyView*		m_pPropertyView;
private:
	EnumChartObject m_eObjType;
};

//=============================================================================
//Classe derivata implementare la combo box con gli enumerativi per la posizione della legenda del grafico
class CRSChartLegendPosComboProp : public CRSChartProp
{

public:
	CRSChartLegendPosComboProp(CObject* pOwner);

protected:
	virtual BOOL OnUpdateValue();
};

//=============================================================================
//Classe derivata implementare la combo box con gli enumerativi per lo stile delle linee
class CRSChartLineStyleComboProp : public CRSChartProp
{

public:
	CRSChartLineStyleComboProp(CObject* pOwner);

protected:
	virtual BOOL OnUpdateValue();
};
//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSBarCodeShowTextProp : public CRSBarCodeProp
{
	friend class CRSBarCodeProp;

public:
	CRSBarCodeShowTextProp(CObject*, const CString& strName, BOOL* value, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, const CString& description = L"");
	BOOL*		m_pBValue;

protected:
	virtual BOOL		OnUpdateValue();

public:
	virtual void DrawProperties(BOOL bDefaultValue = TRUE);
	virtual void UpdatePropertyLayout(BOOL defaultValue = TRUE);
};

//=============================================================================
//Classe derivata implementare la combobox da cui dipende la scelta per selezionare il modulo/larghezza barra del barcode
class CRSBarCodeSizeProp : public CRSBarCodeProp
{
private:
	PropertySizeType m_eSizeType;
public:
	CRSBarCodeSizeProp(CObject*, CBarCode* pBarCode, CRS_ObjectPropertyView* propertyView, PropertySizeType eSizeType, BOOL bDefaultValue = TRUE);

public:
	virtual void UpdatePropertyLayout(BOOL defaultValue = TRUE);
protected:
	virtual BOOL OnUpdateValue();
};

//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (il row separator)
class CRSTableRowSeparatorProp : public CrsProp
{
public:

	CRSTableRowSeparatorProp(Table* pTable);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	Table* m_pTable;
};

//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSAlternateColorProp : public CrsProp
{
public:

	CRSAlternateColorProp(Table* pTable);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	Table* m_pTable;
};

//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSSplitterColumnProp : public CrsProp
{
public:

	CRSSplitterColumnProp(TableColumn* pCol);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	TableColumn* m_pCol;
};

// Classe derivata per implementare una proprietà ternaria della tabella(alternate row color)
class CrsGridStyleProp : public CrsProp
{
public:

	CrsGridStyleProp(WoormIni* pWrmIni);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	WoormIni* m_pWrmIni;
};


//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSTableAllColumnsColorProp : public CRSColorProp
{
public:
	enum PropertyType { ColumnTitleBorderColor,
						BodyBorderColor,
						TotalBorderColor};

	CRSTableAllColumnsColorProp(Table* pTable, PropertyType ePropType);

protected:
	BOOL OnUpdateValue();
	BOOL OnEndEdit();

private:
	void UpdatePropertyValue();

private:
	Table*			m_pTable;
	PropertyType	m_ePropType;
};

//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSTableAllColumnsColorWithExprProp : public CRSColorWithExprProp
{
public:
	enum PropertyType {
		ColumnTitleBackColor	, ColumnTitleForeColor,
		BodyBackColor			, BodyForeColor,
		SubTotalBackColor		, SubTotalForeColor,
		TotalBackColor			, TotalForeColor,
	};

	CRSTableAllColumnsColorWithExprProp(Table* pTable, PropertyType ePropType, CRS_ObjectPropertyView* propertyView);
	BOOL		OnEndEdit();

protected:
	BOOL		HasExpression();
	void		OnRightButtonClick();
	void		OnDrawStateIndicator(CDC* pDC, CRect rect);
private:
	void		UpdatePropertyValue();
	Expression* GetCommonExpression();
	void		SetCommonExpression(Expression * expr);

private:
	Table*			m_pTable;
	PropertyType	m_ePropType;
};

//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSTableMultiColumnsColorProp : public CRSColorProp
{
public:
	enum PropertyType {
		TitleBorderColor,
		BodyBorderColor,
		TotalBorderColor
	};

	CRSTableMultiColumnsColorProp(MultiColumnSelection* pColumns, PropertyType ePropType);

protected:
	BOOL OnUpdateValue();
	BOOL OnEndEdit();

private:
	void UpdatePropertyValue();

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_ePropType;
};

//=============================================================================
//Classe derivata per implementare una proprietà ternaria della tabella (alternate row color)
class CRSTableMultiColumnsColorWithExprProp : public CRSColorWithExprProp
{
public:
	enum PropertyType {
		TitleBackColor, TitleForeColor,
		BodyBackColor, BodyForeColor,
		SubTotalBackColor, SubTotalForeColor,
		TotalBackColor, TotalForeColor,
	};

	CRSTableMultiColumnsColorWithExprProp(MultiColumnSelection* pColumns, PropertyType ePropType, CRS_ObjectPropertyView* propertyView);

	BOOL		OnEndEdit();

protected:
	BOOL		HasExpression();
	void		OnRightButtonClick();
	void		OnDrawStateIndicator(CDC* pDC, CRect rect);
private:
	void		UpdatePropertyValue();
	Expression* GetCommonExpression();
	void		SetCommonExpression(Expression * expr);

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_ePropType;
};

//=============================================================================
//property Hidden per la multiselezione delle colonne
class CRSTableMultiColumnsBoolProp : public CrsProp
{
public:

	enum PropertyType { Hidden };

	CRSTableMultiColumnsBoolProp(MultiColumnSelection* pcolumns, PropertyType ePropType);

protected:
	virtual void	UpdatePropertyValue();
	virtual BOOL	OnUpdateValue();
	void			UpdateValue();

protected:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;

private:
	PropertyType			m_ePropType;
};

//=============================================================================
class CRSMultiColumnsHiddenProp : public CRSTableMultiColumnsBoolProp
{
public:
	CRSMultiColumnsHiddenProp(MultiColumnSelection* pcolumns, CRS_ObjectPropertyView* pPropertyView);

protected:
	BOOL			HasExpression();
	virtual void	OnClickButton(CPoint point);
	void			OnRightButtonClick();
	void			OnDrawStateIndicator(CDC* pDC, CRect rect);
	virtual void	OnDrawButton(CDC* pDC, CRect rectButton);
	void			AdjustButtonRect();
	virtual BOOL	OnUpdateValue();
	virtual void	UpdatePropertyValue();

private:
	Expression* GetCommonExpression();
	void		SetCommonExpression(Expression * expr);
	void		SetState();

private:
	CRS_ObjectPropertyView* m_pPropertyView;
	CBCGPToolBarImages		m_imageExpr;

};

//=============================================================================
//property per le sizes della multiselezione delle colonne
class CRSTableMultiColumnsSizeProp : public CrsProp
{
public:

	enum PropertyType { Width };

	CRSTableMultiColumnsSizeProp(MultiColumnSelection* pcolumns, PropertyType ePropType, int minValue=1, int maxValue=1000);

	virtual void	UpdateIntValue(int previousValue);

protected:
	virtual void	UpdatePropertyValue();
	virtual BOOL	OnUpdateValue();

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_ePropType;
	int						m_nMinValue;
	int						m_nMaxValue;
	int						m_nDefaultSize = 100;
};

//=============================================================================
//property per la misura in mm della parent
class CRSMMProp : public CrsProp
{
public:
	CRSMMProp(CrsProp* pParentProp, CString strName, int initialValueInPixel, LPCTSTR lpszDescr = NULL, int minValueInMM = 1, int maxValueinMM = 300);
	
	void			UpdateMM(int pixel);
	static int		PixelInMM(int pixel);

protected:
	virtual BOOL	OnUpdateValue	();
	int				MMinPixel		();

private:
	CrsProp*		m_pParentProp;
};


//=============================================================================
//Classe derivata per implementare le proprietà di spessore dei bordi delle colonne della tabella selezionata
class CRSTableAllColumnsBorderSizeProp : public CrsProp
{
public:
	enum PropertyType { ColumnTitle , Total, Body };

	CRSTableAllColumnsBorderSizeProp(Table* pTable, PropertyType ePropType);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	Table*			m_pTable;
	PropertyType	m_ePropType;
};

//=============================================================================
//Classe derivata per implementare le proprietà di spessore dei bordi delle colonne selezionate
class CRSTableMultiColumnsBorderSizeProp : public CrsProp
{
public:
	enum PropertyType { Title, Body, Total };

	CRSTableMultiColumnsBorderSizeProp(MultiColumnSelection* pColumns, PropertyType ePropType);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_ePropType;
};

//=============================================================================
//Classe derivata per implementare la property relativa al numero di righe della tabella
class CRSRowCountProp : public CrsProp
{
public:
	CRSRowCountProp(Table* pTable);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	Table* m_pTable;
};

//=============================================================================
//Classe derivata per modificare le altezze di titolo tabella, titolo colonna, corpo e totale di una tabella
class CRSTableHeightsProp : public CrsProp
{
public:

	enum PropertyType{
		TableTitle,
		ColumnTitle,
		Row,
		Total
	};

public:
	CRSTableHeightsProp(Table* pTable, PropertyType propType);

protected:
	BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	Table* m_pTable;
	PropertyType m_propType;
};

//=============================================================================
//Classe derivata per modificare le altezze di titolo tabella, titolo colonna, corpo e totale di una tabella   in MM
class CRSTableHeightsPropMM : public CrsProp
{
public:

	enum PropertyType {
		TableTitle,
		ColumnTitle,
		Row,
		Total
	};

public:
	CRSTableHeightsPropMM(Table* pTable, PropertyType propType);

protected:
	BOOL OnUpdateValue();
private:
	void UpdatePropertyValue();

private:
	PropertyType m_propType;
	Table* m_pTable;
};

//=============================================================================
//Classe derivata per modificare le altezze di titolo tabella, titolo colonna, corpo e totale di una tabella
//todo andrea: vedere se unificare con la crsstring, chiamando la settext nella onupdatevalue
class CRSTableTitlTextProp : public CRSStringProp
{
	public:
		CRSTableTitlTextProp(Table* pTable);

	protected:
		BOOL OnUpdateValue();

	private:
		Table* m_pTable;
};

//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSMulBoolProp : public CrsProp
{
public: 
	enum PropertyType{ Transparent, BorderSideLeft, BorderSideTop, BorderSideRight, BorderSideBottom, Hidden };

public:
	CRSMulBoolProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description);

protected:
	BOOL OnUpdateValue();
	void UpdateValue();
	void UpdatePropertyValue();

protected:
	TDisposablePtr<SelectionRect>	m_pMulSel;

private:
	PropertyType	m_propType;

};


//=============================================================================
class CRSMulHiddenProp : public CRSMulBoolProp
{
public:
	CRSMulHiddenProp(SelectionRect* pMulSel, const CString& strName, const CString& description, CRS_ObjectPropertyView* propertyView);

protected:
	BOOL			HasExpression();
	virtual void	OnClickButton(CPoint point);
	void			OnRightButtonClick();
	void			OnDrawStateIndicator(CDC* pDC, CRect rect);
	virtual void	OnDrawButton(CDC* pDC, CRect rectButton);
	void			AdjustButtonRect();
	virtual BOOL	OnUpdateValue();
	virtual void	UpdatePropertyValue();

private:
	Expression* GetCommonExpression();
	void		SetCommonExpression(Expression * expr);
	void		SetState();

private:
	CRS_ObjectPropertyView* m_pPropertyView;
	CBCGPToolBarImages		m_imageExpr;

};

//=============================================================================
//Classe derivata da BCG per aggiornare i colori di una property e successivamente il documento di Woorm - todo farla derivare da crsProp per la visibilità
class CRSMulColorProp : public CBCGPColorProp
{
	friend class CRSShowColumnTotalProp;
public:
	enum PropertyType{ Shadow, Border };

public:
	CRSMulColorProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description);
	BOOL OnEndEdit();

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;

private:
	void	UpdatePropertyValue();

};

//=============================================================================
class CRSMulColorWithExprProp : public CRSColorWithExprProp
{
public:
	enum PropertyType{ ValueBackGroundColor, ValueForeColor, LabelForeColor };

public:
	CRSMulColorWithExprProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description, CRS_ObjectPropertyView* propertyView);
	~CRSMulColorWithExprProp();

	BOOL		OnEndEdit();

protected:
	BOOL		HasExpression();
	void		OnRightButtonClick();
	void		OnDrawStateIndicator(CDC* pDC, CRect rect);

private:
	void		UpdatePropertyValue();
	Expression* GetCommonExpression();
	void		SetCommonExpression(Expression * expr);

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;

};

//=============================================================================
//Classe derivata da BCG per aggiornare i valori booleani della property e successivamente il documento di Woorm
class CRSMulIntProp : public CrsProp
{
public:
	enum PropertyType{ ShadowSize, BorderSize, HRatio, VRatio, Layer };

public:
	CRSMulIntProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description);

protected:
	virtual BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;

};

//=============================================================================
//Classe derivata da BCG per aggiornare Location e Size di un Rect
class CRSMulRectProp : public CrsProp
{
public:
	enum PropertyType{ AlignToXP, AlignToYP, AlignToXM, AlignToYM, LocationXP, LocationYP, LocationXM, LocationYM, WidthP, HeightP, WidthM, HeightM
	};

	CRSMulRectProp(SelectionRect* pMulSel, const CString& strName, PropertyType propType, const CString& description, int fromValue = 0, int toValue = 2000);

protected:
	virtual BOOL OnUpdateValue();

private:
	void UpdatePropertyValue();
	void UpdateObjectValue(LONG previousValue = 0);

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;

};

//=============================================================================
//Classe derivata da Bcg per settare il fontStyle per oggetti uguali
class CRSMulFontStyleProp : public CrsProp
{
public:
	enum PropertyType{ Label, Value };

	CRSMulFontStyleProp(SelectionRect* pMulSel, PropertyType propType);

protected:
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	void UpdatePropertyValue();
	CString GetFontName(FontIdx fontIdx);

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;
	FontIdx			m_commonFont;
};

//=============================================================================
//Classe derivata da Bcg per settare il fontStyle delle colonne
class CRSMultiColumnFontStyleProp : public CrsProp
{
public:
	enum PropertyType { Title, Body, Total, Subtotal };

	CRSMultiColumnFontStyleProp(MultiColumnSelection* pColumns, PropertyType propType);

protected:
	BOOL HasButton() const { return TRUE; }
	void OnClickButton(CPoint point);

private:
	void UpdatePropertyValue();
	CString GetFontName(FontIdx fontIdx);

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_propType;
	FontIdx					m_commonFont;
};

//=============================================================================*************
//TODO ANDREA: non più utilizzata e probabilmente da rimuovere					!!!!!!!!!!!!
//=============================================================================*************
//Classe derivata da Bcg per settare L'alignment style per oggetti uguali
class CRSMulAlignmentStyleProp : public CrsProp
{
public:
	enum PropertyType{ Label, Value };

	CRSMulAlignmentStyleProp(SelectionRect* pMulSel, PropertyType propType);

protected:
	BOOL HasButton() const	{ return TRUE; }
	void OnClickButton(CPoint point);

private:
	void UpdatePropertyValue();

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;
	AlignType		m_commonAlignType;

};

//=============================================================================
//Classe derivata per implementare la combo box dei print attributes
class CRSColumnPrintAttributesProp : public CrsProp
{

public:
	CRSColumnPrintAttributesProp(TableColumn* pCol);

protected:
	BOOL OnUpdateValue();

private:
	TableColumn*			m_pCol;
};


//=============================================================================
//Classe derivata per implementare una proprietà corrispondente all'align type
class CRSAlignBitwiseProp : public CrsProp
{
	friend class CRSBitBoolProp;
	friend class CRSBitProp;
	friend class CRSBitSingleLineProp;

public:
	CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, CObject* pOwner, const CString& strName, AlignType* alignType, 
						BOOL bAllowVertical, BOOL bAllowCenterBottom, BOOL bAllowFieldSet, 
						DWORD_PTR dwData = 0, LPCTSTR lpszDescr = NULL,
						BOOL bAllowWordBreak=TRUE, BOOL bAllowLineProp = TRUE, BOOL bAllowPrefixSelectionProp = TRUE, BOOL bAllowExpandTabProp = TRUE);
	
	virtual ~CRSAlignBitwiseProp();

	void Rebuild();

	void RedrawSingleLineProp();

protected:

	CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, CObject* pOwner);
	CRSAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, MultiColumnSelection* pMulCol);

	virtual CString FormatProperty();

	virtual BOOL IsEditAvailable() { return FALSE; }
	virtual BOOL OnEdit(LPPOINT /*lptClick*/) { return FALSE; }
	virtual BOOL NoInplaceEdit() const { return TRUE; }

	void			UpdateAlignType		(); //called from children
	void			AlignTypeToProps	();
	inline void		SetAlignType		(AlignType* pAlignType) { m_pAlignType= pAlignType; }
	virtual void	UpdateSelectedObject();
	AlignType		GetAlignType		() { return *m_pAlignType; }
	AlignType		GetOldAlignType		() { return m_nAlignTypeOld; }


	BOOL m_bAllowVertical;
	BOOL m_bAllowCenterBottom;
	BOOL m_bAllowFieldSet;

	BOOL m_bAllowWordBreak;
	BOOL m_bAllowLineProp;
	BOOL m_bAllowPrefixSelectionProp;
	BOOL m_bAllowExpandTabProp;

private:
	enum BitSingleLine		{ MultiLine		= 0,	SingleLine			= DT_SINGLELINE};
	enum BitHorizontalALign { Left			= 0,	CenteredHorizontal	= DT_CENTER,			Right				= DT_RIGHT };
	enum BitVerticalALign	{ Top			= 0,	CenteredVertical	= DT_VCENTER,			CenteredVerticalRel	= DT_EX_VCENTER_LABEL,		Bottom = DT_BOTTOM,		FieldSet = DT_EX_FIELD_SET};
	enum BitOrientation		{ Orientation_0 = 0,	Orientation_90		= DT_EX_ORIENTATION_90,	Orientation_270		= DT_EX_ORIENTATION_270};

	CObject*				m_pOwner;
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	AlignType*				m_pAlignType = NULL;
	AlignType				m_nAlignTypeOld = 0;

	CRS_ObjectPropertyView*	m_pPropertyView;

	//semplici booleani
	BOOL					m_bWordBreak;		//DT_WORDBREAK
	CRSBitBoolProp*			m_WordBreakProp;

	BOOL					m_bExpandTab;		//DT_EXPANDTABS
	CRSBitBoolProp*			m_ExpandTabProp;

	BOOL					m_bWithoutPrefix;	//DT_WITHOUTPREFIX
	CRSBitBoolProp*			m_WithoutPrefixProp;

	//sono enumerativi messi negli itemData delle dropdown
	BitSingleLine			m_eSingleLine;		//DT_SINGLELINE
	CRSBitSingleLineProp*	m_SingleLineProp;

	BitHorizontalALign		m_eHorizontalALign	= BitHorizontalALign::Left;
	CRSBitProp*				m_HorizontalAlignProp;

	BitVerticalALign		m_eVerticalAlign	= BitVerticalALign::Top;
	CRSBitProp*				m_VerticalAlignProp;

	BitOrientation			m_eOrientation		= BitOrientation::Orientation_0;
	CRSBitProp*				m_OrientationProp;

	
private:
	
	CString GetSingleLineString();
	CString GetHorizontalAlignString();
	CString GetVerticalAlignString();
	CString GetOrientationString();
	
};

//=============================================================================
//Classe derivata per implementare una proprietà corrispondente all'align type
class CRSColumnAlignBitwiseProp : public CRSAlignBitwiseProp
{
public:
	enum PropertyType { Body };

public:
	CRSColumnAlignBitwiseProp(CRS_ObjectPropertyView* PropertyView, TableColumn* pCol, const CString& strName, AlignType* pAlignType, PropertyType ePropertyType);
	TableColumn*	GetTableColumn() { return m_pCol; }

protected:
	virtual void	UpdateSelectedObject();

private:
	PropertyType	m_ePropertyType;
	TableColumn*	m_pCol;
};

//=============================================================================
//Classe derivata per decidere se importare oggetti statici di un template
class CRSBitBoolProp : public CRSBoolProp
{
public:

	CRSBitBoolProp(CRSAlignBitwiseProp* pParent, CObject* pOwner, const CString& strName, BOOL* value, const CString& description = L"");
	CRSBitBoolProp(CRSAlignBitwiseProp* pParent, MultiColumnSelection* pColumns, const CString& strName, BOOL* value, const CString& description = L"");

private:
	virtual BOOL OnUpdateValue();

private:
	CRSAlignBitwiseProp*	m_pParent;
};

//=============================================================================
//Classe derivata per decidere se importare oggetti statici di un template
class CRSBitProp : public CrsProp
{
	friend class CRSBitSingleLineProp;

public:

	CRSBitProp(CRSAlignBitwiseProp* pParent, const CString& strName, const _variant_t& varValue);

private:
	virtual BOOL OnUpdateValue();

private:
	CRSAlignBitwiseProp*	m_pParent;
};

//=============================================================================
//Classe derivata per decidere se importare oggetti statici di un template
class CRSBitSingleLineProp : public CRSBitProp
{
	friend class CRSAlignBitwiseProp;
public:

	CRSBitSingleLineProp(CRSAlignBitwiseProp* pParent, CRSBitBoolProp* pChild, const CString& strName, const _variant_t& varValue);
	void		 DrawProperties();

private:
	virtual BOOL OnUpdateValue();
	

private:
	CRSAlignBitwiseProp*	m_pParent;
	CRSBitBoolProp*			m_pChild;
};

//=============================================================================
//Classe derivata da Bcg per settare L'alignment style per la multiselezione
class CRSMulAlignmentStyleBitWiseProp : public CRSAlignBitwiseProp
{
public:
	enum PropertyType { Label, Value };

	CRSMulAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, SelectionRect* pMulSel, PropertyType propType);

protected:
	virtual BOOL	HasButton() const { return TRUE; }
	virtual void	UpdateSelectedObject();

private:
	void			UpdatePropertyValue();

private:
	TDisposablePtr<SelectionRect>	m_pMulSel;
	PropertyType	m_propType;
	AlignType		m_commonAlignType;

};

//=============================================================================
//Classe derivata da Bcg per settare L'alignment style per le allCOlumns di una tabella
class CRSAllColumnsAlignmentStyleBitWiseProp : public CRSAlignBitwiseProp
{
public:
	enum PropertyType { ColumnTitles, Body, Totals };

	CRSAllColumnsAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, Table* pTable, PropertyType propType);

protected:
	virtual BOOL	HasButton() const { return TRUE; }
	virtual void	UpdateSelectedObject();

private:
	void			UpdatePropertyValue();

private:
	Table*			m_pTable;
	PropertyType	m_propType;
	AlignType		m_allColumnsAlignType;

};

//=============================================================================
//Classe derivata da Bcg per settare L'alignment style per le allCOlumns di una tabella
class CRSMultiColumnsAlignmentStyleBitWiseProp : public CRSAlignBitwiseProp
{
public:
	enum PropertyType { ColumnTitles, Body, Totals };

	CRSMultiColumnsAlignmentStyleBitWiseProp(CRS_ObjectPropertyView* PropertyView, MultiColumnSelection* pMulCol, PropertyType propType);
	MultiColumnSelection*	GetColumns() { return m_pColumns; }

protected:
	virtual void	UpdateSelectedObject();

private:
	void			UpdatePropertyValue();

private:
	TDisposablePtr<MultiColumnSelection>	m_pColumns;
	PropertyType			m_propType;
	AlignType				m_allColumnsAlignType;

};

//=============================================================================
//Classe derivata da Bcg per settare lo stile delle linee e aggiornare il file di configurazione di woorm
class CRSLineStyleProp :public CBCGPLineStyleProp
{
public:

	CRSLineStyleProp(CObject* pOwner, const CString& strName, int* pStyle, CRS_ObjectPropertyView* pPropertyView,
		LPCTSTR lpszDescr = NULL, DWORD_PTR dwData = 0);

protected:
	virtual CString FormatProperty();
	virtual BOOL OnUpdateValue();

private:
	CObject*				m_pOwner;
	int*					m_pStyle;
	CRS_ObjectPropertyView* m_pPropertyView;
};

//=============================================================================
//Classe derivata da Bcg per settare lo stile della selezione degli oggetti di woorm e aggiornare il file di configurazione di woorm
class CRSObjectSelectionStyleProp :public CrsProp
{
public:

	CRSObjectSelectionStyleProp(WoormIni* pWrmIni, CRS_ObjectPropertyView* pPropertyView);

	virtual BOOL OnUpdateValue();

private:
	void DrawProperties(BOOL bShowSubItems);

private:
	DataBool*				m_bNewObjectSelectionStyle;
	WoormIni*				m_pWrmIni;
	CRS_ObjectPropertyView*	m_pPropertyView;
};



//=============================================================================
//Class for new link of type function
class CRSNewLinkFunction : public CrsProp
{
	friend CRS_ObjectPropertyView;
public:
	enum FieldType {
		MODULE , FUNCTION
	};

	CRSNewLinkFunction(CString name, LPCTSTR value, CRS_ObjectPropertyView* propView, CRSNewLinkFunction::FieldType fType, DWORD_PTR data= 0UL);
	CRSNewLinkFunction(CString name, variant_t value, CRS_ObjectPropertyView* propView, CRSNewLinkFunction::FieldType fType, DWORD_PTR data = 0UL);

protected:
	BOOL OnUpdateValue();

private:
	CRS_ObjectPropertyView* m_pPropertyView;
	FieldType m_fType;
};

//=============================================================================
//Class for new link of type URL
class CRSNewLinkParam : public CrsProp
{
	friend CRS_ObjectPropertyView;
public:
	enum FieldType {
		ENUM_SUBTYPE ,TYPE
	};

	CRSNewLinkParam(CString name, LPCTSTR value, CRS_ObjectPropertyView* propView, CRSNewLinkParam::FieldType fType, BOOL ifUrl, DWORD_PTR data = 0UL);
	CRSNewLinkParam(CString name, variant_t value, CRS_ObjectPropertyView* propView, CRSNewLinkParam::FieldType fType, BOOL ifUrl, DWORD_PTR data = 0UL);

protected:
	BOOL OnUpdateValue();

private:
	CRS_ObjectPropertyView* m_pPropertyView;
	FieldType m_fType;
	BOOL m_ifUrl;
};

//=============================================================================
//Class for new link of type explorer
class CRSNewLinkExplorer : public CrsProp
{
public:	 
	enum NewLinkType{ REPORT, FORM};
	CRSNewLinkExplorer(const CString& strName, CRS_ObjectPropertyView* pPropertyView, NewLinkType nLType, const CString& description = L"");

protected:
	virtual BOOL HasButton() const { return TRUE; }
	virtual void OnClickButton(CPoint point);
	virtual void UpdateDocument() {/*DO NOTHING*/ };

protected:
	CRS_ObjectPropertyView* m_pPropertyView;
	NewLinkType m_type;
};

//=============================================================================

//Checkbox property
class CRSCheckBoxProp : public CrsProp
{
	friend class CRS_PropertyGrid;

public:
	CRSCheckBoxProp
		(
			const CString& strName,
			BOOL bCheck,
			LPCTSTR lpszDescr = NULL,
			DWORD_PTR dwData = 0 ,
			CRS_ObjectPropertyView *propView = NULL,
			BOOL bHideChildren = TRUE
		);

	CRSCheckBoxProp
	(
		const CString& strName,
		BOOL* pbCheck,
		LPCTSTR lpszDescr = NULL,
		DWORD_PTR dwData = 0,
		CRS_ObjectPropertyView *propView = NULL,
		BOOL bHideChildren = FALSE
		);

protected:
	virtual BOOL OnEdit(LPPOINT /*lptClick*/) { return FALSE; }
	virtual void OnDrawButton(CDC* /*pDC*/, CRect /*rectButton*/) {}
	virtual void OnDrawValue(CDC* /*pDC*/, CRect /*rect*/) {}
	virtual BOOL HasButton() const { return FALSE; }

	virtual BOOL PushChar(UINT nChar);
	virtual void OnDrawCheckBox(CDC * pDC, CRect rectCheck, BOOL bChecked);
	virtual void OnDrawName(CDC* pDC, CRect rect);
	virtual void OnClickName(CPoint point);
	virtual BOOL OnDblClick(CPoint point);

protected:
	CRect  m_rectCheck;
	CRS_ObjectPropertyView *m_propView = NULL;
	BOOL*  m_pBoolValue = NULL;
	BOOL   m_bHideChildren = TRUE;

	void SetVisibleSubItems();

	virtual BOOL OnShowChild(CrsProp*) { return TRUE; }
	virtual BOOL OnShowGrandChild(CrsProp*, CrsProp*) { return TRUE; }
};

//=============================================================================
//Checkbox property
class CRSChkDBCol : public CRSCheckBoxProp
{
public:
	CRSChkDBCol
		(
			const CString& strName,
			BOOL bCheck,
			LPCTSTR lpszDescr = NULL,
			DWORD_PTR dwData = 0,
			CRS_ObjectPropertyView *propView = NULL
		) 
		: 
		CRSCheckBoxProp
			(
				strName,
				bCheck,
				lpszDescr,
				dwData,
				propView 
			) {}

	virtual BOOL OnShowGrandChild(CrsProp*, CrsProp*);

};



///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
