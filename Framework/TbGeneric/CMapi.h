
#pragma once

#include "Array.h"
#include "Dataobj.h"
#include "EnumsConst.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class CDiagnostic;
class SqlSession;

class TB_EXPORT CPostaLiteAddress : public CObject
{
public:
	enum Delivery
	{
		PostaMassiva,
		PostaPrioritaria,
		PostaRaccomandata,
		PostaRaccomandataAR/*,
		Fax*/
	};

	enum Print
	{
		Front_BlackWhite,
		FrontBack_BlackWhite,
		Front_Color,
		FrontBack_Color
	};

	enum CodeState
	{
		Enabled = 1,
		Suspended = 2,
		Disabled = -21,
	};

	static Delivery		ConvertDeliveryType	(DataEnum deDelivery);
	static Print		ConvertPrintType	(DataEnum dePrint);

	enum LotStatus
	{
		Allotted = 0,
		Uploaded = 1,
		Closed = 2,
		Uploading = 3,
		Invalid = 4
	};

	enum LotStatusExt
	{
		None = 0,
		PresoInCarico = 1,
		Annullato = 2,
		InElaborazione = 3,
		Errato = 4,
		Spedito = 5,
		SpeditoConInesitati = 6,
		InStampa = 9,
		Sospeso = 11,
		AnnullatoParzialmente = 12,
	};

	CString	m_sFax;
	CString	m_sAddressee;
	CString	m_sAddress;
	CString	m_sZipCode;	
	CString	m_sCity;	
	CString	m_sCounty;	
	CString	m_sCountry;
	CString	m_sISOCode;

	DataEnum	m_deDeliveryType; 
	DataEnum	m_dePrintType;

	CString	 m_sAuxKey;

public:
	CPostaLiteAddress();
	CPostaLiteAddress(const CPostaLiteAddress&);

	CPostaLiteAddress
		(
			const CString&	sFax,
			const CString&	sAddressee,
			const CString&	sAddress,
			const CString&	sZipCode,	
			const CString&	sCity,	
			const CString&	sCounty,	
			const CString&	sCountry,
			const CString&	sISOCode,

			const DataEnum&	deDeliveryType, 
			const DataEnum&	dePrintType,
			const CString&	sAuxKey
		)
		:
		m_sFax			(sFax),
		m_sAddressee	(sAddressee),
		m_sAddress		(sAddress),
		m_sZipCode		(sZipCode),	
		m_sCity			(sCity),	
		m_sCounty		(sCounty),	
		m_sCountry		(sCountry),
		m_sISOCode		(sISOCode),

		m_deDeliveryType	(deDeliveryType),
		m_dePrintType		(dePrintType),
		m_sAuxKey			(sAuxKey)
	{}

	CPostaLiteAddress
		(
			const CString&	sFax,
			const CString&	sISOCode
		)
		:
		m_sFax				(sFax),
		m_sISOCode			(sISOCode),
		m_deDeliveryType	(0)
	{}

	CPostaLiteAddress& operator = (const CPostaLiteAddress& a);
};

class TB_EXPORT CPostaLiteMsg : public Array
{
public:
	BOOL		m_bUsePostaLite;

	CString		m_sDocNamespace;
	CString		m_sDocPrimaryKey;

	CString		m_sAddresseeNamespace;
	CString		m_sAddresseePrimaryKey;

	CString		m_sFileName;
	int			m_nFilePages;
	int			m_nFileSize;

public:
	CPostaLiteMsg() 
		: 
		m_bUsePostaLite	(FALSE),
		m_nFilePages	(0),
		m_nFileSize		(0)
		{}

	CPostaLiteMsg(const CPostaLiteMsg& e) 
		{ Assign(e); }

	CPostaLiteMsg& operator=(const CPostaLiteMsg& e) { if (this != &e) Assign(e); return *this; }
	void Assign(const CPostaLiteMsg&);

	CPostaLiteAddress* 		operator[]	(int nIndex) const	{ return (CPostaLiteAddress*) GetAt(nIndex); }
	CPostaLiteAddress*& 	operator[]	(int nIndex)		{ return (CPostaLiteAddress*&) ElementAt(nIndex); }

