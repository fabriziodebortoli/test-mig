#pragma once

#include <TbGes\EXTDOC.H>

#include "beginh.dex"

class TAbsenceReasons;
class TCalendars;
class TCalendarsHolidays;
class TCalendarsShifts;

/////////////////////////////////////////////////////////////////////////////
//							ADMAbsenceReasonsObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMAbsenceReasonsObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMAbsenceReasonsObj)

public:
	virtual	ADMObj*	GetADM() = 0;

	virtual	TAbsenceReasons* GetAbsenceReasons() const = 0;
};

/////////////////////////////////////////////////////////////////////////////
//							ADMArrangementsObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMArrangementsObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMArrangementsObj)

public:
	virtual	ADMObj* GetADM() = 0;
};

/////////////////////////////////////////////////////////////////////////////
//							ADMCalendarsObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMCalendarsObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMCalendarsObj)

public:
	virtual	ADMObj*				GetADM					() = 0;

	virtual	TCalendars*			GetCalendars			() const = 0;

	virtual	TCalendarsHolidays*	GetCalendarHolidays		(int nRow) const = 0;	
	virtual TCalendarsHolidays*	AddCalendarHolidays		() = 0;					

	virtual	TCalendarsShifts*	GetCalendarShifts		(int nRow) const = 0;
	virtual TCalendarsShifts*	AddCalendarShifts		() = 0;

	virtual DBTSlaveBuffered*   GetDBTCalendarHolidays	() = 0; 
	virtual DBTSlaveBuffered*   GetDBTCalendarShifts	() = 0;
};

/////////////////////////////////////////////////////////////////////////////
//							ADMResourcesObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMResourcesObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMResourcesObj)

public:
	virtual	ADMObj*	GetADM				() = 0;
	virtual void	SetParentResource	(DataStr aResourceType, DataStr aResource) = 0;
	virtual void	SetParentWorkerID	(DataLng aWorkerID) = 0;
	virtual void	SetResource			(DataStr aResourceType, DataStr aResource) = 0;
};

/////////////////////////////////////////////////////////////////////////////
//							ADMResourceTypesObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMResourceTypesObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMResourceTypesObj)

public:
	virtual	ADMObj*	GetADM() = 0;
};

/////////////////////////////////////////////////////////////////////////////
//							ADMWorkersObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMWorkersObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMWorkersObj)

public:
	virtual	ADMObj*	GetADM() = 0;
	virtual void	SetParentResource	(DataStr aResourceType, DataStr aResource) = 0;
	virtual void	SetParentWorkerID	(DataLng aWorkerID) = 0;
	virtual void	SetWorker			(DataLng aWorkerID) = 0;

};

/////////////////////////////////////////////////////////////////////////////
//							ADMWorkerWindowObj
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMWorkerWindowObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMWorkerWindowObj)

public:
	virtual	ADMObj* GetADM() = 0;
	virtual	void SetWorker(	DataLng		aCreatedWorker,
							DataStr		aCreatedWorkerDes,
							DataDate	aCreatedDate,
							DataStr		aCreatedWorkerOfficePhone,
							DataStr		aCreatedWorkerEmail,
							DataStr		aCreatedWorkerPicture,
							DataLng		aModifiedWorker,
							DataStr		aModifiedWorkerDes,
							DataDate	aModifiedDate,
							DataStr		aModifiedWorkerOfficePhone,
							DataStr		aModifiedWorkerEmail,
							DataStr		aModifiedWorkerPicture) = 0;
};

/////////////////////////////////////////////////////////////////////////////
//					class ADMResourcesLayoutObj definition		
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ADMResourcesLayoutObj : public ADMObj
{
	DECLARE_ADMCLASS(ADMResourcesLayoutObj)

public:
	virtual	ADMObj*	GetADM() = 0;
};

#include "endh.dex"
