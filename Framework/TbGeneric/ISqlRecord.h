#pragma once
#include "Array.h"
//includere alla fine degli include del .H
#include "beginh.dex"
//==============================================================================
class DataObj;
class SqlRecord;

class TB_EXPORT ISqlRecord: public NamedDataObjArray
{
	DECLARE_DYNAMIC(ISqlRecord)
public:
	//virtual ISqlRecord*	GetISqlRecord	() = 0;
	virtual	SqlRecord*	GetSqlRecord	() = 0;

	virtual	ISqlRecord*	IClone			() = 0;
	virtual void		Dispose			() = 0;
	virtual void		Assign			(ISqlRecord*) = 0;

	virtual void		Init			() = 0;

	virtual const CString& GetTableName () const = 0;

	virtual BOOL		IIsEqual		(const ISqlRecord&)	const = 0;

	virtual int			GetIndexFromDataObj			(const DataObj* pDataObj) const = 0;
	virtual	DataObj* 	GetDataObjAt				(int nIndex) const = 0;
	virtual	DataObj*	GetDataObjFromColumnName	(const CString&) = 0;	

	virtual CString		ToString			() const = 0;
};

#include "endh.dex"