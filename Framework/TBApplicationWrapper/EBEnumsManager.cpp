#include "StdAfx.h"
#include "EBEnumsManager.h"

//-----------------------------------------------------------------------------
CEBEnumsManager::CEBEnumsManager(void)
{}

//-----------------------------------------------------------------------------
CEBEnumsManager::~CEBEnumsManager(void)
{}

//-----------------------------------------------------------------------------
void CEBEnumsManager::DeleteTag(EnumTag* enumTag, EnumsTablePtr pEnumsTable)
{
	pEnumsTable->m_pEnumTags->DeleteTag(enumTag->GetTagName());
}
