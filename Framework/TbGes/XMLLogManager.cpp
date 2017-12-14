#include "stdafx.h" 

#include <TbNameSolver\PathFinder.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlibUI\browserdlg.h>
#include <TbGenlib\oslbaseinterface.h>

#include "XMLLogManager.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

static const TCHAR szOpenSquareBrace		[] = _T("[");
static const TCHAR szCloseSquareBrace		[] = _T("]");
static const TCHAR szAttributeSimbol		[] = _T("@");
static const TCHAR szEqualSimbol			[] = _T("=");
static const TCHAR szApex					[] = _T("'");
static const TCHAR szExplorer				[] = _T("IEXPLORE.EXE");
static const TCHAR szTmpLogFile			[] = _T("XMLTmpLog");
static const TCHAR szTarget				[] = _T("xml-stylesheet");
static const TCHAR szData					[] = _T("type='text/xsl' href='%s'");
static const TCHAR szXMLLogManager		[] = _T("XMLLogManager.xsl");

#define TAG_USER			_T("User")
#define TAG_NAME			_T("Name")
#define TAG_MESSAGES		_T("Messages")
#define TAG_MESSAGE			_T("Message")
#define TAG_MESSAGE_DETAIL	_T("Detail")
#define TAG_MSG_TYPE		_T("Type")
#define TAG_TIMESTAMP		_T("Timestamp")
#define TAG_LOGSPACES		_T("LogSpaces")
#define TAG_LOGSPACE		_T("LogSpace")
#define TAG_FILENAME		_T("File")
#define TAG_NEXT			_T("Next")
#define TAG_PREVIOUS		_T("Previous")

#define TAG_LOCALIZED_STRINGS			_T("LocalizedStrings")
#define TAG_LOCALIZED_CLOSE				_T("close") 
#define TAG_LOCALIZED_EXPAND_TEXT		_T("expandText")
#define TAG_LOCALIZED_COLLAPSE_TEXT		_T("collapseText")
#define TAG_LOCALIZED_MESSAGE_FILTER	_T("messageFilter")
#define TAG_LOCALIZED_NEXT_FILE			_T("nextFile")
#define TAG_LOCALIZED_PREVIOUS_FILE		_T("previousFile")
#define TAG_LOCALIZED_MESSAGE_LIST		_T("messageList")
#define TAG_LOCALIZED_USER				_T("user")
#define TAG_LOCALIZED_TYPE				_T("type")
#define TAG_LOCALIZED_TIMESTAMP			_T("timestamp")
#define TAG_LOCALIZED_MESSAGE			_T("message")
#define TAG_LOCALIZED_DETAILS			_T("details")

#define TAG_VALUE_INFO		_T("Info")
#define TAG_VALUE_WARNING	_T("Warning")
#define TAG_VALUE_ERROR		_T("Error")

#define DEFAULT_MAX_MESSAGES 1000


/////////////////////////////////////////////////////////////////////////////
//	CXMLLogSpace implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CXMLLogSpace::CXMLLogSpace(CString strUser)
:
m_bIsUpToDate(FALSE),
m_strUser(strUser),
m_pMessageBuffer(NULL),
m_pLastMessage(NULL),
m_pLogSession(NULL),
m_nMaxMessages(DEFAULT_MAX_MESSAGES),
m_nTotalMessages(0),
m_bShow(TRUE)
{
}

//----------------------------------------------------------------------------
CXMLDocumentObject* CXMLLogSpace::GetBuffer()
{
	if(!m_pMessageBuffer)
	{
		m_pMessageBuffer = new CXMLDocumentObject(TRUE, FALSE, FALSE);
		ASSERT_VALID(m_pMessageBuffer);
		if (
				m_pLogSession && 
				!m_pLogSession->GetXSLFile().IsEmpty()
			)
			m_pMessageBuffer->CreateInitialProcessingInstruction(szTarget, cwsprintf(szData, m_pLogSession->GetXSLFile()));

		CXMLNode* pNode = m_pMessageBuffer->CreateRoot (TAG_MESSAGES);
		if(pNode)
		{
			InitMessageRoot(pNode);
			m_LoggingLevels.Add ((void*)pNode);
			m_nTotalMessages = 0;
		}
		m_bIsUpToDate = FALSE;
	}
	
	return m_pMessageBuffer;
}

