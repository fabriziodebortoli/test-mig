
#include "stdafx.h"

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\ContextFunctions.h>

#include <TbNameSolver\TBNamespaces.h>

#include <TbGenlib\baseapp.h>

#include <TbParser\Parser.h>

#include "ActionsRepEngin.h"
#include "disptbl.h"
#include "prgdata.h"
#include "rpsymtbl.h"
#include "MultiLayout.h"
#include "ExportSymbols.h"
#include "report.h"

#include "repfield.h"
#include "reptable.h"
#include "repengin.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
//									WoormTable
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(WoormTable, SymTable)

//----------------------------------------------------------------------------
WoormTable::WoormTable(WoormSymTableType tt, CWoormDoc* pDocument, GlobalId* pGlobalId/* = NULL*/)
	:
	m_TableType			(tt),
	m_pRepEngine		(NULL),
	m_pDisplayTables	(NULL),
	m_pGlobalId			(pGlobalId)
{
	if (m_TableType == ReportSymTable_EDITOR)
		m_pDisplayTables = new DisplayTables(pDocument, this);

	SetDocument(pDocument);

	AddAlias(_T("Woorm"), _T("Framework.TbWoormViewer.TbWoormViewer"));
	AddAlias(_T("RS"), _T("Framework.TbWoormViewer.TbWoormViewer"));
}

//----------------------------------------------------------------------------
WoormTable::~WoormTable()
{
	SAFE_DELETE(m_pDisplayTables);
	SAFE_DELETE(m_pRepEngine);
}

//----------------------------------------------------------------------------
BOOL WoormTable::IsEmpty ()	const
{ 
	return GetSize() <= m_nCountSpecialFields;
}

//----------------------------------------------------------------------------
void WoormTable::SetDocument (CBaseDocument* pDocument)	
{ 
	if (pDocument == GetDocument())
		return;
	ASSERT((GetDocument() == NULL) && pDocument);

	__super::SetDocument(pDocument);

	if (GetDocument())
	{
		if (m_pDisplayTables) 
			m_pDisplayTables->SetDocument(GetDocument());
		else
		{
			if (m_TableType == RepTable_ENGINE)
				m_pDisplayTables = new DisplayTablesEngine(GetDocument(), this);
		}

		if (GetSize() < 1)
		{

			switch (m_TableType)
			{
				case SymTable_RUNREPORT:
					{
						WoormField* pField = new WoormField(SpecialReportField::NAME.HIDE_ALL_ASK_DIALOGS, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.HIDE_ALL_ASK_DIALOGS);
						pField->SetHidden(TRUE); pField->SetStatic(TRUE);
						if (m_pDocument)
							pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bHideAllAskDialogs), FALSE);
						Add(pField);
					}
					break;
				case ReportSymTable_EDITOR:
					AddCommonSpecialFields();
					AddEditorSpecialFields();
					break;
				case RepTable_ENGINE:
					AddCommonSpecialFields();
					AddEngineSpecialFields();
					break;
				case SimpleTable_RDEVIEW:
					AddCommonSpecialFields();
					AddRdeViewSpecialFields();
					break;
			}

			m_nCountSpecialFields = GetSize();
		}
	}
}

//----------------------------------------------------------------------------
RepEngine*	WoormTable::GetRepEngine() const 
{ 
	return m_pRepEngine; 
}

//----------------------------------------------------------------------------
void WoormTable::SetOwnerId (CBaseDocument* pCallerDocument)	
{ 
	if (!pCallerDocument)
		return;
	ASSERT_VALID(pCallerDocument);
	WoormField* pField = this->GetFieldByID(SpecialReportField::ID.OWNER);
	if (!pField)
		return;
	DataLng* pData =  (DataLng*) pField->GetRuleData();
	ASSERT(pData);
	if (!pData)
		return;
	ASSERT_KINDOF(DataLng, pData);
	pData->Assign((long)(pCallerDocument));
	pData->SetAsHandle();
	pField->AddMethodList(pCallerDocument->GetRuntimeClass());
}

