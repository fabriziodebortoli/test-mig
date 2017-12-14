#include "stdafx.h"

#include <afxpriv.h>
#include <locale.h>

#include <TbNameSolver\chars.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include <TbGeneric\ParametersSections.h>

#include <TbGeneric/CMapi.h>

#include <mapi.h>

#include <mapix\mapidefs.h>
#include <mapix\mapicode.h>
#include <mapix\mapiguid.h>
#include <mapix\mapitags.h>
#include <mapix\MAPIX.h>
#include <mapix\mapiutil.h>


//=======================================================================

//The class which encapsulates the MAPI connection
class TB_EXPORT CMapiSession : public IMapiSession
{
public:
	//Constructors / Destructors
	CMapiSession(BOOL bMultiThreaded = FALSE, BOOL bNoLogonUI = FALSE, BOOL bNoInitializeMAPI = FALSE,
		CDiagnostic* pDiagnostic = NULL);
	~CMapiSession();

	//Logon / Logoff Methods
	BOOL Logon(const CString& sProfileName = CString(USE_DEFAULT_MAPI_PROFILE), const CString& sPassword = CString(), CWnd* pParentWnd = NULL);
	BOOL LoggedOn() const;
	BOOL Logoff();

	//Send a message
	BOOL Send(CMapiMessage& msg);

	//show address book
	BOOL ShowAddressBook(const CStringArray &arOldNames, CStringArray &arNewNames, HWND parent = 0);

	//General MAPI support
	BOOL MapiInstalled() const;

	//Error Handling
	ULONG	  GetLastError() const;
	CString GetLastErrorMessage();	// by A.R.

protected:
	//Methods
	void Initialize(BOOL bMultiThreaded, BOOL bNoInitializeMAPI);
	void Uninitialize();
	BOOL MAPIInitialize(BOOL bMultiThreaded);
	void MAPIUninitialize();

	void SaveRegionalSettings();
	void RestoreRegionalSettings();

	BOOL Resolve(const CString& sName, lpMapiRecipDesc* lppRecip);
	BOOL IsFaxAddress(const CString &sAddress);

	//Data
	LHANDLE			m_hSession;		//Mapi Session handle
	ULONG				m_nLastError;	//Last Mapi error value
	BOOL				m_bMAPIInitialized;	//has MAIPInitialize already been called?
	BOOL				m_bNoLogonUI;
	CString			m_strLocal;			//regional settings

	CDiagnostic* m_pDiagnostic;

	//Instance handle of the MAPI dll
	HINSTANCE				m_hMapi;
	//MAPI dll entry point as function pointer
	LPMAPIINITIALIZE		m_lpfnMAPIInitialize;
	LPMAPIUNINITIALIZE	m_lpfnMAPIUninitialize;
	LPMAPILOGON			m_lpfnMAPILogon;
	LPMAPILOGOFF			m_lpfnMAPILogoff;
	LPMAPISENDMAIL		m_lpfnMAPISendMail;
	LPMAPIRESOLVENAME		m_lpfnMAPIResolveName;
	LPMAPIFREEBUFFER		m_lpfnMAPIFreeBuffer;
	LPMAPIADDRESS			m_lpfnMAPIAddress;

	LPMAPIDETAILS			m_lpfnMAPIDetails;
	LPMAPIFINDNEXT		m_lpfnMAPIFindNext;
	LPMAPIREADMAIL		m_lpfnMAPIReadMail;
	LPMAPISENDDOCUMENTS	m_lpfnMAPISendDocuments;
	LPMAPISAVEMAIL		m_lpfnMAPISaveMail;
	LPMAPIDELETEMAIL		m_lpfnMAPIDeleteMail;

	LPALLOCATEMORE		m_lpfnMAPIAllocateMore;
	LPALLOCATEBUFFER		m_lpfnMAPIAllocateBuffer;
	LPFREEBUFFER			m_lpfnUlRelease;
	LPFREEBUFFER			m_lpfnFreeProws;

	LPFREEBUFFER			m_lpfnFreePadrlist;
	LPMAPILOGONEX			m_lpfnMAPILogonEx;

	HRESULT CopySBinary(LPSBinary psbDest,
		const LPSBinary psbSrc,
		LPVOID pParent);

	HRESULT CMapiSession::ManualResolve(
		CString& sName,
		LPMESSAGE lpMessage = NULL
		);

	BOOL ShowAddressBookEx(CStringArray &arNewNames);
	int ShowAddressBookEx(IMAPISession* pSession, LPADRLIST& pAddressList, LPCTSTR szCaption);
	BOOL GetEmail(IMAPISession* pSession, ADRENTRY& adrEntry, CString& strEmail);
	BOOL GetExEmail(IMAPISession* pSession, SBinary entryID, CString& strEmail);

};

//=============================================================================


//includere come ultimo include all'inizio del cpp
//#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////////

//HINSTANCE			CMapiSession::m_hMapi = NULL;		//Instance handle of the MAPI dll
//BOOL				CMapiSession::m_bMAPIInitialized = FALSE;	//has MAIPInitialize already been called?
//BOOL				CMapiSession::m_bMAPIInstalled = FALSE;	//has MAIPInitialize already been called?
//
//LPMAPILOGON			CMapiSession::m_lpfnMAPILogon = NULL;	
//LPMAPILOGOFF		CMapiSession::m_lpfnMAPILogoff = NULL;	
//LPMAPISENDMAIL		CMapiSession::m_lpfnMAPISendMail = NULL; 
//LPMAPIRESOLVENAME	CMapiSession::m_lpfnMAPIResolveName = NULL; 
//LPMAPIFREEBUFFER	CMapiSession::m_lpfnMAPIFreeBuffer = NULL; 
//LPMAPIADDRESS		CMapiSession::m_lpfnMAPIAddress = NULL;
//LPMAPIDETAILS		CMapiSession::m_lpfnMAPIDetails = NULL;		
//LPMAPIFINDNEXT		CMapiSession::m_lpfnMAPIFindNext = NULL;		
//LPMAPIREADMAIL		CMapiSession::m_lpfnMAPIReadMail = NULL;		
//LPMAPISENDDOCUMENTS CMapiSession::m_lpfnMAPISendDocuments = NULL;
//LPMAPISAVEMAIL		CMapiSession::m_lpfnMAPISaveMail = NULL;

//http://support.microsoft.com/kb/266351/en-us/

//Forward definition of helper function.

