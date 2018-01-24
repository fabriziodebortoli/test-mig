
#pragma once

#include <TbGeneric\DataObj.h>

#include <TbDatabaseManaged\SqlBindingObjects.h>


//includere alla fine degli include del .H
#include "beginh.dex"

class CBindingBlob;

// contiene le classi necessarie per la gestione delle colonne di risultato (Bind columns)
// e per la gestione dei paramentri (Bind params)
// La classe SqlBindingElem serve per descrivere una singola colonna su cui effettuare
// il bind oppure un parametro.
// Il tipo dell'elemento specifica questa differenza e può essere:
//	DBPARAMIO_NOTPARAM : se è una colonna di bind
//	DBPARAMIO_INPUT: se è un parametro di input
//	DBPARAMIO_OUTPUT: se è un parametro di output
//	DBPARAMIO_INPUT | DBPARAMIO_OUTPUT: se è un parametro di input/output 
//	

extern const TB_EXPORT int nEmptySqlRecIdx;

/////////////////////////////////////////////////////////////////////////////
//						class SqlBindingElem definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlBindingElem : public SqlBindObject
{
	friend class SqlAccessor;
	friend class SqlBindingElemArray;
	friend class SqlTable;
	friend class AuditingManager;
	friend class SqlTableStruct;

	DECLARE_DYNAMIC(SqlBindingElem)

public:
	SqlBindingElem
	(
		const CString&		strBindName,
		DataObj*			pDataObj,
		SqlParamType		eParamType = NoParam,
		const int&			nSqlRecIdx = - 1
	) :
		SqlBindObject(strBindName, pDataObj, eParamType)
	{
		m_nSqlRecIdx = nSqlRecIdx;
	}


public:
	DataObj*			GetDataObj() const { return m_pDataObj; };
	DataObj*			GetOldDataObj() const { return m_pOldDataObj; };
	CString				GetBindName(BOOL bQualified = FALSE) const;

	void				GetParamValue(DataObj* pDataObj) const { pDataObj->Assign(*m_pDataObj); }
	const	CString&	GetLocalName() const { return m_sLocalName; }

	void				SetLocalName(const	CString& strLocalName) { m_sLocalName = strLocalName; }

	void				SetReadOnly(bool bSet = true) { m_bReadOnly = bSet; }
	bool				IsReadOnly() { return m_bReadOnly; }
	void				SetUpdatable(bool bSet = true) { m_bUpdatable = bSet; }
	bool				IsUpdatable() { return m_bUpdatable; }

	bool				SameValue()	const { return m_pOldDataObj->IsEqual(*m_pDataObj) == TRUE; }

	DataType			GetDataType() const;
	void				AssignOldDataObj(const DataObj& aDataObj);
	void				ClearOldDataObj();
	void				SetParamValue(const DataObj& aDataObj);

	void Init();

public:
	virtual		int GetIndex() const { return m_nSqlRecIdx; }

};
//---------------------------------------------------------------------------

class TB_EXPORT SqlBindingElem2 : public SqlBindingElem
{
public:
	int m_nIdx;

	SqlBindingElem2
	(
		int nIdx,
		const CString&		strBindName,
		DataObj*			pDataObj,
		SqlParamType		eParamType = NoParam,
		const int&			nSqlRecIdx = -1
	)
		:
		m_nIdx(nIdx),
		SqlBindingElem
		(
			strBindName,
			pDataObj,
			eParamType,
			nSqlRecIdx
		)
	{}

	virtual		int GetIndex() const { return m_nIdx; }
};


//////////////////////////////////////////////////////////////////////////////
//					SqlBindingElemArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlBindingElemArray : public SqlBindObjectArray
{
	DECLARE_DYNAMIC(SqlBindingElemArray)

public:
	SqlBindingElemArray() : SqlBindObjectArray() {}

public:
	SqlBindingElem* GetAt(int nPos) const { return (SqlBindingElem*)SqlBindObjectArray::GetAt(nPos); }
	SqlBindingElem* GetParamByName(const CString& strName) const;

public:
	DataObj*		GetDataObj(const CString& strName) const;

	DataObj*		GetDataObjAt(int pos) const;
	DataObj*		GetOldDataObjAt(int pos) const;
	const CString&	GetParamName(int nIdx) const;

	void		SetParamValue(const CString&	strName, const DataObj& aDataObjValue);
	void		GetParamValue(const CString&	strName, DataObj* pDataObj) const;
	void		GetParamValue(int nPos, DataObj* pDataObj) const;
	DataType	GetParamDataType(int nPos) const;
	int			GetParamPosition(const CString& strParamName) const;

public:
	int	AddColumn
	(
		const CString&	 strName,
		DataObj*	 pDataObj,
		const int& nSqlRecIdx = -1,
		bool  bAutoIncrement = false,
		int	nInsertPos = -1 //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
	);



	int AddParam
	(
		const CString&	strName,
		const DataType& nDataType,
		int nLen, 
		SqlParamType eParamType = Input,
		const SWORD nSqlDataType = DBTYPE_EMPTY, 
		int	nInsertPos = -1 //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
	);


	int AddParam
	(
		const CString&	strName,
		DataObj*		pDataObj,
		SqlParamType	eParamType = Input,
		int	nInsertPos = -1 //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
	);

	void InitColumns();

	bool SameValues() const;

	void Clear() { RemoveAll(); }

public:
	bool ExistParam(const CString& strParamName) const { return GetParamPosition(strParamName) != -1; }

};


//////////////////////////////////////////////////////////////////////////////
//					SqlParamArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlColumnArray : public SqlBindingElemArray
{
	DECLARE_DYNAMIC(SqlColumnArray)
};


//////////////////////////////////////////////////////////////////////////////
//					SqlParamArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlParamArray : public SqlBindingElemArray
{
	DECLARE_DYNAMIC(SqlParamArray)
};

#include "endh.dex"
