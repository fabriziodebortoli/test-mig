#pragma once

#include "beginh.dex"
#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\parsedt.h>
#include <TbGenlib\parscbx.h>
#include <TbGes\extdoc.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\dbt.h>
#include <TbGes\BODYEDIT.H>
#include <TbOleDb\Sqltable.h>
#include <TbGeneric\CMapi.h>
#include "PostaLiteSettings.h"
#include "PostaLiteTables.h"
#include "UIPostaliteDocumentsGraphicNavigation.h"
#include "TbSenderInterface.h"

class PostaliteDocumentsGraphicNavigationStrings;
class TEnhPostaliteDocumentsGraphicNavigationDetail;
class DBTPostaliteDocumentsGraphicNavigationDetail;
class BDPostaliteDocumentsGraphicNavigation;
class CPostaliteDocumentsGraphicNavigationEdit;
class CPostaliteDocumentsGraphicNavigationFrame;

#define ADD_DETAIL_LAYOUT(Name, Value, HasHyperlink) AddDetail(PostaliteDocumentsGraphicNavigationStrings::##Name(), ##Value, ##HasHyperlink);
#define ADD_SEPARATOR_LAYOUT(Name) AddSeparator(PostaliteDocumentsGraphicNavigationStrings::##Name());
#define FIELD(Name)	PostaliteDocumentsGraphicNavigationStrings::##Name()

BEGIN_TB_STRING_MAP(PostaliteDocumentsGraphicNavigationStrings)

	TB_LOCALIZED(MESSAGE_DETAILS,			"Message details")
	TB_LOCALIZED(ENVELOPE_DETAILS,			"Envelope details")
	TB_LOCALIZED(ENVELOPE_ID,				"Envelope Id")
	TB_LOCALIZED(POSTALITE_ID,				"PostaLite Id")
	TB_LOCALIZED(MESSAGE_ID,				"Message Id")
	TB_LOCALIZED(ADDRESSE,					"Addressee")
	TB_LOCALIZED(WORKER,					"Worker")
	TB_LOCALIZED(SUBJECT,					"Subject")
	TB_LOCALIZED(DOCUMENT_FILENAME,			"Document fileName")
	TB_LOCALIZED(DOCUMENT_PAGES,			"Document pages")
	TB_LOCALIZED(DOCUMENT_SIZE,				"Document size [KB]")
	TB_LOCALIZED(SEND_AFTER,				"Send after")
	TB_LOCALIZED(STATUS,					"Status")
	TB_LOCALIZED(TOTAL_PAGES,				"Total pdf pages")
	TB_LOCALIZED(TOTAL_AMOUNT,				"Total amount [€]")
	TB_LOCALIZED(POSTAGE_AMOUNT,			"Postage amount [€]")
	TB_LOCALIZED(PRINT_AMOUNT,				"Print amount [€]")
	TB_LOCALIZED(DELIVERY_TYPE,				"Delivery type")
	TB_LOCALIZED(PRINT_TYPE,				"Print type")
	TB_LOCALIZED(ADDRESSE_DETAILS,			"Addresse details")
	TB_LOCALIZED(ADDRESS,					"Address")
	TB_LOCALIZED(CITY,						"City")
	TB_LOCALIZED(COUNTY,					"County")
	TB_LOCALIZED(COUNTRY,					"Country")
	TB_LOCALIZED(FAX,						"Fax")
	TB_LOCALIZED(ZIP,						"Zip")
	TB_LOCALIZED(ERROR_DESCRIPTION,			"Error description")
	TB_LOCALIZED(LAST_MODIFIED,				"Last modified")
	TB_LOCALIZED(MESSAGE_ALREADY_UPLOADED,	"The message cannot be modified because it's already been uploaded")

	TB_LOCALIZED(CONTEXT_MENU_RUN_ALLOT_PROCEDURE,			"Put unassigned documents into envelopes")
	TB_LOCALIZED(CONTEXT_MENU_UPDATE_SENT_LOTS_STATUS,		"Update envelopes status")
	TB_LOCALIZED(CONTEXT_MENU_SEND_MESSAGE_ALONE,			"Send document alone")
	TB_LOCALIZED(CONTEXT_MENU_SEND_IMMEDIATELY,				"Upload document to PostaLite now")
	TB_LOCALIZED(CONTEXT_MENU_SEND_ENVELOPE_NOW,			"Upload envelope to PostaLite now")
	TB_LOCALIZED(CONTEXT_MENU_DELETE_MESSAGE,				"Delete document")
	TB_LOCALIZED(CONTEXT_MENU_MESSAGE_CHANGE_DELIVERY_TYPE, "Change delivery type")
	TB_LOCALIZED(CONTEXT_MENU_MESSAGE_CHANGE_PRINT_TYPE,	"Change print type")
	TB_LOCALIZED(CONTEXT_MENU_REOPEN_CLOSED_ENVELOPE,		"Re-open closed envelope")
	TB_LOCALIZED(CONTEXT_MENU_LOT_ESTIMATE,					"Get envelope estimated costs")
	TB_LOCALIZED(CONTEXT_MENU_CLOSE_ENVELOPE,				"Close current envelope")
	TB_LOCALIZED(CONTEXT_MENU_REMOVE_FROM_ENVELOPE,			"Remove current document from envelope")
	TB_LOCALIZED(CONTEXT_MENU_EXPAND_SUBNODES,				"Expand subnodes")
	TB_LOCALIZED(CONTEXT_MENU_COLLAPSE_SUBNODES,			"Collapse subnodes")
