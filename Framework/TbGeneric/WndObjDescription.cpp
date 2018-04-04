#include "StdAfx.h"

#include <atlimage.h>
#include <algorithm>
#include <sstream>
#include <iostream>

#include <TBNamesolver\UserMessages.h>
#include <TBNamesolver\PathFinder.h>
#include <TBNamesolver\TbWinThread.h>
#include <TBNamesolver\threadcontext.h>
#include <Tbxmlcore\xmldocobj.h>
#include <TbStringLoader\Generic.h>

#include "generalfunctions.h"
#include "GeneralObjects.h"
#include "bcmenu.h"
#include "CollateCultureFunctions.h"
#include "WndObjDescription.h"
#include "FormatsTable.h"
#include "JsonFormEngine.h"
#include "Linefile.h"
#include "TBThemeManager.h"

TCHAR* szBase = _T("b");
TCHAR* szTarget = _T("t");

//-----------------------------------------------------------------------------
void ExpressionObject::ActivateDefines(bool bActive)
{
	m_Defines.m_bActive = bActive;
}
//-----------------------------------------------------------------------------
bool ExpressionObject::SerializeDefine(LPCTSTR szName, CJsonSerializer& strJson)
{

	CString val;
	if (!m_Defines.m_bActive || !m_Defines.Lookup(szName, val))
		return false;
	strJson.OpenObject(szName);
	strJson.WriteString(szJsonConst, val);
	strJson.CloseObject();
	return true;
}
//-----------------------------------------------------------------------------
bool ExpressionObject::SerializeExpression(LPCTSTR szName, void* pVal, CJsonSerializer& strJson)
{
	CString sExpr;
	if (m_Expressions.Lookup(pVal, sExpr))
	{
		strJson.WriteString(szName, _T("{{") + sExpr + _T("}}"));
		return true;
	}
	return false;
}
//-----------------------------------------------------------------------------
void ExpressionObject::Assign(ExpressionObject* pDesc)
{
	m_Expressions.Assign(&pDesc->m_Expressions, pDesc, this);
	m_Defines.Assign(pDesc->m_Defines);
}
//-----------------------------------------------------------------------------
CString ToString(COLORREF nColor)
{
	DWORD dwR = GetRValue(nColor);
	DWORD dwG = GetGValue(nColor);
	DWORD dwB = GetBValue(nColor);

	CString sValue;
	sValue.Format(_T("#%02X%02X%02X"), dwR, dwG, dwB);

	return sValue;
}
//-----------------------------------------------------------------------------
BOOL FromString(CString sColor, COLORREF& nColor)
{
	sColor.TrimLeft(_T("#"));
	if (sColor.GetLength() != 6)
		return FALSE;

	CString sR = sColor.Mid(0, 2);
	CString sG = sColor.Mid(2, 2);
	CString sB = sColor.Mid(4, 2);
	WORD r, g, b;
	std::wstringstream((LPCTSTR)sR) >> std::hex >> r;
	std::wstringstream((LPCTSTR)sG) >> std::hex >> g;
	std::wstringstream((LPCTSTR)sB) >> std::hex >> b;
	nColor = RGB(r, g, b);
	return TRUE;

}
//-----------------------------------------------------------------------------
bool TabOrderLess(const CWndObjDescription* pFirst, const CWndObjDescription* pSecond)
{
	if (pFirst->m_hAssociatedWnd && !pSecond->m_hAssociatedWnd)
		return true;
	if (!pFirst->m_hAssociatedWnd && pSecond->m_hAssociatedWnd)
		return false;
	HWND hwndFirst = pFirst->m_hAssociatedWnd;
	//mi sposto in avanti a cercare la seconda finestra
	while (hwndFirst = GetWindow(hwndFirst, GW_HWNDNEXT))
	{
		//se la trovo, la prima precede la seconda
		if (hwndFirst == pSecond->m_hAssociatedWnd)
			return true;
	}

	//se non la trovo, la seconda precede la prima
	return false;
}
//-----------------------------------------------------------------------------
bool LessThan(const CWndObjDescription* oFirstOp, const CWndObjDescription* oSecOp)
{
	// compare the two given object descriptions watching at 
	// their rectangle coordinates, i.e. who is in a upper 
	// position (y_1 < y_2 => object 1 comes first) comes first.
	// If the two objects have same vertical position (y_1 == y_2)
	// then horizontal coordinate is taken into account.

	if (oFirstOp && oSecOp) {

		if (oFirstOp->m_Y == oSecOp->m_Y ||
			(oFirstOp->m_Y < oSecOp->GetRect().BottomRight().y) && (oFirstOp->m_Y > oSecOp->m_Y) ||

			(oFirstOp->m_Y < oSecOp->GetRect().BottomRight().y) && (oFirstOp->m_Y > oSecOp->m_Y) ||

			(oSecOp->m_Y < oSecOp->GetRect().BottomRight().y) && (oSecOp->m_Y > oFirstOp->m_Y))
		{
			// the given ctrls are (almost) on the same line.
			return oFirstOp->m_X < oSecOp->m_X;
		}
		else
		{
			return oFirstOp->m_Y < oSecOp->m_Y;
		}
	}
	// at least one operand is null, it should not happen,
	// but when adding a radiobutton from EasyBuilder it messes up
	// and this code is actually executed.

	// op != null comes first.
	return oFirstOp != NULL;
	/*if(!oFirstOp)
	{
	return oSecOp;
	}else
	{
	return !oSecOp;
	}*/

}
//-----------------------------------------------------------------------------
BOOL SafeMapDialog(HWND hDlg, CRect& r)
{
	int sLeft = (r.left > 0) - (r.left < 0);
	int sTop = (r.top > 0) - (r.top < 0);
	int sBottom = (r.bottom > 0) - (r.bottom < 0);
	int sRight = (r.right > 0) - (r.right < 0);

	r.left = abs(r.left);
	r.top = abs(r.top);
	r.right = abs(r.right);
	r.bottom = abs(r.bottom);

	while (!MapDialogRect(hDlg, &r) || r.IsRectNull())
	{
		hDlg = GetParent(hDlg);
		if (!hDlg)
			return FALSE;
	}
	r.left = r.left * sLeft;
	r.top = r.top *sTop;
	r.right = r.right * sRight;
	r.bottom = r.bottom * sBottom;
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL SafeMapDialog(HWND hDlg, CSize& sz)
{
	CRect r = { 0, 0, sz.cx, sz.cy };
	if (!SafeMapDialog(hDlg, r))
		return FALSE;
	sz.cx = r.Width();
	sz.cy = r.Height();
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL SafeMapDialog(HWND hDlg, CPoint& pt)
{
	CRect r = { pt.x, pt.y, 0, 0 };
	if (!SafeMapDialog(hDlg, r))
		return FALSE;
	pt.x = r.left;
	pt.y = r.top;
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL ReverseMapDialog(HWND hDlg, CRect& rect)
{
	// First we get the dialog units for your dialog box
	UINT baseunitX, baseunitY;
	CRect r = { 0, 0, 4, 8 };
	while (!MapDialogRect(hDlg, &r) || r.IsRectNull())
	{
		hDlg = GetParent(hDlg);
		if (!hDlg)
			return FALSE;
	}
	baseunitX = r.right;
	baseunitY = r.bottom;

	// Now we do the conversion
	rect.left = MulDiv(rect.left, 4, baseunitX);
	rect.right = MulDiv(rect.right, 4, baseunitX);
	rect.top = MulDiv(rect.top, 8, baseunitY);
	rect.bottom = MulDiv(rect.bottom, 8, baseunitY);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ReverseMapDialog(HWND hDlg, CSize& sz)
{
	CRect r = { 0, 0, sz.cx, sz.cy };
	if (!ReverseMapDialog(hDlg, r))
		return FALSE;
	sz.cx = r.Width();
	sz.cy = r.Height();
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL ReverseMapDialog(HWND hDlg, CPoint& pt)
{
	CRect r = { pt.x, pt.y, 0, 0 };
	if (!ReverseMapDialog(hDlg, r))
		return FALSE;
	pt.x = r.left;
	pt.y = r.top;
	return TRUE;
}

class CMyMemFile : public CMemFile, public IStream
{
private:
	LONG m_Refcount;

public:

	CMyMemFile()
	{
		m_bAutoDelete = FALSE;
		m_Refcount = 1;
	}
	void SetAutoDelete(BOOL bSet) { m_bAutoDelete = bSet; }
	BYTE* GetBuffer()
	{
		return m_lpBuffer;
	}

public:

	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void ** ppvObject)
	{
		if (iid == __uuidof(IUnknown)
			|| iid == __uuidof(IStream)
			|| iid == __uuidof(ISequentialStream))
		{
			*ppvObject = static_cast<IStream*>(this);
			AddRef();
			return S_OK;
		}
		else
			return E_NOINTERFACE;
	}

	virtual ULONG STDMETHODCALLTYPE AddRef(void)
	{
		return (ULONG)InterlockedIncrement(&m_Refcount);
	}

	virtual ULONG STDMETHODCALLTYPE Release(void)
	{
		ULONG res = (ULONG)InterlockedDecrement(&m_Refcount);
		if (res == 0)
			delete this;
		return res;
	}

	// ISequentialStream Interface
public:
	virtual HRESULT STDMETHODCALLTYPE Read(void* pv, ULONG cb, ULONG* pcbRead)
	{
		ULONG n = __super::Read(pv, cb);
		if (pcbRead)
			*pcbRead = n;
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE Write(void const* pv, ULONG cb, ULONG* pcbWritten)
	{
		__super::Write(pv, cb);
		if (pcbWritten)
			*pcbWritten = cb;
		return S_OK;
	}

	// IStream Interface
public:
	virtual HRESULT STDMETHODCALLTYPE SetSize(ULARGE_INTEGER)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE CopyTo(IStream*, ULARGE_INTEGER, ULARGE_INTEGER*,
		ULARGE_INTEGER*)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Commit(DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Revert(void)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE LockRegion(ULARGE_INTEGER, ULARGE_INTEGER, DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE UnlockRegion(ULARGE_INTEGER, ULARGE_INTEGER, DWORD)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Clone(IStream **)
	{
		return E_NOTIMPL;
	}

	virtual HRESULT STDMETHODCALLTYPE Seek(LARGE_INTEGER liDistanceToMove, DWORD dwOrigin,
		ULARGE_INTEGER* lpNewFilePointer)
	{
		DWORD dwMoveMethod;

		switch (dwOrigin)
		{
		case STREAM_SEEK_SET:
			dwMoveMethod = FILE_BEGIN;
			break;
		case STREAM_SEEK_CUR:
			dwMoveMethod = FILE_CURRENT;
			break;
		case STREAM_SEEK_END:
			dwMoveMethod = FILE_END;
			break;
		default:
			return STG_E_INVALIDFUNCTION;
			break;
		}

		__super::Seek(liDistanceToMove.QuadPart, dwMoveMethod);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE Stat(STATSTG* pStatstg, DWORD grfStatFlag)
	{
		pStatstg->cbSize.QuadPart = __super::GetLength();
		return S_OK;
	}

};

//=================================================================================================
//									CStreamWriter
//=================================================================================================
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(CString& str)
{
	CStringA s = UnicodeToUTF8(str);
	Serialize(s);
}

//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(CStringA& str)
{
	Serialize<int>(str.GetLength());
	m_pStream->Write((LPCSTR)str, str.GetLength());
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(int& i)
{
	Serialize<int>(i);
}

//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(bool& b)
{
	Serialize<bool>(b);
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(double& d)
{
	Serialize<double>(d);
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(float& f)
{
	Serialize<float>(f);
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(DWORD& dw)
{
	Serialize<DWORD>(dw);
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(SerializableObj*& pT)
{
	if (pT)
	{
		Serialize<bool>(true);
		pT->SerializeBinary(*this);
	}
	else
	{
		Serialize<bool>(false);
	}
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(CWndObjDescriptionContainer& cnt)
{
	Serialize(cnt.GetCount());
	for (int i = 0; i < cnt.GetCount(); i++)
	{
		CWndObjDescription* pWnd = cnt[0];
		Serialize(pWnd);
	}
}
//-----------------------------------------------------------------------------
void CStreamWriter::Serialize(CWndObjDescription*& pWnd)
{
	Serialize(pWnd->m_Type);
	pWnd->SerializeBinary(*this);
}


//=================================================================================================
//									CStreamReader
//=================================================================================================
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(CString& str)
{
	CStringA s;
	Serialize(s);
	str = UTF8ToUnicode(s);
}

//-----------------------------------------------------------------------------
void CStreamReader::Serialize(CStringA& str)
{
	int len = 0;
	Serialize(len);
	char* pBuff = str.GetBuffer(len);
	m_pStream->Read(pBuff, len);
	str.ReleaseBuffer();
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(int& i)
{
	Serialize<int>(i);
}

//-----------------------------------------------------------------------------
void CStreamReader::Serialize(bool& b)
{
	Serialize<bool>(b);
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(double& d)
{
	Serialize<double>(d);
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(float& f)
{
	Serialize<float>(f);
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(DWORD& dw)
{
	Serialize<DWORD>(dw);
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(SerializableObj*& pT)
{
	bool b;
	Serialize(b);

	if (b)
	{
		pT = (SerializableObj*)pT->GetRuntimeClass()->CreateObject();
		pT->SerializeBinary(*this);
	}
	else
	{
		pT = NULL;
	}
}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(CWndObjDescriptionContainer& cnt)
{
	int nCount = 0;
	Serialize(nCount);
	for (int i = 0; i < nCount; i++)
	{
		CWndObjDescription* pObj = NULL;
		Serialize(pObj);

		if (pObj)
			cnt.Add(pObj);
	}

}
//-----------------------------------------------------------------------------
void CStreamReader::Serialize(CWndObjDescription*& pWnd)
{
	CWndObjDescription::WndObjType type;
	Serialize(type);

	if (type >= CLASS_MAP_SIZE)
	{
		ASSERT(FALSE);
		return;
	}
	/*if (type == Constants)
	{
		parser.BeginReadArray(szJsonItems);
		for (int i = 0; i < parser.GetCount(); i++)
		{
			parser.BeginReadObject(i);
			CString sName = parser.ReadString(szJsonName);
			Json::Value val = parser.ReadValue(szJsonValue);
			parser.m_pRootContext->m_Defines[sName] = val;
			parser.EndReadObject();
		}
		parser.EndReadArray();
		return NULL;
	}*/
	CRuntimeClass* pClass = CWndObjDescription::classMap[type];
	if (!pClass)
	{
		ASSERT(FALSE);
		return;
	}
	pWnd = (CWndObjDescription*)pClass->CreateObject();
	pWnd->SerializeBinary(*this);
}
//=================================================================================================
//									BindingInfo
//=================================================================================================

//-----------------------------------------------------------------------------
BindingInfo::~BindingInfo()
{
	delete m_pHotLink;
}
//-----------------------------------------------------------------------------
BindingInfo* BindingInfo::Clone()
{
	BindingInfo* pNew = new BindingInfo();
	pNew->Assign(this);
	return pNew;
}
//-----------------------------------------------------------------------------
void BindingInfo::SerializeJson(CJsonSerializer& strJson)
{
	if (m_strDataSource.IsEmpty() &&
		m_nButtonId == BTN_DEFAULT_BINDING &&
		m_strName.IsEmpty() &&
		m_pHotLink == NULL)
		return;
	strJson.OpenObject(szJsonBinding);
	SERIALIZE_STRING(m_strDataSource, szJsonDatasource);
	SERIALIZE_STRING(m_strName, szJsonName);
	SERIALIZE_INT(m_nButtonId, szJsonButtonId, BTN_DEFAULT_BINDING);
	if (m_pHotLink)
	{
		m_pHotLink->SerializeJson(strJson);
	}
	SERIALIZE_INT(m_nButtonId, szJsonButtonId, BTN_DEFAULT_BINDING);

	strJson.CloseObject();
}
//-----------------------------------------------------------------------------
void BindingInfo::ActivateDefines(bool bActive)
{
	__super::ActivateDefines(bActive);
	if (m_pHotLink)
		m_pHotLink->ActivateDefines(bActive);
}
//-----------------------------------------------------------------------------
void BindingInfo::ParseHotLink(CJsonFormParser& parser)
{
	if (!m_pHotLink)
		m_pHotLink = new HotLinkInfo;
	m_pHotLink->ParseJson(parser);
	
	if (m_pHotLink->IsEmpty())
	{
		delete(m_pHotLink); 
		m_pHotLink= NULL;
	}
}
//-----------------------------------------------------------------------------
void BindingInfo::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadObject(szJsonBinding))
	{
		ParseHotLink(parser);

		PARSE_STRING(m_strDataSource, szJsonDatasource);
		PARSE_STRING(m_strName, szJsonName);
		PARSE_INT(m_nButtonId, szJsonButtonId);

		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void BindingInfo::Assign(BindingInfo* pDesc)
{
	m_strDataSource = pDesc->m_strDataSource;
	m_strName = pDesc->m_strName;
	m_nButtonId = pDesc->m_nButtonId;
	delete m_pHotLink;
	m_pHotLink = pDesc->m_pHotLink ? pDesc->m_pHotLink->Clone() : NULL;
	ExpressionObject::Assign(pDesc);
}

//-----------------------------------------------------------------------------
void BindingInfo::AssignDefaults(BindingInfo* pDesc)
{
	if (m_strDataSource.IsEmpty())
		m_strDataSource = pDesc->m_strDataSource;
	if (m_strName.IsEmpty())
		m_strName = pDesc->m_strName;
	if (m_nButtonId == BTN_DEFAULT_BINDING)
		m_nButtonId = pDesc->m_nButtonId;
	if (pDesc->m_pHotLink)
	{
		if (m_pHotLink)
			m_pHotLink->AssignDefaults(pDesc->m_pHotLink);
		else
			m_pHotLink = pDesc->m_pHotLink->Clone();
	}
}


//=================================================================================================
//									HotLinkInfo
//=================================================================================================

//-----------------------------------------------------------------------------
HotLinkInfo* HotLinkInfo::Clone()
{
	HotLinkInfo* pNew = new HotLinkInfo();
	pNew->Assign(this);
	return pNew;
}

//-----------------------------------------------------------------------------
BOOL HotLinkInfo::IsEmpty()
{
	return m_strName.IsEmpty() &&
		m_strNamespace.IsEmpty() &&
		m_strAddOnFlyNs.IsEmpty() &&
		m_strSelector.IsEmpty() &&
		m_bMustExistData == false &&
		m_bEnableAddOnFly == false &&
		m_bEnableHyperLink == false &&
		m_bEnableHotLink == false &&
		m_bAutoFind == true &&
		m_arDescriptionFields.IsEmpty() &&
		m_arHotlinks.IsEmpty();
}

//-----------------------------------------------------------------------------
void HotLinkInfo::SerializeJson(CJsonSerializer& strJson, int index)
{
	if (index == -1)
		strJson.OpenObject(szJsonHotLink);
	else
		strJson.OpenObject(index);
	SERIALIZE_STRING(m_strName, szJsonName);
	SERIALIZE_STRING(m_strAddOnFlyNs, szJsonAddOnFlyNamespace);
	SERIALIZE_BOOL3(m_bMustExistData, szJsonMustExistData);
	SERIALIZE_BOOL3(m_bEnableAddOnFly, szJsonEnabledAddOnFly);
	SERIALIZE_BOOL3(m_bEnableHyperLink, szJsonEnableLink);
	SERIALIZE_BOOL3(m_bEnableHotLink, szJsonEnableHotLink);
	SERIALIZE_BOOL(m_bAutoFind, szJsonAutoFind, true);
	SERIALIZE_STRING(m_strSelector, szJsonSelector);
	if (!m_arDescriptionFields.IsEmpty())
	{
		strJson.OpenArray(szJsonFields);
		for (int i = 0; i < m_arDescriptionFields.GetCount(); i++)
		{
			strJson.WriteString(i, m_arDescriptionFields[i]);
		}
		strJson.CloseArray();
	}
	if (!m_arAdditionalSensitiveFields.IsEmpty())
	{
		strJson.OpenArray(szJsonAuxKeyFields);
		for (int i = 0; i < m_arAdditionalSensitiveFields.GetCount(); i++)
		{
			strJson.WriteString(i, m_arAdditionalSensitiveFields[i]);
		}
		strJson.CloseArray();
	}
	if (m_arHotlinks.IsEmpty())
	{
		//se c'è un array di hotlink, allora il namespace non serve, sarà sempre Framework.TbGes.TbGes.ComposedJsonHotLink
		SERIALIZE_STRING(m_strNamespace, szJsonNamespace);
	}
	else
	{
		strJson.OpenArray(szJsonItems);
		for (int i = 0; i < m_arHotlinks.GetCount(); i++)
		{
			m_arHotlinks[i]->SerializeJson(strJson, i);
		}
		strJson.CloseArray();
	}
	strJson.CloseObject();
}
//-----------------------------------------------------------------------------
void HotLinkInfo::ActivateDefines(bool bActive)
{
	__super::ActivateDefines(bActive);
	for (int i = 0; i < m_arHotlinks.GetCount(); i++)
		m_arHotlinks[i]->ActivateDefines(bActive);
}

//-----------------------------------------------------------------------------
void HotLinkInfo::ParseJson(CJsonFormParser& parser, int index)
{
	BOOL bFlat = FALSE;
	if (index != -1)
	{
		bFlat = FALSE;
		parser.BeginReadObject(index);
	}
	else
	{
		bFlat = !parser.BeginReadObject(szJsonHotLink);
	}

	if (bFlat)
	{
		PARSE_STRING(m_strName, szJsonHotLink);
		PARSE_STRING(m_strNamespace, szJsonHotLinkNs);
	}
	else
	{
		PARSE_STRING(m_strName, szJsonName);
		PARSE_STRING(m_strNamespace, szJsonNamespace);
	}
	PARSE_STRING(m_strAddOnFlyNs, szJsonAddOnFlyNamespace);
	PARSE_STRING(m_strSelector, szJsonSelector);
	PARSE_BOOL3(m_bMustExistData, szJsonMustExistData);
	PARSE_BOOL3(m_bEnableAddOnFly, szJsonEnabledAddOnFly);
	PARSE_BOOL3(m_bEnableHyperLink, szJsonEnableLink);
	PARSE_BOOL3(m_bEnableHotLink, szJsonEnableHotLink);
	PARSE_BOOL(m_bAutoFind, szJsonAutoFind);
	if (parser.BeginReadArray(szJsonFields))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			m_arDescriptionFields.Add(parser.ReadString(i));
		}
		parser.EndReadArray();
	}
	if (parser.BeginReadArray(szJsonAuxKeyFields))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			m_arAdditionalSensitiveFields.Add(parser.ReadString(i));
		}
		parser.EndReadArray();
	}
	if (parser.BeginReadArray(szJsonItems))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			HotLinkInfo* pInfo = new HotLinkInfo;
			pInfo->ParseJson(parser, i);
			if (pInfo->IsEmpty())
				delete pInfo;
			else
				m_arHotlinks.Add(pInfo);
		}
		parser.EndReadArray();
	}
	if (!bFlat)
		parser.EndReadObject();

	if (m_strNamespace.IsEmpty() && !m_arHotlinks.IsEmpty())
	{
		m_strNamespace = _T("Framework.TbGes.TbGes.ComposedJsonHotLink");
	}
}

//-----------------------------------------------------------------------------
void HotLinkInfo::Assign(HotLinkInfo* pDesc)
{
	m_strName = pDesc->m_strName;
	m_strSelector = pDesc->m_strSelector;
	m_strNamespace = pDesc->m_strNamespace;
	m_strAddOnFlyNs = pDesc->m_strAddOnFlyNs;
	m_bMustExistData = pDesc->m_bMustExistData;
	m_bEnableAddOnFly = pDesc->m_bEnableAddOnFly;
	m_bEnableHyperLink = pDesc->m_bEnableHyperLink;
	m_bEnableHotLink = pDesc->m_bEnableHotLink;
	m_bAutoFind = pDesc->m_bAutoFind;

	m_arDescriptionFields.RemoveAll();
	m_arDescriptionFields.Append(pDesc->m_arDescriptionFields);
	m_arAdditionalSensitiveFields.RemoveAll();
	m_arAdditionalSensitiveFields.Append(pDesc->m_arAdditionalSensitiveFields);
	m_arHotlinks.RemoveAll();
	for (int i = 0; i < pDesc->m_arHotlinks.GetSize(); i++)
	{
		m_arHotlinks.Add(pDesc->m_arHotlinks[i]->Clone());
	}

	ExpressionObject::Assign(pDesc);
}

//-----------------------------------------------------------------------------
void HotLinkInfo::AssignDefaults(HotLinkInfo* pDesc)
{
	if (m_strName.IsEmpty())
		m_strName = pDesc->m_strName;
	if (m_strSelector.IsEmpty())
		m_strSelector = pDesc->m_strSelector;
	if (m_strNamespace.IsEmpty())
		m_strNamespace = pDesc->m_strNamespace;
	if (m_strAddOnFlyNs.IsEmpty())
		m_strAddOnFlyNs = pDesc->m_strAddOnFlyNs;
	if (m_bMustExistData == B_UNDEFINED)
		m_bMustExistData = pDesc->m_bMustExistData;
	if (m_bEnableAddOnFly == B_UNDEFINED)
		m_bEnableAddOnFly = pDesc->m_bEnableAddOnFly;
	if (m_bEnableHyperLink == B_UNDEFINED)
		m_bEnableHyperLink = pDesc->m_bEnableHyperLink;
	if (m_bEnableHotLink == B_UNDEFINED)
		m_bEnableHotLink = pDesc->m_bEnableHotLink;
	if (m_bAutoFind)
		m_bAutoFind = pDesc->m_bAutoFind;
	if (m_arDescriptionFields.IsEmpty())
		m_arDescriptionFields.Append(pDesc->m_arDescriptionFields);
	if (m_arAdditionalSensitiveFields.IsEmpty())
		m_arAdditionalSensitiveFields.Append(pDesc->m_arAdditionalSensitiveFields);

	if (m_arHotlinks.IsEmpty())
	{
		for (int i = 0; i < pDesc->m_arHotlinks.GetSize(); i++)
		{
			m_arHotlinks.Add(pDesc->m_arHotlinks[i]->Clone());
		}
	}
}

//-----------------------------------------------------------------------------
StateData* StateData::Clone()
{
	StateData* pNew = new StateData();
	pNew->Assign(this);
	return pNew;
}
//-----------------------------------------------------------------------------
void StateData::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenObject(szJsonStateData);
	if (m_pBindings)
	{
		m_pBindings->SerializeJson(strJson);
	}
	if (m_bInvertDefaultStates != B_UNDEFINED)
		strJson.WriteBool(szJsonInvertState, m_bInvertDefaultStates == B_TRUE);

	strJson.WriteBool(szJsonEnableStateInEdit, m_bEnableStateInEdit);
	strJson.WriteBool(szJsonEnableCtrlInEdit, m_bEnableStateCtrlInEdit);

	strJson.CloseObject();
}

//-----------------------------------------------------------------------------
void StateData::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadObject(szJsonStateData))
	{
		if (parser.Has(szJsonBinding))
		{
			m_pBindings = new BindingInfo();
			m_pBindings->ParseJson(parser);
		}
		if (parser.Has(szJsonInvertState))
			m_bInvertDefaultStates = parser.ReadBool(szJsonInvertState) ? B_TRUE : B_FALSE;

		if (parser.Has(szJsonEnableStateInEdit))
			m_bEnableStateInEdit = parser.ReadBool(szJsonEnableStateInEdit);

		if (parser.Has(szJsonEnableCtrlInEdit))
			m_bEnableStateCtrlInEdit = parser.ReadBool(szJsonEnableCtrlInEdit);

		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void StateData::Assign(StateData* pDesc)
{
	m_bInvertDefaultStates = pDesc->m_bInvertDefaultStates;
	m_bEnableStateInEdit = pDesc->m_bEnableStateInEdit;
	m_bEnableStateCtrlInEdit = pDesc->m_bEnableStateCtrlInEdit;
	delete m_pBindings;
	m_pBindings = NULL;
	if (pDesc->m_pBindings)
	{
		m_pBindings = pDesc->m_pBindings->Clone();
	}
}
//-----------------------------------------------------------------------------
void StateData::AssignDefaults(StateData* pDesc)
{
	if (m_bInvertDefaultStates == B_UNDEFINED)
		m_bInvertDefaultStates = pDesc->m_bInvertDefaultStates;
	if (pDesc->m_pBindings)
	{
		if (!m_pBindings)
			m_pBindings = new BindingInfo();
		m_pBindings->AssignDefaults(pDesc->m_pBindings);
	}
}


/////////////////////////////////////////////////////////////////////////////
//			CControlBehaviourDescription
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void CControlBehaviourDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenObject(szJsonControlBehaviour);
	SERIALIZE_STRING(m_strName, szJsonName);
	SERIALIZE_STRING(m_strNamespace, szJsonNamespace);
	SERIALIZE_BOOL(m_bItemSource, szJsonItemSource, false);
	SERIALIZE_BOOL(m_bDataAdapter, szJsonDataAdapter, false);
	SERIALIZE_BOOL(m_bValidator, szJsonValidator, false);
	SERIALIZE_BOOL(m_bCommandHandler, szJsonCommandHandler, false);
	strJson.CloseObject();
}

//-----------------------------------------------------------------------------
void CControlBehaviourDescription::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadObject(szJsonControlBehaviour))
	{
		PARSE_STRING(m_strName, szJsonName);
		PARSE_STRING(m_strNamespace, szJsonNamespace);
		PARSE_BOOL(m_bItemSource, szJsonItemSource);
		PARSE_BOOL(m_bDataAdapter, szJsonDataAdapter);
		PARSE_BOOL(m_bValidator, szJsonValidator);
		PARSE_BOOL(m_bCommandHandler, szJsonCommandHandler);
		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void CControlBehaviourDescription::Assign(CControlBehaviourDescription* pDesc)
{
	m_strName = pDesc->m_strName;
	m_strNamespace = pDesc->m_strNamespace;
	m_bItemSource = pDesc->m_bItemSource;
	m_bDataAdapter = pDesc->m_bDataAdapter;
	m_bValidator = pDesc->m_bValidator;
	m_bCommandHandler = pDesc->m_bCommandHandler;
	ExpressionObject::Assign(pDesc);
}
//-----------------------------------------------------------------------------
CControlBehaviourDescription*  CControlBehaviourDescription::Clone()
{
	CControlBehaviourDescription* pNew = new CControlBehaviourDescription();
	pNew->Assign(this);
	return pNew;
}


/////////////////////////////////////////////////////////////////////////////
//			CItemSourceDescription
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void CItemSourceDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenObject(szJsonItemSource);
	SERIALIZE_STRING(m_strItemSourceName, szJsonName);
	SERIALIZE_STRING(m_strItemSourceNamespace, szJsonNamespace);
	SERIALIZE_STRING(m_strItemSourceParameter, szJsonParameter);
	SERIALIZE_BOOL(m_bItemSourceUseProductLanguage, szJsonUseProductLanguage, false);
	SERIALIZE_BOOL(m_bAllowChanges, szJsonAllowChanges, false);

	strJson.CloseObject();
}

//-----------------------------------------------------------------------------
void CItemSourceDescription::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadObject(szJsonItemSource))
	{
		PARSE_STRING(m_strItemSourceName, szJsonName);
		PARSE_STRING(m_strItemSourceNamespace, szJsonNamespace);
		PARSE_STRING(m_strItemSourceParameter, szJsonParameter);
		PARSE_BOOL(m_bItemSourceUseProductLanguage, szJsonUseProductLanguage);
		PARSE_BOOL(m_bAllowChanges, szJsonAllowChanges);

		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void CItemSourceDescription::Assign(CItemSourceDescription* pDesc)
{
	m_strItemSourceName = pDesc->m_strItemSourceName;
	m_strItemSourceNamespace = pDesc->m_strItemSourceNamespace;
	m_strItemSourceParameter = pDesc->m_strItemSourceParameter;
	m_bItemSourceUseProductLanguage = pDesc->m_bItemSourceUseProductLanguage;
	m_bAllowChanges = pDesc->m_bAllowChanges;
	ExpressionObject::Assign(pDesc);
}
//-----------------------------------------------------------------------------
CItemSourceDescription*  CItemSourceDescription::Clone()
{
	CItemSourceDescription* pNew = new CItemSourceDescription();
	pNew->Assign(this);
	return pNew;
}

//-----------------------------------------------------------------------------
void CValidatorDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_STRING(m_strValidatorName, szJsonName);
	SERIALIZE_STRING(m_strValidatorNamespace, szJsonNamespace);
}

//-----------------------------------------------------------------------------
void CValidatorDescription::ParseJson(CJsonFormParser& parser)
{
	PARSE_STRING(m_strValidatorName, szJsonName);
	PARSE_STRING(m_strValidatorNamespace, szJsonNamespace);
}

//-----------------------------------------------------------------------------
void CValidatorDescription::Assign(CValidatorDescription* pDesc)
{
	m_strValidatorName = pDesc->m_strValidatorName;
	m_strValidatorNamespace = pDesc->m_strValidatorNamespace;
}

//-----------------------------------------------------------------------------
CValidatorDescription*  CValidatorDescription::Clone()
{
	CValidatorDescription* pNew = new CValidatorDescription();
	pNew->Assign(this);
	return pNew;
}

//-----------------------------------------------------------------------------
void CDataAdapterDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenObject(szJsonDataAdapter);
	SERIALIZE_STRING(m_strDataAdapterName, szJsonName);
	SERIALIZE_STRING(m_strDataAdapterNamespace, szJsonNamespace);
	strJson.CloseObject();
}

//-----------------------------------------------------------------------------
void CDataAdapterDescription::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadObject(szJsonDataAdapter))
	{
		PARSE_STRING(m_strDataAdapterName, szJsonName);
		PARSE_STRING(m_strDataAdapterNamespace, szJsonNamespace);

		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void CDataAdapterDescription::Assign(CDataAdapterDescription* pDesc)
{
	m_strDataAdapterName = pDesc->m_strDataAdapterName;
	m_strDataAdapterNamespace = pDesc->m_strDataAdapterNamespace;
	ExpressionObject::Assign(pDesc);
}

//-----------------------------------------------------------------------------
CDataAdapterDescription*  CDataAdapterDescription::Clone()
{
	CDataAdapterDescription* pNew = new CDataAdapterDescription();
	pNew->Assign(this);
	return pNew;
}

/////////////////////////////////////////////////////////////////////////////
//			CAcceleratorDescription
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CAcceleratorDescription::CAcceleratorDescription(const AcceleratorArray& hAccelTables)
{
	for (int i = 0; i < hAccelTables.GetCount(); i++)
	{
		int nCount = CopyAcceleratorTable(hAccelTables[i], 0, 0);
		ACCEL* pAccel = new ACCEL[nCount];
		CopyAcceleratorTable(hAccelTables[i], pAccel, nCount);
		for (int j = 0; j < nCount; j++)
		{
			CAcceleratorItemDescription* pItem = new CAcceleratorItemDescription();
			pItem->m_Accel = pAccel[j];
			m_arItems.Add(pItem);
		}
		delete pAccel;
	}

}

//-----------------------------------------------------------------------------
void CAcceleratorDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.OpenArray(szJsonAccelerators);
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		CAcceleratorItemDescription* pItem = m_arItems[i];
		ACCEL* accel = &pItem->m_Accel;
		strJson.OpenObject(i);

		strJson.WriteInt(szJsonKey, accel->key);

		if ((accel->fVirt & FVIRTKEY) == FVIRTKEY)
			strJson.WriteBool(szJsonVirtualKey, true);

		if ((accel->fVirt & FCONTROL) == FCONTROL)
			strJson.WriteBool(szJsonControl, true);

		if ((accel->fVirt & FALT) == FALT)
			strJson.WriteBool(szJsonAlt, true);

		if ((accel->fVirt & FSHIFT) == FSHIFT)
			strJson.WriteBool(szJsonShift, true);

		//obsoleto
		//if ((accel->fVirt & FNOINVERT) == FNOINVERT)
		//	strJson.WriteBool(szJsonNoInvert, true);

		if (!pItem->m_sId.IsEmpty())
			strJson.WriteString(szJsonId, pItem->m_sId);

		if (!pItem->m_sActivation.IsEmpty())
			strJson.WriteString(szJsonActivation, pItem->m_sActivation);
		strJson.CloseObject();
	}
	strJson.CloseArray();
}

//-----------------------------------------------------------------------------
void CAcceleratorDescription::ParseJson(CJsonFormParser& parser)
{
	if (parser.BeginReadArray(szJsonAccelerators))
	{
		int nCount = parser.GetCount();

		for (int i = 0; i < nCount; i++)
		{
			parser.BeginReadObject(i);
			CAcceleratorItemDescription* pItem = new CAcceleratorItemDescription();

			if (parser.Has(szJsonFlvirt))
				pItem->m_Accel.fVirt = parser.ReadInt(szJsonFlvirt);
			if (parser.Has(szJsonKey))
				pItem->m_Accel.key = parser.ReadInt(szJsonKey);
			if (parser.Has(szJsonCmd))
				pItem->m_Accel.cmd = parser.ReadInt(szJsonCmd);
			PARSE_STRING(pItem->m_sId, szJsonId);
			PARSE_STRING(pItem->m_sActivation, szJsonActivation);
			bool b;
			if (parser.TryReadBool(szJsonVirtualKey, b) && b)
				pItem->m_Accel.fVirt |= FVIRTKEY;
			if (parser.TryReadBool(szJsonControl, b) && b)
				pItem->m_Accel.fVirt |= FCONTROL;
			if (parser.TryReadBool(szJsonAlt, b) && b)
				pItem->m_Accel.fVirt |= FALT;
			if (parser.TryReadBool(szJsonShift, b) && b)
				pItem->m_Accel.fVirt |= FSHIFT;
			//obsoleto
			//if (parser.TryReadBool(szJsonNoInvert, b) && b)
			//	pItem->m_Accel.fVirt |= FNOINVERT;

			if (pItem->m_Accel.cmd == 0 && !pItem->m_sId.IsEmpty())
			{
				//prima provo a vedere se è un comando
				pItem->m_Accel.cmd = AfxGetTBResourcesMap()->GetExistingTbResourceID(pItem->m_sId, TbCommands);
				//se non lo trovo come comando, provo a vedere se è un controllo
				if (pItem->m_Accel.cmd == 0)
					pItem->m_Accel.cmd = AfxGetTBResourcesMap()->GetExistingTbResourceID(pItem->m_sId, TbControls);
				//se non è nemmeno un controllo, lo genero ex novo come comando
				if (pItem->m_Accel.cmd == 0)
					pItem->m_Accel.cmd = AfxGetTBResourcesMap()->GetTbResourceID(pItem->m_sId, TbCommands);
			}
			m_arItems.Add(pItem);
			parser.EndReadObject();

		}
		parser.EndReadArray();
	}
}

//-----------------------------------------------------------------------------
void CAcceleratorDescription::Assign(CAcceleratorDescription* pDesc)
{
	Clear();
	Append(pDesc);
	ExpressionObject::Assign(pDesc);
}
//-----------------------------------------------------------------------------
void CAcceleratorDescription::Append(CAcceleratorDescription* pDesc)
{
	if (!pDesc)
		return;
	for (int i = 0; i < pDesc->m_arItems.GetCount(); i++)
	{
		m_arItems.Add(pDesc->m_arItems[i]->Clone());
	}
}
//-----------------------------------------------------------------------------
CAcceleratorDescription* CAcceleratorDescription::Clone()
{
	CAcceleratorDescription* pNewAccDesc = new CAcceleratorDescription();
	pNewAccDesc->Assign(this);
	return pNewAccDesc;
}

//-----------------------------------------------------------------------------
ACCEL* CAcceleratorDescription::ToACCEL(CJsonContextObj* pContext, int& nSize)
{
	nSize = 0;
	ACCEL* pAccel = new ACCEL[m_arItems.GetCount()];
	ZeroMemory(pAccel, m_arItems.GetCount());
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		CAcceleratorItemDescription *pItem = m_arItems[i];
		if (!pContext || pContext->CheckActivationExpression(pItem->m_sActivation))
			pAccel[nSize++] = pItem->m_Accel;

	}
	return pAccel;
}
//-----------------------------------------------------------------------------
CString CAcceleratorDescription::GetDescription(WORD id)
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		CAcceleratorItemDescription* pItem = m_arItems[i];
		if (pItem->m_Accel.cmd == id)
			return GetAcceleratorText(pItem->m_Accel);
	}
	return _T("");
}
//Funzione che scrive una CBitmap su uno stream binario
//-----------------------------------------------------------------------------
BOOL WriteBmpMemFile(HBITMAP bitmap, HDC hDC, CMyMemFile* file)
{
	BITMAP bmp;
	PBITMAPINFO pbmi;
	WORD cClrBits;
	BITMAPFILEHEADER hdr; // bitmap file-header 
	PBITMAPINFOHEADER pbih; // bitmap info-header 
	LPBYTE lpBits; // memory pointer 
	DWORD dwTotal; // total count of bytes 
	DWORD cb; // incremental count of bytes 
	BYTE *hp; // byte pointer 

	// create the bitmapinfo header information
	if (!GetObject(bitmap, sizeof(BITMAP), (LPSTR)&bmp))
		return FALSE;

	// Convert the color format to a count of bits. 
	cClrBits = (WORD)(bmp.bmPlanes * bmp.bmBitsPixel);
	if (cClrBits == 1)
		cClrBits = 1;
	else if (cClrBits <= 4)
		cClrBits = 4;
	else if (cClrBits <= 8)
		cClrBits = 8;
	else if (cClrBits <= 16)
		cClrBits = 16;
	else if (cClrBits <= 24)
		cClrBits = 24;
	else cClrBits = 32;

	// Allocate memory for the BITMAPINFO structure.
	if (cClrBits != 24)
		pbmi = (PBITMAPINFO)LocalAlloc(LPTR, sizeof(BITMAPINFOHEADER) + sizeof(RGBQUAD) * (1 << cClrBits));
	else
		pbmi = (PBITMAPINFO)LocalAlloc(LPTR, sizeof(BITMAPINFOHEADER));

	// Initialize the fields in the BITMAPINFO structure. 
	pbmi->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	pbmi->bmiHeader.biWidth = bmp.bmWidth;
	pbmi->bmiHeader.biHeight = bmp.bmHeight;
	pbmi->bmiHeader.biPlanes = bmp.bmPlanes;
	pbmi->bmiHeader.biBitCount = bmp.bmBitsPixel;
	if (cClrBits < 24)
		pbmi->bmiHeader.biClrUsed = (1 << cClrBits);

	// If the bitmap is not compressed, set the BI_RGB flag. 
	pbmi->bmiHeader.biCompression = BI_RGB;

	// Compute the number of bytes in the array of color 
	// indices and store the result in biSizeImage. 
	pbmi->bmiHeader.biSizeImage = (pbmi->bmiHeader.biWidth + 7) / 8 * pbmi->bmiHeader.biHeight * cClrBits;
	// Set biClrImportant to 0, indicating that all of the 
	// device colors are important. 
	pbmi->bmiHeader.biClrImportant = 0;

	// now open file and save the data
	pbih = (PBITMAPINFOHEADER)pbmi;
	lpBits = (LPBYTE)GlobalAlloc(GMEM_FIXED, pbih->biSizeImage);

	if (!lpBits)
		return FALSE;

	// Retrieve the color table (RGBQUAD array) and the bits 
	if (!GetDIBits(hDC, HBITMAP(bitmap), 0, (WORD)pbih->biHeight, lpBits, pbmi, DIB_RGB_COLORS))
		return FALSE;

	hdr.bfType = 0x4d42; // 0x42 = "B" 0x4d = "M" 
	// Compute the size of the entire file. 
	hdr.bfSize = (DWORD)(sizeof(BITMAPFILEHEADER) + pbih->biSize + pbih->biClrUsed * sizeof(RGBQUAD) + pbih->biSizeImage);
	hdr.bfReserved1 = 0;
	hdr.bfReserved2 = 0;

	// Compute the offset to the array of color indices. 
	hdr.bfOffBits = (DWORD) sizeof(BITMAPFILEHEADER) + pbih->biSize + pbih->biClrUsed * sizeof(RGBQUAD);

	ULONG nWritten;
	// Copy the BITMAPFILEHEADER into the .BMP file. 
	file->Write((BYTE*)&hdr, sizeof(BITMAPFILEHEADER), &nWritten);

	// Copy the BITMAPINFOHEADER and RGBQUAD array into the file. 
	file->Write((BYTE*)pbih, sizeof(BITMAPINFOHEADER), &nWritten);

	// Copy the array of color indices into the .BMP file. 
	dwTotal = cb = pbih->biSizeImage;
	hp = lpBits;
	file->Write((BYTE*)(LPSTR)hp, (int)cb, &nWritten);

	// Free memory. 
	GlobalFree((HGLOBAL)lpBits);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CImageBuffer::Assign(HBITMAP hBitmap, const CString& strId, HDC hDc)
{
	if (m_strId == strId)
		return FALSE;

	CImage image;
	image.Attach(hBitmap);
	Assign(&image, strId);
	image.Detach();
	//An. 17405
	//Se e' fallito il salvataggio della CImage nel Buffer, la dimensione e' 0.
	//Scrivo quindi direttamente il contenuto della bitmap sullo stream
	if (m_nSize == 0)
	{
		CMyMemFile file;
		SAFE_DELETE(m_pBuffer);
		BOOL bRet = WriteBmpMemFile(hBitmap, hDc, &file);
		m_strId = strId;
		m_nSize = (UINT)file.GetLength();
		m_pBuffer = file.GetBuffer();
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CImageBuffer::Assign(HBITMAP hBitmap, const CString& strId, CWnd* pWnd)
{
	if (m_strId == strId || !hBitmap)
		return FALSE;

	CImage image;
	image.Attach(hBitmap);
	Assign(&image, strId);
	image.Detach();
	//An. 17405
	//Se e' fallito il salvataggio della CImage nel Buffer, la dimensione e' 0.
	//Scrivo quindi direttamente il contenuto della bitmap sullo stream
	if (m_nSize == 0)
	{
		CMyMemFile file;

		SAFE_DELETE(m_pBuffer);
		//creo un DC compatible da passare al metodo che salva la bitmap su stream
		HDC hdcCompatible = ::CreateCompatibleDC(pWnd->GetDC()->m_hDC);
		BOOL bRet = WriteBmpMemFile(hBitmap, hdcCompatible, &file);
		m_strId = strId;
		m_nSize = (UINT)file.GetLength();
		m_pBuffer = file.GetBuffer();
		//Distruggo il DC
		::DeleteDC(hdcCompatible);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CImageBuffer::Assign(CImage* pImage, const CString& strId)
{
	if (m_strId == strId)
		return FALSE;

	SAFE_DELETE(m_pBuffer);

	CMyMemFile file;
	pImage->Save(&file, Gdiplus::ImageFormatPNG);
	m_strId = strId;
	m_nSize = (UINT)file.GetLength();
	m_pBuffer = file.GetBuffer();
	return TRUE;
}


//funzione che ritorna l'encoder da usare per il formato passato come parametro
//-----------------------------------------------------------------------------
BOOL GetEncoderClsid(const WCHAR* format, CLSID* pClsid)
{
	UINT num, size;
	Gdiplus::GetImageEncodersSize(&num, &size);
	Gdiplus::ImageCodecInfo* pImageCodecInfo = (Gdiplus::ImageCodecInfo*)(malloc(size));
	Gdiplus::GetImageEncoders(num, size, pImageCodecInfo);
	BOOL found = FALSE;
	for (UINT i = 0; !found && i < num; i++)
	{
		if (_wcsicmp(pImageCodecInfo[i].MimeType, format) == 0)
		{
			*pClsid = pImageCodecInfo[i].Clsid;
			found = TRUE;
		}
	}
	free(pImageCodecInfo);
	return found;
}

//-----------------------------------------------------------------------------
BOOL CImageBuffer::Assign(Gdiplus::Image* pImage, const CString& strId)
{
	//Se l'ID e' lo stesso vuol dire che l'immagine non e' cambiata, evito di riassegnarla
	if (m_strId == strId)
		return FALSE;

	SAFE_DELETE(m_pBuffer);

	CMyMemFile file;
	CLSID encoder;
	//recupero l'encoder per il formato png, in cui voglio salvare
	if (!GetEncoderClsid(_T("image/png"), &encoder))
		return FALSE;
	//salvo utilizzando l'encoder appropriato
	Gdiplus::Status status = pImage->Save(&file, &encoder);
	if (status != Gdiplus::Ok)
		return FALSE;

	m_strId = strId;
	m_nSize = (UINT)file.GetLength();
	m_pBuffer = file.GetBuffer();
	return TRUE;
}
//-----------------------------------------------------------------------------
CImageBuffer::~CImageBuffer()
{
	SAFE_DELETE(m_pBuffer);
}
//-----------------------------------------------------------------------------
void CImageBuffer::Assign(CImageBuffer* pBuff)
{
	m_strId = pBuff->m_strId;
	m_nSize = pBuff->m_nSize;
	m_pBuffer = new BYTE[m_nSize];
	memcpy(m_pBuffer, pBuff->m_pBuffer, m_nSize);
}

//-----------------------------------------------------------------------------
void CImageBuffer::ToImage(CImage& image)
{
	ULONG written;
	CMyMemFile file;
	file.SetAutoDelete(TRUE);
	file.Write(m_pBuffer, m_nSize, &written);
	file.SeekToBegin();
	image.Load(&file);
}
IMPLEMENT_DYNCREATE(CWndObjDescription, CObject)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, Undefined)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, GenericWndObj)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, TreeAdv)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, HeaderStrip)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, PropertyGrid)
REGISTER_WND_OBJ_CLASS(CWndObjDescription, DockingPane)

CRuntimeClass* CWndObjDescription::classMap[CLASS_MAP_SIZE] = { 0 };
EnumDescriptionAssociations CWndObjDescription::singletonEnumDescription;
//-----------------------------------------------------------------------------
bool CWndObjDescription::RegisterClassMap(CRuntimeClass* pClass, WndObjType type)
{
	CWndObjDescription::classMap[type] = pClass;
	return true;
}


//-----------------------------------------------------------------------------
CWndObjDescription::CWndObjDescription()
	:
	m_pParent(NULL)
{
	m_Children.SetParent(this);
}

//-----------------------------------------------------------------------------
CWndObjDescription::CWndObjDescription(CWndObjDescription* pParent)
	:
	m_pParent(pParent)
{
	m_Children.SetParent(this);
}

//-----------------------------------------------------------------------------
CWndObjDescription::~CWndObjDescription(void)
{
	delete	(m_pAccelerator);
	delete	(m_pMenu);
	delete	(m_pFontDescription);
	delete	(m_pBindings);
	delete	(m_pStateData);
	delete	(m_pCaptionFontDescription);
	delete	(m_pNumbererDescription);
	delete	(m_pControlBehaviourDescription);
	if (m_hAssociatedWnd)
		CJsonFormEngineObj::GetInstance()->RemoveAssociation(m_hAssociatedWnd, this);
}
//-----------------------------------------------------------------------------
void CWndObjDescription::Dump(CDumpContext& dc) const
{
	__super::Dump(dc);
	dc << GetID();
	dc << m_strName;
}

//-----------------------------------------------------------------------------
CRect CWndObjDescription::GetRect() const
{
	return CRect(CPoint(m_X, m_Y), CSize(m_Width, m_Height));
}

//-----------------------------------------------------------------------------
void  CWndObjDescription::SetRect(const CRect& r, BOOL bSetUpdated)
{
	m_X = r.left;
	m_Y = r.top;
	m_Width = r.Width();
	m_Height = r.Height();
	if (bSetUpdated)
	{
		SetUpdated(&m_X);
		SetUpdated(&m_Y);
		SetUpdated(&m_Width);
		SetUpdated(&m_Height);
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep /*= true*/)
{
	//if (m_Expressions.m_bEvaluated)
	//	return;
	m_Expressions.m_bEvaluated = true;
	if (!pJsonContext->Evaluate(m_Expressions, this))
		return;

	if (m_pBindings)
	{
		pJsonContext->Evaluate(m_pBindings->m_Expressions, this);
		if (m_pBindings->m_pHotLink)
			pJsonContext->Evaluate(m_pBindings->m_pHotLink->m_Expressions, this);
	}
	if (m_pAccelerator)
		pJsonContext->Evaluate(m_pAccelerator->m_Expressions, this);
	if (m_pMenu)
		pJsonContext->Evaluate(m_pMenu->m_Expressions, this);
	if (m_pControlBehaviourDescription)
		pJsonContext->Evaluate(m_pControlBehaviourDescription->m_Expressions, this);
	if (deep)
	{
		for (int i = 0; i < m_Children.GetCount(); i++)
			m_Children.GetAt(i)->EvaluateExpressions(pJsonContext, true);
	}
}


//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::GetRoot()
{
	CWndObjDescription* pParent = GetParent();
	return pParent ? pParent->GetRoot() : this;
}
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::Find(const CString& id)
{
	if (HasID(id))
		return this;
	for (int i = 0; i < m_Children.GetCount(); i++)
	{
		CWndObjDescription* pFound = m_Children.GetAt(i)->Find(id);
		if (pFound)
			return pFound;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CWndObjDescription::FindAll(const CString& id, CArray<CWndObjDescription*>&ar)
{
	if (HasID(id))
		ar.Add(this);
	for (int i = 0; i < m_Children.GetCount(); i++)
	{
		m_Children.GetAt(i)->FindAll(id, ar);
	}
}
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::FindUpwards(const CString& id)
{
	if (HasID(id))
		return this;
	return m_pParent ? m_pParent->FindUpwards(id) : NULL;
}
//-----------------------------------------------------------------------------
void CWndObjDescription::ActivateDefines(bool bActive)
{
	__super::ActivateDefines(bActive);
	if (m_pAccelerator)
		m_pAccelerator->ActivateDefines(bActive);
	if (m_pBindings)
		m_pBindings->ActivateDefines(bActive);
	for (int i = 0; i < m_Children.GetCount(); i++)
		m_Children[i]->ActivateDefines(bActive);
}
//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(WndObjType value)
{
	return singletonEnumDescription.m_arWndObjType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void CWndObjDescription::GetEnumValue(CString description, WndObjType& retVal)
{
	retVal = singletonEnumDescription.m_arWndObjType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(EtchedFrameType value)
{
	return singletonEnumDescription.m_arEtchedFrameType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, EtchedFrameType& retVal)
{
	retVal = singletonEnumDescription.m_arEtchedFrameType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(TextAlignment value)
{
	return singletonEnumDescription.m_arTextAlignment.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, TextAlignment& retVal)
{
	retVal = singletonEnumDescription.m_arTextAlignment.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(VerticalAlignment value)
{
	return singletonEnumDescription.m_arVerticalAlignment.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, VerticalAlignment& retVal)
{
	retVal = singletonEnumDescription.m_arVerticalAlignment.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(WndDescriptionState value)
{
	return singletonEnumDescription.m_arWndDescriptionState.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, WndDescriptionState& retVal)
{
	retVal = singletonEnumDescription.m_arWndDescriptionState.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(ResizableControl value)
{
	return singletonEnumDescription.m_arResizableControl.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, ResizableControl& retVal)
{
	retVal = singletonEnumDescription.m_arResizableControl.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(OwnerDrawType value)
{
	return singletonEnumDescription.m_arOwnerDrawType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, OwnerDrawType& retVal)
{
	retVal = singletonEnumDescription.m_arOwnerDrawType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(ComboType value)
{
	return singletonEnumDescription.m_arComboType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, ComboType& retVal)
{
	retVal = singletonEnumDescription.m_arComboType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(SelectionType value)
{
	return singletonEnumDescription.m_arSelectionType.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, SelectionType& retVal)
{
	retVal = singletonEnumDescription.m_arSelectionType.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/CString CWndObjDescription::GetEnumDescription(SplitterMode value)
{
	return singletonEnumDescription.m_arSplitterMode.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/void	CWndObjDescription::GetEnumValue(CString description, SplitterMode& retVal)
{
	retVal = singletonEnumDescription.m_arSplitterMode.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/ CString CWndObjDescription::GetEnumDescription(CommandCategory value)
{
	return singletonEnumDescription.m_arCommandCategory.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CWndObjDescription::GetEnumValue(CString description, CommandCategory& retVal)
{
	retVal = singletonEnumDescription.m_arCommandCategory.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/ CString CWndObjDescription::GetEnumDescription(SpinCtrlAlignment value)
{
	return singletonEnumDescription.m_arSpinCtrlAlignment.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CWndObjDescription::GetEnumValue(CString description, SpinCtrlAlignment& retVal)
{
	retVal = singletonEnumDescription.m_arSpinCtrlAlignment.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/ CString CWndObjDescription::GetEnumDescription(ViewCategory value)
{
	return singletonEnumDescription.m_arViewCategory.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CWndObjDescription::GetEnumValue(CString description, ViewCategory& retVal)
{
	retVal = singletonEnumDescription.m_arViewCategory.GetEnum(description);
}
//-----------------------------------------------------------------------------
/*static*/ CString CWndObjDescription::GetEnumDescription(ControlStyle value)
{
	return singletonEnumDescription.m_arControlStyles.GetDescription(value, true);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CWndObjDescription::GetEnumValue(CString description, ControlStyle& retVal)
{
	retVal = singletonEnumDescription.m_arControlStyles.GetEnum(description, true);
}
//-----------------------------------------------------------------------------
/*static*/ CString CListCtrlDescription::GetEnumDescription(ListCtrlViewMode value)
{
	return singletonEnumDescription.m_arListCtrlViewMode.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CListCtrlDescription::GetEnumValue(CString description, ListCtrlViewMode& retVal)
{
	retVal = singletonEnumDescription.m_arListCtrlViewMode.GetEnum(description);
}

//-----------------------------------------------------------------------------
/*static*/ CString CListCtrlDescription::GetEnumDescription(ListCtrlAlign value)
{
	return singletonEnumDescription.m_arListCtrlAlign.GetDescription(value);
}

//-----------------------------------------------------------------------------
/*static*/ void	 CListCtrlDescription::GetEnumValue(CString description, ListCtrlAlign& retVal)
{
	retVal = singletonEnumDescription.m_arListCtrlAlign.GetEnum(description);
}



//-----------------------------------------------------------------------------
bool CWndObjDescription::IsUpdated(void* pField)
{
	if (!pField)
		return false;
	for (int i = 0; i < m_arUpdated.GetCount(); i++)
		if (m_arUpdated[i] == pField)
			return true;
	return false;
}
//-----------------------------------------------------------------------------
void CWndObjDescription::SetUpdated(void* pField)
{
	if (m_descState != ADDED)
	{
		m_descState = UPDATED;
		if (!IsUpdated(pField))
			m_arUpdated.Add(pField);
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::SetRemoved()
{
	if (m_descState != REMOVED)
	{
		m_descState = REMOVED;
		m_arUpdated.RemoveAll();
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::SetDeepRemoved()
{
	SetRemoved();
	for (int i = 0; i <= m_Children.GetUpperBound(); i++)
	{
		CWndObjDescription* pChild = m_Children.GetAt(i);
		pChild->SetDeepRemoved();
	}
}
//-----------------------------------------------------------------------------
void CWndObjDescription::SetParsed(bool deep)
{
	m_descState = PARSED;
	m_arUpdated.RemoveAll();
	if (!deep)
		return;
	for (int i = 0; i <= m_Children.GetUpperBound(); i++)
	{
		CWndObjDescription* pChild = m_Children.GetAt(i);
		pChild->SetParsed(deep);
	}
}
//-----------------------------------------------------------------------------
void CWndObjDescription::SetUnchanged(bool deep)
{
	if (m_descState != UNCHANGED)
	{
		m_descState = UNCHANGED;
		m_arUpdated.RemoveAll();
	}
	if (!deep)
		return;
	for (int i = 0; i <= m_Children.GetUpperBound(); i++)
	{
		CWndObjDescription* pChild = m_Children.GetAt(i);
		pChild->SetUnchanged(deep);
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::GetResources(CArray<CJsonResource*>& arFiles)
{
	CWndObjDescription* pRoot = GetRootOfFile();
	while (pRoot)
	{
		for (int i = 0; i < pRoot->m_Resources.GetCount(); i++)
			arFiles.Add(pRoot->m_Resources[i]);

		pRoot = pRoot->m_pParent ? pRoot->m_pParent->GetRootOfFile() : NULL;
	}
}

//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::GetRootOfFile()
{
	if (m_Resources.IsEmpty())
		return m_pParent ? m_pParent->GetRootOfFile() : this;
	return this;
}

//-----------------------------------------------------------------------------
CJsonResource CWndObjDescription::GetResource()
{
	CJsonResource res;
	int n = m_Resources.GetUpperBound();
	if (n > -1)
		res = *m_Resources[n];
	return res;

}
//-----------------------------------------------------------------------------
void CWndObjDescription::AttachTo(HWND hwnd)
{
	m_hAssociatedWnd = hwnd;
	CJsonFormEngineObj::GetInstance()->AddAssociation(hwnd, this);
}
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::GetFrom(HWND hwnd)
{
	return CJsonFormEngineObj::GetInstance()->GetAssociation(hwnd);
}
//-----------------------------------------------------------------------------
void CWndObjDescription::SetAdded(bool deep)
{
	m_descState = ADDED;
	if (!deep)
		return;
	for (int i = 0; i <= m_Children.GetUpperBound(); i++)
	{
		CWndObjDescription* pChild = m_Children.GetAt(i);
		pChild->SetAdded(deep);
	}
}


//-----------------------------------------------------------------------------
void CWndObjDescription::PopulateDeltaDesc(CWndObjDescriptionContainer* pDeltaContainer)
{
	for (int i = 0; i <= m_Children.GetUpperBound(); i++)
	{
		CWndObjDescription* pDesc = m_Children.GetAt(i);

		if (pDesc->IsChanged())
		{
			CWndObjDescription* pWndObjDesc = pDesc->Clone();
			pDeltaContainer->Add(pWndObjDesc);
		}
		if (pDesc->IsRemoved())
		{
			m_Children.RemoveAt(i);
			i--; //riallineo indice
			delete pDesc;
			pDesc = NULL;
		}
		else
		{   //imposta lo stato di tutte le descrizioni rimaste a REMOVED, per capire al giro successivo quali saranno
			//le decrizioni da rimuovere
			pDesc->SetRemoved();
			pDesc->PopulateDeltaDesc(pDeltaContainer); //ricorsione
		}
	}
}

//Aggiorna gli attributi che sono cambiati, e se ci sono cambiamenti setta lo stato della descrizione a UPDATED
//-----------------------------------------------------------------------------
void CWndObjDescription::UpdateAttributes(CWnd *pWnd)
{
	CString s = GetControlClass(pWnd);
	if (s != m_strControlClass)
	{
		m_strControlClass = s;
		SetUpdated(&m_strControlClass);
	}

	//metodo virtuale che controlla lo stato di disabilitazione
	UpdateEnableStatus(pWnd);

	//metodo virtuale per l'aggiornamento del testo/caption/name della finestra
	UpdateWindowText(pWnd);

	CRect rect;
	GetWindowRect(pWnd, rect);
	CWnd* pParent = pWnd->GetParent();
	if (pParent)
	{
		int dx = pParent->GetScrollPos(SB_HORZ);
		int dy = pParent->GetScrollPos(SB_VERT);
		rect.OffsetRect(dx, dy);

		pParent->ScreenToClient(rect);
		ReverseMapDialog(pParent->m_hWnd, rect);
	}

	if (GetRect() != rect)
	{
		SetRect(rect, TRUE);
	}

	int id = pWnd->GetDlgCtrlID();
	CString strCmd = cwsprintf(_T("%d"), id);
	if (m_strCmd != strCmd && id > 0)
	{
		m_strCmd = strCmd;
		CString sHint;
		if (HasTooltip())
		{
			CFrameWnd* pParentFrame = pWnd->GetParentFrame();
			if (pParentFrame)
				pParentFrame->GetMessageString(id, sHint);
		}
		SetUpdated(&m_strCmd);
		if (sHint != m_strHint)
		{
			m_strHint = sHint;
			SetUpdated(&m_strHint);
		}
	}

	UpdatePropertiesFromStyle(pWnd->GetStyle(), pWnd->GetExStyle());
}
//----------------------------------------------------------------------------
void CWndObjDescription::GetWindowRect(CWnd *pWnd, CRect& rect)
{
	pWnd->GetWindowRect(rect);
}

//----------------------------------------------------------------------------
CString CWndObjDescription::GetID() const
{
	int i = m_strIds.GetUpperBound();
	return i == -1 ? _T("") : m_strIds[i];
}


//----------------------------------------------------------------------------
bool CWndObjDescription::HasID(const CString& sId)
{
	for (int i = 0; i < m_strIds.GetCount(); i++)
	{
		if (m_strIds[i] == sId)
			return true;
	}
	return false;
}
//----------------------------------------------------------------------------
void CWndObjDescription::SetID(const CString& sId, bool bClearExisting /*= false*/)
{
	if (bClearExisting)
		m_strIds.RemoveAll();
	if (!HasID(sId))
		m_strIds.Add(sId);
}
//----------------------------------------------------------------------------
CString CWndObjDescription::GetID(CWnd* pWnd)
{
	int id = pWnd->GetDlgCtrlID();

	return
		(id > 0)
		? AfxGetTBResourcesMap()->DecodeID(GetResourceType(), id).m_strName
		: _T("");
}
//----------------------------------------------------------------------------
CString CWndObjDescription::GetName(CWnd* pWnd)
{
	CAbstractCtrl* pCtrl = dynamic_cast<CAbstractCtrl*>(pWnd);
	return pCtrl ? pCtrl->GetCtrlName() : _T("");
}
//----------------------------------------------------------------------------
CString CWndObjDescription::GetControlClass(CWnd* pWnd)
{
	CAbstractCtrl* pCtrl = dynamic_cast<CAbstractCtrl*>(pWnd);
	return pCtrl ? pCtrl->GetCtrlClass() : _T("");
}
//-----------------------------------------------------------------------------
void CWndObjDescription::UpdateEnableStatus(CWnd *pWnd)
{
	bool bEnabled = TRUE == pWnd->IsWindowEnabled();
	if (m_bEnabled != bEnabled)
	{
		m_bEnabled = bEnabled;
		SetUpdated(&m_bEnabled);
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::UpdateWindowText(CWnd *pWnd)
{
	CString strText;
	int nLen = ::GetWindowTextLength(pWnd->m_hWnd);
	if (nLen > 0)
		::GetWindowText(pWnd->m_hWnd, strText.GetBufferSetLength(nLen), nLen + 1);
	if (m_strText != strText)
	{
		m_strText = strText;
		SetUpdated(&m_strText);
	}
}

//Metodo che ritorna true se il controllo descritto da questa CWndObjDescription e'
//un figlio diretto o indiretto della toolbar
//-----------------------------------------------------------------------------
BOOL CWndObjDescription::IsToolbarChild()
{
	CWndObjDescription* pParentDesc = m_pParent;
	while (pParentDesc != NULL)
	{
		if (pParentDesc->m_Type == CWndObjDescription::Toolbar)
			return TRUE;

		pParentDesc = pParentDesc->m_pParent;
	}

	return FALSE;
}


//metodo per settare il font custom (se presente) del controllo
//-----------------------------------------------------------------------------
void CWndObjDescription::SetFont(CString strFaceName, float nFontSize, bool bBold, bool bItalic, bool bUnderline)
{
	if (!m_pFontDescription)
		m_pFontDescription = new CFontDescription();

	m_pFontDescription->m_strFaceName = strFaceName;
	m_pFontDescription->m_nFontSize = nFontSize;
	m_pFontDescription->m_bIsBold = bBold;
	m_pFontDescription->m_bIsItalic = bItalic;
	m_pFontDescription->m_bIsUnderline = bUnderline;
}

//ATTENZIONE: assegna tutti i datamember della CWndObjDescription ma NON i suoi figli
//-----------------------------------------------------------------------------
void CWndObjDescription::Assign(CWndObjDescription* pDesc)
{
	m_bEnabled = pDesc->m_bEnabled;
	m_bVisible = pDesc->m_bVisible;
	m_bTabStop = pDesc->m_bTabStop;
	m_bHasTabIndex = pDesc->m_bHasTabIndex;
	m_bUsed = pDesc->m_bUsed;
	m_X = pDesc->m_X;
	m_Y = pDesc->m_Y;
	m_Width = pDesc->m_Width;
	m_Height = pDesc->m_Height;
	m_MarginLeft = pDesc->m_MarginLeft;
	m_MarginTop = pDesc->m_MarginTop;
	m_MarginBottom = pDesc->m_MarginBottom;
	m_strCmd = pDesc->m_strCmd;
	m_strIds.RemoveAll();
	m_strIds.Append(pDesc->m_strIds);

	m_arHrefHierarchy.RemoveAll();
	m_arHrefHierarchy.Append(pDesc->m_arHrefHierarchy);

	m_strName = pDesc->m_strName;
	m_strText = pDesc->m_strText;

	m_nNumberDecimal = pDesc->m_nNumberDecimal;
	m_sMinValue = pDesc->m_sMinValue;
	m_sMaxValue = pDesc->m_sMaxValue;

	m_strContext = pDesc->m_strContext;
	m_Resources.RemoveAll();
	for (int i = 0; i < pDesc->m_Resources.GetCount(); i++)
	{
		m_Resources.Add(new CJsonResource(*pDesc->m_Resources[i]));
	}

	m_strHint = pDesc->m_strHint;
	m_strActivation = pDesc->m_strActivation;
	m_strBeforeId = pDesc->m_strBeforeId;
	m_Type = pDesc->m_Type;
	m_pParent = pDesc->m_pParent;
	m_descState = pDesc->m_descState;

	m_nStyle = pDesc->m_nStyle;
	m_nExStyle = pDesc->m_nExStyle;
	m_bAcceptFiles = pDesc->m_bAcceptFiles;
	m_sAnchor = pDesc->m_sAnchor;
	m_bGroup = pDesc->m_bGroup;
	m_bChild = pDesc->m_bChild;
	m_bHScroll = pDesc->m_bHScroll;
	m_bVScroll = pDesc->m_bVScroll;
	m_bBorder = pDesc->m_bBorder;
	m_bTransparent = pDesc->m_bTransparent;
	m_bHFill = pDesc->m_bHFill;
	m_bVFill = pDesc->m_bVFill;
	m_bRightAnchor = pDesc->m_bRightAnchor;
	m_Runtime = pDesc->m_Runtime;
	m_strControlClass = pDesc->m_strControlClass;
	m_ControlStyle = pDesc->m_ControlStyle;
	m_strControlCaption = pDesc->m_strControlCaption;
	m_CaptionVerticalAlign = pDesc->m_CaptionVerticalAlign;
	m_CaptionHorizontalAlign = pDesc->m_CaptionHorizontalAlign;
	m_CaptionWidth = pDesc->m_CaptionWidth;
	if (pDesc->m_pCaptionFontDescription)
		m_pCaptionFontDescription = pDesc->m_pCaptionFontDescription->Clone();

	if (pDesc->m_pControlBehaviourDescription)
		m_pControlBehaviourDescription = pDesc->m_pControlBehaviourDescription->Clone();
	m_nChars = pDesc->m_nChars;
	m_nRows = pDesc->m_nRows;
	m_nTextLimit = pDesc->m_nTextLimit;


	if (pDesc->m_pAccelerator)
		m_pAccelerator = pDesc->m_pAccelerator->Clone();
	if (pDesc->m_pMenu)
		m_pMenu = (CMenuDescription*)pDesc->m_pMenu->DeepClone();
	if (pDesc->m_pFontDescription)
		m_pFontDescription = pDesc->m_pFontDescription->Clone();
	if (pDesc->m_pBindings)
		m_pBindings = pDesc->m_pBindings->Clone();
	if (pDesc->m_pStateData)
		m_pStateData = pDesc->m_pStateData->Clone();
	m_hAssociatedWnd = pDesc->m_hAssociatedWnd;

	//Questo codice mi gasa particolarmente! 
	//Siccome in m_arUpdated c'è la lista dei puntatori ai field modificati,
	//io non posso mettere QUEI puntatori nell'array del mio oggetto, perché i field del mio oggetto
	//hanno altri indirizzi di memoria, e se successivamente li cerco nell'array per vedere se sono stati modificati ovviamente non li troverò.
	//allora, per trovare l'indirizzo del campo del MIO oggetto, prima calcolo l'offset fra indirizzo di memoria del campo di origine e indirizzo dell'oggetto che lo contiene,
	//quindi aggiungo questo offset al this del mio oggetto trovando così l'indirizzo del campo del MIO oggetto
	for (int i = 0; i < pDesc->m_arUpdated.GetCount(); i++)
	{
		LONG_PTR pOtherField = (LONG_PTR)pDesc->m_arUpdated[i];		//indirizzo del campo modificato
		LONG_PTR pOtherFieldOffset = pOtherField - (LONG_PTR)pDesc;	//offset del campo rispetto all'oggetto che lo contiene
		LONG_PTR pFieldInThisObject = (LONG_PTR)this + pOtherFieldOffset;//indirizzo del campo modificato corrispondente in questo oggetto (this)
		m_arUpdated.Add((void*)pFieldInThisObject);
	}

	ExpressionObject::Assign(pDesc);
}
//-----------------------------------------------------------------------------
//ATTENZIONE: assegna tutti i datamember della CWndObjDescription con i suoi figli

void CWndObjDescription::DeepAssign(CWndObjDescription* pDesc)
{
	if (pDesc)
	{
		Assign(pDesc);
		m_Children.SetSortable(pDesc->m_Children.GetSortable());
		int iChildrenCount = pDesc->m_Children.GetCount();
		for (int i = 0; i < iChildrenCount; i++)
		{
			CWndObjDescription* pNewChild = pDesc->m_Children.GetAt(i)->DeepClone();
			pNewChild->m_pParent = this;
			m_Children.Add(pNewChild);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CWndObjDescription::SkipWindow(CWnd* pWnd)
{
	// return (pWnd == NULL  || (IsWindow(pWnd->m_hWnd) && ((pWnd->GetStyle() & WS_VISIBLE) == 0)));
	if (pWnd && IsWindow(pWnd->m_hWnd))
	{
		CRect rect;
		pWnd->GetWindowRect(&rect);
		if (rect.Size() == CSize(0, 0))
			return TRUE;

		return ((pWnd->GetStyle() & WS_VISIBLE) == 0);
	}
	else
	{
		return TRUE;
	}
}

//-----------------------------------------------------------------------------
CString CWndObjDescription::GetJsonID()
{
	//in design mode, invece, l'id deve corrispondere al nome dell'id della finestra
	if (m_hAssociatedWnd)
	{
		int id = ::GetDlgCtrlID(m_hAssociatedWnd);
		CJsonResource res = AfxGetTBResourcesMap()->DecodeID(GetResourceType(), id);
		if (!res.IsEmpty())
			return res.m_strName;

	}
	//in tutti i rimanenti casi (non dovrebbe mai passare di qui) ritorno l'id originario
	return GetID();
}
//Sort the windows description 
//Assign label to edit control and remove label description
//Create group of radio button
//Order not assigned label
//-----------------------------------------------------------------------------
void CWndObjDescription::AddChildWindows(CWnd* pWnd)
{
	if (!pWnd) {
		return;
	}

	CWnd* pChild = pWnd->GetWindow(GW_CHILD);

	CString strRadioName;
	while (pChild)
	{
		TCHAR szClassName[MAX_CLASS_NAME + 1];
		GetClassName(pChild->m_hWnd, szClassName, MAX_CLASS_NAME);

		if (!SkipWindow(pChild))
		{
			CWndObjDescription* pTmp = (CWndObjDescription*)pChild->SendMessage(UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(m_Children), NULL);
			if (!pTmp)
				pTmp = m_Children.AddWindow(pChild);

			if (!pTmp)
			{
				//se questa finestra non fornisce una descrizione, metto le sue figlie direttamente come child di quella corrente
				AddChildWindows(pChild);
			}
			//Check RadioBtn: if the window is a radioBtn, add a name to the corresponding WndObjDescription to identify the radio group
			if (pTmp && pTmp->m_Type == CWndObjDescription::Radio && pTmp->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
			{

				if ((pChild->GetStyle() & WS_GROUP) == WS_GROUP)
				{
					strRadioName = cwsprintf(_T("radioGroup_%d"), pChild->m_hWnd);
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
				else
				{
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
			}
		}
		pChild = pChild->GetWindow(GW_HWNDNEXT);
	}
}


//-----------------------------------------------------------------------------
CString CWndObjDescriptionContainer::GetCtrlID(CWnd* pWnd)
{
	CString strId = _T("");
	if (pWnd)
	{
		strId = GetCtrlID(pWnd->m_hWnd);
	}
	return strId;
}

//-----------------------------------------------------------------------------
CString CWndObjDescriptionContainer::GetCtrlID(HWND hWnd)
{
	CString strId = _T("");

	strId = cwsprintf(_T("%d"), hWnd);

	return strId;
}


//a seconda della tipologia di finestra, si applica WS_BORDER o WS_EX_CLIENTEDGE
//-----------------------------------------------------------------------------
void CWndObjDescription::ApplyBorderStyle(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bBorder)
	{
		dwExStyle |= WS_EX_CLIENTEDGE;//di default applico WS_EX_CLIENTEDGE, alcune classi derivate (CWndPanelDescription) applicano WS_BORDER
	}
}


//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndObjDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	bool b_m_bEnabled = (dwStyle & WS_DISABLED) != WS_DISABLED;
	if (m_bEnabled != b_m_bEnabled)
	{
		m_bEnabled = b_m_bEnabled;
		SetUpdated(&m_bEnabled);
	}

	UPDATE_BOOL_EX(m_bBorder, WS_EX_CLIENTEDGE);
	UPDATE_BOOL(m_bChild, WS_CHILD);
	UPDATE_BOOL(m_bTabStop, WS_TABSTOP);
	UPDATE_BOOL(m_bGroup, WS_GROUP);
	UPDATE_BOOL_EX(m_bAcceptFiles, WS_EX_ACCEPTFILES);

	UPDATE_BOOL_EX(m_bTransparent, WS_EX_TRANSPARENT);

}
//-----------------------------------------------------------------------------
void CWndObjDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bVisible)
		dwStyle |= WS_VISIBLE;
	if (m_bTabStop)
		dwStyle |= WS_TABSTOP;
	if (!m_bEnabled)
		dwStyle |= WS_DISABLED;
	if (m_bGroup)
		dwStyle |= WS_GROUP;

	if (m_bAcceptFiles)
		dwExStyle |= WS_EX_ACCEPTFILES;
	if (m_bChild)
		dwStyle |= WS_CHILD;
	if (m_bHScroll)
		dwStyle |= WS_HSCROLL;
	if (m_bVScroll)
		dwStyle |= WS_VSCROLL;
	if (m_bTransparent)
		dwExStyle |= WS_EX_TRANSPARENT;
	ApplyBorderStyle(dwStyle, dwExStyle);
}


//-----------------------------------------------------------------------------
CString CWndObjDescription::GetTempImagesPath(const CString& sFileName)
{
	return AfxGetPathFinder()->GetWebProxyImagesPath(TRUE) + _T("\\") + sFileName;
}

//-----------------------------------------------------------------------------
CString CWndObjDescription::GetTempFilesPath(const CString& sFileName)
{
	return AfxGetPathFinder()->GetWebProxyFilesPath(TRUE) + _T("\\") + sFileName;
}

//Metodo che dato un puntatore a una finestra, crea o aggiorna la sua descrizione.
//il secondo paramentro strId viene passato solo da chi vuole assegnare un Id creato in modo differente 
//dal modo standard (una stringa basata su pWnd->m_hWnd). Esempio il parsed control della cella attiva del bodyedit
//ha un Id con informazione sulla riga cui appartiene visto che l'handle di finestra rimane invariato
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::AddChildWindow(CWnd* pChild, CString strId /*= _T("")*/)
{
	if (SkipWindow(pChild))
		return NULL;

	CWndObjDescription* pDesc = (CWndObjDescription*)pChild->SendMessage(UM_GET_CONTROL_DESCRIPTION, (WPARAM)&m_Children, (LPARAM)((LPCTSTR)strId));
	if (pDesc)
		return pDesc;

	return m_Children.AddWindow(pChild, strId);
}


//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::Clone()
{
	CWndObjDescription* pNewDesc = (CWndObjDescription*)GetRuntimeClass()->CreateObject();
	pNewDesc->Assign(this);
	return pNewDesc;
}

//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::DeepClone()
{
	CWndObjDescription* pNewDesc = (CWndObjDescription*)GetRuntimeClass()->CreateObject();
	pNewDesc->DeepAssign(this);
	return pNewDesc;
}


//Metodo che aggiorna la descrizione del menu del frame.
//Riceve come parametri la descrizione da aggiornare e il puntatore alla descrizione del frame
//(quest'ultimo serve per avere informazioni sul rettangolo del menu)
//[Possibile futura implementazione: passare anche descrizione delle voci di menu]
//-----------------------------------------------------------------------------
void CWndObjDescription::UpdateMenuDescription(CWndObjDescription* pMenuDesc, CWndObjDescription* pFrameDesc)
{
	//Il rettangolo del titolo ha stessa x,y e width del rettangolo del frame, devo cambiare l'altezza
	CRect menuRect = pFrameDesc->GetRect();
	int titleHeight = GetSystemMetrics(SM_CYCAPTION);
	menuRect.top = menuRect.top + titleHeight;
	int menuHeight = GetSystemMetrics(SM_CYMENU);
	menuRect.bottom = menuRect.top + menuHeight;

	//controllo se e' cambiato il rettangolo, devo marcare la descrizione come aggiornata
	if (pMenuDesc->GetRect() != menuRect)
	{
		pMenuDesc->SetRect(menuRect, TRUE);
	}
}
//-----------------------------------------------------------------------------
void CWndObjDescription::WriteJsonStrings(CJsonSerializer* pResp)
{
	CString s;
	if (!m_strControlCaption.IsEmpty())
	{
		s = AfxLoadJsonString(m_strControlCaption, this);
		if (s != m_strControlCaption)
		{
			pResp->OpenObject();
			pResp->WriteString(szBase, m_strControlCaption);
			pResp->WriteString(szTarget, s);
			pResp->CloseObject();
		}
	}
	if (!m_strText.IsEmpty())
	{
		s = AfxLoadJsonString(m_strText, this);
		if (s != m_strText)
		{
			pResp->OpenObject();
			pResp->WriteString(szBase, m_strText);
			pResp->WriteString(szTarget, s);
			pResp->CloseObject();
		}
	}
	if (!m_strHint.IsEmpty())
	{
		s = AfxLoadJsonString(m_strHint, this);
		if (s != m_strHint)
		{
			pResp->OpenObject();
			pResp->WriteString(szBase, m_strHint);
			pResp->WriteString(szTarget, s);
			pResp->CloseObject();
		}
	}
	for (int i = 0; i < m_Children.GetCount(); i++)
		m_Children[i]->WriteJsonStrings(pResp);
}
//-----------------------------------------------------------------------------
void CWndObjDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_STRING(m_strControlCaption, szJsonControlCaption);
	SERIALIZE_ENUM(m_CaptionVerticalAlign, szJsonControlCaptionVerticalAlign, VerticalAlignment::VATOP);
	SERIALIZE_ENUM(m_CaptionHorizontalAlign, szJsonControlCaptionAlign, TextAlignment::TARIGHT);
	SERIALIZE_INT(m_CaptionWidth, szJsonControlCaptionWidth, NULL_COORD);
	if (m_pCaptionFontDescription)
	{
		m_pCaptionFontDescription->SerializeJson(strJson, szJsonControlCaptionFont);
	}

	if (m_pControlBehaviourDescription)
	{
		m_pControlBehaviourDescription->SerializeJson(strJson);
	}

	SERIALIZE_ENUM(m_Type, szJsonType, Undefined);
	SERIALIZE_STRING(m_strText, szJsonText);

	SERIALIZE_INT(m_nChars, szJsonChars, -1);
	SERIALIZE_INT(m_nRows, szJsonRows, 1);
	SERIALIZE_INT(m_nTextLimit, szJsonTextLimit, 0);

	SERIALIZE_INT(m_nNumberDecimal, szJsonNumberDecimal, -1);
	SERIALIZE_STRING(m_sMinValue, szJsonMinValue);
	SERIALIZE_STRING(m_sMaxValue, szJsonMaxValue);

	SERIALIZE_STRING(m_strHint, szJsonHint);
	SERIALIZE_STRING(m_strActivation, szJsonActivation);
	SERIALIZE_STRING(m_strBeforeId, szJsonBeforeItem);
	SERIALIZE_STRING(m_strContext, szJsonContext);
	//l'id, type, stato e nome vanno sempre inviati, indipendentemente dalla loro modifica
	//modifica per la necessità di serializzare degli href (dalla serializzazione in json di documenti EasyStudio - non si devono scrivere degli Id vuoti)
	SERIALIZE_STRING(GetJsonID(), szJsonId);
	SERIALIZE_STRING(m_strName, szJsonName);
	SERIALIZE_STRING(m_strControlClass, szJsonControlClass);
	SERIALIZE_ENUM(m_ControlStyle, szJsonControlStyle, CS_NONE);

	//se sono in modalità statica, le coordinate sono client
	SERIALIZE_INT(m_X, szJsonX, NULL_COORD);
	SERIALIZE_INT(m_Y, szJsonY, NULL_COORD);
	SERIALIZE_INT(m_Width, szJsonWidth, NULL_COORD);
	SERIALIZE_INT(m_Height, szJsonHeight, NULL_COORD);

	SERIALIZE_INT(m_MarginLeft, szJsonMarginLeft, NULL_COORD);
	SERIALIZE_INT(m_MarginTop, szJsonMarginTop, NULL_COORD);
	SERIALIZE_INT(m_MarginBottom, szJsonMarginBottom, NULL_COORD);
	SERIALIZE_BOOL(m_bEnabled, szJsonEnabled, true);
	SERIALIZE_BOOL(m_bVisible, szJsonVisible, true);
	SERIALIZE_BOOL(m_bTabStop, szJsonTabStop, TabStopDefault());
	SERIALIZE_BOOL(m_bGroup, szJsonGroup, false);
	SERIALIZE_BOOL(m_bChild, szJsonChild, true);
	SERIALIZE_BOOL(m_bHScroll, szJsonHScroll, false);
	SERIALIZE_BOOL(m_bVScroll, szJsonVScroll, false);
	SERIALIZE_BOOL(m_bBorder, szJsonBorder, GetDefaultBorder());
	SERIALIZE_BOOL(m_bTransparent, szJsonTransparent, false);
	SERIALIZE_BOOL(m_bHFill, szJsonHfill, false);
	SERIALIZE_BOOL(m_bVFill, szJsonVfill, false);
	SERIALIZE_BOOL(m_bRightAnchor, szJsonRightAnchor, false);

	SERIALIZE_INT(m_nStyle, szJsonStyle, 0);
	SERIALIZE_INT(m_nExStyle, szJsonExStyle, 0);

	SERIALIZE_BOOL(m_bAcceptFiles, szJsonAcceptFiles, false);
	SERIALIZE_STRING(m_sAnchor, szJsonAnchor);

	//serialize href tags
	for (int i = 0; i < m_arHrefHierarchy.GetCount(); i++)
	{
		//strJson.WriteString(szJsonId, GetJsonID());
		CString href = m_arHrefHierarchy.GetAt(i);
		SERIALIZE_STRING(href, szJsonHref);
	}

	if (m_pBindings)
	{
		m_pBindings->SerializeJson(strJson);
	}
	if (m_pStateData)
	{
		m_pStateData->SerializeJson(strJson);
	}
	if (m_pAccelerator)
	{
		m_pAccelerator->SerializeJson(strJson);
	}
	if (m_pMenu)
	{
		m_pMenu->SerializeJson(strJson);
	}
	if (m_pFontDescription)
	{
		m_pFontDescription->SerializeJson(strJson, szJsonFont);
	}

	if (m_pNumbererDescription)
	{
		m_pNumbererDescription->SerializeJson(strJson, szJsonNumberer);
	}

	if (m_Children.GetCount() > 0)
	{
		m_Children.SerializeJson(strJson);
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::ParseJson(CJsonFormParser& parser)
{
	m_descState = PARSED;
	CString sId;
	PARSE_STRING(sId, szJsonId);
	if (!sId.IsEmpty())
	{
		SetID(sId);
	}
	PARSE_STRING(m_strName, szJsonName);

	PARSE_INT(m_nChars, szJsonChars);
	PARSE_INT(m_nRows, szJsonRows);
	PARSE_INT(m_nTextLimit, szJsonTextLimit);


		//se sto 'parsando sopra' una definizione esistente, il tipo non deve essere cambiato
	if (m_Type != Undefined)
	{
		PARSE_ENUM(m_Type, szJsonType, WndObjType);
	}
	PARSE_STRING(m_strText, szJsonText);

	PARSE_STRING(m_strControlCaption, szJsonControlCaption);
	PARSE_ENUM(m_CaptionVerticalAlign, szJsonControlCaptionVerticalAlign, VerticalAlignment);
	PARSE_ENUM(m_CaptionHorizontalAlign, szJsonControlCaptionAlign, TextAlignment);
	PARSE_INT(m_CaptionWidth, szJsonControlCaptionWidth);
	if (parser.Has(szJsonControlCaptionFont))
	{
		if (!m_pCaptionFontDescription)
			m_pCaptionFontDescription = new CFontDescription();
		m_pCaptionFontDescription->ParseJson(parser, szJsonControlCaptionFont);
	}

	if (parser.Has(szJsonControlBehaviour))
	{
		if (!m_pControlBehaviourDescription)
			m_pControlBehaviourDescription = new CControlBehaviourDescription();
		m_pControlBehaviourDescription->ParseJson(parser);
	}

	PARSE_INT(m_nNumberDecimal, szJsonNumberDecimal);
	if (parser.Has(szJsonMinValue))
	{
		Json::Value val;
		if (parser.IsObject(szJsonMinValue) && parser.ResolveValue(szJsonMinValue, m_Defines, val))
		{
			if (val.isInt())
				m_sMinValue = cwsprintf(_T("%d"), val.asInt());
			else if (val.isDouble())
				m_sMinValue = cwsprintf(_T("%f"), val.asDouble());
			else
				m_sMinValue = val.asString().c_str();
		}
		else
		{
			double d;
			int i;
			if (parser.TryReadDouble(szJsonMinValue, d))
				m_sMinValue = cwsprintf(_T("%f"), d);
			else if (parser.TryReadInt(szJsonMinValue, i))
				m_sMinValue = cwsprintf(_T("%d"), i);
			else
			{
				PARSE_STRING(m_sMinValue, szJsonMinValue);
			}
		}
	}

	if (parser.Has(szJsonMaxValue))
	{
		Json::Value val;
		if (parser.IsObject(szJsonMaxValue) && parser.ResolveValue(szJsonMaxValue, m_Defines, val))
		{
			if (val.isInt())
				m_sMaxValue = cwsprintf(_T("%d"), val.asInt());
			else if (val.isDouble())
				m_sMaxValue = cwsprintf(_T("%f"), val.asDouble());
			else
				m_sMaxValue = val.asString().c_str();
		}
		else
		{
			double d;
			int i;
			if (parser.TryReadDouble(szJsonMaxValue, d))
				m_sMaxValue = cwsprintf(_T("%f"), d);
			else if (parser.TryReadInt(szJsonMaxValue, i))
				m_sMaxValue = cwsprintf(_T("%d"), i);
			else
			{
				PARSE_STRING(m_sMaxValue, szJsonMaxValue);
			}
		}
	}

	PARSE_STRING(m_strHint, szJsonHint);
	PARSE_STRING(m_strActivation, szJsonActivation);
	PARSE_STRING(m_strBeforeId, szJsonBeforeItem);
	CString sContext;
	PARSE_STRING(sContext, szJsonContext);
	m_strContext = sContext;

	PARSE_STRING(m_strControlClass, szJsonControlClass);
	PARSE_ENUM(m_ControlStyle, szJsonControlStyle, ControlStyle);

	PARSE_INT(m_X, szJsonX);
	PARSE_INT(m_Y, szJsonY);
	PARSE_INT(m_Width, szJsonWidth);
	PARSE_INT(m_Height, szJsonHeight);
	PARSE_INT(m_MarginLeft, szJsonMarginLeft);
	PARSE_INT(m_MarginTop, szJsonMarginTop);
	PARSE_INT(m_MarginBottom, szJsonMarginBottom);

	if (AfxIsRemoteInterface())
	{
		int xFactor = AfxGetThemeManager()->GetBaseUnitsWidth();
		int yFactor = AfxGetThemeManager()->GetBaseUnitsHeight();

		if (m_X != NULL_COORD)
			m_X = MulDiv(m_X, xFactor, 100);
		if (m_Y != NULL_COORD)
			m_Y = MulDiv(m_Y, xFactor, 100);

		m_Width = MulDiv(m_Width, xFactor, 100);

		m_Height = MulDiv(m_Height, xFactor, 100);
	}
	if (parser.Has(szJsonStyle))
		m_nStyle = parser.ReadInt(szJsonStyle);
	if (parser.Has(szJsonExStyle))
		m_nExStyle = parser.ReadInt(szJsonExStyle);

	PARSE_BOOL(m_bEnabled, szJsonEnabled);
	PARSE_BOOL(m_bVisible, szJsonVisible);
	PARSE_BOOL(m_bTabStop, szJsonTabStop);
	PARSE_BOOL(m_bAcceptFiles, szJsonAcceptFiles);
	PARSE_STRING(m_sAnchor, szJsonAnchor);
	PARSE_BOOL(m_bGroup, szJsonGroup);
	PARSE_BOOL(m_bChild, szJsonChild);
	PARSE_BOOL(m_bVScroll, szJsonVScroll);
	PARSE_BOOL(m_bHScroll, szJsonHScroll);
	PARSE_BOOL(m_bBorder, szJsonBorder);
	PARSE_BOOL(m_bTransparent, szJsonTransparent);
	PARSE_BOOL(m_bHFill, szJsonHfill);
	PARSE_BOOL(m_bVFill, szJsonVfill);
	PARSE_BOOL(m_bRightAnchor, szJsonRightAnchor);

	if (parser.Has(szJsonAccelerators))
	{
		if (!m_pAccelerator)
			m_pAccelerator = new CAcceleratorDescription();
		m_pAccelerator->ParseJson(parser);
	}
	if (parser.Has(szJsonMenu))
	{
		if (!m_pMenu)
			m_pMenu = new CMenuDescription(this);
		parser.BeginReadObject(szJsonMenu);
		m_pMenu->ParseJson(parser);
		parser.EndReadObject();
	}
	if (parser.Has(szJsonBinding))
	{
		if (!m_pBindings)
			m_pBindings = new BindingInfo();
		m_pBindings->ParseJson(parser);
	}
	if (parser.Has(szJsonStateData))
	{
		if (!m_pStateData)
			m_pStateData = new StateData();
		m_pStateData->ParseJson(parser);
	}
	if (parser.Has(szJsonFont))
	{
		if (!m_pFontDescription)
			m_pFontDescription = new CFontDescription();
		m_pFontDescription->ParseJson(parser, szJsonFont);
	}
	if (parser.Has(szJsonNumberer))
	{
		if (!m_pNumbererDescription)
			m_pNumbererDescription = new CNumbererDescription();
		m_pNumbererDescription->ParseJson(parser, szJsonNumberer);
	}
	if (parser.BeginReadArray(szJsonItems))
	{
		m_Children.ParseJson(parser);
		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescription::SerializeBinary(CStreamSerializer& serializer)
{
	serializer.Serialize(m_strControlCaption);
	serializer.Serialize((int&)m_CaptionVerticalAlign);
	serializer.Serialize((int&)m_CaptionHorizontalAlign);
	serializer.Serialize(m_CaptionWidth);
	serializer.Serialize((int&)m_Type);
	serializer.Serialize(m_strText);

	serializer.Serialize(m_nChars);
	serializer.Serialize(m_nNumberDecimal);
	serializer.Serialize(m_sMinValue);
	serializer.Serialize(m_sMaxValue);

	serializer.Serialize(m_strHint);
	serializer.Serialize(m_strActivation);
	serializer.Serialize(m_strBeforeId);
	serializer.Serialize(m_strContext);
	serializer.Serialize(GetID());
	serializer.Serialize(m_strName);
	serializer.Serialize(m_strControlClass);

	//se sono in modalità statica, le coordinate sono client
	serializer.Serialize(m_X);
	serializer.Serialize(m_Y);
	serializer.Serialize(m_Width);
	serializer.Serialize(m_Height);

	serializer.Serialize(m_MarginLeft);
	serializer.Serialize(m_MarginTop);
	serializer.Serialize(m_MarginBottom);
	serializer.Serialize(m_bEnabled);
	serializer.Serialize(m_bVisible);
	serializer.Serialize(m_bTabStop);
	serializer.Serialize(m_bGroup);
	serializer.Serialize(m_bChild);
	serializer.Serialize(m_bHScroll);
	serializer.Serialize(m_bVScroll);
	serializer.Serialize(m_bBorder);
	serializer.Serialize(m_bTransparent);
	serializer.Serialize(m_bHFill);
	serializer.Serialize(m_bVFill);
	serializer.Serialize(m_bRightAnchor);
	serializer.Serialize(m_nStyle);
	serializer.Serialize(m_nExStyle);

	serializer.Serialize(m_bAcceptFiles);
	serializer.Serialize(m_sAnchor);
	serializer.Serialize((SerializableObj*&)m_pBindings);
	serializer.Serialize((SerializableObj*&)m_pStateData);
	serializer.Serialize((SerializableObj*&)m_pAccelerator);
	serializer.Serialize((SerializableObj*&)m_pMenu);
	serializer.Serialize((SerializableObj*&)m_pFontDescription);
	serializer.Serialize((SerializableObj*&)m_pCaptionFontDescription);
	serializer.Serialize((SerializableObj*&)m_pNumbererDescription);
	serializer.Serialize(m_Children);

}


//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::ParseHref(CJsonFormParser& parser, const CString& sHref, CWndObjDescription* pDescriptionToMerge)
{
	CString sResourceName, sContext;
	CJsonResource::SplitNamespace(sHref, sResourceName, sContext);
	CString sOldContext;

	if (parser.m_pRootContext->m_bIsJsonDesigner) {
		CHRefDescription* hrefDescr =  new CHRefDescription(nullptr);
		hrefDescr->m_sHRef = sHref;
		return hrefDescr;
	}

	//se non ho un percorso completo, allora è relativo al contesto (path del json di outline)
	if (sContext.IsEmpty())
		sContext = parser.m_pRootContext->m_strCurrentResourceContext;
	else
	{
		sOldContext = parser.m_pRootContext->m_strCurrentResourceContext;
		parser.m_pRootContext->m_strCurrentResourceContext = sContext;
	}

	CJsonResource res;
	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(sResourceName, TbResources, 1, sContext);
	res.m_strName = sResourceName;
	res.m_strContext = sContext;
	res.m_dwId = nID;

	CString sActivation;
	parser.TryReadString(szJsonActivation, sActivation);
	WndObjType expectedType = Undefined;
	PARSE_ENUM(expectedType, szJsonType, WndObjType);
	//carico il file esterno
	CArray<CWndObjDescription*>ar;
	CJsonFormEngineObj::GetInstance()->ParseDescription(ar, parser.m_pRootContext, res, sActivation, pDescriptionToMerge, expectedType);
	if (!sOldContext.IsEmpty())
		parser.m_pRootContext->m_strCurrentResourceContext = sOldContext;
	//se parso un href, troverò al più una descrizione
	CWndObjDescription* pObj = ar.GetCount() ? ar[0] : NULL;
	//ed eventualmente integro con gli attributi presenti nel corrente (es tag activation)
	if (pObj)
	{
		pObj->m_arHrefHierarchy.Add(sResourceName);
		bool bOldForAppend = parser.m_bForAppend;
		parser.m_bForAppend = true;
		pObj->ParseJson(parser);
		parser.m_bForAppend = bOldForAppend;
	}

	return pObj;
}
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescription::ParseJsonObject(CJsonFormParser& parser, CWndObjDescription* pDescriptionToMerge, WndObjType expectedType)
{
	if (parser.IsArray())
	{
		CWndObjDescription* pDummy = new CFakeArrayContainerDescription();//serve solo per contenere l'array di opggetti che andrà poi travasato nell'elemento chje lo referenzia
		pDummy->m_Children.ParseJson(parser);
		return pDummy;
	}
	if (parser.Has(szJsonHref))
	{
		CString sHref = parser.ReadString(szJsonHref);
		return ParseHref(parser, sHref, pDescriptionToMerge);
	}

	WndObjType type = Undefined;
	PARSE_ENUM(type, szJsonType, WndObjType);
	if (expectedType != Undefined) 
	{
		if (type != Undefined && type != expectedType)
		{
			ASSERT_TRACE2(FALSE, "Expected type: %d, found: %d", expectedType, type);
			return NULL;
		}
		type = expectedType;
	}
	if (type >= CLASS_MAP_SIZE)
	{
		TRACE1("Invalid type in json: %d", type);
		ASSERT(FALSE);
		return NULL;
	}
	if (type == Constants)
	{
		parser.BeginReadArray(szJsonItems);
		for (int i = 0; i < parser.GetCount(); i++)
		{
			parser.BeginReadObject(i);
			CString sName = parser.ReadString(szJsonName);
			Json::Value val = parser.ReadValue(szJsonValue);
			parser.m_pRootContext->m_Defines[sName] = val;
			parser.EndReadObject();
		}
		parser.EndReadArray();
		return NULL;
	}
	CRuntimeClass* pClass = CWndObjDescription::classMap[type];
	if (!pClass)
	{
		ASSERT(FALSE);
		return NULL;
	}
	CWndObjDescription* pObj = (CWndObjDescription*)pClass->CreateObject();
	pObj->ParseJson(parser);
	return pObj;
}

//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::Assign(CWndObjDescriptionContainer *pNewDesc)
{
	if (pNewDesc)
	{
		m_bSortable = pNewDesc->m_bSortable;
		for (int i = 0; i < pNewDesc->GetCount(); i++)
			Add(pNewDesc->GetAt(i)->Clone());

		SetParent(pNewDesc->GetParent());
	}
}

//-----------------------------------------------------------------------------
CWndObjDescriptionContainer::~CWndObjDescriptionContainer()
{
	int iCount = GetCount();
	for (int i = 0; i < iCount; i++)
	{
		CWndObjDescription* pDescr = GetAt(i);
		delete pDescr;
	}
}

//-----------------------------------------------------------------------------
INT_PTR CWndObjDescriptionContainer::Add(CWndObjDescription* newElement)
{
	return __super::Add(newElement);
}

//Metodo che data una finestra generica, ne ritorna la sua descrizione
//Nel caso la descrizione esista gia', la recupera e la aggiorna.
//Nel caso non esiste la crea e la aggiunge al CWndObjDescriptionContainer(il this)
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescriptionContainer::AddWindow(CWnd* pWnd, CString strId /*= _T("")*/)
{
	CWndObjDescription::WndObjType type = CWndObjDescription::Undefined;
	CString sText;

	CWndObjDescription *pDesc = NULL;

	TCHAR szClassName[MAX_CLASS_NAME + 1];
	GetClassName(pWnd->m_hWnd, szClassName, MAX_CLASS_NAME);

	if (_tcsicmp(szClassName, _T("Button")) == 0)
	{

		UINT style = pWnd->GetStyle();
		UINT typeStyle = BS_TYPEMASK & style;
		if (typeStyle == BS_GROUPBOX)
		{
			pDesc = (CWndColoredObjDescription*)(GetWindowDescription(pWnd, RUNTIME_CLASS(CWndColoredObjDescription), strId));
			type = CWndObjDescription::Group;
		}
		else if (typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON)
		{
			type = CWndObjDescription::Radio;
			pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndCheckRadioDescription), strId);
		}
		else if (typeStyle == BS_CHECKBOX || typeStyle == BS_AUTOCHECKBOX
			|| typeStyle == BS_3STATE || typeStyle == BS_AUTO3STATE)
		{
			type = CWndObjDescription::Check;
			pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndCheckRadioDescription), strId);
		}
		else
			type = CWndObjDescription::Button;

		if (!pDesc)
			pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CPushButtonDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("Edit")) == 0)
	{
		type = CWndObjDescription::Edit;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CTextObjDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("ComboBox")) == 0)
	{
		type = CWndObjDescription::Combo;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CComboDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("ListBox")) == 0)
	{
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CListDescription), strId);
		if (pWnd->IsKindOf(RUNTIME_CLASS(CCheckListBox)))
		{
			type = CWndObjDescription::CheckList;
			((CListDescription*)pDesc)->SetItemClass(RUNTIME_CLASS(CItemCheckListDescription));
		}
		else
		{
			CCheckListBox* pCheckLB = dynamic_cast<CCheckListBox*>(pWnd);
			type = CWndObjDescription::List;
			((CListDescription*)pDesc)->SetItemClass(RUNTIME_CLASS(CItemListBoxDescription));
		}
	}
	else if (_tcsicmp(szClassName, _T("Static")) == 0)
	{
		HBITMAP bmp = ((CStatic*)pWnd)->GetBitmap();
		if (bmp)
		{
			type = CWndObjDescription::Image;
			pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndImageDescription), strId);
			CString sName = cwsprintf(_T("bmp%ud.png"), (HBITMAP)bmp);

			CImage aImage;
			aImage.Attach(bmp);
			if (((CWndImageDescription*)pDesc)->m_ImageBuffer.Assign(&aImage, sName), strId)
				pDesc->SetUpdated(&((CWndImageDescription*)pDesc)->m_ImageBuffer);
			aImage.Detach();
		}
		else
		{
			type = CWndObjDescription::Label;
			pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CLabelDescription), strId);
		}
	}
	else if (_tcsicmp(szClassName, _T("msctls_updown32")) == 0)
	{
		type = CWndObjDescription::Spin;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CSpinCtrlDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("msctls_statusbar32")) == 0)
	{
		type = CWndObjDescription::StatusBar;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndStatusBarDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("msctls_progress32")) == 0 && pWnd->IsKindOf(RUNTIME_CLASS(CProgressCtrl)))
	{
		type = CWndObjDescription::ProgressBar;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndProgressBarDescription), strId);
	}
	else if (_tcsistr(szClassName, _T("BCGPDockBar:")) == szClassName)
	{
		type = CWndObjDescription::Panel;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndObjDescription), strId);
	}
	else if (_tcsistr(szClassName, _T("BCGPTabWnd:")) == szClassName)
	{
		type = CWndObjDescription::Panel;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndObjDescription), strId);
	}
	else if (_tcsistr(szClassName, _T("BCGPToolBar:")) == szClassName)
	{
		type = CWndObjDescription::Panel;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndObjDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("#32770")) == 0)
	{
		type = CWndObjDescription::Panel;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndPanelDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("SYSTREEVIEW32")) == 0)
	{
		type = CWndObjDescription::Tree;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndObjDescription), strId);
	}
	else if (_tcsicmp(szClassName, _T("SYSLISTVIEW32")) == 0)
	{
		type = CWndObjDescription::ListCtrl;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CListCtrlDescription), strId);
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CSplitterWnd)))
	{
		type = CWndObjDescription::View;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndImageDescription), strId);
	}
	else if (pWnd->IsKindOf(RUNTIME_CLASS(CListCtrl)))
	{
		type = CWndObjDescription::ListCtrl;
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CListCtrlDescription), strId);
	}
	//non butto più dentro ad una generica label tutto ciò che non riesco a catalogare
	//adesso ci sono troppi oggetti di mezzo, è diventato rischioso!
	/*else
	{
	CString sText;
	pWnd->GetWindowText(sText);
	if (!sText.IsEmpty())
	{
	type = CWndObjDescription::Label;
	pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CLabelDescription), strId);
	((CLabelDescription*)pDesc)->SetStaticTextAlignment(pWnd);
	}
	}*/

	if (type == CWndObjDescription::Undefined)
		return NULL;

	if (!pDesc)
		pDesc = GetWindowDescription(pWnd, RUNTIME_CLASS(CWndObjDescription), strId);

	pDesc->UpdateAttributes(pWnd);
	if (type != CWndObjDescription::Undefined)
		pDesc->m_Type = type;

	pDesc->AddChildWindows(pWnd);

	return pDesc;
}

IMPLEMENT_DYNCREATE(CDummyDescription, CWndObjDescription);
IMPLEMENT_DYNCREATE(CFakeArrayContainerDescription, CDummyDescription);
//-----------------------------------------------------------------------------
void CFakeArrayContainerDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	//se viene specificata un'attivazione, viene travasata su tutti i figli, perché questo elemento è a perdere, serve solo
	//per apportare nuovi elementi ad un array esistente che lo referenzia
	if (!m_strActivation.IsEmpty())
	{
		for (int i = 0; i < m_Children.GetCount(); i++)
		{
			CWndObjDescription* pChild = m_Children[i];
			if (pChild->m_strActivation.IsEmpty())
				pChild->m_strActivation = m_strActivation;
		}
	}
}
static const CDummyDescription g_DummyDescription;



//Metodo che data una finestra restituisce la descrizione della finestra se e' gia' presente nella
//lista delle descrizioni (CWndObjDescriptionContainer), altrimenti istanzia una nuova descrizione
//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescriptionContainer::GetWindowDescription(CWnd* pWnd, CRuntimeClass* pClass, CString strId /*= _T("")*/, int nExpectedIndex /*= -1*/)
{
	//Se non ho un ID gia valorizzato, ne creo uno secondo la nostra convenzione a partire dall'handle di finestra
	if (strId.IsEmpty())
	{
		strId = GetCtrlID(pWnd);
		if (strId.IsEmpty()) //inserito per il caso PropertyItem, veniva creato un item con id vuoto
			return NULL;
	}
	for (int i = 0; i < GetCount(); i++)
	{
		CWndObjDescription* pChild = GetAt(i);
		if (pChild->GetID() == strId)
		{
			//descrizione gia presente, stesso id, la restituisco "resettandone" lo stato
			if (pChild->GetState() == REMOVED)
			{
				//Se e' rimossa, vuol dire che lo stato e' stato impsotato da fuori, per capire le finestre effettivamente rimosse.
				//Se arriva la richiesta in questo metodo, vuol dire che la finestra e' ancora viva, e quindi lo stato va rimesso unchanged
				pChild->SetUnchanged();
			}
			if (nExpectedIndex != -1 && nExpectedIndex != i)
			{
				RemoveAt(i);
				InsertAt(nExpectedIndex, pChild);
			}

			return pChild;
		}


		if (pWnd && pChild->m_hAssociatedWnd == pWnd->m_hWnd && pWnd->m_hWnd != NULL)
		{
			//descrizione gia presente, diverso id ma stessa finestra, la restituisco "resettandone" lo stato e assegnando il nuovo id
			pChild->SetID(strId);
			if (pChild->GetState() == REMOVED)
			{
				//Se e' rimossa, vuol dire che lo stato e' stato impsotato da fuori, per capire le finestre effettivamente rimosse.
				//Se arriva la richiesta in questo metodo, vuol dire che la finestra e' ancora viva, e quindi lo stato va rimesso unchanged
				pChild->SetUnchanged();
			}
			if (nExpectedIndex != -1 && nExpectedIndex != i)
			{
				RemoveAt(i);
				InsertAt(nExpectedIndex, pChild);
			}
			return pChild;
		}
	}
	//descrizione non presente, ne creo una nuova
	CWndObjDescription* pNewDescr = (CWndObjDescription*)pClass->CreateObject();
	pNewDescr->SetParent(GetParent());
	pNewDescr->SetID(strId);
	if (nExpectedIndex != -1)
		InsertAt(nExpectedIndex, pNewDescr);
	else
		Add(pNewDescr);
	pNewDescr->SetAdded();

	return pNewDescr;
}

// rect container the other rect ?
// return T o F
//----------------------------------------------------------------------------
bool RectContainerRect(CRect rect1, CRect rect2)
{
	if (rect1.left <= rect2.left &&
		rect1.right >= rect2.right &&
		rect1.top <= rect2.top &&
		rect1.bottom >= rect2.bottom)
	{
		return TRUE;
	}
	return FALSE;
}
///il precedente algoritmo aveva dei memory leaks dovuto al fatto che si applicava una deepclone per evitare che
//lo spostamento dei controlli intaccasse la struttura originaria, però poi non veniva deletata.
//inoltre nel caso di groupbox con dei controlli modificati, al client veniva sparato un albero json contenente la groupbox come root e stato UNCHANGED
//ed i vari figli con stato UPDATED. Il javascript siccome vedeva la root UNCHANGED lasciava perdere i child che invece andavano processati.
//Adesso invece la groupbox, che è UNCHANGED, non viene più mandata, e vengono mandati solo i figli
//In pratica, prima applico l'algoritmo per trovare i delta, lasciando l'albero inalterato,
//poi sul delta applico gli spostamenti necessari. Possibile peggioramento: caso in cui viene aggiunta una groupbox con due controlli.
//prima nel json avevamo un nodo con stato ADDED (la group) e due nodi figli con stato UNCHANGED
//adesso, siccome vengono processati come fratelli, abbiamo tre nodi flat con stato ADDED, che poi vengono riorganizati ad albero, ma
//lo stato di ADDED nei due figli rimane. Non mi sembra però che questo possa causare problemi...
//----------------------------------------------------------------------------
void CWndObjDescriptionContainer::MoveGroupBoxChildren()
{
	//riarrangio i controlli spostando quelli contenuti nelle groupbox
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		// get a pointer to the i-th child of this current window.		
		CWndObjDescription* pDesc = GetAt(i);
		//se trovo un groupbox, vi sposto dentro tutti i suoi fratelli
		//aventi il rettangolo contenuto in esso
		if (pDesc)
		{
			if (pDesc->m_Type == CWndObjDescription::Group)
			{
				// base case.
				// non più vero, se una GroupBox ha dei figli è perchè glieli abbiamo aggiunti noi.
				// ASSERT(pDesc->m_Children.GetCount() == 0);//i group box mfc non dovrebbero avere figli
				for (int j = 0; j < GetCount(); j++)
				{
					// a potential group sibling, i.e. a child of the current window.
					CWndObjDescription*  pTarget = GetAt(j);
					if (pTarget)
					{
						if (pTarget != pDesc && RectContainerRect(pDesc->GetRect(), pTarget->GetRect()))
						{
							// it is an actual sibling and it is graphically contained by 
							// the group rectangle.
							// append target to child group 
							pDesc->m_Children.Add(pTarget);
							pTarget->SetParent(pDesc);
							RemoveAt(j);
							j--;
							// LP: "i" should not be decreased, it is just 
							// elelement returned by GetAt(j) that has been moved, 
							// not GetAt(i) which is the GroupBox control.
							// With "i" decreased we were missing the processing
							// of a groupbox in OFM::MasterResources::MasterData
							// But, "i" and "j" are indexes to access the same collection,
							// so whenever we do RemoveAt(j) we actually have to resync "i"
							// as well. I can not just decrease "i" as I would 
							// miss some items (i goes downwards, j upwards) so I set "i"
							// to GetUpperBound() again as in the loop initialization.
							// alternatively, we could use two pointers list, one for "i" (not modified)
							// and one for "j" from which we can freely remove items.
							i = GetUpperBound();
							//i--;
						}
					}
				}
			}

			//altrimenti ripeto l'operazione per i figli (se sono un groupbox non lo faccio,
			//i group box mfc non hanno figli
			else
			{
				// recursion case.
				pDesc->m_Children.MoveGroupBoxChildren();
			}
		}
	}
}
//----------------------------------------------------------------------------
void CWndObjDescriptionContainer::ApplyDeltaDesc(CWndObjDescription* pOriginal)
{
	if (!pOriginal)
	{
		return;
	}

	//se rimossa, inserisce un elemento nella lista dei delta con solo la finestra rimossa, e non i suoi figli
	if (pOriginal->IsRemoved())
	{
		//shallow copy
		CWndObjDescription* pWndObjDesc = pOriginal->Clone();
		// add to difference list
		Add(pWndObjDesc);
		return;
	}

	//se aggiunto, inserisce un elemento nella lista dei delta con tutto l'albero aggiunto
	if (pOriginal->IsAdded())
	{
		CWndObjDescription* pWndObjDesc = pOriginal->DeepClone();
		Add(pWndObjDesc);
		return;
	}

	//se cambiato, inserisce un elemento nella lista dei delta con solo la finestra cambiata, e non i suoi figli
	if (pOriginal->IsChanged())
	{
		CWndObjDescription* pWndObjDesc = pOriginal->Clone();
		Add(pWndObjDesc);
	}

	//imposta lo stato di tutte le descrizioni nell'albero originale a REMOVED, per capire al giro successivo quali saranno
	//le descrizioni da rimuovere
	// pOriginal->SetRemoved();

	//ricorsione sui figli	
	int iPos = 0;
	for (int i = 0; i < pOriginal->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = pOriginal->m_Children.GetAt(i);
		if (pChild->IsRemoved())
		{
			// remove the child from the collection.
			pOriginal->m_Children.RemoveAt(i);
			// decrease loop index in order to avoid missing collection items.
			i--;
		}
		ApplyDeltaDesc(pChild);
	}
}


//----------------------------------------------------------------------------
bool CWndObjDescriptionContainer::RemoveItem(CWndObjDescription* pItem, BOOL bDelete /*= TRUE*/)
{
	bool bRemoved = true;
	if (pItem)
	{
		bRemoved = false;
		for (int iCount = GetCount() - 1; iCount >= 0; iCount--)
		{
			CWndObjDescription* pCurrItem = GetAt(iCount);
			if (pCurrItem)
			{
				if (pCurrItem->GetID() == pItem->GetID())
				{
					RemoveAt(iCount);
					bRemoved = true;
					if (bDelete)
					{
						delete pCurrItem;
					}
					// item found and removed, quit the loop
					break;
				}
				// recurse on current item's children.
				bool bRemoved = pCurrItem->m_Children.RemoveItem(pItem);
				if (bRemoved)
				{
					// item found and removed, quit the loop
					break;
				}
			}
		}
	}
	return bRemoved;
}

//----------------------------------------------------------------------------
int CWndObjDescriptionContainer::IndexOf(CWndObjDescription* pItem)
{
	for (int i = 0; i < GetCount(); i++)
	{
		CWndObjDescription* pCurrItem = GetAt(i);
		if (pCurrItem == pItem)
			return i;
	}
	return -1;
}
//----------------------------------------------------------------------------
int CWndObjDescriptionContainer::IndexOf(const CString& strId)
{
	for (int i = 0; i < GetCount(); i++)
	{
		CWndObjDescription* pCurrItem = GetAt(i);
		if (pCurrItem->HasID(strId))
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::SortByTabOrder()
{
	if (GetSize() > 1)
		std::sort(GetData(), GetData() + GetSize(), TabOrderLess);
}
//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::SerializeJson(CJsonSerializer& strJson)
{

	strJson.OpenArray(szJsonItems);

	int nCount = GetCount();
	for (int i = 0; i < nCount; i++)
	{
		IJsonObject* pChild = (IJsonObject*)GetAt(i);
		strJson.OpenObject(i);
		pChild->SerializeJson(strJson);
		strJson.CloseObject();
	}
	strJson.CloseArray();
}

//-----------------------------------------------------------------------------
CWndObjDescription* CWndObjDescriptionContainer::GetChild(CString sId, BOOL bRecursive)
{
	CWndObjDescription* foundChild = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CWndObjDescription* pChild = GetAt(i);
		if (pChild->HasID(sId))
			return pChild;
		if (bRecursive)
		{
			pChild = pChild->m_Children.GetChild(sId, bRecursive);
			if (pChild)
				return pChild;
		}
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::ParseJson(CJsonFormParser& parser)
{
	for (int i = 0; i < parser.GetCount(); i++)
	{
		if (!parser.BeginReadObject(i))
			continue;

		ParseJsonItem(parser);
		parser.EndReadObject();
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::ParseJsonItem(CJsonFormParser& parser)
{
	CWndObjDescription* pObj = NULL;
	CString sHref;
	if (parser.Has(szJsonHref))
		sHref = parser.ReadString(szJsonHref);

	//se sono in modalità append, completo l'oggetto parsato da filettino con quello del master file
	if (parser.m_bForAppend)
	{
		//se ho un href, vado in append aggiungendo al mio parent attuale gli elementi pescati 
		//dal file esterno
		pObj = sHref.IsEmpty() ? NULL : CWndObjDescription::ParseJsonObject(parser, m_pParent);
		if (!pObj)
		{
			CString sId;
			if (parser.Has(szJsonId))
				sId = parser.ReadString(szJsonId);

			//altrimenti vado in append con gli elementi del file corrente 
			pObj = sId.IsEmpty() || AfxGetTBResourcesMap()->IsFixedResource(sId)
				? NULL
				: GetChild(sId, FALSE);
			if (pObj)
			{
				pObj->ParseJson(parser);
			}
		}
	}
	if (!pObj)
		pObj = CWndObjDescription::ParseJsonObject(parser, NULL);

	//se l'oggetto è di nuova creazione (non ho fatto alcun merge) gli devo assegnare un parent e metterlo nella lista
	if (pObj)
	{
		if (pObj->IsDummy())
		{
			for (int i = 0; i < pObj->m_Resources.GetCount(); i++)
			{
				m_pParent->m_Resources.Add(new CJsonResource(*pObj->m_Resources[i]));
			}
			for (int i = 0; i < pObj->m_Children.GetCount(); i++)
			{
				AddJsonItem(pObj->m_Children[i]);
			}
			pObj->m_Children.RemoveAll();
			delete pObj;
		}
		else if (!pObj->GetParent())
		{
			//se sono in un href con attivazione, tutti gli elementi referenziati ereditano l'attivazione
			if (pObj->m_strActivation.IsEmpty() && !parser.m_sActivation.IsEmpty())
				pObj->m_strActivation = parser.m_sActivation;
			AddJsonItem(pObj);
		}
	}
}

//-----------------------------------------------------------------------------
void CWndObjDescriptionContainer::AddJsonItem(CWndObjDescription* pObj)
{
	if (pObj->m_strBeforeId.IsEmpty())
	{
		Add(pObj);
	}
	else
	{
		int i = 0;
		for (; i <= GetUpperBound(); i++)
		{
			CWndObjDescription* pChild = GetAt(i);
			if (pChild->HasID(pObj->m_strBeforeId))
				break;
		}
		InsertAt(i, pObj);
	}
	pObj->SetParent(m_pParent);
}

IMPLEMENT_DYNCREATE(CComboDescription, CTextObjDescription)
REGISTER_WND_OBJ_CLASS(CComboDescription, Combo)

//-----------------------------------------------------------------------------
void CComboDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_ENUM(m_nComboType, szJsonComboType, DROPDOWN);
	SERIALIZE_BOOL(m_bAuto, szJsonAuto, false);
	SERIALIZE_BOOL(m_bNoIntegralHeight, szJsonNoIntegralHeight, false);
	SERIALIZE_BOOL(m_bOemConvert, szJsonOemConvert, false);
	SERIALIZE_BOOL(m_bSort, szJsonSort, false);
	SERIALIZE_BOOL(m_bVScroll, szJsonVScroll, true);
	SERIALIZE_BOOL(m_bUpperCase, szJsonUpperCase, false);
	SERIALIZE_ENUM(m_nComboOwnerDrawType, szJsonOwnerDraw, NO);
	if (m_pItemSourceDescri)
	{
		m_pItemSourceDescri->SerializeJson(strJson);
	}
	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CComboDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_ENUM(m_nComboType, szJsonComboType, ComboType);
	if (parser.Has(szJsonAuto))
		m_bAuto = parser.ReadBool(szJsonAuto);
	if (parser.Has(szJsonNoIntegralHeight))
		m_bNoIntegralHeight = parser.ReadBool(szJsonNoIntegralHeight);
	if (parser.Has(szJsonNemConvert))
		m_bOemConvert = parser.ReadBool(szJsonNemConvert);
	if (parser.Has(szJsonSort))
		m_bSort = parser.ReadBool(szJsonSort);
	if (parser.Has(szJsonVScroll))
		m_bVScroll = parser.ReadBool(szJsonVScroll);
	if (parser.Has(szJsonUpperCase))
		m_bUpperCase = parser.ReadBool(szJsonUpperCase);
	if (parser.Has(szJsonOwnerDraw))
		m_nComboOwnerDrawType = (OwnerDrawType)parser.ReadInt(szJsonOwnerDraw);

	if (parser.Has(szJsonItemSource))
	{
		if (!m_pItemSourceDescri)
		{
			m_pItemSourceDescri = new CItemSourceDescription();
			m_pItemSourceDescri->SetParent(this);
		}
		m_pItemSourceDescri->ParseJson(parser);
	}
}
//-----------------------------------------------------------------------------
void CComboDescription::GetWindowRect(CWnd *pWnd, CRect& rect)
{
	//nel designer, le dimensioni sono date dal rettangolo della combo quando è aperta
	SendMessage(pWnd->m_hWnd, CB_GETDROPPEDCONTROLRECT, NULL, (LPARAM)&rect);
}


//-----------------------------------------------------------------------------
void CComboDescription::EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep /*= true*/)
{
	__super::EvaluateExpressions(pJsonContext, deep);
	if (m_pItemSourceDescri)
		pJsonContext->Evaluate(m_pItemSourceDescri->m_Expressions, this);
}

//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CComboDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	ComboType nComboType = SIMPLE;
	if ((dwStyle & CBS_DROPDOWNLIST) == CBS_DROPDOWNLIST)
		nComboType = DROPDOWNLIST;
	else if ((dwStyle & CBS_DROPDOWN) == CBS_DROPDOWN)
		nComboType = DROPDOWN;
	else if ((dwStyle & CBS_SIMPLE) == CBS_SIMPLE)
		nComboType = SIMPLE;

	if (nComboType != m_nComboType)
	{
		m_nComboType = nComboType;
		SetUpdated(&m_nComboType);
	}

	OwnerDrawType nComboOwnerDrawType = NO;
	if ((dwStyle & CBS_OWNERDRAWFIXED) == CBS_OWNERDRAWFIXED)
		nComboOwnerDrawType = ODFIXED;
	else if ((dwStyle & CBS_OWNERDRAWVARIABLE) == CBS_OWNERDRAWVARIABLE)
		nComboOwnerDrawType = ODVARIABLE;
	if (nComboOwnerDrawType != m_nComboOwnerDrawType)
	{
		m_nComboOwnerDrawType = nComboOwnerDrawType;
		SetUpdated(&m_nComboOwnerDrawType);
	}

	UPDATE_BOOL(m_bAuto, CBS_AUTOHSCROLL);
	UPDATE_BOOL(m_bNoIntegralHeight, CBS_NOINTEGRALHEIGHT);
	UPDATE_BOOL(m_bOemConvert, CBS_OEMCONVERT);
	UPDATE_BOOL(m_bSort, CBS_SORT);
	UPDATE_BOOL(m_bUpperCase, CBS_UPPERCASE);
}
//-----------------------------------------------------------------------------
void CComboDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	switch (m_nComboType)
	{
	case SIMPLE:
		dwStyle |= CBS_SIMPLE;
		break;
	case DROPDOWN:
		dwStyle |= CBS_DROPDOWN;
		break;
	case DROPDOWNLIST:
		dwStyle |= CBS_DROPDOWNLIST;
		break;
	}

	switch (m_nComboOwnerDrawType)
	{
	case ODFIXED:
		dwStyle |= CBS_OWNERDRAWFIXED;
		break;
	case ODVARIABLE:
		dwStyle |= CBS_OWNERDRAWVARIABLE;
		break;
	}
	if (m_bAuto)
		dwStyle |= CBS_AUTOHSCROLL;
	if (m_bNoIntegralHeight)
		dwStyle |= CBS_NOINTEGRALHEIGHT;
	if (m_bOemConvert)
		dwStyle |= CBS_OEMCONVERT;
	if (m_bSort)
		dwStyle |= CBS_SORT;
	if (m_bVScroll)
		dwStyle |= WS_VSCROLL;
	if (m_bUpperCase)
		dwStyle |= CBS_UPPERCASE;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//-----------------------------------------------------------------------------
void CComboDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_nComboType = ((CComboDescription*)pDesc)->m_nComboType;
	m_bAuto = ((CComboDescription*)pDesc)->m_bAuto;
	m_bNoIntegralHeight = ((CComboDescription*)pDesc)->m_bNoIntegralHeight;
	m_bOemConvert = ((CComboDescription*)pDesc)->m_bOemConvert;
	m_bSort = ((CComboDescription*)pDesc)->m_bSort;
	m_bVScroll = ((CComboDescription*)pDesc)->m_bVScroll;
	m_bUpperCase = ((CComboDescription*)pDesc)->m_bUpperCase;
	m_nComboOwnerDrawType = ((CComboDescription*)pDesc)->m_nComboOwnerDrawType;
	if (((CComboDescription*)pDesc)->m_pItemSourceDescri)
		m_pItemSourceDescri = (((CComboDescription*)pDesc)->m_pItemSourceDescri)->Clone();
}

//-----------------------------------------------------------------------------
void CTabbedToolbarDescription::SerializeJson(CJsonSerializer& strJson)
{
	/*	bool bRuntime = IsRuntime();
		if (bRuntime)
		{
			SERIALIZE_INT(m_iActiveTabIndex, szJsonActiveTabIndex, 0);
		}*/
	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
void CTabbedToolbarDescription::SetActiveTabIndex(UINT iActiveIndex)
{
	if (m_iActiveTabIndex != iActiveIndex)
	{
		m_iActiveTabIndex = iActiveIndex;
		SetUpdated(&m_iActiveTabIndex);
	}
}

IMPLEMENT_DYNCREATE(CTabbedToolbarDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CTabbedToolbarDescription, TabbedToolbar)

void CTabbedToolbarDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_iActiveTabIndex = ((CTabbedToolbarDescription*)pDesc)->m_iActiveTabIndex;

}

IMPLEMENT_DYNCREATE(CWndImageDescription, CWndColoredObjDescription)
REGISTER_WND_OBJ_CLASS(CWndImageDescription, Image)

IMPLEMENT_DYNCREATE(CWndButtonDescription, CWndImageDescription)

//-----------------------------------------------------------------------------
void CWndImageDescription::SerializeJson(CJsonSerializer& strJson)
{
	/*bool bRuntime = IsRuntime();
	if (bRuntime)
	{
		SERIALIZE_BOOL(m_bRealSize, szJsonRealSizeImage, false);

	}*/
	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
CString CWndImageDescription::GetImageJson()
{
	CString sResult = L"";

	return sResult;
}

//-----------------------------------------------------------------------------
void CWndImageDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	//TODO lettura immagini; come fare?
}


//-----------------------------------------------------------------------------
void CWndImageDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_ImageBuffer.Assign(&((CWndImageDescription*)pDesc)->m_ImageBuffer);
	m_bRealSize = ((CWndImageDescription*)pDesc)->m_bRealSize;
}

//chi la chiama deve fare la delete del buffer
//-----------------------------------------------------------------------------
void CWndImageDescription::GetImageBytes(BYTE*& buffer, int &nCount)
{
	nCount = m_ImageBuffer.m_nSize;
	buffer = m_ImageBuffer.m_pBuffer;
}
IMPLEMENT_DYNCREATE(CFontDescription, CObject)

//-----------------------------------------------------------------------------
void CFontDescription::SerializeJson(CJsonSerializer& strJson, LPCTSTR szTagName)
{
	strJson.OpenObject(szTagName);

	if (!m_strFaceName.IsEmpty())
		strJson.WriteString(szJsonFontFaceName, m_strFaceName);
	if (m_nFontSize)
		strJson.WriteDouble(szJsonSize, m_nFontSize);
	if (m_bIsBold)
		strJson.WriteBool(szJsonBold, m_bIsBold);
	if (m_bIsItalic)
		strJson.WriteBool(szJsonItalic, m_bIsItalic);
	if (m_bIsUnderline)
		strJson.WriteBool(szJsonFontUnderline, m_bIsUnderline);

	strJson.CloseObject();
}
//-----------------------------------------------------------------------------
void CFontDescription::SerializeBinary(CStreamSerializer& serializer)
{
	serializer.Serialize(m_strFaceName);
	serializer.Serialize(m_nFontSize);
	serializer.Serialize(m_bIsBold);
	serializer.Serialize(m_bIsItalic);
	serializer.Serialize(m_bIsUnderline);
}

//-----------------------------------------------------------------------------
void CFontDescription::ParseJson(CJsonFormParser& parser, LPCTSTR szTagName)
{
	if (!parser.BeginReadObject(szTagName))
	{
		ASSERT(FALSE);
		return;
	}
	if (parser.Has(szJsonFontFaceName))
		m_strFaceName = parser.ReadString(szJsonFontFaceName);
	if (parser.Has(szJsonSize))
		m_nFontSize = (float)parser.ReadDouble(szJsonSize);
	if (parser.Has(szJsonBold))
		m_bIsBold = parser.ReadBool(szJsonBold);
	if (parser.Has(szJsonItalic))
		m_bIsItalic = parser.ReadBool(szJsonItalic);
	if (parser.Has(szJsonFontUnderline))
		m_bIsUnderline = parser.ReadBool(szJsonFontUnderline);
	parser.EndReadObject();
}
//-----------------------------------------------------------------------------
CFontDescription*  CFontDescription::Clone()
{
	CFontDescription* pNewFontDesc = new CFontDescription();
	pNewFontDesc->Assign(this);
	return pNewFontDesc;
}

//-----------------------------------------------------------------------------
void CFontDescription::Assign(CFontDescription* pDesc)
{
	m_strFaceName = pDesc->m_strFaceName;
	m_nFontSize = pDesc->m_nFontSize;
	m_bIsBold = pDesc->m_bIsBold;
	m_bIsItalic = pDesc->m_bIsItalic;
	m_bIsUnderline = pDesc->m_bIsUnderline;
}


IMPLEMENT_DYNCREATE(CNumbererDescription, CObject)

//-----------------------------------------------------------------------------
void CNumbererDescription::SerializeJson(CJsonSerializer& strJson, LPCTSTR szTagName)
{
	strJson.OpenObject(szTagName);

	strJson.WriteBool(szJsonUseFormatMask, m_bUseFormatMask);
	strJson.WriteString(szJsonFormatMask, m_sFormatMask);
	strJson.WriteBool(szJsonEnableCtrlInEdit, m_bEnableCtrlInEdit);
	strJson.WriteString(szJsonServiceNs, m_sServiceNs);
	strJson.WriteString(szJsonServiceName, m_sServiceName);

	strJson.CloseObject();
}
//-----------------------------------------------------------------------------
void CNumbererDescription::SerializeBinary(CStreamSerializer& serializer)
{
	serializer.Serialize(m_bUseFormatMask);
	serializer.Serialize(m_sFormatMask);
	serializer.Serialize(m_bEnableCtrlInEdit);
	serializer.Serialize(m_sServiceNs);
	serializer.Serialize(m_sServiceName);
}

//-----------------------------------------------------------------------------
void CNumbererDescription::ParseJson(CJsonFormParser& parser, LPCTSTR szTagName)
{
	if (!parser.BeginReadObject(szTagName))
	{
		ASSERT(FALSE);
		return;
	}

	if (parser.Has(szJsonUseFormatMask))
		m_bUseFormatMask = parser.ReadBool(szJsonUseFormatMask);
	if (parser.Has(szJsonFormatMask))
		m_sFormatMask = parser.ReadString(szJsonFormatMask);
	if (parser.Has(szJsonEnableCtrlInEdit))
		m_bEnableCtrlInEdit = parser.ReadBool(szJsonEnableCtrlInEdit);
	if (parser.Has(szJsonServiceNs))
		m_sServiceNs = parser.ReadString(szJsonServiceNs);
	if (parser.Has(szJsonServiceName))
		m_sServiceName = parser.ReadString(szJsonServiceName);

	parser.EndReadObject();
}
//-----------------------------------------------------------------------------
CNumbererDescription*  CNumbererDescription::Clone()
{
	CNumbererDescription* pNewDesc = new CNumbererDescription();
	pNewDesc->Assign(this);
	return pNewDesc;
}

//-----------------------------------------------------------------------------
void CNumbererDescription::Assign(CNumbererDescription* pDesc)
{
	m_bUseFormatMask = pDesc->m_bUseFormatMask;
	m_sFormatMask = pDesc->m_sFormatMask;
	m_bEnableCtrlInEdit = pDesc->m_bEnableCtrlInEdit;
	m_sServiceNs = pDesc->m_sServiceNs;
	m_sServiceName = pDesc->m_sServiceName;
}


IMPLEMENT_DYNCREATE(CLinkDescription, CObject)

//-----------------------------------------------------------------------------
void CLinkDescription::SerializeJson(CJsonSerializer& strJson)
{

	strJson.WriteInt(szJsonObjectAlias, m_nObjectAlias);

	//strJson.OpenObject(_T("rect"));
	strJson.WriteInt(szJsonX, m_Rect.left);
	strJson.WriteInt(szJsonY, m_Rect.top);
	strJson.WriteInt(szJsonWidth, m_Rect.Width());
	strJson.WriteInt(szJsonHeight, m_Rect.Height());
	//strJson.CloseObject();
}
//-----------------------------------------------------------------------------
void CLinkDescription::ParseJson(CJsonFormParser& parser)
{
}

//-----------------------------------------------------------------------------
CLinkDescription* CLinkDescription::Clone()
{
	CLinkDescription* pNewLinkDesc = new CLinkDescription();
	pNewLinkDesc->Assign(this);
	return pNewLinkDesc;
}

//-----------------------------------------------------------------------------
void CLinkDescription::Assign(CLinkDescription* pDesc)
{
	m_nObjectAlias = pDesc->m_nObjectAlias;
	m_nRow = pDesc->m_nRow;
	m_Rect = pDesc->m_Rect;
}

IMPLEMENT_DYNCREATE(CWndCheckRadioDescription, CWndButtonDescription)
REGISTER_WND_OBJ_CLASS(CWndCheckRadioDescription, Radio)
REGISTER_WND_OBJ_CLASS(CWndCheckRadioDescription, Check)

//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bChecked, szJsonChecked, false);
	SERIALIZE_BOOL(m_bLabelOnLeft, szJsonLabelOnLeft, false);
	SERIALIZE_BOOL(m_bAutomatic, szJsonAuto, true);
	SERIALIZE_BOOL(m_bThreeState, szJsonThreeState, false);
	SERIALIZE_STRING(m_strGroupName, szJsonGroupName);

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_BOOL(m_bChecked, szJsonChecked);
	PARSE_BOOL(m_bLabelOnLeft, szJsonLabelOnLeft);
	PARSE_BOOL(m_bAutomatic, szJsonAuto);
	PARSE_BOOL(m_bThreeState, szJsonThreeState);
	PARSE_STRING(m_strGroupName, szJsonGroupName);
}
//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bChecked = ((CWndCheckRadioDescription*)pDesc)->m_bChecked;
	m_strGroupName = ((CWndCheckRadioDescription*)pDesc)->m_strGroupName;
	m_bLabelOnLeft = ((CWndCheckRadioDescription*)pDesc)->m_bLabelOnLeft;
	m_bAutomatic = ((CWndCheckRadioDescription*)pDesc)->m_bAutomatic;
	m_bThreeState = ((CWndCheckRadioDescription*)pDesc)->m_bThreeState;
}


//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	bool b_m_bThreeState = false, b_m_bAutomatic = false;
	if ((dwStyle & BS_AUTO3STATE) == BS_AUTO3STATE)
	{
		b_m_bThreeState = true;
		b_m_bAutomatic = true;
	}
	if ((dwStyle & BS_3STATE) == BS_3STATE)
	{
		b_m_bThreeState = true;
	}

	if ((dwStyle & BS_AUTOCHECKBOX) == BS_AUTOCHECKBOX)
	{
		b_m_bAutomatic = true;
	}
	if ((dwStyle & BS_AUTORADIOBUTTON) == BS_AUTORADIOBUTTON)
	{
		b_m_bAutomatic = true;
	}

	if (b_m_bThreeState != m_bThreeState)
	{
		m_bThreeState = b_m_bThreeState;
		SetUpdated(&m_bThreeState);
	}
	if (b_m_bAutomatic != m_bAutomatic)
	{
		m_bAutomatic = b_m_bAutomatic;
		SetUpdated(&m_bAutomatic);
	}
}
//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	dwStyle |= m_bLabelOnLeft ? BS_LEFTTEXT : !BS_LEFTTEXT;

	if (m_bThreeState && m_bAutomatic)
		dwStyle |= BS_AUTO3STATE;
	else if (m_bThreeState)
		dwStyle |= BS_3STATE;
	else if (m_bAutomatic)
		dwStyle |= m_Type == Check ? BS_AUTOCHECKBOX : BS_AUTORADIOBUTTON;
	else
		dwStyle |= m_Type == Check ? BS_CHECKBOX : BS_RADIOBUTTON;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}
