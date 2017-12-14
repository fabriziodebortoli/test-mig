
#pragma once

#include <TbWoormEngine\edtcmm.h>

class DlgInfoItem;
class DlgInfoArray;
class CTileDlg;
class CTileManager;
class CTilePanel;
class CTilePanelTab;

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================

/////////////////////////////////////////////////////////////////////////////
//							COslDlg
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT COslDlg : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(COslDlg)

protected:
	HTREEITEM		m_hCurrentItem;
	LPARAM			m_lParamCurrentItem;
	BOOL			m_bFilling;

	CTBTreeCtrl		m_ctrlTree;
	CBCGPEdit		m_ctrlGuid;
	CImageList		m_imaSmall;
	CLocalizablePropertySheet* m_pSheet;

	CButton			m_ctrlInsert;

protected:
	virtual BOOL OnInitDialog	();

// Construction
public:
	COslDlg (CLocalizablePropertySheet* pSheet, UINT idd);  

// Implementation
protected:
	afx_msg void OnSelchangedTree	(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSelchangingTree	(NMHDR* pNMHDR, LRESULT* pResult);

	afx_msg void OnClickInsert	();
	afx_msg void OnClickRecursiveInsert	();

	void DoClickInsert(BOOL bRecursive);
	
	BOOL DoInsert (HTREEITEM hSubRootItem, BOOL bRecursive, int& nInserted, BOOL bCheckParent);
	BOOL DoInsert (COslTreeItem* pItemInfo, BOOL bRecursive, int& nInserted, BOOL bCheckParent);
	BOOL DoInsert (CInfoOSL* pInfoOSL, int& nInserted, BOOL bCheckParent, CString sNickName);
	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------

class TB_EXPORT COslDlgDoc : public COslDlg
{
	DECLARE_DYNAMIC(COslDlgDoc)
public:
	COslDlgDoc (CLocalizablePropertySheet* pSheet, CAbstractFormDoc* pDoc);  

protected:
	CAbstractFormDoc*	m_pDoc;
	Array				m_arInfoTreeItems;

	void EnumTabbers		(COslTreeItem* pParent, HTREEITEM htmLevParent, TabManagers* pTabbers);
	void EnumTab			(COslTreeItem* pParent, HTREEITEM htmLevParent, DlgInfoItem* pTabDlgInf, CTabDialog* pTab, DlgInfoArray* parDlgInf, int nTab);
	void EnumBodyEdit		(COslTreeItem* pParent, HTREEITEM htmLevParent, Array* pBEInfo);
	void EnumControls		(COslTreeItem* pParent, HTREEITEM htmLevParent, ControlLinks* pControlLinks);
	void EnumTileGroup		(COslTreeItem* pParent, HTREEITEM htmLevParent, CBaseTileGroup*);
	void EnumTile			(COslTreeItem* pParent, HTREEITEM htmLevParent, CBaseTileDialog*);
	void EnumTilePanel		(COslTreeItem* pParent, HTREEITEM htmLevParent, CTilePanel*);
	void EnumTilePanelTab	(COslTreeItem* pParent, HTREEITEM htmLevParent, CTilePanelTab* pTab);
	void EnumView			(COslTreeItem* pParent, HTREEITEM htmLevParent, CAbstractFormView* pView, BOOL isMaster = TRUE);
	void EnumDialog			(COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedDialog* pDialog);
	void EnumElements		(COslTreeItem* pParent, HTREEITEM htmLevParent, LayoutElement* pEment);
	

protected:
	BOOL	m_bFirstMaster;

	void EnumTabbedToolbarElements(COslTreeItem* pParent, HTREEITEM htmLevDoc);
	void EnumCaptionBarObjects(COslTreeItem* pParent, HTREEITEM htmLevParent);
	void EnumRibbonBarElements(COslTreeItem* pParent, HTREEITEM htmLevParent);
	void EnumToolbarElements(COslTreeItem* pParent, HTREEITEM htmLevDoc, CTBToolBar* m_pToolBar);

	virtual BOOL OnInitDialog	();
	virtual void FillAllTree ();

private:
	void EnumPropertiesGrid(COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedForm* pParsedForm);
	void EnumBodyEdits	(COslTreeItem* pParent, HTREEITEM htmLevParent, CParsedForm* pParsedForm);
	void EnumBodyEdit	(COslTreeItem* pParent, HTREEITEM htmLevParent, CBodyEdit* pBodyEdit);
	void RibbonBarButtonsElements(CArray<CInfoOSLButton*,CInfoOSLButton*>* pArButtonOSLInfo, COslTreeItem* pParent, HTREEITEM htree);


};
//----------------------------------------------------------------------

//======================================================================
#include "endh.dex"
