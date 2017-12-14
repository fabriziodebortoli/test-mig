#include "stdafx.h"

#include "JsonSerializer.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


void replace_substring(std::wstring& s, const std::wstring& f, const std::wstring& t)
{
	for (
		size_t pos = s.find(f);         // find first occurrence of f
		pos != std::string::npos;       // make sure f was found
		s.replace(pos, f.size(), t),    // replace with t
		pos = s.find(f, pos + t.size()) // find next occurrence of f
		);
}
/// escape tilde and slash
static CString escape(LPCTSTR sz)
{
	std::wstring s(sz);
	// escape "~"" to "~0" and "/" to "~1"
	replace_substring(s, L"~", L"~0");
	replace_substring(s, L"/", L"~1");
	return s.c_str();
}
//-----------------------------------------------------------------------------
void Patch(Json::Value& target, const Json::Value& patch)
{
	for (Json::ValueIterator it = patch.begin(); it != patch.end(); ++it)
	{
		auto op = (*it)[_T("op")].asString();
		auto path = (TCHAR*)(*it)[_T("path")].asCString();
		TCHAR *next_token = NULL;
		if (op == _T("replace"))
		{
			Json::Value* pVal = &target;
			TCHAR *p = _tcstok_s(path, _T("/"), &next_token);
			while (p) {
				pVal = &(*pVal)[p];
				p = _tcstok_s(NULL, _T("/"), &next_token);
			}
			*pVal = (*it)[_T("value")];
		}
		else if (op == _T("remove"))
		{
			Json::Value* pVal = &target;
			Json::Value* pParent = NULL;
			TCHAR *p = _tcstok_s(path, _T("/"), &next_token);
			TCHAR *pKey = NULL;
			while (p) {
				pKey = p;
				pParent = pVal;
				pVal = &(*pVal)[p];
				p = _tcstok_s(NULL, _T("/"), &next_token);
			}
			pParent->removeMember(pKey);
		}
		else if (op == _T("add"))
		{
			Json::Value* pVal = &target;
			TCHAR *p = _tcstok_s(path, _T("/"), &next_token);
			while (p && pVal->isMember(p)) {
				pVal = &(*pVal)[p];
				p = _tcstok_s(NULL, _T("/"), &next_token);
			}
			(*pVal)[p] = (*it)[_T("value")];
		}
		else
		{
			TRACE("Operation not supported");
			ASSERT(FALSE);
		}
	}
}
//-----------------------------------------------------------------------------
Json::Value Diff(const Json::Value& source, const Json::Value& target, const CString& path /*= ""*/)
{
	// the patch
	Json::Value result = Json::arrayValue;

	// if the values are the same, return empty patch
	if (source == target)
	{
		return result;
	}

	if (source.type() != target.type())
	{
		// different types: replace value
		Json::Value obj = Json::objectValue;
		obj[_T("op")] = _T("replace");
		obj[_T("path")] = (LPCTSTR)path;
		obj[_T("value")] = target;
		result.append(obj);
	}
	else
	{
		switch (source.type())
		{
		case Json::arrayValue:
		{
			// first pass: traverse common elements
			int i = 0;
			while (i < (int)source.size() && i < (int)target.size())
			{
				// recursive call to compare array values at index i
				auto temp_diff = Diff(source[i], target[i], path + _T("/") + std::to_wstring(i).c_str());
				for (Json::ValueIterator it = temp_diff.begin(); it != temp_diff.end(); ++it)
					result.append(*it);
				++i;
			}

			// i now reached the end of at least one array
			// in a second pass, traverse the remaining elements

			// remove my remaining elements;
			for (int j = (int)source.size() - 1; j >= i; j--)
			{
				// add operations in reverse order to avoid invalid
				// indices
				Json::Value obj = Json::objectValue;
				obj[_T("op")] = _T("remove");
				obj[_T("path")] = (LPCTSTR)(path + _T("/") + std::to_wstring(j).c_str());
				result.append(obj);
			}
			i = source.size();
			// add other remaining elements
			while (i < (int)target.size())
			{
				Json::Value obj = Json::objectValue;
				obj[_T("op")] = _T("add");
				obj[_T("path")] = (LPCTSTR)(path + _T("/") + std::to_wstring(i).c_str());
				obj[_T("value")] = target[i];
				result.append(obj);
				++i;
			}

			break;
		}

		case Json::objectValue:
		{
			// first pass: traverse this object's elements
			for (auto it = source.begin(); it != source.end(); ++it)
			{
				// escape the key name to be used in a JSON patch
				const auto key = escape(it.memberName());

				if (!target[it.memberName()].isNull())
				{
					// recursive call to compare object values at key it
					auto temp_diff = Diff(*it, target[it.memberName()], path + _T("/") + key);
					for (Json::ValueIterator it = temp_diff.begin(); it != temp_diff.end(); ++it)
						result.append(*it);
				}
				else
				{
					Json::Value obj = Json::objectValue;
					obj[_T("op")] = _T("remove");
					obj[_T("path")] = (LPCTSTR)(path + _T("/") + key);
					result.append(obj);
				}
			}

			// second pass: traverse other object's elements
			for (auto it = target.begin(); it != target.end(); ++it)
			{
				if (source[it.memberName()].isNull())
				{
					// found a key that is not in this -> add it
					const auto key = escape(it.memberName());
					Json::Value obj = Json::objectValue;
					obj[_T("op")] = _T("add");
					obj[_T("path")] = (LPCTSTR)(path + _T("/") + key);
					obj[_T("value")] = *it;
					result.append(obj);
				}
			}

			break;
		}

		default:
		{
			// both primitive type: replace value
			Json::Value obj = Json::objectValue;
			obj[_T("op")] = _T("replace");
			obj[_T("path")] = (LPCTSTR)path;
			obj[_T("value")] = target;
			result.append(obj);
			break;
		}
		}
	}

	return result;
}

