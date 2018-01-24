#include "stdafx.h" 

#include <comdef.h>

#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Diagnostic.h>
#include "xmlgeneric.h"
#include "XMLDocObj.h"
#include "tbxmlcore.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// CXMLNode
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CXMLNode, CObject)

//----------------------------------------------------------------------------
CXMLNode::CXMLNode(IXMLDOMNode* pXMLNode /* = NULL*/, CXMLDocumentObject* pXMLDoc /* = NULL*/, CXMLNode* pParent /*= NULL*/)
	:
	m_pXMLNode				(pXMLNode),
	m_pXMLElement			(NULL),
	m_pXMLDoc				(pXMLDoc),
	m_pParentNode			(pParent),	
	m_pUnmappedParentNode	(NULL),
	m_pChildsList			(NULL),
	m_pAttributesList		(NULL),
	m_pCurrentChildIdx		(-1),
	m_pCurrentAttributeIdx	(-1)
{
	if (pXMLNode)
		pXMLNode->AddRef();
}  
 
//----------------------------------------------------------------------------
CXMLNode::~CXMLNode()
{
	if (m_pXMLNode)
		m_pXMLNode->Release();

	if (m_pXMLElement)
		m_pXMLElement->Release();

	if (m_pUnmappedParentNode)
		delete m_pUnmappedParentNode;

	if (m_pChildsList)
		delete m_pChildsList;	

	if (m_pAttributesList)
		delete m_pAttributesList;
}

// in strName return the node's name with prefix of namespace
//----------------------------------------------------------------------------
BOOL CXMLNode::GetName(CString& strName)
{
	strName.Empty();

	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BSTR bstrName;
	HRESULT hr = m_pXMLNode->get_nodeName(&bstrName);
	if (SUCCEEDED(hr))
		strName = bstrName;
	::SysFreeString(bstrName);

	return (SUCCEEDED(hr)); 
}

// in strName return the node's name without prefix of namespace
//----------------------------------------------------------------------------
BOOL CXMLNode::GetBaseName(CString& strName)
{
	strName.Empty();

	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BSTR bstrName;
	HRESULT hr = m_pXMLNode->get_baseName(&bstrName);
	if (SUCCEEDED(hr))
		strName = bstrName;
	::SysFreeString(bstrName);

	return (SUCCEEDED(hr)); 
}

//----------------------------------------------------------------------------
BOOL CXMLNode::GetType(DOMNodeType &aType)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	HRESULT hr = m_pXMLNode->get_nodeType (&aType);
	
	return (SUCCEEDED(hr)); 
}

//----------------------------------------------------------------------------
BOOL CXMLNode::IsNamed(LPCTSTR lpszName)
{
	CString strNodeName;
	if (!GetName(strNodeName))
		return !lpszName || !lpszName[0];

	if (!lpszName)
		return FALSE;

	return !strNodeName.Compare(lpszName);
}

//----------------------------------------------------------------------------
BOOL CXMLNode::IsBaseNamed(LPCTSTR lpszName)
{
	CString strNodeName;
	if (!GetBaseName(strNodeName))
		return !lpszName || !lpszName[0];

	if (!lpszName)
		return FALSE;

	return !strNodeName.Compare(lpszName);
}

//----------------------------------------------------------------------------
BOOL CXMLNode::SetText(LPCTSTR  lpszText)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr = S_FALSE;
	BSTR bstrText = NULL;
	
	try
	{
		BSTR bstrText;
		SetBSTRText(lpszText, bstrText);
		
		hr = m_pXMLNode->put_text(bstrText);
		::SysFreeString(bstrText);
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrText)
			::SysFreeString(bstrText);

		TRACE("XML DOM error.\n");
		
		return FALSE;
	}
	
	return (SUCCEEDED(hr)); 
}

//----------------------------------------------------------------------------
BOOL CXMLNode::GetXML(CString& strXML) const
{
	strXML.Empty ();

	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}


	BSTR bstrXML=NULL;
	BOOL bOk = TRUE;

	if (FAILED(m_pXMLNode->get_xml(&bstrXML)))
		bOk = FALSE;
	else
		strXML = bstrXML; 
	
	if (bstrXML)
		::SysFreeString(bstrXML);
	
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::GetCData(CString& str) const
{
	str.Empty();
	//CXMNodeChildsList* pCDATANodes = GetChildsByType(NODE_CDATA_SECTION);
	//if (pCDATANodes && pCDATANodes->GetSize() > 0)
	//{
	//	CXMLNode* pCDATANode = pCDATANodes->GetAt(0);
	//	if (!pCDATANode || !pCDATANode->GetNodeValue(str))
	//	{
	//		delete(pCDATANodes);
	//		return FALSE;
	//	}
	//	delete(pCDATANodes);
	//}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::SetNodeValue	(LPCTSTR lpszText)
{
	if (!m_pXMLNode) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	HRESULT hr = m_pXMLNode->put_nodeValue(_variant_t(lpszText));
	return SUCCEEDED(hr);
}

//----------------------------------------------------------------------------
BOOL CXMLNode::GetNodeValue	(CString& strText)
{
	strText.Empty();

	if (!m_pXMLNode) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	VARIANT vaText;
	VariantInit(&vaText);

	HRESULT hr = m_pXMLNode->get_nodeValue(&vaText);
	if (SUCCEEDED(hr))
	{
		strText = vaText.bstrVal;
		VariantClear (&vaText);
	}
	else
	{
		VariantClear (&vaText);
		return FALSE;
	}

	return TRUE; 
}

//----------------------------------------------------------------------------
BOOL CXMLNode::GetText(CString& strText)
{
	strText.Empty();

	if (!m_pXMLNode) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	//ATTENZIONE: chiamare il metodo get_text del nodo non va bene:
	//se il nodo contiene altri nodi con del testo, questa funzione mi restituisce 
	//la concatenazione del testo di tali nodi (discendendo ricorsivamente)
	//get_text è una funzione proprietaria Microsoft, non standard
	//lo standard è invece get_nodeValue
	CXMLNodeChildsList *pTextChilds = GetChildsByType(NODE_TEXT);
	
	if (!pTextChilds) return TRUE;	//non ho nodi di tipo testo: il testo
									//corrisponde a stringa vuota

	CXMLNode *pTextNode = NULL;
	
	for (int i=0; i<pTextChilds->GetSize(); i++)
	{
		pTextNode = pTextChilds->GetAt(i);

		VARIANT vaText;
		VariantInit(&vaText);

		HRESULT hr = pTextNode->GetIXMLDOMNodePtr()->get_nodeValue(&vaText);
		if (SUCCEEDED(hr))
		{
			strText += vaText.bstrVal;
			VariantClear (&vaText);
		}
		else
		{
			VariantClear (&vaText);
			delete pTextChilds;
			return FALSE;
		}
	}

	delete pTextChilds;
	return TRUE; 
}

//----------------------------------------------------------------------------
void CXMLNode::SetXMLDocument  (CXMLDocumentObject* pDoc)
{
	ASSERT(pDoc != NULL);

	m_pXMLDoc = pDoc; 
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetParentNode()
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return NULL;
	}

	// m_pParentNode viene valorizzato ogniqualvolta si calcolano i childs del parent
	if(m_pParentNode)
		return m_pParentNode;

	// se sono in presenza di un nodo per il quale il parent è sconosciuto, devo chiederlo a MSXML
	IXMLDOMNode* pParent = NULL;
	if (!m_pUnmappedParentNode && SUCCEEDED(m_pXMLNode->get_parentNode(&pParent)))
	{
		m_pUnmappedParentNode = new CXMLNode(pParent);
		pParent->Release();
	}

	return m_pUnmappedParentNode;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::HasChildNodes () const
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	VARIANT_BOOL vaHasChilds;
	return SUCCEEDED(m_pXMLNode->hasChildNodes(&vaHasChilds)) && vaHasChilds;
}

//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLNode::GetChilds()
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	if (m_pChildsList)
		return m_pChildsList;

	if (!HasChildNodes())
		return NULL;

	IXMLDOMNodeList* pNodeList = NULL;
	if (FAILED(m_pXMLNode->get_childNodes(&pNodeList)))
	{
		if(pNodeList)
			pNodeList->Release();
		return NULL;
	}

	if (pNodeList)
	{
		m_pChildsList = new CXMLNodeChildsList(pNodeList, m_pXMLDoc, this);
		ASSERT(m_pChildsList);
	}
	return m_pChildsList; 
}

