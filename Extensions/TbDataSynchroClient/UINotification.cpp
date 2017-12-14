
#include "stdafx.h" 

#include <TbGes\EXTDOC.H>
#include <TbGes\BEColumnInfo.H>

#include <TbGenlib\barcombo.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "DSManager.h"
#include "TbDataSynchroClientEnums.h"
#include "UIProviders.h"
#include "UINotification.hjson"
#include "UINotification.h"
#include "UIProviders.h"

static TCHAR szIconNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");
static TCHAR sz25S[] = _T("25x25");


//////////////////////////////////////////////////////////////////////////////
//						    CDataSynchroPane
//////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDataSynchroPane, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDataSynchroPane, CTaskBuilderDockPane)
	//{{AFX_MSG_MAP(CDataSynchroPane)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDataSynchroPane::CDataSynchroPane()
	:
	CTaskBuilderDockPane(RUNTIME_CLASS(CDataSynchroClientView)),
	m_pDataSynchroClientDoc(NULL)
{
}

//-----------------------------------------------------------------------------
CDataSynchroPane::CDataSynchroPane(CDNotification* pClientDoc)
	:
	CTaskBuilderDockPane(RUNTIME_CLASS(CDataSynchroClientView)),
	m_pDataSynchroClientDoc(pClientDoc)
{
}


//-----------------------------------------------------------------------------
CDataSynchroClientView* CDataSynchroPane::GetDataSynchroPaneView() const
{
	return (CDataSynchroClientView*)GetFormWnd(RUNTIME_CLASS(CDataSynchroClientView));
}

//-----------------------------------------------------------------------------
void CDataSynchroPane::OnCustomizeToolbar()
{
	if (m_pToolBar == NULL)
		return;

	m_pToolBar->AddButton
	(
		ID_FORCE_SYNCHRONIZATION,
		_T("ForceSynchro"),
		TBIcon(szIconOutbound, TOOLBAR),
		_TB("Force Synchronization"),
		_TB("Force Synchronization\r\nForce Synchronization")
	);

	if (USE_VALIDATION)
		m_pToolBar->AddButton
		(
			ID_FORCE_VALIDATION,
			_T("ForceValidation"),
			TBIcon(szIconOk, TOOLBAR),
			_TB("Force Validation"),
			_TB("Force Validation\r\nForce Validation")
		);

	m_pToolBar->AddButton
	(
		ID_DS_REFRESH_BTN,
		_T("DataSynchroRefresh"),
		TBIcon(szIconRefresh, TOOLBAR),
		_TB("Refresh"),
		_TB("Refresh\r\nRefresh")
	);
}


/*
////////////////////////////////////////////////////////////////////////////////
//							CNotificationMainTileGrp
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CNotificationMainTileGrp, CTileGroup)

//----------------------------------------------------------------------------------
void CNotificationMainTileGrp::Customize()
{
CDNotification* pClientDoc = (CDNotification*)((CAbstractFormDoc*)GetDocument())->GetClientDoc(RUNTIME_CLASS(CDNotification));

SetLayoutType(CLayoutContainer::VBOX);

AddJsonTile(IDD_DATA_SYNCHRO_BUTTONS_ACTIONS);

AddJsonTile(IDD_DATA_SYNCHRO_STATUS);
AddJsonTile(IDD_DATA_VALIDATION_STATUS);
AddJsonTile(IDD_DATA_SYNCHRO_HISTORY);

AddTile(RUNTIME_CLASS(CTileMngTileDlg), IDD_DATASYNCHRO_TILE_MNG, _T(""), TILE_AUTOFILL);
}

//////////////////////////////////////////////////////////////////////////////////
//						class CDataSynchroInfoTileGrp implementation
////////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDataSynchroInfoTileGrp, CTileGroup)

//------------------------------------------------------------------------------------
void CDataSynchroInfoTileGrp::Customize()
{
AddJsonTile(IDD_DATA_SYNCHRO_INFO);
}

//////////////////////////////////////////////////////////////////////////////////
//						class CDataSynchroLogActionsTileGrp implementation
////////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDataSynchroLogActionsTileGrp, CTileGroup)

//------------------------------------------------------------------------------------
void CDataSynchroLogActionsTileGrp::Customize()
{
AddJsonTile(IDD_DATA_SYNCHRO_ACTIONS);
}

/////////////////////////////////////////////////////////////////////////////////
//					class CDataSynchroTileMng implementation
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDataSynchroTileMng, CTileManager)

//------------------------------------------------------------------------------------
CDataSynchroTileMng::CDataSynchroTileMng()
:
CTileManager()
{
SetShowMode(NORMAL);
}

//-----------------------------------------------------------------------------------
void CDataSynchroTileMng::Customize()
{
AddTileGroup
(
RUNTIME_CLASS(CDataSynchroLogActionsTileGrp),
_NS_TILEGRP("LogActions"),
_TB("Log Actions")
);

CSynchroProvider* pDMSProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(DMSInfinityProvider);
if (pDMSProvider && pDMSProvider->IsValid())
AddTileGroup
(
RUNTIME_CLASS(CDataSynchroInfoTileGrp),
_NS_TILEGRP("SynchroInfo"),
_TB("Synchro Info")
);
}

//////////////////////////////////////////////////////////////////////////////
//					    CTileMngTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CTileMngTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
CTileMngTileDlg::CTileMngTileDlg()
:
CTileDialog(_NS_TILEDLG("TileMng"), IDD_DATASYNCHRO_TILE_MNG),
m_pTileMng(NULL)
{
SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
CTileMngTileDlg::~CTileMngTileDlg()
{
}

//-----------------------------------------------------------------------------
void CTileMngTileDlg::BuildDataControlLinks()
{
m_pTileMng = (CDataSynchroTileMng*)AddTileManager(IDC_DATA_SYNCHRO_TILE_MNG, RUNTIME_CLASS(CDataSynchroTileMng), _NS_TABMNG("DataSynchroTileMng"));
SetMaxWidth(295);
}

//------------------------------------------------------------------------------------------------------------
void CTileMngTileDlg::EnableTileDialogControlLinks(BOOL bEnable , BOOL bMustSetOSLReadOnly)
{
CDNotification* pClientDoc = (CDNotification*)((CAbstractFormDoc*)GetDocument())->GetClientDoc(RUNTIME_CLASS(CDNotification));

m_pTileMng->TabDialogEnable(IDC_DATA_SYNCHRO_TILE_MNG, IDD_DATA_SYNCHRO_INFO, pClientDoc->m_CurrProviderName.IsEqual(DataStr(DMSInfinityProvider)));
}

*/


