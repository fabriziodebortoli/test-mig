                          
#include "stdafx.h"

#include <TbWoormViewer\WOORMDOC.H>

#include "DArrangements.h"
#include "UIArrangements.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//Parametri per le query
static TCHAR szParamArrangements[] = _T("p1");

//////////////////////////////////////////////////////////////////////////////
//          class DBTArrangements implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTArrangements, DBTMaster)

//-----------------------------------------------------------------------------	
DBTArrangements::DBTArrangements
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
DBTMaster ( pClass, pDocument, _NS_DBT("Arrangements") )
{}

// obbligatoria in quanto pure-virtual
//-----------------------------------------------------------------------------
void DBTArrangements::OnDefineQuery ()
{
	m_pTable->SelectAll	();
	m_pTable->AddParam			(szParamArrangements, GetArrangements()->f_Arrangements);
	m_pTable->AddFilterColumn	(GetArrangements()->f_Arrangements);
}

// obbligatoria in quanto pure-virtual
//-----------------------------------------------------------------------------
void DBTArrangements::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamArrangements, GetArrangements()->f_Arrangements);
}

// obbligatoria in quanto pure-virtual
//-----------------------------------------------------------------------------
BOOL DBTArrangements::OnCheckPrimaryKey	()
{
	return  !GetArrangements()->f_Arrangements.IsEmpty() ||
			!SetError(_TB("The Arrangement code is mandatory"));
}

//-----------------------------------------------------------------------------
void DBTArrangements::OnDisableControlsForEdit ()
{
	GetArrangements()->f_Arrangements.SetReadOnly();
}	

//-----------------------------------------------------------------------------	
void DBTArrangements::OnPrepareBrowser (SqlTable* pTable)
{   
	TArrangements* pRec = (TArrangements*) pTable->GetRecord();
	
	pTable->SelectAll();
	pTable->AddSortColumn(pRec->f_Arrangements);
}

//////////////////////////////////////////////////////////////////////////////
//                    class DArrangements implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DArrangements, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DArrangements, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DArrangements)
	//}}AFX_MSG_MAP											
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DArrangements::DArrangements()
	:
	m_pDBTArrangements(NULL)
{
}

//-----------------------------------------------------------------------------
TArrangements* DArrangements::GetArrangements() const { return (TArrangements*)m_pDBTArrangements->GetRecord();}

//-----------------------------------------------------------------------------
BOOL DArrangements::OnAttachData()
{                                                   
	SetFormTitle(_TB("Arrangements"));

	m_pDBTArrangements = new DBTArrangements(RUNTIME_CLASS(TArrangements), this);

	return Attach(m_pDBTArrangements);
}

//-----------------------------------------------------------------------------
BOOL DArrangements::OnRunReport(CWoormInfo* pWoormInfo)
{
	if (!pWoormInfo)
		return TRUE;

	pWoormInfo->AddParam(_NS_WRMVAR("w_AskDialog"),				&DataBool(FALSE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_UseArrangementsFilter"),	&DataBool(TRUE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_ArrangementsFilter"),	&GetArrangements()->f_Arrangements); 
	
	return TRUE;
}
