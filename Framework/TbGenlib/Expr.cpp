#include "stdafx.h"

#include <ctype.h>
#include <math.h>
#include <time.h>
#include <stdarg.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\LineFile.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\CMapi.h>
#include <TbGeneric\ISqlRecord.h>

#include <TbOleDb\WClause.h>

#include "messages.h"
#include "baseapp.h"
#include "basedoc.h"

#include "expr.h"
#include "command.h"

#include "BarCode.h"

#include "SettingsTableManager.h"

// resources
#include <TbParser\TokensTable.h>

// strings

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// Operator compatibility maps.
//
DATA_TYPE_OP_MAP Expression::PlusMap;
DATA_TYPE_OP_MAP Expression::MinusMap;
DATA_TYPE_OP_MAP Expression::StarMap;
DATA_TYPE_OP_MAP Expression::SlashMap;
DATA_TYPE_OP_MAP Expression::PercMap;
DATA_TYPE_OP_MAP Expression::RelMap;

static const BOOL bInited = Expression::InitExpressionMaps();

//=============================================================================
//
//	LikeFunction evaluate a regular expression.
//
//=============================================================================
#define MATCH_ANY_STR_TAG		_T('%')
#define MATCH_ONE_CHAR_TAG		_T('_')

#define FIND_STRING			0
#define FIND_STRING_EXACT	1

//-----------------------------------------------------------------------------
static BOOL ExecSearch(int nCurrState, LPCTSTR& pszStrTarget, CString& strPattern)
{
	switch (nCurrState)
	{
		case FIND_STRING		:	pszStrTarget = _tcsstr(pszStrTarget, strPattern);
									if (pszStrTarget == NULL)
										return FALSE;

									pszStrTarget += strPattern.GetLength();
									strPattern.Empty();
									break;

		case FIND_STRING_EXACT	:	if (_tcsncmp(pszStrTarget, strPattern, strPattern.GetLength()) != 0)
										return FALSE;

									pszStrTarget += strPattern.GetLength();
									strPattern.Empty();
									break;
	}

	return TRUE;
}

// La logica e` la seguente:
//	Viene scandito la pattern string per estrarre le eventuali sottostringhe da
//	cercare nella stringa target.
//	Il tipo di ricerca da effettuare (cfr. ExecSearch sopra) e` determinato
//	dal carattere incontrato all'inizio della sottostringa estratta:
//	-	se il carattere era MATCH_ONE_CHAR_TAG allora la sottostringa deve
//		INIZIARE esattamente dal "corrente" inizio della stringa target (il puntatore
//		della stringa target viene incrementato)
//  -	se il carattere era MATCH_ANY_STR_TAG allora la sottostringa deve essere
//		CONTENUTO nella stringa target
//-----------------------------------------------------------------------------
static BOOL LikeFunction(LPCTSTR pszStrTarget, LPCTSTR patternStr, LPCTSTR escapeChar = _T("\\"))
{
	int		nCurrState		= FIND_STRING_EXACT;

	CString	strBuffer;
	int		lenStr			= _tcslen(pszStrTarget);
	int		lenPattern		= _tcslen(patternStr);
	int		nCharToMatch	= 0;
	
	for (int i = 0; i < lenPattern; i++)
	{
		switch (patternStr[i])
		{
			case MATCH_ANY_STR_TAG :
			{
				// se necessario si cerca la sottostringa corrente nella modalita` impostata
				// dal carattere all'inizio della stessa.
				//
				if (!strBuffer.IsEmpty() && !ExecSearch(nCurrState, pszStrTarget, strBuffer))
					return FALSE;

				// una sequenza di MATCH_ANY_STR_TAG sono trattati come uno unico
				//
				nCurrState = FIND_STRING;
				
				break;
			}
			case MATCH_ONE_CHAR_TAG :
			{
				// se necessario si cerca la sottostringa corrente nella modalita` impostata
				// dal carattere all'inizio della stessa.
				//
				if (!strBuffer.IsEmpty() && !ExecSearch(nCurrState, pszStrTarget, strBuffer))
					return FALSE;
				
				// in questo caso ci si muove in modo sincrono anche sulla stringa target
				// sempre che questa non sia finita
				//
				if (*pszStrTarget++ == '\0') return FALSE;

				nCurrState = FIND_STRING_EXACT;

				break;
			}
			default :
			{
				// se e` il carattere di escape incrementa l'indice per prendere il successivo
				//
				if	(patternStr[i] == escapeChar[0] && i < (lenPattern - 1)) i++;

				// si bufferizza la sottostringa
				//
				strBuffer += patternStr[i];
			}
		}
	}
	
	switch (nCurrState)
	{
		case FIND_STRING		:	if (strBuffer.IsEmpty())
									{
										// trovato MATCH_ANY_STR_TAG come ultimo carattere
										// quindi il corrente stato della stringa target
										// e` indifferente
										return TRUE;
									}
									
									break;	
									
		case FIND_STRING_EXACT	:	if (strBuffer.IsEmpty())
									{
										// trovato MATCH_ONE_CHAR_TAG come ultimo carattere
										// e quindi nella stringa target non ci devono essere
										// altri caratteri
										return *pszStrTarget == NULL_CHAR;
									}
									
									break;
	}

	// caso in cui il pattern da trovare non e` seguito da nessun carattere MATCH_....
	//
	if (!ExecSearch(nCurrState, pszStrTarget, strBuffer))
		return FALSE;
	
	// in questa condizione se il pattern e` stato trovato deve essere anche l'ultimo
	// segmento della stringa da controllare
	//
	return *pszStrTarget == NULL_CHAR;
}

//=============================================================================
//			EXPRESSION FUNCTION MEMBERS
//=============================================================================
// When user type in a new expression, or modify an existing one, first
// only the string form is assigned. The internal form is assigned by parsing
// the string only when the expression is tested for validity or evaluated.
// Once parsed, the internal form is kept and used for repeated evaluations,
// until the user modify the string form and re-Assign it again.
//-----------------------------------------------------------------------------

IMPLEMENT_DYNAMIC (Expression, CObject);

Expression::Expression(SymTable* pSymTable)
	:
	IDisposingSourceImpl(this),

	m_pSymTable		(pSymTable),
	m_pStopTokens	(NULL),
	m_nErrorPos		(-1),
	m_nErrorID		(EMPTY_MESSAGE),
	m_nParseStartLine	(0)
{}

Expression::Expression(const Expression& aExp) 
	: 
	IDisposingSourceImpl(this),

	m_pSymTable		(NULL),
	m_pStopTokens	(NULL),
	m_nErrorPos		(-1),
	m_nErrorID		(EMPTY_MESSAGE),
	m_nParseStartLine	(0) 
{ 
	Assign(aExp);
}

//-----------------------------------------------------------------------------
CString Expression::GetErrDescription (BOOL bVerbouse /*= TRUE*/)
{ 
	CString s;
	
	if  (bVerbouse)
	{
		s = FormatMessage(GetErrId());
		s += cwsprintf(_T("\n(Line: %d %s)"), this->m_nParseStartLine, m_sErrorDetail);

		if (!m_strExprString.IsEmpty())
			s += '\n' + m_strExprString;
		else if (!m_strAuditString.IsEmpty())
			s += '\n' + m_strAuditString;
	}
	else if(!m_sErrorDetail.IsEmpty())
		s += '\n' + m_sErrorDetail;

	return s;
}

