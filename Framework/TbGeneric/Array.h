#pragma once

#include <AfxTempl.h>
#include <TbNameSolver/MacroToRedifine.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT Array : public CObArray
{      
public:
	typedef int (* TFCompare) (CObject*, CObject*);

	DECLARE_SERIAL(Array);

public:
	Array();
	virtual ~Array();

	void	SetAllocSize	(int nSize, int nGrowBy = -1);
	void	SetSize			(int nSize, int nGrowBy = -1);

	BOOL	MoveUp			(CObject*);
	BOOL	MoveDown		(CObject*);
	void	Swap			(int index1, int index2);

	void	RemoveAt		(int nIndex, int nCount = 1);
	void	RemoveAll		();

	BOOL	SetOwns			(BOOL);
	BOOL	IsOwnsElements  () const			{ return m_bOwnsElements; }
	
	void	SetRemoveAllOnExit (BOOL b = TRUE)	{ m_bRemoveAllOnExit = b; }
	BOOL	IsRemoveAllOnExit  () const			{ return m_bRemoveAllOnExit; }

	BOOL	Sort		(BOOL bDescending = FALSE, int start = 0, int end = -1);
		BOOL	BubbleSort	(BOOL bDescending = FALSE, int start = 0, int end = -1);
		BOOL	QuickSort	(BOOL bDescending = FALSE, int start = 0, int end = -1, bool bRecursive = true);
		BOOL	HeapSort	(BOOL bDescending = FALSE);

	int		BinarySearch	(CObject*, BOOL bDescending = FALSE, int low = 0, int high = -1) const;
	int		BinaryInsert	(CObject* pObj, BOOL bDescending = FALSE);
	int		SortedInsert	(CObject*, BOOL bDescending = FALSE);

	void	SetCompareFunction (TFCompare pfCompare) { m_pfCompare = pfCompare; }

	BOOL    IsSortable() const
		{
			if (m_pfCompare)
				return TRUE;
			if (GetSize() == 0)
				return FALSE;

			if (Compare(GetAt(0), GetAt(0)))
				return FALSE;
			return TRUE;	
		}

	virtual BOOL LessThen (CObject* po1, CObject* po2) const 
		{ 
			if (m_pfCompare)
				return m_bSortDescending ? m_pfCompare (po1, po2) > 0 : m_pfCompare (po1, po2) < 0;

			TRACE1("You need override the LessThen method in the array derived class %s\n", (LPCTSTR)CString(this->GetRuntimeClass()->m_lpszClassName));

			return FALSE; //Compare(po1, po2) < 0; rischio di ricorsione
		}

	virtual int	Compare (CObject* po1, CObject* po2) const 
	{ 
		if (m_pfCompare)
			return m_bSortDescending ? m_pfCompare (po2, po1) : m_pfCompare (po1, po2);

		TRACE1("You need override the Compare method in the array derived class %s\n", (LPCTSTR)CString(this->GetRuntimeClass()->m_lpszClassName));
			
		BOOL b1 = LessThen (po1, po2);

		if (!b1)
		{
			BOOL b2 = LessThen (po2, po1);
			if (!b2) 
				return 0;
		}

		if (m_bSortDescending)
			return b1 ? 1 : -1;
		return b1 ? -1 : 1; 
	}

	virtual BOOL IsElementEqual (CObject* po1, CObject* po2) const 
		{ 
			return Compare (po1, po2) == 0; 
		}

	virtual void Serialize(CArchive& ar);

	virtual int  Find (CObject*, int nStartPos = 0);
	int  FindPtr (CObject*, int nStartPos = 0) const;

protected:
	BOOL		m_bOwnsElements;		//call "delete" on all elements in RemoveAll
	BOOL		m_bRemoveAllOnExit;		//call RemoveAll in Distructor
	BOOL		m_bSortDescending;

	CArray <CObArray*, CObArray*>	m_arAlignArrays;

public:
	BOOL	AddAlignArray			(CObArray* ar);
	void	RemoveAllAlignArray		()				{ return m_arAlignArrays.RemoveAll(); }

	int		GetCountNotNull			() const;
private:
	TFCompare	m_pfCompare;

	void	_RecursiveQuickSort		(int left,		int right);
	int		_QSPartitionIt			(int left,		int right);
	BOOL	_IterativeQuickSort		(int first,		int last/*, BOOL bAscend = TRUE*/);
	void	_BubbleSortInternal		(int first,		int last);
	void	_HeapSort				();

// diagnostics
#ifdef _DEBUG
public:	
	virtual void Dump(CDumpContext&) const;
	virtual void AssertValid() const;
#endif
};