//-----------------------------------------------------------------------------
void CWndCheckRadioDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);
	CButton* pButton = dynamic_cast<CButton*>(pWnd);
	if (pButton)
	{

		bool bChecked = pButton->GetCheck() == BST_CHECKED;
		if (m_bChecked != bChecked)
		{
			m_bChecked = bChecked;
			SetUpdated(&m_bChecked);
		}
	}
	UINT typeStyle = pWnd->GetStyle();
	bool bOnLeft = ((typeStyle & BS_LEFTTEXT) == BS_LEFTTEXT);
	if (bOnLeft != this->m_bLabelOnLeft)
	{
		// set label position.
		this->m_bLabelOnLeft = bOnLeft;
		this->SetUpdated(&this->m_bLabelOnLeft);
	}

}

IMPLEMENT_DYNCREATE(CWndRadioGroupDescription, CWndColoredObjDescription)
REGISTER_WND_OBJ_CLASS(CWndRadioGroupDescription, RadioGroup)


IMPLEMENT_DYNCREATE(CWndColoredObjDescription, CWndObjDescription)

//-----------------------------------------------------------------------------
CWndColoredObjDescription::CWndColoredObjDescription()
{
}

//-----------------------------------------------------------------------------
CWndColoredObjDescription::CWndColoredObjDescription(CWndObjDescription* pParent)
	:
	CWndObjDescription(pParent)
{
}