//----------------------------------------------------------------------------
void CXMLLogSpace::InitMessageRoot(CXMLNode* pRoot)
{
	pRoot->SetAttribute (TAG_USER, m_strUser);
	CXMLNode *pStringNode = pRoot->CreateNewChild(TAG_LOCALIZED_STRINGS);
	pStringNode->SetAttribute(TAG_LOCALIZED_CLOSE,			_TB("Close"));
	pStringNode->SetAttribute(TAG_LOCALIZED_EXPAND_TEXT,	_TB("Expand messages"));
	pStringNode->SetAttribute(TAG_LOCALIZED_COLLAPSE_TEXT,	_TB("Collapse messages"));
	pStringNode->SetAttribute(TAG_LOCALIZED_MESSAGE_FILTER,	_TB("Message filter"));
	pStringNode->SetAttribute(TAG_LOCALIZED_NEXT_FILE,		_TB("Messages continue in file:"));
	pStringNode->SetAttribute(TAG_LOCALIZED_PREVIOUS_FILE,	_TB("Messages continue from file:"));
	pStringNode->SetAttribute(TAG_LOCALIZED_MESSAGE_LIST,	_TB("Message list"));
	pStringNode->SetAttribute(TAG_LOCALIZED_USER,			_TB("User:"));
	pStringNode->SetAttribute(TAG_LOCALIZED_TYPE,			_TB("Type"));
	pStringNode->SetAttribute(TAG_LOCALIZED_TIMESTAMP,		_TB("Timestamp"));
	pStringNode->SetAttribute(TAG_LOCALIZED_MESSAGE,		_TB("Message"));
	pStringNode->SetAttribute(TAG_LOCALIZED_DETAILS,		_TB("Details..."));

	m_bIsUpToDate = FALSE;

}

//----------------------------------------------------------------------------
void CXMLLogSpace::CleanBuffer()
{
	if(m_pMessageBuffer)
		delete m_pMessageBuffer;
	m_pMessageBuffer = NULL;
}

//----------------------------------------------------------------------------
CXMLLogSpace::~CXMLLogSpace() 
{
	ASSERT(m_LoggingLevels.GetSize() == 1);		//mancata corrispondenza Raise/lower del livello di logging?
	if(!IsUpToDate())
		Flush();
	CleanBuffer();
}

