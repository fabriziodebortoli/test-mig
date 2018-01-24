#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

#include "beginh.dex"


enum TB_EXPORT SqlFetchResult { FetchOk, EndOfRowSet, BeginOfRowSet, Error };
enum TB_EXPORT SqlParamType { NoParam, Input, InputOutput, Output, ReturnValue };

//===========================================================================
class TB_EXPORT SqlBindObject : public CObject
{
	DECLARE_DYNAMIC(SqlBindObject)

public:
	DataObj* m_pDataObj;
	DataObj* m_pOldDataObj;
	CString m_strBindName;
	CString m_strParamName; // se necessario deve essere anteposto il simbolo @ al nome del parametro passato dal programmatore (in genere P1, P2...)
	SqlParamType m_eParamType;
	bool	m_bAutoIncrement;
	bool	m_bOwnData;			//se TRUE il dataobj é stato creato al volo e va cancellato nel distruttore altrimenti
								// é un dataobj appartenente ad un altro oggetto

	bool		m_bUpdatable;		//se il campo é presente nella clause di set della query di update nel caso di keyedupdate
	bool		m_bReadOnly;
	CString		m_sLocalName;		//nome del local per i campi calcolati (Es: SqlTable::SelectSqlFunc)

protected:
	int		m_nSqlRecIdx;

public:
	SqlBindObject(const CString& strBindName, DataObj* pDataObj, SqlParamType eParamType = NoParam);
	~SqlBindObject();
};





//===========================================================================
class TB_EXPORT SqlBindObjectArray : public ::Array
{
	DECLARE_DYNAMIC(SqlBindObjectArray)
};


#include "endh.dex"
