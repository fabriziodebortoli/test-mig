#pragma once
#include "afx.h"
#include <TbGeneric\Array.h>
#include "beginh.dex"

#define ALIAS_IDENTIFIER _T('@')
//===========================================================================
class TB_EXPORT CGenericAlias : public CObject
{
	friend class CAbstractFormDoc;
protected:
	CString m_sAlias;
	CString m_sActual;
public:
	CGenericAlias(const CString& sAlias, const CString& sActual);
};


//===========================================================================
class TB_EXPORT CFieldAlias : public CGenericAlias
{
	friend class CAbstractFormDoc;
public:
	CFieldAlias(const CString& sAlias, const CString& sActual);
};
//===========================================================================
class TB_EXPORT CDataSourceAlias : public CFieldAlias
{
	friend class CAbstractFormDoc;
private:
	Array m_arFields;
public:
	CDataSourceAlias(const CString& sAlias, const CString& sActual);
	void RegisterFieldAlias(CString sAlias, const CString& sActual);
};
#include "endh.dex"