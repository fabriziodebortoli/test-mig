#include "stdafx.h"

#include <TbWoormViewer\SoapFunctions.h>

#include "BDPostaliteDocumentsGraphicNavigation.h"
#include "BDPostaliteDocumentsGraphicNavigation.hjson" //JSON AUTOMATIC UPDATE
#include "UIPostaliteDocumentsGraphicNavigation.h"


#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif                                

static TCHAR szRoot							[] = _T("PostaLite_RootElement");
static TCHAR szUnassigned					[] = _T("PostaLite_Unassigned");
static TCHAR szEnvelope						[] = _T("PostaLite_Envelope_");
static TCHAR szMsg							[] = _T("PostaLite_Msg_");

static TCHAR szRootImage					[] = _T("RootImage");
static TCHAR szMessageImage					[] = _T("MessageImage");
static TCHAR szEnvelopeImage				[] = _T("EnvelopeImage");
static TCHAR szUnassignedMessageImage		[] = _T("UnassignedMessageImage");
static TCHAR szUnassignedMessagesImage		[] = _T("UnassignedMessagesImage");
static TCHAR szEnvelopeUploadedImage		[] = _T("EnvelopeUploadedImage");
static TCHAR szCloseEnvelopeImage			[] = _T("CloseEnvelopeImage");
static TCHAR szWarningEnvelopeImage			[] = _T("WarningEnvelopeImage");
static TCHAR szErrorEnvelopeImage			[] = _T("ErrorEnvelopeImage");
static TCHAR szWaitingEnvelopeImage			[] = _T("WaitingEnvelopeImage");
static TCHAR szEnvelopeSentImage			[] = _T("EnvelopeSentImage");

//declasso gli erorri a warning per non bloccare l'apertura del documento
#define CALL_WEB_METHOD(code) \
	AfxGetApp()->BeginWaitCursor();\
	BOOL bOk = code;\
	if (!errorMessage.IsEmpty())\
	{\
		AfxGetDiagnostic()->Add(errorMessage);\
		AfxGetDiagnostic()->Show();\
	}\
	LoadTree();\
	AfxGetApp()->EndWaitCursor();\


/////////////////////////////////////////////////////////////////////////////
//					Static Methods
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
CString GetFullWorkerName(CWorker* worker)
{
	return worker->GetName().IsEmpty()
		? worker->GetLastName()
		: worker->GetName() + " " + worker->GetLastName();
}

//----------------------------------------------------------------------------
CString GetStatusFromLot(TUMsgLots* pLot)
{
	if (pLot->GetRecord()->f_LotID == 0)
		return _TB("Not enveloped");

	return pLot->GetRecord()->f_StatusExt == 0
		? PostaLiteDecodeStatus(pLot->GetRecord()->f_Status)
		: PostaLiteDecodeStatusExt(pLot->GetRecord()->f_StatusExt);
}

//----------------------------------------------------------------------------
CString GetImageForNode(TMsgLots* pLot)
{
	if (pLot->f_StatusExt == 0)
	{
		switch ((int)pLot->f_Status)
		{
		case CPostaLiteAddress::Uploaded:
			return szEnvelopeUploadedImage;
		case CPostaLiteAddress::Allotted:
			return szEnvelopeImage;
		case CPostaLiteAddress::Closed:
			return szCloseEnvelopeImage;
		case CPostaLiteAddress::Invalid:
			return szWarningEnvelopeImage;
		default:
			return szEnvelopeImage;
		}
	}
	else
	{
		switch ((int)pLot->f_StatusExt)
		{
		case CPostaLiteAddress::Spedito :
			return szEnvelopeSentImage;
		case CPostaLiteAddress::SpeditoConInesitati :
		case CPostaLiteAddress::Annullato :
		case CPostaLiteAddress::AnnullatoParzialmente :
			return szWarningEnvelopeImage;
		case CPostaLiteAddress::PresoInCarico :
		case CPostaLiteAddress::InElaborazione :
		case CPostaLiteAddress::InStampa :
			return szWaitingEnvelopeImage;
		case CPostaLiteAddress::Sospeso :
		case CPostaLiteAddress::Errato :
			return szErrorEnvelopeImage;
		default:
			return _T("");
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
//					BDPostaliteDocumentsGraphicNavigation
/////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDPostaliteDocumentsGraphicNavigation, CAbstractFormDoc)

BEGIN_MESSAGE_MAP(BDPostaliteDocumentsGraphicNavigation, CAbstractFormDoc)
	
	ON_BN_CLICKED		(IDC_MESSAGES_MANAGEMENT_REFRESH_DATA, OnBatchExecute)
	ON_CONTROL			(UM_TREEVIEWADV_SELECTION_CHANGED, IDC_MESSAGES_MANAGEMENT_TREECONTROL, OnSelectionNodeChanged)
	ON_CONTROL			(UM_TREEVIEWADV_CONTEXT_MENU_ITEM_CLICK, IDC_MESSAGES_MANAGEMENT_TREECONTROL, OnContextMenuItemClicked)
	
	ON_EN_VALUE_CHANGED	(IDC_MESSAGES_MANAGEMENT_FILTERBYDATE_CHECK, OnFilterByDateCheck)
	ON_EN_VALUE_CHANGED	(IDC_MESSAGES_MANAGEMENT_FILTERBYWORKER_CHECK, OnFilterByWorkerCheck)
	ON_EN_VALUE_CHANGED	(IDC_MESSAGES_MANAGEMENT_FILTERBYWORKER, OnWorkerChanged)

	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_REFRESH_TREE,			OnBatchExecute)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_ALLOT_MESSAGES,			OnRunAllotProcedure)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_CLOSE_ENVELOPE,			OnCloseEnvelope)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_COLLAPSE_NODES,			OnCollapseSubnodes)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_DELETE_MESSAGE,			OnDeleteMessage)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_ENVELOPE_ESTIMATE,		OnEnvelopEstimate)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_EXPAND_NODES,			OnExpandNodes)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_REMOVE_FROM_ENVELOPE,	OnRemoveFromEnvelope)
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_REOPEN_ENVELOPE,		OnReopenEnvelope) 
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_SEND_ENVELOPE_NOW,		OnSendEnvelopeNow) 
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_SEND_MESSAGE_NOW,		OnSendMessageNow) 
	ON_COMMAND			(ID_MESSAGES_MANAGEMENT_UPDATE_ENVELOPE_STATUS, OnUpdateEnvelopeStatus)

	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_REFRESH_TREE,			OnUpdateRefresh)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_ALLOT_MESSAGES,			OnUpdateRunAllotProcedure)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_CLOSE_ENVELOPE,			OnUpdateCloseEnvelope)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_COLLAPSE_NODES,			OnUpdateCollapseSubnodes)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_DELETE_MESSAGE,			OnUpdateDeleteMessage)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_ENVELOPE_ESTIMATE,		OnUpdateEnvelopEstimate)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_EXPAND_NODES,			OnUpdateExpandNodes)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_REMOVE_FROM_ENVELOPE,	OnUpdateRemoveFromEnvelope)
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_REOPEN_ENVELOPE,		OnUpdateReopenEnvelope) 
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_SEND_ENVELOPE_NOW,		OnUpdateSendEnvelopeNow) 
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_SEND_MESSAGE_NOW,		OnUpdateSendMessageNow) 
	ON_UPDATE_COMMAND_UI(ID_MESSAGES_MANAGEMENT_UPDATE_ENVELOPE_STATUS, OnUpdateUpdateEnvelopeStatus)

