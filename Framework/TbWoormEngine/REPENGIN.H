
#pragma once

#include <TbGeneric\RdeProtocol.h>
#include "qryengin.h"
#include "reptable.h"
#include "repfield.h"
#include "askdata.h"
#include "ActionsRepEngin.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class WoormField;
class ReportEngine;
class RepEngine;
class CTBContext;
class WoormTable;
class CStaticFieldArrayCache;

//============================================================================
class TB_EXPORT EventActions : public CObject
{
public:
	EventActions (RepEngine&);
	~EventActions();

public:
	Block&		GetBeforeActions	() { return *m_pBeforeBlock; }
	Block&		GetAfterActions		() { return *m_pAfterBlock; }

	BOOL		ParseEventActions	(Parser&, BOOL = TRUE);

public:
	RepEngine&	m_OwnRepEngine;

	Block*       m_pBeforeBlock;
	Block*       m_pAfterBlock;
};

class TB_EXPORT FormFeedEvents : public EventActions
{
public:
	FormFeedEvents(RepEngine&);
};

class TB_EXPORT FillTableEvents : public EventActions
{
public:
	FillTableEvents(RepEngine&, const CString& sTableName);
};

//============================================================================
class TB_EXPORT ReportEvents : public EventActions
{
public:
	ReportEvents(RepEngine&);
	~ReportEvents();
public:
	Block&	GetAlwaysActions() { return *m_pAlwaysBlock; }
	Block&	GetFinalizeActions() { return *m_pFinalizeBlock; }

	BOOL	Parse	(Parser&);

	Block*	m_pAlwaysBlock;
	Block*	m_pFinalizeBlock;
};

//============================================================================
class TB_EXPORT TriggeredEvent : public EventActions
{
public:
	CString	m_sName;

	TriggeredEvent (RepEngine&, const CString& sName);

public:
	BOOL	Check			(BOOL bQry = TRUE);
	BOOL	DoBeforeActions	();
	BOOL	DoAfterActions	();

	BOOL	Parse			(Parser&);
	BOOL	CheckRuleItems  (CString& sErr);

private:
	BOOL	EvalWhen		(BOOL bQry);

private:
	BOOL		m_bOccurred;
	CObArray	m_BreakList;		// array of WoormField* to check break
	Token		m_BoolOperator;		// T_AND | T_OR
	Expression	m_WhenExpr;
	CObArray	m_BreakingFieldsToCheck;
};

//============================================================================
class TB_EXPORT COutParamBag
{
public:
	CArray <WoormField*>			m_arOutField;
	CArray <WoormField*>			m_arInField;
	CArray <int>					m_arIndexOutParam;
	CFunctionDescription*			m_pFD;
	BOOL							m_bForceUpdateOutputParameters;
	CStaticFieldArrayCache*			m_parStaticFields;
	WoormTable*						m_pSymTable;
	CStringArray					m_arFieldsInitialized;

protected:
	BOOL& m_bUpdateOutputParameters;

public:
	COutParamBag
		(
			CFunctionDescription* pfd, 
			BOOL& bUpdateOutputParameters, 
			CStaticFieldArrayCache* parStaticFields,
			WoormTable* pSymTable
		) 
		: 
		m_pFD							(pfd),
		m_bUpdateOutputParameters		(bUpdateOutputParameters),
		m_bForceUpdateOutputParameters	(FALSE),
		m_parStaticFields				(parStaticFields),
		m_pSymTable						(pSymTable)
		{}

	void Update()
	{
		//Ri-valorizzo parametri di out
		if (m_bUpdateOutputParameters || m_bForceUpdateOutputParameters)
			for (int i = 0; i < m_arIndexOutParam.GetSize(); i++)
			{
				DataObj* pOutVal = m_arOutField[i]->GetRepData();
				ASSERT_VALID(pOutVal);

				m_pFD->SetParamValue(m_arIndexOutParam[i], *pOutVal);
			}
	}
};

//============================================================================
class TB_EXPORT RepEngine : public QueryEngine
{
	friend Block;
	friend Procedure;
	friend ReportEvents;
	friend FormFeedEvents;
	friend FormFeedAction;
	friend DisplayFieldsAction;
	friend DisplayTableAction;
	friend CRSTreeCtrl;

	DECLARE_DYNAMIC(RepEngine);

protected:
	WoormTable* m_pViewST;
public:
	RepEngine	(ReportEngine*);
	~RepEngine	();

	CArray<CBreakpoint*> m_arBreakpoints;
	CStringArray m_arTraceActions;

public:
	enum RepEngineStatus { RE_INIT, RE_BEFORE, RE_FIRST_ROW, RE_BODY, RE_LAST_ROW/*, RE_AFTER*/ };
	RepEngineStatus	GetEngineStatus	() const { return m_EngineStatus; }

	CWoormDoc*	GetCallerDoc() { return m_pOwnerReport ? m_pOwnerReport->GetCallerDoc() : NULL; }

	BOOL			ExecReport			(RDEManager*, Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL, CStaticFieldArrayCache* parStaticFields = NULL, BOOL keepValues=FALSE,  CFunctionDescription* = NULL, WoormTable* = NULL);
	BOOL			RunEngine			();
	void			ExecEngine			();
	RDEManager*		GetOutChannel		() const { return m_pOutChannel; }
	EventActions*	GetOnFFActions		() const { return m_pOnFormFeedEvents; }

	BOOL			Parse				(Parser&, CTBContext*);
	ReportEngine*	GetOwnRepEngine		()	{ return m_pOwnerReport; }

	BOOL			EndExec				(COutParamBag*, EngineScheduler::MessageID msg = EngineScheduler::EMPTY_MESSAGE, const CString& sErr = _T(""), LPCTSTR psz = NULL);
	BOOL			FinalizeExec		(COutParamBag*);

	//DataBool		ShowAskDialog		(DataStr strDialogName);
	DataBool		ShowAllAskDialogs	(const CStringArray& allDialogs);

private:
	BOOL	QueryProcessing		();

	BOOL	ParseAskingRules		(Parser&);
	BOOL	ParseStdActions			(Parser&);
	BOOL	ParseTriggeredEvents	(Parser&);
	BOOL	ParseEvents				(Parser&);
	BOOL	ParseProcedures			(Parser&);
	BOOL	ParseQueries			(Parser&, CTBContext*);
	BOOL	MakeAutoDisplayActions	(Parser&);

	void	UpdateViewSymbolTable	(WoormTable* pST);

public:
	AskRuleData& GetAskingRules()  { return	m_AskingRules; }
	virtual BOOL	ResolveQueries(CString& sErr);
private:
	ReportEngine*		m_pOwnerReport;
	RDEManager*			m_pOutChannel;
	FormFeedEvents*		m_pOnFormFeedEvents;
	ReportEvents*		m_pReportEvents;
	Array				m_TriggeredEvents;
	AskRuleData			m_AskingRules;
	RepEngineStatus		m_EngineStatus;
	FormFeedAction*		m_pAutoFormFeed;
	Array*				m_parOrderOnThisColumns;
	CString*			m_pstrAuxWhereClause;

	int		m_nItemStart;
	BOOL	m_bDispActionFound;
	BOOL	m_bRepDispActionFound;
	BOOL	m_bOnFormFeedAction;
	BOOL	m_bOnTableAction;

	volatile short	m_AsyncEngineRunningStatus;

};

//=============================================================================
#include "endh.dex"
