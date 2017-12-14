
#include "stdafx.h"

#include <TBGes\Tabber.h>
#include <TBGes\BodyEdit.h>
#include <TbGes\FormMng.h>

#include "DSManager.h"
#include "DSTables.h"
#include "DProviders.h"
#include "UIProviders.h"
#include  "UIProviders.hjson"


/////////////////////////////////////////////////////////////////////////////
//				CSynchroProviderCombo Implementation
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE (CSynchroProviderCombo, CStrCombo)

//-----------------------------------------------------------------------------
CSynchroProviderCombo::CSynchroProviderCombo()
	:
	CStrCombo()
{}
	
//-----------------------------------------------------------------------------
void CSynchroProviderCombo::OnFillListBox()
{
	CStrCombo::OnFillListBox();
  
	for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
	{	
		CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
		if (pSynchroProvider->IsValid())
			AddAssociation(pSynchroProvider->m_Name, pSynchroProvider->m_Name);
	}

}
/*
////////////////////////////////////////////////////////////////////////////////////
//					class CProvidersMainTileGrp implementation
/////////////////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProvidersMainTileGrp, CTileGroup)

//---------------------------------------------------------------------------------------------
void CProvidersMainTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	
	AddJsonTile(IDD_DS_PROVIDERS_MAIN);

	CTilePanel* pPanel = AddPanel(_T("ProvidersConnectionStatusTilePanel"), _TB("Connection Data"), CLayoutContainer::COLUMN, CLayoutContainer::STRETCH);

	if (pPanel)
	{
		pPanel->ShowAsTile();
		pPanel->SetCollapsible(TRUE);
		pPanel->SetTileStyle(AfxGetTileDialogStyleNormal());
		
		pPanel->AddJsonTile(IDD_DS_PROVIDERS_CONNECTION);
		pPanel->AddJsonTile(IDD_DS_PROVIDERS_STATUS);
	}

	AddJsonTile(IDD_DS_PROVIDERS_PARAMETERS);
}

/////////////////////////////////////////////////////////////////////////////
//				CProvidersView Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProvidersView, CMasterFormView)

//-----------------------------------------------------------------------------
CProvidersView::CProvidersView()
	:
	CMasterFormView(_NS_VIEW("SynchroProviders"), IDD_DS_PROVIDERS)
{
}

//-----------------------------------------------------------------------------
void CProvidersView::BuildDataControlLinks()
{
	AddTileGroup(IDC_DS_PROVIDERS_TILE_GROUP, RUNTIME_CLASS(CProvidersMainTileGrp), _NS_TILEGRP("ProvidersMainTileGrp"));
}

*/