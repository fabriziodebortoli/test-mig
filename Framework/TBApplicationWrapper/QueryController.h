#pragma once


class SqlTable;
class WClause;
class CQueryController : public CObject
{
	DECLARE_DYNAMIC(CQueryController)
	SqlTable* m_pTable;
	SymTable* m_pSymTable;
	SqlTableInfoArray*	m_pSqlTableArray; 
public:
	CQueryController(
		SqlTable* pTable,
		SymTable* pSymTable,
		SqlTableInfoArray*	pSqlTableArray
	);
	virtual ~CQueryController(void);

	bool OnCheckEdtWhereClause	(const CString& strExpression, CString& strError);

	WClause* CreateValidWhereClause(CString strWhereClause, CString& strError);
};
