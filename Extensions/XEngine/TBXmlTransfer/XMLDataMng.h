
#pragma once

#include <afxhtml.h>

#include <TBxmlcore\XMLSchema.h>
#include <TBxmlcore\XMLDocObj.h>

#include <TBGENLIB\basedoc.h>

#include <TBGeneric\dataobj.h>
#include <TBGeneric\array.h>


#include <TBGES\extdoc.h>
#include <TBGES\eventmng.h>
#include <TBGES\XMLLogManager.h>

#include <XEngine\TBXMLEnvelope\XMLEnvMng.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "ExpCriteriaObj.h"
#include "XMLImpExpDialog.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLExportCriteria;
class CXMLExportDocSelection;
class CXMLProfileInfo;
class CXMLDBTInfo;
class CXMLXRefInfo;
class CXMLDBTInfoArray;
class CExpCriteriaWizardDoc;
class CXMLDataManager;
class CExpImpAVIDlg;
class CDataManagerEvents;

#define XMLDATAMNG_MODE_DISPLAY_MSGBOXES		0x0001
#define XMLDATAMNG_MODE_UNATTENDED				0x0002
#define XMLDATAMNG_MODE_DEFAULT				XMLDATAMNG_MODE_DISPLAY_MSGBOXES 

#define IMPORT_RECFLD_SUCCEEDED				0
#define IMPORT_RECFLD_ERROR_INVALID_CALL	1
#define IMPORT_RECFLD_ERROR_NO_FIELD_NODE	2
#define IMPORT_RECFLD_ERROR_FIELD_NOTFOUND	3
#define IMPORT_RECFLD_ERROR_EXT_REFERENCE	4
#define IMPORT_RECFLD_ERROR_INVALID_VALUE	5

#define EXPORT_ACTION_ONLY_DATA				0
#define EXPORT_ACTION_ONLY_SCHEMA			1
#define EXPORT_ACTION_ONLY_PARAMS			2
#define EXPORT_ACTION_DATA_SCHEMA			3

#define IMPORT_ACTION_INSERT_UPDATE			0
#define IMPORT_ACTION_ONLY_INSERT			1
#define IMPORT_ACTION_ONLY_UPDATE			2
#define IMPORT_ACTION_DELETE				3


#define EXPORT_RESULT_ONE_RECORD			0
#define EXPORT_RESULT_MORE_RECORD			1

#define EXPORT_FORMAT_RESULT_ONE_FILE		0
#define EXPORT_FORMAT_RESULT_MORE_FILE		1


#define COOKIE_NAME			_T("Attempts.xml")
#define TAG_ATTEMPTS		_T("Attempts")
#define MAX_ATTEMPTS		10

//codifiche di errore utilizzate dalla diagnostica per lo smartdocument
#define INIT_DIAGNOSTIC_CODE	1		//errore verificato in fase di inizializzazione (esempio InitDocument)
#define READ_DIAGNOSTIC_CODE	2		//errore verificato in fase di lettura dei dati in un file xml (esempio dati non corretti)
#define IMPEXP_DIAGNOSTIC_CODE	3		//errore verificato in fase di export/import
#define DOC_DIAGNOSTIC_CODE		4		//errore verificato nel documento


void GetInformationFromNsUri(const CString& strNsUri, CString& strProfile, CPathFinder::PosType& ePosType, CString& strUserName);

// classi utilizzati per la parametrizzazione dei processi di import\export utlizzati dalla smart import\export
// (esempio per integrazione con office2003)

/////////////////////////////////////////////////////////////////////////////
//			class CSmartCommonParams declaration
/////////////////////////////////////////////////////////////////////////////
//
class CSmartCommonParams
{
public:
	CString					m_strUserName;
	CPathFinder::PosType	m_ePosType;
	DataArray*				m_pResults;
	CString					m_strProfile;

public:
	CSmartCommonParams(DataArray* pResults);

public:
	void	AddToResult(const CString& strResult) { if (m_pResults && !strResult.IsEmpty()) m_pResults->Add(new DataStr(strResult)); }
	CString GetFirstStringResult()	const { return (m_pResults && HasResultData()) ? m_pResults->GetAt(0)->Str() : _T(""); }
	BOOL	HasResultData()			const { return m_pResults && m_pResults->GetSize() > 0; }
	void	SetProfileInfoByNsUri(const CString strNsUri);

};

/////////////////////////////////////////////////////////////////////////////
//			class CSmartExportParams declaration
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSmartExportParams : public CSmartCommonParams
{
public:
	int						m_loadAction;
	BOOL					m_bUseApproximation;
	BOOL					m_bExportCurrentDocument;

public:
	CSmartExportParams(
						DataArray* pResults,
						BOOL bUseApproximation = FALSE,
						int loadAction = EXPORT_ACTION_ONLY_DATA
					   );

};

/////////////////////////////////////////////////////////////////////////////
//			class CSmartImportParams declaration
/////////////////////////////////////////////////////////////////////////////
//
class CSmartImportParams : public CSmartCommonParams
{
public:
	int		m_saveAction;
	BOOL	m_bOnlySetData; //se a TRUE i dati sono caricati nel documento ma non viene avviato nessun processo di salvataggio. //impr. 5320

public:
	CSmartImportParams(DataArray* pResults, int saveAction = IMPORT_ACTION_INSERT_UPDATE, BOOL bOnlySetData = FALSE);
};

/////////////////////////////////////////////////////////////////////////////
//			class CSmartMessageElement definition
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class CSmartMessageElement : public CObject
{
public:
	CXMLLogSpace::XMLMsgType m_nMsgType;
	int						 m_nCode;
	CString					 m_strSource;
	CString					 m_strMessage;

public:
	CSmartMessageElement(CXMLLogSpace::XMLMsgType, int, const CString&, const CString&); 

public:
	void Unparse(CXMLNode*);
};

