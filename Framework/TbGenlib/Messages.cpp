
#include "stdafx.h" 
//#include "afxpriv.h"

#include <TbGeneric\ParametersSections.h>
#include "messages.h"
#include "diagnosticmanager.h"

//==============================================================================
//          Class CMessages implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CMessages, CDiagnostic);
//------------------------------------------------------------------------------
CMessages::CMessages(const CString& strTitle, const CString& strFileName)
{
	m_UIInterface.Init(0, strTitle, strFileName);
	AttachViewer(AfxCreateDefaultViewer());

	EnableTraceInEventViewer(IsEventViewerTraceEnabled());
}

//------------------------------------------------------------------------------
CMessages::CMessages(UINT nID, const CString& strFileName)
{
	m_UIInterface.Init(nID, _T(""), strFileName);
	AttachViewer(AfxCreateDefaultViewer());
	EnableTraceInEventViewer(IsEventViewerTraceEnabled());
}

//------------------------------------------------------------------------------
BOOL CMessages::IsFatal() const { return __super::HasFatalError(); }
BOOL CMessages::HintFound() const { return __super::InfoFound(); }

//------------------------------------------------------------------------------
void CMessages::SetDefaultButton(UINT nID) { m_UIInterface.SetDefaultButton(nID); }
void CMessages::SetHelpId(UINT nID) { }
void CMessages::SetTextButtonOK(CString str) { m_UIInterface.SetTextButtonOK(str); }
void CMessages::SetTextButtonFix(CString str) { m_UIInterface.SetTextButtonFix(str); }

//------------------------------------------------------------------------------
void CMessages::Add(const CString& strMessage, MessageType type /*MSG_ERROR*/, BOOL bOnlyIfNotExist /*FALSE*/)
{
	__super::Add(strMessage, (CDiagnostic::MsgType) type, szNoErrorCode, bOnlyIfNotExist);
}

//------------------------------------------------------------------------------
void CMessages::Add(const CStringArray& strMessages, MessageType type /*MSG_ERROR*/)
{
	__super::Add(strMessages, (CDiagnostic::MsgType) type);
}

//------------------------------------------------------------------------------
void CMessages::Add(const CString& strMessage, const CString& strDetail, MessageType type /*MSG_ERROR*/)
{
	__super::Add(strMessage + strDetail, (CDiagnostic::MsgType) type);
}

//------------------------------------------------------------------------------
BOOL CMessages::ShowAndLogTo(const CString& sFileName, BOOL bClearMessages /*= TRUE*/)
{
	LogToFile(sFileName);
	return Show(bClearMessages);
}
//------------------------------------------------------------------------------
BOOL CMessages::LogToFile(const CString& sFileName)
{
	return CDiagnosticManager::LogToFile(this, sFileName);
}
//------------------------------------------------------------------------------
BOOL CMessages::LoadFromFile(const CString& sFileName)
{
	return CDiagnosticManager::LoadFromFile(this, sFileName);
}
//------------------------------------------------------------------------------
void CMessages::ToArray(DataObjArray& arValues)
{
	CDiagnosticManager::ToArray(this, arValues);
}

//------------------------------------------------------------------------------
void CMessages::ToJson(CJsonSerializer& ser)
{

	ser.OpenArray(_T("buttons"));
	int index = 0;
	ser.OpenObject(index++);
	ser.WriteString(_T("text"), m_UIInterface.m_strOKText.IsEmpty() ? _TB("OK") : m_UIInterface.m_strOKText);
	ser.WriteBool(_T("ok"), true);
	if (m_UIInterface.m_nIDDefault == IDOK)
		ser.WriteBool(_T("default"), true);
	ser.WriteBool(_T("enabled"), true);
	ser.CloseObject();

	ser.OpenObject(index++);
	ser.WriteString(_T("text"), m_UIInterface.m_strFixText.IsEmpty() ? _TB("Correct") : m_UIInterface.m_strFixText);
	ser.WriteBool(_T("cancel"), true);
	if (m_UIInterface.m_nIDDefault == IDCANCEL)
		ser.WriteBool(_T("default"), true);
	ser.WriteBool(_T("enabled"), (WarningFound() || InfoFound()) && !ErrorFound());
	ser.CloseObject();

	ser.CloseArray();
	__super::ToJson(ser);

}

//------------------------------------------------------------------------------
void CMessages::UpdateDataView()
{
	if (m_pDocument)
		m_pDocument->UpdateDataView(TRUE);
}