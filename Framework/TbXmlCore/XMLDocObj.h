#pragma once 

#include <MSXML6.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define	SELECTION_NAMESPACES	_T("SelectionNamespaces")
#define	DEFAULT_ENCODING		_T("UTF-8")

#define	ESCAPING_XSLT			_T("<?xml version=\"1.0\"?>")\
								_T("<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" >")\
								_T("<xsl:output method=\"xml\" indent=\"yes\"    cdata-section-elements=\"Mode\"/>")\
								_T("<xsl:template match=\"/ | @* | node()\">")\
								_T("    <xsl:copy>")\
								_T("      <xsl:apply-templates select=\"@* | node()\"/>")\
								_T("   </xsl:copy>")\
								_T("  </xsl:template>")\
								_T("</xsl:stylesheet>")

class CXMLDocumentObject;
class CXMLNodeChildsList;
class TBFile;

/////////////////////////////////////////////////////////////////////////////
// CXMLNode Declaration
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXMLNode : public CObject
{
	DECLARE_DYNCREATE(CXMLNode)

protected:
	IXMLDOMNode*		m_pXMLNode;
	IXMLDOMElement*		m_pXMLElement;
	CXMLDocumentObject*	m_pXMLDoc;
	CXMLNode*			m_pParentNode;			//valorizzato dalla GetChilds (mappato nella lista dei childs, quindi da NON deletare nel distruttore)
	CXMLNode*			m_pUnmappedParentNode;	//valorizzato dalla get_parentNode di MSXML (NON mappato nella lista dei childs, quindi da deletare nel distruttore)
	CXMLNodeChildsList*	m_pChildsList;
	CXMLNodeChildsList*	m_pAttributesList;
	int					m_pCurrentChildIdx;
	int					m_pCurrentAttributeIdx;

protected:
	void RemoveFromChildsList(CXMLNode* pNodeToRemove, int nIdxToRemove =-1);

public:
	CXMLNode	(IXMLDOMNode* = NULL, CXMLDocumentObject* = NULL, CXMLNode* pParent = NULL);
	~CXMLNode	();

	BOOL				GetName			(CString&);	//return prefix:tagName
	BOOL				GetBaseName		(CString&); //return tagName
	BOOL				GetType			(DOMNodeType&);
	BOOL				IsNamed			(LPCTSTR);
	BOOL				IsBaseNamed		(LPCTSTR);
	BOOL				SetText			(LPCTSTR);

	BOOL				GetText			(CString&);
	BOOL				GetXML			(CString& strXML) const;
	BOOL				GetCData		(CString&) const;

	BOOL				GetNodeValue	(CString&);
	BOOL				SetNodeValue	(LPCTSTR);

	CXMLDocumentObject*	GetXMLDocument  () const {return m_pXMLDoc;}
	void				SetXMLDocument  (CXMLDocumentObject*);
	
	CXMLNode*			GetParentNode	();

	BOOL				HasChildNodes			() const;
	CXMLNodeChildsList*	GetChilds				();
	int					GetChildsNum			();
	CXMLNode*			GetChildAt				(int);
	CXMLNodeChildsList* GetChildsByType			(DOMNodeType aType);
	CXMLNode*			GetChildByName			(LPCTSTR, BOOL = TRUE);
	CXMLNode*			GetChildByTagValue		(LPCTSTR, LPCTSTR, BOOL bCaseSensitive= TRUE, LCID nCulture = LOCALE_INVARIANT);
	CXMLNode*			GetChildByAttributeValue(LPCTSTR, LPCTSTR, LPCTSTR, BOOL bCaseSensitive= TRUE, LCID nCulture = LOCALE_INVARIANT);
	CXMLNode*			GetFirstChild			();
	CXMLNode*			GetNextChild			();
	CXMLNodeChildsList*	SelectNodes				(LPCTSTR lpszPattern, LPCTSTR lpszPrefix= NULL);
	CXMLNode*			SelectSingleNode		(LPCTSTR lpszPattern, LPCTSTR lpszPrefix= NULL);
	CXMLNode*			AppendChild				(CXMLNode* pNode);

	BOOL				ReplaceChildAt	(int, CXMLNode*);
	BOOL				RemoveChild		(CXMLNode*, int = -1);
	BOOL				RemoveChildAt	(int);

	BOOL				GetAttribute	(LPCTSTR, CString&);
	BOOL				SetAttribute	(LPCTSTR, LPCTSTR);
	BOOL				RemoveAttribute	(LPCTSTR);
	
	CXMLNodeChildsList* GetAttributes	();
	int					GetAttributesNum();
	CXMLNode*			GetAttributeAt	(int nIdx);
	CXMLNode*			GetFirstAttribute();
	CXMLNode*			GetNextAttribute();

	CXMLNode*			CreateNewChild	(LPCTSTR, LPCTSTR = NULL, LPCTSTR = NULL);
	CXMLNode*			CreateCDATASection	(LPCTSTR);
	
	BOOL				AppendText		(LPCTSTR);
	BOOL				Normalize		();
	
	IXMLDOMNode*		GetIXMLDOMNodePtr	() const { return m_pXMLNode;}
	CString				GetNamespaceURI		();
	CString				GetPrefix			();
private:
	IXMLDOMElement*		GetDomElement		();
public:
	// operatori di cast
	operator const	IXMLDOMNode*() const	{ return GetIXMLDOMNodePtr(); }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CXMLNode\n"); CObject::Dump(dc);}
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
// 	CXMLNodeChildsList
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLNodeChildsList : public CObArray
{
	DECLARE_DYNAMIC(CXMLNodeChildsList)

private:
	IXMLDOMNodeList*	m_pXMLNodeList;
	CXMLDocumentObject*	m_pXMLDoc;

public:
	CXMLNodeChildsList(IXMLDOMNodeList* = NULL, CXMLDocumentObject* = NULL, CXMLNode* pParent = NULL);
	~CXMLNodeChildsList();

	CXMLNode* 	GetAt		(int nIndex)const	{ return (CXMLNode*) CObArray::GetAt(nIndex);	}
	CXMLNode*&	ElementAt	(int nIndex)		{ return (CXMLNode*&) CObArray::ElementAt(nIndex); }
	
	CXMLNode* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	CXMLNode*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	void		RemoveAt	(int nIndex, int nCount = 1);
	void		RemoveAll	();
};

/////////////////////////////////////////////////////////////////////////////
// CXMLDocumentObject Declaration
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXMLDocumentObject : public CObject
{
	DECLARE_DYNCREATE(CXMLDocumentObject)

protected:
	IXMLDOMDocument2*	m_pXMLDoc;
	IXMLDOMSchemaCollection2 *m_pSchemaCollection;
	CXMLNode*			m_pRoot;
	BSTR				m_bstrNameSpaceURI;
	CString				m_strPrefix;
	BOOL				m_bAsync;
	BOOL				m_bValidateOnParse;		
	BOOL				m_bPreserveWhiteSpace;
	BOOL				m_bResolveExternals;
	BOOL				m_bMsgMode;
	BOOL				m_bFreeThreaded;
	BOOL				m_bLoaded;
	BOOL				m_bIndent;				//pilota l'inserimento di tab e crlf per formattare l'output su file
	CString				m_sFileName;

public:
	CXMLDocumentObject(BOOL bCreateNewMode = FALSE, BOOL bMsgMode = TRUE, BOOL bFreeThreaded = FALSE);
	~CXMLDocumentObject();
	
	//costruttore di copia
	CXMLDocumentObject(const CXMLDocumentObject& XMLObj);

	// operatore di assegnazione
	CXMLDocumentObject& operator=(const CXMLDocumentObject& XMLObj);


public:
	BOOL	Initialize	(BOOL bFreeThreaded = FALSE);
	void	Close		();
	void	Clear		();

	void	EnableMsgMode (BOOL bMsgMode = TRUE) { m_bMsgMode = bMsgMode; }
	BOOL	IsMsgModeEnabled () const { return m_bMsgMode; }

	void	SetNameSpaceURI(LPCTSTR, LPCTSTR = NULL);
	
	BOOL	SetAsync				(BOOL);
	BOOL	IsAsync					();

	BOOL	SetValidateOnParse		(BOOL);
	BOOL	IsValidatingOnParse		();
	
	BOOL	SetPreserveWhiteSpace	(BOOL);
	BOOL	IsPreservingWhiteSpace	();

	BOOL	SetResolveExternals		(BOOL);
	BOOL	IsResolvingExternals	();
	
	void	SetIndent		(BOOL bSet) {m_bIndent = bSet;}
	BOOL	GetIndent		()			{return m_bIndent;}

	BOOL	IsLoaded () const { return m_bLoaded; }

	BOOL	IsFreeThreaded () const { return m_bFreeThreaded ; }

	// Metodi utili in fase di Parsing
	virtual BOOL		LoadXML					(LPCTSTR);
	virtual BOOL		LoadXMLFile				(const CString&);
	virtual BOOL		LoadXMLFromUrl			(const CString&);

	CString				GetFileName			() const;
	CXMLNode*			GetRoot				();
	BOOL				GetRootName			(CString&);
	CXMLNodeChildsList*	GetRootChilds();
	int					GetRootChildsNum	();
	CXMLNode*			GetRootChildAt		(int);
	CXMLNode*			GetRootChildByName	(LPCTSTR, BOOL = TRUE);
	CXMLNode*			GetFirstRootChild	();
	CXMLNode*			GetNextRootChild	();
	
	CString				GetRootChildNameAt	(int);
	CString				GetRootChildTextAt	(int);

	CXMLNodeChildsList*	GetNodeListByTagName(LPCTSTR lpszTagName);
	
	CXMLNodeChildsList*	SelectNodes			(LPCTSTR lpszPattern, LPCTSTR lpszPrefix= NULL);
	CXMLNode*			SelectSingleNode	(LPCTSTR lpszPattern, LPCTSTR lpszPrefix= NULL);
	
	// Metodi utili in fase di Unparsing
	BOOL			CreateInitialProcessingInstruction	(const CString strTarget =_T("xml"), const CString strData =_T("version=\"1.0\" encoding=\"UTF-8\""));
	BOOL			InsertComment						(LPCTSTR lpszComment) ;
	CXMLNode*		CreateElement						(LPCTSTR, CXMLNode* = NULL, LPCTSTR = NULL, LPCTSTR = NULL);
	BOOL			RemoveNode							(CXMLNode*);
	CXMLNode*		CreateRoot							(LPCTSTR, LPCTSTR = NULL, LPCTSTR = NULL);
	CXMLNode*		CreateRootChild						(LPCTSTR, LPCTSTR = NULL, LPCTSTR = NULL);

	virtual BOOL	SaveXMLFile							(const CString&, BOOL = FALSE);
	

	BOOL			GetXML								(CString&) const;

	BOOL			GetParseErrorString					(CString&, IXMLDOMParseError* pParseError = NULL) const;

	IXMLDOMDocument2* GetIXMLDOMDocumentPtr() const { return m_pXMLDoc;}
	
	BOOL			AddSelectionNamespace				(LPCTSTR lpszPrefix, LPCTSTR lpszNameSpace);
	CString			GetNamespaceURI						();
	CString			GetPrefix							();

	IXMLDOMSchemaCollection2* GetSchemaChache			();
	BOOL			SetSchemaFile						(const CString& strTargetNamespace, const CString& strFilePath);
	BOOL			Validate							(CString& strParseError, const CString& strTargetNamespace = _T(""), const CString& strFilePath = _T(""));
	
	CString			GetEncoding							();

	void			SetFileName							(const CString& sFileName);

protected:
	BOOL			SaveFormattedXML					(const CString&, BOOL = FALSE);


public:
	BOOL LoadMetadata(TBFile* pMetaDataFile);
	BOOL SaveMetadata(TBFile* pMetaDataFile);


// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CXMLDocumentObject\n"); CObject::Dump(dc);}
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};

/////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