//----------------------------------------------------------------------------
class CSmartXMLDiagnosticMng
{
private:
	Array*				m_pErrorList;
	Array*				m_pWarningList;
	CXMLDocumentObject* m_pXMLDocument;

public:
	CSmartXMLDiagnosticMng();
	~CSmartXMLDiagnosticMng();

private:
	BOOL CreateHeader (CXMLNode* pRoot, BOOL bMessageBoxEnable);

public:
	void AddMessage	(CXMLLogSpace::XMLMsgType, int nCode, LPCTSTR lpszSource, LPCTSTR lpszMessage); 
	void ClearMessages();
	// inserisce i messaggi provenienti dal documento
	void InsertDocumentMessage(CDiagnostic* pDiagnostic);
	// creo un file o una stringa contenente il solo nodo di diagnostic
	// ?possibile (ma non obbligatorio) utilizzare un nodo di Root per assegnarli gli attributi di TBNamespace e di XTechProfile 
	// (nonch?il NamespaceUri)
	CString CreateXMLErrorString	(CXMLNode* pRoot, BOOL bMessageBoxEnable = FALSE);
	// crea il nodo dignostic contenente le info di error e/o warning come figlio di pRoot
	void AppendDiagnosticNode		(CXMLNode* pRoot);

public: //inline
	BOOL HasMessages() const { return (m_pErrorList && m_pErrorList->GetSize() > 0) || (m_pWarningList && m_pWarningList->GetSize() > 0); }
	BOOL HasErrors()   const { return (m_pErrorList && m_pErrorList->GetSize() > 0); }
};


// classi per l'ottimizzazione dell'algoritmo di export/import
// Spiegazione:

// ogni elemento identifica il sqlrecord esportato dai valori dei campi di chiave 
// primaria e dal profilo di esportazione
/////////////////////////////////////////////////////////////////////////////
class CXMLRecordInfo : public CObject
{
public:
	CString			m_strProfileName;
	DataObjArray	m_aPrimaryKeyValues;
	CString			m_strFileName;
	long			m_lBookmark;

public:
	CXMLRecordInfo(SqlRecord*, const CString& strProfileName, const CString& strFileName = _T(""));
};


/////////////////////////////////////////////////////////////////////////////
class CQualifiedXMLDocument: public CObject
{
public:
	CQualifiedXMLDocument(const CString& strFileName, CXMLDocumentObject*	pXMLDomDoc) 
	{
		ASSERT(pXMLDomDoc);
		m_strFileName = strFileName;
		m_pXMLDomDoc = pXMLDomDoc;
	}
	
	virtual ~CQualifiedXMLDocument()
	{
		if (m_pXMLDomDoc)
			delete m_pXMLDomDoc;
	}

	CString					m_strFileName;
	CXMLDocumentObject*		m_pXMLDomDoc;
};

/////////////////////////////////////////////////////////////////////////////
class CXMLDomDocElement : public CObject
{
public:
	CString					m_strUrlData;
	CString					m_strFileName;
	CString					m_strXSLTFileName; //xsl style sheet to transform the document after the export
	CXMLDocumentObject*		m_pXMLDomDoc;

public:
	CXMLDomDocElement(const CString&);
	~CXMLDomDocElement();
public:
	void SetXMLDocument	(const CString&, CXMLDocumentObject*);
	
	BOOL ApplyAndSaveTransformXSLT(CAbstractFormDoc* pDocument);
};


/////////////////////////////////////////////////////////////////////////////
class CXMLDocElement : public CObject
{
public:
	AbstractFormDocPtr		m_pDocument;
	BOOL					m_bIsRootDoc; // se TRUE ?il root document
	Array					m_aXMLDomDocArray; // posso avere + dom associati al documento
	Array*					m_pProcessedRecordArray; // elenco dei segmenti di chiave che individuano 
													// i records gi?esportati/importati per il documento con il profilo di esportazione
	CXMLRecordInfo*			m_pCurrentRecord;	// x ottimizzare, tengo quello che sto processando
	CXMLDomDocElement*		m_pCurrentDomDoc;
	CString					m_strCurrFileName;
	BOOL					m_bOldUnattendedMode;

public:
	CXMLDocElement(CAbstractFormDoc*);
	CXMLDocElement(const CTBNamespace&, CBaseDocument *pAncestor, BOOL bCanRunOnlyBusinessObject = FALSE);
	~CXMLDocElement();
	void InitRecordsProcessed();

private:
	void FormatXMLDocForStandard(CXMLDocumentObject*, const CXMLProfileInfo*);
	void FormatXMLDocForSmart(CXMLDocumentObject*, const CXMLProfileInfo*);

public:
	CXMLDocumentObject* GetXMLDomDocument(const CString&, const CString&, const CXMLProfileInfo*, BOOL, BOOL, BOOL = FALSE, BOOL = FALSE);
	CXMLRecordInfo* GetExportedRecord	(SqlRecord*, const CString&);
	CXMLRecordInfo* GetImportedRecord	(SqlRecord*, const CString&);
	CXMLRecordInfo* GetProcessedRecord	(SqlRecord*, const CString&, const CString&);
	BOOL InsertExportedRecord			(SqlRecord*, const CString&);
	BOOL InsertFailedRecord				(SqlRecord*, const CString&);
	BOOL InsertProcessedRecord			(SqlRecord*, const CString&, const CString&);
	void SetFileReferences				(long, const CString&);

	BOOL ApplyAndSaveTransformXSLT		();

public:
	void			SetCurrentRecord(CXMLRecordInfo* pCurrentRecord)	{ m_pCurrentRecord = pCurrentRecord; }
	
	CXMLRecordInfo* GetCurrentRecord	()	const { return m_pCurrentRecord;}
};


/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class CProcessedNode : public CObject 
{
public:	
	CString		m_strFileName;
	CString		m_strBookmark;
	CXMLNode*	m_pNode;
	BOOL		m_bSucceded; //if the import of the node is failed

public:
	CProcessedNode(const CString& strFileName, const CString& strBookmark, CXMLNode* pNode, BOOL bSucceded)
	:
	m_strFileName	(strFileName),
	m_strBookmark	(strBookmark),
	m_pNode			(pNode),
	m_bSucceded		(bSucceded)
	{}

	~CProcessedNode()
	{
		delete m_pNode;
	}
};