//----------------------------------------------------------------------------
int CXMLNode::GetChildsNum()
{
	if (
			!m_pXMLNode	||
			(!m_pChildsList && !GetChilds()) 
		)
	{
		ASSERT(m_pXMLNode);
		return 0;
	}
	return m_pChildsList->GetSize();
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetChildAt(int nIdx)
{
	CXMLNode* pNode = NULL;
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()			||
			(nIdx < 0)							||
			(nIdx >= GetChildsNum())			||
			!(pNode = m_pChildsList->GetAt(nIdx))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	return pNode;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetChildByName (LPCTSTR lpszChildName, BOOL bCaseSensitive /*= TRUE*/)
{
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()			||
			!lpszChildName || !lpszChildName[0]
		)
	{
		ASSERT(m_pXMLNode);
		return NULL;
	}
	for (int nIdx = 0; nIdx < GetChildsNum(); nIdx++)
	{
		CXMLNode* pNode = m_pChildsList->GetAt(nIdx);
		if (!pNode)
			continue;
		// potrei utilizzare un namespace con il prefisso allora prima provo con il basename e 
		// poi eventualmente con il name
		CString strNodeName;
		if 
			(
				pNode->GetBaseName(strNodeName) && 
				(
					(bCaseSensitive && !strNodeName.Compare(lpszChildName)) ||
					(!bCaseSensitive && !_tcsicmp(strNodeName, lpszChildName))
				)
			)
			return pNode;
		
		if 
			(
				pNode->GetName(strNodeName) && 
				(
					(bCaseSensitive && !strNodeName.Compare(lpszChildName)) ||
					(!bCaseSensitive && !_tcsicmp(strNodeName, lpszChildName))
				)
			)
			return pNode;
	}
	return NULL;
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore alla lista dei nodi 
// recuperati dalla stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLNode::GetChildsByType(DOMNodeType aType)
{
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()			
		)
	{
		ASSERT(m_pXMLNode);
		return NULL;
	}

	DOMNodeType tmpType;
	
	CXMLNodeChildsList* pList = NULL;

	for (int nIdx = 0; nIdx < GetChildsNum(); nIdx++)
	{
		CXMLNode* pNode = m_pChildsList->GetAt(nIdx);
		if (!pNode)
			continue;
		
		if (pNode->GetType (tmpType) && (aType == tmpType))
		{
			if(!pList)
				pList = new CXMLNodeChildsList(NULL, m_pXMLDoc, this);
			
			pList->Add (new CXMLNode(pNode->GetIXMLDOMNodePtr(), m_pXMLDoc));
		}
	}

	return pList;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetChildByTagValue	(LPCTSTR lpszChildTag, LPCTSTR lpszChildTagValue, BOOL bCaseSensitive /*= TRUE*/, LCID nCulture /*LOCALE_INVARIANT*/)
{
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()			||
			!lpszChildTag || !lpszChildTag[0]
		)
	{
		ASSERT(m_pXMLNode);
		return NULL;
	}
	CString sChildTagValue (lpszChildTagValue);

	for (int nIdx = 0; nIdx < GetChildsNum(); nIdx++)
	{
		CXMLNode* pNode = m_pChildsList->GetAt(nIdx);
		if (!pNode || !pNode->GetChilds())
			continue;
		CXMLNode* pChildNode = pNode->GetFirstChild();
		while (pChildNode)
		{
			CString strTag;
			CString strValue;
			// potrei utilizzare un namespace con il prefisso allora prima provo con il basename e 
			// poi eventualmente con il name		
			if 
				(
					(
						(pNode->GetBaseName(strTag) && 
						(
							(bCaseSensitive && !strTag.Compare(lpszChildTag)) ||
							(!bCaseSensitive && !_tcsicmp(strTag, lpszChildTag))
						)) ||
						(pChildNode->GetName(strTag) && 
						(
							(bCaseSensitive && !strTag.Compare(lpszChildTag)) ||
							(!bCaseSensitive && !_tcsicmp(strTag, lpszChildTag))
						))
					) &&
					pChildNode->GetText(strValue) && CompareString
								(
									nCulture, 
									bCaseSensitive ? 0 : NORM_IGNORECASE, 
									(LPCTSTR) strValue, 
									strValue.GetLength(), 
									(LPCTSTR) sChildTagValue,
									sChildTagValue.GetLength()
								) == CSTR_EQUAL
				)
				return pNode;
			
			pChildNode = pNode->GetNextChild();
		}
	}
	return NULL;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetChildByAttributeValue(LPCTSTR lpszTag, LPCTSTR lpszAttribute, LPCTSTR lpszAttributeValue, BOOL bCaseSensitive /*= TRUE*/, LCID nCulture /*LOCALE_INVARIANT*/)
{
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()			||
			!lpszAttribute || !lpszAttribute[0]
		)
	{
		ASSERT(m_pXMLNode);
		return NULL;
	}

	
	//the first parameter of the method CompareString in WIN2K should be MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US
	// From method documentation in MSDN
	// On Windows XP and later: CompareString(LOCALE_INVARIANT, NORM_IGNORECASE, mystr, -1, _T("InLap"), -1);
	// For earlier operating systems: DWORD lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT);
	// CompareString(lcid, NORM_IGNORECASE, mystr, -1, _T("InLap"), -1);
	// See Bug #15596
	LCID lcid = nCulture;
	if (lcid == LOCALE_INVARIANT && IsOSWindows2K())
		lcid = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT);

	CString sAttributeValue (lpszAttributeValue);
	for (int nIdx = 0; nIdx < GetChildsNum(); nIdx++)
	{
		CXMLNode* pNode = m_pChildsList->GetAt(nIdx);
		if (!pNode)
			continue;
		
		CString strAttributeText;
		CString strTag;
		if (
				(
					!lpszTag || !lpszTag[0] ||
					(
						(pNode->GetBaseName(strTag) && 
						(
							(bCaseSensitive && !strTag.Compare(lpszTag)) ||
							(!bCaseSensitive && !_tcsicmp(strTag, lpszTag))
						)) ||
						(pNode->GetName(strTag) && 
						(
							(bCaseSensitive && !strTag.Compare(lpszTag)) ||
							(!bCaseSensitive && !_tcsicmp(strTag, lpszTag))
						))
					)
				) &&
				pNode->GetAttribute(lpszAttribute, strAttributeText) && CompareString
						(
								lcid, 
								bCaseSensitive ? 0 : NORM_IGNORECASE, 
								(LPCTSTR) strAttributeText, 
								strAttributeText.GetLength(), 
								(LPCTSTR) sAttributeValue,
								sAttributeValue.GetLength()
							) == CSTR_EQUAL
			)
			return pNode;			
	}
	return NULL;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetFirstChild()
{
	m_pCurrentChildIdx = -1;
	if (
			!m_pXMLNode							||
			(!m_pChildsList && !GetChilds())	||
			!m_pChildsList->GetSize()
		)
		return NULL;

	m_pCurrentChildIdx = 0;

	return m_pChildsList->GetAt(0);
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetNextChild()
{
	ASSERT(m_pCurrentChildIdx >= 0);
	if (++m_pCurrentChildIdx >= GetChildsNum())
		return NULL;

	CXMLNode* pNode = GetChildAt(m_pCurrentChildIdx);

	return pNode;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::ReplaceChildAt(int nIdx, CXMLNode* pNewChild)
{
	CXMLNode* pNodeToReplace = GetChildAt(nIdx);
	if (!pNodeToReplace || !pNodeToReplace->GetIXMLDOMNodePtr())
		return FALSE;
	
	IXMLDOMNode* pOutOldChild = NULL;
	
	HRESULT hr = m_pXMLNode->replaceChild
								(
									pNewChild ? pNewChild->GetIXMLDOMNodePtr() : NULL,
									pNodeToReplace->GetIXMLDOMNodePtr(),
									&pOutOldChild
								);
	
	return (SUCCEEDED(hr) && (!pNewChild || pOutOldChild));
}

//----------------------------------------------------------------------------
BOOL CXMLNode::RemoveChild(CXMLNode* pNodeToRemove, int nIdxToRemove /* -1*/)
{
	if (!m_pXMLNode || !pNodeToRemove || !pNodeToRemove->GetIXMLDOMNodePtr())
		return FALSE;

	IXMLDOMNode* pOutOldChild = NULL; 
	HRESULT hr = S_FALSE;
	try
	{
		HRESULT hr = m_pXMLNode->removeChild
									(
										pNodeToRemove->GetIXMLDOMNodePtr(),
										&pOutOldChild
									);
		if(pOutOldChild)
			pOutOldChild->Release ();

		if (FAILED(hr) && m_pXMLDoc && m_pXMLDoc->IsMsgModeEnabled())
		{
			CString strNodeName;
			pNodeToRemove->GetName(strNodeName);
			CString strMsg;
			switch(hr)
			{
				case E_INVALIDARG:
					// Value returned if oldChild is not a child of this node, when 
					// the specified oldChild is read-only and cannot be removed, 
					// or when oldChild is null.
					strMsg.Format(L"Failed to remove %s node: node not valid or inaccessible.", (LPCTSTR)strNodeName);
					break;
				default:
					strMsg.Format(L"Failed to remove %s node.", (LPCTSTR)strNodeName);
					break;
			}
			AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		}
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_pXMLDoc && m_pXMLDoc->IsMsgModeEnabled())
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);
		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		TRACE("XML DOM error.\n");
		if (m_pXMLDoc && m_pXMLDoc->IsMsgModeEnabled())
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);
		
		return FALSE;
	}
	
	if (SUCCEEDED(hr))
		RemoveFromChildsList(pNodeToRemove, nIdxToRemove);

	return SUCCEEDED(hr);
}

//----------------------------------------------------------------------------
void CXMLNode::RemoveFromChildsList(CXMLNode* pNodeToRemove, int nIdxToRemove /* -1*/)
{
	if (m_pChildsList)
	{
		if (nIdxToRemove < 0 || nIdxToRemove >= m_pChildsList->GetSize())
		{
			for (int nIdx = 0; nIdx < m_pChildsList->GetSize(); nIdx++)
			{
				CXMLNode* pNode = m_pChildsList->GetAt(nIdx);
				if
					(
						pNode && 
						(
							pNode == pNodeToRemove	||
							pNode->GetIXMLDOMNodePtr() == pNodeToRemove->GetIXMLDOMNodePtr()
						)
					)
				{
					m_pChildsList->RemoveAt(nIdx);
					break;
				}
			}
		}
		else
			m_pChildsList->RemoveAt(nIdxToRemove);
	}
}

//----------------------------------------------------------------------------
BOOL CXMLNode::RemoveChildAt(int nIdx)
{
	return RemoveChild(GetChildAt(nIdx), nIdx);
}

