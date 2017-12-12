
#pragma once

#include <TbGeneric\Array.h>
#include "XmlGesInfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#define DOC_DESCR_TREE_ITEM_TYPE_ROOT					0
#define DOC_DESCR_TREE_ITEM_TYPE_DBT					1
#define DOC_DESCR_TREE_ITEM_TYPE_XREF					2
#define DOC_DESCR_TREE_ITEM_TYPE_SEGMENT				3
#define DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY			4
#define DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP	5


class CAbstractFormDoc;
class CReportFields;

//===========================================================================
class CHKLFieldDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CHKLFieldDlg)
private:
	CBCGPComboBox						m_RepFieldsCombo;
	CBCGPComboBox						m_DBTsCombo;
	CBCGPComboBox						m_DocFieldsCombo;
	CXMLHKLFieldArray::HKLListType	m_eListType;

public:
	CHKLFieldDlg(CXMLHKLFieldArray::HKLListType eListType, CWnd* pWndParent);

private:
	void InitCombos(DataTypeNamedArray* pReportColumns, CObArray* pDocumentFields);

protected:                  
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	// Generated message map functions
	//{{AFX_MSG(CHKLFieldDlg)
	afx_msg void OnDBTChange();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class CSingleHKLDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CSingleHKLDlg)
private:
	CBCGPComboBox			m_TextBoxCombo;
	CBCGPComboBox			m_DescriptionCombo;
	CBCGPComboBox			m_ImageCombo;
	CBCGPListCtrl			m_PreviewFieldsList;
	CBCGPListCtrl			m_FilterFieldsList;
	CBCGPListCtrl			m_ResultFieldsList;
    
public:
	CXMLHotKeyLink*		m_pHKLInfo;
	CXMLHotKeyLink*		m_pHKLInfoToModify;
	CXMLHKLField*		m_pCurrHKLField;
	Array*				m_pDocumentFields;
	CReportFields*		m_pReportFields;

public:
	CSingleHKLDlg(CWnd* pWndParent);
	~CSingleHKLDlg();

private:
	void	InitSingleCombo(CComboBox* pCombo, DataTypeNamedArray* pFieldsArray, const CString& strSelectedField);
	void	InitSingleListCtrls(CListCtrl*, CXMLHKLFieldArray*, CXMLHKLFieldArray::HKLListType);
	BOOL	InitCombos();
	void	InitListCtrls();
	void	OnModifyAssociation(CListCtrl*, CXMLHKLFieldArray*, CXMLHKLFieldArray::HKLListType);
	void	OnAddAssociation(CListCtrl* pListCtrl, CXMLHKLFieldArray*, CXMLHKLFieldArray::HKLListType);
	void    OnRemoveAssociation(CListCtrl*, CXMLHKLFieldArray*);

protected:                  
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	// Generated message map functions
	//{{AFX_MSG(CSingleHKLDlg)
		afx_msg void OnAddPreview	();
		afx_msg void OnAddFilter	();
		afx_msg void OnAddResult	();	

		afx_msg void OnRemovePreview();
		afx_msg void OnRemoveFilter	();
		afx_msg void OnRemoveResult	();

		afx_msg void OnModifyPreview(NMHDR*, LRESULT*);
		afx_msg void OnModifyFilter	(NMHDR*, LRESULT*);
		afx_msg void OnModifyResult	(NMHDR*, LRESULT*);	
		afx_msg void OnSelectPreview(NMHDR*, LRESULT*);
		afx_msg void OnSelectFilter	(NMHDR*, LRESULT*);
		afx_msg void OnSelectResult	(NMHDR*, LRESULT*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CHotKeyLinkDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CHotKeyLinkDlg)
private:
	CButton				m_XRefRadioBtn;
	CButton				m_DbtRadioBtn;
	CButton				m_FieldRadioBtn;
	
	CBCGPListCtrl		m_HotKeyLinkList;
	CBCGPComboBox		m_DBTsCombo;
	CBCGPComboBox		m_FieldsCombo;

	CXMLDBTInfo*		m_pDbtInfo;
	CXMLXRefInfo*		m_pXRefInfo;
	SqlRecord*			m_pRecord;

