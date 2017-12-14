#pragma once

#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>
#include <TbGenlib\TBLinearGauge.h>

#include "CommonObjects.h"
#include "SOSObjects.h"

#include "beginh.dex"

class BDSOSAdjustAttachments;

#define CHECK_SOSADJUSTATTACH_TIMER 100

//===========================================================================
//						DMSSOSAdjustAttachmentsEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class DMSSOSAdjustAttachmentsEvents : public System::Object
{
public:
	BDSOSAdjustAttachments* m_pDoc;

public:
	void InitializeEvents(BDSOSAdjustAttachments*, Microarea::EasyAttachment::BusinessLogic::SOSManager^);

public:
	void OnSOSOperationCompleted	(System::Object^, Microarea::EasyAttachment::Components::SOSEventArgs^);
	void OnAdjustAttachmentsFinished(System::Object^, System::EventArgs^);
};

//////////////////////////////////////////////////////////////////////////////
//			       class BDSOSAdjustAttachments definition
//////////////////////////////////////////////////////////////////////////////
//============================================================================
class TB_EXPORT BDSOSAdjustAttachments : public CWizardFormDoc
{
	DECLARE_DYNCREATE(BDSOSAdjustAttachments)
	friend class CSOSAdjustAttachmentsWizardFormView;

public:
	BDSOSAdjustAttachments();
	~BDSOSAdjustAttachments();

private:
	BOOL		m_bCanShowElaborationDlg;
	BOOL		m_bCanClose;

public:
	// data-member selezioni
	DataStr		m_DocumentClass;
	DataStr		m_DocumentType;
	DataStr		m_FiscalYear;

	DataStr				m_ElaborationMessage;
	DataDbl				m_nCurrentElement;
	CTBLinearGaugeCtrl*	m_pGauge;
	DataInt				m_GaugeRange;
	DataInt				m_Range;

	//selezione documenti nel bodyEdit
	BOOL				m_bSelectDeselect;
	CBEButton*			m_pBEBtnSelDesel;

public:
	DBTSOSDocuments*		m_pDBTSOSDocuments;		// dbt sosdocument da inviare
	CSOSSearchRules*		m_pSOSSearchRules;		// struttura contenente i filtri impostati nella form

	SOSDocClassList*		m_pSOSDocClassList;		// lista con elenco classi documentali per combo
	CStringArray*			m_pSOSDocTypeList;		// lista con elenco tipi documento per combo
	CStringArray*			m_pSOSFiscalYearList;	// lista con elenco fiscal year per combo

	RecordArray*			m_pSOSDocumentArray;	// lista sos doc estratti ritornati dal TbRepoManager

	DBTSOSElaboration*		m_pDBTSOSElaboration;	// dbt con elenco msg dell'elaborazione

private:
	SOSDocClassInfo*			m_pSOSDocClassInfo;
	CERPSOSDocumentType			m_ERPSOSDocumentType;
	CSOSDocTypeItemSource*		m_pSOSDocTypeItemSource;
	CSOSFiscalYearItemSource*	m_pSOSFiscalYearItemSource;

public:
	// oggetti di tipo C#
	gcroot<DMSSOSAdjustAttachmentsEvents^>								dmsSOSAdjustAttachEvents;
	gcroot<Microarea::EasyAttachment::BusinessLogic::DMSOrchestrator^>	dmsOrchestrator;

	gcroot<System::Collections::Generic::List<Microarea::EasyAttachment::Components::AttachmentInfo^>^> attachmentsList;

protected:
	virtual BOOL	OnAttachData			();
	virtual	void	DisableControlsForBatch	();
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
	BOOL ExtractAttachmentsToAdjust	();
	void OnSOSOperationCompleted	(CSOSEventArgs* args);
	void OnAdjustAttachmentsFinished();
	//gestioni filtri ERP
	void AddERPFilter				(const CString& name, DataObj* pFromData, DataObj* pToData);

	void StartTimer				();
	void EndTimer				();
	void DoOnTimer				();

private:
	void LoadDMSInformation		();
	void CreateSearchFilters	();
	void InitSelections			();
	BOOL CheckFilters			();
	void StepProgressBar		();
	void SetGaugeColors			();
	void SetProgressRange		(int nRange);

public:
	//{{AFX_MSG(BDSOSAdjustAttachments)
	afx_msg void OnFiscalYearChanged();
	afx_msg void OnDocClassChanged	();
	afx_msg void OnDocTypeChanged	(); 
	afx_msg void OnSelDeselClicked	();
	//}}AFX_MSG	 
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
