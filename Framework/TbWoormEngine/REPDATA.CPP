
#include "stdafx.h"

#include <TBXmlCore\XMLSchema.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbParser\Parser.h>

#include <TbGenlib\FunProto.h>

#include "ActionsRepEngin.h"
#include "inputmng.h"
#include "rpsymtbl.h"
#include "repdata.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// class ReportData
//============================================================================
//----------------------------------------------------------------------------
ReportData::ReportData(WoormTable& aSymTable)
	:
	m_SymTable		(aSymTable),
	m_QueryData		(aSymTable),
	m_EventsData	(aSymTable),
	m_ProcedureData	(aSymTable),
	m_QueryObjectData (aSymTable),
	m_AskRuleData	(aSymTable)
{
	//aSymTable.GetDocument() == NULL when called by export profile (CProfileSelectionPage)
}

//----------------------------------------------------------------------------
BOOL ReportData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	return 
		m_EventsData.CanConvertFieldToInput(pszFieldName, sLog) &&
		m_QueryData.CanConvertFieldToInput(pszFieldName, sLog);
}

//----------------------------------------------------------------------------
BOOL ReportData::ExecAskRules(BOOL bInitOnly /*= FALSE*/, CWnd* pParent /*= NULL*/)
{
	AskDialogInputMng aAskDialogInputMng
		(
			GetAskRuleData(),
			&m_SymTable,
			m_SymTable.GetDocument()
		);

	//eseguo le regole di richiesta in modo da avere i parametri attuali
	return aAskDialogInputMng.ExecAskRules(pParent, bInitOnly);
}

//----------------------------------------------------------------------------
BOOL ReportData::CanDeleteField(LPCTSTR pszFieldName) const
{
	return
		m_QueryData.		CanDeleteField(pszFieldName)	&&
		m_EventsData.		CanDeleteField(pszFieldName)	&&
		m_ProcedureData.	CanDeleteField(pszFieldName)	&&
		m_QueryObjectData.	CanDeleteField(pszFieldName)	&&
		m_AskRuleData.		CanDeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ReportData::DeleteField(LPCTSTR pszFieldName)
{                   
	m_QueryData.		DeleteField(pszFieldName);
	m_EventsData.		DeleteField(pszFieldName);
	m_ProcedureData.	DeleteField(pszFieldName);
	m_AskRuleData.		DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ReportData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	m_QueryData.RenameField(pszOldFieldName, pszNewFieldName);
	//TODO m_EventsData.RenameField(pszOldFieldName, pszNewFieldName);
	//TODO m_ProcedureData.RenameField(pszOldFieldName, pszNewFieldName);
	m_AskRuleData.RenameField(pszOldFieldName, pszNewFieldName);
}

//----------------------------------------------------------------------------
BOOL ReportData::DeleteTable(WORD nId)
{
	if (!m_SymTable.GetDisplayTables())
		return FALSE;

	CString strDispTableName = m_SymTable.GetDisplayTables()->GetName(nId);
	if (strDispTableName.IsEmpty())
		return FALSE;

	// ora si puo` cancellare
	if (!m_SymTable.GetDisplayTables()->DelTable(nId))
		return FALSE;

	// vengono cancellati i riferimenti ad essa
	m_EventsData.	DeleteTable	(strDispTableName);
	m_ProcedureData.DeleteTable	(strDispTableName);

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL ReportData::Parse(Parser& lex, SqlConnection* pSqlConnection, ReportEngine* pReportEngine)
{
	BOOL bOk = lex.ParseBegin() && lex.ParseID	(m_strRepName);
	bOk = bOk &&	m_SymTable.		Parse	(lex);
	bOk = bOk && 	m_QueryData.	Parse	(lex, pSqlConnection);
	bOk = bOk && 	m_EventsData.	Parse	(lex);
	bOk = bOk && 	m_ProcedureData.Parse	(lex);
	bOk = bOk && 	m_QueryObjectData.Parse (lex);
	bOk = bOk && 	m_AskRuleData.	Parse	(lex, pReportEngine);
	bOk = bOk && 	lex.ParseEnd();

	//Bind QueryRules rule with their QueryObjects
	for (int i = 0; i < m_QueryData.GetNumRules(); i++)
	{
		RuleDataObj* pR = m_QueryData.GetRule(i);
		if (pR->IsARule() != RULE_NAMED_QUERY) 
			continue;
		QueryRuleData* pQ = dynamic_cast<QueryRuleData*>(pR);
		ASSERT_VALID(pQ);
		if (!pQ) continue;

		QueryObjItem* pI = m_QueryObjectData.Get(pQ->GetQueryName());
		ASSERT_VALID(pI);
		if (pI)
			pQ->SetQueryItem(pI);
	}

	CString sErr;
	if (m_QueryData.m_pGroupBy)
	{
		if (!m_QueryData.m_pGroupBy->CheckRuleItems(sErr))
		{
			lex.SetError(sErr);
			return FALSE;
		}
	}

	for (int i = 0; i < m_EventsData.m_TriggEvents.GetSize(); i++)
	{
		TriggEventData* pEv = dynamic_cast<TriggEventData*>(m_EventsData.m_TriggEvents.GetAt(i));
		if (pEv)
		{
			if (!pEv->CheckRuleItems(sErr))
			{
				lex.SetError(sErr);
				return FALSE;
			}
		}
	}

	return bOk;
}

//----------------------------------------------------------------------------
void ReportData::Unparse(Unparser& oFile)
{
	oFile.UnparseBegin(FALSE);
	
	if (m_strRepName.IsEmpty())
		m_strRepName = "NoName";

	oFile.UnparseID		(m_strRepName,	TRUE);

	m_SymTable.			Unparse(oFile);
	m_QueryData.		Unparse(oFile);
	m_EventsData.		Unparse(oFile);
	m_ProcedureData.	Unparse(oFile);
	m_QueryObjectData.	Unparse(oFile);
	m_AskRuleData.		Unparse(oFile);

	oFile.UnparseEnd(TRUE);
}

//----------------------------------------------------------------------------
BOOL ReportData::GetSchema(CXSDGenerator* pXMLSchema)
{
	return m_AskRuleData.GetSchema(pXMLSchema);
}
