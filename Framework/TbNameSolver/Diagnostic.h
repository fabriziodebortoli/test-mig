#pragma once

//includere alla fine degli include del .H

#include "beginh.dex"

class CXMLDocumentObject;
class CDiagnosticItem;
class CDiagnosticLevel;
class CDiagnostic;

#define szNoErrorCode _T("TB-0000")

//==================================================================================
class TB_EXPORT CThreadCallFailedException : public CException
{
public:
	DWORD		m_nCallingThreadId;
	DWORD		m_nThreadId;

	CThreadCallFailedException(DWORD nCallingThreadId, DWORD nThreadId)
		: m_nCallingThreadId(nCallingThreadId), 
		m_nThreadId(nThreadId)
	{
	}
};

//==================================================================================
//lanciata in caso di errore generico di applicazione
class TB_EXPORT CApplicationErrorException : public CException
{
public:
	CString m_sErrorMessage;

	CApplicationErrorException(const CString& sErrorMessage)
		: m_sErrorMessage(sErrorMessage)
	{
	}

	virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError,
		_Out_opt_ PUINT /*pnHelpContext*/ = NULL) const 
	{
		_tcscpy_s(lpszError, nMaxError, m_sErrorMessage);

		return !m_sErrorMessage.IsEmpty();
	}
	virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError,
		_Out_opt_ PUINT /*pnHelpContext*/ = NULL)
	{
		_tcscpy_s(lpszError, nMaxError, m_sErrorMessage);

		return !m_sErrorMessage.IsEmpty();
	}
};

//==============================================================================
//lanciata in caso di parametri errati
class TB_EXPORT CParameterException : public CApplicationErrorException
{
public:
	CParameterException(const CString& sErrorMessage)
		: CApplicationErrorException(sErrorMessage)
	{
	}
};

// interface class for diagnostic viewers 
//==============================================================================
class TB_EXPORT IDiagnosticViewer
{
public:
	virtual BOOL Show (CDiagnostic* /*pDiagnostic*/, BOOL /*bClearMessages*/ = TRUE) { return TRUE;	}
	virtual CWnd* ShowNoModal(CDiagnostic* /*pDiagnostic*/, int /*iX = 0*/, int /* iY = 0*/) { return NULL; }

	virtual ~IDiagnosticViewer() {}
};



// class for diagnostic management
//==============================================================================
class TB_EXPORT CDiagnostic : public CObject
{
	DECLARE_DYNAMIC(CDiagnostic)

	friend class CDiagnosticLevel;
	friend class CMessages;
	friend class CThreadContext;
	friend class CMessagesTree;
	friend class CBaseContext;
	friend class CMessageReport;

public:
	//WARNING: these values correspond to those defined in 
	//\Microarea.TaskBuilderNet.Interfaces  Enums.cs (for commonly defined items)
	enum MsgType { FatalError = 16, Error = 2, Warning = 1, Info = 8, Banner = 32 };	

private:
	CDiagnosticLevel*	m_pStartingLevel;
	CDiagnosticLevel*	m_pCurrLevel;
	IDiagnosticViewer*	m_pViewer;
	BOOL				m_bHasFatalError;
	BOOL				m_bShowingMessages;
	BOOL				m_bHasSubLevels;

private:
	CDiagnostic ();

public:
	~CDiagnostic();

public:
	BOOL IsShowingMessages() { return m_bShowingMessages; }
	void ClearMessages	(BOOL bAllLevels = FALSE);

	int				GetUpperBound	() { return GetMessagesCount() - 1; }
	const CString	GetMessageLine	(const int& nIdx) { return GetMessageAt(nIdx); }

	BOOL ErrorFound		(const BOOL bIncludeChildLevels = FALSE) const;
	BOOL WarningFound	(const BOOL bIncludeChildLevels = FALSE) const;
	BOOL InfoFound		(const BOOL bIncludeChildLevels = FALSE) const;
	BOOL MessageFound	(const BOOL bIncludeChildLevels = FALSE) const;
	BOOL MessageFound	(const CString& sMessage, const BOOL bIncludeChildLevels = FALSE) const;
	BOOL ErrorCodeFound	(const CString& strErrCode, const BOOL bIncludeChildLevels = FALSE, BOOL bRemoveMessage = FALSE) const;
	BOOL HasFatalError	() const { return m_bHasFatalError; }
	BOOL IsLastMessage	(const CString& sMessage, const BOOL bIncludeChildLevels = FALSE) const;