	int Add
		(
			const CString&	sFax,
			const CString&	sAddressee,
			const CString&	sAddress,
			const CString&	sZipCode,	
			const CString&	sCity,	
			const CString&	sCounty,	
			const CString&	sCountry,
			const CString&	sISOCode,

			const DataEnum&	deDeliveryType, 
			const DataEnum&	dePrintType,
			const CString&	sAuxKey = CString()
		);

	int Add (const CString&	sFax, const CString& sISOCode);

	int Add (CPostaLiteAddress* pAddr) { return __super::Add(pAddr); }

	void Clear();
	BOOL IsEmpty();
};

//=============================================================================

//Class which encapsulates a mail message OR a PostaLite sends documents
class TB_EXPORT CMapiMessage
{
public:
	CString			m_sSubject;
	CString			m_sBody;
	CString			m_sFrom;
	CString			m_sIdentity; //serve a contenere una descrizione del mittente
	CString			m_sFromName;
	CString			m_sReplayTo;

	CStringArray	m_To;
	CStringArray	m_CC;
	CStringArray	m_BCC;
	CStringArray	m_Attachments;
	CStringArray	m_AttachmentTitles; //Titles to use for the email file attachments

	CString			m_sTemplateFileName;
	CString			m_sMapiProfile;
	CString			m_sAttachmentReportName;

	BOOL			m_bBodyFromFile;
	BOOL			m_bHtmlFromFile;
	BOOL			m_bNeedEncoding;
	BOOL			m_bAlreadyEncoded;
	CString			m_sHtml;

	CStringArray	m_arstrBodyParameters; //array dei parametri del report
	CStringArray	m_arstrBodyParameterNames;//Nome degli eventuali parametri passati con tale modalità: il valore e' nell'altro array con pari indice 

	CStringArray	m_Images;
	CStringArray	m_ImagesCIDs;		//Identifiers to use for the email file gifs

	CPostaLiteMsg	m_PostaLiteMsg;

	DataEnum		m_deEmailAddressType;

public:
	CMapiMessage() 
		:
			m_bBodyFromFile(FALSE),
			m_bHtmlFromFile(FALSE),
			m_bNeedEncoding(FALSE),
			m_bAlreadyEncoded(FALSE),
			m_deEmailAddressType (E_EMAIL_ADDRESS_TYPE_DEFAULT)
		{}
	CMapiMessage(const CMapiMessage& e) 
		{ Assign(e); }

	CMapiMessage& operator=(const CMapiMessage& e) { if (this != &e) Assign(e); return *this; }
	void Assign(const CMapiMessage&);

	void Clear();
	BOOL IsEmpty();
	BOOL IsEmptyRecipients();

public:
	void SetSubject				(LPCTSTR pszSubject, BOOL bAppend=FALSE, BOOL bSkipDuplicate=FALSE);
	void SetBody(LPCTSTR pszBody, BOOL bAppend = TRUE, BOOL bSkipDuplicate = FALSE);
	void SetHtml				(LPCTSTR pszHtml, BOOL bNeedEncoding, BOOL bAppend=TRUE);
	void SetBodyFileName		(LPCTSTR pszBodyFile);
	void SetHtmlFileName		(LPCTSTR pszHtmlFile, BOOL bNeedEncoding);
	void SetFrom				(LPCTSTR pszFrom);
	void SetTemplateFileName	(LPCTSTR pszTemplateFileNam);
	void SetMapiProfile			(LPCTSTR pszMapiProfile);
	void SetIdentity			(LPCTSTR pszIdentity);	//Recipient Identity (Es: Company name)
	void SetFromName			(LPCTSTR psz);
	void SetReplayTo			(LPCTSTR psz);

	void SetTo					(LPCTSTR pszTo, BOOL bAppend=TRUE, LPCTSTR szTag = NULL);
	void SetCc					(LPCTSTR pszCc, BOOL bAppend=TRUE, LPCTSTR szTag = NULL);
	void SetBcc					(LPCTSTR pszBcc, BOOL bAppend=TRUE, LPCTSTR szTag = NULL);

