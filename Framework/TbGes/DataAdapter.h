#pragma once

#include "afx.h"
#include <TbGeneric\DataObj.h>

#include "beginh.dex"


class CAbstractFormDoc;
class TB_EXPORT IDataAdapter
{
public:
	virtual	BOOL	ChangeValue(DataObj* pDataObj) = 0;
};

class TB_EXPORT CDataAdapter : public CObject, public IDataAdapter
{
	DECLARE_DYNAMIC(CDataAdapter)
protected:
	CString m_strName;
	CAbstractFormDoc* m_pDocument;
public:
	CDataAdapter();
	virtual ~CDataAdapter();

	void SetName(const CString& strName) { m_strName = strName; }
	CString GetName() { return m_strName; }

	void SetDocument(CAbstractFormDoc* pDocument) { m_pDocument = pDocument; }
	CAbstractFormDoc* GetDocument() { return m_pDocument; }
};




#include "endh.dex"