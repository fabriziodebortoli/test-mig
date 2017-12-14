#pragma once

#include <TBGenlib\parsobj.h>
#include "beginh.dex"

class CTaskBuilderBreadcrumb;
//======================================================================
class TB_EXPORT CTaskBuilderBreadcrumbItem : public CObject
{
	HBREADCRUMBITEM			m_hParentItem;
	HBREADCRUMBITEM			m_hItem;
	CString					m_sName;
	CTaskBuilderBreadcrumb*	m_pCrumb;
	DataObj*				m_pSourceDataObj;

public:
	CTaskBuilderBreadcrumbItem(CTaskBuilderBreadcrumb* pCrumb, CString sName, HBREADCRUMBITEM parentItem, HBREADCRUMBITEM item);

	HBREADCRUMBITEM GetParentItem	() const { return m_hParentItem; }
	HBREADCRUMBITEM GetItem			() const { return m_hItem; }
	const CString&  GetName			() const { return m_sName; }
	CString			GetText			() const;
	
	const BOOL  IsRootItem			() const { return m_hItem == NULL; }

	void	SetText(const CString& strText);
	void	SetSourceDataObj(DataObj* pDataObj);
	void	SetImage(const UINT& nIDB, BOOL bPng = TRUE);
	void	SetImage(const CString& strImageNamespace);

	CTaskBuilderBreadcrumbItem* AddItem
			(
				const CString& sName, 
				const CString& sText
			);
	void RemoveItem	(const CString& sName);
};

//======================================================================
class TB_EXPORT CTaskBuilderBreadcrumb : public CBCGPBreadcrumb, public CParsedCtrl, public ResizableCtrl
{
	DECLARE_DYNCREATE(CTaskBuilderBreadcrumb);
	
	CObArray	m_arItems;
	CImageList*	m_pImageList;
	CFont*		m_pFont;
	COLORREF	m_BkgColor;
	COLORREF	m_ForeColor;
	
	friend class CTaskBuilderBreadcrumbItem;

public:
	CTaskBuilderBreadcrumb	();
	~CTaskBuilderBreadcrumb	();

	BOOL Create			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
	// dynamic subclassing (don't use in conjunction with create)
	BOOL SubclassEdit	(UINT nID, CWnd* pParent, const CString& strName = _T(""));

	// virtual functions
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
	virtual	CString	GetValue		();
	virtual void	GetValue		(CString& strValue);
	virtual DataType	GetDataType	()	const;
	virtual	BOOL	OnInitCtrl		();
	virtual BOOL	IsValid			();
	virtual BOOL	IsValid			(const DataObj& aValue);

	// metodi per la manipolazione
	void RemoveAll ();
	CTaskBuilderBreadcrumbItem* GetRootItem ();

	CTaskBuilderBreadcrumbItem* AddItem
			(
				const CString& sName, 
				const CString& sText, 
				CTaskBuilderBreadcrumbItem* pParent = NULL
			);
	void RemoveItem			(const CString& sName, CTaskBuilderBreadcrumbItem* pParent = NULL);
	void RemoveItem			(CTaskBuilderBreadcrumbItem* pItem);

	void SetBreadCrumbFont	(CFont* pFont);
	void SetBkgColor	(COLORREF color);
	void SetForeColor	(COLORREF color);

	static TCHAR GetDelimiter();

private:
	CTaskBuilderBreadcrumbItem* FindItem (HBREADCRUMBITEM hSelectedItem);
	CTaskBuilderBreadcrumbItem* FindItem (const CString& sName, CTaskBuilderBreadcrumbItem* pParent = NULL);
	void						SetImage (HBREADCRUMBITEM hSelectedItem, HICON image);
	void						RemoveAllItems	();
	
public: 
	void  InitSizeInfo() { ResizableCtrl::InitSizeInfo(this); }

protected:
	virtual void OnSelectionChanged(HBREADCRUMBITEM hSelectedItem);

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP();
};


#include "endh.dex"