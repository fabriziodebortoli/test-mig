#include "StdAfx.h"


#include <msxml6.h>
#include <comutil.h>

#include <shlwapi.h>

#include ".\dictionaryclasses.h"
#include ".\tbstringloader.h"
#include ".\stringloader.h"
#include ".\const.h"

#define szInvalidDictionary "Error parsing dictionary binary file"

//--------------------------------------------------------------------------------
UINT CDictionaryParser::Read(BYTE* lpBuf, UINT nCount)
{
	if (m_nPosition == m_nBufferLength)
	{
		m_nPosition = 0;
		m_nBufferLength = m_File.Read(m_pBuffer, DICTIONARY_BUFFER_SIZE);
	}

	if (m_nPosition == m_nBufferLength)
		return 0;

	int nRealCount = min (nCount, m_nBufferLength - m_nPosition);
	memcpy(lpBuf, m_pBuffer + m_nPosition, nRealCount);
	m_nPosition += nRealCount;

	if (nRealCount == nCount)
		return nRealCount;

	return nRealCount + Read(lpBuf + nRealCount, nCount - nRealCount);
}

//--------------------------------------------------------------------------------
UINT CDictionaryParser::ParseUInt()
{	
	UINT n;
	if (Read((BYTE*)&n, NUMBER_LENGTH) != NUMBER_LENGTH)
		throw szInvalidDictionary;

	return n;
}

//--------------------------------------------------------------------------------
BYTE CDictionaryParser::ParseByte()
{
	BYTE byte;
	if (Read(&byte, 1) != 1)
		throw szInvalidDictionary;

	return byte;
}

//--------------------------------------------------------------------------------
CString CDictionaryParser::ParseString()
{
	UINT len = ParseUInt();
	if (len == 0)
		return _T("");

	char* buff = (char*)alloca(len);
	
	if (Read((BYTE*)buff, len) != len)
		throw szInvalidDictionary;

	int n = (len + 1 ) * sizeof(wchar_t);
	wchar_t *pwBuff = (wchar_t *) alloca(n);
	int m_nCount = MultiByteToWideChar(CP_UTF8, 0, buff, len, pwBuff, n);
	pwBuff[m_nCount] = L'\0';

	return CString(pwBuff);

}

//--------------------------------------------------------------------------------
BOOL GetAttribute(IXMLDOMElement *pElement, LPCTSTR strName, CString &strAttrValue)
{
	_variant_t value;
	CComBSTR n (strName);
	if (FAILED(pElement->getAttribute(n, &value)) || value.vt == VT_NULL)
		return FALSE;

	strAttrValue = value;
	return !strAttrValue.IsEmpty();
}
//--------------------------------------------------------------------------------
BOOL GetAttribute(IXMLDOMNode *pNode, LPCTSTR strName, CString &strAttrValue)
{
	CComPtr<IXMLDOMElement> pElement = NULL;
	if (FAILED (pNode->QueryInterface(IID_IXMLDOMElement, (void**) &pElement)) || pElement == NULL)
		return FALSE;

	return ::GetAttribute(pElement, strName, strAttrValue);
}

//===========================================================================
// CGenericContainer
//===========================================================================

//--------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CGenericContainer, CMapStringToOb)

//--------------------------------------------------------------------------------
void CGenericContainer::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{	
		ar << m_nCount;
		POSITION pos = GetStartPosition();
		CString strKey;
		CObject *pObject;
		while (pos)
		{
			GetNextAssoc(pos, strKey, pObject);
			ar << strKey;
			pObject->Serialize(ar);
		}
	}
	else
	{	
		int nCount;
		ar >> nCount;
		CString strKey;
		CObject *pObject;
		for (int i = 0; i < nCount; i++)
		{
			ar >> strKey;
			pObject = CreateSerializedObject();
			pObject->Serialize(ar);
			SetAt(strKey, pObject);
		}

	}
}

//--------------------------------------------------------------------------------
CGenericContainer::~CGenericContainer()
{
	DestroyObjects();
}

//--------------------------------------------------------------------------------
void CGenericContainer::SetAt(LPCTSTR key, CObject *newValue)
{
	CObject* pCurrent = (*this)[key];
	if (pCurrent != NULL && pCurrent != newValue)
	{
		ASSERT(FALSE);
		delete pCurrent;
	}
	__super::SetAt(key, newValue);
}

