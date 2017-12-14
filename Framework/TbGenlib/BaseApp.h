#pragma once

#include <afxtempl.h>

#include <TbNamesolver\ApplicationContext.h>
#include <TbNamesolver\LoginContext.h>
#include <TBNameSolver\tbresourcelocker.h>

#include <TbGeneric\vercheck.h>
#include <TbGeneric\localizableobjs.h>

#include <TbClientCore\ServerConnectionInfo.h>
#include <TbClientCore\ClientObjects.h>

#include "addonmng.h"	// AddOn Manager
#include "funproto.h"	// Woorm external function prototyping
#include "TbCommandInterface.h"
#include "InterfaceMacros.h"
#include "messages.h"
#include "basedoc.h"

#include "TBStrings.h"	//gestore delle stringhe per internazionalizzazione

#include <BCGCBPro\BCGPWorkspace.h>

// forgotted afx resource
#include "extres.hjson" //JSON AUTOMATIC UPDATE

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class CSingleExtDocTemplate;
class CBaseDocument;
class COleDbManager;

AFX_DEPRECATED("CMultiExtDocTemplate has been deprecated, you should use CSingleExtDocTemplate")
typedef CSingleExtDocTemplate CMultiExtDocTemplate; //YOU SHOULD USE CSingleExtDocTemplate


// SDI support (zero or more documents) with different menu, icon, accelerators
// MUST subclass standard template for manage Running status.
//=============================================================================
class TB_EXPORT CSingleExtDocTemplate : public CSingleDocTemplate, public IOSLObjectManager
{
	DECLARE_DYNAMIC(CSingleExtDocTemplate)

public:
	HINSTANCE				m_hDllInstance;
	CString					m_sConfig;
	DWORD					m_dwProtections;
	CString					m_sViewMode;
	DocInvocationParams*	m_pDocInvocationParams;
	UINT					m_nViewID;
	CJsonContextObj*		m_pJsonContext;
private:
	const CSingleExtDocTemplate*	m_pOriginalTemplate; 
	BOOL					m_bValid;
	
	CDocument* m_pOpeningDoc;
	CFrameWnd* m_pOpeningFrame;

	// Constructors
public:
	CSingleExtDocTemplate
	(
		UINT nIDResource, 
		CRuntimeClass* pDocClass,
		CRuntimeClass* pFrameClass, 
		CRuntimeClass* pViewClass
	);
	void LoadTemplate();
	int	GetDocCount	() const;
	CSingleExtDocTemplate (const CSingleExtDocTemplate *pTemplate, DocInvocationParams* pParams);

	virtual ~CSingleExtDocTemplate();
	
// Implementation
public:
	BOOL OwnView		(const CRuntimeClass* pViewClass) const;
	BOOL OwnDocument	(const CRuntimeClass* pViewClass, BOOL bIsDerived = FALSE) const;
	BOOL SameResourceID	(UINT nIDResource) const;
	
	BOOL IsValid		() const	{ return m_bValid; }
	void SetValid		(const BOOL& bValue) { m_bValid = bValue; }

	CRuntimeClass*	 GetFrameClass	() const { return m_pFrameClass; }
	CRuntimeClass*	 GetViewClass	() const { return m_pViewClass; }
	CRuntimeClass*	 GetDocClass	() const { return m_pDocClass; }
	UINT			 GetIDResource	() const { return m_nIDResource; }

	virtual CDocument* OpenDocumentFile (LPCTSTR lpszPathName, BOOL bMakeVisible = TRUE);
	virtual CDocument* CreateNewDocument();
	virtual CFrameWnd* CreateNewFrame(CDocument* pDoc, CFrameWnd* pOther);
	virtual void InitialUpdateFrame(CFrameWnd* pFrame, CDocument* pDoc, BOOL bMakeVisible = TRUE);
	virtual void RemoveDocument(CDocument* pDoc);
	
	const CTBNamespace&	GetNamespace()							{ return GetInfoOSL()->m_Namespace; }
	void			SetNamespace	(const CTBNamespace& ns)	{ GetInfoOSL()->m_Namespace = ns; }

	void			LoadMenuStrings () { AfxLoadMenuStrings (CMenu::FromHandle (m_hMenuInPlace), m_nIDResource); }

private:
	void			AdjustMenu();
};

typedef CMap<CString, LPCTSTR, CSingleExtDocTemplate*, CSingleExtDocTemplate*> TemplateMap;
//=============================================================================
class TB_EXPORT CBaseApp : public CBCGPWinApp, public CTBLockable
{   
	DECLARE_DYNAMIC(CBaseApp)
protected:		
	DECLARE_LOCKABLE(TemplateMap,  m_TemplateCache);

public:	
	CBaseApp();
	~CBaseApp();

public:
	virtual LPCSTR  GetObjectName() const { return "CBaseApp"; }

