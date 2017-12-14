#pragma once

#include <TBOleDB\sqltable.h>
#include <TBGes\hotlink.h>

#include  "beginh.dex"

//=============================================================================
//						###	Causali assenza ###					
//=============================================================================
//
class TB_EXPORT TAbsenceReasons : public SqlRecord //(vecchio TBreakdownReasons)
{
	DECLARE_DYNCREATE(TAbsenceReasons)

public:
	DataStr 	f_Reason;
	DataStr 	f_Description;
	DataStr 	f_Notes;

public:
	TAbsenceReasons(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();	

public:
	static LPCTSTR   GetStaticName();
};

//=============================================================================
//		HotLink Definition						
//=============================================================================
//
class TB_EXPORT HKLAbsenceReasons : public HotKeyLink //(vecchio HKLBreakdownReasons)
{
	DECLARE_DYNCREATE(HKLAbsenceReasons)

public:
	HKLAbsenceReasons();

protected:
	virtual void		OnDefineQuery	(SelectionType nQuerySelection);
	virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
	virtual DataObj*	GetDataObj		() const { return &GetRecord()->f_Reason; };
	virtual	BOOL		Customize		(const DataObjArray&) { return TRUE; };
	
public:
	TAbsenceReasons* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TAbsenceReasons)));
			return (TAbsenceReasons*)m_pRecord;
		}
};

#include  "endh.dex"