//-----------------------------------------------------------------------------
BOOL Expression::InitExpressionMaps()
{
	int row, col;
	
	// Initializing all maps at NULL state
	//
	for (row = 0; row <= LAST_MAPPED_DATA_TYPE; row++)
		for (col = 0; col <= LAST_MAPPED_DATA_TYPE; col++)
		{
			PlusMap	[row][col] = DATA_NULL_TYPE;
			MinusMap[row][col] = DATA_NULL_TYPE;
			StarMap	[row][col] = DATA_NULL_TYPE;
			SlashMap[row][col] = DATA_NULL_TYPE;
			PercMap	[row][col] = DATA_NULL_TYPE;
			RelMap	[row][col] = DATA_NULL_TYPE;
		}

	// Initializing binary PLUS(+) operator
	//
	PlusMap	[DATA_STR_TYPE][DATA_STR_TYPE]	= DATA_STR_TYPE;
	PlusMap	[DATA_STR_TYPE][DATA_TXT_TYPE]	= DATA_TXT_TYPE;
	PlusMap	[DATA_TXT_TYPE][DATA_TXT_TYPE]	= DATA_TXT_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_INT_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	PlusMap	[DATA_INT_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	PlusMap	[DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	PlusMap	[DATA_LNG_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	PlusMap	[DATA_LNG_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	PlusMap	[DATA_LNG_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	PlusMap	[DATA_LNG_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	PlusMap	[DATA_DBL_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	PlusMap	[DATA_DBL_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	PlusMap	[DATA_DBL_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	PlusMap	[DATA_DBL_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	PlusMap	[DATA_MON_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	PlusMap	[DATA_QTA_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	PlusMap	[DATA_PERC_TYPE][DATA_PERC_TYPE]= DATA_PERC_TYPE;
	PlusMap	[DATA_DATE_TYPE][DATA_INT_TYPE] = DATA_DATE_TYPE;

	// Initializing binary MINUS(-) operator
	//
	MinusMap [DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_INT_TYPE;
	MinusMap [DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	MinusMap [DATA_INT_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	MinusMap [DATA_INT_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	MinusMap [DATA_INT_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	MinusMap [DATA_INT_TYPE][DATA_PERC_TYPE]= DATA_PERC_TYPE;
	MinusMap [DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	MinusMap [DATA_LNG_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	MinusMap [DATA_LNG_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	MinusMap [DATA_LNG_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	MinusMap [DATA_LNG_TYPE][DATA_PERC_TYPE]= DATA_PERC_TYPE;
	MinusMap [DATA_DBL_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	MinusMap [DATA_DBL_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	MinusMap [DATA_DBL_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	MinusMap [DATA_DBL_TYPE][DATA_PERC_TYPE]= DATA_PERC_TYPE;
	MinusMap [DATA_MON_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	MinusMap [DATA_QTA_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	MinusMap [DATA_PERC_TYPE][DATA_PERC_TYPE]= DATA_PERC_TYPE;
	MinusMap [DATA_DATE_TYPE][DATA_INT_TYPE] = DATA_DATE_TYPE;
	MinusMap [DATA_DATE_TYPE][DATA_DATE_TYPE] = DATA_LNG_TYPE;

	// Initializing binary STAR(*) operator
	//
	StarMap	[DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_INT_TYPE;
	StarMap	[DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	StarMap	[DATA_INT_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_INT_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_INT_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_INT_TYPE][DATA_PERC_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	StarMap	[DATA_LNG_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_LNG_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_LNG_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_LNG_TYPE][DATA_PERC_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_DBL_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_DBL_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_DBL_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_DBL_TYPE][DATA_PERC_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_MON_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_MON_TYPE][DATA_QTA_TYPE]	= DATA_MON_TYPE;
	StarMap	[DATA_MON_TYPE][DATA_PERC_TYPE]	= DATA_MON_TYPE;
	StarMap	[DATA_QTA_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	StarMap	[DATA_QTA_TYPE][DATA_PERC_TYPE]	= DATA_QTA_TYPE;
	StarMap	[DATA_PERC_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;

	// Initializing binary SLASH(/) operator
	//
	SlashMap [DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_INT_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_INT_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_INT_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_INT_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
	SlashMap [DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_LNG_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_LNG_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_LNG_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_LNG_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
	SlashMap [DATA_DBL_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_DBL_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_DBL_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_DBL_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
	SlashMap [DATA_MON_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_MON_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_MON_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
	SlashMap [DATA_QTA_TYPE][DATA_MON_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_QTA_TYPE][DATA_QTA_TYPE]	= DATA_DBL_TYPE;
	SlashMap [DATA_QTA_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
	SlashMap [DATA_PERC_TYPE][DATA_PERC_TYPE]= DATA_DBL_TYPE;
    
	// Initializing binary PERC(%) operator (remainder of SLASH operator)
	//
	PercMap [DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_INT_TYPE;
	PercMap [DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_INT_TYPE;
	PercMap [DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;

	// Initializing binary RELATION( == <= >= !=) operators
	//
	RelMap [DATA_INT_TYPE][DATA_INT_TYPE]	= DATA_INT_TYPE;
	RelMap [DATA_INT_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	RelMap [DATA_INT_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	RelMap [DATA_INT_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	RelMap [DATA_INT_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	RelMap [DATA_INT_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	RelMap [DATA_LNG_TYPE][DATA_LNG_TYPE]	= DATA_LNG_TYPE;
	RelMap [DATA_LNG_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	RelMap [DATA_LNG_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	RelMap [DATA_LNG_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	RelMap [DATA_LNG_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	RelMap [DATA_DBL_TYPE][DATA_DBL_TYPE]	= DATA_DBL_TYPE;
	RelMap [DATA_DBL_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	RelMap [DATA_DBL_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	RelMap [DATA_DBL_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	RelMap [DATA_MON_TYPE][DATA_MON_TYPE]	= DATA_MON_TYPE;
	RelMap [DATA_QTA_TYPE][DATA_QTA_TYPE]	= DATA_QTA_TYPE;
	RelMap [DATA_PERC_TYPE][DATA_PERC_TYPE]	= DATA_PERC_TYPE;
	RelMap [DATA_BOOL_TYPE][DATA_BOOL_TYPE]	= DATA_BOOL_TYPE;
	RelMap [DATA_STR_TYPE][DATA_STR_TYPE]	= DATA_STR_TYPE;
	RelMap [DATA_DATE_TYPE][DATA_DATE_TYPE]	= DATA_DATE_TYPE;
	RelMap [DATA_ENUM_TYPE][DATA_ENUM_TYPE]	= DATA_ENUM_TYPE;
	RelMap [DATA_GUID_TYPE][DATA_GUID_TYPE]	= DATA_GUID_TYPE;
	RelMap [DATA_GUID_TYPE][DATA_STR_TYPE]	= DATA_GUID_TYPE;
	RelMap [DATA_TXT_TYPE][DATA_TXT_TYPE]	= DATA_TXT_TYPE;
	RelMap [DATA_TXT_TYPE][DATA_STR_TYPE]	= DATA_TXT_TYPE;	

	RelMap [DATA_ARRAY_TYPE][DATA_ARRAY_TYPE]	= DATA_ARRAY_TYPE;	

	RelMap [DATA_RECORD_TYPE][DATA_RECORD_TYPE]	= DATA_RECORD_TYPE;	
	RelMap [DATA_SQLRECORD_TYPE][DATA_SQLRECORD_TYPE] = DATA_SQLRECORD_TYPE;

	for (col = 0; col <= LAST_MAPPED_DATA_TYPE; col++)
	{
		PlusMap	[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
		MinusMap[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
		StarMap	[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
		SlashMap[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
		PercMap	[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
		RelMap	[DATA_VARIANT_TYPE][col] = DATA_VARIANT_TYPE;
	}

	// Copy the Values of the simmetric maps,
	// example : int + long == long + int.
	//
	for (row = 0; row <= LAST_MAPPED_DATA_TYPE; row++)
	{
		for (col = 0; col <= LAST_MAPPED_DATA_TYPE; col++)
		{
			if (row == col)
				continue;
			
			if	(
					PlusMap[col][row] == DATA_NULL_TYPE &&
					PlusMap[row][col] != DATA_NULL_TYPE
				)
				PlusMap[col][row] = PlusMap[row][col];

			if	(
					MinusMap[col][row] == DATA_NULL_TYPE &&
					MinusMap[row][col] != DATA_NULL_TYPE &&
					MinusMap[row][col] != DATA_DATE_TYPE
				)
				MinusMap[col][row] = MinusMap[row][col];

			if	(
					StarMap[col][row] == DATA_NULL_TYPE &&
					StarMap[row][col] != DATA_NULL_TYPE
				)
				StarMap[col][row] = StarMap[row][col];
				
			if	(
					SlashMap[col][row] == DATA_NULL_TYPE &&
					SlashMap[row][col] != DATA_NULL_TYPE
				)
				SlashMap[col][row] = SlashMap[row][col];
				
			if	(
					PercMap[col][row] == DATA_NULL_TYPE &&
					PercMap[row][col] != DATA_NULL_TYPE
				)
				PercMap[col][row] = PercMap[row][col];

			if	(
					RelMap[col][row] == DATA_NULL_TYPE &&
					RelMap[row][col] != DATA_NULL_TYPE
				)
				RelMap[col][row] = RelMap[row][col];
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void Expression::Assign(const Expression& aExp)
{
	ExpParse::DupStack(aExp.m_ExprStack, m_ExprStack);

	m_pSymTable			= aExp.m_pSymTable;
	m_strExprString		= aExp.m_strExprString;
	m_strAuditString	= aExp.m_strAuditString;
	m_nParseStartLine	= aExp.m_nParseStartLine;

	if (aExp.m_pStopTokens)
	{
		if (m_pStopTokens)
		{
			m_pStopTokens->Assign(*(aExp.m_pStopTokens));
		}
		else
			m_pStopTokens = new CStopTokens(*(aExp.m_pStopTokens));
	}
	else
	{
		SAFE_DELETE(m_pStopTokens);
	}

	m_nErrorPos	= -1;
	m_nErrorID	= EMPTY_MESSAGE;
	m_sErrorDetail.Empty();

	m_bVrbCompiled			= aExp.m_bVrbCompiled;
	m_bHasFields			= aExp.m_bHasFields;
	m_bHasRuleFields		= aExp.m_bHasRuleFields;
	m_bHasInputFields		= aExp.m_bHasInputFields;
	m_bHasAskFields			= aExp.m_bHasAskFields;
	m_bHasExternalFunctionCall	= aExp.m_bHasExternalFunctionCall;
	m_bHasDynamicFragment		= aExp.m_bHasDynamicFragment;
}

//-----------------------------------------------------------------------------
BOOL Expression::IsEqual(const Expression& expr) const
{
	TRACE("Expression::IsEqual( %s, %s )\n", (LPCTSTR)m_strExprString, (LPCTSTR)expr.m_strExprString);
	
	return ExpParse::CompareStack(m_ExprStack, expr.m_ExprStack);
}

// Serve per segnalare al parser dell'espressione quali sono i possibili
// token che terminano l'espressione in esame
//-----------------------------------------------------------------------------
void Expression::SetStopTokens(Token st1, Token st2, Token st3, Token st4, Token st5)
{
	if (m_pStopTokens != NULL)
		m_pStopTokens->RemoveAll();
	else
		m_pStopTokens = new CStopTokens;

	if (st1 != T_NOTOKEN) m_pStopTokens->Add((WORD)st1);
	if (st2 != T_NOTOKEN) m_pStopTokens->Add((WORD)st2);
	if (st3 != T_NOTOKEN) m_pStopTokens->Add((WORD)st3);
	if (st4 != T_NOTOKEN) m_pStopTokens->Add((WORD)st4);
	if (st5 != T_NOTOKEN) m_pStopTokens->Add((WORD)st5);
}

//-----------------------------------------------------------------------------
void Expression::AddStopTokens(Token st1)
{
	if (m_pStopTokens == NULL)
		m_pStopTokens = new CStopTokens;

	if (st1 != T_NOTOKEN) m_pStopTokens->Add((WORD)st1);
}

//-----------------------------------------------------------------------------
void Expression::SetStopTokens(Token arst[], int nSize)
{
	if (m_pStopTokens != NULL)
		m_pStopTokens->RemoveAll();
	else
		m_pStopTokens = new CStopTokens;

	for (int i = 0; i < nSize; i++) 
		m_pStopTokens->Add((WORD)arst[i]);
}

// viene usata per determinare se l'espressione contiene un determinata variabile
//-----------------------------------------------------------------------------
BOOL Expression::HasMember(LPCTSTR pattern, const Stack& stack) const
{
	int num = stack.GetSize();
	for (int i = 0; i < num; i++)
	{
		ExpItem* item = (ExpItem*) stack[i];
		switch (item->IsA())
		{
			case EXP_ITEM_VRB_CLASS	:
			{
				if (((ExpItemVrb*)item)->m_strNameVrb.CompareNoCase(pattern) == 0)
					return TRUE;

				break;
			}
			case EXP_ITEM_OPE_CLASS	:
			{
				if	(
					HasMember(pattern, ((ExpItemOpe*)item)->m_frstOpStack) ||
					HasMember(pattern, ((ExpItemOpe*)item)->m_scndOpStack)
					)
					return TRUE;

				break;
			}
			case EXP_ITEM_FUN_CLASS	:
			{
				if (item->IsKindOf(RUNTIME_CLASS(ExpItemContentOfFun)))
				{
					ExpItemContentOfFun* cof = (ExpItemContentOfFun*) item;
					if (cof->m_pExpr && HasMember(pattern, *(cof->m_pExpr)))
						return TRUE;
				}
				break;
			}
			case EXP_ITEM_VAL_CLASS:
			{
				if (item->IsKindOf(RUNTIME_CLASS(ExpItemValFromVar)))
				{
					ExpItemValFromVar* vv = (ExpItemValFromVar*)item;
					CString name = vv->m_strNameVrb;
					if (!name.IsEmpty())
						if (name.CompareNoCase(pattern) == 0)
							return TRUE;
				}
				if (m_bHasDynamicFragment)
				{
					ExpItemVal* pIVal = dynamic_cast<ExpItemVal*>(item);
					ASSERT_VALID(pIVal);
					if (pIVal->m_pVal->GetDataType() == DataType::String && !item->IsKindOf(RUNTIME_CLASS(ExpItemValWC)))
					{
						ASSERT_KINDOF(WClauseExpr, this);

						WClauseExpr we(NULL, GetSymTable());
						we.SetNative(TRUE);
						Parser	lex(pIVal->m_pVal->Str());
						if (we.ParseNative(lex))
						{
							if (we.HasMember(pattern))
								return TRUE;
						}
						else
						{
							lex.ClearError();
						}
					}
				}
				break;
			}
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void Expression::GetParameters(CStringArray& ar, const Stack& stack, BOOL bReverse/* = TRUE*/) const
{
	int start = ar.GetSize();

	int num = stack.GetSize();
	for (int i = 0; i < num; i++)
	{
		ExpItem* item = (ExpItem*) stack[i];
		switch (item->IsA())
		{
			case EXP_ITEM_VRB_CLASS:
			{
				CString name = ((ExpItemVrb*)item)->m_strNameVrb;
				if (m_pSymTable && m_pSymTable->GetField(name))
					ar.Add(name);
			
				break;
			}
			case EXP_ITEM_OPE_CLASS:
			{
				GetParameters(ar, ((ExpItemOpe*)item)->m_frstOpStack);
				GetParameters(ar, ((ExpItemOpe*)item)->m_scndOpStack);

				break;
			}
			case EXP_ITEM_FUN_CLASS:
			{
				if (item->IsKindOf(RUNTIME_CLASS(ExpItemContentOfFun)))
				{
					ExpItemContentOfFun* cof = (ExpItemContentOfFun*)item;
					if (cof->m_pExpr)
						GetParameters(ar, *(cof->m_pExpr));
				}
				break;
			}
			case EXP_ITEM_VAL_CLASS:
			{
				if (item->IsKindOf(RUNTIME_CLASS(ExpItemValFromVar)))
				{
					ExpItemValFromVar* vv = (ExpItemValFromVar*)item;
					CString name = vv->m_strNameVrb;
					if (!name.IsEmpty())
						if (m_pSymTable && m_pSymTable->GetField(name))
							ar.Add(name);
				}
				if (m_bHasDynamicFragment)
				{
					ExpItemVal* pIVal = dynamic_cast<ExpItemVal*>(item);
					ASSERT_VALID(pIVal);

					if (
						pIVal->m_pVal->GetDataType() == DataType::String &&
						!item->IsKindOf(RUNTIME_CLASS(ExpItemValWC)) &&
						!item->IsKindOf(RUNTIME_CLASS(ExpItemContentOfParamVal)) 
						)
					{
						ASSERT_KINDOF(WClauseExpr, this);

						WClauseExpr we(NULL, GetSymTable());
						we.SetNative(TRUE);
						CString s = pIVal->m_pVal->Str();
						Parser	lex(s);
						if (we.ParseNative(lex))
							we.GetParameters(ar, FALSE);
						else
						{
							lex.ClearError();
						}
					}
				}
				break;
			}
		}
	}

	//devo invertire l'ordine
	if (bReverse && ar.GetSize() > (start + 1))
	{
		for (int end = ar.GetUpperBound(); start < end; start++, end--)
		{
			CString send = ar[end];
			ar[end] = ar[start];
			ar[start] = send;
		}
	}
}

//-----------------------------------------------------------------------------
// viene usata per determinare se l'espressione contiene un determinata variabile
BOOL Expression::HasMember(LPCTSTR sVarName) const
{
	return HasMember(sVarName, m_ExprStack);
}

// ritorna l'elenco delle variabili referenziate
void Expression::GetParameters(CStringArray& ar, BOOL bReverse/* = TRUE*/) const
{
	GetParameters(ar, m_ExprStack, bReverse);
}

//-----------------------------------------------------------------------------
BOOL Expression::ErrHandler(MessageID anID, ExpItem* o1, ExpItem* o2, ExpItem* o3)
{
	if (m_nErrorID == EMPTY_MESSAGE)
	{
		m_nErrorID	= anID;
		ExpItem* o	= (o1 ? o1 : (o2 ? o2 : o3));
	
		if (o && o->m_nPosInStr != -1)
			m_nErrorPos = o->m_nPosInStr;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void Expression::Space(CString& str, int n)
{
	Replicate(str, n, CString(L" "));
}

void Expression::Replicate(CString& str, int n, const CString& rep)
{
	str.Empty();
	for (int i = 0; i < n; i++)
		str += rep;
}

//-----------------------------------------------------------------------------
double Expression::Round(double d, int nDec/*= 0*/)
{
	::round(d, nDec);
	
	return d;
}

//-----------------------------------------------------------------------------
void Expression::Reset(BOOL bResetAll /*=TRUE*/)
{
	m_strExprString.Empty();
	m_strAuditString.Empty();
	m_nParseStartLine = 0;
	m_sErrorDetail.Empty();

	m_ExprStack.ClearStack();   
	
	if (bResetAll)
		m_pSymTable = NULL;
}

//-----------------------------------------------------------------------------
CString Expression::ToString (BOOL bUseAudit)
{ 
	if (bUseAudit)
	{
		CString s = m_strExprString; s.Trim(); 
		return s.IsEmpty() ?  m_strAuditString : m_strExprString;
	}

	CString str;
	ExpUnparse expUnparse;
	VERIFY(expUnparse.Unparse(str, m_ExprStack));
	return str;
}

//Expression::operator LPCTSTR () const
//{ 
//	return const_cast<Expression*>(this)->ToString();
//}
//-----------------------------------------------------------------------------
BOOL Expression::Parse(CString sExpr, const DataType& aResultType, BOOL bKeepString)
{
	Parser lex(sExpr);

	return Parse(lex, aResultType, bKeepString);
}

//-----------------------------------------------------------------------------
BOOL Expression::Parse(Parser& lex, const DataType& aResultType, BOOL bKeepString)
{
	m_nParseStartLine = lex.GetCurrentLine();

	m_ExprStack.ClearStack();

	BOOL bAudit = lex.IsAuditStringOn();
	CString sAudit = lex.GetAuditString();

	ExpParse expParse(m_pSymTable, m_pStopTokens);
	expParse.m_bWClause = IsKindOf(RUNTIME_CLASS(WClauseExpr));

	if (!expParse.Parse(lex, m_ExprStack, TRUE))
	{
		lex.EnableAuditString(bAudit);
		if (!sAudit.IsEmpty())
			lex.ConcatAuditString(sAudit);
		return FALSE;
	}

	if (bAudit)
	{
		lex.EnableAuditString();
		if (!sAudit.IsEmpty())
			lex.ConcatAuditString(sAudit);
	}

	m_bHasFields				= expParse.HasFields();
	m_bHasRuleFields			= expParse.HasRuleFields();
	m_bHasInputFields			= expParse.HasInputFields();
	m_bHasAskFields				= expParse.HasAskFields();
	m_bHasExternalFunctionCall	= expParse.HasExternalFunctionCall();
	m_bHasDynamicFragment		= expParse.HasDynamicFragment();

	m_strExprString.Empty();
	if (bKeepString)
	{
		m_strExprString = expParse.GetString();
		m_strExprString.TrimRight(L"\r\n");
		StripBlankNearSquareBrackets(m_strExprString);
	}
	else
	{
		m_strAuditString = expParse.GetString();
		m_strAuditString.TrimRight(L"\r\n");
		StripBlankNearSquareBrackets(m_strAuditString);

		if (bAudit)
		{
			if (!m_strAuditString.IsEmpty())
				lex.ConcatAuditString(m_strAuditString);
		}
	}

	if (aResultType == DATA_NULL_TYPE)	
		return TRUE;
	if (m_ExprStack.IsEmpty()) 			
		return lex.SetError(FormatMessage (EXPREMPTY));

	DataType t = CompileOK(lex, m_ExprStack);
	if (!Compatible(t, aResultType))
		return lex.SetError(FormatMessage(RETTYPE), CString(L' ' + _TB("The data type expected is:") + L' ' + aResultType.ToString()));

	return TRUE;
}

// Determina se l'espressione parsata e` anche valutabile correttamente
//-----------------------------------------------------------------------------
DataType Expression::CompileOK(Parser& lex, Stack& aExprStack)
{
	Stack	workStack,tmpStack;

	ExpParse::MoveStack(aExprStack, workStack);
	if (!Compile(lex, workStack, tmpStack))
		return DATA_NULL_TYPE;

	ExpItemVal* itm = (ExpItemVal*) workStack.Pop();
	if (itm == NULL || itm->IsA() != EXP_ITEM_VAL_CLASS)
	{
		if (itm) delete itm;

		lex.SetError(FormatMessage(RETTYPE));
		return DATA_NULL_TYPE;
	}

	while (!tmpStack.IsEmpty())
		aExprStack.Push(tmpStack.Pop());

	DataType ret = itm->GetDataType();
	delete itm;

	return ret;
}

//-----------------------------------------------------------------------------
// Valuta l'espressione
//ci sono due metodi quasi uguali ma è difficile unificarli perchè differiscono per alcune righe
//il metodo con il dataobj reference è quello storico ed è virtuale quindi potrebbe essere stato reimplementato

BOOL Expression::Eval(DataObj& d)
{
	m_nErrorPos	= -1;
	m_nErrorID = EMPTY_MESSAGE;
	m_sErrorDetail.Empty();
	
	if (m_ExprStack.IsEmpty()) 
		return TRUE;

	ExpItemVal* pItmVal = EvalOK(m_ExprStack);
	if (!pItmVal) 
		return FALSE;

	if (!Compatible(pItmVal->GetDataType(), d.GetDataType()))
	{
		if (pItmVal->m_pVal)
		{
			m_sErrorDetail += ' ' + cwsprintf(
				_TB("Evaluated expression has {0-%s} data type but it is incompatible with expected data type {1-%s}"), 
				Unparser::DataTypeToString(pItmVal->GetDataType()), 
				Unparser::DataTypeToString(d.GetDataType())
				);
			m_sErrorDetail += '\n' +  cwsprintf(_TB("Expression result value is {0-%s}"), pItmVal->m_pVal->Str());
		}
		else
			m_sErrorDetail += ' ' + _TB("Expression result value is null");

		ErrHandler(RETTYPE, pItmVal);
	}
	else if (!pItmVal->m_pVal)
	{
		ErrHandler(NULL_OPR, pItmVal);
	}
	else if (!pItmVal->m_pVal->IsValid())
	{
		ErrHandler(NULL_OPR, pItmVal);
	}
	else
		AssignResult(d, *pItmVal);

	if (pItmVal->m_bToBeDeleted)
		delete pItmVal;

	return GetErrId() == EMPTY_MESSAGE;
}

//-----------------------------------------------------------------------------
BOOL Expression::Eval(DataObj*& p)
{
	m_nErrorPos	= -1;
	m_nErrorID = EMPTY_MESSAGE;
	m_sErrorDetail.Empty();

	if (m_ExprStack.IsEmpty()) 
		return TRUE;

	ExpItemVal* pItmVal = EvalOK(m_ExprStack);
	if (!pItmVal) 
		return FALSE;

	//----
	if (p == NULL && pItmVal->m_bVoid)
	{
		goto l_Eval_end;
	}
	if (p == NULL)
		p = DataObj::DataObjCreate(pItmVal->GetDataType());
	//----

	if (!Compatible(pItmVal->GetDataType(), p->GetDataType()))
	{
		if (pItmVal->m_pVal)
		{
			m_sErrorDetail += ' ' + cwsprintf(
				_TB("Evaluated expression has {0-%s} data type but it is incompatible with expected data type {1-%s}"), 
				Unparser::DataTypeToString(pItmVal->GetDataType()), 
				Unparser::DataTypeToString(p->GetDataType())
				);
			m_sErrorDetail += '\n' +  cwsprintf(_TB("Expression result value is {0-%s}"), pItmVal->m_pVal->Str());
		}
		else
			m_sErrorDetail += ' ' + _TB("Expression result value is null");

		ErrHandler(RETTYPE, pItmVal);
	}
	else if (!pItmVal->m_pVal)
		ErrHandler(NULL_OPR, pItmVal);
	else if (!pItmVal->m_pVal->IsValid())
		ErrHandler(NULL_OPR, pItmVal);
	else
		AssignResult(*p, *pItmVal);

l_Eval_end:
	if (pItmVal->m_bToBeDeleted)
		delete pItmVal;

	return GetErrId() == EMPTY_MESSAGE;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::EvalOK(const Stack& aExprStack)
{
	Stack workStack;
	workStack.SetOwns(FALSE);

	// copia lo stack in un'altro di lavoro in modo che quello
	// originario (che possiede gli items) non venga modificato 
	//
	int i = 0;
	for (i = 0; i <= aExprStack.GetUpperBound(); i++)
		workStack.Add(aExprStack[i]);

	if (!Execute(workStack))
	{
		for (i = 0; i <= workStack.GetUpperBound(); i++)
		{
			ExpItem* itm = (ExpItem*) workStack[i];
			if	(
					itm && itm->IsA() == EXP_ITEM_VAL_CLASS &&
					((ExpItemVal*)itm)->m_bToBeDeleted
				)
				delete itm;
		}

		return NULL;
	}

	ExpItem* itm = (ExpItem*) workStack.Top();

	if (itm == NULL || itm->IsA() != EXP_ITEM_VAL_CLASS)
	{
		ErrHandler(UNKNOW, itm);
		ASSERT(FALSE);
		return NULL;
	}

	return (ExpItemVal*) workStack.Pop();
}

//-----------------------------------------------------------------------------
void Expression::AssignResult(DataObj& d, ExpItemVal& res)
{
	if	(
			d.GetDataType() == res.GetDataType() ||
			(
				d.GetDataType().m_wType == DATA_DATE_TYPE &&
				res.GetDataType().m_wType == DATA_DATE_TYPE
			)
		)
	{
		if (d.GetDataType() == DataType::Array)
		{
			if (!Compatible(((DataArray*)res.m_pVal)->GetBaseDataType(), ((DataArray&)d).GetBaseDataType()))
			{
				m_sErrorDetail += ' ' + cwsprintf(
						_TB("Evaluated expression has {0-%s} Array data type but it is incompatible with expected {1-%s} Array data type "), 
						Unparser::DataTypeToString(((DataArray*)res.m_pVal)->GetBaseDataType()), 
						Unparser::DataTypeToString(((DataArray&)d).GetBaseDataType())
						);
		
				ErrHandler(RETTYPE, &res);
				return;
			}
		}
		d.Assign( *(res.m_pVal) );
		d.SetValid(res.m_pVal->IsValid());
		return;
	}

	switch (d.GetDataType().m_wType)
	{
		case DATA_STR_TYPE	: ((DataStr&) d).	Assign( CastStr (res) );	break;
		case DATA_INT_TYPE	: ((DataInt&) d).	Assign( CastInt (res) );	break;
		case DATA_BOOL_TYPE	: ((DataInt&) d).	Assign( CastInt (res) );	break;
		case DATA_LNG_TYPE	: ((DataLng&) d).	Assign( CastLng (res) );	break;
		case DATA_ENUM_TYPE	: ((DataEnum&) d).	Assign( (DWORD)CastLng (res) );	break;
		case DATA_DBL_TYPE	: ((DataDbl&) d).	Assign( CastDbl (res) );	break;
		case DATA_MON_TYPE	: ((DataMon&) d).	Assign( CastDbl (res) );	break;
		case DATA_QTA_TYPE	: ((DataQty&) d).	Assign( CastDbl (res) );	break;
		case DATA_PERC_TYPE	: ((DataPerc&) d).	Assign( CastDbl (res) );	break;
		case DATA_GUID_TYPE	: ((DataGuid&) d).	Assign( CastGuid(res) );	break;
		case DATA_TXT_TYPE	: ((DataText&) d).	Assign( CastTxt (res) );	break;	
		case DATA_ARRAY_TYPE	: ((DataArray&) d).		Assign( * CastArray (res) );	break;	
		case DATA_SQLRECORD_TYPE	: ((DataSqlRecord&) d).	Assign( * CastSqlRecord (res) );	break;	
		case DATA_RECORD_TYPE: ((DataRecord&)d).Assign(*CastRecord(res));	break;
		default: TRACE(_T(" Il tipo del DataObj che deve contenere il risultato dell’espressione non è dei tipi supportati.")); ASSERT(FALSE);		// se succede vuol dire che le mappe di compatibilita` sono errate
	} // switch
	
	d.SetValid(res.m_pVal->IsValid());
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::ReturnFunDataObj(int fun)
{
	DataType c;
	if (!return_func_type(fun, c))
		return NULL;
	
	if (c == DataType::Variant)
		return new ExpItemVal(NULL, 0, TRUE, TRUE, FALSE, TRUE);

	return new ExpItemVal(DataObj::DataObjCreate(c), 0, TRUE);
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::CheckType
			(
				Parser& lex,
				ExpItemOpe* pItemOpe,
				ExpItemVal* o1,
				ExpItemVal* o2	/* = NULL */,
				ExpItemVal* o3	/* = NULL */
			)
{
	DataType resultType;

	if (pItemOpe->GetType() == LOGICAL_OPR)
	{
		ASSERT(o2 == NULL && o3 == NULL);

		resultType = CompileOK(lex, pItemOpe->m_scndOpStack);
		if (resultType != DATA_NULL_TYPE)
		{
			if (pItemOpe->m_nOpe == T_BETWEEN || pItemOpe->m_nOpe == T_QUESTION_MARK)
			{
				if (resultType == DataType::Variant)
					o3 = new ExpItemVal(NULL, 0, TRUE, FALSE, FALSE, TRUE);
				else
					o3 = new ExpItemVal(DataObj::DataObjCreate(resultType), 0, TRUE);

				resultType = CompileOK(lex, pItemOpe->m_frstOpStack);
				if (resultType != DATA_NULL_TYPE)
				{
					if (resultType == DataType::Variant)
						o2 = new ExpItemVal(NULL, 0, TRUE, FALSE, FALSE, TRUE);
					else
						o2 = new ExpItemVal(DataObj::DataObjCreate(resultType), 0, TRUE);
				}
			}
			else
			{
				if (resultType == DataType::Variant)
					o2 = new ExpItemVal(NULL, 0, TRUE, FALSE, FALSE, TRUE);
				else
					o2 = new ExpItemVal(DataObj::DataObjCreate(resultType), 0, TRUE);
			}
		}
		//else ASSERT(FALSE);
	}

	resultType = GiveMeResultType(pItemOpe, o1, o2, o3);
	
	if (resultType == DATA_NULL_TYPE)
	{	
		lex.SetError(FormatMessage (CHECKTYPE), _T(""), o1 ? o1->m_nPosInStr : -1);
	}

	if (o1 && o1->m_bToBeDeleted) delete o1;
	if (o2 && o2->m_bToBeDeleted) delete o2;
	if (o3 && o3->m_bToBeDeleted) delete o3;

	if (resultType == DATA_NULL_TYPE)
	{	
		return NULL;
	}

	pItemOpe->m_ResultType = resultType;

	if (resultType == DataType::Variant)
		return new ExpItemVal(NULL, 0, TRUE, TRUE, FALSE, TRUE);
	if (resultType == DataType::Void)
		return new ExpItemVal(NULL, 0, TRUE, TRUE, TRUE);

	return new ExpItemVal(DataObj::DataObjCreate(resultType), 0, TRUE);
}

//-----------------------------------------------------------------------------
DataType Expression::GiveMeResultTypeForMathOpMap(DATA_TYPE_OP_MAP& map, ExpItemVal* pOpr1, ExpItemVal* pOpr2)
{
	return map[pOpr1->GetDataType().m_wType][pOpr2->GetDataType().m_wType];
}

//-----------------------------------------------------------------------------
DataType Expression::GiveMeResultTypeForBitwiseOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2 /* = NULL */)
{
	if	(
			(!pOpr2 || pOpr1->GetDataType() == pOpr2->GetDataType()) &&
			(
				pOpr1->GetDataType() == DATA_INT_TYPE ||
				pOpr1->GetDataType() == DATA_LNG_TYPE
			)
		)
		return pOpr1->GetDataType();

	if	(
			pOpr1->GetDataType() == DATA_VARIANT_TYPE ||
	 		(!pOpr2 && pOpr2->GetDataType() == DATA_VARIANT_TYPE)
	 	)
		return DATA_VARIANT_TYPE;

	return DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
DataType Expression::GiveMeResultTypeForLogicalOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2 /* = NULL */)
{
	if	(
			pOpr1->GetDataType() == DATA_BOOL_TYPE &&
	 		(!pOpr2 || pOpr2->GetDataType() == DATA_BOOL_TYPE)
	 	)
		return DATA_BOOL_TYPE;

	if	(
			pOpr1->GetDataType() == DATA_VARIANT_TYPE ||
	 		(!pOpr2 && pOpr2->GetDataType() == DATA_VARIANT_TYPE)
	 	)
		return DATA_VARIANT_TYPE;

	return DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
DataType Expression::GiveMeResultTypeForLikeOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2)
{
	if	(
			(pOpr1->GetDataType() == DATA_STR_TYPE || pOpr1->GetDataType() == DATA_TXT_TYPE) &&
			(pOpr2->GetDataType() == DATA_STR_TYPE || pOpr2->GetDataType() == DATA_TXT_TYPE)
		)
		return DATA_BOOL_TYPE;

	if	(
			pOpr1->GetDataType() == DATA_VARIANT_TYPE ||
			pOpr2->GetDataType() == DATA_VARIANT_TYPE 
		)
		return DATA_VARIANT_TYPE;

	return DATA_NULL_TYPE;
}

// Return the dataType result, if it is equal to DATA_NULL_TYPE
// then operation is not possibile.
//
//-----------------------------------------------------------------------------
DataType Expression::GiveMeResultType
					(
						ExpItemOpe* pItemOpe,
						ExpItemVal* pOpr1,
						ExpItemVal* pOpr2,
						ExpItemVal* pOpr3
					)
{
	if (
		(pOpr1 && pOpr1->m_bVariant) ||
		(pOpr2 && pOpr2->m_bVariant) ||
		(pOpr3 && pOpr3->m_bVariant) 
		)
		return DATA_VARIANT_TYPE;

	switch (pItemOpe->GetType())
	{
		case UNARY_OPR   :
		{
			if (pOpr1 == NULL) 
				return DATA_NULL_TYPE;

			switch (pItemOpe->m_nOpe)
			{
				case TT_UNMINUS	:
					switch (pOpr1->GetDataType().m_wType)
					{
						case DATA_INT_TYPE	:
						case DATA_LNG_TYPE	:
						case DATA_DBL_TYPE	:
						case DATA_MON_TYPE	:
						case DATA_QTA_TYPE	:
						case DATA_PERC_TYPE	:	
							return pOpr1->GetDataType();
					}
					return DATA_NULL_TYPE;
		
				case T_NOT		:
				case T_OP_NOT	:
					return GiveMeResultTypeForLogicalOp(pOpr1);
					
				case T_BW_NOT	:
					return GiveMeResultTypeForBitwiseOp(pOpr1);

				case TT_IS_NULL	:
				case TT_IS_NOT_NULL	:
					return DATA_BOOL_TYPE;
			}			

			return DATA_NULL_TYPE;
		}
		case BINARY_OPR  :
		{
			if (pOpr1 == NULL || pOpr2 == NULL) 
				return DATA_NULL_TYPE;

			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		:
					return GiveMeResultTypeForMathOpMap	(PlusMap, pOpr1, pOpr2);

				case T_MINUS	:
					return GiveMeResultTypeForMathOpMap	(MinusMap, pOpr1, pOpr2);

				case T_STAR		:
					return GiveMeResultTypeForMathOpMap	(StarMap, pOpr1, pOpr2);

				case T_SLASH	:
					return GiveMeResultTypeForMathOpMap	(SlashMap, pOpr1, pOpr2);

				case T_PERC		:
					return GiveMeResultTypeForMathOpMap	(PercMap, pOpr1, pOpr2);

				case T_LIKE		:
					return GiveMeResultTypeForLikeOp(pOpr1, pOpr2);

				case T_GT		:
				case T_GE		:
				case T_LT		:
				case T_LE		:
				case T_NE		:
				case T_DIFF		:
				case T_EQ		:
				case T_ASSIGN	:
				{
					if	(
							(
								pOpr1->GetDataType() == DATA_DATE_TYPE && pOpr2->GetDataType() == DATA_DATE_TYPE &&
								pOpr1->GetDataType().IsATime() != pOpr2->GetDataType().IsATime()
							) 
							||
							GiveMeResultTypeForMathOpMap (RelMap, pOpr1, pOpr2) == DATA_NULL_TYPE
						)
						break;

					return DATA_BOOL_TYPE;
				}
				case T_BW_AND	:
				case T_BW_OR	:
				case T_BW_XOR	:
					return GiveMeResultTypeForBitwiseOp(pOpr1);

				case T_CONTAINS	:
				{
                    //if (pOpr1->GetDataType() == DataType::Array)
						return DATA_BOOL_TYPE;
				}
                case T_IN:
				{
                    //if (pOpr2->GetDataType() == DataType::Array || pOpr2->GetDataType() == DataType::String)
						return DATA_BOOL_TYPE;
				}

			}

			return DATA_NULL_TYPE;
        }
		case LOGICAL_OPR :
		{
			if (pOpr1 == NULL || pOpr2 == NULL) return DATA_NULL_TYPE;

			switch (pItemOpe->m_nOpe)
			{
				case T_AND		:
				case T_OP_AND	:
				case T_OR		:
				case T_OP_OR	:
					return GiveMeResultTypeForLogicalOp(pOpr1, pOpr2);

				case T_BETWEEN	:
				{
					if (pOpr3 == NULL) 
						return DATA_NULL_TYPE;

					if	(
							pOpr1->GetDataType() == DATA_DATE_TYPE &&
							pOpr2->GetDataType() == DATA_DATE_TYPE &&
							pOpr3->GetDataType() == DATA_DATE_TYPE &&
							pOpr1->GetDataType().IsATime() != pOpr2->GetDataType().IsATime() &&
							pOpr1->GetDataType().IsATime() != pOpr3->GetDataType().IsATime()
						)
						break; 

					if	(
							GiveMeResultTypeForMathOpMap(RelMap, pOpr1, pOpr2) != DATA_NULL_TYPE &&
							GiveMeResultTypeForMathOpMap(RelMap, pOpr1, pOpr3) != DATA_NULL_TYPE
						)
						return DATA_BOOL_TYPE;
					break;
				}
				case T_QUESTION_MARK:
				{
					if (pOpr3 == NULL) return DATA_NULL_TYPE;

					if	(
							pOpr1->GetDataType().m_wType == DATA_BOOL_TYPE &&
							Compatible(pOpr2->GetDataType(), pOpr3->GetDataType())
						)
					{
						DataType dt = pOpr2->GetDataType();
						if (dt == DataType::Variant)
							return pOpr3->GetDataType();
						return dt;
					}
					break;
				}
			}
			return DATA_NULL_TYPE;
		}				
		case TERNARY_OPR :
		{
			if (pOpr1 == NULL || pOpr2 == NULL || pOpr3 == NULL) 
				return DATA_NULL_TYPE;

			switch (pItemOpe->m_nOpe)
			{
				case TT_ESCAPED_LIKE:
					if	(
							GiveMeResultTypeForLikeOp(pOpr1, pOpr2) != DATA_NULL_TYPE &&
							(pOpr3->GetDataType() == DATA_STR_TYPE || pOpr3->GetDataType() == DATA_TXT_TYPE)
						)
						return DATA_BOOL_TYPE;
					break;
			}
			return DATA_NULL_TYPE;
		}
	}
	return DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
BOOL Expression::CastBool(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataBool)))
		return ((BOOL) (DataBool&) *(d.m_pVal));

	ASSERT(FALSE);
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CString Expression::CastStr(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
		return ((DataStr*) d.m_pVal)->GetString();

	ASSERT(FALSE);
	
	return _T("");
}

//-----------------------------------------------------------------------------
CString Expression::CastTxt(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataText)))
		return ((DataText*) d.m_pVal)->GetString();

	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
		return ((DataStr*) d.m_pVal)->GetString();

	ASSERT(FALSE);
	
	return _T("");
}

//-----------------------------------------------------------------------------
DataArray* Expression::CastArray(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataArray)))
		return ((DataArray*) d.m_pVal);

	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------------

DataRecord* Expression::CastRecord(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataRecord)))
		return ((DataRecord*) d.m_pVal);

	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------------
DataSqlRecord* Expression::CastSqlRecord(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataSqlRecord)))
		return ((DataSqlRecord*)d.m_pVal);

	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------------
SymField* Expression::CastFieldProvider(ExpItemVal& d)
{
	ASSERT_KINDOF(ExpItemValFromVar, &d);
	ExpItemValFromVar* vp = dynamic_cast<ExpItemValFromVar*> (&d);

	ASSERT_VALID(GetSymTable());
	SymField* pField = GetSymTable()->GetField(vp->m_strNameVrb);
	ASSERT_VALID(pField);
	if (!pField)
		return NULL;

	ASSERT(pField->GetProvider());
	if (!pField->GetProvider())
		return NULL;

	return pField;
}

//-----------------------------------------------------------------------------
GUID Expression::CastGuid(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
		return StringToGuid(((DataStr*) d.m_pVal)->GetString());

	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataGuid)))
		return ((DataGuid*) d.m_pVal)->GetGUID();

	ASSERT(FALSE);
	
	return NULL_GUID;
}

//-----------------------------------------------------------------------------
int Expression::CastInt(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
		return ((int) (DataInt&) *(d.m_pVal));
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
		return ((int)(DataLng&) *(d.m_pVal));

	ASSERT(FALSE);
	
	return 0;
}

//-----------------------------------------------------------------------------
long Expression::CastLng(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
		return ((int) (DataInt&) *(d.m_pVal));
		
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
		return ((long) (DataLng&) *(d.m_pVal));
		
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataDate)))
		return ((DataDate&) *(d.m_pVal)).GiulianDate();

	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataEnum)))
		return ((DataEnum&) *(d.m_pVal)).GetValue();

	ASSERT(FALSE);
	
	return 0L;
}

//-----------------------------------------------------------------------------
double Expression::CastDbl(ExpItemVal& d)
{
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
		return ((int) (DataInt&) *(d.m_pVal));
		
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
		return ((long) (DataLng&) *(d.m_pVal));
		
	if (d.m_pVal && d.m_pVal->IsKindOf(RUNTIME_CLASS(DataDbl)))
		return ((double) (DataDbl&) *(d.m_pVal));

	ASSERT(FALSE);
	
	return 0.0;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::GiveMeResult(ExpItemOpe* pItemOpe, ExpItemVal* o1)
{
	DataObj* pData = NULL;
	ExpItemVal* pRetVal = NULL;

	BOOL bValid = o1->m_pVal->IsValid();

	if (
			pItemOpe->m_ResultType == DATA_VARIANT_TYPE &&
			o1->m_pVal->GetDataType() != DATA_VARIANT_TYPE
		)
	{
		pItemOpe->m_ResultType = GiveMeResultType(pItemOpe, o1, NULL, NULL);
	}

	switch (pItemOpe->m_nOpe)
	{
		case TT_UNMINUS	:
		{
			switch (pItemOpe->m_ResultType.m_wType)
			{
				case DATA_INT_TYPE:	 pData = new DataInt( -CastInt(*o1) );	break;
				case DATA_LNG_TYPE:	 pData = new DataLng( -CastLng(*o1) );	break;
				case DATA_DBL_TYPE:  pData = new DataDbl( -CastDbl(*o1) );	break;
				case DATA_MON_TYPE:  pData = new DataMon( -CastDbl(*o1) );	break;
				case DATA_QTA_TYPE:  pData = new DataQty( -CastDbl(*o1) );	break;
				case DATA_PERC_TYPE: pData = new DataPerc( -CastDbl(*o1) );	break;
			}
			break;
		}
		case T_NOT		:
		case T_OP_NOT	:
		{
			pData = new DataBool( !CastBool(*o1) );
			break;
		}
		case TT_IS_NULL	:
		{
			pData = new DataBool(!bValid);

			// lo stato bValid in questo caso e` stato usato per valorizzare
			// il ritorno della operazione
			bValid = TRUE;
			break;
		}
		case TT_IS_NOT_NULL	:
		{
			pData = new DataBool(bValid);

			// lo stato bValid in questo caso e` stato usato per valorizzare
			// il ritorno della operazione
			bValid = TRUE;
			break;
		}
		case T_AND		:
		case T_OP_AND	:
		case T_OR		:
		case T_OP_OR	:
		{
			BOOL bRes = CastBool(*o1);
			if	(
					bValid &&
					(
						(pItemOpe->m_nOpe == T_AND || pItemOpe->m_nOpe == T_OP_AND)
						? bRes
						: !bRes
					)
				)
			{
				pRetVal = EvalOK(pItemOpe->m_scndOpStack);
				if (pRetVal)
				{
					bValid = pRetVal->m_pVal->IsValid();
					bRes = CastBool(*pRetVal);

					if (pRetVal->m_bToBeDeleted) delete pRetVal;
					pRetVal = NULL;
				}
			}

			pData = new DataBool(bRes);
			break;
		}
		case T_BETWEEN	:
		{
			BOOL bRes = FALSE;
			if (bValid)
			{
				pRetVal = EvalOK(pItemOpe->m_frstOpStack);
				bValid = pRetVal->m_pVal->IsValid();
				if (bValid && o1->m_pVal->IsGreaterEqualThan(*(pRetVal->m_pVal)))
				{
					if (pRetVal->m_bToBeDeleted) delete pRetVal;

					pRetVal = EvalOK(pItemOpe->m_scndOpStack);
					bValid = pRetVal->m_pVal->IsValid();

					if (bValid)
						bRes = o1->m_pVal->IsLessEqualThan(*(pRetVal->m_pVal));

					if (pRetVal->m_bToBeDeleted) delete pRetVal;
					pRetVal = NULL;
				}
			}

			pData = new DataBool(bRes);
			break;
		}
		case T_QUESTION_MARK :
		{
			pRetVal = CastBool(*o1)
				? EvalOK(pItemOpe->m_frstOpStack)
				: EvalOK(pItemOpe->m_scndOpStack);

			if (pRetVal)
			{
				bValid = bValid && pRetVal->m_pVal && pRetVal->m_pVal->IsValid();

				pData = pRetVal->m_pVal ? pRetVal->m_pVal->DataObjClone() : NULL;

				if (pRetVal->m_bToBeDeleted) delete pRetVal;

				pRetVal = NULL;
			}

			break;
		}
	}

	if (pData)
	{
		pData->SetValid(bValid);
		pRetVal = new ExpItemVal(pData, 0, TRUE);
	}
	else
		ErrHandler(CHECKTYPE, o1);
		
	if (o1->m_bToBeDeleted) delete o1;

	return pRetVal;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::GiveMeResult(ExpItemOpe* pItemOpe, ExpItemVal* o1, ExpItemVal* o2)
{
	DataObj* pData = NULL;
	BOOL bValid = o1->m_pVal->IsValid() && o2->m_pVal->IsValid();

	if (
			pItemOpe->m_ResultType == DATA_VARIANT_TYPE &&
			o1->m_pVal->GetDataType() != DATA_VARIANT_TYPE &&
			o2->m_pVal->GetDataType() != DATA_VARIANT_TYPE
		)
	{
		pItemOpe->m_ResultType = GiveMeResultType(pItemOpe, o1, o2, NULL);
	}

	WORD wType = pItemOpe->m_ResultType.m_wType;
	switch (wType)
	{
		case DATA_NULL_TYPE:
			break;

		case DATA_INT_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataInt(CastInt(*o1) + CastInt(*o2));	break;
				case T_MINUS	: pData = new DataInt(CastInt(*o1) - CastInt(*o2));	break;
				case T_STAR		: pData = new DataInt(CastInt(*o1) * CastInt(*o2));	break;
				case T_SLASH	:
				case T_PERC		:
				{
					int nVal = CastInt(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataInt(0xFFFF);
						bValid = FALSE;
					}
					else
						if (pItemOpe->m_nOpe == T_SLASH)
							pData = new DataInt(CastInt(*o1) / nVal);
						else
							pData = new DataInt(CastInt(*o1) % nVal);

					break;
				}
				case T_BW_AND	: pData = new DataInt(CastInt(*o1) & CastInt(*o2));	break;
				case T_BW_OR	: pData = new DataInt(CastInt(*o1) | CastInt(*o2));	break;
				case T_BW_XOR	: pData = new DataInt(CastInt(*o1) ^ CastInt(*o2));	break;
			}
			break;
			
		case DATA_LNG_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataLng(CastLng(*o1) + CastLng(*o2));	break;
				case T_MINUS	: pData = new DataLng(CastLng(*o1) - CastLng(*o2));	break;
				case T_STAR		: pData = new DataLng(CastLng(*o1) * CastLng(*o2));	break;
				case T_SLASH	:
				case T_PERC		:
				{
					long nVal = CastLng(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataLng(0xFFFFFFFF);
						bValid = FALSE;
					}
					else
						if (pItemOpe->m_nOpe == T_SLASH)
							pData = new DataLng(CastLng(*o1) / nVal);
						else
							pData = new DataLng(CastLng(*o1) % nVal);

					break;
				}
				case T_BW_AND	: pData = new DataLng(CastLng(*o1) & CastLng(*o2));	break;
				case T_BW_OR	: pData = new DataLng(CastLng(*o1) | CastLng(*o2));	break;
				case T_BW_XOR	: pData = new DataLng(CastLng(*o1) ^ CastLng(*o2));	break;
			}
			break;
			
		case DATA_DBL_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataDbl(CastDbl(*o1) + CastDbl(*o2));	break;
				case T_MINUS	: pData = new DataDbl(CastDbl(*o1) - CastDbl(*o2));	break;
				case T_STAR		: pData = new DataDbl(CastDbl(*o1) * CastDbl(*o2));	break;
				case T_SLASH	:
				{
					double nVal = CastDbl(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataDbl(0xFFFFFFFF);
						bValid = FALSE;
					}
					else
						pData = new DataDbl(CastDbl(*o1) / nVal);
						
					break;
				}
			}
			break;
			
		case DATA_MON_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataMon(CastDbl(*o1) + CastDbl(*o2));	break;
				case T_MINUS	: pData = new DataMon(CastDbl(*o1) - CastDbl(*o2));	break;
				case T_STAR		: pData = new DataMon(CastDbl(*o1) * CastDbl(*o2));	break;
				case T_SLASH	:
				{
					double nVal = CastDbl(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataDbl(0xFFFFFFFF);
						bValid = FALSE;
					}
					else
						pData = new DataDbl(CastDbl(*o1) / nVal);
						
					break;
				}
			}
			break;
	
		case DATA_QTA_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataQty(CastDbl(*o1) + CastDbl(*o2));	break;
				case T_MINUS	: pData = new DataQty(CastDbl(*o1) - CastDbl(*o2));	break;
				case T_STAR		: pData = new DataQty(CastDbl(*o1) * CastDbl(*o2));	break;
				case T_SLASH	:
				{
					double nVal = CastDbl(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataDbl(0xFFFFFFFF);
						bValid = FALSE;
					}
					else
						pData = new DataDbl(CastDbl(*o1) / nVal);
						
					break;
				}
			}
			break;
	
		case DATA_PERC_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataPerc(CastDbl(*o1) + CastDbl(*o2));	break;
				case T_MINUS	: pData = new DataPerc(CastDbl(*o1) - CastDbl(*o2));	break;
				case T_STAR		: pData = new DataPerc(CastDbl(*o1) * CastDbl(*o2));	break;
				case T_SLASH	:
				{
					double nVal = CastDbl(*o2);
					if (nVal == 0)
					{
						ErrHandler(DIVISION_BY_ZERO, o1, o2);
						pData = new DataDbl(0xFFFFFFFF);
						bValid = FALSE;
					}
					else
						pData = new DataDbl(CastDbl(*o1) / nVal);
						
					break;
				}
			}
			break;
	
		case DATA_DATE_TYPE:
			switch (pItemOpe->m_nOpe)
			{
				case T_PLUS		: pData = new DataDate((long) (CastLng(*o1) + CastLng(*o2)));	break;
				case T_MINUS	: pData = new DataDate((long) (CastLng(*o1) - CastInt(*o2)));	break;
			}
			break;

		case DATA_STR_TYPE:
			if (pItemOpe->m_nOpe == T_PLUS)
				pData = new DataStr(o1->m_pVal->Str() + o2->m_pVal->Str());
		    break;

		case DATA_TXT_TYPE:
			if (pItemOpe->m_nOpe == T_PLUS)
				pData = new DataText(o1->m_pVal->Str() + o2->m_pVal->Str());
		    break;
			
		case DATA_BOOL_TYPE:
		{
			BOOL bBool;
			switch (pItemOpe->m_nOpe)
			{
				case T_GT		: pData = new DataBool(o1->m_pVal->IsGreaterThan		(*(o2->m_pVal)));	break;
				case T_GE		: pData = new DataBool(o1->m_pVal->IsGreaterEqualThan	(*(o2->m_pVal)));	break;
				case T_LT		: pData = new DataBool(o1->m_pVal->IsLessThan			(*(o2->m_pVal)));	break;
				case T_LE		: pData = new DataBool(o1->m_pVal->IsLessEqualThan		(*(o2->m_pVal)));	break;
				case T_EQ		:
				case T_ASSIGN	: pData = new DataBool(o1->m_pVal->IsEqual(*(o2->m_pVal)));	break;
				case T_NE		:
				case T_DIFF		: pData = new DataBool(!o1->m_pVal->IsEqual(*(o2->m_pVal)));	break;
				case T_LIKE		:
				{
					bBool = LikeFunction(CastStr(*o1), CastStr(*o2));
					pData = new DataBool(bBool);
					break;
				}
				case T_IN		:
				{
					if (o2->IsKindOf(RUNTIME_CLASS(ExpItemValFromVar)))
					{
						SymField* pField = CastFieldProvider(*o2);
						if (!pField)
							break;
						bBool = pField->GetProvider()->FindRecordIndex(pField->GetName(), o1->m_pVal) > -1;
						pData = new DataBool(bBool);
					}
					else if (o2->GetDataType() == DataType::Array)
					{
						DataArray* ar = CastArray(*o2);
						ASSERT_VALID(ar);
						if (ar)
						{
							bBool = ar->Find(o1->m_pVal) > -1;
							pData = new DataBool(bBool);
						}
					}
					else if (o2->GetDataType() == DataType::String)
					{
						CString s = CastStr(*o2);

						//bBool = s.Find(o1->m_pVal->Str());
						CString sItem = o1->m_pVal->Str(); 
						CStringArray ar;
						CStringArray_Split(ar, L",");
						bBool = CStringArray_Find(ar, sItem);

						pData = new DataBool(bBool);
					}
					break;
				}
				case T_CONTAINS		:
				{
					DataArray* ar = CastArray(*o1);
					if (ar)
					{
						bBool = ar->Find(o2->m_pVal) > -1;
						pData = new DataBool(bBool);
					}
					break;
				}
			}
			break;
		}
	}

	ExpItemVal* pRetVal = NULL;
	
	if (pData)
	{
		pData->SetValid(bValid);
		pRetVal = new ExpItemVal(pData, 0, TRUE);
	}
	else
		ErrHandler(CHECKTYPE, o1, o2);
		
	if (o1->m_bToBeDeleted) delete o1;
	if (o2->m_bToBeDeleted) delete o2;

	return pRetVal;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::GiveMeResult(ExpItemOpe* pItemOpe, ExpItemVal* o1, ExpItemVal* o2, ExpItemVal* o3)
{
	DataObj* pData = NULL;
	
	if (
			pItemOpe->m_ResultType == DATA_VARIANT_TYPE &&
			o1->m_pVal->GetDataType().m_wType != DATA_VARIANT_TYPE &&
			o2->m_pVal->GetDataType().m_wType != DATA_VARIANT_TYPE &&
			o3->m_pVal->GetDataType().m_wType != DATA_VARIANT_TYPE
		)
	{
		pItemOpe->m_ResultType = GiveMeResultType(pItemOpe, o1, o2, o3);
	}

	switch (pItemOpe->m_nOpe)
	{
		case TT_ESCAPED_LIKE :
			if (pItemOpe->m_ResultType == DATA_BOOL_TYPE)
				pData = new DataBool(LikeFunction(CastStr(*o1), CastStr(*o2), CastStr(*o3)));
			break;
	}

	ExpItemVal* pRetVal = NULL;
	
	if (pData)
	{
		pData->SetValid(o1->m_pVal->IsValid() && o2->m_pVal->IsValid() && o3->m_pVal->IsValid());
		pRetVal = new ExpItemVal(pData, 0, TRUE);
	}
	else
		ErrHandler(CHECKTYPE, o1, o2, o3);
		
	if (o1->m_bToBeDeleted) delete o1;
	if (o2->m_bToBeDeleted) delete o2;
	if (o3->m_bToBeDeleted) delete o3;

	return pRetVal;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::ApplyFunction(ExpItemFun* itemFun, Stack& paramStack)
{
	ExpItemVal*	p1 = NULL;
	ExpItemVal*	p2 = NULL;
	ExpItemVal*	p3 = NULL;
	ExpItemVal*	p4 = NULL;
	ExpItemVal*	p5 = NULL;
	ExpItemVal*	p6 = NULL;
	ExpItemVal*	p7 = NULL;
	ExpItemVal*	p8 = NULL;
	CString		strTmp;
	DataObj*	pData = NULL;
	Formatter* pFormatter =  NULL;

	switch (itemFun->m_nFun)
	{
		case T_FABS:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDbl(fabs(CastDbl(*p1)));
			break;

		case T_FASC:
			p1 = (ExpItemVal*) paramStack.Pop();
            strTmp = CastStr(*p1);
			pData = new DataInt((int) strTmp[0]);
			break;

		case T_FCDOW:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataStr(((DataDate*) p1->m_pVal)->WeekDayName());
			break;

		case T_FCEIL:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDbl(ceil(CastDbl(*p1)));
			break;

		case T_FCHR:
			{
			p1 = (ExpItemVal*) paramStack.Pop();
			TCHAR c = (TCHAR)CastInt(*p1);
             strTmp.Empty();
             strTmp = c;
			pData = new DataStr(strTmp);
			}
			break;

		case T_FCMONTH:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataStr(((DataDate*) p1->m_pVal)->MonthName());
			break;

		case T_FMONTH_NAME:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataStr(MonthName((WORD) CastInt(*p1)));
			break;

		case T_FCTOD:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDate(CastStr(*p1), TRUE);	// e` simmetrica alla DOTC
			break;

		case T_FLAST_MONTH_DAY:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataDate& aDate = *((DataDate*) p1->m_pVal);
			pData = new DataDate(::MonthDays(aDate.Month(), aDate.Year()), aDate.Month(), aDate.Year());
			break;
        }

		case T_FAPP_DATE:
			pData = new DataDate(AfxGetApplicationDate());
			break;

		case T_FAPP_YEAR:
			pData = new DataInt(AfxGetApplicationYear());
			break;

		case T_FDATE:
		{
			if (itemFun->m_nNumParam == 0)
			{
				pData = new DataDate();
				((DataDate*) pData)->SetTodayDate();
				break;
			}
			
			if (itemFun->m_nNumParam != 3)
				break;
	
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			//An. 19433
			if (!p1 || !p2 || !p3)
				break;
				
			WORD nDay	= (WORD)CastInt(*p1);
			WORD nMonth	= (WORD)CastInt(*p2);
			WORD nYear	= (WORD)CastInt(*p3);

			pData = new DataDate(nDay, nMonth, nYear);
			break;
        }

		case T_FDATETIME:
		{
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				p2 = (ExpItemVal*) paramStack.Pop();
				p3 = (ExpItemVal*) paramStack.Pop();
				p4 = (ExpItemVal*) paramStack.Pop();
				p5 = (ExpItemVal*) paramStack.Pop();
				p6 = (ExpItemVal*) paramStack.Pop();

				//An. 19433
				if (!p1 || !p2 || !p3 || !p4 || !p5 || !p6)
					break;

				WORD nDay		= (WORD)CastInt(*p1);
				WORD nMonth		= (WORD)CastInt(*p2);
				WORD nYear		= (WORD)CastInt(*p3);
				WORD nHour		= (WORD)CastInt(*p4);
				WORD nMinute	= (WORD)CastInt(*p5);
				WORD nSecond	= (WORD)CastInt(*p6);

				pData = new DataDate(nDay, nMonth, nYear, nHour, nMinute, nSecond);
				pData->SetFullDate();
				break;
			}
	
			pData = new DataDate();
			pData->SetFullDate();
			((DataDate*) pData)->SetTodayDateTime();
			break;
        }

		case T_FDATEADD:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			if (!p1 || !p1->m_pVal || !p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataDate)))
				break;

			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			p4 = (ExpItemVal*) paramStack.Pop();
				
			if (!p2 || !p3 || !p4)
				break;

			int nDay	= CastInt(*p2);
			int nMonth = CastInt(*p3);
			int nYear = CastInt(*p4);
			int nHour = 0;
			int nMinute = 0;
			int nSecond = 0;

			int nOptionalParam = itemFun->m_nNumParam - parameters_of(itemFun->m_nFun);
			if (nOptionalParam > 0)
			{
				p5 = (ExpItemVal*) paramStack.Pop();
				nHour	= CastInt(*p5);

				if (nOptionalParam > 1)
				{
					p6 = (ExpItemVal*) paramStack.Pop();
					nMinute	= CastInt(*p6);

					if (nOptionalParam > 2)
					{
						p7 = (ExpItemVal*) paramStack.Pop();
						nSecond	= CastInt(*p7);
					}
				}
			}
	
			DataDate* pSrcDate = dynamic_cast<DataDate*>(p1->m_pVal);
			if (pSrcDate)
			{
				pData = new DataDate(pSrcDate->AddTime(nDay, nMonth, nYear, nHour, nMinute, nSecond));
				if (pSrcDate->IsFullDate() || nOptionalParam > 0)
					pData->SetFullDate();
			}
			break;
        }

		case T_FWEEKSTARTDATE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			if (!p1 || !p2)
				break;
				
			WORD nYear	= (WORD)CastInt(*p1);
			WORD nWeek	= (WORD)CastInt(*p2);

			DataDate* pDataDate = (DataDate*)(pData = new DataDate(1, 1, nYear));
			pDataDate->SetWeekStartDate(nYear, nWeek);
			break;
        }

		case T_FTIME:
		{
			int	nHour;
			int	nMinute;
			int	nSecond;

			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				p2 = (ExpItemVal*) paramStack.Pop();
				p3 = (ExpItemVal*) paramStack.Pop();
				//An. 19433
				if (!p1 || !p2 || !p3)
					break;
				nHour	= CastInt(*p1);
				nMinute	= CastInt(*p2);
				nSecond	= CastInt(*p3);
				pData = new DataDate();
				pData->SetAsTime();
				((DataDate*)pData)->SetTime(nHour, nMinute, nSecond);
				break;
			}

			pData = new DataDate();
			pData->SetAsTime();
			((DataDate*) pData)->SetTodayTime();
			break;
		}

		case T_FELAPSED_TIME:	//@@ElapsedTime
		{
			pData = new DataLng();
			pData->SetAsTime();

			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				p2 = (ExpItemVal*) paramStack.Pop();
				((DataLng*) pData)->SetElapsedTime(*((DataDate*) p1->m_pVal), *((DataDate*) p2->m_pVal));
			}
	
			break;
        }

		case T_FISLEAPYEAR:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataBool(::Intercalary(CastInt(*p1)) == 1);
			break;

		case T_FEASTERSUNDAY:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDate(DataDate::EasterSunday(CastInt(*p1)));
			break;

		case T_FDAY:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt(((DataDate*) p1->m_pVal)->Day());
			break;

		case T_FDTOC:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataStr(((DataDate*) p1->m_pVal)->Str());
			break;

		case T_FFLOOR:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDbl(floor(CastDbl(*p1)));
			break;

		case T_FLEFT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
			int i = CastInt(*p2);
			if (i < 0) i = 0;
            strTmp = strTmp.Left(i);
			pData = new DataStr(strTmp);
			break;
		}
		case T_FFIND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();

			CString sSub = CastStr(*p1);
			CString sString = CastStr(*p2);

			int nStartIndex = 0;
			int nOccurence = 1;
			if (itemFun->m_nNumParam > 2)
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				nStartIndex = CastInt(*p3);

				if (itemFun->m_nNumParam > 3)
				{
					p4 = (ExpItemVal*) paramStack.Pop();
					nOccurence = CastInt(*p4);
					if (nOccurence < 1)
					{
						ASSERT(FALSE);
						pData = new DataInt(-1);
						break;
					}
				}
			}

			int idx = (nOccurence == 1 ? sString.Find(sSub, nStartIndex) : ::FindOccurence(sString, sSub, nOccurence, nStartIndex));

			pData = new DataInt(idx);
			break;
		}
		case T_FREVERSEFIND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();

			CString sSub = CastStr(*p1);
			CString sString = CastStr(*p2);

			int nStartIndex = sString.GetLength() - 1;
			int nOccurence = 1;

			if (itemFun->m_nNumParam > 2)
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				nStartIndex = CastInt(*p3);

				if (itemFun->m_nNumParam > 3)
				{
					p4 = (ExpItemVal*) paramStack.Pop();
					nOccurence = CastInt(*p4);
					if (nOccurence < 1)
					{
						ASSERT(FALSE);
						pData = new DataInt(-1);
						break;
					}
				}
			}

			int idx = ::ReverseFind(sString, sSub, nStartIndex, nOccurence);

			pData = new DataInt(idx);
			break;
		}
		case T_FWILDCARD_MATCH:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();

			bool b = ::WildcardMatch(CastStr(*p1), CastStr(*p2));
			pData = new DataBool(b);
			break;
		}

		case T_FREPLACE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
			int occurrence = strTmp.Replace(CastStr(*p2), CastStr(*p3));
			pData = new DataStr(strTmp);
			break;
		}
		case T_FREMOVENEWLINE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			strTmp = CastStr(*p1);
			strTmp.Replace(_T("\r\n"), _T(" "));
			strTmp.Replace(_T("\r"), _T(" "));
			strTmp.Replace(_T("\n"), _T(" "));
			pData = new DataStr(strTmp);
			break;
		}

		case T_FLEN:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt(_tcslen(CastStr(*p1)));
			break;

		case T_FMAX:
		case T_FMIN:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			
			DataType dt = p1->m_pVal->GetDataType();
			
			if (itemFun->m_nNumParam == 1)	//casi particolari
			{
				DataObj* pM = NULL;
				if (dt == DataType::Array)
				{
					DataArray* ar = CastArray(*p1);

					pM = itemFun->m_nFun == T_FMAX 
						?
						ar->GetMaxElem() 
						:
						ar->GetMinElem();
				}
				else if (p1->IsKindOf(RUNTIME_CLASS(ExpItemValFromVar)))
				{
					SymField* pField = CastFieldProvider(*p1);

					pM = itemFun->m_nFun == T_FMAX 
							?
							pField->GetProvider()->GetMaxElem(pField->GetName()) 
							:
							pField->GetProvider()->GetMinElem(pField->GetName());
				}
				if (pM)
					pData = pM->Clone();
			}
			else
			{
				pData = p1->m_pVal->Clone();

				for (int np = 1; np < itemFun->m_nNumParam; np++)
				{
					p2 = (ExpItemVal*) paramStack.Pop();

					if (!DataType::IsCompatible(p2->m_pVal->GetDataType(), dt))
					{
						//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
						break;
					}

					if (
						itemFun->m_nFun == T_FMAX 
							?
							pData->IsLessThan(*p2->m_pVal)
							:
							pData->IsGreaterThan(*(p2->m_pVal))
					)
					{
						pData->Assign(*(p2->m_pVal));
					}
				}
			}
			break;
		}

		case T_FMOD:
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			pData = new DataDbl( fmod(CastDbl(*p1), CastDbl(*p2)) );
			break;

		case T_FMONTH:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt(((DataDate*) p1->m_pVal)->Month());
			break;

		case T_FMONTH_DAYS:
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt((int)::MonthDays ((WORD) CastInt(*p1),(WORD) CastInt(*p2)));
			break;

		case T_FRIGHT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
			int i = CastInt(*p2);
			if (i < 0) i = 0;
			strTmp = strTmp.Right(i);
			pData = new DataStr(strTmp);
			break;
		}
		case T_FROUND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			int i = 0;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				i = CastInt(*p2);
			}
			pData = new DataDbl(Round(CastDbl(*p1), i));
			break;
		}

		case T_FSIGN:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt((CastDbl(*p1) >= 0.0 ? +1 : -1));
			break;

		case T_FTRIM:
			p1 = (ExpItemVal*)paramStack.Pop();
			pData = new DataStr();

			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p2 = (ExpItemVal*)paramStack.Pop();
				((DataStr*)pData)->Assign(CastStr(*p1).Trim(CastStr(*p2)));
			}
			else
				((DataStr*)pData)->Assign(CastStr(*p1).Trim());
			break;

		case T_FLTRIM:
			p1 = (ExpItemVal*)paramStack.Pop();
			pData = new DataStr();

			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p2 = (ExpItemVal*)paramStack.Pop();
				((DataStr*)pData)->Assign(CastStr(*p1).TrimLeft(CastStr(*p2)));
			}
			else
				((DataStr*)pData)->Assign(CastStr(*p1).TrimLeft());
			break;

		case T_FRTRIM:
			p1 = (ExpItemVal*)paramStack.Pop();
			pData = new DataStr();

			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p2 = (ExpItemVal*)paramStack.Pop();
				((DataStr*)pData)->Assign(CastStr(*p1).TrimRight(CastStr(*p2)));
			}
			else
				((DataStr*)pData)->Assign(CastStr(*p1).TrimRight());
			break;

		case T_FSPACE:
			p1 = (ExpItemVal*) paramStack.Pop();

			Space(strTmp, CastInt(*p1));

			pData = new DataStr(strTmp);
			break;

		case T_FREPLICATE:
			p1 = (ExpItemVal*)paramStack.Pop();
			p2 = (ExpItemVal*)paramStack.Pop();

			Replicate(strTmp, CastInt(*p2), CastStr(*p1));

			pData = new DataStr(strTmp);
			break;

		case T_FPADLEFT:
			{
			p1 = (ExpItemVal*)paramStack.Pop();
			p2 = (ExpItemVal*)paramStack.Pop();
			p3 = (ExpItemVal*)paramStack.Pop();

			strTmp = CastStr(*p1);
			int oldLen = strTmp.GetLength();

			int newLen = CastInt(*p2);
			CString strPad = CastStr(*p3);

			if (newLen > oldLen && !strPad.IsEmpty())
			{
				CString pad; Replicate(pad, newLen - oldLen, strPad.Left(1));
				strTmp = pad + strTmp;
			}
			pData = new DataStr(strTmp);
			break;
			}
		case T_FPADRIGHT:
		{
			p1 = (ExpItemVal*)paramStack.Pop();
			p2 = (ExpItemVal*)paramStack.Pop();
			p3 = (ExpItemVal*)paramStack.Pop();

			strTmp = CastStr(*p1);
			int oldLen = strTmp.GetLength();

			int newLen = CastInt(*p2);
			CString strPad = CastStr(*p3);

			if (newLen > oldLen && !strPad.IsEmpty())
			{
				CString pad; Replicate(pad, newLen - oldLen, strPad.Left(1));
				strTmp = strTmp + pad;
			}
			pData = new DataStr(strTmp);
			break;
		}

		case T_FCOMPARE_NO_CASE:
		{
			p1 = (ExpItemVal*)paramStack.Pop();
			p2 = (ExpItemVal*)paramStack.Pop();

			strTmp = CastStr(*p1);
			CString str2 = CastStr(*p2);

			pData = new DataInt(strTmp.CompareNoCase(str2));
			break;
		}

		case T_FSTR:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
				
			int lenStr = CastInt(*p2);
	
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				pData = new DataStr(p1->m_pVal->Str(lenStr, CastInt(*p3)));
			}
			else
				pData = new DataStr(p1->m_pVal->Str(lenStr));

			break;
		}

		case T_FFORMAT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataType dataType = p1->m_pVal->GetDataType();
			if (dataType == DataType::Array)
			{
				dataType = ((DataArray*)p1->m_pVal)->GetBaseDataType ();
			}
			pFormatter = NULL;
				
			if (itemFun->m_nNumParam == parameters_of(itemFun->m_nFun))
			{
				if (m_pSymTable && m_pSymTable->GetDocument())
					pFormatter = m_pSymTable->GetDocument()->GetFormatter(dataType);
				if (!pFormatter)
					pFormatter = AfxGetStandardFormatStyleTable()->GetFormatter(dataType, NULL);
			}
			else
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				CString sFormatterName = CastStr(*p2);

				if (sFormatterName.CompareNoCase(_T("xml")) == 0)
				{
					pData = new DataStr(p1->m_pVal->FormatDataForXML());
					break;
				}
				else if (sFormatterName.CompareNoCase(_T("fax")) == 0 && p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
				{
					pData = new DataStr( AfxGetIMailConnector()->FormatFaxAddress(*(DataStr*)p1->m_pVal) );
					break;
				}

				if (m_pSymTable && m_pSymTable->GetDocument())
					pFormatter = m_pSymTable->GetDocument()->GetFormatter(sFormatterName);
				if (!pFormatter)
					pFormatter = AfxGetFormatStyleTable()->GetFormatter(sFormatterName, NULL);

				if (pFormatter && !DataType::IsCompatible(dataType, pFormatter->GetDataType()))
				{
					ErrHandler(FORMAT_TYPEERR, itemFun);
					break;
				}

				if (!pFormatter && sFormatterName.Find(L'%') >= 0)
				{
					if (p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataInt)))
					{
						int n = CastInt(*p1);

						CString s; s.Format(sFormatterName, n);

						pData = new DataStr(s);
						break;
					}
					else if (p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataLng)))
					{
						int n = CastLng(*p1);

						CString s; s.Format(sFormatterName, n);

						pData = new DataStr(s);
						break;
					}
					else if (p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataDbl)))
					{
						double n = CastDbl(*p1);

						CString s; s.Format(sFormatterName, n);

						pData = new DataStr(s);
						break;
					}
					else if (p1->m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
					{
						CString sp = CastStr(*p1);

						CString s; s.Format(sFormatterName, sp);

						pData = new DataStr(s);
						break;
					}
				}
			}
	
			if (pFormatter == NULL)
			{
				ErrHandler(FORMAT_UNKNOWN, itemFun);
				break;
			}

			//TODO occorre convertire il dataobj
			//DataObj* pV = dataType::Convert(p1->m_pVal, pFormatter->GetDataType());
			//if (!pV)
			//{
			//	ErrHandler(FORMAT_TYPEERR, itemFun);
			//	break;
			//}
			//pFormatter->FormatDataObj(*pV, strTmp);
			//delete pV;

			pFormatter->FormatDataObj(*(p1->m_pVal), strTmp);
			pData = new DataStr(strTmp);

			break;
		}

		case T_FLOADTEXT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString strFineName (CastStr(*p1));
			if (CTBNamespace(strFineName).IsValid())
			{
				CTBNamespace aFileNs; 
				aFileNs.SetNamespace(strFineName);
				strFineName = AfxGetPathFinder()->GetFileNameFromNamespace(aFileNs, AfxGetLoginInfos()->m_strUserName);
			}
			CString sText;
			::LoadLineTextFile (strFineName, sText);
			pData = new DataStr(sText);
			break;
		}
		case T_FSAVETEXT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString strFineName (CastStr(*p1));
			if (CTBNamespace(strFineName).IsValid())
			{
				CTBNamespace aFileNs; 
				aFileNs.SetNamespace(strFineName);
				strFineName = AfxGetPathFinder()->GetFileNameFromNamespace(aFileNs, AfxGetLoginInfos()->m_strUserName);
			}
			
			p2 = (ExpItemVal*) paramStack.Pop();
			CString sText (CastStr(*p2));

			int nfmt = 0;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				nfmt = CastInt(*p3);
				if (nfmt < 0 || nfmt > 3) 
				{	
					nfmt = 0;
					//TODO segnalare errore
					ErrHandler(PARAM, itemFun);
					break;
				}
			}
			
			if (!::ExistPath(strFineName))	//TODO
			{
				IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
				CString sPath = ::GetPath(strFineName);
				if (pFileSystemManager)
					pFileSystemManager->CreateFolder(sPath, TRUE);
				else
					::CreateDirectory(sPath);
			}

			CFileException ef;
			CLineFile oFile;
			BOOL bOk = oFile.Open
				(
					strFineName, 
					CFile::modeCreate | CFile::modeWrite | CFile::shareDenyRead | CFile::typeText,
					&ef,
					(CLineFile::FileFormat)nfmt
				);
			if (!bOk)
			{
				pData = new DataBool(FALSE);
				break;
			}

			oFile.WriteString (sText);

			pData = new DataBool(TRUE);
			break;
		}

		case T_FSUBSTR:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
			int i = CastInt(*p2) - 1;
			if (i < 0) 
			{
				//ErrHandler(PARAM, itemFun);
				//break;
				i = 0;
			}

			int j = 1;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				j = CastInt(*p3);
			}
			if (j <= 0 || i >= strTmp.GetLength()) 
			{
				//ErrHandler(PARAM, itemFun);
				pData = new DataStr();
				break;
			}
			strTmp = strTmp.Mid(i, j);
			pData = new DataStr(strTmp);
			break;
		}

		case T_FSUBSTRWW:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
			if (!strTmp.IsEmpty())
			{
				int nLen = strTmp.GetLength();
				int nStart = CastInt(*p2) - 1;
				int nMaxChars = CastInt(*p3);	// massimo numero di caratteri da ritornare
				int nChars = nMaxChars;
				if (nStart < 0) nStart = 0;
				if	(
						nStart < nLen &&
						(nChars <= 0 || (nStart + nChars) > nLen)
					)
					nChars = nLen - nStart;

				if (nChars <= 0 || nStart >= nLen)
					strTmp.Empty();
				else
				{
					if (nChars != nLen - nStart)
					{

						while
							(
								nChars > 0 &&
								!_istspace(strTmp[nStart + nChars])
							)
							nChars--; 

							if (nChars == 0)
								nChars = nMaxChars;
					}
	
					// non vengono strippati i blank ne` a destra ne` a sinistra
					// della stringa ritornata
					strTmp = strTmp.Mid(nStart, nChars);
				}
			}

			pData = new DataStr(strTmp);
			break;
		}


		case T_FLOWER:
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				DataStr* pVal = (DataStr*) p1->m_pVal;
				
				bool bLocalizedCompare = true;
				if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
				{
					p2 = (ExpItemVal*) paramStack.Pop();
					bLocalizedCompare = !CastBool(*p2);
				}
				
				pData = pVal->DataObjClone();
				pData->SetCollateCultureSensitive(bLocalizedCompare);
				((DataStr*)pData)->MakeLower();

				break;
			}
		case T_FUPPER:
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				DataStr* pVal = (DataStr*) p1->m_pVal;
				
				bool bLocalizedCompare = true;
				if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
				{
					p2 = (ExpItemVal*) paramStack.Pop();
					bLocalizedCompare = !CastBool(*p2);
				}
				pData = pVal->DataObjClone();
				pData->SetCollateCultureSensitive(bLocalizedCompare);
				((DataStr*)pData)->MakeUpper();
				break;
			}

		case T_FVAL:
			p1 = (ExpItemVal*) paramStack.Pop();
			strTmp = CastStr(*p1);
//			if (!IsFloatNumber(strTmp))
//			{
//				ErrHandler(IDS_SYNTAX_ERROR, itemFun);
//				break;
//			}
			// default formatter
			if (m_pSymTable && m_pSymTable->GetDocument())
				pFormatter = m_pSymTable->GetDocument()->GetFormatter(_T("Double"));
			
			if (!pFormatter)
				pFormatter = AfxGetFormatStyleTable()->GetFormatter(_T("Double"), NULL);

			if (pFormatter)
			{
				CDblFormatter* pDblFormatter = (CDblFormatter*) pFormatter;
				// decimal separator could be different, but _tstof 
				// recognize only . as decimal separator
				int nPos = strTmp.Find(pDblFormatter->GetDecSeparator());
				if (nPos > 0)
					strTmp.Replace(pDblFormatter->GetDecSeparator(), _T("."));
			}
			pFormatter = NULL;
			pData = new DataDbl(_tstof(strTmp));
			break;

		case T_FYEAR:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt(((DataDate*) p1->m_pVal)->Year());
			break;

		case T_FINT:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataInt( (int) CastDbl(*p1) );
			break;
			
		case T_FLONG:
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataLng( (long) CastDbl(*p1) );
			break;

		case T_FTYPED_BARCODE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();

			//valori di default
			int nChkSum = 0;
			CString sHR;
			CString sVersion;
			int nNarrowBar = -1;
			int nBarHeight = -1;
			int nErrCorrLevel = -2;

			if (itemFun->m_nNumParam > 2)
			{
				 p3 = (ExpItemVal*) paramStack.Pop();
				 nChkSum = CastLng(*p3);

				if (itemFun->m_nNumParam > 3)
				{
					 p4 = (ExpItemVal*) paramStack.Pop();
					 sHR = CastStr(*p4);

					 if (itemFun->m_nNumParam > 4)
					 {
						 p5 = (ExpItemVal*)paramStack.Pop();
						 nNarrowBar = CastLng(*p5);
						 if (itemFun->m_nNumParam > 5)
						 {
							 p6 = (ExpItemVal*)paramStack.Pop();
							 nBarHeight = CastLng(*p6);

							 if (itemFun->m_nNumParam > 6)
							 {
								 p7 = (ExpItemVal*)paramStack.Pop();
								 sVersion = CastStr(*p7);

								 if (itemFun->m_nNumParam > 7)
								 {
									 p8 = (ExpItemVal*)paramStack.Pop();
									 nErrCorrLevel = CastLng(*p8);
								 }
							 }
						 }
					 }
				}
			}
			pData = new DataStr(CBarCodeTypes::TypedBarCode(CastStr(*p1), CastInt(*p2), nChkSum, sHR, nNarrowBar, nBarHeight, sVersion, nErrCorrLevel));
			break;
		}
		case T_FGETBARCODE_ID:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			if (itemFun->m_nNumParam == 2)
			{
				//Enum 86 ASSERT_TRACE(CastLng(*p1) == 0, _T("\nFunction GetBarCodeID called with wrong first parameter\n"));
				p2 = (ExpItemVal*) paramStack.Pop();
				pData = new DataInt(CBarCodeTypes::BarCodeType(CastStr(*p2)));
			}
			else
			{
				DWORD dw = CastLng(*p1);
				pData = new DataInt(CBarCodeTypes::BarCodeType(dw));
			}
			break;
		}
		case T_FDAYOFWEEK:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataDate data (*(DataDate*) p1->m_pVal);

			pData = new DataInt( data.DayOfWeek() + 1 );
			break;
		}
		case T_FWEEKOFMONTH:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataDate data (*(DataDate*) p1->m_pVal);

			if (itemFun->m_nNumParam == 2)
			{
				 p2 = (ExpItemVal*) paramStack.Pop();
				 DataInt alg (*(DataInt*) p2->m_pVal); 
				 if (alg == 1)
				 {
					 pData = new DataInt( data.WeekOfMonth(alg) );
					 break;
				 }
			}
			pData = new DataInt( data.WeekOfMonth() );
			break;
		}
		case T_FWEEKOFYEAR:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
 
			DataDate data (*(DataDate*) p1->m_pVal);

			pData = new DataInt( data.WeekOfYear() );
			break;
		}
		case T_FGIULIANDATE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataDate data (*(DataDate*) p1->m_pVal);

			pData = new DataLng( data.GiulianDate() );
			break;
		}
		case T_FDAYOFYEAR:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
 
			DataDate data (*(DataDate*) p1->m_pVal);

			pData = new DataInt( data.DayOfYear() );
			break;
		}
		case T_FLOCALIZE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			strTmp = AfxLoadReportString (CastStr(*p1), m_pSymTable->GetDocument());
			pData = new DataStr(strTmp);
			break;
		}
		case T_FRGB:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			pData = new DataLng(RGB(CastInt(*p1), CastInt(*p2), CastInt(*p3)));
			break;
		}

		case T_FISACTIVATED:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			pData = new DataBool(AfxIsActivated (CastStr(*p1), CastStr(*p2)));
			break;
		}
		case T_FISADMIN:
		{
			pData = new DataBool(AfxGetLoginInfos()->m_bAdmin);
			break;
		}

		case T_FISREMOTEINTERFACE:
		{
			pData = new DataBool(AfxIsRemoteInterface());
			break;
		}
		case T_FISRUNNINGFROMEXTERNALCONTROLLER:
		{
			pData = new DataBool(m_pSymTable->GetDocument() ? m_pSymTable->GetDocument()->IsRunningFromExternalController () : FALSE);
			break;
		}
		case T_FISWEB:
		{
			pData = new DataBool(); //it returns FALSE, Easylook returns TRUE
			break;
		}

		case T_FISEMPTY: 
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			ASSERT_VALID(p1->m_pVal);

			pData = new DataBool(p1->m_pVal == NULL || p1->m_pVal->IsEmpty()); 
			break;
		}
		case T_FISNULL: 
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			ASSERT_VALID(p1);
			//ASSERT_VALID(p1->m_pVal);
			p2 = (ExpItemVal*) paramStack.Pop();
			ASSERT_VALID(p2);
			ASSERT_VALID(p2->m_pVal);

			if (p1->m_pVal == NULL || !p1->m_pVal->IsValid())
			{
				if (!p1->m_pVal->IsValid())
					p1->m_pVal->SetValid();

				pData = p2->m_pVal->Clone(); 
			}
			else
				pData = p1->m_pVal->Clone(); 

			ASSERT(pData->IsValid());
			break;
		}

		case T_FGETAPPTITLE_FROM_NS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			CTBNamespace ns(CTBNamespace::DOCUMENT,CastStr(*p1));
			AddOnApplication* pApp = AfxGetAddOnApp(ns.GetApplicationName());
			pData = new DataStr(pApp ? pApp->GetTitle() : _T(""));
			break;
		}
		case T_FGETMODTITLE_FROM_NS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			CTBNamespace ns(CTBNamespace::DOCUMENT, CastStr(*p1));
			AddOnModule* pMod = AfxGetAddOnModule(ns);
			pData = new DataStr(pMod ? pMod->GetModuleTitle() : _T(""));
			break;
		}
		case T_FGETDOCTITLE_FROM_NS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			CTBNamespace ns(CTBNamespace::DOCUMENT, CastStr(*p1) );
			AddOnModule* pMod = AfxGetAddOnModule(ns);
			if (pMod == NULL)
				pData = new DataStr(_T(""));
			else
			{
				const CDocumentDescription* pDoc = AfxGetDocumentDescription(ns);
				pData = new DataStr(pDoc ? pDoc->GetTitle() : _T(""));
			}
			break;
		}
		case T_FGETPATH_FROM_NS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString s = CastStr(*p1);

			CTBNamespace ns (s);
			//if (!ns.IsValid())
			//{
			//	pData = new DataStr();
			//	break;
			//}

			DataStr sType(_T("auto"));
			if (itemFun->m_nNumParam > 1)
			{
				 p2 = (ExpItemVal*) paramStack.Pop();
				 if (p2->GetDataType() == DataType::String)
				 {
					sType.Assign(CastStr(*p2));
				 }
			}

			CString sPath;

			if (sType == _T("auto"))
			{
				sPath = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
			}
			else if (sType == _T("user"))
			{
				if (ns.GetType() == CTBNamespace::REPORT)
					sPath = AfxGetPathFinder()->GetModuleReportPath (ns, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
				else
					sPath = AfxGetPathFinder()->GetModuleFilesPath (ns, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
			}
			else if (sType == _T("company"))
			{
				if (ns.GetType() == CTBNamespace::REPORT)
					sPath = AfxGetPathFinder()->GetModuleReportPath (ns, CPathFinder::ALL_USERS);
				else
					sPath = AfxGetPathFinder()->GetModuleFilesPath (ns, CPathFinder::ALL_USERS);
			}
			else if (sType == _T("standard"))
			{
				if (ns.GetType() == CTBNamespace::REPORT)
					sPath = AfxGetPathFinder()->GetModuleReportPath (ns, CPathFinder::STANDARD);
				else
					sPath = AfxGetPathFinder()->GetModuleFilesPath (ns, CPathFinder::STANDARD);
			}

			pData = new DataStr(sPath);
			break;
		}
		case T_FGETNS_FROM_PATH:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString s = CastStr(*p1);

			CTBNamespace ns (AfxGetPathFinder()->GetNamespaceFromPath(s));
			pData = new DataStr(ns.IsValid() ? ns.ToString() : _T(""));
			break;
		}

		case T_FGETDATABASETYPE:
		{
			pData = new DataStr(AfxGetLoginInfos()->m_strDatabaseType);
			break;
		}
		case T_FISDATABASEUNICODE:
		{
			pData = new DataBool(AfxGetLoginInfos()->m_bUseUnicode);
			break;
		}
		case T_FGETEDITION:
		{
			pData = new DataStr(AfxGetLoginManager()->GetEdition());
			break;
		}
		case T_FGETPRODUCTLANGUAGE:
		{
			pData = new DataStr(AfxGetLoginManager()->GetProductLanguage());
			break;
		}
		case T_FGETCOMPUTERNAME:							 
		{
			BOOL bStrip = TRUE;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p1 = (ExpItemVal*) paramStack.Pop();
				 if (p1->GetDataType() == DataType::Bool)
				 {
					 bStrip = CastBool(*p1);
				 }
			}
			pData = new DataStr(::GetComputerName(bStrip));
			break;
		}
		case T_FGETLOGINNAME:							 
		{
			pData = new DataStr(AfxGetThreadContext()->GetSessionLoginName());
			break;
		}
		case T_FSETCULTURE:							 
		{
			pData = new DataStr(AfxGetThreadContext()->GetUICulture());

			p1 = (ExpItemVal*) paramStack.Pop();
			if (p1->GetDataType() == DataType::String)
			{
				if (m_pSymTable && m_pSymTable->GetDocument())
					m_pSymTable->GetDocument()->SetUICulture(CastStr(*p1));
				else
					AfxGetThreadContext()->SetUICulture(CastStr(*p1));
			}
			break;
		}
		case T_FGETCULTURE:							 
		{
			pData = new DataStr();
			DataStr sType(_T("user"));
			DataBool bUI(TRUE);

			if (itemFun->m_nNumParam > 0)
			{
				 p1 = (ExpItemVal*) paramStack.Pop();
				 if (p1->GetDataType() == DataType::String)
				 {
					sType.Assign(CastStr(*p1));
				 }
			}
			else
			{
				if (m_pSymTable && m_pSymTable->GetDocument())
				{
					pData->Assign(m_pSymTable->GetDocument()->GetUICulture());
					break;
				}
			}

			if (itemFun->m_nNumParam == 2)
			{
				 p2 = (ExpItemVal*) paramStack.Pop();
				 if (p2->GetDataType() == DataType::Bool)
				 {
					bUI = CastBool(*p2);
				 }
			}
			if (bUI)
			{
				if (sType == _T("company"))
				{
					pData->Assign(AfxGetLoginInfos()->m_strCompanyLanguage);
				}
				else if (sType == _T("server"))
				{
					pData->Assign(AfxGetCommonClientObjects()->GetServerConnectionInfo ()->m_sPreferredLanguage);
				}
				else if (sType == _T("user") || sType == _T("login"))
				{
					pData->Assign(AfxGetLoginInfos()->m_strPreferredLanguage);
				}
			}
			else
			{
				if (sType == _T("company"))
				{
					pData->Assign(AfxGetLoginInfos()->m_strCompanyApplicationLanguage);
				}
				else if (sType == _T("server"))
				{
					pData->Assign(AfxGetCommonClientObjects()->GetServerConnectionInfo ()->m_sApplicationLanguage);
				}
				else if (sType == _T("user") || sType == _T("login"))
				{
					pData->Assign(AfxGetLoginInfos()->m_strApplicationLanguage);
				}
			}
			break;
		}
		case T_FGETUSERDESCRIPTION:							 
		{   //TODO manca gestione descrizione utente sessione diversa
			pData = new DataStr();
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				 p1 = (ExpItemVal*) paramStack.Pop();
				 if (p1->GetDataType() == DataType::String)
				 {
					pData->Assign(AfxGetLoginManager()->GetUserDescriptionByName(CastStr(*p1)));
				 }
				 else
				 {
					DataLng id = CastLng(*p1);
					pData->Assign(AfxGetLoginManager()->GetUserDescriptionById(id));
				 }
			}
			else
				pData->Assign(AfxGetLoginInfos()->m_strUserDescription);
			break;
		}

		case T_FSEND_BALLOON:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString sBody(CastStr(*p1));

			p2 = (ExpItemVal*) paramStack.Pop();
			CStringArray arRecipients;
			CStringArray_Split(arRecipients, CastStr(*p2));

			p3 = (ExpItemVal*) paramStack.Pop();
			CLoginManagerInterface::BalloonMessageSensation bse(CLoginManagerInterface::bs_Information);
			int bs(CastLng(*p3));
			if (bs >= 0 && bs <= CLoginManagerInterface::bs_Help)
				bse = (CLoginManagerInterface::BalloonMessageSensation) bs;

			p4 = (ExpItemVal*) paramStack.Pop();
			DataDate* dtExpire = dynamic_cast<DataDate*>(p4->m_pVal);

			p5 = (ExpItemVal*) paramStack.Pop();
			BOOL bHistoricize(CastBool(*p5));

			p6 = (ExpItemVal*) paramStack.Pop();
			BOOL bImmediate(CastBool(*p6));

			p7 = (ExpItemVal*) paramStack.Pop();
			int closingTimer(CastLng(*p7));
			
			AfxGetLoginManager()->AdvancedSendBalloon
			(
				sBody,	
				*dtExpire,
				CLoginManagerInterface::bt_Advrtsm,//se  si imposta NONE il messaggio non viene mostrato!
				arRecipients,
				bse,
				bHistoricize,
				bImmediate,
				closingTimer
			);
			if (bImmediate)
				::PostMessage(AfxGetMenuWindowHandle(), UM_IMMEDIATE_BALLOON, NULL, NULL); //CUtility::ShowImmediateBalloon(); non compila
			
			pData = new DataBool(TRUE);
			break;
		}

		case T_FGETWINDOWUSER:							 
		{
			pData = new DataStr(::GetUserName());
			break;
		}
		case T_FGETINSTALLATIONNAME:							 
		{
			pData = new DataStr(AfxGetPathFinder()->GetInstallationName());
			break;
		}
		case T_FGETINSTALLATIONPATH:
		{
			pData = new DataStr(AfxGetPathFinder()->GetInstallationPath());
			break;
		}
		case T_FGETINSTALLATIONVERSION:
		{
			pData = new DataStr(AfxGetLoginManager()->GetInstallationVersion());
			break;
		}
		case T_FGETCOMPANYNAME:							 
		{
			pData = new DataStr(AfxGetLoginInfos()->m_strCompanyName);
			break;
		}
		case T_FGETNEWGUID:							 
		{
			DataGuid dg;
			dg.AssignNewGuid();
			pData = new DataStr(dg.Str(FALSE));
			break;
		}
		case T_FMAKELOWERLIMIT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString s = CastStr(*p1);
			s.Trim();
			pData = new DataStr(s.IsEmpty() ? CParsedCtrl::Strings::FIRST() : s);
			break;
		}
		case T_FMAKEUPPERLIMIT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			CString s = CastStr(*p1);
			s.Trim();
			s = AfxGetCultureInfo()->TrimUpperLimitString(s);
			pData = new DataStr(s.IsEmpty() ? CParsedCtrl::Strings::LAST() : s);
			break;
		}
		case T_FGETUPPERLIMIT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			pData = new DataStr(AfxGetCultureInfo()->PadUpperLimitString(CastInt(*p1)));
			break;
		}
		case T_FVALUEOF:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			pData = new DataStr(AfxGetTbCmdManager()->NativeConvert(p1->m_pVal));
			break;
		}
		case T_FCONTENTOF:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			//TODO 
			pData = new DataStr(p1->m_pVal->Str());
			break;
		}
		case T_FTABLEEXISTS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString sTable = CastStr(*p1);
			CString sColumn;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				sColumn = CastStr(*p2);
				pData = new DataBool(AfxGetTbCmdManager()->TableExists(sTable, sColumn));
			}
			else
				pData = new DataBool(AfxGetTbCmdManager()->TableExists(sTable));
			
			break;
		}
		case T_FFILEEXISTS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString sFile = CastStr(*p1);

			pData = new DataBool(::PathFileExists(sFile));
			break;
		}

		case T_FGETSETTING:
		case T_FSETSETTING:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();
			p3 = (ExpItemVal*) paramStack.Pop();
			p4 = (ExpItemVal*) paramStack.Pop();

			CString sModuleOrFile = CastStr(*p1);  //puo' essere nella forma Framework.TbGenLib se il file di setting si chiama settings.config
													//oppure nella forma estesa Extensions.TbMailer.Smtp.config se il file config ha un nome diverso.
			CString sSection = CastStr(*p2);
			CString sSetting = CastStr(*p3);
			
			CTBNamespace nsModuleOrFile (CTBNamespace::MODULE, sModuleOrFile);
			CString sModule = cwsprintf(_T("%s.%s"), nsModuleOrFile.GetApplicationName(), nsModuleOrFile.GetModuleName());
			CTBNamespace nsModule(CTBNamespace::MODULE, sModule);
			if (!nsModule.IsValid())
			{
				pData = p4->m_pVal->DataObjClone();
				break;
			}
			
			CString sCandidateExt = cwsprintf(_T(".%s"), nsModuleOrFile.GetRightTokens(1));
			BOOL bHasFilename = sCandidateExt.Compare(szSettingsExt) == 0;
			CString sFileName = bHasFilename ? nsModuleOrFile.GetRightTokens(2) : _T("");

			DataObj* pSettingValue = AfxGetSettingValue
													(
														nsModule, 
														sSection, 
														sSetting,
														*(p4->m_pVal),
														sFileName
													);
			pData = pSettingValue->DataObjClone();

			if (itemFun->m_nFun == T_FGETSETTING)
				break;
			//-----------------------
			if (sFileName.IsEmpty())
			{
				sFileName = szTbDefaultSettingFileName;
			}

			AfxSetSettingValue
							(
								nsModule, 
								sSection, 
								sSetting,
								*(p4->m_pVal),
								sFileName
							);

			BOOL bOk = AfxSaveSettings	
							(
								nsModule, 
								sFileName,	
								sSection
							);	
			break;
		}

		case T_FCONVERT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			p2 = (ExpItemVal*) paramStack.Pop();

			DataType dt = p1->m_pVal->GetDataType(); //inserito a livello di parsing al posto del nome tipo
			
			pData = DataObj::DataObjCreate(dt);

			CString sValue = CastStr(*p2);

			pData->AssignFromXMLString(sValue);
			break;
		}
		case T_FTYPEOF:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataType dt = p1->m_pVal->GetDataType();

			CString sTypeName = Unparser::DataTypeToString(dt);
			if (dt == DataType::Array)
			{
				dt = ((DataArray*)(p1->m_pVal))->GetBaseDataType();
				sTypeName += ' ' + Unparser::DataTypeToString(dt);
			}

			pData = new DataStr(sTypeName);
			break;
		}
		case T_FGETTITLE:
		{
			CString sTitle;

			CObject* pO = paramStack.Pop();
			if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVrb)))
			{
				if (m_pSymTable)
				{
					ExpItemVrb* pVrb = (ExpItemVrb*) pO;

					SymField* pF = m_pSymTable->GetField(pVrb->m_strNameVrb);
					if (pF)
						sTitle = pF->GetTitle();
				}
			}
			else if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVal)))
			{
				p1 = (ExpItemVal*) pO;
				if (p1->m_pVal->GetDataType() == DataType::String)
				{
					sTitle = ((DataStr*) p1->m_pVal)->GetString();
					SymField* pF = m_pSymTable->GetField(sTitle);
					if (pF)
						sTitle = pF->GetTitle();
				}
			}
			pData = new DataStr(sTitle);
			break;
		}

		case T_FPREV_VALUE:
		{
			CObject* pO = paramStack.Pop();
			if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVrb)))
			{
				if (m_pSymTable)
				{
					ExpItemVrb* pVrb = (ExpItemVrb*)pO;

					SymField* pF = m_pSymTable->GetField(pVrb->m_strNameVrb);
					if (pF)
					{
						int lev = this->GetSymTable()->GetDataLevel();
						pData = pF->GetData(min(2, lev + 1))->Clone();
						break;
					}
				}
			}
			//TODO notifica errore
			break;
		}
		case T_FNEXT_VALUE:
		{
			CObject* pO = paramStack.Pop();
			if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVrb)))
			{
				if (m_pSymTable)
				{
					ExpItemVrb* pVrb = (ExpItemVrb*)pO;

					SymField* pF = m_pSymTable->GetField(pVrb->m_strNameVrb);
					if (pF)
					{
						int lev = this->GetSymTable()->GetDataLevel();
						pData = pF->GetData(max(0, lev - 1))->Clone();
						break;
					}
				}
			}
			//TODO notifica errore
			break;
		}

		case T_FADDRESSOF:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			long pointer = CastLng(*p1);
			pData = new DataLng(pointer); pData->SetAsHandle();
			break;
		}

		case T_FEXECUTESCRIPT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			CString sTbScript = CastStr(*p1);
			Parser lex(sTbScript); 

			CFunctionDescription aFunction(_T("void_noheader"));
			CDataObjDescription  aRetVal;
			aRetVal.SetDataType(DataType::Void);
			aFunction.SetReturnValueDescription(aRetVal);

			TBScript* pScript = AfxGetTbCmdManager ()->CreateTbScript(&aFunction, this->m_pSymTable); 
			BOOL bRet = pScript->Parse(lex) && pScript->Exec();

			delete pScript;
			pData = new DataBool(bRet);
			break;
		}

		case T_FGETTHREADCONTEXT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString sName = CastStr(*p1);

			CObject* pO = paramStack.Pop();

			pData = new DataBool(); //return FALSE

			if (!AfxGetThreadContextBag())
				break;

			CContextObject* pInfo = AfxGetThreadContextBag()->LookupContextObject(sName);
			if (!pInfo)
				break;
			if (!pInfo->IsKindOf(RUNTIME_CLASS(DataObj)))
			{
				ASSERT_TRACE1(FALSE,"Context Object %s is not a DataObj", sName);
				break;
			}

			/*if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVrb)))
			{
				if (m_pSymTable)
				{
					ExpItemVrb* pVrb = (ExpItemVrb*) pO;

					SymField* pF = m_pSymTable->GetField(pVrb->m_strNameVrb);
					if (pF)
					{
						pF->GetRepData()->Assign(*(DataObj*)pInfo);
						*((DataBool*)pData) = TRUE;	//return TRUE
					}
				}
			}
			else */
			if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVal)))
			{
				p1 = (ExpItemVal*) pO;
				if (p1->m_pVal->GetDataType() == DataType::String)
				{
					CString sName(((DataStr*) p1->m_pVal)->GetString());
					SymField* pF = m_pSymTable->GetField(sName);
					if (pF)
					{
						pF->GetRepData()->Assign(*(DataObj*)pInfo);
						*((DataBool*)pData) = TRUE;	//return TRUE
					}
				}
			}

			break;
		}
		case T_FOWNTHREADCONTEXT:
		{
			CObject* pO = paramStack.Pop();

			pData = new DataBool(); //return FALSE

			if (!AfxGetThreadContextBag())
				break;

			if (pO->IsKindOf(RUNTIME_CLASS(ExpItemVal)))
			{
				p1 = (ExpItemVal*) pO;

				if (p1->m_pVal->GetDataType() == DataType::String)
				{
					CString sName(((DataStr*) p1->m_pVal)->GetString());
					SymField* pF = m_pSymTable->GetField(sName);
					if (pF)
					{
						*((DataBool*)pData) = pF->OwnThreadContextVar();	//return TRUE
					}
				}
			}

			break;
		}

		//DataArray
		case T_FARRAY_CLEAR:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			ar->RemoveAll();

			pData = new DataLng(0);
			break;
		}
		case T_FARRAY_COPY:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			DataArray* arSrc = CastArray(*p2);

			if (!DataType::IsCompatible(arSrc->GetBaseDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				pData = new DataBool(FALSE);
				break;
			}

			ar->GetData().Copy(arSrc->GetData());

			pData = new DataBool(TRUE);
			break;
		}
		case T_FARRAY_APPEND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar1 = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar2 = CastArray(*p2);

			if (!DataType::IsCompatible(ar2->GetBaseDataType(), ar1->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				pData = new DataBool(FALSE);
				break;
			}
			ar1->GetData().Append(ar2->GetData());

			pData = new DataBool(TRUE);
			break;
		}
		case T_FARRAY_SORT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);
			BOOL bDescending = FALSE;
			int start = 0;
			int end = -1;
			int nStdParameters = parameters_of(itemFun->m_nFun);
			if (itemFun->m_nNumParam > nStdParameters)
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				bDescending = CastBool(*p2);

				if (itemFun->m_nNumParam > (nStdParameters + 1))
				{
					p3 = (ExpItemVal*)paramStack.Pop();
					start = CastLng(*p3);

					if (itemFun->m_nNumParam > (nStdParameters + 2))
					{
						p4 = (ExpItemVal*)paramStack.Pop();
						end = CastLng(*p4);
					}
				}
			}

			pData = new DataBool(ar->Sort(bDescending, start, end));
			break;
		}
		case T_FARRAY_SIZE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			pData = new DataLng(ar->GetSize());
			break;
		}

		case T_FARRAY_SUM:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			pData = DataObj::DataObjCreate(ar->GetBaseDataType());

			ar->CalcSum(*pData);
			break;
		}

		case T_FARRAY_ATTACH:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar1 = CastArray(*p1);
			p2 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar2 = CastArray(*p2);

			pData = new DataBool(ar1->Attach(ar2));
			break;
		}
		case T_FARRAY_DETACH:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			ar->Detach ();

			pData = new DataBool(TRUE);
			break;
		}
		case T_FARRAY_FIND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			if (!DataType::IsCompatible(p2->m_pVal->GetDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				break;
			}

			int nStartSearch = 0;
			if (itemFun->m_nNumParam > parameters_of(itemFun->m_nFun))
			{
				p3 = (ExpItemVal*) paramStack.Pop();
				nStartSearch = CastLng(*p3);
			}

			int idx = ar->Find(p2->m_pVal, nStartSearch);
			pData = new DataLng(idx);
			break;
		}
		case T_FARRAY_CONTAINS:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			if (!DataType::IsCompatible(p2->m_pVal->GetDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				pData = new DataBool(FALSE);
				break;
			}

			int idx = ar->Find(p2->m_pVal, 0);
			pData = new DataBool(idx != -1);
			break;
		}
		case T_FARRAY_GETAT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();

			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			int idx = CastLng(*p2);
			if (idx < 0 || idx >= ar->GetSize())
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}
			DataObj* pO = ar->GetAt(idx);
			if (pO == NULL)
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}
			pData = pO->DataObjClone();
			break;
		}
		case T_FARRAY_SETAT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			int idx = CastLng(*p2);
			if (idx < 0)
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}
			p3 = (ExpItemVal*) paramStack.Pop();
			if (!DataType::IsCompatible(p3->m_pVal->GetDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				break;
			}
			pData = p3->m_pVal->DataObjClone();
			ar->SetAtGrow(idx, pData->DataObjClone());
			break;
		}
		case T_FARRAY_REMOVE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			int idx = CastLng(*p2);
			if (idx < 0 || idx >= ar->GetSize())
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}

			pData = ar->GetAt(idx)->DataObjClone();

			ar->GetData().RemoveAt(idx);
			break;
		}

		case T_FARRAY_INSERT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			int idx = CastLng(*p2);
			if (idx < 0)
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				pData = new DataBool(FALSE);
				break;
			}

			p3 = (ExpItemVal*) paramStack.Pop();
			ASSERT_VALID(p3->m_pVal);

			if (!DataType::IsCompatible(p3->m_pVal->GetDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				pData = new DataBool(FALSE);
				break;
			}

			ar->GetData().InsertAt(idx, p3->m_pVal->DataObjClone());

			pData = new DataBool(TRUE);
			break;
		}

		case T_FARRAY_ADD:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataArray* ar = CastArray(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			if (!DataType::IsCompatible(p2->m_pVal->GetDataType(), ar->GetBaseDataType()))
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				pData = new DataBool(FALSE);
				break;
			}

			ar->SetAtGrow(ar->GetSize(), p2->m_pVal->DataObjClone());

			pData = new DataBool(TRUE);
			break;
		}

		case T_FARRAY_CREATE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			ASSERT_VALID(p1->m_pVal);
			DataType baseType = p1->m_pVal->GetDataType();
			DataType  needType = baseType;
			
			pData = new DataArray(baseType);
			((DataArray*)pData)->Add(p1->m_pVal->DataObjClone());

			for (int np = 1; np < itemFun->m_nNumParam; np++)
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				ASSERT_VALID(p2->m_pVal);
				DataType tv = p2->m_pVal->GetDataType();
				DataObj* pVal = NULL;
				if (!DataType::IsCompatible(tv, baseType) || baseType != needType)
				{
					if (
							(baseType == DataType::Integer || baseType == DataType::Long)
							/*&& 
							(p2->m_pVal->IsKindOf(RUNTIME_CLASS(DataLng)) || p2->m_pVal->IsKindOf(RUNTIME_CLASS(DataDbl)))*/
						)
					{
						if ((needType == DataType::Integer || needType == DataType::Long) && tv.IsReal())
							needType = tv;
						else if (needType == DataType::Integer && tv == DataType::Long)
							needType = tv;
					}
					else
					{		
						ASSERT(FALSE);
						//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
						break;
					}

					pVal = DataObj::DataObjCreate(needType);
					pVal->Assign(*p2->m_pVal);
				}
				else
				{
					pVal = p2->m_pVal->DataObjClone();
				}

				((DataArray*)pData)->Add(pVal);
			}
			if (needType != baseType)
			{
				if (!((DataArray*)pData)->FixDataType(needType))
				{
					ASSERT(FALSE);
					//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
					break;
				}
			}
			break;
		}

		//---------------------------------------------------
		case T_FSQLRECORD_GETFIELD:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataSqlRecord* dr = CastSqlRecord(*p1);

			p2 = (ExpItemVal*) paramStack.Pop();
			CString sRecField = CastStr(*p2);

			ISqlRecord* pIRec = dr->GetIRecord();
			if (!pIRec)
			{
				ASSERT(FALSE);
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				break;
			}
			DataObj* pObj = pIRec->GetDataObjFromColumnName(sRecField);
			if (!pObj)
			{
				ASSERT(FALSE);
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
				break;
			}

			pData = pObj->DataObjClone();
			break;
		}
		case T_FRECORD_GETFIELD:
		{
			//TODO
			ASSERT(FALSE);
			break;
		}

		case T_FOBJECT_GETFIELD:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataLng* obj = dynamic_cast<DataLng*>(p1->m_pVal);

			p2 = (ExpItemVal*) paramStack.Pop();
			CString sRecField = CastStr(*p2);

			//TODO
			ASSERT(FALSE);

			break;
		}

		//---------------------------------------------------
		case T_FCOLUMN_GETAT:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			SymField* pField = CastFieldProvider(*p1);
			if (!pField)
				break;

			p2 = (ExpItemVal*) paramStack.Pop();
			int idx = CastLng(*p2);

			if (idx < 0 || idx >= pField->GetProvider()->GetRowCount())
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}

			const DataObj* pO = pField->GetIndexedData(idx);
			ASSERT_VALID(pO);
			if (pO == NULL)
			{
				//TODO ErrHandler(UNKNOWN_FIELD, p2);
				break;
			}
			pData = pO->DataObjClone();
			break;
		}

		case T_FCOLUMN_FIND:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			SymField* pField = CastFieldProvider(*p1);
			if (!pField)
				break;

			p2 = (ExpItemVal*) paramStack.Pop();

			int idx = pField->GetProvider()->FindRecordIndex(pField->GetName(), p2->m_pVal);

			pData = new DataLng(idx);
			break;
		}
				
		case T_FCOLUMN_SIZE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			SymField* pField = CastFieldProvider(*p1);
			if (!pField)
				break;

			int idx = pField->GetProvider()->GetRowCount();

			pData = new DataLng(idx);
			break;
		}

		case T_FCOLUMN_SUM:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			SymField* pField = CastFieldProvider(*p1);
			if (!pField)
				break;

			DataObj* pSum = pField->GetData()->Clone();
			
			BOOL bOk = pField->GetProvider()->CalcSum(pField->GetName(), *pSum);

			pData = pSum;
			break;
		}

		//---------------------------------------------------
		case T_FDECODE:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			DataType baseDt = p1->m_pVal->GetDataType();
			
			int np = 1;
			while (np < (itemFun->m_nNumParam - 1))
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				p3 = (ExpItemVal*) paramStack.Pop();
				np += 2;

				if (!DataType::IsCompatible(p2->m_pVal->GetDataType(), baseDt))
				{
					//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
					break;
				}

				if (p1->m_pVal->IsEqual(*p2->m_pVal))
				{
					pData = p3->m_pVal->DataObjClone();
					break;
				}
			}

			//ha trovato la corrispondenza
			if (pData && np < itemFun->m_nNumParam)
			{
				for(; np < itemFun->m_nNumParam; np++)
					paramStack.Pop();
			}

			//non ha trovato la corrispondenza, tenta il default (ultimo parametro)
			if (!pData && (np + 1) == itemFun->m_nNumParam)
			{
				p3 = (ExpItemVal*) paramStack.Pop();

				pData = p3->m_pVal->DataObjClone();
			}

			break;
		}
		case T_FIIF:
		{
			int np = 0;
			while (np < (itemFun->m_nNumParam - 2))
			{
				p1 = (ExpItemVal*)paramStack.Pop();
				DataType dt = p1->m_pVal->GetDataType();
				if (dt != DataType::Bool)
					break;
				BOOL b = CastBool(*p1);

				p2 = (ExpItemVal*)paramStack.Pop();
				np += 2;
				
				if (b)
				{
					pData = p2->m_pVal->DataObjClone();
					break;
				}
			}

			//ha trovato la corrispondenza
			if (pData && np < itemFun->m_nNumParam)
			{
				for (; np < itemFun->m_nNumParam; np++)
					paramStack.Pop();
			}

			//non ha trovato la corrispondenza, tenta il default (ultimo parametro)
			if (!pData && (np + 1) == itemFun->m_nNumParam)
			{
				p3 = (ExpItemVal*)paramStack.Pop();

				pData = p3->m_pVal->DataObjClone();
			}

			break;
		}
		case T_FCHOOSE:
		{
			p1 = (ExpItemVal*)paramStack.Pop();
			int index = CastInt(*p1);
			if (index < 0 || index >= itemFun->m_nNumParam)
			{
				//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
			}
			else for (int np = 1; np < itemFun->m_nNumParam; np++)
			{
				p2 = (ExpItemVal*)paramStack.Pop();
				
				if (index == np)
				{
					pData = p2->m_pVal->DataObjClone();
				}
			}

			break;
		}

		case T_FFORMAT_TBLINK:
		{
			p1 = (ExpItemVal*) paramStack.Pop();
			CString sNs = CastStr(*p1);

			CString sParameters;
			int np = 1;
			while (np < (itemFun->m_nNumParam - 1))
			{
				p2 = (ExpItemVal*) paramStack.Pop();
				p3 = (ExpItemVal*) paramStack.Pop();
				np += 2;

				if (!p2->m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)))
				{
					//TODO ErrHandler(FORMAT_TYPEERR, itemFun);
					break;
				}
				
				CString sv(p3->m_pVal->Str(0,0));
				ASSERT(sv.Find(';') < 0);	//TODO 

				sParameters += CastStr(*p2) + ':' + sv + ';';
			}
			sParameters.TrimRight(';');

			CString strLink = ::GetTBNavigateUrl(sNs, sParameters);

			pData = new DataStr(strLink);
			break;
		}
		//---------------------------------------------------
	} // switch

	ExpItemVal* pRetVal = NULL;
	
	if (pData)
	{
		BOOL bValid = TRUE;

		if (p1) 
		{
			ASSERT_VALID(p1->m_pVal);
			bValid = bValid && (p1->m_pVal ? p1->m_pVal->IsValid() : FALSE);
		}
		if (p2) 
		{
			ASSERT_VALID(p2->m_pVal);
			bValid = bValid && (p2->m_pVal ? p2->m_pVal->IsValid() : FALSE);
		}
		if (p3) 
		{
			ASSERT_VALID(p3->m_pVal);
			bValid = bValid && (p3->m_pVal ? p3->m_pVal->IsValid() : FALSE);
		}

		pData->SetValid(bValid);

		pRetVal =  new ExpItemVal(pData, 0, TRUE);
	}
		
	if (p1 && p1->m_bToBeDeleted) delete p1;
	if (p2 && p2->m_bToBeDeleted) delete p2;
	if (p3 && p3->m_bToBeDeleted) delete p3;
	if (p4 && p4->m_bToBeDeleted) delete p4;
	if (p5 && p5->m_bToBeDeleted) delete p5;
	if (p6 && p6->m_bToBeDeleted) delete p6;
	if (p7 && p7->m_bToBeDeleted) delete p7;
	
	return pRetVal;
}

