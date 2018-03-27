
#include "stdafx.h"

#include <TbNameSolver\TBNamespaces.h>

#include <TbParser\Parser.h>

#include <TbGenlib\FunProto.h>
#include <TbGenlib\expr.h>
#include <TbGenlib\oslbaseinterface.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>	

#include "rpsymtbl.h"
#include "report.h"
#include "ActionsRepEngin.h"
#include "ruledata.h"
#include "QueryObject.h"
#include "ExportSymbols.h"
#include "EdtMng.h"
#include "PrgData.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// RuleDataObj
//============================================================================
//----------------------------------------------------------------------------
RuleDataObj::RuleDataObj(WoormTable& aSymTable) 
	: 
	m_SymTable(aSymTable)
{}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void RuleDataObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n\tRuleDataObj:", this->GetRuntimeClass()->m_lpszClassName);
}

void RuleDataObj::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG

//============================================================================
// RuleDataArray
//============================================================================
//----------------------------------------------------------------------------
RuleDataArray::RuleDataArray(WoormTable& aSymTable)
	:
	m_SymTable  	(aSymTable),
	m_pLastTblRule	(NULL)
{
}

//----------------------------------------------------------------------------
RuleDataArray::~RuleDataArray()
{
}

//----------------------------------------------------------------------------
void RuleDataArray::RemoveAt	(int nIndex, int nCount/* = 1*/)
{
	ASSERT_VALID(this);
	ASSERT(nIndex >= 0);
	ASSERT(nCount >= 0);
	ASSERT(nIndex + nCount <= m_nSize);

	// search for last table rule in deleted object
	for (int i = nIndex; i < nIndex + nCount; i++)
		if (GetAt(i) == m_pLastTblRule)
		{
			m_pLastTblRule = NULL;
			break;
		}
		
	// do remove		
	Array::RemoveAt(nIndex, nCount);
}

//----------------------------------------------------------------------------
void RuleDataArray::RemoveAll ()
{
	m_pLastTblRule = NULL;
	Array::RemoveAll ();
}

//----------------------------------------------------------------------------
int RuleDataArray::Add(RuleDataObj* pR)
{
	int idx = __super::FindPtr(pR);
	if (idx > -1)
		return idx;

	idx = __super::Add(pR);

	if (pR->IsKindOf(RUNTIME_CLASS(TblRuleData)))
		m_pLastTblRule = (TblRuleData*)pR;

	return idx;
}

//----------------------------------------------------------------------------
RuleDataObj* RuleDataArray::GetRuleData(WORD id)
{
    SymField* pField = m_SymTable.GetFieldByID(id);
	CString strPublicName = pField ? pField->GetName() : _T("");
    if (strPublicName.IsEmpty())
    	return NULL;

	return GetRuleData(strPublicName);
}

RuleDataObj* RuleDataArray::GetRuleData(const CString& strPublicName)
{
    for (int i = 0; i < GetSize(); i++)
    {
        RuleDataObj* pRuleDataObj = (RuleDataObj*) GetAt(i);

        if (pRuleDataObj->ExistPublicName(strPublicName))
            return pRuleDataObj;
    }
    return NULL;
}

//----------------------------------------------------------------------------
BOOL RuleDataArray::IsEmpty()
{
    return GetSize() == 0;
}

//----------------------------------------------------------------------------
BOOL RuleDataArray::CanDeleteField(LPCTSTR pszFieldName) const
{
    for (int i = 0; i < GetSize(); i++)
    {
        RuleDataObj* pRuleDataObj = (RuleDataObj*) GetAt(i);

        if (!pRuleDataObj->CanDeleteField(pszFieldName))
           return FALSE;
    }

    return TRUE;
}

//	return nIndex of a specific rule, if aRuleType == TBLRULE_TYPE then it is
//	possibile indicate a particolar nReplic. This is because the ruleName is
//	the m_strTableName and it can be present more than once time.
//
//----------------------------------------------------------------------------
int RuleDataArray::GetRuleIndex(LPCTSTR pszRuleName, RuleType aRuleType, int nReplic)
{
	int nReplicCount = 0;
	CStringArray arTableNames;

    for (int nIdx = 0; nIdx < GetSize(); nIdx++)
    {
        RuleDataObj* pRuleDataObj = (RuleDataObj*) GetAt(nIdx);
        switch (aRuleType)
        {
            case RULE_DATA_TABLE:
				if (pRuleDataObj->IsARule() == RULE_DATA_TABLE)
				{
					((TblRuleData*) pRuleDataObj)->GetTableNames(arTableNames);
					for (int i = 0; i < arTableNames.GetSize(); i++)
					{
						if (_tcsicmp(pszRuleName, arTableNames.GetAt(i)) == 0)
						{	
							if (nReplic == nReplicCount)
								return nIdx;
							else
							   nReplicCount++;
						    
							break;
						}
					}	
				}
 				break;
           case RULE_EXPR:
                if	(
						(pRuleDataObj->IsARule() == RULE_EXPR) &&
						(_tcsicmp(pszRuleName, ((ExpRuleData*) pRuleDataObj)->GetPublicName()) == 0)
					)
				return nIdx;
		   case RULE_NAMED_QUERY:
			   if (
				   (pRuleDataObj->IsARule() == RULE_NAMED_QUERY) &&
				   (_tcsicmp(pszRuleName, ((QueryRuleData*)pRuleDataObj)->GetPublicName()) == 0)
				   )
				   return nIdx;
		}
    }

	return -1;
}

//----------------------------------------------------------------------------
RuleDataObj* RuleDataArray::GetRuleData(LPCTSTR pszRuleName, RuleType aRuleType, int nReplic)
{
    int nIdx = GetRuleIndex(pszRuleName, aRuleType, nReplic);
    return (nIdx == -1 ? NULL : (RuleDataObj*) GetAt(nIdx));
}

//----------------------------------------------------------------------------
TblRuleData* RuleDataArray::GetTblRuleData(const CString& sTableName)
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(GetAt(nIdx));
		if (!pTblRule) continue;

		if (pTblRule->GetTableIndex(sTableName) > -1)
			return pTblRule;
	}
	return NULL;
}

//----------------------------------------------------------------------------
TblRuleData* RuleDataArray::GetTblRuleDataFromField(const CString& sPublicName)
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(GetAt(nIdx));
		if (!pTblRule) continue;

		if (pTblRule->ExistPublicName(sPublicName))
			return pTblRule;
	}
	return NULL;
}

//----------------------------------------------------------------------------
int RuleDataArray::GetQueryRuleDataIndex(QueryObjItem* pQryItem)
{
	for (int i = 0; i < GetSize(); i++)
	{
		QueryRuleData* pQueryRule = dynamic_cast<QueryRuleData*>(GetAt(i));
		if (!pQueryRule) continue;

		if (pQueryRule->GetQueryItem() == pQryItem)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
void RuleDataArray::DeleteField(LPCTSTR pszFieldName)
{
    // it is an array of nIndex at RuleDataObj that must be deleted.
    //
    CWordArray  idxToDelete;
    int			nIdx;
        
    for (nIdx = 0; nIdx < GetSize(); nIdx++)
    {
        RuleDataObj* pRuleDataObj = (RuleDataObj*) GetAt(nIdx);

        pRuleDataObj->DeleteField(pszFieldName);

        // If the rule is empty then it is added to the deleted list and after
        // it will be removed from the array.
        // This is done because it is not possible remove an object 
        // while it run over the array.
        //
        if (pRuleDataObj->IsEmpty())
        {       
        	// if empty rule is equal to last Table Rule used, then
        	// no last Table Rule is available
        	//
			if (pRuleDataObj == m_pLastTblRule)
				m_pLastTblRule = NULL;

			// must cast because lack of int array				
        	idxToDelete.Add((WORD) nIdx);
        }
    }

    // the run over the "idxToDelete" must be done from bottom to the top
    // so the absolute position not change
    //
    for (nIdx = idxToDelete.GetUpperBound(); nIdx >= 0; nIdx--)
    {
        RemoveAt(idxToDelete[nIdx]);
    }
}

//----------------------------------------------------------------------------
void RuleDataArray::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		RuleDataObj* pRuleDataObj = (RuleDataObj*)GetAt(nIdx);

		pRuleDataObj->RenameField(pszOldFieldName, pszNewFieldName);
	}
}

//----------------------------------------------------------------------------
BOOL RuleDataArray::Parse(Parser& lex, SqlConnection* pSqlConnection)
{
	//lex.GetCommentTrace(this->m_arCommentTraceBefore);
	lex.RemoveCommentTrace();

	if (!lex.LookAhead(T_RULES))
		return (!lex.Bad());
		
    lex.ParseTag(T_RULES);

    do {
        switch (lex.LookAhead())
        {   
            case T_FROM :
            {
                TblRuleData* pRule = new TblRuleData(m_SymTable, pSqlConnection);
                Add(pRule);

				lex.GetCommentTrace(pRule->m_arCommentTraceBefore);
                if (!pRule->Parse(lex))   
					return FALSE;
				lex.GetCommentTrace(pRule->m_arCommentTraceAfter);

                break;
            }
            case T_IF       :
            case T_ID       :
            {
                ExpRuleData* pRule = new ExpRuleData(m_SymTable);
                Add(pRule);

 				lex.GetCommentTrace(pRule->m_arCommentTraceBefore);
                if (!pRule->Parse(lex))   
					return FALSE;
				lex.GetCommentTrace(pRule->m_arCommentTraceAfter);

                break;
            }
            
			case T_FOR      :
			case T_WHILE    :
            {
                WhileRuleData* pRule = new WhileRuleData(m_SymTable);
                Add(pRule);

				lex.GetCommentTrace(pRule->m_arCommentTraceBefore);
                 if (!pRule->Parse(lex))   
					return FALSE;
				lex.GetCommentTrace(pRule->m_arCommentTraceAfter);

                break;
            }

            case T_QUERY      :
            {
                QueryRuleData* pRule = new QueryRuleData(m_SymTable);
                Add(pRule);

				lex.GetCommentTrace(pRule->m_arCommentTraceBefore);
                if (!pRule->Parse(lex))   
					return FALSE;
				lex.GetCommentTrace(pRule->m_arCommentTraceAfter);

                break;
            }
           default         :
                return lex.SetError(_TB("Illegal rule definition"));
        }
    } while (!lex.LookAhead(T_END) && !lex.Bad());
    
	lex.GetCommentTrace(this->m_arCommentTraceInner);

    BOOL bOk = lex.ParseEnd();

	lex.GetCommentTrace(this->m_arCommentTraceAfter);

	return bOk;
}

