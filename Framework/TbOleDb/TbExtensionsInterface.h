
#pragma once

#include <TbGenlib\expr.h>
#include <TbGenlib\\DMSAttachmentInfo.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTable;
class SqlSession;
class DataDate; 
class DBTMaster;
class CXMLFixedKeyArray;
class SqlTableItem;
class CAbstractFormDoc;
class RICheckNode;
class IMassiveValidationSettings;

#define AUDIT_INSERT_OP		1
#define AUDIT_UPDATE_OP		2
#define AUDIT_DELETE_OP		3
#define AUDIT_CHANGEKEY_OP	4

#define InfiniteCRMProvider _T("InfiniteCRM")
#define CRMInfinityProvider _T("CRMInfinity")
#define DMSInfinityProvider _T("DMSInfinity")

//=========================================================================
//	class CAuditingMngObj:
//		classe base di interfaccia per la gestione dell'auditing di una tabella
//=========================================================================
class TB_EXPORT CAuditingMngObj
{
public:
	virtual ~CAuditingMngObj() {};

public:
	virtual BOOL IsValid () const = 0;
	virtual void TraceOperation (int, SqlTable*) = 0;
	virtual void BindTracedColumns(SqlTable*) = 0;

	//chiamate da XTech per l'esportazione
	virtual void PrepareQuery(SqlTable*, DataDate&, DataDate&, int) = 0;
	virtual	BOOL PrepareDeletedQuery(SqlTable*, DBTMaster*, DataDate&, DataDate&) = 0;

	// per la creazione automatica di un report di woorm relativo ai dati di tracciatura del dbtmaster
	// del documento il cui namespace è passato come parametro. Inoltre sono passati l'array contenente
	// le eventuali fixedkey da usare nella rule di estrazione dei dati e il nome dell'utente-oppure allusers
	// per la memorizzazione del report
	virtual CTBNamespace* CreateAuditingReport(CTBNamespace*, CXMLFixedKeyArray*, BOOL bAllUsers,  const CString&) = 0;
};

//=========================================================================
//	class CDataSynchroManagerObj:
//		classe base di interfaccia per la gestione della sincronizzazione dei dati
//=========================================================================
//
class TB_EXPORT IDataSynchroManager : public CObject
{
public:
	enum DataSynchroStatus
	{
		SYNCHRONIZING_OUTBOUND, SYNCHRONIZING_INBOUND, IDLE
	};

protected:
	DataSynchroStatus	m_nSynchroStatus;

public:
	virtual ~IDataSynchroManager() {};

public:
	virtual BOOL NeedMassiveSynchro(const CString& providerName) const = 0;

	virtual	BOOL					IsMassiveSynchronizing	() = 0;
	virtual	BOOL					IsMassiveValidating		() = 0;

	//viene chiamato per la sincronizzazione massiva
//virtual BOOL					SynchronizeOutbound(const CString& providerName, const CString& xmlParameters, CString& strMessage) = 0;
	virtual BOOL					SynchronizeOutbound(const CString& providerName, const CString& xmlParameters, CString& strMessage, DataDate& startSynchroDate, BOOL bDelta = FALSE) = 0;

	// Validazione massiva
	virtual BOOL					ValidateOutbound(RICheckNode* pCheckerProviderNode, BOOL bCheckFK, BOOL bCheckXSD, CString filtersExcluded, CString& message) = 0;

	virtual BOOL					SaveSynchroFilter(const CString& docNamespace, const CString& providerName, const CString& xmlParameters, CString& strMessage) = 0;
	virtual CString					GetSynchroFilter(const CString& docNamespace, const CString& providerName) = 0;

	virtual BOOL					IsValid()						const = 0;
	
	virtual BOOL					IsProviderValid(const CString& providerName)	const = 0;

	virtual BOOL					IsProviderDisabled(const CString& providerName)	const = 0;

	virtual BOOL					GetSynchroProvider(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters) = 0;
	
	virtual BOOL					TestProviderParameters(const CString& providerName, CString& strErrMsg) = 0;

	virtual BOOL					TestProviderParameters(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, CString& strErrMsg, BOOL bDisabled = FALSE) = 0;

	virtual BOOL					CreateExternalServer(const CString& providerName, const CString& extservername, const CString& connstr, CString& strErrMsg) = 0;

	virtual BOOL					CheckCompaniesToBeMapped(const CString& providerName, CString& strCompaniesList, CString& strErrMsg) = 0;

