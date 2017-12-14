#include "StdAfx.h"
#include "TBTreeView.h"

static long g_latestItem = 0;

//----------------------------------------------------------------------------
void CTBTreeItemArray::Swap(int i1, int i2)
{
	TBTreeItem* p1 = GetAt(i1);
	TBTreeItem* p2 = GetAt(i2);
	SetAt(i1, p2);
	p2->m_nIndexInParent = i1;
	SetAt(i2, p1);
	p1->m_nIndexInParent = i2;
}

//----------------------------------------------------------------------------
void CTBTreeItemArray::Sort(TVSORTCB* pfnTVSORTCB)
{
	m_pfnTVSORTCB = pfnTVSORTCB;
	Sort(0, GetCount()-1);
}
//----------------------------------------------------------------------------
int CTBTreeItemArray::Compare(TBTreeItem* p1, TBTreeItem* p2)
{
	if (m_pfnTVSORTCB->lpfnCompare)
		return m_pfnTVSORTCB->lpfnCompare(p1->m_Item.lParam, p2->m_Item.lParam, m_pfnTVSORTCB->lParam);
	return _tcscmp(p1->m_Item.pszText, p2->m_Item.pszText);
}
//----------------------------------------------------------------------------
BOOL CTBTreeItemArray::Sort(int nFirst, int nLast)
{
	if ((nLast - nFirst) <= 0)
		return TRUE;

    // Stack
	int nStackMax = (nLast - nFirst + 1);

	class QS_stack {

		typedef struct s_elem { int first; int last; } t_elem;

		t_elem* Stk;
		int nStkPtr;
		int nMax;

	public:
		QS_stack(int nStackMax)
		{
			Stk = new t_elem[nStackMax];
			nStkPtr = -1;
			nMax = nStackMax;
		}

		void Push (int f, int l)
		{
			if (nStkPtr >= nMax)
			{
				return;
			}
				
			nStkPtr++;
			Stk[nStkPtr].first = f;
			Stk[nStkPtr].last = l;
		}

		void Pop (int& f, int& l)
		{
			if (nStkPtr < 0)
			{
				return;
			}

			f = Stk[nStkPtr].first;
			l = Stk[nStkPtr].last;
			nStkPtr--;
		}
			
		bool IsEmpty()
		{
			return (nStkPtr < 0 || Stk == NULL);
		}
	};

	QS_stack S (nStackMax);
	//----
	int i, j;
    bool bSortCompleted = false;
	bool bDirection = true;

    do
    {
        do
        {
            i = nFirst;
            j = nLast;
            bDirection = true;

            do
            {
                //if ((nData[i] > nData[j]) == bAscend)
				if ((Compare(GetAt(i), GetAt(j)) > 0)/* == bAscend*/)
                {
					Swap(i, j);

                    bDirection = !bDirection;
                }

                if (bDirection)
                    j--;
                else
                    i++;

            } while (i < j);

            if ((i + 1) < nLast)
            {
                S.Push(i + 1, nLast);
            }
            nLast = i - 1;

        } while (nFirst < nLast);

        if (S.IsEmpty())
        {
            // No more partitions to sort, so by definition we've finished!
            bSortCompleted = true;
        }
        else
        {
            // Pop the most recently stored partition and sort that
			S.Pop(nFirst, nLast);
        }

    } while (!bSortCompleted);

    return TRUE;
}
//----------------------------------------------------------------------------
TBTreeItem::TBTreeItem(TBTreeView* pTree, TBTreeItem* pParent) : m_pTree(pTree), m_pParent(pParent), m_nIndexInParent(-1)
{
	ZeroMemory(&m_Item, sizeof (TVITEMEX));
	m_Item.hItem = (HTREEITEM)InterlockedIncrement(&g_latestItem);
	m_Item.iIntegral = 1;
}
//----------------------------------------------------------------------------
TBTreeItem::~TBTreeItem()
{
	NMTREEVIEW nmi;
	ZeroMemory(&nmi, sizeof(NMTREEVIEW));
	nmi.hdr.code = TVN_DELETEITEM;
	nmi.hdr.hwndFrom = m_pTree->GetHWND();
	nmi.hdr.idFrom = m_pTree->GetID();
	nmi.action = TVC_UNKNOWN;
	nmi.itemOld.hItem = m_Item.hItem;
	nmi.itemOld.lParam = m_Item.lParam;
	m_pTree->NotifyParent(TVN_DELETEITEM , &nmi);
	DeleteChilds();
}


