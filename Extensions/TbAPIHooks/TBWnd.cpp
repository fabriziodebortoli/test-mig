#include "stdafx.h"

#include "TBWnd.h"
#include "TBListBox.h"
#include "TBComboBox.h"
#include "TBMessageQueue.h"
#include "TBDC.h"
#include "TBToolTip.h"
#include "TBDialog.h"
#include "TBEdit.h"
#include "TBTreeView.h"
#include "TBStatusBar.h"
#include "HookedFunction.h"
#include "TBGlobals.h"
#include "StreamReaderWriter.h"
#include "memfile.inl"
 
int CAPTION_HEIGHT = GetSystemMetrics(SM_CYCAPTION);
int BORDER_W = GetSystemMetrics(SM_CXBORDER);
int BORDER_H = GetSystemMetrics(SM_CYBORDER);
int THICK_BORDER_W = GetSystemMetrics(SM_CXSIZEFRAME);
int THICK_BORDER_H = GetSystemMetrics(SM_CYSIZEFRAME);
int MENU_H = GetSystemMetrics(SM_CYMENU);

//---------------------------------------------------------------------------------------------------------------
EXTERN_HOOK_LIB(BOOL, PostThreadMessageW, (DWORD dwThreadId, UINT message, WPARAM wParam, LPARAM lParam));


//-----------------------------------------------------------------------------
TBWnd* GetTBWnd(HWND hWnd)
{
	CSingleLock l (Get_CWindowMapSection(), TRUE);
	TBWnd* pWnd = NULL;
	Get_CWindowMap().Lookup(hWnd, pWnd);
	return pWnd;
}
//-----------------------------------------------------------------------------
CDeferItemList* GetDeferItemList(HDWP hdwp)
{
	CSingleLock l (Get_CDeferMapSection(), TRUE);
	CDeferItemList* pList = NULL;
	Get_CDeferMap().Lookup(hdwp, pList);
	return pList;
}


//-----------------------------------------------------------------------------
CWindowMap::~CWindowMap()
{
	POSITION pos =  GetStartPosition();
	HWND key;
	TBWnd* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		delete pVal;
	}
}
//-----------------------------------------------------------------------------
void CWindowMap::CleanUpThreadWnd(DWORD dwThreadId)
{
	CArray<HWND> toDestroy;
	POSITION pos =  GetStartPosition();
	HWND key;
	TBWnd* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		if (pVal->GetHWND() != HWND_TBMFC_SPECIAL && pVal->GetThreadID() == dwThreadId)
		{
			//do not destroy directly, otherwise the loop go corrupted
			toDestroy.Add(pVal->GetHWND());
		}
	}
	//delete the windows I found
	for (int i = 0; i < toDestroy.GetCount(); i++)
	{
		//check if the window still exists, it may have been destroyed 
		//if previously I destroyed a parent
		TBWnd* pWnd = NULL;
		if (Lookup(toDestroy[i], pWnd))
			pWnd->Destroy();
	}
}
//-----------------------------------------------------------------------------
void CWindowMap::SaveToStream(CMemFile* pFile)
{
	CArray<TBWnd*> ar;
	POSITION pos = GetStartPosition();
	HWND key;
	TBWnd* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		if (!pVal->GetParent())//only first level, the others are processed by their parent
			ar.Add(pVal);
	}
	Write<int>(pFile, ar.GetCount());
	for (int i = 0; i < ar.GetCount(); i++)
		ar[i]->SaveToStream(pFile);
}

//-----------------------------------------------------------------------------
TBSpecial::TBSpecial(DWORD dwThreadId) : TBWnd(HWND_TBMFC_SPECIAL, dwThreadId)
{
	m_cs.style |= WS_VISIBLE;
	m_cs.cx = MONITOR_WIDTH;
	m_cs.cy = MONITOR_HEIGHT;
}


//-----------------------------------------------------------------------------
TBSpecial::~TBSpecial()
{
	//quando la distruggo non deve avere figli
	ASSERT(m_Child.GetSize() == 0);
	ASSERT(m_Owned.GetSize() == 0);
	
	CSingleLock l (Get_CWindowMapSection(), TRUE);
	VERIFY(Get_CWindowMap().RemoveKey(m_hWnd));

	//deve essere l'ultima finestra
	ASSERT(Get_CWindowMap().GetSize() == 0);

	delete m_pMemFile;
}

