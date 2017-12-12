
#include "stdafx.h"
#include <io.h>

#include "XMLSchema.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


const TCHAR szXsdExt				[]	= _T(".xsd");
const TCHAR szNamespacePrefix		[]	= _T("maxs");


//----------------------------------------------------------------
//class CXSDGenerator implementation
//----------------------------------------------------------------
//
//----------------------------------------------------------------
CXSDGenerator::CXSDGenerator(const CString& strTargetNamespace /*=""*/, BOOL bDisplayMsgBox/*=FALSE*/, const CString& strId /*= _T("")*/)
{
	m_pSchema = new CXMLDocumentObject (TRUE, bDisplayMsgBox, FALSE);
	m_pSchema->SetNameSpaceURI(SCHEMA_XSD_NAMESPACEURI_VALUE, SCHEMA_XSD_NAMESPACEURI_PREFIX);
	CXMLNode *pRoot = m_pSchema->CreateRoot(SCHEMA_XSD_SCHEMA_TAG);

	if (pRoot)
	{
		m_ContextNodeListForElements.Add (pRoot);
		m_ContextNodeListForAttributes.Add (pRoot);

		pRoot->SetAttribute(SCHEMA_XSD_ELEMENT_FORM_DEFAULT_ATTRIBUTE, SCHEMA_XSD_QUALIFIED_VALUE);

		if(!strTargetNamespace.IsEmpty())
		{
			pRoot->SetAttribute(SCHEMA_XSD_XMLNS, strTargetNamespace);
			pRoot->SetAttribute(SCHEMA_XSD_TARGET_NAMESPACE_ATTRIBUTE, strTargetNamespace);
		}
		if(!strId.IsEmpty())
			pRoot->SetAttribute(SCHEMA_XSD_ID, strId);
		
	}
	else
		ASSERT(FALSE);

	m_pCurrentNode = pRoot;
	m_bCheckForDuplicates = FALSE;
}