//--------------------------------------------------------------------------------
void CGenericContainer::DestroyObjects()
{
	POSITION pos = GetStartPosition();
	CObject *pObject;
	CString strKey;
	while(pos)
	{
		GetNextAssoc(pos, strKey, pObject);
		delete pObject;
	}
}

#ifdef _DEBUG
//--------------------------------------------------------------------------------
void CGenericContainer::Dump(CDumpContext& dc) const
{
	__super::Dump(dc);
}

//--------------------------------------------------------------------------------
void CGenericContainer::AssertValid() const
{
	__super::AssertValid();
}
#endif

//--------------------------------------------------------------------------------
IMPLEMENT_SERIAL(CStringItem, CGenericContainer, SERIALIZATION_VERSION)
//------------------------------------------------------------------------------
void CStringItem::Serialize(CArchive& ar)
{
	__super::Serialize(ar);
	if (ar.IsStoring())
	{
		ar << m_strTarget;
	}
	else
	{
		ar >> m_strTarget;
	}

	m_strAdditionalParams.Serialize(ar);
}

//--------------------------------------------------------------------------------
CStringItem* CStringItem::Parse(CDictionaryParser *pParser, CString &strBase)
{
	strBase = pParser->ParseString();
	CStringItem *pItem = new CStringItem();
	pItem->m_strTarget = pParser->ParseString();

	BYTE nAttributes = pParser->ParseByte();
	for (int i = 0; i < nAttributes; i++)
	{
		CString strAttrName = pParser->ParseString();
		CString strAttrValue = pParser->ParseString();
		pItem->m_strAdditionalParams[strAttrName] = strAttrValue;
	}

	return pItem;
}
//--------------------------------------------------------------------------------
CStringItem* CStringItem::Parse(IXMLDOMNode* pStringNode, CString &strBase)
{
	if (!pStringNode)
		return NULL;
	
	CComPtr<IXMLDOMElement> pElement = NULL;
	if (FAILED (pStringNode->QueryInterface(IID_IXMLDOMElement, (void**) &pElement)) || pElement == NULL)
		return NULL;

	CString strValid;
	
	if (::GetAttribute(pElement, VALID_ATTRIBUTE, strValid) &&
		strValid.CompareNoCase(XML_FALSE) == 0)
		return NULL;
	
	if (!::GetAttribute(pElement, BASE_ATTRIBUTE, strBase) || strBase.IsEmpty())
		return NULL;

	strBase.Trim();
	strBase.Replace(_T("\r\n"), _T("\n"));
	
	CStringItem * pItem = new CStringItem();
	
	::GetAttribute(pElement, TARGET_ATTRIBUTE, pItem->m_strTarget);

	pItem->ParseAttribute(pElement, X_ATTRIBUTE);
	pItem->ParseAttribute(pElement, Y_ATTRIBUTE);
	pItem->ParseAttribute(pElement, H_ATTRIBUTE);
	pItem->ParseAttribute(pElement, W_ATTRIBUTE);
	pItem->ParseAttribute(pElement, FONTNAME_ATTRIBUTE);
	pItem->ParseAttribute(pElement, FONTSIZE_ATTRIBUTE);
	pItem->ParseAttribute(pElement, FONTBOLD_ATTRIBUTE);
	pItem->ParseAttribute(pElement, FONTITALIC_ATTRIBUTE);
	pItem->ParseAttribute(pElement, CURRENT_ATTRIBUTE);

	return pItem;
}
//--------------------------------------------------------------------------------
BOOL CStringItem::GetAttribute(const CString &strName, CString &strValue)
{
	strValue = m_strAdditionalParams[strName];
	return !strValue.IsEmpty();
}
IMPLEMENT_SERIAL(CStringBlock, CGenericContainer, SERIALIZATION_VERSION)
//--------------------------------------------------------------------------------
BOOL CStringBlock::ParseString(const CString &strXml, CString &strName)
{
	CComPtr<IXMLDOMDocument2>	pDomReader;
	if (FAILED (pDomReader.CoCreateInstance(__uuidof(DOMDocument60))))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	VARIANT_BOOL b;
	if (FAILED (pDomReader->loadXML(CComBSTR(strXml), &b))
		||
		b == VARIANT_FALSE
		)
		return FALSE;
	
	
	CComPtr<IXMLDOMElement> pRoot = NULL;
	if (FAILED(pDomReader->get_documentElement(&pRoot)) 
		||
		pRoot == NULL
		)
		return FALSE;
	

	CString strValid;
	if (!::GetAttribute(pRoot, NAME_ATTRIBUTE, strName))
		return FALSE;

	if (::GetAttribute(pRoot, VALID_ATTRIBUTE, strValid) && 
		strValid.CompareNoCase(XML_FALSE) == 0)
		return FALSE;

	CString strBase;
	CComPtr<IXMLDOMNodeList> pStringNodes = NULL;
	if (FAILED(pRoot->get_childNodes(&pStringNodes)) || pStringNodes == NULL) 
		return FALSE;
	
	long length;
	pStringNodes->get_length(&length);
	for (int j = 0; j < length; j++)
	{
		CComPtr<IXMLDOMNode> pChildStringNode = NULL;
		if (FAILED(pStringNodes->get_item(j, &pChildStringNode)) || pChildStringNode == NULL)
			continue;
		CStringItem * pItem = CStringItem::Parse(pChildStringNode, strBase);
		if (pItem)
			SetAt(strBase, pItem);
	}

	return TRUE;
}