//-----------------------------------------------------------------------------
void WoormTable::AddCommonSpecialFields ()
{
	//Report Status field
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.STATUS, WoormField::FIELD_INPUT, DataType(DATA_ENUM_TYPE, TAG_REPORT_STATUS), SpecialReportField::ID.STATUS);
			pField->SetHidden(TRUE);	
			if (m_pDocument)
				pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_deReportStatus), FALSE);
		Add(pField);
	}

	// OwnerId field: memorizza l'ID (puntatore) dell'eventuale documento chiamante
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.OWNER, WoormField::FIELD_INPUT, DataType(DATA_LNG_TYPE), SpecialReportField::ID.OWNER);
			pField->SetHidden(TRUE);	
			// imposto il valore dell'oggetto più interno della gerarchia, la SetReadOnly fa poi sì che
			// venga ereditato anche dagli oggetti figli
			DataLng* pData =  (DataLng*) pField->GetRuleData();
			ASSERT(pData);
			ASSERT_KINDOF(DataLng, pData);
			pData->Assign((long)(m_pDocument ? m_pDocument->GetCallerDocument() : NULL));
			pData->SetAsHandle();
			pField->AddMethodList(RUNTIME_CLASS(CBaseDocument));
		Add(pField);
	}
}

//---------------------------------------------------------------------------
void WoormTable::AddEngineSpecialFields()
{
	// Report Layout field
	{
		WoormField* pField = new WoormField (SpecialReportField::NAME.LAYOUT, WoormField::FIELD_INPUT, DataType(DATA_STR_TYPE), SpecialReportField::ID.LAYOUT);
		// imposto il valore dell'oggetto più interno della gerarchia, la SetReadOnly fa poi sì che
		// venga ereditato anche dagli oggetti figli
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutEngine), FALSE);
		else
		{
			ASSERT(FALSE);
			DataStr* pDataS =  (DataStr*) pField->GetRuleData();
			ASSERT(pDataS);
			ASSERT_KINDOF(DataStr, pDataS);
			pDataS->Assign(REPORT_DEFAULT_LAYOUT_NAME);
		}
		Add(pField);
		pField->SetInitExpression(cwsprintf(_T("\"%s\""), REPORT_DEFAULT_LAYOUT_NAME));
	}

	// current page number field
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.PAGE, WoormField::FIELD_INPUT, DataType(DATA_LNG_TYPE), SpecialReportField::ID.PAGE);
		pField->SetReInit(TRUE);	
		// imposto il valore dell'oggetto più interno della gerarchia, la SetReadOnly fa poi sì che
		// venga ereditato anche dagli oggetti figli
		if (m_pDocument)
		{
			((CWoormDoc*)m_pDocument)->m_dlCurrentPageEngine = 1;
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_dlCurrentPageEngine), FALSE);
		}
		else
		{
			ASSERT(FALSE);
			DataLng* pData =  (DataLng*) pField->GetRuleData();
			ASSERT(pData);
			ASSERT_KINDOF(DataLng, pData);
			pData->Assign(1L);
		}
		Add(pField);
		pField->SetInitExpression(_T("1"));
	}

	// hide ask dialog
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.HIDE_ALL_ASK_DIALOGS, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.HIDE_ALL_ASK_DIALOGS);
		pField->SetHidden(TRUE); pField->SetStatic(TRUE);
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bHideAllAskDialogs), FALSE);
		Add(pField);
	}

	// IsFirstTuple
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.IS_FIRST_TUPLE, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.IS_FIRST_TUPLE);
		pField->SetHidden(TRUE); 
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bIsFirstTuple), FALSE);
		Add(pField);
	}
	// IsLastTuple
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.IS_LAST_TUPLE, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.IS_LAST_TUPLE);
		pField->SetHidden(TRUE); 
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bIsLastTuple), FALSE);
		Add(pField);
	}
}