//----------------------------------------------------------------------------
LRESULT TBSpecial::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case UM_REQUEST_INFO:
	{
		if (!m_pMemFile)
			m_pMemFile = new CMyMemFile;
		BYTE* lpBuff = (BYTE*)lParam;
		int nMaxCount = wParam;
		if (!lpBuff)
		{
			m_pMemFile->SeekToBegin();
			GetTBWnd(HWND_TBMFC_SPECIAL)->SaveToStream(m_pMemFile);
			return (LRESULT)m_pMemFile->GetLength();
		}

		memcpy_s(lpBuff, nMaxCount, m_pMemFile->GetBuffer(), (SIZE_T)m_pMemFile->GetLength());

		return (LRESULT)m_pMemFile->GetLength();
	}
	
	case UM_CHANGE_WINDOW_PROPERTIES:
	{
		BYTE* lpBuff = (BYTE*)lParam;
		int nMaxCount = wParam;
		CMyMemFile file;
		file.Attach(lpBuff, nMaxCount);
		ULONG nWritten = 0;
		CStreamReaderWriter reader;
		HWND hwnd;
		reader.Read<HWND>(&file, hwnd);
		TBWnd* pWnd = GetTBWnd(hwnd);
		if (pWnd)
			pWnd->ReadFromStream(&file);
		return 1L;
	}
	}
	return __super::DefWindowProc(message, wParam, lParam);
}
//-----------------------------------------------------------------------------
CDeferItemList::~CDeferItemList()
{
	for (int i = 0; i < GetCount(); i++)
	{
		delete GetAt(i);
	}
}
//-----------------------------------------------------------------------------
void CDeferItemList::Add(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, UINT uFlags)
{
	CDeferItem* pTargetItem = NULL;
	for (int i = 0; i < GetCount(); i++)
	{
		CDeferItem* pItem = GetAt(i);
		if (pItem->m_hWnd == hWnd)
		{
			pTargetItem = pItem;
			break;
		}
	}
	if (!pTargetItem)
	{
		__super::Add(new CDeferItem(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags));
	}
	else
	{
		//existing window: intelligent merge of settings
		pTargetItem->m_uFlags |= uFlags;//initial merge step: or with new settings (i'm not sure of this!)
		if ((uFlags & SWP_NOMOVE) != SWP_NOMOVE)
		{
			pTargetItem->m_x = x;
			pTargetItem->m_y = y;
			pTargetItem->m_uFlags &= ~SWP_NOMOVE;//force movement
		}
		if ((uFlags & SWP_NOSIZE) != SWP_NOSIZE)
		{
			pTargetItem->m_cx = cx;
			pTargetItem->m_cy = cy;
			pTargetItem->m_uFlags &= ~SWP_NOSIZE;//force sizing
		}
		if ((uFlags & SWP_NOZORDER) != SWP_NOZORDER)
		{
			pTargetItem->m_hWndInsertAfter = hWndInsertAfter;
			pTargetItem->m_uFlags &= ~SWP_NOZORDER;//force reordering
		}
	}
}
//-----------------------------------------------------------------------------
TBWnd* _TBCreateWindow(DWORD dwExStyle, LPCWSTR lpszClassName, LPCWSTR lpWindowName, DWORD dwStyle, int nId, int x, int y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam, WNDPROC wndProc)
{
	CString sClassName = lpszClassName;
	TBWnd* pWnd = NULL;
	HWND hwnd = (HWND)Get_New_CWindowMap();
	if (HIWORD(lpszClassName) == 0)
	{
		GlobalGetAtomName(LOWORD(lpszClassName), sClassName.GetBuffer(MAX_CLASS_NAME + 1), MAX_CLASS_NAME);
		sClassName.ReleaseBuffer();
		pWnd = new TBWnd(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(TOOLBARCLASSNAME) == 0)
	{
		pWnd = new TBToolbar(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(STATUSCLASSNAME) == 0)
	{
		pWnd = new TBStatusBar(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("Button")) == 0)
	{
		pWnd = new TBButton(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("Edit")) == 0)
	{
		pWnd = new TBEdit(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("Static")) == 0)
	{
		pWnd = new TBStatic(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("ListBox")) == 0)
	{
		pWnd = new TBListBox(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("ComboBox")) == 0)
	{
		pWnd = new TBComboBox(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("SysTabControl32")) == 0)
	{
		pWnd = new TBTabControl(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("SysTreeView32")) == 0)
	{
		pWnd = new TBTreeView(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("tooltips_class32")) == 0)
	{
		pWnd = new TBToolTip(hwnd, GetCurrentThreadId());
	}
	else if (sClassName.CompareNoCase(_T("#32770")) == 0)
	{
		pWnd = new TBDialog(hwnd, GetCurrentThreadId());
	}
	else
	{
		pWnd = new TBWnd(hwnd, GetCurrentThreadId());
	}
	//for windows forms I have to leave the original window proc, 
	//because it sets the window handle!
	if (sClassName.Find(_T("WindowsForms")) == 0)
	{
		WNDCLASSEX wndClass;
		if (GetClassInfoEx(hInstance, sClassName, &wndClass))
			pWnd->SetWindowLong(GWLP_WNDPROC, (LONG)wndClass.lpfnWndProc);

	}

	pWnd->SetWindowLong(GWL_STYLE, dwStyle);
	pWnd->SetWindowLong(GWL_EXSTYLE, dwExStyle);
	pWnd->SetWindowLong(GWL_HINSTANCE, (LONG)hInstance);
	pWnd->SetWindowLong(GWL_HWNDPARENT, (LONG)hWndParent);
	pWnd->SetWindowLong(GWL_ID, nId);
	pWnd->SetMenu(hMenu);
	pWnd->SetCreateParams(lpParam);
	pWnd->SetName(lpWindowName);
	pWnd->SetClass(sClassName);

	//PERASSO: semplifico la gestione delle coordinate, rispetto a quanto detto nella documentazione: di fatto non lo usiamo
	if (x == CW_USEDEFAULT || y == CW_USEDEFAULT)
	{
		x = y = 0;
	}
	if (nWidth == CW_USEDEFAULT)
	{
		nWidth = 800;
	}
	if (nHeight == CW_USEDEFAULT)
	{
		nHeight = 800;
	}

	pWnd->SetWidth(nWidth);
	pWnd->SetHeight(nHeight);
	pWnd->SetX (x);
	pWnd->SetY (y);

	pWnd->CallCreationHook();

	//se l'handle che mi arriva non è un parent della mia gerarchia (magari è un handle vero di finestra)
	//allora uso come parent la mia finestra speciale (una specie di mio desktop)
	TBWnd* pParentWnd = GetTBWnd(hWndParent);
	pWnd->SetParent(pParentWnd ? pParentWnd : GetTBWnd(HWND_TBMFC_SPECIAL));

	if (!pWnd->SendCreationMessages())
	{
		pWnd->Destroy();
		return NULL;
	}
	
	return pWnd;
}

//----------------------------------------------------------------------------
LRESULT CALLBACK DefaultWndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	TBWnd* pWnd = GetTBWnd(hwnd);
	return pWnd ? pWnd->DefWindowProc(message, wParam, lParam) : 0L;
}
//----------------------------------------------------------------------------
TBWnd::TBWnd(HWND hwnd, DWORD dwThreadId) : 
m_dwThreadId(dwThreadId), 
m_id(0), 
m_hWnd(hwnd), 
m_wndProc(NULL), 
m_hFont(NULL), 
m_pParent(NULL), 
m_nIndexInParentChild(-1),
m_nIndexInParentOwned(-1),
//m_hRgn(NULL), 
m_dwUserData(NULL),
m_nBorderWidth(0),
m_nBorderHeight(0),
m_nCaptionHeight(0),
m_hDCBitmap(NULL)
{
	m_wndProc = &DefaultWndProc;
	CSingleLock l (Get_CWindowMapSection(), TRUE);
	Get_CWindowMap().SetAt(m_hWnd, this);
	ZeroMemory(&m_arScrollRanges, sizeof(m_arScrollRanges));
	ZeroMemory(&m_cs, sizeof(CREATESTRUCT));
}
//----------------------------------------------------------------------------
TBWnd::~TBWnd()
{
	//dall'esterno qualcuno mi imposta il puntatore ad un booleano per sapere se sono morto (durante la Destroy per evitare nidificazioni di chiamate)
	for (int i = 0; i < m_arDestroyed.GetCount(); i++)
		*((bool*)m_arDestroyed[i]) = true;
	if (m_hDCBitmap)
		DeleteObject(m_hDCBitmap);
}

//----------------------------------------------------------------------------
void TBWnd::SaveToStream(CMemFile* pFile)
{
	WriteString(pFile, m_ClassName);
	Write<HWND>(pFile, m_hWnd);
	Write<DWORD>(pFile, m_id);
	WriteString(pFile, m_text);
	Write<int>(pFile, m_cs.x);
	Write<int>(pFile, m_cs.y);
	Write<int>(pFile, m_cs.cx);
	Write<int>(pFile, m_cs.cy);
	
	POINT p;
	GetTopLeftScreenRelative(p);
	Write<int>(pFile, p.x);
	Write<int>(pFile, p.y);
	
	Write<LONG>(pFile, m_cs.style);
	Write<DWORD>(pFile, m_cs.dwExStyle);
	Write<int>(pFile,  m_Child.GetCount());
	for (int i = 0; i < m_Child.GetCount(); i++)
		m_Child[i]->SaveToStream(pFile);
	Write<int>(pFile,  m_Owned.GetCount());
	for (int i = 0; i < m_Owned.GetCount(); i++)
		m_Owned[i]->SaveToStream(pFile);
}

//----------------------------------------------------------------------------
inline void TBWnd::SetHeight(int h)
{
	if (m_cs.cy != h)
	{
		m_cs.cy = h;
		if (m_hDCBitmap)
			DeleteObject(m_hDCBitmap);
	}
}
//----------------------------------------------------------------------------
inline void TBWnd::SetWidth(int w)		
{
	if (m_cs.cx != w)
	{
		m_cs.cx = w;
		if (m_hDCBitmap)
			DeleteObject(m_hDCBitmap);
	}
}
//----------------------------------------------------------------------------
HBITMAP TBWnd::GetDCBitmap()
{
	//creo il bitmap per questa finestra a beneficio dei DC creati per la stessa
	//per ora sembra non servire, se in futuro ci fossero malfunzionamenti legati ai DC di finestra, proviamo a scommentarla
	//if (!m_hDCBitmap)
	//	m_hDCBitmap = CreateBitmap(GetWidth(), GetHeight(), 1, 32, NULL);
	return m_hDCBitmap;
}
//----------------------------------------------------------------------------
void TBWnd::ReadFromStream(CMemFile* pFile)
{
	ReadString(pFile, m_ClassName);
	Read<HWND>(pFile, m_hWnd);
	Read<DWORD>(pFile, m_id);
	ReadString(pFile, m_text);
	Read<int>(pFile, m_cs.x);
	Read<int>(pFile, m_cs.y);
	Read<int>(pFile, m_cs.cx);
	Read<int>(pFile, m_cs.cy);
	Read<LONG>(pFile, m_cs.style);
	Read<DWORD>(pFile, m_cs.dwExStyle);
	/*Read<int>(pFile,  m_Child.GetCount());
	for (int i = 0; i < m_Child.GetCount(); i++)
		m_Child[i]->SaveToStream(pFile);
	Write<int>(pFile,  m_Owned.GetCount());
	for (int i = 0; i < m_Owned.GetCount(); i++)
		m_Owned[i]->SaveToStream(pFile);*/
}


//----------------------------------------------------------------------------
BOOL TBWnd::GetScrollRange(int nBar, LPINT lpMinPos, LPINT lpMaxPos)
{
	if (nBar < 0 || nBar > 2)
		return FALSE;
	*lpMinPos = m_arScrollRanges[nBar].x;
	*lpMaxPos = m_arScrollRanges[nBar].y;
	return TRUE;
}
//----------------------------------------------------------------------------
BOOL TBWnd::SetScrollRange(int nBar, int nMinPos, int nMaxPos)
{
	if (nBar < 0 || nBar > 2)
		return FALSE;
	m_arScrollRanges[nBar].x = nMinPos;
	m_arScrollRanges[nBar].y = nMaxPos;
	return TRUE;
}
//----------------------------------------------------------------------------
void TBWnd::CallCreationHook()
{
	CBT_CREATEWNDW createWnd;
	createWnd.hwndInsertAfter = NULL;
	createWnd.lpcs = &m_cs;
	GetMessageQueue()->CallHook(m_hWnd, &createWnd);
}
//----------------------------------------------------------------------------
BOOL TBWnd::SendCreationMessages()
{
	if (FALSE == SendMessage(WM_NCCREATE, NULL, (LPARAM)&m_cs))
		return FALSE;

	SendMessage(WM_NCCALCSIZE, NULL, (LPARAM)&CRect(CPoint(m_cs.x, m_cs.y), CSize(m_cs.cx, m_cs.cy)));

	if (-1 == SendMessage(WM_CREATE, NULL, (LPARAM)&m_cs))
	{
		return FALSE;
	}

	return TRUE;
}
//----------------------------------------------------------------------------
LRESULT TBWnd::Dispatch(UINT message, WPARAM wParam, LPARAM lParam)
{
	return m_wndProc(m_hWnd, message, wParam, lParam);
}
//----------------------------------------------------------------------------
void TBWnd::EnableWindow(BOOL bEnable)
{
	if (IsWindowEnabled() == bEnable)
		return;
	if (!bEnable)
		SendMessage(WM_CANCELMODE, NULL, NULL);
	m_cs.style = bEnable ? (m_cs.style & ~WS_DISABLED) : (m_cs.style | WS_DISABLED); 
	SendMessage(WM_ENABLE, bEnable, NULL);
	
}


//----------------------------------------------------------------------------
void TBWnd::SetRectFromDialogUnits(CRect& r)
{ 
	MapDialogRect(r);
	SetX(r.left);
	SetY(r.top);
	SetWidth(r.Width() + 2*m_nBorderWidth);
	SetHeight(r.Height() + 2*m_nBorderHeight + m_nCaptionHeight + (GetMenu() ? MENU_H : 0));

}


//----------------------------------------------------------------------------
BOOL TBWnd::ShowWindow(int nCmdShow)
{
	switch (nCmdShow)
	{
	case SW_HIDE:
		m_cs.style &= ~WS_VISIBLE;
		return TRUE;
	case SW_SHOWNORMAL:
		m_cs.style |= WS_VISIBLE;
		m_cs.style &= ~WS_MAXIMIZE;
		m_cs.style &= ~WS_MINIMIZE; 
		return TRUE;
	case SW_SHOWMINIMIZED:
		m_cs.style |= WS_MINIMIZE; 
		m_cs.style &= ~WS_MAXIMIZE; 
		return TRUE;
	case SW_SHOWMAXIMIZED:
		m_cs.style |= WS_MAXIMIZE;
		m_cs.style &= ~WS_MINIMIZE;  
		return TRUE;
	case SW_SHOWNOACTIVATE:
		m_cs.style |= WS_VISIBLE;
		return TRUE;
	case SW_SHOW:
		m_cs.style |= WS_VISIBLE; 
		return TRUE;
	case SW_MINIMIZE:
		m_cs.style |= (WS_VISIBLE|WS_MINIMIZE);
		m_cs.style &= ~WS_MAXIMIZE; 
		return TRUE;
	case SW_SHOWMINNOACTIVE:
		m_cs.style |= WS_VISIBLE|WS_MINIMIZE; 
		m_cs.style &= ~WS_MAXIMIZE; 
		return TRUE;
	case SW_SHOWNA:
		m_cs.style |= WS_VISIBLE; 
		return TRUE;
	case SW_RESTORE:
		m_cs.style |= WS_VISIBLE;
		m_cs.style &= ~WS_MAXIMIZE;
		m_cs.style &= ~WS_MINIMIZE; 
		return TRUE;
	case SW_SHOWDEFAULT:
		m_cs.style |= WS_VISIBLE;
		m_cs.style &= ~WS_MAXIMIZE;
		m_cs.style &= ~WS_MINIMIZE; 
		return TRUE;
	case SW_FORCEMINIMIZE:
		m_cs.style |= (WS_VISIBLE | WS_MINIMIZE);
		m_cs.style &= ~WS_MAXIMIZE; 
		return TRUE;
	default: 
		ASSERT(FALSE);
		return FALSE;
	}
}
//----------------------------------------------------------------------------
void TBWnd::Destroy()
{
	CMessageQueue* pQueue = GetMessageQueue();
	//if I have focus, remove focus!
	if (this == pQueue->GetFocus())
		pQueue->SetFocus(NULL, FALSE);
	HWND h = m_hWnd;
	MSG msg;

	//flush della coda dei messaggi
	while (pQueue->MyPeekMessage(&msg, h, 0, 0, PM_REMOVE))
		pQueue->DispatchMessage(&msg, this);
	
	bool bDestroyed = false;
	m_arDestroyed.Add(&bDestroyed);
	//mando il messaggio di WM_DESTROY
	pQueue->SendMessage(this, WM_DESTROY, NULL, NULL);
	
	//controllo che non sia stato distrutto a seguito di quel messaggio
	if (bDestroyed)
		return;
	
	//distruggo le figlie
	while (m_Child.GetCount())
	{
		m_Child.GetAt(m_Child.GetUpperBound())->Destroy();//removes itself from m_Children
		//controllo che non sia stato distrutto a seguito della distruzione della figlia
		//(succede: uno hook sulla destroy di CSkinScrollWnd mi causava un crash
		if (bDestroyed)
			return;
	}
	
	//distruggo le owned
	while (m_Owned.GetCount())
	{
		m_Owned.GetAt(m_Owned.GetUpperBound())->Destroy();//removes itself from m_Children
		//controllo di essere ancora in vita
		if (bDestroyed)
			return;
	}

	pQueue->SendMessage(this, WM_NCDESTROY, NULL, NULL);
}

//----------------------------------------------------------------------------
void TBWnd::SetParent(TBWnd* pParent)
{
	if (m_pParent == pParent)
		return;
	BOOL bChild = IsChild();
	if (m_pParent)
	{
		BOOL bFound = FALSE;
		
		CArray<TBWnd*>* ar = m_pParent->GetChildOrOwned(bChild);
		for (int i = 0; i < ar->GetCount(); i++)
		{
			//se l'ho già trovato, allora devo aggiornare l'indice della finestra perché è successiva
			if (bFound)
			{
				TBWnd* pWnd = ar->GetAt(i);
				pWnd->GetChildOrOwnedIndex(bChild) = i;
			}
			else
			{
				ASSERT(ar->GetAt(i)->GetChildOrOwnedIndex(bChild) == i);
				if (ar->GetAt(i) == this)
				{
					ar->RemoveAt(i);
					bFound = TRUE;
					i--;//sposto l'indice indietro perché ho rimosso un elemento
				}
			}
		}
	}
	m_pParent = pParent;
	if (m_pParent)
	{
		GetChildOrOwnedIndex(bChild) = m_pParent->GetChildOrOwned(bChild)->Add(this);
	}
}

//----------------------------------------------------------------------------
TBWnd* TBWnd::GetAncestor(UINT gaFlags)
{
	switch (gaFlags)
	{
	case GA_PARENT:
		return GetParent();
	case GA_ROOT:
	{
		TBWnd* pRoot = this;
		do
		{
			if (pRoot->IsPopup())
				return pRoot;
			pRoot = pRoot->GetParent();
		} while (pRoot);
	}
	case GA_ROOTOWNER:
	{
		TBWnd* pParent, *pCurrent = this;
		while (pParent = pCurrent->GetParent())
		{
			if (pParent->GetHWND() == HWND_TBMFC_SPECIAL)
				return pCurrent;
			pCurrent = pParent;
		}
	}
	default:
		return NULL;
	}
}

//----------------------------------------------------------------------------
LRESULT TBWnd::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_CREATE:
		return 1L;
	case WM_NCCREATE:
		return TRUE;
	case WM_SETFONT:
		m_hFont = (HFONT)wParam;
		return 1L;

	case WM_GETFONT:
		return (LRESULT)m_hFont;
	case WM_RECALCPARENT:
	case WM_ACTIVATE:
	case WM_NCACTIVATE:
	case WM_COMMAND:
	case WM_SETFOCUS:
	case WM_KILLFOCUS:
	case WM_DESTROY:
	case WM_INITIALUPDATE:
	case WM_IDLEUPDATECMDUI:
	case EM_SETMODIFY:
	case WM_GETDLGCODE:
		return 0L;//TODO
	//these messages are MFC specific
	case WM_SETMESSAGESTRING:
	case WM_SIZEPARENT:
		return 0L;
	case WM_WINDOWPOSCHANGED:
		{
		WINDOWPOS* lpwp = (WINDOWPOS*)lParam;
		if ((lpwp->flags & SWP_NOSIZE) != SWP_NOSIZE)
			SendMessage(WM_SIZE, SIZE_RESTORED, MAKELPARAM(lpwp->cx, lpwp->cy));
		if ((lpwp->flags & SWP_NOMOVE) != SWP_NOMOVE)
			SendMessage(WM_MOVE, NULL, MAKELPARAM(lpwp->x, lpwp->y));
		return 1L;
		}
	case WM_CLOSE:
		{
			Destroy();
			return 0L;
		}
	case WM_NCDESTROY:
		{
			SetParent(NULL);
			CSingleLock l (Get_CWindowMapSection(), TRUE);
			VERIFY(Get_CWindowMap().RemoveKey(m_hWnd));
			delete this;
			return 1L;
		}
	case WM_SETTEXT:
		{
		m_text = (LPCTSTR)lParam;
		return TRUE;
		}
	case WM_GETTEXT:
		{
			LPWSTR lpString = LPWSTR(lParam);
			int nMaxCount = wParam;
			_tcscpy_s(lpString, nMaxCount, m_text);
			return min(nMaxCount, m_text.GetLength());
		}
	case WM_GETTEXTLENGTH:
		{
			return m_text.GetLength();
		}

	case WM_SIZE:
	case WM_MOVE:
	case WM_WINDOWPOSCHANGING:
	case WM_SETREDRAW:
	case WM_NCPAINT:
	case WM_ENABLE:
	case WM_ENTERIDLE:
	case WM_KICKIDLE:
	case WM_QUERYCENTERWND:
	case WM_FLOATSTATUS:
	case WM_SETICON:
	case WM_ACTIVATETOPLEVEL:
	case WM_NCCALCSIZE:
	case WM_NULL:
		return 1L;
	case WM_KEYDOWN:
	case WM_KEYUP:
	case WM_CHAR:
	case WM_CONTEXTMENU:
		return 1L;

	case WM_CANCELMODE:
	{
		OnCancelMode();
		return 1L;
	}
	case WM_TIMER:
	{
		UINT_PTR nTimerID = (UINT_PTR)wParam;
		TIMERPROC lpTimerFunc = (TIMERPROC)lParam;
		if (lpTimerFunc)
			lpTimerFunc(m_hWnd, WM_TIMER, nTimerID, GetTickCount());
		return 0L;
	}
	case WM_GETICON:
		return NULL;
	case WM_INITDIALOG:
	{
		CDialog* pDlg = DYNAMIC_DOWNCAST(CDialog, CWnd::FromHandlePermanent(m_hWnd));
		if (pDlg != NULL)
			return pDlg->OnInitDialog();
		else
			return 1;
	}
	case WM_NOTIFY:
		return 0;
	case EM_GETMODIFY:
		return FALSE;
	case WM_NEXTDLGCTL:
		{
			if (lParam)
			{
				SetFocus((HWND)wParam);
			}
			else
			{
				TBWnd* pWnd = this;
				UINT nCmd = wParam ? GW_HWNDPREV :GW_HWNDNEXT;
				do
				{
					if ((pWnd->GetStyle() & WS_TABSTOP) == WS_TABSTOP)
						break;
					pWnd = pWnd->GetWindow(nCmd);
				} while (pWnd);
				if (pWnd)
					SetFocus(pWnd->GetHWND());
			}
			return 0;
		}

	default:
		if (message >= WM_USER/*0xC000 è il minimo per la RegisterWindowMessage*/)
			return 0L;

		// ASSERT(message >= WM_USER);
		return 0L;
	}
}
//----------------------------------------------------------------------------
void TBWnd::OnCancelMode()
{
	m_pContextMenu = NULL;//chiude l'eventuale menu di contesto
}