HRESULT CMapiSession::ManualResolve
(
  CString& sName,
  LPMESSAGE lpMessage/* = NULL*/ 
)
{
	if (m_lpfnMAPILogonEx == NULL)
		return E_FAIL;

   HRESULT         hRes = S_OK;
   IMAPISession* pSession = NULL;

	DWORD dwFlags = MAPI_EXTENDED | MAPI_USE_DEFAULT | MAPI_NEW_SESSION | MAPI_NO_MAIL;
	hRes = m_lpfnMAPILogonEx(NULL, NULL, NULL, dwFlags, &pSession);
	
	if(hRes!=S_OK)
		return E_FAIL;

   ULONG           ulObjType = 0;
   LPSPropTagArray pPropTag = NULL;
   
   LPMAPICONTAINER pAddrRoot = NULL;
   LPMAPITABLE     pBooks = NULL;
   ULONG           ulCount = 0;
   LPSRowSet       pABCRows = NULL;
   LPSRowSet       pRows = NULL;
   LPADRLIST       pAdrList = NULL;
   LPABCONT        pABC = NULL;
   LPADRBOOK       pAddrBook = NULL;
   LPMAPITABLE     pTable = NULL;
   
   enum {
		abcPR_ENTRYID,
		abcPR_DISPLAY_NAME, 
		abcNUM_COLS
   };
   
   static SizedSPropTagArray(abcNUM_COLS, abcCols) =
   {
	  abcNUM_COLS,
	  PR_ENTRYID,
	  PR_DISPLAY_NAME, 
   };   
   
   enum {
	  abPR_ENTRYID,
	  abPR_DISPLAY_NAME,
	  abPR_ADDRTYPE, 
	  abPR_DISPLAY_TYPE,
	  abPR_EMAIL_ADDRESS,
	  abPR_SMTP_ADDRESS, 
	  abPR_RECIPIENT_TYPE, 
	  abNUM_COLS
   };
   
   static SizedSPropTagArray(abNUM_COLS, abCols) = 
   {
	  abNUM_COLS,

	  PR_ENTRYID,
	  PR_DISPLAY_NAME,
	  PR_ADDRTYPE, 
	  PR_DISPLAY_TYPE,
	  PR_EMAIL_ADDRESS,
	  PROP_TAG(PT_TSTRING, 0x39FE),
	  PR_RECIPIENT_TYPE
   };   
   
   hRes = pSession->OpenAddressBook(
	  NULL,
	  NULL,
	  NULL,
	  &pAddrBook);
   if (FAILED(hRes) || !pAddrBook) 
	   goto Cleanup;
   
   // Open root address book (container).
   hRes = pAddrBook->OpenEntry(
	  0L,
	  NULL,
	  NULL,
	  0L,
	  &ulObjType,
	  (LPUNKNOWN*)&pAddrRoot
	  );
   if (FAILED(hRes) || !pAddrRoot) 
	   goto Cleanup;
   
   // Get a table of all of the Address Books.
   hRes = pAddrRoot->GetHierarchyTable(0, &pBooks);
   if (FAILED(hRes) || !pBooks) 
	   goto Cleanup;
   
   // Restrict the table to the properties that we are interested in.
   hRes = pBooks->SetColumns((LPSPropTagArray)&abcCols, 0);
   if (FAILED(hRes)) 
	   goto Cleanup;
   
   // Get the total number of rows returned. Typically, this will be 1.
   hRes = pBooks->GetRowCount(0, &ulCount);
   if (FAILED(hRes) || !ulCount) 
	   goto Cleanup;
   
   for (;;)
   {
	  hRes = pBooks->QueryRows(1,NULL,&pABCRows);
	  if (FAILED(hRes)) 
		  goto Cleanup;
	  if (!pABCRows || !pABCRows->cRows || !pABCRows->aRow || !pABCRows->aRow->lpProps)
		 goto Cleanup;
	  
	  if (PR_ENTRYID == pABCRows->aRow->lpProps[abcPR_ENTRYID].ulPropTag)
	  {
		 hRes = pAddrRoot->OpenEntry(
			pABCRows->aRow->lpProps[abcPR_ENTRYID].Value.bin.cb,
			(ENTRYID*)pABCRows->aRow->lpProps[abcPR_ENTRYID].Value.bin.lpb,
			NULL,
			0L,
			&ulObjType,
			(LPUNKNOWN*)&pABC);
		 if (FAILED(hRes) || !pABC) 
			 goto Cleanup;
		 
		 if (ulObjType == MAPI_ABCONT)
		 {
			hRes = pABC->GetContentsTable(0, &pTable);
			if (FAILED(hRes) || !pTable) 
				goto Cleanup;
			
			hRes = pTable->SetColumns((LPSPropTagArray)&abCols, 0);
			if (FAILED(hRes))
				goto Cleanup;

			hRes = pTable->SeekRow(
			   BOOKMARK_BEGINNING,
			   0,
			   NULL);
			if (FAILED(hRes)) 
				goto Cleanup;
			
			//Set a restriction so that we only find close matches.
			SRestriction    sres;
			SPropValue      spvResType;
			
			spvResType.ulPropTag = PR_ANR_W; //TODO UNICODE: PR_ANR;
			spvResType.Value.lpszW = (LPTSTR)(LPCTSTR) sName;//TODO UNICODE lpszA 
			
			sres.rt = RES_PROPERTY;
			sres.res.resProperty.relop = RELOP_EQ;
			sres.res.resProperty.ulPropTag = PR_ANR_W;	//TODO UNICODE: PR_ANR
			sres.res.resProperty.lpProp = &spvResType;
			
			hRes = pTable->Restrict(&sres,NULL);
			if (FAILED(hRes)) 
				break;
			
			//End FindRow code.
			for (;;)
			{
			   hRes = pTable->QueryRows(1, NULL, &pRows);
			   if (FAILED(hRes)) 
				   goto Cleanup;
			   if (!pRows || !pRows->cRows || !pRows->aRow || !pRows->aRow->lpProps)
				  goto Cleanup;
			   
			   if (abPR_DISPLAY_NAME == pRows->aRow->lpProps[abPR_DISPLAY_NAME].ulPropTag)
			   {
				  //TODO: Add any additional testing here.
				
				  if (_tcsicmp((LPTSTR)(LPCTSTR)sName, pRows->aRow->lpProps[abPR_DISPLAY_NAME].Value.lpszW) == 0)//TODO UNICODE: lpszA
				  { 
// Allocate memory for new Address List structure.
					 hRes = m_lpfnMAPIAllocateBuffer(
							  CbNewADRLIST(1), 
							  (LPVOID*)&pAdrList);
					 if (FAILED(hRes) || !pAdrList) 
						 goto Cleanup;
					 
					 ZeroMemory(pAdrList, CbNewADRLIST(1));
					 pAdrList->cEntries = 1;

// Allocate memory for SPropValue structure that indicates what
// recipient properties will be set. To resolve a name that
// already exists in the Address book, this will always be 1.
					 hRes = m_lpfnMAPIAllocateBuffer(
						abNUM_COLS * sizeof(SPropValue),
						(LPVOID*)&pAdrList->aEntries->rgPropVals);
					 if (FAILED(hRes)) 
						 goto Cleanup;
					 
//TODO: We are setting 5 properties below. 
//If this changes, modify these two lines.
					 ::ZeroMemory(pAdrList->aEntries->rgPropVals, 
					   5 * sizeof(SPropValue));
					 pAdrList->aEntries->cValues = 5;
					 
// Fill out addresslist with required property values.
					 LPSPropValue pProps = pAdrList->aEntries->rgPropVals;
					 LPSPropValue pProp;
					 
					 pProp = &pProps[abPR_ENTRYID];
					 pProp->ulPropTag = PR_ENTRYID;
					 
					 CopySBinary(&pProp->Value.bin,
						&pRows->aRow->lpProps[abPR_ENTRYID].Value.bin,
						pAdrList);
	   
					 pProp = &pProps[abPR_RECIPIENT_TYPE];
					 pProp->ulPropTag = PR_RECIPIENT_TYPE;
					 pProp->Value.l = MAPI_TO;
					 
					 pProp = &pProps[abPR_DISPLAY_NAME];
					 pProp->ulPropTag = PR_DISPLAY_NAME;
					 hRes = m_lpfnMAPIAllocateMore(
						2 + _tcslen(pRows->aRow->lpProps[abPR_DISPLAY_NAME].Value.lpszW),
						pAdrList,
						(LPVOID*)&pProp->Value.lpszW);
					 if (FAILED(hRes)) 
						 goto Cleanup;

					 _tcscpy_s(pProp->Value.lpszW, _tcslen(pRows->aRow->lpProps[abPR_DISPLAY_NAME].Value.lpszW),
					   pRows->aRow->lpProps[abPR_DISPLAY_NAME].Value.lpszW); //TODO UNICODE: lpszA
					 
					 pProp = &pProps[abPR_ADDRTYPE];
					 pProp->ulPropTag = PR_ADDRTYPE;
					 hRes = m_lpfnMAPIAllocateMore(
							  2 + _tcslen(pRows->aRow->lpProps[abPR_ADDRTYPE].Value.lpszW),
							  pAdrList,
							  (LPVOID*)&pProp->Value.lpszW);
					 if (FAILED(hRes)) 
						 goto Cleanup;

					 _tcscpy_s(pProp->Value.lpszW,  _tcslen(pRows->aRow->lpProps[abPR_ADDRTYPE].Value.lpszW),
					   pRows->aRow->lpProps[abPR_ADDRTYPE].Value.lpszW);	//TODO UNICODE: lpszA
					 
					 pProp = &pProps[abPR_DISPLAY_TYPE];
					 pProp->ulPropTag = PR_DISPLAY_TYPE;
					 pProp->Value.l = pRows->aRow->lpProps[abPR_DISPLAY_TYPE].Value.l;
					 
					 if (!lpMessage)
						 goto Cleanup;

					 hRes = lpMessage->ModifyRecipients(
						MODRECIP_ADD,
						pAdrList);
					 if (FAILED(hRes)) 
						 goto Cleanup;
					 
					 if (pAdrList) m_lpfnFreePadrlist(pAdrList);
					 pAdrList = NULL;
					 
					 hRes = lpMessage->SaveChanges(KEEP_OPEN_READWRITE);
					 if (FAILED(hRes)) 
						 goto Cleanup;
					 
//Because we are done with our work, we will exit. We have all
//of the cleanup duplicated below so that we don't leak memory.
					 goto Cleanup;
				  }
			   }
			   if (pRows) m_lpfnFreeProws(pRows);
			   pRows = NULL;
			}
			if (pTable) pTable->Release();
			pTable = NULL;
		 }
	  }
	  if (pABCRows) m_lpfnFreeProws(pABCRows);
	  pABCRows = NULL;
   }
   
Cleanup:
   if (pAdrList) 
	   m_lpfnFreePadrlist(pAdrList);
   m_lpfnUlRelease(pTable);
   if (pRows) 
	   m_lpfnFreeProws(pRows);
   if (pABCRows) 
	   m_lpfnFreeProws(pABCRows);
   m_lpfnUlRelease(pABC);
   m_lpfnUlRelease(pBooks);
   m_lpfnUlRelease(pAddrRoot);
   m_lpfnUlRelease(pAddrBook);
   m_lpfnMAPIFreeBuffer(pPropTag);
   return hRes;
}//ManualResolve.

//-----------------------------------------------------------------------------
HRESULT CMapiSession::CopySBinary
(LPSBinary psbDest,
   const LPSBinary psbSrc,
   LPVOID pParent)
{
   HRESULT     hRes = S_OK;
   psbDest -> cb = psbSrc -> cb;
   if (psbSrc -> cb)
   {
	  if (pParent)
		 hRes = m_lpfnMAPIAllocateMore(psbSrc->cb, 
				  pParent, 
				  (LPVOID *) &psbDest->lpb);
	  else
		 hRes = m_lpfnMAPIAllocateBuffer(psbSrc->cb, (LPVOID *) &psbDest->lpb);

	  if (!FAILED(hRes))
		  ::CopyMemory(psbDest->lpb, psbSrc->lpb, psbSrc->cb);
   }	
   return hRes;
}

