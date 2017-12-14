
#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\eventmng.h>
#include <TbGes\dbt.h>

#include "ADMResourcesMng.h"
#include "TAbsenceReasons.h"

#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//             class DBTAbsenceReasons definition 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTAbsenceReasons : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTAbsenceReasons)

public:
	DBTAbsenceReasons(CRuntimeClass*, CAbstractFormDoc*);

public:
	TAbsenceReasons* GetAbsenceReasons() const { return (TAbsenceReasons*)GetRecord(); }

protected:
	virtual void	OnEnableControlsForFind	() {}
	virtual void	OnDisableControlsForEdit();

	virtual	void	OnDefineQuery		();
	virtual	void	OnPrepareQuery		();  
	virtual	void	OnPrepareBrowser	(SqlTable* pTable);

	virtual	BOOL	OnCheckPrimaryKey	();
	virtual	void	OnPreparePrimaryKey	()	{}
};

//////////////////////////////////////////////////////////////////////////////
//             class DAbsenceReasons definition 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DAbsenceReasons : public CAbstractFormDoc, public ADMAbsenceReasonsObj
{
	DECLARE_DYNCREATE(DAbsenceReasons)

public:
	DAbsenceReasons();
	DBTAbsenceReasons*	m_pDBTAbsenceReasons;

public:	
	virtual ADMObj*				GetADM				() { return this; }
	virtual TAbsenceReasons*	GetAbsenceReasons	() const;

protected:
	virtual BOOL				OnAttachData		();

protected:
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//				class CImportAbsenceReasons definition
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CImportAbsenceReasons : public CXMLEventManager
{
	DECLARE_TB_EVENT_MAP ();
	DECLARE_DYNCREATE(CImportAbsenceReasons);
};

#include "endh.dex"