//----------------------------------------------------------------------------
BOOL TBWnd::MapDialogRect(LPRECT lpRect)
{

	lpRect->left   = MulDiv(lpRect->left,   BASE_UNIT_X, 4);
	lpRect->right  = MulDiv(lpRect->right,  BASE_UNIT_X, 4);
	lpRect->top    = MulDiv(lpRect->top,    BASE_UNIT_Y, 8);
	lpRect->bottom = MulDiv(lpRect->bottom, BASE_UNIT_Y, 8);

	return TRUE;
}
//----------------------------------------------------------------------------
TBWnd* TBWnd::GetDlgItem(int nIDDlgItem)
{
	for (int i = 0; i < m_Child.GetCount(); i++)
	{
		TBWnd* pItem = m_Child.GetAt(i);
		if (pItem->m_id == nIDDlgItem)
			return pItem;
	}
	return NULL;
}


//----------------------------------------------------------------------------
BOOL TBWnd::IsChild(HWND hwndChild)
{
	for (int i = 0; i < m_Child.GetCount(); i++)
	{
		TBWnd* pItem = m_Child.GetAt(i);
		if (pItem->m_hWnd == hwndChild || pItem->IsChild(hwndChild))
			return TRUE;
	}
	return FALSE;
}
//----------------------------------------------------------------------------
void TBWnd::OffsetToClientArea(POINT& p)
{
	p.y += m_nCaptionHeight;
	if (GetMenu()) 
		p.y += MENU_H;
	p.x += m_nBorderWidth;
	p.y += m_nBorderHeight;
}
//----------------------------------------------------------------------------
CMessageQueue* TBWnd::GetMessageQueue()
{
	return ::GetMessageQueue(m_dwThreadId);
}
//----------------------------------------------------------------------------
LRESULT TBWnd::NotifyParent(UINT code, void* lParam /*= NULL*/)
{
	if (!m_pParent)
		return FALSE;
	if (lParam)
	{
		return m_pParent->SendMessage(WM_NOTIFY, m_id, (LPARAM)lParam);
	}
	NMHDR nmh;
	nmh.code = code;    // Message type defined by control.
	nmh.idFrom = m_id;
	nmh.hwndFrom = GetHWND();
	return m_pParent->SendMessage(WM_NOTIFY, nmh.idFrom, (LPARAM)&nmh);
}
//----------------------------------------------------------------------------
LRESULT TBWnd::SendMessage(UINT message, WPARAM wParam, LPARAM lParam) 
{
	return GetMessageQueue()->SendMessage(this, message, wParam, lParam);
}
//----------------------------------------------------------------------------
void TBWnd::GetTopLeftScreenRelative(POINT& p)
{
	if (IsChild())
	{
		CPoint pp;
		TBWnd* pParent = GetParent();
		if (pParent)
		{
			pParent->GetTopLeftScreenRelative(pp);
			pParent->OffsetToClientArea(pp);
		}
		p.x = pp.x + m_cs.x; 
		p.y = pp.y + m_cs.y; 

	}
	else
	{
		p.x = m_cs.x; 
		p.y = m_cs.y; 
	}
}
//----------------------------------------------------------------------------
void TBWnd::GetWindowRect(__out LPRECT lpRect)
{
	POINT p;
	GetTopLeftScreenRelative(p);
	lpRect->left = p.x; 
	lpRect->top = p.y; 
	lpRect->right = lpRect->left + m_cs.cx; 
	lpRect->bottom = lpRect->top + m_cs.cy;
}
//----------------------------------------------------------------------------
void TBWnd::GetClientRect(__out LPRECT lpRect)
{
	lpRect->left = 0; 
	lpRect->top = 0; 
	lpRect->right = /*lpRect->left + */m_cs.cx; 

	lpRect->bottom = /*lpRect->top + */m_cs.cy;
	lpRect->bottom -= m_nCaptionHeight;
	if (GetMenu()) 
		lpRect->bottom -= MENU_H;
	lpRect->bottom -= 2 * m_nBorderHeight;
	lpRect->right -= 2* m_nBorderWidth;
}
//----------------------------------------------------------------------------
LONG TBWnd::SetWindowLong(int nIndex, LONG dwNewLong)
{
	LONG old = GetWindowLong(nIndex);
	switch(nIndex)
	{
	case GWL_WNDPROC: m_wndProc = (WNDPROC)dwNewLong;
	case GWL_HINSTANCE: m_cs.hInstance = (HINSTANCE)dwNewLong;break;
	case GWL_HWNDPARENT: m_cs.hwndParent = (HWND)dwNewLong;break;
	case GWL_STYLE: 
		m_cs.style = dwNewLong;
		m_nCaptionHeight = 0; 
		if ((dwNewLong & WS_CAPTION) == WS_CAPTION) 
		{
			m_nCaptionHeight = CAPTION_HEIGHT;
			m_nBorderWidth =  2*BORDER_W;
			m_nBorderHeight = 2*BORDER_H;
		}
		else if ((dwNewLong & WS_BORDER) == WS_BORDER) 
		{
			m_nBorderWidth =  BORDER_W;
			m_nBorderHeight = BORDER_H;
		}
		else if ((dwNewLong & WS_DLGFRAME) == WS_DLGFRAME) 
		{
			m_nBorderWidth =  2*BORDER_W;
			m_nBorderHeight = 2*BORDER_H;
		}
		else if ((dwNewLong & WS_THICKFRAME) == WS_THICKFRAME)
		{
			m_nBorderWidth = THICK_BORDER_W;
			m_nBorderHeight = THICK_BORDER_H;
		}
		else
		{
			m_nBorderWidth = 0;
			m_nBorderHeight = 0;
		}
		break;
	case GWL_EXSTYLE: m_cs.dwExStyle = dwNewLong;break;
	case GWL_USERDATA : m_dwUserData = dwNewLong;break;
	case GWL_ID : m_id = dwNewLong;break;
	default:
		break;
	}
	return old;
}
//----------------------------------------------------------------------------
LONG TBWnd::GetWindowLong(int nIndex)
{
	switch(nIndex)
	{
	case GWL_WNDPROC: return (LONG)m_wndProc;
	case GWL_HINSTANCE: return (LONG) m_cs.hInstance;
	case GWL_HWNDPARENT: return (LONG)m_cs.hwndParent;
	case GWL_STYLE: return m_cs.style;
	case GWL_EXSTYLE: return m_cs.dwExStyle;
	case GWL_USERDATA : return m_dwUserData;
	case GWL_ID : return m_id;
	default:
		return 0;
	}
}
//----------------------------------------------------------------------------
TBWnd* TBWnd::GetTopWindow()
{
	if (m_Child.GetCount() == 0) return NULL;
	return m_Child.GetAt(0);
}

