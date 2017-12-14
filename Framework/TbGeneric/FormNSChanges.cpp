#include "stdafx.h"

#include <io.h>

#include "FormNSChanges.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//						CFormNSChange
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CFormNSChange::CFormNSChange	(
									CString		sOld,
									CString		sNew,
									DataBool	bExactMatch
								)
	:
	m_sOld			(sOld),
	m_sNew			(sNew),
	m_bExactMatch	(bExactMatch)
{}

/////////////////////////////////////////////////////////////////////////////
//						CFormNSChangeArray
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
// GetNewNS
//    converte, se esiste nella tabella di lookup,  il namespace passato
//    se non esiste ritorna il valore di ingresso.
//
CString CFormNSChangeArray::GetNewNS(CString sOldNS)
{
	for (int i=0; i <= GetUpperBound(); i++)
	{
		CFormNSChange* pFormNSChange = GetAt(i);
		if (pFormNSChange)
		{
			if (pFormNSChange->m_bExactMatch)
			{
				if (pFormNSChange->m_sOld.CompareNoCase(sOldNS) == 0)
					return pFormNSChange->m_sNew;
			}
			else
			{
				if (sOldNS.FindOneOf(pFormNSChange->m_sOld) >= 0)
				{
					CString sRes = sOldNS;
					sRes.Replace(pFormNSChange->m_sOld, pFormNSChange->m_sNew);
					return sRes;
				}
			}
		}
	}

	return sOldNS;
}