END_TB_STRING_MAP()

///////////////////////////////////////////////////////////////////
class TB_EXPORT CPostaLiteWorkersCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CPostaLiteWorkersCombo)

public:
	CPostaLiteWorkersCombo();
	CPostaLiteWorkersCombo(UINT nBtnIDBmp, DataStr* = NULL, CString aUserName = _T(""));
	~CPostaLiteWorkersCombo();

private:
	CString			m_UserName;
    CMapPtrToPtr*	m_pIDMap;
	long			m_LastWorker;
		 
private: 
	void CleanMap	();
	void AddToMap	(int aIndex, DataLng aID);
	
public:
	DataLng GetWorkerID	();

protected:
	virtual BOOL OnInitCtrl		();
	virtual	void OnFillListBox	();
};

///////////////////////////////////////////////////////////////////////////////
//	Class  BDPostaliteDocumentsGraphicNavigation Declaration
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT BDPostaliteDocumentsGraphicNavigation : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(BDPostaliteDocumentsGraphicNavigation)

public:
	DataBool				m_bTreeView;
	DataBool				m_bOnlyToSend;
	DataBool				m_bFilterByDate;
	DataBool				m_bFilterByWorker;
	DataStr					m_FilterByWorker;
	DataDate				m_FilterDateFrom;
	DataDate				m_FilterDateTo;
	DataMon					m_CurrentCredit;
	DataMon					m_UnsentMessageEstimate;
	CTreeViewAdvCtrl*		m_pTreeView;
	
	CBoolButton*			m_pFilterByDateCtrl;
	CBoolButton*			m_pFilterByWorkerCtrl;
	CDateEdit*				m_pFilterDateFromCtrl;
	CDateEdit*				m_pFilterDateToCtrl;
	CBoolButton*			m_pOnlyToSendCtrl;
	CPostaLiteWorkersCombo*	m_pFilterWorkerCtrl;

	CPostaliteDocumentsGraphicNavigationEdit*		m_pBody;
	DBTPostaliteDocumentsGraphicNavigationDetail*	m_pDBTPostaliteDocumentsGraphicNavigationDetail;
	
private:
	TMsgLots*			m_pLots;
	TMsgQueue*			m_pQueueMessage;
	SqlTable*			m_pTblLots;
	SqlTable*			m_pTblQueue;
	TUMsgQueue*			m_pTUMsgQueue;
	TUMsgLots* 			m_pTUMsgLots;
	CString				m_strRootPath;
	BOOL				m_bShowDetails;
	DataLng				m_pCurrentMsgId;
	DataLng				m_pCurrentEnvelopeId;
	DataLng				m_LastSelectedWorkerID;
	
	TbSenderInterface*	m_pTbSenderInterface;
public:
	BDPostaliteDocumentsGraphicNavigation();
	~BDPostaliteDocumentsGraphicNavigation();

public:
	BOOL HasHyperLink (TEnhPostaliteDocumentsGraphicNavigationDetail* pRec);
	void OpenMessagePdf();
	void OpenRelatedDocument();
	void OpenAddressDocument();
	void LoadTree();
	DataLng GetWorkerID	(){return m_LastSelectedWorkerID;}
	CString GetCurrentWorkerName();

protected:	
	virtual void	OnBatchExecute();
	virtual BOOL	OnAttachData();
	virtual void	DisableControlsForBatch();
	virtual BOOL	OnInitDocument();
	virtual BOOL	CanRunDocument();