	virtual BOOL					MapCompany(const CString& providerName, const CString& strAppReg, const int& strMagoCompany, const CString& strInfinityCompany, const CString& strTaxId, CString& strErrMsg) = 0;

	virtual BOOL					UploadActionPackage(const CString& providerName, const CString& strActionPath, CString& strErrMsg) = 0;

	virtual BOOL					SaveDataSynchProviderInfo(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, const CString& iafmodules, CString& strErrMsg) = 0;
	
	virtual BOOL					SetConvergenceCriteria(const CString& providerName, const CString& xmlCriteria, CString& strErrMsg) = 0;

	virtual BOOL					GetConvergenceCriteria(const CString& providerName, const CString& actionName, CString& xmlCriteria, CString& strErrMsg) = 0;

	virtual BOOL					GetSynchroProviderInfo(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters, CString& iafmodules, BOOL& skipcrtvalid) = 0;

	virtual BOOL					SetGadgetPerm(const CString& providerName, CString& strErrMsg) = 0;

	virtual BOOL					CheckVersion(const CString& providerName, CString& strMagoVersion, CString& strErrMsg) = 0;
	virtual BOOL					ImagoStudioRuntimeInstalled() = 0;
	virtual BOOL					IsAlive						() = 0;


	virtual IMassiveValidationSettings*	GetMassiveValidationSettings() = 0;

	DataSynchroStatus	GetStatus()							const { return m_nSynchroStatus; }
	void				SetStatus(DataSynchroStatus eStatus) { m_nSynchroStatus = eStatus; }


	BOOL IsSynchronizing() const { return m_nSynchroStatus != IDLE; }
};

//mi dice se il processo di sincronizzazione è abilitato ovvero:
// se è attibato TBDataSynchroClient
// se sulla company è stato abilitato l'utilizzo della sincronizzazione dei dati
// se almeno un synchro provider ha le info necessari alla sincronizzazione corrette (url, user e pwd)
//=========================================================================================
TB_EXPORT BOOL AFXAPI AfxDataSynchronizeEnabled();


//mi dice se il singolo synchro provider è abilitato ovvero:
// AfxDataSynchronizeEnabled() +
// il privider passato come argomento ha le info necessari alla sincronizzazione corrette (url, user e pwd)
//=========================================================================================
BOOL AFXAPI AfxSynchroProviderEnabled(const CString& strProviderName);


//restituisce l'interfaccia che gestisce il processo di sincronizzazone
//=========================================================================================
TB_EXPORT IDataSynchroManager* AFXAPI AfxGetIDataSynchroManager();

//=========================================================================================
struct MassiveSynchroInfo : public CObject
{
public:
	MassiveSynchroInfo(CString aProviderName, CStringArray* aDocumentsToSynch)
	{
		m_ProviderName = aProviderName;
		m_pDocumentsToSynch = aDocumentsToSynch;
	}

public:
	CString			m_ProviderName;
	CStringArray*	m_pDocumentsToSynch;
	DataDate		m_StartSynchDate;
	DataBool		m_bDeltaSynch;
};

//=========================================================================================
class TB_EXPORT IMassiveValidationSettings : public CObject
{
public:
	IMassiveValidationSettings() {}
		
public:
	virtual BOOL GetNeedMassiveValidation()				   = 0;
	virtual void SetNeedMassiveValidation(DataBool bValue) = 0;
};

//=========================================================================================
//	class CRSResourceElement: classe utilizzata dal gestionale per lo scambio di informazioni con TBRowSecurity
//=========================================================================================
class TB_EXPORT CRSResourceElement : public CObject
{
public:
	int m_WorkerID;
	BOOL m_IsWorker;
	CString	m_ResourceType;
	CString m_ResourceCode;
	CString m_Description;

public:
	CRSResourceElement(int nWorkerID, BOOL bIsWorker, const CString& strResourceType, const CString& strResourceCode, const CString& strDescription);
	CRSResourceElement(const CRSResourceElement&);

//public:
//	const CString& GetSubjectTitle();
};

//=========================================================================================
//	class CRSResourcesArray: classe utilizzata dal gestionale per lo scambio di informazioni con TBRowSecurity
//=========================================================================================
class TB_EXPORT CRSResourcesArray : public Array
{
public:
	CRSResourceElement* 	GetAt		(int nIndex)	const	{ return (CRSResourceElement*) Array::GetAt(nIndex);	}
	CRSResourceElement*&	ElementAt	(int nIndex)			{ return (CRSResourceElement*&) Array::ElementAt(nIndex); }
	