//----------------------------------------------------------------------------
BOOL TBWnd::IsTopLevelWindow()
{
	return GetAncestor(GA_ROOT) == this;
}
//----------------------------------------------------------------------------
void TBWnd::ScreenToClient(LPPOINT lpPoint)
{
	POINT p;
	GetTopLeftScreenRelative(p);
	lpPoint->x = lpPoint->x - p.x;
	lpPoint->y = lpPoint->y - p.y;
}
//----------------------------------------------------------------------------
void TBWnd::ClientToScreen(LPPOINT lpPoint)
{
	POINT p;
	GetTopLeftScreenRelative(p);
	lpPoint->x = lpPoint->x + p.x;
	lpPoint->y = lpPoint->y + p.y;
}
//----------------------------------------------------------------------------
void TBWnd::MoveWindow(int X, int Y, int nWidth, int nHeight, BOOL bRepaint)
{
	SetWindowPos(NULL, X, Y, nWidth, nHeight, SWP_NOZORDER|SWP_NOACTIVATE);
}

//----------------------------------------------------------------------------
void TBWnd::SetWindowPos(HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags)
{
	WINDOWPOS wp;
	wp.cx = cx;
	wp.cy = cy;
	wp.x = X;
	wp.y = Y;
	wp.hwnd = m_hWnd;
	wp.flags = uFlags;
	wp.hwndInsertAfter = hWndInsertAfter;

	if ((wp.flags & SWP_SHOWWINDOW) == SWP_SHOWWINDOW)
	{
		ShowWindow(SW_SHOW);
	}
	if ((wp.flags & SWP_HIDEWINDOW) == SWP_HIDEWINDOW)
	{
		ShowWindow(SW_HIDE);
	}

	if ((wp.flags & SWP_NOACTIVATE) != SWP_NOACTIVATE)
	{
		GetMessageQueue()->SetActiveWindow(this, FALSE);//FALSE: do not change z order to avoid recursion
	}

	BOOL bChanged = FALSE;
	//detect if position needs to change
	if (
		((wp.flags & SWP_NOMOVE) != SWP_NOMOVE)
		&& 
		(m_cs.x != wp.x || m_cs.y != wp.y)
		)
	{
		bChanged = TRUE;
	}
	//detect if size needs to change
	else if (
		((wp.flags & SWP_NOSIZE) != SWP_NOSIZE)
		&&
		(m_cs.cx != wp.cx || m_cs.cy != wp.cy)
		)
	{
		bChanged = TRUE;
	}
	BOOL bChild = IsChild();
	CArray<TBWnd*>* ar = NULL;
	//detect id z-order needs to change
	int thisIdx = -1; //current index of the window to move
	int otherIdx = -1;//current index of the window to move after
	TBWnd* pParent = NULL;
	if ((wp.flags & SWP_NOZORDER) != SWP_NOZORDER)
	{
		pParent = GetParent();
		if (pParent)
		{
			ar = pParent->GetChildOrOwned(bChild);
			if (wp.hwndInsertAfter == HWND_TOP)
			{
				otherIdx = 0;
			}
			else if (wp.hwndInsertAfter == HWND_BOTTOM)
			{
				otherIdx = ar->GetUpperBound();
			}
			for (int i = 0; i < ar->GetCount(); i++)
			{
				TBWnd* pWnd = ar->GetAt(i);
				if (pWnd == this)
				{
					thisIdx = i;
				}
				else if (pWnd->GetHWND() == wp.hwndInsertAfter)
				{
					otherIdx = i;
				}
				if (thisIdx != -1 && otherIdx != -1)
				{
					bChanged = TRUE;
					break;
				}
			}	
			
		}
	}

	//if no change is needed, no action is done
	if (!bChanged)
		return;

	SendMessage(WM_WINDOWPOSCHANGING, NULL, (LPARAM)&wp);
	
	if ((wp.flags & SWP_NOMOVE) != SWP_NOMOVE)
	{
		m_cs.x = wp.x;
		m_cs.y = wp.y;
	}

	if ((wp.flags & SWP_NOSIZE) != SWP_NOSIZE)
	{
		m_cs.cx = wp.cx;
		m_cs.cy = wp.cy;
	}
	if (pParent && thisIdx != -1 && otherIdx != -1 && thisIdx != otherIdx)
	{
		ar->RemoveAt(thisIdx);
		ar->InsertAt(otherIdx > thisIdx ? otherIdx : otherIdx + 1, this);
		//aggiorno l'indice di tutte quelle successive allo spostamento
		for (int i = min(thisIdx, otherIdx); i < ar->GetSize(); i++)
			ar->GetAt(i)->GetChildOrOwnedIndex(bChild) = i;
	}
	
	if (bChanged)
	{
		SendMessage(WM_WINDOWPOSCHANGED, NULL, (LPARAM)&wp);
	}
}
//----------------------------------------------------------------------------
TBWnd* TBWnd::GetWindow(UINT uCmd)
{
	if (uCmd == GW_CHILD)
	{
		if (m_Child.GetCount() == 0)
			return NULL;
		return m_Child.GetAt(0);
	}
	if (uCmd == GW_ENABLEDPOPUP)
	{
		//not yet implemented
		ASSERT(FALSE);
		return NULL;
	}
	if (uCmd == GW_OWNER)
	{
		return GetTBWnd(m_cs.hwndParent);
	}

	if (!m_pParent)
		return NULL;
	int idx = m_nIndexInParentChild;
	if (idx == -1)
		return NULL;

	if (uCmd == GW_HWNDFIRST)
	{
		if (m_pParent->m_Child.GetCount() == 0)
			return NULL;
		return m_pParent->m_Child.GetAt(0);
	}

	if (uCmd == GW_HWNDLAST)
	{
		if (m_pParent->m_Child.GetCount() == 0)
			return NULL;
		return m_pParent->m_Child.GetAt(m_pParent->m_Child.GetUpperBound());
	}
	if (uCmd == GW_HWNDPREV)
	{
		if (idx == 0)
			return NULL;
		return m_pParent->m_Child.GetAt(idx - 1);
	}

	if (uCmd == GW_HWNDNEXT)
	{
		if (idx == m_pParent->m_Child.GetUpperBound())
			return NULL;
		return m_pParent->m_Child.GetAt(idx + 1);
	}
	ASSERT(FALSE);
	return NULL;
}
//----------------------------------------------------------------------------
TBToolbar::~TBToolbar()
{
	for (int i = 0; i < m_arButtons.GetCount(); i++)
		delete m_arButtons[i];
}
//----------------------------------------------------------------------------
LRESULT TBToolbar::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case TB_SETIMAGELIST:
		{
			m_hImageList = (HIMAGELIST) lParam;
			return 1L;
		}
	case TB_GETIMAGELIST:
		{
			return (LRESULT)m_hImageList;
		}
	case TB_SETDISABLEDIMAGELIST:
		{
			m_hDisabledImageList = (HIMAGELIST) lParam;
			return 1L;
		}
	case TB_GETDISABLEDIMAGELIST:
		{
			return (LRESULT)m_hDisabledImageList;
		}
	case TB_SETHOTIMAGELIST:
		{
			m_hHotImageList = (HIMAGELIST) lParam;
			return 1L;
		}
	case TB_GETHOTIMAGELIST:
		{
			return (LRESULT)m_hHotImageList;
		}
	case TB_SETBITMAPSIZE:
		{
			m_BitmapSize.cx = LOWORD(lParam);
			m_BitmapSize.cy = HIWORD(lParam);
			return 1L;
		}
	case TB_SETBUTTONSIZE:
		{
			m_ButtonSize.cx = LOWORD(lParam);
			m_ButtonSize.cy = HIWORD(lParam);
			m_cs.cx = m_ButtonSize.cx * m_arButtons.GetCount();
			m_cs.cy = m_ButtonSize.cy;
			return 1L;
		}
	case TB_GETBUTTONSIZE:
		{
			return MAKELRESULT(m_ButtonSize.cx, m_ButtonSize.cy);
		}

	case TB_SETEXTENDEDSTYLE:
		{
			m_cs.dwExStyle = lParam;
			return 1L;
		}
	case TB_GETEXTENDEDSTYLE:
		{
			return m_cs.dwExStyle;
		}
	case TB_COMMANDTOINDEX:
		{
			UINT nIDFind = wParam;
			for (int i = 0; i < m_arButtons.GetCount(); i++)
			{
				TBBUTTON* pBtn = m_arButtons[i];
				if (pBtn->idCommand == nIDFind)
					return i;
			}
			return -1;
		}
	case TB_ADDBUTTONS:
		{
			int nSize = wParam;
			LPTBBUTTON lpButtons = (TBBUTTON*)lParam;
			for (int i = 0; i<nSize; i++)
			{
				TBBUTTON* pBtn = new TBBUTTON;
				memcpy(pBtn, &lpButtons[i], sizeof(TBBUTTON));
				m_arButtons.Add(pBtn); 
			}
			return 1L;
		}
	case TB_GETBUTTON:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arButtons.GetCount())
				return 0L;
			TBBUTTON* pButton = (TBBUTTON*)lParam;
			TBBUTTON* pExisting = m_arButtons[nIndex];
			memcpy(pButton, pExisting, sizeof(TBBUTTON));
			return 1L;
		}
	case TB_DELETEBUTTON:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arButtons.GetCount())
				return 0L;
			TBBUTTON* pExisting = m_arButtons[nIndex];
			delete pExisting;
			m_arButtons.RemoveAt(nIndex);
			return TRUE;
		}

	case TB_INSERTBUTTON:
		{
			int nIndex = wParam;
			TBBUTTON* pButton = (TBBUTTON*)lParam;
			TBBUTTON* pBtn = new TBBUTTON;
			memcpy(pBtn, pButton, sizeof(TBBUTTON));
			m_arButtons.InsertAt(nIndex, pBtn); 
			return 1L;

		}
	case TB_GETITEMRECT:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arButtons.GetCount())
				return 0L;
			LPRECT pRect = (LPRECT)lParam;
			TBBUTTON* pExisting = m_arButtons[nIndex];
			pRect->left = nIndex*m_ButtonSize.cx;
			pRect->top = 0;
			pRect->right = pRect->left + m_ButtonSize.cx;
			pRect->bottom = m_ButtonSize.cy;
			return 1L;
		}
	case TB_BUTTONCOUNT:
		{
			return m_arButtons.GetCount();
		}
	case TB_BUTTONSTRUCTSIZE:
		{
			return 0L;//TODO
		}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}


