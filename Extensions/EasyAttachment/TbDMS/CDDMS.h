#pragma once

#include <afxhtml.h>

#include <TbGenlib\basedoc.h>
#include <TbGenlib\DMSAttachmentInfo.h>
#include <TBGeneric\dataobj.h>
#include <TBGeneric\array.h>
#include <TbGes\extdoc.h>

#include "TbRepositoryManager.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CAttachmentPane;
class CDDMS;
class DBTBookmarks;
class DMSAttachmentInfo;
class DMSAttachmentsList;
class CSearchFieldList;
class CDMSCategories;
class BDDMSRepositoryBrowser;
class BDAcquisitionFromDevice; 
class CTBDMSBarcodeViewerCtrl;

enum CanBeSentToSOSType;

///////////////////////////////////////////////////////////////////////////////
//	class CDMSAttachmentManager:
//		classe base di interfaccia per permettere la gestione degli allegati da parte del documento
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDMSAttachmentManager : public CDMSAttachmentManagerObj
{
	DECLARE_DYNAMIC(CDMSAttachmentManager)

	friend class TbRepositoryManager;

private:
	CDDMS* m_pDMS;

public:
	CDMSAttachmentManager(CAbstractFormDoc* pDocument);

public:
	virtual ::ArchiveResult		AttachReport(const CString& strPdfFileName, const CString& strReportTitle, const CString& strBarcode, int& nAttachmentId, CString& strMessage);
	virtual ::ArchiveResult		AttachFile(const CString& StrFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage);
	virtual ::ArchiveResult		AttachBinaryContent(const DataBlob& binaryContent, const CString& strFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage);
	virtual ::ArchiveResult		AttachArchivedDocument(int nArchivedDocID, int& nAttachmentID, CString& strMessage);
	virtual	::ArchiveResult		AttachFromTable(CString& strMessage);
	virtual ::ArchiveResult		AttachPapery(const CString& strBarcode, const CString& strDescription, const CString& strReportName, CString& strMessage);
	virtual CAttachmentsArray*  GetAttachments(AttachmentFilterTypeEnum filterType);
	virtual CAttachmentInfo*	GetAttachmentInfo(int nAttachmentID);
	virtual void				OpenAttachmentsListForm(CUIntArray* pSelectedAttachmentID, bool onlyForMail);
	virtual bool				CreateNewSosDocument(int nAttachmentID, CString& strMessage);
	virtual bool				IsDocumentInSOS();
};

//===========================================================================
//							ManagedEventsHandler
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class MDMSDocOrchestratorEvents : public System::Object
{
public:
	CDDMS* cdDMS;

public:
	void InitializeEvents(CDDMS*, Microarea::EasyAttachment::BusinessLogic::DMSDocOrchestrator^);

public:
	void OnAttachCompleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentInfoEventArgs^);
	void OnAttachmentDeleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentEventArgs^);
	void OnUpdateCollectionCompleted(System::Object^, Microarea::EasyAttachment::Components::CollectionEventArgs^ eventArg);
	void OnSyncronizationIndexesFinished(System::Object^, System::EventArgs^ eventArg);
};

