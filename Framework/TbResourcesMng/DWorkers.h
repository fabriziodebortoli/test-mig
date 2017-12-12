#pragma once

#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include <TbWoormViewer\WOORMDOC.H>

#include "ADMResourcesMng.h"
#include "TWorkers.h"
#include "TResources.h"
#include "TAbsenceReasons.h"
#include "RMFunctions.h"

#include "beginh.dex"

class DBTWorkers;
class DBTWorkersFields;
class DBTWorkersArrangement;
class DBTWorkersAbsences;
class DBTWorkersDetails;
class TWorkers;
class TWorkersFields;
class TWorkersArrangement;
class TWorkersAbsences;


//////////////////////////////////////////////////////////////////////////////
//                 DWorkers declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DWorkers : public CAbstractFormDoc, public ADMWorkersObj
{ 
	DECLARE_DYNCREATE(DWorkers)
	friend class CWorkersView; 

public:
	DBTWorkers*				m_pDBTWorkers;
	DBTWorkersArrangement*	m_pDBTWorkersArrangement;
	DBTWorkersFields*		m_pDBTWorkersFields;
	DBTWorkersAbsences*		m_pDBTWorkersAbsences;
	DBTWorkersDetails*		m_pDBTWorkersDetails;
	
public:
	DWorkers();

public:	
	virtual	ADMObj*		GetADM			()	{ return this; }
	virtual TWorkers*	GetWorkers 		()	const;
	virtual BOOL		OnRunReport		(CWoormInfo*);

public: // metodo per passare l'elemento parent dal layout
	virtual void	SetWorker			(DataLng aWorkerID);
	virtual void	SetParentResource	(DataStr aResourceType, DataStr aResource);
	virtual void	SetParentWorkerID	(DataLng aWorkerID);

protected:
	virtual BOOL	CanDoNewRecord			();
	virtual BOOL	OnAttachData 			();
	virtual BOOL 	OnOkDelete				();
	virtual BOOL 	OnOkTransaction			();
	virtual	BOOL	OnNewTransaction		(); 
	virtual	BOOL	OnDeleteTransaction		(); 
	virtual BOOL	OnPrepareAuxData		();
	virtual void	DeleteContents			();
	virtual	void	OnParsedControlCreated	(CParsedCtrl* pCtrl);

public:
	SimHKLUser*				m_pSimHKLUser;

	TRWorkersByLogin*		m_pTRWorkersByLogin;
	TRWorkersByName*		m_pTRWorkersByName;
	TRWorkersByPIN*			m_pTRWorkersByPIN;
	TUWorkers*				m_pTUWorkers;
	TRResources*			m_pTRResources;
	TRWorkers*				m_pTRWorkers;
	
	BOOL		m_CanDeleteWorkers;
	BOOL		m_StandardEdition;

private:
	DataStr		m_ParentResourceType;
	DataStr		m_ParentResource;
	DataLng		m_ParentWorkerID;
	DataStr		m_ParentWorkerNameComplete; // nome/cognome worker
	DataLng		m_DeletedWorkerID;

private:
	BOOL DoCheckAbsencesTime	();
	BOOL DoCheckArrangementTime	();
	void DoResourceTypeChanged	();

	BOOL CheckRecursion			();
	BOOL OkRecurs				();
	BOOL ExistsResource			(const DataStr& aParentResourceCode, const DataStr& aParentResourceType);
	BOOL ExistsWorker			(DataLng& aWorker);

private:
	DeleteWorkerResult CheckIfWorkerIsUsed		();
	DeleteWorkerResult CheckMasterTableForWorker(CString aTableName);

public:	
	afx_msg void OnManagerChanged				();
	afx_msg void OnArrangementFromDateChanged	();
	afx_msg void OnArrangementToDateChanged		();
	afx_msg void OnAbsencesToDateChanged		();
	afx_msg void OnAbsencesFromDateChanged		();
	afx_msg void OnArrangementChanged			();
	afx_msg void OnPINChanged					();
	afx_msg void OnResourceTypeChanged			();
	afx_msg void OnResourceCodeChanged			();
	afx_msg void OnWorkerChanged				();
	afx_msg void OnIsWorkerChanged				();
	afx_msg void OnIsDisabledChanged			();
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkers definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT DBTWorkers : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTWorkers)

public:
	DBTWorkers	(CRuntimeClass*, CAbstractFormDoc*);

public:
	TWorkers* GetWorkers () const { return (TWorkers*) GetRecord(); }
	DWorkers* GetDocument() const { return (DWorkers*) m_pDocument; }

