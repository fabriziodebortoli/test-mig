#pragma once

// Components
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

// TaskBuilder
#include <TbGenLib\TBLinearGauge.h>
#include <tbges\bodyedittree.h>

#include "TBActivityDocument.hjson"

#include "beginh.dex"

class  CTilePanel;
struct CTBActivityLegend;


enum E_ACTIVITYTYPE { 
	ACTIVITY_EMPTY,				//NOT_RESULTS
	ACTIVITY_GRID,				//CONTENT, BODYTREE
	ACTIVITY_GRID_SUMMARY,		//SUMMARY
	ACTIVITY_PROGRESS			//GAUGE
};

enum E_ACTIVITY_PANELACTION  { 
	ACTIVITY_COLLAPSE,			//COLLAPSE
	ACTIVITY_NOT_COLLAPSE		//NOT_COLLAPSE
};

/////////////////////////////////////////////////////////////////////////////
//			Class TDetailSummary
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TSummaryDetail : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(TSummaryDetail)

public:
	DataStr			l_LineSummaryDescription;

public:
	TSummaryDetail							(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord				();

public:
	static LPCTSTR   GetStaticName			();
};

/////////////////////////////////////////////////////////////////////////////
//			Class DBTSummaryDetail
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class DBTSummaryDetail : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSummaryDetail)

public:
	DBTSummaryDetail						(CRuntimeClass* pClass, CAbstractFormDoc* pDoc);

protected:
	virtual void		OnDefineQuery		() {}
	virtual void		OnPrepareQuery		() {}

	virtual BOOL		OnBeforeAddRow		(int nRow) { return TRUE; }
	virtual DataObj*	OnCheckPrimaryKey	(int nRow, SqlRecord* pSqlRecord) { return NULL; }
	virtual void		OnPreparePrimaryKey	(int nRow, SqlRecord* pSqlRecord) {}
};


/////////////////////////////////////////////////////////////////////////////
//					class CTBActivityFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CTBActivityFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CTBActivityFrame)

public:
	CTBActivityFrame();

protected:
	virtual BOOL OnCustomizeJsonToolBar		();
	virtual	BOOL CreateAuxObjects			(CCreateContext*	pCreateContext);

private:
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar) { return TRUE; }//metodo che va a morire => da non potterlo reimplementare nelle classi derivate
};


/////////////////////////////////////////////////////////////////////////////
//					class CTBActivityWithoutLoadDataFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CTBActivityWithoutLoadDataFrame : public CTBActivityFrame
{
	DECLARE_DYNCREATE(CTBActivityWithoutLoadDataFrame)

public:
	CTBActivityWithoutLoadDataFrame();

protected:
	virtual BOOL OnCustomizeJsonToolBar();
};
/////////////////////////////////////////////////////////////////////////////
//					class CTBActivityDocument definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CTBActivityDocument : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(CTBActivityDocument)

	friend class CTBActivityFrame;
	
public:
	CTBActivityDocument();
	virtual ~CTBActivityDocument();


protected:
	BOOL						m_bAskDeleteConfirmation;
	BOOL						m_bSelect;	//manage select/deselect
	DataDbl						m_nProgressStep;

public:
	virtual BOOL				OnFilterValidate			() { return TRUE; }
	virtual BOOL				OnLoadDBT					() { return TRUE; }
	virtual BOOL				OnBeforeLoadDBT				() { return TRUE; }
	virtual BOOL				OnAfterLoadDBT				() { return TRUE; }
	virtual BOOL				OnClearDBT					();
	virtual void				LoadStart					();
	virtual void				LoadStop					();

public:	//virtual methods exposed by CAbstractFormDoc
	virtual BOOL				OnOpenDocument				(LPCTSTR);
	virtual BOOL				CanDoBatchExecute			();
	virtual BOOL				OnAttachData				();
	virtual void				CustomizeBodyEdit			(CBodyEdit* pBE);
	virtual void				OnBEEnableButton			(CBodyEdit*, CBEButton*);
	virtual void				OnParsedControlCreated		(CParsedCtrl* pCtrl);
	virtual BOOL				BatchEnableControls			();
	virtual void				OnManageAfterBatchExecute	();

