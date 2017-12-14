#pragma once 

#include <TbGes\TileManager.h>
#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TbGenlib\BaseTileManager.h>
#include <TbGes\ExtDocAbstract.h>
#include <TbGes\ExtDocView.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>
#include <TbGes\BEColumnInfo.H>


#include "DSTables.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DSMonitor;
class CTBLinearGaugeCtrl;

#define CHECK_DS_MONITOR_TIMER 300
#define CHECK_DS_GAUGE_TIMER 301

/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_SynchroInfoDocSummary definition		   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_SynchroInfoDocSummary: public TDS_SynchronizationInfo
{
	DECLARE_DYNCREATE(TEnhDS_SynchroInfoDocSummary) 
	
public:
	DataStr	l_SynchStatusBmp;
	DataStr	l_NoDocStatusSynchro;
	DataStr	l_NoDocStatusError;
			
public:
	TEnhDS_SynchroInfoDocSummary(BOOL bCallInit = TRUE);
	
public:
	virtual void BindRecord();	
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTSynchroInfoMonitor declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTSynchroInfoMonitor : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSynchroInfoMonitor)
	friend class DSMonitor;

private:
	DataBool m_bDelta;
	DataDate m_startSynchroDate;

public:
	DBTSynchroInfoMonitor(CRuntimeClass*, CAbstractFormDoc*, DataBool bDelta, DataDate startSynchroDate);

public:
	TEnhDS_SynchronizationInfo*	GetSynchroInfoRec	() const { return (TEnhDS_SynchronizationInfo*) GetRecord(); }
	DSMonitor*					GetDocument			() const { return (DSMonitor*) 	m_pDocument; }

protected:
	virtual	void OnDefineQuery						();
	virtual	void OnPrepareQuery						();
	virtual void OnPrepareAuxColumns				(SqlRecord*);

public:
	void ReloadData();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTSynchroMonitorDocSummay declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTSynchroMonitorDocSummay : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSynchroMonitorDocSummay)
	
public:
	DBTSynchroMonitorDocSummay(CRuntimeClass*, CAbstractFormDoc*);

public:
	TEnhDS_SynchroInfoDocSummary*	GetSynchroInfoRecoveryErrRec	() const { return (TEnhDS_SynchroInfoDocSummary*) GetRecord(); }
	TEnhDS_SynchroInfoDocSummary*	GetOldSynchroInfoRecoveryErrRec	() const { return (TEnhDS_SynchroInfoDocSummary*) GetOldRecord(); }
	DSMonitor*						GetDocument						() const { return (DSMonitor*) 	m_pDocument; }

protected:
	virtual	void OnDefineQuery	() {};
	virtual	void OnPrepareQuery	() {};
};


/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_SynchroInfoDocSummary definition		   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT VSynchroInfoDocSummary : public VSynchronizationInfo
{
	DECLARE_DYNCREATE(VSynchroInfoDocSummary)

public:
	DataStr	l_SynchStatusBmp;
	DataStr	l_NoDocStatusSynchro;
	DataStr	l_NoDocStatusError;
	DataStr	l_NsWithFlows;
	DataStr l_Flow;

public:
	VSynchroInfoDocSummary(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTVSynchroInfoMonitor declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTVSynchroInfoMonitor : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTVSynchroInfoMonitor)
	friend class DSMonitor;

private:
	DataBool m_bDelta;
	DataDate m_startSynchroDate;

public:
	DBTVSynchroInfoMonitor(CRuntimeClass*, CAbstractFormDoc*, DataBool bDelta, DataDate startSynchroDate);

public:
	VSynchronizationInfo*		GetSynchroInfoRec() const { return (VSynchronizationInfo*)GetRecord(); }
	DSMonitor*					GetDocument() const { return (DSMonitor*)m_pDocument; }

protected:
	virtual	void OnDefineQuery() {};
	virtual	void OnPrepareQuery() {};
	virtual void OnPrepareAuxColumns(SqlRecord*);

public:
	void ReloadData();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTVSynchroMonitorDocSummay declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTVSynchroMonitorDocSummay : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTVSynchroMonitorDocSummay)

