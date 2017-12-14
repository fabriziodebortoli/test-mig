
#include "stdafx.h"

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

#include <TBParser\symtable.h>
#include <TBParser\parser.h>

#include <TbGenlib\expr.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqlrec.h>	
#include <TbOledb\sqlaccessor.h>	
#include <TbOledb\oledbmng.h>

//#define REPORT_ENGINE	2

#include "QueryObject.h"
#include "Report.h"

#include <TbWoormViewer\WoormDoc.hjson> //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//							TagLink
///////////////////////////////////////////////////////////////////////////////
//
TagLink::TagLink(LPCTSTR pszName, QueryObject::Direction direction, DataObj* pData, int nLen, Expression* pWhenExpr, QueryObject* pExpandClause)
:
    m_strName		(pszName),
	m_Direction		(direction),
	m_pData			(NULL),
	m_nLen			(nLen),
	m_pWhenExpr		(pWhenExpr),
	m_pExpandClause	(pExpandClause),
	m_pElseClause	(NULL),
	m_bWhen			(FALSE)
{
	if (pData)
		m_pData = pData->DataObjClone();
}

TagLink::~TagLink()
{
	SAFE_DELETE(m_pWhenExpr);
	SAFE_DELETE(m_pExpandClause);
	SAFE_DELETE(m_pElseClause);
	SAFE_DELETE(m_pData);
}

/////////////////////////////////////////////////////////////////////////////
//							QueryObject
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(QueryObject, QueryObjectBase)

//------------------------------------------------------------------------------
QueryObject::QueryObject (const CString& sName, SymTable* pSymbolTable, SqlSession* pSession, QueryObject* pParent)
	: 
	m_strQueryName		(sName),
	m_pSymbolTable		(pSymbolTable),
	m_nQueryHandle		(-1),
	m_poSqlTable		(NULL),
	m_poSqlRecord		(NULL),
	m_pSqlSession		(pSession),
	m_pParent			(pParent),
	m_bUseCursor		(FALSE),
	m_bCursorUpdatable	(FALSE),
	m_bSensibility		(TRUE),
	m_bIsQueryRule		(FALSE)
{
}

///<summary>
///Query Object costructor
///</summary>
//[TBWebMethod(name = Query, woorm_method=false)]
QueryObject::QueryObject (SymTable* pSymbolTable, SqlSession* pSession)
	: 
	m_pSymbolTable		(pSymbolTable),
	m_nQueryHandle		(-1),
	m_poSqlTable		(NULL),
	m_poSqlRecord		(NULL),
	m_pSqlSession		(pSession),
	m_pParent			(NULL),
	m_bUseCursor		(FALSE),
	m_bCursorUpdatable	(FALSE),
	m_bSensibility		(TRUE),
	m_bIsQueryRule		(FALSE)
{
}

//------------------------------------------------------------------------------
QueryObject::~QueryObject ()
{
	Clear();
}

//------------------------------------------------------------------------------
int QueryObject::AddAllColumn (LPCTSTR pszName) 
{ 
	CStringArray* pAr = m_pParent ? &m_pParent->m_arAllSelectedField : &m_arAllSelectedField;
	int idx = CStringArray_Find(*pAr, pszName);
	if (idx > -1) 
		return idx;

	return pAr->Add(pszName);
}

int QueryObject::AddAllParameters (LPCTSTR pszName)
{ 
	CStringArray* pAr = m_pParent ? &m_pParent->m_arAllParameters : &m_arAllParameters;
	int idx = CStringArray_Find(*pAr, pszName);
	if (idx > -1)
		return idx;

	return pAr->Add(pszName);
}

int QueryObject::AddCurrentColumn(LPCTSTR pszName)
{
	CStringArray* pAr = m_pParent ? &m_pParent->m_arSelFieldName : &m_arSelFieldName;

	return pAr->Add(pszName);
}

int QueryObject::AddCurrentParameter(LPCTSTR pszName)
{
	CStringArray* pAr = m_pParent ? &m_pParent->m_arParametersName : &m_arParametersName;

	return pAr->Add(pszName);
}
//------------------------------------------------------------------------------
void QueryObject::Clear ()
{
	if (m_poSqlTable)
	{
		if (m_poSqlTable->IsOpen())
			m_poSqlTable->Close();
		SAFE_DELETE(m_poSqlTable);
	}

	SAFE_DELETE(m_poSqlRecord);

	m_msg.ClearMessages(TRUE);
}

//------------------------------------------------------------------------------
int QueryObject::AddLink (LPCTSTR pszName, Direction direction, DataObj* pData, int nLen, Expression* pWhenExpr, QueryObject* pExpandClause)
{
	if (pData && pData->GetDataType() == DataType::String)
		nLen = 2048;

	if (direction == QueryObject::_COL || direction == QueryObject::_OUT  || direction == QueryObject::_INOUT)
		AddAllColumn(pszName);

	if (direction == QueryObject::_IN  || direction == QueryObject::_INOUT)
		AddAllParameters(pszName);
	
	return m_TagLinks.Add(new TagLink(pszName, direction, pData, nLen, pWhenExpr, pExpandClause));
}

//----------------------------------------------------------------------------
void QueryObject::DeleteField(LPCTSTR pszName)
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_strName.CompareNoCase(pszName) == 0 /*&& (tl->m_Direction == QueryObject::_IN || tl->m_Direction == QueryObject::COL)*/)
		{
			//TODO
		}

		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause)
				tl->m_pExpandClause->RenameField(pszName, pszName);
			if (tl->m_pElseClause)
				tl->m_pElseClause->RenameField(pszName, pszName);
		}
	}

	for (i = this->m_arAllSelectedField.GetSize(); i >= 0; i--)
	{
		if (this->m_arAllSelectedField[i].CompareNoCase(pszName) == 0)
			this->m_arAllSelectedField.RemoveAt(i);
	}
	for (i = this->m_arAllSelectedField.GetSize(); i >= 0 ; i--)
	{
		if (this->m_arAllParameters[i].CompareNoCase(pszName) == 0)
			this->m_arAllParameters.RemoveAt(i);
	}
}

