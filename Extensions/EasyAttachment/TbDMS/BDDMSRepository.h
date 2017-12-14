#pragma once

#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>
#include <TbGenlib\TilePanel.h>

#include "CommonObjects.h"
#include "DMSSearchFilter.h"

#include "beginh.dex"

class BDAcquisitionFromDevice;
class BDDMSRepository;
class CWorkersListBox;

//===========================================================================
//						DMSRepositoryEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class DMSRepositoryEvents : public System::Object
{
public:
	BDDMSRepository* m_pDoc;

public:
	void InitializeEvents(BDDMSRepository*, Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^);

public:
	void OnArchiveDocCompleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentInfoEventArgs^);
};

///////////////////////////////////////////////////////////////////////////////
//					DBTBookmarksCategory definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTCategoriesBookmarks : public DBTBookmarks
{
	DECLARE_DYNAMIC(DBTCategoriesBookmarks)

public:
	DBTCategoriesBookmarks(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument);

protected:
	virtual void OnPrepareRow(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTArchivedDocuments definition
//		DBT per visualizzare un elenco di documenti archiviati
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTArchivedDocuments : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTArchivedDocuments)

public:
	DBTArchivedDocuments(CRuntimeClass*	pClass, CAbstractFormDoc* pDocument);

public:
	VArchivedDocument*	GetCurrent			() 			const	{ return (VArchivedDocument*)GetCurrentRow(); }
	VArchivedDocument*	GetArchivedDocument	(int nRow) 	const 	{ return (VArchivedDocument*)GetRow(nRow); }
	VArchivedDocument*	GetArchivedDocument	()		   	const	{ return (VArchivedDocument*)GetRecord(); }

	int					GetCurrentRowIdx	()			const 	{ return m_nCurrentRow; }
	BDDMSRepository*	GetDocument			()			const	{ return (BDDMSRepository*)m_pDocument; }

	virtual void		SetCurrentRow		(int nRow);
	virtual BOOL		LocalFindData		(BOOL bPrepareOld);

protected:
	virtual	void		OnDefineQuery		()	{}
	virtual	void		OnPrepareQuery		()	{}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTAttachmentLinks definition
//	DBT per visualizzare l'elenco di allegati di un documento archiviato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTAttachmentLinks : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTAttachmentLinks)

public:
	DBTAttachmentLinks(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument);

public:
	VAttachmentLink*	GetCurrent			() 			const	{ return (VAttachmentLink*)GetCurrentRow(); }
	VAttachmentLink*	GetAttachmentLink	(int nRow) 	const 	{ return (VAttachmentLink*)GetRow(nRow); }
	VAttachmentLink*	GetAttachmentLink	()		   	const	{ return (VAttachmentLink*)GetRecord(); }

	int					GetCurrentRowIdx	()			const 	{ return m_nCurrentRow; }
	BDDMSRepository*	GetDocument			()			const	{ return (BDDMSRepository*)m_pDocument; }

	virtual void		SetCurrentRow		(int nRow);
	virtual BOOL		LocalFindData		(BOOL bPrepareOld);

protected:
	virtual	void		OnDefineQuery		()	{}
	virtual	void		OnPrepareQuery		()	{}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class BDDMSRepository definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT BDDMSRepository : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(BDDMSRepository)
	friend class CDMSRepositoryView;

public:
	BDDMSRepository();
	~BDDMSRepository();

public:
	// data-member selezioni
	DataDate	m_FromDate;
	DataDate	m_ToDate;
	
	DataStr		m_sFileExtension;

	DataBool	m_bSelectWorkers;
	DataBool	m_bShowDisabledWorkers;
	DataStr		m_Workers;

	DataStr		m_FreeText;
	DataBool	m_bFileNameAndDescription;
	DataBool	m_bTags;
	DataBool	m_bBarcode;
	DataBool	m_bBookmarks;
	DataBool	m_bDocumentContent;

	DataBool	m_bAllExtractedDoc;
	DataBool	m_bFirstExtractedDoc;
	DataInt		m_nTopNrDocuments;

	// ERP filters
	DataBool	m_bAllRepository;
	DataBool	m_bOnlyCollection;
	DataStr		m_DocNamespace;
	DataLng		m_CollectionID;

	DMSCollectionList*			m_pCollectionList;
	DBTSearchFieldsConditions*	m_pDBTSearchFieldsConditions;

public:
	// puntatori ad oggetti grafici
	CWorkersListBox*			m_pWorkersListBox;
	CTBDMSBarcodeViewerCtrl*	m_pBarcodeViewer;

	CSearchFieldsItemSource*		m_pSearchFieldsItemSource;
	CSearchFieldValuesItemSource*	m_pSearchFieldValuesItemSource;
	CBookmarksBodyEdit*				m_pBookmarkBE;

	TDisposablePtr<BDAcquisitionFromDevice> m_pAcquisitionFromDevice;

public:
	// variabili interne al documento
	BOOL		m_bEditMode;
	DataBool	m_bOpenAsBrowser; // parametro per aprire il documento come RepoBrowser. Default = FALSE
	DataBool	m_bEnableBarcode;
	
	// gestione filtri in memoria BodyEdit
	int	m_nIsAttachmentColumnIdx;
	int m_nIsWoormReportColumnIdx;

public:
	DMSAttachmentInfo*		m_pCurrentAttInfo;			// AttachmentInfo della riga corrente del DBT
	DBTArchivedDocuments*	m_pDBTArchivedDocuments;	// dbt documenti archiviati estratti
	DBTCategoriesBookmarks*	m_pDBTCategoriesBookmarks;	// dbt elenco con categorie del documento archiviato
	DBTAttachmentLinks*		m_pDBTAttachmentLinks;		// dbt elenco allegati per il documento archiviato

	DMSAttachmentsList*		m_pAttachmentsList;			// lista doc archiviati
	CSearchFilter*			m_pSearchFilter;			// struttura contenente i filtri impostati nella form

	CUIntArray*				m_pArchivedDocIdsArray;		// array di ArchivedDocId selezionati nel RepoBrowser
	CSearchFilter*			m_pCachedSearchFilter;

	gcroot<DMSRepositoryEvents^> dmsRepositoryEvents;
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^> dmsOrchestrator;

protected:
	virtual BOOL	OnAttachData			();
	virtual	void	DisableControlsForBatch	();
	virtual BOOL	CanRunDocument			();
	virtual void	OnPrepareAuxData		(CTileDialog* pTile);
	virtual	CString	OnGetCaption			(CAbstractFormView* pView);

	virtual void	OnParsedControlCreated	(CParsedCtrl* pCtrl);
	virtual void	OnColumnInfoCreated		(ColumnInfo* pColInfo);
	virtual void	CustomizeBodyEdit		(CBodyEdit* pBodyEdit);

public:
	void ExtractArchivedDocuments	();

	void CheckOutFile				(CUIntArray* pIndexes = NULL);
	void UndoCheckOutFile			(CUIntArray* pIndexes = NULL);
	void EditArchivedDoc			();
	void SaveArchivedDoc			();
	void UndoArchivedDocChanges		();
	void SaveArchiveDocFileInFolder	(CUIntArray* pIndexes = NULL);
	void ShowDocuments				(CUIntArray* pIndexes = NULL);
	void DeleteDocuments			(CUIntArray* pIndexes = NULL);
	void SendDocuments				(CUIntArray* pIndexes = NULL);
	void ArchiveFiles				(const CStringArray* pDroppedFiles);
	void OnArchiveDocCompleted		(DMSAttachmentInfo* attachmentInfo);

	void DoSearchFieldChanged		();
	void DoSearchValueChanged		();
	void DoSelCollectionChanged		();
	void DoCollectionChanged		();

private:
	void EnableCurrentAttachControls();
	void InitSelections				(BOOL bClearSelections = FALSE);
	void CreateCacheSearchFilters	();
	void InitJSONVariables			();

	CTilePanel* GetTilePanel		(UINT nIDD);

	void DoExtractDocumentsEnded	();
	void DoEditDocumentChanged		();
	void DoAttachmentInfoChanged	();

	DMSCollectionInfo* GetFistAvailableCollection();

public:
	//{{AFX_MSG(BDDMSRepository)
	afx_msg void OnArchiveDocRowChanged				();
	afx_msg void OnSearchFieldRowChanged			();
	afx_msg void OnToolbarExtractDocuments			();
	afx_msg void OnUpdateToolbarExtractDocuments	(CCmdUI*);
	afx_msg void OnToolbarUndoExtractDocuments		();
	afx_msg void OnUpdateToolbarUndoExtractDocuments(CCmdUI*);
	afx_msg void OnNewDocumentFromFileSystem		();
	afx_msg void OnNewDocumentFromDevice			();
	afx_msg void OnAllDocumentsChanged				();
	afx_msg void OnSelectWorkersChanged				();
	afx_msg void OnShowDisabledWorkersChanged		();
	afx_msg void OnSelCollectionChanged				();
	afx_msg void OnCollectionChanged				();
	afx_msg void OnBarcodeChanged					();
	afx_msg void OnUpdateMoveToPrevRow				(CCmdUI*);
	afx_msg void OnUpdateMoveToNextRow				(CCmdUI*);
	afx_msg void OnUpdateMoveToFirstRow				(CCmdUI*);
	afx_msg void OnUpdateMoveToLastRow				(CCmdUI*);
	afx_msg void OnBECategoriesRowChanged			();
	afx_msg void OnScanProcessEnded					();
	//}}AFX_MSG	 

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//		       class BDDMSRepositoryBrowser definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT BDDMSRepositoryBrowser : public BDDMSRepository
{
	DECLARE_DYNCREATE(BDDMSRepositoryBrowser)
	friend class CDMSRepositoryView;

private:
	CAbstractFormDoc* m_pCallingDoc;

public:
	BDDMSRepositoryBrowser();

public:
	virtual BOOL OnOpenDocument	(LPCTSTR);

public:
	//{{AFX_MSG(BDDMSRepositoryBrowser)	
	afx_msg void OnToolbarAttach		();
	afx_msg void OnUpdateToolbarAttach	(CCmdUI*);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