//-----------------------------------------------------------------------------
void* GetCorrespondingField(void* pObject, void* pField, void* thisObject)
{
	LONG_PTR pOtherField = (LONG_PTR)pField;		//indirizzo del campo espressione
	LONG_PTR pOtherFieldOffset = pOtherField - (LONG_PTR)pObject;	//offset del campo rispetto all'oggetto che lo contiene
	LONG_PTR pFieldInThisObject = (LONG_PTR)thisObject + pOtherFieldOffset;//indirizzo del campo modificato corrispondente in questo oggetto (this)
	return (void*)pFieldInThisObject;
}
//-----------------------------------------------------------------------------
void CJsonExpressions::Assign(CJsonExpressions* pSource, void* pSourceObject, void* pDestinationObject)
{
	POSITION pos = pSource->m_BoolExpressions.GetStartPosition();
	while (pos)
	{
		bool* pKey = NULL;
		CString sExpr;
		pSource->m_BoolExpressions.GetNextAssoc(pos, pKey, sExpr);
		m_BoolExpressions[(bool*)GetCorrespondingField(pSourceObject, pKey, pDestinationObject)] = sExpr;
	}

	pos = pSource->m_IntExpressions.GetStartPosition();
	while (pos)
	{
		int* pKey = NULL;
		CString sExpr;
		pSource->m_IntExpressions.GetNextAssoc(pos, pKey, sExpr);
		m_IntExpressions[(int*)GetCorrespondingField(pSourceObject, pKey, pDestinationObject)] = sExpr;
	}
	pos = pSource->m_DoubleExpressions.GetStartPosition();
	while (pos)
	{
		double* pKey = NULL;
		CString sExpr;
		pSource->m_DoubleExpressions.GetNextAssoc(pos, pKey, sExpr);
		m_DoubleExpressions[(double*)GetCorrespondingField(pSourceObject, pKey, pDestinationObject)] = sExpr;
	}
	pos = pSource->m_Bool3Expressions.GetStartPosition();
	while (pos)
	{
		Bool3* pKey = NULL;
		CString sExpr;
		pSource->m_Bool3Expressions.GetNextAssoc(pos, pKey, sExpr);
		m_Bool3Expressions[(Bool3*)GetCorrespondingField(pSourceObject, pKey, pDestinationObject)] = sExpr;
	}
	pos = pSource->m_StringExpressions.GetStartPosition();
	while (pos)
	{
		CString* pKey = NULL;
		CString sExpr;
		pSource->m_StringExpressions.GetNextAssoc(pos, pKey, sExpr);
		m_StringExpressions[(CString*)GetCorrespondingField(pSourceObject, pKey, pDestinationObject)] = sExpr;
	}
}
//-----------------------------------------------------------------------------
bool CJsonExpressions::Lookup(void* pObj, CString& sExpr)
{
	if (m_BoolExpressions.Lookup((bool*)pObj, sExpr))
		return true;
	if (m_IntExpressions.Lookup((int*)pObj, sExpr))
		return true;
	if (m_DoubleExpressions.Lookup((double*)pObj, sExpr))
		return true;
	if (m_Bool3Expressions.Lookup((Bool3*)pObj, sExpr))
		return true;
	if (m_StringExpressions.Lookup((CString*)pObj, sExpr))
		return true;
	return false;
}