//////////////////////////////////////////////////////////////////////////////////

CMapiSession::CMapiSession
	(
		BOOL bMultiThreaded /*= FALSE*/, 
		BOOL bNoLogonUI  /*= FALSE*/, 
		BOOL bNoInitializeMAPI /*= FALSE*/,
		CDiagnostic* pDiagnostic /*= NULL*/
	)
	:
	m_pDiagnostic(pDiagnostic)
{
	m_nLastError = 0;

	m_hSession = 0;
	m_hMapi = NULL;

	m_lpfnMAPIInitialize = NULL;
	m_lpfnMAPIUninitialize = NULL;
	m_lpfnMAPILogon = NULL;
	m_lpfnMAPILogoff = NULL;
	m_lpfnMAPISendMail = NULL;
	m_lpfnMAPIResolveName = NULL;
	m_lpfnMAPIFreeBuffer = NULL;
	m_lpfnMAPIAddress = NULL;

	m_lpfnMAPIAllocateMore = NULL;
	m_lpfnMAPIAllocateBuffer= NULL ;
	m_lpfnUlRelease			= NULL ;
	m_lpfnFreeProws			= NULL ;
	m_lpfnFreePadrlist		= NULL ;

	m_bMAPIInitialized = FALSE;

	m_bNoLogonUI = bNoLogonUI;

	SaveRegionalSettings();

	Initialize(bMultiThreaded, bNoInitializeMAPI);
}

//-----------------------------------------------------------------------------
CMapiSession::~CMapiSession()
{
  //Logoff if logged on
  Logoff();

  //Unload the MAPI dll
  Uninitialize();

  RestoreRegionalSettings();
}

//-----------------------------------------------------------------------------
void CMapiSession::Initialize(BOOL bMultiThreaded, BOOL bNoInitializeMAPI)
{
  //First make sure the "WIN.INI" entry for MAPI is present aswell 
  //as the MAPI32 dll being present on the system
	//BOOL bMapiInstalled = (GetProfileInt(_T("MAIL"), _T("MAPI"), 0) != 0) && 
 //                       (SearchPath(NULL, _T("MAPI32.DLL"), NULL, 0, NULL, NULL) != 0);
  //if (bMapiInstalled)
  //{
	//Load up the MAPI dll and get the function pointers we are interested in
	m_hMapi = LoadLibrary(_T("MAPI32.DLL"));
	if (m_hMapi)
	{
		m_lpfnMAPIInitialize	= (LPMAPIINITIALIZE)		GetProcAddress(m_hMapi, "MAPIInitialize");
		m_lpfnMAPIUninitialize  = (LPMAPIUNINITIALIZE)		GetProcAddress(m_hMapi, "MAPIUninitialize");
		m_lpfnMAPILogon			= (LPMAPILOGON)				GetProcAddress(m_hMapi, "MAPILogon");
		m_lpfnMAPILogoff		= (LPMAPILOGOFF)			GetProcAddress(m_hMapi, "MAPILogoff");
		m_lpfnMAPISendMail		= (LPMAPISENDMAIL)			GetProcAddress(m_hMapi, "MAPISendMail");
		m_lpfnMAPIResolveName	= (LPMAPIRESOLVENAME)		GetProcAddress(m_hMapi, "MAPIResolveName");
		m_lpfnMAPIFreeBuffer	= (LPMAPIFREEBUFFER)		GetProcAddress(m_hMapi, "MAPIFreeBuffer");
		m_lpfnMAPIAddress		= (LPMAPIADDRESS)			GetProcAddress(m_hMapi, "MAPIAddress");

	//TODO per ora non sono utilizzate
		//m_lpfnMAPISendDocuments	= (LPMAPISENDDOCUMENTS)		GetProcAddress (m_hMapi, "MAPISendDocuments");
		//m_lpfnMAPIFindNext		= (LPMAPIFINDNEXT)			GetProcAddress (m_hMapi, "MAPIFindNext");
		//m_lpfnMAPIReadMail		= (LPMAPIREADMAIL)			GetProcAddress (m_hMapi, "MAPIReadMail");
		//m_lpfnMAPIDetails		= (LPMAPIDETAILS)			GetProcAddress (m_hMapi, "MAPIDetails");
		//m_lpfnMAPISaveMail		= (LPMAPISAVEMAIL)			GetProcAddress (m_hMapi, "MAPISaveMail");
		//m_lpfnMAPIDeleteMail	= (LPMAPIDELETEMAIL)		GetProcAddress (m_hMapi, "MAPIDeleteMail");

		//m_lpfnMAPIAllocateMore	= (LPALLOCATEMORE)			GetProcAddress (m_hMapi, "MAPIAllocateMore");
		//m_lpfnMAPIAllocateBuffer= (LPALLOCATEBUFFER)		GetProcAddress (m_hMapi, "MAPIAllocateBuffer");

		//m_lpfnUlRelease			= (LPFREEBUFFER)			GetProcAddress (m_hMapi, "UlRelease@4");
		//m_lpfnFreeProws			= (LPFREEBUFFER)			GetProcAddress (m_hMapi, "FreeProws@4");
		m_lpfnFreePadrlist		= (LPFREEBUFFER)			GetProcAddress (m_hMapi, "FreePadrlist@4");
		
		m_lpfnMAPILogonEx		= (LPMAPILOGONEX)			GetProcAddress (m_hMapi, "MAPILogonEx");


	  //If any of the functions are not installed then fail the load
	  //Load fails if MAPI intialization fails, too
	  if (
		  m_lpfnMAPIInitialize == NULL ||
		  m_lpfnMAPIUninitialize == NULL ||
		  m_lpfnMAPILogon == NULL ||
		  m_lpfnMAPILogoff == NULL ||
		  m_lpfnMAPISendMail == NULL ||
		  m_lpfnMAPIResolveName == NULL ||
		  m_lpfnMAPIFreeBuffer == NULL ||
		  m_lpfnMAPIAddress == NULL 
		  )
	  {
		  m_nLastError = ::GetLastError();
		TRACE(_T("Failed to get one of the functions pointer in MAPI32.DLL\n"));
		Uninitialize();
	  }
	  if (
		  !bNoInitializeMAPI && !MAPIInitialize(bMultiThreaded)
		  )
	  {
		  //m_nLastError = ::GetLastError();
		TRACE(_T("Failed to get one of the functions pointer in MAPI32.DLL\n"));
		Uninitialize();
	  }
	}
  //}
  else
  {
	  m_nLastError = ::GetLastError();
	TRACE(_T("Mapi is not installed on this computer\n"));
	}

  RestoreRegionalSettings();
}

//-----------------------------------------------------------------------------
void CMapiSession::Uninitialize()
{
  if (m_hMapi)
  {
	MAPIUninitialize ();
	  //Unload the MAPI dll and reset the function pointers to NULL
	FreeLibrary(m_hMapi);
	m_hMapi = NULL;

	m_lpfnMAPIInitialize = NULL;
	m_lpfnMAPIUninitialize = NULL;
	m_lpfnMAPILogon = NULL;
	m_lpfnMAPILogoff = NULL;
	m_lpfnMAPISendMail = NULL;
	m_lpfnMAPIResolveName = NULL;
	m_lpfnMAPIFreeBuffer = NULL;
	m_lpfnMAPIAddress = NULL ;

	m_lpfnMAPIAllocateMore	= NULL ;
	m_lpfnMAPIAllocateBuffer= NULL ;
	m_lpfnUlRelease			= NULL ;
	m_lpfnFreeProws			= NULL ;
	m_lpfnFreePadrlist		= NULL ;
	m_lpfnMAPILogonEx		= NULL ;
	
  }
}

//-----------------------------------------------------------------------------
void CMapiSession::SaveRegionalSettings ()
{
	m_strLocal = ::setlocale (LC_ALL, NULL);
}

