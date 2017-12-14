#include "stdafx.h"
#include <atlimage.h>
#include <afxcmn.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\Generalfunctions.h>

#include "TbTreeCtrl.h"

//includere come ulo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CTBTreeCtrl	/*BCGP*/
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CTBTreeCtrl, CBCGPTreeCtrl)

BEGIN_MESSAGE_MAP(CTBTreeCtrl, CBCGPTreeCtrl)

	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()

	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)

END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CTBTreeCtrl::CTBTreeCtrl()
	:
	m_hOldItem(NULL),
	m_dwOldTime(0),
	m_bMultiSelect(FALSE),
	m_bMultiSelectCustom(FALSE),
	m_MaxLevel (-1)
{
	m_HItemLevel.SetOwns(FALSE);
	///BCGP 
	CBCGPTreeCtrl::m_bVisualManagerStyle = TRUE;
}

//----------------------------------------------------------------------------
CTBTreeCtrl::~CTBTreeCtrl()
{
	m_ImageList.DeleteImageList();
}

//-----------------------------------------------------------------------------
BOOL CTBTreeCtrl::SubclassDlgItem(UINT IDC, CWnd* pParent)
{
	if (!__super::SubclassDlgItem(IDC, pParent))
		return FALSE;

	InitSizeInfo(this);

	return TRUE;
}

//------------------------------------------------------------------------------
LRESULT CTBTreeCtrl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//----------------------------------------------------------------------------
void CTBTreeCtrl::InitializeImageList()
{
	//da reimplementare nella classe derivata
	ASSERT_TRACE(FALSE, "This method must be overriden in the derived class");
}

//-------------------------------------------------------------------------------
HTREEITEM CTBTreeCtrl::InsertItem
						(
							const CString&	strText,
							int				nImage,			/*= 0*/
							int				nSelectedImage,	/*= 0*/
							HTREEITEM		hParentItem,	/*= TVI_ROOT*/
							HTREEITEM		hInsertAfter	/*= TVI_LAST*/
						)
{
	HTREEITEM hItem = __super::InsertItem(strText, nImage, nSelectedImage, hParentItem, hInsertAfter);
	if (!hItem)
		return 0;

	if (GetItemLevel(hItem) > m_MaxLevel)
		m_MaxLevel = GetItemLevel(hItem);

	return hItem;
}

//----------------------------------------------------------------------------
BOOL CTBTreeCtrl::DeleteItem(_In_ HTREEITEM hItem)
{
	return __super::DeleteItem(hItem);
}

//----------------------------------------------------------------------------
void CTBTreeCtrl::RemoveTreeChilds(HTREEITEM hItem)
{
	if (!hItem)
		return;

	HTREEITEM htParent = GetParentItem(hItem);

	m_bDeleting = TRUE;

	HTREEITEM hFirst = GetNextItem(hItem, TVGN_CHILD);

	if (!hFirst || hFirst == hItem)
	{
		m_bDeleting = FALSE;
		SelectItem(htParent);
		return;
	}

	HTREEITEM hNode;
	while (TRUE)
	{
		try {
			hNode = GetNextItem(hFirst, TVGN_NEXT);
			if (!hNode)
				break;
			DeleteItem(hNode);
		}
		catch(...){
			continue;
		}		
	}

	if (hFirst)
		DeleteItem(hFirst);

	m_bDeleting = FALSE;
	SelectItem(htParent);
}

//----------------------------------------------------------------------------
int CTBTreeCtrl::GetItemLevel(HTREEITEM hItem)
{
	if (!hItem)
		return -1;

	int nLevel = 0;

	HTREEITEM hParent = 0;

	while (hParent = GetParentItem(hItem))
	{
		nLevel++;
		hItem = hParent;
	}

	return nLevel;
}

//----------------------------------------------------------------------------
void CTBTreeCtrl::GetSelectedStrings(CStringArray* pStringArray, HTREEITEM hItem)
{
	if (!pStringArray)
		return;

	if (!hItem)
		return;

	CHTreeItemsArray HItemsArray;

	GetSelectedHItems(&HItemsArray, hItem);

	int size = HItemsArray.GetSize();

	for (int i = 0; i < size; i++)
		pStringArray->Add(GetItemText(HItemsArray.GetAt(i)->m_hItem));
}

//----------------------------------------------------------------------------
void CTBTreeCtrl::GetSelectedHItems(CHTreeItemsArray* pHItemsArray, HTREEITEM hItem /* =NULL */)
{
	//	SetFocus();
	if (!pHItemsArray)
		return;

	if (!hItem)
		hItem = GetRootItem();

	if (!hItem)
		return;

	int nLev = GetItemLevel(hItem);

	CString str = GetItemText(hItem);

	if (GetItemState(hItem, TVIS_SELECTED) & TVIS_SELECTED)
		pHItemsArray->Add(new CHTreeItem(hItem));

	if (ItemHasChildren(hItem))
		GetSelectedHItems(pHItemsArray, GetChildItem(hItem));

	HTREEITEM hSib = hItem;

	while (hSib = GetNextSiblingItem(hSib))
	{
		CString str2 = GetItemText(hSib);

		if (GetItemState(hSib, TVIS_SELECTED) & TVIS_SELECTED)
			pHItemsArray->Add(new CHTreeItem(hSib));

		if (ItemHasChildren(hSib))
			GetSelectedHItems(pHItemsArray, GetChildItem(hSib));
	}
}


