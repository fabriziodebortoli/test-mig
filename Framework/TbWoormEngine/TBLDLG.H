
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\dataobj.h>
#include <TbGeneric\mlistbox.h>
#include <TbGenlib\ExtStatusControlBar.h>

#include "edtcmm.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class TblRuleData;
class WoormTable;
class CMicroBar;
class CMicroEntry;
class SqlTableInfo;
class Path;
class WClause;
class RuleDataArray;
class SqlConnection;

///////////////////////////////////////////////////////////////////////////////
//								PublicFieldObj
///////////////////////////////////////////////////////////////////////////////
//
class PublicFieldObj : public CObject
{
public:
	enum SelectionType { SELECTED, HIDDEN, OLD_SELECTED, OLD_HIDDEN, BAD_TYPE, NONE };

public:
	CString			m_strName;
	SelectionType	m_Selection;
	DataType		m_DataType;
	BOOL			m_bSpecial;
	CString			m_physicalName; //only used for calculated column

public:
	PublicFieldObj(LPCTSTR pszName, SelectionType selType, const DataType& dataType, BOOL bSpecial, LPCTSTR physicalName)
		:
		m_strName(pszName),
		m_Selection(selType),
		m_DataType(dataType),
		m_bSpecial(bSpecial),
		m_physicalName(physicalName)
	{}
};

