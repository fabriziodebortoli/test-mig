#include "stdafx.h"

#include "RMFunctions.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "BDWorkerWindow.h"  

#include "ModuleObjects\WorkerWindow\JsonForms\IDD_WORKER_WINDOW.hjson"
#include "ModuleObjects\WorkerWindow\JsonForms\IDD_WORKER_WINDOW_TOOLBAR.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//                    		BDWorkerWindow								
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDWorkerWindow, CAuxiliaryFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDWorkerWindow, CAuxiliaryFormDoc)
	//{{AFX_MSG_MAP(BDWorkerWindow)
	ON_BN_CLICKED(IDC_WORKER_WINDOW_CLOSE, OnCloseWindowClick)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------ 
BDWorkerWindow::BDWorkerWindow()
	:
	m_pDoc					(NULL),
	m_pCreatedWorkerPicture	(NULL),
	m_pModifiedWorkerPicture(NULL)
{
	m_bBatch = TRUE;
	m_CreatedDate.	SetFullDate();
	m_ModifiedDate.	SetFullDate();	
}

//-----------------------------------------------------------------------------
BOOL BDWorkerWindow::OnOpenDocument(LPCTSTR pParam)
{
	m_pDoc = (CAbstractFormDoc*) GET_AUXINFO(pParam);
	return (CAbstractFormDoc::OnOpenDocument(pParam));
}

//-----------------------------------------------------------------------------
void BDWorkerWindow::OnFrameCreated()
{
	CWnd* pWnd = GetMasterFrame();
	ScaleFrame((CFrameWnd*) pWnd, TRUE);
	__super::OnFrameCreated();
}

//-----------------------------------------------------------------------------
BOOL BDWorkerWindow::OnAttachData()
{              
	SetFormTitle(_TB("Worker Window"));

	DECLARE_VAR_JSON(CreatedWorkerDes);
	DECLARE_VAR_JSON(CreatedDate);
	DECLARE_VAR_JSON(CreatedWorkerEmail);
	DECLARE_VAR_JSON(CreatedWorkerOfficePhone);
	DECLARE_VAR_JSON(CreatedWorkerPicture);
	DECLARE_VAR_JSON(ModifiedWorkerDes);
	DECLARE_VAR_JSON(ModifiedDate);
	DECLARE_VAR_JSON(ModifiedWorkerEmail);
	DECLARE_VAR_JSON(ModifiedWorkerOfficePhone);
	DECLARE_VAR_JSON(ModifiedWorkerPicture);

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDWorkerWindow::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_WORKER_WINDOW_CREATED_WORKER_DES)
	{
		CWorkerStatic* pWorkerStatic = dynamic_cast<CWorkerStatic*>(pCtrl);
		if (pWorkerStatic)
			pWorkerStatic->SetWorker(&m_CreatedWorker);
	}
	
	if (nIDC == IDC_WORKER_WINDOW_MODIFIED_WORKER_DES)
	{
		CWorkerStatic* pWorkerStatic = dynamic_cast<CWorkerStatic*>(pCtrl);
		if (pWorkerStatic)
			pWorkerStatic->SetWorker(&m_ModifiedWorker);
	}

	if (nIDC == IDC_WORKER_WINDOW_CREATED_WORKER_PICTURE)
	{
		m_pCreatedWorkerPicture = dynamic_cast<CResourcesPictureStatic*>(pCtrl);
		m_pCreatedWorkerPicture->OnCtrlStyleBest();
	}

	if (nIDC == IDC_WORKER_WINDOW_MODIFIED_WORKER_PICTURE)
	{
		m_pModifiedWorkerPicture = dynamic_cast<CResourcesPictureStatic*>(pCtrl);
		m_pModifiedWorkerPicture->OnCtrlStyleBest();
	}
}

//-----------------------------------------------------------------------------
void BDWorkerWindow::DisableControlsForBatch()
{
	m_CreatedWorkerEmail.	SetReadOnly();
	m_ModifiedWorkerEmail.	SetReadOnly();
}

//-----------------------------------------------------------------------------
void BDWorkerWindow::SetWorker( DataLng		aCreatedWorker, 
								DataStr		aCreatedWorkerDes, 
								DataDate	aCreatedDate, 
								DataStr		aCreatedWorkerOfficePhone, 
								DataStr		aCreatedWorkerEmail, 
								DataStr		aCreatedWorkerPicture, 
								DataLng		aModifiedWorker, 
								DataStr		aModifiedWorkerDes, 
								DataDate	aModifiedDate,
								DataStr		aModifiedWorkerOfficePhone, 
								DataStr		aModifiedWorkerEmail,
								DataStr		aModifiedWorkerPicture) 	
{
	m_CreatedWorker					= aCreatedWorker;
	m_CreatedWorkerDes				= aCreatedWorkerDes;
	m_CreatedDate					= aCreatedDate;
	m_CreatedWorkerOfficePhone		= aCreatedWorkerOfficePhone;
	m_CreatedWorkerEmail			= aCreatedWorkerEmail;
	m_CreatedWorkerPicture			= aCreatedWorkerPicture.IsEmpty() ? TBGlyph(szGlyphWorkerBig) : aCreatedWorkerPicture;

	m_pCreatedWorkerPicture->SetWorker(aCreatedWorker);

	if (aModifiedWorker == aCreatedWorker && aModifiedDate == aCreatedDate)
	{
		m_ModifiedWorker.			Clear();
		m_ModifiedWorkerDes.		Clear();
		m_ModifiedDate.				Clear();
		m_ModifiedWorkerOfficePhone.Clear();
		m_ModifiedWorkerEmail.		Clear();
		m_ModifiedWorkerPicture.	Clear();
	}
	else
	{
		m_ModifiedWorker			= aModifiedWorker;
		m_ModifiedWorkerDes			= aModifiedWorkerDes;
		m_ModifiedDate				= aModifiedDate;
		m_ModifiedWorkerOfficePhone	= aModifiedWorkerOfficePhone;
		m_ModifiedWorkerEmail		= aModifiedWorkerEmail;
		m_ModifiedWorkerPicture		= aModifiedWorkerPicture.IsEmpty() ? TBGlyph(szGlyphWorkerBig) : aModifiedWorkerPicture;
	}

	m_pModifiedWorkerPicture->SetWorker(aModifiedWorker);

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDWorkerWindow::OnCloseWindowClick()
{
	CloseDocument();
}