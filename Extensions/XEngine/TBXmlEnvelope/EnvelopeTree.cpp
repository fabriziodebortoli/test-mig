
#include "stdafx.h"

#include <afxcmn.h>

#include "EnvelopeTree.h"
#include "XMLEnvMng.hjson" //JSON AUTOMATIC UPDATE

static const TCHAR szPending[] = _T(" (Pending) ");



/////////////////////////////////////////////////////////////////////////////
// CEnvelopeTree

BEGIN_MESSAGE_MAP(CEnvelopeTree, CTBTreeCtrl)
	//{{AFX_MSG_MAP(CEnvelopeTree)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CEnvelopeTree::CEnvelopeTree(CXMLEnvClassArray* pXMLEnvClassArray, BOOL bMultiSelect /*TRUE*/)
{
	m_bMultiSelect = bMultiSelect;

	ASSERT_VALID(pXMLEnvClassArray);
	m_pXMLEnvClassArray = pXMLEnvClassArray;
}

//---------------------------------------------------------------------------
void CEnvelopeTree::SelectAll(HTREEITEM hItem, UINT uSelState, BOOL bIsRoot /*=TRUE*/)
{
	while (hItem) 
	{
		SetItemState(hItem, uSelState, TVIS_SELECTED);
		SelectAll(GetChildItem(hItem), uSelState, FALSE);
		if (bIsRoot) break;
		hItem = GetNextSiblingItem(hItem);
	}
}

//---------------------------------------------------------------------------
void CEnvelopeTree::OnLButtonDown(UINT nFlags, CPoint point) 
{
	// without multiselection, site selectall does not works
	if (!m_bMultiSelect)
	{
		__super::OnLButtonDown(nFlags, point);
		return;
	}

	UINT uFlags2;
	HTREEITEM hItem = HitTest(point, &uFlags2);

	if (hItem == NULL || !(TVHT_ONITEM & uFlags2))
		return;

	SetFocus();
	
	UINT itemState = GetItemState(hItem, TVIS_SELECTED);
	SelectAll(hItem, itemState & TVIS_SELECTED ? 0 : TVIS_SELECTED);
}

//---------------------------------------------------------------------------
void CEnvelopeTree::DeselectAll()
{
	HTREEITEM hRootItem = GetRootItem();
	while(hRootItem)
	{
		SelectAll(hRootItem, 0);
		hRootItem = GetNextSiblingItem(hRootItem);
	}

}

