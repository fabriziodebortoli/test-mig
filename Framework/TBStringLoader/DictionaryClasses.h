#pragma once

#include <afxmt.h>

#define NUMBER_LENGTH		4

class CStringLoader;

template <class T>
class CSerializableArray : public CObArray
{
public:
	virtual void Serialize(CArchive& ar)
	{
		if (ar.IsStoring())
		{	
			ar << GetCount();
			for (int i = 0; i < GetCount(); i++)
				((T*) GetAt(i))->Serialize(ar);
		}
		else
		{	
			int nCount;
			ar >> nCount;
			for (int i = 0; i < nCount; i++)
			{
				T* p = new T();
				p->Serialize(ar);
				Add(p);
			}
		}
	}
};


#define DICTIONARY_BUFFER_SIZE		((UINT)65536)

//================================================================================
class CDictionaryParser
{
	CFile	m_File;
	BYTE	m_pBuffer[DICTIONARY_BUFFER_SIZE];
	UINT	m_nBufferLength;
	UINT	m_nPosition;
public:

	CDictionaryParser(const CString &strFile)
	{
		m_nBufferLength = 0;
		m_nPosition = 0;
		m_File.Open(strFile, CFile::modeRead | CFile::shareDenyWrite);
	}

	virtual ~CDictionaryParser()
	{
		m_File.Close();
	}

	UINT ParseUInt();
	BYTE ParseByte();

	CString ParseString();
private:
	UINT Read(BYTE* lpBuf, UINT nCount);
};

//===========================================================================
class CGenericContainer: public CMapStringToOb
{
public:
	DECLARE_DYNAMIC(CGenericContainer)

	~CGenericContainer();
	virtual void DestroyObjects();
	void SetAt(LPCTSTR key, CObject *newValue);
	virtual void Serialize(CArchive& ar);
	virtual CObject* CreateSerializedObject() = 0;
#ifdef _DEBUG
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};


//===========================================================================
class CResourceIndexItem: public CObject
{
	UINT		m_nNumber;
	CString		m_strUrl;

public:
	DECLARE_SERIAL(CResourceIndexItem)
	//--------------------------------------------------------------------------------
	void Parse(CDictionaryParser* pParser)
	{
		m_nNumber = pParser->ParseUInt();
		m_strUrl = pParser->ParseString();
	}
	//--------------------------------------------------------------------------------
	void Serialize(CArchive& ar);

	//--------------------------------------------------------------------------------
	BOOL Match(UINT nNumber)
	{
		return m_nNumber == nNumber;
	}

	//--------------------------------------------------------------------------------
	const CString& GetUrl()
	{
		return m_strUrl;
	}

	//--------------------------------------------------------------------------------
	CResourceIndexItem * Clone()
	{
		CResourceIndexItem *pItem = new CResourceIndexItem();
		pItem->m_nNumber = m_nNumber;
		pItem->m_strUrl = m_strUrl;
		return pItem;
	}
};
//===========================================================================
class CStringItem: public CObject
{
public:
	DECLARE_SERIAL(CStringItem)

	CString m_strTarget;
	CMapStringToString m_strAdditionalParams;
	
	BOOL GetAttribute(const CString &strName, CString &strValue);
	static CStringItem* Parse(IXMLDOMNode* pNode, CString &strBase);
	static CStringItem* Parse(CDictionaryParser *pParser, CString &strBase);
	void Serialize(CArchive& ar);

	BOOL IsValid() { return !m_strTarget.IsEmpty(); }

	//--------------------------------------------------------------------------------
	inline void ParseAttribute(IXMLDOMElement *pStringNode, LPCTSTR strAttrName)
	{
		_variant_t value;
		if (FAILED(pStringNode->getAttribute(CComBSTR(strAttrName), &value)) || value.vt == VT_NULL)
			return;

		CString strAttrValue = value;
		if (!strAttrValue.IsEmpty())
			m_strAdditionalParams[strAttrName] = strAttrValue;
	}

};


//===========================================================================
class CStringBlock: public CGenericContainer
{
public:

	BOOL m_bUsed;

	DECLARE_SERIAL(CStringBlock)

	//--------------------------------------------------------------------------------
	CStringBlock() : m_bUsed(FALSE) { }
	~CStringBlock() {}

	virtual CObject* CreateSerializedObject() { return new CStringItem(); }

	//--------------------------------------------------------------------------------
	BOOL Lookup(LPCTSTR strKey, CStringItem*& pStringItem) const
	{
		return __super::Lookup(strKey, (CObject*&) pStringItem);
	}

	BOOL ParseString(const CString &strXml, CString &strName);
	void Parse(CDictionaryParser *pParser);

	CStringItem* FindByAttributeValue(const CString &strAttrName, const CString &strAttrValue, CString& strKeyName);

};

//===========================================================================
class CStringBlockContainer: public CGenericContainer
{
protected:
	CString									m_strFilePath;
	CSerializableArray<CResourceIndexItem>	m_arResourceIndexItems;
public:
	DECLARE_SERIAL(CStringBlockContainer)