protected:
	virtual void	OnEnableControlsForFind		() {}
	virtual void	OnDisableControlsForEdit	();
	virtual void	OnDisableControlsForAddNew	();
	
	virtual	void	OnDefineQuery				();
	virtual	void	OnPrepareQuery				();

	virtual	BOOL	OnCheckPrimaryKey			() {return TRUE;};
	virtual	void	OnPreparePrimaryKey			() {};
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersFields definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTWorkersFields : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTWorkersFields)

public:
	DBTWorkersFields	(CRuntimeClass*, CAbstractFormDoc*);

public:
	DWorkers*			GetDocument		()			const { return (DWorkers*) m_pDocument; }
	TWorkers*			GetWorkers		()			const { return GetDocument()->GetWorkers(); }
	TWorkersFields*		GetWorkersField	(int nRow)	const { return (TWorkersFields*) GetRow(nRow); }
	TWorkersFields*		GetWorkersField	()			const { return (TWorkersFields*) GetRecord(); }
	TWorkersFields*		GetCurrentRow	()			const { return (TWorkersFields*) DBTSlaveBuffered::GetCurrentRow(); }

private:
	void CheckRows(int nRow);

protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();

	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord* pRec) ;
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec) {return NULL;};
	virtual void		OnAfterAddRow		(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnAfterInsertRow	(int /*nRow*/, SqlRecord* pRec);
	virtual DataObj* 	OnCheckUserData		(int /*nRow*/);

	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersArrangement definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTWorkersArrangement : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTWorkersArrangement)

public:
	DBTWorkersArrangement(CRuntimeClass*, CAbstractFormDoc*);

public:
	DWorkers*				GetDocument				()			const { return (DWorkers*) m_pDocument; }
	TWorkers*				GetWorkers				()			const { return GetDocument()->GetWorkers(); }
	TWorkersArrangement*	GetWorkersArrangement	(int nRow)	const { return (TWorkersArrangement*) GetRow(nRow); }
	TWorkersArrangement*	GetWorkersArrangement	()			const { return (TWorkersArrangement*) GetRecord(); }
	TWorkersArrangement*	GetCurrentRow			()			const { return (TWorkersArrangement*) DBTSlaveBuffered::GetCurrentRow(); }
	
protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();

	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord* pRec) ;
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec) {return NULL;};
	virtual void		OnAfterInsertRow	(int /*nRow*/, SqlRecord* pRec);

	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersAbsences definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTWorkersAbsences : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTWorkersAbsences)

public:
	DBTWorkersAbsences(CRuntimeClass*, CAbstractFormDoc*);

public:
	DWorkers*			GetDocument			()			const { return (DWorkers*) m_pDocument; }
	TWorkers*			GetWorkers			()			const { return GetDocument()->GetWorkers(); }
	TWorkersAbsences*	GetWorkersAbsences	(int nRow)	const { return (TWorkersAbsences*)GetRow(nRow); }
	TWorkersAbsences*	GetWorkersAbsences	()			const { return (TWorkersAbsences*)GetRecord(); }
	TWorkersAbsences*	GetCurrentRow		()			const { return (TWorkersAbsences*)DBTSlaveBuffered::GetCurrentRow(); }
	
protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();

	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord* pRec) ;
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord* pRec);

	virtual void		OnPrepareAuxColumns	(SqlRecord* pSqlRec);
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);
	virtual	DataObj*	OnCheckUserData		(int nRow);
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersDetails definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTWorkersDetails : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTWorkersDetails)

public:
	DBTWorkersDetails(CRuntimeClass*, CAbstractFormDoc*);

public:
	DWorkers*			GetDocument			()			const { return (DWorkers*)m_pDocument; }
	TWorkers*			GetWorkers			()			const { return GetDocument()->GetWorkers(); }
	TWorkersDetails*	GetWorkersDetails	(int nRow)	const { return (TWorkersDetails*)GetRow(nRow); }
	TWorkersDetails*	GetWorkersDetails	()			const { return (TWorkersDetails*)GetRecord(); }
	TWorkersDetails*	GetCurrentRow		()			const { return (TWorkersDetails*)DBTSlaveBuffered::GetCurrentRow(); }

protected:
	virtual	void		OnDefineQuery			();
	virtual	void		OnPrepareQuery			();
	virtual void		OnDisableControlsForEdit();

	virtual void		OnPrepareRow			(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnPreparePrimaryKey		(int /*nRow*/, SqlRecord* pRec);
	virtual DataObj*	OnCheckPrimaryKey		(int /*nRow*/, SqlRecord* pRec);
	virtual void		OnPrepareAuxColumns		(SqlRecord* pSqlRec);
	virtual	DataObj*	GetDuplicateKeyPos		(SqlRecord*);
	virtual	DataObj*	OnCheckUserData			(int nRow);
};

#include "endh.dex"
