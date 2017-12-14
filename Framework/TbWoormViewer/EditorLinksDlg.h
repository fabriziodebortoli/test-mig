#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGes\FormMngDlg.h>
/*#include <TbGenlib\parsctrl.h>
#include <TbGenlib\ExpParse.h>*/


////per l'editor di expression
//#include <TbWoormEngine\edtcmm.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#ifdef _OUTDATED

enum Mode {MODIFY, NEWCONN};  // dice se si sta creando un nuovo link o modificandone uno esistente


//===========================================================================
class TB_EXPORT CManageLinkDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CManageLinkDlg)
protected:
	CListCtrl			m_listCtrlWhenClauses;
	CWoormDocMng*			m_pDocument;
	WORD				m_nAlias;
	CString				m_strLabelColumn;
	WoormLink*			m_pConn;
	CBCGPComboBox		m_cbxLinkUrlType;

public:
	CManageLinkDlg	(CString strLabelColumn, CWoormDocMng* pDocument, WORD columnID, CWnd* = NULL);
	

protected:
	//CString GetLinkDescription();
	void GetLinkDescription(CString& linkType, CString& strName, CString& strExpr);

	
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CManageLinkDlg)
	afx_msg	void	ChangeRadioSelection		();
	//}}AFX_MSG


    DECLARE_MESSAGE_MAP()
};


//===========================================================================
class TB_EXPORT CParamLinkDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CParamLinkDlg)
protected:
	CWoormDocMng*			m_pDocument;
	WoormLink*			m_pConn;
	int					m_idxLink;

	CEdit				m_editLinkOn;
	CEdit				m_editLinkName;
	CExpEdit			m_editLinkWhen;
	CExpEdit			m_editLinkBefore;
	CExpEdit			m_editLinkAfter;
	CButton				m_btnDeleteLink;
	CButton				m_btnExplorer;
	CButton				m_btnLoadParam;
	CButton				m_btnAddParam;
	CButton				m_btnRemoveParam;
	CButton				m_btnApply;
	CExpEdit			m_editParamExpression;
	CListCtrl			m_listCtrlParams;
	CEdit				m_varInEdit;
	int					m_idxItem;

	// namespace form o report
	CString						m_strDocNamespace;
	 //----LinkType			m_linkType;
	WoormLink::WoormLinkType	m_connType;
	WoormLink::WoormLinkSubType	m_subType;
 	BOOL						m_bLinkTargetByField;
	Mode						m_mode;	//NEW or UPDATE
	
	//per capire su che colonna e' link
	WORD						m_nAlias;
	CString						m_strLabelColumn;
	//SympleSymTable of called report
	WoormTable					m_CalledReportVarSymTable;
	// WoormTable of calling report 
	WoormTable*				m_pRepSymTable;

public:
	CParamLinkDlg
		(
		CWoormDocMng* pDocument, 
		WoormLink::WoormLinkType connType,
		WoormLink::WoormLinkSubType subType, 
		Mode mode, 
		WORD nAlias, 
		CString strLabelColumn, 
		BOOL bIsLinkByVariable = FALSE,
		int idxLink = -1/*pos della connection nel connection array*/, 
		CWnd* = NULL
		);
	
protected:
	BOOL			LoadFormParameters			();
	void			LoadReportParameters		();
	BOOL			LoadFunctionParameters		();
	BOOL			LoadFunctionParameters		(CFunctionDescription* pFunc);
	void			LoadOldLinkParam			();
	void			LoadReportSymTable			();
	void			EnableParameterControls		(BOOL bEnable);
	void			EnableSelectionReport		(BOOL bEnable);
	BOOL			CheckParametersExpression	();
	void			AddConnection				();
	BOOL			CheckNamespace				(CTBNamespace ns);

	//functions to load link object (namespace or string woorm variable)
	void			SelectReportByExplorer		();
	void			SelectDocumentByExplorer	();
	void			SelectObjectByVariable		();
	void			SelectFunctionByDialog		();

	//function to fill control list
	BOOL			InsertIntoCtrlList (DataType itemType, CString itemName , Expression* itemExpr = NULL, const CString& sVal = CString());

	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CManageLinkDlg)
	afx_msg	void	OpenNsSelectionDlg	 ();
	afx_msg	void	LoadConnectionParams ();
	afx_msg	void	AddConnectionParam	 ();
	afx_msg	void	RemoveConnectionParam	 ();
	afx_msg	void	ApplyExprParam		 ();
	afx_msg	void	DeleteLink			 ();
	afx_msg void	OnSelectParam		 (NMHDR*, LRESULT*);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//============================================================================

