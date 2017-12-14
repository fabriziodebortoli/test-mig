
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\RdeProtocol.h>

#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class Parser;
class Unparser;
class WoormTable;
class CWoormDoc;

class FillTableEvents;//class EventActions;
class RepEngine;

//============================================================================
class TB_EXPORT DisplayTableEntry : public CObject
{	//(ex internal DispTableObj)
	DECLARE_DYNAMIC(DisplayTableEntry)
public:
	DataStr		m_dsLayout;
	DataStr		m_dsName;
	WORD		m_wInternalID;
	int			m_nRows;

	DisplayTableEntry(const CString& sName, int nRows, WORD nID, const CString& sLayout)
		:
			m_wInternalID	(nID),
			m_dsName		(sName),
			m_nRows			(nRows),
			m_dsLayout		(sLayout)
		{
			//m_dsName.MakeLower();
			//or
			//m_dsLayout.SetUpperCase();
		}

	virtual ~DisplayTableEntry ()	{}

	virtual void		AddColumn	(CObject* ) {}
	virtual CObArray*	GetColumns	()			{ return NULL; }

	virtual BOOL	WriteLine (RepEngine&, WORD, RDEData* /*= NULL*/) { return FALSE; }

	CString		GetTableName	() const { return m_dsName.GetString(); }
	CString		GetLayoutName	() const { return m_dsLayout.GetString(); }

	WORD		GetId			() const { return m_wInternalID; }
	int			GetRows			() const { return m_nRows; }

	void		SetLayoutName	(const CString& sLayout)	{ m_dsLayout = sLayout; /*m_dsLayout.MakeLower();*/ }
	void		SetTableName	(const CString& sName)		{ m_dsName = sName; /*m_dsName.MakeLower();*/ }

	virtual void SetDataDisplayed() {}
};

//===========================================================================

class TB_EXPORT DisplayTables: public CObject
{	//(ex DispTable)
	DECLARE_DYNAMIC(DisplayTables)
public:
	Array			m_DispTables;
	CWoormDoc*	m_pDocument;
	SymTable*		m_pSymTable;

	static const CString	ALL_LAYOUT;
	static const CString	CURRENT_LAYOUT;

public:
				DisplayTables	(CWoormDoc*, SymTable*);
	virtual		~DisplayTables	()	{}

	void		SetDocument		(CWoormDoc* pDoc);
	void		SetSymTable		(SymTable* pST);

	DisplayTableEntry*	GetAt (int idx);

	virtual void		AddTable		(const CString& sName, int nRows, WORD nId, const CString& sLayout);
	virtual BOOL		OnDelTable		(DisplayTableEntry*) { return TRUE; }

	virtual DataStr&	GetCurrLayout	();

	virtual 	DisplayTableEntry*	FindSingleDisplayTable	();
	virtual		DisplayTableEntry*	MatchDisplayTable		(Parser&, BOOL& bUnnamed);

	BOOL		DelTable		(WORD nId);
	BOOL		SetRows			(WORD nId, UINT nRows);
	UINT		GetRows			(WORD nId);
	CString		GetNameFromIdx	(int idx);
	int			GetRowsFromIdx	(int idx);

	CString		GetName			(WORD nId);
	void		SetName			(WORD nId, const CString& name);
	int			Find			(WORD nId, const CString& layout);
	int			Find			(const CString& name, const CString& layout);
	BOOL		Exist			(const CString& name, const CString& layout);

	int			GetCount		() { return m_DispTables.GetSize(); };

	BOOL		Parse			(Parser&);
	void		Unparse			(Unparser&);

	BOOL		LayoutMove		(WORD nId, const CString& srcLayout, const CString& dstLayout);
	BOOL		LayoutClone		(WORD nId, const CString& srcLayout, const CString& dstLayout);
	void		LayoutRename	(		   const CString& srcLayout, const CString& dstLayout);
};

//============================================================================
class TB_EXPORT DTCommon 
{	
public:
	CObArray		m_OwnColumns;		// NON possiede gli oggetti

	int				m_nCurrRow;

	BOOL			m_bTableFull;
	BOOL			m_bDataDisplayed;
	int				m_nMultiLineFieldsNum;
	int				m_nMultiLineFieldsCurrLine;
	FillTableEvents*	m_pTableActions;	// actions before a overflow table event and/or
										// actions after a full table event
	BOOL			m_bExecutingTableActions;

	DTCommon::DTCommon ()
		:
		m_nCurrRow					(0),
		m_bTableFull				(FALSE),
		m_bDataDisplayed			(FALSE),
		m_pTableActions				(NULL),
		m_nMultiLineFieldsNum		(0),
		m_nMultiLineFieldsCurrLine	(0),
		m_bExecutingTableActions	(FALSE)
	{}

	DTCommon::~DTCommon ();
};

//-----------------------------------------------------------------------------
class TB_EXPORT DisplayTableEntryEngine : public DisplayTableEntry
{	//(ex DisplayTable)                                   
	DECLARE_DYNAMIC(DisplayTableEntryEngine)
public:
	BOOL		m_bOwnDT;
	DTCommon*	m_pDT;

public:
	DisplayTableEntryEngine(const CString& sName, int nRows, WORD nId, const CString& sLayout);
	~DisplayTableEntryEngine();

	virtual void		AddColumn				(CObject* pCol) { m_pDT->m_OwnColumns.Add(pCol); }

public:
	BOOL		ExecOverflowActions		();
	void		SetOverflow				() { m_pDT->m_bTableFull = TRUE; }
	virtual void SetDataDisplayed		() { m_pDT->m_bDataDisplayed	= TRUE; }
	
	BOOL		WriteLine				(RepEngine&, WORD, RDEData* /*= NULL*/);
	BOOL		WriteLineSeparator		(RepEngine&, WORD, RDEData* /*= NULL*/);

	void		ResetRowsCounter		();

	CObArray*	GetColumns				() { return &(m_pDT->m_OwnColumns); }

	int			GetCurrentRow			() { return m_pDT->m_nCurrRow + 1; }

	FillTableEvents*&	TableActions		() { return m_pDT->m_pTableActions; }
};

//===========================================================================
class TB_EXPORT DisplayTablesEngine: public DisplayTables
{
	DECLARE_DYNAMIC(DisplayTablesEngine)
public:
	DisplayTablesEngine	(CWoormDoc* pDoc, SymTable* = NULL);

	virtual void		AddTable		(const CString& aName, int nRows, WORD anId, const CString& sLayout);
	virtual BOOL		OnDelTable		(DisplayTableEntry*);
	virtual DataStr&	GetCurrLayout	();
};

//===========================================================================

#include "endh.dex"