//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndImageDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	UPDATE_BOOL(m_bRealSize, SS_REALSIZEIMAGE);
}
//---------------------------------------------------------------------
void CWndImageDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_Type == CWndObjDescription::Image && !m_strText.IsEmpty())
	{
		dwStyle |=
			(m_strText.Right(4).CompareNoCase(_T(".ico")) == 0)
			? SS_ICON
			: SS_BITMAP;
	}
	if (m_bRealSize)
		dwStyle |= SS_REALSIZEIMAGE;
	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//-----------------------------------------------------------------------------
void CWndColoredObjDescription::SerializeJson(CJsonSerializer& strJson)
{
	// Serialize the bkg color in HEX format.
	if (m_crBkgColor != EMPTY_COLOR)
	{
		strJson.WriteString(szJsonBkgColor, ToString(m_crBkgColor));
	}

	// Serialize the text color in HEX format.
	if (m_crTextColor != EMPTY_COLOR)
	{
		strJson.WriteString(szJsonForeColor, ToString(m_crTextColor));
	}

	SERIALIZE_ENUM(m_textAlign, szJsonTextAlign, TALEFT);

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CWndColoredObjDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	CString sColor;
	PARSE_STRING(sColor, szJsonBkgColor);
	if (!sColor.IsEmpty())
	{
		if (!FromString(sColor, m_crBkgColor))
		{
			TRACE2("Invalid color for %s: %s", szJsonBkgColor, sColor);
			ASSERT(FALSE);
		}
	}
	PARSE_STRING(sColor, szJsonForeColor);
	if (!sColor.IsEmpty())
	{
		if (!FromString(sColor, m_crTextColor))
		{
			TRACE2("Invalid color for %s: %s", szJsonForeColor, sColor);
			ASSERT(FALSE);
		}
	}
	PARSE_ENUM(m_textAlign, szJsonTextAlign, TextAlignment);

}
//-----------------------------------------------------------------------------
void CWndColoredObjDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_crBkgColor = ((CWndColoredObjDescription*)pDesc)->m_crBkgColor;
	m_crTextColor = ((CWndColoredObjDescription*)pDesc)->m_crTextColor;
	m_textAlign = ((CWndColoredObjDescription*)pDesc)->m_textAlign;


}

