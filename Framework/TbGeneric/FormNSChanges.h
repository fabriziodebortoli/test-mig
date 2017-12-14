#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//----------------------------------------------------------------
// CFormNSChange
//----------------------------------------------------------------
class TB_EXPORT CFormNSChange: public CObject
{
public:
	CString		m_sOld;
	CString		m_sNew;
	DataBool	m_bExactMatch;

public:
	CFormNSChange	(
									CString		sOld,
									CString		aNew,
									DataBool	bExactMatch
								);
};

//----------------------------------------------------------------
// array di CFormNSChange
//----------------------------------------------------------------
class TB_EXPORT CFormNSChangeArray: public Array
{
public:
	CFormNSChangeArray () : m_NsRelease(1) {}

public:
	int	m_NsRelease;
public:
	CFormNSChange* 	GetAt			(int nIndex) const					{ return (CFormNSChange*) Array::GetAt(nIndex);	}
	CFormNSChange*&	ElementAt		(int nIndex)						{ return (CFormNSChange*&) Array::ElementAt(nIndex); }
	int						Add				(CFormNSChange* pObj)		{ return Array::Add(pObj); }
	
	CFormNSChange* 	operator[]		(int nIndex) const					{ return GetAt(nIndex);	}
	CFormNSChange*& operator[]		(int nIndex)						{ return ElementAt(nIndex);	}

	CString		GetNewNS	(CString sOldNS);
};

#include "endh.dex"
