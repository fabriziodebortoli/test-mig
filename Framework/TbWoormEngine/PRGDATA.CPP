
#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\PathFinder.h>
#include <TbParser\Parser.h>
#include <TbGenlib\baseapp.h>

#include "ActionsRepEngin.h"
#include "disptbl.h"
#include "qrydata.h"
#include "ruledata.h"
#include "prgdata.h"
#include "report.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//	class GlobalId
//=============================================================================
//-----------------------------------------------------------------------------
GlobalId::GlobalId() : m_wLastID(0)	{}

//-----------------------------------------------------------------------------
WORD GlobalId::GetNextId()
{
	m_wLastID++;
    return m_wLastID;
}

//-----------------------------------------------------------------------------
void GlobalId::DecId()
{
	if (m_wLastID > 0)	
		m_wLastID--;
}

//-----------------------------------------------------------------------------
WORD GlobalId::GetId()
{
	return m_wLastID;
}

//-----------------------------------------------------------------------------
void GlobalId::SetLastId(WORD nId)
{
	if (nId > m_wLastID) m_wLastID = nId;
}

//============================================================================
// ProgramData
//============================================================================
IMPLEMENT_DYNAMIC (ProgramData, CObject)

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void ProgramData::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	//dc << " ProgramData\n\tOrderBy: " << m_strOrderBy << ";\n\tWClause: " <<  m_strFilter << ";\n";
	CObject::Dump(dc);
}
#endif // _DEBUG

//----------------------------------------------------------------------------
ProgramData::ProgramData(CWoormDoc* pDoc)
	:
	m_nCounter			(0),
	m_ReportSymTable	(WoormTable::ReportSymTable_EDITOR, pDoc, &m_GlobalId),
	m_ReportData		(m_ReportSymTable)
{}

//----------------------------------------------------------------------------
ProgramData::~ProgramData()
{
	ASSERT(!m_nCounter);
}

//----------------------------------------------------------------------------
BOOL ProgramData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	BOOL ok = m_ReportData.CanConvertFieldToInput(pszFieldName, sLog);
	return ok;
}

//----------------------------------------------------------------------------
BOOL ProgramData::CanDeleteField(LPCTSTR pszFieldName, CString& sLog, BOOL* onlyUI) const
{
	BOOL ok =
		m_ReportData.		CanDeleteField(pszFieldName) &&
		m_ReportSymTable.	CanDeleteField(pszFieldName);

	if (ok)
	{
		CWoormDoc* pWDoc = m_ReportSymTable.GetDocument();
		if (pWDoc)
			ok = pWDoc->CanDeleteField(pszFieldName, sLog, onlyUI);
	}
	return ok;
}

//----------------------------------------------------------------------------
void ProgramData::DeleteField(LPCTSTR pszFieldName)
{
	CWaitCursor wc;

	CWoormDoc* pWDoc = m_ReportSymTable.GetDocument();
	if (pWDoc)
		pWDoc->PreDeleteField(pszFieldName);

	m_ReportData.		DeleteField(pszFieldName);
	m_ReportSymTable.	DelField(pszFieldName);

	if (pWDoc)
		pWDoc->GetSymTable()->DelField(pszFieldName);
}

//----------------------------------------------------------------------------
BOOL ProgramData::DeleteTable(WORD nID)
{
	return m_ReportData.DeleteTable(nID);
}

//----------------------------------------------------------------------------
void ProgramData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	m_ReportData.RenameField(pszOldFieldName, pszNewFieldName);

	m_ReportSymTable.RenameField(pszOldFieldName, pszNewFieldName);

	CWoormDoc* pWDoc = m_ReportSymTable.GetDocument();
	if (pWDoc)
	{
		pWDoc->GetSymTable()->RenameField(pszOldFieldName, pszNewFieldName);
	}
}

//----------------------------------------------------------------------------
BOOL ProgramData::Parse(Parser& lex, SqlConnection* pSqlConnection, ReportEngine* pReportEngine)
{
	lex.RemoveCommentTrace(); //remove previous comments
	// can have .wrm without REPORT section
	if (!lex.LookAhead(T_REPORT))
		return TRUE;

	lex.SkipToken();

    if (lex.Matched(T_HELP))
    {
    	if (!lex.ParseString(m_strHelpFile))
    		return FALSE;

		CBaseDocument* pDoc = GetSymTable()->GetDocument();
		CString strHelpFile;
		if (pDoc)
			AfxGetPathFinder()->GetModuleHelpPath(pDoc->GetNamespace(), CPathFinder::STANDARD) + SLASH_CHAR + m_strHelpFile;

		if (strHelpFile.IsEmpty())
			return lex.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_INVALID_HELP_FILE));
	}
    	
	return m_ReportData.Parse(lex, pSqlConnection, pReportEngine);
}

//----------------------------------------------------------------------------
BOOL ProgramData::Unparse(Unparser& oFile)
{
	//A. 19804
	//if (m_ReportSymTable.IsEmpty())
	//	return TRUE;
	
	oFile.UnparseCrLf	();
    oFile.UnparseTag	(T_REPORT,	FALSE);

	if (!m_strHelpFile.IsEmpty())
	{
	    oFile.UnparseTag	(T_HELP,		FALSE);
		oFile.UnparseString	(m_strHelpFile,	FALSE);
	}

	oFile.UnparseCrLf	();

 	m_ReportData.Unparse (oFile);

	oFile.UnparseCrLf();
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL ProgramData::GetSchema(CXSDGenerator* pXMLSchema)
{
	return m_ReportData.GetSchema(pXMLSchema);
}
