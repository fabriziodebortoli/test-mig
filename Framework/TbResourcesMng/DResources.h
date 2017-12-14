
#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include <TbWoormViewer\WOORMDOC.H>

#include "ADMResourcesMng.h"
#include "TAbsenceReasons.h"
#include "TResources.h"
#include "TWorkers.h"

#include "beginh.dex"

class DBTResources;
class DBTResourcesDetails;
class DBTResourcesFields;
class DBTResourcesAbsences;
class TResources;
class TResourcesDetails;
class TResourcesAbsences;
class CWorkerStatic;


//////////////////////////////////////////////////////////////////////////////
//                 DResources declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DResources : public CAbstractFormDoc, public ADMResourcesObj
{ 
	DECLARE_DYNCREATE(DResources)
	friend class CResourcesView; 

public:
	DBTResources*			m_pDBTResources;
	DBTResourcesDetails*	m_pDBTResourcesDetails;
	DBTResourcesFields*		m_pDBTResourcesFields;
	DBTResourcesAbsences*	m_pDBTResourcesAbsences;
	TRWorkers				m_TRWorkers;
	CWorkerStatic*			m_pWorkerCtrl;
public:
	DResources();
	
public:	
	virtual	ADMObj*			GetADM		()	{ return this; }
	virtual TResources*		GetResources()	const;
	virtual BOOL			OnRunReport	(CWoormInfo*);

public: // metodo per passare l'elemento parent dal layout
	virtual void	SetParentResource	(DataStr aResourceType, DataStr aResource);
	virtual void	SetResource			(DataStr aResourceType, DataStr aResource);
	virtual void	SetParentWorkerID	(DataLng aWorkerID);

private:
	DataStr		m_ParentResourceType;
	DataStr		m_ParentResource;
	DataLng		m_ParentWorkerID;
	DataStr		m_ParentWorkerNameComplete; // nome/cognome worker
	DataStr		m_DeletedResourceType;
	DataStr		m_DeletedResourceCode;

protected:
	virtual BOOL	CanDoNewRecord			();
	virtual BOOL	CanDoEditRecord			();
	virtual BOOL	CanDoDeleteRecord		(); 
	virtual BOOL	OnAttachData 			();
	virtual BOOL 	OnOkTransaction			();
	virtual BOOL 	OnOkDelete				();
	virtual	BOOL	OnNewTransaction		(); 
	virtual	BOOL	OnDeleteTransaction		(); 
	virtual BOOL	OnInitAuxData			();
	virtual BOOL	OnPrepareAuxData		();
	virtual void	DeleteContents			();
	virtual void	DisableControlsForEdit	();
	virtual	void	OnParsedControlCreated	(CParsedCtrl* pCtrl);

public:
	TRResources			m_TRResources;
	TRResourceTypes		m_TRResourceTypes;

public:
	void DoResourceTypeChanged		();
	BOOL CheckRecursion				();
	BOOL OkRecurs					();
	BOOL ExistsResource				(const DataStr& aParentResourceCode, const DataStr& aParentResourceType);
	void SetDefaultImage			();

public:	// Generated message map functions
	afx_msg void OnTypeChanged				();
	afx_msg void OnManagerChanged			();
	afx_msg void OnResourcesDetailChanged	();
	afx_msg void OnResourceTypeChanged		();
	afx_msg void OnResourceCodeChanged		();
	afx_msg void OnWorkerChanged			();
	afx_msg void OnIsWorkerChanged			();
	afx_msg void OnBreakDManagerChanged		();
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTResources definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT DBTResources : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTResources)

public:
	DBTResources	(CRuntimeClass*, CAbstractFormDoc*);

public:
	TResources*	GetResources()	const { return (TResources*) GetRecord(); }
	DResources*	GetDocument	()	const { return (DResources*) m_pDocument; }

protected:
	// Gestiscono la query
	virtual void	OnEnableControlsForFind		() {}
	virtual void	OnDisableControlsForEdit	();
	virtual void	OnDisableControlsForAddNew	();
	
	virtual	void	OnDefineQuery				();
	virtual	void	OnPrepareQuery				();

	virtual	BOOL	OnCheckPrimaryKey			();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesDetails definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTResourcesDetails : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTResourcesDetails)

public:
	DBTResourcesDetails	(CRuntimeClass*, CAbstractFormDoc*);

public:
	DResources*			GetDocument			()			const { return (DResources*) m_pDocument; }
	TResources*			GetResources		()			const { return GetDocument()->GetResources(); }
	TResourcesDetails*	GetResourcesDetails	(int nRow)	const { return (TResourcesDetails*) GetRow(nRow); }
	TResourcesDetails*	GetResourcesDetails	()			const { return (TResourcesDetails*) GetRecord(); }
	TResourcesDetails*	GetCurrentRow		()			const { return (TResourcesDetails*) DBTSlaveBuffered::GetCurrentRow(); }
	
protected:
	virtual	void		OnDefineQuery			();
	virtual	void		OnPrepareQuery			();
	virtual void		OnDisableControlsForEdit();

	virtual void		OnPreparePrimaryKey		(int /*nRow*/, SqlRecord* pRec);
	virtual DataObj*	OnCheckPrimaryKey		(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnPrepareAuxColumns		(SqlRecord* pSqlRec);
	virtual	DataObj*	GetDuplicateKeyPos		(SqlRecord*);
	virtual	DataObj*	OnCheckUserData			(int nRow);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesFields definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTResourcesFields : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTResourcesFields)

public:
	DBTResourcesFields	(CRuntimeClass*, CAbstractFormDoc*);

public:
	DResources*			GetDocument			()			const { return (DResources*) m_pDocument; }
	TResources*			GetResources		()			const { return GetDocument()->GetResources(); }
	TResourcesFields*	GetResourcesFields	(int nRow)	const { return (TResourcesFields*) GetRow(nRow); }
	TResourcesFields*	GetResourcesFields	()			const { return (TResourcesFields*) GetRecord(); }
	TResourcesFields*	GetCurrentRow		()			const { return (TResourcesFields*) DBTSlaveBuffered::GetCurrentRow(); }

private:
	void CheckRows(int nRow);

protected:
	virtual	void		OnDefineQuery	();
	virtual	void		OnPrepareQuery	();

	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord* pRec) ;
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec) {return NULL;};
	virtual void		OnAfterAddRow		(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnAfterInsertRow	(int /*nRow*/, SqlRecord* pRec);
	virtual DataObj* 	OnCheckUserData		(int /*nRow*/);

	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesAbsences definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTResourcesAbsences : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTResourcesAbsences)

public:
	DBTResourcesAbsences(CRuntimeClass*, CAbstractFormDoc*);

public:
	DResources*				GetDocument			()			const { return (DResources*) m_pDocument; }
	TResources*				GetResources		()			const { return GetDocument()->GetResources(); }
	TResourcesAbsences*		GetResourcesAbsences(int nRow)	const { return (TResourcesAbsences*) GetRow(nRow); }
	TResourcesAbsences*		GetResourcesAbsences()			const { return (TResourcesAbsences*) GetRecord(); }
	TResourcesAbsences*		GetCurrentRow		()			const { return (TResourcesAbsences*) DBTSlaveBuffered::GetCurrentRow(); }
	
	
protected:
	virtual	void		OnDefineQuery	();
	virtual	void		OnPrepareQuery	();

	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord* pRec) ;
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec);
	virtual	DataObj*	OnCheckUserData		(int nRow);

	virtual void		OnPrepareAuxColumns(SqlRecord* pSqlRec);

	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

#include "endh.dex"