public:	//virtual methods exposed by CTBActivityDocument
	virtual void				OnRowBEResultsChanged		() {}
	virtual void				OnSelectDeselect			() {}
	virtual DBTSlaveBuffered*	GetDBT						() { return NULL; }
	virtual void				ManageExtractData			() {}
	virtual BOOL				BeforeUndoExtraction		() { return TRUE; }
	virtual void				ManageUndoExtraction		() {}
	virtual void				ManageAddData				() {}
	//manage legend
	virtual void				CustomizeLegend				(CTBActivityLegend* pLegend) {}
		
	//manage OnUpdateCommandUI - these virtuals allow you to change/extend the visibility of buttons
	virtual BOOL				DoUpdateUndoExtraction		();
	virtual BOOL				DoUpdateBatchStartStop		();
	virtual BOOL				DoUpdateAddData				();
	virtual BOOL				DoUpdateLoadDataStart		();
	
private:
	CTilePanel*					GetTilePanel				(CString sName);
	void						SetPanelCollapsed			(CTilePanel* pPanel, BOOL bSet);
	void						SetPanelEnabled				(CTilePanel* pPanel, BOOL bSet);
	void						ManagePanelsState			();
	void						EnsureExistancePanels		();
	void						DoExtractData				();
	void						DoUndoExtraction			();
	void						DoAddData					();
	CString						GetCaptionOnSelectButton	() { return (m_bSelect ? _TB("Select") : _TB("Deselect")); }
	//manage caption/tooltip on bodyedit's buttons (SOLO PER RETROCOMPATIBILITA')
	void						UpdateBEButton				(UINT idBody, UINT idButton, CString text, CString tooltip);

	BOOL						DispatchOnBeforeLoadDBT		();
	BOOL						DispatchOnLoadDBT			();
	BOOL						DispatchOnAfterLoadDBT		();
	BOOL						DispatchOnBeforeUndoExtraction();
	void						ManageDefaultFocus			();
				
public:
	void						SetHeaderTitle(DataStr sTitle, BOOL bBold = FALSE)
	{
		m_HeaderTitle			= sTitle;
		m_bHeaderTitleBold		= bBold;
	}

	void						SetHeaderSubTitle(DataStr sSubTitle, BOOL bBold = FALSE)
	{
		m_HeaderSubTitle		= sSubTitle;
		m_bHeaderSubTitleBold	= bBold;
	}

	void						Restart						();
	
	void						CustomizeFrame
		(
			BOOL					bHasLegend				= FALSE
		);

	void						CustomizeFilters
		(
			E_ACTIVITY_PANELACTION	eAction					= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE
		);

	void						CustomizeActions
		(
			E_ACTIVITY_PANELACTION		eAction					= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE,
			BOOL				bActionsAlwaysEnabled	= FALSE,
			BOOL				bActionsAsFilters		= FALSE
		);

	void						CustomizeResults
		(
			E_ACTIVITYTYPE		eResultsType			= E_ACTIVITYTYPE::ACTIVITY_EMPTY,
			BOOL				bManageSelectButton		= TRUE
		);

	void						CustomizeFooter
		(
			E_ACTIVITY_PANELACTION		eAction					= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE,
			BOOL				bFooterAlwaysEnabled	= FALSE
		);

	//manage Gauge
	void						UpdateGauge					();
	void						SetGaugeUpperRange			(DataLng nMax = 100);
	void						ClearGauge					(const DataLng& aMax, int perc = 10);
	void						SetGaugeTitle				(CString title);
	void						SetProgressStep				(DataDbl nStep) { nStep = m_nProgressStep; }

	//manage Summary
	void						AddSummaryString			(const DataStr& aSummaryString);
	void						UpdateSummaryString			(const DataStr& aSummaryString, int index = -1);
	void						ClearSummary				();
	void						RemoveAllSummary			();

	CTilePanel*					GetPanelFilters				();
	CTilePanel*					GetPanelActions				();
	CTilePanel*					GetPanelFooter				();
	