	CRSResourceElement* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	CRSResourceElement*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}
	
	int	AddWorker(int nWorkerID, const CString& strDescription) { return Array::Add(new CRSResourceElement(nWorkerID, TRUE, _T(""), _T(""), strDescription));}
	int	AddResource(const CString& strResourceType, const CString& strResourceCode, const CString& strDescription)	{ return Array::Add(new CRSResourceElement(-1, FALSE, strResourceType, strResourceCode, strDescription));}

	//crea l'elemento di tipo risorsa/worker se non esiste
	CRSResourceElement* GetResource(int nWorkerID, BOOL bIsWorker, const CString& strResourceType, const CString& strResourceCode);
};

//=========================================================================================
//	class CHierarchyElement: classe utilizzata dal gestionale per lo scambio di informazioni con TBRowSecurity
//=========================================================================================
class TB_EXPORT CHierarchyElement : public CObject
{
public:
	CRSResourceElement* m_pMasterElement;
	CRSResourceElement* m_pSlaveElement;
	int m_nrLevel;

public:
	CHierarchyElement(CRSResourceElement* pMasterElement, CRSResourceElement* pSlaveElement, int nrLevel);
};

//=========================================================================================
//	class CHierarchyArray: classe utilizzata dal gestionale per lo scambio di informazioni con TBRowSecurity
//=========================================================================================
class TB_EXPORT CHierarchyArray : public Array
{
public:
	CHierarchyElement* 	GetAt		(int nIndex)	const	{ return (CHierarchyElement*) Array::GetAt(nIndex);	}
	CHierarchyElement*&	ElementAt	(int nIndex)			{ return (CHierarchyElement*&) Array::ElementAt(nIndex); }
	
	CHierarchyElement* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	CHierarchyElement*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}
	
	int	Add	(CHierarchyElement* pElement)	{ return Array::Add(pElement); };

public:
	BOOL Contains		(CHierarchyElement*);
	BOOL MatchElements	(CRSResourceElement* pMasterElement, CRSResourceElement* pHElement);
};

//=========================================================================================
//	class CTableRowSecurityMngObj:
//		classe base di interfaccia per la gestione della protezione dei dati di una singola tabella
//=========================================================================================
class TB_EXPORT CTableRowSecurityMngObj
{
public:
	virtual ~CTableRowSecurityMngObj() {};

public:
	virtual void	AddRowSecurityFilters (SqlTable*, SqlTableItem*) = 0;	
	virtual CString GetSelectGrantString (SqlTable*) = 0;	
	virtual void	ValorizeRowSecurityParameters(SqlTable*) = 0;
	virtual BOOL	CanCurrentWorkerUsesRecord(SqlRecord*, SqlTable*) = 0;
	virtual void	HideProtectedFields(SqlRecord*) = 0;
};

//=========================================================================================
//	class IRowSecurityManager
//		Interfaccia per esporre i metodi esternamente a TbExtensions
//=========================================================================================
class TB_EXPORT IRowSecurityManager : public CObject
{
private:
	CString m_strMaintenanceDocNamespace; //è il namespace che identifica la procedura utilizzata per effettuare la manuntezione dei dati della rowsecurity (RowSecurityId, gerarchie...)
										  // dal TBF viene valorizzata con 	"Extensions.TbRowSecurity.TbRowSecurity.RSMaintenance" ma può essere personalizzata dall'applicativo gestionale 
										  // vedi OFM 
public:
	virtual ~IRowSecurityManager() {};

public:
	virtual void ManageSubjects				(CRSResourcesArray*) {};
	virtual void ManageHierarchies			(CHierarchyArray*) {};
	virtual void DeleteAllSubjects			() {};
	virtual void CreateOldSubjectsCacheArray() {};
	virtual BOOL ManageSubjectsGrants		() { return TRUE; }; 
	virtual BOOL ValorizeRowSecurityID		() { return TRUE; }; 
	virtual BOOL ExistRowsInTable			(const CString& strTableName) { return TRUE; }; 

	virtual BOOL UpdateRSConfiguration		(const BOOL& bSetValid) { return TRUE; };
	virtual BOOL IsValid					() const { return FALSE; }

	virtual void AddImplicitGrant			(const CString& strEntityName, int rowSecurityID, int workerID) {};
	virtual void RemoveImplicitGrant		(const CString& strEntityName, int rowSecurityID, int nWorkerID) {};
	virtual void ModifyImplicitGrant		(const CString& strEntityName, int rowSecurityID, int nOldWorkerID, int nNewWorkerID, CAbstractFormDoc* pBaseDoc) {};

