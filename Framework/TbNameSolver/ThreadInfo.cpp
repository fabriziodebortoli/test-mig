#include "StdAfx.h"
#include "ThreadInfo.h"
#include "JsonSerializer.h"

// CThreadInfo

//----------------------------------------------------------------------------
CString CThreadInfo::ToXmlString()
{ 
	CString s;
	s.Format(_T("<Thread name=\"%s\" id=\"%d\" title=\"%s\" company=\"%s\" user=\"%s\" operationDate=\"%s\" loginthreadname=\"%s\" loginthreadid=\"%d\" documentthread=\"%s\" canbestopped=\"%s\" remoteinterfaceattached=\"%s\" inactivitytime=\"%I64d\" modalstate=\"%s\">%s</Thread>"), 
		m_strThreadName, 
		m_nThreadId,
		m_strMainWndTitle,
		m_strCompany,
		m_strUser,
		m_strOperationDate,
		m_strLoginThreadName, 
		m_nLoginThreadId,
		m_bDocumentThread ? _T("true") : _T("false"), 
		m_bCanBeSafeStopped ? _T("true") : _T("false"),	
        m_bRemoteUserInterfaceAttached ? _T("true") : _T("false"),
		m_nInactivityTime,
		m_bInModalState ? _T("true") : _T("false"),
		m_arThreadInfos.ToXmlString()
		);
	return s;
}

//----------------------------------------------------------------------------
void CThreadInfo::ToJSON(CJsonSerializer& serializer)
{
	serializer.WriteString(_T("name"), m_strThreadName);
	serializer.WriteInt(_T("id"), m_nThreadId);
	serializer.WriteString(_T("title"), m_strMainWndTitle);
	serializer.WriteString(_T("company"), m_strCompany);
	serializer.WriteString(_T("user"), m_strUser);
	serializer.WriteString(_T("operationDate"), m_strOperationDate);
	serializer.WriteString(_T("loginthreadname"), m_strLoginThreadName);
	serializer.WriteInt(_T("loginthreadid"), m_nLoginThreadId);
	serializer.WriteBool(_T("documentthread"), m_bDocumentThread == TRUE);
	serializer.WriteBool(_T("canbestopped"), m_bCanBeSafeStopped == TRUE);
	serializer.WriteBool(_T("remoteinterfaceattached"), m_bRemoteUserInterfaceAttached == TRUE);
	CString sInactivityTime;
	sInactivityTime.Format(_T("%I64d"), m_nInactivityTime);
	serializer.WriteString(_T("inactivitytime"), sInactivityTime);
	serializer.WriteBool(_T("modalstate"), m_bInModalState == TRUE);
	 
	m_arThreadInfos.ToJSON(serializer);
}

//----------------------------------------------------------------------------
CThreadInfoArray::~CThreadInfoArray()
{
	Clear();
}
//----------------------------------------------------------------------------
CString CThreadInfoArray::ToXmlString()
{
	TB_LOCK_FOR_READ();

	CString s;
	s.Append(_T("<Threads>"));

	for (int i = 0; i < __super::GetCount(); i++)
	{
		s.Append(_T("\r\n"));
		s.Append(GetAt(i)->ToXmlString());
	}
	s.Append(_T("\r\n</Threads>"));

//	CJsonSerializer jsonSerializer;
//	CString f = ToJSON(jsonSerializer);
	
	return s;
}

//----------------------------------------------------------------------------
CString CThreadInfoArray::ToJSON(CJsonSerializer& serializer)
{
	TB_LOCK_FOR_READ();

	serializer.OpenArray(_T("Threads"));
	for (int i = 0; i < __super::GetCount(); i++)
	{
		serializer.OpenObject(i);
		GetAt(i)->ToJSON(serializer);
		serializer.CloseObject();
	}
	serializer.CloseArray();
	
	return serializer.GetJson();
}

//----------------------------------------------------------------------------
int CThreadInfoArray::GetCount()
{
	TB_LOCK_FOR_READ();
	return __super::GetCount();
}
//----------------------------------------------------------------------------
void CThreadInfoArray::Clear()
{
	TB_LOCK_FOR_WRITE();

	for (int i = 0; i < __super::GetCount(); i++)
		delete GetAt(i);

	RemoveAll();
}