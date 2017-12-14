
#pragma once

#include <TbGeneric\Array.h>

#include <TbXmlCore\XMLDocObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLLogSession;

//----------------------------------------------------------------
//class CXMLLogSpace 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLLogSpace: public CObject
{
friend class CXMLLogSession;

	CXMLDocumentObject	*m_pMessageBuffer;
	CString				m_strFileName;
	CString				m_strUser;
	CString				m_strName;
	BOOL				m_bIsUpToDate;
	CPtrArray			m_LoggingLevels;
	CXMLNode			*m_pLastMessage;
	CXMLLogSession		*m_pLogSession;
	int					m_nMaxMessages;
	int					m_nTotalMessages;
	BOOL				m_bShow;

public:
	enum XMLMsgType	{ XML_INFO, XML_WARNING, XML_ERROR};


public:

			CXMLLogSpace		(CString strUser);
			~CXMLLogSpace		();

	CXMLDocumentObject* GetBuffer();
	void	CleanBuffer			();
	BOOL	Add					(const CString&	strMessage, XMLMsgType nMsgType) ;
	BOOL	Add					(UINT	nMsgStringID, XMLMsgType nMsgType) ;
	BOOL	AppendDetail		(const CString& strMessage);
	BOOL	AppendDetail		(UINT nMsgStringID);
	void	SetFileName			(const CString& strFileName) {m_strFileName = strFileName;}
	CString GetFullFileName		();
	void	SetName				(const CString& strName) {m_strName = strName;}
	CString GetName				() {return m_strName;}
	BOOL	Flush				();
	BOOL	IsUpToDate			() {return m_bIsUpToDate;}
	BOOL	MsgTypeFound		(XMLMsgType nMsgType);
	void	RaiseLoggingLevel	();
	void	LowerLoggingLevel	();
	void	SetMaxMessages		(int nMaxMessages) {ASSERT(nMaxMessages>1); m_nMaxMessages = nMaxMessages;}
	int		GetMaxMessages		() {return m_nMaxMessages;}
	
	BOOL	BreakNeeded			() {return m_nTotalMessages > m_nMaxMessages;}
	void	SetNextFile			(const CString&	strFile);
	void	SetPreviousFile		(const CString&	strFile);

private:
	static CString	GetTimeStr	();
	CString	GetTypeDescription	(XMLMsgType nMsgType);
	CXMLNode* GetLoggingNode	();
	void InitMessageRoot		(CXMLNode* pRoot);
};

//----------------------------------------------------------------
//class CXMLLogSession 
//----------------------------------------------------------------
//
class TB_EXPORT CXMLLogSession: public CObject
{
	Array				m_LogSpaces;
	CXMLLogSpace*		m_pCurrentLogSpace;
	CString				m_strXSLFile;
	BOOL				m_bUseDialog;
	CString				m_strName;
	CString				m_strLogPath;

public:

	CXMLLogSession		();
	~CXMLLogSession		();
	
	void	Init(const CString &strName, const CString& strLogPath);
	
	void	SetLogPath(const CString& strLogPath);
	const CString&	GetLogPath() { return m_strLogPath; }
	
	void	AddLogSpace(CXMLLogSpace* pLogSpace);
	void	AddLogSpace(BOOL bIsToShow = TRUE);
	
	BOOL	FlushCurrentLogSpace();
	BOOL	ShowLogSpaces(CXMLLogSpace::XMLMsgType *pRequiredType = NULL);
	BOOL	AddMessage			(const CString&	strMessage, CXMLLogSpace::XMLMsgType nMsgType, BOOL bBreakIfNeeded = TRUE) ;
	BOOL	AddMessage			(UINT	nMsgStringID, CXMLLogSpace::XMLMsgType nMsgType, BOOL bBreakIfNeeded = TRUE) ;
	BOOL	AppendDetail		(const CString& strMessage);
	BOOL	AppendDetail		(UINT nMsgStringID);
	CString GetTmpLogFile		();
	void	GetLogSpaceFileList	(CStringArray* pArray);
	CString	GetXSLFile			(){return m_strXSLFile;}
	void	SetXSLFile			(const CString& strXSLFile){m_strXSLFile = strXSLFile;}
	void	RaiseLoggingLevel	(){if(m_pCurrentLogSpace) m_pCurrentLogSpace->RaiseLoggingLevel();}
	void	LowerLoggingLevel	(){if(m_pCurrentLogSpace) m_pCurrentLogSpace->LowerLoggingLevel();}
	CString GetCurrentLogFile	() const { return m_pCurrentLogSpace ? m_pCurrentLogSpace->GetFullFileName() : _T("");}
	UINT	GetLogSpacesNumber	(BOOL bOnlyIfToShow = FALSE);
	void	SetUseDialog		(BOOL bSet=TRUE) {m_bUseDialog = bSet;}
	BOOL	GetUseDialog		() {return m_bUseDialog;}
	CString GetNextLogFile		();
};

/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"