//---------------------------------------------------------------------------
void CTBTreeCtrl::DeselectLevel(int nLevel)
{
	if (nLevel < 0 || nLevel > m_MaxLevel)
		return;

	HTREEITEM hItem = GetRootItem();

	if (!hItem)
		return;

	while ((nLevel > GetItemLevel(hItem)) && hItem)
		hItem = GetChildItem(hItem);

	CString tag = GetItemText(hItem);

	if (!hItem)
		return;

	while (hItem)
	{
		SetItemState(hItem, 0, TVIS_SELECTED);
		hItem = GetNextSiblingItem(hItem);
	}
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::OnRButtonDown(UINT nFlags, CPoint point)
{
	UINT uFlags2;
	HTREEITEM hItem = HitTest(point, &uFlags2);

	if (!CanSelect(hItem))
		return;

	CHTreeItemsArray HItemsArray;
	GetSelectedHItems(&HItemsArray);
	//impedisce la multiselezione se con: tasto destro + CTRL
	if ((nFlags & MK_CONTROL) && m_bMultiSelect)
		return;
	//permette di selezionare con il tasto dx l'ultimo hitem selezionato in ordine di sequenza
	//ma in questo modo nn riesco + ad avere popup da multiselect con dx!!!!
	BOOL bBeetween = FALSE;

	for (int i = 0; i <= HItemsArray.GetUpperBound(); i++)
	{
		if (HItemsArray.GetAt(i)->m_hItem == hItem)
		{
			bBeetween = TRUE;
			break;
		}
	}

	__super::OnRButtonDown(nFlags, point);

	if (!bBeetween)
	{

		HTREEITEM hroot = GetRootItem();

		SelectAll(hroot, 0);

		/*BOOL b = */SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
		/*if(GetItemState(hItem, TVIS_SELECTED) & (TVIS_SELECTED) )
		b= FALSE;
		GetSelectedHItems(& HItemsArray);*/
	}
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTBTreeCtrl::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{

}

//--------------------------------------------------------------------------
//virtuale
void CTBTreeCtrl::OnRename()
{

}

//---------------------------------------------------------------------------
HTREEITEM CTBTreeCtrl::GetLastChild (HTREEITEM htItem)
{
	if (!ItemHasChildren(htItem))
		return NULL;

	HTREEITEM htLastChildItem = NULL;

	HTREEITEM htChildItem = GetChildItem(htItem);
	for (;htChildItem != NULL; htChildItem = GetNextItem(htChildItem, TVGN_NEXT))
	{
		htLastChildItem = htChildItem;
	}

	return htLastChildItem;
}

//--------------------------------------------------------------------------
BOOL CTBTreeCtrl::IsEqualItemData(DWORD dwItemData, DWORD dwExternalData)
{
	return dwItemData == dwExternalData;
}

//--------------------------------------------------------------------------
HTREEITEM CTBTreeCtrl::FindItemData(DWORD dwFindData, HTREEITEM htItem /*= TVI_ROOT*/, BOOL bFindNextSibling/* = FALSE*/)
{
	DWORD dwItemData = GetItemData(htItem);

	if (IsEqualItemData(dwItemData, dwFindData))
		return htItem;

	if (ItemHasChildren(htItem))
	{
		for (HTREEITEM htChildItem = GetChildItem(htItem); htChildItem != NULL; htChildItem = GetNextItem(htChildItem, TVGN_NEXT))
		{
			HTREEITEM htFoundItem = FindItemData(dwFindData, htChildItem, FALSE);
			if (htFoundItem)
				return htFoundItem;
		}
	}

	if (bFindNextSibling)
	{
		while (htItem = GetNextSiblingItem(htItem))
		{
			HTREEITEM htFoundItem = FindItemData(dwFindData, htItem, FALSE);
			if (htFoundItem)
				return htFoundItem;
		}
	}

	return NULL;
}

//---------------------------------------------------------------------------
HTREEITEM	CTBTreeCtrl::FindItemText(const CString& str, HTREEITEM hCurrentItem)
{
	if (hCurrentItem == NULL)
		hCurrentItem = GetSelectedItem();
	if (hCurrentItem == NULL)
		hCurrentItem = TVI_ROOT;

	return FindItemText(str, hCurrentItem, TRUE, TRUE, TRUE);
}

HTREEITEM CTBTreeCtrl::FindItemText(const CString& str, const HTREEITEM htItem, BOOL bFindNextSibling, BOOL bFindParentNextSibling, BOOL bSkipCurrent)
{
	if (htItem == NULL)
		return NULL;

	CString sItemText;

	try {
		if (htItem != TVI_ROOT)
			sItemText = GetItemText(htItem);
	} 
	catch (...) 
	{
		ASSERT(FALSE);
	}
	if (htItem == TVI_ROOT)
		goto l_children;

	if (htItem != TVI_ROOT && !bSkipCurrent)
	{
		if (::WildcardMatch(sItemText, str))
			return htItem;
	}

	if (ItemHasChildren(htItem))
	{
		if  (OnFindItemTextOnFirstChild(htItem))
		{
l_children:
			for (HTREEITEM htChildItem = GetChildItem(htItem); htChildItem != NULL; htChildItem = GetNextItem(htChildItem, TVGN_NEXT))
			{
				if (OnFindItemTextOnChild(htChildItem, str))
					return htChildItem;
				
				HTREEITEM htFoundItem = FindItemText(str, htChildItem, FALSE, FALSE, FALSE);
				if (htFoundItem)
					return htFoundItem;
			}
			OnFindItemTextOnLastChild(htItem);
		}

	}

	if (bFindNextSibling && htItem != TVI_ROOT)
	{
		HTREEITEM htSibling = htItem;
		while (htSibling = GetNextSiblingItem(htSibling))
		{
			HTREEITEM htFoundItem = FindItemText(str, htSibling, FALSE, FALSE, FALSE);
			if (htFoundItem)
				return htFoundItem;
		}
	}

	if (bFindParentNextSibling && htItem != TVI_ROOT)
	{
		//risale nella struttura del tree
		HTREEITEM htParent = htItem;
		while (htParent = GetParentItem(htParent))
		{
			HTREEITEM htNextParent = GetNextSiblingItem(htParent);
			if (htNextParent)
			{
				return FindItemText(str, htNextParent, TRUE, TRUE, FALSE);
			}
		}
	}

	return NULL;
}

//---------------------------------------------------------------------------
/* NON funziona perchè non sposta i children

HTREEITEM CTBTreeCtrl::Move(HTREEITEM htCurrent, BOOL bNext)
{
	UINT mask = TVIF_HANDLE | TVIF_TEXT | TVIF_STATE | TVIF_IMAGE | TVIF_SELECTEDIMAGE | TVIF_CHILDREN | TVIS_SELECTED | TVIS_EXPANDED | TVIS_BOLD | TVIF_STATEEX;
	// we want to retrieve all attributest, but also specify TVIF_HANDLE
	// because we're getting the item by its handle.
	TVITEM sCurrentItem;
	sCurrentItem.hItem = htCurrent;
	sCurrentItem.mask = mask;
	TCHAR szCurrentText[1024];
	sCurrentItem.cchTextMax = 1024;
	sCurrentItem.pszText = szCurrentText;

	BOOL bOk = GetItem(&sCurrentItem);
	if (!bOk)
		return NULL;
	DWORD dwCurrentItemData = GetItemData(htCurrent);

	// Try to get the next item
	HTREEITEM htNewPosItem = bNext ? GetNextSiblingItem(htCurrent) : GetPrevSiblingItem(htCurrent);
	if (!htNewPosItem)
		return NULL;

	TVITEM sNewPosItem;
	sNewPosItem.hItem = htNewPosItem;
	sNewPosItem.mask = mask;
	TCHAR szNewPosItemText[1024];
	sNewPosItem.cchTextMax = 1024;
	sNewPosItem.pszText = szNewPosItemText;

	bOk = GetItem(&sNewPosItem);
	if (!bOk)
		return NULL;

	DWORD dwPrevItemData = GetItemData(htNewPosItem);

	//----
	sNewPosItem.hItem = htCurrent;
	bOk = SetItem(&sNewPosItem);
	if (!bOk)
		return NULL;
	SetItemData(htCurrent, dwPrevItemData);

	//----
	sCurrentItem.hItem = htNewPosItem;
	bOk = SetItem(&sCurrentItem);
	if (!bOk)
		return NULL;
	SetItemData(htNewPosItem, dwCurrentItemData);

	return htNewPosItem;
}
*/

//---------------------------------------------------------------------------
void CTBTreeCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	//considera anche di usare questa alternativa: http://www.codeguru.com/cpp/controls/treeview/misc-advanced/article.php/c723/Allowing-multiple-selection.htm

	SetFocus();
	UINT uFlags2;
	HTREEITEM hItem = HitTest(point, &uFlags2);

	m_bMultiSelectCustom = FALSE;

	if (hItem == NULL || !CanSelect(hItem))
		return;

	if (m_bMultiSelect && ((nFlags & MK_CONTROL) || (nFlags & MK_SHIFT)))
	{

		CHTreeItemsArray hItemsArray;
		GetSelectedHItems(&hItemsArray);

		if ((nFlags & MK_CONTROL) && (nFlags &MK_SHIFT))
		{
			if (m_hOldItem)
				SelectItems(m_hOldItem, hItem, FALSE);
		}
		else if (nFlags & MK_CONTROL)
		{

			if (!CanSelectLevel(hItem))
				return;

			if ((hItem != NULL) && (TVHT_ONITEM | TVHT_ONITEMRIGHT & uFlags2))
			{
				SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
				SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED); //?
				m_HItemLevel.Add((CLevelGrantItem*)hItem);

				if (!hItemsArray.Contains(hItem))
				{
					hItemsArray.Add(new CHTreeItem(hItem));
					SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
				}
				else if (hItemsArray.GetSize() > 1)
				{
					hItemsArray.Remove(hItem);
					SetItemState(hItem, 0, TVIS_SELECTED);
				}
			}
			//aggiorno l'olditem
			m_hOldItem = hItem;
		}
		else if (nFlags & MK_SHIFT)
		{ 
			if (m_hOldItem)
				SelectItems(m_hOldItem, hItem);	
		}
		

		GetSelectedHItems(&hItemsArray);
		

		//se è il primo elemento della multiselezione, devo aggiungere anche l'elemento precedente (in prima posizione è meglio per la descr delle colonne)
		if (hItemsArray.GetSize() == 1 && m_hOldItem != NULL && !hItemsArray.Contains(m_hOldItem))
		{
			hItemsArray.Add(new CHTreeItem(m_hOldItem));
			hItemsArray.Swap(0, 1);
		}

		//se ho più di 1 elemento eventualmente attivo la multiselezione custom
		if (/*m_bMultiSelectCustom && */hItemsArray.GetSize() > 1)
		{
			if (OnMultiSelect())
			{
			   m_bMultiSelectCustom = TRUE;
			   return;
			}
				
		}
	}

	HTREEITEM hRoot = GetRootItem();

	SelectAll(hRoot, 0);

	//MULTISEL 
	//SelectItem(NULL); //l'avevamo aggiunta per il problema del HitTest che non riconosce un item o per la riselezione del nodo corrente

	__super::OnLButtonDown(nFlags, point);



#ifdef _DEBUG
try {
	CString str;
	if (hItem && hItem != TVI_ROOT)
		str = GetItemText(hItem);
} catch(...) {}
#endif

	//per riselezionare lo stesso elemento precedentemente selezionato
	if ((hItem != NULL) && (TVHT_ONITEM & uFlags2))
		//è stato aggiunto questo if (rispetto alla versione precedente) per evitare di selezionare a video,
		//cose che non vengono selezionate di fatto, poichè se il click non viene fatto TVHT_ONITEM, non viene effettuata la notifica TVN_SELCHANGED
		SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);

	DWORD newTime = GetTickCount();

	if (m_dwOldTime != 0 && m_hOldItem != NULL && m_hOldItem == hItem && newTime > m_dwOldTime + 200 && newTime < m_dwOldTime + 500)
	{
		//return;
		OnRename();

		m_hOldItem = NULL;
		m_dwOldTime = 0;
	}
	else
	{
		m_dwOldTime = newTime;
		m_hOldItem = hItem;
	}
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::ToggleItemSelect(HTREEITEM hItem)
{
	if (!(GetItemState(hItem, TVIS_SELECTED) & TVIS_SELECTED))
		SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
	else
		SetItemState(hItem, 0, TVIS_SELECTED);
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::DeselectItem(HTREEITEM hItem)
{
	if (GetItemState(hItem, TVIS_SELECTED) & TVIS_SELECTED)
		SetItemState(hItem, 0, TVIS_SELECTED);
}

// SelectItems	- Selects items from hItemFrom to hItemTo. Does not
//		- select child item if parent is collapsed. Removes
//		- selection from all other items
// hItemFrom	- item to start selecting from
// hItemTo	- item to end selection at.
BOOL CTBTreeCtrl::SelectItems(HTREEITEM hItemFrom, HTREEITEM hItemTo, BOOL bRemovesOtherSelection)
{
	if (bRemovesOtherSelection)
	{
		HTREEITEM hItem = GetRootItem();

		// Clear selection upto the first item
		while (hItem && hItem != hItemFrom && hItem != hItemTo)
		{
			hItem = GetNextVisibleItem(hItem);
			SetItemState(hItem, 0, TVIS_SELECTED);
		}

		if (!hItem)
			return FALSE;	// Item is not visible

		SelectItem(hItemTo);

		// Rearrange hItemFrom and hItemTo so that hItemFirst is at top
		if (hItem == hItemTo)
		{
			hItemTo = hItemFrom;
			hItemFrom = hItem;
		}


		// Go through remaining visible items
		BOOL bSelect = TRUE;
		while (hItem)
		{
			// Select or remove selection depending on whether item
			// is still within the range.
			SetItemState(hItem, bSelect ? TVIS_SELECTED : 0, TVIS_SELECTED);

			// Do we need to start removing items from selection
			if (hItem == hItemTo)
				bSelect = FALSE;

			hItem = GetNextVisibleItem(hItem);
		}

		return TRUE;
	}
	else
	{
		if (!hItemTo || !hItemFrom)
			return FALSE;	// Item is not visible

		//ordino i due item in modo da avere il from come primo (in alto)--------------------------

		HTREEITEM hFromParent = GetParentItem(hItemFrom);
		HTREEITEM hToParent = GetParentItem(hItemTo);

		if (hFromParent != hToParent)
			return FALSE;

		HTREEITEM hChildItem = GetChildItem(hFromParent);
		BOOL bSelectionUpDown = TRUE;
		
		while (hChildItem)
		{
			if (hChildItem == hItemFrom)
			{
				bSelectionUpDown = TRUE;
				break;
			}
			if (hChildItem == hItemTo)
			{
				bSelectionUpDown = FALSE;
				break;
			}
			
			hChildItem = GetNextVisibleItem(hChildItem);
		}
		//---------------------------------------------------------------------------------------

		HTREEITEM hItem = hItemFrom;
		SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);

		while (hItem && hItem != hItemTo && hItem != hFromParent)
		{
			if(bSelectionUpDown)
				hItem = GetNextVisibleItem(hItem);
			else
				hItem = GetPrevVisibleItem(hItem);

			SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
		}
		return TRUE;
	}
}

