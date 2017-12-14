#pragma once

#include "XMLParser.h"
#include "beginh.dex"

TB_EXPORT extern const TCHAR szXsdExt[];
TB_EXPORT extern const TCHAR szNamespacePrefix[];

#define SCHEMA_XSD_NAMESPACEURI_VALUE		_T("http://www.w3.org/2001/XMLSchema")
#define SCHEMA_XSD_DATA_URI_VALUE			_T("urn:schemas-microsoft-com:xml-msdata")
#define SCHEMA_XSD_NAMESPACEURI_PREFIX		_T("xs")
#define SCHEMA_XSD_NAMESPACEURI_PREFIX_EX	SCHEMA_XSD_NAMESPACEURI_PREFIX _T(":")

#define SCHEMA_XSD_SCHEMA_TAG				_T("schema")
#define SCHEMA_XSD_XMLNS					_T("xmlns")
#define SCHEMA_XSD_ID						_T("id")
#define SCHEMA_XSD_DATA_URI_ATTRIBUTE		SCHEMA_XSD_XMLNS _T(":msdata")

#define SCHEMA_XSD_ELEMENT_FORM_DEFAULT_ATTRIBUTE		_T("elementFormDefault")
#define SCHEMA_XSD_ATTRIBUTE_FORM_DEFAULT_ATTRIBUTE		_T("attributeFormDefault")

#define SCHEMA_XSD_QUALIFIED_VALUE			_T("qualified")
#define SCHEMA_XSD_UNQUALIFIED_VALUE		_T("unqualified")

#define SCHEMA_XSD_USE_ATTRIBUTE			_T("use")
#define SCHEMA_XSD_FIXED_ATTRIBUTE			_T("fixed")
#define SCHEMA_XSD_DEFAULT_ATTRIBUTE		_T("default")

#define SCHEMA_XSD_OPTIONAL_VALUE			_T("optional")
#define SCHEMA_XSD_PROHIBITED_VALUE			_T("prohibited")
#define SCHEMA_XSD_REQUIRED_VALUE			_T("required")

#define SCHEMA_XSD_ELEMENT_TAG				_T("element")
#define SCHEMA_XSD_ATTRIBUTE_TAG			_T("attribute")
#define SCHEMA_XSD_COMPLEX_TYPE_TAG			_T("complexType")
#define SCHEMA_XSD_SIMPLE_TYPE_TAG			_T("simpleType")
#define SCHEMA_XSD_RESTRICTION_TAG			_T("restriction")
#define SCHEMA_XSD_ENUMERATION_TAG			_T("enumeration")
#define SCHEMA_XSD_UNION_TAG				_T("union")

#define SCHEMA_XSD_EXTENSION_TAG			_T("extension")

#define SCHEMA_XSD_MAXLENGTH_TAG			_T("maxLength")

#define SCHEMA_XSD_MINOCCURS_TAG			_T("minOccurs")
#define SCHEMA_XSD_MAXOCCURS_TAG			_T("maxOccurs")
#define SCHEMA_XSD_UNBOUNDED_VALUE			_T("unbounded")

#define SCHEMA_XSD_TARGET_NAMESPACE_ATTRIBUTE _T("targetNamespace")
#define SCHEMA_XSD_NAME_ATTRIBUTE			_T("name")
#define SCHEMA_XSD_TYPE_ATTRIBUTE			_T("type")
#define SCHEMA_XSD_BASE_ATTRIBUTE			_T("base")
#define SCHEMA_XSD_VALUE_ATTRIBUTE			_T("value")
#define SCHEMA_XSD_MEMBER_TYPES_ATTRIBUTE	_T("memberTypes")

#define SCHEMA_XSD_TYPE_ALL					_T("all")
#define SCHEMA_XSD_TYPE_SEQUENCE			_T("sequence")
#define SCHEMA_XSD_TYPE_CHOICE				_T("choice")
#define SCHEMA_XSD_TYPE_GROUP				_T("group")
#define SCHEMA_XSD_TYPE_COMPLEXCONTENT		_T("complexContent")
#define SCHEMA_XSD_TYPE_SIMPLECONTENT		_T("simpleContent")

#define SCHEMA_DATATYPE_STRING_VALUE	_T("string")
#define SCHEMA_DATATYPE_INTEGER_VALUE		_T("integer") //da chiedere a PERASSO

#define SCHEMA_DATATYPE_FLOAT_VALUE		_T("float") //da chiedere a PERASSO
#define SCHEMA_DATATYPE_DATE_VALUE		_T("date")
#define SCHEMA_DATATYPE_DATETIME_VALUE	_T("dateTime")
#define SCHEMA_DATATYPE_TIME_VALUE		_T("time")
#define SCHEMA_DATATYPE_BOOLEAN_VALUE	_T("boolean")

