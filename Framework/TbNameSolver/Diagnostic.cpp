#include "stdafx.h"

#include "ThreadContext.h"
#include "JsonSerializer.h"

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szRowBanner[] = _T("=================================================");
static const TCHAR szMessageLineFeed[] = _T("\r\n");
///////////////////////////////////////////////////////////////////////////////
//							General Functions
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
CDiagnostic* AFXAPI AfxGetDiagnostic()
{
	return AfxGetThreadContext()->GetDiagnostic();
}

//-----------------------------------------------------------------------------
CDiagnostic* CloneDiagnostic(BOOL bClearMessages /*FALSE*/)
{
	CDiagnostic* pDiagnostic = AfxGetDiagnostic()->Clone();
	if (bClearMessages)
		AfxGetDiagnostic()->ClearMessages();

	return pDiagnostic;
}

//-----------------------------------------------------------------------------
TB_EXPORT int WriteEventViewerMessage(
	const CString& strMsg,
	WORD			wMsgType			/*EVENTLOG_INFORMATION_TYPE*/,
	const CString	sApplicationName	/*MA_CLIENT_LOGNAME*/,
	DWORD			dwEventID			/*0*/,
	WORD			wCategoryID			/*0*/
)
{
	if (strMsg.IsEmpty())
		return 4;

	HANDLE hEventLog = OpenEventLog(NULL, sApplicationName);

	if (hEventLog == NULL)
	{
		// if key does not exist I try to write into default event log
		hEventLog = OpenEventLog(NULL, sApplicationName);
		if (hEventLog == NULL)
		{
			ASSERT(hEventLog != NULL);
			TRACE2("Failed to RegisterEventSource into EventViewer section %s for msg %s", strMsg, sApplicationName);
			return 2;
		}
	}

	// No security attributes set
	PSID pSecurity = 0;
	// No raw data so size is 0
	DWORD dwDataSize = 0;
	// No raw data
	LPVOID* lpRawData = 0;

	// I log only one string
	WORD wNumofStrs = 1;
	LPCTSTR pStr = (LPCTSTR)strMsg;

	// Logging string to event viewer with type information
	BOOL bOk = ReportEvent
	(
		hEventLog,
		wMsgType,
		wCategoryID,
		dwEventID,
		pSecurity,
		wNumofStrs,
		dwDataSize,
		&pStr,
		lpRawData
	);

	if (!bOk)
	{
		TRACE2("Failed to Report Event into EventViewer section %s for msg %s", strMsg, sApplicationName);
		CloseEventLog(hEventLog);
		return 3;
	}

	// Closes the handle to the specified event log
	CloseEventLog(hEventLog);
	return 1;
}

///////////////////////////////////////////////////////////////////////////////
//				class CDiagnosticItem implementation
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDiagnosticItem, CObject)
//-----------------------------------------------------------------------------
CDiagnosticItem::CDiagnosticItem(const CString& strErrCode, const CString& sMessage, const CDiagnostic::MsgType eType)
	:
	m_eType(eType),
	m_strErrCode(strErrCode)
{
	m_sMessage = sMessage;
}

//-----------------------------------------------------------------------------
void CDiagnosticItem::Assign(CDiagnosticItem* pItem)
{
	m_sMessage = pItem->m_sMessage;
	m_eType = pItem->m_eType;
	m_strErrCode = pItem->m_strErrCode;
}

//-----------------------------------------------------------------------------
CDiagnosticItem* CDiagnosticItem::Clone()
{
	CDiagnosticItem* pNew = new CDiagnosticItem(m_strErrCode, m_sMessage, m_eType);

	pNew->Assign(this);

	return pNew;
}

//-----------------------------------------------------------------------------
const CString& CDiagnosticItem::GetMessageText() const
{
	return m_sMessage;
}

//-----------------------------------------------------------------------------
const CDiagnostic::MsgType	CDiagnosticItem::GetType() const
{
	return m_eType;
}

//-----------------------------------------------------------------------------
const CString& CDiagnosticItem::GetErrCode() const
{
	return m_strErrCode;
}

//------------------------------------------------------------------------------
const int CDiagnosticItem::GetMessagesCount()
{
	return 1;
}

