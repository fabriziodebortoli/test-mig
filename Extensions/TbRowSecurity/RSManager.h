#pragma once 

#include <TbXmlCore\XmlSaxReader.h>
#include <TbNameSolver\TBResourceLocker.h>
#include <TbOleDb\TbExtensionsInterface.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlSession;
class RSProtectedTableInfo;
class TRS_Subjects;
class TRS_SubjectsGrants;
class CSubjectsManager;
class CSubjectCache;
class CSubjectCacheArray;
class RSEntityInfo;
class CRSHierarchyRowArray;
class CRSEntityArray;
class CEntityGrantsArray;
class DBTEntitySubjectsGrants;

//e' il manager legato ad una singola tabella interpellato in fase di filtraggio 
///////////////////////////////////////////////////////////////////////////////
//						CTableRowSecurityMng definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTableRowSecurityMng : public CTableRowSecurityMngObj, public CTBLockable
{
private:
	SqlSession* m_pSqlSession;	
	RSProtectedTableInfo* m_pProtectedInfo; //contiene le informazioni di protezione

public:
	CTableRowSecurityMng() : m_pProtectedInfo(NULL),m_pSqlSession(NULL)  {}
	CTableRowSecurityMng(SqlSession*, RSProtectedTableInfo*);
	virtual ~CTableRowSecurityMng();

public:
	virtual void	AddRowSecurityFilters(SqlTable*, SqlTableItem*);	
	virtual CString		GetSelectGrantString(SqlTable* pTable); //used in HotKeyLink class	
	virtual void	ValorizeRowSecurityParameters(SqlTable* pTable);
	virtual BOOL	CanCurrentWorkerUsesRecord(SqlRecord*, SqlTable*);
	virtual void	HideProtectedFields(SqlRecord*); 

public:
	virtual LPCSTR  GetObjectName() const { return "RowSecurityManager"; }
};

///////////////////////////////////////////////////////////////////////////////
//						CEntitiesManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CEntitiesManager
{
private:
	SqlSession*		m_pSqlSession;
	CRSEntityArray*	m_pEntityArray;
	CString			m_strUsedEntities;

public:
	CEntitiesManager(SqlSession* pSession);
	~CEntitiesManager();

public:
	void		LoadInfoFromXML				(CXMLNode* pnNode);
	void		SetProtectionInformation	(const CString& strUsedEntities);
	void		SetUsedEntities				(const CString& strUsedEntities);
	void		UpdateUsedEntities			(const CString& strUsedEntities); 
	BOOL		ValorizeRowSecurityID		();
	SqlRecord*	GetEntityMasterRecord		(const CString& strEntityName, const DataLng& nRowSecurityID);

public:
	RSEntityInfo*	GetEntityInfo		(const CString& strEntityName, BOOL bAddIfNotExist);
	RSEntityInfo*	GetEntityInfo		(const CString& strEntityName);
	BOOL			IsEntityDocument	(const CTBNamespace& docNS); //dato il namespace di un documento restituisce TRUE se il documento corrisponde al documento che gestisce un entità da proteggere FALSE altrimenti
	BOOL			IsEntityMasterTable (const CTBNamespace& tableNS);//dato il namespace di un SqlRecord restituisce TRUE se il SqlRecord corrisponde alla master table un entità da proteggere FALSE altrimenti
	RSEntityInfo*	GetEntityInfo		(const CTBNamespace& docNS);

	CStringArray*	GetAllEntities		() const;
	CString			GetUsedEntities		() const { return m_strUsedEntities; }
	CRSEntityArray*	GetEntityArray		() const { return m_pEntityArray; }
};

///////////////////////////////////////////////////////////////////////////////
//						CSubjectsManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CSubjectsManager
{
private: 
	SqlSession*			m_pSqlSession;
	int					m_nMaxLevel;

	CSubjectCacheArray*	m_pSubjectsCacheArray;
	CSubjectCacheArray*	m_pOldSubjectsCacheArray;

public:
	CSubjectsManager	(SqlSession*);
	~CSubjectsManager	();

public:
	CSubjectCache*		GetSubjectCache				(int nSubjectID);
	CSubjectCache*		GetSubjectCacheFromWorkerID	(int nWorkerID);
	CSubjectCache*		GetSubjectCacheFromResource	(const CString& resourceType, const CString& resourceCode);

	CSubjectCache*		GetOldSubjectCache			(int nSubjectID);

	CSubjectCacheArray* GetSubjectCacheArray		() const;

	int	GetSubjectID		(int nWorkerID);

	void LoadInformation	();
	void LoadSubjects		();
	void LoadHierarchies	();

	void CreateOldSubjectsCacheArray	();
	void SetMaxLevel					(int nLevel) { if (m_nMaxLevel < nLevel) m_nMaxLevel = nLevel; }
	int GetMaxLevel						() const { return m_nMaxLevel; }
};