	BOOL CanCloseApplication	(BOOL bWithMsgBox = FALSE);
	BOOL CanUseReportEditor		(); 

	// gestione AddOnApplications
	AddOnApplication*		GetTaskBuilderAddOnApp	()	const;
	AddOnApplication*		GetExtensionsAddOnApp	()	const;
	AddOnApplication*		GetMasterAddOnApp		()	const;

	LPCTSTR	GetSignature	() const	{ return GetMasterAddOnApp() ? GetMasterAddOnApp()->GetSignature() : _T("TBA"); }
	CString	GetAppVersion	() const	{ return GetMasterAddOnApp() ? GetMasterAddOnApp()->GetAppVersion() : GetTBVersion(); }

	BOOL	IsDevelopment	()	const;
	
	BOOL OtherDocumentPresent	(BOOL bDoDiagnostic = FALSE) const;

	// Usually application related functions
	int			GetNrOpenDocuments		() const;

public:
	CString GetAppTitle		() const;
	
	// da vedere di fare meglio in pageinfo.
	//HGLOBAL	GetDevMode	() { return m_hDevMode; }
	
public:
	BOOL	IsNoCancOnWarning()	const;
	
	BOOL	IsInUnattendedMode() const;

	void	InvalidateTemplates	();
	void	ValidateTemplates	(const CTBNamespace& aModuleNs);

	const CSingleExtDocTemplate* GetDocTemplate(UINT nIDResource) const;
	const CSingleExtDocTemplate* GetDocTemplate(const CRuntimeClass* pViewClass, UINT nViewId, UINT nFrameId, const CRuntimeClass* pDocClass = NULL, BOOL bIsDerived = FALSE, BOOL bCheckMasterFrame = FALSE) const;
	const CSingleExtDocTemplate* GetDocTemplate(const CString& sDocumentName, const CString& sViewMode);
	const CSingleExtDocTemplate* GetDocTemplate(const CTBNamespace&);
	
	// count how many template have same resource ID
	int TemplateNo(UINT nIDResource) const;
    
	//----- Safe Register Template (evita registrazioni doppie)
	CSingleExtDocTemplate*  RegisterTemplate
		(
			UINT					nIDResource,
			CRuntimeClass*			pDocClass,
			CRuntimeClass*			pFrameClass,
			CRuntimeClass*			pViewClass,
			UINT					nViewId,
			UINT					nFrameId,
			CTBNamespace&			documentNamespace,
			AddOnInterfaceObj*		pParent = NULL,
			CString					sConfig = _T(""),
			DWORD					dwProtections = TPL_NO_PROTECTION,
			CString					sViewMode = szDefaultViewMode
		);	
    
	// vedi problema ClientDoc che aggiungono un template ad un'ADM interattivo e la dll
	// del clientdoc registra prima il template
	virtual BOOL IsMasterFrame(CRuntimeClass*) { return FALSE; }

    // Calcola se ci sono sufficienti risorse per lanciare l'applicativo
	BOOL ResourceAvailable		();

public:
	// primary status bar set text function
	TB_OLD_METHOD void	SetStatusBarText	(const CString& strText)	{ return AfxSetStatusBarText(strText); }
	TB_OLD_METHOD void 	SetStatusBarText	(UINT nID)					{ return AfxSetStatusBarText(nID); }
	TB_OLD_METHOD void 	ClearStatusBar		()							{ return AfxClearStatusBar(); }

public:
	// general application error code
	void			ClearError		() { SetError(_T("")); }
	void			ClearWarning	() { SetWarning(_T("")); }
	void			ClearMessages	() { ClearError(); ClearWarning(); }
	void			SetError		(const CString& strMess);
	void			SetWarning		(const CString& strMess, BOOL bNoCanc = FALSE);
	const CString&	GetError		()	const;	
	const CString&	GetWarning		()	const;
	BOOL			ErrorFound		()	const	{ return !GetError().IsEmpty(); }	
	BOOL			WarningFound	()	const	{ return !GetWarning().IsEmpty(); }	
	BOOL			MessageFound	()	const	{ return ErrorFound() || WarningFound(); }	
	
