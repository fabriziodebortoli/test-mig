
#include "stdafx.h"  

#include <TbNameSolver\MacroToRedifine.h>

#include "TCalendars.h"
#include "TResources.h"

#include "DCalendars.h"

#include "ModuleObjects\Calendars\JsonForms\IDD_CALENDARS.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szParamCalendar[] =_T("p1");

///////////////////////////////////////////////////////////////////////////////////////////
// VCalendarWorkingPeriod (SqlRecord virtuale per i BodyEdit con i Working days / months //
///////////////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VCalendarWorkingPeriod, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VCalendarWorkingPeriod::VCalendarWorkingPeriod(BOOL bCallInit)
	:
	SqlVirtualRecord(GetStaticName())
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VCalendarWorkingPeriod::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_STR	(_NS_LFLD("VCalendarWorkingPeriod_p1"), l_Period,	LEN_RM_DESCRIPTION);
		LOCAL_DATA	(_NS_LFLD("VCalendarWorkingPeriod_p2"), l_IsEnabled);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VCalendarWorkingPeriod::GetStaticName() { return _NS_TBL("VCalendarWorkingPeriod"); }

//////////////////////////////////////////////////////////////////////////////
//             class DBTCalendars implementation
//////////////////////////////////////////////////////////////////////////////
//                                                                          
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTCalendars, DBTMaster)

//-----------------------------------------------------------------------------	
DBTCalendars::DBTCalendars(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument)
	:
	DBTMaster(pClass, pDocument, _NS_DBT("Calendars"))
{
}

//-----------------------------------------------------------------------------
void DBTCalendars::Init() 
{
	DBTMaster::Init();
}

//-----------------------------------------------------------------------------
void DBTCalendars::OnDisableControlsForEdit()
{ 
	GetCalendars()->f_Calendar.SetReadOnly();
}

//-----------------------------------------------------------------------------	
void DBTCalendars::OnPrepareBrowser (SqlTable* pTable)
{   
	TCalendars* pRec = (TCalendars*) pTable->GetRecord();

	pTable->SelectAll();
	pTable->AddSortColumn(pRec->f_Calendar);
}

// Serve a definire sia i criteri di sort (ORDER BY chiave primaria in questo caso)
// ed i criterio di filtraggio (WHERE)
// La routine parent deve essere chiamata perche inizializza il vettore di parametri
//-----------------------------------------------------------------------------
void DBTCalendars::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamCalendar, GetCalendars()->f_Calendar);
	m_pTable->AddFilterColumn	(GetCalendars()->f_Calendar);
}

// Serve a valorizzare i parametri di query. In questo caso utilizza il codice 
// titolo.
//-----------------------------------------------------------------------------
void DBTCalendars::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamCalendar, 	GetCalendars()->f_Calendar);
}

// Forza il programmatore a controllare che i campi dell'indice primario 
// (PRIMARY INDEX) siano stati valorizzati correttamente onde non archiviare
// records non piu` rintracciabili
//
//-----------------------------------------------------------------------------
BOOL DBTCalendars::OnCheckPrimaryKey()
{
	if (GetCalendars()->f_Calendar.IsEmpty())
	{
		SetError(_TB("The calendar code is mandatory"));
		return FALSE;
	}
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//				DBTCalendarWorkingDays
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTCalendarWorkingDays, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTCalendarWorkingDays::DBTCalendarWorkingDays
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("WorkingDays"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTCalendarWorkingDays::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTCalendarWorkingDays::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VCalendarWorkingPeriod)));
}

//-----------------------------------------------------------------------------
void DBTCalendarWorkingDays::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

//////////////////////////////////////////////////////////////////////////////
//				DBTCalendarWorkingMonths
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTCalendarWorkingMonths, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTCalendarWorkingMonths::DBTCalendarWorkingMonths
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("WorkingMonths"), ALLOW_EMPTY_BODY, FALSE)
{}

