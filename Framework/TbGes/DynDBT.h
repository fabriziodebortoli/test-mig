#pragma once
#include "Dbt.h" 
#include "beginh.dex"

#define PARAM_PREFIX(id) cwsprintf(_T("EB@%i"), id)

class TB_EXPORT CPredicate : public CObject
{
	DECLARE_DYNAMIC(CPredicate)
public:
	CString m_sField;
	CString m_sOperator;
	CPredicate(const CString& sField, const CString& sOperator = _T("="))
		: m_sField(sField), m_sOperator(sOperator)
	{
	}
};

class TB_EXPORT CFieldPredicate : public CPredicate
{
	DECLARE_DYNAMIC(CFieldPredicate)
public:
	CString m_sFieldParameter;

	CFieldPredicate(const CString& sField, const CString& sFieldParameter, const CString& sOperator = _T("="))
		: CPredicate(sField, sOperator), m_sFieldParameter(sFieldParameter)
	{}
};

class TB_EXPORT CValuePredicate : public CPredicate
{
	DECLARE_DYNAMIC(CValuePredicate)
public:
	CString m_sFieldValue;

	CValuePredicate(const CString& sField, const CString& sFieldValue, const CString& sOperator = _T("="))
		: CPredicate(sField, sOperator), m_sFieldValue(sFieldValue)
	{}
};
class CForeignKeyPredicate : public CFieldPredicate
{
	DECLARE_DYNAMIC(CForeignKeyPredicate)
public:
	CForeignKeyPredicate(const CString& sPrimary, const CString& sForeign)
		: CFieldPredicate(sPrimary, sForeign)
	{}

};
class TB_EXPORT CPredicateContainer
{
private:
	CArray<CPredicate*> m_arPredicates;

public:
	CArray<CPredicate*>* GetPredicates() { return &m_arPredicates; }
	void				 CopyPredicates(CArray<CPredicate*>* pPredicates);

public:
	~CPredicateContainer();
	void OnDefineDynamicQuery(SqlTable* pTable, int& nParam);
	void OnPrepareDynamicQuery(SqlTable* pTable, SqlRecord* pMasterRecord, int& nParam);
	void OnPrepareDynamicPrimaryKey(SqlRecord* pRecord, SqlRecord* pMasterRecord);
	void AddForeignKey(const CString& sPrimary, const CString& sForeign);
	void AddFieldPredicate(const CString& sField, const CString& sValueField, const CString& sOperator);
	void AddValuePredicate(const CString& sField, const CString& sValue, const CString& sOperator);
private:
	void SplitQualifiedName(const CString& sQualified, CString& sTable, CString &sColumn);
};


///////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT DynDBTMaster : public DBTMaster
{
	DECLARE_DYNAMIC(DynDBTMaster)
	CPredicateContainer m_Query;
	CPredicateContainer m_BrowserQuery;
public:
	~DynDBTMaster(){}
	//-----------------------------------------------------------------------------	
	DynDBTMaster(SqlRecord *pRecord, CAbstractFormDoc* pDocument, const CString& sName) : DBTMaster(pRecord, pDocument, sName)
	{

	}

	static DynDBTMaster* Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName);
	CPredicateContainer* GetQuery() { return &m_Query; }
	CPredicateContainer* GetBrowserQuery() { return &m_BrowserQuery; }
private:
	//-----------------------------------------------------------------------------	
	DynDBTMaster(CRuntimeClass* pClass, CAbstractFormDoc* pDocument, const CString& sName):DBTMaster(pClass, pDocument, sName)
	{
	}

	//-----------------------------------------------------------------------------	
	DynDBTMaster( const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName ) : DBTMaster(sTableName, pDocument, sName)
	{

	}
	
	void OnPrepareBrowser(SqlTable*);
	void OnDefineQuery();
	void OnPrepareQuery();
};



///////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT DynDBTSlave : public DBTSlave
{
	DECLARE_DYNAMIC(DynDBTSlave)
	CPredicateContainer m_Query;

public:
	virtual ~DynDBTSlave(){}

	//-----------------------------------------------------------------------------	
	DynDBTSlave(SqlRecord *pRecord, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty) : DBTSlave(pRecord, pDocument, sName, bAllowEmpty)
	{

	}
	static DynDBTSlave* Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty);
	CPredicateContainer* GetQuery() { return &m_Query; }
private:
	//-----------------------------------------------------------------------------	
	DynDBTSlave (CRuntimeClass* pClass, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty ) : DBTSlave(pClass, pDocument, sName, bAllowEmpty)
	{
	}

	//-----------------------------------------------------------------------------	
	DynDBTSlave ( const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty ) : DBTSlave(sTableName, pDocument, sName, bAllowEmpty)
	{

	}


	void OnDefineQuery();
	void OnPrepareQuery();
	void OnPreparePrimaryKey();
};


///////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT DynDBTSlaveBuffered : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DynDBTSlaveBuffered)
	CPredicateContainer m_Query;
public:
	~DynDBTSlaveBuffered(){}
	//-----------------------------------------------------------------------------	
	DynDBTSlaveBuffered(SqlRecord *pRecord, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty, BOOL bCheckDuplicateKey) : DBTSlaveBuffered(pRecord, pDocument, sName, bAllowEmpty, bCheckDuplicateKey)
	{

	}

	//-----------------------------------------------------------------------------	
	DynDBTSlaveBuffered(CRuntimeClass* pClass, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty, BOOL bCheckDuplicateKey) : DBTSlaveBuffered(pClass, pDocument, sName, bAllowEmpty, bCheckDuplicateKey)
	{

	}

	//-----------------------------------------------------------------------------	
	DynDBTSlaveBuffered(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty, BOOL bCheckDuplicateKey) : DBTSlaveBuffered(sTableName, pDocument, sName, bAllowEmpty, bCheckDuplicateKey)
	{

	}

	static DynDBTSlaveBuffered* Create(const CString& sTableName, CAbstractFormDoc* pDocument, const CString& sName, BOOL bAllowEmpty, BOOL bCheckDuplicateKey);
	CPredicateContainer* GetQuery() { return &m_Query; }
private:
	void OnDefineQuery();
	void OnPrepareQuery();
	void OnPreparePrimaryKey();
};

#include "endh.dex"