END_MESSAGE_MAP()

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnToolbarRefresh()
{
	LoadTree();	
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateRefresh(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateRunAllotProcedure(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();

	BOOL enabled = 
		(aKey.CompareNoCase(szRoot) == 0 || aKey.CompareNoCase(szUnassigned) == 0) &&
		GetTreeNodeChildCount(szUnassigned) > 0;

	pCmdUI->Enable(enabled);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateCloseEnvelope(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable(aKey.Find(szEnvelope) >= 0 && (m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Allotted));
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateCollapseSubnodes(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable(aKey.CompareNoCase(szRoot) == 0);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateDeleteMessage(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable
		(
		aKey.Find(szMsg) >= 0 &&
		CanAlterMessage() && 
		(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateEnvelopEstimate(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable(aKey.Find(szEnvelope) >= 0 && m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) && CanAlterMessage());
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateExpandNodes(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable(aKey.CompareNoCase(szRoot) == 0);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateRemoveFromEnvelope(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable
		(
		aKey.Find(szMsg) >= 0 && 
		m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) &&
		CanAlterMessage() &&
		(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateReopenEnvelope(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();

	pCmdUI->Enable(
		aKey.Find(szEnvelope) >= 0 &&  
		m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) && 
		m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Closed
		);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateSendEnvelopeNow(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	pCmdUI->Enable
		(
		m_pTUMsgLots != NULL &&
		aKey.Find(szEnvelope) >= 0 && 
		m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) &&
		CanAlterMessage()
		);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateSendMessageNow(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	CString parentLotNode = m_pTreeView->GetParentKey(aKey);
		
	pCmdUI->Enable
		(
		aKey.Find(szMsg) >= 0 &&
		CanAlterMessage() && 
		GetTreeNodeChildCount(parentLotNode) > 1 &&
		(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateUpdateEnvelopeStatus(CCmdUI* pCmdUI)
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();

	pCmdUI->Enable(aKey.Find(szEnvelope) >= 0);
}

//----------------------------------------------------------------------------
BDPostaliteDocumentsGraphicNavigation::BDPostaliteDocumentsGraphicNavigation()
	: 
		m_bTreeView(NULL),
		m_bOnlyToSend(TRUE),
		m_bFilterByDate(FALSE),
		m_pTreeView(NULL),
		m_pOnlyToSendCtrl(NULL),
		m_pFilterByDateCtrl(NULL),
		m_pFilterDateFromCtrl(NULL),
		m_pFilterDateToCtrl(NULL),
		m_pBody(NULL),
		m_pDBTPostaliteDocumentsGraphicNavigationDetail(NULL),
		m_pLots(NULL),
		m_pQueueMessage(NULL),
		m_pTblLots(NULL),
		m_pTblQueue(NULL),
		m_pTUMsgQueue(NULL),
		m_pTUMsgLots(NULL),
		m_strRootPath(),
		m_bShowDetails(NULL),
		m_pCurrentMsgId(-1),
		m_pCurrentEnvelopeId(-1),
		m_CurrentCredit(),
		m_UnsentMessageEstimate(),
		m_pTbSenderInterface (NULL),
		m_bFilterByWorker(FALSE),
		m_FilterByWorker()
{
	DataDate DateApp = AfxGetApplicationDate();
	m_FilterDateTo.SetFullDate();
	m_FilterDateTo = AfxGetApplicationDate();
	
	DateApp -= 7;
	m_FilterDateFrom.SetFullDate();
	m_FilterDateFrom = DateApp;

	m_LastSelectedWorkerID = AfxGetLoginContext()->GetWorkerId();
}
		
//----------------------------------------------------------------------------
CString BDPostaliteDocumentsGraphicNavigation::GetCurrentWorkerName()
{
	CWorker* worker = AfxGetWorkersTable()->GetWorker(AfxGetLoginContext()->GetWorkerId());
	return worker
		? GetFullWorkerName(worker)
		: _T("");
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnRunAllotProcedure()
{
	RunAllotProcedure();
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnCloseEnvelope()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	CloseEnvelope(aKey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnCollapseSubnodes()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	CollapseSubnodes(aKey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnDeleteMessage()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	DeleteMessage(aKey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnEnvelopEstimate()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	EnvelopeEstimate(aKey);
}
	
//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnExpandNodes()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	ExpandSubnodes(aKey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnRemoveFromEnvelope()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	RemoveFromEnvelope(aKey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnReopenEnvelope()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	ReopenClosedEnvelope(aKey);
}
		
//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnSendEnvelopeNow()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	SendEnvelopeNow(aKey);
}
		
//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnSendMessageNow()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();
	SendMessageAlone(aKey);
}
		
//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnUpdateEnvelopeStatus()
{
	RunUpdateSentEnvelopesStatus(FALSE);
}

//----------------------------------------------------------------------------
BDPostaliteDocumentsGraphicNavigation::~BDPostaliteDocumentsGraphicNavigation()
{
	if (m_pTblLots)
		m_pTblLots->Close();
	if (m_pTblQueue)
		m_pTblQueue->Close();

	SAFE_DELETE (m_pTblLots);
	SAFE_DELETE (m_pTblQueue);
	SAFE_DELETE (m_pTUMsgQueue);
	SAFE_DELETE (m_pTUMsgLots);
	SAFE_DELETE (m_pLots);
	SAFE_DELETE (m_pQueueMessage);
	SAFE_DELETE (m_pDBTPostaliteDocumentsGraphicNavigationDetail);
	SAFE_DELETE (m_pTbSenderInterface);
}

//---------------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::CanRunDocument()
{
	PostaLiteSettings settings;
	if (!settings.m_Enabled || settings.m_TokenAuth.IsEmpty())
	{
		AfxGetDiagnostic()->Add(_TB("Unable to run document: in order to use this document the PostaLite service from the \"PostaLite Settings\" must be enabled and you have to be logged to the PostaLite Service"));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}
	return TRUE;
}

//---------------------------------------------------------------------------------
CPostaliteDocumentsGraphicNavigationFrame* BDPostaliteDocumentsGraphicNavigation::GetFrame()
{
	POSITION pos = GetFirstViewPosition();
	CView* pView = GetNextView(pos);
	ASSERT (pView);
    CPostaliteDocumentsGraphicNavigationFrame* pFrame = (CPostaliteDocumentsGraphicNavigationFrame*) pView->GetParentFrame();
    ASSERT_VALID (pFrame);
    return pFrame;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::OnAttachData()
{              
	SetFormTitle (_TB("PostaLite Documents Graphic Navigation"));

	m_pLots = new TMsgLots();
	m_pQueueMessage = new TMsgQueue();
	
	m_pTblLots = new SqlTable(m_pLots, GetReadOnlySqlSession());	
	m_pTblQueue = new SqlTable(m_pQueueMessage, GetReadOnlySqlSession());	
	
	m_strRootPath = AfxGetPathFinder()->GetModuleFilesPath(CTBNamespace(_T("Image.Extensions.TbMailer.Images")), CPathFinder::STANDARD);

	m_pTUMsgQueue = new TUMsgQueue(this);
	m_pTUMsgLots = new TUMsgLots(this);

	m_pDBTPostaliteDocumentsGraphicNavigationDetail = new DBTPostaliteDocumentsGraphicNavigationDetail (RUNTIME_CLASS(TEnhPostaliteDocumentsGraphicNavigationDetail), this); 
	
	m_pTbSenderInterface = new TbSenderInterface(); 

	return TRUE;
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OpenMessagePdf()
{
	if (
		m_pCurrentMsgId < 0 ||
		AfxIsRemoteInterface() || 
		!(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		)
		return;

	CFunctionDescription fd;		
	DataStr strTempPath;

	AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("Extensions.TbMailer.TbMailer.GetPostaLiteDocument"), fd);
	fd.SetParamValue(_T("msgID"),	m_pCurrentMsgId);
	fd.AddOutParam(_T("tempFileName"),	&strTempPath);

	BOOL bOk = AfxGetTbCmdManager()->RunFunction(&fd, 0);
	::ShellExecute(strTempPath.Str());
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OpenRelatedDocument()
{
	if (
		m_pCurrentMsgId < 0 ||
		m_pTUMsgQueue->GetRecord()->f_DocNamespace.IsEmpty() || 
		m_pTUMsgQueue->GetRecord()->f_DocPrimaryKey.IsEmpty() || 
		!(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		)
		return;

	DataStr aNs;
	DataStr aPrimarykey;

	aNs = m_pTUMsgQueue->GetRecord()->f_DocNamespace;
	aPrimarykey = m_pTUMsgQueue->GetRecord()->f_DocPrimaryKey;
		
	CTBNamespace ns(aNs);
	if (ns.GetType() == CTBNamespace::DOCUMENT)
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(aNs.Str());
		if (pDoc)
			pDoc->GoInBrowserMode(aPrimarykey);
	}
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OpenAddressDocument()
{
	if (
		m_pCurrentMsgId < 0 ||
		m_pTUMsgQueue->GetRecord()->f_AddresseeNamespace.IsEmpty() || 
		m_pTUMsgQueue->GetRecord()->f_AddresseePrimaryKey.IsEmpty() ||
		!(AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId())
		)
		return;

	DataStr aNs;
	DataStr aPrimarykey;
	aNs = m_pTUMsgQueue->GetRecord()->f_AddresseeNamespace;
	aPrimarykey = m_pTUMsgQueue->GetRecord()->f_AddresseePrimaryKey;
		
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(aNs.Str());
	pDoc->GoInBrowserMode(aPrimarykey);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::ShowLotDetails()
{
	if (!m_pTUMsgLots || !m_pTUMsgLots->GetRecord())
		return;

	ADD_SEPARATOR_LAYOUT(ENVELOPE_DETAILS);
	ADD_DETAIL_LAYOUT(ENVELOPE_ID, m_pTUMsgLots->GetRecord()->f_LotID, FALSE);

	if (m_pTUMsgLots->GetRecord()->f_IdExt != DataLng(0))
		ADD_DETAIL_LAYOUT(POSTALITE_ID, m_pTUMsgLots->GetRecord()->f_IdExt, FALSE);
	
	//Se esiste lo StatusExt, vuol dire che l'envelope in qualche modo è arrivato a postaLite, quindi è 
	//lui stesso a dirmi in che stato è, altrimenti usiamo lo status intenro nostro
	DataStr status = GetStatusFromLot(m_pTUMsgLots);
	ADD_DETAIL_LAYOUT(STATUS, status, FALSE);

	if (m_pTUMsgLots->GetRecord()->f_ErrorExt != DataLng(0))
		ADD_DETAIL_LAYOUT(ERROR_DESCRIPTION, PostaLiteDecodeErrorExt(m_pTUMsgLots->GetRecord()->f_ErrorExt), FALSE);

	ADD_DETAIL_LAYOUT(TOTAL_PAGES, m_pTUMsgLots->GetRecord()->f_TotalPages, FALSE);
	ADD_DETAIL_LAYOUT(TOTAL_AMOUNT, m_pTUMsgLots->GetRecord()->f_TotalAmount, FALSE);
	ADD_DETAIL_LAYOUT(POSTAGE_AMOUNT, m_pTUMsgLots->GetRecord()->f_PostageAmount, FALSE);
	ADD_DETAIL_LAYOUT(PRINT_AMOUNT, m_pTUMsgLots->GetRecord()->f_PrintAmount, FALSE);
		
	m_pTUMsgLots->GetRecord()->f_SendAfter.SetFullDate();
	ADD_DETAIL_LAYOUT(SEND_AFTER, m_pTUMsgLots->GetRecord()->f_SendAfter, FALSE);
		
	DataStr deliverString = PostaLiteDecodeDeliveryType(m_pTUMsgLots->GetRecord()->f_DeliveryType);
	DataStr printString = PostaLiteDecodePrintType(m_pTUMsgLots->GetRecord()->f_PrintType);
		
	ADD_DETAIL_LAYOUT(DELIVERY_TYPE, deliverString, FALSE);
	ADD_DETAIL_LAYOUT(PRINT_TYPE, printString, FALSE);
		
	ADD_SEPARATOR_LAYOUT(ADDRESSE_DETAILS);
	ADD_DETAIL_LAYOUT(ADDRESSE, m_pTUMsgLots->GetRecord()->f_Addressee, FALSE);
	ADD_DETAIL_LAYOUT(ADDRESS, m_pTUMsgLots->GetRecord()->f_Address, FALSE);
	ADD_DETAIL_LAYOUT(CITY, m_pTUMsgLots->GetRecord()->f_City, FALSE);
	ADD_DETAIL_LAYOUT(COUNTY, m_pTUMsgLots->GetRecord()->f_County, FALSE);
	ADD_DETAIL_LAYOUT(COUNTRY, m_pTUMsgLots->GetRecord()->f_Country, FALSE);
	ADD_DETAIL_LAYOUT(FAX, m_pTUMsgLots->GetRecord()->f_Fax, FALSE);
	ADD_DETAIL_LAYOUT(ZIP, m_pTUMsgLots->GetRecord()->f_ZipCode, FALSE);
	ADD_DETAIL_LAYOUT(LAST_MODIFIED, m_pTUMsgLots->GetRecord()->f_TBModified, FALSE);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::ShowMessageDetails()
{	
	if (!m_pTUMsgQueue || !m_pTUMsgQueue->GetRecord())
		return;

	ADD_SEPARATOR_LAYOUT(MESSAGE_DETAILS);
	ADD_DETAIL_LAYOUT(ENVELOPE_ID, m_pTUMsgQueue->GetRecord()->f_LotID, FALSE);
	ADD_DETAIL_LAYOUT(MESSAGE_ID, m_pTUMsgQueue->GetRecord()->f_MsgID, FALSE);
	
	//Se esiste lo StatusExt, vuol dire che l'envelope in qualche modo è arrivato a postaLite, quindi è 
	//lui stesso a dirmi in che stato è, altrimenti usiamo lo status intenro nostro
	DataStr status = GetStatusFromLot(m_pTUMsgLots);
	ADD_DETAIL_LAYOUT(STATUS, status, FALSE);
	
	BOOL bUserCanOpen = AfxGetLoginInfos()->m_bAdmin || m_pTUMsgQueue->GetRecord()->f_TBCreatedID == AfxGetLoginContext()->GetWorkerId();

	ADD_DETAIL_LAYOUT(ADDRESSE, m_pTUMsgQueue->GetRecord()->f_Addressee, bUserCanOpen);

	CWorker* worker = AfxGetWorkersTable()->GetWorker(m_pTUMsgQueue->GetRecord()->f_TBCreatedID);
	DataStr workerStr (GetFullWorkerName(worker));
	ADD_DETAIL_LAYOUT(WORKER, workerStr, FALSE);

	if (!m_pTUMsgQueue->GetRecord()->f_AddresseeNamespace.IsEmpty())
	{
		CTBNamespace ns(m_pTUMsgQueue->GetRecord()->f_AddresseeNamespace);
		ADD_DETAIL_LAYOUT(SUBJECT, m_pTUMsgQueue->GetRecord()->f_Subject, ns.GetType() == CTBNamespace::DOCUMENT && bUserCanOpen);
	}

	ADD_DETAIL_LAYOUT(DOCUMENT_FILENAME, m_pTUMsgQueue->GetRecord()->f_DocFileName, bUserCanOpen);
	ADD_DETAIL_LAYOUT(DOCUMENT_PAGES, m_pTUMsgQueue->GetRecord()->f_DocPages, FALSE);
	ADD_DETAIL_LAYOUT(DOCUMENT_SIZE, m_pTUMsgQueue->GetRecord()->f_DocSize / 1000, FALSE);

	DataStr deliverString = PostaLiteDecodeDeliveryType(m_pTUMsgQueue->GetRecord()->f_DeliveryType);
	DataStr printString = PostaLiteDecodePrintType(m_pTUMsgQueue->GetRecord()->f_PrintType);
	ADD_DETAIL_LAYOUT(DELIVERY_TYPE, deliverString, FALSE);
	ADD_DETAIL_LAYOUT(PRINT_TYPE, printString, FALSE);

	ADD_DETAIL_LAYOUT(LAST_MODIFIED, m_pTUMsgQueue->GetRecord()->f_TBModified, FALSE);

	ADD_SEPARATOR_LAYOUT(ADDRESSE_DETAILS);
	ADD_DETAIL_LAYOUT(ADDRESSE, m_pTUMsgQueue->GetRecord()->f_Addressee, FALSE);
	ADD_DETAIL_LAYOUT(ADDRESS, m_pTUMsgQueue->GetRecord()->f_Address, FALSE);
	ADD_DETAIL_LAYOUT(CITY, m_pTUMsgQueue->GetRecord()->f_City, FALSE);
	ADD_DETAIL_LAYOUT(COUNTY, m_pTUMsgQueue->GetRecord()->f_County, FALSE);
	ADD_DETAIL_LAYOUT(COUNTRY, m_pTUMsgQueue->GetRecord()->f_Country, FALSE);
	ADD_DETAIL_LAYOUT(FAX, m_pTUMsgQueue->GetRecord()->f_Fax, FALSE);
	ADD_DETAIL_LAYOUT(ZIP, m_pTUMsgQueue->GetRecord()->f_ZipCode, FALSE);
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::OnInitDocument()
{
	GetFrame()->m_wndSplitter.SplitHorizontally();
	SetSplitter();

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::DisableControlsForBatch()
{
	m_FilterDateFrom.SetReadOnly(!m_bFilterByDate);
	m_FilterDateTo.SetReadOnly(!m_bFilterByDate);

	if (!AfxGetLoginInfos()->m_bAdmin)
		m_bFilterByWorker = TRUE;

	m_bFilterByWorker.SetReadOnly(!AfxGetLoginInfos()->m_bAdmin);
	m_FilterByWorker.SetReadOnly(!AfxGetLoginInfos()->m_bAdmin || !m_bFilterByWorker);

	m_CurrentCredit.SetReadOnly(TRUE);
	m_UnsentMessageEstimate.SetReadOnly(TRUE);
}

//----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::SetSplitter()
{
	CRect rect;
	GetFrame()->GetWindowRect(&rect);

	if (m_bShowDetails)
	{
		GetFrame()->m_wndSplitter.SetColumnInfo(0, (int) (rect.Width() * 0.6), 1);
		GetFrame()->m_wndSplitter.SetColumnInfo(1, (int) (rect.Width() * 0.4), 1);
	}
	else
	{
		GetFrame()->m_wndSplitter.SetColumnInfo(0, rect.Width() - 1, 1);
		GetFrame()->m_wndSplitter.SetColumnInfo(1, 1, 1);
	}
	GetFrame()->m_wndSplitter.RecalcLayout();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnBatchExecute()
{
	//Carica il tree dopo aver lanciato in maniera asincrona la update dei lotti su postalite.
	RunUpdateSentEnvelopesStatus(TRUE);
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnSelectionNodeChanged()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();

	ShowDetails(aKey);

	if (aKey.CompareNoCase(szRoot) == 0 || aKey.CompareNoCase(szUnassigned) == 0)
	{
		int countAssigned = GetTreeNodeChildCount(szRoot);
		int countUnassigned = GetTreeNodeChildCount(szUnassigned);

		if (countAssigned > 1)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_UPDATE_SENT_LOTS_STATUS));
			
		if (countUnassigned > 0)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_RUN_ALLOT_PROCEDURE));

		if (aKey.CompareNoCase(szRoot) == 0)
		{
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_COLLAPSE_SUBNODES));
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_EXPAND_SUBNODES));
		}
		return;
	}

	if (aKey.Find(szEnvelope) >= 0)
	{
		m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_UPDATE_SENT_LOTS_STATUS));

		if (
			m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) &&
			CanAlterMessage()
			)
		{
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_SEND_ENVELOPE_NOW));
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_LOT_ESTIMATE));
		}
		
		if (	
			m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) &&
			m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Closed
			)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_REOPEN_CLOSED_ENVELOPE));

		if (m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Allotted)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_CLOSE_ENVELOPE));
	}

	//solo se il msg non è uploadato
	if (
		aKey.Find(szMsg) >= 0 &&
		CanAlterMessage()
		)
	{
		//se non sono admin, non posso toccare i messaggi che non sono miei
		if (!AfxGetLoginInfos()->m_bAdmin && m_pTUMsgQueue->GetRecord()->f_TBCreatedID != AfxGetLoginContext()->GetWorkerId())
			return;

		//la voce di menù "manda il messaggio da solo" è presente solo se il messaggio non è già da solo
		CString parentLotNode = m_pTreeView->GetParentKey(aKey);
		int count = GetTreeNodeChildCount(parentLotNode);
		if (count > 1)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_SEND_MESSAGE_ALONE));

		m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_SEND_IMMEDIATELY));
		m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_DELETE_MESSAGE));

		if (
			m_pTUMsgLots->GetRecord()->f_LotID != DataLng(0) &&
			CanAlterMessage()
			)
			m_pTreeView->AddContextMenuItem(FIELD(CONTEXT_MENU_REMOVE_FROM_ENVELOPE));
		
		CArray<CString> arDeliveryType;
		arDeliveryType.Add(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaMassiva));
		arDeliveryType.Add(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaPrioritaria));
		arDeliveryType.Add(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandata));
		arDeliveryType.Add(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandataAR));
		//arDeliveryType.Add(PostaLiteDecodeDeliveryType(CPostaLiteAddress::Fax));

		m_pTreeView->AddContextSubMenuItem(FIELD(CONTEXT_MENU_MESSAGE_CHANGE_DELIVERY_TYPE), arDeliveryType);
		m_pTreeView->SetMenuItemCheck(PostaLiteDecodeDeliveryType(m_pTUMsgQueue->GetRecord()->f_DeliveryType));

		CArray<CString> arPrintType;
		arPrintType.Add(PostaLiteDecodePrintType(CPostaLiteAddress::Front_BlackWhite));
		arPrintType.Add(PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_BlackWhite));
		arPrintType.Add(PostaLiteDecodePrintType(CPostaLiteAddress::Front_Color));
		arPrintType.Add(PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_Color));

		m_pTreeView->AddContextSubMenuItem(FIELD(CONTEXT_MENU_MESSAGE_CHANGE_PRINT_TYPE), arPrintType);
		m_pTreeView->SetMenuItemCheck(PostaLiteDecodePrintType(m_pTUMsgQueue->GetRecord()->f_PrintType));	

		//La posta massiva è valida solo in italia
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaMassiva), CountryValidForPostaMassiva());	
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaPrioritaria), TRUE);	
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandata), TRUE);	
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandataAR), TRUE);	
		//m_pTreeView->SetMenuItemEnable(PostaLiteDecodeDeliveryType(CPostaLiteAddress::Fax), m_pTUMsgQueue->GetRecord()->f_PrintType == CPostaLiteAddress::Front_BlackWhite);	
		
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodePrintType(CPostaLiteAddress::Front_BlackWhite), TRUE);
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_BlackWhite), TRUE);
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodePrintType(CPostaLiteAddress::Front_Color), TRUE);
		m_pTreeView->SetMenuItemEnable(PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_Color), TRUE);
	}
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::CountryValidForPostaMassiva()
{
	return 
		m_pTUMsgQueue && 
		(m_pTUMsgQueue->GetRecord()->f_Country.Str().CompareNoCase(_T("Italia")) == 0 ||
		m_pTUMsgQueue->GetRecord()->f_Country.Str().CompareNoCase(_T("Italy")) == 0);
}

