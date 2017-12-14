#pragma once 

#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>
#include <TbGenlib\TBPropertyGrid.h>

#include "CommonObjects.h"
//includere alla fine degli include del .H
#include "beginh.dex"

class DDMSSettings;

//////////////////////////////////////////////////////////////////////////////
//         class DBTDMSSettings definition 
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class TB_EXPORT DBTDMSSettings : public DBTMaster
{
	DECLARE_DYNAMIC(DBTDMSSettings)

public:
	DBTDMSSettings(CRuntimeClass*, CAbstractFormDoc*);

public:
	VSettings* GetVSettings		() const { return (VSettings*)GetRecord(); }

protected: // Gestione delle query
	DDMSSettings*	GetDocument	() { return (DDMSSettings*)m_pDocument; }

	virtual void	OnEnableControlsForFind	() {}
	virtual	void	OnDefineQuery			();
	virtual	void	OnPrepareQuery			();
	virtual	void	OnPrepareBrowser		(SqlTable* pTable);
	virtual	BOOL	OnCheckPrimaryKey		();
	virtual	void	OnPreparePrimaryKey		();
	virtual void	OnDisableControlsForEdit();
	virtual	BOOL	CheckTransaction		() { return TRUE; }

public:
	virtual BOOL	FindData	(BOOL bPrepareOld = TRUE);
	virtual BOOL	AddNew		(BOOL bInit = TRUE) { return TRUE; }
	virtual BOOL	Edit		() { return TRUE; }
	virtual BOOL	Delete		() { return TRUE; }
	virtual BOOL	Update		();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTDMSExtensions definition 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTDMSExtensions : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTDMSExtensions)

public:
	DBTDMSExtensions(CRuntimeClass*, CAbstractFormDoc*);

public:
	DDMSSettings*		GetDocument() { return (DDMSSettings*)m_pDocument; }
	VExtensionMaxSize*	GetVExtensionMaxSize() const { return (VExtensionMaxSize*)GetRecord(); }

public:
	// Gestiscono la query
	virtual	void		OnDefineQuery();
	virtual	void		OnPrepareQuery();

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey(int /*nRow*/, SqlRecord*);
	virtual	void		OnPreparePrimaryKey(int nRow, SqlRecord*);

	virtual BOOL		LocalFindData(BOOL bPrepareOld);
};

//////////////////////////////////////////////////////////////////////////////
//     class DDMSSettings definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DDMSSettings : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DDMSSettings)

public:
	DDMSSettings();
	~DDMSSettings();

private:
	BOOL m_bFileStorageIsValid;

public:
	DBTDMSSettings*		m_pDBTSettings;
	DBTDMSExtensions*	m_pDBTExtensions;
	CDMSSettings*		m_pDMSSettings;

	DataBool			m_bDMSSOSEnable;

public:
	VSettings*			GetVSettings	() const;

protected:
	virtual BOOL	OnAttachData 		();
	virtual BOOL	CanRunDocument		();
	virtual BOOL	CanDoEditRecord		();
	virtual BOOL	CanSaveParameters	() { return AfxGetBaseApp()->GetNrOpenDocuments() == 1; }

	virtual BOOL	OnOkTransaction		();
	virtual	BOOL	OnPrepareAuxData	();
	virtual void	OnPropertyCreated	(CTBProperty* pProperty);

	// della toolbar mi servono solo i bottoni di edit, save ed escape
	virtual BOOL	CanDoDeleteRecord		()	{ return FALSE; }
	virtual BOOL	CanDoNewRecord			()	{ return FALSE; }
	virtual void	OnExtraEditTransaction	();

public:
	//{{AFX_MSG(DDMSSettings)	
	afx_msg void OnEnableFTS				();
	afx_msg void OnEnableBarcode			();
	afx_msg void OnEnableSOS				();
	afx_msg void OnEnableStorageToFileSystem();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"