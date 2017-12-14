
#pragma once

#include <afxtempl.h>

#include "DataObj.h"

#include "beginh.dex"

//=============================================================================
// CElemRicorsione : elemento utile per liste di controllo ricorsione
//=============================================================================
class TB_EXPORT CElemRecursion : public CObject
{      
	DECLARE_DYNAMIC(CElemRecursion)

public:
	CElemRecursion(const CString& strKey, int nLev);

	CString		m_strsKey;
	int			m_nLev;
};

typedef	CTypedPtrList <CObList, CElemRecursion*> ElementList;

//=============================================================================
//		Class Definition: CCheckRecursion
//=============================================================================
class TB_EXPORT CCheckRecursion : public CObject
{
	DECLARE_DYNCREATE(CCheckRecursion)

private:
	ElementList	m_Elements;

public:		
	CCheckRecursion();
	~CCheckRecursion();

public:		
	void Clear();

	// metodi "base"
	void Add		(const CString& strKey,	int nLev);
	BOOL IsRecursive(const CString& strKey,	int nLev);

	// chiave su id, es. per ricorsivita` OdP
	void Add		(const DataLng& aID, const DataInt& aLev);
	BOOL IsRecursive(const DataLng& aID, int nLev);

	// chiave su enum + 2 codici stringa, es. per ricorsivita` Tipo + Articolo + Variante
	void Add		(const DataEnum& aEnum, const DataStr& aKey1, const DataStr& aKey2, const DataInt& aLev);
	BOOL IsRecursive(const DataEnum& aEnum, const DataStr& aKey1, const DataStr& aKey2, int nLev);

	// chiave 2 codici stringa, es. per ricorsivita` Worker e Risorsa
	void Add		(const DataStr& aKeyType1, const DataStr& aKey1, const DataStr& aKeyType2, const DataStr& aKey2, const DataInt& aLev);
	BOOL IsRecursive(const DataStr& aKeyType1, const DataStr& aKey1, const DataStr& aKeyType2, const DataStr& aKey2, int nLev);
};

#include "endh.dex"