//-----------------------------------------------------------------------------
void CJsonWrapper::Diff(CJsonWrapper& source, CJsonWrapper& target)
{
	m_Root = ::Diff(source.GetCurrent(), target.GetCurrent(), _T(""));
}
//-----------------------------------------------------------------------------
void CJsonWrapper::Patch(CJsonWrapper& patch)
{
	::Patch(GetCurrent(), patch.GetCurrent());
}
//-----------------------------------------------------------------------------
void CJsonWrapper::Clear()
{
	m_Root.clear();
}

//-----------------------------------------------------------------------------
Json::Value& CJsonWrapper::GetCurrent()
{
	return m_Root;
}

//-----------------------------------------------------------------------------
CString CJsonWrapper::GetJson() const
{
	Json::StyledWriter writer;
	return writer.write(m_Root).c_str();
}

//-----------------------------------------------------------------------------
void CJsonWrapper::operator = (const CJsonWrapper& other)
{
	Assign(other);
}
//-----------------------------------------------------------------------------
void CJsonWrapper::Assign(const CJsonWrapper& other)
{
	m_Root = other.m_Root;
}

//=================================================================================================
//									CJsonParser
//=================================================================================================
CJsonParser::CJsonParser()
{
	m_Stack.push(&m_Root);
}
//-----------------------------------------------------------------------------
bool CJsonParser::ReadJsonFromString(LPCTSTR sJson)
{
	if (!m_Reader.parse(std::wstring(sJson), m_Root))
		return false;
	
	return true;
}

//-----------------------------------------------------------------------------
void CJsonParser::Reset()
{
	while (!m_Stack.empty())
		m_Stack.pop();
	m_Stack.push(&m_Root);
}

//-----------------------------------------------------------------------------
void CJsonParser::Assign(const CJsonWrapper& other)
{
	__super::Assign(other);
	Reset();
}
//-----------------------------------------------------------------------------
void CJsonParser::Assign(const Json::Value& root)
{
	m_Root = root;
	Reset();
}
//-----------------------------------------------------------------------------
bool CJsonParser::Has(LPCTSTR sName)
{
	return !(*m_Stack.top())[sName].isNull();
}
//-----------------------------------------------------------------------------
bool CJsonParser::IsObject(LPCTSTR sName)
{
	return (*m_Stack.top())[sName].isObject();
}
//-----------------------------------------------------------------------------
Json::Value& CJsonParser::GetCurrent()
{
	return m_Stack.empty() ? m_Root : *m_Stack.top();
}
//-----------------------------------------------------------------------------
bool CJsonParser::IsObject()
{
	return (*m_Stack.top()).isObject();
}
//-----------------------------------------------------------------------------
bool CJsonParser::IsArray()
{
	return (*m_Stack.top()).isArray();
}
//-----------------------------------------------------------------------------
int CJsonParser::ReadInt(LPCTSTR sName)
{
	return (*m_Stack.top())[sName].asInt();
}
//-----------------------------------------------------------------------------
bool CJsonParser::TryReadInt(LPCTSTR sName, int& n)
{
	if (Has(sName) && (*m_Stack.top())[sName].isInt())
	{
		n = ReadInt(sName);
		return true;
	}
	n = 0;
	return false;
}

