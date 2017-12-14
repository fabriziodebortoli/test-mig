
#include "stdafx.h"

#include "CheckRecursion.h"

/////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[]	= __FILE__;     
#endif                                

/////////////////////////////////////////////////////////////////////////////
//				class CCheckRecursion implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CElemRecursion, CObject)

//----------------------------------------------------------------------------
CElemRecursion::CElemRecursion(const CString& strKey, int	nLev)
	:	
	m_strsKey	(strKey),
	m_nLev		(nLev)
{}

/////////////////////////////////////////////////////////////////////////////
//				class CCheckRecursion implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CCheckRecursion, CObject)

//----------------------------------------------------------------------------
CCheckRecursion::CCheckRecursion()
{}

//----------------------------------------------------------------------------
CCheckRecursion::~CCheckRecursion()
{
	Clear();
}

//----------------------------------------------------------------------------
void CCheckRecursion::Clear()
{
	CElemRecursion* pElem;
	
	while (!m_Elements.IsEmpty())
	{
		pElem = m_Elements.RemoveHead();
		delete pElem;
	}
}

//----------------------------------------------------------------------------
void CCheckRecursion::Add(const CString& strKey, int nLev)
{
	m_Elements.AddHead(new CElemRecursion(strKey, nLev));
}

//----------------------------------------------------------------------------
BOOL CCheckRecursion::IsRecursive(const CString& strKey, int nLev)
{
	CElemRecursion* pElem;
	// rimuove tutti gli elementi di livello inferiore (m_nLiv piu' alto) o pari
	while	(
				!m_Elements.IsEmpty() &&
				m_Elements.GetHead()->m_nLev >= nLev
			)
	{
		pElem = m_Elements.RemoveHead();
		delete pElem;
	}

	for (POSITION aPos = m_Elements.GetHeadPosition(); aPos != NULL;)
		if (m_Elements.GetNext(aPos)->m_strsKey == strKey)
			return TRUE;

	return FALSE;
}

//----------------------------------------------------------------------------
void CCheckRecursion::Add(const DataLng& aID, const DataInt& aLev)
{
	Add(aID.Str(), aLev);
}

//----------------------------------------------------------------------------
BOOL CCheckRecursion::IsRecursive(const DataLng& aID, int nLev)
{
	return IsRecursive(aID.Str(), nLev);
}

//----------------------------------------------------------------------------
void CCheckRecursion::Add
	(
		const DataEnum& aEnum,
		const DataStr&	aKey1,	
		const DataStr&	aKey2,	
		const DataInt&	aLev
	)
{
	Add(aEnum.Str() + aKey1.Str() + aKey2.Str(), aLev);
}

//----------------------------------------------------------------------------
BOOL CCheckRecursion::IsRecursive
	(
		const	DataEnum&	aEnum,
		const	DataStr&	aKey1, 
		const	DataStr&	aKey2,
				int			nLev
	)
{
	return IsRecursive(aEnum.Str() + aKey1.Str() + aKey2.Str(), nLev);
}

//----------------------------------------------------------------------------
void CCheckRecursion::Add(const DataStr& aKeyType1, const DataStr& aKey1, const DataStr& aKeyType2, const DataStr& aKey2, const DataInt& aLev)
{
	Add(aKeyType1.Str() + aKey1.Str() + aKeyType2.Str() + aKey2.Str(), aLev);
}

//----------------------------------------------------------------------------
BOOL CCheckRecursion::IsRecursive(const DataStr& aKeyType1, const DataStr& aKey1, const DataStr& aKeyType2, const DataStr& aKey2, int nLev)
{
	return IsRecursive(aKeyType1.Str() + aKey1.Str() + aKeyType2.Str() + aKey2.Str(), nLev);
}
