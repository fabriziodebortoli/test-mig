#pragma once

#include "DataObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

// valore e decodifica di un'eccezione presente sul file xml
//----------------------------------------------------------------
class TB_EXPORT CNumberToLiteralLookUpTable: public CObject
{
public:
	long		m_Value;
	CString		m_Description;
	CString		m_ForThousands;
	CString		m_ForMillions;

public:
	CNumberToLiteralLookUpTable	(
				long		lValue,
				CString		sDescription
			);
	~CNumberToLiteralLookUpTable(){};
};

// array di corrispondenze di LookUp
//----------------------------------------------------------------
class TB_EXPORT CNumberToLiteralLookUpTableList: public Array
{
public:
	CNumberToLiteralLookUpTableList () {}

public:
	CNumberToLiteralLookUpTable* 	GetAt			(int nIndex) const					{ return (CNumberToLiteralLookUpTable*) Array::GetAt(nIndex);	}
	CNumberToLiteralLookUpTable*&	ElementAt		(int nIndex)						{ return (CNumberToLiteralLookUpTable*&) Array::ElementAt(nIndex); }
	int			Add				(CNumberToLiteralLookUpTable* pObj)						{ return Array::Add(pObj); }
	
	CNumberToLiteralLookUpTable* 	operator[]		(int nIndex) const					{ return GetAt(nIndex);	}
	CNumberToLiteralLookUpTable*&	operator[]		(int nIndex)						{ return ElementAt(nIndex);	}

	CString					GetElementDescription	(long aValue, long aIndex);
	BOOL					ElementExist			(long aValue);
};

// eccezione relativa all'uso dei separatori
//----------------------------------------------------------------
class TB_EXPORT CSeparatorException: public CObject
{
public:
	int		m_Value;

public:
	CSeparatorException	(int aValue);
	~CSeparatorException(){};
};

// array di eccezioni all'uso del separatore
//----------------------------------------------------------------
class TB_EXPORT CSeparatorExceptionList: public Array
{
public:
	CSeparatorExceptionList () {}

public:
	CSeparatorException* 	GetAt					(int nIndex) const	{ return (CSeparatorException*) Array::GetAt(nIndex);	}
	CSeparatorException*&	ElementAt				(int nIndex)		{ return (CSeparatorException*&) Array::ElementAt(nIndex); }
	int		Add			(CSeparatorException* pObj)						{ return Array::Add(pObj); }
	
	CSeparatorException* 	operator[]				(int nIndex) const	{ return GetAt(nIndex);	}
	CSeparatorException*&	operator[]				(int nIndex)		{ return ElementAt(nIndex);	}
};

// eccezione relativa all'uso dei separatori
//----------------------------------------------------------------
class TB_EXPORT CDeclinationException: public CObject
{
public:
	CString	m_Kind;
	int		m_Value;

public:
	CDeclinationException	(CString aKind, int aValue);
	~CDeclinationException(){};

public:
	bool	IsException(int aValue);
};

// array di eccezioni alla declinazione
//----------------------------------------------------------------
class TB_EXPORT CDeclinationExceptionList: public Array
{
public:
	CDeclinationExceptionList () {}

public:
	CDeclinationException* 	GetAt					(int nIndex) const	{ return (CDeclinationException*) Array::GetAt(nIndex);	}
	CDeclinationException*&	ElementAt				(int nIndex)		{ return (CDeclinationException*&) Array::ElementAt(nIndex); }
	int		Add			(CDeclinationException* pObj)						{ return Array::Add(pObj); }
	
	CDeclinationException* 	operator[]				(int nIndex) const	{ return GetAt(nIndex);	}
	CDeclinationException*&	operator[]				(int nIndex)		{ return ElementAt(nIndex);	}
};

// declinazione
//----------------------------------------------------------------
class TB_EXPORT CDeclination: public CObject
{
public:
	int							m_Value;
	CString						m_Description;
	CDeclinationExceptionList*	m_pDeclinationExceptionList;
	

public:
	CDeclination	(int aValue, CString aDescription);
	~CDeclination();

public:
	void AddDeclinationException(CString aKind, int aValue);
	bool IsException(int aValue);
};

