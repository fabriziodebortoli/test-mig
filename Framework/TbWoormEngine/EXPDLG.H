
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsctrl.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ExpRuleData;
class WoormTable;
class RuleDataArray;
class WoormField;

//===========================================================================
class TB_EXPORT CExpRuleDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CExpRuleDlg)

protected:
	ExpRuleData*		m_pExpRule;
	WoormTable*		m_pSymTable;
	WoormField*		m_pRepFieldData;
	BOOL				m_bItIsNewExpr;

	CExpEdit			m_edtExpr;
	CExpEdit			m_edtFilter;
	CIdentifierEdit		m_edtRepFieldName;
	CDataObjTypesCombo	m_cbDataObjTypes;
	CIntEdit			m_edtRepFieldLen;
	CIntEdit			m_edtRepFieldPrec;
	CBoolButton			m_chkArray;
	DispTableInfo*		m_pDispTableInfo;
public:
	CBaseDocument*		m_pDocument;

public:
	CExpRuleDlg
		(
			ExpRuleData* 	pExpRule,
			WoormTable* pSymTable,
			RuleDataArray*		pRuleData,
			WoormField*	pFieldData = NULL,
			DispTableInfo*	pDispTableInfo = NULL,
			CBaseDocument*	pDocument	= NULL
		);

protected:
	void			ShowData			();
	BOOL			CheckExprOK			();

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CExpRuleDlg )                                                    
	afx_msg	void	DataTypeChanged		();
	afx_msg	void	PublicNameChanged	();
	afx_msg	void	OnChkArrayChanged	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

BEGIN_TB_STRING_MAP(WoormDialogStrings)
	TB_LOCALIZED(FIELD, "Field_{0-%d}")
END_TB_STRING_MAP()

#include "endh.dex"
