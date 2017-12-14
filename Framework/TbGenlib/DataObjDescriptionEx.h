#pragma once

#include <TbGeneric\DataObjDescription.h>
#include <TbParser\SymTable.h>

#include "Expr.h"

#include "Beginh.dex"

//----------------------------------------------------------------
class TB_EXPORT CDataObjDescriptionExpr : public CDataObjDescription
{
	DECLARE_DYNCREATE(CDataObjDescriptionExpr)

protected:
	CString			m_sExprValue;
	::Expression*		m_pExpr;

public:
	CDataObjDescriptionExpr ();
	CDataObjDescriptionExpr		(const CDataObjDescriptionExpr& dd);
	~CDataObjDescriptionExpr	();

	
			BOOL		Parse			(CXMLNode*, BOOL bWithValues = TRUE);
	const	CString&	GetStringExpr	() const;
			::Expression*	GetExpr			() const;
			DataObj*	Eval			();

	CDataObjDescriptionExpr&	Assign			(const CDataObjDescriptionExpr& dd);
	BOOL						IsEqual			(const CDataObjDescriptionExpr& dd);
	BOOL						InitializeExpr	(SymTable* pSymTable);

	virtual CBaseDescription*	Clone	();
};
#include "Endh.dex"