//--------------------------------------------------------------------------------
void CStringBlock::Parse(CDictionaryParser *pParser)
{
	UINT count = pParser->ParseUInt();
			
	for (UINT i = 0; i < count; i++)
	{
		CString strBase;
		CStringItem *pItem = CStringItem::Parse(pParser, strBase);
		SetAt(strBase, pItem);
	}
}

//--------------------------------------------------------------------------------
CStringItem* CStringBlock::FindByAttributeValue(
	const CString &strAttrName, 
	const CString &strAttrValue,
	CString& strKeyName
	)
{
	POSITION pos = GetStartPosition();
	CStringItem *pItem;
	CString strValue;
	while(pos)
	{
		GetNextAssoc(pos, strKeyName, (CObject*&) pItem);
		if (pItem->GetAttribute(strAttrName, strValue) &&
			strValue == strAttrValue)
			return pItem;
	}
	return NULL;
}


IMPLEMENT_SERIAL(CStringBlockContainer, CGenericContainer, SERIALIZATION_VERSION)
//--------------------------------------------------------------------------------
void CStringBlockContainer::Parse(CDictionaryParser *pParser)
{
	try
	{
		UINT localVersion = pParser->ParseUInt();
		if (localVersion != DICTIONARY_VERSION)
			return;
		
		UINT count = pParser->ParseUInt();
		for (UINT i = 0; i < count; i++)
		{
			CResourceIndexItem *pItem = new CResourceIndexItem();
			pItem->Parse(pParser);
			m_arResourceIndexItems.Add(pItem);
		}

		count = pParser->ParseUInt();
		for (UINT i = 0; i < count; i++)
		{
			CIndexItem item;
			item.Parse(pParser);
			CStringBlock *pBlock = new CStringBlock();
			pBlock->Parse(pParser);

			SetAt(item.ToString(), pBlock);
		}
	}
	catch (...)
	{
		RemoveAll();
	}
}

//--------------------------------------------------------------------------------
CResourceIndexItem* CStringBlockContainer::ItemFromNumber(CString strNumber) const
{
	UINT nNumber = _ttoi(strNumber);
	for (int i = 0; i < m_arResourceIndexItems.GetSize(); i++)
	{
		CResourceIndexItem *pItem = (CResourceIndexItem *) m_arResourceIndexItems[i];
		if (pItem->Match(nNumber))
			return pItem;
	}
	return NULL;
}

//--------------------------------------------------------------------------------
void CStringBlockContainer::ParseDictionary(const CString& strDictionaryFile)
{
	m_strFilePath = strDictionaryFile;
	if (!PathFileExists(m_strFilePath))
		return;

	CDictionaryParser* parser = new CDictionaryParser(m_strFilePath);
	Parse(parser);
	delete parser;
}

//--------------------------------------------------------------------------------
CStringBlock* CStringBlockContainer::FindStringBlock(const CString &strType, const CString &strId, const CString &strName, CString &strKey) const
{
	if (strType == STRINGTABLE_TYPE)
	{
		CResourceIndexItem *pItem = ItemFromNumber(strName);
		if (!pItem)
			return NULL;
	
		strKey = CIndexItem::ToString(strType, pItem->GetUrl(), _T(""));
	}
	else
	{
		strKey = CIndexItem::ToString(strType, strId, strName);
	}
	
	ASSERT(!strKey.IsEmpty());

	CStringBlock * pBlock = NULL;
	Lookup(strKey, pBlock);
	return pBlock;
}

