
#include "stdafx.h"
#include <math.h>

#include <TbNameSolver/MacroToRedifine.h>

#include "array.h"
#include "ISqlRecord.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

IMPLEMENT_SERIAL(Array, CObArray, 1 | VERSIONABLE_SCHEMA)
	
IMPLEMENT_DYNAMIC(ISqlRecord, NamedDataObjArray)
//----------------------------------------------------------------------------
Array::Array()
	:
	m_bOwnsElements (TRUE),
	m_bRemoveAllOnExit (TRUE),
	m_pfCompare (NULL),
	m_bSortDescending (FALSE)
{
}

//----------------------------------------------------------------------------
Array::~Array()
{
	ASSERT_VALID(this);

	if (m_bRemoveAllOnExit)
		RemoveAll();

	RemoveAllAlignArray();
}

//----------------------------------------------------------------------------
void Array::RemoveAll()
{
	if (m_bOwnsElements)
	{
		int n = GetSize();
		CObject* pO = NULL;
		int i = 0;
		try
		{
			for (;i < n; i++) 
				if (pO = GetAt(i)) 
				{
					ASSERT_VALID(pO);

					try { delete pO; } 
					catch (...) { ASSERT(FALSE);	}

					SetAt(i, NULL);
				}
		}
		catch (...)
		{
			ASSERT(FALSE);
		}
	}

	CObArray::RemoveAll();
}

//----------------------------------------------------------------------------
void Array::RemoveAt(int nIndex, int nCount)
{
	if (m_bOwnsElements)
	{
		int n = GetSize();
		int j = nCount;
		CObject* pO;
		for (int i = nIndex; (i < n) && (j-- > 0); i++)
			if (pO = GetAt(i))
			{
				ASSERT_VALID(pO);

				try { delete pO; }
				catch (...) { ASSERT(FALSE); }
			}
	}

	CObArray::RemoveAt(nIndex, nCount);
}

