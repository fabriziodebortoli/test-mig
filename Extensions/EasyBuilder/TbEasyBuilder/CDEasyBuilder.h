#pragma once

#include <TbGes/tabber.h>
#include <TbGes/ExtdocClientDoc.h>
#include <TbGes/EVENTMNG.H>
#include <TbGes/ExtdocAbstract.h>
#include <TbGeneric/DockableFrame.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CFormEditor;
class CDocumentManager;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::EasyBuilder;
using namespace Microarea::EasyBuilder::MVC;
using namespace Microarea::EasyBuilder::UI;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::Framework::TBApplicationWrapper;


//////////////////////////////////////////////////////////////////////////////////	
//								AddNewTagType enum
//////////////////////////////////////////////////////////////////////////////////	
enum class AddNewTagType
{
	None,
	AddNewCustomization,
	AddNewStandardization,
	Forbidden
};


//////////////////////////////////////////////////////////////////////////////////	
//								CustomizationInfo class declaration
//////////////////////////////////////////////////////////////////////////////////	
ref class CustomizationInfo
{
private:
	bool runFromButton;
	bool loadAllCustomizations;
	bool documentRestarted;
	BOOL centerControls;
	bool isEasyStudioDesigner;
	INameSpace^ customizationNamespace; //programmativo
	INameSpace^ documentNamespace;  //del documento lanciato
	DocumentController^ controller;
		
public:
	property bool RunFromButton { bool get() { return runFromButton; }	void set(bool value) { runFromButton = value; } }
	property bool LoadAllCustomizations			{ bool get ()		{ return loadAllCustomizations; }	void set(bool value){loadAllCustomizations = value;} }
	property bool DocumentRestarted				{ bool get ()		{ return documentRestarted; }		void set(bool value){documentRestarted = value;} }
	property bool IsEasyStudioDesigner { bool get() { return isEasyStudioDesigner; }		void set(bool value) { isEasyStudioDesigner = value; } }
	property BOOL CenterControlsActive			{ BOOL get ()		{ return centerControls; } }
	property INameSpace^ CustomizationNamespace	{ INameSpace^ get () { return customizationNamespace; }	void set(INameSpace^ value){customizationNamespace = value;} }
	property INameSpace^ DocumentNamespace		{ INameSpace^ get () { return documentNamespace; }	void set(INameSpace^ value){documentNamespace = value;} }
	property DocumentController^ Controller { DocumentController^ get() { return controller; }	void set(DocumentController^ value) { controller = value; } }

public:
	CustomizationInfo();
};

//////////////////////////////////////////////////////////////////////////////////	
//								CDEasyBuilder class declaration
//////////////////////////////////////////////////////////////////////////////////	
ref class CDManagedGate;
//=============================================================================
class CDEasyBuilder : public CClientDoc
{
friend class ControllersEventManager;

protected:
	DECLARE_DYNCREATE(CDEasyBuilder)

private:
	gcroot<FormEditor^>				formEditor;
	gcroot<DocumentControllers^>	documentControllers;
	gcroot<CDManagedGate^>			managedGate;
	gcroot<BusinessObject^>			caller;
	HINSTANCE						hInstance;
	bool							m_bNewDocument;
	CStringArray					controllersNameSpace;
	TCHAR*							m_pNsDocToRestart;

	enum DBTEvent { 
						Queried, 
						PreparePrimaryKey, 
						PrepareRow, 
						BeforeAddRow, 
						AfterAddRow, 
						BeforeInsertRow, 
						AfterInsertRow, 
						BeforeDeleteRow, 
						AfterDeleteRow,
						SetCurrentRow,
						PrepareAuxColumns
				};
	
public:
	CDEasyBuilder	();
	~CDEasyBuilder	();

public:
	void SetServerDocumentDesignMode(CBaseDocument::DesignMode dm) { if (m_pServerDocument) m_pServerDocument->SetDesignMode(dm); }

protected:
	BOOL PreTranslateMsg	(HWND hWnd, MSG* pMsg);
	BOOL OnGetToolTipText			(UINT nId, CString& strMessage);
	
	void Customize					();
	
	BOOL OnBeforeBatchExecute		();
	void OnDuringBatchExecute		(SqlRecord* /*pCurrProcessedRecord*/);
	void OnAfterBatchExecute		();
	
	void OnDocumentCreated			();
	void OnFrameCreated				();
	void OnBeforeCloseDocument		();
	void OnCloseServerDocument		();
	void OnBeforeBrowseRecord		();
	void OnGoInBrowseMode			();
	BOOL OnOkTransaction			();
	BOOL OnBeforeOkTransaction		();

	BOOL SaveModified				();
	BOOL OnOkDelete					();
	BOOL OnOkEdit					();
	BOOL OnOkNewRecord				();

	BOOL CanDoDeleteRecord			();
	BOOL CanDoEditRecord			();
	BOOL CanDoNewRecord				();

	BOOL OnBeforeDeleteRecord		();
	BOOL OnBeforeEditRecord			();
	BOOL OnBeforeNewRecord			();

	BOOL OnBeforeNewTransaction		();
	BOOL OnBeforeEditTransaction	();
	BOOL OnBeforeDeleteTransaction	(); 

	BOOL OnNewTransaction			();
	BOOL OnEditTransaction			();
	BOOL OnDeleteTransaction		(); 

