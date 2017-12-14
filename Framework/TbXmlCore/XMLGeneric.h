#pragma once
//#include <winsock2.h>

//includere alla fine degli include del .H
#include "beginh.dex"

TB_EXPORT BOOL	IsIE5OrLaterInstalled	();
TB_EXPORT BOOL	IsInternetConnected		();
/////////////////////////////////////////////////////////////////////////////
/*typedef int (WSAAPI* LPWSASTARTUP)
	(
		WORD		wVersionRequested,
		LPWSADATA	lpWSAData
    );

typedef int (WSAAPI* LPWSACLEANUP)
	(
		void
    );

typedef int (WSAAPI* LPWSAGETLASTERROR)
	(
		void
    );

typedef int (WSAAPI* LPGETHOSTNAME)
	(
		LPTSTR name,
		int    namelen
    );

typedef struct hostent FAR* (WSAAPI* LPGETHOSTBYNAME)
	(
		LPCTSTR name
    );
*/

/////////////////////////////////////////////////////////////////////////////
TB_EXPORT CString	GetFileFullPath					(const CString&, BOOL = FALSE);
TB_EXPORT BOOL		CreateDirectoryTree				(const CString&);
TB_EXPORT BOOL		CreateFileDirectoryIfNecessary	(const CString&);

/////////////////////////////////////////////////////////////////////////////
TB_EXPORT CString	FormatStringForXML	(LPCTSTR);
TB_EXPORT CString	FormatBoolForXML	(const BOOL&, BOOL bSoapMode = FALSE);
TB_EXPORT BOOL		GetBoolFromXML		(const CString&);
/////////////////////////////////////////////////////////////////////////////

TB_EXPORT BOOL	IsGUIDStringValid	(LPCTSTR, BOOL = FALSE, GUID* = NULL);
TB_EXPORT CString GetGUIDString		(const GUID&, BOOL = FALSE);

/////////////////////////////////////////////////////////////////////////////

TB_EXPORT void SetBSTRText		(LPCTSTR, BSTR&);
TB_EXPORT void SetVariantText		(LPCTSTR, VARIANT&);

//============================================================================================
#define XML_NAMESPACEURI_TAG				_T("xmlns:")

#define SCHEMA_XML_NAMESPACEURI_VALUE		_T("urn:schemas-microsoft-com:xml-data")
#define SCHEMA_XML_DATATYPE_URI_VALUE		_T("urn:schemas-microsoft-com:datatypes")
#define SCHEMA_XML_SCHEMA_TAG				_T("Schema")
#define SCHEMA_XML_ELEMENT_TYPE_TAG			_T("ElementType")
#define SCHEMA_XML_ELEMENT_TAG				_T("element")
#define SCHEMA_XML_ATTRIBUTE_TYPE_TAG		_T("AttributeType")
#define SCHEMA_XML_DATA_TYPE_TAG			_T("datatype")
#define SCHEMA_XML_ATTRIBUTE_TAG			_T("attribute")
#define SCHEMA_XML_NAME_ATTRIBUTE			_T("name")
#define SCHEMA_XML_TYPE_ATTRIBUTE			_T("type")
#define SCHEMA_XML_DATATYPE_URI_ATTRIBUTE	_T("xmlns:dt")
#define SCHEMA_XML_DATATYPE_ATTRIBUTE		_T("dt:type")
#define SCHEMA_XML_CONTENT_ATTRIBUTE		_T("content")
#define SCHEMA_XML_DATATYPE_STRING_VALUE	_T("string")
#define SCHEMA_XML_DATATYPE_INT_VALUE		_T("int")
#define SCHEMA_XML_DATATYPE_FLOAT_VALUE		_T("float")
#define SCHEMA_XML_DATATYPE_UUID_VALUE		_T("uuid")
#define SCHEMA_XML_DATATYPE_DATE_VALUE		_T("date")
#define SCHEMA_XML_DATATYPE_DATETIME_VALUE	_T("dateTime")
#define SCHEMA_XML_DATATYPE_TIME_VALUE		_T("time")
#define SCHEMA_XML_DATATYPE_BOOLEAN_VALUE	_T("boolean")
#define SCHEMA_XML_CONTENT_TEXTONLY_VALUE	_T("textOnly") 

//============================================================================================
#define BEGIN_XML_SCHEMA(XMLSchema)\
	XMLSchema.SetNameSpaceURI(SCHEMA_XML_NAMESPACEURI_VALUE);\
	CXMLNode* pRoot = XMLSchema.CreateRoot(SCHEMA_XML_SCHEMA_TAG);\
	if (pRoot)\
	{\
		pRoot->SetAttribute(SCHEMA_XML_DATATYPE_URI_ATTRIBUTE, SCHEMA_XML_DATATYPE_URI_VALUE);

//============================================================================================
#define INSERT_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
		}

//============================================================================================
// content = Indicator of whether the content must be empty, or can contain text, 
// elements, or both. 
// The following values can be assigned to this attribute. 
//	empty		The element cannot contain content. 
//	textOnly	The element can contain only text, not elements. Note that if the 
//				model attribute is set to "open," the element can contain text and 
//				other unnamed elements. 
//	eltOnly		The element can contain only the specified elements. It cannot 
//				contain any free text. 
//	mixed		The element can contain a mix of named elements and text. 
//============================================================================================

