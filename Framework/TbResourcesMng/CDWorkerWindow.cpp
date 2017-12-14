#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGes\Dbt.h>

#include "ADMResourcesMng.h"

#include "CDWorkerWindow.h"
#include "ModuleObjects\WorkerWindow\JsonForms\IDD_WORKER_WINDOW.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char  THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//					Class CDWorkerWindow
//==============================================================================
IMPLEMENT_DYNCREATE(CDWorkerWindow, CClientDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDWorkerWindow, CClientDoc)
	//{{AFX_MSG_MAP(CDWorkerWindow)
	ON_COMMAND			(ID_WORKER_WINDOW, OnWorkerWindow)
	ON_UPDATE_COMMAND_UI(ID_WORKER_WINDOW, OnEnableWorkerWindow)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDWorkerWindow::CDWorkerWindow()
	:
	CClientDoc	(),
	m_pDoc		(NULL),
	m_pTRWorkers(NULL)
{
	m_CreatedDate.	SetFullDate();
	m_ModifiedDate.	SetFullDate();
}

//-----------------------------------------------------------------------------
CDWorkerWindow::~CDWorkerWindow()
{
	SAFE_DELETE(m_pTRWorkers);
}

//-----------------------------------------------------------------------------
BOOL CDWorkerWindow::OnAttachData()
{ 
	if (GetServerDoc()->m_pDBTMaster)
		GetServerDoc()->m_pDBTMaster->GetTable()->SetForceReadTraceColumns(TRUE);
	m_pTRWorkers = new TRWorkers (GetServerDoc());
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDWorkerWindow::Customize()
{
	AddButton (ID_WORKER_WINDOW, _NS_TOOLBARBTN("WorkerWindow"), TBIcon(szIconUser, TOOLBAR), _TB("Worker"), _T("Tools"), _TB("Open the worker window"));
}

//-----------------------------------------------------------------------------
void CDWorkerWindow::OnEnableWorkerWindow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetServerDoc()->GetFormMode() == CAbstractFormDoc::BROWSE);
}

//-----------------------------------------------------------------------------
void CDWorkerWindow::OnWorkerWindow()
{
	GetServerDoc() ->GetNotValidView(TRUE);

	if (m_pDoc)
		return;

	m_pDoc = (ADMWorkerWindowObj*) AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMWorkerWindowObj), szDefaultViewMode, NULL, GetServerDoc(), GetServerDoc());
	if (m_pDoc)
	{
		LoadWorker();
		m_pDoc->SetWorker(	m_CreatedWorker,	m_CreatedWorkerDes,		m_CreatedDate,	m_CreatedWorkerOfficePhone,		m_CreatedWorkerEmail,	m_CreatedWorkerPicture,
							m_ModifiedWorker,	m_ModifiedWorkerDes,	m_ModifiedDate,	m_ModifiedWorkerOfficePhone,	m_ModifiedWorkerEmail,	m_ModifiedWorkerPicture);
	}
}

