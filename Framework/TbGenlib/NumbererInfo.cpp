#include "stdafx.h"

#include "TBCommandInterface.h"
#include "PARSOBJ.H"
#include "NumbererInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
///						CNumbererRequest
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CNumbererRequest, CObject)

//-----------------------------------------------------------------------------	
CNumbererRequest::CNumbererRequest(CObject* pOwner, DataObj* pData, const CString& sEntity, DataBool* pDisabled, CNumbererRequestParams* pParams /*NULL*/)
	:
	IBehaviourRequest		(pOwner, NULL),
	m_pParams				(pParams),
	m_pData					(pData),
	m_pOldData				(NULL),
	m_bNotifyUI				(TRUE),
	m_bIsPrimaryKey			(FALSE),
	m_pNumberingDisabled	(pDisabled),
	m_pOwnsNumberingDisabled(FALSE)
{
	if (!sEntity.IsEmpty())
	{
		m_nsEntity.AutoCompleteNamespace(CTBNamespace::ENTITY, sEntity, m_nsEntity);
		SetReceiver(AfxGetTbCmdManager()->GetBehaviourService(m_nsEntity.ToString()));
	}
	if (!m_pNumberingDisabled)
	{
		m_pNumberingDisabled = new DataBool();
		m_pOwnsNumberingDisabled = TRUE;
	}
}

//-----------------------------------------------------------------------------	
CNumbererRequest::~CNumbererRequest()
{
	if (m_pOwnsNumberingDisabled)
		SAFE_DELETE(m_pNumberingDisabled);

	SAFE_DELETE(m_pParams);
}

//-----------------------------------------------------------------------------	
inline const BOOL& CNumbererRequest::HasNotifyUI () const { return m_bNotifyUI; }

//-----------------------------------------------------------------------------	
inline const CTBNamespace& CNumbererRequest::GetEntity() const { return m_nsEntity; }

//-----------------------------------------------------------------------------	
inline DataObj* CNumbererRequest::GetData () const {  return m_pData; }

//-----------------------------------------------------------------------------	
inline DataObj* CNumbererRequest::GetOldData () const {  return m_pOldData; }


//-----------------------------------------------------------------------------	
inline const BOOL& CNumbererRequest::IsPrimaryKey () const {  return m_bIsPrimaryKey; }

//-----------------------------------------------------------------------------	
DataBool* CNumbererRequest::GetNumberingDisabled ()
{
	return m_pNumberingDisabled;
}

//-----------------------------------------------------------------------------	
const DataBool&	CNumbererRequest::GetDatabaseNumberingDisabled  () const
{
	return m_bDatabaseNumberingDisabled;
}

//-----------------------------------------------------------------------------	
const DataBool&	CNumbererRequest::GetEnableCtrlInEditMode  () const
{
	return m_bEnableCtrlInEditMode;
}