IMPLEMENT_DYNCREATE(CEditObjDescription, CTextObjDescription);
REGISTER_WND_OBJ_CLASS(CEditObjDescription, Edit)

//-----------------------------------------------------------------------------
CEditObjDescription::CEditObjDescription()
{
	m_bBorder = GetDefaultBorder();
}
//-----------------------------------------------------------------------------
CEditObjDescription::CEditObjDescription(CWndObjDescription* pParent)
	: CTextObjDescription(pParent)
{
	m_bBorder = GetDefaultBorder();
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CEditObjDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	UPDATE_BOOL(m_bMultiline, ES_MULTILINE);
	UPDATE_BOOL(m_bAutoHScroll, ES_AUTOHSCROLL);
	UPDATE_BOOL(m_bAutoVScroll, ES_AUTOVSCROLL);
	UPDATE_BOOL(m_bNoHideSelection, ES_NOHIDESEL);
	UPDATE_BOOL(m_bNumber, ES_NUMBER);
	UPDATE_BOOL(m_bPassword, ES_PASSWORD);
	UPDATE_BOOL(m_bReadOnly, ES_READONLY);
	UPDATE_BOOL(m_bUpperCase, ES_UPPERCASE);
	UPDATE_BOOL(m_bLowerCase, ES_LOWERCASE);
	UPDATE_BOOL(m_bWantReturn, ES_WANTRETURN);

	TextAlignment	textAlign = m_textAlign;
	/*
	OKKIO! ES_LEFT vale zero, il test sotto sarebbe sempre vero!
	if ((dwStyle & ES_LEFT) == ES_LEFT)
	textAlign = TALEFT;
	else */if ((dwStyle & ES_CENTER) == ES_CENTER)
		textAlign = TACENTER;
	else if ((dwStyle & ES_RIGHT) == ES_RIGHT)
		textAlign = TARIGHT;
	else
		textAlign = TALEFT;
	if (textAlign != m_textAlign)
	{
		m_textAlign = textAlign;
		SetUpdated(&m_textAlign);
	}
}
//---------------------------------------------------------------------
void CEditObjDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bMultiline)
		dwStyle |= ES_MULTILINE;
	if (m_bAutoHScroll)
		dwStyle |= ES_AUTOHSCROLL;
	if (m_bAutoVScroll)
		dwStyle |= ES_AUTOVSCROLL;
	if (m_bNoHideSelection)
		dwStyle |= ES_NOHIDESEL;
	if (m_bNumber)
		dwStyle |= ES_NUMBER;
	if (m_bPassword)
		dwStyle |= ES_PASSWORD;
	if (m_bReadOnly)
		dwStyle |= ES_READONLY;
	if (m_bUpperCase)
		dwStyle |= ES_UPPERCASE;
	if (m_bLowerCase)
		dwStyle |= ES_LOWERCASE;
	if (m_bWantReturn)
		dwStyle |= ES_WANTRETURN;

	switch (m_textAlign)
	{
	case TALEFT:	dwStyle |= ES_LEFT;		break;
	case TACENTER:	dwStyle |= ES_CENTER;	break;
	case TARIGHT:	dwStyle |= ES_RIGHT;	break;
	}

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//-----------------------------------------------------------------------------
void CEditObjDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bAutoHScroll, szJsonAutoHScroll, false);
	SERIALIZE_BOOL(m_bAutoVScroll, szJsonAutoVScroll, false);
	SERIALIZE_BOOL(m_bNoHideSelection, szJsonNoHideSelection, false);
	SERIALIZE_BOOL(m_bNumber, szJsonNumber, false);
	SERIALIZE_BOOL(m_bPassword, szJsonPassword, false);
	SERIALIZE_BOOL(m_bReadOnly, szJsonReadOnly, false);
	SERIALIZE_BOOL(m_bUpperCase, szJsonUpperCase, false);
	SERIALIZE_BOOL(m_bLowerCase, szJsonLowerCase, false);
	SERIALIZE_BOOL(m_bWantReturn, szJsonWantReturn, false);
	SERIALIZE_ENUM(m_Resizable, szJsonResizable, R_NONE);
	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CEditObjDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bAutoHScroll, szJsonAutoHScroll);
	PARSE_BOOL(m_bAutoVScroll, szJsonAutoVScroll);
	PARSE_BOOL(m_bNoHideSelection, szJsonNoHideSelection);
	PARSE_BOOL(m_bNumber, szJsonNumber);
	PARSE_BOOL(m_bPassword, szJsonPassword);
	PARSE_BOOL(m_bReadOnly, szJsonReadOnly);
	PARSE_BOOL(m_bUpperCase, szJsonUpperCase);
	PARSE_BOOL(m_bLowerCase, szJsonLowerCase);
	PARSE_BOOL(m_bWantReturn, szJsonWantReturn);
	PARSE_ENUM(m_Resizable, szJsonResizable, ResizableControl);
}

