/////////////////////////////////////////////////////////////////////////
// 24-06-1996 :	la sintassi accetta:
//
// <where_clause>		::=	<base_where_clause> |
//							IF <bool_expression>
//								THEN <where_clause> | ALL | BREAK
//								[ ELSE <where_clause> | ALL | BREAK ]
//
// <base_where_clause>	::= NATIVE <sql_expression> | <sql_like_expression>
//////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include <TbNameSolver\Chars.h>

#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\baseapp.h>

#include "sqlcatalog.h"
#include "sqltable.h"
#include "wclause.h"
#include "oledbmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// Tag di 1 byte (faccina che sorride nel codice ascii) per contrassegnare il che
// il byte seguente nella stringa della espressione e` da interpretare con indice
// del parametro i-esimo
//
static const TCHAR  chSubstPrefix = 0x01;

//-----------------------------------------------------------------------------
BOOL IsPhysicalName(const CString& strName, const SqlTableInfoArray* parSqlTableInfo, CStringArray* parstrAlias)
{
	int i = 0;
	for (i = 0 ; i < parSqlTableInfo->GetSize(); i++)
		if (IsPhysicalName(strName, parSqlTableInfo->GetAt(i)))
			return TRUE;

	if (parstrAlias == NULL) 
		return FALSE;
	int nDot = strName.Find(DOT_CHAR);
	if (nDot < 0) 
		return FALSE;

	CString strAlias = strName.Left(nDot);
	CString strColName = strName.Mid(nDot+1);
	for (i = 0; i < parstrAlias->GetSize(); i++)
	{
		if (strAlias.CompareNoCase(parstrAlias->GetAt(i)) ==0)
			break;
	}
	if (i == parstrAlias->GetSize() || i >= parSqlTableInfo->GetSize()) 
		return FALSE;

	return IsPhysicalName(strColName, parSqlTableInfo->GetAt(i));
}

//-----------------------------------------------------------------------------
BOOL IsPhysicalName(const CString& strName, const SqlTableInfo* pSqlTableInfo)
{
    int nIdx = strName.Find(DOT_CHAR);

	// per fisico si intende un dato appartenente and una tabella ossia : table.variabile
	if (nIdx > 0)
	{
	    CString strTableName = pSqlTableInfo->GetTableName();

		if (strTableName.CompareNoCase(strName.Left(nIdx)) != 0)
			return FALSE;
			
		return pSqlTableInfo->ExistColumn(strName.Mid(nIdx + 1));
	}

    return pSqlTableInfo->ExistColumn(strName);
}

// Strippa la parte di nome che e` relativa alla tabella
//-----------------------------------------------------------------------------
CString GetPhysicalName(const CString& strFromName, const SqlTableInfoArray* parSqlTableInfo /* = NULL */)
{
	for (int i=0; i < parSqlTableInfo->GetSize(); i++)
	{
		CString str = GetPhysicalName(strFromName, parSqlTableInfo->GetAt(i));
		if (!str.IsEmpty())
			return str;
	}
	return _T("");
}

//-----------------------------------------------------------------------------
CString GetPhysicalName(const CString& strFromName, const SqlTableInfo* pSqlTableInfo /* = NULL */)
{
    CString	strColName, strTableName;
  
	int	nIdx = strFromName.Find(DOT_CHAR); 
	if (nIdx > 0)
	{
		strColName = strFromName.Mid(nIdx + 1);
		strTableName = strFromName.Left(nIdx+1);
	}
	else
	{
		strColName = strFromName;

		// se e` stato passato un SqlTableInfo vuol dire che ci si vuol far tornare
		// il nome qualificato
		if (pSqlTableInfo)
			strTableName = pSqlTableInfo->GetTableName() + DOT_CHAR;
	}
  
	return strTableName + strColName;
}