//----------------------------------------------------------------
BOOL CXSDGenerator::InsertElement 
						(
							const CString& strName, 
							const CString& strType /*=""*/,
							const CString& strInf /*=""*/, 
							const CString& strSup /*=""*/,
							const CString& strFixed /*=""*/,
							const CString& strDefault /*=""*/)
{
	CXMLNode *pNode =  GetElementContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	CXMLNode *pTestNode = m_bCheckForDuplicates
							? pNode->GetChildByAttributeValue(CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + SCHEMA_XSD_ELEMENT_TAG, SCHEMA_XSD_NAME_ATTRIBUTE, strName)
							: NULL;
	if(pTestNode)
	{
		CString strTypeAttr;
		pTestNode->GetAttribute (SCHEMA_XSD_TYPE_ATTRIBUTE, strTypeAttr);
		if(strTypeAttr == strType)
			return FALSE;

		return pTestNode->SetAttribute (SCHEMA_XSD_TYPE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE);

	}

	CXMLNode *pNewNode = pNode->CreateNewChild (SCHEMA_XSD_ELEMENT_TAG, SCHEMA_XSD_NAMESPACEURI_VALUE);
	if(!pNewNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pNewNode->SetAttribute(SCHEMA_XSD_NAME_ATTRIBUTE, strName);

	if(!strType.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_TYPE_ATTRIBUTE, strType);
	
	if(!strInf.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_MINOCCURS_TAG, strInf);
	
	if(!strSup.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_MAXOCCURS_TAG, strSup);
	
	if(!strFixed.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_FIXED_ATTRIBUTE, strFixed);
	
	if(!strDefault.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_DEFAULT_ATTRIBUTE, strDefault);

	m_pCurrentNode = pNewNode;

	return TRUE;
}

//----------------------------------------------------------------
BOOL CXSDGenerator::InsertAttribute 
						(
							const CString& strName, 
							const CString& strType /*=""*/, 
							const CString& strUse /*=""*/, 
							const CString& strFixed /*=""*/, 
							const CString& strDefault /*=""*/
						)
{
	CXMLNode *pNode = GetAttributeContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	CXMLNode *pTestNode = m_bCheckForDuplicates
							? pNode->GetChildByAttributeValue(CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + SCHEMA_XSD_ATTRIBUTE_TAG, SCHEMA_XSD_NAME_ATTRIBUTE, strName)
							: NULL; 
	if(pTestNode)
	{
		return FALSE;
	}

	CXMLNode *pNewNode = pNode->CreateNewChild (SCHEMA_XSD_ATTRIBUTE_TAG);
	if(!pNewNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pNewNode->SetAttribute(SCHEMA_XSD_NAME_ATTRIBUTE, strName);

	if(!strType.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_TYPE_ATTRIBUTE, strType);

	if(!strUse.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_USE_ATTRIBUTE, strUse);
	
	if(!strFixed.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_FIXED_ATTRIBUTE, strFixed);

	if(!strDefault.IsEmpty ())
		pNewNode->SetAttribute(SCHEMA_XSD_DEFAULT_ATTRIBUTE, strDefault);
	
	m_pCurrentNode = pNewNode;

	return TRUE;
}

//----------------------------------------------------------------
BOOL CXSDGenerator::BeginComplexType (const CString& strName /*=""*/, const CString& strContentType /*=SCHEMA_XSD_TYPE_SEQUENCE*/)
{
	CXMLNode *pNode = GetElementContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	CXMLNode *pTestNode = m_bCheckForDuplicates
							? strName.IsEmpty()
								? pNode->GetChildByName (CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + SCHEMA_XSD_COMPLEX_TYPE_TAG)
								: pNode->GetChildByAttributeValue(CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + SCHEMA_XSD_COMPLEX_TYPE_TAG, SCHEMA_XSD_NAME_ATTRIBUTE, strName)
							: NULL; 
	if(pTestNode)
	{
		m_ContextNodeListForAttributes.Add (NULL);
		m_ContextNodeListForElements.Add (NULL);
		return FALSE;
	}

	CXMLNode *pAttrNode = pNode->CreateNewChild (SCHEMA_XSD_COMPLEX_TYPE_TAG);
	if(!pAttrNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	if (!strName.IsEmpty())
		pAttrNode->SetAttribute(SCHEMA_XSD_NAME_ATTRIBUTE, strName);

	CXMLNode *pElNode = pAttrNode->CreateNewChild (strContentType);
	if(!pElNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_ContextNodeListForAttributes.Add (pAttrNode);
	m_ContextNodeListForElements.Add (pElNode);

	m_pCurrentNode = pElNode;
		
	return TRUE;
}

//----------------------------------------------------------------
void CXSDGenerator::EndComplexType ()
{
	EndGenericElement();
}

//----------------------------------------------------------------
BOOL CXSDGenerator::BeginComplexElement (const CString& strName, const CString& strInf /*=""*/, const CString& strSup /*=""*/)
{
	if(!InsertElement(strName, _T(""), strInf, strSup))
	{
		m_ContextNodeListForAttributes.Add (NULL);	//complex element
		m_ContextNodeListForElements.Add (NULL);	//complex element
	
		m_ContextNodeListForAttributes.Add (NULL);	//complex type
		m_ContextNodeListForElements.Add (NULL);	//complex type

		return FALSE;
	}

	m_ContextNodeListForAttributes.Add (m_pCurrentNode);
	m_ContextNodeListForElements.Add (m_pCurrentNode);

	return BeginComplexType ();
}

//----------------------------------------------------------------
void CXSDGenerator::EndComplexElement ()
{
	EndComplexType();
	EndGenericElement();
}

//----------------------------------------------------------------
BOOL CXSDGenerator::BeginSimpleElement (const CString& strName, const CString& strType, const CString& strInf /*=""*/, const CString& strSup /*=""*/)
{
	if(!InsertElement(strName, _T(""), strInf, strSup))
	{
		m_ContextNodeListForAttributes.Add (NULL);	//complex element
		m_ContextNodeListForElements.Add (NULL);	//complex element
	
		m_ContextNodeListForAttributes.Add (NULL);	//complex type
		m_ContextNodeListForElements.Add (NULL);	//complex type

		return FALSE;
	}

	m_ContextNodeListForAttributes.Add (m_pCurrentNode);
	m_ContextNodeListForElements.Add (m_pCurrentNode);

	if (!BeginComplexType (_T(""), SCHEMA_XSD_TYPE_SIMPLECONTENT)) return FALSE;

	if (!BeginGenericElement(SCHEMA_XSD_EXTENSION_TAG)) return FALSE;

	CXMLNode *pNode = GetElementContextNode();
	if (!pNode) return FALSE;

	return pNode->SetAttribute(SCHEMA_XSD_BASE_ATTRIBUTE, strType);
}

//----------------------------------------------------------------
void CXSDGenerator::EndSimpleElement ()
{
	EndGenericElement();	//SCHEMA_XSD_EXTENSION_TAG

	EndComplexType();
	EndGenericElement();
}

//----------------------------------------------------------------
BOOL CXSDGenerator::BeginSimpleType (const CString& strName /*=""*/)
{
	CXMLNode *pNode = GetElementContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	CXMLNode *pTestNode = m_bCheckForDuplicates
							? pNode->GetChildByAttributeValue(CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + SCHEMA_XSD_SIMPLE_TYPE_TAG, SCHEMA_XSD_NAME_ATTRIBUTE, strName)
							: NULL;
	if(pTestNode)
	{
		m_ContextNodeListForAttributes.Add (NULL);
		m_ContextNodeListForElements.Add (NULL);
		return FALSE;
	}

	CXMLNode *pAttrNode = pNode->CreateNewChild (SCHEMA_XSD_SIMPLE_TYPE_TAG);
	if(!pAttrNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if(!strName.IsEmpty())
		pAttrNode->SetAttribute(SCHEMA_XSD_NAME_ATTRIBUTE, strName);

	m_ContextNodeListForAttributes.Add (pAttrNode);
	m_ContextNodeListForElements.Add (pAttrNode);

	m_pCurrentNode = pAttrNode;
		
	return TRUE;
}

//----------------------------------------------------------------
void CXSDGenerator::EndSimpleType ()
{
	EndGenericElement();
}


//----------------------------------------------------------------
BOOL CXSDGenerator::BeginGenericElement (const CString& strElement)
{
	if(!InsertGenericElement(strElement))
	{
		m_ContextNodeListForAttributes.Add (NULL);
		m_ContextNodeListForElements.Add (NULL);
		return FALSE;
	}

	m_ContextNodeListForAttributes.Add (m_pCurrentNode);
	m_ContextNodeListForElements.Add (m_pCurrentNode);
		
	return TRUE;
}

//----------------------------------------------------------------
void CXSDGenerator:: EndGenericElement ()
{
	ASSERT(m_ContextNodeListForAttributes.GetSize() == m_ContextNodeListForElements.GetSize() && 
		m_ContextNodeListForElements.GetSize ()>1);

	m_ContextNodeListForAttributes.RemoveAt (m_ContextNodeListForAttributes.GetUpperBound ());
	m_ContextNodeListForElements.RemoveAt (m_ContextNodeListForElements.GetUpperBound ());
}

//----------------------------------------------------------------
BOOL CXSDGenerator::InsertGenericElementAttribute (const CString& strName, const CString& strValue)
{
	CXMLNode *pNode = GetAttributeContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	return pNode->SetAttribute(strName, strValue);
}

//----------------------------------------------------------------
BOOL CXSDGenerator::InsertGenericElement (const CString& strElement)
{
	CXMLNode *pNode = GetElementContextNode();
	if(!pNode)
	{
		return FALSE;
	}

	CXMLNode *pTestNode = m_bCheckForDuplicates
								? pNode->GetChildByName(CString(SCHEMA_XSD_NAMESPACEURI_PREFIX_EX) + strElement)
								: NULL;
	if(pTestNode)
	{
		return FALSE;
	}

	pNode = pNode->CreateNewChild (strElement);
	if(!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pCurrentNode = pNode;
	return TRUE;
}