	const DataObj&	GetChangingCtrlData() const;
	const DataObj&	GetOldCtrlData	() const;
	BOOL			SetOldCtrlData	(DataObj* pData, DataObj* pOldData);
	BOOL			IsValidOldCtrlData	();
	BOOL			IsCtrlDataChanged();
	BOOL			DoCloseAll		();

public:
	virtual BOOL	InitInstance();
	virtual int		ExitInstance();
	virtual void AttachClientDocs		(CAbstractFormDoc*, CClientDocArray*);
	virtual void AttachFamilyClientDocs	(CAbstractFormDoc*, CClientDocArray*);
	virtual int DoMessageBox(LPCTSTR lpszPrompt, UINT nType, UINT nIDPrompt);
	virtual BOOL OnIdle(LONG lCount);
	virtual CDocument* OpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible = TRUE);
	
protected:
	virtual BOOL StoreWindowPlacement (const CRect& rectNormalPosition, int nFflags, int nShowCmd) { return TRUE; }
	virtual BOOL LoadWindowPlacement ( CRect& rectNormalPosition, int& nFflags, int& nShowCmd) { return FALSE;}
	virtual BOOL ReloadWindowPlacement (CFrameWnd* pFrame) { return FALSE; }

protected:
	// Implementation
	//{{AFX_MSG(CBaseApp)
	afx_msg void OnCloseAll					();
	afx_msg void OnUpdatePrintSetup			(CCmdUI* pCmdUI);

	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};



//============================================================================
// Global Windows state data helper functions (inlines)
//=============================================================================
TB_EXPORT CBaseApp*				AFXAPI AfxGetBaseApp		();

//-----------------------------------------------------------------------------
TB_EXPORT const CSingleExtDocTemplate* AFXAPI AfxGetTemplate	(const CRuntimeClass* pViewClass, UINT nViewId, const CRuntimeClass* pDocClass = NULL);
TB_EXPORT const CSingleExtDocTemplate* AFXAPI AfxGetTemplate	(const CTBNamespace&);
TB_EXPORT BOOL					AfxIsCustomObject				(const CTBNamespace*);

//-----------------------------------------------------------------------------
const TB_EXPORT AddOnAppsArray*	AFXAPI AfxGetAddOnAppsTable			();
TB_EXPORT AddOnApplication*		AFXAPI AfxGetAddOnApp				(const CString& strAppName);
TB_EXPORT AddOnApplication*		AFXAPI AfxGetAddOnAppFromSignature	(LPCTSTR pszSignature);
TB_EXPORT AddOnModule*			AFXAPI AfxGetAddOnModule			(const CTBNamespace& aNamespace);
TB_EXPORT AddOnLibrary*			AFXAPI AfxGetAddOnLibrary			(const CTBNamespace& aNamespace);
TB_EXPORT AddOnDll*				AFXAPI AfxGetAddOnDll				(const CTBNamespace& aDllNamespace);
TB_EXPORT AddOnDll*				AFXAPI AfxGetAddOnDll				(HINSTANCE hDllInstance);
TB_EXPORT void					AFXAPI AfxGetAddOnModules			(HINSTANCE hDllInstance, AddOnModsArray &arModules);
TB_EXPORT AddOnModule*			AFXAPI AfxGetAddOnModule			(CString strFilePath);
TB_EXPORT AddOnModule*			AFXAPI AfxGetOwnerAddOnModule		(CRuntimeClass* pObjectClass);
TB_EXPORT AddOnLibrary*			AFXAPI AfxGetOwnerAddOnLibrary		(const CTBNamespace*);
TB_EXPORT AddOnLibrary*			AFXAPI AfxGetOwnerAddOnLibrary		(const CTBNamespace::NSObjectType aType, const CString& sRuntimeClassName);

//-----------------------------------------------------------------------------
TB_EXPORT void	AFXAPI AfxGetClientDocs	(const CTBNamespace& aServerNs, CObArray& arClientDocs);

TB_EXPORT const CDbObjectDescription*	AFXAPI AfxGetDbObjectDescription	(const CString& sName);

//-----------------------------------------------------------------------------
TB_EXPORT BOOL		AFXAPI AfxIsAppActivated		(const CString& sApplication);

//-----------------------------------------------------------------------------
TB_EXPORT CString	AFXAPI AfxGetXEngineSiteCode	();

//-----------------------------------------------------------------------------
TB_EXPORT BOOL			AFXAPI AfxIsCurrentlyInUnattendedMode(); 
TB_EXPORT BOOL			AFXAPI AfxThreadLockTraceEnabled();
TB_EXPORT BOOL			AFXAPI AfxAPIHookingEnabled();
TB_EXPORT BOOL			AFXAPI AfxCenterControlsEnabled();
TB_EXPORT void			AFXAPI AfxSetCenterControlsEnabled(BOOL bEnabled);
TB_EXPORT void			ChangeOperationsDate();
TB_EXPORT CDocument*	AfxOpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible = TRUE);


#include "endh.dex"
