
#include "stdafx.h"  

//Local declarations
#include "TCalendars.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szP1[]	= _T("p1");

//=============================================================================
//						###	CALENDARI ###					
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE( TCalendars, SqlRecord ) 

//-----------------------------------------------------------------------------
TCalendars::TCalendars(BOOL bCallInit)
	:
	SqlRecord (GetStaticName())
{
	//segmenti di primary key:
	f_Calendar.SetUpperCase(); 

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TCalendars::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("Calendar"),				f_Calendar);
		BIND_DATA	(_NS_FLD("Description"),			f_Description);
		BIND_DATA	(_NS_FLD("ExcludedDays"),			f_ExcludedDays);
		BIND_DATA	(_NS_FLD("ExcludedMonths"),			f_ExcludedMonths);
		BIND_DATA	(_NS_FLD("ShiftDays"),				f_ShiftDays);
		BIND_DATA	(_NS_FLD("MoveShiftsOnExclDays"),	f_MoveShiftsOnExclDays);
		BIND_DATA	(_NS_FLD("Notes"),					f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TCalendars::GetStaticName() { return _NS_TBL("RM_Calendars"); }

//=============================================================================
//			###	Date giorni non lavorativi per calendario ###
//=============================================================================
//
IMPLEMENT_DYNCREATE(TCalendarsHolidays, SqlRecord)

//-----------------------------------------------------------------------------
TCalendarsHolidays::TCalendarsHolidays(BOOL bCallInit)
	:
	SqlRecord (GetStaticName())
{
	f_StartingDay.	SetFullDate();
	f_EndingDay.	SetFullDate();
	f_Calendar.		SetUpperCase();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TCalendarsHolidays::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("Calendar"),			f_Calendar);
		BIND_DATA	(_NS_FLD("Line"),				f_LineNo);
		BIND_DATA	(_NS_FLD("StartingDay"),		f_StartingDay);
		BIND_DATA	(_NS_FLD("EndingDay"),			f_EndingDay);
		BIND_DATA	(_NS_FLD("ReasonOfExclusion"),	f_ReasonOfExclusion);
		BIND_DATA	(_NS_FLD("Notes"),				f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TCalendarsHolidays::GetStaticName() { return _NS_TBL("RM_CalendarsHolidays"); }

//=============================================================================
//						Turni
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TCalendarsShifts, SqlRecord) 
     
//-----------------------------------------------------------------------------
TCalendarsShifts::TCalendarsShifts(BOOL bCallInit)
	:
	SqlRecord (GetStaticName())
{
	f_Calendar.SetUpperCase();

	BindRecord();	
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TCalendarsShifts::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("Calendar"),		f_Calendar);
		BIND_DATA	(_NS_FLD("Line"),			f_Line);
		BIND_DATA	(_NS_FLD("DayNo"),			f_DayNo);
		BIND_DATA	(_NS_FLD("StartingHour"),	f_StartingHour);    
		BIND_DATA	(_NS_FLD("StartingMinute"),	f_StartingMinute);    
		BIND_DATA	(_NS_FLD("EndingHour"),		f_EndingHour);      
		BIND_DATA	(_NS_FLD("EndingMinute"),	f_EndingMinute);      
		BIND_DATA	(_NS_FLD("Notes"),			f_Notes);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TCalendarsShifts::GetStaticName() { return _NS_TBL("RM_CalendarsShifts"); }

////////////////////////////////////////////////////////////////////////////////
//	HotKeyLink					## Calendars ##
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (HKLCalendars, HotKeyLink)

//------------------------------------------------------------------------------
HKLCalendars::HKLCalendars() 
	: 
	HotKeyLink(RUNTIME_CLASS(TCalendars), _NS_DOC("Framework.TbResourcesMng.TbResourcesMng.Calendars"))
{}

//------------------------------------------------------------------------------
void HKLCalendars::OnDefineQuery (SelectionType nQuerySelection)
{
	m_pTable->SelectAll();
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn	(GetRecord()->f_Calendar);
			m_pTable->AddParam			(szP1, GetRecord()->f_Calendar);
			break;
			
		case UPPER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Calendar);
			m_pTable->AddFilterLike		(GetRecord()->f_Calendar);
			m_pTable->AddParam			(szP1, GetRecord()->f_Calendar);
			break;

		case LOWER_BUTTON:
			m_pTable->AddSortColumn		(GetRecord()->f_Description);
			m_pTable->AddFilterLike		(GetRecord()->f_Description);
			m_pTable->AddParam			(szP1, GetRecord()->f_Description);
			break;
	}
}

//------------------------------------------------------------------------------
void HKLCalendars::OnPrepareQuery (DataObj* pDataObj, SelectionType nQuerySelection)
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