//@@BAUZI
#define SCHEMA_DATATYPE_SHORT_VALUE		_T("short")//per i DataInt
#define SCHEMA_DATATYPE_INT_VALUE		_T("int") //per i DataLng
#define SCHEMA_DATATYPE_DOUBLE_VALUE	_T("double") //per i DataDbl e derivati
#define SCHEMA_DATATYPE_UINT_VALUE		_T("unsignedInt") //per gli enumerativi


#define SCHEMA_XSD_DATATYPE_STRING_VALUE	SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_STRING_VALUE
#define SCHEMA_XSD_DATATYPE_INTEGER_VALUE	SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_INTEGER_VALUE
#define SCHEMA_XSD_DATATYPE_FLOAT_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_FLOAT_VALUE
#define SCHEMA_XSD_DATATYPE_DATE_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_DATE_VALUE
#define SCHEMA_XSD_DATATYPE_DATETIME_VALUE	SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_DATETIME_VALUE
#define SCHEMA_XSD_DATATYPE_TIME_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_TIME_VALUE
#define SCHEMA_XSD_DATATYPE_BOOLEAN_VALUE	SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_BOOLEAN_VALUE

//@@BAUZI
#define SCHEMA_XSD_DATATYPE_SHORT_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_SHORT_VALUE		
#define SCHEMA_XSD_DATATYPE_INT_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_INT_VALUE		
#define SCHEMA_XSD_DATATYPE_DOUBLE_VALUE	SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_DOUBLE_VALUE	
#define SCHEMA_XSD_DATATYPE_UINT_VALUE		SCHEMA_XSD_NAMESPACEURI_PREFIX_EX SCHEMA_DATATYPE_UINT_VALUE		




//----------------------------------------------------------------
//class CXMLXSDGenerator
//----------------------------------------------------------------
class TB_EXPORT CXSDGenerator : public CObject
{
private:
	CXMLDocumentObject	*m_pSchema;
	CXMLNode			*m_pCurrentNode;
	BOOL				m_bCheckForDuplicates;

	CPtrArray			m_ContextNodeListForElements;
	CPtrArray				m_ContextNodeListForAttributes;

public:

public:
	CXSDGenerator(const CString& strTargetNamespace, BOOL bDisplayMsgBox=FALSE, const CString& strId = _T(""));
	~CXSDGenerator()				{if(m_pSchema) delete m_pSchema;}

public:
	BOOL		InsertElement		(const CString& strName, const CString& strType =_T(""), const CString& strInf =_T(""), const CString& strSup =_T(""), const CString& strFixed =_T(""), const CString& strDefault =_T(""));
	BOOL		InsertAttribute		(const CString& strName, const CString& strType =_T(""), const CString& strUse =_T(""),	const CString& strFixed =_T(""), const CString& strDefault =_T(""));
	
	BOOL		BeginComplexType	(const CString& strName = _T(""), const CString& strContentType = SCHEMA_XSD_TYPE_SEQUENCE);
	void		EndComplexType		();

	BOOL		BeginComplexElement	(const CString& strName, const CString& strInf =_T(""), const CString& strSup =_T(""));
	void		EndComplexElement	();

	BOOL		BeginSimpleElement	(const CString& strName, const CString& strType, const CString& strInf =_T(""), const CString& strSup =_T(""));
	void		EndSimpleElement	();

	BOOL		BeginSimpleType		(const CString& strName =_T(""));
	void		EndSimpleType		();

	BOOL		BeginGenericElement		(const CString& strElement);
	void		EndGenericElement		();
	BOOL		InsertGenericElement	(const CString& strElement);
	BOOL		InsertGenericElementAttribute (const CString& strName, const CString& strValue);

	CXMLNode*	GetElementContextNode	(){return(CXMLNode*)m_ContextNodeListForElements[m_ContextNodeListForElements.GetUpperBound()];}
	CXMLNode*	GetAttributeContextNode	(){return(CXMLNode*)m_ContextNodeListForAttributes[m_ContextNodeListForAttributes.GetUpperBound()];}

	BOOL		SaveXMLFile		(const CString& strFileName, BOOL bCreatePath = FALSE){ return m_pSchema ? m_pSchema->SaveXMLFile(strFileName, bCreatePath) : NULL;}
	CXMLNode*	GetCurrentNode	() {return m_pCurrentNode;}

	void		SetCheckForDuplicates(BOOL bSet=TRUE)	{m_bCheckForDuplicates = bSet;}

	CXMLDocumentObject* GetSchema	()	{return m_pSchema; }


public: //operator

};

#include "endh.dex"