//----------------------------------------------------------------------------
void QueryObject::RenameField(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_strName.CompareNoCase(pszOldName) == 0 /*&& (tl->m_Direction == QueryObject::_IN || tl->m_Direction == QueryObject::COL)*/)
			tl->m_strName = pszNewName;

		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause)
				tl->m_pExpandClause->RenameField(pszOldName, pszNewName);
			if (tl->m_pElseClause)
				tl->m_pElseClause->RenameField(pszOldName, pszNewName);
		}
	}

	ASSERT(CStringArray_Find(m_arAllSelectedField, pszNewName) == -1);
	for (i = 0; i < this->m_arAllSelectedField.GetSize(); i++)
	{
		if (this->m_arAllSelectedField[i].CompareNoCase(pszOldName) == 0)
			this->m_arAllSelectedField[i] = pszNewName;
	}
	ASSERT(CStringArray_Find(m_arAllParameters, pszNewName) == -1);
	for (i = 0; i < this->m_arAllParameters.GetSize(); i++)
	{
		if (this->m_arAllParameters[i].CompareNoCase(pszOldName) == 0)
			this->m_arAllParameters[i] = pszNewName;
	}
}

//------------------------------------------------------------------------------
BOOL QueryObject::HasMember (LPCTSTR pszName) const
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_strName.CompareNoCase(pszName) == 0 && (tl->m_Direction == QueryObject::_IN || tl->m_Direction == QueryObject::_INOUT))
			return TRUE;

		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE|| tl->m_Direction == QueryObject::_EVAL)
		{
			if (tl->m_pWhenExpr && tl->m_pWhenExpr->HasMember(pszName))
				return TRUE;
		}
		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause && tl->m_pExpandClause->HasMember (pszName))
				return TRUE;
			if (tl->m_pElseClause && tl->m_pElseClause->HasMember (pszName))
				return TRUE;
		}
	}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::HasColumn(LPCTSTR pszName) const
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_strName.CompareNoCase(pszName) == 0 && (tl->m_Direction == QueryObject::_COL))
			return TRUE;

		if (tl->m_Direction == QueryObject::_EXPAND)
		{
			if (tl->m_pExpandClause && tl->m_pExpandClause->HasColumn(pszName))
				return TRUE;
			if (tl->m_pElseClause && tl->m_pElseClause->HasColumn(pszName))
				return TRUE;
		}
		if (tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause && tl->m_pExpandClause->HasColumn(pszName))
				return TRUE;
			if (tl->m_pElseClause && tl->m_pElseClause->HasColumn(pszName))
				return TRUE;
		}
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void QueryObject::SetCurrentQueryParameters()
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_Direction == QueryObject::_IN || tl->m_Direction == QueryObject::_INOUT)
		{
			this->AddCurrentParameter(tl->m_strName);
			continue;
		}

		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause && tl->m_bWhen)
				tl->m_pExpandClause->SetCurrentQueryParameters();
			if (tl->m_pElseClause && !tl->m_bWhen)
				tl->m_pElseClause->SetCurrentQueryParameters();
		}
	}
}

//------------------------------------------------------------------------------
void QueryObject::SetCurrentQueryColumns()
{
	int i = 0;
	for (; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* tl = (TagLink*)(m_TagLinks.GetAt(i));

		if (tl->m_Direction == QueryObject::_COL)
		{
			this->AddCurrentColumn(tl->m_strName);
			continue;
		}

		if (tl->m_Direction == QueryObject::_EXPAND || tl->m_Direction == QueryObject::_INCLUDE)
		{
			if (tl->m_pExpandClause && tl->m_bWhen)
				tl->m_pExpandClause->SetCurrentQueryColumns();
			if (tl->m_pElseClause && !tl->m_bWhen)
				tl->m_pElseClause->SetCurrentQueryColumns();
		}
	}
}

//------------------------------------------------------------------------------
///<summary>
///Query Object costructor
///</summary>
//[TBWebMethod(name = Query_Define, woorm_method=false)]
DataBool QueryObject::Define (DataStr dsSql)
{
	CString sSql = dsSql.GetString();
	sSql = _T("QUERY _q") + cwsprintf(_T("%d"), this) + _T(" BEGIN { ") + sSql + _T(" } END");
	Parser lex(sSql);
	BOOL bOk = Parse(lex);
	return DataBool(bOk);
}

//------------------------------------------------------------------------------
///<summary>
///Return Column Name
///</summary>
//[TBWebMethod(name = Query_GetColumnName, woorm_method=false)]
DataStr QueryObject::GetColumnName	(DataInt index)
{
	//TODO
	ASSERT(FALSE);
	return _TB("<not implemented>");
}

//------------------------------------------------------------------------------
///<summary>
///Return Column Data
///</summary>
//[TBWebMethod(name = Query_GetData, woorm_method=false)]
DataObj* QueryObject::GetData (DataStr name)
{
	TagLink* pTag = GetColumn(name);

	return pTag ? pTag->m_pData : NULL;
}

//------------------------------------------------------------------------------
///<summary>
///Return Column Data in portable string format
///</summary>
//[TBWebMethod(name = Query_GetValue, woorm_method=false)]
DataStr QueryObject::GetValue (DataStr sName)
{
	DataObj* pObj = GetData (sName);
	return pObj ? pObj->FormatDataForXML() : DataStr(_T(""));
}

//------------------------------------------------------------------------------
BOOL QueryObject::Parse (Parser& parser)
{
	m_TagLinks.RemoveAll();

	m_strQueryTemplate.Empty();
	m_strSql.Empty();

	m_arAllSelectedField.RemoveAll();
	m_arAllParameters.RemoveAll();
	m_arExternalParameters.RemoveAll();

	m_arSelFieldName.RemoveAll();
	m_arParametersName.RemoveAll();
	//----

	if (parser.Matched(T_QUERY)) 
	{
		if (!parser.ParseID(m_strQueryName)) 
		{
			SetError(_TB("Expected query name"));
			return FALSE;
		}
	}

	BOOL bHasBegin = parser.Matched(T_BEGIN);

	if (!parser.Match(T_BRACEOPEN)) 
	{
		SetError(_TB("Missing BRACEOPEN token"));
		return FALSE;
	}

	CString sPrecAuditString;
	BOOL bPrecAuditingState = parser.IsAuditStringOn();
	if (bPrecAuditingState) 
		sPrecAuditString = parser.GetAuditString();
	else
		parser.EnableAuditString();

	GetSymTable()->TraceFieldsUsed(&m_arExternalParameters);

	if (!ParseInternal(parser))
		return SetError(_TB("It fails parsing query"), parser.GetError());

	GetSymTable()->TraceFieldsUsed(NULL);

	if (bPrecAuditingState)
	{
		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Missing BRACECLOSE token"));
			return FALSE;
		}
		parser.ConcatAuditString(sPrecAuditString + m_strQueryTemplate + parser.GetAuditString());
	}
	else
	{
		parser.EnableAuditString(FALSE);
		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Missing BRACECLOSE token"));
			return FALSE;
		}
	}

	if (bHasBegin && !parser.Match(T_END)) 
	{
		SetError(_TB("Missing END token"));
		return FALSE;
	}

	parser.ClearError();parser.ClearErrorNumber();
	ASSERT(!parser.ErrorFound());
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ParseInternal (Parser& parser)
{
	CString s;
	while (!parser.LookAhead(T_BRACECLOSE) && !parser.LookAhead(T_EOF)) 
	{
		if (parser.LookAhead(T_BRACEOPEN))
		{
			s = parser.GetAuditString();
			m_strQueryTemplate += s;
			m_strSql += _T(" ") + s; //aggiunto spazio per staccare lista colonne dalla from 

			if (!ParseTag(parser))
				return SetError(parser.GetError(), m_strQueryTemplate + _T("\n") + m_strSql); ;

			m_strQueryTemplate += parser.GetAuditString();

			continue;
		}
		parser.SkipToken();
	} 
	s = parser.GetAuditString();
	m_strQueryTemplate += s;
	m_strSql += s;
	
	TRACE(_T("\nQuery Name: %s:\nQuery: %s\nSql: %s\n"), m_strQueryName, m_strQueryTemplate, m_strSql);
	return TRUE;
}