private:	//private members
	BOOL						m_bExtractingData;
	BOOL						m_bExtractData;
	BOOL						m_bAddMoreData;
	BOOL						m_bManageSelectButton;
	E_ACTIVITY_PANELACTION		m_eFiltersActionOnExtract;
	E_ACTIVITY_PANELACTION		m_eActionsActionOnExtract;
	E_ACTIVITY_PANELACTION		m_eFooterActionOnExtract;
	BOOL						m_bActionsAlwaysEnabled;
	BOOL						m_bActionsAsFilters;
	BOOL						m_bFooterCollapsibile;
	BOOL						m_bFooterAlwaysEnabled;
	E_ACTIVITYTYPE				m_eResultsType;
	CTBActivityLegend*			m_pActivityLegend;
	CTilePanel*					m_pPanelFilters;
	CTilePanel*					m_pPanelActions;
	CTilePanel*					m_pPanelFooter;
	
	CTBLinearGaugeCtrl*			m_pGauge;
	CStrStatic*					m_pGaugeLabel;
	DBTSummaryDetail*			m_pDBTSummary;
	DataDbl						m_nCurrentElement;
	DataLng						m_nGaugeRangeMax;
	DataStr						m_GaugeDescription;
	DataStr						m_HeaderTitle;
	BOOL						m_bHeaderTitleBold;
	DataStr						m_HeaderSubTitle;
	BOOL						m_bHeaderSubTitleBold;
	DataStr						m_FiltersPanelText;
	DataStr						m_ActionsPanelText;
	DataStr						m_FooterPanelText;
	long						m_nStep;
	BOOL						m_bGoToStart;
	
protected:
	//{{AFX_MSG(CActivityDocument)
	afx_msg void				OnGridRowChanged			() { OnRowBEResultsChanged(); }
	afx_msg void				OnSelectDeselectClicked		();
	afx_msg void				OnLoadDataStart				();
	afx_msg void				OnUpdateLoadDataStart		(CCmdUI*);
	afx_msg void				OnLoadPause					();
	afx_msg void				OnLoadResume				();
	afx_msg void				OnLoadStop					();
	afx_msg void				OnUpdateLoadPause			(CCmdUI*);
	afx_msg void				OnUpdateLoadResume			(CCmdUI*);
	afx_msg	void				OnUpdateLoadStop			(CCmdUI*);
	afx_msg void				OnAddData					();
	afx_msg void				OnUpdateAddData				(CCmdUI*);
	afx_msg void				OnUndoExtraction			();
	afx_msg void				OnUpdateUndoExtraction		(CCmdUI*);
	afx_msg void				OnUpdateBatchStartStop		(CCmdUI*);
	afx_msg void				OnUpdateBatchPauseResume	(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//			Struct CJsonActivityLegend
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
struct CTBActivityLegend
{
public:
	CTBActivityLegend()
		:
		m_nIDD										(0),
		m_LegendName								(_T("Legend")),
		m_LegendTitle								(_T("")),
		m_wAlignment								(CBRS_RIGHT),
		m_Size										(CSize(360, 200)),
		m_pCreateContext							(NULL)
	{
		m_pCreateContext = new CCreateContext();
	}
	~CTBActivityLegend() {
		delete m_pCreateContext;
	}
public:
	UINT					m_nIDD;
	CString					m_LegendTitle;
	DWORD					m_wAlignment;
	CSize					m_Size;
	CCreateContext*			m_pCreateContext;
	
public:
	CString					GetLegendName			() { return m_LegendName; }
	
private:
	CString					m_LegendName;

};

#include "endh.dex"
