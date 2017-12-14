
#include "stdafx.h"

#include <TbParser\Parser.h>
#include <TBGenlib\baseapp.h>

#include "rpsymtbl.h"
#include "RepTable.h"
#include "procdata.h"
#include "ExportSymbols.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// class ObjItem                                                         	
//============================================================================
CString ObjItem::Unparse()
{
	Unparser buff(TRUE);
	Unparse(buff, TRUE, TRUE);
	buff.Close();
	CString s = buff.GetBufferString();
	return s;
}

//============================================================================
// class ProcedureObjItem                                                         	
//============================================================================
IMPLEMENT_DYNAMIC(ProcedureObjItem, ObjItem)

//----------------------------------------------------------------------------
ProcedureObjItem::ProcedureObjItem(WoormTable *pSymTable, const CString& sName/* = _T("")*/)
	:
	m_pProcedure	(new Procedure(sName, pSymTable))
{}

//----------------------------------------------------------------------------
ProcedureObjItem::ProcedureObjItem(RepEngine* pOwnRepEngine, const CString& sName/* = _T("")*/)
	:
	m_pProcedure	(new Procedure(sName, NULL, pOwnRepEngine))
{}

//----------------------------------------------------------------------------
ProcedureObjItem::~ProcedureObjItem()
{ 
	if (m_pProcedure)
		ASSERT_VALID(m_pProcedure);
	SAFE_DELETE(m_pProcedure); 
}