	//--------------------------------------------------------------------------------
	const CString& GetFilePath() { return m_strFilePath; }
	//--------------------------------------------------------------------------------
	CStringBlockContainer()
	{
	}

	//--------------------------------------------------------------------------------
	~CStringBlockContainer()
	{
		for (int i = 0; i < m_arResourceIndexItems.GetSize(); i++)
			delete m_arResourceIndexItems[i];

	}
	
	virtual CObject* CreateSerializedObject() { return new CStringBlock(); }

	//--------------------------------------------------------------------------------
	void DestroyObjects();

	//--------------------------------------------------------------------------------
	void AddResourceIndexItem(CResourceIndexItem* pItem)
	{
		if (pItem)
			m_arResourceIndexItems.Add(pItem->Clone());
	}

	//--------------------------------------------------------------------------------
	void RemoveAll();

	//--------------------------------------------------------------------------------
	CResourceIndexItem* ItemFromNumber(CString strNumber) const;
	
	//--------------------------------------------------------------------------------
	BOOL Lookup(LPCTSTR strKey, CStringBlock*& pStringBlock) const
	{
		pStringBlock = NULL;
		CObject* pObject = NULL;
		if (! __super::Lookup(strKey, pObject))
			return FALSE;

		pStringBlock = (CStringBlock*) pObject;
		return pStringBlock != NULL;
	}

	//--------------------------------------------------------------------------------
	void SetAt(LPCTSTR key, CStringBlock* pNewValue)
	{
		CObject* pCurrent = (*this)[key];
		CObject* pNew = dynamic_cast<CObject*> (pNewValue);
		if (pCurrent != NULL && pCurrent != pNew)
			delete pCurrent;
	
		CMapStringToOb::SetAt(key, pNew);
	}

	//--------------------------------------------------------------------------------
	virtual void ParseDictionary(const CString& strDictionaryFile);
	//--------------------------------------------------------------------------------
	virtual CStringBlock *FindStringBlock(const CString &strType, const CString &strId, const CString &strName, CString &strKey) const; 

	//--------------------------------------------------------------------------------
	void Parse(CDictionaryParser *pParser);
};

//===========================================================================
class CUsedStringBlockContainer: public CStringBlockContainer
{
	CString					m_strDictionaryFile;
	::CCriticalSection		m_CriticalSection;

public:

	//--------------------------------------------------------------------------------
	CUsedStringBlockContainer()
	{
	}
	//--------------------------------------------------------------------------------
	~CUsedStringBlockContainer()
	{
	}

	DECLARE_SERIAL(CUsedStringBlockContainer)
	//--------------------------------------------------------------------------------
	void Serialize(CArchive& ar);
	void DestroyObjects() { CGenericContainer::DestroyObjects(); } 

	//--------------------------------------------------------------------------------
	void ParseDictionary(const CString& strDictionaryFile) { m_strDictionaryFile = strDictionaryFile;}
	CStringBlock *FindStringBlock(CStringLoader *pLoader, const CString &strType, const CString &strId, const CString &strName);

};


//===========================================================================
class CIndexItem: public CObject
{
friend class CStringBlockContainer;
	
	UINT	m_nType;
	CString	m_strId;
	CString	m_strName;

public:
	//--------------------------------------------------------------------------------
	void Parse(CDictionaryParser* pParser)
	{
		m_nType = pParser->ParseUInt();
		m_strId = pParser->ParseString();
		m_strName = pParser->ParseString();
	}

	//--------------------------------------------------------------------------------
	CString ToString()
	{
		return ToString(m_nType, m_strId, m_strName);
	}

	//--------------------------------------------------------------------------------
	static CString ToString(const CString &strType, const CString &strId, const CString &strName)
	{
		return ToString(TypeToUInt(strType), strId, strName);
	}

	//--------------------------------------------------------------------------------
	static CString ToString(UINT nType, const CString &strId, const CString &strName)
	{
		CString s;
		s.Format(_T("%d %s %s"), nType, strId, strName);
		s.MakeLower();
		return s;
	}

	//--------------------------------------------------------------------------------
	static UINT TypeToUInt(CString strType)
	{
		if (strType == _T(""))
			return 0;
		if (strType == _T("databasescript"))
			return 1;
		if (strType == _T("dialog"))
			return 2;
		if (strType == _T("other"))
			return 3;
		if (strType == _T("report"))
			return 4;
		if (strType == _T("source"))
			return 5;
		if (strType == _T("stringtable"))
			return 6;
		if (strType == _T("xml"))
			return 7;
		if (strType == _T("menu"))
			return 8;
		if (strType == _T("jsonforms"))
			return 9;

		return 0;
	}

	BOOL IsStringTable() { return m_nType == 6; }
};


//===========================================================================
class CDictionaryBinaryFileLRU : public CObList
{
public:

	CStringBlockContainer* GetDictionary(const CString & filePath); 
	virtual ~CDictionaryBinaryFileLRU();
	void RemoveAll();

};