//------------------------------------------------------------------------------
QueryObject* QueryObject::ParseSubQuery (Parser& parser)
{
	QueryObject* pQ = new QueryObject(_T(""), m_pSymbolTable, m_pSqlSession, m_pParent ? m_pParent : this);

	if (!parser.Match(T_BRACEOPEN)) //apertura query di EXPAND
	{
		SetError(_TB("Expected BRACEOPEN token"));
		return NULL;
	}
	
	m_strQueryTemplate += parser.GetAuditString();

	if (!pQ->ParseInternal(parser)) 
		return NULL;

	m_strQueryTemplate += pQ->GetQueryTemplate();

	if (!parser.Match(T_BRACECLOSE)) //chiusura query di EXPAND
	{
		SetError(_TB("Expected BRACECLOSE token"));
		return NULL;
	}
	return pQ;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ParseTag (Parser& parser)
{
	if (!parser.Match(T_BRACEOPEN)) 
	{
		SetError(_TB("Missing BRACEOPEN token"));
		return FALSE;
	}

	CString strName;
	DataObj* pObj = NULL;

	Token t = parser.LookAhead();
	if (t == T_COLUMNS)
	{
		parser.SkipToken(); 
		if (parser.LookAhead() != T_ID) 
		{
			SetError(_TB("Expected field name"));
			return FALSE;
		}
		if (!parser.ParseID(strName)) 
		{
			SetError(_TB("Fail reading field name"));
			return FALSE;
		}
		SqlRecord* pRec = AfxCreateRecord(strName);
		if (!pRec)
		{
			SetError(_TB("Unknown table name") + cwsprintf(_T(" (%s)"), strName));
			return FALSE;
		}
		for (int i = 0; i < pRec->GetSize(); i++)
		{
			pObj = pRec->GetDataObjAt(i);
			ASSERT_VALID(pObj);
			SqlRecordItem* pRecItem = pRec->GetAt(i);
			ASSERT_VALID(pRecItem);

			AddLink(strName + '.' + pRecItem->GetColumnName(), QueryObject::_COL, pObj, pRecItem->GetColumnLength());
			
			if (i > 0)
				m_strSql += ',';
			m_strSql += ' ';
			m_strSql += strName + '.' + pRecItem->GetColumnName();

		}
		//m_strSql += ' ' + strName + _T(".* ");
		m_RecordBag.Add(pRec);

		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}
		return TRUE;
	}
	else if (t == T_COL || t == T_IN || t == T_OUT || t == T_REF)
	{
		parser.SkipToken(); 
		if (parser.LookAhead() != T_ID) 
		{
			SetError(_TB("Expected field name"));
			return FALSE;
		}
		if (!parser.ParseID(strName)) 
		{
			SetError(_TB("Fail reading field name"));
			return FALSE;
		}

		if (t == T_COL)
		{
			if (!m_pSymbolTable->ExistField(strName))
			{
				int idx = strName.Find(L'.');
				if (idx > -1)
				{
					CString sTable = strName.Left(idx);
					SqlRecord* pRec =  m_RecordBag.FindRecordByTableName(sTable);
					if (!pRec)
					{
						pRec = AfxCreateRecord(sTable);
						if (pRec) m_RecordBag.Add(pRec);
					}
					if (!pRec)
					{
						DataType dt = DataType::String;

						if (parser.Matched(T_TYPE))
						{
							if (!parser.ParseDataType(dt))
							{
								SetError(_TB("Fail reading field type") + cwsprintf(_T(" (%s)"), strName));
								return FALSE;
							}
						}
							SymField* pField = new SymField(strName, dt, 0);
							m_pSymbolTable->Add(pField);
						//else
						//{
						//	SetError(_TB("Unknown field name") + cwsprintf(_T(" (%s)"), strName));
						//	return FALSE;
						//}
					}
					else
					{
						int nCol = pRec->GetIndexFromColumnName(strName.Mid(idx + 1));
						if (nCol < 0)
						{
							SetError(_TB("Unknown field name") + cwsprintf(_T(" (%s)"), strName));
							return FALSE;
						}
						DataObj* pColData = pRec->GetDataObjAt(nCol);

						SymField* pField = new SymField(
														strName,
														pColData->GetDataType(),
														0,
														pColData,
														FALSE
												);
						m_pSymbolTable->Add(pField);
					}
				}
			}

			SymField* pField = m_pSymbolTable->GetField(strName);
			if (pField == NULL)
			{
				SetError(_TB("Unknown field name") + cwsprintf(_T(" (%s)"), strName));
				return FALSE;
			}
			pObj = pField->GetData();
			if (pObj == NULL)
			{
				SetError(_TB("field name with bad value") + cwsprintf(_T(" (%s)"), strName));
				return FALSE;
			}

			DataType dt = DataType::String;
			if (parser.Matched(T_TYPE))
			{
				if (!parser.ParseDataType(dt))
				{
					SetError(_TB("Fail reading field type") + cwsprintf(_T(" (%s)"), strName));
					return FALSE;
				}

				if (!DataType::IsCompatible(dt, pField->GetDataType()))
				{
					SetError(cwsprintf(_T("Incompatible datatype %s for field %s (dataTye: %s)"), dt.ToString() , strName, pField->GetDataType().ToString()));
					return FALSE;
				}
			}

			{
				CString sTitle;
				if (parser.Matched(T_TITLE))
				{
					if (!parser.ParseString(sTitle))
					{
						SetError(_TB("Fail reading field title") + cwsprintf(_T(" (%s)"), strName));
						return FALSE;
					}
				}

				int idx = AddLink(strName, QueryObject::_COL, pObj, pField->GetLen());
				if (idx > -1 && !sTitle.IsEmpty())
				{
					TagLink* pTag = dynamic_cast<TagLink*>(m_TagLinks[idx]);
					if (pTag) pTag->m_strTitle = sTitle;
				}
			}

		}
		else //if (t == T_IN || t == T_OUT || t == T_REF)
		{
			if (!m_pSymbolTable->ExistField(strName)) 
			{
				SetError(_TB("Unknown field name") + cwsprintf(_T(" (%s)"), strName));
				return FALSE;
			}

			SymField* pField = m_pSymbolTable->GetField(strName);
			if (pField == NULL) 
			{
				SetError(_TB("field name with bad value") + cwsprintf(_T(" (%s)"), strName));
				return FALSE;
			}
			pObj = pField->GetData();
			if (pObj == NULL) 
			{
				SetError(_TB("field name with bad value") + cwsprintf(_T(" (%s)"), strName));
				return FALSE;
			}

			QueryObject::Direction dir = QueryObject::Direction::_IN;
			switch (t)
			{
				//case T_IN:
				//	break;
				case T_OUT:
					dir = QueryObject::_OUT;
					break;
				case T_REF:
					dir = QueryObject::_INOUT;
					break;
			}
			AddLink(strName, dir, pObj, pField->GetLen());
			m_strSql += _T(" ? ");
		}

		if (parser.Matched(T_AS)) 
			parser.SkipToken();

		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}
		return TRUE;
	}
	else if (t == T_PLUS)
	{
		parser.SkipToken(); 
		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}

		m_strSql += AfxGetLoginInfos()->m_strDatabaseType.Find (_T("ORACLE")) < 0 ? _T(" + ") : _T(" || ");
		
		return TRUE;
	}
	else if (t == T_FALSE || t == T_TRUE)
	{
		parser.SkipToken(); 

		DataBool db(t == T_TRUE);

		CString s;
		if (m_pSqlSession && m_pSqlSession->GetSqlConnection())
			s = m_pSqlSession->GetSqlConnection()->NativeConvert(&db);
		else
			s =  _T("\'0\'");

		BOOL bOracle = AfxGetLoginInfos()->m_strDatabaseType.Find (_T("ORACLE")) >= 0;
		if (bOracle)
		{
			s = cwsprintf(_T("CAST(%s AS %s)"),
				(LPCTSTR)s,
				AfxGetLoginInfos()->m_bUseUnicode ?	_T("NCHAR") : _T("CHAR")
				);
		}

		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}

		m_strSql +=  ' ' + s + ' ';
		return TRUE;
	}
	else if (t == T_STR)
	{
		CString s;
		if (!parser.ParseString(s)) 
		{
			SetError(_TB("Expected string"));
			return FALSE;
		}
		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}

		int len = s.GetLength();

		BOOL bOracle = AfxGetLoginInfos()->m_strDatabaseType.Find (_T("ORACLE")) >= 0;

		if (m_pSqlSession && m_pSqlSession->GetSqlConnection())
			s = m_pSqlSession->GetSqlConnection()->NativeConvert(&DataStr(s));
		else
		{
			if (bOracle && len == 0)
			{
				s = " "; len++;
			}
			s =  '\'' + s + '\'';
			if (AfxGetLoginInfos()->m_bUseUnicode)
				s = 'N' + s;
		}

		if (bOracle)
		{
			s = cwsprintf(_T("CAST(%s AS %s(%d))"),
				(LPCTSTR)s,
				AfxGetLoginInfos()->m_bUseUnicode ?	_T("NVARCHAR2") : _T("VARCHAR2"),
				max(1, len)
				);
		}

		m_strSql +=  ' ' + s + ' ';
		return TRUE;
	}
	else if (t == T_WHEN)
	{
		parser.SkipToken(); 
	}
	else if (t == T_EVAL)
	{
		parser.SkipToken(); 

		Expression* pWhenExpr = new Expression(m_pSymbolTable);

		pWhenExpr->SetStopTokens(T_BRACECLOSE); pWhenExpr->GetStopTokens()->m_bSkipInnerBraceBrackets = TRUE;
		if (!pWhenExpr->Parse(parser, DataType::Variant, FALSE))
		{
			SetError(_TB("Error on parsing conditional expression of query tag"));
			return FALSE;
		}
		if (!parser.Match(T_BRACECLOSE)) 
		{
			//SetError(_TB("Expected BRACECLOSE token"));
			return FALSE;
		}

		int nIdx = m_TagLinks.GetSize();
		AddLink(strName, QueryObject::_EVAL, NULL, 0, pWhenExpr);
		CString strMarker = cwsprintf(_T("{EVAL%d}"), nIdx);
		m_strSql += cwsprintf(_T(" %s "), strMarker);
		((TagLink*)m_TagLinks[nIdx])->m_strSqlName = strMarker;

		return TRUE;
	}
	else
	{
		DataObj* pData = parser.ParseComplexData(FALSE);
		if (!pData)
		{
			SetError(_TB("Expected valid query tag"));
			return FALSE;
		}

		if (m_pSqlSession && m_pSqlSession->GetSqlConnection())
		{
			CString s = m_pSqlSession->GetSqlConnection()->NativeConvert(pData);

			m_strSql +=  ' ' + s + ' ';

			SAFE_DELETE(pData);
			return TRUE;
		}
		//manca la Connection quando il report è in modifica
		SAFE_DELETE(pData);
		return TRUE;
	}
	
	//T_INCLUDE, T_EXPAND

	Expression* pWhenExpr = new Expression(m_pSymbolTable);
	pWhenExpr->SetStopTokens(T_INCLUDE, T_EXPAND);
	if (!pWhenExpr->Parse(parser, DataType::Bool, FALSE))
	{
		SetError(_TB("Error on parsing conditional expression of query tag") + _T(". ") + pWhenExpr->GetErrDescription());
		delete pWhenExpr;
		return FALSE;
	}

	if (parser.Matched(T_INCLUDE))
	{
		if (parser.LookAhead() != T_ID) 
		{
			SetError(_TB("Expected field name"));
			return FALSE;
		}
		if (!parser.ParseID(strName)) 
		{
			SetError(_TB("Expected field name"));
			return FALSE;
		}
		if (!m_pSymbolTable->ExistField(strName)) 
		{
			SetError(_TB("Unknown field name"), strName);
			return FALSE;
		}
		SymField* pF = m_pSymbolTable->GetField(strName);
		if (pF == NULL) 
		{
			SetError(_TB("Expected string field"), strName);
			return FALSE;
		}
		DataObj* pObj = pF->GetData();
		if (pObj == NULL || pObj->GetDataType() != DataType::String) 
		{
			SetError(_TB("Expected string field"), strName);
			return FALSE;
		}

		int nIdx = m_TagLinks.GetSize();
		AddLink(strName, QueryObject::_INCLUDE, NULL, 0, pWhenExpr);
		CString strMarker = cwsprintf(_T("{INCLUDE%d}"), nIdx);
		m_strSql += cwsprintf(_T(" %s "), strMarker);
		((TagLink*)m_TagLinks[nIdx])->m_strSqlName = strMarker;

		if (!parser.Match(T_BRACECLOSE)) 
		{
			SetError(_TB("Expected BRACECLOSE token"), strName);
			return FALSE;
		}
		return TRUE;	
	}

	if (!parser.Match(T_EXPAND)) 
	{
		SetError(_TB("Expected EXPAND token"));
		return FALSE;
	}

	QueryObject* pQ = ParseSubQuery(parser);
	if (!pQ)
		return FALSE;

	QueryObject* pQryElse = NULL;
	if (parser.Matched(T_ELSE)) 
	{		
		pQryElse = ParseSubQuery (parser);
		if (!pQryElse)
		{
			delete pQ;
			return FALSE;
		}
	}

	if (!parser.Match(T_BRACECLOSE)) //chiusura tag WHEN - EXPAND
	{
		SetError(_TB("Expected BRACECLOSE token"));
		delete pQ;
		SAFE_DELETE(pQryElse);
		return FALSE;
	}

	int nIdx = m_TagLinks.GetSize();
	strName = cwsprintf(_T("{EXPAND%d}"), nIdx);
	
	AddLink(strName, QueryObject::_EXPAND, NULL, 0, pWhenExpr, pQ);
	
	m_strSql += cwsprintf(_T(" %s "), strName);

	((TagLink*)m_TagLinks[nIdx])->m_strSqlName = strName;

	if (pQryElse)
		((TagLink*)m_TagLinks[nIdx])->m_pElseClause = pQryElse;

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::Unparse(Unparser& unparser, BOOL bSkipHeader/* = FALSE*/, BOOL bSkipBeginEnd /*= FALSE*/) const
{
	if (!bSkipHeader)
	{
		unparser.		UnparseTag		(T_QUERY, FALSE);
		unparser.		UnparseBlank	();
		unparser.		UnparseID		(m_strQueryName, TRUE);
	}

	if (!bSkipBeginEnd)
	{
		unparser.		UnparseTag(T_BEGIN, TRUE);
		unparser.		IncTab();
		unparser.		UnparseTag		(T_BRACEOPEN, TRUE);
	}

	CString qt = m_strQueryTemplate; //E' ottenuta da una AuditString è ha quindi perso l'indentazione iniziale.
	qt.Trim( _T("\r\n\t "));
	CString sIndent('\t', unparser.GetTabCounter() + 1); 
	qt.Replace(_T("\r\n"), _T("\r\n") + sIndent);

	unparser.WriteString (sIndent + qt);	
	unparser.UnparseCrLf ();

	if (!bSkipBeginEnd)
	{
		unparser.		UnparseTag(T_BRACECLOSE, TRUE);
		unparser.		DecTab();
		unparser.		UnparseTag		(T_END, TRUE);
	}
	unparser.			UnparseCrLf		();

	return TRUE;
}

