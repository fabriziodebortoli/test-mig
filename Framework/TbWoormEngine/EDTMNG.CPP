
#include "stdafx.h"

#include <TbGeneric\DataObj.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\basedoc.h>
//#include <TbWoormViewer\TABLE.H>
#include <TbOleDb\HelperExternalReferences.h>
#include <TbOledb\sqlconnect.h>
#include <TbOledb\oledbmng.h>
#include <TbParser\XmlFunctionObjectsParser.h>
#include <TbGeneric\EnumsTable.h>
//#include <TbWoormViewer\WOORMDOC.H	>

#include "askdlg.h"
#include "evndlg.h"
#include "expdlg.h"
#include "orddlg.h"
#include "prgdata.h"
#include "procdlg.h"
#include "rpsymdlg.h"
#include "rpsymtbl.h"
#include "ruledata.h"
#include "tbldlg.h"
#include "report.h"
#include "repengin.h"
#include "edtmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//	class EditorManager implementation
//=============================================================================
//-----------------------------------------------------------------------------
EditorManager::EditorManager (ReportEngine* pReportEngine, DataBool& bShowAdvancedForms)
	:
	m_pDocument		(NULL),
	m_pPrgData		(NULL),
	m_bModifiedFlag	(FALSE),
	m_bCurrRunValid	(TRUE),
	m_bProgramLoaded(FALSE),
	m_pReportEngine (pReportEngine),
	m_bShowAdvancedForms	(bShowAdvancedForms),
	m_pInternalFunctions(NULL),
	m_pEnumArray		(NULL),
	m_pHelperSqlCatalog	(NULL),
	m_pHelperExternalReferences	(NULL)
{
	m_pPrgData = new ProgramData(pReportEngine->GetCallerDoc());
}

//-----------------------------------------------------------------------------
EditorManager::~EditorManager ()
{
	SAFE_DELETE(m_pPrgData);
	SAFE_DELETE(m_pInternalFunctions);
	SAFE_DELETE(m_pHelperSqlCatalog);
	SAFE_DELETE(m_pHelperExternalReferences);
	SAFE_DELETE(m_pEnumArray);
}

//-----------------------------------------------------------------------------
void EditorManager::Attach	(CBaseDocument* pDoc)
{
	m_pDocument = pDoc;
}

//-----------------------------------------------------------------------------
CInternalFunctionObjectsParser* EditorManager::GetInternalFunctions()
{
	if (!m_pInternalFunctions)
	{
		CWaitCursor wc;

		m_pInternalFunctions = new CInternalFunctionObjectsParser();
		VERIFY(m_pInternalFunctions->Load());
	}
	return m_pInternalFunctions;
}

//-----------------------------------------------------------------------------
EnumTagArray* EditorManager::GetEnumsArray()
{
	if (!m_pEnumArray)
	{
		CWaitCursor wc;

		m_pEnumArray = new EnumTagArray; m_pEnumArray->SetOwns(FALSE);
		m_pEnumArray->Copy(*AfxGetEnumsTable()->GetEnumTags());

		m_pEnumArray->SetCompareFunction(&CompareEnumsByTitle);
		m_pEnumArray->QuickSort();
	}
	return m_pEnumArray;
}

//-----------------------------------------------------------------------------
CHelperSqlCatalog* EditorManager::GetHelperSqlCatalog()
{
	if (!m_pHelperSqlCatalog)
	{
		CWaitCursor wc;

		m_pHelperSqlCatalog = new CHelperSqlCatalog();
		m_pHelperSqlCatalog->Load();
	}
	return m_pHelperSqlCatalog;
}

//-----------------------------------------------------------------------------
CHelperExternalReferences* EditorManager::GetHelperExternalReferences()
{
	if (!m_pHelperExternalReferences)
	{
		CWaitCursor wc;

		m_pHelperExternalReferences = new CHelperExternalReferences();
		m_pHelperExternalReferences->SetHelperSqlCatalog(GetHelperSqlCatalog());
		m_pHelperExternalReferences->LoadDBTs();
	}

	return m_pHelperExternalReferences;
}

