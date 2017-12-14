#pragma once

#include <TbGenlib\TBDockPane.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include "parsbtn.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class TB_EXPORT CBaseFrame : public CLocalizableFrame
{
	friend class CAbstractDoc;
	DECLARE_DYNAMIC(CBaseFrame)

protected:
	CDockingPanes				m_DockingPanes;
	CAcceleratorDescription*	m_pAccelDesc			= NULL;
	BOOL					    m_bHasStatusBar			= FALSE;
	CExtButton*					m_pStatusbarButtonHome	= NULL;
	CExtButton*					m_pStatusbarButtonSwitch= NULL;

public:
	~CBaseFrame();
	CTaskBuilderDockPane* CreateDockingPane(CRuntimeClass* pPaneClass, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize = CSize(0, 0), CCreateContext* pCreateContext = NULL, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);
	CTaskBuilderDockPane* CreateDockingPane(CTaskBuilderDockPane* pPane, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize = CSize(0, 0), CCreateContext* pCreateContext = NULL, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);
	CTaskBuilderDockPane* CreateJsonDockingPane(UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize = CSize(0, 0), CCreateContext* pCreateContext = NULL, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);
	BOOL				  DestroyPane(CTaskBuilderDockPane*);
	void				  EnableDockingLayout(CRuntimeClass* pClass = NULL);

	virtual BOOL DestroyWindow();
	virtual void OnFrameCreated();
	virtual BOOL OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext);
	virtual BOOL CreateAuxObjects(CCreateContext* /*pCreateContext*/) { return TRUE; }
	virtual BOOL IsEditingParamsFromExternalController();
	BOOL LoadAccelTable(LPCTSTR lpszResourceName);

	void				MakeSwitchTomenu(CMenu *pMenu);
	virtual CPoint		GetPositionSwitchTo();

	CTileDesignModeParamsObj* GetTileDesignModeParams(CDocument* pDoc = NULL);

	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual CString GetRanorexNamespace();

protected:
	// Status Bar functions
	CExtButton*	AddButtonToPane(CTaskBuilderStatusBar* pStatusBar, INT nIDPane, UINT nID, LPCTSTR lpszCaption, CString sNSStdImage, CString sToolTip);
	// Set the Pane info
	virtual BOOL SetPane(CTaskBuilderStatusBar* pStatusBar, INT nIDPane, INT nWidth, UINT nStyle = SBPS_NORMAL);
};

///////////////////////////////////////////////////////////////////////////////

//=============================================================================
#include "endh.dex"
