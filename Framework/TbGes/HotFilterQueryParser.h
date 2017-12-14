
#pragma once


#include "beginh.dex"

class HotFilterRange;
class HotFilterQueries;
class CQueriesCombo;
class QueryObj;
class ProgramData;

typedef CMap<CString, LPCTSTR, DataObj*, DataObj*> QueryParams;

//----------------------------------------------------------------
class TB_EXPORT HotFilterQueryParser : public CObject
{
	DECLARE_DYNAMIC(HotFilterQueryParser)

public:
	HotFilterQueryParser	(HotFilterRange* pHotFilter);
	~HotFilterQueryParser	();

public:
	// if strQueryName empty or "default", switch to "save as" and return the choosen name in strQueryName
	void Save	(CWnd* pParentFrame, DataStr& strQueryName); 
	void SaveAs	(CWnd* pParentFrame, DataStr& strQueryName); 
	// if delete confirmed, set strQueryName to "default"
	BOOL Delete	(DataStr& strQueryName); 
	void Load	(const CString& strQueryName);

	void FillAvailableQueries(CQueriesCombo* pQueryCombo);

private:
	BOOL			AskQuerySaveAs		(CWnd* pParent, CString& strQueryName);
	void			SaveNewQuery		(const CString& strQueryName);
	void			UpdateExistingQuery	(const CString& strQueryName);
	BOOL			FindQuery			(const CString& strQueryName, QueryObj*& pQuery);
	ProgramData*	PrepareProgramData	();

private:
	HotFilterRange*		m_pHotFilter;
	HotFilterQueries*	m_pHFLQueries;
	SqlRecord*			m_pRec;
	SqlTable*			m_pTbl; 
	QueryParams			m_QueryParams;
};

//-----------------------------------------------------------------------------
class TB_EXPORT CQueriesCombo : public CStrCombo
{
	DECLARE_DYNCREATE (CQueriesCombo)

public:
	CQueriesCombo();
	CQueriesCombo(UINT nBtnIDBmp, DataStr* = NULL);

public:
	void AttachQueryParser		(HotFilterQueryParser* pQueryParser)	{ m_pQueryParser = pQueryParser; }

protected:
	virtual	void	OnFillListBox	();

private:
	HotFilterQueryParser*	m_pQueryParser;
};

BEGIN_TB_STRING_MAP(PredefinedQuery)
	TB_LOCALIZED	(Default,	"Default")
END_TB_STRING_MAP()

#include "endh.dex"
