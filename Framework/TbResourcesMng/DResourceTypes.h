#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include "ADMResourcesMng.h"
#include "TResources.h"

#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//  DBTMaster:               DBTResourceType declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTResourceType : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTResourceType)

public:
	DBTResourceType(CRuntimeClass*, CAbstractFormDoc*);

public:
	TResourceTypes* GetResourceTypes() const { return (TResourceTypes*)GetRecord(); }

protected: 
	// Gestione delle query
	virtual void	OnEnableControlsForFind			();
	virtual void	OnDisableControlsForEdit		();
	
	virtual	void	OnDefineQuery		();
	virtual	void	OnPrepareQuery		();
	virtual	void	OnPrepareBrowser	(SqlTable* pTable);

	virtual	BOOL	OnCheckPrimaryKey	();
	virtual	void	OnPreparePrimaryKey	() {}
};

//////////////////////////////////////////////////////////////////////////////
//                 DResourceTypes declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DResourceTypes : public CAbstractFormDoc, public ADMResourceTypesObj
{
	DECLARE_DYNCREATE(DResourceTypes)

public: 
	DBTResourceType*	m_pDBTResourceType;
	DResourceTypes();
	
public:	
	virtual	ADMObj*	GetADM() { return this; }
	
protected:
	virtual BOOL OnAttachData 			();
	virtual	void OnParsedControlCreated	(CParsedCtrl* pCtrl);
};

///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
