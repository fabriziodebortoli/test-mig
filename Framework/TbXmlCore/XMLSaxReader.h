#pragma once 

#include <MSXML6.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////
// CXMLSaxReader	is the class that implements ReadFile methods.
//					It needs a CXMLSaxContent object to perform file parsing
//
// CXMLSaxContent	is the class that must be implemented by developer to
//				    catch parsing events. The class must override the
//					OnBindParseFunctions () method and use BIND_PARSE_XXX
//				    macro to bind class functions with parsing events.
//					The BIND_PARSE_XXX macro uses a binding key that is
//					exactly the XPath query of the grammar.
//
//					CXMLSaxContent class implements the ReturnValue enum 
//					data that is used to continue or abort the file reading.
//					ReturnValue::OK to continue the parsing
//					ReturnValue::ABORT to abort Sax parsing.
//					ReturnValue::SKIP_THE_CHILDS to skip childs nodes.
//					
//					CXMLSaxContent class implements AddError and AddWarning
//					to manage diagnostic and it stores until the end of file
//					CDiagnostic messages are not shown by default, the client
//					has the task to show diagnostic messages
//
// Example of CXMLSaxContent implementation:
//
// class CMyContent : public CXMLSaxContent
// {
//	public:
//		CApplicationConfigContent (CApplicationConfigInfo* pConfigInfo);
//
//	protected:
//		virtual CString	OnGetRootTag			() const
//		{
//			return _T("rootTag");
//		}
//
//		virtual void OnBindParseFunctions	()
//		{
//			...
//			BIND_PARSE_TAG		 (_T("/rootTag/myTag"),	ParseMyTag);
//			BIND_PARSE_ATTRIBUTES(_T("/rootTag/myTag"),	ParseMyTagAttributes);
//		}
//
//		int ParseMyTag			(const CString& sUri, const CString& sTagValue);
//		int ParseMyTagAttributes(const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
// };
//
// Example of use:
//
// BOOL ReadMyFile (CString sFileName)
// {
//		CMyContent aMyContent;
//		CXMLSaxReader aSaxReader.AttachContent(&aMyContent);
//		return m_SaxReader.ReadFile (sFileName);
// }
//////////////////////////////////////////////////////////////////////////

class CXMLSaxDiagnostic;
class CXMLSaxContentHandler;
class CXMLSaxContent;
class CDiagnostic;
//========================================================================
// CXMLXPathSyntax Declaration
//========================================================================
class CXMLXPathSyntax : public CObject
{
	DECLARE_DYNAMIC (CXMLXPathSyntax)

private:
	const TCHAR* szTagSeparator;

public:
	CXMLXPathSyntax ();

public:
	LPCTSTR			GetInitQuery	() const;
	const TCHAR*	GetTagSeparator () const;
	const BOOL		IsThisRoot		(const CString& sCurrentQuery) const;
	const BOOL		IsThisAChild	(const CString& sChildQuery, const CString& sCurrentQuery) const;

	const CString AddTag			(const CString& sCurrentQuery, const CString& sTag) const;
	const CString RemoveLastTag	(const CString& sCurrentQuery, const CString& sTag) const;
};

//========================================================================
// CXMLSaxErrorHandler Declaration
//========================================================================
class CXMLSaxErrorHandler : public ISAXErrorHandler  
{
private:
	long					m_RefCount;
	CXMLSaxContentHandler*	m_pContentHandler;


public:
	CXMLSaxErrorHandler				(CXMLSaxContentHandler* pContentHandler);
	virtual ~CXMLSaxErrorHandler	();

	// interface management
	long			__stdcall		QueryInterface		(const struct _GUID &riid, void ** ppObj);
	unsigned long	__stdcall		AddRef				();
	unsigned long	__stdcall		Release				();

	// error events
	virtual HRESULT STDMETHODCALLTYPE error				( 
														/* [in] */ ISAXLocator *pLocator,
														/* [in] */ const TCHAR *pwchErrorMessage,
														/* [in] */ HRESULT hrErrorCode
														);
    
    virtual HRESULT STDMETHODCALLTYPE fatalError		( 
														/* [in] */ ISAXLocator *pLocator,
														/* [in] */ const TCHAR *pwchErrorMessage,
														/* [in] */ HRESULT hrErrorCode
														);
    
    virtual HRESULT STDMETHODCALLTYPE ignorableWarning	( 
														/* [in] */ ISAXLocator *pLocator,
														/* [in] */ const TCHAR *pwchErrorMessage,
														/* [in] */ HRESULT hrErrorCode
														);
};

