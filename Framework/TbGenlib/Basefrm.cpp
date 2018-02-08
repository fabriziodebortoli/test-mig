
#include "stdafx.h"

#include <TbGeneric\JsonFormEngine.h>
#include <TbGeneric\WebServiceStateObjects.h>

#include <TbGeneric\TBThemeManager.h>
#include <TbGes\ExtDocView.h>
#include "TBCommandInterface.h"
#include "basefrm.h"

#include "commands.hrc"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CBaseFrame, CLocalizableFrame)

//----------------------------------------------------------------------------
CBaseFrame::~CBaseFrame()
{
	if (m_hAccelTable)
	{
		DestroyAcceleratorTable(m_hAccelTable);
		m_hAccelTable = NULL;
	}
	if (m_pStatusbarButtonHome)		SAFE_DELETE(m_pStatusbarButtonHome);
	if (m_pStatusbarButtonSwitch)	SAFE_DELETE(m_pStatusbarButtonSwitch);
	delete m_pAccelDesc;
}

//----------------------------------------------------------------------------
BOOL CBaseFrame::DestroyWindow()
{
	this->SuspendLayout();

	for (int i = m_DockingPanes.GetUpperBound(); i >= 0; i--)
	{
		try
		{
			CTaskBuilderDockPane* pPane = m_DockingPanes.GetAt(i);
			ASSERT_VALID(pPane);

			SAFE_DELETE(pPane);
		} 
		catch(...)
		{
			ASSERT(FALSE);
		}
	}
	m_DockingPanes.RemoveAll();

	return __super::DestroyWindow();
}

//----------------------------------------------------------------------------
void CBaseFrame::OnFrameCreated()
{
	__super::OnFrameCreated();

	m_DockingPanes.EndOnCreateFrame();
}

//----------------------------------------------------------------------------
BOOL CBaseFrame::IsEditingParamsFromExternalController()
{
	CDocument* pDoc = GetActiveDocument();
	return pDoc && ((CBaseDocument*)pDoc)->IsEditingParamsFromExternalController();
}

//-----------------------------------------------------------------------------
CTileDesignModeParamsObj* CBaseFrame::GetTileDesignModeParams(CDocument* pDoc)
{
	CBaseDocument* pDocument = dynamic_cast<CBaseDocument*>(pDoc ? pDoc : GetDocument());
	if (pDocument && pDocument->GetManagedParameters())
	{
		CTileDesignModeParamsObj* pParam = dynamic_cast<CTileDesignModeParamsObj*>(pDocument->GetDesignModeManipulatorObj());
		if (pParam)
			return pParam;
	}

	return dynamic_cast<CTileDesignModeParamsObj*>(AfxGetThemeManager());
}

//----------------------------------------------------------------------------
BOOL CBaseFrame::OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext)
{
	if (!GetTileDesignModeParams(pContext->m_pCurrentDoc)->HasToolbar())
		m_bHasToolbar = FALSE;

	if (!__super::OnCreateClient(lpcs, pContext))
		return FALSE;
	EnableDocking(CBRS_ALIGN_TOP);
	EnableAutoHideBars(CBRS_ALIGN_ANY, AfxGetThemeManager()->GetActivateDockPaneOnMouseClick());

	m_DockingPanes.BeginOnCreateFrame(pContext);

	return CreateAuxObjects(pContext);
}

//----------------------------------------------------------------------------
void CBaseFrame::EnableDockingLayout(CRuntimeClass* pClass /*NULL*/)
{
	m_DockingPanes.EnableDockingLayout(pClass);
}

//----------------------------------------------------------------------------
CTaskBuilderDockPane* CBaseFrame::CreateDockingPane(CRuntimeClass* pClass, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle /*dwDefaultBCGDockingBarStyle*/, BOOL bVisible /*= TRUE*/)
{
	return m_DockingPanes.CreatePane(this, pClass, nID, sName, sTitle, wAlignment, aSize, pCreateContext, dwBCGStyle, bVisible);
}

//----------------------------------------------------------------------------
CTaskBuilderDockPane* CBaseFrame::CreateDockingPane(CTaskBuilderDockPane* pPane, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle /*dwDefaultBCGDockingBarStyle*/, BOOL bVisible /*= TRUE*/)
{
	return m_DockingPanes.CreatePane(this, pPane, nID, sName, sTitle, wAlignment, aSize, pCreateContext, dwBCGStyle, bVisible);
}
//----------------------------------------------------------------------------
CTaskBuilderDockPane* CBaseFrame::CreateJsonDockingPane(UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle /*dwDefaultBCGDockingBarStyle*/, BOOL bVisible /*= TRUE*/)
{
	CTaskBuilderDockPane* pPane = new CTaskBuilderDockPane();
	CJsonFormView* pForm = pCreateContext->m_pNewViewClass == RUNTIME_CLASS(CJsonFormView)
		? new CJsonFormView(nID)
		: (CJsonFormView*)pCreateContext->m_pNewViewClass->CreateObject();
	pPane->AddForm(new CTaskBuilderDockPaneForm(pForm, sTitle));
	return m_DockingPanes.CreatePane(this, pPane, nID, sName, sTitle, wAlignment, aSize, pCreateContext, dwBCGStyle, bVisible);
}
//----------------------------------------------------------------------------
BOOL CBaseFrame::DestroyPane(CTaskBuilderDockPane* pPane)
{
	if (m_DockingPanes.DestroyPane(pPane))
	{
		RecalcLayout();
		return TRUE;
	}
	
	return FALSE;
}