class TB_EXPORT CSelectReportParamDlg: public CParsedDialog
{
	DECLARE_DYNAMIC(CSelectReportParamDlg)
public:
    CStringArray			m_arStrSelectedVariables;

protected:	
	CResizableListBox		m_lbReportVariables;
	CResizableListBox		m_lbSelectedVariables;
	CButton					m_addParam;
	CButton					m_removeParam;

	CString					m_strReportNS;
	WoormTable*			m_pVarSymTable;
	CStringArray*			m_parVariablesOldSelected;
	

public:
	CSelectReportParamDlg	( 
							CString			strReportNS,
							WoormTable* pVarSymTable,
							CStringArray*	parVariablesOldSelected,
							CWnd* aParent = NULL
						);

	
protected:
	void			CheckSelectionsToAdd (CStringArray& arSelections);
	BOOL			ParamAlreadySelected (CString strVarName);

	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

    //{{AFX_MSG( CSelectReportParamDlg )
	afx_msg void    AddSelection					();
	afx_msg void    RemoveSelection					(); 
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//============================================================================

class TB_EXPORT CSelectVarNsDlg: public CParsedDialog
{
	DECLARE_DYNAMIC(CSelectVarNsDlg)
 private:
    WoormTable*		m_pRepSymTable;
	CResizableListBox	m_lbReportStringVariables;
	CString				m_strSelectedVar;
 
 public:
	 CSelectVarNsDlg (WoormTable*	pRepSymTable, CWnd* aParent = NULL);

	 CString GetSelectedVar();

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

    //{{AFX_MSG( CSelectVarNsDlg )
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================

class TB_EXPORT CAddParamDlg: public CParsedDialog
{
	DECLARE_DYNAMIC(CAddParamDlg)
public:
	CString				m_strNameParam;
	DataType			m_dtParam;

private:
	CIdentifierEdit		m_edtParamName;
	CDataObjTypesCombo	m_cbDataObjTypes;
    
 public:
	 CAddParamDlg (CWnd* aParent = NULL);



protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

    //{{AFX_MSG( CAddParamDlg )
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//===========================================================================
class TB_EXPORT CAllLinksColsDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CAllLinksColsDlg)
protected:
	CColumnArray*		m_pArrayColumnWithLink;
	CTBListBox			m_lbColumns;
    CWoormDocMng*			m_pDocument;
	Table*				m_pTable;

public:
	CAllLinksColsDlg (CColumnArray* pArrayColumnWithLink, CWoormDocMng*	pDocument,Table* pTable, CWnd* = NULL);
	
protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CListHiddenColsDlg)
	afx_msg	void	EditColumnLinks	();
	//}}AFX_MSG


    DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CSelExtFunctDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CSelExtFunctDlg)

	DECLARE_MESSAGE_MAP()

	CResizableComboBox		m_cbSelApplication;
	CResizableComboBox		m_cbSelModule;
	CResizableListBox		m_lbExtFuncts;
	CResizableStrEdit		m_edtShowDescr;

	Array					m_arModItemLoc;
	Array					m_arAppItemLoc;

public:
	CString m_sNamespace;
	CFunctionDescription* m_pFunc;

	CSelExtFunctDlg ();

private:
	afx_msg void	OnComboAppChanged		();
	afx_msg void	OnComboModChanged		();
	afx_msg void	SelChangeExtFunct		();
	afx_msg void	SelExtFunct				();

	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual	void	OnCancel		();

};
#endif
//===========================================================================
#include "endh.dex"
