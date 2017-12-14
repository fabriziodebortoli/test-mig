#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGes\DBT.h>
#include <TbGes\ExtDoc.h>
#include <TbGes\ExtDocClientDoc.h>
#include <TbGes\Tabber.h>

#include "DSTables.h"

#include "beginh.dex"

#define USE_VALIDATION true

class CDataSynchronizerWrapper;
class CDNotification;
class CDataSynchroPane;
class CSynchroProviderCombo;

/*
//////////////////////////////////////////////////////////////////////////////
//             class DBTSynchronizationInfo declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTSynchronizationInfo : public DBTSlave
{
	DECLARE_DYNAMIC(DBTSynchronizationInfo)
	
public:
	DBTSynchronizationInfo	(CRuntimeClass*, CAbstractFormDoc*);

public:
	CAbstractFormDoc*			GetDocument			() const { return (CAbstractFormDoc*) m_pDocument; }	
	CDNotification*			GetDataSynchroClientDoc	()  { return (CDNotification*)GetClientDocOwner(); }
	TDS_SynchronizationInfo*	GetSynchroInfoRec	() const { return (TDS_SynchronizationInfo*)GetRecord(); }

protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();
	virtual void		OnDisableControlsAlways();

public:
	void ReloadData();
};


//////////////////////////////////////////////////////////////////////////////
//             class DBTActionsLog declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTActionsLog : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTActionsLog)


public:
	DBTActionsLog	(CRuntimeClass*, CAbstractFormDoc*);

public:
	CAbstractFormDoc*		GetDocument			() const { return (CAbstractFormDoc*) m_pDocument; }
	CDNotification*			GetDataSynchroClientDoc	()  { return (CDNotification*)GetClientDocOwner(); }
	TEnhDS_ActionsLog*		GetActionLogRec		() const { return (TEnhDS_ActionsLog*)GetRecord(); }
	TEnhDS_ActionsLog*		GetMostRecentActionLogRec() const { return (m_pRecords && m_pRecords->GetSize() > 0) ? (TEnhDS_ActionsLog*)m_pRecords->GetAt(0) : NULL;}

protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();

	virtual void		OnDisableControlsAlways ();
	virtual void		OnPrepareAuxColumns		(SqlRecord*);
};
*/


///////////////////////////////////////////////////////////////////////////////
//						CDNotificationInfo definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class CNotificationInfo : public CObject
{
	friend class CDataSynchroNotifier;
	friend class CDNotification;

private:
	DataEnum m_SynchStatus;
	DataEnum m_ActionType;
	DataStr  m_ActionData;
	DataStr	 m_ProviderName;
	CString	m_strTableName;
	CString m_strDocNamespace;
	DataGuid m_DocGuid;
	CString	m_strOnlyForDMS;
	CString m_strIMagoConfigurations;

	SqlRecord*	m_pRecord; //serve per la validazione

public:
	CNotificationInfo(SqlRecord*);
	~CNotificationInfo();
};

///////////////////////////////////////////////////////////////////////////////
//						CDataSynchroManager definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT CDataSynchroNotifier : public CDataSynchroNotifierObj
{
	DECLARE_DYNAMIC(CDataSynchroNotifier)

private:
	CAbstractFormDoc*			m_pDocument;
	Array						m_NotificationInfoArray;
	CDataSynchronizerWrapper*   m_pDataSynchronizer;
	CString						m_strInsertCmd;

	SqlConnection*				m_pSqlConnection;
	SqlSession*					m_pSqlSession;

	SqlTable*					m_pLogTable;
	//TUDS_SynchronizationInfo*	m_pTUSynchroInfo;	//mi serve per aggiornare lo stato di synchro
	
	
public:
	CDataSynchroNotifier(CAbstractFormDoc*, CDataSynchronizerWrapper*, SqlConnection*);
	~CDataSynchroNotifier();

public:
	void AddNotificationInfo(CNotificationInfo*);
	BOOL ValidateNotification(CNotificationInfo* pNotificationInfo);

	virtual void NotifiyToDataSynchronizer();
	virtual void RemoveNotifications();
	
	void SetInfo(CDataSynchronizerWrapper* pDataSynchronizer);

private:
	DataLng InsertLogAction(CNotificationInfo*);
};



///////////////////////////////////////////////////////////////////////////////
//						DMSEventKey definition
// per la gestione delle chiavi collection/allegati
///////////////////////////////////////////////////////////////////////////////
//
class DMSEventKey : public CObject
{
public:
	DMSEventKey(int key) :  m_EventKey(key) { };

public:
	int m_EventKey;
};

///////////////////////////////////////////////////////////////////////////////
//						DMSSynchroManager definition
// per la gestione della sincronizzazione degli allegati
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT DMSSynchroManager
{
private:
	CAbstractFormDoc* m_pDocument;
	BOOL m_bNeedToSynchroCollection;
	BOOL m_bCollectionChecked;

public:
	DMSSynchroManager(CAbstractFormDoc* pDocument);

public:
	void SetCollectionSynchronized() { m_bCollectionChecked = TRUE; m_bNeedToSynchroCollection = FALSE; }

public:
	BOOL NeedToSynchroCollection();
	CString GetXMLSynchroData(DMSEventTypeEnum eDMSEventType, Array* eventKeys);
	CString GetXMLSynchroData(DMSEventTypeEnum eventType, int pEventKey);
};