//----------------------------------------------------------------------------
void RuleDataArray::Unparse(Unparser& oFile)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (GetSize() == 0) 
		return;
	
    oFile.IncTab();
    oFile.UnparseTag(T_RULES, TRUE);
    oFile.IncTab();

    int numRules = GetSize();
    for (int i = 0; i < numRules; i++)
    {
		if (i > 0) oFile.UnparseCrLf();

        RuleDataObj* pRuleDataObj = (RuleDataObj*) GetAt(i);

 		oFile.UnparseComment(pRuleDataObj->m_arCommentTraceBefore);
        if (!pRuleDataObj->IsEmpty())
            pRuleDataObj->Unparse(oFile);
 		oFile.UnparseComment(pRuleDataObj->m_arCommentTraceAfter);
    }

	oFile.UnparseComment(this->m_arCommentTraceInner);

	oFile.UnparseEnd();
	oFile.UnparseCrLf();
		
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//============================================================================
// TblRuleData
//============================================================================
//----------------------------------------------------------------------------
TblRuleData::TblRuleData(WoormTable& aSymTable, SqlConnection* pSqlConnection, LPCTSTR pszTblName)
    :
    RuleDataObj			(aSymTable),
 	m_pSqlConnection	(pSqlConnection),

    m_ConstraintMode	(RuleSelectionMode::SEL_ALL),
	m_bDistinct			(FALSE),
	m_nTop				(0),
	m_pWhereClause		(NULL),
	m_pHavingClause		(NULL),
	m_pWhenExpr			(NULL),

	m_bTemporaryJoin	(FALSE)
{
	if (pszTblName)
		m_arSqlTableJoinInfoArray.Add(pSqlConnection, &aSymTable, pszTblName);
}

//----------------------------------------------------------------------------
TblRuleData::~TblRuleData()
{
	SAFE_DELETE(m_pWhereClause);
	SAFE_DELETE(m_pHavingClause);
	SAFE_DELETE(m_pWhenExpr);
}

//-----------------------------------------------------------------------------
void TblRuleData::Empty() 
{
	SAFE_DELETE(m_pWhereClause);
	SAFE_DELETE(m_pHavingClause);
	SAFE_DELETE(m_pWhenExpr);

	m_arSqlTableJoinInfoArray.RemoveAll();
	//m_arFieldLinks.RemoveAll();
	m_CalcColumnLinks.RemoveAll();

	m_ConstraintMode = SEL_ALL;
	m_bDistinct = FALSE;
	m_nTop = 0;
	m_bTemporaryJoin = FALSE;
}

//----------------------------------------------------------------------------
RuleType TblRuleData::IsARule()
{
    return RULE_DATA_TABLE;
}

//-----------------------------------------------------------------------------
CString TblRuleData::GetSelectionModeDescr(RuleSelectionMode selMode)
{
	switch (selMode)
	{
		case RuleSelectionMode::SEL_ALL:
			return L"All";
		case RuleSelectionMode::SEL_NOT_NULL:
			return L"Not Null";
		case RuleSelectionMode::SEL_NULL:
			return L"Null";
		default:
			ASSERT(FALSE);
	}
	return L"";
}

CString TblRuleData::GetSelectionModeDescr() const
{
	return GetSelectionModeDescr(m_ConstraintMode);
}

//----------------------------------------------------------------------------
WClause* TblRuleData::GetWhereClause() const
{
	ASSERT_VALID(m_pWhereClause);
	return m_pWhereClause;
}

//----------------------------------------------------------------------------
void TblRuleData::AddWhereClause() 
{
	ASSERT(!m_pWhereClause);
	m_pWhereClause = new WClause(m_pSqlConnection, &m_SymTable, &m_arSqlTableJoinInfoArray, m_SqlTest.m_pDataTable);
}
//----------------------------------------------------------------------------
void TblRuleData::AddHavingClause()
{
	ASSERT(!m_pHavingClause);
	m_pHavingClause = new WClause(m_pSqlConnection, &m_SymTable, &m_arSqlTableJoinInfoArray, m_SqlTest.m_pDataTable);
	m_pHavingClause->SetHavingClause();
}

//----------------------------------------------------------------------------
void TblRuleData::AssignWhereClause(const WClause& aWhereClause)
{
	ASSERT_VALID(&aWhereClause);
	if (m_pWhereClause)
	{
		ASSERT_VALID(m_pWhereClause);
		*m_pWhereClause = aWhereClause;
	}
	else
		m_pWhereClause = new WClause(aWhereClause);
}

//----------------------------------------------------------------------------
WClause* TblRuleData::GetHavingClause() const
{
	ASSERT_VALID(m_pHavingClause);
	return m_pHavingClause;
}

//----------------------------------------------------------------------------
void TblRuleData::AssignHavingClause(const WClause& aHavingClause)
{
	ASSERT_VALID(&aHavingClause);

	if (m_pHavingClause)
	{
		ASSERT_VALID(m_pHavingClause);
		*m_pHavingClause = aHavingClause;
	}
	else
		m_pHavingClause = new WClause(aHavingClause);
}

//-----------------------------------------------------------------------------
CString TblRuleData::GetRuleDescription() const
{
	//return GetTableNames();

	if (m_arSqlTableJoinInfoArray.GetSize() == 0)
		return L"";

	CString sDescr;

	CString sTable = m_arSqlTableJoinInfoArray.GetAt(0)->GetTableName();
	int idx = sTable.Find('_');
	if (idx > -1 && idx < 4 && sTable.GetLength() > 7)
		sDescr = sTable.Mid(idx + 1);
	else sDescr = sTable;

	for (int i = 1; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		sDescr += L" - ";

		sTable = m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName();

		int idx = sTable.Find('_');
		if (idx > -1 && idx < 4 && sTable.GetLength() > 7)
			sDescr += sTable.Mid(idx + 1);
		else sDescr += sTable;
	}

	return sDescr;
}

//----------------------------------------------------------------------------
CString TblRuleData::GetTableNames() const
{
	if (m_arSqlTableJoinInfoArray.GetSize() == 0)
		return L"";

	CString sTables = m_arSqlTableJoinInfoArray.GetAt(0)->GetTableName();

	for (int i = 1; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		sTables += L", " + m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName();
	}

	return sTables;
}

//----------------------------------------------------------------------------
void TblRuleData::GetTableNames(CStringArray &arTableNames) const
{   
	arTableNames.RemoveAll();
	for (int i = 0; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		arTableNames.Add(m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName());
	}
}

//----------------------------------------------------------------------------
BOOL TblRuleData::IsEmpty() 
{
	return 
		(
		  (m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() == 0)
		  && 
		  (m_CalcColumnLinks.GetSize() == 0)
		 );
}