	void StartSession	(const CString& strOpeningBanner = _T(""), BOOL bForceBanner = FALSE); 
	void EndSession		(const CString& strClosingBanner = _T(""));

	int		Add (CDiagnosticItem* pItem);
	void	Add	(CDiagnostic* pDiagnostic);
	void	Add (const CString& strMessage, CDiagnostic::MsgType type = CDiagnostic::Error, const CString strErrCode = szNoErrorCode, BOOL bOnlyIfNotExist = FALSE);
	void	Add	(const CStringArray& strMessages, MsgType type = Error);
	CString Add	(const CException* pException, const CString& strMessage = _T(""), CDiagnostic::MsgType type = CDiagnostic::Error, const CString strErrCode = szNoErrorCode, bool bTrace = true);

	// viewer management
	void	AttachViewer(IDiagnosticViewer *pViewer){ delete m_pViewer; m_pViewer = pViewer; }

	BOOL	IsTopLevel		()	const	{ return m_pStartingLevel == m_pCurrLevel;  }
	BOOL	IsUnattendedMode()	const	{ return !IsTopLevel();  }

	BOOL	HasSubLevels()	const	{ return m_bHasSubLevels; }

	void	Copy			(CDiagnostic* pDiagnostic, BOOL bNewSession = TRUE, const CString& strOpeningBanner = _T(""), const CString& strClosingBanner = _T(""));
	void	ToArray			(CObArray& arItems);
	void	ToStringArray	(CStringArray& arValues, BOOL bAddLFToMessage = FALSE);
	CString ToString		();
	virtual void	ToJson	(CJsonSerializer& ser);
	void	EnableTraceInEventViewer	(const BOOL bEnable = TRUE);
	BOOL	IsTraceInEventViewerEnabled	() const;

private:
	void		Initialize	();
	void		ToArray		(CObArray& arItems, CDiagnosticLevel* pLevel);

public:
	CDiagnosticLevel*		GetStartingLevel() const { return m_pStartingLevel; }
	const int				GetMessagesCount();
	const CDiagnosticItem*	GetItemAt		(const int& nIdx);
	const CString			GetMessageAt	(const int& nIdx);

	void					SetStartingBanner(const CString& strOpeningBanner);

public:
	// operators
	CDiagnostic*	Clone	();

public:
	virtual BOOL Show (BOOL bClearMessages = TRUE);
	virtual CWnd* ShowNoModal(int iX = 0, int iY = 0);
protected:
	virtual void UpdateDataView() {}//non  fa nulla, se sono una CMessages, invece, forza l'updatedataview del documento prima della visualizzazione
};

// single diagnostic item
//==============================================================================
class TB_EXPORT CDiagnosticItem : public CObject
{
private:
	CString					m_sMessage;
	CDiagnostic::MsgType	m_eType;
	CString					m_strErrCode;

public:
	CDiagnosticItem (const CString& strErrCode, const CString& sMessage, const CDiagnostic::MsgType eType);

public:
	const CString&				GetMessageText	() const;
	const CDiagnostic::MsgType	GetType			() const;
	const CString&				GetErrCode		() const;

	void				Assign	(CDiagnosticItem* pItem);

public:
	virtual BOOL HasMessages	(CDiagnostic::MsgType aType, const BOOL bAnyType = FALSE, const BOOL bIncludeChildLevels = FALSE) const;
	virtual BOOL HasMessage		(const CString& sMessage, const BOOL bIncludeChildLevels = FALSE) const;
	virtual BOOL HasErrorCode	(const CString& strErrCode, const BOOL = FALSE, BOOL = FALSE);

	virtual const BOOL			IsNestedLevel	() const { return FALSE; }
	virtual CDiagnosticItem*	GetItemAt		(const int& nIdx, int& nCurrIndex);
	virtual const int			GetMessagesCount();
	virtual	CDiagnosticItem*	Clone			();
	virtual void				ToJson			(CJsonSerializer& ser);
	DECLARE_DYNAMIC(CDiagnosticItem);
};

