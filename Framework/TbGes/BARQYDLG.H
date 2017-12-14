
#pragma once


#include <TbGenlib\parsobj.h>
#include <TbGenlib\parsctrl.h>

#include "barquery.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class SqlConnection;
class CQueriesListBox;

class CBarQueriesDlg;
class CBarQueryBaseDlg;
		class CBarQueryRenameDlg;
		class CBarQueryBaseEditModeDlg;
			class CBarQueryModifyDlg;
			class CBarQueryNewDlg;


// Definizione di CBarQueryBaseDlg
//=============================================================================
class TB_EXPORT CBarQueryBaseDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CBarQueryBaseDlg)
protected:
	CStrEdit*			m_pStrEdtName;
	AllQueries*			m_pAllQueries;
	CQueriesListBox*	m_pQueriesList;

public:
	CBarQueryBaseDlg	(UINT, CQueriesListBox*, AllQueries*);
	~CBarQueryBaseDlg	();

public:
	CQueriesListBox*	GetQueriesList	();
	AllQueries*			GetAllQueries	();

			QueryObj*	GetCurSelQueryObj	();
	const	CString&	GetCurSelQueryName	();
			CString		GetNewName			();

	void BtnOkEnableWindow (BOOL bEnable = TRUE);

public:
	BOOL TestQueryName ();

protected:
	virtual BOOL OnInitDialog ();

	//{{AFX_MSG(CBarQueryBaseDlg)
		afx_msg void OnEdtChange() {BtnOkEnableWindow ();}
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG
};

// Definizione di CBarQueryRenameDlg
//============================================================================= 
class TB_EXPORT CBarQueryRenameDlg : public CBarQueryBaseDlg
{
public:
	CBarQueryRenameDlg (CQueriesListBox*, AllQueries*);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBarQueryBaseDlg::AssertValid(); }
#endif // _DEBUG
};

// Definizione di CBarQueryBaseEditModeDlg
//=============================================================================
class TB_EXPORT CBarQueryBaseEditModeDlg : public CBarQueryBaseDlg
{
protected:
	BOOL m_bInsert;
	BOOL m_bPrgDataOwns;

	SqlConnection*	m_pSqlConnection;
	CExpEdit*		m_pEdtExpWClause;
	CEqnEdit*		m_pEdtOrderBy;

	ProgramData*	m_pPrgData;

public:
	CBarQueryBaseEditModeDlg	(UINT nIDD, SqlConnection*, CQueriesListBox*, AllQueries*);
	~CBarQueryBaseEditModeDlg	();

	void BtnTestEnableWindow	(BOOL bEnable = TRUE);
	void CovertStrOrderByInLF	(CString&);
	void InitForModify			(QueryObj*);

	CButton* GetChkBoxDefault	();
	CButton* GetChkBoxNativeExpr();

	BOOL IsDefaultQuery	()	{ return GetChkBoxDefault()->GetCheck(); }
	BOOL IsNativeExpr	()	{ return GetChkBoxNativeExpr()->GetCheck(); }
	BOOL TestQuery		(CString* = NULL);

protected:
	virtual void OnOK			() = 0;
	virtual BOOL OnInitDialog	();

	//{{AFX_MSG(CBarQueryBaseEditModeDlg)
		afx_msg void OnTest			() { TestQuery (); }
		afx_msg void ShowAskRules	();
		afx_msg void OnEnableBtnTest() { BtnTestEnableWindow(); }
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBarQueryBaseDlg::AssertValid(); }
#endif // _DEBUG
};

// Definizione di CBarQueryNewDlg
//=============================================================================
class TB_EXPORT CBarQueryNewDlg : public CBarQueryBaseEditModeDlg
{
protected:
	BOOL m_bFromModify;

public:
	CBarQueryNewDlg	(SqlConnection*, CQueriesListBox*, AllQueries*, BOOL bFromModify = FALSE);

protected:
	virtual void OnOK			();
	virtual BOOL OnInitDialog	();

	//{{AFX_MSG(CBarQueryNewDlg)
		afx_msg void OnQueryNameKillFocus ();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBarQueryBaseEditModeDlg::AssertValid(); }
#endif // _DEBUG
};

// Definizione di CBarQueryModifyDlg
//=============================================================================
class TB_EXPORT CBarQueryModifyDlg : public CBarQueryBaseEditModeDlg
{
public:
	CBarQueryModifyDlg	(SqlConnection*, CQueriesListBox*, AllQueries*);

protected:
	virtual void OnOK			();
	virtual BOOL OnInitDialog	();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBarQueryBaseEditModeDlg::AssertValid(); }
#endif // _DEBUG
};

