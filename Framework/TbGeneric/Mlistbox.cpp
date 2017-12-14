
#include "stdafx.h"

#include <ctype.h>

#include "dibitmap.h"
#include "GeneralFunctions.h"
#include "mlistbox.h"
#include "globals.h"

#include <TbNameSolver\Chars.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Esegue ricorsivamente una ricerca dicotomica nell'array di stringhe passato
//----------------------------------------------------------------------------
static int GetNewStringPos(const CStringArray& strArray, int nStart, int nEnd, LPCTSTR pszStr)
{
	// nel caso la distanza sia dispari si arrotonda per eccesso
	//
	int nMiddleIdx = (nStart + nEnd + 1) / 2;
	int n = nEnd - nStart;
	
	if (strArray[nMiddleIdx].CompareNoCase(pszStr) > 0)
	{
		// se la distanza e` 0 vuol dire che senza alcun dubbio
		// la nuova posizione e` quella della corrente stringa centrale
		// (vedi commento sotto)
		//
		if (n == 0)
			return nMiddleIdx;

		// se la distanza e` unitaria si e` in pratica confrontato con il
		// secondo estremo, di conseguenza visto che cio` e` stato fatto in
		// maniera arbitraria e` necessario forzare il confronto con il primo
		// estremo prima di prendere la decisione
		//
		if (n == 1)
			nMiddleIdx = nStart;

		return GetNewStringPos(strArray, nStart, nMiddleIdx, pszStr);
	}

	// se la stringa e` maggiore di quella della posizione centrale
	// e la distanza tra gli estremi e` minore o uguale a 1 senzaltro
	// la nuova stringa deve essere posizionata dopo la posizione centrale
	//
	if (n <= 1)
		return nMiddleIdx + 1;
			
	return GetNewStringPos(strArray, nMiddleIdx, nEnd, pszStr);
}

//============================================================================
// CMultiListBox
//============================================================================
IMPLEMENT_DYNAMIC(CMultiListBox, CListBox)

BEGIN_MESSAGE_MAP(CMultiListBox, CListBox)
	//{{AFX_MSG_MAP( CMultiListBox )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// bitmap1 is for flag = TRUE 
// bitmap2 is for flag = FALSE
//----------------------------------------------------------------------------
CMultiListBox::CMultiListBox (UINT nID1, UINT nID2, UINT nID3, UINT nID4, UINT nID5)
	:
	m_pBitmap1		(NULL),
	m_pBitmap2		(NULL),
	m_pBitmap3		(NULL),
	m_pBitmap4		(NULL),
	m_pBitmap5		(NULL),
	m_Style			(UNSORTED)
{
	if (nID1)
		m_pBitmap1 = new CCheckBitmap(nID1);
	
	if (nID2)
		m_pBitmap2 = new CCheckBitmap(nID2);

	if (nID3)
		m_pBitmap3 = new CCheckBitmap(nID3);

	if (nID4)
		m_pBitmap4 = new CCheckBitmap(nID4);

	if (nID5)
		m_pBitmap5 = new CCheckBitmap(nID5);

	// deve essere l'ultima istruzione!
	SetOffsets(0, 0);
}

//----------------------------------------------------------------------------
CMultiListBox::CMultiListBox(CString namespace1, CString namespace2, CString namespace3, CString namespace4, CString namespace5)
	:
	m_pBitmap1(NULL),
	m_pBitmap2(NULL),
	m_pBitmap3(NULL),
	m_pBitmap4(NULL),
	m_pBitmap5(NULL),
	m_Style(UNSORTED)
{
	if (!namespace1.IsEmpty())
		m_pBitmap1 = new CCheckBitmap(namespace1);

	if (!namespace2.IsEmpty())
		m_pBitmap2 = new CCheckBitmap(namespace2);

	if (!namespace3.IsEmpty())
		m_pBitmap3 = new CCheckBitmap(namespace3);

	if (!namespace4.IsEmpty())
		m_pBitmap4 = new CCheckBitmap(namespace4);

	if (!namespace5.IsEmpty())
		m_pBitmap5 = new CCheckBitmap(namespace5);

	// deve essere l'ultima istruzione!
	SetOffsets(0, 0);
}

//----------------------------------------------------------------------------
CMultiListBox::~CMultiListBox ()
{
	if (m_pBitmap1)	delete m_pBitmap1;
	if (m_pBitmap2)	delete m_pBitmap2;
	if (m_pBitmap3)	delete m_pBitmap3;
	if (m_pBitmap4)	delete m_pBitmap4;
	if (m_pBitmap5)	delete m_pBitmap5;
}

