#pragma once

#include <TbGenlib\SettingstableManager.h>
//#include "HotFilterQueryParser.h"
#include "UnpinnedTilesPane.h"



#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE_DATE.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE_DATE_WITH_SELECTION.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE_WITH_SELECTION.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE_INT.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_RANGE_INT_WITH_SELECTION.hjson"
#include "JsonForms\HotFilter\IDD_TD_HOTFILTER_LIST_CHECKBOX.hjson"

#include "beginh.dex"

enum EHotFilterType {
	HF_WRONG,
	HF_DATE_SIMPLE, HF_DATE_WITHSELECTION, HF_RANGE_SIMPLE, HF_RANGE_WITHSELECTION, HF_RANGE_WITHPICKER,
	HF_LISTBOX, HF_LIST_POPUP, HF_CHECKLISTBOX,
	HF_CUSTOM, HF_ARRAY
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterObj		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterObj : public CCmdTarget, public IDisposingSourceImpl
{
	friend class HotFilterManager;
	DECLARE_DYNAMIC(HotFilterObj)
	bool m_bManageUI = true;
public:
	HotFilterObj	(EHotFilterType type, CAbstractFormDoc* pDocument, HotFilterManager* pHotFilterManager, int nNotificationIDC = 0);
	~HotFilterObj();

public:
	EHotFilterType		GetType() const { return m_eHFType; }
	void				SetType(EHotFilterType type);
	void				SetManageUI(bool bManageUI) { m_bManageUI = bManageUI; }
	CAbstractFormDoc*	GetDocument() { return m_pDocument; }
	void 				AttachDocument(CAbstractFormDoc* pDoc) { m_pDocument = pDoc; }

	HotFilterManager*	GetHFManager() { return m_pHotFilterManager; }
	void				AttachHFMngParent(HotFilterManager* pParentHFMng) { m_pHotFilterManager = pParentHFMng; }

	void				SetName(const CString& strName);
	CString				GetName()	const;
	const CTBNamespace&	GetNamespace() const;

	void				AttachHFLParent(HotFilterObj* pParentHFL) { m_pParentHFL = pParentHFL; }

	void				EnableSelectionReturn(BOOL m_bSel = TRUE) { m_bSetSelected = m_bSel; }

	DataObjArray&		GetSelectedObj() { return m_SelectedObj; }

	// provide automatic read/store of picked values via settings, override for custom behavior
	virtual void	GetSettingsValues(CParameterInfo& aSettings);
	virtual void	SetSettingsValues(CParameterInfo& aSettings);
	virtual BOOL	OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	void	InitItemsList(const DataObj& aValue);
	void	InitItemsList(const DataObjArray& aValue);


	// The instance name is used to uniquely identify the hotfilter, i.e., for query parameters, settings, etc.
	// It is automatically set, it may be useful to change it to avoid name clashing in case of multiple instance of
	// the same hotfilter
	void				SetInstanceName(const DataStr& aName);
	// if the host document want to perform some action at the change of some of the HFL's controls, must set this IDC
	// and will receive a ON_EN_VALUE_CHANGED message. Call GetLastAction to know the changed element (see HFL_ELEM below)
	void				SetNotificationIDC(UINT nIDC);
	int					GetLastAction() { return m_nLastAction; }

	void				SetCaption(CString strCaption) { m_strCaption = strCaption; }
	CString				GetCaption() { return m_strCaption; }

	virtual void			ResetCriteria();

	virtual BOOL			IsEmptyQuery() = 0;
	virtual void			DefineQuery
							(
										SqlTable*	pTable,
										SqlRecord*	pRec,
								const	DataObj&	aColumn,
										CString		sOperator = _T("AND")
							) = 0;


	virtual void			PrepareQuery(SqlTable* pTable) = 0;
	virtual void			OnResetCriteria() = 0;
	virtual void			ManageReadOnly() = 0;
		    void			InitializeHotFilter();
			void			SetReadOnly(const DataBool& bReadOnly) { m_bIsReadOnly = bReadOnly; ManageReadOnly(); }


	virtual BOOL			OnBeforeBatchExecute() { return TRUE; }
	virtual void			CompleteQuery(SqlTable* pTable, SqlRecord*) { ASSERT(FALSE); }
	
	virtual void			Customize();
			void			AddVar(const CString& sVarName, DataObj& aVar);

			void			AttachNsHotlink(const HotLinkInfo* hklInfo);

	virtual void			CreateHotlinks() {}
	virtual void			OnParsedControlCreated(CParsedCtrl* pCtrl, CAbstractFormDoc* pDoc) {}

protected:
	virtual CRuntimeClass*	GetDataObjClass() const = 0;
	virtual	void			OnInitItemsList() { /* default do nothing */ }
	virtual void			PreparePickedItemsList() {}

	const	DataStr&		GetInstanceName();

	void					DefineQuerySelectionPicked
							(
										SqlTable*	pTable,
										SqlRecord*	pRec,
								const	DataObj&	aColumn,
										CString		sOperator = _T("AND")
							);

	void					PrepareQuerySelectionPicked
							(
								SqlTable*	pTable
							);

	void NotifyChanged(UINT nAction, HWND hwnd);

protected:
	EHotFilterType		m_eHFType = EHotFilterType::HF_WRONG;
	CAbstractFormDoc*	m_pDocument = NULL;
	HotFilterObj*		m_pParentHFL = NULL;
	HotFilterManager*	m_pHotFilterManager = NULL;

	CString				m_strCaption;
	CString				m_strName;
	CTBNamespace		m_Namespace;
	DataStr				m_InstanceName;

	DataObjArray		m_SelectedObj;
	UINT				m_nNotificationIDC = 0; // if nonzero used to notify a "changed" on the HFL interface
	CString				m_sNotificationIDC;
	DataObjArray		m_PickedItemsList;
	BOOL				m_bSetSelected = FALSE;
	UINT				m_nLastAction = 0; // store the last changed element of the HFL interface (it stales quickly!)
	CString				m_sNsHotlink;
	BOOL				m_bMustExistData = FALSE;
	BOOL				m_bEnableAddOnFly = FALSE;
	BOOL				m_bIsReadOnly = FALSE;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterInfo		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterInfo : public CObject
{
public:
	HotFilterInfo
	(
		HotFilterObj*	pHFL,
		const	CString&		strLinkedColumnName,
		const	CString&		strTileTitle,
		BOOL			bInitiallyUnpinned
	)
	{
		m_pHFL = pHFL;
		m_strLinkedColumnName = strLinkedColumnName;
		m_strTileTitle = strTileTitle;
		m_bInitiallyUnpinned = bInitiallyUnpinned;
	}

	~HotFilterInfo() { SAFE_DELETE(m_pHFL); }

public:
	HotFilterObj*	m_pHFL;
	CString			m_strLinkedColumnName;
	BOOL			m_bInitiallyUnpinned;
	CString			m_strTileTitle;
};

//////////////////////////////////////////////////////////////////////////////////////////
//							HotFilterRangeElementColumn		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterRangeElementColumn : public CObject
{
public:
	HotFilterRangeElementColumn
	(
		CString		strColumnName,
		CString		strColumnTitle,
		CString 	strFieldName
	)
	{
		m_strColumnName = strColumnName;
		m_strColumnTitle = strColumnTitle;
		m_strFieldName = strFieldName;
	}

public:
	CString		m_strColumnName;
	CString		m_strColumnTitle;
	CString		m_strFieldName;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterRange		
//////////////////////////////////////////////////////////////////////////////////////////
// Allow the user to select all, a range or pick a list of elements
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterRange : public HotFilterObj
{
	friend class CHotFilterRangeWithSelectionTile;
	friend class CHotFilterPickerTile;
	friend class DBTDataPicker;
	friend class DataPickerRecordSet;
	friend class HotFilterDataPicker;
	friend class CHotFilterDataPickerView;
	friend class CHotFilterDataPickerTileGrp;

	DECLARE_DYNCREATE(HotFilterRange)
	DECLARE_MESSAGE_MAP()
public:
	enum Style { PICKER, WITH_ALL_SELECTION, SIMPLE };

public:
	HotFilterRange();
	HotFilterRange
	(
		EHotFilterType type, CAbstractFormDoc* pDocument, HotFilterManager* pHotFilterManager,
		CRuntimeClass*		pHKLClass,
		int					nNotificationIDC = 0
	);
	~HotFilterRange ();
	
public:
	virtual BOOL			IsEmptyQuery();
	virtual void			DefineQuery
							(
										SqlTable*	pTable,
										SqlRecord*	pRec,
								const	DataObj&	aColumn,
										CString		sOperator = _T("AND")
							);

	virtual void			PrepareQuery
							(
								SqlTable*	pTable
							);

	virtual BOOL			OnBeforeBatchExecute();

	virtual void			OnResetCriteria();
	virtual void			ManageReadOnly();
	virtual BOOL			CheckData();

	void					SetStyle(Style aStyle);
	Style					GetStyle() { return m_Style; }

	virtual void			GetSettingsValues(CParameterInfo& aSettings);

	int						GetHKLLinkedColumnIdx() { return m_nHKLLinkedColumnIdx; }
	void					SetHKLLinkedColumnIdx(int aLinkedColumnIdx) { m_nHKLLinkedColumnIdx = aLinkedColumnIdx; }
	void					AllowSavingQueries(const DataBool& bAllow = TRUE);

	DataBool&				GetRadioAll()	 { return m_bAll; }
	DataBool&				GetRadioRange()	 { return m_bRange; }

	void					SetRadioAllReadOnly(BOOL bReadOnly) { m_bAll.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioRangeReadOnly(BOOL bReadOnly) { m_bRange.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioFromReadOnly(BOOL bReadOnly) { m_RangeFrom.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioToReadOnly(BOOL bReadOnly) { m_RangeTo.SetAlwaysReadOnly(bReadOnly); }

	void					SetRangeAll(BOOL bAll) { m_bAll = bAll; m_bRange = !bAll; ManageReadOnly(); }

	const					 DataStr&	GetRangeFrom()	const { return m_RangeFrom; }
	const					 DataStr&	GetRangeTo()	const { return m_RangeTo; }

	void					SetRangeFrom(const DataStr& dRangeFrom) { m_RangeFrom = dRangeFrom; }
	void					SetRangeTo(const DataStr& dRangeTo) { m_RangeTo = dRangeTo; }

	const					DataBool&	GetRadioSelection()	const { return m_bSelection; }

	void					AddPickerColumn(CString strColumnName, CString strColumnTitle, CString strFieldName);

	template<class T> T*	GetHKLRangeFrom() { return dynamic_cast<T*>(m_pHKLRangeFrom); }
	template<class T> T*	GetHKLRangeTo() { return dynamic_cast<T*>(m_pHKLRangeTo); }

	virtual void			Customize();

protected:
	virtual CRuntimeClass*	GetDataObjClass() const;
	virtual	void			OnInitItemsList();

	virtual void			CustomizeDataPicker(SqlRecord* pRec, HotFilterDataPicker* pPicker);

	virtual BOOL			IsEmptyDataPickerSelectionQuery();
	// used by DBT and hotfilter to return the filtered query
	virtual	void			OnDefineDataPickerParamsQuery(const CString&, SqlTable*, SqlRecord*);
	virtual	void			OnPrepareDataPickerParamsQuery(const CString&, SqlTable*);
	// useful to reset picker criteria, called when the radiobutton changes from "selection" to other
	virtual void			ResetPickerCriteria();

	virtual BOOL			OnAttachDataPickerData(HotFilterDataPicker*) { return TRUE; /* default do nothing */ }
	virtual void			OnDataPickerCompleted(HotFilterDataPicker*) { /* default do nothing */ }
	virtual void			InitializeControls() {/* default do nothing */ }
	// ----------------------------------------------------------------------------------
	// To be overriden to allow saving custom queries for the hotfilter
	// Property m_bAllowSavingQueries must be set to TRUE
	virtual void			BindQueryParameters(/*QueryParams* pQueryParams*/) { ASSERT(FALSE); /* MUST be overriden to support saving queries */ }

	HotFilterObj*			AddDataPickerHotFilter(HotFilterObj* pHFL, const	CString& strLinkedColumnName, const CString& strTileTitle, BOOL bInitiallyUnpinned);

	virtual void			DefineQueryRange
							(
								SqlTable*	pTable,
								SqlRecord*	pRec,
								const	DataObj&	aColumn
							);
	virtual void			PrepareQueryRange(SqlTable*	pTable);

	void					OnFromChanged();
	void					OnToChanged();
	void					CreateHotlinks();


	void					DefineQuerySelectionByParams
							(
										SqlTable*	pTable,
										SqlRecord*	pRec,
								const	DataObj&	aColumn,
										CString		sOperator = _T("AND")
							);

	void					PrepareQuerySelectionByParams(SqlTable* pTable);

	void					DataPickerCompleted(HotFilterDataPicker* pDataPicker);
	CRuntimeClass*			GetRecordClass() const;

protected:
	CString					m_strDataPickerFormTitle;
	CRuntimeClass*			m_pRangeCtrlClass;

	Style					m_Style;
	DataBool		        m_bAll;
	DataBool		        m_bRange;
	DataBool		        m_bSelection;
	DataStr			        m_RangeFrom;
	DataStr			        m_RangeTo;
	DataBool		        m_bOpenSelection;

	HotKeyLink*		        m_pHKLRangeFrom;
	HotKeyLink*		        m_pHKLRangeTo;
	int				        m_nHKLLinkedColumnIdx;

	CArray<HotFilterInfo*>	m_DataPickerHotFilters;

	DataStr			        m_CurrentQuery;
	//HotFilterQueryParser*	m_pQueryParser;

	// Default FALSE, set this property to TRUE if the DataPicker must allow saving the query parameters
	// Parameters must be bound via the BindQueryParameters method
	DataBool		m_bAllowSavingQueries;

	DataObjArray	m_UnselectedItemsList;

	CArray<HotFilterRangeElementColumn*>	m_RangeColumns;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterList		
//////////////////////////////////////////////////////////////////////////////////////////
// Allow the user to pick a list of elements
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterList : public HotFilterObj
{
	friend class CHotFilterListPopupTile;
	friend class CHotFilterListListboxTile;
	friend class CHotFilterCheckListboxTile;

	DECLARE_DYNCREATE(HotFilterList)
	DECLARE_MESSAGE_MAP()
public:
	enum SelectionStyle { POPUP, LISTBOX, CHECKBOX };

public:
	HotFilterList
	(
		EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
		CRuntimeClass*		pHKLClass,
		int					nNotificationIDC = 0
	);
	HotFilterList();

	~HotFilterList();

public:
	virtual void		Customize();

	virtual BOOL		IsEmptyQuery();
	virtual void		PrepareQuery(SqlTable*	pTable);
	virtual void		OnResetCriteria();
	virtual void		ManageReadOnly();

	void				DefineQuery
						(
									SqlTable*	pTable,
									SqlRecord*	pRec,
							const	DataObj&	aColumn,
									CString		sOperator = _T("AND")
						);

	void				SetStyle(SelectionStyle aStyle) { m_SelectionStyle = aStyle; }
	SelectionStyle		GetStyle() { return m_SelectionStyle; }

	DataObjArray&		GetPickedItemList() { PreparePickedItemsList(); return m_PickedItemsList; }
	void				SetPickedItemList(DataObjArray& aArray) { m_PickedItemsList = aArray; }

	void				SetItemListReadOnly(BOOL bReadOnly) { m_arItemsList.SetReadOnly(bReadOnly); }

	void				OnFilledListbox();
	void				OnEmptyListbox();

protected:
	virtual CRuntimeClass*	GetDataObjClass() const;
	virtual void			PreparePickedItemsList();

	virtual void			CreateHotlinks() ;

private:
	SelectionStyle			m_SelectionStyle = SelectionStyle::CHECKBOX;
	HotKeyLink*				m_pHKLItemsList = NULL;
	int						m_nHKLLinkedColumnIdx = -1;
public:
	DataArray				m_arItemsList;
	DataStr					m_Caption;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterDateRange		
//////////////////////////////////////////////////////////////////////////////////////////
// Allow the user to select all or a range of dates
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterDateRange : public HotFilterObj
{
	friend class CHotFilterDateRangeWithSelectionTile;
	friend class CHotFilterRangeDateTile;

	DECLARE_DYNCREATE(HotFilterDateRange)
	DECLARE_MESSAGE_MAP()

public:
	enum SelectionStyle { SIMPLE, WITH_ALL_SELECTION };

public:
	HotFilterDateRange(EHotFilterType type, CAbstractFormDoc* pDocument, HotFilterManager* pHotFilterManager,
		SelectionStyle style, int nNotificationIDC = 0);
	HotFilterDateRange();

	~HotFilterDateRange();

public:
	virtual void			Customize();
	virtual	BOOL			IsEmptyQuery();

	virtual	void			DefineQuery
							(
										SqlTable*	pTable,
										SqlRecord*	pRec,
								const	DataObj&	aColumn,
										CString		sOperator = _T("AND")
							);
	virtual void			PrepareQuery(SqlTable* pTable);

	virtual BOOL			OnBeforeBatchExecute();

	virtual void			OnResetCriteria();
	virtual void			ManageReadOnly();

	// read-only access to range member variables
	const DataBool&			GetRadioAll()	const { return m_bAll; }
	const DataBool&			GetRadioRange()	const { return m_bRange; }

	void					SetRangeAll(BOOL bAll) { m_bAll = bAll; m_bRange = !bAll; }

	const DataDate&			GetRangeFrom()	const { return m_RangeFrom; }
	const DataDate&			GetRangeTo()	const { return m_RangeTo; }

	void					SetRangeFrom(const DataDate& d) { m_RangeFrom = d; }
	void					SetRangeTo(const DataDate& d) { m_RangeTo = d; }

	void					SetRangeMin(const DataDate& d) { m_RangeMin = d; }
	void					SetRangeMax(const DataDate& d) { m_RangeMax = d; }

	void					SetDefaultCurrentMonth(BOOL b) { m_bDefaultCurrentMonth = b; }

	void					SetRadioAllReadOnly(BOOL bReadOnly) { m_bAll.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioRangeReadOnly(BOOL bReadOnly) { m_bRange.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioFromReadOnly(BOOL bReadOnly) { m_RangeFrom.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioToReadOnly(BOOL bReadOnly) { m_RangeTo.SetAlwaysReadOnly(bReadOnly); }

protected:
	virtual CRuntimeClass*	GetDataObjClass() const;

private:
	void OnFromChanged();
	void OnToChanged();

private:
	DataBool		m_bAll;
	DataBool		m_bRange;

	DataDate		m_RangeFrom;
	DataDate		m_RangeTo;

	DataDate		m_RangeMin;
	DataDate		m_RangeMax;

	BOOL			m_bDefaultCurrentMonth;
	SelectionStyle	m_SelectionStyle;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterIntRange		
//////////////////////////////////////////////////////////////////////////////////////////
// Allow the user to select all or a range of integers
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterIntRange : public HotFilterObj
{
	friend class CHotFilterDateRangeWithSelectionTile;
	friend class CHotFilterRangeDateTile;

	DECLARE_DYNCREATE(HotFilterIntRange)
	DECLARE_MESSAGE_MAP()

public:
	enum SelectionStyle { SIMPLE, WITH_ALL_SELECTION };

public:
	HotFilterIntRange(EHotFilterType type, CAbstractFormDoc* pDocument, HotFilterManager* pHotFilterManager,
		SelectionStyle style, int nNotificationIDC = 0);
	HotFilterIntRange();

	~HotFilterIntRange();

public:
	virtual void			Customize();
	virtual	BOOL			IsEmptyQuery();

	virtual	void			DefineQuery
	(
		SqlTable*	pTable,
		SqlRecord*	pRec,
		const	DataObj&	aColumn,
		CString		sOperator = _T("AND")
	);
	virtual void			PrepareQuery(SqlTable* pTable);

	virtual BOOL			OnBeforeBatchExecute();

	virtual void			OnResetCriteria();
	virtual void			ManageReadOnly();

	// read-only access to range member variables
	const DataBool&			GetRadioAll()	const { return m_bAll; }
	const DataBool&			GetRadioRange()	const { return m_bRange; }

	void					SetRangeAll(BOOL bAll) { m_bAll = bAll; m_bRange = !bAll; }

	const DataInt&			GetRangeFrom()	const { return m_RangeFrom; }
	const DataInt&			GetRangeTo()	const { return m_RangeTo; }

	void					SetRangeFrom(const DataInt& d) { m_RangeFrom = d; }
	void					SetRangeTo	(const DataInt& d) { m_RangeTo = d; }

	void					SetRangeMin(const DataInt& d) { m_RangeMin = d; }
	void					SetRangeMax(const DataInt& d) { m_RangeMax = d; }

	void					SetRadioAllReadOnly(BOOL bReadOnly) { m_bAll.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioRangeReadOnly(BOOL bReadOnly) { m_bRange.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioFromReadOnly(BOOL bReadOnly) { m_RangeFrom.SetAlwaysReadOnly(bReadOnly); }
	void					SetRadioToReadOnly(BOOL bReadOnly) { m_RangeTo.SetAlwaysReadOnly(bReadOnly); }

protected:
	virtual CRuntimeClass*	GetDataObjClass() const;

private:
	void OnFromChanged();
	void OnToChanged();

private:
	DataBool		m_bAll;
	DataBool		m_bRange;

	DataInt			m_RangeFrom;
	DataInt			m_RangeTo;

	DataInt			m_RangeMin;
	DataInt			m_RangeMax;
	SelectionStyle	m_SelectionStyle;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterCombo		
//////////////////////////////////////////////////////////////////////////////////////////
// Use a MSStrCOmbo to allow the user to pick a list of elements
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterCombo : public HotFilterObj
{
	DECLARE_DYNCREATE(HotFilterCombo)

public:
	HotFilterCombo
	(
		EHotFilterType type, CAbstractFormDoc*	pDocument, HotFilterManager* pHotFilterManager,
		CRuntimeClass*		pHKLClass,
		int					nNotificationIDC = 0
	);
	HotFilterCombo();
	~HotFilterCombo();

public:
	virtual BOOL	IsEmptyQuery();

	virtual void	PrepareQuery
	(
		SqlTable*	pTable
	);

	virtual void	OnResetCriteria();
	virtual void	ManageReadOnly();

	void			DefineQuery
					(
								SqlTable*	pTable,
								SqlRecord*	pRec,
						const	DataObj&	aColumn,
								CString		sOperator = _T("AND")
					);

	HotKeyLink*		GetHKLMSStrCombo() { return m_pHKLMSStrCombo; }
	void			AttachMSStrCombo(CMSStrCombo* pMSStrCombo);
	void			DoMSStrComboChanged();

protected:
	virtual CRuntimeClass*	GetDataObjClass() const;
	virtual	void			OnInitItemsList();

private:
	virtual void			CreateHotlinks();

public:
	DataStr		m_MSInputString;

protected:
	HotKeyLink*			m_pHKLMSStrCombo;
	CMSStrCombo*		m_pCtrlMSStrCombo;
	int					m_nHKLLinkedColumnIdx;
};

//////////////////////////////////////////////////////////////////////////////////////////
//								HotFilterDateRange		
//////////////////////////////////////////////////////////////////////////////////////////
// Allow the user to select all or a range of dates
//--------------------------------------------------------------------------------------
class TB_EXPORT HotFilterArray : public HotFilterObj
{
	DECLARE_DYNAMIC(HotFilterArray)

public:
	HotFilterArray();

public:
	virtual	BOOL			IsEmptyQuery() { return TRUE; }

	virtual	void			DefineQuery
							(
								SqlTable*,
								SqlRecord*,
								const	DataObj&
							) {}
	virtual void			PrepareQuery(SqlTable*) {}

	virtual void			OnResetCriteria() {}
	virtual void			ManageReadOnly() {}

protected:
	virtual CRuntimeClass*	GetDataObjClass() const { return NULL; }
};

#include "endh.dex"
