#pragma once

#include <TbGeneric\CheckRecursion.h>

#include "TResources.h"
#include "TWorkers.h"

#include "beginh.dex"

class TRResources;

//=============================================================================
enum DeleteWorkerResult { NOT_USED, USED, ERROR_OCCURRED };

/////////////////////////////////////////////////////////////////////////////
//						CCheckWorker declaration
// (richiamata nella OnDSNChanged per assegnare il workerID corrispondente
// alla login, utilizzato poi da TB per scrivere nel TBCreatedID e TBModifiedID)
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CCheckWorker : public CObject
{
	DECLARE_DYNCREATE(CCheckWorker)

public:
	CCheckWorker();

public:
	DataLng GetWorkerID();

public:
	void IntegrateConvertedWorkers();

private:
	DataStr m_WorkerDescription;
	DataStr m_WorkerLastName;

private:
	DataLng CreateWorker();
};

/////////////////////////////////////////////////////////////////////////////
//						CWorkersTable declaration
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CWorkersTable : public CWorkersTableObj
{
	DECLARE_DYNAMIC(CWorkersTable)

public:
	virtual void LoadWorkers();
};

//=============================================================================
// CResourcesFunctions : classe generica di funzioni per le risorse 
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT CResourcesFunctions : public CObject
{      
	DECLARE_DYNAMIC(CResourcesFunctions)

public:
	CResourcesFunctions(CAbstractFormDoc* pDoc);

private:
	CAbstractFormDoc*	m_pDoc;
	TRWorkers			m_TRWorkers;

public:
	BOOL	OpenEmailConnector	(CString aToEmailAddress, CString aFromEmailAddress = _T(""));
	BOOL	OpenEmailClient		(CString aToEmailAddress);
	BOOL	SendEmail			(CString aToEmailAddress, CString aFromEmailAddress = _T(""));
	BOOL	SendEmailbyWorker	(CString aToEmailAddress);
	BOOL	OpenUrl				(CString aUrl);
};

//=============================================================================
// CElemResourceRecursion : elemento utile per liste di controllo ricorsione
//=============================================================================
class TB_EXPORT CElemResourceRecursion : public CObject
{      
	DECLARE_DYNAMIC(CElemResourceRecursion)

public:
	CElemResourceRecursion
		(
			const DataStr&	sResourceType,
			const DataStr& 	sResourceCode,
			int	  nLev
		);

	const DataStr	m_sResourceType;
	const DataStr 	m_sResourceCode;
	int	  m_nLev;
};

typedef	CTypedPtrList <CObList, CElemResourceRecursion*> ElementRecursionList;

/////////////////////////////////////////////////////////////////////////////
//				CheckResourcesRecursion declaration
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CheckResourcesRecursion : public CObject
{
	DECLARE_DYNCREATE(CheckResourcesRecursion)

public:
	CheckResourcesRecursion(CAbstractFormDoc* pDoc = NULL);
	~CheckResourcesRecursion();

public:
	CCheckRecursion		m_CCheckRecursion;

	BOOL IsRecursive(	const DataStr& aParentResourceCode,
						const DataStr& aParentResourceType,
						const DataStr& aResourceCode,
						const DataStr& aResourceType);

private:
	CAbstractFormDoc*		m_pDoc;
	ElementRecursionList	m_ResourceList;
	DataInt					m_ResourceLevel;
	TRResources				m_TRResources;
	TRWorkers				m_TRWorkers;

	BOOL AddChildResources				(const DataStr& aParentResourceCode, const DataStr& aParentResourceType);
	BOOL AddChildResourcesFromWorkers	(const DataLng& aParentResourceCode);
	void Clear							();
};

//-----------------------------------------------------------------------------
DataBool SetWorkerByName(DataStr aUserName, DataStr aWorkerName, DataStr aWorkerLastName);
DataBool SetWorkerByID	(DataStr aUserName, DataLng aWorkerID);

#include "endh.dex"