//-----------------------------------------------------------------------------
void CMapiSession::RestoreRegionalSettings() 
{
  CString strLocal2 = ::_tsetlocale (LC_ALL, NULL); //se i regional settings sono cambiati, li ripristino
	if (m_strLocal != strLocal2)
		::_tsetlocale (LC_ALL, m_strLocal);	
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::MAPIInitialize(BOOL bMultiThreaded)
{
	HRESULT				hr = MAPI_E_FAILURE;
	MAPIINIT_0			MAPIIinit = { 0, MAPI_MULTITHREAD_NOTIFICATIONS};

	if (FAILED(hr = (*m_lpfnMAPIInitialize)(bMultiThreaded ? &MAPIIinit : NULL)))
	{
		m_bMAPIInitialized = FALSE;
		m_nLastError = hr;

		RestoreRegionalSettings();

		return TRUE; //3.2.1 su certi sistemi (es. Win 2800 senza Outlook) anche se fallisce poi le mail partirebbero ugualmente //=FALSE;
	}

	m_bMAPIInitialized = TRUE;

	RestoreRegionalSettings();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CMapiSession::MAPIUninitialize()
{
  if (m_hMapi && m_bMAPIInitialized)
  {
	(*m_lpfnMAPIUninitialize)();

	m_bMAPIInitialized = FALSE;
	
	RestoreRegionalSettings();
  }
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::Logon(const CString& sProfileName, const CString& sPassword, CWnd* pParentWnd)
{
	//For correct operation of the T2A macro, see MFC Tech Note 59
	USES_CONVERSION;

	if	(
			!MapiInstalled() || //MAPI must be installed
			!m_lpfnMAPILogon	//Function pointer must be valid
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}
  
	//Initialise the function return value
  BOOL bSuccess = FALSE;

  //Just in case we are already logged in
  Logoff();

  // by A.R.: se il profile name è il default, tenta il login con il dafault MAPI profile
  // senza aprire la dialog anche se main window presente
  if (m_bNoLogonUI || sProfileName == USE_DEFAULT_MAPI_PROFILE)
  {
	ULONG nUIParam = 0;
	ULONG nError = m_lpfnMAPILogon(nUIParam, NULL, NULL, 0, 0, &m_hSession);
	if (nError == SUCCESS_SUCCESS)
	{
	  m_nLastError = SUCCESS_SUCCESS;
	  bSuccess = TRUE;
	}
	RestoreRegionalSettings();
	return bSuccess;
  }

  //Setup the ascii versions of the profile name and password
  int nProfileLength = sProfileName.GetLength();
  //int nPasswordLength = sPassword.GetLength();
  LPSTR pszProfileName = NULL;
  LPSTR pszPassword = NULL;
  if (nProfileLength)
  {
	pszProfileName = T2A((LPTSTR)(LPCTSTR)sProfileName);
	pszPassword = T2A((LPTSTR)(LPCTSTR)sPassword);
  }

  //Setup the flags & UIParam parameters used in the MapiLogon call
  FLAGS flags = 0;
  ULONG nUIParam = 0;
  if (nProfileLength == 0)
  {
	//No profile name given, then we must interactively request a profile name
	if (pParentWnd)
	{
	  nUIParam = (ULONG) pParentWnd->GetSafeHwnd();
	  flags |= MAPI_LOGON_UI;
	}
	else
	{
	  //No CWnd given, just use the main window of the app as the parent window
	  if (AfxGetMainWnd())
	  {
		nUIParam = (ULONG) AfxGetMainWnd()->GetSafeHwnd();
		flags |= MAPI_LOGON_UI;
	  }
	}
  }
  
  //First try to acquire a new MAPI session using the supplied settings using the MAPILogon functio
  ULONG nError = m_lpfnMAPILogon(nUIParam, pszProfileName, pszPassword, flags | MAPI_NEW_SESSION, 0, &m_hSession);
  if (nError != SUCCESS_SUCCESS && nError != MAPI_E_USER_ABORT)
  {
	//Failed to create a create mapi session, try to acquire a shared mapi session
	TRACE(_T("Failed to logon to MAPI using a new session, trying to acquire a shared one\n"));
	nError = m_lpfnMAPILogon(nUIParam, NULL, NULL, 0, 0, &m_hSession);
	if (nError == SUCCESS_SUCCESS)
	{
	  m_nLastError = SUCCESS_SUCCESS;
	  bSuccess = TRUE;
	}
	else
	{
	  TRACE(_T("Failed to logon to MAPI using a shared session, Error:%d\n"), nError);
	  m_nLastError = nError;
	}
  }
  else if (nError == SUCCESS_SUCCESS)
  {
	m_nLastError = SUCCESS_SUCCESS;
	bSuccess = TRUE;
  }

  RestoreRegionalSettings();
  return bSuccess;
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::LoggedOn() const
{
  return (m_hSession != 0);
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::MapiInstalled() const
{
  return (m_hMapi != NULL);
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::Logoff()
{
  if	(
			!MapiInstalled() ||	//MAPI must be installed
			!m_lpfnMAPILogoff	//Function pointer must be valid
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

  //Initialise the function return value
  BOOL bSuccess = FALSE;

  if (m_hSession)
  {
	//Call the MAPILogoff function
	ULONG nError = m_lpfnMAPILogoff(m_hSession, 0, 0, 0); 
	if (nError != SUCCESS_SUCCESS)
	{
	  TRACE(_T("Failed in call to MapiLogoff, Error:%d"), nError);
	  m_nLastError = nError;
	  bSuccess = TRUE;
	}
	else
	{
	  m_nLastError = SUCCESS_SUCCESS;
	  bSuccess = TRUE;
	}
	m_hSession = 0;
  }
	
  RestoreRegionalSettings();
  return bSuccess;
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::ShowAddressBook(const CStringArray &arOldNames, CStringArray &arNewNames, HWND parent /*= 0*/)
{
	// clean up output array
	arNewNames.RemoveAll ();

	if (!m_hMapi)
		return FALSE;

	if (ShowAddressBookEx(arNewNames))
	{
		return TRUE;
	}

	ULONG nMAPIError;
	lpMapiRecipDesc lpTmpMapiRecipient = NULL;
	
	//filter correct names
	CPtrArray arOldRecipients;
	int i = 0;
	for (i=0; i<arOldNames.GetSize(); i++)
	{
		if (Resolve(arOldNames[i], &lpTmpMapiRecipient))
		{
			arOldRecipients.Add (lpTmpMapiRecipient);
		}
	}

	int nOldSize = arOldRecipients.GetSize();
	lpMapiRecipDesc lpOldMapiRecipients = nOldSize ? new MapiRecipDesc[nOldSize] : NULL;
	
	//store correct names in the array
	for (i=0; i<nOldSize; i++)
	{
		lpTmpMapiRecipient = (lpMapiRecipDesc)arOldRecipients[i];
		lpOldMapiRecipients[i].ulReserved		= lpTmpMapiRecipient->ulReserved  ;
		lpOldMapiRecipients[i].ulRecipClass		= lpTmpMapiRecipient->ulRecipClass;
		lpOldMapiRecipients[i].ulEIDSize		= lpTmpMapiRecipient->ulEIDSize;
		
		lpOldMapiRecipients[i].lpszName			= new char [strlen(lpTmpMapiRecipient->lpszName)+1];
		TB_STRCPY(lpOldMapiRecipients[i].lpszName, lpTmpMapiRecipient->lpszName);
		
		lpOldMapiRecipients[i].lpszAddress		= new char [strlen(lpTmpMapiRecipient->lpszAddress)+1];
		TB_STRCPY(lpOldMapiRecipients[i].lpszAddress, lpTmpMapiRecipient->lpszAddress);
		
		lpOldMapiRecipients[i].lpEntryID		= new char [strlen((LPCSTR)lpTmpMapiRecipient->lpEntryID)+1];
		TB_STRCPY((char*)lpOldMapiRecipients[i].lpEntryID, (LPCSTR)lpTmpMapiRecipient->lpEntryID);

		m_lpfnMAPIFreeBuffer(lpTmpMapiRecipient);
	}
	arOldRecipients.RemoveAll ();

	lpMapiRecipDesc		lpMapiRecipientsList;
	ULONG nNewRecips = 0;

	// invoke dialog and retrieve new names
	nMAPIError = (*m_lpfnMAPIAddress)(m_hSession, (ULONG)parent, NULL, 1, NULL, nOldSize, lpOldMapiRecipients, 0L, 0L, &nNewRecips, &lpMapiRecipientsList);
		
	// release old name resources
	if(lpOldMapiRecipients)
	{
		for (i=0; i<nOldSize; i++)
		{
			delete lpOldMapiRecipients[i].lpszName;
			delete lpOldMapiRecipients[i].lpszAddress;
			delete lpOldMapiRecipients[i].lpEntryID;
		}
		delete [] lpOldMapiRecipients;
	}

	//if(nMAPIError == SUCCESS_SUCCESS)
	{
		USES_CONVERSION;
		for (ULONG i=0; i<nNewRecips; i++)
		{
			//arNewNames.Add (A2T(lpMapiRecipientsList[i].lpszAddress));	

			//TODO RICCARDO
			// per avere l'indirizzo SMTP vero e proprio occorre recuperare dalla struttura sottostante l'id dell'entry e con esso andare a recuperarlo altrove ....
			
			//3.2.1: ma in alcuni casi c'e' veramente l'indirizzo smtp o quello del FAX
			CString sA (lpMapiRecipientsList[i].lpszAddress);
			if (sA.Find(_T("SMTP:")) == 0)
			{
				arNewNames.Add (sA.Mid(5));
			}
			else
			{
				if (sA.Find(_T("FAX:")) == 0)
					arNewNames.Add (sA.Mid(4));
				else 
				{
					CString s(A2T(lpMapiRecipientsList[i].lpszName));
					ManualResolve(s);
					arNewNames.Add (s);
				}
			}
		}
	}
	m_lpfnMAPIFreeBuffer(lpMapiRecipientsList);		
	m_nLastError = nMAPIError;

	return TRUE; //evita falsi negativi (nMAPIError == SUCCESS_SUCCESS);
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::Resolve(const CString& sName, lpMapiRecipDesc* lppRecip)
{
	//For correct operation of the T2A macro, see MFC Tech Note 59
	USES_CONVERSION;

	if	(
			!MapiInstalled() ||			//MAPI must be installed
			!m_lpfnMAPIResolveName ||	//Function pointer must be valid
			!LoggedOn() ||				//Must be logged on to MAPI
			!m_hSession					//MAPI session handle must be valid
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

  //Call the MAPIResolveName function
  LPSTR lpszName = T2A((LPTSTR)(LPCTSTR)sName);
  ULONG nError = m_lpfnMAPIResolveName(m_hSession, 0, lpszName, 0, 0, lppRecip);
  if (nError != SUCCESS_SUCCESS)
  {
	TRACE(_T("Failed to resolve the name: %s, Error:%d\n"), sName, nError);
	m_nLastError = nError;

	
  }

  RestoreRegionalSettings();
  return (nError == SUCCESS_SUCCESS);
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::Send(CMapiMessage& message)
{
	//For correct operation of the T2A macro, see MFC Tech Note 59
	USES_CONVERSION;

	if	(
			!MapiInstalled() ||			//MAPI must be installed
			!m_lpfnMAPISendMail ||		//Function pointer must be valid
			!m_lpfnMAPIFreeBuffer ||	//Function pointer must be valid
			!LoggedOn() ||				//Must be logged on to MAPI
			!m_hSession					//MAPI session handle must be valid
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	MailConnectorParams* generalParams = AfxGetIMailConnector()->GetParams();
	CString sRedirectTo = generalParams->GetRedirectToAddress();
	CString sTrackingAddress = generalParams->GetTrackingAddressForSentEmails();

  //Initialise the function return value
  BOOL bSuccess = FALSE;  

  //Create the MapiMessage structure to match the message parameter send into us
  MapiMessage mapiMessage;
  ZeroMemory(&mapiMessage, sizeof(mapiMessage));
  
  mapiMessage.lpszSubject = T2A((LPTSTR)(LPCTSTR)message.m_sSubject);
  mapiMessage.lpszNoteText = T2A((LPTSTR)(LPCTSTR)message.m_sBody);
  
  mapiMessage.nRecipCount = message.m_To.GetSize() + message.m_CC.GetSize() + message.m_BCC.GetSize(); 
  if (mapiMessage.nRecipCount == 0)
   {
	   //Must have at least 1 recipient!
		ASSERT(FALSE);
		return FALSE;
  }
  if (!sTrackingAddress.IsEmpty())
		mapiMessage.nRecipCount++;

  //NON funziona
 //CString senderAddr(message.m_sReplayTo);
 //if (senderAddr.IsEmpty())
	// senderAddr = message.m_sFrom;
 // if (!senderAddr.IsEmpty())
 // {
	//  mapiMessage.lpOriginator = new MapiRecipDesc;
	//  ZeroMemory(mapiMessage.lpOriginator, sizeof(MapiRecipDesc));
	//  mapiMessage.lpOriginator->lpszAddress = T2A((LPTSTR)(LPCTSTR)senderAddr);

	//  CString sIdent = message.m_sIdentity;
	//  if (sIdent.IsEmpty())
	//	  sIdent = message.m_sFromName;
	//  if (!sIdent.IsEmpty())
	//	mapiMessage.lpOriginator->lpszName = T2A((LPTSTR)(LPCTSTR)sIdent);
 // }

  //Allocate the recipients array
  mapiMessage.lpRecips = new MapiRecipDesc[mapiMessage.nRecipCount];

  int nRecipIndex = 0;
  int i = 0;

  //Setup the "To" recipients
  int nToSize = message.m_To.GetSize();
 for (i = 0; i<nToSize; i++)
  {
	CString sAddrs = message.m_To.ElementAt(i); sAddrs.Trim();
	if (sAddrs.IsEmpty())
		continue;

	MapiRecipDesc& recip = mapiMessage.lpRecips[nRecipIndex];
	ZeroMemory(&recip, sizeof(MapiRecipDesc));
	recip.ulRecipClass = MAPI_TO;
	CString& sName = sRedirectTo.IsEmpty() ? sAddrs : sRedirectTo;

	//Try to resolve the name
	lpMapiRecipDesc lpTempRecip;  
	if (sName.Find('@') < 0 && Resolve(sName, &lpTempRecip))
	{
	  //Resolve worked, put the resolved name back into the sName
	  if (!IsFaxAddress(sName))	//an.9088
		sName = lpTempRecip->lpszName;

	  //Don't forget to free up the memory MAPI allocated for us
	  m_lpfnMAPIFreeBuffer(lpTempRecip);
	}

	recip.lpszName = T2A((LPTSTR)(LPCTSTR)sName);

	++nRecipIndex;
  }

  //Setup the "CC" recipients
  int nCCSize = message.m_CC.GetSize();
  for (i=0; i<nCCSize; i++)
  {
	CString sAddrs = message.m_CC.ElementAt(i); sAddrs.Trim();
	if (sAddrs.IsEmpty())
		continue;

	MapiRecipDesc& recip = mapiMessage.lpRecips[nRecipIndex];
	ZeroMemory(&recip, sizeof(MapiRecipDesc));
	recip.ulRecipClass = MAPI_CC;
	CString& sName = sRedirectTo.IsEmpty() ? sAddrs : sRedirectTo;

	//Try to resolve the name
	lpMapiRecipDesc lpTempRecip;  
	if (sName.Find('@') < 0 && Resolve(sName, &lpTempRecip))
	{
	  //Resolve worked, put the resolved name back into the sName
	  if (!IsFaxAddress(sName))	//an.9088
		sName = lpTempRecip->lpszName;

	  //Don't forget to free up the memory MAPI allocated for us
	  m_lpfnMAPIFreeBuffer(lpTempRecip);
	}
	recip.lpszName = T2A((LPTSTR)(LPCTSTR)sName);

	++nRecipIndex;
  }

  //Setup the "BCC" recipients
  int nBCCSize = message.m_BCC.GetSize();
  for (i=0; i<nBCCSize; i++)
  {
	CString sAddrs = message.m_BCC.ElementAt(i); sAddrs.Trim();
	if (sAddrs.IsEmpty())
		continue;

	MapiRecipDesc& recip = mapiMessage.lpRecips[nRecipIndex];
	ZeroMemory(&recip, sizeof(MapiRecipDesc));
	recip.ulRecipClass = MAPI_BCC;
	CString& sName = sRedirectTo.IsEmpty() ? sAddrs : sRedirectTo;

	//Try to resolve the name
	lpMapiRecipDesc lpTempRecip;  
	if (sName.Find('@') < 0 && Resolve(sName, &lpTempRecip))
	{
	  //Resolve worked, put the resolved name back into the sName
	  if (!IsFaxAddress(sName))	//an.9088
		sName = lpTempRecip->lpszName;

	  //Don't forget to free up the memory MAPI allocated for us
	  m_lpfnMAPIFreeBuffer(lpTempRecip);
	}
	recip.lpszName = T2A((LPTSTR)(LPCTSTR)sName);

	++nRecipIndex;
  }

  if (!sTrackingAddress.IsEmpty())
  {
   MapiRecipDesc& recip = mapiMessage.lpRecips[nRecipIndex];
	ZeroMemory(&recip, sizeof(MapiRecipDesc));
	recip.ulRecipClass = MAPI_BCC;
	CString& sName = sTrackingAddress;

	//Try to resolve the name
	lpMapiRecipDesc lpTempRecip;  
	if (sName.Find('@') < 0 && Resolve(sName, &lpTempRecip))
	{
	  //Resolve worked, put the resolved name back into the sName
	  if (!IsFaxAddress(sName))	//an.9088
		sName = lpTempRecip->lpszName;

	  //Don't forget to free up the memory MAPI allocated for us
	  m_lpfnMAPIFreeBuffer(lpTempRecip);
	}
	recip.lpszName = T2A((LPTSTR)(LPCTSTR)sName);

	++nRecipIndex;
  }

  //Check the attachments 
  int nAttachmentSize = message.m_Attachments.GetSize();
  if (nAttachmentSize)
  {
	for (i = 0; i < nAttachmentSize;)
	{
	   CString sFilename = message.m_Attachments.GetAt(i); sFilename.Trim();
	   if (sFilename.IsEmpty() || !::ExistFile(sFilename))
	   {
		   nAttachmentSize--;
		   message.m_Attachments.RemoveAt(i);
		   if (i < message.m_AttachmentTitles.GetSize())
				message.m_AttachmentTitles.RemoveAt(i);
	   }
	   else 
		   i++;
	}
  }

  int nTitleSize = message.m_AttachmentTitles.GetSize();
  if (nAttachmentSize)
  {
	mapiMessage.nFileCount = nAttachmentSize;
	mapiMessage.lpFiles = new MapiFileDesc[nAttachmentSize];
	for (i = 0; i < nAttachmentSize; i++)
	{
	  CString& sFilename = message.m_Attachments.ElementAt(i);
	  MapiFileDesc& file = mapiMessage.lpFiles[i];
	  ZeroMemory(&file, sizeof(MapiFileDesc));
	  file.nPosition = 0xFFFFFFFF;
	  file.lpszPathName = T2A((LPTSTR)(LPCTSTR)sFilename);
	  file.lpszFileName = file.lpszPathName;
	  if (nTitleSize == nAttachmentSize)
	  {
		CString& sTitle = message.m_AttachmentTitles.ElementAt(i);
		file.lpszFileName = T2A((LPTSTR)(LPCTSTR)sTitle);
	  }
	}
  }

  //Do the actual send using MAPISendMail
  ULONG nError = m_lpfnMAPISendMail(m_hSession, 0, &mapiMessage, 0, 0);
  if (nError == SUCCESS_SUCCESS)
  {
	bSuccess = TRUE;
	m_nLastError = SUCCESS_SUCCESS;
  }
  else
  {
	TRACE(_T("Failed to send mail message, Error:%d\n"), nError);
	m_nLastError = nError;
  }

  //Tidy up the Attachements
  if (nAttachmentSize)
	delete [] mapiMessage.lpFiles;
  
  //Free up the Recipients memory
  delete [] mapiMessage.lpRecips;

  if (mapiMessage.lpOriginator)
	delete mapiMessage.lpOriginator;

  RestoreRegionalSettings();
  return bSuccess;
}

//-----------------------------------------------------------------------------
ULONG CMapiSession::GetLastError() const
{
  return m_nLastError;
}

//-----------------------------------------------------------------------------
BOOL CMapiSession::IsFaxAddress(const CString &sAddress)
{
	CString str(sAddress);
	str.MakeUpper();
	return str.Find(_T("[FAX:")) == 0 || str.Find(_T("<FAX@")) > 0;
}

// by A.R.: decodifica codici di errore
//-----------------------------------------------------------------------------
CString CMapiSession::GetLastErrorMessage()
{
	CString strErrText;
	strErrText.Format(_T("(%u) "), m_nLastError);

	switch (m_nLastError)
	{
		case SUCCESS_SUCCESS						:	strErrText += _T("Success");					break;
		case MAPI_USER_ABORT						:	strErrText += _T("User Abort");					break;
		case MAPI_E_FAILURE							:	strErrText += _T("Failure");					break;
		case MAPI_E_LOGON_FAILURE					:	strErrText += _T("Logon Failure");				break;
		case MAPI_E_DISK_FULL						:	strErrText += _T("Disk Full");					break;
		case MAPI_E_INSUFFICIENT_MEMORY				:	strErrText += _T("Insufficent Memory");			break;
		case MAPI_E_ACCESS_DENIED					:	strErrText += _T("Access Denied");				break;
		case MAPI_E_TOO_MANY_SESSIONS				:	strErrText += _T("Too Many Sessions");			break;
		case MAPI_E_TOO_MANY_FILES					:	strErrText += _T("Too Many Files");				break;
		case MAPI_E_TOO_MANY_RECIPIENTS				:	strErrText += _T("Too Many Recipients");		break;
		case MAPI_E_ATTACHMENT_NOT_FOUND			:	strErrText += _T("Attachment not Found");		break;
		case MAPI_E_ATTACHMENT_OPEN_FAILURE			:	strErrText += _T("Attachment Open Failure");	break;
		case MAPI_E_ATTACHMENT_WRITE_FAILURE		:	strErrText += _T("Attachment Write Failure");	break;
		case MAPI_E_UNKNOWN_RECIPIENT				:	strErrText += _T("Unknow Recipient");			break;
		case MAPI_E_BAD_RECIPTYPE					:	strErrText += _T("Bad Recipient Type");			break;
		case MAPI_E_NO_MESSAGES						:	strErrText += _T("No Messages");				break;
		case MAPI_E_INVALID_MESSAGE					:	strErrText += _T("Invalid Message");			break;
		case MAPI_E_TEXT_TOO_LARGE					:	strErrText += _T("Text Too Large");				break;
		case MAPI_E_INVALID_SESSION					:	strErrText += _T("Invalid Session");			break;
		case MAPI_E_TYPE_NOT_SUPPORTED				:	strErrText += _T("Type Not Supported");			break;
		case MAPI_E_AMBIGUOUS_RECIPIENT				:	strErrText += _T("Ambiguous Recipient");		break;
		case MAPI_E_MESSAGE_IN_USE					:	strErrText += _T("Message in Use");				break;
		case MAPI_E_NETWORK_FAILURE					:	strErrText += _T("Network Failure");			break;
		case MAPI_E_INVALID_EDITFIELDS				:	strErrText += _T("Invalid Edit Fields");		break;
		case MAPI_E_INVALID_RECIPS					:	strErrText += _T("Invalid Recipients");			break;
		case MAPI_E_NOT_SUPPORTED					:	strErrText += _T("Not Supported");				break;

		default:
			strErrText += _T("Unknow MAPI Error");
		break;
	}

	return strErrText;
}

/*
// utility function to release ADRLIST entries
void CMAPIEx::ReleaseAddressList(LPADRLIST pAddressList)
{
#ifndef _WIN32_WCE
	FreePadrlist(pAddressList);
#else
	if(pAddressList) 
	{
		for(ULONG i=0;i<pAddressList->cEntries;i++) MAPIFreeBuffer(pAddressList->aEntries[i].rgPropVals);
		MAPIFreeBuffer(pAddressList);
	}
#endif
}

BOOL CMAPIEx::ShowAddressBook(CString& str)
{
	CMAPIEx mapi;
	CString strProfileName;
	BOOL bOk = FALSE;
	if (mapi.GetProfileName(strProfileName))
		bOk = mapi.Login(strProfileName);
	if (!bOk)
		bOk = mapi.Login();
	if (!bOk || !mapi.OpenMessageStore()) 
	{
		TRACE(_T("Failed to initialize MAPI\n"));
		return FALSE;
	}

	LPADRLIST pAddressList = NULL;
	if (mapi.ShowAddressBook(pAddressList) == IDOK) 
	{
		CString strEmail;
		for(ULONG i=0;i<pAddressList->cEntries;i++) 
		{
			if(mapi.GetEmail(pAddressList->aEntries[i],strEmail))
			{
				if (i < (pAddressList->cEntries - 1))
					str += strEmail + ';';
				else
					str += strEmail;
				TRACE(_T("AddressBook recipient %d: %s\n"), i+1, strEmail);
			}
		}
		CMAPIEx::ReleaseAddressList(pAddressList);
	}
	return TRUE;
}


// Shows the default Address Book dialog, return FALSE on failure, IDOK or IDCANCEL on success
// I force narrow strings here because it for some reason doesn't work in UNICODE
int CMAPIEx::ShowAddressBook(LPADRLIST& pAddressList, LPCTSTR szCaption)
{
#ifndef _WIN32_WCE
	if(!m_pSession) return FALSE;

	LPADRBOOK pAddressBook;
	if(m_pSession->OpenAddressBook(0, NULL, 0, &pAddressBook)==S_OK) 
	{
		pAddressList=NULL;

		char* lppszDestTitles[]={ "To" };
		ULONG lpulDestComps[]={ MAPI_TO };

		ADRPARM adrparm;
		memset(&adrparm, 0,sizeof(ADRPARM));
		adrparm.ulFlags=DIALOG_MODAL | AB_RESOLVE;

		adrparm.lpszCaption=(LPTSTR)szCaption;
#ifdef UNICODE
		if(szCaption) 
		{
			char szNarrowCaption[256];
			WideCharToMultiByte(CP_ACP, 0, szCaption,-1, szNarrowCaption,255, NULL, NULL);
			adrparm.lpszCaption=(LPTSTR)szNarrowCaption;
		}
#endif
		adrparm.cDestFields = 1;
		adrparm.lppszDestTitles=(LPTSTR*)lppszDestTitles;
		adrparm.lpulDestComps=lpulDestComps;

		HWND hDesktop=::GetDesktopWindow();
		HRESULT hr=pAddressBook->Address((ULONG_PTR*)&hDesktop, &adrparm, &pAddressList);
		RELEASE(pAddressBook);
		if(hr==S_OK) return IDOK;
		if(hr==MAPI_E_USER_CANCEL) return IDCANCEL;
	}
#endif
	return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//utilizzata sino alla 3.2.1 era in TbGeneric - CMapi.cpp
BOOL MapiShowAddressBook(HWND hWnd, CString& strAddress)
{
	HINSTANCE			hlibMAPI ;
	LPMAPILOGON			lpfnMAPILogon = NULL;
	LPMAPIADDRESS		lpfnMAPIAddress = NULL;
	LPMAPIFREEBUFFER	lpfnMAPIFreeBuffer = NULL;
	LPMAPIRESOLVENAME	lpfnMAPIResolveName = NULL;
	
	ULONG				nMAPIError;
	lpMapiRecipDesc		lpMapiRecipientsList;
	ULONG nNewRecips = 0;


	if(::GetProfileInt(_T("MAIL"), _T("MAPI"), 0) != 0 &&
			SearchPath(NULL, _T("MAPI32.DLL"), NULL, 0, NULL, NULL) != 0)
		hlibMAPI = ::LoadLibrary(_T("MAPI32.DLL"));

	if (!hlibMAPI)
	{
		//AfxMessageBox(cwsprintf(IDS_UNABLE_TO_LOAD_LIBRARY, "MAPI32.DLL"));
		return FALSE;
	}
	if 
		(
			!(lpfnMAPILogon = (LPMAPILOGON) GetProcAddress (hlibMAPI, "MAPILogon")) ||
			!(lpfnMAPIResolveName = (LPMAPIRESOLVENAME) GetProcAddress (hlibMAPI, "MAPIResolveName")) ||
			!(lpfnMAPIAddress = (LPMAPIADDRESS) GetProcAddress (hlibMAPI, "MAPIAddress")) ||
			!(lpfnMAPIFreeBuffer = (LPMAPIFREEBUFFER) GetProcAddress (hlibMAPI, "MAPIFreeBuffer"))
		)
	{
		::FreeLibrary(hlibMAPI);
		return FALSE;
	}
	//BeginWaitCursor();
	
	int				nOldRecNum = 0;
	LHANDLE			hMAPISession = 0L;

	MapiRecipDesc* pMapiOldRecipient = NULL;

	if (!strAddress.IsEmpty())
	{
		TCHAR* nextToken;
		TCHAR* pStrRecipients = strAddress.GetBuffer(strAddress.GetLength());
		TCHAR *pToken = _tcstok_s(pStrRecipients, _T(";"), &nextToken);
		CStringArray arRecipients;
		while(pToken != NULL)
		{
			arRecipients.Add(pToken);
			pToken = _tcstok_s(NULL, _T(";"), &nextToken);
		}
		
		pMapiOldRecipient = new MapiRecipDesc[arRecipients.GetSize()];

		for (int i=0; i < arRecipients.GetSize(); i++)
		{
			lpMapiRecipDesc lpTmpMapiRecipient = NULL;
			while(TRUE)
			{
				USES_CONVERSION;
				nMAPIError = (*lpfnMAPIResolveName)
									(
										hMAPISession,		// session handle
										0L,					// no UI handle
										T2A(arRecipients[i].GetBuffer(arRecipients[i].GetLength())),	// friendly name
										0L,					// no flags, no UI allowed
										0L,					// reserved; must be 0
										&lpTmpMapiRecipient	// where to put the result
									);
				if (!hMAPISession && nMAPIError == MAPI_E_LOGON_FAILURE)
				{
					nMAPIError = (*lpfnMAPILogon)
									(
										(ULONG) hWnd,       
										"",   
										"",   
										MAPI_LOGON_UI,         
										0L,      
										&hMAPISession  
									);
					if( nMAPIError != SUCCESS_SUCCESS)
					{
						(*lpfnMAPIFreeBuffer)(lpTmpMapiRecipient);  // release the recipient descriptors
						::FreeLibrary(hlibMAPI);
						//if( nMAPIError != MAPI_E_USER_ABORT)
							//AfxMessageBox(cwsprintf(IDS_SCHEDULER_MAPI_LOGON_FAILED_LOGFILE_MSG, nMAPIError));
						delete []pMapiOldRecipient;

						return FALSE;
					}
					continue;
				}
				break;
			}
			
			if ((nMAPIError == SUCCESS_SUCCESS) && lpTmpMapiRecipient)
				{
					pMapiOldRecipient[nOldRecNum].ulReserved	= lpTmpMapiRecipient->ulReserved  ;
					pMapiOldRecipient[nOldRecNum].ulRecipClass	= lpTmpMapiRecipient->ulRecipClass;
					pMapiOldRecipient[nOldRecNum].ulEIDSize		= lpTmpMapiRecipient->ulEIDSize;
					pMapiOldRecipient[nOldRecNum].lpszName		= new char [strlen(lpTmpMapiRecipient->lpszName)+1];
					TB_STRCPY(pMapiOldRecipient[nOldRecNum].lpszName, lpTmpMapiRecipient->lpszName);
					pMapiOldRecipient[nOldRecNum].lpszAddress		= new char [strlen(lpTmpMapiRecipient->lpszAddress)+1];
					TB_STRCPY(pMapiOldRecipient[nOldRecNum].lpszAddress, lpTmpMapiRecipient->lpszAddress);
					pMapiOldRecipient[nOldRecNum].lpEntryID		= new char [strlen((LPCSTR)lpTmpMapiRecipient->lpEntryID)+1];
					TB_STRCPY((char*)pMapiOldRecipient[nOldRecNum].lpEntryID, (LPCSTR)lpTmpMapiRecipient->lpEntryID);

					nOldRecNum ++;
				}

			nMAPIError = (*lpfnMAPIFreeBuffer)(lpTmpMapiRecipient);  // release the recipient descriptors
		}	
	}
//	EndWaitCursor();

	while(TRUE)
	{
		nMAPIError = (*lpfnMAPIAddress)(hMAPISession, (ULONG)hWnd, NULL, 1, NULL, nOldRecNum, nOldRecNum ? pMapiOldRecipient : NULL, 0L, 0L, &nNewRecips, &lpMapiRecipientsList);
		if (!hMAPISession && nMAPIError == MAPI_E_LOGON_FAILURE)
		{
			nMAPIError = (*lpfnMAPILogon)
							(
								(ULONG) hWnd,       
								"",   
								"",   
								MAPI_LOGON_UI,         
								0L,      
								&hMAPISession  
							);
			if( nMAPIError != SUCCESS_SUCCESS)
			{
				//if( nMAPIError != MAPI_E_USER_ABORT)
					//AfxMessageBox(cwsprintf(IDS_SCHEDULER_MAPI_LOGON_FAILED_LOGFILE_MSG, nMAPIError));
				break;
			}
			continue;
		}
		break;
	}

	if(nMAPIError == SUCCESS_SUCCESS && lpMapiRecipientsList)
	{
		strAddress.Empty(); 
		
		for (UINT i = 0; i < nNewRecips ; i++)
		{
			if (i > 0) strAddress += _T("; ");
			strAddress += lpMapiRecipientsList[i].lpszName;			
		}
	}
	else if(nMAPIError == 2147500037 && lpMapiRecipientsList && nNewRecips > 0)
	{
		//TODO RICCARDO

		//strAddress.Empty(); 
		
		//for (UINT i = 0; i < nNewRecips ; i++)
		//{
		//	if (i > 0) strAddress += _T("; ");
		//	else if (!strAddress.IsEmpty()) strAddress += _T("; ");
		//	// per avere l'indirizzo vero e proprio occorre recuperare dalla struttura sottostante l'id dell'entry e con esso andare a recuperarlo altrove ....
		//	strAddress += lpMapiRecipientsList[i].lpszAddress;			
		//}
	}
	else strAddress.Empty(); 

	if (hMAPISession)
	{
		LPMAPILOGOFF lpfnMAPILogoff = NULL;
		if (lpfnMAPILogoff = (LPMAPILOGOFF) GetProcAddress (hlibMAPI, "MAPILogoff"))
			(*lpfnMAPILogoff)
					(
						(LHANDLE)hMAPISession,   
						(ULONG) hWnd,       
						0L,       
						0L     
					);
	}
	
	for (int i=0; i<nOldRecNum; i++)
	{
		delete pMapiOldRecipient[i].lpszName;
		delete pMapiOldRecipient[i].lpszAddress;
		delete pMapiOldRecipient[i].lpEntryID;
	}

	delete [] pMapiOldRecipient;

	(*lpfnMAPIFreeBuffer)(lpMapiRecipientsList);  // release the recipient descriptors

	::FreeLibrary(hlibMAPI);
	return (nMAPIError == SUCCESS_SUCCESS);
}
///////////////////////////////////////////////////////////////////////////////
I need to know how to get an e-mail address using MAPI.
When I examine the contents of a MapiRecipDesc structure
(returned, for instance, by a call to MAPIAddress()) 
I see that the lpszAddress field contains not a normal SMTP address,
but what appears to be some sort of an Exchange server query:
EX:/o=MMS/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Recipients/cn=laiic
Does anyone know if I could obtain an external email address this way, and if so, how I might accomplish this?

=>Answer:
Its been a while since I've worked with MAPI, but in a nutshell:
This is an X500 address.
You should be able to lookup this address via LDAP (to the AD Server)
using the LegacyExchangeDN attribute as the filter.
At that point you can pull the proxyAddresses attribute of the returned object 
and iterate through it looking for the address with a smtp: prefix ...
the one with the all uppercase SMTP: is the primary email account
for the object if more than one smtp: exist

*/

#define RELEASE(s) if(s!=NULL) { s->Release();s=NULL; }

#ifndef PR_SMTP_ADDRESS
#define PR_SMTP_ADDRESS PROP_TAG(PT_TSTRING, 0x39FE)
#endif

// special case of GetValidString to take the narrow string in UNICODE
// sometimes the string in prop is invalid, causing unexpected crashes
LPCTSTR ValidateString(LPCTSTR s)
{
	if(s && !::IsBadStringPtr(s, (UINT_PTR)-1)) return s;
	return NULL;
}

LPCTSTR GetValidString(SPropValue& prop)
{
	return ValidateString(prop.Value.LPSZ);
}

void GetNarrowString(SPropValue& prop, CString& strNarrow)
{
	LPCTSTR s = GetValidString(prop);
	if(!s) 
		strNarrow=_T("");
	else {
#ifdef UNICODE
		// VS2005 can copy directly
		if(_MSC_VER>=1400) 
		{
			strNarrow=(char*)s;
		} 
		else 
		{
			WCHAR wszWide[256];
			MultiByteToWideChar(CP_ACP, 0, (LPCSTR)s,-1,wszWide,255);
			strNarrow=wszWide;
		}
#else
		strNarrow=s;
#endif
	}
}

ULONG  myFBadPropTag(ULONG ulPropTag)
 {
     //TRACE("(0x%08x)\n", ulPropTag);
 
     switch (ulPropTag & (~MV_FLAG & PROP_TYPE_MASK))
     {
     case PT_UNSPECIFIED:
     case PT_NULL:
     case PT_I2:
     case PT_LONG:
     case PT_R4:
     case PT_DOUBLE:
     case PT_CURRENCY:
     case PT_APPTIME:
     case PT_ERROR:
     case PT_BOOLEAN:
     case PT_OBJECT:
     case PT_I8:
     case PT_STRING8:
     case PT_UNICODE:
     case PT_SYSTIME:
     case PT_CLSID:
     case PT_BINARY:
         return FALSE;
     }
     return TRUE;
 }


LPSPropValue myPpropFindProp(LPSPropValue lpProps, ULONG cValues, ULONG ulPropTag)
 {
     //TRACE("(%p,%d,%d)\n", lpProps, cValues, ulPropTag);
 
     if (lpProps && cValues)
     {
         ULONG i;
         for (i = 0; i < cValues; i++)
         {
             if (!myFBadPropTag(lpProps[i].ulPropTag) &&
                 (lpProps[i].ulPropTag == ulPropTag ||
                  (PROP_TYPE(ulPropTag) == PT_UNSPECIFIED &&
                   PROP_ID(lpProps[i].ulPropTag) == PROP_ID(ulPropTag))))
                 return &lpProps[i];
         }
     }
     return NULL;
 }

//-----------------------------------------------------------------------------
BOOL CMapiSession::ShowAddressBookEx(CStringArray &arNewNames)
{
	if (m_lpfnMAPILogonEx == NULL)
		return FALSE;

   HRESULT         hRes = S_OK;
   IMAPISession* pSession = NULL;

	DWORD dwFlags = MAPI_EXTENDED | MAPI_USE_DEFAULT | MAPI_NEW_SESSION | MAPI_NO_MAIL;
	hRes = m_lpfnMAPILogonEx(NULL, NULL, NULL, dwFlags, &pSession);
	
	if (hRes!=S_OK)
		return FALSE;

	LPADRLIST pAddressList;
	int ret = ShowAddressBookEx(pSession, pAddressList, _T("Address Book"));
	if (ret == IDCANCEL)
		return TRUE;
	if (ret == IDOK) 
	{
		CString strEmail;
		for(ULONG i=0;i<pAddressList->cEntries;i++) 
		{
			if(GetEmail(pSession, pAddressList->aEntries[i], strEmail)) 
			{
				TRACE(_T("AddressBook recipient %d: %s\n"), i+1, strEmail);
				arNewNames.Add(strEmail);
			}
		}
		m_lpfnFreePadrlist(pAddressList); //ReleaseAddressList
		return TRUE;
	}
	return FALSE;
}


// Shows the default Address Book dialog, return FALSE on failure, IDOK or IDCANCEL on success
// I force narrow strings here because it for some reason doesn't work in UNICODE
int CMapiSession::ShowAddressBookEx(IMAPISession* pSession, LPADRLIST& pAddressList, LPCTSTR szCaption)
{
	if(!pSession) return FALSE;

	LPADRBOOK pAddressBook;
	if(pSession->OpenAddressBook(0, NULL, 0, &pAddressBook)==S_OK) 
	{
		pAddressList=NULL;

		char* lppszDestTitles[]={ "To" };
		ULONG lpulDestComps[]={ MAPI_TO };

		ADRPARM adrparm;
		memset(&adrparm, 0,sizeof(ADRPARM));
		adrparm.ulFlags=DIALOG_MODAL | AB_RESOLVE;

		adrparm.lpszCaption=(LPTSTR)szCaption;
#ifdef UNICODE
		if(szCaption) 
		{
			char szNarrowCaption[256];
			WideCharToMultiByte(CP_ACP, 0, szCaption,-1, szNarrowCaption,255, NULL, NULL);
			adrparm.lpszCaption=(LPTSTR)szNarrowCaption;
		}
#endif
		adrparm.cDestFields = 1;
		adrparm.lppszDestTitles=(LPTSTR*)lppszDestTitles;
		adrparm.lpulDestComps=lpulDestComps;

		HWND hDesktop=::GetDesktopWindow();
		HRESULT hr=pAddressBook->Address((ULONG_PTR*)&hDesktop, &adrparm, &pAddressList);
		RELEASE(pAddressBook);
		if(hr==S_OK) return IDOK;
		if(hr==MAPI_E_USER_CANCEL) return IDCANCEL;
	}

	return FALSE;
}


// utility function to release ADRLIST entries
//void CMapiSession::ReleaseAddressList(LPADRLIST pAddressList)
//{
//
//	FreePadrlist(pAddressList);
//
//	//if(pAddressList) 
//	//{
//	//	for(ULONG i=0;i<pAddressList->cEntries;i++) MAPIFreeBuffer(pAddressList->aEntries[i].rgPropVals);
//	//	MAPIFreeBuffer(pAddressList);
//	//}
//
//}

//BOOL CMapiSession::::CompareEntryIDs(ULONG cb1, LPENTRYID lpb1, ULONG cb2, LPENTRYID lpb2)
//{
//	ULONG ulResult;
//	if(m_pSession && m_pSession->CompareEntryIDs(cb1, lpb1, cb2, lpb2, 0, &ulResult)==S_OK) 
//	{
//		return ulResult;
//	}
//	return FALSE;
//}

// ADDRENTRY objects from Address don't come in unicode so I check for _A and force narrow strings
BOOL CMapiSession::GetEmail(IMAPISession* pSession, ADRENTRY& adrEntry, CString& strEmail)
{
	LPSPropValue pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_ADDRTYPE);
	if (!pProp) 
		pProp=myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_ADDRTYPE_A);
	if (pProp) 
	{
		CString strAddrType;
		GetNarrowString(*pProp, strAddrType);
		if (strAddrType == _T("EX")) 
		{
			pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_ENTRYID);

			SBinary entryID;
			entryID.cb = pProp->Value.bin.cb;
			entryID.lpb = pProp->Value.bin.lpb;

			return GetExEmail(pSession, entryID, strEmail);
		}
		else if (strAddrType == _T("MAPIPDL")) 
		{
/*			pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_DISPLAY_NAME);
			if (!pProp)
				pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_EMAIL_ADDRESS);
			if (!pProp)
				*/pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_ENTRYID);
			if (pProp)
			{
				SBinary entryID;
				entryID.cb=pProp->Value.bin.cb;
				entryID.lpb=pProp->Value.bin.lpb;

				return GetExEmail(pSession, entryID, strEmail);
			}
		}
	}
	pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_EMAIL_ADDRESS);
	if (!pProp) 
		pProp = myPpropFindProp(adrEntry.rgPropVals,adrEntry.cValues, PR_EMAIL_ADDRESS_A);
	if (pProp) 
	{
		GetNarrowString(*pProp, strEmail);
		return TRUE;
	}
	return FALSE;
}

BOOL CMapiSession::GetExEmail(IMAPISession* pSession, SBinary entryID, CString& strEmail)
{
	BOOL bResult=FALSE;

	if (!pSession) 
		return FALSE;

	LPADRBOOK pAddressBook;
	if (pSession->OpenAddressBook(0, NULL, AB_NO_DIALOG, &pAddressBook) == S_OK) 
	{
		ULONG ulObjType;
		IMAPIProp* pItem=NULL;
		if (pAddressBook->OpenEntry(entryID.cb, (ENTRYID*)entryID.lpb, NULL,MAPI_BEST_ACCESS, &ulObjType, (LPUNKNOWN*)&pItem)==S_OK) 
		{
			if (ulObjType == MAPI_MAILUSER || ulObjType == MAPI_DISTLIST)
			{
				LPSPropValue pProp;
				ULONG ulPropCount;
				ULONG pSMTP[]={ 1, PR_SMTP_ADDRESS };
				ULONG pEmail[]={ 1, PR_EMAIL_ADDRESS };
				ULONG pName[]={ 1, PR_DISPLAY_NAME };

				if (pItem->GetProps((LPSPropTagArray)pSMTP, MAPI_UNICODE, &ulPropCount, &pProp)==S_OK) 
				{
					strEmail = GetValidString(*pProp);
					m_lpfnMAPIFreeBuffer(pProp);
					bResult=TRUE;
				}
				if (!bResult && pItem->GetProps((LPSPropTagArray)pEmail, MAPI_UNICODE, &ulPropCount, &pProp)==S_OK) 
				{
					strEmail = GetValidString(*pProp);
					m_lpfnMAPIFreeBuffer(pProp);
					bResult=TRUE;
				}
				if (!bResult && pItem->GetProps((LPSPropTagArray)pName, MAPI_UNICODE, &ulPropCount, &pProp)==S_OK) 
				{
					strEmail = GetValidString(*pProp);
					m_lpfnMAPIFreeBuffer(pProp);
					bResult=TRUE;
				}
			}
			RELEASE(pItem);
		}
		RELEASE(pAddressBook);
	}

	return bResult;
}

//-----------------------------------------------------------------------------
IMapiSession* NewMapiSession(BOOL bMultiThreaded /*= TRUE*/, BOOL bNoLogonUI /*= FALSE*/, BOOL bNoInitializeMAPI /*= FALSE*/, CDiagnostic* pDiagnostic /*= NULL*/)
{
	return new CMapiSession(bMultiThreaded, bNoLogonUI, bNoInitializeMAPI, pDiagnostic);
}