//----------------------------------------------------------------------------
IXMLDOMElement* CXMLNode::GetDomElement()
{
	ASSERT(m_pXMLNode);
	if (m_pXMLElement)
		return m_pXMLElement;

	VERIFY(SUCCEEDED(m_pXMLNode->QueryInterface
								(
									IID_IXMLDOMElement,
									(void **)&m_pXMLElement
								)));
	return m_pXMLElement;
}
//----------------------------------------------------------------------------
BOOL CXMLNode::GetAttribute(LPCTSTR lpszAttributeName, CString& strAttributeText)
{
	strAttributeText.Empty();

	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	IXMLDOMElement* pXMLElement = GetDomElement();
	
	if (!pXMLElement)
		return FALSE;
	
	_variant_t variantText;
	_bstr_t bstrAttrName(lpszAttributeName);
	HRESULT hr = pXMLElement->getAttribute(bstrAttrName, &variantText);
	
	if (FAILED(hr) || !variantText.bstrVal)
		return FALSE;
	
	strAttributeText = variantText.bstrVal;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::RemoveAttribute (LPCTSTR lpszAttributeName)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	IXMLDOMElement* pXMLElement = GetDomElement();
	if (!pXMLElement)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrAttrName;
	SetBSTRText(lpszAttributeName, bstrAttrName);

	HRESULT hr = pXMLElement->removeAttribute(bstrAttrName);
	
	::SysFreeString(bstrAttrName);

	return (SUCCEEDED(hr));
}

//----------------------------------------------------------------------------
BOOL CXMLNode::SetAttribute(LPCTSTR lpszAttributeName, LPCTSTR lpszAttributeText)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	IXMLDOMElement* pXMLElement = GetDomElement();
	
	if (!pXMLElement)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrAttrName;
	SetBSTRText(lpszAttributeName, bstrAttrName);

	VARIANT vaAttrText;
	SetVariantText(lpszAttributeText, vaAttrText);
	
	HRESULT hr = pXMLElement->setAttribute(bstrAttrName, vaAttrText);
	
	::SysFreeString(bstrAttrName);
	::SysFreeString(vaAttrText.bstrVal);

	return (SUCCEEDED(hr));
}


//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLNode::GetAttributes()
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	if (m_pAttributesList)
		return m_pAttributesList;

	IXMLDOMNamedNodeMap *pNodeMapAttr = NULL;

	if (FAILED(m_pXMLNode->get_attributes(&pNodeMapAttr)))
	{
		if(pNodeMapAttr)
			pNodeMapAttr->Release ();
		return NULL;
	}

	long nItems=0;
	if (FAILED(pNodeMapAttr->get_length(&nItems)) || nItems==0)
	{
		if(pNodeMapAttr)
			pNodeMapAttr->Release ();
		return NULL;
	}
	
	m_pAttributesList = new CXMLNodeChildsList(NULL, m_pXMLDoc, this);
	for (long index=0; index<nItems; index++)
	{
		IXMLDOMNode *pAttr = NULL;
		if(SUCCEEDED(pNodeMapAttr->get_item (index, &pAttr) && pAttr))
		{
			m_pAttributesList->Add(new CXMLNode(pAttr, m_pXMLDoc));
			pAttr->Release();
		}
	}

	if(pNodeMapAttr)
		pNodeMapAttr->Release ();

	return m_pAttributesList; 
}

//----------------------------------------------------------------------------
int CXMLNode::GetAttributesNum()
{
	if (
			!m_pXMLNode	||
			(!m_pAttributesList && !GetAttributes()) 
		)
	{
		ASSERT(m_pXMLNode);
		return 0;
	}
	return m_pAttributesList->GetSize();
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetAttributeAt(int nIdx)
{
	CXMLNode* pNode = NULL;
	if (
			!m_pXMLNode									||
			(!m_pAttributesList && !GetAttributes())	||
			!m_pAttributesList->GetSize()				||
			(nIdx < 0)									||
			(nIdx >= GetAttributesNum())				||
			!(pNode = m_pAttributesList->GetAt(nIdx))
		)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	return pNode;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetFirstAttribute()
{
	m_pCurrentAttributeIdx = -1;
	if (
			!m_pXMLNode									||
			(!m_pAttributesList && !GetAttributes())	||
			!m_pAttributesList->GetSize()
		)
		return NULL;

	m_pCurrentAttributeIdx = 0;

	return m_pAttributesList->GetAt(0);
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::GetNextAttribute()
{
	ASSERT(m_pCurrentAttributeIdx >= 0);
	if (++m_pCurrentAttributeIdx >= GetAttributesNum())
		return NULL;

	CXMLNode* pNode = GetAttributeAt(m_pCurrentAttributeIdx);

	return pNode;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::CreateNewChild
							(
								LPCTSTR lpszTagName, 
								LPCTSTR lpszNameSpaceURI /* = NULL*/, 
								LPCTSTR lpszPrefix /* = NULL*/
							)
{
	if (!m_pXMLNode || !m_pXMLDoc || !lpszTagName)
	{
		ASSERT(FALSE);
		return NULL;
	}

	CXMLNode* pNewChild = m_pXMLDoc->CreateElement(lpszTagName, this, lpszNameSpaceURI, lpszPrefix);
	if (!pNewChild)
		return NULL;
	
	if (!m_pChildsList && !GetChilds())
		m_pChildsList = new CXMLNodeChildsList;

	m_pChildsList->Add(pNewChild);
	pNewChild->m_pParentNode = this;

	return pNewChild;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLNode::CreateCDATASection (LPCTSTR lpszData)
{
	if (!m_pXMLNode || !m_pXMLDoc || !lpszData)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	BSTR data = CString(lpszData).AllocSysString();
	IXMLDOMCDATASection *pData = NULL;
	CXMLNode *pNewChild = NULL;

	HRESULT hr = m_pXMLDoc->GetIXMLDOMDocumentPtr()->createCDATASection(data, &pData);

	SysFreeString(data);

	if(FAILED(hr) || !pData)
		return NULL;

	IXMLDOMNode * pOutNewChild = NULL;
	hr = m_pXMLNode->appendChild (pData, &pOutNewChild);
	if(FAILED(hr) || !pOutNewChild)
		return NULL;

	pNewChild = new CXMLNode (pOutNewChild, m_pXMLDoc, this);
	pOutNewChild->Release();
	if (!m_pChildsList && !GetChilds())
		m_pChildsList = new CXMLNodeChildsList;

	m_pChildsList->Add(pNewChild);
	pData->Release();

	return pNewChild;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::AppendText(LPCTSTR lpszTextToAppend)
{
	if (!m_pXMLNode || !lpszTextToAppend)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	IXMLDOMDocument* pDOMDocument= NULL;
	HRESULT hr = m_pXMLNode->get_ownerDocument(&pDOMDocument);
	if (FAILED(hr) || !pDOMDocument)
	{
		if(pDOMDocument)
			pDOMDocument->Release ();
		ASSERT(FALSE);
		return FALSE;
	}
	
	BSTR bstrText;
	SetBSTRText(lpszTextToAppend, bstrText);
	
	IXMLDOMText* pText = NULL;
	hr = pDOMDocument->createTextNode(bstrText, &pText);	
	::SysFreeString(bstrText);
	

	IXMLDOMNode* pOutNewChild = NULL;
	BOOL bResult = (SUCCEEDED(hr) && pText && SUCCEEDED(m_pXMLNode->appendChild(pText,&pOutNewChild)));

	if(pText)
		pText->Release();
	if(pDOMDocument)
		pDOMDocument->Release();
	if(pOutNewChild)
		pOutNewChild->Release();

	return bResult;
}

//----------------------------------------------------------------------------
BOOL CXMLNode::Normalize()
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	IXMLDOMElement* pOutXMLElement = GetDomElement();
	if (pOutXMLElement)
		SUCCEEDED(pOutXMLElement->normalize());

	return FALSE;
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore alla lista dei nodi 
// recuperati dalla stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLNode::SelectNodes(LPCTSTR lpszPattern, LPCTSTR lpszPrefix /*=NULL*/)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return NULL;
	}

	if(lpszPrefix && _tcslen(lpszPrefix) && m_pXMLDoc) 
		m_pXMLDoc->AddSelectionNamespace (lpszPrefix, GetNamespaceURI());		

	IXMLDOMNodeList* pNodeList = NULL;

	BSTR bstrPattern = CString(lpszPattern).AllocSysString();
	HRESULT hr = m_pXMLNode->selectNodes(bstrPattern, &pNodeList);
	::SysFreeString(bstrPattern);

	if (FAILED(hr) || !pNodeList)
	{
		if(pNodeList)
			pNodeList->Release ();
		return NULL;
	}
	
	return new CXMLNodeChildsList(pNodeList, m_pXMLDoc);
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore al nodo 
// recuperato dalla stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
//----------------------------------------------------------------------------
CXMLNode* CXMLNode::SelectSingleNode(LPCTSTR lpszPattern, LPCTSTR lpszPrefix /*=NULL*/)
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return NULL;
	}

	IXMLDOMNode* pNode = NULL;

	BSTR bstrPattern = CString(lpszPattern).AllocSysString();
	HRESULT hr = m_pXMLNode->selectSingleNode(bstrPattern, &pNode);
	::SysFreeString(bstrPattern);

	if (FAILED(hr) || !pNode)
	{
		if(pNode)
			pNode->Release();
		return NULL;
	}

	CXMLNode *pOutNode = new CXMLNode(pNode, m_pXMLDoc);
	pNode->Release();
	return pOutNode;
}

///aggiunge il nodo rimuovendolo dal vecchio parent (se esiste)
//----------------------------------------------------------------------------
CXMLNode* CXMLNode::AppendChild(CXMLNode* pNode)
{
	if (!m_pXMLNode || 
		!pNode ||
		!pNode->GetIXMLDOMNodePtr ())
	{
		ASSERT(FALSE);
		return NULL;
	}

	IXMLDOMNode *pIXMLNode = pNode->GetIXMLDOMNodePtr();
	IXMLDOMNode *pIXMLNewNode = NULL;

	// se ci sono figli, valorizzo m_pChildsList;
	GetChilds();

	HRESULT hr = m_pXMLNode->appendChild (pIXMLNode, &pIXMLNewNode);
	if (FAILED(hr) || !pIXMLNewNode)
	{
		if(pIXMLNewNode)
			pIXMLNewNode->Release ();
		return NULL;
	}
	
	CXMLNode *pNewNode = new CXMLNode(pIXMLNewNode);
	pIXMLNewNode->Release();

	if (!m_pChildsList)
		m_pChildsList = new CXMLNodeChildsList (NULL, GetXMLDocument ());
	m_pChildsList->Add(pNewNode);
	pNewNode->m_pParentNode = this;
	
	CXMLNode *pParentNode = pNode->GetParentNode();
	pParentNode->RemoveFromChildsList (pNode);

	return pNewNode;
}

//----------------------------------------------------------------------------
CString CXMLNode::GetNamespaceURI() 
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrURI = NULL;
	CString strURI;
	if(SUCCEEDED(m_pXMLNode->get_namespaceURI(&bstrURI)))
	{
		strURI = bstrURI;
		if(bstrURI) SysFreeString(bstrURI);
		return strURI;
	}

	return _T("");
}

//----------------------------------------------------------------------------
CString CXMLNode::GetPrefix() 
{
	if (!m_pXMLNode)
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrPrefix = NULL;
	CString strPrefix;
	if(SUCCEEDED(m_pXMLNode->get_prefix(&bstrPrefix)))
	{
		strPrefix = bstrPrefix;
		if(bstrPrefix) SysFreeString(bstrPrefix);
		return strPrefix;
	}

	return _T("");
}

/////////////////////////////////////////////////////////////////////////////
// CXMLNodeChildsList
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CXMLNodeChildsList, CObArray)

//----------------------------------------------------------------------------
CXMLNodeChildsList::CXMLNodeChildsList(IXMLDOMNodeList* pXMLNodeList /* = NULL */, CXMLDocumentObject* pXMLDoc /* = NULL*/, CXMLNode* pParent /*=NULL*/)
	:
	m_pXMLNodeList	(pXMLNodeList),
	m_pXMLDoc		(pXMLDoc)
{
	LONG nCount = 0;
	if (pXMLNodeList && SUCCEEDED(pXMLNodeList->get_length(&nCount)))
	{
		for (int i = 0; i < nCount; i++)
		{
			IXMLDOMNode* pChild;
			if (SUCCEEDED(pXMLNodeList->get_item(i, &pChild)))
			{
				Add(new CXMLNode(pChild, m_pXMLDoc, pParent));
				pChild->Release();
			}
		}
	}
}

//----------------------------------------------------------------------------
CXMLNodeChildsList::~CXMLNodeChildsList()
{
	if(m_pXMLNodeList)
		m_pXMLNodeList->Release();

	RemoveAll();
}

//----------------------------------------------------------------------------
void CXMLNodeChildsList::RemoveAll()
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
void CXMLNodeChildsList::RemoveAt(int nIndex, int nCount)
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

/////////////////////////////////////////////////////////////////////////////
// CXMLDocumentObject
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CXMLDocumentObject, CObject)

