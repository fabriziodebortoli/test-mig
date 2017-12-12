#pragma once

#include <TbGeneric\Dibitmap.h>
#include <TBGes\ExtDocView.h>
#include <TBGes\Tabber.h>
#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include <TbGes\BODYEDIT.H>
#include <TbGes\JsonFormEngineEx.h>

#include "beginh.dex"

//=============================================================================
// usefuls class declaration
//=============================================================================
class DProviders;
class TDS_Providers;


/////////////////////////////////////////////////////////////////////////////
//					CSynchroProviderCombo Definition
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CSynchroProviderCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CSynchroProviderCombo)
	
public:
	CSynchroProviderCombo();


protected:
	virtual	void OnFillListBox();
};
/*
//////////////////////////////////////////////////////////////////////////////
//						class CProvidersView
//////////////////////////////////////////////////////////////////////////////
class CProvidersView : public CMasterFormView
{
	DECLARE_DYNCREATE(CProvidersView)

public:	
	CProvidersView();

public:		
	virtual	void BuildDataControlLinks();
};

/////////////////////////////////////////////////////////////////////////////
//							CProvidersMainTileGrp
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CProvidersMainTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CProvidersMainTileGrp)

protected:
	virtual	void Customize();
};
*/