//classe che si occupa della scrittura dei grant su RS_SubjectsGrants
///////////////////////////////////////////////////////////////////////////////
//					CGrantsManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CGrantsManager
{
private: 
	SqlSession*			m_pSqlSession;
	CSubjectsManager*	m_pSubjectsManager;

public:
	CGrantsManager	(SqlSession*, CSubjectsManager*);
	~CGrantsManager	() ;

public:
	Array*	GetSubjectsToImplicitGrant	(int nWorkerID);
	Array*	GetSubjectsToExplictGrant	(int nSubjectID);

	void	ModifyImplicitGrant			(const CString& strEntityName, int rowSecurityID, int nOldWorkerID, int nNewWorkerID, CAbstractFormDoc* pDoc);	
	Array*	PutRecordUnderProtection	(const CString& strEntityName, SqlRecord* pRecord);

	void	SaveExplicitGrants			(DBTEntitySubjectsGrants* pDBTEntitySubjectsGrants);
	void	DeleteAllGrants				(const CString& strEntityName, int rowSecurityID);

	void    AddFilterForEntity			(SqlTable* pTable, const CString& strEntityName);
	void    SetRowSecurityIDParam		(SqlTable* pTable, const DataLng& rowSecurityID);
};

// classe di appoggio 
///////////////////////////////////////////////////////////////////////////////
//						CWorkerGrantRow definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CWorkerGrantRow : public CObject
{
public:
	CString m_Entity;
	int m_RowSecurityID;

public:
	CWorkerGrantRow(CString sEntity, int nRowSecurityID);
};

//classe che raggruppa le varie operazioni di manutenzione da eseguire per le tabelle RS*
///////////////////////////////////////////////////////////////////////////////
//					CRSMaintenanceManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CRSMaintenanceManager
{
private: 
	SqlSession*				m_pSqlSession;
	CSubjectsManager*		m_pSubjectsManager;
	CGrantsManager*			m_pGrantsManager;
	CEntitiesManager*		m_pEntitiesManager;

	CRSHierarchyRowArray*	m_pOldHierarchyRowsArray;		// record in RS_SubjectsHierarchy al tempo n-1
	CRSHierarchyRowArray*	m_pCurrentHierarchyRowsArray;	// record in RS_SubjectsHierarchy al tempo n

	CRSHierarchyRowArray*	m_pOldRowsNotFoundInCurrentArray; // array record variati nelle old hierarchies NON trovati nelle current hierarchies
	CRSHierarchyRowArray*	m_pCurrentRowsNotFoundInOldArray; // array record variati nelle current hierarchies NON trovati nelle old hierarchies

	CSubjectCacheArray*		m_pSubjectsToRemoveArray;

public:
	CRSMaintenanceManager	(SqlSession*, CSubjectsManager*, CGrantsManager*, CEntitiesManager*);
	~CRSMaintenanceManager	();

public:
	void DeleteAllSubjects		();
	void ManageSubjects			(CRSResourcesArray*);
	void ManageHierarchies		(CHierarchyArray*);
	BOOL ManageSubjectsGrants	();
	BOOL ValorizeRowSecurityID	();
	BOOL ExistRowsInTable		(const CString& strTableName);

private:
	void AddSubjects			(SqlTable&, TRS_Subjects&, CRSResourcesArray*);
	int	 GetSubjectElementPos	(TRS_Subjects&, CRSResourcesArray*);

	BOOL FillHierarchies		(CHierarchyArray*);
	BOOL PurgeHierarchies		();

	BOOL DeleteAllFromTable		(const CString& strTableName);

	void CompareHierarchyRowArray		();
	void AnalyzeOldHierarchiesRows		();
	void AnalyzeCurrentHierarchiesRows	();
	BOOL ManageOldSubjectsGrants		();
	BOOL ManageNewSubjectsGrants		();

	void RemoveGrantsForDeadSubjects	();
	void DeleteGrantsForSubject			(int nSubjectID);
	void DeleteSubjectAndHierarchies	(int nSubjectID);

	void AddGrant	(int nSubjectID, const CString& strEntityName, int nRowSecurityID, int nWorkerID, const DataEnum& eGrantType);
	void RemoveGrant(int nSubjectID, const CString& strEntityName, int nRowSecurityID);
};

