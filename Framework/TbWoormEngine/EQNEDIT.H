#pragma once

#include <TbGenlib\parsobj.h>
#include <TbGenlib\parsedt.h>
#include <TbGenlib\parslbx.h>
#include <TbGenlib\ExtStatusControlBar.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CWnd;
class WoormTable;
class SqlTableInfo;
class RuleDataArray;
class TblRuleData;
class WoormField;

//===========================================================================
class CEqnItemNoLoc : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;

public: 
	CEqnItemNoLoc (CString strName);
};

//===========================================================================
class TB_EXPORT CEqnEditDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CEqnEditDlg)
private:        
	CResizableStrEdit		m_edtExpr;

	CComboBoxExt			m_cbSelFields;
	CComboBoxExt			m_cbSelTable;
	CTBListBox				m_lbEventActions;
	CDataObjTypesCombo		m_cbTagName;
	CResizableListCtrl		m_lbItemName;
	CResizableListBox		m_lbFuncts;
	CResizableListBox		m_lbExtFuncts;
	CResizableListBox		m_lbVars;
	CResizableMultiListBox	m_lbPhisicFields;
	
	CTaskBuilderStatusBar	m_sbInfo;

	CButton					m_ck_viewLong;
	CResizableComboBox		m_cbSelApplication;
	CResizableComboBox		m_cbSelModule;
	CResizableStrEdit		m_edtShowDescr;
	CComboBoxExt			m_cbFilterSelFields;

	SymTable*			    m_pSymTable;
	CString&				m_strExpr;
	DWORD					m_dwStartPos;                   
	SqlTableInfoArray		m_arSqlTableInfo;	
	TblRuleData*			m_pTblRule;
	CObArray				m_Rules;
	Array					m_arAppItemLoc;
	Array					m_arModItemLoc;
	CTBNamespace			m_Ns;
	BOOL					m_bOnlyPubNames;

public:
	CEqnEditDlg	(
				  CWnd*				pParent, 
				  SymTable*			pSymTable, 
				  CString& 			strToEdit, 
				  DWORD				dwPosSel, 
				  RuleDataArray*			pRuleData,
				  TblRuleData*		pTblRule,
				  SqlTableInfoArray arSqlTableInfo
				);

	CEqnEditDlg	(
				  CWnd*				pParent, 
				  SymTable*			pSymTable, 
				  CString& 			strToEdit, 
				  DWORD				dwPosSel, 
				  RuleDataArray*			pRuleData,
				  TblRuleData*		pTblRule,
				  SqlTableInfo*		pSqlTableInfo
				);
public:
	static void ShowFunctions	(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts);
	static void ShowModules		(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts, Array& arModItemLoc);
	static void	FillExtFuncList	(CResizableListBox& lbExtFuncts, AddOnModule* pAddOnMod);
	static BOOL ExistExtFuncList(AddOnModule* pAddOnMod);	
	static void FillModuleCombo	(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts, Array& arModItemLoc, const CString& strLabel = _TB(""));
	static void	GetAppModSel	(CResizableComboBox& m_cbSelApplication, CResizableComboBox& m_cbSelModule, CString& sApplication, CString& sModule);
	static CString SelChangeExtFunct	(CResizableListBox& lbExtFuncts);
	static void FillAppCombo	(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, Array& arAppItemLoc);

private:        
	void		ShowBaseFunctions	();
	void		ShowPhysicalNames	();
	void		ShowComboBoxes		();
	void		ShowItemName		();
	void		ShowEventActions	();
	void		InitStatusBar		();
	void		BtnClk				();
	BOOL		FieldInCurTableRule	(WoormField*);
	BOOL		IsFieldApplicable	(SymField*);
	BOOL		BelongsToDataTable	(WoormField*);
	void		SetCaptionButtons	();
	CString		GetTableName		() const;

protected:        
	virtual void	RecalcLayout	();
	virtual	BOOL	OnInitDialog	();
	virtual	BOOL	OnCommand		(WPARAM wParam, LPARAM lParam);
	virtual	void	OnOK			();

	//{{AFX_MSG( CEqnEditDlg )
	afx_msg	void	ShowVars			();
	afx_msg void	SelFunct			();
	afx_msg void	SelExtFunct			();
	afx_msg void	SelVar				();
	afx_msg void	SelPhysicalFields	();
	afx_msg void	EnumSetItemChanged	(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void	SelEnumSetItem		(NMHDR *pNMHDR, LRESULT *pResult);
			BOOL	SelDataType			(DataType dt);

	afx_msg void	VarFieldChanged		();
	afx_msg void	TagNameChanged		();
	afx_msg	void	TypeChecked			();
	afx_msg void	SelChangeExtFunct	();
	afx_msg void	SelChangeFunct		();
	afx_msg void	SelChangePhysicalFields	();
	afx_msg void	SelChangeActionEvents	();
	afx_msg void	SelActionEvents			();
	afx_msg void	OnClickedCheckLong		();
	afx_msg void	OnComboAppChanged		();
	afx_msg void	OnComboModChanged		();
	afx_msg void	OnRunEnumsViewer		();

	afx_msg void	OnSize (UINT nType, int cx, int cy);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
public:
	virtual INT_PTR OnToolHitTest(CPoint point, TOOLINFO* pTI) const;
};

//-----------------------------------------------------------------------------
#include "endh.dex"