// it inherit from CDianosticItem only in order to be added in the array of messages
//==============================================================================
class CDiagnosticLevel : public CDiagnosticItem
{
	friend class CDiagnostic;
	friend class CMessagesTree;
	friend class CMessageReport;

private:
	// diagnostic objects
	CString				m_strOpeningBanner;
	CObArray			m_arMessages;
	CDiagnosticLevel*	m_pParent;
	BOOL				m_bTraceInEventViewer;
	BOOL				m_bForceBanner;

private:
	CDiagnosticLevel	(CDiagnosticLevel* pParent, const CString& strOpeningBanner, const BOOL& bTraceInEventViewer);
	~CDiagnosticLevel	();

public:
	virtual BOOL HasMessages	(CDiagnostic::MsgType aType, const BOOL bAnyType = FALSE, const BOOL bIncludeChildLevels = FALSE) const;
	virtual BOOL HasMessage		(const CString& sMessage, const BOOL bIncludeChildLevels = FALSE) const;
	virtual BOOL HasErrorCode	(const CString& strErrCode, const BOOL bIncludeChildLevels = FALSE, BOOL bRemoveMessage = FALSE);
	
	virtual const BOOL			IsNestedLevel	() const { return TRUE; }
	virtual const int			GetMessagesCount();
	virtual CDiagnosticItem*	GetItemAt		(const int& nIdx);
	virtual CDiagnosticItem*	GetItemAt		(const int& nIdx, int& nCurrIndex);

private:
	void Clear		();

	BOOL			HasMessages		() const { return HasMessages(CDiagnostic::Info, TRUE); }
	BOOL			IsLastMessage	(const CString& sMessage, const BOOL bIncludeChildLevels = FALSE) const;
	BOOL			HasOpeningBanner() const { return !m_strOpeningBanner.IsEmpty(); }
	const CString&	GetOpeningBanner() const { return m_strOpeningBanner; }
	void			SetOpeningBanner(const CString& strOpeningBanner) { m_strOpeningBanner = strOpeningBanner; };

	CDiagnosticLevel*	StartSession(const CString& strOpeningBanner = _T(""), BOOL bForceBanner = FALSE); 
	void				EndSession	(const CString& strClosingBanner = _T(""));

	void	EnableTraceInEventViewer	(const BOOL bEnable = TRUE) { m_bTraceInEventViewer = bEnable; }
	BOOL	IsTraceInEventViewerEnabled	() const { return m_bTraceInEventViewer; }
	int		ToEventViewerMessageType	(CDiagnostic::MsgType aType);

	int	 Add (CDiagnosticItem*  pItem);

	// operators
	virtual CDiagnosticLevel*	Clone	();

	void ToJson(CJsonSerializer& ser);
	DECLARE_DYNAMIC(CDiagnosticLevel);
};

// General Functions
//==================================================================================
TB_EXPORT CDiagnostic*	AFXAPI AfxGetDiagnostic		();
TB_EXPORT CDiagnostic*	CloneDiagnostic	(BOOL bClearMessages = FALSE);

#define MA_CLIENT_LOGNAME _T("MA Server") 
//-----------------------------------------------------------------------------
// Event Viewer operations
// returns value 0=Not Enabled 1=Success 2=Error registering log section 3=Error writing log message 4=Empty Message
TB_EXPORT int	WriteEventViewerMessage	(
											const CString& strMsg, 
											WORD wMsgType = EVENTLOG_INFORMATION_TYPE, 
											const CString sApplicationName = MA_CLIENT_LOGNAME, 
											DWORD dwEventID = 0,
											WORD wCategoryID = 0
										);

#define USES_DIAGNOSTIC() CDiagnosticLevelMng __diagnosticMng;
#define SHOW_DIAGNOSTIC() __diagnosticMng.Show();
#define USES_UNATTENDED_DIAGNOSTIC() CDiagnosticLevelMng __diagnosticMng(TRUE);

//-----------------------------------------------------------------------------
class TB_EXPORT CDiagnosticLevelMng
{
	CDiagnostic* pDiagnostic;
	BOOL		 m_bUnattended;
	BOOL		m_bShown;
public:
	CDiagnosticLevelMng(BOOL bUnattended = FALSE)
		:
		m_bUnattended	(bUnattended),
		m_bShown		(FALSE)
	{
		pDiagnostic = AfxGetDiagnostic();
		pDiagnostic->StartSession();
	}
	void Show()
	{
		if (!m_bShown)
		{
			pDiagnostic->EndSession();
			if (!m_bUnattended && pDiagnostic->MessageFound())
				pDiagnostic->Show(TRUE);
			m_bShown = TRUE;
		}
	}
	~CDiagnosticLevelMng()
	{ 
		Show();
	}
};

#include "endh.dex"