CString QueryObject::Unparse() const
{
	Unparser buff(TRUE);
	Unparse(buff, TRUE, TRUE);
	buff.Close();
	CString s = buff.GetBufferString();
	return s;
}

//------------------------------------------------------------------------------
BOOL QueryObject::Build ()
{
	Clear();

	m_arSelFieldName.RemoveAll();
	m_arParametersName.RemoveAll();

	CString strSql = m_strSql;

	if (!ExpandTemplate(strSql)) 
		return FALSE;

	SetCurrentQueryColumns();
	SetCurrentQueryParameters();

	m_poSqlRecord = new SqlRecordDynamic();
	m_poSqlTable = new SqlTable(m_poSqlRecord, m_pSqlSession);

	int nBindCol = 0;
	if (!BindColumn(m_poSqlTable, m_poSqlRecord, nBindCol)) 
		return FALSE;

	StripBlankNearSquareBrackets (strSql);
	while (strSql.Replace(L"\n\n", L"\n"));
	while (strSql.Replace(L"\r\n\r\n", L"\r\n"));

	m_poSqlTable->m_strSQL = strSql;

	int nBindPar = 0;
	if (!BindParameter(m_poSqlTable, NULL, nBindPar)) 
		return FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ExpandTemplate (CString& strSql)
{
	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (pTag->m_Direction == QueryObject::_EVAL) 
		{	
			DataObj* pData = NULL;

			ASSERT(pTag->m_pWhenExpr);
			
			if (!pTag->m_pWhenExpr->Eval(pData))
			{
				SAFE_DELETE(pData);
				return SetError(_TB("It fails evaluating expression of conditional subtag"), pTag->m_pWhenExpr->GetErrDescription());
			}

			if (pData && m_pSqlSession && m_pSqlSession->GetSqlConnection())
			{
				CString s = ' ' + m_pSqlSession->GetSqlConnection()->NativeConvert(pData) + ' ';

				strSql.Replace(pTag->m_strSqlName, s);
			}

			SAFE_DELETE(pData);
			continue;
		}

		if (
			pTag->m_Direction != QueryObject::_EXPAND &&
			pTag->m_Direction != QueryObject::_INCLUDE
			) 
			continue;

		ASSERT(pTag->m_pWhenExpr);
		DataBool dbWhen;
		if (!pTag->m_pWhenExpr->Eval(dbWhen))
			return SetError(_TB("It fails evaluating expression of conditional subtag"), pTag->m_pWhenExpr->GetErrDescription());
		pTag->m_bWhen = (BOOL) dbWhen;

		//per gli INCLUDE: occorre istanziare un oggetto subquery e parsarlo al volo a partire del valore corrente della variabile di woorm sorgente
		if (pTag->m_Direction == QueryObject::_INCLUDE)
		{
			if (!pTag->m_bWhen)
			{
				strSql.Replace(pTag->m_strSqlName, _T(" "));
				continue;
			}

			SAFE_DELETE(pTag->m_pExpandClause);

			SymField* pField = m_pSymbolTable->GetField(pTag->m_strName);
			ASSERT(pField);
			if (!pField)
				return SetError(_TB("Unknown field:"), pTag->m_strName);

			DataObj* o = pField->GetData();
			ASSERT(o && o->IsKindOf(RUNTIME_CLASS(DataStr)));

			CString sInc = ((DataStr*)o)->Str();
			Parser parser(sInc);
			parser.EnableAuditString();
			pTag->m_pExpandClause = new QueryObject(_T(""), m_pSymbolTable, m_pSqlSession, m_pParent ? m_pParent : this);

			if (!pTag->m_pExpandClause->ParseInternal(parser)) 
			{
				SetError(_TB("It fails parsing conditional subtag"), parser.GetError());
				parser.ClearError();
				return FALSE;
			}
		}

		//----

		QueryObject* pQ = pTag->m_bWhen ? pTag->m_pExpandClause : pTag->m_pElseClause;
		if (!pQ)
		{
			strSql.Replace(pTag->m_strSqlName, _T(" "));
			continue;
		}

		CString strSubSql = pQ->m_strSql;

		pQ->ExpandTemplate (strSubSql);

		strSql.Replace(pTag->m_strSqlName, strSubSql);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::BindColumn (SqlTable* pSqlTable, SqlRecord* pSqlRecord, int& nBind)
{
	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (
				pTag->m_Direction == QueryObject::_EXPAND ||
				pTag->m_Direction == QueryObject::_INCLUDE
			)
		{
			if (pTag->m_bWhen)
				pTag->m_pExpandClause->BindColumn(pSqlTable, pSqlRecord, nBind);
			else if (pTag->m_pElseClause)
				pTag->m_pElseClause->BindColumn(pSqlTable, pSqlRecord, nBind);
			continue;
		}

		if (pTag->m_Direction != QueryObject::_COL) continue;
		
		//pTag->m_strSqlName = cwsprintf(_T("COL_%d_%s"), ++nBind, pTag->m_strName);
		pTag->m_strSqlName = pTag->m_strName;	++nBind;

		SqlColumnInfo* pColInfo = new SqlColumnInfo(_T("_DYNAMIC_"), pTag->m_strSqlName, *(pTag->m_pData));
			pColInfo->m_bVirtual = FALSE; pColInfo->m_bVisible = TRUE; 
			pColInfo->m_lLength = pTag->m_nLen;
			pColInfo->UpdateDataObjType(pTag->m_pData);
			pColInfo->SetDataObjInfo(pTag->m_pData);
			if (pTag->m_pData->IsKindOf(RUNTIME_CLASS(DataText)))
				pColInfo->m_nSqlDataType = DBTYPE_WSTR;	//force unicode: sql datatype ntext

		SqlRecordItem* pRecItem = new SqlRecordItem(pTag->m_pData, pTag->m_strSqlName, pColInfo);
			pRecItem->m_bOwnColumnInfo = TRUE;
			pRecItem->m_lLength = pTag->m_nLen;
		pSqlRecord->Add(pRecItem);

		DBTYPE eSqlType = m_pSqlSession->GetSqlConnection()->GetSqlDataType(pTag->m_pData->GetDataType());

		pSqlTable->m_pColumnArray->Add(pTag->m_strSqlName, pTag->m_pData, eSqlType, nEmptySqlRecIdx);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::BindParameter (SqlTable* pSqlTable, SqlRecord* pSqlRecord, int& nBind, BOOL bIsProcedure)
{
	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (
				pTag->m_Direction == QueryObject::_EXPAND ||
				pTag->m_Direction == QueryObject::_INCLUDE
			)
		{
			if (pTag->m_bWhen)
				pTag->m_pExpandClause->BindParameter(pSqlTable, pSqlRecord, nBind, bIsProcedure);
			else if (pTag->m_pElseClause)
				pTag->m_pElseClause->BindParameter(pSqlTable, pSqlRecord, nBind, bIsProcedure);
			continue;
		}

		if (
			pTag->m_Direction != QueryObject::_IN &&
			pTag->m_Direction != QueryObject::_OUT &&
			pTag->m_Direction != QueryObject::_INOUT
			) continue;
		
		pTag->m_strSqlName = cwsprintf(_T("%s_%d"), pTag->m_strName, ++nBind);

		short nOleDbParamType = DBPARAMTYPE_INPUT;
		DBPARAMIO eParamType = DBPARAMIO_INPUT;
		switch (pTag->m_Direction)
		{
			case QueryObject::_IN:
					eParamType		= DBPARAMIO_INPUT;
					nOleDbParamType = DBPARAMTYPE_INPUT;
					break;
			case QueryObject::_OUT:
					eParamType		= DBPARAMIO_OUTPUT;
					nOleDbParamType = DBPARAMTYPE_OUTPUT;
					break;
			case QueryObject::_INOUT:
					eParamType		= DBPARAMIO_OUTPUT | DBPARAMIO_INPUT;
					nOleDbParamType = DBPARAMTYPE_INPUTOUTPUT;
					break;
			default:
				ASSERT(FALSE);
				return SetError(_TB("Unknown parameter direction")+ cwsprintf(_T(" (%s)"), pTag->m_strName));
		}

		SymField* pField = m_pSymbolTable->GetField(pTag->m_strName);
		if (!pField)
			return SetError(_TB("Unknown field into QueryObject::BindParam:"), pTag->m_strName);
		DataObj* pO = pField->GetData();

		TRACE(_T("Parameter %s: %s\n"), pTag->m_strSqlName, pO->FormatData());
		if (pTag->m_Direction != QueryObject::_OUT)
			pTag->m_pData->Assign(*pO);
			
		ASSERT_VALID(pSqlTable);
		ASSERT_VALID(pTag->m_pData);

		if (!bIsProcedure)
		{
			pSqlTable->AddParam(pTag->m_strSqlName, *(pTag->m_pData), eParamType);
			pSqlTable->SetParamValue(pTag->m_strSqlName, *pTag->m_pData);
		}
		else	
			pSqlTable->AddProcParam(pTag->m_strSqlName, nOleDbParamType, pTag->m_pData);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ReplaceInputParameters (CString& sSql, int& nStartQuestionMarkPos/*=0*/)
{
	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (
				pTag->m_Direction == QueryObject::_EXPAND ||
				pTag->m_Direction == QueryObject::_INCLUDE
			)
		{
			if (pTag->m_bWhen)
				pTag->m_pExpandClause->ReplaceInputParameters(sSql, nStartQuestionMarkPos);
			else if (pTag->m_pElseClause)
				pTag->m_pElseClause->ReplaceInputParameters(sSql, nStartQuestionMarkPos);

			continue;
		}

		if (
			pTag->m_Direction == QueryObject::_OUT ||
			pTag->m_Direction == QueryObject::_INOUT
			) 
			return SetError(_TB("Execute methods cannot support output parameter") + cwsprintf(_T(" (%s)"), pTag->m_strName));

		if (pTag->m_Direction == QueryObject::_COL) 
			return SetError(_TB("Execute methods cannot support selected column") + cwsprintf(_T(" (%s)"), pTag->m_strName));
		
		SymField* pField = m_pSymbolTable->GetField(pTag->m_strName);
		if (!pField)
			return SetError(_TB("Unknown field into QueryObject::BindParam:"), pTag->m_strName);
		DataObj* pO = pField->GetData();

		CString sValue =  m_pSqlSession->GetSqlConnection()->NativeConvert(pO);
		TRACE(_T("Parameter %d: %s\n"), i, sValue);

		int nPos = sSql.Mid(nStartQuestionMarkPos).Find('?');
		if (nPos < 0)
			return SetError( _TB("It fails evaluating input parameter") + cwsprintf(_T(" (%s)"), pTag->m_strName), sSql);
		nPos += nStartQuestionMarkPos;

		nStartQuestionMarkPos = nPos + sValue.GetLength() + 1;

		sSql = sSql.Left(nPos) + ' ' + sValue + sSql.Mid(nPos + 1);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
///<summary>
///Open cursor 
///</summary>
//[TBWebMethod(name = Query_Open, woorm_method=false)]
DataBool QueryObject::Open ()
{
	Close ();

	if (!Build())
	{
		return SetError(_TB("Query is not well formed")), ShowError();
	}

	ASSERT_VALID(m_poSqlTable);

#ifdef _DEBUG
	{
		TRACE(_T("Query %s Sql:\n%s\nend query\n"), m_strQueryName, m_poSqlTable->m_strSQL);
		CString sTest (m_poSqlTable->m_strSQL);
		sTest.Trim();
		sTest.MakeUpper();
		if (sTest.Find(L"SELECT") < 0)
		{
			TRACE2("QueryOpen/QueryRead: sql query does not begin with a SELECT statment:\n", m_strQueryName, m_poSqlTable->m_strSQL);
			if (sTest.Find(L"INSERT") == 0 || sTest.Find(L"DELETE") == 0 || sTest.Find(L"UPDATE") == 0)
			{
				TRACE("Probably you have to use QueryExecute\n");
			}
			ASSERT(FALSE);
		}
	}
#endif

	TRY
	{
		m_poSqlTable->SetUseDataCaching (FALSE);
		if (m_bUseCursor)
			m_poSqlTable->Open(m_bCursorUpdatable, m_CursorType, m_bSensibility);		
		else
			m_poSqlTable->Open(FALSE, E_FAST_FORWARD_ONLY);
	}
	CATCH(SqlException, e)	
	{
		return SetError(_TB("It is not well formed"), e->m_strError), ShowError();
	}
	END_CATCH

	if (m_poSqlTable->HasError())
	{
		return SetError(_TB("It is not well formed"), m_poSqlTable->GetError()), ShowError();
	}

	return TRUE;
}

//------------------------------------------------------------------------------
///<summary>
///Close cursor 
///</summary>
//[TBWebMethod(name = Query_Close, woorm_method=false)]
DataBool QueryObject::Close ()
{
	if (IsOpen())
		m_poSqlTable->Close();

	m_nQueryHandle = -1;
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ValorizeColumns (BOOL bFetched /*=TRUE*/, SqlTable* paramTable /*=NULL*/)
{
	TRACE(_T("Query %s Fetch row\n"), m_strQueryName);

	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (
				pTag->m_Direction == QueryObject::_EXPAND ||
				pTag->m_Direction == QueryObject::_INCLUDE
			)
		{
			if (pTag->m_bWhen)
				pTag->m_pExpandClause->ValorizeColumns(bFetched);
			else if (pTag->m_pElseClause)
				pTag->m_pElseClause->ValorizeColumns(bFetched);
			continue;
		}

		if (
			pTag->m_Direction != QueryObject::_COL &&
			pTag->m_Direction != QueryObject::_OUT &&
			pTag->m_Direction != QueryObject::_INOUT
			) continue;
		

		DataObj* pO = pTag->m_pData;

		SymField* pField = m_pSymbolTable->GetField(pTag->m_strName);
		if (!pField)
		{
			ASSERT(FALSE);
			continue;
		}

		int lev = m_pSymbolTable->GetDataLevel();

		if (pO && pO->IsValid()/* && bFetched preferiscono i default...*/)
		{
			if (m_bIsQueryRule)
			{
				ASSERT(lev == RULE_ENGINE);
				pField->GetData(lev)->Assign(*pO);

				pField->RuleUpdated();
			}
			else if (m_bValorizeAll)
			{
				pField->GetData(0)->Assign(*pO);
				pField->GetData(1)->Assign(*pO);
				pField->GetData(2)->Assign(*pO);
			}
			else
				pField->GetData(lev)->Assign(*pO);
		}
		else
		{
			if (m_bIsQueryRule) pField->RuleNullified();
			else pField->GetData(lev)->Clear(FALSE);
				
		}

		TRACE(_T("Column %s: %s\n"), pTag->m_strSqlName, pTag->m_pData->FormatData());
	}
	return TRUE;
}

//------------------------------------------------------------------------------
TagLink* QueryObject::GetColumn (const CString& name)
{
	for (int i = 0; i < m_TagLinks.GetSize(); i++)
	{
		TagLink* pTag = (TagLink*)m_TagLinks.GetAt(i);

		if (
				pTag->m_Direction == QueryObject::_EXPAND ||
				pTag->m_Direction == QueryObject::_INCLUDE
			)
		{
			if (pTag->m_bWhen)
			{
				pTag = pTag->m_pExpandClause->GetColumn(name);
				if (pTag)
					return pTag;
			}
			else if (pTag->m_pElseClause)
			{
				pTag = pTag->m_pElseClause->GetColumn(name);
				if (pTag)
					return pTag;
			}
			continue;
		}

		if (pTag->m_Direction != QueryObject::_COL) continue;
		
		if (pTag->m_strName.CompareNoCase(name) == 0)
			return pTag;
	}
	return NULL;
}

//------------------------------------------------------------------------------
///<summary>
///Fetch row 
///</summary>
//[TBWebMethod(name = Query_Read, woorm_method=false)]
DataBool QueryObject::Read ()
{
	if (!IsOpen() && !Open())
		return SetError(_TB("It is not opened")), ShowError();
	
	if (m_poSqlTable->IsPreQueryState())
	{
		TRY
		{
			m_poSqlTable->Query();
		}
		CATCH(SqlException, e)	
		{
			return SetError(_TB("DB Error"), e->m_strError), ShowError();
		}
		END_CATCH

		if (m_poSqlTable->HasError())
		{
			return SetError(_TB("It is not well formed"), m_poSqlTable->GetError()), ShowError();
		}
	}
	else
	{
		TRY
		{
			if (m_poSqlTable->IsEOF())
			{
				ValorizeColumns(FALSE);

				Close();
				return FALSE; //SetError(_TB("There are not more rows")), ShowError();
			}

			m_poSqlTable->MoveNext();
		}
		CATCH(SqlException, e)	
		{
			return SetError(_TB("DB error"), e->m_strError), ShowError();
		}
		END_CATCH

	}

	if (!m_poSqlTable->IsEOF())
	{
		ValorizeColumns ();
		return TRUE;
	}
	
	ValorizeColumns (FALSE);

	Close();
	return FALSE;
}

//------------------------------------------------------------------------------
///<summary>
///Fetch one row 
///</summary>
//[TBWebMethod(name = Query_ReadOne, woorm_method=false)]
DataBool QueryObject::ReadOne ()
{
	BOOL bOk = Open();
	bOk = bOk && Read();
	Close();
	return DataBool(bOk);
}

//------------------------------------------------------------------------------
// non esiste in ADO.NET
BOOL QueryObject::IsEof () const
{
	if (m_poSqlTable == NULL || !m_poSqlTable->IsOpen())
		return TRUE;

	return 	m_poSqlTable->IsEOF();
}

BOOL QueryObject::IsBof() const
{
	if (m_poSqlTable == NULL || !m_poSqlTable->IsOpen())
		return TRUE;

	return 	m_poSqlTable->IsBOF();
}

//------------------------------------------------------------------------------
BOOL QueryObject::IsEmpty() const
{
	if (m_poSqlTable == NULL || !m_poSqlTable->IsOpen())
		return TRUE;

	return 	m_poSqlTable->IsEmpty();
}

//------------------------------------------------------------------------------
BOOL QueryObject::IsOpen () const
{
	return m_poSqlTable && m_poSqlTable->IsOpen();
}

//------------------------------------------------------------------------------
///<summary>
///Execute sql statment
///</summary>
//[TBWebMethod(name = Query_Execute, woorm_method=false)]
DataBool QueryObject::Execute ()
{
	Clear();

	CString strSql = m_strSql;

	if (!ExpandTemplate(strSql)) 
		return SetError(_TB("It fails to expand conditional tags"), strSql), ShowError();

#ifdef _DEBUG
	{
		CString sTest (strSql);
		sTest.Trim();
		sTest.MakeUpper();
		ASSERT_TRACE2(sTest.Find(L"SELECT") != 0, "QueryObject::Execute: sql query cannot begin with a SELECT statment:\n", m_strQueryName, m_strSql);
	}
#endif

	int startQuestionMarkerPos = 0;
	if (!ReplaceInputParameters(strSql, startQuestionMarkerPos)) 
		return ShowError();

	StripBlankNearSquareBrackets (strSql);

	TRY
	{	
		m_pSqlSession->GetSqlConnection()->ExecuteSQL(strSql, m_pSqlSession);
	}
	CATCH(SqlException, e)	
	{
		return SetError(e->m_strError, strSql), ShowError();
	}
	END_CATCH

	return TRUE;
}

//------------------------------------------------------------------------------
CString QueryObject::GetQueryName()
{
	return m_pParent ? m_pParent->m_strQueryName + _T("-") + m_strQueryName : m_strQueryName;
}

//------------------------------------------------------------------------------
BOOL QueryObject::SetError(LPCTSTR szErr, LPCTSTR szAuxErr)
{
	CMessages* pMsg = m_pParent ? &(m_pParent->m_msg) : &m_msg;

	pMsg->Add(_TB("Query object error") + cwsprintf(_T(" (%s) "), GetQueryName()),  szErr);
	pMsg->Add(m_strSql, CMessages::MSG_WARNING);
	if (szAuxErr)
		pMsg->Add(szAuxErr, CMessages::MSG_WARNING);

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL QueryObject::ShowError()
{
	if (m_pParent == NULL)
	{
		BOOL bRet = m_msg.Show(TRUE);
		CBaseDocument* pDoc = m_pSymbolTable ? m_pSymbolTable->GetDocument() : NULL;
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
		{
			pDoc->PostMessage(WM_COMMAND, ID_STOP, 0);
		}
	}

	return FALSE;
}

//------------------------------------------------------------------------------
/*
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TestReportQuery]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[TestReportQuery]
GO

CREATE PROCEDURE TestReportQuery
	(
		@par_in_a int, 
		@par_in_b varchar (32),	
		@par_out_c int OUTPUT,
		@par_out_d varchar (32) OUTPUT
	) AS 

	
	SET @par_out_d = 'test-' + Upper( @par_in_b )
	SET @par_out_c = @par_in_a * 10
		
	RETURN @par_in_a + 1

GO

*/

//------------------------------------------------------------------------------
DataBool QueryObject::Call ()
{
	Close();
	Clear();

	CString strSql = m_strSql;

	if (!ExpandTemplate(strSql)) 
		return SetError(_TB("It fails expanding conditional tags"), strSql), ShowError();

	m_poSqlRecord = new SqlRecordProcedureQuery();
	
	m_poSqlTable = new SqlTable(m_poSqlRecord, m_pSqlSession);

	StripBlankNearSquareBrackets (strSql);

	m_poSqlTable->m_strSQL = cwsprintf(_T("{ %s }") , strSql);

	TRACE(_T("Query %s Sql:\n%s\nend query\n"), m_strQueryName, m_strSql);

	int nBindPar = 0;
	if (!BindParameter(m_poSqlTable, NULL, nBindPar, TRUE)) 
		return SetError(_TB("It fails binding parameters"), strSql), ShowError();

	TRY
	{
		m_poSqlTable->Open();
		m_poSqlTable->DirectCall();
		
		ValorizeColumns(TRUE, m_poSqlTable);

		m_poSqlTable->Close();
	}
	CATCH(SqlException, e)	
	{
		return SetError(e->m_strError, strSql), ShowError();
	}
	END_CATCH

	return TRUE;
}

//-----------------------------------------------------------------------------
CString QueryObject::ToSqlString() const
{
	CString query = m_poSqlTable->ToString(TRUE, FALSE, FALSE);
	return query;
}

///////////////////////////////////////////////////////////////////////////////
//						SqlRecordProcedureQuery
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(SqlRecordProcedureQuery, SqlRecordProcedure) 

//-----------------------------------------------------------------------------
SqlRecordProcedureQuery::SqlRecordProcedureQuery()
:
	SqlRecordProcedure(_T("_QUERY_PROC_"))
{
	SetValid (TRUE); //NON ha il TableInfo
	BindRecord();
}

//-----------------------------------------------------------------------------
void SqlRecordProcedureQuery::BindRecord()
{
}

//==============================================================================
