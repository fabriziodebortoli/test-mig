
#pragma once

#include "repdata.h"
#include "rpsymtbl.h"
#include "repfield.h"
#include "reptable.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CProgramDlg;
class QueryData;
class RuleDataArray;
class EventsData;

class WoormTable;
class DispTable;

class AskRuleData;
class ProcedureData;
class Parser;
class Unparser;
class SqlConnection;
class ReportEngine;
class CXSDGenerator;

//===========================================================================
class TB_EXPORT GlobalId
{
private:
	WORD	m_wLastID;

public:
	GlobalId();

public:
	WORD	GetNextId	();
	WORD	GetId		();
	void	SetLastId	(WORD id);
	void	DecId		();
};

//==========================================================================//
class TB_EXPORT ProgramData : public CObject
{
	friend class	CProgramDlg;
	
	DECLARE_DYNAMIC (ProgramData)

public:
	int				m_nCounter;

protected:
	GlobalId		m_GlobalId;
	WoormTable		m_ReportSymTable;
	ReportData		m_ReportData;
	CString			m_strHelpFile;
	
public:
	ProgramData(CWoormDoc*);
	~ProgramData();

public:
	BOOL			Parse				(Parser&, SqlConnection*, ReportEngine*);
	BOOL			Unparse				(Unparser&);
	BOOL			GetSchema			(CXSDGenerator* pSchema);

	ReportData*			GetReportData		()	{ return &m_ReportData;					}
	QueryData*			GetQueryData		()	{ return m_ReportData.GetQueryData();	}
	RuleDataArray*		GetRuleData			()	{ return m_ReportData.GetRuleData();	}
	WoormTable*			GetSymTable			()	{ return &m_ReportSymTable;				}
	EventsData*			GetEventsData		()	{ return m_ReportData.GetEventsData();	}
	ProcedureData*		GetProcedureData	()	{ return m_ReportData.GetProcedureData();}
	AskRuleData*		GetAskRuleData		()	{ return m_ReportData.GetAskRuleData(); }
	QueryObjectData*	GetQueryObjectData	()	{ return m_ReportData.GetQueryObjectData();}
	GlobalId*			GetGlobalId			()	{ return &m_GlobalId;					}

	BOOL			ExecAskRules		(BOOL bInitOnly = FALSE, CWnd* pParent = NULL)	{ return m_ReportData.ExecAskRules(bInitOnly, pParent); }

	const CString&	GetHelpFile			()	{ return m_strHelpFile;					}
	void			SetHelpFile			(const CString& strHelpFile)	{ m_strHelpFile = strHelpFile; }

	BOOL			CanDeleteField		(LPCTSTR pszFieldName, CString& sLog, BOOL* onlyUI = NULL) const;
	void			DeleteField			(LPCTSTR pszFieldName);
	BOOL			DeleteTable			(WORD nID);
	void			RenameField			(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);

	BOOL			CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const;

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
#endif // _DEBUG
};
#include "endh.dex"