//-----------------------------------------------------------------------------
void CEditObjDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	CEditObjDescription* pTextDesc = (CEditObjDescription*)pDesc;
	m_bAutoHScroll = pTextDesc->m_bAutoHScroll;
	m_bAutoVScroll = pTextDesc->m_bAutoVScroll;
	m_bNoHideSelection = pTextDesc->m_bNoHideSelection;
	m_bNumber = pTextDesc->m_bNumber;
	m_bPassword = pTextDesc->m_bPassword;
	m_bReadOnly = pTextDesc->m_bReadOnly;
	m_bUpperCase = pTextDesc->m_bUpperCase;
	m_bLowerCase = pTextDesc->m_bLowerCase;
	m_bWantReturn = pTextDesc->m_bWantReturn;
	m_Resizable = pTextDesc->m_Resizable;
}
IMPLEMENT_DYNCREATE(CLabelDescription, CTextObjDescription);
//-----------------------------------------------------------------------------
CLabelDescription::CLabelDescription()
{
	m_bTabStop = TabStopDefault();
	m_bGroup = TRUE;
	m_Type = Label;
}
//-----------------------------------------------------------------------------
CLabelDescription::CLabelDescription(CWndObjDescription* pParent)
	: CTextObjDescription(pParent)
{
	m_bTabStop = TabStopDefault();
	m_bGroup = TRUE;
	m_Type = Label;
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CLabelDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);

	TextAlignment	textAlign = m_textAlign;
	/*OKKIO! SS_LEFT vale zero, il test sotto sarebbe sempre vero!
	if ((dwStyle & SS_LEFT) == SS_LEFT)
	textAlign = TALEFT;
	else */if ((dwStyle & SS_CENTER) == SS_CENTER)
		textAlign = TACENTER;
	else if ((dwStyle & SS_RIGHT) == SS_RIGHT)
		textAlign = TARIGHT;
	else
		textAlign = TALEFT;
	if (textAlign != m_textAlign)
	{
		m_textAlign = textAlign;
		SetUpdated(&m_textAlign);
	}
	if ((dwStyle & SS_CENTERIMAGE) == SS_CENTERIMAGE)
		m_vertAlign = VACENTER;
	else
		m_vertAlign = VATOP;

	EtchedFrameType EtchedFrame = EFNO;
	if ((dwStyle & SS_ETCHEDFRAME) == SS_ETCHEDFRAME)
		EtchedFrame = EFALL;
	else if ((dwStyle & SS_ETCHEDHORZ) == SS_ETCHEDHORZ)
		EtchedFrame = EFHORZ;
	else if ((dwStyle & SS_ETCHEDVERT) == SS_ETCHEDVERT)
		EtchedFrame = EFVERT;
	if (EtchedFrame != m_EtchedFrame)
	{
		m_EtchedFrame = EtchedFrame;
		SetUpdated(&m_EtchedFrame);
	}


	UPDATE_BOOL(m_bBitmap, SS_BITMAP);
	UPDATE_BOOL(m_bBlackFrame, SS_BLACKFRAME);
	UPDATE_BOOL(m_bBlackRect, SS_BLACKRECT);
	UPDATE_BOOL(m_bCenterImage, SS_CENTERIMAGE);
	UPDATE_BOOL(m_bEditControl, SS_EDITCONTROL);
	UPDATE_BOOL(m_bEndEllipsis, SS_ENDELLIPSIS);
	UPDATE_BOOL(m_bGrayFrame, SS_GRAYFRAME);
	UPDATE_BOOL(m_bGrayRect, SS_GRAYRECT);
	UPDATE_BOOL(m_bIcon, SS_ICON);
	UPDATE_BOOL(m_bLeftNoWrap, SS_LEFTNOWORDWRAP);
	UPDATE_BOOL(m_bNoPrefix, SS_NOPREFIX);
	UPDATE_BOOL(m_bOwnerDraw, SS_OWNERDRAW);
	UPDATE_BOOL(m_bPathEllipsis, SS_PATHELLIPSIS);
	UPDATE_BOOL(m_bSimple, SS_SIMPLE);
	UPDATE_BOOL(m_bSunken, SS_SUNKEN);
	UPDATE_BOOL(m_bWhiteFrame, SS_WHITEFRAME);
	UPDATE_BOOL(m_bWhiteRect, SS_WHITERECT);
	UPDATE_BOOL(m_bWordEllipsis, SS_WORDELLIPSIS);
	UPDATE_BOOL(m_bNotify, SS_NOTIFY);

}


//---------------------------------------------------------------------
void CLabelDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	switch (m_textAlign)
	{
	case TALEFT: dwStyle |= SS_LEFT; break;
	case TACENTER: dwStyle |= SS_CENTER; break;
	case TARIGHT: dwStyle |= SS_RIGHT; break;
	}
	switch (m_vertAlign)
	{
	case VACENTER: dwStyle |= SS_CENTERIMAGE; break;
	}
	switch (m_EtchedFrame)
	{
	case EtchedFrameType::EFALL:
		dwStyle |= SS_ETCHEDFRAME; break;

	case EtchedFrameType::EFHORZ:
		dwStyle |= SS_ETCHEDHORZ; break;

	case EtchedFrameType::EFVERT:
		dwStyle |= SS_ETCHEDVERT; break;
	}
	if (m_bBitmap)
		dwStyle |= SS_BITMAP;
	if (m_bBlackFrame)
		dwStyle |= SS_BLACKFRAME;
	if (m_bBlackRect)
		dwStyle |= SS_BLACKRECT;
	if (m_bCenterImage)
		dwStyle |= SS_CENTERIMAGE;
	if (m_bEditControl)
		dwStyle |= SS_EDITCONTROL;
	if (m_bEndEllipsis)
		dwStyle |= SS_ENDELLIPSIS;
	if (m_bGrayFrame)
		dwStyle |= SS_GRAYFRAME;
	if (m_bGrayRect)
		dwStyle |= SS_GRAYRECT;
	if (m_bIcon)
		dwStyle |= SS_ICON;
	if (m_bLeftNoWrap)
		dwStyle |= SS_LEFTNOWORDWRAP;
	if (m_bNoPrefix)
		dwStyle |= SS_NOPREFIX;
	if (m_bOwnerDraw)
		dwStyle |= SS_OWNERDRAW;
	if (m_bPathEllipsis)
		dwStyle |= SS_PATHELLIPSIS;
	if (m_bSimple)
		dwStyle |= SS_SIMPLE;
	if (m_bSunken)
		dwStyle |= SS_SUNKEN;
	if (m_bWhiteFrame)
		dwStyle |= SS_WHITEFRAME;
	if (m_bWhiteRect)
		dwStyle |= SS_WHITERECT;
	if (m_bWordEllipsis)
		dwStyle |= SS_WORDELLIPSIS;
	if (m_bNotify)
		dwStyle |= SS_NOTIFY;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//---------------------------------------------------------------------