public:
	DBTVSynchroMonitorDocSummay(CRuntimeClass*, CAbstractFormDoc*);
	DBTVSynchroMonitorDocSummay();

public:
	virtual void 		Init();
	virtual	void		OnDefineQuery();
	virtual	void		OnPrepareQuery();
	virtual DataObj*	OnCheckPrimaryKey(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey(int /*nRow*/, SqlRecord*);
	virtual DataObj*	GetDuplicateKeyPos(SqlRecord* pRec);
public:
	VSynchroInfoDocSummary*	GetSynchroInfoRecoveryErrRec() const { return (VSynchroInfoDocSummary*)GetRecord(); }
	VSynchroInfoDocSummary*	GetOldSynchroInfoRecoveryErrRec() const { return (VSynchroInfoDocSummary*)GetOldRecord(); }
	DSMonitor*				GetDocument() const { return (DSMonitor*)m_pDocument; }
};

//////////////////////////////////////////////////////////////////////////////
//             class DSMonitor declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class DSMonitor : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DSMonitor)

public:
	DSMonitor();
	~DSMonitor();

public: 
	DataBool							m_bFromBatch;
	DataBool							m_bAutoRefresh;
	DataBool							m_bShowOnlyDocWithErr; // La gestonione "Recovery Errors" è gestita solo per CRM Infinity

	DataStr								m_DocToSynch; 
	DataStr								m_ProviderName;

	DataBool							m_bIsMassiveSynchronizing;
	DataInt								m_nSynchronizationCounter;
	DataBool							m_bSynchronizationEnded;
	DataBool							m_bSynchronizationStarted;
	DataBool							m_bNeedMassiveSynchro;
	DataBool							m_bErrorsOccurred;
	
	DataBool							m_SynchStatusAll;
	DataEnum							m_SynchStatus; 
	
	DataBool							m_SynchDateAll;
	DataBool							m_SynchDateSel;
	DataBool							m_Pause;
	DataBool							m_Abort;
	DataDate							m_SynchDateFrom;
	DataDate							m_SynchDateTo;

	DataStr								m_InboundPic;
	DataStr								m_OutboundPic;
	DataStr								m_StatusOkPic;
	DataStr								m_StatusWaitPic;
	DataStr								m_StatusErrorPic;
	DataStr								m_StatusExcludedPic;
	DataStr								m_StatusWarningPic;
	DataDbl								m_nValueGauge;
	DataStr								m_GaugeDescription;
	DataStr								m_PictureStatus;
	DataDbl								m_nGaugeUpperRange;
	CLabelStatic*						m_pGaugeLabel;
	CTBLinearGaugeCtrl*					m_pGauge;

	CSynchroDocInfoArray*				m_pDocumentsToMonitor;
	CSynchroDocInfoArray*				m_pOnlyDocWithErr;
	MassiveSynchroInfo*					m_pMassiveSynchroInfo;
	DataDate							m_StartSynchDateForDelta;
	
	DBTSynchroInfoMonitor*				m_pDBTDetail;
	DBTSynchroMonitorDocSummay*			m_pDBTDetailDocSummary;

	DBTVSynchroInfoMonitor*				m_pDBVTDetail;
	DBTVSynchroMonitorDocSummay*		m_pDBVDetailDocSummary;


	TRDS_ActionsLog*					m_pTRDS_ActionsLog;
	RRDS_SynchronizationInfoByStatus*	m_pRRDS_SynchronizationInfoByStatus;

	CTilePanel*							m_pResultsTilePanel;
	CBaseTileDialog*					m_pSummaryTile;
	CBaseTileDialog*					m_pMonitorTile;

	DataBool							m_bIsActivatedCRMInfinity;
	DataBool							m_bDelta;
	DataBool							m_bFirstTimeRunning;
	DataBool							m_bIsActivatedImagoStudio;

	DataInt								m_nDetailPage;
	
	// For pagination
	DataBool							m_bNamespaceChanged;
	DataInt								m_LineRequested;

	DataStr								m_nDetailPageStr;
	DataInt								m_nPageTot;
	CString								m_strRecoveryGuid;

