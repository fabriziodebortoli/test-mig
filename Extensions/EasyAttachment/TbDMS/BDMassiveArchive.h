#pragma once

#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>

#include "CommonObjects.h"

#include "beginh.dex"
 
class DMSAttachmentInfo;
class BDMassiveArchive;
class CTBLinearGaugeCtrl;
class BDAcquisitionFromDevice;

#define CHECK_MASSIVEARCHIVE_TIMER 100

//===========================================================================
//						BarcodeManagerEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class BarcodeManagerEvents : public System::Object
{
public:
	BDMassiveArchive* m_pDoc;

public:
	void InitializeEvents(BDMassiveArchive*, Microarea::EasyAttachment::BusinessLogic::BarcodeManager^);

public:
	void OnMassiveRowAdded			(System::Object^, Microarea::EasyAttachment::Components::MassiveEventArgs^);
	void OnMassiveRowProcessed		(System::Object^, Microarea::EasyAttachment::Components::MassiveEventArgs^);	
	void OnMassiveOperationCompleted(System::Object^, System::EventArgs^);
};

///////////////////////////////////////////////////////////////////////////////
//						VFileToAttach definition
//			SqlVirtualRecord per i documenti da allegare nel DMS
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VFileToAttach : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VFileToAttach)

public:
	gcroot<Microarea::EasyAttachment::Components::AttachmentInfoOtherData^> attachInfoOtherData; // puntatore all'AttachmentInfo del C#

	::Array*			m_pErpDocumentBarcodes;  // array di oggetti di tipo VAttachmentLink, con elenco documenti di ERP a cui allegare il papery
	DMSAttachmentInfo*	m_pAttachmentInfo;

public:
	DataBool	l_IsSelected;
	DataLng		l_ArchivedDocID;
	DataLng		l_AttachmentID;
	DataStr		l_FileName;
	DataStr		l_FilePath;
	DataStr		l_MassiveStatus;
	DataStr		l_MassiveResult;
	DataStr		l_MassiveAction;
	DataStr		l_ResultBmp;
	DataStr		l_MassiveInfo;

public:
	VFileToAttach(BOOL bCallInit = TRUE);
	~VFileToAttach();

public:
	virtual void BindRecord();

public:
	DMSAttachmentInfo*	GetAttachmentInfo();
	::Array*			GetErpDocumentBarcodes();

public:
	static LPCTSTR		GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTFilesToArchive definition
//		DBT per visualizzare un elenco di documenti archiviati
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTFilesToArchive : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTFilesToArchive)

public:
	DBTFilesToArchive(CRuntimeClass* pClass, CAbstractFormDoc* pDocument, CString sName);

public:
	VFileToAttach*	GetCurrent		() 			const	{ return (VFileToAttach*)GetCurrentRow(); }
	VFileToAttach*	GetFileToAttach	(int nRow) 	const 	{ return (VFileToAttach*)GetRow(nRow); }
	VFileToAttach*	GetFileToAttach	()		   	const	{ return (VFileToAttach*)GetRecord(); }

	int					GetCurrentRowIdx	()	const 	{ return m_nCurrentRow; }
	BDMassiveArchive*	GetDocument			()	const	{ return (BDMassiveArchive*)m_pDocument; }

	virtual void		SetCurrentRow		(int nRow);
	virtual BOOL		LocalFindData		(BOOL bPrepareOld);

protected:
	virtual	void		OnDefineQuery		()	{}
	virtual	void		OnPrepareQuery		()	{}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTDocumentLinks definition
//	DBT con l'elenco di allegati di un documento "candidato" all'archiviazione
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTDocumentLinks : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTDocumentLinks)

public:
	DBTDocumentLinks(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument);

public:
	VAttachmentLink*	GetCurrent			() 			const { return (VAttachmentLink*)GetCurrentRow(); }
	VAttachmentLink*	GetAttachmentLink	(int nRow) 	const { return (VAttachmentLink*)GetRow(nRow); }
	VAttachmentLink*	GetAttachmentLink	()		   	const { return (VAttachmentLink*)GetRecord(); }

	int					GetCurrentRowIdx	()			const { return m_nCurrentRow; }
	BDMassiveArchive*	GetDocument			()			const { return (BDMassiveArchive*)m_pDocument; }

	virtual void		SetCurrentRow		(int nRow);
	virtual BOOL		LocalFindData		(BOOL bPrepareOld);