// array di declinazioni
//----------------------------------------------------------------
class TB_EXPORT CDeclinationList: public Array
{
public:
	CDeclinationList () {}

public:
	CDeclination* 	GetAt					(int nIndex) const	{ return (CDeclination*) Array::GetAt(nIndex);	}
	CDeclination*&	ElementAt				(int nIndex)		{ return (CDeclination*&) Array::ElementAt(nIndex); }
	int		Add			(CDeclination* pObj)					{ return Array::Add(pObj); }
	
	CDeclination* 	operator[]				(int nIndex) const	{ return GetAt(nIndex);	}
	CDeclination*&	operator[]				(int nIndex)		{ return ElementAt(nIndex);	}
};

// Decodifica dei gruppi di cifre (centinaia, migliaia, milioni e miliardi
//----------------------------------------------------------------
class TB_EXPORT CNumberGroup
{
public:
	CString					m_Value;
	CDeclinationList*		m_pDeclinationList;
	CString					m_bUseJunction;

public:
	void AddDeclination (int aValue, CString aDescription);
	CString GetDescription(int nValue, int lastDigit) const;
	void AddDeclinationException(int decValue, CString kind, int val);

public:
	CNumberGroup	(CString aValue, CString aUseJunction);
	~CNumberGroup();
};

// Gestore delle opzioni di decodifica
//----------------------------------------------------------------
class TB_EXPORT CNumberToLiteralLookUpTableManager : public CObject
{
	friend class CXmlNumberToLiteralParser;

	DECLARE_DYNCREATE(CNumberToLiteralLookUpTableManager);
public:
	enum DeclinationType		   { Hundreds, Thousands, Millions, Milliards };

private:
	CNumberToLiteralLookUpTableList*	m_pLookUpList;
	CNumberGroup*						m_pHundreds;
	CNumberGroup*						m_pThousands;
	CNumberGroup*						m_pMillions;
	CNumberGroup*						m_pMilliards;
	CSeparatorExceptionList*			m_pExceptions;

public:	
	CString								m_Junction;
	CString								m_Culture;
	CString								m_Separator;
	
	BOOL								m_bUnitInversion;
	BOOL								m_bDecimalLiteral;
	CString								m_strCentesimalSingular;
	CString								m_strCentesimalPlural;
	CString								m_strCurrencySingular;
	CString								m_strCurrencyPlural;
	CString								m_strUniversalSeparator;
//	BOOL								m_bUsedJunction;
//	BOOL								m_bWriteMilliards; //server per evitare cose tipo mille miliardi e 123 miliardi

public:
	CNumberToLiteralLookUpTableManager();
	~CNumberToLiteralLookUpTableManager();

public:
	CString Get (long aValue, long aIndex) const {return m_pLookUpList->GetElementDescription(aValue, aIndex);}

	const CNumberToLiteralLookUpTableList*	GetLookUpList()	const	 {return m_pLookUpList;}
	const CNumberGroup*						GetHundreds()	const	{return m_pHundreds;}
	const CNumberGroup*						GetThousands()	const	{return m_pThousands;}
	const CNumberGroup*						GetMillions()	const	{return m_pMillions;}
	const CNumberGroup*						GetMilliards()	const	{return m_pMilliards;}
	const CSeparatorExceptionList*			GetExceptions()	const	{return m_pExceptions;}

private:	
	void Clear();
	void Add (CNumberToLiteralLookUpTable* pLU);
	void AddSeparatorException (int aValue);
	void AddNumberGroup (DeclinationType eDecType, CString aValue, CString aUseJunction);
	void AddDeclination (DeclinationType eDecType, int aValue, CString aDescription);
	void AddDeclinationException (DeclinationType eDecType, int aDecValue, CString aKind, int aExcValue);

};

//=============================================================================
#include "endh.dex"
