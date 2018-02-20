//===========================================================================
// module name  : ItemsListTools.H
// author       :
// description  : Dialog per la selezione degli Items
// Copyright (c) MicroArea S.p.A. All rights reserved
//===========================================================================

#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGenlib\ParsObj.H>
#include <TbGenlib\ParsBtn.H>
#include <TbGenlib\ParsCbx.H>
#include <TbGenlib\ParsEdt.H>
#include <TbGes\HotLink.h>

//includere alla fine degli include del .h
#include "beginh.dex"

class CItemsListEdit;
class CAbstractFormView;

//////////////////////////////////////////////////////////////////////////////
TB_EXPORT BOOL ParseMultipleItems(const DataStr& strItems, CStringArray&, BOOL bNoRemove = FALSE);
//===========================================================================
TB_EXPORT void GetMultipleItemsORClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter,
		const DataType& = DataType::String
	);

//===========================================================================
TB_EXPORT void GetMultipleItemsLIKEClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter
	);

//===========================================================================
TB_EXPORT void GetMultipleArrayItemsLIKEClause
	(
		const CStringArray& arrayItems,
		const CString& strColName,
		CString& strFilter
	);

//===========================================================================
TB_EXPORT void GetCheckItemClause
	(
		const DataStr& strItems,
		const CString& strColName,
		CString& strFilter
	);

//////////////////////////////////////////////////////////////////////////////
//					class CItemsListDlg definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CItemsListDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CItemsListDlg)
// Attenzione: devono essere dichiarati PRIMA i dataxxxx e DOPO i CxxxxEdit !!!
public:
	DataObjArray*	m_pItemsList;
	CString			m_RadioLabel1;
	CString			m_RadioLabel2;
	DataBool*		m_pRadio1;
	DataBool*		m_pRadio2;

protected:
	CResizableListBox	m_LBItems;
	CResizableListBox	m_LBItemsSelected;
	CBoolButton			m_Radio1;
	CBoolButton			m_Radio2;
	HotKeyLink*			m_pHKL;
	CBaseDocument*		m_pDocument;
	BOOL				m_bEnumMode; // La dialog prende in input un enumerativo
	DWORD				m_dwValue;

public:
	CItemsListDlg(HotKeyLink*, DataObjArray* pItemsList, CBaseDocument*);
	CItemsListDlg(HotKeyLink*, DataObjArray* pItemsList, CBaseDocument*, BOOL bEnumMode, DWORD dwValue);
	~CItemsListDlg();

protected:
	virtual BOOL	OnInitDialog	();
			BOOL	QueryLBItems	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

protected:
	//{{AFX_MSG(CItemsListDlg)  
	afx_msg			void OnLoadItem		();
	afx_msg			void OnDeleteItem	();
	afx_msg			void OnQueryChanged	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//					class CItemsListDlg definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CItemsStringDlg : public CItemsListDlg
{
public:
	DataStrArray	m_StringArray;
	CString			m_strCommaSeparated;

	CItemsStringDlg(HotKeyLink*, CBaseDocument*);
	CItemsStringDlg(HotKeyLink*, CBaseDocument*, BOOL bEnumMode, DWORD dwValue);

	void SetCommaSeparetdString (const CString&);

protected:
	virtual	void	OnOK			();
};

//////////////////////////////////////////////////////////////////////////////
//					class CItemsMSCombo definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CItemsMS : public CMSStrButton
{
	DECLARE_DYNCREATE (CItemsMS) 

public:
	CItemsListEdit* m_pCItemsListEdit;

public:
	CItemsMS();
	~CItemsMS();

protected:
	virtual	BOOL	UpdateCtrlData		(BOOL bEmitError, BOOL bSendMessage = FALSE);
	virtual	void	OnUpdateCtrlStatus	(int = -1);
	virtual	BOOL	OnCommand(WPARAM wParam, LPARAM lParam);

	//{{AFX_MSG(CItemsCombo)
	afx_msg void	OnKeyDown		(UINT nChar, UINT nRepCnt, UINT nFlags);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
}; 

//////////////////////////////////////////////////////////////////////////////
//					class CItemsEdit definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CItemsEdit: public CStrEdit
{
	DECLARE_DYNCREATE (CItemsEdit) 

public:
	CItemsListEdit* m_pCItemsListEdit;

public:
	CItemsEdit();
	~CItemsEdit();
	virtual void	ModifiedCtrlData	();

protected:
	virtual	BOOL	UpdateCtrlData		(BOOL bEmitError, BOOL bSendMessage = FALSE);
	virtual	void	OnUpdateCtrlStatus	(int = -1);

	//{{AFX_MSG(CItemsEdit)
	afx_msg void	OnKeyDown		(UINT nChar, UINT nRepCnt, UINT nFlags);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};


//////////////////////////////////////////////////////////////////////////////
//           class CItemsListEdit definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CItemsListEdit: public CStrEdit
{ 
	DECLARE_DYNCREATE (CItemsListEdit) 

public:
	CItemsListEdit();
	CItemsListEdit(DataStr* pData);
	~CItemsListEdit();

public:
	void	UpdateItemsList		();
	void	UpdateItemsToOpen	();

	void	Attach
			(
				DataObjArray*	pItemsList,
				HotKeyLink*		pHKL
			);

	void	CreateItemsMSButton
		(
		const	CString&		sName,
		CWnd*			pView,
		UINT			nIDC,
		CString			sNSImage = _T("")
	);

	void	CreateItemsMSCombo	
			(
				const	CString&		sName, 
						CWnd*			pView, 
						UINT			nIDC
			);

	HotKeyLinkObj*	CreateItemsCombo(const CString& sName, CWnd* pView, UINT nIDC, HotKeyLink* pHKL);

	// porting diretto da 3_x (uscita della 3_11?)
	HotKeyLinkObj*	CreateItemsEdit (const CString& sName, CWnd* pView, UINT nIDC, HotKeyLink* pHKL);
	int		DoModal();
	void	SetSeparator		(char separator); // Imposta il carattere per separare gli item.
	void	SetShowDescriptions(BOOL bSet = TRUE) { m_bShowDescriptions = bSet; }

protected:
	virtual BOOL	OnShowingPopupMenu	(CMenu&);
	virtual	void	OnUpdateCtrlStatus	(int = -1);
	virtual BOOL	PreCreateWindow		(CREATESTRUCT& cs);
	virtual	BOOL	DoOnChar			(UINT nChar);

	//{{AFX_MSG(CItemsListEdit)
	afx_msg void	OnDeleteItem	();
	afx_msg void	OnKeyDown		(UINT nChar, UINT nRepCnt, UINT nFlags);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

private:
	void UpdateInputString();
	void RestoreSubtract(CString pStrOld, CString pStrToInsert, DataStr* pDataStr);

private:
	BOOL			m_bOwnEdit;
	CItemsEdit*		m_pCItemsEdit;
	DataStr			m_Item;
	CItemsMS*		m_pCItemsMS;
	DataStr			m_InputString;
	BOOL			m_bOwnList;
	DataObjArray*	m_pItemsList;
	HotKeyLink*		m_pHKL;
	char			m_Separator;
	BOOL			m_bOwnCombo;
	BOOL			m_bShowDescriptions;
}; 

#include "endh.dex"
