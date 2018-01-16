#pragma once

#include "afx.h"
#include "beginh.dex"

class DataObj;
class CAbstractFormDoc;
class DataArray;
class CParsedCtrl;
class TB_EXPORT CControlBehaviour : public CCmdTarget
{
friend class CParsedCtrl;
	DECLARE_DYNCREATE(CControlBehaviour)
protected:
	CAbstractFormDoc* m_pDocument = NULL;
	HotKeyLink* m_pControlHotLink = NULL;
	DataObj* m_pControlData = NULL;
	CTBNamespace* m_pFormatContext = NULL;
	int m_nFormatIdx = UNDEF_FORMAT;
	CString m_strName;
	CString m_strNamespace;
	CUIntArray m_arControlIDs;
public:
	CControlBehaviour();
	virtual ~CControlBehaviour();

	CAbstractFormDoc* GetDocument() { return m_pDocument; }
	HotKeyLink* GetHotLink() { return m_pControlHotLink; }

	void SetDocument(CAbstractFormDoc* pDocument) { m_pDocument = pDocument; OnAttachDocument(); }
	virtual void OnAttachDocument() {}
	virtual void OnSelect(DataObj* pDataObj, int nIndex) {}//per le liste, quando viene selezionato un elemento
	virtual void OnValueChanged() {}//quando cambia il dato imputato dall'utente
	virtual void SetName(const CString& strName) { m_strName = strName; }
	virtual void SetNamespace(const CString& strNamespace) { m_strNamespace = strNamespace; }
	virtual void OnPrepareForFind(SqlRecord* pRec) {}
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	void NotifyValueChanged();

	CString GetName() { return m_strName; }
	CString GetNamespace() { return m_strNamespace; }
};


#include "endh.dex"
