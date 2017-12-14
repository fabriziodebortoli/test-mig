
#include "StdAfx.h"

#include <tboledb\SqlCatalog.h>
#include <tboledb\SqlTable.h>
#include <tboledb\SqlObject.h>
#include <tboledb\OleDbMng.h>

#include "soapfunctions.h"
#include "AuditTables.h"
#include "AuditingManager.h"


//funzione diretta
//----------------------------------------------------------------------------
bool SetAuditManagerFunction(CString tableName, CAuditingMngObj** pAuditMng)
{
	if (AfxGetDefaultSqlConnection() == NULL || AfxGetAuditingSqlConnection() == NULL)
		return FALSE;

	
	CString strAuditTable = szAUDIT + tableName;
	if (!AfxGetAuditingSqlConnection()->ExistTable(strAuditTable))
		return TRUE;

	//AuditingManager** p = (AuditingManager**)(long)auditingManagerPointer;
	if (*pAuditMng != NULL)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bTraced = FALSE;

	// verifico se la tabella é sotto tracciatura andando a leggere la tabella 
	// AUDIT_Tables e verificando o meno la presenza di tableName
	TAuditTables aRec;
	SqlTable aTable(&aRec, AfxGetAuditingSqlConnection()->GetDefaultSqlSession());
	TRY
	{
		aTable.Open(FALSE, E_FAST_FORWARD_ONLY);
		aTable.Select(aRec.f_TableName);
		aTable.m_strFilter = (cwsprintf(_T(" %s = '%s' AND %s = %s"),
										(LPCTSTR)aRec.GetColumnName(&aRec.f_TableName),
										tableName,
										(LPCTSTR)aRec.GetColumnName(&aRec.f_Suspended),
										AfxGetDefaultSqlConnection()->NativeConvert(&DataBool(FALSE))
								));
		aTable.Query();
		bTraced = !aTable.IsEOF();
		aTable.Close();	
	}
	
	CATCH(SqlException, e)
	{
		aTable.m_pSqlSession->ShowMessage(e->m_strError);
		if (aTable.IsOpen())
			aTable.Close();
		return FALSE;					
	}
	END_CATCH

	if (!bTraced)
		return TRUE;
	
	*pAuditMng = new AuditingManager(tableName.GetString());
	
	return TRUE;		

}

// funzionalitá di start e stop del sistema di auditing
//----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool OpenAuditing() 
{
	return AfxGetAuditingInterface()->OpenAuditing();
}

//----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool CloseAuditing() 
{
	return AfxGetAuditingInterface()->CloseAuditing();
}



