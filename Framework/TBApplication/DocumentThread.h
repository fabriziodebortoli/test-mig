#pragma once

#include <TbGenlib\baseapp.h>

//intervallo di tempo (in sec) trascorso il quale il thread di documento viene considerato "orfano" di una finestra di browser
//che interagisca con lui
//il browser effettua un ping ogni tot millisecondi. Questo valore e' definito nel file:
//TaskBuilder\Library\TBWebFormControl\TBWebFormServerControl.cs nella costante MaxPingInterval
#define THREAD_INTERACTION_THRESHOLD_TIME 65  // 65 sec



//////////////////////////////////////////////////////////////////////////////////	
//								CDocumentThread class declaration
//////////////////////////////////////////////////////////////////////////////////	
class CDocumentThread : public CTBWinThread
{
	

	DECLARE_DYNCREATE(CDocumentThread)
	friend class CTbCommandManager;
private:
	CDocument*					m_pMainDocument = NULL;
	CLoginThread*				m_pLoginThread = NULL;
	DocInvocationParams*		m_pInfo = NULL;
	int							m_nOpenStack = 0;
	ThreadHooksState			m_nThreadHooksState = DEFAULT;
	const CSingleDocTemplate*	m_pOriginalTemplate = NULL;
	CTBEvent					m_Ready;
	BOOL						m_bMakeVisible = TRUE;
	bool						m_bManagedMessagePump = false;
	CString						m_strContextName;

	
	CDocumentThread();           // protected constructor used by dynamic creation
	virtual ~CDocumentThread();


	void ManageRunException(CException* pException);
	void ManageInitException(CException* pException);
	CDocument*  OpenDocument();
	void DestroyAllDocuments();
	BOOL InitInstanceInternal();
	int ExitInstanceInternal();
	
public:
	// own document manager
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	virtual BOOL IsDocumentThread() { return TRUE; }
	virtual CWnd* GetMainWnd();

	CDocument* OpenDocumentOnNewThread(const CSingleDocTemplate* pTemplate, DocInvocationParams* pInfo, const CString &strContextName, BOOL bMakeVisible = TRUE);
	void Start(const CString &strContextName);
	CDocument* OpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible = TRUE);
	static LRESULT CALLBACK GetMessageProc(int nCode, WPARAM wParam, LPARAM lParam);
protected:
	DECLARE_MESSAGE_MAP()
	virtual BOOL DoEvents(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/);
	
public:
	virtual int					Run();
	virtual LRESULT				ProcessWndProcException(CException* e, const MSG* pMsg);
			void				OnStopThread(WPARAM, LPARAM);
	virtual BOOL				CanStopThread();
	virtual bool IsManaged()	{ return m_bManagedMessagePump; }
	DocInvocationParams* GetDocInfo() { return m_pInfo; }
#ifdef DEBUG
	virtual BOOL PumpMessage() { return __super::PumpMessage(); }
#endif
protected:
	CThreadInfo* AddThreadInfos(CThreadInfoArray& arInfos);
};

