
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbParser\Parser.h>

#include <TbGenlib\parsctrl.h>
#include <TBGenlib\baseapp.h>

#include "ActionsRepEngin.h"
#include "rpsymtbl.h"
#include "ruledata.h"
#include "disptbl.h"
#include "report.h"
#include "multilayout.h"
#include "repengin.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DisplayTableEntry, CObject)

//============================================================================
// DisplayTables (ex DispTable)
//============================================================================
const CString	DisplayTables::ALL_LAYOUT = _T("[ALL]");
const CString	DisplayTables::CURRENT_LAYOUT = _T("[CURRENT]");

IMPLEMENT_DYNAMIC(DisplayTables, CObject)
//-----------------------------------------------------------------------------

DisplayTables::DisplayTables (CWoormDoc* pDoc, SymTable* pSymTable)
	: 
	m_pDocument (pDoc),
	m_pSymTable(pSymTable)
{} 

void DisplayTables::SetDocument (CWoormDoc* pDoc)
{ m_pDocument = pDoc; }

void DisplayTables::SetSymTable (SymTable* pST)
{ m_pSymTable = pST; }

//----------------------------------------------------------------------------
void DisplayTables::AddTable(const CString& sName, int nRows, WORD nID, const CString& sLayout)
{
	ASSERT(sName[0] && nRows > 0 && m_pSymTable);

	m_pSymTable->SetLastId(nID);

	m_DispTables.Add( new DisplayTableEntry(sName, nRows, nID, sLayout) );
}

//----------------------------------------------------------------------------
DataStr& DisplayTables::GetCurrLayout()
{
	return m_pDocument->m_dsCurrentLayoutView;
}

//----------------------------------------------------------------------------
DisplayTableEntry* DisplayTables::GetAt (int idx)
{ 
	return (DisplayTableEntry*) m_DispTables[idx];
}

