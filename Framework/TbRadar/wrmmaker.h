
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTable;
class CAbstractFormDoc;

//	Definizione di CWrmMaker
//=============================================================================
class TB_EXPORT CWrmMaker : public CObject
{
protected:
	CString				m_strFilter;
	CString				m_strOrderBy;
	CString				m_strFindFilter;
	CString				m_strLoadFindParameters;
	SqlTable*			m_pSqlTable;
	SqlRecord*			m_pSqlRecord;
	CAbstractFormDoc*	m_pDocument;
	int					m_nAliasCount;
	CString				m_strFirstKeySegName;
	int					m_nTableColumnCount;

public:
	BOOL				m_bUnparseOnString;
	CString				m_strReportName;

public:
			CWrmMaker	(CAbstractFormDoc*);
	virtual ~CWrmMaker	();

public:
	virtual BOOL BuildWoorm		(BOOL bUnparseOnString = FALSE);
	void DeleteReport	();

protected:
	virtual void WriteReport		(CLineFile&);

	void CR			(CLineFile&);
	void WS			(CLineFile&, const CString& strMask);
	void TT			(CLineFile&, const CString& strMask);
	void PI			(CLineFile&, const CString& strMask1);
	void TF			(CLineFile&, const CString& strMask);
		void TF1	(SqlRecord*, CLineFile&, const CString&);
	void RT			(CLineFile&, const CString& strMask);
	void RV			(CLineFile&, const CString& strMask);
		void RV1	(SqlRecord*, int&, CLineFile&, const CString&);
	void RN			(CLineFile&, const CString& strMask);
	void RS			(CLineFile&, const CString& strMask);
		void RS1	(SqlRecord*, CString, CLineFile&, const CString&);
	void SL			(CLineFile&, const CString& strMask);
	void KY			(CLineFile&, const CString& strMask);
	void WO			(CLineFile&, const CString& strMask);
	void WW			(CLineFile&, const CString& strMask);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};


#include "endh.dex"