//========================================================================
// CXMLSaxContentAttributes Declaration
//========================================================================
class TB_EXPORT CXMLSaxContentAttributes : public CMapStringToString
{
	DECLARE_DYNCREATE(CXMLSaxContentAttributes)

public:
	CXMLSaxContentAttributes ();
	~CXMLSaxContentAttributes ();

public:
	const CString	GetAttributeByName	(const CString& sName) const;
	void			SetAttributes		(ISAXAttributes *pAttributes, CXMLSaxDiagnostic* pDiagnostic);
};

 //========================================================================
// CXMLBaseSaxContent Declaration
//========================================================================
class CXMLBaseSaxContent : public CObject
{
	DECLARE_DYNAMIC(CXMLBaseSaxContent)

public:
	CXMLBaseSaxContent ();
};

//========================================================================
// CXMLSaxContentBindedItem Declaration
//========================================================================
class CXMLSaxContentFunction : public CObject
{
	friend class CXMLSaxContentBindings; 

	DECLARE_DYNAMIC(CXMLSaxContentFunction)

public:
	enum ProtoType { ATTRIBUTE, TAG };

protected:
	CString		m_sKey;	
	ProtoType	m_ProtoType;

public:
	CXMLSaxContentFunction (const CString& sKey, ProtoType aProtoType);
};

typedef  int (CXMLBaseSaxContent::*SAX_ATTR_FUNCTION) (const CString& sUri, const CXMLSaxContentAttributes& arAttributes);
//========================================================================
class CXMLSaxContentAttribFunction : public CXMLSaxContentFunction
{
	friend class CXMLSaxContentBindings;

	DECLARE_DYNAMIC(CXMLSaxContentAttribFunction)

private:
	SAX_ATTR_FUNCTION	m_pFunction;

public:
	CXMLSaxContentAttribFunction (const CString& sKey, SAX_ATTR_FUNCTION pFunction);
};

typedef  int (CXMLBaseSaxContent::*SAX_TAG_FUNCTION)  (const CString& sUri, const CString& sTagValue);
//========================================================================
class CXMLSaxContentTagFunction : public CXMLSaxContentFunction
{
	friend class CXMLSaxContentBindings;

	DECLARE_DYNAMIC(CXMLSaxContentTagFunction)

private:
	SAX_TAG_FUNCTION	m_pFunction;

public:
	CXMLSaxContentTagFunction (const CString& sKey, SAX_TAG_FUNCTION pFunction);
};

//========================================================================
// CXMLSaxContentBindings Declaration
//========================================================================
class CXMLSaxContentBindings : public CObArray
{
	DECLARE_DYNAMIC(CXMLSaxContentBindings)

public:
	CXMLSaxContentBindings	();
	~CXMLSaxContentBindings ();

	CXMLSaxContentFunction* 	GetAt		(int nIndex)const	{ return (CXMLSaxContentFunction*) CObArray::GetAt(nIndex);	}
	CXMLSaxContentFunction*&	ElementAt	(int nIndex)		{ return (CXMLSaxContentFunction*&) CObArray::ElementAt(nIndex); }
	
	CXMLSaxContentFunction* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	CXMLSaxContentFunction*&	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	void		RemoveAt	(int nIndex, int nCount = 1);
	void		RemoveAll	();

	// keys management
	BOOL				HasKey					(const CString& sKey);
	SAX_ATTR_FUNCTION	GetAttributeFunction	(const CString& sKey);
	SAX_TAG_FUNCTION	GetTagFunction			(const CString& sKey);
};

//========================================================================
// CXMLSaxContentHandler Declaration
//========================================================================
class TB_EXPORT CXMLSaxContentHandler : public ISAXContentHandler  
{
	friend class CXMLSaxReader;

private:
	long					m_RefCount;
	CXMLSaxContent*			m_pBindedContent;

public:
	CXMLSaxContentHandler();
	virtual ~CXMLSaxContentHandler();

	// interface management
	long			__stdcall	QueryInterface	(const struct _GUID &riid, void ** ppObj);
	unsigned long	__stdcall	AddRef			();
	unsigned long	__stdcall	Release			();


public:
	// SaxContext management
	CXMLSaxContent* GetSaxContent () { return m_pBindedContent; }

	// SaxContext attaching methods
	void AttachContent	(CXMLSaxContent*);
	void DetachContent	();

	void BindSaxContentData	();

	// fileName
	void			SetFileName	(const CString& sFileName);
	const CString	GetFileName	() const;

	CXMLSaxDiagnostic*	GetDiagnostic	();

public:
	// events
    virtual HRESULT STDMETHODCALLTYPE putDocumentLocator		(/* [in] */ ISAXLocator *pLocator);
    virtual HRESULT STDMETHODCALLTYPE startDocument				();
    virtual HRESULT STDMETHODCALLTYPE endDocument				();
    
    virtual HRESULT STDMETHODCALLTYPE startPrefixMapping		( 
																	/* [in] */ const TCHAR *pwchPrefix,
																	/* [in] */ int cchPrefix,
																	/* [in] */ const TCHAR *pwchUri,
																	/* [in] */ int cchUri
																);
    