//----------------------------------------------------------------------------
BOOL ProcedureObjItem::CanDeleteField(LPCTSTR pszFieldName)
{
	return m_pProcedure->m_pBlock->CanDeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ProcedureObjItem::DeleteField(LPCTSTR pszFieldName)
{
	m_pProcedure->m_pBlock->DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ProcedureObjItem::DeleteTable(LPCTSTR pszDispTableName)
{
	m_pProcedure->m_pBlock->DeleteTable(pszDispTableName);
}

//----------------------------------------------------------------------------
BOOL ProcedureObjItem::IsEmpty() const
{
	return m_pProcedure->m_pBlock->IsEmpty();
}

//----------------------------------------------------------------------------
void ProcedureObjItem::DispTableChanged(LPCTSTR pszOldTableName, LPCTSTR pszNewTableName)
{
	m_pProcedure->m_pBlock->DispTableChanged(pszOldTableName, pszNewTableName);
}

//----------------------------------------------------------------------------
CString ProcedureObjItem::GetName() const
{
	return m_pProcedure->GetName();
}

//----------------------------------------------------------------------------
void ProcedureObjItem::SetName(LPCTSTR pszName)
{
	m_pProcedure->SetName(pszName);
}

//----------------------------------------------------------------------------
SymTable* ProcedureObjItem::GetSymTable() const
{
	ASSERT_VALID(m_pProcedure);
	ASSERT_VALID(m_pProcedure->GetSymTable());

	return m_pProcedure->GetSymTable()->GetRoot();
}

//----------------------------------------------------------------------------
BOOL ProcedureObjItem::Parse(Parser& lex)
{
	lex.GetCommentTrace(this->m_arCommentTraceBefore);

	BOOL bOk = m_pProcedure->Parse(lex);

	lex.GetCommentTrace(this->m_arCommentTraceAfter);
	return bOk;
}

//----------------------------------------------------------------------------
void ProcedureObjItem::Unparse(Unparser& oFile, BOOL bSkipHeader/* = FALSE*/, BOOL bSkipBeginEnd /*= FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	m_pProcedure->Unparse(oFile, bSkipHeader, bSkipBeginEnd);

	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class QueryObjItem implementation
//===========================================================================
IMPLEMENT_DYNAMIC(QueryObjItem, ObjItem)

QueryObjItem::Arraym_arAlias_TBLockable QueryObjItem::m_arAlias;

//---------------------------------------------------------------------------
QueryObjItem::QueryObjItem
	(
		WoormTable*	pSymTable, 
		const CString& sName,
		CTBContext* pTBContext
	)
	:
	m_Query(sName, pSymTable, pTBContext ? pTBContext->GetReadOnlySqlSession() : NULL)
{
}

//----------------------------------------------------------------------------
SymTable* QueryObjItem::GetSymTable() const
{
	return m_Query.GetSymTable();
}

//----------------------------------------------------------------------------
BOOL QueryObjItem::CanDeleteField(LPCTSTR pszFieldName)
{
	return !m_Query.HasMember(pszFieldName);
}

//----------------------------------------------------------------------------
BOOL QueryObjItem::Parse (Parser& p)
{ 
	m_sError.Empty();

	p.GetCommentTrace(this->m_arCommentTraceBefore);

	BOOL bOk = m_Query.Parse(p);
	if (!bOk)
	{
		p.SetError(m_Query.GetError());
		m_sError = p.GetError();
	}

	p.GetCommentTrace(this->m_arCommentTraceAfter);
	return bOk;
}

//----------------------------------------------------------------------------
BOOL QueryObjItem::Parse(const CString& sQuery)
{
	m_sError.Empty();

	CString sQry = L"Begin\n{\n" + sQuery + L"\n}\nEnd";
	Parser	lex(sQry);

	BOOL bOk = m_Query.Parse(lex);
	if (!bOk)
	{
		lex.SetError(m_Query.GetError());
		m_sError = lex.GetError();
		return FALSE;
	}

	return TRUE;
}


//----------------------------------------------------------------------------
void QueryObjItem::Unparse(Unparser& oFile, BOOL bSkipHeader/* = FALSE*/, BOOL bSkipBeginEnd /*= FALSE*/)
{ 
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	m_Query.Unparse(oFile, bSkipHeader, bSkipBeginEnd);
	
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//---------------------------------------------------------------------------
/* static */ void QueryObjItem::AddAlias(LPCTSTR name, LPCTSTR sNs)
{
	CFunctionDescription* pFuncPrototype = new CFunctionDescription();

	CTBNamespace ns(CTBNamespace::FUNCTION, sNs);

	BOOL bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns, *pFuncPrototype, FALSE);

	if (bOk)
		m_arAlias.Add(new Alias(name, pFuncPrototype));
}

/* static */ void QueryObjItem::InitCallQuery()
{
	AddAlias(_T("Open")				, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryOpen"));
	AddAlias(_T("Close")			, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryClose"));
	AddAlias(_T("Read")				, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryRead"));
	AddAlias(_T("ReadOne")			, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryReadOne"));
	AddAlias(_T("Execute")			, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryExecute"));
	AddAlias(_T("IsOpen")			, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryIsOpen"));
	AddAlias(_T("SetConnection")	, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QuerySetConnection"));
	AddAlias(_T("SetCursorType")	, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QuerySetCursorType"));
	AddAlias(_T("Call")				, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.QueryCall"));
}

//---------------------------------------------------------------------------
/* static */ BOOL QueryObjItem::ResolveCallQuery(CString sFuncName, CFunctionDescription& aMethod)
{
	BEGIN_TB_OBJECT_LOCK(&m_arAlias)

		if (m_arAlias.GetSize() == 0)
			InitCallQuery();

	END_TB_OBJECT_LOCK()

	for (int i = 0; i < m_arAlias.GetSize(); i++)
	{
		Alias* pA = (Alias*) m_arAlias.GetAt(i);
		if (pA->m_strAlias.CompareNoCase(sFuncName) == 0)
		{
			aMethod = *(pA->m_pFD);
			return TRUE;
		}
	}
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void QueryObjItem::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\nSymField = ", GetName());
}

void QueryObjItem::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//============================================================================
// ObjectsData                                                           	//
//============================================================================
//----------------------------------------------------------------------------
ObjectsData::ObjectsData(WoormTable& aSymTable, Array& ar)
	:
	m_SymTable	(aSymTable),
	m_Objects	(ar)
{}

//----------------------------------------------------------------------------
BOOL ObjectsData::CanDeleteField(LPCTSTR pszFieldName) const
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];

		if (!pObj->CanDeleteField(pszFieldName))
        	return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL ObjectsData::Exist(LPCTSTR strName)
{
    return Get(strName) != NULL;
}

//----------------------------------------------------------------------------
int ObjectsData::GetIndex(LPCTSTR strName)
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];

		if (_tcsicmp (strName, pObj->GetName()) == 0)
        	return i;
	}

    return -1;
}

//----------------------------------------------------------------------------
CString ObjectsData::GetName (int Idx)
{
	return ((ObjItem *) m_Objects[Idx])->GetName();
}

//----------------------------------------------------------------------------
ObjItem* ObjectsData::Get (LPCTSTR strName)
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];

		if (_tcsicmp (strName, pObj->GetName()) == 0)
        	return pObj;
	}

    return NULL;
}

//----------------------------------------------------------------------------
void ObjectsData::Rename (LPCTSTR oldName, LPCTSTR newName)
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];

		if (!_tcsicmp (oldName, pObj->GetName()))
		{
        	pObj->SetName (newName);
        	break;
        }
	}
}

//----------------------------------------------------------------------------
void ObjectsData::Delete (LPCTSTR name)
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];

		if (!_tcsicmp (name, pObj->GetName()))
		{
			m_Objects.RemoveAt(i);
	       	break;
        }
	}
}

//----------------------------------------------------------------------------
void ObjectsData::Unparse(Unparser& oFile, Token tk)
{
	if (m_Objects.GetSize() == 0)
		return;

    oFile.UnparseCrLf();
    oFile.UnparseTag(tk);
	oFile.UnparseBegin();

	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ObjItem* pObj = (ObjItem*) m_Objects[i];
		pObj->Unparse (oFile);
	}

	oFile.UnparseComment(this->m_arCommentTraceInner);

	oFile.UnparseEnd();
}