//-----------------------------------------------------------------------------
void WoormTable::AddEditorSpecialFields ()
{
	// memorizza report layout
	{
		WoormField* pRepField = new WoormField(SpecialReportField::NAME.LAYOUT, WoormField::FIELD_INPUT);
		pRepField->SetId(SpecialReportField::ID.LAYOUT);
		DataStr* pds = m_pDocument ?
				&(((CWoormDoc*)m_pDocument)->m_dsCurrentLayoutView)
				:
				(DataStr*) DataObj::DataObjCreate(DataType(DATA_STR_TYPE));
		pds->Assign(REPORT_DEFAULT_LAYOUT_NAME);
		pds->SetValid();
		pRepField->SetDataPtr(pds, m_pDocument == NULL);

		Add(pRepField);
		pRepField->SetInitExpression(cwsprintf(_T("\"%s\""), REPORT_DEFAULT_LAYOUT_NAME));
	}

	// memorizza report current page number
	{
		WoormField* pRepField = new WoormField(SpecialReportField::NAME.PAGE, WoormField::FIELD_INPUT);
		pRepField->SetId(SpecialReportField::ID.PAGE);
		DataLng* pdl = m_pDocument ?
			&(((CWoormDoc*)m_pDocument)->m_dlCurrentPageView)
			:
			(DataLng*) DataObj::DataObjCreate(DataType(DATA_LNG_TYPE));
		pdl->Assign(1);
		pRepField->SetDataPtr(pdl, m_pDocument == NULL);
		Add(pRepField);
		pRepField->SetInitExpression(_T("1"));
	}

	// IsFirstTuple
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.IS_FIRST_TUPLE, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.IS_FIRST_TUPLE);
		pField->SetHidden(TRUE);
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bIsFirstTuple), FALSE);
		Add(pField);
	}
	// IsLastTuple
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.IS_LAST_TUPLE, WoormField::FIELD_INPUT, DataType(DATA_BOOL_TYPE), SpecialReportField::ID.IS_LAST_TUPLE);
		pField->SetHidden(TRUE);
		if (m_pDocument)
			pField->SetDataPtr(&(((CWoormDoc*)m_pDocument)->m_bIsLastTuple), FALSE);
		Add(pField);
	}

	// colonna temporanea per creazione tabella vuota
	//TODO
}

//----------------------------------------------------------------------------
void WoormTable::AddRdeViewSpecialFields()
{
	WoormField* pF;

	pF = new WoormField(SpecialReportField::NAME.PAGE, WoormField::FIELD_INPUT,
		DataType::Long, SpecialReportField::ID.PAGE, NULL, TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.LAYOUT, WoormField::FIELD_INPUT,
		DataType::String,	SpecialReportField::ID.LAYOUT, NULL, TRUE);
	Add(pF);

	//---- Campi speciali di sola visualizzazione
	pF = new WoormField(SpecialReportField::NAME.IS_PRINTING,	WoormField::FIELD_INPUT, 
		DataType::Bool,	SpecialReportField::ID.IS_PRINTING, &(((CWoormDoc*)m_pDocument)->m_bIsPrinting), FALSE); 
	pF->SetHidden(TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.IS_ARCHIVING,	WoormField::FIELD_INPUT, 
		DataType::Bool,	SpecialReportField::ID.IS_ARCHIVING, &(((CWoormDoc*)m_pDocument)->m_bIsArchiving), FALSE); 
	pF->SetHidden(TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.IS_EXPORTING,	WoormField::FIELD_INPUT, 
		DataType::Bool,	SpecialReportField::ID.IS_EXPORTING, &(((CWoormDoc*)m_pDocument)->m_bIsExporting), FALSE); 
	pF->SetHidden(TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.LAST_PAGE, WoormField::FIELD_INPUT,
		DataType::Long,	SpecialReportField::ID.LAST_PAGE, NULL, TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.USE_DEFAULT_ATTRIBUTE, WoormField::FIELD_INPUT,
		DataType::Variant,	SpecialReportField::ID.USE_DEFAULT_ATTRIBUTE, NULL, TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.PRINT_ON_LETTERHEAD, WoormField::FIELD_INPUT,
		DataType::Bool, SpecialReportField::ID.PRINT_ON_LETTERHEAD, &(((CWoormDoc*)m_pDocument)->m_bPrintOnLetterhead), FALSE);
	pF->SetHidden(TRUE);
	Add(pF);

	pF = new WoormField(SpecialReportField::NAME.CURRENT_COPY, WoormField::FIELD_INPUT,
		DataType::Integer, SpecialReportField::ID.CURRENT_COPY, &(((CWoormDoc*)m_pDocument)->m_nCurrentCopyNumber), FALSE);
	pF->SetHidden(TRUE);
	Add(pF);
}

//----------------------------------------------------------------------------
void WoormTable::CopyStaticField (CStaticFieldArrayCache& arStaticField)
{
	arStaticField.RemoveAll();
	for (int i = 0; i < GetSize(); i++)
	{
		SymField* pField = GetAt(i);
		if (pField->IsKindOf(RUNTIME_CLASS(WoormField)))
		{
			WoormField* pRF = (WoormField*) pField;
			//Potrebbe dare fastidio al Layout if (pRF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID) continue;

			arStaticField.CheckAndAddField(pRF);
		}
	}
}