private: 
	void LoadAllottedMessages();
	void LoadUnassignedMessages();
	void EstimateCostForUnsentMessages();

	CString AddEnvelopeElement();
	void AddMessageElement(const CString& parentKey);
	DataLng GetIdFromKey(CString searchKey, CString strKey);
	
	BOOL CanAlterMessage();
	BOOL CountryValidForPostaMassiva();
	BOOL RunAllotProcedure();
	BOOL RunUpdateSentEnvelopesStatus(BOOL bAsync);
	BOOL SendMessageAlone(CString strKey);
	BOOL SendMessageImmediately(CString strKey);
	BOOL SendEnvelopeNow(CString strKey);
	BOOL DeleteMessage(CString strKey);
	BOOL ReopenClosedEnvelope(CString strKey);
	BOOL CloseEnvelope(CString strKey);
	BOOL RemoveFromEnvelope(CString strKey);
	BOOL EnvelopeEstimate(CString strKey);
	BOOL ChangeDeliveryType(CString aKey, CPostaLiteAddress::Delivery deliveryType);
	BOOL ChangePrintType(CString aKey, CPostaLiteAddress::Print printType);
	
    void OnRunAllotProcedure();
	void OnCloseEnvelope();
	void OnCollapseSubnodes();
	void OnDeleteMessage();
	void OnEnvelopEstimate();
	void OnExpandNodes();
	void OnRemoveFromEnvelope();
	void OnReopenEnvelope();
	void OnSendEnvelopeNow();
	void OnSendMessageNow();
	void OnUpdateEnvelopeStatus();

	void ExpandSubnodes(CString strKey);
	void CollapseSubnodes(CString strKey);
	void ShowDetails(CString strKey);
	int  GetTreeNodeChildCount(CString aKey);
	void ShowLotDetails();
	void ShowMessageDetails();

	void SetSplitter();
	void AddDetail (const DataStr& aName, const DataObj& aValue, BOOL bHasHyperLink);
	void AddSeparator (const DataStr& aName);

	CPostaliteDocumentsGraphicNavigationFrame* GetFrame();

protected:
	afx_msg void OnSelectionNodeChanged();
	afx_msg void OnContextMenuItemClicked();
	afx_msg void OnFilterByDateCheck();
	afx_msg void OnFilterByWorkerCheck();
	afx_msg	void OnWorkerChanged();
	afx_msg void OnToolbarRefresh();
	afx_msg void OnUpdateRefresh(CCmdUI* pCmdUI);
	afx_msg void OnUpdateRunAllotProcedure(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCloseEnvelope(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCollapseSubnodes(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDeleteMessage(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEnvelopEstimate(CCmdUI* pCmdUI);
	afx_msg void OnUpdateExpandNodes(CCmdUI* pCmdUI);
	afx_msg void OnUpdateRemoveFromEnvelope(CCmdUI* pCmdUI);
	afx_msg void OnUpdateReopenEnvelope(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSendEnvelopeNow(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSendMessageNow(CCmdUI* pCmdUI); 
	afx_msg void OnUpdateUpdateEnvelopeStatus(CCmdUI* pCmdUI);

protected:
	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
//	Class  TEnhCompanyLayoutDetail Declaration
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT TEnhPostaliteDocumentsGraphicNavigationDetail : public SqlRecord
{
	DECLARE_DYNCREATE(TEnhPostaliteDocumentsGraphicNavigationDetail) 
	
public:
	DataStr	 l_FieldName;
	DataStr	 l_FieldValue;
	DataBool l_HasHyperlink;
	DataBool l_IsSeparator;
		
public:	
	TEnhPostaliteDocumentsGraphicNavigationDetail(BOOL bCallInit = TRUE);
	static LPCTSTR GetStaticName();

public:
    virtual void BindRecord();	
};

///////////////////////////////////////////////////////////////////////////////
//	Class  DBTPostaliteDocumentsGraphicNavigationDetail Declaration
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTPostaliteDocumentsGraphicNavigationDetail : public DBTSlaveBuffered
{ 
	friend class BDPostaliteDocumentsGraphicNavigation;
	
	DECLARE_DYNAMIC(DBTPostaliteDocumentsGraphicNavigationDetail)

public:
	DBTPostaliteDocumentsGraphicNavigationDetail
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	);

public:
	TEnhPostaliteDocumentsGraphicNavigationDetail*	GetCurrent		() 			const	{ return (TEnhPostaliteDocumentsGraphicNavigationDetail*)GetCurrentRow(); } 
	int												GetCurrentRowIdx()			const 	{ return m_nCurrentRow; }
	BDPostaliteDocumentsGraphicNavigation*			GetDocument		()			const	{ return (BDPostaliteDocumentsGraphicNavigation*) m_pDocument; }
  	TEnhPostaliteDocumentsGraphicNavigationDetail*	GetDetail		(int nRow) 	const 	{ return (TEnhPostaliteDocumentsGraphicNavigationDetail*)GetRow(nRow); } 
	TEnhPostaliteDocumentsGraphicNavigationDetail*	GetDetail		()		   	const	{ return (TEnhPostaliteDocumentsGraphicNavigationDetail*)GetRecord(); }
	virtual void									SetCurrentRow	(int nRow);

protected:
	virtual	void	 OnDefineQuery			()	{}
	virtual	void	 OnPrepareQuery			()	{}
	virtual DataObj* OnCheckPrimaryKey		(int /*nRow*/, SqlRecord*);
	virtual void	 OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

#include "endh.dex"