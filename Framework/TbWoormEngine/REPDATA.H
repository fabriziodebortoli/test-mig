
#pragma once

#include "disptbl.h"
#include "qrydata.h"
#include "events.h"
#include "procdata.h"
#include "askdata.h"
#include "reptable.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==========================================================================//
class Parser;
class Unparser;
class RuleDataArray;
class EventsData;
class SqlConnection;
class ReportEngine;
class CXSDGenerator;

//==========================================================================//
class TB_EXPORT ReportData : public CObject
{
protected:
	WoormTable&		m_SymTable;
	CString			m_strRepName;
	QueryData		m_QueryData;
	EventsData		m_EventsData;
	ProcedureData	m_ProcedureData;
	QueryObjectData	m_QueryObjectData;
	AskRuleData		m_AskRuleData;

public:
	ReportData (WoormTable&);

public:
	BOOL			Parse			(Parser&, SqlConnection*, ReportEngine*);
	void			Unparse			(Unparser&);
	BOOL			GetSchema		(CXSDGenerator* pXMLSchema);

	WoormTable*			GeWoormTable		()	{ return &m_SymTable; }
	QueryData*			GetQueryData		()	{ return &m_QueryData; }
	RuleDataArray*		GetRuleData			()	{ return m_QueryData.GetRuleData(); }
	EventsData*			GetEventsData		()	{ return &m_EventsData; }
	ProcedureData*		GetProcedureData	()	{ return &m_ProcedureData; }
	QueryObjectData*	GetQueryObjectData	()	{ return &m_QueryObjectData; }
	AskRuleData*		GetAskRuleData		()	{ return &m_AskRuleData; }
	BOOL				ExecAskRules		(BOOL bInitOnly = FALSE, CWnd* pParent = NULL);

	BOOL			CanDeleteField	(LPCTSTR) const;
	void			DeleteField		(LPCTSTR);
	BOOL			DeleteTable		(WORD nID);
	void			RenameField		(LPCTSTR, LPCTSTR);

	BOOL			CanConvertFieldToInput (LPCTSTR pszFieldName, CString& sLog) const;
};
#include "endh.dex"