//----------------------------------------------------------------------------
BOOL TblRuleData::CanDeleteField(LPCTSTR pszFieldName) const
{
	for (int i = 1; i < this->m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
	{
		WClause* pJoinOn = m_arSqlTableJoinInfoArray.m_arJoinOn.GetAt(i);
		if (pJoinOn && pJoinOn->HasMember(pszFieldName))
			return FALSE;
	}

	return 
		(
			(m_pWhereClause == NULL || !m_pWhereClause->HasMember(pszFieldName)) 
			&& 
			(m_pHavingClause == NULL || !m_pHavingClause->HasMember(pszFieldName))
			&& 
			(m_pWhenExpr == NULL || !m_pWhenExpr->HasMember(pszFieldName))
		);
}

//----------------------------------------------------------------------------
BOOL TblRuleData::CanDeleteRule(CString& sUsedFieldName, CString& sLog) const
{
	CWoormDoc* pWDoc = GetSymTable()->GetDocument();
	if (!pWDoc)
		return TRUE;
	EditorManager* pEM = pWDoc->GetEditorManager();
	if (!pEM)
		return TRUE;

	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);
			
			if (!pEM->GetPrgData()->CanDeleteField(pObjLink->m_strPublicName, sLog))
			{
				sUsedFieldName = pObjLink->m_strPublicName;
				sLog = _TB("Column name:") + ' ' + (m_arSqlTableJoinInfoArray.GetSize() > 1 ? m_arSqlTableJoinInfoArray[j]->GetTableName() + '.' : L"") + pObjLink->m_strPhysicalName;
				return FALSE;
			}
		}
	}
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (!pEM->GetPrgData()->CanDeleteField(pObjLink->m_strPublicName, sLog))
		{
			sUsedFieldName = pObjLink->m_strPublicName;
			return FALSE;
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
DataFieldLink* TblRuleData::AddLink(LPCTSTR pszPhysicalName, LPCTSTR pszPublicName, BOOL bNativeExpr, DataType type, int nTable /*-1*/) //aggiunto nTable
{
	//se nTable nn e' indice di un array gia' esistente, aggiungo un nuovo array  
	if (!bNativeExpr && nTable < 0)
		return NULL;

	DataFieldLink* dfl = new DataFieldLink(pszPhysicalName, pszPublicName, FALSE, type);

	if (!bNativeExpr)    //DB Table Column
	{
		if (nTable >= 0 && nTable < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize())
			m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(nTable)->Add(dfl);
		else 
		{  
			ASSERT(FALSE);
			DataFieldLinkArray* arFieldLink = new DataFieldLinkArray();
			arFieldLink->Add(dfl);
			m_arSqlTableJoinInfoArray.m_arFieldLinks.Add(arFieldLink);
		}
	}
	else    // calculated column
	{
		m_CalcColumnLinks.Add(dfl);
	}
	return dfl;
}

//----------------------------------------------------------------------------
void TblRuleData::AddTableInfo(SqlTableInfo* tableInfo, const CString& sAlias/* = CString()*/)
{
	m_arSqlTableJoinInfoArray.Add(this->GetConnection(), GetSymTable(), tableInfo);
	m_arSqlTableJoinInfoArray.m_arstrAliasTableName.Add(sAlias);
}

int TblRuleData::AddJoinTable(SqlTableInfo* tableInfo)
{
	int pos = m_arSqlTableJoinInfoArray.Add(GetConnection(), GetSymTable(), tableInfo);

	if (m_pWhereClause)
		m_pWhereClause->SetTableInfo(&m_arSqlTableJoinInfoArray);
	if (m_pHavingClause)
		m_pHavingClause->SetTableInfo(&m_arSqlTableJoinInfoArray);

	return pos;
}

//----------------------------------------------------------------------------
int TblRuleData::GetTableIndex(const CString& sTableName)
{
	for (int i = 0; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		if (sTableName.CompareNoCase(m_arSqlTableJoinInfoArray[i]->GetTableName()) == 0)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::GetLink(int nIndex, CString& strPhysicalName, CString& strPublicName, int nTable) //aggiunto nTable
{
	if (nIndex >= 0 && nIndex <= m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(nTable)->GetUpperBound())
    {
		DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(nTable)->GetAt(nIndex);
        strPhysicalName = pObjLink->m_strPhysicalName;
        strPublicName = pObjLink->m_strPublicName; 
        return TRUE;
    }

    return FALSE;
}

//----------------------------------------------------------------------------
int TblRuleData::GetNumTableLinks(int nTable) const
{
	return m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(nTable)->GetSize();
}

//----------------------------------------------------------------------------
int TblRuleData::GetTotFieldLinks() const
{
	int tot = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetFieldLinkCount() + m_CalcColumnLinks.GetSize();
	return tot;
}

//----------------------------------------------------------------------------
int TblRuleData::GetArSqlTableInfoSize()
{
	return m_arSqlTableJoinInfoArray.GetSize();
}

//----------------------------------------------------------------------------
BOOL TblRuleData::ExistLink(LPCTSTR pszPhysicalName, LPCTSTR pszPublicName)
{
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
	    DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if  (
				(_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0) &&
				(_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
			)
			return TRUE;
	}
	
	if (m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 1)
		ASSERT (_tcschr(pszPhysicalName,DOT_CHAR)); 
	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

			if  (
					(_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0) &&
					(_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				)
				return TRUE;
		}
	}
    return FALSE;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::ExistPhysicalName(LPCTSTR pszPhysicalName)
{
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
	    DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if  (_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0)
				return TRUE;
	}

	//if (m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 1)
	//	ASSERT (_tcschr(pszPhysicalName,DOT_CHAR));

	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

			if  (_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0)
				return TRUE;
		}
	}
    return FALSE;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::ExistPublicName(LPCTSTR pszPublicName)
{
  for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
   {
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

			if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
        		return TRUE;
		}
   }
   for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
   {
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
        		return TRUE;
	 }
   return FALSE;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::ExistPublicNameInTableInfo(LPCTSTR pszPublicName, int idxTableInfo)
{
	if (m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() <= idxTableInfo || idxTableInfo < 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	DataFieldLinkArray* arDataFieldLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(idxTableInfo);

	for (int i = 0; i < arDataFieldLink->GetSize(); i++)
	{
		DataFieldLink*  pObjLink = arDataFieldLink->GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
    		return TRUE;
	}

	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
        		return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
CString	TblRuleData::GetPublicNameOf(LPCTSTR pszPhysicalName)
{
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	 {
		 DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0)
					return pObjLink->m_strPublicName;
	 }

	if (m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize() > 1)
		ASSERT (_tcschr(pszPhysicalName,DOT_CHAR));
	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	 {
			for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
			{
				DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

				if (_tcsicmp(pszPhysicalName, pObjLink->m_strPhysicalName) == 0)
					return pObjLink->m_strPublicName;
			}
	 }
    return CString("");
}

//----------------------------------------------------------------------------
CString	TblRuleData::GetPhysicalNameOf(LPCTSTR pszPublicName)
{
	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

			if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				return pObjLink->m_strPhysicalName;
		}
	}
	 for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	 {
		 DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				return pObjLink->m_strPhysicalName;
	 }
    return CString("");
}

//----------------------------------------------------------------------------
DataType TblRuleData::GetTypeOf(LPCTSTR pszPublicName)
{
	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);

			if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				return pObjLink->m_type;
		}
	}
	 for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	 {
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);
		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				return pObjLink->m_type;
	 }
	return DATA_NULL_TYPE;
}

//---------------------------------------------------------------------------

void TblRuleData::RenameField(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);
			if (_tcsicmp(pszOldName, pObjLink->m_strPublicName) == 0)
			{
				pObjLink->m_strPublicName = pszNewName;
			}
		}
	}
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszOldName, pObjLink->m_strPublicName) == 0)
		{
			pObjLink->m_strPublicName = pszNewName;
		}
	}
}

//----------------------------------------------------------------------------
BOOL TblRuleData::RemoveLink(LPCTSTR pszPublicName, BOOL bDeleteField)
{
	ProgramData* pPrgData = m_SymTable.GetDocument()->GetEditorManager()->GetPrgData();
	CString sLog;

	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->GetAt(i);
			if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
			{
				m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j)->RemoveAt(i);

				if (bDeleteField && pPrgData->CanDeleteField(pszPublicName, sLog))
				{
					pPrgData->DeleteField(pszPublicName);
				}
				else
				{
					WoormField* pF = GetSymTable()->GetField(pszPublicName);
					if (pF)
						pF->SetTableRuleField(FALSE);
				}

				return TRUE;
			}
		}
	}
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
		{
			m_CalcColumnLinks.RemoveAt(i);

			if (bDeleteField && pPrgData->CanDeleteField(pszPublicName, sLog))
			{
				pPrgData->DeleteField(pszPublicName);
			}
			else
			{
				WoormField* pF = GetSymTable()->GetField(pszPublicName);
				if (pF)
					pF->SetTableRuleField(FALSE);
			}

			return TRUE;
		}
	}
	return FALSE;
}
//----------------------------------------------------------------------------
void TblRuleData::RemoveAllLinks(BOOL bDeleteField)
{
	ProgramData* pPrgData = m_SymTable.GetDocument()->GetEditorManager()->GetPrgData();
	CString sLog;

	for (int i = m_CalcColumnLinks.GetUpperBound(); i >= 0; i--)
	{
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);
	
		CString sName = pObjLink->m_strPublicName;

		m_CalcColumnLinks.RemoveAt(i);
		
		if (bDeleteField && pPrgData->CanDeleteField(sName, sLog))
		{
			pPrgData->DeleteField(sName);
		}
		else
		{
			WoormField* pF = GetSymTable()->GetField(sName);
			if (pF)
				pF->SetTableRuleField(FALSE);
		}
	}

	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{
		DataFieldLinkArray* pLinks = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j);

		for (int i = pLinks->GetUpperBound(); i >= 0 ; i--)
		{
			DataFieldLink*  pObjLink = pLinks->GetAt(i);

			CString sName = pObjLink->m_strPublicName;

			pLinks->RemoveAt(i);

			if (bDeleteField && pPrgData->CanDeleteField(sName, sLog))
			{
				pPrgData->DeleteField(sName);
			}
			else
			{
				WoormField* pF = GetSymTable()->GetField(sName);
				if (pF)
					pF->SetTableRuleField(FALSE);
			}
		}
	}
	m_arSqlTableJoinInfoArray.m_arFieldLinks.RemoveAll();
}

//----------------------------------------------------------------------------
void TblRuleData::GetCalculatedColumns(CStringArray &arCalculatedColumns)
{
	arCalculatedColumns.RemoveAll();
	
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
    {
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);
       	arCalculatedColumns.Add(pObjLink->m_strPhysicalName);
	}
}

