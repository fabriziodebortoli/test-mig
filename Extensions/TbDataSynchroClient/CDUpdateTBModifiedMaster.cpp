#include "stdafx.h" 

#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\Sqltable.h>
#include "CDUpdateTBModifiedMaster.h"

static TCHAR szParamProvider[]	= _T("P1");
static TCHAR szParamTBGuid[]	= _T("P2");

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDUpdateTBModifiedMaster, CClientDoc)

//-----------------------------------------------------------------------------
void CDUpdateTBModifiedMaster::OnDuringBatchExecute(SqlRecord* pCurrentRecord)
{	
	if(!pCurrentRecord)
		return;
	
	DataStr SLAVE_TABLE,SLAVE_COLUMN,MASTER_TABLE,MASTER_COLUMN;
	CString m_strFromCmd =  _T(" sys.foreign_key_columns fkc INNER JOIN sys.objects obj  ON obj.object_id = fkc.constraint_object_id");
			m_strFromCmd += _T(" INNER JOIN sys.tables tab1  ON tab1.object_id = fkc.parent_object_id");
			m_strFromCmd += _T(" INNER JOIN sys.schemas sch  ON tab1.schema_id = sch.schema_id");
			m_strFromCmd += _T(" INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id");
			m_strFromCmd += _T(" INNER JOIN sys.tables tab2  ON tab2.object_id = fkc.referenced_object_id");
			m_strFromCmd += _T(" INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id");
			

	CString	m_strWhereCmd = cwsprintf((_T(" tab1.name ='%s'")),pCurrentRecord->GetTableName().GetString());

	SqlTable aTable(GetServerDoc()->GetReadOnlySqlSession());	
	aTable.Select(_T("tab1.name "),&SLAVE_TABLE,100);
	aTable.Select(_T("col1.name "),&SLAVE_COLUMN,100);
	aTable.Select(_T("tab2.name "),&MASTER_TABLE,100);
	aTable.Select(_T("col2.name "),&MASTER_COLUMN,100);

	SqlTable aTableUpdate(GetServerDoc()->GetUpdatableSqlSession());		

	TRY
	{	
		aTable.Open();
		aTable.m_strFrom  = m_strFromCmd;
		aTable.m_strFilter += m_strWhereCmd;
		aTable.Query();
		
		CString SQLWhere;
		CString SQLUpdate;

		if (!aTable.IsEOF())
			 SQLUpdate = cwsprintf(_T("UPDATE %s set TBModified = GETDATE() where "),  MASTER_TABLE.Str());
		else
		{
			if (aTable.IsOpen())
				aTable.Close();
			return;
		}
		int i= 0;

		while (!aTable.IsEOF())
		{	
			if (i > 0)
				SQLWhere += _T(" AND ");


			DataObj* tmp = pCurrentRecord->GetDataObjFromColumnName(SLAVE_COLUMN);

			if (tmp->IsKindOf(RUNTIME_CLASS(DataStr)))
				SQLWhere += cwsprintf(_T(" %s = '%s'"), MASTER_COLUMN.Str(), pCurrentRecord->GetDataObjFromColumnName(SLAVE_COLUMN)->Str());
			else
				if ((tmp->IsKindOf(RUNTIME_CLASS(DataEnum))))
					SQLWhere += cwsprintf(_T(" %s = %s"), MASTER_COLUMN.Str(), pCurrentRecord->GetDataObjFromColumnName(SLAVE_COLUMN)->FormatDataForXML());	
				else
					SQLWhere += cwsprintf(_T(" %s = %d"), MASTER_COLUMN.Str(), pCurrentRecord->GetDataObjFromColumnName(SLAVE_COLUMN));	

			i++;
			aTable.MoveNext();

		}

		TRY
		{	
			aTableUpdate.Open(TRUE);
			aTableUpdate.ExecuteQuery(SQLUpdate + SQLWhere);

		}

		CATCH(SqlException, e)
		{
			if (aTableUpdate.IsOpen())
				aTableUpdate.Close();
			if (aTable.IsOpen())
				aTable.Close();
			ASSERT(FALSE);
		}
		
		END_CATCH
	
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		ASSERT(FALSE);
	}
	END_CATCH	
	

	if (aTableUpdate.IsOpen())
		aTableUpdate.Close();
	if (aTable.IsOpen())
		aTable.Close();
	
}