//----------------------------------------------------------------------------
int DisplayTables::Find(WORD nID, const CString& sLayout)
{
	for (int i = 0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[i];

		if  (pDispTblObject->m_wInternalID != nID) 
			continue;

		if (sLayout.CompareNoCase (DisplayTables::ALL_LAYOUT) == 0)
			return i;

		if (pDispTblObject->GetLayoutName().CompareNoCase(sLayout) == 0)
			return i;

		if (
				sLayout.CompareNoCase (DisplayTables::CURRENT_LAYOUT) == 0
				&&
				pDispTblObject->GetLayoutName().CompareNoCase (GetCurrLayout().GetString()) == 0
			)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
int DisplayTables::Find(const CString& searchedName, const CString& sLayout)
{
	for (int i=0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[i];

		if (pDispTblObject->GetTableName().CompareNoCase(searchedName))
			continue;
		
		if (sLayout.CompareNoCase (DisplayTables::ALL_LAYOUT) == 0)
			return i;
		
		if (pDispTblObject->GetLayoutName().CompareNoCase(sLayout) == 0)
			return i;

		if (
				sLayout.CompareNoCase (DisplayTables::CURRENT_LAYOUT) == 0
				&&
				pDispTblObject->GetLayoutName().CompareNoCase(GetCurrLayout().GetString()) == 0
			)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
BOOL DisplayTables::Exist(const CString& searchedName, const CString& sLayout)
{
	return Find (searchedName, sLayout) >= 0;
}

//----------------------------------------------------------------------------
BOOL DisplayTables::DelTable(WORD nId)
{
	int idx = Find(nId, DisplayTables::CURRENT_LAYOUT);
	if (idx < 0) 
		return FALSE;
	DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[idx];

	if (OnDelTable (pDispTblObject)) 
	{
		m_DispTables.RemoveAt(idx);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL DisplayTables::LayoutMove (WORD nId, const CString& srcLayout, const CString& dstLayout)
{
	int idx = Find(nId, srcLayout);
	if (idx < 0) 
		return FALSE;
	DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[idx];
	
	pDispTblObject->SetLayoutName(dstLayout);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL DisplayTables::LayoutClone (WORD nId, const CString& srcLayout, const CString& dstLayout)
{
	int idx = Find(nId, srcLayout);
	if (idx < 0) 
		return FALSE;
	DisplayTableEntry* pDTO = (DisplayTableEntry*) m_DispTables[idx];

	AddTable(pDTO->GetTableName(), pDTO->m_nRows, pDTO->m_wInternalID, dstLayout);
	return TRUE;
}

//----------------------------------------------------------------------------
void DisplayTables::LayoutRename (const CString& srcLayout, const CString& dstLayout)
{
	for (int i=0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[i];

		if (pDispTblObject->GetLayoutName().CompareNoCase(srcLayout)) 
			continue;
	
		pDispTblObject->SetLayoutName(dstLayout);
	}
}

//----------------------------------------------------------------------------
BOOL DisplayTables::SetRows(WORD nID, UINT nRows)
{
	int idx = Find(nID, DisplayTables::CURRENT_LAYOUT);
	if (idx < 0) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[idx];

	pDispTblObject->m_nRows = nRows;
	return TRUE;
}

//----------------------------------------------------------------------------
UINT DisplayTables::GetRows(WORD nID)
{
	int nIdx = Find(nID, DisplayTables::CURRENT_LAYOUT);
	return GetRowsFromIdx(nIdx);
}

//----------------------------------------------------------------------------
CString DisplayTables::GetNameFromIdx(int nIdx)
{
	if (nIdx < 0 || nIdx > m_DispTables.GetUpperBound()) 
	{
		//ASSERT(FALSE);
		return _T("");
	}
	DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[nIdx];
	return pDispTblObject->GetTableName();
}

//----------------------------------------------------------------------------
int DisplayTables::GetRowsFromIdx(int nIdx)
{
	if (nIdx < 0 || nIdx > m_DispTables.GetUpperBound()) 
	{
		ASSERT(FALSE);
		return 0;
	}

	DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[nIdx];
	return pDispTblObject->m_nRows;
}

//----------------------------------------------------------------------------
CString DisplayTables::GetName(WORD nID)
{
	int idx = Find(nID, DisplayTables::ALL_LAYOUT);	//sarebbe CURRENT ma è lo stesso
	return GetNameFromIdx(idx);
}

//----------------------------------------------------------------------------
void DisplayTables::SetName(WORD nId, const CString& sName)
{
	BOOL bFound = FALSE;
	for (int i=0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[i];

		if (pDispTblObject->m_wInternalID == nId)
		{
			pDispTblObject->SetTableName(sName);
			bFound = TRUE;
		}
	}
	ASSERT(bFound);
}

//---------------------------------------------------------------------------
DisplayTableEntry* DisplayTables::FindSingleDisplayTable()
{
	ASSERT(GetCount() == 1);
	return (DisplayTableEntry*) m_DispTables[0];
}

//---------------------------------------------------------------------------
DisplayTableEntry* DisplayTables::MatchDisplayTable(Parser& parser, BOOL& bUnnamed)
{
	bUnnamed = TRUE;

	parser.Matched(T_OF);	//la sua presenza dipende dal contesto di chiamata

	if (parser.LookAhead(T_ID))
	{
		CString strTableName;
		if (parser.ParseID(strTableName))
		{
			bUnnamed = FALSE;
			int idx = Find(strTableName, DisplayTables::ALL_LAYOUT);
			if (idx >= 0)
				return (DisplayTableEntry*) m_DispTables[idx];
			else
				parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_UNKNOWN_DISPLAY_TABLE), strTableName);
		}
	}
	else
		if (GetCount() == 1)
			return FindSingleDisplayTable();
		else
			parser.SetError(_TB("Table name missing or no table defined"));

	return NULL;
}

//----------------------------------------------------------------------------
BOOL DisplayTables::Parse(Parser& lex)
{
	CString	strNameDispTable;
	int		nRows;
	WORD	nID;

	if (!lex.Matched(T_TABLES))
		return (!lex.Bad());
		
	do 
	{
		if (!lex.ParseID(strNameDispTable))	
			return FALSE;
					
		if (!(
				lex.ParseSquareOpen	()		&&
				lex.ParseInt		(nRows)	&&
				lex.ParseSquareClose()		&&
				lex.ParseAlias		(nID)
			))
			return FALSE;

		DataStr dsLayout = REPORT_DEFAULT_LAYOUT_NAME; //m_pDocument->m_dsCurrentLayout;
		if (lex.Matched(T_PAGE_LAYOUT))
		{
			CString s;
			if (!lex.ParseString(s))
				return FALSE;
			dsLayout = s;
		}

		if (!lex.ParseSep())
			return FALSE;

		CString sLayout(dsLayout.GetString());
		if (Exist(strNameDispTable, (LPCTSTR)sLayout))
			return lex.SetError(_TB("Report-Tables section - Table name already defined"), cwsprintf(L"%s - alias:%d", strNameDispTable, nID));
		
		ASSERT(m_pDocument);
		IBaseObj* pObj = m_pDocument->FindByID((WORD)nID, sLayout);
		
		if (
				!pObj || !pObj->IsKindOf(RUNTIME_CLASS(IBaseObj)) || 
				(
					strcmp(pObj->GetRuntimeClass()->m_lpszClassName, "Table") &&
					strcmp(pObj->GetRuntimeClass()->m_lpszClassName, "Repeater")
				)
			)
			return lex.SetError(_TB("Report-Tables section - Wrong Table/Repeater Internal identifier"), cwsprintf(L"%s - alias:%d", strNameDispTable, nID));

		if (nRows != pObj->RowsNumber())
			return lex.SetError(_TB("Report-Tables Section - Wrong Table/Repeater rows number"), cwsprintf(L"%s - alias:%d - rows: %d,%d", strNameDispTable, nID, nRows, pObj->RowsNumber()));

		this->AddTable(strNameDispTable, nRows, nID, dsLayout.GetString());
	} 
	while (lex.LookAhead(T_ID));

	lex.ParseEnd();
	
	return (!lex.Bad());
}

//----------------------------------------------------------------------------
void DisplayTables::Unparse(Unparser& oFile)
{
	if (m_DispTables.GetSize() == 0)	
		return;

	oFile.UnparseTag (T_TABLES, TRUE);
	oFile.IncTab();
	for (int i = 0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntry* pDispTblObject = (DisplayTableEntry*) m_DispTables[i];

		oFile.UnparseID			(pDispTblObject->GetTableName(),		FALSE);
		oFile.UnparseSquareOpen	(								FALSE);
		oFile.UnparseInt		(pDispTblObject->m_nRows,		FALSE);
		oFile.UnparseSquareClose(								FALSE);
		oFile.UnparseAlias		(pDispTblObject->m_wInternalID,	FALSE);
		if (pDispTblObject->GetLayoutName().CompareNoCase(REPORT_DEFAULT_LAYOUT_NAME))
		{
			oFile.UnparseTag	(T_PAGE_LAYOUT, FALSE);
			oFile.UnparseString	(pDispTblObject->GetLayoutName(), FALSE);
		}
		oFile.UnparseSep		(TRUE);
	}
	oFile.UnparseEnd();
	oFile.UnparseCrLf();
	oFile.IncTab();
}

//===========================================================================
//
//              Class DisplayTable implementation
//
//===========================================================================
IMPLEMENT_DYNAMIC(DisplayTableEntryEngine, DisplayTableEntry)

//---------------------------------------------------------------------------
DisplayTableEntryEngine::DisplayTableEntryEngine (const CString& sName, int nRows, WORD nID, const CString& sLayout)
	:
	DisplayTableEntry	(sName, nRows, nID, sLayout),
	m_bOwnDT			(TRUE),
	m_pDT				(NULL)
{}

//---------------------------------------------------------------------------
DisplayTableEntryEngine::~DisplayTableEntryEngine ()
{
	if (m_bOwnDT) SAFE_DELETE(m_pDT);
}

//---------------------------------------------------------------------------
void DisplayTableEntryEngine::ResetRowsCounter ()
{
	m_pDT->m_nCurrRow				= 0;
	m_pDT->m_bDataDisplayed			= FALSE;
	m_pDT->m_bTableFull				= FALSE;
	m_pDT->m_bExecutingTableActions = FALSE;
}

//---------------------------------------------------------------------------
BOOL DisplayTableEntryEngine::ExecOverflowActions()
{
	if (m_pDT->m_bTableFull)
	{
		if (m_pDT->m_pTableActions)
		{
			if (m_pDT->m_bExecutingTableActions)
			{
				m_pDT->m_bExecutingTableActions = FALSE;
				m_pDT->m_pTableActions->m_OwnRepEngine.ResetRTError();

				return m_pDT->m_pTableActions->m_OwnRepEngine.SetRTError(EngineScheduler::REPORT_EXEC_TABLE_ACTIONS, 0, GetTableName());
			}

			// semaforizzo l'esecuzione delle azioni Before per evitare
			// ricorsivita` indotta da comandi di visualizzazione
			// su una tabella in stato di full
			m_pDT->m_bExecutingTableActions = TRUE;
			if (!(m_pDT->m_pTableActions->GetBeforeActions()).Exec())
			{
				m_pDT->m_bExecutingTableActions = FALSE;
				return FALSE;
			}
		}

		ResetRowsCounter();
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DisplayTableEntryEngine::WriteLine (RepEngine& aRepEngine, WORD wCmd, RDEData* pRDE/*= NULL*/)
{
	if (m_pDT->m_nMultiLineFieldsNum > 0)
	{
		//
		// Se sono state "displayate" delle colonne multilinea si innesca una
		// scrittura ricorsiva delle sotto-stringhe rimanenti
		//

		// Il comando INTER_LINE deve essere pre o post-posto ai comandi
		// TITLE_LINE/NEXT_LINE
		if (wCmd == RDEManager::INTER_LINE)
		{
			aRepEngine.ResetRTError();
			return aRepEngine.SetRTError(EngineScheduler::REPORT_INTERLINE_WHILE_RASTER_FIELDS, 0, GetTableName());
		}

		// Viene forzato un comando di NEXT_LINE
		if (!WriteLineSeparator(aRepEngine, RDEManager::NEXT_LINE, NULL))
		{
			m_pDT->m_nMultiLineFieldsCurrLine = 0;
			return FALSE;
		}

		// la "prima linea" e` gia` stata scritta dalle Display
		// chiamate direttamente dal RepEngine, quindi si incrementa
		// PRIMA di chiamare le Write
		//
		m_pDT->m_nMultiLineFieldsCurrLine++;

		for (int i = 0; i <= m_pDT->m_OwnColumns.GetUpperBound(); i++)
		{
			WoormField* pColField = (WoormField*) m_pDT->m_OwnColumns[i];

			if	(
					!pColField->IsHidden() && !pColField->IsColTotal() &&
					!pColField->Write(aRepEngine)
				)
			{
				m_pDT->m_nMultiLineFieldsCurrLine = 0;
				return FALSE;
			}
		}

		if (!WriteLine (aRepEngine, wCmd, pRDE))
			return FALSE;

		return TRUE;
	}

	m_pDT->m_nMultiLineFieldsCurrLine = 0;

	return WriteLineSeparator(aRepEngine, wCmd, pRDE);
}

//---------------------------------------------------------------------------
BOOL DisplayTableEntryEngine::WriteLineSeparator (RepEngine& aRepEngine, WORD wCmd, RDEData* pRDE/*= NULL*/)
{
	if (!m_pDT->m_bDataDisplayed) 
		return TRUE;

	if ((wCmd != RDEManager::INTER_LINE) && !ExecOverflowActions()) 
		goto writeError;

	if (pRDE)
	{
		if (!aRepEngine.GetOutChannel()->Write(-m_wInternalID, *pRDE))
			goto writeError;
	}
	else if (!aRepEngine.GetOutChannel()->Write(m_wInternalID, wCmd, 0))
		goto writeError;	
	

	if	(
			(wCmd != RDEManager::INTER_LINE) &&
			!m_pDT->m_bTableFull             &&
			(m_nRows > 0)					 &&
			(++(m_pDT->m_nCurrRow) == m_nRows)
		)
	{
		m_pDT->m_nCurrRow = 0;
		m_pDT->m_bDataDisplayed = FALSE;

		// table full: Exec AFTER actions
		m_pDT->m_bTableFull = TRUE;

		if (!m_pDT->m_pTableActions || !m_pDT->m_pTableActions->GetAfterActions().Exec())
			goto writeError;
	}

	return TRUE;

writeError:
	EngineScheduler::MessageID nErrExpl = aRepEngine.GetRTErrorCode();
	LPCTSTR pszString = aRepEngine.GetRTErrorFailedName();

	aRepEngine.ResetRTError();
	return aRepEngine.SetRTError(EngineScheduler::REPORT_WRITE_TBL_CMD, EngineScheduler::FormatMessage (nErrExpl), pszString);
}

///////////////////////////////////////////////////////////////////////////////
DTCommon::~DTCommon ()
{
	SAFE_DELETE (m_pTableActions);
}

//============================================================================
// DisplayTablesEngine
//============================================================================
IMPLEMENT_DYNAMIC(DisplayTablesEngine, DisplayTables)

DisplayTablesEngine::DisplayTablesEngine (CWoormDoc* pDoc, SymTable* pTable/*= NULL*/) 
	: 
	DisplayTables (pDoc, pTable)
{} 

//----------------------------------------------------------------------------
DataStr& DisplayTablesEngine::GetCurrLayout()
{
	return m_pDocument->m_dsCurrentLayoutEngine;
}

//----------------------------------------------------------------------------
void DisplayTablesEngine::AddTable(const CString& sName, int nRows, WORD nID, const CString& sLayout)
{
	DisplayTableEntryEngine* pDTE = new DisplayTableEntryEngine(sName, nRows, nID, sLayout);

	for (int i = 0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntryEngine* pDisp = (DisplayTableEntryEngine*) GetAt(i);
		if (pDisp->m_wInternalID == nID)
		{
			ASSERT(sLayout.CompareNoCase(pDisp->GetLayoutName()));
			ASSERT(sName.CompareNoCase(pDisp->GetTableName()) == 0);
			ASSERT(pDisp->m_pDT);

			pDTE->m_bOwnDT = FALSE;
			pDTE->m_pDT = pDisp->m_pDT;
			break;
		}
	}

	if (pDTE->m_pDT == NULL)
	{
		pDTE->m_bOwnDT = TRUE;
		pDTE->m_pDT = new DTCommon;
	}

	m_DispTables.Add(pDTE);
}

//---------------------------------------------------------------------------
BOOL DisplayTablesEngine::OnDelTable (DisplayTableEntry* pD) 
{ 
	DisplayTableEntryEngine* pDTE = (DisplayTableEntryEngine*) pD;
	if (!pDTE->m_bOwnDT) 
		return TRUE;

	for (int i = 0; i < m_DispTables.GetSize(); i++)
	{
		DisplayTableEntryEngine* pDisp = (DisplayTableEntryEngine*) GetAt(i);
		if (pDisp == pD) continue;

		if (pDisp->m_wInternalID == pDTE->m_wInternalID)
		{
			ASSERT(pDisp->GetLayoutName().CompareNoCase(pDTE->GetLayoutName()));
			ASSERT(pDTE->GetTableName().CompareNoCase(pDisp->GetTableName()) == 0);
			ASSERT(pDisp->m_pDT == pDTE->m_pDT && !pDisp->m_bOwnDT);

			pDisp->m_bOwnDT = TRUE;
			pDTE->m_bOwnDT = FALSE;
			return TRUE; 
		}
	}
	ASSERT(FALSE);
	return FALSE; 
}

///////////////////////////////////////////////////////////////////////////////