//----------------------------------------------------------------------------
DataFieldLink* TblRuleData::GetCalcColumn(LPCTSTR pszPublicName)
{
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{  
		DataFieldLink*  pObjLink = m_CalcColumnLinks.GetAt(i);

		if (_tcsicmp(pszPublicName, pObjLink->m_strPublicName) == 0)
				return pObjLink;
	}
   return NULL;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::IsNativeExpr(LPCTSTR pszPublicName)
{
	return GetCalcColumn(pszPublicName) != NULL;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::IsNativeQuery()
{
	if (m_pWhereClause && m_pWhereClause->IsNative())
		return TRUE;

	if (m_pHavingClause && m_pHavingClause->IsNative())
		return TRUE;

	if (m_CalcColumnLinks.GetSize())
		return TRUE;

	if (m_arSqlTableJoinInfoArray.GetSize() > 1)
	{
		for (int i = 1; i < m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
		{
			WClause* pW = m_arSqlTableJoinInfoArray.m_arJoinOn[i];
			if (pW && pW->IsNative())
				return TRUE;
		}
	}

	return FALSE;
}

//----------------------------------------------------------------------------
/*	Frammento pseudo-grammatica:
	From-Rule-Column ::= [ \{ <native-sql-expr> \} | <column-name> ] INTO <woorm-field> 
	From-Rule ::= 
		FROM [ <table-name> { , <table-name> } |
			   <table-name> [Inner Join | Left Outer Join  | Right Outer Join| Full Outer Join] <table-name> ON [NATIVE <native-sql-expr> | <wc-expr>] |
			   <table-name> CrossJoin <table-name>
			]
		SELECT [NULL | NOT NULL]] [DISTINCT] [TOP n]
		<From-Rule-Column> { , <From-Rule-Column> }
		[WHERE [NATIVE <native-sql-expr> | <wc-expr>]]
		[GROUP BY <column-list>]
		[HAVING [NATIVE <native-sql-expr | <wc-expr>]]
		[ORDER BY <column-list> ] 
		[WHEN <woorm-bool-expr> ] ;
*/
BOOL TblRuleData::Parse(Parser& parser, BOOL bCheckUnique, BOOL bAllowUnknownVar)
{
	ASSERT_VALID(m_pSqlConnection);

    CString     	strPhysicalName;
    CString     	strPublicName;
	CString			strDataTableName;

	//m_arFieldLinks.RemoveAll();
	m_arSqlTableJoinInfoArray.RemoveAll();
	
	m_SqlTest.Clear();

   if (!parser.ParseTag(T_FROM))			
		return FALSE;

	BOOL bNatural = TRUE;
	BOOL bContinue = TRUE;
	do
	{
		SqlTableJoinInfoArray::EJoinType eJoinType = SqlTableJoinInfoArray::EJoinType::CROSS;

		if (m_arSqlTableJoinInfoArray.GetSize() > 0 && !bNatural)
		{
			if (parser.Matched(T_INNER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::INNER;
			}
			else if (parser.Matched(T_CROSS) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::CROSS;
			}
			else if (parser.Matched(T_LEFT) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::LEFT_OUTER;
			}
			else if (parser.Matched(T_RIGHT) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::RIGHT_OUTER;
			}
			else if (parser.Matched(T_FULL) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::FULL_OUTER;
			}
			if (parser.ErrorFound())
				return FALSE;
		}
		//------------

		if (!parser.ParseSquaredIdent(strDataTableName))
			return FALSE;

		if (!m_pSqlConnection->ExistTable(strDataTableName))
		{
			parser.SetError(_TB("DataBase table unknown") + _T(" (") + strDataTableName + ')');
			return FALSE;
		}

		// attraverso il nome e la sua descrizione Xml scateno il caricamento delle DLL
		const CDbObjectDescription* pXmlDescri = AfxGetDbObjectDescription(strDataTableName);
		if (pXmlDescri)
			AfxGetTbCmdManager()->LoadNeededLibraries(pXmlDescri->GetNamespace());

		const SqlCatalogEntry* pCatalogEntry = m_pSqlConnection->GetCatalogEntry(strDataTableName);
		ASSERT_VALID(pCatalogEntry);
		if (pCatalogEntry)
		{
			SqlRecord* pRec = pCatalogEntry->CreateRecord();
			//ASSERT_VALID(pRec);
			if (pRec)
			{
				pRec->EnableExtraFiltering(TRUE);
				pRec->OnExtraFiltering(this->m_SqlTest.m_ExtraFilter);
				this->m_SqlTest.m_arContextBagRecords.Add(pRec);
			}
		}

		CString strAliasTableName;
		if (parser.Matched(T_ALIAS) && !parser.ParseID(strAliasTableName))
			return FALSE;

		SqlTableInfo* pSqlTableInfo = pCatalogEntry->m_pTableInfo;

		int pos = m_arSqlTableJoinInfoArray.Add(m_pSqlConnection, &m_SymTable, pSqlTableInfo, strAliasTableName, eJoinType);

		SqlRecord* pBindRec = new SqlRecord(strDataTableName, NULL, TABLE_TYPE, true);
		this->m_SqlTest.m_arBindRecords.Add(pBindRec);

		if (!this->m_SqlTest.m_pDataTable)
			this->m_SqlTest.m_pDataTable = new SqlTable(pBindRec, m_pSqlConnection->GetDefaultSqlSession());

		//----------------- 
		if (m_arSqlTableJoinInfoArray.GetSize() > 0)
		{
			WClause* pJoinOn = m_arSqlTableJoinInfoArray.m_arJoinOn[pos];
			pJoinOn->SetSqlTable(this->m_SqlTest.m_pDataTable);

			if (eJoinType != SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				if (!parser.Match(T_ON) || !pJoinOn->Parse(parser))
					return FALSE;
			}
		}

		//----------------- 
		bContinue = FALSE;
		if (bNatural && parser.Matched(T_COMMA))
		{
			bContinue = TRUE;
		}
		else
		{
			if (bNatural && m_arSqlTableJoinInfoArray.GetSize() > 1)
				break;

			bNatural = FALSE;
			bContinue = parser.LookAhead(T_INNER) || parser.LookAhead(T_LEFT) || parser.LookAhead(T_RIGHT) || parser.LookAhead(T_FULL) || parser.LookAhead(T_CROSS);
		}
	}
	while (bContinue);

	if (!parser.ParseTag(T_SELECT))
    	return FALSE;

   m_ConstraintMode = SEL_ALL;

    if (parser.LookAhead(T_NOT))
    {
    	parser.SkipToken();
    	if (!parser.ParseTag(T_NULL))
    		return FALSE;
    		
    	m_ConstraintMode = SEL_NOT_NULL;
    }  
    else
    {
    	if (!parser.Bad() && parser.LookAhead(T_NULL))
    	{
    		parser.SkipToken();
        	m_ConstraintMode = SEL_NULL;
        }
    	else
        	if (parser.Bad()) return FALSE;
	}

	m_bDistinct = parser.Matched(T_DISTINCT);
	if (parser.Matched(T_TOP))
	{
		if (!parser.ParseInt(m_nTop))
			return FALSE;
		if (m_nTop < 0)
			m_nTop = 0;
	}

    do
    {
        BOOL bNativeColumnExpr = FALSE;

		if (parser.LookAhead(T_BRACEOPEN))
		{
			if (!parser.ParseBracedContent(strPhysicalName))
				return FALSE;

			CString err;
			if (!::ParseCalculateColumn(strPhysicalName, &m_SymTable, err))
				return parser.SetError(err);

			bNativeColumnExpr = TRUE;
		}
		else if (!parser.ParseSquaredCoupleIdent(strPhysicalName))
				return FALSE;

		if (!parser.ParseTag(T_INTO))          return FALSE;
        if (!parser.ParseID(strPublicName))    return FALSE;

        WoormField* pRepField = m_SymTable.GetField(strPublicName);
        if (pRepField == NULL)
		{
			if (!bAllowUnknownVar)
				return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strPublicName);
		}
		else
		{
			if (pRepField->IsInput())
				return parser.SetError(_TB("Field with INPUT attribute is not allowed in rule context"), strPublicName);

			if (pRepField->IsExprRuleField())
				return parser.SetError(_TB("Field already used by another Expression Rule"), strPublicName);

			if (pRepField->IsTableRuleField() && bCheckUnique)
			{
				return parser.SetError(_TB("Field already used by another table rule"), strPublicName);
			}

			if (bNativeColumnExpr)
			{
				pRepField->SetNativeColumnExpr(TRUE);
			}
			else
			{
				// verifica di compatibilita` tra la colonna della data table e tipo
				// della variabile che si vuole associare
				const SqlColumnInfo* pColumnInfo = m_arSqlTableJoinInfoArray.GetColumnInfo(strPhysicalName);
				if (pColumnInfo == NULL)
        			return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strPhysicalName);

				CWordArray dataTypes;
				VERIFY(pColumnInfo->GetDataObjTypes(dataTypes));
				
				int i = 0;
				for (i = 0; i <= dataTypes.GetUpperBound(); i++)
					if (pRepField->GetDataType().m_wType == dataTypes[i])
						break;

				if (i > dataTypes.GetUpperBound())
					return parser.SetError(_TB("Illegal field type"), strPublicName);
			}
			pRepField->SetTableRuleField(TRUE);
		}

		if (AfxGetExportSymbols()->IsActivated())
		{
			for (int i = 0; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
				AfxGetExportSymbols()->AddColumnNameToItemOnLastFile(strPublicName, m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName() + _T(".") + strPhysicalName);
		}

        //qui devo smistarli tra le tabelle del join
		int nTable = 0;
        if (strPhysicalName.Find(DOT_CHAR) != -1 && !bNativeColumnExpr)
		{//cerco nome tabella in arraSqlInfo
			int curPos = 0;
			CString tableName = strPhysicalName.Tokenize(_T("."), curPos); 
			for (int i = 0; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
				if (tableName.Compare(m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName()) == 0)
				{  
					nTable = i;
					break;
				}
		}

		DataType typeField = DataType::Null;
		if (pRepField)
		{
			pRepField->SetPhysicalName(strPhysicalName);

 			CString strErr = pRepField->BindRuleItem(this->m_SqlTest.m_arBindRecords, m_arSqlTableJoinInfoArray);
			if (!strErr.IsEmpty())
				return parser.SetError(strErr, strPhysicalName);

			typeField = pRepField->GetDataType();
		}
		else 
		{
			ASSERT(bAllowUnknownVar);

			const SqlColumnInfo* pColumnInfo = m_arSqlTableJoinInfoArray.GetColumnInfo(strPhysicalName);
			if (pColumnInfo == NULL)
				return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strPhysicalName);

			CWordArray dataTypes;
			VERIFY(pColumnInfo->GetDataObjTypes(dataTypes));
			if (dataTypes.GetCount() == 0)
			{
				return parser.SetError(Expression::FormatMessage(Expression::CHECKTYPE), strPhysicalName);
			}

			typeField = dataTypes[0];
		}

		AddLink(strPhysicalName, strPublicName, bNativeColumnExpr, typeField, nTable);
  
	} while (parser.Matched(T_COMMA));

	if (parser.ErrorFound())
		return FALSE;

	if (m_arSqlTableJoinInfoArray.GetSize() > 1)
		m_arSqlTableJoinInfoArray.QualifiedLinks();

	ASSERT(this->m_SqlTest.m_arBindRecords.GetSize() == m_arSqlTableJoinInfoArray.GetSize());
	for (int i = 0; i < this->m_SqlTest.m_arBindRecords.GetSize(); i++)
	{
		CString strAlias = m_arSqlTableJoinInfoArray.m_arstrAliasTableName.GetAt(i);
		this->m_SqlTest.m_arBindRecords.GetAt(i)->SetQualifier(strAlias);
		this->m_SqlTest.m_pDataTable->FromTable(this->m_SqlTest.m_arBindRecords.GetAt(i), strAlias.IsEmpty() ? NULL : &strAlias);
	}

	if (parser.Matched(T_WHERE))
	{
		AddWhereClause();
	
		if (!m_pWhereClause->Parse(parser))
			return FALSE;
	}
 
	if 	(parser.Matched(T_GROUP))
    {
		if	(
				!parser.ParseTag(T_BY)	||
				!::ParseOrderBy(parser, &m_SymTable, &m_arSqlTableJoinInfoArray, m_strGroupBy)
			)
			return FALSE;
	}

	if (parser.Matched(T_HAVING))
	{
		AddHavingClause();
		
		if (!m_pHavingClause->Parse(parser))
			return FALSE;
	}

    if 	(parser.Matched(T_ORDER))
    {
		if	(
				!parser.ParseTag(T_BY)	||
				!::ParseOrderBy(parser, &m_SymTable, &m_arSqlTableJoinInfoArray, m_strOrderBy)
			)
			return FALSE;
	}

	if (parser.Matched(T_WHEN))
	{
		SAFE_DELETE(m_pWhenExpr);
		m_pWhenExpr = new Expression(&m_SymTable);
 	    m_pWhenExpr->SetStopTokens(T_SEP);
        if (!m_pWhenExpr->Parse(parser, DATA_BOOL_TYPE, TRUE))
			return FALSE;
    }

	if (parser.ErrorFound())	
		return FALSE;

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void TblRuleData::Unparse(Unparser& oFile)
{
	if (!oFile.IsMemoryFile())
	{
		if (this->GetTotFieldLinks() == 0)
		{
			return;
		}
	}

	oFile.UnparseTag(T_FROM, FALSE);

	m_arSqlTableJoinInfoArray.Unparse(oFile);

	oFile.UnparseTag	(T_SELECT, FALSE);

	switch (m_ConstraintMode)
	{
		case SEL_ALL :
			break;

		case SEL_NOT_NULL :
			oFile.UnparseBlank  ();
			oFile.UnparseTag	(T_NOT,	 FALSE);
			oFile.UnparseTag    (T_NULL, FALSE);
			break;

        case SEL_NULL :
			oFile.UnparseBlank  ();
			oFile.UnparseTag    (T_NULL, FALSE);
			break;
    }

	if (m_bDistinct)
	{
		oFile.UnparseBlank  ();
		oFile.UnparseTag (T_DISTINCT,	 FALSE);
	}
	if (m_nTop)
	{
		oFile.UnparseBlank  ();
		oFile.UnparseTag (T_TOP,	 FALSE);
		oFile.UnparseInt (m_nTop,	 FALSE);
	}

	oFile.UnparseCrLf();
    oFile.IncTab();
    
	BOOL bIsFirstCol = TRUE;

	for (int j = 0; j < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); j++)
	{ 	
		DataFieldLinkArray* pLinks = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(j);
		for (int i = 0; i < pLinks->GetSize(); i++)
		{
			DataFieldLink*  pObjLink = pLinks->GetAt(i);

			if (!bIsFirstCol)
				oFile.UnparseComma(TRUE);
			bIsFirstCol = FALSE;

			oFile.Write         (cwsprintf(_T("%-20s"), pObjLink->m_strPhysicalName), FALSE);

			oFile.UnparseBlank  ();
			oFile.UnparseTag    (T_INTO, FALSE);
			oFile.UnparseID     (pObjLink->m_strPublicName,	FALSE);
		}
	}

    //Unparsing calculated columns
	for (int i = 0; i < m_CalcColumnLinks.GetSize(); i++)
	{
		DataFieldLink*  pObjLink =  m_CalcColumnLinks.GetAt(i);

		pObjLink->m_strPhysicalName.Trim();
		if (pObjLink->m_strPhysicalName.IsEmpty())
			continue;

		if (!bIsFirstCol)
			oFile.UnparseComma(TRUE);
		bIsFirstCol = FALSE;

		oFile.Write         (cwsprintf(_T("{%s}"), pObjLink->m_strPhysicalName), FALSE);
			
		oFile.UnparseBlank  ();
		oFile.UnparseTag    (T_INTO, FALSE);
		oFile.UnparseID     (pObjLink->m_strPublicName,	FALSE);
	}

	if (m_pWhereClause && !m_pWhereClause->IsEmpty())
    {
        oFile.UnparseCrLf();
	    oFile.DecTab();
        oFile.UnparseTag    (T_WHERE,		FALSE);
		oFile.IncTab();

         oFile.UnparseExpr   (m_pWhereClause->ToString(),	FALSE);
    }
	oFile.DecTab();
    
	if (!m_strGroupBy.IsEmpty())
    {
        oFile.UnparseCrLf();
		oFile.DecTab();

        oFile.UnparseTag    (T_GROUP,	FALSE);
        oFile.UnparseTag    (T_BY,		FALSE);
        oFile.UnparseExpr   (m_strGroupBy,   FALSE);
    }

 	if (m_pHavingClause && !m_pHavingClause->IsEmpty())
	{
		oFile.UnparseCrLf();
		oFile.UnparseTag    (T_HAVING,		FALSE);

		oFile.UnparseExpr   (m_pHavingClause->ToString(),	FALSE);
	}

   if (!m_strOrderBy.IsEmpty())
    {
        oFile.UnparseCrLf();
        oFile.UnparseTag    (T_ORDER,	FALSE);
        oFile.UnparseTag    (T_BY,		FALSE);
        oFile.UnparseExpr   (m_strOrderBy,   FALSE);
    }

   	if (m_pWhenExpr && !m_pWhenExpr->IsEmpty())
    {
		oFile.UnparseCrLf	();
		oFile.UnparseTag    (T_WHEN,      	FALSE);
		oFile.UnparseExpr   (m_pWhenExpr->ToString(),	FALSE);
    }

	oFile.UnparseSep    (TRUE);
}

//----------------------------------------------------------------------------
CString TblRuleData::Unparse()
{
	Unparser buff(TRUE);
	Unparse(buff);
	buff.Close();
	CString sRule = buff.GetBufferString();

	return sRule;
}

//----------------------------------------------------------------------------
CString TblRuleData::ToNamedQuery() 
{
	ASSERT_VALID(m_SqlTest.m_pDataTable);
	if (!m_SqlTest.m_pDataTable) 
		return L"";

	CString s = ToNamedQuerySelect();

	CString sError;
	if (!BuildQuery(sError))
		return s;

	m_SqlTest.m_pDataTable->StripBlankNearSquareBrackets();
	if (!m_SqlTest.m_pDataTable->BuildQuery())
		return FALSE;

	CString query = m_SqlTest.m_pDataTable->ToString(TRUE, TRUE, FALSE);
	int idx = ::FindWord(query, L"From");
	if (idx > -1)
	{
		query = query.Mid(idx);
	}
	s += query;
	return s;
}

//-----------------------------------------------------------------------------
CString TblRuleData::ToNamedQuerySelect() 
{
	CString s (L"Select\n");
	BOOL numcol = 0;
	for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetCount(); i++)
	{
		DataFieldLinkArray* pLinks = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(i);

		for (int j = 0; j < pLinks->GetCount(); j++)
		{
			DataFieldLink* pLinkField = pLinks->GetAt(j);

			if (numcol++) s += ',';
			s += cwsprintf(L"%s {COL %s}\n", pLinkField->m_strPhysicalName, pLinkField->m_strPublicName);
		}
	}
	for (int k = 0; k < m_CalcColumnLinks.GetCount(); k++)
	{
		DataFieldLink* pLinkField = m_CalcColumnLinks.GetAt(k);

		if (numcol++) s += ',';
		s += cwsprintf(L"%s {IN %s}\n", pLinkField->m_strPhysicalName, pLinkField->m_strPublicName);
	}
	return s;
}

//-----------------------------------------------------------------------------
void TblRuleData::GetColumns(CStringArray& columns) const
{
	columns.RemoveAll();

	for (int i = 0; i < m_arSqlTableJoinInfoArray.m_arFieldLinks.GetCount(); i++)
	{
		DataFieldLinkArray* pLinks = m_arSqlTableJoinInfoArray.m_arFieldLinks.GetAt(i);

		for (int j = 0; j < pLinks->GetCount(); j++)
		{
			DataFieldLink* pLinkField = pLinks->GetAt(j);

			columns.Add(pLinkField->m_strPublicName);
		}
	}

	for (int k = 0; k < m_CalcColumnLinks.GetCount(); k++)
	{
		DataFieldLink* pLinkField = m_CalcColumnLinks.GetAt(k);

		columns.Add(pLinkField->m_strPublicName);
	}
}

//-----------------------------------------------------------------------------
void TblRuleData::GetParameters(CStringArray& parameters) const
{
	parameters.RemoveAll();

	for (int i = 1; i < this->m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
	{
		WClause* pJoinOn = m_arSqlTableJoinInfoArray.m_arJoinOn.GetAt(i);
		if (pJoinOn)
			pJoinOn->GetParameters(parameters);
	}

	if (m_pWhereClause)
		m_pWhereClause->GetParameters(parameters);
	if (m_pHavingClause)
		m_pHavingClause->GetParameters(parameters);
	//TODO verificare se serve
	//if (m_pWhenExpr)
	//	m_pWhenExpr->GetParameters(parameters);
}

//-----------------------------------------------------------------------------
BOOL TblRuleData::BuildQuery(CString& sErr)
{
	ASSERT_VALID(this->m_pSqlConnection);

	for (int i = 0; i < m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		CString sTableName = m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName();

		const SqlCatalogEntry* pCatalogEntry = this->m_pSqlConnection->GetCatalogEntry(sTableName);
		ASSERT_VALID(pCatalogEntry);
		if (!pCatalogEntry)
			continue;

		//OSL
		if (AfxGetSecurityInterface()->IsSecurityEnabled())
		{
			CInfoOSL oslInfo(*(const_cast<SqlCatalogEntry*>(pCatalogEntry)->GetInfoOSL()));
			AfxGetSecurityInterface()->GetObjectGrant(&oslInfo);
			if (!OSL_CAN_DO(&oslInfo, OSL_GRANT_EXECUTE))
			{
				sErr = EngineScheduler::FormatMessage(EngineScheduler::OSL_MISSING_GRANT) + _T("\n") + m_arSqlTableJoinInfoArray.Unparse();
				return FALSE;
			}
		}
	}
	//----

	TRY
	{
		if (m_bDistinct)
		m_SqlTest.m_pDataTable->AddSelectKeyword(SqlTable::DISTINCT);

		if (m_nTop > 0 && m_SqlTest.m_pDataTable->m_pSqlConnection->GetDBMSType() != DBMS_ORACLE)
			m_SqlTest.m_pDataTable->AddSelectKeyword(SqlTable::TOP, m_nTop);

		if (!m_strGroupBy.IsEmpty())
		{
			m_SqlTest.m_pDataTable->m_strGroupBy = m_strGroupBy;
			if (!ExpandContentOfClause(m_SqlTest.m_pDataTable->m_strGroupBy, GetSymTable(), m_SqlTest.m_pDataTable->m_pSqlConnection))
				return FALSE;
		}
		if (!m_strOrderBy.IsEmpty())
		{
			m_SqlTest.m_pDataTable->m_strSort = m_strOrderBy;
			if (!ExpandContentOfClause(m_SqlTest.m_pDataTable->m_strSort, GetSymTable(), m_SqlTest.m_pDataTable->m_pSqlConnection))
				return FALSE;
		}

		m_SqlTest.m_pDataTable->Attach(GetSymTable());

		BOOL bAppend = FALSE;
		for (int i = 1/*l'elemento 0 è sempre vuoto*/; i < m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
		{
			WClause* pJoinOnClause = m_arSqlTableJoinInfoArray.m_arJoinOn[i];
			if (pJoinOnClause)
			{
				if (!pJoinOnClause->PrepareQuery(bAppend, i))
				{
					sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_EVAL_JOINON_ERR) + L"\n" + 
						m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName() + L"\n" + pJoinOnClause->GetErrDescription();
					return FALSE;
				}
				bAppend = TRUE;
			}
		}

		if (m_pWhereClause == NULL && m_pHavingClause == NULL && !bAppend)
		{
			// Se la tabella non e` ancora aperta occorre aprirla
			if (!m_SqlTest.m_pDataTable->IsOpen())
			{
				TRY
				{
					m_SqlTest.m_pDataTable->Open(FALSE, E_FAST_FORWARD_ONLY);
					m_SqlTest.m_pDataTable->SelectFromAllTable(); //SelectAll();
				}
				CATCH(SqlException, e)
				{
					sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR)  + _T("\n") +
						m_arSqlTableJoinInfoArray.Unparse() + _T("\n") + e->m_strError;;
					return FALSE;
				}
				END_CATCH
			}
		}
		else
		{
			// PrepareQuery di WClause esegue anche l'apertura della tabella e la select di tutte
			// le colonne del SqlRecord correntemente associato alla SqlTable connessa alla WClause
			if (m_pWhereClause && !m_pWhereClause->PrepareQuery(bAppend))
			{
				if (m_pWhereClause->GetErrId() == Expression::EMPTY_MESSAGE)
					return FALSE;

				sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_EVAL_WHERE_ERR) + _T("\n") +
						 m_pWhereClause->GetErrDescription();
				return FALSE;
			}
		}

		// Add Context Bag Filters
		if (
			m_SymTable.GetDocument() &&
			!m_SymTable.GetDocument()->m_bSkipContext
			)
		{
			m_SqlTest.m_pDataTable->AddContextBagFilters(this->m_SqlTest.m_arContextBagRecords);
		}

		if (m_pHavingClause && !m_pHavingClause->PrepareQuery(bAppend || m_pWhereClause != NULL))
		{
			if (m_pHavingClause->GetErrId() == Expression::EMPTY_MESSAGE)
				return FALSE;

			sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_EVAL_HAVING_ERR) + _T("\n") +
					 m_pHavingClause->GetErrDescription();
			return FALSE;
		}

		// WoormRadar potrebbe impostare un filtro ausiliario derivato dalla ricerca QBE (F3)
		CWoormDoc* pWDoc = dynamic_cast<CWoormDoc*>(m_SymTable.GetDocument());
		if (pWDoc && !pWDoc->m_strDocumentFilter.IsEmpty())
		{
			CString strFilter, strOrderBy;
			CString strAux = pWDoc->m_strDocumentFilter;
			ASSERT(strAux.Left(5) == _T("FROM "));
			strAux = strAux.Mid(5);

			int nPos = strAux.Find(_T(" WHERE "));
			if (nPos < 0)
			{
				nPos = strAux.Find(_T(" ORDER BY "));
				if (nPos >= 0)
				{
					strOrderBy = strAux.Right(strAux.GetLength() - nPos - 10);
					strOrderBy.Trim();
				}
			}
			else
			{
				strFilter = strAux.Mid(nPos + 7);

				int nPosSort = strFilter.Find(_T(" ORDER BY "));
				if (nPosSort >= 0)
				{
					strOrderBy = strFilter.Right(strFilter.GetLength() - nPosSort - 10);
					strOrderBy.Trim();

					strFilter = strFilter.Left(nPosSort); strFilter.TrimRight();
				}
			}
			ASSERT(nPos >= 0);

			CString strTableName = strAux.Left(nPos); strTableName.TrimLeft(); strTableName.TrimRight();
			CString strWoormTable = m_SqlTest.m_pDataTable->GetAllTableName();
			if (strTableName.CompareNoCase(strWoormTable) == 0)
			{
				if (!strFilter.IsEmpty())
				{
					if (!m_SqlTest.m_pDataTable->m_strFilter.IsEmpty())
						m_SqlTest.m_pDataTable->m_strFilter = _T("(") + m_SqlTest.m_pDataTable->m_strFilter + _T(") AND ") + strFilter;
					else
						m_SqlTest.m_pDataTable->m_strFilter = strFilter;
				}
				if (!strOrderBy.IsEmpty())
				{
					if (m_SqlTest.m_pDataTable->m_strSort.IsEmpty())
						m_SqlTest.m_pDataTable->m_strSort = strOrderBy;
				}
			}
		}

		if (!this->m_SqlTest.m_ExtraFilter.IsEmpty() && m_SqlTest.m_pDataTable->m_strFilter.Find(this->m_SqlTest.m_ExtraFilter) == -1)
		{
			if (!m_SqlTest.m_pDataTable->m_strFilter.IsEmpty())
				m_SqlTest.m_pDataTable->m_strFilter = _T("(") + m_SqlTest.m_pDataTable->m_strFilter + _T(") AND ") + this->m_SqlTest.m_ExtraFilter;
			else
				m_SqlTest.m_pDataTable->m_strFilter = this->m_SqlTest.m_ExtraFilter;
		}

		//----
		m_SqlTest.m_pDataTable->StripBlankNearSquareBrackets();
	}
	CATCH(SqlException, e)
	{
		sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR) +
			m_arSqlTableJoinInfoArray.Unparse() + _T("\n") + e->m_strError;

		return FALSE;
	}
	END_CATCH
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL TblRuleData::ExecQuery(CString& sErr)
{
	ASSERT_VALID(this->m_pSqlConnection);

	if (!BuildQuery(sErr))
		return FALSE;

	TRY
	{
		m_SqlTest.m_pDataTable->Query();
	}
	CATCH(SqlException, e)
	{
		sErr = EngineScheduler::FormatMessage(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR) + 
			m_arSqlTableJoinInfoArray.Unparse() + _T("\n") + e->m_strError;

		return FALSE;
	}
	END_CATCH

	return TRUE;
}
//============================================================================
// ExpRuleData
//============================================================================
//----------------------------------------------------------------------------
ExpRuleData::ExpRuleData(WoormTable& aSymTable)
    :
    RuleDataObj	(aSymTable),
	m_pValidateWhereExpr(NULL),
    m_TestExpr	(&aSymTable),
    m_ThenExpr	(&aSymTable),
    m_ElseExpr	(&aSymTable) ,
	m_bDisableUniqueConstraint(FALSE)
{}

