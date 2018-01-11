
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include <TbGeneric\RdeProtocol.h>

#include <TbGeneric\DataObj.h>

#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include "RpSymTbl.h"

#include "RepField.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class Parser;
class Unparser;
class CReportSymTableDlg;
class GlobalId;
class DisplayTables;
class CWoormDoc;
class DisplayTableEntryEngine;
class DisplayTablesEngine;
class RepEngine;
class ProcedureObjItem;
class QueryObjItem;
class CStaticFieldArrayCache;

///////////////////////////////////////////////////////////////////////////////
enum TB_EXPORT RuleSelectionMode { SEL_ALL, SEL_NOT_NULL, SEL_NULL };

//============================================================================
class TB_EXPORT WoormTable : public SymTable
{
	friend class ProcedureData;
	friend class QueryObjectData;

	DECLARE_DYNAMIC(WoormTable)
public:
	enum WoormSymTableType
	{
		ReportSymTable_EDITOR, RepTable_ENGINE, SimpleTable_RDEVIEW, SymTable_ACTION, SymTable_RUNREPORT
	};

private:
	WoormSymTableType		m_TableType;

	DisplayTables*			m_pDisplayTables;

	RepEngine*				m_pRepEngine;

	GlobalId*				m_pGlobalId;

	Array					m_arProcedures;
	Array					m_arQueries;

public:
	WoormTable(WoormSymTableType tt, CWoormDoc* pDoc, GlobalId* = NULL);
	virtual ~WoormTable();

	BOOL		IsEmpty () const;

	WoormField*	GetAt			(INT_PTR idx)	const { return (WoormField*) __super::GetAt(idx); }

	WoormField*	GetField		(LPCTSTR sName, BOOL bFindParent)	const { return (WoormField*) __super::GetField(sName, bFindParent); }
	WoormField*	GetField		(LPCTSTR sName)						const { return (WoormField*) __super::GetField(sName); }

	WoormField*	GetFieldByID	(WORD wId)		const { return (WoormField*) __super::GetFieldByID(wId); }

	WoormSymTableType		GetTableType ()	const { return m_TableType; }

	virtual CWoormDoc*	GetDocument () const { return (CWoormDoc*) (__super::GetDocument()); }
	virtual void			SetDocument (CBaseDocument*	pDocument);

	DisplayTables*			GetDisplayTables() const { return m_pDisplayTables; }
	const Array&		GetProcedures		() const { return m_arProcedures; }
	const Array&		GetQueries			() const { return m_arQueries; }

	virtual	DataObj*	GetDataMember	(int)		const;

	void				CopyStaticField		(CStaticFieldArrayCache& arStaticField);
	
	//virtual because forward declaration on action call
	virtual void				AddProcedure	(ProcedureObjItem* pProcedure);
	virtual ProcedureObjItem*	FindProcedure	(LPCTSTR) const;

	virtual void				AddQuery		(QueryObjItem* pQuery);
	virtual QueryObjItem*		FindQuery		(LPCTSTR) const;
	
	void		AddRepEngine			(RepEngine* pRepEngine)		{ m_pRepEngine = pRepEngine; }
	RepEngine*	GetRepEngine			() const					{ return m_pRepEngine; }
	
	void		SetOwnerId				(CBaseDocument* pDocument);

	CString		GetAdviseName (const CString& sPrefix);

private:
	void		AddCommonSpecialFields	();
	void		AddEngineSpecialFields	();
	void		AddRdeViewSpecialFields	();
	void		AddEditorSpecialFields	();

public:
	int						GetDisplayTablesNum		();
	BOOL					ExistsDisplayTable		(const CString& sTableName);
	DisplayTableEntry*		FindSingleDisplayTable	();
	DisplayTableEntry*		MatchDisplayTable		(Parser& lex, BOOL& bUnnamed);
	void					ReattachDisplayTable	(const CString& sLayout);
	void					ResetRowsCounter		();
	
//reportsymtable
public:
	BOOL		MoreVisibleFreeFields	(LPCTSTR)	const;

	virtual void		SetLastId		(WORD wId);
	virtual WORD		GetCurId		() const;

	virtual INT_PTR		Add				(SymField*);

	virtual BOOL		OnBeforeDelField (SymField*);

	BOOL		CanDeleteField		(LPCTSTR)		const;

	void 		GetTotalOf			(WORD wIdColumn, CWordArray& IdArray, WoormField::RepFieldType aType);

//----
	BOOL		Parse				(Parser& lex);
	void		Unparse				(Unparser&);	
protected:
	BOOL		ParseDisplayTables	(Parser& lex);
	BOOL		ParseFields			(Parser&);
	BOOL		ParseVariables		(Parser* lex, CString& sError, int& nLine, BOOL bAllowVarsWithSameAlias = FALSE);
public:
	//RDEVIEW
	/*bAllowVarsWithSameAlias permettere parsing di variabili con alias uguali(colonna e suo totale) */
	BOOL		ParseVariablesFromString	(const CString& sReport, CString& sError, int& nLine, BOOL bAllowVarsWithSameAlias = FALSE);	
	BOOL		ParseVariables				(const CString& sFileName, CString& sError, int& nLine, BOOL bAllowVarsWithSameAlias = FALSE);
	void		UnparseVariables			(Unparser& parser);

	virtual BOOL	ResolveCallQuery		(CString sFuncName, CFunctionDescription& aMethods, CString& sHandleName) const;
	virtual BOOL	ResolveCallProcedure	(CString, CFunctionDescription&) const;

	//local scope
	virtual void		DeleteMeAsLocalScope();
	virtual SymTable* CreateLocalScope();
};

//=============================================================================
#include "endh.dex"
