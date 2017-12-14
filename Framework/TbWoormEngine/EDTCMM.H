
#pragma once

#include <TbNameSolver\Chars.h>
#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsctrl.h>

#include <TbOledb\sqlcatalog.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class Expression;
class WClause;
class Parser;
class WoormTable;
class SqlTableInfo;
class RuleDataArray;
class TblRuleData;
class CBCGPStatusBar;

typedef bool (CObject::*CHECK_EXPRESSION) (const CString& strOutExpression, CString& strError);
//============================================================================
enum TB_EXPORT ConversType { LF_TO_CRLF, CRLF_TO_LF };

//============================================================================
// useful string converters
TB_EXPORT void ConvertCString (CString& str, ConversType conversType);

//============================================================================
// genearte a name from an existing name with a prefix
//
TB_EXPORT CString GenerateName(SymTable* pSymTable, LPCTSTR strFromName, const CString&);

typedef bool (*DOEXPRESSIONEDITORFUNCTION)(
	const SqlTableInfoArray* pArTableInfo, 
	const SymTable* pSymTable, 
	CString& strOutExpression,  
	bool onlyQueryLanguageItems,
	CObject* pObject, 
	CHECK_EXPRESSION lpCheckExpression);

TB_EXPORT void SetExpressionEditorFunction(DOEXPRESSIONEDITORFUNCTION ptr);
TB_EXPORT void AddEventActionsKeywords(CStringArray& arKeywords);

//============================================================================
// general check identifier
//
TB_EXPORT CString	GetIdentifier			(CComboBox& owner);
TB_EXPORT BOOL 	GetIdentifier			(CComboBox& owner, CString& strName);
TB_EXPORT BOOL	IsNumericDataObj		(const DataType&);

//============================================================================
class TB_EXPORT CEqnEdit : public CResizableStrEdit
{
	DECLARE_DYNCREATE (CEqnEdit)

protected:
	SymTable*		m_pSymTable;
	TblRuleData*	m_pTblRule;
	RuleDataArray*		m_pRuleData;
	SqlTableInfoArray	m_arSqlTableInfo;
	DWORD			m_dwPrevCaretPos;
	CStatusBar*		m_psbParentDialog;
	CHECK_EXPRESSION m_pCheckExpression;
	CObject*			m_pCheckObject;
public:
	CEqnEdit();
	CEqnEdit(SymTable*, RuleDataArray*);
	~CEqnEdit();

public:
	void	SetSymbolTable	(SymTable* pSymTable) { m_pSymTable = pSymTable; }
	void	SetTableInfo	(SqlTableInfo*, TblRuleData* = NULL);
	void	SetTableInfo	(SqlTableInfoArray, TblRuleData* = NULL);	
	void	SetDlgStatusBar	(CStatusBar* psbParentDialog);
	BOOL	IsEmpty			();
	void	SetCheckMethod	(CObject *pCheckObject, CHECK_EXPRESSION pCheckExpression) { m_pCheckObject = pCheckObject; m_pCheckExpression = pCheckExpression; }
protected:
	// Overridables
	virtual	void	OnScrollCaret	(UINT nChar);
	virtual	BOOL	OnInitCtrl		();

protected:
	//{{AFX_MSG( EqnEdit )
		afx_msg	void OnLButtonDblClk(UINT, CPoint);
		afx_msg	void OnKeyDown		(UINT nChar, UINT nRepCnt, UINT nFlags);
		afx_msg	void OnKeyUp		(UINT nChar, UINT nRepCnt, UINT nFlags);
		afx_msg	void OnLButtonDown	(UINT nFlags, CPoint point);
		afx_msg	void OnLButtonUp	(UINT nFlags, CPoint point);
		afx_msg	void OnSetFocus		(CWnd* pWnd);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CExpEdit : public CEqnEdit
{
	DECLARE_DYNCREATE (CExpEdit)

public:
	CExpEdit	();
	CExpEdit	(SymTable*, RuleDataArray* = NULL);
	~CExpEdit	();

public:
	BOOL CheckWC	(WClause&);
	BOOL CheckExp	(Expression&, const DataType&, BOOL bCanBeEmpty = FALSE);
	BOOL CheckExp	(Parser&, Expression&, const DataType&, BOOL bCanBeEmpty = FALSE);
	BOOL CheckExp	(const DataType&);

protected:
	void InsertInCString	(CString& strTarget, LPCTSTR str, int nPos, int nShift = 0);
	void ChangePublicNames	(CString&, BOOL bNative);
};

//=============================================================================
class TB_EXPORT DispTableInfo
{
	friend class EditorManager;

protected:
	EditorManager*	m_pEditorManager;
	CString			m_strDispTableName;
	int				m_nRowsNum;
	CIdentifierEdit	m_edtDispTableName;
	CIntEdit		m_edtRowsNum;

public:
	DispTableInfo	(EditorManager*);

	void	OnInitDialog		(CDialog*, BOOL bShow = TRUE);
	BOOL	CheckControls		();
	void	ShowControls		(CDialog*, BOOL bShow = TRUE);
	BOOL	AreControlsVisible	();
	void	SetDispTableName	(LPCTSTR);
	CString	GetEditDispTableName();
};

#include "endh.dex"
