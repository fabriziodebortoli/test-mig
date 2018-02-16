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

#define CHECK_VALIDATION_MONITOR_TIMER 300
#define CHECK_VALIDATION_GAUGE_TIMER 301

class DValidationMonitor;
class CTBLinearGaugeCtrl;
class CValidationMonitorFKToFixPanel;

/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_ValidationInfoDetail definition				   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_ValidationInfoDetail : public TDS_ValidationInfo
{
	DECLARE_DYNCREATE(TEnhDS_ValidationInfoDetail)
	
public:
	DataStr l_Code;
	DataStr l_Description;
	DataStr l_FormattedMsgError;
	DataStr	l_WorkerDescri;
			
public:
	TEnhDS_ValidationInfoDetail(BOOL bCallInit = TRUE);
	
public:
	virtual void BindRecord();	
};

/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_ValidationInfoSummary definition				   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_ValidationInfoSummary : public TDS_ValidationInfo
{
	DECLARE_DYNCREATE(TEnhDS_ValidationInfoSummary)
	
public:
	DataStr	l_NoErrors;
			
public:
	TEnhDS_ValidationInfoSummary(BOOL bCallInit = TRUE);
	
public:
	virtual void BindRecord();	
};

/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_ValidationFKToFixSummary definition			   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_ValidationFKToFixSummary : public TDS_ValidationFKtoFix
{
	DECLARE_DYNCREATE(TEnhDS_ValidationFKToFixSummary)
	
public:
	DataStr	l_NoFKToFix;
			
public:
	TEnhDS_ValidationFKToFixSummary(BOOL bCallInit = TRUE);
	
public:
	virtual void BindRecord();	
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTValidationInfoMonitor declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTValidationInfoMonitor : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTValidationInfoMonitor)
	
public:
	DBTValidationInfoMonitor(CRuntimeClass*, CAbstractFormDoc*);

public:
	TEnhDS_ValidationInfoDetail*	GetValidationInfoRecord		() const { return (TEnhDS_ValidationInfoDetail*) GetRecord(); }
	DValidationMonitor*				GetDocument					() const { return (DValidationMonitor*) m_pDocument; }

protected:
	virtual	void OnDefineQuery									();
	virtual	void OnPrepareQuery									();
	virtual void OnPrepareAuxColumns							(SqlRecord*);

public:
	void ReloadData();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTValidationMonitorDocSummary declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTValidationMonitorDocSummary : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTValidationMonitorDocSummary)
	
public:
	DBTValidationMonitorDocSummary(CRuntimeClass*, CAbstractFormDoc*);

public:
	TEnhDS_ValidationInfoSummary*	GetValidationInfoRecord	()			const { return (TEnhDS_ValidationInfoSummary*) GetRecord(); }
	TEnhDS_ValidationInfoSummary*	GetDetail				(int nRow)	const { return (TEnhDS_ValidationInfoSummary*)GetRow(nRow); } 
	DValidationMonitor*				GetDocument				()			const { return (DValidationMonitor*) m_pDocument; }
	
protected:
	virtual	void OnDefineQuery	() {};
	virtual	void OnPrepareQuery	() {};
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTValidationFKToFixDetail declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTValidationFKToFixDetail : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTValidationFKToFixDetail)
	
public:
	DBTValidationFKToFixDetail(CRuntimeClass*, CAbstractFormDoc*);

public:
	TDS_ValidationFKtoFix*	GetValidationFKtoFixRecord	() const { return (TDS_ValidationFKtoFix*) GetRecord(); }
	DValidationMonitor*		GetDocument					() const { return (DValidationMonitor*) m_pDocument; }

protected:
	virtual	void OnDefineQuery	();
	virtual	void OnPrepareQuery	();

public:
	void ReloadData();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTValidationFKToFixSummary declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DBTValidationFKToFixSummary : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTValidationFKToFixSummary)
	
public:
	DBTValidationFKToFixSummary(CRuntimeClass*, CAbstractFormDoc*);

public:
	TEnhDS_ValidationFKToFixSummary*	GetValidationFKtoFixRecord	()			const	{ return (TEnhDS_ValidationFKToFixSummary*) GetRecord(); }
	TEnhDS_ValidationFKToFixSummary*	GetDetail					(int nRow)	const 	{ return (TEnhDS_ValidationFKToFixSummary*)GetRow(nRow); } 
	DValidationMonitor*					GetDocument					()			const	{ return (DValidationMonitor*) m_pDocument; }
	
protected:
	virtual	void OnDefineQuery	() {};
	virtual	void OnPrepareQuery	() {};
};

//////////////////////////////////////////////////////////////////////////////
//             class DValidationMonitor declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class DValidationMonitor : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DValidationMonitor)

public:
	DValidationMonitor				();
	~DValidationMonitor				();
	