// Definizione di CBarQueriesDlg
//=============================================================================
class TB_EXPORT CBarQueriesDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CBarQueriesDlg)
protected:
	CQueriesListBox*	m_pLBQueries;
	SqlConnection*		m_pSqlConnection;
	BOOL m_bOnlyExec;

public:
	BOOL m_bModified;

	AllQueries*	m_pAllQueries;

public:
	CBarQueriesDlg	(AllQueries*, BOOL bOnlyExec = FALSE);
	~CBarQueriesDlg	();

protected:
	BOOL IsQuerySelect	();

	BOOL CanDoModify	();
	BOOL CanDoDelete	();
	BOOL CanDoRename	();
	BOOL CanDoNew		();

	BOOL EnableModify	(BOOL bEnable = TRUE);
	BOOL EnableDelete	(BOOL bEnable = TRUE);
	BOOL EnableRename	(BOOL bEnable = TRUE);
	BOOL EnableNew		(BOOL bEnable = TRUE);

	BOOL EnableButton (int nIDC, BOOL bEnable = TRUE);

	virtual BOOL OnInitDialog ();

	//{{AFX_MSG(CBarQueriesDlg)
		afx_msg void OnQuerySelChange	();
		afx_msg void OnModify			();
		afx_msg void OnExec				();
		afx_msg void OnNew				();
		afx_msg void OnRename			();
		afx_msg void OnDelete			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG
};

// Definizione della listbox con bitmap
//=============================================================================
class TB_EXPORT CQueriesListBox : public CBCGPListBox
{
protected:
	int m_nBmpHeight;
	int m_nBmpWidth;

	CBitmap m_bmpTemp;
	CBitmap m_bmpDefault;
	CBitmap m_bmpPredefinite;
	CBitmap m_bmpOnFile;
	CBitmap m_bmpCerca;
	CFont*	m_pFont;

public:
	CQueriesListBox	();
	~CQueriesListBox();

public:
	BOOL SubclassDlgItem (UINT, CWnd*);

	int		AddQuery			(QueryObj*);
	int		FindQueryName		(const CString&);
	void	DeleteCurSelQuery	();
	void	RenameCurSelQuery 	();
	void	SelectQueryName		(const CString&);

	QueryObj* GetCurSelQueryObj ();
	QueryObj* GetQueryObj		(int nIdx);

protected:
	CBarQueriesDlg*	GetParent		() { return (CBarQueriesDlg*)CBCGPListBox::GetParent(); }
	AllQueries*		GetAllQueries	() { return GetParent()->m_pAllQueries; }

	CBitmap* GetQueryObjBitmap (QueryObj*);
	virtual void OnDrawItemContent(CDC* pDC, CRect rect, int nIndex);
	virtual void MeasureItem(LPMEASUREITEMSTRUCT);

// Diagnostics
#ifdef _DEBUG
public:                 
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const {	CBCGPListBox::AssertValid(); }
#endif // _DEBUG
};

//-----------------------------------------------------------------------------
inline BOOL CQueriesListBox::FindQueryName (const CString& strQueryName)
	{ return FindStringExact (-1, strQueryName) < 0 ? FALSE : TRUE; }
inline void CQueriesListBox::SelectQueryName (const CString& strQueryName)
	{ SelectString (-1, strQueryName); }

inline QueryObj* CQueriesListBox::GetQueryObj (int nIdx)
	{ return (QueryObj*) GetItemDataPtr (nIdx);}
inline QueryObj* CQueriesListBox::GetCurSelQueryObj ()
	{ return GetQueryObj (GetCurSel());}

//-----------------------------------------------------------------------------
inline AllQueries* CBarQueryBaseDlg::GetAllQueries	()
	{ ASSERT_VALID (m_pAllQueries); return  m_pAllQueries; }
inline CQueriesListBox* CBarQueryBaseDlg::GetQueriesList ()
	{ ASSERT_VALID (m_pQueriesList); return  m_pQueriesList; }
inline QueryObj* CBarQueryBaseDlg::GetCurSelQueryObj ()
	{ return  GetQueriesList ()->GetCurSelQueryObj (); }
inline const CString& CBarQueryBaseDlg::GetCurSelQueryName ()
	{ return  GetCurSelQueryObj ()->m_strQueryName; }

//----------------------------------------------------------------------------
inline BOOL CBarQueriesDlg::CanDoModify	() { return !m_bOnlyExec && IsQuerySelect(); }
inline BOOL CBarQueriesDlg::CanDoDelete () { return !m_bOnlyExec && IsQuerySelect(); }
inline BOOL CBarQueriesDlg::CanDoRename () { return !m_bOnlyExec && IsQuerySelect(); }
inline BOOL CBarQueriesDlg::CanDoNew	() { return !m_bOnlyExec; }

#include "endh.dex"