void CLabelDescription::SerializeJson(CJsonSerializer& strJson)
{

	SERIALIZE_BOOL(m_bBitmap, szJsonBitmap, false);
	SERIALIZE_BOOL(m_bBlackFrame, szJsonBlackFrame, false);
	SERIALIZE_BOOL(m_bBlackRect, szJsonBlackRect, false);
	SERIALIZE_BOOL(m_bCenterImage, szJsonCenterImage, false);
	SERIALIZE_BOOL(m_bEditControl, szJsonEditControl, false);
	SERIALIZE_BOOL(m_bEndEllipsis, szJsonEndEllipsis, false);
	SERIALIZE_BOOL(m_bGrayFrame, szJsonGrayFrame, false);
	SERIALIZE_BOOL(m_bGrayRect, szJsonGrayRect, false);
	SERIALIZE_BOOL(m_bIcon, szJsonIcon, false);
	SERIALIZE_BOOL(m_bLeftNoWrap, szJsonLeftNoWrap, false);
	SERIALIZE_BOOL(m_bNoPrefix, szJsonNoPrefix, false);
	SERIALIZE_BOOL(m_bOwnerDraw, szJsonOwnerDraw, false);
	SERIALIZE_BOOL(m_bPathEllipsis, szJsonPathEllipsis, false);
	SERIALIZE_BOOL(m_bSimple, szJsonSimple, false);
	SERIALIZE_BOOL(m_bSunken, szJsonSunken, false);
	SERIALIZE_BOOL(m_bWhiteFrame, szJsonWhiteFrame, false);
	SERIALIZE_BOOL(m_bWhiteRect, szJsonWhiteRect, false);
	SERIALIZE_BOOL(m_bWordEllipsis, szJsonWordEllipsis, false);
	SERIALIZE_BOOL(m_bNotify, szJsonNotify, false);
	SERIALIZE_ENUM(m_EtchedFrame, szJsonEtchedFrame, EFNO);
	SERIALIZE_ENUM(m_vertAlign, szJsonVertAlign, VATOP);
	/*bool bRuntime = IsRuntime();
	if (bRuntime)
		strJson.WriteBool(szJsonIsStatic, true);*/
	__super::SerializeJson(strJson);
}
//---------------------------------------------------------------------
void CLabelDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_BOOL(m_bBitmap, szJsonBitmap);
	PARSE_BOOL(m_bBlackFrame, szJsonBlackFrame);
	PARSE_BOOL(m_bBlackRect, szJsonBlackRect);
	PARSE_BOOL(m_bCenterImage, szJsonCenterImage);
	PARSE_BOOL(m_bEditControl, szJsonEditControl);
	PARSE_BOOL(m_bEndEllipsis, szJsonEndEllipsis);
	PARSE_BOOL(m_bGrayFrame, szJsonGrayFrame);
	PARSE_BOOL(m_bGrayRect, szJsonGrayRect);
	PARSE_BOOL(m_bIcon, szJsonIcon);
	PARSE_BOOL(m_bLeftNoWrap, szJsonLeftNoWrap);
	PARSE_BOOL(m_bNoPrefix, szJsonNoPrefix);
	PARSE_BOOL(m_bOwnerDraw, szJsonOwnerDraw);
	PARSE_BOOL(m_bPathEllipsis, szJsonPathEllipsis);
	PARSE_BOOL(m_bSimple, szJsonSimple);
	PARSE_BOOL(m_bSunken, szJsonSunken);
	PARSE_BOOL(m_bWhiteFrame, szJsonWhiteFrame);
	PARSE_BOOL(m_bWhiteRect, szJsonWhiteRect);
	PARSE_BOOL(m_bWordEllipsis, szJsonWordEllipsis);
	PARSE_BOOL(m_bNotify, szJsonNotify);


	PARSE_ENUM(m_EtchedFrame, szJsonEtchedFrame, EtchedFrameType);
	PARSE_ENUM(m_vertAlign, szJsonVertAlign, VerticalAlignment);
}
//---------------------------------------------------------------------
void CLabelDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bBitmap = ((CLabelDescription*)pDesc)->m_bBitmap;
	m_bBlackFrame = ((CLabelDescription*)pDesc)->m_bBlackFrame;
	m_bBlackRect = ((CLabelDescription*)pDesc)->m_bBlackRect;
	m_bCenterImage = ((CLabelDescription*)pDesc)->m_bCenterImage;
	m_bEditControl = ((CLabelDescription*)pDesc)->m_bEditControl;
	m_bEndEllipsis = ((CLabelDescription*)pDesc)->m_bEndEllipsis;
	m_EtchedFrame = ((CLabelDescription*)pDesc)->m_EtchedFrame;
	m_bGrayFrame = ((CLabelDescription*)pDesc)->m_bGrayFrame;
	m_bGrayRect = ((CLabelDescription*)pDesc)->m_bGrayRect;
	m_bIcon = ((CLabelDescription*)pDesc)->m_bIcon;
	m_bLeftNoWrap = ((CLabelDescription*)pDesc)->m_bLeftNoWrap;
	m_bNoPrefix = ((CLabelDescription*)pDesc)->m_bNoPrefix;
	m_bOwnerDraw = ((CLabelDescription*)pDesc)->m_bOwnerDraw;
	m_bPathEllipsis = ((CLabelDescription*)pDesc)->m_bPathEllipsis;
	m_bSimple = ((CLabelDescription*)pDesc)->m_bSimple;
	m_bSunken = ((CLabelDescription*)pDesc)->m_bSunken;
	m_bWhiteFrame = ((CLabelDescription*)pDesc)->m_bWhiteFrame;
	m_bWhiteRect = ((CLabelDescription*)pDesc)->m_bWhiteRect;
	m_bWordEllipsis = ((CLabelDescription*)pDesc)->m_bWordEllipsis;
	m_bNotify = ((CLabelDescription*)pDesc)->m_bNotify;
	m_vertAlign = ((CLabelDescription*)pDesc)->m_vertAlign;
}

//-----------------------------------------------------------------------------
CTextObjDescription::CTextObjDescription()
{
}

//-----------------------------------------------------------------------------
CTextObjDescription::CTextObjDescription(CWndObjDescription* pParent)
{
}

//-----------------------------------------------------------------------------
CTextObjDescription::~CTextObjDescription()
{
	for (int i = 0; i < m_arValidators.GetCount(); i++)
	{
		SAFE_DELETE(m_arValidators.GetAt(i));
	}
	m_arValidators.RemoveAll();

	SAFE_DELETE(m_pDataAdapter);
}

//-----------------------------------------------------------------------------
void CTextObjDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bMultiline, szJsonMultiline, false);
	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CTextObjDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_BOOL(m_bMultiline, szJsonMultiline);

	if (parser.Has(szJsonFormatter))
	{
		if (parser.BeginReadObject(szJsonFormatter))
		{
			//TODO formattatori; è opportuno avere una classe a parte per la deserializzazione?
			parser.EndReadObject();
		}
	}

	if (parser.BeginReadArray(szJsonValidators))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			parser.BeginReadObject(i);
			CValidatorDescription* pDesc = new CValidatorDescription();
			pDesc->ParseJson(parser);
			m_arValidators.Add(pDesc);
			parser.EndReadObject();
		}
		parser.EndReadArray();
	}

	if (parser.Has(szJsonDataAdapter))
	{
		m_pDataAdapter = new CDataAdapterDescription();
		m_pDataAdapter->ParseJson(parser);
	}

}


//-----------------------------------------------------------------------------
void CTextObjDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);

	bool bIsMultiline = ((pWnd->GetStyle() & ES_MULTILINE) == ES_MULTILINE) /*|| (!((pWnd->GetStyle() & SS_LEFTNOWORDWRAP) == SS_LEFTNOWORDWRAP))*/;
	if (m_bMultiline != bIsMultiline)
	{
		m_bMultiline = bIsMultiline;
		SetUpdated(&m_bMultiline);
	}

	DWORD ulExStyle = pWnd->GetExStyle();
	TextAlignment align = m_textAlign;
	if ((ulExStyle & WS_EX_RIGHT) == WS_EX_RIGHT)
	{
		align = TARIGHT;
	}

	if (m_textAlign != align)
	{
		m_textAlign = align;
		SetUpdated(&m_textAlign);
	}
}

//-----------------------------------------------------------------------------
void CTextObjDescription::UpdateEnableStatus(CWnd *pWnd)
{
	//lo disabilita se la CWnd e non enabled, oppure  e' abilitata ma e' readonly
	bool bEnabled = pWnd->IsWindowEnabled() && !((pWnd->GetStyle() & ES_READONLY) == ES_READONLY);
	if (m_bEnabled != bEnabled)
	{
		m_bEnabled = bEnabled;
		SetUpdated(&m_bEnabled);
	}
}

//-----------------------------------------------------------------------------
void CTextObjDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CTextObjDescription* pTextDesc = ((CTextObjDescription*)pDesc);

	m_bMultiline = pTextDesc->m_bMultiline;
	m_pFormatter = pTextDesc->m_pFormatter;
	m_pDataAdapter = pTextDesc->m_pDataAdapter;

	m_arValidators.RemoveAll();
	for (int i = 0; i < pTextDesc->m_arValidators.GetCount(); i++)
	{
		CValidatorDescription* pNewItem = pTextDesc->m_arValidators.GetAt(i)->Clone();
		m_arValidators.Add(pNewItem);
	}
}

IMPLEMENT_DYNCREATE(CTextObjDescription, CWndColoredObjDescription)

//=============================================================================
IMPLEMENT_DYNCREATE(CListDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CListDescription, List)


//-----------------------------------------------------------------------------
CListDescription::~CListDescription()
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		SAFE_DELETE(m_arItems.GetAt(i));
	}
	m_arItems.RemoveAll();
	delete m_pItemSourceDescri;
}

//Metodo che imposta la classe del tipo degli item con cui dovra essere popolata la listbox 
//(CItemListBoxDescription o CItemCheckListDescription) 
//-----------------------------------------------------------------------------
void CListDescription::SetItemClass(CRuntimeClass* pClass)
{
	m_pListItemRTC = pClass;
}

//-----------------------------------------------------------------------------
void CListDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bDisableNoScroll, szJsonDisableNoScroll, false);
	SERIALIZE_BOOL(m_bHasStrings, szJsonHasStrings, false);
	SERIALIZE_BOOL(m_bNoIntegralHeight, szJsonNoIntegralHeight, false);
	SERIALIZE_BOOL(m_bSort, szJsonSort, false);
	SERIALIZE_BOOL(m_bWantKeyInput, szJsonWantKeyInput, false);
	SERIALIZE_BOOL(m_bMultiColumn, szJsonMultiColumn, false);
	SERIALIZE_BOOL(m_bNotify, szJsonNotify, false);

	SERIALIZE_ENUM(m_nSelection, szJsonSelection, SINGLE);
	SERIALIZE_ENUM(m_nOwnerDraw, szJsonOwnerDraw, NO);

	if (m_pItemSourceDescri)
	{
		m_pItemSourceDescri->SerializeJson(strJson);
	}
	if (m_arItems.GetCount())
	{
		strJson.OpenArray(szJsonItems);

		for (int i = 0; i < m_arItems.GetCount(); i++)
		{
			strJson.OpenObject(i);
			m_arItems[i]->SerializeJson(strJson);
			strJson.CloseObject();
		}
		strJson.CloseArray();
	}
	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CListDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bDisableNoScroll, szJsonDisableNoScroll);
	PARSE_BOOL(m_bHasStrings, szJsonHasStrings);
	PARSE_BOOL(m_bNoIntegralHeight, szJsonNoIntegralHeight);
	PARSE_BOOL(m_bSort, szJsonSort);
	PARSE_BOOL(m_bWantKeyInput, szJsonWantKeyInput);
	PARSE_BOOL(m_bMultiColumn, szJsonMultiColumn);
	PARSE_BOOL(m_bNotify, szJsonNotify);
	PARSE_ENUM(m_nOwnerDraw, szJsonOwnerDraw, OwnerDrawType);
	PARSE_ENUM(m_nSelection, szJsonSelection, SelectionType);

	if (parser.Has(szJsonItemSource))
	{
		if (!m_pItemSourceDescri)
		{
			m_pItemSourceDescri = new CItemSourceDescription();
			m_pItemSourceDescri->SetParent(this);
		}
		m_pItemSourceDescri->ParseJson(parser);
	}

	if (parser.BeginReadArray(szJsonItems))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			CItemDescription* pDesc = new CItemListBoxDescription();
			pDesc->ParseJson(parser);
			m_arItems.Add(pDesc);
		}
		parser.EndReadArray();
	}
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CListDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);

	SelectionType nSelection = SINGLE;
	if ((dwStyle & LBS_NOSEL) == LBS_NOSEL)
		nSelection = NOSEL;
	else if ((dwStyle & LBS_EXTENDEDSEL) == LBS_EXTENDEDSEL)
		nSelection = EXTENDEDSEL;
	else if ((dwStyle & LBS_MULTIPLESEL) == LBS_MULTIPLESEL)
		nSelection = MULTIPLESEL;
	if (nSelection != m_nSelection)
	{
		m_nSelection = nSelection;
		SetUpdated(&m_nSelection);
	}

	OwnerDrawType	nOwnerDraw = NO;
	if ((dwStyle & LBS_OWNERDRAWFIXED) == LBS_OWNERDRAWFIXED)
		nOwnerDraw = ODFIXED;
	else if ((dwStyle & LBS_OWNERDRAWVARIABLE) == LBS_OWNERDRAWVARIABLE)
		nOwnerDraw = ODVARIABLE;
	if (nOwnerDraw != m_nOwnerDraw)
	{
		m_nOwnerDraw = nOwnerDraw;
		SetUpdated(&m_nOwnerDraw);
	}

	UPDATE_BOOL(m_bDisableNoScroll, LBS_DISABLENOSCROLL);
	UPDATE_BOOL(m_bHasStrings, LBS_HASSTRINGS);
	UPDATE_BOOL(m_bNoIntegralHeight, LBS_NOINTEGRALHEIGHT);
	UPDATE_BOOL(m_bSort, LBS_SORT);
	UPDATE_BOOL(m_bWantKeyInput, LBS_WANTKEYBOARDINPUT);
	UPDATE_BOOL(m_bMultiColumn, LBS_MULTICOLUMN);
	UPDATE_BOOL(m_bNotify, SS_NOTIFY);
}
//---------------------------------------------------------------------
void CListDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);

	//stili cablati
	dwStyle |= LBS_NOTIFY;

	if (m_bDisableNoScroll)
		dwStyle |= LBS_DISABLENOSCROLL;

	switch (m_nSelection)
	{
	case NOSEL: dwStyle |= LBS_NOSEL; break;
	case EXTENDEDSEL: dwStyle |= LBS_EXTENDEDSEL; break;
	case MULTIPLESEL: dwStyle |= LBS_MULTIPLESEL; break;
	}

	if (m_bHasStrings)
		dwStyle |= LBS_HASSTRINGS;

	if (m_bNoIntegralHeight)
		dwStyle |= LBS_NOINTEGRALHEIGHT;

	switch (m_nOwnerDraw)
	{
	case ODFIXED: dwStyle |= LBS_OWNERDRAWFIXED; break;
	case ODVARIABLE: dwStyle |= LBS_OWNERDRAWVARIABLE; break;
	}

	if (m_bSort)
		dwStyle |= LBS_SORT;
	if (m_bWantKeyInput)
		dwStyle |= LBS_WANTKEYBOARDINPUT;
	if (m_bMultiColumn)
		dwStyle |= LBS_MULTICOLUMN;
	if (m_bNotify)
		dwStyle |= SS_NOTIFY;
}

//-----------------------------------------------------------------------------
void CListDescription::EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep /*= true*/)
{
	__super::EvaluateExpressions(pJsonContext, deep);
	if (m_pItemSourceDescri)
		pJsonContext->Evaluate(m_pItemSourceDescri->m_Expressions, this);
}



//-----------------------------------------------------------------------------
void CListDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);

	CListBox* listBox = (CListBox*)pWnd;

	//se e' cambiato il numero degli item della listbox cancello e ricreo l'array degli items
	if (m_arItems.GetCount() != listBox->GetCount())
	{
		m_arItems.RemoveAll();
		for (int i = 0; i < listBox->GetCount(); i++)
		{
			CItemDescription* itemList = (CItemDescription*)m_pListItemRTC->CreateObject();
			itemList->UpdateItemAttributes(pWnd, i);
			m_arItems.Add(itemList);
		}
		SetUpdated(&m_arItems);
	}
	else //controllo se e' cambiata il singolo item
	{
		for (int i = 0; i < listBox->GetCount(); i++)
		{
			//Se e' cambiato un suo item segno la descrizione come cambiata
			if (m_arItems.GetAt(i)->UpdateItemAttributes(pWnd, i))
				SetUpdated(&m_arItems);
		}
	}
}


//-----------------------------------------------------------------------------
void CListDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CListDescription* pListDesc = (CListDescription*)pDesc;
	m_pListItemRTC = pListDesc->m_pListItemRTC;

	m_bDisableNoScroll = pListDesc->m_bDisableNoScroll;
	m_nSelection = pListDesc->m_nSelection;
	m_bHasStrings = pListDesc->m_bHasStrings;
	m_bNoIntegralHeight = pListDesc->m_bNoIntegralHeight;
	m_nOwnerDraw = pListDesc->m_nOwnerDraw;
	m_bSort = pListDesc->m_bSort;
	m_bWantKeyInput = pListDesc->m_bWantKeyInput;
	m_bMultiColumn = pListDesc->m_bMultiColumn;
	m_bNotify = pListDesc->m_bNotify;
	if (((CListDescription*)pDesc)->m_pItemSourceDescri)
		m_pItemSourceDescri = ((CListDescription*)pDesc)->m_pItemSourceDescri->Clone();

	m_arItems.RemoveAll();
	for (int i = 0; i < ((CListDescription*)pDesc)->m_arItems.GetCount(); i++)
	{
		CItemDescription* pNewItem = ((CListDescription*)pDesc)->m_arItems.GetAt(i)->Clone();
		m_arItems.Add(pNewItem);
	}
}


//==============================================================================
IMPLEMENT_DYNAMIC(CItemDescription, CObject)

//-----------------------------------------------------------------------------
void CItemDescription::SerializeJson(CJsonSerializer& strJson)
{
	//strJson.WriteString(_T("class"), CString(GetRuntimeClass()->m_lpszClassName));
}

//-----------------------------------------------------------------------------
void CItemDescription::ParseJson(CJsonFormParser& parser)
{
}
//-----------------------------------------------------------------------------
CItemDescription* CItemDescription::Clone()
{
	CItemDescription* pNewItemDesc = (CItemDescription*)GetRuntimeClass()->CreateObject();
	pNewItemDesc->Assign(this);
	return pNewItemDesc;
}
//-----------------------------------------------------------------------------
void CItemDescription::Assign(CItemDescription* pDesc)
{
	ExpressionObject::Assign(pDesc);
}

//==============================================================================

IMPLEMENT_DYNCREATE(CItemListBoxDescription, CItemDescription)
REGISTER_WND_OBJ_CLASS(CItemListBoxDescription, ListCtrlItem)

//-----------------------------------------------------------------------------
void CItemListBoxDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.WriteString(szJsonText, m_strText);
	strJson.WriteBool(szJsonSelected, m_bSelected);
	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CItemListBoxDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_STRING(m_strText, szJsonText);
	PARSE_BOOL(m_bSelected, szJsonSelected);
}

//Riceve il puntatore alla ListBox e l'indice dell'item da cui deve prendere la descrizione
//-----------------------------------------------------------------------------
BOOL CItemListBoxDescription::UpdateItemAttributes(CWnd *pWnd, int i)
{
	BOOL bUpdated = FALSE;
	CListBox* listBox = (CListBox*)pWnd;
	int	n = listBox->GetTextLen(i);
	CString strText;
	listBox->GetText(i, strText.GetBuffer(n));
	strText.ReleaseBuffer();
	if (m_strText != strText)
	{
		m_strText = strText;
		bUpdated = TRUE;
	}
	bool bSelected = (listBox->GetSel(i) > 0);
	if (m_bSelected != bSelected)
	{
		m_bSelected = bSelected;
		bUpdated = TRUE;
	}
	return bUpdated;
}

//-----------------------------------------------------------------------------
void CItemListBoxDescription::Assign(CItemDescription* pDesc)
{
	__super::Assign(pDesc);

	m_strText = ((CItemListBoxDescription*)pDesc)->m_strText;
	m_bSelected = ((CItemListBoxDescription*)pDesc)->m_bSelected;
}

//==============================================================================

IMPLEMENT_DYNCREATE(CItemCheckListDescription, CItemListBoxDescription)
REGISTER_WND_OBJ_CLASS(CItemCheckListDescription, CheckList)

//-----------------------------------------------------------------------------
void CItemCheckListDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.WriteBool(szJsonChecked, m_bChecked);
	strJson.WriteBool(szJsonDisabled, m_bDisabled);
	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CItemCheckListDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	if (parser.Has(szJsonChecked))
		m_bChecked = parser.ReadBool(szJsonChecked);

	if (parser.Has(szJsonDisabled))
		m_bDisabled = parser.ReadBool(szJsonDisabled);

}

//Riceve il puntatore alla ListBox e l'indice dell'item da cui deve prendere la descrizione
//-----------------------------------------------------------------------------
BOOL CItemCheckListDescription::UpdateItemAttributes(CWnd *pWnd, int i)
{
	BOOL bUpdated = FALSE;
	bUpdated = __super::UpdateItemAttributes(pWnd, i);

	CCheckListBox* checkListBox = (CCheckListBox*)pWnd;
	bool bChecked = (checkListBox->GetCheck(i) == BST_CHECKED);
	if (m_bChecked != bChecked)
	{
		m_bChecked = bChecked;
		bUpdated = TRUE;
	}
	bool bDisabled = !checkListBox->IsEnabled(i);
	if (m_bDisabled != bDisabled)
	{
		m_bDisabled = bDisabled;
		bUpdated = TRUE;
	}
	return bUpdated;
}

//-----------------------------------------------------------------------------
void CItemCheckListDescription::Assign(CItemDescription* pDesc)
{
	__super::Assign(pDesc);

	m_bChecked = ((CItemCheckListDescription*)pDesc)->m_bChecked;
	m_bDisabled = ((CItemCheckListDescription*)pDesc)->m_bDisabled;
}



//=============================================================================
IMPLEMENT_DYNCREATE(CMenuDescription, CWndObjDescription)
//REGISTER_WND_OBJ_CLASS(CMenuDescription, ?) 

//-----------------------------------------------------------------------------
void CMenuDescription::UpdateAttributes(CMenu* pMenu, HWND hWnd)
{
	m_Type = CWndObjDescription::Menu;

	SetID(cwsprintf(_T("%d"), pMenu->m_hMenu));
	CRect r;
	GetMenuItemRect(hWnd, pMenu->m_hMenu, 0, r);
	SetRect(r, TRUE);
	if (pMenu && ::IsMenu(pMenu->GetSafeHmenu()))
	{
		for (int i = 0; i < (int)pMenu->GetMenuItemCount(); i++)
		{
			CMenuItemDescription* pMenuItemDesc = new CMenuItemDescription(this);
			pMenuItemDesc->SetAdded();
			pMenuItemDesc->m_Type = CWndObjDescription::MenuItem;

			//Controllo se il menu e' di classe BCMenu, perche in questo caso le voci del menu sono messe in una struttura diversa 
			//da quella del CMenu MFC da cui deriva, e quindi devo ottenere la descrizione in maniera diversa.
			//In un caso pero (Quando il menu e' popolato nella toolbarDropdown dei clientdoc) risulta che il menu e' di tipo CTBMenu,
			//ma e' valorizzato come un CMenu. Con il metodo HasItems capisco se e' valorizzato come BCMenu o come CMenu.
			//Questo e' dovuto al fatto che il metodo Appendmenu di BCMenu non e' virtuale, e nel metodo onToolbarDropdown viene passto 
			//un argomento di tipo CMenu.
			if (pMenu->IsKindOf(RUNTIME_CLASS(BCMenu)) && ((BCMenu*)pMenu)->HasItems())
				((BCMenu*)pMenu)->GetMenuText(i, pMenuItemDesc->m_strText, MF_BYPOSITION);
			else
				pMenu->GetMenuString(i, pMenuItemDesc->m_strText, MF_BYPOSITION);
			UINT menuItemId = pMenu->GetMenuItemID(i);
			pMenuItemDesc->m_strCmd = cwsprintf(_T("%d"), menuItemId);
			pMenuItemDesc->SetID(cwsprintf(_T("%d"), menuItemId));

			UINT menuItemState = pMenu->GetMenuState(i, MF_BYPOSITION);
			pMenuItemDesc->m_bEnabled = ((MF_DISABLED & menuItemState) != MF_DISABLED) || ((MF_GRAYED & menuItemState) == MF_GRAYED);
			pMenuItemDesc->m_bChecked = (MF_CHECKED & menuItemState) == MF_CHECKED;

			CMenu* pSubMenu = pMenu->GetSubMenu(i);

			if (pSubMenu) //do recursion
			{
				CMenuDescription* pSubMenuDesc = new CMenuDescription(GetParent());
				pSubMenuDesc->UpdateAttributes(pSubMenu, hWnd);
				CRect subRect = GetRect();
				subRect.OffsetRect(subRect.Width() + 25, 0);
				pSubMenuDesc->SetRect(subRect, FALSE);
				pMenuItemDesc->m_pSubMenu = pSubMenuDesc;
			}
			m_Children.Add(pMenuItemDesc);
		}
		CWnd::FromHandle(hWnd)->SendMessage(WM_CANCELMODE, NULL, NULL);
	}
}

//-----------------------------------------------------------------------------
void CMenuDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_sOwnerId = ((CMenuDescription*)pDesc)->m_sOwnerId;
}

REGISTER_WND_OBJ_CLASS(CMenuItemDescription, MenuItem)

//=============================================================================
IMPLEMENT_DYNCREATE(CMenuItemDescription, CWndObjDescription)

//-----------------------------------------------------------------------------
void CMenuItemDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bChecked, szJsonChecked, false);
	SERIALIZE_BOOL(m_bIsSeparator, szJsonIsSeparator, false);
	SERIALIZE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes::IMG);

	strJson.WriteInt(szJsonSubMenu, m_pSubMenu != NULL);

	if (m_pSubMenu)
		m_pSubMenu->SerializeJson(strJson);
	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CMenuItemDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bChecked, szJsonChecked);
	PARSE_BOOL(m_bIsSeparator, szJsonIsSeparator);
	PARSE_STRING(m_strIcon, szJsonIcon);
	PARSE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes);

	if (parser.Has(szJsonSubMenu))
	{
		m_pSubMenu = new CMenuDescription(this);
		m_pSubMenu->ParseJson(parser);
	}
}

//-----------------------------------------------------------------------------
CString CMenuItemDescription::GetEnumDescription(IconTypes value)
{
	return singletonEnumDescription.m_arIconTypes.GetDescription(value);
}
//-----------------------------------------------------------------------------
void CMenuItemDescription::GetEnumValue(CString description, IconTypes & retVal)
{
	retVal = singletonEnumDescription.m_arIconTypes.GetEnum(description);
}

//-----------------------------------------------------------------------------
void CMenuItemDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CMenuItemDescription* pMenuItemDesc = (CMenuItemDescription*)pDesc;
	m_bChecked = pMenuItemDesc->m_bChecked;
	m_bIsSeparator = pMenuItemDesc->m_bIsSeparator;
	m_strIcon = pMenuItemDesc->m_strIcon;
	m_IconType = pMenuItemDesc->m_IconType;

	if (pMenuItemDesc->m_pSubMenu != NULL)
	{
		m_pSubMenu = (CMenuDescription*)(pMenuItemDesc->m_pSubMenu->Clone());
	}
}


//==============================================================================
IMPLEMENT_DYNCREATE(CListCtrlDescription, CWndImageDescription);
REGISTER_WND_OBJ_CLASS(CListCtrlDescription, ListCtrl)

//-----------------------------------------------------------------------------
CListCtrlDescription::~CListCtrlDescription()
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		SAFE_DELETE(m_arItems.GetAt(i));
	}
	m_arItems.RemoveAll();
}

//-----------------------------------------------------------------------------
void CListCtrlDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_INT(m_nIconHeight, szJsonIconHeight, 0);
	SERIALIZE_ENUM(m_nAlignment, szJsonAlignment, LC_LEFT);
	SERIALIZE_BOOL(m_bAutoArrange, szJsonAutoArrange, false);
	SERIALIZE_BOOL(m_bNoColumnHeader, szJsonNoColumnHeader, false);
	SERIALIZE_BOOL(m_bNoSortHeader, szJsonNoSortHeader, false);
	SERIALIZE_ENUM(m_nView, szJsonView, LC_ICON);
	SERIALIZE_BOOL(m_bAlwaysShowSelection, szJsonAlwaysShowSelection, false);
	SERIALIZE_BOOL(m_bSingleSelection, szJsonSingleSelection, false);

	strJson.OpenArray(szJsonHeaderTexts);
	for (int i = 0; i < m_arColumnHeaderText.GetCount(); i++)
		strJson.WriteString(szJsonHeaderText, m_arColumnHeaderText[i]);
	strJson.CloseArray();

	strJson.OpenArray(szJsonItems);
	for (int i = 0; i < m_arItems.GetCount(); i++)
		((CListCtrlItemDescription*)m_arItems[i])->SerializeJson(strJson);
	strJson.CloseArray();
	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CListCtrlDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	if (parser.Has(szJsonIconHeight))
		m_nIconHeight = parser.ReadInt(szJsonIconHeight);

	if (parser.Has(szJsonAutoArrange))
		m_bAutoArrange = parser.ReadBool(szJsonAutoArrange);

	if (parser.Has(szJsonNoColumnHeader))
		m_bNoColumnHeader = parser.ReadBool(szJsonNoColumnHeader);

	if (parser.Has(szJsonNoSortHeader))
		m_bNoSortHeader = parser.ReadBool(szJsonNoSortHeader);

	PARSE_ENUM(m_nAlignment, szJsonAlignment, ListCtrlAlign);
	PARSE_ENUM(m_nView, szJsonView, ListCtrlViewMode);

	if (parser.Has(szJsonAlwaysShowSelection))
		m_bAlwaysShowSelection = parser.ReadBool(szJsonAlwaysShowSelection);

	if (parser.Has(szJsonSingleSelection))
		m_bSingleSelection = parser.ReadBool(szJsonSingleSelection);

	if (parser.BeginReadArray(szJsonHeaderTexts))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			if (parser.BeginReadObject(i))
			{
				m_arColumnHeaderText.Add(parser.ReadString(szJsonHeaderTexts));
				parser.EndReadObject();
			}
		}
		parser.EndReadArray();
	}

	if (parser.BeginReadArray(szJsonItems))
	{
		for (int i = 0; i < parser.GetCount(); i++)
		{
			if (parser.BeginReadObject(i))
			{
				CListCtrlItemDescription* pDesc = new CListCtrlItemDescription(this);
				pDesc->ParseJson(parser);
				m_arItems.Add(pDesc);
				parser.EndReadObject();
			}
		}
		parser.EndReadArray();
	}
}

