#pragma once 

#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TbGes\ExtDocAbstract.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>

#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//             class CBrowserByTBGuid declaration
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CBrowserByTBGuid : public CObject
{
	DECLARE_DYNAMIC(CBrowserByTBGuid)

public:
	CBrowserByTBGuid(CAbstractFormDoc* pDoc, SqlSession* pSqlSession);

private:
	CAbstractFormDoc*	m_pDocument;
	SqlSession*			m_pSqlSession;

public:
	BOOL BrowseOnRecord(DataGuid idTBGuid);
};