//-----------------------------------------------------------------------------
//	Function Member for Visible Single Field
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
BOOL EditorManager::AddDataField(WORD& nID, CWordArray& ids, TblRuleData* pTblRule/* = NULL*/)
{
	return AddGenericField(nID, ids, DATA_FLD, FALSE, FALSE, pTblRule);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddExpField(WORD& nID)
{
	CWordArray	nIDs;
	return AddGenericField(nID, nIDs, EXP_FLD, TRUE);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddVarField(WORD& nID)
{
	CWordArray	nIDs;
	return AddGenericField(nID, nIDs, VAR_FLD, TRUE);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddHiddenField(WORD& nID)
{
	CWordArray	nIDs;
	return AddGenericField(nID, nIDs, HID_FLD, TRUE);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::ModifyField (WORD nID, BOOL bAlsoModifyType /*= TRUE*/)
{
	RuleDataArray*		pRuleData		= m_pPrgData->GetRuleData();
	WoormTable*		pSymTable		= m_pPrgData->GetSymTable();
	RuleDataObj*	pRuleDataObj	= pRuleData->GetRuleData(nID);

	WoormField*		pRepFieldData	= pSymTable->GetFieldByID(nID);
	if (pRepFieldData == NULL)
	{
		AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD));
		return FALSE;
	}

	BOOL bDlgOk = FALSE;
	
	// it is mean that the field is VarField (that is without a rule)
	//
	if (pRuleDataObj == NULL)
		switch (pRepFieldData->GetFieldType())
		{
			case WoormField::FIELD_NORMAL	:
			case WoormField::FIELD_COLUMN	:
			case WoormField::FIELD_SUBTOTAL	:
			case WoormField::FIELD_COLTOTAL	:
			{
				CVarFieldDlg varFieldDlg(pSymTable, pRuleData, (WoormField*) pRepFieldData, NULL, FALSE);
				bDlgOk = (varFieldDlg.DoModal() == IDOK);
				break;
			}
			case WoormField::FIELD_INPUT	:
			{
				AskFieldData* askField = m_pPrgData->GetAskRuleData()->GetAskField(pRepFieldData->GetName());
				if (askField)
				{
					CAskFieldDlg askFieldDlg(askField, pSymTable, pRuleData, FALSE);
					bDlgOk = (askFieldDlg.DoModal() == IDOK);
				}
				break;
			}
			default	: break;
 
		}                                                       
	else
		switch (pRuleDataObj->IsARule())
		{
			case RULE_EXPR:
			{
				CExpRuleDlg	expRuleDlg((ExpRuleData*) pRuleDataObj, pSymTable, pRuleData);
				bDlgOk = (expRuleDlg.DoModal() == IDOK);
				break;
			}
			case RULE_DATA_TABLE:
			{
				CWordArray		idsTmpArray;
				CModTblRuleDlg	modTblRuleDlg
				(
					pRepFieldData->GetName(),
					pRuleData,
					(TblRuleData*) pRuleDataObj,
					pSymTable,
					bAlsoModifyType,
					(m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)) ? ((CWoormDoc*)m_pDocument)->m_strDocumentFilter : L""),
					(m_pDocument ? m_pDocument->m_pSqlConnection : AfxGetDefaultSqlConnection()),
					m_bShowAdvancedForms
				);
	
				bDlgOk = (modTblRuleDlg.DoModal() == IDOK);
				break;
			}
			default : break;
		}

	if (bDlgOk)
		SetModifiedFlag();
		
	return bDlgOk;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::CanDeleteFields (CWordArray& nIDs, CString& sLog)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();

	for (int i = 0; i < nIDs.GetSize(); i++)
	{
		SymField* pRepFieldData = pSymTable->GetFieldByID(nIDs[i]);
		if (!pRepFieldData) 
			continue;

		if (!m_pPrgData->CanDeleteField(pRepFieldData->GetName(), sLog))
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void EditorManager::DeleteFields (CWordArray& nIDs)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();

	for (int i = 0; i < nIDs.GetSize(); i++)
	{
		SymField* pRepFieldData = pSymTable->GetFieldByID( nIDs[i] );
		if (pRepFieldData)
		{
			//Mantengo sincronizzata la view SymTable ,se elimino dalla reportSymTable, elimino anche dalal viewSymTable
		
			((CWoormDoc*)m_pDocument)->RemoveViewField(pRepFieldData->GetName());

			m_pPrgData->DeleteField( pRepFieldData->GetName() );
		}
	}
}

//-----------------------------------------------------------------------------
void EditorManager::ChangeInHidden(CWordArray& nIDs)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();

	for (int i = 0; i < nIDs.GetSize(); i++)
	{
		WoormField* pRepFieldData =  pSymTable->GetFieldByID(nIDs[i]);
		if (pRepFieldData)
		{
			pRepFieldData->SetFieldType(WoormField::FIELD_NORMAL);
			pRepFieldData->SetHidden(TRUE);

			((CWoormDoc*)m_pDocument)->RemoveViewField(pRepFieldData->GetName());
		}
	}
}

//-----------------------------------------------------------------------------
BOOL EditorManager::DeleteField (WORD nID/*, BOOL bTrasformInHidden*/)
{
	WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
	WoormField*	pRepFieldData	= pSymTable->GetFieldByID(nID);
	if (pRepFieldData == NULL)
	{
		AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD));

		// e` sconosciuto quindi si puo` senz'altro eliminare
		return TRUE;
	}

	// check if the field is an ask field, if yes, set it hidden
	//
	if (pRepFieldData->IsInput())
	{
		pRepFieldData->SetHidden(TRUE);
		SetModifiedFlag();
		return TRUE;
	}

	// check if the field is a column, if yes, must be taken 
	// all subtotal and coltotal field associate with it
	//
	CWordArray		idsSubTotal;
	CWordArray		idsColTotal;

	if (pRepFieldData->IsColumn())
	{
		pSymTable->GetTotalOf(nID, idsSubTotal, WoormField::FIELD_SUBTOTAL);
		pSymTable->GetTotalOf(nID, idsColTotal, WoormField::FIELD_COLTOTAL);
	}

	//if (bTrasformInHidden)
	{
		EventsData* pEventsData = m_pPrgData->GetEventsData();

		if (pRepFieldData->GetFieldType() == WoormField::FIELD_COLTOTAL || pRepFieldData->GetFieldType() == WoormField::FIELD_SUBTOTAL)
		{
			if (pRepFieldData->GetFieldType() == WoormField::FIELD_COLTOTAL)
				idsColTotal.Add(nID);
			else
				idsSubTotal.Add(nID);

			CString sLog;
			if (
					CanDeleteFields(idsSubTotal, sLog) &&
					CanDeleteFields(idsColTotal, sLog) &&
					m_pPrgData->CanDeleteField(pRepFieldData->GetName(), sLog)
				)
			{
				pEventsData->DeleteReportNamedAction(pRepFieldData->GetName(), ACT_EVAL);
				pEventsData->DeleteNewPageNamedAction(pRepFieldData->GetName(), ACT_DISPLAY);
				pEventsData->DeleteReportNamedAction(pRepFieldData->GetName(), ACT_DISPLAY);

				// NO REFACTORING OF THIS BLOCK //
				pEventsData->m_pNewPageActions->m_pBeforeBlock->DeleteField(pRepFieldData->GetName());
					
				pEventsData->m_pNewPageActions->m_pAfterBlock->DeleteField(pRepFieldData->GetName());

				pEventsData->m_pReportActions->m_pBeforeBlock->DeleteField(pRepFieldData->GetName());

				pEventsData->m_pReportActions->m_pAfterBlock->DeleteField(pRepFieldData->GetName());
				//////////////////////////////////
			}
		}

		// change current in hidden NORMAL field
		pRepFieldData->SetFieldType(WoormField::FIELD_NORMAL);
		pRepFieldData->SetHidden(TRUE);

		// change SubTotal e ColTotal variables in hidden NORMAL field
		ChangeInHidden(idsColTotal);
		ChangeInHidden(idsSubTotal);

		SetModifiedFlag();
		return TRUE;
	}
