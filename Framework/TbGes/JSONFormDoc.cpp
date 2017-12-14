#include "stdafx.h"
#include "JSONFormDoc.h"
#include "DynDBT.h"

IMPLEMENT_DYNCREATE(CJSONFormDoc, CDynamicFormDoc)


//=========================================================================
class JSONSqlRecord : public SqlRecord
{
	DECLARE_DYNCREATE(JSONSqlRecord)

public:
	JSONSqlRecord();
	JSONSqlRecord(LPCTSTR szTableName, SqlConnection* pConn = NULL, short nType = TABLE_TYPE);

	virtual void	BindDynamicDeclarations(int& nStartPos){}
	static JSONSqlRecord* CreateRecord(CJsonParser& parser);
private:
	void BindDynamic();
	void OnCreated(SqlRecord* pRec) const;
};


//=========================================================================
IMPLEMENT_DYNCREATE(JSONSqlRecord, SqlRecord)

//-----------------------------------------------------------------------------
JSONSqlRecord::JSONSqlRecord()
:
SqlRecord()
{

}
//-----------------------------------------------------------------------------
void JSONSqlRecord::OnCreated(SqlRecord* pRec) const
{
	for (int i = 0; i < GetCount(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		BOOL bVirtual = !pItem->GetColumnInfo() || pItem->GetColumnInfo()->m_bVirtual;
		int nLength = pItem->m_lLength;
		CString sName = pItem->GetColumnName();
		if (sName.Compare(CREATED_COL_NAME) == 0 || sName.Compare(MODIFIED_COL_NAME) == 0 || sName.Compare(CREATED_ID_COL_NAME) == 0 || sName.Compare(MODIFIED_ID_COL_NAME) == 0)
			continue;
		if (bVirtual)
			pRec->BindLocalDataObj(i, sName, *pItem->GetDataObj()->Clone(), nLength);
		else
			pRec->BindDataObj(i, sName, *pItem->GetDataObj()->Clone());
	}
	
	((JSONSqlRecord*)pRec)->BindDynamic();
}
//-----------------------------------------------------------------------------
JSONSqlRecord::JSONSqlRecord(LPCTSTR szTableName, SqlConnection* pConn /*NULL*/, short nType /*TABLE_TYPE*/)
	:
	SqlRecord(szTableName, pConn, nType)
{

}
//-----------------------------------------------------------------------------
void JSONSqlRecord::BindDynamic()
{
	int nPos = GetSize();
	EndBindData(nPos);
}
//-----------------------------------------------------------------------------
JSONSqlRecord* JSONSqlRecord::CreateRecord(CJsonParser& parser)
{
	CString sTable = parser.ReadString(_T("table"));
	if (sTable.IsEmpty())
		return NULL;
	if (!parser.BeginReadArray(_T("fields")))
		return NULL;
	JSONSqlRecord *pRecord = new JSONSqlRecord(sTable);
	pRecord->m_bBindingDynamically = true;
	for (int i = 0; i < parser.GetCount(); i++)
	{
		if (!parser.BeginReadObject(i))
			return NULL;
		int nColType = parser.ReadInt(_T("type"));
		int nLength = parser.ReadInt(_T("length"));
		CString sName = parser.ReadString(_T("name"));
		DataType dataType = parser.ReadInt(_T("dataType"));
		switch (nColType)
		{
		case CDbFieldDescription::Column:
			pRecord->BindDataObj(i, sName, *DataObj::DataObjCreate(dataType));
			break;
		case CDbFieldDescription::Variable:
			pRecord->BindLocalDataObj(i, sName, *DataObj::DataObjCreate(dataType), nLength);
			break;
			// case CDbFieldDescription::Parameter:
			// store procedure parameters binding cannot be called in this method
			// as now I cannot call virtual methods. See SqlRecordProcedure::BindDynamicParameters
		default:
			break;
		}

		parser.EndReadObject();
	
	}
	pRecord->m_bBindingDynamically = false;
	parser.EndReadArray();
	pRecord->BindDynamic();
	return pRecord;
}

CJSONFormDoc::CJSONFormDoc()
{
}


CJSONFormDoc::~CJSONFormDoc()
{
}

BOOL CJSONFormDoc::SetupQuery(CJsonParser& parser, CPredicateContainer* pPredicateContainer)
{
	if (!pPredicateContainer)
		return FALSE;
	
	if (!parser.BeginReadArray(_T("where")))
		return FALSE;

	for (int i = 0; i < parser.GetCount(); i++)
	{
		if (!parser.BeginReadObject(i))
			return FALSE;
		ParamType type = (ParamType)parser.ReadInt(_T("type"));
		CString sField = parser.ReadString(_T("field"));
		CString sValue = parser.ReadString(_T("value"));
		CString sOperator = parser.ReadString(_T("op"));
		switch (type)
		{
		case FOREIGN_KEY:
		{
			pPredicateContainer->AddForeignKey(sField, sValue);
			break;
		}
		case FIELD:
		{
			pPredicateContainer->AddFieldPredicate(sField, sValue, sOperator);
			break;
		}
		case CONSTANT:
		{
			pPredicateContainer->AddValuePredicate(sField, sValue, sOperator);
			break;
		}
		}
		parser.EndReadObject();
	}
	parser.EndReadArray();

	return TRUE;
}


DBTObject* CJSONFormDoc::CreateDBT(CJsonParser& parser, CPredicateContainer*& pPredicateContainer)
{
	CString sDTBName = parser.ReadString(_T("name"));
	if (sDTBName.IsEmpty())
		return NULL;

	JSONSqlRecord *pRecord = JSONSqlRecord::CreateRecord(parser);
	if (!pRecord)
		return NULL;
	DBTType type = (DBTType)parser.ReadInt(_T("type"));

	DBTObject* pDBT = NULL;
	switch (type)
	{
	case DBTType::MASTER:
		pDBT = new DynDBTMaster(pRecord, this, sDTBName);
		pPredicateContainer = ((DynDBTMaster*)pDBT)->GetQuery();

		if (parser.BeginReadObject(_T("browserQuery")))
		{
			if (!SetupQuery(parser, ((DynDBTMaster*)pDBT)->GetBrowserQuery()))
			{
				delete pDBT;
				return NULL;
			}
			parser.EndReadObject();
		}
		break;
	case DBTType::SLAVE:
		pDBT = new DynDBTSlave(pRecord, this, sDTBName, ALLOW_EMPTY_BODY);
		pPredicateContainer = ((DynDBTSlaveBuffered*)pDBT)->GetQuery();

		break;
	case DBTType::SLAVE_BUFFERED:
		pDBT = new DynDBTSlaveBuffered(pRecord, this, sDTBName, ALLOW_EMPTY_BODY, !CHECK_DUPLICATE_KEY);
		pPredicateContainer = ((DynDBTSlaveBuffered*)pDBT)->GetQuery();
		break;
	}
	if (parser.BeginReadObject(_T("query")))
	{
		if (!SetupQuery(parser, pPredicateContainer))
		{
			delete pDBT;
			return NULL;
		}
		parser.EndReadObject();
	}

	return pDBT;

}
BOOL CJSONFormDoc::OnAttachData()
{
	
	LPCTSTR lpszJSON = (LPCTSTR)m_pDocInvocationInfo->m_pAuxInfo;

	if (!lpszJSON)
		return FALSE;

	CJsonFormParser parser;
	if (!parser.ReadJsonFromString(lpszJSON))
		return FALSE;

	CString sTitle = parser.ReadString(_T("title"));
	SetFormTitle(sTitle);
	if (!parser.BeginReadObject(_T("master")))
		return FALSE;
	CPredicateContainer* pContainer = NULL;

	DBTObject* pDBT = CreateDBT(parser, pContainer);
	if (!pDBT || !pDBT->IsKindOf(RUNTIME_CLASS(DBTMaster)))
		return FALSE;
	
	DBTMaster* pMaster = (DBTMaster*)pDBT;
	
	
	if (!parser.BeginReadArray(_T("dbts")))
		return FALSE;
	for (int i = 0; i < parser.GetCount(); i++)
	{
		if (!parser.BeginReadObject(i))
			return FALSE;
		DBTObject* pDBT = CreateDBT(parser, pContainer);
		if (!pDBT || !pDBT->IsKindOf(RUNTIME_CLASS(DBTSlave)))
			return FALSE;

		DBTSlave* pSlave = (DBTSlave*)pDBT;
		
		
		pMaster->Attach(pSlave);
		parser.EndReadObject();

	}
	parser.EndReadArray();
	parser.EndReadObject();
	return Attach(pMaster);
}