	void SetAttachment			(LPCTSTR pszAttachment, BOOL bAppend = TRUE);
	void SetAttachment			(LPCTSTR pszAttachment, LPCTSTR pszAttachmentTitle, BOOL bAppend, int pos = -1);
	void InsertTopAttachment	(LPCTSTR pszAttachment, LPCTSTR pszAttachmentTitle);
	void RemoveAllPdfAttachments();

	void SetImage				(LPCTSTR pszImage, LPCTSTR pszImageCID = NULL, BOOL bAppend = TRUE);

	void SetBodyParameter		(LPCTSTR pszNameParameter, LPCTSTR pValue);
	BOOL GetBodyParameter		(LPCTSTR pszName, CString& sValue);
	void SetAttachmentReportName (LPCTSTR psz);

	int AddPostaLiteMsg
		(
			const CString&	sFax,
			const CString&	sAddressee,
			const CString&	sAddress,
			const CString&	sZipCode,	
			const CString&	sCity,	
			const CString&	sCounty,	
			const CString&	sCountry,
			const CString&	sISOCode,

			const DataEnum&	deDeliveryType, 
			const DataEnum&	dePrintType,
			const CString&	sAuxKey = CString()
		);

	BOOL UsePostaLite () const { return m_PostaLiteMsg.m_bUsePostaLite; }
	void SetUsePostaLite (BOOL b)  { m_PostaLiteMsg.m_bUsePostaLite = b; }

	void SplitAllAddress();
	BOOL SplitCertifiedAddress(CMapiMessage& msgNormal, CMapiMessage& msgCertified);

	static CString TAG_CERTIFIED; 
};

//=============================================================================

// by A.R. parametro pe forzare l'uso del profilo MAPI di default anche se c'è
// una main window
#define USE_DEFAULT_MAPI_PROFILE	"!!DEFAULT!!"

class MailConnectorParams;

//The class which encapsulates the MAPI connection
class TB_EXPORT IMapiSession
{
public:
//Constructors / Destructors
 IMapiSession(BOOL bMultiThreaded = FALSE, BOOL bNoLogonUI = FALSE, BOOL bNoInitializeMAPI = FALSE, CDiagnostic* pDiagnostic = NULL);
 virtual ~IMapiSession();

public:

//Logon / Logoff Methods
  virtual BOOL Logon(const CString& sProfileName = CString(USE_DEFAULT_MAPI_PROFILE), const CString& sPassword = CString(), CWnd* pParentWnd = NULL);
  virtual BOOL LoggedOn() const;
  virtual BOOL Logoff();

//Send a message
  virtual BOOL Send(CMapiMessage& mesage);

//show address book
  virtual BOOL ShowAddressBook(const CStringArray &arOldNames, CStringArray &arNewNames, HWND parent = 0);

  BOOL IsFaxAddress(const CString &sAddress) { return FALSE; }

//General MAPI support
  virtual BOOL MapiInstalled() const;

//Error Handling
  virtual ULONG	  GetLastError() const;
  virtual CString GetLastErrorMessage();	// by A.R.
};

//=============================================================================
class TB_EXPORT IMailConnector : public CObject
{
	DECLARE_DYNAMIC(IMailConnector);
public:
	IMailConnector() : m_pParams(NULL) {}
	virtual ~IMailConnector();

	virtual IMapiSession* NewMapiSession(BOOL bMultiThreaded = TRUE, BOOL bNoLogonUI = FALSE, BOOL bNoInitializeMAPI = FALSE, CDiagnostic* pDiagnostic = NULL) { return new IMapiSession(); }

	virtual BOOL SmtpSendMail 
		(
			CMapiMessage& msgMapi,
			BOOL* pbRequestDeliveryNotification = NULL,
			BOOL* pbRequestReadNotification = NULL,
			CDiagnostic* pDiagnostic = NULL
		) 
		{ return FALSE; }

	virtual BOOL ShowEmailDlg		
		(
			CDocument* pCallerDoc,
			CMapiMessage& e, 

			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,

			int*  = NULL,
			int*  = NULL,

			BOOL* pbConcatAttachPDF = NULL,
			BOOL* pbRequestDeliveryNotification = NULL,
			BOOL* pbRequestReadNotification = NULL,

			LPCTSTR	pszCaptionOkBtn = NULL

		)	{ return FALSE; }

