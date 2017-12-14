#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include "parsobj.h"
#include "TBToolBar.h"
#include "TBBreadCrumb.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szRootName[] = _T("root");
static const int nImageSize = 16;
static const TCHAR szDelimiter = _T('\\');

/////////////////////////////////////////////////////////////////////////////
//					CTaskBuilderBreadcrumbItem
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem::CTaskBuilderBreadcrumbItem(CTaskBuilderBreadcrumb* pCrumb, CString sName, HBREADCRUMBITEM parentItem, HBREADCRUMBITEM item)
	:
	m_sName (sName),
	m_hItem	(item),
	m_hParentItem(parentItem),
	m_pCrumb(pCrumb),
	m_pSourceDataObj(NULL)
{
}

//-----------------------------------------------------------------------------
CString CTaskBuilderBreadcrumbItem::GetText() const
{
	ASSERT(m_pCrumb);
	return m_pCrumb->GetItemText(m_hItem);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumbItem::SetText(const CString& strText)
{
	ASSERT(m_pCrumb);
	m_pCrumb->SetItemText(m_hItem, strText);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumbItem::SetImage(const UINT& nIDB, BOOL bPng /*TRUE*/)
{
	ASSERT(m_pCrumb);
	CDC* pDC = m_pCrumb->GetDC();

	HICON image = TBLoadImage(pDC, NULL, nIDB, nImageSize, bPng);
	if (image)
		m_pCrumb->SetImage(GetItem(), image);

	m_pCrumb->ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumbItem::SetImage(const CString& strImageNamespace)
{
	ASSERT(m_pCrumb);
	CDC* pDC = m_pCrumb->GetDC();

	HICON image = ::TBLoadImage(strImageNamespace, pDC, nImageSize); 
	if (image)
		m_pCrumb->SetImage(GetItem(), image);

	m_pCrumb->ReleaseDC(pDC);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumbItem::SetSourceDataObj(DataObj* pDataObj)
{
	m_pSourceDataObj = pDataObj;
	if (m_pSourceDataObj)
	{
		SetText(m_pSourceDataObj->FormatData());
	}
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CTaskBuilderBreadcrumbItem::AddItem
		(
			const CString& sName, 
			const CString& sText
		)
{
	ASSERT(m_pCrumb);
	return m_pCrumb->AddItem(sName, sText, this);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumbItem::RemoveItem	(const CString& sName)
{
	ASSERT(m_pCrumb);
	m_pCrumb->RemoveItem(sName, this);
}

/////////////////////////////////////////////////////////////////////////////
//						CTaskBuilderDockPaneTabs 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE (CTaskBuilderBreadcrumb, CBCGPBreadcrumb)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTaskBuilderBreadcrumb, CBCGPBreadcrumb)
	ON_MESSAGE			(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumb::CTaskBuilderBreadcrumb()
	:
	CParsedCtrl		(NULL),
	m_pImageList	(NULL), 
	m_pFont			(NULL)
{
	CParsedCtrl::Attach(this);

	TBThemeManager* pManager = AfxGetThemeManager();

	m_BkgColor	= pManager->GetEnabledControlBkgColor();
	m_ForeColor = pManager->GetEnabledControlForeColor();
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumb::~CTaskBuilderBreadcrumb()
{
	if (m_pFont)
		delete m_pFont;

	RemoveAllItems();
	if (m_pImageList)
		delete m_pImageList;
}


//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::SetBreadCrumbFont	(CFont* pFont)
{
	if (!pFont)
		return;

	if (!m_pFont || m_pFont->m_hObject != pFont->m_hObject)
	{
		if (m_pFont)
			delete m_pFont;

		LOGFONT lf;
		pFont->GetLogFont(&lf);
		CFont* cloneFont = new CFont();
		cloneFont->CreateFontIndirect(&lf);
		SetFont(cloneFont);
		m_pFont = cloneFont;
	}
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CTaskBuilderBreadcrumb::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk = CheckControl(nID, pParentWnd) && __super::Create(rect, pParentWnd, nID, dwStyle);
	bOk = bOk && InitCtrl();
	if (bOk)
		__super::SetImageList(NULL);
	
	return bOk;
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::SetValue(const DataObj& aValue)
{
	__super::SelectPath(aValue.Str());
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::GetValue(DataObj& aValue)
{
	CString strBuffer = GetSelectedPath();
	aValue.Assign(strBuffer);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::GetValue (CString& strValue)
{ 
	strValue = GetSelectedPath();
}

//-----------------------------------------------------------------------------
CString CTaskBuilderBreadcrumb::GetValue()
{
	CString str;
	GetValue(str);
	
	return str;
}

//-----------------------------------------------------------------------------
DataType CTaskBuilderBreadcrumb::GetDataType	()	const
{
	return DataType::String;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderBreadcrumb::SubclassEdit (UINT nID, CWnd* pParent, const CString& strName)
{
	BOOL bOk =
			CheckControl(nID, pParent, _T("EDIT"))	&&
			SubclassDlgItem(nID, pParent)			&&
			InitCtrl();

	if (bOk)
		SetNamespace(strName);

	ResizableCtrl::InitSizeInfo (this);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderBreadcrumb::OnInitCtrl ()
{
	ModifyStyle(0, BCCS_SHOWROOTALWAYS);
	
	SetBackColor(m_BkgColor);
	SetDefaultTextColor(m_ForeColor);

	TBThemeManager* pManager = AfxGetThemeManager();
	SetBreadCrumbFont(pManager->GetControlFont());

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::SetBkgColor (COLORREF color)
{
	m_BkgColor = color;
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::SetForeColor (COLORREF color)
{
	m_ForeColor = color;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderBreadcrumb::IsValid ()
{ 
	return CParsedCtrl::IsValid() && GetValue(); 
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderBreadcrumb::IsValid (const DataObj& aValue)
{ 
	return CParsedCtrl::IsValid(aValue); 
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CTaskBuilderBreadcrumb::GetRootItem ()
{
	CTaskBuilderBreadcrumbItem* pRoot = FindItem(__super::GetRootItem());
	// alla prima invocazione genero la wrapper della root
	if (!pRoot)
	{
		pRoot = new CTaskBuilderBreadcrumbItem(this, szRootName, NULL, __super::GetRootItem());
		m_arItems.Add(pRoot);
	}
	return pRoot;
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::OnSelectionChanged(HBREADCRUMBITEM hSelectedItem)
{
	SetModifyFlag(TRUE);
	UpdateCtrlData(TRUE); 
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::RemoveAll ()
{
	RemoveAllItems();
	__super::DeleteItem(__super::GetRootItem());
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CTaskBuilderBreadcrumb::FindItem (HBREADCRUMBITEM hItem)
{
	for (int i=0; i < m_arItems.GetSize(); i++)
	{
		CTaskBuilderBreadcrumbItem* pItem = (CTaskBuilderBreadcrumbItem*) m_arItems.GetAt(i);
		if (pItem->GetItem() == hItem)
			return pItem;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CTaskBuilderBreadcrumb::FindItem (const CString& sName, CTaskBuilderBreadcrumbItem* pParent /*NULL*/)
{
	for (int i=0; i < m_arItems.GetSize(); i++)
	{
		CTaskBuilderBreadcrumbItem* pItem = (CTaskBuilderBreadcrumbItem*) m_arItems.GetAt(i);
		if	(
				(!pParent || pParent->GetItem() == pItem->GetParentItem()) && 
				pItem->GetName().CompareNoCase(sName) == 0 
			)
			return pItem;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CTaskBuilderBreadcrumbItem* CTaskBuilderBreadcrumb::AddItem
		(
			const CString& sName, 
			const CString& sText, 
			CTaskBuilderBreadcrumbItem* pParent/* NULL*/
		)
{
	HBREADCRUMBITEM hParentItem = pParent ? pParent->GetItem() : NULL;
	HBREADCRUMBITEM hItem =  __super::InsertItem(hParentItem, sText);
	if (!hItem)
	{
		ASSERT_TRACE(FALSE, " CTaskBuilderBreadcrumb::AddItem error: item not added");
		return NULL;
	}

	CTaskBuilderBreadcrumbItem* pItem = new CTaskBuilderBreadcrumbItem(this, sName, hParentItem, hItem);
	m_arItems.Add(pItem);
	return pItem;
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::RemoveItem	(const CString& sName, CTaskBuilderBreadcrumbItem* pParent /*NULL*/)
{
	CTaskBuilderBreadcrumbItem* pItemToRemove =  FindItem(sName, pParent);
	if (pItemToRemove)
		RemoveItem(pItemToRemove);
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::RemoveItem	(CTaskBuilderBreadcrumbItem* pItemToRemove)
{
	for (int i=m_arItems.GetUpperBound(); i >=0 ; i--)
	{
		CTaskBuilderBreadcrumbItem* pItem = (CTaskBuilderBreadcrumbItem*) m_arItems.GetAt(i);
		if	(pItemToRemove->GetItem() == pItem->GetParentItem())
		{
			delete pItem;
			m_arItems.RemoveAt(i);
		}
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::RemoveAllItems()
{
	for (int i=m_arItems.GetUpperBound(); i >=0 ; i--)
	{
		CTaskBuilderBreadcrumbItem* pItem = (CTaskBuilderBreadcrumbItem*) m_arItems.GetAt(i);
		delete pItem;
		m_arItems.RemoveAt(i);
	}
}

//-----------------------------------------------------------------------------
void CTaskBuilderBreadcrumb::SetImage (HBREADCRUMBITEM hItem, HICON image)
{
	if (!m_pImageList)
	{
		m_pImageList = new CImageList();
		m_pImageList->Create(16,16, ILC_COLOR32, 1,1);
		SetImageList(m_pImageList);
	}
	int nIdx = m_pImageList->Add(image);
	__super::SetItemImageIndex(hItem, nIdx);
}
	
//-----------------------------------------------------------------------------
/*static*/ TCHAR CTaskBuilderBreadcrumb::GetDelimiter()
{
	return szDelimiter;
}

//------------------------------------------------------------------------------
LRESULT CTaskBuilderBreadcrumb::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}


