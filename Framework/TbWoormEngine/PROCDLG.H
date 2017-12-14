
#pragma once

#include <TbGenlib\parsctrl.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class WoormTable;
class EventsData;
class ObjectsData;
class ProcedureData;
class Parser;
class RuleDataArray;
class EditorManager;

//============================================================================
class TB_EXPORT CProcedureDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CProcedureDlg)
protected:
	EditorManager*	m_pEditorManager;
	EventsData*		m_pEventsData;
	ObjectsData*	m_pProcData;
	WoormTable*	m_pSymTable;

	BOOL		m_bCanDelRen;
    BOOL		m_bEmptyEditFlag;
    BOOL		m_bErrorFlag;
    BOOL		m_bModifyFlag;
	BOOL		m_bEditProcIsModified;

	CString		m_strEmptyTemplate;
	CString		m_strLastProcEdit;

	//CButton		m_btnRename;
	//CButton		m_btnDelete;
	//CButton		m_btnConfirm;
	//CButton		m_btnCancel;

	CComboBoxExt m_cbProcs;
	CEqnEdit	m_edtProc;

	CTaskBuilderStatusBar	m_sbStatus;
        
public:
	CString		m_sTitle;

	CProcedureDlg
		(
			EditorManager*	pEditorManager,
			EventsData*		pEventsData,
			ObjectsData*	pProcData,
			WoormTable* pSymTable,
			RuleDataArray*		pRuleData,
			BOOL			bCanDelRen = TRUE
		);

protected:
	// generic procedures
	BOOL 	OkProcedure			();
	void	EnableBtnDelRen		(BOOL bEnableFlag);

	// bStatus bar procedures
	BOOL	InitStatusBar		();
	void	ShowActionStatusBar	(LPCTSTR pszProcName, const CString& strAction);
	void	ClearErrorStatusBar	();
	void	ShowError			(Parser& lex);

	void 	ShowProcedure		(LPCTSTR pszProcName);

	virtual ObjItem* CreateItem();

protected:
	virtual BOOL	OnInitDialog	();
	virtual void	RecalcLayout	();
	virtual	void	OnOK			();
	virtual	void	OnCancel		();

	//{{AFX_MSG( CProcedureDlg )
	afx_msg	void	BtnCancProcedure	();
	afx_msg	void	BtnConfProcedure	();
	afx_msg	void	BtnDelProcedure		();
	afx_msg	void	BtnRenProcedure		();
	afx_msg	void	ChangeProcEdit		();
	afx_msg	void	SetFocusProcEdit	();
	afx_msg	void	SelChangeProcList	();
	afx_msg	void	EditChangeProcList	();
	afx_msg	void	DblClkProcList		();
	afx_msg	void	OnSize (UINT nType, int cx, int cy);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CQueriesDlg : public CProcedureDlg
{
public:
	CQueriesDlg
		(
			EditorManager*	pEditorManager,
			EventsData*		pEventsData,
			ObjectsData*	pProcData,
			WoormTable* pSymTable,
			RuleDataArray*		pRuleData,
			BOOL			bCanDelRen = TRUE
		);

protected:
	virtual ObjItem* CreateItem();
};

//============================================================================
class TB_EXPORT CProcedureEventDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CProcedureEventDlg)
protected:
	EventsData*	m_pEventsData;
	CString		m_strCurrProcName;
	
	BOOL		m_bIsEventNewPage;
	BOOL		m_bIsEventReport;

	CButton		m_btnReport;
	CButton		m_btnNewPage;

	CTBListBox	m_lbTriggEvents;
	CTBListBox	m_lbTableActions;

public:
	CProcedureEventDlg
		(
			int			nDlgIndex,
			EventsData*	pEventsData,
			LPCTSTR		pszProcName
		);

protected:
	virtual	void	EnableAllItemDlg		(BOOL bEnable = TRUE);

protected:
			void 	SetStdEventCheckBox		(BOOL bStatus = TRUE);

private:
	void	UpdateTriggEventsList	();
	void	UpdateTableActionsList	();
	void	UpdatePredefineEvent	();

protected:
	virtual BOOL	OnInitDialog 	();

	//{{AFX_MSG( CProcedureEventDlg )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CShowProcEventsDlg : public CProcedureEventDlg
{
public:
	CShowProcEventsDlg
		(
			EventsData*	pEventsData,
			LPCTSTR		pszProcName
		);

protected:
	virtual BOOL	OnInitDialog ();
	virtual	void	OnOK ();

	//{{AFX_MSG( CShowProcEventsDlg )
	afx_msg	void	OnStdEventClicked();
	//}}AFX_MSG             

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT ModifyProcedureDlg : public CProcedureEventDlg
{
protected:
	ObjectsData*	m_pProcData;
	CButton			m_btnSelectAll;
	CButton			m_btnDeselectAll;
	CButton			m_btnSelAllTriggEvn;
	CButton			m_btnDeselAllTriggEvn;
	CButton			m_btnSelAllTableAct;
	CButton			m_btnDeselAllTableAct;
        
public:
	ModifyProcedureDlg
		(
			int				nDlgIndex,
			ObjectsData*	pProcData,
			EventsData*		pEventsData,
			LPCTSTR			pszProcName
		);

protected:
    virtual	void	EnableAllItemDlg(BOOL bEnable = TRUE);
    
protected:
	void 	SelectAllList			(CListBox& listBox, BOOL bSelectAll = TRUE);

	BOOL	GetItemSelect			(CStringArray& itemArray, CListBox& listBox);

	BOOL	GetTriggEventsSelect	(CStringArray& aTriggEventArray);
	BOOL	GetTableActionsSelect	(CStringArray& aTableActionsArray);
    
    BOOL	CheckBtnReport			()		{ return m_btnReport.GetCheck(); }
    BOOL	CheckBtnNewPage			()		{ return m_btnNewPage.GetCheck(); }
    
protected:
	virtual BOOL	OnInitDialog 	();

	//{{AFX_MSG( CProcedureEventDlg )
	afx_msg	void	SelAllTriggEvn			();
	afx_msg	void	DeselAllTriggEvn		();
	afx_msg	void	DblClkTriggEvn			();
	afx_msg	void	SelAllTableAct			();
	afx_msg	void	DeselAllTableAct		();
	afx_msg	void	DblClkTableAct			();
	afx_msg	void	SelectAll				();
	afx_msg	void	DeselectAll				();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT RenameProcedureDlg : public ModifyProcedureDlg
{
protected:
	BOOL			m_bItemsEnabled;
	CString*		m_pNewName;
	CIdentifierEdit	m_edtNewName;

public:
	RenameProcedureDlg
		(
			EventsData*		pEventsData,
			ObjectsData*	pProcData,
			LPCTSTR			oldName,
			CString*		pStrNewName
		);
            
protected:
	virtual BOOL	OnInitDialog	();
	virtual	void	OnOK			();

	//{{AFX_MSG( RenameProcedureDlg )
	afx_msg	void	ModifyNewName	();
	//}}AFX_MSG             

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CDeleteProcedureDlg : public ModifyProcedureDlg
{
public:
	CDeleteProcedureDlg
		(
			EventsData*		pEventsData,
			ObjectsData*	pProcData,
			LPCTSTR			pszProcName
		);
	
protected:
	virtual BOOL	OnInitDialog ();
	virtual	void	OnOK ();

	//{{AFX_MSG( RenameProcedureDlg )
	//}}AFX_MSG             

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