//----------------------------------------------------------------------------
LRESULT TBStatic::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch(message)
	{
	case STM_GETIMAGE:
		{
			if (wParam == IMAGE_BITMAP)
				return (LRESULT)m_hBitmap;
			if (wParam == IMAGE_CURSOR)
				return (LRESULT)m_hCursor;
			return 0L;

		}
	case STM_SETIMAGE:
		{
			if (wParam == IMAGE_BITMAP)
			{
				m_hBitmap = (HBITMAP) lParam;
				return 1L;
			}

			if (wParam == IMAGE_CURSOR)
			{
				m_hCursor = (HCURSOR) lParam;
				return 1L;
			}
			return 0L;
		}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}


//----------------------------------------------------------------------------
LRESULT TBButton::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch(message)
	{
	case BM_GETIMAGE:
		{
			if (wParam == IMAGE_BITMAP)
				return (LRESULT)m_hBitmap;
			if (wParam == IMAGE_CURSOR)
				return (LRESULT)m_hCursor;
			return 0L;

		}
	case BM_SETIMAGE:
		{
			if (wParam == IMAGE_BITMAP)
			{
				m_hBitmap = (HBITMAP) lParam;
				return 1L;
			}

			if (wParam == IMAGE_CURSOR)
			{
				m_hCursor = (HCURSOR) lParam;
				return 1L;
			}
			return 0L;
		}
	case BM_GETSTATE:
		{
			return m_bState;
		}
	case BM_SETSTATE:
		{
			m_bState = wParam;
			return 1L;
		}
	case BM_GETCHECK:
		{
			return m_nCheck;
		}
	case BM_SETCHECK:
		{
			m_nCheck = wParam;
			return 1L;
		}
	case BM_CLICK:
		{
			return GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM((long)m_id, BN_CLICKED), (LPARAM)m_hWnd);
		}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}

