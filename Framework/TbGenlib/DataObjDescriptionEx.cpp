#include "stdafx.h"
#include "DataObjDescriptionEx.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// This class derive from CDataObjDescription class, which is in TbGeneric.
// CDataObjDescriptionExpr cannot belong in Tbgeneric, due to need to know Expression (which is in Tbgenlib), 
// because add a reference to TbGenlib should be created a circular dependency.

//----------------------------------------------------------------------------------------------
//	class CDataObjDescriptionExpr implementation
//----------------------------------------------------------------------------------------------

IMPLEMENT_DYNCREATE(CDataObjDescriptionExpr, CDataObjDescription)

//----------------------------------------------------------------------------------------------
CDataObjDescriptionExpr::CDataObjDescriptionExpr ()
	:
	CDataObjDescription				()
{
	m_sExprValue = _T("");
	m_pExpr = NULL;
}

//-----------------------------------------------------------------------------
CDataObjDescriptionExpr::CDataObjDescriptionExpr(const CDataObjDescriptionExpr& dd)
{ 
	Assign(dd); 
}

//----------------------------------------------------------------------------------------------
CDataObjDescriptionExpr::~CDataObjDescriptionExpr ()
{
	SAFE_DELETE(m_pExpr);
}

//----------------------------------------------------------------------------------------------
BOOL CDataObjDescriptionExpr::Parse (CXMLNode* pNode, BOOL bWithValues /*TRUE*/)
{
	BOOL bRet =  __super::Parse(pNode, bWithValues);
	
	if (!bRet)
		return FALSE;

	
	CString sTemp;
	
	pNode->GetAttribute (_T("expr"), sTemp);
	if (!sTemp.IsEmpty())
		m_sExprValue = sTemp;

	return TRUE;
}

//----------------------------------------------------------------------------------------------
const CString&	CDataObjDescriptionExpr::GetStringExpr() const
{
	return m_sExprValue;
}
//----------------------------------------------------------------------------------------------
Expression*	CDataObjDescriptionExpr::GetExpr() const
{
	return m_pExpr;
}

//-----------------------------------------------------------------------------
DataObj* CDataObjDescriptionExpr::Eval()
{
	if (GetStringExpr().IsEmpty())
		return GetValue();

	// valuto l'espressione
	GetExpr()->Eval(*GetValue());
	
	return GetValue();
}

//--------------------------------------------------------------------
BOOL CDataObjDescriptionExpr::IsEqual (const CDataObjDescriptionExpr& dd)
{
	return 	CDataObjDescription::IsEqual(dd) && 
			(m_sExprValue.Compare(dd.GetStringExpr()) == 0) &&
			m_pExpr->IsEqual(*(dd.GetExpr()));
}

//----------------------------------------------------------------------------
CDataObjDescriptionExpr& CDataObjDescriptionExpr::Assign (const CDataObjDescriptionExpr& dd)
{
	CDataObjDescription::Assign(dd);
	m_sExprValue = dd.GetStringExpr();
	
	if (dd.GetExpr() != NULL)
		m_pExpr->Assign(*(dd.GetExpr()));
	else
		m_pExpr = NULL;

	return *this;
}

//----------------------------------------------------------------------------
BOOL CDataObjDescriptionExpr::InitializeExpr(SymTable* pSymTable)
{
	Parser		lex	(GetStringExpr());
	m_pExpr = new Expression(pSymTable);
	return m_pExpr->Parse(lex, GetDataType(), FALSE);
}

//----------------------------------------------------------------------------
CBaseDescription* CDataObjDescriptionExpr::Clone()
{
	return new CDataObjDescriptionExpr(*this);;
}