#pragma once

#include "lib_json\json.h"
//includere alla fine degli include del .H
#include "beginh.dex"

#define BYTE_DATA_PARAM_NAME _T("BYTEDATA")


enum Bool3 { B_UNDEFINED, B_TRUE, B_FALSE };

class UsedDefines : public CMap<CString, LPCTSTR, CString, LPCTSTR>
{
public:
	bool m_bActive = true;
	void Assign(const UsedDefines& defines) 
	{
		m_bActive = defines.m_bActive;
		POSITION pos = defines.GetStartPosition();
		CString sKey, sValue;
		while (pos)
		{

			defines.GetNextAssoc(pos, sKey, sValue);
			this->SetAt(sKey, sValue);
		}
	}
};

typedef CMap<CString, LPCTSTR, Json::Value, Json::Value> DefineMap;


//=================================================================================================
class TB_EXPORT CJsonExpressions
{
public:
	bool m_bEvaluated = false;
	CMap<CString*, CString*, CString, LPCTSTR> m_StringExpressions;
	CMap<bool*, bool*, CString, LPCTSTR> m_BoolExpressions;
	CMap<int*, int*, CString, LPCTSTR> m_IntExpressions;
	CMap<Bool3*, Bool3*, CString, LPCTSTR> m_Bool3Expressions;
	CMap<double*, double*, CString, LPCTSTR> m_DoubleExpressions;

	void Assign(CJsonExpressions* pSource, void* pSourceObject, void* pDestinationObject);
	bool Lookup(void* pObj, CString& sExpr);
};
//=================================================================================================
class TB_EXPORT CJsonIterator
{
	friend class CJsonParser;
private:
	Json::Value::iterator m_it;
	Json::Value* m_pVal;
	void Begin(Json::Value* pVal)
	{
		m_pVal = pVal;
		m_it = pVal->begin();
	}
public:
	BOOL GetNext(CString &sKey, CString& sVal);
	BOOL GetNext(CString &sKey, CJsonParser& aVal);
};

//=================================================================================================
class TB_EXPORT CJsonWrapper
{
protected:
	Json::Value m_Root;
public:
	void Diff(CJsonWrapper& source, CJsonWrapper& target);
	void Patch(CJsonWrapper& patch);
	CString GetJson() const;
	bool IsEmpty() { return m_Root.size() == 0; }
	void Clear();
	void operator = (const CJsonWrapper& other);
	virtual void Assign (const CJsonWrapper& other);
	virtual Json::Value& GetCurrent();
};

//=================================================================================================
class TB_EXPORT CJsonParser : public CJsonWrapper
{
private: 
	std::stack<Json::Value*> m_Stack;
	Json::Reader m_Reader;
	CJsonIterator m_Iterator;
public:
	CJsonParser();
	void Reset();
	bool ReadJsonFromString(LPCTSTR sJson);

	int GetCount();
	bool Has (LPCTSTR sName);
	bool IsObject(LPCTSTR sName);
	bool IsObject();
	bool IsArray();
	Json::Value ReadValue(LPCTSTR sName);
	bool ReadBool(LPCTSTR sName);
	bool TryReadBool(LPCTSTR sName, bool& b);
	int ReadInt (LPCTSTR sName);
	bool TryReadInt(LPCTSTR sName, int& n);
	double ReadDouble(LPCTSTR sName);
	bool TryReadDouble(LPCTSTR sName, double& d);
	CString ReadString(int index);
	CString ReadString(LPCTSTR sName);
	bool TryReadString(LPCTSTR sName, CString& s);
	bool BeginReadObject (LPCTSTR sName);
	bool BeginReadObject (int index);
	void EndReadObject();
	bool BeginReadArray(LPCTSTR sName);
	bool BeginReadArray(int index);
	void EndReadArray();
	CJsonIterator* BeginIteration();
	void CopyAttribute(const CJsonParser& otherParser, const CString& sAttrName);
	CString GetError();
	virtual Json::Value& GetCurrent();
	virtual void Assign(const CJsonWrapper& other); 
	void Assign(const Json::Value& root);
};
//=================================================================================================
class TB_EXPORT CJsonSerializer : public CJsonWrapper
{
private: 
	std::stack<Json::Value*> m_Stack;
public:
	CJsonSerializer() { m_Stack.push(&m_Root); }
	
	void WriteString(int index, LPCTSTR sValue, LPCTSTR sComment = NULL);
	void WriteString(LPCTSTR sName, LPCTSTR sValue, LPCTSTR sComment=NULL);
	void OpenObject		(const CString sName);
	void OpenObject		(int index);
	void OpenObject		();
	void CloseObject	();
	void OpenArray		(const CString sName);// = _T(""));
	void CloseArray		();
	void WriteValue(int index, Json::Value value, LPCTSTR sComment = NULL);
	void WriteValue(LPCTSTR sName, Json::Value value, LPCTSTR sComment = NULL);
	void WriteBool(int index, bool bValue, LPCTSTR sComment = NULL);
	void WriteBool(LPCTSTR sName, bool bValue, LPCTSTR sComment=NULL);
	void WriteInt(int index, int nValue, LPCTSTR sComment = NULL);
	void WriteInt(LPCTSTR sName, int nValue, LPCTSTR sComment=NULL);
	void WriteDouble(int index, double dValue, LPCTSTR sComment = NULL);
	void WriteDouble(LPCTSTR sName, double dValue, LPCTSTR sComment=NULL);
	void WriteJsonFragment(LPCTSTR sName, LPCTSTR sFragment);
private:	

	CString Escape(LPCTSTR sValue);
};
class TB_EXPORT IJsonModelProvider
{
public:
	virtual void		GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound) = 0;
	virtual void		SetJson(CJsonParser& jsonParser) = 0;
	virtual CString		GetComponentId() = 0;
};


////==============================================================================
//class TB_EXPORT CIntArray : public CArray<int>
//{
////public:
//	//CIntArray(){}
//	/*CIntArray(const CIntArray& arSource)
//	{
//		for (int i = 0; i < arSource.GetCount(); i++)
//			Add(arSource[i]);
//	}*/
//	/*BOOL IsEmpty() const{
//		return CArray<int>::IsEmpty();
//	}*/
//};


//==============================================================================
class TB_EXPORT CNameValuePair 
{
protected:
	CString m_sName;
	CString m_sValue;
	BYTE* m_pValue = NULL;
	int m_nSize = 0;
public:
	CNameValuePair();
	CNameValuePair(LPCTSTR sName, LPCTSTR sValue);
	CNameValuePair(LPCTSTR sName, BYTE*	pValue, int nLength);
	~CNameValuePair() { delete m_pValue; }
public:
	
	virtual CString GetName() const { return m_sName; }
	virtual CString GetValue() const;
	virtual bool GetValue(BYTE*& pValue, int& nSize) const;

};

class TB_EXPORT CNameValueCollection : public CArray<CNameValuePair*>
{
public:
	~CNameValueCollection();
	CString GetValueByName (LPCTSTR sName) const;
	bool GetValueByName(LPCTSTR sName, BYTE*& pValue, int& nSize) const;
	void GetValuesByName (LPCTSTR sName, CStringArray& arValues) const;
	void GetValuesByName (LPCTSTR sName, std::vector<int>& arValues) const;
	virtual void Add(CString name, CString value);
	virtual void Add(CString name, BYTE* pValue, int nSize);
};

#include "endh.dex"