//----------------------------------------------------------------------------
DataObj* WoormTable::GetDataMember(int nIdx) const
{
	if (nIdx < 0 || nIdx > GetUpperBound())
		return NULL;

	WoormField* pFieldMember = dynamic_cast<WoormField*>(GetAt(nIdx));
	if (pFieldMember == NULL) 
		return NULL;
	ASSERT_VALID(pFieldMember);

	return pFieldMember->GetData();
}

//---------------------------------------------------------------------------
void WoormTable::AddProcedure (ProcedureObjItem* pProcedure)
{ 
	SymTable* pRoot = GetRoot();
	if (pRoot != this && pRoot->IsKindOf(RUNTIME_CLASS(WoormTable)))
	{
		((WoormTable*)pRoot)->AddProcedure (pProcedure);
		return;
	}
	
	m_arProcedures.Add(pProcedure);
}

ProcedureObjItem* WoormTable::FindProcedure	(LPCTSTR pszName) const
{
	SymTable* pRoot = GetRoot();
	if (pRoot != this && pRoot->IsKindOf(RUNTIME_CLASS(WoormTable)))
	{
		return ((WoormTable*)pRoot)->FindProcedure (pszName); 
	}

	for (int i = 0; i < m_arProcedures.GetSize(); i++)
	{
		ProcedureObjItem* P = (ProcedureObjItem*) m_arProcedures.GetAt(i);
		ASSERT(P->IsKindOf(RUNTIME_CLASS(ProcedureObjItem)));

		if (!_tcsicmp(P->GetName(), pszName))
			return P;
	}

	return NULL;
}

//---------------------------------------------------------------------------
void WoormTable::AddQuery (QueryObjItem* pQuery)
{ 
	SymTable* pRoot = GetRoot();
	if (pRoot != this && pRoot->IsKindOf(RUNTIME_CLASS(WoormTable)))
	{
		((WoormTable*)pRoot)->AddQuery (pQuery);
		return;
	}
	
	m_arQueries.Add(pQuery);
}

QueryObjItem* WoormTable::FindQuery	(LPCTSTR pszName) const
{
	SymTable* pRoot = GetRoot();
	if (pRoot != this && pRoot->IsKindOf(RUNTIME_CLASS(WoormTable)))
		return ((WoormTable*)pRoot)->FindQuery (pszName); 

	for (int i = 0; i < m_arQueries.GetSize(); i++)
	{
		QueryObjItem* Q = (QueryObjItem*) m_arQueries.GetAt(i);
		ASSERT(Q->IsKindOf(RUNTIME_CLASS(QueryObjItem)));

		if (!_tcsicmp(Q->GetName(), pszName))
			return Q;
	}
	return NULL;
}

//------------------------------------------------------------------------------
BOOL WoormTable::ResolveCallQuery(CString sFuncName, CFunctionDescription& aMethod, CString& sHandleName) const
{
	sHandleName.Empty();
	int idx = sFuncName.Find('.');
	if (idx < 0)
		return FALSE;
	CString sName = sFuncName.Left(idx);
	sFuncName = sFuncName.Mid(idx + 1);

	QueryObjItem* qry = FindQuery (sName);
	if (!qry)
		return FALSE;

	BOOL bOk = QueryObjItem::ResolveCallQuery(sFuncName, aMethod);
	if (bOk)
		sHandleName = sName;
	return bOk;
}

//------------------------------------------------------------------------------
BOOL WoormTable::ResolveCallProcedure (CString sName, CFunctionDescription& aFun) const
{
	ProcedureObjItem* proc = FindProcedure	(sName);
	if (!proc)
		return FALSE;

	if (!proc->m_pProcedure || !proc->m_pProcedure->GetFun())
		return FALSE;

	aFun = *proc->m_pProcedure->GetFun();
	
	return TRUE;
}
//==============================================================================

int WoormTable::GetDisplayTablesNum	()
{ return m_pDisplayTables ? m_pDisplayTables->GetCount() : 0; }

BOOL WoormTable::ExistsDisplayTable(const CString& searchedName)
{
	ASSERT(m_pDisplayTables);
	int idx = m_pDisplayTables->Find(searchedName, DisplayTables::ALL_LAYOUT);
	return idx  > -1;
}

DisplayTableEntry* WoormTable::FindSingleDisplayTable()
{ 
	ASSERT(m_pDisplayTables);
	return m_pDisplayTables->FindSingleDisplayTable();
}