//--------------------------------------------------------------------------------
void CStringBlockContainer::DestroyObjects()
{
	POSITION pos = GetStartPosition();
	CObject *pObject;
	CString strKey;
	while(pos)
	{
		GetNextAssoc(pos, strKey, pObject);

		CStringBlock *pBlock = (CStringBlock *)pObject;
		if (pBlock && !pBlock->m_bUsed)
			delete pBlock;
	}
}

//--------------------------------------------------------------------------------
void CStringBlockContainer::RemoveAll()
{
	DestroyObjects();
	__super::RemoveAll();
}



//--------------------------------------------------------------------------------
IMPLEMENT_SERIAL(CResourceIndexItem, CObject, SERIALIZATION_VERSION)
//--------------------------------------------------------------------------------
void CResourceIndexItem::Serialize(CArchive& ar)
{
	__super::Serialize(ar);
	if (ar.IsStoring())
	{
		ar << m_strUrl;
		ar << m_nNumber;
	}
	else
	{
		ar >> m_strUrl;
		ar >> m_nNumber;
	}
}

//--------------------------------------------------------------------------------
IMPLEMENT_SERIAL(CUsedStringBlockContainer, CStringBlockContainer, SERIALIZATION_VERSION)
//--------------------------------------------------------------------------------
CStringBlock * CUsedStringBlockContainer::FindStringBlock(CStringLoader *pLoader, const CString &strType, const CString &strId, const CString &strName)
{
	CString strKey;
	CSingleLock lock(&m_CriticalSection, TRUE);

	CStringBlock *pBlock = __super::FindStringBlock(strType, strId, strName, strKey);
	if (pBlock)
		return pBlock;

	CStringBlockContainer *pContainer = pLoader->GetDictionary(m_strDictionaryFile);

	pBlock = pContainer->FindStringBlock(strType, strId, strName, strKey);
	
	//se la chiave non viene valorizzata dalla FindStringBlock, significa che 
	//la risorsa non va messa nella struttura
	if (strKey.IsEmpty())
		return NULL;

	if (!pBlock)
		pBlock = new CStringBlock(); // empty string block

	if (strType == STRINGTABLE_TYPE)
		AddResourceIndexItem(pContainer->ItemFromNumber(strName));
	
	
	SetAt(strKey, pBlock);
	pBlock->m_bUsed = TRUE;
	return pBlock;
}

//------------------------------------------------------------------------------
void CUsedStringBlockContainer::Serialize(CArchive& ar)
{
	__super::Serialize(ar);
	m_arResourceIndexItems.Serialize(ar);
	if (ar.IsStoring())
	{
		ar << m_strDictionaryFile;
	}
	else
	{
		ar >> m_strDictionaryFile;
	}
}
//--------------------------------------------------------------------------------
CStringBlockContainer* CDictionaryBinaryFileLRU::GetDictionary(const CString& strFilePath)
{
	POSITION pos = GetTailPosition();
	POSITION currPos;
	while (pos)
	{
		currPos = pos;
		CStringBlockContainer* pContainer = (CStringBlockContainer*) GetPrev(pos);
		if (strFilePath.CompareNoCase(pContainer->GetFilePath()) != 0)
			continue;

		RemoveAt(currPos);
		AddTail(pContainer);
		return pContainer;
	}

	CStringBlockContainer *pContainer = new CStringBlockContainer();
	pContainer->ParseDictionary(strFilePath);

	if (GetCount() == DICTIONARY_LRU_CACHE_SIZE)
	{
		CStringBlockContainer *p = (CStringBlockContainer *) RemoveHead();
		p->RemoveAll();
		delete p;

	}

	AddTail(pContainer);
	return pContainer;
}

//--------------------------------------------------------------------------------
CDictionaryBinaryFileLRU::~CDictionaryBinaryFileLRU()
{
	RemoveAll();
}

//--------------------------------------------------------------------------------
void CDictionaryBinaryFileLRU::RemoveAll()
{
	POSITION pos = GetHeadPosition();
	while (pos)
	{
		CStringBlockContainer *p = (CStringBlockContainer *) GetNext(pos);
		p->RemoveAll();
		delete p;
	}

	__super::RemoveAll();
}
