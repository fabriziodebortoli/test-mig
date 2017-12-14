#pragma once

#include <TbGeneric\dataObj.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\BaseApp.h>

#include "LibrariesLoader.h"

class CTaskBuilderApp;
class CXMLNodeChildsList;
#pragma warning (disable : 4996)



//==============================================================================
class CTbCommandManager : public CTbCommandInterface
{
	DECLARE_DYNAMIC(CTbCommandManager)

	CEbCommandManagerObj*		m_pEBCommands;
	CLibrariesLoader			m_LibrariesLoader;
public:
	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(INVALID_COMPANY, "The product is not currently connected to a valid company.\r\nPlease use Change Login functionality to change the connected company.")
	END_TB_STRING_MAP()

public:
	CTbCommandManager ();
	~CTbCommandManager();

public:
	virtual CEbCommandManagerObj*	GetEBCommands() { return m_pEBCommands; }
	void SetEBCommands(CEbCommandManagerObj* pCommands) { ASSERT(!m_pEBCommands); m_pEBCommands = pCommands; }
	// documents
	const CSingleExtDocTemplate*	GetDocTemplate
									(
										LPCTSTR				pszDocNamespace, 
										FailedInvokeCode&	invokeCode,
										LPCTSTR				pszViewMode		= szDefaultViewMode, 
										BOOL				bFromFunction	= FALSE,
										BOOL				bUseDiagnostic	= TRUE
									);
	virtual FailedInvokeCode	CanDoRunDocument
									(
										LPCTSTR		pszDocNamespace, 
										LPCTSTR		pszViewMode		= szDefaultViewMode, 
										BOOL		bFromFunction	= FALSE,
										BOOL		bUseDiagnostic	= TRUE
									);
	virtual CBaseDocument*		RunDocument	
									(
										LPCTSTR					pszNamespace, 
										LPCTSTR					pszViewMode		= szDefaultViewMode, 
										BOOL					bFromFunction	= FALSE,
										CBaseDocument*			pAncestor		= NULL, 
										LPAUXINFO				lpAuxInfo		= NULL, 
										CBaseDocument**			ppExistingDoc	= NULL, 
										FailedInvokeCode*		pFailedCode		= NULL,
										CExternalControllerInfo*pControllerInfo = NULL,
										BOOL					IsRunningAsADM  = FALSE,
										CTBContext*				pTBContext      = NULL,
										CManagedDocComponentObj* pManagedParameters = NULL,
										CContextBag*			 pContextBag = NULL
									);
	virtual ADMObj*				RunDocument
									(
										ADMClass*			pszADMClass, 
										LPCTSTR				pszViewMode	 = szDefaultViewMode, 
										LPCTSTR				pszNamespace = NULL,
										CBaseDocument*		pAncestor	 = NULL, 
										LPAUXINFO			lpAuxInfo	 = NULL, 
										ADMObj**			ppExistingADM= NULL, 
										FailedInvokeCode*	pFailedCode	 = NULL,
										CTBContext*			pTBContext   = NULL,
										CContextBag*		pContextBag = NULL
									);

	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	ExistDocument	(CBaseDocument* pDocument, HWND& hwndFrame);
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	ExistDocument	(CBaseDocument* pDocument);
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	ExistDocument	(ADMObj* pAdm, HWND& hwndFrame);
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	ExistDocument	(ADMObj* pAdm);
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	ReportRunning		(CWoormDoc*);
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL	EditorRunning	(CPadDoc* pPadDoc);

	virtual BOOL	DestroyDocument	(const CBaseDocument* pDocument);
	virtual BOOL	DestroyDocument	(CBaseDocument*& pDocument);
	virtual BOOL	DestroyDocument	(const ADMObj*);
	virtual BOOL	DestroyDocument	(ADMObj*&);
	
	virtual BOOL	CloseDocument	(const CBaseDocument* pDocument, BOOL bAsync = FALSE);
	virtual BOOL	CloseDocument	(CBaseDocument*& pDocument, BOOL bAsync = FALSE);

	virtual BOOL	CloseWoormReport	(CWoormDoc*);
	virtual	BOOL	CloseReportReady	(CWoormDoc*);

	virtual void	WaitDocumentEnd		(CBaseDocument*);
	virtual void	WaitReportEnd		(CWoormDoc*);
	virtual	void	WaitReportRunning	(CWoormDoc*);
	
	virtual BOOL	CanCloseDocument	(CBaseDocument* pDocument);

	virtual BOOL	CanUseNamespace(CTBNamespace tbNamespace, OSLTypeObject oslT, DWORD grant, CDiagnostic* pDiagnostic, BOOL* bProtected = NULL);

	// functions
	virtual BOOL				RunFunction				(CFunctionDescription* pFunction, FailedInvokeCode* pFailedCode = NULL);
	virtual BOOL				RunFunction				(LPCTSTR lpcszNamespace, FailedInvokeCode* pFailedCode = NULL);
	virtual BOOL				FireEvent				(const CString& sEvent, FunctionDataInterface*, FailedInvokeCode* pFailedCode = NULL);
	virtual	BOOL				ExistFunction			(const CTBNamespace& aNs);
	virtual CItemSource*		CreateItemSource		(const CTBNamespace& aNs);
	virtual CValidator*			CreateValidator			(const CTBNamespace& aNs);
	virtual CDataAdapter*		CreateDataAdapters		(const CTBNamespace& aNs);
	virtual CControlBehaviour*	CreateControlBehaviour	(const CTBNamespace& aNs);

