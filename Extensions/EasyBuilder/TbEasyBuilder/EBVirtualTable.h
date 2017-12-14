#pragma once

#include <TbOleDb\SqlRec.h>

class TEBVirtualTable : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(TEBVirtualTable)
public:
	TEBVirtualTable(void);
	static  LPCTSTR  GetStaticName();
};