//-----------------------------------------------------------------------------
DataObj* DBTCalendarWorkingMonths::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTCalendarWorkingMonths::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VCalendarWorkingPeriod)));
}

//-----------------------------------------------------------------------------
void DBTCalendarWorkingMonths::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

//////////////////////////////////////////////////////////////////////////////
//					class DBTCalendarHolidays implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTCalendarHolidays, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTCalendarHolidays::DBTCalendarHolidays(CRuntimeClass*	pClass, CAbstractFormDoc* pDocument)
	:
	DBTSlaveBuffered (pClass, pDocument,_NS_DBT("CalendarHolidays"), ALLOW_EMPTY_BODY, TRUE)
{}

//-----------------------------------------------------------------------------
TCalendarsHolidays* DBTCalendarHolidays::GetRecordCurrent() const
{ 
	ASSERT(!GetCurrentRow() || GetCurrentRow()->IsKindOf(RUNTIME_CLASS(TCalendarsHolidays)));
	return (TCalendarsHolidays*)GetCurrentRow();
}     

//-----------------------------------------------------------------------------
TCalendars* DBTCalendarHolidays::GetCalendars() const { return GetDocument()->GetCalendars(); }  

//-----------------------------------------------------------------------------
void DBTCalendarHolidays::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamCalendar, GetCalendarHolidays()->f_Calendar);
	m_pTable->AddFilterColumn	(GetCalendarHolidays()->f_Calendar);
	m_pTable->AddSortColumn		(GetCalendarHolidays()->f_StartingDay);
}

//-----------------------------------------------------------------------------
void DBTCalendarHolidays::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamCalendar, GetCalendars()->f_Calendar);
}

//-----------------------------------------------------------------------------
void DBTCalendarHolidays::OnPrepareRow(int nRow, SqlRecord* pSqlRec)
{
}

//-----------------------------------------------------------------------------	
DataObj* DBTCalendarHolidays::OnCheckPrimaryKey(int /* nRow */, SqlRecord* /*pSqlRec*/)
{   
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTCalendarHolidays::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{ 
	ASSERT	(pSqlRec->IsKindOf(RUNTIME_CLASS(TCalendarsHolidays)));    
	TCalendarsHolidays*	pRec = (TCalendarsHolidays*) pSqlRec;

	pRec->f_Calendar 		= GetCalendars()->f_Calendar;
	pRec->f_LineNo 			= nRow + 1;
}

//-----------------------------------------------------------------------------	
DataObj* DBTCalendarHolidays::GetDuplicateKeyPos(SqlRecord* pRec)
{
	return FALSE;
}

//-----------------------------------------------------------------------------	
DataObj* DBTCalendarHolidays::OnCheckUserData(int nRow)
{
	TCalendarsHolidays* pRec = GetCalendarHolidays(nRow);

	pRec->SetStorable(!pRec->f_StartingDay.IsEmpty());
	return NULL;
}

//////////////////////////////////////////////////////////////////////////////
//				class DBTCalendarShifts implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTCalendarShifts, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTCalendarShifts::DBTCalendarShifts(CRuntimeClass* pClass, CAbstractFormDoc* pDocument)
	:
	DBTSlaveBuffered (pClass, pDocument,_NS_DBT("CalendarShifts"), ALLOW_EMPTY_BODY, TRUE)
{}

//-----------------------------------------------------------------------------
TCalendarsShifts* DBTCalendarShifts::GetRecordCurrent() const
{ 
	ASSERT(!GetCurrentRow() || GetCurrentRow()->IsKindOf(RUNTIME_CLASS(TCalendarsShifts)));
	return (TCalendarsShifts*) GetCurrentRow(); 
}     

//-----------------------------------------------------------------------------
TCalendars* DBTCalendarShifts::GetCalendars() const { return GetDocument()->GetCalendars(); }

//-----------------------------------------------------------------------------
void DBTCalendarShifts::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamCalendar, GetCalendarShifts()->f_Calendar);
	m_pTable->AddFilterColumn	(GetCalendarShifts()->f_Calendar);

	m_pTable->AddSortColumn		(GetCalendarShifts()->f_DayNo);
	m_pTable->AddSortColumn		(GetCalendarShifts()->f_StartingHour); // mettere poi il dataTime
}

