#include "StdAfx.h"

using namespace System::ComponentModel;
using namespace System::ComponentModel::Design;

#include <TbOleDb\Sqltable.h>
#include <TbOleDb\WClause.h>

#include "QueryController.h"

using namespace System;


IMPLEMENT_DYNAMIC(CQueryController, CObject)
//-----------------------------------------------------------------------------
CQueryController::CQueryController(
		SqlTable* pTable,
		SymTable* pSymTable,
		SqlTableInfoArray*	pSqlTableArray 
	)
	:
	m_pTable(pTable), 
	m_pSymTable(pSymTable),
	m_pSqlTableArray(pSqlTableArray)
{
}


//-----------------------------------------------------------------------------
CQueryController::~CQueryController(void)
{
}

//-----------------------------------------------------------------------------
bool CQueryController::OnCheckEdtWhereClause (const CString& strExpression, CString& strError)
{
	WClause* pClause = CreateValidWhereClause(strExpression, strError);
	bool valid = pClause != NULL;
	delete pClause;
	return valid;
}

//-----------------------------------------------------------------------------
WClause* CQueryController::CreateValidWhereClause(CString strWhereClause, CString& strError)
{
	WClause* pWhereClause = new WClause(m_pTable->m_pSqlConnection, m_pSymTable, m_pSqlTableArray ? *m_pSqlTableArray : NULL, m_pTable);
	pWhereClause->SetNative(FALSE);
		
	Parser lex(strWhereClause);
	if (!pWhereClause->Parse(lex))
	{
		delete pWhereClause;
		strError = lex.GetError();
		lex.ClearError();
		return NULL;
	}
	return pWhereClause;
}