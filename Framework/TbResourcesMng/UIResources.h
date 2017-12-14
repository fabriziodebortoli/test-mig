
#pragma once

#include <TbGes\JsonFormEngineEx.h>

#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//						CResourcesView
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class CResourcesView : public CJsonFormView
{
	DECLARE_DYNCREATE(CResourcesView)
	
public:	
	CResourcesView();
};

// BE Fields (Custom Data)
//=============================================================================
class CResourcesFieldsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CResourcesFieldsBodyEdit)

public:
	CResourcesFieldsBodyEdit() {}


public:
	virtual void Customize();
	virtual BOOL OnGetToolTipProperties(CBETooltipProperties* pTooltip);
	virtual BOOL OnGetCustomColor(CBodyEditRowSelected* pCurrentRow);
};

#include "endh.dex"