void CTBTreeCtrl::ClearSelection()
{
	this->SelectItem(NULL); //consente di riselezionare l'ultimo elemento 
	
	for (HTREEITEM hItem = GetRootItem(); hItem != NULL; hItem = TbGetNextItem(hItem))
	{
		CString dtr = GetItemText(hItem);
		if ((GetItemState(hItem, TVIS_SELECTED) & TVIS_SELECTED) != 0)
		{
		   SetItemState(hItem, UINT(~TVIS_SELECTED), TVIS_SELECTED);
		   
		}
			
	}
		
}

HTREEITEM CTBTreeCtrl::TbGetNextItem(HTREEITEM hItem)
{
	HTREEITEM   hti;

	if (ItemHasChildren(hItem))
		return GetChildItem(hItem); // return first child
	else {
		// return next sibling item
		// Go up the tree to find a parent's sibling if needed.
		while ((hti = GetNextSiblingItem(hItem)) == NULL) {
			if ((hItem = GetParentItem(hItem)) == NULL)
				return NULL;
		}
	}
	return hti;
}

//---------------------------------------------------------------------------
BOOL CTBTreeCtrl::CanSelectLevel(HTREEITEM hItem)
{
	int nLevHitemSel = GetItemLevel(hItem);

	CHTreeItemsArray aHTreeItemsArray;
	GetSelectedHItems(&aHTreeItemsArray);

	for (int i = 0; i <= aHTreeItemsArray.GetUpperBound(); i++)
	{
		int nLev = GetItemLevel(aHTreeItemsArray.GetAt(i)->m_hItem);
		if (nLev != nLevHitemSel)
			return FALSE;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::SelectAll(HTREEITEM hItem, UINT uSelState)
{
	while (hItem)
	{
		if (uSelState == TVIS_SELECTED /*&& GetItemLevel(hItem) == m_MaxLevel*/)
		{
			SetItemState(hItem, TVIS_SELECTED, TVIS_SELECTED);
		}
		else
		{
			SetItemState(hItem, 0, TVIS_SELECTED);
		}

		SelectAll(GetChildItem(hItem), uSelState);
		hItem = GetNextSiblingItem(hItem);
	}
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::ExpandAll(HTREEITEM hItem, UINT nCode)
{
	Expand(hItem, nCode);
	HTREEITEM hChildItem = GetChildItem(hItem);
	while (hChildItem)
	{
		ExpandAll(hChildItem, nCode);
		hChildItem = GetNextSiblingItem(hChildItem);
	}
}

//---------------------------------------------------------------------------
void CTBTreeCtrl::ExpandAll(UINT nCode)
{
	HTREEITEM hItem = GetRootItem();
	while (hItem)
	{
		Expand(hItem, nCode);
		HTREEITEM hChildItem = GetChildItem(hItem);
		while (hChildItem)
		{
			ExpandAll(hChildItem, nCode);
			hChildItem = GetNextSiblingItem(hChildItem);
		}
		hItem = GetNextSiblingItem(hItem);
	}
}

//-----------------------------------------------------------------------------
LRESULT CTBTreeCtrl::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndTreeCtrlDescription* pTreeDesc = (CWndTreeCtrlDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndTreeCtrlDescription), strId);
	pTreeDesc->UpdateAttributes(this);

	//a partire dal nodo radice chiede la descrizione di tutti i nodi in ricorsione chiamando il
	//metodo GetItemDescription
	HTREEITEM hItem = GetRootItem();
	//calcolo di quanto deve essere spostato il rettangolo del nodo figlio rispetto al nodo padre
	CRect rc;
	CPoint ptScrollOffset(0, 0);
	if (GetItemRect(hItem, &rc, TRUE))
		ptScrollOffset.Offset(-(rc.TopLeft()));
	if (hItem)
	{
		//siccome non ho il CWnd e quindi il suo handle, devo generare un Id diverso dalla convenzione usata per tutte le 
		//altre finestra
		CString strIdNode = cwsprintf(_T("%d"), hItem);
		CWndTreeNodeDescription* pNodeDescr = (CWndTreeNodeDescription*)pTreeDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndTreeNodeDescription), strIdNode);
		pNodeDescr->GetItemDescription(hItem, this, pTreeDesc, ptScrollOffset);
	}
	//Recupero icone associate ai nodi del tree dalla imagelist del tree(contine immagini normali-selezionate-disabilitate)
	CImageList* pImageList = GetImageList(TVSIL_NORMAL);
	if (pImageList && pImageList->GetImageCount())
	{
		IMAGEINFO imageInfo;
		pImageList->GetImageInfo(0, &imageInfo);
		CString sName;
		sName.AppendFormat(_T("tree%ud.png"), imageInfo.hbmImage);
		COLORREF bkColor = pImageList->GetBkColor();
		pImageList->SetBkColor(WND_IMAGE_BACK_COLOR);
		CImage aImage;
		aImage.Attach(imageInfo.hbmImage);
		if (pTreeDesc->m_ImageBuffer.Assign(&aImage, sName))
			pTreeDesc->SetUpdated(&pTreeDesc->m_ImageBuffer);
		pImageList->SetBkColor(bkColor);
		aImage.Detach();
		int nIconHeight = imageInfo.rcImage.bottom - imageInfo.rcImage.top;
		if (pTreeDesc->m_nIconHeight != nIconHeight)
		{
			pTreeDesc->m_nIconHeight = nIconHeight;
			pTreeDesc->SetUpdated(&pTreeDesc->m_nIconHeight);
		}
		int nIcons = pImageList->GetImageCount();
		if (pTreeDesc->m_nIcons != nIcons)
		{
			pTreeDesc->m_nIcons = nIcons;
			pTreeDesc->SetUpdated(&pTreeDesc->m_nIcons);
		}
	}
	return (LRESULT)pTreeDesc;
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CWndTreeCtrlDescription
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CWndTreeCtrlDescription, CWndImageDescription)