//-----------------------------------------------------------------------------
void CListCtrlDescription::UpdateAttributes(CWnd* pWnd)
{
	CListCtrl* pListCtrl = (CListCtrl*)pWnd;
	__super::UpdateAttributes(pWnd);

	//se e' cambiato il numero degli item della listbox cancello e ricreo l'array degli items
	if (m_arItems.GetCount() != pListCtrl->GetItemCount())
	{
		m_arItems.RemoveAll();
		for (int i = 0; i < pListCtrl->GetItemCount(); i++)
		{
			CListCtrlItemDescription* pItemDescr = new CListCtrlItemDescription(this);
			pItemDescr->UpdateItemDescription(pListCtrl, i);
			m_arItems.Add(pItemDescr);
		}
		SetUpdated(&m_arItems);
	}
	else //controllo se e' cambiata il singolo item
	{
		for (int i = 0; i < m_arItems.GetCount(); i++)
		{
			//Se e' cambiato un suo item segno la descrizione come cambiata
			if (m_arItems.GetAt(i)->UpdateItemDescription(pListCtrl, i))
				SetUpdated(&m_arItems);
		}
	}

	CWndObjDescription::WndObjType type = CWndObjDescription::ListCtrl;
	CString strViewMode = _T("");
	CImageList* pImageList = NULL;
	switch (pListCtrl->GetView())
	{
	case LV_VIEW_DETAILS:
	{
		type = CWndObjDescription::ListCtrlDetails;
		strViewMode = _T("Details");
		CHeaderCtrl* pHeaderCtrl = pListCtrl->GetHeaderCtrl();
		m_arColumnHeaderText.RemoveAll();
		for (int i = 0; i < pHeaderCtrl->GetItemCount(); i++)
		{
			HDITEM item;
			enum { sizeOfBuffer = 256 };
			TCHAR  lpBuffer[sizeOfBuffer];

			item.mask = HDI_TEXT;
			item.pszText = lpBuffer;
			item.cchTextMax = sizeOfBuffer;

			pHeaderCtrl->GetItem(i, &item);
			m_arColumnHeaderText.Add(item.pszText);
		}
		break;
	}
	case LV_VIEW_ICON:
	{
		strViewMode = _T("Icon");
		pImageList = pListCtrl->GetImageList(TVSIL_NORMAL);
		break;
	}
	case LV_VIEW_LIST:
	{
		strViewMode = _T("List");
		break;
	}
	case LV_VIEW_SMALLICON:
	{
		strViewMode = _T("SmallIcon");
		pImageList = pListCtrl->GetImageList(LVSIL_SMALL);
		break;
	}
	case LV_VIEW_TILE:
	{
		strViewMode = _T("Tile");
		break;
	}
	}
	// il tipo e' diverso se la visualizzazione e' "Details", lato web verra generato un controllo diverso (tabellare)
	// rispetto agli altri casi
	if (m_Type != type)
	{
		m_Type = type;
		SetUpdated(&m_Type);
	}
	if (pImageList && pImageList->GetImageCount())
	{
		IMAGEINFO imageInfo;
		pImageList->GetImageInfo(0, &imageInfo);
		CString sName;
		sName.AppendFormat(_T("lv%ud.png"), imageInfo.hbmImage);

		COLORREF bkColor = pImageList->GetBkColor();
		pImageList->SetBkColor(WND_IMAGE_BACK_COLOR);
		CImage aImage;
		aImage.Attach(imageInfo.hbmImage);
		if (m_ImageBuffer.Assign(&aImage, sName))
			SetUpdated(&m_ImageBuffer); //e' cambiata l'immagina, segno descrizione come aggiornata
		pImageList->SetBkColor(bkColor);
		aImage.Detach();

		int nIconHeight = imageInfo.rcImage.bottom - imageInfo.rcImage.top;
		if (m_nIconHeight != nIconHeight)
		{
			m_nIconHeight = nIconHeight;
			SetUpdated(&m_nIconHeight);
		}
	}
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CListCtrlDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);

	ListCtrlAlign align = m_nAlignment;
	if ((dwStyle & LVS_ALIGNLEFT) == LVS_ALIGNLEFT)
		align = LC_LEFT;
	else
		align = LC_TOP;


	if (align != m_nAlignment)
	{
		m_nAlignment = align;
		SetUpdated(&m_nAlignment);
	}

	ListCtrlViewMode	nView = m_nView;
	if ((dwStyle & LVS_SMALLICON) == LVS_SMALLICON)
		nView = LC_SMALLICON;
	else if ((dwStyle & LVS_LIST) == LVS_LIST)
		nView = LC_LIST;
	else if ((dwStyle & LVS_REPORT) == LVS_REPORT)
		nView = LC_REPORT;

	if (nView != m_nView)
	{
		m_nView = nView;
		SetUpdated(&m_nView);
	}
	UPDATE_BOOL(m_bAutoArrange, LVS_AUTOARRANGE);
	UPDATE_BOOL(m_bNoColumnHeader, LVS_NOCOLUMNHEADER);
	UPDATE_BOOL(m_bNoSortHeader, LVS_NOSORTHEADER);
	UPDATE_BOOL(m_bAlwaysShowSelection, LVS_SHOWSELALWAYS);
	UPDATE_BOOL(m_bSingleSelection, LVS_SINGLESEL);

}
//-----------------------------------------------------------------------------
void CListCtrlDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	switch (m_nAlignment)
	{
	case LC_TOP: dwStyle |= LVS_ALIGNTOP; break;
	case LC_LEFT: dwStyle |= LVS_ALIGNLEFT; break;
	}

	if (m_bAutoArrange)
		dwStyle |= LVS_AUTOARRANGE;

	if (m_bNoColumnHeader)
		dwStyle |= LVS_NOCOLUMNHEADER;

	if (m_bNoSortHeader)
		dwStyle |= LVS_NOSORTHEADER;

	switch (m_nView)
	{
	case LC_SMALLICON: dwStyle |= LVS_SMALLICON; break;
	case LC_LIST: dwStyle |= LVS_LIST; break;
	case LC_REPORT: dwStyle |= LVS_REPORT; break;
	}

	if (m_bAlwaysShowSelection)
		dwStyle |= LVS_SHOWSELALWAYS;

	if (m_bSingleSelection)
		dwStyle |= LVS_SINGLESEL;


	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//-----------------------------------------------------------------------------
void CListCtrlDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CListCtrlDescription* pListCtrlDesc = ((CListCtrlDescription*)pDesc);
	m_nIconHeight = pListCtrlDesc->m_nIconHeight;
	//	m_strViewMode = pListCtrlDesc->m_strViewMode;
	m_arColumnHeaderText.RemoveAll();
	m_arColumnHeaderText.Append(pListCtrlDesc->m_arColumnHeaderText);

	m_nAlignment = pListCtrlDesc->m_nAlignment;
	m_bAutoArrange = pListCtrlDesc->m_bAutoArrange;
	m_bNoColumnHeader = pListCtrlDesc->m_bNoColumnHeader;
	m_bNoSortHeader = pListCtrlDesc->m_bNoSortHeader;
	m_nView = pListCtrlDesc->m_nView;
	m_bAlwaysShowSelection = pListCtrlDesc->m_bAlwaysShowSelection;
	m_bSingleSelection = pListCtrlDesc->m_bSingleSelection;

	m_arItems.RemoveAll();
	for (int i = 0; i < ((CListCtrlDescription*)pDesc)->m_arItems.GetCount(); i++)
	{
		CListCtrlItemDescription* pListCtrlItemDesc = (CListCtrlItemDescription*)((CListCtrlDescription*)pDesc)->m_arItems.GetAt(i)->Clone();
		m_arItems.Add(pListCtrlItemDesc);
	}
};




//==============================================================================
IMPLEMENT_DYNCREATE(CListCtrlItemDescription, CWndObjDescription);
REGISTER_WND_OBJ_CLASS(CListCtrlItemDescription, ListCtrlItem);
//-----------------------------------------------------------------------------
CListCtrlItemDescription::~CListCtrlItemDescription()
{
	for (int i = 0; i < m_arCells.GetCount(); i++)
	{
		SAFE_DELETE(m_arCells.GetAt(i));
	}
	m_arCells.RemoveAll();
}

//-----------------------------------------------------------------------------
void CListCtrlItemDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.WriteInt(szJsonItemIdx, m_nIdxItem);
	strJson.WriteBool(szJsonSelected, m_bSelected == TRUE);

	strJson.OpenArray(szJsonCells);

	for (int i = 0; i < m_arCells.GetCount(); i++)
		m_arCells.GetAt(i)->SerializeJson(strJson);

	strJson.CloseArray();

	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CListCtrlItemDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	if (parser.Has(szJsonItemIdx))
		m_nIdxItem = parser.ReadInt(szJsonItemIdx);
	if (parser.Has(szJsonSelected))
		m_bSelected = parser.ReadBool(szJsonSelected);
	if (parser.BeginReadObject(szJsonCells))
	{
		CWndObjDescription* pLabel = new CWndObjDescription(GetParent());
		pLabel->ParseJson(parser);
		m_arCells.Add(pLabel);
		parser.EndReadObject();
	}
}

//Metodo che aggiorna la descrizione di un item del ListCtrl. Puo' aggiornare la precedente descrizione, se gia esistente, o crearne 
//una nuova
//-----------------------------------------------------------------------------
BOOL CListCtrlItemDescription::UpdateItemDescription(CListCtrl* pListCtrl, int index)
{
	BOOL bUpdated = FALSE;
	SetID(cwsprintf(_T("%s_%d"), GetParent()->GetID(), index));
	m_Type = CWndObjDescription::ListCtrlItem;
	m_nIdxItem = index;

	//Se e' cambiato il rettangolo, la descrizione e' da aggiornare
	CRect itemRect(0, 0, 0, 0);
	pListCtrl->GetItemRect(index, itemRect, LVIR_BOUNDS);
	itemRect.OffsetRect(GetParent()->m_X, GetParent()->m_Y);
	if (GetRect() != itemRect)
	{
		SetRect(itemRect, FALSE);
		bUpdated = TRUE;
	}
	//Popolo le varie celle della singola riga(item) del listctrl.
	//Se c'e' un headerCtrl, allora il listctrl e' visualizzato in forma tabellare, quindi oltre alla label con il testo
	//che identifica l'item, ci sono altre celle con altre informazioni testuali
	CHeaderCtrl* pHeaderCtrl = pListCtrl->GetHeaderCtrl();
	//Guardo se e' cambiato il numero di celle, se non ho il pHeaderCtrl, nell'array c'e' solo una elemento (con il testo del nodo)
	//altrimenti nell'array c'e' l'elemento con il testo del nodo + il numero di elementi che contiene il pHeaderCtrl
	BOOL bNumCellsChanged = pHeaderCtrl ? (m_arCells.GetCount() != pHeaderCtrl->GetItemCount() + 1) : (m_arCells.GetCount() != 1);
	//Se e' cambiato il numero di celle ricreo l'array, altrimenti lo aggiorno
	//creazione da zero dell'array delle celle
	if (bNumCellsChanged)
	{
		m_arCells.RemoveAll(); //pulisco array precedente se conteneva elementi
		CWndObjDescription* pLabel = new CWndObjDescription(GetParent());
		pLabel->m_strText = pListCtrl->GetItemText(index, 0);
		CRect r;
		pListCtrl->GetItemRect(index, r, LVIR_LABEL);
		r.OffsetRect(GetParent()->m_X, GetParent()->m_Y);
		pLabel->SetRect(r, TRUE);
		m_arCells.Add(pLabel);

		if (pHeaderCtrl)
		{
			for (int i = 1; i < pHeaderCtrl->GetItemCount(); i++)
			{
				CWndObjDescription* pCell = new CWndObjDescription(GetParent());
				pCell->m_strText = pListCtrl->GetItemText(index, i);
				pListCtrl->GetSubItemRect(index, i, LVIR_BOUNDS, r);
				r.OffsetRect(GetParent()->m_X, GetParent()->m_Y);
				pCell->SetRect(r, TRUE);
				m_arCells.Add(pCell);
			}
		}
	}
	//aggiornamento dell'array delle celle esistente
	else if (m_arCells.GetCount() > 0) //numero di elementi invariato, guardo se ci sono cambiamenti e se si, aggiorno la descrizione
	{
		//prendo la descrizione della label dall'array degli items (e' il primo elemento)
		CWndObjDescription* pLabel = m_arCells.GetAt(0);
		//controllo se e' cambiato il testo della label
		CString strText = pListCtrl->GetItemText(index, 0);
		if (pLabel->m_strText != strText)
		{
			pLabel->m_strText = strText;
			bUpdated = TRUE;
		}
		//controllo se e' cambiata la dimensione del rettangolo della label
		CRect labelRect(0, 0, 0, 0);
		pListCtrl->GetItemRect(index, labelRect, LVIR_LABEL);
		labelRect.OffsetRect(GetParent()->m_X, GetParent()->m_Y);
		if (pLabel->GetRect() != labelRect)
		{
			pLabel->SetRect(labelRect, FALSE);
			bUpdated = TRUE;
		}
		if (pHeaderCtrl)
		{
			for (int i = 1; i < pHeaderCtrl->GetItemCount(); i++)
			{
				//prendo le eventuali descrizioni dei subitems
				CWndObjDescription* pCell = m_arCells.GetAt(i);
				//controllo se e' cambiato il testo del subitem
				CString strSubItemText = pListCtrl->GetItemText(index, i);
				if (pCell->m_strText != strSubItemText)
				{
					pCell->m_strText = strSubItemText;
					bUpdated = TRUE;
				}
				//controllo se e' cambiata la dimensione del rettangolo del subitem
				CRect subItemRect(0, 0, 0, 0);
				pListCtrl->GetSubItemRect(index, i, LVIR_BOUNDS, subItemRect);
				subItemRect.OffsetRect(GetParent()->m_X, GetParent()->m_Y);
				if (pCell->GetRect() != subItemRect)
				{
					pCell->SetRect(subItemRect, FALSE);
					bUpdated = TRUE;
				}
			}
		}
	}
	bool bSelected = (pListCtrl->GetItemState(index, LVIS_SELECTED) & LVIS_SELECTED) != 0;
	if (m_bSelected != bSelected)
	{
		m_bSelected = bSelected;
		bUpdated = TRUE;
	}
	return bUpdated;
}

//-----------------------------------------------------------------------------
void CListCtrlItemDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_nIdxItem = ((CListCtrlItemDescription*)pDesc)->m_nIdxItem;
	m_bSelected = ((CListCtrlItemDescription*)pDesc)->m_bSelected;
	m_arCells.RemoveAll();
	for (int i = 0; i < ((CListCtrlItemDescription*)pDesc)->m_arCells.GetCount(); i++)
	{
		CWndObjDescription* pCellDesc = ((CListCtrlItemDescription*)pDesc)->m_arCells.GetAt(i)->Clone();
		m_arCells.Add(pCellDesc);
	}
};


//==============================================================================
IMPLEMENT_DYNCREATE(CWndStatusBarDescription, CWndObjDescription);
REGISTER_WND_OBJ_CLASS(CWndStatusBarDescription, StatusBar)

//reimplemento il metodo virtuale AddChildWindows. Questo perche' i panelli indicatori della CStatusBar 
//non sono finestre figlie della statusbar devo quindi aggiungerli ai figli di questa descrizione esplicitamente
//-----------------------------------------------------------------------------
void CWndStatusBarDescription::AddChildWindows(CWnd* pWnd)
{
	if (pWnd->IsKindOf(RUNTIME_CLASS(CStatusBar)))
	{
		CStatusBar* pStatusBar = (CStatusBar*)pWnd;
		int nPaneCount = pStatusBar->GetCount();
		for (int i = 0; i < nPaneCount; i++)
		{
			//creo o recupero la descrizione del singolo indicatore della statusbar
			//come id uso handle della status + indice dell'indicatore
			CString strPaneId = cwsprintf(_T("sb_%d_%d"), pWnd->m_hWnd, i);
			CWndObjDescription* pPaneDesc = m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndObjDescription), strPaneId);
			pPaneDesc->m_Type = CWndObjDescription::SbPane;
			//Se e' cambiato il testo, devo segnare la descrizione come aggiornata
			CString sText = pStatusBar->GetPaneText(i);
			//Controllo se il testo e' visibile
			UINT nID, nStyle;
			int cxWidth;
			pStatusBar->GetPaneInfo(i, nID, nStyle, cxWidth);
			BOOL bTextVisible = ((nStyle & SBPS_DISABLED) != SBPS_DISABLED);
			if (pPaneDesc->m_strText != sText && bTextVisible)
			{
				pPaneDesc->m_strText = sText;
				pPaneDesc->SetUpdated(&pPaneDesc->m_strText);
			}
			//Se e' cambiato il rettangolo, devo segnare la descrizione come aggiornata
			CRect rectPane(0, 0, 0, 0);
			pStatusBar->GetItemRect(i, &rectPane);
			//devo rendere le dimensioni del rettangolo dell singolo indicator relative al frame, siccome la
			//GetItemRect le restituisce relative alla CStatusBar
			rectPane.OffsetRect(m_X, m_Y);
			if (pPaneDesc->GetRect() != rectPane)
			{
				pPaneDesc->SetRect(rectPane, FALSE);
			}
		}
	}
}

//==============================================================================
IMPLEMENT_DYNCREATE(CWndProgressBarDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CWndProgressBarDescription, ProgressBar)

//-----------------------------------------------------------------------------
void CWndProgressBarDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);

	CProgressCtrl* pProgressCtrl = (CProgressCtrl*)pWnd;
	int nLower, nUpper;
	pProgressCtrl->GetRange(nLower, nUpper);
	int nPos = pProgressCtrl->GetPos();

	if (m_nLower != nLower)
	{
		m_nLower = nLower;
		SetUpdated(&m_nLower);
	}

	if (m_nUpper != nUpper)
	{
		m_nUpper = nUpper;
		SetUpdated(&m_nUpper);
	}

	if (m_nPos != nPos)
	{
		m_nPos = nPos;
		SetUpdated(&m_nPos);
	}
}

//-----------------------------------------------------------------------------
void CWndProgressBarDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_nLower = ((CWndProgressBarDescription*)pDesc)->m_nLower;
	m_nUpper = ((CWndProgressBarDescription*)pDesc)->m_nUpper;
	m_nPos = ((CWndProgressBarDescription*)pDesc)->m_nPos;
}

//==============================================================================
IMPLEMENT_DYNCREATE(CWndPanelDescription, CWndColoredObjDescription)
REGISTER_WND_OBJ_CLASS(CWndPanelDescription, Panel)
REGISTER_WND_OBJ_CLASS(CWndPanelDescription, View)


//-----------------------------------------------------------------------------
void CWndPanelDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bModalFrame, szJsonModalFrame, false);
	SERIALIZE_BOOL(m_bCenter, szJsonCenter, false);
	SERIALIZE_BOOL(m_bCenterMouse, szJsonCenterMouse, false);
	SERIALIZE_BOOL(m_bUserControl, szJsonUserControl, false);
	SERIALIZE_BOOL(m_bCaption, szJsonCaption, false);
	SERIALIZE_BOOL(m_bSystemMenu, szJsonSystemMenu, false);
	SERIALIZE_BOOL(m_bClipChildren, szJsonClipChildren, false);
	SERIALIZE_BOOL(m_bClipSiblings, szJsonClipSiblings, false);
	SERIALIZE_BOOL(m_bDialogFrame, szJsonDialogFrame, false);
	SERIALIZE_BOOL(m_bMaximizeBox, szJsonMaximizeBox, false);
	SERIALIZE_BOOL(m_bMinimizeBox, szJsonMinimizeBox, false);
	SERIALIZE_BOOL(m_bOverlapped, szJsonOverlapped, false);
	SERIALIZE_BOOL(m_bThickFrame, szJsonThickFrame, false);
	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CWndPanelDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_BOOL(m_bModalFrame, szJsonModalFrame);
	PARSE_BOOL(m_bCenter, szJsonCenter);
	PARSE_BOOL(m_bCenterMouse, szJsonCenterMouse);
	PARSE_BOOL(m_bUserControl, szJsonUserControl);
	PARSE_BOOL(m_bCaption, szJsonCaption);
	PARSE_BOOL(m_bSystemMenu, szJsonSystemMenu);
	PARSE_BOOL(m_bClipChildren, szJsonClipChildren);
	PARSE_BOOL(m_bClipSiblings, szJsonClipSiblings);
	PARSE_BOOL(m_bDialogFrame, szJsonDialogFrame);
	PARSE_BOOL(m_bMaximizeBox, szJsonMaximizeBox);
	PARSE_BOOL(m_bMinimizeBox, szJsonMinimizeBox);
	PARSE_BOOL(m_bOverlapped, szJsonOverlapped);
	PARSE_BOOL(m_bThickFrame, szJsonThickFrame);

}


//-----------------------------------------------------------------------------
void CWndPanelDescription::Assign(CWndObjDescription* pDesc)
{
	m_bModalFrame = ((CWndPanelDescription*)pDesc)->m_bModalFrame;
	m_bCenter = ((CWndPanelDescription*)pDesc)->m_bCenter;
	m_bCenterMouse = ((CWndPanelDescription*)pDesc)->m_bCenterMouse;
	m_bUserControl = ((CWndPanelDescription*)pDesc)->m_bUserControl;
	m_bCaption = ((CWndPanelDescription*)pDesc)->m_bCaption;
	m_bSystemMenu = ((CWndPanelDescription*)pDesc)->m_bSystemMenu;
	m_bClipChildren = ((CWndPanelDescription*)pDesc)->m_bClipChildren;
	m_bClipSiblings = ((CWndPanelDescription*)pDesc)->m_bClipSiblings;
	m_bDialogFrame = ((CWndPanelDescription*)pDesc)->m_bDialogFrame;
	m_bMaximizeBox = ((CWndPanelDescription*)pDesc)->m_bMaximizeBox;
	m_bMinimizeBox = ((CWndPanelDescription*)pDesc)->m_bMinimizeBox;
	m_bOverlapped = ((CWndPanelDescription*)pDesc)->m_bOverlapped;
	m_bThickFrame = ((CWndPanelDescription*)pDesc)->m_bThickFrame;
	__super::Assign(pDesc);
}

//-----------------------------------------------------------------------------
TbResourceType CWndPanelDescription::GetResourceType()
{
	return GetParent() && !GetParent()->IsKindOf(RUNTIME_CLASS(CDummyDescription))
		? TbControls
		: TbResources;
}
//-----------------------------------------------------------------------------
//a seconda della tipologia di finestra, si applica WS_BORDER o WS_EX_CLIENTEDGE
void CWndPanelDescription::ApplyBorderStyle(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bBorder)
	{
		dwStyle |= WS_BORDER;//la classe base applica WS_EX_CLIENTEDGE
	}
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndPanelDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);


	bool b_m_bChild = (dwStyle & WS_POPUP) != WS_POPUP;
	if (m_bChild != b_m_bChild)
	{
		m_bChild = b_m_bChild;
		SetUpdated(&m_bChild);
	}
	UPDATE_BOOL(m_bModalFrame, DS_MODALFRAME);
	UPDATE_BOOL(m_bCenter, DS_CENTER);
	UPDATE_BOOL(m_bCenterMouse, DS_CENTERMOUSE);
	UPDATE_BOOL(m_bUserControl, DS_CONTROL);
	UPDATE_BOOL(m_bCaption, WS_CAPTION);
	UPDATE_BOOL(m_bSystemMenu, WS_SYSMENU);
	UPDATE_BOOL(m_bClipChildren, WS_CLIPCHILDREN);
	UPDATE_BOOL(m_bClipSiblings, WS_CLIPSIBLINGS);
	UPDATE_BOOL(m_bDialogFrame, WS_DLGFRAME);
	UPDATE_BOOL(m_bMaximizeBox, WS_MAXIMIZEBOX);
	UPDATE_BOOL(m_bMinimizeBox, WS_MINIMIZEBOX);
	UPDATE_BOOL(m_bOverlapped, WS_OVERLAPPED);
	UPDATE_BOOL(m_bThickFrame, WS_THICKFRAME);
	UPDATE_BOOL(m_bBorder, WS_BORDER);
}
//---------------------------------------------------------------------
void CWndPanelDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	dwStyle |= DS_SETFONT;
	//dwExStyle |= WS_EX_CONTROLPARENT;

	if (!m_bChild)//se non sono child, applico lo stile WS_POPUP, altrimenti il parent applicherà lo stile WS_CHILD
		dwStyle |= WS_POPUP;
	if (m_bModalFrame)
		dwStyle |= DS_MODALFRAME;
	if (m_bCenter)
		dwStyle |= DS_CENTER;
	if (m_bCenterMouse)
		dwStyle |= DS_CENTERMOUSE;
	if (m_bUserControl)
		dwStyle |= DS_CONTROL;
	if (m_bCaption)
		dwStyle |= WS_CAPTION;
	if (m_bSystemMenu)
		dwStyle |= WS_SYSMENU;
	if (m_bClipChildren)
		dwStyle |= WS_CLIPCHILDREN;
	if (m_bClipSiblings)
		dwStyle |= WS_CLIPSIBLINGS;
	if (m_bDialogFrame)
		dwStyle |= WS_DLGFRAME;
	if (m_bMaximizeBox)
		dwStyle |= WS_MAXIMIZEBOX;
	if (m_bMinimizeBox)
		dwStyle |= WS_MINIMIZEBOX;
	if (m_bOverlapped)
		dwStyle |= WS_OVERLAPPED;
	if (m_bThickFrame)
		dwStyle |= WS_THICKFRAME;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

/////////////////////////////////////////////////////////////////////////////
//			CWndFrameDescription
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CWndFrameDescription, CWndPanelDescription)
REGISTER_WND_OBJ_CLASS(CWndFrameDescription, Frame)
//-----------------------------------------------------------------------------
void CWndFrameDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bWizard, szJsonWizard, false);
	SERIALIZE_BOOL(m_bStepper, szJsonStepper, false);
	SERIALIZE_BOOL(m_bDockable, szJsonDockable, true);
	SERIALIZE_BOOL(m_bStatusBar, szJsonStatusBar, true);
	SERIALIZE_ENUM(m_ViewCategory, szJsonCategory, VIEW_DATA_ENTRY);

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CWndFrameDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_BOOL(m_bWizard, szJsonWizard);
	PARSE_BOOL(m_bStepper, szJsonStepper);
	PARSE_BOOL(m_bDockable, szJsonDockable);
	PARSE_BOOL(m_bStatusBar, szJsonStatusBar);
	PARSE_ENUM(m_ViewCategory, szJsonCategory, ViewCategory);
}

//-----------------------------------------------------------------------------
void CWndFrameDescription::Assign(CWndObjDescription* pDesc)
{
	m_bWizard = ((CWndFrameDescription*)pDesc)->m_bWizard;
	m_bStepper = ((CWndFrameDescription*)pDesc)->m_bStepper;
	m_bDockable = ((CWndFrameDescription*)pDesc)->m_bDockable;
	m_bStatusBar = ((CWndFrameDescription*)pDesc)->m_bStatusBar;
	m_ViewCategory = ((CWndFrameDescription*)pDesc)->m_ViewCategory;
	__super::Assign(pDesc);
}

/////////////////////////////////////////////////////////////////////////////
//			CTabberDescription
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CTabberDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CTabberDescription, Tabber)
REGISTER_WND_OBJ_CLASS(CTabberDescription, TileManager)


//-----------------------------------------------------------------------------
void CTabberDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL3(m_bIsVertical, szJsonIsVertical);
	SERIALIZE_BOOL(m_bWizard, szJsonWizard, false);
	SERIALIZE_INT(m_nIconHeight, szJsonIconHeight, 0);

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CTabberDescription::ParseJson(CJsonFormParser& parser)
{

	PARSE_BOOL3(m_bIsVertical, szJsonIsVertical);
	PARSE_BOOL(m_bWizard, szJsonWizard);
	__super::ParseJson(parser);
}

//-----------------------------------------------------------------------------
void CTabberDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bIsVertical = ((CTabberDescription*)pDesc)->m_bIsVertical;
	m_bWizard = ((CTabberDescription*)pDesc)->m_bWizard;
	m_nIconWidth = ((CTabberDescription*)pDesc)->m_nIconWidth;
	m_nIconHeight = ((CTabberDescription*)pDesc)->m_nIconHeight;
}

/////////////////////////////////////////////////////////////////////////////
//			CTabDescription
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
void CTabDescription::SerializeJson(CJsonSerializer& strJson)
{
	/*bool bRuntime = IsRuntime();
	if (bRuntime)
	{
		SERIALIZE_BOOL(m_bActive, szJsonActive, false);
		SERIALIZE_BOOL(m_bProtected, szJsonProtected, false);
	}*/

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CTabDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
}
//-----------------------------------------------------------------------------
void CTabDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bActive = ((CTabDescription*)pDesc)->m_bActive;
	m_bProtected = ((CTabDescription*)pDesc)->m_bProtected;
	m_strIconSource = ((CTabDescription*)pDesc)->m_strIconSource;
}



IMPLEMENT_DYNCREATE(CTabDescription, CWndPanelDescription)
REGISTER_WND_OBJ_CLASS(CTabDescription, Tab)

//==============================================================================
IMPLEMENT_DYNCREATE(CWndDialogDescription, CWndPanelDescription)
REGISTER_WND_OBJ_CLASS(CWndDialogDescription, Dialog)

//-----------------------------------------------------------------------------
void CWndDialogDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_bIsModal = ((CWndDialogDescription*)pDesc)->m_bIsModal;
}

//---------------------------------------------------------------------
void CWndDialogDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	//di default questa classe rappresenta una popup
	dwStyle |= WS_POPUP;
	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//-----------------------------------------------------------------------------
void CWndDialogDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);
}


//==============================================================================
IMPLEMENT_DYNCREATE(CToolbarDescription, CWndButtonDescription)
REGISTER_WND_OBJ_CLASS(CToolbarDescription, Toolbar)

//-----------------------------------------------------------------------------
CToolbarDescription::CToolbarDescription()
{
	m_Type = CWndObjDescription::Toolbar;
	m_Children.SetSortable(FALSE);
}


//-----------------------------------------------------------------------------
CToolbarDescription::CToolbarDescription(CWndObjDescription* pParent)
	:
	CWndButtonDescription(pParent)
{
	m_Children.SetSortable(FALSE);
	m_Type = CWndObjDescription::Toolbar;
}
//-----------------------------------------------------------------------------
CToolbarDescription::~CToolbarDescription()
{
}


IMPLEMENT_DYNCREATE(CGroupBoxDescription, CWndButtonDescription);

REGISTER_WND_OBJ_CLASS(CGroupBoxDescription, Group)