//-----------------------------------------------------------------------------
void DBTCalendarShifts::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamCalendar, GetCalendars()->f_Calendar);
}

//-----------------------------------------------------------------------------
void DBTCalendarShifts::OnPrepareRow(int nRow, SqlRecord* pSqlRec)
{
	TCalendarsShifts*	pRec	= (TCalendarsShifts*) pSqlRec;
	TCalendars*			pRecCal = GetCalendars();  

	pRec->f_EndingHour = 23;
	pRec->f_EndingMinute = 59;

	if	(nRow < pRecCal->f_ShiftDays) 
		pRec->f_DayNo = nRow + 1;
	else
		pRec->f_DayNo = pRecCal->f_ShiftDays;
}

//-----------------------------------------------------------------------------	
DataObj* DBTCalendarShifts::OnCheckPrimaryKey(int /* nRow */, SqlRecord* /*pSqlRec*/)
{   
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTCalendarShifts::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{ 
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TCalendarsShifts)));    
	TCalendarsShifts* pRec = (TCalendarsShifts*) pSqlRec;

	pRec->f_Calendar = GetCalendars()->f_Calendar;
	pRec->f_Line 	= nRow + 1;
}

//-----------------------------------------------------------------------------	
DataObj* DBTCalendarShifts::GetDuplicateKeyPos(SqlRecord* /*pRec*/)
{
	return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//	DCalendars
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DCalendars, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DCalendars, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DCalendars)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_HOLIDAYS_START_DATE,	OnHolidayStartDateChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_HOLIDAYS_END_DATE,	OnHolidayEndDateChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_DAY_SHIFTSNR,			OnDayShiftNrChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_SHIFTS_HOUR_START,	OnShiftHourStartChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_SHIFTS_MIN_START,		OnShiftMinStartChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_SHIFTS_HOUR_END,		OnShiftEndingHourChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_SHIFTS_MIN_END,		OnShiftEndingMinuteChanged)
	ON_EN_VALUE_CHANGED(IDC_CALENDARS_SHIFTS_NO_DAY,		OnShiftDayNoChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
TCalendars*			DCalendars::GetCalendars()			const { return (TCalendars*)m_pDBTCalendars->GetRecord(); }
TCalendarsHolidays*	DCalendars::GetCalendarHolidays()	const { return (TCalendarsHolidays*)m_pDBTCalendarHolidays->GetRecord(); }
TCalendarsShifts*	DCalendars::GetCalendarShifts()		const { return (TCalendarsShifts*)m_pDBTCalendarShifts->GetRecord(); }

//-----------------------------------------------------------------------------
DCalendars::DCalendars()
	:
	m_pDBTCalendars				(NULL),
	m_pDBTCalendarWorkingDays	(NULL),
	m_pDBTCalendarWorkingMonths	(NULL),
	m_pDBTCalendarHolidays		(NULL),
	m_pDBTCalendarShifts		(NULL)
{
}       

//-----------------------------------------------------------------------------
DCalendars::~DCalendars()
{
	SAFE_DELETE(m_pDBTCalendarWorkingDays);
	SAFE_DELETE(m_pDBTCalendarWorkingMonths);
}

//-----------------------------------------------------------------------------
TCalendarsHolidays* DCalendars::AddCalendarHolidays()
{
	TCalendarsHolidays* pRec = (TCalendarsHolidays*)m_pDBTCalendarHolidays->AddRecord();
	pRec->SetStorable();
	return pRec;
}

//-----------------------------------------------------------------------------
TCalendarsShifts* DCalendars::AddCalendarShifts()
{
	TCalendarsShifts* pRec = (TCalendarsShifts*)m_pDBTCalendarShifts->AddRecord();
	pRec->SetStorable();
	return pRec;
}

//-----------------------------------------------------------------------------
BOOL DCalendars::OnAttachData()
{
 	SetFormTitle(_TB("Calendar"));

	m_pDBTCalendars = new DBTCalendars(RUNTIME_CLASS(TCalendars), this);
	
	m_pDBTCalendarShifts = new DBTCalendarShifts(RUNTIME_CLASS(TCalendarsShifts), this);
	m_pDBTCalendars->Attach(m_pDBTCalendarShifts);

	m_pDBTCalendarHolidays = new DBTCalendarHolidays(RUNTIME_CLASS(TCalendarsHolidays), this);
	m_pDBTCalendars->Attach(m_pDBTCalendarHolidays);
	
	// sono dbt fittizi
	m_pDBTCalendarWorkingDays = new DBTCalendarWorkingDays(RUNTIME_CLASS(VCalendarWorkingPeriod), this);
	m_pDBTCalendarWorkingMonths = new DBTCalendarWorkingMonths(RUNTIME_CLASS(VCalendarWorkingPeriod), this);

	Attach(new CImportCalendars());

	return Attach(m_pDBTCalendars);
}

// inizializza i dati locali
// NB: inizializzazione di dataobj del database e' nelle Init dei DBT
//-----------------------------------------------------------------------------
BOOL DCalendars::OnInitAuxData()
{
	m_Monday	.Clear();
	m_Tuesday	.Clear();
	m_Wednesday	.Clear();
	m_Wednesday	.Clear();
	m_Friday	.Clear();
	m_Saturday	.Clear();
	m_Sunday	.Clear();
	m_January	.Clear();
	m_February	.Clear();
	m_March		.Clear();
	m_April		.Clear();
	m_May		.Clear();
	m_June		.Clear();
	m_July		.Clear();
	m_August	.Clear();
	m_September	.Clear();
	m_October	.Clear();
	m_November	.Clear();
	m_December	.Clear();

	return CAbstractFormDoc::OnInitAuxData();
}

//-----------------------------------------------------------------------------
BOOL DCalendars::OnPrepareAuxData()
{
	ShowWorkingDays();
	ShowWorkingMonths();

	if (GetFormMode() == CBaseDocument::NEW)
	{
		GetCalendars()->f_ShiftDays = 1;
		AddCalendarShifts();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DCalendars::OnOkTransaction()
{                                        
	BOOL bOk = TRUE;

	bOk = AssignDetailShift();
	
	if (!bOk) return FALSE;

	// prima devo assegnare i valori letti dalla colonna con le checkbox
	AssignWorkingDaysValues();
	GetCalendars()->f_ExcludedDays = 
	    			(BOOL)m_Monday + (BOOL)m_Tuesday * 2 + (BOOL)m_Wednesday * 4 + (BOOL)m_Thursday * 8 +
					(BOOL)m_Friday * 16 + (BOOL)m_Saturday * 32 + (BOOL)m_Sunday * 64;

	if (GetCalendars()->f_ExcludedDays >= 127)
	{
		Message(_TB("You must enter a working day at least!"), MB_ICONSTOP);
		return FALSE;
	}

	// prima devo assegnare i valori letti dalla colonna con le checkbox
	AssignWorkingMonthsValues();
	GetCalendars()->f_ExcludedMonths =
	    			(BOOL)m_January + (BOOL)m_February * 2 + (BOOL)m_March * 4 + (BOOL)m_April * 8 +
					(BOOL)m_May * 16 + (BOOL)m_June * 32 + (BOOL)m_July * 64 +
					(BOOL)m_August * 128 + (BOOL)m_September * 256 + (BOOL)m_October * 512 +
					(BOOL)m_November * 1024 + (BOOL)m_December * 2048;
	
	if (GetCalendars()->f_ExcludedMonths >= 4095)
	{
		Message(_TB("You must enter a working month at least!"), MB_ICONSTOP);
		return FALSE;
	}
	
	bOk = DoCheckDaysTurn();

	if (!bOk) return FALSE;

	if (bOk)
		bOk = DoCheckRangeTurn();

	return CAbstractFormDoc::OnOkTransaction() && bOk;              
};

// assegno manualmente i valori impostati dall'utente nei giorni lavorativi
// okkio che devo invertire i valori!
//----------------------------------------------------------------------------------------------
void DCalendars::AssignWorkingDaysValues()
{
	for (int i = 0; i <= m_pDBTCalendarWorkingDays->GetUpperBound(); i++)
	{
		VCalendarWorkingPeriod* pRec = (VCalendarWorkingPeriod*)m_pDBTCalendarWorkingDays->GetPeriod(i);
		switch (i)
		{
			case 0:
				m_Monday = !pRec->l_IsEnabled;
				break;
			case 1:
				m_Tuesday = !pRec->l_IsEnabled;
				break;
			case 2:
				m_Wednesday = !pRec->l_IsEnabled;
				break;
			case 3:
				m_Thursday = !pRec->l_IsEnabled;;
				break;
			case 4: 
				m_Friday = !pRec->l_IsEnabled;
				break;
			case 5:
				m_Saturday = !pRec->l_IsEnabled;
				break;
			case 6:
				m_Sunday = !pRec->l_IsEnabled;
				break;
		}
	}
}

// assegno manualmente i valori impostati dall'utente nei mesi lavorativi
// okkio che devo invertire i valori!
//----------------------------------------------------------------------------------------------
void DCalendars::AssignWorkingMonthsValues()
{
	for (int i = 0; i <= m_pDBTCalendarWorkingMonths->GetUpperBound(); i++)
	{
		VCalendarWorkingPeriod* pRec = (VCalendarWorkingPeriod*)m_pDBTCalendarWorkingMonths->GetPeriod(i);
		switch (i)
		{
			case 0:
				m_January = !pRec->l_IsEnabled;
				break;
			case 1:
				m_February = !pRec->l_IsEnabled;
				break;
			case 2:
				m_March = !pRec->l_IsEnabled;
				break;
			case 3:
				m_April = !pRec->l_IsEnabled;;
				break;
			case 4:
				m_May = !pRec->l_IsEnabled;
				break;
			case 5:
				m_June = !pRec->l_IsEnabled;
				break;
			case 6:
				m_July = !pRec->l_IsEnabled;
				break;
			case 7:
				m_August = !pRec->l_IsEnabled;
				break;
			case 8:
				m_September = !pRec->l_IsEnabled;
				break;
			case 9:
				m_October = !pRec->l_IsEnabled;
				break;
			case 10:
				m_November = !pRec->l_IsEnabled;
				break;
			case 11:
				m_December = !pRec->l_IsEnabled;
				break;
		}
	}
}

//----------------------------------------------------------------------------------------------
void DCalendars::AddDay(const DataStr& aName, const DataObj& aValue)
{
	VCalendarWorkingPeriod* pRec = (VCalendarWorkingPeriod*)m_pDBTCalendarWorkingDays->AddRecord();
	pRec->l_Period = aName;
	CString val = aValue.Str(0); // torna TRUE/FALSE
	pRec->l_IsEnabled = (val.CompareNoCase(_T("TRUE")) == 0) ? FALSE : TRUE; // il valore va negato
}

//----------------------------------------------------------------------------------------------
void DCalendars::AddMonth(const DataStr& aName, const DataObj& aValue)
{
	VCalendarWorkingPeriod* pRec = (VCalendarWorkingPeriod*)m_pDBTCalendarWorkingMonths->AddRecord();
	pRec->l_Period = aName;
	CString val = aValue.Str(0); // torna TRUE/FALSE
	pRec->l_IsEnabled = (val.CompareNoCase(_T("TRUE")) == 0) ? FALSE : TRUE; // il valore va negato
}

// caricamento manuale del bodyedit dei WorkingDays
//-----------------------------------------------------------------------------
void DCalendars::ShowWorkingDays()
{
	char aStr[16];
	_itoa_s(GetCalendars()->f_ExcludedDays, aStr, 16, 2);
	CString myStr(aStr);
	int len = myStr.GetLength();

	m_Monday = (len > 0) && (myStr.GetAt(len - 1) == '1');
	m_Tuesday = (len > 1) && (myStr.GetAt(len - 2) == '1');
	m_Wednesday = (len > 2) && (myStr.GetAt(len - 3) == '1');
	m_Thursday = (len > 3) && (myStr.GetAt(len - 4) == '1');
	m_Friday = (len > 4) && (myStr.GetAt(len - 5) == '1');
	m_Saturday = (len > 5) && (myStr.GetAt(len - 6) == '1');
	m_Sunday = (len > 6) && (myStr.GetAt(len - 7) == '1');

	m_pDBTCalendarWorkingDays->RemoveAll();

	ADD_DAY(MONDAY, m_Monday);
	ADD_DAY(TUESDAY, m_Tuesday);
	ADD_DAY(WEDNESDAY, m_Wednesday);
	ADD_DAY(THURSDAY, m_Thursday);
	ADD_DAY(FRIDAY, m_Friday);
	ADD_DAY(SATURDAY, m_Saturday);
	ADD_DAY(SUNDAY, m_Sunday);
}

// caricamento manuale del bodyedit dei WorkingMonths
//-----------------------------------------------------------------------------
void DCalendars::ShowWorkingMonths()
{
	char aStr[16];
	_itoa_s(GetCalendars()->f_ExcludedMonths, aStr, 16, 2);
	CString myStr(aStr);
	int len = myStr.GetLength();

	m_January = (len > 0) && (myStr.GetAt(len - 1) == '1');
	m_February = (len > 1) && (myStr.GetAt(len - 2) == '1');
	m_March = (len > 2) && (myStr.GetAt(len - 3) == '1');
	m_April = (len > 3) && (myStr.GetAt(len - 4) == '1');
	m_May = (len > 4) && (myStr.GetAt(len - 5) == '1');
	m_June = (len > 5) && (myStr.GetAt(len - 6) == '1');
	m_July = (len > 6) && (myStr.GetAt(len - 7) == '1');
	m_August = (len > 7) && (myStr.GetAt(len - 8) == '1');
	m_September = (len > 8) && (myStr.GetAt(len - 9) == '1');
	m_October = (len > 9) && (myStr.GetAt(len - 10) == '1');
	m_November = (len > 10) && (myStr.GetAt(len - 11) == '1');
	m_December = (len > 11) && (myStr.GetAt(len - 12) == '1');

	m_pDBTCalendarWorkingMonths->RemoveAll();

	ADD_MONTH(JANUARY, m_January);
	ADD_MONTH(FEBRUARY, m_February);
	ADD_MONTH(MARCH, m_March);
	ADD_MONTH(APRIL, m_April);
	ADD_MONTH(MAY, m_May);
	ADD_MONTH(JUNE, m_June);
	ADD_MONTH(JULY, m_July);
	ADD_MONTH(AUGUST, m_August);
	ADD_MONTH(SEPTEMBER, m_September);
	ADD_MONTH(OCTOBER, m_October);
	ADD_MONTH(NOVEMBER, m_November);
	ADD_MONTH(DECEMBER, m_December);
}

//-----------------------------------------------------------------------------
BOOL DCalendars::AssignDetailShift()
{
	if (m_pDBTCalendarShifts->IsEmpty()) 
	{
		Message(_TB("You must enter a row with turn time at least"), MB_ICONSTOP);
		return FALSE;
	}
	
	for (int i = 0; i <= m_pDBTCalendarShifts->GetUpperBound(); i++)
	{
		TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetCalendarShifts(i);
		pRec->SetStorable(pRec->f_DayNo > 0);
	}

	return TRUE;	
}    

//-----------------------------------------------------------------------------
void DCalendars::OnDayShiftNrChanged()
{
	TCalendars*	pRecCal = m_pDBTCalendars->GetCalendars();  
	if (pRecCal->f_ShiftDays <= 0)
		SetError(_TB("You must enter number of turn days."));
}    

//-----------------------------------------------------------------------------
void DCalendars::OnHolidayStartDateChanged()
{
	TCalendarsHolidays* pRec = m_pDBTCalendarHolidays->GetRecordCurrent();

	if (pRec->f_EndingDay.IsEmpty())
		pRec->f_EndingDay = DataDate(pRec->f_StartingDay.Day(), pRec->f_StartingDay.Month(), pRec->f_StartingDay.Year(), 23, 59, 00);
	else
		DoCheckWorkingTime();
}    

//-----------------------------------------------------------------------------
void DCalendars::OnHolidayEndDateChanged()
{
	DoCheckWorkingTime();
}    
    
//-----------------------------------------------------------------------------
BOOL DCalendars::DoCheckWorkingTime()
{
	BOOL bOk = TRUE;

	TCalendarsHolidays* pRec = m_pDBTCalendarHolidays->GetRecordCurrent();

	if ((pRec->f_StartingDay > pRec->f_EndingDay))
	{
		bOk = FALSE;
		SetError(_TB("Start time cannot follow end time"));
	}
	return bOk;
}    

//-----------------------------------------------------------------------------
void DCalendars::OnShiftHourStartChanged()
{
	TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetRecordCurrent();  

	if (pRec->f_StartingHour == 23)
		pRec->f_StartingMinute = 59;

	DoCheckWorkingTimeShift();
}

//-----------------------------------------------------------------------------
void DCalendars::OnShiftMinStartChanged()
{
	TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetRecordCurrent();  

	if ((pRec->f_StartingHour == 23) && (pRec->f_StartingMinute > 59))
	   SetError(_TB("Wrong Hourly format "));

	DoCheckWorkingTimeShift();
}    

//-----------------------------------------------------------------------------
void DCalendars::OnShiftEndingHourChanged()
{
	DoCheckWorkingTimeShift();
}    

//-----------------------------------------------------------------------------
void DCalendars::OnShiftEndingMinuteChanged()
{
	TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetRecordCurrent();  

	if ((pRec->f_EndingHour == 23) && (pRec->f_EndingMinute > 59))
	   SetError(_TB("Wrong Hourly format "));

	DoCheckWorkingTimeShift();
}    

//-----------------------------------------------------------------------------
BOOL DCalendars::DoCheckWorkingTimeShift(BOOL bShiftSpec /*= FALSE */)
{
	BOOL bOk = TRUE;
	TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetRecordCurrent();  

	if	(
			(pRec->f_StartingHour > pRec->f_EndingHour) ||
			((pRec->f_StartingHour == pRec->f_EndingHour) && (pRec->f_StartingMinute > pRec->f_EndingMinute))
		)
	{
		bOk = FALSE;
		SetError(_TB("Start time cannot follow end time"));
	}

	return bOk;
}    

//-----------------------------------------------------------------------------
void DCalendars::OnShiftDayNoChanged()
{
	TCalendarsShifts*	pRec	= m_pDBTCalendarShifts->GetRecordCurrent();  
	TCalendars*			pRecCal = m_pDBTCalendars->GetCalendars();  

	if	(pRec->f_DayNo > pRecCal->f_ShiftDays) 
		SetError(_TB("Day number cannot be greater than number of turn days."));
}    

//-----------------------------------------------------------------------------
BOOL DCalendars::DoCheckDaysTurn()
{
	TCalendars*	  pRecCal = m_pDBTCalendars->GetCalendars();  

	BOOL bFound = FALSE;
	int MaxElem = m_pDBTCalendarShifts->GetUpperBound();

	// Controllo che il giorno di ogni turno non sia superiore al numero dei giorni di un turno,
	//	e se si chiedo se devo elminare le righe che sono superiori; 
	//	se la risposta e' no, non lascio salvare.
	for (int i = 0; i <= MaxElem; i++)
	{
		TCalendarsShifts* pRec = m_pDBTCalendarShifts->GetCalendarShifts(i);

		if (pRec->f_DayNo > pRecCal->f_ShiftDays)
			if (bFound) 
			{
				m_pDBTCalendarShifts->DeleteRecord(i);
				--i;
				--MaxElem;
			}	
			else
			{
				int Result = Message(_TB("All days greater than turn days will be deleted."), MB_YESNO, 0, _TB("Continue?"));
				if (Result == IDYES || Result == NO_MSG_BOX_SHOWN)
				{
					bFound = TRUE;
					m_pDBTCalendarShifts->DeleteRecord(i);
					--i;
					--MaxElem;
				}
				else
					return FALSE;
			}
	}

	UpdateDataView();
	return TRUE;
}    

//-----------------------------------------------------------------------------
// Controlla che un intervallo di ore e minuti di un record di un giorno del turno
// non caschi dentro un altro intervallo dello stesso giorno 
//-----------------------------------------------------------------------------
BOOL DCalendars::DoCheckRangeTurn()
{
	BOOL bOk = TRUE;

	for (int i = 0; i <= m_pDBTCalendarShifts->GetUpperBound(); i++)
	{
		TCalendarsShifts* pRec1 = m_pDBTCalendarShifts->GetCalendarShifts(i);
		
		for (int j = 0; j <= m_pDBTCalendarShifts->GetUpperBound(); j++)
		{
			if (i != j)
			{
				TCalendarsShifts* pRec2 = m_pDBTCalendarShifts->GetCalendarShifts(j);

				// Se:
				// -	il giorno del primo record (i) e' lo stesso del secondo (j);
				// -	l'orario d'inizio del primo record e' maggiore dell'orario di inizio del secondo;
				// -	l'orario d'inizio del primo record e' minore dell'orario di fine del secondo;
				// allora l'intervallo di orario del secondo record casca dentro al primo record.
				if (
					(pRec1->f_DayNo == pRec2->f_DayNo) &&
					((pRec1->f_StartingHour >= pRec2->f_StartingHour) ||
					((pRec1->f_StartingHour == pRec2->f_StartingHour) &&
					(pRec1->f_StartingMinute >= pRec2->f_StartingMinute))) &&
					((pRec1->f_StartingHour < pRec2->f_EndingHour) ||
					((pRec1->f_StartingHour == pRec2->f_EndingHour) &&
					(pRec1->f_StartingMinute < pRec2->f_EndingMinute))) 
					)
				{
					bOk = FALSE;
					GetMessages()->Add(cwsprintf(_TB("There are time overlapping for day {0-%d}"), (int)pRec1->f_DayNo));
					break;
				}
			}
		}
	}
	GetMessages()->Show(!bOk);
	return bOk;
}    

/////////////////////////////////////////////////////////////////////////////
// 						CImportCalendars
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
BEGIN_TB_EVENT_MAP(CImportCalendars)
	TB_EVENT(DCalendars, OnHolidayStartDateChanged)
	TB_EVENT(DCalendars, OnHolidayEndDateChanged)
	TB_EVENT(DCalendars, OnDayShiftNrChanged)
	TB_EVENT(DCalendars, OnShiftHourStartChanged)
	TB_EVENT(DCalendars, OnShiftMinStartChanged)
	TB_EVENT(DCalendars, OnShiftEndingHourChanged)
	TB_EVENT(DCalendars, OnShiftEndingMinuteChanged)
	TB_EVENT(DCalendars, OnShiftDayNoChanged)
END_TB_EVENT_MAP

IMPLEMENT_DYNCREATE(CImportCalendars, CXMLEventManager);