ExpRuleData::~ExpRuleData()
{
	SAFE_DELETE(m_pValidateWhereExpr);
}

//----------------------------------------------------------------------------
RuleType ExpRuleData::IsARule()
{
    return RULE_EXPR;
}

//----------------------------------------------------------------------------
CString ExpRuleData::GetPublicName()
{
    return m_strPublicName;
}

//-----------------------------------------------------------------------------
CString ExpRuleData::GetRuleDescription() const
{
	//CString sDescr = m_strPublicName + L" = ...";
	//if (m_TestExpr.IsEmpty())
	//	return sDescr;
	//if (m_ElseExpr.IsEmpty())
	//	return L"If-Then " + sDescr;
	//return L"If-Then-Else " + sDescr;
	return m_strPublicName;
}

//----------------------------------------------------------------------------
BOOL ExpRuleData::IsEmpty()
{
    return m_strPublicName.IsEmpty();
}

//----------------------------------------------------------------------------
void ExpRuleData::Empty()
{
	if (m_pValidateWhereExpr)
		m_pValidateWhereExpr->Reset(FALSE);	

	m_TestExpr.Reset(FALSE);				
	m_ThenExpr.Reset(FALSE);				
	m_ElseExpr.Reset(FALSE);				

	m_bDisableUniqueConstraint = FALSE;
	m_strPublicName.Empty();
}

