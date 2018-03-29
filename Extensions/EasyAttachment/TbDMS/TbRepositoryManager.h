#pragma once
#include <TbGeneric\DataObj.h>
#include <TbOleDb\SqlRec.h>
#include <TbOleDB\OleDbMng.h>
#include <TbOleDb\TbExtensionsInterface.h>
#include <TbGeneric\Array.h>
#include <TbGenlib\DMSAttachmentInfo.h>

#include "beginh.dex"

class MDMSOrchestrator;
class CDDMS;
class CDMSCategories;
class CDMSCategory;
class VSettings;
class CSearchFilter;
class DMSAttachmentsList;
class DMSAttachmentInfo;
class DMSCollectionList;
class BDDMSRepository;
class CSOSConfiguration;
class CDMSSettings;
class CSOSSearchRules;
class CDMSAttachmentManager;
class BDSOSDocSender;
class BDSOSAdjustAttachments;
class BDMassiveArchive;



// gestisce l'apertura della form di gestione del repository di EasyAttachment
// e fornisce le funzionalità necessarie per l'archiviazione massimale
///////////////////////////////////////////////////////////////////////////////
//						TbRepositoryManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT TbRepositoryManager : public IDMSRepositoryManager
{
	DECLARE_DYNCREATE(TbRepositoryManager)

private:
	MDMSOrchestrator*	m_pDMSOrchestrator;
	BOOL				b_IsTemporary;

	SqlConnection*		m_pDMSSqlConnection;

	// classi globali per i parametri del DMS e della SOS
	CDMSSettings*		m_pDMSSettings;
	CSOSConfiguration*	m_pSOSConfiguration;

public:
	TbRepositoryManager	();
	~TbRepositoryManager();

public:
	static DMSStatusEnum	CheckDMSStatus						(int dbRelease, CString& strMsg);
	static bool				ExistEmptyTBGuidValuesInERPDocument	();
	

	SqlConnection* GetDMSSqlConnection();

	CString GetEasyAttachmentTempPath();

	 //vecchie form c# da rimuovere quando saranno rifatte in C++
	void OpenBBTraylet		();
	void OpenBBSettings		();

	void InitializeManager	(CDDMS*					pCDDMS);
	void InitializeManager	(BDDMSRepository*		pBDDMSRepository);
	void InitializeManager	(BDSOSDocSender*		pBDSOSDocSender);
	void InitializeManager	(BDSOSAdjustAttachments* pBDSOSAdjustAttachments);
	void InitializeManager	(BDMassiveArchive*		pBDMassiveArchive);

	//metodi di IDMSRepositoryManager /////////////////////////////////////////////////

	//Attachment management without document instance
	CAttachmentInfo*	GetAttachmentInfo(int nAttachmentID);
	CString				GetAttachmentTempFile(int nAttachmentID);
	void				OpenAttachment(int nAttachmentID);	
	bool				GetAttachmentBinaryContent(int nAttachmentID, DataBlob& binaryContent, DataStr& fileName, DataBool& veryLargeFile);

	CAttachmentsArray*	GetAttachments(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType);
	CAttachmentsArray*	GetAttachmentsByGuid(const CString& strDocNamespace, const CString& strTbGuid, AttachmentFilterTypeEnum filterType);
	int					GetAttachmentsCount(const CString& strDocNamespace, const CString& strDocKey, AttachmentFilterTypeEnum filterType);
			
	bool				DeleteAttachment(int attachmentID);
	bool				DeleteDocumentAttachments(const CString& strDocNamespace, const CString& strDocKey, CString& strMessage);
	bool				DeleteAllERPDocumentInfo(const CString& strDocNamespace, const CString& strDocKey, CString& strMessage);

	//CheckOut/CheckOut operations
	bool				CheckIn(int nAttachmentID);
	bool				CheckOut(int nAttachmentID);
	bool				Undo(int nAttachmentID);
	
	//Enabled functions
	bool BarcodeEnabled();
	bool SosConnectorEnabled();	

	//SOS used to add ERP filter to BDSOSSender
	void	AddERPFilter(CBaseDocument* pDocument, const CString& name, DataObj* pFromData, DataObj* pToData);
	

	//used from Reporting Studio
	CString				GetPdfFileName(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName);
	CTypedBarcode		GetBarcodeValue(const CString& strReportNamespace, CBaseDocument* pCallerDoc, const CString& strAlternativeName, bool bArchivePdf);
	::ArchiveResult		ArchiveReport(const CString& strPdfFileName, const CString& strReportTitle, CBaseDocument* pCallerDoc, const CString& strBarcode, bool deletePdfFileName, CString& strMessage);
	::ArchiveResult		GeneratePapery(const CString& strReportName, const CString& strBarcode, CBaseDocument* pCallerDoc, CString& strMessage);

	//archiving operation
	::ArchiveResult		ArchiveFile(const CString& fileName, const CString& description, int& archivedDocID, CString& strMessage);
	::ArchiveResult		AttachArchivedDocInDocument(int archivedDocID, const CString& documentNamespace, const CString& primaryKey, int& nAttachmentID, CString& strMessage);

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	CStringArray*		GetAllExtensions	(); 
	DMSCollectionList*	GetUsedCollections	();

	//Gestione categories
	CDMSCategories* GetCategories		();
	CDMSCategory*	GetDMSCategory		(const CString& name) const;
	bool			SaveDMSCategory		(CDMSCategory* pCategory) const;
	bool			DeleteDMSCategory	(const CString& name) const;

	//gestione Settings
	CDMSSettings*	GetDMSSettings	();
	BOOL			SaveDMSSettings	();
	void			SetAttachmentPanelOptions(int nMaxDocNr);

	//Gestione Repository 
	// ritorna una lista di documenti archiviati (i filtri sono opzionali)
	DMSAttachmentsList* GetArchivedDocuments				(CSearchFilter* pSearchFilter = NULL);
	DMSAttachmentInfo*	GetAttachmentInfoFromArchivedDocId	(int archivedDocId);
	::Array*			GetERPDocumentAttachment			(int archivedDocId);
	bool				UpdateArchivedDoc					(DMSAttachmentInfo* pAttachmentInfo, CString newDescription, CString newFreeTag, CString newBarcode);
	CString				GetDocumentKeyDescription			(int attachmentID);
	
	void	OpenDocument						(const CString& strFilePath);
	CString GetArchivedDocTempFile				(int archivedDocID);
	bool	DeleteArchiveDocInCascade			(DMSAttachmentInfo* pAttachmentInfo);
	void	SaveArchiveDocFileInFolder			(int archivedDocID);
	void	SaveMultipleArchiveDocFileInFolder	(CUIntArray* pArchivedDocIds); 
	
	void	SendAsAttachments		(DMSAttachmentsList* pAttachmentsList);
	void	MultipleScanWithBarcode	(CString sFileName, CString sExtension, CStringArray* pAcquiredFiles); // utilizzato per l'acquisizione da device

	// gestione check-in/out
	bool	CheckIn				(DMSAttachmentInfo* pAttachmentInfo);
	bool	CheckOut			(DMSAttachmentInfo* pAttachmentInfo);
	bool	Undo				(DMSAttachmentInfo* pAttachmentInfo);

	bool IsValidEABarcodeValue	(CString sBarcode, BOOL bShowMessage = TRUE);
	bool SearchInContentEnabled	();

	// gestione SOS
	bool				IsDocumentNamespaceInSOS	(CString docNamespace);
	CSOSConfiguration*	GetSOSConfiguration			();
	BOOL				SaveSOSConfiguration		();
	BOOL				LoadSOSDocumentClasses		();
	
	// per l'aggiornamento della chiave alternativa TBGuid
	bool UpdateTBGuidInERPDocument();

public:
	//@@BAUZI DA RIMUOVERE
	void OpenQuickAttachForm();

	//CHIAMATE DAI WEBMETHODS
	bool			ArchiveFolder				(CString folderName);
	CString			MassiveArchiveUnattendedMode(CString folder, bool splitFile);
	bool			GetDefaultBarcodeType		(DataStr& type, DataStr& prefix);
	CTypedBarcode	GetNewBarcodeValue			();
	CString			SaveAttachmentFileInFolder	(int attachmentID, const CString& strFolder);
	::ArchiveResult	ArchiveBinaryContent		(DataBlob binaryContent, DataStr sourceFileName, DataStr description, DataLng& archivedDocID, DataStr& strMessage, CString barcode = _T(""));
	int				GetAttachmentIDByFileName	(CString documentNamespace, CString documentKey, CString fileName);

	//CHIAMATE DAI WEBMETHODS
	//metodo che permette di ricercare attachment di uno specifico documento con i seguenti criteri
	//- eventuale testo da ricercare
	//- dove effettuare la ricerca del testo
	//- lista degli eventuali chiavi di ricerca da utilizzare (formato: "bookmark1:valore1;bookmark2:valore2;bookmark3:valore4;"
	CAttachmentsArray* SearchAttachmentsForDocument	(CString docNamespace, CString docKey, CString searchText, SearchLocationEnum location, CString searchFields);
	CAttachmentsArray* SearchAttachments			(DataStr collectorName, DataStr collectionName, DataStr extType, DataDate fromDate, DataDate toDate, DataStr searchText, int location, DataStr searchFields);	
};

TB_EXPORT TbRepositoryManager*	AFXAPI AfxGetTbRepositoryManager();
TB_EXPORT SqlConnection*		AFXAPI AfxGetDMSSqlConnection();




#include "endh.dex"