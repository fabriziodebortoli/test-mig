
#pragma once

#include <TbGes\JsonFormEngineEx.h>

#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//						CWorkersView
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class CWorkersView : public CJsonFormView
{
	DECLARE_DYNCREATE(CWorkersView)
	
public:	
	CWorkersView();
};

// BE Fields (Custom Data)
//=============================================================================
class CWorkersFieldsBodyEdit : public CJsonBodyEdit
{ 
	DECLARE_DYNCREATE(CWorkersFieldsBodyEdit)
	
public:
	CWorkersFieldsBodyEdit() {}
	
public:                            
	virtual void Customize				();
	virtual BOOL OnGetToolTipProperties	(CBETooltipProperties* pTooltip);
	virtual BOOL OnGetCustomColor		(CBodyEditRowSelected* pCurrentRow);
};

#include "endh.dex"