//----------------------------------------------------------------------------
int TBTreeItem::RecalcChildRect(BOOL bRecursive)
{
	int deltaY = 0, start = 0;
	int depth = GetItemDepth();
	for (int i = 0; i < m_arChilds.GetCount(); i++)
	{
		TBTreeItem* pChild =  m_arChilds[i];
		if (i == 0)
			start = pChild->GetPrevVisibleBottomMargin();
		pChild->m_ItemRect.top = start + deltaY;
		pChild->m_ItemRect.bottom = pChild->m_ItemRect.top + pChild->GetItemHeight(FALSE);
		pChild->m_ItemRect.left = depth * TABULATION;
		pChild->m_ItemRect.right =  m_pTree->GetWidth();
		if (bRecursive)
			pChild->RecalcChildRect(TRUE);
		deltaY += pChild->GetItemHeight(TRUE);//aggiusto l'offset che dovrò applicare ai nodi successivi
	}
	return deltaY;
}
//----------------------------------------------------------------------------
void TBTreeItem::Expand(BOOL bExpand) 
{
	int deltaY = 0; //accumulo di tutti gli spostamenti verso il basso dovuti all'espansione

	if (bExpand) 
	{
		m_Item.state |= (TVIS_EXPANDED); 
		m_Item.state |= TVIS_EXPANDEDONCE;//imposto lo stato che tiene traccia del fatto che è già stato espanso una volta
		deltaY = RecalcChildRect(FALSE);
	}
	else
	{
		m_Item.state &= ~TVIS_EXPANDED; 
		for (int i = 0; i < m_arChilds.GetCount(); i++)
		{
			TBTreeItem* pChild = m_arChilds[i];
			deltaY -= pChild->GetItemHeight(TRUE);//aggiusto l'offset che dovrò applicare ai nodi successivi
		}
	}

	OffsetNextSiblings(deltaY);
}

//----------------------------------------------------------------------------
void TBTreeItem::OffsetChilds(int delta)
{
	for (int i = 0; i < m_arChilds.GetCount(); i++)
	{
		TBTreeItem* pChild = m_arChilds[i];
		pChild->m_ItemRect.OffsetRect(0, delta);
		pChild->OffsetChilds(delta);
	}
}

//----------------------------------------------------------------------------
void TBTreeItem::OffsetNextSiblings(int delta)
{
	if (m_pParent)
	{
		for (int i = m_nIndexInParent + 1; i < m_pParent->m_arChilds.GetCount(); i++)
		{
			TBTreeItem* pChild = m_pParent->m_arChilds[i];
			pChild->m_ItemRect.OffsetRect(0, delta);
			if (pChild->IsExpanded())
				pChild->OffsetChilds(delta);
		}
		m_pParent->OffsetNextSiblings(delta);
	}
}