//----------------------------------------------------------------------------
CXMLDocumentObject::CXMLDocumentObject(BOOL bCreateNewMode /*= FALSE*/, BOOL bMsgMode /*= TRUE*/, BOOL bFreeThreaded /*= FALSE*/)
	:
	m_pXMLDoc				(NULL),
	m_pRoot					(NULL),
	m_bstrNameSpaceURI		(NULL),
	m_bAsync				(FALSE),
	m_bValidateOnParse		(FALSE),
	m_bPreserveWhiteSpace	(FALSE),
	m_bResolveExternals		(FALSE),
	m_bMsgMode				(bMsgMode),
	m_bLoaded				(FALSE),
	m_bFreeThreaded			(bFreeThreaded), 
	m_pSchemaCollection		(NULL)
{
	Initialize(bFreeThreaded);
	if (bCreateNewMode)
		CreateInitialProcessingInstruction();
}

//----------------------------------------------------------------------------
CXMLDocumentObject::CXMLDocumentObject (const CXMLDocumentObject& XMLObj)
:
	m_pXMLDoc				(NULL),
	m_pRoot					(NULL),
	m_bstrNameSpaceURI		(NULL), 
	m_pSchemaCollection		(NULL)	
{
	*this = XMLObj;
}

//----------------------------------------------------------------------------
CXMLDocumentObject::~CXMLDocumentObject()
{
	Close();

	if (m_bstrNameSpaceURI)
		::SysFreeString(m_bstrNameSpaceURI);
}


