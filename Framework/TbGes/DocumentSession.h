#pragma once
#include <TbGeneric\WndObjDescription.h>
#include <TbGenLibManaged\Main.h>
#include <TbGenLib\BaseDoc.h>
#include <TbGes\ExtDocAbstract.h>
#include "beginh.dex"

class CThreadDescription;
class CTBResponse;
class CWndBodyTableDescription; 


enum SetDataResult { OK = 1, FAIL  = 2, CONFLICT = 3};
enum ObjectType { DOCUMENT, REPORT, FUNCTION };

TB_EXPORT CBaseDocument* GetDocumentFromHwnd(HWND hWnd);
//--------------------------------------------------------------------------------
class TB_EXPORT CJSonResponse : public CJsonSerializer
{
public:
	void SetMessage(const CString& sMessage, CDiagnostic::MsgType type = CDiagnostic::Error);
	void SetOK();
	void SetError();
	void SetResult(BOOL bResult);
};

//--------------------------------------------------------------------------------
class TB_EXPORT CDocumentSession : public CDocumentSessionObj
{
private:
	int							m_nSuspendPushToClient = 0;
	short						m_nPushDataNeedLevel = 0;
	bool						m_bIgnoreModelChanges = false;
	
	int							m_nMessageType = 0;
	bool						m_bDiagnosticResult = false;
	CString						m_strMessage;			//messaggio corrente della AfxMessageBox
	CDiagnostic*				m_pDiagnostic = NULL;	//diagnostico corrente della CMessages modale
	CTBEvent					m_ModalClosed;

	DWORD						m_nLoginThreadID;
	DWORD						m_nDocumentThreadID = 0;
	CArray<HWND>				m_arWindowsToNotifyForCreation;//finestre la cui creazione necessita di essere notificata al client
	CArray<IJsonModelProvider*>	m_arJsonModelsToNotify;//modelli dati che necessitano di essere notificati al client

protected:
	bool						m_bPushOnlyWebBoundData = false;//ottimizzazione: per mandare i soli dati usati dal client

public:
	int							m_bOperationId = 0;
	CDocumentSession (DWORD nLoginThreadID);
	~CDocumentSession ();
	void PushDataToClients();
	virtual void OnAddThreadWindow(HWND hwnd);
	virtual void OnRemoveThreadWindow(HWND hwnd);
	virtual void PushDataToClients(IJsonModelProvider* pProvider);
	virtual void PushMessageMapToClients(CAbstractFormDoc* pDoc);
	virtual void IgnoreModelChanges(bool bIgnore) { m_bIgnoreModelChanges = bIgnore; }
	virtual void PushItemSourceToClients(const CString& cmpId, const CStringArray& arDescriptions, const CStringArray& arData);
	virtual int MessageBoxDialog(LPCTSTR lpszText, UINT nType);
	virtual void PushToClients(CJsonSerializer& resp);
	virtual BOOL DiagnosticDialog(CDiagnostic* pDiagnostic, BOOL bModal);
	void CloseMessageBoxDialog(CJsonParser& json);
	void CloseDiagnosticDialog(CJsonParser& json);
	void PushWindowStringsToClients(HWND cmpId, const CString& sCulture);
	void PushExistDataCompletedToClient(HWND cmpId,  DataObj *pValue, bool found, bool mustExist, CString requestId);
	void PushWindowsToClients();
	void PushDiagnosticToClients();
	void PushRunErrorToClients();
	void PushMessageToClients();
	void PushActivationDataToClients(HWND hwnd);
	void PushRadarInfoToClient(CAbstractFormDoc* pDoc);
	void PushButtonsStateToClients(HWND hwnd);
	void SuspendPushToClient();
	void ResumePushToClient();
	void PushDataNeeded();
	void PushBehavioursToClient(IBehaviourContext* pProvider);
private:
	void Init();
};

class CRecordingDocumentSession : public CDocumentSession
{
	CJsonSerializer m_Serializer;
	int m_nItems = 0;
public:

	CRecordingDocumentSession();
	void Start();
	void Stop();
	void Save(LPCTSTR szFile);
	void PushToClients(CJsonSerializer& resp);
};
HWND GetHWND(const CString& strHandle);
#include "endh.dex"