//----------------------------------------------------------------------------
BOOL ExpRuleData::CanDeleteField(LPCTSTR pszFieldName) const
{
    return
    	!(
			m_TestExpr. 	HasMember(pszFieldName)  ||
			m_ThenExpr.		HasMember(pszFieldName)  ||
			m_ElseExpr.		HasMember(pszFieldName)  ||
			(m_pValidateWhereExpr && m_pValidateWhereExpr->HasMember(pszFieldName))
		);
}

//----------------------------------------------------------------------------
void ExpRuleData::DeleteField(LPCTSTR pszFieldName)
{
    if (m_strPublicName.CompareNoCase(pszFieldName) == 0)
    {
        // this mean to set empty the Expression Rule
        // so it can be deleted after
        m_strPublicName.Empty();
    }
}

//----------------------------------------------------------------------------
void ExpRuleData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	if (m_strPublicName.CompareNoCase(pszOldFieldName) == 0)
	{
		// this mean to set empty the Expression Rule
		// so it can be deleted after
		m_strPublicName = pszNewFieldName;
	}
}

//----------------------------------------------------------------------------
BOOL ExpRuleData::ExistPublicName(LPCTSTR pszName)
{
    return (m_strPublicName.CompareNoCase(pszName) == 0);
}

//----------------------------------------------------------------------------
BOOL ExpRuleData::ParseId(Parser& lex)
{
	if (m_bDisableUniqueConstraint && !m_strPublicName.IsEmpty())
	{
		//if (lex.LookAhead(T_ID))
		//{
		//	CString sId;
		//	if (!lex.ParseID(sId))
		//		return FALSE;

		//	if (!lex.ParseTag(T_ASSIGN))
		//		return FALSE;

		//	if (sId.CompareNoCase(m_strPublicName))
		//	{
		//		return lex.SetError(_TB("Expected expression field named:") + ' ' + m_strPublicName, _TB("Instead found field named:") + ' ' + sId);
		//	}
		//}
	}
	else
	{
		if (m_strPublicName.IsEmpty())
		{
			if (!lex.ParseID(m_strPublicName))
				return FALSE;
		}
		else
		{
			CString sId;
			if (!lex.ParseID(sId))
				return FALSE;

			if (sId.CompareNoCase(m_strPublicName))
			{
				return lex.SetError(_TB("Expected expression field named:") + ' ' + m_strPublicName, _TB("Instead found field named:") + ' ' + sId);
			}
		}

		if (!lex.ParseTag(T_ASSIGN))
			return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
// ATTENZIONE: se si modifica modificare anche in EXPDLG.CPP
//----------------------------------------------------------------------------
BOOL ExpRuleData::Parse(Parser& lex, BOOL bParseWhereClause /*= TRUE*/)
{
    CString strTmp;
	BOOL	bParseSep = TRUE;

    if (lex.Matched(T_IF))
    {
  	    m_TestExpr.SetStopTokens(T_THEN);
        if (!m_TestExpr.Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;

        if (!lex.ParseTag(T_THEN))      	
			return FALSE;

		if (!ParseId(lex))
			return FALSE;

        WoormField* pRepField = m_SymTable.GetField(m_strPublicName);
        if (pRepField == NULL)
			return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), m_strPublicName);

        if (pRepField->IsInput())
			return lex.SetError(_TB("Request field are not allowed. The field cannot has INPUT attribute"), m_strPublicName);

        if (pRepField->IsTableRuleField() || pRepField->IsExprRuleField())
		{
			if (!m_bDisableUniqueConstraint)
				return lex.SetError(_TB("Field already used by another rule"), m_strPublicName);
		}
        pRepField->SetExprRuleField(TRUE);

 		m_ThenExpr.SetStopTokens(T_ELSE, T_WHERE, T_SEP);
        if (!m_ThenExpr.Parse(lex, pRepField->GetDataType(), TRUE))
			return FALSE;

		bParseSep = !lex.Matched(T_SEP);

        if (lex.Matched(T_ELSE))
        {
			if (!ParseId(lex))
				return FALSE;

			m_ElseExpr.SetStopTokens(T_WHERE, T_SEP);
			if (!m_ElseExpr.Parse(lex, pRepField->GetDataType(), TRUE))
				return FALSE;

			bParseSep = !lex.Matched(T_SEP);
		}
    }
    else
    {
		if (!ParseId(lex))
			return FALSE;

        WoormField* pRepField = m_SymTable.GetField(m_strPublicName);
        if (pRepField == NULL)
        	return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), m_strPublicName);

        if (pRepField->IsInput())
            return lex.SetError(_TB("Request field are not allowed. The field cannot has INPUT attribute"), m_strPublicName);

		if (pRepField->IsTableRuleField() || pRepField->IsExprRuleField())
		{
			if (!m_bDisableUniqueConstraint)
				return lex.SetError(_TB("Field already used by another rule"), m_strPublicName);
		}
		pRepField->SetExprRuleField(TRUE);

		m_ThenExpr.SetStopTokens(T_WHERE, T_SEP);
        if (!m_ThenExpr.Parse(lex, pRepField->GetDataType(), TRUE))
            return FALSE;

		bParseSep = !lex.Matched(T_SEP);
    }

	if (bParseWhereClause && lex.Matched(T_WHERE))
    {
		SAFE_DELETE(m_pValidateWhereExpr);
		m_pValidateWhereExpr = new Expression(GetSymTable());
 		if (!m_pValidateWhereExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
            return FALSE;

		bParseSep = TRUE;
    }

	if (!bParseSep)
		return TRUE;

	if (m_bDisableUniqueConstraint)
	{
		lex.Matched(T_SEP);
		return TRUE;
	}	
    return lex.ParseSep();
}

