#pragma once

#include "tbresourcelocker.h"
#include "beginh.dex"

class CThreadInfo;
class CJsonSerializer;


class TB_EXPORT CThreadInfoArray : public CArray<CThreadInfo*, CThreadInfo*>, public CTBLockable
{
public:
	virtual ~CThreadInfoArray();
	CString ToXmlString();
	CString ToJSON(CJsonSerializer& serializer);
	int GetCount();
	void Clear();
	virtual LPCSTR  GetObjectName() const { return "CThreadInfoArray"; }

};


//*****************************************************************************
class TB_EXPORT CThreadInfo
{
private:
	HWND		m_hwndMainWindow;
	BOOL		m_bDocumentThread;
	BOOL		m_bInModalState;
	DWORD		m_nThreadId;
	DWORD		m_nLoginThreadId;
	CString		m_strThreadName;
	CString		m_strLoginThreadName;
	CString		m_strMainWndTitle;  //main form title
	CString		m_strCompany;
	CString		m_strUser;
	CString		m_strOperationDate;
	BOOL		m_bCanBeSafeStopped;
	BOOL		m_bRemoteUserInterfaceAttached;  //stimando il tempo dall'ultima interazione memorizza se il thread e' pilotato da remoto (Browser) o meno
    time_t		m_nInactivityTime; //in secondi

public:
	CThreadInfoArray m_arThreadInfos;
public:
	CThreadInfo
		(
		HWND hwndMainWindow,
		BOOL bDocumentThread, 
		BOOL bInModalState, 
		DWORD nThreadId, 
		DWORD nLoginThreadId, 
		const CString& strThreadName,
		const CString& strLoginThreadName
		)
	{
		m_hwndMainWindow = hwndMainWindow;
		m_bDocumentThread = bDocumentThread;
		m_bInModalState = bInModalState;
		m_strThreadName = strThreadName;		
		m_strLoginThreadName = strLoginThreadName;
		m_nThreadId = nThreadId;
		m_nLoginThreadId = nLoginThreadId;
		m_strMainWndTitle = _T("");
		m_strCompany = _T("");
		m_strUser = _T("");
		m_strOperationDate = _T("");
		m_bCanBeSafeStopped = TRUE;
		m_bRemoteUserInterfaceAttached = TRUE;
		m_nInactivityTime = 0;
	}
	CString ToXmlString();
	void	ToJSON(CJsonSerializer& serializer);

	void SetTitle(CString strMainWndTitle) { m_strMainWndTitle = strMainWndTitle;}
	void SetCompany(CString strCompany) { m_strCompany = strCompany;}
	void SetUser(CString strUser) { m_strUser = strUser;}
	void SetOperationDate(CString strOperationDate) { m_strOperationDate = strOperationDate;}
	void SetCanBeStopped(BOOL bCanBeSafeStopped) { m_bCanBeSafeStopped = bCanBeSafeStopped;}
	void SetRemoteUserInterfaceAttached(BOOL bRemoteUserInterfaceAttached) { m_bRemoteUserInterfaceAttached = bRemoteUserInterfaceAttached;}
	void SetInactivityTime(time_t nInactivityTime) { m_nInactivityTime = nInactivityTime;}
};


#include "endh.dex"