//----------------------------------------------------------------------------
void TBTreeItem::Add(TBTreeItem* pItem)
{
	pItem->m_nIndexInParent = m_arChilds.Add(pItem);
	int start = pItem->GetPrevVisibleBottomMargin();
	pItem->m_ItemRect.top = start;
	pItem->m_ItemRect.bottom = pItem->m_ItemRect.top + pItem->GetItemHeight(FALSE);
	pItem->m_ItemRect.left = GetItemDepth() * TABULATION;
	pItem->m_ItemRect.right = m_pTree->GetWidth();
	
	//sposto in basso i nodi sottostanti di un delta pari al nodo aggiunto, ma solo se il nodo è espanso
	if (IsExpanded())
		pItem->OffsetNextSiblings(pItem->m_ItemRect.Height());
}
//----------------------------------------------------------------------------
void TBTreeItem::Remove(TBTreeItem* pItem)
{
	ASSERT(pItem->m_nIndexInParent >= 0 && pItem->m_nIndexInParent < m_arChilds.GetCount());
	ASSERT(m_arChilds[pItem->m_nIndexInParent] == pItem);
	
	//sposto in alto i nodi sottostanti di un delta pari al nodo aggiunto, ma solo se il nodo è espanso
	if (IsExpanded())
		pItem->OffsetNextSiblings(-pItem->GetItemHeight(TRUE));

	m_arChilds.RemoveAt(pItem->m_nIndexInParent);
	//riaggiusto gli indici degli elementi successivi
	for (int i = pItem->m_nIndexInParent; i < m_arChilds.GetCount(); i++)
		m_arChilds[i]->m_nIndexInParent = i;
	pItem->m_nIndexInParent = -1;
}
//----------------------------------------------------------------------------
void TBTreeItem::InsertAt(int index, TBTreeItem* pItem)
{
	m_arChilds.InsertAt(index, pItem);
	pItem->m_nIndexInParent = index;
	//riaggiusto gli indici degli elementi successivi
	for (int i = pItem->m_nIndexInParent; i < m_arChilds.GetCount(); i++)
		m_arChilds[i]->m_nIndexInParent = i;
	int start = pItem->GetPrevVisibleBottomMargin();
	pItem->m_ItemRect.top = start;
	pItem->m_ItemRect.bottom = pItem->m_ItemRect.top + pItem->GetItemHeight(FALSE);
	pItem->m_ItemRect.left = GetItemDepth() * TABULATION;
	pItem->m_ItemRect.right = m_pTree->GetWidth();
	//sposto in basso i nodi sottostanti di un delta pari al nodo aggiunto, ma solo se il nodo è espanso
	if (IsExpanded())
		OffsetNextSiblings(pItem->m_ItemRect.Height());
}
//----------------------------------------------------------------------------
void TBTreeItem::DeleteChilds()
{
	for (int i = 0; i < m_arChilds.GetCount(); i++)
	{
		delete m_arChilds[i];
	}
	m_arChilds.RemoveAll();
}