//-----------------------------------------------------------------------------	
CFormatMask* CNumbererRequest::GetFormatMask ()
{
	return &m_FormatMask;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetIsPrimaryKey(BOOL value) 
{ 
	m_bIsPrimaryKey = value; 
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetNotifyUI (const BOOL& value) 
{ 
	m_bNotifyUI = value; 
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetEntity (const CString& value) 
{ 
	m_nsEntity.Clear();
	m_nsEntity.AutoCompleteNamespace(CTBNamespace::ENTITY, value, m_nsEntity);
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetData (DataObj* pData) 
{ 
	m_pData = pData; 
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetOldData (DataObj* pOldData) 
{ 
	m_pOldData = pOldData; 
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetFormatMask (const CFormatMask& value)
{
	m_FormatMask = value;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetNumberingDisabled (const BOOL& value)
{
	*m_pNumberingDisabled = value;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetEnableCtrlInEditMode (const BOOL& value)
{
	m_bEnableCtrlInEditMode = value;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetDatabaseNumberingDisabled (const BOOL& value)
{
	m_bDatabaseNumberingDisabled = value;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::OnRequestExecuted (const int& nEvent, IBehaviourContext* pContext)
{
	if (!m_bNotifyUI)
		return;

	CDataEventsObj* pEvents = (CDataEventsObj*)m_pData->GetDataEvents();
	while (pEvents) 
	{
		CParsedCtrlEvents* pCtrlEvents = dynamic_cast<CParsedCtrlEvents*>(pEvents);
		if (pCtrlEvents)
		{
			CParsedCtrl* pControl = pCtrlEvents->GetControl();
			if (pControl && pControl->GetCtrlCWnd() && pControl->GetCtrlCWnd()->m_hWnd)
				pControl->UpdateCtrlView();
		}
		
		pEvents = pEvents->GetParent();
	}
}

//-----------------------------------------------------------------------------	
CNumbererRequest* CNumbererRequest::GetRequestFor (DataObj* pData, IBehaviourContext* pContext, CString sEntity /*= _T("")*/)
{
	CTBNamespace aEntityNs(CTBNamespace::ENTITY, sEntity);
	
	CArray<IBehaviourRequest*> requests;
	pContext->GetRequestsBy(RUNTIME_CLASS(CNumbererRequest), requests);
	
	for (int i=0; i <= requests.GetUpperBound(); i++)
	{
		CNumbererRequest* pRequest = dynamic_cast<CNumbererRequest*>(requests.GetAt(i));
		if (pRequest && pRequest->GetData() == pData && (sEntity.IsEmpty() || CTBNamespace(pRequest->GetEntity()) == aEntityNs))
			return pRequest;
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
CNumbererRequestParams* CNumbererRequest::GetParams	() const
{
	return m_pParams;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetParams	(CNumbererRequestParams* pParams)
{
	m_pParams = pParams;
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	GetJson(jsonSerializer, AfxGetTBThread()->m_pActiveWnd);
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::GetJson(CJsonSerializer& jsonSerializer, CWnd* pWnd)
{
	// nell'entità mi sono appoggiata il control
	jsonSerializer.OpenObject(m_nsEntity.ToUnparsedString());
	jsonSerializer.WriteBool(_T("enableStateInEdit"), m_bDatabaseNumberingDisabled == TRUE ? false : true);
	jsonSerializer.WriteBool(_T("enableCtrlInEdit"), m_bEnableCtrlInEditMode == TRUE ? true : false);
	jsonSerializer.WriteString(_T("formatMask"), m_FormatMask.GetMask());

	jsonSerializer.CloseObject();
}

//-----------------------------------------------------------------------------	
void CNumbererRequest::SetJson(CJsonParser& jsonParser)
{
	if (jsonParser.BeginReadObject(m_nsEntity.ToString()))
	{
		m_bDatabaseNumberingDisabled = jsonParser.ReadBool(_T("enableStateInEdit"));
		m_bEnableCtrlInEditMode = jsonParser.ReadBool(_T("enableCtrlInEdit"));
		m_FormatMask.SetMask(jsonParser.ReadString(_T("formatMask")));
		jsonParser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------	
CString CNumbererRequest::GetComponentId()
{
	return m_nsEntity.ToUnparsedString();
}

////////////////////////////////////////////////////////////////////////////////
///						CDateNumbererRequestParams
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
CDateNumbererRequestParams::CDateNumbererRequestParams (DataDate* pDataDate)
	:
	m_pDocDate (pDataDate)
{
}

//-----------------------------------------------------------------------------	
inline DataDate* CDateNumbererRequestParams::GetDocDate () const {  return m_pDocDate; }

//-----------------------------------------------------------------------------	
inline DataDate* CDateNumbererRequestParams::GetOldDocDate () const {  return m_pOldDocDate; }

//-----------------------------------------------------------------------------	
void CDateNumbererRequestParams::SetDocDate (DataDate* pDocDate) 
{ 
	m_pDocDate = pDocDate; 
}

//-----------------------------------------------------------------------------	
void CDateNumbererRequestParams::SetOldDocDate (DataDate* pOldDocDate) 
{ 
	m_pOldDocDate = pOldDocDate; 
}

///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CFormatMaskLegendaMng::CFormatMaskLegendaMng(CWnd* pParent)
	:
	m_pDlg		(NULL),
	m_pParent	(pParent)
{
}

//-----------------------------------------------------------------------------
CFormatMaskLegendaMng::~CFormatMaskLegendaMng()
{
	if (m_pDlg)
		ToggleLegenda(FALSE);
}

//-----------------------------------------------------------------------------
void CFormatMaskLegendaMng::ToggleLegenda (BOOL bValue)
{
	if (bValue && !m_pDlg)
		m_pDlg = new CFormatMaskLegendaDlg(this);
	else if (!bValue && m_pDlg)
		((CFormatMaskLegendaDlg*)m_pDlg)->OnCancel();
}

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(CFormatMaskLegendaDlg, CLocalizableDialog)
//-----------------------------------------------------------------------------	
BEGIN_MESSAGE_MAP(CFormatMaskLegendaDlg, CLocalizableDialog)
	ON_WM_CTLCOLOR()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------	
CFormatMaskLegendaDlg::CFormatMaskLegendaDlg (CFormatMaskLegendaMng* pManager) 
	: 
	CLocalizableDialog	(IDD_ENTITIES_FORMATMASKS, pManager->m_pParent),
	m_pManager			(pManager)
{
	Create(IDD_ENTITIES_FORMATMASKS, m_pManager->m_pParent);
}

//-----------------------------------------------------------------------------	
BOOL CFormatMaskLegendaDlg::OnInitDialog() 
{
	__super::OnInitDialog();
	CRect parentRect, myRect;
	m_pManager->m_pParent->GetWindowRect(&parentRect);
	GetWindowRect(&myRect);
	SetWindowPos(NULL, parentRect.right - myRect.Width() , parentRect.top + 1, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE);
	return TRUE;
}

//-----------------------------------------------------------------------------	
HBRUSH CFormatMaskLegendaDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	COLORREF bkg = RGB(255, 255, 240);
	HBRUSH hbr = CreateSolidBrush(bkg);
	pDC->SetBkColor(bkg);

	if (pWnd->GetDlgCtrlID() == IDC_ENTITIES_FORMATMASKS_CHARS)
		pDC->SetTextColor(RGB(178, 34, 34));
	else if (pWnd->GetDlgCtrlID() == IDC_ENTITIES_FORMATMASKS_CHARSEXP)
		pDC->SetTextColor(RGB(18, 10, 143));
	else if (pWnd->GetDlgCtrlID() == IDC_ENTITIES_FORMATMASKS_GRPEX || pWnd->GetDlgCtrlID() == IDC_ENTITIES_FORMATMASKS_GRPPH)
		pDC->SetTextColor(RGB(18, 10, 143));
	else if (pWnd->GetDlgCtrlID() == IDC_ENTITIES_FORMATMASKS_SAMPLE)
		pDC->SetTextColor(RGB(0, 100, 0));
	return hbr;
}

//-----------------------------------------------------------------------------	
void CFormatMaskLegendaDlg::OnCancel ()
{
	m_pManager->m_pDlg = NULL;
	EndDialog(IDCANCEL);
	delete this;
}