//-----------------------------------------------------------------------------
BOOL CDWorkerWindow::OnPrepareAuxData()
{ 
	LoadWorker();

	if (m_pDoc)
	{
		if (GetServerDoc()->GetFormMode() ==  CBaseDocument::BROWSE)
			m_pDoc->SetWorker(	m_CreatedWorker,	m_CreatedWorkerDes,		m_CreatedDate,	m_CreatedWorkerOfficePhone,		m_CreatedWorkerEmail,	m_CreatedWorkerPicture,
								m_ModifiedWorker,	m_ModifiedWorkerDes,	m_ModifiedDate,	m_ModifiedWorkerOfficePhone,	m_ModifiedWorkerEmail,	m_ModifiedWorkerPicture);
		else
			m_pDoc->GetDocument()->GetMasterFrame()->SendMessage(WM_CLOSE);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDWorkerWindow::OnGoInBrowseMode()
{ 
	LoadWorker();
}

//-----------------------------------------------------------------------------
void CDWorkerWindow::LoadWorker()
{ 
	if (!GetServerDoc() || !GetServerDoc()->m_pDBTMaster || !m_pTRWorkers)
	{
		m_CreatedWorker.			Clear();
		m_CreatedWorkerDes.			Clear();
		m_CreatedWorkerLastName.	Clear();
		m_CreatedDate.				Clear();
		m_CreatedWorkerOfficePhone.	Clear();
		m_CreatedWorkerEmail.		Clear();
		m_CreatedWorkerPicture.		Clear();
		m_ModifiedWorker.			Clear();
		m_ModifiedWorkerDes.		Clear();
		m_ModifiedWorkerLastName.	Clear();
		m_ModifiedDate.				Clear();
		m_ModifiedWorkerOfficePhone.Clear();
		m_ModifiedWorkerEmail.		Clear();
		m_ModifiedWorkerPicture.	Clear();
		return;
	}

	m_CreatedWorker		= GetServerDoc()->m_pDBTMaster->GetDBTRecord()->f_TBCreatedID;
	m_CreatedDate		= GetServerDoc()->m_pDBTMaster->GetDBTRecord()->f_TBCreated;
	m_ModifiedWorker	= GetServerDoc()->m_pDBTMaster->GetDBTRecord()->f_TBModifiedID;
	m_ModifiedDate		= GetServerDoc()->m_pDBTMaster->GetDBTRecord()->f_TBModified;

	if (m_CreatedWorker.IsEmpty())
	{
		m_CreatedWorkerDes.			Clear();
		m_CreatedWorkerLastName.	Clear();
		m_CreatedWorkerOfficePhone.	Clear();
		m_CreatedWorkerEmail.		Clear();
		m_CreatedWorkerPicture.		Clear();
	}
	else
	{
		m_pTRWorkers->FindRecord(m_CreatedWorker);
		m_CreatedWorkerDes			= m_pTRWorkers->GetWorker();
		m_CreatedWorkerLastName		= m_pTRWorkers->GetRecord()->f_LastName;
		m_CreatedWorkerOfficePhone	= m_pTRWorkers->GetRecord()->f_Telephone4;
		m_CreatedWorkerEmail		= m_pTRWorkers->GetRecord()->f_Email;
		m_CreatedWorkerPicture		= m_pTRWorkers->GetRecord()->f_ImagePath;
	}

	if (m_ModifiedWorker.IsEmpty())
	{
		m_ModifiedWorkerDes.		Clear();
		m_ModifiedWorkerLastName.	Clear();
		m_ModifiedWorkerOfficePhone.Clear();
		m_ModifiedWorkerEmail.		Clear();
		m_ModifiedWorkerPicture.	Clear();
	}
	else
	{
		m_pTRWorkers->FindRecord(m_ModifiedWorker);
		m_ModifiedWorkerDes			= m_pTRWorkers->GetWorker();
		m_ModifiedWorkerLastName	= m_pTRWorkers->GetRecord()->f_LastName;
		m_ModifiedWorkerOfficePhone	= m_pTRWorkers->GetRecord()->f_Telephone4;
		m_ModifiedWorkerEmail		= m_pTRWorkers->GetRecord()->f_Email;
		m_ModifiedWorkerPicture		= m_pTRWorkers->GetRecord()->f_ImagePath;
	}
}

//-----------------------------------------------------------------------------
BOOL CDWorkerWindow::OnShowStatusBarMsg(CString& sMsg)
{ 
	if (m_CreatedWorkerLastName.IsEmpty() && m_ModifiedWorkerLastName.IsEmpty())
		return TRUE;

	sMsg = _T("");

	if (m_ModifiedWorkerLastName.IsEmpty() || (m_CreatedWorkerLastName == m_ModifiedWorkerLastName && m_CreatedDate == m_ModifiedDate))
		sMsg = cwsprintf(_TB("Created: {0-%s} on {1-%s}"), m_CreatedWorkerLastName.GetString(), m_CreatedDate.FormatData());
	else
		sMsg = cwsprintf(_TB("Modified: {0-%s} on {1-%s}"), m_ModifiedWorkerLastName.GetString(), m_ModifiedDate.FormatData());
	return TRUE;
}