DisplayTableEntry* WoormTable::MatchDisplayTable (Parser& lex, BOOL& bUnnamed)
{ 
	ASSERT(m_pDisplayTables);
	return m_pDisplayTables->MatchDisplayTable(lex, bUnnamed);
}

//---------------------------------------------------------------------------
void WoormTable::ResetRowsCounter		()
{
	ASSERT(m_pDisplayTables);
	for (int i = 0; i < m_pDisplayTables->m_DispTables.GetSize(); i++)
	{
		((DisplayTableEntryEngine*) (m_pDisplayTables->m_DispTables[i]))->ResetRowsCounter();
	}
}

//---------------------------------------------------------------------------
void WoormTable::ReattachDisplayTable		(const CString& sLayout)
{
	ASSERT (!sLayout.IsEmpty());
	
	for (int i = 0; i < GetSize(); i++)
	{
		SymField* pField = GetAt(i);
		if (pField->IsKindOf(RUNTIME_CLASS(WoormField)))
		{
			WoormField* pRF = (WoormField*) pField;
			pRF->ReattachDisplayTable(sLayout);
		}
	}
}

//----------------------------------------------------------------------------
WORD WoormTable::GetCurId() const
{
	return m_pGlobalId ? m_pGlobalId->GetId() : SpecialReportField::NO_INTERNAL_ID;
}

//----------------------------------------------------------------------------
void WoormTable::SetLastId(WORD wId)
{
	if (m_pGlobalId) 
		m_pGlobalId->SetLastId(wId);
}

//----------------------------------------------------------------------------
INT_PTR	 WoormTable::Add (SymField* pF)
{
	if (m_pGlobalId && pF->GetId() == SpecialReportField::NO_INTERNAL_ID)
		((WoormField*)pF)->SetId(m_pGlobalId->GetNextId());
	return __super::Add(pF);
}