///////////////////////////////////////////////////////////////////////////////
//						CDDMS definition
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDDMS : public CClientDoc
{
	friend class TbRepositoryManager;

	DECLARE_DYNAMIC(CDDMS)

private:
	CDMSAttachmentManager* m_pDMSAttachmentManager;

	gcroot<Microarea::Framework::TBApplicationWrapper::MView^>				view;
	gcroot<Microarea::Framework::TBApplicationWrapper::MDocument^>			document;
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSDocOrchestrator^>	dmsDocOrchestrator;
	gcroot<MDMSDocOrchestratorEvents^>										dmsOrchestratorEventsMng;

	CObserverContext*	m_pObserverContext;
	Array				m_arEventToDetach;
	bool				m_bObserving;
	int					m_AttachCount;
	HINSTANCE			hInstance;
	CStringArray*		m_pShowAsDescriptionFields; 
	WoormDocPtr			m_pReportDoc;			

	//SOS
	int		m_SOSDocumentStatus;			//mappa l'enumerativo Microarea.TaskBuilderNet.Core.WebServicesWrapper:StatoDocumento
	CString m_SOSDocumentStatusToolTip;		//stringa da mostrare all'utente. Dipende dalla stato e mi viene fornita dal SOSManager
	int		m_SOSAttachmentID;

	TDisposablePtr<BDDMSRepositoryBrowser>	m_pRepositoryBrowser;
	TDisposablePtr<BDAcquisitionFromDevice> m_pAcquisitionFromDevice;

public:
	CAttachmentPane*	m_pAttachmentPane;
	BOOL				m_bEditMode;
	BOOL				m_bMailActivated;

	DataLng				m_nCurrAttachment;
	DMSAttachmentsList*	m_pAttachments;	
	DMSAttachmentInfo*	m_pCurrAttachmentInfo;
	
	DBTBookmarks*		m_pDBTBookmarks;
	CSearchFilter*		m_pCachedSearchFilter;

public: // variabili in uso nel JSON
	DataBool			m_bManualBarcodeDetection;
	DataBool			m_bEnableBarcode;
	DataBool			m_bAutomaticBarcodeDetection;

	// AddPapery variables
	DataStr						m_sPaperyDescripion;
	DataStr						m_sPaperyBarcodeValue;
	CString						m_sPaperyBarcodeType;
	DataStr						m_sPaperyNotes;
	CTBDMSBarcodeViewerCtrl*	m_pPaperyBarcodeViewer;
	//

	DataStr		m_sAttachmentsCounter;
	DataInt		m_nMaxDocNrToShow;

public:
	CDDMS();
	~CDDMS();

public:
	CAbstractFormDoc*	GetServerDoc();

public: //DMS events manager
	void OnAttachCompleted				(DMSAttachmentInfo*);
	void OnUpdateCollectionCompleted	();
	void OnSyncronizationIndexesFinished();

private:
	void StartObserving				();
	void EndObserving				(); 
	void InitJSONVariables			();
	void UpdateAttachmentsCounter	();

public:
	::ArchiveResult	AttachFiles				(CStringArray* pFilesToArchvie);
	::ArchiveResult AttachFile				(int& nAttachmentId, const CString& strFileName, const CString& strReportTitle, bool report, bool skipDetectBarcode = false, const CString& barcode = _T(""));
	::ArchiveResult AttachArchivedDocument	(int nArchivedDocId, int& nAttachmentId);
	::ArchiveResult AttachPapery			(const CString& strBarcode, const CString& strDescription, const CString& strFileName);
	::ArchiveResult AttachBinaryContent		(const DataBlob& binaryContent, const CString& strFileName, const CString& strDescription, int& nAttachmentID);
	::ArchiveResult AttachFromTable			();

	bool CreateNewSosDocument				(int nAttachmentId);
	void OpenAttachmentsListForm			(CUIntArray* pSelectedAttachments, bool onlyForMail);
	bool DeleteAttachment					(int nAttachmentId, int nArchivedDocId);
	void RunCreatePaperyReport				();
	void LoadAttachments					();
	void SetCurrentAttachment				();
	BOOL IsCurrAttachmentValid				();
	BOOL SaveCurrentAttachment				();
	void UndoChangesAttachment				();
	BOOL DeleteCurrentAttachment			();
	void SendCurrentAttachment				();
	void ViewCurrentAttachment				();
	void CheckOutCurrentAttachment			();
	void CheckInCurrentAttachment			();
	void DoDetectBarcodeForCurrentAttachInfo();

	::CanBeSentToSOSType CanBeSentToSOS		(CString& strMsg);
	BOOL CreateNewSosDocument				();
	bool IsDocumentNamespaceInSOS			();
	bool IsDocumentInSOS					();

protected:
	virtual BOOL OnInitDocument				();
	virtual BOOL OnAttachData				();
	virtual void OnSaveCurrentRecord		();
			void OnCloseServerDocument		();
	virtual BOOL OnToolbarDropDown			(UINT, CMenu& );
    virtual void Customize					(); 
	virtual void OnParsedControlCreated		(CParsedCtrl* pCtrl);

	virtual void OnGoInBrowseMode			();
	virtual void OnAfterSetFormMode			(CBaseDocument::FormMode oldFormMode);
	virtual BOOL OnOkNewRecord				();	
	virtual BOOL OnOkEdit					();
	virtual BOOL OnBeforeEscape				();
	virtual BOOL OnExtraEditTransaction		();
	virtual BOOL OnExtraDeleteTransaction	();
	virtual void OnAfterDelete				(); 
	virtual BOOL OnPrepareAuxData			();
	virtual BOOL SaveModified				();

	virtual WebCommandType OnGetWebCommandType(UINT commandID);

	//{{AFX_MSG(CDDMS)
	afx_msg void OnRunCreatePaperyReport			();
	afx_msg void OnRepositoryBrowserAttachCompleted	();
	afx_msg void OnScanProcessEnded					();
	//gestione toolbar
	//i comman del dropdown menu  e gli updatecommandui arrivano solo al clientdoc e non alla parseddlg o al dockpane
	afx_msg void OnNewAttachmentFromFileSystem	();
	afx_msg void OnNewAttachmentFromRepository	();
	afx_msg void OnNewAttachmentFromDevice		();	
	afx_msg void OnNewAttachmentFromPapery		();
	afx_msg void OnUndoCheckOutAttachment		();	
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CBookmarkFieldEvents
//=============================================================================
class TB_EXPORT CBookmarkFieldEvents : public CObject, public CDataEventsObj
{
	DECLARE_DYNAMIC(CBookmarkFieldEvents)

public:
	CObserverContext*		m_pObsContext;
	gcroot<Microarea::EasyAttachment::Components::BookmarkToObserve^> BookmarkToObserve;
	
public:
	CBookmarkFieldEvents(CObserverContext* pContext, gcroot<Microarea::EasyAttachment::Components::BookmarkToObserve^> bookmark);
	
public:
	virtual CObserverContext* GetContext() const { return m_pObsContext; }
	virtual void Fire(CObservable*, EventType ) { }
};

#include "endh.dex"
