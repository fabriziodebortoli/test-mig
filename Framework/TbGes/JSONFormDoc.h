#pragma once
#include "EXTDOC.H"
class JSONSqlRecord;
class CPredicateContainer;

enum DBTType { MASTER = 1, SLAVE = 2, SLAVE_BUFFERED = 3 };
enum ParamType { PLAIN = 0, FIELD = 1, CONSTANT = 2, FOREIGN_KEY=3 };
class CJSONFormDoc : public CDynamicFormDoc
{
	DECLARE_DYNCREATE(CJSONFormDoc)
public:
	CJSONFormDoc();
	~CJSONFormDoc();
	BOOL OnAttachData();
private:
	DBTObject* CreateDBT(CJsonParser& parser, CPredicateContainer*& pPredicateContainer);
	BOOL SetupQuery(CJsonParser& parser, CPredicateContainer* pPredicateContainer);
	
};