public:
	CXMLDocObjectInfo*	m_pDocObjectInfo;
	CXMLDocObjectInfo*	m_pExtDocObjectInfo;
	Array*				m_pDocumentFields;
	CXMLHotKeyLink*		m_pCurrHKLInfo;

public:
	CHotKeyLinkDlg(CXMLDBTInfo*, CXMLDocObjectInfo*, CXMLXRefInfo* = NULL, CXMLDocObjectInfo* = NULL, CWnd* pWndParent = NULL);
	~CHotKeyLinkDlg();

private:
    void LoadSingleDBTField		(CXMLDBTInfo* pDBTInfo, BOOL bExtRef);
	BOOL InitDocumentFields		();
	void FillDBTsCombo			();
	void FillHKLList			();
	void FillFieldsCombo		(CXMLDBTInfo* pDBTInfo);
	void GetFieldInformation(CString& strFieldName, CString& strTitleName, CXMLHotKeyLink::HKLFieldType& eType);
	int	 IsFieldPresent(const CString& strFieldName, CXMLHotKeyLink::HKLFieldType eType);
	BOOL OpenTBExplorer(CTBNamespace& nsReport);
	SqlRecord* GetSqlRecord(const CString& strTableName);
	void OnAddHKL				();
	

protected:                  
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	virtual void OnCancel		();

	// Generated message map functions
	//{{AFX_MSG(CHotKeyLinkDlg)
	afx_msg void OnHKLTypeChanged	();
	afx_msg void OnEditHKL			();
	afx_msg void OnRemoveHKL		();
	afx_msg void OnDBTChange		();
	afx_msg void OnFieldChange		();		
	afx_msg void OnAssociateReport	();
	afx_msg void OnSelectHKL		(NMHDR*, LRESULT*);
	afx_msg void OnDBLClickHKL		(NMHDR*, LRESULT*);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class CFixedFieldDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CFixedFieldDlg)
private:
	CBCGPComboBox	m_FixedFieldsCombo;
	CEnumCombo		m_EnumCombo;
	CBCGPComboBox	m_BoolCombo;
	CBCGPListCtrl	m_FixedFieldsList;
	
	CXMLDBTInfo*	m_pDbtMasterInfo;
	
	SqlRecord*		m_pRecord;

public:
	CFixedFieldDlg(CXMLDBTInfo* pDbtMaster, CWnd* pWndParent = NULL);
	~CFixedFieldDlg();

private:
	void		FillFieldList	();
	void		FillFieldCombo	();
	int			IsFieldPresent	(const CString& strFieldName);

protected:                  
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	// Generated message map functions
	//{{AFX_MSG(CFixedFieldDlg)
		afx_msg void		OnAddFixedField		();
		afx_msg void		OnFieldChange		();
		afx_msg void		OnRemoveFixedField	();
	//}}AFX_MSG
	
	DECLARE_MESSAGE_MAP()
};


//---------------------------------------------------------------------------------
//class CDBTPropertiesDlg
//---------------------------------------------------------------------------------
class TB_EXPORT CDBTPropertiesDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CDBTPropertiesDlg)
private:
	CXMLDBTInfo* m_pDBTInfo;

public:
	CDBTPropertiesDlg(CXMLDBTInfo*);
		
protected:
	// Generated message map functions
	//{{AFX_MSG(CDBTPropertiesDlg)
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class CUniversalKeyGroupDlg
//---------------------------------------------------------------------------------
class TB_EXPORT CUniversalKeyGroupDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CUniversalKeyGroupDlg)
private:
	CXMLUniversalKeyGroup*	m_pXMLUniversalKeyGroup;
	BOOL					m_bReadOnly;
	CBCGPComboBox			m_FuncCombo;
public:
	CUniversalKeyGroupDlg	(CXMLUniversalKeyGroup*, BOOL = FALSE);
	
private:
	int FillUKFuncCombo	(const CString&);