//////////////////////////////////////////////////////////////////////////////
//							CDataSynchroClientView implementation				//
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDataSynchroClientView, CJsonFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDataSynchroClientView, CJsonFormView)
	//{{AFX_MSG_MAP(CDataSynchroClientView)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDataSynchroClientView::CDataSynchroClientView()
	:
	CJsonFormView(IDD_DATASYNCHRO_VIEW),
	m_nWindowTimer(0)
{
//	StartTimer();
}

//-----------------------------------------------------------------------------
CDataSynchroClientView::~CDataSynchroClientView()
{
}

//-----------------------------------------------------------------------------
void CDataSynchroClientView::StartTimer()
{
	m_nWindowTimer = SetTimer(ID_DATA_SYNC_NOTIFICATION_TIMER, 2000, NULL);
	ASSERT(m_nWindowTimer == ID_DATA_SYNC_NOTIFICATION_TIMER); // Se l'identificatore del timer è univoco, questo stesso valore viene restituito da SetTimer.
}

//-----------------------------------------------------------------------------
void CDataSynchroClientView::StopTimer()
{
	KillTimer(m_nWindowTimer);
}

//-----------------------------------------------------------------------------
void CDataSynchroClientView::OnTimer(UINT nIDEvent)
{
	if (nIDEvent == ID_DATA_SYNC_NOTIFICATION_TIMER)
		GetDataSynchroClientDoc()->DoReloadSynchroInfo(TRUE);
}

//---------------------------------------------------------------------------------------------------------------
CDNotification* CDataSynchroClientView::GetDataSynchroClientDoc() const
{
	return (CDNotification*)((CAbstractFormDoc*)GetDocument())->GetClientDoc(RUNTIME_CLASS(CDNotification));
}


/*

//------------------------------------------------------------------------------------------------------------------
void CDataSynchroClientView::OnInitialUpdate()
{
__super::OnInitialUpdate();

__super::OnInitDialog();
SetToolbarStyle(ToolbarStyle::TOP, 25, FALSE, TRUE);
AddTileGroup
(
IDC_DATA_SYNCHRO_TILE_GROUP,
RUNTIME_CLASS(CNotificationMainTileGrp),
_NS_TILEGRP("NotificationMainTileGrp")
);
StartTimer();

}
*/
/*
//-------------------------------------------------------------------------------------------------------------------
void CDataSynchroClientView::OnCustomizeToolbar()
{
int		nResult;
TCHAR	bufferSynchro1[512];
TCHAR	bufferSynchro2[512];
TCHAR   bufferRefresh[512];

nResult = swprintf_s(bufferSynchro1,	szIconNamespace, sz25S, szIconOutbound);
nResult = swprintf_s(bufferSynchro2,	szIconNamespace, sz25S, szIconOk);
nResult = swprintf_s(bufferRefresh,		szIconNamespace, sz25S, szIconRefresh);

if (m_pToolBar == NULL)
return;

m_pToolBar->AddButton
(
ID_FORCE_SYNCHRONIZATION,
_T("ForceSynchro"),
bufferSynchro1,
_TB("Force Synchronization"),
_TB("Force Synchronization\r\nForce Synchronization")
);

if (USE_VALIDATION)
m_pToolBar->AddButton
(
ID_FORCE_VALIDATION,
_T("ForceValidation"),
bufferSynchro2,
_TB("Force Validation"),
_TB("Force Validation\r\nForce Validation")
);

m_pToolBar->AddButton
(
ID_DS_REFRESH_BTN,
_T("DataSynchroRefresh"),
bufferRefresh,
_TB("Refresh"),
_TB("Refresh\r\nRefresh")
);

}


/////////////////////////////////////////////////////////////////////////////
//				class CActionsBodyEdit Implementation					   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CActionsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
CActionsBodyEdit::CActionsBodyEdit()
{
}

//------------------------------------------------------------------------------------
BOOL CActionsBodyEdit::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	TEnhDS_ActionsLog* pRec = (TEnhDS_ActionsLog*)(pTooltip->m_pRec);

	if (pTooltip->m_nControlID == IDC_DS_ACTION_STATUS)
	{
		pTooltip->m_strTitle.Empty();

		if (pRec)
		{
			if (pRec->l_SynchStatusBmp == TBGlyph(szGlyphOk)) // Ok
				pTooltip->m_strText = _TB("Synchronized");
			else if (pRec->l_SynchStatusBmp == TBGlyph(szIconError)) // Error
				pTooltip->m_strText = _TB("Error");
			else if (pRec->l_SynchStatusBmp == TBGlyph(szGlyphRemove)) // Excluded
				pTooltip->m_strText = _TB("Excluded");
			else // Wait
				pTooltip->m_strText = _TB("Wait");

			return TRUE;
		}
	}

	return __super::OnGetToolTipProperties(pTooltip);
}
*/