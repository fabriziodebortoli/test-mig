
#pragma once

#include <TbGeneric\mlistbox.h>
#include <TbGenlib\parsctrl.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class QueryData;
class CMicroBar;
class CMicroEntry;
class WoormTable;
class WoormField;
class Expression;
class EditorManager;
class RuleDataArray;
class DataType;
class EventFunction;

//============================================================================
enum RepFieldType;	

//============================================================================
class TB_EXPORT CExpressionEdit : public CExpEdit
{
protected:
	Parser*			m_pLex;
	WoormField*	m_pRepField;
	BOOL			m_bMustCheck;

public:
	CExpressionEdit	(WoormTable*, RuleDataArray*);
	virtual ~CExpressionEdit	();       

public:
	virtual	BOOL	ErrorMessage	();

			void	Attach			(WoormField*);

protected:
	//{{AFX_MSG( CExpressionEdit )
	afx_msg void	OnLButtonDblClk	(UINT, CPoint);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CFuncEdit : public CExpressionEdit
{
public:
	CFuncEdit	(WoormTable*, RuleDataArray*);

public:
	virtual	BOOL	IsValid	();

private:
	BOOL	CheckFunctionData(EventFunction*);

protected:
	//{{AFX_MSG( CFuncEdit )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CInitEdit : public CExpressionEdit
{
public:
	CInitEdit	(WoormTable*, RuleDataArray*);

public:
	virtual	BOOL	IsValid	();

protected:
	//{{AFX_MSG( CInitEdit )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CReportSymTableDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CReportSymTableDlg)
protected:
	WoormTable*		m_pSymTable;
	EditorManager*		m_pEdtMng;
	BOOL				m_bFieldNormal;
	BOOL				m_bFieldColumn;
	BOOL				m_bFieldInput;
	BOOL				m_bFieldVisible;
	BOOL				m_bFieldHidden;
	BOOL				m_bFieldAll;

	CTBListBox			m_lbRepField;
	CIntEdit			m_edtRepFieldLen;
	CIntEdit			m_edtRepFieldPrec;
	CFuncEdit			m_edtRepFieldFunc;
	CInitEdit			m_edtRepFieldInit;
	CDataObjTypesCombo	m_cbDataObjTypes;
	CIdentifierEdit		m_edtRepFieldAliasName;

public:
	CReportSymTableDlg(EditorManager*);

protected:
	void 			ShowFieldData		(WoormField*);
	void			ShowFunctionData	(CEdit&, EventFunction*);
	void 			UpdateFieldsList	();

	void			DataFunctionAction	();

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CReportSymTableDlg )
	afx_msg	void	SelectRepFieldAction	();
	afx_msg	void	ModifyRule				();
	afx_msg	void	FieldLenChangedAction	();
	afx_msg	void	FieldPrecChangedAction	();
	afx_msg	void	FieldTypeChangedAction	();
	afx_msg	void	DeleteFieldAction		();
	afx_msg	void	FieldXmlAliasChangedAction();
	afx_msg	void	FieldNoXmlCheckChangedAction();

	afx_msg	void	FieldSetInputChangedAction	();
	afx_msg	void	FieldSetReinitChangedAction	();
	afx_msg	void	FieldSetStaticChangedAction	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CVarFieldDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CVarFieldDlg)
protected:
	WoormTable*		m_pSymTable;
	WoormField*		m_pRepField;
	WoormField*		m_pColRepField;
    BOOL				m_bIsNewField;

	CIdentifierEdit		m_edtRepFieldName;
	CIntEdit			m_edtRepFieldLen;
	CIntEdit			m_edtRepFieldPrec;
	CFuncEdit			m_edtRepFieldFunc;
	CInitEdit			m_edtRepFieldInit;
	CButton				m_btnHiddenField;
	CDataObjTypesCombo	m_cbDataObjTypes;
	CBoolButton			m_chkArray;

	DispTableInfo*		m_pDispTableInfo;

public:
	CVarFieldDlg	
		(
			WoormTable*	pSymTable,
			RuleDataArray*		pRuleData,
			WoormField*	pRepField,
	        WoormField*	pColumnRepField,
			BOOL			bIsNew,
			DispTableInfo*	pDispTableInfo = NULL
		);

protected:
	void			ShowFieldData		();
	void			ShowFunctionData	(CEdit&, EventFunction*);

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CVarFieldDlg )
	afx_msg	void	FieldNameChanged	();
	afx_msg	void	DataTypeChanged		();
	afx_msg	void	OnChkArrayChanged	();

	afx_msg	void	FieldSetHiddenChangedAction	();
	afx_msg	void	FieldSetInputChangedAction	();
	afx_msg	void	FieldSetReinitChangedAction	();
	afx_msg	void	FieldSetStaticChangedAction	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CSelFieldDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CSelFieldDlg)
protected:
	WoormTable*	m_pSymTable;
	CWordArray&		m_IdArray;
	BOOL			m_bOnlyOneField;

	CMultiListBox	m_mlbFields;

	DispTableInfo*	m_pDispTableInfo;

public:
	CSelFieldDlg
		(
			WoormTable*	pSymTable,
			CWordArray&		IdArray,
			BOOL			bOneField,
			DispTableInfo*	pDispTableInfo = NULL
		);

protected:
	virtual BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual	void	OnCancel		();

	//{{AFX_MSG( CSelFieldDlg )
	afx_msg	void	FieldTypeChangedAction	();
	afx_msg	void	SelectField				();
	afx_msg	void	ShowFieldList			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
#include "endh.dex"
