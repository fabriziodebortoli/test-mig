
#include "stdafx.h" 

#include <TbGenlib\messages.h>
#include <TbGeneric\dataobj.h>

#include "sqlmark.h"
#include "oledbmng.h"
#include "sqltable.h"
#include "lentbl.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//////////////////////////////////////////////////////////////////////////////
//								SqlDBMark
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
// Table Name
static const TCHAR szDBMarkTableName[]	= _T("TB_DBMark");

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SqlDBMark, SqlRecord) 

//-----------------------------------------------------------------------------
SqlDBMark::SqlDBMark(BOOL bCallInit)
	:
	SqlRecord(szDBMarkTableName),
	f_Status	(TRUE),
	f_ExecLevel3(FALSE)
{
	f_Signature.SetUpperCase();
	f_Module.	SetUpperCase();

	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void SqlDBMark::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_DATA	(_T("Application"),		f_Signature)
		BIND_DATA	(_T("AddOnModule"),		f_Module)
		BIND_DATA	(_T("DBRelease"),		f_DBRelease)
		BIND_DATA	(_T("ReleaseDate"),		f_ReleaseDate)
		BIND_DATA	(_T("Status"),			f_Status)
		BIND_DATA	(_T("ExecLevel3"),		f_ExecLevel3)
		BIND_DATA	(_T("UpgradeLevel"),	f_Level)
		BIND_DATA	(_T("Step"),			f_UpgradeStep)
	END_BIND_DATA()	 
}
	
//-----------------------------------------------------------------------------
LPCTSTR SqlDBMark::GetStaticName() { return szDBMarkTableName; }

//=============================================================================