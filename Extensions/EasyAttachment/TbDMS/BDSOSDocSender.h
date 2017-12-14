#pragma once

#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>
#include <TbGenlib\TBLinearGauge.h>

#include "CommonObjects.h"
#include "SOSObjects.h"

#include "beginh.dex"

#define CHECK_SOSDOCSENDER_TIMER 100

//===========================================================================
//						DMSSOSDocSenderEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class DMSSOSDocSenderEvents : public System::Object
{
public:
	BDSOSDocSender* m_pDoc;

public:
	void InitializeEvents(BDSOSDocSender*, Microarea::EasyAttachment::BusinessLogic::SOSManager^);

public:
	void OnSOSOperationCompleted(System::Object^, Microarea::EasyAttachment::Components::SOSEventArgs^);
	void OnSOSOperationFinished	(System::Object^, System::EventArgs^);
};

//////////////////////////////////////////////////////////////////////////////
//			       class BDSOSDocSender definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT BDSOSDocSender : public CWizardFormDoc
{
	DECLARE_DYNCREATE(BDSOSDocSender)
	friend class CSOSDocSenderWizardFormView;

public:
	BDSOSDocSender();
	~BDSOSDocSender();

private:
	BOOL		m_bCanShowElaborationDlg;
	BOOL		m_bCanClose;

public:
	// data-member selezioni
	DataBool	m_bSendDocToSOS;
	DataBool	m_bExcludeDocFromSOS;

	DataStr		m_DocumentClass;
	DataStr		m_DocumentType;
	DataStr		m_TaxJournal;
	DataStr		m_FiscalYear;
	DataBool	m_bOnlyMainDoc;

	DataBool	m_bDocIdle;
	DataBool	m_bDocToResend;

	DataStr				m_ElaborationMessage;
	DataDbl				m_nCurrentElement;
	CTBLinearGaugeCtrl*	m_pGauge;
	DataInt				m_GaugeRange;
	DataInt				m_Range;

	//selezione documenti nel bodyEdit
	BOOL				m_bSelectDeselect;
	CBEButton*			m_pBEBtnSelDesel;

	CSOSDocTypeItemSource*		m_pSOSDocTypeItemSource;
	CSOSTaxJournalItemSource*	m_pSOSTaxJournalItemSource;
	CSOSFiscalYearItemSource*	m_pSOSFiscalYearItemSource;

public:
	DBTSOSDocuments*		m_pDBTSOSDocuments;		// dbt sosdocument da inviare
	DBTSOSElaboration*		m_pDBTSOSElaboration;	// dbt con elenco msg dell'elaborazione
	CSOSSearchRules*		m_pSOSSearchRules;		// struttura contenente i filtri impostati nella form

	SOSDocClassList*		m_pSOSDocClassList;		// lista con elenco classi documentali per combo
	CStringArray*			m_pSOSDocTypeList;		// lista con elenco tipi documento per combo
	CStringArray*			m_pSOSTaxJournalList;	// lista con elenco tax journal per combo
	CStringArray*			m_pSOSFiscalYearList;	// lista con elenco fiscal year per combo
	RecordArray*			m_pSOSDocumentArray;	// lista sos doc estratti ritornati dal TbRepoManager

public:
	// oggetti di tipo C#
	gcroot<DMSSOSDocSenderEvents^>										dmsSOSDocSenderEvents;
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^>	dmsOrchestrator;

	gcroot<System::Collections::Generic::List<Microarea::EasyAttachment::Components::AttachmentInfo^>^> attachmentsToSendList;
	gcroot<System::Collections::Generic::List<System::String^>^> documentTypesList; // lista di tipi documento per classe documentale ritornati dal TbRepoManager
	gcroot<System::Collections::Generic::List<Microarea::TaskBuilderNet::Core::WebServicesWrapper::StatoDocumento>^> docStatusList;

protected:
	virtual BOOL	OnAttachData			();
	virtual	void	DisableControlsForBatch();
	virtual BOOL	CanRunDocument			();
	virtual void	OnBatchExecute			();
	virtual void	OnBatchCompleted		();
	virtual void	OnCloseDocument			();
	virtual BOOL	CanDoBatchExecute		();

	virtual void	CustomizeBodyEdit		(CBodyEdit* pBodyEdit);
	virtual void	OnParsedControlCreated	(CParsedCtrl* pCtrl);
	virtual void	OnEnableWizardNext		(CCmdUI* pCmdUI);
	virtual void	OnEnableWizardBack		(CCmdUI* pCmdUI);
	virtual LRESULT OnWizardNext			(UINT);
	virtual void	OnWizardActivate		(UINT nIDD);
			
public:
	BOOL ExtractSOSDocuments	();
	void OnSOSOperationCompleted(CSOSEventArgs* args);
	void OnSOSOperationFinished	();
	
	void StartTimer				();
	void EndTimer				();
	void DoOnTimer				();

	//gestioni filtri ERP
	void AddERPFilter(const CString& name, DataObj* pFromData, DataObj* pToData);

private:
	void LoadSOSDocClasses		();
	void CreateSearchFilters	();
	void InitSelections			();
	BOOL CheckFilters			();
	void StepProgressBar		();
	void SetGaugeColors			();
	void SetProgressRange		(int nRange);

public:
	//{{AFX_MSG(BDSOSDocSender)
	afx_msg void OnDocClassChanged		();
	afx_msg void OnDocTypeChanged		();
	afx_msg void OnTaxJournalChanged	();
	afx_msg void OnFiscalYearChanged	();	
	afx_msg void OnStatusIdleChanged	();
	afx_msg void OnStatusToResendChanged();
	afx_msg void OnSelDeselClicked		();
	//}}AFX_MSG	 

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