    virtual HRESULT STDMETHODCALLTYPE endPrefixMapping			( 
																	/* [in] */ const TCHAR *pwchPrefix,
																	/* [in] */ int cchPrefix
																);
    
    virtual HRESULT STDMETHODCALLTYPE startElement				( 
																	/* [in] */ const TCHAR *pchNamespaceUri,
																	/* [in] */ int cchNamespaceUri,
																	/* [in] */ const TCHAR *pwchLocalName,
																	/* [in] */ int cchLocalName,
																	/* [in] */ const TCHAR *pwchQName,
																	/* [in] */ int cchQName,
																	/* [in] */ ISAXAttributes *pAttributes
																);
    
    virtual HRESULT STDMETHODCALLTYPE endElement				( 
																	/* [in] */ const TCHAR *pchNamespaceUri,
																	/* [in] */ int cchNamespaceUri,
																	/* [in] */ const TCHAR *pwchLocalName,
																	/* [in] */ int cchLocalName,
																	/* [in] */ const TCHAR *pwchQName,
																	/* [in] */ int cchQName
																);
    
    virtual HRESULT STDMETHODCALLTYPE characters				( 
																	/* [in] */ const wchar_t *pwchChars,
																	/* [in] */ int cchChars
																);
    
    virtual HRESULT STDMETHODCALLTYPE ignorableWhitespace		( 
																	/* [in] */ const TCHAR *pwchChars,
																	/* [in] */ int cchChars
																);
    
    virtual HRESULT STDMETHODCALLTYPE processingInstruction		( 
																	/* [in] */ const TCHAR *pwchTarget,
																	/* [in] */ int cchTarget,
																	/* [in] */ const TCHAR *pwchData,
																	/* [in] */ int cchData
																);
    
    virtual HRESULT STDMETHODCALLTYPE skippedEntity				( 
																	/* [in] */ const TCHAR *pwchName,
																	/* [in] */ int cchName
																);
 };

//========================================================================
// CXMLSaxLexicalHandler Declaration
//========================================================================
class TB_EXPORT CXMLSaxLexicalHandler : public ISAXLexicalHandler
{
	friend class CXMLSaxReader;

private:
	long					m_RefCount;

public:
	CXMLSaxLexicalHandler();
	virtual ~CXMLSaxLexicalHandler();

	// interface management
	long			__stdcall	QueryInterface	(const struct _GUID &riid, void ** ppObj);
	unsigned long	__stdcall	AddRef			();
	unsigned long	__stdcall	Release			();

public:
	// events
    virtual HRESULT STDMETHODCALLTYPE startDTD		( 
														/* [in] */ const wchar_t *pwchName,
														/* [in] */ int cchName,
														/* [in] */ const wchar_t *pwchPublicId,
														/* [in] */ int cchPublicId,
														/* [in] */ const wchar_t *pwchSystemId,
														/* [in] */ int cchSystemId
													);
    
    virtual HRESULT STDMETHODCALLTYPE endDTD		();
    virtual HRESULT STDMETHODCALLTYPE startEntity	( 
														/* [in] */ const wchar_t *pwchName,
														/* [in] */ int cchName
													);
    
    virtual HRESULT STDMETHODCALLTYPE endEntity		( 
														/* [in] */ const wchar_t *pwchName,
														/* [in] */ int cchName
													);
    
    virtual HRESULT STDMETHODCALLTYPE startCDATA	();
    virtual HRESULT STDMETHODCALLTYPE endCDATA		();
    virtual HRESULT STDMETHODCALLTYPE comment		( 
														/* [in] */ const wchar_t *pwchChars,
														/* [in] */ int cchChars
													);
 };

 //========================================================================
// CXMLSaxDiagnostic Declaration
//========================================================================
class TB_EXPORT CXMLSaxDiagnostic : public CObject
{
	DECLARE_DYNAMIC(CXMLSaxDiagnostic)

private:
	CString		m_sFileName;
	BOOL		m_bHasErrors;
	BOOL		m_bHasWarnings;
	BOOL		m_bEnabled;

public:
	CXMLSaxDiagnostic ();
	~CXMLSaxDiagnostic ();

public:
	void			SetFileName	(const CString& sFileName);
	const CString&	GetFileName () const;

	void ClearMessages		();
	void AddWarning			(const CString& sMessage);
	void AddError			(const CString& sMessage);

	void	Enable(BOOL bSet = TRUE) { m_bEnabled = bSet;  }

