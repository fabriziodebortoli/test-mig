#include "stdafx.h"  

//Local declarations
#include "TAbsenceReasons.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szP1[]= _T("p1");

//=============================================================================
//				###	Causali Assenza ###					
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TAbsenceReasons, SqlRecord)

//-----------------------------------------------------------------------------
TAbsenceReasons::TAbsenceReasons(BOOL bCallInit  /* = TRUE */)
	:
	SqlRecord (GetStaticName())
{
	//segmenti di primary key:
	f_Reason.SetUpperCase();

	BindRecord();
	if (bCallInit) Init();  
}

//-----------------------------------------------------------------------------
void TAbsenceReasons::BindRecord()
{
	BEGIN_BIND_DATA	();
	BIND_DATA	(_NS_FLD("Reason"),			f_Reason);
	BIND_DATA	(_NS_FLD("Description"),	f_Description);
	BIND_DATA	(_NS_FLD("Notes"),			f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TAbsenceReasons::GetStaticName() { return _NS_TBL("RM_AbsenceReasons"); }

/////////////////////////////////////////////////////////////////////////////
//		HotLink:		 Causali Assenza
/////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(HKLAbsenceReasons, HotKeyLink)

//------------------------------------------------------------------------------
HKLAbsenceReasons::HKLAbsenceReasons()
	: 
	HotKeyLink(RUNTIME_CLASS(TAbsenceReasons), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.AbsenceReasons"))
{
}

//------------------------------------------------------------------------------
void HKLAbsenceReasons::OnDefineQuery(SelectionType nQuerySelection)
{
	m_pTable->SelectAll	();

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn	(GetRecord()->f_Reason);
			m_pTable->AddParam			(szP1, GetRecord()->f_Reason);
			break;
			
		case UPPER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Reason);
			m_pTable->AddFilterLike		(GetRecord()->f_Reason);
			m_pTable->AddParam			(szP1, GetRecord()->f_Reason);
			break;

		case LOWER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Description);
			m_pTable->AddFilterLike		(GetRecord()->f_Description);
			m_pTable->AddParam			(szP1, GetRecord()->f_Description);
			break;
	}
}
	
//------------------------------------------------------------------------------
void HKLAbsenceReasons::OnPrepareQuery(DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->SetParamValue(szP1, *pDataObj);
			break;
			
		case UPPER_BUTTON:
		case LOWER_BUTTON:
			m_pTable->SetParamLike(szP1, *pDataObj);
			break;
	}
}