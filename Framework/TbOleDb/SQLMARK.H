
#pragma once 

#include <TbGeneric\DataObj.h>
#include "sqlrec.h"

class SqlConnection;

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//								SqlDBMark
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT SqlDBMark : public SqlRecord
{
	friend class SqlRecord;
	
	DECLARE_DYNCREATE(SqlDBMark) 
	
public:
	DataStr		f_Signature;
	DataStr		f_Module;	
	DataInt		f_DBRelease;
	DataDate	f_ReleaseDate;	
	DataBool	f_Status;	
	DataBool	f_ExecLevel3;
	DataInt		f_Level;		
	DataInt		f_UpgradeStep;

public:
	SqlDBMark(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord();	

public:
	static  LPCTSTR  GetStaticName();
};

#include "endh.dex"