#pragma once

#include <TbGes\DocumentSession.h>
#include "beginh.dex"
class CParsersForModule;
class CStatusBarMsg;
class CThreadInfo;
class AddOnModule;

// CLoginThread
typedef CMap<CString, LPCTSTR, BOOL, BOOL> MapStringToBOOL;
class TB_EXPORT CLoginThread : public CLoginContext
{
	friend class CDocumentThread;
	friend class CTaskBuilderApp;

	CTBEvent		m_StartupReady;
	bool			m_bProxy;//se true, non e' un vero thread a se' stante ma si aggangia al main thread

	UINT_PTR		m_nTimeoutTimer;//timer for session timeout
	UINT_PTR		m_nCheckTokenTimer;//timer for authenticationtoken validation
	UINT_PTR		m_nCheckSqlSessionTimer;

	DECLARE_LOCKABLE(CXMLVariableArray, m_Variables);
	DECLARE_LOCKABLE(MapStringToBOOL, m_NamespaceMap);

	DECLARE_DYNCREATE(CLoginThread)

protected:
	CLoginThread();           // protected constructor used by dynamic creation
	virtual ~CLoginThread();
	
	BOOL InitInstanceInternal();
	int ExitInstanceInternal();
	int	RunInternal();
public:
	void WaitForStartupReady();
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	BOOL Login(const CString& sAuthenticationToken, BOOL bAsync);
	BOOL Login();
	CDocument* OpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible = TRUE);
	virtual BOOL CanClose();
	virtual BOOL CloseAllDocuments();
	virtual BOOL SilentCloseLoginDocuments();
	virtual void DestroyAllDocuments();
	virtual void AddFormatter(Formatter* pFormatter);
	void UpdateMenuTitle();
	virtual int GetOpenDocuments();
	virtual int GetOpenDocumentsInDesignMode();
	virtual void AddDiagnostic(CDiagnostic *pDiagnostic);	
	void	SetTbDevParams	(HANDLE hDevMode, HANDLE hDevNames);
	void	GetTbDevParams	(HANDLE& hDevMode, HANDLE& hDevNames);
	virtual void Close();
	void StartTimeoutTimer(UINT nTimeoutMilliseconds);
	CXMLVariable* GetVariable(const CString& sName);
	void DeclareVariable(const CString& sName, DataObj* pDataObj, BOOL bOwnsDataObj, BOOL bReplaceExisting = FALSE);
	void CopyVariables(CAbstractFormDoc* pDoc);
#ifdef DEBUG
	virtual BOOL PumpMessage() { return __super::PumpMessage(); }
#endif

protected:
	void PostLoginInitializations();
	void InitLoginContext();
	virtual void SetMenuWindowHandle(HWND handle);

	afx_msg void OnCloseLoginAsync(WPARAM wParam, LPARAM lParam);
	afx_msg void OnStopThread(WPARAM, LPARAM);
	afx_msg void OnThreadTimer(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
private:
	void AddDiagnosticInternal(CDiagnostic *pDiagnostic);
	void GetDocumentThreadArray(CDWordArray& arThreadIds);
	void FireOnDSNChanged();
	BOOL InternalCanClose();
	BOOL LoadComponents (AddOnModule*	pAddOnMod, const CString& sUser, CStatusBarMsg *pStatusBar);
	void ChangeXpStyle();
	void ValidateDate();
	void LoadTaskBuilderParameters	();
	BOOL ManageException(CException* pException);
	BOOL CanUseNamespace(const CString&	nameSpace, int grantType);

	BOOL InternalCanUseNamespace		(const CString&	nameSpace, int grantType, BOOL& bCanUse);
	void InternalSetCanUseNamespace		(const CString&	nameSpace, int grantType, BOOL bCanUse);
public:
	virtual CWnd* GetMainWnd();
	virtual int Run();
	void CheckAuthenticationToken();
protected:
	CThreadInfo* AddThreadInfos(CThreadInfoArray& arInfos);
};

/////////////////////////////////////////////////////////////////////////////
//								CultureOverride
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CultureOverride : public CObject
{
	DECLARE_DYNCREATE(CultureOverride);
public:
	CultureOverride() {  }
public:
	CString	m_strPreferredLanguage;
	CString	m_strApplicationLanguage;
	LCID	m_wDataBaseCultureLCID;
};



inline CLoginThread* AfxGetLoginThread() { return (CLoginThread*) AfxGetLoginContext(); }
#include "endh.dex"