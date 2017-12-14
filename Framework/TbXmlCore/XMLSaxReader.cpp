#include "stdafx.h" 

#include <comdef.h>

#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\Diagnostic.h>

#include "XMLSaxReader.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szLexicalHanlderProperty[]	= _T("http://xml.org/sax/properties/lexical-handler");
static const TCHAR szParseFileBannerMessage[]	= _T("The parsing of the file\r\n%s\r\n has the following errors:"); 
static const TCHAR szSysErrorMessage[]			= _T("The system has thrown the following error: %s"); 
static const TCHAR szAttributeNameError[]		= _T("Error reading a tag attribute: the name is not available. Attribute skipped."); 
static const TCHAR szAttributeValueError[]		= _T("Error parsing value related to attribute %s.");
static const TCHAR szSkippedEntityInfo[]		= _T("There is the following skipped entity %s:\n found in file %s.");
static const TCHAR szNotRequiredRootWarning[]	= _T("Root of the file %s does not match.\r\nTag found is %s, tag desidered is %s. File skipped.");
static const TCHAR szCoCreateInstanceError[]	= _T("Failed to initialize CXMLSaxReader: CoCreateInstance failure with error %d.");
static const TCHAR szPutContentHandlerError[]	= _T("Failed to initialize and attach CXMLSaxContentHandler on CXMLSaxReader object: link failure.");
static const TCHAR szputLexicalHandlerError[]	= _T("Failed to initialize and attach CXMLSaxLexicalHandler on CXMLSaxReader object: link failure.");
static const TCHAR szPutErrorHandlerError[]		= _T("Failed to initialize and attach CXMLSaxErrorHandler on CXMLSaxReader object: link failure.");
static const TCHAR szAttachNullContentError[]	= _T("CXMLSaxReader::AttachContent method invoked with NULL content parameter.");
static const TCHAR szAttachBeforeInitError[]	= _T("CXMLSaxReader::AttachContent method invoked before CXMLSaxReader::Initialize method.");

/////////////////////////////////////////////////////////////////////////////
// CXMLXPathSyntax
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC (CXMLXPathSyntax, CObject)

//----------------------------------------------------------------------------
CXMLXPathSyntax::CXMLXPathSyntax ()
{
	szTagSeparator = _T("/");
}

//----------------------------------------------------------------------------
const TCHAR* CXMLXPathSyntax::GetTagSeparator () const
{
	return szTagSeparator;
}

//----------------------------------------------------------------------------
LPCTSTR CXMLXPathSyntax::GetInitQuery() const
{
	return szTagSeparator;
}

//----------------------------------------------------------------------------
const CString CXMLXPathSyntax::AddTag (const CString& sCurrentQuery, const CString& sTag) const
{
	if (sCurrentQuery.IsEmpty ())
		return (szTagSeparator + sTag);

	// checks if separator is already present
	if (_tcsicmp(sCurrentQuery.Right (1), szTagSeparator) == 0)
		return sCurrentQuery + sTag;

	return sCurrentQuery + (szTagSeparator + sTag);
}

//----------------------------------------------------------------------------
const CString CXMLXPathSyntax::RemoveLastTag (const CString& sCurrentQuery, const CString& sTag) const 
{
	CString sLastChars = sCurrentQuery.Right (sTag.GetLength() + 1);

	if	(
			_tcsicmp((LPCTSTR) sLastChars, (LPCTSTR)(szTagSeparator + sTag)) == 0 ||
			_tcsicmp((LPCTSTR) sLastChars, (LPCTSTR)(sTag + szTagSeparator)) == 0
		)
		return sCurrentQuery.Left (sCurrentQuery.GetLength() - sLastChars.GetLength()); 

	return sCurrentQuery;
}

//----------------------------------------------------------------------------
const BOOL CXMLXPathSyntax::IsThisRoot (const CString& sCurrentQuery) const
{
	return sCurrentQuery.Compare(szTagSeparator) == 0;
}