/*
//----------------------------------------------------------------------------
void Array::MoveAt (int nFromIndex, int nToIndex)
{
CObject* pO = __super::GetAt(nFromIndex);
ASSERT_VALID(pO);

__super::RemoveAt(nFromIndex) ;

if (nFromIndex < nToIndex)
nToIndex--;

__super::InsertAt(nToIndex, pO);
}

*/
//----------------------------------------------------------------------------
BOOL Array::MoveDown(CObject* pO)
{
	ASSERT_VALID(pO);

	int nCurPos = FindPtr(pO);
	if (nCurPos < 1)
		return FALSE;

	Swap(nCurPos, nCurPos - 1);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Array::MoveUp(CObject* pO)
{
	ASSERT_VALID(pO);

	int nCurPos = FindPtr(pO);
	if (nCurPos < 0 || nCurPos == GetUpperBound())
		return FALSE;

	Swap(nCurPos, nCurPos + 1);
	return TRUE;
}

//----------------------------------------------------------------------------
void Array::SetSize (int nSize, int nGrowBy/* = -1*/)
{
	ASSERT(nSize >= 0);
	if (m_bOwnsElements && nSize < GetSize())
	{
		if (nSize > 0)
			RemoveAt(nSize, GetSize() - nSize);
		else
			RemoveAll();
	}

	__super::SetSize(nSize, nGrowBy);
}

void Array::SetAllocSize (int nSize, int nGrowBy/* = -1*/)
{
	int currSize = GetSize();

	SetSize(nSize, nGrowBy);

	if (nSize > currSize)
		m_nSize = currSize;
}

//----------------------------------------------------------------------------
BOOL Array::SetOwns(BOOL IAmOwns)
{
	BOOL bOld = m_bOwnsElements;
	m_bOwnsElements = IAmOwns;
	return bOld;
}

//----------------------------------------------------------------------------
BOOL Array::AddAlignArray (CObArray* ar)	
{ 
	if (ar == this || ar == NULL)
		return FALSE;

	if (ar->IsKindOf(RUNTIME_CLASS(Array)))
		for (int i = 0; i < ((Array*)ar)->m_arAlignArrays.GetSize(); i++)
		{
			CObArray* a = ((Array*)ar)->m_arAlignArrays.GetAt(i);
			if (a == this) 
				return FALSE;
		}

	return 	m_arAlignArrays.Add(ar) >= 0; 
}

//----------------------------------------------------------------------------
int Array::GetCountNotNull() const
{
	int c = 0;
	for (int i = 0; i < GetSize(); i++)
		if (GetAt(i)) c++;
	return c;
}

//----------------------------------------------------------------------------
void Array::Serialize(CArchive& ar)
{
	CObArray::Serialize(ar);

   if ( ar.IsStoring() )
   {
		ar << m_bOwnsElements;
		ar << m_bRemoveAllOnExit;
   }
   else
   {
	   UINT nVer = ar.GetObjectSchema();
	   switch(nVer)
	   {
	   case 1:
			ar >> m_bOwnsElements; 
			ar >> m_bRemoveAllOnExit; 
			break;
			
	   default:
			;
	   }
   }
}

//----------------------------------------------------------------------------
BOOL Array::BubbleSort (BOOL bDescending/* = FALSE*/, int start/* = 0*/, int end/* = -1*/)
{	
	for (int i = 0; i < m_arAlignArrays.GetSize(); i++)
	{
		CObArray* ar = m_arAlignArrays.GetAt(i);
		if (ar->GetSize() != GetSize()) 
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	if (GetSize() < 2) 
		return TRUE;

	if (!IsSortable())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bSortDescending = bDescending;

	if (LessThen(GetAt(0), GetAt(0))) 
	{
		ASSERT(FALSE);
		return FALSE; //metodo non reiplementato
	}

	if (end < start)
		end = GetUpperBound();

	if (start == end)
		return TRUE;

	_BubbleSortInternal (start, end + 1);

	return TRUE;
}

//----------------------------------------------------------------------------
void Array::_BubbleSortInternal (int start, int n)
{	
	int a, b;
	for (a = start + 1; a < n ; a++)
		for ( b = n - 1; b >= a; b--)
			if (LessThen(GetAt(b), GetAt(b - 1))) 
			{
				Swap(b, b - 1);
			}
}

//-----------------------------------------------------------------------------
// Sposta di posizione i puntatori all'interno dell' Array
void Array::Swap (int index1, int index2)
{
	CObject* pTemp = GetAt (index1);
	SetAt (index1, GetAt (index2));
	SetAt (index2, pTemp);

	for (int i = 0; i < m_arAlignArrays.GetSize(); i++)
	{
		CObArray* ar = m_arAlignArrays.GetAt(i);
		if (ar->GetSize() != GetSize()) continue;

		if (ar->IsKindOf(RUNTIME_CLASS(Array)))
		{
			((Array*)ar)->Swap(index1, index2);	//eventuale ricorsione
		}
		else
		{
			pTemp = ar->GetAt (index1);
			ar->SetAt (index1, ar->GetAt (index2));
			ar->SetAt (index2, pTemp);
		}
	}
}

//-----------------------------------------------------------------------------
int Array::_QSPartitionIt (int left,	int right)
{
	// Il pivot e' l'elemento che si trova in fondo all'array o al segmento di array
	{
        // check atleast three elements to find a better median candidate
        int x, s, g;

        if (Compare(GetAt(left) , GetAt((left + right) / 2)) < 0)
        {  
            s = left;
            g = (left + right) / 2;
        }
        else
        {
            g = left;
            s = (left + right) / 2;
        }
        if (Compare(GetAt(right), GetAt(s)) <= 0)
            x = s;
        else if (Compare(GetAt(right), GetAt(g)) <= 0)
            x = right;
        else
            x = g;

        Swap(x, right);      // swap the split-point element
	}
	//----
	CObject* pPivot = GetAt (right);
	int up = right;
	left--;
	for (;;)
	{
		//Cerco all'interno dell'array il massimo elemento
		while (Compare(GetAt(++left), pPivot) < 0);

		//Cerco all'interno dell'array il minimo elemento
		while (right > 0 && Compare(GetAt (--right), pPivot) > 0);

		if (left >= right)
			break;
		else
			Swap (left, right);
	}

	Swap (left, up);

	return left;
}

//-----------------------------------------------------------------------------
// L'algoritmo di quicksort, separa una array in due segmenti di array, e applica 
//	ricorsivamente l'ordinamento sui due segmenti.
void Array::_RecursiveQuickSort (int left, int right)
{
	{
		// Se la dimensione dell'array e' <= 1, allora vuol dire che l'array e'ordinato
		int l = (right - left);
		if (l <= 0)
			return;
		// Se la dimensione dell'array e' < 10, conviene un alg. più semplice (Knuth)
		if (l < 10)
		{
			_BubbleSortInternal(left, right + 1);
			return;
		}
	}

	int Partition = _QSPartitionIt (left, right);
		
	// applico l'algoritmo di ordinamento ai due segmenti di array
	_RecursiveQuickSort (left, Partition - 1);
	_RecursiveQuickSort (Partition + 1, right);
}

//-----------------------------------------------------------------------------
BOOL Array::QuickSort (BOOL bDescending/* = FALSE*/, int start/* = 0*/, int end /*= -1*/, bool bRecursive/* = true*/)
{
	for (int i = 0; i < m_arAlignArrays.GetSize(); i++)
	{
		CObArray* ar = m_arAlignArrays.GetAt(i);
		if (ar->GetSize() != GetSize()) 
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	if (GetSize() < 2)
		return TRUE;

	if (!IsSortable())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bSortDescending = bDescending;

	if (end < start)
		end = GetUpperBound();

	if (start == end)
		return TRUE;

	//non si puo' utilizzare per gli m_arAlignArrays e per eventuale classi derivate che hanno reimplementato il metodo virtuale Compare
	//::qsort(GetData(), GetSize(), sizeof(CObject*), m_pfCompare);
	//::qsort(GetData(), GetSize(), sizeof(CObject*), CompareVoid);

	if (bRecursive)
	{
		_RecursiveQuickSort (start, end);
		return TRUE;
	}

	return _IterativeQuickSort (start, end);
}

///////////////////////////////////////////////////////////////////////////////
/// Sort the array between a low and high bound in either ascending or descending order
/// Parameters: 
///      int  nFirst : between 0 and the upper bound of the array.
///      int  nLast  : between 0 and the upper bound of the array.
///                  : must be guaranteed >= nFirst.
///      bool bAscend: true  - sort in ascending order
///                  : false - sort in descending order
BOOL Array::_IterativeQuickSort(int nFirst, int nLast/*, BOOL bAscend=FALSE*/)
{
	if ((nLast - nFirst) <= 0)
		return TRUE;

    // Stack
	int nStackMax = (nLast - nFirst + 1);

	class QS_stack {

		typedef struct s_elem { int first; int last; } t_elem;

		t_elem* Stk;
		int nStkPtr;
		int nMax;

	public:
		QS_stack(int nStackMax)
		{
			Stk = new t_elem[nStackMax];
			nStkPtr = -1;
			nMax = nStackMax;
		}

		void Push (int f, int l)
		{
			if (nStkPtr >= nMax)
			{
				return;
			}
				
			nStkPtr++;
			Stk[nStkPtr].first = f;
			Stk[nStkPtr].last = l;
		}

		void Pop (int& f, int& l)
		{
			if (nStkPtr < 0)
			{
				return;
			}

			f = Stk[nStkPtr].first;
			l = Stk[nStkPtr].last;
			nStkPtr--;
		}
			
		bool IsEmpty()
		{
			return (nStkPtr < 0 || Stk == NULL);
		}
	};

	QS_stack S (nStackMax);
	//----
	int i, j;
    bool bSortCompleted = false;
	bool bDirection = true;

    do
    {
        do
        {
            i = nFirst;
            j = nLast;
            bDirection = true;

            do
            {
                //if ((nData[i] > nData[j]) == bAscend)
				if ((Compare(GetAt(i), GetAt(j)) > 0)/* == bAscend*/)
                {
					Swap(i, j);

                    bDirection = !bDirection;
                }

                if (bDirection)
                    j--;
                else
                    i++;

            } while (i < j);

            if ((i + 1) < nLast)
            {
                S.Push(i + 1, nLast);
            }
            nLast = i - 1;

        } while (nFirst < nLast);

        if (S.IsEmpty())
        {
            // No more partitions to sort, so by definition we've finished!
            bSortCompleted = true;
        }
        else
        {
            // Pop the most recently stored partition and sort that
			S.Pop(nFirst, nLast);
        }

    } while (!bSortCompleted);

    return TRUE;
}

//-----------------------------------------------------------------------------
int  Array::Find (CObject* pVal, int nStartPos/* = 0*/)
{
	for (int i = nStartPos; i < GetSize(); i++)
	{
		CObject* pO = GetAt(i);
		if (pVal == pO) 
			return i;
		if (IsElementEqual (pO, pVal))
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int  Array::FindPtr (CObject* pVal, int nStartPos/* = 0*/) const
{
	ASSERT_VALID(pVal);
	for (int i = nStartPos; i < GetSize(); i++)
	{
		CObject* pO = GetAt(i);
		if (pVal == pO) 
			return i;
	}
	return -1;
}

//------------------------------------------------------------------------------
void Array::_HeapSort ()
{          
	int n = GetSize();
	if (n < 2) return;

    int i = n / 2;
	int parent, child;  
    CObject* pElem;  
  
    for (;;) 
    { /* Loops until arr is sorted */  
        if (i > 0) 
        { /* First stage - Sorting the heap */  
            i--;           /* Save its index to i */
            pElem = GetAt(i);    /* Save parent value to t */  
        } 
        else 
        {     /* Second stage - Extracting elements in-place */  
            n--;           /* Make the new heap smaller */  
            if (n == 0) 
                return; /* When the heap is empty, we are done */

            pElem = GetAt(n);    /* Save last value (it will be overwritten) */
            SetAt(n, GetAt(0)); /* Save largest value at the end of arr */  
        }  
  
        parent = i; /* We will start pushing down t from parent */  
        child = i * 2 + 1; /* parent's left child */  
  
        /* Sift operation - pushing the value of t down the heap */  
        while (child < n)
        {
            if ( ((child + 1) < n) && (Compare(GetAt(child + 1), GetAt(child)) > 0) )
            {  
                child++; /* Choose the largest child */  
            }

            if (Compare(GetAt(child), pElem) > 0)
            { /* If any child is bigger than the parent */

                SetAt(parent,  GetAt(child)); /* Move the largest child up */ 
 
                parent = child; /* Move parent pointer to this child */  
                //child = parent*2-1; /* Find the next child */  
                child = parent * 2 + 1; /* the previous line is wrong*/  
            } 
            else
            {  
                break; /* t's place is found */  
            }  
        }
        SetAt(parent, pElem); /* We save t in the heap */  
    }  
}

//------------------------------------------------------------------------------
BOOL Array::HeapSort (BOOL bDescending/* = FALSE*/)
{
	if (GetSize() < 2)
		return TRUE;

	if (!IsSortable())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (m_arAlignArrays.GetSize())	//_HeapSort NON supporta gli array ausiliari
		return QuickSort(bDescending, 0, -1, GetSize() < 5000);

	m_bSortDescending = bDescending;

	_HeapSort ();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL Array::Sort (BOOL bDescending/* = FALSE*/, int start/* = 0*/, int end /*= -1*/)
{ 
	if (GetSize() < 2)
		return TRUE;

	//if (!IsSortable())
	//{
	//	ASSERT(FALSE);
	//	return FALSE;
	//}

	if (m_arAlignArrays.GetSize() == 0 && start == 0 && end == -1)
		return HeapSort(bDescending);

	if (GetSize() < 5000)
		return QuickSort(bDescending, start, end, true);

	return BubbleSort(bDescending, start, end);
}

//-----------------------------------------------------------------------------
int Array::SortedInsert(CObject* pObj, BOOL bDescending /*= FALSE*/)
{
	if (GetCount() == 0)
		return Add(pObj);

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		int nCmp = Compare(pObj, GetAt(i));
		if (nCmp == 0)
			return i;

		if (bDescending)
		{
			if (nCmp > 0)
			{
				InsertAt(i, pObj);
				return i;
			}
		}
		else
		{
			if (nCmp < 0)
			{
				InsertAt(i, pObj);
				return i;
			}
		}
	}
	return Add(pObj);
}

//-----------------------------------------------------------------------------
int Array::BinaryInsert(CObject* pObj, BOOL bDescending/* = FALSE*/)
{
	if (GetCount() == 0)
		return -1;

	int low = 0;
	int high = GetUpperBound();
	int h = bDescending ? low : high;

	while (low < high)
	{
		int mid = low + (high - low) / 2;
		int nCmp = Compare(pObj, GetAt(mid));
		if (!nCmp)
			return mid;

		if (nCmp < 0)
			high = mid - 1;
		else
			low = mid + 1;
	}

	int pos = -1;

	if (bDescending)
	{
		if (high == h)
			pos = h;
		else if (Compare(pObj, GetAt(high)) < 0) //a[first] > x)
			pos = high + 1;
		else
			pos = high;
	}
	else
	{
		if (low == h)
			pos = h;
		else if (Compare(pObj, GetAt(low)) < 0) //a[first] > x)
			pos = low;
		else
			pos = low + 1;
	}

	ASSERT(pos >= 0 && pos < GetSize());
	InsertAt(pos, pObj);
	return pos;
}

//-----------------------------------------------------------------------------
int Array::BinarySearch(CObject* pObj, BOOL bDescending /*= FALSE*/, int low /*=0*/, int high/*=-1*/) const
{
	if (GetCount() == 0)
		return -1;
	if (high == -1)
		high = GetUpperBound();
	if (low > high)
		return -1;

	while (low <= high)
	{
		int mid = low + (high-low) / 2;
		int nCmp = Compare(pObj, GetAt(mid));
		if (!nCmp) 
			return mid;
		
		if (bDescending)
		{
			if (nCmp < 0)
				low = mid + 1;
			else
				high = mid - 1;
		}
		else
		{
			if (nCmp < 0)
				high = mid - 1;
			else
				low = mid + 1;
		}
	}

	return - 1;
}
/*
int mid = (low+high) / 2;
int nCmp = Compare(pObj, GetAt(mid));
if (!nCmp) 
	return mid;
if (nCmp < 0)
	return BinarySearch(pObj, low, mid -1);
//else
	return BinarySearch(pObj, mid + 1, high);
*/
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void Array::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n\t(Tb)Array = ", this->GetRuntimeClass()->m_lpszClassName);
}

void Array::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(NamedDataObjArray, Array);

//////////////////////////////////////////////////////////////////////////////
//
//				class CIdStringArray implementation
//
//////////////////////////////////////////////////////////////////////////////
//
CIdStringElement ::CIdStringElement (UINT nID, const CString& strNameElem)
:
	m_nID			(nID),
	m_strNameElem	(strNameElem)
{
}

//////////////////////////////////////////////////////////////////////////////
//
//				class CIdStringArray implementation
//
//////////////////////////////////////////////////////////////////////////////
//
CString	CIdStringArray::GetStringByID(UINT nID) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetAt(i)->m_nID == nID) return GetAt(i)->m_strNameElem;
	
	return _T("");
}

///////////////////////////////////////////////////////////////////////////////
int CStringArray_Find (const CStringArray& ar, LPCTSTR psz, BOOL bNoCase /*= TRUE*/, int start/* = 0*/)
{
	for(int i=start; i <= ar.GetUpperBound(); i++)
		if
			( 
				bNoCase 
				?
				(ar.GetAt(i)).CompareNoCase(psz) == 0 
				:
				(ar.GetAt(i)).Compare(psz) == 0
			)
				return i;

	return -1;
}

int CStringArray_Remove (CStringArray& ar, LPCTSTR psz, BOOL bNoCase /*= TRUE*/, BOOL bAll/*=FALSE*/)
{
	int idx = 0; int deleted = 0;
	while ((idx = CStringArray_Find(ar, psz, bNoCase, idx)) != -1)
	{
		ar.RemoveAt(idx);

		deleted++;

		if (!bAll) 
			break;
	}
	return deleted;
}

int  CStringArray_AddUnique(CStringArray& ar, LPCTSTR psz, BOOL bNoCase/* = TRUE*/)
{
	int idx = CStringArray_Find(ar, psz, bNoCase);
	if (idx > -1)
		return idx;
	return ar.Add(psz);
}

BOOL  CStringArray_AppendUnique(CStringArray& ar, const CStringArray& arApp, BOOL bNoCase/* = TRUE*/)
{
	int s = ar.GetSize();
	for (int i = 0; i < arApp.GetSize(); i++)
	{
		CStringArray_AddUnique(ar, arApp[i], bNoCase);
	}
	return ar.GetSize() > s;
}

///////////////////////////////////////////////////////////////////////////////
int CompareAscNoCase(const void* left, const void* right)
{
    return ((CString*)left)->CompareNoCase(*((CString*)right));
}

int CompareAscCase(const void* left, const void* right)
{
    return ((CString*)left)->Compare(*((CString*)right));
}

int CompareDescNoCase(const void* left, const void* right)
{
    return ((CString*)right)->CompareNoCase(*((CString*)left));
}

int CompareDescCase(const void* left, const void* right)
{
    return ((CString*)right)->Compare(*((CString*)left));
}

void CStringArray_Sort (CStringArray& ar, BOOL bNoCase /*= TRUE*/, BOOL bDescending /*= FALSE*/)
{
	if (bNoCase)
		qsort(ar.GetData(), ar.GetSize(), sizeof(CString*), bDescending ? CompareDescNoCase : CompareAscNoCase);
	else
		qsort(ar.GetData(), ar.GetSize(), sizeof(CString*), bDescending ? CompareDescCase : CompareAscCase);
}

//-----------------------------------------------------------------------------
int CStringArray_BinarySearch(const CStringArray& ar, const CString& s, BOOL bNoCase /*= TRUE*/)
{
	void * ptr =  bsearch(&s, ar.GetData(), ar.GetSize(), sizeof(CString*), bNoCase ? CompareAscNoCase : CompareAscCase);
	if (!ptr) 
		return -1;
	return ( ((CString*)ptr) - ar.GetData()) / sizeof(CString*);
}

//-----------------------------------------------------------------------------
//int CStringArray_BInsert (int low, int high, CStringArray& ar, const CString& s, BOOL bNoCase /*= TRUE*/, BOOL bDescending /*= FALSE*/)
//{
//	ASSERT_TRACE(FALSE, "CStringArray_BInsert NON funziona!\n");
//	if (high < low)
//	{
//		ar.InsertAt(++low, s);
//		return low;
//	}
//
//    int mid = low + (high - low) / 2;
//
//	int cmp = bDescending ?
//				(bNoCase ? CompareDescNoCase(&ar[mid] , &s) : CompareDescCase(&ar[mid] , &s)) 
//				:
//				(bNoCase ? CompareAscNoCase (&ar[mid] , &s) :	CompareAscCase (&ar[mid] , &s));
//
//    if (cmp > 0)
//        return CStringArray_BInsert(low, mid - 1,	ar, s, bNoCase, bDescending);
//    else if (cmp < 0)
//        return CStringArray_BInsert(mid + 1, high,	ar, s, bNoCase, bDescending);
//    else
//        return mid; // found
//}
//
//int CStringArray_BInsert (CStringArray& ar, const CString& s, BOOL bNoCase /*= TRUE*/, BOOL bDescending /*= FALSE*/)
//{
//	if (ar.GetCount() == 0)
//		return ar.Add(s);
//
//	return CStringArray_BInsert (0, ar.GetCount() - 1, ar, s, bNoCase, bDescending);
//}

//-----------------------------------------------------------------------------
int CStringArray_SortInsert(CStringArray& ar, const CString& s, BOOL bNoCase /*= TRUE*/, BOOL bDescending /*= FALSE*/)
{
	if (ar.GetCount() == 0)
		return ar.Add(s);

	for (int i = 0; i <= ar.GetUpperBound(); i++)
	{
		int nCmp = bNoCase ? s.CompareNoCase(ar.GetAt(i)) : s.Compare(ar.GetAt(i));
		if (nCmp == 0)
			return i;

		if (bDescending)
		{
			if (nCmp > 0)
			{
				ar.InsertAt(i, s);
				return i;
			}
		}
		else
		{
			if (nCmp < 0)
			{
				ar.InsertAt(i, s);
				return i;
			}
		}
	}
	return ar.Add(s);
}

//-----------------------------------------------------------------------------
void CStringArray_Concat (const CStringArray& ar, CString& str, LPCTSTR szSep/* = _T(";")*/)
{
	str.Empty();
	for(int i = 0; i <= ar.GetUpperBound(); i++)
	{
		str += ar.GetAt(i);
		if (i < ar.GetUpperBound())
			str += szSep;
	}
}

void CStringArray_ConcatComment	(const CStringArray& ar, CString& str, LPCTSTR szSep, LPCTSTR szPrefix, LPCTSTR szPostfix)
{
	str.Empty();
	for(int i = 0; i <= ar.GetUpperBound(); i++)
	{
		CString s = ar.GetAt(i);

		s.Replace(L"//", L"");
		s.Replace(L"/*", L"");
		s.Replace(L"*/", L"");
		s.TrimRight();
		if (s.IsEmpty()) 
			continue;

		if (!str.IsEmpty())
			str += szSep;

		str += s;
	}
	if (!str.IsEmpty())
		str = szPrefix + str + szPostfix;
}

//-----------------------------------------------------------------------------
int CStringArray_Split(CStringArray& ar, const CString& str, LPCTSTR szSep/* = _T(";")*/, BOOL bAllowDuplicate/* = FALSE*/, BOOL bAllowNULL/* = FALSE*/)
{
	ar.RemoveAll();
	if (str.IsEmpty()) 
		return 0;

	CString stringToSplit = str;
	// gestione del ccaso in cui venga passata una stringa in cui il doppio separetor indica "collonna" vuota

	if (bAllowNULL)
	{
		CString oldDoubledSep = CString(szSep) + CString(szSep);
		CString newDoubledSep = CString(szSep) + _T(" ") + CString(szSep);
		stringToSplit.Replace(oldDoubledSep, newDoubledSep);
	}

	TCHAR* nextToken;
	TCHAR* psz = stringToSplit.GetBuffer();
	
	for 
		(
			TCHAR* pszToken = _tcstok_s(psz, szSep, &nextToken); 
			pszToken; 
			pszToken = _tcstok_s(NULL, szSep, &nextToken) 
		)
	{
		CString s(pszToken); s.Trim();

		if (bAllowDuplicate || CStringArray_Find(ar, s) == -1)
		{
			ar.Add(s);
		}
	}

	stringToSplit.ReleaseBuffer();

	return ar.GetCount();
}
/*
int CStringArray_Split (CStringArray& saItems, const CString& sFrom, LPCTSTR pszTokens)
{
	int i = 0;
	for (CString sItem = sFrom.Tokenize(pszTokens, i); i >= 0; sItem = sFrom.Tokenize(pszTokens, i))
	{
		sItem.Trim();
		saItems.Add(sItem);
	}

	return saItems.GetCount();
}
*/
//-----------------------------------------------------------------------------
void CStringArray_RemoveWhenPrefixed (CStringArray& ar, LPCTSTR szPrefix)
{
	int len = _tcsclen(szPrefix);
	for (int i = ar.GetUpperBound(); i >= 0 ; i--)
	{
		if (_tcsnicmp(szPrefix, (LPCTSTR)ar.GetAt(i), len) == 0)
			ar.RemoveAt(i);
	}
}
//-----------------------------------------------------------------------------
void CStringArray_RemoveWhenNotPrefixed (CStringArray& ar, LPCTSTR szPrefix)
{
	int len = _tcsclen(szPrefix);
	for (int i = ar.GetUpperBound(); i >= 0 ; i--)
	{
		if (_tcsnicmp(szPrefix, ar.GetAt(i), len))
			ar.RemoveAt(i);
	}
}
//-----------------------------------------------------------------------------
void CStringArray_RemovePrefix (CStringArray& ar, LPCTSTR szPrefix)
{
	int len = _tcsclen(szPrefix);
	for (int i = ar.GetUpperBound(); i >= 0 ; i--)
	{
		if (_tcsnicmp(szPrefix, ar.GetAt(i), len) == 0)
		{
			CString s = ar[i].Mid(len); s.Trim();
			//si potrebbe aggiungere il controllo che non sia duplicato, vedremo
			ar[i] = s;
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