//-----------------------------------------------------------------------------
double CJsonParser::ReadDouble(LPCTSTR sName)
{
	return (*m_Stack.top())[sName].asDouble();
}
//-----------------------------------------------------------------------------
bool CJsonParser::TryReadDouble(LPCTSTR sName, double& d)
{
	if (Has(sName) && (*m_Stack.top())[sName].isDouble())
	{
		d = ReadDouble(sName);
		return true;
	}
	d = 0;
	return false;
}
//-----------------------------------------------------------------------------
Json::Value CJsonParser::ReadValue(LPCTSTR sName)
{
	return (*m_Stack.top())[sName];
}
//-----------------------------------------------------------------------------
bool CJsonParser::ReadBool(LPCTSTR sName)
{
	return (*m_Stack.top())[sName].asBool();
}
//-----------------------------------------------------------------------------
bool CJsonParser::TryReadBool(LPCTSTR sName, bool& b)
{
	if (Has(sName) && (*m_Stack.top())[sName].isBool())
	{
		b = ReadBool(sName);
		return true;
	}
	b = false;
	return false;
}
//-----------------------------------------------------------------------------
CString CJsonParser::ReadString(int index)
{
	return (*m_Stack.top())[index].asString().c_str();
}
//-----------------------------------------------------------------------------
CString CJsonParser::ReadString(LPCTSTR sName)
{
	return (*m_Stack.top())[sName].asString().c_str();
}
//-----------------------------------------------------------------------------
bool CJsonParser::TryReadString(LPCTSTR sName, CString& s)
{
	if (Has(sName) && (*m_Stack.top())[sName].isString())
	{
		s = ReadString(sName);
		return true;
	}
	s = _T("");
	return false;
}
//-----------------------------------------------------------------------------
bool CJsonParser::BeginReadObject(LPCTSTR sName)
{
	Json::Value& val = (*m_Stack.top())[sName];
	if (val.isNull() || !val.isObject())
		return false;
	m_Stack.push(&val);
	return true;
}
//-----------------------------------------------------------------------------
bool CJsonParser::BeginReadObject(int index)
{
	Json::Value& val = (*m_Stack.top())[index];
	if (val.isNull() || !val.isObject())
		return false;
	m_Stack.push(&val);
	return true;
}
//-----------------------------------------------------------------------------
void CJsonParser::EndReadObject()
{
	m_Stack.pop();
}
//-----------------------------------------------------------------------------
bool CJsonParser::BeginReadArray(LPCTSTR sName)
{
	Json::Value& val = (*m_Stack.top())[sName];
	if (val.isNull() || !val.isArray())
		return false;
	m_Stack.push(&val);
	return true;
}

//-----------------------------------------------------------------------------
bool CJsonParser::BeginReadArray(int index)
{
	Json::Value& val = (*m_Stack.top())[index];
	if (val.isNull() || !val.isArray())
		return false;
	m_Stack.push(&val);
	return true;
}

//-----------------------------------------------------------------------------
void CJsonParser::EndReadArray()
{
	m_Stack.pop();
}
//-----------------------------------------------------------------------------
int CJsonParser::GetCount()
{
	Json::Value* pVal = m_Stack.top();
	return pVal->size();
}

//-----------------------------------------------------------------------------
CJsonIterator* CJsonParser::BeginIteration()
{
	m_Iterator.Begin(m_Stack.top());
	return &m_Iterator;
}

