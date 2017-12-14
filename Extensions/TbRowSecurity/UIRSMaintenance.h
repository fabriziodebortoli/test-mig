
#pragma once

//NOW INCLUDED IN COMMON PCH: #include <tbges\extdoc.h>

// TaskBuilder
#include <TBGes\TileDialog.h>
#include <TBGes\TileManager.h>

#include "beginh.dex"


//=============================================================================
// CRSMaintenanceFrame
//=============================================================================
class CRSMaintenanceFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CRSMaintenanceFrame)
	
protected:
    CRSMaintenanceFrame();

public:
	BDRSMaintenance*		GetDocument();

	BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CRSMaintenanceView : public CMasterFormView
{
	DECLARE_DYNCREATE(CRSMaintenanceView)
	
public:	
	CRSMaintenanceView();
	
public:		
	BDRSMaintenance* GetDocument() const;

public:
	virtual	void BuildDataControlLinks();	
};

/////////////////////////////////////////////////////////////////////////////
//						CTileRSMaintenanceBase
/////////////////////////////////////////////////////////////////////////////
class CTileRSMaintenance : public CTileDialog
{
	DECLARE_DYNCREATE(CTileRSMaintenance)

public:
	// Construction
	CTileRSMaintenance();
	~CTileRSMaintenance();
	
public:
	BDRSMaintenance*			GetDocument		()		const { return (BDRSMaintenance*) m_pDocument; }

protected:
	virtual void BuildDataControlLinks();
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//						CTileRSEntities
/////////////////////////////////////////////////////////////////////////////
class CTileRSEntities : public CTileDialog
{
	DECLARE_DYNCREATE(CTileRSEntities)

public:
	// Construction
	CTileRSEntities();

public:
	BDRSMaintenance*			GetDocument()		const { return (BDRSMaintenance*)m_pDocument; }

protected:
	virtual void BuildDataControlLinks();
	DECLARE_MESSAGE_MAP()
};


//=============================================================================
class TB_EXPORT CRSMaintenanceTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CRSMaintenanceTileGroup)

protected:
	virtual void Customize();
};

#include "endh.dex"