///////////////////////////////////////////////////////////////////////////////
//						CDNotification definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT CDNotification : public CClientDoc
{
	DECLARE_DYNCREATE(CDNotification)

private:
	CString						m_strDocNamespace;	
	DataStr						m_FirstValidProvider;

	SqlConnection*				m_pSqlConnection;

	CDFilterManager*			m_pCDSynchroFilter; //clientdoc che gestisce i filtri di sincronizzazione
//	TRDS_SynchronizationInfo*	m_pTRSynchroInfo;	//mi serve per leggere lo stato di synchro
	
	CStringArray				m_arNeedMassiveSynchroProviders; //elenco dei provider che necessitano di una massiva (questa per non andare a leggere ogni volta l'informazione sul database, lo leggo solo nell'OnAttachData)

	CDataSynchronizerWrapper*	m_pDataSynchronizer;

	DMSSynchroManager*			m_pDMSSynchroMng;
	CDataSynchroPane*			m_pDataSynchroPane;
	CBCGPAutoHideToolBar*		m_pAutoHideBar;

	CSynchroProviderCombo*		m_pProviderCombo;
	TRDS_ValidationInfo*		m_pTRValidationInfo;
//	TRDS_SynchronizationInfo*	m_pTRSynchronizationInfo;
//	TRDS_SynchronizationInfo*	m_pTRSynchronizationInfoDMS;
	CBaseTileDialog*			m_pValidationTileDlg;
//	CBaseTileDialog*			m_pHistoryTileDlg;

	//per la gestione delle notifiche
	Array						m_arNotifications;

public:
//	DBTActionsLog*				m_pDBTActionsLog;	
//	DBTSynchronizationInfo*		m_pDBTSynchroInfo;

	DataStr						m_CurrProviderName;
	DataStr						m_SynchStatusPicture;
	DataStr						m_SynchStatus;
	DataStr						m_SynchWorker;
	DataDate					m_SynchDate;
	DataStr						m_SynchDirection;
	DataStr						m_SynchMsg;
	SqlSession*					m_pSqlSession;
	//add msg


	DataStr						m_ValidationStatus;
	DataStr						m_ValidationStatusPicture;
	DataStr						m_SynchStatusHints;
	DataBool					m_bOpenDockPane;
	
	TCHAR						m_strImgOk		[512];
	TCHAR						m_strImgWait	[512];
	TCHAR						m_strImgError	[512];
	TCHAR						m_strImgExcluded[512];
	TCHAR						m_strDMS[512];

	DataBool					m_bImagoStudioRuntimeInstalled;
	DataBool					m_bDataSynchroEnabled;

public:
	CDNotification();
	~CDNotification();
	
private:
	CAbstractFormDoc* GetServerDoc() { return (CAbstractFormDoc*) m_pServerDocument; };	
	DataLng InsertLogAction(const DataEnum& synchStatus, const DataEnum& actionType, const DataStr& actionData, const DataStr& providerName, CSynchroDocInfo* pDocInfo);
	BOOL NotifyAction(const DataEnum& actionType, const DataStr& actionData);
	CAbstractFormDoc* GetFirsDocument(CAbstractFormDoc* pDoc) const;
	void AddNotificationInfo(CNotificationInfo*);
	BOOL NotifyEasyAttachmentAction(const DataEnum& actionType, const DataStr& actionData);
	void ManageJsonVariables();
	void EnableAllTileControls(CBaseTileDialog* pTile, BOOL bEnable);
	void ShowTile(CBaseTileDialog* pTile, BOOL bShow);
	CBaseTileDialog* GetTile(UINT nIDD);

	CString GetValidationStatus				();
	void	DeleteFromValidationFKToFix		();
	void	NotifyDeleteRecordForValidation	();
	void	SetCollapsedValidationTile		(BOOL bCollapsed);
	void	SetCollapsedHistoryTile			(BOOL bCollapsed);

	void	ParseXMLLogs					(const CString strProviderName, CString XmlLog, CString& strSynchStatusPicture, CString& strSynchStatusMsg);
public:
	SqlRecord*					GetMasterRec()			{ return GetServerDoc() && GetServerDoc()->GetMaster() ?  GetServerDoc()->GetMaster()->GetRecord() : NULL; };
//	TEnhDS_ActionsLog*			GetMostRecentActionLogRec() const { return m_pDBTActionsLog->GetMostRecentActionLogRec(); }
	BOOL						ProviderNeedMassiveSynchro(const CString& strProviderName) const;

	void	ReloadSynchronizationInfo		();
	void	DoOpenDockPanel					(BOOL bOpen = TRUE);
	void	DoReloadSynchroInfo				(BOOL bFromTimer = FALSE);
	void	SetSynchroData					(BOOL bFromTimer = FALSE);
	void	SetValidationData				(BOOL bFromForceValidation = FALSE);
	
protected:
	virtual BOOL OnPrepareAuxData			();
	virtual BOOL OnAttachData				();	
	virtual BOOL OnOkTransaction			();
	virtual	BOOL OnExtraNewTransaction		();
	virtual	BOOL OnExtraEditTransaction		();
	virtual	BOOL OnExtraDeleteTransaction	();
	
	//Notifiche di EasyAttachment
	virtual void OnDMSEvent(DMSEventTypeEnum eventType, int eventKey);

	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForEdit		(); 
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForDelete	();

	virtual void Customize					();
	virtual BOOL OnShowStatusBarMsg			(CString& sMsg);

public:
	//{{AFX_MSG(CDNotification)
	//afx_msg void OnOpenDataSynchroView			();
	afx_msg void OnForceSynchronization			();
	afx_msg void OnForceValidation				();
	afx_msg void OnReloadSynchroInfo			();
	afx_msg void OnProviderChanged				();
	afx_msg void OnCopyMsg						();
	afx_msg void OnCopyMsgHints					();
	afx_msg void OnUpdateOpenDataSynchroView	(CCmdUI*);
	afx_msg void OnUpdateForceSynchronization	(CCmdUI*);	
	afx_msg void OnUpdateForceValidation		(CCmdUI*);	
	afx_msg void OnUpdateRefresh				(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};


#include "endh.dex"