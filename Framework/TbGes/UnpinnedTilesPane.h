#pragma once

#include <TbGenlib\TBDockPane.h>
#include "TileManager.h"

#include "beginh.dex"

class CMasterFrame;

//======================================================================
class TB_EXPORT CUnpinnedTilesPane : public CTaskBuilderDockPane
{
	DECLARE_DYNCREATE(CUnpinnedTilesPane);

public:
	CUnpinnedTilesPane();

public:
	static CUnpinnedTilesPane* Create(CMasterFrame* pFrame, BOOL bForceInitialUpdate =  TRUE, CRuntimeClass* pPaneClass = NULL);
};

//======================================================================
class TB_EXPORT CPinnedTilesTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CPinnedTilesTileGroup)

private:
	TDisposablePtr<CUnpinnedTilesPane>	m_pUnpinnedTilesPane;
	BOOL								m_bOwnsPane;
public:
	CPinnedTilesTileGroup();

	void AttachUnpinnedTilesPane(CUnpinnedTilesPane* pPane);

	CBaseTileDialog* AddJsonTile(UINT nDialogID, CLayoutContainer*	pContainer);
	CBaseTileDialog* AddTile	(CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog* AddTile	(CLayoutContainer*	pContainer, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog* AddTile	(CLayoutContainer* pContainer, CBaseTileDialog* pTileDialog, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, CObject* pOwner = NULL);
	CBaseTileDialog* AddTile	(CTilePanel* pPanel, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);

	virtual void OnAfterCustomize();
	
	void SetOwnsPane			(BOOL bOwns);

protected:
	virtual void OnTileDialogUnpin(CBaseTileDialog* pTileDlg);

private:
	void CreateUnpinnedTilePane();
	void DestroyUnpinnedTilePane();
	void LoadUnpinnedTilesPane();
	void ClearUnpinnedTilesPane();

	afx_msg void OnDestroy();
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
