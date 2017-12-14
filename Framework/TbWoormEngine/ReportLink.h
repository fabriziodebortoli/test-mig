
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class WoormTable;
class Block;

//===========================================================================
class TB_EXPORT WoormLinkFilter
{
public:
	DataType	m_FilterType;	//tipo di filtro
	Token		m_nOpFilter;	//token dell'operatore 

	int			m_nFilterAlias;	//alias del valore da confrontare per abilitare il link

	int			m_nValueFilterAlias;	//alias del valore da confrontare con il precedente
	DataObj*	m_pFilterValue;	//valore costante del filtro

public:
	WoormLinkFilter();
	~WoormLinkFilter();

	void Clear();
};

//===========================================================================
class TB_EXPORT WoormLink : public CObject
{
	DECLARE_DYNAMIC(WoormLink);

public:
	enum WoormLinkType { 
						ConnectionEmpty, ConnectionRadar, 
						ConnectionForm, ConnectionReport, ConnectionFunction, ConnectionURL 
						
						};
	enum WoormLinkSubType { Url, File, MailTo, CallTo, GoogleMap };

//PROPERTIES
	WoormLinkType		m_LinkType;
	WoormLinkSubType	m_SubType;
	BOOL				m_bLinkTargetByField;

	CString m_strTarget;	//namespace del report/documento/funzione o nome di un campo di tipo string che contiene il namespace
	CString m_strLinkOwner;	//nome del campo che possiede il link
     
	Expression*			m_pEnableLinkWhenExpr;

	Block*				m_pBeforeLink;
	Block*				m_pAfterLink;
//end PROPERTIES	

	WoormTable*			m_pViewSymbolTable;	//Woorm's Symbol Table on view state

	WoormTable*			m_pLocalSymbolTable;

	WoormTable*			m_pDocumentContextSymbolTable;

	int					m_nCurrentRow;

	int					m_nAlias;	//lasciato per ottimizzazione (e usato per la gestione grafica)

	BOOL				m_bSyntaxWithExpr;	//nuova sintassi
	WoormLinkFilter		m_Filter1;
	Token				m_nOpLogicalFilter;	//token logico per legare il secondo filtro 
	WoormLinkFilter		m_Filter2;

	BOOL				m_bWrongName;
	int					m_nCounterForGenerateLocalID;

public:
	WoormLink	(WoormTable* pViewSymbolTable);
	virtual ~WoormLink();

public:
	BOOL Parse (Parser& lex);
	void Unparse (Unparser& ofile);
	BOOL AddLinkParam(CString sName, DataType dtType, CString strExpr/*, int nAlias = SpecialReportField::NO_INTERNAL_ID*/);
	CString	EncodeURLString (CString strLinkUrl); //nel caso sia una connessione URL http ritorna la stringa get con parametri 

	BOOL CanDeleteField (LPCTSTR szName, CString& sLog) const;

protected:
	void Clear();

	BOOL ParseItem (Parser& lex, BOOL bHaveBegin, WoormTable*);
	BOOL ParseItems (Parser& lex, WoormTable*);
	void UnparseItems (Unparser& ofile, WoormTable*);

	BOOL ParseFilterClause (Parser& lex, WoormLinkFilter& Filter);
	BOOL ParseConstValue (Parser& lex, DataType dtFilterType, DataObj*& pValue);

	void UnparseFilterClause (Unparser& ofile, const WoormLinkFilter&	Filter);
	void UnparseConstValue (Unparser& ofile, const DataObj* pValue);
	CString	UnparseConstValue (const DataObj* pValue);

	void UnparseFilter(Unparser& ofile, BOOL bConvertExpression = FALSE);
	CString	UnparseFilter();

public:
	static BOOL ParseAlias(Parser& lex, int& nAlias);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	
		{ ASSERT_VALID(this); AFX_DUMP0(dc, " WoormLink\n");}
#endif // _DEBUG
};

//===========================================================================
class TB_EXPORT WoormLinkArray : public Array
{
protected:
	WoormLink* m_pConnectionRadar;

public:
	WoormTable*		m_pViewSymbolTable;
	BOOL				m_bPlayBack;	//TRUE se si sta leggendo un RDE

	WoormLinkArray (WoormTable* pViewSymbolTable = NULL) 
		: 
		m_pConnectionRadar (NULL),
		m_pViewSymbolTable (pViewSymbolTable)
		{}

	WoormLink* 	GetAt		(int nIdx)const	{ return (WoormLink*) Array::GetAt(nIdx);	}
	WoormLink*&	ElementAt	(int nIdx)		{ return (WoormLink*&) Array::ElementAt(nIdx); }
	
	WoormLink* 	operator[]	(int nIdx)const	{ return GetAt(nIdx);	}
	WoormLink*&	operator[]	(int nIdx)		{ return ElementAt(nIdx);	}

	void			RemoveAll	() { Array::RemoveAll(); m_pConnectionRadar = NULL; }

	WoormLink* 	GetFromID			(int nID) const;
	WoormLink*		GetConnectionRadar	() const { return m_pConnectionRadar; }

	BOOL Parse(Parser& lex);
	void Unparse(Unparser& oFile) const;

	BOOL CanDeleteField (LPCTSTR szName, CString& sLog) const;
};
//=============================================================================
#include "endh.dex"