//-----------------------------------------------------------------------------
void CJsonParser::CopyAttribute(const CJsonParser& otherParser, const CString& sAttrName)
{
	Json::Value* pVal = m_Stack.top();
	Json::Value* pOtherVal = otherParser.m_Stack.top();
	(*pVal)[sAttrName] = (*pOtherVal)[sAttrName];
}
//-----------------------------------------------------------------------------
CString CJsonParser::GetError()
{
	return m_Reader.getFormatedErrorMessages().c_str();
}
//-----------------------------------------------------------------------------
BOOL CJsonIterator::GetNext(CString &sKey, CString& sVal)
{
	if (m_it == m_pVal->end())
		return FALSE;
	Json::Value key = m_it.key();
	Json::Value value = (*m_it);
	/*if (!key.isString() || !value.isString()) ///TODOLUCA TODOMARCO, mi potrebbe arrivare un booleano, e mi viene escluso da questo metodo
		return FALSE;*/
	
	sKey = CString(key.asString().c_str());
	sVal = CString(value.asString().c_str());
	m_it++;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CJsonIterator::GetNext(CString &sKey, CJsonParser& aVal)
{
	if (m_it == m_pVal->end())
		return FALSE;
	Json::Value key = m_it.key();
	Json::Value value = (*m_it);
	sKey = key.asCString();
	aVal.Assign(value);
	m_it++;
	return TRUE;
}

//=================================================================================================
//									CJSonSerializer
//=================================================================================================
//-----------------------------------------------------------------------------
CString CJsonSerializer::Escape(LPCTSTR sValue)
{
	CString str(sValue);
	str.Replace(_T("\\"), _T("\\\\"));
	str.Replace(_T("\t"), _T("\\t"));
	str.Replace(_T("\r"), _T("\\r"));
	str.Replace(_T("\n"), _T("\\n"));
	return str;
}


//-----------------------------------------------------------------------------
void CJsonSerializer::Clear()
{
	__super::Clear();
	Reset();
}

//-----------------------------------------------------------------------------
void CJsonSerializer::Reset()
{
	while (!m_Stack.empty())
		m_Stack.pop();
	m_Stack.push(&m_Root);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteValue(int index, Json::Value value, LPCTSTR sComment /*= NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = value;
	if (sComment)
		(*pVal)[index].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteValue(LPCTSTR sName, Json::Value value, LPCTSTR sComment /*= NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = value;
	if (sComment)
		(*pVal)[sName].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteString(int index, LPCTSTR sValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = sValue;
	if (sComment)
		(*pVal)[index].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteString(LPCTSTR sName, LPCTSTR sValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = sValue;
	if (sComment)
		(*pVal)[sName].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteBool(int index, bool bValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = bValue;
	if (sComment)
		(*pVal)[index].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteBool(LPCTSTR sName, bool bValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = bValue;
	if (sComment)
		(*pVal)[sName].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//Write the simple value, ex. 1
//-----------------------------------------------------------------------------
void CJsonSerializer::WriteInt(int index, int nValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = nValue;
	if (sComment && sComment[0] != 0)
		(*pVal)[index].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//Write the name-value ex. "name": 1
//-----------------------------------------------------------------------------
void CJsonSerializer::WriteInt(LPCTSTR sName, int nValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = nValue;
	if (sComment && sComment[0] != 0)
		(*pVal)[sName].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteDouble(int index, double dValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = dValue;
	if (sComment)
		(*pVal)[index].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteDouble(LPCTSTR sName, double dValue, LPCTSTR sComment/*=NULL*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = dValue;
	if (sComment)
		(*pVal)[sName].setComment(CString("/*") + sComment + _T("*/"), Json::CommentPlacement::commentAfterOnSameLine);
}
//-----------------------------------------------------------------------------
void CJsonSerializer::OpenObject(const CString sName)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = Json::objectValue;
	Json::Value& pNewVal = (*pVal)[sName];
	m_Stack.push(&pNewVal);
}
//-----------------------------------------------------------------------------
void CJsonSerializer::OpenObject(int index)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[index] = Json::objectValue;
	Json::Value& pNewVal = (*pVal)[index];
	m_Stack.push(&pNewVal);
}
//-----------------------------------------------------------------------------
void CJsonSerializer::OpenObject()
{
	Json::Value* pVal = m_Stack.top();
	int index = pVal->size();
	(*pVal)[index] = Json::objectValue;
	Json::Value& pNewVal = (*pVal)[index];
	m_Stack.push(&pNewVal);
}
//-----------------------------------------------------------------------------
void CJsonSerializer::CloseObject()
{
	m_Stack.pop();
}