/*
	// first check if it is possible the deleting of the field and relative subtotal and coltotal,
	// if yes call the DeleteField function else ask if the user
	// wish add it in the hidden list.
	CString sLog;
	if (
			CanDeleteFields(idsSubTotal, sLog)	&&
			CanDeleteFields(idsColTotal, sLog)	&&			
			m_pPrgData->CanDeleteField(pRepFieldData->GetName(), sLog)
	   )
	{
		DeleteFields(idsSubTotal);
		DeleteFields(idsColTotal);
		
		//Mantengo sincronizzata la view SymTable ,se elimino dalla reportSymTable, elimino anche dalla viewSymTable
		((CWoormDoc*)m_pDocument)->RemoveViewField(pRepFieldData->GetName());		
		m_pPrgData->DeleteField(pRepFieldData->GetName());

		SetModifiedFlag();

		return TRUE;
	}
	else
	{
		CString strMessage;
		strMessage	= cwsprintf(_TB("Unable to delete!\r\nAdd {0-%s} to hidden list?"), (LPCTSTR) pRepFieldData->GetName());
		if (!sLog.IsEmpty())
			strMessage += '\n' + _TB("It is used by an other object:") + ' ' + sLog;

		if (AfxMessageBox(strMessage, MB_OKCANCEL | MB_ICONQUESTION) == IDOK)
		{
			return DeleteField(nID, TRUE);	// recall itself to hidden current field
		}
	}

	return FALSE;
*/
}