//----------------------------------------------------------------------------
int TBTreeItem::GetItemHeight(BOOL bWithChilds)
{
	int h = m_Item.iIntegral * m_pTree->m_nItemHeight; 
	if (bWithChilds && IsExpanded())
	{
		for (int i = 0; i < m_arChilds.GetCount(); i++)
			h += m_arChilds[i]->GetItemHeight(bWithChilds);
	}
	return h;
}
//----------------------------------------------------------------------------
int TBTreeItem::GetItemDepth()
{
	return m_pParent ? m_pParent->GetItemDepth() +1 : 0; 
}
//----------------------------------------------------------------------------
int TBTreeItem::GetPrevVisibleBottomMargin() 
{
	TBTreeItem* pPrevVisible = GetPrevVisible(); 
	return pPrevVisible ? pPrevVisible->m_ItemRect.bottom : 0; 
}
//----------------------------------------------------------------------------
TBTreeItem* TBTreeItem::GetPrevVisible()
{
	if (!m_pParent)
		return NULL;
	
	if (m_nIndexInParent == 0)//sono il primo figlio
		return m_pParent;
	//prendo il fratello che sta sopra di me
	TBTreeItem* pPrevVisible = m_pParent->m_arChilds[m_nIndexInParent-1]; 
	//se mio fratello è espanso, devo prendere l'ultimo nodo visibile (che sarà quello immediatamente sopra di me)
	while(pPrevVisible->IsExpanded())
	{
		pPrevVisible = pPrevVisible->m_arChilds[pPrevVisible->m_arChilds.GetUpperBound()];
	}
	return pPrevVisible;
}
//----------------------------------------------------------------------------
BOOL TBTreeItem::GetItemRect(BOOL onlyText, LPRECT lpRect)
{
	if (!m_pParent || !m_pParent->IsExpanded())
		return FALSE;
		
	*lpRect = m_ItemRect;
	return TRUE;
}
//----------------------------------------------------------------------------
TBTreeItem* TBTreeItem::Find(HTREEITEM item)
{
	if (GetHandle() == item)
		return this;
	for (int i = 0; i < m_arChilds.GetCount(); i++)
	{
		TBTreeItem* pItem = m_arChilds[i];
		if (pItem->GetHandle() == item)
		{
			return pItem;
		}
		pItem = pItem->Find(item);
		if (pItem)
			return pItem;
	}
	return NULL;
}
//----------------------------------------------------------------------------
void TBTreeItem::SetItem(TVITEM* pItem)
{
	/*if ((pItem->mask & TVIF_CHILDREN) == TVIF_CHILDREN)
	{
	pItem->cChildren = pTBItem->m_arChilds.GetCount() ? 1 : 0;
	}*/
	/*if ((pItem->mask & TVIF_DI_SETITEM) == TVIF_DI_SETITEM) //The tree-view control will retain the supplied information and will not request it again. This flag is valid only when processing the TVN_GETDISPINFO notification.
	{
	pItem->cChildren = pTBItem->m_arChilds.GetCount() ? 1 : 0;
	}*/
	if ((pItem->mask & TVIF_HANDLE) == TVIF_HANDLE)
	{
		m_Item.hItem = pItem->hItem;
	}
	if ((pItem->mask & TVIF_IMAGE) == TVIF_IMAGE)
	{
		m_Item.iImage = pItem->iImage;
	}
	if ((pItem->mask & TVIF_PARAM) == TVIF_PARAM)
	{
		m_Item.lParam = pItem->lParam;
	}
	if ((pItem->mask & TVIF_SELECTEDIMAGE) == TVIF_SELECTEDIMAGE)
	{
		m_Item.iSelectedImage = pItem->iSelectedImage;
	}
	if ((pItem->mask & TVIF_STATE) == TVIF_STATE)
	{
		m_Item.state = pItem->state;
		m_Item.stateMask = pItem->stateMask;
	}
	if ((pItem->mask & TVIF_TEXT) == TVIF_TEXT)
	{
		m_Item.cchTextMax = _tcslen(pItem->pszText) + 1;
		delete m_Item.pszText;
		m_Item.pszText = new TCHAR[m_Item.cchTextMax];
		_tcscpy_s( m_Item.pszText, m_Item.cchTextMax, pItem->pszText); 
	}
}
//----------------------------------------------------------------------------
void TBTreeItem::GetItem(TVITEM* pItem)
{
	if ((pItem->mask & TVIF_HANDLE) == TVIF_HANDLE)
	{
		pItem->hItem = m_Item.hItem ;
	}
	if ((pItem->mask & TVIF_CHILDREN) == TVIF_CHILDREN)
	{
		pItem->cChildren = m_arChilds.GetCount() ? 1 : 0;
	}
	/*if ((pItem->mask & TVIF_DI_SETITEM) == TVIF_DI_SETITEM) //The tree-view control will retain the supplied information and will not request it again. This flag is valid only when processing the TVN_GETDISPINFO notification.
	{
	pItem->cChildren = m_arChilds.GetCount() ? 1 : 0;
	}*/
	if ((pItem->mask & TVIF_HANDLE) == TVIF_HANDLE)
	{
		pItem->cChildren = m_arChilds.GetCount() ? 1 : 0;
	}
	if ((pItem->mask & TVIF_IMAGE) == TVIF_IMAGE)
	{
		pItem->iImage = m_Item.iImage;
	}
	if ((pItem->mask & TVIF_PARAM) == TVIF_PARAM)
	{
		pItem->lParam = m_Item.lParam;
	}
	if ((pItem->mask & TVIF_SELECTEDIMAGE) == TVIF_SELECTEDIMAGE)
	{
		pItem->iSelectedImage = m_Item.iSelectedImage;
	}
	if ((pItem->mask & TVIF_STATE) == TVIF_STATE)
	{
		pItem->state = m_Item.state;
		pItem->stateMask = m_Item.stateMask;
	}
	if ((pItem->mask & TVIF_TEXT) == TVIF_TEXT)
	{
		_tcscpy_s(pItem->pszText, pItem->cchTextMax, m_Item.pszText);
	}
}

