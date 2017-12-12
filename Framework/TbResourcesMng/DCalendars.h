#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\eventmng.h>
#include <TbGes\dbt.h>
#include <TbGes\BODYEDIT.H>

#include "ADMResourcesMng.h"

#include "beginh.dex"

class DCalendars;
class TCalendars;
class TCalendarsHoliday;
class TCalendarsShifts;
class CalendarsWorkingPeriodStrings;

#define ADD_DAY(Name, Value) AddDay(CalendarsWorkingPeriodStrings::##Name(), ##Value);
#define ADD_MONTH(Name, Value) AddMonth(CalendarsWorkingPeriodStrings::##Name(), ##Value);
#define PERIODFIELD(Name) CalendarsWorkingPeriodStrings::##Name()

//=============================================================================
BEGIN_TB_STRING_MAP(CalendarsWorkingPeriodStrings)
	TB_LOCALIZED(MONDAY,	"Monday")
	TB_LOCALIZED(TUESDAY,	"Tuesday")
	TB_LOCALIZED(WEDNESDAY, "Wednesday")
	TB_LOCALIZED(THURSDAY,	"Thursday")
	TB_LOCALIZED(FRIDAY,	"Friday")
	TB_LOCALIZED(SATURDAY,	"Saturday")
	TB_LOCALIZED(SUNDAY,	"Sunday")
	TB_LOCALIZED(JANUARY,	"January")
	TB_LOCALIZED(FEBRUARY,	"February")
	TB_LOCALIZED(MARCH,		"March")
	TB_LOCALIZED(APRIL,		"April")
	TB_LOCALIZED(MAY,		"May")
	TB_LOCALIZED(JUNE,		"June")
	TB_LOCALIZED(JULY,		"July")
	TB_LOCALIZED(AUGUST,	"August")
	TB_LOCALIZED(SEPTEMBER, "September")
	TB_LOCALIZED(OCTOBER,	"October")
	TB_LOCALIZED(NOVEMBER,	"November")
	TB_LOCALIZED(DECEMBER,	"December")
END_TB_STRING_MAP()

///////////////////////////////////////////////////////////////////////////////
//	Class VCalendarWorkingPeriod declaration
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT VCalendarWorkingPeriod : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VCalendarWorkingPeriod)

public:
	DataStr		l_Period;
	DataBool	l_IsEnabled;

public:
	VCalendarWorkingPeriod(BOOL bCallInit = TRUE);
	static LPCTSTR GetStaticName();

public:
	virtual void BindRecord();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTCalendars definition
//////////////////////////////////////////////////////////////////////////////
//                                                                          
class TB_EXPORT DBTCalendars : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTCalendars)

public:
	DBTCalendars(CRuntimeClass*, CAbstractFormDoc*);

public:
	TCalendars* GetCalendars() const { return (TCalendars*) GetRecord(); }

protected:
	virtual void 	Init					();
	virtual void	OnEnableControlsForFind	() {}
	virtual void	OnDisableControlsForEdit();
	// Gestiscono la query
	virtual	void	OnDefineQuery			();
	virtual	void	OnPrepareQuery			();
	virtual	void	OnPrepareBrowser		(SqlTable* pTable);

	// DEVONO essere implementate nella classe finale
	virtual	BOOL 	OnCheckPrimaryKey		();
	virtual	void 	OnPreparePrimaryKey		()  {}
};

//////////////////////////////////////////////////////////////////////////////
//      class DBTCalendarWorkingDays definition	(BODYEDIT giorni lavorativi)
//////////////////////////////////////////////////////////////////////////////
//                                                                          
class TB_EXPORT DBTCalendarWorkingDays : public DBTSlaveBuffered
{
	friend class DCalendars;

