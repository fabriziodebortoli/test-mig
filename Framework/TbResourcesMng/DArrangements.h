
#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include "ADMResourcesMng.h"
#include "TWorkers.h"

#include "beginh.dex"

class TArrangements;
class DArrangements;

//////////////////////////////////////////////////////////////////////////////
//  DBTMaster:               DBTArrangements declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTArrangements : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTArrangements)

public:
	DBTArrangements(CRuntimeClass*, CAbstractFormDoc*);

public:
	TArrangements*	GetArrangements	() const { return (TArrangements*) GetRecord();}
	DArrangements*	GetDocument		() const { return (DArrangements*) m_pDocument; }

protected:
	virtual void	OnEnableControlsForFind		() {}
	virtual	void	OnDisableControlsForEdit	();

	virtual	void	OnDefineQuery		();
	virtual	void	OnPrepareQuery		();
	virtual	void	OnPrepareBrowser	(SqlTable* pTable);
	virtual	BOOL	OnCheckPrimaryKey	();
};

//////////////////////////////////////////////////////////////////////////////
//             class DArrangements definition 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DArrangements : public CAbstractFormDoc, public ADMArrangementsObj
{
	DECLARE_DYNCREATE(DArrangements)

public:
	DArrangements();

public:	
	DBTArrangements*		m_pDBTArrangements;

public:	
			TArrangements*	GetArrangements	()  const;
	virtual	ADMObj*			GetADM			() { return this; }
	virtual BOOL			OnRunReport		(CWoormInfo*);

protected:
	virtual BOOL OnAttachData();

protected:	// Generated message map functions
	//{{AFX_MSG(DArrangements)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