	BOOL	HasDiagnostic	() const;
	BOOL	HasErrors		() const;
	BOOL	HasWarnings		() const;

public:
	// metodi per la formattazione dei messaggi di errore
	static const CString coCreateInstanceError	(HRESULT hr);
	static const CString putContentHandlerError	();
	static const CString putErrorHandlerError	();
	static const CString putLexicalHandlerError	();
	static const CString attachNullContentError	();
	static const CString attachBeforeInitError	();

	static const CString systemError			(LPCTSTR pszError);
	static const CString attributeError			(LPCTSTR szName = NULL);
	static const CString noRequiredRootError	(
													LPCTSTR szTagFound, 
													LPCTSTR szTagRequired, 
													const CString& sFileName
												);
	
	// di informazione
	static const CString skippedEntityInfo		(LPCTSTR szName, const CString& sFileName);

private:
	const CString parsingFileBanner	();
};

 //========================================================================
// CXMLSaxContent Declaration
//========================================================================
class TB_EXPORT CXMLSaxContent : public CXMLBaseSaxContent
{
	friend class CXMLSaxContentHandler;

	DECLARE_DYNAMIC(CXMLSaxContent)

public:
	enum ReturnValue { ABORT, OK, SKIP_THE_CHILDS };

private:
	CXMLSaxContentBindings			m_Bindings;
	static const CXMLXPathSyntax	m_KeySyntax;
	CXMLSaxDiagnostic				m_Diagnostic;
	CString							m_sCurrentKey;
	CString							m_sCurrentTagText;
	int								m_Status;
	CString							m_sChildsToSkip;
	CXMLSaxContentAttributes		m_arCurrentAttributes;

public:
	CXMLSaxContent ();

public:
	void				SetFileName		(const CString& sFileName);
	const CString		GetFileName		() const;
	const int&			GetStatus		() const;
	CXMLSaxDiagnostic*	GetDiagnostic	();
	void				ClearAttributes	();
	void				SetAttributes	(ISAXAttributes *pAttributes);

	// diagnostic
	BOOL	HasDiagnostic	() const;
	BOOL	HasErrors		() const;
	BOOL	HasWarnings		() const;
	
	// messages management
	void ClearMessages		();
	void AddWarning			(const CString& sMessage);
	void AddError			(const CString& sMessage);

protected:
	// bindings management
	void ClearBindings		();
	void BindParseFunctions	();
	void BindParseFunction	(const CString& sKey, SAX_ATTR_FUNCTION pFunctionPtr);
	void BindParseFunction	(const CString& sKey, SAX_TAG_FUNCTION pFunctionPtr);

private:
	// called by content event handler
	BOOL StartDocument	();
	BOOL EndDocument	();
	BOOL StartElement	(const CString& sUri, const CString& sTagName);
	BOOL EndElement		(const CString& sUri, const CString& sTagName);

protected:
	// to override to parse content data
	virtual void	OnBindParseFunctions();
	virtual CString OnGetRootTag		() const;
	virtual int		OnStartDocument		();
	virtual int		OnEndDocument		();
	virtual int		OnStartElement		(
											const CString& sKey, 
											const CString& sUri, 
											const CXMLSaxContentAttributes& arAttributes
										);
	virtual int		OnEndElement		(
											const CString& sKey, 
											const CString& sUri, 
											const CString& sTagValue
										);
};

//========================================================================
// CXMLSaxReader Declaration
//========================================================================
class TB_EXPORT CXMLSaxReader : public CObject
{
	DECLARE_DYNAMIC(CXMLSaxReader)

protected:
	ISAXXMLReader*			m_pXMLReader;
	CXMLSaxErrorHandler*	m_pErrorHandler;
	CXMLSaxContentHandler*	m_pContentHandler;
	CXMLSaxLexicalHandler*	m_pLexicalHandler;
	BOOL					m_bInitialized;


public:
	CXMLSaxReader	(BOOL bFreeThreaded = FALSE);
	CXMLSaxReader	(const CXMLSaxReader& XMLObj);
	~CXMLSaxReader	();
	
	// operatori
	CXMLSaxReader& operator=	(const CXMLSaxReader& XMLObj);

public:
	BOOL	Initialize	();
	void	Close		();

	BOOL	ReadFile	(const CString& sFileName);
	BOOL	Read		(const CString& sDoc, LPCTSTR pszFileName = NULL);
	
	CString	GetFileName		() const;

	void	AttachContent	(CXMLSaxContent*);
	void	DetachContent	();

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CXMLSaxReader\n"); CObject::Dump(dc);}
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};

//========================================================================
// MACROS
//========================================================================
//========================================================================
// functions binding prototype
//========================================================================
#define BIND_PARSE_ATTRIBUTES(key,pf) BindParseFunction(key, (SAX_ATTR_FUNCTION) pf);
#define BIND_PARSE_TAG(key,pf) BindParseFunction(key,(SAX_TAG_FUNCTION)  pf);

#include "endh.dex"
