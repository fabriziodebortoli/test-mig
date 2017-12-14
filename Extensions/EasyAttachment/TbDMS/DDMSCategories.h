
#pragma once

#include <tbges\extdoc.h>
#include <tbges\dbt.h>

#include "beginh.dex"

class DDMSCategories;
class CDMSCategories;
class CDMSCategory;
class VCategoryValues;

///////////////////////////////////////////////////////////////////////////////
//					class DBTDMSCategoriesValues 						 //
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTDMSCategoriesValues : public DBTSlaveBuffered
{ 
	DECLARE_DYNAMIC(DBTDMSCategoriesValues)
	
public:
	DBTDMSCategoriesValues
		(
			CRuntimeClass*		pClass, 
			CAbstractFormDoc*	pDocument
		);

public: 
	DDMSCategories*		GetDocument			()	 		const	{ return (DDMSCategories*)m_pDocument; }
	VCategoryValues*	GetVCategoryValues	()			const	{ return (VCategoryValues*)GetRecord(); }
	VCategoryValues*	GetVCategoryValues	(int nRow)	const	{ return (VCategoryValues*)GetRow(nRow); }
	VCategoryValues*	GetVCategoryValues	(const DataStr& value) const;

public: 
	virtual void OnPreparePrimaryKey	(int nRow, SqlRecord*);
	virtual BOOL LocalFindData(BOOL bPrepareOld);

protected:
	// Gestiscono la query
	virtual	void	OnDefineQuery	();
	virtual	void	OnPrepareQuery	();

	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual DataObj*	OnCheckUserData		(int nRow);


	virtual void	 OnPrepareRow(int nRow, SqlRecord* pRec);
	virtual DataObj* GetDuplicateKeyPos(SqlRecord* pRec);
	virtual CString  GetDuplicateKeyMsg(SqlRecord* pRec);
};

///////////////////////////////////////////////////////////////////////////////
//					class DDMSCategories									 //
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DDMSCategories : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DDMSCategories)

private:
	BOOL	m_bSaving;

public:
	CString						m_strCurrentCategoryName;
	CDMSCategories*				m_pCategories;
	CDMSCategory*				m_pOldDMSCategory;
	CDMSCategory*				m_pCurrentDMSCategory;
	DBTDMSCategoriesValues*		m_pDBTCategoriesValues;
	DataStr						m_CategoryInUseMsg;

	//tree control
	CDMSCategoriesTreeViewAdv*	m_pCategoriesTreeView;
	DataBool					m_bTreeView;
	BOOL						m_bTreeViewLoaded;

public:
	DDMSCategories();
	~DDMSCategories();

public:	// function Member
	CDMSCategory*		GetCurrentDMSCategory	() const;
	VCategoryValues*	GetVCategoryValues		() const;

	void SetTreeView			(CDMSCategoriesTreeViewAdv* pTreeView);
	void SetCurrentDMSCategory	();

private:
	void EnableControls			();
	void InitCurrentCategory	();
	void LoadCategories			();

protected:
	virtual BOOL OnAttachData			();
	virtual BOOL OnOkTransaction		();
	virtual BOOL CanRunDocument			();
	virtual void OnInitializeUI			(const CTBNamespace& aFormNs);

	//gestione control
	virtual	void DisableControlsForBatch();	
	
	//gestione toolbar
	virtual BOOL CanDoEditRecord		();
	virtual BOOL CanDoDeleteRecord		();
	virtual BOOL CanDoNewRecord			();
	virtual BOOL CanDoSaveRecord		();
	virtual BOOL CanDoRefreshRowset		();
	virtual BOOL CanDoEscape			();
	// json
	virtual	void OnParsedControlCreated	(CParsedCtrl* pCtrl);

public:
	//{{AFX_MSG(CAbstractFormFrame)
	DECLARE_MESSAGE_MAP()
	afx_msg void OnBeDefaultCheckChanged();
	afx_msg void OnNewRecord			();
	afx_msg void OnEditRecord			();
	afx_msg void OnSaveRecord			();
	afx_msg void OnDeleteRecord			();
	afx_msg void OnEscape				();
	afx_msg void OnRefreshRowset		();
	afx_msg void OnSelectionNodeChanged	();
	afx_msg void OnLoadDMSCategoriesTree();
	afx_msg void OnSetCurrentNode		();
	//}}AFX_MSG
};

#include "endh.dex"