	BOOL ShowEmailDlg 		
		(
			CDocument* pCallerDoc, 
			CMapiMessage& msg,
			CString	sCaptionOkBtn
		)
	{ return ShowEmailDlg(pCallerDoc, msg, NULL,NULL,NULL, NULL,NULL, NULL,NULL,NULL, (LPCTSTR) sCaptionOkBtn); }

	BOOL ShowEmailDlg 		
		(
			CMapiMessage& msg
		)
	{ return ShowEmailDlg(NULL, msg); }

	virtual BOOL ShowEmailWithChildDlg		
		(
			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,
			BOOL* pbConcatAttachPDF = NULL
		)	{ return FALSE; }

	virtual BOOL MapiShowAddressBook(HWND hWnd, CString& strAddress, CDiagnostic* pDiagnostic = NULL) { return FALSE; }

	virtual BOOL Html2Mime (CMapiMessage& msgMapi) { return FALSE; }

	virtual BOOL SendMail 
		(
			CMapiMessage& msgMapi,
			BOOL* pbRequestDeliveryNotification = NULL,
			BOOL* pbRequestReadNotification = NULL,
			CDiagnostic* pDiagnostic = NULL
		) 
		{ return FALSE; }


	virtual BOOL SendAsAttachments
		(
			CStringArray& arAttachmentsFiles, 
			CStringArray& arAttachmentsTitles, 
			CDiagnostic* pDiagnostic = NULL
		)		
		{ return FALSE; }

	MailConnectorParams* GetParams	();

	virtual CString FormatFaxAddress	(CString sFax)	{ return sFax; }

	virtual BOOL IsPostaLiteEnabled		()				{ return FALSE; }
	virtual BOOL PostaLiteEnqueueMsg	(CMapiMessage*, SqlSession*, CDiagnostic* = NULL) { return FALSE; }
	virtual BOOL PostaLitePdfMargins	(CRect&) { return FALSE; }

	virtual BOOL ShowSendPostaLiteDlg	(CDocument*, CMapiMessage&)	{ return FALSE; }

	virtual DataEnum GetPostaLiteDefaultDeliveryType	() { return DataEnum(E_POSTALITE_DELIVERY_TYPE_DEFAULT); }
	virtual DataEnum GetPostaLiteDefaultPrintType		() { return DataEnum(E_POSTALITE_PRINT_TYPE_DEFAULT); }
	
	virtual BOOL RotateLandscape						() { return FALSE; }

protected:
	MailConnectorParams* m_pParams;
};

//=============================================================================
TB_EXPORT IMailConnector* AfxGetIMailConnector();

TB_EXPORT void CStringArray_ConcatAttachmentsWithTitle (const CStringArray& ar, const CStringArray& arTitle, CString& str);
TB_EXPORT void CStringArray_SplitAttachmentsWithTitle (CStringArray& ar, CStringArray& arTitle, const CString& str);

//=============================================================================
class TB_EXPORT CCompanyAddressInfo
{
public:
	DataStr		f_CompanyName;
	DataStr		f_TaxIdNumber;
	DataStr		f_FiscalCode;
	DataStr		f_Address;
	DataStr		f_ZIPCode;
	DataStr		f_City;
	DataStr		f_County;
	DataStr		f_Country;
	DataStr		f_Telephone1;
	DataStr		f_Fax;
	DataStr		f_EMail;
public:
	CCompanyAddressInfo() {}
};

//=============================================================================
typedef BOOL(FGetCompanyAddressInfos)(CCompanyAddressInfo*);

class TB_EXPORT PostaLiteStaticMethods
{
public:
	static DataStr RemoveInvalidCharsFromTelephone(CString strNumber);
	static BOOL IsValidEmailAddress(CString emailAddress);
	static BOOL TelephoneNumberHasInvalidChars(CString strNumber);
	static BOOL IsValidTaxIdNumber(DataStr strTaxIdNumber);
	static BOOL IsValidFiscalCode(DataStr strFiscalCode);

	static FGetCompanyAddressInfos* s_pfGetCompanyInfos;
};

/////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