//------------------------------------------------------------------------------
CDiagnosticItem* CDiagnosticItem::GetItemAt(const int& nIdx, int& nCurrIndex)
{
	nCurrIndex++;
	return this;
}
//---------------------------------------------------------------------------
void CDiagnosticItem::ToJson(CJsonSerializer& ser)
{
	ser.WriteString(_T("text"), GetMessageText());
	ser.WriteInt(_T("type"), GetType());
}
//---------------------------------------------------------------------------
void CDiagnosticItem::PurgeEmptyItems()
{
	
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticItem::HasMessages(CDiagnostic::MsgType aType, const BOOL bAnyType /*FALSE*/, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	return  (bAnyType && GetType() != CDiagnostic::Banner) || GetType() == aType;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticItem::HasMessage(const CString& sMessage, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	return  sMessage.CompareNoCase(m_sMessage) == 0;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticItem::HasErrorCode(const CString& strErrCode, const BOOL /* = FALSE*/, BOOL /* = FALSE*/)
{
	return  strErrCode.CompareNoCase(m_strErrCode) == 0;
}


///////////////////////////////////////////////////////////////////////////////
//          Class CDiagnosticLevel implemetation
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDiagnosticLevel, CDiagnosticItem)
//---------------------------------------------------------------------------
CDiagnosticLevel::CDiagnosticLevel(CDiagnosticLevel* pParent, const CString& strOpeningBanner, const BOOL& bTraceInEventViewer)
	:
	CDiagnosticItem(szNoErrorCode, _T(""), CDiagnostic::Banner),
	m_bTraceInEventViewer(bTraceInEventViewer),
	m_pParent(NULL),
	m_bForceBanner(FALSE)
{
	m_pParent = pParent;
	m_strOpeningBanner = strOpeningBanner;
}

//---------------------------------------------------------------------------
CDiagnosticLevel::~CDiagnosticLevel()
{
	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);
		if (pItem)
			delete pItem;
	}

	m_arMessages.RemoveAll();
}

//---------------------------------------------------------------------------
void CDiagnosticLevel::Clear()
{
	//m_strOpeningBanner.Empty();

	CDiagnosticItem* pItem;
	for (int i = m_arMessages.GetUpperBound(); i >= 0; i--)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (!pItem)
			continue;

		if (pItem->IsNestedLevel())
			((CDiagnosticLevel*)pItem)->Clear();
		else
		{
			delete pItem;
			m_arMessages.RemoveAt(i);
		}
	}
}
//---------------------------------------------------------------------------
void CDiagnosticLevel::PurgeEmptyItems()
{
	if (m_arMessages.IsEmpty())
		return;
	CDiagnosticItem* pItem;
	for (int i = m_arMessages.GetUpperBound(); i >= 0; i--)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);
		pItem->PurgeEmptyItems();
		if (!pItem->HasMessages(CDiagnostic::Info, TRUE, TRUE))
		{
			m_arMessages.RemoveAt(i);
			delete pItem;
		}
	}
}

