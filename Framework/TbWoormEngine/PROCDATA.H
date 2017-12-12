
#pragma once

#include "events.h"
#include "QueryObject.h"
#include "RepEngin.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class WoormTable;
class Parser;
class Unparser;

//============================================================================
class TB_EXPORT ObjItem : public CObject
{
protected:
	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceAfter;

public:
	virtual ~ObjItem() {}

	virtual CString		GetName	() const = 0;
	virtual void		SetName	(LPCTSTR) = 0;
	virtual BOOL		Parse	(Parser&) = 0;
	virtual void		Unparse(Unparser&, BOOL = FALSE, BOOL = FALSE) = 0;
	virtual BOOL		CanDeleteField (LPCTSTR) = 0;
	virtual BOOL		IsEmpty	() const = 0;

	virtual CString		Unparse();
};

//============================================================================
class TB_EXPORT ProcedureObjItem : public ObjItem
{
	DECLARE_DYNAMIC(ProcedureObjItem)

public:
	Procedure* m_pProcedure;

public:
	ProcedureObjItem(WoormTable*, const CString& sName = _T(""));
	ProcedureObjItem(RepEngine*, const CString& sName = _T(""));
	virtual ~ProcedureObjItem();
	
public:
	virtual CString		GetName			() const;
	virtual void		SetName			(LPCTSTR);

	SymTable* GetSymTable() const;

	virtual BOOL		Parse			(Parser&);
	virtual void		Unparse(Unparser&, BOOL bSkipHeader = FALSE, BOOL bSkipBeginEnd = FALSE);
	virtual CString		Unparse() { return __super::Unparse(); }

	virtual BOOL		CanDeleteField (LPCTSTR);

	void		DeleteField			(LPCTSTR);
	void		DeleteTable			(LPCTSTR);
	BOOL		IsEmpty				() const;

	void		DispTableChanged	(LPCTSTR pszTableName, LPCTSTR pszNewTableName);
};

//============================================================================
class TB_EXPORT QueryObjItem : public ObjItem
{
	DECLARE_DYNAMIC(QueryObjItem)
protected:
	CString m_sError;
public:
	QueryObject	m_Query;

public:
	QueryObjItem(WoormTable* pSymTable, const CString& sName, CTBContext* pTBContext = NULL);

public:
			QueryObject* GetQueryObject()		{ return &m_Query; }
			CString GetError() { return m_sError; }

	BOOL	IsEmpty () const { return FALSE; }

	virtual CString		GetName () const		{ return m_Query.GetName(); }
	virtual void		SetName (LPCTSTR sName) { m_Query.SetName(sName); }

	SymTable* GetSymTable() const;

	virtual BOOL		Parse	(const CString& sQuery);
	virtual BOOL		Parse	(Parser& p);
	virtual void		Unparse(Unparser& up, BOOL bSkipHeader = FALSE, BOOL bSkipBeginEnd = FALSE);
	virtual CString		Unparse() { return __super::Unparse(); }

	virtual BOOL		CanDeleteField (LPCTSTR);

protected:
	class Alias : public CObject
	{
		public:
			CString m_strAlias;
			CFunctionDescription* m_pFD;

			Alias(LPCTSTR sAlias, CFunctionDescription* pFD)
				:
				m_strAlias	(sAlias),
				m_pFD		(pFD)
			{}		
			virtual ~Alias() { SAFE_DELETE(m_pFD); } 
	};

	static DECLARE_LOCKABLE(Array, m_arAlias);

	static void AddAlias(LPCTSTR name, LPCTSTR ns);

	static void InitCallQuery();

public:
	static BOOL ResolveCallQuery(CString sFuncName, CFunctionDescription& aMethod);

#ifdef _DEBUG
	void AssertValid() const;
	void Dump(CDumpContext& dc) const;
#endif //_DEBUG
};

//============================================================================
class TB_EXPORT ObjectsData : public CObject
{
protected:
	WoormTable&		m_SymTable;
	Array&			m_Objects;

	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceInner;
	CStringArray	m_arCommentTraceAfter;

public:
	ObjectsData(WoormTable&, Array&);

	WoormTable* GetSymTable() const { return &m_SymTable; }

public:
	virtual BOOL	Parse		(Parser&) = 0;
	virtual void	Unparse		(Unparser&, Token tk);

	BOOL	CanDeleteField		(LPCTSTR) const;

	BOOL	Exist				(LPCTSTR pszName);

	int 	GetSize				()							{ return m_Objects.GetSize(); }
	Array& 	GetArray			() { return m_Objects; }

	int		GetIndex			(LPCTSTR pszName);
	CString GetName				(int nProcIdx);

	void 	Rename				(LPCTSTR pszOldName, LPCTSTR pszNewName);
	void 	Delete				(LPCTSTR pszDelProc);

	ObjItem* Get				(LPCTSTR pszName);
	ObjItem* GetAt				(int nIdx)					{ return (ObjItem*) m_Objects.GetAt(nIdx); }

	void	AddNew				(ObjItem* pNew) 			{ m_Objects.Add(pNew); }
	void	InsertAt			(int nIdx, ObjItem* pNew)	{ m_Objects.InsertAt (nIdx, pNew); }
	void	Delete				(int nIdx)					{ m_Objects.RemoveAt (nIdx); }
};

//============================================================================
class TB_EXPORT ProcedureData : public ObjectsData
{
	DECLARE_DYNAMIC(ProcedureData)
public:
	ProcedureData(WoormTable& st) : ObjectsData(st, st.m_arProcedures) {}

	virtual BOOL	Parse(Parser&);
	virtual void	Unparse(Unparser& oFile);

	ProcedureObjItem* Get (LPCTSTR pszName) { return (ProcedureObjItem*) ObjectsData::Get(pszName); }

	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);
    void	DispTableChanged	(LPCTSTR pszTableName, LPCTSTR pszNewTableName);

	void	Remove				(LPCTSTR);
};

//============================================================================
class TB_EXPORT QueryObjectData : public ObjectsData
{
	DECLARE_DYNAMIC(QueryObjectData)
public:
	QueryObjectData(WoormTable& st) : ObjectsData(st, st.m_arQueries) {}

	virtual BOOL	Parse(Parser&);
	virtual void	Unparse(Unparser& oFile);

	QueryObjItem* Get (LPCTSTR pszName) { return (QueryObjItem*) ObjectsData::Get(pszName); }
};
//============================================================================
#include "endh.dex"