//----------------------------------------------------------------------------
CXMLNode* CXMLLogSpace::GetLoggingNode() 
{
	if(m_LoggingLevels.GetSize() == 0)
		GetBuffer();
	
	CXMLNode *pNode = (CXMLNode*) m_LoggingLevels.GetAt (m_LoggingLevels.GetUpperBound ());
	ASSERT_VALID (pNode);

	if(!pNode)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return pNode;
}
//----------------------------------------------------------------------------
CString CXMLLogSpace::GetFullFileName()
{
	return m_pLogSession->GetLogPath() + m_strFileName;
}
//----------------------------------------------------------------------------
BOOL CXMLLogSpace::Add(const CString& strMessage, XMLMsgType nMsgType) 
{
	CXMLNode *pNode = GetLoggingNode();
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pLastMessage = pNode->CreateNewChild (TAG_MESSAGE);
	if(!m_pLastMessage)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bOk = m_pLastMessage->SetText (strMessage);
	bOk = bOk && m_pLastMessage->SetAttribute (TAG_MSG_TYPE, GetTypeDescription(nMsgType));
	bOk = bOk && m_pLastMessage->SetAttribute (TAG_TIMESTAMP, GetTimeStr());
	
	m_bIsUpToDate = FALSE;
	m_nTotalMessages ++;
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSpace::Add(UINT nMsgStringID, XMLMsgType nMsgType) 
{
	return Add(cwsprintf(nMsgStringID), nMsgType);
}

//----------------------------------------------------------------------------
BOOL CXMLLogSpace::AppendDetail(const CString& strMessage) 
{
	if(!m_pLastMessage)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CXMLNode* pMessageNode = m_pLastMessage->CreateNewChild (TAG_MESSAGE_DETAIL);
	if(!pMessageNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bOk = pMessageNode->SetText (strMessage);
	
	m_bIsUpToDate = FALSE;
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSpace::AppendDetail(UINT nMsgStringID) 
{
	return AppendDetail(cwsprintf(nMsgStringID));
}

//----------------------------------------------------------------------------
BOOL CXMLLogSpace::Flush()
{
	if (IsUpToDate()) return TRUE;
	
	CString strFilename = GetFullFileName();
	
	CString strPath = GetPath(strFilename);
	if(!ExistPath (strPath))
		RecursiveCreateFolders(strPath);
	GetBuffer()->SaveXMLFile(strFilename);

	CString strRemoteXSLFile = AfxGetPathFinder()->GetModuleXmlPath(snsTbGes, CPathFinder::STANDARD) 
							+ SLASH_CHAR + 
							 m_pLogSession->GetXSLFile();

	CString strLocalXSLFile = strPath + SLASH_CHAR + m_pLogSession->GetXSLFile();
	if (!ExistFile(strLocalXSLFile))
		CopyFile(strRemoteXSLFile, strLocalXSLFile);
		
	m_bIsUpToDate = TRUE;
		
	return TRUE;
}


//----------------------------------------------------------------------------
CString CXMLLogSpace::GetTypeDescription(XMLMsgType nMsgType) 
{
	switch(nMsgType)
	{
	case XML_INFO:
		return TAG_VALUE_INFO;
	case XML_WARNING:
		return TAG_VALUE_WARNING;
	case XML_ERROR:
		return TAG_VALUE_ERROR;
	}
	
	ASSERT(FALSE);
	return _T("");
}

//----------------------------------------------------------------------------
CString CXMLLogSpace::GetTimeStr()
{
	CString time;
	SYSTEMTIME	systime;
	::GetLocalTime (&systime);
	time.Format(_T("%u-%.2u-%.2u-%.2u-%.2u-%.2u"),
				systime.wYear, systime.wMonth, systime.wDay,
				systime.wHour, systime.wMinute, systime.wSecond
			);
	return time;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSpace::MsgTypeFound	(XMLMsgType nMsgType)
{
	if(!m_pMessageBuffer)
		return FALSE;
	CString strFilter = CString(URL_SLASH_CHAR) + URL_SLASH_CHAR + TAG_MESSAGE +
																	szOpenSquareBrace + 
																	szAttributeSimbol + 
																	TAG_MSG_TYPE + 
																	szEqualSimbol + 
																	szApex + 
																	GetTypeDescription(nMsgType) + 
																	szApex + 
																	szCloseSquareBrace; 
				
	CXMLNode *pNode = m_pMessageBuffer->SelectSingleNode (strFilter);

	if(pNode)
		delete pNode;
	
	return pNode!=NULL;
}

//----------------------------------------------------------------------------
void CXMLLogSpace::RaiseLoggingLevel ()
{
	if(!m_pLastMessage)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLNode* pNode = m_pLastMessage->CreateNewChild (TAG_MESSAGES);
	if(pNode)
	{
		m_LoggingLevels.Add ((void*)pNode);
		m_bIsUpToDate = FALSE;
	}
}

//----------------------------------------------------------------------------
void CXMLLogSpace::LowerLoggingLevel()
{
	if(m_LoggingLevels.GetSize() == 0)
	{
		ASSERT(FALSE);
		return;
	}

	m_LoggingLevels.RemoveAt (m_LoggingLevels.GetUpperBound());
	m_bIsUpToDate = FALSE;
}

//----------------------------------------------------------------------------
void CXMLLogSpace::SetNextFile (const CString&	strFile)
{
	if(m_LoggingLevels.GetSize() == 0)
		GetBuffer();

	CXMLNode *pNode = (CXMLNode*) m_LoggingLevels.GetAt(0);
	ASSERT_VALID (pNode);
	if(pNode)
	{
		pNode->SetAttribute (TAG_NEXT, strFile);
		m_bIsUpToDate = FALSE;
	}
}

//----------------------------------------------------------------------------
void CXMLLogSpace::SetPreviousFile(const CString&	strFile)
{
	if(m_LoggingLevels.GetSize() == 0)
		GetBuffer();

	CXMLNode *pNode = (CXMLNode*) m_LoggingLevels.GetAt(0);
	ASSERT_VALID (pNode);
	if(pNode)
	{
		pNode->SetAttribute (TAG_PREVIOUS, strFile);
		m_bIsUpToDate = FALSE;
	}
}


/////////////////////////////////////////////////////////////////////////////
//	CXMLLogSession implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CXMLLogSession::CXMLLogSession()
:
	m_pCurrentLogSpace		(NULL),
	m_bUseDialog			(FALSE)
{
	m_strXSLFile = szXMLLogManager;
}

//----------------------------------------------------------------------------
void CXMLLogSession::Init(const CString &strName, const CString& strLogPath)
{
	m_LogSpaces.RemoveAll();
	m_strName = strName;

	SetLogPath(strLogPath);

	AddLogSpace();
}

//----------------------------------------------------------------------------
void CXMLLogSession::SetLogPath(const CString& strLogPath)
{
	m_strLogPath = strLogPath;
	if (m_strLogPath.Right(1) != SLASH_CHAR)
		m_strLogPath += SLASH_CHAR;
}
	

//----------------------------------------------------------------------------
CXMLLogSession::~CXMLLogSession()
{

}

//----------------------------------------------------------------------------
void CXMLLogSession::AddLogSpace(CXMLLogSpace* pLogSpace)
{
	if(!pLogSpace)
	{
		ASSERT (FALSE);
		return;
	}
	pLogSpace->m_pLogSession = this;
	m_LogSpaces.Add (pLogSpace);
}

//----------------------------------------------------------------------------
void CXMLLogSession::AddLogSpace(BOOL bIsToShow /*= TRUE*/)
{ 
 	m_pCurrentLogSpace = new CXMLLogSpace(AfxGetLoginInfos()->m_strUserName);
	
	m_pCurrentLogSpace->SetName(m_strName);
	
	m_pCurrentLogSpace->SetFileName(GetNextLogFile());
	
	m_pCurrentLogSpace->m_bShow = bIsToShow;

	AddLogSpace(m_pCurrentLogSpace);

} 



//----------------------------------------------------------------------------
BOOL CXMLLogSession::FlushCurrentLogSpace()
{
	if (m_pCurrentLogSpace)
		return m_pCurrentLogSpace->Flush ();

	return FALSE;
}

//----------------------------------------------------------------------------
CString CXMLLogSession::GetNextLogFile()
{
	CString s;
	s.Format(_T("Log%.5u.xml"), m_LogSpaces.GetCount() + 1);
	return s;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSession::AddMessage	(const CString&	strMessage, CXMLLogSpace::XMLMsgType nMsgType, BOOL bBreakIfNeeded /*= TRUE*/)
{
	if (!m_pCurrentLogSpace)
		return FALSE;

	if (bBreakIfNeeded && m_pCurrentLogSpace->BreakNeeded ())
	{
		// ho esaurito lo spazio: creo un nuovo log space con lo stesso nome
		// e file name modificato con l'aggiunta di un numero in appendice
		CString strName, strOldFileName, strFileName;
		
		strName = m_pCurrentLogSpace->GetName ();
		strOldFileName = m_pCurrentLogSpace->GetFullFileName();
		strFileName = GetNextLogFile();
		
		ASSERT(strFileName.CompareNoCase(strOldFileName) != 0);

		//per evitare la ricorsione
		m_pCurrentLogSpace->m_nTotalMessages = 0;

		int nLevels = m_pCurrentLogSpace->m_LoggingLevels.GetSize();
		while(m_pCurrentLogSpace->m_LoggingLevels.GetSize() != 1)
			m_pCurrentLogSpace->LowerLoggingLevel();

		m_pCurrentLogSpace->SetNextFile(GetNameWithExtension(strFileName));
		
		m_pCurrentLogSpace->Flush();

		AddLogSpace (FALSE);
		m_pCurrentLogSpace->SetPreviousFile(GetNameWithExtension(strOldFileName));

		nLevels --;
		for (int i = 0; i < nLevels; i++)
		{
			AddMessage(cwsprintf(_TB("Continues from log file: {0-%s}."), GetNameWithExtension(strOldFileName)), CXMLLogSpace::XML_INFO, FALSE);
			m_pCurrentLogSpace->RaiseLoggingLevel();
		}	
	}

	return m_pCurrentLogSpace->Add (strMessage, nMsgType);
}

//----------------------------------------------------------------------------
BOOL CXMLLogSession::AddMessage	(UINT nMsgStringID, CXMLLogSpace::XMLMsgType nMsgType, BOOL bBreakIfNeeded /*= TRUE*/)
{
	return AddMessage(cwsprintf(nMsgStringID), nMsgType, bBreakIfNeeded);
}

//----------------------------------------------------------------------------
BOOL CXMLLogSession::AppendDetail (const CString& strMessage)
{
	if (m_pCurrentLogSpace)
		return m_pCurrentLogSpace->AppendDetail(strMessage);

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSession::AppendDetail (UINT nMsgStringID)
{
	if (m_pCurrentLogSpace)
		return m_pCurrentLogSpace->AppendDetail(nMsgStringID);

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CXMLLogSession::ShowLogSpaces(CXMLLogSpace::XMLMsgType *pRequiredType /*=NULL*/)
{
	if(m_LogSpaces.GetSize () == 0)
		return FALSE;

	CXMLLogSpace *pLogSpace = NULL;

	for (int i=m_LogSpaces.GetUpperBound(); i>=0; i--)
	{
		pLogSpace = (CXMLLogSpace*)m_LogSpaces.GetAt (i);
		ASSERT(pLogSpace);

		if(!pLogSpace ||
			!pLogSpace->m_bShow ||
			!pLogSpace->GetBuffer() || 
			(pRequiredType && !pLogSpace->MsgTypeFound(*pRequiredType)))
			continue;

		// salvo su file system
		pLogSpace->Flush ();
		
		if(m_bUseDialog)
		{
			CBrowserDlg dlg(pLogSpace->GetFullFileName ());
			//dlg.SetURL (pLogSpace->GetFileName ());
			dlg.DoModal ();
		}
		else
		{
			HINSTANCE hInst = ::TBShellExecute(szExplorer, pLogSpace->GetFullFileName());
			if (hInst <= (HINSTANCE)32)
				return FALSE;
		}
	}

	return TRUE;
}



//----------------------------------------------------------------------------
CString CXMLLogSession::GetTmpLogFile()
{
	TCHAR szTmpPath[_MAX_PATH];
	GetTempPath (_MAX_PATH, szTmpPath);
	return CString (szTmpPath) + szTmpLogFile + szXmlExt;
}

//----------------------------------------------------------------------------
void CXMLLogSession::GetLogSpaceFileList(CStringArray* pArray)
{
	if(!pArray) return;

	pArray->RemoveAll ();

	for (int i=0; i<m_LogSpaces.GetSize (); i++)
	{
		CXMLLogSpace* pLogSpace = (CXMLLogSpace*)m_LogSpaces.GetAt(i);
		ASSERT(pLogSpace);
		pArray->Add(pLogSpace->GetFullFileName ());
	}
}

//----------------------------------------------------------------------------
UINT CXMLLogSession::GetLogSpacesNumber	(BOOL bOnlyIfToShow /*= FALSE*/)
{
	if(bOnlyIfToShow)
	{
		UINT number = 0;
		for (int i=0; i<m_LogSpaces.GetSize(); i++)
		{
			CXMLLogSpace* pLogSpace = (CXMLLogSpace*)m_LogSpaces.GetAt(i);
			ASSERT(pLogSpace);
			if(pLogSpace && pLogSpace->m_bShow) 
				number++;
		}
		return number;
	}
	
	return m_LogSpaces.GetSize();
}