//============================================================================
// ProcedureData                                                           	//
//============================================================================

IMPLEMENT_DYNAMIC(ProcedureData, CObject)

void ProcedureData::Unparse(Unparser& oFile)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);
	__super::Unparse(oFile, T_PROCEDURES);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
BOOL ProcedureData::Parse(Parser& parser)
{
	parser.GetCommentTrace(this->m_arCommentTraceBefore);

	BOOL bHaveProcedures = parser.Matched(T_PROCEDURES);
	if (bHaveProcedures && !parser.Match(T_BEGIN)) return FALSE;

	CString		strProcName;
	ProcedureObjItem*  pProcObj;
	while (parser.Matched(T_PROCEDURE))
	{
		if (!parser.ParseID(strProcName)) return FALSE;

		pProcObj = m_SymTable.FindProcedure(strProcName);
		if (pProcObj == NULL)
		{
			pProcObj = new ProcedureObjItem(&m_SymTable, strProcName);

			m_SymTable.AddProcedure(pProcObj);
		}
		else
			if (!pProcObj->IsEmpty())
				return parser.SetError(_TB("Procedure already defined"), strProcName);

		if (!pProcObj->Parse(parser)) 
			return FALSE;

		if (AfxGetExportSymbols()->IsActivated())
			AfxGetExportSymbols()->AddItemOnLastFile(pProcObj->GetName(), EXP_SYMB_PROCEDURE);
	}

	parser.GetCommentTrace(this->m_arCommentTraceInner);

	if (bHaveProcedures && !parser.Match(T_END)) return FALSE;

	parser.GetCommentTrace(this->m_arCommentTraceAfter);

	return !parser.Bad();
}

//----------------------------------------------------------------------------
void ProcedureData::DeleteField(LPCTSTR pszFieldName)
{
	CWordArray	idxToDelete;
	int			nIdx;

	for (nIdx = 0; nIdx < m_Objects.GetSize(); nIdx++)
	{
		ProcedureObjItem* pObj = (ProcedureObjItem*) m_Objects[nIdx];

        pObj->DeleteField(pszFieldName);
		if (pObj->IsEmpty())
			idxToDelete.Add((WORD)nIdx);
	}

	for (nIdx = idxToDelete.GetUpperBound(); nIdx >= 0; nIdx--)
		m_Objects.RemoveAt(idxToDelete[ nIdx ]);
}

//----------------------------------------------------------------------------
void ProcedureData::DeleteTable(LPCTSTR pszDispTableName)
{
	CWordArray	idxToDelete;
	int			nIdx;

	for (nIdx = 0; nIdx < m_Objects.GetSize(); nIdx++)
	{
		ProcedureObjItem* pProcObj = (ProcedureObjItem*) m_Objects[nIdx];

        pProcObj->DeleteTable(pszDispTableName);
		if (pProcObj->IsEmpty())
			idxToDelete.Add((WORD)nIdx);
	}

	for (nIdx = idxToDelete.GetUpperBound(); nIdx >= 0; nIdx--)
		m_Objects.RemoveAt(idxToDelete[ nIdx ]);
}

//----------------------------------------------------------------------------
void ProcedureData::DispTableChanged(LPCTSTR pszOldTableName, LPCTSTR pszNewTableName)
{
	for (int i = 0; i < m_Objects.GetSize(); i++)
	{
		ProcedureObjItem* pObj = (ProcedureObjItem*) m_Objects[i];

		pObj->DispTableChanged(pszOldTableName, pszNewTableName);
	}
}

//============================================================================
// QueryObjectData                                                           	
//============================================================================
IMPLEMENT_DYNAMIC(QueryObjectData, CObject)

void QueryObjectData::Unparse(Unparser& oFile)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);
	__super::Unparse(oFile, T_QUERIES);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
BOOL QueryObjectData::Parse(Parser& lex)
{
	lex.GetCommentTrace(this->m_arCommentTraceBefore);

	BOOL bHaveQueries = lex.Matched(T_QUERIES);
	if (bHaveQueries && !lex.Match(T_BEGIN)) return FALSE;
	while (lex.LookAhead(T_QUERY) && !lex.Bad())
	{
		QueryObjItem* pObj = new QueryObjItem(&m_SymTable, L"");

		if (!pObj->Parse(lex))
		{
			delete pObj;
            return FALSE;
		}

		if (Exist(pObj->GetName()))
		{
			lex.SetError(_TB("Query already defined"), pObj->GetName());
			delete pObj;
            return FALSE;
		}
		
		m_SymTable.AddQuery(pObj);
	}

	lex.GetCommentTrace(this->m_arCommentTraceInner);

	if (bHaveQueries && !lex.Match(T_END)) return FALSE;
		
	lex.GetCommentTrace(this->m_arCommentTraceAfter);

	return !lex.Bad();
}

