#include "StdAfx.h"
#include "EBVirtualTable.h"

static const TCHAR szTableName	[]	=	_T("EB_LocalData");

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TEBVirtualTable, SqlVirtualRecord)

TEBVirtualTable::TEBVirtualTable(void)
	: SqlVirtualRecord(szTableName)
{
}


LPCTSTR  TEBVirtualTable::GetStaticName()
{
	return szTableName;
}