REGISTER_WND_OBJ_CLASS(CWndTreeCtrlDescription, Tree)

//-----------------------------------------------------------------------------
CWndTreeCtrlDescription::CWndTreeCtrlDescription(CWndObjDescription* pParent) :
CWndImageDescription(pParent),
m_nIconHeight(0),
m_nIcons(0),
m_bCheckBoxes(false),
m_bDisableDragDrop(false),
m_bEditLabels(false),
m_bHasButtons(false),
m_bHasLines(false),
m_bInfoTip(false),
m_bLinesAtRoot(false),
m_bNoTooltips(false),
m_bAlwaysShowSelection(false)
{
	m_Type = CWndObjDescription::Tree;
}

//-----------------------------------------------------------------------------
void CWndTreeCtrlDescription::SerializeJson(CJsonSerializer& strJson)
{
	strJson.WriteInt( szJsonIconHeight, m_nIconHeight);
	strJson.WriteInt(szJsonIcons, m_nIcons);

	SERIALIZE_BOOL(m_bCheckBoxes, szJsonCheckBoxes, false);
	SERIALIZE_BOOL(m_bDisableDragDrop, szJsonDisableDragDrop, false);
	SERIALIZE_BOOL(m_bEditLabels, szJsonEditLabels, false);
	SERIALIZE_BOOL(m_bHasButtons, szJsonHasButtons, false);
	SERIALIZE_BOOL(m_bHasLines, szJsonHasLines, false);
	SERIALIZE_BOOL(m_bInfoTip, szJsonInfoTip, false);
	SERIALIZE_BOOL(m_bLinesAtRoot, szJsonLinesAtRoot, false);
	SERIALIZE_BOOL(m_bNoTooltips, szJsonNoTooltips, true);
	SERIALIZE_BOOL(m_bAlwaysShowSelection, szJsonAlwaysShowSelection, false);

	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CWndTreeCtrlDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	if (parser.Has(szJsonIconHeight ))
		m_nIconHeight = parser.ReadInt(szJsonIconHeight);

	if (parser.Has(szJsonIcons))
		m_nIcons = parser.ReadInt(szJsonIcons);

	if (parser.Has(szJsonCheckBoxes ))
		m_bCheckBoxes = parser.ReadBool(szJsonCheckBoxes);
	if (parser.Has(szJsonDisableDragDrop))
		m_bDisableDragDrop = parser.ReadBool(szJsonDisableDragDrop);
	if (parser.Has(szJsonEditLabels))
		m_bEditLabels = parser.ReadBool(szJsonEditLabels);
	if (parser.Has(szJsonHasButtons))
		m_bHasButtons = parser.ReadBool(szJsonHasButtons);
	if (parser.Has(szJsonHasLines))
		m_bHasLines = parser.ReadBool(szJsonHasLines);
	if (parser.Has(szJsonInfoTip))
		m_bInfoTip = parser.ReadBool(szJsonInfoTip);
	if (parser.Has(szJsonLinesAtRoot))
		m_bLinesAtRoot = parser.ReadBool(szJsonLinesAtRoot);
	if (parser.Has(szJsonNoTooltips))
		m_bNoTooltips = parser.ReadBool(szJsonNoTooltips);
	if (parser.Has(szJsonAlwaysShowSelection))
		m_bAlwaysShowSelection = parser.ReadBool(szJsonAlwaysShowSelection);
}
//-----------------------------------------------------------------------------
void CWndTreeCtrlDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	CWndTreeCtrlDescription* pTreeDesc = (CWndTreeCtrlDescription*)pDesc;

	m_nIconHeight = pTreeDesc->m_nIconHeight;
	m_nIcons = pTreeDesc->m_nIcons;

	m_bCheckBoxes = pTreeDesc->m_bCheckBoxes;
	m_bDisableDragDrop = pTreeDesc->m_bDisableDragDrop;
	m_bEditLabels = pTreeDesc->m_bEditLabels;
	m_bHasButtons = pTreeDesc->m_bHasButtons;
	m_bHasLines = pTreeDesc->m_bHasLines;
	m_bInfoTip = pTreeDesc->m_bInfoTip;
	m_bLinesAtRoot = pTreeDesc->m_bLinesAtRoot;
	m_bNoTooltips = pTreeDesc->m_bNoTooltips;
	m_bAlwaysShowSelection = pTreeDesc->m_bAlwaysShowSelection;
}
//travasa gli stili Windows nelle corrispondenti proprietà
//-----------------------------------------------------------------------------
void CWndTreeCtrlDescription::UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle)
{
	__super::UpdatePropertiesFromStyle(dwStyle, dwExStyle);
	UPDATE_BOOL(m_bCheckBoxes, TVS_CHECKBOXES);
	UPDATE_BOOL(m_bDisableDragDrop, TVS_DISABLEDRAGDROP);
	UPDATE_BOOL(m_bEditLabels, TVS_EDITLABELS);
	UPDATE_BOOL(m_bHasButtons, TVS_HASBUTTONS);
	UPDATE_BOOL(m_bHasLines, TVS_HASLINES);
	UPDATE_BOOL(m_bInfoTip, TVS_INFOTIP);
	UPDATE_BOOL(m_bLinesAtRoot, TVS_LINESATROOT);
	UPDATE_BOOL(m_bNoTooltips, TVS_NOTOOLTIPS);
	UPDATE_BOOL(m_bAlwaysShowSelection, TVS_SHOWSELALWAYS);
}
//-----------------------------------------------------------------------------
void CWndTreeCtrlDescription::ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle)
{
	if (m_bCheckBoxes)
		dwStyle |= TVS_CHECKBOXES;

	if (m_bDisableDragDrop)
		dwStyle |= TVS_DISABLEDRAGDROP;

	if (m_bEditLabels)
		dwStyle |= TVS_EDITLABELS;

	if (m_bHasButtons)
		dwStyle |= TVS_HASBUTTONS;

	if (m_bHasLines)
		dwStyle |= TVS_HASLINES;

	if (m_bInfoTip)
		dwStyle |= TVS_INFOTIP;

	if (m_bLinesAtRoot)
		dwStyle |= TVS_LINESATROOT;

	if (m_bNoTooltips)
		dwStyle |= TVS_NOTOOLTIPS;

	if (m_bAlwaysShowSelection)
		dwStyle |= TVS_SHOWSELALWAYS;
	__super::ApplyStyleFromProperties(dwStyle, dwExStyle);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CTBTreeCtrl::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CTBTreeCtrl\n");
	__super::Dump(dc);
}
#endif // _DEBUG