//----------------------------------------------------------------------------
void ExpRuleData::Unparse(Unparser& oFile, BOOL bUnparseWhereClause /*= TRUE*/, BOOL bUnparseId)
{
    if (m_TestExpr.IsEmpty())
    {
		if (bUnparseId)
		{
			oFile.UnparseID     (m_strPublicName,	FALSE);
			oFile.UnparseTag    (T_ASSIGN,      	FALSE);
		}
        oFile.UnparseExpr   (m_ThenExpr.ToString(),		FALSE);
    }
    else
    {
        oFile.UnparseTag    (T_IF,			FALSE);
        oFile.UnparseExpr   (m_TestExpr.ToString(),	TRUE);

        oFile.IncTab();
        oFile.UnparseTag    (T_THEN,			FALSE);
		if (bUnparseId)
		{
			oFile.UnparseID(m_strPublicName, FALSE);
			oFile.UnparseTag(T_ASSIGN, FALSE);
		}
		oFile.UnparseExpr   (m_ThenExpr.ToString(),		FALSE);

        if (!m_ElseExpr.IsEmpty())
        {
            oFile.UnparseCrLf   ();
            oFile.UnparseTag    (T_ELSE,			FALSE);
			if (bUnparseId)
			{
				oFile.UnparseID(m_strPublicName, FALSE);
				oFile.UnparseTag(T_ASSIGN, FALSE);
			}
			oFile.UnparseExpr   (m_ElseExpr.ToString(),		FALSE);
        }

        oFile.DecTab();
    }

	if (bUnparseWhereClause && m_pValidateWhereExpr && !m_pValidateWhereExpr->IsEmpty())
    {
        oFile.UnparseCrLf   ();
        oFile.UnparseTag    (T_WHERE,	FALSE);
		oFile.UnparseExpr	(m_pValidateWhereExpr->ToString(), FALSE);
    }

	if (bUnparseId)
		oFile.UnparseSep(TRUE);
}

//----------------------------------------------------------------------------
CString ExpRuleData::Unparse(BOOL bUnparseWhereClause /*= TRUE*/, BOOL bUnparseId /*= TRUE*/)
{
	Unparser buff(TRUE);
	Unparse(buff, bUnparseWhereClause, bUnparseId);
	buff.Close();

	CString sRule = buff.GetBufferString();
	return sRule;
}

//============================================================================
// WhileRuleData
//============================================================================
//----------------------------------------------------------------------------
WhileRuleData::WhileRuleData(WoormTable& aSymTable)
    :
    RuleDataObj	(aSymTable),
    m_pCondExpr	(new Expression (&aSymTable)), 
    m_pBefore	(new Block (NULL, &aSymTable, NULL)),
    m_pAfter	(new Block (NULL, &aSymTable, NULL)),
	m_pBody		(new Block (NULL, &aSymTable, NULL))
{
	m_pBefore->SetForceBeginEnd();
	m_pBody->SetForceBeginEnd();
	m_pAfter->SetForceBeginEnd();
}