protected:
	virtual	void		OnDefineQuery		() {}
	virtual	void		OnPrepareQuery		() {}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class BDMassiveArchive definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT BDMassiveArchive : public CWizardFormDoc
{
	DECLARE_DYNCREATE(BDMassiveArchive)
	friend class CMassiveArchiveView;

public:
	BDMassiveArchive();
	~BDMassiveArchive();

private:
	BOOL	m_bCanShowElaborationDlg;
	BOOL	m_bCanClose;

public:
	DataBool			m_bSplitFile;
	BOOL				m_bSelectDeselect;

	//Gauge per elaborazione
	DataDbl				m_nCurrentElement;
	CTBLinearGaugeCtrl*	m_pGauge;
	DataInt				m_GaugeRange;
	DataInt				m_Range;
	DataStr				m_ElaborationMessage;

	// puntatori ad oggetti grafici
	CTBDMSBarcodeViewerCtrl*	m_pBarcodeViewer;
	CBEButton*					m_pBEBtnSelDesel;

public:
	DMSAttachmentInfo*		m_pCurrentAttInfo;		// AttachmentInfo della riga corrente del DBT
	DBTFilesToArchive*		m_pDBTFilesToAdd;		// dbt file da allegare
	DBTFilesToArchive*		m_pDBTProcessedFiles;	// dbt dei file processati
	DBTDocumentLinks*		m_pDBTDocumentLinks;	// dbt elenco allegati per il documento

	TDisposablePtr<BDAcquisitionFromDevice> m_pAcquisitionFromDevice;

	gcroot<BarcodeManagerEvents^> barcodeManagerEvents;
	gcroot<Microarea::EasyAttachment::BusinessLogic::BarcodeManager^> barcodeManager;
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^> dmsOrchestrator;
	gcroot<System::Collections::Generic::List<Microarea::EasyAttachment::Components::AttachmentInfoOtherData^>^> attachmentInfoOtherDataList;

public:
	virtual VFileToAttach*	GetVFileToAttach() { return (VFileToAttach*)m_pDBTFilesToAdd->GetRecord(); }

protected:
	virtual BOOL	OnAttachData			();
	virtual BOOL	CanRunDocument			();
	virtual void	OnBatchExecute			();
	virtual void	OnBatchCompleted		();
	virtual void	OnCloseDocument			();
	virtual	CString	OnGetCaption			(CAbstractFormView* pView);
	virtual BOOL	CanDoBatchExecute		();

	virtual void	OnPrepareAuxData		(CTileDialog* pTile);
	virtual void	CustomizeBodyEdit		(CBodyEdit* pBodyEdit);
	virtual void	OnWizardActivate		(UINT nPageIDD);
	virtual void	OnParsedControlCreated	(CParsedCtrl* pCtrl);

public:
	void AddFiles					(const CStringArray* pFilesToAdd);
	void OnSingleAttachCompleted	(DMSAttachmentInfo* attachmentInfo);
	void OnMassiveRowAdded			(VFileToAttach* pFileToAttach);
	void OnMassiveRowProcessed		(VFileToAttach* pFileToAttach);
	void OnMassiveOperationCompleted();
	void DoOnTimer					();
	void ShowDocument				(SqlRecord* pCurrentRow);
	BOOL CheckSelections			();
	void DoAttachmentInfoChanged	();

private:
	void InitSelections				();
	void StartTimer					();
	void EndTimer					();
	void StepProgressBar			();
	void SetGaugeColors				();
	void SetProgressRange			(int nRange);

public:
	//{{AFX_MSG(BDMassiveArchive)
	afx_msg void OnAddFileFromFileSystem();
	afx_msg void OnAddFileFromDevice	();
	afx_msg void OnAddFileRowChanged	();
	afx_msg void OnSelDeselClicked		();
	afx_msg void OnScanProcessEnded		();
	afx_msg void OnUpdateDeleteRow		(CCmdUI*);
	//}}AFX_MSG	 

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