//-----------------------------------------------------------------------------
void CJsonSerializer::OpenArray(const CString sName /*_T("")*/)
{
	Json::Value* pVal = m_Stack.top();
	(*pVal)[sName] = Json::arrayValue;
	Json::Value& pNewVal = (*pVal)[sName];
	m_Stack.push(&pNewVal);
}

//-----------------------------------------------------------------------------
void CJsonSerializer::CloseArray()
{
	m_Stack.pop();
}

//-----------------------------------------------------------------------------
void CJsonSerializer::WriteJsonFragment(LPCTSTR sName, LPCTSTR sFragment)
{
	Json::Value* pVal = m_Stack.top();

	Json::Reader r;
	std::wstring doc = sFragment;
	Json::Value root;
	r.parse(doc, root);
	Json::Value node = (root.type() == Json::arrayValue)
		? Json::nullValue
		: root.get(sName, Json::nullValue);
	(*pVal)[sName] = node == Json::nullValue ? root : node;
	//Json::Value& pNewVal = (*pVal)[sName];
	//m_Stack.push(&pNewVal);
}

//=================================================================================================
//									CJsonObject
//=================================================================================================

//--------------------------------------------------------------------------------
CNameValuePair::CNameValuePair()
{
}
//--------------------------------------------------------------------------------
CNameValuePair::CNameValuePair(LPCTSTR sName, LPCTSTR sValue)
{
	m_sName = sName;
	m_sValue = sValue;
}
//--------------------------------------------------------------------------------
CNameValuePair::CNameValuePair(LPCTSTR sName, BYTE*	pValue, int nSize)
{
	m_sName = sName;
	m_pValue = pValue;
	m_nSize = nSize;
}

//--------------------------------------------------------------------------------
CString CNameValuePair::GetValue() const
{
	return m_sValue;
}
//--------------------------------------------------------------------------------
bool CNameValuePair::GetValue(BYTE*& pValue, int& nSize) const
{
	pValue = m_pValue;
	nSize = m_nSize;

	return m_pValue != NULL;
}

//--------------------------------------------------------------------------------
CNameValueCollection::~CNameValueCollection()
{
	for (int i = 0; i < GetSize(); i++)
	{
		delete GetAt(i);
	}
}
//--------------------------------------------------------------------------------
CString CNameValueCollection::GetValueByName(LPCTSTR sName) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		CNameValuePair* o = GetAt(i);
		if (o->GetName() == sName)
			return o->GetValue();
	}
	return _T("");
}

//--------------------------------------------------------------------------------
bool CNameValueCollection::GetValueByName(LPCTSTR sName, BYTE*& pValue, int& nSize) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		CNameValuePair* o = GetAt(i);
		if (o->GetName() == sName)
			return o->GetValue(pValue, nSize);
	}
	return false;
}

//--------------------------------------------------------------------------------
void CNameValueCollection::GetValuesByName(LPCTSTR sName, CStringArray& arValues) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		CNameValuePair* o = GetAt(i);
		if (o->GetName() == sName)
		{
			arValues.Add(o->GetValue());
		}
	}
}


//--------------------------------------------------------------------------------
void CNameValueCollection::GetValuesByName(LPCTSTR sName, std::vector<int>& arValues) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		CNameValuePair* o = GetAt(i);
		if (o->GetName() == sName)
		{
			arValues.push_back(_ttoi(o->GetValue()));
		}
	}
}

//--------------------------------------------------------------------------------
void CNameValueCollection::Add(CString name, BYTE* pValue, int nSize)
{
	CNameValuePair* aObject = new CNameValuePair(name, pValue, nSize);
	__super::Add(aObject);
}
//--------------------------------------------------------------------------------
void CNameValueCollection::Add(CString name, CString value)
{
	CNameValuePair* aObject = new CNameValuePair(name, value);
	__super::Add(aObject);
}


