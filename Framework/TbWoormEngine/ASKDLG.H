#pragma once

#include <TbGeneric\DataObj.h>

#include <TbGenlib\parsctrl.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class WoormTable;
class Parser;
class AskRuleData;
class AskDialogData;
class AskGroupData;
class AskFieldData;
class ProgramData;
class RuleDataArray;

//============================================================================
// costanti stringa
BEGIN_TB_STRING_MAP(AskDlgStrings)
	TB_LOCALIZED(DEFAULT,	"Default")
	TB_LOCALIZED(LEFT,		"Left")
	TB_LOCALIZED(UP,		"Top")
END_TB_STRING_MAP()


//============================================================================
class TB_EXPORT CAskRuleDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CAskRuleDlg)
protected:
	ProgramData*	m_pPrgData;

	AskRuleData*	m_pAskRule;
	WoormTable*		m_pSymTable;
	CBaseDocument*	m_pDocument;

	CBCGPListBox		m_lbDialog;
	CBCGPListBox		m_lbGroup;
	CBCGPListBox		m_lbField;

	//CButton			m_btnAddDialog;
	//CButton			m_btnModDialog;
	//CButton			m_btnDelDialog;

	//CButton			m_btnAddGroup;
	//CButton			m_btnModGroup;
	//CButton			m_btnDelGroup;

	//CButton			m_btnAddField;
	//CButton			m_btnModField;
	//CButton			m_btnDelField;

	CBCGPButton		m_btnPreView;
	
	AskDialogData*	m_pCurAskDialog;
	AskGroupData*	m_pCurAskGroup;
	AskFieldData*	m_pCurAskField;
	
	BOOL			m_bModified;

public:
	CAskRuleDlg (ProgramData* prgData, CBaseDocument *pDocument);

protected:
	BOOL 			CanDeleteDialog		(AskDialogData*);
	BOOL			CanDeleteGroup		(AskGroupData*);
	BOOL			CanDeleteField		(AskFieldData*);
	void			ShowAskDialog		();
	void			ShowAskGroup		();
	void			ShowAskField		();
	BOOL			AskConfirm			(const CString&);
	void			EnableBtnPreView	();
	void			AddDefaultDialog	();
	void			AddDefaultGroup		();

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CAskRuleDlg )
	afx_msg	void	OnLbnDialog			();
	afx_msg	void	OnLbnGroup			();
	afx_msg	void	OnLbnField			();

	afx_msg	void	OnBtnAddDialog		();
	afx_msg	void	OnBtnModDialog		();
	afx_msg	void	OnBtnDelDialog		();
	afx_msg	void	OnBtnDialogUp		();
	afx_msg	void	OnBtnDialogDown		();

	afx_msg	void	OnBtnAddGroup		();
	afx_msg	void	OnBtnModGroup		();
	afx_msg	void	OnBtnDelGroup		();
	afx_msg	void	OnBtnGroupUp		();
	afx_msg	void	OnBtnGroupDown		();

	afx_msg	void	OnBtnAddField		();
	afx_msg	void	OnBtnModField		();
	afx_msg	void	OnBtnDelField		();
	afx_msg	void	OnBtnFieldUp		();
	afx_msg	void	OnBtnFieldDown		();

	afx_msg	void	OnBtnPreView		();
	afx_msg	void	OnBtnHelpFilePath	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CAskDialogDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CAskDialogDlg)
protected:
	WoormTable*	m_pSymTable;
	AskDialogData*	m_pAskDialog;
	AskRuleData*	m_pAskRule;
    BOOL			m_bIsNewDialog;
    
	CIdentifierEdit	m_edtDialogName;
	CEdit			m_edtDialogTitle;
	CButton			m_btnOnAsk;
	CBCGPComboBox	m_cbCaptionPos;

	CExpEdit		m_edtWhenExpr;
	CExpEdit		m_edtOnExpr;
	CExpEdit		m_edtAbortExpr;
	CExpEdit		m_edtBeforeBlock;
	CExpEdit		m_edtAfterBlock;

public:
	CAskDialogDlg
		(
			AskDialogData*	bAskDialogData,
			AskRuleData*	bAskRuleData,
			BOOL			bNewDialog
		);