//----------------------------------------------------------------------------
void CMultiListBox::SetOffsets(int n1stOffset, int n2ndOffset)
{
	if (m_List1.GetSize() || n1stOffset < 0 || n2ndOffset < 0 || n1stOffset > n2ndOffset)
	{
		ASSERT_TRACE2(m_List1.GetSize() > 0 && n1stOffset >= 0 && n2ndOffset >= 0 && n1stOffset <= n2ndOffset,"bad parameters: n1stOffset = %d, n2ndOffset = %d", n1stOffset, n2ndOffset);
		return;
	}
	
	m_n1stOffset = n1stOffset;
	
	// NB. SI MEMORIZZA < 0 PER SGNALARE CHE E` UN VALORE IMPOSTATO DA FUORI
	m_n2ndOffset = -n2ndOffset;

	if (m_pBitmap1)
		m_n1stOffset = max(m_pBitmap1->GetWidth() + 1, m_n1stOffset);

	if (m_pBitmap2)
		m_n1stOffset = max(m_pBitmap2->GetWidth() + 1, m_n1stOffset);

	if (m_pBitmap3)
		m_n1stOffset = max(m_pBitmap3->GetWidth() + 1, m_n1stOffset);

	if (m_pBitmap4)
		m_n1stOffset = max(m_pBitmap4->GetWidth() + 1, m_n1stOffset);

	if (m_pBitmap5)
		m_n1stOffset = max(m_pBitmap5->GetWidth() + 1, m_n1stOffset);
}

// ritorna la "larghezza" degli eventuali tab in testa alla stringa eliminandoli
// contemporaneamente dalla stessa
//----------------------------------------------------------------------------
int CMultiListBox::GetTabWidth(CString& str)
{
	int ntw = 0;
	
	while (!str.IsEmpty() && str[0] == '\t') // tab
	{
		str = str.Mid(1);	// elimina il tab
		ntw += 15;
	}
	
	return ntw;
}

//----------------------------------------------------------------------------
void CMultiListBox::Recalc2ndOffset(LPCTSTR pszStr)
{
	// se il secondo offset e` < 0 vuol dire che e` stato impostato da fuori
	// (cfr. SetOffsets()) e quindi non lo si deve ricalcolare
	if (m_n2ndOffset >= 0)
	{
		CString str(pszStr);

		// 6 e` lo spazio imposto tra prima e seconda stringa
		int nBase = GetTabWidth(str) + m_n1stOffset + 6;
		CDC* pDC = this->GetDC();
		CSize cs = GetTextSize(pDC, str, GetFont());
		ReleaseDC(pDC);

		m_n2ndOffset = max(m_n2ndOffset, cs.cx + nBase);
	}
}

//----------------------------------------------------------------------------
void CMultiListBox::AddString(int nIdx, LPCTSTR s1, LPCTSTR s2, ItemStatus Flag)
{
	ASSERT_TRACE((GetStyle() & (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS)) == (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS),"Bad style for the listbox");

	m_List1.InsertAt(nIdx, s1);
	m_List2.InsertAt(nIdx, s2);
	m_Flags.InsertAt(nIdx, Flag);

	// per far la ricerca standard da tastiera sulla base del primo carattere
	// si memorizza solo il primo e non tutta stringa
	TCHAR str[2];
	str[0] = s1[0];
	str[1] = NULL_CHAR;
	__super::InsertString(nIdx, str);

	Recalc2ndOffset(s1);
}

//----------------------------------------------------------------------------
int CMultiListBox::AddString(LPCTSTR s1, LPCTSTR s2, ItemStatus Flag)
{
	ASSERT_TRACE((GetStyle() & (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS)) == (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS),"Bad style for the listbox");

	int nPos = m_List1.GetSize();

	int style = m_Style & SORT_ON_BOTH_STRINGS;
	
	if (nPos && style != UNSORTED)
	{
		if (style == SORT_ON_SECOND_STRING)
			nPos = GetNewStringPos(m_List2, 0, m_List2.GetUpperBound(), s2);
		else
		{
			nPos = GetNewStringPos(m_List1, 0, m_List1.GetUpperBound(), s1);

			// dato che l'algoritmo di ricerca nel caso di stringhe uguali
			// da` la posizione dopo a quella della stringa trovata allora
			// non e` necessario effettuare la ricerca per la seconda stringa
			// se la posizione ritornata e` la 0
			if (nPos > 0 && s2 && style == SORT_ON_BOTH_STRINGS)
			{
				int nStart = 0;
				for (nStart = nPos; nStart > 0; nStart--)
					if (m_List1[nStart - 1].CompareNoCase(s1) != 0) break;
			
				if (nStart != nPos)
					nPos = GetNewStringPos(m_List2, nStart, nPos - 1, s2);
			}
		}
	}

	AddString(nPos, s1, s2, Flag);
	
	return nPos;
}