///////////////////////////////////////////////////////////////////////////////
//								PublicFieldsLinks
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT PublicFieldsLinks : public Array
{
	friend class CTblRuleDlg;
    friend class CAddCalcColumn;
	
public:
	// accessing elements
	PublicFieldObj*		GetAt		(int nIndex) const	{ return (PublicFieldObj*) Array::GetAt(nIndex); }
	PublicFieldObj*&	ElementAt	(int nIndex)		{ return (PublicFieldObj*&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	PublicFieldObj*		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	PublicFieldObj*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }
};
     

//===========================================================================
class TB_EXPORT CTblRuleDlg : public CParsedDialog
{
friend class CAddJoin;
DECLARE_DYNAMIC(CTblRuleDlg)
public:
	// costanti stringa
	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(ALL_TABLES, "<All archives>")
		TB_LOCALIZED(OLD_SELECTED, "<Already selected>")
	END_TB_STRING_MAP()
	
public:
	BOOL				m_bIsApplyPressed;

protected:
	TblRuleData*		m_pTblRule;
	BOOL				m_bNewRule;
	BOOL				m_bIsJoin;
	RuleDataArray*			m_pRuleData;
	WoormTable*		m_pSymTable;
	CWordArray&			m_IdArray;
	CWordArray			m_IdsHidden;
	CStringArray		m_TableNames;
	PublicFieldsLinks	m_PublicFieldsLinks;
	
	BOOL				m_bOnlyOneLink;
	SqlTableInfoArray	m_arSqlTableInfo;   
	SqlTableInfo*		m_pSqlTableInfo;
    PublicFieldsLinks   m_PublicCalculatedColumnsLinks; 
	CMultiListBox		m_mlbTables;
	CMultiListBox		m_mlbFields;
	CMultiListBox		m_mlbCalculatedCols;
	
	CComboBoxExt		m_cbTableGroups;

    CEdit				m_stcTblOfRule;
	CIdentifierEdit		m_edtPublicName;
	CDataObjTypesCombo	m_cbDataObjTypes;
	CBCGPComboBox		m_cbConstrainMode;
	CExpEdit			m_edtWClause;
	CEqnEdit			m_edtOrderBy;
	CExpEdit			m_edtHClause;
	CEqnEdit			m_edtGroupBy;
	CExpEdit			m_edtWhenClause;
	CIntEdit			m_edtTopRows;

	CBCGPButton			m_btnAddLink;
	CBCGPButton			m_btnAddLinkHid;
	CButton				m_btnAddCalcColumn;
	CBCGPButton			m_btnAddHiddenCalcColumn;
	CButton				m_btnRemoveCalcColumn;
	CButton				m_btnAddJoin;
	CButton				m_btnUpdateJoin;
	CBCGPButton			m_btnUndo;
	CBCGPButton			m_btnApply;

	CTaskBuilderStatusBar	m_sbStatus;

	DispTableInfo*		m_pDispTableInfo;
	SqlConnection*		m_pSqlConnection; //la connessione su cui leggere le info delle tabelle/colonne
	CBaseDocument*		m_pDocument;

public:
	CTblRuleDlg
		(
			RuleDataArray*			pRuleData,
			WoormTable*		pSymTable,
			CWordArray& 		IdArray,
			BOOL 				bOneLink,
			SqlConnection*		pSqlConnection,
			CBaseDocument*		pDocument,
			DispTableInfo*		pDispTableInfo,
			BOOL				bShowAdvancedForm
		);
	virtual ~CTblRuleDlg();

protected:
	void			CreateListBox				();
	void			InitStatusBar				();
	BOOL			LoadDictionary				(LPCTSTR);
	BOOL			LoadCalculatedColumns		();
	int				MarkSelectedTables			();
	int				MarkTableName				(int nIdx);
	CString			GetCurTableName				();
	int				GetCurReplicTable			();
	void 			EnableSelectionControls		(BOOL bEnableAdd, BOOL bCheckAlso, BOOL bEnableModify, BOOL bClear);
	void			EnableCalcColumnControls	(BOOL bEnableAdd, BOOL bEnableAddHidden, BOOL bEnableRemove);
	void 			ShowTableGroups				();
	void			ShowWclause					();
	void			ShowConstraintMode			();
	void			AddLinks					(BOOL bAddHiddenLinks);
	void			AddCalculatedColumn			(BOOL bHiddenCalcColumn);
	void			AddOneLink					(BOOL bAddHiddenLinks);
	void			AddMultiLink				(BOOL bAddHiddenLinks);
	void			SetConstraintMode			();
	int				GetLinkIdxFromFieldListIdx	(int nIdx);
	PublicFieldObj*	GetLinkFromFieldListIdx		(int nIdx)	{ return m_PublicFieldsLinks[GetLinkIdxFromFieldListIdx(nIdx)]; }
	BOOL			ExistPublicName				(CString&, int nCurrIdx, BOOL bRetry);
	void			CheckUndoAll				();
	BOOL			Save						();

protected:
	virtual void	RecalcLayout			();
	virtual BOOL	OnInitDialog			();
	virtual	void	OnOK					();
	virtual	void	OnCancel				();
	virtual INT_PTR OnToolHitTest(CPoint point, TOOLINFO* pTI) const;

	afx_msg	void	AddVisibleLinkAction			();
	afx_msg	void	AddHiddenLinkAction				();
	afx_msg	void	AddVisibleCalculatedColumn		();
	afx_msg	void	AddHiddenCalculatedColumn		();
	afx_msg	void	RemoveCalculatedColumn			();
	afx_msg	void	EditCalculatedColumn			();
	afx_msg void	AddJoin							();
	afx_msg void	UpdateJoin						();
	afx_msg	void	ShowFieldList					();
	afx_msg	void    ShowCalculatedColumns			();
	afx_msg	void	UndoAction						();
	afx_msg	void	TableNameChanged				();
	afx_msg	void	TableGroupChanged				();
	afx_msg	void	AddNewReplicTable				();
	afx_msg void	TableFieldChanged				();
	afx_msg void	TableCalculatedColumnsChanged	();
	afx_msg	void	PublicNameChanged				();
	afx_msg	void	DataTypeChanged					();
	afx_msg	void	FilterChanged					();
	afx_msg	void	OnApplySelection				();

	DECLARE_MESSAGE_MAP()
};

//============================================================================
class TB_EXPORT CModTblRuleDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CModTblRuleDlg)
protected:
	CString				m_strPublicNameField;
	TblRuleData*		m_pTblRule;
	RuleDataArray*			m_pRuleData;
	WoormTable*		m_pSymTable;
	SqlTableInfoArray	m_arSqlTableInfo;
	SqlConnection*		m_pSqlConnection;

	CStrStatic			m_stcPublicName;
	CEdit				m_stcTableName;
	CDataObjTypesCombo	m_cbDataObjTypes;
	CBCGPComboBox		m_cbConstrainMode;
	CExpEdit			m_edtWClause;
	CEqnEdit			m_edtOrderBy;
	CExpEdit			m_edtHClause;
	CEqnEdit			m_edtGroupBy;

	CTaskBuilderStatusBar	m_sbStatus;

	BOOL				m_bAlsoModifyType;
	CString				m_strDocumentFixedQuery;
	CExpEdit			m_edtWhenClause;
	CIntEdit			m_edtTopRows;

