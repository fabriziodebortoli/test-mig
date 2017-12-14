#pragma once

#include "HotFilter.h"

#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//							 HotFilterManager
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT HotFilterManager : public Array
{
	DECLARE_DYNAMIC(HotFilterManager)

public:
	HotFilterManager
		(
			CAbstractFormDoc* pCallerDoc,
			BOOL			  bViewPanelUnpinnedFilters,
			UINT			  nIDCTileGroup,
			CString			  strPanelName
		);
	~HotFilterManager();
	
public:
	HotFilterObj*		Add				(const CString& strName, CRuntimeClass* pCustomHotFilterClass, UINT nIdentificationIDC = 0);

	HotFilterObj*		AddPickerColumn	(const CString& strName, const CString& strColumnName, const CString strColumnTitle, CString strFieldName);

	void				CompleteQuery	(const CString& strName, SqlTable* pTable, SqlRecord* pRec, const DataObj& aColumn) ;

	void				OnParsedControlCreated(CParsedCtrl* pCtrl);

	template<class T> T* GetHotFilter(const CString& strName, const DataBool& bForce = FALSE) { return (T*)GetHotFilter(strName, RUNTIME_CLASS(T), bForce); }

private:
	HotFilterObj*		GetHotFilter		(const CString& strName, CRuntimeClass* pClass, const DataBool& bForce = FALSE);
	HotFilterObj*		GetExistHotFilter	(const CString& strName);

private:
	CAbstractFormDoc* m_pCallerDoc;

	BOOL			  m_bViewPanelUnpinnedFilters;
	UINT			  m_nIDCTileGroup;
	CString			  m_strPanelName;

	INT_PTR			  Add				(HotFilterObj* pHF) { return __super::Add(pHF); }

public:
	virtual void OnBuildDataControlLinks	(CAbstractFormView*  pView);
	virtual void OnBuildDataControlLinks	(CTabDialog* pTile);

	virtual BOOL OnBeforeBatchExecute		();
	virtual void OnPinUnpin					(CBaseTileDialog* pTileDlg);
	
};

//-----------------------------------------------------------------------------
class TB_EXPORT CHotFilterDescription : public CWndTileDescription
{
	DECLARE_DYNCREATE(CHotFilterDescription);
public:

	//virtual void SerializeJson(CJsonSerializer& strJson);
	//virtual void ParseJson(CJsonFormParser& parser);
	//virtual void Assign(CWndObjDescription* pDesc);
	//CString GetEnumDescription(EHotFilterType value);

};
#include "endh.dex"