/////////////////////////////////////////////////////////////////////////////
// CXMLExportImportManager
/////////////////////////////////////////////////////////////////////////////
class CXMLExportImportManager 
{
public:	
	CXMLDataManager*		m_pXMLDataManager;
	BOOL					m_bLoggingEnabled;
	Array*					m_pXMLDataExpImpArray;
	Array*					m_pXMLDocumentArray;
	CXMLDocElement*			m_pCurrentDocElem; // per ottimizzare. Evito il passaggio di parametri
	CXMLEnvelopeManager*	m_pEnvelopeMng;
	CXMLLogSession 			m_LogSession;
	CSmartXMLDiagnosticMng*	m_pSmartXMLDiagnosticMng;
	CString					m_strSiteCode;		// per la gestione dei conflitti di codifica in fase di import
	BOOL					m_bValidateOnParse;
	BOOL					m_bCreateSchemaFiles;
	BOOL					m_bCreateDataFiles;
	Array					m_arProcessedNodes;

	BOOL					m_bExistErrors;

public:
	CXMLExportImportManager(CXMLDataManager* pDataManager);
	~CXMLExportImportManager();

public:
	void			InitXMLDocumentArray	();
	void			InitRecordsProcessed	();

//logging methods
	void			InitLogSpace			(const CString& strName, const CString& strLogFolder);
	
	void			FlushLogSpace			();
	BOOL			ShowLogSpaces			(CXMLLogSpace::XMLMsgType *pRequiredType = NULL);
	BOOL			ApplyImportXSLT			(CXMLDocumentObject* pDocToTransform);
	BOOL			ApplyAndSaveTransformXSLT();
	
	BOOL			Validate				(CXMLDocumentObject* pDocToTransform, const CString strFileName);

	CXMLDocElement* GetXMLDocElement		(const CTBNamespace&, CBaseDocument* pAncestor, BOOL bCanRunOnlyBusinessObject = FALSE);
	void			GetDocsInEditMode		(const CTBNamespace&, Array&);
	CXMLDocElement* GetXMLDocElement		(CAbstractFormDoc* pDoc);
	CXMLRecordInfo*	GetExportedRecord		(SqlRecord*, const CString&); //restituisce il puntatore se il record ?gi?stato esportato
	CXMLRecordInfo*	GetImportedRecord		(SqlRecord*, const CString&); //restituisce il puntatore se il record ?gi?stato esportato
	BOOL			InsertExportedRecord	(SqlRecord*, const CString&);
	BOOL			InsertFailedRecord		(SqlRecord*, const CString&);
	void			SetCurrentDocElem		(CXMLDocElement*);
	void			SetCurrentDocElem		(const CTBNamespace&);
	void			SetCurrentDocElem		(CAbstractFormDoc* pDoc);
	void			SetCurrentRecord		(CXMLRecordInfo*);
	BOOL			Init					(CAbstractFormDoc*);
	void			EnableLogging			(BOOL bSet)	{ m_bLoggingEnabled = bSet; }
	BOOL			OutputMessage			(const CString&	strMessageToLog, CXMLLogSpace::XMLMsgType eMsgType = CXMLLogSpace::XML_ERROR, int nCode = 0, const CString&	strSource = _T(""));
	BOOL			OutputMessage			(UINT nMsgStringID, CXMLLogSpace::XMLMsgType eMsgType = CXMLLogSpace::XML_ERROR, int nCode = 0, const CString& strSource = _T(""));
	BOOL			AppendMessageDetail		(const CString&	strMessageToLog);
	BOOL			AppendMessageDetail		(UINT	nMsgStringID);
	void			RaiseLoggingLevel		();
	void			LowerLoggingLevel		();

	void			UseSmartXMLDiagnosticMng(); //per il magicdocument (Office integration) non utilizzo il logging ma la classe pi?semplice CSmartXMLDiagnosticMng
		

	CXMLDocElement*	GetCurrentDocElem ()			{ return m_pCurrentDocElem; }
	CXMLRecordInfo* GetCurrentRecord  ()	const	{ return (m_pCurrentDocElem) ? m_pCurrentDocElem->GetCurrentRecord() : NULL;}

	long			GetNextBookmark	();
	void			SetFileReferences(long lBookmark, const CString &strFileName);

	CXMLDocumentObject* GetXMLImportDomDocument	(const CString& strFileName, const CString& strPath, BOOL bTransform = FALSE, BOOL bAvoidValidation = FALSE);
	CXMLDocumentObject* GetXMLExportDomDocument	
							(
								const CString& strUrlData, 
								const CString& strFileName, 
								const CXMLProfileInfo* pXMLProfileInfo, 
								BOOL bNewXMLFile, 
								BOOL bDisplayMsgBox,
								BOOL bNextFile = FALSE,
								BOOL bForSmartDocument = FALSE

							) 
		{ return (m_pCurrentDocElem) ? m_pCurrentDocElem->GetXMLDomDocument(strUrlData, strFileName, pXMLProfileInfo, bNewXMLFile, bDisplayMsgBox, bNextFile, bForSmartDocument) : NULL; }

	//funzioni relative alla gestione dell'envelope
	void	CreateEnvelope		(BOOL bDisplayMsgBox = TRUE);
	BOOL	DropEnvelope		();	
	CXMLExpFileElem*	GetXMLExpFileName	(const CString& strTitle, CString& strXMLFile, int nPadding, BOOL& bNew) { return m_pEnvelopeMng ? m_pEnvelopeMng->GetXMLExpFileName(strTitle, strXMLFile, nPadding, bNew) : NULL; } 