protected:
	// Generated message map functions
	//{{AFX_MSG(CUniversalKeyGroupDlg)
	virtual BOOL OnInitDialog			();
	virtual void OnOK					();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class CUniversalKeyDlg
//---------------------------------------------------------------------------------
class TB_EXPORT CUniversalKeyDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CUniversalKeyDlg)
private:
	CXMLUniversalKey*	m_pXMLUniversalKey;
	BOOL				m_bReadOnly;
	CString				m_strTableName;
	CTBListBox			m_FieldList;
	CTBListBox			m_UkSegmentList;

public:
	CUniversalKeyDlg(CXMLUniversalKey*, const CString&, BOOL = FALSE);

private:
	void FillFieldList		();
	void FillSegmentList	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CUniversalKeyDlg)
	virtual BOOL OnInitDialog			();
	virtual void OnOK					();
	afx_msg void OnAddSegment			();
	afx_msg void OnRemoveSegment		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------------
//	CDocDescrTreeCtrl
//----------------------------------------------------------------------------
class TB_EXPORT CDocDescrTreeCtrl : public CTBTreeCtrl
{
	DECLARE_DYNCREATE(CDocDescrTreeCtrl)

protected:
	BOOL			m_bEnableDBTNaming;
	BOOL			m_bReadOnly;
	BOOL			m_bDescription;
	CTBNamespace	m_nsDocument;

public:
	CImageList			m_ImageList;
	CXMLDocObjectInfo*	m_pXMLDocObject;

public:
	CDocDescrTreeCtrl	();
	~CDocDescrTreeCtrl	();

protected:
	HTREEITEM				InsertDBTItem						(CXMLDBTInfo*, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	HTREEITEM				InsertXRefItem						(CXMLXRefInfo*, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	HTREEITEM				InsertXRefSegmentItem				(CXMLSegmentInfo*, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	HTREEITEM				InsertUniversalKeyGroupItem			(CXMLUniversalKeyGroup*, const CString&, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	HTREEITEM				InsertUniversalKeyItem				(CXMLUniversalKey*, const CString&, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	HTREEITEM				InsertUniversalKeySegmentItem		(const CString&, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST);
	
	int						GetItemType							(HTREEITEM) const;
	BOOL					IsXRefNameAlreadyUsed				(HTREEITEM ) const;
	void					AddDbtInfo							(CXMLDBTInfo*, HTREEITEM, CXMLDocObjectInfo* = NULL);
	
public:
	static BOOL				LoadMenu(CMenu& menu, int nItemType);
	CXMLDBTInfo*			GetCurrentDBT					(HTREEITEM* = NULL) const;
	CXMLXRefInfo*			GetCurrentXRef					(HTREEITEM* = NULL) const;
	CXMLSegmentInfo*		GetCurrentSegment				(HTREEITEM* = NULL) const;
	CXMLUniversalKey*		GetCurrentUniversalKey			(HTREEITEM* = NULL) const;
	CXMLUniversalKeyGroup*	GetCurrentUniversalKeyGroup		(HTREEITEM* = NULL) const;

	void					SetSegmentItemText				(HTREEITEM);
	void					EnableDBTNaming					(BOOL = TRUE);
	void					RemoveTreeChilds				(HTREEITEM);
	//CAbstractFormDoc*		GetDocument						() const;
	void					SetReadOnly						(BOOL = FALSE);
	void					InitializeImageList				();
	void					UpdateXRefIcon					(HTREEITEM, CXMLXRefInfo*);	
	void					SetXMLDocObjInfo(CXMLDocObjectInfo* pXMLDocObj);

public:
	virtual		void		FillTree						(CXMLDocObjectInfo* pOldXMLDocObj);
	virtual		void		SetBackColor					();

public:
	//{{AFX_MSG(CDocDescrTreeCtrl)
	virtual afx_msg void OnContextMenu					(CWnd*, CPoint);
	virtual afx_msg void OnDBTProperties				();

	afx_msg void OnDoubleClick					(NMHDR*, LRESULT*);
	afx_msg void OnItemExpanding				(NMHDR*, LRESULT*);
	afx_msg void OnItemBeginEdit				(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit					(NMHDR*, LRESULT*);
	afx_msg void OnKeydown(NMHDR* pNMHDR, LRESULT* pResult);
	
	afx_msg void OnNewXRef						();
	afx_msg void OnNewUniversalKeyGroup			();
	afx_msg void OnFixedField					();
	afx_msg void OnDBTHotKeyLink				();
	afx_msg void OnXRefHotKeyLink				();
	
	afx_msg void OnModifyXRef					();
	afx_msg void OnRemoveXRef					();
	afx_msg void OnAppendXRefs					();
	
	afx_msg void OnNewUniversalKey				();
	afx_msg void OnUniversalKeyGroupProperties	();
	afx_msg void OnRemoveUniversalKeyGroup		();
	
	afx_msg void OnUniversalKeyProperties		();
	afx_msg void OnRemoveUniversalKey			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class CSegmentsGrid 
//---------------------------------------------------------------------------------
class TB_EXPORT CSegmentsGrid : public CBCGPListCtrl
{
	CXMLXRefInfo*	m_pXRef;
	CString			m_sCurrDBTNs;

// Construction
public:
	CSegmentsGrid(LPCTSTR = NULL, CXMLXRefInfo* = NULL);

//Operations
private:
	BOOL	InsertSegmentColumn(int, int, LPCTSTR);
	
public:
	void	AttachXRef			(CXMLXRefInfo* pXRef);

	void	SetColumnTitle			(int, LPCTSTR);
	BOOL	SetFKSegmentAt			(int, LPCTSTR);
	CString	GetFKSegmentAt			(int) const;
	BOOL	SetReferencedSegmentAt	(int, LPCTSTR);
	CString	GetReferencedSegmentAt	(int) const;
	BOOL	SetFKFixedValueAt		(int, LPCTSTR);
	CString	GetFKFixedValueAt		(int) const;
	
	int		GetRowCount			() const;
	int		GetColumnCount		();

	BOOL	SelectSegment		(LPCTSTR); 
	int		GetSelectedRowIdx	() const;
	BOOL	GetSelectedRowRect	(CRect&) const;
	BOOL	RemoveSelectedRow	();
	BOOL	RemoveRowAt			(int);

	BOOL	ModifySegmentAt		(int, LPCTSTR, LPCTSTR, LPCTSTR);
	BOOL	InsertNewSegment	(LPCTSTR, LPCTSTR, LPCTSTR);
	CString	GetCurrDBT			();
	void	SetCurrDBT			(const CString& strNs);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSegmentsGrid)
protected:
	virtual void PreSubclassWindow();
	//}}AFX_VIRTUAL

	// Generated message map functions
protected:
	//{{AFX_MSG(CSegmentsGrid)
	afx_msg void OnKeyDown	(NMHDR*, LRESULT*);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//             class CProfileCombo definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CProfileCombo : public CBCGPComboBox
{
	DECLARE_DYNCREATE (CProfileCombo)
private:
	CTBNamespace m_nsDocument;

public:
	CProfileCombo ();

	CTBNamespace	GetDocumentNameSpace()	const { return m_nsDocument;}
	void			SetDocumentNameSpace(const CTBNamespace&, const CStringArray&);

public:
	void	Fill();
	int		SelectProfile(LPCTSTR = NULL);
	CString GetSelectedProfile() const;

};


//---------------------------------------------------------------------------------
//class CAppendXRefDialog
//---------------------------------------------------------------------------------
class TB_EXPORT CAppendXRefDialog : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CAppendXRefDialog)
private:
	CStringArray*  m_pExistingDocNamespaces;
	CStringArray*  m_pSelectedDocNamespaces;

public:
	CAppendXRefDialog(CStringArray*	pExistingDocNamespaces, CStringArray* pSelectedDocNamespaces);
	
private:
	void FillListBox	();

protected:
	virtual BOOL OnInitDialog			();
	virtual void OnOK					();
};
#include "endh.dex"