///////////////////////////////////////////////////////////////////////////////
//					CRowSecurityManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CRowSecurityManager: public IRowSecurityManager, public CTBLockable
{
	DECLARE_DYNCREATE(CRowSecurityManager)

private:
	SqlSession*				m_pSqlSession;
	BOOL					m_bIsValid;
	//CMapStringToOb			m_EntitiesMap; //mappa che contiene le entità

public:
	CEntitiesManager*		m_pEntitiesManager;
	CGrantsManager*			m_pGrantsManager;
	CSubjectsManager*		m_pSubjectsManager;
	CRSMaintenanceManager*	m_pMaintenanceManager;

public:
	CRowSecurityManager();
	virtual ~CRowSecurityManager();

public:
	virtual LPCSTR		GetObjectName				() const { return "CRowSecurityManager"; }
			SqlSession*	GetSqlSession				() const { return m_pSqlSession; }	
			void		LoadProtectionInformation	(); //carica le informazioni presenti nei vari file RowSecurityObjects.xml apportati dai moduli applicativi
			void		LoadSingleRowSecurityInfo	(const CTBNamespace& aModuleNS);	
			BOOL		IsValid						() const { return m_bIsValid; }

public:	//CSubjectsManager
	virtual void				CreateOldSubjectsCacheArray	()		 { m_pSubjectsManager->CreateOldSubjectsCacheArray(); };
			CSubjectCacheArray* GetSubjectCacheArray		() const { return m_pSubjectsManager->GetSubjectCacheArray(); }

public:	//CEntityManager
			RSEntityInfo*	GetEntityInfo		(const CString& strEntityName, BOOL bAddIfNotExist)	{ return m_pEntitiesManager->GetEntityInfo(strEntityName, bAddIfNotExist); }
			RSEntityInfo*	GetEntityInfo		(const CTBNamespace& docNS)							{ return m_pEntitiesManager->GetEntityInfo(docNS); }
			BOOL			IsEntityDocument	(const CTBNamespace& docNS)							{ return m_pEntitiesManager->IsEntityDocument(docNS); }  
			BOOL			IsEntityMasterTable (const CTBNamespace& tableNS)						{ return m_pEntitiesManager->IsEntityMasterTable(tableNS); }	
	virtual CStringArray*	GetAllEntities		() const											{ return m_pEntitiesManager->GetAllEntities(); }
	virtual CString			GetUsedEntities		() const											{ return m_pEntitiesManager->GetUsedEntities(); }
	virtual void			UpdateUsedEntities	(const CString& strUsedEntities)					{ return m_pEntitiesManager->UpdateUsedEntities(strUsedEntities); }

public:	//CGrantsManager
	virtual void	ModifyImplicitGrant			(const CString& strEntityName, int rowSecurityID, int nOldWorkerID, int nNewWorkerID, CAbstractFormDoc* pDoc);
	virtual Array*	PutRecordUnderProtection	(const CString& strEntityName, SqlRecord* pRecord); //mette sotto protezione la singola istanza dell'entità
	virtual void	AddFilterForEntity			(SqlTable* pTable, const CString& strEntityName)	{ m_pGrantsManager->AddFilterForEntity(pTable, strEntityName); }
	virtual void	SetRowSecurityIDParam		(SqlTable* pTable, const DataLng& rowSecurityID)	{ m_pGrantsManager->SetRowSecurityIDParam(pTable, rowSecurityID); }

public: //CRSMaintenanceManager
	virtual void ManageSubjects			(CRSResourcesArray* pResources) { m_pMaintenanceManager->ManageSubjects(pResources); }
	virtual void ManageHierarchies		(CHierarchyArray* pHierarchies) { m_pMaintenanceManager->ManageHierarchies(pHierarchies); }
	virtual void DeleteAllSubjects		()								{ m_pMaintenanceManager->DeleteAllSubjects(); };
	virtual BOOL ManageSubjectsGrants	()								{ return m_pMaintenanceManager->ManageSubjectsGrants(); } 
	virtual BOOL ValorizeRowSecurityID	()								{ return m_pMaintenanceManager->ValorizeRowSecurityID(); }	
	virtual BOOL ExistRowsInTable		(const CString& strTableName)	{ return m_pMaintenanceManager->ExistRowsInTable(strTableName); }	
	virtual BOOL UpdateRSConfiguration	(const BOOL& bSetValid);
};

//=========================================================================================
TB_EXPORT CRowSecurityManager*	AFXAPI AfxGetRowSecurityManager();
TB_EXPORT CSubjectsManager*		AFXAPI AfxGetSubjectsManager();
TB_EXPORT CGrantsManager*		AFXAPI AfxGetGrantsManager();
TB_EXPORT CSubjectCache*		AFXAPI AfxGetLoggedSubject();
TB_EXPORT CEntitiesManager*		AFXAPI AfxGetEntitiesManager();

#include "endh.dex"