//----------------------------------------------------------------------------
// Esegue il parsing delle colonne calcolate per verificare la sintassi delle eventuali ContentOf
BOOL ParseCalculateColumn(CString str, SymTable* pSymTable, CString& err)
{
	Parser parser(str);

	do
	{
		if (parser.LookAhead(T_FCONTENTOF))
		{
			parser.EnableAuditString();

			if (!parser.Match(T_FCONTENTOF) || !parser.Match(T_ROUNDOPEN))
				return FALSE;

			if (!parser.SkipToToken(T_ROUNDCLOSE, TRUE, TRUE, TRUE))
				return FALSE;

			CString sContentOf = parser.GetAuditString();

			if (pSymTable)
			{
				int b = sContentOf.Find('(');
				CString exprC = sContentOf.Mid(b + 1);
				int f = exprC.ReverseFind(')');
				exprC = exprC.Left(f);
				exprC.Trim();

				Expression expr(pSymTable);
				if (!expr.Parse(exprC, DataType::String))
				{
					err = expr.GetErrDescription();
					return FALSE;
				}
			}

			parser.EnableAuditString(FALSE);
		}
	} while (parser.SkipToken() != T_EOF);

	if (parser.ErrorFound())
	{
		err = parser.GetError();
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
// Esegue il parsing del predicato ORDER BY 
// E' utilizzata anche per la GROUP BY (elenco di colonne della tabella)
BOOL ParseOrderBy	(
						Parser& parser,
						SymTable* pSymTable,
						const SqlTableInfoArray* parSqlTableInfo,
						CString& strOrderBy,
						BOOL bQualified,	 /* = FALSE */
						CStringArray* items	/*  = NULL */,
						CStringArray* parstrAliasTableName /*  = NULL */
					)
{
	CString strTmp;
	CString strPhysicalName;

    do
	{
		if (parser.LookAhead(T_FCONTENTOF))
		{
			CString sPrecAuditString;
			BOOL bPrecAuditingState = parser.IsAuditStringOn();
			if (bPrecAuditingState) 
				sPrecAuditString = parser.GetAuditString();
			else
				parser.EnableAuditString();

			if (!parser.Match(T_FCONTENTOF) || !parser.Match(T_ROUNDOPEN))
				return FALSE;

			if (!parser.SkipToToken(T_ROUNDCLOSE, TRUE, TRUE, TRUE))
				return FALSE;

			CString sContentOf = parser.GetAuditString();

			if (pSymTable)
			{
				int b = sContentOf.Find('(');
				CString exprC = sContentOf.Mid(b + 1);
				int f = exprC.ReverseFind(')');
				exprC = exprC.Left(f);
				exprC.Trim();

				Expression expr(pSymTable);
				if (!expr.Parse(exprC, DataType::String))
				{
					return FALSE;
				}
			}

			strTmp += sContentOf;

			if (bPrecAuditingState)
			{
				parser.ConcatAuditString(sPrecAuditString + sContentOf);
			}
			else
			{
				parser.EnableAuditString(FALSE);
			}
		}
		else
		{
			if (!parser.ParseSquaredCoupleIdent(strPhysicalName))
				return FALSE;

			if (!IsPhysicalName(strPhysicalName, parSqlTableInfo, parstrAliasTableName))
        		return parser.SetError(Expression::FormatMessage (Expression::UNKNOWN_FIELD), strPhysicalName);

			if (items) items->Add(strPhysicalName);

			// l'espressione viene memorizzata cosi` come l'utente l'ha scritta
			// (piu` o meno)
			
			strTmp += BLANK_CHAR;
		
			if (bQualified)
				strTmp += GetPhysicalName(strPhysicalName, parSqlTableInfo);
			else
				strTmp += strPhysicalName;
		}
		Token nTok = parser.LookAhead();
			
		if (nTok != T_EOF && nTok != T_COMMA && nTok != T_SEP && nTok != T_ORDER && nTok != T_HAVING && nTok != T_WHEN)
			if (nTok == T_DESCENDING || nTok == T_ASCENDING)
			{
				strTmp += BLANK_CHAR;
				strTmp += cwsprintf(parser.SkipToken());
			}
			else
				return parser.SetError(Expression::FormatMessage (Expression::SYNTAX_ERROR));
			
		if (parser.LookAhead() == T_COMMA)
			strTmp += cwsprintf(T_COMMA);
			
	} while (parser.Matched(T_COMMA));

	if (parser.ErrorFound())	return FALSE;

	strOrderBy = strTmp;

	return TRUE;	
}

//-----------------------------------------------------------------------------
BOOL ExpandContentOfClause(CString& strSql, SymTable* pSymTable, int nStartPos/* = 0*/)
{
	Parser parser(strSql);
	BOOL bOk = parser.SkipToToken(T_FCONTENTOF);

	if (!parser.Matched(T_FCONTENTOF)) 
	{
		return FALSE;
	}
	if (!parser.Match(T_ROUNDOPEN)) 
	{
		return FALSE;
	}

	Expression expr(pSymTable);
	expr.SetStopTokens(T_ROUNDCLOSE); expr.GetStopTokens()->m_bSkipInnerRoundBrackets = TRUE;	
	if (!expr.Parse(parser, DataType::String, TRUE))
	{
		return FALSE;
	}

	int nEnd = parser.GetCurrentPos();
	ASSERT(nEnd >= 0);

	if (!parser.Match(T_ROUNDCLOSE)) 
	{
		return FALSE;
	}

	DataStr dsDynamicContentOf;
	if (!expr.Eval(dsDynamicContentOf))
	{
		return FALSE;
	}
		
	CString s1,s2;
	s1 = strSql.Left(nStartPos);
	s2 = strSql.Mid(nEnd + 1);
	strSql = s1 + dsDynamicContentOf.GetString() + s2; 
	return TRUE;
}

//-----------------------------------------------------------------------------
//new syntax 'ContentOf( <woorm-string-expr> )'
// replace function with <woorm-string-expr> evalutation value 
BOOL ExpandContentOfClause (CString& strSql, SymTable* pSymTable, SqlConnection* pSqlConnection)
{
	int nPos = -1;
	CString sTk(AfxGetTokensTable()->ToString(T_FCONTENTOF)); sTk.MakeLower();
	strSql.Replace('\n', ' ');
	strSql.Replace('\r', ' ');

	while (TRUE)
	{
		CString sSqlLower(strSql); sSqlLower.MakeLower();
		if ((nPos = sSqlLower.Find(sTk)) < 0)
			break;

		if (!ExpandContentOfClause(strSql, pSymTable, nPos))
			return FALSE;
	}

	CString str(strSql);
	return WClauseExpr::ConvertToNative(str, strSql, pSymTable, pSqlConnection, NULL);
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ExpItemValWC, ExpItemVal)

//-----------------------------------------------------------------------------
ExpItemValWC::ExpItemValWC
	(
		DataObj* pData,
		int nPos,
		BOOL bToBeDeleted,
		BOOL bOwnsData,
		const CWordArray* pCompatTypes
	)
	:
	ExpItemVal				(pData, nPos, bToBeDeleted, bOwnsData),
	m_pCompatibleDataTypes	(NULL)
{
	if (pCompatTypes)
	{
		m_pCompatibleDataTypes = new CWordArray;
	
		for (int i = 0 ; i <= pCompatTypes->GetUpperBound(); i++)
			m_pCompatibleDataTypes->Add(pCompatTypes->GetAt(i));
	}
}

//-----------------------------------------------------------------------------
ExpItemValWC::~ExpItemValWC()
{
	if (m_pCompatibleDataTypes)
		delete m_pCompatibleDataTypes;
}

//-----------------------------------------------------------------------------
ExpItem* ExpItemValWC::Clone()
{
	return new ExpItemValWC
			(
				m_bOwnsData ? m_pVal->DataObjClone() : m_pVal,
				m_nPosInStr,
				m_bToBeDeleted,
				m_bOwnsData,
				m_pCompatibleDataTypes
			);
}

//===========================================================================
//		Local ParamItem class
//===========================================================================
class ParamItem : public CObject
{
public:
	CString		m_strParamName;
	DataObj*	m_pAssociatedData;
	BOOL		m_bOwnsDataObj;

public:
	ParamItem(const CString& strParamName, DataObj* pDataObj, BOOL bOwnDataObj);
	~ParamItem();
};

//-----------------------------------------------------------------------------
ParamItem::ParamItem(const CString& strParamName, DataObj* pDataObj, BOOL bOwnDataObj)
	:
	m_strParamName		(strParamName),
	m_pAssociatedData	(pDataObj),
	m_bOwnsDataObj		(bOwnDataObj)
{}

//-----------------------------------------------------------------------------
ParamItem::~ParamItem()
{
	if (m_bOwnsDataObj)
		delete m_pAssociatedData;
}

//===========================================================================
//		Local ParamsArray class
//===========================================================================
class ParamsArray : public Array
{
	DECLARE_DYNAMIC(ParamsArray)

public:
	ParamItem* 	GetAt		(int nIndex)	const	{ return (ParamItem*) Array::GetAt(nIndex);	}
	ParamItem*&	ElementAt	(int nIndex)			{ return (ParamItem*&) Array::ElementAt(nIndex); }
	
	ParamItem* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	ParamItem*& operator[]	(int nIndex)			{ return ElementAt(nIndex);	}
	
	CString		AddParamGetName	(const CString&, DataObj*, BOOL bOwnDataObj = FALSE);
	ParamItem*	AddParamGetItem	(const CString&, DataObj*, BOOL bOwnDataObj = FALSE);
};

IMPLEMENT_DYNAMIC(ParamsArray, Array);

// viene ritornata una stringa di due caratteri costuita da un prefisso
// (necessario per l'individuazione della stessa nell'espressione transitoria)
// piu` il carattere corrispondente alla posizione del ParamItem inserito nell'array
// locale (incrementato di 1 per evitare lo 0, che altrimenti farebbe da terminatore
// della stringa)
//-----------------------------------------------------------------------------
CString ParamsArray::AddParamGetName(const CString& strVrbName, DataObj* pDataObj, BOOL bOwnDataObj /* = FALSE */)
{
	AddParamGetItem(strVrbName, pDataObj, bOwnDataObj);
	return CString(chSubstPrefix) + (TCHAR)GetSize();	
}

// viene ritornata una stringa di due caratteri costuita da un prefisso
// (necessario per l'individuazione della stessa nell'espressione transitoria)
// piu` il carattere corrispondente alla posizione del ParamItem inserito nell'array
// locale (incrementato di 1 per evitare lo 0, che altrimenti farebbe da terminatore
// della stringa)
//-----------------------------------------------------------------------------
ParamItem* ParamsArray::AddParamGetItem(const CString& strVrbName, DataObj* pDataObj, BOOL bOwnDataObj /* = FALSE */)
{
	int nIdx = GetSize();
	VERIFY(nIdx++ < 255);
	
	CString strParamName = strVrbName + cwsprintf(_T("_%d"), nIdx);

	nIdx = Add(new ParamItem(strParamName, pDataObj, bOwnDataObj));

	return GetAt(nIdx);
}

///////////////////////////////////////////////////////////////////////////////
//		Class WClauseExpr implementation
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(WClauseExpr, Expression)

//-----------------------------------------------------------------------------
WClauseExpr::WClauseExpr
	(
		SqlConnection*		pSqlConnection,
		SymTable*			pSymTable,
		const SqlTableJoinInfoArray*	parSqlTableInfo/*= NULL */,
		SqlTable*			pSqlTable /*= NULL */
	)
	:
	Expression		(pSymTable),
	m_pSqlConnection(pSqlConnection),
	m_pSqlTable		(pSqlTable),
	m_pParamsArray	(NULL),
	m_EmptyWhere	(T_NOTOKEN),
	m_bNative		(FALSE),
	m_eClauseType	(EClauseType::WHERE),
	m_nPrepareTableIndex (0)
{
	if (parSqlTableInfo)
		m_arSqlTableInfo = *parSqlTableInfo;
}

//-----------------------------------------------------------------------------
WClauseExpr::WClauseExpr(const WClauseExpr& aWClause)
	:
	Expression		(NULL),
	m_pSqlTable		(NULL),
	m_pParamsArray	(NULL),
	m_EmptyWhere	(T_NOTOKEN),
	m_bNative		(FALSE),
	m_eClauseType   (EClauseType::WHERE),
	m_nPrepareTableIndex(0)
{
	*this = aWClause;
}

//-----------------------------------------------------------------------------
WClauseExpr::~WClauseExpr()
{
	SAFE_DELETE (m_pParamsArray);
}

//-----------------------------------------------------------------------------
WClauseExpr* WClauseExpr::Clone	()
{
	return new WClauseExpr(*this);
}

//-----------------------------------------------------------------------------
void WClauseExpr::SetForbiddenPublicIdents (const CStringArray& arForbiddenPublicIdents)
{ 
	m_arForbiddenPublicIdents.Append(arForbiddenPublicIdents); 
}

//-----------------------------------------------------------------------------
void WClauseExpr::Reset(BOOL bResetAll/* = TRUE*/)
{
	Expression::Reset(bResetAll);

	SAFE_DELETE(m_pParamsArray);
	m_EmptyWhere	= T_NOTOKEN;
	m_bNative		= FALSE;

	m_arCommentTraceBefore.RemoveAll();
	m_arCommentTraceAfter.RemoveAll();

	if (bResetAll)
	{
		m_arSqlTableInfo.RemoveAll();
		m_pSqlTable		= NULL;
	}
}

//-----------------------------------------------------------------------------
void WClauseExpr::operator = (const WClauseExpr& aWClause)
{
	Expression::Assign(aWClause);

	m_arForbiddenPublicIdents.Append(aWClause.m_arForbiddenPublicIdents);

	m_pSqlConnection = aWClause.m_pSqlConnection;
	m_arSqlTableInfo = aWClause.m_arSqlTableInfo;

	m_pSqlTable		= aWClause.m_pSqlTable;
	m_EmptyWhere	= aWClause.m_EmptyWhere;
	m_bNative		= aWClause.m_bNative;
	m_eClauseType   = aWClause.m_eClauseType;
	m_nPrepareTableIndex = aWClause.m_nPrepareTableIndex;

	if (m_pParamsArray)
		delete m_pParamsArray;

	m_pParamsArray	= NULL;

	if (aWClause.m_pParamsArray)
	{
		m_pParamsArray = new ParamsArray;
		for (int i = 0; i < aWClause.m_pParamsArray->GetSize(); i++)
		{
			ParamItem* pParamItem = aWClause.m_pParamsArray->GetAt(i);
			m_pParamsArray->Add(new ParamItem
				(
					pParamItem->m_strParamName,
					pParamItem->m_pAssociatedData,
					pParamItem->m_bOwnsDataObj
				));
		}
	}
	m_arCommentTraceBefore.Copy(aWClause.m_arCommentTraceBefore);
	m_arCommentTraceAfter.Copy(aWClause.m_arCommentTraceAfter);
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::Parse(Parser& lex)
{
	m_arCommentTraceBefore.RemoveAll();
	m_arCommentTraceAfter.RemoveAll();

	m_strExprString.Empty();
	m_ExprStack.ClearStack();

	lex.GetCommentTrace(this->m_arCommentTraceBefore);

	if (lex.Matched(T_BREAK))
	{
		m_EmptyWhere = T_BREAK;
		return TRUE;
	}

	if (lex.Matched(T_ALL))
	{
		m_EmptyWhere = T_ALL;
		return TRUE;
	}

	m_EmptyWhere = T_NOTOKEN;

	BOOL bParseTagNative = FALSE;
	if (lex.Matched(T_NATIVE))
	{
		bParseTagNative = TRUE;
		m_bNative = TRUE;
	}

	if (m_bNative)
	{
		BOOL bOk = ParseNative(lex);

		if (bParseTagNative && ::FindWord(m_strExprString, cwsprintf(T_NATIVE)) < 0)
			m_strExprString = cwsprintf(T_NATIVE) + BLANK_CHAR + m_strExprString;

		lex.GetCommentTrace(this->m_arCommentTraceAfter);

		return bOk;
	}

	// si imposta altri token di chiusura dell'espressione 

	//TODO solo se c'e' un IF prima
	AddStopTokens(T_ELSE);

	if (this->m_eClauseType == WHERE)
	{
		AddStopTokens(T_GROUP);
		AddStopTokens(T_HAVING);
		AddStopTokens(T_ORDER);
		AddStopTokens(T_WHEN);
	}
	else if (this->m_eClauseType == HAVING)
	{
		AddStopTokens(T_ORDER);
		AddStopTokens(T_WHEN);
	}
	else if (this->m_eClauseType == JOIN_ON)
	{
		AddStopTokens(T_INNER);
		AddStopTokens(T_CROSS);
		AddStopTokens(T_FULL);
		AddStopTokens(T_LEFT);
		AddStopTokens(T_RIGHT);

		AddStopTokens(T_SELECT);	//dopo le join
	}

	//lex.EnableAuditString();

	// per il parser dell'espressione e` necessario conoscere il tipo di ritorno
	// per poter effettuare gli opportuni check di congruenza, una Where Clause
	// per definizione dovrebbe tornare TRUE o FALSE
	if (!Expression::Parse(lex, DATA_BOOL_TYPE, TRUE))
		return FALSE;
	
	// l'espressione non puo` essere vuota
	if (IsEmpty())
		return lex.SetError(Expression::FormatMessage (Expression::SYNTAX_ERROR));

	lex.GetCommentTrace(this->m_arCommentTraceAfter);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::ParseNative(Parser& lex)
{
	m_EmptyWhere = T_NOTOKEN;
	m_bNative = TRUE;
	m_strExprString.Empty();
	m_ExprStack.ClearStack();

	int nOpenRoundBrackets = 0;
	int nOpenCASE = 0;
	
	lex.EnableAuditString();

	while (!lex.ErrorFound())
	{
		Token tk = lex.LookAhead();

		if	(tk == T_CASE)
			nOpenCASE++;

		if	(nOpenCASE > 0 && tk == T_END)
			nOpenCASE--;

		if	(tk == T_EOF || tk == T_SEP || nOpenCASE == 0 && tk == T_ELSE)
			break;

		if	(tk == T_ROUNDOPEN)
			nOpenRoundBrackets++;
		else if	(tk == T_ROUNDCLOSE)
			nOpenRoundBrackets--;

		if	(nOpenRoundBrackets == 0 && (tk == T_ORDER || tk == T_GROUP || nOpenCASE == 0 && tk == T_WHEN))
			break;

		if	(!ParseVariableOrConstForNativeWhere(lex))
			break;
	}

	CString strSubexpr = lex.GetAuditString();
	lex.EnableAuditString(FALSE);

	if (nOpenRoundBrackets != 0)
		lex.SetError(Expression::FormatMessage(Expression::SYNTAX_ERROR), ToString());

	if (lex.ErrorFound())
		return FALSE;

	if (!strSubexpr.IsEmpty())
	{
		m_strExprString += strSubexpr;
		if (strSubexpr.Left(1) != AfxGetTokensTable()->ToString(T_COLON) )	
			m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
	}

	// elimina i CR/LF in coda alla stringa
	int nLen = m_strExprString.GetLength();
	TCHAR* pszExprString = m_strExprString.GetBuffer(nLen);
	int i = nLen - 1;
	while (i >= 0 && (pszExprString[i] == LF_CHAR || pszExprString[i] == CR_CHAR)) i--;
	m_strExprString.ReleaseBuffer(i + 1);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::ParseVariableOrConstForNativeWhere(Parser& lex)
{
	Token tk = lex.LookAhead();
	switch (tk)
	{
		case T_ID:
	    {
			CString strSubexpr = lex.GetAuditString();

			CString strName;
			if (!lex.ParseID(strName))
				return FALSE;

			if (m_pSymTable)
			{
				ASSERT_VALID(m_pSymTable);
				SymField* pField = m_pSymTable->GetField(strName);
				if (pField) 
				{
					pField->IncRefCount();
					m_pSymTable->TraceFieldsUsed(pField->GetName());

					if (CStringArray_Find(m_arForbiddenPublicIdents, strName) == -1)
					{
						if (!strSubexpr.IsEmpty())
						{
							m_strExprString += strSubexpr;
							m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
						}
			
						m_strExprString += lex.GetAuditString();
						m_ExprStack.Add(new ExpItemVrb(strName, lex.GetCurrentPos()));
						break;
					}
				}
			}

			// altrimenti e` un token del linguaggio SQL
			// corretta an. 24514
			// caso speciale costante stringa unicode tipo : N'0001'
			// eliminiamo la N perche' verra' riaggiunta successivamente dalla nativeconvert
			BOOL bSkipUnicodeMark = m_pSqlConnection ?
									((strName.Compare(L"N") == 0) && lex.LookAhead(T_STR) && m_pSqlConnection->UseUnicode() && m_pSqlConnection->m_pProviderInfo->UseConstParameter())
									:
									FALSE;
			if (!bSkipUnicodeMark)
			{
				strSubexpr += strName + BLANK_CHAR;
				m_strExprString += strSubexpr;
			}

			m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			(void) lex.GetAuditString();
			break;
	    }
	
		case T_BRACEOPEN :
		{
			CString strSubexpr = lex.GetAuditString();
			lex.SkipToken();
			DataObj* pData = NULL;

			if (lex.Matched(T_EVAL))
            {
				m_bHasDynamicFragment = TRUE;

				Expression expr(m_pSymTable);

				expr.SetStopTokens(T_BRACECLOSE); expr.GetStopTokens()->m_bSkipInnerBraceBrackets = TRUE;
				if (!expr.Parse(lex, DataType::Variant, FALSE))
				{
					//SetError(_TB("Error on parsing conditional expression of query tag"));
					return FALSE;
				}
				if (!lex.Match(T_BRACECLOSE)) 
				{
					//SetError(_TB("Expected } token"));
					return FALSE;
				}

				if (!expr.Eval(pData))	//alloca e valorizza pData
				{
					return FALSE;
				}
			}
			else 
			{
				pData = lex.ParseComplexData(FALSE);
			}

			if (!pData)
			{
				return FALSE;
			}

			if (!strSubexpr.IsEmpty())
			{
				m_strExprString += strSubexpr;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}

			m_strExprString += lex.GetAuditString();

			//@@OLE
			if (m_pSqlConnection && m_pSqlConnection->m_pProviderInfo->UseConstParameter())
			{
				strSubexpr = m_pSqlConnection->NativeConvert(pData) + BLANK_CHAR;
				delete pData;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}
			else
				m_ExprStack.Add(new ExpItemVal(pData, lex.GetCurrentPos()));

			break;
		}
	
		case T_STR:
	    {
			CString strSubexpr = lex.GetAuditString();

			CString	aString;
			if (!lex.ParseString(aString)) return FALSE;
			DataStr* pData = new DataStr(aString);

			if (!strSubexpr.IsEmpty())
			{
				m_strExprString += strSubexpr;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}

			m_strExprString += lex.GetAuditString();
			
			if (m_pSqlConnection && m_pSqlConnection->m_pProviderInfo->UseConstParameter())
			{
				strSubexpr = m_pSqlConnection->NativeConvert(pData) + BLANK_CHAR;
				delete pData;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}
			else
				m_ExprStack.Add(new ExpItemVal(pData, lex.GetCurrentPos()));

			break;
	    }

		case T_EQ:
		case T_NE:
		{
			//sostituisce gli operatori di confronto sql
			CString sAudit = lex.GetAuditString();
			lex.EnableAuditString(FALSE);
			lex.SkipToken();
			lex.EnableAuditString(TRUE);
			lex.ConcatAuditString(sAudit);
			lex.ConcatAuditString(::cwsprintf(tk == T_EQ ? T_ASSIGN : T_DIFF));
			break;
		}

		case T_TRUE:
		case T_FALSE:
		{
			CString strSubexpr = lex.GetAuditString();   
			
			Token token = lex.SkipToken();
			DataBool*	pData = new DataBool(token == T_TRUE);

			if (!strSubexpr.IsEmpty())
			{
				m_strExprString += strSubexpr;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}

			m_strExprString += lex.GetAuditString();

			if (m_pSqlConnection && m_pSqlConnection->m_pProviderInfo->UseConstParameter())
			{
				strSubexpr = m_pSqlConnection->NativeConvert(pData) + BLANK_CHAR;
				delete pData;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}
			else
				m_ExprStack.Add(new ExpItemVal(pData, lex.GetCurrentPos()));

			break;
        }

		case T_FCONTENTOF:
		{
			m_bHasDynamicFragment = TRUE;

			CString strSubexpr = lex.GetAuditString();   
			if (!strSubexpr.IsEmpty())
			{
				m_strExprString += strSubexpr;
				m_ExprStack.Add(new ExpItemValWC(new DataStr(strSubexpr), lex.GetCurrentPos()));
			}

			ExpParse ep (m_pSymTable);
			if (!ep.ParseContentOfFunc(lex, m_ExprStack, TRUE)) 
				return lex.SetError(Expression::FormatMessage(Expression::SYNTAX_ERROR), _T("(wrong ContentOf) ") + ToString());

			m_strExprString += lex.GetAuditString();
			break;
		}

 		default :
			lex.SkipToken();
			break;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::ConvertToNative (const CString& sSource, CString& sWhere)
{
	return ConvertToNative
	(
		sSource, sWhere,
		m_pSymTable,
		m_pSqlConnection,
		&m_arForbiddenPublicIdents
	);
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::ConvertToNative
	(
		const CString& sSource, CString& sWhere,
		SymTable* pSymTable,
		SqlConnection* pSqlConnection,
		CStringArray* parForbiddenPublicIdents
	)
{
	ASSERT_VALID(pSqlConnection);
	sWhere.Empty();

	Parser lex (sSource);
	lex.EnableAuditString();

	while (!lex.ErrorFound())
	{
		Token tk = lex.LookAhead();
		if	(tk == T_EOF)
			break;

		switch (lex.LookAhead())
		{
			case T_ID:
			{
				sWhere += lex.GetAuditString();

				CString strName;
				if (!lex.ParseID(strName)) 
					return FALSE;

				SymField* pF = pSymTable ?  pSymTable->GetField(strName) : NULL;
				if (pF && pF->GetData())
				{
					if (parForbiddenPublicIdents && CStringArray_Find(*parForbiddenPublicIdents, strName) > -1)
					{
						break;
					}

					sWhere += pSqlConnection->NativeConvert(pF->GetData()) + BLANK_CHAR;

					(void) lex.GetAuditString();
					break;
				}
				// altrimenti e` un token del linguaggio SQL
				break;
			}
	
			case T_BRACEOPEN :
			{
				sWhere += lex.GetAuditString();
                lex.SkipToken();

				DataObj* pData = NULL;

                if (lex.Matched(T_EVAL))
                {
					//m_bHasDynamicFragment = TRUE;

					Expression expr(pSymTable);

					expr.SetStopTokens(T_BRACECLOSE); expr.GetStopTokens()->m_bSkipInnerBraceBrackets = TRUE;
					if (!expr.Parse(lex, DataType::Variant, FALSE))
					{
						//SetError(_TB("Error on parsing conditional expression of query tag"));
						return FALSE;
					}
					if (!lex.Match(T_BRACECLOSE)) 
					{
						//SetError(_TB("Expected } token"));
						return FALSE;
					}

					if (!expr.Eval(pData))	//alloca e valorizza pData
					{
						return FALSE;
					}
				}
				else 
				{
				/* TODO
					//skip sintassi T-SQL {fn Hour (colonna) }
					if (lex.LookAhead(T_ID) && lex.GetCurrentStringToken().CompareNoCase(L"fn") == 0)
					{
						lex.SkipToToken2(T_BRACECLOSE, TRUE, TRUE);
						sWhere += lex.GetAuditString();
						break;
					}
				*/
					pData = lex.ParseComplexData(FALSE);
				}

				if (!pData)
				{
					return FALSE;
				}
			
				sWhere += pSqlConnection->NativeConvert(pData) + BLANK_CHAR;

				SAFE_DELETE(pData);

				(void) lex.GetAuditString();
				break;
			}
	
			case T_STR:
			{
				sWhere += lex.GetAuditString();

				CString	aString;
				if (!lex.ParseString(aString)) 
					return FALSE;

				DataStr aData(aString);

				sWhere += pSqlConnection->NativeConvert(&aData) + BLANK_CHAR;

				(void) lex.GetAuditString();
				break;
			}
	
			case T_TRUE:
			case T_FALSE:
			{
				sWhere += lex.GetAuditString();   
			
				lex.SkipToken();

				DataBool aData(tk == T_TRUE);

				sWhere += pSqlConnection->NativeConvert(&aData) + BLANK_CHAR;

				(void) lex.GetAuditString();
				break;
			}

 			default :
				lex.SkipToken();
				break;
		}
	}
	
	sWhere += lex.GetAuditString();   
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::ModifyVariableOrConst	(Stack& modifiedStack)
{
	// Al posto di costanti e variabili utente devono essere sostituiti
	// i "?" come vuole la sintassi per il binding dei parametri.
	// Anche le costanti sono bindate in questo modo poiche cosi` facendo non ci
	// si deve porre il problema della convenzione di "quoting" dello specifico DataBase
	// lasciando all'interfaccia tra DataObj e parametro di ODBC il compito di quotare
	// correttamente
	//
    for (int i = 0; i < modifiedStack.GetSize(); i++)
    {
        ExpItem* pExpItem = (ExpItem*) modifiedStack[i];

        switch(pExpItem->IsA())
        {
        	case EXP_ITEM_OPE_CLASS :	// e` un operatore
			{
				if	(
						!ModifyVariableOrConst(((ExpItemOpe*)pExpItem)->m_frstOpStack) ||
						!ModifyVariableOrConst(((ExpItemOpe*)pExpItem)->m_scndOpStack)
					)
					return FALSE;

				break;
			}
        	case EXP_ITEM_VRB_CLASS :	// e` una variabile
			{
				// strVrbName e` un REFERENCE a CString poiche` il suo contenuto verra` modificato
				CString& strVrbName = ((ExpItemVrb*) pExpItem)->m_strNameVrb;

				// si verifica che sia un campo della tabella corrente o di symbol table o di entrambe
				BOOL bPhysicalCol = IsPhysicalName(strVrbName, &m_arSqlTableInfo);

				SymField* pField = m_pSymTable ? m_pSymTable->GetField(strVrbName) : NULL;
				DataObj* pDataObj = pField ? pField->GetData() : NULL;

				if (pDataObj)
				{
					BOOL bForbidden = CStringArray_Find(m_arForbiddenPublicIdents, strVrbName) > -1;

					if (bForbidden)
					{
						if (bPhysicalCol)
						{
							//nome ambiguo (lista dei campi "INTO"), essendo proibito sarà il nome della colonna
							pDataObj = NULL;
						}
						else
						{
							m_sErrorDetail = cwsprintf(_TB("The variable name {0-%s} cannot be used in this context\n"), strVrbName);
							return ErrHandler(FORBIDDEN_FIELD);
						}
					}
				}

				if (pDataObj)
				{
					if (bPhysicalCol)
						TRACE("WClauseExpr::ModifyVariableOrConst: ambiguity between column name and variable name %s\n", strVrbName);

					// E` una variabile utente con un DataObj associato
					if (m_pParamsArray == NULL)
						m_pParamsArray = new ParamsArray;
										
					// SOSTIUISCE il nome della variabile con una stringa transitoria ritornata
					// dalla AddParamGetName (cfr. piu` sopra)
					//
					strVrbName = m_pParamsArray->AddParamGetName(strVrbName, pDataObj);
				}
				else
				{
					if (!bPhysicalCol)
					{
						m_sErrorDetail = cwsprintf(_TB("Unknown variable name {0-%s}\n"), strVrbName);
						return ErrHandler(UNKNOWN_FIELD, pExpItem);
					}

					// se e` un campo di tabella viene "qualificato"
					strVrbName = GetPhysicalName(strVrbName, &m_arSqlTableInfo);
				}
				break;
        	}
        	case EXP_ITEM_VAL_CLASS :	// e` una costante
			{
				// Al posto dell'ExpItemVal (costante) trovato si sostituisce una variabile fittizia
				// e si cancella l'ExpItemVal che non serve piu`
				// COSTRUISCE il nome della variabile con una stringa corrispondente alla
				// rappresentazione specifica per il data base corrente della costante trovata
				CString strVrbName;

				ExpItemVal* pExpItemVal = (ExpItemVal*) pExpItem;
				DataObj* pDataObj = pExpItemVal->m_pVal;
	
				if (pExpItemVal->IsKindOf(RUNTIME_CLASS(ExpItemContentOfVal)))
				{
					strVrbName = ((DataStr*)pExpItemVal->m_pVal)->GetString();

					CString sConverted;
					if (ConvertToNative(strVrbName, sConverted))
						strVrbName = sConverted;

					strVrbName += BLANK_CHAR;
				}
				else if (m_pSqlConnection->m_pProviderInfo->UseConstParameter())
				{
					strVrbName = m_pSqlConnection->NativeConvert(pDataObj) + BLANK_CHAR;
				}
				else
				{
					if (m_pParamsArray == NULL)
						m_pParamsArray = new ParamsArray;
	
					// Il DataObj del corrente ExpItemVal viene "POSSEDUTO" dal ParaItem ----v
					strVrbName = m_pParamsArray->AddParamGetName(_T("Param"), pDataObj, TRUE);

					// Dato che verra` deletato il ExpItemVal ci si assicura che non venga cancellato
					// il DataObj associato riutilizzato dal ParamItem
					pExpItemVal->m_bOwnsData = FALSE;
				}

				modifiedStack[i] = new ExpItemVrb(strVrbName);
				delete pExpItemVal;
				
				break;
			}
        }
    }
	return TRUE;
}

//-----------------------------------------------------------------------------
void WClauseExpr::AddSqlParam(const CString& sParamName, DataObj* pAssociatedData)
{
	ASSERT_VALID(pAssociatedData);
	m_pSqlTable->AddParam(sParamName, *pAssociatedData);
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::BindParams(SqlTable* pSqlTable /*= NULL*/, BOOL bAppend /*=FALSE*/)
{
	ASSERT(!m_bNative);
	
	if (pSqlTable)
		m_pSqlTable = pSqlTable;

	ASSERT_VALID(m_pSqlTable);
		
	if (m_pParamsArray)
		m_pParamsArray->RemoveAll();
	
	if (!bAppend) // se non sono nella condizione di append pulisco i parametri della query
	{
		m_pSqlTable->ClearParams();

		if (m_eClauseType == EClauseType::WHERE)
			m_pSqlTable->m_strFilter.Empty();
		else if (m_eClauseType == EClauseType::HAVING)
			m_pSqlTable->m_strHaving.Empty();
		else if (m_eClauseType == EClauseType::JOIN_ON)
			m_pSqlTable->m_strFrom.Empty();
	}

	if (IsEmpty())
	{
		if (m_eClauseType == EClauseType::JOIN_ON)
		{
			ASSERT(m_nPrepareTableIndex > 0 && m_nPrepareTableIndex < m_arSqlTableInfo.GetSize());
			SqlTableJoinInfoArray::EJoinType eJoinType = m_arSqlTableInfo.m_arJoinType[m_nPrepareTableIndex];
			if (eJoinType != SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				ASSERT(FALSE);
				eJoinType = SqlTableJoinInfoArray::EJoinType::CROSS;
			}
		}
		else
			return TRUE;
	}
	// Si costruisce un vettore di ParamItem tramite il quale (quando verra`
	// chiamato il metodo Eval) verranno effettuate le SetParamValue
	
	Stack modifiedStack;
	ExpParse::ExpandStack(m_ExprStack, modifiedStack);

	ModifyVariableOrConst(modifiedStack);

	// Finalmente si Unparsa l'espressione per ottenerne una sua versione in formato stringa
	// da passare a ODBC
	//
	CString strClause;             
	ExpUnparse expUnparse;
	
	if (!IsEmpty() && !expUnparse.Unparse(strClause, modifiedStack))
		return ErrHandler(UNKNOWN_FIELD);

	// La Where clause potrebbe anche non avere riferimenti a costanti o variabili
	if (m_pParamsArray && m_pParamsArray->GetSize() > 0)
	{
		// ora bisogna riscandire l'espressione in formato stringa e sostituire ai
		// nomi "transitori" inseriti la stringa "? " e contemporaneamente effettuare
		// la AddParam(name, type)
		//
		int len = strClause.GetLength();
		LPTSTR pszFilter = strClause.GetBuffer(len);
		
		for (int i = 0; i < len; i++)
			if (pszFilter[i] == chSubstPrefix)
			{
				// un blank e` gia` stato inserito dall'unparser e quindi si puo` sostituire
				// il chSubstPrefix trovato con il "?" come desiderato da ODBC
				//
				pszFilter[i++] = _T('?');
				
				// il byte successivo contiene l'indice (incrementato di 1: vedi
				// ParamsArray::AddParamGetName()) dell'array ove e` memorizzato il parametro
				//
				int nArrayIdx = (int) pszFilter[i] - 1;
				ParamItem* pItem = m_pParamsArray->GetAt(nArrayIdx);
				pszFilter[i] = BLANK_CHAR;
				if (bAppend)
					pItem->m_strParamName += cwsprintf(_T("_%d"), nArrayIdx);
			
				// Vengono "bindati" alla table passata i parametri sulla base della
				// espressione
				//
				//@@OLEDB
				AddSqlParam(pItem->m_strParamName, pItem->m_pAssociatedData);
			}
			
		strClause.ReleaseBuffer();
	}

	// si memorizza nella variabile del SqlTable la nuova WhereClause costruita
	// se sono in append devo aggiungere una nuova condizione
	if (m_eClauseType == EClauseType::WHERE)
	{
		if (bAppend && !m_pSqlTable->m_strFilter.IsEmpty())
			m_pSqlTable->m_strFilter += _T(" AND ");
		m_pSqlTable->m_strFilter += strClause;
	}
	else if (m_eClauseType == EClauseType::HAVING)
	{
		if (bAppend && !m_pSqlTable->m_strHaving.IsEmpty()) 
			m_pSqlTable->m_strHaving += _T(" AND ");
		m_pSqlTable->m_strHaving += strClause;
	}
	else if (m_eClauseType == EClauseType::JOIN_ON)
	{
		ASSERT(m_nPrepareTableIndex > 0 && m_nPrepareTableIndex < m_arSqlTableInfo.GetSize());

		if (this->m_nPrepareTableIndex == 1 && m_pSqlTable->m_strFrom.IsEmpty())
		{
			m_pSqlTable->m_strFrom += this->m_arSqlTableInfo[0]->GetTableName() + ' ';
		}

		ASSERT(m_arSqlTableInfo.m_arJoinType.GetSize() == m_arSqlTableInfo.GetSize());

		SqlTableJoinInfoArray::EJoinType eJoinType = m_arSqlTableInfo.m_arJoinType[m_nPrepareTableIndex];

		if (eJoinType == SqlTableJoinInfoArray::EJoinType::INNER)
		{
			m_pSqlTable->m_strFrom += L" Inner Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::CROSS)
		{
			m_pSqlTable->m_strFrom += L" Cross Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::LEFT_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Left Outer Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Right Outer Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::FULL_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Full Outer Join ";
		}
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}

		m_pSqlTable->m_strFrom += m_arSqlTableInfo[m_nPrepareTableIndex]->GetTableName();

		if (eJoinType != SqlTableJoinInfoArray::EJoinType::CROSS)
		{
			m_pSqlTable->m_strFrom += L" On ";
			m_pSqlTable->m_strFrom += strClause; 
		}
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::BindParamsNative(SqlTable* pSqlTable /*= NULL*/, BOOL bAppend /*=FALSE*/)
{
	ASSERT(m_bNative);
	
	if (pSqlTable)
		m_pSqlTable = pSqlTable;

	ASSERT_VALID(m_pSqlTable);
		
	if (m_pParamsArray)
		m_pParamsArray->RemoveAll();
	
	// se non sono nella condizione di append la table viene ripulita da eventuali 
	// parametri precedenti
	if (!bAppend) 
	{
		m_pSqlTable->ClearParams();

		if (m_eClauseType == EClauseType::WHERE)
			m_pSqlTable->m_strFilter.Empty();
		else if (m_eClauseType == EClauseType::HAVING)
			m_pSqlTable->m_strHaving.Empty();
		else if (m_eClauseType == EClauseType::JOIN_ON)
			m_pSqlTable->m_strFrom.Empty();
	}		

	if (IsEmpty())
		return TRUE;
	
	//expand ContentOf : NO! l'albero è incompleto, espande direttamente sotto
	//Stack modifiedStack;
	//ExpParse::ExpandStack(m_ExprStack, modifiedStack);

	if (m_eClauseType == EClauseType::WHERE)
	{
		if (bAppend && !m_pSqlTable->m_strFilter.IsEmpty()) 
			m_pSqlTable->m_strFilter += _T(" AND ");
	}
	else if (m_eClauseType == EClauseType::HAVING)
	{
		if (bAppend && !m_pSqlTable->m_strHaving.IsEmpty()) 
			m_pSqlTable->m_strHaving += _T(" AND ");
	} 
	else if (m_eClauseType == EClauseType::JOIN_ON)
	{
		ASSERT(m_nPrepareTableIndex > 0 && m_nPrepareTableIndex < m_arSqlTableInfo.GetSize());

		if (this->m_nPrepareTableIndex == 1 && m_pSqlTable->m_strFrom.IsEmpty())
		{
			m_pSqlTable->m_strFrom += this->m_arSqlTableInfo[0]->GetTableName() + ' ';
		}

		ASSERT(m_arSqlTableInfo.m_arJoinType.GetSize() == m_arSqlTableInfo.GetSize());

		SqlTableJoinInfoArray::EJoinType eJoinType = m_arSqlTableInfo.m_arJoinType[m_nPrepareTableIndex];
		if (eJoinType == SqlTableJoinInfoArray::EJoinType::INNER)
		{
			m_pSqlTable->m_strFrom += L" Inner Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::CROSS)
		{
			m_pSqlTable->m_strFrom += L" Cross Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::LEFT_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Left Outer Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Right Outer Join ";
		}
		else if (eJoinType == SqlTableJoinInfoArray::EJoinType::FULL_OUTER)
		{
			m_pSqlTable->m_strFrom += L" Full Outer Join ";
		}
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}
		m_pSqlTable->m_strFrom += m_arSqlTableInfo[m_nPrepareTableIndex]->GetTableName();
		m_pSqlTable->m_strFrom += L" On ";
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// Al posto di costanti e variabili utente devono essere sostituiti
	// i "?" come vuole la sintassi per il binding dei parametri.
	// Anche le costanti sono bindate in questo modo poiche cosi` facendo non ci
	// si deve porre il problema della convenzione di "quoting" dello specifico DataBase
	// lasciando all'interfaccia tra DataObj e parametro di ODBC il compito di quotare
	// correttamente
	//
    for (int i = 0; i < m_ExprStack.GetSize(); i++)
    {
        ExpItem* pExpItem = (ExpItem*) m_ExprStack[i];
	
        switch(pExpItem->IsA())
        {
        	case EXP_ITEM_VRB_CLASS :	// e` una variabile
			{
				CString& strVrbName = ((ExpItemVrb*) pExpItem)->m_strNameVrb;
				SymField* pField = m_pSymTable ? m_pSymTable->GetField(strVrbName) : NULL;
				DataObj* pDataObj = pField ? pField->GetData() : NULL;					
				if (!pDataObj)
					return ErrHandler(UNKNOWN_FIELD);

				if (m_pParamsArray == NULL)
					m_pParamsArray = new ParamsArray;
												
				// SOSTIUISCE il nome della variabile con una stringa transitoria ritornata
				// dalla AddParamGetItem (cfr. piu` sopra)
				ParamItem* pItem = m_pParamsArray->AddParamGetItem(strVrbName, pDataObj);

				// Vengono "bindati" alla table passata i parametri sulla base dell'espressione
				if (m_eClauseType == EClauseType::WHERE)
				{
					m_pSqlTable->m_strFilter += _T("? ");
				}
				else if (m_eClauseType == EClauseType::HAVING)
				{
					m_pSqlTable->m_strHaving += _T("? ");
				}
				else if (m_eClauseType == EClauseType::JOIN_ON)
				{
					m_pSqlTable->m_strFrom += _T("? ");
				}
				
				AddSqlParam(pItem->m_strParamName, pItem->m_pAssociatedData);
 				break;
       		}
			case EXP_ITEM_FUN_CLASS:
			{
			    if (pExpItem->IsKindOf(RUNTIME_CLASS(ExpItemContentOfFun)))
				{
					ExpItemContentOfFun* pContentOf = (ExpItemContentOfFun*) pExpItem;

					ExpItemContentOfVal* pContentVal = (ExpItemContentOfVal*) pContentOf->Expand();

					ASSERT(pContentVal->m_pVal && pContentVal->m_pVal->IsKindOf(RUNTIME_CLASS(DataStr)));

					CString sContentOfVal = (((DataStr*)pContentVal->m_pVal)->GetString());

					//parsa il risultato e converte in nativo le costanti ed eventuali i valori dei campi del report utilizzati
					CString sConverted;
					if (ConvertToNative(sContentOfVal, sConverted))
						sContentOfVal = sConverted;

					if (m_eClauseType == EClauseType::WHERE)
					{
						m_pSqlTable->m_strFilter += sContentOfVal + BLANK_CHAR;
					}
					else if (m_eClauseType == EClauseType::HAVING)
					{
						m_pSqlTable->m_strHaving +=  sContentOfVal + BLANK_CHAR;
					}
					else if (m_eClauseType == EClauseType::JOIN_ON)
					{
						m_pSqlTable->m_strFrom += sContentOfVal + BLANK_CHAR;
					}
					break;
				}
				ASSERT(FALSE);
				break;
			}
        	case EXP_ITEM_VAL_CLASS :	// e` una costante oppure un pezzo di espressione nativa
        	{
		    	if (pExpItem->IsKindOf(RUNTIME_CLASS(ExpItemValWC)))
				{
					if (m_eClauseType == EClauseType::WHERE)
					{
						m_pSqlTable->m_strFilter += ((DataStr*) ((ExpItemValWC*) pExpItem)->m_pVal)->GetString();
					}
					else if (m_eClauseType == EClauseType::HAVING)
					{
						m_pSqlTable->m_strHaving += ((DataStr*) ((ExpItemValWC*) pExpItem)->m_pVal)->GetString();
					}
					else if (m_eClauseType == EClauseType::JOIN_ON)
					{
						m_pSqlTable->m_strFrom += ((DataStr*)((ExpItemValWC*)pExpItem)->m_pVal)->GetString();
					}
			        break;
				}

				if (m_pParamsArray == NULL)
					m_pParamsArray = new ParamsArray;
		
				DataObj* pDataObj = ((ExpItemVal*) pExpItem)->m_pVal;
		
				// COSTRUISCE il nome della variabile con una stringa transitoria ritornata
				// dalla AddParamGetItem (cfr. piu` sopra)
				ParamItem* pItem = m_pParamsArray->AddParamGetItem(_T("Param"), pDataObj);
	
				// Vengono "bindati" alla table passata i parametri sulla base della
				// espressione
				if (m_eClauseType == EClauseType::WHERE)
				{
					m_pSqlTable->m_strFilter += _T("? ");
				}
				else if (m_eClauseType == EClauseType::HAVING)
				{
					m_pSqlTable->m_strHaving += _T("? ");
				}
				else if (m_eClauseType == EClauseType::JOIN_ON)
				{
					m_pSqlTable->m_strFrom += _T("? ");
				}

				AddSqlParam(pItem->m_strParamName, pItem->m_pAssociatedData);
				break;
			}
		}
	}

	CString& s = m_eClauseType == EClauseType::HAVING ? m_pSqlTable->m_strHaving : 
									(m_eClauseType == EClauseType::JOIN_ON ? m_pSqlTable->m_strFrom :
									 m_pSqlTable->m_strFilter);
		s.Remove('\r');
		s.Replace('\n', ' ');
		s.Replace(_T("=="), _T("="));
		s.Replace(_T("!="), _T("<>"));

	//TRACE(m_pSqlTable->m_strFilter + '\n');
	return TRUE;
}

//-----------------------------------------------------------------------------
CString	WClauseExpr::ToString(SqlTable* pSqlTable/* = NULL*/)
{
	switch (m_EmptyWhere)
	{
		case T_BREAK	: m_strExprString = cwsprintf(T_BREAK);	return m_strExprString;
		case T_ALL		: m_strExprString = cwsprintf(T_ALL);	return m_strExprString;
	}

	SqlTable* pTmpSqlTable = pSqlTable == NULL ? m_pSqlTable : pSqlTable;

	CString strBefore;
	CStringArray_ConcatComment(m_arCommentTraceBefore, strBefore);
	CString strAfter;
	CStringArray_ConcatComment(m_arCommentTraceAfter, strAfter);

	if (!m_strExprString.IsEmpty() || pTmpSqlTable == NULL)
	{
		return strBefore + m_strExprString + strAfter;
	}

	int nParam = 0;
	int nStart = 0;
	int nEnd = 0;
	for (nEnd = 0; nEnd < pTmpSqlTable->m_strFilter.GetLength(); nEnd++)
		if (pTmpSqlTable->m_strFilter[nEnd] == _T('?'))
		{
			m_strExprString += pTmpSqlTable->m_strFilter.Mid(nStart, nEnd - nStart);

			DataType dataType = pTmpSqlTable->GetParamType(nParam);
			DataObj* pDataObj = DataObj::DataObjCreate(dataType);
			pTmpSqlTable->GetParamValue(nParam++, pDataObj);
			
			m_strExprString += ExpUnparse::UnparseData(*pDataObj);

			nStart = nEnd + 1;
			delete pDataObj;
		}

	if (nEnd > nStart)
		m_strExprString += pTmpSqlTable->m_strFilter.Mid(nStart, nEnd - nStart);

	return strBefore + m_strExprString + strAfter;;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::OpenTable(BOOL bAppend /*=FALSE*/)
{
	ASSERT_VALID(m_pSqlTable);
	// se sono nella condizione di append
	// devo aggiungere condizioni di filtraggio ad una query già esistente
	if (bAppend)
	{
		if (!m_pSqlTable->IsOpen())
			return FALSE;
		// se non ho select allora effettuo una selectall()
		if (m_pSqlTable->IsSelectEmpty())
			m_pSqlTable->SelectFromAllTable();
	}
	else
	{
		TRY
		{
			if (!m_pSqlTable->IsOpen())
				m_pSqlTable->Open(FALSE, E_FORWARD_ONLY);
			else
			{
				// La Close svuota anche la stringa di OrderBy
				// quindi bisogna salvarsi la corrente e reimpostarla
				CString strCurrSort = m_pSqlTable->m_strSort;
				m_pSqlTable->ClearQuery();
				m_pSqlTable->m_strSort = strCurrSort;
			}
			m_pSqlTable->SelectFromAllTable();
		}
		CATCH(SqlException, e)	
		{
			AfxMessageBox(e->m_strError);
			return ErrHandler(UNKNOW);
		}
		END_CATCH
	}
	return TRUE;
}

//----------------------------------------------------------------------------------
BOOL WClauseExpr::PrepareQuery(WClauseExpr*& pCurrWClause, BOOL bAppend /*=FALSE*/, int nTableIndex/* = 0*/)
{
	m_nPrepareTableIndex = nTableIndex;

	if (pCurrWClause != this)
	{
		pCurrWClause = this;

		if (!OpenTable(bAppend))
			return FALSE;

		if (m_EmptyWhere == T_NOTOKEN)
		{
			if (m_bNative)
			{
				if (!BindParamsNative(NULL, bAppend))
					return FALSE;
			}
			else if (!BindParams(NULL, bAppend))
				return FALSE;
		}
	}
	
	if (m_EmptyWhere == T_BREAK)
		return FALSE;

	if (m_EmptyWhere == T_ALL)
		return TRUE;

	if (m_pParamsArray == NULL)
		return TRUE;
		
	for (int i = 0; i <= m_pParamsArray->GetUpperBound(); i++)
	{
		ParamItem* pItem = m_pParamsArray->GetAt(i);

		// se uno dei parametri non e` stato valorizzato corettamente
		// dal chiamante si ritorna FALSE per indicare che la WhereClause
		// non e` stata valutata
		//
		if (!pItem->m_pAssociatedData->IsValid())
			return FALSE;

		m_pSqlTable->SetParamValue(pItem->m_strParamName, *(pItem->m_pAssociatedData));
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::IsEmpty()
{
	if (m_EmptyWhere != T_NOTOKEN)
		return FALSE;

	return Expression::IsEmpty();
}

//-----------------------------------------------------------------------------
void WClauseExpr::SetTableInfo(const SqlTableInfo* pSqlTableInfo, SqlConnection* pConnection, SymTable* pSymTable)
{ 
	m_arSqlTableInfo.Add(pConnection, pSymTable, pSqlTableInfo); 
}

//-----------------------------------------------------------------------------
ExpItemVal* WClauseExpr::ResolveSymbol(ExpItemVrb& aExpItemVrb)
{
	if (m_arSqlTableInfo.GetSize() == 0)
	{
		TRACE0("WClauseExpr::ResolveSymbol: not available SqlTableInfo\n");
		ASSERT(FALSE);
		
		return NULL;
	}

	CString strName = aExpItemVrb.m_strNameVrb;

	// prova a vedere se il simbolo e` accettabile per la query
	// (anche senza nome di tabella prefisso)
	BOOL bPhysicalCol = IsPhysicalName(strName, &m_arSqlTableInfo);

	BOOL bPublicField = m_pSymTable && m_pSymTable->ExistField(strName);
	
	BOOL bForbidden = CStringArray_Find(m_arForbiddenPublicIdents, strName) > -1;

	if (bPhysicalCol && bPublicField)
	{
		if (bForbidden)
			bPublicField = FALSE;
		else
			TRACE("WClauseExpr::ResolveSymbol: Ambiguity between column name and variable name %s\n", (LPCTSTR)aExpItemVrb.m_strNameVrb);
	}
	else if (bPublicField && bForbidden)
	{
		ErrHandler(FORBIDDEN_FIELD);
		return NULL;
	}

	ASSERT(m_arSqlTableInfo.m_arstrAliasTableName.GetSize() <= m_arSqlTableInfo.GetSize());
	int nDot = strName.Find(DOT_CHAR);

	if (!bPhysicalCol && !bPublicField && nDot > 0 && m_arSqlTableInfo.m_arstrAliasTableName.GetSize() == m_arSqlTableInfo.GetSize())
	{
		CString strAlias = strName.Left(nDot);
		//possibile colonna qualificata con un alias: 

		int r = 0;
		for (; r < m_arSqlTableInfo.m_arstrAliasTableName.GetSize(); r++)
		{
			if (strAlias.CompareNoCase(m_arSqlTableInfo.m_arstrAliasTableName.GetAt(r)) == 0)
				break;
		}
		if (r < m_arSqlTableInfo.m_arstrAliasTableName.GetSize())
		{
			strName =  (m_arSqlTableInfo.GetAt(r))->GetTableName() + strName.Mid(nDot);
			bPhysicalCol = IsPhysicalName(strName, m_arSqlTableInfo.GetAt(r));
		}	
	}

	if (bPublicField || !bPhysicalCol)
		return Expression::ResolveSymbol(aExpItemVrb);
	
	const SqlColumnInfo* pSqlColumnInfo	= m_arSqlTableInfo.GetColumnInfo(strName);
	
	// Poiche` non si conosce a priori il tipo MicroArea della colonna
	// e` necessario istanziare un particolare ExpItemValWC che derivato
	// da ExpItemVal implementi la capacita` di verificare la compatibilita`
	// degli operandi dell'espressione eseguita dai metodi GiveMeResultTypeFor....
	// (vedi sotto)
	
	// si chiede alla SqlColumnInfo la lista dei tipi compatibili
	CWordArray dataTypes;
	VERIFY(pSqlColumnInfo->GetDataObjTypes(dataTypes));

	// si crea un DataObj di default con il tipo primario (quello nella posizione 0)
	DataObj* pDataObj = DataObj::DataObjCreate(DataType(dataTypes[0]));

	// si ritorna al "compilatore" dell'espressione l'ExpItemValWC richiesto
	// (vedere sotto la spiegazione)
	return new ExpItemValWC(pDataObj, aExpItemVrb.m_nPosInStr, TRUE, TRUE, &dataTypes);
}

// I metodi seguenti GiveMeResultTypeFor....() vengo chiamati virtualmente dalla
// fase di compilazione della classe Expression per implementare la diagniostica 
// relativa alla compatibilta` da operandi in funzione dell'operatore da applicare
//
// Essi si basano sul fatto che, nel caso di operandi corrispondenti a colonne della
// tabella, la WClauseExpr::ResolvSymbol (vedi sopra) ha istanziato dei particolari ExpItemValWC,
// derivati da ExpItemVal, che contengono la lista dei possibili tipi in cui la colonna
// puo` essere fatta corrispondere, e quindi la ricerca della compatibilita` tra i due
// operandi non viene limitata al solo tipo del DataObj associato all'ExpItemVal, ma
// vengono provate tutte le permutazioni tra i tipi a disposizione fino a che non si trova
// la coppia di DataType che soddisfa l'operatore applicato
//
// GiveMeResultTypeForMathOpMap :	si basa sulle mappe inizializzate in expr.cpp per
//									la compatibilita` con operazioni matematiche
//									(+,-,/,*,%,<,>,>=,<=,==,!=)
//-----------------------------------------------------------------------------
DataType WClauseExpr::GiveMeResultTypeForMathOpMap(DATA_TYPE_OP_MAP& map, ExpItemVal* pOpr1, ExpItemVal* pOpr2)
{
	DataType retType;
	
	if (pOpr1->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
	{
		ExpItemValWC& aOpr1WC = *(ExpItemValWC*) pOpr1;
		for (int i = 0; i <= aOpr1WC.m_pCompatibleDataTypes->GetUpperBound(); i++)
		{
			if (pOpr2->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
			{
				ExpItemValWC& aOpr2WC = *(ExpItemValWC*) pOpr2;
				for (int j = 0; j <= aOpr2WC.m_pCompatibleDataTypes->GetUpperBound(); j++)
				{
					retType = map[aOpr1WC.m_pCompatibleDataTypes->GetAt(i)][aOpr2WC.m_pCompatibleDataTypes->GetAt(j)];
			
					if (retType != DATA_NULL_TYPE)
						return retType;
				}
			}
			else
				retType = map[aOpr1WC.m_pCompatibleDataTypes->GetAt(i)][pOpr2->GetDataType().m_wType];
	
			if (retType != DATA_NULL_TYPE)
				return retType;
		}
	
		return DATA_NULL_TYPE;
	}

	if (pOpr2->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
	{
		ExpItemValWC& aOpr2WC = *(ExpItemValWC*) pOpr2;
		for (int i = 0; i <= aOpr2WC.m_pCompatibleDataTypes->GetUpperBound(); i++)
		{
			retType = map[pOpr1->GetDataType().m_wType][aOpr2WC.m_pCompatibleDataTypes->GetAt(i)];
			
			if (retType != DATA_NULL_TYPE)
				return retType;
		}
	}
	else
		retType = map[pOpr1->GetDataType().m_wType][pOpr2->GetDataType().m_wType];

	return retType;
}

// GiveMeResultTypeForBitwiseOp : verifica la compatibilita` con operazioni bitwise
//-----------------------------------------------------------------------------
DataType WClauseExpr::GiveMeResultTypeForBitwiseOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2 /* = NULL */)
{
	DataType nOpr1Type = pOpr1->GetDataType();
	if	(
			nOpr1Type != DATA_INT_TYPE &&
			nOpr1Type != DATA_LNG_TYPE &&
			pOpr1->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC)
		)
	{
		ExpItemValWC& aOprWC = *(ExpItemValWC*) pOpr1;
		for (int i = 0; i <= aOprWC.m_pCompatibleDataTypes->GetUpperBound(); i++)
			if	(
					aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_INT_TYPE ||
					aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_LNG_TYPE
				)
			{
				nOpr1Type = (DataType) aOprWC.m_pCompatibleDataTypes->GetAt(i);
				break;
			}
	}

	if (nOpr1Type != DATA_INT_TYPE && nOpr1Type != DATA_LNG_TYPE)
		return DATA_NULL_TYPE;

	if (pOpr2 == NULL)
		return nOpr1Type;
		
	DataType nOpr2Type = pOpr2->GetDataType();
	if	(
			nOpr2Type != DATA_INT_TYPE &&
			nOpr2Type != DATA_LNG_TYPE &&
			pOpr2->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC)
		)
	{
		ExpItemValWC& aOprWC = *(ExpItemValWC*) pOpr2;
		for (int i = 0; i <= aOprWC.m_pCompatibleDataTypes->GetUpperBound(); i++)
			if	(
					aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_INT_TYPE ||
					aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_LNG_TYPE
				)
			{
				nOpr2Type = (DataType) aOprWC.m_pCompatibleDataTypes->GetAt(i);
				break;
			}
	}

	if (nOpr2Type != DATA_INT_TYPE && nOpr2Type != DATA_LNG_TYPE)
		return DATA_NULL_TYPE;
		
	if (nOpr1Type != nOpr2Type)
		return DATA_NULL_TYPE;

	return nOpr1Type;
}

// GiveMeResultTypeForLogicalOp :	verifica la compatibilita` con operazioni logiche
//									(AND, OR, NOT)
//-----------------------------------------------------------------------------
DataType WClauseExpr::GiveMeResultTypeForLogicalOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2 /* = NULL */)
{
	DataType nOprType = pOpr1->GetDataType();
	if (nOprType != DATA_BOOL_TYPE && pOpr1->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
	{
		ExpItemValWC& aOprWC = *(ExpItemValWC*) pOpr1;
		for (int i = 0; i <= aOprWC.m_pCompatibleDataTypes->GetUpperBound(); i++)
			if (aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_BOOL_TYPE)
			{
				nOprType = (DataType) aOprWC.m_pCompatibleDataTypes->GetAt(i);
				break;
			}
	}

	if (nOprType != DATA_BOOL_TYPE)
		return DATA_NULL_TYPE;

	if (pOpr2 == NULL)
		return DATA_BOOL_TYPE;
		
	nOprType = pOpr2->GetDataType();
	if (nOprType != DATA_BOOL_TYPE && pOpr2->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
	{
		ExpItemValWC& aOprWC = *(ExpItemValWC*) pOpr2;
		for (int i = 0; i <= aOprWC.m_pCompatibleDataTypes->GetUpperBound(); i++)
			if (aOprWC.m_pCompatibleDataTypes->GetAt(i) == DATA_BOOL_TYPE)
			{
				nOprType = (DataType) aOprWC.m_pCompatibleDataTypes->GetAt(i);
				break;
			}
	}

	if (nOprType != DATA_BOOL_TYPE)
		return DATA_NULL_TYPE;

	return DATA_BOOL_TYPE;
}

//-----------------------------------------------------------------------------
DataType WClauseExpr::GiveMeResultTypeForLikeOp(ExpItemVal* pOpr1, ExpItemVal* pOpr2)
{
	// non viene accettato come secondo operando un colonna di tabella
	if (pOpr2->GetRuntimeClass() == RUNTIME_CLASS(ExpItemValWC))
		return DATA_NULL_TYPE;

	return Expression::GiveMeResultTypeForLikeOp(pOpr1, pOpr2);
}

//-----------------------------------------------------------------------------
ExpItemVal*	WClauseExpr::OnApplyFunction (ExpItemFun* itemFun, Stack& paramStack)
{
	if (itemFun->m_nFun != T_FCONTENTOF)
		return NULL;

	ExpItem* p = (ExpItem*) paramStack.Pop();
	if (p->IsKindOf(RUNTIME_CLASS(ExpItemContentOfFun)))
	{
		ExpItemContentOfFun* p1 = (ExpItemContentOfFun*) p;

		return (ExpItemVal*) p1->Expand();
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//		Class WClause implementation
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(WClause, WClauseExpr)

//-----------------------------------------------------------------------------
WClause::WClause
	(
		SqlConnection*		pConnection,
		SymTable*			pSymTable,
		const SqlTableJoinInfoArray*	parSqlTableInfo,
		SqlTable*			pSqlTable	/* = NULL */
	)
	:
	WClauseExpr		(pConnection, pSymTable, parSqlTableInfo, pSqlTable),
	m_pCondExpr		(NULL),
	m_pThenWClause	(NULL),
	m_pElseWClause	(NULL),
	m_pCurrWClause	(NULL)
{
}

//-----------------------------------------------------------------------------
WClause::WClause
	(
		SqlConnection*		pConnection,
		SymTable*			pSymTable,
		const SqlTableInfoArray&	arSqlTableInfo,
		SqlTable*			pSqlTable	/* = NULL */,
		CStringArray*		parstrTableAlias /*= NULL*/
	)
:
	WClauseExpr(pConnection, pSymTable, &SqlTableJoinInfoArray(arSqlTableInfo), pSqlTable),
	m_pCondExpr(NULL),
	m_pThenWClause(NULL),
	m_pElseWClause(NULL),
	m_pCurrWClause(NULL)
{
	ASSERT(!arSqlTableInfo.IsKindOf(RUNTIME_CLASS(SqlTableJoinInfoArray)));

	if (parstrTableAlias)
	{
		ASSERT(m_arSqlTableInfo.GetSize() == parstrTableAlias->GetSize());
		m_arSqlTableInfo.m_arstrAliasTableName.Copy(*parstrTableAlias);
	}
}

//-----------------------------------------------------------------------------
WClause::WClause(const WClause& aWClause)
	:
	WClauseExpr		(NULL, NULL),
	m_pCondExpr		(NULL),
	m_pThenWClause	(NULL),
	m_pElseWClause	(NULL),
	m_pCurrWClause	(NULL)
{
	*this = aWClause;
}

//-----------------------------------------------------------------------------
WClause::~WClause()
{
	Dispose();
}

//-----------------------------------------------------------------------------
void WClause::Dispose()
{
	m_pCurrWClause = NULL;

	SAFE_DELETE (m_pCondExpr)
	SAFE_DELETE (m_pThenWClause)	
	SAFE_DELETE (m_pElseWClause)
}

//-----------------------------------------------------------------------------
WClauseExpr* WClause::Clone	()
{
	return new WClause(*this);
}

//-----------------------------------------------------------------------------
void WClause::Reset(BOOL bResetAll/* = TRUE*/)
{
	WClauseExpr::Reset(bResetAll);

	m_arCommentTraceBefore.RemoveAll();
	m_arCommentTraceAfter.RemoveAll();

	Dispose();
}

//-----------------------------------------------------------------------------
void WClause::operator = (const WClause& aWClause)
{
	WClauseExpr::operator=(aWClause);

	Dispose();

	if (aWClause.m_pCondExpr)
	{
		m_pCondExpr = new Expression(m_pSymTable);
		m_pCondExpr->Assign(*aWClause.m_pCondExpr);
		
		ASSERT(aWClause.m_pThenWClause);
		m_pThenWClause = aWClause.m_pThenWClause->Clone();
		
		if (aWClause.m_pElseWClause)
			m_pElseWClause = aWClause.m_pElseWClause->Clone();
	}
}

//-----------------------------------------------------------------------------
BOOL WClause::Parse(Parser& lex)
{
	Dispose();

	m_arCommentTraceBefore.RemoveAll();
	m_arCommentTraceAfter.RemoveAll();

	if (lex.LookAhead(T_IF))
		return ParseCondWhere(lex);
	
	return WClauseExpr::Parse(lex);
}

//-----------------------------------------------------------------------------
BOOL WClause::ParseCondWhere(Parser& lex)
{
	if (!lex.ParseTag(T_IF))	
		return FALSE;
	
	m_pCondExpr = new Expression(m_pSymTable);
	
	m_pCondExpr->SetStopTokens(T_THEN);
	if (!m_pCondExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))	
		return FALSE;
	
	if (!lex.ParseTag(T_THEN)) 
		return FALSE;
	
	if (lex.LookAhead(T_IF))
	{	
		m_pThenWClause = new WClause(m_pSqlConnection, m_pSymTable, &m_arSqlTableInfo, m_pSqlTable);
		m_pThenWClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);
		if (!((WClause*) m_pThenWClause)->ParseCondWhere(lex)) 
			return FALSE;
	}
	else
	{
		m_pThenWClause = new WClauseExpr(m_pSqlConnection, m_pSymTable, &m_arSqlTableInfo, m_pSqlTable);
		m_pThenWClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);
		if (!m_pThenWClause->Parse(lex)) 
			return FALSE;
	}
	
	if (!lex.Matched(T_ELSE)) 
		return TRUE;
	
	if (lex.LookAhead(T_IF))
	{
		m_pElseWClause = new WClause(m_pSqlConnection, m_pSymTable, &m_arSqlTableInfo, m_pSqlTable);
		m_pElseWClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);
		return ((WClause*) m_pElseWClause)->ParseCondWhere(lex);
	}

	m_pElseWClause = new WClauseExpr(m_pSqlConnection, m_pSymTable, &m_arSqlTableInfo, m_pSqlTable);
	m_pElseWClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);

	return m_pElseWClause->Parse(lex);
}

//-----------------------------------------------------------------------------
CString WClause::ToString(SqlTable* pSqlTable /* = NULL */)
{
	if (m_pCondExpr)
	{
		CString strBefore;
		CStringArray_ConcatComment(m_arCommentTraceBefore, strBefore);
		CString strAfter;
		CStringArray_ConcatComment(m_arCommentTraceAfter, strAfter);

		m_strExprString = cwsprintf(T_IF);
		m_strExprString += BLANK_CHAR;
		m_strExprString += m_pCondExpr->ToString();
		m_strExprString += LF_CHAR;
		m_strExprString += cwsprintf(T_THEN);
		m_strExprString += BLANK_CHAR;

 		m_strExprString += m_pThenWClause->ToString(pSqlTable);
		
		if (m_pElseWClause)
		{
			m_strExprString += LF_CHAR;
			m_strExprString += cwsprintf(T_ELSE);
			m_strExprString += BLANK_CHAR;

			m_strExprString += m_pElseWClause->ToString(pSqlTable);
		}
		
		return strBefore + m_strExprString + strAfter;
	}
	
	return __super::ToString(pSqlTable);
}

//-----------------------------------------------------------------------------
BOOL WClause::PrepareQuery(BOOL bAppend /*=FALSE*/, int nTableIndex/* = 0*/)
{
	return PrepareQuery(m_pCurrWClause, bAppend, nTableIndex);
}

//-----------------------------------------------------------------------------
BOOL WClause::PrepareQuery(WClauseExpr*& pCurrWClause, BOOL bAppend /*=FALSE*/, int nTableIndex /*= 0*/)
{
	if (!m_pCondExpr)
		return WClauseExpr::PrepareQuery(pCurrWClause, bAppend, nTableIndex);

	DataBool cond;
	if (!m_pCondExpr->Eval(cond) || !cond.IsValid())
		return ErrHandler(m_pCondExpr->GetErrId());
		
	if ((BOOL) cond)
	{
		if (!m_pThenWClause->PrepareQuery(pCurrWClause, bAppend, nTableIndex))
			return ErrHandler(m_pThenWClause->GetErrId());
			
		return TRUE;
	}
			
	if (m_pElseWClause)
	{
		if (!m_pElseWClause->PrepareQuery(pCurrWClause, bAppend, nTableIndex))
			return ErrHandler(m_pElseWClause->GetErrId());
			
		return TRUE;
	}
			
	if (pCurrWClause != this)
	{
		pCurrWClause = this;

		if (!OpenTable(bAppend))
			return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL WClauseExpr::HasMember(LPCTSTR pszVarName) const
{
	//TODO WClause::HasMember non verifica clausola ContentOf
	BOOL bHas = __super::HasMember(pszVarName);
	return bHas;
}

BOOL WClause::HasMember	(LPCTSTR pszStr) const
{
	if (!m_pCondExpr)
		return __super::HasMember(pszStr);

	return	m_pCondExpr->HasMember(pszStr)		||
			m_pThenWClause->HasMember(pszStr)	||
			(m_pElseWClause && m_pElseWClause->HasMember(pszStr));
}

//-----------------------------------------------------------------------------
void WClauseExpr::GetParameters(CStringArray& ar, BOOL bReverse/* = TRUE*/) const
{
	return __super::GetParameters(ar, bReverse);
}

//-----------------------------------------------------------------------------
void  WClause::GetParameters(CStringArray& ar, BOOL bReverse /*= TRUE*/) const
{
	if (!m_pCondExpr)
	{
		__super::GetParameters(ar, bReverse);
		return;
	}

	m_pCondExpr->GetParameters(ar, bReverse);
	m_pThenWClause->GetParameters(ar, bReverse);

	if (m_pElseWClause)
		m_pElseWClause->GetParameters(ar, bReverse);
}

//-----------------------------------------------------------------------------
BOOL WClause::IsEmpty()
{
	if (!m_pCondExpr)
		return WClauseExpr::IsEmpty();

	return	m_pCondExpr->IsEmpty()		&&
			m_pThenWClause->IsEmpty()	&&
			(!m_pElseWClause || m_pElseWClause->IsEmpty());
}

//-----------------------------------------------------------------------------
void WClause::SetTableInfo(const SqlTableInfo* pSqlTableInfo)
{
	WClauseExpr::SetTableInfo(pSqlTableInfo, m_pSqlConnection, GetSymTable());

	if (m_pCondExpr)
	{
		m_pThenWClause->SetTableInfo(pSqlTableInfo, m_pSqlConnection, GetSymTable());
			
		if (m_pElseWClause)
			m_pElseWClause->SetTableInfo(pSqlTableInfo, m_pSqlConnection, GetSymTable());
	}
}
//-----------------------------------------------------------------------------
void WClause::SetTableInfo(const SqlTableInfoArray* parSqlTableInfo)
{
	WClauseExpr::SetTableInfo(parSqlTableInfo);

	if (m_pCondExpr)
	{
		m_pThenWClause->SetTableInfo(parSqlTableInfo);
			
		if (m_pElseWClause)
			m_pElseWClause->SetTableInfo(parSqlTableInfo);
	}
}

//-----------------------------------------------------------------------------
void WClause::SetTableInfo(const SqlTableJoinInfoArray* parSqlTableInfo)
{
	WClauseExpr::SetTableInfo(parSqlTableInfo);

	if (m_pCondExpr)
	{
		m_pThenWClause->SetTableInfo(parSqlTableInfo);

		if (m_pElseWClause)
			m_pElseWClause->SetTableInfo(parSqlTableInfo);
	}
}

//-----------------------------------------------------------------------------
void WClause::SetNative	(BOOL bNative /*= TRUE */)
{
	if (m_pCondExpr) return;

	WClauseExpr::SetNative(bNative);
}
