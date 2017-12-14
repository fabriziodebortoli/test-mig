#pragma once

#include <TbGes\ExtDocView.h>

#include "BDResourcesLayout.h"

#include  "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//			class CResourcesLayoutActionSlaveView definition
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class CResourcesLayoutActionSlaveView : public CJsonSlaveFormView
{
	DECLARE_DYNCREATE(CResourcesLayoutActionSlaveView)

	BDResourcesLayout* GetDocument() const;

protected:	
	CResourcesLayoutActionSlaveView();

protected:		
	afx_msg void OnMoveClick	();
	afx_msg void OnCopyClick	();
	afx_msg void OnCancelClick	();
	DECLARE_MESSAGE_MAP()
};

#include  "endh.dex"