	virtual BOOL	GetFunctionDescription(const CTBNamespace& aNs, CFunctionDescription &aFunctionDescription, BOOL bEmitDiagnostic = TRUE);
	virtual BOOL	GetFunctionDescription	(LPCTSTR lpcszNamespace, CFunctionDescription &aFunctionDescription);
	
	virtual const CFunctionDescription* GetFunctionDescription(const CTBNamespace& aNs, BOOL bEmitDiagnostic = TRUE);

	virtual TBScript* CreateTbScript (CFunctionDescription*, SymTable* pSymTable);
	virtual BOOL	  GetSchemaReportVariables(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields);

	// editor
	virtual CPadDoc*		RunEditor		(CString sName = _T(""));
	
	// reports
	virtual CWoormDoc*		RunWoormReport		
								(
									LPCTSTR strReportNamespace, 
									CBaseDocument* pCallerDoc = NULL, 
									BOOL bUseRadarFrame = FALSE,
									BOOL bRunReport = TRUE
								);

	virtual CWoormDoc*		RunWoormReport		
								(
									CWoormInfo* pInfo, 
									CBaseDocument* pCallerDoc = NULL, 
									CExternalControllerInfo*pControllerInfo = NULL, 
									BOOL bUseRadarFrame = FALSE,
									BOOL bRunReport = TRUE
								); 

	// Reference Objects
	FunctionDataInterface* GetHotlinkDescription (const CTBNamespace& aHKLNs, BOOL& bIsHKLDynamic, BOOL& bIsHKLXml);
	virtual HotKeyLinkObj* RunHotlink		(const CTBNamespace& aHKLNs, FailedInvokeCode* pFailedCode = NULL, CRuntimeClass** ppControlClass = NULL);
	virtual CString		   GetHotlinkQuery	(const CString& sNamespace, const CString& sParams, const int& nAction, const CString sFilter = _T(""), HotKeyLink* pHotlink = NULL);

	// Data File
	virtual CDataFileInfo*  GetDataFileInfo				(LPCTSTR lpcszNamespace, BOOL bAllowChanges = FALSE, BOOL bUseProductLanguage = FALSE);
	virtual BOOL	  	    SaveDataFileInfo			(CDataFileInfo* pDFI); //impr 5177: BAUZI

	// DLL on demand
	virtual BOOL AutoRegisterLibrary (const CString& sLibraryNamespace, HINSTANCE hLib);
	virtual BOOL LoadNeededLibraries (const CTBNamespace& aComponentNamespace, CTBNamespaceArray* pLibDependencies = NULL, LoadLibrariesMode aMode = LoadNeeded);
	virtual BOOL IsDLLLoading		 ();	

	//deprecated, no more supported 
	TB_OLD_METHOD virtual void		BeginDLLLoading	() { ASSERT(FALSE); }
	TB_OLD_METHOD virtual void		EndDLLLoading	() { ASSERT(FALSE); }
	
	// metodi per Login e chiusura
	virtual BOOL	Login				(const CString&	sAuthenticationToken);
	virtual DWORD	GetProcessID		();
	virtual BOOL	GetCurrentUser		(CString& strUser, CString& strCompany);
	virtual BOOL	CanCloseTB			(BOOL bWithMsgBox = FALSE);
	virtual void	CloseTB				(BOOL bWithMsgBox = FALSE);
	virtual BOOL	IsTBLocked			();
	virtual BOOL	LockTB				(const CString&	sAuthenticationToken);
	virtual BOOL	UnLockTB			(const CString&	sAuthenticationToken);
	virtual BOOL	CanChangeLogin		(BOOL bLock = FALSE);
	virtual int		ChangeLogin			(const CString&	sOldAuthenticationToken, const CString&	sNewAuthenticationToken, BOOL bUnLock = FALSE);
	virtual BOOL	TableExists			(LPCTSTR sSqlTableName, LPCTSTR sSqlColumnName = NULL);
	virtual int		DisconnectCompany	(const CString&	sAuthenticationToken);
	virtual int		ReconnectCompany	(const CString&	sAuthenticationToken);

	// Native convert
	virtual CString	NativeConvert		(const DataObj*);
	virtual CBehavioursRegistryService*	GetBehaviourService	(const CString& sEntity);

private:
	const CTBNamespace	GetDocNamespace	(ADMClass* pADMClass);
	AddOnLibrary* GetAddOnLibrary(const CTBNamespace& aNs);
		  
	BOOL PrepareParamsToCustomize		(const CTBNamespace& aNamespace, CXMLNodeChildsList* pNodes, DataObjArray& arParams);
	void CleanBadDocument				(CBaseDocument* pDocument, const CSingleExtDocTemplate* pTemplate);

	const CSingleExtDocTemplate*		GetDocTemplate			(
											const CDocumentDescription*	pDocDescri, 
											FailedInvokeCode&			invokeCode,
											LPCTSTR						pszViewMode, 
											BOOL						bFromFunction,
											BOOL						bUseDiagnostic
										);
	BOOL IsBackground					(LPCTSTR pszViewMode);
	virtual void InitOnDemandEnabled ();
public:
	virtual CWinThread*	CreateDocumentThread(bool bManagedMessagePump = true, ThreadHooksState hookingState = ThreadHooksState::DEFAULT);
};
#pragma warning (default : 4996)