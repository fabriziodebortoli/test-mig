#pragma once

#include <TBGes\ExtDocView.h>
#include <TbGes\BodyEdit.h>
#include <TbGes\Tabber.h>
#include "TbGes\JsonFormEngineEx.h"

#include "UIDSMonitor.h"

#include "beginh.dex"
/*
/////////////////////////////////////////////////////////////////////////////
//					class CValidationFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CValidationFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CValidationFrame)

public:
	CValidationFrame(){}

protected:
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar*	pTabbedBar);
	
};
*/

//////////////////////////////////////////////////////////////////////////////
//						class CValidationMonitorView
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CValidationMonitorView : public CJsonFormView
{
	DECLARE_DYNCREATE(CValidationMonitorView)

public:
	CValidationMonitorView				();

public:
	DValidationMonitor* GetDocument		() const;

public:		
//	virtual	void BuildDataControlLinks	();

protected:
	//{{AFX_MSG(CValidationMonitorView)
		afx_msg	void OnTimer			(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
/*
/////////////////////////////////////////////////////////////////////////////
//			Class CValidationMonitorTileGrp
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CValidationMonitorTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CValidationMonitorTileGrp)

public:
	CValidationMonitorTileGrp() {}

protected:
	virtual void Customize();
};
*/

/////////////////////////////////////////////////////////////////////////////
//					CDocumentsToValidateCombo Definition
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CDocumentsToValidateCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CDocumentsToValidateCombo)
	
public:
	CDocumentsToValidateCombo();

protected:
	virtual	void OnFillListBox();
};

//////////////////////////////////////////////////////////////////////////////
//							CValidationMonitorDetailBodyEdit				//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CValidationMonitorDetailBodyEdit : public CJsonBodyEdit
{ 
	DECLARE_DYNCREATE(CValidationMonitorDetailBodyEdit)

public:
	CValidationMonitorDetailBodyEdit();
};

//////////////////////////////////////////////////////////////////////////////
//							CValidationSummaryDetailBodyEdit				//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CValidationSummaryDetailBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CValidationSummaryDetailBodyEdit)

public:
	CValidationSummaryDetailBodyEdit();

protected:
	virtual BOOL OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow);
};

//////////////////////////////////////////////////////////////////////////////
//							CValidationMonitorFKToFixPanel					//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class CValidationMonitorFKToFixPanel : public CTaskBuilderDockPane
{
	DECLARE_DYNCREATE(CValidationMonitorFKToFixPanel)

public:
	CValidationMonitorFKToFixPanel();

protected:
	virtual BOOL CanBeClosed() const { return FALSE; }
};

//////////////////////////////////////////////////////////////////////////////
//							CValidationMonitorFKToFixView					//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CValidationMonitorFKToFixView : public CJsonFormView
{
	DECLARE_DYNCREATE(CValidationMonitorFKToFixView)

public:
	CValidationMonitorFKToFixView();
};

/*
//////////////////////////////////////////////////////////////////////////////
//							CValidationMonitorFKToFixTileGrp				//
//////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT CValidationMonitorFKToFixTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CValidationMonitorFKToFixTileGrp)

protected:
	virtual void Customize();
};
*/