///////////////////////////////////////////////////////////////////////////////
// Implementazione di CWndTreeNodeDescription
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CWndTreeNodeDescription, CWndImageDescription)

REGISTER_WND_OBJ_CLASS(CWndTreeNodeDescription, TreeNode)

//-----------------------------------------------------------------------------
CWndTreeNodeDescription::CWndTreeNodeDescription()
{
	m_Type = CWndObjDescription::TreeNode;
}

//-----------------------------------------------------------------------------
CWndTreeNodeDescription::CWndTreeNodeDescription(CWndObjDescription* pParent)
	:
	CWndImageDescription(pParent)
{
	m_Type = CWndObjDescription::TreeNode;
}


//Metodo che costruisce la descrizione di un Treenode. Riceve come parametri:
// - un handle HTREEITEM che serve a identificare il nodo che dovra descrivere
// - un puntatore a un CTBTreeCtrl
// - il puntarore alla descrizione del CTBTreeCtrl (serve per aggiungiere ai suoi figli la descrizione di questo nodo, nel caso sia di primo livello)
// - un point che dice quanto e' spostato (in orizzontale e verticale) il rettangolo di questo nodo ripetto al rettangolo del nodo padre
//-----------------------------------------------------------------------------
void CWndTreeNodeDescription::GetItemDescription(HTREEITEM hItem, CTBTreeCtrl* pTreeCtrl, CWndTreeCtrlDescription* pTreeDesc, CPoint ptScrollOffset)
{
	//leggo gli attributi che popolano la descrizione, se sono cambiati
	//li aggiorno e imposto lo stato della descrizione ad UPDATED (con la chiamata al metodo SetUpdated)
	CString strText = pTreeCtrl->GetItemText(hItem);
	if (m_strText != strText)
	{
		m_strText = strText;
		SetUpdated(&m_strText);
	}
	bool bExpanded = (pTreeCtrl->GetItemState(hItem, TVIS_EXPANDED) & TVIS_EXPANDED) != 0;
	if (m_bExpanded != bExpanded)
	{
		m_bExpanded = bExpanded;
		SetUpdated(&m_bExpanded);
	}
	bool bSelected = (pTreeCtrl->GetItemState(hItem, TVIS_SELECTED) & TVIS_SELECTED) != 0;
	if (m_bSelected != bSelected)
	{
		m_bSelected = bSelected;
		SetUpdated(&m_bSelected);
	}
	int nSelectedIcon = 0;
	pTreeCtrl->GetItemImage(hItem, m_nIcon, nSelectedIcon);
	if (m_nSelectedIcon != nSelectedIcon)
	{
		m_nSelectedIcon = nSelectedIcon;
		SetUpdated(&m_nSelectedIcon);
	}

	if ((pTreeCtrl->GetItemState(hItem, TVIS_STATEIMAGEMASK) & TVIS_STATEIMAGEMASK) != 0)
	{
		//N.B In tutto TB e Mago veien usato solo in 2 punti l'immagine di stato per mettere un flag al nodo,
		// quando ha valore 1. Il valore viene memorizzato con la macro INDEXTOSTATEIMAGEMASK, che effettua 
		//uno shift a sx di 12. Non ho trovato una macro inversa, quindi eseguo qui lo shift opposto per ottenere 
		//il valore originario.
		//Al lato Web passo solo un booleano che dice se visualizzare una checkmark o no a fianco al nodo
		bool bHasStateImg = (pTreeCtrl->GetItemState(hItem, TVIS_STATEIMAGEMASK) >> 12) == 1;
		if (m_bHasStateImg != bHasStateImg)
		{
			m_bHasStateImg = bHasStateImg;
			SetUpdated(&m_bHasStateImg);
		}
	}

	CRect rc;
	if (pTreeCtrl->GetItemRect(hItem, &rc, TRUE))
	{
		pTreeCtrl->ClientToScreen(&rc);
		rc.MoveToXY(rc.left + ptScrollOffset.x, rc.top + ptScrollOffset.y);
		if (GetRect() != rc)
		{
			SetRect(rc, FALSE);
		}
		bool bHasChild = TRUE == pTreeCtrl->ItemHasChildren(hItem);
		if (m_bHasChild != bHasChild)
		{
			m_bHasChild = bHasChild;
			SetUpdated(&m_bHasChild);
		}

		//se ha figli vado in ricorsione su di essi
		if (m_bHasChild)
		{
			HTREEITEM hChildItem = pTreeCtrl->GetChildItem(hItem);
			//siccome non ho il CWnd e quindi il suo handle, devo generare un Id diverso dalla convenzione usata per tutte le 
			//altre finestra
			CString strIdNode = cwsprintf(_T("%d"), hChildItem);
			if (pTreeCtrl->GetItemRect(hChildItem, &rc, TRUE))
			{
				CWndTreeNodeDescription* pChildDesc = (CWndTreeNodeDescription*)m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndTreeNodeDescription), strIdNode);
				pChildDesc->GetItemDescription(hChildItem, pTreeCtrl, NULL, ptScrollOffset);
			}
		}

		//nevigo sui fratelli del nodo corrente
		HTREEITEM hSiblingItem = pTreeCtrl->GetNextSiblingItem(hItem);
		if (hSiblingItem != NULL && pTreeCtrl->GetItemRect(hSiblingItem, &rc, TRUE))
		{
			//siccome non ho il CWnd e quindi il suo handle, devo generare un Id diverso dalla convenzione usata per tutte le 
			//altre finestra
			CString strSiblingID = cwsprintf(_T("%d"), hSiblingItem);
			CWndTreeNodeDescription* pSiblingDesc;
			CWndObjDescription* pParentDesc = GetParent();
			if (pParentDesc)
			{
				pSiblingDesc = (CWndTreeNodeDescription*)pParentDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndTreeNodeDescription), strSiblingID);
			}
			else //sono nodi di primo livello, fratelli della radice
			{
				ASSERT_TRACE(pTreeDesc, "Parameter pTreeDesc cannot be null");
				pSiblingDesc = (CWndTreeNodeDescription*)pTreeDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndTreeNodeDescription), strSiblingID);
			}

			pSiblingDesc->GetItemDescription(hSiblingItem, pTreeCtrl, NULL, ptScrollOffset);
		}
	}
}