	BOOL OnExtraNewTransaction		();
	BOOL OnExtraEditTransaction		();
	BOOL OnExtraDeleteTransaction	(); 

	CAbstractFormDoc::LockStatus	OnLockDocumentForNew		();	
	CAbstractFormDoc::LockStatus	OnLockDocumentForEdit		();
	CAbstractFormDoc::LockStatus	OnLockDocumentForDelete	();

	BOOL OnPrepareAuxData		 ();

	BOOL OnInitAuxData			 ();
	BOOL OnExistTables			 ();
	BOOL OnInitDocument			 ();

	void OnDisableControlsForBatch	();
	void OnDisableControlsForAddNew	();
	void OnDisableControlsForEdit	();
	void OnEnableControlsForFind	();
	void OnDisableControlsAlways	();
	void OnDisableControlsAlways	(CTabDialog* pTabDialog);

	virtual BOOL	OnAfterOnAttachData		();
	virtual void	OnPrepareBrowser		(SqlTable*);

	virtual void	OnPrepareAuxData		 (CTabDialog* pTabDialog);
	virtual void	OnPrepareAuxData		 (CAbstractFormView* pView);

	virtual void	OnPreparePrimaryKey	(DBTObject*);
	virtual void	OnPreparePrimaryKey	(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	virtual void	OnPrepareRow		(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	virtual BOOL	OnBeforeAddRow		(DBTSlaveBuffered*, int /*nRow*/);
	virtual void	OnAfterAddRow		(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	virtual BOOL	OnBeforeInsertRow	(DBTSlaveBuffered*, int /*nRow*/);
	virtual void	OnAfterInsertRow	(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	virtual BOOL	OnBeforeDeleteRow	(DBTSlaveBuffered*, int /*nRow*/);
	virtual void	OnAfterDeleteRow	(DBTSlaveBuffered*, int /*nRow*/);
	virtual	void	OnSetCurrentRow		(DBTSlaveBuffered*);
	virtual void	OnPrepareAuxColumns	(DBTSlaveBuffered*, SqlRecord*);
	virtual BOOL	OnRunReport			(CWoormInfo*);
	virtual BOOL	OnHKLIsValid		(HotKeyLink* pHotKeyLink);

	void OnBuildDataControlLinks(CAbstractFormView* pView);
	void OnBuildDataControlLinks (CTabDialog* pDialog);

	void OnModifyDBTDefineQuery		(DBTObject*, SqlTable*);
	void OnModifyDBTPrepareQuery	(DBTObject*, SqlTable*);
	void OnModifyHKLDefineQuery		(HotKeyLink*, SqlTable*, HotKeyLink::SelectionType = HotKeyLink::DIRECT_ACCESS);
	void OnModifyHKLPrepareQuery	(HotKeyLink*, SqlTable*, DataObj*, HotKeyLink::SelectionType  = HotKeyLink::DIRECT_ACCESS);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec, HotKeyLink* pHotKeyLink);

	virtual WebCommandType OnGetWebCommandType(UINT commandID) override;

	CManagedDocComponentObj*	GetComponent	(CString& sParentNamespace, CString& sName);
	void						GetComponents	(CManagedDocComponentObj* pRequest, ::Array& returnedComponens);

	void OnBuildingSecurityTree	(CTBTreeCtrl* pTree, ::Array* arInfoTreeItems);
public:
	void EasyBuilderIt					();

	// routing
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	
	void ProcessWebMessage(UINT nID, int nCode, BOOL isEasyBuilderAction);

protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void ChooseContext();
	afx_msg void OnEditForm					(UINT nCmd);
	afx_msg void OnEditForm					();
	afx_msg void OnUpdateEditForm			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateDropdown			(CCmdUI* PCCmdUI);

	BOOL OnToolbarDropDown					(UINT nID, CMenu& menu);
	
private:
	NameSpace^				GetServerDocumentNamespace	();
	System::Collections::Generic::IList<TbToolBoxItem^>^	GetToolBoxItems();
	System::Drawing::Bitmap^		GetBitmapFromType						(System::Type^ familyType);
	void		AddItem(System::Collections::Generic::IList<TbToolBoxItem^>^ items, System::String^ caption, System::Type^ familyType);
	void		RestoreFromEditMode							();
	void		ChooseCustomizationContextAndRunEasyStudio	();
	bool		IsEditing									();
	static BOOL	OnIdleHandler								(LONG lCount);
	int			AskForDisablingNotWorkingControllers		(System::Collections::ICollection^ notWorkingControllers);
	NameSpace^	GetNewCustomizationName						(INameSpace^ documentNamespace);
	int FireAction(const CString& funcName, CString* pstrInputOutput);
 	int FireAction(const CString& funcName, void* pVoidInputOutput);
	int FireAction(const CString& funcName);

	void InitialMessage();
	BOOL DispatchDBTEvent (DBTEvent theEvent, DBTObject* pDbt, int nRow, SqlRecord* pRecord);

	bool IsEditableController(DocumentController^ controller);
	bool IsControllerEditableInCurrentCustomizationContext(DocumentController^ controller);
	AddNewTagType GetAddNewTag();
	DocumentController^ GetActiveController();
	BusinessObject^ GetCaller();
	System::String^ DecodeEventName(int nCode, bool isEasyBuilderAction);

public:
	static	BOOL IsLicenseForEasyBuilderVerified();
	
};

#include "endh.dex"
