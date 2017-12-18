#include "stdafx.h"
#include "CJsonModelGenerator.h"
#include "DocumentSession.h"

#include "JsonForms\JsonModelGenerator\IDD_GENERATE_JSON_MODEL_FRAME.hjson"

//-----------------------------------------------------------------------------
CJsonModelGenerator::CJsonModelGenerator(CAbstractFormDoc* pDoc)
	: CBusinessServiceProviderObj(pDoc, _NS_CD_BSP("Framerwork.TbGes.TbGes.Documents.JsonModelGenerator"))
{
	MessagesToEvents* pMTEMap = new MessagesToEvents();
	pMTEMap->OnCommand(ID_GENERATE, _BSP_EVENT("OnGenerate"));
	Init(pMTEMap);
	DECLARE_VAR_JSON(NrOfDocuments);
	m_NrOfDocuments.SetAlwaysEditable();
}
//-----------------------------------------------------------------------------
CJsonModelGenerator::~CJsonModelGenerator()
{
}
//-----------------------------------------------------------------------------
void CJsonModelGenerator::Generate()
{
	CJsonFormView* pView = (CJsonFormView*)m_pCallerDoc->CreateSlaveView(IDD_GENERATE_JSON_MODEL_FRAME);
	m_pFrame = (CMasterFrame*)pView->GetParentFrame();
	m_pFrame->DoModal();
}
//-----------------------------------------------------------------------------
void CJsonModelGenerator::OnGenerate()
{
	if (m_pCallerDoc->GetNotValidView(TRUE))
		return;
	CJsonParser dummy;

	CString sNamespace = m_pCallerDoc->GetNamespace().ToString();
	sNamespace.TrimLeft(_T("Document."));
	CString sPath = AfxGetPathFinder()->GetContainerPath(CPathFinder::TB) + _T("\\server\\web-server\\mock\\") + sNamespace;
	if (!ExistPath(sPath))
		CreateDirectoryTree(sPath);

	CRecordingDocumentSession session;
	session.Start();
	session.OnAddThreadWindow(m_pCallerDoc->GetFrameHandle());
	session.Stop();
	session.Save(sPath + _T("\\runDocument.json"));

	session.Start();
	session.PushWindowStringsToClients(m_pCallerDoc->GetFrameHandle(), AfxGetCulture());
	session.Stop();
	session.Save(sPath + _T("\\getWindowStrings.json"));

	session.Start();
	m_pCallerDoc->ResetJsonData(dummy);
	session.PushDataToClients(m_pCallerDoc);
	session.PushButtonsStateToClients(m_pCallerDoc->GetFrameHandle());
	session.PushMessageMapToClients(m_pCallerDoc);
	session.PushActivationDataToClients();
	session.Stop();
	session.Save(sPath + _T("\\getDocumentData.json"));
	if (m_pCallerDoc->m_pDBTMaster->GetRecord()->IsEmpty())
		m_pCallerDoc->FirstRecord();

	const CUIntArray& idx = m_pCallerDoc->m_pDBTMaster->GetRecord()->GetPrimaryKeyIndexes();
	CString strOldKey;
	for (int i = 0; i < m_NrOfDocuments; i++)
	{
		if (m_pCallerDoc->m_pDBTMaster->GetRecord()->IsEmpty())
			break;
		CString strKey = m_pCallerDoc->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();
		if (strKey == strOldKey)
			break;
		strOldKey = strKey;
		session.Start();
		m_pCallerDoc->ResetJsonData(dummy);
		session.PushDataToClients(m_pCallerDoc);
		session.PushButtonsStateToClients(m_pCallerDoc->GetFrameHandle());
		m_pCallerDoc->OnEditRecord();
		m_pCallerDoc->ResetJsonData(dummy);
		session.PushDataToClients(m_pCallerDoc);
		session.PushButtonsStateToClients(m_pCallerDoc->GetFrameHandle());
		m_pCallerDoc->GoInBrowseMode();
		session.Stop();
		session.Save(sPath + _T("\\") + cwsprintf(L"%d", i) + _T(".doc.json"));
		m_pCallerDoc->NextRecord();
	}
	session.Start();
	m_pCallerDoc->ResetJsonData(dummy);
	m_pCallerDoc->NewRecord();
	session.PushDataToClients(m_pCallerDoc);
	session.PushButtonsStateToClients(m_pCallerDoc->GetFrameHandle());
	m_pCallerDoc->Escape(FALSE);
	session.Stop();
	session.Save(sPath + _T("\\new.json"));

	AfxMessageBox(cwsprintf(_TB("JSON Model successfully generated on path %s"), sPath));

	m_pFrame->PostMessage(WM_CLOSE);
}

IMPLEMENT_DYNAMIC(CJsonModelGenerator, CBusinessServiceProviderObj)
BEGIN_TB_EVENT_MAP(CJsonModelGenerator)
	TB_EVENT(CJsonModelGenerator, OnGenerate)
END_TB_EVENT_MAP