//----------------------------------------------------------------------------
void CMultiListBox::DelString(int nIndex)
{
	m_List1.RemoveAt(nIndex);
	m_List2.RemoveAt(nIndex);
	m_Flags.RemoveAt(nIndex);
		
	__super::DeleteString(nIndex);
	
	// se il secondo offset e` < 0 vuol dire che e` stato impostato da fuori
	// (cfr. SetOffsets()) e quindi cancellando un item non lo si deve ricalcolare
	if (m_n2ndOffset >= 0)
		m_n2ndOffset = 0;
		
	for (int i = 0; i < m_List1.GetSize(); i++)
		Recalc2ndOffset(m_List1[i]);
}

//----------------------------------------------------------------------------
int CMultiListBox::DelString(LPCTSTR pszFirstName, LPCTSTR pszSecondName)
{
	int nIdx = SearchString(pszFirstName, pszSecondName);
	if (nIdx >= 0) 
		DelString(nIdx);
		
	return nIdx;
}

//----------------------------------------------------------------------------
int CMultiListBox::SearchString(LPCTSTR pszFirstName, LPCTSTR pszSecondName)
{
	int nNumItm = m_List1.GetSize();
	for (int i = 0; i < nNumItm; i++)
	{
		if (pszFirstName != NULL && pszSecondName != NULL)
			if (
				m_List1[i].CompareNoCase(pszFirstName) == 0	&&
				m_List2[i].CompareNoCase(pszSecondName) == 0
			   )
            	return i;

		if (pszFirstName != NULL && m_List1[i].CompareNoCase(pszFirstName) == 0)
            return i;

		if (pszSecondName != NULL && m_List2[i].CompareNoCase(pszSecondName) == 0)
            return i;
	}

    return -1;
}


// Derived class is responsible for implementing these handlers
//   for owner/self draw controls (except for the optional DeleteItem)
//	Afx standard behavior do nothing but assert FALSE ??
//
//----------------------------------------------------------------------------
void CMultiListBox::MeasureItem(LPMEASUREITEMSTRUCT)
	{ ASSERT(TRUE); }	 //@@ TODO itri