//----------------------------------------------------------------------------
const BOOL CXMLXPathSyntax::IsThisAChild (const CString& sChildQuery, const CString& sCurrentQuery) const
{
	if (sCurrentQuery.GetLength () <= sChildQuery.GetLength())
		return FALSE;

	return _tcsicmp((LPCTSTR) sCurrentQuery.Left (sChildQuery.GetLength()), (LPCTSTR)sChildQuery) == 0;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxErrorHandler
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
CXMLSaxErrorHandler::CXMLSaxErrorHandler (CXMLSaxContentHandler* pContentHandler)
	:
	m_RefCount			(0),
	m_pContentHandler	(pContentHandler)
{
	if (!pContentHandler)
		ASSERT (FALSE);
}

//----------------------------------------------------------------------------
CXMLSaxErrorHandler::~CXMLSaxErrorHandler ()
{
	m_pContentHandler = NULL;
}

//----------------------------------------------------------------------------
long __stdcall CXMLSaxErrorHandler::QueryInterface(const struct _GUID &riid, void ** ppObj)
{
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IUnknown*>(this);
	}
	if (riid == __uuidof(ISAXErrorHandler))
	{
		*ppObj = static_cast<ISAXErrorHandler*>(this);
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
	}

	AddRef() ;

	return S_OK;
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxErrorHandler::AddRef ()
{
	return InterlockedIncrement(&m_RefCount);
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxErrorHandler::Release ()
{
	long nRefCount = 0;

	nRefCount = InterlockedDecrement(&m_RefCount) ;
	
	return nRefCount;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxErrorHandler::error 
									(
										/* [in] */ ISAXLocator *pLocator,
										/* [in] */ const TCHAR *pwchErrorMessage,
										/* [in] */ HRESULT hrErrorCode
									) 
{
	CString sMessage (pwchErrorMessage);
	if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
		m_pContentHandler->GetDiagnostic()->AddError (sMessage);

	return E_FAIL;
}
  
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxErrorHandler::fatalError
									( 
										/* [in] */ ISAXLocator *pLocator,
										/* [in] */ const TCHAR *pwchErrorMessage,
										/* [in] */ HRESULT hrErrorCode
									)
{
	CString sMessage (pwchErrorMessage);
	if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
		m_pContentHandler->GetDiagnostic()->AddError (sMessage);

	return E_FAIL;
}
        
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxErrorHandler::ignorableWarning
									( 
										/* [in] */ ISAXLocator *pLocator,
										/* [in] */ const TCHAR *pwchErrorMessage,
										/* [in] */ HRESULT hrErrorCode
									)
{
	CString sMessage (pwchErrorMessage);
	if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
		m_pContentHandler->GetDiagnostic()->AddWarning(sMessage);

	return S_OK;
}    

/////////////////////////////////////////////////////////////////////////////
// CXMLBaseSaxContent
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLBaseSaxContent, CObject)

//----------------------------------------------------------------------------
CXMLBaseSaxContent::CXMLBaseSaxContent ()
{
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContentFunction
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxContentFunction, CObject)

//----------------------------------------------------------------------------
CXMLSaxContentFunction::CXMLSaxContentFunction (const CString& sKey, ProtoType aProtoType)
{
	m_sKey		= sKey;
	m_ProtoType	= aProtoType;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContentAttribFunction
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxContentAttribFunction, CXMLSaxContentFunction)

//----------------------------------------------------------------------------
CXMLSaxContentAttribFunction::CXMLSaxContentAttribFunction (const CString& sKey, SAX_ATTR_FUNCTION pFunction)
	:
	CXMLSaxContentFunction (sKey, CXMLSaxContentFunction::ATTRIBUTE)
{
	m_pFunction = pFunction;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContentTagFunction
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxContentTagFunction, CXMLSaxContentFunction)

//----------------------------------------------------------------------------
CXMLSaxContentTagFunction::CXMLSaxContentTagFunction (const CString& sKey, SAX_TAG_FUNCTION pFunction)
	:
	CXMLSaxContentFunction (sKey, CXMLSaxContentFunction::TAG)
{
	m_pFunction = pFunction;
}


/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContentBindings
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxContentBindings, CObArray)

//----------------------------------------------------------------------------
CXMLSaxContentBindings::CXMLSaxContentBindings ()
{
}

//----------------------------------------------------------------------------
CXMLSaxContentBindings::~CXMLSaxContentBindings ()
{
	RemoveAll ();
}

//----------------------------------------------------------------------------
void CXMLSaxContentBindings::RemoveAt (int nIndex, int nCount /*= 1*/)
{
	int j = nCount;
	for (int i = nIndex; (i < GetSize()) && (j-- > 0); i++)
	{
		CObject* pO = GetAt(i);
		if (pO) 
		{
			ASSERT_VALID(pO);
			delete pO;
		}
	}
	CObArray::RemoveAt(nIndex, nCount);
}

//----------------------------------------------------------------------------
void CXMLSaxContentBindings::RemoveAll	()
{
	for (int i = 0; i < GetSize(); i++) 
	{
		CObject* pO = GetAt(i);
		if (pO) 
		{
			ASSERT_VALID(pO);
			delete pO;
		}
	}
	CObArray::RemoveAll();
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContentBindings::HasKey	(const CString& sKey)
{
	LPCTSTR pKey = (LPCTSTR) sKey;
	for (int i = 0; i < GetSize(); i++) 
	{
		CXMLSaxContentFunction* pBinding = (CXMLSaxContentFunction*) GetAt(i);

		if (_tcsicmp((LPCTSTR) pBinding->m_sKey, pKey) == 0)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
SAX_ATTR_FUNCTION CXMLSaxContentBindings::GetAttributeFunction (const CString& sKey)
{
	LPCTSTR pKey = (LPCTSTR) sKey;

	for (int i = 0; i < GetSize(); i++) 
	{
		CXMLSaxContentFunction* pBinding = (CXMLSaxContentFunction*) GetAt(i);

		if	(
				pBinding->m_ProtoType == CXMLSaxContentFunction::ATTRIBUTE && 
				_tcsicmp((LPCTSTR) pBinding->m_sKey, pKey) == 0 
				
			)
			return ((CXMLSaxContentAttribFunction*) pBinding)->m_pFunction;
	}

	return NULL;
}

//----------------------------------------------------------------------------
SAX_TAG_FUNCTION CXMLSaxContentBindings::GetTagFunction (const CString& sKey)
{
	LPCTSTR pKey = (LPCTSTR) sKey;

	for (int i = 0; i < GetSize(); i++) 
	{
		CXMLSaxContentFunction* pBinding = (CXMLSaxContentFunction*) GetAt(i);

		if	(
				pBinding->m_ProtoType == CXMLSaxContentFunction::TAG &&
				_tcsicmp((LPCTSTR) pBinding->m_sKey, pKey) == 0 
				
			)
			return ((CXMLSaxContentTagFunction*) pBinding)->m_pFunction;
	}

	return NULL;
}

//========================================================================
// XMLSaxContentAttributes Declaration
//========================================================================
IMPLEMENT_DYNAMIC(CXMLSaxContentAttributes, CMapStringToString)

//----------------------------------------------------------------------------
CXMLSaxContentAttributes::CXMLSaxContentAttributes ()
{
}

//----------------------------------------------------------------------------
CXMLSaxContentAttributes::~CXMLSaxContentAttributes ()
{
	RemoveAll ();
}

//----------------------------------------------------------------------------
void CXMLSaxContentAttributes::SetAttributes (ISAXAttributes *pAttributes, CXMLSaxDiagnostic* pDiagnostic)
{
	RemoveAll ();

	if (!pAttributes)
		return;

	int nSize = 0;
	HRESULT hResult = pAttributes->getLength(&nSize);
	if (FAILED(hResult) || !nSize)
		return;
	
	int nNameLen;
	int nValueLen;

	for (int i=0; i < nSize; i++)
	{
		TCHAR* szAttrName = NULL;
		TCHAR* szAttrValue = NULL;
		nNameLen = 0;
		nValueLen = 0;

		// local name and value of the current attribute
		//hResult = pAttributes->getLocalName(i,  (const TCHAR**) &szAttrName, &nNameLen);
		hResult = pAttributes->getQName(i,  (const TCHAR**) &szAttrName, &nNameLen);

		if (FAILED(hResult) || nNameLen <= 0)
		{
			if (pDiagnostic)
				pDiagnostic->AddError (CXMLSaxDiagnostic::attributeError());
			continue;
		}
		CString sName (szAttrName, nNameLen);

		hResult = pAttributes->getValue	 (i, (const TCHAR**) &szAttrValue,	&nValueLen);
		if (FAILED(hResult))
		{
			if (pDiagnostic)
				pDiagnostic->AddError (CXMLSaxDiagnostic::attributeError(szAttrName));
			continue;
		}

		if (nValueLen > 0)
		{
			CString sValue (szAttrValue, nValueLen);
			SetAt (sName, sValue);
		}
		else
			SetAt (sName, _T(""));

	}
}


//----------------------------------------------------------------------------
const CString CXMLSaxContentAttributes::GetAttributeByName (const CString& sName) const
{
	CString sValue;
	Lookup (sName, sValue);

	return sValue;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxLexicalHandler
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
CXMLSaxLexicalHandler::CXMLSaxLexicalHandler ()
	:
	m_RefCount(0) 
{
}

//----------------------------------------------------------------------------
CXMLSaxLexicalHandler::~CXMLSaxLexicalHandler ()
{
}

//----------------------------------------------------------------------------
long __stdcall CXMLSaxLexicalHandler::QueryInterface(const struct _GUID &riid, void ** ppObj)
{ 
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IUnknown*>(this);
	}
	if (riid == __uuidof(ISAXLexicalHandler))
	{
		*ppObj = static_cast<ISAXLexicalHandler*>(this);
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
	}

	AddRef() ;
	return S_OK;
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxLexicalHandler::AddRef ()
{
	return InterlockedIncrement(&m_RefCount);
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxLexicalHandler::Release ()
{
	long nRefCount = 0;
	nRefCount=InterlockedDecrement(&m_RefCount);
	

	return nRefCount;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::startDTD		
										( 
											/* [in] */ const wchar_t *pwchName,
											/* [in] */ int cchName,
											/* [in] */ const wchar_t *pwchPublicId,
											/* [in] */ int cchPublicId,
											/* [in] */ const wchar_t *pwchSystemId,
											/* [in] */ int cchSystemId
										)
{
	return S_OK;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::endDTD	()
{
	return S_OK;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::startEntity
										( 
											/* [in] */ const wchar_t *pwchName,
											/* [in] */ int cchName
										)
{
	return S_OK;
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::endEntity
										( 
											/* [in] */ const wchar_t *pwchName,
											/* [in] */ int cchName
										)
{
	return S_OK;
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::startCDATA	()
{
	return S_OK;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::endCDATA ()
{
	return S_OK;
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxLexicalHandler::comment
										( 
											/* [in] */ const wchar_t *pwchChars,
											/* [in] */ int cchChars
										)
{
	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContentHandler
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
CXMLSaxContentHandler::CXMLSaxContentHandler ()
	:
	m_RefCount(0) 
{
	m_pBindedContent = NULL;
}

//----------------------------------------------------------------------------
CXMLSaxContentHandler::~CXMLSaxContentHandler ()
{
	m_pBindedContent = NULL;
}

//----------------------------------------------------------------------------
long __stdcall CXMLSaxContentHandler::QueryInterface(const struct _GUID &riid, void ** ppObj)
{ 
	if (riid == IID_IUnknown)
	{
		*ppObj = static_cast<IUnknown*>(this);
	}
	if (riid == __uuidof(ISAXContentHandler))
	{
		*ppObj = static_cast<ISAXContentHandler*>(this);
	}
	else
	{
		*ppObj = NULL ;
		return E_NOINTERFACE ;
	}

	AddRef() ;
	return S_OK;
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxContentHandler::AddRef ()
{
	return InterlockedIncrement(&m_RefCount);
}

//----------------------------------------------------------------------------
unsigned long __stdcall CXMLSaxContentHandler::Release ()
{
	long nRefCount=0;
	nRefCount=InterlockedDecrement(&m_RefCount) ;

	return nRefCount;
}

//----------------------------------------------------------------------------
void CXMLSaxContentHandler::AttachContent (CXMLSaxContent* pContent)
{ 
	m_pBindedContent = pContent; 
}

//----------------------------------------------------------------------------
void CXMLSaxContentHandler::SetFileName	(const CString& sFileName)
{ 
	if (m_pBindedContent)
		m_pBindedContent->SetFileName (sFileName); 
}

//----------------------------------------------------------------------------
const CString CXMLSaxContentHandler::GetFileName () const
{ 
	if (m_pBindedContent)
		return m_pBindedContent->GetFileName ();
	
	ASSERT (FALSE);

	return CString ();
}

//----------------------------------------------------------------------------
CXMLSaxDiagnostic*	CXMLSaxContentHandler::GetDiagnostic ()
{
	if (m_pBindedContent)
		return m_pBindedContent->GetDiagnostic();

	return NULL;
}

//----------------------------------------------------------------------------
void CXMLSaxContentHandler::DetachContent ()
{ 
	m_pBindedContent = NULL; 
}

//----------------------------------------------------------------------------
void CXMLSaxContentHandler::BindSaxContentData ()
{ 
	if (!m_pBindedContent)
	{
		ASSERT (FALSE);
		TRACE ("CXMLSaxContentHandler::BindSaxContentData called with NULL SaxContent"); 
	}
	m_pBindedContent->BindParseFunctions ();
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::putDocumentLocator (/*[in]*/ ISAXLocator *pLocator) 
{ 
	return S_OK; 
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::startDocument ()  
{
	if (m_pBindedContent && m_pBindedContent->StartDocument ())
		return S_OK;

	return E_FAIL;
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::endDocument ()
{ 
	if (m_pBindedContent && m_pBindedContent->EndDocument ())
		return S_OK;

	return E_FAIL;
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::startPrefixMapping
													( 
														/* [in] */ const TCHAR *pwchPrefix,
														/* [in] */ int cchPrefix,
														/* [in] */ const TCHAR *pwchUri,
														/* [in] */ int cchUri
													)
{ 
	return S_OK; 
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::endPrefixMapping
													( 
														/* [in] */ const TCHAR *pwchPrefix,
														/* [in] */ int cchPrefix
													) 
{ 
	return S_OK; 
}
        
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::characters	
													( 
														/* [in] */ const TCHAR *pwchChars,
														/* [in] */ int cchChars
													)  
{ 
	// tag values
	if (cchChars > 0) 
		m_pBindedContent->m_sCurrentTagText = CString (pwchChars, cchChars);
	else
		m_pBindedContent->m_sCurrentTagText.Empty ();

	return S_OK; 
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::ignorableWhitespace
												( 
													/* [in] */ const TCHAR *pwchChars,
													/* [in] */ int cchChars
												) 
{ 
	return S_OK; 
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::processingInstruction
											( 
												/* [in] */ const TCHAR *pwchTarget,
												/* [in] */ int cchTarget,
												/* [in] */ const TCHAR *pwchData,
												/* [in] */ int cchData
											)  
{ 
	return S_OK; 
}
    
//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::skippedEntity		
											( 
												/* [in] */ const TCHAR *pwchName,
												/* [in] */ int cchName
											)  
{
	CString aName (pwchName, cchName);

	if (m_pBindedContent)
		m_pBindedContent->AddWarning (CXMLSaxDiagnostic::skippedEntityInfo (aName, GetFileName ()));

	return S_OK; 
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE CXMLSaxContentHandler::startElement	
											( 
												/* [in] */ const TCHAR *pchNamespaceUri,
												/* [in] */ int cchNamespaceUri,
												/* [in] */ const TCHAR *pwchLocalName,
												/* [in] */ int cchLocalName,
												/* [in] */ const TCHAR *pwchQName,
												/* [in] */ int cchQName,
												/* [in] */ ISAXAttributes *pAttributes
											)  
{ 
	// the content class
	if (!m_pBindedContent)
		return S_OK;

	CString sUri		(pchNamespaceUri);
	CString sTagName	(pwchLocalName, cchLocalName);

	// I put the attributes in a known structure and virtual call to childs
	m_pBindedContent->SetAttributes (pAttributes);
	BOOL bOk = m_pBindedContent->StartElement (pchNamespaceUri, sTagName);
	m_pBindedContent->ClearAttributes ();
	
	return bOk ? S_OK : E_FAIL; 
}

//----------------------------------------------------------------------------
HRESULT STDMETHODCALLTYPE  CXMLSaxContentHandler::endElement
												( 
													/* [in] */ const TCHAR *pchNamespaceUri,
													/* [in] */ int cchNamespaceUri,
													/* [in] */ const TCHAR *pwchLocalName,
													/* [in] */ int cchLocalName,
													/* [in] */ const TCHAR *pwchQName,
													/* [in] */ int cchQName
												)
{
	if (!m_pBindedContent)
		return S_OK;

	CString sUri		(pchNamespaceUri);
	CString sTagName	(pwchLocalName, cchLocalName);

	BOOL bOk = m_pBindedContent->EndElement (sUri, sTagName);

	return bOk ? S_OK : E_FAIL; 
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxDiagnostic
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxDiagnostic, CObject)

//----------------------------------------------------------------------------
CXMLSaxDiagnostic::CXMLSaxDiagnostic ()
	:
	m_bHasErrors	(FALSE),
	m_bHasWarnings	(FALSE),
	m_bEnabled		(TRUE)
{
}

//----------------------------------------------------------------------------
CXMLSaxDiagnostic::~CXMLSaxDiagnostic ()
{
}

//----------------------------------------------------------------------------
void CXMLSaxDiagnostic::SetFileName	(const CString& sFileName)
{
	m_sFileName = sFileName;
}

//----------------------------------------------------------------------------
const CString& CXMLSaxDiagnostic::GetFileName () const
{
	return m_sFileName;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxDiagnostic::HasWarnings () const
{
	// AfxGetDiagnostic()->WarningFound() could be fake as
	// it could contain errors of another object. I have
	// to consider only this session problems.
	return m_bHasWarnings;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxDiagnostic::HasErrors () const
{
	// AfxGetDiagnostic()->ErrorFound() could be fake as
	// it could contain errors of another object. I have
	// to consider only this session problems.
	return m_bHasErrors; 
}

//----------------------------------------------------------------------------
BOOL CXMLSaxDiagnostic::HasDiagnostic () const
{
	return m_bHasErrors || m_bHasWarnings;
}

//----------------------------------------------------------------------------
void CXMLSaxDiagnostic::ClearMessages ()
{
	m_bHasErrors	= FALSE;
	m_bHasWarnings	= FALSE;
}

//----------------------------------------------------------------------------
void CXMLSaxDiagnostic::AddWarning	(const CString& sMessage)
{
	if (!m_bEnabled)
		return;

	// first line is file name
	if (!AfxGetDiagnostic()->ErrorFound() && !AfxGetDiagnostic()->WarningFound())
		AfxGetDiagnostic()->Add (CXMLSaxDiagnostic::parsingFileBanner(), CDiagnostic::Info);

	AfxGetDiagnostic()->Add (sMessage, CDiagnostic::Warning);
	m_bHasWarnings = TRUE;
}

//----------------------------------------------------------------------------
void CXMLSaxDiagnostic::AddError (const CString& sMessage)
{
	if (!m_bEnabled)
		return;

	// first line is file name
	if (!AfxGetDiagnostic()->ErrorFound() && !AfxGetDiagnostic()->WarningFound())
		AfxGetDiagnostic()->Add (CXMLSaxDiagnostic::parsingFileBanner(), CDiagnostic::Info);

	AfxGetDiagnostic()->Add (sMessage, CDiagnostic::Error);
	m_bHasErrors = TRUE;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::coCreateInstanceError (HRESULT hr)
{
	CString sMessage;
	sMessage.Format (szCoCreateInstanceError, hr);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::parsingFileBanner ()
{
	CString sMessage;
	sMessage.Format (szParseFileBannerMessage, (LPCTSTR) GetFileName());
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::systemError (LPCTSTR pszError)
{
	CString sMessage;
	sMessage.Format (szSysErrorMessage, pszError);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::attributeError (LPCTSTR szName)
{
	CString sMessage;
	if (szName)
		sMessage.Format (szAttributeValueError, szName);
	else
		sMessage = szAttributeNameError;
	
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::skippedEntityInfo (LPCTSTR szName, const CString& sFileName)
{
	CString sMessage;
	sMessage.Format (szSkippedEntityInfo, szName,(LPCTSTR) sFileName);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::noRequiredRootError	(
															LPCTSTR szTagFound, 
															LPCTSTR szTagRequired, 
															const CString& sFileName
														)
{
	CString sMessage;
	sMessage.Format (szNotRequiredRootWarning, (LPCTSTR) sFileName, szTagFound, szTagRequired);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::putContentHandlerError()
{
	CString sMessage;
	sMessage.Format (szPutContentHandlerError);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::putLexicalHandlerError ()
{
	CString sMessage;
	sMessage.Format (szputLexicalHandlerError);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::putErrorHandlerError ()
{
	CString sMessage;
	sMessage.Format (szPutErrorHandlerError);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::attachNullContentError()
{
	CString sMessage;
	sMessage.Format (szAttachNullContentError);
	return sMessage;
}

//----------------------------------------------------------------------------
const CString CXMLSaxDiagnostic::attachBeforeInitError	()
{
	CString sMessage;
	sMessage.Format (szAttachBeforeInitError);
	return sMessage;
}

/////////////////////////////////////////////////////////////////////////////
// CXMLSaxContent
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxContent, CXMLBaseSaxContent)

//----------------------------------------------------------------------------
CXMLSaxContent::CXMLSaxContent ()
{
	m_sCurrentKey.Empty ();
	
	ClearBindings ();

	m_Status = CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
const CXMLXPathSyntax CXMLSaxContent::m_KeySyntax;

//----------------------------------------------------------------------------
void CXMLSaxContent::ClearBindings ()
{
	m_Bindings.RemoveAll ();
}

//----------------------------------------------------------------------------
void CXMLSaxContent::BindParseFunction (const CString& sKey, SAX_ATTR_FUNCTION pFunctionPtr)
{
	CXMLSaxContentAttribFunction* pBinding = new CXMLSaxContentAttribFunction (sKey, pFunctionPtr);
	m_Bindings.Add (pBinding);
}

//----------------------------------------------------------------------------
void CXMLSaxContent::BindParseFunction (const CString& sKey, SAX_TAG_FUNCTION pFunctionPtr)
{
	CXMLSaxContentTagFunction* pBinding = new CXMLSaxContentTagFunction (sKey, pFunctionPtr);
	m_Bindings.Add (pBinding);
}

//----------------------------------------------------------------------------
void CXMLSaxContent::SetFileName (const CString& sFileName)
{ 
	m_Diagnostic.SetFileName (sFileName);
}


//----------------------------------------------------------------------------
const CString CXMLSaxContent::GetFileName () const
{
	return m_Diagnostic.GetFileName ();
}

//----------------------------------------------------------------------------
void CXMLSaxContent::SetAttributes (ISAXAttributes* pAttributes)
{ 
	m_arCurrentAttributes.SetAttributes (pAttributes, GetDiagnostic ());
}

//----------------------------------------------------------------------------
void CXMLSaxContent::ClearAttributes ()
{ 
	m_arCurrentAttributes.RemoveAll ();
}

//----------------------------------------------------------------------------
const int&	CXMLSaxContent::GetStatus () const
{
	return m_Status;
}

//----------------------------------------------------------------------------
CXMLSaxDiagnostic* CXMLSaxContent::GetDiagnostic ()
{
	return &m_Diagnostic;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::HasWarnings () const
{
	return m_Diagnostic.HasWarnings ();
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::HasErrors () const
{
	return m_Diagnostic.HasErrors ();
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::HasDiagnostic () const
{
	return m_Diagnostic.HasDiagnostic();
}

//----------------------------------------------------------------------------
void CXMLSaxContent::BindParseFunctions ()
{
	OnBindParseFunctions();
}

//----------------------------------------------------------------------------
void CXMLSaxContent::OnBindParseFunctions ()
{
}

//----------------------------------------------------------------------------
void CXMLSaxContent::ClearMessages ()
{
	m_Diagnostic.ClearMessages ();
}

//----------------------------------------------------------------------------
void CXMLSaxContent::AddWarning	(const CString& sMessage)
{
	m_Diagnostic.AddWarning (sMessage);
}

//----------------------------------------------------------------------------
void CXMLSaxContent::AddError (const CString& sMessage)
{
	m_Diagnostic.AddError (sMessage);
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::StartDocument	()
{
	m_sCurrentKey = m_KeySyntax.GetInitQuery();
	m_sChildsToSkip.Empty ();
	
	m_Status = CXMLSaxContent::OK;

	return OnStartDocument () == CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::EndDocument	()
{
	m_sCurrentKey = m_KeySyntax.GetInitQuery();
	m_sChildsToSkip.Empty ();

	return OnEndDocument () == CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::StartElement	(
										const CString& sUri, 
										const CString& sTagName
									)
{
	// known root check
	if (m_KeySyntax.IsThisRoot (m_sCurrentKey))
	{
		CString sRequiredRoot = OnGetRootTag ();
		if (!sRequiredRoot.IsEmpty () && _tcsicmp((LPCTSTR) sRequiredRoot, (LPCTSTR) sTagName) != 0)
		{
			AddError (CXMLSaxDiagnostic::noRequiredRootError (sTagName, sRequiredRoot, GetFileName ()));
			m_Status = CXMLSaxContent::ABORT;
			return FALSE;
		}
	}

    m_sCurrentKey = m_KeySyntax.AddTag (m_sCurrentKey, sTagName);
	
	// if the developer asked to skip the childs, I skip
	if (m_Status == SKIP_THE_CHILDS && m_KeySyntax.IsThisAChild (m_sChildsToSkip, m_sCurrentKey))
		return TRUE;
	
	// I call the element parsing
	int oldStatus = m_Status;
	
	m_Status = OnStartElement (m_sCurrentKey, sUri, m_arCurrentAttributes);

	if  (m_Status == CXMLSaxContent::ABORT)
		return FALSE;

	SAX_ATTR_FUNCTION pFunction = m_Bindings.GetAttributeFunction(m_sCurrentKey);
	if (pFunction)
		m_Status = (this->*(pFunction))(sUri, m_arCurrentAttributes);

	// the initial state of skipping the childs
	if (oldStatus == CXMLSaxContent::OK && m_Status == CXMLSaxContent::SKIP_THE_CHILDS)
	{
		m_sChildsToSkip = m_sCurrentKey;
	}

	return (m_Status != CXMLSaxContent::ABORT);
}

//----------------------------------------------------------------------------
BOOL CXMLSaxContent::EndElement		(
										const CString& sUri, 
										const CString& sTagName
									)
{
	// if the developer asked to skip the childs
	if (m_Status == SKIP_THE_CHILDS)
	{
		// I'm in the childs loop, I skip
		if (m_KeySyntax.IsThisAChild (m_sChildsToSkip, m_sCurrentKey))
		{
			m_sCurrentKey = m_KeySyntax.RemoveLastTag (m_sCurrentKey, sTagName);
			return TRUE;
		}
		else 
		{
			// I'm finishing the childs to skip
			if (_tcsicmp((LPCTSTR)m_sChildsToSkip, (LPCTSTR)m_sCurrentKey) == 0)
			{
				m_sChildsToSkip.Empty ();
				m_Status = CXMLSaxContent::OK;
			}
		}
	}

	m_Status = OnEndElement (m_sCurrentKey, sUri, m_sCurrentTagText);

	if (m_Status != CXMLSaxContent::OK)
		return FALSE;

	SAX_TAG_FUNCTION pFunction = m_Bindings.GetTagFunction(m_sCurrentKey);
	if (pFunction)
		m_Status = (this->*(pFunction))(sUri, m_sCurrentTagText);

	m_sCurrentKey = m_KeySyntax.RemoveLastTag (m_sCurrentKey, sTagName);

	return (m_Status != CXMLSaxContent::ABORT);
}

//----------------------------------------------------------------------------
CString CXMLSaxContent::OnGetRootTag () const
{
	ASSERT (FALSE);
	return m_sCurrentKey;
}

//----------------------------------------------------------------------------
int CXMLSaxContent::OnStartDocument ()
{
	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
int CXMLSaxContent::OnEndDocument ()
{
	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
int CXMLSaxContent::OnStartElement	(
										const CString& sKey, 
										const CString& sUri, 
										const CXMLSaxContentAttributes& arAttributes
									)
{
	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------
int CXMLSaxContent::OnEndElement	(
										const CString& sKey, 
										const CString& sUri, 
										const CString& sTagValue
									)
{
	return CXMLSaxContent::OK;
}


/////////////////////////////////////////////////////////////////////////////
// CXMLSaxReader
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLSaxReader, CObject)

//----------------------------------------------------------------------------
CXMLSaxReader::CXMLSaxReader(BOOL bFreeThreaded /*= FALSE*/)
	:
	m_pXMLReader		(NULL),
	m_pErrorHandler		(NULL),
	m_bInitialized		(FALSE),
	m_pContentHandler	(NULL),
	m_pLexicalHandler	(NULL)
{
	Initialize ();
}

//----------------------------------------------------------------------------
CXMLSaxReader::CXMLSaxReader (const CXMLSaxReader& XMLObj)
:
	m_pXMLReader		(NULL),
	m_pErrorHandler		(NULL),
	m_bInitialized		(FALSE),
	m_pContentHandler	(NULL),
	m_pLexicalHandler	(NULL)
{
	*this = XMLObj;
}

//----------------------------------------------------------------------------
CXMLSaxReader::~CXMLSaxReader()
{
	Close();
}


//----------------------------------------------------------------------------
CXMLSaxReader& CXMLSaxReader::operator = (const CXMLSaxReader& XMLObj)
{
	m_bInitialized		= XMLObj.m_bInitialized;
	Initialize ();

	return *this;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxReader::Initialize ()
{
	Close();

	HRESULT hr = CoCreateInstance
			(
				__uuidof(SAXXMLReader60), 
				NULL, 
				CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER, //CLSCTX_ALL, 
				__uuidof(ISAXXMLReader), 
				(void **)&m_pXMLReader
			);


   if (FAILED(hr) || !m_pXMLReader)
    {
		Close();

		if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
			m_pContentHandler->GetDiagnostic()->AddError (CXMLSaxDiagnostic::coCreateInstanceError (hr));
		
		return FALSE;
    }

   	if (!m_pContentHandler)
		m_pContentHandler = new CXMLSaxContentHandler ();

	if (m_pContentHandler)
		hr = m_pXMLReader->putContentHandler(m_pContentHandler);

	if (FAILED(hr) || !m_pContentHandler)
    {
		Close();

		if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
			m_pContentHandler->GetDiagnostic()->AddError (CXMLSaxDiagnostic::putContentHandlerError());
		
		return FALSE;
    }

	// lexical
	if (!m_pLexicalHandler)
		m_pLexicalHandler = new CXMLSaxLexicalHandler ();
	
	hr = m_pXMLReader->putProperty (szLexicalHanlderProperty, _variant_t (m_pLexicalHandler));

	if (FAILED(hr))
    {
		Close();

		if (m_pLexicalHandler && m_pContentHandler->GetDiagnostic())
			m_pContentHandler->GetDiagnostic()->AddError (CXMLSaxDiagnostic::putLexicalHandlerError());
		
		return FALSE;
    }

// link to error handler
	if (!m_pErrorHandler)
		m_pErrorHandler = new CXMLSaxErrorHandler (m_pContentHandler);

	hr = m_pXMLReader->putErrorHandler(m_pErrorHandler);

	if (FAILED(hr))
    {
		Close();

		if (m_pContentHandler && m_pContentHandler->GetDiagnostic())
			m_pContentHandler->GetDiagnostic()->AddError (CXMLSaxDiagnostic::putErrorHandlerError());
		
		return FALSE;
    }

	m_bInitialized = TRUE;
	return m_bInitialized;
}

//----------------------------------------------------------------------------
void CXMLSaxReader::Close()
{
	m_bInitialized = FALSE;

	if (m_pErrorHandler)
	{
		m_pErrorHandler->Release ();
		delete m_pErrorHandler;
	}

	m_pErrorHandler = NULL; 

	if (m_pLexicalHandler)
	{
		m_pLexicalHandler->Release ();
		delete m_pLexicalHandler;
		m_pLexicalHandler = NULL;
	}

	if (m_pContentHandler)
	{
		m_pContentHandler->DetachContent ();
		m_pContentHandler->Release ();
		delete m_pContentHandler;
		m_pContentHandler = NULL;
	}

	m_pXMLReader = NULL;
}

//----------------------------------------------------------------------------
CString CXMLSaxReader::GetFileName () const
{
	return m_pContentHandler ? m_pContentHandler->GetFileName() : CString();
}

//----------------------------------------------------------------------------
void CXMLSaxReader::AttachContent (CXMLSaxContent* pContent)
{
	if (!pContent)
	{
		ASSERT (FALSE);
		AfxGetDiagnostic()->Add(CXMLSaxDiagnostic::attachNullContentError(), CDiagnostic::Warning);
		return;
	}

	if (!m_bInitialized || !m_pXMLReader)
	{
		ASSERT (FALSE);
		pContent->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::attachBeforeInitError());
		return;
	}

	if (!m_pContentHandler)
		m_pContentHandler = new CXMLSaxContentHandler ();

	m_pContentHandler->AttachContent (pContent);

	HRESULT hr = m_pXMLReader->putContentHandler(m_pContentHandler);

	if (FAILED(hr) || !pContent)
    {
		Close();
		ASSERT (FALSE);
		pContent->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::putContentHandlerError());
    }	
}

//----------------------------------------------------------------------------
void CXMLSaxReader::DetachContent ()
{
	if (m_pContentHandler)
		m_pContentHandler->DetachContent ();
}

//----------------------------------------------------------------------------
BOOL CXMLSaxReader::ReadFile (const CString& sFileName)
{
	if (!m_bInitialized || !m_pContentHandler || !m_pContentHandler->GetSaxContent())
		return FALSE;

	BOOL bOk =  FALSE;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(sFileName))
	{
		CString sFileContent = pFileSystemManager->GetTextFile (sFileName);
		if (!sFileContent.IsEmpty ())
			bOk = Read (sFileContent, (LPCTSTR) sFileName);

		return bOk;
	}

	CString sMessage;
	LPVOID	lpMsgBuf;

	// links bindings to data
	m_pContentHandler->BindSaxContentData	();
	m_pContentHandler->SetFileName			(sFileName);

	CString aFileName = sFileName;
	try
	{
		HRESULT hr = m_pXMLReader->parseURL((LPCTSTR)aFileName);
		if (FAILED(hr))
		{
			FormatMessage
			( 
				FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL,
				GetLastError(),
				MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
				(LPTSTR) &lpMsgBuf,
				0,
				NULL
			);
			if (m_pContentHandler->GetDiagnostic ())
				m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) lpMsgBuf));

			return FALSE;
		}
	}
	// includes COleExceptions.
	catch (CException* e) 
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);

		if (m_pContentHandler->GetDiagnostic ())
			m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) szError));

		e->Delete();
		
		return FALSE;
	}
	catch (...)
	{
		LPVOID lpMsgBuf;
		FormatMessage
		( 
			FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL,
			GetLastError(),
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
			(LPTSTR) &lpMsgBuf,
			0,
			NULL
		);

		if (m_pContentHandler->GetDiagnostic ())
			m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) lpMsgBuf));
		
		return FALSE;
	}

	// errors or aborted
	if (
			m_pContentHandler->GetSaxContent()->HasErrors () ||
			m_pContentHandler->GetSaxContent()->GetStatus () == CXMLSaxContent::ABORT
		)
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLSaxReader::Read (const CString& sDoc, LPCTSTR pszFileName /*NULL*/)
{
	if (!m_bInitialized || !m_pContentHandler || !m_pContentHandler->GetSaxContent())
		return FALSE;

	CString sMessage;
	LPVOID	lpMsgBuf;

	// links bindings to data
	m_pContentHandler->BindSaxContentData	();
	m_pContentHandler->SetFileName			(pszFileName ? pszFileName : _T(""));
	
	m_pContentHandler->GetDiagnostic ()->ClearMessages ();
	CString aDoc = sDoc;

	try
	{
		VARIANT vDoc; 
		vDoc.vt = VT_BSTR; 
		vDoc.bstrVal = aDoc.AllocSysString();
		HRESULT hr = m_pXMLReader->parse(vDoc);
		aDoc.ReleaseBuffer ();
		if (FAILED(hr))
		{
			FormatMessage
			( 
				FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL,
				GetLastError(),
				MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
				(LPTSTR) &lpMsgBuf,
				0,
				NULL
			);
			if (m_pContentHandler->GetDiagnostic ())
				m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) lpMsgBuf));

			return FALSE;
		}
	}
	// includes COleExceptions.
	catch (CException* e) 
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);

		if (m_pContentHandler->GetDiagnostic ())
			m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) szError));

		e->Delete();
		
		return FALSE;
	}
	catch (...)
	{
		LPVOID lpMsgBuf;
		FormatMessage
		( 
			FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL,
			GetLastError(),
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
			(LPTSTR) &lpMsgBuf,
			0,
			NULL
		);

		if (m_pContentHandler->GetDiagnostic ())
			m_pContentHandler->GetDiagnostic ()->AddError (CXMLSaxDiagnostic::systemError((LPCTSTR) lpMsgBuf));
		
		return FALSE;
	}

	// errors or aborted
	if (
			m_pContentHandler->GetSaxContent()->HasErrors () ||
			m_pContentHandler->GetSaxContent()->GetStatus () == CXMLSaxContent::ABORT
		)
		return FALSE;

	return TRUE;
}