private:
	UINT	m_MonitorRefresh;
	UINT	m_GaugeRefresh;

public:	
	TDS_SynchronizationInfo* GetSynchroInfoRec() const;

private:
			void StartTimer						();
			void EndTimer						();
			void StartGaugeTimer				();
			void EndGaugeTimer					();
			CBaseTileDialog* GetTile			(UINT nIDD);
			void ManageJsonVars					();
			void InitGauge						();
			void UpdateGauge					(double nStep = 1.0);
			void UpdateGauge					(double nStep = 1.0, BOOL bIsIndeterminate = FALSE);
			void UpdateGaugeIndeterminate		();
			void CompleteGauge					();
			void SetFiltersEnable				(BOOL bSet);
			void GetResultsTileDialog			();
			DataDate GetStartSynchDateForDelta	();
			void SetStartSynchDateForDelta		();

protected:
	virtual BOOL CanRunDocument 		();
	virtual	BOOL InitDocument			();
	virtual void DeleteContents			();
	virtual BOOL OnAttachData 			();
	virtual	BOOL OnOpenDocument			(LPCTSTR);
	virtual void DisableControlsForBatch();
	virtual void OnCloseDocument		();
	//manage json
	virtual	BOOL OnGetToolTipProperties (CBETooltipProperties* pTooltip);
	virtual void OnParsedControlCreated	(CParsedCtrl* pCtrl);
	virtual void CustomizeBodyEdit		(CBodyEdit* pBE);
	virtual CString OnGetCaption		(CAbstractFormView* pView);
		
public:
	void GetDecodingInfo			(const DataGuid& tbGuid, DataStr& recordKey, DataStr& recordDescription);
	void CheckSynchroDate			();
	BOOL InPause					();
	void DoSynchStatusAllChanged	();
	void SetCollapsedResultsPanel	();
	BOOL ErrorsOccurred				();
	void LoadDocSummaryDBT			();
	void SynchroDocSummaryManagement();
	void DoGaugeManagement			();
	void SetMonitorParameters		(BOOL bErrorsOccurred = FALSE);
	void StartRecoveryErr			();
	void DoRefresh					();
	void DoOnTimer					();
	void DoGaugeRefresh				();
	void DoOnGaugeTimer				();
	void ExpandDetail				();
	
protected:	
	// Generated message map functions
	//{{AFX_MSG(DSMonitor)
	afx_msg	void OnSynchStatusAllChanged			();
	afx_msg	void OnDocNamespaceChanged				();
	afx_msg	void OnProviderChanged					();
	afx_msg	void OnSynchStatusChanged				();
	afx_msg	void OnSynchDateSelChanged				();
	afx_msg	void OnSynchDateFromEntered				();
	afx_msg	void OnSynchDateToEntered				();
	afx_msg	void OnShowOnlyDocWithErrChanged		();
	afx_msg void OnAutomaticRefreshChanged			();
	afx_msg void OnErrorsRecoveryClick				();
	afx_msg void OnMonitorRefresh					();
	afx_msg void OnMonitorSummaryRowChanged			();
	afx_msg void OnMonitorDetailOpenDocument		();
	afx_msg void OnMonitorDetailCopyMessage			();	
	afx_msg void PauseMassiveSynchro				();
	afx_msg void ContinueMassiveSynchro				();
	afx_msg void AbortMassiveSynchro				();
	afx_msg void OnErrorsRecoveryUpdate				(CCmdUI* pCmdUI);
	afx_msg void PauseMassiveSynchroEnable			(CCmdUI * pCmdUI);
	afx_msg void ContinueMassiveSynchroEnable		(CCmdUI * pCmdUI);
	afx_msg void AbortMassiveSynchroEnable			(CCmdUI * pCmdUI);
	afx_msg void OnMonitorRefreshUpdate				(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDetailOpenDocument			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDetailCopyMessage			(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