//---------------------------------------------------------------------------
void CDiagnosticLevel::ToJson(CJsonSerializer& ser)
{
	if (m_arMessages.IsEmpty())
		return;
	
	if (m_arMessages.GetCount() == 1)
	{
		//ho un solo figlio ed è un sottolivello: allora taglio il livello
		CDiagnosticItem* pItem = (CDiagnosticItem*)m_arMessages.GetAt(0);
		if (pItem->IsKindOf(RUNTIME_CLASS(CDiagnosticLevel)))
		{
			pItem->ToJson(ser);
			return;
		}

	}
	ser.OpenArray(_T("messages"));

	CDiagnosticItem* pItem;
	for (int i = 0; i < m_arMessages.GetCount(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);
		ser.OpenObject(i);
		pItem->ToJson(ser);
		ser.CloseObject();
	}
	ser.CloseArray();
}
//---------------------------------------------------------------------------
CDiagnosticLevel* CDiagnosticLevel::Clone()
{
	// forst of all I clone the root, parent will be assigned by the caller
	CDiagnosticLevel* pNewLevel = new CDiagnosticLevel(NULL, m_strOpeningBanner, m_bTraceInEventViewer);

	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (pItem)
			pNewLevel->Add(pItem->Clone());
	}

	return pNewLevel;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticLevel::HasMessages(CDiagnostic::MsgType aType, const BOOL bAnyType /*FALSE*/, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	// messages of this level
	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (!pItem)
			continue;

		// recursion on the child levels
		if (!bIncludeChildLevels && pItem->IsNestedLevel())
			continue;

		if (pItem->HasMessages(aType, bAnyType, bIncludeChildLevels))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticLevel::HasMessage(const CString& sMessage, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	// messages of this level
	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (!pItem)
			continue;

		// recursion on the child levels
		if (!bIncludeChildLevels && pItem->IsNestedLevel())
			continue;

		if (pItem->HasMessage(sMessage, bIncludeChildLevels))
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticLevel::HasErrorCode(const CString& strErrCode, const BOOL bIncludeChildLevels /*FALSE*/, BOOL bRemoveMessage /*FALSE*/)
{
	// messages of this level
	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (!pItem)
			continue;

		// recursion on the child levels
		if (!bIncludeChildLevels && pItem->IsNestedLevel())
			continue;

		if (pItem->HasErrorCode(strErrCode, bIncludeChildLevels, bRemoveMessage))
		{
			if (bRemoveMessage && !pItem->IsNestedLevel()) // do not remove it if it is a level
			{
				delete pItem;
				m_arMessages.RemoveAt(i);
			}
			return TRUE;
		}
	}

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnosticLevel::IsLastMessage(const CString& sMessage, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	if (m_arMessages.GetSize() == 0)
		return FALSE;

	CDiagnosticItem* pItem = (CDiagnosticItem*)m_arMessages.GetAt(m_arMessages.GetUpperBound());

	return sMessage.CompareNoCase(pItem->GetMessageText()) == 0;
}

//-----------------------------------------------------------------------------
CDiagnosticLevel* CDiagnosticLevel::StartSession(const CString& strOpeningBanner /* _T("")*/, BOOL bForceBanner/* FALSE)*/)
{
	CDiagnosticLevel* pNewLevel = new CDiagnosticLevel(this, strOpeningBanner, m_bTraceInEventViewer);
	pNewLevel->m_bForceBanner = bForceBanner;
	m_arMessages.Add(pNewLevel);
	return pNewLevel;
}

//-----------------------------------------------------------------------------
void CDiagnosticLevel::EndSession(const CString& strClosingBanner /*_T("")*/)
{
	// I have to close banner
	if (!m_strOpeningBanner.IsEmpty() && HasMessages() && !strClosingBanner.IsEmpty())
		Add(new CDiagnosticItem(0, szRowBanner, CDiagnostic::Banner));
}

//-----------------------------------------------------------------------------
int	CDiagnosticLevel::Add(CDiagnosticItem* pItem)
{
	ASSERT(pItem);
	if (!pItem)
		return -1;

	if (pItem->IsNestedLevel())
		((CDiagnosticLevel*)pItem)->m_pParent = this;

	if (
		m_bTraceInEventViewer &&
		(
			pItem->GetType() == CDiagnostic::Error ||
			pItem->GetType() == CDiagnostic::FatalError ||
			pItem->GetType() == CDiagnostic::Warning
			)
		)
		WriteEventViewerMessage(pItem->GetMessageText(), ToEventViewerMessageType(pItem->GetType()));
	return m_arMessages.Add(pItem);
}

//------------------------------------------------------------------------------
const int CDiagnosticLevel::GetMessagesCount()
{
	int nMessages = 0;
	CDiagnosticItem* pItem;
	for (int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (pItem)
			nMessages += pItem->GetMessagesCount();
	}

	return nMessages;
}

//------------------------------------------------------------------------------
CDiagnosticItem* CDiagnosticLevel::GetItemAt(const int& nIdx)
{
	int nCurrIndex = -1;
	return GetItemAt(nIdx, nCurrIndex);
}

//------------------------------------------------------------------------------
CDiagnosticItem* CDiagnosticLevel::GetItemAt(const int& nIdx, int& nCurrIndex)
{
	CDiagnosticItem* pItem;
	for (
		int i = 0; i <= m_arMessages.GetUpperBound(); i++)
	{
		pItem = (CDiagnosticItem*)m_arMessages.GetAt(i);

		if (!pItem)
			continue;

		// recursion on the child levels
		pItem = pItem->GetItemAt(nIdx, nCurrIndex);

		if (pItem && nCurrIndex == nIdx)
			return pItem;
	}

	return NULL;
}

//------------------------------------------------------------------------------
int	CDiagnosticLevel::ToEventViewerMessageType(CDiagnostic::MsgType aType)
{
	switch (aType)
	{
	case CDiagnostic::Info:
	case CDiagnostic::Banner:		return EVENTLOG_INFORMATION_TYPE;
	case CDiagnostic::Warning:		return EVENTLOG_WARNING_TYPE;
	case CDiagnostic::FatalError:
	case CDiagnostic::Error:		return EVENTLOG_ERROR_TYPE;
	default:
		ASSERT(FALSE);
		return EVENTLOG_INFORMATION_TYPE;
	}
}

///////////////////////////////////////////////////////////////////////////////
//				class CDiagnostic implementation
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDiagnostic, CObject);
//-----------------------------------------------------------------------------
CDiagnostic::CDiagnostic()
	:
	m_pViewer(NULL),
	m_pCurrLevel(NULL),
	m_pStartingLevel(NULL),
	m_bShowingMessages(FALSE),
	m_bHasSubLevels(FALSE)
{
	Initialize();
}

//------------------------------------------------------------------------------
CDiagnostic::~CDiagnostic()
{
	if (m_pStartingLevel)
		delete m_pStartingLevel;

	m_pCurrLevel = NULL;

	if (m_pViewer)
		delete m_pViewer;
}

//------------------------------------------------------------------------------
void CDiagnostic::ClearMessages(BOOL bAllLevels /*FALSE*/)
{
	if (bAllLevels)
	{
		m_pStartingLevel->Clear();
		m_bHasFatalError = FALSE;
		m_bHasSubLevels = FALSE;
	}
	else if (m_pCurrLevel)
		m_pCurrLevel->Clear();
}

//---------------------------------------------------------------------------
CWnd* CDiagnostic::ShowNoModal(int iX /*= 0*/, int iY /*= 0*/)
{
	if (!m_pViewer) {
		//non ho un visualizzatore
		return NULL;
	}
	return m_pViewer->ShowNoModal(this, iX, iY);
}

//---------------------------------------------------------------------------
BOOL CDiagnostic::Show(BOOL bClearMessages /*= TRUE*/)
{
	//evito il loop: se sto gia` visualizzando i messaggi, non devo farlo nuovamente
	if (IsShowingMessages() //sto gia' visualizzando i messaggi
		|| !MessageFound()) //non ho messaggi
		return TRUE;

	if (!m_pViewer) //non ho un visualizzatore
		return FALSE;
	UpdateDataView();//se ho un documento (CMessages) ne forzo l'UpdateDataView
	m_bShowingMessages = TRUE;
	BOOL bRet = m_pViewer->Show(this, bClearMessages);
	m_bShowingMessages = FALSE;
	if (bClearMessages)
		m_bHasSubLevels = FALSE;

	return bRet;
}
//------------------------------------------------------------------------------
void CDiagnostic::Initialize()
{
	m_bHasFatalError = FALSE;
	m_bHasSubLevels = FALSE;

	if (m_pStartingLevel)
		delete m_pStartingLevel;

	m_pStartingLevel = new CDiagnosticLevel(NULL, _T(""), FALSE);
	m_pCurrLevel = m_pStartingLevel;
}

//------------------------------------------------------------------------------
void CDiagnostic::StartSession(const CString& strOpeningBanner /*_T("")*/, BOOL bForceBanner /*FALSE*/)
{
	ASSERT(m_pCurrLevel);
	if (m_pCurrLevel)
	{
		m_pCurrLevel = m_pCurrLevel->StartSession(strOpeningBanner, bForceBanner);
		m_bHasSubLevels = TRUE;
	}
}

//------------------------------------------------------------------------------
void CDiagnostic::EndSession(const CString& strClosingBanner /*_T("")*/)
{
	ASSERT(m_pCurrLevel);
	if (!m_pCurrLevel)
		return;

	if (m_pCurrLevel->m_pParent == NULL)
	{
		ASSERT(FALSE);
		TRACE("CDiagnostic: EndSession method called without a preceding StartSession call!\n");
		return;
	}

	m_pCurrLevel->EndSession(strClosingBanner);
	CDiagnosticLevel* pOld = m_pCurrLevel;
	m_pCurrLevel = m_pCurrLevel->m_pParent;
	if (!pOld->HasMessages(CDiagnostic::Info, TRUE, TRUE))
	{
		for (int i = m_pCurrLevel->m_arMessages.GetUpperBound(); i>=0; i--)
		{
			if (m_pCurrLevel->m_arMessages[i] == pOld)
			{
				m_pCurrLevel->m_arMessages.RemoveAt(i);
				delete pOld;
			}
		}
	}
}

//------------------------------------------------------------------------------
void CDiagnostic::SetStartingBanner(const CString& strOpeningBanner)
{
	if (m_pStartingLevel) m_pStartingLevel->SetOpeningBanner(strOpeningBanner);
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::MessageFound(const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->HasMessages(CDiagnostic::Info, TRUE, TRUE) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::MessageFound(const CString& sMessage, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->HasMessage(sMessage, bIncludeChildLevels) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::ErrorCodeFound(const CString& strErrCode, const BOOL bIncludeChildLevels /*FALSE*/, BOOL bRemoveMessage /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->HasErrorCode(strErrCode, bIncludeChildLevels, bRemoveMessage) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::ErrorFound(const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? (pLevel->HasMessages(CDiagnostic::Error, FALSE, TRUE) || m_bHasFatalError) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::WarningFound(const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->HasMessages(CDiagnostic::Warning, FALSE, TRUE) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::InfoFound(const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->HasMessages(CDiagnostic::Info, FALSE, TRUE) : FALSE;
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::IsLastMessage(const CString& sMessage, const BOOL bIncludeChildLevels /*FALSE*/) const
{
	CDiagnosticLevel* pLevel = bIncludeChildLevels ? m_pStartingLevel : m_pCurrLevel;
	ASSERT(pLevel);

	return pLevel ? pLevel->IsLastMessage(sMessage, TRUE) : FALSE;
}

//------------------------------------------------------------------------------
int	CDiagnostic::Add(CDiagnosticItem* pItem)
{
	ASSERT(m_pCurrLevel);
	if (!m_pCurrLevel)
		return -1;

	if (pItem->GetType() == CDiagnostic::FatalError)
		m_bHasFatalError = TRUE;

	return m_pCurrLevel->Add(pItem);
}

//-----------------------------------------------------------------------------
void CDiagnostic::Add(
	const CString& strMessage,
	CDiagnostic::MsgType type /*Error*/,
	const CString strErrCode /*strNoErrorCode*/,
	BOOL bOnlyIfNotExist /*FALSE*/
)
{
	ASSERT(m_pCurrLevel);
	if (!m_pCurrLevel)
		return;

	// add message only if not exist
	if (bOnlyIfNotExist && m_pCurrLevel->IsLastMessage(strMessage))
		return;

	// if StartSession has been called, I insert the banner row when the
	// first message is added
	if (m_pCurrLevel->HasOpeningBanner() && !m_pCurrLevel->HasMessages() && (type != CDiagnostic::Info || m_pCurrLevel->m_bForceBanner))
	{
		Add(new CDiagnosticItem(0, szRowBanner, CDiagnostic::Banner));
		Add(new CDiagnosticItem(0, m_pCurrLevel->GetOpeningBanner(), CDiagnostic::Banner));
		Add(new CDiagnosticItem(0, szRowBanner, CDiagnostic::Banner));
	}

	Add(new CDiagnosticItem(strErrCode, strMessage, type));
}

//------------------------------------------------------------------------------
void CDiagnostic::Add(const CStringArray& strMessages, MsgType type /*Error*/)
{
	for (int i = 0; i <= strMessages.GetUpperBound(); i++)
		Add(strMessages.GetAt(i), type);
}

//------------------------------------------------------------------------------
CString CDiagnostic::Add
(
	const	CException*				pException,
	const	CString&				strMessage, /*= _T("") */
	CDiagnostic::MsgType	type, /*= CDiagnostic::Error*/
	const	CString					strErrCode, /*= strNoErrorCode*/
	bool							bTrace /*TRUE*/
)
{
	CString sDiagnostic;

	TCHAR szErrorMessage[512];
	if (pException && pException->GetErrorMessage(szErrorMessage, sizeof(szErrorMessage) / sizeof(*szErrorMessage), 0))
		sDiagnostic.Format(_T("The following unexpected error has occurred: '%s'"), szErrorMessage);
	else
		sDiagnostic = _T("An unexpected error has occurred");

	if (!strMessage.IsEmpty())
		sDiagnostic = strMessage + _T("\r\n") + sDiagnostic;
	Add(sDiagnostic, type, strErrCode);

	if (bTrace && !sDiagnostic.IsEmpty())
		TRACE(sDiagnostic);

	return sDiagnostic;
}

//-----------------------------------------------------------------------------
void CDiagnostic::Add(CDiagnostic* pDiagnostic)
{
	if (pDiagnostic->m_bHasFatalError)
		m_bHasFatalError = TRUE;

	m_pStartingLevel->Add(pDiagnostic->m_pStartingLevel);
}

//------------------------------------------------------------------------------
void CDiagnostic::Copy
(
	CDiagnostic*	pDiagnostic,
	BOOL			bNewSession,
	const	CString&		strOpeningBanner,
	const	CString&		strClosingBanner
)
{
	// to avoid loops || no messages
	if (this == pDiagnostic || pDiagnostic->GetMessagesCount() < 0)
		return;

	if (bNewSession)
		StartSession(strOpeningBanner);

	CObArray arItems;
	pDiagnostic->ToArray(arItems, pDiagnostic->m_pStartingLevel);

	for (int i = 0; i <= pDiagnostic->GetUpperBound(); i++)
	{
		const CDiagnosticItem* pItem = pDiagnostic->GetItemAt(i);
		if (pItem)
			Add(pItem->GetMessageText(), pItem->GetType(), pItem->GetErrCode());
	}

	if (bNewSession)
		EndSession(strClosingBanner);
}

//------------------------------------------------------------------------------
CString CDiagnostic::ToString()
{
	CStringArray arMessages;
	ToStringArray(arMessages);

	CString strRetVal;
	for (int i = 0; i <= arMessages.GetUpperBound(); i++)
	{
		if (!strRetVal.IsEmpty())
			strRetVal += szMessageLineFeed;

		strRetVal += arMessages.GetAt(i);
	}

	return strRetVal;
}

//------------------------------------------------------------------------------
const int CDiagnostic::GetMessagesCount()
{
	return m_pStartingLevel ? m_pStartingLevel->GetMessagesCount() : 0;
}

//------------------------------------------------------------------------------
const CDiagnosticItem* CDiagnostic::GetItemAt(const int& nIdx)
{
	return m_pStartingLevel ? m_pStartingLevel->GetItemAt(nIdx) : 0;
}

//------------------------------------------------------------------------------
const CString CDiagnostic::GetMessageAt(const int& nIdx)
{
	if (!m_pStartingLevel)
		return _T("");

	CDiagnosticItem* pItem = m_pStartingLevel->GetItemAt(nIdx);
	return pItem ? pItem->GetMessageText() : _T("");
}

//------------------------------------------------------------------------------
void CDiagnostic::ToArray(CObArray& arItems)
{
	arItems.RemoveAll();
	ToArray(arItems, m_pStartingLevel);
}

//------------------------------------------------------------------------------
void CDiagnostic::ToArray(CObArray& arItems, CDiagnosticLevel* pLevel)
{
	ASSERT(pLevel);
	for (int i = 0; i < pLevel->GetMessagesCount(); i++)
		arItems.Add(pLevel->GetItemAt(i));
}

//------------------------------------------------------------------------------
void CDiagnostic::ToStringArray(CStringArray& arMessages, BOOL bAddLFToMessage /*FALSE*/)
{
	arMessages.RemoveAll();

	CString sMessage;
	CDiagnosticItem* pItem;
	for (int i = 0; i < m_pStartingLevel->GetMessagesCount(); i++)
	{
		pItem = (CDiagnosticItem*)m_pStartingLevel->GetItemAt(i);
		if (pItem)
		{
			sMessage = pItem->GetMessageText();
			if (pItem->GetErrCode() != szNoErrorCode)
				sMessage = pItem->GetErrCode() + _T(" - ") + sMessage;
		}
		if (bAddLFToMessage) sMessage += _T("\r\n");
		arMessages.Add(sMessage);
	}
}

//------------------------------------------------------------------------------
void CDiagnostic::ToJson(CJsonSerializer& ser)
{
	m_pStartingLevel->PurgeEmptyItems();
	m_pStartingLevel->ToJson(ser);

}
//------------------------------------------------------------------------------
CDiagnostic* CDiagnostic::Clone()
{
	CDiagnostic* pNew = new CDiagnostic();

	if (pNew->m_pStartingLevel)
		delete pNew->m_pStartingLevel;

	pNew->m_pStartingLevel = m_pStartingLevel->Clone();

	// I dont't know at which nesting level I am, I restart
	pNew->m_pCurrLevel = pNew->m_pStartingLevel;

	return pNew;
}

//------------------------------------------------------------------------------
void CDiagnostic::EnableTraceInEventViewer(const BOOL bEnable /*TRUE*/)
{
	if (m_pCurrLevel)
		m_pCurrLevel->EnableTraceInEventViewer(bEnable);
}

//------------------------------------------------------------------------------
BOOL CDiagnostic::IsTraceInEventViewerEnabled() const
{
	return m_pCurrLevel ? m_pCurrLevel->IsTraceInEventViewerEnabled() : FALSE;
}
