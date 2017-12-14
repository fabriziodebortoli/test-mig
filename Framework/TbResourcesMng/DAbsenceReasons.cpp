
#include "stdafx.h"  

#include "DAbsenceReasons.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szP1[] = _T("p1");

//////////////////////////////////////////////////////////////////////////////
//                 class DBTAbsenceReasons implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTAbsenceReasons, DBTMaster)

//-----------------------------------------------------------------------------	
DBTAbsenceReasons::DBTAbsenceReasons
	(
	CRuntimeClass*		pClass, 
	CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster(pClass, pDocument, _NS_DBT("AbsenceReasons"))
{
}

//-----------------------------------------------------------------------------
void DBTAbsenceReasons::OnDisableControlsForEdit()
{ 
	GetAbsenceReasons()->f_Reason.SetReadOnly();
}

//-----------------------------------------------------------------------------	
void DBTAbsenceReasons::OnPrepareBrowser(SqlTable* pTable)
{
	TAbsenceReasons* pRec = (TAbsenceReasons*)pTable->GetRecord();

	pTable->SelectAll();
	pTable->AddSortColumn(pRec->f_Reason);
}

// Serve a definire sia i criteri di sort (ORDER BY chiave primaria in questo caso)
// ed i criterio di filtraggio (WHERE)
// La routine parent deve essere chiamata perche inizializza il vettore di parametri
//-----------------------------------------------------------------------------
void DBTAbsenceReasons::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szP1, GetAbsenceReasons()->f_Reason);
	m_pTable->AddFilterColumn	(GetAbsenceReasons()->f_Reason);
}

// Serve a valorizzare i parametri di query.
//-----------------------------------------------------------------------------
void DBTAbsenceReasons::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1, GetAbsenceReasons()->f_Reason);
}

// Forza il programmatore a controllare che i campi dell'indice primario 
// (PRIMARY INDEX) siano stati valorizzati correttamente onde non archiviare
// records non piu` rintracciabili
//-----------------------------------------------------------------------------
BOOL DBTAbsenceReasons::OnCheckPrimaryKey()
{ 
	return
		!GetAbsenceReasons()->f_Reason.IsEmpty() ||
		!SetError(_TB("Reason Code is mandatory"));
}

//////////////////////////////////////////////////////////////////////////////
//                 class DAbsenceReasons implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DAbsenceReasons, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DAbsenceReasons, CAbstractFormDoc)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
inline TAbsenceReasons* DAbsenceReasons::GetAbsenceReasons() const { return (TAbsenceReasons*)m_pDBTAbsenceReasons->GetRecord(); }

//-----------------------------------------------------------------------------
DAbsenceReasons::DAbsenceReasons()
	:
	m_pDBTAbsenceReasons(NULL)
{}

//----------------------------------------------------------------------------- 
BOOL DAbsenceReasons::OnAttachData()
{                                                   
	SetFormTitle(_TB("Absence Reasons"));
	
	m_pDBTAbsenceReasons = new DBTAbsenceReasons(RUNTIME_CLASS(TAbsenceReasons), this);

	Attach(new CImportAbsenceReasons());
	return Attach(m_pDBTAbsenceReasons);
}

/////////////////////////////////////////////////////////////////////////////
// 						CImportAbsenceReasons
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
BEGIN_TB_EVENT_MAP(CImportAbsenceReasons)
END_TB_EVENT_MAP

//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CImportAbsenceReasons, CXMLEventManager);