public:
	CModTblRuleDlg
		(
			LPCTSTR				pszNameField,
			RuleDataArray*			pRuleData,
			TblRuleData*		pTblRule,
			WoormTable*			pSymTable,
			BOOL				bAlsoModifyType,
			CString				strDocumentFixedQuery,
			SqlConnection*		pSqlConnection,
			BOOL				bShowAdvancedForm
		);
	virtual ~CModTblRuleDlg();

protected:
	BOOL			LoadDictionary		(LPCTSTR);
	void			ShowWclause			();
	void			ShowConstraintMode	();
	void			ShowStatusBarInfo	();
	void			InitStatusBar		();

	CString			GetCurTableName		();
	void			SetConstraintMode	();
	bool			OnCheckEdtWhereClause	(const CString& strExpression, CString& strError);
	bool			OnCheckEdtHavingClause (const CString& strExpression, CString& strError);
	bool			OnCheckEdtOrderClause (const CString& strExpression, CString& strError);
protected:
	virtual void	RecalcLayout		();
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();
    virtual INT_PTR OnToolHitTest(CPoint point, TOOLINFO* pTI) const;

	const SqlColumnInfo*	GetColumnInfo		(CString strPhysicalName);

	afx_msg	void	DataTypeChanged		();

	DECLARE_MESSAGE_MAP()
};

//============================================================================

class TB_EXPORT CAddCalcColumn : public CParsedDialog
{
	DECLARE_DYNAMIC(CAddCalcColumn)
public:
	TblRuleData*	    m_pTblRule;
	CString				m_publicName;
	CString				m_physicalName;
	DataType			m_type;
	
protected:
	WoormTable*				m_pSymTable;
	CIdentifierEdit				m_edtName;
	CDataObjTypesCombo			m_cbType;
	CEqnEdit					m_edtExpr;
	const PublicFieldsLinks&	m_calculatedColumnsArray;
	const PublicFieldsLinks&	m_publicFieldsArray;
    CString						m_old_publicName;
	CString						m_old_physicalName;
    DataType					m_old_type;
	int							m_old_position;

public:
	CAddCalcColumn
		(
			TblRuleData*				pTblRule,
			WoormTable*				pSymTable,
			const PublicFieldsLinks&	calculatedColumnsArray, 
			const PublicFieldsLinks&	publicFieldsArray,
			CString						old_physicalName	=	NULL,
			CString						old_publicName		=	_T(""),
			DataType					old_type			=	DATA_INT_TYPE,
			int			                old_position		=	-1
		);
	virtual ~CAddCalcColumn();

protected:
	virtual BOOL	OnInitDialog						();
	virtual	void	OnOK								();
	virtual	void	OnCancel							();
    BOOL			CheckExistsName						(const PublicFieldsLinks& fieldsArray);
	BOOL			CheckExistsNameWithoutMessageBox 	(CString advisedName);
	DECLARE_MESSAGE_MAP()

};

//============================================================================

class TB_EXPORT CAddJoin : public CParsedDialog
{
	DECLARE_DYNAMIC(CAddJoin)
public:
	// costanti stringa
	BEGIN_TB_STRING_MAP(Strings)
		TB_LOCALIZED(ALL_TABLES, "<All archives>")
	END_TB_STRING_MAP()
    
public:
    CStringArray			m_tablesNewJoin;

protected:
	CComboBoxExt			m_cbTableGroups;
	CResizableListBox		m_lbTables;
	CResizableListBox		m_lbSelectedTables;
	CButton					m_addTbl;
	CButton					m_removeTbl;
	CStringArray			m_TableNames;

	CTblRuleDlg*			m_pCTblRuleDlg;
	BOOL					m_oldJoin;
	TblRuleData*			m_pTblRule;
	int						m_idxRule;

public:
	CAddJoin	( 
				 CTblRuleDlg*				pCTblRuleDlg,
				 BOOL						oldJoin,
				 int						idxRule = -1
				);
	virtual ~CAddJoin();

protected:
	void				ShowTableGroups			();
    void				CheckSelectionsToAdd	(CStringArray& arSelections);
	void				ShowTableOldJoin		();
	BOOL				IsDeletableTable		(const SqlTableInfo* pSqlTableInfo);	

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

    //{{AFX_MSG( CAddJoin )
	afx_msg	void	TableGroupChanged				();
	afx_msg void    AddSelection					();
	afx_msg void    RemoveSelection					(); 
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
#include "endh.dex"