//============================================================================================
#define INSERT_TEXTONLY_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: int
// Number, with optional sign, no fractions, and no exponent.
//============================================================================================
#define INSERT_TEXTONLY_INT_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_INT_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: float
// Real number, with no limit on digits; can potentially have a leading sign, 
// fractional digits, and optionally an exponent. Punctuation as in U.S. English. 
// Values range from 1.7976931348623157E+308 to 2.2250738585072014E-308.
//============================================================================================
#define INSERT_TEXTONLY_FLOAT_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_FLOAT_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: uuid
// Hexadecimal digits representing octets, optional embedded hyphens that are ignored.
// For example: "333C7BC4-460F-11D0-BC04-0080C7055A83".
//============================================================================================
#define INSERT_UUID_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_UUID_VALUE);\
			}\
		}

#define INSERT_TEXTONLY_UUID_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_UUID_VALUE);\
			}\
		}


//============================================================================================
// XML Data Type: date
// Date in a subset ISO 8601 format, without the time data. 
// For example: "1994-11-05"
//============================================================================================
#define INSERT_TEXTONLY_DATE_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATE_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: dateTime
// Date in a subset of ISO 8601 format, with optional time and no optional zone. 
// Fractional seconds can be as precise as nanoseconds. 
// For example, "1988-04-07T18:39:09".
//============================================================================================
#define INSERT_TEXTONLY_DATETIME_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATETIME_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: time
// Time in a subset ISO 8601 format, with no date and no time zone.
// For example: "08:15:27".
//============================================================================================
#define INSERT_TEXTONLY_TIME_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_TIME_VALUE);\
			}\
		}

//============================================================================================
// XML Data Type: boolean
// 0 or 1, where 0 == "false" and 1 =="true".
//============================================================================================
#define INSERT_TEXTONLY_BOOLEAN_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_CONTENT_ATTRIBUTE, SCHEMA_XML_CONTENT_TEXTONLY_VALUE);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_BOOLEAN_VALUE);\
			}\
		}

//============================================================================================
// ATTRIBUTE
//============================================================================================
#define INSERT_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
			}\
		}

//============================================================================================
#define INSERT_STRING_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_STRING_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_INT_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_INT_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_FLOAT_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_FLOAT_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_UUID_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_UUID_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_DATE_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATE_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_DATETIME_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATETIME_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_TIME_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_TIME_VALUE);\
			}\
		}

//============================================================================================
#define INSERT_BOOLEAN_ATTRIBUTE_TYPE_IN_XML_SCHEMA(name)\
		if (!pRoot->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
		{\
			CXMLNode* pNode = pRoot->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				pNode->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_BOOLEAN_VALUE);\
			}\
		}



//============================================================================================
#define BEGIN_ELEMENT_TYPE_IN_XML_SCHEMA(name)\
		{\
			CXMLNode* pNode = pRoot->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name);\
			if (!pNode)\
				pNode = pRoot->CreateNewChild(SCHEMA_XML_ELEMENT_TYPE_TAG);\
			ASSERT(pNode);\
			if (pNode)\
			{\
				CXMLNode* pChild;\
				pNode->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);

#define INSERT_ELEMENT_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ELEMENT_TAG, SCHEMA_XML_TYPE_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ELEMENT_TAG);\
					ASSERT(pChild);\
					if (pChild)\
						pChild->SetAttribute(SCHEMA_XML_TYPE_ATTRIBUTE, name);\
				}

#define INSERT_ATTRIBUTE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TAG, SCHEMA_XML_TYPE_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
						pChild->SetAttribute(SCHEMA_XML_TYPE_ATTRIBUTE, name);\
				}

#define INSERT_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
				}

#define INSERT_STRING_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_STRING_VALUE);\
					}\
				}

#define INSERT_INT_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_INT_VALUE);\
					}\
				}

#define INSERT_FLOAT_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_FLOAT_VALUE);\
					}\
				}

#define INSERT_UUID_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_UUID_VALUE);\
					}\
				}

#define INSERT_DATE_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATE_VALUE);\
					}\
				}

#define INSERT_DATETIME_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_DATETIME_VALUE);\
					}\
				}

#define INSERT_TIME_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_TIME_VALUE);\
					}\
				}


#define INSERT_BOOLEAN_ATTRIBUTE_TYPE_IN_XML_ELEMENT_TYPE(name)\
				if (!pNode->GetChildByAttributeValue(SCHEMA_XML_ATTRIBUTE_TYPE_TAG, SCHEMA_XML_NAME_ATTRIBUTE, name))\
				{\
					pChild = pNode->CreateNewChild(SCHEMA_XML_ATTRIBUTE_TYPE_TAG);\
					ASSERT(pChild);\
					if (pChild)\
					{\
						pChild->SetAttribute(SCHEMA_XML_NAME_ATTRIBUTE, name);\
						pChild->SetAttribute(SCHEMA_XML_DATATYPE_ATTRIBUTE, SCHEMA_XML_DATATYPE_BOOLEAN_VALUE);\
					}\
				}

#define END_ELEMENT_TYPE\
			}\
		}

//============================================================================================
#define END_XML_SCHEMA\
	}

//============================================================================================

#include "endh.dex"
