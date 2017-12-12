
#pragma once

#include "beginh.dex"

class HotFilterDataPicker;
class CHotFilterDataPickerTBGrid;
class CHotFilterDataPickerFrame;
class CHotFilterDataPickerTabManager;
//class HotFilterQueryParser;
class CUnpinnedTilesPane;
class CPinnedTilesTileGroup;
class DataPickerRecordSet;
class CTBGridColumnInfo;
class CTilePanel;

/////////////////////////////////////////////////////////////////////////////
//				class HotFilterDataPicker
/////////////////////////////////////////////////////////////////////////////

//=============================================================================
class TB_EXPORT HotFilterDataPicker : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(HotFilterDataPicker)
	friend class DBTDataPicker;
	friend class DataPickerRecordSet;
	friend class HotFilterRange;
	friend class CHotFilterDataPickerView;
	friend class CHotFilterDataPickerResultsBETileDlg;
	friend class CHotFilterDataPickerFrame;
	friend class CHotFilterDataPickerTileGrp;
	friend class CHotFilterDataPickerTBGridTileDlg;
	friend class CHotFilterDataPickerTBGrid;
	friend class CHotFilterDataPickerResultsTBGridTileDlg;
	
public: 
	HotFilterDataPicker();
	~HotFilterDataPicker();

			void				Execute			()						{ OnBatchExecute();}
			DataBool*			GetSelected		(SqlRecord* pRecord);

public:	
	virtual	BOOL			OnOpenDocument			(LPCTSTR);
			HotFilterRange*	GetHotFilter			()				{ return m_pHotFilter; }

	// TBGridControl columns
	CTBGridColumnInfo* AddColumn
		(
			const	CString&		sColumnName,
			const	CString&		sColumnTitle,
					DataObj*		pDataObj,
					CRuntimeClass*	pHKLClass = NULL,
					BOOL			bWithDescription = FALSE
		);

	CTBGridColumnInfo* AddColumn
		(
			const	CString&		sColumnName,
			const	CString&		sColumnTitle,
					DataType		aType,
					int				nLen
		);

protected:
	virtual BOOL	OnAttachData 			();
	virtual	void	DisableControlsForBatch	();
	virtual BOOL	OnToolbarDropDown		(UINT nID, CMenu& aMenu);

	void	GetSelectedItems		(DataObjArray& selectedItems, DataObjArray& unselectedItems);
	void	PopulateGrid			(BOOL bPreselect);

private:
	void		CreateGridLayout			();
	void		RefreshQueryList			(BOOL bReset = FALSE);
	void		SetFiltersCollapsed			(BOOL bSet);
	void		SetFiltersEnabled			(BOOL bEnable);
	void		ClearGrid					();

protected:	
	DataPickerRecordSet*				m_pRecordSet;
	CHotFilterDataPickerTBGrid*			m_pTBGridControl;

private:
	BOOL						m_bSelectAll; 
	SqlRecordLocals* 			m_pLocalFields;
	HotFilterRange*				m_pHotFilter;
	int							m_nLastIDC;
	int							m_nSelectedRows;
	CUnpinnedTilesPane*			m_pUnpinnedTilesPane;
	BOOL						m_bCanDoExtractData;
	CTilePanel*					m_pFiltersPanel;
	CArray<CBaseTileDialog*>	m_FilterTiles;

	//CQueriesCombo			m_SavedQueriesCombo;
		
	LPCTSTR					m_SelectedColumnName;

protected:	
	//{{AFX_MSG( HotFilterDataPicker )
		afx_msg void	OnExtractData			();
		afx_msg void	OnUpdateExtractData		(CCmdUI*);
		afx_msg void	OnUndoExtraction		();
		afx_msg void	OnUpdateUndoExtraction	(CCmdUI*);
		afx_msg void	OnCompleted				();
		afx_msg	void	OnEnableCompleted		(CCmdUI*);
		afx_msg void	OnSaveQuery				();
		afx_msg void	OnSaveQueryAs			();
		afx_msg void	OnDeleteQuery			();
		afx_msg	void	OnEnableSaveQueryMenu	(CCmdUI*);
		afx_msg	void	OnQueryChanged			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};



#include "endh.dex"

