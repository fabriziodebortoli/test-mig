#pragma once 

#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>

#include "DSTables.h"
#include "UIProviders.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DProviders;

//////////////////////////////////////////////////////////////////////////////
//             class DBTProviders definition and implementation
//////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT DBTProviders : public DBTMaster
{ 
	DECLARE_DYNAMIC (DBTProviders)

public:
	DBTProviders (CRuntimeClass*, CAbstractFormDoc*);

public:
	TDS_Providers* GetProvider () const { return (TDS_Providers*) GetRecord(); }	

protected: // Gestione delle query
	DProviders*		GetDocument					() {return (DProviders*) m_pDocument;}
	virtual void	OnEnableControlsForFind		() {}

	virtual	void	OnDefineQuery		();
	virtual	void	OnPrepareQuery		();

	virtual	BOOL	OnCheckPrimaryKey	();
	virtual	void	OnPreparePrimaryKey	();
	virtual void	OnDisableControlsForEdit();
};


//=============================================================================
class TB_EXPORT DBTVProviderParams : public DBTSlaveBuffered
{ 
	DECLARE_DYNAMIC(DBTVProviderParams)
	
public:
	DBTVProviderParams
		(
			CRuntimeClass*		pClass, 
			CAbstractFormDoc*	pDocument
		);

public: 
	DProviders*			GetDocument		() 			const	{ return (DProviders*) m_pDocument; } 
	VProviderParams*	GetParameter	()			const	{ return (VProviderParams*) GetRecord(); }
	VProviderParams*	GetParameter	(int nRow)	const	{ return (VProviderParams*) GetRow(nRow); } 

public: 
	virtual void	OnPreparePrimaryKey			(int nRow, SqlRecord*);
	virtual void	OnDisableControlsForEdit	();

protected:
	// Gestiscono la query
	virtual	void	OnDefineQuery	();
	virtual	void	OnPrepareQuery	();

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual DataObj*	OnCheckUserData		(int nRow);
};


//=============================================================================
class TB_EXPORT DProviders : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DProviders)

public:
	DProviders();
	~DProviders();

public: 
	DBTProviders*	m_pDBTProviders;
	DBTVProviderParams*	m_pDBTProviderParameters;

	DataStr m_ProviderStatusImage;
	DataStr m_ProviderStatus;

	DataStr m_ProviderUrl;
	DataStr m_ProviderName;
	DataStr m_ProviderUsername;
	DataStr m_ProviderPassword;
	DataBool m_bDisabledChanged;

private:
	CSynchroProvider*	m_pProvider;
		
public:	
	TDS_Providers*	GetProvider			() const;     
	BOOL			DoTestConnection	(CString& strMessage);

private:
	BOOL			SetProviderParameters		();
	void			SetEnabledDMSProvider		();
protected:
	virtual BOOL	OnAttachData 				();
	virtual	BOOL	OnPrepareAuxData			();

	virtual BOOL	CanDoNewRecord				() { return FALSE; } //non posso aggiungere dei nuovi record. La lista dei provider è determinata dalla console
	virtual BOOL	CanDoDeleteRecord			() { return FALSE; } //non posso cancellare i provider. La lista dei provider è determinata dalla console
	virtual BOOL	CanDoEditRecord				() { return !AfxGetDataSynchroManager()->IsMassiveSynchronizing() && !AfxGetDataSynchroManager()->IsMassiveValidating();	}
	virtual BOOL	CanRunDocument				();
	virtual BOOL	OnOkTransaction				();
	virtual BOOL	OnOkEdit					();
	virtual void	OnExtraEditTransaction		();

	virtual void	OnUpdateTitle				(CTileDialog* pTileDialog);
	virtual BOOL	OnOpenDocument				(LPCTSTR);
protected:	
	// Generated message map functions
	//{{AFX_MSG(DProviders)
	//}}AFX_MSG
	afx_msg void OnDisableChanged();
	DECLARE_MESSAGE_MAP()
};














#include "endh.dex"
