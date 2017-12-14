#pragma once 

#include <TbGeneric\DataObj.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\ExtDocFrame.h>
#include <TbGes\BodyEdit.h>
#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include "TbGes\JsonFormEngineEx.h"

#include "CDNotification.h"

//includere alla fine degli include del .H
#include "beginh.dex"


class DataSynchroAddOnFields;
class CDataSynchroClientView;

//////////////////////////////////////////////////////////////////////////////
//						    CDataSynchroPane
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CDataSynchroPane : public CTaskBuilderDockPane
{
	DECLARE_DYNCREATE(CDataSynchroPane)

public:
	CDNotification* m_pDataSynchroClientDoc;

public:
	CDataSynchroPane();
	CDataSynchroPane(CDNotification* pClientDoc);

public:
	CDataSynchroClientView* GetDataSynchroPaneView() const;
	virtual void OnCustomizeToolbar();
protected:
	//{{AFX_MSG(CDataSynchroPane)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/*
//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBodyTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CTileMngTileDlg : public CTileDialog
{
DECLARE_DYNCREATE(CTileMngTileDlg)

public:
CTileMngTileDlg();
~CTileMngTileDlg();

private:
CDataSynchroTileMng* m_pTileMng;

protected:
virtual void BuildDataControlLinks();
virtual void EnableTileDialogControlLinks(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
};
*/

//////////////////////////////////////////////////////////////////////////////
//             class CDataSynchroClientView definition 
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CDataSynchroClientView : public CJsonFormView
{
	DECLARE_DYNCREATE(CDataSynchroClientView)

protected:
	// Construction
	CDataSynchroClientView();
	~CDataSynchroClientView();

private:
	UINT	m_nWindowTimer;

private:
	CDNotification* GetDataSynchroClientDoc() const;

public:
	void	StartTimer();
	void	StopTimer();

public:
	//virtual	BOOL OnInitDialog				();
	//virtual	void OnInitialUpdate();
	//virtual void OnCustomizeToolbar			();

protected:
	//{{AFX_MSG(CDataSynchroClientView)		
	afx_msg	void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/*
/////////////////////////////////////////////////////////////////////////////
//	             class  CDataSynchroTileMng declaration
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT  CDataSynchroTileMng : public CTileManager
{
DECLARE_DYNCREATE(CDataSynchroTileMng)

public:
CDataSynchroTileMng();

protected:
virtual void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//				Class CDataSynchroInfoTileGrp Declaration
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CDataSynchroInfoTileGrp : public CTileGroup
{
DECLARE_DYNCREATE(CDataSynchroInfoTileGrp)

protected:
virtual void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//				Class CDataSynchroLogActionsTileGrp Declaration
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CDataSynchroLogActionsTileGrp : public CTileGroup
{
DECLARE_DYNCREATE(CDataSynchroLogActionsTileGrp)

protected:
virtual void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//							CNotificationMainTileGrp
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CNotificationMainTileGrp : public CTileGroup
{
DECLARE_DYNCREATE(CNotificationMainTileGrp)

protected:
virtual	void Customize();
};
*/

/*
//////////////////////////////////////////////////////////////////////////////
//							CActionsBodyEdit								//
//////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT CActionsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CActionsBodyEdit)

public:
	CActionsBodyEdit();

public:
	virtual BOOL OnGetToolTipProperties(CBETooltipProperties* tp);
};
]*/

#include "endh.dex"