//-----------------------------------------------------------------------------
CWndTreeNodeDescription::~CWndTreeNodeDescription()
{
}

//-----------------------------------------------------------------------------
void CWndTreeNodeDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bExpanded, szJsonExpanded, false);
	SERIALIZE_BOOL(m_bSelected, szJsonSelected, false);
	SERIALIZE_BOOL(m_bHasChild, szJsonHasChild, false);
	SERIALIZE_BOOL(m_bHasStateImg, szJsonHasStateImg, false);
	strJson.WriteInt(szJsonIcon, m_nIcon);
	strJson.WriteInt(szJsonSelectedIcon, m_nSelectedIcon);
	__super::SerializeJson(strJson);

}
//-----------------------------------------------------------------------------
void CWndTreeNodeDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	if (parser.Has(szJsonExpanded))
		m_bExpanded = parser.ReadBool(szJsonExpanded);

	if (parser.Has(szJsonSelected))
		m_bSelected = parser.ReadBool(szJsonSelected);

	if (parser.Has(szJsonHasChild))
		m_bHasChild = parser.ReadBool(szJsonHasChild);

	if (parser.Has(szJsonIcon))
		m_nIcon = parser.ReadInt(szJsonIcon);


	if (parser.Has(szJsonSelectedIcon))
		m_nSelectedIcon = parser.ReadInt(szJsonSelectedIcon);
	if (parser.Has(szJsonHasStateImg))
		m_bHasStateImg = parser.ReadBool(szJsonHasStateImg);

}
//-----------------------------------------------------------------------------
void CWndTreeNodeDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	m_bExpanded = ((CWndTreeNodeDescription*)pDesc)->m_bExpanded;
	m_bSelected = ((CWndTreeNodeDescription*)pDesc)->m_bSelected;
	m_bHasChild = ((CWndTreeNodeDescription*)pDesc)->m_bHasChild;
	m_nIcon = ((CWndTreeNodeDescription*)pDesc)->m_nIcon;
	m_nSelectedIcon = ((CWndTreeNodeDescription*)pDesc)->m_nSelectedIcon;
	m_bHasStateImg = ((CWndTreeNodeDescription*)pDesc)->m_bHasStateImg;
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CHTreeItemsArray
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
BOOL CHTreeItemsArray::Contains(HTREEITEM item)
{
	for (int i = 0; i <= Array::GetUpperBound(); i++) {
		if (GetAt(i)->m_hItem == item) 
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CHTreeItemsArray::Remove(HTREEITEM item)
{
	for (int i = 0; i <= Array::GetUpperBound(); i++)
		if (GetAt(i)->m_hItem == item)
			Array::RemoveAt(i, 1);
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CLevelGrantArray
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
BOOL CLevelGrantArray::Contains(CLevelGrantItem* item)
{
	for (int i = 0; i <= Array::GetUpperBound(); i++) {
		if (GetAt(i) == item) 
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CLevelGrantArray::Remove(CLevelGrantItem* item)
{
	for (int i = 0; i <= Array::GetUpperBound(); i++)
		if (GetAt(i) == item)
			Array::RemoveAt(i, 1);
}