	DECLARE_DYNAMIC(DBTCalendarWorkingDays)

public:
	DBTCalendarWorkingDays
		(
		CRuntimeClass*		pClass,
		CAbstractFormDoc*	pDocument
		);

public:
	VCalendarWorkingPeriod*	GetCurrent		() 			const	{ return (VCalendarWorkingPeriod*)GetCurrentRow(); }
	int						GetCurrentRowIdx()			const 	{ return m_nCurrentRow; }
	DCalendars*				GetDocument		()			const	{ return (DCalendars*)m_pDocument; }
	VCalendarWorkingPeriod*	GetPeriod		(int nRow) 	const 	{ return (VCalendarWorkingPeriod*)GetRow(nRow); }
	VCalendarWorkingPeriod*	GetPeriod		()	   		const	{ return (VCalendarWorkingPeriod*)GetRecord(); }

	virtual void			SetCurrentRow	(int nRow);

protected:
	virtual	void			OnDefineQuery	()	{}
	virtual	void			OnPrepareQuery	()	{}

	virtual DataObj*		OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void			OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//    class DBTCalendarWorkingMonths definition	(BODYEDIT mesi lavorativi)
//////////////////////////////////////////////////////////////////////////////
//                                                                          
class TB_EXPORT DBTCalendarWorkingMonths : public DBTSlaveBuffered
{
	friend class DCalendars;

	DECLARE_DYNAMIC(DBTCalendarWorkingMonths)

public:
	DBTCalendarWorkingMonths
		(
		CRuntimeClass*		pClass,
		CAbstractFormDoc*	pDocument
		);

public:
	VCalendarWorkingPeriod*	GetCurrent		() 			const	{ return (VCalendarWorkingPeriod*)GetCurrentRow(); }
	int						GetCurrentRowIdx()			const 	{ return m_nCurrentRow; }
	DCalendars*				GetDocument		()			const	{ return (DCalendars*)m_pDocument; }
	VCalendarWorkingPeriod*	GetPeriod		(int nRow) 	const 	{ return (VCalendarWorkingPeriod*)GetRow(nRow); }
	VCalendarWorkingPeriod*	GetPeriod		()			const	{ return (VCalendarWorkingPeriod*)GetRecord(); }

	virtual void			SetCurrentRow	(int nRow);

protected:
	virtual	void			OnDefineQuery	()	{}
	virtual	void			OnPrepareQuery	()	{}