	virtual CStringArray*	GetAllEntities		() const { return NULL; };
	virtual CString			GetUsedEntities		() const { return _T(""); };
	virtual void			UpdateUsedEntities	(const CString&) {}

	virtual void AddFilterForEntity			(SqlTable* pTable, const CString& strEntityName) {}
	virtual void SetRowSecurityIDParam		(SqlTable* pTable, const DataLng& rowSecurityID) {}

	void SetMaintenanceDocNamespace(const CString& docNamespace) { m_strMaintenanceDocNamespace = docNamespace; };
	const CString& GetMaintenanceDocNamespace() const {	return m_strMaintenanceDocNamespace; }
};


//=========================================================================================
//	class IDMSRepositoryManager
//		Interfaccia per esporre i metodi esternamente a Extension del DMSRepositoryManager del DMS
//=========================================================================================
class TB_EXPORT IDMSRepositoryManager : public CObject
{
public:
	virtual ~IDMSRepositoryManager() {};

public:
	virtual CAttachmentInfo*	GetAttachmentInfo			(int nAttachmentID) { return NULL; }
	virtual CString				GetAttachmentTempFile		(int nAttachmentID) { return _T(""); }
	virtual	void				OpenAttachment				(int nAttachmentID) { return; }
	virtual bool				GetAttachmentBinaryContent	(int nAttachmentID, DataBlob& binaryContent, DataStr& strFleName, DataBool& veryLargeFile) { return FALSE; }

	virtual CAttachmentsArray*  GetAttachments				(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType) { return NULL; }
	virtual CAttachmentsArray*  GetAttachmentsByGuid		(const CString& strDocNamespace, const CString& strDocGuid, AttachmentFilterTypeEnum filterType) { return NULL; }
	virtual int					GetAttachmentsCount			(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType) { return -1; }
	
	virtual bool				DeleteAttachment			(int nAttachmentID) { return FALSE; }
	virtual bool				DeleteDocumentAttachments	(const CString& strDocNamespace, const CString& strDocKey, CString& strMessage) { return FALSE; }
	virtual bool				DeleteAllERPDocumentInfo	(const CString& strDocNamespace, const CString& strDocKey, CString& strMessage) { return FALSE; }

	//CheckOut/CheckOut operations
	virtual bool				CheckIn						(int nAttachmentID) { return FALSE; }
	virtual bool				CheckOut					(int nAttachmentID) { return FALSE; }
	virtual bool				Undo						(int nAttachmentID) { return FALSE; }

	virtual bool				BarcodeEnabled				()	{ return FALSE; }
	virtual bool				SosConnectorEnabled			()	{ return FALSE; }
	virtual void				AddERPFilter				(CBaseDocument* pDocument, const CString& name, DataObj* pFromData, DataObj* pToData) {}

	
	//used from Reporting Studio
	virtual CString				GetPdfFileName				(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName) { return _T(""); }
	virtual CTypedBarcode		GetBarcodeValue				(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName, bool bArchivePdf) { CTypedBarcode aTypedBarcode; return aTypedBarcode; }
	virtual ::ArchiveResult		ArchiveReport				(const CString& strPdfFileName, const CString& strReportTitle, CBaseDocument* pCallerDoc, const CString& strBarcode, bool deletePdfFileName, CString& strMessages) { return ArchiveResult::Cancel; }
	virtual ::ArchiveResult		GeneratePapery				(const CString& strReportName, const CString& strBarcode, CBaseDocument* pCallerDoc, CString& strMessage) { return ArchiveResult::Cancel; }

	//archiving operation
	virtual ::ArchiveResult		ArchiveFile					(const CString& strFileName, const CString& strDescription, int& archivedDocID, CString& strMessage) { return ArchiveResult::Cancel; }
	virtual ::ArchiveResult		AttachArchivedDocInDocument	(int nArchivedDocID, const CString& strDocNamespace, const CString& strDocKey, int& nAttachmentID, CString& strMessage) { return ArchiveResult::Cancel; }
};



//=========================================================================================
TB_EXPORT BOOL AFXAPI AfxRowSecurityEnabled();

//=========================================================================================
TB_EXPORT IRowSecurityManager* AFXAPI AfxGetIRowSecurityManager();

//=========================================================================================
TB_EXPORT IDMSRepositoryManager* AFXAPI AfxGetIDMSRepositoryManager();


#include "endh.dex"
