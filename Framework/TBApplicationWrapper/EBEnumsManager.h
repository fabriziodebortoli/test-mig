#pragma once

//=============================================================================  
#include <TbGeneric\EnumsTable.h>

//Helper class per la cancellazione di EnumTag.
//È friend di EnumsTable in EnumsTable.h
//=============================================================================
private class CEBEnumsManager
{
public:
	CEBEnumsManager(void);
	~CEBEnumsManager(void);

	void DeleteTag(EnumTag* enumTag, EnumsTablePtr pEnumsTable);
};