//-----------------------------------------------------------------------------
int BDPostaliteDocumentsGraphicNavigation::GetTreeNodeChildCount(CString aKey)
{
	CTreeNodeAdvWrapperObj* pNode = m_pTreeView->GetNode(aKey);
	CTreeNodeAdvArray ar;
	if (!pNode)
		return -1;
	
	pNode->GetChildren(ar);
	return ar.GetCount();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::ShowDetails(CString strKey)
{
	m_pDBTPostaliteDocumentsGraphicNavigationDetail->RemoveAll();
	m_pCurrentMsgId = m_pCurrentEnvelopeId = -1;
	
	if (strKey.Find(szMsg) >= 0)
	{
		m_pCurrentMsgId = GetIdFromKey(szMsg, strKey); 

		if (m_pTUMsgQueue->FindRecord(m_pCurrentMsgId, FALSE) == TableUpdater::FOUND)
		{
			m_pCurrentEnvelopeId = m_pTUMsgQueue->GetRecord()->f_LotID;
			m_pTUMsgLots->FindRecord(m_pCurrentEnvelopeId, FALSE);

			ShowMessageDetails();
		}
	}
	else if (strKey.Find(szEnvelope) >= 0)
	{
		m_pCurrentEnvelopeId = GetIdFromKey(szEnvelope, strKey); 
		
		if (m_pTUMsgLots->FindRecord(m_pCurrentEnvelopeId, FALSE) == TableUpdater::FOUND)
		{
			ShowLotDetails();

			//rinfresca l'immagine
			CString image = GetImageForNode(m_pTUMsgLots->GetRecord());
			m_pTreeView->SetImage(strKey, image);
		}
	}
	
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnWorkerChanged()
{ 
	m_LastSelectedWorkerID = m_pFilterWorkerCtrl->GetWorkerID();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnFilterByDateCheck()
{
	m_FilterDateFrom.SetReadOnly(!m_bFilterByDate);
	m_FilterDateTo.SetReadOnly(!m_bFilterByDate);

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnFilterByWorkerCheck()
{
	m_FilterByWorker.SetReadOnly(!m_bFilterByWorker);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::OnContextMenuItemClicked()
{
	CString aKey = m_pTreeView->GetSelectedNodeKey();

	CString menuItemKey;
	m_pTreeView->GetTextContextMenuItemClicked(menuItemKey);

	if (menuItemKey == FIELD(CONTEXT_MENU_RUN_ALLOT_PROCEDURE))
		RunAllotProcedure();
	else if (menuItemKey == FIELD(CONTEXT_MENU_UPDATE_SENT_LOTS_STATUS))
		RunUpdateSentEnvelopesStatus(FALSE);
	else if (menuItemKey == FIELD(CONTEXT_MENU_SEND_MESSAGE_ALONE))
		SendMessageAlone(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_SEND_IMMEDIATELY))
		SendMessageImmediately(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_SEND_ENVELOPE_NOW))
		SendEnvelopeNow(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_DELETE_MESSAGE))
		DeleteMessage(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_REOPEN_CLOSED_ENVELOPE))
		ReopenClosedEnvelope(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_CLOSE_ENVELOPE))
		CloseEnvelope(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_REMOVE_FROM_ENVELOPE))
		RemoveFromEnvelope(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_LOT_ESTIMATE))
		EnvelopeEstimate(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_EXPAND_SUBNODES))
		ExpandSubnodes(aKey);
	else if (menuItemKey == FIELD(CONTEXT_MENU_COLLAPSE_SUBNODES))
		CollapseSubnodes(aKey);

	//Cambio delivery type
	else if (menuItemKey == PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaMassiva))
		ChangeDeliveryType(aKey, CPostaLiteAddress::PostaMassiva );
	else if (menuItemKey == PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaPrioritaria))
		ChangeDeliveryType(aKey, CPostaLiteAddress::PostaPrioritaria );
	else if (menuItemKey == PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandata))
		ChangeDeliveryType(aKey, CPostaLiteAddress::PostaRaccomandata );
	else if (menuItemKey == PostaLiteDecodeDeliveryType(CPostaLiteAddress::PostaRaccomandataAR))
		ChangeDeliveryType(aKey, CPostaLiteAddress::PostaRaccomandataAR );
	//else if (menuItemKey == PostaLiteDecodeDeliveryType(CPostaLiteAddress::Fax))
	//	ChangeDeliveryType(aKey, CPostaLiteAddress::Fax );

	//Cambio print type
	else if (menuItemKey == PostaLiteDecodePrintType(CPostaLiteAddress::Front_BlackWhite))
		ChangePrintType(aKey, CPostaLiteAddress::Front_BlackWhite );
	else if (menuItemKey == PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_BlackWhite))
		ChangePrintType(aKey, CPostaLiteAddress::FrontBack_BlackWhite );
	else if (menuItemKey == PostaLiteDecodePrintType(CPostaLiteAddress::Front_Color))
		ChangePrintType(aKey, CPostaLiteAddress::Front_Color);
	else if (menuItemKey == PostaLiteDecodePrintType(CPostaLiteAddress::FrontBack_Color))
		ChangePrintType(aKey, CPostaLiteAddress::FrontBack_Color );
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::ChangeDeliveryType(CString aKey, CPostaLiteAddress::Delivery deliveryType)
{
	DataLng msgId = GetIdFromKey(szMsg, aKey);
	DataLng delType = deliveryType;
	DataStr errorMessage;

	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}

	CALL_WEB_METHOD(m_pTbSenderInterface->ChangeMessageDeliveryType(msgId, delType, errorMessage));
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::ChangePrintType(CString aKey, CPostaLiteAddress::Print printType)
{
	DataLng msgId = GetIdFromKey(szMsg, aKey);
	DataLng prntType = printType;
	DataStr errorMessage;

	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}

	CALL_WEB_METHOD(m_pTbSenderInterface->ChangeMessagePrintType(msgId, prntType, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::RunUpdateSentEnvelopesStatus(BOOL bAsync)
{
	DataStr errorMessage;

	DataInt nCodeState;
	DataDate expiryDate;
	DataStr errorMsg;
	PostaLiteSettings settings;
	m_CurrentCredit = m_pTbSenderInterface->GetCreditState(settings.m_LoginId, settings.m_TokenAuth, nCodeState, expiryDate, errorMsg);

	CALL_WEB_METHOD(m_pTbSenderInterface->UpdateSentLotsStatus(DataBool(bAsync), errorMessage));

	UpdateDataView();
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::RunAllotProcedure()
{
	DataStr errorMessage;

	CALL_WEB_METHOD(m_pTbSenderInterface->AllotMessages(errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::SendMessageAlone(CString strKey)
{
	DataStr errorMessage;
	DataLng msgId = GetIdFromKey(szMsg, strKey);

	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}
	
	CALL_WEB_METHOD(m_pTbSenderInterface->CreateSingleMessageLot(msgId, DataBool(FALSE), errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::CanAlterMessage()
{
	//Posso alterare un msg solo se non è già stato uploadato o in uploading oppure se postalite in qualche modo
	//ha deciso che quella busta non va bene
	return
		m_pTUMsgLots->GetRecord()->f_StatusExt == 0 
		?	(
			m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Allotted ||
			m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Closed || 
			m_pTUMsgLots->GetRecord()->f_Status == CPostaLiteAddress::Invalid
			)
		:	(
			m_pTUMsgLots->GetRecord()->f_StatusExt == CPostaLiteAddress::Annullato ||
			m_pTUMsgLots->GetRecord()->f_StatusExt == CPostaLiteAddress::Sospeso || 
			m_pTUMsgLots->GetRecord()->f_StatusExt == CPostaLiteAddress::Errato
			);
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::SendMessageImmediately(CString strKey)
{
	DataStr errorMessage;
	DataLng msgId = GetIdFromKey(szMsg, strKey);
	
	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}

	CALL_WEB_METHOD(m_pTbSenderInterface->CreateSingleMessageLot(msgId, DataBool(TRUE), errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::CloseEnvelope(CString strKey)
{
	DataStr errorMessage;
	DataLng lotId = GetIdFromKey(szEnvelope, strKey);
	
	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}

	CALL_WEB_METHOD(m_pTbSenderInterface->CloseLot(lotId, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::ExpandSubnodes(CString strKey)
{
	m_pTreeView->ExpandAllFromSelectedNode();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::CollapseSubnodes(CString strKey)
{
	m_pTreeView->CollapseAllFromSelectedNode();
	CTreeNodeAdvWrapperObj* pNode = m_pTreeView->GetNode(strKey);
	if (pNode)
		pNode->ToggleNode();
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::EnvelopeEstimate(CString strKey)
{
	DataStr errorMessage;
	DataLng lotID = GetIdFromKey(szEnvelope, strKey);
	
	CALL_WEB_METHOD(m_pTbSenderInterface->GetLotCostEstimate(lotID, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::RemoveFromEnvelope(CString strKey)
{
	if (AfxTBMessageBox(_TB("Are you really sure you want to remove this document from the envelope?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return TRUE;

	DataStr errorMessage;
	DataLng msgId = GetIdFromKey(szMsg, strKey);
	

	CALL_WEB_METHOD(m_pTbSenderInterface->RemoveFromLot(msgId, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::ReopenClosedEnvelope(CString strKey)
{
	if (AfxTBMessageBox(_TB("Are you really sure you want to re-open this envelope?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return TRUE;

	DataStr errorMessage;
	DataLng lotId = GetIdFromKey(szEnvelope, strKey);
	
	CALL_WEB_METHOD(m_pTbSenderInterface->ReopenClosedLot(lotId, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::DeleteMessage(CString strKey)
{
	if (!CanAlterMessage())
	{
		AfxGetDiagnostic()->Add(FIELD(MESSAGE_ALREADY_UPLOADED));
		AfxGetDiagnostic()->Show();
		return FALSE;
	}

	if (AfxTBMessageBox(_TB("Are you really sure you want to delete this message?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return TRUE;

	DataStr errorMessage;
	DataLng msgId = GetIdFromKey(szMsg, strKey);
	
	CALL_WEB_METHOD(m_pTbSenderInterface->DeleteMessage(msgId, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::SendEnvelopeNow(CString strKey)
{
	DataLng lotId = GetIdFromKey(szEnvelope, strKey);
	DataStr errorMessage;

	CALL_WEB_METHOD(m_pTbSenderInterface->UploadSingleLot(lotId, errorMessage));
	return bOk;
}

//-----------------------------------------------------------------------------
DataLng BDPostaliteDocumentsGraphicNavigation::GetIdFromKey(CString searchKey, CString strKey)
{
	if (strKey.Find(searchKey) < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	strKey.Replace(searchKey, _T(""));
	long id = _ttol(strKey);
	DataLng msgId(id);
	return msgId;
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::LoadTree()
{
	GetNotValidView();

	CString previouslySelectedKey = m_pTreeView->GetSelectedNodeKey();

	m_pTreeView->ClearTree();
	m_pTreeView->SetNodeStateIcon(TRUE);

	m_pTreeView->AddImage(szRootImage,						m_strRootPath + _T("\\Root.png"));
	m_pTreeView->AddImage(szMessageImage,					m_strRootPath + _T("\\Message.png"));
	m_pTreeView->AddImage(szEnvelopeImage,					m_strRootPath + _T("\\Envelope.png"));
	m_pTreeView->AddImage(szUnassignedMessageImage,			m_strRootPath + _T("\\UnassignedMessage.png"));
	m_pTreeView->AddImage(szUnassignedMessagesImage,		m_strRootPath + _T("\\UnassignedMessages.png"));
	m_pTreeView->AddImage(szEnvelopeUploadedImage,			m_strRootPath + _T("\\UploadedEnvelope.png"));
	m_pTreeView->AddImage(szCloseEnvelopeImage,				m_strRootPath + _T("\\ClosedEnvelope.png"));
	m_pTreeView->AddImage(szWarningEnvelopeImage,			m_strRootPath + _T("\\WarningEnvelope.png"));
	m_pTreeView->AddImage(szErrorEnvelopeImage,				m_strRootPath + _T("\\ErrorEnvelope.png"));
	m_pTreeView->AddImage(szWaitingEnvelopeImage,			m_strRootPath + _T("\\WaitingEnvelope.png"));
	m_pTreeView->AddImage(szEnvelopeSentImage,				m_strRootPath + _T("\\EnvelopeSent.png"));

	m_pTreeView->AddNode(_TB("Envelopes"), szRoot, szRootImage);
	m_pTreeView->InsertChild(szRoot, _TB("Not enveloped"), szUnassigned, szUnassignedMessagesImage);
	
	LoadAllottedMessages();
	LoadUnassignedMessages();

	EstimateCostForUnsentMessages();

	m_pTreeView->AddControls();
	m_pTreeView->SetViewContextMenu(TRUE);

	CTreeNodeAdvWrapperObj* pNode = m_pTreeView->GetNode(szUnassigned);
	if (pNode && !pNode->IsExpanded())
		m_pTreeView->ToggleNode(szUnassigned);

	m_pTreeView->SetNodeAsSelected(szRoot);

	if (!previouslySelectedKey.IsEmpty())
		m_pTreeView->SelectNode(previouslySelectedKey);
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::LoadAllottedMessages()
{
	if (m_pTblLots->IsOpen())
		m_pTblLots->Close();
	
	m_pLots->SetQualifier();
	m_pQueueMessage->SetQualifier(); 
	
	m_pTblLots->Open();
	m_pTblLots->SelectAll();

	m_pTblLots->Select(m_pQueueMessage, m_pQueueMessage->f_MsgID);
	m_pTblLots->Select(m_pQueueMessage, m_pQueueMessage->f_LotID);
	m_pTblLots->Select(m_pQueueMessage, m_pQueueMessage->f_City);
	m_pTblLots->Select(m_pQueueMessage, m_pQueueMessage->f_Subject);
	m_pTblLots->Select(m_pQueueMessage, m_pQueueMessage->f_Addressee);

	m_pTblLots->FromTable(m_pLots);
	m_pTblLots->FromTable(m_pQueueMessage); 
	m_pTblLots->AddCompareColumn(m_pLots->f_LotID, m_pQueueMessage, m_pQueueMessage->f_LotID);

	if (m_bOnlyToSend)
	{
		CString columnStatusExt = m_pLots->GetQualifiedColumnName(&m_pLots->f_StatusExt);
		m_pTblLots->m_strFilter += cwsprintf
			(
			_T(" AND ((%s <> %d) AND (%s <> %d) AND (%s <> %d))"),
			columnStatusExt,
			CPostaLiteAddress::Spedito,
			columnStatusExt,
			CPostaLiteAddress::SpeditoConInesitati,
			columnStatusExt,
			CPostaLiteAddress::Annullato
			);
	}

	if (m_bFilterByDate)
	{
		m_FilterDateTo.SetTime(23, 59, 59);
		m_pTblLots->m_strFilter += cwsprintf
			(
			_T(" AND ((%s >= %s) AND (%s <= %s))"),
			(LPCTSTR) m_pLots->GetQualifiedColumnName(&m_pLots->f_SendAfter),
			(LPCTSTR) (AfxGetDefaultSqlConnection()->NativeConvert(&m_FilterDateFrom)),
			(LPCTSTR) m_pLots->GetQualifiedColumnName(&m_pLots->f_SendAfter),
			(LPCTSTR) (AfxGetDefaultSqlConnection()->NativeConvert(&m_FilterDateTo))
			);
	}

	if (m_bFilterByWorker)
	{
		m_pTblLots->m_strFilter += cwsprintf
			(
			_T(" AND (exists (SELECT LotId From TB_MsgQueue x where x.TBCreatedID = %lu and TB_MsgQueue.LotId = x.LotId))"),
			(long)m_LastSelectedWorkerID
			);
	}
	
	CString columnMsgId = m_pQueueMessage->GetQualifiedColumnName(&m_pQueueMessage->f_MsgID);
	m_pTblLots->m_strFilter += cwsprintf
			(
				_T(" Order by %s Desc"),
				columnMsgId
			);
	TRY
	{
		m_pTblLots->Query();
	
		while (!m_pTblLots->IsEOF())
		{
			CString lotKey = AddEnvelopeElement();
			AddMessageElement(lotKey);
			
			m_pTblLots->MoveNext();
		}
	}
	CATCH (SqlException, e)
	{
		CString err = e->m_strError;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::EstimateCostForUnsentMessages()
{
	if (m_pTblLots->IsOpen())
		m_pTblLots->Close();
	
	m_pLots->SetQualifier();
	m_pTblLots->Open();

	m_pTblLots->Select(m_pLots, m_pLots->f_LotID);
	m_pTblLots->Select(m_pLots, m_pLots->f_Status);
	m_pTblLots->Select(m_pLots, m_pLots->f_StatusExt);
	m_pTblLots->Select(m_pLots, m_pLots->f_TotalAmount);
	m_pTblLots->Select(m_pLots, m_pLots->f_PostageAmount);
	m_pTblLots->Select(m_pLots, m_pLots->f_PrintAmount);

	m_pTblLots->FromTable(m_pLots);

	m_pTblLots->m_strFilter += cwsprintf
			(
			_T(" (%s = %d) OR (%s = %d) OR (%s = %d)"),
			m_pLots->GetQualifiedColumnName(&m_pLots->f_StatusExt),
			CPostaLiteAddress::None,
			m_pLots->GetQualifiedColumnName(&m_pLots->f_Status),
			CPostaLiteAddress::Allotted,
			m_pLots->GetQualifiedColumnName(&m_pLots->f_Status),
			CPostaLiteAddress::Closed
			);

	TRY
	{
		m_pTblLots->Query();
		m_UnsentMessageEstimate = 0;
		while (!m_pTblLots->IsEOF())
		{
			m_UnsentMessageEstimate += m_pLots->f_TotalAmount;
			m_pTblLots->MoveNext();
		}
	}
	CATCH (SqlException, e)
	{
		CString err = e->m_strError;
	}
	END_CATCH

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::LoadUnassignedMessages()
{
	if (m_pTblQueue->IsOpen())
		m_pTblQueue->Close();
	
	m_pTblQueue->Open();
	m_pTblQueue->SelectAll();
	m_pTblQueue->FromTable(m_pQueueMessage); 
	m_pTblQueue->AddParam		(_T("p1"),	m_pQueueMessage->f_LotID);
	m_pTblQueue->AddFilterColumn(			m_pQueueMessage->f_LotID);
	m_pTblQueue->SetParamValue	(_T("p1"), (DataLng)0);

	if (m_bFilterByWorker)
	{
		m_pTblQueue->AddParam		(_T("p2"),	m_pQueueMessage->f_TBCreatedID);
		m_pTblQueue->AddFilterColumn(			m_pQueueMessage->f_TBCreatedID);
		m_pTblQueue->SetParamValue	(_T("p2"), m_LastSelectedWorkerID);
	}

	TRY
	{
		m_pTblQueue->Query();
	
		while (!m_pTblQueue->IsEOF())
		{
			AddMessageElement(szUnassigned);
			
			m_pTblQueue->MoveNext();
		}
	}
	CATCH (SqlException, e)
	{
		CString err = e->m_strError;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
CString BDPostaliteDocumentsGraphicNavigation::AddEnvelopeElement()
{
	CString key = szEnvelope + m_pLots->f_LotID.Str();
	CString envText = _TB("Envelope ") + m_pLots->f_LotID.Str();
	CString nodeText = cwsprintf(_T("%s ( %s )"), envText, m_pLots->f_Addressee.Str());
	CString image = GetImageForNode(m_pLots);

	if (!m_pTreeView->ExistsNode(key))
		m_pTreeView->InsertChild(szRoot, nodeText, key, image);

	return key;
}

//-----------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::AddMessageElement(const CString& parentKey)
{
	CString nodeKey = szMsg + m_pQueueMessage->f_MsgID.Str();

	CString nodeName = m_pQueueMessage->f_LotID == 0 
		? cwsprintf(_T("%s - %s"), m_pQueueMessage->f_Addressee.Str(), m_pQueueMessage->f_Subject.Str())
		: m_pQueueMessage->f_Subject.Str();
	
	m_pTreeView->InsertChild(parentKey, nodeName, nodeKey, szMessageImage);
}

//----------------------------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::AddDetail(const DataStr& aName, const DataObj& aValue, BOOL bHasHyperLink)
{
	TEnhPostaliteDocumentsGraphicNavigationDetail* pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) m_pDBTPostaliteDocumentsGraphicNavigationDetail->AddRecord();

	pRec->l_FieldName		= aName;
	pRec->l_HasHyperlink	= bHasHyperLink;
	pRec->l_IsSeparator		= FALSE;

	DataType aType = aValue.GetDataType();

	if (aType == DataType::Double || aType == DataType::Money)
	{
		if (!aValue.IsEmpty())
			pRec->l_FieldValue = aValue.Str(15, 2 /*m_MoneyDecimalNumbers*/).Trim();
	}
	else if (aType == DataType::Quantity)
		pRec->l_FieldValue = aValue.Str(15, 2 /*m_QuantityDecimalNumbers*/).Trim();
	
	else if (aType == DataType::Percent)
		pRec->l_FieldValue = aValue.Str(3, 2).Trim();
	
	else if (aType == DataType::Date || aType == DataType::DateTime)
	{
		if (!aValue.IsEmpty())
		{
			DataDate aDate;
			aDate.Assign(aValue);
			pRec->l_FieldValue = aValue.Str(0, 1);
		}
	}
	else if (aType == DataType::Bool)
		pRec->l_FieldValue = aValue.Str(1);
	
	else if (aType == DataType::ElapsedTime)
	{
		if (!aValue.IsEmpty())
			pRec->l_FieldValue = aValue.FormatData();
	}
	else if (aType == DataType::String)
		pRec->l_FieldValue = aValue;
	
	else
		pRec->l_FieldValue = aValue.FormatData();
	
	pRec->l_FieldValue.SetReadOnly();
}

//----------------------------------------------------------------------------------------------
void BDPostaliteDocumentsGraphicNavigation::AddSeparator(const DataStr& aName)
{
	TEnhPostaliteDocumentsGraphicNavigationDetail* pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) m_pDBTPostaliteDocumentsGraphicNavigationDetail->AddRecord();

	pRec->l_FieldName		= aName;
	pRec->l_HasHyperlink	= FALSE;
	pRec->l_IsSeparator		= TRUE;
	pRec->l_FieldValue		= _T("");
	pRec->l_FieldValue.SetReadOnly();
}

//-----------------------------------------------------------------------------
BOOL BDPostaliteDocumentsGraphicNavigation::HasHyperLink(TEnhPostaliteDocumentsGraphicNavigationDetail* pRec)
{
	return pRec->l_HasHyperlink && !pRec->l_FieldValue.IsEmpty();
}

///////////////////////////////////////////////////////////////////////////////
//					CPostaLiteWorkersCombo - implementation
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE (CPostaLiteWorkersCombo, CStrCombo)

//-----------------------------------------------------------------------------
CPostaLiteWorkersCombo::CPostaLiteWorkersCombo()
	:
	CStrCombo(),
	m_pIDMap (NULL)
{
	m_pIDMap	= new CMapPtrToPtr();
}
 
//-----------------------------------------------------------------------------
CPostaLiteWorkersCombo::CPostaLiteWorkersCombo(UINT nBtnIDBmp, DataStr* pData, CString aUserName)
	:
	CStrCombo	(nBtnIDBmp, pData),
	m_pIDMap	(NULL)
{
	m_pIDMap	= new CMapPtrToPtr();
	m_UserName	= aUserName;
}

//-----------------------------------------------------------------------------
BOOL CPostaLiteWorkersCombo::OnInitCtrl()
{
	BOOL bOK = CStrCombo::OnInitCtrl();
	FillListBox();
	
	return bOK;
}

//-----------------------------------------------------------------------------
void CPostaLiteWorkersCombo::OnFillListBox()
{
	CStrCombo::OnFillListBox(); 
	CleanMap();
	
	CWorker* worker = NULL;
	CWorkersTableObj* s = AfxGetWorkersTable();
	
	int count = s->GetWorkersCount();
	int i = 0;
	for (int j = 0; j <= count ; j++)
	{
		worker = s->GetWorkerAt(i);
		if (worker == NULL)
			continue;
		CString temp (GetFullWorkerName(worker));
		AddAssociation(temp, temp);

		AddToMap(i++, worker->GetWorkerID());
	}
}

//----------------------------------------------------------------------------
void CPostaLiteWorkersCombo::CleanMap()
{	
	if (m_pIDMap)
	{
		DataLng* pID;	
		void*	 key;
		POSITION pos = m_pIDMap->GetStartPosition();
		
		for(pos = m_pIDMap->GetStartPosition(); pos;)
		{
			m_pIDMap->GetNextAssoc(pos, key, (void*&)pID);
			SAFE_DELETE(pID);
		}
		m_pIDMap->RemoveAll();
	}
}

//-----------------------------------------------------------------------------
void CPostaLiteWorkersCombo::AddToMap(int aIndex, DataLng aID)
{
	DataLng* pID;

	if (!m_pIDMap->Lookup((void*) aIndex, (void*&)pID))
	{
		pID = new DataLng; *pID = aID;
		m_pIDMap->SetAt((void*) aIndex, (void*&)pID);
	}
}

//-----------------------------------------------------------------------------	
DataLng CPostaLiteWorkersCombo::GetWorkerID()
{
	DataLng* pID;
	int aIndex = GetCurSel();

	if (m_pIDMap->Lookup((void*)aIndex, (void*&)pID))
		return *pID;
	else
		return 0;
}

//----------------------------------------------------------------------------
CPostaLiteWorkersCombo::~CPostaLiteWorkersCombo()
{
	CleanMap();
	SAFE_DELETE(m_pIDMap);
}

//////////////////////////////////////////////////////////////////////////////
// DBTPostaliteDocumentsGraphicNavigationDetail
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTPostaliteDocumentsGraphicNavigationDetail, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTPostaliteDocumentsGraphicNavigationDetail::DBTPostaliteDocumentsGraphicNavigationDetail
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("DBTPostaliteDocumentsGraphicNavigationDetail"), ALLOW_EMPTY_BODY, FALSE) 
{
}

//-----------------------------------------------------------------------------
DataObj* DBTPostaliteDocumentsGraphicNavigationDetail::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{ 
	return NULL; 
}

//-----------------------------------------------------------------------------
void DBTPostaliteDocumentsGraphicNavigationDetail::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{   
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TEnhPostaliteDocumentsGraphicNavigationDetail)));
}

//-----------------------------------------------------------------------------
void DBTPostaliteDocumentsGraphicNavigationDetail::SetCurrentRow(int nRow)
{   
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

//////////////////////////////////////////////////////////////////////////////
// TEnhCompanyLayoutDetail
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TEnhPostaliteDocumentsGraphicNavigationDetail, SqlRecord)

//-----------------------------------------------------------------------------
TEnhPostaliteDocumentsGraphicNavigationDetail::TEnhPostaliteDocumentsGraphicNavigationDetail(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TEnhPostaliteDocumentsGraphicNavigationDetail::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR (_NS_LFLD("TEnhPostaliteDocumentsGraphicNavigationDetail_P1"),		l_FieldName,		32);
		LOCAL_STR (_NS_LFLD("TEnhPostaliteDocumentsGraphicNavigationDetail_P2"),		l_FieldValue,		64);
		LOCAL_DATA(_NS_LFLD("TEnhPostaliteDocumentsGraphicNavigationDetail_P3"),		l_HasHyperlink);
		LOCAL_DATA(_NS_LFLD("TEnhPostaliteDocumentsGraphicNavigationDetail_P4"),		l_IsSeparator);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TEnhPostaliteDocumentsGraphicNavigationDetail::GetStaticName() { return _NS_TBL("TB_DBMARK"); }