//----------------------------------------------------------------------------
BOOL WoormTable::OnBeforeDelField (SymField* pF)
{
	// if it is the last ID added then global ID is decremented
	if (m_pGlobalId && m_pGlobalId->GetId() == pF->GetId())
		m_pGlobalId->DecId();
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL WoormTable::CanDeleteField(LPCTSTR pszFieldName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SymField* pF = GetAt(i);
		if (!pF->IsKindOf(RUNTIME_CLASS(WoormField)))
			continue;
		WoormField* pRepField	= (WoormField*)pF;
                                                 
		if (_tcsicmp(pszFieldName, pRepField->GetName()) == 0)
		{
			// no delete if field is currently displayed		                                                    
			if (pRepField->IsInput() && !pRepField->IsHidden())
				return FALSE;
		}
		else
			if (!pRepField->CanDeleteField(pszFieldName))
				return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void WoormTable::GetTotalOf(WORD wIdColumn, CWordArray& IdArray, WoormField::RepFieldType aType)
{                       
	if (aType != WoormField::FIELD_SUBTOTAL && aType != WoormField::FIELD_COLTOTAL) 
		return;
	
	WoormField*	pColumnField = GetFieldByID(wIdColumn);
	if (pColumnField == NULL)
		return;

	CString strColumnName = pColumnField->GetName();
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		WoormField* pRepField = GetAt(i);
		ASSERT_VALID(pRepField);
		ASSERT_KINDOF (WoormField, pRepField);
		if(!pRepField) continue;

		if (pRepField->GetFieldType() == aType)
		{
			EventFunction*	pFuncData = pRepField->GetEventFunction();
			if (pFuncData && _tcsicmp(pFuncData->GetPublicName(), strColumnName) == 0)
			{
            	IdArray.Add(pRepField->GetId());
            }
		}
	}
}

// verifica se ci sono altri campi liberi oltre a quello passato
//----------------------------------------------------------------------------
BOOL WoormTable::MoreVisibleFreeFields(LPCTSTR pszFieldName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		WoormField* pRepField = GetAt(i);
		ASSERT_VALID(pRepField);
		ASSERT_KINDOF (WoormField, pRepField);
		if (!pRepField) continue;

		if	(
				!pRepField->IsHidden() && pRepField->GetFieldType() == WoormField::FIELD_NORMAL &&
				_tcsicmp(pRepField->GetName(), pszFieldName) != 0
			)
			return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void WoormTable::Unparse(Unparser& oFile)
{
	if (GetDisplayTables())
	{
		GetDisplayTables()->Unparse(oFile);
	}

	if (IsEmpty())
		return;
	int	nNumItems = GetSize();

	BOOL bNoDispTableName = GetDisplayTables() ? GetDisplayTables()->GetCount() <= 1 : TRUE;

	oFile.UnparseTag(T_VAR, TRUE);
	oFile.IncTab();

	for (int i = 0; i < nNumItems; i++)
	{
		WoormField* pRepField = GetAt(i);
		ASSERT_VALID(pRepField);
		ASSERT_KINDOF (WoormField, pRepField);
		if (!pRepField) continue;
		
		pRepField->Unparse(oFile, bNoDispTableName);
	}

	oFile.UnparseEnd();
	oFile.UnparseCrLf();
}

//=============================================================================
//Engine and Editor
BOOL WoormTable::Parse(Parser& lex)
{ 
	if (!ParseDisplayTables(lex))
		return FALSE;
	return ParseFields(lex);
}

//---------------------------------------------------------------------------
BOOL WoormTable::ParseDisplayTables(Parser& lex)
{ 
	if (!lex.LookAhead(T_TABLES))
		return TRUE;
	ASSERT(m_pDisplayTables);
	return m_pDisplayTables ? m_pDisplayTables->Parse(lex) : TRUE; 
}

//----------------------------------------------------------------------------
//Engine and Editor
BOOL WoormTable::ParseFields(Parser& parser)
{	
	if (parser.ErrorFound())
		return FALSE;

	if (!parser.Matched(T_VAR))
		return TRUE;

	while (!parser.LookAhead(T_END) && !parser.Bad())
	{
		WoormField* pRepField = new WoormField(_T(""));

		if (!pRepField->Parse(parser, this))
		{
			if (GetAt(GetUpperBound()) == pRepField)
				RemoveAt(GetUpperBound());
			else 
				delete pRepField;
			return FALSE;
		}

		if (m_TableType == ReportSymTable_EDITOR)
		{ 
			if (AfxGetExportSymbols()->IsActivated())
				AfxGetExportSymbols()->AddItemOnLastFile
					(
						pRepField->GetName(), 
						pRepField->IsColumn() ? 
							EXP_SYMB_VAR_COLUMN : 
							(pRepField->IsInput() ? EXP_SYMB_VAR_INPUT : EXP_SYMB_VAR),
						pRepField->GetDataType()
					);

			SetLastId(pRepField->GetId());
		}
	}

	return parser.ErrorFound() ? FALSE : parser.ParseEnd();
}


//------------------------------------------------------------------------------
BOOL WoormTable::ParseVariables(const CString& sFileName, CString& sError, int& nLine, BOOL bAllowVarsWithSameAlias /*= FALSE*/)
{
	Parser lex;
	if (!lex.Open(sFileName))
		return TRUE;

	if (!lex.SkipToToken(T_VAR))
		return TRUE;

	if (lex.LookAhead(T_EOF))
		return TRUE;

	BOOL bOk = ParseVariables(&lex, sError, nLine, bAllowVarsWithSameAlias);
	if (!bOk)
	{
		sError = lex.BuildErrMsg();
		nLine = lex.GetCurrentLine();
		//nPos = lex.GetCurrentPos();
		lex.ClearError();
	}
	return bOk;
}

//------------------------------------------------------------------------------
BOOL WoormTable::ParseVariablesFromString(const CString& sReport, CString& sError, int& nLine, BOOL bAllowVarsWithSameAlias /*= FALSE*/)
{
	Parser lex(sReport, NULL, -1, TRUE);

	if (!lex.SkipToToken(T_VAR))
		return TRUE;

	if (lex.LookAhead(T_EOF))
		return TRUE;

	BOOL bOk = ParseVariables(&lex, sError, nLine, bAllowVarsWithSameAlias);
	if (!bOk)
	{
		sError = lex.BuildErrMsg();
		nLine = lex.GetCurrentLine();
		//nPos = lex.GetCurrentPos();
		lex.ClearError();
	}
	return bOk;
}

//------------------------------------------------------------------------------
//used by ViewSymbolTable and LinkReport editor (
BOOL WoormTable::ParseVariables(Parser* parser, CString& /*sError*/, int& /*nLine*/, BOOL bAllowVarsWithSameAlias /*= FALSE*/)
{
    if (!parser->ErrorFound() && parser->ParseTag(T_VAR)) 
    {
		// la sezione variables è vuota
		if (parser->LookAhead(T_END))
			return parser->ParseEnd() && !parser->ErrorFound();

		do 
		{
			DataType aType, aBaseType;
			if (!parser->ParseDataType(aType, aBaseType))
			{
    			return FALSE;
			}

			CString strFieldName;
			if (!parser->ParseID(strFieldName))
			{
    			return FALSE;
			}
			if (ExistField(strFieldName))
				return parser->SetError(_TB("Identifier already defined"), strFieldName);
			
			Token arT1 [] = {T_ALIAS, T_COMMA, T_CSUM};
			if (!parser->SkipToToken(arT1, sizeof(arT1)/sizeof(Token)))
				return FALSE;
	
			CString strFieldUsedByCsum;
		    if (parser->Matched(T_CSUM))
			{
				parser->Match(T_ROUNDOPEN);
				parser->ParseID(strFieldUsedByCsum);
				parser->Match(T_ROUNDCLOSE);

				Token arT2 [] = {T_ALIAS, T_COMMA};
				if (!parser->SkipToToken(arT2, sizeof(arT2)/sizeof(Token)))
					return FALSE;
			}
			
			BOOL bMultiLine = FALSE;
			if (parser->LookAhead(T_COMMA))	// [n ,1] 'Break' mark in 'Variables' section
			{
				bMultiLine = TRUE;
				if (!parser->SkipToToken(T_ALIAS))
					return FALSE;
			}

			WORD wInternalId;
			if (!parser->ParseAlias(wInternalId)) 
			{
				return FALSE;
			}
			if (GetFieldByID(wInternalId) && !bAllowVarsWithSameAlias) //in case of unparsing of RDE file parser must tolerate variables with same Id (for Total Column)
				return parser->SetError(_TB("Internal identifier already defined"), strFieldName);

			if (parser->Matched(T_AS))
				parser->SkipToken();
			parser->Matched(T_NO_XML);

			//---- THREAD CONTEXT
			CString sThreadContextVar;
			if (parser->Matched(T_CONTEXT))
			{
				parser->ParseString(sThreadContextVar);
			}

			BOOL bHidden = FALSE;
			DataObj* value = NULL;
 			CString sTableName;
			WoormField::RepFieldType ft = WoormField::FIELD_NORMAL;

			Token arT[] = { T_SEP, T_HIDDEN, T_COLTOTAL, T_SUBTOTAL, T_INPUT, T_COLUMN};
			if (!parser->SkipToToken(arT, sizeof(arT)/sizeof(Token)))
				return FALSE;

			if (parser->Matched(T_INPUT))
			{
				ft = WoormField::FIELD_INPUT;
			}
			else if (parser->Matched(T_HIDDEN))
			{
				bHidden = TRUE;

				if (parser->Matched(T_INPUT))
					ft = WoormField::FIELD_INPUT;

				if (parser->Matched(T_COLUMN))
				{
					ft = WoormField::FIELD_COLUMN;

					if (parser->Matched(T_OF))
						parser->ParseID(sTableName);
				};
			}
			else  if (parser->Matched(T_COLUMN))
			{
				ft = WoormField::FIELD_COLUMN;

				if (parser->Matched(T_OF))
					parser->ParseID(sTableName);
			}

			//TODO init espression non legge correttamente il base type per gli array (double vs int)
			if (parser->Matched(T_INIT))
			{
				if (!parser->Match(T_ASSIGN))
					return FALSE; 
				
                Expression  initExpression(this);
				if 
					(
						initExpression.Parse(*parser, aType, TRUE) 
						&& 
						!initExpression.HasExternalFunctionCall()
					)
				{	
			  		value = DataObj::DataObjCreate(aType); 
					initExpression.Eval(*value);     
				}
				else
					parser->ClearError();
			}

			if (parser->Matched(T_COLTOTAL))
			{
				ft = WoormField::FIELD_COLTOTAL;

				SymField* pSF = GetField(strFieldUsedByCsum);
				if (pSF != NULL)				
					wInternalId = pSF->GetId();
			}
			
			if (parser->Matched(T_SUBTOTAL))
				ft = WoormField::FIELD_SUBTOTAL;

			if (!parser->SkipToToken(T_SEP, TRUE))
			{
				SAFE_DELETE(value);
				return FALSE;
			}

			WoormField* pF = new WoormField(strFieldName, ft, aType, wInternalId, value, TRUE);
			
			SAFE_DELETE(value);

			if (bHidden)
				pF->SetHidden(TRUE);
			if (bMultiLine)
				pF->SetMultiline(TRUE);

			if (ft != WoormField::FIELD_SUBTOTAL)
				Add(pF);
			
			if (aType == DataType::Array)
			{
				pF->SetDataType(aBaseType, TRUE);
			}

			if (ft == WoormField::FIELD_SUBTOTAL)
				SAFE_DELETE (pF);

			if (!sThreadContextVar.IsEmpty())
			{
				pF->SetThreadContextName(sThreadContextVar);

				BOOL bOwnThreadContextVar = !CheckContextObject(AfxGetThreadContextBag(), sThreadContextVar);
				pF->SetOwnThreadContextVar(bOwnThreadContextVar);

				ASSERT_VALID(pF->GetData());
				DataObj* pThData = AddContextDataObj(AfxGetThreadContextBag(), sThreadContextVar, pF->GetData()->GetRuntimeClass(), FALSE);

				if (pThData)
				{
					pF->SetFieldType(WoormField::FIELD_INPUT);
					pF->SetDataPtr	(pThData, FALSE);
				}
				else
				{
					ASSERT_TRACE1(FALSE, "Fallito bind a variabile di contesto di thread %s\n", (LPCTSTR)sThreadContextVar);
				}
			}

		} 
		while (!parser->Matched(T_END) && !parser->ErrorFound());
	}
    return !parser->ErrorFound();
}

//------------------------------------------------------------------------------
// unparse only graphic information if needed (RDE)
void WoormTable::UnparseVariables(Unparser& unparser)
{
	if (IsEmpty())
		return;

	unparser.UnparseTag(T_VAR);
	unparser.IncTab();

	int i;
	for (i = 0; i < GetSize(); i++)
	{
		WoormField* pF =  (WoormField*) GetAt(i);

		if (pF->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID || pF->GetId() == 0)
			continue;

		if (pF->GetDataType() == DataType::Array)	
		{
			unparser.UnparseDataType(pF->GetDataType(), ((DataArray*)(pF->GetData()))->GetBaseDataType(), FALSE, TRUE);
		}
		else if (pF->GetDataType() == DataType::Record)	
		{
			ASSERT(FALSE);
			unparser.UnparseDataType(pF->GetDataType(), DataType::Null , FALSE, TRUE, L"/* DataType unsupported */");
		}
		else
		{
			unparser.UnparseDataType(pF->GetDataType(), DataType::Null, FALSE, TRUE);

			if (pF->GetDataType() == DATA_ENUM_TYPE)
			{
				unparser.Write	
					(
						_T(" /* ") + AfxGetEnumsTable()->GetEnumTagName(pF->GetDataType().m_wTag)  + _T(" */ "),
						FALSE
					);
			}
		}
		unparser.UnparseID(pF->GetName(), FALSE);

		unparser.UnparseAlias(pF->GetId(), FALSE);

		if (pF->IsHidden())
			unparser.UnparseTag(T_HIDDEN,FALSE);

		unparser.UnparseSep(TRUE); 
	}

	unparser.UnparseEnd();
	unparser.UnparseCrLf();
}

//---------------------------------------------------------------------------
SymTable* WoormTable::CreateLocalScope()
{
	WoormTable* pLocal = new WoormTable(SymTable_ACTION, this->GetDocument());
	
	pLocal->SetParent(this);

	pLocal->m_TableType = m_TableType;

	pLocal->m_pDisplayTables = m_pDisplayTables;
	pLocal->m_pRepEngine = m_pRepEngine;
	
	return pLocal;
}

void WoormTable::DeleteMeAsLocalScope()
{
	m_pDisplayTables = NULL;
	m_pRepEngine = NULL;

	delete this;
}

//-----------------------------------------------------------------------------
CString WoormTable::GetAdviseName(const CString& sPrefix)
{
	CString strAdvisedName = sPrefix;
	strAdvisedName.Remove(' ');	strAdvisedName.Remove('[');	strAdvisedName.Remove(']');

	int i = 1;
	while (this->ExistField(strAdvisedName))
		strAdvisedName.Format(_T("%s%d"), sPrefix, ++i);
	return strAdvisedName;
}

//=============================================================================