protected:
	virtual BOOL	OnInitDialog	();
	virtual void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG( CAskDialogDlg )
	afx_msg	void	OnAskClicked		();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CAskGroupDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CAskGroupDlg)
protected:
	AskGroupData*	m_pAskGroup;
	AskDialogData*	m_pAskDialog;
	WoormTable* m_pSymTable;
	BOOL			m_bIsNewGroup;

	CEdit			m_edtGroupTitle;
	CBCGPComboBox	m_cbCaptionPos;
	CExpEdit		m_edtAskGroupWhen;

public:
	CAskGroupDlg
		(
			AskGroupData*	bAskGroupData,
			AskDialogData*	bAskDialogData,
			RuleDataArray*		pRuleData,
			WoormTable* pSymTable,
			BOOL			bNewDialog
		);

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CAskGroupDlg )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CAskFieldDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CAskFieldDlg)
public:
	// costanti stringa
	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(ALL_GROUPS, "<All groups>")
		TB_LOCALIZED(NO_GROUP, "<None>")
	END_TB_STRING_MAP()

protected:
	AskFieldData*		m_pAskField;
	WoormTable*			m_pSymTable;
	BOOL				m_bIsNewField;

	CIdentifierEdit		m_edtAskFieldName;
	CDataObjTypesCombo	m_cbDataObjTypes;

    CEdit				m_edtAskFieldCaption;
	CButton				m_btnAskFieldStaticCaption;
	CButton				m_btnAskFieldDynamicCaption;
	CExpEdit			m_edtAskFieldDynamicCaption;

	CBCGPComboBox		m_cbCaptionPos;
	CIntEdit			m_edtAskFieldLen;
	CIntEdit			m_edtAskFieldPrec;
	CBCGPComboBox		m_cbCtrlStyle;

	CButton				m_btnAskFieldLowerLimit;
	CButton				m_btnAskFieldUpperLimit;

	CButton				m_btnAskFieldCheckRadioOnLeft;
	CButton				m_btnAskFieldCheckRadioTextOnLeft;

	CExpEdit			m_edtAskFieldDefault;
	CExpEdit			m_edtAskFieldReadOnly;

	CExpEdit			m_edtAskFieldWhen;
	CButton				m_btnAskFieldDynamicHidden;

	CComboBoxExt		m_cbHotLinkGroups;
	CComboBoxExt		m_cbHotLinkNames;
	CEqnEdit			m_edtHotLinkParams;
	CTBNamespace		m_nsHotLink;
	AddOnModule*		m_pHLAddOnModule;
	BOOL				m_bHLHasCombo;
	CButton				m_btnShowHotLinkDescription;

	CButton				m_btnAskFieldAlwaysReInit;
	CButton				m_btnAskFieldNeverReInit;

	CButton				m_btnAskUseDescriptionCombo;
	CButton				m_btnAskUseMultiSelectionCombo;

public:
	CAskFieldDlg
		(
			AskFieldData*	pAskFieldData,
			WoormTable*		pSymTable,
			RuleDataArray*	pRuleData,
			BOOL			bNewField
		);

protected:
	BOOL	ReadHotlink				();
	BOOL	ShowAskFieldData		();
	void	ShowHotlinkGroups		();
	void	ShowCurrentHotlinkGroup	();
	void	ShowCurrentHotlink		();

private:
	void	SetCurrentCtrlStyle		(const int& nCtrlStyle);
	BOOL	SetHotLinkNamespace		(const CTBNamespace&);
	void	DoCtrlStyleChanged		();

protected:
	virtual BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual	void	OnCancel		();

	//{{AFX_MSG( CAskFieldDlg )
	afx_msg	void	PublicNameChanged	();
	afx_msg	void	EnableControls		();
	afx_msg	void	DataTypeChanged		();
	afx_msg	void	LowerLimitChanged	();
	afx_msg	void	UpperLimitChanged	();
	afx_msg	void	CtrlStyleChanged	();
	afx_msg	void	HotLinkGroupChanged	();
	afx_msg	void	HotLinkNameChanged	();
	afx_msg	void	OnDataLengthChanged	();
	afx_msg	void	OnBtnDynamicCaption	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
#include "endh.dex"