//----------------------------------------------------------------------------
void CMultiListBox::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	int nIndex = (int) lpDIS->itemID;

	if (nIndex < 0 || nIndex > m_List1.GetUpperBound())	return;

	COLORREF oldBkColor 	= GetBkColor(lpDIS->hDC);
	COLORREF oldTextColor	= GetTextColor(lpDIS->hDC);

	COLORREF bkColor;
	COLORREF textColor;

	// Selezione dei colori in base all'elemento selezionato
	if (lpDIS->itemState & ODS_SELECTED)
	{
		bkColor	= AfxGetThemeManager()->GetControlsHighlightBkgColor();
		textColor	= lpDIS->itemState & ODS_DISABLED
						? AfxGetThemeManager()->GetDisabledControlForeColor()
						: AfxGetThemeManager()->GetControlsHighlightForeColor();
	}
	else
	{
		bkColor	= AfxGetThemeManager()->GetEnabledControlBkgColor();
		textColor	= lpDIS->itemState & ODS_DISABLED
						? AfxGetThemeManager()->GetDisabledControlForeColor()
						: AfxGetThemeManager()->GetEnabledControlForeColor();
  	}
	

	SetBkMode	(lpDIS->hDC, OPAQUE);
	SetBkColor  (lpDIS->hDC, bkColor);
	SetTextColor(lpDIS->hDC, textColor);

	CString s1		= m_List1[nIndex];
	CString s2		= m_List2[nIndex];

	int n1stOffset = m_n1stOffset + GetTabWidth(s1);
	int n2ndOffset = abs(m_n2ndOffset);		// e` < 0 se impostato da fuori (cfr. SetOffsets(..))
	
	RECT rect;
	
	rect.left	= lpDIS->rcItem.left;
	rect.top	= lpDIS->rcItem.top;
	rect.right	= lpDIS->rcItem.left + n1stOffset;
	rect.bottom	= lpDIS->rcItem.bottom;

	// fill all backgrounds with bkColor
	//
	HBRUSH hBrush1 = CreateSolidBrush(bkColor);
	FillRect(lpDIS->hDC, &rect, hBrush1);
	DeleteObject(hBrush1);

	// se ci sono bitmap vengono visualizzati sulla base della variabile wFlag
	if (m_pBitmap1)
		m_pBitmap1->FloodDrawBitmap
			(
				lpDIS->hDC, lpDIS->rcItem.left, lpDIS->rcItem.top, 
				SRCCOPY, textColor, bkColor, m_Flags[nIndex] == CHECK_ONE
			);

	if (m_pBitmap2)
		m_pBitmap2->FloodDrawBitmap
			(
				lpDIS->hDC, lpDIS->rcItem.left, lpDIS->rcItem.top, 
				SRCCOPY, textColor, bkColor, m_Flags[nIndex] == CHECK_TWO
			);

	if (m_pBitmap3)
		m_pBitmap3->FloodDrawBitmap
			(
				lpDIS->hDC, lpDIS->rcItem.left, lpDIS->rcItem.top, 
				SRCCOPY, textColor, bkColor, m_Flags[nIndex] == CHECK_THREE
			);

	if (m_pBitmap4)
		m_pBitmap4->FloodDrawBitmap
			(
				lpDIS->hDC, lpDIS->rcItem.left, lpDIS->rcItem.top, 
				SRCCOPY, textColor, bkColor, m_Flags[nIndex] == CHECK_FOUR
			);

	if (m_pBitmap5)
		m_pBitmap5->FloodDrawBitmap
			(
				lpDIS->hDC, lpDIS->rcItem.left, lpDIS->rcItem.top, 
				SRCCOPY, textColor, bkColor, m_Flags[nIndex] == CHECK_FIVE
			);

	// visualizza la prima stringa
	//
	rect.left	= lpDIS->rcItem.left + n1stOffset;
	if (s2.IsEmpty() || (m_Style & HIDE_SECOND_STRING) == HIDE_SECOND_STRING)
	{
		rect.right	= lpDIS->rcItem.right;
		
		ExtTextOut(
					lpDIS->hDC,
					rect.left,
					rect.top,
					ETO_OPAQUE,
					&rect,
					s1, s1.GetLength(),
					(LPINT) NULL
				  );
	}
	else 
	{
		rect.right	= lpDIS->rcItem.left + n2ndOffset - 6;

		ExtTextOut(
					lpDIS->hDC,
					rect.left,
					rect.top,
					ETO_OPAQUE,
					&rect,
					s1, s1.GetLength(),
					(LPINT) NULL
				  );

		// cancella l'interspazio tra prima e seconda stringa
		//
		rect.left	= lpDIS->rcItem.left + n2ndOffset - 6;
		rect.right	= lpDIS->rcItem.left + n2ndOffset;
	
		ExtTextOut(
					lpDIS->hDC,
					rect.left,
					rect.top,
					ETO_OPAQUE,
					&rect,
					_T(""), 0,
					(LPINT) NULL
				  );

		// visualizza la seconda stringa
		//
		rect.left	= lpDIS->rcItem.left + n2ndOffset;
		rect.right	= lpDIS->rcItem.right;
	
		ExtTextOut(
					lpDIS->hDC,
					rect.left,
					rect.top,
					ETO_OPAQUE,
					&rect,
					s2, s2.GetLength(),
	                (LPINT) NULL
				  );
	}

	SetBkColor	(lpDIS->hDC, oldBkColor);
	SetTextColor(lpDIS->hDC, oldTextColor);

	if (lpDIS->itemState & ODS_FOCUS)
		DrawFocusRect (lpDIS->hDC, &(lpDIS->rcItem));
}

//----------------------------------------------------------------------------
void CMultiListBox::ResetContent()
{
	m_List1.RemoveAll();
	m_List2.RemoveAll();
	m_Flags.RemoveAll();
	__super::ResetContent();
}

//----------------------------------------------------------------------------
void CMultiListBox::SetAllFlags(ItemStatus Flag)
{
	int numItems = m_Flags.GetSize();
	for (int i = 0; i < numItems; i++)
		m_Flags[i] = (WORD) Flag;

	SetRedraw(TRUE);
	Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CMultiListBox::SetFlag(int nIndex, ItemStatus Flag)
{
	if (nIndex >= 0 && nIndex <= m_Flags.GetUpperBound())
		m_Flags.SetAt(nIndex, Flag);

	SetRedraw(TRUE);
	Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CMultiListBox::SetFlag(LPCTSTR pszFirstName, LPCTSTR pszSecondName, ItemStatus Flag)
{
	int nIdx = SearchString(pszFirstName, pszSecondName);
    if (nIdx >= 0) 
    	SetFlag(nIdx, Flag);
}

//----------------------------------------------------------------------------
CMultiListBox::ItemStatus CMultiListBox::GetFlag(int nIndex)
{
	if (nIndex >= 0 && nIndex <= m_Flags.GetUpperBound())
		return (ItemStatus) m_Flags[nIndex];

	return UNCHECKED;
}

//----------------------------------------------------------------------------
CString CMultiListBox::GetString1(int nIndex) const
{
	if (nIndex >= 0 && nIndex <= m_List1.GetUpperBound())
		return m_List1[nIndex];

	return CString("");
}

//----------------------------------------------------------------------------
CString CMultiListBox::GetString2(int nIndex) const
{
	if (nIndex >= 0 && nIndex <= m_List2.GetUpperBound())
		return m_List2[nIndex];

	return CString("");
}