//=============================================================================
class DataObj;

class TB_EXPORT NamedDataObjArray : public Array
{
	DECLARE_DYNAMIC(NamedDataObjArray);
public:
	virtual DataObj* GetDataObjFromColumnName (const CString&) = 0;
};

// ogni elemento è una coppia identificatore di comando e stringa associata
// serve nella gestione dei menù pop in cui è necessario associare al comando una stringa
//=============================================================================
class TB_EXPORT CIdStringElement : public CObject
{
public:
	UINT		m_nID;
	CString		m_strNameElem;

public:
	CIdStringElement(UINT, const CString&);
};

//=============================================================================
class TB_EXPORT CIdStringArray : public Array
{
public:
	CIdStringElement* 	GetAt		(int nIndex)const	{ return (CIdStringElement*) Array::GetAt(nIndex);	}
	CIdStringElement*&	ElementAt	(int nIndex)		{ return (CIdStringElement*&) Array::ElementAt(nIndex); }
	
	CIdStringElement* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	CIdStringElement*&	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	CString				GetStringByID(UINT nID) const;
};
	
//=============================================================================
TB_EXPORT int  CStringArray_Find		(const CStringArray& ar, LPCTSTR psz, BOOL bNoCase = TRUE, int start = 0);
TB_EXPORT int  CStringArray_Remove		(CStringArray& ar, LPCTSTR psz, BOOL bNoCase = TRUE, BOOL bRemoveAllOccurrences = FALSE);
TB_EXPORT int  CStringArray_AddUnique	(CStringArray& ar, LPCTSTR psz, BOOL bNoCase = TRUE);
TB_EXPORT BOOL CStringArray_AppendUnique(CStringArray& ar, const CStringArray& arApp, BOOL bNoCase = TRUE);

TB_EXPORT void CStringArray_Sort		(CStringArray& ar, BOOL bNoCase = TRUE, BOOL bDescending = FALSE);
TB_EXPORT int  CStringArray_SortInsert	(CStringArray& ar, const CString& s, BOOL bNoCase = TRUE, BOOL bDescending = FALSE);

TB_EXPORT int  CStringArray_BinarySearch(const CStringArray& ar,	const CString& s, BOOL bNoCase = TRUE);
//TB_EXPORT int  CStringArray_BInsert (CStringArray& ar,			const CString& s, BOOL bNoCase = TRUE, BOOL bDescending = FALSE);

TB_EXPORT void CStringArray_Concat	(const CStringArray& ar,	CString& str,		LPCTSTR szSep = _T(";"));
TB_EXPORT int  CStringArray_Split(CStringArray& ar, const CString& str, LPCTSTR szSep = _T(";"), BOOL bAllowDuplicate = FALSE, BOOL bAllowNULL = FALSE);

TB_EXPORT void CStringArray_ConcatComment	(const CStringArray& ar, CString& str, LPCTSTR szSep = _T("\r\n"), LPCTSTR szPrefix = _T("/* "), LPCTSTR szPostfix = _T(" */"));

TB_EXPORT void CStringArray_RemoveWhenPrefixed		(CStringArray& ar, LPCTSTR szPrefix);
TB_EXPORT void CStringArray_RemoveWhenNotPrefixed	(CStringArray& ar, LPCTSTR szPrefix);
TB_EXPORT void CStringArray_RemovePrefix			(CStringArray& ar, LPCTSTR szPrefix);

///////////////////////////////////////////////////////////////////////////////
//
// class TArray
//
///////////////////////////////////////////////////////////////////////////////
template <class OBJ_TYPE>
class TArray : public CArray< OBJ_TYPE*, OBJ_TYPE*>
{
protected:
	BOOL					m_bOwnElements;
public:
	TArray (BOOL bOwnElement = TRUE) : m_bOwnElements (bOwnElement) {}

	virtual ~TArray () { Reset(); }

	void Reset () 
		{
			if (m_bOwnElements)
			{
				for (int c = 0; c <= GetUpperBound(); c++)
				{
					delete GetAt(c);
				}
			}

			RemoveAll();
		}

	BOOL	SetOwns(BOOL bSet = TRUE) { BOOL bOld = m_bOwnElements;	m_bOwnElements = bSet; return bOld; }
};


//=============================================================================
#include "endh.dex"