//----------------------------------------------------------------------------
CXMLDocumentObject& CXMLDocumentObject::operator = (const CXMLDocumentObject& XMLObj)
{
	if (m_bstrNameSpaceURI) 
	{
		::SysFreeString(m_bstrNameSpaceURI);
		m_bstrNameSpaceURI=::SysAllocString (XMLObj.m_bstrNameSpaceURI);
	}
	
	m_bFreeThreaded			=	XMLObj.m_bFreeThreaded;

	Initialize(m_bFreeThreaded);
	m_bAsync				=	XMLObj.m_bAsync;
	m_bValidateOnParse		=	XMLObj.m_bValidateOnParse;		
	m_bPreserveWhiteSpace	=	XMLObj.m_bPreserveWhiteSpace;
	m_bResolveExternals		=	XMLObj.m_bResolveExternals;
	m_bMsgMode				=	XMLObj.m_bMsgMode;
	m_bLoaded				=	XMLObj.m_bLoaded;
	m_bIndent				=   XMLObj.m_bIndent;
	m_sFileName				=   XMLObj.m_sFileName;
	
	VARIANT_BOOL bResult;
	
	VARIANT vaDOMDoc;
	VariantInit(&vaDOMDoc);
	vaDOMDoc.vt = VT_UNKNOWN;
	vaDOMDoc.punkVal = XMLObj.m_pXMLDoc;

	HRESULT hr = m_pXMLDoc->load(vaDOMDoc, &bResult);
	if(FAILED(hr) || bResult == VARIANT_FALSE)
	{
		ASSERT(FALSE);
		m_pXMLDoc->Release();
		m_pXMLDoc = NULL;
	}

	return *this;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::Initialize(BOOL bFreeThreaded /*= FALSE*/)
{
	Close();

	REFCLSID classID = bFreeThreaded ? CLSID_FreeThreadedDOMDocument60 : CLSID_DOMDocument60; 
	HRESULT hr = ::CoCreateInstance
						(
							classID,
							NULL,								
							CLSCTX_INPROC_SERVER,
							IID_IXMLDOMDocument2,
							(void **)&m_pXMLDoc	
						);
    if (FAILED(hr))
    {
		if (m_pXMLDoc)
			m_pXMLDoc->Release();
		m_pXMLDoc = NULL;

		TRACE("Failed to create XML document. Please verify that MSXML 6.0 library has been installed.");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"Failed to create XML document. Please verify that MSXML 6.0 library has been installed.", CDiagnostic::Warning);
		
		return FALSE;
    }
	
	SetAsync				(FALSE);
	SetValidateOnParse		(FALSE);
	SetPreserveWhiteSpace	(FALSE);
	SetResolveExternals		(FALSE);
	SetIndent				(TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------
void CXMLDocumentObject::Close()
{
	if (m_pRoot) 
	{
		delete m_pRoot;
		m_pRoot = NULL;
	}

	if (m_pXMLDoc)
        m_pXMLDoc->Release();
	m_pXMLDoc = NULL;
	
	m_sFileName.Empty();

	if (m_pSchemaCollection)
        m_pSchemaCollection->Release();
	m_pSchemaCollection = NULL;
}

//----------------------------------------------------------------------------
void CXMLDocumentObject::Clear()
{
	delete m_pRoot;
	m_pRoot = NULL;

	IXMLDOMNodeList* pNodes = NULL;
	if (SUCCEEDED(m_pXMLDoc->get_childNodes(&pNodes)) && pNodes)
	{
		IXMLDOMNode *pNode = NULL;
		while(SUCCEEDED(pNodes->nextNode(&pNode)) && pNode)
		{
			IXMLDOMNode *pOldNode = NULL;
			if (SUCCEEDED(m_pXMLDoc->removeChild(pNode, &pOldNode)) && pOldNode)
				pOldNode->Release();
			pNode->Release();
		}
		pNodes->Release();
	}
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetFileName () const
{
	return m_sFileName;
}

//----------------------------------------------------------------------------
void CXMLDocumentObject::SetFileName (const CString& strFileName)
{
	m_sFileName = strFileName;
}

//----------------------------------------------------------------------------
IXMLDOMSchemaCollection2* CXMLDocumentObject::GetSchemaChache()
{
	if(!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return  NULL;
	}

	//lazy initialization
	if(!m_pSchemaCollection)
	{
		HRESULT hr = ::CoCreateInstance
						(
							CLSID_XMLSchemaCache60,
							NULL,								
							CLSCTX_INPROC_SERVER,
							IID_IXMLDOMSchemaCollection2,
							(void **)&m_pSchemaCollection	
						);

		if(FAILED(hr) || !m_pSchemaCollection)
		{
			if(m_pSchemaCollection)
				m_pSchemaCollection->Release ();
			return NULL;
		}
		
		VARIANT vaSchema;
		VariantInit(&vaSchema);
		vaSchema.vt = VT_UNKNOWN;
		vaSchema.punkVal = m_pSchemaCollection;

		hr = m_pXMLDoc->putref_schemas(vaSchema);
		if(FAILED(hr)) 
		{
			m_pSchemaCollection->Release ();
			m_pSchemaCollection=NULL;
			return NULL;
		}
	}

	return m_pSchemaCollection;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SetSchemaFile (const CString& strTargetNamespace, const CString& strFilePath)
{
	IXMLDOMSchemaCollection2* pSchemaCache = GetSchemaChache();
	
	if(!pSchemaCache)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrNamespace = strTargetNamespace.AllocSysString ();
	VARIANT vaSchema;
	VariantInit(&vaSchema);
	vaSchema.vt = VT_BSTR;
	vaSchema.bstrVal = strFilePath.AllocSysString ();

	HRESULT hr = pSchemaCache->add (bstrNamespace, vaSchema);

	if(bstrNamespace) SysFreeString (bstrNamespace);
	if(vaSchema.bstrVal) SysFreeString (vaSchema.bstrVal);

	return SUCCEEDED(hr);
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::Validate (CString& strParseError, const CString& strTargetNamespace /*=""*/, const CString& strFilePath /*=""*/)
{
	if(!strTargetNamespace.IsEmpty() && 
		!strFilePath.IsEmpty() &&
		!SetSchemaFile(strTargetNamespace, strFilePath)) 
		return FALSE;

	if(!m_pXMLDoc || !m_pSchemaCollection)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	IXMLDOMParseError*	pError = NULL;
	HRESULT hr = m_pXMLDoc->validate (&pError);
	if(pError)
	{
		GetParseErrorString(strParseError, pError);
		pError->Release ();
	}

	return hr==S_OK;
}

//----------------------------------------------------------------------------
void CXMLDocumentObject::SetNameSpaceURI(LPCTSTR lpszNameSpaceURI, LPCTSTR lpszPrefix /*= NULL*/)
{
	if (m_bstrNameSpaceURI)
		::SysFreeString(m_bstrNameSpaceURI);
	
	m_bstrNameSpaceURI = NULL;

	if (!lpszNameSpaceURI)
		return;
	
	m_strPrefix = lpszPrefix;

	SetBSTRText(lpszNameSpaceURI, m_bstrNameSpaceURI);
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SetAsync(BOOL bAsync)
{ 
	if (!m_pXMLDoc || FAILED(m_pXMLDoc->put_async(bAsync ? VARIANT_TRUE:VARIANT_FALSE))) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	m_bAsync = bAsync;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::IsAsync()
{
	VARIANT_BOOL vaAsync;
	
	BOOL bAsync =	m_pXMLDoc && 
					SUCCEEDED(m_pXMLDoc->get_async(&vaAsync)) && 
					vaAsync;
	if (bAsync != m_bAsync)
	{
		ASSERT(FALSE);
		m_bAsync = bAsync;
	}
	return bAsync;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SetValidateOnParse(BOOL bValidateOnParse)
{ 
	if (!m_pXMLDoc || FAILED(m_pXMLDoc->put_validateOnParse(bValidateOnParse ? VARIANT_TRUE:VARIANT_FALSE)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bValidateOnParse = bValidateOnParse;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::IsValidatingOnParse()
{
	VARIANT_BOOL vaValidateOnParse;
	BOOL bValidateOnParse =	m_pXMLDoc && 
							SUCCEEDED(m_pXMLDoc->get_validateOnParse(&vaValidateOnParse)) && 
							vaValidateOnParse;
	if (bValidateOnParse != m_bValidateOnParse)
	{
		ASSERT(FALSE);
		m_bValidateOnParse = bValidateOnParse;
	}
	return bValidateOnParse;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SetPreserveWhiteSpace(BOOL bPreserveWhiteSpace)
{ 
	if (!m_pXMLDoc || FAILED(m_pXMLDoc->put_preserveWhiteSpace(bPreserveWhiteSpace ? VARIANT_TRUE:VARIANT_FALSE)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bPreserveWhiteSpace = bPreserveWhiteSpace;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::IsPreservingWhiteSpace()
{
	VARIANT_BOOL vaPreserveWhiteSpace;
	BOOL bPreserveWhiteSpace =	m_pXMLDoc && 
								SUCCEEDED(m_pXMLDoc->get_preserveWhiteSpace(&vaPreserveWhiteSpace)) && 
								vaPreserveWhiteSpace;
	if (bPreserveWhiteSpace != m_bPreserveWhiteSpace)
	{
		ASSERT(FALSE);
		m_bPreserveWhiteSpace = bPreserveWhiteSpace;
	}
	return bPreserveWhiteSpace;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SetResolveExternals(BOOL bResolveExternals)
{ 
	if (!m_pXMLDoc || FAILED(m_pXMLDoc->put_resolveExternals(bResolveExternals ? VARIANT_TRUE:VARIANT_FALSE)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bResolveExternals = bResolveExternals;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::IsResolvingExternals()
{
	VARIANT_BOOL vaResolveExternals;
	
	BOOL bResolveExternals =	m_pXMLDoc && 
								SUCCEEDED(m_pXMLDoc->get_resolveExternals(&vaResolveExternals)) && 
								vaResolveExternals;
	if (bResolveExternals != m_bResolveExternals)
	{
		ASSERT(FALSE);
		m_bResolveExternals = bResolveExternals;
	}
	return bResolveExternals;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::LoadXML(LPCTSTR lpszXML)
{
	BSTR bstrXML = NULL;
	try
	{
		if (m_pRoot)
		{
			delete m_pRoot;
			m_pRoot = NULL;
		}
		
		SetBSTRText(lpszXML, bstrXML);

		VARIANT_BOOL bResult;
		HRESULT hr = m_pXMLDoc->loadXML(bstrXML, &bResult);
		::SysFreeString(bstrXML);

		if (FAILED(hr) || bResult == VARIANT_FALSE)
			return FALSE;
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (bstrXML)
			::SysFreeString(bstrXML);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add (szError, CDiagnostic::Warning);
		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrXML)
			::SysFreeString(bstrXML);
		TRACE("XML DOM error.\n");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);
		
		return FALSE;
	}

	return TRUE;
}

//=============================================================================
BOOL CXMLDocumentObject::LoadMetadata(TBFile* pMetaDataFile)
{
	if (!pMetaDataFile)
		return TRUE;
	
	return  (pMetaDataFile->m_pFileContent)
		? LoadXML(pMetaDataFile->GetContentAsString())
		: LoadXMLFile(pMetaDataFile->m_strCompleteFileName);
}


//=============================================================================
BOOL CXMLDocumentObject::SaveMetadata(TBFile* pMetaDataFile)
{
	if (!pMetaDataFile)
		return TRUE;

	if (pMetaDataFile->m_pFileContent)
	{
		ASSERT(FALSE);
		CString strXML;
		GetXML(strXML);
		//return pMetaDataFile->SetContentFromString(strXML);
		return FALSE;
	}

	 return SaveXMLFile(pMetaDataFile->m_strCompleteFileName);
}

//=============================================================================
BOOL CXMLDocumentObject::LoadXMLFile (const CString& strFileName)
{
	m_bLoaded = FALSE; 
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(strFileName))
	{
		CString sFileContent = pFileSystemManager->GetTextFile (strFileName);		
		return !sFileContent.IsEmpty() && LoadXML(sFileContent);
	}

	// sono su file system
	if ( _taccess((LPCTSTR)strFileName, 04 ) == -1)
		return FALSE;
	
	return LoadXMLFromUrl(strFileName);
}

//=============================================================================
BOOL CXMLDocumentObject::LoadXMLFromUrl (const CString& strUrl)
{
	m_bLoaded = FALSE; 

	if	(
			!m_pXMLDoc	|| 
			strUrl.IsEmpty()
		)
		return FALSE;
	
	BOOL bOk =  FALSE;


	if (m_pRoot)
	{
		delete m_pRoot;
		m_pRoot = NULL;
	}

	VARIANT vaFileName;
	try
	{
		VariantInit(&vaFileName);
		vaFileName.vt = VT_BSTR;
		vaFileName.bstrVal = strUrl.AllocSysString();

		short nSuccess;

		HRESULT hr = m_pXMLDoc->load(vaFileName, &nSuccess);
		::SysFreeString(vaFileName.bstrVal);
		
		if (FAILED(hr) || !nSuccess)
		{
			CString strError;
			if (m_bMsgMode && GetParseErrorString(strError))
				AfxGetDiagnostic()->Add(strError, CDiagnostic::Warning);
			return FALSE;
		}
		else
		{
			GetRoot();	// If the URL cannot be resolved or accessed or does not 
						// reference an XML document, the IXMLDOMDocument interface's 
						// documentElement property is set to null. 
			ASSERT(m_pRoot);
			
			// root node could have default uri defined
			if (m_pRoot && !m_pRoot->GetNamespaceURI().IsEmpty())
				SetNameSpaceURI(m_pRoot->GetNamespaceURI());
		}
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (vaFileName.bstrVal)
			::SysFreeString(vaFileName.bstrVal);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);
		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (vaFileName.bstrVal)
			::SysFreeString(vaFileName.bstrVal);
		TRACE("XML DOM error.\n");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);
		
		return FALSE;
	}
	
	m_bLoaded = TRUE;
	m_sFileName = strUrl;

	return TRUE;
}

//----------------------------------------------------------------------------
// La funzione GetRootElement restituisce un puntatore al nodo radice del
// documento XML. Se restituisce NULL vuol dire o che la radice non esiste 
// oppure che la funzione è fallita.
//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::GetRoot()
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (m_pRoot)
		return m_pRoot;
	
	IXMLDOMElement* pRootElement = NULL;
	if (FAILED(m_pXMLDoc->get_documentElement(&pRootElement)) || !pRootElement)
	{
		TRACE("Root element not found in XML document.");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"Root element not found in XML document.", CDiagnostic::Warning);
		if(pRootElement)
			pRootElement->Release();
		return NULL;
	}
	m_pRoot = new CXMLNode((IXMLDOMNode*)pRootElement, this); 
	pRootElement->Release();

	return m_pRoot; 
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::GetRootName(CString& strRootName)
{
	strRootName.Empty();

	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	if (!m_pRoot && !GetRoot())
		return FALSE;
	
	return m_pRoot->GetName(strRootName); 
}

//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLDocumentObject::GetRootChilds()
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	if (!m_pRoot && !GetRoot())
		return NULL;
	
	CXMLNodeChildsList* pRootChilds = m_pRoot->GetChilds();
	if (!pRootChilds)
	{
		TRACE("Root element childs of XML document not found.");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"Root element childs of XML document not found.", CDiagnostic::Warning);
		return NULL;
	}

	return pRootChilds; 
}

//----------------------------------------------------------------------------
int CXMLDocumentObject::GetRootChildsNum()
{
	if (
			!m_pXMLDoc	||
			(!m_pRoot && !GetRoot())
		)
	{
		ASSERT(FALSE);
		return -1;
	}
	
	return m_pRoot->GetChildsNum();
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::GetRootChildAt(int nIdx)
{
	CXMLNode* pNode = NULL;
	if (
			!m_pXMLDoc							||
			(!m_pRoot && !GetRoot())			||
			!(pNode = m_pRoot->GetChildAt(nIdx))
		)
	{
		ASSERT(FALSE);
		if (m_bMsgMode)
		{
			CString strMsg;
			strMsg.Format(L"Node indexed %d not found.", nIdx);
			AfxGetDiagnostic()->Add (strMsg, CDiagnostic::Warning);
		}
		return NULL;
	}

	return pNode;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::GetRootChildByName (LPCTSTR lpszChildName, BOOL bCaseSensitive /*= TRUE*/)
{
	if (!m_pXMLDoc || (!m_pRoot && !GetRoot()))
	{
		ASSERT(FALSE);
		return NULL;
	}

	return m_pRoot->GetChildByName(lpszChildName, bCaseSensitive);
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::GetFirstRootChild()
{
	if (
			!m_pXMLDoc							||
			(!m_pRoot && !GetRoot())			||
			!GetRootChildsNum()
		)
		return NULL;

	return m_pRoot->GetFirstChild();
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::GetNextRootChild()
{
	if (
			!m_pXMLDoc							||
			(!m_pRoot && !GetRoot())			||
			!GetRootChildsNum()
		)
		return NULL;

	return m_pRoot->GetNextChild();
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetRootChildNameAt(int nIdx)
{
	CXMLNode* pNode = GetRootChildAt(nIdx);
	if (!pNode)
		return _T("");
	CString strName;
	return pNode->GetName(strName) ? strName : _T("");
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetRootChildTextAt(int nIdx)
{
	CXMLNode* pNode = GetRootChildAt(nIdx);
	if (!pNode)
		return _T("");
	CString strText;
	return pNode->GetText(strText) ? strText : _T("");
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore alla lista dei nodi 
// aventi come nome di Tag la stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLDocumentObject::GetNodeListByTagName(LPCTSTR lpszTagName)
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	IXMLDOMNodeList* pNodeList = NULL;

	BSTR bstrTagName = CString(lpszTagName).AllocSysString();
	HRESULT hr = m_pXMLDoc->getElementsByTagName(bstrTagName, &pNodeList);
	::SysFreeString(bstrTagName);

	if (FAILED(hr) || !pNodeList)
	{
		if(pNodeList)
			pNodeList->Release ();
		return NULL;
	}
	
	return new CXMLNodeChildsList(pNodeList, this);
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore alla lista dei nodi 
// recuperati dalla stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
CXMLNodeChildsList* CXMLDocumentObject::SelectNodes(LPCTSTR lpszPattern, LPCTSTR lpszPrefix /*=NULL*/)
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	if(lpszPrefix && _tcslen(lpszPrefix)) AddSelectionNamespace (lpszPrefix, GetNamespaceURI());		
	
	IXMLDOMNodeList* pNodeList = NULL;

	BSTR bstrPattern = CString(lpszPattern).AllocSysString();
	HRESULT hr = m_pXMLDoc->selectNodes(bstrPattern, &pNodeList);
	::SysFreeString(bstrPattern);

	if (FAILED(hr) || !pNodeList)
	{
		if(pNodeList)
			pNodeList->Release ();
		return NULL;
	}	

	return new CXMLNodeChildsList(pNodeList, this);
}

//----------------------------------------------------------------------------
// ATTENZIONE: Questa funzione restituisce un puntatore al nodo 
// recuperato dalla stringa passata per argomento. Chi la chiama
// deve poi di preoccuparsi di liberare la memoria!!!!
//----------------------------------------------------------------------------
//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::SelectSingleNode(LPCTSTR lpszPattern, LPCTSTR lpszPrefix /*=NULL*/)
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return NULL;
	}

	if(lpszPrefix && _tcslen(lpszPrefix)) AddSelectionNamespace (lpszPrefix, GetNamespaceURI());		

	IXMLDOMNode* pNode = NULL;

	BSTR bstrPattern = CString(lpszPattern).AllocSysString();
	HRESULT hr = m_pXMLDoc->selectSingleNode(bstrPattern, &pNode);
	::SysFreeString(bstrPattern);

	if (FAILED(hr) || !pNode)
	{
		if(pNode)
			pNode->Release ();
		return NULL;
	}

	CXMLNode* pOutNode = new CXMLNode(pNode, this);
	pNode->Release();
	return pOutNode;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::CreateInitialProcessingInstruction(const CString strTarget /*="xml"*/, const CString strData /*="version=\"1.0\" encoding=\"UTF-8\""*/)
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr = S_FALSE;
	IXMLDOMNode* pOutNewChild = NULL;
	BSTR bstrTarget = NULL;
	BSTR bstrData = NULL;
	try
	{
		// The <?xml?> element is the standard XML declaration that enables
		// XML validation and specifies the XML version that you're using. 
		bstrTarget = strTarget.AllocSysString();

		// Although Unicode is a uniform character set representing nearly
		// all the world's languages, there are many byte representations,
		// or transformation formats, that a Unicode file can use.
		// The most popular format is Unicode Translation Format-8 (UTF-8),
		// which represents Unicode characters as a sequence of one to four
		// 8-bit bytes. 
		// Based on the XML 1.0 standard, Section 4.3.3, a valid XML file
		// is required to be one of following: 
		//      - A Unicode file in UTF-8 format.
		//      - A Unicode file in UTF-16 format.
		//      - A file in some other character encoding (for example,
		//        ASCII) that has as its very first bytes the <?xml?> tag,
		//        denoting the version and encoding of the current document.

		bstrData = strData.AllocSysString();

		IXMLDOMProcessingInstruction* pXMLPI = NULL;
		hr = m_pXMLDoc->createProcessingInstruction(bstrTarget,bstrData, &pXMLPI);

		::SysFreeString(bstrTarget);
		::SysFreeString(bstrData);
		
		if (FAILED(hr) || !pXMLPI)
		{
			if(pXMLPI)
				pXMLPI->Release ();
			return FALSE;
		}

		if (m_pRoot)
			hr = m_pXMLDoc->insertBefore(pXMLPI, _variant_t(m_pRoot->GetIXMLDOMNodePtr()), &pOutNewChild);
		else
			hr = m_pXMLDoc->appendChild(pXMLPI, &pOutNewChild);

		if(pXMLPI)
			pXMLPI->Release ();
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (bstrTarget)
			::SysFreeString(bstrTarget);
		if (bstrData)
			::SysFreeString(bstrData);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)			
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);
		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrTarget)
			::SysFreeString(bstrTarget);
		if (bstrData)
			::SysFreeString(bstrData);

		TRACE("XML DOM error.\n");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);

		
		return FALSE;
	}
	
	if(pOutNewChild)
		pOutNewChild->Release ();

	return (SUCCEEDED(hr) && pOutNewChild != NULL);
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::InsertComment(LPCTSTR lpszComment) 
{
	if (!m_pXMLDoc  || !m_pRoot  || !lpszComment)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BSTR bstrComment = NULL;
	IXMLDOMComment* pComment = NULL;
	try
	{
		SetBSTRText(lpszComment, bstrComment);

		HRESULT hr = m_pXMLDoc->createComment(bstrComment, &pComment);

		::SysFreeString(bstrComment);

		if (FAILED(hr) || !pComment)
		{
			if(pComment)
				pComment->Release ();

			TRACE("Failed to insert comment.");
			if (m_bMsgMode)
				AfxGetDiagnostic()->Add(L"Failed to insert comment.", CDiagnostic::Warning);

			return FALSE;
		}
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (bstrComment)
			::SysFreeString(bstrComment);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);

		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrComment)
			::SysFreeString(bstrComment);

		TRACE("XML DOM error.\n");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);

		return FALSE;
	}
	
	IXMLDOMNode* pOutComment=NULL;
	BOOL bResult = pComment ? SUCCEEDED(m_pXMLDoc->appendChild(pComment, &pOutComment)) : FALSE;
	if(pComment)
		pComment->Release ();
	if(pOutComment)
		pOutComment->Release ();
	return bResult;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::CreateElement
								(
									LPCTSTR		lpszTagName, 
									CXMLNode*	pParentNode			/* = NULL*/,
									LPCTSTR		lpszNameSpaceURI	/* = NULL*/, 
									LPCTSTR		lpszPrefix			/* = NULL*/
								)
{
	if (!m_pXMLDoc || !lpszTagName || (pParentNode && !pParentNode->GetIXMLDOMNodePtr()))
	{
		ASSERT(FALSE);
		return NULL;
	}
	IXMLDOMNode* pOutNewChild = NULL;
	BSTR bstrTagName = NULL;
	try
	{
		IXMLDOMNode* pNewNode = NULL;

		CString strNode = lpszPrefix ? lpszPrefix : (m_bstrNameSpaceURI ? m_strPrefix : _T(""));
		if(!strNode.IsEmpty())
			bstrTagName = (strNode + _T(":") + lpszTagName).AllocSysString();
		else
			SetBSTRText(lpszTagName, bstrTagName);
		
		HRESULT hr = S_FALSE;
		VARIANT vaNodeType;
		VariantInit(&vaNodeType);
		vaNodeType.vt = VT_I4;
		vaNodeType.lVal = NODE_ELEMENT;
		
		if (lpszNameSpaceURI)
		{
			BSTR bstrNameSpaceURI;
			SetBSTRText(lpszNameSpaceURI, bstrNameSpaceURI);
			
			hr = m_pXMLDoc->createNode
								(
									vaNodeType,
									bstrTagName,
									bstrNameSpaceURI,
									&pNewNode
									);

			SysFreeString(bstrNameSpaceURI);
		}
		else if (m_bstrNameSpaceURI)
		{
			hr = m_pXMLDoc->createNode
								(
									vaNodeType,
									bstrTagName,
									m_bstrNameSpaceURI,
									&pNewNode
								);
		}
		else
		{
			IXMLDOMElement*	pNewElement = NULL;
			hr = m_pXMLDoc->createElement(bstrTagName, &pNewElement);
			pNewNode = (IXMLDOMNode*)pNewElement;
		}
		::SysFreeString(bstrTagName);

		if (FAILED(hr) || !pNewNode)
		{
			if(pNewNode)
				pNewNode->Release();
			return NULL;
		}

		if (pParentNode && pParentNode->GetIXMLDOMNodePtr() != m_pXMLDoc)
			hr = pParentNode->GetIXMLDOMNodePtr()->appendChild(pNewNode, &pOutNewChild);
		else
			hr = m_pXMLDoc->appendChild(pNewNode, &pOutNewChild);

		pNewNode->Release();

		if (FAILED(hr) || !pOutNewChild)
		{
			if(pOutNewChild)
				pOutNewChild->Release();
			return NULL;
		}
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (bstrTagName)
			::SysFreeString(bstrTagName);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);
		e->Delete();

		return NULL;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrTagName)
			::SysFreeString(bstrTagName);
		TRACE("XML DOM error.\n");

		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);

		return NULL;
	}
	
	if (!pOutNewChild)
		return NULL;

	CXMLNode* pOutNode = new CXMLNode(pOutNewChild, this); 
	pOutNewChild->Release();
	return pOutNode;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::RemoveNode	(CXMLNode* pNodeToRemove)
{
	if (!m_pXMLDoc || !pNodeToRemove || !pNodeToRemove->GetIXMLDOMNodePtr())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CXMLNode* pParentNode = pNodeToRemove->GetParentNode();
	if (!pParentNode)
		return FALSE;

	BOOL bRc = pParentNode->RemoveChild(pNodeToRemove);

	return bRc;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::CreateRoot
								(
									LPCTSTR lpszRootTagName, 
									LPCTSTR lpszNameSpaceURI /* = NULL*/, 
									LPCTSTR lpszPrefix /* = NULL*/
								)
{
	if (!m_pXMLDoc || !lpszRootTagName)
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (m_pRoot)
	{
		ASSERT (FALSE);
		delete m_pRoot;
		m_pRoot = NULL;
	}
	m_pRoot = CreateElement(lpszRootTagName, NULL, lpszNameSpaceURI, lpszPrefix);

	return m_pRoot;
}

//----------------------------------------------------------------------------
CXMLNode* CXMLDocumentObject::CreateRootChild
								(
									LPCTSTR lpszTagName, 
									LPCTSTR lpszNameSpaceURI /* = NULL*/, 
									LPCTSTR lpszPrefix /* = NULL*/
								)
{
	if (!m_pXMLDoc  || !m_pRoot  || !lpszTagName)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return m_pRoot->CreateNewChild(lpszTagName, lpszNameSpaceURI, lpszPrefix);
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::GetXML(CString& strXML) const
{
	strXML.Empty ();

	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrXML=NULL;
	BOOL bOk = TRUE;

	if (FAILED(m_pXMLDoc->get_xml(&bstrXML)))
		bOk = FALSE;
	else
		strXML = bstrXML; 
	
	if (bstrXML)
		::SysFreeString(bstrXML);
	
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SaveXMLFile(const CString& strFileName, BOOL bCreatePath /*= FALSE*/)
{
	if (!m_pXMLDoc || strFileName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bOk =  FALSE;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(strFileName))
	{
		CString sContent;
		this->GetXML (sContent);
		bOk = pFileSystemManager->SaveTextFile (strFileName, sContent);
		if (bOk && AfxGetPathFinder()->IsCustomPath(strFileName))
		{
			//Aggiunge il file alla customizzazione corrente
			AfxAddFileToCustomizationContext(strFileName);
		}
		return bOk;
	}

	if (GetIndent() && SaveFormattedXML (strFileName, bCreatePath))
	{
		m_sFileName = strFileName;
		if (AfxGetPathFinder()->IsCustomPath(strFileName))
		{
			//Aggiunge il file alla customizzazione corrente
			AfxAddFileToCustomizationContext(strFileName);
		}
		return TRUE;
	}
		
	HRESULT hr = S_FALSE;
	VARIANT vaFileName;
	VariantInit(&vaFileName);
	vaFileName.vt = VT_BSTR;
	vaFileName.bstrVal = strFileName.AllocSysString();
	try
	{
		if (bCreatePath && !CreateFileDirectoryIfNecessary(strFileName))
		{
			CString strMsg;
			strMsg.Format(L"Creation of file path \n%s\n failed.", (LPCTSTR)strFileName);
			AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		}
		hr = m_pXMLDoc->save(vaFileName);

		::SysFreeString(vaFileName.bstrVal);
		
		if (FAILED(hr) && m_bMsgMode)
		{
			CString strMsg;
			switch(hr)
			{
				// @@TODO: XML_BAD_ENCODING risulta non definito...
				//case XML_BAD_ENCODING:
					// The document contains a character that does not
					// belong in the specified encoding. The character 
					// must use a numeric entity reference. For example,
					// the Japanese Unicode character 20013 does not fit
					// into the encoding windows-1250 (which is the Central 
					// European alphabet) and therefore must be represented
					// in markup as the numeric entity reference &#20013; 
					// or &#x4E2D;. This version of save does not automatically
					// convert characters to the numeric entity references. 
				//	break;
				case E_INVALIDARG:
					// A string was provided but it is not a valid file name. 
					strMsg.Format(L"Failed to save XML document; file name %s is not valid.\n", (LPCTSTR)strFileName);
					break;
				case E_ACCESSDENIED:
					// Save operation is not permitted. 
					strMsg.Format(L"Failed to save XML document: access denied for file %s.\n", (LPCTSTR)strFileName);
					break;
				case E_OUTOFMEMORY:
					// Save does need to allocate buffers.
					strMsg = L"Failed to save XML document: insufficient memory.\n";
					break;
				default:
					GetParseErrorString(strMsg);
					if (strMsg.IsEmpty())
						strMsg = "Failed to save XML document.\n";
					break;
			} 
			AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		}
	}
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (vaFileName.bstrVal)
			::SysFreeString(vaFileName.bstrVal);
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);

		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (vaFileName.bstrVal)
			::SysFreeString(vaFileName.bstrVal);

		TRACE("XML DOM error.\n");
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);

		return FALSE;
	}

	m_sFileName = strFileName;
	BOOL b = (SUCCEEDED(hr));
	//comunico al menu di rinfrescare la vista sui report custom
	if (b && AfxGetPathFinder()->IsCustomPath(strFileName))
	{
		//Aggiunge il file alla customizzazione corrente
		AfxAddFileToCustomizationContext(strFileName);
	}
	return b;
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::SaveFormattedXML(const CString& strFileName, BOOL bCreatePath /*= FALSE*/)
{
	IXSLProcessor *pIXSLProcessor = NULL;

	IXSLTemplate *pTemplate = NULL; 

	BOOL bRetVal = TRUE;
	try
	{
		HRESULT hr;

		hr = CoCreateInstance (	CLSID_XSLTemplate60, 
										NULL, 
										CLSCTX_INPROC_SERVER,
										IID_IXSLTemplate, 
										(LPVOID*)(&pTemplate));
		
		if(FAILED(hr) || !pTemplate) 
			throw hr;
		
		CXMLDocumentObject xslDoc (FALSE, FALSE, TRUE);
		if (!xslDoc.LoadXML (ESCAPING_XSLT))
		{
			TRACE("It's impossible to load the trasformation sheet"); 
			throw -1;
		}	

		CString strEncoding = GetEncoding();
		if(strEncoding.IsEmpty())
			strEncoding = DEFAULT_ENCODING;

		CXMLNode *pOutput = xslDoc.GetRootChildByName (_T("xsl:output"));
		if(pOutput)
			pOutput->SetAttribute (_T("encoding"), strEncoding);

		hr = pTemplate->putref_stylesheet(xslDoc.GetIXMLDOMDocumentPtr());
		if (FAILED(hr)) throw hr;
			
		hr = pTemplate->createProcessor(&pIXSLProcessor);
		if (FAILED(hr) || !pIXSLProcessor) throw hr;
		
		CXMLDocumentObject docOutput(FALSE, FALSE, FALSE);

		hr = pIXSLProcessor->put_input(_variant_t (m_pXMLDoc));
		if (FAILED(hr)) throw hr;

		hr = pIXSLProcessor->put_output(_variant_t (docOutput.GetIXMLDOMDocumentPtr ()));
		if (FAILED(hr)) throw hr;

		VARIANT_BOOL bResult;
		hr = pIXSLProcessor->transform(&bResult);
		if ( FAILED(hr) || bResult == VARIANT_FALSE ) throw hr;	

		docOutput.SetIndent (FALSE);
		bRetVal = docOutput.SaveXMLFile (strFileName, bCreatePath);
	}
	catch(...)
	{
		ASSERT(FALSE);
		TRACE("Document format failed\n");
		bRetVal = FALSE;
	}

	if (pTemplate)
	{
		pTemplate->Release();
		pTemplate = NULL;
	}

	if (pIXSLProcessor)
	{
		pIXSLProcessor->Release();
		pIXSLProcessor = NULL;
	}

	return bRetVal;
	
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetEncoding()
{
	CString strEncoding;

	IXMLDOMNode *pNode = NULL;		
	IXMLDOMProcessingInstruction* pInstruction = NULL;

	try
	{
		HRESULT hr = m_pXMLDoc->get_firstChild(&pNode);
		DOMNodeType type;
		hr = pNode->get_nodeType (&type);

		if(FAILED(hr))
			throw hr;

		if(type == NODE_PROCESSING_INSTRUCTION)
		{
			hr = pNode->QueryInterface(IID_IXMLDOMProcessingInstruction, (void**)&pInstruction);
			if(FAILED(hr) || !pInstruction)
				throw hr;

			BSTR bstrTarget = NULL;
			hr = pInstruction->get_target(&bstrTarget);
			
			if(FAILED(hr))
				throw hr;

			CString strTarget = CString(bstrTarget);
			
			if(bstrTarget)
				::SysFreeString(bstrTarget);

			if(_tcsicmp(strTarget, _T("xml")) == 0)
			{
				BSTR bstrData= NULL;
				hr = pInstruction->get_data(&bstrData);
				if(FAILED(hr))
					throw hr;

				CString strData = CString(bstrData);

				if(bstrData)
					::SysFreeString(bstrData);

				strData = strData.MakeLower();
				int nPos = strData.Find (_T("encoding"));
				if(nPos != -1)
				{
					int nStart = strData.Find (_T("\""), nPos);
					int nEnd = strData.Find (_T("\""), nStart+1);
					strEncoding = strData.Mid (nStart+1, nEnd-nStart-1);
				}			
			}			
		}
	}
	catch(...)
	{
		TRACE("It's impossible to resolve the document encoding");
	}

	if(pNode)
		pNode->Release();

	if(pInstruction)
		pInstruction->Release();

	return strEncoding;
}

//----------------------------------------------------------------------------
///questo metodo va invocato per ovviare ad un funzionamento un po' "eccentrico" del DOM 4 
// (secondo Microsoft, corrispondente alle specifiche W3C);
// se viene utilizzato un namespace di default (cioè senza prefisso identificativo), infatti, 
// non funzionano più le query XPath: bisogna in tal caso attribuire un prefisso 
// a tale namespace attraverso questo metodo, quindi effettuare la query XPath anteponendo tale
// prefisso al nome del nodo cercato
BOOL CXMLDocumentObject::AddSelectionNamespace (LPCTSTR lpszPrefix, LPCTSTR lpszNameSpace)
{
	if (!m_pXMLDoc || !lpszPrefix || !lpszNameSpace)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strPrefix = lpszPrefix;
	
	if(strPrefix.IsEmpty ()) return FALSE;

	int pos = strPrefix.GetLength ()-1;
	if(strPrefix.ReverseFind (_T(':')) == pos)
		strPrefix = strPrefix.Left (pos);

	CString strProperty = XML_NAMESPACEURI_TAG;
	strProperty += strPrefix;
	strProperty += _T("='");
	strProperty +=lpszNameSpace;
	strProperty += _T("'");
	
	BSTR name;
	SetBSTRText(SELECTION_NAMESPACES, name);
	
	VARIANT vaNamespaces;
	VariantInit(&vaNamespaces);
	CString strNamespaces;
	// mi faccio prima restituire i namespace già associati
	HRESULT hr = m_pXMLDoc->getProperty(name, &vaNamespaces);
	if (SUCCEEDED(hr))
	{
		strNamespaces += vaNamespaces.bstrVal;
		if (strNamespaces.Find(strProperty) >= 0)
		{
			VariantClear(&vaNamespaces);
			::SysFreeString(name);
			return TRUE;
		}
		if (!strNamespaces.IsEmpty())
			strNamespaces += _T(" ");	
	}
	
	strNamespaces += strProperty;
	
	SetVariantText(strNamespaces, vaNamespaces);	
	
	hr = m_pXMLDoc->setProperty(name, vaNamespaces);
	
	::SysFreeString(name);
	VariantClear(&vaNamespaces);

	return SUCCEEDED(hr);
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetNamespaceURI() 
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return _T("");
	}

	CString strURI;	
	if (m_bstrNameSpaceURI != NULL)
	{
		strURI = m_bstrNameSpaceURI;
		if(!strURI.IsEmpty())
			return strURI;
	}
		
	BSTR bstrURI = NULL;
	if(SUCCEEDED(m_pXMLDoc->get_namespaceURI(&bstrURI)))
	{
		strURI = bstrURI;
		if(bstrURI) SysFreeString(bstrURI);
		if(!strURI.IsEmpty ())
			return strURI;
	}

	CXMLNode *pNode = GetRoot();
	return pNode? pNode->GetNamespaceURI() : _T("");
}

//----------------------------------------------------------------------------
CString CXMLDocumentObject::GetPrefix() 
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrPrefix = NULL;
	CString strPrefix;
	if(SUCCEEDED(m_pXMLDoc->get_prefix(&bstrPrefix)))
	{
		strPrefix = bstrPrefix;
		if(bstrPrefix) SysFreeString(bstrPrefix);
		if(!strPrefix.IsEmpty ())
			return strPrefix;
	}

	CXMLNode *pNode = GetRoot();
	return pNode? pNode->GetPrefix() : _T("");
}

//----------------------------------------------------------------------------
BOOL CXMLDocumentObject::GetParseErrorString(CString& strError, IXMLDOMParseError* pParseError /*=NULL*/) const
{
	if (!m_pXMLDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}
    strError.Empty();
	
	IXMLDOMParseError*	pError = pParseError;
	if(pError)
		pError->AddRef();

	BSTR				bstrReason	= NULL;
	BSTR				bstrURL		= NULL;
	BSTR				bstrText	= NULL;
	try
	{
		if (!pError && FAILED(m_pXMLDoc->get_parseError(&pError)))
			return FALSE;

		if (SUCCEEDED(pError->get_reason(&bstrReason)) && bstrReason)
			strError = bstrReason; 
		if (bstrReason)
			::SysFreeString(bstrReason);

		if (SUCCEEDED(pError->get_url(&bstrURL)) && bstrURL)
			strError += _T("\nURL: ") + CString(bstrURL); 
		if (bstrURL)
			::SysFreeString(bstrURL);
 
		long lPos,lLine = 0;
		if 
			(
				SUCCEEDED(pError->get_line(&lLine))			&&
				lLine > 0									&&
				SUCCEEDED(pError->get_srcText(&bstrText))	&&
				bstrText									&&
				SUCCEEDED(pError->get_linepos(&lPos))
			)
		{
			CString strAdditionalInfo;
			strAdditionalInfo.Format(_T("\nLinea %d, Posizione %d "), lLine, lPos);
			strError += strAdditionalInfo;

			CString strText = CString(bstrText);
			long lLen = strText.GetLength();
			for (int i = 0; i < lLen; i++)
			{
				if (strText[i] == _T('\t'))
					strError += _T(' ');
				else
					strError += strText[i];
			}

		}
		if (bstrText)
			::SysFreeString(bstrText);
    
		if (pError) 
			pError->Release();
	}	
	catch (CException* e) // Catch all MFC exceptions, including COleExceptions.
	{
		if (bstrReason)
			::SysFreeString(bstrReason);
		if (bstrURL)
			::SysFreeString(bstrURL);
		if (bstrText)
			::SysFreeString(bstrText);
		if (pError) 
			pError->Release();
		
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE(szError);
		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(szError, CDiagnostic::Warning);

		e->Delete();
		
		return FALSE;
	}
	catch(...)	// The catch clause handles any type of exception.
				// An ellipsis catch handler must be the last handler for its try block.
	{
		if (bstrReason)
			::SysFreeString(bstrReason);
		if (bstrURL)
			::SysFreeString(bstrURL);
		if (bstrText)
			::SysFreeString(bstrText);
		if (pError) 
			pError->Release();
		
		TRACE("XML DOM error.\n");

		if (m_bMsgMode)
			AfxGetDiagnostic()->Add(L"XML DOM error.\n", CDiagnostic::Warning);

		return FALSE;
	}
	
	return !strError.IsEmpty();
}

