
#pragma once

#include <TbGeneric\DataObj.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class WoormTable;
class Parser;
class QueryData;
class Expression;
class GroupByData;

//===========================================================================
class TB_EXPORT CGroupByDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CGroupByDlg)
public:
	enum DialogStatus	{ DLG_OPEN, DLG_CLOSE };
	enum EventType		{ TRIGG_EVENT, DEFAULT_ACTION, NEWPAGE_ACTION, DISPTABLE_ACTION };

protected:
	QueryData*		m_pQueryData;
	WoormTable*	m_pSymTable;
	GroupByData*	m_pGroupBy;

	CExpEdit		m_edtOrderBy;
	CExpEdit		m_edtGroupBy;
	CExpEdit		m_edtFilter;
	CEqnEdit		m_edtActions;
	CButton			m_btnDescending;

public:
	CGroupByDlg		(QueryData* pQueryData, WoormTable* pSymTable);
	~CGroupByDlg	();

protected:
	void	ShowGroupByActions	();
	BOOL 	GroupByOK			(GroupByData*);
	BOOL 	GroupByActionOK		(GroupByData*);

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( COrderByDlg )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
#include "endh.dex"
