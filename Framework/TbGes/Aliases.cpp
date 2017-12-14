#include "stdafx.h"
#include "Aliases.h"


//----------------------------------------------------------------------------
CGenericAlias::CGenericAlias(const CString& sAlias, const CString& sActual) : m_sAlias(sAlias), m_sActual(sActual)
{
}


//----------------------------------------------------------------------------
CFieldAlias::CFieldAlias(const CString& sAlias, const CString& sActual) : CGenericAlias(sAlias, sActual)
{
}
//----------------------------------------------------------------------------
CDataSourceAlias::CDataSourceAlias(const CString& sAlias, const CString& sActual) : CFieldAlias(sAlias, sActual)
{
}
//----------------------------------------------------------------------------
void CDataSourceAlias::RegisterFieldAlias(CString sAlias, const CString& sActual)
{
	if (sAlias.IsEmpty() || sActual.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}
	if (sAlias[0] != ALIAS_IDENTIFIER)
		sAlias.Insert(0, ALIAS_IDENTIFIER);
	m_arFields.Add(new CFieldAlias(sAlias, sActual));
}