	int AddEnvFile 
				(
					CXMLEnvFile::ContentFileType	eFileType, 
					LPCTSTR							lpszUrlDati,
					LPCTSTR							lpszProfile = NULL, 
					LPCTSTR							lpszEnvClass= NULL, 
					LPCTSTR							lpszDocName	= NULL, 
					int								nDocNum		= 0
				 ) 
		{ return (m_pEnvelopeMng) ? m_pEnvelopeMng->AddEnvFile(eFileType, lpszUrlDati, lpszProfile, lpszEnvClass, lpszDocName, nDocNum) : -1; }

	void	IncrementExpRecordCount	(CXMLExpFileElem* pExpElem, const CString& strXMLFileName,  int nDataInstancesNumb) { if (m_pEnvelopeMng) m_pEnvelopeMng->IncrementExpRecordCount(pExpElem, strXMLFileName,nDataInstancesNumb); }
	
	BOOL	IsRootFilePresent			()	const { return (m_pEnvelopeMng) ? m_pEnvelopeMng->IsRootFilePresent() : FALSE; }
	CString	GetTXEnvFolderLoggingPath	()	const { return (m_pEnvelopeMng) ? m_pEnvelopeMng->GetTXEnvFolderLoggingPath() : _T("");	}
	CString	GetTXEnvFolderSchemaPath	()	const { return (m_pEnvelopeMng) ? m_pEnvelopeMng->GetTXEnvFolderSchemaPath() : _T("");	}

	void	AddProcessedNode	(const CString&, const CString&, CXMLNode*, BOOL);
	BOOL	IsProcessedNode		(const CString&, const CString&, BOOL&) const;
	void	ManageProcessedNodes (BOOL bSuccess);

	//for tuning.Save the actions.xml files are been changed by the tuning process
	void	SaveEvents(Array* pChanges);

	void    InitTablesEvents();
};


/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXMLRowFilters : public CObject
{   
	friend class CXMLDataManager;

private:
	CString			  m_sXRefTableName;
	CXMLVariableArray m_RowFilters;

public:
	void	Clear ();

	CXMLRowFilters&	operator = (const CXMLRowFilters&);
};

//XTECH OPTIMIZATION
/////////////////////////////////////////////////////////////////////////////
//	CRequiredTabManagers class 
/////////////////////////////////////////////////////////////////////////////
class CRequiredTabManager : public CObject
{
public:
	CTabManager*	m_pTabMng;
	Array			m_arRequiredTabDlgs;
	CTabDialog*		m_pActivedTabDlg;

public:
	CRequiredTabManager(CTabManager* pTabMng) 
	:
	m_pTabMng		(pTabMng)	
	{
		m_pActivedTabDlg = pTabMng->GetActiveDlg();
	}

public:
	int Add(CTabDialog* pTabDlg) { return m_arRequiredTabDlgs.Add(pTabDlg); }
	CTabDialog* GetAt(int idx) const { return (CTabDialog*)(m_arRequiredTabDlgs.GetAt(idx)); }
};