//-----------------------------------------------------------------------------
//	Function Member For Visible Table Column Field
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
BOOL EditorManager::AddTableDataField(WORD& nIdTable, CWordArray& nIDs, BOOL bCreateTable, TblRuleData* pTblRule/*=NULL*/)
{
	//DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szWoormRunningOptions, L"ShowNewEditor", DataBool(FALSE), szTbWoormViewerSettingFileName);
	//BOOL b = pSetting ? *((DataBool*)pSetting) : FALSE;
	//if (b) {

		nIdTable = GetNextId();

		WoormTable*	pSymTable = m_pPrgData->GetSymTable();
		DisplayTables*	pDispTable = pSymTable->GetDisplayTables();
		ASSERT(pDispTable);

		CString layout = ((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutView.GetString();

		((CWoormDoc*)m_pDocument)->SyncronizeViewSymbolTable(pSymTable);
		SetModifiedFlag();

		CString sTableName = cwsprintf(L"Table_%d", nIdTable);
		pDispTable->AddTable
			(
				sTableName,
				10,
				nIdTable,
				layout
				);
		
	
		for (int i = 0; i < nIDs.GetSize(); i++)
		{
			WoormField* pRepFieldData = pSymTable->GetFieldByID(nIDs[i]);
			if (pRepFieldData)
				pRepFieldData->SetDispTable(sTableName);
		}

		return TRUE;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddTableExpField(WORD& nIdTable, CWordArray& nIDs, BOOL bCreateTable)
{
	BOOL rc = AddGenericField(nIdTable, nIDs, EXP_FLD, FALSE, bCreateTable);

	if (rc)
	{
		WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
		DisplayTables*	pDispTable	= pSymTable->GetDisplayTables();
		ASSERT(pDispTable);

		for (int i = 0; i < nIDs.GetSize(); i++)
		{
			WoormField* pRepFieldData =  pSymTable->GetFieldByID(nIDs[i]);
			if (pRepFieldData)
				pRepFieldData->SetDispTable(pDispTable->GetName( nIdTable ));
		}
	}

	return rc;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddTableHiddenField(WORD& nIdTable, CWordArray& nIDs, BOOL bCreateTable)
{
	int		nCols = 0;
	BOOL	rc = AddGenericField(nIdTable, nIDs, HID_FLD, FALSE, bCreateTable);

	if (rc)
	{
		CMessages message;
		WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
		DisplayTables*	pDispTable	= pSymTable->GetDisplayTables();
		ASSERT(pDispTable);

		for (int i = nIDs.GetUpperBound(); i >= 0; i--)
		{
			WoormField* pRepFieldData =  pSymTable->GetFieldByID( nIDs[i] );

			if (pRepFieldData)
				if (pRepFieldData->IsInput())
				{
					// i campi di tipo input non possono essere anche colonne
					//
					message.Add(cwsprintf(_TB("Read-only field: {0-%s}"), pRepFieldData->GetName()));
					pRepFieldData->SetHidden(TRUE);
					
					// lo elimina dalla lista dei campi selezionati
					nIDs.RemoveAt(i);
				}
				else
				{
					pRepFieldData->SetDispTable( pDispTable->GetName( nIdTable ) );
					nCols++;
				}
		}

		message.Show();
	}

	return rc && nCols > 0;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddTableVarField(WORD& nIdTable, CWordArray& nIDs, BOOL bCreateTable)
{
	BOOL rc = AddGenericField(nIdTable, nIDs, VAR_FLD, FALSE, bCreateTable);

	if (rc)
	{
		WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
		DisplayTables*	pDispTable	= pSymTable->GetDisplayTables();
		ASSERT(pDispTable);

		for (int i = 0; i < nIDs.GetSize(); i++)
		{
			WoormField* pRepFieldData =  pSymTable->GetFieldByID( nIDs[i] );
			if (pRepFieldData)
				pRepFieldData->SetDispTable( pDispTable->GetName( nIdTable ) );
		}
	}

	return rc;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddRepeater(WORD& nId)
{
	ASSERT_VALID(m_pPrgData);
	ASSERT_VALID(m_pDocument);
	WoormTable*		pSymTable		= m_pPrgData->GetSymTable();
	ASSERT_VALID(pSymTable);
	DisplayTables*	pDispTable		= pSymTable->GetDisplayTables();
	ASSERT_VALID(pDispTable);

	nId = m_pPrgData->GetGlobalId()->GetNextId();

	pDispTable->AddTable
		(
			cwsprintf(_T("Repeater_%d"), nId),
			1,
			nId,
			((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutView.GetString()
		);

	SetModifiedFlag();
	
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddChart(WORD& nId)
{
	ASSERT_VALID(m_pPrgData);
	ASSERT_VALID(m_pDocument);
	WoormTable*		pSymTable = m_pPrgData->GetSymTable();
	ASSERT_VALID(pSymTable);

	//TODO CHART

	nId = m_pPrgData->GetGlobalId()->GetNextId();

	//pDispTable->AddTable
	//(
	//	cwsprintf(_T("Repeater_%d"), nId),
	//	1,
	//	nId,
	//	((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutView.GetString()
	//);

	SetModifiedFlag();

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::DeleteTable(WORD nID)
{
	return m_pPrgData->DeleteTable(nID);
}

//-----------------------------------------------------------------------------
//	Function Member For Total And Subtotal Field
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
void EditorManager::ModColTotal(WORD nID)
{
	RuleDataArray*		pRuleData = m_pPrgData->GetRuleData();
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();
	CWordArray		idsColTotal;

	pSymTable->GetTotalOf(nID, idsColTotal, WoormField::FIELD_COLTOTAL);
	if (idsColTotal.GetSize() == 0)
	{
		AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD));
		return;
	}

	SymField* pRepFieldData = pSymTable->GetFieldByID(idsColTotal[0]);
	if (pRepFieldData == NULL)
	{
		AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD));
		return;
	}

	CVarFieldDlg varFieldDlg(pSymTable, pRuleData, (WoormField*) pRepFieldData, NULL, FALSE);
	if (varFieldDlg.DoModal() == IDOK)
		SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void EditorManager::GetSubTotal(WORD nIdColumn, CWordArray& nIDs)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();
	pSymTable->GetTotalOf(nIdColumn, nIDs, WoormField::FIELD_SUBTOTAL);
}

//-----------------------------------------------------------------------------
void EditorManager::GetColTotal(WORD nIdColumn, CWordArray& nIDs)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();
	pSymTable->GetTotalOf(nIdColumn, nIDs, WoormField::FIELD_COLTOTAL);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::DelColTotal(WORD nIdColumn/*, BOOL bTrasformInHidden*/)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();

	CWordArray	idsColTotal;
	WORD		idColTotal;

	pSymTable->GetTotalOf(nIdColumn, idsColTotal, WoormField::FIELD_COLTOTAL);

	if (idsColTotal.GetSize() > 0)
		idColTotal = idsColTotal[0];
	else
		return TRUE;

	return DeleteField(idColTotal/*, bTrasformInHidden*/);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::ExistColTotalOf(WORD nIdColumn)
{
	WoormTable*	pSymTable = m_pPrgData->GetSymTable();
	CWordArray		idsDummy;

	pSymTable->GetTotalOf(nIdColumn, idsDummy, WoormField::FIELD_COLTOTAL);

	return idsDummy.GetSize() > 0;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddAutoColTotal
	(
	WORD		nIdColumn,
	BOOL		bTotalOnNewPage,
	BOOL		bResetOnNewPage
	)
{
	WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
	EventsData*		pEventsData		= m_pPrgData->GetEventsData();
	SymField*		pRepFieldColumn	= pSymTable->GetFieldByID( nIdColumn );

	if (pRepFieldColumn == NULL)
	{
		//AfxMessageBox(_TB("Column not found."));
		return FALSE;
	}

	DataType colDataType = pRepFieldColumn->GetDataType();

	//@@ TODO sarebbe bello mettere "Totali" descrittivi anche per le altre tipologie
	//   pero` bisogna rivedere la logica del wrmeng (vedi anche evndlg::ShowColumnField())
	if (!IsNumericDataObj(colDataType) && colDataType != DATA_STR_TYPE)
		return FALSE;

	CString strDefFieldName = GenerateName(pSymTable, pRepFieldColumn->GetName(), _T("Tot{0-%s}"));

	WoormField* pNewRepField = new WoormField(strDefFieldName, WoormField::FIELD_COLTOTAL);
	pSymTable->Add (pNewRepField);

	if (IsNumericDataObj(colDataType))
	{
		pNewRepField->SetDataType (colDataType);
		pNewRepField->SetEventFunction(pSymTable, pRepFieldColumn->GetName(), T_CSUM);
	}
	else
	{
		// "Totali" descrittivi
		pNewRepField->SetDataType (DATA_STR_TYPE);
		CString strTmp; strTmp = _TB("\"Total\"");
		pNewRepField->SetEventFunction(pSymTable, pRepFieldColumn->GetName(), strTmp, DATA_STR_TYPE);
	}				

	pEventsData->AddActionInAlways( ACT_EVAL, pNewRepField->GetName() );

	//Aggiunta totale alla viewSymbolTable..
	if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		WoormField* pField = new WoormField(strDefFieldName, WoormField::FIELD_COLTOTAL, pNewRepField->GetDataType(), nIdColumn, NULL, TRUE);
		if (pNewRepField->IsHidden())
			pField->SetHidden(TRUE);
		if (DataType::IsCompatible(pNewRepField->GetDataType(), pField->GetDataType()))
				pField->AssignData(*(pNewRepField->GetData()));
		
		((CWoormDoc*)m_pDocument)->AddViewField(pField, FALSE);
	}
	
	// the actions are added in the before section of the event /*TRUE*/
	//
	if (bTotalOnNewPage)
	{
		pEventsData->AddActionInNewPage	(ACT_DISPLAY, pNewRepField->GetName(), TRUE);
		pEventsData->AddActionInReport	(ACT_DISPLAY, pNewRepField->GetName(), FALSE);

		if (bResetOnNewPage)
			pEventsData->AddActionInNewPage(ACT_RESET, pNewRepField->GetName(), TRUE);
	}
	else
	{
		pEventsData->AddActionInReport(ACT_DISPLAY, pNewRepField->GetName(), FALSE);
	}
	
	SetModifiedFlag();

	return TRUE;
}

//-----------------------------------------------------------------------------
//	Function Member To Show Any Informations
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
/*
void EditorManager::ShowVariables()
{
	CReportSymTableDlg	reportSymTableDlg(this);
	reportSymTableDlg.DoModal();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowEvents()
{
	CEventDlg eventDlg
		(
			this,
			m_pPrgData->GetEventsData		(),
			m_pPrgData->GetProcedureData	(),
			m_pPrgData->GetSymTable			(),
			m_pPrgData->GetRuleData			(),
			m_pPrgData->GetSymTable()->GetDisplayTables ()
		 );
					 
	if (eventDlg.DoModal() == IDOK)	
		SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowOrderBy ()
{
	WoormTable*	pSymTable	= m_pPrgData->GetSymTable();
	QueryData*		pQueryData	= m_pPrgData->GetQueryData();

	CGroupByDlg GroupByDlg (pQueryData, pSymTable);
	if (GroupByDlg.DoModal() == IDOK)
		SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowAskRules()
{
	CBaseDocument *pDoc = NULL;
	if (m_pDocument->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		pDoc = (CBaseDocument*)m_pDocument;
	CAskRuleDlg	askRuleDlg (m_pPrgData, pDoc);
	if (askRuleDlg.DoModal() == IDOK) 
		SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowRelation (WORD)
{
	//@@ TODO showRelation 
	AfxMessageBox(_TB("Future implementations"));
}

//-----------------------------------------------------------------------------
void EditorManager::ShowRelations (CWordArray&)
{
	//@@ TODO showRelations
	AfxMessageBox(_TB("Future implementations"));
}

//-----------------------------------------------------------------------------
void EditorManager::ShowProcedures ()
{
	CProcedureDlg procDlg
		(
			this,
			m_pPrgData->GetEventsData		(),
			m_pPrgData->GetProcedureData	(),
			m_pPrgData->GetSymTable			(),
			m_pPrgData->GetRuleData			()
		);

	procDlg.DoModal ();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowQueries ()
{
	CQueriesDlg procDlg
		(
			this,
			m_pPrgData->GetEventsData		(),
			m_pPrgData->GetQueryObjectData	(),
			m_pPrgData->GetSymTable			(),
			m_pPrgData->GetRuleData			()
		);

	procDlg.DoModal ();
}

//-----------------------------------------------------------------------------
void EditorManager::ShowSubTotals(WORD)
{
	WoormTable*	pSymTable		= m_pPrgData->GetSymTable();
	EventsData*		pEventsData		= m_pPrgData->GetEventsData();

	CSubTotalDlg subTotalDlg(pEventsData, pSymTable);
	if (subTotalDlg.DoModal() == IDOK)
		SetModifiedFlag();
}
*/

//-----------------------------------------------------------------------------
//	Function Member To Retrive Field Information
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
CString EditorManager::GetPublicName(WORD nID)
{
	SymField* 	pRepFieldData = m_pPrgData->GetSymTable()->GetFieldByID(nID);
	CString strTmp;
	if (pRepFieldData) 
		strTmp = pRepFieldData->GetName();
	return strTmp;
}

//-----------------------------------------------------------------------------
CString EditorManager::GetDescrMenu(WORD nID)
{
	RuleDataArray*	pRuleData		= m_pPrgData->GetRuleData();
	RuleDataObj* 	pRuleDataObj	= pRuleData->GetRuleData(nID);
	WoormField*	pRepFieldData	=  m_pPrgData->GetSymTable()->GetFieldByID(nID);
	CString			strDescrMenu	= _TB("Change UNKNOWN field");

	if (pRepFieldData)
	{
		if (pRuleDataObj == NULL)
			switch (pRepFieldData->GetFieldType())
			{
				case WoormField::FIELD_NORMAL	:
				case WoormField::FIELD_COLUMN	:
				case WoormField::FIELD_SUBTOTAL	:
				case WoormField::FIELD_COLTOTAL	: strDescrMenu = _TB("Change function field");	break;
				case WoormField::FIELD_INPUT	: strDescrMenu = _TB("Change request field");	break;
			}                                                       
		else
			switch (pRuleDataObj->IsARule())
			{
				case RULE_EXPR	: strDescrMenu = _TB("Change expression field");	break;
				case RULE_DATA_TABLE	: strDescrMenu = _TB("Change archive field");	break;
			}
	}
	return strDescrMenu;
}
	
//-----------------------------------------------------------------------------
DataType EditorManager::GetDataType(WORD nID)
{
	SymField* pRepFieldData = m_pPrgData->GetSymTable()->GetFieldByID(nID);
	//ASSERT(pRepFieldData);
	return pRepFieldData == NULL ? DATA_NULL_TYPE : pRepFieldData->GetDataType();
}

//-----------------------------------------------------------------------------
int EditorManager::GetLen(WORD nID)
{
	SymField* 	pRepFieldData = m_pPrgData->GetSymTable()->GetFieldByID(nID);

	return pRepFieldData == NULL ? 0 : pRepFieldData->GetLen();
}

//-----------------------------------------------------------------------------
int EditorManager::GetDec(WORD nID)
{
	WoormField* pRepFieldData =  m_pPrgData->GetSymTable()->GetFieldByID(nID);

	return pRepFieldData == NULL ? 0 : pRepFieldData->GetNumDec();
}

//-----------------------------------------------------------------------------
BOOL EditorManager::SetFieldWidth(WORD nID, int nChars, BOOL bMultiline)
{
	WoormField* 	pRepFieldData =  m_pPrgData->GetSymTable()->GetFieldByID(nID);

	ASSERT(pRepFieldData);

	if (!pRepFieldData)
		return FALSE;

	pRepFieldData->SetLen(nChars);
	pRepFieldData->SetNumDec(bMultiline ? 1 : 0);

	if (pRepFieldData->IsColumn())
	{
		CWordArray		idsSubTotal;
		CWordArray		idsColTotal;

		m_pPrgData->GetSymTable()->GetTotalOf(nID, idsSubTotal, WoormField::FIELD_SUBTOTAL);
		m_pPrgData->GetSymTable()->GetTotalOf(nID, idsColTotal, WoormField::FIELD_COLTOTAL);

		int i = 0;
		for (i = 0; i < idsSubTotal.GetSize(); i++)
		{
			pRepFieldData =  m_pPrgData->GetSymTable()->GetFieldByID( idsSubTotal[i] );
			if (pRepFieldData)
			{
				pRepFieldData->SetLen(nChars);
				pRepFieldData->SetNumDec(bMultiline ? 1 : 0);
			}
		}

		for (i = 0; i < idsColTotal.GetSize(); i++)
		{
			pRepFieldData =  m_pPrgData->GetSymTable()->GetFieldByID( idsColTotal[i] );
			if (pRepFieldData)
			{
				pRepFieldData->SetLen(nChars);
				pRepFieldData->SetNumDec(bMultiline ? 1 : 0);
			}
		}
	}

	SetModifiedFlag();
	return TRUE;
}

//-----------------------------------------------------------------------------
//	Function Member To Retrive Table Information
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
CString	EditorManager::GetTablePublicName (WORD nID)
{
	DisplayTables*	pDispTable = m_pPrgData->GetSymTable()->GetDisplayTables ();
	ASSERT(pDispTable);
	return pDispTable->GetName(nID);
}

//-----------------------------------------------------------------------------
void EditorManager::SetTablePublicName (WORD nID, LPCTSTR newTableName)
{
	WoormTable*		pSymTable	= m_pPrgData->GetSymTable();
	DisplayTables*	pDispTable	= pSymTable->GetDisplayTables();
	ASSERT(pDispTable);

	EventsData*		pEventsData	= m_pPrgData->GetEventsData();
	ProcedureData*	procData	= m_pPrgData->GetProcedureData();
	CString			strOldTblName;

	strOldTblName = pDispTable->GetName(nID);
	pDispTable->SetName(nID, newTableName);

	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pRepFieldData = pSymTable->GetAt(i);
		if (pRepFieldData && strOldTblName.CompareNoCase(pRepFieldData->GetName()) == 0)
		{
			pRepFieldData->SetDispTable( newTableName );
		}
	}

	pEventsData->DispTableChanged( strOldTblName, newTableName );
	procData->DispTableChanged( strOldTblName, newTableName );
}

//-----------------------------------------------------------------------------
BOOL EditorManager::SetTableRows(WORD nID, int nRows)
{
	DisplayTables*	pDispTable	= m_pPrgData->GetSymTable()->GetDisplayTables();
	ASSERT(pDispTable);

	if (pDispTable->SetRows(nID, nRows))
		SetModifiedFlag();

	return TRUE;
}

//-----------------------------------------------------------------------------
int EditorManager::GetTableRows(WORD nID)
{
	DisplayTables*	pDispTable	= m_pPrgData->GetSymTable()->GetDisplayTables();
	ASSERT(pDispTable);

	return pDispTable->GetRows(nID);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::ExistsTable(WORD nID)
{
	DisplayTables*	pDispTable	= m_pPrgData->GetSymTable()->GetDisplayTables();
	ASSERT(pDispTable);
	int nIdx = pDispTable->Find(nID, DisplayTables::CURRENT_LAYOUT);
	return nIdx >= 0;
}

//-----------------------------------------------------------------------------
//	Function Member To Generate nId - Identifier
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
WORD EditorManager::GetId()
{
	return m_pPrgData->GetGlobalId()->GetId();
}

//-----------------------------------------------------------------------------
WORD EditorManager::GetNextId()
{
	return m_pPrgData->GetGlobalId()->GetNextId();
}

//-----------------------------------------------------------------------------
void EditorManager::SetLastId(WORD anId)
{
	m_pPrgData->GetGlobalId()->SetLastId(anId);
}


//-----------------------------------------------------------------------------
//	Function Member To Load & Save
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
BOOL EditorManager::Parse(Parser& lex)
{
	CBaseDocument *pDocument = NULL;
	if(m_pDocument->IsKindOf (RUNTIME_CLASS(CBaseDocument)))
		pDocument = (CBaseDocument*) m_pDocument;

	m_pPrgData->GetReportData()->GetAskRuleData()->SetOwnerDocument(pDocument);
	m_bProgramLoaded = m_pPrgData->Parse(lex,  pDocument->m_pSqlConnection, m_pReportEngine);
	return m_bProgramLoaded;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::Parse(const CString& strRepFileName, CStringArray& arErrors, int& nLine, int& nCol)
{
	CString sReport;
	CWoormDoc* pWDoc = NULL;

	if (m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		pWDoc = (CWoormDoc*)m_pDocument;
		if (pWDoc->IsReportString())
			sReport = pWDoc->GetReportString();
	}

	Parser lex(sReport, NULL, -1, TRUE);

	if (sReport.IsEmpty())	// ! pDoc->IsReportString()
	{
		if (strRepFileName.IsEmpty() || !lex.Open(strRepFileName, TRUE))
		{
			AfxMessageBox (EngineScheduler::FormatMessage(EngineScheduler::REPORT_PROGRAM_NOT_LOADED), MB_OK | MB_ICONSTOP);
			return FALSE;
		}
	}

	// skip graphic info
	lex.SkipToToken(T_REPORT);
	lex.RemoveCommentTrace(); //remove previous comments
	if (lex.LookAhead(T_EOF))
		return TRUE;
	if (lex.ErrorFound()) 
		return FALSE;

	SAFE_DELETE(m_pPrgData);

	m_pPrgData	= new ProgramData(pWDoc);
	ASSERT(m_pPrgData);
	
	BOOL bOk = Parse(lex);

	if (pWDoc/* && !bOk*/)
	{
		if (pWDoc->IsInUnattendedMode())
		{
			CString sErr = lex.BuildErrMsg();

			arErrors.Add(sErr);

			pWDoc->Message(sErr);

			if (pWDoc->m_pWoormInfo && pWDoc->m_pWoormInfo->m_bHideFrame)
			{
				pWDoc->m_pMessages->ToStringArray(pWDoc->m_pWoormInfo->m_arErrors);
				pWDoc->m_pWoormInfo->m_nLine = lex.GetCurrentLine();
				pWDoc->m_pWoormInfo->m_nCol = lex.GetCurrentPos();
			}
		}
		else //if (!bOk)
		{
			pWDoc->m_pMessages->ToStringArray(arErrors);

			arErrors.Add(lex.BuildErrMsg());
			nLine = lex.GetCurrentLine();
			nCol = lex.GetCurrentPos();
		}

		lex.ClearError();
		lex.Abort();
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::Unparse(Unparser& oFile)
{
	ASSERT(m_pPrgData);
	return m_pPrgData->Unparse(oFile);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::GetSchema(CXSDGenerator* pSchema)
{
	ASSERT(m_pPrgData);
	return m_pPrgData->GetSchema(pSchema);
}

//-----------------------------------------------------------------------------
BOOL EditorManager::ExistProgram()
{
	return m_bProgramLoaded;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::GetModifiedFlag()
{
	return m_bModifiedFlag;
}

//-----------------------------------------------------------------------------
BOOL EditorManager::IsCurrRunValid()
{
	return m_bCurrRunValid;
}

//-----------------------------------------------------------------------------
void EditorManager::SetModifiedFlag(BOOL bModified/*TRUE*/, BOOL setDocToo/*=TRUE*/)
{
	if (m_pDocument && setDocToo)
		m_pDocument->SetModifiedFlag(bModified);

	if (bModified)
	{
		m_bCurrRunValid = FALSE;
		m_bProgramLoaded = TRUE;
	}

	m_bModifiedFlag = bModified;
}

//-----------------------------------------------------------------------------
void EditorManager::SetValidRun(BOOL bValidRun/*TRUE*/)
{
	m_bCurrRunValid = bValidRun;
}

//-----------------------------------------------------------------------------
//	PRIVATE Function Member
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
void EditorManager::SetHidden(WoormTable* pSymTable, CWordArray& nIDs, BOOL bHiddenFlag)
{
	// set the hidden field attribute
	for (int i = 0; i < nIDs.GetSize(); i++)
	{
		WoormField* pRepFieldData = pSymTable->GetFieldByID(nIDs[i]);
		if (pRepFieldData) pRepFieldData->SetHidden(bHiddenFlag);
	}
}

//-----------------------------------------------------------------------------
BOOL EditorManager::AddGenericField
	(
		WORD&			nId,
		CWordArray&		nIds,
		LocalFieldType	localFieldType,
		BOOL			bIsSingleField,
		BOOL			bCreateTable /* = FALSE */,
		TblRuleData*	pTblRule /*= NULL*/
	)
{
	RuleDataArray*		pRuleData		= m_pPrgData->GetRuleData();
	WoormTable*		pSymTable		= m_pPrgData->GetSymTable();
	DisplayTables*	pDispTable		= pSymTable->GetDisplayTables();
	DispTableInfo*	pDispTableInfo	= NULL;
	BOOL			bOk				= TRUE;

	if (bCreateTable)
		pDispTableInfo = new DispTableInfo(this);

	nIds.RemoveAll();

	switch (localFieldType)
	{
		case DATA_FLD:
		{
			//DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szWoormRunningOptions, L"ShowNewEditor", DataBool(FALSE), szTbWoormViewerSettingFileName);
			//BOOL b = pSetting ? *((DataBool*)pSetting) : FALSE;
			//if (!b)
			//{
				CTblRuleDlg	tblRuleDlg
					(
						pRuleData,
							pSymTable,
							nIds,
							bIsSingleField,
							(m_pDocument) ? m_pDocument->m_pSqlConnection : AfxGetDefaultSqlConnection(),
							m_pDocument,
							pDispTableInfo,
							m_bShowAdvancedForms
					);

				if ( (tblRuleDlg.DoModal() != IDOK) && (!tblRuleDlg.m_bIsApplyPressed) )
					bOk = FALSE;
			//}
			//else if (pTblRule)
			//{
			//	CTblRuleDlgNEW	tblRuleDlg
			//		(
			//		pTblRule,
			//			pSymTable,
			//			nIds,
			//			bIsSingleField,
			//			(m_pDocument) ? m_pDocument->m_pSqlConnection : AfxGetDefaultSqlConnection(),
			//			m_pDocument,
			//			pDispTableInfo
			//		);

			//	if ((tblRuleDlg.DoModal() != IDOK) && (!tblRuleDlg.m_bIsApplyPressed))
			//		bOk = FALSE;

			//}
			//else
			//{
			//	CTblRuleDlgNEW	tblRuleDlg
			//		(
			//		pRuleData,
			//			pSymTable,
			//			nIds,
			//			bIsSingleField,
			//			(m_pDocument) ? m_pDocument->m_pSqlConnection : AfxGetDefaultSqlConnection(),
			//			m_pDocument,
			//			pDispTableInfo
			//		);

			//	if ((tblRuleDlg.DoModal() != IDOK) && (!tblRuleDlg.m_bIsApplyPressed))
			//		bOk = FALSE;
			//	
			//}

			//if (bOk && m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
			//{
			//	((CWoormDoc*)m_pDocument)->RefreshEditor(CWoormDoc::ERefreshEditor::Rules);
			//}

			break;
		}
/*		
		case EXP_FLD:
		{
			ExpRuleData*	pExpRuleData = new ExpRuleData(*pSymTable);
			WoormField*		pRepFieldData = new WoormField(_T(""));

			CExpRuleDlg expRuleDlg
				(
					pExpRuleData,
					pSymTable,
					pRuleData,
					pRepFieldData,
					pDispTableInfo,
					m_pDocument ? m_pDocument->GetCallerDocument() : NULL
				);

			if (expRuleDlg.DoModal() != IDOK)
				bOk = FALSE;
			else
			{
				pRuleData->Add(pExpRuleData);

				if (!pRepFieldData->IsHidden())
					nIds.Add(pRepFieldData->GetId());

				//if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
				//{
				//	((CWoormDoc*)m_pDocument)->RefreshEditor(CWoormDoc::ERefreshEditor::Variables);
				//}
			}

			break;
		}
		
		case VAR_FLD:
		{
			WoormField* pRepFieldData = new WoormField(_T(""));

			CVarFieldDlg varFieldDlg
				(
					pSymTable,
					pRuleData,
					pRepFieldData,
					NULL,
					TRUE,
					pDispTableInfo
				);

			if (varFieldDlg.DoModal() != IDOK)
				bOk = FALSE;
			else
			{
				if (!pRepFieldData->IsHidden())
					nIds.Add(pRepFieldData->GetId());

				//if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
				//{
				//	((CWoormDoc*)m_pDocument)->RefreshEditor(CWoormDoc::ERefreshEditor::Variables);
				//}
			}

			break;
		}
		
		case HID_FLD:
		{
			CSelFieldDlg selFieldDlg
				(
					pSymTable,
					nIds,
					bIsSingleField,
					pDispTableInfo
				);

			if (selFieldDlg.DoModal() != IDOK)
				bOk = FALSE;
			else
			{
				SetHidden(pSymTable, nIds, FALSE);

				//if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
				//{
				//	((CWoormDoc*)m_pDocument)->RefreshEditor(CWoormDoc::ERefreshEditor::Variables);
				//}
			}

			break;
		}
		*/
		default:
			ASSERT(FALSE);
			bOk = FALSE;
			break;
	}

	if (bOk)
	{
		if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
		{
			((CWoormDoc*)m_pDocument)->SyncronizeViewSymbolTable(pSymTable);
		}
		SetModifiedFlag();

		if (nIds.GetSize() > 0)
		{
			if (bCreateTable)
			{
				ASSERT(!bIsSingleField);

				// in questo caso nId e` l'ID della tabella che si sta creando
				nId = m_pPrgData->GetGlobalId()->GetNextId();

				ASSERT(pDispTable);
				pDispTable->AddTable
					(
						pDispTableInfo->m_strDispTableName,
						pDispTableInfo->m_nRowsNum,
						nId,
						((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutView.GetString()
					);
			}
			else
				if (bIsSingleField)
				{
					// in questo caso nId e` l'ID di un nuovo campo libero
					nId = nIds[0];
				}
		}
	}

	if (pDispTableInfo)
		delete pDispTableInfo;

	return bOk && nIds.GetSize() > 0;
}

//----------------------------------------------------------------------------
WoormTable*	EditorManager::GetSymTable() const	
{
	return m_pPrgData->GetSymTable();
}

//----------------------------------------------------------------------------
RuleDataArray* EditorManager::GetRuleData() const	
{
	return m_pPrgData->GetRuleData();
}

//----------------------------------------------------------------------------
ProgramData* EditorManager::GetPrgData() const	
{
	return m_pPrgData;
}

//----------------------------------------------------------------------------
DisplayTables*	EditorManager::GetDispTable()
{
	if (!m_pPrgData) 
		return NULL;
	
	return m_pPrgData->GetSymTable()->GetDisplayTables();
}
//=============================================================================