//----------------------------------------------------------------------------
TBTreeItem* TBTreeView::Find(HTREEITEM item)
{
	return m_pRootItem->Find(item); 
}
//----------------------------------------------------------------------------
LRESULT TBTreeView::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case TVM_INSERTITEM:
		{
			TVINSERTSTRUCT* tvis = (TVINSERTSTRUCT*)lParam;
			TBTreeItem* pParent = NULL;
			if (tvis->hParent == NULL || tvis->hParent == TVI_ROOT)
			{
				TBTreeItem* pItem = new TBTreeItem(this, m_pRootItem);
				pItem->SetItem(&tvis->item);
				m_pRootItem->Add(pItem);
				return (LRESULT)pItem->GetHandle();
			}

			switch ((int)tvis->hInsertAfter)
			{
			case TVI_FIRST:
				{
					TBTreeItem*	pParent = Find(tvis->hParent);
					if (!pParent)
						return NULL;
					TBTreeItem* pItem = new TBTreeItem(this, pParent);
					pItem->SetItem(&tvis->item);
					pParent->InsertAt(0, pItem);
					return (LRESULT)pItem->GetHandle();
				}
			case TVI_LAST:
				{
					TBTreeItem*	pParent = Find(tvis->hParent);
					if (!pParent)
						return NULL;
					TBTreeItem* pItem = new TBTreeItem(this, pParent);
					pItem->SetItem(&tvis->item);
					pParent->Add(pItem);
					return (LRESULT)pItem->GetHandle();
				}
			case TVI_ROOT:
				{
					TBTreeItem* pItem = new TBTreeItem(this, pParent);
					pItem->SetItem(&tvis->item);
					m_pRootItem->Add(pItem);
					return (LRESULT)pItem->GetHandle();
				}
			case TVI_SORT:
				{
					TBTreeItem*	pParent = Find(tvis->hParent);
					if (!pParent)
						return NULL;
					TBTreeItem* pItem = new TBTreeItem(this, pParent);
					pItem->SetItem(&tvis->item);
					int i;
					for (i = 0; i < pParent->m_arChilds.GetCount(); i++)
					{
						TBTreeItem* pBrother = pParent->m_arChilds[i];
						if (_tcscmp(pBrother->m_Item.pszText, tvis->item.pszText) > 0 )
						{
							break;
						}
					}

					pParent->InsertAt(i, pItem);
					return (LRESULT)pItem->GetHandle();
				}
			default:
				{
					TBTreeItem*	pParent = Find(tvis->hParent);
					if (!pParent)
						return NULL;
					for (int i = 0; i < pParent->m_arChilds.GetCount(); i++)
						if (pParent->m_arChilds[i]->GetHandle() == tvis->hInsertAfter)
						{
							TBTreeItem* pItem = new TBTreeItem(this, pParent);
							pItem->SetItem(&tvis->item);
							pParent->InsertAt(i+1, pItem);
							return (LRESULT)pItem->GetHandle();
						}
				}
			}
			return NULL;
		}
	case TVM_SETITEM:
		{
			TVITEM* pItem = (TVITEM*) lParam;

			TBTreeItem* pTBItem = Find(pItem->hItem);
			if (!pTBItem)
				return FALSE;
			pTBItem->SetItem(pItem);
			return TRUE;
		}
	case TVM_GETITEM:
		{
			TVITEM* pItem = (TVITEM*) lParam;

			TBTreeItem* pTBItem = Find(pItem->hItem);
			if (!pTBItem)
				return FALSE;
			pTBItem->GetItem(pItem);
			return TRUE;
		}
	case TVM_SETIMAGELIST:
		{
			HIMAGELIST old = NULL;
			if (wParam == TVSIL_NORMAL)
			{
				old = m_hImageList;
				m_hImageList = (HIMAGELIST) lParam;
			}
			else if (wParam == TVSIL_STATE)
			{
				old = m_hStateImageList;
				m_hStateImageList = (HIMAGELIST) lParam;
			}

			return (LRESULT)old;
		}
	case TVM_GETIMAGELIST:
		{
			if (wParam == TVSIL_NORMAL)
				return (LRESULT)m_hImageList;
			else if (wParam == TVSIL_STATE)
				return (LRESULT)m_hStateImageList;
			else
				return NULL;
		}
	case TVM_GETNEXTITEM:
		{
			HTREEITEM hItem = (HTREEITEM)lParam;
			switch (wParam)
			{
			case TVGN_CARET: //Retrieves the currently selected item. You can use the TreeView_GetSelection macro to send this message.
				return m_pSelectedItem ? (LRESULT) m_pSelectedItem->GetHandle() : NULL;
			case TVGN_CHILD://Retrieves the first child item of the item specified by the hitem parameter. You can use the TreeView_GetChild macro to send this message.
				{
					TBTreeItem* pItem = Find(hItem);
					return (LRESULT)((pItem && pItem->m_arChilds.GetCount()) ? pItem->m_arChilds[0]->GetHandle() : NULL);
				}
			case TVGN_DROPHILITE://Retrieves the item that is the target of a drag-and-drop operation. You can use the TreeView_GetDropHilight macro to send this message.
				ASSERT(FALSE);
				return NULL;
			case TVGN_FIRSTVISIBLE: //Retrieves the first item that is visible in the tree-view window. You can use the TreeView_GetFirstVisible macro to send this message.
				ASSERT(FALSE);
				return NULL;
			case TVGN_LASTVISIBLE: //Version 4.71. Retrieves the last expanded item in the tree. This does not retrieve the last item visible in the tree-view window. You can use the TreeView_GetLastVisible macro to send this message.
				ASSERT(FALSE);
				return NULL;
			case TVGN_NEXT: //Retrieves the next sibling item. You can use the TreeView_GetNextSibling macro to send this message.
				{
					TBTreeItem* pItem = Find(hItem);
					TBTreeItem* pParent = pItem ? pItem->m_pParent : NULL;
					int idx = pItem ? pItem->m_nIndexInParent + 1 : -1;//l'indice del successivo
					if (!pParent || !pItem || idx >= pParent->m_arChilds.GetCount() )
						return NULL;
					ASSERT(pParent->m_arChilds[idx]->m_nIndexInParent == idx);
					return (LRESULT)pParent->m_arChilds[idx]->GetHandle();
				}

			case TVGN_NEXTVISIBLE: //Retrieves the next visible item that follows the specified item. The specified item must be visible. Use the TVM_GETITEMRECT message to determine whether an item is visible. You can use the TreeView_GetNextVisible macro to send this message.
				ASSERT(FALSE);
				return NULL;
			case TVGN_PARENT: //Retrieves the parent of the specified item. You can use the TreeView_GetParent macro to send this message.
				{
					TBTreeItem* pItem = Find(hItem);
					TBTreeItem* pParent = pItem ? pItem->m_pParent : NULL;
					return pParent ? (LRESULT)pParent->GetHandle() : NULL;
				}
			case TVGN_PREVIOUS: //Retrieves the previous sibling item. You can use the TreeView_GetPrevSibling macro to send this message.
				{
					TBTreeItem* pItem = Find(hItem);
					TBTreeItem* pParent = pItem ? pItem->m_pParent : NULL;
					int idx = pItem ? pItem->m_nIndexInParent - 1 : -1;//l'indice del precedente
					if (!pParent || idx < 0)
						return NULL;
					ASSERT(pParent->m_arChilds[idx]->m_nIndexInParent == idx);
					return (LRESULT)pParent->m_arChilds[idx];
				}
			case TVGN_PREVIOUSVISIBLE: //Retrieves the first visible item that precedes the specified item. The specified item must be visible. Use the TVM_GETITEMRECT message to determine whether an item is visible. You can use the TreeView_GetPrevVisible macro to send this message.
				ASSERT(FALSE);
				return NULL;
			case TVGN_ROOT: //Retrieves the topmost or very first item of the tree-view control. You can use the TreeView_GetRoot macro to send this message.
				{
					return m_pRootItem->m_arChilds.GetCount() ? (LRESULT)m_pRootItem->m_arChilds[0]->GetHandle() : NULL;
				}
			}
		}
	case TVM_DELETEITEM:
		{
			HTREEITEM hItem = (HTREEITEM)lParam;
			if (hItem == NULL || hItem==TVI_ROOT)
			{
				m_pRootItem->DeleteChilds();
				return TRUE;
			}
			TBTreeItem* pItem = Find(hItem);
			if (!pItem)
				return NULL;
			TBTreeItem* pParent = pItem->m_pParent;
			ASSERT(pParent);
			pParent->Remove(pItem);
			delete pItem;
			return TRUE;
		}
	case TVM_GETITEMRECT:
		{
			BOOL onlyText = wParam;
			LPRECT lpRect = (LPRECT)lParam;
			HTREEITEM hItem = *((HTREEITEM*)lpRect);
			TBTreeItem* pItem = Find(hItem);
			if (pItem)
				return pItem->GetItemRect(onlyText, lpRect);
			return FALSE;
		}
	case TVM_SETITEMHEIGHT:
		{
			int old = m_nItemHeight;
			int h = (int)wParam;
			if (h == -1)
			{
				m_nItemHeight = DEFAULT_ITEM_HEIGHT;
				return old;
			}
			if (h < 1)
				h = 1;

			//se lo stile non lo prevede, lo arrotondo al più vicino valore pari per difetto
			if (h%2 == 1 && (GetStyle() & TVS_NONEVENHEIGHT) != TVS_NONEVENHEIGHT)
			{
				h--;
			}
			m_nItemHeight = h;
			return old;
		}
	case TVM_GETITEMHEIGHT:
		{
			return m_nItemHeight;
		}
	case TVM_SELECTITEM:
		{
			HTREEITEM item = (HTREEITEM)lParam;
			if ((wParam & TVGN_CARET) == TVGN_CARET)
			{
				NMTREEVIEW nmi;
				ZeroMemory(&nmi, sizeof(NMTREEVIEW));
				nmi.hdr.code = TVN_SELCHANGING;
				nmi.hdr.hwndFrom = m_hWnd;
				nmi.hdr.idFrom = m_id;
				nmi.action = TVC_UNKNOWN;
				if (m_pSelectedItem)
				{
					//se avevo un nodo selezionato, lo metto nell'old; da documentazione, solo i valori assegnati sono da gestire
					nmi.itemOld.hItem = m_pSelectedItem->m_Item.hItem;
					nmi.itemOld.mask = m_pSelectedItem->m_Item.mask;
					nmi.itemOld.state = m_pSelectedItem->m_Item.state;	
					nmi.itemOld.lParam = m_pSelectedItem->m_Item.lParam;
				}
				if (!item)
				{
					//qualcuno potrebbe inibire il cambiamento
					if (NotifyParent(TVN_SELCHANGING, &nmi))
						return FALSE;
					m_pSelectedItem = NULL;
					return TRUE;
				}
				TBTreeItem* pItem = Find(item);
				if (!pItem)
					return FALSE;

				//metto le informazioni del nuovo nodo selezionato; da documentazione, solo i valori assegnati sono da gestire
				nmi.itemNew.hItem = pItem->m_Item.hItem;
				nmi.itemNew.mask = pItem->m_Item.mask;
				nmi.itemNew.state = pItem->m_Item.state;	
				nmi.itemNew.lParam = pItem->m_Item.lParam;
				//qualcuno potrebbe inibire il cambiamento
				if (NotifyParent(TVN_SELCHANGING, &nmi))
					return FALSE;
				if (m_pSelectedItem)
					m_pSelectedItem->m_Item.state &= ~TVIS_SELECTED;
				m_pSelectedItem = pItem;
				pItem->m_Item.state |= TVIS_SELECTED;

				nmi.hdr.code = TVN_SELCHANGED;
				NotifyParent(TVN_SELCHANGED, &nmi);
				return TRUE;
				//case TVGN_DROPHILITE:
				//case TVGN_FIRSTVISIBLE:
				//case TVSI_NOSINGLEEXPAND:
			}

			return FALSE;
		}
	case TVM_ENSUREVISIBLE:
		{

			return TRUE;
		}
	case TVM_EXPAND:
		{
			HTREEITEM item = (HTREEITEM)lParam;
			TBTreeItem* pItem = Find(item);
			if (!pItem)
				return FALSE;
			NMTREEVIEW nmi;
			ZeroMemory(&nmi, sizeof(NMTREEVIEW));
			nmi.hdr.code = TVN_ITEMEXPANDING;
			nmi.hdr.hwndFrom = m_hWnd;
			nmi.hdr.idFrom = m_id;
			
			nmi.itemNew.hItem = pItem->m_Item.hItem;
			nmi.itemNew.state = pItem->m_Item.state;	
			nmi.itemNew.lParam = pItem->m_Item.lParam;
			//solo la prima volta si manda la notifica
			BOOL bNotify = !pItem->IsExpandedOnce();
			if (wParam == TVE_COLLAPSE || wParam == (TVE_COLLAPSE|TVE_COLLAPSERESET))
			{
				if (!pItem->IsExpanded())
					return FALSE;
				nmi.action = TVE_COLLAPSE;
				//la notify la faccio solo
				if (bNotify && NotifyParent(TVN_ITEMEXPANDING, &nmi))
					return FALSE;

				if ((wParam & TVE_COLLAPSERESET) == TVE_COLLAPSERESET)
				{
					//resetto lo stato di expanded once
					pItem->m_Item.state &= ~TVIS_EXPANDEDONCE; 
					pItem->DeleteChilds();
				}
				nmi.hdr.code = TVN_ITEMEXPANDED;
				if (bNotify)
					NotifyParent(TVN_ITEMEXPANDED, &nmi);
				pItem->Expand(FALSE);
				return TRUE;
			}

			if (wParam == TVE_EXPAND)
			{
				if (pItem->m_arChilds.GetCount() == 0)
					return FALSE;
				nmi.action = TVE_EXPAND;
				if (bNotify && NotifyParent(TVN_ITEMEXPANDING, &nmi))
					return FALSE;
				pItem->Expand(TRUE);
				nmi.hdr.code = TVN_ITEMEXPANDED;
				if (bNotify)
					NotifyParent(TVN_ITEMEXPANDED, &nmi);
				return TRUE;
			}
			if (wParam == TVE_EXPANDPARTIAL)
			{
				if (pItem->m_arChilds.GetCount() == 0)
					return FALSE;
				nmi.action = TVE_EXPAND;
				if (bNotify && NotifyParent(TVN_ITEMEXPANDING, &nmi))
					return FALSE;
				pItem->Expand(TRUE);
				nmi.hdr.code = TVN_ITEMEXPANDED;
				if (bNotify)
					NotifyParent(TVN_ITEMEXPANDED, &nmi);
				return TRUE;
			}
			if (wParam == TVE_TOGGLE)
			{
				if (!pItem->IsExpanded() && pItem->m_arChilds.GetCount() == 0)
					return FALSE;
				nmi.action = pItem->IsExpanded() ? TVE_COLLAPSE : TVE_EXPAND;
				
				if (bNotify && NotifyParent(TVN_ITEMEXPANDING, &nmi))
					return FALSE;
				pItem->Expand(!pItem->IsExpanded());
				nmi.hdr.code = TVN_ITEMEXPANDED;
				if (bNotify)
					NotifyParent(TVN_ITEMEXPANDED, &nmi);
				return TRUE;
			}

			return TRUE;
		}
	case TVM_SORTCHILDRENCB:
		{
			TVSORTCB* lpfnTVSORTCB = (TVSORTCB* )lParam;
			TBTreeItem* pItem = Find(lpfnTVSORTCB->hParent);
			if (!pItem)
				return FALSE;
			pItem->m_arChilds.Sort(lpfnTVSORTCB);
			//ricalcolo i rettangoli degli item, visto che sono stati spostati
			pItem->RecalcChildRect(TRUE);//ricorsivo, perché cambiando la posizione dei nodi cambia anche quella dei nodi figli
			return TRUE;
		}
	default:
		{
			//ASSERT(message<TV_FIRST);
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}
