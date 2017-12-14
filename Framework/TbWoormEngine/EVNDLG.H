
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\mlistbox.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include <TbGenlib\parsctrl.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class WoormTable;
class EventsData;
class Parser;
class DisplayTables;
class RuleDataArray;
class TriggEventData;
class ProcedureData;
class EditorManager;

//===========================================================================
class TB_EXPORT CEventDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CEventDlg)
public:
	enum EventType
	{
		NONE_ACTION,
		CUSTOM_EVENT,
		REPORT_ACTION,
		NEWPAGE_ACTION,
		DISPLAY_TABLE_ACTION
	};

protected:
	EditorManager*	m_pEditorManager;
	EventsData*		m_pEventData;
	ProcedureData*	m_pProcData;
	WoormTable*		m_pSymTable;
	RuleDataArray*		m_pRuleData;
	DisplayTables*	m_pDispTable;
	EventType		m_CurEventType;
	BOOL			m_bStatusNew;
	int				m_nOtherEventsCurSel;

	CTBListBox		m_lbCustomEvents;
	CTBListBox		m_lbPredefEvents;
	CEqnEdit		m_edtEventActions;
	CTaskBuilderStatusBar	m_sbStatus;

	BOOL			m_bModified;

public:
	CEventDlg
		(
			EditorManager*	pEditorManager,
			EventsData*	 	pEventData,
			ProcedureData*	pProcedureData,
			WoormTable* pSymTable,
			RuleDataArray*		pRuleData,
			DisplayTables*	pdispTable
		);

protected:
	void	ShowPrototype		();
	void	ShowEvent			(LPCTSTR pszFileName);
	void	ShowError			(Parser&);
	void	InitStatusBar		();
	void	RecalcLayout		();
	void	UpdateEventList		();
	CString	GetCurrTableName	();

	void	MoveCustomEvent			(int nFrom, int nTo);
	void	ShowReportActions		();
	void	ShowNewPageActions		();
	void	ShowTableActions		();

	void	DelEvent				();
	BOOL	DoSave					(Parser&);
	BOOL	SaveCustomEventActions	(Parser&);
	BOOL	SaveReportActions		(Parser&);
	BOOL	SaveNewPageActions		(Parser&);
	BOOL	SaveTableActions		(Parser&);

protected:
	virtual	BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CEventDlg )
	afx_msg	void	OnCustomEventUp			();
	afx_msg	void	OnCustomEventDown		();
	afx_msg	void	OnAddCustomEvent		();
	afx_msg	void	OnDelCustomEvent		();
	afx_msg	void	OnShowCustomEventActions();
	afx_msg	void	OnCustomEventSetFocus	();
	afx_msg	void	OnShowPredefEventActions();
	afx_msg	void	OnPredefEventSetFocus	();
	afx_msg	void	OnCodeModified			();
	afx_msg	void	OnShowCurrentEvent		();
	afx_msg	void	OnClearActions			();
	afx_msg	void	OnCallProcedureDlg		();
	afx_msg	void	OnCallAddFieldDlg		();
	afx_msg void	OnSize (UINT nType, int cx, int cy);
	afx_msg	void	OnClickRadio		();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//==========================================================================//
class TB_EXPORT CSubTotalDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CSubTotalDlg)
public:
	EventsData*		m_pEventData;
	WoormTable*	m_pSymTable;
	TriggEventData*	m_pCurTriggEvent;
	Array			m_NewSubtotals;

	CTBListBox		m_lbCustomEvents;
	CMultiListBox	m_mlbColumnField;

public:
	CSubTotalDlg
		(
			EventsData*	 	pEventData,
			WoormTable* pSymTable
		);

public:
	void	ShowEventList			();
	void	ShowColumnField			();
	void	ShowSubtotalList		();

	void	AddSubtotalField		();
	void	AddDefaultAction		();
	BOOL	ExistInTriggEvent		(LPCTSTR);

protected:
	virtual BOOL	OnInitDialog		();
	virtual void	OnOK				();
	virtual void	OnCancel			();

	//{{AFX_MSG( CSubTotalDlg )
	afx_msg	void	AddSubtotalAction		();
	afx_msg	void	SelectEventAction		();
	afx_msg	void	SubTotalButtonAction	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
