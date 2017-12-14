#pragma once

#include <TBOleDB\sqltable.h>
#include <TBGes\hotlink.h>

#include "beginh.dex"

//=============================================================================
//				###	Anagrafica Calendario ###					
//=============================================================================
//
class TB_EXPORT TCalendars : public SqlRecord
{
	DECLARE_DYNCREATE(TCalendars) 

public:
	DataStr		f_Calendar; // chiave, codice fisso "CALENDARIO_DI_FABBRICA"
	DataStr		f_Description;
	DataInt		f_ExcludedDays;
	DataInt		f_ExcludedMonths;
	DataInt		f_ShiftDays;           
	DataBool	f_MoveShiftsOnExclDays;
	DataStr		f_Notes;

public:
	TCalendars(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();	

public:
	static LPCTSTR   GetStaticName();
};

//=============================================================================
//		###	Date giorni non lavorativi per calendario ###	SLAVES  (1 a 0) 
//=============================================================================
//
class TB_EXPORT TCalendarsHolidays : public SqlRecord 
{
	DECLARE_DYNCREATE(TCalendarsHolidays)

public:
	DataStr		f_Calendar;
	DataInt		f_LineNo;
	DataDate	f_StartingDay;
	DataDate	f_EndingDay;
	DataStr		f_ReasonOfExclusion;
	DataStr		f_Notes;

public:
	TCalendarsHolidays(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
}; 

//=============================================================================
//				###	Turni orari per calendario ###	SLAVES  (1 a 0) 
//=============================================================================
//
class TB_EXPORT TCalendarsShifts : public SqlRecord
{
	DECLARE_DYNCREATE(TCalendarsShifts) 

public:
	DataStr		f_Calendar;
	DataInt		f_Line;
	DataInt		f_DayNo;
	DataInt		f_StartingHour;
	DataInt		f_StartingMinute;
	DataInt		f_EndingHour;      
	DataInt		f_EndingMinute;      
	DataStr		f_Notes;

public:
	TCalendarsShifts(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
}; 

//=============================================================================
class TB_EXPORT HKLCalendars : public HotKeyLink
{
DECLARE_DYNCREATE (HKLCalendars)

public:
	HKLCalendars();

protected:
	virtual void		OnDefineQuery	(SelectionType nQuerySelection);
	virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
	virtual DataObj*	GetDataObj		() const { return &GetRecord()->f_Calendar; };
	virtual	BOOL		Customize		(const DataObjArray&) { return TRUE; };

public:
	TCalendars* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TCalendars)));
			return (TCalendars*) m_pRecord;                              
		}
};

#include "endh.dex"