//-----------------------------------------------------------------------------
DataLng Expression::ApplyExternalFunctionWrp(DataLng dlItemFun, DataLng dlParamStack)
{
	ExpItemExternalFun* itemFun		= (ExpItemExternalFun*)(long)dlItemFun;
	Stack* 				paramStack	= (Stack*)(long)dlParamStack;

	ExpItemVal* pItmVal = ApplyExternalFunction(itemFun, paramStack);

	return DataLng(pItmVal, TRUE);
}

//-----------------------------------------------------------------------------
DataBool Expression::CallWebMethodWrp(DataLng dlFun)
{
	CFunctionDescription* pFD = (CFunctionDescription*)(long)dlFun;

	BOOL bOk = AfxGetTbCmdManager()->RunFunction(pFD, NULL);

	return DataBool(bOk);
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::ApplyExternalFunction(ExpItemExternalFun* itemFun, Stack* paramStack)
{
	ASSERT(itemFun->m_pFunctionDescription);

	ASSERT(itemFun->m_nActualParameters == paramStack->GetSize());
	ASSERT(itemFun->m_pFunctionDescription->GetParameters().GetSize() <= paramStack->GetSize());

	BOOL bLateBindingResolved = FALSE;
	if (itemFun->m_bLateBinding)
	{
		CString sHandleName;
		BOOL bOk = m_pSymTable->ResolveCallMethod
									(
										itemFun->m_sLateBindingName, 
										*itemFun->m_pFunctionDescription, 
										sHandleName
									);
		if (bOk)
			bLateBindingResolved = TRUE;
		else
		{
			bOk = m_pSymTable->ResolveCallProcedure
									(
										itemFun->GetFuncPrototype()->GetName(), 
										*itemFun->m_pFunctionDescription
									);
			if (bOk)
				itemFun->m_bIsProcedure = TRUE;

			if (!bOk)
			{
				bOk = m_pSymTable->ResolveCallQuery
									(
										itemFun->GetFuncPrototype()->GetName(),
										*itemFun->m_pFunctionDescription,
										sHandleName
									);
			}

			if (bOk)
				itemFun->m_bLateBinding = FALSE;
		}
	}

	ExpItemVal*	pParam;
	Array		ParamToDelete;

	for (int i = 0; i < itemFun->m_nActualParameters; i++)
	{
		pParam = (ExpItemVal*) paramStack->Pop();

		if (i < itemFun->m_pFunctionDescription->GetParameters().GetSize())
		{
			if (itemFun->m_pFunctionDescription->GetParamDescription(i)->IsPassedModeIn())
			{
				// se il parametro e` read only si assegna al DataObj preallocato una copia del valore
				itemFun->m_pFunctionDescription->GetParamDescription(i)->SetValue(*(pParam->m_pVal));
			}
			else
			{
				// altrimenti il parametro e` il DataObj stesso
				itemFun->m_pFunctionDescription->GetParamDescription(i)->SetDataObj(pParam->m_pVal);
			}
		}
		else //parametri optional: manca Description
		{
			CString strPName; strPName.Format(_T("__optional%d"), i);
			int j = itemFun->m_pFunctionDescription->AddParam(new CDataObjDescription(strPName, pParam->m_pVal->GetDataType(), CDataObjDescription::_INOUT));
			itemFun->m_pFunctionDescription->GetParamDescription(j)->SetDataObj(pParam->m_pVal);
		}

		// all'uscita il distruttore di Array eliminera` i parametri inseriti
		if (pParam->m_bToBeDeleted)
			ParamToDelete.Add(pParam);
	}

	AfxGetBaseApp()->ClearMessages();
			
	// ora si puo` eseguire la funzione
	ASSERT(itemFun->m_pFunctionDescription && (itemFun->m_pFunctionDescription->GetNamespace().GetType() == CTBNamespace::FUNCTION || itemFun->m_bLateBinding|| itemFun->m_bIsProcedure));
	
	BOOL bFail = (itemFun->m_bLateBinding && !bLateBindingResolved);
	if (!bFail)
	{
		bFail = m_pSymTable && m_pSymTable->GetDocument() 
					? !m_pSymTable->DispatchFunctionCall(itemFun->m_pFunctionDescription) && !m_pSymTable->GetDocument()->DispatchFunctionCall(itemFun->m_pFunctionDescription)
					: TRUE;
				
		if (bFail)
		{
			if (m_pSymTable && m_pSymTable->GetDocument() && m_pSymTable->GetDocument()->IsAWoormRunningMultithread())
			{
				HWND hwndThread = m_pSymTable->GetDocument()->GetFrameHandle();
				bFail = !AfxInvokeThreadFunction<DataBool, Expression, DataLng>(hwndThread, this, &Expression::CallWebMethodWrp, DataLng((long)itemFun->m_pFunctionDescription));
			}
			else
				bFail = !AfxGetTbCmdManager()->RunFunction(itemFun->m_pFunctionDescription, NULL);
		}
	}

	if (bFail)
	{
		// se non si riesce a risolvere la funzione di un documento allora ne viene invalidato
		// il risultato (che puo` essere cosi testato con l'operatore IS [NOT] NULL
		// (viene sfruttato per esempio per decidere se far apparire la AskDialog
		// nel caso in cui il report sia stato lanciato direttamente da WOORM oppure
		// da un documento che gli ha passatoo i parametri di input)
		BOOL bVoid = itemFun->m_pFunctionDescription->GetReturnValueDataType() == DataType::Void;
		if (!bVoid)
		{
			if (itemFun->m_pFunctionDescription->GetReturnValue())
				itemFun->m_pFunctionDescription->GetReturnValue()->SetValid(FALSE);
			else
			{
				ErrHandler(UNKNOWN_EXTERNAL_FUNC, itemFun);
				return NULL;
			}
		}
		
		// l'item non possiede il DataObj------------------------------------------------v
		return new ExpItemVal(itemFun->m_pFunctionDescription->GetReturnValue(), 0, TRUE, FALSE, bVoid);
	}
	else
	{
		CString	strMessage;
		UINT	nMsgStyle;
		if(AfxGetBaseApp()->ErrorFound())
		{
			strMessage	= AfxGetBaseApp()->GetError();
			nMsgStyle	= MB_ICONSTOP;
		}
		else
		{
			strMessage	= AfxGetBaseApp()->GetWarning();
			nMsgStyle	= MB_ICONEXCLAMATION;
		}
			
		AfxGetBaseApp()->ClearMessages();
					
		if (!strMessage.IsEmpty())
		{
			AfxMessageBox(strMessage, MB_OK | nMsgStyle);
			if (nMsgStyle == MB_ICONSTOP)
			{
				ErrHandler(EXTERNAL_FUNC_ERROR, itemFun);
				return NULL;
			}	
		}

		BOOL bVoid = itemFun->m_pFunctionDescription->GetReturnValueDataType() == DataType::Void;
		// l'item non possiede il DataObj------------------------------------------------v
		return new ExpItemVal(itemFun->m_pFunctionDescription->GetReturnValue(), 0, TRUE, FALSE, bVoid);
	}

	ErrHandler(UNKNOWN_EXTERNAL_FUNC, itemFun);
	return NULL;
}

//-----------------------------------------------------------------------------
ExpItemVal* Expression::ResolveSymbol(ExpItemVrb& itemVrb)
{
	if (m_pSymTable == NULL)
	{
		ErrHandler(NOSYMTABLE, &itemVrb);
		return NULL;
	}

	SymField* pField = m_pSymTable->GetField(itemVrb.m_strNameVrb);
	if (pField == NULL)
	{
		ErrHandler(UNKNOWN_FIELD, &itemVrb);
		return NULL;
	}

	// se si usa l'opzione di precompilazione il simbolo e` gia` stato risolto
	// nella fase di Compile
	if (m_bVrbCompiled && itemVrb.m_pData)
	{
		ASSERT(!pField->GetProvider());

		// l'item non possiede il DataObj-----------------------------------v
		return pField->GetProvider() 
					?
						new ExpItemValFromVar	(itemVrb.m_strNameVrb, itemVrb.m_pData, itemVrb.m_nPosInStr, TRUE, FALSE)
					:
						new ExpItemVal			(						itemVrb.m_pData, itemVrb.m_nPosInStr, TRUE, FALSE); 
	}


	//if (!m_bVrbCompiled)
	//	pField->IncRefCount();
	//else
	//{
	//	ASSERT(pField->GetRefCount() > 0);
	//}

	DataObj* pData = pField->GetData();
	if (pData == NULL)
	{
		if (pField->GetDataType() == DataType::Variant)
			return new ExpItemVal(NULL, itemVrb.m_nPosInStr, TRUE, FALSE, FALSE, TRUE);

		ErrHandler(UNKNOWN_FIELD, &itemVrb);
		return NULL;
	}

	if (!pData->IsValid())
	{
		m_sErrorDetail += ' ' + itemVrb.m_strNameVrb;
	}

	if (m_bVrbCompiled)
	{
		ASSERT(!pField->GetProvider());
		itemVrb.m_pData = pData;
	}

	// l'item non possiede il DataObj-------------------------v
	return pField->GetProvider() 
		?
			new ExpItemValFromVar	(itemVrb.m_strNameVrb, pData, itemVrb.m_nPosInStr, TRUE, FALSE)
		:
			new ExpItemVal			(						pData, itemVrb.m_nPosInStr, TRUE, FALSE);
}

//-----------------------------------------------------------------------------
BOOL Expression::Execute(Stack& workStack)
{
	ExpItemVal*	res = NULL;

	if (GetErrId()) 
		return FALSE;

	// gli item vengono "pop-ati" ma rimangono inseriti nello stack
	// originale che li possiede e li cancellera` nel distruttore
	
	switch (((ExpItem*) workStack.Top())->IsA())
	{
		case EXP_ITEM_OPE_CLASS:
		{
			ExpItemOpe* itemOpe = (ExpItemOpe*) workStack.Pop();

			if (!Execute(workStack)) 
				return FALSE;

			ExpItemVal* opr1 = (ExpItemVal*) workStack.Pop();

			switch (itemOpe->GetType())
			{
				case LOGICAL_OPR	:
				case UNARY_OPR		:
				{
					res = GiveMeResult(itemOpe, opr1);
					break;
				}
					
				case BINARY_OPR	:
				{
					if (!Execute(workStack))
					{
						if (opr1->m_bToBeDeleted) 
							delete opr1;

						return FALSE;
					}
					
					ExpItemVal* opr2 = (ExpItemVal*) workStack.Pop();
					
					res = GiveMeResult(itemOpe, opr2, opr1);
					break;
				}
				
				case TERNARY_OPR:
				{
					if (!Execute(workStack))
					{
						if (opr1->m_bToBeDeleted) delete opr1;

						return FALSE;
					}

					ExpItemVal* opr2 = (ExpItemVal*) workStack.Pop();

					if (!Execute(workStack))
					{
						if (opr1->m_bToBeDeleted) delete opr1;
						if (opr2->m_bToBeDeleted) delete opr2;

						return FALSE;
					}

					ExpItemVal* opr3 = (ExpItemVal*) workStack.Pop();
					
					res = GiveMeResult(itemOpe, opr3, opr2, opr1);
					break;
				}
			}

			if (res == NULL) 
				return FALSE;

			workStack.Push(res);
			break;
        }

		case EXP_ITEM_FUN_CLASS:
		{
			ExpItemFun* itemFun = (ExpItemFun*) workStack.Pop();

			Stack paramStack;

			for (int p = itemFun->m_nNumParam; p > 0; p--)
			{
				if (!Execute(workStack))
				{
					ASSERT(itemFun->m_nFun != T_FISNULL);	//TODO
					while (!paramStack.IsEmpty())
					{
						ExpItem* itm = (ExpItem*) paramStack.Pop();
						if	(
								itm && itm->IsA() == EXP_ITEM_VAL_CLASS &&
								((ExpItemVal*)itm)->m_bToBeDeleted
							)
							delete itm;
					}

					m_sErrorDetail += cwsprintf(_TB(" Error on parameter  evaluation on position {0-%d} of internal function {1-%s}" ), itemFun->m_nNumParam - p,  AfxGetTokensTable()->ToString(itemFun->m_nFun));
					return FALSE;
				}
				
				ExpItem* itm = (ExpItem*) workStack.Pop();

				if ((itemFun->m_nFun == T_FISNULL) && p == 1)
				{
					if (itm->IsA () == EXP_ITEM_VAL_CLASS)
					{
						ExpItemVal* itmVal = dynamic_cast<ExpItemVal*>(itm);
						if (!itmVal->m_pVal->IsValid())
						{
							itmVal->m_pVal->Clear();
							itmVal->m_pVal->SetValid(FALSE);
						}
					}
				}
				else if ((itemFun->m_nFun == T_FISEMPTY) && p == 1)
				{
					if (itm->IsA () == EXP_ITEM_VAL_CLASS)
					{
						ExpItemVal* itmVal = dynamic_cast<ExpItemVal*>(itm);
						if (!itmVal->m_pVal->IsValid())
						{
							itmVal->m_pVal->Clear();
							itmVal->m_pVal->SetValid();
						}
					}
				}

				paramStack.Push(itm);
			}
			
			res = ApplyFunction(itemFun, paramStack);

			ASSERT(paramStack.GetSize() == 0);

			if (res == NULL) 
			{
				m_sErrorDetail += cwsprintf(_TB(" Error on evaluation of internal function {0-%s}" ),  AfxGetTokensTable()->ToString(itemFun->m_nFun));
				return FALSE;
			}

			workStack.Push(res);
			break;
        }

		case EXP_ITEM_EXTERNAL_FUN_CLASS:
		{
			ExpItemExternalFun* itemFun = (ExpItemExternalFun*) workStack.Pop();

			Stack paramStack;

			for (int p = itemFun->m_nActualParameters; p > 0; p--)
			{
				if (!Execute(workStack))
				{
					while (!paramStack.IsEmpty())
					{
						ExpItem* itm = (ExpItem*) paramStack.Pop();
						if	(
								itm && itm->IsA() == EXP_ITEM_VAL_CLASS &&
								((ExpItemVal*)itm)->m_bToBeDeleted
							)
							delete itm;
					}
					m_sErrorDetail += cwsprintf(_TB(" Error on parameter  evaluation on position {0-%d} of external function {1-%s}" ), itemFun->m_nActualParameters - p, itemFun->GetFuncPrototype()->GetNamespace().ToString());
			
					return FALSE;
				}
				
				paramStack.Push(workStack.Pop());
			}
			
			//if (m_pSymTable && m_pSymTable->GetDocument() && m_pSymTable->GetDocument()->IsAWoormRunningMultithread())
			//{
			//	HWND hwndThread = m_pSymTable->GetDocument()->GetFrameHandle();
			//	DataLng dlRet = AfxInvokeThreadFunction<DataLng, Expression, DataLng, DataLng>(hwndThread, this, &Expression::ApplyExternalFunctionWrp, DataLng((long)itemFun), DataLng((long)&paramStack) );

			//	res = (ExpItemVal*)(long) dlRet;
			//}
			//else 
				res = ApplyExternalFunction(itemFun, &paramStack);

			ASSERT(paramStack.GetSize() == 0);

			if (res == NULL) 
			{
				m_sErrorDetail += cwsprintf(_TB(" Error on evaluation of external function {0-%s}" ), itemFun->GetFuncPrototype()->GetNamespace().ToString());
				return FALSE;
			}

			workStack.Push(res);
			break;
        }

		case EXP_ITEM_VRB_CLASS:
		{
			ExpItemVrb* itemVrb = (ExpItemVrb*) workStack.Pop();

			res = ResolveSymbol(*itemVrb);
			if (res == NULL) 
			{
				m_sErrorDetail += cwsprintf(_TB(" Error on evaluation of variable {0-%s}" ), itemVrb->m_strNameVrb);
				return FALSE;
			}

			workStack.Push(res);
			break;
		}
	} // switch

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL Expression::Compile(Parser& lex, Stack& workStack, Stack& outStack)
{
	ExpItemVal*	res = NULL;

	if (lex.ErrorFound() || workStack.Top() == NULL)
		return FALSE;

	switch (((ExpItem*) workStack.Top())->IsA())
	{
		case EXP_ITEM_VAL_CLASS:
			res = dynamic_cast<ExpItemVal*>(workStack.Top());
			outStack.Push(res->Clone());
			res->m_bToBeDeleted = TRUE;
			//ASSERT(res);
			break;

		case EXP_ITEM_OPE_CLASS:
		{
			ExpItemOpe* itemOpe = (ExpItemOpe*) workStack.Pop();
			outStack.Push(itemOpe);

			if (!Compile(lex, workStack, outStack))
				return FALSE;

			ExpItemVal* opr1 = (ExpItemVal*) workStack.Pop();
			ASSERT_VALID(opr1);

			switch (itemOpe->GetType())
			{
				case LOGICAL_OPR	:
				case UNARY_OPR		:
				{
					res = CheckType(lex, itemOpe, opr1);
					//ASSERT(res);
					break;
				}
					
				case BINARY_OPR	:
				{
					if (!Compile(lex, workStack, outStack))
					{
						if (opr1->m_bToBeDeleted) delete opr1;
						return FALSE;
					}
					
					ExpItemVal* opr2 = (ExpItemVal*) workStack.Pop();
	
					res = CheckType(lex, itemOpe, opr2, opr1);
					//ASSERT(res);
					break;
				}
				
				case TERNARY_OPR:
				{
					if (!Compile(lex, workStack, outStack))
					{
						if (opr1->m_bToBeDeleted) delete opr1;
						
						return FALSE;
					}

					ExpItemVal* opr2 = (ExpItemVal*) workStack.Pop();

					if (!Compile(lex, workStack, outStack))
					{
						if (opr1->m_bToBeDeleted) delete opr1;
						if (opr2->m_bToBeDeleted) delete opr2;
						
						return FALSE;
					}

					ExpItemVal* opr3 = (ExpItemVal*) workStack.Pop();

					res = CheckType(lex, itemOpe, opr3, opr2, opr1);
					//ASSERT(res);
					break;
				}
				//default: ASSERT(res);
			}

			if (res == NULL) 
				return FALSE;

			workStack.Push(res);
			break;
        }

		case EXP_ITEM_FUN_CLASS:
		{
			ExpItemFun* itemFun = dynamic_cast<ExpItemFun*>(workStack.Pop());
			DataType	paramDataType;
			DataType	expectedDataType;
			DataType	resDataType;
			DataType	returnDataType;
			DataType	variantDataType;

			outStack.Push(itemFun);

			int	numFixParam = parameters_of(itemFun->m_nFun);
			for (int p = itemFun->m_nNumParam; p > 0; p--)
			{
				if (!Compile(lex, workStack, outStack))
					return FALSE;

				if (p > numFixParam)
					expectedDataType = opt_param_fun(itemFun->m_nFun, p - numFixParam);
				else
					expectedDataType = param_fun(itemFun->m_nFun, p);
				ASSERT(expectedDataType != DataType::Null);

				ExpItemVal* itemVal = dynamic_cast<ExpItemVal*>(workStack.Pop());
				paramDataType = itemVal->GetDataType();

				//---- check return Variant type 
				if 
					(
						p == 1 && 
						itemVal->m_pVal &&
						itemVal->m_pVal->IsKindOf(RUNTIME_CLASS(DataArray)) &&
						(
							itemFun->m_nFun == T_FARRAY_GETAT 
							||
							itemFun->m_nFun == T_FARRAY_SETAT
							||
							itemFun->m_nFun == T_FARRAY_REMOVE
							||
							itemFun->m_nFun == T_FARRAY_SUM
						)
					)
				{
					returnDataType = ((DataArray*)(itemVal->m_pVal))->GetBaseDataType();
				}
				else if 
					(
						p == 1 && 
						itemVal->m_pVal &&
						itemVal->IsKindOf(RUNTIME_CLASS(ExpItemValFromVar)) &&
						(
							itemFun->m_nFun == T_FCOLUMN_GETAT 
							|| 
							itemFun->m_nFun == T_FCOLUMN_SUM
						)
					)
				{
					//per campi con data provider
					ASSERT(! dynamic_cast<ExpItemValFromVar*>(itemVal)->m_strNameVrb.IsEmpty() );
					returnDataType = itemVal->m_pVal->GetDataType();
				}
				else if 
					(
						p == 1 && 
						itemVal->m_pVal &&
						itemFun->m_nFun == T_FCONVERT
					)
				{
					//T_FCONVERT: il nome del tipo passato viene convertito in parsing in un value
					returnDataType = itemVal->m_pVal->GetDataType();
				}
				else if 
					(
						p == 1 && 
						itemVal->m_pVal &&
						itemFun->m_nFun == T_FFORMAT
					)
				{
					variantDataType = itemVal->m_pVal->GetDataType();
				}		
		
				else if 
					(
						p == 2 && 
						itemVal->m_pVal &&
						(
							itemFun->m_nFun == T_FARRAY_FIND
							||
							itemFun->m_nFun == T_FARRAY_ADD
							||
							itemFun->m_nFun == T_FCOLUMN_FIND
						)
					)
				{
					variantDataType = itemVal->m_pVal->GetDataType();
				}
				else if
					(
						p == 2 &&
						itemVal->m_pVal &&
						(itemFun->m_nFun == T_FCHOOSE || itemFun->m_nFun == T_FIIF)
						)
				{
					returnDataType = itemVal->m_pVal->GetDataType();
				}

				else if 
					(
						p == 3 && 
						itemVal->m_pVal 
					)
				{
					if (
							itemFun->m_nFun == T_FARRAY_SETAT
							||
							itemFun->m_nFun == T_FARRAY_INSERT
						)
					{
						variantDataType = itemVal->m_pVal->GetDataType();
					}
					else if (itemFun->m_nFun == T_FDECODE)
					{
						returnDataType = itemVal->m_pVal->GetDataType();
					}
				}

				else if 
					(
						p == 4 && 
						itemVal->m_pVal &&
						(itemFun->m_nFun == T_FGETSETTING || itemFun->m_nFun == T_FSETSETTING)
					)
				{
					returnDataType = itemVal->m_pVal->GetDataType();
				}

				//---- check parameter Variant type 
				if 
					(
						p == 1 && 
						itemVal->m_pVal
					)
				{
					if (itemVal->m_pVal->IsKindOf(RUNTIME_CLASS(DataArray)))
					{
						if (itemFun->m_nFun == T_FARRAY_SETAT)
						{
							if (!Compatible(variantDataType, returnDataType))
							{
								return lex.SetError(FormatMessage(PARAM), lex.GetTokenString(itemFun->m_nFun), itemFun->m_nPosInStr);
							}
						}
						else if (
							itemFun->m_nFun == T_FARRAY_FIND ||
							itemFun->m_nFun == T_FARRAY_INSERT ||
							itemFun->m_nFun == T_FARRAY_ADD
							)
						{
							if (!Compatible(variantDataType, ((DataArray*)(itemVal->m_pVal))->GetBaseDataType()))
							{
								return lex.SetError(FormatMessage(PARAM), lex.GetTokenString(itemFun->m_nFun), itemFun->m_nPosInStr);
							}
						}
						else if (itemFun->m_nFun == T_FARRAY_REMOVE || itemFun->m_nFun == T_FARRAY_SUM)
						{
							returnDataType =  ((DataArray*)itemVal->m_pVal)->GetBaseDataType();
						}
					}
					else if (
								itemFun->m_nFun == T_FMIN || itemFun->m_nFun == T_FMAX || 
								itemFun->m_nFun == T_FPREV_VALUE || itemFun->m_nFun == T_FNEXT_VALUE
							)
					{
						returnDataType = itemVal->m_pVal->GetDataType();
					}
					else if (itemFun->m_nFun == T_FCOLUMN_FIND)
					{
						if (!Compatible(variantDataType, itemVal->m_pVal->GetDataType()))
						{
							return lex.SetError(FormatMessage(PARAM), lex.GetTokenString(itemFun->m_nFun), itemFun->m_nPosInStr);
						}
					}
				}

				delete itemVal;

				if (expectedDataType != DATA_NULL_TYPE)
				{
					if (!Compatible(paramDataType, expectedDataType))
					{
						return lex.SetError( FormatMessage(PARAM), lex.GetTokenString(itemFun->m_nFun), itemFun->m_nPosInStr);
					}
				}
			}
			
			if (returnDataType != DataType::Null)
			{
				res = new ExpItemVal(DataObj::DataObjCreate(returnDataType), 0, TRUE);
			}
			else
				res = ReturnFunDataObj(itemFun->m_nFun);

			if (res == NULL)
				return lex.SetError(FormatMessage (RETTYPE), lex.GetTokenString(itemFun->m_nFun), itemFun->m_nPosInStr);

			workStack.Push(res);
			break;
		}

		case EXP_ITEM_EXTERNAL_FUN_CLASS:
		{
			ExpItemExternalFun* itemFun = (ExpItemExternalFun*) workStack.Pop();
			if (itemFun->m_bLateBinding && GetSymTable())
			{
				CFunctionDescription fd;
				if (GetSymTable()->ResolveCallProcedure(itemFun->m_pFunctionDescription->GetName(), fd))
					*itemFun->m_pFunctionDescription = fd;
			}

			outStack.Push(itemFun);

			CFunctionDescription* pFuncPrototype = itemFun->m_pFunctionDescription;
			if (itemFun->m_nActualParameters > pFuncPrototype->GetParameters().GetSize())
			{
				for (int p = itemFun->m_nActualParameters; p > pFuncPrototype->GetParameters().GetSize(); p--)
				{
					if (!Compile(lex, workStack, outStack))
						return FALSE;

					ExpItemVal* itemVal = (ExpItemVal*) workStack.Pop();
					delete itemVal;
				}
			}

			for (int p = pFuncPrototype->GetParameters().GetUpperBound(); p >= 0; p--)
			{
				if (!Compile(lex, workStack, outStack))
					return FALSE;
	
				ExpItemVal* itemVal = (ExpItemVal*) workStack.Pop();
				DataType paramDataType		= itemVal->GetDataType();
				DataType expectedDataType	= pFuncPrototype->GetParamDescription(p)->GetDataType();
				delete itemVal;
	
				if (!Compatible(paramDataType, expectedDataType))
					return lex.SetError
						(
							FormatMessage(PARAM),
							pFuncPrototype->GetName(),
							itemFun->m_nPosInStr
						);
			}

			if (itemFun->m_pFunctionDescription->GetReturnValueDataType() == DataType::Void)
			{
				res = new ExpItemVal(NULL, 0, TRUE, FALSE, TRUE);
			}
			else if (itemFun->m_pFunctionDescription->GetReturnValueDataType() == DataType::Variant)
			{
				res = new ExpItemVal(NULL, 0, TRUE, FALSE, FALSE, TRUE);
			}
			else
			{
				DataObj* pObj = itemFun->m_pFunctionDescription->GetReturnValue();
				//ASSERT(pObj);
				res = new ExpItemVal(pObj, 0, TRUE, FALSE);
			}

			if (res == NULL)
				return lex.SetError
					(
						FormatMessage(UNKNOW),
						pFuncPrototype->GetName(),
						itemFun->m_nPosInStr
					);

			workStack.Push(res);
			break;
		}

		case EXP_ITEM_VRB_CLASS:
		{
			ExpItemVrb* itemVrb = (ExpItemVrb*) workStack.Pop();
			outStack.Push(itemVrb);

			res = ResolveSymbol(*itemVrb);

			if (res == NULL)
				return lex.SetError(FormatMessage(GetErrId()), itemVrb->m_strNameVrb, itemVrb->m_nPosInStr);

			workStack.Push(res);
			break;
		}
	} // switch

	return TRUE;
}

//-----------------------------------------------------------------------------
CString Expression::FormatMessage	(Expression::MessageID ID)
{
	switch(ID)
	{
		case EMPTY_MESSAGE		: return _T("");
		case PARAM				: return _TB("Wrong parameter for the function");
		case UNKNOW             : return _TB("Unknown error");
		case EXPREMPTY          : return _TB("Empty expression not allowed");
		case RETTYPE            : return _TB("Wrong type expression result");
		case NOSYMTABLE         : return _TB("Undefined symbol table");
		case UNKNOWN_FIELD      : return _TB("Unknown identifier");
		case UNKNOWN_DB_COL_FIELD		 : return _TB("Unknown database column identifier");
		case CHECKTYPE          : return _TB("Operation not allowed");
		case NULL_OPR           : return _TB("Unable to evaluate the expression with one or more unevaluated operands");
		case UNKNOWN_CALLER_DOC : return _TB("Unable to call external functions");
		case UNKNOWN_EXTERNAL_FUNC: return _TB("External function not available");
		case EXTERNAL_FUNC_ERROR : return _TB("Error in the execution of the external function");
		case DIVISION_BY_ZERO    : return _TB("Division by ZERO: operation aborted");
		case FORMAT_TYPEERR		 : return _TB("Incompatible format type");
		case FORMAT_UNKNOWN		 : return _TB("Unknown format type");
		case SYNTAX_ERROR		 : return _TB("Syntax error");
		case EXTERNAL_FUNC_MISSING_ROUNDCLOSE: return _TB("The closing parenthesis of the External Function parameters is missing");
		case EXTERNAL_FUNC_MISSING_COMMA: return _TB("A comma is missing between the External Function parameters");
		case EXTERNAL_FUNC_TOOMANY_PARAMETERS: return _TB("Too many parameters in the External Function");
		case EXTERNAL_FUNC_MISSING_PARAMETERS: return _TB("The External Function needs further parameters");
		case EXTERNAL_FUNC_BAD_PARAMETER: return _TB("One parameter of the External Function is syntactically incorrect");
		case FORBIDDEN_FIELD      : return _TB("Forbidden identifier in this context");
	}

	ASSERT(FALSE);
	return _T("");
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void Expression::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	dc << _T("\n\tExpression = ") << this->GetRuntimeClass()->m_lpszClassName << _T(", ") << m_strExprString;
}

void Expression::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG

