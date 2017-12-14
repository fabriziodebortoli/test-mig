#pragma once

#include <TbNameSolver\Diagnostic.h>
#include <TbGeneric\DataObj.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\TBToolbar.h>
#include <TbGenlib\Parsbtn.h>
#include <TbGenlib\Parslbx.h>
#include <TbGenlib\BaseTileDialog.h>
#include <TbGenlib\ParsObjManaged.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLDocumentObject;

#define ID_DIAGNOSTIC_TIMER		GET_ID(ID_DIAGNOSTIC_TIMER)		// Id of timer
#define DELAY_DIAGNOSTIC_TIMER	500								// delay in ms

// interface to menu manager
//---------------------------------------------------------------------------
class TB_EXPORT CDiagnosticManager : public CObject
{
	DECLARE_DYNAMIC (CDiagnosticManager);

public:
	static BOOL LogToXml		(CDiagnostic* pDiagnostic, const CString& sFileName);
	static BOOL LogToFile		(CDiagnostic* pDiagnostic, const CString& sFileName);
	static BOOL LoadFromFile	(CDiagnostic* pDiagnostic, const CString& sFileName);
	static void ToArray			(CDiagnostic* pDiagnostic, DataObjArray& arValues);

};

// class that manages a single extended info composition
//=============================================================================
class TB_EXPORT CDiagnosticManagerWriterExtInfo : public CObject
{
public:
	CDiagnosticManagerWriterExtInfo (const CString& sName, const CString& sValue);

public:
	CString	m_sName;
	CString	m_sValue;
};

// class that writes the C# diagnostic manager synatx
//=============================================================================
class TB_EXPORT CDiagnosticManagerWriter : public CObject
{
	DECLARE_DYNAMIC (CDiagnosticManagerWriter)

public:
	enum MessageType { Error, Warning, Information };

private:
	CXMLDocumentObject	m_LogFile;
	CString				m_sFileName;
	CString				m_sXSLFileName;
	BOOL				m_bValid;

public:
	CDiagnosticManagerWriter ();

public:
	BOOL Open	(const CString& sFileName, const CString& sXSLFileName = _T(""), BOOL bAppend = TRUE);
	BOOL Save	();
	BOOL Save	(const CString& sFileName);

	void AddError		(const CString& sMessage, Array* pExtendedInfos = NULL);
	void AddWarning		(const CString& sMessage, Array* pExtendedInfos = NULL);
	void AddInformation (const CString& sMessage, Array* pExtendedInfos = NULL);
	void AddMessage		(const MessageType& eType,  const CString& sMessage, Array* pExtendedInfos = NULL);

	void			SetLogFileName (const CString& sLogFileName);
	const CString&	GetLogFileName () const;

private:
	CString TypeToXml (const MessageType& eType);

};

//---------------------------------------------------------------------------
class TB_EXPORT CMsgBoxViewer : public IDiagnosticViewer
{
public:
	CMsgBoxViewer ();

public:
	virtual BOOL Show (CDiagnostic* pDiagnostic, BOOL bClearMessages = TRUE);
};

enum ShowFilterType
{
	SHOW_INFO = 0x0001,
	SHOW_WARNING = 0x0002,
	SHOW_ERROR = 0x0004
};

//==============================================================================
//          Class CInternalMessagesViewer implementation
//==============================================================================
class CMessageReport : public CBCGPReportCtrl
{
	DECLARE_DYNAMIC(CMessageReport)

private:
	CDiagnostic* m_pDiagnostic;
	CImageList	 m_Images;

	int m_nInfoCount;
	int m_nWarningCount;
	int m_nErrorCount;

	CUIntArray m_arNumMsgForLevel;

public:
	CMessageReport();
	~CMessageReport();

private:
	void		LoadImages();
	int			AddLevelMessages(CDiagnosticLevel* pLevel, WORD filterStatus, int& level, int& nMaxLevel);
	void		FistLoading();
	void		CopyTextInClipboard(const CString& strText);
	CString		GetAllMessages();

public:
	void SetDiagnostic(CDiagnostic* pDiagnostic);
	void Load(WORD filterStatus);
	int GetInfoCount()		const	{ return m_nInfoCount; }
	int GetWarningCount()	const	{ return m_nWarningCount; }
	int GetErrorCount()		const	{ return m_nErrorCount; }

protected:
	virtual CString GetGroupName(int nGroupCol, CBCGPGridItem* pItem);

public:
	virtual void OnInitControl();
	//{{AFX_MSG(CMessageReport)
	afx_msg void OnContextMenu(CWnd*, CPoint point);
	afx_msg void CopyMessage();
	afx_msg void CopyGroupMessages();
	afx_msg void CopyAllMessages();
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//==============================================================================
//          Class CInternalMessagesViewer implementation
//==============================================================================
class CDefaultMessagesViewer : public CParsedDialogWithTiles
{
	friend class CMessagesViewer;
	DECLARE_DYNAMIC(CDefaultMessagesViewer)

private:
	CImageList			m_ImageList;
	CStatic				m_wndMsgReportLocation;
	WORD				m_wFilterStatus;
	CDiagnostic*		m_pDiagnostic;
	CMessageReport		m_aWndMsgReport;

public:
	CDefaultMessagesViewer();
	~CDefaultMessagesViewer();

private:
	void InitToolbar();
	void SetFilterStatus(WORD aStatusFlag);
	BOOL HasFilterStatus(WORD aStatusFlag);

protected:
	virtual BOOL OnInitDialog();
	virtual void OnCustomizeToolbar();
	virtual void ResizeOtherComponents(CRect aRect);

public:
	BOOL Show(BOOL bReset) { return Show(m_pDiagnostic, bReset); }
	virtual BOOL Show(CDiagnostic* pDiagnostic, BOOL bClearMessages = TRUE);

protected:
	//{{AFX_MSG(CDefaultMessagesViewer)
	afx_msg void OnCorrect();
	afx_msg void OnInfoShow();
	afx_msg void OnWarningShow();
	afx_msg void OnErrorShow();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class CDefaultMessagesLogDialog : public CParsedDialogWithTiles
{
	friend class CMessagesViewer;
	DECLARE_DYNAMIC(CDefaultMessagesLogDialog)

public:
	CDefaultMessagesLogDialog();
	~CDefaultMessagesLogDialog();

	virtual BOOL Show(CDiagnostic* pDiagnostic, int iX = 0, int iY = 0);
	virtual void RepositionWindows(int iX, int iY);

private:
	BOOL			m_bShow;
	CWnd*			m_pActiveWindows;
	CMessageReport	m_aWndMsgReport;
	CStatic			m_wndMsgReportLocation;
	CDiagnostic*	m_pDiagnostic;

protected:
	virtual BOOL OnInitDialog();

protected:
	//{{AFX_MSG(CDefaultMessagesViewer)
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT CMessagesViewer : public IDiagnosticViewer
{
public:
	CMessagesViewer ();
	~CMessagesViewer();

	virtual BOOL Show (CDiagnostic* pDiagnostic, BOOL bClearMessages = TRUE);
	virtual CWnd* ShowNoModal(CDiagnostic* pDiagnostic, int iX = 0, int iY = 0);

private:
	CDefaultMessagesLogDialog* m_pMessagesLog;
};

//==============================================================================
class TB_EXPORT CLogFileViewer : public IDiagnosticViewer
{
private:
	CString m_sFileName;

public:	
	CLogFileViewer (const CString& sFileName);
	~CLogFileViewer (){}

	virtual BOOL Show (CDiagnostic* pDiagnostic, BOOL bClearMessages = TRUE);

};

//==============================================================================
class TB_EXPORT CMessagesWebViewer : public IDiagnosticViewer
{
public:
	
	virtual BOOL Show(CDiagnostic* pDiagnostic, BOOL bClearMessages = TRUE);
	virtual CWnd* ShowNoModal(CDiagnostic* pDiagnostic, int iX = 0, int iY = 0);

private:
	CDefaultMessagesLogDialog* m_pMessagesLog;
};

TB_EXPORT IDiagnosticViewer* AfxCreateDefaultViewer();

#include "endh.dex"