/////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////
//	CXMLDataManager class 
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXMLDataManager : public CXMLDataManagerObj
{   
	friend class CXMLDocElement;
	friend class CXMLExportImportManager;

	DECLARE_DYNAMIC(CXMLDataManager)

public:
	enum XMLDataMngProcessStatus 
		{ 
			XML_EXPORTING_DATA, XML_IMPORTING_DATA,
			XML_EXPORT_SUCCEEDED, XML_IMPORT_SUCCEEDED,
			XML_EXPORT_SUCCEEDED_WITH_ERRORS, XML_IMPORT_SUCCEEDED_WITH_ERRORS,
			XML_EXPORT_FAILED, XML_IMPORT_FAILED,
			XML_EXPORT_ABORTED, XML_IMPORT_ABORTED,
			XML_PROCESS_IDLE
		};

protected:
	XMLDataMngProcessStatus m_nProcessStatus;

	CAbstractFormDoc*			m_pDoc;
	CXMLExportImportManager*	m_pXMLExpImpMng;
	CXMLExportDocSelection*		m_pExportDocSelection;
	short						m_nMode;
	CWizardFormDoc*				m_pWizardDocument;	

private:
	BOOL						m_bIsRootDoc;			// TRUE per il documento ROOT	
	BOOL						m_bIsExtRef;			// TRUE se sto esportando un external reference FALSE altrimenti (attenzione che lo stesso documento ROOT 
	BOOL						m_bIsPostBack;			// TRUE se sto effettuando l'esportazione di PostBack nel caso di MagicDocument o MagicLink
	
	BOOL						m_bErrorFound; 
	int							m_nDataInstancesNumb; // numero di istanze di master presenti in un file
	BOOL						m_bContinueExpImp;
	BOOL						m_bImportPendingData;	//per sapere se sto importando un'envelope vergine o sto ripetendo un'import fallito

	UINT						m_nDocsImported;
	UINT						m_nDocsFailed;

	BOOL						m_bExpDelRecord; // a TRUE: exporto i record deleted

	BOOL						m_bImportDownload;
	BOOL						m_bImportValidate;
	CString						m_strAppImportCriteriaFileName; //the name of import parameter file for scheduled import task

	CString						m_strAlternativeExportPath; //path di esportazione alternativa
	
	//using via MagicLink export\import procedure
	CSmartCommonParams*			m_pXMLSmartParam;
	CXMLProfileInfo*			m_pSmartProfile;	//il profilo utilizzato via MagicLink, viene effettuato il cache per ottimizzare i tempi. E' legato al cache del documento in g_pCachedDocument;

	CMapStringToString			m_maxStrTypes;		//used for extended string types(dengxiaobin)

	CTableEvents*				m_pTableEvents;
	CString						m_sLastEventsTableName;
	// m_bForceUseOldXTechMode = TRUE the magicLink SetData using always the old xtech mode: Fire events and always active tabdialog
	// m_bForceUseOldXTechMode =  FALSE otherwise;
	BOOL						m_bForceUseOldXTechMode;
	volatile bool				m_bCachedDocumentBusy;	//TRUE se il documento è in uso durante un'operazione di importazione/esportazione

public:
	BOOL						m_bBusy;
	CXMLRowFilters				m_RowFilters;

	CObserverContext*			m_pObserverContext;

public:

	CXMLDataManager								(CBaseDocument*, short = XMLDATAMNG_MODE_DEFAULT);
	virtual ~CXMLDataManager					();

public:
	BOOL			CreateXMLExpImpManager		();
	virtual	BOOL	IsExtRef					() const { return m_bIsExtRef; }

	virtual int		GetExportCmdMsg				() const;
	virtual void	SetUnattendedExportParams	(LPCTSTR, LPCTSTR, LPCTSTR, int, int, BOOL);
	virtual	BOOL	Export						();
	
			void	FinishExportWizard			();
			void	CancelExportWizard			();
	
	virtual int		GetImportCmdMsg				() const;
	virtual void	SetUnattendedImportParams	(BOOL, BOOL, LPCTSTR);
	
	virtual	BOOL	Import						(const CString& strEnvFolder, CString &strRetVal);
	virtual	BOOL	Import						();
	virtual	BOOL	Import						(CXMLEnvElemArray* pRXSelectedElems, CString *pstrRetVal = NULL);
	CString	GetCurrentSiteCode					() {return m_pXMLExpImpMng ? m_pXMLExpImpMng->m_strSiteCode : _T("");}
	static BOOL		IsNotEmptyDataObj			(DataObj*);

	//New add function to support magic document schema
	//@@CHINA : Generate Drop list based on enumerate data automatically
	void InsertEnumerationInSmartXMLSchema(SqlRecordItem* pRecItem, WORD enumValue, CString enumName, CXSDGenerator* pSchema);
	void InsertPrimaryKeyInSmartXMLSchema(SqlRecordItem* pRecItem, CXSDGenerator* pSchema);
	void SetCachedDocumentBusy	(bool bBusy = true) { m_bCachedDocumentBusy = bBusy; }
	bool GetCachedDocumentBusy	() { return m_bCachedDocumentBusy; }


	//impr. 5320
	virtual BOOL	SetDataFromXMLString(CString strXML,  const CString& strXSLTFileName);
	virtual CString	GetDataToXMLString(const CString& strProfileName, const CString& strXSLTFileName);	

	virtual CString	GetProfileName() const;



private:
	CString		FormatData						(DataObj*) const;
	BOOL		ExportRecordSet					(const CXMLProfileInfo* = NULL);
	BOOL		ExportCurrRecord				(const CXMLProfileInfo* = NULL, CXMLXRefInfo* = NULL, BOOL bNextFile = FALSE);
	CXMLNode*	ExportDBT						(DBTObject*, CXMLNode*, const CXMLProfileInfo*, int = 1, CXMLXRefInfo* = NULL);
	BOOL		ExportXMLSchemas				(CXMLProfileInfo* pProfileInfo, CAbstractFormDoc* pDoc, CStringArray* pProcessedArray = NULL, BOOL bSmartDocument = FALSE);
	BOOL		ExportXRefXMLSchemas			(CXMLXRefInfo* pXRefInfo, CStringArray* pProcessedArray = NULL);
	BOOL		OnOkToExportRow					(SqlRecord* pRecord);

	BOOL		InsertMasterUniversalKey		(DBTMaster*, CXMLNode*, const CXMLProfileInfo*);
	BOOL		ProcessExtRef					(CXMLXRefInfo*, LPCTSTR, SqlRecord*, CXMLNode*, BOOL = FALSE);
	BOOL		ValorizeExtRefNode				(CXMLNode* pExtRefsNode, CXMLXRefInfo* pXRefInfo, SqlRecord* pDBTRecord, BOOL bAddBookMark);
	void		AddIfNotExists					(Array* pRecordArray, SqlRecord* pRecord);

	BOOL	ExportExtRef						(CXMLXRefInfo*, LPCTSTR, SqlRecord*, CXMLNode* pExtRefsNode, BOOL bForSmartDocument);
	BOOL	LoadExtRefRecordInDocument			(CXMLXRefInfo*, BOOL bNextFile = FALSE, CXMLNode* = NULL, CXMLProfileInfo* pProfileInfo = NULL);
	BOOL	ExportExtRefRecords					(CXMLXRefInfo*, SqlRecord*, CXMLNode* pExtRefsNode, BOOL bForSmartDocument, CXMLProfileInfo* pProfileInfo = NULL);
	BOOL	ExportExtRefRecordsNotFromDocQuery	(CXMLXRefInfo*, SqlRecord*);
	BOOL	ExportExtRefToDBTSlave				(CXMLXRefInfo*, SqlRecord*, CAbstractFormDoc* pExtRefDoc, CXMLNode* pExtRefsNode, BOOL bForSmartDocument);
	BOOL	ImportDocumentList					(CXMLNode*, const CString&, const CString& = _T("")); 
	BOOL	ImportDocument						(CXMLNode*, DBTMaster*, BOOL = TRUE);
	BOOL	DeleteDocument						(DBTMaster *pDBTMaster, SqlRecord* pNewMasterRec);
	BOOL	ImportDBTSlaves						(CXMLNode*, BOOL = TRUE);
	int		ImportRecordField					(CXMLNode*, SqlRecord*, CString&);
	BOOL	ImportRecordFields					(CXMLNode*, SqlRecord*);
	void	GetForeignKeySegments				(DataObjArray& arSegs, SqlRecord* pRec, DBTSlave* pDBT, int nRow = -1);
	SqlRecord* GetCurrentRecord					(DataObjArray& arSegs, DBTSlaveBuffered* pDBT, CXMLDBTInfo::UpdateType eUpdateType, CXMLNode* pRowNode, int& nCurrentRow);

	CXMLNode* GetEnvelopeBackupNode				(const CString&, CXMLDocumentObject* pDoc);
	BOOL	BackupEnvelope						(CString strFileName);
	BOOL	LinkFiles							(const CString& strOldFileName, const CString& strFileName);
	BOOL	BackupRecords						();
	CXMLNodeChildsList* GetNodesToBackup		(CXMLDocumentObject * pDom, DataObjArray* pKeyArray, const CString &strBookmark);
	BOOL	TestCanRetryImport					();
	BOOL	RemoveSchemaFiles					();
	void	WaitWizardDocument					();
	BOOL	MoveSuccessEnvelope					();
	BOOL	MoveFailureEnvelope					();
	BOOL	SavePendingDOMS						();
	BOOL	ImportEnvelope						();
	BOOL	ImportRecordsFromXMLFile			(const CString&, const CString& = _T(""), BOOL = FALSE);
	BOOL	ImportExtRef						(CXMLNode*, SqlRecord*); 
	BOOL	ImportExtRef						(const CString& strFileName, const CString& strBookmark);
	BOOL	GetNewKey							(CXMLNode* pFieldsNode, CXMLNode* pUniversalKey, CAbstractFormDoc* pDoc, SqlRecord* pRecord);
	void	FireDBTSlaveRecEvents				(SqlRecord* pPrototypeRecord, SqlRecord* pCurrentRow, CXMLNode* pChildNode);
	void	FireRecEvents						(SqlRecord* pRec, CXMLNode* pRecordNode);
	void	UnlockFields						(DBTMaster* pDBTMaster);
	void	UnlockFields						(SqlRecord* pRecord);
	//void	InitDocumentForImpExp				(CAbstractFormDoc *pDoc, BOOL bForImport = FALSE);
	void	BeginListeningToDoc					(CAbstractFormDoc *pDoc);
	void	EndListeningToDoc					(CAbstractFormDoc *pDoc, BOOL bGetMsg = TRUE);
	BOOL	IsDocumentEmpty						(CXMLDocumentObject *pDOMDoc);
	BOOL	RearrangeUniversalKey				(CXMLNode *pUniversalKey, const CStringArray &strArray);
	BOOL	CheckUniversalKey					(CXMLNode *pNode, CAbstractFormDoc * pDoc);
	BOOL	HasToCreateFileData					(){return m_pXMLExpImpMng ? m_pXMLExpImpMng->m_bCreateDataFiles : FALSE;}
	SqlTable* GetUnlockedTable					();
	CXMLExpFileElem* GetXMLExpFileName			(CString& strXMLExpFile, const CXMLProfileInfo* lpProfileInfo, BOOL& bNewXMLFile);
	BOOL	SetEnvelopeManager					(CXMLProfileInfo* lpCurrentProfile);
	void	ShowExportResult					(BOOL bShowLog = TRUE);

	void 	ExportXMLData						(CXMLProfileInfo*);
	void 	ExportXMLSchema						(CXMLProfileInfo*);
	void 	ExportSmartXMLSchema				(CXMLProfileInfo*);

	RecordArray*	ExecJoinFromSlaves			(	
													CXMLXRefInfo*		pXRefInfo, 
													SqlRecord*			pDBTRecord,
													CAbstractFormDoc*	pExtRefDoc,
													CXMLNode*			pExtRefNode
												);
						

//il controllo sulle funzioni da chiamare è stato spostato da XMLEventMng a CXMLDataManager per risolvere il problema
// relativo al bug #
//Infatti prima di considerare la chiamata o meno di una funzione è necessario verificare la presenza del campo corrispondente
// (secondo quanto descritto nel file actions.xml) nell'envelope che si sta importando
	void ComposeEventsToFire(CTableEvents* pTableEvents, SqlRecord* pRecord, CXMLNode* pRecordNode);
	BOOL CanFireFunction(CFieldFunction* pFunction, SqlRecord* pRecord, CXMLNode* pRecordNode);
	void AddFieldToEventTable(const CString&, CXMLNode* pFieldsNode);
	void AddFieldToEventTable(CTableEvents* pTableEvents, CXMLNode* pFieldsNode);

	CXMLDocumentObject* CreateExportParametersFile (CString strProfileName, CPathFinder::PosType ePosType);

public:
	void	InitExportDocSelection				(LPCTSTR = NULL, LPCTSTR = NULL, LPCTSTR = NULL, int = EXPORT_ONLY_CURR_DOC, int = USE_PREFERRED_PROFILE, BOOL = FALSE, CAutoExpressionMng* = NULL);
	void	SetContinueImportExport				(BOOL bSet) { m_bContinueExpImp = bSet; }
	void	SetValidateOnParse					(BOOL bSet) { if (m_pXMLExpImpMng) m_pXMLExpImpMng->m_bValidateOnParse = bSet; }

	CWizardFormDoc*			CreateImportWizard	(CAbstractFormDoc* = NULL);
	CWizardFormDoc*			CreateExportWizard	(CAutoExpressionMng* = NULL, CAbstractFormDoc* = NULL);
	BOOL					RunExportWizard		(CXMLProfileInfo** = NULL, LPCTSTR = NULL, LPCTSTR = NULL, LPCTSTR = NULL, int = EXPORT_ONLY_CURR_DOC, int = USE_PREFERRED_PROFILE, BOOL = FALSE, CAbstractFormDoc* = NULL);
	BOOL					RunImportWizard		(CXMLEnvElemArray*, LPCTSTR	= NULL, CAbstractFormDoc* = NULL);

	//for import parameter from Schedule
	void					SetAppImportCriteriaFileName(const CString& strFileName);
	const CString&			GetAppImportCriteriaFileName() const { return m_strAppImportCriteriaFileName;}

	CTBNamespace			GetDocumentNamespace() const;
	CString					GetDocTitle			() const;
	CString					GetDocumentDataPath	() const;
	CString					GetProfilesPath		() const {return _T(""); };
	CXMLEnvelopeManager*	GetEnvelopeManager	() const { return m_pXMLExpImpMng ? m_pXMLExpImpMng->m_pEnvelopeMng : NULL;}
	BOOL					UseOldXTechMode		() const { return AfxGetXEngineObject()->UseOldXTechMode() || m_bForceUseOldXTechMode; }

	CPropertyPage*	CreateProfilesWizardPropPage(const CTBNamespace&) const;
	BOOL	GetUKCommonFunctionList				(CStringArray*) const;
	void	EnableLogging						(BOOL bSet)	{ if (m_pXMLExpImpMng) m_pXMLExpImpMng->EnableLogging(bSet); }
	BOOL	OutputMessage						(const CString&	strMessageToLog, CXMLLogSpace::XMLMsgType eMsgType = CXMLLogSpace::XML_ERROR, int nCode = 0, const CString& strSource = _T("")) const;
	BOOL	OutputMessage						(UINT nMsgStringID, CXMLLogSpace::XMLMsgType eMsgType = CXMLLogSpace::XML_ERROR, int nCode = 0, const CString& strSource = _T("")) const;
	BOOL	AppendMessageDetail					(UINT	nMsgStringID) const;
	BOOL	AppendMessageDetail					(const CString&	strMessageToLog) const;
	void	RaiseLoggingLevel					();
	void	LowerLoggingLevel					();

	void	UseSmartXMLDiagnosticMng			(){ if (m_pXMLExpImpMng) m_pXMLExpImpMng->UseSmartXMLDiagnosticMng(); }

public:
	void SetUnattendedMode			(BOOL = TRUE);
	BOOL IsInUnattendedMode			() const { return m_nMode & XMLDATAMNG_MODE_UNATTENDED;}

	BOOL IsDisplayingMsgBoxesEnabled() const { return m_nMode & XMLDATAMNG_MODE_DISPLAY_MSGBOXES;}
	
	BOOL CreateXMLSchemaFile				(const CString&, CAbstractFormDoc* = NULL);
	BOOL CreateXMLSchemaFile				(CXMLProfileInfo*, CAbstractFormDoc* = NULL);
	BOOL CreateXMLSchemaFile				(const CString&, const CTBNamespace&);

	void ExtendDateType						(CXSDGenerator &XMLSchema, LPCTSTR lpszExtendedType, LPCTSTR lpszBaseType);

	BOOL CreateExportXMLSchemaFile			(CXMLProfileInfo*, CAbstractFormDoc* = NULL);
	
	void InsertFieldsInXMLSchema			(DBTObject*, CXSDGenerator*, const CXMLProfileInfo*);
	void InsertMasterUKInXMLSchema			(DBTMaster*, CXSDGenerator*, const CXMLProfileInfo*);
	void InsertExtRefUKInXMLSchema			(DBTObject*, CXSDGenerator*, const CXMLProfileInfo*);
	void InsertDBTSlavesInXMLSchema			(DBTMaster*, CXSDGenerator*, const CXMLProfileInfo*);
	
	CString	GetDataObjType					(DataObj* pdataObj);

	CAbstractFormDoc*		GetDocument()				const { return m_pDoc;}
	CXMLExportDocSelection*	GetXMLExportDocSelection()	const { return m_pExportDocSelection; }	
	CXMLProfileInfo*		GetXMLProfileInfo	(CAbstractFormDoc* pDoc, const CString& strProfile, const CString& strDocNamespace = _T(""));

	BOOL					IsDataObjValue		(CXMLNode*, DataObj*) const;	

//XTECH optimization
public:
	BOOL	m_bTuningEnable;	

private: 
	Array	m_requiredTabManagers;
	CString	 m_strTuningEmailTo;
	CString	 m_strTuningEmailFrom;
		
	void PrepareRequiredTabDlgs		();
	void ActivateRequiredTabDlgs	();
	void RestoreActiveTabs			();
	void RegisterObservableDataField();
	void CheckChangingDataObj		(CFieldFunction* pFieldFunction);
	BOOL CheckEventsToFire			(SqlRecord* pRecord);

	//old behaviour
	void FreezeActiveTabs	(CUIntArray* pTabbers, CUIntArray* pTabs);
	void RestoreActiveTabs  (CUIntArray* pTabberList, CUIntArray* pTabList); 
	void ActivateAllTabs	();

public:
	void SetTuningEnable	(BOOL bTuningEnable)				{ m_bTuningEnable = bTuningEnable; }
	void SetTuningEMailTo	(const CString& strTuningEmailTo)	{ m_strTuningEmailTo = strTuningEmailTo;}
	void SetTuningEMailFrom	(const CString& strTuningEmailFrom)	{ m_strTuningEmailFrom = strTuningEmailFrom;}


	// SMART XTECH: per integrazione con OFFICE 2003
private:
	// COMMON
	BOOL	LoadSmartProfile			(const CString&);
	void	GetActionDescription		(int, CString&, CString&);
	BOOL	GetForeingnKeysValue		(CXMLNode*, DBTObject*, SqlRecord*, CXMLProfileInfo*, BOOL = FALSE);
	void	InitializeExportSmart		();

	// GENERAZIONE SCHEMA
	void InsertDiagnosticTagInSmartXMLSchema	(CXSDGenerator*);
	BOOL InsertParametersTagInSmartXMLSchema	(CXSDGenerator*, const CXMLProfileInfo*);
	void InsertDataTagInSmartXMLSchema			(DBTMaster*, CXSDGenerator*, const CXMLProfileInfo*);
	CXMLDBTInfo* InsertSingleDBTInSmartXMLSchema(DBTObject*, CXSDGenerator*, const CXMLProfileInfo*);
	void InsertDBTFieldsInSmartXMLSchema		(CXMLDBTInfo*, CXMLHotKeyLinkArray*, CXSDGenerator*, const CXMLProfileInfo*, SqlRecord*, CXMLXRefInfo* pXRefInfo = NULL);

	void InsertSingleDBTXRefInSmartXMLSchema(DBTObject*, CXMLHotKeyLinkArray*, CXSDGenerator*, const CXMLProfileInfo*, CXMLXRefInfo*);	
	void InsertXRefInSmartXMLSchema			(CXMLXRefInfo*,	CXMLHotKeyLinkArray*, CXSDGenerator*);
	void InsertHKLInSmartXMLSchema			(CXMLHotKeyLink*, SqlRecordItem*, BOOL, CXSDGenerator*);

	// IMPORT
	void EscapeDBTMaster					(SqlRecord*);
	void LockedIndexes						(CUIntArray*, BOOL, SqlRecord*, SqlRecord*);
	BOOL PreparePKForSmartImport			();
	BOOL ImportSmartExternalReference		(CXMLNode*, CXMLXRefInfo*);
	BOOL ImportSmartDBTExtReferences		(CXMLNode*, CXMLDBTInfo*);
	BOOL ImportSmartRecordFields			(CXMLNode*, DBTObject*, SqlRecord*, CXMLProfileInfo*, BOOL = FALSE);
	BOOL ImportSmartSingleDBTSlave			(CXMLNode*, DBTSlave*, CXMLProfileInfo*);

	BOOL ImportSmartDBTSlaves				(CXMLNode*, CXMLProfileInfo*);
	BOOL ImportSmartDBTMaster				(CXMLNode*, CXMLProfileInfo*, SqlTable* pTableToRelock, int);
	BOOL ImportSmartCurrentRecord			(CXMLNode*, CXMLProfileInfo*, int);
	
	// EXPORT	
	BOOL ExportSmartExternalReference	(CXMLProfileInfo*, CXMLXRefInfo*, CXMLNode*);
	BOOL ProcessSmartExtRef				(CXMLXRefInfo*, LPCTSTR, SqlRecord*, CXMLNode*);
	void ExportSmartRecordFields		(CXMLNode*, SqlRecord*,CXMLDBTInfo*);
	BOOL ExportSmartSingleDBT			(CXMLNode*, DBTObject*, CXMLProfileInfo*);
	BOOL ExportSmartDataSection			(CXMLNode*, DBTMaster*, CXMLProfileInfo*);
	BOOL ExportSmartCurrentRecord		(CXMLProfileInfo*);	
	BOOL ExtractAndExportRecords		(int, CXMLProfileInfo*, const CString& strMessage);
	BOOL ExportSmartWithFindQuery		(CXMLNode*, CXMLProfileInfo*);
	BOOL ExportSmartWithExportCriteria	(CXMLNode*, CXMLProfileInfo*);
	BOOL ExportSmartSingleRecord		(CXMLNode*, CXMLProfileInfo*);
	BOOL ExportSmartData				(CXMLNode*, CXMLProfileInfo*);
	void CreateExtRefParameters			(SqlRecord*, CXMLXRefInfoArray*, CStringArray*, CXMLNode*);
	void ExportSmartOnlyParameters		(CXMLDocumentObject*, CXMLProfileInfo*);

	CXMLDBTInfo::UpdateType GetDBTSlaveUpdateType	(DBTSlave* pDBTSlave, CString strUpdateType);
	

public:
	BOOL			ExportSmartDocument			(CXMLDocumentObject* pXMLParamDoc, CSmartExportParams*);
	BOOL			ImportSmartDocument			(const CString& strData, CSmartImportParams* pImportParams, BOOL bLoadParamFromNsUri = FALSE);
	BOOL			CreateSmartXMLSchemaFile	(const CXMLProfileInfo*, CAbstractFormDoc*);	
	CXSDGenerator*	GetSmartXMLSchemaString		(const CXMLProfileInfo* pProfileInfo, CAbstractFormDoc* pDoc);
};

