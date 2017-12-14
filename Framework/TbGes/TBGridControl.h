#pragma once

//includere alla fine degli include del .H
#include <TbGeneric\DataObj.h>
#include <TbGenlib\OslInfo.h>
#include <TbGenlib\PARSOBJ.H>

#include "ExtDocAbstract.h"
#include "TBGridControlDataSource.h"

#include "beginh.dex"

class DBTSlaveBuffered;

//============================================================================
class TB_EXPORT CTBGridColumnInfo : public CObject, public IOSLObjectManager
{
private:
	CTBGridControl*		m_pTBGridControl;
	DataObj*			m_pDataObj;
	CString				m_strTitle;
	int					m_nDataInfoIdx;
	int					m_nPixelsWidth;
	int					m_nWidth;
	// for hyperlink on columns
	CRuntimeClass*		m_pHKLClass;
	BaseDocumentPtr		m_pLinkedDocument;
	HotKeyLink*			m_pHotKeyLink;
	// for automatic htolink descriptions
	CTBGridColumnInfo*	m_pMasterColumn;

public:
	CTBGridColumnInfo 
		(
			const	CString&		strName, 
					DataObj*		pDataObj, 
			const	CString&		strColTitle,
					int				nWidth = 0,
					CRuntimeClass*	pHKLClass = NULL
		);

	void Attach				(CTBGridControl*	pTBGridControl)		{ 	m_pTBGridControl = pTBGridControl; }
	void AttachMasterColumn	(CTBGridColumnInfo*	pMasterColumn)		{ 	m_pMasterColumn = pMasterColumn; }

protected:
	~CTBGridColumnInfo	();

public:
	CTBNamespace&		GetNamespace	() { return GetInfoOSL()->m_Namespace; }
	CString				GetTitle		() { return m_strTitle; }
	DataObj*			GetDataObj		() { return m_pDataObj; }
	CString				GetColumnName	() { return GetInfoOSL()->m_Namespace.GetObjectName(); }

	void				SetColWidth (int width)			{ m_nWidth = width;}
	int					GetColWidth ()					{ return m_nWidth;}

	void				SetColPixelsWidth (int width)	{ m_nPixelsWidth = width;}
	int					GetColPixelsWidth ()			{ return m_nPixelsWidth;}

	void				SetDataInfoIdx	(int nDataInfoIdx) { m_nDataInfoIdx = nDataInfoIdx; }
	int					GetDataInfoIdx	() { return m_nDataInfoIdx; }

	CRuntimeClass*		GetHKLClass		() { return m_pHKLClass; }
	CTBGridColumnInfo*	GetMasterColumn	() { return m_pMasterColumn; }
	HotKeyLink*			GetHotKeyLink	();

	void				DoFollowHyperlink	(CString strData, BOOL bActivate = TRUE);
};

//=============================================================================
class TB_EXPORT CTBGridColumnInfoArray : public Array
{
public:
	CTBGridColumnInfoArray () {};

	// Accessing elements
	CTBGridColumnInfo*		GetAt(int nIndex) const	{ return nIndex >= 0 && nIndex <= CObArray::GetUpperBound() ?  (CTBGridColumnInfo*) CObArray::GetAt(nIndex) : NULL; }
	CTBGridColumnInfo*&		ElementAt(int nIndex)	{ return (CTBGridColumnInfo*&) CObArray::ElementAt(nIndex); }
	
	// overloaded operator helpers
	CTBGridColumnInfo*		operator[](int nIndex) const	{ return GetAt(nIndex); }
	CTBGridColumnInfo*&		operator[](int nIndex)			{ return ElementAt(nIndex); }

	int		GetColumnIdx(CTBGridColumnInfo* pInfo);

	virtual void    Add		 (CTBGridColumnInfo* pInfo);
	virtual void    Remove	 (CTBGridColumnInfo* pInfo);
};

//==============================================================================
class CTBGridHyperlink : public CBCGPGridURLItem
{
	DECLARE_DYNCREATE(CTBGridHyperlink)
protected:
	CTBGridHyperlink() {}

public:
	CTBGridHyperlink
		(
			CString				strValue,
			CTBGridColumnInfo*	pColInfo
		)
		:
		CBCGPGridURLItem(strValue, _T(""))
	{
		m_pColInfo = pColInfo;
	}

public:
	virtual BOOL OnClickValue	(UINT uiMsg, CPoint point);
	virtual BOOL OnSetCursor	() const;

private:
	CTBGridColumnInfo*	m_pColInfo;
};

//============================================================================
class TB_EXPORT CTBGridControl : public CTBGridControlObj, public IOSLObjectManager
{
	DECLARE_DYNAMIC (CTBGridControl)

private:
	CString						m_sName;
	CParsedForm*				m_pParentForm;
	
	CTBGridColumnInfoArray*		m_pGridColumnInfoArray;

	TBGridControlDataSource*	m_pGridDataSource;
	CAbstractFormDoc*			m_pDoc;

protected:
	// for virtual mode
	SqlRecord*					m_pCurrentRecord;

private:
	bool IsOslVisible(CInfoOSL* pInfoOSL);
	void Attach		 ();

protected:
	TBGridControlDataSource* GetGridDataSource() const { return m_pGridDataSource; }

public:
	CTBGridControl(const CString sName = _T(""));
	~CTBGridControl(void);
	
	void SetDataSource(DBTSlaveBuffered* pDBTSlaveBuffered);
	void SetDataSource(SqlTable* pSqlTable);
	void SetDataSource(RecordArray* records);
	void SetParentForm (CParsedForm* pParentForm);
	void SetName(const CString& sName);
	void Reload();

	// for virtual mode
			void		EnableVirtualMode	();
	virtual void		SetCurrentRecord	(int nRow);
			SqlRecord*	GetCurrentRecord	(int nRow)				{ return m_pCurrentRecord; }

	CString				GetName			() const { return m_sName; }
	CTBNamespace&		GetNamespace	() { return GetInfoOSL()->m_Namespace; }
	CAbstractFormDoc*	GetDocument		();
	int					GetColumnsCount	() const { return m_pGridColumnInfoArray->GetSize(); }
	CParsedForm*		GetParentForm	() const { return m_pParentForm; }
	
	CTBGridColumnInfo*	GetColumnInfo(int index) const { return m_pGridColumnInfoArray->GetAt(index); }
	CTBGridColumnInfo*	AddColumn
						(
							const	CString&		strName,
									DataObj*		pDataObj,
							const	CString&		strColTitle,
									int				nCharWidth = 0,
									CRuntimeClass*	pHKLClass = NULL
						);

	virtual	void	Customize		();
	void			CreateAllColumns();
	virtual void	OnUpdateControls(BOOL bParentIsVisible = TRUE);
	virtual BOOL	IsColumnReadOnly(int nColumn)	{ return TRUE; } // by default all cells of all rows are readonly
			
	void AddRow(SqlRecord* pRec);

protected:
	virtual CBCGPGridItem*	OnCreateVirtualItem (BCGPGRID_DISPINFO* pdi);

	//{{AFX_MSG( CTBGridControl )
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"