// reimplement standard CFrame::LoadAccellTable to allow multiple accelerators
//-----------------------------------------------------------------------------
BOOL CBaseFrame::LoadAccelTable(LPCTSTR lpszResourceName)
{
	ASSERT(lpszResourceName != NULL);
	DWORD dwAcc = REVERSEMAKEINTRESOURCE(lpszResourceName);
	AutoDeletePtr<CJsonContextObj> pContext = TBLoadAcceleratorContext(dwAcc);
	if (pContext)
	{
		int nSize = 0;
		AutoDeletePtr<ACCEL> pAccel = pContext->m_pDescription->m_pAccelerator->ToACCEL(pContext, nSize);

		if (m_hAccelTable)
			DestroyAcceleratorTable(m_hAccelTable);
		m_hAccelTable = CreateAcceleratorTable(pAccel, nSize);
		m_pAccelDesc = pContext->m_pDescription->m_pAccelerator->Clone();
	}
	return (m_hAccelTable != NULL);
}

//-----------------------------------------------------------------------------
CPoint CBaseFrame::GetPositionSwitchTo()
{
	CPoint point(0,0);
	return point;
}

// Make switch menu of open document
//-----------------------------------------------------------------------------
void CBaseFrame::MakeSwitchTomenu(CMenu *pMenu)
{
	int nId = ID_SWITCHTO_START;
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	for (int i = 0; i < handles.GetCount(); i++)
	{
		CBaseDocument* pDoc = (CBaseDocument*)handles[i];
		if (pDoc == GetDocument())
		{
			continue;
		}

		if (!pDoc->CanShowInOpenDocuments())
			continue;

		CString sMenuDesk = pDoc->GetDefaultMenuDescription();
		CString sTitle = pDoc->GetTitle();
		if (!sMenuDesk.IsEmpty())
		{
			sTitle.Append(_T(" - "));
			sTitle.Append(sMenuDesk);
		}
		pMenu->AppendMenu(MF_STRING, nId++, sTitle);
	}
}

//-----------------------------------------------------------------------------
CExtButton*  CBaseFrame::AddButtonToPane(CTaskBuilderStatusBar* pStatusBar, INT nIDPane, UINT nID, LPCTSTR lpszCaption, CString sNSStdImage, CString sToolTip)
{
	if (ULONG(AfxGetThemeManager()->GetStatusbarBkgColor()) < ULONG(RGB(127, 127, 190)))
	{
		// Appen in name fo file icone name _W
		if (sNSStdImage.Right(4).CompareNoCase(_T(".PNG")) == 0)
		{
			sNSStdImage = sNSStdImage.Left(sNSStdImage.GetLength() - 4) + _T("_W") + sNSStdImage.Right(4);
		}
	}

	if (!SetPane(pStatusBar, nIDPane, 40)) {
		return NULL;
	}

	CPaneButton* pButton;
	pButton = new CPaneButton();
	pButton->Create(lpszCaption, WS_CHILD | WS_VISIBLE | BS_OWNERDRAW, CRect(0, 0, 0, 0), pStatusBar, nID);
	pButton->SetPngImages(sNSStdImage);
	pButton->m_bDrawFrame = FALSE;
	pButton->m_bUseImageSize = TRUE;
	pButton->SetBkgColor(AfxGetThemeManager()->GetStatusbarBkgColor());
	pButton->SetToolTip(sToolTip);
	pStatusBar->AddPaneControl(pButton, nIDPane, TRUE);
	return pButton;
}

//-----------------------------------------------------------------------------
BOOL CBaseFrame::SetPane(CTaskBuilderStatusBar* pStatusBar, INT nIDPane, INT nWidth, UINT nStyle /*= SBPS_NORMAL*/)
{
	if (pStatusBar == NULL) return NULL;
	BOOL bFount = FALSE;
	for (int nIndexPane = 0; nIndexPane < pStatusBar->GetPanesCount(); nIndexPane++)
	{
		UINT nGetID;
		UINT nGetStyle;
		int  cxWidth;

		pStatusBar->GetPaneInfo(nIndexPane, nGetID, nGetStyle, cxWidth);

		if (nGetID == nIDPane)
		{
			pStatusBar->SetPaneInfo(nIndexPane, nGetID, nStyle, nWidth);
			bFount = TRUE;
			break;
		}
	}

	ASSERT(bFount);
	return bFount;
}

//-----------------------------------------------------------------------------
CString CBaseFrame::GetRanorexNamespace()
{
	if (GetDocument())
	{
		CBaseDocument* pDocument = dynamic_cast<CBaseDocument*>(GetDocument());
		return cwsprintf(_T("{0-%s}Frame"), pDocument->GetNamespace().GetObjectName());
	}
	return __super::GetRanorexNamespace();
}