/////////////////////////////////////////////////////////////////////////////
class CXMLDataManagerClientDoc : public CClientDoc
{
	DECLARE_DYNAMIC(CXMLDataManagerClientDoc)

public:
	CXMLDataManagerClientDoc();

protected:
	virtual BOOL OnAttachData		();
	virtual BOOL OnPrepareAuxData	() { return TRUE;}
	virtual BOOL OnOkTransaction	() { return TRUE;}
    virtual void Customize	(); 

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
class CXMLDataExportDoc : public CClientDoc
{
	DECLARE_DYNAMIC(CXMLDataExportDoc)

public:
	CXMLDataExportDoc();

public:
	CAbstractFormDoc*	GetServerDoc();
	void ExportData();
	bool CanExportData();
protected:
	virtual BOOL OnAttachData		();
	virtual BOOL OnPrepareAuxData	() {return TRUE;}
	virtual BOOL OnOkTransaction	() {return TRUE;}
    virtual void Customize	(); 
	
	//{{AFX_MSG(CXMLDataExportDoc)
	afx_msg void OnDataExport();
	afx_msg void OnUpdateExportXMLData	(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
class CXMLDataImportDoc : public CClientDoc
{
	DECLARE_DYNAMIC(CXMLDataImportDoc)

public:
	CXMLDataImportDoc();

public:
	CAbstractFormDoc*	GetServerDoc();
	void ImportData();
	bool CanImportData();
protected:
	virtual BOOL OnAttachData		(); 
	virtual BOOL OnPrepareAuxData	() {return TRUE;}
	virtual BOOL OnOkTransaction	() {return TRUE;}
    virtual void Customize			(); 

	//{{AFX_MSG(CXMLDataImportDoc)
	afx_msg void OnDataImport();
	afx_msg void OnUpdateImportXMLData	(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};




//=============================================================================
//			Class CDataFieldEvents
//=============================================================================
class TB_EXPORT CDataFieldEvents : public CDataEventsObj
{
public:
	const SqlColumnInfo*	m_pColumnInfo;
	CObserverContext*		m_pObsContext;

public:
	CDataFieldEvents(const SqlColumnInfo* pColumnInfo, CObserverContext* pContext) 
	:
	m_pColumnInfo	(pColumnInfo),
	m_pObsContext	(pContext)
	{
		m_bOwned = true;
	}

	virtual CObserverContext* GetContext() const { return m_pObsContext; }
	virtual void Fire(CObservable*, EventType ) { }
};



#include "endh.dex"

