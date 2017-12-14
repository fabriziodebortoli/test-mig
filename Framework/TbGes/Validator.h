#pragma once

#include "afx.h"
#include <TbGeneric\DataObj.h>

#include "beginh.dex"


class CAbstractFormDoc;
class DataArray;
class CParsedCtrl;
class TB_EXPORT IValidator
{
public:
	virtual BOOL IsValid(DataObj* pDataObj, CString& message, CDiagnostic::MsgType& msgType) = 0;
};

class TB_EXPORT CValidator : public CObject, public IValidator
{
	DECLARE_DYNAMIC(CValidator)
private:
	CString m_strName;
	CAbstractFormDoc* m_pDocument;
public:
	CValidator();
	virtual ~CValidator();

	void SetName(const CString& strName) { m_strName = strName; }
	CString GetName() { return m_strName; }

	void SetDocument(CAbstractFormDoc* pDocument) { m_pDocument = pDocument; }
	CAbstractFormDoc* GetDocument() { return m_pDocument; }
	
};

class TB_EXPORT CEmptyValidator : public CValidator
{
	DECLARE_DYNCREATE(CEmptyValidator)

public:
	virtual BOOL IsValid(DataObj* pDataObj, CString& message, CDiagnostic::MsgType& msgType);

};




#include "endh.dex"