//---------------------------------------------------------------------------
BOOL CEnvelopeTree::GetEnvSelArray(CXMLEnvElemArray* pEnvArray)
{
	if (!pEnvArray)
		return FALSE;
	
	pEnvArray->RemoveAll ();

	CHTreeItemsArray HItemsArray;

	GetSelectedHItems(&HItemsArray, GetRootItem());

	int size = HItemsArray.GetSize();

	for(int i = 0 ; i < size ; i++)
	{
		HTREEITEM hItem = HItemsArray.GetAt(i)->m_hItem;

		if (
			hItem												&&
			GetItemData(hItem)									&&
			((CObject*)GetItemData(hItem))->GetRuntimeClass()	&&
			((CObject*)GetItemData(hItem))->IsKindOf(RUNTIME_CLASS( CXMLEnvElem ))
		  ) 
		{
			CXMLEnvElem* pEnvElem = new CXMLEnvElem(*(CXMLEnvElem*)GetItemData(hItem));
			pEnvArray->Add(pEnvElem);
		}
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CEnvelopeTree::GetSelectedHItems(CHTreeItemsArray* pHItemsArray, HTREEITEM hItem)
{
	if (!pHItemsArray)
		return;

	if (!hItem)
		return;

	HTREEITEM hSib = hItem;
	
	// controlla il nodo corrente ed i suoi fratelli
	while(hSib)
	{
		// se e' una foglia, lo aggiunge solo se selezionato
		if (!ItemHasChildren(hSib))
		{
			if (GetItemState(hSib, TVIS_SELECTED) == TVIS_SELECTED)
				pHItemsArray->Add(new CHTreeItem(hSib));
		}
		else //se non e' una foglia, cicla sui figli
			GetSelectedHItems(pHItemsArray, GetChildItem(hSib));
		
		hSib = GetNextSiblingItem(hSib);
	}
	
}

/////////////////////////////////////////////////////////////////////////////
// CRxEnvelopeTree
//---------------------------------------------------------------------------
CRxEnvelopeTree::CRxEnvelopeTree(CXMLEnvClassArray* pXMLEnvClassArray, BOOL bMultiSelect /*TRUE*/)
	:
	CEnvelopeTree(pXMLEnvClassArray, bMultiSelect)
{
}

//---------------------------------------------------------------------------
void CRxEnvelopeTree::InitializeImageList()
{
	HICON hIcon[3];
	int n;

	m_ImageList.Create(16, 16, ILC_COLOR32, 3, 3);

	m_ImageList.SetBkColor(::GetSysColor(COLOR_WINDOW));

	hIcon[0] = AfxGetApp()->LoadIcon(IDI_ENV_CLASS);
	hIcon[1] = AfxGetApp()->LoadIcon(IDI_SITE);
	hIcon[2] = AfxGetApp()->LoadIcon(IDI_ENVELOPE);
	
	for(n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}

	SetImageList(&m_ImageList, TVSIL_NORMAL);
}

//---------------------------------------------------------------------------
BOOL CRxEnvelopeTree::FillTree()
{
	if (!m_pXMLEnvClassArray)
		return FALSE;

	InitializeImageList();

	HTREEITEM hRoot;
	HTREEITEM hSite; 
	HTREEITEM hEnv;

	for(int nEnvClass = 0; nEnvClass < m_pXMLEnvClassArray->GetSize() ; nEnvClass++)
	{
		CXMLEnvClassElem* pXMLEnvClassElem = m_pXMLEnvClassArray->GetAt(nEnvClass);

		if (!pXMLEnvClassElem || 
			!pXMLEnvClassElem->m_pEnvSiteArray ||
			!pXMLEnvClassElem->m_pEnvSiteArray->GetSize () )
			continue;

		CString strDescription = pXMLEnvClassElem->m_strEnvClass;
		if (pXMLEnvClassElem->m_bIsPending)
			strDescription += szPending;
		 
		hRoot = InsertItem(strDescription, 0,0, TVI_ROOT, TVI_LAST);

		SetItemData(hRoot, (DWORD)pXMLEnvClassElem);

		Expand(hRoot, TVE_EXPAND);

		int nEnvSiteSize = pXMLEnvClassElem->m_pEnvSiteArray->GetSize();

		for(int nEnvSite = 0 ; nEnvSite < nEnvSiteSize ; nEnvSite++)
		{
			CXMLEnvSiteElem* pXMLEnvSiteElem = pXMLEnvClassElem->m_pEnvSiteArray->GetAt(nEnvSite);
			if (!pXMLEnvSiteElem || 
				!pXMLEnvSiteElem->m_pEnvElemArray ||
				!pXMLEnvSiteElem->m_pEnvElemArray->GetSize () )
				continue;

			hSite = InsertItem(pXMLEnvSiteElem->m_strSiteName, 1, 1, hRoot, TVI_LAST);

			SetItemData(hSite, (DWORD)pXMLEnvSiteElem);

			if (!pXMLEnvSiteElem->m_pEnvElemArray)
				continue;

			Expand(hSite, TVE_EXPAND);
			
			int nEnvNum = pXMLEnvSiteElem->m_pEnvElemArray->GetSize();

			for(int nEnv = 0 ; nEnv < nEnvNum ; nEnv++)
			{
				CXMLEnvElem* pXMLEnvElem = pXMLEnvSiteElem->m_pEnvElemArray->GetAt(nEnv);

				if (!pXMLEnvElem)
					continue;

				hEnv = InsertItem(pXMLEnvElem->m_strEnvName, 2, 2, hSite, TVI_LAST);

				SetItemData(hEnv, (DWORD)pXMLEnvElem);
			}
		}
	}	

	SetFocus();
	DeselectAll();

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// CTxEnvelopeTree

//---------------------------------------------------------------------------
CTxEnvelopeTree::CTxEnvelopeTree(CXMLEnvClassArray* pXMLEnvClassElem)
	:
	CEnvelopeTree(pXMLEnvClassElem)
{
}

//---------------------------------------------------------------------------
void CTxEnvelopeTree::InitializeImageList()
{
	HICON hIcon[2];
	int n;

	m_ImageList.Create(16, 16, ILC_COLOR32, 2, 2);

	m_ImageList.SetBkColor(::GetSysColor(COLOR_WINDOW));

	hIcon[0] = AfxGetApp()->LoadIcon(IDI_ENV_CLASS);
	hIcon[1] = AfxGetApp()->LoadIcon(IDI_ENVELOPE);
	
	for(n = 0 ; n < 2 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}

	SetImageList(&m_ImageList, TVSIL_NORMAL);
}

//---------------------------------------------------------------------------
BOOL CTxEnvelopeTree::FillTree()
{
	if (!m_pXMLEnvClassArray)
		return FALSE;

	InitializeImageList();

	HTREEITEM hRoot;
	HTREEITEM hEnv;

	for(int nEnvClass = 0; nEnvClass < m_pXMLEnvClassArray->GetSize() ; nEnvClass++)
	{
		CXMLEnvClassElem* pXMLEnvClassElem = m_pXMLEnvClassArray->GetAt(nEnvClass);

		if (!pXMLEnvClassElem || 
			!pXMLEnvClassElem->m_pEnvElemArray ||
			!pXMLEnvClassElem->m_pEnvElemArray->GetSize () )
			continue;

		CString strDescription = pXMLEnvClassElem->m_strEnvClass;
		 
		hRoot = InsertItem(strDescription, 0,0, TVI_ROOT, TVI_LAST);

		SetItemData(hRoot, (DWORD)pXMLEnvClassElem);

		Expand(hRoot, TVE_EXPAND);

		
		int nEnvNum = pXMLEnvClassElem->m_pEnvElemArray->GetSize();
		for(int nEnv = 0 ; nEnv < nEnvNum ; nEnv++)
		{
			CXMLEnvElem* pXMLEnvElem = pXMLEnvClassElem->m_pEnvElemArray->GetAt(nEnv);

			if (!pXMLEnvElem)
				continue;

			hEnv = InsertItem(pXMLEnvElem->m_strEnvName, 1, 1, hRoot, TVI_LAST);

			SetItemData(hEnv, (DWORD)pXMLEnvElem);
		}
	}	

	SetFocus();
	DeselectAll();

	return TRUE;
}