//-----------------------------------------------------------------------------
void CGroupBoxDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	dwStyle |= BS_GROUPBOX;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndButtonDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	VerticalAlignment vertAlign = VATOP;
	/*if ((dwStyle & BS_TOP) == BS_TOP)
		vertAlign = VATOP;
		else */if ((dwStyle & BS_VCENTER) == BS_VCENTER)
			vertAlign = VACENTER;
		else if ((dwStyle & BS_BOTTOM) == BS_BOTTOM)
			vertAlign = VABOTTOM;
	if (vertAlign != m_vertAlign)
	{
		m_vertAlign = vertAlign;
		SetUpdated(&m_vertAlign);
	}

	TextAlignment align = m_textAlign;
	//ATTENZIONE: se è BS_CENTER, è anche BS_LEFT e BS_RIGHT, quindi BS_CENTER va testato per primo
	if ((dwStyle & BS_CENTER) == BS_CENTER)
		align = TACENTER;
	else if ((dwStyle & BS_RIGHT) == BS_RIGHT)
		align = TARIGHT;
	else
		align = TALEFT;
	if (align != m_textAlign)
	{
		m_textAlign = align;
		SetUpdated(&m_textAlign);
	}

	if (!IsOwnerDraw())//questo stile è incompatibile con gli altri
	{
		UPDATE_BOOL(m_bMultiline, BS_MULTILINE);
		UPDATE_BOOL(m_bBitmap, BS_BITMAP);
		UPDATE_BOOL(m_bText, BS_TEXT);
		UPDATE_BOOL(m_bFlat, BS_FLAT);
		UPDATE_BOOL(m_bIcon, BS_ICON);
	}
	UPDATE_BOOL(m_bClipChildren, WS_CLIPCHILDREN);
}
//-----------------------------------------------------------------------------
void CWndButtonDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	switch (m_vertAlign)
	{
	case VACENTER: dwStyle |= BS_VCENTER; break;
	case VABOTTOM: dwStyle |= BS_BOTTOM; break;
	case VATOP: dwStyle |= BS_TOP; break;
	}

	switch (m_textAlign)
	{
	case TACENTER: dwStyle |= BS_CENTER; break;
	case TALEFT: dwStyle |= BS_LEFT; break;
	case TARIGHT: dwStyle |= BS_RIGHT; break;
	}

	if (!IsOwnerDraw())//questo stile è incompatibile con gli altri
	{
		if (m_bMultiline)
			dwStyle |= BS_MULTILINE;

		if (m_bBitmap)
			dwStyle |= BS_BITMAP;

		if (m_bText)
			dwStyle |= BS_TEXT;

		if (m_bFlat)
			dwStyle |= BS_FLAT;

		if (m_bIcon)
			dwStyle |= BS_ICON;
	}
	if (m_bClipChildren)
		dwStyle |= WS_CLIPCHILDREN;
	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}
//-----------------------------------------------------------------------------
void CWndButtonDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	m_vertAlign = ((CWndButtonDescription*)pDesc)->m_vertAlign;
	m_bMultiline = ((CWndButtonDescription*)pDesc)->m_bMultiline;

	m_bBitmap = ((CWndButtonDescription*)pDesc)->m_bBitmap;
	m_bFlat = ((CWndButtonDescription*)pDesc)->m_bFlat;
	m_bIcon = ((CWndButtonDescription*)pDesc)->m_bIcon;
	m_bClipChildren = ((CWndButtonDescription*)pDesc)->m_bClipChildren;
	m_bText = ((CWndButtonDescription*)pDesc)->m_bText;
	m_sImgNamespace = ((CWndButtonDescription*)pDesc)->m_sImgNamespace;
}
//-----------------------------------------------------------------------------
void CWndButtonDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_ENUM(m_vertAlign, szJsonVertAlign, VATOP);
	SERIALIZE_BOOL(m_bClipChildren, szJsonClipChildren, false);
	SERIALIZE_BOOL(m_bMultiline, szJsonMultiline, false);
	SERIALIZE_BOOL(m_bBitmap, szJsonBitmap, false);
	SERIALIZE_BOOL(m_bFlat, szJsonFlat, false);
	SERIALIZE_BOOL(m_bIcon, szJsonIcon, false);
	SERIALIZE_BOOL(m_bText, szJsonIsText, false);

	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CWndButtonDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_ENUM(m_vertAlign, szJsonVertAlign, VerticalAlignment);
	PARSE_BOOL(m_bClipChildren, szJsonClipChildren);
	PARSE_BOOL(m_bMultiline, szJsonMultiline);
	PARSE_BOOL(m_bBitmap, szJsonBitmap);
	PARSE_BOOL(m_bFlat, szJsonFlat);
	PARSE_BOOL(m_bIcon, szJsonIcon);
	PARSE_BOOL(m_bText, szJsonIsText);
}


//-----------------------------------------------------------------------------
void CWndButtonDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);
}


//-----------------------------------------------------------------------------
void CToolbarDescription::UpdateWindowText(CWnd *pWnd)
{
	//do nothing
}

//-----------------------------------------------------------------------------
void CToolbarDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_INT(m_nImageHeight, szJsonImageHeight, 0);
	SERIALIZE_BOOL(m_bBottom, szJsonBottom, false);
	SERIALIZE_BOOL(m_bAutoHide, szJsonAutoHide, true);
	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CToolbarDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_INT(m_nImageHeight, szJsonImageHeight);
	PARSE_BOOL(m_bBottom, szJsonBottom);
	PARSE_BOOL(m_bAutoHide, szJsonAutoHide);
}
//-----------------------------------------------------------------------------
void CToolbarDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	ASSERT(pDesc->IsKindOf(RUNTIME_CLASS(CToolbarDescription)));

	m_nImageHeight = ((CToolbarDescription*)pDesc)->m_nImageHeight;
	m_bBottom = ((CToolbarDescription*)pDesc)->m_bBottom;
	m_bAutoHide = ((CToolbarDescription*)pDesc)->m_bAutoHide;
}

//==============================================================================
IMPLEMENT_DYNCREATE(CToolbarBtnDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CToolbarBtnDescription, ToolbarButton)
//-----------------------------------------------------------------------------
CToolbarBtnDescription::CToolbarBtnDescription()
{
	m_Type = CWndObjDescription::ToolbarButton;
}

//-----------------------------------------------------------------------------
CToolbarBtnDescription::CToolbarBtnDescription(CWndObjDescription* pParent)
	: CWndObjDescription(pParent)
{
	m_Type = CWndObjDescription::ToolbarButton;
}

//-----------------------------------------------------------------------------
CToolbarBtnDescription::~CToolbarBtnDescription()
{
	delete m_pImageBytes;
	m_pImageBytes = NULL;
}
//-----------------------------------------------------------------------------
CString CToolbarBtnDescription::GetEnumDescription(IconTypes value)
{
	return singletonEnumDescription.m_arIconTypes.GetDescription(value);
}
//-----------------------------------------------------------------------------
void CToolbarBtnDescription::GetEnumValue(CString description, IconTypes & retVal)
{
	retVal = singletonEnumDescription.m_arIconTypes.GetEnum(description);
}

//chi la chiama deve fare la delete del buffer
//-----------------------------------------------------------------------------
void CToolbarBtnDescription::GetImageBytes(BYTE*& buffer, int &nCount)
{
	if (!m_pImageBytes)
	{
		m_pImageBytes = new CMyMemFile();
		m_pImageBytes->SetAutoDelete(TRUE);
		CWndImageDescription* pImageDescri = (CWndImageDescription*)GetParent();
		if (!pImageDescri->m_ImageBuffer.IsEmpty())
		{
			CImage img;
			pImageDescri->m_ImageBuffer.ToImage(img);
			CImage btnImg;
			btnImg.Create(m_Width, m_Height, img.GetBPP());
			btnImg.ReleaseDC();
			btnImg.Save(m_pImageBytes, Gdiplus::ImageFormatPNG);
		}
	}
	nCount = (int)m_pImageBytes->GetLength();
	buffer = m_pImageBytes->GetBuffer();
}

//-----------------------------------------------------------------------------
void CToolbarBtnDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_STRING(m_sIcon, szJsonIcon);
	SERIALIZE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes::IMG);

	SERIALIZE_BOOL(m_bIsDropdown, szJsonIsDropdown, false);
	SERIALIZE_BOOL(m_bIsSeparator, szJsonIsSeparator, false);
	SERIALIZE_BOOL(m_bRight, szJsonAlignRight, false);
	SERIALIZE_BOOL(m_bHasMenu, szJsonHasMenu, false);
	SERIALIZE_BOOL(m_bDefault, szJsonDefault, false);
	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
void CToolbarBtnDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_STRING(m_sIcon, szJsonIcon);
	PARSE_ENUM(m_IconType, szJsonIconType, CWndObjDescription::IconTypes);

	PARSE_BOOL(m_bIsDropdown, szJsonIsDropdown);
	PARSE_BOOL(m_bIsSeparator, szJsonIsSeparator);
	PARSE_BOOL(m_bRight, szJsonAlignRight);
	PARSE_BOOL(m_bHasMenu, szJsonHasMenu);
	PARSE_BOOL(m_bDefault, szJsonDefault);
}
//-----------------------------------------------------------------------------
void CToolbarBtnDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	ASSERT(pDesc->IsKindOf(RUNTIME_CLASS(CToolbarBtnDescription)));

	m_bIsDropdown = ((CToolbarBtnDescription*)pDesc)->m_bIsDropdown;
	m_bIsSeparator = ((CToolbarBtnDescription*)pDesc)->m_bIsSeparator;
	m_bRight = ((CToolbarBtnDescription*)pDesc)->m_bRight;
	m_sIcon = ((CToolbarBtnDescription*)pDesc)->m_sIcon;
	m_IconType = ((CToolbarBtnDescription*)pDesc)->m_IconType;
	m_bHasMenu = ((CToolbarBtnDescription*)pDesc)->m_bHasMenu;
	m_bDefault = ((CToolbarBtnDescription*)pDesc)->m_bDefault;
	m_ImageBuffer.Assign(&((CToolbarBtnDescription*)pDesc)->m_ImageBuffer);
}

//Aggiorna gli attributi che sono cambiati, e se ci sono cambiamenti setta lo stato della descrizione a UPDATED
//-----------------------------------------------------------------------------
void CToolbarBtnDescription::UpdateAttributes(CWnd *pWnd)
{
	__super::UpdateAttributes(pWnd);
}


//==============================================================================
IMPLEMENT_DYNCREATE(CCaptionBarDescription, CWndButtonDescription)
REGISTER_WND_OBJ_CLASS(CCaptionBarDescription, CaptionBar)
//-----------------------------------------------------------------------------
CCaptionBarDescription::CCaptionBarDescription()
{
	m_Type = CWndObjDescription::CaptionBar;
}


//-----------------------------------------------------------------------------
CCaptionBarDescription::CCaptionBarDescription(CWndObjDescription* pParent)
	:
	CWndButtonDescription(pParent)
{
	m_Type = CWndObjDescription::CaptionBar;
}
//-----------------------------------------------------------------------------
CCaptionBarDescription::~CCaptionBarDescription()
{
}

//-----------------------------------------------------------------------------
void CCaptionBarDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_INT(m_nTextAlign, szJsonTextAlign, 0);
	SERIALIZE_INT(m_nImageAlign, szJsonImageAlign, 0);

	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
void CCaptionBarDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	PARSE_INT(m_nTextAlign, szJsonTextAlign);
	PARSE_INT(m_nImageAlign, szJsonImageAlign);

}

//-----------------------------------------------------------------------------
void CCaptionBarDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	ASSERT(pDesc->IsKindOf(RUNTIME_CLASS(CCaptionBarDescription)));

	m_nTextAlign = ((CCaptionBarDescription*)pDesc)->m_nTextAlign;
	m_bHasButton = ((CCaptionBarDescription*)pDesc)->m_bHasButton;
	m_nButtonAlign = ((CCaptionBarDescription*)pDesc)->m_nButtonAlign;
	m_nImageAlign = ((CCaptionBarDescription*)pDesc)->m_nImageAlign;
}


//==============================================================================
IMPLEMENT_DYNCREATE(CSpinCtrlDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CSpinCtrlDescription, Spin)


//-----------------------------------------------------------------------------
void CSpinCtrlDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_ENUM(m_nAlignment, szJsonAlignment, UNATTACHED);
	SERIALIZE_BOOL(m_bArrowKeys, szJsonArrowKeys, true);
	SERIALIZE_BOOL(m_bAutoBuddy, szJsonAutoBuddy, false);
	SERIALIZE_BOOL(m_bNoThousands, szJsonNoThousands, false);
	SERIALIZE_BOOL(m_bSetBuddyInteger, szJsonSetBuddyInteger, false);

	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
void CSpinCtrlDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	if (parser.Has(szJsonAlignment))
		m_nAlignment = (SpinCtrlAlignment)parser.ReadInt(szJsonAlignment);

	if (parser.Has(szJsonArrowKeys))
		m_bArrowKeys = parser.ReadBool(szJsonArrowKeys);

	if (parser.Has(szJsonAutoBuddy))
		m_bAutoBuddy = parser.ReadBool(szJsonAutoBuddy);

	if (parser.Has(szJsonNoThousands))
		m_bNoThousands = parser.ReadBool(szJsonNoThousands);

	if (parser.Has(szJsonSetBuddyInteger))
		m_bSetBuddyInteger = parser.ReadBool(szJsonSetBuddyInteger);
}

//-----------------------------------------------------------------------------
void CSpinCtrlDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	ASSERT(pDesc->IsKindOf(RUNTIME_CLASS(CSpinCtrlDescription)));

	CSpinCtrlDescription* pSpinDesc = (CSpinCtrlDescription*)pDesc;

	m_nAlignment = pSpinDesc->m_nAlignment;
	m_bArrowKeys = pSpinDesc->m_bArrowKeys;
	m_bAutoBuddy = pSpinDesc->m_bAutoBuddy;
	m_bNoThousands = pSpinDesc->m_bNoThousands;
	m_bSetBuddyInteger = pSpinDesc->m_bSetBuddyInteger;
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CSpinCtrlDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	SpinCtrlAlignment align = UNATTACHED;
	if ((dwStyle & UDS_ALIGNLEFT) == UDS_ALIGNLEFT)
		align = SC_LEFT;
	else if ((dwStyle & UDS_ALIGNRIGHT) == UDS_ALIGNRIGHT)
		align = SC_RIGHT;

	if (align != m_nAlignment)
	{
		m_nAlignment = align;
		SetUpdated(&m_nAlignment);
	}

	UPDATE_BOOL(m_bArrowKeys, UDS_ARROWKEYS);
	UPDATE_BOOL(m_bAutoBuddy, UDS_AUTOBUDDY);
	UPDATE_BOOL(m_bNoThousands, UDS_NOTHOUSANDS);
	UPDATE_BOOL(m_bSetBuddyInteger, UDS_SETBUDDYINT);
}

//-----------------------------------------------------------------------------
void CSpinCtrlDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	switch (m_nAlignment)
	{
	case SC_LEFT: dwStyle |= UDS_ALIGNLEFT; break;
	case SC_RIGHT: dwStyle |= UDS_ALIGNRIGHT; break;
	}

	if (m_bArrowKeys)
		dwStyle |= UDS_ARROWKEYS;

	if (m_bAutoBuddy)
		dwStyle |= UDS_AUTOBUDDY;

	if (m_bNoThousands)
		dwStyle |= UDS_NOTHOUSANDS;

	if (m_bSetBuddyInteger)
		dwStyle |= UDS_SETBUDDYINT;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}



IMPLEMENT_DYNCREATE(CPushButtonDescription, CWndButtonDescription);

REGISTER_WND_OBJ_CLASS(CPushButtonDescription, Button)

//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CPushButtonDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	UPDATE_BOOL(m_bOwnerDraw, BS_OWNERDRAW);
	if (!m_bOwnerDraw)//questo stile è incompatibile con gli altri
	{
		UPDATE_BOOL(m_bDefault, BS_DEFPUSHBUTTON);
	}
	//la modifica alla proprietà dell'ownerDraw deve essere fatta prima della chiamata al papà
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);

}
//-----------------------------------------------------------------------------
void CPushButtonDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bOwnerDraw)//questo stile è incompatibile con gli altri
		dwStyle |= BS_OWNERDRAW;
	else
		dwStyle |= m_bDefault ? BS_DEFPUSHBUTTON : BS_PUSHBUTTON;

	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}
//-----------------------------------------------------------------------------
void CPushButtonDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	m_bDefault = ((CPushButtonDescription*)pDesc)->m_bDefault;
	m_bOwnerDraw = ((CPushButtonDescription*)pDesc)->m_bOwnerDraw;
}
//-----------------------------------------------------------------------------
void CPushButtonDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bDefault, szJsonDefault, false);
	SERIALIZE_BOOL(m_bOwnerDraw, szJsonOwnerDraw, false);
	__super::SerializeJson(strJson);
}
//-----------------------------------------------------------------------------
void CPushButtonDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bDefault, szJsonDefault);
	PARSE_BOOL(m_bOwnerDraw, szJsonOwnerDraw);
}

//-----------------------------------------------------------------------------
bool CPushButtonDescription::IsOwnerDraw()
{
	return m_bOwnerDraw;
}


//==============================================================================

IMPLEMENT_DYNCREATE(CSplitterDescription, CWndObjDescription);

REGISTER_WND_OBJ_CLASS(CSplitterDescription, Splitter)

//-----------------------------------------------------------------------------
void CSplitterDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	m_fSplitRatio = ((CSplitterDescription*)pDesc)->m_fSplitRatio;
	m_bPanesSwapped = ((CSplitterDescription*)pDesc)->m_bPanesSwapped;
	m_nSplitResolution = ((CSplitterDescription*)pDesc)->m_nSplitResolution;
	m_SplitterMode = ((CSplitterDescription*)pDesc)->m_SplitterMode;
	m_nRows = ((CSplitterDescription*)pDesc)->m_nRows;
	m_nCols = ((CSplitterDescription*)pDesc)->m_nCols;
}

//-----------------------------------------------------------------------------
void CSplitterDescription::SerializeJson(CJsonSerializer& strJson)
{
	if (m_fSplitRatio)
		strJson.WriteDouble(szJsonSplitRatio, m_fSplitRatio);

	SERIALIZE_BOOL(m_bPanesSwapped, szJsonSplitterPanesSwapped, false);
	SERIALIZE_INT(m_nSplitResolution, szJsonSplitResolution, 1);
	SERIALIZE_ENUM(m_SplitterMode, szJsonSplitterMode, S_HORIZONTAL);
	SERIALIZE_INT(m_nRows, szJsonRows, -1);
	SERIALIZE_INT(m_nCols, szJsonCols, -1);

	__super::SerializeJson(strJson);
}


//-----------------------------------------------------------------------------
void CSplitterDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	if (parser.Has(szJsonSplitRatio))
		m_fSplitRatio = (float)parser.ReadDouble(szJsonSplitRatio);

	PARSE_BOOL(m_bPanesSwapped, szJsonSplitterPanesSwapped);
	PARSE_INT(m_nSplitResolution, szJsonSplitResolution);
	PARSE_ENUM(m_SplitterMode, szJsonSplitterMode, SplitterMode);
	PARSE_INT(m_nRows, szJsonRows);
	PARSE_INT(m_nCols, szJsonCols);
}

//-----------------------------------------------------------------------------
void EnumDescriptionAssociations::InitEnumDescriptionStructures()
{
	/*WndObjType*/
	m_arWndObjType.Add(CWndObjDescription::Undefined, _T("Undefined"));
	m_arWndObjType.Add(CWndObjDescription::View, _T("View"));
	m_arWndObjType.Add(CWndObjDescription::Label, _T("Label"));
	m_arWndObjType.Add(CWndObjDescription::Button, _T("Button"));
	m_arWndObjType.Add(CWndObjDescription::Image, _T("Image"));
	m_arWndObjType.Add(CWndObjDescription::Group, _T("Group"));
	m_arWndObjType.Add(CWndObjDescription::Radio, _T("Radio"));
	m_arWndObjType.Add(CWndObjDescription::Check, _T("Check"));
	m_arWndObjType.Add(CWndObjDescription::Combo, _T("Combo"));
	m_arWndObjType.Add(CWndObjDescription::Edit, _T("Edit"));
	m_arWndObjType.Add(CWndObjDescription::Toolbar, _T("Toolbar"));
	m_arWndObjType.Add(CWndObjDescription::ToolbarButton, _T("ToolbarButton"));
	m_arWndObjType.Add(CWndObjDescription::Tabber, _T("Tabber"));
	m_arWndObjType.Add(CWndObjDescription::Tab, _T("Tab"));
	m_arWndObjType.Add(CWndObjDescription::BodyEdit, _T("BodyEdit"));
	m_arWndObjType.Add(CWndObjDescription::TreeBodyEdit, _T("TreeBodyEdit"));
	m_arWndObjType.Add(CWndObjDescription::ColTitle, _T("ColTitle"));
	m_arWndObjType.Add(CWndObjDescription::List, _T("List"));
	m_arWndObjType.Add(CWndObjDescription::CheckList, _T("CheckList"));
	m_arWndObjType.Add(CWndObjDescription::Tree, _T("Tree"));
	m_arWndObjType.Add(CWndObjDescription::TreeNode, _T("TreeNode"));
	m_arWndObjType.Add(CWndObjDescription::Menu, _T("Menu"));
	m_arWndObjType.Add(CWndObjDescription::MenuItem, _T("MenuItem"));
	m_arWndObjType.Add(CWndObjDescription::ListCtrl, _T("ListCtrl"));
	m_arWndObjType.Add(CWndObjDescription::ListCtrlItem, _T("ListCtrlItem"));
	m_arWndObjType.Add(CWndObjDescription::ListCtrlDetails, _T("ListCtrlDetails"));
	m_arWndObjType.Add(CWndObjDescription::Spin, _T("Spin"));
	m_arWndObjType.Add(CWndObjDescription::StatusBar, _T("StatusBar"));
	m_arWndObjType.Add(CWndObjDescription::SbPane, _T("SbPane"));
	m_arWndObjType.Add(CWndObjDescription::Frame, _T("Frame"));
	m_arWndObjType.Add(CWndObjDescription::Dialog, _T("Dialog"));
	m_arWndObjType.Add(CWndObjDescription::PropertyDialog, _T("PropertyDialog"));
	m_arWndObjType.Add(CWndObjDescription::GenericWndObj, _T("GenericWndObj"));
	m_arWndObjType.Add(CWndObjDescription::TreeAdv, _T("TreeAdv"));
	m_arWndObjType.Add(CWndObjDescription::RadioGroup, _T("RadioGroup"));
	m_arWndObjType.Add(CWndObjDescription::Panel, _T("Panel"));
	m_arWndObjType.Add(CWndObjDescription::Splitter, _T("Splitter"));
	m_arWndObjType.Add(CWndObjDescription::DockingPane, _T("DockingPane"));
	m_arWndObjType.Add(CWndObjDescription::TabbedToolbar, _T("TabbedToolbar"));
	m_arWndObjType.Add(CWndObjDescription::Tile, _T("Tile"));
	m_arWndObjType.Add(CWndObjDescription::TileGroup, _T("TileGroup"));
	m_arWndObjType.Add(CWndObjDescription::TilePart, _T("TilePart"));
	m_arWndObjType.Add(CWndObjDescription::TilePartStatic, _T("TilePartStatic"));
	m_arWndObjType.Add(CWndObjDescription::TilePartContent, _T("TilePartContent"));
	m_arWndObjType.Add(CWndObjDescription::LayoutContainer, _T("LayoutContainer"));
	m_arWndObjType.Add(CWndObjDescription::HeaderStrip, _T("HeaderStrip"));
	m_arWndObjType.Add(CWndObjDescription::TileManager, _T("TileManager"));
	m_arWndObjType.Add(CWndObjDescription::TilePanel, _T("TilePanel"));
	m_arWndObjType.Add(CWndObjDescription::PropertyGrid, _T("PropertyGrid"));
	m_arWndObjType.Add(CWndObjDescription::PropertyGridItem, _T("PropertyGridItem"));
	m_arWndObjType.Add(CWndObjDescription::ProgressBar, _T("ProgressBar"));

	m_arWndObjType.Add(CWndObjDescription::CaptionBar, _T("CaptionBar"));

	//m_arWndObjType.Add(CWndObjDescription::VerticalContainer, _T("VerticalContainer"));
	//m_arWndObjType.Add(CWndObjDescription::HorizontalContainer, _T("HorizontalContainer"));

	m_arWndObjType.Add(CWndObjDescription::StatusTile, _T("StatusTile"));
	m_arWndObjType.Add(CWndObjDescription::HotFilter, _T("HotFilter"));
	m_arWndObjType.Add(CWndObjDescription::StatusTilePanel, _T("StatusTilePanel"));

	/*SplitterMode*/
	m_arSplitterMode.Add(S_HORIZONTAL, _T("Horizontal"));
	m_arSplitterMode.Add(S_VERTICAL, _T("Vertical"));

	/*CommandCategory*/
	m_arCommandCategory.Add(CTG_UNDEFINED, _T("Undefined"));
	m_arCommandCategory.Add(CTG_ADVANCED, _T("Advanced"));
	m_arCommandCategory.Add(CTG_EDIT, _T("Edit"));
	m_arCommandCategory.Add(CTG_NAVIGATION, _T("Navigation"));
	m_arCommandCategory.Add(CTG_PRINT, _T("Print"));
	m_arCommandCategory.Add(CTG_SEARCH, _T("Search"));
	m_arCommandCategory.Add(CTG_TOOLS, _T("Tools"));

	/*EtchedFrameType*/
	m_arEtchedFrameType.Add(EFALL, _T("All"));
	m_arEtchedFrameType.Add(EFHORZ, _T("Horizontal"));
	m_arEtchedFrameType.Add(EFVERT, _T("Vertical"));
	m_arEtchedFrameType.Add(EFNO, _T("No"));

	/*VerticalAlignment*/
	m_arVerticalAlignment.Add(VATOP, _T("Top"));
	m_arVerticalAlignment.Add(VABOTTOM, _T("Bottom"));
	m_arVerticalAlignment.Add(VACENTER, _T("Center"));

	/*WndDescriptionState*/
	m_arWndDescriptionState.Add(REMOVED, _T("Removed"));
	m_arWndDescriptionState.Add(UNCHANGED, _T("Unchanged"));
	m_arWndDescriptionState.Add(UPDATED, _T("Updated"));
	m_arWndDescriptionState.Add(ADDED, _T("Added"));
	m_arWndDescriptionState.Add(PARSED, _T("Parsed"));

	/*TextAlignment*/
	m_arTextAlignment.Add(TACENTER, _T("Center"));
	m_arTextAlignment.Add(TARIGHT, _T("Right"));
	m_arTextAlignment.Add(TALEFT, _T("Left"));

	/*ResizableControl*/
	m_arResizableControl.Add(R_ALL, _T("All"));
	m_arResizableControl.Add(R_HORIZONTAL, _T("Horizontal"));
	m_arResizableControl.Add(R_VERTICAL, _T("Vertical"));

	/*OwnerDrawType*/
	m_arOwnerDrawType.Add(ODFIXED, _T("Fixed"));
	m_arOwnerDrawType.Add(ODVARIABLE, _T("Variable"));
	m_arOwnerDrawType.Add(NO, _T("No"));

	/*ComboType*/
	m_arComboType.Add(SIMPLE, _T("Simple"));
	m_arComboType.Add(DROPDOWNLIST, _T("DropDownList"));
	m_arComboType.Add(DROPDOWN, _T("DropDown"));

	/*SelectionType*/
	m_arSelectionType.Add(NOSEL, _T("No"));
	m_arSelectionType.Add(EXTENDEDSEL, _T("Extended"));
	m_arSelectionType.Add(MULTIPLESEL, _T("Multiple"));
	m_arSelectionType.Add(SINGLE, _T("Single"));

	/*SpinCtrlAlignment*/
	m_arSpinCtrlAlignment.Add(SC_LEFT, _T("Left"));
	m_arSpinCtrlAlignment.Add(SC_RIGHT, _T("Right"));
	m_arSpinCtrlAlignment.Add(UNATTACHED, _T("Unattached"));

	/*ViewCategory*/
	m_arViewCategory.Add(VIEW_ACTIVITY, _T("Activity"));
	m_arViewCategory.Add(VIEW_BATCH, _T("Batch"));
	m_arViewCategory.Add(VIEW_DATA_ENTRY, _T("DataEntry"));
	m_arViewCategory.Add(VIEW_FINDER, _T("Finder"));
	m_arViewCategory.Add(VIEW_PARAMETER, _T("Parameter"));
	m_arViewCategory.Add(VIEW_WIZARD, _T("Wizard"));


	/*ControlStyle*/
	m_arControlStyles.Add(CS_NONE, _T("None"));
	m_arControlStyles.Add(CS_RESET_DEFAULTS, _T("ResetDefault"));
	m_arControlStyles.Add(CS_NUMBERS, _T("Numbers"));
	m_arControlStyles.Add(CS_LETTERS, _T("Letters"));
	m_arControlStyles.Add(CS_UPPERCASE, _T("UpperCase"));
	m_arControlStyles.Add(CS_OTHERCHARS, _T("OtherChars"));
	m_arControlStyles.Add(CS_ALLCHARS, _T("AllChars"));
	m_arControlStyles.Add(CS_NO_EMPTY, _T("NoEmpty"));
	m_arControlStyles.Add(CS_BLANKS, _T("Blanks"));
	m_arControlStyles.Add(CS_PATH_STYLE_AS_PATH, _T("AsPath"));
	m_arControlStyles.Add(CS_PATH_STYLE_NO_CHECK_EXIST, _T("NoChechExist"));
	m_arControlStyles.Add(CS_COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL, _T("EnableDelete"));

	/*ListCtrlViewMode*/
	m_arListCtrlViewMode.Add(LC_ICON, _T("Icon"));
	m_arListCtrlViewMode.Add(LC_SMALLICON, _T("SmallIcon"));
	m_arListCtrlViewMode.Add(LC_LIST, _T("List"));
	m_arListCtrlViewMode.Add(LC_REPORT, _T("Report"));

	/*ListCtrlAlign*/
	m_arListCtrlAlign.Add(LC_TOP, _T("Top"));
	m_arListCtrlAlign.Add(LC_LEFT, _T("Left"));

	/*IconTypes*/
	m_arIconTypes.Add(CWndObjDescription::IconTypes::IMG, _T("IMG"));
	m_arIconTypes.Add(CWndObjDescription::IconTypes::M4, _T("M4"));
	m_arIconTypes.Add(CWndObjDescription::IconTypes::TB, _T("TB"));
	m_arIconTypes.Add(CWndObjDescription::IconTypes::CLASS, _T("CLASS"));
}

//==============================================================================
IMPLEMENT_DYNCREATE(CHRefDescription, CWndObjDescription)
REGISTER_WND_OBJ_CLASS(CHRefDescription, HRef)
//-----------------------------------------------------------------------------
CHRefDescription::CHRefDescription()
{
	m_Type = CWndObjDescription::HRef;
}

//-----------------------------------------------------------------------------
CHRefDescription::CHRefDescription(CWndObjDescription * pParent) : CWndObjDescription(pParent)
{
	m_Type = CWndObjDescription::HRef;
}

//-----------------------------------------------------------------------------
CHRefDescription::~CHRefDescription()
{
}

//-----------------------------------------------------------------------------
void CHRefDescription::SerializeJson(CJsonSerializer & strJson)
{
	//SERIALIZE_BOOL(m_bIsNsCorrect, szJsonNamespace, true);
	SERIALIZE_STRING(m_sHRef, szJsonHref);
}

//-----------------------------------------------------------------------------
void CHRefDescription::ParseJson(CJsonFormParser & parser)
{
	//PARSE_BOOL(m_bIsNsCorrect, szJsonNamespace);
	PARSE_STRING(m_sHRef, szJsonHref);
}

//-----------------------------------------------------------------------------
void CHRefDescription::Assign(CWndObjDescription * pDesc)
{
	//m_bIsNsCorrect = ((CHRefDescription*)pDesc)->m_bIsNsCorrect;
	m_sHRef = ((CHRefDescription*)pDesc)->m_sHRef;
}