//----------------------------------------------------------------------------
LRESULT TBTabControl::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch(message)
	{
		case TCM_SETIMAGELIST:
		{
			m_hImageList = (HIMAGELIST) lParam;
			return 1L;
		}
	case TCM_GETIMAGELIST:
		{
			return (LRESULT)m_hImageList;
		}
	case TCM_GETITEMCOUNT:
		{
			return m_arItems.GetCount();
		}
	case TCM_GETITEM:
		{
			int nItem = wParam;
			if (nItem < 0 || nItem >= m_arItems.GetCount())
				return 0L;
			TCITEM* pTabCtrlItem = (TCITEM*)lParam;
			TCITEM* pExisting = m_arItems[nItem];
			memcpy(pTabCtrlItem, pExisting, sizeof(TCITEM));
			return 1L;
		}
	case TCM_SETITEM:
		{
			int nItem = wParam;
			if (nItem < 0 || nItem >= m_arItems.GetCount())
				return 0L;
			TCITEM* pTabCtrlItem = (TCITEM*)lParam;
			TCITEM* pExisting = m_arItems[nItem];
			memcpy(pExisting, pTabCtrlItem, sizeof(TCITEM));
			return 1L;
		}
	case TCM_INSERTITEM:
		{
			int nItem = wParam;
			TCITEM* pTabCtrlItem = (TCITEM*)lParam;
			TCITEM* pNew = new TCITEM;
			memcpy(pNew, pTabCtrlItem, sizeof(TCITEM));
			m_arItems.InsertAt(nItem, pNew);
			return 1L;
		}
	case TCM_DELETEITEM:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arItems.GetCount())
				return 0L;
			TCITEM* pExisting = m_arItems[nIndex];
			delete pExisting;
			m_arItems.RemoveAt(nIndex);
			return TRUE;
		}
	case TCM_DELETEALLITEMS:
		{
			for (int i = 0; i < m_arItems.GetCount(); i++)
				delete m_arItems[i];
			m_arItems.RemoveAll();
			m_nCurSel = -1;
			return TRUE;
		}
	case TCM_GETITEMRECT:
	{
#define TABW 100
#define TABH 20
		int nIndex = wParam;
		if (nIndex < 0 || nIndex >= m_arItems.GetCount())
			return 0L;
		LPRECT pRect = (LPRECT)lParam;
		TCITEM* pExisting = m_arItems[nIndex];
		pRect->left = nIndex*TABW;
		pRect->top = 0;
		pRect->right = pRect->left + TABW;
		pRect->bottom = TABH;
		return 1L;
	}
	case TCM_GETCURSEL:
		{
			return m_nCurSel;
		}
	case TCM_SETCURSEL:
	{
		int nIndex = wParam;
		if (nIndex < 0 || nIndex >= m_arItems.GetCount())
			return 0L;
		m_nCurSel = nIndex;
		return 1L;
	}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}
//----------------------------------------------------------------------------
TBTabControl::~TBTabControl()
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
		delete m_arItems[i];
}