public:
	DBTValidationInfoMonitor*		m_pDBTDetail;
	DBTValidationMonitorDocSummary*	m_pDBTDetailDocSummary;
	DBTValidationFKToFixDetail*		m_pDBTFKToFixDetail;
	DBTValidationFKToFixSummary*	m_pDBTFKToFixSummary;

	MassiveSynchroInfo*				m_pMassiveSynchroInfo;
	CSynchroDocInfoArray*			m_pDocumentsToMonitor;

	DataBool						m_bIsActivatedCRMInfinity;

	DataBool						m_bFromBatch;
	DataBool						m_bAutoRefresh;
	DataBool						m_bIsMassiveValidating;
	DataInt							m_nValidationCounter;
	DataBool						m_bValidationEnded;
	DataBool						m_bValidationStarted;
	DataBool						m_bNeedMassiveValidation;
	DataBool						m_bErrorsOccurred;

	DataBool						m_bAllDate;
	DataBool						m_bSelectionDate;
	DataDate						m_DateFrom;
	DataDate						m_DateTo;
	DataStr							m_DocNamespace;
	DataStr							m_ProviderName;

	DataStr							m_DocNamespaceFKToFix;
	
	DataDbl							m_nValueGauge;
	DataStr							m_GaugeDescription;
	DataStr							m_PictureStatus;
	DataDbl							m_nGaugeUpperRange;
	CLabelStatic*					m_pGaugeLabel;
	CTBLinearGaugeCtrl*				m_pGauge;
	CBaseTileDialog*				m_pSummaryTile;
	CBaseTileDialog*				m_pMonitorTile;
	CTilePanel*						m_pResultsTilePanel;
	CValidationMonitorFKToFixPanel* m_pViewFKToFixPanel;
	
private:
	UINT	m_MonitorRefresh;
	UINT	m_GaugeRefresh;
	UINT	m_IDTileSummary;
	UINT	m_IDTileDetail;

protected:
	void	ValidationDocSummaryManagement	();
	BOOL	ErrorsOccurred					();
	void	StartTimer						();
	void	EndTimer						();
	void	StartGaugeTimer					();
	void	EndGaugeTimer					();
	void	GetResultsTileDialog			();

public:
	void	DoOnTimer						();
	void	GetDecodingInfo					(const DataGuid& tbGuid, DataStr& recordKey, DataStr& recordDescription);
	void	SetTileStatus					();
	void	ExpandDetail					();
	void	DoOpenDockPanel					(BOOL bOpen = TRUE);
	void	DoGaugeManagement				();
	void	DoOnGaugeTimer					();
	void	DoReloadErrorToFixPanel			();

protected:
	virtual void	OnParsedControlCreated	(CParsedCtrl* pCtrl);
	virtual void	CustomizeBodyEdit		(CBodyEdit* pBE);
	virtual CString OnGetCaption			(CAbstractFormView* pView);
	virtual void	DeleteContents			();
	virtual BOOL	OnAttachData			();
	virtual void	DoReloadData			();
	virtual void	DoRefresh				();
	virtual void    DoGaugeRefresh			();
	virtual void	DisableControlsForBatch	();
	virtual	BOOL	OnOpenDocument			(LPCTSTR);
	virtual BOOL	CanRunDocument 			();
	virtual	BOOL	InitDocument			();
	virtual void	OnCloseDocument			();
	virtual void	OnPrepareAuxData		(CTileDialog * pTileDialog);
protected:
	void LoadDocSummaryDBT					();
	void LoadKFToFixSummaryDBT				();
	void CheckValidationDate				();
	void ManageJsonVars						();
	void InitGauge							();
	void UpdateGauge						(double nStep = 1.0);
	void UpdateGauge						(double nStep = 1.0, BOOL bIsIndeterminate = FALSE);
	void UpdateGaugeIndeterminate			();
	void CompleteGauge						();
	void SetFiltersEnable					(BOOL bSet);

protected:	
	//{{AFX_MSG(DValidationMonitor)
	afx_msg void OnMonitorSummaryRowChanged				();
	afx_msg void OnDocNamespaceChanged					();
	afx_msg	void OnSelectionDateChanged					();
	afx_msg	void OnDateFromChanged						();
	afx_msg	void OnDateToChanged						();
	afx_msg void OnMonitorRefresh						();
	afx_msg void OnValidationMonitorSummaryRowChanged	();
	afx_msg void OnValidationFKToFixSummaryRowChanged	();
	afx_msg void OnValidationMonitorDetailOpenDocument	();
	afx_msg void OnValidationFKToFixAdd					();
	afx_msg void OnValidationMonitorDetailCopyMessage	();	
	afx_msg void OnValidationFKToFixDetailCopyValue		();	
	afx_msg void OnMonitorRefreshUpdate					(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDetailOpenDocument				(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDetailCopyMessage				(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFKToFixDetailCopyValue			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateValidationFKToFixAdd			(CCmdUI* pCmdUI);
	//}}AFX_MSG	
	DECLARE_MESSAGE_MAP()
};

