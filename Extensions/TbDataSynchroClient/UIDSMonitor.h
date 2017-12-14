#pragma once

#include <TBGes\ExtDocView.h>
#include <TbGes\BodyEdit.h>
#include <TbGes\Tabber.h>
#include "TbGes\JsonFormEngineEx.h"

#include "beginh.dex"

//=============================================================================
// usefuls class declaration
//=============================================================================
class DSMonitor;
class DBTSynchroInfoMonitor;


/////////////////////////////////////////////////////////////////////////////
//					class CDSMonitorFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CDSMonitorFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CDSMonitorFrame)

public:
	CDSMonitorFrame(){}

protected:
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar*	pTabbedBar);
	
};

/////////////////////////////////////////////////////////////////////////////
//			Class CDSMonitorTileGrp
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CDSMonitorTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CDSMonitorTileGrp)

public:
	CDSMonitorTileGrp() {}

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//						class CDSMonitorView
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CDSMonitorView : public CMasterFormView
{
	DECLARE_DYNCREATE(CDSMonitorView)

public:	
	CDSMonitorView();

public:
	DSMonitor* GetDocument() const;

public:		
	virtual	void BuildDataControlLinks();
	
protected:
	//{{AFX_MSG(CDSMonitorView)
		afx_msg	void OnTimer				(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//			Class CDSMonitorLegendTileGrp
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CDSMonitorLegendTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CDSMonitorLegendTileGrp)

protected:
	virtual void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//			Class CDSMonitorLegendView
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CDSMonitorLegendView : public CMasterFormView
{
	DECLARE_DYNCREATE(CDSMonitorLegendView)

public:
	CDSMonitorLegendView();

private:
	virtual	void BuildDataControlLinks();

};

/////////////////////////////////////////////////////////////////////////////
//					CDocumentsToSynchCombo Definition
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CDocumentsToSynchCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CDocumentsToSynchCombo)
	
public:
	CDocumentsToSynchCombo();

protected:
	virtual	void OnFillListBox();
};

//////////////////////////////////////////////////////////////////////////////
//							CMonitorDetailBodyEdit							//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CMonitorDetailBodyEdit : public CJsonBodyEdit
{ 
	DECLARE_DYNCREATE(CMonitorDetailBodyEdit)

private:
	CBEButton*	m_pBEBtnNext;
	CBEButton*	m_pBEBtnPrev;
	DataInt		n_Page;
public:
	CMonitorDetailBodyEdit();
	virtual void OnBeforeCustomize();
	virtual void Customize();
	DSMonitor * GetDocument() const { return (DSMonitor*)__super::GetDocument(); }
	
protected:
	//{{AFX_MSG(CSOSDocSenderResultsBodyEdit)
	afx_msg void OnNextClicked();
	afx_msg void OnPrevClicked();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//							CMonitorSummaryDetailBodyEdit					//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CMonitorSummaryDetailBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CMonitorSummaryDetailBodyEdit)

public:
	CMonitorSummaryDetailBodyEdit();

protected:
	virtual BOOL OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow);
};