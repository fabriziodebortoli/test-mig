#pragma once 

#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>
#include <TbGenlib\TBPropertyGrid.h>

#include "SOSObjects.h"
//includere alla fine degli include del .H
#include "beginh.dex"

class DSOSConfiguration;

//////////////////////////////////////////////////////////////////////////////
//         class DBTSOSConfiguration definition 
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class TB_EXPORT DBTSOSConfiguration : public DBTMaster
{
	DECLARE_DYNAMIC(DBTSOSConfiguration)

public:
	DBTSOSConfiguration(CRuntimeClass*, CAbstractFormDoc*);

public:
	VSOSConfiguration* GetVSOSConfiguration() const { return (VSOSConfiguration*)GetRecord(); }

protected: 
	DSOSConfiguration* GetDocument() { return (DSOSConfiguration*)m_pDocument; }

	virtual void OnEnableControlsForFind	() {}
	virtual	void OnDefineQuery				();
	virtual	void OnPrepareQuery				();
	virtual	void OnPrepareBrowser			(SqlTable* pTable);
	virtual	BOOL OnCheckPrimaryKey			();
	virtual	void OnPreparePrimaryKey		();
	virtual void OnDisableControlsForEdit	();
	virtual	BOOL CheckTransaction			() { return TRUE; }

public:
	virtual BOOL FindData	(BOOL bPrepareOld = TRUE);
	virtual BOOL AddNew		(BOOL bInit = TRUE) { return TRUE; }
	virtual BOOL Edit		() { return TRUE; }
	virtual BOOL Delete		() { return TRUE; }
	virtual BOOL Update		();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTSOSDocClasses definition 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTSOSDocClasses : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSOSDocClasses)

public:
	DBTSOSDocClasses(CRuntimeClass*, CAbstractFormDoc*);

public:
	DSOSConfiguration*	GetDocument		() { return (DSOSConfiguration*)m_pDocument; }
	VSOSDocClass*		GetVSOSDocClass	() const { return (VSOSDocClass*)GetRecord(); }

public:
	// Gestiscono la query
	virtual	void		OnDefineQuery	();
	virtual	void		OnPrepareQuery	();

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual	void		OnPreparePrimaryKey	(int nRow, SqlRecord*);

	virtual BOOL		LocalFindData(BOOL bPrepareOld);
};

//////////////////////////////////////////////////////////////////////////////
//				class DSOSConfiguration definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DSOSConfiguration : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DSOSConfiguration)

public:
	DSOSConfiguration();
	~DSOSConfiguration();

public:
	DBTSOSConfiguration*	m_pDBTSOSConfiguration;
	DBTSOSDocClasses*		m_pDBTSOSDocClasses;
	VSOSConfiguration*		GetVSOSConfiguration() const;

	CSOSConfiguration*		m_pSOSConfiguration; 

public:
	BOOL LoadSOSDocumentClasses		(); 
	void ClearSOSDocumentClasses	();

private:
	BOOL CheckInputData				();

protected:
	virtual BOOL OnAttachData 		();
	virtual BOOL CanRunDocument		();
	virtual BOOL CanDoEditRecord	();
	virtual BOOL CanSaveParameters	() { return AfxGetBaseApp()->GetNrOpenDocuments() == 1; }

	virtual BOOL OnOkTransaction	();
	virtual	BOOL OnPrepareAuxData	() { return TRUE; }

	// della toolbar mi servono solo i bottoni di edit, save ed escape
	virtual BOOL CanDoDeleteRecord	()	{ return FALSE; }
	virtual BOOL CanDoNewRecord		()	{ return FALSE; }

	virtual void OnPropertyCreated	(CTBProperty* pProperty);

public:
	//{{AFX_MSG(DSOSConfiguration)
	afx_msg void OnToolbarReloadDocClasses		();
	afx_msg void OnUpdateToolbarReloadDocClasses(CCmdUI*);
	afx_msg void OnEnableFTPSend				();
	afx_msg void OnSubjectCodeChanged			();
	afx_msg void OnMySOSUserChanged				();
	afx_msg void OnMySOSPasswordChanged			();
	afx_msg void OnSOSUrlChanged				();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"