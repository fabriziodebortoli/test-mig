#pragma once

#include <TbGes\TileDialog.h>
#include <TbGes\TileManager.h>
#include <TbGes\BODYEDIT.H>
#include <TbGes\JsonFormEngineEx.h>

#include "beginh.dex"

class BDDMSRepository;
class VSearchFieldCondition;
class DMSCollectionList;

//////////////////////////////////////////////////////////////////////////////
//			       class CCollectionsItemSource definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class CCollectionsItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CCollectionsItemSource)

public:
	CCollectionsItemSource();
	~CCollectionsItemSource();

private:
	DMSCollectionList* m_pCollections;

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

//////////////////////////////////////////////////////////////////////////////
//		       class CFilesExtensionsItemSource definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CFilesExtensionsItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CFilesExtensionsItemSource)

public:
	CFilesExtensionsItemSource();
	~CFilesExtensionsItemSource();

private:
	CStringArray* m_pExtensionsList;

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

//////////////////////////////////////////////////////////////////////////////
//			       class CWorkersListBox definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class CWorkersListBox : public CParsedCheckListBox
{
	DECLARE_DYNCREATE(CWorkersListBox)

public:
	CWorkersListBox();

private: 
	CWorkersTableObj* m_pWorkersTblObj;
	BOOL m_bAlsoDisabled;

public:
	void LoadWorkers(BOOL bAlsoDisabled = FALSE);

protected:
	virtual void		OnFillListBox();
	virtual DataType	GetDataType()	const { return DataType::String; }
};

/////////////////////////////////////////////////////////////////////////////
//					class CBookmarksBodyEdit definition
//			BodyEdit per la visualizzazione dell'elenco dei bookmarks				
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchFieldsConditionsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CSearchFieldsConditionsBodyEdit)

public:
	CSearchFieldsConditionsBodyEdit() {}

public:
	VSearchFieldCondition* GetSearchFieldConditionRow		() { return (VSearchFieldCondition*)m_pDBT->GetRecord(); }
	VSearchFieldCondition* GetCurrentSearchFieldConditionRow() { return (VSearchFieldCondition*)m_pDBT->GetCurrentRow(); }

	BDDMSRepository* GetDocument() { return (BDDMSRepository*)CBodyEdit::GetDocument(); }

public:
	virtual void	Customize();
	virtual	BOOL	OnCommand(WPARAM wParam, LPARAM lParam);
};

///////////////////////////////////////////////////////////////////////////////
//		Class CArchivedDocumentsBodyEdit definition
///////////////////////////////////////////////////////////////////////////////
//
class CArchivedDocumentsBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CArchivedDocumentsBodyEdit)

public:
	//@@TODOMICHI: a regime questi non dovrebbero esistere piu'
	// li rendo pubblici cosi da valorizzarli dentro il documento nel metodo CustomizeBodyEdit
	CBEButton*	m_pBEBtnCheckInOut;
	CBEButton*	m_pBEBtnUndoCheckInOut;
	CBEButton*	m_pBEBtnEditSave;
	CBEButton*	m_pBEBtnSelectDeselect;
	CBEButton*	m_pBEBtnMemoryFilters;

private:
	BOOL m_bSelectDeselect;				// per il repobrowser
	BOOL m_bIsActiveMultipleSelection;	// per il repoexplorer

	// filtri in memoria sul DBT
	BOOL m_bFilterWoormReport;		// solo i report di Woorm
	BOOL m_bFilterAttachments;		// solo gli allegati

public:
	CArchivedDocumentsBodyEdit();

public:
	BDDMSRepository* GetDocument() { return (BDDMSRepository*)CBodyEdit::GetDocument(); }

public:
	virtual void EnableButtons		();
	virtual void Customize			();
	virtual BOOL OnDblClick			(UINT nFlags, CBodyEditRowSelected* pCurrentRow);
	virtual	BOOL OnCanLeaveCurrPos  (int nNewCurrRec);
	virtual void OnBeginMultipleSel	();
	virtual void OnEndMultipleSel	();
	virtual BOOL OnPostCreateClient	();
	virtual void OnDropFiles		(const CStringArray& arDroppedFiles);
	virtual BOOL OnSubFolderFound	();

private:
	void UpdateEditSaveToolbarBtn		();
	void UpdateCheckInCheckOutToolbarBtn();
	void UpdateSelectDeselectToolbarBtn	();

	CUIntArray* GetSelectedRowsIndexes	();

	//{{AFX_MSG(CArchivedDocumentsBodyEdit)
	afx_msg void OnShowDocument					();
	afx_msg void OnDeleteDocument				();
	afx_msg void OnSendDocument					();
	afx_msg void OnCheckInOutClicked			();
	afx_msg void OnUndoCheckOutClicked			();
	afx_msg void OnEditSaveClicked				();
	afx_msg void OnCopyInClicked				();
	afx_msg void OnSelectDeselectClicked		();
	afx_msg void OnUndoArchivedDocChanges		();	
	afx_msg void OnShowReportClicked			();
	afx_msg void OnShowAttachmentsClicked		();
	afx_msg void OnExtractDocumentsEnded		();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
//					class CDMSRepositoryView Definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CDMSRepositoryView : public CJsonFormView
{
	DECLARE_DYNCREATE(CDMSRepositoryView)

public:
	CDMSRepositoryView();
};