WhileRuleData::~WhileRuleData()
{
	SAFE_DELETE(m_pCondExpr);
	SAFE_DELETE(m_pBefore);
	SAFE_DELETE(m_pAfter);
	SAFE_DELETE(m_pBody);
}

//----------------------------------------------------------------------------
RuleType WhileRuleData::IsARule()
{
    return RULE_LOOP_WHILE;
}

//-----------------------------------------------------------------------------
CString WhileRuleData::GetRuleDescription() const
{
	//return L"For-While-Do ...";
	return L"Repeat";
}

//----------------------------------------------------------------------------
CString WhileRuleData::GetPublicName()
{
    return _T("");
}

//----------------------------------------------------------------------------
BOOL WhileRuleData::IsEmpty()
{
    return m_pCondExpr->IsEmpty() && m_pBefore->IsEmpty() && m_pAfter->IsEmpty() && m_pBody->IsEmpty();
}

//----------------------------------------------------------------------------
BOOL WhileRuleData::CanDeleteField(LPCTSTR pszFieldName) const
{
    return	
		!m_pCondExpr->HasMember(pszFieldName) &&
		!m_pBefore->HasMember(pszFieldName) &&
		!m_pAfter->HasMember(pszFieldName) &&
		!m_pBody->HasMember(pszFieldName);
}

//----------------------------------------------------------------------------
void WhileRuleData::DeleteField(LPCTSTR pszFieldName)
{
}

//----------------------------------------------------------------------------
BOOL WhileRuleData::ExistPublicName(LPCTSTR sName)
{
    return CStringArray_Find(m_arWSelFieldName, sName) > -1;
}

//----------------------------------------------------------------------------
void WhileRuleData::SetRuleFields(BOOL setRuleField)
{
	for (int i = 0; i < m_arWSelFieldName.GetSize(); i++)
	{
		WoormField* pF = GetSymTable()->GetField(m_arWSelFieldName.GetAt(i));
		if (pF)
			pF->SetTableRuleField(setRuleField);
	}
}
//----------------------------------------------------------------------------
BOOL WhileRuleData::Parse(Parser& parser)
{
	m_arWSelFieldName.RemoveAll();
	CStringArray* pOld = m_SymTable.TraceFieldsModified(&this->m_arWSelFieldName);

	if (parser.Matched(T_FOR))
	{
		m_pBefore->Parse(parser);
	}
		
	if (parser.Match(T_WHILE))
	{
		m_pCondExpr->SetStopTokens(T_DO);
		if (!m_pCondExpr->Parse(parser, DATA_BOOL_TYPE, TRUE))
		{
			this->m_SymTable.TraceFieldsModified(pOld);
			return FALSE;
		}
	}

	if (parser.Match(T_DO))
	{
		m_pBody->Parse(parser);
	}

	if (parser.Matched(T_CONTINUE))
	{
		m_pAfter->Parse(parser);
	}

   parser.Matched(T_SEP);

   if (pOld && m_arWSelFieldName.GetSize())
	   pOld->Append(m_arWSelFieldName);
   this->m_SymTable.TraceFieldsModified(pOld);

   SetRuleFields(TRUE);

   return !parser.ErrorFound();
}

//----------------------------------------------------------------------------
void WhileRuleData::Unparse(Unparser& oFile)
{
    if (!m_pCondExpr->IsEmpty())
    {
		if (!m_pBefore->IsEmpty())
		{
			oFile.UnparseTag (T_FOR,  TRUE);
			m_pBefore->Unparse(oFile);
		}

        oFile.UnparseTag    (T_WHILE,      	FALSE);
        oFile.UnparseExpr   (m_pCondExpr->ToString(),	FALSE);
        oFile.UnparseTag    (T_DO,      	TRUE);

	    m_pBody->Unparse(oFile);

		if (!m_pAfter->IsEmpty())
		{
			oFile.UnparseTag (T_CONTINUE,  TRUE);
			m_pAfter->Unparse(oFile);
		}

 		//oFile.UnparseSep	(TRUE);
    }
}

//============================================================================
// QueryRuleData
//============================================================================
//----------------------------------------------------------------------------
QueryRuleData::QueryRuleData(WoormTable& aSymTable)
    :
    RuleDataObj	(aSymTable),
    m_pWhenExpr	(NULL),
	m_pObjQuery	(NULL)
{}

//----------------------------------------------------------------------------
QueryObjItem*	QueryRuleData::GetQueryItem() 
{ 
	ASSERT_VALID(m_pObjQuery); 
	return m_pObjQuery; 
}

//----------------------------------------------------------------------------
void QueryRuleData::SetQueryItem(QueryObjItem* pQ)
{
	m_pObjQuery = pQ; 
	if (pQ) 
	{ 
		ASSERT_VALID(pQ); 
		m_strQueryName = pQ->GetName(); 

		SetRuleFields(TRUE);
	}
}

//----------------------------------------------------------------------------
RuleType QueryRuleData::IsARule()
{
    return RULE_NAMED_QUERY;
}

//-----------------------------------------------------------------------------
CString QueryRuleData::GetRuleDescription() const
{
	ASSERT_VALID(m_pObjQuery);
	return L"Query " + m_strQueryName;
}

//----------------------------------------------------------------------------
CString QueryRuleData::GetPublicName()
{
	ASSERT_VALID(m_pObjQuery);
	return m_strQueryName;
}

//----------------------------------------------------------------------------
BOOL QueryRuleData::IsEmpty()
{
	if (m_strQueryName.IsEmpty() && m_pObjQuery == NULL)
		return TRUE;
    return FALSE;
}

//----------------------------------------------------------------------------
BOOL QueryRuleData::CanDeleteField(LPCTSTR pszFieldName) const
{
	ASSERT_VALID(m_pObjQuery);
	ASSERT_VALID(m_pObjQuery->GetQueryObject());

	if (
			m_pObjQuery->GetQueryObject()->HasMember(pszFieldName) 
			|| 
			m_pObjQuery->GetQueryObject()->HasColumn(pszFieldName)
			||
			(m_pWhenExpr && m_pWhenExpr->HasMember(pszFieldName))
		)
		return FALSE;

   return TRUE;
}

//----------------------------------------------------------------------------
void QueryRuleData::RenameField(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	ASSERT_VALID(m_pObjQuery);
	ASSERT_VALID(m_pObjQuery->GetQueryObject());

	m_pObjQuery->GetQueryObject()->RenameField(pszOldName, pszNewName);
}


//----------------------------------------------------------------------------
void QueryRuleData::DeleteField(LPCTSTR pszName)
{
	ASSERT_VALID(m_pObjQuery);
	ASSERT_VALID(m_pObjQuery->GetQueryObject());

	m_pObjQuery->GetQueryObject()->DeleteField(pszName);
}

//----------------------------------------------------------------------------
BOOL QueryRuleData::ExistPublicName(LPCTSTR pszFieldName)
{
	ASSERT_VALID(m_pObjQuery);
	ASSERT_VALID(m_pObjQuery->GetQueryObject());

	return m_pObjQuery && m_pObjQuery->GetQueryObject()->HasColumn(pszFieldName);
}

//----------------------------------------------------------------------------
BOOL QueryRuleData::Parse(Parser& lex)
{
    if (!lex.ParseTag(T_QUERY) || !lex.ParseID(m_strQueryName))
		return FALSE;

	m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	if (lex.Matched(T_NOT) && lex.ParseTag(T_NULL))
		m_ConstraintMode = RuleSelectionMode::SEL_NOT_NULL;
	else
		if (!lex.ErrorFound() && lex.Matched(T_NULL))
			m_ConstraintMode = RuleSelectionMode::SEL_NULL;
		else
			if (lex.ErrorFound())
				return FALSE;

    if (lex.Matched(T_WHEN))
	{
		SAFE_DELETE(m_pWhenExpr);
		m_pWhenExpr = new Expression(&m_SymTable);
 	    m_pWhenExpr->SetStopTokens(T_SEP);
        if (!m_pWhenExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;
    }

	lex.Match(T_SEP);
	return !lex.ErrorFound();
}

//----------------------------------------------------------------------------
void QueryRuleData::Unparse(Unparser& oFile)
{
	oFile.UnparseTag (T_QUERY, FALSE);
	oFile.UnparseID	(m_strQueryName, FALSE);

	switch (m_ConstraintMode)
	{
	case SEL_ALL:
		break;

	case SEL_NOT_NULL:
		oFile.UnparseBlank();
		oFile.UnparseTag(T_NOT, FALSE);
		oFile.UnparseTag(T_NULL, FALSE);
		break;

	case SEL_NULL:
		oFile.UnparseBlank();
		oFile.UnparseTag(T_NULL, FALSE);
		break;
	}

	if (m_pWhenExpr && !m_pWhenExpr->IsEmpty())
    {
		oFile.UnparseCrLf	();
		oFile.UnparseTag    (T_WHEN,      	FALSE);
		oFile.UnparseExpr   (m_pWhenExpr->ToString(),	FALSE);
    }
	oFile.UnparseTag    (T_SEP,      	TRUE);
}

//----------------------------------------------------------------------------
void QueryRuleData::SetRuleFields(BOOL setRuleField)
{
	QueryObject* pQ = NULL;
	ASSERT_VALID(GetQueryItem());

	if (GetQueryItem() && (pQ = GetQueryItem()->GetQueryObject()))
	{
		ASSERT_VALID(pQ);
		for (int i = 0; i < pQ->AllQueryColumns().GetSize(); i++)
		{
			WoormField* pF = GetSymTable()->GetField(pQ->AllQueryColumns().GetAt(i));
			if (pF)
				pF->SetTableRuleField(setRuleField);
		}
	}
}
//=============================================================================