	virtual DataObj*		OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void			OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//         class DBTCalendarHolidays definition	(BODYEDIT festivita')
//////////////////////////////////////////////////////////////////////////////
//                                                                          
class TB_EXPORT DBTCalendarHolidays : public DBTSlaveBuffered
{ 
	DECLARE_DYNAMIC(DBTCalendarHolidays)

public:
	DBTCalendarHolidays
		(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
		);

public:
	DCalendars* 		GetDocument			() 			const { return (DCalendars*) m_pDocument; }
	TCalendarsHolidays*	GetCalendarHolidays	() 			const { return (TCalendarsHolidays*)GetRecord(); }
	TCalendars*			GetCalendars		() 			const;
	TCalendarsHolidays*	GetCalendarHolidays	(int nRow) 	const { return (TCalendarsHolidays*)GetRow(nRow); }
	TCalendarsHolidays*	GetRecordCurrent	()	 		const;

public:
	// Gestiscono la query
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();
	virtual	DataObj* 	OnCheckUserData		(int nRow);

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/,	SqlRecord* pRec);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/,	SqlRecord*);
	virtual void		OnPrepareRow		(int,			SqlRecord*);
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTCalendarShifts definition (body edit turni lavoro)
//////////////////////////////////////////////////////////////////////////////
//                                                                          
class TB_EXPORT DBTCalendarShifts : public DBTSlaveBuffered
{ 
	DECLARE_DYNAMIC(DBTCalendarShifts)

public:
	DBTCalendarShifts
		(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
		);

public:
	DCalendars* 		GetDocument			() 			const { return (DCalendars*) m_pDocument; }
	TCalendarsShifts*	GetCalendarShifts	() 			const { return (TCalendarsShifts*) GetRecord();  }
	TCalendars*			GetCalendars		() 			const;
	TCalendarsShifts*	GetCalendarShifts	(int nRow) 	const { return (TCalendarsShifts*) GetRow(nRow); } 
	TCalendarsShifts*	GetRecordCurrent	()	 		const; 

public:
	// Gestiscono la query
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPrepareRow		(int,			SqlRecord*);
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//             class DCalendars definition								//
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT DCalendars : public CAbstractFormDoc, public ADMCalendarsObj
{
	friend class CImportCalendars;

	DECLARE_DYNCREATE(DCalendars)

public:        
	// DBT
	DBTCalendars*				m_pDBTCalendars;
	DBTCalendarWorkingDays*		m_pDBTCalendarWorkingDays;
	DBTCalendarWorkingMonths*	m_pDBTCalendarWorkingMonths;
	DBTCalendarHolidays*		m_pDBTCalendarHolidays;
	DBTCalendarShifts*			m_pDBTCalendarShifts;

public:		
	DCalendars();
	~DCalendars();

public:	
	TCalendarsHolidays*			GetCalendarHolidays	() const;
	TCalendarsShifts*			GetCalendarShifts 	() const;

public :
	virtual	ADMObj*				GetADM()				{ return this; }

	virtual	TCalendars*			GetCalendars()			const;

	virtual	TCalendarsHolidays*	GetCalendarHolidays		(int nRow) const  { return (TCalendarsHolidays*)m_pDBTCalendarHolidays->GetRow(nRow); }
	virtual TCalendarsHolidays*	AddCalendarHolidays		();

	virtual	TCalendarsShifts*	GetCalendarShifts		(int nRow) const  {return (TCalendarsShifts*)m_pDBTCalendarShifts->GetRow(nRow); }
	virtual TCalendarsShifts*	AddCalendarShifts		();

	virtual DBTSlaveBuffered*   GetDBTCalendarHolidays	()	{ return m_pDBTCalendarHolidays;	}
	virtual DBTSlaveBuffered*   GetDBTCalendarShifts	()	{ return m_pDBTCalendarShifts;		}

private:
	BOOL	AssignDetailShift			();	
	void	ShowWorkingDays				();
	void	ShowWorkingMonths			();
	void	AddDay						(const DataStr& aName, const DataObj& aValue);
	void	AddMonth					(const DataStr& aName, const DataObj& aValue);

	void	AssignWorkingDaysValues		();
	void	AssignWorkingMonthsValues	();

protected:
	virtual BOOL 	OnAttachData 	 	();
	virtual BOOL 	OnInitAuxData	 	();
	virtual BOOL 	OnPrepareAuxData	();
	virtual BOOL 	OnOkTransaction 	();

public:
	DataBool	m_Monday;
	DataBool	m_Tuesday;
	DataBool	m_Wednesday;
	DataBool	m_Thursday;
	DataBool	m_Friday;
	DataBool	m_Saturday;
	DataBool	m_Sunday;
	DataBool	m_January;
	DataBool	m_February;
	DataBool	m_March;
	DataBool	m_April;
	DataBool	m_May;
	DataBool	m_June;
	DataBool	m_July;
	DataBool	m_August;
	DataBool	m_September;
	DataBool	m_October;
	DataBool	m_November;
	DataBool	m_December;

protected:	
	BOOL DoCheckWorkingTime			();
	BOOL DoCheckWorkingTimeShift	(BOOL bSpecialShifts = FALSE);
	BOOL DoCheckDaysTurn			();
	BOOL DoCheckRangeTurn			();

protected:	
	//{{AFX_MSG(DCalendars)
	afx_msg void OnHolidayStartDateChanged	();
	afx_msg void OnHolidayEndDateChanged	();
	afx_msg void OnDayShiftNrChanged		();
	afx_msg void OnShiftHourStartChanged	();
	afx_msg void OnShiftEndingHourChanged	();
	afx_msg void OnShiftMinStartChanged		();
	afx_msg void OnShiftEndingMinuteChanged	();
	afx_msg void OnShiftDayNoChanged		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//					class CImportCalendars definition
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CImportCalendars : public CXMLEventManager
{
	DECLARE_TB_EVENT_MAP ();
	DECLARE_DYNCREATE(CImportCalendars);
};

#include "endh.dex"