#pragma once

#include "afx.h"

#include "beginh.dex"

class DataObj;
class CAbstractFormDoc;
class DataArray;
class CParsedCtrl;
class CDataFileInfo;


class TB_EXPORT IItemSource 
{

	int m_nMaxItemsNo = DEFAULT_COMBO_ITEMS;

public:
	bool m_bAllowEmptyData = false;

public:
	IItemSource();
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue) {}
	virtual CString GetDescription(const DataObj* pValue) { return _T(""); }
	virtual	BOOL IsValidItem(const DataObj&) { return TRUE; }
	virtual void SetControl(CParsedCtrl* pControl) = 0;
	virtual bool GetShowDescription() { return FALSE; }

	long		GetMaxItemsNo() { return m_nMaxItemsNo; }
	void		SetMaxItemsNo(int nItems) { if (nItems != 0) m_nMaxItemsNo = nItems; }

};

class TB_EXPORT CItemSource : public CObject, public IItemSource
{
       DECLARE_DYNCREATE(CItemSource)
protected:
       CParsedCtrl * m_pControl = NULL;
       CString m_strName;
       CString m_strNamespace;
       bool m_bShowDescription = false;
       bool m_bNoData = false;

public:
       CItemSource();
       virtual ~CItemSource();

       void SetControl(CParsedCtrl* pControl);
       CAbstractFormDoc* GetDocument();
       bool GetNoData() { return m_bNoData; }

       //CParsedCtrl* GetControl() { return m_pControl; }
       DataObj* GetDataObj() { return m_pControl->GetCtrlData(); }

       virtual void SetName(const CString& strName) { m_strName = strName; }
       virtual void SetNamespace(const CString& strNamespace) { m_strNamespace = strNamespace; }

       CString GetName() { return m_strName; }
       CString GetNamespace() { return m_strNamespace; }

       bool GetShowDescription() { return m_bShowDescription; }
       void SetShowDescription(bool bShowDescription) { m_bShowDescription = bShowDescription; }

       virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue) { m_bNoData = true; }
       virtual CString GetDescription(const DataObj* pValue);
       virtual       BOOL IsValidItem(const DataObj&) { return TRUE; }
       virtual       void OnControlAttached() { }


};


class TB_EXPORT CItemSourceXml : public CItemSource
{
	DECLARE_DYNCREATE(CItemSourceXml)

private:
	CString m_strNamespace;
	CString m_strParameter;
protected:
	CDataFileInfo* m_pDfi;
public:
	CItemSourceXml();
	virtual ~CItemSourceXml();
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
	void Initialize(const CString& strParameter, bool allowChanges = false, bool bUseProductLanguage = false);
	CString GetNamespace() { return m_strNamespace; }
	CString GetParameter() { return m_strParameter; }
protected:
	void SetKey(const CString& strFieldName);
	void SetHidden(const CString& strFieldName, BOOL bHidden /*= TRUE*/);

};


#include "endh.dex"
